using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MetaCompta.Application.Services;

// Payroll Engine - Moroccan Payroll with formula evaluation

public class RubriqueResult
{
    public int RubriqueId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int OrdreAffichage { get; set; }
    public bool AfficherSurBulletin { get; set; }
    public decimal Base { get; set; }
    public decimal Taux { get; set; }
    public decimal Montant { get; set; }
    public decimal Gain { get; set; }
    public decimal Retenue { get; set; }
}

public class PayrollContext
{
    public Employee Employee { get; set; } = null!;
    public int Mois { get; set; }
    public int Annee { get; set; }
    public Dictionary<string, RubriqueResult> Results { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, decimal> Constantes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, decimal> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, bool> VariablesActives { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, decimal> CumulsPrecedents { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<IrBracket> IrBrackets { get; set; } = new();

    public decimal GetMontant(string code) =>
        Results.TryGetValue(code, out var r) ? r.Montant : 0;

    public decimal SumGains()
    {
        decimal total = 0;
        foreach (var r in Results.Values)
        {
            if (r.Code == "BRUT_GLOBAL" || r.Code == "NET_IMPOSABLE" || r.Code == "NET_A_PAYER") continue;
            if (r.Gain > 0) total += r.Gain;
        }
        return total;
    }

    public decimal SumRetenues()
    {
        decimal total = 0;
        foreach (var r in Results.Values)
        {
            if (r.Code == "BRUT_GLOBAL" || r.Code == "NET_IMPOSABLE" || r.Code == "NET_A_PAYER") continue;
            if (r.Type == "Retenue" && r.Retenue > 0) total += r.Retenue;
        }
        return total;
    }

    public decimal SumAllRetenues()
    {
        decimal total = 0;
        foreach (var r in Results.Values)
        {
            if (r.Code == "NET_A_PAYER") continue;
            if (r.Retenue > 0 && r.Type is "Retenue" or "Cotisation" or "Impot") total += r.Retenue;
        }
        return total;
    }

    public decimal SumGainsBySection(string section) =>
        Results.Values.Where(r => r.Section.Equals(section, StringComparison.OrdinalIgnoreCase)).Sum(r => r.Gain);
    public decimal SumRetenuesBySection(string section) =>
        Results.Values.Where(r => r.Section.Equals(section, StringComparison.OrdinalIgnoreCase)).Sum(r => r.Retenue);
    public decimal SumGainsByType(string type) =>
        Results.Values.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).Sum(r => r.Gain);
    public decimal SumRetenuesByType(string type) =>
        Results.Values.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).Sum(r => r.Retenue);
}

public class IrBracket
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Taux { get; set; }
}

public class PayrollEngine
{
    private readonly MetaComptaDbContext _db;
    public PayrollEngine(MetaComptaDbContext db) => _db = db;

    public async Task<List<RubriqueResult>> CalculateAsync(
        Employee employee, int cabinetId, int annee, int mois, int? clientId = null)
    {
        var rubriques = await _db.Rubriques
            .Where(r => r.CabinetId == cabinetId && r.Actif
                && (r.ClientId == null || r.ClientId == clientId))
            .OrderBy(r => r.OrdreExecution)
            .ToListAsync();

        // Load employee's rubrique affectations (ERP-style)
        var affectations = await _db.AffectationsRubriques
            .Where(a => a.EmployeeId == employee.Id && a.CabinetId == cabinetId
                && !a.IsDeleted && a.Actif)
            .ToListAsync();
        var affectedRubriqueIds = new HashSet<int>(affectations.Select(a => a.RubriqueId));

        // Filter: only process rubriques that are Obligatoire OR assigned to this employee
        rubriques = rubriques.Where(r => r.Obligatoire || affectedRubriqueIds.Contains(r.Id)).ToList();

        var referenceDate = new DateTime(annee, mois, 1);
        var endOfMonth = new DateTime(annee, mois, DateTime.DaysInMonth(annee, mois));

        var constantes = await _db.ConstantesGlobales
            .Where(c => c.CabinetId == cabinetId && c.Actif
                && c.DateEffet <= endOfMonth
                && (c.DateFin == null || c.DateFin >= referenceDate))
            .OrderByDescending(c => c.DateEffet)
            .ToListAsync();

        var variables = await _db.VariablesEmployes
            .Where(v => v.EmployeeId == employee.Id && v.Actif
                && v.DateEffet <= endOfMonth
                && (v.DateFin == null || v.DateFin >= referenceDate))
            .ToListAsync();

        var cumuls = await _db.CumulsEmployes
            .Where(c => c.EmployeeId == employee.Id && c.Annee == annee && c.Mois < mois)
            .ToListAsync();

        var ctx = new PayrollContext { Employee = employee, Mois = mois, Annee = annee };

        foreach (var c in constantes)
            if (!ctx.Constantes.ContainsKey(c.Code))
                ctx.Constantes[c.Code] = c.Valeur;

        LoadIrBrackets(ctx);

        foreach (var v in variables.OrderByDescending(v => v.DateEffet))
            if (!ctx.Variables.ContainsKey(v.RubriqueCode))
            {
                ctx.Variables[v.RubriqueCode] = v.Valeur;
                ctx.VariablesActives[v.RubriqueCode] = v.Actif;
            }

        // Inject affectation default values for assigned rubriques when no monthly variable exists
        foreach (var aff in affectations)
        {
            if (!ctx.Variables.ContainsKey(aff.RubriqueCode) && aff.ValeurDefaut != 0)
            {
                ctx.Variables[aff.RubriqueCode] = aff.ValeurDefaut;
                ctx.VariablesActives[aff.RubriqueCode] = true;
            }
        }

        foreach (var g in cumuls.GroupBy(c => c.RubriqueCode))
            ctx.CumulsPrecedents[g.Key] = g.Sum(c => c.Valeur);

        foreach (var rubrique in rubriques)
        {
            if (!rubrique.Obligatoire &&
                ctx.VariablesActives.TryGetValue(rubrique.Code, out var isActive) && !isActive)
                continue;

            var result = EvaluateRubrique(rubrique, ctx);
            ctx.Results[rubrique.Code] = result;
        }

        return ctx.Results.Values.ToList();
    }

    private RubriqueResult EvaluateRubrique(Rubrique rubrique, PayrollContext ctx)
    {
        var result = new RubriqueResult
        {
            RubriqueId = rubrique.Id,
            Code = rubrique.Code,
            Libelle = rubrique.Libelle,
            Type = rubrique.Type,
            Section = rubrique.Section,
            OrdreAffichage = rubrique.OrdreAffichage,
            AfficherSurBulletin = rubrique.AfficherSurBulletin
        };

        result.Base = Eval(rubrique.FormuleBase, ctx);
        result.Taux = Eval(rubrique.FormuleTaux, ctx);

        if (!string.IsNullOrWhiteSpace(rubrique.FormuleMontant))
            result.Montant = Eval(rubrique.FormuleMontant, ctx);
        else if (result.Taux != 0)
            result.Montant = Math.Round(result.Base * result.Taux / 100m, 2);
        else
            result.Montant = result.Base;

        if (!string.IsNullOrWhiteSpace(rubrique.FormuleGain))
            result.Gain = Eval(rubrique.FormuleGain, ctx);
        else if (rubrique.Type is "Gain" or "Calcule")
            result.Gain = Math.Max(0, result.Montant);

        if (!string.IsNullOrWhiteSpace(rubrique.FormuleRetenue))
            result.Retenue = Eval(rubrique.FormuleRetenue, ctx);
        else if (rubrique.Type is "Retenue" or "Cotisation" or "Impot")
            result.Retenue = Math.Abs(result.Montant);

        result.Base = Math.Round(result.Base, 2);
        result.Taux = Math.Round(result.Taux, 6);
        result.Montant = Math.Round(result.Montant, 2);
        result.Gain = Math.Round(result.Gain, 2);
        result.Retenue = Math.Round(result.Retenue, 2);

        return result;
    }

    private decimal Eval(string? formula, PayrollContext ctx)
    {
        if (string.IsNullOrWhiteSpace(formula)) return 0;
        try
        {
            var resolved = ResolveAll(formula.Trim(), ctx);
            return EvaluateArithmetic(resolved);
        }
        catch { return 0; }
    }

    private string ResolveAll(string formula, PayrollContext ctx)
    {
        for (int pass = 0; pass < 5; pass++)
        {
            var prev = formula;
            formula = ResolveFunctions(formula, ctx);
            formula = ResolveReferences(formula, ctx);
            if (formula == prev) break;
        }
        return formula;
    }

    private string ResolveFunctions(string formula, PayrollContext ctx)
    {
        formula = Regex.Replace(formula, @"SUM_GAINS\(\)", _ =>
            ctx.SumGains().ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"SUM_RETENUES\(\)", _ =>
            ctx.SumAllRetenues().ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"SUM_GAINS_SECTION\((\w+)\)", m =>
            ctx.SumGainsBySection(m.Groups[1].Value).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"SUM_RETENUES_SECTION\((\w+)\)", m =>
            ctx.SumRetenuesBySection(m.Groups[1].Value).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"SUM_GAINS_TYPE\((\w+)\)", m =>
            ctx.SumGainsByType(m.Groups[1].Value).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"SUM_RETENUES_TYPE\((\w+)\)", m =>
            ctx.SumRetenuesByType(m.Groups[1].Value).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"CUMUL\((\w+)\)", m =>
        {
            ctx.CumulsPrecedents.TryGetValue(m.Groups[1].Value, out var v);
            return v.ToString(CultureInfo.InvariantCulture);
        }, RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"IR_PROGRESSIF\(([^)]+)\)", m =>
        {
            var inner = ResolveAll(m.Groups[1].Value, ctx);
            var baseIR = EvaluateArithmetic(inner);
            return CalculateProgressiveIR(baseIR, ctx).ToString(CultureInfo.InvariantCulture);
        }, RegexOptions.IgnoreCase);

        formula = ResolveMinMax(formula, "MIN", ctx, Math.Min);
        formula = ResolveMinMax(formula, "MAX", ctx, Math.Max);

        formula = Regex.Replace(formula, @"ROUND\(([^,]+),\s*(\d+)\)", m =>
        {
            var val = EvaluateArithmetic(ResolveAll(m.Groups[1].Value, ctx));
            var dec = int.Parse(m.Groups[2].Value);
            return Math.Round(val, dec).ToString(CultureInfo.InvariantCulture);
        }, RegexOptions.IgnoreCase);

        return formula;
    }

    private string ResolveMinMax(string formula, string funcName, PayrollContext ctx, Func<decimal, decimal, decimal> op)
    {
        var pattern = funcName + @"\(([^,()]+),\s*([^,()]+)\)";
        return Regex.Replace(formula, pattern, m =>
        {
            var a = EvaluateArithmetic(ResolveAll(m.Groups[1].Value.Trim(), ctx));
            var b = EvaluateArithmetic(ResolveAll(m.Groups[2].Value.Trim(), ctx));
            return op(a, b).ToString(CultureInfo.InvariantCulture);
        }, RegexOptions.IgnoreCase);
    }

    private string ResolveReferences(string formula, PayrollContext ctx)
    {
        formula = Regex.Replace(formula, @"CONST\.(\w+)", m =>
        {
            var code = m.Groups[1].Value;
            return ctx.Constantes.TryGetValue(code, out var val)
                ? val.ToString(CultureInfo.InvariantCulture) : "0";
        }, RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"VAR\.(\w+)", m =>
        {
            var code = m.Groups[1].Value;
            return ctx.Variables.TryGetValue(code, out var val)
                ? val.ToString(CultureInfo.InvariantCulture) : "0";
        }, RegexOptions.IgnoreCase);

        formula = Regex.Replace(formula, @"EMP\.(\w+)", m =>
        {
            var field = m.Groups[1].Value;
            return GetEmployeeField(ctx.Employee, field).ToString(CultureInfo.InvariantCulture);
        }, RegexOptions.IgnoreCase);

        var sortedCodes = ctx.Results.Keys.OrderByDescending(k => k.Length).ToList();
        foreach (var code in sortedCodes)
        {
            formula = Regex.Replace(formula, @"\b" + Regex.Escape(code) + @"\b",
                ctx.Results[code].Montant.ToString(CultureInfo.InvariantCulture),
                RegexOptions.IgnoreCase);
        }

        return formula;
    }

    private decimal GetEmployeeField(Employee emp, string field)
    {
        return field.ToUpperInvariant() switch
        {
            "SALAIRE_BASE" or "SALAIREBASE" => emp.SalaireBase,
            "PRIME_ANCIENNETE" or "PRIMEANCIENNETE" => emp.PrimeAnciennete ?? 0,
            "PRIME_RESPONSABILITE" or "PRIMERESPONSABILITE" => emp.PrimeResponsabilite ?? 0,
            "PRIME_PRESENCE" or "PRIMEPRESENCE" => emp.PrimePresence ?? 0,
            "INDEMNITE_TRANSPORT" or "INDEMNITETRANSPORT" => emp.IndemniteTransport ?? 0,
            "INDEMNITE_REPRESENTATION" or "INDEMNITEREPRESENTATION" => emp.IndemniteRepresentation ?? 0,
            "AVANTAGES_NATURE" or "AVANTAGESNATURE" => emp.AvantagesNature ?? 0,
            "TAUX_HORAIRE" or "TAUXHORAIRE" => emp.TauxHoraire ?? (emp.SalaireBase / 191m),
            "NB_ENFANTS" or "NOMBREENFANTSACHARGE" => emp.NombreEnfantsACharge,
            "ANCIENNETE" => CalculateAnciennete(emp),
            _ => 0
        };
    }

    private decimal CalculateAnciennete(Employee emp)
    {
        var years = (DateTime.UtcNow - emp.DateEmbauche).TotalDays / 365.25;
        return Math.Floor((decimal)years);
    }

    private decimal CalculateProgressiveIR(decimal netImposableMensuel, PayrollContext ctx)
    {
        if (netImposableMensuel <= 0) return 0;

        ctx.Constantes.TryGetValue("IR_DEDUCTION_FAMILLE", out var deductionParEnfant);
        ctx.Constantes.TryGetValue("IR_PLAFOND_ENFANTS", out var plafondEnfantsDec);
        var plafondEnfants = plafondEnfantsDec > 0 ? (int)plafondEnfantsDec : 6;
        var nbEnfants = Math.Min(ctx.Employee.NombreEnfantsACharge, plafondEnfants);

        var netImposableAnnuel = netImposableMensuel * 12m;
        var fraisPro = Math.Min(netImposableAnnuel * 0.20m, 30000m);
        var deductionFamille = nbEnfants * deductionParEnfant;
        var baseIRAnnuel = netImposableAnnuel - fraisPro - deductionFamille;
        if (baseIRAnnuel <= 0) return 0;

        decimal irAnnuel = 0;
        foreach (var bracket in ctx.IrBrackets.OrderBy(b => b.Min))
        {
            if (baseIRAnnuel <= bracket.Min) break;
            var taxable = Math.Min(baseIRAnnuel, bracket.Max) - bracket.Min;
            if (taxable > 0)
                irAnnuel += taxable * bracket.Taux / 100m;
        }

        return Math.Round(irAnnuel / 12m, 2);
    }

    private void LoadIrBrackets(PayrollContext ctx)
    {
        for (int i = 1; i <= 10; i++)
        {
            if (ctx.Constantes.TryGetValue($"IR_TRANCHE_{i}_MIN", out var min) &&
                ctx.Constantes.TryGetValue($"IR_TRANCHE_{i}_MAX", out var max) &&
                ctx.Constantes.TryGetValue($"IR_TRANCHE_{i}_TAUX", out var taux))
                ctx.IrBrackets.Add(new IrBracket { Min = min, Max = max, Taux = taux });
            else break;
        }
    }

    private decimal EvaluateArithmetic(string expression)
    {
        try
        {
            expression = expression.Trim();
            if (string.IsNullOrEmpty(expression)) return 0;
            if (decimal.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out var direct))
                return direct;
            var tokens = Tokenize(expression);
            if (tokens.Count == 0) return 0;
            var rpn = ShuntingYard(tokens);
            return EvaluateRPN(rpn);
        }
        catch { return 0; }
    }

    private enum TokenType { Number, Operator, LeftParen, RightParen }
    private record Token(TokenType Type, string Value, decimal? NumValue = null);

    private List<Token> Tokenize(string expr)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (char.IsDigit(c) || c == '.' ||
                (c == '-' && i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.') &&
                 (tokens.Count == 0 || tokens[^1].Type == TokenType.Operator || tokens[^1].Type == TokenType.LeftParen)))
            {
                var start = i;
                if (c == '-') i++;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
                var numStr = expr[start..i];
                if (decimal.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                    tokens.Add(new Token(TokenType.Number, numStr, num));
                continue;
            }
            if (c is '+' or '-' or '*' or '/') { tokens.Add(new Token(TokenType.Operator, c.ToString())); i++; continue; }
            if (c == '(') { tokens.Add(new Token(TokenType.LeftParen, "(")); i++; continue; }
            if (c == ')') { tokens.Add(new Token(TokenType.RightParen, ")")); i++; continue; }
            if (char.IsLetter(c) || c == '_')
            {
                while (i < expr.Length && (char.IsLetterOrDigit(expr[i]) || expr[i] == '_' || expr[i] == '.')) i++;
                tokens.Add(new Token(TokenType.Number, "0", 0));
                continue;
            }
            i++;
        }
        return tokens;
    }

    private static readonly Dictionary<string, int> Precedence = new()
    {
        { "+", 1 }, { "-", 1 }, { "*", 2 }, { "/", 2 }
    };

    private List<Token> ShuntingYard(List<Token> tokens)
    {
        var output = new List<Token>();
        var ops = new Stack<Token>();
        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number: output.Add(token); break;
                case TokenType.Operator:
                    while (ops.Count > 0 && ops.Peek().Type == TokenType.Operator &&
                           Precedence.GetValueOrDefault(ops.Peek().Value) >= Precedence.GetValueOrDefault(token.Value))
                        output.Add(ops.Pop());
                    ops.Push(token);
                    break;
                case TokenType.LeftParen: ops.Push(token); break;
                case TokenType.RightParen:
                    while (ops.Count > 0 && ops.Peek().Type != TokenType.LeftParen) output.Add(ops.Pop());
                    if (ops.Count > 0) ops.Pop();
                    break;
            }
        }
        while (ops.Count > 0) output.Add(ops.Pop());
        return output;
    }

    private decimal EvaluateRPN(List<Token> rpn)
    {
        var stack = new Stack<decimal>();
        foreach (var token in rpn)
        {
            if (token.Type == TokenType.Number) { stack.Push(token.NumValue ?? 0); }
            else if (token.Type == TokenType.Operator && stack.Count >= 2)
            {
                var b = stack.Pop(); var a = stack.Pop();
                stack.Push(token.Value switch
                {
                    "+" => a + b, "-" => a - b, "*" => a * b,
                    "/" => b != 0 ? Math.Round(a / b, 6) : 0, _ => 0
                });
            }
        }
        return stack.Count > 0 ? Math.Round(stack.Pop(), 2) : 0;
    }
}
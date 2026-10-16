using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace MetaCompta.BackOffice.Services;

/// <summary>
/// Parses DGI Maroc BNIF PDFs (Bulletin de Notification du N° d'Identification Fiscale — modèle AAC140B).
/// Uses word geometry (Y/X) because page.Text is mis-ordered on these JasperReports PDFs.
/// </summary>
public static class BnifPdfParser
{
    private static readonly string[] KnownCities =
    [
        "Casablanca", "Rabat", "Marrakech", "Fès", "Fes", "Tanger", "Agadir", "Meknès", "Meknes",
        "Oujda", "Kénitra", "Kenitra", "Tétouan", "Tetouan", "Salé", "Sale", "Nador", "Mohammédia",
        "Mohammedia", "El Jadida", "Béni Mellal", "Beni Mellal", "Khouribga", "Settat", "Safi"
    ];

    private static readonly string[] LabelMarkers =
    [
        "RAISON SOCIALE", "NOM ET PRENOM", "FORME JURIDIQUE", "REGISTRE DE COMMERCE",
        "N° ICE", "N ICE", "TAXE PROFESSIONNELLE", "SIEGE SOCIAL", "DOMICILE FISCAL",
        "ACTIVITE(S) PRINCIPALE", "PROFESSION(S) OU ACTIVITE", "ADRESSE DU DIRIGEANT",
        "REPRESENTANT LEGAL", "CNI/", "CARTE DE SEJOUR", "CODE D'ACCES", "CODE D ACCES",
        "IDENTIFICATION FISCALE", "DATE D'EDITION", "DATE D’ÉDITION", "SERVICE D'ASSIETTE",
        "RECETTE DE L'ADMINISTRATION", "BULLETIN DE NOTIFICATION", "IMPOT SUR", "TAXE SUR LA VALEUR"
    ];

    public sealed class Result
    {
        public string? RaisonSociale { get; set; }
        public string? ICE { get; set; }
        public string? IF { get; set; }
        public string? RC { get; set; }
        public string? NumeroPatente { get; set; }
        public string? FormeJuridique { get; set; }
        public string? SecteurActivite { get; set; }
        public string? AdresseSiegeSocial { get; set; }
        public string? VilleSiegeSocial { get; set; }
        public DateTime? DateCreation { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> FilledFields { get; set; } = new();
    }

    public static Result Parse(Stream pdfStream, string? fileName = null)
    {
        using var doc = PdfDocument.Open(pdfStream);
        var lines = new List<string>();
        foreach (var page in doc.GetPages())
            lines.AddRange(BuildLines(page));

        var result = new Result();
        var joined = string.Join('\n', lines);

        if (!joined.Contains("BULLETIN DE NOTIFICATION", StringComparison.OrdinalIgnoreCase)
            && !joined.Contains("IDENTIFICATION FISCALE", StringComparison.OrdinalIgnoreCase)
            && !joined.Contains("RAISON SOCIALE", StringComparison.OrdinalIgnoreCase)
            && !Regex.IsMatch(fileName ?? "", @"BNIF_", RegexOptions.IgnoreCase))
        {
            result.Warnings.Add("Le PDF ne ressemble pas à un BNIF DGI standard. Vérifiez le document.");
        }

        // Raison sociale — value on same line after label, or next content line
        var rs = ValueAfterLabel(lines,
            l => ContainsAny(l, "RAISON SOCIALE", "NOM ET PRENOM"));
        result.RaisonSociale = CleanBusinessName(rs);
        SetIfPresent(result, nameof(Result.RaisonSociale), result.RaisonSociale,
            "Raison sociale non détectée.");

        // Forme juridique
        var formeRaw = ValueAfterLabel(lines, l => ContainsAny(l, "FORME JURIDIQUE"));
        result.FormeJuridique = MapFormeJuridique(formeRaw);
        SetIfPresent(result, nameof(Result.FormeJuridique), result.FormeJuridique);

        // RC — only keep if it looks like an RC number (not the next field's label/value)
        var rc = ValueAfterLabel(lines, l => ContainsAny(l, "REGISTRE DE COMMERCE"));
        result.RC = CleanOptionalId(rc);
        if (result.RC != null && (
                ContainsAny(result.RC, "N° ICE", "N ICE", "TAXE PROFESSIONNELLE", "SIEGE SOCIAL")
                || Regex.IsMatch(result.RC, @"^\d{15}$")))
            result.RC = null;
        SetIfPresent(result, nameof(Result.RC), result.RC);

        // ICE — 15 digits near N° ICE line / following line
        var iceCtx = ValueAfterLabel(lines, l => Regex.IsMatch(l, @"N[°º]?\s*ICE", RegexOptions.IgnoreCase)
                                                  || l.Contains("N ICE", StringComparison.OrdinalIgnoreCase));
        result.ICE = ExtractFirst(iceCtx ?? string.Empty, @"\b(\d{15})\b")
                     ?? ExtractFirst(joined, @"N[°º]?\s*ICE\s*:?\s*(\d{15})")
                     ?? ExtractFirst(string.Join(' ', lines.Where(LooksLikeValueLine)), @"\b(\d{15})\b");
        SetIfPresent(result, nameof(Result.ICE), result.ICE, "ICE non détecté.");

        // Taxe professionnelle → patente (digits on same/next line, or any 5–12 digit id that isn't ICE/IF)
        var tp = ValueAfterLabel(lines, l => ContainsAny(l, "TAXE PROFESSIONNELLE"));
        result.NumeroPatente = ExtractFirst(tp ?? string.Empty, @"\b(\d{5,12})\b")
                               ?? ExtractDigitsNearLabel(lines, "TAXE PROFESSIONNELLE");
        if (!string.IsNullOrEmpty(result.NumeroPatente) && result.NumeroPatente == result.ICE)
            result.NumeroPatente = null;
        SetIfPresent(result, nameof(Result.NumeroPatente), result.NumeroPatente);

        // Adresse siège
        var adr = ValueAfterLabel(lines, l => ContainsAny(l, "SIEGE SOCIAL", "DOMICILE FISCAL", "ETABLISSEMENT"));
        // Prefer address-looking lines (LOT / RUE / MAGASIN) if label line empty
        if (string.IsNullOrWhiteSpace(adr) || LooksLikeLabelOnly(adr))
        {
            adr = lines.FirstOrDefault(l =>
                Regex.IsMatch(l, @"\b(LOT|RUE|AV\.|AVENUE|BD|BOULEVARD|MAGASIN|QUARTIER|HAY)\b", RegexOptions.IgnoreCase)
                && !ContainsAny(l, "DIRIGEANT", "REPRESENTANT", "RECETTE", "SERVICE D"));
        }
        result.AdresseSiegeSocial = CleanAddress(adr);
        if (!string.IsNullOrWhiteSpace(result.AdresseSiegeSocial))
        {
            result.FilledFields.Add(nameof(Result.AdresseSiegeSocial));
            result.VilleSiegeSocial = InferVille(result.AdresseSiegeSocial);
            SetIfPresent(result, nameof(Result.VilleSiegeSocial), result.VilleSiegeSocial);
        }

        // Activité
        var act = ValueAfterLabel(lines, l => ContainsAny(l, "ACTIVITE(S) PRINCIPALE", "PROFESSION(S) OU ACTIVITE", "ACTIVITE(S)"));
        result.SecteurActivite = CleanActivity(act);
        SetIfPresent(result, nameof(Result.SecteurActivite), result.SecteurActivite);

        // IF — notified number (often alone on a line / after activité); filename fallback
        var fileIf = ExtractFirst(fileName ?? "", @"BNIF[_\-](\d{4,12})[_\-]", RegexOptions.IgnoreCase);
        var bodyIf = FindIdentifiantFiscal(lines, result.ICE, result.NumeroPatente);
        result.IF = bodyIf ?? fileIf;
        SetIfPresent(result, nameof(Result.IF), result.IF, "Identifiant fiscal (IF) non détecté automatiquement.");

        // Date d'édition
        var dateLine = lines.FirstOrDefault(l => ContainsAny(l, "DATE D'EDITION", "DATE D’ÉDITION", "DATE D'ÉDITION"))
                       ?? lines.FirstOrDefault(l => Regex.IsMatch(l, @"\b\d{2}[-/]\d{2}[-/]\d{4}\b"));
        var dateStr = ExtractFirst(dateLine ?? "", @"(\d{2}[-/]\d{2}[-/]\d{4})")
                      ?? ExtractFirst(fileName ?? "", @"(\d{2}_\d{2}_\d{4})")?.Replace('_', '-');
        if (dateStr != null
            && DateTime.TryParseExact(dateStr.Replace('/', '-'), "dd-MM-yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
        {
            result.DateCreation = dt.Date;
            result.FilledFields.Add(nameof(Result.DateCreation));
        }

        if (result.FilledFields.Count == 0)
            result.Warnings.Add("Aucun champ n'a pu être extrait. Remplissez le formulaire manuellement.");

        return result;
    }

    private static IEnumerable<string> BuildLines(Page page)
    {
        var words = page.GetWords()
            .Select(w => new
            {
                Text = SanitizeToken(w.Text),
                Y = w.BoundingBox.Bottom,
                X = w.BoundingBox.Left
            })
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .OrderByDescending(w => w.Y)
            .ThenBy(w => w.X)
            .ToList();

        if (words.Count == 0) yield break;

        var line = new StringBuilder();
        double? lastY = null;
        foreach (var w in words)
        {
            var y = Math.Round(w.Y, 0);
            if (lastY != null && Math.Abs(lastY.Value - y) > 3)
            {
                var built = NormalizeSpaces(line.ToString());
                if (!string.IsNullOrWhiteSpace(built))
                    yield return built;
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(w.Text);
            lastY = y;
        }
        var last = NormalizeSpaces(line.ToString());
        if (!string.IsNullOrWhiteSpace(last))
            yield return last;
    }

    // Arabic + presentation forms (BNIF bilingual column often uses FB50–FEFF glyphs)
    private static readonly Regex ArabicOrPresentation = new(
        @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]",
        RegexOptions.Compiled);

    private static string SanitizeToken(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        // BNIF bilingual layout: Arabic tokens sit on the same Y as Latin labels/values — drop them.
        if (ArabicOrPresentation.IsMatch(text)) return string.Empty;
        return text.Trim();
    }

    private static bool IsMostlyNonLatin(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (ArabicOrPresentation.IsMatch(text)) return true;
        // leftover isolated presentation forms / empty after strip
        var letters = text.Count(char.IsLetter);
        var latin = text.Count(c => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= 'À' and <= 'ÿ'));
        return letters > 0 && latin == 0;
    }

    private static string? ValueAfterLabel(List<string> lines, Func<string, bool> isLabelLine)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!isLabelLine(line)) continue;

            // Same-line value after colon (Latin only)
            var colon = line.IndexOf(':');
            if (colon >= 0 && colon < line.Length - 1)
            {
                var same = StripLeadingLabelJunk(line[(colon + 1)..].Trim());
                if (!IsMostlyNonLatin(same) && !LooksLikeLabelOnly(same) && LooksLikeValueLine(same))
                    return same;
            }

            // Next content lines that aren't labels / Arabic mirrors / boilerplate
            for (var j = i + 1; j < Math.Min(i + 6, lines.Count); j++)
            {
                var next = lines[j].Trim();
                if (string.IsNullOrWhiteSpace(next)) continue;
                if (IsMostlyNonLatin(next)) continue;
                if (LooksLikeLabelOnly(next)) continue;
                if (isLabelLine(next)) continue;
                if (IsBoilerplate(next)) continue;
                return next;
            }
        }
        return null;
    }

    private static string? ExtractDigitsNearLabel(List<string> lines, string labelMarker)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (!ContainsAny(lines[i], labelMarker)) continue;
            var onLine = ExtractFirst(lines[i], @"\b(\d{5,12})\b");
            if (onLine != null) return onLine;
            for (var j = i + 1; j < Math.Min(i + 5, lines.Count); j++)
            {
                if (IsMostlyNonLatin(lines[j])) continue;
                if (LooksLikeLabelOnly(lines[j])) continue;
                var cand = ExtractFirst(lines[j], @"\b(\d{5,12})\b");
                if (cand != null) return cand;
            }
        }
        return null;
    }

    private static string? FindIdentifiantFiscal(List<string> lines, string? ice, string? patente)
    {
        // Prefer standalone 5–10 digit number that isn't ICE/patente, near notification wording
        for (var i = 0; i < lines.Count; i++)
        {
            var l = lines[i];
            if (ContainsAny(l, "NUMERO D'IDENTIFICATION FISCALE", "NUMÉRO D'IDENTIFICATION FISCALE",
                    "COMMUNIQUER VOTRE", "IDENTIFIANT FISCAL"))
            {
                var onLine = ExtractFirst(l, @"\b(\d{4,12})\b");
                if (IsPlausibleIf(onLine, ice, patente)) return onLine;
                for (var j = i + 1; j < Math.Min(i + 6, lines.Count); j++)
                {
                    var cand = ExtractFirst(lines[j], @"^\s*(\d{4,12})\s*$")
                               ?? ExtractFirst(lines[j], @"\b(\d{4,12})\b");
                    if (IsPlausibleIf(cand, ice, patente)) return cand;
                }
            }
        }

        // Fallback: line that is only digits 4–12
        foreach (var l in lines)
        {
            var cand = ExtractFirst(l, @"^\s*(\d{4,12})\s*$");
            if (IsPlausibleIf(cand, ice, patente)) return cand;
        }

        return null;
    }

    private static bool IsPlausibleIf(string? cand, string? ice, string? patente)
    {
        if (string.IsNullOrWhiteSpace(cand)) return false;
        if (cand.Length is < 4 or > 12) return false;
        if (cand.Length == 15) return false;
        if (!string.IsNullOrEmpty(ice) && cand == ice) return false;
        if (!string.IsNullOrEmpty(patente) && cand == patente) return false;
        return true;
    }

    private static void SetIfPresent(Result result, string field, string? value, string? warningIfMissing = null)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (!result.FilledFields.Contains(field))
                result.FilledFields.Add(field);
        }
        else if (!string.IsNullOrWhiteSpace(warningIfMissing))
        {
            result.Warnings.Add(warningIfMissing);
        }
    }

    private static bool ContainsAny(string line, params string[] markers)
        => markers.Any(m => line.Contains(m, StringComparison.OrdinalIgnoreCase));

    private static bool LooksLikeLabelOnly(string text)
    {
        var u = text.Trim().ToUpperInvariant().TrimEnd(':', ' ');
        if (LabelMarkers.Any(m => u.Equals(m, StringComparison.OrdinalIgnoreCase)
                                  || u.Equals(m + " :", StringComparison.OrdinalIgnoreCase)))
            return true;

        // Short line dominated by a known label and no substantial value digits/name
        if (text.Length <= 55
            && !Regex.IsMatch(text, @"\d{5,}")
            && LabelMarkers.Any(m => u.Contains(m, StringComparison.OrdinalIgnoreCase))
            && text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 10)
            return true;

        return false;
    }

    private static bool LooksLikeValueLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (IsBoilerplate(text)) return false;
        if (LooksLikeLabelOnly(text) && ExtractFirst(text, @"\d{5,}") == null) return false;
        return true;
    }

    private static bool IsBoilerplate(string text)
    {
        return ContainsAny(text,
            "Nous avons l'honneur", "obligatoirement mentionné", "Code Général des Impôts",
            "Code de vérification", "www.tax.gov.ma", "CODE D'ACCES", "Visa de l'Administration",
            "Par ailleurs", "factures ou mémoires");
    }

    private static string? CleanBusinessName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || IsMostlyNonLatin(raw)) return null;
        var v = StripLeadingLabelJunk(raw);
        // Cut if parser accidentally glued labels
        foreach (var stop in new[] { "FORME JURIDIQUE", "N° ICE", "N ICE", "IDENTIFIANT", "PROFESSION", "Nous avons" })
        {
            var idx = v.IndexOf(stop, StringComparison.OrdinalIgnoreCase);
            if (idx > 0) v = v[..idx];
        }
        v = NormalizeSpaces(v).Trim(' ', ':', '-', '·');
        if (v.Length < 2 || LooksLikeLabelOnly(v)) return null;
        if (v.Length > 200) v = v[..200];
        return v;
    }

    private static string? CleanOptionalId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || IsMostlyNonLatin(raw)) return null;
        var v = StripLeadingLabelJunk(raw);
        if (LooksLikeLabelOnly(v)) return null;
        // Reject if we accidentally captured the next field (ICE / taxe / address)
        if (ContainsAny(v, "N° ICE", "N ICE", "TAXE PROFESSIONNELLE", "SIEGE SOCIAL", "FORME JURIDIQUE"))
            return null;
        v = NormalizeSpaces(v);
        if (v.Length < 2) return null;
        // Prefer RC-looking tokens (alphanumeric, not a lone 15-digit ICE)
        if (Regex.IsMatch(v, @"^\d{15}$")) return null;
        if (v.Length > 50) v = v[..50];
        return v;
    }

    private static string? CleanAddress(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || IsMostlyNonLatin(raw)) return null;
        var v = StripLeadingLabelJunk(raw);
        foreach (var stop in new[] { "PROFESSION", "ACTIVITE", "ADRESSE DU DIRIGEANT", "Nous avons", "TAXE SUR" })
        {
            var idx = v.IndexOf(stop, StringComparison.OrdinalIgnoreCase);
            if (idx > 0) v = v[..idx];
        }
        v = NormalizeSpaces(v).Trim(' ', ':', '-', '·');
        if (v.Length < 5 || LooksLikeLabelOnly(v)) return null;
        if (v.Length > 300) v = v[..300];
        return v;
    }

    private static string? CleanActivity(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || IsMostlyNonLatin(raw)) return null;
        var v = StripLeadingLabelJunk(raw);
        foreach (var stop in new[] { "ADRESSE DU DIRIGEANT", "REPRESENTANT", "Nous avons", "TAXE SUR", "CODE D" })
        {
            var idx = v.IndexOf(stop, StringComparison.OrdinalIgnoreCase);
            if (idx > 0) v = v[..idx];
        }
        v = NormalizeSpaces(v).Trim(' ', ':', '-', '·');
        if (v.Length < 2 || LooksLikeLabelOnly(v)) return null;
        if (v.Length > 100) v = v[..100];
        return v;
    }

    private static string StripLeadingLabelJunk(string value)
    {
        var v = value.Trim();
        v = Regex.Replace(v, @"^(N[°º]?\s*CNI[^\s]*\s*)+", "", RegexOptions.IgnoreCase);
        v = Regex.Replace(v, @"^[:\-\s]+", "");
        // Drop leading Arabic leftovers already sanitized
        return NormalizeSpaces(v);
    }

    private static string? MapFormeJuridique(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var u = raw.ToUpperInvariant();
        if (u.Contains("ASSOCIE UNIQUE") || u.Contains("ASSOCIÉ UNIQUE") || u.Contains("SARLAU") || u.Contains("SARL AU"))
            return "SARL";
        if (u.Contains("SARL")) return "SARL";
        if (Regex.IsMatch(u, @"\bSA\b") && !u.Contains("SARL")) return "SA";
        if (u.Contains("SNC")) return "SNC";
        if (u.Contains("SCS")) return "SCS";
        if (u.Contains("SCA")) return "SCA";
        if (u.Contains("AUTO") && u.Contains("ENTREPRENEUR")) return "Auto-Entrepreneur";
        if (u.Contains("ASSOCIATION")) return "Association";
        if (u.Contains("COOP")) return "Coopérative";
        if (u.Contains("PERSONNE PHYSIQUE")) return "Personne Physique";
        return null;
    }

    private static string? InferVille(string adresse)
    {
        foreach (var city in KnownCities.OrderByDescending(c => c.Length))
        {
            if (adresse.Contains(city, StringComparison.OrdinalIgnoreCase))
            {
                return city switch
                {
                    "Fes" => "Fès",
                    "Meknes" => "Meknès",
                    "Kenitra" => "Kénitra",
                    "Tetouan" => "Tétouan",
                    "Sale" => "Salé",
                    "Mohammedia" => "Mohammédia",
                    "Beni Mellal" => "Béni Mellal",
                    _ => city
                };
            }
        }
        return null;
    }

    private static string? ExtractFirst(string input, string pattern, RegexOptions options = RegexOptions.None)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var m = Regex.Match(input, pattern, options);
        return m.Success && m.Groups.Count > 1 ? m.Groups[1].Value.Trim() : null;
    }

    private static string NormalizeSpaces(string value)
        => Regex.Replace(value.Replace('\u00A0', ' '), @"\s+", " ").Trim();
}

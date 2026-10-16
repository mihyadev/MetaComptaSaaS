namespace MetaCompta.Domain.Common;

/// <summary>
/// Morocco TVA calendar (CGI + DGI filing windows):
/// activity month M is opened/checked in month M+1 (open day 1 → due = end of filing month).
/// YYYY = année d'activité / paramètre runtime (not a fixed calendar year in the template).
/// </summary>
public static class MoroccoTvaCalendar
{
    private static readonly string[] MonthFr =
    {
        "", "janvier", "f\u00e9vrier", "mars", "avril", "mai", "juin",
        "juillet", "ao\u00fbt", "septembre", "octobre", "novembre", "d\u00e9cembre"
    };

    private static readonly string[] MonthFrCap =
    {
        "", "Janvier", "F\u00e9vrier", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Ao\u00fbt", "Septembre", "Octobre", "Novembre", "D\u00e9cembre"
    };

    private static readonly int[] DimPattern = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    /// <summary>
    /// Default filing window for activity month 1..12
    /// (filing month = activity month + 1, Dec → Jan YYYY+1).
    /// Open day = 1; due day = last day of filing month (28/29/30/31).
    /// </summary>
    private static (int OpenDay, int DueDay) FilingWindow(int activityMonth)
    {
        var (fm, _) = FilingOf(activityMonth);
        return (1, DimPattern[fm]);
    }

    public sealed record DeclarationRow(
        int Index,
        DateTime ActivityStart,
        DateTime ActivityEnd,
        DateTime OpenDate,
        DateTime Deadline,
        string ActivityLabel,
        string OpenLabel,
        string DeadlineLabel,
        bool DisplayOnly = false);

    public sealed record TemplateRow(
        int Index,
        string ActivityLabel,
        string OpenLabel,
        string DeadlineLabel,
        int ActivityMonth,
        int ActivityYearOffset,
        int OpenMonth,
        int OpenDay,
        int OpenYearOffset,
        int DueMonth,
        int DueDay,
        int DueYearOffset,
        bool DisplayOnly = false);

    /// <summary>Relative Init pattern — years filled at runtime via YYYY.</summary>
    public sealed record PatternRow(
        string Label,
        int StartMonth,
        int StartDay,
        int EndMonth,
        int EndDay,
        int OpenMonth,
        int OpenDay,
        int DueMonth,
        int DueDay);

    private static string YearToken(int offsetFromYyy) => offsetFromYyy switch
    {
        -1 => "YYYY-1",
        0 => "YYYY",
        1 => "YYYY+1",
        _ => offsetFromYyy > 0 ? $"YYYY+{offsetFromYyy}" : $"YYYY{offsetFromYyy}"
    };

    private static (int FilingMonth, int FilingYearOffset) FilingOf(int activityMonth)
    {
        if (activityMonth >= 12)
            return (1, 1);
        return (activityMonth + 1, 0);
    }

    /// <summary>
    /// Editable monthly template (12 rows): activity Jan…Dec YYYY,
    /// open/check + échéance in month M+1 (DGI windows).
    /// </summary>
    public static IReadOnlyList<TemplateRow> GenerateMonthlyTemplate()
    {
        var list = new List<TemplateRow>(12);
        for (var activityMonth = 1; activityMonth <= 12; activityMonth++)
        {
            var (fm, fOff) = FilingOf(activityMonth);
            var (openDay, dueDay) = FilingWindow(activityMonth);
            list.Add(new TemplateRow(
                activityMonth,
                $"{MonthFrCap[activityMonth]} {YearToken(0)}",
                $"{openDay:D2}/{fm:D2}/{YearToken(fOff)}",
                $"{dueDay:D2}/{fm:D2}/{YearToken(fOff)}",
                activityMonth, 0,
                fm, openDay, fOff,
                fm, dueDay, fOff,
                DisplayOnly: false));
        }
        return list;
    }

    public static IReadOnlyList<TemplateRow> GenerateQuarterlyTemplate()
    {
        // Keep quarterly N+1: activity quarter closed → file in first month of next quarter (EOM default editable)
        var defs = new (string Act, int Sm, int Em, int Ed, int Om, int Od, int Dm, int Dd, int ActOff, int FileOff)[]
        {
            ("Octobre\u2013d\u00e9cembre", 10, 12, 31, 1, 1, 1, 31, -1, 0),
            ("Janvier\u2013mars", 1, 3, 31, 4, 1, 4, 30, 0, 0),
            ("Avril\u2013juin", 4, 6, 30, 7, 1, 7, 31, 0, 0),
            ("Juillet\u2013septembre", 7, 9, 30, 10, 1, 10, 31, 0, 0),
        };
        var list = new List<TemplateRow>(4);
        foreach (var d in defs)
        {
            list.Add(new TemplateRow(
                list.Count + 1,
                $"{d.Act} {YearToken(d.ActOff)}",
                $"{d.Od:D2}/{d.Om:D2}/{YearToken(d.FileOff)}",
                $"{d.Dd:D2}/{d.Dm:D2}/{YearToken(d.FileOff)}",
                d.Sm, d.ActOff,
                d.Om, d.Od, d.FileOff,
                d.Dm, d.Dd, d.FileOff));
        }
        return list;
    }

    /// <summary>12 monthly patterns — Jan activity first, DGI open/due windows.</summary>
    public static IReadOnlyList<PatternRow> MonthlyPatterns()
    {
        var list = new List<PatternRow>(12);
        for (var activityMonth = 1; activityMonth <= 12; activityMonth++)
        {
            var (fm, _) = FilingOf(activityMonth);
            var (openDay, dueDay) = FilingWindow(activityMonth);
            list.Add(new PatternRow(
                $"{MonthFrCap[activityMonth]} (d\u00e9p\u00f4t {MonthFr[fm]})",
                activityMonth, 1,
                activityMonth, DimPattern[activityMonth],
                fm, openDay,
                fm, dueDay));
        }
        return list;
    }

    public static IReadOnlyList<PatternRow> QuarterlyPatterns()
    {
        return new[]
        {
            new PatternRow("Octobre\u2013d\u00e9cembre (d\u00e9p\u00f4t janvier)", 10, 1, 12, 31, 1, 1, 1, 31),
            new PatternRow("Janvier\u2013mars (d\u00e9p\u00f4t avril)", 1, 1, 3, 31, 4, 1, 4, 30),
            new PatternRow("Avril\u2013juin (d\u00e9p\u00f4t juillet)", 4, 1, 6, 30, 7, 1, 7, 31),
            new PatternRow("Juillet\u2013septembre (d\u00e9p\u00f4t octobre)", 7, 1, 9, 30, 10, 1, 10, 31),
        };
    }

    public static IReadOnlyList<DeclarationRow> GenerateMonthlyDeclarationYear(int activityYear)
    {
        var list = new List<DeclarationRow>(12);
        foreach (var p in MonthlyPatterns())
        {
            var actYear = activityYear;
            var fileYear = p.DueMonth < p.StartMonth ? activityYear + 1 : activityYear;
            var actStart = new DateTime(actYear, p.StartMonth, 1);
            var actEnd = new DateTime(actYear, p.EndMonth, DateTime.DaysInMonth(actYear, p.EndMonth));
            var open = new DateTime(fileYear, p.OpenMonth, Math.Min(p.OpenDay, DateTime.DaysInMonth(fileYear, p.OpenMonth)));
            var due = new DateTime(fileYear, p.DueMonth, Math.Min(p.DueDay, DateTime.DaysInMonth(fileYear, p.DueMonth)));
            list.Add(new DeclarationRow(
                list.Count + 1,
                actStart, actEnd, open, due,
                $"{MonthFrCap[p.StartMonth]} {actYear}",
                $"{open:dd/MM/yyyy}",
                $"{due:dd/MM/yyyy}"));
        }
        return list;
    }

    public static IReadOnlyList<DeclarationRow> GenerateQuarterlyDeclarationYear(int declarationYear)
    {
        var list = new List<DeclarationRow>(4);
        foreach (var p in QuarterlyPatterns())
        {
            var actYear = p.StartMonth >= 10 && p.DueMonth == 1 ? declarationYear - 1 : declarationYear;
            var fileYear = p.DueMonth == 1 && p.StartMonth >= 10 ? declarationYear : declarationYear;
            if (p.StartMonth >= 10 && p.DueMonth == 1) { actYear = declarationYear - 1; fileYear = declarationYear; }
            var actStart = new DateTime(actYear, p.StartMonth, 1);
            var actEnd = new DateTime(actYear, p.EndMonth, Math.Min(p.EndDay, DateTime.DaysInMonth(actYear, p.EndMonth)));
            var open = new DateTime(fileYear, p.OpenMonth, Math.Min(p.OpenDay, DateTime.DaysInMonth(fileYear, p.OpenMonth)));
            var due = new DateTime(fileYear, p.DueMonth, Math.Min(p.DueDay, DateTime.DaysInMonth(fileYear, p.DueMonth)));
            list.Add(new DeclarationRow(
                list.Count + 1,
                actStart, actEnd, open, due,
                $"{p.Label.Split('(')[0].Trim()} {actYear}",
                $"{open:dd/MM/yyyy}",
                $"{due:dd/MM/yyyy}"));
        }
        return list;
    }

    public static IReadOnlyList<DeclarationRow> MaterializeFromPatterns(
        int year,
        bool monthly,
        IReadOnlyList<(int DueMonth, int DueDay)>? dueOverrides = null)
    {
        var rows = (monthly ? GenerateMonthlyDeclarationYear(year) : GenerateQuarterlyDeclarationYear(year)).ToList();
        if (dueOverrides == null) return rows;
        for (var i = 0; i < Math.Min(rows.Count, dueOverrides.Count); i++)
        {
            var (dm, dd) = dueOverrides[i];
            if (dm is < 1 or > 12) continue;
            var y = rows[i].Deadline.Year;
            if (dm < rows[i].ActivityEnd.Month) y = rows[i].ActivityEnd.Year + 1;
            var dim = DateTime.DaysInMonth(y, dm);
            var day = Math.Min(Math.Max(dd, 1), dim);
            var r = rows[i];
            rows[i] = r with { Deadline = new DateTime(y, dm, day) };
        }
        return rows;
    }
}

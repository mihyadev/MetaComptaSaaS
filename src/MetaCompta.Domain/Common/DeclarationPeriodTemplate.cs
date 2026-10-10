namespace MetaCompta.Domain.Common;

/// <summary>
/// Declaration-period template: one frequency → all periods for a year.
/// Mensuel = 12, Trimestriel = 4, Annuel = 1, Périodique = user-defined.
/// Init / Create pick the current slice — they do not redefine each month separately.
/// </summary>
public static class DeclarationPeriodTemplate
{
    public sealed record Slice(
        string Code,
        string Label,
        DateTime Start,
        DateTime End,
        int Index);

    // ASCII-safe source (accents via \u) — avoids mojibake if file encoding drifts.
    private static readonly string[] MonthLabelsFr =
    {
        "", "Janvier", "F\u00e9vrier", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Ao\u00fbt", "Septembre", "Octobre", "Novembre", "D\u00e9cembre"
    };

    public static string NormalizeFrequency(string? value)
    {
        var v = (value ?? "").Trim();
        if (v.StartsWith("Trim", StringComparison.OrdinalIgnoreCase) || v.Equals("quarterly", StringComparison.OrdinalIgnoreCase))
            return "Trimestrielle";
        if (v.StartsWith("Ann", StringComparison.OrdinalIgnoreCase) || v.Equals("annually", StringComparison.OrdinalIgnoreCase) || v.Equals("yearly", StringComparison.OrdinalIgnoreCase))
            return "Annuelle";
        if (v.StartsWith("Périod", StringComparison.OrdinalIgnoreCase) || v.StartsWith("Period", StringComparison.OrdinalIgnoreCase)
            || v.StartsWith("Perso", StringComparison.OrdinalIgnoreCase) || v.Equals("custom", StringComparison.OrdinalIgnoreCase))
            return "Periodique";
        if (v.StartsWith("Ponc", StringComparison.OrdinalIgnoreCase))
            return "Ponctuelle";
        return "Mensuelle";
    }

    /// <summary>Generate all periods for a calendar year from frequency.</summary>
    public static IReadOnlyList<Slice> GenerateForYear(
        int year,
        string? frequency,
        IReadOnlyList<(DateTime Start, DateTime End, string? Label)>? customPeriods = null)
    {
        var freq = NormalizeFrequency(frequency);
        return freq switch
        {
            "Trimestrielle" => GenerateQuarterly(year),
            "Annuelle" => GenerateAnnual(year),
            "Periodique" => GeneratePeriodic(year, customPeriods),
            "Ponctuelle" => GenerateMonthly(year), // same calendar grid; create-on-demand
            _ => GenerateMonthly(year)
        };
    }

    /// <summary>How many periods this frequency yields for a full year (custom = count of slices).</summary>
    public static int ExpectedCount(string? frequency, int customCount = 0)
    {
        var freq = NormalizeFrequency(frequency);
        return freq switch
        {
            "Trimestrielle" => 4,
            "Annuelle" => 1,
            "Periodique" => Math.Max(0, customCount),
            _ => 12
        };
    }

    public static IReadOnlyList<Slice> GenerateMonthly(int year)
    {
        var list = new List<Slice>(12);
        for (var m = 1; m <= 12; m++)
        {
            var start = new DateTime(year, m, 1);
            var end = new DateTime(year, m, DateTime.DaysInMonth(year, m));
            list.Add(new Slice($"{year}-{m:D2}", MonthLabelsFr[m], start, end, m));
        }
        return list;
    }

    public static IReadOnlyList<Slice> GenerateQuarterly(int year)
    {
        var list = new List<Slice>(4);
        for (var q = 1; q <= 4; q++)
        {
            var startMonth = (q - 1) * 3 + 1;
            var start = new DateTime(year, startMonth, 1);
            var end = start.AddMonths(3).AddDays(-1);
            var shortM = new[] { "", "Jan", "F\u00e9v", "Mar", "Avr", "Mai", "Juin", "Juil", "Ao\u00fbt", "Sep", "Oct", "Nov", "D\u00e9c" };
            list.Add(new Slice($"{year}-Q{q}", $"T{q} ({shortM[startMonth]}-{shortM[end.Month]})", start, end, q));
        }
        return list;
    }

    public static IReadOnlyList<Slice> GenerateAnnual(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31);
        return new[] { new Slice(year.ToString(), "Année", start, end, 1) };
    }

    public static IReadOnlyList<Slice> GeneratePeriodic(
        int year,
        IReadOnlyList<(DateTime Start, DateTime End, string? Label)>? customPeriods)
    {
        if (customPeriods == null || customPeriods.Count == 0)
            return Array.Empty<Slice>();

        var list = new List<Slice>(customPeriods.Count);
        for (var i = 0; i < customPeriods.Count; i++)
        {
            var (start, end, label) = customPeriods[i];
            // Patterns are year-agnostic (cron-like): rematerialize onto the target year
            var (s, e) = MaterializeMonthDayPattern(start.Month, start.Day, end.Month, end.Day, year);
            var code = !string.IsNullOrWhiteSpace(label)
                ? label!.Trim()
                : $"{year}-P{i + 1}";
            list.Add(new Slice(code, string.IsNullOrWhiteSpace(label) ? $"Période {i + 1}" : label!.Trim(), s, e, i + 1));
        }
        return list;
    }

    /// <summary>
    /// Year-agnostic period pattern (like cron): JJ/MM/YYYY where YYYY is filled at runtime.
    /// If end month/day is before start, end falls in the next calendar year.
    /// </summary>
    public static (DateTime Start, DateTime End) MaterializeMonthDayPattern(
        int startMonth, int startDay, int endMonth, int endDay, int year)
    {
        startMonth = Math.Clamp(startMonth, 1, 12);
        endMonth = Math.Clamp(endMonth, 1, 12);
        startDay = Math.Clamp(startDay, 1, DateTime.DaysInMonth(year, startMonth));
        var start = new DateTime(year, startMonth, startDay);

        var endYear = year;
        if (endMonth < startMonth || (endMonth == startMonth && endDay < startDay))
            endYear = year + 1;

        endDay = Math.Clamp(endDay, 1, DateTime.DaysInMonth(endYear, endMonth));
        var end = new DateTime(endYear, endMonth, endDay);
        return (start, end);
    }

    /// <summary>
    /// Parse template date: "MM-DD", "yyyy-MM-dd", or "dd/MM/yyyy" → month/day (year ignored).
    /// </summary>
    public static bool TryParseMonthDay(string? raw, out int month, out int day)
    {
        month = 0; day = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var v = raw.Trim();

        if (DateTime.TryParseExact(v, new[] { "MM-dd", "M-d" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var md))
        {
            month = md.Month; day = md.Day; return true;
        }

        if (DateTime.TryParseExact(v, new[] { "yyyy-MM-dd", "yyyy-M-d", "dd/MM/yyyy", "d/M/yyyy" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var full))
        {
            month = full.Month; day = full.Day; return true;
        }

        // Numeric month/day fields stored separately as "01-31"
        var parts = v.Split('-', '/', '.');
        if (parts.Length == 2
            && int.TryParse(parts[0], out var a) && int.TryParse(parts[1], out var b)
            && a is >= 1 and <= 12 && b is >= 1 and <= 31)
        {
            month = a; day = b; return true;
        }
        if (parts.Length == 3
            && int.TryParse(parts[1], out var m) && int.TryParse(parts[2], out var d)
            && m is >= 1 and <= 12 && d is >= 1 and <= 31)
        {
            // yyyy-MM-dd style already handled; dd/MM/yyyy leftover
            month = m; day = d; return true;
        }

        return false;
    }

    /// <summary>Materialize all pattern rows onto year-1 / year / year+1 and pick the Init slice.</summary>
    public static Slice? ResolvePatternTableAsOf(
        DateTime asOf,
        IReadOnlyList<(int StartMonth, int StartDay, int EndMonth, int EndDay, string? Label)> patterns,
        bool previousClosed = true)
    {
        if (patterns == null || patterns.Count == 0) return null;
        asOf = asOf.Date;
        var candidates = new List<Slice>();
        foreach (var year in new[] { asOf.Year - 1, asOf.Year, asOf.Year + 1 })
        {
            for (var i = 0; i < patterns.Count; i++)
            {
                var p = patterns[i];
                var (start, end) = MaterializeMonthDayPattern(p.StartMonth, p.StartDay, p.EndMonth, p.EndDay, year);
                var label = string.IsNullOrWhiteSpace(p.Label) ? $"Période {i + 1}" : p.Label!.Trim();
                candidates.Add(new Slice($"{start:yyyy-MM-dd}", label, start, end, i + 1));
            }
        }

        if (previousClosed)
        {
            var closed = candidates.Where(c => c.End < asOf).OrderByDescending(c => c.End).FirstOrDefault();
            if (closed != null) return closed;
        }

        return candidates.Where(c => c.Start <= asOf && asOf <= c.End).OrderByDescending(c => c.Start).FirstOrDefault()
               ?? candidates.OrderBy(c => c.Start).FirstOrDefault();
    }

    /// <summary>
    /// Pick the period slice for Init/Create as-of a reference date.
    /// previousClosed: open the last closed period (typical for CNSS/TVA filing).
    /// </summary>
    public static Slice? ResolveAsOf(
        DateTime asOf,
        string? frequency,
        bool previousClosed = true,
        int? anneeFiscale = null,
        IReadOnlyList<(DateTime Start, DateTime End, string? Label)>? customPeriods = null)
    {
        var freq = NormalizeFrequency(frequency);
        asOf = asOf.Date;

        if (freq == "Annuelle")
        {
            var year = anneeFiscale ?? asOf.Year - (asOf.Month <= 3 ? 1 : 0);
            return GenerateAnnual(year)[0];
        }

        if (freq == "Periodique")
        {
            var slices = GeneratePeriodic(asOf.Year, customPeriods);
            if (slices.Count == 0) return null;
            var hit = slices.LastOrDefault(s => s.Start <= asOf && asOf <= s.End)
                      ?? slices.LastOrDefault(s => s.End < asOf)
                      ?? slices[0];
            return hit;
        }

        if (freq == "Trimestrielle")
        {
            var q = (asOf.Month - 1) / 3 + 1;
            var year = asOf.Year;
            if (previousClosed)
            {
                q -= 1;
                if (q < 1) { q = 4; year -= 1; }
            }
            return GenerateQuarterly(year).First(s => s.Index == q);
        }

        // Mensuelle / Ponctuelle
        var mStart = new DateTime(asOf.Year, asOf.Month, 1);
        if (previousClosed) mStart = mStart.AddMonths(-1);
        var slicesM = GenerateMonthly(mStart.Year);
        return slicesM.First(s => s.Start.Month == mStart.Month);
    }
}

namespace MetaCompta.Domain.Common;

/// <summary>
/// Moroccan fiscal/social calendar: période d'ouverture / clôture + échéance de dépôt
/// selon le type de déclaration et la périodicité (mensuel / trimestriel / annuel).
/// </summary>
public static class MoroccoDeclarationCalendar
{
    public sealed record Window(
        DateTime PeriodStart,
        DateTime PeriodEnd,
        DateTime FilingDeadline,
        /// <summary>Date à laquelle le cabinet ouvre typiquement le dossier (après clôture de période).</summary>
        DateTime ProcessOpenDate,
        string PeriodeLabel,
        string Frequency);

    /// <summary>
    /// Builds the fiscal window for a declaration type as of a reference date
    /// (usually "now" when creating the previous period's dossier).
    /// </summary>
    public static Window ResolveForCreate(
        string declarationType,
        DateTime? asOfUtc = null,
        int? anneeFiscale = null,
        string? periodeOverride = null,
        string? clientTypeTva = null,
        string? frequencyOverride = null)
    {
        var asOf = (asOfUtc ?? DateTime.UtcNow).Date;
        var type = (declarationType ?? "").Trim().ToUpperInvariant();
        var frequency = ResolveFrequency(type, clientTypeTva, frequencyOverride);

        // Explicit period string wins when provided
        if (!string.IsNullOrWhiteSpace(periodeOverride))
        {
            var fromLabel = TryParsePeriodeLabel(periodeOverride!.Trim(), type, frequency);
            if (fromLabel != null) return fromLabel;
        }

        // After period end we open the dossier for the *previous* closed period (TVA/CNSS…).
        // Acomptes are worked during the current quarter; RAS are ponctuels on current month.
        var previousClosed = type is not ("ACOMPTES" or "RAS_FONC" or "RAS_DIV");

        // Declaration-period template: frequency → calendar slice (12 / 4 / 1)
        var slice = DeclarationPeriodTemplate.ResolveAsOf(
            asOf,
            frequency,
            previousClosed,
            anneeFiscale);

        if (slice != null)
        {
            return new Window(
                slice.Start,
                slice.End,
                ComputeFilingDeadline(type, slice.Start, slice.End, frequency),
                GetProcessOpenDate(slice.End),
                slice.Code,
                frequency == "Ponctuelle" ? "Ponctuelle" : DeclarationPeriodTemplate.NormalizeFrequency(frequency));
        }

        return frequency switch
        {
            "Trimestrielle" => BuildQuarterly(type, asOf, previous: previousClosed),
            "Annuelle" => BuildAnnual(type, anneeFiscale ?? asOf.Year - (asOf.Month <= 3 ? 1 : 0)),
            "Ponctuelle" => BuildMonthly(type, asOf, previous: false),
            _ => BuildMonthly(type, asOf, previous: previousClosed)
        };
    }

    /// <summary>
    /// Filing deadline (échéance dépôt) for an already-known period — used by workflow CalendrierMaroc.
    /// </summary>
    public static DateTime GetFilingDeadline(string declarationType, DateTime periodStart, DateTime periodEnd, string? frequency = null)
    {
        var type = (declarationType ?? "").Trim().ToUpperInvariant();
        var freq = frequency ?? InferFrequencyFromSpan(periodStart, periodEnd, type);
        return ComputeFilingDeadline(type, periodStart.Date, periodEnd.Date, freq);
    }

    /// <summary>Process open date: typically the day after period end (1er du mois / trimestre suivant).</summary>
    public static DateTime GetProcessOpenDate(DateTime periodEnd)
        => periodEnd.Date.AddDays(1);

    /// <summary>
    /// Types whose cabinet work happens after period close (filing window next month).
    /// Ex. CNSS juillet → dépôt 1–10 août ; TVA → jusqu'au 20 du mois suivant.
    /// </summary>
    public static bool UsesPostPeriodFilingWindow(string? declarationType)
    {
        var t = (declarationType ?? "").Trim().ToUpperInvariant();
        return t is "CNSS" or "IR_SAL" or "TVA";
    }

    /// <summary>
    /// Legacy Maroc dépôt-window end (CNSS 10, TVA 20…). Prefer Init LifecycleEndMode=PeriodEnd
    /// which uses Fin/échéance from Modèle de périodes instead.
    /// </summary>
    public static DateTime GetLifecycleEndDate(string? declarationType, DateTime periodEnd, DateTime filingDeadline)
        => UsesPostPeriodFilingWindow(declarationType) ? filingDeadline.Date : periodEnd.Date;

    /// <summary>Smart defaults for Init window (jour début / jour fin) by declaration type.</summary>
    public sealed record FilingWindowDefaults(int StartDay, int EndDay, int MonthOffsetAfterPeriod);

    public static FilingWindowDefaults GetDefaultFilingWindow(string? declarationType)
    {
        var t = (declarationType ?? "").Trim().ToUpperInvariant();
        return t switch
        {
            "CNSS" => new(1, 10, 1),      // juillet → 1–10 août
            "IR_SAL" => new(1, 15, 1),
            "TVA" => new(1, 20, 1),
            "DTS" => new(1, 28, 2),       // fin fév. N+1 approx.
            "IS" or "REG_IS" or "CM" or "DRF" or "LIASSEFISCALE" => new(1, 31, 3), // mars N+1
            "IR" => new(1, 30, 4),        // avril N+1
            "ACOMPTES" => new(1, 31, 0),  // fin de trimestre
            _ => UsesPostPeriodFilingWindow(t) ? new(1, 20, 1) : new(1, 31, 0)
        };
    }

    public static DateTime DayInMonthAfter(DateTime periodEnd, int monthsAfter, int day)
    {
        var m = periodEnd.Date.AddMonths(monthsAfter);
        var dim = DateTime.DaysInMonth(m.Year, m.Month);
        return new DateTime(m.Year, m.Month, Math.Min(Math.Max(day, 1), dim));
    }

    /// <summary>Applies user/Init window end day (échéance) relative to period end.</summary>
    public static DateTime ApplyWindowEnd(DateTime periodEnd, int endDay, int monthOffset)
        => DayInMonthAfter(periodEnd, monthOffset, endDay);

    public static DateTime ApplyWindowStart(DateTime periodEnd, int startDay, int monthOffset)
        => DayInMonthAfter(periodEnd, monthOffset, startDay);

    /// <summary>
    /// DateLimite from Init: after period end → +années +mois, then jour (clamped to month length).
    /// Offsets default by fréquence when not provided (Mensuel +1m, Trimestriel +3m, Annuel +1y).
    /// Legacy annual absolute month (deadlineMonth) still supported.
    /// </summary>
    public static DateTime ApplyInitDeadline(
        string? frequency,
        DateTime periodEnd,
        int? windowEndDay,
        int? windowMonthOffset,
        int? deadlineMonth,
        int? deadlineYearOffset,
        string? declarationType = null)
    {
        var freq = NormalizeFrequency(frequency);
        var day = Math.Clamp(windowEndDay ?? 1, 1, 31);

        // Legacy: absolute calendar month in year N / N+1
        if (freq == "Annuelle" && deadlineMonth is >= 1 and <= 12)
        {
            var year = periodEnd.Year + (deadlineYearOffset ?? 1);
            var dim = DateTime.DaysInMonth(year, deadlineMonth.Value);
            return new DateTime(year, deadlineMonth.Value, Math.Min(day, dim));
        }

        // Generic frequency defaults — not tied to declaration type
        var (defaultYears, defaultMonths) = freq switch
        {
            "Trimestrielle" => (0, 3),
            "Annuelle" => (1, 0),
            _ => (0, 1) // Mensuelle
        };

        var years = Math.Max(0, deadlineYearOffset ?? defaultYears);
        var months = Math.Max(0, windowMonthOffset ?? defaultMonths);
        return ApplyWindowEnd(periodEnd, day, years * 12 + months);
    }

    /// <summary>Default Init lifecycle offsets by fréquence only (generic).</summary>
    public static (int Years, int Months, int StartDay, int EndDay) GetFrequencyLifecycleDefaults(string? frequency)
    {
        var freq = NormalizeFrequency(frequency);
        return freq switch
        {
            "Trimestrielle" => (0, 3, 1, 1),
            "Annuelle" => (1, 0, 1, 1),
            "Periodique" => (0, 0, 1, 1),
            _ => (0, 1, 1, 1) // Mensuelle
        };
    }

    public static string ResolveFrequency(string declarationType, string? clientTypeTva = null, string? frequencyOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(frequencyOverride))
            return NormalizeFrequency(frequencyOverride);

        var type = (declarationType ?? "").Trim().ToUpperInvariant();
        if (type == "TVA" && !string.IsNullOrWhiteSpace(clientTypeTva))
            return NormalizeFrequency(clientTypeTva);

        var catalog = DeclarationServiceCatalog.Get(declarationType);
        if (catalog != null)
            return NormalizeFrequency(catalog.Frequency);

        return "Mensuelle";
    }

    public static string InferFrequencyFromSpan(DateTime periodStart, DateTime periodEnd, string? declarationType = null)
    {
        var days = (periodEnd.Date - periodStart.Date).TotalDays;
        if (days > 300) return "Annuelle";
        if (days > 80) return "Trimestrielle";
        if (!string.IsNullOrWhiteSpace(declarationType))
        {
            var catalog = DeclarationServiceCatalog.Get(declarationType);
            if (catalog != null) return NormalizeFrequency(catalog.Frequency);
        }
        return "Mensuelle";
    }

    private static string NormalizeFrequency(string? value)
        => DeclarationPeriodTemplate.NormalizeFrequency(value);

    private static Window BuildMonthly(string type, DateTime asOf, bool previous)
    {
        var start = new DateTime(asOf.Year, asOf.Month, 1);
        if (previous) start = start.AddMonths(-1);
        var end = start.AddMonths(1).AddDays(-1);
        var freq = "Mensuelle";
        return new Window(
            start,
            end,
            ComputeFilingDeadline(type, start, end, freq),
            GetProcessOpenDate(end),
            start.ToString("yyyy-MM"),
            freq);
    }

    private static Window BuildQuarterly(string type, DateTime asOf, bool previous)
    {
        var q = (asOf.Month - 1) / 3 + 1;
        var year = asOf.Year;
        if (previous)
        {
            q -= 1;
            if (q < 1) { q = 4; year -= 1; }
        }
        var start = new DateTime(year, (q - 1) * 3 + 1, 1);
        var end = start.AddMonths(3).AddDays(-1);
        var freq = "Trimestrielle";
        return new Window(
            start,
            end,
            ComputeFilingDeadline(type, start, end, freq),
            GetProcessOpenDate(end),
            $"{year}-Q{q}",
            freq);
    }

    private static Window BuildAnnual(string type, int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31);
        var freq = "Annuelle";
        return new Window(
            start,
            end,
            ComputeFilingDeadline(type, start, end, freq),
            GetProcessOpenDate(end),
            year.ToString(),
            freq);
    }

    /// <summary>Official Moroccan deposit deadlines (cabinet defaults).</summary>
    private static DateTime ComputeFilingDeadline(string type, DateTime periodStart, DateTime periodEnd, string frequency)
    {
        type = type.ToUpperInvariant();

        // Monthly social / withholding — early next month
        if (type is "CNSS")
            return DayOfMonthAfter(periodEnd, monthsAfter: 1, day: 10);

        if (type is "IR_SAL")
            return DayOfMonthAfter(periodEnd, monthsAfter: 1, day: 15);

        // TVA — mensuelle or trimestrielle: typically 20th of month following period end
        if (type == "TVA")
            return DayOfMonthAfter(periodEnd, monthsAfter: 1, day: 20);

        // Acomptes IS — échéance = fin du trimestre (31/03, 30/06, 30/09, 31/12)
        if (type == "ACOMPTES")
            return periodEnd;

        // Annual corporate / income tax packages
        if (type is "IS" or "REG_IS" or "CM" or "DRF" or "LIASSEFISCALE")
            return new DateTime(periodEnd.Year + 1, 3, 31);

        if (type is "IR")
            return new DateTime(periodEnd.Year + 1, 4, 30);

        if (type is "DTS")
            return new DateTime(periodEnd.Year + 1, 2, 28); // état 9421 — défaut cabinet (fin février N+1)

        // RAS ponctuel — 30 jours après fin de période
        if (type is "RAS_FONC" or "RAS_DIV")
            return periodEnd.AddDays(30);

        // Generic by frequency
        return frequency switch
        {
            "Annuelle" => new DateTime(periodEnd.Year + 1, 3, 31),
            "Trimestrielle" => DayOfMonthAfter(periodEnd, monthsAfter: 1, day: 20),
            _ => DayOfMonthAfter(periodEnd, monthsAfter: 1, day: 20)
        };
    }

    private static DateTime DayOfMonthAfter(DateTime periodEnd, int monthsAfter, int day)
        => DayInMonthAfter(periodEnd, monthsAfter, day);

    private static Window? TryParsePeriodeLabel(string periode, string type, string frequency)
    {
        // yyyy-MM
        if (periode.Length == 7 && periode[4] == '-'
            && int.TryParse(periode.AsSpan(0, 4), out var ymY)
            && int.TryParse(periode.AsSpan(5, 2), out var ymM)
            && ymM is >= 1 and <= 12)
        {
            var start = new DateTime(ymY, ymM, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return new Window(start, end, ComputeFilingDeadline(type, start, end, "Mensuelle"),
                GetProcessOpenDate(end), periode, "Mensuelle");
        }

        // yyyy-Qn
        if (periode.Contains('Q', StringComparison.OrdinalIgnoreCase))
        {
            var parts = periode.ToUpperInvariant().Split('Q');
            if (parts.Length == 2
                && int.TryParse(parts[0].TrimEnd('-', ' '), out var qY)
                && int.TryParse(parts[1].Trim(), out var q)
                && q is >= 1 and <= 4)
            {
                var start = new DateTime(qY, (q - 1) * 3 + 1, 1);
                var end = start.AddMonths(3).AddDays(-1);
                return new Window(start, end, ComputeFilingDeadline(type, start, end, "Trimestrielle"),
                    GetProcessOpenDate(end), $"{qY}-Q{q}", "Trimestrielle");
            }
        }

        // yyyy
        if (periode.Length == 4 && int.TryParse(periode, out var year))
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);
            return new Window(start, end, ComputeFilingDeadline(type, start, end, "Annuelle"),
                GetProcessOpenDate(end), year.ToString(), "Annuelle");
        }

        return null;
    }
}

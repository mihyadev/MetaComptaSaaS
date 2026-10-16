/**
 * Shared cron / Init period-table builder for Workflow Builder + Client Personnaliser.
 * Init source of truth: editable periods[] (label, start, end, due) — due = \u00e9ch\u00e9ance per row.
 */
(function (global) {
    'use strict';

    var dayNames = ['Di', 'Lu', 'Ma', 'Me', 'Je', 'Ve', 'Sa'];
    // Locale-driven names (never hardcode accented literals \u2192 avoids F\u00c3\u00a9vrier / Ao\u00c3\u00bbt mojibake).
    var monthNames = [''];
    var monthNamesShort = [''];
    (function buildMonthNames() {
        try {
            var longFmt = new Intl.DateTimeFormat('fr-FR', { month: 'long' });
            var shortFmt = new Intl.DateTimeFormat('fr-FR', { month: 'short' });
            for (var m = 1; m <= 12; m++) {
                var d = new Date(Date.UTC(2000, m - 1, 1));
                var longName = longFmt.format(d);
                var shortName = shortFmt.format(d).replace(/\.$/, '');
                monthNames[m] = longName.charAt(0).toUpperCase() + longName.slice(1);
                monthNamesShort[m] = shortName.charAt(0).toUpperCase() + shortName.slice(1);
            }
        } catch (e) {
            // ASCII fallback if Intl unavailable
            monthNames = ['', 'Janvier', 'Fevrier', 'Mars', 'Avril', 'Mai', 'Juin', 'Juillet', 'Aout', 'Septembre', 'Octobre', 'Novembre', 'Decembre'];
            monthNamesShort = ['', 'Jan', 'Fev', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Aout', 'Sep', 'Oct', 'Nov', 'Dec'];
        }
    })();

    function pad(n) { return String(n).padStart(2, '0'); }
    function pad2(n) { return pad(n); }

    function closest(el, sel) {
        return el && el.closest ? el.closest(sel) : null;
    }

    /** Year-agnostic pattern MM-DD (YYYY filled at runtime like cron). */
    function md(m, d) { return pad2(m) + '-' + pad2(d); }

    function daysInMonth(month) {
        return [0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][Math.min(12, Math.max(1, month))] || 31;
    }

    /** Non-leap EOM pattern (Feb=28) — leap day clamped at runtime materialization. */
    function daysInMonthPattern(month) {
        return [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][Math.min(12, Math.max(1, month))] || 31;
    }

    /** Due = last day of calendar month after period-end month (Dec \u2192 Jan). */
    function eomNextMonth(endMonth) {
        var nm = endMonth >= 12 ? 1 : (endMonth + 1);
        return { m: nm, d: daysInMonthPattern(nm) };
    }

    /** Parse "MM-DD" or legacy "yyyy-MM-dd" / "dd/MM/yyyy" \u2192 { m, d } (year ignored). */
    function parseMd(s) {
        if (!s) return null;
        var v = String(s).trim();
        var p = v.split(/[-/.]/);
        if (p.length === 2) {
            var m2 = parseInt(p[0], 10), d2 = parseInt(p[1], 10);
            if (m2 >= 1 && m2 <= 12 && d2 >= 1 && d2 <= 31) return { m: m2, d: Math.min(d2, daysInMonth(m2)) };
            return null;
        }
        if (p.length >= 3) {
            // yyyy-MM-dd
            if (p[0].length === 4) {
                var m3 = parseInt(p[1], 10), d3 = parseInt(p[2], 10);
                if (m3 >= 1 && m3 <= 12 && d3 >= 1 && d3 <= 31) return { m: m3, d: Math.min(d3, daysInMonth(m3)) };
            }
            // dd/MM/yyyy
            var d4 = parseInt(p[0], 10), m4 = parseInt(p[1], 10);
            if (m4 >= 1 && m4 <= 12 && d4 >= 1 && d4 <= 31) return { m: m4, d: Math.min(d4, daysInMonth(m4)) };
        }
        return null;
    }

    /** Repair classic UTF-8\u2192Latin1 mojibake (F\u00c3\u00a9vrier \u2192 F\u00e9vrier). */
    function fixMojibake(s) {
        if (!s || s.indexOf('\u00c3') < 0) return s || '';
        try {
            var bytes = new Uint8Array(s.length);
            for (var i = 0; i < s.length; i++) bytes[i] = s.charCodeAt(i) & 0xff;
            return new TextDecoder('utf-8').decode(bytes);
        } catch (e) { return s; }
    }

    function normalizePeriod(p) {
        var s = parseMd(p && (p.start || p.startMd));
        var e = parseMd(p && (p.end || p.endMd));
        var op = parseMd(p && (p.open || p.openMd));
        var du = parseMd(p && (p.due || p.dueMd || p.echeance));
        if (p && p.startMonth && p.startDay) s = { m: +p.startMonth, d: +p.startDay };
        if (p && p.endMonth && p.endDay) e = { m: +p.endMonth, d: +p.endDay };
        if (p && p.openMonth && p.openDay) op = { m: +p.openMonth, d: +p.openDay };
        if (p && p.dueMonth && p.dueDay) du = { m: +p.dueMonth, d: +p.dueDay };
        // Default: open/due = filing month after activity end
        if (!du && e && e.m) du = eomNextMonth(e.m);
        if (!op && du) op = { m: du.m, d: 1 };
        var label = fixMojibake((p && p.label) || '');
        if (s && s.m >= 1 && s.m <= 12 && /\u00c3./.test(label)) {
            label = monthNames[s.m] || label;
        }
        var startY = (p && p.startYearOffset != null) ? parseInt(p.startYearOffset, 10) : null;
        var openY = (p && p.openYearOffset != null) ? parseInt(p.openYearOffset, 10) : null;
        var dueY = (p && p.dueYearOffset != null) ? parseInt(p.dueYearOffset, 10) : null;
        if (isNaN(startY)) startY = null;
        if (isNaN(openY)) openY = null;
        if (isNaN(dueY)) dueY = null;
        // Infer filing year when not stored (Dec activity → Jan filing = +1)
        if (openY == null && s && op) openY = (op.m < s.m) ? 1 : 0;
        if (dueY == null && s && du) dueY = (du.m < s.m) ? 1 : 0;
        if (startY == null) startY = 0;
        return {
            label: label,
            start: s ? md(s.m, s.d) : '',
            end: e ? md(e.m, e.d) : '',
            open: op ? md(op.m, op.d) : '',
            due: du ? md(du.m, du.d) : '',
            startMonth: s ? s.m : null,
            startDay: s ? s.d : null,
            endMonth: e ? e.m : null,
            endDay: e ? e.d : null,
            openMonth: op ? op.m : null,
            openDay: op ? op.d : null,
            dueMonth: du ? du.m : null,
            dueDay: du ? du.d : null,
            startYearOffset: startY,
            openYearOffset: openY != null ? openY : 0,
            dueYearOffset: dueY != null ? dueY : 0
        };
    }

    var monthShort = { 1: 'Jan', 2: 'F\u00e9v', 3: 'Mar', 4: 'Avr', 5: 'Mai', 6: 'Juin',
        7: 'Juil', 8: 'Ao\u00fbt', 9: 'Sep', 10: 'Oct', 11: 'Nov', 12: 'D\u00e9c' };

    function monthOptionsHtml(selected) {
        var html = '';
        for (var i = 1; i <= 12; i++) {
            html += '<option value="' + i + '"' + (i === selected ? ' selected' : '') + '>'
                + monthShort[i] + '</option>';
        }
        return html;
    }

    /** Compact day/month editor (periodic / advanced only). */
    function mdEditorHtml(prefix, m, d, label, yearHint) {
        m = m || 1; d = d || 1;
        var badge = yearHint === 'N+1'
            ? '<span class="pt-yyyy pt-yyyy-next" title="Ann\u00e9e suivante">N+1</span>'
            : '';
        return '<div class="pt-field' + (prefix === 'due' ? ' pt-due' : '') + '"'
            + (label ? ' title="' + label + '"' : '') + '>'
            + '<div class="pt-md-row">'
            + '<input type="number" class="pt-' + prefix + '-d form-control form-control-sm" min="1" max="31" value="' + d + '" title="Jour" aria-label="Jour" />'
            + '<span class="pt-sep">/</span>'
            + '<select class="pt-' + prefix + '-m form-select form-select-sm" title="Mois" aria-label="Mois">' + monthOptionsHtml(m) + '</select>'
            + badge
            + '</div></div>';
    }

    function dimAbs(year, month) {
        return new Date(year, month, 0).getDate();
    }

    /** Year token for template: YYYY / YYYY-1 / YYYY+1. */
    function yearToken(offset) {
        if (offset === -1) return 'YYYY-1';
        if (offset === 0) return 'YYYY';
        if (offset === 1) return 'YYYY+1';
        return offset > 0 ? ('YYYY+' + offset) : ('YYYY' + offset);
    }

    function resolveYearToken(token, yyyy) {
        if (yyyy == null || yyyy === '' || token == null) return token;
        var y = parseInt(yyyy, 10);
        if (!(y >= 2000 && y <= 2100)) return token;
        if (token === 'YYYY-1') return String(y - 1);
        if (token === 'YYYY') return String(y);
        if (token === 'YYYY+1') return String(y + 1);
        return String(token).replace(/YYYY/g, String(y));
    }

    /** Year offset for filing month vs activity month (Dec\to Jan = +1). */
    function filingYearOffset(activityMonth, filingMonth) {
        return (filingMonth < activityMonth) ? 1 : 0;
    }

    function formatTokenDate(day, month, yearOff, previewYear) {
        var tok = yearToken(yearOff);
        if (previewYear >= 2000 && previewYear <= 2100) {
            return fmtDmY(day, month, previewYear + yearOff);
        }
        return pad2(day) + '/' + pad2(month) + '/' + tok;
    }

    /**
     * Build display model from editable pattern rows (YYYY tokens, optional aperçu).
     */
    function materializeMoroccoCalendar(previewYear, freq, patternRows) {
        var preview = parseInt(previewYear, 10);
        var usePreview = preview >= 2000 && preview <= 2100;
        var list = (patternRows && patternRows.length) ? patternRows.map(normalizePeriod) : generatePeriodRows(freq);
        return list.map(function (p, i) {
            var actOff = (freq === 'quarterly' && p.startMonth >= 10 && p.dueMonth === 1) ? -1 : 0;
            var fileOff = filingYearOffset(p.endMonth || p.startMonth || 1, p.dueMonth || 1);
            if (freq === 'quarterly' && p.startMonth >= 10 && p.dueMonth === 1) { actOff = -1; fileOff = 0; }
            var actLabel = (p.label || '').replace(/\s*\(d[\u00e9e]p[\u00f4o]t[^)]*\)/i, '').trim();
            if (!actLabel) actLabel = monthNames[p.startMonth] || ('P\u00e9riode ' + (i + 1));
            if (usePreview) actLabel = actLabel + ' ' + (preview + actOff);
            else actLabel = actLabel + ' ' + yearToken(actOff);
            return {
                index: i + 1,
                displayOnly: false,
                activityLabel: actLabel,
                openStr: formatTokenDate(p.openDay || 1, p.openMonth || 1, fileOff, usePreview ? preview : null),
                deadlineStr: formatTokenDate(p.dueDay || daysInMonthPattern(p.dueMonth || 1), p.dueMonth || 1, fileOff, usePreview ? preview : null),
                label: p.label,
                startMonth: p.startMonth, startDay: p.startDay || 1,
                endMonth: p.endMonth, endDay: p.endDay || daysInMonthPattern(p.endMonth || 1),
                openMonth: p.openMonth, openDay: p.openDay,
                dueMonth: p.dueMonth, dueDay: p.dueDay,
                fileYearOff: fileOff,
                actYearOff: actOff
            };
        });
    }

    function periodFrequencyValue(freq) {
        if (freq === 'monthly') return 'Mensuelle';
        if (freq === 'quarterly') return 'Trimestrielle';
        if (freq === 'annually') return 'Annuelle';
        if (freq === 'periodic') return 'Periodique';
        return null;
    }

    function freqFromPeriodTemplate(v) {
        v = (v || '').toLowerCase();
        if (v.indexOf('trim') >= 0 || v === 'quarterly') return 'quarterly';
        if (v.indexOf('ann') >= 0 || v === 'annually') return 'annually';
        if (v.indexOf('period') >= 0 || v === 'periodic') return 'periodic';
        return 'monthly';
    }

    function tplTitle(freq) {
        if (freq === 'quarterly') return 'TVA trimestrielle Maroc \u2014 fen\u00eatre DGI (trimestre N \u2192 mois suivant)';
        if (freq === 'annually') return 'Annuel \u2014 1 periode';
        if (freq === 'periodic') return 'Periodique \u2014 periodes manuelles';
        return 'TVA mensuelle Maroc \u2014 fen\u00eatre DGI (activit\u00e9 M \u2192 d\u00e9p\u00f4t M+1)';
    }

    function monthNameLower(m) {
        var n = monthNames[m] || '';
        return n ? (n.charAt(0).toLowerCase() + n.slice(1)) : '';
    }

    /** Morocco: activity Jan…Dec, open/due in next month (editable). Open=1, due=EOM. */
    function generatePeriodRows(freq) {
        var rows = [];
        if (freq === 'quarterly') {
            var qDefs = [
                { label: 'Octobre\u2013d\u00e9cembre (d\u00e9p\u00f4t janvier)', sm: 10, em: 12, ed: 31, om: 1, od: 1, dm: 1, ay: -1, fy: 0 },
                { label: 'Janvier\u2013mars (d\u00e9p\u00f4t avril)', sm: 1, em: 3, ed: 31, om: 4, od: 1, dm: 4, ay: 0, fy: 0 },
                { label: 'Avril\u2013juin (d\u00e9p\u00f4t juillet)', sm: 4, em: 6, ed: 30, om: 7, od: 1, dm: 7, ay: 0, fy: 0 },
                { label: 'Juillet\u2013septembre (d\u00e9p\u00f4t octobre)', sm: 7, em: 9, ed: 30, om: 10, od: 1, dm: 10, ay: 0, fy: 0 }
            ];
            qDefs.forEach(function (q) {
                rows.push(normalizePeriod({
                    label: q.label,
                    startMonth: q.sm, startDay: 1, endMonth: q.em, endDay: q.ed,
                    openMonth: q.om, openDay: q.od, dueMonth: q.dm, dueDay: daysInMonthPattern(q.dm),
                    startYearOffset: q.ay, openYearOffset: q.fy, dueYearOffset: q.fy
                }));
            });
            return rows;
        }
        if (freq === 'annually') {
            rows.push(normalizePeriod({
                label: 'Ann\u00e9e', startMonth: 1, startDay: 1, endMonth: 12, endDay: 31,
                openMonth: 3, openDay: 1, dueMonth: 3, dueDay: 31,
                startYearOffset: 0, openYearOffset: 0, dueYearOffset: 0
            }));
            return rows;
        }
        if (freq === 'periodic') {
            rows.push(normalizePeriod({
                label: 'Periode 1', startMonth: 1, startDay: 1, endMonth: 6, endDay: 30,
                openMonth: 7, openDay: 1, dueMonth: 7, dueDay: 31,
                startYearOffset: 0, openYearOffset: 0, dueYearOffset: 0
            }));
            return rows;
        }
        for (var activityMonth = 1; activityMonth <= 12; activityMonth++) {
            var fm = activityMonth === 12 ? 1 : (activityMonth + 1);
            var fileOff = activityMonth === 12 ? 1 : 0;
            rows.push(normalizePeriod({
                label: monthNames[activityMonth] + ' (d\u00e9p\u00f4t ' + monthNameLower(fm) + ')',
                startMonth: activityMonth, startDay: 1,
                endMonth: activityMonth, endDay: daysInMonthPattern(activityMonth),
                openMonth: fm, openDay: 1,
                dueMonth: fm, dueDay: daysInMonthPattern(fm),
                startYearOffset: 0, openYearOffset: fileOff, dueYearOffset: fileOff
            }));
        }
        return rows;
    }

    function readCfg(card) {
        var cfgInput = card && card.querySelector('.action-config-input');
        if (!cfgInput) return {};
        try { return JSON.parse(cfgInput.value || '{}') || {}; } catch (e) { return {}; }
    }

    function writeCfg(card, cfg) {
        var cfgInput = card && card.querySelector('.action-config-input');
        if (cfgInput) cfgInput.value = JSON.stringify(cfg);
    }

    function readPeriodsFromDom(card) {
        var box = card && card.querySelector('.period-template-editor');
        if (!box) return [];
        var rows = [];
        var nodes = box.querySelectorAll('.pt-card:not(.pt-display-only)');
        if (!nodes.length) nodes = box.querySelectorAll('tbody tr:not(.pt-display-only)');
        nodes.forEach(function (tr) {
            var labelEl = tr.querySelector('.pt-label');
            var label = labelEl ? (labelEl.value || labelEl.getAttribute('data-value') || '') : '';
            var sm = parseInt((tr.querySelector('.pt-start-m') || {}).value, 10) || 0;
            var sd = parseInt((tr.querySelector('.pt-start-d') || {}).value, 10) || 0;
            var em = parseInt((tr.querySelector('.pt-end-m') || {}).value, 10) || 0;
            var ed = parseInt((tr.querySelector('.pt-end-d') || {}).value, 10) || 0;
            var om = parseInt((tr.querySelector('.pt-open-m') || {}).value, 10) || 0;
            var od = parseInt((tr.querySelector('.pt-open-d') || {}).value, 10) || 0;
            var dm = parseInt((tr.querySelector('.pt-due-m') || {}).value, 10) || 0;
            var dd = parseInt((tr.querySelector('.pt-due-d') || {}).value, 10) || 0;
            var startYEl = tr.querySelector('.pt-start-y');
            var openYEl = tr.querySelector('.pt-open-y');
            var dueYEl = tr.querySelector('.pt-due-y');
            var startY = startYEl ? parseInt(startYEl.value, 10) : 0;
            var openY = openYEl ? parseInt(openYEl.value, 10) : null;
            var dueY = dueYEl ? parseInt(dueYEl.value, 10) : null;
            if (!sm && !em && !label) return;
            if (!em) em = sm;
            if (!sd) sd = 1;
            if (!ed) ed = daysInMonthPattern(em || 1);
            sd = Math.min(Math.max(1, sd), daysInMonth(sm || 1));
            ed = Math.min(Math.max(1, ed), daysInMonth(em || 1));
            if (!dm && em) {
                var auto = eomNextMonth(em);
                dm = auto.m; dd = dd || auto.d;
            }
            if (!om) om = dm;
            if (!od) od = 1;
            // Fin / échéance = always last day of selected month (28/29/30/31)
            if (dm) dd = daysInMonthPattern(dm);
            else if (dd) dd = Math.min(Math.max(1, dd), daysInMonth(dm || 1));
            if (od) od = Math.min(Math.max(1, od), daysInMonth(om || 1));
            if (isNaN(startY)) startY = 0;
            if (openY == null || isNaN(openY)) openY = (om && sm && om < sm) ? 1 : 0;
            if (dueY == null || isNaN(dueY)) dueY = (dm && sm && dm < sm) ? 1 : 0;
            if (!label && sm) label = monthNames[sm] + ' (d\u00e9p\u00f4t ' + monthNameLower(dm || om) + ')';
            rows.push(normalizePeriod({
                label: String(label).trim(),
                startMonth: sm, startDay: sd,
                endMonth: em, endDay: ed,
                openMonth: om, openDay: od,
                dueMonth: dm || null, dueDay: dd || null,
                startYearOffset: startY,
                openYearOffset: openY,
                dueYearOffset: dueY
            }));
        });
        return rows;
    }

    /** N offset select: -2 … +2 (shown next to typed YYYY). */
    function yearNOptionsHtml(selected) {
        var opts = [-2, -1, 0, 1, 2];
        var html = '';
        var sel = (selected == null || isNaN(selected)) ? 0 : selected;
        opts.forEach(function (off) {
            var label = off === 0 ? '0' : (off > 0 ? ('+' + off) : String(off));
            html += '<option value="' + off + '"' + (off === sel ? ' selected' : '') + '>'
                + label + '</option>';
        });
        return html;
    }

    /** Inline: typed YYYY + select N. */
    function yearInlineHtml(prefix, yOff, yyyyText) {
        var txt = (yyyyText && String(yyyyText).trim()) ? String(yyyyText).trim() : 'YYYY';
        return '<span class="pt-year-inline">'
            + '<input type="text" class="pt-' + prefix + '-yyyy form-control form-control-sm pt-yyyy-input" value="'
            + txt.replace(/"/g, '&quot;') + '" maxlength="8" title="Ann\u00e9e (saisir YYYY ou une ann\u00e9e)" />'
            + '<select class="pt-' + prefix + '-y form-select form-select-sm pt-nselect" title="N (offset)">'
            + yearNOptionsHtml(yOff || 0) + '</select>'
            + '</span>';
    }

    /** Simple day + month + YYYY (typed) + N (select), inline. */
    function simpleDayMonthHtml(prefix, m, d, yOff, opts) {
        opts = opts || {};
        var dayVal = d || 1;
        var dayExtra = opts.dayReadonly
            ? ' readonly tabindex="-1" title="Dernier jour du mois (28/29/30/31)"'
            : ' title="Jour"';
        var dayCls = 'pt-' + prefix + '-d form-control form-control-sm'
            + (opts.dayReadonly ? ' pt-day-eom' : '');
        return '<div class="pt-md-row pt-simple">'
            + '<input type="number" class="' + dayCls + '" min="1" max="31" value="' + dayVal + '"' + dayExtra + ' />'
            + '<span class="pt-sep">/</span>'
            + '<select class="pt-' + prefix + '-m form-select form-select-sm" title="Mois">' + monthOptionsHtml(m || 1) + '</select>'
            + yearInlineHtml(prefix, yOff, opts.yyyyText)
            + '</div>';
    }

    function renderMoroccoCalendarEditor(card, patternRows, freq) {
        var box = card && card.querySelector('.period-template-editor');
        if (!box) return;
        patternRows = (patternRows || generatePeriodRows(freq)).map(normalizePeriod);
        var html = '<div class="period-template-box pt-morocco pt-simple-ui">'
            + '<div class="pt-head">'
            + '<div class="pt-head-main">'
            + '<span>Calendrier TVA Maroc</span>'
            + '<strong>' + tplTitle(freq) + '</strong>'
            + '<div class="pt-head-hint">1) Mois + YYYY / N \u00b7 2) D\u00e9but jour/mois + YYYY / N \u00b7 3) Fin = dernier jour du mois (28/29/30/31) + YYYY / N.</div>'
            + '</div></div>'
            + '<div class="pt-scroll">'
            + '<table class="pt-cal-table pt-morocco-table"><thead><tr>'
            + '<th class="pt-col-n">#</th>'
            + '<th>Mois d\'activit\u00e9</th>'
            + '<th>D\u00e9but</th>'
            + '<th class="pt-col-due">Fin / \u00e9ch\u00e9ance</th>'
            + '</tr></thead><tbody>';
        patternRows.forEach(function (r, i) {
            var actM = r.startMonth || 1;
            var openM = r.openMonth || ((actM === 12) ? 1 : actM + 1);
            var dueM = r.dueMonth || openM;
            var dueD = daysInMonthPattern(dueM);
            var actOff = (r.startYearOffset != null) ? r.startYearOffset : 0;
            var openOff = (r.openYearOffset != null) ? r.openYearOffset : filingYearOffset(actM, openM);
            var dueOff = (r.dueYearOffset != null) ? r.dueYearOffset : filingYearOffset(actM, dueM);
            var label = monthNames[actM] + ' (d\u00e9p\u00f4t ' + monthNameLower(dueM) + ')';
            html += '<tr class="pt-card" data-idx="' + i + '">'
                + '<td class="pt-col-n"><span class="pt-badge">' + (i + 1) + '</span></td>'
                + '<td class="pt-col-act">'
                + '<div class="pt-md-row pt-simple">'
                + '<select class="pt-start-m form-select form-select-sm" title="Mois d\'activit\u00e9">' + monthOptionsHtml(actM) + '</select>'
                + yearInlineHtml('start', actOff)
                + '</div>'
                + '<input type="hidden" class="pt-label" value="' + label.replace(/"/g, '&quot;') + '" />'
                + '<input type="hidden" class="pt-start-d" value="1" />'
                + '<input type="hidden" class="pt-end-m" value="' + actM + '" />'
                + '<input type="hidden" class="pt-end-d" value="' + daysInMonthPattern(actM) + '" />'
                + '</td>'
                + '<td class="pt-col-open">' + simpleDayMonthHtml('open', openM, r.openDay != null ? r.openDay : 1, openOff) + '</td>'
                + '<td class="pt-col-due">' + simpleDayMonthHtml('due', dueM, dueD, dueOff, { dayReadonly: true }) + '</td>'
                + '</tr>';
        });
        html += '</tbody></table></div>'
            + '<div class="pt-foot">'
            + '<button type="button" class="btn btn-sm btn-outline-primary pt-add" onclick="WfCron.addPeriodRow(this.closest(\'.step-card\'))"><i class="bi bi-plus-lg"></i> Ajouter</button>'
            + '<span class="small text-muted">Saisir YYYY puis choisir N (0, +1, -1\u2026). Fin = dernier jour du mois choisi.</span>'
            + '</div></div>';
        box.innerHTML = html;

        function syncDueEom(tr) {
            if (!tr) return;
            var dueMEl = tr.querySelector('.pt-due-m');
            var dueDEl = tr.querySelector('.pt-due-d');
            var m = parseInt(dueMEl && dueMEl.value, 10) || 1;
            if (dueDEl) dueDEl.value = String(daysInMonthPattern(m));
        }

        function onPeriodMonthChange(sel) {
            var tr = closest(sel, '.pt-card');
            if (!tr) return;
            var m = parseInt(sel.value, 10) || 1;
            var endM = tr.querySelector('.pt-end-m');
            var endD = tr.querySelector('.pt-end-d');
            var lab = tr.querySelector('.pt-label');
            var dueM = parseInt((tr.querySelector('.pt-due-m') || {}).value, 10) || (m === 12 ? 1 : m + 1);
            if (endM) endM.value = String(m);
            if (endD) endD.value = String(daysInMonthPattern(m));
            if (lab) lab.value = monthNames[m] + ' (d\u00e9p\u00f4t ' + monthNameLower(dueM) + ')';
            var off = filingYearOffset(m, dueM);
            var openY = tr.querySelector('.pt-open-y');
            var dueY = tr.querySelector('.pt-due-y');
            if (openY) openY.value = String(off);
            if (dueY) dueY.value = String(off);
            syncDueEom(tr);
        }

        box.querySelectorAll('.pt-start-m').forEach(function (sel) {
            sel.addEventListener('change', function () {
                onPeriodMonthChange(sel);
                syncPeriodTable(card);
            });
        });
        box.querySelectorAll('.pt-due-m').forEach(function (sel) {
            sel.addEventListener('change', function () {
                var tr = closest(sel, '.pt-card');
                syncDueEom(tr);
                if (tr) {
                    var actM = parseInt((tr.querySelector('.pt-start-m') || {}).value, 10) || 1;
                    var dueM = parseInt(sel.value, 10) || 1;
                    var lab = tr.querySelector('.pt-label');
                    if (lab) lab.value = monthNames[actM] + ' (d\u00e9p\u00f4t ' + monthNameLower(dueM) + ')';
                    var off = filingYearOffset(actM, dueM);
                    var dueY = tr.querySelector('.pt-due-y');
                    if (dueY) dueY.value = String(off);
                }
                syncPeriodTable(card);
            });
        });
        box.querySelectorAll('.pt-open-d, .pt-open-m, .pt-open-y, .pt-open-yyyy, .pt-due-y, .pt-due-yyyy, .pt-start-y, .pt-start-yyyy').forEach(function (inp) {
            inp.addEventListener('change', function () {
                var tr = closest(inp, '.pt-card');
                if (tr) {
                    var actM = parseInt((tr.querySelector('.pt-start-m') || {}).value, 10) || 1;
                    var dueM = parseInt((tr.querySelector('.pt-due-m') || {}).value, 10) || 1;
                    var lab = tr.querySelector('.pt-label');
                    if (lab) lab.value = monthNames[actM] + ' (d\u00e9p\u00f4t ' + monthNameLower(dueM) + ')';
                }
                syncPeriodTable(card);
            });
            if (inp.classList.contains('pt-yyyy-input')) {
                inp.addEventListener('input', function () { syncPeriodTable(card); });
            }
        });
        card.querySelectorAll('.period-tpl-btn').forEach(function (b) {
            b.classList.toggle('active', b.getAttribute('data-tpl') === freq);
        });
        toggleGlobalWindowFields(card, true);
        var prev = card.querySelector('.echeance-work-preview');
        if (prev) prev.innerHTML = '';
        syncPeriodTable(card);
    }

    function renderPeriodEditor(card, rows, freq) {
        var box = card && card.querySelector('.period-template-editor');
        if (!box) return;
        freq = freq || 'monthly';
        rows = (rows || []).map(normalizePeriod);
        if (freq === 'monthly' || freq === 'quarterly') {
            renderMoroccoCalendarEditor(card, rows, freq);
            return;
        }
        var html = '<div class="period-template-box period-template-pro">'
            + '<div class="pt-head">'
            + '<span>Mod\u00e8le de p\u00e9riodes</span>'
            + '<strong>' + tplTitle(freq) + '</strong>'
            + '</div>'
            + '<div class="pt-scroll">'
            + '<table class="pt-table"><thead><tr>'
            + '<th class="pt-col-n">#</th>'
            + '<th class="pt-col-label">P\u00e9riode</th>'
            + '<th class="pt-col-md">D\u00e9but</th>'
            + '<th class="pt-col-md">Cl\u00f4ture</th>'
            + '<th class="pt-col-due">\u00c9ch\u00e9ance</th>'
            + '<th class="pt-col-del"></th>'
            + '</tr></thead><tbody>';
        rows.forEach(function (r, i) {
            var s = parseMd(r.start) || { m: 1, d: 1 };
            var e = parseMd(r.end) || { m: 1, d: 1 };
            var du = parseMd(r.due) || eomNextMonth(e.m);
            var safeLabel = String(r.label || '').replace(/"/g, '&quot;').replace(/</g, '&lt;');
            html += '<tr class="pt-card" data-idx="' + i + '">'
                + '<td class="pt-col-n"><span class="pt-badge">' + (i + 1) + '</span></td>'
                + '<td class="pt-col-label"><input type="text" class="pt-label form-control form-control-sm" value="' + safeLabel + '" /></td>'
                + '<td class="pt-col-md">' + mdEditorHtml('start', s.m, s.d, 'D\u00e9but', 'N') + '</td>'
                + '<td class="pt-col-md">' + mdEditorHtml('end', e.m, e.d, 'Cl\u00f4ture', 'N') + '</td>'
                + '<td class="pt-col-due">' + mdEditorHtml('due', du.m, du.d, '\u00c9ch\u00e9ance', 'N') + '</td>'
                + '<td class="pt-col-del"><button type="button" class="pt-del" title="Supprimer" onclick="WfCron.removePeriodRow(this)"><i class="bi bi-trash"></i></button></td>'
                + '</tr>';
        });
        html += '</tbody></table></div>'
            + '<div class="pt-foot">'
            + '<button type="button" class="btn btn-sm btn-outline-primary pt-add" onclick="WfCron.addPeriodRow(this.closest(\'.step-card\'))"><i class="bi bi-plus-lg"></i> Ajouter</button>'
            + '</div></div>';
        box.innerHTML = html;
        box.querySelectorAll('.pt-label, .pt-start-d, .pt-start-m, .pt-end-d, .pt-end-m, .pt-due-d, .pt-due-m').forEach(function (inp) {
            inp.addEventListener('change', function () { syncPeriodTable(card); });
            inp.addEventListener('input', function () {
                if (inp.classList.contains('pt-label')) return;
                syncPeriodTable(card);
            });
        });
        card.querySelectorAll('.period-tpl-btn').forEach(function (b) {
            b.classList.toggle('active', b.getAttribute('data-tpl') === freq);
        });
        toggleGlobalWindowFields(card, true);
    }

    function toggleGlobalWindowFields(card, hideWhenPerRowDue) {
        var wrap = card && card.querySelector('.init-global-window-fields');
        if (!wrap) return;
        wrap.classList.toggle('d-none', !!hideWhenPerRowDue);
    }

    function syncPeriodTable(card) {
        if (!card) return;
        var initBlock = card.querySelector('.init-freq-echeance');
        if (!initBlock || initBlock.classList.contains('d-none')) return;

        var periods = readPeriodsFromDom(card);
        var cfg = readCfg(card);
        var builder = card.querySelector('.cron-builder');
        var freq = (builder && builder.dataset.periodTpl) || freqFromPeriodTemplate(cfg.periodTemplate || cfg.periodFrequency) || 'monthly';

        // Persist year-agnostic MM-DD patterns; YYYY is a runtime parameter
        cfg.yearMode = 'YYYY';
        delete cfg.calendarYear;
        delete cfg.previewYearEnabled;
        cfg.periods = periods.map(function (p) {
            return {
                label: p.label,
                start: p.start,
                end: p.end,
                open: p.open,
                due: p.due,
                startMonth: p.startMonth,
                startDay: p.startDay,
                endMonth: p.endMonth,
                endDay: p.endDay,
                openMonth: p.openMonth,
                openDay: p.openDay,
                dueMonth: p.dueMonth,
                dueDay: p.dueDay,
                startYearOffset: p.startYearOffset != null ? p.startYearOffset : 0,
                openYearOffset: p.openYearOffset != null ? p.openYearOffset : 0,
                dueYearOffset: p.dueYearOffset != null ? p.dueYearOffset : 0
            };
        });
        cfg.periodTemplate = periodFrequencyValue(freq) || 'Mensuelle';
        cfg.periodFrequency = cfg.periodTemplate;

        // Legacy window* kept as weak fallback only when no per-row due
        var hasPerRowDue = periods.some(function (p) { return p.dueMonth && p.dueDay; });
        toggleGlobalWindowFields(card, hasPerRowDue);
        if (!hasPerRowDue) {
            var startEl = card.querySelector('.window-start-day');
            var endEl = card.querySelector('.window-end-day');
            var offEl = card.querySelector('.window-month-offset');
            var yearEl = card.querySelector('.deadline-year-offset');
            var startDay = parseInt(startEl && startEl.value, 10);
            var endDay = parseInt(endEl && endEl.value, 10);
            var monthOff = parseInt(offEl && offEl.value, 10);
            if (!(startDay >= 1 && startDay <= 31)) startDay = cfg.windowStartDay || 1;
            if (!(endDay >= 1 && endDay <= 31)) endDay = cfg.windowEndDay || 31;
            if (!(monthOff >= 0)) monthOff = cfg.windowMonthOffset != null ? cfg.windowMonthOffset : 1;
            cfg.windowStartDay = startDay;
            cfg.windowEndDay = endDay;
            cfg.windowMonthOffset = monthOff;
            cfg.deadlineYearOffset = parseInt(yearEl && yearEl.value, 10) || cfg.deadlineYearOffset || 0;
            if (startEl) startEl.value = String(cfg.windowStartDay);
            if (endEl) endEl.value = String(cfg.windowEndDay);
            if (offEl) offEl.value = String(cfg.windowMonthOffset);
        } else {
            delete cfg.windowEndDay;
            delete cfg.windowMonthOffset;
            delete cfg.windowStartDay;
        }
        delete cfg.deadlineMonth;

        writeCfg(card, cfg);
        updateEcheanceWorkPreview(card, periods, cfg);
        if (builder) {
            builder.dataset.periodTpl = freq;
            builder.dataset.freq = freq === 'periodic' ? 'monthly' : freq;
            buildCronFromPeriods(builder, periods, freq);
        }
    }

    function fmtDmY(day, month, year) {
        var dim = new Date(year, month, 0).getDate();
        return pad2(Math.min(Math.max(day, 1), dim)) + '/' + pad2(month) + '/' + year;
    }

    /** Preview merged into Morocco calendar editor — clear legacy duplicate. */
    function updateEcheanceWorkPreview(card, periods, cfg) {
        var el = card && card.querySelector('.echeance-work-preview');
        if (!el) return;
        var freq = freqFromPeriodTemplate((cfg && (cfg.periodTemplate || cfg.periodFrequency)) || '') || 'monthly';
        if (freq === 'monthly' || freq === 'quarterly') {
            el.innerHTML = '';
            return;
        }
        var year = (cfg && cfg.calendarYear) || new Date().getFullYear();
        var list = periods || [];
        if (!list.length) {
            el.innerHTML = '<span class="text-muted">Aucune p\u00e9riode</span>';
            return;
        }
        var rowsHtml = list.map(function (p, idx) {
            var e = parseMd(p.end);
            var s = parseMd(p.start);
            var du = parseMd(p.due);
            if (!du && e) du = eomNextMonth(e.m);
            if (!e || !du) return '';
            var actLabel = (p.label || ('Periode ' + (idx + 1))).replace(/</g, '&lt;');
            return '<tr><td class="pt-cal-n">' + (idx + 1) + '</td>'
                + '<td>' + actLabel + '</td>'
                + '<td class="pt-cal-due">' + fmtDmY(du.d, du.m, year) + '</td></tr>';
        }).join('');
        el.innerHTML = '<div class="pt-cal-wrap"><table class="pt-cal-table"><thead><tr>'
            + '<th>#</th><th>P\u00e9riode</th><th>\u00c9ch\u00e9ance</th></tr></thead><tbody>'
            + rowsHtml + '</tbody></table></div>';
    }

    function loadPeriodTemplate(card, freq) {
        if (!card) return;
        freq = freq || 'monthly';
        var builder = card.querySelector('.cron-builder');
        if (builder) builder.dataset.periodTpl = freq;
        var rows = generatePeriodRows(freq);
        renderPeriodEditor(card, rows, freq);
        syncPeriodTable(card);
    }

    function addPeriodRow(card) {
        if (!card) return;
        var periods = readPeriodsFromDom(card);
        periods.push(normalizePeriod({
            label: 'Periode ' + (periods.length + 1),
            start: md(1, 1), end: md(1, 31), due: md(2, 28)
        }));
        var builder = card.querySelector('.cron-builder');
        if (builder) builder.dataset.periodTpl = 'periodic';
        renderPeriodEditor(card, periods, 'periodic');
        syncPeriodTable(card);
    }

    function removePeriodRow(btn) {
        var row = closest(btn, '.pt-card') || closest(btn, 'tr');
        var card = closest(btn, '.step-card');
        if (row) row.remove();
        if (card) syncPeriodTable(card);
    }

    function initPeriodEditor(card) {
        if (!card) return;
        var initBlock = card.querySelector('.init-freq-echeance');
        if (!initBlock || initBlock.classList.contains('d-none')) return;
        var cfg = readCfg(card);
        var freq = freqFromPeriodTemplate(cfg.periodTemplate || cfg.periodFrequency) || 'monthly';
        var builder = card.querySelector('.cron-builder');
        if (builder) builder.dataset.periodTpl = freq;

        cfg.yearMode = 'YYYY';
        if (cfg.calendarYear && !cfg.previewYear) cfg.previewYear = cfg.calendarYear;

        // Seed visible echeance fields from ActionConfigJson before sync
        var startEl = card.querySelector('.window-start-day');
        var endEl = card.querySelector('.window-end-day');
        var offEl = card.querySelector('.window-month-offset');
        if (startEl && cfg.windowStartDay) startEl.value = String(cfg.windowStartDay);
        if (endEl && cfg.windowEndDay) endEl.value = String(cfg.windowEndDay);
        if (offEl && cfg.windowMonthOffset != null) offEl.value = String(cfg.windowMonthOffset);

        var rows;
        if (Array.isArray(cfg.periods) && cfg.periods.length) {
            var raw = cfg.periods;
            var missingYearOff = raw.some(function (p) {
                return p == null || p.openYearOffset == null || p.dueYearOffset == null;
            });
            rows = raw.map(normalizePeriod);
            var missingDue = rows.some(function (p) { return !p.dueMonth || !p.dueDay; });
            var missingOpen = rows.some(function (p) { return !p.openMonth || !p.openDay; });
            // Monthly must start with January activity; quarterly with Oct–Déc
            var wrongOrder = (freq === 'monthly' && rows[0] && rows[0].startMonth !== 1)
                || (freq === 'quarterly' && rows[0] && rows[0].startMonth !== 10);
            if ((missingDue || missingOpen || missingYearOff || wrongOrder)
                && (freq === 'monthly' || freq === 'quarterly')) {
                rows = generatePeriodRows(freq);
            }
        } else {
            rows = generatePeriodRows(freq);
        }
        writeCfg(card, cfg);
        renderPeriodEditor(card, rows, freq);
        syncPeriodTable(card);
    }

    function uniqueSorted(nums) {
        var seen = {};
        var out = [];
        nums.forEach(function (n) {
            if (!seen[n]) { seen[n] = 1; out.push(n); }
        });
        out.sort(function (a, b) { return a - b; });
        return out;
    }

    function buildCronFromPeriods(builder, periods, freq) {
        if (!builder) return;
        var timeInput = builder.querySelector('.cron-time');
        var hm = (timeInput && timeInput.value ? timeInput.value : '08:00').split(':').map(Number);
        var h = hm[0] || 8, mi = hm[1] || 0;
        var timeStr = pad(h) + ':' + pad(mi);

        var starts = periods.map(function (p) {
            var x = parseMd(p.start);
            return x ? { m: x.m, d: x.d } : null;
        }).filter(Boolean);
        var ends = periods.map(function (p) {
            var x = parseMd(p.end);
            return x ? { m: x.m, d: x.d } : null;
        }).filter(Boolean);

        var cron = '', desc = '';
        if (starts.length) {
            var startDays = uniqueSorted(starts.map(function (x) { return x.d; }));
            var startMonths = uniqueSorted(starts.map(function (x) { return x.m; }));
            if (freq === 'annually' && starts.length === 1) {
                cron = mi + ' ' + h + ' ' + starts[0].d + ' ' + starts[0].m + ' *';
                desc = 'Ouverture le ' + starts[0].d + ' ' + monthNames[starts[0].m] + ' \u00e0 ' + timeStr;
            } else if (freq === 'quarterly' || (startMonths.length > 0 && startMonths.length <= 4 && startMonths.length < 12)) {
                cron = mi + ' ' + h + ' ' + startDays.join(',') + ' ' + startMonths.join(',') + ' *';
                desc = 'Ouverture le ' + startDays.join(', ') + ' (' + startMonths.map(function (m) { return monthNames[m]; }).join(', ') + ') \u00e0 ' + timeStr;
            } else {
                cron = mi + ' ' + h + ' ' + startDays.join(',') + ' * *';
                desc = 'Ouverture le ' + startDays.join(', ') + ' de chaque mois \u00e0 ' + timeStr;
            }
        }

        var section = closest(builder, '.schedule-section');
        if (section) {
            var cronInput = section.querySelector('.cron-input');
            var descInput = section.querySelector('.cron-desc-input');
            if (cronInput) cronInput.value = cron;
            if (descInput) descInput.value = desc;
        }

        var preview = builder.querySelector('.cron-preview');
        if (preview && cron) {
            preview.classList.remove('d-none');
            var t = preview.querySelector('.cron-preview-text');
            var c = preview.querySelector('.cron-preview-cron');
            if (t) t.textContent = desc;
            if (c) c.textContent = cron;
        } else if (preview) {
            preview.classList.add('d-none');
        }

        // \u00c9ch\u00e9ance de travail = per-row due (fallback: legacy window* / EOM next month)
        var endPreview = builder.querySelector('.cron-preview-end');
        var card = closest(builder, '.step-card');
        var actionSel = card && card.querySelector('select[name*="ActionOnLifecycleEnd"]');
        var action = actionSel && actionSel.value ? actionSel.value : 'ClotureDeclaration';
        var actionLabel = action === 'ArchiveDeclaration' ? 'Archiver'
            : action === 'SuspendDeclaration' ? 'Suspendre'
            : action === 'CompleteDeclaration' ? 'Terminer'
            : 'Cl\u00f4turer';

        var cfg = readCfg(card);
        var dues = periods.map(function (p) {
            var du = parseMd(p.due);
            if (du) return du;
            var e = parseMd(p.end);
            return e ? eomNextMonth(e.m) : null;
        }).filter(Boolean);

        if ((dues.length || ends.length) && endPreview) {
            var dueDays = uniqueSorted(dues.map(function (x) { return x.d; }));
            var dueMonths = uniqueSorted(dues.map(function (x) { return x.m; }));
            var endCron, endDesc;
            if (dues.length) {
                endCron = mi + ' ' + h + ' '
                    + (dueDays.length === 1 ? dueDays[0] : dueDays.join(','))
                    + ' '
                    + (dueMonths.length && dueMonths.length < 12 ? dueMonths.join(',') : '*')
                    + ' *';
                var sample = dues.slice(0, 3).map(function (x) {
                    return pad2(x.d) + '/' + pad2(x.m);
                }).join(', ');
                endDesc = '\u00c9ch\u00e9ance par p\u00e9riode (' + sample
                    + (dues.length > 3 ? '\u2026' : '')
                    + ') \u00e0 ' + timeStr + ' \u00b7 ' + actionLabel;
            } else {
                // Legacy fallback when rows have no due
                var dueDay = parseInt(cfg.windowEndDay, 10);
                if (!(dueDay >= 1 && dueDay <= 31)) dueDay = 31;
                var monthOff = parseInt(cfg.windowMonthOffset, 10);
                if (!(monthOff >= 0)) monthOff = 1;
                dueMonths = uniqueSorted(ends.map(function (x) {
                    var m = x.m + monthOff;
                    while (m > 12) m -= 12;
                    while (m < 1) m += 12;
                    return m;
                }));
                endCron = mi + ' ' + h + ' ' + dueDay + ' ' + (dueMonths.length && dueMonths.length < 12 ? dueMonths.join(',') : '*') + ' *';
                endDesc = '\u00c9ch\u00e9ance de travail le ' + dueDay
                    + (monthOff === 1 ? ' du mois suivant' : ' (+' + monthOff + ' mois)')
                    + ' \u00e0 ' + timeStr + ' \u00b7 ' + actionLabel;
            }
            endPreview.classList.remove('d-none');
            var et = endPreview.querySelector('.cron-preview-text');
            var ec = endPreview.querySelector('.cron-preview-cron');
            if (et) et.textContent = endDesc;
            if (ec) ec.textContent = endCron;
            cfg.periodEndCron = endCron;
            cfg.periodEndCronDescription = endDesc;
            writeCfg(card, cfg);
            var plain = card.querySelector('.period-end-preview');
            if (plain) plain.value = endDesc;
        } else if (endPreview) {
            endPreview.classList.add('d-none');
        }
    }

    function setTriggerMode(card, mode) {
        if (!card) return;
        var triggerSel = card.querySelector('.init-trigger-source, select[name*=".TriggerSource"]');
        var schedChk = card.querySelector('.sched-checkbox');
        var cronPanel = card.querySelector('.cron-freq-panel');
        var allowManual = card.querySelector('.init-allow-manual');
        var cfgInput = card.querySelector('.action-config-input');

        mode = mode || 'both';
        card.querySelectorAll('.init-trigger-btn').forEach(function (b) {
            b.classList.toggle('active', b.getAttribute('data-mode') === mode);
        });

        if (mode === 'manual') {
            if (schedChk) schedChk.checked = false;
            if (triggerSel) triggerSel.value = 'Manual';
            if (cronPanel) cronPanel.classList.add('d-none');
        } else {
            if (schedChk) schedChk.checked = true;
            if (triggerSel) triggerSel.value = 'Schedule';
            if (cronPanel) cronPanel.classList.remove('d-none');
        }

        if (allowManual) {
            allowManual.checked = mode === 'both' || mode === 'manual';
            allowManual.disabled = mode === 'manual';
        }

        if (cfgInput) {
            var cfg = {};
            try { cfg = JSON.parse(cfgInput.value || '{}') || {}; } catch (e) { cfg = {}; }
            cfg.initTrigger = mode;
            cfg.allowManualInit = mode === 'both' || mode === 'manual';
            cfgInput.value = JSON.stringify(cfg);
        }

        var section = card.querySelector('.schedule-section');
        if (section) section.classList.toggle('d-none', false);
    }

    function setCronFreq(btn, freq) {
        var builder = closest(btn, '.cron-builder');
        if (!builder) return;
        builder.querySelectorAll('.cron-freq-btn[data-freq]').forEach(function (b) { b.classList.remove('active'); });
        btn.classList.add('active');
        builder.dataset.freq = freq;

        var card = closest(builder, '.step-card');
        var isInit = !!(card && card.querySelector('.init-freq-echeance') && !card.querySelector('.init-freq-echeance').classList.contains('d-none'));
        if (isInit) {
            loadPeriodTemplate(card, freq === 'periodic' ? 'periodic' : freq);
            return;
        }

        ['weekly', 'monthly', 'quarterly', 'annually', 'custom'].forEach(function (f) {
            var panel = builder.querySelector('.cron-options-' + f);
            if (panel) panel.classList.toggle('d-none', f !== freq);
        });
        buildCron(builder);
    }

    function toggleCronDay(btn) {
        btn.classList.toggle('active');
        buildCron(closest(btn, '.cron-builder'));
    }
    function toggleCronDom(btn) {
        btn.classList.toggle('active');
        buildCron(closest(btn, '.cron-builder'));
    }
    function toggleCronMonth(btn) {
        btn.classList.toggle('active');
        buildCron(closest(btn, '.cron-builder'));
    }

    function buildCron(builder) {
        if (!builder) return;
        var card = closest(builder, '.step-card');
        var isInit = !!(card && card.querySelector('.init-freq-echeance') && !card.querySelector('.init-freq-echeance').classList.contains('d-none'));
        if (isInit) {
            syncPeriodTable(card);
            return;
        }

        var freq = builder.dataset.freq;
        if (!freq || freq === 'manual') {
            hideCronPreview(builder);
            var section0 = closest(builder, '.schedule-section');
            if (section0) {
                var c0 = section0.querySelector('.cron-input');
                var d0 = section0.querySelector('.cron-desc-input');
                if (c0) c0.value = '';
                if (d0) d0.value = 'D\u00e9clenchement manuel uniquement';
            }
            return;
        }

        var timeInput = builder.querySelector('.cron-time');
        var hm = (timeInput && timeInput.value ? timeInput.value : '08:00').split(':').map(Number);
        var h = hm[0] || 8, m = hm[1] || 0;
        var cron = '', desc = '';

        if (freq === 'daily') {
            cron = m + ' ' + h + ' * * *';
            desc = 'Tous les jours \u00e0 ' + pad(h) + ':' + pad(m);
        } else if (freq === 'weekly') {
            var days = Array.prototype.map.call(builder.querySelectorAll('.cron-options-weekly .cron-day-btn.active'), function (b) { return b.dataset.dow; });
            if (!days.length) { hideCronPreview(builder); return; }
            cron = m + ' ' + h + ' * * ' + days.join(',');
            desc = 'Chaque ' + days.map(function (d) { return dayNames[parseInt(d, 10)]; }).join(', ') + ' \u00e0 ' + pad(h) + ':' + pad(m);
        } else if (freq === 'monthly') {
            var doms = Array.prototype.map.call(builder.querySelectorAll('.cron-options-monthly .cron-day-btn.active'), function (b) { return b.dataset.dom; });
            if (!doms.length) { hideCronPreview(builder); return; }
            cron = m + ' ' + h + ' ' + doms.join(',') + ' * *';
            desc = 'Le ' + doms.join(', ') + ' de chaque mois \u00e0 ' + pad(h) + ':' + pad(m);
        } else if (freq === 'quarterly') {
            var months = Array.prototype.map.call(builder.querySelectorAll('.cron-options-quarterly .cron-month-btn.active'), function (b) { return b.dataset.m; });
            if (!months.length) { hideCronPreview(builder); return; }
            var qDay = (builder.querySelector('.cron-quarterly-day') || {}).value || '1';
            cron = m + ' ' + h + ' ' + qDay + ' ' + months.join(',') + ' *';
            desc = 'Le ' + qDay + ' de ' + months.map(function (mm) { return monthNames[parseInt(mm, 10)]; }).join(', ') + ' \u00e0 ' + pad(h) + ':' + pad(m);
        } else if (freq === 'annually') {
            var monthSel = builder.querySelector('.cron-annual-month');
            var daySel = builder.querySelector('.cron-annual-day');
            var mm = monthSel ? monthSel.value : '1';
            var dd = daySel ? daySel.value : '1';
            cron = m + ' ' + h + ' ' + dd + ' ' + mm + ' *';
            desc = 'Le ' + dd + ' ' + monthNames[parseInt(mm, 10)] + ' de chaque ann\u00e9e \u00e0 ' + pad(h) + ':' + pad(m);
        } else if (freq === 'custom') {
            var dateInput = builder.querySelector('.cron-custom-date');
            if (!dateInput || !dateInput.value) { hideCronPreview(builder); return; }
            var parts = dateInput.value.split('-');
            if (parts.length < 3) { hideCronPreview(builder); return; }
            var cDay = parseInt(parts[2], 10);
            var cMonth = parseInt(parts[1], 10);
            var repeat = (builder.querySelector('.cron-custom-repeat') || {}).value || 'yearly';
            if (repeat === 'monthly') {
                cron = m + ' ' + h + ' ' + cDay + ' * *';
                desc = 'Personnalis\u00e9 \u2014 le ' + cDay + ' de chaque mois \u00e0 ' + pad(h) + ':' + pad(m);
            } else {
                cron = m + ' ' + h + ' ' + cDay + ' ' + cMonth + ' *';
                desc = 'Personnalis\u00e9 \u2014 chaque ann\u00e9e le ' + cDay + ' ' + monthNames[cMonth] + ' \u00e0 ' + pad(h) + ':' + pad(m) + ' (r\u00e9f. ' + dateInput.value + ')';
            }
        }

        var section = closest(builder, '.schedule-section');
        if (section) {
            var cronInput = section.querySelector('.cron-input');
            var descInput = section.querySelector('.cron-desc-input');
            if (cronInput) cronInput.value = cron;
            if (descInput) descInput.value = desc;
        }

        var preview = builder.querySelector('.cron-preview');
        if (preview) {
            preview.classList.remove('d-none');
            var t = preview.querySelector('.cron-preview-text');
            var c = preview.querySelector('.cron-preview-cron');
            if (t) t.textContent = desc;
            if (c) c.textContent = cron;
        }
        var endPreview = builder.querySelector('.cron-preview-end');
        if (endPreview) endPreview.classList.add('d-none');
    }

    function hideCronPreview(builder) {
        var preview = builder && builder.querySelector('.cron-preview');
        if (preview) preview.classList.add('d-none');
        var endPreview = builder && builder.querySelector('.cron-preview-end');
        if (endPreview) endPreview.classList.add('d-none');
    }

    function parseCronToUI(builder) {
        if (!builder) return;
        var card = closest(builder, '.step-card');
        var isInit = !!(card && card.querySelector('.init-freq-echeance') && !card.querySelector('.init-freq-echeance').classList.contains('d-none'));
        if (isInit) return; // Init uses period table, not cron freq buttons

        var cronStr = builder.dataset.cron;
        if (!cronStr || !cronStr.trim()) return;
        var parts = cronStr.trim().split(/\s+/);
        if (parts.length < 5) return;
        var mm = parts[0], hh = parts[1], dom = parts[2], mon = parts[3], dow = parts[4];
        var timeInput = builder.querySelector('.cron-time');
        if (timeInput) timeInput.value = pad(parseInt(hh, 10)) + ':' + pad(parseInt(mm, 10));

        function clickFreq(freq) {
            var freqBtn = builder.querySelector('.cron-freq-btn[data-freq="' + freq + '"]');
            if (freqBtn) setCronFreq(freqBtn, freq);
        }

        if (dow !== '*') {
            clickFreq('weekly');
            dow.split(',').forEach(function (d) {
                var btn = builder.querySelector('.cron-day-btn[data-dow="' + d + '"]');
                if (btn) btn.classList.add('active');
            });
        } else if (mon !== '*') {
            var months = mon.split(',');
            if (months.length <= 4) {
                clickFreq('quarterly');
                builder.querySelectorAll('.cron-options-quarterly .cron-month-btn').forEach(function (b) { b.classList.remove('active'); });
                months.forEach(function (m) {
                    var btn = builder.querySelector('.cron-month-btn[data-m="' + m + '"]');
                    if (btn) btn.classList.add('active');
                });
                var qd = builder.querySelector('.cron-quarterly-day');
                if (qd) qd.value = dom;
            } else {
                clickFreq('annually');
                var mSel = builder.querySelector('.cron-annual-month');
                var dSel = builder.querySelector('.cron-annual-day');
                if (mSel) mSel.value = months[0];
                if (dSel) dSel.value = dom;
            }
        } else if (dom !== '*') {
            clickFreq('monthly');
            dom.split(',').forEach(function (d) {
                var btn = builder.querySelector('.cron-day-btn[data-dom="' + d + '"]');
                if (btn) btn.classList.add('active');
            });
        } else {
            clickFreq('daily');
        }
        buildCron(builder);
    }

    function syncInitScheduleVisibility(card) {
        if (!card) return;
        var typeSel = card.querySelector('.step-type-select, select[name*=".StepType"]');
        var assignSel = card.querySelector('.assign-select, select[name*=".AssignmentType"]');
        var type = typeSel ? typeSel.value : '';
        var assign = assignSel ? assignSel.value : '';
        var isRelance = type === 'Relance';
        var isInit = type === 'Initialisation';
        var isInitAuto = isInit && assign === 'AutoSystem';
        var initPanel = card.querySelector('.init-schedule-panel');
        var scheduleSection = card.querySelector('.schedule-section');
        var schedChk = card.querySelector('.sched-checkbox');
        var initEcheance = card.querySelector('.init-freq-echeance');
        var titleEl = card.querySelector('.schedule-section-title-text');
        var nonInitFreq = card.querySelector('.cron-freq-non-init-wrap, .cron-freq-non-init');

        if (initPanel) initPanel.classList.toggle('d-none', !isInitAuto);
        if (initEcheance) initEcheance.classList.toggle('d-none', !isInit);
        if (nonInitFreq) {
            var wrap = card.querySelector('.cron-freq-non-init-wrap') || nonInitFreq;
            wrap.classList.toggle('d-none', isInit);
        }
        card.querySelectorAll('.cron-freq-extra').forEach(function (btn) { btn.classList.toggle('d-none', isInit); });
        card.querySelectorAll('.cron-options-monthly-extra, .cron-options-weekly, .cron-options-monthly, .cron-options-quarterly, .cron-options-annually, .cron-options-custom').forEach(function (el) {
            if (isInit) el.classList.add('d-none');
        });
        if (titleEl) titleEl.textContent = isInit ? 'P\u00e9riodes & \u00e9ch\u00e9ances' : 'Planification';

        if (isRelance) {
            // Relance is config on other steps \u2014 no dedicated schedule panel
            if (scheduleSection) scheduleSection.classList.add('d-none');
            if (schedChk) schedChk.checked = false;
            return;
        }

        if (isInit || assign === 'AutoSystem' || isInitAuto) {
            if (scheduleSection) scheduleSection.classList.remove('d-none');
            if (schedChk && isInit) schedChk.checked = true;
            if (isInitAuto) {
                var mode = readInitMode(card);
                setTriggerMode(card, mode || 'both');
            }
            if (isInit) initPeriodEditor(card);
        }
    }

    function readInitMode(card) {
        if (!card) return 'both';
        var cfgInput = card.querySelector('.action-config-input');
        if (cfgInput && cfgInput.value) {
            try {
                var cfg = JSON.parse(cfgInput.value);
                if (cfg && cfg.initTrigger) return cfg.initTrigger;
                if (cfg && cfg.allowManualInit === false) return 'schedule';
            } catch (e) { /* ignore */ }
        }
        var triggerSel = card.querySelector('.init-trigger-source, select[name*=".TriggerSource"]');
        if (triggerSel && triggerSel.value === 'Manual') return 'manual';
        var activeMode = card.querySelector('.init-trigger-btn.active');
        return activeMode ? activeMode.getAttribute('data-mode') : 'both';
    }

    function validateInitSteps(root) {
        root = root || document;
        var cards = root.querySelectorAll('.step-card');
        for (var i = 0; i < cards.length; i++) {
            var card = cards[i];
            var typeSel = card.querySelector('.step-type-select, select[name*=".StepType"]');
            var assignSel = card.querySelector('.assign-select, select[name*=".AssignmentType"]');
            if (!typeSel || !assignSel) continue;
            if (typeSel.value !== 'Initialisation' || assignSel.value !== 'AutoSystem') continue;
            syncPeriodTable(card);
            var cfg = readCfg(card);
            var periods = Array.isArray(cfg.periods) ? cfg.periods : [];
            var valid = periods.filter(function (p) { return p.startMonth && p.startDay && p.endMonth && p.endDay; });
            if (!valid.length) {
                var name = (card.querySelector('.step-name-input') || {}).value || ('\u00c9tape ' + (i + 1));
                return 'Initialisation \u00ab ' + name + ' \u00bb : renseignez au moins une p\u00e9riode (date d\u00e9but / date fin).';
            }
            var mode = readInitMode(card);
            if (mode === 'manual') continue;
            var cronInput = card.querySelector('.cron-input');
            if (!cronInput || !String(cronInput.value || '').trim()) {
                var name2 = (card.querySelector('.step-name-input') || {}).value || ('\u00c9tape ' + (i + 1));
                return 'Initialisation automatique \u00ab ' + name2 + ' \u00bb : le tableau des p\u00e9riodes doit d\u00e9finir les dates d\'ouverture.';
            }
        }
        return null;
    }

    function cronBuilderHtml(prefix, cronExpr, cronDesc) {
        cronExpr = cronExpr || '';
        cronDesc = cronDesc || '';
        return ''
            + '<div class="field-section schedule-section">'
            + '<div class="field-section-title"><i class="bi bi-calendar-event"></i><span class="schedule-section-title-text">P\u00e9riodes &amp; \u00e9ch\u00e9ances</span></div>'
            + '<div class="init-schedule-panel mb-2">'
            + '<label class="form-label-sm mb-1">Mode de d\u00e9clenchement</label>'
            + '<div class="d-flex flex-wrap gap-1 mb-2">'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="schedule" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'schedule\')"><i class="bi bi-clock-history"></i> Planifi\u00e9</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="manual" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'manual\')"><i class="bi bi-hand-index"></i> Manuel</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn active" data-mode="both" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'both\')"><i class="bi bi-shuffle"></i> Planifi\u00e9 + Manuel</button>'
            + '</div>'
            + '<label class="chk-item mb-2" style="display:inline-flex;max-width:100%;"><input type="checkbox" class="form-check-input init-allow-manual" checked /><span>Autoriser d\u00e9marrage manuel</span></label>'
            + '<input type="hidden" name="' + prefix + '.TriggerSource" class="init-trigger-source" value="Schedule" />'
            + '<textarea name="' + prefix + '.ActionConfigJson" class="action-config-input d-none" rows="1">{"initTrigger":"both","allowManualInit":true,"periodTemplate":"Mensuelle","periodFrequency":"Mensuelle","periods":[]}</textarea>'
            + '</div>'
            + '<div class="cron-builder cron-freq-panel" data-cron="' + String(cronExpr).replace(/"/g, '&quot;') + '">'
            + '<div class="init-freq-echeance mb-2"><div class="life-panel">'
            + '<span class="life-label">Mod\u00e8le de p\u00e9riodes</span>'
            + '<div class="d-flex flex-wrap gap-1 mb-2">'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="monthly" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'monthly\')"><i class="bi bi-calendar-month"></i> Mensuel (12)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="quarterly" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'quarterly\')"><i class="bi bi-calendar3"></i> Trimestriel (4)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="annually" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'annually\')"><i class="bi bi-calendar-heart"></i> Annuel (1)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="periodic" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'periodic\')"><i class="bi bi-calendar-range"></i> P\u00e9riodique</button>'
            + '</div>'
            + '<div class="period-template-editor mb-2"></div>'
            + '<div class="row g-2 align-items-end mb-2">'
            + '<div class="col-md-3 init-global-window-fields d-none"><span class="life-label">Jour d\'\u00e9ch\u00e9ance (legacy)</span>'
            + '<input type="number" class="form-control form-control-sm window-end-day" min="1" max="31" value="31" onchange="WfCron.syncPeriodTable(this.closest(\'.step-card\'))" /></div>'
            + '<div class="col-md-3 init-global-window-fields d-none"><span class="life-label">Mois apr\u00e8s fin (legacy)</span>'
            + '<input type="number" class="form-control form-control-sm window-month-offset" min="0" max="24" value="1" onchange="WfCron.syncPeriodTable(this.closest(\'.step-card\'))" /></div>'
            + '<div class="col-md-3 init-global-window-fields d-none"><span class="life-label">Jour ouverture (legacy)</span>'
            + '<input type="number" class="form-control form-control-sm window-start-day" min="1" max="31" value="1" onchange="WfCron.syncPeriodTable(this.closest(\'.step-card\'))" /></div>'
            + '<div class="col-md-3"><span class="life-label">Action \u00e0 la fin</span>'
            + '<select name="' + prefix + '.ActionOnLifecycleEnd" class="form-select form-select-sm" onchange="WfCron.syncPeriodTable(this.closest(\'.step-card\'))">'
            + '<option value="ClotureDeclaration" selected>Cl\u00f4turer</option>'
            + '<option value="ArchiveDeclaration">Archiver</option>'
            + '<option value="SuspendDeclaration">Suspendre</option>'
            + '<option value="CompleteDeclaration">Terminer</option>'
            + '</select></div></div>'
            + '<div class="echeance-work-preview small text-muted mb-1"></div>'
            + '<input type="hidden" class="period-end-preview" />'
            + '<input type="hidden" class="deadline-year-offset" value="0" />'
            + '</div></div>'
            + '<div class="row g-2 mt-1"><div class="col-md-4"><label class="form-label-sm">Heure</label><input type="time" class="form-control form-control-sm cron-time" value="08:00" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))" /></div></div>'
            + '<div class="cron-preview d-none mt-2"><span class="cron-preview-icon"><i class="bi bi-play-fill"></i></span><div><div class="cron-preview-label">Ouverture</div><div class="cron-preview-text"></div><div class="cron-preview-cron"></div></div></div>'
            + '<div class="cron-preview-end d-none"><span class="cron-preview-icon"><i class="bi bi-flag-fill"></i></span><div><div class="cron-preview-label">\u00c9ch\u00e9ance</div><div class="cron-preview-text"></div><div class="cron-preview-cron"></div></div></div>'
            + '</div>'
            + '<input type="hidden" name="' + prefix + '.CronExpression" class="cron-input" value="' + String(cronExpr).replace(/"/g, '&quot;') + '" />'
            + '<input type="hidden" name="' + prefix + '.CronDescription" class="cron-desc-input" value="' + String(cronDesc).replace(/"/g, '&quot;') + '" />'
            + '<input type="checkbox" name="' + prefix + '.IsScheduled" value="true" class="sched-checkbox d-none" checked />'
            + '</div>';
    }

    // Back-compat aliases
    function syncWindowDays(card) { syncPeriodTable(card); }
    function applySmartWindowDefaults(card) { initPeriodEditor(card); }
    function syncPeriodEndFromFreq(card, freq) {
        if (card) loadPeriodTemplate(card, freq || 'monthly');
    }

    global.WfCron = {
        setTriggerMode: setTriggerMode,
        setCronFreq: setCronFreq,
        toggleCronDay: toggleCronDay,
        toggleCronDom: toggleCronDom,
        toggleCronMonth: toggleCronMonth,
        buildCron: buildCron,
        parseCronToUI: parseCronToUI,
        syncWindowDays: syncWindowDays,
        syncPeriodTable: syncPeriodTable,
        loadPeriodTemplate: loadPeriodTemplate,
        initPeriodEditor: initPeriodEditor,
        addPeriodRow: addPeriodRow,
        removePeriodRow: removePeriodRow,
        applySmartWindowDefaults: applySmartWindowDefaults,
        syncPeriodEndFromFreq: syncPeriodEndFromFreq,
        syncInitScheduleVisibility: syncInitScheduleVisibility,
        cronBuilderHtml: cronBuilderHtml,
        readInitMode: readInitMode,
        validateInitSteps: validateInitSteps
    };

    global.setCronFreq = function (btn, freq) { setCronFreq(btn, freq); };
    global.toggleCronDay = function (btn) { toggleCronDay(btn); };
    global.toggleCronDom = function (btn) { toggleCronDom(btn); };
    global.toggleCronMonth = function (btn) { toggleCronMonth(btn); };
    global.buildCron = function (builder) { buildCron(builder); };
    global.parseCronToUI = function (builder) { parseCronToUI(builder); };

    document.addEventListener('change', function (e) {
        var t = e.target;
        if (!t || !t.classList || !t.classList.contains('init-allow-manual')) return;
        var card = closest(t, '.step-card');
        if (!card) return;
        var cfgInput = card.querySelector('.action-config-input');
        if (!cfgInput) return;
        var cfg = {};
        try { cfg = JSON.parse(cfgInput.value || '{}') || {}; } catch (err) { cfg = {}; }
        cfg.allowManualInit = !!t.checked;
        if (cfg.initTrigger === 'schedule' && t.checked) cfg.initTrigger = 'both';
        if (cfg.initTrigger === 'both' && !t.checked) cfg.initTrigger = 'schedule';
        cfgInput.value = JSON.stringify(cfg);
        card.querySelectorAll('.init-trigger-btn').forEach(function (b) {
            b.classList.toggle('active', b.getAttribute('data-mode') === (cfg.initTrigger || 'both'));
        });
    });
})(window);

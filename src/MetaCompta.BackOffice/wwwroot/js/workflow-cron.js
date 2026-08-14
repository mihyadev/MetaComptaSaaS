/**
 * Shared cron / Init period-table builder for Workflow Builder + Client Personnaliser.
 * Init source of truth: editable periods[] (label, start, end = échéance).
 */
(function (global) {
    'use strict';

    var dayNames = ['Di', 'Lu', 'Ma', 'Me', 'Je', 'Ve', 'Sa'];
    var monthNames = ['', 'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin', 'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'];

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

    /** Parse "MM-DD" or legacy "yyyy-MM-dd" / "dd/MM/yyyy" → { m, d } (year ignored). */
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

    function normalizePeriod(p) {
        var s = parseMd(p && (p.start || p.startMd));
        var e = parseMd(p && (p.end || p.endMd));
        if (p && p.startMonth && p.startDay) s = { m: +p.startMonth, d: +p.startDay };
        if (p && p.endMonth && p.endDay) e = { m: +p.endMonth, d: +p.endDay };
        return {
            label: (p && p.label) || '',
            start: s ? md(s.m, s.d) : '',
            end: e ? md(e.m, e.d) : '',
            startMonth: s ? s.m : null,
            startDay: s ? s.d : null,
            endMonth: e ? e.m : null,
            endDay: e ? e.d : null
        };
    }

    function monthOptionsHtml(selected) {
        var html = '';
        for (var i = 1; i <= 12; i++) {
            html += '<option value="' + i + '"' + (i === selected ? ' selected' : '') + '>' + pad2(i) + ' ' + monthNames[i].slice(0, 3) + '</option>';
        }
        return html;
    }

    function mdEditorHtml(prefix, m, d) {
        m = m || 1; d = d || 1;
        return '<div class="pt-md-row">'
            + '<input type="number" class="pt-' + prefix + '-d" min="1" max="31" value="' + d + '" title="Jour" />'
            + '<span class="pt-sep">/</span>'
            + '<select class="pt-' + prefix + '-m" title="Mois">' + monthOptionsHtml(m) + '</select>'
            + '<span class="pt-yyyy" title="Année courante à l\'exécution (comme cron)">/YYYY</span>'
            + '</div>';
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
        if (freq === 'quarterly') return 'Trimestriel — 4 périodes';
        if (freq === 'annually') return 'Annuel — 1 période';
        if (freq === 'periodic') return 'Périodique — périodes manuelles';
        return 'Mensuel — 12 périodes';
    }

    /** Seed rows as year-agnostic JJ/MM/YYYY patterns (YYYY = any year). */
    function generatePeriodRows(freq) {
        var rows = [];
        if (freq === 'quarterly') {
            for (var q = 1; q <= 4; q++) {
                var sm = (q - 1) * 3 + 1;
                var em = sm + 2;
                rows.push(normalizePeriod({ label: 'T' + q, start: md(sm, 1), end: md(em, daysInMonth(em)) }));
            }
            return rows;
        }
        if (freq === 'annually') {
            rows.push(normalizePeriod({ label: 'Année', start: md(1, 1), end: md(12, 31) }));
            return rows;
        }
        if (freq === 'periodic') {
            rows.push(normalizePeriod({ label: 'Période 1', start: md(1, 1), end: md(6, 30) }));
            return rows;
        }
        for (var m = 1; m <= 12; m++) {
            rows.push(normalizePeriod({ label: monthNames[m], start: md(m, 1), end: md(m, daysInMonth(m)) }));
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
        box.querySelectorAll('tbody tr').forEach(function (tr) {
            var label = (tr.querySelector('.pt-label') || {}).value || '';
            var sm = parseInt((tr.querySelector('.pt-start-m') || {}).value, 10) || 0;
            var sd = parseInt((tr.querySelector('.pt-start-d') || {}).value, 10) || 0;
            var em = parseInt((tr.querySelector('.pt-end-m') || {}).value, 10) || 0;
            var ed = parseInt((tr.querySelector('.pt-end-d') || {}).value, 10) || 0;
            if (!sm && !em && !label) return;
            if (sd) sd = Math.min(Math.max(1, sd), daysInMonth(sm || 1));
            if (ed) ed = Math.min(Math.max(1, ed), daysInMonth(em || 1));
            rows.push(normalizePeriod({
                label: label.trim(),
                startMonth: sm, startDay: sd || 1,
                endMonth: em, endDay: ed || 1
            }));
        });
        return rows;
    }

    function renderPeriodEditor(card, rows, freq) {
        var box = card && card.querySelector('.period-template-editor');
        if (!box) return;
        freq = freq || 'monthly';
        rows = (rows || []).map(normalizePeriod);
        var html = '<div class="period-template-box"><div class="pt-head"><span>Modèle récurrent (comme cron)</span><strong>' + tplTitle(freq) + '</strong></div>'
            + '<div class="pt-scroll"><table><thead><tr><th>Période</th><th>Début (JJ/MM/YYYY)</th><th>Fin / échéance (JJ/MM/YYYY)</th><th></th></tr></thead><tbody>';
        rows.forEach(function (r, i) {
            var s = parseMd(r.start) || { m: 1, d: 1 };
            var e = parseMd(r.end) || { m: 1, d: 1 };
            html += '<tr data-idx="' + i + '">'
                + '<td><input type="text" class="pt-label" value="' + String(r.label || '').replace(/"/g, '&quot;') + '" /></td>'
                + '<td>' + mdEditorHtml('start', s.m, s.d) + '</td>'
                + '<td>' + mdEditorHtml('end', e.m, e.d) + '</td>'
                + '<td><button type="button" class="pt-del" title="Supprimer" onclick="WfCron.removePeriodRow(this)"><i class="bi bi-trash"></i></button></td>'
                + '</tr>';
        });
        html += '</tbody></table></div>'
            + '<div class="p-2 border-top d-flex justify-content-between align-items-center gap-2 flex-wrap">'
            + '<button type="button" class="btn btn-sm btn-outline-secondary pt-add" onclick="WfCron.addPeriodRow(this.closest(\'.step-card\'))"><i class="bi bi-plus-lg"></i> Ajouter une période</button>'
            + '<span class="small text-muted"><strong>YYYY</strong> = année courante à l\'exécution (pas une année fixe)</span>'
            + '</div></div>';
        box.innerHTML = html;
        box.querySelectorAll('.pt-label, .pt-start-d, .pt-start-m, .pt-end-d, .pt-end-m').forEach(function (inp) {
            inp.addEventListener('change', function () { syncPeriodTable(card); });
        });
        card.querySelectorAll('.period-tpl-btn').forEach(function (b) {
            b.classList.toggle('active', b.getAttribute('data-tpl') === freq);
        });
    }

    function syncPeriodTable(card) {
        if (!card) return;
        var initBlock = card.querySelector('.init-freq-echeance');
        if (!initBlock || initBlock.classList.contains('d-none')) return;

        var periods = readPeriodsFromDom(card);
        var cfg = readCfg(card);
        var builder = card.querySelector('.cron-builder');
        var freq = (builder && builder.dataset.periodTpl) || freqFromPeriodTemplate(cfg.periodTemplate || cfg.periodFrequency) || 'monthly';

        // Persist year-agnostic patterns only (MM-DD + month/day fields)
        cfg.periods = periods.map(function (p) {
            return {
                label: p.label,
                start: p.start,
                end: p.end,
                startMonth: p.startMonth,
                startDay: p.startDay,
                endMonth: p.endMonth,
                endDay: p.endDay
            };
        });
        cfg.periodTemplate = periodFrequencyValue(freq) || 'Mensuelle';
        cfg.periodFrequency = cfg.periodTemplate;
        delete cfg.windowStartDay;
        delete cfg.windowEndDay;
        delete cfg.windowMonthOffset;
        delete cfg.deadlineYearOffset;
        delete cfg.deadlineMonth;

        var first = periods[0];
        if (first) {
            var startEl = card.querySelector('.window-start-day');
            var endEl = card.querySelector('.window-end-day');
            if (startEl && first.startDay) startEl.value = String(first.startDay);
            if (endEl && first.endDay) endEl.value = String(first.endDay);
        }

        writeCfg(card, cfg);
        if (builder) {
            builder.dataset.periodTpl = freq;
            builder.dataset.freq = freq === 'periodic' ? 'monthly' : freq;
            buildCronFromPeriods(builder, periods, freq);
        }
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
        periods.push(normalizePeriod({ label: 'Période ' + (periods.length + 1), start: md(1, 1), end: md(1, 31) }));
        var builder = card.querySelector('.cron-builder');
        if (builder) builder.dataset.periodTpl = 'periodic';
        renderPeriodEditor(card, periods, 'periodic');
        syncPeriodTable(card);
    }

    function removePeriodRow(btn) {
        var tr = closest(btn, 'tr');
        var card = closest(btn, '.step-card');
        if (tr) tr.remove();
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
        var rows = Array.isArray(cfg.periods) && cfg.periods.length
            ? cfg.periods.map(normalizePeriod)
            : generatePeriodRows(freq);
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
                desc = 'Ouverture le ' + starts[0].d + ' ' + monthNames[starts[0].m] + ' à ' + timeStr;
            } else if (freq === 'quarterly' || (startMonths.length > 0 && startMonths.length <= 4 && startMonths.length < 12)) {
                cron = mi + ' ' + h + ' ' + startDays.join(',') + ' ' + startMonths.join(',') + ' *';
                desc = 'Ouverture le ' + startDays.join(', ') + ' (' + startMonths.map(function (m) { return monthNames[m]; }).join(', ') + ') à ' + timeStr;
            } else {
                cron = mi + ' ' + h + ' ' + startDays.join(',') + ' * *';
                desc = 'Ouverture le ' + startDays.join(', ') + ' de chaque mois à ' + timeStr;
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

        // Échéance cron from Date fin
        var endPreview = builder.querySelector('.cron-preview-end');
        var card = closest(builder, '.step-card');
        var actionSel = card && card.querySelector('select[name*="ActionOnLifecycleEnd"]');
        var action = actionSel && actionSel.value ? actionSel.value : 'ClotureDeclaration';
        var actionLabel = action === 'ArchiveDeclaration' ? 'Archiver'
            : action === 'SuspendDeclaration' ? 'Suspendre'
            : action === 'CompleteDeclaration' ? 'Terminer'
            : 'Clôturer';

        if (ends.length && endPreview) {
            var endDays = uniqueSorted(ends.map(function (x) { return x.d; }));
            var endMonths = uniqueSorted(ends.map(function (x) { return x.m; }));
            var endCron = '', endDesc = '';
            if (freq === 'annually' && ends.length === 1) {
                endCron = mi + ' ' + h + ' ' + ends[0].d + ' ' + ends[0].m + ' *';
                endDesc = 'Échéance le ' + ends[0].d + ' ' + monthNames[ends[0].m] + ' à ' + timeStr + ' · ' + actionLabel;
            } else if (freq === 'quarterly' || (endMonths.length > 0 && endMonths.length <= 4 && endMonths.length < 12)) {
                endCron = mi + ' ' + h + ' ' + endDays.join(',') + ' ' + endMonths.join(',') + ' *';
                endDesc = 'Échéance le ' + endDays.join(', ') + ' (' + endMonths.map(function (m) { return monthNames[m]; }).join(', ') + ') à ' + timeStr + ' · ' + actionLabel;
            } else {
                endCron = mi + ' ' + h + ' ' + endDays.join(',') + ' * *';
                endDesc = 'Échéance le ' + endDays.join(', ') + ' de chaque mois à ' + timeStr + ' · ' + actionLabel;
            }
            endPreview.classList.remove('d-none');
            var et = endPreview.querySelector('.cron-preview-text');
            var ec = endPreview.querySelector('.cron-preview-cron');
            if (et) et.textContent = endDesc;
            if (ec) ec.textContent = endCron;
            var cfg = readCfg(card);
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
                if (d0) d0.value = 'Déclenchement manuel uniquement';
            }
            return;
        }

        var timeInput = builder.querySelector('.cron-time');
        var hm = (timeInput && timeInput.value ? timeInput.value : '08:00').split(':').map(Number);
        var h = hm[0] || 8, m = hm[1] || 0;
        var cron = '', desc = '';

        if (freq === 'daily') {
            cron = m + ' ' + h + ' * * *';
            desc = 'Tous les jours à ' + pad(h) + ':' + pad(m);
        } else if (freq === 'weekly') {
            var days = Array.prototype.map.call(builder.querySelectorAll('.cron-options-weekly .cron-day-btn.active'), function (b) { return b.dataset.dow; });
            if (!days.length) { hideCronPreview(builder); return; }
            cron = m + ' ' + h + ' * * ' + days.join(',');
            desc = 'Chaque ' + days.map(function (d) { return dayNames[parseInt(d, 10)]; }).join(', ') + ' à ' + pad(h) + ':' + pad(m);
        } else if (freq === 'monthly') {
            var doms = Array.prototype.map.call(builder.querySelectorAll('.cron-options-monthly .cron-day-btn.active'), function (b) { return b.dataset.dom; });
            if (!doms.length) { hideCronPreview(builder); return; }
            cron = m + ' ' + h + ' ' + doms.join(',') + ' * *';
            desc = 'Le ' + doms.join(', ') + ' de chaque mois à ' + pad(h) + ':' + pad(m);
        } else if (freq === 'quarterly') {
            var months = Array.prototype.map.call(builder.querySelectorAll('.cron-options-quarterly .cron-month-btn.active'), function (b) { return b.dataset.m; });
            if (!months.length) { hideCronPreview(builder); return; }
            var qDay = (builder.querySelector('.cron-quarterly-day') || {}).value || '1';
            cron = m + ' ' + h + ' ' + qDay + ' ' + months.join(',') + ' *';
            desc = 'Le ' + qDay + ' de ' + months.map(function (mm) { return monthNames[parseInt(mm, 10)]; }).join(', ') + ' à ' + pad(h) + ':' + pad(m);
        } else if (freq === 'annually') {
            var monthSel = builder.querySelector('.cron-annual-month');
            var daySel = builder.querySelector('.cron-annual-day');
            var mm = monthSel ? monthSel.value : '1';
            var dd = daySel ? daySel.value : '1';
            cron = m + ' ' + h + ' ' + dd + ' ' + mm + ' *';
            desc = 'Le ' + dd + ' ' + monthNames[parseInt(mm, 10)] + ' de chaque année à ' + pad(h) + ':' + pad(m);
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
                desc = 'Personnalisé — le ' + cDay + ' de chaque mois à ' + pad(h) + ':' + pad(m);
            } else {
                cron = m + ' ' + h + ' ' + cDay + ' ' + cMonth + ' *';
                desc = 'Personnalisé — chaque année le ' + cDay + ' ' + monthNames[cMonth] + ' à ' + pad(h) + ':' + pad(m) + ' (réf. ' + dateInput.value + ')';
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
        if (titleEl) titleEl.textContent = isInit ? 'Périodes & échéances' : 'Planification';

        if (isRelance) {
            // Relance is config on other steps — no dedicated schedule panel
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
                var name = (card.querySelector('.step-name-input') || {}).value || ('Étape ' + (i + 1));
                return 'Initialisation « ' + name + ' » : renseignez au moins une période (date début / date fin).';
            }
            var mode = readInitMode(card);
            if (mode === 'manual') continue;
            var cronInput = card.querySelector('.cron-input');
            if (!cronInput || !String(cronInput.value || '').trim()) {
                var name2 = (card.querySelector('.step-name-input') || {}).value || ('Étape ' + (i + 1));
                return 'Initialisation automatique « ' + name2 + ' » : le tableau des périodes doit définir les dates d\'ouverture.';
            }
        }
        return null;
    }

    function cronBuilderHtml(prefix, cronExpr, cronDesc) {
        cronExpr = cronExpr || '';
        cronDesc = cronDesc || '';
        return ''
            + '<div class="field-section schedule-section">'
            + '<div class="field-section-title"><i class="bi bi-calendar-event"></i><span class="schedule-section-title-text">Périodes &amp; échéances</span></div>'
            + '<div class="init-schedule-panel mb-2">'
            + '<label class="form-label-sm mb-1">Mode de déclenchement</label>'
            + '<div class="d-flex flex-wrap gap-1 mb-2">'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="schedule" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'schedule\')"><i class="bi bi-clock-history"></i> Planifié</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="manual" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'manual\')"><i class="bi bi-hand-index"></i> Manuel</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn active" data-mode="both" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'both\')"><i class="bi bi-shuffle"></i> Planifié + Manuel</button>'
            + '</div>'
            + '<label class="chk-item mb-2" style="display:inline-flex;max-width:100%;"><input type="checkbox" class="form-check-input init-allow-manual" checked /><span>Autoriser démarrage manuel</span></label>'
            + '<input type="hidden" name="' + prefix + '.TriggerSource" class="init-trigger-source" value="Schedule" />'
            + '<input type="hidden" name="' + prefix + '.ActionConfigJson" class="action-config-input" value=\'{"initTrigger":"both","allowManualInit":true,"periodTemplate":"Mensuelle","periods":[]}\' />'
            + '</div>'
            + '<div class="cron-builder cron-freq-panel" data-cron="' + String(cronExpr).replace(/"/g, '&quot;') + '">'
            + '<div class="init-freq-echeance mb-2"><div class="life-panel">'
            + '<span class="life-label">Modèle de périodes</span>'
            + '<div class="d-flex flex-wrap gap-1 mb-2">'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="monthly" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'monthly\')"><i class="bi bi-calendar-month"></i> Mensuel (12)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="quarterly" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'quarterly\')"><i class="bi bi-calendar3"></i> Trimestriel (4)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="annually" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'annually\')"><i class="bi bi-calendar-heart"></i> Annuel (1)</button>'
            + '<button type="button" class="cron-freq-btn period-tpl-btn" data-tpl="periodic" onclick="WfCron.loadPeriodTemplate(this.closest(\'.step-card\'),\'periodic\')"><i class="bi bi-calendar-range"></i> Périodique</button>'
            + '</div>'
            + '<div class="period-template-editor mb-2"></div>'
            + '<div class="row g-2"><div class="col-md-6"><span class="life-label">Action à la fin</span>'
            + '<select name="' + prefix + '.ActionOnLifecycleEnd" class="form-select form-select-sm" onchange="WfCron.syncPeriodTable(this.closest(\'.step-card\'))">'
            + '<option value="ClotureDeclaration" selected>Clôturer</option>'
            + '<option value="ArchiveDeclaration">Archiver</option>'
            + '<option value="SuspendDeclaration">Suspendre</option>'
            + '<option value="CompleteDeclaration">Terminer</option>'
            + '</select></div></div>'
            + '<input type="hidden" class="period-end-preview" />'
            + '<input type="hidden" class="window-start-day" value="1" />'
            + '<input type="hidden" class="window-end-day" value="1" />'
            + '<input type="hidden" class="window-month-offset" value="1" />'
            + '<input type="hidden" class="deadline-year-offset" value="0" />'
            + '</div></div>'
            + '<div class="row g-2 mt-1"><div class="col-md-4"><label class="form-label-sm">Heure</label><input type="time" class="form-control form-control-sm cron-time" value="08:00" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))" /></div></div>'
            + '<div class="cron-preview d-none mt-2"><span class="cron-preview-icon"><i class="bi bi-play-fill"></i></span><div><div class="cron-preview-label">Ouverture</div><div class="cron-preview-text"></div><div class="cron-preview-cron"></div></div></div>'
            + '<div class="cron-preview-end d-none"><span class="cron-preview-icon"><i class="bi bi-flag-fill"></i></span><div><div class="cron-preview-label">Échéance</div><div class="cron-preview-text"></div><div class="cron-preview-cron"></div></div></div>'
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

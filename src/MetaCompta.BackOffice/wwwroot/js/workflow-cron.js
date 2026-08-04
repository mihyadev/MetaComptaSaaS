/**
 * Shared cron / Init frequency builder for Workflow Builder + Client Personnaliser.
 * Frequencies: daily, weekly, monthly, quarterly, annually, custom, manual.
 */
(function (global) {
    'use strict';

    var dayNames = ['Di', 'Lu', 'Ma', 'Me', 'Je', 'Ve', 'Sa'];
    var monthNames = ['', 'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin', 'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'];

    function pad(n) { return String(n).padStart(2, '0'); }

    function closest(el, sel) {
        return el && el.closest ? el.closest(sel) : null;
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
            var parts = dateInput.value.split('-'); // YYYY-MM-DD
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
    }

    function hideCronPreview(builder) {
        var preview = builder && builder.querySelector('.cron-preview');
        if (preview) preview.classList.add('d-none');
    }

    function parseCronToUI(builder) {
        if (!builder) return;
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
        var isInitAuto = type === 'Initialisation' && assign === 'AutoSystem';
        var initPanel = card.querySelector('.init-schedule-panel');
        var scheduleSection = card.querySelector('.schedule-section');
        var schedChk = card.querySelector('.sched-checkbox');

        if (initPanel) initPanel.classList.toggle('d-none', !isInitAuto);

        if (assign === 'AutoSystem' || isInitAuto) {
            if (scheduleSection) scheduleSection.classList.remove('d-none');
            if (schedChk && isInitAuto) schedChk.checked = true;
            if (isInitAuto) {
                var mode = readInitMode(card);
                setTriggerMode(card, mode || 'both');
            }
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

    /** Validate Init+AutoSystem steps before save. Returns error message or null. */
    function validateInitSteps(root) {
        root = root || document;
        var cards = root.querySelectorAll('.step-card');
        for (var i = 0; i < cards.length; i++) {
            var card = cards[i];
            var typeSel = card.querySelector('.step-type-select, select[name*=".StepType"]');
            var assignSel = card.querySelector('.assign-select, select[name*=".AssignmentType"]');
            if (!typeSel || !assignSel) continue;
            if (typeSel.value !== 'Initialisation' || assignSel.value !== 'AutoSystem') continue;
            var mode = readInitMode(card);
            if (mode === 'manual') continue;
            var cronInput = card.querySelector('.cron-input');
            if (!cronInput || !String(cronInput.value || '').trim()) {
                var name = (card.querySelector('.step-name-input') || {}).value || ('Étape ' + (i + 1));
                return 'Initialisation automatique « ' + name + ' » : choisissez une fréquence (mensuel / trimestriel / annuel / personnalisé).';
            }
        }
        return null;
    }

    function cronBuilderHtml(prefix, cronExpr, cronDesc) {
        cronExpr = cronExpr || '';
        cronDesc = cronDesc || '';
        return ''
            + '<div class="field-section schedule-section">'
            + '<div class="field-section-title"><i class="bi bi-calendar-event"></i>Initialisation automatique — fréquence</div>'
            + '<div class="init-schedule-panel mb-2">'
            + '<label class="form-label-sm mb-1">Mode de déclenchement</label>'
            + '<div class="d-flex flex-wrap gap-1 mb-2">'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="schedule" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'schedule\')"><i class="bi bi-clock-history"></i> Planifié (cron)</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn" data-mode="manual" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'manual\')"><i class="bi bi-hand-index"></i> Manuel</button>'
            + '<button type="button" class="cron-freq-btn init-trigger-btn active" data-mode="both" onclick="WfCron.setTriggerMode(this.closest(\'.step-card\'),\'both\')"><i class="bi bi-shuffle"></i> Planifié + Manuel</button>'
            + '</div>'
            + '<label class="chk-item mb-2" style="display:inline-flex;max-width:100%;"><input type="checkbox" class="form-check-input init-allow-manual" checked /><span>Autoriser le démarrage manuel (bouton Démarrer)</span></label>'
            + '<input type="hidden" name="' + prefix + '.TriggerSource" class="init-trigger-source" value="Schedule" />'
            + '<input type="hidden" name="' + prefix + '.ActionConfigJson" class="action-config-input" value=\'{"initTrigger":"both","allowManualInit":true}\' />'
            + '</div>'
            + '<div class="cron-builder cron-freq-panel" data-cron="' + String(cronExpr).replace(/"/g, '&quot;') + '">'
            + '<div class="mb-2"><label class="form-label-sm mb-1">Fréquence</label>'
            + '<div class="d-flex flex-wrap gap-1">'
            + '<button type="button" class="cron-freq-btn" data-freq="monthly" onclick="WfCron.setCronFreq(this,\'monthly\')"><i class="bi bi-calendar-month"></i> Mensuel</button>'
            + '<button type="button" class="cron-freq-btn" data-freq="quarterly" onclick="WfCron.setCronFreq(this,\'quarterly\')"><i class="bi bi-calendar3"></i> Trimestriel</button>'
            + '<button type="button" class="cron-freq-btn" data-freq="annually" onclick="WfCron.setCronFreq(this,\'annually\')"><i class="bi bi-calendar-heart"></i> Annuel</button>'
            + '<button type="button" class="cron-freq-btn" data-freq="custom" onclick="WfCron.setCronFreq(this,\'custom\')"><i class="bi bi-calendar-plus"></i> Personnalisé</button>'
            + '</div></div>'
            + '<div class="cron-options-monthly d-none mb-2"><label class="form-label-sm mb-1">Jour du mois</label><div class="d-flex flex-wrap gap-1">'
            + [1,5,10,15,20,25,28].map(function (d) { return '<button type="button" class="cron-day-btn" data-dom="' + d + '" onclick="WfCron.toggleCronDom(this)">' + d + '</button>'; }).join('')
            + '</div></div>'
            + '<div class="cron-options-quarterly d-none mb-2">'
            + '<label class="form-label-sm mb-1">Mois (trimestres)</label><div class="d-flex flex-wrap gap-1 mb-2">'
            + [[1,'Jan'],[4,'Avr'],[7,'Jul'],[10,'Oct']].map(function (x) { return '<button type="button" class="cron-month-btn' + (x[0]===1||x[0]===4||x[0]===7||x[0]===10?' active':'') + '" data-m="' + x[0] + '" onclick="WfCron.toggleCronMonth(this)">' + x[1] + '</button>'; }).join('')
            + '</div><label class="form-label-sm">Jour</label><select class="form-select form-select-sm cron-quarterly-day" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))"><option value="1">1</option><option value="5">5</option><option value="10">10</option><option value="15">15</option></select></div>'
            + '<div class="cron-options-annually d-none mb-2"><label class="form-label-sm mb-1">Mois et jour</label><div class="row g-2"><div class="col-6"><select class="form-select form-select-sm cron-annual-month" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))">'
            + monthNames.slice(1).map(function (n, i) { return '<option value="' + (i + 1) + '">' + n + '</option>'; }).join('')
            + '</select></div><div class="col-6"><select class="form-select form-select-sm cron-annual-day" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))"><option value="1">1</option><option value="5">5</option><option value="10">10</option><option value="15">15</option><option value="20">20</option><option value="25">25</option><option value="28">28</option></select></div></div></div>'
            + '<div class="cron-options-custom d-none mb-2"><label class="form-label-sm mb-1">Date personnalisée</label>'
            + '<div class="row g-2"><div class="col-md-6"><input type="date" class="form-control form-control-sm cron-custom-date" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))" /></div>'
            + '<div class="col-md-6"><select class="form-select form-select-sm cron-custom-repeat" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))"><option value="yearly">Répéter chaque année</option><option value="monthly">Répéter chaque mois (jour)</option></select></div></div></div>'
            + '<div class="row g-2 mt-1"><div class="col-md-4"><label class="form-label-sm">Heure</label><input type="time" class="form-control form-control-sm cron-time" value="08:00" onchange="WfCron.buildCron(this.closest(\'.cron-builder\'))" /></div></div>'
            + '<div class="cron-preview d-none mt-2"><span class="cron-preview-icon"><i class="bi bi-check-lg"></i></span><div><div class="cron-preview-text"></div><div class="cron-preview-cron"></div></div></div>'
            + '</div>'
            + '<input type="hidden" name="' + prefix + '.CronExpression" class="cron-input" value="' + String(cronExpr).replace(/"/g, '&quot;') + '" />'
            + '<input type="hidden" name="' + prefix + '.CronDescription" class="cron-desc-input" value="' + String(cronDesc).replace(/"/g, '&quot;') + '" />'
            + '<input type="checkbox" name="' + prefix + '.IsScheduled" value="true" class="sched-checkbox d-none" checked />'
            + '</div>';
    }

    global.WfCron = {
        setTriggerMode: setTriggerMode,
        setCronFreq: setCronFreq,
        toggleCronDay: toggleCronDay,
        toggleCronDom: toggleCronDom,
        toggleCronMonth: toggleCronMonth,
        buildCron: buildCron,
        parseCronToUI: parseCronToUI,
        syncInitScheduleVisibility: syncInitScheduleVisibility,
        cronBuilderHtml: cronBuilderHtml,
        readInitMode: readInitMode,
        validateInitSteps: validateInitSteps
    };

    // Back-compat for Builder.cshtml inline onclick handlers
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

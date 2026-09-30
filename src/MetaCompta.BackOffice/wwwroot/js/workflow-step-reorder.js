/**
 * Workflow Builder — step reorder (drag & drop, ↑↓, editable order number).
 * Requires SortableJS (global Sortable).
 */
(function (global) {
    'use strict';

    function getCards(container) {
        return Array.prototype.slice.call(container.querySelectorAll(':scope > .step-card'));
    }

    function remapGoto(container, orderMap) {
        getCards(container).forEach(function (card) {
            card.querySelectorAll('[name$=".GotoStepOrder"]').forEach(function (el) {
                if (!el.value && el.value !== '0') return;
                var g = parseInt(el.value, 10);
                if (isNaN(g)) return;
                if (Object.prototype.hasOwnProperty.call(orderMap, g)) {
                    el.value = orderMap[g];
                }
            });
        });
    }

    function reindex(container, opts) {
        opts = opts || {};
        var cards = getCards(container);
        var oldOrders = cards.map(function (c) {
            var hi = c.querySelector('.step-order-input');
            if (hi && hi.value !== '') return parseInt(hi.value, 10);
            return parseInt(c.dataset.index || '0', 10);
        });
        var orderMap = {};
        cards.forEach(function (card, newIdx) {
            orderMap[oldOrders[newIdx]] = newIdx;
        });

        if (typeof opts.beforeReindex === 'function') opts.beforeReindex(container);

        cards.forEach(function (card, i) {
            card.dataset.index = String(i);
            var numInput = card.querySelector('.step-num-input');
            if (numInput) {
                numInput.value = String(i + 1);
                numInput.max = String(cards.length);
            }
            var numLabel = card.querySelector('.step-num:not(input)');
            if (numLabel && !numLabel.classList.contains('step-num-input')) {
                numLabel.textContent = String(i + 1);
            }
            card.querySelectorAll('[name]').forEach(function (el) {
                if (el.name && /Steps\[\d+\]/.test(el.name)) {
                    el.name = el.name.replace(/Steps\[\d+]/, 'Steps[' + i + ']');
                }
            });
            var orderInput = card.querySelector('.step-order-input');
            if (orderInput) orderInput.value = String(i);
        });

        remapGoto(container, orderMap);
        updateMoveButtons(container);

        if (typeof opts.afterReindex === 'function') opts.afterReindex(container);
        if (typeof opts.onCountChange === 'function') opts.onCountChange(cards.length);
    }

    function updateMoveButtons(container) {
        var cards = getCards(container);
        cards.forEach(function (card, i) {
            var up = card.querySelector('.btn-move-up');
            var down = card.querySelector('.btn-move-down');
            if (up) up.disabled = i === 0;
            if (down) down.disabled = i === cards.length - 1;
        });
    }

    function moveCard(container, card, toIndex, opts) {
        var cards = getCards(container);
        var from = cards.indexOf(card);
        if (from < 0) return;
        var to = Math.max(0, Math.min(cards.length - 1, toIndex));
        if (to === from) {
            reindex(container, opts);
            return;
        }
        if (to < from) {
            container.insertBefore(card, cards[to]);
        } else {
            var ref = cards[to].nextSibling;
            container.insertBefore(card, ref);
        }
        reindex(container, opts);
    }

    function moveByDelta(btn, delta, opts) {
        var card = btn.closest('.step-card');
        var container = card && card.parentElement;
        if (!card || !container) return;
        var cards = getCards(container);
        var from = cards.indexOf(card);
        moveCard(container, card, from + delta, opts);
    }

    function onNumberChange(input, opts) {
        var card = input.closest('.step-card');
        var container = card && card.parentElement;
        if (!card || !container) return;
        var cards = getCards(container);
        var from = cards.indexOf(card);
        var raw = parseInt(input.value, 10);
        if (isNaN(raw)) {
            input.value = String(from + 1);
            return;
        }
        var to = Math.max(1, Math.min(cards.length, raw)) - 1;
        if (to === from) {
            input.value = String(from + 1);
            return;
        }
        moveCard(container, card, to, opts);
        card.classList.remove('collapsed-step');
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function initSortable(container, opts) {
        opts = opts || {};
        if (!container || typeof Sortable === 'undefined') return null;
        if (container._wfSortable) {
            try { container._wfSortable.destroy(); } catch (e) { /* ignore */ }
        }
        container._wfSortable = Sortable.create(container, {
            animation: 180,
            handle: '.step-drag-handle',
            draggable: '.step-card',
            ghostClass: 'step-ghost',
            chosenClass: 'step-chosen',
            dragClass: 'step-dragging',
            forceFallback: false,
            fallbackOnBody: true,
            swapThreshold: 0.65,
            onStart: function () {
                container.classList.add('is-reordering');
            },
            onEnd: function () {
                container.classList.remove('is-reordering');
                reindex(container, opts);
            }
        });
        updateMoveButtons(container);
        return container._wfSortable;
    }

    /** Markup helpers for headers (optional) */
    function orderControlsHtml(num, total) {
        total = total || num;
        return ''
            + '<div class="step-order-controls" onclick="event.stopPropagation()">'
            +   '<button type="button" class="step-drag-handle" title="Glisser pour réordonner" aria-label="Glisser">'
            +     '<i class="bi bi-grip-vertical"></i>'
            +   '</button>'
            +   '<input type="number" class="step-num step-num-input" value="' + num + '" min="1" max="' + total + '"'
            +     ' title="Changer le numéro d\'ordre" aria-label="Ordre de l\'étape"'
            +     ' onchange="window.WfStepReorder && WfStepReorder._onNum(this)"'
            +     ' onclick="event.stopPropagation()" />'
            +   '<div class="step-move-btns">'
            +     '<button type="button" class="btn-move-step btn-move-up" title="Monter" aria-label="Monter"'
            +       ' onclick="event.stopPropagation();window.WfStepReorder && WfStepReorder._move(this,-1)">'
            +       '<i class="bi bi-chevron-up"></i></button>'
            +     '<button type="button" class="btn-move-step btn-move-down" title="Descendre" aria-label="Descendre"'
            +       ' onclick="event.stopPropagation();window.WfStepReorder && WfStepReorder._move(this,1)">'
            +       '<i class="bi bi-chevron-down"></i></button>'
            +   '</div>'
            + '</div>';
    }

    var _defaultOpts = null;

    global.WfStepReorder = {
        init: function (container, opts) {
            _defaultOpts = opts || {};
            return initSortable(container, _defaultOpts);
        },
        reindex: reindex,
        updateMoveButtons: updateMoveButtons,
        moveByDelta: moveByDelta,
        onNumberChange: onNumberChange,
        orderControlsHtml: orderControlsHtml,
        _move: function (btn, delta) {
            moveByDelta(btn, delta, _defaultOpts || {});
        },
        _onNum: function (input) {
            onNumberChange(input, _defaultOpts || {});
        },
        bindContainer: function (container, opts) {
            _defaultOpts = opts || {};
            initSortable(container, _defaultOpts);
            reindex(container, _defaultOpts);
        }
    };
})(window);

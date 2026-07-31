from pathlib import Path

BUILDER_CSS = r"""<style>
    .builder-card { border: 0; border-radius: 16px; box-shadow: var(--monday-shadow); }
    .step-card { background: var(--monday-surface); border: 1px solid var(--monday-border); border-radius: 16px; margin-bottom: 1rem; position: relative; overflow: hidden; }
    .step-card.collapsed-step .step-body { display: none; }
    .step-card.collapsed-step .step-header { border-bottom: 0; }
    .step-card.collapsed-step .chevron-icon { transform: rotate(-90deg); }
    .step-num { width: 34px; height: 34px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 0.82rem; color: #fff; background: var(--monday-blue); flex-shrink: 0; }
    .step-header { display: flex; align-items: center; gap: 0.7rem; cursor: pointer; padding: 0.9rem 1.1rem; background: var(--monday-bg); border-bottom: 1px solid var(--monday-border); }
    .step-header h6 { margin: 0; font-size: 0.9rem; font-weight: 700; color: var(--monday-text); }
    .step-type-badge { font-size: 0.68rem; padding: 3px 10px; border-radius: 20px; font-weight: 700; background: color-mix(in srgb, var(--monday-blue) 12%, var(--monday-surface)); color: var(--monday-blue); border: 1px solid var(--monday-border); }
    .assign-badge { font-size: 0.68rem; padding: 3px 10px; border-radius: 20px; background: color-mix(in srgb, var(--monday-green) 12%, var(--monday-surface)); color: var(--monday-green); font-weight: 600; border: 1px solid var(--monday-border); }
    .sla-badge { font-size: 0.68rem; padding: 3px 10px; border-radius: 20px; background: color-mix(in srgb, var(--monday-orange) 12%, var(--monday-surface)); color: var(--monday-orange); font-weight: 600; border: 1px solid var(--monday-border); }
    .chevron-icon { transition: transform .25s ease; font-size: 0.9rem; color: var(--monday-muted); }
    .step-body { padding: 1.1rem; background: var(--monday-surface); }
    .field-section { background: var(--monday-bg); border: 1px solid var(--monday-border); border-radius: 12px; padding: 0.75rem 1rem; margin-bottom: 0.65rem; }
    .field-section-title { font-size: 0.72rem; font-weight: 800; color: var(--monday-blue); text-transform: uppercase; letter-spacing: 0.07em; margin-bottom: 0.55rem; display: flex; align-items: center; gap: 0.4rem; padding-bottom: 0.4rem; border-bottom: 1px solid var(--monday-border); }
    .form-label-sm { font-size: 0.74rem; font-weight: 600; color: var(--monday-muted); margin-bottom: 0.2rem; display: block; }
    .tpl-mini { display: flex; align-items: center; gap: 0.5rem; padding: 0.55rem 0.8rem; border: 1px solid var(--monday-border); border-radius: 12px; margin-bottom: 0.45rem; cursor: pointer; font-size: 0.8rem; text-decoration: none; }
    .tpl-mini.active { background: color-mix(in srgb, var(--monday-blue) 8%, var(--monday-surface)); border-color: var(--monday-blue); }
    .tpl-mini .tpl-type { font-size: 0.65rem; padding: 2px 7px; border-radius: 10px; font-weight: 700; }
    .chk-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.45rem; }
    .chk-item { display: flex; align-items: center; gap: 0.4rem; padding: 0.45rem 0.65rem; background: var(--monday-surface); border: 1px solid var(--monday-border); border-radius: 10px; font-size: 0.74rem; font-weight: 600; color: var(--monday-text); cursor: pointer; }
    .chk-item input:checked ~ span { color: var(--monday-blue); font-weight: 700; }
    .chk-item .form-check-input { width: 1rem; height: 1rem; margin: 0; }
    .btn-add-step { border: 2px dashed var(--monday-border); border-radius: 14px; padding: 1rem; text-align: center; color: var(--monday-blue); font-weight: 700; font-size: 0.85rem; background: transparent; cursor: pointer; width: 100%; display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
    .section-title { font-size: 0.8rem; font-weight: 800; color: var(--monday-text); text-transform: uppercase; letter-spacing: 0.05em; display: flex; align-items: center; gap: 0.45rem; margin-bottom: 0.6rem; }
    .section-title i { font-size: 0.9rem; color: var(--monday-blue); }
    .field-section details summary { list-style: none; cursor: pointer; }
    .field-section details summary::-webkit-details-marker { display: none; }
    .field-section details[open] summary .toggle-hint { display: none; }
    .toggle-hint { font-size: 0.65rem; font-weight: 400; color: var(--monday-muted); text-transform: none; letter-spacing: 0; }
    .btn-delete-step { width: 26px; height: 26px; border-radius: 8px; border: 1px solid var(--monday-border); background: var(--monday-surface); color: var(--monday-red); display: flex; align-items: center; justify-content: center; font-size: 0.72rem; cursor: pointer; }
    .cron-builder { background: var(--monday-surface); border: 1px solid var(--monday-border); border-radius: 12px; padding: 0.85rem 1rem; }
    .cron-freq-btn, .cron-day-btn, .cron-month-btn { border: 1px solid var(--monday-border); border-radius: 10px; cursor: pointer; font-size: 0.76rem; font-weight: 600; color: var(--monday-text); background: var(--monday-surface); }
    .cron-freq-btn.active, .cron-day-btn.active, .cron-month-btn.active { background: var(--monday-blue); color: #fff; border-color: var(--monday-blue); }
    .cron-day-btn { width: 36px; height: 36px; display: inline-flex; align-items: center; justify-content: center; font-size: 0.74rem; }
    .cron-preview { background: color-mix(in srgb, var(--monday-green) 10%, var(--monday-surface)); border: 1px solid var(--monday-border); border-radius: 10px; padding: 0.5rem 0.85rem; margin-top: 0.6rem; display: flex; align-items: center; gap: 0.5rem; }
    .cron-preview-icon { width: 28px; height: 28px; border-radius: 8px; background: var(--monday-green); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 0.8rem; flex-shrink: 0; }
    .cron-preview-text { font-size: 0.78rem; font-weight: 600; color: var(--monday-text); }
    .cron-preview-cron { font-size: 0.68rem; color: var(--monday-muted); font-family: monospace; }
</style>"""

def replace_style_block(path: str):
    p = Path(path)
    text = p.read_text(encoding="utf-8")
    s = text.index("<style>")
    e = text.index("</style>", s) + len("</style>")
    p.write_text(text[:s] + BUILDER_CSS + text[e:], encoding="utf-8")

replace_style_block("src/MetaCompta.BackOffice/Views/Workflow/Builder.cshtml")
print("builder ok")

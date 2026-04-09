#!/usr/bin/env python3
"""Generate the unified Workflow Builder view for MetaCompta BackOffice."""

import os

VIEW_PATH = os.path.join(
    os.path.dirname(__file__),
    "src", "MetaCompta.BackOffice", "Views", "Workflow", "Builder.cshtml"
)

content = r'''@using MetaCompta.Application.DTOs
@model WorkflowTemplateCreateDto
@{
    ViewData["Title"] = "Workflow Builder";
    var clients = ViewBag.Clients as List<ClientDto> ?? new List<ClientDto>();
    var departments = ViewBag.Departments as List<DepartmentDto> ?? new List<DepartmentDto>();
    var allTemplates = ViewBag.AllTemplates as List<WorkflowTemplateDto> ?? new List<WorkflowTemplateDto>();
    var editTemplate = ViewBag.EditTemplate as WorkflowTemplateDto;
    var editId = ViewBag.EditTemplateId as int?;
    var isEditMode = editId.HasValue;

    var stepTypes = new[] { "Initialisation", "DemandePieces", "Collecte", "Saisie", "Controle", "ValidationInterne", "ValidationClient", "DepotSoumission", "ClotureArchivage" };
    var assignmentTypes = new[] { "Employee", "Role", "Department", "Client", "AutoSystem" };
    var taskTypes = new[] { "Standard", "DocumentRequest", "DataEntry", "Review", "Validation", "Submission", "Archiving", "ClientAction" };
    var priorities = new[] { "Faible", "Moyenne", "Haute", "Urgente" };
    var roles = new[] { "Gerant", "Manager", "Collaborateur", "Assistant", "Stagiaire" };

    var typeColors = new Dictionary<string, (string bg, string fg, string icon)>
    {
        ["TVA"] = ("#dbeafe", "#2563eb", "bi-receipt-cutoff"),
        ["IS"] = ("#fce7f3", "#be185d", "bi-building"),
        ["IR"] = ("#ede9fe", "#7c3aed", "bi-person-vcard"),
        ["CNSS"] = ("#d1fae5", "#059669", "bi-shield-plus"),
        ["Paie"] = ("#fef3c7", "#d97706", "bi-cash-stack"),
        ["Juridique"] = ("#fee2e2", "#dc2626", "bi-file-earmark-text"),
        ["Comptabilite"] = ("#e0f2fe", "#0284c7", "bi-journal-bookmark-fill"),
        ["IR_SALAIRES"] = ("#f5f3ff", "#6d28d9", "bi-people-fill"),
        ["All"] = ("#f1f5f9", "#475569", "bi-grid-3x3-gap-fill")
    };
}

<style>
    /* ═══════════ BUILDER LAYOUT ═══════════ */
    .wb-page { display:flex; gap:0; height:calc(100vh - 60px - 3rem); margin:-1.5rem; }
    .wb-panel { background:rgba(255,255,255,0.95); backdrop-filter:blur(20px); border:1px solid #e2e8f0; overflow-y:auto; }
    .wb-panel-left { width:280px; min-width:280px; border-right:1px solid #e2e8f0; display:flex; flex-direction:column; }
    .wb-panel-center { flex:1; display:flex; flex-direction:column; overflow:hidden; }
    .wb-panel-right { width:360px; min-width:360px; border-left:1px solid #e2e8f0; display:flex; flex-direction:column; }

    /* LEFT PANEL - Template list + Actions */
    .wb-left-header { padding:1rem 1.15rem; border-bottom:1px solid #e2e8f0; background:#f8fafc; flex-shrink:0; }
    .wb-left-header h6 { font-size:0.88rem; font-weight:700; color:#1e293b; margin:0; }
    .wb-left-scroll { flex:1; overflow-y:auto; padding:0.75rem; }
    .wb-tpl-item { background:#fff; border:1px solid #e2e8f0; border-radius:10px; padding:0.65rem 0.85rem; margin-bottom:0.5rem; cursor:pointer; transition:all .2s ease; position:relative; overflow:hidden; }
    .wb-tpl-item:hover { border-color:#2563eb; box-shadow:0 4px 12px rgba(37,99,235,0.08); transform:translateY(-1px); }
    .wb-tpl-item.active { border-color:#2563eb; background:rgba(37,99,235,0.04); box-shadow:0 0 0 2px rgba(37,99,235,0.15); }
    .wb-tpl-item::before { content:''; position:absolute; top:0; left:0; width:3px; height:100%; background:#e2e8f0; border-radius:3px 0 0 3px; }
    .wb-tpl-item.active::before { background:#2563eb; }
    .wb-tpl-name { font-weight:600; font-size:0.82rem; color:#1e293b; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
    .wb-tpl-meta { font-size:0.68rem; color:#94a3b8; margin-top:0.15rem; }
    .wb-tpl-badge { display:inline-flex; align-items:center; gap:0.2rem; padding:0.1rem 0.4rem; border-radius:12px; font-size:0.62rem; font-weight:600; }
    .wb-action-grid { display:grid; grid-template-columns:1fr 1fr; gap:0.4rem; padding:0.75rem; border-top:1px solid #e2e8f0; flex-shrink:0; }
    .wb-action-btn { background:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:0.5rem 0.4rem; text-align:center; cursor:pointer; transition:all .2s; font-size:0.68rem; font-weight:600; color:#475569; }
    .wb-action-btn:hover { background:#2563eb; color:#fff; border-color:#2563eb; }
    .wb-action-btn i { display:block; font-size:1rem; margin-bottom:0.15rem; }

    /* CENTER PANEL - Pipeline / Steps */
    .wb-center-header { padding:0.85rem 1.25rem; border-bottom:1px solid #e2e8f0; display:flex; justify-content:space-between; align-items:center; flex-shrink:0; background:linear-gradient(135deg,#f8fafc,#fff); }
    .wb-center-scroll { flex:1; overflow-y:auto; padding:1.25rem; }
    .wb-pipeline-empty { text-align:center; padding:3rem 2rem; color:#94a3b8; }
    .wb-pipeline-empty i { font-size:3rem; display:block; margin-bottom:0.75rem; color:#cbd5e1; }
    .wb-step-card { background:#fff; border:1px solid #e2e8f0; border-radius:12px; margin-bottom:0.75rem; position:relative; overflow:hidden; transition:all .3s ease; cursor:grab; }
    .wb-step-card:hover { box-shadow:0 6px 20px rgba(0,0,0,0.06); transform:translateY(-1px); }
    .wb-step-card.dragging { opacity:0.5; transform:rotate(1deg); }
    .wb-step-card.drag-over { border-color:#2563eb; border-style:dashed; background:rgba(37,99,235,0.02); }
    .wb-step-accent { position:absolute; top:0; left:0; width:4px; height:100%; border-radius:4px 0 0 4px; }
    .wb-step-header { display:flex; justify-content:space-between; align-items:center; padding:0.65rem 0.85rem 0.35rem; }
    .wb-step-num { width:28px; height:28px; border-radius:8px; display:flex; align-items:center; justify-content:center; font-size:0.72rem; font-weight:800; flex-shrink:0; }
    .wb-step-title { font-weight:700; font-size:0.82rem; color:#1e293b; flex:1; margin:0 0.5rem; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
    .wb-step-actions { display:flex; gap:0.25rem; }
    .wb-step-actions .btn { padding:0.15rem 0.35rem; font-size:0.72rem; border-radius:6px; }
    .wb-step-body { padding:0 0.85rem 0.65rem; }
    .wb-step-tags { display:flex; flex-wrap:wrap; gap:0.3rem; }
    .wb-step-tag { display:inline-flex; align-items:center; gap:0.15rem; padding:0.1rem 0.45rem; border-radius:12px; font-size:0.65rem; font-weight:600; }
    .wb-connector { height:20px; display:flex; justify-content:center; position:relative; }
    .wb-connector::before { content:''; width:2px; height:100%; background:#e2e8f0; }
    .wb-connector::after { content:'\F285'; font-family:'bootstrap-icons'; position:absolute; bottom:-4px; color:#cbd5e1; font-size:0.5rem; background:#f1f5f9; padding:0 2px; }

    /* RIGHT PANEL - Config */
    .wb-right-header { padding:1rem 1.15rem; border-bottom:1px solid #e2e8f0; background:#f8fafc; flex-shrink:0; }
    .wb-right-header h6 { font-size:0.88rem; font-weight:700; color:#1e293b; margin:0; }
    .wb-right-scroll { flex:1; overflow-y:auto; padding:0.85rem; }
    .wb-config-section { background:#fff; border:1px solid #e2e8f0; border-radius:10px; padding:0.85rem; margin-bottom:0.75rem; }
    .wb-config-section h6 { font-size:0.78rem; font-weight:700; color:#1e293b; margin-bottom:0.6rem; display:flex; align-items:center; gap:0.4rem; }
    .wb-lbl { font-size:0.72rem; font-weight:600; color:#475569; margin-bottom:0.2rem; }
    .wb-input { border-radius:8px; border:1.5px solid #e2e8f0; font-size:0.8rem; padding:0.4rem 0.65rem; transition:all .2s; }
    .wb-input:focus { border-color:#2563eb; box-shadow:0 0 0 3px rgba(37,99,235,0.08); }
    .wb-select { border-radius:8px; border:1.5px solid #e2e8f0; font-size:0.8rem; padding:0.4rem 0.65rem; }
    .wb-switch .form-check-input { border-radius:6px; }
    .wb-switch .form-check-label { font-size:0.76rem; color:#475569; font-weight:500; }

    /* DRAG & DROP HIGHLIGHT */
    .wb-step-card.selected { border-color:#2563eb; box-shadow:0 0 0 2px rgba(37,99,235,0.2); }

    /* Scrollbar */
    .wb-panel::-webkit-scrollbar, .wb-left-scroll::-webkit-scrollbar, .wb-center-scroll::-webkit-scrollbar, .wb-right-scroll::-webkit-scrollbar { width:4px; }
    .wb-panel::-webkit-scrollbar-thumb, .wb-left-scroll::-webkit-scrollbar-thumb, .wb-center-scroll::-webkit-scrollbar-thumb, .wb-right-scroll::-webkit-scrollbar-thumb { background:#cbd5e1; border-radius:2px; }

    @@media (max-width:1200px) { .wb-panel-left { width:220px; min-width:220px; } .wb-panel-right { width:300px; min-width:300px; } }
    @@media (max-width:992px) { .wb-page { flex-direction:column; height:auto; } .wb-panel-left,.wb-panel-right { width:100%; min-width:100%; max-height:300px; } }
</style>

<div class="wb-page">
    <!-- ═══════════ LEFT PANEL — Templates + Quick Actions ═══════════ -->
    <div class="wb-panel wb-panel-left">
        <div class="wb-left-header">
            <div class="d-flex justify-content-between align-items-center">
                <h6><i class="bi bi-kanban text-primary me-1"></i> Mod&#232;les</h6>
                <span class="badge bg-primary bg-opacity-10 text-primary" style="font-size:0.65rem;">@allTemplates.Count</span>
            </div>
            <input type="text" class="form-control wb-input mt-2" id="searchTemplates" placeholder="Rechercher..." style="font-size:0.75rem; padding:0.3rem 0.55rem;" />
        </div>
        <div class="wb-left-scroll" id="templateList">
            <!-- New Template Card -->
            <a href="@Url.Action("Builder")" class="wb-tpl-item d-block text-decoration-none @(!isEditMode ? "active" : "")" style="border-style:dashed; border-color:#2563eb; background:rgba(37,99,235,0.02);">
                <div class="d-flex align-items-center gap-2">
                    <div class="wb-step-num" style="background:#dbeafe; color:#2563eb;"><i class="bi bi-plus-lg"></i></div>
                    <div>
                        <div class="wb-tpl-name" style="color:#2563eb;">Nouveau mod&#232;le</div>
                        <div class="wb-tpl-meta">Cr&#233;er depuis z&#233;ro</div>
                    </div>
                </div>
            </a>

            @foreach (var tpl in allTemplates.OrderByDescending(t => t.IsDefault).ThenBy(t => t.DeclarationType))
            {
                var tc = typeColors.ContainsKey(tpl.DeclarationType) ? typeColors[tpl.DeclarationType] : typeColors["All"];
                <a href="@Url.Action("Builder", new { id = tpl.Id })" class="wb-tpl-item d-block text-decoration-none @(editId == tpl.Id ? "active" : "")" data-search="@tpl.Name.ToLower() @tpl.DeclarationType.ToLower()">
                    <div class="d-flex align-items-center gap-2">
                        <div class="wb-step-num" style="background:@tc.bg; color:@tc.fg;"><i class="bi @tc.icon" style="font-size:0.7rem;"></i></div>
                        <div style="flex:1; min-width:0;">
                            <div class="wb-tpl-name">@tpl.Name</div>
                            <div class="wb-tpl-meta d-flex gap-1 align-items-center">
                                <span class="wb-tpl-badge" style="background:@tc.bg; color:@tc.fg;">@tpl.DeclarationType</span>
                                <span>@tpl.StepCount &#233;tapes</span>
                                @if (tpl.IsDefault) { <span class="wb-tpl-badge" style="background:#fef3c7; color:#d97706;"><i class="bi bi-star-fill"></i></span> }
                            </div>
                        </div>
                    </div>
                </a>
            }
        </div>
        <div class="wb-action-grid">
            <form asp-action="CreateDefaultTemplate" method="post" style="display:contents;">@Html.AntiForgeryToken()<button type="submit" class="wb-action-btn" onclick="return confirm('Cr&#233;er un mod&#232;le par d&#233;faut ?')"><i class="bi bi-magic"></i>D&#233;faut</button></form>
            <form asp-action="SeedAllDefaultTemplates" method="post" style="display:contents;">@Html.AntiForgeryToken()<button type="submit" class="wb-action-btn" onclick="return confirm('G&#233;n&#233;rer tous les mod&#232;les par d&#233;faut ?')"><i class="bi bi-stars"></i>Seed All</button></form>
            <a href="@Url.Action("StartWorkflow")" class="wb-action-btn text-decoration-none"><i class="bi bi-play-circle-fill"></i>D&#233;marrer WF</a>
            <a href="@Url.Action("Index")" class="wb-action-btn text-decoration-none"><i class="bi bi-speedometer2"></i>Dashboard</a>
        </div>
    </div>

    <!-- ═══════════ CENTER PANEL — Visual Pipeline ═══════════ -->
    <div class="wb-panel wb-panel-center">
        <div class="wb-center-header">
            <div>
                <h6 class="fw-bold mb-0" style="font-size:0.92rem; color:#1e293b;">
                    <i class="bi bi-diagram-3-fill text-primary me-1"></i>
                    @if (isEditMode) { <span>@editTemplate?.Name</span> } else { <span>Nouveau Workflow</span> }
                </h6>
                <div style="font-size:0.7rem; color:#94a3b8;">
                    @if (isEditMode) { <span>Mode &#233;dition &middot; v@(editTemplate?.Version ?? 1) &middot; @(Model.Steps?.Count ?? 0) &#233;tapes</span> }
                    else { <span>Glissez et configurez les &#233;tapes</span> }
                </div>
            </div>
            <div class="d-flex gap-2">
                <button type="button" class="btn btn-sm btn-outline-primary" style="border-radius:8px; font-size:0.75rem; font-weight:600;" onclick="addStep()"><i class="bi bi-plus-circle me-1"></i>&#201;tape</button>
                @if (isEditMode)
                {
                    <form asp-action="DeleteTemplate" asp-route-id="@editId" method="post" class="d-inline">@Html.AntiForgeryToken()<button type="submit" class="btn btn-sm btn-outline-danger" style="border-radius:8px; font-size:0.75rem;" onclick="return confirm('Supprimer ce mod&#232;le ?')"><i class="bi bi-trash3"></i></button></form>
                }
            </div>
        </div>
        <div class="wb-center-scroll" id="pipelineScroll">
            <form asp-action="BuilderSave" method="post" id="builderForm">
                @Html.AntiForgeryToken()
                @if (isEditMode) { <input type="hidden" name="id" value="@editId" /> }

                <!-- Hidden General Fields (synced from right panel) -->
                <input type="hidden" name="Name" id="fldName" value="@Model.Name" />
                <input type="hidden" name="Description" id="fldDesc" value="@Model.Description" />
                <input type="hidden" name="DeclarationType" id="fldDeclType" value="@Model.DeclarationType" />
                <input type="hidden" name="ClientId" id="fldClientId" value="@Model.ClientId" />
                <input type="hidden" name="IsDefault" id="fldIsDefault" value="@(Model.IsDefault ? "true" : "false")" />

                <div id="stepsContainer">
                    @if (Model.Steps != null && Model.Steps.Any())
                    {
                        for (int i = 0; i < Model.Steps.Count; i++)
                        {
                            var s = Model.Steps[i];
                            var sColor = s.StepType switch { "Initialisation"=>"#475569", "DemandePieces"=>"#2563eb", "Collecte"=>"#d97706", "Saisie"=>"#0284c7", "Controle"=>"#7c3aed", "ValidationInterne"=>"#059669", "ValidationClient"=>"#d97706", "DepotSoumission"=>"#dc2626", "ClotureArchivage"=>"#475569", _=>"#94a3b8" };
                            var sBg = s.StepType switch { "Initialisation"=>"#f1f5f9", "DemandePieces"=>"#dbeafe", "Collecte"=>"#fef3c7", "Saisie"=>"#e0f2fe", "Controle"=>"#ede9fe", "ValidationInterne"=>"#d1fae5", "ValidationClient"=>"#fef3c7", "DepotSoumission"=>"#fee2e2", "ClotureArchivage"=>"#f1f5f9", _=>"#f1f5f9" };
                            var sIcon = s.StepType switch { "Initialisation"=>"bi-rocket-takeoff-fill", "DemandePieces"=>"bi-envelope-paper-fill", "Collecte"=>"bi-cloud-arrow-up-fill", "Saisie"=>"bi-keyboard-fill", "Controle"=>"bi-search", "ValidationInterne"=>"bi-shield-check", "ValidationClient"=>"bi-person-check", "DepotSoumission"=>"bi-send-fill", "ClotureArchivage"=>"bi-archive-fill", _=>"bi-circle-fill" };

                            <div class="wb-step-card" data-index="@i" id="step-@i" draggable="true" onclick="selectStep(@i)">
                                <div class="wb-step-accent" style="background:@sColor;"></div>
                                <div class="wb-step-header">
                                    <div class="d-flex align-items-center gap-2" style="flex:1; min-width:0;">
                                        <i class="bi bi-grip-vertical text-muted" style="cursor:grab; font-size:0.85rem;"></i>
                                        <div class="wb-step-num" style="background:@sBg; color:@sColor;">@(i+1)</div>
                                        <div class="wb-step-title step-display-name">@s.Name</div>
                                    </div>
                                    <div class="wb-step-actions">
                                        <button type="button" class="btn btn-outline-primary" onclick="event.stopPropagation(); selectStep(@i)" title="Configurer"><i class="bi bi-gear-fill"></i></button>
                                        <button type="button" class="btn btn-outline-danger" onclick="event.stopPropagation(); removeStep(@i)" title="Supprimer"><i class="bi bi-trash3"></i></button>
                                    </div>
                                </div>
                                <div class="wb-step-body">
                                    <div class="wb-step-tags">
                                        <span class="wb-step-tag" style="background:@sBg; color:@sColor;"><i class="bi @sIcon"></i> @s.StepType</span>
                                        <span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-people-fill"></i> @s.AssignmentType</span>
                                        @if (s.SLAHours.HasValue) { <span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-stopwatch"></i> @(s.SLAHours)h</span> }
                                        @if (s.IsRequired) { <span class="wb-step-tag" style="background:#fee2e2; color:#dc2626;"><i class="bi bi-asterisk"></i></span> }
                                        @if (s.IsBlocking) { <span class="wb-step-tag" style="background:#fef3c7; color:#d97706;"><i class="bi bi-lock-fill"></i></span> }
                                        @if (s.IsClientStep) { <span class="wb-step-tag" style="background:#fef3c7; color:#d97706;"><i class="bi bi-person-badge"></i> Client</span> }
                                        @if (s.AutoNotify) { <span class="wb-step-tag" style="background:#d1fae5; color:#059669;"><i class="bi bi-bell-fill"></i></span> }
                                    </div>
                                </div>
                                <!-- Hidden fields for this step -->
                                <input type="hidden" name="Steps[@i].Name" class="h-step-name" value="@s.Name" />
                                <input type="hidden" name="Steps[@i].Description" class="h-step-desc" value="@s.Description" />
                                <input type="hidden" name="Steps[@i].StepOrder" class="h-step-order" value="@s.StepOrder" />
                                <input type="hidden" name="Steps[@i].StepType" class="h-step-type" value="@s.StepType" />
                                <input type="hidden" name="Steps[@i].AssignmentType" class="h-step-assign" value="@s.AssignmentType" />
                                <input type="hidden" name="Steps[@i].AssignedToUserId" class="h-step-userid" value="@s.AssignedToUserId" />
                                <input type="hidden" name="Steps[@i].AssignedToRoleName" class="h-step-role" value="@s.AssignedToRoleName" />
                                <input type="hidden" name="Steps[@i].AssignedToDepartmentId" class="h-step-dept" value="@s.AssignedToDepartmentId" />
                                <input type="hidden" name="Steps[@i].IsClientStep" class="h-step-client" value="@s.IsClientStep.ToString().ToLower()" />
                                <input type="hidden" name="Steps[@i].SLAHours" class="h-step-sla" value="@s.SLAHours" />
                                <input type="hidden" name="Steps[@i].IsRequired" class="h-step-required" value="@s.IsRequired.ToString().ToLower()" />
                                <input type="hidden" name="Steps[@i].IsBlocking" class="h-step-blocking" value="@s.IsBlocking.ToString().ToLower()" />
                                <input type="hidden" name="Steps[@i].AutoCreateTask" class="h-step-task" value="@s.AutoCreateTask.ToString().ToLower()" />
                                <input type="hidden" name="Steps[@i].TaskType" class="h-step-tasktype" value="@s.TaskType" />
                                <input type="hidden" name="Steps[@i].TaskPriority" class="h-step-priority" value="@s.TaskPriority" />
                                <input type="hidden" name="Steps[@i].AutoNotify" class="h-step-notify" value="@s.AutoNotify.ToString().ToLower()" />
                                <input type="hidden" name="Steps[@i].RequiredPermission" class="h-step-perm" value="@s.RequiredPermission" />
                                <input type="hidden" name="Steps[@i].DeclarationStatusOnEntry" class="h-step-statusentry" value="@s.DeclarationStatusOnEntry" />
                                <input type="hidden" name="Steps[@i].DeclarationStatusOnComplete" class="h-step-statuscomp" value="@s.DeclarationStatusOnComplete" />
                                <input type="hidden" name="Steps[@i].ValidationRulesJson" class="h-step-rules" value="@s.ValidationRulesJson" />
                            </div>
                            @if (i < Model.Steps.Count - 1) { <div class="wb-connector"></div> }
                        }
                    }
                    else
                    {
                        <div class="wb-pipeline-empty" id="emptyState">
                            <i class="bi bi-diagram-3"></i>
                            <h6 class="fw-bold">Aucune &#233;tape</h6>
                            <p style="font-size:0.82rem;">Cliquez sur <strong>+ &#201;tape</strong> pour commencer &#224; construire votre workflow, ou s&#233;lectionnez un mod&#232;le existant.</p>
                            <button type="button" class="btn btn-sm btn-mc-primary" onclick="addStep()"><i class="bi bi-plus-circle me-1"></i> Ajouter une &#233;tape</button>
                        </div>
                    }
                </div>

                <!-- SAVE BAR -->
                <div style="padding:0.85rem 1.25rem; border-top:1px solid #e2e8f0; background:#f8fafc; display:flex; justify-content:space-between; align-items:center; flex-shrink:0;">
                    <div style="font-size:0.75rem; color:#94a3b8;" id="stepCountLabel">
                        <i class="bi bi-layers me-1"></i> <span id="stepCountNum">@(Model.Steps?.Count ?? 0)</span> &#233;tape(s)
                    </div>
                    <div class="d-flex gap-2">
                        <a href="@Url.Action("Builder")" class="btn btn-sm btn-outline-secondary" style="border-radius:8px; font-weight:600;">R&#233;initialiser</a>
                        <button type="submit" class="btn btn-sm btn-mc-primary" style="border-radius:8px; font-weight:700; padding:0.35rem 1rem;">
                            <i class="bi bi-check-circle-fill me-1"></i>@(isEditMode ? "Mettre &#224; jour" : "Cr&#233;er le mod&#232;le")
                        </button>
                    </div>
                </div>
            </form>
        </div>
    </div>

    <!-- ═══════════ RIGHT PANEL — Configuration ═══════════ -->
    <div class="wb-panel wb-panel-right">
        <div class="wb-right-header">
            <div class="d-flex justify-content-between align-items-center">
                <h6><i class="bi bi-sliders2 text-primary me-1"></i> Configuration</h6>
                <div class="btn-group btn-group-sm" id="configTabGroup">
                    <button type="button" class="btn btn-outline-primary btn-sm active" style="border-radius:6px 0 0 6px; font-size:0.7rem; font-weight:600;" onclick="showConfigTab('general')">G&#233;n&#233;ral</button>
                    <button type="button" class="btn btn-outline-primary btn-sm" style="border-radius:0 6px 6px 0; font-size:0.7rem; font-weight:600;" onclick="showConfigTab('step')">&#201;tape</button>
                </div>
            </div>
        </div>
        <div class="wb-right-scroll">
            <!-- GENERAL CONFIG TAB -->
            <div id="tabGeneral">
                <div class="wb-config-section">
                    <h6><i class="bi bi-info-circle text-primary"></i> Informations du mod&#232;le</h6>
                    <div class="mb-2">
                        <label class="wb-lbl">Nom du mod&#232;le *</label>
                        <input type="text" class="form-control wb-input" id="cfgName" value="@Model.Name" oninput="syncField('fldName',this.value)" required />
                    </div>
                    <div class="mb-2">
                        <label class="wb-lbl">Description</label>
                        <textarea class="form-control wb-input" id="cfgDesc" rows="2" oninput="syncField('fldDesc',this.value)">@Model.Description</textarea>
                    </div>
                    <div class="row g-2">
                        <div class="col-6">
                            <label class="wb-lbl">Type de d&#233;claration</label>
                            <select class="form-select wb-select" id="cfgDeclType" onchange="syncField('fldDeclType',this.value)">
                                <option value="All" @(Model.DeclarationType == "All" ? "selected" : "")>Tous types</option>
                                @foreach (var t in new[] { "TVA", "IS", "IR", "CNSS", "IR_SALAIRES", "Paie", "Juridique", "Comptabilite" })
                                {
                                    <option value="@t" @(Model.DeclarationType == t ? "selected" : "")>@t</option>
                                }
                            </select>
                        </div>
                        <div class="col-6">
                            <label class="wb-lbl">Client</label>
                            <select class="form-select wb-select" id="cfgClient" onchange="syncField('fldClientId',this.value)">
                                <option value="">&#8212; Global &#8212;</option>
                                @foreach (var c in clients)
                                {
                                    <option value="@c.Id" @(Model.ClientId == c.Id ? "selected" : "")>@c.RaisonSociale</option>
                                }
                            </select>
                        </div>
                    </div>
                    <div class="form-check form-switch wb-switch mt-2">
                        <input class="form-check-input" type="checkbox" id="cfgIsDefault" @(Model.IsDefault ? "checked" : "") onchange="syncField('fldIsDefault',this.checked?'true':'false')" />
                        <label class="form-check-label">Mod&#232;le par d&#233;faut</label>
                    </div>
                </div>

                @if (isEditMode && editTemplate != null)
                {
                    <div class="wb-config-section">
                        <h6><i class="bi bi-bar-chart text-success"></i> Statistiques</h6>
                        <div class="d-flex justify-content-between mb-1" style="font-size:0.78rem;"><span class="text-muted">Version</span><span class="fw-bold">v@editTemplate.Version</span></div>
                        <div class="d-flex justify-content-between mb-1" style="font-size:0.78rem;"><span class="text-muted">Instances</span><span class="fw-bold">@editTemplate.InstanceCount</span></div>
                        <div class="d-flex justify-content-between mb-1" style="font-size:0.78rem;"><span class="text-muted">Cr&#233;&#233; le</span><span class="fw-bold">@editTemplate.CreatedAt.ToString("dd/MM/yyyy HH:mm")</span></div>
                        <div class="d-flex justify-content-between" style="font-size:0.78rem;"><span class="text-muted">Statut</span><span class="fw-bold text-success">@(editTemplate.IsActive ? "Actif" : "Inactif")</span></div>
                    </div>
                }

                <div class="wb-config-section">
                    <h6><i class="bi bi-lightning-charge text-warning"></i> Actions rapides</h6>
                    <div class="d-grid gap-2">
                        <button type="button" class="btn btn-sm btn-outline-primary" style="border-radius:8px; font-size:0.76rem; font-weight:600;" onclick="addDefaultPipeline()"><i class="bi bi-magic me-1"></i> G&#233;n&#233;rer pipeline par d&#233;faut (9 &#233;tapes)</button>
                        <button type="button" class="btn btn-sm btn-outline-secondary" style="border-radius:8px; font-size:0.76rem; font-weight:600;" onclick="clearAllSteps()"><i class="bi bi-trash3 me-1"></i> Supprimer toutes les &#233;tapes</button>
                    </div>
                </div>
            </div>

            <!-- STEP CONFIG TAB (shown when a step is selected) -->
            <div id="tabStep" style="display:none;">
                <div class="wb-config-section" id="stepConfigPanel">
                    <div class="text-center text-muted py-3" style="font-size:0.82rem;" id="noStepSelected">
                        <i class="bi bi-hand-index-thumb-fill d-block" style="font-size:1.5rem; color:#cbd5e1; margin-bottom:0.5rem;"></i>
                        Cliquez sur une &#233;tape dans le pipeline pour la configurer.
                    </div>
                    <div id="stepConfigForm" style="display:none;">
                        <h6 class="mb-3"><i class="bi bi-gear text-primary"></i> &#201;tape <span id="cfgStepNum" class="text-primary"></span></h6>
                        <div class="mb-2">
                            <label class="wb-lbl">Nom *</label>
                            <input type="text" class="form-control wb-input" id="cfgStepName" oninput="updateStepField('name',this.value)" />
                        </div>
                        <div class="mb-2">
                            <label class="wb-lbl">Description</label>
                            <input type="text" class="form-control wb-input" id="cfgStepDesc" oninput="updateStepField('desc',this.value)" />
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-6">
                                <label class="wb-lbl">Type d'&#233;tape</label>
                                <select class="form-select wb-select" id="cfgStepType" onchange="updateStepField('type',this.value)">
                                    @foreach (var st in stepTypes) { <option value="@st">@st</option> }
                                </select>
                            </div>
                            <div class="col-6">
                                <label class="wb-lbl">SLA (heures)</label>
                                <input type="number" class="form-control wb-input" id="cfgStepSLA" oninput="updateStepField('sla',this.value)" />
                            </div>
                        </div>

                        <div class="p-2 rounded mb-2" style="background:#f8fafc; border:1px solid #f1f5f9;">
                            <div class="wb-lbl mb-1"><i class="bi bi-people-fill text-primary me-1"></i>Assignation</div>
                            <div class="row g-2">
                                <div class="col-12">
                                    <select class="form-select wb-select" id="cfgStepAssign" onchange="updateStepField('assign',this.value); toggleAssignFields()">
                                        @foreach (var at in assignmentTypes) { <option value="@at">@at</option> }
                                    </select>
                                </div>
                                <div class="col-12" id="assignUserField" style="display:none;">
                                    <input type="text" class="form-control wb-input" id="cfgStepUserId" placeholder="User ID" oninput="updateStepField('userid',this.value)" />
                                </div>
                                <div class="col-12" id="assignRoleField" style="display:none;">
                                    <select class="form-select wb-select" id="cfgStepRole" onchange="updateStepField('role',this.value)">
                                        <option value="">--</option>
                                        @foreach (var r in roles) { <option value="@r">@r</option> }
                                    </select>
                                </div>
                                <div class="col-12" id="assignDeptField" style="display:none;">
                                    <select class="form-select wb-select" id="cfgStepDept" onchange="updateStepField('dept',this.value)">
                                        <option value="">--</option>
                                        @foreach (var d in departments) { <option value="@d.Id">@d.Name</option> }
                                    </select>
                                </div>
                            </div>
                        </div>

                        <div class="p-2 rounded mb-2" style="background:#fefbf3; border:1px solid #fef3c7;">
                            <div class="wb-lbl mb-1"><i class="bi bi-sliders text-warning me-1"></i>R&#232;gles</div>
                            <div class="row g-1">
                                <div class="col-6"><div class="form-check form-switch wb-switch"><input class="form-check-input" type="checkbox" id="cfgStepRequired" onchange="updateStepField('required',this.checked)" /><label class="form-check-label">Requis</label></div></div>
                                <div class="col-6"><div class="form-check form-switch wb-switch"><input class="form-check-input" type="checkbox" id="cfgStepBlocking" onchange="updateStepField('blocking',this.checked)" /><label class="form-check-label">Bloquant</label></div></div>
                                <div class="col-6"><div class="form-check form-switch wb-switch"><input class="form-check-input" type="checkbox" id="cfgStepTask" onchange="updateStepField('task',this.checked)" /><label class="form-check-label">Auto-t&#226;che</label></div></div>
                                <div class="col-6"><div class="form-check form-switch wb-switch"><input class="form-check-input" type="checkbox" id="cfgStepNotify" onchange="updateStepField('notify',this.checked)" /><label class="form-check-label">Notifier</label></div></div>
                                <div class="col-6"><div class="form-check form-switch wb-switch"><input class="form-check-input" type="checkbox" id="cfgStepClientStep" onchange="updateStepField('clientstep',this.checked)" /><label class="form-check-label">&#201;tape client</label></div></div>
                            </div>
                        </div>

                        <div class="row g-2 mb-2">
                            <div class="col-6">
                                <label class="wb-lbl">Type de t&#226;che</label>
                                <select class="form-select wb-select" id="cfgStepTaskType" onchange="updateStepField('tasktype',this.value)">
                                    @foreach (var tt in taskTypes) { <option value="@tt">@tt</option> }
                                </select>
                            </div>
                            <div class="col-6">
                                <label class="wb-lbl">Priorit&#233;</label>
                                <select class="form-select wb-select" id="cfgStepPriority" onchange="updateStepField('priority',this.value)">
                                    @foreach (var p in priorities) { <option value="@p">@p</option> }
                                </select>
                            </div>
                        </div>
                        <div class="row g-2">
                            <div class="col-6">
                                <label class="wb-lbl">Statut entr&#233;e</label>
                                <input type="text" class="form-control wb-input" id="cfgStepStatusEntry" oninput="updateStepField('statusentry',this.value)" />
                            </div>
                            <div class="col-6">
                                <label class="wb-lbl">Statut compl&#233;tion</label>
                                <input type="text" class="form-control wb-input" id="cfgStepStatusComp" oninput="updateStepField('statuscomp',this.value)" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
<script>
    var selectedStepIdx = -1;
    var stepIndex = @(Model.Steps?.Count ?? 0);

    var stepTypeInfo = {
        'Initialisation':   { color:'#475569', bg:'#f1f5f9', icon:'bi-rocket-takeoff-fill' },
        'DemandePieces':    { color:'#2563eb', bg:'#dbeafe', icon:'bi-envelope-paper-fill' },
        'Collecte':         { color:'#d97706', bg:'#fef3c7', icon:'bi-cloud-arrow-up-fill' },
        'Saisie':           { color:'#0284c7', bg:'#e0f2fe', icon:'bi-keyboard-fill' },
        'Controle':         { color:'#7c3aed', bg:'#ede9fe', icon:'bi-search' },
        'ValidationInterne':{ color:'#059669', bg:'#d1fae5', icon:'bi-shield-check' },
        'ValidationClient': { color:'#d97706', bg:'#fef3c7', icon:'bi-person-check' },
        'DepotSoumission':  { color:'#dc2626', bg:'#fee2e2', icon:'bi-send-fill' },
        'ClotureArchivage': { color:'#475569', bg:'#f1f5f9', icon:'bi-archive-fill' }
    };

    var defaultSteps = [
        { name:'Initialisation', type:'Initialisation', assign:'AutoSystem', sla:2 },
        { name:'Demande de pi\u00e8ces', type:'DemandePieces', assign:'Employee', sla:24 },
        { name:'Collecte documents', type:'Collecte', assign:'Client', sla:72, clientStep:true },
        { name:'Saisie comptable', type:'Saisie', assign:'Employee', sla:48 },
        { name:'Contr\u00f4le qualit\u00e9', type:'Controle', assign:'Role', sla:24, role:'Manager' },
        { name:'Validation interne', type:'ValidationInterne', assign:'Role', sla:24, role:'Gerant' },
        { name:'Validation client', type:'ValidationClient', assign:'Client', sla:48, clientStep:true },
        { name:'D\u00e9p\u00f4t / Soumission', type:'DepotSoumission', assign:'Employee', sla:24 },
        { name:'Cl\u00f4ture & Archivage', type:'ClotureArchivage', assign:'AutoSystem', sla:4 }
    ];

    // ── Sync general fields ──
    function syncField(id, val) { document.getElementById(id).value = val; }

    // ── Template search ──
    document.getElementById('searchTemplates').addEventListener('input', function() {
        var q = this.value.toLowerCase();
        document.querySelectorAll('#templateList .wb-tpl-item[data-search]').forEach(function(el) {
            el.style.display = el.dataset.search.indexOf(q) >= 0 ? '' : 'none';
        });
    });

    // ── Config tabs ──
    function showConfigTab(tab) {
        document.getElementById('tabGeneral').style.display = tab === 'general' ? '' : 'none';
        document.getElementById('tabStep').style.display = tab === 'step' ? '' : 'none';
        var btns = document.querySelectorAll('#configTabGroup .btn');
        btns.forEach(function(b, i) { b.classList.toggle('active', (i === 0 && tab === 'general') || (i === 1 && tab === 'step')); });
    }

    // ── Select step ──
    function selectStep(idx) {
        selectedStepIdx = idx;
        document.querySelectorAll('.wb-step-card').forEach(function(c) { c.classList.remove('selected'); });
        var card = document.getElementById('step-' + idx);
        if (!card) return;
        card.classList.add('selected');
        showConfigTab('step');
        document.getElementById('noStepSelected').style.display = 'none';
        document.getElementById('stepConfigForm').style.display = '';
        document.getElementById('cfgStepNum').textContent = '#' + (idx + 1);

        // Load values from hidden fields
        document.getElementById('cfgStepName').value = card.querySelector('.h-step-name').value;
        document.getElementById('cfgStepDesc').value = card.querySelector('.h-step-desc').value;
        document.getElementById('cfgStepType').value = card.querySelector('.h-step-type').value;
        document.getElementById('cfgStepSLA').value = card.querySelector('.h-step-sla').value;
        document.getElementById('cfgStepAssign').value = card.querySelector('.h-step-assign').value;
        document.getElementById('cfgStepUserId').value = card.querySelector('.h-step-userid').value;
        document.getElementById('cfgStepRole').value = card.querySelector('.h-step-role').value;
        document.getElementById('cfgStepDept').value = card.querySelector('.h-step-dept').value;
        document.getElementById('cfgStepRequired').checked = card.querySelector('.h-step-required').value === 'true';
        document.getElementById('cfgStepBlocking').checked = card.querySelector('.h-step-blocking').value === 'true';
        document.getElementById('cfgStepTask').checked = card.querySelector('.h-step-task').value === 'true';
        document.getElementById('cfgStepNotify').checked = card.querySelector('.h-step-notify').value === 'true';
        document.getElementById('cfgStepClientStep').checked = card.querySelector('.h-step-client').value === 'true';
        document.getElementById('cfgStepTaskType').value = card.querySelector('.h-step-tasktype').value || 'Standard';
        document.getElementById('cfgStepPriority').value = card.querySelector('.h-step-priority').value || 'Moyenne';
        document.getElementById('cfgStepStatusEntry').value = card.querySelector('.h-step-statusentry').value;
        document.getElementById('cfgStepStatusComp').value = card.querySelector('.h-step-statuscomp').value;
        toggleAssignFields();
    }

    function toggleAssignFields() {
        var v = document.getElementById('cfgStepAssign').value;
        document.getElementById('assignUserField').style.display = v === 'Employee' ? '' : 'none';
        document.getElementById('assignRoleField').style.display = v === 'Role' ? '' : 'none';
        document.getElementById('assignDeptField').style.display = v === 'Department' ? '' : 'none';
    }

    // ── Update step field from config panel ──
    function updateStepField(field, val) {
        if (selectedStepIdx < 0) return;
        var card = document.getElementById('step-' + selectedStepIdx);
        if (!card) return;
        var map = {
            'name': '.h-step-name', 'desc': '.h-step-desc', 'type': '.h-step-type',
            'sla': '.h-step-sla', 'assign': '.h-step-assign', 'userid': '.h-step-userid',
            'role': '.h-step-role', 'dept': '.h-step-dept', 'required': '.h-step-required',
            'blocking': '.h-step-blocking', 'task': '.h-step-task', 'notify': '.h-step-notify',
            'clientstep': '.h-step-client', 'tasktype': '.h-step-tasktype', 'priority': '.h-step-priority',
            'statusentry': '.h-step-statusentry', 'statuscomp': '.h-step-statuscomp', 'perm': '.h-step-perm'
        };
        var sel = map[field];
        if (!sel) return;
        var el = card.querySelector(sel);
        if (typeof val === 'boolean') val = val ? 'true' : 'false';
        if (el) el.value = val;

        // Update visual display
        if (field === 'name') { card.querySelector('.step-display-name').textContent = val; }
        if (field === 'type') { refreshStepVisual(card, val); }
    }

    function refreshStepVisual(card, stepType) {
        var info = stepTypeInfo[stepType] || { color:'#94a3b8', bg:'#f1f5f9', icon:'bi-circle-fill' };
        card.querySelector('.wb-step-accent').style.background = info.color;
        var numEl = card.querySelector('.wb-step-num');
        numEl.style.background = info.bg;
        numEl.style.color = info.color;
        // Rebuild tags
        rebuildStepTags(card);
    }

    function rebuildStepTags(card) {
        var type = card.querySelector('.h-step-type').value;
        var assign = card.querySelector('.h-step-assign').value;
        var sla = card.querySelector('.h-step-sla').value;
        var req = card.querySelector('.h-step-required').value === 'true';
        var blk = card.querySelector('.h-step-blocking').value === 'true';
        var cli = card.querySelector('.h-step-client').value === 'true';
        var ntf = card.querySelector('.h-step-notify').value === 'true';
        var info = stepTypeInfo[type] || { color:'#94a3b8', bg:'#f1f5f9', icon:'bi-circle-fill' };
        var html = '';
        html += '<span class="wb-step-tag" style="background:' + info.bg + '; color:' + info.color + ';"><i class="bi ' + info.icon + '"></i> ' + type + '</span>';
        html += '<span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-people-fill"></i> ' + assign + '</span>';
        if (sla) html += '<span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-stopwatch"></i> ' + sla + 'h</span>';
        if (req) html += '<span class="wb-step-tag" style="background:#fee2e2; color:#dc2626;"><i class="bi bi-asterisk"></i></span>';
        if (blk) html += '<span class="wb-step-tag" style="background:#fef3c7; color:#d97706;"><i class="bi bi-lock-fill"></i></span>';
        if (cli) html += '<span class="wb-step-tag" style="background:#fef3c7; color:#d97706;"><i class="bi bi-person-badge"></i> Client</span>';
        if (ntf) html += '<span class="wb-step-tag" style="background:#d1fae5; color:#059669;"><i class="bi bi-bell-fill"></i></span>';
        card.querySelector('.wb-step-tags').innerHTML = html;
    }

    // ── Add step ──
    function addStep(cfg) {
        cfg = cfg || { name: 'Nouvelle \u00e9tape', type: 'Saisie', assign: 'Employee', sla: '' };
        var c = document.getElementById('stepsContainer');
        var emp = document.getElementById('emptyState');
        if (emp) emp.remove();

        var i = stepIndex;
        var info = stepTypeInfo[cfg.type] || { color:'#94a3b8', bg:'#f1f5f9', icon:'bi-circle-fill' };

        // Add connector if not first
        if (c.querySelectorAll('.wb-step-card').length > 0) {
            var conn = document.createElement('div');
            conn.className = 'wb-connector';
            conn.id = 'conn-' + i;
            c.appendChild(conn);
        }

        var div = document.createElement('div');
        div.className = 'wb-step-card';
        div.id = 'step-' + i;
        div.dataset.index = i;
        div.setAttribute('draggable', 'true');
        div.setAttribute('onclick', 'selectStep(' + i + ')');

        var tags = '';
        tags += '<span class="wb-step-tag" style="background:' + info.bg + '; color:' + info.color + ';"><i class="bi ' + info.icon + '"></i> ' + cfg.type + '</span>';
        tags += '<span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-people-fill"></i> ' + cfg.assign + '</span>';
        if (cfg.sla) tags += '<span class="wb-step-tag" style="background:#f1f5f9; color:#475569;"><i class="bi bi-stopwatch"></i> ' + cfg.sla + 'h</span>';
        if (cfg.clientStep) tags += '<span class="wb-step-tag" style="background:#fef3c7; color:#d97706;"><i class="bi bi-person-badge"></i> Client</span>';

        div.innerHTML = '<div class="wb-step-accent" style="background:' + info.color + ';"></div>'
            + '<div class="wb-step-header">'
            + '<div class="d-flex align-items-center gap-2" style="flex:1; min-width:0;">'
            + '<i class="bi bi-grip-vertical text-muted" style="cursor:grab; font-size:0.85rem;"></i>'
            + '<div class="wb-step-num" style="background:' + info.bg + '; color:' + info.color + ';">' + (i+1) + '</div>'
            + '<div class="wb-step-title step-display-name">' + cfg.name + '</div>'
            + '</div>'
            + '<div class="wb-step-actions">'
            + '<button type="button" class="btn btn-outline-primary" onclick="event.stopPropagation(); selectStep(' + i + ')" title="Configurer"><i class="bi bi-gear-fill"></i></button>'
            + '<button type="button" class="btn btn-outline-danger" onclick="event.stopPropagation(); removeStep(' + i + ')" title="Supprimer"><i class="bi bi-trash3"></i></button>'
            + '</div></div>'
            + '<div class="wb-step-body"><div class="wb-step-tags">' + tags + '</div></div>'
            + '<input type="hidden" name="Steps[' + i + '].Name" class="h-step-name" value="' + cfg.name + '" />'
            + '<input type="hidden" name="Steps[' + i + '].Description" class="h-step-desc" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].StepOrder" class="h-step-order" value="' + i + '" />'
            + '<input type="hidden" name="Steps[' + i + '].StepType" class="h-step-type" value="' + cfg.type + '" />'
            + '<input type="hidden" name="Steps[' + i + '].AssignmentType" class="h-step-assign" value="' + cfg.assign + '" />'
            + '<input type="hidden" name="Steps[' + i + '].AssignedToUserId" class="h-step-userid" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].AssignedToRoleName" class="h-step-role" value="' + (cfg.role||'') + '" />'
            + '<input type="hidden" name="Steps[' + i + '].AssignedToDepartmentId" class="h-step-dept" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].IsClientStep" class="h-step-client" value="' + (cfg.clientStep ? 'true' : 'false') + '" />'
            + '<input type="hidden" name="Steps[' + i + '].SLAHours" class="h-step-sla" value="' + (cfg.sla||'') + '" />'
            + '<input type="hidden" name="Steps[' + i + '].IsRequired" class="h-step-required" value="true" />'
            + '<input type="hidden" name="Steps[' + i + '].IsBlocking" class="h-step-blocking" value="true" />'
            + '<input type="hidden" name="Steps[' + i + '].AutoCreateTask" class="h-step-task" value="true" />'
            + '<input type="hidden" name="Steps[' + i + '].TaskType" class="h-step-tasktype" value="Standard" />'
            + '<input type="hidden" name="Steps[' + i + '].TaskPriority" class="h-step-priority" value="Moyenne" />'
            + '<input type="hidden" name="Steps[' + i + '].AutoNotify" class="h-step-notify" value="true" />'
            + '<input type="hidden" name="Steps[' + i + '].RequiredPermission" class="h-step-perm" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].DeclarationStatusOnEntry" class="h-step-statusentry" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].DeclarationStatusOnComplete" class="h-step-statuscomp" value="" />'
            + '<input type="hidden" name="Steps[' + i + '].ValidationRulesJson" class="h-step-rules" value="" />';

        c.appendChild(div);
        stepIndex++;
        updateStepCount();
        initDragDrop();
    }

    // ── Remove step ──
    function removeStep(idx) {
        var el = document.getElementById('step-' + idx);
        var conn = document.getElementById('conn-' + idx);
        if (el) el.remove();
        if (conn) conn.remove();
        // Also remove trailing connector if needed
        var container = document.getElementById('stepsContainer');
        var first = container.querySelector('.wb-connector');
        if (first && !first.previousElementSibling) first.remove();
        updateStepCount();
        if (selectedStepIdx === idx) {
            selectedStepIdx = -1;
            document.getElementById('noStepSelected').style.display = '';
            document.getElementById('stepConfigForm').style.display = 'none';
        }
    }

    // ── Clear all ──
    function clearAllSteps() {
        if (!confirm('Supprimer toutes les \u00e9tapes ?')) return;
        document.getElementById('stepsContainer').innerHTML = '<div class="wb-pipeline-empty" id="emptyState"><i class="bi bi-diagram-3"></i><h6 class="fw-bold">Aucune \u00e9tape</h6><p style="font-size:0.82rem;">Cliquez sur <strong>+ \u00c9tape</strong> pour commencer.</p><button type="button" class="btn btn-sm btn-mc-primary" onclick="addStep()"><i class="bi bi-plus-circle me-1"></i> Ajouter</button></div>';
        stepIndex = 0;
        updateStepCount();
    }

    // ── Generate default pipeline ──
    function addDefaultPipeline() {
        if (document.querySelectorAll('.wb-step-card').length > 0) {
            if (!confirm('Cela ajoutera 9 \u00e9tapes par d\u00e9faut. Continuer ?')) return;
        }
        clearAllStepsQuiet();
        defaultSteps.forEach(function(s) { addStep(s); });
    }
    function clearAllStepsQuiet() {
        document.getElementById('stepsContainer').innerHTML = '';
        stepIndex = 0;
    }

    // ── Step counter ──
    function updateStepCount() {
        var n = document.querySelectorAll('.wb-step-card').length;
        var el = document.getElementById('stepCountNum');
        if (el) el.textContent = n;
    }

    // ── Drag & Drop ──
    function initDragDrop() {
        document.querySelectorAll('.wb-step-card[draggable]').forEach(function(card) {
            card.removeEventListener('dragstart', onDragStart);
            card.removeEventListener('dragend', onDragEnd);
            card.removeEventListener('dragover', onDragOver);
            card.removeEventListener('drop', onDrop);
            card.addEventListener('dragstart', onDragStart);
            card.addEventListener('dragend', onDragEnd);
            card.addEventListener('dragover', onDragOver);
            card.addEventListener('drop', onDrop);
        });
    }
    var dragSrcEl = null;
    function onDragStart(e) { dragSrcEl = this; this.classList.add('dragging'); e.dataTransfer.effectAllowed = 'move'; }
    function onDragEnd(e) { this.classList.remove('dragging'); document.querySelectorAll('.wb-step-card').forEach(function(c) { c.classList.remove('drag-over'); }); }
    function onDragOver(e) { e.preventDefault(); e.dataTransfer.dropEffect = 'move'; this.classList.add('drag-over'); }
    function onDrop(e) {
        e.stopPropagation();
        this.classList.remove('drag-over');
        if (dragSrcEl !== this) {
            var container = document.getElementById('stepsContainer');
            var allCards = Array.from(container.querySelectorAll('.wb-step-card'));
            var fromIdx = allCards.indexOf(dragSrcEl);
            var toIdx = allCards.indexOf(this);
            if (fromIdx < toIdx) { this.parentNode.insertBefore(dragSrcEl, this.nextSibling); }
            else { this.parentNode.insertBefore(dragSrcEl, this); }
            renumberSteps();
        }
    }
    function renumberSteps() {
        var cards = document.querySelectorAll('#stepsContainer .wb-step-card');
        // Remove all connectors first
        document.querySelectorAll('#stepsContainer .wb-connector').forEach(function(c) { c.remove(); });
        cards.forEach(function(card, idx) {
            card.querySelector('.wb-step-num').textContent = idx + 1;
            card.querySelector('.h-step-order').value = idx;
            // Re-add connector
            if (idx < cards.length - 1) {
                var conn = document.createElement('div');
                conn.className = 'wb-connector';
                card.after(conn);
            }
        });
    }

    // ── Init ──
    initDragDrop();
</script>
}
'''

os.makedirs(os.path.dirname(VIEW_PATH), exist_ok=True)
with open(VIEW_PATH, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"Generated: {VIEW_PATH}")
print(f"Size: {len(content)} bytes")

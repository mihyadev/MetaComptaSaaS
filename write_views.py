import os

base_path = r'c:\Users\delli\OneDrive\Bureau\MetaComptaSAS\src\MetaCompta.BackOffice\Views\Workflow'

# ── CreateTemplate.cshtml ──
ct = r'''@using MetaCompta.Application.DTOs
@model WorkflowTemplateCreateDto
@{
    ViewData["Title"] = ViewBag.TemplateId != null ? "Modifier le mod\u00e8le" : "Nouveau mod\u00e8le de workflow";
    var clients = ViewBag.Clients as List<ClientDto> ?? new List<ClientDto>();
    var departments = ViewBag.Departments as List<DepartmentDto> ?? new List<DepartmentDto>();
    var isEdit = ViewBag.TemplateId != null;
    var formAction = isEdit ? "EditTemplate" : "CreateTemplate";
    var stepTypes = new[] { "Initialisation", "DemandePieces", "Collecte", "Saisie", "Controle", "ValidationInterne", "ValidationClient", "DepotSoumission", "ClotureArchivage" };
    var assignmentTypes = new[] { "Employee", "Role", "Department", "Client", "AutoSystem" };
    var taskTypes = new[] { "Standard", "DocumentRequest", "DataEntry", "Review", "Validation", "Submission", "Archiving", "ClientAction" };
    var priorities = new[] { "Faible", "Moyenne", "Haute", "Urgente" };
}

<style>
    .ct-header { background:linear-gradient(135deg,#1e3a5f 0%,#2563eb 50%,#7c3aed 100%); border-radius:18px; color:#fff; padding:1.75rem 2.25rem; position:relative; overflow:hidden; }
    .ct-header::before { content:''; position:absolute; top:-50%; right:-15%; width:300px; height:300px; border-radius:50%; background:rgba(255,255,255,0.05); }
    .ct-glass { background:rgba(255,255,255,0.92); backdrop-filter:blur(20px); border:1px solid rgba(255,255,255,0.5); border-radius:16px; box-shadow:0 8px 30px rgba(0,0,0,0.05); }
    .ct-section-title { font-size:0.88rem; font-weight:700; color:#1e293b; display:flex; align-items:center; gap:0.5rem; margin-bottom:1rem; }
    .ct-step-card { background:#fff; border:1px solid #e2e8f0; border-radius:14px; overflow:hidden; transition:all .3s ease; position:relative; }
    .ct-step-card:hover { box-shadow:0 8px 25px rgba(0,0,0,0.06); }
    .ct-step-card .step-accent { position:absolute; top:0; left:0; width:4px; height:100%; border-radius:4px 0 0 4px; }
    .ct-step-header { display:flex; justify-content:space-between; align-items:center; padding:0.75rem 1rem; background:#f8fafc; border-bottom:1px solid #f1f5f9; cursor:pointer; }
    .ct-step-header:hover { background:#f1f5f9; }
    .ct-step-num { width:32px; height:32px; border-radius:8px; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:0.82rem; }
    .ct-step-body { padding:1rem; }
    .ct-badge { display:inline-flex; align-items:center; gap:0.2rem; padding:0.12rem 0.5rem; border-radius:20px; font-size:0.68rem; font-weight:600; }
    .ct-form-label { font-size:0.78rem; font-weight:600; color:#475569; margin-bottom:0.3rem; }
    .ct-form-control { border-radius:10px; border:1.5px solid #e2e8f0; font-size:0.83rem; transition:all .2s; }
    .ct-form-control:focus { border-color:#2563eb; box-shadow:0 0 0 3px rgba(37,99,235,0.1); }
    .ct-form-select { border-radius:10px; border:1.5px solid #e2e8f0; font-size:0.83rem; }
    .ct-form-check .form-check-input { border-radius:6px; }
    .ct-form-check .form-check-label { font-size:0.8rem; color:#475569; font-weight:500; }
    .ct-preview-mini { display:flex; align-items:center; gap:0.5rem; padding:0.75rem; overflow-x:auto; background:#f8fafc; border-radius:10px; }
    .ct-preview-dot { width:28px; height:28px; border-radius:50%; display:flex; align-items:center; justify-content:center; font-size:0.55rem; font-weight:800; color:#fff; flex-shrink:0; }
    .ct-preview-line { width:20px; height:2px; background:#cbd5e1; flex-shrink:0; }
</style>

<!-- HEADER -->
<div class="ct-header mb-4">
    <div class="d-flex justify-content-between align-items-start flex-wrap gap-3" style="position:relative; z-index:2;">
        <div>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-2" style="font-size:0.8rem;">
                    <li class="breadcrumb-item"><a href="@Url.Action("Index")" style="color:rgba(255,255,255,0.7);">Workflow</a></li>
                    <li class="breadcrumb-item"><a href="@Url.Action("Templates")" style="color:rgba(255,255,255,0.7);">Mod\u00e8les</a></li>
                    <li class="breadcrumb-item text-white">@(isEdit ? "Modifier" : "Nouveau")</li>
                </ol>
            </nav>
            <h4 class="fw-bold mb-1"><i class="bi @(isEdit ? "bi-pencil-fill" : "bi-plus-circle-fill") me-2"></i>@ViewData["Title"]</h4>
            <p class="mb-0" style="font-size:0.85rem; opacity:0.8;">D\u00e9finissez les \u00e9tapes, les assignations et les r\u00e8gles du processus</p>
        </div>
        <a asp-action="Templates" class="btn btn-outline-light" style="border-radius:10px; font-weight:600; font-size:0.83rem;"><i class="bi bi-arrow-left me-1"></i> Retour</a>
    </div>
</div>

<form asp-action="@formAction" method="post" id="templateForm">
    @Html.AntiForgeryToken()
    @if (isEdit) { <input type="hidden" name="id" value="@ViewBag.TemplateId" /> }

    <!-- GENERAL INFO -->
    <div class="ct-glass p-4 mb-4">
        <div class="ct-section-title"><i class="bi bi-info-circle text-primary"></i> Informations g\u00e9n\u00e9rales</div>
        <div class="row g-3">
            <div class="col-md-6">
                <label class="ct-form-label">Nom du mod\u00e8le <span class="text-danger">*</span></label>
                <input asp-for="Name" class="form-control ct-form-control" placeholder="Ex: Processus TVA standard" required />
            </div>
            <div class="col-md-3">
                <label class="ct-form-label">Type de d\u00e9claration</label>
                <select asp-for="DeclarationType" class="form-select ct-form-select" id="declType">
                    <option value="All">Tous types</option>
                    @foreach (var t in new[] { "TVA", "IS", "IR", "CNSS", "IR_SALAIRES", "Paie", "Juridique", "Comptabilite" })
                    {
                        <option value="@t">@t</option>
                    }
                </select>
            </div>
            <div class="col-md-3">
                <label class="ct-form-label">Client sp\u00e9cifique</label>
                <select asp-for="ClientId" class="form-select ct-form-select">
                    <option value="">&#8212; Global (tous clients) &#8212;</option>
                    @foreach (var c in clients)
                    {
                        <option value="@c.Id">@c.RaisonSociale</option>
                    }
                </select>
            </div>
            <div class="col-12">
                <label class="ct-form-label">Description</label>
                <textarea asp-for="Description" class="form-control ct-form-control" rows="2" placeholder="Description du processus..."></textarea>
            </div>
            <div class="col-md-3">
                <div class="form-check form-switch mt-2 ct-form-check">
                    <input asp-for="IsDefault" class="form-check-input" type="checkbox" />
                    <label class="form-check-label">Mod\u00e8le par d\u00e9faut</label>
                </div>
            </div>
        </div>
    </div>

    <!-- MINI PREVIEW -->
    <div class="ct-glass px-4 py-3 mb-4">
        <div class="d-flex justify-content-between align-items-center mb-2">
            <div class="ct-section-title mb-0"><i class="bi bi-eye text-primary"></i> Aper\u00e7u du pipeline</div>
            <span class="ct-badge" style="background:#f1f5f9; color:#475569;" id="stepCountBadge">0 \u00e9tapes</span>
        </div>
        <div class="ct-preview-mini" id="pipelinePreview">
            <div class="text-muted" style="font-size:0.78rem;">Ajoutez des \u00e9tapes ci-dessous...</div>
        </div>
    </div>

    <!-- STEPS -->
    <div class="ct-glass p-4 mb-4">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div class="ct-section-title mb-0"><i class="bi bi-list-ol text-primary"></i> \u00c9tapes du workflow</div>
            <button type="button" class="btn btn-sm btn-mc-primary" style="border-radius:10px; font-weight:600;" onclick="addStep()">
                <i class="bi bi-plus-circle me-1"></i> Ajouter une \u00e9tape
            </button>
        </div>
        <div id="stepsContainer">
            @if (Model.Steps != null && Model.Steps.Any())
            {
                for (int i = 0; i < Model.Steps.Count; i++)
                {
                    var s = Model.Steps[i];
                    <div class="ct-step-card mb-3" data-index="@i" id="step-@i">
                        <div class="step-accent" style="background:#94a3b8;"></div>
                        <div class="ct-step-header" onclick="toggleStep(@i)">
                            <div class="d-flex align-items-center gap-2">
                                <div class="ct-step-num" style="background:#f1f5f9; color:#475569;">@(i+1)</div>
                                <div>
                                    <div class="fw-bold" style="font-size:0.85rem;">@(string.IsNullOrEmpty(s.Name) ? $"\u00c9tape {i+1}" : s.Name)</div>
                                    <div style="font-size:0.7rem; color:#94a3b8;">@s.StepType &middot; @s.AssignmentType</div>
                                </div>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <button type="button" class="btn btn-sm btn-outline-danger" style="border-radius:6px;" onclick="event.stopPropagation(); removeStep(@i)"><i class="bi bi-trash3"></i></button>
                                <i class="bi bi-chevron-down" id="chevron-@i" style="transition:transform .3s;"></i>
                            </div>
                        </div>
                        <div class="ct-step-body" id="body-@i">
                            <div class="row g-3">
                                <div class="col-md-5">
                                    <label class="ct-form-label">Nom *</label>
                                    <input name="Steps[@i].Name" class="form-control ct-form-control step-name-input" value="@s.Name" required data-index="@i" />
                                </div>
                                <div class="col-md-3">
                                    <label class="ct-form-label">Type</label>
                                    <select name="Steps[@i].StepType" class="form-select ct-form-select step-type-select" data-index="@i">
                                        @foreach (var t in stepTypes) { <option value="@t" selected="@(s.StepType == t)">@t</option> }
                                    </select>
                                </div>
                                <div class="col-md-2">
                                    <label class="ct-form-label">Ordre</label>
                                    <input name="Steps[@i].StepOrder" type="number" class="form-control ct-form-control" value="@s.StepOrder" />
                                </div>
                                <div class="col-md-2">
                                    <label class="ct-form-label">SLA (h)</label>
                                    <input name="Steps[@i].SLAHours" type="number" class="form-control ct-form-control" value="@s.SLAHours" />
                                </div>
                                <div class="col-12">
                                    <label class="ct-form-label">Description</label>
                                    <input name="Steps[@i].Description" class="form-control ct-form-control" value="@s.Description" placeholder="Ce qui se passe..." />
                                </div>
                                <div class="col-12">
                                    <div class="p-3 rounded-3" style="background:#f8fafc; border:1px solid #f1f5f9;">
                                        <div class="ct-form-label mb-2"><i class="bi bi-people-fill text-primary me-1"></i> Assignation</div>
                                        <div class="row g-2">
                                            <div class="col-md-3">
                                                <select name="Steps[@i].AssignmentType" class="form-select ct-form-select" onchange="toggleAssignFields(this, @i)">
                                                    @foreach (var a in assignmentTypes) { <option value="@a" selected="@(s.AssignmentType == a)">@a</option> }
                                                </select>
                                            </div>
                                            <div class="col-md-3" id="assignUser-@i" style="@(s.AssignmentType == "Employee" ? "" : "display:none;")">
                                                <input name="Steps[@i].AssignedToUserId" class="form-control ct-form-control" value="@s.AssignedToUserId" placeholder="User ID" />
                                            </div>
                                            <div class="col-md-3" id="assignRole-@i" style="@(s.AssignmentType == "Role" ? "" : "display:none;")">
                                                <select name="Steps[@i].AssignedToRoleName" class="form-select ct-form-select">
                                                    <option value="">&#8212;</option>
                                                    @foreach (var r in new[] { "Gerant", "Manager", "Collaborateur", "Assistant", "Stagiaire" }) { <option value="@r" selected="@(s.AssignedToRoleName == r)">@r</option> }
                                                </select>
                                            </div>
                                            <div class="col-md-3" id="assignDept-@i" style="@(s.AssignmentType == "Department" ? "" : "display:none;")">
                                                <select name="Steps[@i].AssignedToDepartmentId" class="form-select ct-form-select">
                                                    <option value="">&#8212;</option>
                                                    @foreach (var d in departments) { <option value="@d.Id" selected="@(s.AssignedToDepartmentId == d.Id)">@d.Name</option> }
                                                </select>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-check form-switch ct-form-check mt-1">
                                                    <input name="Steps[@i].IsClientStep" type="checkbox" class="form-check-input" value="true" @(s.IsClientStep ? "checked" : "") />
                                                    <label class="form-check-label">Client</label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="p-3 rounded-3" style="background:#fefbf3; border:1px solid #fef3c7;">
                                        <div class="ct-form-label mb-2"><i class="bi bi-sliders text-warning me-1"></i> R\u00e8gles</div>
                                        <div class="row g-2">
                                            <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].IsRequired" type="checkbox" class="form-check-input" value="true" @(s.IsRequired ? "checked" : "") /><label class="form-check-label">Requis</label></div></div>
                                            <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].IsBlocking" type="checkbox" class="form-check-input" value="true" @(s.IsBlocking ? "checked" : "") /><label class="form-check-label">Bloquant</label></div></div>
                                            <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].AutoCreateTask" type="checkbox" class="form-check-input" value="true" @(s.AutoCreateTask ? "checked" : "") /><label class="form-check-label">T\u00e2che</label></div></div>
                                            <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].AutoNotify" type="checkbox" class="form-check-input" value="true" @(s.AutoNotify ? "checked" : "") /><label class="form-check-label">Notif</label></div></div>
                                            <div class="col-md-2"><select name="Steps[@i].TaskType" class="form-select ct-form-select" style="font-size:0.78rem;">@foreach (var tt in taskTypes) { <option value="@tt" selected="@(s.TaskType == tt)">@tt</option> }</select></div>
                                            <div class="col-md-2"><select name="Steps[@i].TaskPriority" class="form-select ct-form-select" style="font-size:0.78rem;">@foreach (var p in priorities) { <option value="@p" selected="@(s.TaskPriority == p)">@p</option> }</select></div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <label class="ct-form-label">Statut entr\u00e9e</label>
                                    <input name="Steps[@i].DeclarationStatusOnEntry" class="form-control ct-form-control" value="@s.DeclarationStatusOnEntry" placeholder="Ex: ACompleter" />
                                </div>
                                <div class="col-md-6">
                                    <label class="ct-form-label">Statut compl\u00e9tion</label>
                                    <input name="Steps[@i].DeclarationStatusOnComplete" class="form-control ct-form-control" value="@s.DeclarationStatusOnComplete" placeholder="Ex: Validee" />
                                </div>
                            </div>
                        </div>
                    </div>
                }
            }
        </div>
        <div id="noSteps" class="text-center py-5 @(Model.Steps?.Any() == true ? "d-none" : "")" style="border:2px dashed #e2e8f0; border-radius:14px;">
            <i class="bi bi-diagram-3" style="font-size:3rem; color:#cbd5e1;"></i>
            <h6 class="fw-bold mt-2" style="color:#1e293b;">Aucune \u00e9tape</h6>
            <p class="text-muted mb-3" style="font-size:0.82rem;">Cliquez sur Ajouter pour construire votre workflow</p>
            <button type="button" class="btn btn-mc-primary" style="border-radius:10px;" onclick="addStep()"><i class="bi bi-plus-circle me-1"></i> Premi\u00e8re \u00e9tape</button>
        </div>
    </div>

    <div class="d-flex gap-2 justify-content-end">
        <a asp-action="Templates" class="btn btn-outline-secondary" style="border-radius:10px; padding:0.5rem 1.5rem; font-weight:600;">Annuler</a>
        <button type="submit" class="btn btn-mc-primary" style="border-radius:10px; padding:0.5rem 1.5rem; font-weight:700;"><i class="bi bi-check-circle-fill me-2"></i>@(isEdit ? "Mettre \u00e0 jour" : "Cr\u00e9er le mod\u00e8le")</button>
    </div>
</form>

@section Scripts {
<script>
    let stepIndex = @(Model.Steps?.Count ?? 0);
    const stepTypesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(stepTypes));
    const assignmentTypesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(assignmentTypes));
    const taskTypesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(taskTypes));
    const prioritiesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(priorities));
    const deptOptions = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(departments.Select(d => new { d.Id, d.Name, d.Code })));
    const roleOptions = ['Gerant','Manager','Collaborateur','Assistant','Stagiaire'];
    const stepColorMap = {'Initialisation':'#475569','DemandePieces':'#2563eb','Collecte':'#d97706','Saisie':'#0284c7','Controle':'#7c3aed','ValidationInterne':'#059669','ValidationClient':'#d97706','DepotSoumission':'#dc2626','ClotureArchivage':'#475569'};

    function addStep() {
        document.getElementById('noSteps')?.classList.add('d-none');
        const c = document.getElementById('stepsContainer');
        const i = stepIndex;
        let html = '<div class="ct-step-card mb-3" data-index="' + i + '" id="step-' + i + '">';
        html += '<div class="step-accent" style="background:#94a3b8;"></div>';
        html += '<div class="ct-step-header" onclick="toggleStep(' + i + ')">';
        html += '<div class="d-flex align-items-center gap-2"><div class="ct-step-num" style="background:#f1f5f9; color:#475569;">' + (i+1) + '</div><div><div class="fw-bold" style="font-size:0.85rem;" id="stepTitle-' + i + '">\u00c9tape ' + (i+1) + '</div><div style="font-size:0.7rem; color:#94a3b8;">Initialisation</div></div></div>';
        html += '<div class="d-flex align-items-center gap-2"><button type="button" class="btn btn-sm btn-outline-danger" style="border-radius:6px;" onclick="event.stopPropagation(); removeStep(' + i + ')"><i class="bi bi-trash3"></i></button><i class="bi bi-chevron-down" id="chevron-' + i + '"></i></div>';
        html += '</div>';
        html += '<div class="ct-step-body" id="body-' + i + '"><div class="row g-3">';
        html += '<div class="col-md-5"><label class="ct-form-label">Nom *</label><input name="Steps[' + i + '].Name" class="form-control ct-form-control step-name-input" required data-index="' + i + '" oninput="updatePreview()" /></div>';
        html += '<div class="col-md-3"><label class="ct-form-label">Type</label><select name="Steps[' + i + '].StepType" class="form-select ct-form-select step-type-select" data-index="' + i + '" onchange="updatePreview()">' + stepTypesArr.map(function(t){return '<option value="'+t+'">'+t+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-2"><label class="ct-form-label">Ordre</label><input name="Steps[' + i + '].StepOrder" type="number" class="form-control ct-form-control" value="' + i + '" /></div>';
        html += '<div class="col-md-2"><label class="ct-form-label">SLA (h)</label><input name="Steps[' + i + '].SLAHours" type="number" class="form-control ct-form-control" /></div>';
        html += '<div class="col-12"><label class="ct-form-label">Description</label><input name="Steps[' + i + '].Description" class="form-control ct-form-control" placeholder="Ce qui se passe..." /></div>';
        html += '<div class="col-12"><div class="p-3 rounded-3" style="background:#f8fafc; border:1px solid #f1f5f9;"><div class="ct-form-label mb-2"><i class="bi bi-people-fill text-primary me-1"></i> Assignation</div><div class="row g-2">';
        html += '<div class="col-md-3"><select name="Steps[' + i + '].AssignmentType" class="form-select ct-form-select" onchange="toggleAssignFields(this,' + i + ')">' + assignmentTypesArr.map(function(a){return '<option value="'+a+'">'+a+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-3" id="assignUser-' + i + '"><input name="Steps[' + i + '].AssignedToUserId" class="form-control ct-form-control" placeholder="User ID" /></div>';
        html += '<div class="col-md-3" id="assignRole-' + i + '" style="display:none;"><select name="Steps[' + i + '].AssignedToRoleName" class="form-select ct-form-select"><option value="">--</option>' + roleOptions.map(function(r){return '<option value="'+r+'">'+r+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-3" id="assignDept-' + i + '" style="display:none;"><select name="Steps[' + i + '].AssignedToDepartmentId" class="form-select ct-form-select"><option value="">--</option>' + deptOptions.map(function(d){return '<option value="'+d.Id+'">'+d.Name+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-3"><div class="form-check form-switch ct-form-check mt-1"><input name="Steps[' + i + '].IsClientStep" type="checkbox" class="form-check-input" value="true" /><label class="form-check-label">Client</label></div></div>';
        html += '</div></div></div>';
        html += '<div class="col-12"><div class="p-3 rounded-3" style="background:#fefbf3; border:1px solid #fef3c7;"><div class="ct-form-label mb-2"><i class="bi bi-sliders text-warning me-1"></i> R\u00e8gles</div><div class="row g-2">';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].IsRequired" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Requis</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].IsBlocking" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Bloquant</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].AutoCreateTask" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">T\u00e2che</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].AutoNotify" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Notif</label></div></div>';
        html += '<div class="col-md-2"><select name="Steps[' + i + '].TaskType" class="form-select ct-form-select" style="font-size:0.78rem;">' + taskTypesArr.map(function(t){return '<option>'+t+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-2"><select name="Steps[' + i + '].TaskPriority" class="form-select ct-form-select" style="font-size:0.78rem;">' + prioritiesArr.map(function(p){return '<option'+(p==='Moyenne'?' selected':'')+'>'+p+'</option>';}).join('') + '</select></div>';
        html += '</div></div></div>';
        html += '<div class="col-md-6"><label class="ct-form-label">Statut entr\u00e9e</label><input name="Steps[' + i + '].DeclarationStatusOnEntry" class="form-control ct-form-control" placeholder="Ex: ACompleter" /></div>';
        html += '<div class="col-md-6"><label class="ct-form-label">Statut compl\u00e9tion</label><input name="Steps[' + i + '].DeclarationStatusOnComplete" class="form-control ct-form-control" placeholder="Ex: Validee" /></div>';
        html += '</div></div></div>';
        c.insertAdjacentHTML('beforeend', html);
        stepIndex++;
        updatePreview();
    }

    function removeStep(index) {
        var card = document.getElementById('step-' + index);
        if (card) card.remove();
        updatePreview();
        if (!document.querySelectorAll('.ct-step-card').length) document.getElementById('noSteps')?.classList.remove('d-none');
    }

    function toggleStep(index) {
        var body = document.getElementById('body-' + index);
        var chev = document.getElementById('chevron-' + index);
        if (body) { var h = body.style.display === 'none'; body.style.display = h ? '' : 'none'; if (chev) chev.style.transform = h ? '' : 'rotate(-90deg)'; }
    }

    function toggleAssignFields(sel, index) {
        var v = sel.value;
        var u = document.getElementById('assignUser-' + index);
        var r = document.getElementById('assignRole-' + index);
        var d = document.getElementById('assignDept-' + index);
        if (u) u.style.display = v === 'Employee' ? '' : 'none';
        if (r) r.style.display = v === 'Role' ? '' : 'none';
        if (d) d.style.display = v === 'Department' ? '' : 'none';
    }

    function updatePreview() {
        var cards = document.querySelectorAll('.ct-step-card');
        var preview = document.getElementById('pipelinePreview');
        var badge = document.getElementById('stepCountBadge');
        if (!cards.length) { preview.innerHTML = '<div class="text-muted" style="font-size:0.78rem;">Ajoutez des \u00e9tapes...</div>'; badge.textContent = '0 \u00e9tapes'; return; }
        badge.textContent = cards.length + ' \u00e9tape' + (cards.length > 1 ? 's' : '');
        var html = '';
        cards.forEach(function(card, idx) {
            var sel = card.querySelector('.step-type-select');
            var type = sel ? sel.value : 'Initialisation';
            var color = stepColorMap[type] || '#94a3b8';
            if (idx > 0) html += '<div class="ct-preview-line"></div>';
            html += '<div class="ct-preview-dot" style="background:' + color + ';">' + (idx+1) + '</div>';
        });
        preview.innerHTML = html;
    }

    updatePreview();
    document.querySelectorAll('.step-name-input, .step-type-select').forEach(function(el) { el.addEventListener('input', updatePreview); el.addEventListener('change', updatePreview); });
</script>
}
'''

with open(os.path.join(base_path, 'CreateTemplate.cshtml'), 'w', encoding='utf-8') as f:
    f.write(ct)
print(f'CreateTemplate.cshtml written: {len(ct)} chars')

# ── EditTemplate.cshtml ── (reuse CreateTemplate with isEdit context)
et = r'''@using MetaCompta.Application.DTOs
@model WorkflowTemplateCreateDto
@{
    ViewData["Title"] = "Modifier le mod\u00e8le de workflow";
    ViewBag.TemplateId = ViewBag.TemplateId;
}

@await Html.PartialAsync("CreateTemplate", Model)
'''

# Actually, EditTemplate should redirect to CreateTemplate form.
# But since both share the same logic, let's just make EditTemplate reuse the same form.
# The CreateTemplate view already handles isEdit = ViewBag.TemplateId != null

# Let's update the EditTemplate to be minimal - just set ViewBag and use shared form
et2 = r'''@using MetaCompta.Application.DTOs
@model WorkflowTemplateCreateDto
@{
    ViewData["Title"] = "Modifier le mod\u00e8le de workflow";
    var clients = ViewBag.Clients as List<ClientDto> ?? new List<ClientDto>();
    var departments = ViewBag.Departments as List<DepartmentDto> ?? new List<DepartmentDto>();
    var stepTypes = new[] { "Initialisation", "DemandePieces", "Collecte", "Saisie", "Controle", "ValidationInterne", "ValidationClient", "DepotSoumission", "ClotureArchivage" };
    var assignmentTypes = new[] { "Employee", "Role", "Department", "Client", "AutoSystem" };
    var taskTypes = new[] { "Standard", "DocumentRequest", "DataEntry", "Review", "Validation", "Submission", "Archiving", "ClientAction" };
    var priorities = new[] { "Faible", "Moyenne", "Haute", "Urgente" };
    var templateId = (int)ViewBag.TemplateId;
}

<style>
    .ct-header { background:linear-gradient(135deg,#1e3a5f 0%,#2563eb 50%,#7c3aed 100%); border-radius:18px; color:#fff; padding:1.75rem 2.25rem; position:relative; overflow:hidden; }
    .ct-header::before { content:''; position:absolute; top:-50%; right:-15%; width:300px; height:300px; border-radius:50%; background:rgba(255,255,255,0.05); }
    .ct-glass { background:rgba(255,255,255,0.92); backdrop-filter:blur(20px); border:1px solid rgba(255,255,255,0.5); border-radius:16px; box-shadow:0 8px 30px rgba(0,0,0,0.05); }
    .ct-section-title { font-size:0.88rem; font-weight:700; color:#1e293b; display:flex; align-items:center; gap:0.5rem; margin-bottom:1rem; }
    .ct-step-card { background:#fff; border:1px solid #e2e8f0; border-radius:14px; overflow:hidden; transition:all .3s ease; position:relative; }
    .ct-step-card:hover { box-shadow:0 8px 25px rgba(0,0,0,0.06); }
    .ct-step-card .step-accent { position:absolute; top:0; left:0; width:4px; height:100%; border-radius:4px 0 0 4px; }
    .ct-step-header { display:flex; justify-content:space-between; align-items:center; padding:0.75rem 1rem; background:#f8fafc; border-bottom:1px solid #f1f5f9; cursor:pointer; }
    .ct-step-header:hover { background:#f1f5f9; }
    .ct-step-num { width:32px; height:32px; border-radius:8px; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:0.82rem; }
    .ct-step-body { padding:1rem; }
    .ct-badge { display:inline-flex; align-items:center; gap:0.2rem; padding:0.12rem 0.5rem; border-radius:20px; font-size:0.68rem; font-weight:600; }
    .ct-form-label { font-size:0.78rem; font-weight:600; color:#475569; margin-bottom:0.3rem; }
    .ct-form-control { border-radius:10px; border:1.5px solid #e2e8f0; font-size:0.83rem; transition:all .2s; }
    .ct-form-control:focus { border-color:#2563eb; box-shadow:0 0 0 3px rgba(37,99,235,0.1); }
    .ct-form-select { border-radius:10px; border:1.5px solid #e2e8f0; font-size:0.83rem; }
    .ct-form-check .form-check-input { border-radius:6px; }
    .ct-form-check .form-check-label { font-size:0.8rem; color:#475569; font-weight:500; }
    .ct-preview-mini { display:flex; align-items:center; gap:0.5rem; padding:0.75rem; overflow-x:auto; background:#f8fafc; border-radius:10px; }
    .ct-preview-dot { width:28px; height:28px; border-radius:50%; display:flex; align-items:center; justify-content:center; font-size:0.55rem; font-weight:800; color:#fff; flex-shrink:0; }
    .ct-preview-line { width:20px; height:2px; background:#cbd5e1; flex-shrink:0; }
</style>

<!-- HEADER -->
<div class="ct-header mb-4">
    <div class="d-flex justify-content-between align-items-start flex-wrap gap-3" style="position:relative; z-index:2;">
        <div>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-2" style="font-size:0.8rem;">
                    <li class="breadcrumb-item"><a href="@Url.Action("Index")" style="color:rgba(255,255,255,0.7);">Workflow</a></li>
                    <li class="breadcrumb-item"><a href="@Url.Action("Templates")" style="color:rgba(255,255,255,0.7);">Mod\u00e8les</a></li>
                    <li class="breadcrumb-item text-white">Modifier</li>
                </ol>
            </nav>
            <h4 class="fw-bold mb-1"><i class="bi bi-pencil-fill me-2"></i>Modifier le mod\u00e8le</h4>
        </div>
        <a asp-action="TemplateDetails" asp-route-id="@templateId" class="btn btn-outline-light" style="border-radius:10px; font-weight:600; font-size:0.83rem;"><i class="bi bi-arrow-left me-1"></i> Retour</a>
    </div>
</div>

<form asp-action="EditTemplate" asp-route-id="@templateId" method="post">
    @Html.AntiForgeryToken()

    <div class="ct-glass p-4 mb-4">
        <div class="ct-section-title"><i class="bi bi-info-circle text-primary"></i> Informations g\u00e9n\u00e9rales</div>
        <div class="row g-3">
            <div class="col-md-6">
                <label class="ct-form-label">Nom du mod\u00e8le *</label>
                <input asp-for="Name" class="form-control ct-form-control" required />
            </div>
            <div class="col-md-3">
                <label class="ct-form-label">Type de d\u00e9claration</label>
                <select asp-for="DeclarationType" class="form-select ct-form-select">
                    <option value="All">Tous types</option>
                    @foreach (var t in new[] { "TVA", "IS", "IR", "CNSS", "IR_SALAIRES", "Paie", "Juridique", "Comptabilite" })
                    {
                        <option value="@t">@t</option>
                    }
                </select>
            </div>
            <div class="col-md-3">
                <label class="ct-form-label">Client</label>
                <select asp-for="ClientId" class="form-select ct-form-select">
                    <option value="">&#8212; Global &#8212;</option>
                    @foreach (var c in clients)
                    {
                        <option value="@c.Id">@c.RaisonSociale</option>
                    }
                </select>
            </div>
            <div class="col-12">
                <label class="ct-form-label">Description</label>
                <textarea asp-for="Description" class="form-control ct-form-control" rows="2"></textarea>
            </div>
            <div class="col-md-3">
                <div class="form-check form-switch mt-2 ct-form-check">
                    <input asp-for="IsDefault" class="form-check-input" type="checkbox" />
                    <label class="form-check-label">Mod\u00e8le par d\u00e9faut</label>
                </div>
            </div>
        </div>
    </div>

    <!-- STEPS -->
    <div class="ct-glass p-4 mb-4">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div class="ct-section-title mb-0"><i class="bi bi-list-ol text-primary"></i> \u00c9tapes du workflow</div>
            <button type="button" class="btn btn-sm btn-mc-primary" style="border-radius:10px; font-weight:600;" onclick="addStep()"><i class="bi bi-plus-circle me-1"></i> Ajouter</button>
        </div>
        <div id="stepsContainer">
            @if (Model.Steps != null)
            {
                for (int i = 0; i < Model.Steps.Count; i++)
                {
                    var s = Model.Steps[i];
                    <div class="ct-step-card mb-3" data-index="@i" id="step-@i">
                        <div class="step-accent" style="background:#94a3b8;"></div>
                        <div class="ct-step-header" onclick="toggleStep(@i)">
                            <div class="d-flex align-items-center gap-2">
                                <div class="ct-step-num" style="background:#f1f5f9; color:#475569;">@(i+1)</div>
                                <div><div class="fw-bold" style="font-size:0.85rem;">@s.Name</div><div style="font-size:0.7rem; color:#94a3b8;">@s.StepType</div></div>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <button type="button" class="btn btn-sm btn-outline-danger" style="border-radius:6px;" onclick="event.stopPropagation(); removeStep(@i)"><i class="bi bi-trash3"></i></button>
                                <i class="bi bi-chevron-down" id="chevron-@i"></i>
                            </div>
                        </div>
                        <div class="ct-step-body" id="body-@i">
                            <div class="row g-3">
                                <div class="col-md-4"><label class="ct-form-label">Nom</label><input name="Steps[@i].Name" class="form-control ct-form-control" value="@s.Name" required /></div>
                                <div class="col-md-3"><label class="ct-form-label">Type</label><select name="Steps[@i].StepType" class="form-select ct-form-select">@foreach (var t in stepTypes) { <option value="@t" selected="@(s.StepType == t)">@t</option> }</select></div>
                                <div class="col-md-2"><label class="ct-form-label">Ordre</label><input name="Steps[@i].StepOrder" type="number" class="form-control ct-form-control" value="@s.StepOrder" /></div>
                                <div class="col-md-3"><label class="ct-form-label">Assignation</label><select name="Steps[@i].AssignmentType" class="form-select ct-form-select" onchange="toggleAssignFields(this, @i)">@foreach (var a in assignmentTypes) { <option value="@a" selected="@(s.AssignmentType == a)">@a</option> }</select></div>
                                <div class="col-md-2"><label class="ct-form-label">SLA (h)</label><input name="Steps[@i].SLAHours" type="number" class="form-control ct-form-control" value="@s.SLAHours" /></div>
                                <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].IsRequired" type="checkbox" class="form-check-input" value="true" @(s.IsRequired ? "checked" : "") /><label class="form-check-label">Requis</label></div></div>
                                <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].IsBlocking" type="checkbox" class="form-check-input" value="true" @(s.IsBlocking ? "checked" : "") /><label class="form-check-label">Bloquant</label></div></div>
                                <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].AutoCreateTask" type="checkbox" class="form-check-input" value="true" @(s.AutoCreateTask ? "checked" : "") /><label class="form-check-label">T\u00e2che</label></div></div>
                                <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].AutoNotify" type="checkbox" class="form-check-input" value="true" @(s.AutoNotify ? "checked" : "") /><label class="form-check-label">Notif</label></div></div>
                                <div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[@i].IsClientStep" type="checkbox" class="form-check-input" value="true" @(s.IsClientStep ? "checked" : "") /><label class="form-check-label">Client</label></div></div>
                            </div>
                        </div>
                    </div>
                }
            }
        </div>
    </div>

    <div class="d-flex gap-2 justify-content-end">
        <a asp-action="TemplateDetails" asp-route-id="@templateId" class="btn btn-outline-secondary" style="border-radius:10px; padding:0.5rem 1.5rem; font-weight:600;">Annuler</a>
        <button type="submit" class="btn btn-mc-primary" style="border-radius:10px; padding:0.5rem 1.5rem; font-weight:700;"><i class="bi bi-check-circle-fill me-2"></i>Mettre \u00e0 jour</button>
    </div>
</form>

@section Scripts {
<script>
    let stepIndex = @(Model.Steps?.Count ?? 0);
    const stepTypesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(stepTypes));
    const assignmentTypesArr = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(assignmentTypes));
    const roleOptions = ['Gerant','Manager','Collaborateur','Assistant','Stagiaire'];
    const deptOptions = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(departments.Select(d => new { d.Id, d.Name, d.Code })));

    function addStep() {
        var c = document.getElementById('stepsContainer');
        var i = stepIndex;
        var html = '<div class="ct-step-card mb-3" data-index="' + i + '" id="step-' + i + '">';
        html += '<div class="step-accent" style="background:#94a3b8;"></div>';
        html += '<div class="ct-step-header" onclick="toggleStep(' + i + ')"><div class="d-flex align-items-center gap-2"><div class="ct-step-num" style="background:#f1f5f9; color:#475569;">' + (i+1) + '</div><div class="fw-bold" style="font-size:0.85rem;">\u00c9tape ' + (i+1) + '</div></div><div class="d-flex align-items-center gap-2"><button type="button" class="btn btn-sm btn-outline-danger" style="border-radius:6px;" onclick="event.stopPropagation(); removeStep(' + i + ')"><i class="bi bi-trash3"></i></button></div></div>';
        html += '<div class="ct-step-body" id="body-' + i + '"><div class="row g-3">';
        html += '<div class="col-md-4"><input name="Steps[' + i + '].Name" class="form-control ct-form-control" placeholder="Nom" required /></div>';
        html += '<div class="col-md-3"><select name="Steps[' + i + '].StepType" class="form-select ct-form-select">' + stepTypesArr.map(function(t){return '<option>'+t+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-2"><input name="Steps[' + i + '].StepOrder" type="number" class="form-control ct-form-control" value="' + i + '" /></div>';
        html += '<div class="col-md-3"><select name="Steps[' + i + '].AssignmentType" class="form-select ct-form-select">' + assignmentTypesArr.map(function(t){return '<option>'+t+'</option>';}).join('') + '</select></div>';
        html += '<div class="col-md-2"><input name="Steps[' + i + '].SLAHours" type="number" class="form-control ct-form-control" /></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].IsRequired" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Requis</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].IsBlocking" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Bloquant</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].AutoCreateTask" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">T\u00e2che</label></div></div>';
        html += '<div class="col-md-2"><div class="form-check form-switch ct-form-check"><input name="Steps[' + i + '].AutoNotify" type="checkbox" class="form-check-input" checked value="true" /><label class="form-check-label">Notif</label></div></div>';
        html += '</div></div></div>';
        c.insertAdjacentHTML('beforeend', html);
        stepIndex++;
    }

    function removeStep(idx) { var el = document.getElementById('step-' + idx); if (el) el.remove(); }
    function toggleStep(idx) { var b = document.getElementById('body-' + idx); if (b) b.style.display = b.style.display === 'none' ? '' : 'none'; }
    function toggleAssignFields(sel, idx) {}
</script>
}
'''

with open(os.path.join(base_path, 'EditTemplate.cshtml'), 'w', encoding='utf-8') as f:
    f.write(et2)
print(f'EditTemplate.cshtml written: {len(et2)} chars')

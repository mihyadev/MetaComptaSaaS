import pathlib

BASE = pathlib.Path(r"c:\Users\delli\OneDrive\Bureau\MetaComptaSAS\src\MetaCompta.BackOffice\Views\Cabinet")

# ============================================================
# 1. CreateEmployee.cshtml - Full wizard with all fields
# ============================================================
create_content = """@model MetaCompta.Application.DTOs.CabinetEmployeeCreateDto
@{
    ViewData["Title"] = "Nouvel employé du cabinet";
    var claimsByDomain = ViewBag.ClaimsByDomain as Dictionary<string, List<string>> ?? new Dictionary<string, List<string>>();
    var roles = ViewBag.Roles as List<MetaCompta.Application.DTOs.RoleDto> ?? new List<MetaCompta.Application.DTOs.RoleDto>();
}

<div class="page-header mb-4">
    <div>
        <h2 class="mb-1"><i class="bi bi-person-plus-fill text-primary me-2"></i>Nouvel Employé du Cabinet</h2>
        <nav aria-label="breadcrumb"><ol class="breadcrumb mb-0">
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Home")">Accueil</a></li>
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Cabinet")">Cabinet</a></li>
            <li class="breadcrumb-item active">Nouvel employé</li>
        </ol></nav>
    </div>
    <a href="@Url.Action("Index", "Cabinet")" class="btn btn-outline-secondary rounded-pill px-3">
        <i class="bi bi-arrow-left me-1"></i>Retour
    </a>
</div>

<form method="post" asp-action="CreateEmployee" id="formCreateEmployee">
    @Html.AntiForgeryToken()

    <!-- Wizard Progress -->
    <div class="mc-card mb-4">
        <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center" id="wizardProgress">
                <div class="wizard-step active" data-step="0">
                    <div class="wizard-step-circle"><i class="bi bi-person-vcard"></i></div>
                    <span class="wizard-step-label">Identité & Compte</span>
                </div>
                <div class="wizard-connector"></div>
                <div class="wizard-step" data-step="1">
                    <div class="wizard-step-circle"><i class="bi bi-briefcase"></i></div>
                    <span class="wizard-step-label">Professionnel</span>
                </div>
                <div class="wizard-connector"></div>
                <div class="wizard-step" data-step="2">
                    <div class="wizard-step-circle"><i class="bi bi-shield-lock"></i></div>
                    <span class="wizard-step-label">Rôle & Permissions</span>
                </div>
            </div>
        </div>
    </div>

    <!-- STEP 0: Identity & Account -->
    <div class="wizard-panel" id="step0">
        <div class="row g-4">
            <div class="col-lg-6">
                <div class="mc-card">
                    <div class="card-header"><i class="bi bi-person me-2"></i>Informations Personnelles</div>
                    <div class="card-body">
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label asp-for="FirstName" class="form-label">Prénom <span class="text-danger">*</span></label>
                                <input asp-for="FirstName" class="form-control" placeholder="Prénom" required />
                                <span asp-validation-for="FirstName" class="text-danger small"></span>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="LastName" class="form-label">Nom <span class="text-danger">*</span></label>
                                <input asp-for="LastName" class="form-control" placeholder="Nom de famille" required />
                                <span asp-validation-for="LastName" class="text-danger small"></span>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="CIN" class="form-label">CIN</label>
                                <input asp-for="CIN" class="form-control" placeholder="Ex: AB123456" />
                            </div>
                            <div class="col-md-6">
                                <label asp-for="Sexe" class="form-label">Sexe</label>
                                <select asp-for="Sexe" class="form-select">
                                    <option value="">-- Sélectionner --</option>
                                    <option value="Homme">Homme</option>
                                    <option value="Femme">Femme</option>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="DateNaissance" class="form-label">Date de naissance</label>
                                <input asp-for="DateNaissance" type="date" class="form-control" />
                            </div>
                            <div class="col-md-6">
                                <label asp-for="LieuNaissance" class="form-label">Lieu de naissance</label>
                                <input asp-for="LieuNaissance" class="form-control" placeholder="Ville" />
                            </div>
                            <div class="col-md-6">
                                <label asp-for="SituationFamiliale" class="form-label">Situation familiale</label>
                                <select asp-for="SituationFamiliale" class="form-select">
                                    <option value="">-- Sélectionner --</option>
                                    <option value="Célibataire">Célibataire</option>
                                    <option value="Marié(e)">Marié(e)</option>
                                    <option value="Divorcé(e)">Divorcé(e)</option>
                                    <option value="Veuf/Veuve">Veuf/Veuve</option>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="Telephone" class="form-label">Téléphone</label>
                                <input asp-for="Telephone" class="form-control" placeholder="+212 6XX XXX XXX" />
                            </div>
                            <div class="col-12">
                                <label asp-for="Adresse" class="form-label">Adresse</label>
                                <input asp-for="Adresse" class="form-control" placeholder="Adresse complète" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mc-card">
                    <div class="card-header"><i class="bi bi-key me-2"></i>Compte Utilisateur</div>
                    <div class="card-body">
                        <div class="row g-3">
                            <div class="col-12">
                                <label asp-for="Email" class="form-label">Email <span class="text-danger">*</span></label>
                                <div class="input-group">
                                    <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                    <input asp-for="Email" type="email" class="form-control" placeholder="email@cabinet.ma" required />
                                </div>
                                <span asp-validation-for="Email" class="text-danger small"></span>
                            </div>
                            <div class="col-12">
                                <label asp-for="Password" class="form-label">Mot de passe <span class="text-danger">*</span></label>
                                <div class="input-group">
                                    <span class="input-group-text"><i class="bi bi-lock"></i></span>
                                    <input asp-for="Password" type="password" class="form-control" placeholder="Min. 8 caractères" required />
                                    <button type="button" class="btn btn-outline-secondary" onclick="togglePwd(this)"><i class="bi bi-eye"></i></button>
                                </div>
                                <span asp-validation-for="Password" class="text-danger small"></span>
                                <div class="form-text">Minimum 8 caractères, 1 majuscule, 1 chiffre, 1 caractère spécial</div>
                            </div>
                        </div>
                        <div class="alert alert-info mt-3 mb-0 small">
                            <i class="bi bi-info-circle me-1"></i>
                            L'employé recevra ses identifiants après la création du compte.
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- STEP 1: Professional -->
    <div class="wizard-panel d-none" id="step1">
        <div class="row g-4">
            <div class="col-lg-6">
                <div class="mc-card">
                    <div class="card-header"><i class="bi bi-briefcase me-2"></i>Informations Professionnelles</div>
                    <div class="card-body">
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label asp-for="Poste" class="form-label">Poste / Fonction</label>
                                <input asp-for="Poste" class="form-control" placeholder="Ex: Comptable senior" />
                            </div>
                            <div class="col-md-6">
                                <label asp-for="Departement" class="form-label">Département</label>
                                <select asp-for="Departement" class="form-select">
                                    <option value="">-- Sélectionner --</option>
                                    <option value="Direction">Direction</option>
                                    <option value="Comptabilité">Comptabilité</option>
                                    <option value="Fiscal">Fiscal</option>
                                    <option value="Social">Social / Paie</option>
                                    <option value="Juridique">Juridique</option>
                                    <option value="Audit">Audit</option>
                                    <option value="Support">Support / Admin</option>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="TypeContrat" class="form-label">Type de contrat</label>
                                <select asp-for="TypeContrat" class="form-select">
                                    <option value="">-- Sélectionner --</option>
                                    <option value="CDI">CDI</option>
                                    <option value="CDD">CDD</option>
                                    <option value="Stage">Stage</option>
                                    <option value="Freelance">Freelance</option>
                                    <option value="Intérim">Intérim</option>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label asp-for="DateEmbauche" class="form-label">Date d'embauche</label>
                                <input asp-for="DateEmbauche" type="date" class="form-control" />
                            </div>
                            <div class="col-12">
                                <label asp-for="NumeroCNSS" class="form-label">N° CNSS</label>
                                <input asp-for="NumeroCNSS" class="form-control" placeholder="Numéro d'affiliation CNSS" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mc-card">
                    <div class="card-header"><i class="bi bi-info-circle me-2"></i>Récapitulatif Identité</div>
                    <div class="card-body">
                        <div class="text-center py-3">
                            <div class="rounded-circle bg-primary bg-opacity-10 d-inline-flex align-items-center justify-content-center" style="width:80px;height:80px">
                                <i class="bi bi-person-fill text-primary fs-1"></i>
                            </div>
                            <h5 class="mt-3 mb-1" id="previewName">-</h5>
                            <p class="text-muted mb-0" id="previewEmail">-</p>
                        </div>
                        <hr />
                        <div class="row g-2 small">
                            <div class="col-6"><span class="text-muted">CIN:</span> <span id="previewCIN">-</span></div>
                            <div class="col-6"><span class="text-muted">Tél:</span> <span id="previewTel">-</span></div>
                            <div class="col-6"><span class="text-muted">Sexe:</span> <span id="previewSexe">-</span></div>
                            <div class="col-6"><span class="text-muted">Naissance:</span> <span id="previewDOB">-</span></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- STEP 2: Role & Permissions -->
    <div class="wizard-panel d-none" id="step2">
        <div class="row g-4">
            <div class="col-lg-5">
                <div class="mc-card">
                    <div class="card-header"><i class="bi bi-person-gear me-2"></i>Rôle</div>
                    <div class="card-body">
                        <label asp-for="Role" class="form-label">Rôle dans le cabinet <span class="text-danger">*</span></label>
                        <select asp-for="Role" class="form-select form-select-lg mb-3" required id="roleSelect">
                            <option value="Assistant">Assistant</option>
                            <option value="Comptable">Comptable</option>
                            <option value="Fiscaliste">Fiscaliste</option>
                            <option value="Manager">Manager / Chef d'équipe</option>
                            <option value="SuperAdmin">Super Administrateur</option>
                            @foreach (var role in roles.Where(r => r.Name != "SuperAdmin" && r.Name != "Manager" && r.Name != "Assistant" && r.Name != "Comptable" && r.Name != "Fiscaliste"))
                            {
                                <option value="@role.Name">@role.Name</option>
                            }
                        </select>
                        <div class="alert alert-warning small mb-0" id="roleHelp">
                            <i class="bi bi-info-circle me-1"></i>
                            <strong>Assistant</strong> : Accès limité aux tâches assignées uniquement.
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-7">
                <div class="mc-card">
                    <div class="card-header">
                        <i class="bi bi-shield-lock me-2"></i>Permissions
                        <div class="float-end">
                            <button type="button" class="btn btn-sm btn-outline-primary me-1" onclick="toggleAllPerms(true)"><i class="bi bi-check-all me-1"></i>Tout</button>
                            <button type="button" class="btn btn-sm btn-outline-secondary" onclick="toggleAllPerms(false)"><i class="bi bi-x-lg me-1"></i>Aucun</button>
                        </div>
                    </div>
                    <div class="card-body" style="max-height:450px;overflow-y:auto;">
                        @foreach (var domain in claimsByDomain.OrderBy(d => d.Key))
                        {
                            <div class="mb-3">
                                <div class="d-flex align-items-center mb-2">
                                    <h6 class="fw-bold text-capitalize mb-0"><i class="bi bi-folder2 me-1 text-muted"></i>@domain.Key</h6>
                                    <button type="button" class="btn btn-sm btn-link p-0 ms-2" onclick="toggleDomain('@domain.Key')">tout</button>
                                </div>
                                @foreach (var claim in domain.Value)
                                {
                                    <div class="form-check ms-3">
                                        <input type="checkbox" class="form-check-input perm-cb" data-domain="@domain.Key"
                                               name="Permissions" value="@claim" id="perm_@claim.Replace(".", "_")" />
                                        <label class="form-check-label small" for="perm_@claim.Replace(".", "_")">
                                            @claim.Split('.').Last()
                                        </label>
                                    </div>
                                }
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Navigation -->
    <div class="d-flex justify-content-between mt-4">
        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" id="btnPrev" onclick="wizardNav(-1)" disabled>
            <i class="bi bi-arrow-left me-1"></i>Précédent
        </button>
        <div>
            <button type="button" class="btn btn-primary rounded-pill px-4" id="btnNext" onclick="wizardNav(1)">
                Suivant<i class="bi bi-arrow-right ms-1"></i>
            </button>
            <button type="submit" class="btn btn-success rounded-pill px-4 d-none" id="btnSubmit">
                <i class="bi bi-check-lg me-1"></i>Créer l'employé
            </button>
        </div>
    </div>
</form>

@section Scripts {
<style>
.wizard-step-circle{width:42px;height:42px;border-radius:50%;background:#e2e8f0;display:flex;align-items:center;justify-content:center;font-size:1.1rem;color:#64748b;transition:all .3s}
.wizard-step.active .wizard-step-circle{background:#4f46e5;color:#fff;box-shadow:0 0 0 4px rgba(79,70,229,0.2)}
.wizard-step.done .wizard-step-circle{background:#059669;color:#fff}
.wizard-step-label{font-size:.75rem;font-weight:600;color:#94a3b8;margin-top:.25rem;text-align:center}
.wizard-step.active .wizard-step-label{color:#4f46e5}
.wizard-step.done .wizard-step-label{color:#059669}
.wizard-step{display:flex;flex-direction:column;align-items:center;cursor:pointer}
.wizard-connector{flex:1;height:2px;background:#e2e8f0;margin:0 .5rem}
</style>
<script>
let step=0;const total=3;
function wizardNav(d){
    document.querySelectorAll('.wizard-panel')[step].classList.add('d-none');
    let ss=document.querySelectorAll('.wizard-step');
    if(d>0){ss[step].classList.remove('active');ss[step].classList.add('done');}
    step+=d; step=Math.max(0,Math.min(total-1,step));
    document.querySelectorAll('.wizard-panel')[step].classList.remove('d-none');
    ss[step].classList.add('active');ss[step].classList.remove('done');
    document.getElementById('btnPrev').disabled=step===0;
    document.getElementById('btnNext').classList.toggle('d-none',step===total-1);
    document.getElementById('btnSubmit').classList.toggle('d-none',step!==total-1);
    updatePreview();
}
function updatePreview(){
    let v=s=>document.querySelector('[name='+s+']')?.value||'-';
    document.getElementById('previewName').textContent=(v('FirstName')+' '+v('LastName')).trim()||'-';
    document.getElementById('previewEmail').textContent=v('Email');
    document.getElementById('previewCIN').textContent=v('CIN');
    document.getElementById('previewTel').textContent=v('Telephone');
    document.getElementById('previewSexe').textContent=v('Sexe');
    let dob=document.querySelector('[name=DateNaissance]')?.value;
    document.getElementById('previewDOB').textContent=dob?new Date(dob).toLocaleDateString('fr-FR'):'-';
}
function toggleAllPerms(c){document.querySelectorAll('.perm-cb').forEach(x=>x.checked=c);}
function toggleDomain(d){let cbs=document.querySelectorAll('.perm-cb[data-domain="'+d+'"]');let a=Array.from(cbs).every(x=>x.checked);cbs.forEach(x=>x.checked=!a);}
function togglePwd(b){let i=b.previousElementSibling;if(i.type==='password'){i.type='text';b.innerHTML='<i class="bi bi-eye-slash"></i>';}else{i.type='password';b.innerHTML='<i class="bi bi-eye"></i>';}}
const roleDesc={'Assistant':'Accès limité aux tâches assignées.','Comptable':'Gestion des déclarations et saisie comptable.','Fiscaliste':'Spécialiste déclarations fiscales (TVA, IS, IR).','Manager':'Supervision d\\'équipe, validation, contrôle qualité.','SuperAdmin':'Accès complet à toutes les fonctions.'};
document.getElementById('roleSelect')?.addEventListener('change',function(){document.getElementById('roleHelp').innerHTML='<i class="bi bi-info-circle me-1"></i><strong>'+this.value+'</strong> : '+(roleDesc[this.value]||this.value);});
document.querySelectorAll('.wizard-step').forEach((s,i)=>s.addEventListener('click',()=>{while(step>i)wizardNav(-1);while(step<i)wizardNav(1);}));
</script>
}
"""

(BASE / "CreateEmployee.cshtml").write_text(create_content, encoding="utf-8")
print(f"OK: CreateEmployee.cshtml ({len(create_content)} chars)")

# ============================================================
# 2. EditEmployee.cshtml - Full form with all fields
# ============================================================
edit_content = """@model MetaCompta.Application.DTOs.CabinetEmployeeUpdateDto
@{
    ViewData["Title"] = "Modifier employé";
    var claimsByDomain = ViewBag.ClaimsByDomain as Dictionary<string, List<string>> ?? new Dictionary<string, List<string>>();
    var roles = ViewBag.Roles as List<MetaCompta.Application.DTOs.RoleDto> ?? new List<MetaCompta.Application.DTOs.RoleDto>();
    var employeeId = ViewBag.EmployeeId as string;
    var employeeEmail = ViewBag.EmployeeEmail as string;
}

<div class="page-header mb-4">
    <div>
        <h2 class="mb-1"><i class="bi bi-pencil-square text-primary me-2"></i>Modifier Employé</h2>
        <nav aria-label="breadcrumb"><ol class="breadcrumb mb-0">
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Home")">Accueil</a></li>
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Cabinet")">Cabinet</a></li>
            <li class="breadcrumb-item active">Modifier</li>
        </ol></nav>
    </div>
    <div>
        <a href="@Url.Action("EmployeeDetails", "Cabinet", new { id = employeeId })" class="btn btn-outline-info rounded-pill px-3 me-2">
            <i class="bi bi-eye me-1"></i>Voir détails
        </a>
        <a href="@Url.Action("Index", "Cabinet")" class="btn btn-outline-secondary rounded-pill px-3">
            <i class="bi bi-arrow-left me-1"></i>Retour
        </a>
    </div>
</div>

<form method="post" asp-action="EditEmployee" asp-route-id="@employeeId">
    @Html.AntiForgeryToken()

    <div class="row g-4">
        <!-- LEFT COLUMN -->
        <div class="col-lg-8">
            <!-- Identity -->
            <div class="mc-card mb-4">
                <div class="card-header"><i class="bi bi-person me-2"></i>Informations Personnelles</div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-12">
                            <label class="form-label text-muted">Email (non modifiable)</label>
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                <input type="text" class="form-control bg-light" value="@employeeEmail" disabled />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <label asp-for="FirstName" class="form-label">Prénom <span class="text-danger">*</span></label>
                            <input asp-for="FirstName" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="LastName" class="form-label">Nom <span class="text-danger">*</span></label>
                            <input asp-for="LastName" class="form-control" required />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="CIN" class="form-label">CIN</label>
                            <input asp-for="CIN" class="form-control" placeholder="AB123456" />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="Sexe" class="form-label">Sexe</label>
                            <select asp-for="Sexe" class="form-select">
                                <option value="">--</option>
                                <option value="Homme">Homme</option>
                                <option value="Femme">Femme</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label asp-for="SituationFamiliale" class="form-label">Situation familiale</label>
                            <select asp-for="SituationFamiliale" class="form-select">
                                <option value="">--</option>
                                <option value="Célibataire">Célibataire</option>
                                <option value="Marié(e)">Marié(e)</option>
                                <option value="Divorcé(e)">Divorcé(e)</option>
                                <option value="Veuf/Veuve">Veuf/Veuve</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label asp-for="DateNaissance" class="form-label">Date de naissance</label>
                            <input asp-for="DateNaissance" type="date" class="form-control" />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="LieuNaissance" class="form-label">Lieu de naissance</label>
                            <input asp-for="LieuNaissance" class="form-control" />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="Telephone" class="form-label">Téléphone</label>
                            <input asp-for="Telephone" class="form-control" placeholder="+212 6XX" />
                        </div>
                        <div class="col-12">
                            <label asp-for="Adresse" class="form-label">Adresse</label>
                            <input asp-for="Adresse" class="form-control" placeholder="Adresse complète" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Professional -->
            <div class="mc-card mb-4">
                <div class="card-header"><i class="bi bi-briefcase me-2"></i>Informations Professionnelles</div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label asp-for="Poste" class="form-label">Poste / Fonction</label>
                            <input asp-for="Poste" class="form-control" placeholder="Ex: Comptable senior" />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="Departement" class="form-label">Département</label>
                            <select asp-for="Departement" class="form-select">
                                <option value="">--</option>
                                <option value="Direction">Direction</option>
                                <option value="Comptabilité">Comptabilité</option>
                                <option value="Fiscal">Fiscal</option>
                                <option value="Social">Social / Paie</option>
                                <option value="Juridique">Juridique</option>
                                <option value="Audit">Audit</option>
                                <option value="Support">Support / Admin</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label asp-for="TypeContrat" class="form-label">Contrat</label>
                            <select asp-for="TypeContrat" class="form-select">
                                <option value="">--</option>
                                <option value="CDI">CDI</option>
                                <option value="CDD">CDD</option>
                                <option value="Stage">Stage</option>
                                <option value="Freelance">Freelance</option>
                                <option value="Intérim">Intérim</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label asp-for="DateEmbauche" class="form-label">Date d'embauche</label>
                            <input asp-for="DateEmbauche" type="date" class="form-control" />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="NumeroCNSS" class="form-label">N° CNSS</label>
                            <input asp-for="NumeroCNSS" class="form-control" />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="Role" class="form-label">Rôle <span class="text-danger">*</span></label>
                            <select asp-for="Role" class="form-select" required>
                                <option value="Assistant">Assistant</option>
                                <option value="Comptable">Comptable</option>
                                <option value="Fiscaliste">Fiscaliste</option>
                                <option value="Manager">Manager</option>
                                <option value="SuperAdmin">Super Admin</option>
                                @foreach (var role in roles.Where(r => r.Name != "SuperAdmin" && r.Name != "Manager" && r.Name != "Assistant" && r.Name != "Comptable" && r.Name != "Fiscaliste"))
                                {
                                    <option value="@role.Name">@role.Name</option>
                                }
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label asp-for="IsActive" class="form-label">Statut</label>
                            <select asp-for="IsActive" class="form-select">
                                <option value="true">✅ Actif</option>
                                <option value="false">❌ Inactif</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- RIGHT: Permissions -->
        <div class="col-lg-4">
            <div class="mc-card mb-4" style="position:sticky;top:80px;">
                <div class="card-header">
                    <i class="bi bi-shield-lock me-2"></i>Permissions
                    <span class="badge bg-primary ms-1">@(Model.Permissions?.Count ?? 0)</span>
                    <div class="float-end">
                        <button type="button" class="btn btn-sm btn-outline-primary me-1" onclick="toggleAllPerms(true)"><i class="bi bi-check-all"></i></button>
                        <button type="button" class="btn btn-sm btn-outline-secondary" onclick="toggleAllPerms(false)"><i class="bi bi-x-lg"></i></button>
                    </div>
                </div>
                <div class="card-body" style="max-height:600px;overflow-y:auto;">
                    @foreach (var domain in claimsByDomain.OrderBy(d => d.Key))
                    {
                        <div class="mb-3">
                            <div class="d-flex align-items-center mb-1">
                                <h6 class="fw-bold text-capitalize mb-0 small"><i class="bi bi-folder2 me-1 text-muted"></i>@domain.Key</h6>
                                <button type="button" class="btn btn-sm btn-link p-0 ms-2 small" onclick="toggleDomain('@domain.Key')">tout</button>
                            </div>
                            @foreach (var claim in domain.Value)
                            {
                                <div class="form-check ms-3">
                                    <input type="checkbox" class="form-check-input perm-cb" data-domain="@domain.Key"
                                           name="Permissions" value="@claim" id="ep_@claim.Replace(".", "_")"
                                           @(Model.Permissions?.Contains(claim) == true ? "checked" : "") />
                                    <label class="form-check-label small" for="ep_@claim.Replace(".", "_")">@claim.Split('.').Last()</label>
                                </div>
                            }
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <div class="d-flex justify-content-between mt-3 mb-5">
        <a href="@Url.Action("Index", "Cabinet")" class="btn btn-outline-secondary rounded-pill px-4">
            <i class="bi bi-arrow-left me-1"></i>Annuler
        </a>
        <button type="submit" class="btn btn-success rounded-pill px-4">
            <i class="bi bi-check-lg me-1"></i>Enregistrer les modifications
        </button>
    </div>
</form>

@section Scripts {
<script>
function toggleAllPerms(c){document.querySelectorAll('.perm-cb').forEach(x=>x.checked=c);}
function toggleDomain(d){let cbs=document.querySelectorAll('.perm-cb[data-domain="'+d+'"]');let a=Array.from(cbs).every(x=>x.checked);cbs.forEach(x=>x.checked=!a);}
</script>
}
"""

(BASE / "EditEmployee.cshtml").write_text(edit_content, encoding="utf-8")
print(f"OK: EditEmployee.cshtml ({len(edit_content)} chars)")

# ============================================================
# 3. EmployeeDetails.cshtml - Rich detail view
# ============================================================
details_content = """@model MetaCompta.Application.DTOs.CabinetEmployeeDto
@{
    ViewData["Title"] = "Détails employé";
    var initials = (!string.IsNullOrEmpty(Model.FirstName) ? Model.FirstName[0].ToString().ToUpper() : "") +
                   (!string.IsNullOrEmpty(Model.LastName) ? Model.LastName[0].ToString().ToUpper() : "");
    var roleColor = Model.Role switch {
        "SuperAdmin" => "#dc2626",
        "Manager" => "#7c3aed",
        "Comptable" => "#2563eb",
        "Fiscaliste" => "#d97706",
        _ => "#64748b"
    };
}

<div class="page-header mb-4">
    <div>
        <h2 class="mb-1"><i class="bi bi-person-badge text-primary me-2"></i>Fiche Employé</h2>
        <nav aria-label="breadcrumb"><ol class="breadcrumb mb-0">
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Home")">Accueil</a></li>
            <li class="breadcrumb-item"><a href="@Url.Action("Index", "Cabinet")">Cabinet</a></li>
            <li class="breadcrumb-item active">@Model.NomComplet</li>
        </ol></nav>
    </div>
    <div>
        <a href="@Url.Action("EditEmployee", "Cabinet", new { id = Model.Id })" class="btn btn-mc-primary rounded-pill px-3">
            <i class="bi bi-pencil me-1"></i>Modifier
        </a>
    </div>
</div>

<!-- Profile Header -->
<div class="mc-card mb-4">
    <div class="card-body">
        <div class="row align-items-center">
            <div class="col-auto">
                <div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold"
                     style="width:80px;height:80px;font-size:1.6rem;background:linear-gradient(135deg, @(roleColor), @(roleColor)cc);">
                    @initials
                </div>
            </div>
            <div class="col">
                <h3 class="mb-1">@Model.NomComplet</h3>
                <p class="text-muted mb-1"><i class="bi bi-envelope me-1"></i>@Model.Email</p>
                <div class="d-flex gap-2 flex-wrap">
                    <span class="badge rounded-pill" style="background:@(roleColor);font-size:.8rem;">
                        <i class="bi bi-shield-check me-1"></i>@(Model.Role ?? "Non défini")
                    </span>
                    @if (!string.IsNullOrEmpty(Model.Departement))
                    {
                        <span class="badge bg-info rounded-pill"><i class="bi bi-building me-1"></i>@Model.Departement</span>
                    }
                    @if (!string.IsNullOrEmpty(Model.Poste))
                    {
                        <span class="badge bg-secondary rounded-pill"><i class="bi bi-briefcase me-1"></i>@Model.Poste</span>
                    }
                    <span class="badge rounded-pill @(Model.EstActif ? "bg-success" : "bg-danger")">
                        @(Model.EstActif ? "✅ Actif" : "❌ Inactif")
                    </span>
                </div>
            </div>
            <div class="col-auto text-end">
                @if (Model.DerniereConnexion.HasValue)
                {
                    <div class="small text-muted">Dernière connexion</div>
                    <div class="fw-semibold">@Model.DerniereConnexion.Value.ToString("dd/MM/yyyy HH:mm")</div>
                }
                else
                {
                    <span class="badge bg-warning text-dark">Jamais connecté</span>
                }
            </div>
        </div>
    </div>
</div>

<div class="row g-4">
    <!-- LEFT -->
    <div class="col-lg-8">
        <!-- Personal Info -->
        <div class="mc-card mb-4">
            <div class="card-header"><i class="bi bi-person-vcard me-2"></i>Informations Personnelles</div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">CIN</label>
                        <p class="fw-semibold mb-2">@(Model.CIN ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Sexe</label>
                        <p class="fw-semibold mb-2">@(Model.Sexe ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Situation familiale</label>
                        <p class="fw-semibold mb-2">@(Model.SituationFamiliale ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Date de naissance</label>
                        <p class="fw-semibold mb-2">@(Model.DateNaissance?.ToString("dd/MM/yyyy") ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Lieu de naissance</label>
                        <p class="fw-semibold mb-2">@(Model.LieuNaissance ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Téléphone</label>
                        <p class="fw-semibold mb-2"><i class="bi bi-telephone me-1 text-muted"></i>@(Model.Telephone ?? "—")</p>
                    </div>
                    <div class="col-12">
                        <label class="form-label text-muted small mb-0">Adresse</label>
                        <p class="fw-semibold mb-0"><i class="bi bi-geo-alt me-1 text-muted"></i>@(Model.Adresse ?? "—")</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Professional Info -->
        <div class="mc-card mb-4">
            <div class="card-header"><i class="bi bi-briefcase me-2"></i>Informations Professionnelles</div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Poste</label>
                        <p class="fw-semibold mb-2">@(Model.Poste ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Département</label>
                        <p class="fw-semibold mb-2">@(Model.Departement ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Type de contrat</label>
                        <p class="fw-semibold mb-2">@(Model.TypeContrat ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Date d'embauche</label>
                        <p class="fw-semibold mb-2"><i class="bi bi-calendar me-1 text-muted"></i>@Model.DateEmbauche.ToString("dd/MM/yyyy")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">N° CNSS</label>
                        <p class="fw-semibold mb-2">@(Model.NumeroCNSS ?? "—")</p>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label text-muted small mb-0">Langue</label>
                        <p class="fw-semibold mb-2"><i class="bi bi-translate me-1 text-muted"></i>@(Model.PreferredLanguage ?? "fr-FR")</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Stats -->
        <div class="mc-card mb-4">
            <div class="card-header"><i class="bi bi-graph-up me-2"></i>Activité</div>
            <div class="card-body">
                <div class="row g-3 text-center">
                    <div class="col-md-4">
                        <div class="p-3 bg-primary bg-opacity-10 rounded-3">
                            <div class="fs-2 fw-bold text-primary">@Model.ClientsAssignes</div>
                            <div class="small text-muted">Clients assignés</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="p-3 bg-warning bg-opacity-10 rounded-3">
                            <div class="fs-2 fw-bold text-warning">@Model.TachesEnCours</div>
                            <div class="small text-muted">Tâches en cours</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="p-3 bg-success bg-opacity-10 rounded-3">
                            <div class="fs-2 fw-bold text-success">@(Model.Permissions?.Count ?? 0)</div>
                            <div class="small text-muted">Permissions</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- RIGHT: Permissions -->
    <div class="col-lg-4">
        <div class="mc-card mb-4" style="position:sticky;top:80px;">
            <div class="card-header">
                <i class="bi bi-shield-lock me-2"></i>Permissions
                <span class="badge bg-primary ms-2">@(Model.Permissions?.Count ?? 0)</span>
            </div>
            <div class="card-body" style="max-height:500px;overflow-y:auto;">
                @if (Model.Permissions?.Any() == true)
                {
                    var grouped = Model.Permissions
                        .GroupBy(p => p.Contains('.') ? p.Split('.')[0] : p)
                        .OrderBy(g => g.Key);
                    foreach (var group in grouped)
                    {
                        <div class="mb-3">
                            <h6 class="fw-bold text-capitalize mb-2 small">
                                <i class="bi bi-folder2 me-1 text-muted"></i>@group.Key
                                <span class="badge bg-light text-dark ms-1">@group.Count()</span>
                            </h6>
                            @foreach (var perm in group)
                            {
                                <span class="badge bg-light text-dark border me-1 mb-1 small">
                                    <i class="bi bi-check2 text-success me-1"></i>@perm.Split('.').Last()
                                </span>
                            }
                        </div>
                    }
                }
                else
                {
                    <div class="text-center text-muted py-4">
                        <i class="bi bi-shield-x fs-1 d-block mb-2 text-muted"></i>
                        <p class="mb-0">Aucune permission directe</p>
                    </div>
                }
            </div>
        </div>
    </div>
</div>

<div class="mt-3 mb-5">
    <a href="@Url.Action("Index", "Cabinet")" class="btn btn-outline-secondary rounded-pill px-4">
        <i class="bi bi-arrow-left me-1"></i>Retour à la liste
    </a>
</div>
"""

(BASE / "EmployeeDetails.cshtml").write_text(details_content, encoding="utf-8")
print(f"OK: EmployeeDetails.cshtml ({len(details_content)} chars)")

print("\\n=== All 3 views written successfully ===")

$layoutPath = "c:\Users\delli\OneDrive\Bureau\MetaComptaSAS\src\MetaCompta.BackOffice\Views\Shared\_Layout.cshtml"

$content = @"
@{
    Layout = null;
    var currentController = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
    var currentAction = ViewContext.RouteData.Values["action"]?.ToString() ?? "";

    // Global Client Context from Session
    var selectedClientId = Context.Session.GetString("SelectedClientId");
    var selectedClientName = Context.Session.GetString("SelectedClientName");
    var hasClient = !string.IsNullOrEmpty(selectedClientId);

    // Client-dependent controllers
    var clientControllers = new[] { "Declarations", "EtatsFiscaux", "Comptabilite", "Payroll", "Employees", "Juridique", "Documents" };
    var isClientSection = clientControllers.Contains(currentController);
}
<!DOCTYPE html>
<html lang="fr" dir="ltr">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - MetaCompta</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
    <link href="https://cdn.datatables.net/1.13.8/css/dataTables.bootstrap5.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css" rel="stylesheet" />
    <style>
        :root {
            --mc-primary: #2563eb;
            --mc-primary-dark: #1d4ed8;
            --mc-primary-light: #3b82f6;
            --mc-secondary: #059669;
            --mc-accent: #f59e0b;
            --mc-danger: #dc2626;
            --mc-sidebar-bg: #0f172a;
            --mc-sidebar-hover: #1e293b;
            --mc-sidebar-text: #94a3b8;
            --mc-sidebar-active: #2563eb;
            --mc-sidebar-divider: rgba(255,255,255,0.06);
            --mc-body-bg: #f1f5f9;
            --mc-card-bg: #ffffff;
            --mc-border: #e2e8f0;
            --mc-sidebar-width: 270px;
        }
        * { font-family: 'Inter', sans-serif; }
        body { background: var(--mc-body-bg); min-height: 100vh; }

        /* SIDEBAR */
        .mc-sidebar {
            position: fixed; top: 0; left: 0;
            width: var(--mc-sidebar-width); height: 100vh;
            background: var(--mc-sidebar-bg);
            z-index: 1040; overflow-y: auto;
            transition: transform 0.3s ease;
            display: flex; flex-direction: column;
        }
        .mc-sidebar .sidebar-brand {
            padding: 1.15rem 1.25rem;
            display: flex; align-items: center; gap: 0.75rem;
            border-bottom: 1px solid var(--mc-sidebar-divider);
            flex-shrink: 0;
        }
        .mc-sidebar .sidebar-brand h4 { color: #fff; font-weight: 700; margin: 0; font-size: 1.2rem; }
        .mc-sidebar .sidebar-brand .brand-icon {
            width: 36px; height: 36px;
            background: linear-gradient(135deg, var(--mc-primary), #7c3aed);
            border-radius: 10px; display: flex; align-items: center;
            justify-content: center; color: #fff; font-size: 1.1rem; font-weight: 700;
        }

        /* Client Context Block */
        .sidebar-client-context {
            padding: 0.85rem 1rem;
            border-bottom: 1px solid var(--mc-sidebar-divider);
            flex-shrink: 0;
        }
        .client-context-card {
            background: rgba(37,99,235,0.08);
            border: 1px solid rgba(37,99,235,0.2);
            border-radius: 10px; padding: 0.75rem 0.85rem;
            transition: all 0.2s ease;
        }
        .client-context-card.no-client {
            background: rgba(148,163,184,0.08);
            border-color: rgba(148,163,184,0.15);
        }
        .client-context-label {
            font-size: 0.65rem; text-transform: uppercase;
            letter-spacing: 0.08em; color: #64748b; font-weight: 600;
            margin-bottom: 0.25rem; display: flex; align-items: center; gap: 0.35rem;
        }
        .client-context-name {
            font-size: 0.85rem; font-weight: 600; color: #e2e8f0;
            white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
        }
        .client-context-name.empty { color: #f59e0b; font-weight: 500; font-style: italic; font-size: 0.78rem; }
        .client-context-hint { font-size: 0.6rem; color: #64748b; margin-top: 0.2rem; }

        /* Sidebar Scroll */
        .sidebar-scroll { flex: 1; overflow-y: auto; padding-bottom: 1rem; }
        .mc-sidebar .nav-section {
            padding: 1.1rem 1rem 0.3rem 1.25rem;
            font-size: 0.65rem; text-transform: uppercase;
            letter-spacing: 0.1em; color: #475569; font-weight: 700;
            display: flex; align-items: center; gap: 0.5rem;
        }
        .mc-sidebar .nav-section .section-line { flex: 1; height: 1px; background: rgba(255,255,255,0.04); }
        .mc-sidebar .nav-link {
            color: var(--mc-sidebar-text);
            padding: 0.5rem 1.25rem; display: flex;
            align-items: center; gap: 0.7rem;
            font-size: 0.84rem; border-radius: 8px;
            margin: 1px 0.65rem; transition: all 0.2s ease;
            position: relative;
        }
        .mc-sidebar .nav-link:hover { background: var(--mc-sidebar-hover); color: #e2e8f0; transform: translateX(2px); }
        .mc-sidebar .nav-link.active {
            background: var(--mc-sidebar-active); color: #fff; font-weight: 500;
        }
        .mc-sidebar .nav-link.active::before {
            content: ''; position: absolute; left: -0.65rem; top: 50%;
            transform: translateY(-50%); width: 3px; height: 60%;
            background: #60a5fa; border-radius: 0 2px 2px 0;
        }
        .mc-sidebar .nav-link i { font-size: 1.05rem; width: 22px; text-align: center; flex-shrink: 0; }
        .mc-sidebar .nav-link .nav-badge {
            margin-left: auto; font-size: 0.65rem; padding: 0.15rem 0.45rem;
            border-radius: 6px; font-weight: 600;
        }
        .mc-sidebar .nav-link.client-disabled { opacity: 0.35; pointer-events: none; cursor: not-allowed; }

        /* Submenu */
        .sidebar-submenu .nav-link {
            padding: 0.35rem 1.25rem 0.35rem 2.5rem; font-size: 0.8rem; margin: 0 0.65rem;
        }
        .sidebar-submenu .nav-link::before { display: none; }
        .nav-link[data-bs-toggle="collapse"] .chevron-icon {
            transition: transform 0.2s ease; font-size: 0.65rem; margin-left: auto;
        }
        .nav-link[data-bs-toggle="collapse"][aria-expanded="true"] .chevron-icon { transform: rotate(180deg); }

        .client-section-warning {
            margin: 0 0.85rem 0.25rem; padding: 0.4rem 0.65rem; border-radius: 6px;
            font-size: 0.62rem; color: #94a3b8;
            background: rgba(245,158,11,0.06); border-left: 2px solid #f59e0b; line-height: 1.3;
        }

        /* MAIN + TOPBAR */
        .mc-main { margin-left: var(--mc-sidebar-width); min-height: 100vh; }
        .mc-topbar {
            background: var(--mc-card-bg); border-bottom: 1px solid var(--mc-border);
            padding: 0 1.5rem; height: 60px;
            display: flex; align-items: center; justify-content: space-between;
            position: sticky; top: 0; z-index: 1030;
        }

        /* Client Selector in Topbar */
        .client-selector-wrapper { position: relative; min-width: 320px; }
        .client-selector-btn {
            display: flex; align-items: center; gap: 0.6rem;
            padding: 0.45rem 0.85rem; border-radius: 10px;
            border: 1px solid var(--mc-border); background: var(--mc-body-bg);
            cursor: pointer; transition: all 0.2s ease; width: 100%;
            font-size: 0.85rem; color: #334155;
        }
        .client-selector-btn:hover { border-color: var(--mc-primary); box-shadow: 0 0 0 3px rgba(37,99,235,0.08); }
        .client-selector-btn .selected-name { font-weight: 600; color: #1e293b; }
        .client-selector-btn .no-selection { color: #94a3b8; font-style: italic; }
        .client-selector-dropdown {
            position: absolute; top: calc(100% + 6px); left: 0; right: 0;
            background: #fff; border: 1px solid var(--mc-border);
            border-radius: 12px; box-shadow: 0 12px 36px rgba(0,0,0,0.12);
            z-index: 1050; display: none; max-height: 380px; overflow: hidden;
        }
        .client-selector-dropdown.show { display: block; }
        .client-search-input {
            width: 100%; border: none; border-bottom: 1px solid var(--mc-border);
            padding: 0.65rem 0.85rem; font-size: 0.84rem; outline: none;
            border-radius: 12px 12px 0 0;
        }
        .client-list { max-height: 280px; overflow-y: auto; padding: 0.35rem; }
        .client-item {
            display: flex; align-items: center; gap: 0.65rem;
            padding: 0.55rem 0.75rem; border-radius: 8px; cursor: pointer;
            transition: all 0.15s ease; font-size: 0.84rem;
        }
        .client-item:hover { background: #f1f5f9; }
        .client-item.selected { background: rgba(37,99,235,0.06); border: 1px solid rgba(37,99,235,0.15); }
        .client-item .client-avatar {
            width: 32px; height: 32px; border-radius: 8px;
            background: linear-gradient(135deg, #e0e7ff, #c7d2fe);
            display: flex; align-items: center; justify-content: center;
            color: #4338ca; font-weight: 700; font-size: 0.75rem; flex-shrink: 0;
        }
        .client-item .client-info { flex: 1; min-width: 0; }
        .client-item .client-info .name {
            font-weight: 600; color: #1e293b; font-size: 0.82rem;
            white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
        }
        .client-item .client-info .ice { font-size: 0.7rem; color: #94a3b8; }
        .client-clear-btn {
            display: block; text-align: center; padding: 0.5rem;
            font-size: 0.78rem; color: #dc2626; cursor: pointer;
            border-top: 1px solid var(--mc-border); transition: background 0.15s;
        }
        .client-clear-btn:hover { background: #fef2f2; }

        .mc-topbar .search-box { position: relative; width: 260px; }
        .mc-topbar .search-box input {
            padding-left: 2.3rem; border-radius: 10px;
            border: 1px solid var(--mc-border); background: var(--mc-body-bg);
            font-size: 0.84rem; height: 38px;
        }
        .mc-topbar .search-box i {
            position: absolute; left: 0.75rem; top: 50%;
            transform: translateY(-50%); color: #94a3b8;
        }
        .mc-topbar .user-menu { display: flex; align-items: center; gap: 0.75rem; }
        .mc-topbar .notification-bell {
            position: relative; cursor: pointer; font-size: 1.2rem; color: #64748b;
            width: 38px; height: 38px; display: flex; align-items: center;
            justify-content: center; border-radius: 10px; transition: all 0.2s;
        }
        .mc-topbar .notification-bell:hover { background: var(--mc-body-bg); color: #1e293b; }
        .mc-topbar .notification-bell .badge {
            position: absolute; top: 2px; right: 2px;
            font-size: 0.6rem; padding: 0.12rem 0.35rem;
        }
        .mc-topbar .user-avatar {
            width: 36px; height: 36px; border-radius: 10px;
            background: linear-gradient(135deg, var(--mc-primary), #7c3aed);
            display: flex; align-items: center; justify-content: center;
            color: #fff; font-weight: 600; font-size: 0.8rem;
        }

        /* CONTENT */
        .mc-content { padding: 1.5rem; }
        .mc-card {
            background: var(--mc-card-bg); border-radius: 14px;
            border: 1px solid var(--mc-border); box-shadow: 0 1px 3px rgba(0,0,0,0.04);
            transition: all 0.2s ease;
        }
        .mc-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.08); }
        .mc-card .card-header { background: transparent; border-bottom: 1px solid var(--mc-border); padding: 1rem 1.25rem; font-weight: 600; }
        .stat-card { padding: 1.25rem; border-radius: 14px; background: var(--mc-card-bg); border: 1px solid var(--mc-border); }
        .stat-card .stat-icon { width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 1.3rem; }
        .stat-card .stat-value { font-size: 1.75rem; font-weight: 700; color: #1e293b; }
        .stat-card .stat-label { font-size: 0.8rem; color: #64748b; font-weight: 500; }

        /* Badges */
        .badge-brouillon { background: #e2e8f0; color: #475569; }
        .badge-acompleter { background: #fef3c7; color: #92400e; }
        .badge-completee { background: #dbeafe; color: #1e40af; }
        .badge-enrevision { background: #ede9fe; color: #6b21a8; }
        .badge-validee { background: #d1fae5; color: #065f46; }
        .badge-soumise { background: #dcfce7; color: #166534; }
        .badge-refusee { background: #fee2e2; color: #991b1b; }
        .badge-faible { background: #e2e8f0; color: #475569; }
        .badge-moyenne { background: #fef3c7; color: #92400e; }
        .badge-haute { background: #fed7aa; color: #9a3412; }
        .badge-urgente { background: #fee2e2; color: #991b1b; }

        /* Tables */
        .mc-table { font-size: 0.875rem; }
        .mc-table th { font-weight: 600; color: #475569; font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; border-bottom-width: 2px; }
        .mc-table td { vertical-align: middle; color: #334155; }

        /* Buttons */
        .btn-mc-primary {
            background: linear-gradient(135deg, var(--mc-primary), #4f46e5);
            border: none; color: #fff; border-radius: 10px;
            padding: 0.5rem 1.25rem; font-weight: 500; font-size: 0.875rem; transition: all 0.2s ease;
        }
        .btn-mc-primary:hover {
            background: linear-gradient(135deg, var(--mc-primary-dark), #4338ca);
            color: #fff; transform: translateY(-1px); box-shadow: 0 4px 12px rgba(37,99,235,0.3);
        }
        .btn-mc-success { background: linear-gradient(135deg, #059669, #10b981); border: none; color: #fff; border-radius: 10px; }
        .btn-mc-danger { background: linear-gradient(135deg, #dc2626, #ef4444); border: none; color: #fff; border-radius: 10px; }

        /* Page Header */
        .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.5rem; }
        .page-header h2 { font-weight: 700; color: #1e293b; margin: 0; font-size: 1.5rem; }
        .page-header .breadcrumb { margin: 0; font-size: 0.8rem; }

        /* Forms */
        .form-control, .form-select { border-radius: 10px; border: 1px solid var(--mc-border); padding: 0.6rem 1rem; font-size: 0.875rem; }
        .form-control:focus, .form-select:focus { border-color: var(--mc-primary); box-shadow: 0 0 0 3px rgba(37,99,235,0.15); }
        .form-label { font-weight: 500; font-size: 0.85rem; color: #475569; margin-bottom: 0.35rem; }

        /* Responsive */
        @@media (max-width: 768px) {
            .mc-sidebar { transform: translateX(-100%); }
            .mc-sidebar.show { transform: translateX(0); }
            .mc-main { margin-left: 0; }
            .client-selector-wrapper { min-width: 200px; }
        }

        /* Scrollbar */
        ::-webkit-scrollbar { width: 5px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 3px; }
        ::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

        .fade-in { animation: fadeIn 0.3s ease-in; }
        @@keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }
    </style>
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <!-- SIDEBAR -->
    <nav class="mc-sidebar" id="sidebar" aria-label="Navigation principale">
        <div class="sidebar-brand">
            <div class="brand-icon"><i class="bi bi-briefcase-fill"></i></div>
            <div>
                <h4>MetaCompta</h4>
                <div style="font-size:0.6rem; color:#64748b; margin-top:-2px;">@(Context.Session.GetString("CabinetName") ?? "Cabinet")</div>
            </div>
        </div>

        <!-- Client Context Block -->
        <div class="sidebar-client-context">
            <div class="client-context-card @(hasClient ? "" : "no-client")" id="sidebarClientCard">
                <div class="client-context-label"><i class="bi bi-person-badge"></i> Client actif</div>
                @if (hasClient)
                {
                    <div class="client-context-name" title="@selectedClientName">
                        <i class="bi bi-building" style="font-size:0.8rem; margin-right:0.25rem;"></i>@selectedClientName
                    </div>
                    <div class="client-context-hint"><i class="bi bi-info-circle"></i> S&#233;lectionn&#233; depuis la barre sup&#233;rieure</div>
                }
                else
                {
                    <div class="client-context-name empty"><i class="bi bi-exclamation-triangle" style="font-size:0.75rem;"></i> Aucun client s&#233;lectionn&#233;</div>
                    <div class="client-context-hint">Utilisez la barre sup&#233;rieure pour s&#233;lectionner un client</div>
                }
            </div>
        </div>

        <!-- Scrollable Nav -->
        <div class="sidebar-scroll">
            <!-- A. CABINET (Global) -->
            <div class="nav-section">Cabinet <span class="section-line"></span></div>
            <ul class="nav flex-column">
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Home" ? "active" : "")" href="@Url.Action("Index", "Home")">
                        <i class="bi bi-grid-1x2-fill"></i> Tableau de bord
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Clients" && currentAction == "Index" ? "active" : "")" href="@Url.Action("Index", "Clients")">
                        <i class="bi bi-buildings"></i> Clients
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Clients" && currentAction == "Create" ? "active" : "")" href="@Url.Action("Create", "Clients")">
                        <i class="bi bi-plus-circle"></i> Nouveau client
                    </a>
                </li>
            </ul>

            <div class="nav-section">Planning &amp; T&#226;ches <span class="section-line"></span></div>
            <ul class="nav flex-column">
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Planning" ? "active" : "")" href="@Url.Action("Index", "Planning")">
                        <i class="bi bi-calendar-week"></i> Calendrier
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Tasks" && currentAction == "Index" ? "active" : "")" href="@Url.Action("Index", "Tasks")">
                        <i class="bi bi-list-task"></i> Toutes les t&#226;ches
                        <span class="nav-badge bg-warning bg-opacity-10 text-warning" id="badgeTasks">0</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Tasks" && currentAction == "MyTasks" ? "active" : "")" href="@Url.Action("MyTasks", "Tasks")">
                        <i class="bi bi-person-check"></i> Mes t&#226;ches
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="@Url.Action("Index", "EtatsFiscaux")">
                        <i class="bi bi-calendar-event"></i> &#201;ch&#233;ances fiscales
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(currentController == "Tasks" && currentAction == "Overdue" ? "active" : "")" href="@Url.Action("Overdue", "Tasks")">
                        <i class="bi bi-exclamation-triangle"></i> T&#226;ches en retard
                        <span class="nav-badge bg-danger bg-opacity-10 text-danger" id="badgeOverdue">0</span>
                    </a>
                </li>
            </ul>

            <div class="nav-section">Gestion du cabinet <span class="section-line"></span></div>
            <ul class="nav flex-column">
                <li class="nav-item"><a class="nav-link @(currentController == "Cabinet" && currentAction == "Index" ? "active" : "")" href="@Url.Action("Index", "Cabinet")"><i class="bi bi-people"></i> Employ&#233;s</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Cabinet" && currentAction == "Roles" ? "active" : "")" href="@Url.Action("Roles", "Cabinet")"><i class="bi bi-shield-lock"></i> R&#244;les &amp; Permissions</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Cabinet" && currentAction == "Equipes" ? "active" : "")" href="@Url.Action("Equipes", "Cabinet")"><i class="bi bi-people-fill"></i> &#201;quipes</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Cabinet" && currentAction == "AuditLogs" ? "active" : "")" href="@Url.Action("AuditLogs", "Cabinet")"><i class="bi bi-clock-history"></i> Historique connexion</a></li>
            </ul>

            <div class="nav-section">Rapports <span class="section-line"></span></div>
            <ul class="nav flex-column">
                <li class="nav-item"><a class="nav-link @(currentController == "Rapports" && ViewBag.TypeRapport?.ToString() == "Financier" ? "active" : "")" href="@Url.Action("Financiers", "Rapports")"><i class="bi bi-graph-up-arrow"></i> Financiers</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Rapports" && ViewBag.TypeRapport?.ToString() == "Fiscal" ? "active" : "")" href="@Url.Action("Fiscaux", "Rapports")"><i class="bi bi-file-earmark-bar-graph"></i> Fiscaux</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Rapports" && ViewBag.TypeRapport?.ToString() == "Social" ? "active" : "")" href="@Url.Action("Sociaux", "Rapports")"><i class="bi bi-file-earmark-medical"></i> Sociaux</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Rapports" && ViewBag.TypeRapport?.ToString() == "Activite" ? "active" : "")" href="@Url.Action("Activites", "Rapports")"><i class="bi bi-activity"></i> Activit&#233;s</a></li>
            </ul>

            <div class="nav-section">Param&#232;tres <span class="section-line"></span></div>
            <ul class="nav flex-column">
                <li class="nav-item"><a class="nav-link @(currentController == "Parametres" && currentAction == "Index" ? "active" : "")" href="@Url.Action("Index", "Parametres")"><i class="bi bi-gear"></i> Configuration</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Parametres" && currentAction == "Templates" ? "active" : "")" href="@Url.Action("Templates", "Parametres")"><i class="bi bi-file-earmark-ruled"></i> Mod&#232;les documents</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Parametres" && currentAction == "Notifications" ? "active" : "")" href="@Url.Action("Notifications", "Parametres")"><i class="bi bi-bell-fill"></i> Notifications</a></li>
                <li class="nav-item"><a class="nav-link @(currentController == "Parametres" && currentAction == "Integrations" ? "active" : "")" href="@Url.Action("Integrations", "Parametres")"><i class="bi bi-plug"></i> Int&#233;grations (API)</a></li>
            </ul>

            <!-- B. ESPACE CLIENT (Context-Dependent) -->
            <div class="nav-section" style="margin-top:0.5rem; color: @(hasClient ? "#3b82f6" : "#475569");">
                <i class="bi bi-person-workspace" style="font-size:0.7rem;"></i> Espace Client <span class="section-line"></span>
            </div>

            @if (!hasClient)
            {
                <div class="client-section-warning"><i class="bi bi-info-circle"></i> S&#233;lectionnez un client dans la barre sup&#233;rieure pour acc&#233;der aux fonctions ci-dessous.</div>
            }
            else
            {
                <div class="client-section-warning" style="border-left-color: #3b82f6; background: rgba(37,99,235,0.04);"><i class="bi bi-link-45deg"></i> Toutes les actions ci-dessous sont li&#233;es au client <strong style="color:#e2e8f0;">@selectedClientName</strong></div>
            }

            <ul class="nav flex-column">
                <!-- Fiscalit&#233; -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuFiscalite" role="button" aria-expanded="@(currentController == "Declarations" ? "true" : "false")">
                        <i class="bi bi-receipt"></i><span>Fiscalit&#233;</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "Declarations" ? "show" : "")" id="menuFiscalite">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link @(currentController == "Declarations" && ViewBag.Type?.ToString() == "TVA" ? "active" : "")" href="@Url.Action("Index", "Declarations", new { type = "TVA", clientId = selectedClientId })"><i class="bi bi-receipt-cutoff"></i> TVA</a></li>
                            <li class="nav-item"><a class="nav-link @(currentController == "Declarations" && ViewBag.Type?.ToString() == "IS" ? "active" : "")" href="@Url.Action("Index", "Declarations", new { type = "IS", clientId = selectedClientId })"><i class="bi bi-bank2"></i> Imp&#244;t sur soci&#233;t&#233;s (IS)</a></li>
                            <li class="nav-item"><a class="nav-link @(currentController == "Declarations" && ViewBag.Type?.ToString() == "IR" ? "active" : "")" href="@Url.Action("Index", "Declarations", new { type = "IR", clientId = selectedClientId })"><i class="bi bi-person-lines-fill"></i> Imp&#244;t sur le revenu (IR)</a></li>
                            <li class="nav-item"><a class="nav-link @(currentController == "Declarations" && ViewBag.Type == null ? "active" : "")" href="@Url.Action("Index", "Declarations", new { clientId = selectedClientId })"><i class="bi bi-clock-history"></i> Historique fiscal</a></li>
                        </ul>
                    </div>
                </li>

                <!-- &#201;tats financiers -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuEtats" role="button" aria-expanded="@(currentController == "EtatsFiscaux" ? "true" : "false")">
                        <i class="bi bi-file-earmark-text"></i><span>&#201;tats financiers</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "EtatsFiscaux" ? "show" : "")" id="menuEtats">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Etat9421", "EtatsFiscaux")"><i class="bi bi-file-text"></i> &#201;tat 9421 (salaires)</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("LiasseFiscale", "EtatsFiscaux")"><i class="bi bi-files"></i> Liasse fiscale</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Bilan", "EtatsFiscaux")"><i class="bi bi-clipboard-data"></i> Bilan fiscal</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("CPC", "EtatsFiscaux")"><i class="bi bi-bar-chart-line"></i> CPC</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Amortissements", "EtatsFiscaux")"><i class="bi bi-graph-down-arrow"></i> Amortissements</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("EtatsSynthese", "EtatsFiscaux")"><i class="bi bi-journal-text"></i> &#201;tats de synth&#232;se</a></li>
                        </ul>
                    </div>
                </li>

                <!-- Comptabilit&#233; -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuCompta" role="button" aria-expanded="@(currentController == "Comptabilite" ? "true" : "false")">
                        <i class="bi bi-calculator"></i><span>Comptabilit&#233;</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "Comptabilite" ? "show" : "")" id="menuCompta">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Ecritures", "Comptabilite")"><i class="bi bi-pencil-square"></i> &#201;critures comptables</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Journaux", "Comptabilite")"><i class="bi bi-journal-bookmark"></i> Journaux</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("GrandLivre", "Comptabilite")"><i class="bi bi-book"></i> Grand livre</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Balance", "Comptabilite")"><i class="bi bi-symmetry-horizontal"></i> Balance</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("BalanceAgee", "Comptabilite")"><i class="bi bi-hourglass-split"></i> Balance &#226;g&#233;e</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Rapprochements", "Comptabilite")"><i class="bi bi-arrow-left-right"></i> Rapprochement bancaire</a></li>
                        </ul>
                    </div>
                </li>

                <!-- Social & Paie -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuPaie" role="button" aria-expanded="@(currentController == "Payroll" || currentController == "Employees" ? "true" : "false")">
                        <i class="bi bi-cash-stack"></i><span>Social &amp; Paie</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "Payroll" || currentController == "Employees" ? "show" : "")" id="menuPaie">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link @(currentController == "Declarations" && ViewBag.Type?.ToString() == "CNSS" ? "active" : "")" href="@Url.Action("Index", "Declarations", new { type = "CNSS", clientId = selectedClientId })"><i class="bi bi-shield-check"></i> CNSS (DMS)</a></li>
                            <li class="nav-item"><a class="nav-link @(currentController == "Payroll" ? "active" : "")" href="@Url.Action("Index", "Payroll")"><i class="bi bi-file-earmark-spreadsheet"></i> Bulletins de paie</a></li>
                            <li class="nav-item"><a class="nav-link @(currentController == "Employees" ? "active" : "")" href="@Url.Action("Index", "Employees")"><i class="bi bi-person-badge"></i> Gestion des employ&#233;s</a></li>
                        </ul>
                    </div>
                </li>

                <!-- Juridique -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuJuridique" role="button" aria-expanded="@(currentController == "Juridique" ? "true" : "false")">
                        <i class="bi bi-bank"></i><span>Juridique</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "Juridique" ? "show" : "")" id="menuJuridique">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Juridique", new { type = "CreationSociete" })"><i class="bi bi-building-add"></i> Cr&#233;ation soci&#233;t&#233;</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Juridique", new { type = "ModificationStatuts" })"><i class="bi bi-pencil"></i> Modifications statuts</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Juridique", new { type = "PVAG" })"><i class="bi bi-file-earmark-check"></i> PV Assembl&#233;e G&#233;n&#233;rale</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Juridique", new { type = "DepotLegal" })"><i class="bi bi-archive"></i> D&#233;p&#244;t l&#233;gal</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Juridique", new { type = "RC" })"><i class="bi bi-card-text"></i> Registre de commerce</a></li>
                        </ul>
                    </div>
                </li>

                <!-- Documents -->
                <li class="nav-item">
                    <a class="nav-link d-flex align-items-center @(hasClient ? "" : "client-disabled")" data-bs-toggle="collapse" href="#menuDocuments" role="button" aria-expanded="@(currentController == "Documents" ? "true" : "false")">
                        <i class="bi bi-folder2-open"></i><span>Documents</span><i class="bi bi-chevron-down chevron-icon"></i>
                    </a>
                    <div class="collapse sidebar-submenu @(currentController == "Documents" ? "show" : "")" id="menuDocuments">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Documents", new { categorie = "PieceJustificative" })"><i class="bi bi-paperclip"></i> Pi&#232;ces justificatives</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Documents", new { categorie = "Contrat" })"><i class="bi bi-file-earmark-lock"></i> Contrats &amp; Statuts</a></li>
                            <li class="nav-item"><a class="nav-link" href="@Url.Action("Index", "Documents", new { categorie = "Formulaire" })"><i class="bi bi-ui-checks-grid"></i> Formulaires officiels</a></li>
                        </ul>
                    </div>
                </li>

                <!-- Client Tasks & Planning -->
                <li class="nav-item">
                    <a class="nav-link @(hasClient ? "" : "client-disabled")" href="@Url.Action("Index", "Tasks", new { clientId = selectedClientId })">
                        <i class="bi bi-check2-square"></i> T&#226;ches client
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link @(hasClient ? "" : "client-disabled")" href="@Url.Action("Index", "Planning", new { clientId = selectedClientId })">
                        <i class="bi bi-calendar3"></i> Planning client
                    </a>
                </li>
            </ul>
            <div style="height:2rem;"></div>
        </div>
    </nav>

    <!-- MAIN -->
    <div class="mc-main">
        <header class="mc-topbar">
            <div class="d-flex align-items-center gap-3">
                <button class="btn btn-link d-md-none text-dark p-0" onclick="document.getElementById('sidebar').classList.toggle('show')"><i class="bi bi-list fs-4"></i></button>
                <!-- Client Selector -->
                <div class="client-selector-wrapper" id="clientSelectorWrapper">
                    <div class="client-selector-btn" id="clientSelectorBtn" tabindex="0" role="combobox" aria-expanded="false" aria-haspopup="listbox" aria-label="S&#233;lecteur de client actif">
                        <i class="bi bi-building text-primary"></i>
                        @if (hasClient)
                        {
                            <span class="selected-name" id="selectedClientDisplay">@selectedClientName</span>
                        }
                        else
                        {
                            <span class="no-selection" id="selectedClientDisplay">S&#233;lectionner un client...</span>
                        }
                        <i class="bi bi-chevron-expand ms-auto text-muted" style="font-size:0.75rem;"></i>
                    </div>
                    <div class="client-selector-dropdown" id="clientDropdown" role="listbox">
                        <input type="text" class="client-search-input" id="clientSearchInput" placeholder="Rechercher un client (nom, ICE)..." autocomplete="off" />
                        <div class="client-list" id="clientList">
                            <div class="text-center text-muted py-3" style="font-size:0.82rem;"><i class="bi bi-search"></i> Tapez pour rechercher...</div>
                        </div>
                        @if (hasClient)
                        {
                            <div class="client-clear-btn" id="clientClearBtn"><i class="bi bi-x-circle"></i> Retirer la s&#233;lection</div>
                        }
                    </div>
                </div>
            </div>
            <div class="user-menu">
                <div class="search-box d-none d-lg-block">
                    <i class="bi bi-search"></i>
                    <input type="text" class="form-control" placeholder="Rechercher..." id="globalSearch" />
                </div>
                <div class="dropdown">
                    <button class="btn btn-link text-decoration-none p-0" data-bs-toggle="dropdown"><i class="bi bi-translate fs-5 text-muted"></i></button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="?culture=fr-FR">&#127467;&#127479; Fran&#231;ais</a></li>
                        <li><a class="dropdown-item" href="?culture=ar-MA">&#127474;&#127462; &#1575;&#1604;&#1593;&#1585;&#1576;&#1610;&#1577;</a></li>
                        <li><a class="dropdown-item" href="?culture=en-US">&#127468;&#127463; English</a></li>
                    </ul>
                </div>
                <div class="notification-bell" id="notificationBell"><i class="bi bi-bell"></i><span class="badge bg-danger rounded-pill" id="notifCount" style="display:none;">0</span></div>
                <div class="dropdown">
                    <button class="btn btn-link text-decoration-none d-flex align-items-center gap-2 p-0" data-bs-toggle="dropdown">
                        <div class="user-avatar">@(Context.Session.GetString("UserName")?.Substring(0, 1).ToUpper() ?? "U")</div>
                        <div class="d-none d-md-block text-start">
                            <div class="fw-semibold text-dark" style="font-size:0.82rem;">@(Context.Session.GetString("UserName") ?? "Utilisateur")</div>
                            <div class="text-muted" style="font-size:0.68rem;">@(Context.Session.GetString("UserRole") ?? "")</div>
                        </div>
                        <i class="bi bi-chevron-down text-muted" style="font-size:0.7rem;"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="#"><i class="bi bi-person me-2"></i> Mon Profil</a></li>
                        <li><a class="dropdown-item" href="@Url.Action("Index", "Parametres")"><i class="bi bi-gear me-2"></i> Param&#232;tres</a></li>
                        <li><hr class="dropdown-divider" /></li>
                        <li><a class="dropdown-item text-danger" href="@Url.Action("Logout", "Account")"><i class="bi bi-box-arrow-right me-2"></i> D&#233;connexion</a></li>
                    </ul>
                </div>
            </div>
        </header>

        <div class="mc-content fade-in">
            @if (TempData["Success"] != null)
            {
                <div class="alert alert-success alert-dismissible fade show" role="alert">
                    <i class="bi bi-check-circle me-2"></i>@TempData["Success"]
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>
            }
            @if (TempData["Error"] != null)
            {
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    <i class="bi bi-exclamation-triangle me-2"></i>@TempData["Error"]
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>
            }
            @RenderBody()
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.8/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.8/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/echarts@5.5.0/dist/echarts.min.js"></script>
    <script>
        toastr.options = { positionClass: 'toast-top-right', closeButton: true, progressBar: true, timeOut: 4000 };
        $.extend($.fn.dataTable.defaults, { language: { url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/fr-FR.json' }, pageLength: 20, responsive: true });
        $(function() { $('.mc-datatable').DataTable(); });
        setTimeout(function() { $('.alert-dismissible').alert('close'); }, 5000);

        // CLIENT SELECTOR
        (function() {
            var wrapper = document.getElementById('clientSelectorWrapper');
            var btn = document.getElementById('clientSelectorBtn');
            var dropdown = document.getElementById('clientDropdown');
            var searchInput = document.getElementById('clientSearchInput');
            var clientList = document.getElementById('clientList');
            var display = document.getElementById('selectedClientDisplay');
            var clearBtn = document.getElementById('clientClearBtn');
            var debounceTimer = null;

            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                var isOpen = dropdown.classList.contains('show');
                dropdown.classList.toggle('show');
                btn.setAttribute('aria-expanded', !isOpen);
                if (!isOpen) { searchInput.value = ''; searchInput.focus(); loadClients(''); }
            });

            document.addEventListener('click', function(e) {
                if (!wrapper.contains(e.target)) { dropdown.classList.remove('show'); btn.setAttribute('aria-expanded', 'false'); }
            });

            searchInput.addEventListener('input', function() {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function() { loadClients(searchInput.value); }, 250);
            });

            function loadClients(query) {
                clientList.innerHTML = '<div class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary"></div></div>';
                $.get('@Url.Action("SearchClients", "Clients")', { q: query }, function(data) {
                    if (!data || data.length === 0) {
                        clientList.innerHTML = '<div class="text-center text-muted py-3" style="font-size:0.82rem;">Aucun client trouv&#233;</div>';
                        return;
                    }
                    var html = '';
                    var currentId = '@(selectedClientId ?? "")';
                    data.forEach(function(c) {
                        var initials = (c.raisonSociale || '??').substring(0,2).toUpperCase();
                        var isSelected = c.id.toString() === currentId;
                        html += '<div class="client-item ' + (isSelected ? 'selected' : '') + '" data-id="' + c.id + '" data-name="' + (c.raisonSociale || '').replace(/"/g, '&quot;') + '" role="option">';
                        html += '<div class="client-avatar">' + initials + '</div>';
                        html += '<div class="client-info"><div class="name">' + c.raisonSociale + '</div><div class="ice">' + (c.ice || '') + (c.formeJuridique ? ' - ' + c.formeJuridique : '') + '</div></div>';
                        if (isSelected) html += '<i class="bi bi-check-circle-fill text-primary"></i>';
                        html += '</div>';
                    });
                    clientList.innerHTML = html;
                    clientList.querySelectorAll('.client-item').forEach(function(item) {
                        item.addEventListener('click', function() { selectClient(item.dataset.id, item.dataset.name); });
                    });
                });
            }

            function selectClient(id, name) {
                $.post('@Url.Action("SetClientContext", "Clients")', { clientId: id }, function(res) {
                    if (res.success) { toastr.success('Client actif : ' + name); window.location.reload(); }
                });
            }

            if (clearBtn) {
                clearBtn.addEventListener('click', function(e) {
                    e.stopPropagation();
                    $.post('@Url.Action("SetClientContext", "Clients")', { clientId: '' }, function(res) {
                        if (res.success) { toastr.info('Client d\u00e9s\u00e9lectionn\u00e9'); window.location.reload(); }
                    });
                });
            }

            searchInput.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') { dropdown.classList.remove('show'); btn.focus(); }
            });
        })();
    </script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
"@

[System.IO.File]::WriteAllText($layoutPath, $content, [System.Text.Encoding]::UTF8)
Write-Host "DONE - Size: $((Get-Item $layoutPath).Length) bytes"

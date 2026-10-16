namespace MetaCompta.BackOffice.Tests.Helpers;

/// <summary>
/// Factory methods for creating test DTOs with realistic Moroccan data.
/// </summary>
public static class TestDataFactory
{
    // ── Clients ─────────────────────────────────────────────────
    public static ClientDto CreateClient(int id = 1, string statut = "Active") => new()
    {
        Id = id,
        RaisonSociale = $"Société Test {id} SARL",
        ICE = $"00{id}234567890{id:D3}",
        IF = $"1234{id:D4}",
        RC = $"RC-{id:D5}",
        NumeroCNSS = $"CNSS-{id:D7}",
        NumeroPatente = $"PAT-{id:D6}",
        FormeJuridique = "SARL",
        SecteurActivite = "Commerce",
        CapitalSocial = 100000m * id,
        AdresseSiegeSocial = $"Rue Mohammed V, N° {id}",
        VilleSiegeSocial = "Casablanca",
        TelephonePrincipal = $"+212522{id:D6}",
        EmailPrincipal = $"contact{id}@societe.ma",
        ContactNom = $"Ben Ahmed {id}",
        ContactFonction = "Gérant",
        TypeTVA = "Mensuelle",
        MoisClotureExercice = 12,
        Statut = statut,
        EmployeeCount = id * 5,
        DeclarationCount = id * 3
    };

    public static ClientCreateDto CreateClientDto() => new()
    {
        RaisonSociale = "Nouvelle Société SARL",
        ICE = "001234567890001",
        IF = "12340001",
        RC = "RC-00001",
        FormeJuridique = "SARL",
        SecteurActivite = "Services",
        VilleSiegeSocial = "Rabat",
        TelephonePrincipal = "+212537123456",
        EmailPrincipal = "contact@nouvelle.ma",
        TypeTVA = "Mensuelle",
        MoisClotureExercice = 12
    };

    public static PagedResult<ClientDto> CreateClientPage(int count = 5, int page = 1) => new()
    {
        Items = Enumerable.Range(1, count).Select(i => CreateClient(i)).ToList(),
        TotalCount = count * 2,
        Page = page,
        PageSize = 10
    };

    // ── Employees ───────────────────────────────────────────────
    public static EmployeeDto CreateEmployee(int id = 1, int clientId = 1) => new()
    {
        Id = id,
        CIN = $"BE{id:D6}",
        Nom = $"Alami{id}",
        Prenom = $"Mohamed{id}",
        ClientId = clientId,
        ClientName = $"Société {clientId} SARL",
        NumeroCNSS = $"CNSS-EMP-{id:D6}",
        DateEmbauche = new DateTime(2023, 1, 15),
        TypeContrat = "CDI",
        Poste = "Comptable",
        Departement = "Finance",
        SalaireBase = 8000m + (id * 500),
        Statut = "Actif",
        SituationFamiliale = "Marié",
        NombreEnfantsACharge = 2,
        ModePaiement = "Virement",
        RIB = $"007{id:D20}"
    };

    public static EmployeeCreateDto CreateEmployeeDto(int clientId = 1) => new()
    {
        CIN = "BE000001",
        Nom = "Alami",
        Prenom = "Mohamed",
        ClientId = clientId,
        DateEmbauche = new DateTime(2023, 1, 15),
        TypeContrat = "CDI",
        Poste = "Comptable",
        Departement = "Finance",
        SalaireBase = 8000m,
        SituationFamiliale = "Marié",
        NombreEnfantsACharge = 2,
        ModePaiement = "Virement"
    };

    public static PagedResult<EmployeeDto> CreateEmployeePage(int count = 5, int page = 1) => new()
    {
        Items = Enumerable.Range(1, count).Select(i => CreateEmployee(i)).ToList(),
        TotalCount = count * 2,
        Page = page,
        PageSize = 10
    };

    // ── Declarations ────────────────────────────────────────────
    public static DeclarationDto CreateDeclaration(int id = 1, string type = "TVA", string statut = "Brouillon") => new()
    {
        Id = id,
        ClientId = 1,
        ClientName = "Société Test SARL",
        Type = type,
        Periode = "Janvier 2025",
        DateDebut = new DateTime(2025, 1, 1),
        DateFin = new DateTime(2025, 1, 31),
        DateLimite = new DateTime(2025, 2, 20),
        Statut = statut,
        MontantDu = 15000m + (id * 1000),
        DaysUntilDeadline = 10 - id
    };

    public static DeclarationTVADto CreateTVAData() => new()
    {
        DeclarationId = 1,
        CAHT_20 = 500000m,
        CAHT_14 = 100000m,
        CAHT_10 = 50000m,
        CAHT_7 = 0m,
        CAExonere = 20000m,
        TVARecuperableCharges = 35000m,
        TVARecuperableImmobilisations = 10000m
    };

    public static PagedResult<DeclarationDto> CreateDeclarationPage(int count = 5, int page = 1) => new()
    {
        Items = Enumerable.Range(1, count).Select(i => CreateDeclaration(i)).ToList(),
        TotalCount = count * 2,
        Page = page,
        PageSize = 10
    };

    // ── Payroll ─────────────────────────────────────────────────
    public static PayrollPeriodDto CreatePayrollPeriod(int id = 1, int month = 1) => new()
    {
        Id = id,
        ClientId = 1,
        ClientName = "Société Test SARL",
        Year = 2025,
        Month = month,
        Status = "Brouillon",
        TotalEmployees = 10,
        TotalBrutGlobal = 120000m,
        TotalNetPay = 95000m,
        TotalChargesPatronales = 30000m
    };

    public static PaySlipDto CreatePaySlip(int id = 1) => new()
    {
        Id = id,
        EmployeeId = id,
        EmployeeName = $"Alami Mohamed{id}",
        Matricule = $"MAT-{id:D4}",
        PeriodLabel = "Janvier 2025",
        SalaireBase = 8000m,
        SalaireBrut = 8500m,
        CotisationCNSS = 375.36m,
        CotisationAMO = 191.25m,
        IR = 1200m,
        SalaireNet = 6733.39m,
        TotalChargesPatronales = 2200m
    };

    // ── Dashboard ───────────────────────────────────────────────
    public static DashboardDto CreateDashboard() => new()
    {
        TotalClientsActifs = 42,
        DeclarationsMoisEnCours = 38,
        DeclarationsEnAttente = 12,
        DeclarationsSoumises = 18,
        DocumentsEnAttente = 5,
        TachesEnCours = 15,
        ProchainsDelais = new List<DeclarationDto>
        {
            CreateDeclaration(1, "TVA", "EnAttente"),
            CreateDeclaration(2, "IS", "Brouillon"),
            CreateDeclaration(3, "CNSS", "EnAttente")
        },
        NotificationsRecentes = new List<NotificationDto>
        {
            new() { Id = 1, Titre = "Échéance TVA", Message = "TVA Janvier due le 20/02", Type = "Warning", CreatedAt = DateTime.Now },
            new() { Id = 2, Titre = "Nouveau client", Message = "Client ajouté", Type = "Info", CreatedAt = DateTime.Now }
        },
        Charts = new DashboardChartsDto()
    };

    // ── Cabinet Employees ───────────────────────────────────────
    public static CabinetEmployeeDto CreateCabinetEmployee(string id = "user-1", bool active = true) => new()
    {
        Id = id,
        Email = $"employee{id}@cabinet.ma",
        FirstName = "Mohamed",
        LastName = $"Alami",
        Role = "Manager",
        EstActif = active,
        DateEmbauche = new DateTime(2023, 1, 15),
        DerniereConnexion = DateTime.Now.AddDays(-1),
        Departement = "Comptabilite",
        Poste = "Comptable senior",
        ProfileImageUrl = null,
        PreferredLanguage = "fr-FR",
        Permissions = new List<string> { "client.view", "client.create", "declaration.view" }
    };

    public static CabinetEmployeeCreateDto CreateCabinetEmployeeCreateDto() => new()
    {
        Email = "nouveau@cabinet.ma",
        FirstName = "Ahmed",
        LastName = "Benani",
        Telephone = "+212600000000",
        Poste = "Assistant",
        Departement = "Fiscal",
        Role = "Assistant",
        Permissions = new List<string> { "client.view", "declaration.view" }
    };

    public static CabinetEmployeeUpdateDto CreateCabinetEmployeeUpdateDto() => new()
    {
        FirstName = "Ahmed",
        LastName = "Benani Modifié",
        Role = "Manager",
        IsActive = true,
        Permissions = new List<string> { "client.view", "client.create", "declaration.view", "declaration.edit" }
    };

    public static PagedResult<CabinetEmployeeDto> CreateCabinetEmployeePage(int count = 3) => new()
    {
        Items = Enumerable.Range(1, count).Select(i => CreateCabinetEmployee($"user-{i}")).ToList(),
        TotalCount = count,
        Page = 1,
        PageSize = 20
    };

    public static CabinetDetailDto CreateCabinetDetail() => new()
    {
        Id = 1,
        Name = "Cabinet Demo SARL",
        ICE = "001234567890001",
        IF = "12340001",
        RC = "RC-00001",
        Address = "Rue Mohammed V",
        City = "Casablanca",
        Phone = "+212522123456",
        Email = "contact@cabinet-demo.ma",
        IsActive = true,
        CreatedAt = new DateTime(2024, 1, 1),
        EmployeeCount = 5,
        ClientCount = 42,
        SubscriptionPlanName = "Premium",
        SubscriptionEndDate = DateTime.Now.AddMonths(6)
    };

    public static UserProfileDto CreateUserProfile() => new()
    {
        Id = "user-1",
        Email = "gerant@cabinet-demo.ma",
        FirstName = "Mohamed",
        LastName = "Alami",
        Role = "SuperAdmin",
        PreferredLanguage = "fr-FR",
        CreatedAt = new DateTime(2024, 1, 1),
        LastLoginAt = DateTime.Now.AddHours(-1),
        Permissions = new List<string> { "client.view", "client.create", "employee.create", "employee.edit" }
    };

    public static RoleDto CreateRole(string id = "role-1", string name = "Manager") => new()
    {
        Id = id,
        Name = name,
        Description = $"Rôle {name}",
        UserCount = 3,
        Permissions = new List<string> { "client.view", "client.create", "declaration.view" }
    };

    public static PagedResult<RoleDto> CreateRolePage(int count = 3) => new()
    {
        Items = Enumerable.Range(1, count).Select(i => CreateRole($"role-{i}", $"Role{i}")).ToList(),
        TotalCount = count,
        Page = 1,
        PageSize = 20
    };
}

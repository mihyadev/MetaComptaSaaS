using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Infrastructure.Data;

public class MetaComptaDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public MetaComptaDbContext(DbContextOptions<MetaComptaDbContext> options) : base(options) { }

    // System
    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ReferenceData> ReferenceData => Set<ReferenceData>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LocalizationEntry> LocalizationEntries => Set<LocalizationEntry>();

    // Business
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ServiceContrat> ServiceContrats => Set<ServiceContrat>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeAbsence> EmployeeAbsences => Set<EmployeeAbsence>();
    public DbSet<Declaration> Declarations => Set<Declaration>();
    public DbSet<DeclarationTVA> DeclarationsTVA => Set<DeclarationTVA>();
    public DbSet<TVALigneFacture> TVALignesFactures => Set<TVALigneFacture>();
    public DbSet<DeclarationIS> DeclarationsIS => Set<DeclarationIS>();
    public DbSet<DeclarationCNSS> DeclarationsCNSS => Set<DeclarationCNSS>();
    public DbSet<DeclarationHistory> DeclarationHistories => Set<DeclarationHistory>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PaySlip> PaySlips => Set<PaySlip>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<Message> Messages => Set<Message>();

    // Workflow Engine
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentMember> DepartmentMembers => Set<DepartmentMember>();
    public DbSet<WorkflowTemplate> WorkflowTemplates => Set<WorkflowTemplate>();
    public DbSet<WorkflowStepTemplate> WorkflowStepTemplates => Set<WorkflowStepTemplate>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowStepInstance> WorkflowStepInstances => Set<WorkflowStepInstance>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();
    public DbSet<ClientWorkflowConfig> ClientWorkflowConfigs => Set<ClientWorkflowConfig>();

    // Commercial & Facturation
    public DbSet<Devis> Devis => Set<Devis>();
    public DbSet<BonCommande> BonsCommande => Set<BonCommande>();
    public DbSet<ContratCommercial> ContratsCommerciaux => Set<ContratCommercial>();
    public DbSet<Facture> Factures => Set<Facture>();
    public DbSet<LigneDocumentCommercial> LignesDocumentsCommerciaux => Set<LigneDocumentCommercial>();
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<DocumentCommercialHistorique> DocumentsCommerciauxHistorique => Set<DocumentCommercialHistorique>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== Identity tables rename =====
        builder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Users");

            e.HasOne(u => u.Cabinet)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.CabinetId)
             .OnDelete(DeleteBehavior.SetNull);

            // Client relationship is configured in Client entity builder
        });
        builder.Entity<ApplicationRole>(e => e.ToTable("Roles"));

        // ===== Cabinet =====
        builder.Entity<Cabinet>(e =>
        {
            e.HasIndex(c => c.ICE).IsUnique().HasFilter("[ICE] IS NOT NULL");
            e.Property(c => c.Name).HasMaxLength(300).IsRequired();
        });

        // ===== Subscription Plan =====
        builder.Entity<SubscriptionPlan>(e =>
        {
            e.Property(s => s.MonthlyPrice).HasPrecision(18, 2);
            e.Property(s => s.AnnualPrice).HasPrecision(18, 2);
        });

        // ===== Permissions =====
        builder.Entity<Permission>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
        });

        builder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            e.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            e.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        // ===== Client =====
        builder.Entity<Client>(e =>
        {
            e.HasIndex(c => new { c.CabinetId, c.ICE }).IsUnique().HasFilter("[ICE] IS NOT NULL");
            e.HasIndex(c => new { c.CabinetId, c.IF }).IsUnique().HasFilter("[IF] IS NOT NULL");
            e.HasIndex(c => new { c.CabinetId, c.RC }).IsUnique().HasFilter("[RC] IS NOT NULL");
            e.HasIndex(c => new { c.CabinetId, c.NumeroCNSS }).IsUnique().HasFilter("[NumeroCNSS] IS NOT NULL");
            e.Property(c => c.RaisonSociale).HasMaxLength(500).IsRequired();
            e.Property(c => c.ICE).HasMaxLength(15);
            e.Property(c => c.CapitalSocial).HasPrecision(18, 2);
            e.HasQueryFilter(c => !c.IsDeleted);

            // Explicit relationships with ApplicationUser
            e.HasOne(c => c.ManagerPrincipal)
             .WithMany()
             .HasForeignKey(c => c.ManagerPrincipalId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(c => c.ExtranetUsers)
             .WithOne(u => u.Client)
             .HasForeignKey(u => u.ClientId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== ServiceContrat (Enterprise-grade client service contracts) =====
        builder.Entity<ServiceContrat>(e =>
        {
            e.ToTable("ServiceContrats");
            e.HasIndex(s => new { s.ClientId, s.TypeService, s.ExerciceFiscal }).IsUnique().HasFilter("[ExerciceFiscal] IS NOT NULL");
            e.HasIndex(s => new { s.ClientId, s.TypeService }).HasFilter("[ExerciceFiscal] IS NULL");
            e.Property(s => s.TypeService).HasMaxLength(50).IsRequired();
            e.Property(s => s.LibelleService).HasMaxLength(200);
            e.Property(s => s.Categorie).HasMaxLength(50);
            e.Property(s => s.Frequence).HasMaxLength(50);
            e.Property(s => s.HonorairesMensuels).HasPrecision(18, 2);
            e.Property(s => s.HonorairesAnnuels).HasPrecision(18, 2);
            e.Property(s => s.ResponsableId).HasMaxLength(450);
            e.HasQueryFilter(s => !s.IsDeleted);
            e.HasOne(s => s.Client).WithMany(c => c.ServiceContrats).HasForeignKey(s => s.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Responsable).WithMany().HasForeignKey(s => s.ResponsableId).OnDelete(DeleteBehavior.SetNull);
        });

        // ===== Employee =====
        builder.Entity<Employee>(e =>
        {
            e.HasIndex(emp => new { emp.ClientId, emp.NumeroCNSS }).IsUnique().HasFilter("[NumeroCNSS] IS NOT NULL");
            e.Property(emp => emp.SalaireBase).HasPrecision(18, 2);
            e.Property(emp => emp.PrimeAnciennete).HasPrecision(18, 2);
            e.Property(emp => emp.PrimeResponsabilite).HasPrecision(18, 2);
            e.Property(emp => emp.PrimePresence).HasPrecision(18, 2);
            e.Property(emp => emp.IndemniteTransport).HasPrecision(18, 2);
            e.Property(emp => emp.IndemniteRepresentation).HasPrecision(18, 2);
            e.Property(emp => emp.AvantagesNature).HasPrecision(18, 2);
            e.Property(emp => emp.TauxHoraire).HasPrecision(18, 4);
            e.HasQueryFilter(emp => !emp.IsDeleted);
            e.HasOne(emp => emp.Client).WithMany(c => c.Employees).HasForeignKey(emp => emp.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Declaration =====
        builder.Entity<Declaration>(e =>
        {
            e.Property(d => d.MontantDu).HasPrecision(18, 2);
            e.Property(d => d.MontantCredit).HasPrecision(18, 2);
            e.HasQueryFilter(d => !d.IsDeleted);
            e.HasOne(d => d.Client).WithMany(c => c.Declarations).HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Declaration TVA =====
        builder.Entity<DeclarationTVA>(e =>
        {
            e.Property(d => d.CAHT_20).HasPrecision(18, 2);
            e.Property(d => d.CAHT_14).HasPrecision(18, 2);
            e.Property(d => d.CAHT_10).HasPrecision(18, 2);
            e.Property(d => d.CAHT_7).HasPrecision(18, 2);
            e.Property(d => d.CAExonere).HasPrecision(18, 2);
            e.Property(d => d.TVA_20).HasPrecision(18, 2);
            e.Property(d => d.TVA_14).HasPrecision(18, 2);
            e.Property(d => d.TVA_10).HasPrecision(18, 2);
            e.Property(d => d.TVA_7).HasPrecision(18, 2);
            e.Property(d => d.TVACollecteeTotale).HasPrecision(18, 2);
            e.Property(d => d.TVARecuperableCharges).HasPrecision(18, 2);
            e.Property(d => d.TVARecuperableImmobilisations).HasPrecision(18, 2);
            e.Property(d => d.TVARecuperableTotale).HasPrecision(18, 2);
            e.Property(d => d.CreditPrecedent).HasPrecision(18, 2);
            e.Property(d => d.TVANette).HasPrecision(18, 2);
            e.Property(d => d.TVAaPayer).HasPrecision(18, 2);
            e.Property(d => d.CreditTVA).HasPrecision(18, 2);
        });

        // ===== TVA Lignes Factures =====
        builder.Entity<TVALigneFacture>(e =>
        {
            e.ToTable("TVALignesFactures");
            e.Property(l => l.MontantHT).HasPrecision(18, 2);
            e.Property(l => l.MontantTVA).HasPrecision(18, 2);
            e.Property(l => l.MontantTTC).HasPrecision(18, 2);
            e.Property(l => l.TauxTVA).HasPrecision(5, 2);
            e.Property(l => l.ProrataPercent).HasPrecision(5, 2);
            e.Property(l => l.TypePiece).HasMaxLength(50);
            e.Property(l => l.NumeroFacture).HasMaxLength(100);
            e.Property(l => l.FournisseurClient).HasMaxLength(300);
            e.Property(l => l.ICE).HasMaxLength(20);
            e.Property(l => l.IdentifiantFiscal).HasMaxLength(50);
            e.Property(l => l.Description).HasMaxLength(500);
            e.Property(l => l.TypeOperation).HasMaxLength(50);
            e.Property(l => l.Regime).HasMaxLength(50);
            e.HasOne(l => l.DeclarationTVA).WithMany(d => d.LignesFactures).HasForeignKey(l => l.DeclarationTVAId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(l => !l.IsDeleted);
        });

        // ===== Declaration IS =====
        builder.Entity<DeclarationIS>(e =>
        {
            e.Property(d => d.ResultatComptable).HasPrecision(18, 2);
            e.Property(d => d.Reintegrations).HasPrecision(18, 2);
            e.Property(d => d.Deductions).HasPrecision(18, 2);
            e.Property(d => d.ResultatFiscal).HasPrecision(18, 2);
            e.Property(d => d.DeficitsAnterieurs).HasPrecision(18, 2);
            e.Property(d => d.BaseImposable).HasPrecision(18, 2);
            e.Property(d => d.TauxIS).HasPrecision(5, 2);
            e.Property(d => d.ISDu).HasPrecision(18, 2);
            e.Property(d => d.AcomptesVerses).HasPrecision(18, 2);
            e.Property(d => d.ISNet).HasPrecision(18, 2);
            e.Property(d => d.Acompte1).HasPrecision(18, 2);
            e.Property(d => d.Acompte2).HasPrecision(18, 2);
            e.Property(d => d.Acompte3).HasPrecision(18, 2);
            e.Property(d => d.Acompte4).HasPrecision(18, 2);
        });

        // ===== Declaration CNSS =====
        builder.Entity<DeclarationCNSS>(e =>
        {
            e.Property(d => d.TotalSalairesBruts).HasPrecision(18, 2);
            e.Property(d => d.TotalCotisationSalariale).HasPrecision(18, 2);
            e.Property(d => d.TotalCotisationPatronale).HasPrecision(18, 2);
            e.Property(d => d.TotalAMOSalariale).HasPrecision(18, 2);
            e.Property(d => d.TotalAMOPatronale).HasPrecision(18, 2);
            e.Property(d => d.TotalFormationProfessionnelle).HasPrecision(18, 2);
            e.Property(d => d.MontantTotal).HasPrecision(18, 2);
        });

        // ===== PaySlip =====
        builder.Entity<PaySlip>(e =>
        {
            var decimalProps = e.Metadata.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));
            foreach (var prop in decimalProps)
            {
                prop.SetPrecision(18);
                prop.SetScale(2);
            }
            e.HasOne(ps => ps.Employee).WithMany(emp => emp.PaySlips).HasForeignKey(ps => ps.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ps => ps.PayrollPeriod).WithMany(pp => pp.PaySlips).HasForeignKey(ps => ps.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== PayrollPeriod =====
        builder.Entity<PayrollPeriod>(e =>
        {
            e.HasIndex(pp => new { pp.ClientId, pp.Annee, pp.Mois }).IsUnique();
            var decimalProps = e.Metadata.GetProperties().Where(p => p.ClrType == typeof(decimal));
            foreach (var prop in decimalProps) { prop.SetPrecision(18); prop.SetScale(2); }
            e.HasOne(pp => pp.Client).WithMany(c => c.PayrollPeriods).HasForeignKey(pp => pp.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Document =====
        builder.Entity<Document>(e =>
        {
            e.HasQueryFilter(d => !d.IsDeleted);
            e.HasOne(d => d.Client).WithMany(c => c.Documents).HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== ReferenceData =====
        builder.Entity<ReferenceData>(e =>
        {
            e.HasIndex(r => new { r.Category, r.Code }).IsUnique();
            e.Property(r => r.NumericValue).HasPrecision(18, 4);
        });

        // ===== AuditLog =====
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => new { a.EntityType, a.EntityId });
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.CreatedAt);
        });

        // ===== Localization =====
        builder.Entity<LocalizationEntry>(e =>
        {
            e.HasIndex(l => new { l.ResourceKey, l.Culture, l.CabinetId }).IsUnique();
        });

        // ===== Department =====
        builder.Entity<Department>(e =>
        {
            e.HasIndex(d => new { d.CabinetId, d.Code }).IsUnique();
            e.Property(d => d.Name).HasMaxLength(200).IsRequired();
            e.Property(d => d.Code).HasMaxLength(50).IsRequired();
            e.HasQueryFilter(d => !d.IsDeleted);
        });

        builder.Entity<DepartmentMember>(e =>
        {
            e.HasIndex(dm => new { dm.DepartmentId, dm.UserId }).IsUnique();
            e.HasOne(dm => dm.Department).WithMany(d => d.Members).HasForeignKey(dm => dm.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(dm => dm.User).WithMany().HasForeignKey(dm => dm.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(dm => !dm.IsDeleted);
        });

        // ===== Workflow Template =====
        builder.Entity<WorkflowTemplate>(e =>
        {
            e.Property(wt => wt.Name).HasMaxLength(300).IsRequired();
            e.HasIndex(wt => new { wt.CabinetId, wt.DeclarationType, wt.ClientId, wt.IsDefault })
             .HasFilter("[IsActive] = 1");
            e.HasQueryFilter(wt => !wt.IsDeleted);
            e.HasOne(wt => wt.Client).WithMany().HasForeignKey(wt => wt.ClientId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<WorkflowStepTemplate>(e =>
        {
            e.Property(ws => ws.Name).HasMaxLength(300).IsRequired();
            e.HasIndex(ws => new { ws.WorkflowTemplateId, ws.StepOrder }).IsUnique();
            e.HasOne(ws => ws.WorkflowTemplate).WithMany(wt => wt.Steps).HasForeignKey(ws => ws.WorkflowTemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ws => ws.AssignedToUser).WithMany().HasForeignKey(ws => ws.AssignedToUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(ws => ws.AssignedToDepartment).WithMany().HasForeignKey(ws => ws.AssignedToDepartmentId).OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(ws => !ws.IsDeleted);
        });

        // ===== Workflow Instance =====
        builder.Entity<WorkflowInstance>(e =>
        {
            e.HasIndex(wi => new { wi.CabinetId, wi.Status });
            e.HasIndex(wi => wi.DeclarationId);
            e.HasOne(wi => wi.WorkflowTemplate).WithMany(wt => wt.Instances).HasForeignKey(wi => wi.WorkflowTemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wi => wi.Declaration).WithMany().HasForeignKey(wi => wi.DeclarationId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(wi => wi.Client).WithMany().HasForeignKey(wi => wi.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wi => wi.StartedByUser).WithMany().HasForeignKey(wi => wi.StartedByUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(wi => !wi.IsDeleted);
        });

        builder.Entity<WorkflowStepInstance>(e =>
        {
            e.HasIndex(wsi => new { wsi.WorkflowInstanceId, wsi.StepOrder }).IsUnique();
            e.HasIndex(wsi => wsi.AssignedToUserId);
            e.HasOne(wsi => wsi.WorkflowInstance).WithMany(wi => wi.StepInstances).HasForeignKey(wsi => wsi.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wsi => wsi.WorkflowStepTemplate).WithMany().HasForeignKey(wsi => wsi.WorkflowStepTemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wsi => wsi.AssignedToUser).WithMany().HasForeignKey(wsi => wsi.AssignedToUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(wsi => wsi.CompletedByUser).WithMany().HasForeignKey(wsi => wsi.CompletedByUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(wsi => wsi.TaskItem).WithMany().HasForeignKey(wsi => wsi.TaskItemId).OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(wsi => !wsi.IsDeleted);
        });

        // ===== Workflow History =====
        builder.Entity<WorkflowHistory>(e =>
        {
            e.HasIndex(wh => wh.WorkflowInstanceId);
            e.HasIndex(wh => wh.ActionDate);
            e.HasOne(wh => wh.WorkflowInstance).WithMany(wi => wi.History).HasForeignKey(wh => wh.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wh => wh.WorkflowStepInstance).WithMany().HasForeignKey(wh => wh.WorkflowStepInstanceId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Client Workflow Config =====
        builder.Entity<ClientWorkflowConfig>(e =>
        {
            e.HasIndex(cwc => new { cwc.CabinetId, cwc.ClientId, cwc.DeclarationType }).IsUnique();
            e.Property(cwc => cwc.SLAMultiplier).HasPrecision(5, 2);
            e.HasOne(cwc => cwc.Client).WithMany().HasForeignKey(cwc => cwc.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(cwc => cwc.WorkflowTemplate).WithMany().HasForeignKey(cwc => cwc.WorkflowTemplateId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(cwc => cwc.PrimaryAssignee).WithMany().HasForeignKey(cwc => cwc.PrimaryAssigneeUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(cwc => !cwc.IsDeleted);
        });

        // ════════════════════════════════════════════════════════════════════
        //  COMMERCIAL & FACTURATION
        // ════════════════════════════════════════════════════════════════════

        // ===== Devis =====
        builder.Entity<Devis>(e =>
        {
            e.ToTable("Devis");
            e.HasIndex(d => new { d.CabinetId, d.Reference }).IsUnique();
            e.Property(d => d.Reference).HasMaxLength(20).IsRequired();
            e.Property(d => d.Objet).HasMaxLength(500);
            e.Property(d => d.Devise).HasMaxLength(10);
            e.Property(d => d.Statut).HasMaxLength(30);
            e.Property(d => d.MontantHT).HasPrecision(18, 2);
            e.Property(d => d.MontantTVA).HasPrecision(18, 2);
            e.Property(d => d.MontantTTC).HasPrecision(18, 2);
            e.Property(d => d.MontantRemise).HasPrecision(18, 2);
            e.Property(d => d.TauxChange).HasPrecision(18, 6);
            e.HasQueryFilter(d => !d.IsDeleted);
            e.HasOne(d => d.Client).WithMany(c => c.Devis).HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Commercial).WithMany().HasForeignKey(d => d.CommercialId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(d => d.Validateur).WithMany().HasForeignKey(d => d.ValidateurId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(d => d.ParentDevis).WithMany().HasForeignKey(d => d.ParentDevisId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Bon de Commande =====
        builder.Entity<BonCommande>(e =>
        {
            e.ToTable("BonsCommande");
            e.HasIndex(b => new { b.CabinetId, b.Reference }).IsUnique();
            e.Property(b => b.Reference).HasMaxLength(20).IsRequired();
            e.Property(b => b.Objet).HasMaxLength(500);
            e.Property(b => b.Devise).HasMaxLength(10);
            e.Property(b => b.Statut).HasMaxLength(30);
            e.Property(b => b.MontantHT).HasPrecision(18, 2);
            e.Property(b => b.MontantTVA).HasPrecision(18, 2);
            e.Property(b => b.MontantTTC).HasPrecision(18, 2);
            e.Property(b => b.MontantRemise).HasPrecision(18, 2);
            e.Property(b => b.TauxChange).HasPrecision(18, 6);
            e.HasQueryFilter(b => !b.IsDeleted);
            e.HasOne(b => b.Client).WithMany(c => c.BonsCommande).HasForeignKey(b => b.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Devis).WithOne(d => d.BonCommande).HasForeignKey<BonCommande>(b => b.DevisId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Responsable).WithMany().HasForeignKey(b => b.ResponsableId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Contrat Commercial =====
        builder.Entity<ContratCommercial>(e =>
        {
            e.ToTable("ContratsCommerciaux");
            e.HasIndex(c => new { c.CabinetId, c.Reference }).IsUnique();
            e.Property(c => c.Reference).HasMaxLength(20).IsRequired();
            e.Property(c => c.Objet).HasMaxLength(500);
            e.Property(c => c.Devise).HasMaxLength(10);
            e.Property(c => c.Statut).HasMaxLength(30);
            e.Property(c => c.ModeleFacturation).HasMaxLength(30);
            e.Property(c => c.FrequenceFacturation).HasMaxLength(30);
            e.Property(c => c.MontantHT).HasPrecision(18, 2);
            e.Property(c => c.MontantTVA).HasPrecision(18, 2);
            e.Property(c => c.MontantTTC).HasPrecision(18, 2);
            e.Property(c => c.MontantRemise).HasPrecision(18, 2);
            e.Property(c => c.TauxChange).HasPrecision(18, 6);
            e.HasQueryFilter(c => !c.IsDeleted);
            e.HasOne(c => c.Client).WithMany(cl => cl.ContratsCommerciaux).HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.BonCommande).WithOne(b => b.Contrat).HasForeignKey<ContratCommercial>(c => c.BonCommandeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Devis).WithOne(d => d.ContratDirect).HasForeignKey<ContratCommercial>(c => c.DevisId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Responsable).WithMany().HasForeignKey(c => c.ResponsableId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(c => c.ValidateurUser).WithMany().HasForeignKey(c => c.ValidateurId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Facture =====
        builder.Entity<Facture>(e =>
        {
            e.ToTable("Factures");
            e.HasIndex(f => new { f.CabinetId, f.Reference }).IsUnique();
            e.Property(f => f.Reference).HasMaxLength(20).IsRequired();
            e.Property(f => f.Objet).HasMaxLength(500);
            e.Property(f => f.Devise).HasMaxLength(10);
            e.Property(f => f.Statut).HasMaxLength(30);
            e.Property(f => f.MontantHT).HasPrecision(18, 2);
            e.Property(f => f.MontantTVA).HasPrecision(18, 2);
            e.Property(f => f.MontantTTC).HasPrecision(18, 2);
            e.Property(f => f.MontantRemise).HasPrecision(18, 2);
            e.Property(f => f.MontantPaye).HasPrecision(18, 2);
            e.Property(f => f.MontantRestant).HasPrecision(18, 2);
            e.Property(f => f.TauxChange).HasPrecision(18, 6);
            e.HasQueryFilter(f => !f.IsDeleted);
            e.HasOne(f => f.Client).WithMany(c => c.Factures).HasForeignKey(f => f.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Contrat).WithMany(c => c.Factures).HasForeignKey(f => f.ContratId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Emetteur).WithMany().HasForeignKey(f => f.EmetteurId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Ligne Document Commercial =====
        builder.Entity<LigneDocumentCommercial>(e =>
        {
            e.ToTable("LignesDocumentsCommerciaux");
            e.Property(l => l.TypeLigne).HasMaxLength(30);
            e.Property(l => l.TypeService).HasMaxLength(50);
            e.Property(l => l.CodeService).HasMaxLength(50);
            e.Property(l => l.Description).HasMaxLength(1000).IsRequired();
            e.Property(l => l.Unite).HasMaxLength(30);
            e.Property(l => l.Quantite).HasPrecision(18, 4);
            e.Property(l => l.PrixUnitaireHT).HasPrecision(18, 2);
            e.Property(l => l.RemisePourcentage).HasPrecision(5, 2);
            e.Property(l => l.RemiseMontant).HasPrecision(18, 2);
            e.Property(l => l.TauxTVA).HasPrecision(5, 2);
            e.Property(l => l.MontantTVA).HasPrecision(18, 2);
            e.Property(l => l.MontantHT).HasPrecision(18, 2);
            e.Property(l => l.MontantTTC).HasPrecision(18, 2);
            e.Property(l => l.QuantiteConsommee).HasPrecision(18, 4);
            e.HasOne(l => l.Devis).WithMany(d => d.Lignes).HasForeignKey(l => l.DevisId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(l => l.BonCommande).WithMany(b => b.Lignes).HasForeignKey(l => l.BonCommandeId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(l => l.Contrat).WithMany(c => c.Lignes).HasForeignKey(l => l.ContratId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(l => l.Facture).WithMany(f => f.Lignes).HasForeignKey(l => l.FactureId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(l => l.ServiceContrat).WithMany().HasForeignKey(l => l.ServiceContratId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Paiement =====
        builder.Entity<Paiement>(e =>
        {
            e.ToTable("Paiements");
            e.HasIndex(p => new { p.CabinetId, p.Reference }).IsUnique();
            e.Property(p => p.Reference).HasMaxLength(20).IsRequired();
            e.Property(p => p.Montant).HasPrecision(18, 2);
            e.Property(p => p.Devise).HasMaxLength(10);
            e.Property(p => p.ModePaiement).HasMaxLength(30);
            e.Property(p => p.Statut).HasMaxLength(30);
            e.HasOne(p => p.Facture).WithMany(f => f.Paiements).HasForeignKey(p => p.FactureId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.EnregistrePar).WithMany().HasForeignKey(p => p.EnregistreParId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Document Commercial Historique =====
        builder.Entity<DocumentCommercialHistorique>(e =>
        {
            e.ToTable("DocumentsCommerciauxHistorique");
            e.HasIndex(h => new { h.TypeDocument, h.DocumentId });
            e.HasIndex(h => h.DateAction);
            e.Property(h => h.TypeDocument).HasMaxLength(30);
            e.Property(h => h.Action).HasMaxLength(100);
            e.Property(h => h.AncienStatut).HasMaxLength(30);
            e.Property(h => h.NouveauStatut).HasMaxLength(30);
            e.HasOne(h => h.Devis).WithMany(d => d.Historique).HasForeignKey(h => h.DevisId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(h => h.BonCommande).WithMany(b => b.Historique).HasForeignKey(h => h.BonCommandeId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(h => h.Contrat).WithMany(c => c.Historique).HasForeignKey(h => h.ContratId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(h => h.Facture).WithMany(f => f.Historique).HasForeignKey(h => h.FactureId).OnDelete(DeleteBehavior.NoAction);
        });

        // ===== Seed default data =====
        // Override all remaining cascade deletes to Restrict to prevent SQL Server cycle errors
        foreach (var relationship in builder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed Subscription Plans
        builder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = 1, Name = "Starter", Description = "Pour les petits cabinets", MonthlyPrice = 499m, AnnualPrice = 4990m, MaxInternalUsers = 3, MaxClientCompanies = 10, MaxExtranetUsersPerClient = 5, MaxStorageMB = 2048 },
            new SubscriptionPlan { Id = 2, Name = "Professionnel", Description = "Pour les cabinets en croissance", MonthlyPrice = 999m, AnnualPrice = 9990m, MaxInternalUsers = 10, MaxClientCompanies = 50, MaxExtranetUsersPerClient = 10, MaxStorageMB = 10240 },
            new SubscriptionPlan { Id = 3, Name = "Entreprise", Description = "Pour les grands cabinets", MonthlyPrice = 1999m, AnnualPrice = 19990m, MaxInternalUsers = 50, MaxClientCompanies = 200, MaxExtranetUsersPerClient = 20, MaxStorageMB = 51200 }
        );

        // Seed Permissions
        var permissions = new List<Permission>();
        int id = 1;
        var modules = new Dictionary<string, string[]>
        {
            ["Client"] = new[] { "View", "Create", "Edit", "Delete", "Archive", "Configure" },
            ["Declaration"] = new[] { "View", "Create", "Edit", "Validate", "Submit", "Delete", "Revert" },
            ["Employee"] = new[] { "View", "Create", "Edit", "Delete", "Archive", "ManageAbsences" },
            ["Payroll"] = new[] { "View", "Create", "Edit", "Validate", "Process", "GenerateSlips", "GenerateBankFile" },
            ["Document"] = new[] { "View", "Upload", "Edit", "Validate", "Delete", "Share", "Download" },
            ["Task"] = new[] { "View", "Create", "Edit", "Delete", "Assign", "ManageAll" },
            ["Report"] = new[] { "View", "Export", "ViewStrategic" },
            ["Notification"] = new[] { "View", "Manage", "Configure" },
            ["System"] = new[] { "Admin", "UserManage", "ProfileManage", "AuditView", "ConfigurationView", "ConfigurationEdit" },
            ["Workflow"] = new[] { "View", "Create", "Edit", "Delete", "Configure", "Advance", "Validate", "Reassign", "Cancel", "Monitor" },
            ["Department"] = new[] { "View", "Create", "Edit", "Delete", "ManageMembers" },
            ["Commercial"] = new[] { "View", "Create", "Edit", "Delete", "Validate", "Convert", "Sign", "GenerateInvoice" },
            ["Facture"] = new[] { "View", "Create", "Edit", "Send", "Cancel", "Refund", "ManagePayments" },
        };
        foreach (var module in modules)
        {
            foreach (var action in module.Value)
            {
                permissions.Add(new Permission
                {
                    Id = id++,
                    Name = $"{module.Key}.{action}",
                    Module = module.Key,
                    Action = action,
                    Description = $"Permission to {action} in {module.Key} module"
                });
            }
        }
        builder.Entity<Permission>().HasData(permissions);

        // Seed Reference Data (TVA rates, CNSS rates, IR brackets, etc.)
        builder.Entity<ReferenceData>().HasData(
            // TVA Rates
            new ReferenceData { Id = 1, Category = "TVA_RATE", Code = "TVA_20", Label = "TVA 20%", NumericValue = 20m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 2, Category = "TVA_RATE", Code = "TVA_14", Label = "TVA 14%", NumericValue = 14m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 3, Category = "TVA_RATE", Code = "TVA_10", Label = "TVA 10%", NumericValue = 10m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 4, Category = "TVA_RATE", Code = "TVA_7", Label = "TVA 7%", NumericValue = 7m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // CNSS Rates
            new ReferenceData { Id = 5, Category = "CNSS", Code = "PLAFOND", Label = "Plafond CNSS", NumericValue = 6000m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 6, Category = "CNSS", Code = "TAUX_SALARIAL", Label = "Taux CNSS Salariale", NumericValue = 6.74m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 7, Category = "CNSS", Code = "TAUX_PATRONAL", Label = "Taux CNSS Patronale", NumericValue = 20.48m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // AMO Rates
            new ReferenceData { Id = 8, Category = "AMO", Code = "TAUX_SALARIAL", Label = "Taux AMO Salariale", NumericValue = 2.26m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 9, Category = "AMO", Code = "TAUX_PATRONAL", Label = "Taux AMO Patronale", NumericValue = 4.11m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // Formation Professionnelle
            new ReferenceData { Id = 10, Category = "FP", Code = "TAUX", Label = "Taux Formation Prof.", NumericValue = 1.6m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // IR Brackets (progressive)
            new ReferenceData { Id = 11, Category = "IR_BRACKET", Code = "T1", Label = "0 - 30 000 DH", Value = "{\"min\":0,\"max\":30000,\"rate\":0}", NumericValue = 0m, SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 12, Category = "IR_BRACKET", Code = "T2", Label = "30 001 - 50 000 DH", Value = "{\"min\":30001,\"max\":50000,\"rate\":10}", NumericValue = 10m, SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 13, Category = "IR_BRACKET", Code = "T3", Label = "50 001 - 60 000 DH", Value = "{\"min\":50001,\"max\":60000,\"rate\":20}", NumericValue = 20m, SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 14, Category = "IR_BRACKET", Code = "T4", Label = "60 001 - 80 000 DH", Value = "{\"min\":60001,\"max\":80000,\"rate\":30}", NumericValue = 30m, SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 15, Category = "IR_BRACKET", Code = "T5", Label = "80 001 - 180 000 DH", Value = "{\"min\":80001,\"max\":180000,\"rate\":34}", NumericValue = 34m, SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 16, Category = "IR_BRACKET", Code = "T6", Label = "> 180 000 DH", Value = "{\"min\":180001,\"max\":999999999,\"rate\":38}", NumericValue = 38m, SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // IS Rates
            new ReferenceData { Id = 17, Category = "IS_RATE", Code = "NORMAL", Label = "Taux IS Normal", NumericValue = 31m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 18, Category = "IS_RATE", Code = "REDUIT", Label = "Taux IS Réduit (PME)", NumericValue = 20m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 19, Category = "IS_RATE", Code = "MINIMUM", Label = "Taux IS Minimum", NumericValue = 10m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // IR Family deductions
            new ReferenceData { Id = 20, Category = "IR_DEDUCTION", Code = "CHARGE_FAMILLE", Label = "Déduction par enfant", NumericValue = 360m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 21, Category = "IR_DEDUCTION", Code = "PLAFOND_ENFANTS", Label = "Plafond nombre enfants", NumericValue = 6m, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // Document categories (old IDs preserved)
            new ReferenceData { Id = 22, Category = "DOC_CATEGORY", Code = "LEGAL", Label = "Documents Légaux", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 23, Category = "DOC_CATEGORY", Code = "FISCAL", Label = "Documents Fiscaux", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },
            new ReferenceData { Id = 24, Category = "DOC_CATEGORY", Code = "SOCIAL", Label = "Documents Sociaux", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success" },
            new ReferenceData { Id = 25, Category = "DOC_CATEGORY", Code = "COMPTABLE", Label = "Documents Comptables", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info" },
            new ReferenceData { Id = 26, Category = "DOC_CATEGORY", Code = "RH", Label = "Documents RH", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#d97706", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 27, Category = "DOC_CATEGORY", Code = "ADMIN", Label = "Documents Administratifs", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary" },

            // Contract types (old IDs preserved)
            new ReferenceData { Id = 32, Category = "TYPE_CONTRAT", Code = "CDI", Label = "CDI", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success" },
            new ReferenceData { Id = 33, Category = "TYPE_CONTRAT", Code = "CDD", Label = "CDD", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 34, Category = "TYPE_CONTRAT", Code = "INTERIM", Label = "Intérim", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info" },
            new ReferenceData { Id = 35, Category = "TYPE_CONTRAT", Code = "STAGE", Label = "Stage", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },

            // Forme Juridique
            new ReferenceData { Id = 28, Category = "FORME_JURIDIQUE", Code = "SARL", Label = "SARL", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 29, Category = "FORME_JURIDIQUE", Code = "SA", Label = "SA", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },
            new ReferenceData { Id = 30, Category = "FORME_JURIDIQUE", Code = "SNC", Label = "SNC", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success" },
            new ReferenceData { Id = 31, Category = "FORME_JURIDIQUE", Code = "SCS", Label = "SCS", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info" },
            new ReferenceData { Id = 36, Category = "FORME_JURIDIQUE", Code = "SCA", Label = "SCA", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#d97706", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 37, Category = "FORME_JURIDIQUE", Code = "AUTO_ENTREPRENEUR", Label = "Auto-Entrepreneur", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 38, Category = "FORME_JURIDIQUE", Code = "PERSONNE_PHYSIQUE", Label = "Personne Physique", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary" },
            new ReferenceData { Id = 39, Category = "FORME_JURIDIQUE", Code = "ASSOCIATION", Label = "Association", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#ec4899", CssClass = "bg-pink-subtle text-pink" },
            new ReferenceData { Id = 40, Category = "FORME_JURIDIQUE", Code = "COOPERATIVE", Label = "Coopérative", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#14b8a6", CssClass = "bg-teal-subtle text-teal" },
            new ReferenceData { Id = 41, Category = "FORME_JURIDIQUE", Code = "EI", Label = "Entreprise Individuelle", SortOrder = 10, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#6366f1", CssClass = "bg-indigo-subtle text-indigo" },

            // Statut Client
            new ReferenceData { Id = 42, Category = "STATUT_CLIENT", Code = "Active", Label = "Actif", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 43, Category = "STATUT_CLIENT", Code = "Inactive", Label = "Inactif", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },
            new ReferenceData { Id = 44, Category = "STATUT_CLIENT", Code = "Dormante", Label = "Dormant", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-pause-circle" },
            new ReferenceData { Id = 45, Category = "STATUT_CLIENT", Code = "Dissoute", Label = "Dissout", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-slash-circle" },

            // Statut Employé
            new ReferenceData { Id = 46, Category = "STATUT_EMPLOYE", Code = "Actif", Label = "Actif", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 47, Category = "STATUT_EMPLOYE", Code = "Inactif", Label = "Inactif", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },
            new ReferenceData { Id = 48, Category = "STATUT_EMPLOYE", Code = "Suspendu", Label = "Suspendu", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-pause-circle" },

            // Type Déclaration
            new ReferenceData { Id = 49, Category = "TYPE_DECLARATION", Code = "TVA", Label = "TVA", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-receipt" },
            new ReferenceData { Id = 50, Category = "TYPE_DECLARATION", Code = "IS", Label = "Impôt sur les Sociétés", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-building" },
            new ReferenceData { Id = 51, Category = "TYPE_DECLARATION", Code = "IR", Label = "Impôt sur le Revenu", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-person" },
            new ReferenceData { Id = 52, Category = "TYPE_DECLARATION", Code = "CNSS", Label = "CNSS", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info", Icone = "bi-shield" },
            new ReferenceData { Id = 53, Category = "TYPE_DECLARATION", Code = "IR_SALAIRES", Label = "IR Salaires", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#d97706", CssClass = "bg-warning-subtle text-warning", Icone = "bi-cash" },

            // Statut Déclaration
            new ReferenceData { Id = 54, Category = "STATUT_DECLARATION", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "badge-brouillon", Icone = "bi-pencil" },
            new ReferenceData { Id = 55, Category = "STATUT_DECLARATION", Code = "ACompleter", Label = "À Compléter", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "badge-acompleter", Icone = "bi-exclamation-circle" },
            new ReferenceData { Id = 56, Category = "STATUT_DECLARATION", Code = "Completee", Label = "Complétée", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "badge-completee", Icone = "bi-check" },
            new ReferenceData { Id = 57, Category = "STATUT_DECLARATION", Code = "EnRevision", Label = "En Révision", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "badge-enrevision", Icone = "bi-eye" },
            new ReferenceData { Id = 58, Category = "STATUT_DECLARATION", Code = "Validee", Label = "Validée", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "badge-validee", Icone = "bi-check-circle" },
            new ReferenceData { Id = 59, Category = "STATUT_DECLARATION", Code = "Soumise", Label = "Soumise", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#166534", CssClass = "badge-soumise", Icone = "bi-send" },
            new ReferenceData { Id = 60, Category = "STATUT_DECLARATION", Code = "Refusee", Label = "Refusée", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "badge-refusee", Icone = "bi-x-circle" },
            new ReferenceData { Id = 300, Category = "STATUT_DECLARATION", Code = "EnCours", Label = "En Cours", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#3b82f6", CssClass = "badge-acompleter", Icone = "bi-arrow-repeat" },
            new ReferenceData { Id = 301, Category = "STATUT_DECLARATION", Code = "EnPreparation", Label = "En Préparation", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#8b5cf6", CssClass = "badge-enrevision", Icone = "bi-gear" },
            new ReferenceData { Id = 302, Category = "STATUT_DECLARATION", Code = "Payee", Label = "Payée", SortOrder = 10, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#047857", CssClass = "badge-soumise", Icone = "bi-cash-coin" },
            new ReferenceData { Id = 303, Category = "STATUT_DECLARATION", Code = "Rejetee", Label = "Rejetée", SortOrder = 11, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#b91c1c", CssClass = "badge-refusee", Icone = "bi-x-octagon" },

            // Type TVA (régime)
            new ReferenceData { Id = 61, Category = "TYPE_TVA", Code = "Mensuelle", Label = "Mensuelle", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 62, Category = "TYPE_TVA", Code = "Trimestrielle", Label = "Trimestrielle", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },
            new ReferenceData { Id = 63, Category = "TYPE_TVA", Code = "Exonere", Label = "Exonéré", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary" },

            // Villes Maroc
            new ReferenceData { Id = 64, Category = "VILLE", Code = "CASABLANCA", Label = "Casablanca", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 65, Category = "VILLE", Code = "RABAT", Label = "Rabat", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 66, Category = "VILLE", Code = "MARRAKECH", Label = "Marrakech", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 67, Category = "VILLE", Code = "FES", Label = "Fès", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 68, Category = "VILLE", Code = "TANGER", Label = "Tanger", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 69, Category = "VILLE", Code = "AGADIR", Label = "Agadir", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 70, Category = "VILLE", Code = "MEKNES", Label = "Meknès", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 71, Category = "VILLE", Code = "OUJDA", Label = "Oujda", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 72, Category = "VILLE", Code = "KENITRA", Label = "Kénitra", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 73, Category = "VILLE", Code = "TETOUAN", Label = "Tétouan", SortOrder = 10, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 74, Category = "VILLE", Code = "SALE", Label = "Salé", SortOrder = 11, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 75, Category = "VILLE", Code = "NADOR", Label = "Nador", SortOrder = 12, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 76, Category = "VILLE", Code = "MOHAMMEDIA", Label = "Mohammédia", SortOrder = 13, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 77, Category = "VILLE", Code = "EL_JADIDA", Label = "El Jadida", SortOrder = 14, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 78, Category = "VILLE", Code = "BENI_MELLAL", Label = "Béni Mellal", SortOrder = 15, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 79, Category = "VILLE", Code = "KHOURIBGA", Label = "Khouribga", SortOrder = 16, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 80, Category = "VILLE", Code = "SETTAT", Label = "Settat", SortOrder = 17, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 81, Category = "VILLE", Code = "SAFI", Label = "Safi", SortOrder = 18, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 82, Category = "VILLE", Code = "LAAYOUNE", Label = "Laâyoune", SortOrder = 19, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 83, Category = "VILLE", Code = "DAKHLA", Label = "Dakhla", SortOrder = 20, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // Situation Familiale
            new ReferenceData { Id = 84, Category = "SITUATION_FAMILIALE", Code = "Celibataire", Label = "Célibataire", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-person" },
            new ReferenceData { Id = 85, Category = "SITUATION_FAMILIALE", Code = "Marie", Label = "Marié(e)", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-people" },
            new ReferenceData { Id = 86, Category = "SITUATION_FAMILIALE", Code = "Divorce", Label = "Divorcé(e)", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-person-dash" },
            new ReferenceData { Id = 87, Category = "SITUATION_FAMILIALE", Code = "Veuf", Label = "Veuf(ve)", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-person-x" },

            // Sexe
            new ReferenceData { Id = 88, Category = "SEXE", Code = "M", Label = "Masculin", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 89, Category = "SEXE", Code = "F", Label = "Féminin", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // Mode Paiement
            new ReferenceData { Id = 90, Category = "MODE_PAIEMENT", Code = "Virement", Label = "Virement bancaire", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-bank" },
            new ReferenceData { Id = 91, Category = "MODE_PAIEMENT", Code = "Cheque", Label = "Chèque", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-credit-card" },
            new ReferenceData { Id = 92, Category = "MODE_PAIEMENT", Code = "Especes", Label = "Espèces", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-cash" },

            // Type Absence
            new ReferenceData { Id = 93, Category = "TYPE_ABSENCE", Code = "Annuel", Label = "Congé annuel", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 94, Category = "TYPE_ABSENCE", Code = "Maladie", Label = "Congé maladie", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger" },
            new ReferenceData { Id = 95, Category = "TYPE_ABSENCE", Code = "SansSolde", Label = "Congé sans solde", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 96, Category = "TYPE_ABSENCE", Code = "Maternite", Label = "Congé maternité", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#ec4899", CssClass = "bg-pink-subtle text-pink" },
            new ReferenceData { Id = 97, Category = "TYPE_ABSENCE", Code = "Exceptionnel", Label = "Congé exceptionnel", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },

            // Statut Document
            new ReferenceData { Id = 98, Category = "STATUT_DOCUMENT", Code = "EnAttente", Label = "En attente", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-clock" },
            new ReferenceData { Id = 99, Category = "STATUT_DOCUMENT", Code = "Valide", Label = "Validé", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 100, Category = "STATUT_DOCUMENT", Code = "Rejete", Label = "Rejeté", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },
            new ReferenceData { Id = 101, Category = "STATUT_DOCUMENT", Code = "Archive", Label = "Archivé", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-archive" },

            // Priorité Tâche
            new ReferenceData { Id = 102, Category = "PRIORITE_TACHE", Code = "Faible", Label = "Faible", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "badge-faible", Icone = "bi-arrow-down" },
            new ReferenceData { Id = 103, Category = "PRIORITE_TACHE", Code = "Moyenne", Label = "Moyenne", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "badge-moyenne", Icone = "bi-arrow-right" },
            new ReferenceData { Id = 104, Category = "PRIORITE_TACHE", Code = "Haute", Label = "Haute", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#ea580c", CssClass = "badge-haute", Icone = "bi-arrow-up" },
            new ReferenceData { Id = 105, Category = "PRIORITE_TACHE", Code = "Urgente", Label = "Urgente", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "badge-urgente", Icone = "bi-exclamation-triangle" },

            // Statut Tâche
            new ReferenceData { Id = 106, Category = "STATUT_TACHE", Code = "AFaire", Label = "À Faire", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-circle" },
            new ReferenceData { Id = 107, Category = "STATUT_TACHE", Code = "EnCours", Label = "En Cours", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-play-circle" },
            new ReferenceData { Id = 108, Category = "STATUT_TACHE", Code = "EnAttenteRevision", Label = "En Attente Révision", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-eye" },
            new ReferenceData { Id = 109, Category = "STATUT_TACHE", Code = "Termine", Label = "Terminé", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 110, Category = "STATUT_TACHE", Code = "Annule", Label = "Annulé", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },

            // Statut Paie
            new ReferenceData { Id = 111, Category = "STATUT_PAIE", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "badge-brouillon", Icone = "bi-pencil" },
            new ReferenceData { Id = 112, Category = "STATUT_PAIE", Code = "EnCours", Label = "En Cours", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-play-circle" },
            new ReferenceData { Id = 113, Category = "STATUT_PAIE", Code = "EnAttenteValidation", Label = "En Attente Validation", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-clock" },
            new ReferenceData { Id = 114, Category = "STATUT_PAIE", Code = "Validee", Label = "Validée", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 115, Category = "STATUT_PAIE", Code = "Cloturee", Label = "Clôturée", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-lock" },

            // Secteur Activité
            new ReferenceData { Id = 116, Category = "SECTEUR_ACTIVITE", Code = "COMMERCE", Label = "Commerce", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 117, Category = "SECTEUR_ACTIVITE", Code = "SERVICES", Label = "Services", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 118, Category = "SECTEUR_ACTIVITE", Code = "INDUSTRIE", Label = "Industrie", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 119, Category = "SECTEUR_ACTIVITE", Code = "BTP", Label = "BTP", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 120, Category = "SECTEUR_ACTIVITE", Code = "IMMOBILIER", Label = "Immobilier", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 121, Category = "SECTEUR_ACTIVITE", Code = "TRANSPORT", Label = "Transport & Logistique", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 122, Category = "SECTEUR_ACTIVITE", Code = "RESTAURATION", Label = "Hôtellerie & Restauration", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 123, Category = "SECTEUR_ACTIVITE", Code = "TECH", Label = "Technologies & IT", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 124, Category = "SECTEUR_ACTIVITE", Code = "AGRICULTURE", Label = "Agriculture", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 125, Category = "SECTEUR_ACTIVITE", Code = "SANTE", Label = "Santé", SortOrder = 10, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 126, Category = "SECTEUR_ACTIVITE", Code = "EDUCATION", Label = "Éducation & Formation", SortOrder = 11, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },
            new ReferenceData { Id = 127, Category = "SECTEUR_ACTIVITE", Code = "AUTRE", Label = "Autre", SortOrder = 99, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1) },

            // Catégorie Document
            new ReferenceData { Id = 128, Category = "CATEGORIE_DOCUMENT", Code = "Legal", Label = "Documents Légaux", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-file-earmark-text", Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 129, Category = "CATEGORIE_DOCUMENT", Code = "Fiscal", Label = "Documents Fiscaux", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-calculator", Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },
            new ReferenceData { Id = 130, Category = "CATEGORIE_DOCUMENT", Code = "Social", Label = "Documents Sociaux", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-people", Couleur = "#059669", CssClass = "bg-success-subtle text-success" },
            new ReferenceData { Id = 131, Category = "CATEGORIE_DOCUMENT", Code = "Comptable", Label = "Documents Comptables", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-journal-text", Couleur = "#0891b2", CssClass = "bg-info-subtle text-info" },
            new ReferenceData { Id = 132, Category = "CATEGORIE_DOCUMENT", Code = "RH", Label = "Documents RH", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-person-badge", Couleur = "#d97706", CssClass = "bg-warning-subtle text-warning" },
            new ReferenceData { Id = 133, Category = "CATEGORIE_DOCUMENT", Code = "Admin", Label = "Documents Administratifs", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-folder", Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary" },

            // ════════════════════════════════════════════════════════════════
            // STATUTS DOCUMENTS COMMERCIAUX (stockés en SQL selon la spec)
            // ════════════════════════════════════════════════════════════════

            // Statut Devis
            new ReferenceData { Id = 200, Category = "STATUT_DEVIS", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-pencil" },
            new ReferenceData { Id = 201, Category = "STATUT_DEVIS", Code = "Soumis", Label = "Soumis", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-send" },
            new ReferenceData { Id = 202, Category = "STATUT_DEVIS", Code = "Approuve", Label = "Approuvé", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 203, Category = "STATUT_DEVIS", Code = "Rejete", Label = "Rejeté", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },
            new ReferenceData { Id = 204, Category = "STATUT_DEVIS", Code = "Expire", Label = "Expiré", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-clock-history" },
            new ReferenceData { Id = 205, Category = "STATUT_DEVIS", Code = "Annule", Label = "Annulé", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-dark-subtle text-dark", Icone = "bi-slash-circle" },

            // Statut Bon de Commande
            new ReferenceData { Id = 210, Category = "STATUT_BON_COMMANDE", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-pencil" },
            new ReferenceData { Id = 211, Category = "STATUT_BON_COMMANDE", Code = "Confirme", Label = "Confirmé", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-check" },
            new ReferenceData { Id = 212, Category = "STATUT_BON_COMMANDE", Code = "Approuve", Label = "Approuvé", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 213, Category = "STATUT_BON_COMMANDE", Code = "Annule", Label = "Annulé", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },

            // Statut Contrat
            new ReferenceData { Id = 220, Category = "STATUT_CONTRAT", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-pencil" },
            new ReferenceData { Id = 221, Category = "STATUT_CONTRAT", Code = "EnCours", Label = "En cours", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-arrow-repeat" },
            new ReferenceData { Id = 222, Category = "STATUT_CONTRAT", Code = "Signe", Label = "Signé", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-pen" },
            new ReferenceData { Id = 223, Category = "STATUT_CONTRAT", Code = "Confirme", Label = "Confirmé", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info", Icone = "bi-check" },
            new ReferenceData { Id = 224, Category = "STATUT_CONTRAT", Code = "Valide", Label = "Validé", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle-fill" },
            new ReferenceData { Id = 225, Category = "STATUT_CONTRAT", Code = "Actif", Label = "Actif", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#166534", CssClass = "bg-success text-white", Icone = "bi-play-circle" },
            new ReferenceData { Id = 226, Category = "STATUT_CONTRAT", Code = "Suspendu", Label = "Suspendu", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-pause-circle" },
            new ReferenceData { Id = 227, Category = "STATUT_CONTRAT", Code = "Termine", Label = "Terminé", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-stop-circle" },
            new ReferenceData { Id = 228, Category = "STATUT_CONTRAT", Code = "Annule", Label = "Annulé", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },

            // Statut Facture
            new ReferenceData { Id = 230, Category = "STATUT_FACTURE", Code = "Brouillon", Label = "Brouillon", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#94a3b8", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-pencil" },
            new ReferenceData { Id = 231, Category = "STATUT_FACTURE", Code = "Generee", Label = "Générée", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-file-earmark-text" },
            new ReferenceData { Id = 232, Category = "STATUT_FACTURE", Code = "Envoyee", Label = "Envoyée", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-send" },
            new ReferenceData { Id = 233, Category = "STATUT_FACTURE", Code = "Vue", Label = "Vue", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info", Icone = "bi-eye" },
            new ReferenceData { Id = 234, Category = "STATUT_FACTURE", Code = "PartiellementPayee", Label = "Partiellement payée", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-cash" },
            new ReferenceData { Id = 235, Category = "STATUT_FACTURE", Code = "Payee", Label = "Payée", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 236, Category = "STATUT_FACTURE", Code = "EnRetard", Label = "En retard", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-exclamation-triangle" },
            new ReferenceData { Id = 237, Category = "STATUT_FACTURE", Code = "Annulee", Label = "Annulée", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-dark-subtle text-dark", Icone = "bi-x-circle" },
            new ReferenceData { Id = 238, Category = "STATUT_FACTURE", Code = "Remboursee", Label = "Remboursée", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#ec4899", CssClass = "bg-pink-subtle text-pink", Icone = "bi-arrow-counterclockwise" },

            // Statut Paiement
            new ReferenceData { Id = 240, Category = "STATUT_PAIEMENT", Code = "Valide", Label = "Validé", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-check-circle" },
            new ReferenceData { Id = 241, Category = "STATUT_PAIEMENT", Code = "EnAttente", Label = "En attente", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-clock" },
            new ReferenceData { Id = 242, Category = "STATUT_PAIEMENT", Code = "Rejete", Label = "Rejeté", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-x-circle" },

            // Mode de Facturation
            new ReferenceData { Id = 250, Category = "MODELE_FACTURATION", Code = "Forfait", Label = "Forfait (Prix fixe)", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-box" },
            new ReferenceData { Id = 251, Category = "MODELE_FACTURATION", Code = "Regie", Label = "Régie (Temps & Matériel)", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-clock-history" },

            // Type de Ligne Document
            new ReferenceData { Id = 255, Category = "TYPE_LIGNE", Code = "Service", Label = "Prestation de service", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary" },
            new ReferenceData { Id = 256, Category = "TYPE_LIGNE", Code = "Regie", Label = "Régie (Temps & Matériel)", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple" },
            new ReferenceData { Id = 257, Category = "TYPE_LIGNE", Code = "Forfait", Label = "Forfait (Prix fixe)", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success" },

            // Mode de Paiement (Commercial)
            new ReferenceData { Id = 260, Category = "MODE_PAIEMENT_COMMERCIAL", Code = "Virement", Label = "Virement bancaire", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-bank" },
            new ReferenceData { Id = 261, Category = "MODE_PAIEMENT_COMMERCIAL", Code = "Cheque", Label = "Chèque", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-credit-card" },
            new ReferenceData { Id = 262, Category = "MODE_PAIEMENT_COMMERCIAL", Code = "Carte", Label = "Carte bancaire", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-credit-card-2-front" },
            new ReferenceData { Id = 263, Category = "MODE_PAIEMENT_COMMERCIAL", Code = "Especes", Label = "Espèces", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-cash" },
            new ReferenceData { Id = 264, Category = "MODE_PAIEMENT_COMMERCIAL", Code = "Prelevement", Label = "Prélèvement automatique", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Icone = "bi-arrow-repeat" },

            // ════════════════════════════════════════════════════════════════
            // TYPE DE SERVICE (ServiceContrat.TypeService)
            // ════════════════════════════════════════════════════════════════
            new ReferenceData { Id = 270, Category = "TYPE_SERVICE", Code = "TVA", Label = "Taxe sur la Valeur Ajoutée", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-receipt" },
            new ReferenceData { Id = 271, Category = "TYPE_SERVICE", Code = "IS", Label = "Impôt sur les Sociétés", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-building" },
            new ReferenceData { Id = 272, Category = "TYPE_SERVICE", Code = "IR", Label = "Impôt sur le Revenu", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-person" },
            new ReferenceData { Id = 273, Category = "TYPE_SERVICE", Code = "CNSS", Label = "Caisse Nationale de Sécurité Sociale", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info", Icone = "bi-shield" },
            new ReferenceData { Id = 274, Category = "TYPE_SERVICE", Code = "PAIE", Label = "Gestion de la Paie", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#d97706", CssClass = "bg-warning-subtle text-warning", Icone = "bi-cash-stack" },
            new ReferenceData { Id = 275, Category = "TYPE_SERVICE", Code = "JURIDIQUE", Label = "Dossiers Juridiques", SortOrder = 6, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-briefcase" },
            new ReferenceData { Id = 276, Category = "TYPE_SERVICE", Code = "COMPTABILITE", Label = "Tenue Comptable", SortOrder = 7, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#14b8a6", CssClass = "bg-teal-subtle text-teal", Icone = "bi-journal-text" },
            new ReferenceData { Id = 277, Category = "TYPE_SERVICE", Code = "BILAN", Label = "Bilan & États de Synthèse", SortOrder = 8, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#6366f1", CssClass = "bg-indigo-subtle text-indigo", Icone = "bi-bar-chart-line" },
            new ReferenceData { Id = 278, Category = "TYPE_SERVICE", Code = "ETATSFISCAUX", Label = "États Fiscaux", SortOrder = 9, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#ec4899", CssClass = "bg-pink-subtle text-pink", Icone = "bi-file-earmark-spreadsheet" },
            new ReferenceData { Id = 279, Category = "TYPE_SERVICE", Code = "AUDITCOMPTABLE", Label = "Audit Comptable", SortOrder = 10, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-search" },
            new ReferenceData { Id = 280, Category = "TYPE_SERVICE", Code = "CONSEILFISCAL", Label = "Conseil Fiscal", SortOrder = 11, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-lightbulb" },

            // ════════════════════════════════════════════════════════════════
            // CATEGORIE DE SERVICE
            // ════════════════════════════════════════════════════════════════
            new ReferenceData { Id = 285, Category = "CATEGORIE_SERVICE", Code = "Fiscal", Label = "Fiscal", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-calculator" },
            new ReferenceData { Id = 286, Category = "CATEGORIE_SERVICE", Code = "Social", Label = "Social", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#0891b2", CssClass = "bg-info-subtle text-info", Icone = "bi-people" },
            new ReferenceData { Id = 287, Category = "CATEGORIE_SERVICE", Code = "Comptable", Label = "Comptable", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-journal-text" },
            new ReferenceData { Id = 288, Category = "CATEGORIE_SERVICE", Code = "Juridique", Label = "Juridique", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#dc2626", CssClass = "bg-danger-subtle text-danger", Icone = "bi-briefcase" },
            new ReferenceData { Id = 289, Category = "CATEGORIE_SERVICE", Code = "Conseil", Label = "Conseil", SortOrder = 5, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-lightbulb" },
            new ReferenceData { Id = 290, Category = "CATEGORIE_SERVICE", Code = "Autre", Label = "Autre", SortOrder = 99, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#64748b", CssClass = "bg-secondary-subtle text-secondary", Icone = "bi-three-dots" },

            // ════════════════════════════════════════════════════════════════
            // FREQUENCE
            // ════════════════════════════════════════════════════════════════
            new ReferenceData { Id = 295, Category = "FREQUENCE", Code = "Mensuelle", Label = "Mensuelle", SortOrder = 1, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#2563eb", CssClass = "bg-primary-subtle text-primary", Icone = "bi-calendar-month" },
            new ReferenceData { Id = 296, Category = "FREQUENCE", Code = "Trimestrielle", Label = "Trimestrielle", SortOrder = 2, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#7c3aed", CssClass = "bg-purple-subtle text-purple", Icone = "bi-calendar3" },
            new ReferenceData { Id = 297, Category = "FREQUENCE", Code = "Annuelle", Label = "Annuelle", SortOrder = 3, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#059669", CssClass = "bg-success-subtle text-success", Icone = "bi-calendar-event" },
            new ReferenceData { Id = 298, Category = "FREQUENCE", Code = "Ponctuelle", Label = "Ponctuelle", SortOrder = 4, IsActive = true, EffectiveFrom = new DateTime(2026, 1, 1), Couleur = "#f59e0b", CssClass = "bg-warning-subtle text-warning", Icone = "bi-calendar-check" }
        );
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using MetaCompta.Application.Interfaces;
using MetaCompta.Application.Mappings;
using MetaCompta.Application.Services;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== Database =====
builder.Services.AddDbContext<MetaComptaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("MetaCompta.API"))
    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ===== Identity =====
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<MetaComptaDbContext>()
.AddDefaultTokenProviders();

// ===== JWT Authentication =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// ===== Authorization Policies =====
builder.Services.AddAuthorization(options =>
{
    // Single source of truth: lowercase domain.action claim policies from ClaimDefinitions
    // Note: ASP.NET Core's PolicyMap uses StringComparer.OrdinalIgnoreCase,
    // so we MUST use only one naming convention to avoid silent overwrites.
    foreach (var claim in ClaimDefinitions.All)
    {
        options.AddPolicy(claim, policy => policy.RequireClaim("Permission", claim));
    }

    options.AddPolicy("BackOfficeAccess", policy => policy.RequireRole("SuperAdmin", "Manager", "Assistant", "Comptable", "Fiscaliste"));
    options.AddPolicy("ExtranetAccess", policy => policy.RequireRole("Client"));
    options.AddPolicy("AdminAccess", policy => policy.RequireRole("PlatformAdmin"));
});

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===== Services (DI) =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDeclarationService, DeclarationService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICabinetService, CabinetService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<ICommercialService, CommercialService>();
builder.Services.AddScoped<IWorkflowSchedulerService, WorkflowSchedulerService>();

// ===== Hangfire (Background Jobs) =====
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        SchemaName = "HangFire"
    }));
builder.Services.AddHangfireServer();

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ===== Controllers =====
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MetaCompta API",
        Version = "v1",
        Description = "API for MetaCompta - Moroccan Fiduciary Cabinet Management System"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
// Swagger always enabled (needed for Docker healthcheck and production API docs)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaCompta API v1"));

// Only redirect to HTTPS outside Docker (containers use HTTP internally)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ===== Hangfire Dashboard =====
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new Hangfire.Dashboard.IDashboardAuthorizationFilter[] { new HangfireAllowAllFilter() }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

// ===== Auto-migrate & seed on startup =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MetaComptaDbContext>();
    await db.Database.MigrateAsync();

    // ===== Patch: Add missing ReferenceData columns (Couleur, CssClass, Icone) =====
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReferenceData') AND name = 'Couleur')
        BEGIN
            ALTER TABLE [ReferenceData] ADD [Couleur] nvarchar(max) NULL;
            ALTER TABLE [ReferenceData] ADD [CssClass] nvarchar(max) NULL;
            ALTER TABLE [ReferenceData] ADD [Icone] nvarchar(max) NULL;
        END
    ");

    // ===== Patch: Update existing seed data with Couleur/CssClass + insert new reference data =====
    await db.Database.ExecuteSqlRawAsync(@"
        -- Update DOC_CATEGORY (22-27)
        UPDATE [ReferenceData] SET [Couleur]='#2563eb',[CssClass]='bg-primary-subtle text-primary' WHERE Id=22 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#7c3aed',[CssClass]='bg-purple-subtle text-purple' WHERE Id=23 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#059669',[CssClass]='bg-success-subtle text-success' WHERE Id=24 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#0891b2',[CssClass]='bg-info-subtle text-info' WHERE Id=25 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#d97706',[CssClass]='bg-warning-subtle text-warning' WHERE Id=26 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#64748b',[CssClass]='bg-secondary-subtle text-secondary' WHERE Id=27 AND [Couleur] IS NULL;

        -- Update FORME_JURIDIQUE (28-31)
        UPDATE [ReferenceData] SET [Couleur]='#2563eb',[CssClass]='bg-primary-subtle text-primary' WHERE Id=28 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#7c3aed',[CssClass]='bg-purple-subtle text-purple' WHERE Id=29 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#059669',[CssClass]='bg-success-subtle text-success' WHERE Id=30 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Code]='SCS',[Label]='SCS',[Couleur]='#0891b2',[CssClass]='bg-info-subtle text-info' WHERE Id=31 AND [Couleur] IS NULL;

        -- Update TYPE_CONTRAT (32-35)
        UPDATE [ReferenceData] SET [Couleur]='#059669',[CssClass]='bg-success-subtle text-success' WHERE Id=32 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#f59e0b',[CssClass]='bg-warning-subtle text-warning' WHERE Id=33 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#0891b2',[CssClass]='bg-info-subtle text-info' WHERE Id=34 AND [Couleur] IS NULL;
        UPDATE [ReferenceData] SET [Couleur]='#7c3aed',[CssClass]='bg-purple-subtle text-purple' WHERE Id=35 AND [Couleur] IS NULL;

        -- Insert new rows only if they don't exist
        SET IDENTITY_INSERT [ReferenceData] ON;

        -- FORME_JURIDIQUE additions (36-41)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=36)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(36,'FORME_JURIDIQUE','SCA','SCA',5,1,0,'2026-01-01',GETUTCDATE(),'#d97706','bg-warning-subtle text-warning');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=37)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(37,'FORME_JURIDIQUE','AUTO_ENTREPRENEUR','Auto-Entrepreneur',6,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=38)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(38,'FORME_JURIDIQUE','PERSONNE_PHYSIQUE','Personne Physique',7,1,0,'2026-01-01',GETUTCDATE(),'#64748b','bg-secondary-subtle text-secondary');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=39)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(39,'FORME_JURIDIQUE','ASSOCIATION','Association',8,1,0,'2026-01-01',GETUTCDATE(),'#ec4899','bg-pink-subtle text-pink');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=40)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(40,'FORME_JURIDIQUE','COOPERATIVE',N'Coopérative',9,1,0,'2026-01-01',GETUTCDATE(),'#14b8a6','bg-teal-subtle text-teal');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=41)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(41,'FORME_JURIDIQUE','EI','Entreprise Individuelle',10,1,0,'2026-01-01',GETUTCDATE(),'#6366f1','bg-indigo-subtle text-indigo');

        -- STATUT_CLIENT (42-45)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=42)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(42,'STATUT_CLIENT','Active','Actif',1,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=43)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(43,'STATUT_CLIENT','Inactive','Inactif',2,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','bg-danger-subtle text-danger','bi-x-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=44)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(44,'STATUT_CLIENT','Dormante','Dormant',3,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning','bi-pause-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=45)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(45,'STATUT_CLIENT','Dissoute','Dissout',4,1,0,'2026-01-01',GETUTCDATE(),'#64748b','bg-secondary-subtle text-secondary','bi-slash-circle');

        -- STATUT_EMPLOYE (46-48)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=46)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(46,'STATUT_EMPLOYE','Actif','Actif',1,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=47)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(47,'STATUT_EMPLOYE','Inactif','Inactif',2,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','bg-danger-subtle text-danger','bi-x-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=48)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(48,'STATUT_EMPLOYE','Suspendu','Suspendu',3,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning','bi-pause-circle');

        -- TYPE_DECLARATION (49-53)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=49)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(49,'TYPE_DECLARATION','TVA','TVA',1,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','bg-primary-subtle text-primary','bi-receipt');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=50)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(50,'TYPE_DECLARATION','IS',N'Impôt sur les Sociétés',2,1,0,'2026-01-01',GETUTCDATE(),'#7c3aed','bg-purple-subtle text-purple','bi-building');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=51)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(51,'TYPE_DECLARATION','IR',N'Impôt sur le Revenu',3,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-person');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=52)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(52,'TYPE_DECLARATION','CNSS','CNSS',4,1,0,'2026-01-01',GETUTCDATE(),'#0891b2','bg-info-subtle text-info','bi-shield');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=53)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(53,'TYPE_DECLARATION','IR_SALAIRES','IR Salaires',5,1,0,'2026-01-01',GETUTCDATE(),'#d97706','bg-warning-subtle text-warning','bi-cash');

        -- STATUT_DECLARATION (54-60)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=54)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(54,'STATUT_DECLARATION','Brouillon','Brouillon',1,1,0,'2026-01-01',GETUTCDATE(),'#94a3b8','badge-brouillon','bi-pencil');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=55)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(55,'STATUT_DECLARATION','ACompleter',N'À Compléter',2,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','badge-acompleter','bi-exclamation-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=56)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(56,'STATUT_DECLARATION','Completee',N'Complétée',3,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','badge-completee','bi-check');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=57)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(57,'STATUT_DECLARATION','EnRevision',N'En Révision',4,1,0,'2026-01-01',GETUTCDATE(),'#7c3aed','badge-enrevision','bi-eye');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=58)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(58,'STATUT_DECLARATION','Validee',N'Validée',5,1,0,'2026-01-01',GETUTCDATE(),'#059669','badge-validee','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=59)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(59,'STATUT_DECLARATION','Soumise','Soumise',6,1,0,'2026-01-01',GETUTCDATE(),'#166534','badge-soumise','bi-send');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=60)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(60,'STATUT_DECLARATION','Refusee',N'Refusée',7,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','badge-refusee','bi-x-circle');

        -- TYPE_TVA (61-63)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=61)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(61,'TYPE_TVA','Mensuelle','Mensuelle',1,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','bg-primary-subtle text-primary');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=62)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(62,'TYPE_TVA','Trimestrielle','Trimestrielle',2,1,0,'2026-01-01',GETUTCDATE(),'#7c3aed','bg-purple-subtle text-purple');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=63)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(63,'TYPE_TVA','Exonere',N'Exonéré',3,1,0,'2026-01-01',GETUTCDATE(),'#64748b','bg-secondary-subtle text-secondary');

        -- VILLES (64-83)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=64)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(64,'VILLE','CASABLANCA','Casablanca',1,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=65)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(65,'VILLE','RABAT','Rabat',2,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=66)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(66,'VILLE','MARRAKECH','Marrakech',3,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=67)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(67,'VILLE','FES',N'Fès',4,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=68)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(68,'VILLE','TANGER','Tanger',5,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=69)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(69,'VILLE','AGADIR','Agadir',6,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=70)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(70,'VILLE','MEKNES',N'Meknès',7,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=71)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(71,'VILLE','OUJDA','Oujda',8,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=72)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(72,'VILLE','KENITRA',N'Kénitra',9,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=73)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(73,'VILLE','TETOUAN',N'Tétouan',10,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=74)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(74,'VILLE','SALE',N'Salé',11,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=75)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(75,'VILLE','NADOR','Nador',12,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=76)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(76,'VILLE','MOHAMMEDIA',N'Mohammédia',13,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=77)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(77,'VILLE','EL_JADIDA','El Jadida',14,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=78)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(78,'VILLE','BENI_MELLAL',N'Béni Mellal',15,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=79)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(79,'VILLE','KHOURIBGA','Khouribga',16,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=80)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(80,'VILLE','SETTAT','Settat',17,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=81)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(81,'VILLE','SAFI','Safi',18,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=82)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(82,'VILLE','LAAYOUNE',N'Laâyoune',19,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=83)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(83,'VILLE','DAKHLA','Dakhla',20,1,0,'2026-01-01',GETUTCDATE());

        -- SITUATION_FAMILIALE (84-87)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=84)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(84,'SITUATION_FAMILIALE','Celibataire',N'Célibataire',1,1,0,'2026-01-01',GETUTCDATE(),'bi-person');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=85)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(85,'SITUATION_FAMILIALE','Marie',N'Marié(e)',2,1,0,'2026-01-01',GETUTCDATE(),'bi-people');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=86)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(86,'SITUATION_FAMILIALE','Divorce',N'Divorcé(e)',3,1,0,'2026-01-01',GETUTCDATE(),'bi-person-dash');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=87)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(87,'SITUATION_FAMILIALE','Veuf','Veuf(ve)',4,1,0,'2026-01-01',GETUTCDATE(),'bi-person-x');

        -- SEXE (88-89)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=88)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(88,'SEXE','M','Masculin',1,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=89)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(89,'SEXE','F',N'Féminin',2,1,0,'2026-01-01',GETUTCDATE());

        -- MODE_PAIEMENT (90-92)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=90)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(90,'MODE_PAIEMENT','Virement','Virement bancaire',1,1,0,'2026-01-01',GETUTCDATE(),'bi-bank');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=91)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(91,'MODE_PAIEMENT','Cheque',N'Chèque',2,1,0,'2026-01-01',GETUTCDATE(),'bi-credit-card');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=92)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone) VALUES(92,'MODE_PAIEMENT','Especes',N'Espèces',3,1,0,'2026-01-01',GETUTCDATE(),'bi-cash');

        -- TYPE_ABSENCE (93-97)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=93)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(93,'TYPE_ABSENCE','Annuel',N'Congé annuel',1,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','bg-primary-subtle text-primary');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=94)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(94,'TYPE_ABSENCE','Maladie',N'Congé maladie',2,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','bg-danger-subtle text-danger');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=95)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(95,'TYPE_ABSENCE','SansSolde',N'Congé sans solde',3,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=96)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(96,'TYPE_ABSENCE','Maternite',N'Congé maternité',4,1,0,'2026-01-01',GETUTCDATE(),'#ec4899','bg-pink-subtle text-pink');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=97)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass) VALUES(97,'TYPE_ABSENCE','Exceptionnel',N'Congé exceptionnel',5,1,0,'2026-01-01',GETUTCDATE(),'#7c3aed','bg-purple-subtle text-purple');

        -- STATUT_DOCUMENT (98-101)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=98)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(98,'STATUT_DOCUMENT','EnAttente','En attente',1,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning','bi-clock');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=99)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(99,'STATUT_DOCUMENT','Valide',N'Validé',2,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=100)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(100,'STATUT_DOCUMENT','Rejete',N'Rejeté',3,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','bg-danger-subtle text-danger','bi-x-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=101)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(101,'STATUT_DOCUMENT','Archive',N'Archivé',4,1,0,'2026-01-01',GETUTCDATE(),'#64748b','bg-secondary-subtle text-secondary','bi-archive');

        -- PRIORITE_TACHE (102-105)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=102)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(102,'PRIORITE_TACHE','Faible','Faible',1,1,0,'2026-01-01',GETUTCDATE(),'#94a3b8','badge-faible','bi-arrow-down');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=103)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(103,'PRIORITE_TACHE','Moyenne','Moyenne',2,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','badge-moyenne','bi-arrow-right');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=104)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(104,'PRIORITE_TACHE','Haute','Haute',3,1,0,'2026-01-01',GETUTCDATE(),'#ea580c','badge-haute','bi-arrow-up');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=105)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(105,'PRIORITE_TACHE','Urgente','Urgente',4,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','badge-urgente','bi-exclamation-triangle');

        -- STATUT_TACHE (106-110)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=106)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(106,'STATUT_TACHE','AFaire',N'À Faire',1,1,0,'2026-01-01',GETUTCDATE(),'#94a3b8','bg-secondary-subtle text-secondary','bi-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=107)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(107,'STATUT_TACHE','EnCours','En Cours',2,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','bg-primary-subtle text-primary','bi-play-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=108)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(108,'STATUT_TACHE','EnAttenteRevision',N'En Attente Révision',3,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning','bi-eye');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=109)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(109,'STATUT_TACHE','Termine',N'Terminé',4,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=110)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(110,'STATUT_TACHE','Annule',N'Annulé',5,1,0,'2026-01-01',GETUTCDATE(),'#dc2626','bg-danger-subtle text-danger','bi-x-circle');

        -- STATUT_PAIE (111-115)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=111)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(111,'STATUT_PAIE','Brouillon','Brouillon',1,1,0,'2026-01-01',GETUTCDATE(),'#94a3b8','badge-brouillon','bi-pencil');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=112)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(112,'STATUT_PAIE','EnCours','En Cours',2,1,0,'2026-01-01',GETUTCDATE(),'#2563eb','bg-primary-subtle text-primary','bi-play-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=113)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(113,'STATUT_PAIE','EnAttenteValidation','En Attente Validation',3,1,0,'2026-01-01',GETUTCDATE(),'#f59e0b','bg-warning-subtle text-warning','bi-clock');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=114)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(114,'STATUT_PAIE','Validee',N'Validée',4,1,0,'2026-01-01',GETUTCDATE(),'#059669','bg-success-subtle text-success','bi-check-circle');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=115)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Couleur,CssClass,Icone) VALUES(115,'STATUT_PAIE','Cloturee',N'Clôturée',5,1,0,'2026-01-01',GETUTCDATE(),'#64748b','bg-secondary-subtle text-secondary','bi-lock');

        -- SECTEUR_ACTIVITE (116-127)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=116)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(116,'SECTEUR_ACTIVITE','COMMERCE','Commerce',1,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=117)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(117,'SECTEUR_ACTIVITE','SERVICES','Services',2,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=118)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(118,'SECTEUR_ACTIVITE','INDUSTRIE','Industrie',3,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=119)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(119,'SECTEUR_ACTIVITE','BTP','BTP',4,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=120)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(120,'SECTEUR_ACTIVITE','IMMOBILIER','Immobilier',5,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=121)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(121,'SECTEUR_ACTIVITE','TRANSPORT','Transport & Logistique',6,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=122)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(122,'SECTEUR_ACTIVITE','RESTAURATION',N'Hôtellerie & Restauration',7,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=123)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(123,'SECTEUR_ACTIVITE','TECH','Technologies & IT',8,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=124)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(124,'SECTEUR_ACTIVITE','AGRICULTURE','Agriculture',9,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=125)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(125,'SECTEUR_ACTIVITE','SANTE',N'Santé',10,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=126)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(126,'SECTEUR_ACTIVITE','EDUCATION',N'Éducation & Formation',11,1,0,'2026-01-01',GETUTCDATE());
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=127)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt) VALUES(127,'SECTEUR_ACTIVITE','AUTRE','Autre',99,1,0,'2026-01-01',GETUTCDATE());

        -- CATEGORIE_DOCUMENT (128-133)
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=128)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(128,'CATEGORIE_DOCUMENT','Legal',N'Documents Légaux',1,1,0,'2026-01-01',GETUTCDATE(),'bi-file-earmark-text','#2563eb','bg-primary-subtle text-primary');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=129)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(129,'CATEGORIE_DOCUMENT','Fiscal','Documents Fiscaux',2,1,0,'2026-01-01',GETUTCDATE(),'bi-calculator','#7c3aed','bg-purple-subtle text-purple');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=130)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(130,'CATEGORIE_DOCUMENT','Social','Documents Sociaux',3,1,0,'2026-01-01',GETUTCDATE(),'bi-people','#059669','bg-success-subtle text-success');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=131)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(131,'CATEGORIE_DOCUMENT','Comptable','Documents Comptables',4,1,0,'2026-01-01',GETUTCDATE(),'bi-journal-text','#0891b2','bg-info-subtle text-info');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=132)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(132,'CATEGORIE_DOCUMENT','RH','Documents RH',5,1,0,'2026-01-01',GETUTCDATE(),'bi-person-badge','#d97706','bg-warning-subtle text-warning');
        IF NOT EXISTS (SELECT 1 FROM [ReferenceData] WHERE Id=133)
        INSERT INTO [ReferenceData](Id,Category,Code,Label,SortOrder,IsActive,IsDeleted,EffectiveFrom,CreatedAt,Icone,Couleur,CssClass) VALUES(133,'CATEGORIE_DOCUMENT','Admin','Documents Administratifs',6,1,0,'2026-01-01',GETUTCDATE(),'bi-folder','#64748b','bg-secondary-subtle text-secondary');

        SET IDENTITY_INSERT [ReferenceData] OFF;
    ");

    // ===== Patch: Add IsSystem column to WorkflowTemplates =====
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WorkflowTemplates') AND name = 'IsSystem')
            ALTER TABLE [WorkflowTemplates] ADD [IsSystem] bit NOT NULL DEFAULT 0;
    ");
    // Mark all seeded default templates as system (non-modifiable) ? separate batch so column is visible
    await db.Database.ExecuteSqlRawAsync(@"
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WorkflowTemplates') AND name = 'IsSystem')
            UPDATE [WorkflowTemplates] SET [IsSystem] = 1 WHERE [IsDefault] = 1 AND [IsSystem] = 0;
    ");

    // ===== Patch: Seed test declarations for multiple clients (idempotent) =====
    await db.Database.ExecuteSqlRawAsync(@"
        -- Only seed test declarations if we have less than 60 (the original 53 seeded ones)
        IF (SELECT COUNT(*) FROM [Declarations] WHERE CabinetId = 1) < 60
        BEGIN
            DECLARE @now DATETIME2 = GETUTCDATE();
            DECLARE @gerantId NVARCHAR(450) = '11f5aee3-114f-4894-bd4d-64c20ec6f7c6';

            -- Client 2: MAROC DIGITAL SA - TVA June 2026
            INSERT INTO [Declarations](CabinetId, ClientId, Type, Periode, DateDebut, DateFin, DateLimite, Statut, Version, CreatedAt, CreatedBy, IsDeleted)
            VALUES(1, 2, 'TVA', '2026-06', '2026-06-01', '2026-06-30', '2026-07-20', 'Brouillon', 1, @now, @gerantId, 0);

            -- Client 3: COMPTOIR GENERAL - TVA June 2026
            INSERT INTO [Declarations](CabinetId, ClientId, Type, Periode, DateDebut, DateFin, DateLimite, Statut, Version, CreatedAt, CreatedBy, IsDeleted)
            VALUES(1, 3, 'TVA', '2026-06', '2026-06-01', '2026-06-30', '2026-07-20', 'Brouillon', 1, @now, @gerantId, 0);

            -- Client 4: FOOD INVEST - CNSS June 2026
            INSERT INTO [Declarations](CabinetId, ClientId, Type, Periode, DateDebut, DateFin, DateLimite, Statut, Version, CreatedAt, CreatedBy, IsDeleted)
            VALUES(1, 4, 'CNSS', '2026-06', '2026-06-01', '2026-06-30', '2026-07-10', 'Brouillon', 1, @now, @gerantId, 0);

            -- Client 5: SAHARA LOGISTICS - IS 2026
            INSERT INTO [Declarations](CabinetId, ClientId, Type, Periode, DateDebut, DateFin, DateLimite, Statut, Version, CreatedAt, CreatedBy, IsDeleted)
            VALUES(1, 5, 'IS', '2026', '2026-01-01', '2026-12-31', '2027-03-31', 'Brouillon', 1, @now, @gerantId, 0);

            -- Client 6: ZENITH CONSTRUCTION - IR June 2026
            INSERT INTO [Declarations](CabinetId, ClientId, Type, Periode, DateDebut, DateFin, DateLimite, Statut, Version, CreatedAt, CreatedBy, IsDeleted)
            VALUES(1, 6, 'IR', '2026-06', '2026-06-01', '2026-06-30', '2026-07-31', 'Brouillon', 1, @now, @gerantId, 0);

            PRINT '[Startup] Seeded 5 test declarations for clients 2-6.';
        END
    ");

    // Seed default roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    string[] roles = { "PlatformAdmin", "SuperAdmin", "Manager", "Assistant", "Client" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role, IsSystemRole = true, Description = $"Rôle système: {role}" });
        }
    }

    // Seed platform admin user
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "admin@metacompta.ma";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "MetaCompta",
            EmailConfirmed = true,
            IsActive = true,
            PreferredLanguage = "fr-FR"
        };
        await userManager.CreateAsync(admin, "Admin@2026!");
        await userManager.AddToRoleAsync(admin, "PlatformAdmin");
    }

    // Seed demo cabinet + SuperAdmin
    if (!await db.Cabinets.AnyAsync())
    {
        var cabinet = new Cabinet
        {
            Name = "Cabinet Fiduciaire Demo",
            Email = "demo@cabinet-demo.ma",
            ICE = "001234567890123",
            IsActive = true,
            SubscriptionPlanId = 2
        };
        db.Cabinets.Add(cabinet);
        await db.SaveChangesAsync();

        var cabinetAdmin = new ApplicationUser
        {
            UserName = "gerant@cabinet-demo.ma",
            Email = "gerant@cabinet-demo.ma",
            FirstName = "Mohammed",
            LastName = "El Amrani",
            EmailConfirmed = true,
            IsActive = true,
            CabinetId = cabinet.Id,
            PreferredLanguage = "fr-FR"
        };
        await userManager.CreateAsync(cabinetAdmin, "Gerant@2026!");
        await userManager.AddToRoleAsync(cabinetAdmin, "SuperAdmin");
        await db.SaveChangesAsync();
    }

    // ===== Seed demo clients (idempotent — only if no clients exist for demo cabinet) =====
    {
        var demoCabinet = await db.Cabinets.FirstOrDefaultAsync(c => c.Email == "demo@cabinet-demo.ma");
        if (demoCabinet != null && !await db.Clients.AnyAsync(c => c.CabinetId == demoCabinet.Id))
        {
            var gerant = await db.Users.FirstOrDefaultAsync(u => u.Email == "gerant@cabinet-demo.ma");
            var gerantId = gerant?.Id;

            var clients = new List<Client>
            {
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "ATLAS TEXTILE SARL", ICE = "001567890123456", IF = "10234567", RC = "RC-CASA-123456", NumeroCNSS = "1234567", NumeroPatente = "35678901", FormeJuridique = "SARL", SecteurActivite = "Textile & Confection", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "123, Zone Industrielle Ain Sebaa", TelephonePrincipal = "0522345678", EmailPrincipal = "contact@atlastextile.ma", ContactNom = "Karim Benjelloun", ContactFonction = "Gérant", ContactTelephone = "0661234567", ContactEmail = "karim@atlastextile.ma", CapitalSocial = 500000, DateCreation = new DateTime(2015, 3, 10), DateDebutActivite = new DateTime(2015, 4, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "MAROC DIGITAL SA", ICE = "002345678901234", IF = "20345678", RC = "RC-CASA-234567", NumeroCNSS = "2345678", NumeroPatente = "45678902", FormeJuridique = "SA", SecteurActivite = "Technologies de l'Information", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "45, Bd Zerktouni, Maârif", TelephonePrincipal = "0522456789", EmailPrincipal = "info@marocdigital.ma", ContactNom = "Sara Idrissi", ContactFonction = "Directrice Générale", ContactTelephone = "0662345678", ContactEmail = "sara@marocdigital.ma", CapitalSocial = 3000000, DateCreation = new DateTime(2018, 7, 15), DateDebutActivite = new DateTime(2018, 8, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "COMPTOIR GENERAL DU BATIMENT", ICE = "003456789012345", IF = "30456789", RC = "RC-RABAT-345678", NumeroCNSS = "3456789", NumeroPatente = "56789012", FormeJuridique = "SARL", SecteurActivite = "BTP & Construction", VilleSiegeSocial = "Rabat", AdresseSiegeSocial = "Zone Industrielle, Temara", TelephonePrincipal = "0537567890", EmailPrincipal = "contact@cgb.ma", ContactNom = "Ahmed Berrada", ContactFonction = "Directeur Technique", ContactTelephone = "0663456789", ContactEmail = "ahmed@cgb.ma", CapitalSocial = 2000000, DateCreation = new DateTime(2010, 1, 20), DateDebutActivite = new DateTime(2010, 3, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "FOOD INVEST SARL AU", ICE = "004567890123456", IF = "40567890", RC = "RC-TANGER-456789", NumeroCNSS = "4567890", NumeroPatente = "67890123", FormeJuridique = "SARL AU", SecteurActivite = "Agroalimentaire", VilleSiegeSocial = "Tanger", AdresseSiegeSocial = "Zone Franche, Tanger Med", TelephonePrincipal = "0539678901", EmailPrincipal = "info@foodinvest.ma", ContactNom = "Fatima Zahra El Fassi", ContactFonction = "Gérante", ContactTelephone = "0664567890", ContactEmail = "fatima@foodinvest.ma", CapitalSocial = 100000, DateCreation = new DateTime(2020, 6, 5), DateDebutActivite = new DateTime(2020, 7, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "SAHARA LOGISTICS SA", ICE = "005678901234567", IF = "50678901", RC = "RC-AGADIR-567890", NumeroCNSS = "5678901", NumeroPatente = "78901234", FormeJuridique = "SA", SecteurActivite = "Transport & Logistique", VilleSiegeSocial = "Agadir", AdresseSiegeSocial = "Zone Logistique, Ait Melloul", TelephonePrincipal = "0528789012", EmailPrincipal = "contact@saharalogistics.ma", ContactNom = "Omar Tazi", ContactFonction = "PDG", ContactTelephone = "0665678901", ContactEmail = "omar@saharalogistics.ma", CapitalSocial = 5000000, DateCreation = new DateTime(2012, 11, 8), DateDebutActivite = new DateTime(2013, 1, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "MOULAY YOUSSEF IMMOBILIER", ICE = "006789012345678", IF = "60789012", RC = "RC-MARRAKECH-678901", NumeroCNSS = "6789012", NumeroPatente = "89012345", FormeJuridique = "SARL", SecteurActivite = "Immobilier & Promotion", VilleSiegeSocial = "Marrakech", AdresseSiegeSocial = "Guéliz, Av. Mohammed V", TelephonePrincipal = "0524890123", EmailPrincipal = "contact@myimmobilier.ma", ContactNom = "Hassan Alaoui", ContactFonction = "Gérant", ContactTelephone = "0666789012", ContactEmail = "hassan@myimmobilier.ma", CapitalSocial = 10000000, DateCreation = new DateTime(2008, 4, 12), DateDebutActivite = new DateTime(2008, 5, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "ORIENTAL PHARMA SNC", ICE = "007890123456789", IF = "70890123", RC = "RC-OUJDA-789012", NumeroCNSS = "7890123", NumeroPatente = "90123456", FormeJuridique = "SNC", SecteurActivite = "Pharmacie & Parapharmacie", VilleSiegeSocial = "Oujda", AdresseSiegeSocial = "16, Bd Allal Ben Abdellah", TelephonePrincipal = "0536901234", EmailPrincipal = "contact@orientalpharma.ma", ContactNom = "Leila Bennani", ContactFonction = "Associée Gérante", ContactTelephone = "0667890123", ContactEmail = "leila@orientalpharma.ma", CapitalSocial = 800000, DateCreation = new DateTime(2016, 9, 25), DateDebutActivite = new DateTime(2016, 10, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "GREEN ENERGY MAROC SA", ICE = "008901234567890", IF = "80901234", RC = "RC-FES-890123", NumeroCNSS = "8901234", NumeroPatente = "01234567", FormeJuridique = "SA", SecteurActivite = "Energies Renouvelables", VilleSiegeSocial = "Fès", AdresseSiegeSocial = "Technopolis, Route de Meknès", TelephonePrincipal = "0535012345", EmailPrincipal = "info@greenenergy.ma", ContactNom = "Youssef Amrani", ContactFonction = "Directeur Général", ContactTelephone = "0668901234", ContactEmail = "youssef@greenenergy.ma", CapitalSocial = 15000000, DateCreation = new DateTime(2019, 2, 14), DateDebutActivite = new DateTime(2019, 4, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "BOUTIQUE LUXE MARRAKECH", ICE = "009012345678901", IF = "90012345", RC = "RC-MARRAKECH-901234", NumeroCNSS = "9012345", NumeroPatente = "12345678", FormeJuridique = "SARL", SecteurActivite = "Commerce de Détail", VilleSiegeSocial = "Marrakech", AdresseSiegeSocial = "Av. Mohammed VI, Hivernage", TelephonePrincipal = "0524123456", EmailPrincipal = "contact@boutiqueluxe.ma", ContactNom = "Nadia Chraibi", ContactFonction = "Directrice", ContactTelephone = "0669012345", ContactEmail = "nadia@boutiqueluxe.ma", CapitalSocial = 300000, DateCreation = new DateTime(2021, 5, 3), DateDebutActivite = new DateTime(2021, 6, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "AUTO PIECES DU SUD SARL", ICE = "010123456789012", IF = "10123456", RC = "RC-AGADIR-012345", NumeroCNSS = "0123456", NumeroPatente = "23456789", FormeJuridique = "SARL", SecteurActivite = "Automobile & Pièces Détachées", VilleSiegeSocial = "Agadir", AdresseSiegeSocial = "Zone Industrielle Tassila", TelephonePrincipal = "0528234567", EmailPrincipal = "contact@autopiecesdusud.ma", ContactNom = "Rachid El Mansouri", ContactFonction = "Gérant", ContactTelephone = "0660123456", ContactEmail = "rachid@autopiecesdusud.ma", CapitalSocial = 400000, DateCreation = new DateTime(2014, 8, 20), DateDebutActivite = new DateTime(2014, 9, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "IMPRIMERIE MODERNE DE RABAT", ICE = "011234567890123", IF = "11234567", RC = "RC-RABAT-112345", NumeroCNSS = "1123456", NumeroPatente = "34567890", FormeJuridique = "SARL", SecteurActivite = "Imprimerie & Édition", VilleSiegeSocial = "Rabat", AdresseSiegeSocial = "Quartier Industriel, Youssoufia", TelephonePrincipal = "0537345678", EmailPrincipal = "contact@imprimeriemoderne.ma", ContactNom = "Abdelkader Filali", ContactFonction = "Directeur", ContactTelephone = "0661123456", ContactEmail = "abdelkader@imprimeriemoderne.ma", CapitalSocial = 600000, DateCreation = new DateTime(2005, 12, 1), DateDebutActivite = new DateTime(2006, 1, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "HOTEL RIAD PALAIS SAS", ICE = "012345678901234", IF = "12345678", RC = "RC-FES-123456", NumeroCNSS = "2234567", NumeroPatente = "45678901", FormeJuridique = "SAS", SecteurActivite = "Hôtellerie & Tourisme", VilleSiegeSocial = "Fès", AdresseSiegeSocial = "Médina, Derb Zouak", TelephonePrincipal = "0535456789", EmailPrincipal = "reservation@riadpalais.ma", ContactNom = "Amina Kettani", ContactFonction = "Directrice Générale", ContactTelephone = "0662234567", ContactEmail = "amina@riadpalais.ma", CapitalSocial = 8000000, DateCreation = new DateTime(2011, 6, 15), DateDebutActivite = new DateTime(2011, 9, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "CLINIQUE DENTAIRE AL AMAL", ICE = "013456789012345", IF = "13456789", RC = "RC-MEKNES-134567", NumeroCNSS = "3345678", NumeroPatente = "56789012", FormeJuridique = "SARL", SecteurActivite = "Santé & Médical", VilleSiegeSocial = "Meknès", AdresseSiegeSocial = "Av. des FAR, Hamria", TelephonePrincipal = "0535567890", EmailPrincipal = "contact@cliniquealmal.ma", ContactNom = "Dr. Mourad Sqalli", ContactFonction = "Directeur Médical", ContactTelephone = "0663345678", ContactEmail = "mourad@cliniquealmal.ma", CapitalSocial = 1500000, DateCreation = new DateTime(2017, 3, 22), DateDebutActivite = new DateTime(2017, 5, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "CONSERVERIE DU NORD SARL", ICE = "014567890123456", IF = "14567890", RC = "RC-TANGER-145678", NumeroCNSS = "4456789", NumeroPatente = "67890123", FormeJuridique = "SARL", SecteurActivite = "Agroalimentaire", VilleSiegeSocial = "Tanger", AdresseSiegeSocial = "Zone Industrielle Gzenaya", TelephonePrincipal = "0539678901", EmailPrincipal = "contact@conserveriedunord.ma", ContactNom = "Abdelhak Riffi", ContactFonction = "Gérant", ContactTelephone = "0664456789", ContactEmail = "abdelhak@conserveriedunord.ma", CapitalSocial = 1200000, DateCreation = new DateTime(2009, 7, 10), DateDebutActivite = new DateTime(2009, 9, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "ECOLE PRIVEE LES PALMIERS", ICE = "015678901234567", IF = "15678901", RC = "RC-CASA-156789", NumeroCNSS = "5567890", NumeroPatente = "78901234", FormeJuridique = "SARL", SecteurActivite = "Enseignement & Formation", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "Hay Hassani, Ain Chock", TelephonePrincipal = "0522789012", EmailPrincipal = "direction@lespalmiers.ma", ContactNom = "Khadija Ouazzani", ContactFonction = "Directrice Pédagogique", ContactTelephone = "0665567890", ContactEmail = "khadija@lespalmiers.ma", CapitalSocial = 2500000, DateCreation = new DateTime(2007, 8, 28), DateDebutActivite = new DateTime(2007, 9, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "CABINET D'AVOCATS BENNANI & ASSOCIES", ICE = "016789012345678", IF = "16789012", RC = "RC-RABAT-167890", NumeroCNSS = "6678901", NumeroPatente = "89012345", FormeJuridique = "SNC", SecteurActivite = "Services Juridiques", VilleSiegeSocial = "Rabat", AdresseSiegeSocial = "5, Rue Abou Inane, Agdal", TelephonePrincipal = "0537890123", EmailPrincipal = "contact@bennanilaw.ma", ContactNom = "Maître Yassine Bennani", ContactFonction = "Avocat Associé", ContactTelephone = "0666678901", ContactEmail = "yassine@bennanilaw.ma", CapitalSocial = 200000, DateCreation = new DateTime(2013, 10, 5), DateDebutActivite = new DateTime(2013, 11, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "BRASSERIE ARTISANALE DU RIF", ICE = "017890123456789", IF = "17890123", RC = "RC-CHEFCHAOUEN-178901", NumeroCNSS = "7789012", NumeroPatente = "90123456", FormeJuridique = "SARL", SecteurActivite = "Restauration & Boissons", VilleSiegeSocial = "Chefchaouen", AdresseSiegeSocial = "Route de Fès, Km 3", TelephonePrincipal = "0539901234", EmailPrincipal = "contact@brasserierif.ma", ContactNom = "Mehdi Lahrichi", ContactFonction = "Fondateur", ContactTelephone = "0667789012", ContactEmail = "mehdi@brasserierif.ma", CapitalSocial = 150000, DateCreation = new DateTime(2022, 1, 18), DateDebutActivite = new DateTime(2022, 3, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "TRANSPORT FRIGORIFIQUE SOUSS", ICE = "018901234567890", IF = "18901234", RC = "RC-AGADIR-189012", NumeroCNSS = "8890123", NumeroPatente = "01234568", FormeJuridique = "SARL", SecteurActivite = "Transport & Logistique", VilleSiegeSocial = "Agadir", AdresseSiegeSocial = "Route de Marrakech, Km 5", TelephonePrincipal = "0528012345", EmailPrincipal = "contact@tfsouss.ma", ContactNom = "Brahim Ait Baha", ContactFonction = "Directeur d'Exploitation", ContactTelephone = "0668890123", ContactEmail = "brahim@tfsouss.ma", CapitalSocial = 3500000, DateCreation = new DateTime(2006, 5, 12), DateDebutActivite = new DateTime(2006, 7, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "SOCIETE DE GARDIENNAGE ATLAS SECURITY", ICE = "019012345678901", IF = "19012345", RC = "RC-CASA-190123", NumeroCNSS = "9901234", NumeroPatente = "12345679", FormeJuridique = "SA", SecteurActivite = "Sécurité & Gardiennage", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "Sidi Maarouf, Nearshore Park", TelephonePrincipal = "0522123456", EmailPrincipal = "contact@atlassecurity.ma", ContactNom = "Colonel Driss Lahlou", ContactFonction = "Directeur Général", ContactTelephone = "0669901234", ContactEmail = "driss@atlassecurity.ma", CapitalSocial = 7000000, DateCreation = new DateTime(2004, 2, 28), DateDebutActivite = new DateTime(2004, 4, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "MENUISERIE BOIS & ALU SARL", ICE = "020123456789012", IF = "20123456", RC = "RC-KENITRA-201234", NumeroCNSS = "0012345", NumeroPatente = "23456790", FormeJuridique = "SARL", SecteurActivite = "Menuiserie & Agencement", VilleSiegeSocial = "Kénitra", AdresseSiegeSocial = "Zone Industrielle, Route de Mehdya", TelephonePrincipal = "0537234567", EmailPrincipal = "contact@menuiserieboisalu.ma", ContactNom = "Mustapha El Ghazi", ContactFonction = "Gérant", ContactTelephone = "0660012345", ContactEmail = "mustapha@menuiserieboisalu.ma", CapitalSocial = 250000, DateCreation = new DateTime(2019, 11, 7), DateDebutActivite = new DateTime(2020, 1, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "CENTRE D'APPELS CONNECT PLUS", ICE = "021234567890123", IF = "21234567", RC = "RC-CASA-212345", NumeroCNSS = "1012345", NumeroPatente = "34567891", FormeJuridique = "SA", SecteurActivite = "Centres d'Appels & BPO", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "CasaNearshore, Shore 1", TelephonePrincipal = "0522345679", EmailPrincipal = "contact@connectplus.ma", ContactNom = "Zineb Mernissi", ContactFonction = "Directrice Opérationnelle", ContactTelephone = "0661012345", ContactEmail = "zineb@connectplus.ma", CapitalSocial = 4000000, DateCreation = new DateTime(2016, 4, 30), DateDebutActivite = new DateTime(2016, 6, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "COOPERATIVE AGRICOLE SOUSS MASSA", ICE = "022345678901234", IF = "22345678", RC = "RC-TAROUDANT-223456", NumeroCNSS = "2012345", NumeroPatente = "45678902", FormeJuridique = "Coopérative", SecteurActivite = "Agriculture & Export", VilleSiegeSocial = "Taroudant", AdresseSiegeSocial = "Route Nationale N10, Km 8", TelephonePrincipal = "0528345678", EmailPrincipal = "contact@coopsoussmassa.ma", ContactNom = "Mohamed Outaleb", ContactFonction = "Président", ContactTelephone = "0662012345", ContactEmail = "mohamed@coopsoussmassa.ma", CapitalSocial = 900000, DateCreation = new DateTime(2002, 3, 15), DateDebutActivite = new DateTime(2002, 4, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 6, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "STARTUP FINTECH CASAPAY", ICE = "023456789012345", IF = "23456789", RC = "RC-CASA-235678", NumeroCNSS = "3012345", NumeroPatente = "56789013", FormeJuridique = "SAS", SecteurActivite = "Fintech & Paiement Digital", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "Technopark, Route de Nouaceur", TelephonePrincipal = "0522456790", EmailPrincipal = "info@casapay.ma", ContactNom = "Othmane Skalli", ContactFonction = "CEO & Co-fondateur", ContactTelephone = "0663012345", ContactEmail = "othmane@casapay.ma", CapitalSocial = 1000000, DateCreation = new DateTime(2023, 9, 1), DateDebutActivite = new DateTime(2023, 10, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Active", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "SOCIETE DORMANTE EXEMPLE", ICE = "024567890123456", IF = "24567890", RC = "RC-CASA-245678", NumeroCNSS = "4012345", NumeroPatente = "67890124", FormeJuridique = "SARL", SecteurActivite = "Commerce Général", VilleSiegeSocial = "Casablanca", AdresseSiegeSocial = "Bd d'Anfa", TelephonePrincipal = "0522567890", EmailPrincipal = "contact@dormante.ma", ContactNom = "Adil Hajji", ContactFonction = "Gérant", ContactTelephone = "0664012345", ContactEmail = "adil@dormante.ma", CapitalSocial = 100000, DateCreation = new DateTime(2015, 6, 20), DateDebutActivite = new DateTime(2015, 7, 1), TypeTVA = "Trimestrielle", MoisClotureExercice = 12, Statut = "Dormante", ManagerPrincipalId = gerantId },
                new Client { CabinetId = demoCabinet.Id, RaisonSociale = "SOCIETE DISSOUTE OMEGA", ICE = "025678901234567", IF = "25678901", RC = "RC-RABAT-256789", NumeroCNSS = "5012345", NumeroPatente = "78901235", FormeJuridique = "SA", SecteurActivite = "Import-Export", VilleSiegeSocial = "Rabat", AdresseSiegeSocial = "Av. Hassan II", TelephonePrincipal = "0537678901", EmailPrincipal = "contact@omega.ma", ContactNom = "Hamid Chaouki", ContactFonction = "Ex-PDG", ContactTelephone = "0665012345", ContactEmail = "hamid@omega.ma", CapitalSocial = 500000, DateCreation = new DateTime(2010, 4, 1), DateDebutActivite = new DateTime(2010, 5, 1), TypeTVA = "Mensuelle", MoisClotureExercice = 12, Statut = "Dissoute", ManagerPrincipalId = gerantId },
            };

            db.Clients.AddRange(clients);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Startup] Seeded {clients.Count} demo clients.");

            // ===== Seed test Devis for the first 8 clients =====
            var seededClients = await db.Clients.Where(c => c.CabinetId == demoCabinet.Id).OrderBy(c => c.Id).Take(8).ToListAsync();
            var year = DateTime.UtcNow.Year;
            var devisList = new List<Devis>();

            for (int i = 0; i < seededClients.Count; i++)
            {
                var cl = seededClients[i];
                var idx = i + 1;
                var statut = idx switch { 1 or 2 or 3 => RefCodes.StatutDevis.Brouillon, 4 or 5 => RefCodes.StatutDevis.Soumis, 6 or 7 => RefCodes.StatutDevis.Approuve, _ => RefCodes.StatutDevis.Brouillon };
                var devis = new Devis
                {
                    CabinetId = demoCabinet.Id,
                    Reference = $"DEV-{year}-{idx:D4}",
                    ClientId = cl.Id,
                    Objet = $"Prestation de services comptables et fiscaux — {cl.RaisonSociale}",
                    Description = $"Mission d'accompagnement comptable, fiscal et social pour l'exercice {year}. Inclut la tenue de comptabilité, les déclarations TVA, IS/IR, et CNSS.",
                    Devise = "MAD",
                    DateEmission = new DateTime(year, 1, 15 + i).ToUniversalTime(),
                    DateValidite = new DateTime(year, 3, 15 + i).ToUniversalTime(),
                    Statut = statut,
                    MontantHT = (5000 + i * 2500),
                    MontantTVA = (5000 + i * 2500) * 0.20m,
                    MontantTTC = (5000 + i * 2500) * 1.20m,
                    MontantRemise = 0,
                    ConditionsParticulieres = "Paiement à 30 jours fin de mois. TVA 20%.",
                    NotesPourClient = "Merci pour votre confiance.",
                    Version = 1,
                    CommercialId = gerantId,
                    Notes = $"Devis de démonstration #{idx}",
                };
                if (statut == RefCodes.StatutDevis.Approuve) { devis.DateApprobation = devis.DateEmission.AddDays(10); devis.ValidateurId = gerantId; }
                devisList.Add(devis);
            }

            db.Set<Devis>().AddRange(devisList);
            await db.SaveChangesAsync();

            // ── Add lines to each Devis ──
            foreach (var devis in devisList)
            {
                var lignes = new List<LigneDocumentCommercial>
                {
                    new LigneDocumentCommercial { CabinetId = demoCabinet.Id, DevisId = devis.Id, Ordre = 1, TypeLigne = "Forfait", TypeService = "Comptabilite", Description = "Tenue de comptabilité mensuelle", Quantite = 12, Unite = "Mois", PrixUnitaireHT = devis.MontantHT * 0.4m / 12, TauxTVA = 20, MontantHT = devis.MontantHT * 0.4m, MontantTVA = devis.MontantHT * 0.4m * 0.20m, MontantTTC = devis.MontantHT * 0.4m * 1.20m },
                    new LigneDocumentCommercial { CabinetId = demoCabinet.Id, DevisId = devis.Id, Ordre = 2, TypeLigne = "Forfait", TypeService = "TVA", Description = "Déclarations TVA mensuelles", Quantite = 12, Unite = "Mois", PrixUnitaireHT = devis.MontantHT * 0.25m / 12, TauxTVA = 20, MontantHT = devis.MontantHT * 0.25m, MontantTVA = devis.MontantHT * 0.25m * 0.20m, MontantTTC = devis.MontantHT * 0.25m * 1.20m },
                    new LigneDocumentCommercial { CabinetId = demoCabinet.Id, DevisId = devis.Id, Ordre = 3, TypeLigne = "Forfait", TypeService = "IS", Description = "Déclaration IS annuelle", Quantite = 1, Unite = "Forfait", PrixUnitaireHT = devis.MontantHT * 0.20m, TauxTVA = 20, MontantHT = devis.MontantHT * 0.20m, MontantTVA = devis.MontantHT * 0.20m * 0.20m, MontantTTC = devis.MontantHT * 0.20m * 1.20m },
                    new LigneDocumentCommercial { CabinetId = demoCabinet.Id, DevisId = devis.Id, Ordre = 4, TypeLigne = "Forfait", TypeService = "CNSS", Description = "Déclarations CNSS trimestrielles", Quantite = 4, Unite = "Trimestre", PrixUnitaireHT = devis.MontantHT * 0.15m / 4, TauxTVA = 20, MontantHT = devis.MontantHT * 0.15m, MontantTVA = devis.MontantHT * 0.15m * 0.20m, MontantTTC = devis.MontantHT * 0.15m * 1.20m },
                };
                db.Set<LigneDocumentCommercial>().AddRange(lignes);
            }
            await db.SaveChangesAsync();
            Console.WriteLine($"[Startup] Seeded {devisList.Count} demo Devis with line items.");
        }
    }

    // ===== Auto-seed role permissions (always runs — idempotent) =====
    {
        // First, ensure all ClaimDefinitions permissions exist in the DB (lowercase domain.action format)
        foreach (var claim in ClaimDefinitions.All.Distinct())
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == claim))
            {
                var parts = claim.Split('.');
                var module = parts.Length > 0 ? parts[0] : claim;
                var action = parts.Length > 1 ? string.Join(".", parts.Skip(1)) : claim;
                db.Permissions.Add(new Permission
                {
                    Name = claim,
                    Module = module,
                    Action = action,
                    Description = $"Permission: {claim}"
                });
                await db.SaveChangesAsync();
            }
        }
        Console.WriteLine("[Startup] ClaimDefinitions permissions sync complete.");

        var allPermissions = await db.Permissions.ToListAsync();
        Console.WriteLine($"[Startup] Found {allPermissions.Count} permissions in DB.");

        // SuperAdmin & Manager get ALL permissions
        foreach (var roleName in new[] { "SuperAdmin", "Manager" })
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var existingPermIds = await db.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var missing = allPermissions.Where(p => !existingPermIds.Contains(p.Id)).ToList();
            if (missing.Any())
            {
                foreach (var perm in missing)
                    db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
                await db.SaveChangesAsync();
                Console.WriteLine($"[Startup] Assigned {missing.Count} permissions to {roleName} role.");
            }
        }

        // Assistant gets View/List/Detail + limited Create/Edit permissions
        var assistantRole = await roleManager.FindByNameAsync("Assistant");
        if (assistantRole != null)
        {
            var assistantActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "View", "List", "Detail",
                "Create", "Edit",
                "Upload", "Download", "Share",
                "Assign",
                "Advance", "Validate",
                "Export"
            };
            // Exclude system/admin/delete/configure permissions for Assistant
            var excludePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "System.Admin", "System.UserManage", "System.ConfigurationEdit",
                "system.admin", "user.manage",
                "cabinet.role.manage"
            };

            var assistantPerms = allPermissions
                .Where(p => !excludePatterns.Contains(p.Name)
                            && (assistantActions.Contains(p.Action ?? "")
                                || p.Name.EndsWith(".view", StringComparison.OrdinalIgnoreCase)
                                || p.Name.EndsWith(".list", StringComparison.OrdinalIgnoreCase)
                                || p.Name.EndsWith(".detail", StringComparison.OrdinalIgnoreCase)
                                || p.Name.EndsWith(".create", StringComparison.OrdinalIgnoreCase)
                                || p.Name.EndsWith(".edit", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var existingAssistantPermIds = await db.RolePermissions
                .Where(rp => rp.RoleId == assistantRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var missingAssistant = assistantPerms.Where(p => !existingAssistantPermIds.Contains(p.Id)).ToList();
            if (missingAssistant.Any())
            {
                foreach (var perm in missingAssistant)
                    db.RolePermissions.Add(new RolePermission { RoleId = assistantRole.Id, PermissionId = perm.Id });
                await db.SaveChangesAsync();
                Console.WriteLine($"[Startup] Assigned {missingAssistant.Count} permissions to Assistant role.");
            }
        }
    }

    // ===== Auto-seed default workflow templates for every cabinet =====
    try
    {
        var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
        var allCabinets = await db.Cabinets.Where(c => !c.IsDeleted && c.IsActive).ToListAsync();
        foreach (var cab in allCabinets)
        {
            // Check if any workflow templates already exist for this cabinet
            var hasTemplates = await db.WorkflowTemplates.AnyAsync(t => t.CabinetId == cab.Id && !t.IsDeleted);
            if (!hasTemplates)
            {
                // Get the SuperAdmin user for this cabinet (creator of templates)
                var cabAdmin = await db.Users.FirstOrDefaultAsync(u => u.CabinetId == cab.Id && u.IsActive);
                if (cabAdmin != null)
                {
                    // Seed all 8 default templates (TVA, IS, IR, CNSS, Paie, Juridique, Comptabilité, IR Salaires)
                    await workflowService.SeedAllDefaultTemplatesAsync(cab.Id, cabAdmin.Id);
                    // Also seed the generic "Workflow Standard — 9 Étapes" template
                    await workflowService.CreateDefaultTemplateAsync(cab.Id, cabAdmin.Id);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Workflow template seeding skipped or failed: {ex.Message}");
    }

    // ===== Seed cabinet employees with different profiles =====
    try
    {
        var empUserManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var cabinetEmployees = new[]
        {
            new { Email = "comptable1@cabinet-demo.ma", First = "Fatima", Last = "Bennis", Role = "Assistant", Phone = "+212 661 112233", Poste = "Comptable Senior" },
            new { Email = "comptable2@cabinet-demo.ma", First = "Youssef", Last = "Tazi", Role = "Assistant", Phone = "+212 662 223344", Poste = "Comptable Junior" },
            new { Email = "manager1@cabinet-demo.ma",   First = "Karim",   Last = "Idrissi", Role = "Manager",  Phone = "+212 663 334455", Poste = "Chef Comptable" },
            new { Email = "fiscal@cabinet-demo.ma",     First = "Amina",   Last = "Chraibi", Role = "Assistant", Phone = "+212 664 445566", Poste = "Responsable Fiscal" },
            new { Email = "social@cabinet-demo.ma",     First = "Hassan",  Last = "Alaoui",  Role = "Assistant", Phone = "+212 665 556677", Poste = "Gestionnaire Social" },
            new { Email = "juridique@cabinet-demo.ma",  First = "Nadia",   Last = "Fassi",   Role = "Manager",  Phone = "+212 666 667788", Poste = "Responsable Juridique" },
            new { Email = "stagiaire@cabinet-demo.ma",  First = "Omar",    Last = "Berrada", Role = "Assistant", Phone = "+212 667 778899", Poste = "Stagiaire Comptable" },
        };
        foreach (var emp in cabinetEmployees)
        {
            var existing = await empUserManager.FindByEmailAsync(emp.Email);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    UserName = emp.Email,
                    Email = emp.Email,
                    FirstName = emp.First,
                    LastName = emp.Last,
                    CabinetId = 1,
                    IsActive = true,
                    PhoneNumber = emp.Phone,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    PreferredLanguage = "fr-FR"
                };
                var createResult = await empUserManager.CreateAsync(user, "Employee@2026!");
                if (createResult.Succeeded)
                {
                    await empUserManager.AddToRoleAsync(user, emp.Role);
                    Console.WriteLine($"[Seed] Created employee: {emp.Email} ({emp.Role})");
                }
            }
        }
    }
    catch (Exception ex) { Console.WriteLine($"[Seed] Employee seeding: {ex.Message}"); }

    // ===== Hangfire: Sync scheduled workflow jobs =====
    try
    {
        var scheduler = scope.ServiceProvider.GetRequiredService<IWorkflowSchedulerService>();
        await scheduler.SyncScheduledJobsAsync();

        RecurringJob.AddOrUpdate<IWorkflowSchedulerService>(
            "workflow-sync-scheduled-jobs",
            svc => svc.SyncScheduledJobsAsync(),
            Cron.Hourly);

        Console.WriteLine("[Startup] Hangfire workflow scheduler synced successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Hangfire scheduler sync failed: {ex.Message}");
    }
}

app.Run();

// Hangfire: Allow all connections (dev only)
public class HangfireAllowAllFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
}

using MetaCompta.BackOffice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("MetaComptaAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7100");
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// ── Facade / Service layer DI registrations ─────────────────
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IReferenceDataFacade, ReferenceDataFacade>();
builder.Services.AddScoped<IClientFacade, ClientFacade>();
builder.Services.AddScoped<IEmployeeFacade, EmployeeFacade>();
builder.Services.AddScoped<IDeclarationFacade, DeclarationFacade>();
builder.Services.AddScoped<IPayrollFacade, PayrollFacade>();
builder.Services.AddScoped<IDashboardFacade, DashboardFacade>();
builder.Services.AddScoped<IDocumentFacade, DocumentFacade>();
builder.Services.AddScoped<ITaskFacade, TaskFacade>();
builder.Services.AddScoped<IComptabiliteFacade, ComptabiliteFacade>();
builder.Services.AddScoped<IJuridiqueFacade, JuridiqueFacade>();
builder.Services.AddScoped<IPlanningFacade, PlanningFacade>();
builder.Services.AddScoped<ICabinetFacade, CabinetFacade>();
builder.Services.AddScoped<IRapportFacade, RapportFacade>();
builder.Services.AddScoped<IParametresFacade, ParametresFacade>();
builder.Services.AddScoped<IEtatFiscalFacade, EtatFiscalFacade>();
builder.Services.AddScoped<IDepartmentFacade, DepartmentFacade>();
builder.Services.AddScoped<IWorkflowFacade, WorkflowFacade>();
builder.Services.AddScoped<ICommercialFacade, CommercialFacade>();
builder.Services.AddScoped<IMessageFacade, MessageFacade>();
builder.Services.AddSingleton<BulletinPdfService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

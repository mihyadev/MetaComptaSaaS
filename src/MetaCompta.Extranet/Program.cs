using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("MetaComptaAPI", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Docker is HTTP-only; do not redirect to https://localhost when no HTTPS port is bound.
var listenUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "";
if (listenUrls.Contains("https://", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

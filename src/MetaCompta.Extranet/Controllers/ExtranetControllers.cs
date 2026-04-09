using System;
using System.Net.Http;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

public class HomeController : ExtranetBaseController
{
    public HomeController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        var dashboard = await GetAsync<DashboardDto>("api/dashboard/extranet");
        ViewBag.Dashboard = dashboard;
        return View();
    }
}

public class DeclarationsController : ExtranetBaseController
{
    public DeclarationsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        var declarations = await GetAsync<List<DeclarationDto>>("api/declarations");
        return View(declarations);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var decl = await GetAsync<DeclarationDto>($"api/declarations/{id}");
        if (decl == null) return NotFound();
        return View(decl);
    }
}

public class DocumentsController : ExtranetBaseController
{
    public DocumentsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        var docs = await GetAsync<List<DocumentDto>>("api/documents");
        return View(docs);
    }
}

public class PaySlipsController : ExtranetBaseController
{
    public PaySlipsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        var payslips = await GetAsync<List<PaySlipDto>>("api/payroll/myslips");
        return View(payslips);
    }
}

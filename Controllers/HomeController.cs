using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? codigo) =>
        View(new ErrorViewModel { Codigo = codigo ?? 500, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

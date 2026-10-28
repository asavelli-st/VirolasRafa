using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirolasRafa.Models;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Controllers;

[AllowAnonymous]
public class AccesoController(SignInManager<Usuario> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await signInManager.PasswordSignInAsync(vm.Email.Trim(), vm.Password, vm.Recordarme, lockoutOnFailure: true);
        if (resultado.Succeeded)
            return Url.IsLocalUrl(vm.ReturnUrl) ? LocalRedirect(vm.ReturnUrl) : RedirectToAction("Index", "Home");

        var mensaje = resultado switch
        {
            { IsLockedOut: true } => "El usuario quedó bloqueado por varios intentos fallidos. Probá de nuevo en unos minutos.",
            { IsNotAllowed: true } => "El usuario está desactivado. Consultá con un administrador.",
            _ => "Email o contraseña incorrectos.",
        };
        ModelState.AddModelError("", mensaje);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Salir()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Denegado() => View();
}

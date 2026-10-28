using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirolasRafa.Models;
using VirolasRafa.Services;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UsuariosController(UsuariosService usuarios, UserManager<Usuario> userManager) : Controller
{
    private string UsuarioActualId => userManager.GetUserId(User)!;

    public async Task<IActionResult> Index() => View(await usuarios.ListarAsync(UsuarioActualId));

    [HttpGet]
    public IActionResult Crear() => View(new UsuarioCrearViewModel());

    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await usuarios.CrearAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = $"Se creó el usuario {vm.Nombre}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(string id)
    {
        var vm = await usuarios.ObtenerParaEditarAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(UsuarioEditarViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await usuarios.EditarAsync(vm, UsuarioActualId);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = $"Se guardaron los cambios de {vm.Nombre}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public Task<IActionResult> Activar(string id) => CambiarActivo(id, true);

    [HttpPost]
    public Task<IActionResult> Desactivar(string id) => CambiarActivo(id, false);

    [HttpGet]
    public async Task<IActionResult> ResetearPassword(string id)
    {
        var nombre = await usuarios.ObtenerNombreAsync(id);
        return nombre is null ? NotFound() : View(new ResetearPasswordViewModel { Id = id, Nombre = nombre });
    }

    [HttpPost]
    public async Task<IActionResult> ResetearPassword(ResetearPasswordViewModel vm)
    {
        vm.Nombre = await usuarios.ObtenerNombreAsync(vm.Id);
        if (vm.Nombre is null)
            return NotFound();
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await usuarios.ResetearPasswordAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = $"Se cambió la contraseña de {vm.Nombre}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> CambiarActivo(string id, bool activo)
    {
        var resultado = await usuarios.CambiarActivoAsync(id, activo, UsuarioActualId);
        if (resultado.Ok)
            TempData["Exito"] = activo ? "Usuario activado." : "Usuario desactivado.";
        else
            TempData["Error"] = string.Join(" ", resultado.Errores);
        return RedirectToAction(nameof(Index));
    }

    private void AgregarErrores(Resultado resultado)
    {
        foreach (var error in resultado.Errores)
            ModelState.AddModelError("", error);
    }
}

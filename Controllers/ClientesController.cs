using Microsoft.AspNetCore.Mvc;
using VirolasRafa.Services;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Controllers;

/// <summary>Clientes: Admin y Vendedor pueden dar de alta, editar y activar/desactivar.</summary>
public class ClientesController(ClientesService clientes) : Controller
{
    public async Task<IActionResult> Index(string? buscar, string estado = EstadoListado.Activos, int pagina = 1) =>
        View(new ListadoViewModel<ClienteListaViewModel>
        {
            Pagina = await clientes.ListarAsync(buscar, estado, pagina),
            Buscar = buscar,
            Estado = estado,
        });

    public async Task<IActionResult> Detalle(int id)
    {
        var vm = await clientes.ObtenerDetalleAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet]
    public IActionResult Crear() => View(new ClienteFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Crear(ClienteFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (resultado, id) = await clientes.CrearAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = $"Se creó el cliente {vm.Nombre.Trim()}.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var vm = await clientes.ObtenerParaEditarAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(ClienteFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await clientes.EditarAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = "Se guardaron los cambios.";
        return RedirectToAction(nameof(Detalle), new { id = vm.Id });
    }

    [HttpPost]
    public Task<IActionResult> Activar(int id) => CambiarActivo(id, true);

    [HttpPost]
    public Task<IActionResult> Desactivar(int id) => CambiarActivo(id, false);

    private async Task<IActionResult> CambiarActivo(int id, bool activo)
    {
        var resultado = await clientes.CambiarActivoAsync(id, activo);
        if (resultado.Ok)
            TempData["Exito"] = activo ? "Cliente activado." : "Cliente desactivado.";
        else
            TempData["Error"] = string.Join(" ", resultado.Errores);
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private void AgregarErrores(Resultado resultado)
    {
        foreach (var error in resultado.Errores)
            ModelState.AddModelError("", error);
    }
}

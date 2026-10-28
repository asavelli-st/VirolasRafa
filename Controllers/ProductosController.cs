using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirolasRafa.Models;
using VirolasRafa.Services;
using VirolasRafa.Services.Importacion;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Controllers;

/// <summary>
/// Productos. El Vendedor solo consulta, y sus acciones usan ViewModels sin costo ni margen.
/// Todo lo demás es solo para Admin.
/// </summary>
public class ProductosController(ProductosService productos, ImportacionProductosService importacion, UserManager<Usuario> userManager) : Controller
{
    private const long TamanioMaximoArchivo = 5 * 1024 * 1024;

    private bool EsAdmin => User.IsInRole(Roles.Admin);

    public async Task<IActionResult> Index(string? buscar, string estado = EstadoListado.Activos, int pagina = 1)
    {
        if (!EsAdmin)
            return View("IndexVendedor", new ListadoViewModel<ProductoVendedorViewModel>
            {
                Pagina = await productos.ListarVendedorAsync(buscar, pagina),
                Buscar = buscar,
            });

        return View(new ListadoViewModel<ProductoAdminViewModel>
        {
            Pagina = await productos.ListarAdminAsync(buscar, estado, pagina),
            Buscar = buscar,
            Estado = estado,
        });
    }

    public async Task<IActionResult> Detalle(int id)
    {
        if (!EsAdmin)
        {
            var vendedor = await productos.ObtenerVendedorAsync(id);
            return vendedor is null ? NotFound() : View("DetalleVendedor", vendedor);
        }

        var vm = await productos.ObtenerAdminAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet, Authorize(Roles = Roles.Admin)]
    public IActionResult Crear() => View(new ProductoFormViewModel());

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Crear(ProductoFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (resultado, id) = await productos.CrearAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = $"Se creó el producto {vm.Codigo.Trim()}.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Editar(int id)
    {
        var vm = await productos.ObtenerParaEditarAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Editar(ProductoFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await productos.EditarAsync(vm);
        if (!resultado.Ok)
        {
            AgregarErrores(resultado);
            return View(vm);
        }

        TempData["Exito"] = "Se guardaron los cambios.";
        return RedirectToAction(nameof(Detalle), new { id = vm.Id });
    }

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Activar(int id) => CambiarActivo(id, true);

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public Task<IActionResult> Desactivar(int id) => CambiarActivo(id, false);

    [HttpGet, Authorize(Roles = Roles.Admin)]
    public IActionResult Importar() => View(new VistaPreviaImportacionViewModel());

    [HttpPost, Authorize(Roles = Roles.Admin)]
    [RequestSizeLimit(TamanioMaximoArchivo + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = TamanioMaximoArchivo + 64 * 1024)]
    public async Task<IActionResult> Importar(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return View(new VistaPreviaImportacionViewModel { ErrorGeneral = "Elegí un archivo .xlsx." });
        if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return View(new VistaPreviaImportacionViewModel { NombreArchivo = archivo.FileName, ErrorGeneral = "El archivo tiene que ser un Excel .xlsx." });
        if (archivo.Length > TamanioMaximoArchivo)
            return View(new VistaPreviaImportacionViewModel { NombreArchivo = archivo.FileName, ErrorGeneral = "El archivo supera los 5 MB." });

        await using var contenido = new MemoryStream();
        await archivo.CopyToAsync(contenido);
        contenido.Position = 0;
        return View(await importacion.AnalizarAsync(contenido, archivo.FileName, userManager.GetUserId(User)!));
    }

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ConfirmarImportacion(string token)
    {
        var (resultado, resumen) = await importacion.ConfirmarAsync(token, userManager.GetUserId(User)!);
        if (!resultado.Ok)
        {
            TempData["Error"] = string.Join(" ", resultado.Errores);
            return RedirectToAction(nameof(Importar));
        }

        static string Cantidad(int n, string singular, string plural) => $"{n} {(n == 1 ? singular : plural)}";
        TempData["Exito"] = $"Importación terminada: {Cantidad(resumen!.Creados, "producto nuevo", "productos nuevos")} y "
            + $"{Cantidad(resumen.Actualizados, "actualizado", "actualizados")}"
            + (resumen.Reactivados > 0 ? $" ({Cantidad(resumen.Reactivados, "reactivado", "reactivados")})." : ".");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = Roles.Admin)]
    public IActionResult Plantilla() =>
        File(PlantillaImportacion.Generar(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", PlantillaImportacion.NombreArchivo);

    private async Task<IActionResult> CambiarActivo(int id, bool activo)
    {
        var resultado = await productos.CambiarActivoAsync(id, activo);
        if (resultado.Ok)
            TempData["Exito"] = activo ? "Producto activado." : "Producto desactivado.";
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

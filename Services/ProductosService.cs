using Microsoft.EntityFrameworkCore;
using VirolasRafa.Data;
using VirolasRafa.Models;
using VirolasRafa.Services.Infraestructura;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Services;

/// <summary>
/// ABM de productos. Las consultas "Vendedor" proyectan solo columnas sin costo: el costo ni siquiera se lee de la base.
/// </summary>
public class ProductosService(AppDbContext db)
{
    public Task<Pagina<ProductoAdminViewModel>> ListarAdminAsync(string? buscar, string estado, int pagina)
    {
        var consulta = Buscar(db.Productos.AsNoTracking(), buscar);
        consulta = estado switch
        {
            EstadoListado.Inactivos => consulta.Where(p => !p.Activo),
            EstadoListado.Todos => consulta,
            _ => consulta.Where(p => p.Activo),
        };
        return Pagina<ProductoAdminViewModel>.CrearAsync(ProyectarAdmin(consulta.OrderBy(p => p.Codigo)), pagina);
    }

    public Task<Pagina<ProductoVendedorViewModel>> ListarVendedorAsync(string? buscar, int pagina)
    {
        var consulta = Buscar(db.Productos.AsNoTracking(), buscar).Where(p => p.Activo);
        return Pagina<ProductoVendedorViewModel>.CrearAsync(ProyectarVendedor(consulta.OrderBy(p => p.Codigo)), pagina);
    }

    public Task<ProductoAdminViewModel?> ObtenerAdminAsync(int id) =>
        ProyectarAdmin(db.Productos.AsNoTracking().Where(p => p.Id == id)).FirstOrDefaultAsync();

    /// <summary>El Vendedor solo ve productos activos.</summary>
    public Task<ProductoVendedorViewModel?> ObtenerVendedorAsync(int id) =>
        ProyectarVendedor(db.Productos.AsNoTracking().Where(p => p.Id == id && p.Activo)).FirstOrDefaultAsync();

    public async Task<ProductoFormViewModel?> ObtenerParaEditarAsync(int id)
    {
        var p = await db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? null : new ProductoFormViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            Costo = p.Costo,
            Precio = p.Precio,
            Margen = CalculosPrecio.Margen(p.Costo, p.Precio),
            Cantidad2 = p.Cantidad2,
            Precio2 = p.Precio2,
            Margen2 = CalculosPrecio.Margen(p.Costo, p.Precio2),
            Cantidad3 = p.Cantidad3,
            Precio3 = p.Precio3,
            Margen3 = CalculosPrecio.Margen(p.Costo, p.Precio3),
        };
    }

    public async Task<(Resultado Resultado, int Id)> CrearAsync(ProductoFormViewModel vm)
    {
        var producto = new Producto { Activo = true };
        var resultado = await GuardarAsync(producto, vm);
        return (resultado, producto.Id);
    }

    public async Task<Resultado> EditarAsync(ProductoFormViewModel vm)
    {
        var producto = await db.Productos.FindAsync(vm.Id);
        return producto is null ? Resultado.Error("El producto no existe.") : await GuardarAsync(producto, vm);
    }

    public async Task<Resultado> CambiarActivoAsync(int id, bool activo)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Resultado.Error("El producto no existe.");
        producto.Activo = activo;
        await db.SaveChangesAsync();
        return Resultado.Exito();
    }

    private async Task<Resultado> GuardarAsync(Producto producto, ProductoFormViewModel vm)
    {
        var codigo = vm.Codigo.Trim();
        var errores = new List<string>();

        var precio = CalculosPrecio.PrecioSegunUltimoEditado(vm.Costo, vm.Precio, vm.Margen, vm.UsarBase == ProductoFormViewModel.UsarMargen);
        var precio2 = CalculosPrecio.PrecioSegunUltimoEditado(vm.Costo, vm.Precio2, vm.Margen2, vm.Usar2 == ProductoFormViewModel.UsarMargen);
        var precio3 = CalculosPrecio.PrecioSegunUltimoEditado(vm.Costo, vm.Precio3, vm.Margen3, vm.Usar3 == ProductoFormViewModel.UsarMargen);

        if (precio is null)
            errores.Add("Cargá el precio o el margen.");
        foreach (var (nombre, valor) in new[] { ("El costo", (decimal?)vm.Costo), ("El precio", vm.Precio), ("El precio del escalón 2", vm.Precio2),
                     ("El precio del escalón 3", vm.Precio3), ("El margen", vm.Margen), ("El margen del escalón 2", vm.Margen2), ("El margen del escalón 3", vm.Margen3) })
        {
            if (valor is decimal v && Numeros.TieneMasDecimales(v, 2))
                errores.Add($"{nombre} admite hasta 2 decimales.");
        }
        if (errores.Count == 0)
            errores.AddRange(ReglasProducto.Validar(vm.Costo, precio!.Value, vm.Cantidad2, precio2, vm.Cantidad3, precio3));

        if (await db.Productos.AnyAsync(p => p.Codigo == codigo && p.Id != producto.Id))
            errores.Add($"Ya existe un producto con el código {codigo}.");
        if (errores.Count > 0)
            return Resultado.Error([.. errores]);

        producto.Codigo = codigo;
        producto.Descripcion = vm.Descripcion.Trim();
        producto.Costo = vm.Costo;
        producto.Precio = precio!.Value;
        producto.Cantidad2 = vm.Cantidad2;
        producto.Precio2 = precio2;
        producto.Cantidad3 = vm.Cantidad3;
        producto.Precio3 = precio3;

        if (producto.Id == 0)
            db.Productos.Add(producto);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ErroresDb.EsDuplicado(ex))
        {
            return Resultado.Error($"Ya existe un producto con el código {codigo}.");
        }
        return Resultado.Exito();
    }

    private static IQueryable<Producto> Buscar(IQueryable<Producto> consulta, string? buscar)
    {
        if (string.IsNullOrWhiteSpace(buscar))
            return consulta;
        var texto = buscar.Trim();
        return consulta.Where(p => p.Codigo.Contains(texto) || p.Descripcion.Contains(texto));
    }

    private static IQueryable<ProductoVendedorViewModel> ProyectarVendedor(IQueryable<Producto> consulta) =>
        consulta.Select(p => new ProductoVendedorViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Cantidad2 = p.Cantidad2,
            Precio2 = p.Precio2,
            Cantidad3 = p.Cantidad3,
            Precio3 = p.Precio3,
            Activo = p.Activo,
        });

    private static IQueryable<ProductoAdminViewModel> ProyectarAdmin(IQueryable<Producto> consulta) =>
        consulta.Select(p => new ProductoAdminViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            Costo = p.Costo,
            Precio = p.Precio,
            Cantidad2 = p.Cantidad2,
            Precio2 = p.Precio2,
            Cantidad3 = p.Cantidad3,
            Precio3 = p.Precio3,
            Activo = p.Activo,
        });
}

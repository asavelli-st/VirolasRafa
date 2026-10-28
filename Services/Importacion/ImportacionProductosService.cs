using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirolasRafa.Data;
using VirolasRafa.Models;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Services.Importacion;

/// <summary>
/// Importación en dos pasos: <see cref="AnalizarAsync"/> lee el archivo y arma la vista previa (las filas válidas quedan
/// guardadas en memoria 30 minutos, a nombre del usuario); <see cref="ConfirmarAsync"/> vuelve a comparar contra la base
/// y aplica los cambios en una transacción. Nunca borra ni desactiva productos.
/// </summary>
public class ImportacionProductosService(AppDbContext db, IMemoryCache cache)
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(30);

    public async Task<VistaPreviaImportacionViewModel> AnalizarAsync(Stream archivo, string nombreArchivo, string usuarioId)
    {
        var lectura = LectorExcelProductos.Leer(archivo);
        if (lectura.ErrorGeneral is not null)
            return new VistaPreviaImportacionViewModel { NombreArchivo = nombreArchivo, ErrorGeneral = lectura.ErrorGeneral };

        var clasificadas = ClasificadorImportacion.Clasificar(lectura.Validas, await ExistentesAsync(lectura.Validas));

        var token = Guid.NewGuid().ToString("N");
        cache.Set(Clave(usuarioId, token), lectura.Validas, Vigencia);

        return new VistaPreviaImportacionViewModel
        {
            NombreArchivo = nombreArchivo,
            Token = token,
            Nuevos = clasificadas.Where(f => f.Tipo == TipoCambio.Nuevo).ToList(),
            Actualizados = clasificadas.Where(f => f.Tipo == TipoCambio.Actualiza).ToList(),
            SinCambios = clasificadas.Count(f => f.Tipo == TipoCambio.SinCambios),
            Errores = lectura.Errores,
        };
    }

    public async Task<(Resultado Resultado, ResumenImportacion? Resumen)> ConfirmarAsync(string token, string usuarioId)
    {
        var clave = Clave(usuarioId, token);
        if (!cache.TryGetValue(clave, out List<FilaImportada>? filas) || filas is null)
            return (Resultado.Error("La vista previa venció o ya se usó. Volvé a subir el archivo."), null);

        await using var transaccion = await db.Database.BeginTransactionAsync();

        var codigos = filas.Select(f => f.Codigo).ToList();
        var productos = await db.Productos.Where(p => codigos.Contains(p.Codigo)).ToDictionaryAsync(p => p.Codigo, StringComparer.OrdinalIgnoreCase);
        var clasificadas = ClasificadorImportacion.Clasificar(filas, productos.ToDictionary(
            kv => kv.Key, kv => AExistente(kv.Value), StringComparer.OrdinalIgnoreCase));

        int creados = 0, actualizados = 0, reactivados = 0;
        foreach (var c in clasificadas)
        {
            if (c.Tipo == TipoCambio.SinCambios)
                continue;

            var f = c.Fila;
            if (!productos.TryGetValue(f.Codigo, out var producto))
            {
                producto = new Producto { Codigo = f.Codigo };
                db.Productos.Add(producto);
                creados++;
            }
            else
            {
                actualizados++;
                if (c.Reactiva)
                    reactivados++;
            }

            producto.Descripcion = f.Descripcion;
            producto.Costo = f.Costo;
            producto.Precio = f.Precio;
            producto.Cantidad2 = f.Cantidad2;
            producto.Precio2 = f.Precio2;
            producto.Cantidad3 = f.Cantidad3;
            producto.Precio3 = f.Precio3;
            producto.Activo = true;
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        cache.Remove(clave);
        return (Resultado.Exito(), new ResumenImportacion(creados, actualizados, reactivados));
    }

    private async Task<Dictionary<string, ProductoExistente>> ExistentesAsync(IEnumerable<FilaImportada> filas)
    {
        var codigos = filas.Select(f => f.Codigo).ToList();
        var productos = await db.Productos.AsNoTracking().Where(p => codigos.Contains(p.Codigo)).ToListAsync();
        return productos.ToDictionary(p => p.Codigo, AExistente, StringComparer.OrdinalIgnoreCase);
    }

    private static ProductoExistente AExistente(Producto p) =>
        new(p.Codigo, p.Descripcion, p.Costo, p.Precio, p.Cantidad2, p.Precio2, p.Cantidad3, p.Precio3, p.Activo);

    private static string Clave(string usuarioId, string token) => $"importacion-productos:{usuarioId}:{token}";
}

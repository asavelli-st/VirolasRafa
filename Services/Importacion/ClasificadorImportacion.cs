namespace VirolasRafa.Services.Importacion;

public enum TipoCambio { Nuevo, Actualiza, SinCambios }

public record ProductoExistente(string Codigo, string Descripcion, decimal Costo, decimal Precio,
    int? Cantidad2, decimal? Precio2, int? Cantidad3, decimal? Precio3, bool Activo);

/// <param name="Reactiva">El producto existía desactivado y vuelve a quedar activo.</param>
public record FilaClasificada(FilaImportada Fila, TipoCambio Tipo, ProductoExistente? Anterior, bool Reactiva);

public static class ClasificadorImportacion
{
    /// <summary>
    /// Compara cada fila válida con el producto del mismo código. Un producto desactivado que viene en el archivo
    /// se actualiza y se reactiva, aunque sus datos no cambien.
    /// </summary>
    public static List<FilaClasificada> Clasificar(IEnumerable<FilaImportada> filas, IReadOnlyDictionary<string, ProductoExistente> existentes) =>
        filas.Select(f =>
        {
            if (!existentes.TryGetValue(f.Codigo, out var e))
                return new FilaClasificada(f, TipoCambio.Nuevo, null, false);

            var igual = e.Descripcion == f.Descripcion && e.Costo == f.Costo && e.Precio == f.Precio
                && e.Cantidad2 == f.Cantidad2 && e.Precio2 == f.Precio2 && e.Cantidad3 == f.Cantidad3 && e.Precio3 == f.Precio3;
            var tipo = igual && e.Activo ? TipoCambio.SinCambios : TipoCambio.Actualiza;
            return new FilaClasificada(f, tipo, e, !e.Activo);
        }).ToList();
}

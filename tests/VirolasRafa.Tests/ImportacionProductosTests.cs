using ClosedXML.Excel;
using VirolasRafa.Services.Importacion;

namespace VirolasRafa.Tests;

public class ImportacionProductosTests
{
    /// <summary>Arma un .xlsx en memoria: la primera fila son los encabezados.</summary>
    private static MemoryStream Excel(params object?[][] filas)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Hoja1");
        for (var f = 0; f < filas.Length; f++)
        {
            for (var c = 0; c < filas[f].Length; c++)
            {
                hoja.Cell(f + 1, c + 1).Value = filas[f][c] switch
                {
                    null => Blank.Value,
                    string s => s,
                    int n => n,
                    double d => d,
                    _ => throw new ArgumentException("Tipo no soportado"),
                };
            }
        }
        var salida = new MemoryStream();
        libro.SaveAs(salida);
        salida.Position = 0;
        return salida;
    }

    private static readonly object?[] Encabezados = ["Codigo", "Descripcion", "Costo", "Margen", "Precio", "Cantidad2", "Precio2", "Cantidad3", "Precio3"];

    [Fact]
    public void LeeFilasValidasConPrecioYEscalones()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["1001", "Tornillo", 40, null, 70, 100, 62, 500, 55]));

        Assert.Null(r.ErrorGeneral);
        Assert.Empty(r.Errores);
        var f = Assert.Single(r.Validas);
        Assert.Equal(("1001", "Tornillo", 40m, 70m, 100, 62m, 500, 55m), (f.Codigo, f.Descripcion, f.Costo, f.Precio, f.Cantidad2, f.Precio2, f.Cantidad3, f.Precio3));
    }

    [Fact]
    public void ConSoloMargenCalculaElPrecio()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["1002", "Tarugo", 25, 90, null]));
        Assert.Equal(47.5m, Assert.Single(r.Validas).Precio);
    }

    [Fact]
    public void ConPrecioYMargenGanaElPrecio()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["1002", "Tarugo", 25, 90, 48]));
        Assert.Equal(48m, Assert.Single(r.Validas).Precio);
    }

    [Fact]
    public void EncabezadosSinImportarMayusculasAcentosNiOrden()
    {
        var r = LectorExcelProductos.Leer(Excel(["PRECIO", "Descripción", "código", "costo"], [70, "Tornillo", 1001, 40]));
        var f = Assert.Single(r.Validas);
        Assert.Equal(("1001", 70m), (f.Codigo, f.Precio));
    }

    [Fact]
    public void AceptaNumerosEscritosComoTexto()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["A1", "Pintura", "61.000,50", null, "98000.75"]));
        var f = Assert.Single(r.Validas);
        Assert.Equal((61000.50m, 98000.75m), (f.Costo, f.Precio));
    }

    [Fact]
    public void RedondeaImportesA2Decimales()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["A1", "Pintura", 10.004, null, 15.555]));
        var f = Assert.Single(r.Validas);
        Assert.Equal((10m, 15.56m), (f.Costo, f.Precio));
    }

    [Fact]
    public void FaltanColumnasObligatorias()
    {
        var r = LectorExcelProductos.Leer(Excel(["Codigo", "Costo"], ["1", 2]));
        Assert.NotNull(r.ErrorGeneral);
        Assert.Contains("Descripcion", r.ErrorGeneral);
        Assert.Contains("Precio o Margen", r.ErrorGeneral);
    }

    [Fact]
    public void ArchivoQueNoEsExcel()
    {
        var r = LectorExcelProductos.Leer(new MemoryStream("no soy un excel"u8.ToArray()));
        Assert.NotNull(r.ErrorGeneral);
    }

    [Fact]
    public void ReportaErroresPorFilaEImportaLasValidas()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados,
            ["1001", "Bien", 40, null, 70],
            [null, "Sin código", 40, null, 70],
            ["1003", "Costo inválido", "abc", null, 70],
            ["1004", "Sin precio ni margen", 40, null, null],
            ["1005", "Escalón inconsistente", 40, null, 70, 100, 62, 50, 55],
            ["1006", "Cantidad con decimales", 40, null, 70, 10.5, 62],
            ["1001", "Código repetido", 40, null, 70]));

        Assert.Equal(["1001"], r.Validas.Select(f => f.Codigo));
        Assert.Equal([3, 4, 5, 6, 7, 8], r.Errores.Select(e => e.Fila));
        Assert.Contains(r.Errores.Single(e => e.Fila == 8).Errores, m => m.Contains("repetido") && m.Contains("fila 2"));
        Assert.Contains(r.Errores.Single(e => e.Fila == 4).Errores, m => m.Contains("costo"));
        Assert.DoesNotContain(r.Errores.Single(e => e.Fila == 4).Errores, m => m == "Falta el costo.");
    }

    [Fact]
    public void IgnoraFilasVacias()
    {
        var r = LectorExcelProductos.Leer(Excel(Encabezados, ["1001", "A", 1, null, 2], [null, null, null], ["1002", "B", 1, null, 2]));
        Assert.Equal(2, r.Validas.Count);
        Assert.Empty(r.Errores);
    }

    [Fact]
    public void LaPlantillaSeLeeSinErrores()
    {
        var r = LectorExcelProductos.Leer(new MemoryStream(PlantillaImportacion.Generar()));
        Assert.Null(r.ErrorGeneral);
        Assert.Empty(r.Errores);
        Assert.Equal(2, r.Validas.Count);
    }

    private static FilaImportada Fila(string codigo, decimal precio = 70) => new(2, codigo, "Desc", 40, precio, null, null, null, null);

    private static ProductoExistente Existente(string codigo, decimal precio = 70, bool activo = true) =>
        new(codigo, "Desc", 40, precio, null, null, null, null, activo);

    [Fact]
    public void ClasificaNuevosActualizadosYSinCambios()
    {
        var existentes = new Dictionary<string, ProductoExistente>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = Existente("A"),
            ["B"] = Existente("B", precio: 60),
        };

        var c = ClasificadorImportacion.Clasificar([Fila("A"), Fila("b"), Fila("C")], existentes);

        Assert.Equal([TipoCambio.SinCambios, TipoCambio.Actualiza, TipoCambio.Nuevo], c.Select(x => x.Tipo));
        Assert.All(c, x => Assert.False(x.Reactiva));
    }

    [Fact]
    public void UnProductoDesactivadoSeActualizaYReactivaAunqueNoCambie()
    {
        var existentes = new Dictionary<string, ProductoExistente> { ["A"] = Existente("A", activo: false) };

        var c = Assert.Single(ClasificadorImportacion.Clasificar([Fila("A")], existentes));

        Assert.Equal(TipoCambio.Actualiza, c.Tipo);
        Assert.True(c.Reactiva);
    }
}

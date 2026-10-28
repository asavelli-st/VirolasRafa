using ClosedXML.Excel;

namespace VirolasRafa.Services.Importacion;

/// <summary>Plantilla descargable para la importación de productos.</summary>
public static class PlantillaImportacion
{
    public const string NombreArchivo = "Plantilla-Productos.xlsx";

    public static byte[] Generar()
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Productos");

        for (var i = 0; i < LectorExcelProductos.Columnas.Length; i++)
            hoja.Cell(1, i + 1).Value = LectorExcelProductos.Columnas[i];
        var encabezado = hoja.Range(1, 1, 1, LectorExcelProductos.Columnas.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCEEEA");

        // Ejemplos: uno con precio (se ignora el margen) y uno solo con margen (el precio se calcula desde el costo).
        object?[][] ejemplos =
        [
            ["1001", "Tornillo autoperforante 6x1\"", 40, null, 70, 100, 62, 500, 55],
            ["1002", "Tarugo Fischer 8 mm", 25, 90, null, 50, 42, null, null],
        ];
        for (var f = 0; f < ejemplos.Length; f++)
        {
            for (var c = 0; c < ejemplos[f].Length; c++)
            {
                var celda = hoja.Cell(f + 2, c + 1);
                celda.Value = ejemplos[f][c] switch
                {
                    null => Blank.Value,
                    string s => s,
                    int n => n,
                    _ => Blank.Value,
                };
            }
        }

        hoja.Column(1).Style.NumberFormat.Format = "@";
        foreach (var c in new[] { 3, 5, 7, 9 })
            hoja.Column(c).Style.NumberFormat.Format = "#,##0.00";
        hoja.Column(4).Style.NumberFormat.Format = "0.00";
        hoja.Columns().AdjustToContents();
        hoja.SheetView.FreezeRows(1);

        var ayuda = libro.Worksheets.Add("Instrucciones");
        string[] lineas =
        [
            "Importación de productos",
            "",
            "Se lee solo la primera hoja. La fila 1 lleva los encabezados; el orden de las columnas no importa.",
            "Codigo: obligatorio. Si ya existe, el producto se actualiza (y se reactiva si estaba desactivado); si no, se crea.",
            "Descripcion y Costo: obligatorios.",
            "Precio o Margen (%): al menos uno. Si vienen los dos, gana Precio. Con solo Margen: Precio = Costo x (1 + Margen/100).",
            "Cantidad2 / Precio2 y Cantidad3 / Precio3: opcionales, siempre de a pares. Cantidad2 > 1, Cantidad3 > Cantidad2.",
            "Los precios por cantidad tienen que ser menores que el escalón anterior. No puede haber escalón 3 sin escalón 2.",
            "Los productos que no están en el archivo no se modifican ni se borran.",
            "Las filas con errores no se importan; se muestran en la vista previa antes de confirmar.",
        ];
        for (var i = 0; i < lineas.Length; i++)
            ayuda.Cell(i + 1, 1).Value = lineas[i];
        ayuda.Cell(1, 1).Style.Font.Bold = true;
        ayuda.Column(1).Width = 120;

        using var salida = new MemoryStream();
        libro.SaveAs(salida);
        return salida.ToArray();
    }
}

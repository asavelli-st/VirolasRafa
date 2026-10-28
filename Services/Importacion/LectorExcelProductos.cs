using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace VirolasRafa.Services.Importacion;

/// <summary>Una fila válida del Excel, con el precio ya resuelto (si vino solo margen, calculado desde el costo).</summary>
public record FilaImportada(int Fila, string Codigo, string Descripcion, decimal Costo, decimal Precio,
    int? Cantidad2, decimal? Precio2, int? Cantidad3, decimal? Precio3);

public record FilaConError(int Fila, string? Codigo, IReadOnlyList<string> Errores);

public class ResultadoLectura
{
    public List<FilaImportada> Validas { get; } = [];
    public List<FilaConError> Errores { get; } = [];

    /// <summary>Error que impide leer el archivo entero (formato inválido, faltan columnas).</summary>
    public string? ErrorGeneral { get; init; }
}

/// <summary>
/// Lee la lista de precios desde la primera hoja de un .xlsx. La fila 1 tiene los encabezados (sin importar mayúsculas,
/// acentos ni el orden): Codigo, Descripcion, Costo, Margen y/o Precio, y opcionales Cantidad2, Precio2, Cantidad3, Precio3.
/// </summary>
public static class LectorExcelProductos
{
    public static readonly string[] Columnas = ["Codigo", "Descripcion", "Costo", "Margen", "Precio", "Cantidad2", "Precio2", "Cantidad3", "Precio3"];

    public static ResultadoLectura Leer(Stream xlsx)
    {
        XLWorkbook libro;
        try
        {
            libro = new XLWorkbook(xlsx);
        }
        catch (Exception)
        {
            return new ResultadoLectura { ErrorGeneral = "El archivo no es un Excel (.xlsx) válido." };
        }

        using (libro)
        {
            var hoja = libro.Worksheets.FirstOrDefault();
            var encabezado = hoja?.FirstRowUsed();
            if (hoja is null || encabezado is null)
                return new ResultadoLectura { ErrorGeneral = "El archivo está vacío." };

            var columnas = new Dictionary<string, int>();
            foreach (var celda in encabezado.CellsUsed())
            {
                var nombre = Normalizar(celda.GetFormattedString());
                var columna = Columnas.FirstOrDefault(c => Normalizar(c) == nombre);
                if (columna is not null)
                    columnas.TryAdd(columna, celda.Address.ColumnNumber);
            }

            var faltan = new[] { "Codigo", "Descripcion", "Costo" }.Where(c => !columnas.ContainsKey(c)).ToList();
            if (!columnas.ContainsKey("Precio") && !columnas.ContainsKey("Margen"))
                faltan.Add("Precio o Margen");
            if (faltan.Count > 0)
                return new ResultadoLectura { ErrorGeneral = $"Faltan columnas en la fila de encabezados: {string.Join(", ", faltan)}." };

            var resultado = new ResultadoLectura();
            var filaDeCodigo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var ultima = hoja.LastRowUsed()!.RowNumber();

            for (var numero = encabezado.RowNumber() + 1; numero <= ultima; numero++)
            {
                var fila = hoja.Row(numero);
                IXLCell? Celda(string columna) => columnas.TryGetValue(columna, out var c) ? fila.Cell(c) : null;
                if (columnas.Values.All(c => fila.Cell(c).IsEmpty()))
                    continue;

                var errores = new List<string>();
                var codigo = Texto(Celda("Codigo"));
                var descripcion = Texto(Celda("Descripcion"));

                if (codigo.Length == 0)
                    errores.Add("Falta el código.");
                else if (codigo.Length > 50)
                    errores.Add("El código tiene más de 50 caracteres.");
                else if (filaDeCodigo.TryGetValue(codigo, out var anterior))
                    errores.Add($"El código está repetido (ya aparece en la fila {anterior}).");
                if (descripcion.Length == 0)
                    errores.Add("Falta la descripción.");
                else if (descripcion.Length > 250)
                    errores.Add("La descripción tiene más de 250 caracteres.");

                var antes = errores.Count;
                var costo = Importe(Celda("Costo"), "el costo", errores);
                if (costo is null && errores.Count == antes)
                    errores.Add("Falta el costo.");

                antes = errores.Count;
                var precio = Importe(Celda("Precio"), "el precio", errores);
                var margen = Numero(Celda("Margen"), "el margen", errores);
                var precioOMargenInvalido = errores.Count > antes;
                // Si vienen precio y margen, gana el precio.
                if (precio is null && margen is not null && costo is not null)
                    precio = CalculosPrecio.PrecioDesdeMargen(costo.Value, margen.Value);
                if (precio is null && margen is null && !precioOMargenInvalido)
                    errores.Add("Falta el precio o el margen.");

                var cantidad2 = Cantidad(Celda("Cantidad2"), "escalón 2", errores);
                var precio2 = Importe(Celda("Precio2"), "el precio del escalón 2", errores);
                var cantidad3 = Cantidad(Celda("Cantidad3"), "escalón 3", errores);
                var precio3 = Importe(Celda("Precio3"), "el precio del escalón 3", errores);

                if (errores.Count == 0)
                    errores.AddRange(ReglasProducto.Validar(costo!.Value, precio!.Value, cantidad2, precio2, cantidad3, precio3));

                if (codigo.Length > 0)
                    filaDeCodigo.TryAdd(codigo, numero);

                if (errores.Count > 0)
                    resultado.Errores.Add(new FilaConError(numero, codigo.Length > 0 ? codigo : null, errores));
                else
                    resultado.Validas.Add(new FilaImportada(numero, codigo, descripcion, costo!.Value, precio!.Value, cantidad2, precio2, cantidad3, precio3));
            }

            return resultado;
        }
    }

    private static string Texto(IXLCell? celda) => celda is null ? "" : celda.GetFormattedString().Trim();

    /// <summary>Lee un número de una celda numérica o de texto ("1.234,56", "1234.56"). Vacía → null.</summary>
    private static decimal? Numero(IXLCell? celda, string nombre, List<string> errores)
    {
        if (celda is null || celda.IsEmpty() || Texto(celda).Length == 0)
            return null;
        try
        {
            if (celda.DataType == XLDataType.Number)
                return (decimal)celda.GetDouble();
            if (Numeros.TryParse(celda.GetFormattedString(), out var valor))
                return valor;
        }
        catch (Exception ex) when (ex is OverflowException or InvalidCastException or FormatException)
        {
        }
        errores.Add($"{Mayuscula(nombre)} no es un número válido ({celda.GetFormattedString()}).");
        return null;
    }

    /// <summary>Importe redondeado a 2 decimales (las fórmulas de Excel suelen dejar muchos decimales).</summary>
    private static decimal? Importe(IXLCell? celda, string nombre, List<string> errores) =>
        Numero(celda, nombre, errores) is decimal v ? CalculosPrecio.Redondear(v) : null;

    private static int? Cantidad(IXLCell? celda, string escalon, List<string> errores)
    {
        var valor = Numero(celda, $"la cantidad del {escalon}", errores);
        if (valor is null)
            return null;
        if (valor != Math.Truncate(valor.Value) || valor > int.MaxValue || valor < int.MinValue)
        {
            errores.Add($"La cantidad del {escalon} tiene que ser un número entero.");
            return null;
        }
        return (int)valor.Value;
    }

    private static string Mayuscula(string texto) => char.ToUpper(texto[0], CultureInfo.CurrentCulture) + texto[1..];

    private static string Normalizar(string texto)
    {
        var sinAcentos = new string(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return new string(sinAcentos.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}

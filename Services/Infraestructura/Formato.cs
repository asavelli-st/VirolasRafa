using System.Globalization;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>Formatos de pantalla en es-AR (la cultura la fija RequestLocalization).</summary>
public static class Formato
{
    public static string Moneda(decimal valor) => valor.ToString("C2", CultureInfo.CurrentCulture);

    public static string Moneda(decimal? valor) => valor is null ? "" : Moneda(valor.Value);

    /// <summary>Porcentaje con hasta 2 decimales; vacío si es nulo (por ejemplo, margen con costo 0).</summary>
    public static string Porcentaje(decimal? valor) => valor is null ? "" : valor.Value.ToString("0.##", CultureInfo.CurrentCulture) + " %";
}

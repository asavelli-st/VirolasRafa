using System.Globalization;

namespace VirolasRafa.Services;

/// <summary>
/// Lectura de números escritos a mano o en planillas, en formato argentino o con punto decimal.
/// Con coma: el punto es separador de miles ("1.234,56"). Sin coma: un único punto es el decimal ("1234.56").
/// Se ignoran espacios, "$" y "%".
/// </summary>
public static class Numeros
{
    public static bool TryParse(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var limpio = new string(texto.Where(c => !char.IsWhiteSpace(c) && c != '$' && c != '%').ToArray());
        if (limpio.Contains(','))
            limpio = limpio.Replace(".", "").Replace(',', '.');
        else if (limpio.Count(c => c == '.') > 1)
            limpio = limpio.Replace(".", "");

        return decimal.TryParse(limpio, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor);
    }

    /// <summary>True si el valor tiene más de <paramref name="decimales"/> decimales distintos de cero.</summary>
    public static bool TieneMasDecimales(decimal valor, int decimales) => Math.Round(valor, decimales) != valor;
}

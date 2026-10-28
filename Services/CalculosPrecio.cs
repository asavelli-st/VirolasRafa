namespace VirolasRafa.Services;

/// <summary>Cálculos de margen y precio. Todo el redondeo de importes pasa por <see cref="Redondear"/>.</summary>
public static class CalculosPrecio
{
    public static decimal Redondear(decimal importe) => Math.Round(importe, 2, MidpointRounding.AwayFromZero);

    /// <summary>Margen sobre costo en %: (Precio − Costo) / Costo × 100. Nulo si el costo es 0.</summary>
    public static decimal? Margen(decimal costo, decimal? precio) =>
        costo == 0 || precio is null ? null : Redondear((precio.Value - costo) / costo * 100);

    /// <summary>Precio = Costo × (1 + Margen / 100), redondeado a 2 decimales.</summary>
    public static decimal PrecioDesdeMargen(decimal costo, decimal margen) => Redondear(costo * (1 + margen / 100));

    /// <summary>
    /// Precio a guardar según el último campo que editó el Admin: si fue el margen (y hay margen), se calcula desde el costo;
    /// si no, se usa el precio cargado.
    /// </summary>
    public static decimal? PrecioSegunUltimoEditado(decimal costo, decimal? precio, decimal? margen, bool usarMargen) =>
        usarMargen && margen.HasValue ? PrecioDesdeMargen(costo, margen.Value) : precio;
}

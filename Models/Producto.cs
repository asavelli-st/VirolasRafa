using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirolasRafa.Services;

namespace VirolasRafa.Models;

/// <summary>
/// Producto de la lista de precios. <see cref="Codigo"/> es el identificador de la lista y es único.
/// No se borra: se desactiva con <see cref="Activo"/> (los presupuestos viejos lo referencian).
/// </summary>
public class Producto
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Codigo { get; set; } = "";

    [MaxLength(250)]
    public string Descripcion { get; set; } = "";

    public decimal Costo { get; set; }
    public decimal Precio { get; set; }

    public int? Cantidad2 { get; set; }
    public decimal? Precio2 { get; set; }
    public int? Cantidad3 { get; set; }
    public decimal? Precio3 { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>Margen sobre costo en %. No se guarda; es nulo si el costo es 0.</summary>
    [NotMapped]
    public decimal? Margen => CalculosPrecio.Margen(Costo, Precio);
}

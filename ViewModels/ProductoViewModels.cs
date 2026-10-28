using System.ComponentModel.DataAnnotations;
using VirolasRafa.Services;

namespace VirolasRafa.ViewModels;

// Hay dos juegos de ViewModels: los "Admin" llevan costo y margen; los "Vendedor" no tienen esas propiedades,
// así que esos datos no pueden llegar al navegador de un Vendedor por ningún camino.

/// <summary>Precio de un escalón: "desde N unidades, $ X".</summary>
public record EscalonVendedor(int Desde, decimal Precio);

public record EscalonAdmin(int Desde, decimal Precio, decimal? Margen);

public class ProductoVendedorViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Precio { get; set; }
    public int? Cantidad2 { get; set; }
    public decimal? Precio2 { get; set; }
    public int? Cantidad3 { get; set; }
    public decimal? Precio3 { get; set; }
    public bool Activo { get; set; }

    public IEnumerable<EscalonVendedor> Escalones()
    {
        yield return new(1, Precio);
        if (Cantidad2 is int c2 && Precio2 is decimal p2)
            yield return new(c2, p2);
        if (Cantidad3 is int c3 && Precio3 is decimal p3)
            yield return new(c3, p3);
    }
}

public class ProductoAdminViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public int? Cantidad2 { get; set; }
    public decimal? Precio2 { get; set; }
    public int? Cantidad3 { get; set; }
    public decimal? Precio3 { get; set; }
    public bool Activo { get; set; }

    public decimal? Margen => CalculosPrecio.Margen(Costo, Precio);
    public decimal? Margen2 => CalculosPrecio.Margen(Costo, Precio2);
    public decimal? Margen3 => CalculosPrecio.Margen(Costo, Precio3);

    public IEnumerable<EscalonAdmin> Escalones()
    {
        yield return new(1, Precio, Margen);
        if (Cantidad2 is int c2 && Precio2 is decimal p2)
            yield return new(c2, p2, Margen2);
        if (Cantidad3 is int c3 && Precio3 is decimal p3)
            yield return new(c3, p3, Margen3);
    }
}

/// <summary>Formulario de alta y edición (solo Admin).</summary>
public class ProductoFormViewModel
{
    /// <summary>Valor de los campos "Usar*": cuál de los dos campos editó el Admin por último.</summary>
    public const string UsarMargen = "margen";
    public const string UsarPrecio = "precio";

    public int Id { get; set; }

    [StringLength(50)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = "";

    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = "";

    [Display(Name = "Costo")]
    public decimal Costo { get; set; }

    [Display(Name = "Margen (%)")]
    public decimal? Margen { get; set; }

    [Display(Name = "Precio")]
    public decimal? Precio { get; set; }

    public string UsarBase { get; set; } = UsarPrecio;

    [Display(Name = "Desde (unidades)")]
    public int? Cantidad2 { get; set; }

    [Display(Name = "Margen (%)")]
    public decimal? Margen2 { get; set; }

    [Display(Name = "Precio")]
    public decimal? Precio2 { get; set; }

    public string Usar2 { get; set; } = UsarPrecio;

    [Display(Name = "Desde (unidades)")]
    public int? Cantidad3 { get; set; }

    [Display(Name = "Margen (%)")]
    public decimal? Margen3 { get; set; }

    [Display(Name = "Precio")]
    public decimal? Precio3 { get; set; }

    public string Usar3 { get; set; } = UsarPrecio;
}

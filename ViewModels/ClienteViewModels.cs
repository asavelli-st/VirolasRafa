using System.ComponentModel.DataAnnotations;

namespace VirolasRafa.ViewModels;

public class ClienteListaViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal DescuentoHabitualPct { get; set; }
    public bool Activo { get; set; }
}

public class ClienteFormViewModel
{
    public int Id { get; set; }

    [StringLength(150)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [StringLength(20)]
    [Display(Name = "CUIT / DNI")]
    public string? Documento { get; set; }

    [StringLength(50)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Range(0, 100)]
    [Display(Name = "Descuento habitual (%)")]
    public decimal DescuentoHabitualPct { get; set; }

    [StringLength(1000)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}

public class ClienteDetalleViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal DescuentoHabitualPct { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
}

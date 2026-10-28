using System.ComponentModel.DataAnnotations;

namespace VirolasRafa.Models;

/// <summary>Cliente. No se borra: se desactiva con <see cref="Activo"/>.</summary>
public class Cliente
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Nombre { get; set; } = "";

    /// <summary>CUIT o DNI, guardado solo con letras y números. Único cuando se carga.</summary>
    [MaxLength(20)]
    public string? Documento { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }

    /// <summary>0 a 100, decimal(5,2).</summary>
    public decimal DescuentoHabitualPct { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;
}

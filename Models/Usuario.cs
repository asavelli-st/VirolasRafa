using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VirolasRafa.Models;

/// <summary>Usuario del sistema. No se borra: se desactiva con <see cref="Activo"/>.</summary>
public class Usuario : IdentityUser
{
    [MaxLength(100)]
    public string Nombre { get; set; } = "";

    public bool Activo { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using VirolasRafa.Models;

namespace VirolasRafa.ViewModels;

public class UsuarioListaViewModel
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool Activo { get; set; }
    public bool Bloqueado { get; set; }
    public bool EsUsuarioActual { get; set; }
}

public class UsuarioCrearViewModel
{
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Rol")]
    public string Rol { get; set; } = Roles.Vendedor;

    [DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Repetir contraseña")]
    public string ConfirmarPassword { get; set; } = "";
}

public class UsuarioEditarViewModel
{
    public string Id { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Rol")]
    public string Rol { get; set; } = "";
}

public class ResetearPasswordViewModel
{
    public string Id { get; set; } = "";

    public string? Nombre { get; set; }

    [DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    [Display(Name = "Nueva contraseña")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Repetir contraseña")]
    public string ConfirmarPassword { get; set; } = "";
}

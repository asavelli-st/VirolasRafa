using System.ComponentModel.DataAnnotations;

namespace VirolasRafa.ViewModels;

public class LoginViewModel
{
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Display(Name = "Recordarme en este dispositivo")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}

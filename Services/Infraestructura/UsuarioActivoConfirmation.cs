using Microsoft.AspNetCore.Identity;
using VirolasRafa.Models;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>
/// Con <c>SignIn.RequireConfirmedAccount = true</c>, Identity solo deja entrar a los usuarios "confirmados".
/// Acá "confirmado" significa activo: los usuarios desactivados no pueden iniciar sesión.
/// </summary>
public class UsuarioActivoConfirmation : IUserConfirmation<Usuario>
{
    public Task<bool> IsConfirmedAsync(UserManager<Usuario> manager, Usuario user) => Task.FromResult(user.Activo);
}

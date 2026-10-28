using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirolasRafa.Models;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>Agrega el nombre del usuario a la cookie para mostrarlo en la barra superior.</summary>
public class UsuarioClaimsFactory(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<Usuario, IdentityRole>(userManager, roleManager, options)
{
    public const string ClaimNombre = "nombre";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identidad = await base.GenerateClaimsAsync(user);
        identidad.AddClaim(new Claim(ClaimNombre, user.Nombre));
        return identidad;
    }
}

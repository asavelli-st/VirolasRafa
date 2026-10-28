using Microsoft.AspNetCore.Identity;
using VirolasRafa.Models;

namespace VirolasRafa.Data;

/// <summary>
/// Crea los roles y, si todavía no hay ningún Admin, el usuario Admin inicial.
/// Email y contraseña salen de la configuración (User Secrets en desarrollo), nunca del código.
/// </summary>
public static class SeedInicial
{
    public static async Task EjecutarAsync(IServiceProvider services, IConfiguration configuracion, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        if ((await userManager.GetUsersInRoleAsync(Roles.Admin)).Count > 0)
            return;

        var email = configuracion["AdminInicial:Email"];
        var password = configuracion["AdminInicial:Password"];
        var nombre = configuracion["AdminInicial:Nombre"] ?? "Administrador";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No hay ningún Admin y faltan AdminInicial:Email / AdminInicial:Password en la configuración. No se creó el usuario inicial.");
            return;
        }

        var admin = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Nombre = nombre, Activo = true };
        var resultado = await userManager.CreateAsync(admin, password);
        if (resultado.Succeeded)
            resultado = await userManager.AddToRoleAsync(admin, Roles.Admin);

        if (resultado.Succeeded)
            logger.LogInformation("Usuario Admin inicial creado: {Email}", email);
        else
            logger.LogError("No se pudo crear el Admin inicial: {Errores}", string.Join(" ", resultado.Errors.Select(e => e.Description)));
    }
}

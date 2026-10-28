using Microsoft.AspNetCore.Identity;
using VirolasRafa.Models;

namespace VirolasRafa.Data;

/// <summary>
/// Crea los roles y, si todavía no hay ningún Admin, el usuario Admin inicial.
/// Email y contraseña salen de la configuración (User Secrets en desarrollo), nunca del código.
/// </summary>
public static class SeedInicial
{
    public static async Task EjecutarAsync(IServiceProvider services, IConfiguration configuracion, ILogger logger, bool esDesarrollo)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        await CrearAdminInicialAsync(userManager, configuracion, logger);
        if (esDesarrollo)
            await CrearUsuariosDePruebaAsync(userManager, configuracion, logger);
    }

    private static async Task CrearAdminInicialAsync(UserManager<Usuario> userManager, IConfiguration configuracion, ILogger logger)
    {
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

        await CrearAsync(userManager, logger, email, password, nombre, Roles.Admin, "Usuario Admin inicial");
    }

    /// <summary>
    /// Solo en Development: crea los usuarios de la sección <c>UsuariosPrueba</c> (Email, Password, Nombre, Rol) que todavía
    /// no existan. Sirven para probar la app sin usar las cuentas reales. Si ya existen, no se tocan.
    /// </summary>
    private static async Task CrearUsuariosDePruebaAsync(UserManager<Usuario> userManager, IConfiguration configuracion, ILogger logger)
    {
        foreach (var seccion in configuracion.GetSection("UsuariosPrueba").GetChildren())
        {
            var email = seccion["Email"];
            var password = seccion["Password"];
            var rol = seccion["Rol"] ?? Roles.Vendedor;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || !Roles.Todos.Contains(rol))
            {
                logger.LogWarning("UsuariosPrueba:{Clave} está incompleto o tiene un rol inválido; se ignora.", seccion.Key);
                continue;
            }
            if (await userManager.FindByEmailAsync(email) is not null)
                continue;

            await CrearAsync(userManager, logger, email, password, seccion["Nombre"] ?? email, rol, "Usuario de prueba");
        }
    }

    private static async Task CrearAsync(UserManager<Usuario> userManager, ILogger logger, string email, string password, string nombre, string rol, string descripcion)
    {
        var usuario = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Nombre = nombre, Activo = true };
        var resultado = await userManager.CreateAsync(usuario, password);
        if (resultado.Succeeded)
            resultado = await userManager.AddToRoleAsync(usuario, rol);

        if (resultado.Succeeded)
            logger.LogInformation("{Descripcion} creado: {Email} ({Rol})", descripcion, email, rol);
        else
            logger.LogError("No se pudo crear {Descripcion} {Email}: {Errores}", descripcion, email, string.Join(" ", resultado.Errors.Select(e => e.Description)));
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirolasRafa.Data;
using VirolasRafa.Models;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Services;

/// <summary>Alta, edición, activación y reseteo de contraseña de usuarios. Cada usuario tiene un único rol.</summary>
public class UsuariosService(UserManager<Usuario> userManager, AppDbContext db)
{
    public Task<List<UsuarioListaViewModel>> ListarAsync(string usuarioActualId)
    {
        var ahora = DateTimeOffset.UtcNow;
        return db.Users
            .OrderByDescending(u => u.Activo).ThenBy(u => u.Nombre)
            .Select(u => new UsuarioListaViewModel
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email ?? "",
                Rol = (from ur in db.UserRoles
                       join r in db.Roles on ur.RoleId equals r.Id
                       where ur.UserId == u.Id
                       select r.Name).FirstOrDefault() ?? "",
                Activo = u.Activo,
                Bloqueado = u.LockoutEnd != null && u.LockoutEnd > ahora,
                EsUsuarioActual = u.Id == usuarioActualId,
            })
            .ToListAsync();
    }

    public async Task<UsuarioEditarViewModel?> ObtenerParaEditarAsync(string id)
    {
        var usuario = await userManager.FindByIdAsync(id);
        if (usuario is null)
            return null;

        return new UsuarioEditarViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email ?? "",
            Rol = await RolDeAsync(usuario),
        };
    }

    public async Task<string?> ObtenerNombreAsync(string id) =>
        (await userManager.FindByIdAsync(id))?.Nombre;

    public async Task<Resultado> CrearAsync(UsuarioCrearViewModel vm)
    {
        if (!Roles.Todos.Contains(vm.Rol))
            return Resultado.Error("El rol elegido no es válido.");

        await using var transaccion = await db.Database.BeginTransactionAsync();

        var email = vm.Email.Trim();
        var usuario = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Nombre = vm.Nombre.Trim(), Activo = true };
        var resultado = await userManager.CreateAsync(usuario, vm.Password);
        if (resultado.Succeeded)
            resultado = await userManager.AddToRoleAsync(usuario, vm.Rol);
        if (!resultado.Succeeded)
            return Resultado.De(resultado);

        await transaccion.CommitAsync();
        return Resultado.Exito();
    }

    public async Task<Resultado> EditarAsync(UsuarioEditarViewModel vm, string usuarioActualId)
    {
        if (!Roles.Todos.Contains(vm.Rol))
            return Resultado.Error("El rol elegido no es válido.");

        var usuario = await userManager.FindByIdAsync(vm.Id);
        if (usuario is null)
            return Resultado.Error("El usuario no existe.");

        var rolActual = await RolDeAsync(usuario);
        var error = ReglasUsuarios.ValidarCambioDeRol(
            usuario.Id == usuarioActualId, usuario.Activo, rolActual, vm.Rol, await ContarAdminsActivosAsync());
        if (error is not null)
            return Resultado.Error(error);

        await using var transaccion = await db.Database.BeginTransactionAsync();

        var email = vm.Email.Trim();
        usuario.Nombre = vm.Nombre.Trim();
        usuario.Email = email;
        usuario.UserName = email;
        var resultado = await userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
            return Resultado.De(resultado);

        if (rolActual != vm.Rol)
        {
            if (rolActual != "")
                resultado = await userManager.RemoveFromRoleAsync(usuario, rolActual);
            if (resultado.Succeeded)
                resultado = await userManager.AddToRoleAsync(usuario, vm.Rol);
            if (!resultado.Succeeded)
                return Resultado.De(resultado);
        }

        await transaccion.CommitAsync();
        return Resultado.Exito();
    }

    public async Task<Resultado> CambiarActivoAsync(string id, bool activo, string usuarioActualId)
    {
        var usuario = await userManager.FindByIdAsync(id);
        if (usuario is null)
            return Resultado.Error("El usuario no existe.");
        if (usuario.Activo == activo)
            return Resultado.Exito();

        if (!activo)
        {
            var error = ReglasUsuarios.ValidarDesactivacion(
                usuario.Id == usuarioActualId, await RolDeAsync(usuario) == Roles.Admin, await ContarAdminsActivosAsync());
            if (error is not null)
                return Resultado.Error(error);
        }

        usuario.Activo = activo;
        var resultado = await userManager.UpdateAsync(usuario);
        // Cambiar el sello de seguridad invalida las sesiones abiertas del usuario desactivado.
        if (resultado.Succeeded && !activo)
            resultado = await userManager.UpdateSecurityStampAsync(usuario);
        return Resultado.De(resultado);
    }

    public async Task<Resultado> ResetearPasswordAsync(ResetearPasswordViewModel vm)
    {
        var usuario = await userManager.FindByIdAsync(vm.Id);
        if (usuario is null)
            return Resultado.Error("El usuario no existe.");

        var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await userManager.ResetPasswordAsync(usuario, token, vm.Password);
        if (!resultado.Succeeded)
            return Resultado.De(resultado);

        // Si estaba bloqueado por intentos fallidos, queda desbloqueado.
        await userManager.SetLockoutEndDateAsync(usuario, null);
        await userManager.ResetAccessFailedCountAsync(usuario);
        return Resultado.Exito();
    }

    private async Task<string> RolDeAsync(Usuario usuario) =>
        (await userManager.GetRolesAsync(usuario)).FirstOrDefault() ?? "";

    private Task<int> ContarAdminsActivosAsync() =>
        (from ur in db.UserRoles
         join r in db.Roles on ur.RoleId equals r.Id
         join u in db.Users on ur.UserId equals u.Id
         where r.Name == Roles.Admin && u.Activo
         select u.Id).CountAsync();
}

using VirolasRafa.Models;

namespace VirolasRafa.Services;

/// <summary>
/// Reglas puras para no dejar el sistema sin administradores.
/// Devuelven el mensaje de error, o null si la operación está permitida.
/// </summary>
public static class ReglasUsuarios
{
    public static string? ValidarDesactivacion(bool esUsuarioActual, bool esAdmin, int adminsActivos)
    {
        if (esUsuarioActual)
            return "No podés desactivar tu propio usuario.";
        if (esAdmin && adminsActivos <= 1)
            return "Debe quedar al menos un Admin activo.";
        return null;
    }

    public static string? ValidarCambioDeRol(bool esUsuarioActual, bool estaActivo, string rolActual, string rolNuevo, int adminsActivos)
    {
        if (rolActual != Roles.Admin || rolNuevo == Roles.Admin)
            return null;
        if (esUsuarioActual)
            return "No podés quitarte el rol Admin a vos mismo.";
        if (estaActivo && adminsActivos <= 1)
            return "Debe quedar al menos un Admin activo.";
        return null;
    }
}

using Microsoft.AspNetCore.Identity;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>Mensajes de error de Identity en español.</summary>
public class IdentityErrorDescriberEs : IdentityErrorDescriber
{
    private static IdentityError E(string codigo, string descripcion) => new() { Code = codigo, Description = descripcion };

    public override IdentityError DefaultError() => E(nameof(DefaultError), "Ocurrió un error inesperado.");
    public override IdentityError ConcurrencyFailure() => E(nameof(ConcurrencyFailure), "Otro usuario modificó estos datos al mismo tiempo. Volvé a intentarlo.");
    public override IdentityError PasswordMismatch() => E(nameof(PasswordMismatch), "La contraseña es incorrecta.");
    public override IdentityError InvalidToken() => E(nameof(InvalidToken), "El código de seguridad no es válido.");
    public override IdentityError RecoveryCodeRedemptionFailed() => E(nameof(RecoveryCodeRedemptionFailed), "El código de recuperación no es válido.");
    public override IdentityError LoginAlreadyAssociated() => E(nameof(LoginAlreadyAssociated), "Ya hay un usuario asociado a ese inicio de sesión.");
    public override IdentityError InvalidUserName(string? userName) => E(nameof(InvalidUserName), $"El nombre de usuario '{userName}' no es válido.");
    public override IdentityError InvalidEmail(string? email) => E(nameof(InvalidEmail), $"El email '{email}' no es válido.");
    public override IdentityError DuplicateUserName(string userName) => E(nameof(DuplicateUserName), $"Ya existe un usuario con el email '{userName}'.");
    public override IdentityError DuplicateEmail(string email) => E(nameof(DuplicateEmail), $"Ya existe un usuario con el email '{email}'.");
    public override IdentityError InvalidRoleName(string? role) => E(nameof(InvalidRoleName), $"El rol '{role}' no es válido.");
    public override IdentityError DuplicateRoleName(string role) => E(nameof(DuplicateRoleName), $"El rol '{role}' ya existe.");
    public override IdentityError UserAlreadyHasPassword() => E(nameof(UserAlreadyHasPassword), "El usuario ya tiene contraseña.");
    public override IdentityError UserLockoutNotEnabled() => E(nameof(UserLockoutNotEnabled), "El bloqueo no está habilitado para este usuario.");
    public override IdentityError UserAlreadyInRole(string role) => E(nameof(UserAlreadyInRole), $"El usuario ya tiene el rol '{role}'.");
    public override IdentityError UserNotInRole(string role) => E(nameof(UserNotInRole), $"El usuario no tiene el rol '{role}'.");
    public override IdentityError PasswordTooShort(int length) => E(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => E(nameof(PasswordRequiresUniqueChars), $"La contraseña debe tener al menos {uniqueChars} caracteres distintos.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => E(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe tener al menos un símbolo (por ejemplo ! o #).");
    public override IdentityError PasswordRequiresDigit() => E(nameof(PasswordRequiresDigit), "La contraseña debe tener al menos un número.");
    public override IdentityError PasswordRequiresLower() => E(nameof(PasswordRequiresLower), "La contraseña debe tener al menos una minúscula.");
    public override IdentityError PasswordRequiresUpper() => E(nameof(PasswordRequiresUpper), "La contraseña debe tener al menos una mayúscula.");
}

using Microsoft.AspNetCore.Identity;

namespace VirolasRafa.Services;

/// <summary>Resultado de una operación de negocio: éxito o lista de errores para mostrar al usuario.</summary>
public sealed class Resultado
{
    private Resultado(IReadOnlyList<string> errores) => Errores = errores;

    public IReadOnlyList<string> Errores { get; }

    public bool Ok => Errores.Count == 0;

    public static Resultado Exito() => new([]);

    public static Resultado Error(params string[] errores) => new(errores);

    public static Resultado De(IdentityResult resultado) =>
        resultado.Succeeded ? Exito() : new(resultado.Errors.Select(e => e.Description).ToList());
}

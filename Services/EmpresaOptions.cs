namespace VirolasRafa.Services;

/// <summary>Datos de la empresa (sección <c>Empresa</c> de appsettings.json) para encabezados y PDF.</summary>
public class EmpresaOptions
{
    public const string Seccion = "Empresa";

    public string Nombre { get; set; } = "[NOMBRE DE LA EMPRESA]";
    public string Domicilio { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Logo { get; set; }
}

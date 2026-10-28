using VirolasRafa.Services.Importacion;

namespace VirolasRafa.ViewModels;

/// <summary>Vista previa de una importación, antes de que el Admin confirme.</summary>
public class VistaPreviaImportacionViewModel
{
    public string? NombreArchivo { get; init; }

    /// <summary>Identifica la vista previa guardada en el servidor; se manda al confirmar.</summary>
    public string? Token { get; init; }

    public string? ErrorGeneral { get; init; }
    public IReadOnlyList<FilaClasificada> Nuevos { get; init; } = [];
    public IReadOnlyList<FilaClasificada> Actualizados { get; init; } = [];
    public int SinCambios { get; init; }
    public IReadOnlyList<FilaConError> Errores { get; init; } = [];

    public bool HayCambios => Nuevos.Count + Actualizados.Count > 0;
}

public record ResumenImportacion(int Creados, int Actualizados, int Reactivados);

using VirolasRafa.Services;

namespace VirolasRafa.ViewModels;

/// <summary>Valores del filtro de estado de los listados.</summary>
public static class EstadoListado
{
    public const string Activos = "activos";
    public const string Inactivos = "inactivos";
    public const string Todos = "todos";
}

/// <summary>Listado paginado con búsqueda y filtro de estado.</summary>
public class ListadoViewModel<T>
{
    public required Pagina<T> Pagina { get; init; }
    public string? Buscar { get; init; }
    public string Estado { get; init; } = EstadoListado.Activos;

    public FiltrosListado Filtros(string placeholder, bool conEstado = true) =>
        new(Buscar, conEstado ? Estado : null, placeholder, Pagina.Numero, Pagina.TotalPaginas, Pagina.Total);
}

/// <summary>Datos para los parciales _Buscador y _Paginacion. <see cref="Estado"/> nulo: sin filtro de estado.</summary>
public record FiltrosListado(string? Buscar, string? Estado, string Placeholder, int Pagina, int TotalPaginas, int Total);

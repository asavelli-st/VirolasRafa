using Microsoft.EntityFrameworkCore;

namespace VirolasRafa.Services;

/// <summary>Una página de resultados de un listado.</summary>
public class Pagina<T>
{
    public const int TamanioPorDefecto = 50;

    public required IReadOnlyList<T> Items { get; init; }
    public int Numero { get; init; }
    public int Tamanio { get; init; }
    public int Total { get; init; }

    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)Tamanio));

    public static async Task<Pagina<T>> CrearAsync(IQueryable<T> consulta, int numero, int tamanio = TamanioPorDefecto)
    {
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanio));
        numero = Math.Clamp(numero, 1, totalPaginas);
        var items = await consulta.Skip((numero - 1) * tamanio).Take(tamanio).ToListAsync();
        return new Pagina<T> { Items = items, Numero = numero, Tamanio = tamanio, Total = total };
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace VirolasRafa.Services.Infraestructura;

public static class ErroresDb
{
    /// <summary>True si el error es por un índice único (por ejemplo, dos altas simultáneas con el mismo código).</summary>
    public static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirolasRafa.Models;

namespace VirolasRafa.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Usuario>(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Importes: decimal(18,2). Los porcentajes se configuran como decimal(5,2) en cada propiedad.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}

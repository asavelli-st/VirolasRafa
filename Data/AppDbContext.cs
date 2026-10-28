using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirolasRafa.Models;

namespace VirolasRafa.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Usuario>(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Importes: decimal(18,2). Los porcentajes se configuran como decimal(5,2) en cada propiedad.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>(e =>
        {
            e.Property(c => c.DescuentoHabitualPct).HasPrecision(5, 2);
            e.HasIndex(c => c.Documento).IsUnique().HasFilter("[Documento] IS NOT NULL");
            e.HasIndex(c => c.Nombre);
        });

        builder.Entity<Producto>(e =>
        {
            e.HasIndex(p => p.Codigo).IsUnique();
            e.HasIndex(p => p.Descripcion);
        });
    }
}

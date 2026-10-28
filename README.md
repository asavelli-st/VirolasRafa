# VirolasRafa

Presupuestos, órdenes de armado, cuenta corriente y cobranzas. Ver [SPEC.md](SPEC.md).

## Puesta en marcha (desarrollo)

Requisitos: .NET SDK 10 y SQL Server (desarrollo: SQL Express `.\SQLEXPRESS2025`, base `VirolasRafa`,
login SQL `virolasrafa_app` con `db_owner` sobre esa base).

El connection string completo va en User Secrets (en `appsettings.json` queda sin contraseña):

```bash
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:Default" "Server=.\SQLEXPRESS2025;Database=VirolasRafa;User Id=virolasrafa_app;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "AdminInicial:Email" "admin@ejemplo.com"
dotnet user-secrets set "AdminInicial:Password" "UnaClaveSegura1"
dotnet run --launch-profile http
```

En Development, al arrancar se aplican las migraciones y se crea el Admin inicial con los datos de User Secrets
(solo si todavía no existe ningún Admin). Fuera de Development, aplicar las migraciones con `dotnet ef database update`.

- Usuarios de prueba (solo Development): los de la sección `UsuariosPrueba` de User Secrets se crean al arrancar si no existen,
  por ejemplo `dotnet user-secrets set "UsuariosPrueba:0:Email" "prueba@ejemplo.com"` más `:Password`, `:Nombre` y `:Rol` (Admin o Vendedor).
- Librerías de front (Bootstrap, Font Awesome, jQuery Validation): `libman.json`, se restauran con `dotnet libman restore`.
- Migraciones: `dotnet ef migrations add <Nombre> -o Data/Migrations`.
- Tests: `dotnet test VirolasRafa.slnx`.
- Datos de la empresa para encabezados y PDF: sección `Empresa` de `appsettings.json`.

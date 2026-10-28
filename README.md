# VirolasRafa

Presupuestos, órdenes de armado, cuenta corriente y cobranzas. Ver [SPEC.md](SPEC.md).

## Puesta en marcha (desarrollo)

Requisitos: .NET SDK 10 y SQL Server LocalDB.

```bash
dotnet tool restore
dotnet user-secrets set "AdminInicial:Email" "admin@ejemplo.com"
dotnet user-secrets set "AdminInicial:Password" "UnaClaveSegura1"
dotnet run --launch-profile http
```

En Development, al arrancar se aplican las migraciones y se crea el Admin inicial con los datos de User Secrets
(solo si todavía no existe ningún Admin). Fuera de Development, aplicar las migraciones con `dotnet ef database update`.

- Librerías de front (Bootstrap, Font Awesome, jQuery Validation): `libman.json`, se restauran con `dotnet libman restore`.
- Migraciones: `dotnet ef migrations add <Nombre> -o Data/Migrations`.
- Tests: `dotnet test VirolasRafa.slnx`.
- Datos de la empresa para encabezados y PDF: sección `Empresa` de `appsettings.json`.

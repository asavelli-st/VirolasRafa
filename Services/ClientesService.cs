using Microsoft.EntityFrameworkCore;
using VirolasRafa.Data;
using VirolasRafa.Models;
using VirolasRafa.Services.Infraestructura;
using VirolasRafa.ViewModels;

namespace VirolasRafa.Services;

public class ClientesService(AppDbContext db)
{
    public Task<Pagina<ClienteListaViewModel>> ListarAsync(string? buscar, string estado, int pagina)
    {
        var consulta = FiltrarPorEstado(db.Clientes.AsNoTracking(), estado);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            var documento = NormalizarDocumento(texto) ?? texto;
            consulta = consulta.Where(c => c.Nombre.Contains(texto)
                || (c.Documento != null && c.Documento.Contains(documento))
                || (c.Telefono != null && c.Telefono.Contains(texto))
                || (c.Email != null && c.Email.Contains(texto)));
        }

        return Pagina<ClienteListaViewModel>.CrearAsync(
            consulta.OrderBy(c => c.Nombre).Select(c => new ClienteListaViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Documento = c.Documento,
                Telefono = c.Telefono,
                Email = c.Email,
                DescuentoHabitualPct = c.DescuentoHabitualPct,
                Activo = c.Activo,
            }),
            pagina);
    }

    public Task<ClienteDetalleViewModel?> ObtenerDetalleAsync(int id) =>
        db.Clientes.AsNoTracking().Where(c => c.Id == id).Select(c => new ClienteDetalleViewModel
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Documento = c.Documento,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            DescuentoHabitualPct = c.DescuentoHabitualPct,
            Observaciones = c.Observaciones,
            Activo = c.Activo,
        }).FirstOrDefaultAsync();

    public Task<ClienteFormViewModel?> ObtenerParaEditarAsync(int id) =>
        db.Clientes.AsNoTracking().Where(c => c.Id == id).Select(c => new ClienteFormViewModel
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Documento = c.Documento,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            DescuentoHabitualPct = c.DescuentoHabitualPct,
            Observaciones = c.Observaciones,
        }).FirstOrDefaultAsync();

    public async Task<(Resultado Resultado, int Id)> CrearAsync(ClienteFormViewModel vm)
    {
        var cliente = new Cliente { Activo = true };
        var resultado = await GuardarAsync(cliente, vm);
        return (resultado, cliente.Id);
    }

    public async Task<Resultado> EditarAsync(ClienteFormViewModel vm)
    {
        var cliente = await db.Clientes.FindAsync(vm.Id);
        return cliente is null ? Resultado.Error("El cliente no existe.") : await GuardarAsync(cliente, vm);
    }

    public async Task<Resultado> CambiarActivoAsync(int id, bool activo)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente is null)
            return Resultado.Error("El cliente no existe.");
        cliente.Activo = activo;
        await db.SaveChangesAsync();
        return Resultado.Exito();
    }

    /// <summary>Deja solo letras y números ("20-12345678-9" → "20123456789"); vacío pasa a null.</summary>
    public static string? NormalizarDocumento(string? documento)
    {
        var limpio = new string((documento ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return limpio.Length == 0 ? null : limpio;
    }

    private async Task<Resultado> GuardarAsync(Cliente cliente, ClienteFormViewModel vm)
    {
        if (Numeros.TieneMasDecimales(vm.DescuentoHabitualPct, 2))
            return Resultado.Error("El descuento habitual admite hasta 2 decimales.");

        var documento = NormalizarDocumento(vm.Documento);
        if (documento is not null)
        {
            var otro = await db.Clientes.Where(c => c.Documento == documento && c.Id != cliente.Id)
                .Select(c => c.Nombre).FirstOrDefaultAsync();
            if (otro is not null)
                return Resultado.Error($"Ya existe un cliente con ese CUIT / DNI: {otro}.");
        }

        cliente.Nombre = vm.Nombre.Trim();
        cliente.Documento = documento;
        cliente.Telefono = Limpiar(vm.Telefono);
        cliente.Email = Limpiar(vm.Email);
        cliente.Direccion = Limpiar(vm.Direccion);
        cliente.DescuentoHabitualPct = vm.DescuentoHabitualPct;
        cliente.Observaciones = Limpiar(vm.Observaciones);

        if (cliente.Id == 0)
            db.Clientes.Add(cliente);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ErroresDb.EsDuplicado(ex))
        {
            return Resultado.Error("Ya existe un cliente con ese CUIT / DNI.");
        }
        return Resultado.Exito();
    }

    private static IQueryable<Cliente> FiltrarPorEstado(IQueryable<Cliente> consulta, string estado) => estado switch
    {
        EstadoListado.Inactivos => consulta.Where(c => !c.Activo),
        EstadoListado.Todos => consulta,
        _ => consulta.Where(c => c.Activo),
    };

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

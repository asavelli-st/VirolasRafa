using VirolasRafa.Models;
using VirolasRafa.Services;

namespace VirolasRafa.Tests;

public class ReglasUsuariosTests
{
    [Fact]
    public void NoSePuedeDesactivarElPropioUsuario() =>
        Assert.NotNull(ReglasUsuarios.ValidarDesactivacion(esUsuarioActual: true, esAdmin: false, adminsActivos: 3));

    [Fact]
    public void NoSePuedeDesactivarAlUltimoAdminActivo() =>
        Assert.NotNull(ReglasUsuarios.ValidarDesactivacion(esUsuarioActual: false, esAdmin: true, adminsActivos: 1));

    [Fact]
    public void SePuedeDesactivarUnAdminSiQuedanOtros() =>
        Assert.Null(ReglasUsuarios.ValidarDesactivacion(esUsuarioActual: false, esAdmin: true, adminsActivos: 2));

    [Fact]
    public void SePuedeDesactivarUnVendedorAunqueHayaUnSoloAdmin() =>
        Assert.Null(ReglasUsuarios.ValidarDesactivacion(esUsuarioActual: false, esAdmin: false, adminsActivos: 1));

    [Fact]
    public void NoSePuedeQuitarseAUnoMismoElRolAdmin() =>
        Assert.NotNull(ReglasUsuarios.ValidarCambioDeRol(true, true, Roles.Admin, Roles.Vendedor, adminsActivos: 3));

    [Fact]
    public void NoSePuedeQuitarElRolAlUltimoAdminActivo() =>
        Assert.NotNull(ReglasUsuarios.ValidarCambioDeRol(false, true, Roles.Admin, Roles.Vendedor, adminsActivos: 1));

    [Fact]
    public void SePuedeQuitarElRolAUnAdminInactivo() =>
        Assert.Null(ReglasUsuarios.ValidarCambioDeRol(false, false, Roles.Admin, Roles.Vendedor, adminsActivos: 1));

    [Theory]
    [InlineData(Roles.Vendedor, Roles.Admin)]
    [InlineData(Roles.Admin, Roles.Admin)]
    [InlineData(Roles.Vendedor, Roles.Vendedor)]
    public void LosCambiosQueNoQuitanAdminSiempreSePermiten(string rolActual, string rolNuevo) =>
        Assert.Null(ReglasUsuarios.ValidarCambioDeRol(true, true, rolActual, rolNuevo, adminsActivos: 1));
}

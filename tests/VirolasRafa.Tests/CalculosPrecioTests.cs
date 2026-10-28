using VirolasRafa.Services;

namespace VirolasRafa.Tests;

public class CalculosPrecioTests
{
    [Theory]
    [InlineData(40, 70, 75)]
    [InlineData(100, 100, 0)]
    [InlineData(3, 4, 33.33)]
    [InlineData(100, 80, -20)]
    public void MargenSobreCosto(decimal costo, decimal precio, decimal esperado) =>
        Assert.Equal(esperado, CalculosPrecio.Margen(costo, precio));

    [Fact]
    public void ConCostoCeroElMargenEsNulo() => Assert.Null(CalculosPrecio.Margen(0, 100));

    [Fact]
    public void SinPrecioElMargenEsNulo() => Assert.Null(CalculosPrecio.Margen(10, null));

    [Theory]
    [InlineData(40, 75, 70)]
    [InlineData(25, 90, 47.5)]
    [InlineData(3.33, 33.33, 4.44)]   // 4,439889 → 4,44
    [InlineData(0.01, 50, 0.02)]      // 0,015 → 0,02 (redondeo hacia arriba en el medio)
    public void PrecioDesdeMargenRedondeaA2Decimales(decimal costo, decimal margen, decimal esperado) =>
        Assert.Equal(esperado, CalculosPrecio.PrecioDesdeMargen(costo, margen));

    [Fact]
    public void SiElUltimoEditadoFueElMargenSeCalculaElPrecio() =>
        Assert.Equal(70m, CalculosPrecio.PrecioSegunUltimoEditado(40, precio: 999, margen: 75, usarMargen: true));

    [Fact]
    public void SiElUltimoEditadoFueElPrecioSeUsaElPrecio() =>
        Assert.Equal(999m, CalculosPrecio.PrecioSegunUltimoEditado(40, precio: 999, margen: 75, usarMargen: false));

    [Fact]
    public void SiPideMargenPeroNoHayMargenSeUsaElPrecio() =>
        Assert.Equal(80m, CalculosPrecio.PrecioSegunUltimoEditado(40, precio: 80, margen: null, usarMargen: true));
}

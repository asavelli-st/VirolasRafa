using VirolasRafa.Services;

namespace VirolasRafa.Tests;

public class ReglasProductoTests
{
    private static List<string> Validar(decimal costo = 40, decimal precio = 70, int? c2 = null, decimal? p2 = null, int? c3 = null, decimal? p3 = null) =>
        ReglasProducto.Validar(costo, precio, c2, p2, c3, p3);

    [Fact]
    public void ProductoSinEscalonesEsValido() => Assert.Empty(Validar());

    [Fact]
    public void ProductoConDosEscalonesEsValido() => Assert.Empty(Validar(c2: 100, p2: 62, c3: 500, p3: 55));

    [Fact]
    public void CostoCeroEsValido() => Assert.Empty(Validar(costo: 0));

    [Fact]
    public void CostoNegativoNoEsValido() => Assert.NotEmpty(Validar(costo: -1));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void PrecioTieneQueSerMayorQueCero(decimal precio) => Assert.NotEmpty(Validar(precio: precio));

    [Fact]
    public void CantidadSinPrecioNoEsValido() => Assert.NotEmpty(Validar(c2: 10));

    [Fact]
    public void PrecioSinCantidadNoEsValido() => Assert.NotEmpty(Validar(p2: 60));

    [Fact]
    public void EscalonTresSinEscalonDosNoEsValido() => Assert.NotEmpty(Validar(c3: 500, p3: 55));

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void Cantidad2TieneQueSerMayorQueUno(int c2) => Assert.NotEmpty(Validar(c2: c2, p2: 60));

    [Fact]
    public void Cantidad2IgualADosEsValida() => Assert.Empty(Validar(c2: 2, p2: 60));

    [Theory]
    [InlineData(100)]
    [InlineData(50)]
    public void Cantidad3TieneQueSerMayorQueCantidad2(int c3) => Assert.NotEmpty(Validar(c2: 100, p2: 62, c3: c3, p3: 55));

    [Theory]
    [InlineData(70)]
    [InlineData(75)]
    public void Precio2TieneQueSerMenorQueElPrecio(decimal p2) => Assert.NotEmpty(Validar(c2: 100, p2: p2));

    [Theory]
    [InlineData(62)]
    [InlineData(65)]
    public void Precio3TieneQueSerMenorQuePrecio2(decimal p3) => Assert.NotEmpty(Validar(c2: 100, p2: 62, c3: 500, p3: p3));
}

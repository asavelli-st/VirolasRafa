using VirolasRafa.Services;

namespace VirolasRafa.Tests;

public class NumerosTests
{
    [Theory]
    [InlineData("1234,56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1.234.567", 1234567)]
    [InlineData("$ 1.234,5", 1234.5)]
    [InlineData("15 %", 15)]
    [InlineData("-3,5", -3.5)]
    [InlineData("70", 70)]
    [InlineData(",5", 0.5)]
    public void LeeFormatoArgentinoYPuntoDecimal(string texto, decimal esperado)
    {
        Assert.True(Numeros.TryParse(texto, out var valor));
        Assert.Equal(esperado, valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("12a")]
    [InlineData("1,2,3")]
    public void RechazaLoQueNoEsNumero(string? texto) => Assert.False(Numeros.TryParse(texto, out _));

    [Theory]
    [InlineData(1.5, false)]
    [InlineData(1.25, false)]
    [InlineData(1.255, true)]
    public void DetectaMasDeDosDecimales(decimal valor, bool esperado) => Assert.Equal(esperado, Numeros.TieneMasDecimales(valor, 2));
}

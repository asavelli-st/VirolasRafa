using VirolasRafa.Services;

namespace VirolasRafa.Tests;

public class ClientesTests
{
    [Theory]
    [InlineData("20-12345678-9", "20123456789")]
    [InlineData(" 20.123.456 ", "20123456")]
    [InlineData("ab 123", "AB123")]
    public void NormalizaElDocumento(string documento, string esperado) =>
        Assert.Equal(esperado, ClientesService.NormalizarDocumento(documento));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" - . ")]
    public void DocumentoVacioQuedaNulo(string? documento) => Assert.Null(ClientesService.NormalizarDocumento(documento));
}

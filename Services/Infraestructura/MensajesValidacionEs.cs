using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>
/// Mensajes de validación en español para los atributos de DataAnnotations que no traen uno propio,
/// incluido el [Required] implícito de las propiedades no anulables. Aplica al servidor y al cliente.
/// </summary>
public class MensajesValidacionEs : IValidationMetadataProvider
{
    // Estos atributos traen su mensaje en inglés ya cargado en ErrorMessage.
    private static readonly string? EmailPorDefecto = new EmailAddressAttribute().ErrorMessage;
    private static readonly string? TelefonoPorDefecto = new PhoneAttribute().ErrorMessage;

    private static bool TieneMensajePropio(ValidationAttribute atributo) =>
        atributo.ErrorMessageResourceType is not null || atributo switch
        {
            EmailAddressAttribute => atributo.ErrorMessage != EmailPorDefecto,
            PhoneAttribute => atributo.ErrorMessage != TelefonoPorDefecto,
            _ => atributo.ErrorMessage is not null,
        };

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (TieneMensajePropio(atributo))
                continue;

            var mensaje = atributo switch
            {
                RequiredAttribute => "El campo {0} es obligatorio.",
                StringLengthAttribute { MinimumLength: > 0 } => "{0} debe tener entre {2} y {1} caracteres.",
                StringLengthAttribute => "{0} no puede tener más de {1} caracteres.",
                MaxLengthAttribute => "{0} no puede tener más de {1} caracteres.",
                MinLengthAttribute => "{0} debe tener al menos {1} caracteres.",
                RangeAttribute => "{0} debe estar entre {1} y {2}.",
                EmailAddressAttribute => "{0} no es un email válido.",
                PhoneAttribute => "{0} no es un teléfono válido.",
                CompareAttribute => "{0} y {1} no coinciden.",
                RegularExpressionAttribute => "{0} no tiene un formato válido.",
                _ => null,
            };
            if (mensaje is not null)
                atributo.ErrorMessage = mensaje;
        }
    }

    /// <summary>Mensajes del model binding (por ejemplo, texto en un campo numérico).</summary>
    public static void Configurar(MvcOptions opciones)
    {
        var m = opciones.ModelBindingMessageProvider;
        m.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor de {campo}.");
        m.SetMissingKeyOrValueAccessor(() => "Falta un valor obligatorio.");
        m.SetMissingRequestBodyRequiredValueAccessor(() => "Faltan los datos de la solicitud.");
        m.SetValueMustNotBeNullAccessor(campo => $"El campo {campo} es obligatorio.");
        m.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
        m.SetUnknownValueIsInvalidAccessor(campo => $"El valor de {campo} no es válido.");
        m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor no es válido.");
        m.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
        m.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
        m.SetNonPropertyValueMustBeANumberAccessor(() => "El campo debe ser un número.");

        opciones.ModelMetadataDetailsProviders.Add(new MensajesValidacionEs());
    }
}

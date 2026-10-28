using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VirolasRafa.Services.Infraestructura;

/// <summary>
/// Lee los decimales de los formularios con <see cref="Numeros.TryParse"/>: acepta "1.234,56" y también "1234.56"
/// (el teclado del celular suele mandar punto). Con el binder estándar en es-AR, "1234.56" se leería como 123456.
/// </summary>
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valor = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valor == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valor);
        var texto = valor.FirstValue;
        var metadata = bindingContext.ModelMetadata;
        var mensajes = metadata.ModelBindingMessageProvider;

        if (string.IsNullOrWhiteSpace(texto))
        {
            if (metadata.IsReferenceOrNullableType)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, mensajes.ValueMustNotBeNullAccessor(metadata.GetDisplayName()));
            return Task.CompletedTask;
        }

        if (Numeros.TryParse(texto, out var numero))
            bindingContext.Result = ModelBindingResult.Success(numero);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, mensajes.AttemptedValueIsInvalidAccessor(texto, metadata.GetDisplayName()));
        return Task.CompletedTask;
    }
}

public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.UnderlyingOrModelType == typeof(decimal) ? new DecimalModelBinder() : null;
}

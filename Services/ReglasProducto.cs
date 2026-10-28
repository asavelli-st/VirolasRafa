namespace VirolasRafa.Services;

/// <summary>Validaciones de precios y escalones de un producto. Las usan el formulario y la importación.</summary>
public static class ReglasProducto
{
    public static List<string> Validar(decimal costo, decimal precio, int? cantidad2, decimal? precio2, int? cantidad3, decimal? precio3)
    {
        var errores = new List<string>();

        if (costo < 0)
            errores.Add("El costo no puede ser negativo.");
        if (precio <= 0)
            errores.Add("El precio tiene que ser mayor que 0.");

        if (cantidad2.HasValue != precio2.HasValue)
            errores.Add("En el escalón 2 hay que cargar la cantidad y el precio, o ninguno de los dos.");
        if (cantidad3.HasValue != precio3.HasValue)
            errores.Add("En el escalón 3 hay que cargar la cantidad y el precio, o ninguno de los dos.");
        if ((cantidad3.HasValue || precio3.HasValue) && !cantidad2.HasValue && !precio2.HasValue)
            errores.Add("No puede haber escalón 3 sin escalón 2.");

        if (cantidad2 <= 1)
            errores.Add("La cantidad del escalón 2 tiene que ser mayor que 1.");
        if (cantidad2.HasValue && cantidad3 <= cantidad2)
            errores.Add("La cantidad del escalón 3 tiene que ser mayor que la del escalón 2.");

        if (precio2 <= 0)
            errores.Add("El precio del escalón 2 tiene que ser mayor que 0.");
        else if (precio2 >= precio)
            errores.Add("El precio del escalón 2 tiene que ser menor que el precio base.");
        if (precio3 <= 0)
            errores.Add("El precio del escalón 3 tiene que ser mayor que 0.");
        else if (precio2.HasValue && precio3 >= precio2)
            errores.Add("El precio del escalón 3 tiene que ser menor que el del escalón 2.");

        return errores;
    }
}

namespace VirolasRafa.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendedor = "Vendedor";

    public static readonly IReadOnlyList<string> Todos = [Admin, Vendedor];
}

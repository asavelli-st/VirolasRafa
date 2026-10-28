using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirolasRafa.Data;
using VirolasRafa.Models;
using VirolasRafa.Services;
using VirolasRafa.Services.Importacion;
using VirolasRafa.Services.Infraestructura;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentity<Usuario, IdentityRole>(o =>
    {
        // Lo resuelve UsuarioActivoConfirmation: solo inician sesión los usuarios activos.
        o.SignIn.RequireConfirmedAccount = true;
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 8;
        o.Password.RequireNonAlphanumeric = false;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<IdentityErrorDescriberEs>()
    .AddClaimsPrincipalFactory<UsuarioClaimsFactory>();
builder.Services.AddScoped<IUserConfirmation<Usuario>, UsuarioActivoConfirmation>();

// Revalida la cookie cada minuto: un usuario desactivado o con otro rol lo nota enseguida.
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(1));

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Acceso/Login";
    o.LogoutPath = "/Acceso/Salir";
    o.AccessDeniedPath = "/Acceso/Denegado";
    o.Cookie.Name = "VirolasRafa.Auth";
});

// Todo requiere login salvo lo marcado con [AllowAnonymous].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    o.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    MensajesValidacionEs.Configurar(o);
});

builder.Services.AddMemoryCache();
builder.Services.Configure<EmpresaOptions>(builder.Configuration.GetSection(EmpresaOptions.Seccion));
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<ClientesService>();
builder.Services.AddScoped<ProductosService>();
builder.Services.AddScoped<ImportacionProductosService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (app.Environment.IsDevelopment())
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await SeedInicial.EjecutarAsync(scope.ServiceProvider, app.Configuration, logger, app.Environment.IsDevelopment());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

app.UseHttpsRedirection();

var esAR = (CultureInfo)CultureInfo.GetCultureInfo("es-AR").Clone();
esAR.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(esAR),
    SupportedCultures = [esAR],
    SupportedUICultures = [esAR],
    RequestCultureProviders = [],
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

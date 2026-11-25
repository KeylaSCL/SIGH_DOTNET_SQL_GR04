using Ec.Edu.Sigh.Gr04;
using Ec.Edu.Sigh.Gr04.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MVC con protección antifalsificación (CSRF) en todos los POST.
builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Configuración modificable desde appsettings.json
builder.Services.Configure<SighoOptions>(builder.Configuration.GetSection("Sigho"));

// Acceso a datos (equivalente a ConexionBD + DAO del proyecto Java)
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Sesión de usuario con cookie (equivale a HttpSession "usuarioAutenticado")
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.Cookie.Name = "SIGHO.Auth";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opciones.LoginPath = "/login";
        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(30); // igual que session-timeout de web.xml
        opciones.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/inicio"));
app.MapControllers();

app.Run();

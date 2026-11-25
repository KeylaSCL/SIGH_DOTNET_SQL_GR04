using System.Security.Claims;
using Ec.Edu.Sigh.Gr04.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Ec.Edu.Sigh.Gr04.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Ec.Edu.Sigh.Gr04.Controllers;

[AllowAnonymous]
public class LoginController : Controller
{
    private readonly IUsuarioRepository _usuarios;
    private readonly SighoOptions _opciones;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IUsuarioRepository usuarios,
                           IOptions<SighoOptions> opciones,
                           ILogger<LoginController> logger)
    {
        _usuarios = usuarios;
        _opciones = opciones.Value;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Inicio", "Home");

        return View(new LoginViewModel());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Index(LoginViewModel modelo)
    {
        // Los mensajes "Ingrese su usuario/contraseña" vienen de [Required].
        if (!ModelState.IsValid)
            return View(new LoginViewModel { Codigo = modelo.Codigo });

        try
        {
            Usuario usuario = await _usuarios.BuscarPorCodigoAsync(modelo.Codigo.Trim());

            bool valido = usuario != null
                && PasswordUtil.Verificar(modelo.Clave, usuario.ContrasenaAlmacenada)
                && usuario.CodigoEstado == _opciones.EstadoActivo;

            if (!valido)
            {
                ModelState.AddModelError(string.Empty,
                    "Credenciales incorrectas o cuenta no habilitada.");
                return View(new LoginViewModel { Codigo = modelo.Codigo });
            }

            var identidad = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, usuario.Codigo) },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            return RedirectToAction("Inicio", "Home");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error al consultar el usuario");
            ModelState.AddModelError(string.Empty, "No se pudo procesar el inicio de sesión.");
            return View(new LoginViewModel { Codigo = modelo.Codigo });
        }
    }

    [Authorize]
    [HttpPost("login/salir")]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
    }
}

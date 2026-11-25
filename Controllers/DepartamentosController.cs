using Ec.Edu.Sigh.Gr04.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Controllers;

[Authorize]
[Route("departamentos")]
public class DepartamentosController : Controller
{
    private const int LargoCodigo = 3;
    private const int LargoDescripcion = 50;

    private readonly IDepartamentoRepository _repo;
    private readonly ILogger<DepartamentosController> _logger;

    public DepartamentosController(IDepartamentoRepository repo,
                                   ILogger<DepartamentosController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ---------------- CONSULTAR ----------------
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await ConstruirAsync());

    // ---------------- REGISTRAR ----------------
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(string codigo, string descripcion)
    {
        codigo = LimpiarCodigo(codigo);
        descripcion = LimpiarTexto(descripcion);
        var nuevo = new Departamento(codigo, descripcion);

        if (!Validar(codigo, descripcion))
            return View("Index", await ConstruirAsync(nuevo: nuevo));

        try
        {
            if (await _repo.ExisteAsync(codigo))
            {
                Error($"Ya existe un departamento con el código {codigo}.");
                return View("Index", await ConstruirAsync(nuevo: nuevo));
            }

            await _repo.InsertarAsync(nuevo);
            TempData["Ok"] = $"Departamento {codigo} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al registrar departamento {Codigo}", codigo);
            if (ex.Number is 2627 or 2601)
                Error($"Ya existe un departamento con el código {codigo}.");
            else
                Error(MensajeSql(ex, "registrar", "INSERT"));
            return View("Index", await ConstruirAsync(nuevo: nuevo));
        }
    }

    // ---------------- MODIFICAR ----------------
    [HttpPost("actualizar")]
    public async Task<IActionResult> Actualizar(string codigo, string descripcion)
    {
        codigo = LimpiarCodigo(codigo);
        descripcion = LimpiarTexto(descripcion);
        var edicion = new Departamento(codigo, descripcion);

        if (!Validar(codigo, descripcion))
            return View("Index", await ConstruirAsync(edicion: edicion));

        try
        {
            int filas = await _repo.ActualizarAsync(edicion);
            if (filas == 0)
            {
                Error("No se encontró el departamento. Actualice el listado y vuelva a intentar.");
                return View("Index", await ConstruirAsync(edicion: edicion));
            }

            TempData["Ok"] = $"Departamento {codigo} actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al actualizar departamento {Codigo}", codigo);
            Error(MensajeSql(ex, "actualizar", "UPDATE"));
            return View("Index", await ConstruirAsync(edicion: edicion));
        }
    }

    // ---------------- ELIMINAR ----------------
    [HttpPost("eliminar")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        codigo = LimpiarCodigo(codigo);

        if (codigo.Length != LargoCodigo)
        {
            TempData["Error"] = "No se pudo identificar el departamento a eliminar.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            int filas = await _repo.EliminarAsync(codigo);
            if (filas == 0)
                TempData["Error"] = $"El departamento {codigo} ya no existe. Actualice el listado.";
            else
                TempData["Ok"] = $"Departamento {codigo} eliminado correctamente.";
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al eliminar departamento {Codigo}", codigo);
            TempData["Error"] = ex.Number == 547
                ? "No se puede eliminar: el departamento tiene registros asociados protegidos por la base de datos."
                : MensajeSql(ex, "eliminar", "DELETE");
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------------- UTILIDADES ----------------
    private async Task<DepartamentosViewModel> ConstruirAsync(Departamento nuevo = null,
                                                              Departamento edicion = null)
    {
        var vm = new DepartamentosViewModel
        {
            Nuevo = nuevo ?? new Departamento(),
            Edicion = edicion
        };

        try
        {
            vm.Lista = await _repo.ListarAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error al cargar departamentos");
            Error("No se pudo cargar la lista de departamentos.");
        }

        return vm;
    }

    private bool Validar(string codigo, string descripcion)
    {
        if (codigo.Length != LargoCodigo)
        {
            Error($"El código debe tener exactamente {LargoCodigo} caracteres.");
            return false;
        }
        if (descripcion.Length == 0)
        {
            Error("Ingrese la descripción del departamento.");
            return false;
        }
        if (descripcion.Length > LargoDescripcion)
        {
            Error($"La descripción no puede superar {LargoDescripcion} caracteres.");
            return false;
        }
        return true;
    }

    /// <summary>Traduce los números de error de SQL Server a mensajes claros.</summary>
    private static string MensajeSql(SqlException ex, string accion, string permiso) =>
        ex.Number switch
        {
            229 or 230 or 262 =>
                $"La cuenta de SQL Server no tiene permiso {permiso} sobre PEDEP_DEPAR. Ejecute Database/02_permisos.sql.",
            4060 or 18456 or -1 or 53 =>
                "No se pudo establecer la conexión con SQL Server.",
            _ => $"No se pudo {accion} el departamento. Revise el registro de la aplicación."
        };

    private static string LimpiarCodigo(string s) => s?.Trim().ToUpperInvariant() ?? "";
    private static string LimpiarTexto(string s) => s?.Trim() ?? "";
    private void Error(string mensaje) => ModelState.AddModelError(string.Empty, mensaje);
}

using Ec.Edu.Sigh.Gr04.Data;
using Ec.Edu.Sigh.Gr04.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Ec.Edu.Sigh.Gr04.Controllers;

[Authorize]
[Route("cargos")]
public class CargosController : Controller
{
    private const int LargoCodigo = 3;
    private const int LargoDescripcion = 50;

    private readonly ICargoRepository _repo;
    private readonly IDepartamentoRepository _repoDepartamentos;
    private readonly ILogger<CargosController> _logger;

    public CargosController(ICargoRepository repo,
                            IDepartamentoRepository repoDepartamentos,
                            ILogger<CargosController> logger)
    {
        _repo = repo;
        _repoDepartamentos = repoDepartamentos;
        _logger = logger;
    }

    // ---------------- CONSULTAR ----------------
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await ConstruirAsync());

    // ---------------- REGISTRAR ----------------
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(string codigo, string descripcion, string departamentoCodigo)
    {
        codigo = LimpiarCodigo(codigo);
        descripcion = LimpiarTexto(descripcion);
        departamentoCodigo = LimpiarCodigo(departamentoCodigo);
        var nuevo = new Cargo(codigo, descripcion, departamentoCodigo);

        if (!Validar(codigo, descripcion, departamentoCodigo))
            return View("Index", await ConstruirAsync(nuevo: nuevo));

        try
        {
            if (!await _repoDepartamentos.ExisteAsync(departamentoCodigo))
            {
                Error("El departamento seleccionado ya no existe. Se actualizó la lista.");
                return View("Index", await ConstruirAsync(nuevo: nuevo));
            }

            if (await _repo.ExisteAsync(codigo))
            {
                Error($"Ya existe un cargo con el código {codigo}.");
                return View("Index", await ConstruirAsync(nuevo: nuevo));
            }

            await _repo.InsertarAsync(nuevo);
            TempData["Ok"] = $"Cargo {codigo} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al registrar cargo {Codigo}", codigo);
            if (ex.Number is 2627 or 2601)
                Error($"Ya existe un cargo con el código {codigo}.");
            else if (ex.Number == 547)
                Error("El departamento seleccionado no es válido.");
            else
                Error(MensajeSql(ex, "registrar", "INSERT"));
            return View("Index", await ConstruirAsync(nuevo: nuevo));
        }
    }

    // ---------------- MODIFICAR ----------------
    [HttpPost("actualizar")]
    public async Task<IActionResult> Actualizar(string codigo, string descripcion, string departamentoCodigo)
    {
        codigo = LimpiarCodigo(codigo);
        descripcion = LimpiarTexto(descripcion);
        departamentoCodigo = LimpiarCodigo(departamentoCodigo);
        var edicion = new Cargo(codigo, descripcion, departamentoCodigo);

        if (!Validar(codigo, descripcion, departamentoCodigo))
            return View("Index", await ConstruirAsync(edicion: edicion));

        try
        {
            int filas = await _repo.ActualizarAsync(edicion);
            if (filas == 0)
            {
                Error("No se encontró el cargo. Actualice el listado y vuelva a intentar.");
                return View("Index", await ConstruirAsync(edicion: edicion));
            }

            TempData["Ok"] = $"Cargo {codigo} actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al actualizar cargo {Codigo}", codigo);
            if (ex.Number == 547)
                Error("El departamento seleccionado no es válido.");
            else
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
            TempData["Error"] = "No se pudo identificar el cargo a eliminar.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            int filas = await _repo.EliminarAsync(codigo);
            if (filas == 0)
                TempData["Error"] = $"El cargo {codigo} ya no existe. Actualice el listado.";
            else
                TempData["Ok"] = $"Cargo {codigo} eliminado correctamente.";
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al eliminar cargo {Codigo}", codigo);
            TempData["Error"] = ex.Number == 547
                ? "No se puede eliminar: el cargo tiene registros asociados protegidos por la base de datos."
                : MensajeSql(ex, "eliminar", "DELETE");
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------------- UTILIDADES ----------------
    private async Task<CargosViewModel> ConstruirAsync(Cargo nuevo = null, Cargo edicion = null)
    {
        var vm = new CargosViewModel
        {
            Nuevo = nuevo ?? new Cargo(),
            Edicion = edicion
        };

        try
        {
            vm.Lista = await _repo.ListarAsync();
            vm.Departamentos = await _repoDepartamentos.ListarAsync(); // list box
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error al cargar cargos o departamentos");
            Error("No se pudo cargar la información de cargos y departamentos.");
        }

        return vm;
    }

    private bool Validar(string codigo, string descripcion, string departamentoCodigo)
    {
        if (codigo.Length != LargoCodigo)
        {
            Error($"El código debe tener exactamente {LargoCodigo} caracteres.");
            return false;
        }
        if (departamentoCodigo.Length != LargoCodigo)
        {
            Error("Seleccione un departamento de la lista.");
            return false;
        }
        if (descripcion.Length == 0)
        {
            Error("Ingrese la descripción del cargo.");
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
                $"La cuenta de SQL Server no tiene permiso {permiso} sobre PECAR_CARGO. Ejecute Database/02_permisos.sql.",
            4060 or 18456 or -1 or 53 =>
                "No se pudo establecer la conexión con SQL Server.",
            _ => $"No se pudo {accion} el cargo. Revise el registro de la aplicación."
        };

    private static string LimpiarCodigo(string s) => s?.Trim().ToUpperInvariant() ?? "";
    private static string LimpiarTexto(string s) => s?.Trim() ?? "";
    private void Error(string mensaje) => ModelState.AddModelError(string.Empty, mensaje);
}
namespace Ec.Edu.Sigh.Gr04.Models;

public class CargosViewModel
{
    public List<Cargo> Lista { get; set; } = new();

    /// <summary>Datos del list box de departamentos.</summary>
    public List<Departamento> Departamentos { get; set; } = new();

    /// <summary>Valores del formulario de registro (se conservan si hay errores).</summary>
    public Cargo Nuevo { get; set; } = new();

    /// <summary>Si no es null, la ventana de modificación se muestra abierta.</summary>
    public Cargo Edicion { get; set; }
}
namespace Ec.Edu.Sigh.Gr04.Models;

public class DepartamentosViewModel
{
    public List<Departamento> Lista { get; set; } = new();

    /// <summary>Valores del formulario de registro (se conservan si hay errores).</summary>
    public Departamento Nuevo { get; set; } = new();

    /// <summary>Si no es null, la ventana de modificación se muestra abierta.</summary>
    public Departamento Edicion { get; set; }
}

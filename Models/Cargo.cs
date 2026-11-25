namespace Ec.Edu.Sigh.Gr04.Models;

public class Cargo
{
    public string Codigo { get; set; }                  // PECAR_CODIGO
    public string Descripcion { get; set; }             // PECAR_DESCRI
    public string DepartamentoCodigo { get; set; }      // PEDEP_CODIGO (clave foránea)

    /// <summary>Solo para mostrar en la tabla (viene del JOIN).</summary>
    public string DepartamentoDescripcion { get; set; }

    public Cargo() { }

    public Cargo(string codigo, string descripcion,
                 string departamentoCodigo, string departamentoDescripcion = null)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        DepartamentoCodigo = departamentoCodigo;
        DepartamentoDescripcion = departamentoDescripcion;
    }
}
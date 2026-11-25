namespace Ec.Edu.Sigh.Gr04.Models;

public class Departamento
{
    public string Codigo { get; set; }
    public string Descripcion { get; set; }

    public Departamento() { }

    public Departamento(string codigo, string descripcion)
    {
        Codigo = codigo;
        Descripcion = descripcion;
    }
}

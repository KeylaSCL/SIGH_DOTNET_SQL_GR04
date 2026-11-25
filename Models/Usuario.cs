namespace Ec.Edu.Sigh.Gr04.Models;

public class Usuario
{
    public string Codigo { get; set; }
    public string CodigoPersona { get; set; }
    public string CodigoEstado { get; set; }
    public string ContrasenaAlmacenada { get; set; }
    public int IntentosFallidos { get; set; }
}

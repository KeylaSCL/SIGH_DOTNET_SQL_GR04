namespace Ec.Edu.Sigh.Gr04;

/// <summary>Opciones modificables en appsettings.json, sección "Sigh".</summary>
public class SighoOptions
{
    /// <summary>Código de estado que permite iniciar sesión (columna XEEST_CODIGO).</summary>
    public string EstadoActivo { get; set; } = "A";
}

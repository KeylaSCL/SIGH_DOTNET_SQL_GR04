using System.ComponentModel.DataAnnotations;

namespace Ec.Edu.Sigh.Gr04.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su usuario.")]
    public string Codigo { get; set; }

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    public string Clave { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Core_Models.ViewModels
{
    public class FamiliaUsuarioViewModel
    {
        // Datos de la familia
        public int FamiliaId { get; set; }
        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; }

        // Datos del usuario asociado
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        // Propiedad adicional para mostrar en listados
        [Display(Name = "Nombre de Usuario")]
        public string? NombreUsuario { get; set; }
    }
}

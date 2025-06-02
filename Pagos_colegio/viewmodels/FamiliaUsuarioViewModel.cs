using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.ViewModels
{
    public class FamiliaUsuarioViewModel
    {
        // Datos de familia
        public string ApellidoMaterno { get; set; }

        public string ApellidoPaterno { get; set; }

        // Datos del usuario asociado
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

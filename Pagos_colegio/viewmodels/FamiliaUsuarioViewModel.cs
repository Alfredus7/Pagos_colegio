using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.ViewModels
{
    public class FamiliaUsuarioViewModel
    {
        // Datos de la familia
        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        // Datos del usuario asociado
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Cuenta
    {
        [Key]
        public int ID_CUENTA { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(6)]
        public string PIN { get; set; }

        // Relaciones
        public virtual Familia Familia { get; set; }
    }

}

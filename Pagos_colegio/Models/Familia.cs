using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pagos_colegio_web.Models
{
    public class Familia
    {
        [Key]
        public int ID_FAMILIA { get; set; }

        [MaxLength(50)]
        public string ApellidoMaterno { get; set; }

        [MaxLength(50)]
        public string ApellidoPaterno { get; set; }

        // Usuario vinculado con Identity
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser Usuario { get; set; }

        // Relación con estudiantes
        public virtual List<Estudiante> Estudiantes { get; set; }
    }


}

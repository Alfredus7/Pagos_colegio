using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pagos_colegio_web.Models
{
    public class Familia
    {
        [Key]
        public int FamiliaId { get; set; }

        [MaxLength(50)]
        public string ApellidoMaterno { get; set; }

        [MaxLength(50)]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual IdentityUser Usuario { get; set; }

        // Propiedad auxiliar para mostrar UserName en vistas
        [NotMapped]
        public string NombreUsuario => Usuario?.UserName;

        public virtual ICollection<Estudiante> Estudiantes { get; set; }
    }


}

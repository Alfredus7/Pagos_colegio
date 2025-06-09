using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Pagos_colegio.Models
{
    public class Familia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID Familia")]
        public int FamiliaId { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; }

        // Relación con IdentityUser
        [Required]
        [Display(Name = "Usuario Asociado")]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual IdentityUser Usuario { get; set; }

        // Relación con Estudiantes
        [Display(Name = "Estudiantes a Cargo")]
        public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

        // Propiedades no mapeadas
        [NotMapped]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario => Usuario?.UserName;

        [NotMapped]
        [Display(Name = "Familia")]
        public string NombreFamilia => $"{ApellidoPaterno} {ApellidoMaterno}";


    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Pagos_colegio_web.Models
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

        [Required]
        [Display(Name = "Usuario Asociado")]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual IdentityUser Usuario { get; set; }

        [Display(Name = "Estudiantes a Cargo")]
        public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

        [NotMapped]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario => Usuario?.UserName;

        [NotMapped]
        [Display(Name = "Familia")]
        public string NombreFamilia => $"{ApellidoPaterno} {ApellidoMaterno}";
    }
}

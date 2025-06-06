using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagos_colegio_web.Models
{
    public class Estudiante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EstudianteId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una familia")]
        [Display(Name = "Familia")]
        public int FamiliaId { get; set; }

        [ForeignKey("FamiliaId")]
        [Display(Name = "Familia")]
        public virtual Familia? Familia { get; set; }

        [Display(Name = "Pagos Realizados")]
        public virtual ICollection<Pago>? Pagos { get; set; } = new List<Pago>();

        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Familia?.ApellidoPaterno} {Familia?.ApellidoMaterno}";
    }
}

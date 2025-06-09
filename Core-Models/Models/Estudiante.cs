using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_Models.Models
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

        // Relación con Familia
        [Required(ErrorMessage = "Debe seleccionar una familia")]
        [Display(Name = "Familia")]
        public int FamiliaId { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0% y 100%")]
        [Display(Name = "Descuento (%)")]
        public int Descuento { get; set; }

        [ForeignKey("FamiliaId")]
        public virtual Familia? Familia { get; set; }

        // Relación con Pagos
        [Display(Name = "Pagos Realizados")]
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Propiedad combinada no mapeada a la BD
        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Familia?.ApellidoPaterno} {Familia?.ApellidoMaterno}";

        // Fecha de inscripción
        [Required(ErrorMessage = "La fecha de inscripción es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        // Relación con Tarifa
        [Required(ErrorMessage = "Debe asignarse una tarifa")]
        [Display(Name = "Tarifa Asignada")]
        public int TarifaId { get; set; }

        [ForeignKey("TarifaId")]
        public virtual Tarifa? Tarifa { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagos_colegio_web.Models
{
    public class Pago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID Pago")]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        // Relación con Estudiante
        [Required(ErrorMessage = "Debe seleccionar un estudiante")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }

        // Montos
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        [Display(Name = "Descuento Aplicado")]
        public decimal Descuento { get; set; } = 0;

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Total Pagado")]
        public decimal TotalPago { get; set; }
    }
}

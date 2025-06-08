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
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe seleccionar un estudiante")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        [Display(Name = "Estudiante")]
        public virtual Estudiante? Estudiante { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Descuento Aplicado")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public decimal Descuento { get; set; } = 0;

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Total Pagado")]
        public decimal TotalPago { get; set; }

    }
}
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

        [Required(ErrorMessage = "Debe especificar el mes al que corresponde el pago.")]
        [Display(Name = "Periodo")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{4}$", ErrorMessage = "El formato debe ser MM/yyyy.")]
        public string Periodo { get; set; } = DateTime.Now.ToString("MM/yyyy");

        // Relación con Estudiante
        [Required(ErrorMessage = "Debe seleccionar un estudiante")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Total Pagado")]
        public decimal TotalPago { get; set; }
    }
}

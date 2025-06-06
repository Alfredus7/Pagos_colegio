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
        public virtual Estudiante Estudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una tarifa")]
        [Display(Name = "Tarifa")]
        public int TarifaId { get; set; }

        [ForeignKey("TarifaId")]
        [Display(Name = "Tarifa")]
        public virtual Tarifa Tarifa { get; set; }

        [Display(Name = "Recibo")]
        public virtual Recibo Recibo { get; set; }

        [NotMapped]
        [Display(Name = "Monto Pagado")]
        public decimal MontoPagado => Tarifa?.Monto ?? 0;
    }
}
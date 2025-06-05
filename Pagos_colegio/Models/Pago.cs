using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Pago
    {
        [Key]
        public int PagoId { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante Estudiante { get; set; }

        public int TarifaId { get; set; }

        [ForeignKey("TarifaId")]
        public virtual Tarifa Tarifa { get; set; }

        public virtual Recibo Recibo { get; set; }
    }

}

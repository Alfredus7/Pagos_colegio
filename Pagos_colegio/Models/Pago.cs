using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Pago
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        // FK Estudiante
        public int ID_ESTUDIANTE { get; set; }

        [ForeignKey("ID_ESTUDIANTE")]
        public virtual Estudiante Estudiante { get; set; }

        // FK Tarifa
        public int ID_TARIFA { get; set; }

        [ForeignKey("ID_TARIFA")]
        public virtual Tarifa Tarifa { get; set; }

        public virtual Recibo Recibo { get; set; }
    }

}

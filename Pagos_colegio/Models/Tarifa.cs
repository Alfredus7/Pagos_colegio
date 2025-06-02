using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Tarifa
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public DateTime FechaIni { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public virtual List<Pago> Pagos { get; set; }
    }

}

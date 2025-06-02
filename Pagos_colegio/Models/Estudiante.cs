using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Estudiante
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }

        // FK
        public int ID_FAMILIA { get; set; }

        [ForeignKey("ID_FAMILIA")]
        public virtual Familia Familia { get; set; }

        public virtual List<Pago> Pagos { get; set; }
    }

}

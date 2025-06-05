using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Recibo
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(100)]
        public string Descripcion { get; set; }

        // FK Pago de donde saldra el recibo
        public int ID_PAGO { get; set; }

        [ForeignKey("ID_PAGO")]
        public virtual Pago Pago { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Tarifa
    {
        public int Id { get; set; }
        public DateTime? Fecha_Ini { get; set; }
        public DateTime? Fecha_Fin { get; set; }
        public decimal Monto { get; set; }

        public virtual ICollection<Pago> Pagos { get; set; }
    }
}

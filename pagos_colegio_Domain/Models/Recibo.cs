using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Recibo
    {
        public int Id { get; set; }
        public int? Id_Pago { get; set; }
        public string Descripcion { get; set; }

        public virtual Pago Pago { get; set; }
    }
}

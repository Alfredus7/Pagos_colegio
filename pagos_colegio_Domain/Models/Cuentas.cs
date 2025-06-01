using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Cuentas
    {
        public string UserName { get; set; }
        public string PIN { get; set; }

        // Relación con familia
        public virtual Familia Familia { get; set; }
    }
}

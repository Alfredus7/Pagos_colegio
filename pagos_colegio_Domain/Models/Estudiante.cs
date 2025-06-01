using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Estudiante
    {
        public int Id { get; set; }
        public int Id_Familia { get; set; }
        public string Nombre { get; set; }

        public virtual Familia Familia { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}

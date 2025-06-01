using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int? Id_Estudiante { get; set; }
        public int? Id_Tarifa { get; set; }
        public DateTime? Fecha_Pago { get; set; }

        public virtual Estudiante Estudiante { get; set; }
        public virtual Tarifa Tarifa { get; set; }
        public virtual Recibo Recibo { get; set; }
    }
}

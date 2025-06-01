using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pagos_colegio_Core.Models
{
    public class Familia
    {
        public int Id_Familia { get; set; }
        public string Apellido_Materno { get; set; }
        public string Apellido_Paterno { get; set; }
        public string UserName { get; set; }

        public ICollection<Estudiante> Estudiantes { get; set; }
    }

}

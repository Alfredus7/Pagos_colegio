using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Familia
    {
        [Key]
        public int ID_FAMILIA { get; set; }

        [MaxLength(50)]
        public string ApellidoMaterno { get; set; }

        [MaxLength(50)]
        public string ApellidoPaterno { get; set; }

        // FK
        public int ID_CUENTA { get; set; }

        [ForeignKey("ID_CUENTA")]
        public virtual Cuenta Cuenta { get; set; }

        // Relaciones
        public virtual List<Estudiante> Estudiantes { get; set; }
    }

}

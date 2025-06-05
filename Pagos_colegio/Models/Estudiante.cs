using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pagos_colegio_web.Models
{
    public class Estudiante
    {
        [Key]
        public int EstudianteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }

        public int FamiliaId { get; set; }

        [ForeignKey("FamiliaId")]
        public virtual Familia Familia { get; set; }

        public virtual ICollection<Pago> Pagos { get; set; }
    }

}

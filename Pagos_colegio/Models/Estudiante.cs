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

        // Clave foránea a Familia
        public int FamiliaId { get; set; }

        [ForeignKey("FamiliaId")]
        public virtual Familia? Familia { get; set; }

        // Lista de pagos del estudiante
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }

}

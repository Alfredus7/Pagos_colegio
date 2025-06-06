using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagos_colegio_web.Models
{
    public class Recibo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID Recibo")]
        public int ID { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "ID Pago")]
        public int ID_PAGO { get; set; }

        [ForeignKey("ID_PAGO")]
        [Display(Name = "Pago Asociado")]
        public virtual Pago Pago { get; set; }

        [NotMapped]
        [Display(Name = "Código de Recibo")]
        public string CodigoRecibo => $"REC-{ID.ToString().PadLeft(6, '0')}";
    }
}
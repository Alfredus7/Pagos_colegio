using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagos_colegio_web.Models
{
    public class Tarifa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID Tarifa")]
        public int TarifaId { get; set; }

        [StringLength(50)]
        [Display(Name = "Gestión")]
        public string? Gestion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; } = DateTime.Now.AddMonths(1);

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        // Relación con pagos
        [Display(Name = "Pagos Realizados")]
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Periodo")]
        public string Periodo => $"{FechaInicio:MM/yyyy} - {FechaFin:MM/yyyy}";

        [NotMapped]
        [Display(Name = "Vigente")]
        public bool EstaVigente => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;

        [NotMapped]
        [Display(Name = "Monto Total")]
        public decimal MontoTotal
        {
            get
            {
                var fechaFin = (FechaFin.Year == 1) ? DateTime.Today : FechaFin;
                int cantidadMeses = ((fechaFin.Year - FechaInicio.Year) * 12) + (fechaFin.Month - FechaInicio.Month) + 1;
                return Math.Round(Monto * cantidadMeses, 2);
            }
        }

    }
}

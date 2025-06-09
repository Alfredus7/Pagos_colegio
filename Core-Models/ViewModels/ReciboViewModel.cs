// ViewModel/ReciboViewModel.cs
using Pagos_colegio.Models;

namespace Pagos_colegio.ViewModel
{
    public class ReciboViewModel
    {
        public Pago Pago { get; set; }
        public Estudiante Estudiante => Pago?.Estudiante;
        public Familia Familia => Estudiante?.Familia;
        public Tarifa Tarifa => Estudiante?.Tarifa;
        public string Periodo { get; set; } // Ej: "06/2025"
    }
}


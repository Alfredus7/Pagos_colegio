namespace Pagos_colegio.ViewModel
{
    public class ReporteViewModel
    {
        public List<HistorialPagoItem> Historial { get; set; } = new();
        public List<HistorialPagoItem> PagosDelMes { get; set; } = new();

        // Deuda mensual por estudiante, periodo y monto
        public List<DeudaMensualItem> DeudaMensual { get; set; } = new(); // ← NUEVO
    }

    public class HistorialPagoItem
    {
        public string Estudiante { get; set; }
        public DateTime Fecha { get; set; }
        public string? Periodo { get; set; }
        public decimal MontoPagado { get; set; }
    }

    // ✨ NUEVA clase para representar deudas mensuales
    public class DeudaMensualItem
    {
        public string Estudiante { get; set; }
        public string Periodo { get; set; } // MM/yyyy
        public decimal Monto { get; set; }
    }
}
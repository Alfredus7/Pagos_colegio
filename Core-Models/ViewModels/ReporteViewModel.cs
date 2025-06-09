using System;
using System.Collections.Generic;
using Pagos_colegio.Models;

namespace Pagos_colegio.ViewModels
{
    public class ReporteViewModel
    {
        public List<HistorialPagoItem> Historial { get; set; } = new();
        public List<(Estudiante estudiante, Tarifa tarifa)> Deuda { get; set; } = new();
        public List<HistorialPagoItem> PagosDelMes { get; set; } = new();
    }

    public class HistorialPagoItem
    {
        public string Estudiante { get; set; }
        public DateTime Fecha { get; set; }
        public string? Periodo { get; set; }
        public decimal Monto { get; set; }
    }
}


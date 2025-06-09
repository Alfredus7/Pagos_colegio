using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Pagos_colegio_web.ViewModels;
using Rotativa.AspNetCore;

namespace Pagos_colegio_web.Controllers
{
    [Authorize(Roles = "Familia,Admin")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Genera un recibo de pago en formato PDF.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerarReciboPdf(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Familia)
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Tarifa) // <- Esto es lo que faltaba
                .FirstOrDefaultAsync(p => p.PagoId == id);

            if (pago == null)
                return NotFound();

            var viewModel = new ReciboViewModel
            {
                Pago = pago,
                Periodo = $"{pago.FechaPago:MM/yyyy}"
            };

            return new ViewAsPdf("Recibo", viewModel)
            {
                FileName = $"Recibo_{pago.PagoId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A5,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }




        [HttpGet]
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> PagosPendientes()
        {
            var userId = _userManager.GetUserId(User);

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Tarifa)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            var pagosPendientes = new List<(Estudiante estudiante, string periodo, decimal monto)>();
            var hoy = DateTime.Today;
            var primerDiaDelMesActual = new DateTime(hoy.Year, hoy.Month, 1);

            foreach (var estudiante in familia.Estudiantes)
            {
                var tarifa = estudiante.Tarifa;
                if (tarifa == null) continue;

                // Comenzar desde el mes de inscripción o desde el inicio de la tarifa, el más reciente
                var fechaInicio = new DateTime(estudiante.FechaInscripcion.Year, estudiante.FechaInscripcion.Month, 1);
                var fechaTarifaInicio = new DateTime(tarifa.FechaInicio.Year, tarifa.FechaInicio.Month, 1);
                var fecha = (fechaInicio > fechaTarifaInicio) ? fechaInicio : fechaTarifaInicio;

                // Terminar en el mes actual (no en el futuro)
                var fechaFin = primerDiaDelMesActual;

                while (fecha <= fechaFin)
                {
                    bool yaPagoEseMes = estudiante.Pagos.Any(p =>
                        p.FechaPago.Year == fecha.Year && p.FechaPago.Month == fecha.Month);

                    if (!yaPagoEseMes)
                    {
                        var periodoTexto = fecha.ToString("MM/yyyy");
                        pagosPendientes.Add((estudiante, periodoTexto, tarifa.Monto));
                    }

                    fecha = fecha.AddMonths(1);
                }
            }

            return View(pagosPendientes);
        }

        /// <summary>
        /// Muestra el historial de pagos para un estudiante específico.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> HistorialPagos(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Pagos)
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null) return NotFound();

            estudiante.Pagos = estudiante.Pagos
                .OrderByDescending(p => p.FechaPago)
                .ToList();

            return View(estudiante);
        }

        /// <summary>
        /// Muestra un resumen de reportes para la familia actual:
        /// historial completo, pagos del mes y deudas.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> Reportes()
        {
            var userId = _userManager.GetUserId(User);

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Tarifa)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            var estudiantes = familia.Estudiantes;
            var now = DateTime.Now;

            // Historial completo
            var historialPagos = estudiantes
                .SelectMany(e => e.Pagos.Select(p => new HistorialPagoItem
                {
                    Estudiante = e.NombreCompleto,
                    Fecha = p.FechaPago,
                    Periodo = p.FechaPago.ToString("MM/yyyy"),
                    Monto = p.TotalPago
                }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // Pagos del mes actual
            var pagosMes = historialPagos
                .Where(p => p.Fecha.Month == now.Month && p.Fecha.Year == now.Year)
                .ToList();

            // Deudas: tarifas vencidas no pagadas
            var deuda = estudiantes
                .Where(e =>
                    e.Tarifa != null &&
                    e.Tarifa.FechaFin < now &&
                    !e.Pagos.Any(p => p.FechaPago >= e.Tarifa.FechaInicio && p.FechaPago <= e.Tarifa.FechaFin))
                .Select(e => (e, e.Tarifa!))
                .ToList();

            var modelo = new ReporteViewModel
            {
                Historial = historialPagos,
                PagosDelMes = pagosMes,
                Deuda = deuda
            };

            return View(modelo);
        }

    }
}


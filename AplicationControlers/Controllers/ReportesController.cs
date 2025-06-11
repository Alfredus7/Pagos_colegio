using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio.Data;
using Pagos_colegio.Models;
using Pagos_colegio.ViewModel;
using Rotativa.AspNetCore;

namespace Pagos_colegio.Controllers
{
    /// <summary>
    /// Controlador responsable de generar reportes, recibos PDF, historial y deudas para estudiantes.
    /// </summary>
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
        /// Genera un recibo de pago en formato PDF usando Rotativa.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Familia,Admin")]
        public async Task<IActionResult> GenerarReciboPdf(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Familia)
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Tarifa)
                .FirstOrDefaultAsync(p => p.PagoId == id);

            if (pago == null)
                return NotFound();

            var viewModel = new ReciboViewModel
            {
                Pago = pago,
                Periodo = pago.FechaPago.ToString("MM/yyyy")
            };

            return new ViewAsPdf("Recibo", viewModel)
            {
                FileName = $"Recibo_{pago.PagoId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A6,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

        

        /// <summary>
        /// Muestra el historial de pagos de un estudiante específico.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Familia,Admin")]
        public async Task<IActionResult> HistorialPagos(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Pagos)
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null) return NotFound();

            estudiante.Pagos = estudiante.Pagos.OrderByDescending(p => p.FechaPago).ToList();
            return View(estudiante);
        }
        /// <summary>
        /// Muestra los pagos pendientes por periodo para cada estudiante.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Familia,Admin")]
        public async Task<IActionResult> PagosPendientes(int? estudianteId = null)
        {
            var pagosPendientes = new List<(Estudiante estudiante, string periodo, decimal monto)>();
            var hoy = DateTime.Today;
            var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);

            List<Estudiante> estudiantes = new();

            if (User.IsInRole("Admin"))
            {
                estudiantes = await _context.Estudiantes
                    .Include(e => e.Pagos)
                    .Include(e => e.Tarifa)
                    .ToListAsync();

                if (estudianteId.HasValue)
                    estudiantes = estudiantes.Where(e => e.EstudianteId == estudianteId.Value).ToList();
            }
            else if (User.IsInRole("Familia"))
            {
                var userId = _userManager.GetUserId(User);
                var familia = await _context.Familias
                    .Include(f => f.Estudiantes)
                        .ThenInclude(e => e.Pagos)
                    .Include(f => f.Estudiantes)
                        .ThenInclude(e => e.Tarifa)
                    .FirstOrDefaultAsync(f => f.UsuarioId == userId);

                if (familia == null)
                    return NotFound("No se encontró una familia asociada.");

                estudiantes = familia.Estudiantes.Where(e => e.Tarifa != null).ToList();
            }

            foreach (var estudiante in estudiantes)
            {
                var tarifa = estudiante.Tarifa;
                if (tarifa == null) continue;

                var pagos = estudiante.Pagos ?? new List<Pago>();
                var inicio = MaxDate(
                    new DateTime(estudiante.FechaInscripcion.Year, estudiante.FechaInscripcion.Month, 1),
                    new DateTime(tarifa.FechaInicio.Year, tarifa.FechaInicio.Month, 1));

                var fin = MinDate(
                    primerDiaMes,
                    new DateTime(tarifa.FechaFin.Year, tarifa.FechaFin.Month, 1));

                while (inicio <= fin)
                {
                    var periodo = inicio.ToString("MM/yyyy");
                    if (!pagos.Any(p => p.Periodo == periodo))
                    {
                        var total = Math.Round(tarifa.Mensualidad * (1 - ((decimal)estudiante.Descuento / 100)), 2);
                        pagosPendientes.Add((estudiante, periodo, total));
                    }
                    inicio = inicio.AddMonths(1);
                }
            }

            return View(pagosPendientes);
        }

        /// <summary>
        /// Muestra un resumen para la familia actual: historial, pagos del mes y deudas pendientes.
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
            var primerDiaMesActual = new DateTime(now.Year, now.Month, 1);

            var historialPagos = estudiantes
                .SelectMany(e => e.Pagos.Select(p => new HistorialPagoItem
                {
                    Estudiante = e.NombreCompleto,
                    Fecha = p.FechaPago,
                    Periodo = p.Periodo,
                    MontoPagado = p.TotalPago
                }))
                .OrderBy(p => p.Fecha)
                .ToList();

            var pagosMes = historialPagos
                .Where(p => p.Fecha.Month == now.Month && p.Fecha.Year == now.Year)
                .ToList();

            var deudas = new List<DeudaMensualItem>();

            foreach (var estudiante in estudiantes)
            {
                if (estudiante.Tarifa == null) continue;

                var pagos = estudiante.Pagos ?? new List<Pago>();
                var tarifa = estudiante.Tarifa;

                // Fecha de inicio: la mayor entre fecha de inscripción y fecha inicio de tarifa
                var inicio = MaxDate(
                    new DateTime(estudiante.FechaInscripcion.Year, estudiante.FechaInscripcion.Month, 1),
                    new DateTime(tarifa.FechaInicio.Year, tarifa.FechaInicio.Month, 1));

                // Fecha de fin: la menor entre fin de tarifa y mes actual
                var fin = MinDate(
                    primerDiaMesActual,
                    new DateTime(tarifa.FechaFin.Year, tarifa.FechaFin.Month, 1));

                while (inicio <= fin)
                {
                    var periodo = inicio.ToString("MM/yyyy");
                    if (!pagos.Any(p => p.Periodo == periodo))
                    {
                        var monto = Math.Round(tarifa.Mensualidad * (1 - ((decimal)estudiante.Descuento / 100)), 2);
                        deudas.Add(new DeudaMensualItem
                        {
                            Estudiante = estudiante.NombreCompleto,
                            Periodo = periodo,
                            Monto = monto
                        });
                    }
                    inicio = inicio.AddMonths(1);
                }
            }

            var modelo = new ReporteViewModel
            {
                Historial = historialPagos,
                PagosDelMes = pagosMes,
                DeudaMensual = deudas
            };

            return View(modelo);
        }

        // 🔧 Utilidades privadas

        private static DateTime MaxDate(DateTime a, DateTime b) => (a > b) ? a : b;
        private static DateTime MinDate(DateTime a, DateTime b) => (a < b) ? a : b;
    }
}



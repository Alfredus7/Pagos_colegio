using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
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
        /// <param name="id">ID del pago</param>
        [HttpGet]
        public async Task<IActionResult> GenerarReciboPdf(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Familia)
                .FirstOrDefaultAsync(p => p.PagoId == id);

            if (pago == null)
                return NotFound();

            return new ViewAsPdf("Recibo", pago)
            {
                FileName = $"Recibo_{pago.PagoId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A5,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

        /// <summary>
        /// Muestra los pagos pendientes por estudiante de una familia.
        /// </summary>
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

            var pagosPendientes = familia.Estudiantes
                .Where(e => e.Tarifa != null &&
                            e.Tarifa.FechaInicio <= DateTime.Now &&
                            e.Tarifa.FechaFin >= DateTime.Now &&
                            !e.Pagos.Any(p => p.FechaPago >= e.Tarifa.FechaInicio && p.FechaPago <= e.Tarifa.FechaFin))
                .Select(e => (e, e.Tarifa!))
                .ToList();

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
                    Periodo = e.Tarifa?.Periodo ?? "N/A",
                    Monto = e.Tarifa?.Monto ?? 0
                }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // Pagos del mes actual
            var pagosMes = estudiantes
                .SelectMany(e => e.Pagos
                    .Where(p => p.FechaPago.Month == now.Month && p.FechaPago.Year == now.Year)
                    .Select(p => new HistorialPagoItem
                    {
                        Estudiante = e.NombreCompleto,
                        Fecha = p.FechaPago,
                        Periodo = e.Tarifa?.Periodo ?? "N/A",
                        Monto = e.Tarifa?.Monto ?? 0
                    }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // Deudas: tarifas vencidas no pagadas
            var deuda = estudiantes
                .Where(e => e.Tarifa != null &&
                            e.Tarifa.FechaFin < now &&
                            !e.Pagos.Any(p => p.FechaPago >= e.Tarifa.FechaInicio &&
                                             p.FechaPago <= e.Tarifa.FechaFin))
                .Select(e => (e, e.Tarifa!))
                .ToList();

            var modelo = new ReporteViewModel
            {
                Historial = historialPagos,
                Deuda = deuda,
                PagosDelMes = pagosMes
            };

            return View(modelo);
        }
    }
}

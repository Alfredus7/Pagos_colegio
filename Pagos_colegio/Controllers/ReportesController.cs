using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Rotativa.AspNetCore;

namespace Pagos_colegio_web.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Familia,Admin")]
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

            var pagosPendientes = new List<(Estudiante estudiante, Tarifa tarifa)>();

            foreach (var estudiante in familia.Estudiantes)
            {
                var tarifa = estudiante.Tarifa;

                if (tarifa == null)
                    continue;

                bool tarifaVigente = tarifa.FechaInicio <= DateTime.Now && tarifa.FechaFin >= DateTime.Now;

                if (!tarifaVigente)
                    continue;

                bool yaPagado = estudiante.Pagos.Any(p =>
                    p.FechaPago >= tarifa.FechaInicio &&
                    p.FechaPago <= tarifa.FechaFin
                );

                if (!yaPagado)
                {
                    pagosPendientes.Add((estudiante, tarifa));
                }
            }

            return View(pagosPendientes);
        }

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

            estudiante.Pagos = estudiante.Pagos.OrderByDescending(p => p.FechaPago).ToList();

            return View(estudiante);
        }



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
            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            var pagosMes = estudiantes
                .SelectMany(e => e.Pagos
                    .Where(p => p.FechaPago.Month == mesActual && p.FechaPago.Year == añoActual)
                    .Select(p => new HistorialPagoItem
                    {
                        Estudiante = e.NombreCompleto,
                        Fecha = p.FechaPago,
                        Periodo = e.Tarifa?.Periodo ?? "N/A",
                        Monto = e.Tarifa?.Monto ?? 0
                    }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // Deudas: solo tarifa asociada al estudiante y si no fue pagada
            var deuda = new List<(Estudiante estudiante, Tarifa tarifa)>();

            foreach (var estudiante in estudiantes)
            {
                var tarifa = estudiante.Tarifa;

                if (tarifa == null)
                    continue;

                // Considerar solo tarifas ya vencidas
                if (tarifa.FechaFin < DateTime.Now)
                {
                    bool pagado = estudiante.Pagos.Any(p =>
                        p.FechaPago >= tarifa.FechaInicio && p.FechaPago <= tarifa.FechaFin);

                    if (!pagado)
                    {
                        deuda.Add((estudiante, tarifa));
                    }
                }
            }

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


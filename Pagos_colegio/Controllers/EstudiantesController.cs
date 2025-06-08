using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Rotativa.AspNetCore;

namespace Pagos_colegio_web.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EstudiantesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===========================
        // 1. Listado general (Admin)
        // ===========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Estudiantes.Include(e => e.Familia).Include(e => e.Tarifa);
            var familias = _context.Familias.Include(f => f.Usuario).ToList();
            var tarifas = _context.Tarifas.ToList();

            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario");
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion");
            return View(await applicationDbContext.ToListAsync());
        }

        // ================================
        // Vista para usuarios familia
        // ================================
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> VerHijos()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized();

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Tarifa)
                .FirstOrDefaultAsync(f => f.UsuarioId == usuario.Id);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            return View("Index", familia.Estudiantes);
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

        // ================================
        // 2. Pagos pendientes
        // ================================
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> PagosPendientes()
        {
            var userId = _userManager.GetUserId(User);

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            var tarifasVigentes = await _context.Tarifas
                .Where(t => t.FechaInicio <= DateTime.Now && t.FechaFin >= DateTime.Now)
                .ToListAsync();

            var pagosPendientes = new List<(Estudiante estudiante, Tarifa tarifa)>();

            foreach (var estudiante in familia.Estudiantes)
            {
                foreach (var tarifa in tarifasVigentes)
                {
                    bool yaPagado = estudiante.Pagos.Any(p => p.FechaPago >= tarifa.FechaInicio && p.FechaPago <= tarifa.FechaFin);

                    if (!yaPagado)
                    {
                        pagosPendientes.Add((estudiante, tarifa));
                    }
                }
            }

            return View(pagosPendientes);
        }


        // ================================
        // Reportes: historial y deuda
        // ================================
        [Authorize(Roles = "Familia")]
        public async Task<IActionResult> Reportes()
        {
            var userId = _userManager.GetUserId(User);

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            var estudiantes = familia.Estudiantes;

            // Historial de pagos (simple)
            var historialPagos = estudiantes
                .SelectMany(e => e.Pagos.Select(p => new HistorialPagoItem
                {
                    Estudiante = e.NombreCompleto,
                    Fecha = p.FechaPago,
                }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // Tarifas pasadas (por ejemplo para deuda)
            var tarifas = await _context.Tarifas
                .Where(t => t.FechaFin < DateTime.Now)
                .ToListAsync();

            var deuda = new List<(Estudiante estudiante, Tarifa tarifa)>();

            foreach (var estudiante in estudiantes)
            {
                foreach (var tarifa in tarifas)
                {
                    bool pagado = estudiante.Pagos.Any(p =>
                        p.FechaPago >= tarifa.FechaInicio && p.FechaPago <= tarifa.FechaFin);

                    if (!pagado)
                    {
                        deuda.Add((estudiante, tarifa));
                    }
                }
            }

            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            // Pagos del mes actual
            var pagosMes = estudiantes
                .SelectMany(e => e.Pagos
                    .Where(p => p.FechaPago.Month == mesActual && p.FechaPago.Year == añoActual)
                    .Select(p => new HistorialPagoItem
                    {
                        Estudiante = e.NombreCompleto,
                        Fecha = p.FechaPago,
                    }))
                .OrderBy(p => p.Fecha)
                .ToList();

            var modelo = new ReporteViewModel
            {
                Historial = historialPagos,
                Deuda = deuda,
                PagosDelMes = pagosMes
            };

            return View(modelo);
        }


   
        public async Task<IActionResult> HistorialPagos(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Pagos)
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null) return NotFound();

            // Ordenar pagos por fecha
            estudiante.Pagos = estudiante.Pagos.OrderByDescending(p => p.FechaPago).ToList();

            return View(estudiante);
        }



        // ================================
        // CRUD (solo Admin)
        // ================================

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var familias = _context.Familias.Include(f => f.Usuario).ToList();
            var tarifas = _context.Tarifas.ToList();
            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario");
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EstudianteId,Nombre,FamiliaId,FechaInscripcion,TarifaId")] Estudiante estudiante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var familias = await _context.Familias.Include(f => f.Usuario).ToListAsync();
            var tarifas = await _context.Tarifas.ToListAsync();
            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario", estudiante.FamiliaId);
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion", estudiante.TarifaId);
            return View(estudiante);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            var familias = await _context.Familias.Include(f => f.Usuario).ToListAsync();
            var tarifas = await _context.Tarifas.ToListAsync();
            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario", estudiante.FamiliaId);
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion", estudiante.TarifaId);

            return View(estudiante);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("EstudianteId,Nombre,FamiliaId,FechaInscripcion,TarifaId")] Estudiante estudiante)
        {
            if (id != estudiante.EstudianteId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estudiante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstudianteExists(estudiante.EstudianteId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var familias = await _context.Familias.Include(f => f.Usuario).ToListAsync();
            var tarifas = await _context.Tarifas.ToListAsync();
            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario", estudiante.FamiliaId);
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion", estudiante.TarifaId);

            return View(estudiante);
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(m => m.EstudianteId == id);

            if (estudiante == null) return NotFound();

            return View(estudiante);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante != null)
            {
                _context.Estudiantes.Remove(estudiante);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.EstudianteId == id);
        }
    }
}



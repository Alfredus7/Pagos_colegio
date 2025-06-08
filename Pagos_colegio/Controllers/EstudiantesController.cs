using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;

namespace Pagos_colegio_web.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EstudiantesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================
        // 1. Listado General
        // ============================================

        // Vista principal para administradores
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Estudiantes.Include(e => e.Familia);
            var familias = _context.Familias.Include(f => f.Usuario).ToList();

            ViewBag.FamiliaId = new SelectList(
                familias,
                "FamiliaId",
                "NombreUsuario" // propiedad calculada
            );

            return View(await applicationDbContext.ToListAsync());
        }

        // Vista para usuarios con rol familia (solo ven sus hijos)
        public async Task<IActionResult> VerHijos()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized();

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                .FirstOrDefaultAsync(f => f.UsuarioId == usuario.Id);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            return View("Index", familia.Estudiantes);
        }

        // ============================================
        // 2. Consultas de pagos y deudas
        // ============================================

        // Consulta de pagos pendientes (HU-003)
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
                    bool yaPagado = estudiante.Pagos.Any(p => p.TarifaId == tarifa.TarifaId);
                    if (!yaPagado)
                    {
                        pagosPendientes.Add((estudiante, tarifa));
                    }
                }
            }

            return View(pagosPendientes);
        }

        // Reportes: historial, deuda acumulada y pagos del mes (HU-007)
        public async Task<IActionResult> Reportes()
        {
            var userId = _userManager.GetUserId(User);

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                        .ThenInclude(p => p.Tarifa)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
                return NotFound("No se encontró una familia asociada al usuario actual.");

            var estudiantes = familia.Estudiantes;

            // 1. Historial de pagos
            var historialPagos = estudiantes
                .SelectMany(e => e.Pagos.Select(p => new HistorialPagoItem
                {
                    Estudiante = e.NombreCompleto,
                    Fecha = p.FechaPago,
                    Periodo = p.Tarifa?.Periodo,
                    Monto = p.Tarifa?.Monto ?? 0
                }))
                .OrderBy(p => p.Fecha)
                .ToList();

            // 2. Deuda acumulada
            var tarifas = await _context.Tarifas
                .Where(t => t.FechaFin < DateTime.Now)
                .ToListAsync();

            var deuda = new List<(Estudiante estudiante, Tarifa tarifa)>();

            foreach (var estudiante in estudiantes)
            {
                foreach (var tarifa in tarifas)
                {
                    bool pagado = estudiante.Pagos.Any(p => p.TarifaId == tarifa.TarifaId);
                    if (!pagado)
                    {
                        deuda.Add((estudiante, tarifa));
                    }
                }
            }

            // 3. Pagos del mes actual
            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            var pagosMes = estudiantes
                .SelectMany(e => e.Pagos
                    .Where(p => p.FechaPago.Month == mesActual && p.FechaPago.Year == añoActual)
                    .Select(p => new HistorialPagoItem
                    {
                        Estudiante = e.NombreCompleto,
                        Fecha = p.FechaPago,
                        Periodo = p.Tarifa?.Periodo,
                        Monto = p.Tarifa?.Monto ?? 0
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

        // Historial de pagos por estudiante
        public async Task<IActionResult> HistorialPagos(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Pagos).ThenInclude(p => p.Tarifa)
                .Include(e => e.Pagos).ThenInclude(p => p.Recibo)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null) return NotFound();

            return View(estudiante);
        }

        // ============================================
        // 3. CRUD: Crear, Editar, Detalles y Eliminar
        // ============================================

        // GET: Estudiantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .FirstOrDefaultAsync(m => m.EstudianteId == id);

            if (estudiante == null) return NotFound();

            return View(estudiante);
        }

        // GET: Estudiantes/Create
        public IActionResult Create()
        {
            var familias = _context.Familias.Include(f => f.Usuario).ToList();

            ViewBag.FamiliaId = new SelectList(
                familias,
                "FamiliaId",
                "NombreUsuario"
            );

            return View();
        }

        // POST: Estudiantes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EstudianteId,Nombre,FamiliaId")] Estudiante estudiante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["FamiliaId"] = new SelectList(_context.Familias, "FamiliaId", "UsuarioId", estudiante.FamiliaId);
            return View(estudiante);
        }

        // GET: Estudiantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            ViewData["FamiliaId"] = new SelectList(_context.Familias, "FamiliaId", "UsuarioId", estudiante.FamiliaId);
            return View(estudiante);
        }

        // POST: Estudiantes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstudianteId,Nombre,FamiliaId")] Estudiante estudiante)
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
                    if (!EstudianteExists(estudiante.EstudianteId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["FamiliaId"] = new SelectList(_context.Familias, "FamiliaId", "UsuarioId", estudiante.FamiliaId);
            return View(estudiante);
        }

        // GET: Estudiantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .FirstOrDefaultAsync(m => m.EstudianteId == id);

            if (estudiante == null) return NotFound();

            return View(estudiante);
        }

        // POST: Estudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        // Utilidad: Verifica si existe el estudiante
        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.EstudianteId == id);
        }
    }
}

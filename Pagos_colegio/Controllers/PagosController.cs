using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Rotativa.AspNetCore;
using System.Diagnostics;

namespace Pagos_colegio_web.Controllers
{
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PagosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            // Cargamos Pago con Estudiante, pero no con Tarifa porque Pago no tiene relación directa
            var pagos = await _context.Pagos
                .Include(p => p.Estudiante)
                .ToListAsync();
            return View(pagos);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Incluimos Estudiante para poder obtener la tarifa luego
            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                .FirstOrDefaultAsync(m => m.PagoId == id);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto");
            return View();
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PagoId,FechaPago,EstudianteId")] Pago pago)
        {
            var estudiante = await _context.Estudiantes
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(e => e.EstudianteId == pago.EstudianteId);

            if (estudiante == null)
            {
                ModelState.AddModelError("EstudianteId", "Estudiante no encontrado.");
            }
            else if (estudiante.Tarifa == null)
            {
                ModelState.AddModelError("", "El estudiante no tiene una tarifa asignada.");
            }
            else
            {
                var tarifa = estudiante.Tarifa;
                Debug.WriteLine($"Monto: {tarifa.Monto}, Descuento: {estudiante.Descuento}");
                pago.TotalPago = Math.Round(tarifa.Monto * (1 - ((decimal)estudiante.Descuento / 100)), 2);

            }

            if (ModelState.IsValid)
            {
                _context.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto", pago.EstudianteId);
            return View(pago);
        }


        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null) return NotFound();

            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto", pago.EstudianteId);
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PagoId,FechaPago,EstudianteId,Descuento")] Pago pago)
        {
            if (id != pago.PagoId) return NotFound();

            if (ModelState.IsValid)
            {
                var estudiante = await _context.Estudiantes.FindAsync(pago.EstudianteId);
                if (estudiante == null)
                {
                    ModelState.AddModelError("EstudianteId", "Estudiante no encontrado");
                    ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto", pago.EstudianteId);
                    return View(pago);
                }

                var tarifa = await _context.Tarifas.FindAsync(estudiante.TarifaId);
                if (tarifa == null)
                {
                    ModelState.AddModelError("", "Tarifa no encontrada para el estudiante");
                    ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto", pago.EstudianteId);
                    return View(pago);
                }

                pago.TotalPago = Math.Round(tarifa.Monto * (1 - (estudiante.Descuento / 100)), 2);

                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.PagoId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "NombreCompleto", pago.EstudianteId);
            return View(pago);
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                .FirstOrDefaultAsync(m => m.PagoId == id);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.PagoId == id);
        }
    }
}
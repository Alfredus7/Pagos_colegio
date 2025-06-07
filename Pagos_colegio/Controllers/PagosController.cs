using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;

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
            var applicationDbContext = _context.Pagos.Include(p => p.Estudiante).Include(p => p.Tarifa);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                .Include(p => p.Tarifa)
                .FirstOrDefaultAsync(m => m.PagoId == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "Nombre");
            ViewData["TarifaId"] = new SelectList(_context.Tarifas, "TarifaId", "Monto");
            return View();
        }

        // POST: Pagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PagoId,FechaPago,EstudianteId,TarifaId")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "Nombre", pago.EstudianteId);
            ViewData["TarifaId"] = new SelectList(_context.Tarifas, "TarifaId", "Monto", pago.TarifaId);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "Nombre", pago.EstudianteId);
            ViewData["TarifaId"] = new SelectList(_context.Tarifas, "TarifaId", "Monto", pago.TarifaId);
            return View(pago);
        }

        // POST: Pagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PagoId,FechaPago,EstudianteId,TarifaId")] Pago pago)
        {
            if (id != pago.PagoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.PagoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EstudianteId"] = new SelectList(_context.Estudiantes, "EstudianteId", "Nombre", pago.EstudianteId);
            ViewData["TarifaId"] = new SelectList(_context.Tarifas, "TarifaId", "Monto", pago.TarifaId);
            return View(pago);
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Estudiante)
                .Include(p => p.Tarifa)
                .FirstOrDefaultAsync(m => m.PagoId == id);
            if (pago == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.PagoId == id);
        }

        public async Task<IActionResult> IndexFamilia()
        {
            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Buscar la familia asociada al usuario actual (UsuarioId == user.Id)
            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                .FirstOrDefaultAsync(f => f.UsuarioId == user.Id);

            if (familia == null)
                return NotFound("No se encontró una familia asociada a este usuario.");

            // Obtener los IDs de los estudiantes de esa familia
            var estudianteIds = familia.Estudiantes.Select(e => e.EstudianteId).ToList();

            // Filtrar pagos de los estudiantes de esa familia
            var pagos = await _context.Pagos
                .Include(p => p.Estudiante)
                    .ThenInclude(e => e.Familia) // Por si necesitas mostrar nombre completo
                .Include(p => p.Tarifa)
                .Include(p => p.Recibo)
                .Where(p => estudianteIds.Contains(p.EstudianteId))
                .ToListAsync();

            return View(pagos); // Asegúrate de tener la vista correspondiente
        }

    }
}

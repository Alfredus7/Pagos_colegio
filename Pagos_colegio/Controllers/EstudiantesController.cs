using Microsoft.AspNetCore.Authorization;
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

        public EstudiantesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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

        // CRUD para Admin

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



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio.Data;
using Pagos_colegio.Models;

namespace Pagos_colegio.Controllers
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

        private void CargarSelectLists()
        {
            var familias = _context.Familias.Include(f => f.Usuario).ToList();
            var tarifas = _context.Tarifas.ToList();

            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario");
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion");
        }
        // Acción solo para admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            CargarSelectLists();

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Tarifa)
                .ToListAsync();

            return View(estudiantes);
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

            // Puedes usar una vista específica como "MisHijos.cshtml"
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
        public async Task<IActionResult> Create([Bind("EstudianteId,Nombre,FamiliaId,FechaInscripcion,TarifaId,Descuento")] Estudiante estudiante)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("EstudianteId,Nombre,FamiliaId,FechaInscripcion,TarifaId,Descuento")] Estudiante estudiante)
        {
            if (id != estudiante.EstudianteId) return NotFound();

            // Obtener el estudiante actual de la base de datos
            var estudianteActual = await _context.Estudiantes
                .Include(e => e.Pagos)
                .Include(e => e.Tarifa)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudianteActual == null) return NotFound();

            // Verificar si se está cambiando la tarifa y si tiene pagos pendientes
            if (estudianteActual.TarifaId != estudiante.TarifaId)
            {
                var hoy = DateTime.Today;
                var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
                var tarifaActual = estudianteActual.Tarifa;

                if (tarifaActual != null)
                {
                    var inicio = MaxDate(
                        new DateTime(estudianteActual.FechaInscripcion.Year, estudianteActual.FechaInscripcion.Month, 1),
                        new DateTime(tarifaActual.FechaInicio.Year, tarifaActual.FechaInicio.Month, 1));

                    var fin = MinDate(
                        primerDiaMes,
                        new DateTime(tarifaActual.FechaFin.Year, tarifaActual.FechaFin.Month, 1));

                    // Verificar pagos pendientes
                    while (inicio <= fin)
                    {
                        var periodo = inicio.ToString("MM/yyyy");
                        if (!estudianteActual.Pagos.Any(p => p.Periodo == periodo))
                        {
                            ModelState.AddModelError("TarifaId", "No se puede cambiar la tarifa porque el estudiante tiene pagos pendientes.");
                            break;
                        }
                        inicio = inicio.AddMonths(1);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar solo los campos permitidos
                    estudianteActual.Nombre = estudiante.Nombre;
                    estudianteActual.FamiliaId = estudiante.FamiliaId;
                    estudianteActual.FechaInscripcion = estudiante.FechaInscripcion;
                    estudianteActual.Descuento = estudiante.Descuento;

                    // Solo actualizar TarifaId si no hay pagos pendientes
                    if (!ModelState.ContainsKey("TarifaId"))
                    {
                        estudianteActual.TarifaId = estudiante.TarifaId;
                    }

                    _context.Update(estudianteActual);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstudianteExists(estudiante.EstudianteId)) return NotFound();
                    else throw;
                }
            }

            // Cargar datos para el dropdown
            var familias = await _context.Familias.Include(f => f.Usuario).ToListAsync();
            var tarifas = await _context.Tarifas.ToListAsync();
            ViewBag.FamiliaId = new SelectList(familias, "FamiliaId", "NombreUsuario", estudiante.FamiliaId);
            ViewBag.TarifaId = new SelectList(tarifas, "TarifaId", "Gestion", estudiante.TarifaId);

            return View(estudiante);
        }

        // Métodos auxiliares (deberían estar en algún helper)
        private DateTime MaxDate(DateTime date1, DateTime date2) => date1 > date2 ? date1 : date2;
        private DateTime MinDate(DateTime date1, DateTime date2) => date1 < date2 ? date1 : date2;

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



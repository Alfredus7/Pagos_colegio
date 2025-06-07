using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Pagos_colegio_web.ViewModels;

namespace Pagos_colegio_web.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inyecta UserManager en tu constructor si no lo tienes ya:
        private readonly UserManager<IdentityUser> _userManager;

        public EstudiantesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Estudiantes

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

        public async Task<IActionResult> VerHijos()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Unauthorized();
            }

            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                .FirstOrDefaultAsync(f => f.UsuarioId == usuario.Id);

            if (familia == null)
            {
                return NotFound("No se encontró una familia asociada al usuario actual.");
            }

            return View("Index", familia.Estudiantes);
        }


        // GET: Estudiantes/PagosPendientes
        public async Task<IActionResult> PagosPendientes()
        {
            // 1. Obtener el usuario actual
            var userId = _userManager.GetUserId(User);

            // 2. Buscar la familia del usuario
            var familia = await _context.Familias
                .Include(f => f.Estudiantes)
                    .ThenInclude(e => e.Pagos)
                .FirstOrDefaultAsync(f => f.UsuarioId == userId);

            if (familia == null)
            {
                return NotFound("No se encontró una familia asociada al usuario actual.");
            }

            // 3. Obtener tarifas vigentes
            var tarifasVigentes = await _context.Tarifas
                .Where(t => t.FechaInicio <= DateTime.Now && t.FechaFin >= DateTime.Now)
                .ToListAsync();

            // 4. Determinar los pagos pendientes
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

        // GET: Estudiantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .FirstOrDefaultAsync(m => m.EstudianteId == id);
            if (estudiante == null)
            {
                return NotFound();
            }

            return View(estudiante);
        }

        // GET: Estudiantes/Create
        public IActionResult Create()
        {
            var familias = _context.Familias.Include(f => f.Usuario).ToList();

            ViewBag.FamiliaId = new SelectList(
                familias,
                "FamiliaId",
                "NombreUsuario" // propiedad calculada
            );

            return View();
        }


        // POST: Estudiantes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null)
            {
                return NotFound();
            }
            ViewData["FamiliaId"] = new SelectList(_context.Familias, "FamiliaId", "UsuarioId", estudiante.FamiliaId);
            return View(estudiante);
        }

        // POST: Estudiantes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstudianteId,Nombre,FamiliaId")] Estudiante estudiante)
        {
            if (id != estudiante.EstudianteId)
            {
                return NotFound();
            }

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
            ViewData["FamiliaId"] = new SelectList(_context.Familias, "FamiliaId", "UsuarioId", estudiante.FamiliaId);
            return View(estudiante);
        }

        // GET: Estudiantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .FirstOrDefaultAsync(m => m.EstudianteId == id);
            if (estudiante == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.EstudianteId == id);
        }

        public async Task<IActionResult> HistorialPagos(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Familia)
                .Include(e => e.Pagos)
                    .ThenInclude(p => p.Tarifa)
                .Include(e => e.Pagos)
                    .ThenInclude(p => p.Recibo)
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null) return NotFound();

            return View(estudiante);
        }

    }
}

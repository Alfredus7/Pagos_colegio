using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio.Data;
using Pagos_colegio.Models;
using Pagos_colegio.ViewModel;

namespace Pagos_colegio.Controllers
{
    public class FamiliasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FamiliasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Familias
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var familias = await _context.Familias
                .Include(f => f.Usuario)
                .Select(f => new FamiliaUsuarioViewModel
                {
                    FamiliaId = f.FamiliaId,
                    ApellidoPaterno = f.ApellidoPaterno,
                    ApellidoMaterno = f.ApellidoMaterno,
                    NombreUsuario = f.Usuario.UserName,
                    Email = f.Usuario.Email
                })
                .ToListAsync();

            return View(familias);
        }

        // GET: Familias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familia = await _context.Familias
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(m => m.FamiliaId == id);
            if (familia == null)
            {
                return NotFound();
            }

            return View(familia);
        }

        // GET: Familias/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Familias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamiliaUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Crear usuario
                var usuario = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(usuario, model.Password);

                if (result.Succeeded)
                {
                    // 2. Asignar rol "Familia"
                    await _userManager.AddToRoleAsync(usuario, "Familia");

                    // 3. Crear familia y vincular usuario
                    var familia = new Familia
                    {
                        ApellidoMaterno = model.ApellidoMaterno,
                        ApellidoPaterno = model.ApellidoPaterno,
                        UsuarioId = usuario.Id
                    };

                    _context.Add(familia);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Si hubo errores al crear el usuario
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }



        // GET: Familias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familia = await _context.Familias.FindAsync(id);
            if (familia == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id", familia.UsuarioId);
            return View(familia);
        }

        // POST: Familias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FamiliaId,ApellidoMaterno,ApellidoPaterno,UsuarioId")] Familia familia)
        {
            if (id != familia.FamiliaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(familia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamiliaExists(familia.FamiliaId))
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
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id", familia.UsuarioId);
            return View(familia);
        }

        // GET: Familias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var familia = await _context.Familias
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(m => m.FamiliaId == id);
            if (familia == null)
            {
                return NotFound();
            }

            return View(familia);
        }

        // POST: Familias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var familia = await _context.Familias.FindAsync(id);
            if (familia != null)
            {
                _context.Familias.Remove(familia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FamiliaExists(int id)
        {
            return _context.Familias.Any(e => e.FamiliaId == id);
        }

    }
}

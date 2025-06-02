using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;
using Pagos_colegio_web.ViewModels;

namespace Pagos_colegio_web.Controllers
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
        public async Task<IActionResult> Index()
        {
            var familias = _context.Familias.Include(f => f.Usuario);
            return View(await familias.ToListAsync());
        }

        // GET: Familias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var familia = await _context.Familias
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(m => m.ID_FAMILIA == id);

            if (familia == null) return NotFound();

            return View(familia);
        }

        // GET: Familias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Familias/Create (Crea usuario + familia)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamiliaUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crear usuario
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Crear familia asociada al usuario
                    var familia = new Familia
                    {
                        ApellidoMaterno = model.ApellidoMaterno,
                        ApellidoPaterno = model.ApellidoPaterno,
                        UserId = user.Id
                    };

                    _context.Familias.Add(familia);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

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
            if (id == null) return NotFound();

            var familia = await _context.Familias.FindAsync(id);
            if (familia == null) return NotFound();

            return View(familia);
        }

        // POST: Familias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_FAMILIA,ApellidoMaterno,ApellidoPaterno,UserId")] Familia familia)
        {
            if (id != familia.ID_FAMILIA) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(familia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamiliaExists(familia.ID_FAMILIA))
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

            return View(familia);
        }

        // GET: Familias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var familia = await _context.Familias
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(m => m.ID_FAMILIA == id);

            if (familia == null) return NotFound();

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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FamiliaExists(int id)
        {
            return _context.Familias.Any(e => e.ID_FAMILIA == id);
        }
    }
}

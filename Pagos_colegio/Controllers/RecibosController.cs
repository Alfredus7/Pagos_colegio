using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Pagos_colegio_web.Models;

namespace Pagos_colegio_web.Controllers
{
    public class RecibosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecibosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Recibos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Recibos.Include(r => r.Pago);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Recibos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recibo = await _context.Recibos
                .Include(r => r.Pago)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recibo == null)
            {
                return NotFound();
            }

            return View(recibo);
        }

        // GET: Recibos/Create
        public IActionResult Create()
        {
            ViewData["ID_PAGO"] = new SelectList(_context.Pagos, "PagoId", "PagoId");
            return View();
        }

        // POST: Recibos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Descripcion,ID_PAGO")] Recibo recibo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recibo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ID_PAGO"] = new SelectList(_context.Pagos, "PagoId", "PagoId", recibo.ID_PAGO);
            return View(recibo);
        }

        // GET: Recibos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recibo = await _context.Recibos.FindAsync(id);
            if (recibo == null)
            {
                return NotFound();
            }
            ViewData["ID_PAGO"] = new SelectList(_context.Pagos, "PagoId", "PagoId", recibo.ID_PAGO);
            return View(recibo);
        }

        // POST: Recibos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Descripcion,ID_PAGO")] Recibo recibo)
        {
            if (id != recibo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recibo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReciboExists(recibo.ID))
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
            ViewData["ID_PAGO"] = new SelectList(_context.Pagos, "PagoId", "PagoId", recibo.ID_PAGO);
            return View(recibo);
        }

        // GET: Recibos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recibo = await _context.Recibos
                .Include(r => r.Pago)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recibo == null)
            {
                return NotFound();
            }

            return View(recibo);
        }

        // POST: Recibos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recibo = await _context.Recibos.FindAsync(id);
            if (recibo != null)
            {
                _context.Recibos.Remove(recibo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReciboExists(int id)
        {
            return _context.Recibos.Any(e => e.ID == id);
        }
    }
}

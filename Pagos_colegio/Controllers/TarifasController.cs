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
    public class TarifasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TarifasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tarifas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tarifas.ToListAsync());
        }

        // GET: Tarifas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(m => m.TarifaId == id);
            if (tarifa == null)
            {
                return NotFound();
            }

            return View(tarifa);
        }

        // GET: Tarifas/Create
        public IActionResult Create()
        {
            // Calcular cuántas gestiones hay del mismo año
            int anioActual = DateTime.Now.Year;
            int conteo = _context.Tarifas
                .Count(t => t.FechaInicio.Year == anioActual);

            var tarifa = new Tarifa
            {
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(1),
                Gestion = $"Gestión {conteo + 1} - {anioActual}"
            };

            return View(tarifa);
        }


        // POST: Tarifas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TarifaId,FechaInicio,FechaFin,Monto")] Tarifa tarifa)
        {
            // Aseguramos que FechaInicio sea el primer día del mes
            tarifa.FechaInicio = new DateTime(tarifa.FechaInicio.Year, tarifa.FechaInicio.Month, 1);

            // Definimos una "fecha nula" para las tarifas vigentes
            DateTime fechaNula = new DateTime(1, 1, 1);
            tarifa.FechaFin = fechaNula;

            // Buscar la tarifa vigente (FechaFin = fechaNula) para actualizar su FechaFin
            var tarifaAnterior = await _context.Tarifas.FirstOrDefaultAsync(t => t.FechaFin == fechaNula);

            if (tarifaAnterior != null)
            {
                // FechaFin de la tarifa anterior será el último día del mes anterior a la nueva tarifa
                tarifaAnterior.FechaFin = tarifa.FechaInicio.AddDays(-1);
                _context.Update(tarifaAnterior);
            }

            // Calcular la gestión automáticamente con el año de la nueva tarifa
            int anio = tarifa.FechaInicio.Year;
            int cantidadGestiones = await _context.Tarifas.CountAsync(t => t.FechaInicio.Year == anio);

            tarifa.Gestion = $"Gestión {cantidadGestiones + 1} {anio}";

            if (!ModelState.IsValid)
                return View(tarifa);

            _context.Add(tarifa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }






        // GET: Tarifas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifa = await _context.Tarifas.FindAsync(id);
            if (tarifa == null)
            {
                return NotFound();
            }
            return View(tarifa);
        }

        // POST: Tarifas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TarifaId,Gestion,FechaInicio,FechaFin,Monto")] Tarifa tarifa)
        {
            if (id != tarifa.TarifaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarifa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarifaExists(tarifa.TarifaId))
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
            return View(tarifa);
        }

        // GET: Tarifas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(m => m.TarifaId == id);
            if (tarifa == null)
            {
                return NotFound();
            }

            return View(tarifa);
        }

        // POST: Tarifas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tarifa = await _context.Tarifas.FindAsync(id);
            if (tarifa != null)
            {
                _context.Tarifas.Remove(tarifa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TarifaExists(int id)
        {
            return _context.Tarifas.Any(e => e.TarifaId == id);
        }
    }
}

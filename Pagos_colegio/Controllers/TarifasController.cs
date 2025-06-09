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
          

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FechaInicio,FechaFin,Monto")] Tarifa tarifa)
        {
            // Normalizamos: FechaInicio al inicio del día, FechaFin al final
            tarifa.FechaInicio = tarifa.FechaInicio.Date;
            tarifa.FechaFin = tarifa.FechaFin.Date;

            // Validación de fechas
            if (tarifa.FechaFin <= tarifa.FechaInicio)
            {
                ModelState.AddModelError(string.Empty, "La fecha de fin debe ser posterior a la fecha de inicio.");
                return View(tarifa);
            }

            // Validar solapamientos: inicio o fin dentro de alguna gestión existente
            bool solapa = await _context.Tarifas.AnyAsync(t =>
                (tarifa.FechaInicio <= t.FechaFin && tarifa.FechaFin >= t.FechaInicio));

            if (solapa)
            {
                ModelState.AddModelError(string.Empty, "Ya existe una tarifa entre ese rango de fechas ingresado.");
                return View(tarifa);
            }

            // Asignar nombre automático a la gestión
            string nombreMesInicio = tarifa.FechaInicio.ToString("MMM");
            string nombreMesFin = tarifa.FechaFin.ToString("MMM");
            int anio = tarifa.FechaInicio.Year;
            tarifa.Gestion = $"Gestión {anio} ({nombreMesInicio} - {nombreMesFin})";

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

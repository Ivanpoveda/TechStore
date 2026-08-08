using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class GarantiaController : Controller
    {
        private readonly TechStoreContext _context;

        public GarantiaController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: Garantia
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.Garantia.Include(g => g.IdDetalleVentaNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: Garantia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantium = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                .FirstOrDefaultAsync(m => m.IdGarantia == id);
            if (garantium == null)
            {
                return NotFound();
            }

            return View(garantium);
        }

        // GET: Garantia/Create
        public IActionResult Create()
        {
            ViewData["IdDetalleVenta"] = new SelectList(_context.DetalleVenta, "IdDetalleVenta", "IdDetalleVenta");
            return View();
        }

        // POST: Garantia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdGarantia,FechaSolicitud,Motivo,Descripcion,Estado,FechaResolucion,IdDetalleVenta")] Garantium garantium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(garantium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDetalleVenta"] = new SelectList(_context.DetalleVenta, "IdDetalleVenta", "IdDetalleVenta", garantium.IdDetalleVenta);
            return View(garantium);
        }

        // GET: Garantia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantium = await _context.Garantia.FindAsync(id);
            if (garantium == null)
            {
                return NotFound();
            }
            ViewData["IdDetalleVenta"] = new SelectList(_context.DetalleVenta, "IdDetalleVenta", "IdDetalleVenta", garantium.IdDetalleVenta);
            return View(garantium);
        }

        // POST: Garantia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGarantia,FechaSolicitud,Motivo,Descripcion,Estado,FechaResolucion,IdDetalleVenta")] Garantium garantium)
        {
            if (id != garantium.IdGarantia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(garantium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GarantiumExists(garantium.IdGarantia))
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
            ViewData["IdDetalleVenta"] = new SelectList(_context.DetalleVenta, "IdDetalleVenta", "IdDetalleVenta", garantium.IdDetalleVenta);
            return View(garantium);
        }

        // GET: Garantia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantium = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                .FirstOrDefaultAsync(m => m.IdGarantia == id);
            if (garantium == null)
            {
                return NotFound();
            }

            return View(garantium);
        }

        // POST: Garantia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var garantium = await _context.Garantia.FindAsync(id);
            if (garantium != null)
            {
                _context.Garantia.Remove(garantium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GarantiumExists(int id)
        {
            return _context.Garantia.Any(e => e.IdGarantia == id);
        }
    }
}

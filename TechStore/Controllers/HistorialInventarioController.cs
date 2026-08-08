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
    public class HistorialInventarioController : Controller
    {
        private readonly TechStoreContext _context;

        public HistorialInventarioController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: HistorialInventario
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.HistorialInventarios.Include(h => h.IdProductoNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: HistorialInventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialInventario = await _context.HistorialInventarios
                .Include(h => h.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (historialInventario == null)
            {
                return NotFound();
            }

            return View(historialInventario);
        }

        // GET: HistorialInventario/Create
        public IActionResult Create()
        {
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            return View();
        }

        // POST: HistorialInventario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMovimiento,TipoMovimiento,Cantidad,Fecha,Observacion,IdProducto")] HistorialInventario historialInventario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(historialInventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", historialInventario.IdProducto);
            return View(historialInventario);
        }

        // GET: HistorialInventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialInventario = await _context.HistorialInventarios.FindAsync(id);
            if (historialInventario == null)
            {
                return NotFound();
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", historialInventario.IdProducto);
            return View(historialInventario);
        }

        // POST: HistorialInventario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMovimiento,TipoMovimiento,Cantidad,Fecha,Observacion,IdProducto")] HistorialInventario historialInventario)
        {
            if (id != historialInventario.IdMovimiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(historialInventario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HistorialInventarioExists(historialInventario.IdMovimiento))
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
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", historialInventario.IdProducto);
            return View(historialInventario);
        }

        // GET: HistorialInventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialInventario = await _context.HistorialInventarios
                .Include(h => h.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (historialInventario == null)
            {
                return NotFound();
            }

            return View(historialInventario);
        }

        // POST: HistorialInventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historialInventario = await _context.HistorialInventarios.FindAsync(id);
            if (historialInventario != null)
            {
                _context.HistorialInventarios.Remove(historialInventario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HistorialInventarioExists(int id)
        {
            return _context.HistorialInventarios.Any(e => e.IdMovimiento == id);
        }
    }
}

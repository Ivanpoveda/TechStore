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
    public class DetalleVentaController : Controller
    {
        private readonly TechStoreContext _context;

        public DetalleVentaController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: DetalleVenta
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.DetalleVenta.Include(d => d.IdProductoNavigation).Include(d => d.IdVentaNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: DetalleVenta/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVentum = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleVenta == id);
            if (detalleVentum == null)
            {
                return NotFound();
            }

            return View(detalleVentum);
        }

        // GET: DetalleVenta/Create
        public IActionResult Create()
        {
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            ViewData["IdVenta"] = new SelectList(_context.Venta, "IdVenta", "IdVenta");
            return View();
        }

        // POST: DetalleVenta/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleVenta,Cantidad,PrecioUnitario,Subtotal,IdVenta,IdProducto")] DetalleVentum detalleVentum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleVentum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleVentum.IdProducto);
            ViewData["IdVenta"] = new SelectList(_context.Venta, "IdVenta", "IdVenta", detalleVentum.IdVenta);
            return View(detalleVentum);
        }

        // GET: DetalleVenta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVentum = await _context.DetalleVenta.FindAsync(id);
            if (detalleVentum == null)
            {
                return NotFound();
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleVentum.IdProducto);
            ViewData["IdVenta"] = new SelectList(_context.Venta, "IdVenta", "IdVenta", detalleVentum.IdVenta);
            return View(detalleVentum);
        }

        // POST: DetalleVenta/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleVenta,Cantidad,PrecioUnitario,Subtotal,IdVenta,IdProducto")] DetalleVentum detalleVentum)
        {
            if (id != detalleVentum.IdDetalleVenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleVentum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleVentumExists(detalleVentum.IdDetalleVenta))
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
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleVentum.IdProducto);
            ViewData["IdVenta"] = new SelectList(_context.Venta, "IdVenta", "IdVenta", detalleVentum.IdVenta);
            return View(detalleVentum);
        }

        // GET: DetalleVenta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVentum = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleVenta == id);
            if (detalleVentum == null)
            {
                return NotFound();
            }

            return View(detalleVentum);
        }

        // POST: DetalleVenta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleVentum = await _context.DetalleVenta.FindAsync(id);
            if (detalleVentum != null)
            {
                _context.DetalleVenta.Remove(detalleVentum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleVentumExists(int id)
        {
            return _context.DetalleVenta.Any(e => e.IdDetalleVenta == id);
        }
    }
}

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
    public class DetalleCompraController : Controller
    {
        private readonly TechStoreContext _context;

        public DetalleCompraController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: DetalleCompra
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.DetalleCompras.Include(d => d.IdCompraNavigation).Include(d => d.IdProductoNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: DetalleCompra/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetalleCompras
                .Include(d => d.IdCompraNavigation)
                .Include(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleCompra == id);
            if (detalleCompra == null)
            {
                return NotFound();
            }

            return View(detalleCompra);
        }

        // GET: DetalleCompra/Create
        public IActionResult Create()
        {
            ViewData["IdCompra"] = new SelectList(_context.CompraProveedors, "IdCompra", "IdCompra");
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            return View();
        }

        // POST: DetalleCompra/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleCompra,Cantidad,PrecioCompra,Subtotal,IdCompra,IdProducto")] DetalleCompra detalleCompra)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleCompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCompra"] = new SelectList(_context.CompraProveedors, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCompra.IdProducto);
            return View(detalleCompra);
        }

        // GET: DetalleCompra/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetalleCompras.FindAsync(id);
            if (detalleCompra == null)
            {
                return NotFound();
            }
            ViewData["IdCompra"] = new SelectList(_context.CompraProveedors, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCompra.IdProducto);
            return View(detalleCompra);
        }

        // POST: DetalleCompra/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleCompra,Cantidad,PrecioCompra,Subtotal,IdCompra,IdProducto")] DetalleCompra detalleCompra)
        {
            if (id != detalleCompra.IdDetalleCompra)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleCompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleCompraExists(detalleCompra.IdDetalleCompra))
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
            ViewData["IdCompra"] = new SelectList(_context.CompraProveedors, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCompra.IdProducto);
            return View(detalleCompra);
        }

        // GET: DetalleCompra/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetalleCompras
                .Include(d => d.IdCompraNavigation)
                .Include(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleCompra == id);
            if (detalleCompra == null)
            {
                return NotFound();
            }

            return View(detalleCompra);
        }

        // POST: DetalleCompra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleCompra = await _context.DetalleCompras.FindAsync(id);
            if (detalleCompra != null)
            {
                _context.DetalleCompras.Remove(detalleCompra);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleCompraExists(int id)
        {
            return _context.DetalleCompras.Any(e => e.IdDetalleCompra == id);
        }
    }
}

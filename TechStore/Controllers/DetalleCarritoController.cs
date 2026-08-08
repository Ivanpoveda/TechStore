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
    public class DetalleCarritoController : Controller
    {
        private readonly TechStoreContext _context;

        public DetalleCarritoController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: DetalleCarrito
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.DetalleCarritos.Include(d => d.IdCarritoNavigation).Include(d => d.IdProductoNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: DetalleCarrito/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCarrito = await _context.DetalleCarritos
                .Include(d => d.IdCarritoNavigation)
                .Include(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleCarrito == id);
            if (detalleCarrito == null)
            {
                return NotFound();
            }

            return View(detalleCarrito);
        }

        // GET: DetalleCarrito/Create
        public IActionResult Create()
        {
            ViewData["IdCarrito"] = new SelectList(_context.Carritos, "IdCarrito", "IdCarrito");
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            return View();
        }

        // POST: DetalleCarrito/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleCarrito,Cantidad,IdCarrito,IdProducto")] DetalleCarrito detalleCarrito)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleCarrito);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCarrito"] = new SelectList(_context.Carritos, "IdCarrito", "IdCarrito", detalleCarrito.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCarrito.IdProducto);
            return View(detalleCarrito);
        }

        // GET: DetalleCarrito/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCarrito = await _context.DetalleCarritos.FindAsync(id);
            if (detalleCarrito == null)
            {
                return NotFound();
            }
            ViewData["IdCarrito"] = new SelectList(_context.Carritos, "IdCarrito", "IdCarrito", detalleCarrito.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCarrito.IdProducto);
            return View(detalleCarrito);
        }

        // POST: DetalleCarrito/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleCarrito,Cantidad,IdCarrito,IdProducto")] DetalleCarrito detalleCarrito)
        {
            if (id != detalleCarrito.IdDetalleCarrito)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleCarrito);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleCarritoExists(detalleCarrito.IdDetalleCarrito))
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
            ViewData["IdCarrito"] = new SelectList(_context.Carritos, "IdCarrito", "IdCarrito", detalleCarrito.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", detalleCarrito.IdProducto);
            return View(detalleCarrito);
        }

        // GET: DetalleCarrito/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCarrito = await _context.DetalleCarritos
                .Include(d => d.IdCarritoNavigation)
                .Include(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalleCarrito == id);
            if (detalleCarrito == null)
            {
                return NotFound();
            }

            return View(detalleCarrito);
        }

        // POST: DetalleCarrito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleCarrito = await _context.DetalleCarritos.FindAsync(id);
            if (detalleCarrito != null)
            {
                _context.DetalleCarritos.Remove(detalleCarrito);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleCarritoExists(int id)
        {
            return _context.DetalleCarritos.Any(e => e.IdDetalleCarrito == id);
        }
    }
}

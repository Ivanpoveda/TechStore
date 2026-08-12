using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class CompraProveedorController : Controller
    {
        private readonly TechStoreContext _context;

        public CompraProveedorController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: CompraProveedor
        public async Task<IActionResult> Index()
        {
            var techStoreContext = _context.CompraProveedors.Include(c => c.IdProveedorNavigation);
            return View(await techStoreContext.ToListAsync());
        }

        // GET: CompraProveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compraProveedor = await _context.CompraProveedors
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compraProveedor == null)
            {
                return NotFound();
            }

            return View(compraProveedor);
        }

        // GET: CompraProveedor/Create
        public IActionResult Create()
        {
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: CompraProveedor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCompra,FechaCompra,Total,Estado,IdProveedor")] CompraProveedor compraProveedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(compraProveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compraProveedor.IdProveedor);
            return View(compraProveedor);
        }

        // GET: CompraProveedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compraProveedor = await _context.CompraProveedors.FindAsync(id);
            if (compraProveedor == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compraProveedor.IdProveedor);
            return View(compraProveedor);
        }

        // POST: CompraProveedor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCompra,FechaCompra,Total,Estado,IdProveedor")] CompraProveedor compraProveedor)
        {
            if (id != compraProveedor.IdCompra)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compraProveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompraProveedorExists(compraProveedor.IdCompra))
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
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compraProveedor.IdProveedor);
            return View(compraProveedor);
        }

        // GET: CompraProveedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compraProveedor = await _context.CompraProveedors
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compraProveedor == null)
            {
                return NotFound();
            }

            return View(compraProveedor);
        }

        // POST: CompraProveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compraProveedor = await _context.CompraProveedors.FindAsync(id);
            if (compraProveedor != null)
            {
                _context.CompraProveedors.Remove(compraProveedor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompraProveedorExists(int id)
        {
            return _context.CompraProveedors.Any(e => e.IdCompra == id);
        }
    }
}

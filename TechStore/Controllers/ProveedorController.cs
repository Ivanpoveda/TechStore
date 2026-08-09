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
    public class ProveedorController : Controller
    {
        private readonly TechStoreContext _context;

        public ProveedorController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Proveedors.ToListAsync());
        }

        // GET: Proveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Proveedor());
        }


        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Nombre,
            string? Empresa,
            string? Telefono,
            string? Correo,
            string? SitioWeb)
        {
            try
            {
                // =============================================
                // VALIDAR NOMBRE
                // =============================================
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    TempData["Error"] =
                        "El nombre del proveedor es obligatorio.";

                    return View(new Proveedor
                    {
                        Nombre = Nombre,
                        Empresa = Empresa,
                        Telefono = Telefono,
                        Correo = Correo,
                        SitioWeb = SitioWeb
                    });
                }

                // =============================================
                // CREAR PROVEEDOR
                // =============================================
                var proveedor = new Proveedor
                {
                    Nombre = Nombre.Trim(),
                    Empresa = string.IsNullOrWhiteSpace(Empresa)
                        ? null
                        : Empresa.Trim(),

                    Telefono = string.IsNullOrWhiteSpace(Telefono)
                        ? null
                        : Telefono.Trim(),

                    Correo = string.IsNullOrWhiteSpace(Correo)
                        ? null
                        : Correo.Trim(),

                    SitioWeb = string.IsNullOrWhiteSpace(SitioWeb)
                        ? null
                        : SitioWeb.Trim()
                };

                // =============================================
                // INSERTAR
                // =============================================
                _context.Proveedors.Add(proveedor);

                int filas = await _context.SaveChangesAsync();

                // =============================================
                // CONFIRMAR
                // =============================================
                if (filas > 0)
                {
                    TempData["Success"] =
                        $"El proveedor '{proveedor.Nombre}' fue registrado correctamente.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] =
                    "No se pudo registrar el proveedor.";

                return View(proveedor);
            }
            catch (DbUpdateException ex)
            {
                string mensaje =
                    ex.InnerException?.Message ?? ex.Message;

                TempData["Error"] =
                    "No se pudo registrar el proveedor: " + mensaje;

                return View(new Proveedor
                {
                    Nombre = Nombre,
                    Empresa = Empresa,
                    Telefono = Telefono,
                    Correo = Correo,
                    SitioWeb = SitioWeb
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Ocurrió un error: " + ex.Message;

                return View(new Proveedor
                {
                    Nombre = Nombre,
                    Empresa = Empresa,
                    Telefono = Telefono,
                    Correo = Correo,
                    SitioWeb = SitioWeb
                });
            }
        }

        // GET: Proveedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,Nombre,Empresa,Telefono,Correo,SitioWeb")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.IdProveedor))
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
            return View(proveedor);
        }

        // GET: Proveedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor != null)
            {
                _context.Proveedors.Remove(proveedor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedors.Any(e => e.IdProveedor == id);
        }
    }
}

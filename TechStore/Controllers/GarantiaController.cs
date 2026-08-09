using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // =========================================================
        // INDEX
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var garantias = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)
                        .ThenInclude(v => v.IdUsuarioNavigation)
                .OrderByDescending(g => g.IdGarantia)
                .ToListAsync();

            return View(garantias);
        }

        // =========================================================
        // DETAILS
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantia = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)
                        .ThenInclude(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(g =>
                    g.IdGarantia == id);

            if (garantia == null)
            {
                return NotFound();
            }

            return View(garantia);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create(int? idDetalleVenta)
        {
            await CargarDetallesVenta(idDetalleVenta);

            var modelo = new Garantium
            {
                FechaSolicitud = DateTime.Now,
                Estado = "En proceso"
            };

            if (idDetalleVenta.HasValue)
            {
                var detalle = await _context.DetalleVenta
                    .Include(d => d.IdProductoNavigation)
                    .Include(d => d.IdVentaNavigation)
                    .FirstOrDefaultAsync(d =>
                        d.IdDetalleVenta == idDetalleVenta.Value);

                if (detalle == null)
                {
                    return NotFound();
                }

                modelo.IdDetalleVenta = idDetalleVenta.Value;
            }

            return View(modelo);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Motivo,
            string Descripcion,
            int IdDetalleVenta)
        {
            try
            {
                // =================================================
                // VALIDAR DETALLE
                // =================================================

                var detalle = await _context.DetalleVenta
                    .Include(d => d.IdProductoNavigation)
                    .Include(d => d.IdVentaNavigation)
                    .FirstOrDefaultAsync(d =>
                        d.IdDetalleVenta == IdDetalleVenta);

                if (detalle == null)
                {
                    TempData["Error"] =
                        "El detalle de venta seleccionado no existe.";

                    await CargarDetallesVenta(IdDetalleVenta);

                    return View(new Garantium
                    {
                        Motivo = Motivo,
                        Descripcion = Descripcion,
                        IdDetalleVenta = IdDetalleVenta,
                        FechaSolicitud = DateTime.Now,
                        Estado = "En proceso"
                    });
                }

                // =================================================
                // VALIDAR CAMPOS
                // =================================================

                if (string.IsNullOrWhiteSpace(Motivo))
                {
                    TempData["Error"] =
                        "Debe indicar el motivo de la garantía.";

                    await CargarDetallesVenta(IdDetalleVenta);

                    return View(new Garantium
                    {
                        Motivo = Motivo,
                        Descripcion = Descripcion,
                        IdDetalleVenta = IdDetalleVenta,
                        FechaSolicitud = DateTime.Now,
                        Estado = "En proceso"
                    });
                }

                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    TempData["Error"] =
                        "Debe proporcionar una descripción del problema.";

                    await CargarDetallesVenta(IdDetalleVenta);

                    return View(new Garantium
                    {
                        Motivo = Motivo,
                        Descripcion = Descripcion,
                        IdDetalleVenta = IdDetalleVenta,
                        FechaSolicitud = DateTime.Now,
                        Estado = "En proceso"
                    });
                }

                // =================================================
                // CREAR GARANTÍA
                // =================================================

                var garantia = new Garantium
                {
                    FechaSolicitud = DateTime.Now,

                    Motivo = Motivo.Trim(),

                    Descripcion = Descripcion.Trim(),

                    // Estado permitido por SQL Server
                    Estado = "En proceso",

                    FechaResolucion = null,

                    IdDetalleVenta = IdDetalleVenta
                };

                _context.Garantia.Add(garantia);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La garantía fue registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo registrar la garantía: " +
                    ObtenerMensajeError(ex);

                await CargarDetallesVenta(IdDetalleVenta);

                return View(new Garantium
                {
                    Motivo = Motivo,
                    Descripcion = Descripcion,
                    IdDetalleVenta = IdDetalleVenta,
                    FechaSolicitud = DateTime.Now,
                    Estado = "En proceso"
                });
            }
        }

        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantia = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)
                .FirstOrDefaultAsync(g =>
                    g.IdGarantia == id);

            if (garantia == null)
            {
                return NotFound();
            }

            return View(garantia);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int IdGarantia,
            string Motivo,
            string Descripcion,
            string Estado,
            DateTime? FechaResolucion)
        {
            try
            {
                var garantia = await _context.Garantia
                    .FirstOrDefaultAsync(g =>
                        g.IdGarantia == IdGarantia);

                if (garantia == null)
                {
                    return NotFound();
                }

                // =================================================
                // VALIDAR ESTADO
                // =================================================

                if (Estado != "En proceso" &&
                    Estado != "Aprobada" &&
                    Estado != "Rechazada")
                {
                    TempData["Error"] =
                        "El estado seleccionado no es válido.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = IdGarantia });
                }

                // =================================================
                // ACTUALIZAR DATOS
                // =================================================

                garantia.Motivo =
                    Motivo?.Trim();

                garantia.Descripcion =
                    Descripcion?.Trim();

                garantia.Estado =
                    Estado;

                // =================================================
                // FECHA DE RESOLUCIÓN
                // =================================================

                if (Estado == "En proceso")
                {
                    garantia.FechaResolucion = null;
                }
                else
                {
                    garantia.FechaResolucion =
                        FechaResolucion ?? DateTime.Now;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La garantía fue actualizada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = IdGarantia });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GarantiumExists(IdGarantia))
                {
                    return NotFound();
                }

                TempData["Error"] =
                    "La garantía fue modificada por otro usuario.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = IdGarantia });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo actualizar la garantía: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    nameof(Edit),
                    new { id = IdGarantia });
            }
        }

        // =========================================================
        // DELETE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garantia = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)
                        .ThenInclude(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(g =>
                    g.IdGarantia == id);

            if (garantia == null)
            {
                return NotFound();
            }

            return View(garantia);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var garantia = await _context.Garantia
                    .FirstOrDefaultAsync(g =>
                        g.IdGarantia == id);

                if (garantia == null)
                {
                    TempData["Error"] =
                        "La garantía no existe.";

                    return RedirectToAction(nameof(Index));
                }

                _context.Garantia.Remove(garantia);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La garantía fue eliminada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo eliminar la garantía: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================
        // CARGAR DETALLES DE VENTA
        // =========================================================
        private async Task CargarDetallesVenta(
            int? idSeleccionado = null)
        {
            var detalles = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                    .ThenInclude(v => v.IdUsuarioNavigation)
                .OrderByDescending(d => d.IdDetalleVenta)
                .ToListAsync();

            ViewBag.DetallesVenta = detalles;

            ViewBag.DetalleSeleccionado =
                idSeleccionado;
        }

        // =========================================================
        // VERIFICAR EXISTENCIA
        // =========================================================
        private bool GarantiumExists(int id)
        {
            return _context.Garantia
                .Any(e => e.IdGarantia == id);
        }

        // =========================================================
        // OBTENER MENSAJE REAL DEL ERROR
        // =========================================================
        private string ObtenerMensajeError(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return ex.InnerException.Message;
            }

            return ex.Message;
        }
    }
}
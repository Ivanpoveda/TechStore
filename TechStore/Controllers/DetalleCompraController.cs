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

        // =========================================================
        // INDEX
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var detalles = await _context.DetalleCompras
                .Include(d => d.IdCompraNavigation)
                    .ThenInclude(c => c.IdProveedorNavigation)

                .Include(d => d.IdProductoNavigation)

                .OrderByDescending(d => d.IdDetalleCompra)
                .ToListAsync();

            return View(detalles);
        }


        // =========================================================
        // DETAILS
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var detalleCompra = await _context.DetalleCompras

                .Include(d => d.IdCompraNavigation)
                    .ThenInclude(c => c.IdProveedorNavigation)

                .Include(d => d.IdProductoNavigation)

                .FirstOrDefaultAsync(d =>
                    d.IdDetalleCompra == id);

            if (detalleCompra == null)
                return NotFound();

            return View(detalleCompra);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create(int? idCompra)
        {
            var productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewData["IdProducto"] = new SelectList(
                productos,
                "IdProducto",
                "Nombre"
            );

            var compras = await _context.CompraProveedors
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.Estado != "Recibida")
                .OrderByDescending(c => c.IdCompra)
                .ToListAsync();

            ViewData["IdCompra"] = new SelectList(
                compras,
                "IdCompra",
                "IdCompra"
            );

            var detalle = new DetalleCompra();

            if (idCompra.HasValue)
                detalle.IdCompra = idCompra.Value;

            return View(detalle);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int IdCompra,
            int IdProducto,
            int Cantidad,
            decimal PrecioCompra)
        {
            try
            {
                // -------------------------------------------------
                // VALIDAR COMPRA
                // -------------------------------------------------
                var compra = await _context.CompraProveedors
                    .FirstOrDefaultAsync(c =>
                        c.IdCompra == IdCompra);

                if (compra == null)
                {
                    TempData["Error"] =
                        "La compra seleccionada no existe.";

                    return RedirectToAction(
                        "Details",
                        "CompraProveedor",
                        new { id = IdCompra });
                }


                // -------------------------------------------------
                // VALIDAR PRODUCTO
                // -------------------------------------------------
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == IdProducto);

                if (producto == null)
                {
                    TempData["Error"] =
                        "El producto seleccionado no existe.";

                    return RedirectToAction(
                        "Details",
                        "CompraProveedor",
                        new { id = IdCompra });
                }


                // -------------------------------------------------
                // VALIDAR CANTIDAD
                // -------------------------------------------------
                if (Cantidad <= 0)
                {
                    TempData["Error"] =
                        "La cantidad debe ser mayor que cero.";

                    return RedirectToAction(
                        "Details",
                        "CompraProveedor",
                        new { id = IdCompra });
                }


                // -------------------------------------------------
                // VALIDAR PRECIO
                // -------------------------------------------------
                if (PrecioCompra <= 0)
                {
                    TempData["Error"] =
                        "El precio de compra debe ser mayor que cero.";

                    return RedirectToAction(
                        "Details",
                        "CompraProveedor",
                        new { id = IdCompra });
                }


                // -------------------------------------------------
                // CALCULAR SUBTOTAL
                // -------------------------------------------------
                decimal subtotal =
                    Cantidad * PrecioCompra;


                // -------------------------------------------------
                // CREAR DETALLE
                // -------------------------------------------------
                var detalle = new DetalleCompra
                {
                    Cantidad = Cantidad,
                    PrecioCompra = PrecioCompra,
                    Subtotal = subtotal,
                    IdCompra = IdCompra,
                    IdProducto = IdProducto
                };


                _context.DetalleCompras.Add(detalle);


                // -------------------------------------------------
                // ACTUALIZAR TOTAL DE LA COMPRA
                // -------------------------------------------------
                compra.Total =
                    (compra.Total ?? 0) + subtotal;


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    $"Producto '{producto.Nombre}' agregado correctamente a la compra.";


                return RedirectToAction(
                    "Details",
                    "CompraProveedor",
                    new { id = IdCompra });
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    "No se pudo registrar el detalle: " +
                    (ex.InnerException?.Message ?? ex.Message);

                return RedirectToAction(
                    "Details",
                    "CompraProveedor",
                    new { id = IdCompra });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Ocurrió un error: " + ex.Message;

                return RedirectToAction(
                    "Details",
                    "CompraProveedor",
                    new { id = IdCompra });
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var detalle = await _context.DetalleCompras
                .Include(d => d.IdCompraNavigation)
                    .ThenInclude(c => c.IdProveedorNavigation)

                .Include(d => d.IdProductoNavigation)

                .FirstOrDefaultAsync(d =>
                    d.IdDetalleCompra == id);

            if (detalle == null)
                return NotFound();


            // -----------------------------------------------------
            // PRODUCTOS
            // -----------------------------------------------------
            ViewData["IdProducto"] = new SelectList(
                await _context.Productos
                    .OrderBy(p => p.Nombre)
                    .ToListAsync(),
                "IdProducto",
                "Nombre",
                detalle.IdProducto
            );


            return View(detalle);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            int Cantidad,
            decimal PrecioCompra,
            int IdProducto)
        {
            if (id <= 0)
                return NotFound();


            var detalle = await _context.DetalleCompras
                .FirstOrDefaultAsync(d =>
                    d.IdDetalleCompra == id);

            if (detalle == null)
                return NotFound();


            try
            {
                // -------------------------------------------------
                // VALIDACIONES
                // -------------------------------------------------
                if (Cantidad <= 0)
                {
                    TempData["Error"] =
                        "La cantidad debe ser mayor que cero.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id });
                }


                if (PrecioCompra <= 0)
                {
                    TempData["Error"] =
                        "El precio debe ser mayor que cero.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id });
                }


                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == IdProducto);

                if (producto == null)
                {
                    TempData["Error"] =
                        "El producto seleccionado no existe.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id });
                }


                // -------------------------------------------------
                // OBTENER COMPRA
                // -------------------------------------------------
                var compra = await _context.CompraProveedors
                    .FirstOrDefaultAsync(c =>
                        c.IdCompra == detalle.IdCompra);

                if (compra == null)
                    return NotFound();


                // -------------------------------------------------
                // RESTAR SUBTOTAL ANTERIOR
                // -------------------------------------------------
                decimal subtotalAnterior =
                    detalle.Subtotal;

                compra.Total =
                    (compra.Total ?? 0) - subtotalAnterior;


                // -------------------------------------------------
                // NUEVOS VALORES
                // -------------------------------------------------
                decimal nuevoSubtotal =
                    Cantidad * PrecioCompra;


                detalle.Cantidad = Cantidad;
                detalle.PrecioCompra = PrecioCompra;
                detalle.Subtotal = nuevoSubtotal;
                detalle.IdProducto = IdProducto;


                // -------------------------------------------------
                // SUMAR NUEVO SUBTOTAL
                // -------------------------------------------------
                compra.Total =
                    (compra.Total ?? 0) + nuevoSubtotal;


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "El detalle de la compra fue actualizado correctamente.";


                return RedirectToAction(
                    "Details",
                    "CompraProveedor",
                    new { id = detalle.IdCompra });
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    "No se pudo actualizar el detalle: " +
                    (ex.InnerException?.Message ?? ex.Message);

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var detalle = await _context.DetalleCompras

                .Include(d => d.IdCompraNavigation)
                    .ThenInclude(c => c.IdProveedorNavigation)

                .Include(d => d.IdProductoNavigation)

                .FirstOrDefaultAsync(d =>
                    d.IdDetalleCompra == id);


            if (detalle == null)
                return NotFound();


            return View(detalle);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var detalle = await _context.DetalleCompras
                    .FirstOrDefaultAsync(d =>
                        d.IdDetalleCompra == id);

                if (detalle == null)
                    return NotFound();


                int idCompra =
                    detalle.IdCompra;


                // -------------------------------------------------
                // OBTENER COMPRA
                // -------------------------------------------------
                var compra = await _context.CompraProveedors
                    .FirstOrDefaultAsync(c =>
                        c.IdCompra == idCompra);


                if (compra != null)
                {
                    // ---------------------------------------------
                    // RESTAR EL SUBTOTAL
                    // ---------------------------------------------
                    compra.Total =
                        (compra.Total ?? 0) -
                        detalle.Subtotal;


                    if (compra.Total < 0)
                        compra.Total = 0;
                }


                _context.DetalleCompras.Remove(detalle);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "El producto fue eliminado de la compra.";


                return RedirectToAction(
                    "Details",
                    "CompraProveedor",
                    new { id = idCompra });
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    "No se pudo eliminar el detalle: " +
                    (ex.InnerException?.Message ?? ex.Message);

                return RedirectToAction(
                    "Index",
                    "CompraProveedor");
            }
        }


        // =========================================================
        // VER DETALLES DE UNA COMPRA
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> PorCompra(int id)
        {
            var compra = await _context.CompraProveedors

                .Include(c => c.IdProveedorNavigation)

                .Include(c => c.DetalleCompras)
                    .ThenInclude(d => d.IdProductoNavigation)

                .FirstOrDefaultAsync(c =>
                    c.IdCompra == id);


            if (compra == null)
                return NotFound();


            return View(compra);
        }
    }
}
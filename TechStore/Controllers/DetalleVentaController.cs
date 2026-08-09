using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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

        // =========================================================
        // INDEX
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var detalles = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                    .ThenInclude(v => v.IdUsuarioNavigation)
                .OrderByDescending(d => d.IdDetalleVenta)
                .ToListAsync();

            return View(detalles);
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

            var detalle = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                    .ThenInclude(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDetalleVenta == id);

            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create(int? idVenta)
        {
            // -----------------------------------------------------
            // Debe existir una venta
            // -----------------------------------------------------
            if (!idVenta.HasValue)
            {
                return BadRequest("Debe especificar una venta.");
            }

            // -----------------------------------------------------
            // Buscar venta
            // -----------------------------------------------------
            var venta = await _context.Venta
                .FirstOrDefaultAsync(v =>
                    v.IdVenta == idVenta.Value);

            if (venta == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Validar estado
            // -----------------------------------------------------
            if (venta.Estado == "Cancelada")
            {
                TempData["Error"] =
                    "No se pueden agregar productos a una venta cancelada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = idVenta.Value });
            }

            if (venta.Estado == "Completada")
            {
                TempData["Error"] =
                    "No se pueden agregar productos a una venta completada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = idVenta.Value });
            }

            // -----------------------------------------------------
            // Cargar productos
            // -----------------------------------------------------
            await CargarProductos();

            // -----------------------------------------------------
            // Guardar ID de venta para la vista
            // -----------------------------------------------------
            ViewData["IdVenta"] = idVenta.Value;

            // -----------------------------------------------------
            // Crear modelo
            // -----------------------------------------------------
            var modelo = new DetalleVentum
            {
                IdVenta = idVenta.Value,
                Cantidad = 1
            };

            return View(modelo);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int IdVenta,
            int IdProducto,
            int Cantidad)
        {
            Producto? producto = null;

            try
            {
                // =================================================
                // VALIDAR VENTA
                // =================================================
                var venta = await _context.Venta
                    .FirstOrDefaultAsync(v =>
                        v.IdVenta == IdVenta);

                if (venta == null)
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "La venta seleccionada no existe.");
                }
                else if (venta.Estado == "Cancelada")
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "No se pueden agregar productos a una venta cancelada.");
                }
                else if (venta.Estado == "Completada")
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "No se pueden agregar productos a una venta completada.");
                }

                // =================================================
                // VALIDAR PRODUCTO
                // =================================================
                producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == IdProducto);

                if (producto == null)
                {
                    ModelState.AddModelError(
                        "IdProducto",
                        "El producto seleccionado no existe.");
                }
                else if (producto.Estado != "Activo")
                {
                    ModelState.AddModelError(
                        "IdProducto",
                        "El producto seleccionado está inactivo.");
                }

                // =================================================
                // VALIDAR CANTIDAD
                // =================================================
                if (Cantidad <= 0)
                {
                    ModelState.AddModelError(
                        "Cantidad",
                        "La cantidad debe ser mayor que cero.");
                }

                // =================================================
                // VALIDAR STOCK
                // =================================================
                if (producto != null &&
                    Cantidad > producto.Stock)
                {
                    ModelState.AddModelError(
                        "Cantidad",
                        $"No hay suficiente stock. Stock disponible: {producto.Stock}.");
                }

                // =================================================
                // SI HAY ERRORES
                // =================================================
                if (!ModelState.IsValid)
                {
                    await CargarProductos(IdProducto);

                    ViewData["IdVenta"] = IdVenta;

                    var detalleError = new DetalleVentum
                    {
                        IdVenta = IdVenta,
                        IdProducto = IdProducto,
                        Cantidad = Cantidad,
                        PrecioUnitario =
                            producto?.Precio ?? 0,
                        Subtotal =
                            (producto?.Precio ?? 0) * Cantidad
                    };

                    return View(detalleError);
                }

                // =================================================
                // PRECIO DEL PRODUCTO
                // =================================================
                decimal precioUnitario = producto!.Precio;

                // =================================================
                // PARÁMETROS NEW_DETALLE_VENTA
                // =================================================
                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_ID_VENTA",
                        System.Data.SqlDbType.Int)
                    {
                        Value = IdVenta
                    },

                    new SqlParameter(
                        "@p_ID_PRODUCTO",
                        System.Data.SqlDbType.Int)
                    {
                        Value = IdProducto
                    },

                    new SqlParameter(
                        "@p_CANTIDAD",
                        System.Data.SqlDbType.Int)
                    {
                        Value = Cantidad
                    },

                    new SqlParameter(
                        "@p_PRECIO_UNITARIO",
                        System.Data.SqlDbType.Decimal)
                    {
                        Precision = 10,
                        Scale = 2,
                        Value = precioUnitario
                    }
                };

                // =================================================
                // EJECUTAR PROCEDIMIENTO
                // =================================================
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC NEW_DETALLE_VENTA " +
                    "@p_ID_VENTA, " +
                    "@p_ID_PRODUCTO, " +
                    "@p_CANTIDAD, " +
                    "@p_PRECIO_UNITARIO",
                    parameters
                );

                // =================================================
                // MENSAJE DE ÉXITO
                // =================================================
                TempData["Success"] =
                    $"Producto '{producto.Nombre}' agregado correctamente a la venta #{IdVenta}.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = IdVenta });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo agregar el producto: " +
                    ObtenerMensajeError(ex);

                await CargarProductos(IdProducto);

                ViewData["IdVenta"] = IdVenta;

                var detalleError = new DetalleVentum
                {
                    IdVenta = IdVenta,
                    IdProducto = IdProducto,
                    Cantidad = Cantidad,
                    PrecioUnitario =
                        producto?.Precio ?? 0,
                    Subtotal =
                        (producto?.Precio ?? 0) * Cantidad
                };

                return View(detalleError);
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

            var detalle = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDetalleVenta == id);

            if (detalle == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Validar estado de venta
            // -----------------------------------------------------
            if (detalle.IdVentaNavigation.Estado == "Cancelada")
            {
                TempData["Error"] =
                    "No se puede modificar un detalle de una venta cancelada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = detalle.IdVenta });
            }

            if (detalle.IdVentaNavigation.Estado == "Completada")
            {
                TempData["Error"] =
                    "No se puede modificar un detalle de una venta completada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = detalle.IdVenta });
            }

            // -----------------------------------------------------
            // Cargar productos
            // -----------------------------------------------------
            await CargarProductos(detalle.IdProducto);

            // -----------------------------------------------------
            // Guardar venta
            // -----------------------------------------------------
            ViewData["IdVenta"] = detalle.IdVenta;

            return View(detalle);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int IdDetalleVenta,
            int IdVenta,
            int IdProducto,
            int Cantidad)
        {
            Producto? producto = null;

            try
            {
                // =================================================
                // VALIDAR ID
                // =================================================
                if (IdDetalleVenta <= 0)
                {
                    return NotFound();
                }

                // =================================================
                // BUSCAR DETALLE ACTUAL
                // =================================================
                var detalleActual = await _context.DetalleVenta
                    .Include(d => d.IdVentaNavigation)
                    .FirstOrDefaultAsync(d =>
                        d.IdDetalleVenta == IdDetalleVenta);

                if (detalleActual == null)
                {
                    TempData["Error"] =
                        "El detalle de venta no existe.";

                    return RedirectToAction(
                        "Index",
                        "Venta");
                }

                // =================================================
                // VALIDAR QUE PERTENEZCA A LA VENTA
                // =================================================
                if (detalleActual.IdVenta != IdVenta)
                {
                    TempData["Error"] =
                        "El detalle no pertenece a la venta seleccionada.";

                    return RedirectToAction(
                        "Details",
                        "Venta",
                        new { id = detalleActual.IdVenta });
                }

                // =================================================
                // VALIDAR VENTA
                // =================================================
                var venta = await _context.Venta
                    .FirstOrDefaultAsync(v =>
                        v.IdVenta == IdVenta);

                if (venta == null)
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "La venta seleccionada no existe.");
                }
                else if (venta.Estado == "Cancelada")
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "No se puede modificar una venta cancelada.");
                }
                else if (venta.Estado == "Completada")
                {
                    ModelState.AddModelError(
                        "IdVenta",
                        "No se puede modificar una venta completada.");
                }

                // =================================================
                // VALIDAR PRODUCTO
                // =================================================
                producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == IdProducto);

                if (producto == null)
                {
                    ModelState.AddModelError(
                        "IdProducto",
                        "El producto seleccionado no existe.");
                }
                else if (producto.Estado != "Activo")
                {
                    ModelState.AddModelError(
                        "IdProducto",
                        "El producto seleccionado está inactivo.");
                }

                // =================================================
                // VALIDAR CANTIDAD
                // =================================================
                if (Cantidad <= 0)
                {
                    ModelState.AddModelError(
                        "Cantidad",
                        "La cantidad debe ser mayor que cero.");
                }

                // =================================================
                // SI HAY ERRORES
                // =================================================
                if (!ModelState.IsValid)
                {
                    await CargarProductos(IdProducto);

                    ViewData["IdVenta"] = IdVenta;

                    detalleActual.IdVenta = IdVenta;
                    detalleActual.IdProducto = IdProducto;
                    detalleActual.Cantidad = Cantidad;

                    if (producto != null)
                    {
                        detalleActual.PrecioUnitario =
                            producto.Precio;

                        detalleActual.Subtotal =
                            producto.Precio * Cantidad;
                    }

                    return View(detalleActual);
                }

                // =================================================
                // PARÁMETROS UPD_DETALLE_VENTA
                // =================================================
                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_ID_DETALLE_VENTA",
                        System.Data.SqlDbType.Int)
                    {
                        Value = IdDetalleVenta
                    },

                    new SqlParameter(
                        "@p_ID_VENTA",
                        System.Data.SqlDbType.Int)
                    {
                        Value = IdVenta
                    },

                    new SqlParameter(
                        "@p_ID_PRODUCTO",
                        System.Data.SqlDbType.Int)
                    {
                        Value = IdProducto
                    },

                    new SqlParameter(
                        "@p_CANTIDAD",
                        System.Data.SqlDbType.Int)
                    {
                        Value = Cantidad
                    }
                };

                // =================================================
                // EJECUTAR PROCEDIMIENTO
                // =================================================
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UPD_DETALLE_VENTA " +
                    "@p_ID_DETALLE_VENTA, " +
                    "@p_ID_VENTA, " +
                    "@p_ID_PRODUCTO, " +
                    "@p_CANTIDAD",
                    parameters
                );

                TempData["Success"] =
                    "El detalle de la venta fue actualizado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = IdVenta });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo modificar el detalle: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    "Edit",
                    new { id = IdDetalleVenta });
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

            var detalle = await _context.DetalleVenta
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdVentaNavigation)
                    .ThenInclude(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDetalleVenta == id);

            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
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
                // =================================================
                // BUSCAR DETALLE
                // =================================================
                var detalle = await _context.DetalleVenta
                    .Include(d => d.IdVentaNavigation)
                    .Include(d => d.IdProductoNavigation)
                    .FirstOrDefaultAsync(d =>
                        d.IdDetalleVenta == id);

                if (detalle == null)
                {
                    TempData["Error"] =
                        "El detalle de venta no existe.";

                    return RedirectToAction(
                        "Index",
                        "Venta");
                }

                int idVenta = detalle.IdVenta;

                // =================================================
                // VALIDAR ESTADO
                // =================================================
                if (detalle.IdVentaNavigation.Estado == "Cancelada")
                {
                    TempData["Error"] =
                        "No se puede eliminar un detalle de una venta cancelada.";

                    return RedirectToAction(
                        "Details",
                        "Venta",
                        new { id = idVenta });
                }

                if (detalle.IdVentaNavigation.Estado == "Completada")
                {
                    TempData["Error"] =
                        "No se puede eliminar un detalle de una venta completada.";

                    return RedirectToAction(
                        "Details",
                        "Venta",
                        new { id = idVenta });
                }

                // =================================================
                // PARÁMETRO
                // =================================================
                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_ID_DETALLE_VENTA",
                        System.Data.SqlDbType.Int)
                    {
                        Value = id
                    }
                };

                // =================================================
                // EJECUTAR DEL_DETALLE_VENTA
                // =================================================
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC DEL_DETALLE_VENTA " +
                    "@p_ID_DETALLE_VENTA",
                    parameters
                );

                TempData["Success"] =
                    "El producto fue eliminado de la venta y el stock fue actualizado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = idVenta });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo eliminar el detalle: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    "Index",
                    "Venta");
            }
        }

        // =========================================================
        // CARGAR PRODUCTOS
        // =========================================================
        private async Task CargarProductos(
            int? idProductoSeleccionado = null)
        {
            var productos = await _context.Productos
                .Where(p => p.Estado == "Activo")
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewData["IdProducto"] = new SelectList(
                productos,
                "IdProducto",
                "Nombre",
                idProductoSeleccionado
            );
        }

        // =========================================================
        // CARGAR VENTAS
        // =========================================================
        private async Task CargarVentas(
            int? idVentaSeleccionada = null)
        {
            var ventas = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .Where(v =>
                    v.Estado != "Cancelada" &&
                    v.Estado != "Completada")
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var listaVentas = ventas.Select(v => new
            {
                v.IdVenta,

                Descripcion =
                    $"Venta #{v.IdVenta} - " +
                    $"{v.IdUsuarioNavigation.Nombre} " +
                    $"{v.IdUsuarioNavigation.Apellidos}"
            });

            ViewData["Ventas"] = new SelectList(
                listaVentas,
                "IdVenta",
                "Descripcion",
                idVentaSeleccionada
            );
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

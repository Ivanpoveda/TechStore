using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TechStore.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TechStore.Controllers
{
    public class CarritoController : Controller
    {
        private readonly TechStoreContext _context;

        public CarritoController(TechStoreContext context)
        {
            _context = context;
        }


        // =====================================================
        // OBTENER ID DEL USUARIO LOGUEADO
        // =====================================================
        private int? ObtenerUsuarioId()
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }


        // =====================================================
        // MOSTRAR CARRITO
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                    .ThenInclude(dc => dc.IdProductoNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId.Value &&
                    c.Estado == "Activo");

            // Si no existe carrito activo, crearlo
            if (carrito == null)
            {
                carrito = new Carrito
                {
                    FechaCreacion = DateTime.Now,
                    Estado = "Activo",
                    IdUsuario = userId.Value
                };

                _context.Carritos.Add(carrito);

                await _context.SaveChangesAsync();
            }

            return View(carrito);
        }


        // =====================================================
        // AGREGAR PRODUCTO AL CARRITO
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarProducto(
            int productoId,
            int cantidad)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            if (cantidad <= 0)
            {
                TempData["Error"] =
                    "La cantidad debe ser mayor que cero.";

                return RedirectToAction("Catalogo", "Cliente");
            }

            // =================================================
            // BUSCAR PRODUCTO
            // =================================================

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.IdProducto == productoId &&
                    p.Estado == "Activo");

            if (producto == null)
            {
                TempData["Error"] =
                    "El producto no existe o no está disponible.";

                return RedirectToAction("Catalogo", "Cliente");
            }

            // =================================================
            // BUSCAR CARRITO ACTIVO
            // =================================================

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId.Value &&
                    c.Estado == "Activo");

            // =================================================
            // CREAR CARRITO SI NO EXISTE
            // =================================================

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    FechaCreacion = DateTime.Now,
                    Estado = "Activo",
                    IdUsuario = userId.Value
                };

                _context.Carritos.Add(carrito);

                await _context.SaveChangesAsync();
            }

            // =================================================
            // BUSCAR SI EL PRODUCTO YA ESTÁ EN EL CARRITO
            // =================================================

            var detalle = carrito.DetalleCarritos
                .FirstOrDefault(dc =>
                    dc.IdProducto == productoId);

            int cantidadFinal;

            if (detalle == null)
            {
                cantidadFinal = cantidad;
            }
            else
            {
                cantidadFinal =
                    detalle.Cantidad + cantidad;
            }

            // =================================================
            // VALIDAR STOCK
            // =================================================

            if (cantidadFinal > producto.Stock)
            {
                TempData["Error"] =
                    $"No hay suficiente stock. " +
                    $"Stock disponible: {producto.Stock}.";

                return RedirectToAction(
                    "DetalleProducto",
                    "Cliente",
                    new { id = productoId });
            }

            // =================================================
            // AGREGAR O ACTUALIZAR
            // =================================================

            if (detalle == null)
            {
                detalle = new DetalleCarrito
                {
                    IdCarrito = carrito.IdCarrito,
                    IdProducto = productoId,
                    Cantidad = cantidad
                };

                _context.DetalleCarritos.Add(detalle);
            }
            else
            {
                detalle.Cantidad = cantidadFinal;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Producto agregado al carrito.";

            return RedirectToAction("Index");
        }


        // =====================================================
        // QUITAR PRODUCTO
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarProducto(
            int productoId)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId.Value &&
                    c.Estado == "Activo");

            if (carrito == null)
            {
                return RedirectToAction("Index");
            }

            var detalle = carrito.DetalleCarritos
                .FirstOrDefault(dc =>
                    dc.IdProducto == productoId);

            if (detalle != null)
            {
                _context.DetalleCarritos.Remove(detalle);

                await _context.SaveChangesAsync();
            }

            TempData["Success"] =
                "Producto eliminado del carrito.";

            return RedirectToAction("Index");
        }


        // =====================================================
        // ACTUALIZAR CANTIDAD
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(
            int productoId,
            int cantidad)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            if (cantidad <= 0)
            {
                return await QuitarProducto(productoId);
            }

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId.Value &&
                    c.Estado == "Activo");

            if (carrito == null)
            {
                TempData["Error"] =
                    "No se encontró el carrito.";

                return RedirectToAction("Index");
            }

            var detalle = carrito.DetalleCarritos
                .FirstOrDefault(dc =>
                    dc.IdProducto == productoId);

            if (detalle == null)
            {
                TempData["Error"] =
                    "El producto no está en el carrito.";

                return RedirectToAction("Index");
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.IdProducto == productoId);

            if (producto == null)
            {
                TempData["Error"] =
                    "El producto no existe.";

                return RedirectToAction("Index");
            }

            if (cantidad > producto.Stock)
            {
                TempData["Error"] =
                    $"Stock disponible: {producto.Stock}.";

                return RedirectToAction("Index");
            }

            detalle.Cantidad = cantidad;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cantidad actualizada.";

            return RedirectToAction("Index");
        }


        // =====================================================
        // CONFIRMAR COMPRA
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCompra()
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =================================================
                // 1. BUSCAR CARRITO
                // =================================================

                var carrito = await _context.Carritos
                    .Include(c => c.DetalleCarritos)
                        .ThenInclude(dc => dc.IdProductoNavigation)
                    .FirstOrDefaultAsync(c =>
                        c.IdUsuario == userId.Value &&
                        c.Estado == "Activo");

                if (carrito == null)
                {
                    TempData["Error"] =
                        "PRUEBA 1: No se encontró el carrito.";

                    await transaction.RollbackAsync();

                    return RedirectToAction("Index");
                }

                if (!carrito.DetalleCarritos.Any())
                {
                    TempData["Error"] =
                        "PRUEBA 1: El carrito está vacío.";

                    await transaction.RollbackAsync();

                    return RedirectToAction("Index");
                }


                // =================================================
                // 2. VALIDAR STOCK
                // =================================================

                foreach (var item in carrito.DetalleCarritos)
                {
                    var producto =
                        item.IdProductoNavigation;

                    if (producto == null)
                    {
                        throw new Exception(
                            "PRUEBA 2: Producto no encontrado. ID: "
                            + item.IdProducto);
                    }

                    if (item.Cantidad <= 0)
                    {
                        throw new Exception(
                            "PRUEBA 2: Cantidad inválida.");
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        TempData["Error"] =
                            "PRUEBA 2: Stock insuficiente para "
                            + producto.Nombre;

                        await transaction.RollbackAsync();

                        return RedirectToAction("Index");
                    }
                }


                // =================================================
                // 3. CALCULAR TOTAL
                // =================================================

                decimal total =
                    carrito.DetalleCarritos.Sum(item =>
                        item.Cantidad *
                        item.IdProductoNavigation.Precio);


                // =================================================
                // 4. CREAR VENTA
                // =================================================

                var venta = new Ventum
                {
                    IdUsuario = userId.Value,
                    Fecha = DateTime.Now,
                    Descuento = 0,
                    Impuesto = 0,
                    Total = total,
                    Estado = "Pendiente"
                };

                _context.Venta.Add(venta);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new Exception(
                        "PRUEBA 4: ERROR AL GUARDAR LA VENTA. "
                        + ex.Message);
                }


                // =================================================
                // 5. CREAR DETALLES DE VENTA
                // =================================================

                foreach (var item in carrito.DetalleCarritos)
                {
                    var producto =
                        item.IdProductoNavigation;

                    var detalleVenta =
                        new DetalleVentum
                        {
                            IdVenta = venta.IdVenta,
                            IdProducto = item.IdProducto,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = producto.Precio,
                            Subtotal =
                                item.Cantidad *
                                producto.Precio
                        };

                    _context.DetalleVenta.Add(detalleVenta);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new Exception(
                        "PRUEBA 5: ERROR AL GUARDAR DETALLE_VENTA. "
                        + ex.Message);
                }


                // =================================================
                // 6. EL STOCK SE ACTUALIZA EN EL TRIGGER
                // =================================================
                //
                // TRIG_STOCK_VENTA:
                // - valida stock
                // - descuenta stock
                // - registra historial
                //
                // NO SE HACE UPDATE DE PRODUCTO AQUÍ.


                // =================================================
                // 7. FACTURAR CARRITO
                // =================================================

                var filasCarrito =
                    await _context.Database
                        .ExecuteSqlInterpolatedAsync($@"
                            UPDATE CARRITO
                            SET ESTADO = {"Facturado"}
                            WHERE ID_CARRITO = {carrito.IdCarrito}
                            AND ID_USUARIO = {userId.Value}
                            AND ESTADO = {"Activo"}
                        ");

                if (filasCarrito == 0)
                {
                    throw new Exception(
                        "PRUEBA 7: No se pudo cambiar el carrito a Facturado.");
                }


                // =================================================
                // 8. CONFIRMAR TRANSACCIÓN
                // =================================================

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Compra realizada correctamente.";

                return RedirectToAction(
                    "MisCompras",
                    "Cliente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "ERROR REAL: " + ex.Message;

                return RedirectToAction("Index");
            }
        }
    }
}
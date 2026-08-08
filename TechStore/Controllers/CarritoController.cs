using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class CarritoController : Controller
    {
        private readonly TechStoreContext _context;

        public CarritoController(TechStoreContext context)
        {
            _context = context;
        }

        // Mostrar carrito activo del usuario logueado
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .ThenInclude(dc => dc.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdUsuario == userId && c.Estado == "Activo");

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    FechaCreacion = DateTime.Now,
                    Estado = "Activo",
                    IdUsuario = userId
                };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return View(carrito);
        }

        // Agregar producto al carrito
        [HttpPost]
        public async Task<IActionResult> AgregarProducto(int productoId, int cantidad)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .FirstOrDefaultAsync(c => c.IdUsuario == userId && c.Estado == "Activo");

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null || producto.Stock < cantidad)
            {
                TempData["Error"] = "Stock insuficiente";
                return RedirectToAction("Index");
            }

            var detalle = carrito.DetalleCarritos.FirstOrDefault(dc => dc.IdProducto == productoId);
            if (detalle == null)
            {
                detalle = new DetalleCarrito
                {
                    IdCarrito = carrito.IdCarrito,
                    IdProducto = productoId,
                    Cantidad = cantidad
                };
                carrito.DetalleCarritos.Add(detalle);
            }
            else
            {
                detalle.Cantidad += cantidad;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Quitar producto del carrito
        [HttpPost]
        public async Task<IActionResult> QuitarProducto(int productoId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .FirstOrDefaultAsync(c => c.IdUsuario == userId && c.Estado == "Activo");

            var detalle = carrito?.DetalleCarritos.FirstOrDefault(dc => dc.IdProducto == productoId);
            if (detalle != null)
            {
                _context.DetalleCarritos.Remove(detalle);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Confirmar compra
        [HttpPost]
        public async Task<IActionResult> ConfirmarCompra()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarritos)
                .ThenInclude(dc => dc.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdUsuario == userId && c.Estado == "Activo");

            if (carrito == null || !carrito.DetalleCarritos.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("Index");
            }

            // Crear venta
            var venta = new Ventum
            {
                IdUsuario = userId,
                Fecha = DateTime.Now, // Usa la propiedad real de tu modelo Ventum
                Total = carrito.DetalleCarritos.Sum(dc => dc.Cantidad * dc.IdProductoNavigation.Precio)
            };
            _context.Venta.Add(venta);
            await _context.SaveChangesAsync();

            // Crear detalle de venta y actualizar stock
            foreach (var item in carrito.DetalleCarritos)
            {
                var detalleVenta = new DetalleVentum
                {
                    IdVenta = venta.IdVenta,
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.IdProductoNavigation.Precio
                };
                _context.DetalleVenta.Add(detalleVenta);

                item.IdProductoNavigation.Stock -= item.Cantidad;
            }

            carrito.Estado = "Finalizado";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Compra realizada con éxito";
            return RedirectToAction("Index", "Venta");
        }
    }
}



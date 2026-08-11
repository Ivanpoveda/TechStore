using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechStore.Models;

namespace TechStore.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly TechStoreContext _context;

        public ClienteController(TechStoreContext context)
        {
            _context = context;
        }

        // =====================================================
        // CATÁLOGO DEL CLIENTE
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Catalogo()
        {
            var productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .Where(p => p.Estado == "Activo")
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View("Catalogo", productos);
        }


        // =====================================================
        // DETALLE DEL PRODUCTO
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> DetalleProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .FirstOrDefaultAsync(p =>
                    p.IdProducto == id &&
                    p.Estado == "Activo");

            if (producto == null)
            {
                return NotFound();
            }

            return View("DetalleProducto", producto);
        }


        // =====================================================
        // MIS COMPRAS
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> MisCompras()
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var compras = await _context.Venta
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Where(v => v.IdUsuario == userId)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View("MisCompras", compras);
        }
    }
}
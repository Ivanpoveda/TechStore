using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechStore.Models;

namespace TechStore.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class GarantiaClienteController : Controller
    {
        private readonly TechStoreContext _context;

        public GarantiaClienteController(TechStoreContext context)
        {
            _context = context;
        }

        // =========================================================
        // OBTENER ID DEL CLIENTE LOGUEADO
        // =========================================================
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


        // =========================================================
        // INDEX - MIS GARANTÍAS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            var garantias = await _context.Garantia
                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)

                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)

                .Where(g =>
                    g.IdDetalleVentaNavigation
                        .IdVentaNavigation
                        .IdUsuario == userId.Value)

                .OrderByDescending(g => g.FechaSolicitud)
                .ToListAsync();

            return View(garantias);
        }


        // =========================================================
        // MIS PRODUCTOS COMPRADOS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MisProductos()
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            var detalles = await _context.DetalleVenta

                .Include(d => d.IdProductoNavigation)

                .Include(d => d.IdVentaNavigation)

                .Where(d =>
                    d.IdVentaNavigation.IdUsuario == userId.Value
                    &&
                    !_context.Garantia.Any(g =>
                        g.IdDetalleVenta == d.IdDetalleVenta)
                )

                .OrderByDescending(d =>
                    d.IdVentaNavigation.Fecha)

                .ToListAsync();

            return View(detalles);
        }


        // =========================================================
        // SOLICITAR GARANTÍA - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Solicitar(int id)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();

            var detalle = await _context.DetalleVenta

                .Include(d => d.IdProductoNavigation)

                .Include(d => d.IdVentaNavigation)

                .FirstOrDefaultAsync(d =>
                    d.IdDetalleVenta == id
                    &&
                    d.IdVentaNavigation.IdUsuario == userId.Value
                );

            if (detalle == null)
            {
                TempData["Error"] =
                    "El producto no pertenece a una de tus compras.";

                return RedirectToAction(nameof(MisProductos));
            }


            // -----------------------------------------------------
            // VERIFICAR SI YA EXISTE GARANTÍA
            // -----------------------------------------------------
            var garantiaExistente =
                await _context.Garantia
                    .FirstOrDefaultAsync(g =>
                        g.IdDetalleVenta == id);

            if (garantiaExistente != null)
            {
                TempData["Error"] =
                    "Este producto ya tiene una solicitud de garantía.";

                return RedirectToAction(nameof(Index));
            }


            return View(detalle);
        }


        // =========================================================
        // SOLICITAR GARANTÍA - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(
            int IdDetalleVenta,
            string Motivo,
            string Descripcion)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();


            // -----------------------------------------------------
            // VALIDAR ID DEL DETALLE
            // -----------------------------------------------------
            if (IdDetalleVenta <= 0)
            {
                TempData["Error"] =
                    "El producto seleccionado no es válido.";

                return RedirectToAction(nameof(MisProductos));
            }


            // -----------------------------------------------------
            // VALIDAR MOTIVO
            // -----------------------------------------------------
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                TempData["Error"] =
                    "Debe indicar el motivo de la garantía.";

                return RedirectToAction(
                    nameof(Solicitar),
                    new { id = IdDetalleVenta });
            }


            // -----------------------------------------------------
            // VALIDAR DESCRIPCIÓN
            // -----------------------------------------------------
            if (string.IsNullOrWhiteSpace(Descripcion))
            {
                TempData["Error"] =
                    "Debe proporcionar una descripción del problema.";

                return RedirectToAction(
                    nameof(Solicitar),
                    new { id = IdDetalleVenta });
            }


            // -----------------------------------------------------
            // BUSCAR DETALLE Y VERIFICAR PROPIETARIO
            // -----------------------------------------------------
            var detalle = await _context.DetalleVenta

                .Include(d => d.IdVentaNavigation)

                .FirstOrDefaultAsync(d =>
                    d.IdDetalleVenta == IdDetalleVenta
                    &&
                    d.IdVentaNavigation.IdUsuario == userId.Value
                );

            if (detalle == null)
            {
                TempData["Error"] =
                    "No tienes permiso para solicitar garantía para este producto.";

                return RedirectToAction(nameof(MisProductos));
            }


            // -----------------------------------------------------
            // VERIFICAR GARANTÍA EXISTENTE
            // -----------------------------------------------------
            var garantiaExistente =
                await _context.Garantia
                    .FirstOrDefaultAsync(g =>
                        g.IdDetalleVenta == IdDetalleVenta);

            if (garantiaExistente != null)
            {
                TempData["Error"] =
                    "Este producto ya tiene una solicitud de garantía.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // CREAR GARANTÍA
            // -----------------------------------------------------
            var garantia = new Garantium
            {
                FechaSolicitud = DateTime.Now,

                Motivo = Motivo.Trim(),

                Descripcion = Descripcion.Trim(),

                Estado = "En proceso",

                FechaResolucion = null,

                IdDetalleVenta = IdDetalleVenta
            };


            _context.Garantia.Add(garantia);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    "No se pudo registrar la garantía: " +
                    (ex.InnerException?.Message ?? ex.Message);

                return RedirectToAction(
                    nameof(Solicitar),
                    new { id = IdDetalleVenta });
            }


            // -----------------------------------------------------
            // ÉXITO
            // -----------------------------------------------------
            TempData["Success"] =
                "La solicitud de garantía fue registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETAILS - DETALLE DE GARANTÍA
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = ObtenerUsuarioId();

            if (!userId.HasValue)
                return Unauthorized();


            var garantia = await _context.Garantia

                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdProductoNavigation)

                .Include(g => g.IdDetalleVentaNavigation)
                    .ThenInclude(d => d.IdVentaNavigation)

                .FirstOrDefaultAsync(g =>
                    g.IdGarantia == id
                    &&
                    g.IdDetalleVentaNavigation
                        .IdVentaNavigation
                        .IdUsuario == userId.Value
                );


            if (garantia == null)
            {
                TempData["Error"] =
                    "La garantía no existe o no tienes permiso para verla.";

                return RedirectToAction(nameof(Index));
            }


            return View(garantia);
        }
    }
}
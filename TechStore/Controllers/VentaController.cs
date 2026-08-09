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
    public class VentaController : Controller
    {
        private readonly TechStoreContext _context;

        public VentaController(TechStoreContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: Venta
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        // =========================================================
        // GET: Venta/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // =========================================================
        // GET: Venta/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            await CargarUsuarios();

            return View();
        }

        // =========================================================
        // POST: Venta/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int IdUsuario,
            decimal? Impuesto,
            decimal? Descuento)
        {
            if (IdUsuario <= 0)
            {
                ModelState.AddModelError("IdUsuario", "Debe seleccionar un usuario.");
            }

            if (!ModelState.IsValid)
            {
                await CargarUsuarios(IdUsuario);
                return View();
            }

            try
            {
                // Valores por defecto
                decimal impuesto = Impuesto ?? 0;
                decimal descuento = Descuento ?? 0;

                // Parámetro OUTPUT para recibir el ID de la venta
                var idVentaParameter = new SqlParameter
                {
                    ParameterName = "@p_ID_VENTA",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                var parameters = new[]
                {
                    new SqlParameter("@p_ID_USUARIO", IdUsuario),

                    new SqlParameter("@p_IMPUESTO", impuesto),

                    new SqlParameter("@p_DESCUENTO", descuento),

                    idVentaParameter
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC NEW_VENTA @p_ID_USUARIO, @p_IMPUESTO, @p_DESCUENTO, @p_ID_VENTA OUTPUT",
                    parameters
                );

                int idVenta = Convert.ToInt32(idVentaParameter.Value);

                TempData["Success"] =
                    $"Venta #{idVenta} creada correctamente. Ahora puedes agregar los productos.";

                return RedirectToAction(nameof(Details), new { id = idVenta });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo crear la venta: " + ex.Message;

                await CargarUsuarios(IdUsuario);

                return View();
            }
        }

        // =========================================================
        // GET: Venta/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // =========================================================
        // POST: Venta/Edit
        // Solo permite cambiar el estado
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int IdVenta,
            string Estado)
        {
            if (IdVenta <= 0)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(Estado))
            {
                ModelState.AddModelError(
                    "Estado",
                    "Debe seleccionar un estado."
                );
            }

            if (!ModelState.IsValid)
            {
                var ventaError = await _context.Venta
                    .Include(v => v.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(v => v.IdVenta == IdVenta);

                if (ventaError == null)
                {
                    return NotFound();
                }

                return View(ventaError);
            }

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@p_ID_VENTA", IdVenta),

                    new SqlParameter("@p_ESTADO", Estado)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UPD_ESTADO_VENTA @p_ID_VENTA, @p_ESTADO",
                    parameters
                );

                TempData["Success"] =
                    $"El estado de la venta #{IdVenta} fue actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo actualizar el estado: " + ex.Message;

                var venta = await _context.Venta
                    .Include(v => v.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(v => v.IdVenta == IdVenta);

                if (venta == null)
                {
                    return NotFound();
                }

                return View(venta);
            }
        }

        // =========================================================
        // GET: Venta/Delete/5
        // =========================================================
        // Por ahora mostramos una pantalla informativa.
        // NO hacemos DELETE físico porque no existe DEL_VENTA.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // =========================================================
        // MÉTODO AUXILIAR
        // =========================================================
        private async Task CargarUsuarios(int? idUsuarioSeleccionado = null)
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();

            ViewData["IdUsuario"] = new SelectList(
                usuarios,
                "IdUsuario",
                "Nombre",
                idUsuarioSeleccionado
            );
        }
    }
}
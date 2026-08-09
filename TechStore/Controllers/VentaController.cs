using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        // INDEX
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
        // DETAILS
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
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarUsuarios();

            return View();
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int IdUsuario,
            decimal? Impuesto,
            decimal? Descuento)
        {
            try
            {
                // -------------------------------------------------
                // VALIDAR USUARIO
                // -------------------------------------------------
                if (IdUsuario <= 0)
                {
                    TempData["Error"] =
                        "Debe seleccionar un cliente.";

                    await CargarUsuarios(IdUsuario);

                    return View();
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == IdUsuario &&
                        u.Estado == "Activo");

                if (usuario == null)
                {
                    TempData["Error"] =
                        "El cliente seleccionado no existe o está inactivo.";

                    await CargarUsuarios(IdUsuario);

                    return View();
                }

                // -------------------------------------------------
                // VALORES POR DEFECTO
                // -------------------------------------------------
                decimal impuesto = Impuesto ?? 0;
                decimal descuento = Descuento ?? 0;

                // -------------------------------------------------
                // VALIDAR IMPUESTO
                // -------------------------------------------------
                if (impuesto < 0)
                {
                    TempData["Error"] =
                        "El impuesto no puede ser negativo.";

                    await CargarUsuarios(IdUsuario);

                    return View();
                }

                // -------------------------------------------------
                // VALIDAR DESCUENTO
                // -------------------------------------------------
                if (descuento < 0)
                {
                    TempData["Error"] =
                        "El descuento no puede ser negativo.";

                    await CargarUsuarios(IdUsuario);

                    return View();
                }

                // -------------------------------------------------
                // PARÁMETRO OUTPUT
                // -------------------------------------------------
                var idVentaParameter = new SqlParameter
                {
                    ParameterName = "@p_ID_VENTA",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                // -------------------------------------------------
                // PARÁMETROS NEW_VENTA
                // -------------------------------------------------
                var parameters = new[]
                {
                new SqlParameter(
                    "@p_ID_USUARIO",
                    System.Data.SqlDbType.Int)
                {
                    Value = IdUsuario
                },

                new SqlParameter(
                    "@p_IMPUESTO",
                    System.Data.SqlDbType.Decimal)
                {
                    Precision = 10,
                    Scale = 2,
                    Value = impuesto
                },

                new SqlParameter(
                    "@p_DESCUENTO",
                    System.Data.SqlDbType.Decimal)
                {
                    Precision = 10,
                    Scale = 2,
                    Value = descuento
                },

                idVentaParameter
            };

                // -------------------------------------------------
                // EJECUTAR NEW_VENTA
                // -------------------------------------------------
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC NEW_VENTA " +
                    "@p_ID_USUARIO, " +
                    "@p_IMPUESTO, " +
                    "@p_DESCUENTO, " +
                    "@p_ID_VENTA OUTPUT",
                    parameters
                );

                // -------------------------------------------------
                // OBTENER ID GENERADO
                // -------------------------------------------------
                int idVenta =
                    Convert.ToInt32(idVentaParameter.Value);

                TempData["Success"] =
                    $"La venta #{idVenta} fue creada correctamente.";

                // -------------------------------------------------
                // IR A DETAILS PARA AGREGAR PRODUCTOS
                // -------------------------------------------------
                return RedirectToAction(
                    nameof(Details),
                    new { id = idVenta }
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo crear la venta: " +
                    ObtenerMensajeError(ex);

                await CargarUsuarios(IdUsuario);

                return View();
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

            var venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(v =>
                    v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int IdVenta,
            string Estado)
        {
            try
            {
                if (IdVenta <= 0)
                {
                    return NotFound();
                }

                // -------------------------------------------------
                // VALIDAR ESTADO
                // -------------------------------------------------
                if (string.IsNullOrWhiteSpace(Estado))
                {
                    TempData["Error"] =
                        "Debe seleccionar un estado.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = IdVenta });
                }

                string[] estadosPermitidos =
                {
                "Pendiente",
                "Completada",
                "Cancelada"
            };

                if (!estadosPermitidos.Contains(Estado))
                {
                    TempData["Error"] =
                        "El estado seleccionado no es válido.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = IdVenta });
                }

                // -------------------------------------------------
                // VERIFICAR QUE EXISTA
                // -------------------------------------------------
                var venta = await _context.Venta
                    .FirstOrDefaultAsync(v =>
                        v.IdVenta == IdVenta);

                if (venta == null)
                {
                    TempData["Error"] =
                        "La venta no existe.";

                    return RedirectToAction(nameof(Index));
                }

                // -------------------------------------------------
                // EJECUTAR UPD_ESTADO_VENTA
                // -------------------------------------------------
                var parameters = new[]
                {
                new SqlParameter(
                    "@p_ID_VENTA",
                    System.Data.SqlDbType.Int)
                {
                    Value = IdVenta
                },

                new SqlParameter(
                    "@p_ESTADO",
                    System.Data.SqlDbType.VarChar,
                    15)
                {
                    Value = Estado
                }
            };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UPD_ESTADO_VENTA " +
                    "@p_ID_VENTA, " +
                    "@p_ESTADO",
                    parameters
                );

                TempData["Success"] =
                    $"El estado de la venta #{IdVenta} fue actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo actualizar la venta: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    nameof(Edit),
                    new { id = IdVenta });
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

            var venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(v =>
                    v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int IdVenta)
        {
            try
            {
                // -------------------------------------------------
                // BUSCAR VENTA
                // -------------------------------------------------
                var venta = await _context.Venta
                    .FirstOrDefaultAsync(v =>
                        v.IdVenta == IdVenta);

                if (venta == null)
                {
                    TempData["Error"] =
                        "La venta no existe.";

                    return RedirectToAction(nameof(Index));
                }

                // -------------------------------------------------
                // NO CANCELAR DOS VECES
                // -------------------------------------------------
                if (venta.Estado == "Cancelada")
                {
                    TempData["Error"] =
                        "La venta ya se encuentra cancelada.";

                    return RedirectToAction(nameof(Index));
                }

                // -------------------------------------------------
                // CANCELAR MEDIANTE PROCEDIMIENTO
                // -------------------------------------------------
                //
                // UPD_ESTADO_VENTA
                //        ↓
                // ESTADO = Cancelada
                //        ↓
                // TRIG_REPOSICION_VENTA_CANCELADA
                //        ↓
                // STOCK +
                //        ↓
                // HISTORIAL_INVENTARIO
                //
                // -------------------------------------------------

                var parameters = new[]
                {
                new SqlParameter(
                    "@p_ID_VENTA",
                    System.Data.SqlDbType.Int)
                {
                    Value = IdVenta
                },

                new SqlParameter(
                    "@p_ESTADO",
                    System.Data.SqlDbType.VarChar,
                    15)
                {
                    Value = "Cancelada"
                }
            };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UPD_ESTADO_VENTA " +
                    "@p_ID_VENTA, " +
                    "@p_ESTADO",
                    parameters
                );

                TempData["Success"] =
                    $"La venta #{IdVenta} fue cancelada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo cancelar la venta: " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================
        // CARGAR USUARIOS
        // =========================================================
        private async Task CargarUsuarios(
            int? usuarioSeleccionado = null)
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Estado == "Activo")
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();

            ViewBag.Usuarios = usuarios;

            ViewBag.UsuarioSeleccionado =
                usuarioSeleccionado;
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

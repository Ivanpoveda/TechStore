using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class CompraProveedorController : Controller
    {
        private readonly TechStoreContext _context;

        public CompraProveedorController(TechStoreContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var compras = await _context.CompraProveedors
                .Include(c => c.IdProveedorNavigation)
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();

            return View(compras);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.CompraProveedors
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCompra == id);

            if (compra == null)
                return NotFound();

            var detalles = await _context.DetalleCompras
                .Include(d => d.IdProductoNavigation)
                .Where(d => d.IdCompra == id)
                .ToListAsync();

            ViewBag.Detalles = detalles;

            return View(compra);
        }


        // =========================================================
        // CREATE - GET
        // CREA UNA COMPRA CON SU PRIMER PRODUCTO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View(new CompraProveedor
            {
                FechaCompra = DateTime.Now,
                Estado = "Pendiente",
                Total = 0
            });
        }


        // =========================================================
        // CREATE - POST
        // CREA COMPRA + PRIMER DETALLE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int IdProveedor,
            int IdProducto,
            int Cantidad,
            decimal PrecioCompra)
        {
            try
            {
                // =================================================
                // VALIDAR PROVEEDOR
                // =================================================

                if (IdProveedor <= 0)
                {
                    TempData["Error"] =
                        "Debe seleccionar un proveedor.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // VALIDAR PRODUCTO
                // =================================================

                if (IdProducto <= 0)
                {
                    TempData["Error"] =
                        "Debe seleccionar un producto.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // VALIDAR CANTIDAD
                // =================================================

                if (Cantidad <= 0)
                {
                    TempData["Error"] =
                        "La cantidad debe ser mayor que cero.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // VALIDAR PRECIO
                // =================================================

                if (PrecioCompra <= 0)
                {
                    TempData["Error"] =
                        "El precio de compra debe ser mayor que cero.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // VERIFICAR PROVEEDOR
                // =================================================

                var proveedor = await _context.Proveedors
                    .FirstOrDefaultAsync(p =>
                        p.IdProveedor == IdProveedor);

                if (proveedor == null)
                {
                    TempData["Error"] =
                        "El proveedor seleccionado no existe.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // VERIFICAR PRODUCTO
                // =================================================

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == IdProducto);

                if (producto == null)
                {
                    TempData["Error"] =
                        "El producto seleccionado no existe.";

                    await CargarListas(
                        IdProveedor,
                        IdProducto);

                    return View();
                }


                // =================================================
                // CREAR COMPRA DEL PROVEEDOR
                // =================================================

                var parametroProveedor =
                    new SqlParameter(
                        "@p_ID_PROVEEDOR",
                        SqlDbType.Int)
                    {
                        Value = IdProveedor
                    };


                var parametroCompra =
                    new SqlParameter(
                        "@p_ID_COMPRA",
                        SqlDbType.Int)
                    {
                        Direction =
                            ParameterDirection.Output
                    };


                await _context.Database
                    .ExecuteSqlRawAsync(
                        "EXEC NEW_COMPRA_PROVEEDOR " +
                        "@p_ID_PROVEEDOR, " +
                        "@p_ID_COMPRA OUTPUT",
                        parametroProveedor,
                        parametroCompra
                    );


                // =================================================
                // OBTENER ID DE LA COMPRA
                // =================================================

                int idCompra =
                    Convert.ToInt32(
                        parametroCompra.Value);


                // =================================================
                // CREAR PRIMER DETALLE
                // =================================================

                var parametroIdCompra =
                    new SqlParameter(
                        "@p_ID_COMPRA",
                        SqlDbType.Int)
                    {
                        Value = idCompra
                    };


                var parametroIdProducto =
                    new SqlParameter(
                        "@p_ID_PRODUCTO",
                        SqlDbType.Int)
                    {
                        Value = IdProducto
                    };


                var parametroCantidad =
                    new SqlParameter(
                        "@p_CANTIDAD",
                        SqlDbType.Int)
                    {
                        Value = Cantidad
                    };


                var parametroPrecio =
                    new SqlParameter(
                        "@p_PRECIO_COMPRA",
                        SqlDbType.Decimal)
                    {
                        Precision = 10,
                        Scale = 2,
                        Value = PrecioCompra
                    };


                await _context.Database
                    .ExecuteSqlRawAsync(
                        "EXEC NEW_DETALLE_COMPRA " +
                        "@p_ID_COMPRA, " +
                        "@p_ID_PRODUCTO, " +
                        "@p_CANTIDAD, " +
                        "@p_PRECIO_COMPRA",
                        parametroIdCompra,
                        parametroIdProducto,
                        parametroCantidad,
                        parametroPrecio
                    );


                // =================================================
                // ÉXITO
                // =================================================

                TempData["Success"] =
                    "La compra fue creada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = idCompra });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo crear la compra: " +
                    (ex.InnerException?.Message ??
                     ex.Message);

                await CargarListas(
                    IdProveedor,
                    IdProducto);

                return View();
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var compra = _context.CompraProveedors
                .FirstOrDefault(c => c.IdCompra == id);

            if (compra == null)
            {
                return NotFound();
            }

            if (compra.Estado == "Cancelada")
            {
                TempData["Error"] =
                    "No se puede editar una compra que está cancelada.";

                return RedirectToAction("Index");
            }

            if (compra.Estado == "Recibida")
            {
                TempData["Error"] =
                    "No se puede editar una compra que ya fue recibida.";

                return RedirectToAction("Index");
            }

            return View(compra);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CompraProveedor compra)
        {
            var compraExistente = _context.CompraProveedors
                .FirstOrDefault(c => c.IdCompra == id);

            if (compraExistente == null)
            {
                return NotFound();
            }

            if (compraExistente.Estado == "Cancelada")
            {
                TempData["Error"] =
                    "No se puede editar una compra que está cancelada.";

                return RedirectToAction("Index");
            }

            if (compraExistente.Estado == "Recibida")
            {
                TempData["Error"] =
                    "No se puede editar una compra que ya fue recibida.";

                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(compra);
            }

            // Actualiza únicamente los campos que permites modificar
            compraExistente.IdProveedor = compra.IdProveedor;
            compraExistente.FechaCompra = compra.FechaCompra;
            compraExistente.Total = compra.Total;
            compraExistente.Estado = compra.Estado;

            _context.SaveChanges();

            TempData["Success"] =
                "Compra actualizada correctamente.";

            return RedirectToAction("Index");
        }


        // =========================================================
        // AGREGAR DETALLE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AgregarDetalle(
            int id)
        {
            var compra =
                await _context.CompraProveedors
                    .FirstOrDefaultAsync(c =>
                        c.IdCompra == id);

            if (compra == null)
                return NotFound();


            // =================================================
            // NO AGREGAR A RECIBIDA
            // =================================================

            if (compra.Estado == "Recibida")
            {
                TempData["Error"] =
                    "No puedes agregar productos a una compra recibida.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // NO AGREGAR A CANCELADA
            // =================================================

            if (compra.Estado == "Cancelada")
            {
                TempData["Error"] =
                    "No puedes agregar productos a una compra cancelada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // CARGAR PRODUCTOS
            // =================================================

            ViewBag.IdCompra = id;


            ViewBag.IdProducto =
                new SelectList(
                    await _context.Productos
                        .OrderBy(p => p.Nombre)
                        .ToListAsync(),
                    "IdProducto",
                    "Nombre"
                );


            return View();
        }


        // =========================================================
        // AGREGAR DETALLE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDetalle(
            int IdCompra,
            int IdProducto,
            int Cantidad,
            decimal PrecioCompra)
        {
            try
            {
                // =================================================
                // VERIFICAR COMPRA
                // =================================================

                var compra =
                    await _context.CompraProveedors
                        .FirstOrDefaultAsync(c =>
                            c.IdCompra == IdCompra);

                if (compra == null)
                    return NotFound();


                // =================================================
                // VERIFICAR ESTADO
                // =================================================

                if (compra.Estado == "Recibida")
                {
                    TempData["Error"] =
                        "No puedes modificar una compra recibida.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = IdCompra });
                }


                if (compra.Estado == "Cancelada")
                {
                    TempData["Error"] =
                        "No puedes modificar una compra cancelada.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = IdCompra });
                }


                // =================================================
                // VALIDAR CANTIDAD
                // =================================================

                if (Cantidad <= 0)
                {
                    TempData["Error"] =
                        "La cantidad debe ser mayor que cero.";

                    return RedirectToAction(
                        nameof(AgregarDetalle),
                        new { id = IdCompra });
                }


                // =================================================
                // VALIDAR PRECIO
                // =================================================

                if (PrecioCompra <= 0)
                {
                    TempData["Error"] =
                        "El precio de compra debe ser mayor que cero.";

                    return RedirectToAction(
                        nameof(AgregarDetalle),
                        new { id = IdCompra });
                }


                // =================================================
                // VERIFICAR PRODUCTO
                // =================================================

                var productoExiste =
                    await _context.Productos
                        .AnyAsync(p =>
                            p.IdProducto == IdProducto);

                if (!productoExiste)
                {
                    TempData["Error"] =
                        "El producto seleccionado no existe.";

                    return RedirectToAction(
                        nameof(AgregarDetalle),
                        new { id = IdCompra });
                }


                // =================================================
                // INSERTAR DETALLE
                // =================================================

                await _context.Database
                    .ExecuteSqlRawAsync(
                        "EXEC NEW_DETALLE_COMPRA " +
                        "@p_ID_COMPRA, " +
                        "@p_ID_PRODUCTO, " +
                        "@p_CANTIDAD, " +
                        "@p_PRECIO_COMPRA",

                        new SqlParameter(
                            "@p_ID_COMPRA",
                            IdCompra),

                        new SqlParameter(
                            "@p_ID_PRODUCTO",
                            IdProducto),

                        new SqlParameter(
                            "@p_CANTIDAD",
                            Cantidad),

                        new SqlParameter(
                            "@p_PRECIO_COMPRA",
                            SqlDbType.Decimal)
                        {
                            Precision = 10,
                            Scale = 2,
                            Value = PrecioCompra
                        }
                    );


                TempData["Success"] =
                    "El producto fue agregado correctamente.";


                return RedirectToAction(
                    nameof(Details),
                    new { id = IdCompra });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo agregar el producto: " +
                    (ex.InnerException?.Message ??
                     ex.Message);

                return RedirectToAction(
                    nameof(AgregarDetalle),
                    new { id = IdCompra });
            }
        }


        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                var compra =
                    await _context.CompraProveedors
                        .FirstOrDefaultAsync(c =>
                            c.IdCompra == id);

                if (compra == null)
                    return NotFound();


                // =================================================
                // NO ELIMINAR RECIBIDA
                // =================================================

                if (compra.Estado == "Recibida")
                {
                    TempData["Error"] =
                        "No puedes eliminar una compra recibida.";

                    return RedirectToAction(
                        nameof(Index));
                }


                // =================================================
                // VERIFICAR DETALLES
                // =================================================

                var tieneDetalles =
                    await _context.DetalleCompras
                        .AnyAsync(d =>
                            d.IdCompra == id);


                if (tieneDetalles)
                {
                    TempData["Error"] =
                        "No puedes eliminar una compra que contiene productos. " +
                        "Puedes marcarla como Cancelada.";

                    return RedirectToAction(
                        nameof(Index));
                }


                // =================================================
                // ELIMINAR
                // =================================================

                _context.CompraProveedors.Remove(
                    compra);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "La compra fue eliminada correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo eliminar la compra: " +
                    (ex.InnerException?.Message ??
                     ex.Message);

                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =========================================================
        // CAMBIAR ESTADO MEDIANTE PROCEDIMIENTO
        // =========================================================

        private async Task CambiarEstadoConProcedimiento(
            int idCompra,
            string estado)
        {
            await _context.Database
                .ExecuteSqlRawAsync(
                    "EXEC UPD_ESTADO_COMPRA " +
                    "@p_ID_COMPRA, " +
                    "@p_ESTADO",

                    new SqlParameter(
                        "@p_ID_COMPRA",
                        idCompra),

                    new SqlParameter(
                        "@p_ESTADO",
                        estado)
                );
        }


        // =========================================================
        // CARGAR PROVEEDORES
        // =========================================================

        private async Task CargarProveedores(
            int? seleccionado = null)
        {
            var proveedores =
                await _context.Proveedors
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();


            ViewBag.IdProveedor =
                new SelectList(
                    proveedores,
                    "IdProveedor",
                    "Nombre",
                    seleccionado
                );
        }


        // =========================================================
        // CARGAR PROVEEDORES Y PRODUCTOS
        // =========================================================

        private async Task CargarListas(
            int? proveedorSeleccionado = null,
            int? productoSeleccionado = null)
        {
            var proveedores =
                await _context.Proveedors
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();


            var productos =
                await _context.Productos
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();


            ViewBag.IdProveedor =
                new SelectList(
                    proveedores,
                    "IdProveedor",
                    "Nombre",
                    proveedorSeleccionado
                );


            ViewBag.IdProducto =
                new SelectList(
                    productos,
                    "IdProducto",
                    "Nombre",
                    productoSeleccionado
                );
        }
    }
}
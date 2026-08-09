using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class ProductoController : Controller
    {
        private readonly TechStoreContext _context;

        public ProductoController(TechStoreContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: Producto
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var productos = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation);

            return View(await productos.ToListAsync());
        }


        // =========================================================
        // GET: Producto/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }


        // =========================================================
        // GET: Producto/Create
        // =========================================================
        public IActionResult Create()
        {
            CargarCategorias();
            CargarMarcas();

            return View();
        }


        // =========================================================
        // POST: Producto/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            // Estos campos los genera la base de datos/procedimiento
            ModelState.Remove(nameof(Producto.Estado));
            ModelState.Remove(nameof(Producto.FechaRegistro));

            // No necesitamos validar las propiedades de navegación
            ModelState.Remove(nameof(Producto.IdCategoriaNavigation));
            ModelState.Remove(nameof(Producto.IdMarcaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    var sql = @"
                        EXEC NEW_PRODUCTO
                            @p_NOMBRE,
                            @p_DESCRIPCION,
                            @p_PRECIO,
                            @p_STOCK,
                            @p_STOCK_MIN,
                            @p_ID_CATEGORIA,
                            @p_ID_MARCA";

                    var parameters = new[]
                    {
                        new SqlParameter("@p_NOMBRE", producto.Nombre),

                        new SqlParameter(
                            "@p_DESCRIPCION",
                            (object?)producto.Descripcion ?? DBNull.Value
                        ),

                        new SqlParameter("@p_PRECIO", producto.Precio),

                        new SqlParameter("@p_STOCK", producto.Stock),

                        new SqlParameter("@p_STOCK_MIN", producto.StockMin),

                        new SqlParameter("@p_ID_CATEGORIA", producto.IdCategoria),

                        new SqlParameter("@p_ID_MARCA", producto.IdMarca)
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        sql,
                        parameters
                    );

                    TempData["Success"] =
                        "Producto creado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] =
                        "No se pudo crear el producto: " + ex.Message;
                }
            }

            CargarCategorias(producto.IdCategoria);
            CargarMarcas(producto.IdMarca);

            return View(producto);
        }


        // =========================================================
        // GET: Producto/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            CargarCategorias(producto.IdCategoria);
            CargarMarcas(producto.IdMarca);

            return View(producto);
        }


        // =========================================================
        // POST: Producto/Edit
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Producto producto)
        {
            // El stock NO se modifica mediante UPD_PRODUCTO.
            // Se mantiene únicamente para mostrarlo en pantalla.
            ModelState.Remove(nameof(Producto.Stock));

            // Propiedades de navegación
            ModelState.Remove(nameof(Producto.IdCategoriaNavigation));
            ModelState.Remove(nameof(Producto.IdMarcaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    var sql = @"
                        EXEC UPD_PRODUCTO
                            @p_ID_PRODUCTO,
                            @p_NOMBRE,
                            @p_DESCRIPCION,
                            @p_PRECIO,
                            @p_STOCK_MIN,
                            @p_ESTADO,
                            @p_ID_CATEGORIA,
                            @p_ID_MARCA";

                    var parameters = new[]
                    {
                        new SqlParameter(
                            "@p_ID_PRODUCTO",
                            producto.IdProducto
                        ),

                        new SqlParameter(
                            "@p_NOMBRE",
                            producto.Nombre
                        ),

                        new SqlParameter(
                            "@p_DESCRIPCION",
                            (object?)producto.Descripcion ?? DBNull.Value
                        ),

                        new SqlParameter(
                            "@p_PRECIO",
                            producto.Precio
                        ),

                        new SqlParameter(
                            "@p_STOCK_MIN",
                            producto.StockMin
                        ),

                        new SqlParameter(
                            "@p_ESTADO",
                            producto.Estado
                        ),

                        new SqlParameter(
                            "@p_ID_CATEGORIA",
                            producto.IdCategoria
                        ),

                        new SqlParameter(
                            "@p_ID_MARCA",
                            producto.IdMarca
                        )
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        sql,
                        parameters
                    );

                    TempData["Success"] =
                        "Producto actualizado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] =
                        "No se pudo actualizar el producto: " + ex.Message;
                }
            }

            CargarCategorias(producto.IdCategoria);
            CargarMarcas(producto.IdMarca);

            return View(producto);
        }


        // =========================================================
        // GET: Producto/Delete/5
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }


        // =========================================================
        // POST: Producto/Delete
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int IdProducto)
        {
            try
            {
                var sql = @"
                    EXEC DEL_PRODUCTO
                        @p_ID_PRODUCTO";

                var parameter = new SqlParameter(
                    "@p_ID_PRODUCTO",
                    IdProducto
                );

                await _context.Database.ExecuteSqlRawAsync(
                    sql,
                    parameter
                );

                TempData["Success"] =
                    "Producto procesado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "No se pudo eliminar el producto: " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // CARGAR CATEGORÍAS
        // =========================================================
        private void CargarCategorias(int? idSeleccionado = null)
        {
            ViewData["IdCategoria"] = new SelectList(
                _context.Categoria
                    .OrderBy(c => c.Nombre)
                    .ToList(),
                "IdCategoria",
                "Nombre",
                idSeleccionado
            );
        }


        // =========================================================
        // CARGAR MARCAS
        // =========================================================
        private void CargarMarcas(int? idSeleccionado = null)
        {
            ViewData["IdMarca"] = new SelectList(
                _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .ToList(),
                "IdMarca",
                "Nombre",
                idSeleccionado
            );
        }


        // =========================================================
        // VERIFICAR SI EXISTE PRODUCTO
        // =========================================================
        private bool ProductoExists(int id)
        {
            return _context.Productos
                .Any(e => e.IdProducto == id);
        }
    }
}

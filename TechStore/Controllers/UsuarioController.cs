using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly TechStoreContext _context;

        public UsuarioController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation);
            return View(await usuarios.ToListAsync());
        }

        // GET: Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            // PRUEBA 1: saber si llega al controlador
            Console.WriteLine("========== CREATE ==========");
            Console.WriteLine("Nombre: " + usuario.Nombre);
            Console.WriteLine("Apellidos: " + usuario.Apellidos);
            Console.WriteLine("Correo: " + usuario.Correo);
            Console.WriteLine("Telefono: " + usuario.Telefono);
            Console.WriteLine("IdRol: " + usuario.IdRol);

            // PRUEBA 2: revisar ModelState
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                ViewBag.Error = "ModelState inválido: " +
                                string.Join(" | ", errores);

                return View(usuario);
            }

            try
            {
                Console.WriteLine("ModelState correcto.");
                Console.WriteLine("Ejecutando NEW_USUARIO...");

                var sql = @"EXEC NEW_USUARIO
                    @p_NOMBRE,
                    @p_APELLIDOS,
                    @p_CORREO,
                    @p_CONTRASENIA,
                    @p_TELEFONO,
                    @p_ID_ROL";

                var parameters = new[]
                {
            new SqlParameter("@p_NOMBRE", usuario.Nombre),
            new SqlParameter("@p_APELLIDOS", usuario.Apellidos),
            new SqlParameter("@p_CORREO", usuario.Correo),
            new SqlParameter("@p_CONTRASENIA", usuario.Contrasenia),
            new SqlParameter(
                "@p_TELEFONO",
                (object?)usuario.Telefono ?? DBNull.Value
            ),
            new SqlParameter("@p_ID_ROL", usuario.IdRol)
        };

                var resultado = await _context.Database
                    .ExecuteSqlRawAsync(sql, parameters);

                Console.WriteLine("NEW_USUARIO ejecutado.");
                Console.WriteLine("Resultado: " + resultado);

                TempData["Mensaje"] =
                    "Usuario creado correctamente. Filas afectadas: " + resultado;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR SQL:");
                Console.WriteLine(ex.ToString());

                ViewBag.Error = "ERROR SQL: " + ex.Message;

                return View(usuario);
            }
        }


        // GET: Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuario/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                ViewBag.Error = "ModelState inválido: " + string.Join(" | ", errores);

                return View(usuario);
            }

            try
            {
                var sql = @"EXEC UPD_USUARIO 
                    @p_ID_USUARIO, 
                    @p_NOMBRE, 
                    @p_APELLIDOS, 
                    @p_TELEFONO";

                var parameters = new[]
                {
            new SqlParameter("@p_ID_USUARIO", usuario.IdUsuario),
            new SqlParameter("@p_NOMBRE", usuario.Nombre),
            new SqlParameter("@p_APELLIDOS", usuario.Apellidos),
            new SqlParameter("@p_TELEFONO",
                (object?)usuario.Telefono ?? DBNull.Value)
        };

                var resultado = await _context.Database
                    .ExecuteSqlRawAsync(sql, parameters);

                TempData["Mensaje"] = "Usuario actualizado. Filas afectadas: " + resultado;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "ERROR SQL: " + ex.Message;

                return View(usuario);
            }
        }


        // GET: Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuario/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int IdUsuario)
        {
            try
            {
                var sql = "EXEC DEL_USUARIO @p_ID_USUARIO";

                var parameters = new[]
                {
            new SqlParameter("@p_ID_USUARIO", IdUsuario)
        };

                var resultado = await _context.Database
                    .ExecuteSqlRawAsync(sql, parameters);

                TempData["Mensaje"] =
                    "Proceso de eliminación ejecutado. Filas afectadas: " + resultado;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "ERROR SQL: " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

    }
}


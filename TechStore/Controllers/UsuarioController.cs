using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TechStore.Models;
using Microsoft.AspNetCore.Identity;

namespace TechStore.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly TechStoreContext _context;

        // Hasher para generar y verificar contraseñas
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public UsuarioController(TechStoreContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation);

            return View(await usuarios.ToListAsync());
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(
                    m => m.IdUsuario == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            Console.WriteLine("========== CREATE ==========");
            Console.WriteLine("Nombre: " + usuario.Nombre);
            Console.WriteLine("Apellidos: " + usuario.Apellidos);
            Console.WriteLine("Correo: " + usuario.Correo);
            Console.WriteLine("Telefono: " + usuario.Telefono);
            Console.WriteLine("IdRol: " + usuario.IdRol);


            // -----------------------------------------------------
            // VALIDAR MODELO
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .ToList();

                ViewBag.Error =
                    "ModelState inválido: " +
                    string.Join(" | ", errores);

                return View(usuario);
            }


            try
            {
                // -------------------------------------------------
                // VERIFICAR SI EL CORREO YA EXISTE
                // -------------------------------------------------

                var correoExiste =
                    await _context.Usuarios
                        .AnyAsync(u =>
                            u.Correo == usuario.Correo);

                if (correoExiste)
                {
                    ViewBag.Error =
                        "Ya existe un usuario registrado con ese correo.";

                    return View(usuario);
                }


                // -------------------------------------------------
                // VALIDAR CONTRASEÑA
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    usuario.Contrasenia))
                {
                    ViewBag.Error =
                        "La contraseña es obligatoria.";

                    return View(usuario);
                }


                // -------------------------------------------------
                // GENERAR HASH
                // -------------------------------------------------

                string contraseniaHash =
                    _passwordHasher.HashPassword(
                        usuario,
                        usuario.Contrasenia);


                Console.WriteLine(
                    "Contraseña hasheada correctamente.");


                // -------------------------------------------------
                // PROCEDIMIENTO NEW_USUARIO
                // -------------------------------------------------

                var sql = @"
                    EXEC NEW_USUARIO
                        @p_NOMBRE,
                        @p_APELLIDOS,
                        @p_CORREO,
                        @p_CONTRASENIA,
                        @p_TELEFONO,
                        @p_ID_ROL";


                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_NOMBRE",
                        usuario.Nombre),

                    new SqlParameter(
                        "@p_APELLIDOS",
                        usuario.Apellidos),

                    new SqlParameter(
                        "@p_CORREO",
                        usuario.Correo),

                    // IMPORTANTE:
                    // aquí mandamos el HASH,
                    // NO la contraseña original
                    new SqlParameter(
                        "@p_CONTRASENIA",
                        contraseniaHash),

                    new SqlParameter(
                        "@p_TELEFONO",
                        (object?)usuario.Telefono
                        ?? DBNull.Value),

                    new SqlParameter(
                        "@p_ID_ROL",
                        usuario.IdRol)
                };


                var resultado =
                    await _context.Database
                        .ExecuteSqlRawAsync(
                            sql,
                            parameters);


                Console.WriteLine(
                    "NEW_USUARIO ejecutado.");

                Console.WriteLine(
                    "Resultado: " + resultado);


                TempData["Mensaje"] =
                    "Usuario creado correctamente.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "ERROR SQL:");

                Console.WriteLine(
                    ex.ToString());


                ViewBag.Error =
                    "ERROR SQL: " +
                    (ex.InnerException?.Message
                    ?? ex.Message);


                return View(usuario);
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var usuario =
                await _context.Usuarios
                    .FindAsync(id);


            if (usuario == null)
                return NotFound();


            return View(usuario);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrEmpty(x))
                    .ToList();

                ViewBag.Error =
                    "ModelState inválido: " +
                    string.Join(" | ", errores);

                return View(usuario);
            }


            try
            {
                // -------------------------------------------------
                // ACTUALIZAR DATOS DEL USUARIO
                // -------------------------------------------------

                var sql = @"
                    EXEC UPD_USUARIO
                        @p_ID_USUARIO,
                        @p_NOMBRE,
                        @p_APELLIDOS,
                        @p_TELEFONO";


                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_ID_USUARIO",
                        usuario.IdUsuario),

                    new SqlParameter(
                        "@p_NOMBRE",
                        usuario.Nombre),

                    new SqlParameter(
                        "@p_APELLIDOS",
                        usuario.Apellidos),

                    new SqlParameter(
                        "@p_TELEFONO",
                        (object?)usuario.Telefono
                        ?? DBNull.Value)
                };


                var resultado =
                    await _context.Database
                        .ExecuteSqlRawAsync(
                            sql,
                            parameters);


                TempData["Mensaje"] =
                    "Usuario actualizado correctamente.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "ERROR SQL: " +
                    (ex.InnerException?.Message
                    ?? ex.Message);

                return View(usuario);
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var usuario =
                await _context.Usuarios
                    .Include(u =>
                        u.IdRolNavigation)
                    .FirstOrDefaultAsync(
                        m => m.IdUsuario == id);


            if (usuario == null)
                return NotFound();


            return View(usuario);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int IdUsuario)
        {
            try
            {
                var sql =
                    "EXEC DEL_USUARIO @p_ID_USUARIO";


                var parameters = new[]
                {
                    new SqlParameter(
                        "@p_ID_USUARIO",
                        IdUsuario)
                };


                var resultado =
                    await _context.Database
                        .ExecuteSqlRawAsync(
                            sql,
                            parameters);


                TempData["Mensaje"] =
                    "Usuario eliminado correctamente.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] =
                    "ERROR SQL: " +
                    (ex.InnerException?.Message
                    ?? ex.Message);


                return RedirectToAction(
                    nameof(Index));
            }
        }
    }
}
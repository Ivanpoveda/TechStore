using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TechStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace TechStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly TechStoreContext _context;

        public AccountController(TechStoreContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login

// POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasenia)
        {
            string hashContrasenia = HashPassword(contrasenia);

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo &&
                    u.Contrasenia == hashContrasenia &&
                    u.Estado == "Activo");

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            // =====================================================
            // OBTENER NOMBRE DEL ROL
            // =====================================================

            string nombreRol = usuario.IdRolNavigation?.Nombre ?? "";

            // =====================================================
            // VALIDAR ROL
            // =====================================================

            if (string.IsNullOrWhiteSpace(nombreRol))
            {
                ViewBag.Error = "El usuario no tiene un rol asignado.";
                return View();
            }

            // =====================================================
            // CREAR CLAIMS
            // =====================================================

            var claims = new List<Claim>
                {
                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    nombreRol
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // =====================================================
            // CREAR COOKIE DE AUTENTICACIÓN
            // =====================================================

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // =====================================================
            // REDIRECCIÓN SEGÚN ROL
            // =====================================================

            if (usuario.IdRol == 1)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }

            if (usuario.IdRol == 2)
            {
                return RedirectToAction(
                    "Catalogo",
                    "Cliente"
                );
            }

            return RedirectToAction(
                "AccessDenied",
                "Account"
            );
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Usuario nuevoUsuario)
        {
            string hashContrasenia =
                HashPassword(nuevoUsuario.Contrasenia);

            var sql = @"INSERT INTO USUARIO (NOMBRE, APELLIDOS, CORREO, CONTRASENIA, TELEFONO, FECHA_REGISTRO, ESTADO, ID_ROL)
VALUES (@Nombre, @Apellidos, @Correo, @Contrasenia, @Telefono, @FechaRegistro, @Estado, @IdRol);";



            var parameters = new[]
            {
                new SqlParameter("@Nombre", nuevoUsuario.Nombre),

                new SqlParameter("@Apellidos", nuevoUsuario.Apellidos),

                new SqlParameter("@Correo", nuevoUsuario.Correo),

                new SqlParameter("@Contrasenia", hashContrasenia),

                new SqlParameter(
                    "@Telefono",
                    (object?)nuevoUsuario.Telefono ?? DBNull.Value
                ),

                new SqlParameter(
                    "@FechaRegistro",
                    DateTime.Now
                ),

                new SqlParameter(
                    "@Estado",
                    "Activo"
                ),

                new SqlParameter(
                    "@IdRol",
                    2
                )
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    sql,
                    parameters
                );

                return RedirectToAction("Login", "Account");
            }
            catch (SqlException)
            {
                ViewBag.Error =
                    "No se pudo registrar el usuario. " +
                    "Verifique que el correo no esté registrado.";

                return View(nuevoUsuario);
            }
            catch (DbUpdateException)
            {
                ViewBag.Error =
                    "Ocurrió un error al guardar el usuario.";

                return View(nuevoUsuario);
            }
        }


        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Account");
        }


        // Hash de contraseña
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();

            var bytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(password)
            );

            return Convert.ToBase64String(bytes);
        }
    }
}
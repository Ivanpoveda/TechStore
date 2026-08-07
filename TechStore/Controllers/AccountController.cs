using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechStore.Models;
using Oracle.ManagedDataAccess.Client; // Necesario para OracleParameter

namespace TechStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly TiendaDbContext _context;

        public AccountController(TiendaDbContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // GET: AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasenia)
        {
            var usuario = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u => u.Correo == correo && u.Contrasenia == contrasenia);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.IdRolNavigation.Nombre)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirigir según rol
            if (usuario.IdRolNavigation?.Nombre == "Administrador")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Catalogo", "Cliente");
            }
        }

        // GET: Registro
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario nuevoUsuario)
        {
            var sql = @"BEGIN NEW_USUARIO(?, ?, ?, ?, ?, ?); END;";

            var parameters = new[]
            {
        new OracleParameter { Value = nuevoUsuario.Nombre },
        new OracleParameter { Value = nuevoUsuario.Apellidos },
        new OracleParameter { Value = nuevoUsuario.Correo },
        new OracleParameter { Value = nuevoUsuario.Contrasenia },
        new OracleParameter { Value = nuevoUsuario.Telefono ?? string.Empty },
        new OracleParameter { Value = 2 } // Cliente
    };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);

            return RedirectToAction("Login");
        }


        // POST: Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}




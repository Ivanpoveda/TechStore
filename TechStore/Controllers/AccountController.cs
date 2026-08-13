using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly TechStoreContext _context;

        public AccountController(TechStoreContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string correo,
            string contrasenia)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo &&
                    u.Estado == "Activo");

            if (usuario == null)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

                return View();
            }




            bool contraseñaCorrecta = false;




            string hashSha256 =
                HashPassword(contrasenia);

            if (usuario.Contrasenia == hashSha256)
            {
                contraseñaCorrecta = true;
            }




            if (!contraseñaCorrecta &&
                usuario.Contrasenia.StartsWith("AQAAAA"))
            {
                var passwordHasher =
                    new PasswordHasher<Usuario>();

                var resultado =
                    passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.Contrasenia,
                        contrasenia
                    );

                if (resultado ==
                    PasswordVerificationResult.Success ||
                    resultado ==
                    PasswordVerificationResult.SuccessRehashNeeded)
                {
                    contraseñaCorrecta = true;
                }
            }



            if (!contraseñaCorrecta)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

                return View();
            }




            string nombreRol =
                usuario.IdRolNavigation?.Nombre ?? "";


            if (string.IsNullOrWhiteSpace(nombreRol))
            {
                ViewBag.Error =
                    "El usuario no tiene un rol asignado.";

                return View();
            }




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


            var principal =
                new ClaimsPrincipal(identity);




            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );



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




        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            Usuario nuevoUsuario)
        {
            if (!ModelState.IsValid)
            {
                return View(nuevoUsuario);
            }


            try
            {

                string hashContrasenia =
                    HashPassword(nuevoUsuario.Contrasenia);


                var sql = @"
                    INSERT INTO USUARIO
                    (
                        NOMBRE,
                        APELLIDOS,
                        CORREO,
                        CONTRASENIA,
                        TELEFONO,
                        FECHA_REGISTRO,
                        ESTADO,
                        ID_ROL
                    )
                    VALUES
                    (
                        @Nombre,
                        @Apellidos,
                        @Correo,
                        @Contrasenia,
                        @Telefono,
                        @FechaRegistro,
                        @Estado,
                        @IdRol
                    );";


                var parameters = new[]
                {
                    new SqlParameter(
                        "@Nombre",
                        nuevoUsuario.Nombre
                    ),

                    new SqlParameter(
                        "@Apellidos",
                        nuevoUsuario.Apellidos
                    ),

                    new SqlParameter(
                        "@Correo",
                        nuevoUsuario.Correo
                    ),

                    new SqlParameter(
                        "@Contrasenia",
                        hashContrasenia
                    ),

                    new SqlParameter(
                        "@Telefono",
                        (object?)nuevoUsuario.Telefono
                        ?? DBNull.Value
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


                await _context.Database
                    .ExecuteSqlRawAsync(
                        sql,
                        parameters
                    );


                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "No se pudo registrar el usuario: " +
                    ex.Message;

                return View(nuevoUsuario);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "Login",
                "Account"
            );
        }



        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }



        private string HashPassword(string password)
        {
            using var sha256 =
                SHA256.Create();

            var bytes =
                sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(password)
                );

            return Convert.ToBase64String(bytes);
        }
    }
}
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Conexión a la base TechStore
builder.Services.AddDbContext<TechStoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TechStoreDb")
    )
);

// Autenticación con cookies
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    // Página de inicio de sesión
    options.LoginPath = "/Account/Login";

    // Página de acceso denegado
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Ruta para cerrar sesión
    options.LogoutPath = "/Account/Logout";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
)
.WithStaticAssets();

app.Run();

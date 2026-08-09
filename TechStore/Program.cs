using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC con runtime compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

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
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

var app = builder.Build();

// Configuración del pipeline
if (app.Environment.IsDevelopment())
{
    // 🔎 Mostrar errores detallados en desarrollo
    app.UseDeveloperExceptionPage();
}
else
{
    // ⚠️ Manejo de errores en producción
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();


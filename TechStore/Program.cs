using Microsoft.EntityFrameworkCore;
using TechStore.Models; // Aquí está tu TiendaDbContext

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar DbContext con Oracle usando la cadena de conexión de appsettings.json
builder.Services.AddDbContext<TiendaDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("TechStoreConnection")));

// 2. Registrar servicios MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Seguridad: Strict Transport Security
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Necesario para CSS, JS, imágenes

app.UseRouting();

app.UseAuthentication(); // Si luego agregas login con Identity
app.UseAuthorization();

// 4. Rutas por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


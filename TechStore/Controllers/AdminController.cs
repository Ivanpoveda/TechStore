using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;
using TechStore.ViewModels;
using System.Linq;

namespace TechStore.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly TiendaDbContext _context;

        public AdminController(TiendaDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var ingresosTotales = _context.Ventas.Sum(v => v.Total);
            var nuevosClientes = _context.Usuarios.Count(u => u.IdRol == 2);
            var ordenesPendientes = _context.Ventas.Count(v => v.Estado == "Pendiente");

            var model = new DashboardViewModel
            {
                IngresosTotales = (decimal)ingresosTotales,
                NuevosClientes = nuevosClientes,
                OrdenesPendientes = ordenesPendientes,
                UltimosPedidos = _context.Ventas
                    .OrderByDescending(v => v.Fecha)
                    .Take(5)
                    .Select(v => new PedidoDto
                    {
                        Cliente = v.IdUsuarioNavigation.Nombre,
                        Estado = v.Estado,
                        Monto = (decimal)v.Total
                    }).ToList()
            };

            return View(model);
        }
    }
}

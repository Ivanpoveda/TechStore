using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TechStore.Models;
using TechStore.ViewModels;

namespace TechStore.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly TechStoreContext _context;

        public AdminController(TechStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var IngresosTotales = _context.Venta
    .Where(v => v.Estado == "Completada")
    .Select(v => (decimal?)v.Total)
    .Sum() ?? 0;

            var usuariosActivos = await _context.Usuarios
                .CountAsync(u => u.Estado == "Activo");

            var productosEnStock = await _context.Productos
                .SumAsync(p => (int?)p.Stock) ?? 0;

            var ordenesPendientes = await _context.Venta
                .CountAsync(v => v.Estado == "Pendiente");

            var ventasAgrupadasMes = await _context.Venta
                .Where(v => v.Fecha.Year == DateTime.Now.Year && v.Estado == "Completada")
                .GroupBy(v => v.Fecha.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(v => (decimal?)v.Total) ?? 0m
                })
                .OrderBy(x => x.Mes)
                .ToListAsync();

            var culturaEsp = new CultureInfo("es-CR");
            var meses = ventasAgrupadasMes
                .Select(x => culturaEsp.DateTimeFormat.GetMonthName(x.Mes))
                .ToList();

            var montosMes = ventasAgrupadasMes
                .Select(x => x.Total)
                .ToList();


            var ventasAgrupadasEstado = await _context.Venta
                .GroupBy(v => v.Estado)
                .Select(g => new
                {
                    Estado = g.Key ?? "Sin Estado",
                    Cantidad = g.Count()
                })
                .ToListAsync();

            var ultimosPedidos = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .Select(v => new PedidoDto
                {
                    Cliente = (v.IdUsuarioNavigation != null)
                        ? (v.IdUsuarioNavigation.Nombre + " " + v.IdUsuarioNavigation.Apellidos)
                        : "Cliente General",
                    Estado = v.Estado,
                    Monto = v.Total.GetValueOrDefault()
                })
                .ToListAsync();

            var productosStockBajo = await _context.VisStockBajos.ToListAsync();

            var detalleVentas = await _context.VisVentasDetalles
                .OrderByDescending(v => v.Fecha)
                .Take(10)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                IngresosTotales = IngresosTotales,
                UsuariosActivos = usuariosActivos,
                ProductosEnStock = productosEnStock,
                OrdenesPendientes = ordenesPendientes,
                UltimosPedidos = ultimosPedidos,
                Meses = meses,
                VentasPorMes = montosMes,
                Estados = ventasAgrupadasEstado.Select(x => x.Estado).ToList(),
                VentasPorEstado = ventasAgrupadasEstado.Select(x => x.Cantidad).ToList(),
                ProductosStockBajo = productosStockBajo,
                DetalleVentas = detalleVentas
            };

            return View(model);
        }
    }
}
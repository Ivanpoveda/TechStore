using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class HistorialInventarioController : Controller
    {
        private readonly TechStoreContext _context;

        public HistorialInventarioController(TechStoreContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var historial = await _context.VisHistorialInventarios
                .OrderByDescending(h => h.Fecha)
                .ThenByDescending(h => h.IdMovimiento)
                .ToListAsync();

            return View(historial);
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace TechStore.Controllers
{
    public class ClienteController : Controller
    {
        public IActionResult Catalogo()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStore.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        public IActionResult Catalogo()
        {
            return View();
        }
    }
}

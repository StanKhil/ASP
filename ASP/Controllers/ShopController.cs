using Microsoft.AspNetCore.Mvc;

namespace ASP.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Admin()
        {
            return View();
        }
    }
}

using System.Diagnostics;
using ASP.Models;
using ASP.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Razor()
        {
            HomeRazorPageModel model = new()
            {
                Arr = ["item1", "item2", "item3", "item4", "item5"]
            };
            return View(model);
        }

        public IActionResult Demo()
        {
            Product product1 = new() { Name = "Product1", Price = 10, Amount = 1 };
            Product product2 = new() { Name = "Product2", Price = 100, Amount = 20 };
            Product product3 = new() { Name = "Product3", Price = 25, Amount = 4 };

            HomeDemoPageModel model = new()
            {
                Products = [product1, product2, product3]
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TrucksWeighingWebApp.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

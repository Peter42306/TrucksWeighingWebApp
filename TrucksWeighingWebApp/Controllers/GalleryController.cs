using Microsoft.AspNetCore.Mvc;

namespace TrucksWeighingWebApp.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

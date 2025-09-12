using Microsoft.AspNetCore.Mvc;

namespace TrucksWeighingWebApp.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

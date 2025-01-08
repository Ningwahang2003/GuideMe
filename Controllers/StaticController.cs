using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Controllers
{
    public class StaticController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

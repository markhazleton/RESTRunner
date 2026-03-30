using Microsoft.AspNetCore.Mvc;

namespace RequestSpark.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}


using Microsoft.AspNetCore.Mvc;

namespace RESTRunner.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

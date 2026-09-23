using Microsoft.AspNetCore.Mvc;

namespace LTWebAPi.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

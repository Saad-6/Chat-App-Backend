using Microsoft.AspNetCore.Mvc;

namespace Chat_App.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

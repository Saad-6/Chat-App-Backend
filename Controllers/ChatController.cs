using Microsoft.AspNetCore.Mvc;

namespace Chat_App.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

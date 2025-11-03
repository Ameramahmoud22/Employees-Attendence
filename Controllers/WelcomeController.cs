using Microsoft.AspNetCore.Mvc;

namespace Employees_Attendence.Controllers
{
    public class WelcomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnterSystem()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}

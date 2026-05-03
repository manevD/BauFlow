using BauFlow.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BauFlow.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Wenn eingeloggt UND noch nicht weitergeleitet
            if (User.Identity.IsAuthenticated &&
                HttpContext.Session.GetString("Redirected") != "true")
            {
                HttpContext.Session.SetString("Redirected", "true");

                return RedirectToAction("Index", "Customers");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error/{code}")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace BauFlow.Controllers
{
    public class BillingController : Controller
    {
        public IActionResult Expired() => View();

        public IActionResult Locked() => View();
    }
}

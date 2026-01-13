using Microsoft.AspNetCore.Mvc;

namespace CodeReviews.Controllers
{
    public class ReviewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ReviewRequests()
        {
            return View();
        }
    }
}

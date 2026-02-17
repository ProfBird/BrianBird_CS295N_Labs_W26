using CodeReviews.Data;
using CodeReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Controllers
{
    public class ReviewController : Controller
    {
        AppDbContext context;

        // constructor
        public ReviewController(AppDbContext c)

        {
            context = c;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var reviews = context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Submission)
                .ToList();
            return View(reviews);
        }

        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Review(Review review)
        {
            review.ReviewDate = DateTime.Now;

            // Use the seeded dummy submission
            var dummySubmission = context.Submissions.FirstOrDefault();
            review.Submission = dummySubmission;

            context.Reviews.Add(review);
            context.SaveChanges();
            return RedirectToAction("List");
        }
    }
}

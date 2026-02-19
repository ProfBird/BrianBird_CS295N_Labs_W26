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

        public IActionResult Filter(string reviewer, string date)
        {
            var reviews = context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Submission)
                .ToList()
                .Where(r => reviewer == null || r.Reviewer.Name == reviewer)
                .Where(r => date == null || r.ReviewDate == DateOnly.Parse(date))
                .ToList();
            return View("List", reviews);
        }

        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Review(Review review)
        {
            review.ReviewDate = DateOnly.FromDateTime(DateTime.Now);

            // Use the seeded dummy submission
            var dummySubmission = context.Submissions.FirstOrDefault();
            review.Submission = dummySubmission;

            context.Reviews.Add(review);
            context.SaveChanges();
            return RedirectToAction("List");
        }
    }
}

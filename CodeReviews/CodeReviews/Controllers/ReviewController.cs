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

        // Lists all the reviews
        public IActionResult List()
        {
            var reviews = context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Submission)
                .ToList();
            return View(reviews);
        }

        public IActionResult Submissions()
        {
            var submissions = context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .ToList();
            return View(submissions);
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

        // Open the review entry form
        [HttpGet]
        public IActionResult Review(int? id)
        {
            var review = new Review();
            
            if (id.HasValue)
            {
                var submission = context.Submissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment)
                    .FirstOrDefault(s => s.SubmissionId == id.Value);
                
                if (submission != null)
                {
                    review.Submission = submission;
                }
            }
            
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int submissionId, Review review)
        {
            var submission = await context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .ThenInclude(a => a.ClassSection)
                .ThenInclude(sec => sec.Course)
                .Include(s => s.Assignment)
                .ThenInclude(a => a.ClassSection)
                .ThenInclude(sec => sec.Instructor)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission is null)
            {
                return NotFound();
            }
            review.Submission = submission;
            
            ModelState.Clear();
            TryValidateModel(review);
            if (!ModelState.IsValid)
            {
                return View(review);
            }
            review.ReviewDate = DateOnly.FromDateTime(DateTime.Now);

            context.Reviews.Add(review);
            context.SaveChanges();
            return RedirectToAction("List");
        }
    }
}

using CodeReviews.Models;

namespace CodeReviews.Data
{
    public class SeedData
    {
        public static void Seed(AppDbContext context)

        {

            if (!context.Reviews.Any())  // this is to prevent adding duplicate data
            {

                // Create AppUser objects

                AppUser reviewer1 = new AppUser { Name = "Ada Lovelace" };

                AppUser reviewer2 = new AppUser { Name = "Charles Babage" };

                // Queue up AppUser objects to be saved to the DB
                context.AppUsers.Add(reviewer1);
                context.AppUsers.Add(reviewer2);
                context.SaveChanges();  // Saving adds AppUserId to AppUser objects

                Submission submission1 = new Submission
                {
                    CodeUrl = "",
                    Version = "A",
                    SubmissionDate = DateTime.Now,
                    Student = new AppUser { Name = "Dummy Student" },
                    Assignment = new Assignment
                    {
                        AssignmentName = "Dummy Assignment",
                        DraftDueDate = DateOnly.FromDateTime(DateTime.Now),
                        ReviewDueDate = DateOnly.FromDateTime(DateTime.Now),
                        FinalDueDate = DateOnly.FromDateTime(DateTime.Now),
                        ClassSection = new Section
                        {
                            SectionNumber = 0,
                            Day = "TBD",
                            StartTime = "TBD",
                            Modality = "TBD",
                            Term = "TBD",
                            Year = DateTime.Now.Year,
                            Course = new Course
                            {
                                CoursePrefix = "CS",
                                CourseNumber = "000",
                                CourseName = "Dummy Course"
                            },
                            Instructor = new AppUser { Name = "Dummy Instructor" }
                        }
                    }
                };

                context.Submissions.Add(submission1);
                context.SaveChanges();

                // Create Review objects
                Review review1 = new Review
                {
                    Reviewer = reviewer1,
                    Submission = submission1,
                    ReviewDate = DateTime.Now.AddDays(-2),
                    Comments = "Great code structure and clear variable names. " +
                    "Consider adding more comments to explain the algorithm logic. " +
                    "Overall well done!",
                    ReviewUrl = "https://github.com/example/pull/123#discussion_r12345"
                };
                context.Reviews.Add(review1);

                Review review2 = new Review
                {
                    Reviewer = reviewer2,
                    Submission = submission1,
                    ReviewDate = DateTime.Now.AddDays(-1),
                    Comments = "Code works correctly and handles edge cases well. " +
                    "Nice use of helper functions to break down the problem. " +
                    "Minor: could optimize the loop in line 42.",
                    ReviewUrl = "https://github.com/example/pull/124#discussion_r12346"
                };
                context.Reviews.Add(review2);

                context.SaveChanges();  // Save all reviews to the database

            }
        }
    }
}

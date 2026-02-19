using CodeReviews.Models;

namespace CodeReviews.Data
{
    public class SeedData
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Reviews.Any())  // this is to prevent adding duplicate data
            {
                // Create User objects
                AppUser reviewer1 = new AppUser { Name = "Ada Lovelace" };
                AppUser reviewer2 = new AppUser { Name = "Charles Babage" };
                // Queue up user objects to be saved to the DB
                context.AppUsers.Add(reviewer1);
                context.AppUsers.Add(reviewer2);
                context.SaveChanges();  // Saving adds Id to User objects

                // Create dummy submission
                Submission dummySubmission = new Submission
                {
                    CodeUrl = "https://github.com/student/assignment-submission",
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
                context.Submissions.Add(dummySubmission);
                context.SaveChanges();  // Save dummy submission to get its Id

                Review review = new Review
                {
                    Reviewer = reviewer1,
                    ReviewDate = new DateOnly(2026, 2, 10),
                    Comments = "Great code structure and clear variable names. " +
                    "Consider adding more comments to explain the algorithm logic. " +
                    "Overall well done!",
                    ReviewUrl = "https://github.com/example/pull/123#discussion_r12345",
                    Submission = dummySubmission
                };
                context.Reviews.Add(review);  // queues up a review to be added to the DB

                Review review2 = new Review
                {
                    Reviewer = reviewer2,
                    ReviewDate = new DateOnly(2025, 6, 3),
                    Comments = "Code works correctly and handles edge cases well. " +
                    "Nice use of helper functions to break down the problem. " +
                    "Minor: could optimize the loop in line 42.",
                    ReviewUrl = "https://github.com/example/pull/124#discussion_r12346",
                    Submission = dummySubmission
                };
                context.Reviews.Add(review2);  // queues up the second review to be added to the DB

                context.SaveChanges();  // Save all reviews to the database
            }
        }
    }
}

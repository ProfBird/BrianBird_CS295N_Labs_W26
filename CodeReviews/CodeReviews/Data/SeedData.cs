using CodeReviews.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Data
{
    public class SeedData
    {
        public static void Seed(AppDbContext context, IServiceProvider provider)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define roles
            string[] roleNames = { "Admin", "Instructor", "Student" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                if (!context.Roles.Any(r => r.Name == roleName))
                {
                    roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
                }
            }

            /*** This seed data is just for testing. Disable it for production ***/
            if (!context.Reviews.Any())  // this is to prevent adding duplicate data
            {
                var userManager = provider.GetRequiredService<UserManager<AppUser>>();
                const string SECRET_PASSWORD = "Secret!123";
                // Create User objects

                AppUser reviewer1 = new AppUser { Name = "Ada Lovelace" };
                AppUser reviewer2 = new AppUser { Name = "Charles Babage" };
                AppUser student1 = new AppUser { Name = "Grace Hopper" };
                AppUser student2 = new AppUser { Name = "Alan Turing" };
                AppUser student3 = new AppUser { Name = "Margaret Hamilton" };
                AppUser instructor1 = new AppUser { Name = "Donald Knuth" };
                AppUser instructor2 = new AppUser { Name = "Barbara Liskov" };

                // Create users
                // TODO: Check result after creating each user to see if it succeedded
                var result = userManager.CreateAsync(reviewer1, SECRET_PASSWORD);
                result = userManager.CreateAsync(reviewer2, SECRET_PASSWORD);
                result = userManager.CreateAsync(student1, SECRET_PASSWORD);
                result = userManager.CreateAsync(student2, SECRET_PASSWORD);
                result = userManager.CreateAsync(student3, SECRET_PASSWORD);
                result = userManager.CreateAsync(instructor1, SECRET_PASSWORD);
                result = userManager.CreateAsync(instructor2, SECRET_PASSWORD);

                // Create Course objects
                Course course1 = new Course
                {
                    CoursePrefix = "CS",
                    CourseNumber = "161",
                    CourseName = "Introduction to Computer Science"
                };
                Course course2 = new Course
                {
                    CoursePrefix = "CS",
                    CourseNumber = "295N",
                    CourseName = "Web Development"
                };
                context.Courses.Add(course1);
                context.Courses.Add(course2);
                context.SaveChanges();

                // Create Section objects
                Section section1 = new Section
                {
                    SectionNumber = 12345,
                    Day = "MW",
                    StartTime = "10:00 AM",
                    Modality = "Hybrid",
                    Term = "Winter",
                    Year = 2026,
                    Course = course1,
                    Instructor = instructor1
                };
                Section section2 = new Section
                {
                    SectionNumber = 23456,
                    Day = "TuTh",
                    StartTime = "2:00 PM",
                    Modality = "Online",
                    Term = "Winter",
                    Year = 2026,
                    Course = course2,
                    Instructor = instructor2
                };
                context.Sections.Add(section1);
                context.Sections.Add(section2);
                context.SaveChanges();

                // Create Assignment objects
                Assignment assignment1 = new Assignment
                {
                    AssignmentName = "Lab01 Variables",
                    DraftDueDate = new DateTime(2026, 1, 20),
                    ReviewDueDate = new DateTime(2026, 1, 27),
                    FinalDueDate = new DateTime(2026, 2, 3),
                    ClassSection = section1,
                    Versions = new List<AssignmentVersion>
        {
            new AssignmentVersion { VersionName = "A", Description = "Console version", InstructionsLink = "https://example.com/lab01a" },
            new AssignmentVersion { VersionName = "B", Description = "GUI version", InstructionsLink = "https://example.com/lab01b" }
        }
                };
                Assignment assignment2 = new Assignment
                {
                    AssignmentName = "Lab02 Branching",
                    DraftDueDate = new DateTime(2026, 2, 3),
                    ReviewDueDate = new DateTime(2026, 2, 10),
                    FinalDueDate = new DateTime(2026, 2, 17),
                    ClassSection = section1,
                    Versions = new List<AssignmentVersion>
        {
            new AssignmentVersion { VersionName = "A", Description = "If-else logic", InstructionsLink = "https://example.com/lab02a" },
            new AssignmentVersion { VersionName = "B", Description = "Switch-case logic", InstructionsLink = "https://example.com/lab02b" }
        }
                };
                Assignment assignment3 = new Assignment
                {
                    AssignmentName = "Lab03 MVC",
                    DraftDueDate = new DateTime(2026, 2, 10),
                    ReviewDueDate = new DateTime(2026, 2, 17),
                    FinalDueDate = new DateTime(2026, 2, 24),
                    ClassSection = section2,
                    Versions = new List<AssignmentVersion>
        {
            new AssignmentVersion { VersionName = "Standard", Description = "Basic MVC pattern", InstructionsLink = "https://example.com/lab03" }
        }
                };
                context.Assignments.Add(assignment1);
                context.Assignments.Add(assignment2);
                context.Assignments.Add(assignment3);
                context.SaveChanges();

                // Create Submission objects
                Submission submission1 = new Submission
                {
                    CodeUrl = "https://github.com/ghopper/lab01-variables",
                    Version = "A",
                    SubmissionDate = new DateTime(2026, 1, 26),
                    Student = student1,
                    Assignment = assignment1
                };
                Submission submission2 = new Submission
                {
                    CodeUrl = "https://github.com/aturing/lab02-branching",
                    Version = "B",
                    SubmissionDate = new DateTime(2026, 2, 9),
                    Student = student2,
                    Assignment = assignment2
                };
                Submission submission3 = new Submission
                {
                    CodeUrl = "https://github.com/mhamilton/lab03-mvc",
                    Version = null,
                    SubmissionDate = new DateTime(2026, 2, 16),
                    Student = student3,
                    Assignment = assignment3
                };
                context.Submissions.Add(submission1);
                context.Submissions.Add(submission2);
                context.Submissions.Add(submission3);
                context.SaveChanges();  // Save submissions to get their Ids

                // Create Review objects
                Review review1 = new Review
                {
                    Reviewer = reviewer1,
                    ReviewDate = new DateTime(2026, 1, 28),
                    Comments = "Great code structure and clear variable names. " +
                    "Consider adding more comments to explain the algorithm logic. " +
                    "Overall well done!",
                    ReviewUrl = "https://github.com/ghopper/lab01-variables/pull/1",
                    Submission = submission1
                };
                Review review2 = new Review
                {
                    Reviewer = reviewer2,
                    ReviewDate = new DateTime(2026, 2, 11),
                    Comments = "Code works correctly and handles edge cases well. " +
                    "Nice use of helper functions to break down the problem. " +
                    "Minor: could optimize the loop in line 42.",
                    ReviewUrl = "https://github.com/aturing/lab02-branching/pull/2",
                    Submission = submission2
                };
                context.Reviews.Add(review1);
                context.Reviews.Add(review2);

                context.SaveChanges();  // Save all reviews to the database
            }

        }

    }
}

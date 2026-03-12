using CodeReviews.Data;
using CodeReviews.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/* This controller manages user and role administration.
   Requires Admin role authorization (can be commented out for development).
   Adapted in 2026 by Brian Bird from code accompanying Murach's ASP.NET Core MVC 2nd Ed.
   Refactoring assisted by GitHub Copilot, 3/12/2026.
*/
namespace CodeReviews.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private UserManager<AppUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private AppDbContext dbContext;
        public UserController(UserManager<AppUser> userMngr, RoleManager<IdentityRole> roleMngr, AppDbContext context)
        {
            userManager = userMngr;
            roleManager = roleMngr;
            dbContext = context;
        }
        public IActionResult Index()
        {
            List<AppUser> users = userManager.Users.ToList();
            foreach (AppUser user in users)
            //foreach (AppUser user in userManager.Users)
            //AppUser user = userManager.FindByNameAsync("admin").Result;
            {
                // user.RoleNames = await userManager.GetRolesAsync(user);
                var task = userManager.GetRolesAsync(user);
                task.Wait();
                user.RoleNames = task.Result;
                // users.Add(user);
            }
            UserVM model = new UserVM
            {
                Users = users,
                Roles = roleManager.Roles
            }; return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser? user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["message"] = "User not found.";
                return RedirectToAction("Index");
            }

            UserDeleteImpactVM model = new UserDeleteImpactVM
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Name = user.Name,
                SectionsCount = await dbContext.Sections.CountAsync(s => EF.Property<string>(s, "InstructorId") == id),
                SubmissionsCount = await dbContext.Submissions.CountAsync(s => EF.Property<string>(s, "StudentId") == id),
                ReviewsCount = await dbContext.Reviews.CountAsync(r => EF.Property<string>(r, "ReviewerId") == id)
            };

            return View("DeleteConfirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            AppUser? user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["message"] = "User not found.";
                return RedirectToAction("Index");
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                List<int> sectionIds = await dbContext.Sections
                    .Where(s => EF.Property<string>(s, "InstructorId") == id)
                    .Select(s => s.SectionId)
                    .ToListAsync();

                List<int> assignmentIds = await dbContext.Assignments
                    .Where(a => sectionIds.Contains(EF.Property<int>(a, "ClassSectionSectionId")))
                    .Select(a => a.AssignmentId)
                    .ToListAsync();

                List<int> submissionIdsFromInstructor = await dbContext.Submissions
                    .Where(s => assignmentIds.Contains(EF.Property<int>(s, "AssignmentId")))
                    .Select(s => s.SubmissionId)
                    .ToListAsync();

                List<int> submissionIdsFromStudent = await dbContext.Submissions
                    .Where(s => EF.Property<string>(s, "StudentId") == id)
                    .Select(s => s.SubmissionId)
                    .ToListAsync();

                List<int> submissionIds = submissionIdsFromInstructor
                    .Union(submissionIdsFromStudent)
                    .Distinct()
                    .ToList();

                var reviewsToDelete = await dbContext.Reviews
                    .Where(r => EF.Property<string>(r, "ReviewerId") == id
                                || submissionIds.Contains(EF.Property<int>(r, "SubmissionId")))
                    .ToListAsync();
                dbContext.Reviews.RemoveRange(reviewsToDelete);

                var assignmentVersionsToDelete = await dbContext.AssignmentVersions
                    .Where(v => EF.Property<int?>(v, "AssignmentId") != null
                                && assignmentIds.Contains(EF.Property<int?>(v, "AssignmentId")!.Value))
                    .ToListAsync();
                dbContext.AssignmentVersions.RemoveRange(assignmentVersionsToDelete);

                var submissionsToDelete = await dbContext.Submissions
                    .Where(s => submissionIds.Contains(s.SubmissionId))
                    .ToListAsync();
                dbContext.Submissions.RemoveRange(submissionsToDelete);

                var assignmentsToDelete = await dbContext.Assignments
                    .Where(a => assignmentIds.Contains(a.AssignmentId))
                    .ToListAsync();
                dbContext.Assignments.RemoveRange(assignmentsToDelete);

                var sectionsToDelete = await dbContext.Sections
                    .Where(s => sectionIds.Contains(s.SectionId))
                    .ToListAsync();
                dbContext.Sections.RemoveRange(sectionsToDelete);

                await dbContext.SaveChangesAsync();

                IdentityResult result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["message"] = string.Join(" | ", result.Errors.Select(e => e.Description));
                    return RedirectToAction("Index");
                }

                await transaction.CommitAsync();
                TempData["message"] = $"Deleted user '{user.UserName}' and related data.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["message"] = $"Delete failed: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        // the Add() methods work like the Register() methods from 16-11 and 16-12
        [HttpPost]
        public async Task<IActionResult> AddToAdmin(string id)
        {
            IdentityRole? adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                TempData["message"] = "Admin role does not exist. " + "Click 'Create Admin Role' button to create it.";
            }
            else
            {
                AppUser? user = await userManager.FindByIdAsync(id);
                if (user != null)
                    await userManager.AddToRoleAsync(user, adminRole.Name!);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromAdmin(string id)
        {
            AppUser? user = await userManager.FindByIdAsync(id);
            if (user != null)
                await userManager.RemoveFromRoleAsync(user, "Admin");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            IdentityRole? role = await roleManager.FindByIdAsync(id);
            if (role != null)
                await roleManager.DeleteAsync(role);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CreateAdminRole()
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            return RedirectToAction("Index");
        }
    }
}


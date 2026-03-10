# Implementing Authorization in the Code Reviews Application

This document describes all the additions required to implement role-based authorization (RBAC) in the Code Reviews project.

## Overview

Authorization in ASP.NET Core controls what authenticated users can **do**. This project uses:
- **ASP.NET Core Identity** for authentication (login/logout)
- **Role-based authorization** to restrict access based on user roles (Admin, Instructor, Student)

---

## 1. Database Setup

### 1.1 AppDbContext Configuration

The `DbContext` must inherit from `IdentityDbContext<AppUser>` instead of `DbContext` to support Identity tables:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CodeReviews.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
```

This automatically creates:
- `AspNetUsers` table
- `AspNetRoles` table
- `AspNetUserRoles` table (junction table)
- `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens` tables

### 1.2 AppUser Model

`AppUser` must inherit from `IdentityUser`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace CodeReviews.Models
{
    public class AppUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Section> InstructorSections { get; set; } = new List<Section>();
        public ICollection<Submission> StudentSubmissions { get; set; } = new List<Submission>();
        public ICollection<Review> ReviewsPerformed { get; set; } = new List<Review>();
    }
}
```

---

## 2. Program.cs Configuration

### 2.1 Add Roles to Identity Setup

```csharp
builder.Services.AddDefaultIdentity<AppUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // ← Add this line
    .AddEntityFrameworkStores<AppDbContext>();
```

### 2.2 Add Authentication and Authorization Middleware

```csharp
app.UseAuthentication();  // ← Add before UseAuthorization
app.UseAuthorization();   // Already present
```

### 2.3 Update SeedData Call to Include IConfiguration

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    SeedData.Seed(dbContext, scope.ServiceProvider, config);
}
```

---

## 3. SeedData Modifications

### 3.1 Update Seed Method Signature

```csharp
public static void Seed(AppDbContext context, IServiceProvider provider, IConfiguration config)
```

### 3.2 Seed Roles

```csharp
var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

string[] roleNames = { "Admin", "Instructor", "Student" };

foreach (var roleName in roleNames)
{
    if (!context.Roles.Any(r => r.Name == roleName))
    {
        var result = roleManager.CreateAsync(new IdentityRole(roleName)).Result;
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create role '{roleName}': " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
```

### 3.3 Seed Users with Roles and Email

**Get password from User Secrets** (never hardcode):

```sh
dotnet user-secrets set "SeedData:DefaultPassword" "SecurePassword123!"
```

**Seed users with role assignment:**

```csharp
var userManager = provider.GetRequiredService<UserManager<AppUser>>();
var seedPassword = config["SeedData:DefaultPassword"];

if (string.IsNullOrEmpty(seedPassword))
    throw new InvalidOperationException("SeedData:DefaultPassword not configured in User Secrets.");

var usersWithRoles = new List<(AppUser user, string role)>
{
    (new AppUser 
    { 
        Name = "Ada Lovelace", 
        UserName = "ada@example.com", 
        Email = "ada@example.com",
        EmailConfirmed = true 
    }, "Admin"),
    
    (new AppUser 
    { 
        Name = "Donald Knuth", 
        UserName = "donald@example.com", 
        Email = "donald@example.com",
        EmailConfirmed = true 
    }, "Instructor"),
    
    (new AppUser 
    { 
        Name = "Grace Hopper", 
        UserName = "grace@example.com", 
        Email = "grace@example.com",
        EmailConfirmed = true 
    }, "Student"),
};

foreach (var (user, role) in usersWithRoles)
{
    if (!context.Users.Any(u => u.UserName == user.UserName))
    {
        var createResult = userManager.CreateAsync(user, seedPassword).Result;
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create user '{user.UserName}': " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = userManager.AddToRoleAsync(user, role).Result;
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign role '{role}' to '{user.UserName}': " +
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}
```

---

## 4. Controller / Razor Pages Authorization

### 4.1 Protect Entire Controller

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]  // Requires any authenticated user
public class AdminController : Controller
{
    // All actions require authentication
}
```

### 4.2 Require Specific Role

```csharp
[Authorize(Roles = "Admin,Instructor")]  // Only Admin or Instructor
public IActionResult ManageAssignments()
{
    return View();
}
```

### 4.3 Allow Anonymous (Override)

```csharp
[AllowAnonymous]
public IActionResult Index()
{
    return View();
}
```

### 4.4 Razor Pages Authorization

Add to page model:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Instructor")]
public class CreateAssignmentModel : PageModel
{
    public void OnGet() { }
    
    public IActionResult OnPost() { }
}
```

Or in `@page` directive (if supported):

```html
@page
@attribute [Authorize(Roles = "Instructor")]
@model CreateAssignmentModel
```

---

## 5. Authorization Policy (Advanced)

For complex rules, create custom policies in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InstructorOrAdmin", policy =>
        policy.RequireRole("Instructor", "Admin"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));
});
```

Then use:

```csharp
[Authorize(Policy = "InstructorOrAdmin")]
public IActionResult EditCourse(int id) { }
```

---

## 6. Check Roles Programmatically

### 6.1 In Controller/Page Code

```csharp
if (User.IsInRole("Admin"))
{
    // Admin-only logic
}

if (User.IsInRole("Instructor") || User.IsInRole("Admin"))
{
    // Instructor or Admin logic
}
```

### 6.2 In Views/Razor Pages

```html
@if (User.IsInRole("Admin"))
{
    <a href="/admin/dashboard">Admin Dashboard</a>
}

@if (User.IsInRole("Instructor"))
{
    <button>Create Assignment</button>
}
```

---

## 7. Data-Level Authorization (Claims/Resource-Based)

For row-level security (e.g., "Instructors can only see their own sections"):

```csharp
[Authorize]
public IActionResult EditSection(int id)
{
    var section = _context.Sections.Find(id);
    
    if (section.InstructorId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
        && !User.IsInRole("Admin"))
    {
        return Forbid();  // 403 Forbidden
    }
    
    return View(section);
}
```

---

## 8. Testing Authorization

### 8.1 Anonymous Users

Unauthenticated requests to `[Authorize]` pages redirect to login:

```
GET /admin/dashboard → Redirect to /Identity/Account/Login
```

### 8.2 Insufficient Role

If a user lacks the required role:

```
GET /admin/users (requires Admin role)
User: Student → 403 Forbidden
```

### 8.3 Test with Different Roles

1. Create multiple seed users with different roles
2. Login as each role
3. Verify access control behavior

---

## 9. Common Authorization Patterns

### 9.1 Hide UI from Unauthorized Users

```html
@if (User.IsInRole("Admin"))
{
    <a href="/admin">Admin Panel</a>
}
```

### 9.2 Cascade Permissions

```csharp
// Student < Instructor < Admin (Admin has all permissions)
if (User.IsInRole("Admin") || User.IsInRole("Instructor"))
{
    // Allow instructor+ actions
}
```

### 9.3 Check Current User ID

```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var user = await _userManager.GetUserAsync(User);
```

---

## 10. Checklist: Authorization Implementation

- [ ] `AppDbContext` inherits from `IdentityDbContext<AppUser>`
- [ ] `AppUser` inherits from `IdentityUser`
- [ ] `.AddRoles<IdentityRole>()` in `Program.cs`
- [ ] `app.UseAuthentication()` before `app.UseAuthorization()`
- [ ] Roles seeded in `SeedData`
- [ ] Users created with `UserManager`
- [ ] Users assigned roles with `AddToRoleAsync`
- [ ] Seed password stored in User Secrets (not hardcoded)
- [ ] Controllers/Pages decorated with `[Authorize]` or `[Authorize(Roles = "...")]`
- [ ] Sensitive routes protected
- [ ] UI hides links/buttons from unauthorized users

---

## 11. Migrations

After updating models and `DbContext`, create a migration:

```sh
dotnet ef migrations add AddIdentity
dotnet ef database update
```

This creates all Identity tables and required columns.

---

## References

- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/)
- [Role-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)

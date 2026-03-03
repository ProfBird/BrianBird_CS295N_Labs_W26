# Adding ASP.NET Core Identity to an Existing Project

This document outlines the steps taken to add ASP.NET Core Identity to the CodeReviews project, which already had an existing `AppDbContext` and `AppUser` model.

## Table of Contents
1. [Initial Scaffolding](#initial-scaffolding)
2. [Refactoring Steps](#refactoring-steps)
3. [Common Issues and Solutions](#common-issues-and-solutions)
4. [Testing the Implementation](#testing-the-implementation)

---

## Initial Scaffolding

### Step 1: Scaffold Identity Pages

Using Visual Studio's scaffolding tool:

1. Right-click on the project in Solution Explorer
2. Select **Add** > **New Scaffolded Item**
3. Choose **Identity** from the left menu
4. Select **Add Identity**
5. In the scaffolding dialog:
   - Select the pages you want to override (or select all)
   - The scaffolder will create a new DbContext (e.g., `CodeReviewsContext`)
   - Click **Add**

This creates:
- `Areas/Identity/Pages/Account/` folder with Razor Pages
- `Areas/Identity/Data/CodeReviewsContext.cs` (a separate context)
- Identity configuration in `Program.cs`

### Step 2: Packages Added by Scaffolding

The scaffolder automatically adds these NuGet packages:
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.2" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.2" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="10.0.2" />
```

---

## Refactoring Steps

The scaffolded Identity system creates a separate context, but we want to use our existing `AppDbContext` and `AppUser` model. Here are the refactoring steps:

### Step 1: Modify `AppUser` to Inherit from `IdentityUser`

**Before:**
```csharp
public class AppUser
{
    public int AppUserId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }
}
```

**After:**
```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class AppUser : IdentityUser
    {
        // The person's full name
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
```

**Key Changes:**
- ? Inherit from `IdentityUser`
- ? Remove `AppUserId` property (IdentityUser already provides `Id`)
- ? Initialize `Name` property to `string.Empty` (fixes nullable warning)
- ? Add `using Microsoft.AspNetCore.Identity;`

### Step 2: Modify `AppDbContext` to Inherit from `IdentityDbContext<AppUser>`

**Before:**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Course> Courses { get; set; }
    // ... other DbSets
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // custom configurations
    }
}
```

**After:**
```csharp
using CodeReviews.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Remove or comment out AppUsers DbSet - Identity provides this
        // public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentVersion> AssignmentVersions { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Your custom configurations here
        }
    }
}
```

**Key Changes:**
- ? Inherit from `IdentityDbContext<AppUser>` instead of `DbContext`
- ? Add `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;`
- ? Keep existing DbSets for domain models
- ? Call `base.OnModelCreating(modelBuilder)` first in `OnModelCreating`

### Step 3: Update `Program.cs`

**Configure Identity to use your existing context:**

```csharp
#undef SQLITE  // To use SQLite, change #undef to #define. MySQL is the default.
using CodeReviews.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CodeReviews.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#if SQLITE
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
#else
var baseConnectionString = builder.Configuration.GetConnectionString("MySqlConnection");
var user = builder.Configuration["DbUser"];
var password = builder.Configuration["DbPassword"];
var connectionString = $"{baseConnectionString}userid={user};password={password};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));
#endif

// Configure Identity - applies to both SQLite and MySQL
builder.Services.AddDefaultIdentity<AppUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Add this - must be before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages(); // Add this - required for Identity Razor Pages

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Seed(dbContext);
}

app.Run();
```

**Key Changes:**
- ? Add `using Microsoft.AspNetCore.Identity;` and `using CodeReviews.Models;`
- ? Move Identity configuration outside the `#if/#else` block so it applies to both databases
- ? Use `AddDefaultIdentity<AppUser>()` instead of `IdentityUser`
- ? Call `.AddEntityFrameworkStores<AppDbContext>()` to use your existing context
- ? Add `app.UseAuthentication();` before `app.UseAuthorization();`
- ? Add `app.MapRazorPages();` for Identity Razor Pages routing

### Step 4: Delete the Scaffolded Context

Delete the file:
- `Areas/Identity/Data/CodeReviewsContext.cs`

### Step 5: Update All Identity Razor Page Code-Behind Files

All scaffolded Identity pages use `IdentityUser` by default. You need to replace all references with `AppUser`.

**Files that need updating** (22 files total):

#### Account Pages:
- `Register.cshtml.cs`
- `Login.cshtml.cs`
- `Logout.cshtml.cs`
- `ExternalLogin.cshtml.cs`
- `RegisterConfirmation.cshtml.cs`
- `ConfirmEmail.cshtml.cs`
- `ConfirmEmailChange.cshtml.cs`
- `ForgotPassword.cshtml.cs`
- `ResetPassword.cshtml.cs`
- `ResendEmailConfirmation.cshtml.cs`
- `LoginWith2fa.cshtml.cs`
- `LoginWithRecoveryCode.cshtml.cs`

#### Account/Manage Pages:
- `Index.cshtml.cs`
- `Email.cshtml.cs`
- `ChangePassword.cshtml.cs`
- `SetPassword.cshtml.cs`
- `PersonalData.cshtml.cs`
- `DeletePersonalData.cshtml.cs`
- `DownloadPersonalData.cshtml.cs`
- `ExternalLogins.cshtml.cs`
- `TwoFactorAuthentication.cshtml.cs`
- `EnableAuthenticator.cshtml.cs`
- `Disable2fa.cshtml.cs`
- `ResetAuthenticator.cshtml.cs`
- `GenerateRecoveryCodes.cshtml.cs`

**For each file, make these replacements:**

1. Add the using statement:
```csharp
using CodeReviews.Models;
```

2. Replace all type references:
```csharp
// Before:
private readonly SignInManager<IdentityUser> _signInManager;
private readonly UserManager<IdentityUser> _userManager;
private readonly IUserStore<IdentityUser> _userStore;
private readonly IUserEmailStore<IdentityUser> _emailStore;

// After:
private readonly SignInManager<AppUser> _signInManager;
private readonly UserManager<AppUser> _userManager;
private readonly IUserStore<AppUser> _userStore;
private readonly IUserEmailStore<AppUser> _emailStore;
```

3. Replace in constructor parameters:
```csharp
// Before:
public RegisterModel(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    SignInManager<IdentityUser> signInManager,
    // ...
)

// After:
public RegisterModel(
    UserManager<AppUser> userManager,
    IUserStore<AppUser> userStore,
    SignInManager<AppUser> signInManager,
    // ...
)
```

4. Replace in helper methods:
```csharp
// Before:
private IdentityUser CreateUser()
{
    return Activator.CreateInstance<IdentityUser>();
}

private async Task LoadAsync(IdentityUser user) { }

// After:
private AppUser CreateUser()
{
    return Activator.CreateInstance<AppUser>();
}

private async Task LoadAsync(AppUser user) { }
```

5. Replace in return types:
```csharp
// Before:
private IUserEmailStore<IdentityUser> GetEmailStore()
{
    return (IUserEmailStore<IdentityUser>)_userStore;
}

// After:
private IUserEmailStore<AppUser> GetEmailStore()
{
    return (IUserEmailStore<AppUser>)_userStore;
}
```

**Quick PowerShell Script to Update All Files:**

```powershell
$files = Get-ChildItem -Path "CodeReviews\Areas\Identity\Pages\Account" -Recurse -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Skip if already using AppUser
    if ($content -notmatch "IdentityUser") { continue }
    
    $newContent = $content `
        -replace 'UserManager<IdentityUser>', 'UserManager<AppUser>' `
        -replace 'SignInManager<IdentityUser>', 'SignInManager<AppUser>' `
        -replace 'IUserStore<IdentityUser>', 'IUserStore<AppUser>' `
        -replace 'IUserEmailStore<IdentityUser>', 'IUserEmailStore<AppUser>' `
        -replace 'Activator\.CreateInstance<IdentityUser>\(\)', 'Activator.CreateInstance<AppUser>()' `
        -replace "nameof\(IdentityUser\)", "nameof(AppUser)" `
        -replace 'Task LoadAsync\(IdentityUser user\)', 'Task LoadAsync(AppUser user)' `
        -replace 'Task LoadSharedKeyAndQrCodeUriAsync\(IdentityUser user\)', 'Task LoadSharedKeyAndQrCodeUriAsync(AppUser user)'
    
    # Add using statement if not present
    if ($newContent -notmatch "using CodeReviews.Models;") {
        $newContent = $newContent -replace '(using Microsoft\.AspNetCore\.Mvc\.RazorPages;)', "`$1`r`nusing CodeReviews.Models;"
    }
    
    Set-Content -Path $file.FullName -Value $newContent -NoNewline
}
```

### Step 6: Update Foreign Key References (If Applicable)

If you have models with foreign keys to `AppUser`, update them from `int` to `string`:

**Example - Before:**
```csharp
public class Submission
{
    public int SubmissionId { get; set; }
    public int SubmitterId { get; set; } // Foreign key to AppUser
    public AppUser Submitter { get; set; }
}
```

**Example - After:**
```csharp
public class Submission
{
    public int SubmissionId { get; set; }
    public string SubmitterId { get; set; } // Changed to string (IdentityUser.Id is string)
    public AppUser Submitter { get; set; }
}
```

### Step 7: Create New Migrations

Since you've significantly changed the database schema:

1. **Delete existing migrations folder:**
   - Delete the `Migrations` folder

2. **Create a new initial migration:**
   ```bash
   Add-Migration InitialWithIdentity
   ```

3. **Update the database:**
   ```bash
   Update-Database
   ```

This will create tables for:
- Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, etc.)
- Your domain model tables
- All properly related

---

## Common Issues and Solutions

### Issue 1: "Unable to resolve service for type 'UserManager<IdentityUser>'"

**Cause:** Identity pages are still using `IdentityUser` instead of `AppUser`.

**Solution:** Update all Identity Razor Page code-behind files as described in Step 5.

### Issue 2: Razor Pages Not Routing (Shows query string in URL)

**Symptom:** URL shows `?area=Identity&page=%2FAccount%2FRegister` instead of `/Identity/Account/Register`

**Cause:** Missing `app.MapRazorPages()` in `Program.cs`.

**Solution:** Add `app.MapRazorPages();` in the middleware pipeline in `Program.cs`.

### Issue 3: "Cannot convert from 'IdentityUser' to 'AppUser'"

**Cause:** Helper methods like `LoadAsync(IdentityUser user)` still use `IdentityUser`.

**Solution:** Update method signatures:
```csharp
// Before:
private async Task LoadAsync(IdentityUser user)

// After:
private async Task LoadAsync(AppUser user)
```

### Issue 4: Nullable Property Warning on `AppUser.Name`

**Cause:** C# nullable reference types are enabled, and `Name` isn't initialized.

**Solution:** Initialize the property:
```csharp
public string Name { get; set; } = string.Empty;
```

### Issue 5: Authentication Not Working

**Cause:** Missing `app.UseAuthentication()` or it's in the wrong order.

**Solution:** Ensure middleware is in correct order:
```csharp
app.UseRouting();
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();
```

---

## Testing the Implementation

### Test 1: Registration

1. Run the application
2. Navigate to `/Identity/Account/Register`
3. Register a new user
4. Verify the confirmation page appears

### Test 2: Login

1. Navigate to `/Identity/Account/Login`
2. Login with registered credentials
3. Verify successful login and redirection

### Test 3: Database Verification

Check the database to verify Identity tables were created:
- `AspNetUsers` - Contains user data including your custom `Name` field
- `AspNetRoles`
- `AspNetUserRoles`
- `AspNetUserClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`
- `AspNetRoleClaims`

### Test 4: User Manager Access

Verify you can access users from controllers:

```csharp
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    
    public HomeController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewBag.UserName = user.Name; // Custom property
        }
        return View();
    }
}
```

---

## Additional Customizations

### Adding Login Links to Layout

Create `Views/Shared/_LoginPartial.cshtml`:

```razor
@using Microsoft.AspNetCore.Identity
@using CodeReviews.Models
@inject SignInManager<AppUser> SignInManager
@inject UserManager<AppUser> UserManager

<ul class="navbar-nav">
@if (SignInManager.IsSignedIn(User))
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Manage/Index" title="Manage">
            Hello @User.Identity?.Name!
        </a>
    </li>
    <li class="nav-item">
        <form class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">
            <button type="submit" class="nav-link btn btn-link text-dark">Logout</button>
        </form>
    </li>
}
else
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Register">Register</a>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Login">Login</a>
    </li>
}
</ul>
```

Then include it in `_Layout.cshtml`:

```razor
<div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
    <ul class="navbar-nav flex-grow-1">
        <!-- Existing nav items -->
    </ul>
    <partial name="_LoginPartial" />
</div>
```

---

## Summary

Successfully integrating Identity into an existing project requires:

1. ? Scaffold Identity pages
2. ? Modify `AppUser` to inherit from `IdentityUser`
3. ? Modify `AppDbContext` to inherit from `IdentityDbContext<AppUser>`
4. ? Update `Program.cs` with proper Identity configuration and middleware
5. ? Delete the scaffolded context
6. ? Update all 20+ Identity Razor Page files to use `AppUser`
7. ? Update foreign key types if needed
8. ? Create new migrations and update the database
9. ? Test registration, login, and database integration

The key insight is that scaffolding creates a starting point, but integrating with existing models requires systematic refactoring of all generated files to use your custom user type instead of the default `IdentityUser`.

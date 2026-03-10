#define SQLITE  // To use SQLite, change #undef to #define. MySQL is the default.
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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    SeedData.Seed(dbContext, scope.ServiceProvider, config);
}

app.Run();

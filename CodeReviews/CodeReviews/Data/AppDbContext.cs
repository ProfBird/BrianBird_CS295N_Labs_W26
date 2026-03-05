using CodeReviews.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CodeReviews.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        // constructor just calls the base class constructor
        public AppDbContext(
           DbContextOptions<AppDbContext> options) : base(options) { }

        // one DbSet for each domain model class
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentVersion> AssignmentVersions { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

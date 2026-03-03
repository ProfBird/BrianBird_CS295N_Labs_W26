using CodeReviews.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CodeReviews.Data
{
    public class AppDbContext : DbContext
    {
        // constructor just calls the base class constructor
        public AppDbContext(
           DbContextOptions<AppDbContext> options) : base(options) { }

        // one DbSet for each domain model class
        public DbSet<AppUser> AppUsers { get; set; }
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

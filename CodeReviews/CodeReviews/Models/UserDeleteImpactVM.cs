namespace CodeReviews.Models
{
    public class UserDeleteImpactVM
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SectionsCount { get; set; }
        public int SubmissionsCount { get; set; }
        public int ReviewsCount { get; set; }

        public bool HasDependencies => SectionsCount > 0 || SubmissionsCount > 0 || ReviewsCount > 0;
    }
}

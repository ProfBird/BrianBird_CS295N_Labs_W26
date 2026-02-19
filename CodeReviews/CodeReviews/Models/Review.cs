namespace CodeReviews.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public Submission Submission { get; set; }
        public AppUser Reviewer { get; set; }
        public DateOnly ReviewDate { get; set; }
        public string Comments { get; set; }
        public string? ReviewUrl { get; set; }
    }
}

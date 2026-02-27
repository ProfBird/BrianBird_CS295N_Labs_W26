using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        
        // An object representing the code being reviewed
        [Required]
        public Submission Submission { get; set; }
        
        // The logged-in user
        [Required]
        public AppUser Reviewer { get; set; }
        
        // Today's date is automatically added in the controller
        public DateOnly ReviewDate { get; set; }
        
        // Review itself, or comments about the review at ReviewURL
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        // A link to the review (could be a PR)
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? ReviewUrl { get; set; }
    }
}

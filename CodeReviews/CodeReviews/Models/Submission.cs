using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        
        // An object containing instructions for the assignment
        [Required]
        public Assignment Assignment { get; set; }
        
        // Student whose assignment is being submitted
        [Required]
        public AppUser Student { get; set; }
        
        // Today's date is automatically added in the controller
        [Required]
        public DateTime SubmissionDate { get; set; }
        
        // Version of the assignment being submitted
        public string? Version { get; set; }
        
        // Address of the code. Could be a Git repo.
        [Required]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string CodeUrl { get; set; } // URL to the student's code submission
    }
}

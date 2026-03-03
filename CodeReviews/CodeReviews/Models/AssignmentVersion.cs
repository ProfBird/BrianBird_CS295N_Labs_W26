using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class AssignmentVersion
    {
        public int AssignmentVersionId { get; set; }
        
        // Version number, letter or name (could be the only version)
        [Required]
        [MaxLength(50)]
        public required string VersionName { get; set; }
        
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? InstructionsLink { get; set; }
        
        public string? Description { get; set; }
    }
}

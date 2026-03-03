using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        
        // Object describing the class section
        [Required]
        public Section ClassSection { get; set; }
        
        // A title for the assignment
        [Required]
        [MaxLength(100)]
        public required string AssignmentName { get; set; }  // Ex: Lab01-Variables
       
        // Due date for the draft or beta (if there is one)
        public DateTime DraftDueDate { get; set; }
        
        // Due date for the peer or code review
        [Required]
        public DateTime ReviewDueDate { get; set; }
        
        // Due date for the final or production version
        [Required]
        public DateTime FinalDueDate { get; set; }
        
        // Collection of assignment versions. Could only be one
        public List<AssignmentVersion>? Versions { get; set; } 
    }
}

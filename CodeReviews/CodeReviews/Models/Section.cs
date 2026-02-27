using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models;

public class Section  // Course Section
{
    public int SectionId { get; set; }
    
    // Object containing information about the course
    [Required]
    public Course Course { get; set; }
    
    // The CRN for this section of the course (class number)
    [Required]
    [Range(10000, 99999)]
    public int SectionNumber { get; set; }  // CRN
    
    // The days the class meets
    public string? Day { get; set; } // Ex: MW or TuTh
    
    public string? StartTime { get; set; } // Ex: 10:00 AM
    
    public string? Modality { get; set; } // Ex: Hybrid or Online
    
    // The term the class is offered
    [Required]
    public string Term { get; set; } // Ex: Fall
    
    // The year the class is offered
    [Required]
    public int Year { get; set; } // Ex: 2026
    
    // The instructor for this class
    [Required]
    public AppUser Instructor { get; set; }
}
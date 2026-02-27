using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models;

public class Course
{
    public int CourseId { get; set; }
    
    // Alphabetic prefix for course number
    [Required]
    public string CoursePrefix { get; set; }  // Ex: CS
    
    // Alphanumeric course number
    [Required]
    public string CourseNumber { get; set; }  // Ex: 161
    
    // Name of the course
    [Required]
    public string CourseName { get; set; }   // Ex: Beginning Python
}
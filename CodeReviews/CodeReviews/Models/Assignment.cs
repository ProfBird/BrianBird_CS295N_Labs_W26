namespace CodeReviews.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        public string Name { get; set; }
        public DateOnly DueDate { get; set; }
    }
}

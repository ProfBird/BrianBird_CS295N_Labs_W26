namespace CodeReviews.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        public Section SectionId { get; set; }
        public string Name { get; set; }  // Ex: Lab01-Variables
        public DateOnly BetaDueDate { get; set; }
        public DateOnly ReviewDueDate { get; set; }
        public DateOnly ProductionDueDate { get; set; }
        public List<string> Versions { get; set; }  // Ex: A, B, C
    }
}

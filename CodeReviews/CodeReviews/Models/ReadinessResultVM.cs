namespace CodeReviews.Models;

/// <summary>
/// Result of readiness evaluation
/// </summary>
public class ReadinessResultVM
{
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public ReadinessCheckVM? Checklist { get; set; }
}

namespace CodeReviews.Models;

/// <summary>
/// ViewModel for the Code Readiness Checker form
/// </summary>
public class ReadinessCheckVM
{
    public List<ReadinessCategory> Categories { get; set; } = new();
    public List<ReadinessItem> Items { get; set; } = new();
    public List<ReadinessSelection> Selections { get; set; } = new();
}

public class ReadinessCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ReadinessItem
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Category { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class ReadinessSelection
{
    public string Title { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
}

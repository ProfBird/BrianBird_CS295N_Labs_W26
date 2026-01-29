using Microsoft.AspNetCore.Mvc;
using CodeReviews.Models;
using System.Text.Json;

namespace CodeReviews.Controllers;

/// <summary>
/// Controller for the Code Readiness Checker
/// </summary>
public class ReadinessCheckController : Controller
{
    private readonly IWebHostEnvironment _environment;
    public ReadinessCheckController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // GET: ReadinessChecker
    public IActionResult Index()
    {
        return View(BuildViewModel());
    }

    // POST: ReadinessChecker
    [HttpPost]
    public IActionResult Index(ReadinessCheckVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildViewModel(model.Selections));
        }

        var checklist = BuildViewModel(model.Selections);
        var result = EvaluateReadiness(checklist);
        return View("Result", result);
    }

    /***** Helper Methods for checking code readiness *****/
    
    /// <summary>
    /// Evaluates the readiness checklist and returns a result with score and recommendations
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>ReadinessResultViewModel with score and status</returns>
    public ReadinessResultVM EvaluateReadiness(ReadinessCheckVM checklist)
    {
        var score = CalculateScore(checklist);
        var maxScore = GetMaxScore(checklist);
        var status = DetermineStatus(score, maxScore);
        var message = GenerateMessage(status);
        var recommendations = GenerateRecommendations(checklist);

        return new ReadinessResultVM
        {
            Score = score,
            MaxScore = maxScore,
            Status = status,
            Message = message,
            Recommendations = recommendations,
            Checklist = checklist
        };
    }

    /// <summary>
    /// Calculates the total score based on weighted criteria
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>Total score</returns>
    public int CalculateScore(ReadinessCheckVM checklist)
    {
        var selectionMap = BuildSelectionMap(checklist);

        return checklist.Items.Sum(item =>
            selectionMap.TryGetValue(item.Title, out var isChecked) && isChecked ? item.Points : 0);
    }

    /// <summary>
    /// Gets the maximum possible score
    /// </summary>
    /// <returns>Maximum score</returns>
    public int GetMaxScore(ReadinessCheckVM checklist)
    {
        return checklist.Items.Sum(item => item.Points);
    }

    /// <summary>
    /// Determines readiness status based on score
    /// </summary>
    /// <param name="score">Current score</param>
    /// <param name="maxScore">Maximum possible score</param>
    /// <returns>Status: "Ready", "Almost Ready", or "Not Ready"</returns>
    public string DetermineStatus(int score, int maxScore)
    {
        double percentage = (double)score / maxScore * 100;

        if (percentage >= 80) return "Ready";
        if (percentage >= 60) return "Almost Ready";
        return "Not Ready";
    }

    /// <summary>
    /// Generates a message based on readiness status
    /// </summary>
    /// <param name="status">The readiness status</param>
    /// <returns>User-friendly message</returns>
    public string GenerateMessage(string status)
    {
        return status switch
        {
            "Ready" => "Great! Your code is ready for review.",
            "Almost Ready" => "You're close! Address a few more items before requesting a review.",
            "Not Ready" => "Your code needs more work before it's ready for review.",
            _ => "Unable to determine readiness."
        };
    }

    /// <summary>
    /// Generates a list of recommendations based on unchecked items
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>List of recommendations</returns>
    public List<string> GenerateRecommendations(ReadinessCheckVM checklist)
    {
        var recommendations = new List<string>();
        var selectionMap = BuildSelectionMap(checklist);

        foreach (var item in checklist.Items)
        {
            var isChecked = selectionMap.TryGetValue(item.Title, out var selected) && selected;
            if (isChecked)
            {
                continue;
            }

            if (item.Title.Equals("TestsPass", StringComparison.OrdinalIgnoreCase))
            {
                var hasTests = selectionMap.TryGetValue("HasTests", out var testsSelected) && testsSelected;
                if (!hasTests)
                {
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Recommendation))
            {
                recommendations.Add(item.Recommendation);
            }
        }

        return recommendations;
    }

    private ReadinessCheckVM BuildViewModel(List<ReadinessSelection>? selections = null)
    {
        var categories = LoadCategories();
        var items = LoadItems();
        var selectionMap = selections?.ToDictionary(
                selection => selection.Title,
                selection => selection.IsChecked,
                StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        var orderedSelections = items.Select(item => new ReadinessSelection
        {
            Title = item.Title,
            IsChecked = selectionMap.TryGetValue(item.Title, out var isChecked) && isChecked
        }).ToList();

        return new ReadinessCheckVM
        {
            Categories = categories,
            Items = items,
            Selections = orderedSelections
        };
    }

    private List<ReadinessCategory> LoadCategories()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Data", "ReadinessCategories.json");
        var json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<ReadinessCategory>>(json, ReadinessJsonOptions.Options) ?? new List<ReadinessCategory>();
    }

    private List<ReadinessItem> LoadItems()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Data", "ReadinessChecklist.json");
        var json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<ReadinessItem>>(json, ReadinessJsonOptions.Options) ?? new List<ReadinessItem>();
    }

    private static Dictionary<string, bool> BuildSelectionMap(ReadinessCheckVM checklist)
    {
        var selectionMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var selection in checklist.Selections)
        {
            if (!string.IsNullOrWhiteSpace(selection.Title))
            {
                selectionMap[selection.Title] = selection.IsChecked;
            }
        }

        return selectionMap;
    }
}

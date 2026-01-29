using Microsoft.AspNetCore.Mvc;
using CodeReviews.Models;

namespace CodeReviews.Controllers;

/// <summary>
/// REFERENCE IMPLEMENTATION - Complete version of ReadinessCheckerController
/// This file shows one possible solution. Students should implement their own version.
/// </summary>
public class ReadinessCheckerController_SOLUTION : Controller
{
    // Action methods omitted - same as main controller

    /// <summary>
    /// Calculates the total score based on weighted criteria
    /// </summary>
    public int CalculateScore(ReadinessChecklistViewModel checklist)
    {
        int score = 0;

        // Critical criteria (10 points each = 30 points total)
        if (checklist.ProjectBuilds) score += 10;
        if (checklist.AppRuns) score += 10;
        if (checklist.NoRuntimeErrors) score += 10;

        // Important criteria (8 points each = 24 points total)
        if (checklist.CodeIsClean) score += 8;
        if (checklist.FollowsNamingConventions) score += 8;
        if (checklist.HasErrorHandling) score += 8;

        // Documentation (7 points each = 14 points total)
        if (checklist.HasReadme) score += 7;
        if (checklist.HasComments) score += 7;

        // Testing (8 points each = 16 points total)
        if (checklist.HasTests) score += 8;
        if (checklist.TestsPass) score += 8;

        // Repository (8 points each = 16 points total)
        if (checklist.CommitsHaveMessages) score += 8;
        if (checklist.NoSensitiveData) score += 8;

        return score;
    }

    /// <summary>
    /// Gets the maximum possible score
    /// </summary>
    public int GetMaxScore()
    {
        // 30 (critical) + 24 (important) + 14 (docs) + 16 (testing) + 16 (repo) = 100
        return 100;
    }

    /// <summary>
    /// Determines readiness status based on score
    /// Enhanced version that requires critical items
    /// </summary>
    public string DetermineStatus(int score, int maxScore, ReadinessChecklistViewModel? checklist = null)
    {
        // If critical items are missing, not ready regardless of score
        if (checklist != null)
        {
            if (!checklist.ProjectBuilds || !checklist.AppRuns || !checklist.NoRuntimeErrors)
            {
                return "Not Ready";
            }
        }

        double percentage = (double)score / maxScore * 100;

        if (percentage >= 80) return "Ready";
        if (percentage >= 60) return "Almost Ready";
        return "Not Ready";
    }

    /// <summary>
    /// Generates a message based on readiness status
    /// </summary>
    public string GenerateMessage(string status)
    {
        return status switch
        {
            "Ready" => "Great! Your code is ready for review. You can now request a peer review.",
            "Almost Ready" => "You're close! Address the recommendations below before requesting a review.",
            "Not Ready" => "Your code needs more work before it's ready for review. Focus on the critical items first.",
            _ => "Unable to determine readiness. Please try again."
        };
    }

    /// <summary>
    /// Generates a list of recommendations based on unchecked items
    /// </summary>
    public List<string> GenerateRecommendations(ReadinessChecklistViewModel checklist)
    {
        var recommendations = new List<string>();

        // Critical items first
        if (!checklist.ProjectBuilds)
        {
            recommendations.Add("CRITICAL: Fix all build errors. Your project must compile successfully.");
        }
        if (!checklist.AppRuns)
        {
            recommendations.Add("CRITICAL: Ensure your application runs without crashing on startup.");
        }
        if (!checklist.NoRuntimeErrors)
        {
            recommendations.Add("CRITICAL: Test all main pages and features to ensure they work without runtime errors.");
        }

        // Important items
        if (!checklist.CodeIsClean)
        {
            recommendations.Add("Remove debug statements, commented-out code, and unused variables.");
        }
        if (!checklist.FollowsNamingConventions)
        {
            recommendations.Add("Follow C# naming conventions: PascalCase for classes/methods, camelCase for local variables.");
        }
        if (!checklist.HasErrorHandling)
        {
            recommendations.Add("Add try-catch blocks and validation for user input and external operations.");
        }

        // Documentation
        if (!checklist.HasReadme)
        {
            recommendations.Add("Create a README.md with project description, features, and setup instructions.");
        }
        if (!checklist.HasComments)
        {
            recommendations.Add("Add comments to explain complex logic and algorithm choices.");
        }

        // Testing
        if (!checklist.HasTests)
        {
            recommendations.Add("Consider adding unit tests for business logic and controller methods.");
        }
        if (checklist.HasTests && !checklist.TestsPass)
        {
            recommendations.Add("Fix failing tests before requesting a review.");
        }

        // Repository
        if (!checklist.CommitsHaveMessages)
        {
            recommendations.Add("Write clear, descriptive commit messages that explain what and why.");
        }
        if (!checklist.NoSensitiveData)
        {
            recommendations.Add("CRITICAL: Remove passwords, API keys, and connection strings from your repository.");
        }

        return recommendations;
    }

    /// <summary>
    /// Alternative scoring approach with category weights
    /// </summary>
    public int CalculateScore_AlternativeApproach(ReadinessChecklistViewModel checklist)
    {
        // Define category weights
        var criticalWeight = 30;
        var importantWeight = 24;
        var documentationWeight = 14;
        var testingWeight = 16;
        var repositoryWeight = 16;

        // Calculate scores for each category
        int criticalScore = CalculateCategoryScore(
            new[] { checklist.ProjectBuilds, checklist.AppRuns, checklist.NoRuntimeErrors },
            criticalWeight
        );

        int importantScore = CalculateCategoryScore(
            new[] { checklist.CodeIsClean, checklist.FollowsNamingConventions, checklist.HasErrorHandling },
            importantWeight
        );

        int documentationScore = CalculateCategoryScore(
            new[] { checklist.HasReadme, checklist.HasComments },
            documentationWeight
        );

        int testingScore = CalculateCategoryScore(
            new[] { checklist.HasTests, checklist.TestsPass },
            testingWeight
        );

        int repositoryScore = CalculateCategoryScore(
            new[] { checklist.CommitsHaveMessages, checklist.NoSensitiveData },
            repositoryWeight
        );

        return criticalScore + importantScore + documentationScore + testingScore + repositoryScore;
    }

    /// <summary>
    /// Helper method to calculate score for a category
    /// </summary>
    private int CalculateCategoryScore(bool[] items, int categoryWeight)
    {
        int checkedCount = items.Count(item => item);
        int totalItems = items.Length;
        
        return (int)((double)checkedCount / totalItems * categoryWeight);
    }
}

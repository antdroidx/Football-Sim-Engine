using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Builds fictional in-game media from recorded stories. No stats, quotes or outcomes are invented.</summary>
public static class FootballNewsSocialService
{
    public static IReadOnlyList<FootballNewsStory> Build(IReadOnlyList<FootballNewsStory> stories)
    {
        ArgumentNullException.ThrowIfNull(stories);
        return stories.Where(story =>
            story.MediaKind is FootballNewsMediaKind.Story &&
            !string.IsNullOrWhiteSpace(story.Id))
            .Select(story => new FootballNewsStory
            {
                Id = "social/" + story.Id,
                Category = "In-game media",
                Headline = "Fans react to " + story.TeamName,
                Body = "In-game media reaction to " + story.TeamName + ": " + story.Headline,
                TeamName = story.TeamName,
                SeasonYear = story.SeasonYear,
                Week = story.Week,
                RelatedId = story.Id,
                MediaKind = FootballNewsMediaKind.Social
            })
            .ToArray();
    }
}

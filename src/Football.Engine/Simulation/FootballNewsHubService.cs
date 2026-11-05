using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class FootballNewsHubService
{
    public static IReadOnlyList<FootballNewsStory> Build(IReadOnlyList<FootballNewsStory> stories)
    {
        ArgumentNullException.ThrowIfNull(stories);
        return stories
            .Concat(FootballNewsSocialService.Build(stories))
            .DistinctBy(story => story.Id)
            .OrderByDescending(story => story.SeasonYear)
            .ThenByDescending(story => story.Week)
            .ThenBy(story => story.Id, StringComparer.Ordinal)
            .ToArray();
    }
}

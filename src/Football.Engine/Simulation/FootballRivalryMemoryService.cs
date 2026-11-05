using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Builds rivalry threads from the recorded head-to-head results. A thread states only what the schedule already
/// shows: who met, how often, and in which seasons.
/// </summary>
public static class FootballRivalryMemoryService
{
    public static IReadOnlyList<FootballNarrativeMemoryRecord> Records(
        IReadOnlyList<ScheduledGame> games,
        int seasonYear)
    {
        ArgumentNullException.ThrowIfNull(games);

        return RivalryService.Derive(games)
            .Select(rivalry =>
            {
                var latest = rivalry.Games.OrderByDescending(game => game.SeasonYear).ThenByDescending(game => game.Week).First();
                var seasons = rivalry.Games.Select(game => game.SeasonYear).Distinct().OrderBy(year => year).ToArray();
                return new FootballNarrativeMemoryRecord
                {
                    Id = "rivalry/" + StableId(rivalry.TeamAName, rivalry.TeamBName),
                    TeamName = rivalry.TeamAName,
                    Topic = "Rivalry vs " + rivalry.TeamBName,
                    Kind = FootballNarrativeKind.Rivalry,
                    SeasonYear = latest.SeasonYear,
                    FirstWeek = latest.Week,
                    LastWeek = latest.Week,
                    LatestHeadline = $"{rivalry.TeamAName} and {rivalry.TeamBName} have met {rivalry.Games.Count} times across {seasons.Length} season{(seasons.Length == 1 ? "" : "s")}",
                    SourceStoryIds = []
                };
            })
            .ToArray();
    }

    private static string StableId(string first, string second)
    {
        var teams = new[] { first, second }.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{teams[0]}|{teams[1]}"));
        return Convert.ToHexString(bytes.AsSpan(0, 8)).ToLowerInvariant();
    }
}
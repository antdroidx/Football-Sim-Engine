using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Reads a persisted rivalry set. Both games store the same records and label the same matchup the same way.
/// </summary>
public static class RivalryRecordService
{
    public static IReadOnlyList<RivalryRecord> Derive(IReadOnlyList<ScheduledGame> games) =>
        RivalryService.Derive(games);

    /// <summary>Labels a matchup when the two teams share a recorded series.</summary>
    public static string Label(IReadOnlyList<RivalryRecord> rivalries, string homeTeamName, string awayTeamName) =>
        rivalries.Any(rivalry => rivalry.Involves(homeTeamName, awayTeamName)) ? "Rivalry" : string.Empty;

    public static IReadOnlyList<RivalryRecord> For(IReadOnlyList<RivalryRecord> rivalries, string teamName) =>
        rivalries.Where(rivalry => rivalry.Involves(teamName)).ToArray();

    public static string Opponent(RivalryRecord rivalry, string teamName) =>
        rivalry.TeamAName.Equals(teamName, StringComparison.OrdinalIgnoreCase) ? rivalry.TeamBName : rivalry.TeamAName;

    /// <summary>The recorded series from one team's point of view.</summary>
    public static string Series(RivalryRecord rivalry, string teamName)
    {
        var played = rivalry.Games.Count;
        var wins = rivalry.Games.Count(game =>
            game.WinnerTeamName?.Equals(teamName, StringComparison.OrdinalIgnoreCase) == true);
        return $"{played} meeting{(played == 1 ? "" : "s")} · {wins}-{played - wins}";
    }
}
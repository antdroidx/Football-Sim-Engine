using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class RivalryService
{
    public static IReadOnlyList<RivalryRecord> Derive(IReadOnlyList<ScheduledGame> games)
    {
        return games
            .GroupBy(
                game => Key(game.HomeTeamName, game.AwayTeamName),
                StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var ordered = group.OrderBy(game => game.SeasonYear).ThenBy(game => game.Week).ToArray();
                return new RivalryRecord(
                    Key(ordered[0].HomeTeamName, ordered[0].AwayTeamName),
                    ordered[0].HomeTeamName,
                    ordered[0].AwayTeamName,
                    ordered.Select(ToGame).ToArray());
            })
            .Where(rivalry => rivalry.Games.Count > 1)
            .OrderBy(rivalry => rivalry.TeamAName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(rivalry => rivalry.TeamBName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool IsRivalry(IReadOnlyList<ScheduledGame> games, string homeTeamName, string awayTeamName)
    {
        return Derive(games).Any(rivalry =>
            rivalry.Involves(homeTeamName, awayTeamName));
    }

    public static string Label(IReadOnlyList<ScheduledGame> games, string homeTeamName, string awayTeamName) =>
        IsRivalry(games, homeTeamName, awayTeamName) ? "Rivalry" : string.Empty;

    private static RivalryGameRecord ToGame(ScheduledGame game) => new(
        game.SeasonYear,
        game.Week,
        game.HomeTeamName,
        game.AwayTeamName,
        game.WinnerTeamName,
        game.IsNeutralSite,
        game.BowlName);

    private static string Key(string homeTeamName, string awayTeamName)
    {
        var teams = new[] { homeTeamName, awayTeamName }
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return $"{teams[0]}|{teams[1]}";
    }
}

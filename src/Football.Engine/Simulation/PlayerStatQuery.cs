using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Null season selects a career; null team/player selects all identities.</summary>
public enum StatCompetition { RegularSeason, Postseason, ConferenceChampionship }
public sealed record PlayerStatGame(ScheduledGame Game, StatCompetition Competition);
public sealed record PlayerStatScope(int? SeasonYear = null, string? TeamName = null, Guid? PlayerId = null, StatCompetition? Competition = null);

public static class PlayerStatQuery
{
    public static IReadOnlyList<PlayerSeasonStatLine> FromCompetitiveGames(IEnumerable<PlayerStatGame> games, PlayerStatScope scope)
    {
        var lines = games.Where(item => item.Game.HasPlayed &&
                (!scope.Competition.HasValue || item.Competition == scope.Competition) &&
                (!scope.SeasonYear.HasValue || item.Game.SeasonYear == scope.SeasonYear))
            .DistinctBy(item => (item.Competition, item.Game.SeasonYear, item.Game.GameId))
            .SelectMany(item => item.Game.PlayerStats);
        return Filter(lines, scope with { SeasonYear = null, Competition = null });
    }

    public static IReadOnlyList<PlayerSeasonStatLine> Filter(IEnumerable<PlayerSeasonStatLine> lines, PlayerStatScope scope)
    {
        if (scope.Competition.HasValue) throw new ArgumentException("Competition filtering requires phase-tagged game records.", nameof(scope));
        return lines.Where(line => (!scope.SeasonYear.HasValue || line.SeasonYear == scope.SeasonYear) &&
            (scope.TeamName is null || line.TeamName == scope.TeamName) &&
            (!scope.PlayerId.HasValue || line.PlayerId == scope.PlayerId)).ToArray();
    }

    /// <summary>
    /// League adapters select competition phases before calling. Input order is
    /// precedence: current played results precede archive copies. The game's year
    /// is authoritative even when an older player line lacks year metadata.
    /// </summary>
    public static IReadOnlyList<PlayerSeasonStatLine> FromCompletedGames(IEnumerable<ScheduledGame> games, PlayerStatScope scope)
    {
        if (scope.Competition.HasValue) throw new ArgumentException("Supply phase-tagged games for competition filtering.", nameof(scope));
        var lines = games.Where(game => game.HasPlayed &&
                (!scope.SeasonYear.HasValue || game.SeasonYear == scope.SeasonYear))
            .DistinctBy(game => (game.SeasonYear, game.GameId))
            .SelectMany(game => game.PlayerStats);
        return Filter(lines, scope with { SeasonYear = null });
    }
}

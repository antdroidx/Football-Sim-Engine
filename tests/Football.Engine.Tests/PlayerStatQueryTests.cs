using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class PlayerStatQueryTests
{
    [Fact]
    public void CompetitionScopesDoNotCollapseSameNamedGamesOrPretendRowsHavePhaseHistory()
    {
        var id = Guid.NewGuid();
        ScheduledGame Game(int yards) => new() { GameId = "same", SeasonYear = 2026, HomeTeamName = "A", AwayTeamName = "B",
            HasPlayed = true, PlayerStats = [Line(id, 2026, "A", yards)] };
        PlayerStatGame[] games = [new(Game(10), StatCompetition.RegularSeason), new(Game(20), StatCompetition.Postseason),
            new(Game(900), StatCompetition.Postseason)];
        Assert.Equal(30, PlayerStatQuery.FromCompetitiveGames(games, new()).Sum(p => p.PassYards));
        Assert.Equal(20, Assert.Single(PlayerStatQuery.FromCompetitiveGames(games, new(Competition: StatCompetition.Postseason))).PassYards);
        Assert.Throws<ArgumentException>(() => PlayerStatQuery.Filter([Line(id, 2026, "A", 1)],new(Competition:StatCompetition.Postseason)));
    }
    static PlayerSeasonStatLine Line(Guid id, int year, string team, int yards) => new()
    { PlayerId = id, PlayerName = "Player", TeamName = team, SeasonYear = year, Games = 1,
        PassYards = yards, LongestPass = yards, TrackedGames = 0 };

    [Fact]
    public void ScopeKeepsTransferredTeamsAndYearsSeparateWithoutInventingTrackedHistory()
    {
        var id = Guid.NewGuid();
        PlayerSeasonStatLine[] lines = [Line(id, 2025, "A", 30), Line(id, 2026, "A", 20),
            Line(id, 2026, "B", 10), Line(Guid.NewGuid(), 2026, "B", 90)];
        var career = PlayerStatTotals.GroupByPlayer(PlayerStatQuery.Filter(lines, new(PlayerId: id)), "Multiple teams");
        var total = Assert.Single(career);
        Assert.Equal(60, total.PassYards); Assert.Equal(30, total.LongestPass);
        Assert.Equal(0, total.TrackedGames); Assert.Equal("Multiple teams", total.TeamName);
        Assert.Equal(10, Assert.Single(PlayerStatQuery.Filter(lines, new(2026, "B", id))).PassYards);
        Assert.Empty(PlayerStatQuery.Filter(lines, new(2024, PlayerId: id)));
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public void PlayedGameSelectionUsesSeasonAndIdWithFirstPlayedCopyTakingPrecedence()
    {
        var id = Guid.NewGuid();
        ScheduledGame Game(string key, int year, int yards, bool played = true) => new()
        { GameId = key, SeasonYear = year, HomeTeamName = "A", AwayTeamName = "B", HasPlayed = played,
            PlayerStats = [Line(id, 0, "A", yards)] };
        ScheduledGame[] games = [Game("same", 2026, 999, false), Game("same", 2026, 20),
            Game("same", 2026, 800), Game("same", 2025, 30), Game("unplayed", 2026, 700, false)];
        var season = Assert.Single(PlayerStatQuery.FromCompletedGames(games, new(2026, "A", id)));
        Assert.Equal(20, season.PassYards); // Older lines without a year remain readable.
        var career = PlayerStatQuery.FromCompletedGames(games, new(PlayerId: id));
        Assert.Equal(2, career.Count); Assert.Equal(50, career.Sum(line => line.PassYards));
        Assert.Empty(PlayerStatQuery.FromCompletedGames(games, new(TeamName: "B")));
    }
}

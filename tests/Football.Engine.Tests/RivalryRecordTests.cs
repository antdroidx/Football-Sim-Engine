using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class RivalryRecordTests
{
    [Fact]
    public void LabelNamesAMatchupWithARecordedSeries()
    {
        var rivalries = RivalryRecordService.Derive(Games());

        Assert.Equal("Rivalry", RivalryRecordService.Label(rivalries, "Washington", "Oregon"));
        Assert.Equal("Rivalry", RivalryRecordService.Label(rivalries, "Oregon", "Washington"));
        Assert.Equal(string.Empty, RivalryRecordService.Label(rivalries, "Washington", "Michigan"));
    }

    [Fact]
    public void SeriesCountsMeetingsAndWinsFromOneTeamsPointOfView()
    {
        var rivalry = Assert.Single(RivalryRecordService.Derive(Games()));

        Assert.Equal("3 meetings · 1-2", RivalryRecordService.Series(rivalry, "Washington"));
        Assert.Equal("3 meetings · 2-1", RivalryRecordService.Series(rivalry, "Oregon"));
    }

    [Fact]
    public void OpponentNamesTheOtherClub()
    {
        var rivalry = Assert.Single(RivalryRecordService.Derive(Games()));

        Assert.Equal("Oregon", RivalryRecordService.Opponent(rivalry, "Washington"));
        Assert.Equal("Washington", RivalryRecordService.Opponent(rivalry, "Oregon"));
    }

    [Fact]
    public void ForReturnsEverySeriesATeamBelongsTo()
    {
        var rivalries = RivalryRecordService.Derive(Games());

        var series = RivalryRecordService.For(rivalries, "Washington");
        Assert.Single(series);
        Assert.Empty(RivalryRecordService.For(rivalries, "Michigan"));
    }

    [Fact]
    public void TheSameGamesAlwaysProduceTheSameRivalrySet()
    {
        var first = RivalryRecordService.Derive(Games());
        var second = RivalryRecordService.Derive(Games());

        Assert.Equal(first[0].Id, second[0].Id);
        Assert.Equal(first[0].Games.Count, second[0].Games.Count);
        Assert.Equal(first[0].Games[0].SeasonYear, second[0].Games[0].SeasonYear);
    }

    static ScheduledGame[] Games() =>
    [
        Game("2025-1", "Washington", "Oregon", 2025, 1, homeWon: true),
        Game("2025-9", "Oregon", "Washington", 2025, 9, homeWon: true),
        Game("2026-3", "Washington", "Oregon", 2026, 3, homeWon: false),
        Game("2026-5", "Washington", "Michigan", 2026, 5, homeWon: true)
    ];

    static ScheduledGame Game(string gameId, string home, string away, int year, int week, bool homeWon) => new()
    {
        GameId = gameId, SeasonYear = year, Week = week,
        HomeTeamName = home, AwayTeamName = away, HasPlayed = true,
        HomeScore = homeWon ? 21 : 7, AwayScore = homeWon ? 7 : 21
    };
}
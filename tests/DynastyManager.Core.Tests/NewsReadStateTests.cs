using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class NewsReadStateTests
{
    [Fact]
    public void MarkingStoriesReadIsIdempotentAndFiltersUnread()
    {
        var game = new ScheduledGame { GameId = "game-1", AwayTeamName = "Away", HomeTeamName = "Home",
            SeasonYear = 2026, Week = 1, HasPlayed = true, AwayScore = 7, HomeScore = 10 };
        var state = State() with { Schedule = [game] };
        var stories = DynastyNewsService.Build(state, 2).ToArray();
        Assert.Single(stories);
        var marked = DynastyNewsService.MarkRead(state, [stories[0].Id, stories[0].Id, ""]);
        Assert.Equal([stories[0].Id], marked.ReadNewsIds);
        Assert.Empty(DynastyNewsService.Unread(marked, 2));
        Assert.Equal([stories[0].Id], marked.ReadNewsIds);
    }

    [Fact]
    public void CollegeNewsReadStateSurvivesSerialization()
    {
        var state = State() with { ReadNewsIds = ["game/123"] };
        var restored = JsonSerializer.Deserialize<DynastyState>(JsonSerializer.Serialize(state))!;
        Assert.Equal(["game/123"], restored.ReadNewsIds);
    }

    static DynastyState State() => new()
    {
        DynastyName = "News Test", UserTeamName = "Home", SeasonYear = 2026, Week = 1,
        Phase = SeasonPhase.RegularSeason
    };
}

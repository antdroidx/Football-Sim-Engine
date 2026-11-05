using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class TeamLeadershipServiceTests
{
    [Fact]
    public void CaptainsPreferQualitiesAndStayDistinct()
    {
        var roster = new[]
        {
            Player("Quarterback", "Alpha", Position.QB, 1),
            Player("Blocker", "Bravo", Position.OL, 5),
            Player("Breaker", "Charlie", Position.DE, 2),
            Player("Rookie", "Delta", Position.WR, 1)
        };
        var captains = TeamLeadershipService.Captains(roster, 2);
        Assert.Equal(2, captains.Count);
        Assert.Distinct(captains);
        Assert.Contains(roster[0].PlayerId, captains);
    }

    [Fact]
    public void EmptyRosterHasNoCaptains()
        => Assert.Empty(TeamLeadershipService.Captains([], 4));

    private static DynastyPlayer Player(string text, string name, Position position, int depth) =>
        new()
        {
            PlayerId = Guid.NewGuid(),
            FullName = $"{name} {text}",
            TeamName = "Test",
            Position = position,
            OverallRating = 75,
            AwarenessRating = 70,
            DepthChartOrder = depth,
            Skills = new PlayerSkillRatings { Leadership = 75, Discipline = 70 }
        };
}


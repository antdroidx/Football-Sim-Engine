using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class SharedCoreTests
{
    [Fact]
    public void SharedStaffLifecycleArithmeticHasStableBoundaries()
    {
        Assert.Equal(1, StaffLifecycleMath.Shift(1, 10, 20));
        Assert.Equal(-1, StaffLifecycleMath.Shift(95, 10, 20));
        Assert.Equal(0, StaffLifecycleMath.Shift(50, 10, 20));
        Assert.Equal(35, StaffLifecycleMath.Attribute(35, -1));
        Assert.Equal(96, StaffLifecycleMath.Attribute(96, 1));
        Assert.Equal(60, StaffLifecycleMath.Attribute(59, 1));
    }

    [Fact]
    public void SharedStatAggregationPreservesMultiTeamIdentityAndMaximums()
    {
        var id = Guid.NewGuid();
        var first = new PlayerSeasonStatLine
        {
            PlayerId = id, PlayerName = "Player", TeamName = "First", SeasonYear = 2026,
            Games = 3, Targets = 10, Receptions = 7, ReceivingYards = 80, LongestReception = 42
        };
        var second = first with
        {
            TeamName = "Second", SeasonYear = 2027, Games = 4, Targets = 12,
            Receptions = 9, ReceivingYards = 70, LongestReception = 35
        };
        var total = Assert.Single(PlayerStatTotals.GroupByPlayer([first, second], "Multiple teams"));
        Assert.Equal("Multiple teams", total.TeamName);
        Assert.Equal(7, total.Games);
        Assert.Equal(22, total.Targets);
        Assert.Equal(150, total.ReceivingYards);
        Assert.Equal(42, total.LongestReception);
    }
}

using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class KickoffPolicyTests
{
    [Theory]
    [InlineData(false, false, 35, 25)] [InlineData(false, true, 35, 25)]
    [InlineData(true, false, 35, 35)] [InlineData(true, true, 35, 20)]
    [InlineData(true, false, 20, 35)] [InlineData(true, true, 20, 20)]
    [InlineData(true, false, 50, 20)] [InlineData(true, true, 50, 20)]
    public void TouchbackSpotsFollowLeagueLandingAndKickOrigin(bool pro, bool landed, int from, int expected) =>
        Assert.Equal(expected, KickoffPolicy.TouchbackSpot(pro ? FootballRules.Professional : FootballRules.College, landed, from));

    [Theory]
    [InlineData(65)] [InlineData(80)] [InlineData(50)]
    public void DynamicFormationHasTenCoveragePlayersAtFortyAndLegalSetupBands(int origin)
    {
        Assert.All(Enumerable.Range(1, 10), slot => Assert.Equal(40, KickoffPolicy.AlignmentYard(false, false, slot, 5, origin, true)));
        var setup = Enumerable.Range(1, 10).Select(slot => KickoffPolicy.AlignmentYard(true, false, slot, 5, origin, true)).ToArray();
        Assert.All(setup, spot => Assert.InRange(spot, 30, 35));
        Assert.Equal(6, setup.Count(spot => spot == 35));
        Assert.Equal(origin + 5, KickoffPolicy.AlignmentYard(false, true, 0, 5, origin, true));
        Assert.Equal(5, KickoffPolicy.AlignmentYard(true, true, 0, 5, origin, true));
        Assert.Equal(0, KickoffPolicy.MovementProgress(.64, true));
        Assert.Equal(0, KickoffPolicy.MovementProgress(.65, true));
        Assert.Equal(1, KickoffPolicy.MovementProgress(1, true));
        Assert.Equal(.3, KickoffPolicy.MovementProgress(.3, false));
    }
}

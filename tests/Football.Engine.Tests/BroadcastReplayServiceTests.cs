using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class BroadcastReplayServiceTests
{
    [Fact]
    public void ReplayAnglesAreExplicitAndFallbackUsesBroadcast()
    {
        Assert.Equal(3, BroadcastReplayService.Angles.Count);
        Assert.Equal(ReplayAngle.Broadcast, BroadcastReplayService.AngleFor(null));
        Assert.Equal(ReplayAngle.FollowBall, BroadcastReplayService.AngleFor("followball"));
        Assert.Equal(ReplayAngle.Broadcast, BroadcastReplayService.AngleFor("invalid"));
    }
}

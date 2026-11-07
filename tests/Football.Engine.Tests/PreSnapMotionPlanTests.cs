using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class PreSnapMotionPlanTests
{
    [Fact]
    public void PlayAwareProfilesProduceDistinctSnapAlignmentsAndPaths()
    {
        var play = PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-spread-jet-zone");
        var jet = play.PreSnapAction!;
        var basePoint = GameDayAnimationPlanner.OffensiveAlignment(play, Position.WR, jet.MotionRoleIndex);
        var quarterback = GameDayAnimationPlanner.OffensiveAlignment(play, Position.QB, 0);
        var jetSnap = PreSnapMotionPlan.SnapAlignment(jet, true, false, basePoint, basePoint, quarterback);
        Assert.True(jetSnap.X < quarterback.X);
        Assert.NotEqual(basePoint, jetSnap);
        Assert.Equal(jetSnap, PreSnapMotionPlan.Path(jet, basePoint, jetSnap, quarterback)[^1]);
        Assert.True(jet.MotionManIsBallCarrier);
        Assert.True(jet.MotionLiveAtSnap);

        var set = PreSnapCatalog.Set("Right", Position.WR, 0, .32, -2);
        var setSnap = PreSnapMotionPlan.SnapAlignment(set, true, false, basePoint, basePoint, quarterback);
        Assert.Equal(-2, setSnap.X);
        Assert.Equal(.32, setSnap.Y);

        var orbit = PreSnapCatalog.Orbit("Right", Position.WR, 0);
        var orbitSnap = PreSnapMotionPlan.SnapAlignment(orbit, true, false, basePoint, basePoint, quarterback);
        Assert.Equal(-2.5, orbitSnap.X);
        Assert.Equal(.82, orbitSnap.Y);
        Assert.Equal(orbitSnap, PreSnapMotionPlan.Path(orbit, basePoint, orbitSnap, quarterback)[^1]);

        var swap = PreSnapCatalog.Swap("Right", Position.WR, 0, Position.WR, 1);
        var partnerPoint = GameDayAnimationPlanner.OffensiveAlignment(play, Position.WR, swap.SwapRoleIndex);
        Assert.Equal(partnerPoint, PreSnapMotionPlan.SnapAlignment(swap, true, false, basePoint, partnerPoint, quarterback));
        Assert.Equal(basePoint, PreSnapMotionPlan.SnapAlignment(swap, false, true, partnerPoint, basePoint, quarterback));
    }

    [Fact]
    public void JetSweepHandsOffToTheSelectedMotionReceiver()
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        var play = PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-spread-jet-zone");
        var snap = GameDayEngine.Step(session, play.Id, null, play.PreSnapAction);
        var players = snap.OffensePersonnel.Select(id => session.PlayersById[id]).ToArray();
        var motionReceiver = PreSnapMotionPlan.SelectPrimary(play.PreSnapAction!, players);

        Assert.NotNull(motionReceiver);
        Assert.Equal(motionReceiver!.PlayerId, snap.BallCarrierId);
        Assert.Equal(play.PreSnapAction, snap.PreSnap);
        Assert.Equal(PreSnapMotionKind.JetHandoff, snap.PreSnap!.MotionKind);
    }

    [Fact]
    public void ReturnMotionKeepsTheLegacyGenericBehavior()
    {
        var choice = PreSnapCatalog.Motion("Right", 2);
        Assert.Equal(PreSnapMotionKind.ReturnToFormation, choice.MotionKind);
        Assert.Null(choice.MotionPosition);
        Assert.False(choice.MotionManIsBallCarrier);
        Assert.False(choice.MotionLiveAtSnap);
    }
}
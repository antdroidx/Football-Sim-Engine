using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class JetSweepChoreographyTests
{
    static OffensivePlayDefinition JetPlay() =>
        PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-spread-jet-zone");

    static GameFieldPoint Base(OffensivePlayDefinition play, PreSnapChoice jet) =>
        GameDayAnimationPlanner.OffensiveAlignment(play, Position.WR, jet.MotionRoleIndex);

    static GameFieldPoint Quarterback(OffensivePlayDefinition play) =>
        GameDayAnimationPlanner.OffensiveAlignment(play, Position.QB, 0);

    [Fact]
    public void TheMotionManStartsInHisWideReceiverAlignment()
    {
        var play = JetPlay();
        var jet = play.PreSnapAction!;
        var basePoint = Base(play, jet);

        Assert.Equal(Position.WR, jet.MotionPosition);
        Assert.Equal(1, jet.MotionRoleIndex);
        Assert.True(basePoint.Y > .8, $"The motion man starts on the right side: {basePoint}");
        Assert.True(basePoint.X < 0, $"The motion man starts behind the line: {basePoint}");
        Assert.Equal(basePoint, PreSnapMotionPlan.Path(jet, basePoint, basePoint, Quarterback(play))[0]);
    }

    [Fact]
    public void TheHandoffHappensBesideTheQuarterbackRatherThanBehindHim()
    {
        var play = JetPlay();
        var jet = play.PreSnapAction!;
        var quarterback = Quarterback(play);
        var handoff = PreSnapMotionPlan.HandoffAlignment(jet, quarterback);

        Assert.Equal(quarterback.X, handoff.X);
        Assert.InRange(Math.Abs(handoff.Y - quarterback.Y), .01, PreSnapMotionPlan.HandoffLaneOffset + .01);
        Assert.InRange(handoff.Y, .08, .92);
    }

    [Fact]
    public void ThePreSnapPathOnlyMovesTowardTheQuarterback()
    {
        var play = JetPlay();
        var jet = play.PreSnapAction!;
        var basePoint = Base(play, jet);
        var quarterback = Quarterback(play);
        var handoff = PreSnapMotionPlan.HandoffAlignment(jet, quarterback);
        var path = PreSnapMotionPlan.Path(jet, basePoint, handoff, quarterback, stopBeforeHandoff: true);

        Assert.Equal(basePoint, path[0]);
        for (var index = 1; index < path.Count; index++)
            Assert.True(path[index].X < path[index - 1].X,
                $"The path reversed at segment {index}: {path[index - 1]} -> {path[index]}");
        Assert.True(path[^1].X > handoff.X, "The receiver is still closing on the quarterback at the snap");
        Assert.True(path[^1].X < basePoint.X, "The receiver left his alignment");
    }

    [Fact]
    public void TheSnapContinuesExactlyWhereThePreSnapMotionEnded()
    {
        var play = JetPlay();
        var jet = play.PreSnapAction!;
        var basePoint = Base(play, jet);
        var quarterback = Quarterback(play);
        var handoff = PreSnapMotionPlan.HandoffAlignment(jet, quarterback);
        var path = PreSnapMotionPlan.Path(jet, basePoint, handoff, quarterback, stopBeforeHandoff: true);
        var snapAlignment = PreSnapMotionPlan.SnapAlignment(jet, true, false, basePoint, basePoint, quarterback);

        // The live animation starts the receiver from the point the pre-snap motion left him.
        Assert.Equal(handoff, snapAlignment);
        Assert.Equal(path[^1], PreSnapMotionPlan.HandoffStartAlignment(jet, basePoint, snapAlignment));
    }

    [Fact]
    public void TheCarrierArrivesAtTheQuarterbackAndThenRuns()
    {
        var play = JetPlay();
        var jet = play.PreSnapAction!;
        var basePoint = Base(play, jet);
        var quarterback = Quarterback(play);
        var handoff = PreSnapMotionPlan.HandoffAlignment(jet, quarterback);
        var snapPoint = PreSnapMotionPlan.HandoffStartAlignment(jet, basePoint, handoff);
        var runEnd = new GameFieldPoint(snapPoint.X + 8, .22);
        var transfer = .18;

        var atSnap = GameDayAnimationPlanner.CarrierTrack(snapPoint, runEnd, 0, 75, false, handoff, transfer);
        Assert.Equal(snapPoint, atSnap);

        var atHandoff = GameDayAnimationPlanner.CarrierTrack(snapPoint, runEnd, transfer, 75, false, handoff, transfer);
        Assert.Equal(handoff.X, atHandoff.X, 3);
        Assert.Equal(handoff.Y, atHandoff.Y, 3);

        var afterHandoff = GameDayAnimationPlanner.CarrierTrack(snapPoint, runEnd, transfer + .2, 75, false, handoff, transfer);
        Assert.True(afterHandoff.X > handoff.X, "The receiver runs upfield after the handoff");
        Assert.True(afterHandoff.X < runEnd.X);
    }

    [Fact]
    public void TheBallChangesHandsAtTheHandoff()
    {
        var quarterbackId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var jet = JetPlay().PreSnapAction!;
        var snap = new GameSnapEvent
        {
            OffenseTeamName = "Home",
            DefenseTeamName = "Away",
            OffensivePlayId = "off-spread-jet-zone",
            DefensivePlayId = "def-base",
            Outcome = SnapOutcome.Run,
            PreSnap = jet,
            QuarterbackId = quarterbackId,
            BallCarrierId = receiverId
        };
        var transfer = GameDayAnimationPlanner.BallTransferProgress(snap);

        Assert.Equal(.18, transfer);
        Assert.Equal(quarterbackId, GameDayAnimationPlanner.BallOwner(snap, transfer - .01, .4));
        Assert.Equal(receiverId, GameDayAnimationPlanner.BallOwner(snap, transfer + .01, .4));
    }

    [Fact]
    public void TheJetSweepHandsTheBallToTheSelectedMotionReceiver()
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        var play = JetPlay();
        var snap = GameDayEngine.Step(session, play.Id, null, play.PreSnapAction);
        var players = snap.OffensePersonnel.Select(id => session.PlayersById[id]).ToArray();
        var motionReceiver = PreSnapMotionPlan.SelectPrimary(play.PreSnapAction!, players);

        Assert.NotNull(motionReceiver);
        Assert.Equal(motionReceiver!.PlayerId, snap.BallCarrierId);
        Assert.Equal(play.PreSnapAction, snap.PreSnap);
        Assert.Equal(PreSnapMotionKind.JetHandoff, snap.PreSnap!.MotionKind);
        Assert.True(snap.PreSnap.MotionLiveAtSnap);
    }
}

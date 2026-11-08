using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class ConversionPresentationTests
{
    static GameSnapEvent MadeConversion(string call, out GameDaySession session)
    {
        for (var seed = 7000; seed < 7400; seed++)
        {
            var candidate = GameDayFixture.Session(GameDayMode.Play, seed: seed);
            candidate.TryPending = true;
            candidate.HomeScore = 6;
            candidate.YardLine = 97;
            var snap = GameDayEngine.Step(candidate, call);
            if (snap.Outcome is SnapOutcome.TwoPointConversion or SnapOutcome.ExtraPoint)
            {
                session = candidate;
                return snap;
            }
        }
        throw new InvalidOperationException("No made try was produced.");
    }

    [Fact]
    public void AMadeTwoPointConversionIsAwardedAndCrossesTheGoalLine()
    {
        var snap = MadeConversion("special-two-point", out var session);

        Assert.Equal(SnapOutcome.TwoPointConversion, snap.Outcome);
        Assert.Equal(2, snap.Points);
        Assert.Equal(8, session.HomeScore);
        Assert.True(GameDayAnimationPlanner.CarrierDestinationYardLine(snap) > 100,
            "The carrier has to be carried past the goal line");
        Assert.Equal(102, GameDayAnimationPlanner.CarrierDestinationYardLine(snap));
        Assert.Equal(102, GameDayAnimationPlanner.CatchDestinationYardLine(snap));
    }

    [Fact]
    public void AMadeExtraPointIsAwarded()
    {
        var snap = MadeConversion("special-extra-point", out var session);

        Assert.Equal(SnapOutcome.ExtraPoint, snap.Outcome);
        Assert.Equal(1, snap.Points);
        Assert.Equal(7, session.HomeScore);
    }

    [Fact]
    public void AMissedTryScoresNothing()
    {
        for (var seed = 7000; seed < 7400; seed++)
        {
            var candidate = GameDayFixture.Session(GameDayMode.Play, seed: seed);
            candidate.TryPending = true;
            candidate.HomeScore = 6;
            candidate.YardLine = 97;
            var snap = GameDayEngine.Step(candidate, "special-two-point");
            if (snap.Outcome != SnapOutcome.FailedTwoPointConversion) continue;
            Assert.Equal(6, candidate.HomeScore);
            Assert.True(GameDayAnimationPlanner.CarrierDestinationYardLine(snap) <= 100,
                "A failed try is not carried past the goal line");
            return;
        }
        throw new InvalidOperationException("No failed try was produced.");
    }

    [Fact]
    public void PreSnapPenaltiesCarryTheAlignmentTheyWereCalledOn()
    {
        // The foul is announced over the formation, so the event has to carry both units.
        for (var seed = 7000; seed < 7100; seed++)
        {
            var session = GameDayFixture.Session(GameDayMode.Play, seed: seed);
            session.YardLine = 40;
            GameDayEngine.Step(session);
            var snap = GameDayEngine.Step(session);
            if (!snap.IsPreSnapPenalty) continue;
            Assert.Equal(SnapOutcome.Penalty, snap.Outcome);
            Assert.Equal(11, snap.OffensePersonnel.Count);
            Assert.Equal(11, snap.DefensePersonnel.Count);
            return;
        }
        throw new InvalidOperationException("No pre-snap penalty was produced.");
    }
}

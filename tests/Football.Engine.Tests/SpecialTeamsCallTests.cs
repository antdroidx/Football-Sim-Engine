using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public sealed class SpecialTeamsCallTests
{
    [Theory]
    [InlineData(30, SpecialTeamsCallService.PuntSafeId, SpecialTeamsCallService.PuntBlockId)]
    [InlineData(75, SpecialTeamsCallService.FieldGoalSafeId, SpecialTeamsCallService.FieldGoalBlockId)]
    public void DefensiveKickCallsMatchTheCpuKickDecision(int yardLine, string safeId, string blockId)
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        session.PossessionTeamName = session.Game.HomeTeamName;
        session.ControlledTeamName = session.Game.AwayTeamName;
        session.DelegateOffense = true;
        session.DelegateDefense = false;
        session.KickoffPending = false;
        session.Down = 4;
        session.Distance = 10;
        session.YardLine = yardLine;
        session.ClockStopped = true;

        var control = new GameDayLiveControl(session);
        Assert.True(control.CanCall(true));
        Assert.Equal([safeId, blockId], control.SpecialDefensiveCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockCallsRaiseBlockRiskAndSafeCallsLowerIt(bool fieldGoal)
    {
        var standard = SpecialTeamsCallService.BlockChance(SpecialTeamsDefensiveCall.None, fieldGoal);
        var safe = SpecialTeamsCallService.BlockChance(
            fieldGoal ? SpecialTeamsDefensiveCall.FieldGoalSafe : SpecialTeamsDefensiveCall.PuntSafe, fieldGoal);
        var block = SpecialTeamsCallService.BlockChance(
            fieldGoal ? SpecialTeamsDefensiveCall.FieldGoalBlock : SpecialTeamsDefensiveCall.PuntBlock, fieldGoal);
        Assert.True(safe < standard);
        Assert.True(standard < block);
    }

    [Fact]
    public void TwoPointTryRecordsTheSelectedPlayAndConversionAnimationData()
    {
        var session = GameDayFixture.Session(GameDayMode.Play, seed: 42);
        session.TryPending = true;
        session.PossessionTeamName = session.Game.HomeTeamName;
        session.ControlledTeamName = session.Game.HomeTeamName;

        var snap = GameDayEngine.Step(session, "off-spread-verts");
        Assert.Equal("off-spread-verts", snap.OffensivePlayId);
        Assert.True(snap.IsTry);
        Assert.True(GameDayAnimationPlanner.IsConversionPlay(snap));
        Assert.True(GameDayLivePresentation.InstantReplay(snap));
        Assert.NotNull(snap.QuarterbackId);
        Assert.Equal(11, snap.OffensePersonnel.Count);
        Assert.Equal(11, snap.DefensePersonnel.Count);
        Assert.NotNull(snap.TargetId);
        var visualSuccess = snap with { Outcome = SnapOutcome.TwoPointConversion };
        Assert.Equal(snap.TargetId, GameDayAnimationPlanner.BallOwner(visualSuccess, .8, visualSuccess.PassReleaseProgress));
        Assert.Contains(GameDayAnimationPlanner.OutcomeHeadline(snap), new[] { "TWO POINTS", "CONVERSION STOPPED" });
    }

    [Theory]
    [InlineData("special-punt", 30, SpecialTeamsCallService.PuntBlockId)]
    [InlineData("special-field-goal", 75, SpecialTeamsCallService.FieldGoalBlockId)]
    public void BlockCallCanRecordABlockedKickWithTheDefensiveCall(string special, int yardLine, string responseId)
    {
        for (var seed = 1; seed <= 400; seed++)
        {
            var session = GameDayFixture.Session(GameDayMode.Play, seed: seed);
            session.PossessionTeamName = session.Game.HomeTeamName;
            session.ControlledTeamName = session.Game.AwayTeamName;
            session.DelegateOffense = true;
            session.DelegateDefense = false;
            session.KickoffPending = false;
            session.Down = 4;
            session.Distance = 10;
            session.YardLine = yardLine;
            session.ClockStopped = true;

            var snap = GameDayEngine.Step(session, null, responseId);
            if (!snap.IsBlockedKick) continue;
            Assert.Equal(special == "special-punt" ? SnapOutcome.Punt : SnapOutcome.MissedFieldGoal, snap.Outcome);
            Assert.Equal(responseId, snap.DefensivePlayId);
            Assert.NotNull(snap.PrimaryDefenderId);
            Assert.Contains("blocks", snap.Description, StringComparison.OrdinalIgnoreCase);
            Assert.True(GameDayLivePresentation.ScoreHeadline(snap) == "KICK BLOCKED");
            return;
        }

        Assert.Fail("The block call did not produce a blocked kick in the deterministic seed range.");
    }
}
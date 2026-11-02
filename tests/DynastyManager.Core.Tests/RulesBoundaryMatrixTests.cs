using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

[Trait("Category", "Fast")]
public sealed class RulesBoundaryMatrixTests
{
    static GameDaySession Session(bool pro, int seed = 7123)
    {
        var source = GameDayEngineTests.Session(GameDayMode.Play, seed: seed);
        var session = GameDayEngine.Start(source.Game, source.HomeProfile, source.AwayProfile, source.Roster,
            GameDayMode.Play, rules: pro ? FootballRules.Professional : FootballRules.College);
        session.PossessionTeamName = "Home";
        session.ControlledTeamName = "Home";
        session.KickoffPending = false;
        return session;
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 2)] [InlineData(false, 3)] [InlineData(false, 4)]
    [InlineData(true, 1)] [InlineData(true, 2)] [InlineData(true, 3)] [InlineData(true, 4)]
    public void ExhaustedAndInvalidTimeoutsCannotCreateClockEventsOrDelayExpiry(bool pro, int quarter)
    {
        var s=Session(pro); s.Quarter=quarter;s.ClockSeconds=1;s.HomeTimeouts=s.AwayTimeouts=0;
        s.TwoMinuteTimeoutHalf=2;s.HomeScore=7;
        Assert.False(GameDayEngine.TryCallTimeout(s,"Home"));
        Assert.False(GameDayEngine.TryCallTimeout(s,"Away"));
        Assert.False(GameDayEngine.TryCallTimeout(s,"Unknown"));
        Assert.Empty(s.Events);
        var end=GameDayEngine.Step(s,"off-spread-zone");
        Assert.Equal(SnapOutcome.ClockStoppage,end.Outcome);
        Assert.Equal(quarter,end.Quarter);Assert.Equal(0,end.LiveBallSeconds);
        Assert.Equal(1,end.PreSnapSeconds);
        if(quarter==4) Assert.True(s.IsComplete); else Assert.Equal(quarter+1,s.Quarter);
        if(quarter==2) Assert.Equal(3,s.HomeTimeouts);
    }

    [Theory]
    [InlineData(false, "Home")] [InlineData(false, "Away")]
    [InlineData(true, "Home")] [InlineData(true, "Away")]
    public void TimeoutDuringReadyRestartUsesNoSnapRunoff(bool pro,string team)
    {
        var s=Session(pro);s.ClockSeconds=35;s.Quarter=2;s.TwoMinuteTimeoutHalf=1;
        s.ClockStopped=true;s.ClockRestartsOnReady=true;
        Assert.True(GameDayEngine.TryCallTimeout(s,team));
        var stopped=s.Events[^1];Assert.Equal(team,stopped.TimeoutTeamName);
        var snap=GameDayEngine.Step(s,"special-spike");
        Assert.Equal(0,snap.PreSnapSeconds);Assert.Equal(35,snap.ClockSeconds);
        Assert.False(s.ClockRestartsOnReady);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TwoPointPolicyCoversBothPossessionSidesAndLateGameBoundaries(bool pro)
    {
        foreach (var team in new[] { "Home", "Away" })
        foreach (var quarter in new[] { 2, 3, 4 })
        foreach (var seconds in new[] { 0, 1, 120, 300, 301 })
        foreach (var margin in Enumerable.Range(-16, 33))
        {
            var s = Session(pro);
            s.PossessionTeamName = team; s.Quarter = quarter; s.ClockSeconds = seconds;
            s.HomeScore = 20 + (team == "Home" ? margin : 0);
            s.AwayScore = 20 + (team == "Away" ? margin : 0);
            var expected = quarter == 4 && seconds <= 300 && margin is -8 or -5 or -2 or 1 or 5;
            Assert.Equal(expected, GameDayEngine.ShouldAttemptTwo(s));
            s.OvertimeRound = 2;
            Assert.Equal(!pro || expected, GameDayEngine.ShouldAttemptTwo(s));
        }
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 2)] [InlineData(false, 3)] [InlineData(false, 4)]
    [InlineData(true, 1)] [InlineData(true, 2)] [InlineData(true, 3)] [InlineData(true, 4)]
    public void TimeoutBeforePeriodExpiryPreservesTimeForTheSnap(bool pro, int quarter)
    {
        var s = Session(pro);
        s.Quarter = quarter; s.ClockSeconds = 1; s.TwoMinuteTimeoutHalf = 2;
        s.HomeScore = 7; s.AwayScore = 0;
        Assert.True(GameDayEngine.TryCallTimeout(s, "Home"));
        Assert.Equal(1, s.ClockSeconds);
        Assert.Equal(2, s.HomeTimeouts);
        Assert.False(GameDayEngine.TryCallTimeout(s, "Home"));
        var snap = GameDayEngine.Step(s, "special-spike");
        Assert.Equal(SnapOutcome.Spike, snap.Outcome);
        Assert.Equal(0, snap.PreSnapSeconds);
        Assert.Equal(quarter, snap.Quarter);
        if (quarter == 4) Assert.True(s.IsComplete);
        else Assert.Equal(quarter + 1, s.Quarter);
        if (quarter == 2) Assert.Equal(3, s.HomeTimeouts);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RequestedOnsideKickCoversBothRecoveryOutcomesWithoutReturnStatistics(bool pro)
    {
        var outcomes = new HashSet<bool>();
        for (var seed = 0; seed < 64; seed++)
        {
            var s = Session(pro, seed);
            s.KickoffPending = true; s.KickoffTeamName = "Away";
            s.ControlledTeamName = "Away"; s.OnsideKickRequested = true;
            var clock = s.ClockSeconds;
            var kick = GameDayEngine.Step(s);
            Assert.True(kick.IsOnsideKick);
            outcomes.Add(kick.KickingTeamRecovered);
            Assert.Equal(kick.KickingTeamRecovered ? "Away" : "Home", s.PossessionTeamName);
            Assert.Equal(kick.KickingTeamRecovered ? 45 : 55, s.YardLine);
            Assert.False(s.KickoffPending); Assert.False(s.OnsideKickRequested);
            Assert.Equal(1, s.Down); Assert.Equal(10, s.Distance);
            Assert.Equal(0, kick.ReturnYards); Assert.Null(kick.ReturnerId);
            Assert.Equal(clock, s.ClockSeconds);
            var box = GameDayEngine.BoxScore(s);
            Assert.Equal(0, box.HomeStats.KickReturns + box.AwayStats.KickReturns);
        }
        Assert.Equal(2, outcomes.Count);
    }

    [Theory]
    [InlineData(false, true, 3)] [InlineData(false, false, 97)]
    [InlineData(true, true, 3)] [InlineData(true, false, 97)]
    [InlineData(false, true, 50)] [InlineData(false, false, 50)]
    [InlineData(true, true, 50)] [InlineData(true, false, 50)]
    public void DeadBallFoulsEnforceHalfDistanceWithoutAPlayOrClockLoss(bool pro, bool offense, int spot)
    {
        for (var seed = 0; seed < 1000; seed++)
        {
            var s = Session(pro, seed);
            s.Quarter = 4; s.ClockSeconds = 60; s.TwoMinuteTimeoutHalf = 2;
            s.HomeScore = 7; s.ClockStopped = true;
            s.YardLine = spot; s.Down = 3; s.Distance = 7;
            s.Events.Add(new GameSnapEvent { OffenseTeamName = "Home", DefenseTeamName = "Away",
                OffensivePlayId = "", DefensivePlayId = "", Outcome = SnapOutcome.ClockStoppage });
            var foul = GameDayEngine.Step(s, "off-spread-zone");
            if (foul.Outcome != SnapOutcome.Penalty || (foul.PenalizedTeamName == "Home") != offense) continue;
            var yards = Math.Min(5, (offense ? spot : 100 - spot) / 2);
            Assert.Equal(yards, foul.PenaltyYards);
            Assert.Equal(spot + (offense ? -yards : yards), s.YardLine);
            Assert.Equal(3, s.Down); Assert.Equal(60, s.ClockSeconds);
            Assert.False(GameDayEngine.IsScrimmage(foul));
            var box = GameDayEngine.BoxScore(s);
            Assert.Equal(0, box.HomeStats.PassAttempts + box.HomeStats.RushAttempts);
            return;
        }
        Assert.Fail("No seeded dead-ball foul found.");
    }
    [Theory]
    [InlineData(false, true)] [InlineData(false, false)]
    [InlineData(true, true)] [InlineData(true, false)]
    public void RunningClockFoulRecordsElapsedTimeWithoutLiveBallOrAttempt(bool pro, bool offense)
    {
        for (var seed = 0; seed < 1000; seed++)
        {
            var s = Session(pro, seed);
            s.Quarter = 1; s.ClockSeconds = 300; s.ClockStopped = false;
            s.YardLine = 50; s.Down = 3; s.Distance = 3;
            s.Events.Add(new GameSnapEvent { OffenseTeamName = "Home", DefenseTeamName = "Away",
                OffensivePlayId = "", DefensivePlayId = "", Outcome = SnapOutcome.ClockStoppage });
            var foul = GameDayEngine.Step(s, "off-spread-zone");
            if (foul.Outcome != SnapOutcome.Penalty || (foul.PenalizedTeamName == "Home") != offense) continue;
            Assert.InRange(foul.PreSnapSeconds, 1, 40);
            Assert.Equal(300 - foul.PreSnapSeconds, s.ClockSeconds);
            Assert.Equal(s.ClockSeconds, foul.ClockSeconds);
            Assert.Equal(0, foul.LiveBallSeconds);
            Assert.True(s.ClockStopped); Assert.True(s.ClockRestartsOnReady);
            Assert.Equal(offense ? 3 : 1, s.Down);
            Assert.Equal(offense ? 8 : 10, s.Distance);
            var box = GameDayEngine.BoxScore(s);
            Assert.Equal(0, box.HomeStats.PassAttempts + box.HomeStats.RushAttempts);
            Assert.Equal(0, box.HomeStats.TotalYards);
            return;
        }
        Assert.Fail("No seeded running-clock foul found.");
    }

}

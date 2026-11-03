using Football.Testing;
using static Football.Testing.GameDayFixture;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Category", "Fast")]
[Trait("Owner", "Engine")]
public sealed class SafetyRulesTests
{
    static GameDaySession Session(bool pro, bool home = true, int seed = 7123, bool postseason = false)
    {
        var source = GameDayFixture.Session(GameDayMode.Play, seed: seed);
        var s = GameDayEngine.Start(source.Game, source.HomeProfile, source.AwayProfile, source.Roster,
            GameDayMode.Play, rules: (pro ? FootballRules.Professional : FootballRules.College) with { Postseason = postseason });
        s.KickoffPending = false; s.PossessionTeamName = home ? "Home" : "Away";
        s.ControlledTeamName = s.PossessionTeamName; s.YardLine = 1; s.ClockStopped = true;
        return s;
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void AutomaticKneelsAreCreditedToCpuRatherThanUser(bool pro)
    {
        var s=Session(pro);s.ControlledTeamName=null;s.YardLine=50;
        s.Quarter=4;s.ClockSeconds=40;s.TwoMinuteTimeoutHalf=2;s.HomeScore=7;s.AwayTimeouts=0;
        var snap=GameDayEngine.Step(s);
        Assert.Equal(SnapOutcome.Kneel,snap.Outcome);Assert.Equal(PlayCallSource.Cpu,snap.CallSource);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void OwnGoalLineLossAwardsDefenseTwoPointsAndQueuesItsReceivingPossession(bool pro, bool home)
    {
        var s = Session(pro, home);
        var snap = GameDayEngine.Step(s, "special-kneel");
        Assert.Equal(home ? 0 : 2, s.HomeScore);
        Assert.Equal(home ? 2 : 0, s.AwayScore);
        Assert.Equal(SnapOutcome.Kneel, snap.Outcome);
        Assert.Equal(2, snap.Points);
        Assert.False(s.TryPending); Assert.True(s.KickoffPending);
        Assert.Equal(home ? "Away" : "Home", s.PossessionTeamName);
        Assert.Equal(home ? "Home" : "Away", s.KickoffTeamName);
        var box = GameDayEngine.BoxScore(s);
        Assert.Equal(s.HomeScore, box.Periods.Sum(p => p.HomePoints));
        Assert.Equal(s.AwayScore, box.Periods.Sum(p => p.AwayPoints));
        Assert.All(box.Players, p => Assert.Equal(0, p.Touchdowns));
        Assert.True(snap.IsSafety); Assert.Equal(snap.DefenseTeamName, snap.ScoringTeamName);
        Assert.Equal(snap.DefenseTeamName, LiveFootballChoreography.NotificationTeam(snap));
        Assert.Contains("SAFETY", GameDayCommentaryService.Describe(snap, s.PlayersById));
        s.IsComplete = true;
        var result = GameDayEngine.Finish(s);
        Assert.Equal(DriveOutcome.Safety, Assert.Single(result.Drives).Outcome);
        Assert.Equal(0, result.Drives[0].Points);
        Assert.Contains(result.Highlights, e => e.IsSafety);
        var loaded = System.Text.Json.JsonSerializer.Deserialize<ScheduledGame>(
            System.Text.Json.JsonSerializer.Serialize(result))!;
        Assert.Equal(DriveOutcome.Safety, loaded.Drives[0].Outcome);
        Assert.True(Assert.Single(loaded.Highlights).IsSafety);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SafetyKickStartsAtTwentyAndResetsItsStateForTheNextKick(bool pro)
    {
        var outcomes = new HashSet<bool>();
        for (var seed = 0; seed < 64; seed++)
        {
            var s = Session(pro, seed: seed);
            GameDayEngine.Step(s, "special-kneel");
            Assert.True(s.SafetyKickPending);
            var clock = s.ClockSeconds;
            var kick = GameDayEngine.Step(s);
            outcomes.Add(kick.IsTouchback);
            Assert.True(kick.IsSafetyKick); Assert.Equal(80, kick.StartYardLine);
            Assert.Equal("Away", kick.OffenseTeamName); Assert.Equal("Home", kick.DefenseTeamName);
            Assert.Equal(11, kick.OffensePersonnel.Distinct().Count());
            Assert.Equal(11, kick.DefensePersonnel.Distinct().Count());
            Assert.False(s.SafetyKickPending); Assert.False(s.KickoffPending);
            if (kick.IsTouchback)
            {
                Assert.Equal(KickoffPolicy.TouchbackSpot(s.Rules, kick.KickTouchedLandingZone, 20), s.YardLine);
                Assert.Equal(clock, s.ClockSeconds); Assert.Equal(0, kick.ReturnYards);
            }
            else Assert.True(s.ClockSeconds < clock);
            Assert.Equal(kick.IsTouchback ? 0 : 1, GameDayEngine.BoxScore(s).AwayStats.KickReturns);
            s.KickoffPending = true; s.TryPending = false;
            Assert.Equal(65, GameDayEngine.Step(s).StartYardLine);
        }
        Assert.Equal(2, outcomes.Count);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SafetyOnsideKickUsesItsOwnRestrainingLineForEitherRecovery(bool pro)
    {
        var outcomes = new HashSet<bool>();
        for (var seed = 0; seed < 64; seed++)
        {
            var s = Session(pro, seed: seed);
            GameDayEngine.Step(s, "special-kneel");
            s.OnsideKickRequested = true;
            var kick = GameDayEngine.Step(s);
            Assert.True(kick.IsSafetyKick); Assert.True(kick.IsOnsideKick);
            outcomes.Add(kick.KickingTeamRecovered);
            Assert.Equal(80, kick.StartYardLine); Assert.Equal(70, kick.KickLandingYardLine);
            Assert.Equal(kick.KickingTeamRecovered ? 30 : 70, s.YardLine);
            Assert.Equal(kick.KickingTeamRecovered ? "Home" : "Away", s.PossessionTeamName);
            Assert.False(s.SafetyKickPending); Assert.False(s.OnsideKickRequested);
            Assert.Equal(11, kick.OffensePersonnel.Distinct().Count());
            Assert.Equal(11, kick.DefensePersonnel.Distinct().Count());
        }
        Assert.Equal(2, outcomes.Count);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 2)] [InlineData(false, 4)]
    [InlineData(true, 1)] [InlineData(true, 2)] [InlineData(true, 4)]
    public void SafetyAtPeriodExpiryKeepsOnlyTheAppropriateKick(bool pro, int quarter)
    {
        var s = Session(pro); s.Quarter = quarter; s.ClockSeconds = 1; s.TwoMinuteTimeoutHalf = 2;
        GameDayEngine.Step(s, "special-kneel");
        Assert.Equal(2, s.AwayScore);
        Assert.Equal(2, GameDayEngine.BoxScore(s).Periods.Single(p => p.Period == quarter).AwayPoints);
        if (quarter == 4) { Assert.True(s.IsComplete); Assert.False(s.KickoffPending); Assert.False(s.SafetyKickPending); }
        else
        {
            Assert.Equal(quarter + 1, s.Quarter); Assert.True(s.KickoffPending);
            Assert.Equal(quarter == 1, s.SafetyKickPending);
            Assert.Equal(quarter == 1 ? 80 : 65, GameDayEngine.Step(s).StartYardLine);
        }
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public void OvertimeSafetyEndsTheGameWithoutATryOrFreeKick(bool pro, bool postseason)
    {
        for (var seed = 0; seed < 500; seed++)
        {
            var s = Session(pro, seed: seed, postseason: postseason); s.OvertimeRound = 1; s.Quarter = 5; s.ClockSeconds = 1;
            var snap = GameDayEngine.Step(s, "off-spread-verts");
            if (!snap.IsSafety) continue;
            Assert.Equal(2, s.AwayScore); Assert.True(s.IsComplete);
            Assert.False(s.KickoffPending); Assert.False(s.SafetyKickPending); Assert.False(s.TryPending);
            Assert.Equal(5, s.Quarter);
            return;
        }
        Assert.Fail("No seeded overtime safety found.");
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void RunAndSackSafetiesKeepLeagueStatAccountingWithoutTouchdownCredit(bool pro, bool sack)
    {
        for (var seed = 0; seed < 500; seed++)
        {
            var s = Session(pro, seed: seed); var spot = sack ? 3 : 1; s.YardLine = spot;
            var snap = GameDayEngine.Step(s, sack ? "off-spread-verts" : "off-spread-zone");
            if (!snap.IsSafety || snap.Outcome != (sack ? SnapOutcome.Sack : SnapOutcome.Run)) continue;
            Assert.Equal(-spot, snap.Yards); Assert.Equal(0, snap.BallEndYardLine);
            var box = GameDayEngine.BoxScore(s);
            Assert.Equal(sack && pro ? 0 : -spot, box.HomeStats.RushYards);
            Assert.Equal(sack && pro ? 0 : 1, box.HomeStats.RushAttempts);
            Assert.Equal(sack && pro ? 3 : 0, box.HomeStats.SackYardsLost);
            Assert.All(box.Players, p => { Assert.Equal(0, p.Touchdowns); Assert.Equal(0, p.DefensiveTouchdowns); });
            if (sack) Assert.Equal(1, Assert.Single(box.Players, p => p.PlayerId == snap.PrimaryDefenderId).Sacks);
            return;
        }
        Assert.Fail("No seeded run/sack safety found.");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CpuDoesNotAutomaticallyKneelForASafetyWhenProtectingALead(bool pro)
    {
        var s = Session(pro); s.ControlledTeamName = null;
        s.Quarter = 4; s.ClockSeconds = 80; s.TwoMinuteTimeoutHalf = 2;
        s.HomeScore = 1; s.AwayTimeouts = 0;
        Assert.NotEqual(SnapOutcome.Kneel, GameDayEngine.Step(s).Outcome);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SafetyThatTiesAtRegulationExpiryStartsLeagueOvertimeWithNoStaleSafetyKick(bool pro)
    {
        var s = Session(pro); s.HomeScore = 2;
        s.Quarter = 4; s.ClockSeconds = 1; s.TwoMinuteTimeoutHalf = 2;
        GameDayEngine.Step(s, "special-kneel");
        Assert.Equal(2, s.HomeScore); Assert.Equal(2, s.AwayScore);
        Assert.False(s.IsComplete); Assert.Equal(5, s.Quarter); Assert.Equal(1, s.OvertimeRound);
        Assert.False(s.SafetyKickPending); Assert.Equal(pro, s.KickoffPending);
    }
}

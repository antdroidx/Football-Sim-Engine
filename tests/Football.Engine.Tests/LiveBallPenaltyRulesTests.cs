using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;
namespace Football.Engine.Tests;

[Trait("Owner","Engine"), Trait("Category","Fast")]
public sealed class LiveBallPenaltyRulesTests
{
    static GameDaySession Session(bool pro, int quarter=1, int seconds=300)
    {
        var source=GameDayFixture.Session(GameDayMode.Play);
        var s=GameDayEngine.Start(source.Game,source.HomeProfile,source.AwayProfile,source.Roster,
            GameDayMode.Play,rules:pro?FootballRules.Professional:FootballRules.College);
        s.KickoffPending=false;s.PossessionTeamName="Home";s.ControlledTeamName="Home";
        s.YardLine=50;s.Down=2;s.Distance=10;s.Quarter=quarter;s.ClockSeconds=seconds;
        s.TwoMinuteTimeoutHalf=quarter==2?1:quarter==4?2:0;s.AwayScore=7;
        return s;
    }
    static GameSnapEvent Play(GameDaySession s, SnapOutcome outcome=SnapOutcome.Incompletion, int yards=0) => new() {
        StatsVersion=1,Sequence=s.Events.Count+1,Quarter=s.Quarter,ClockSeconds=s.ClockSeconds,
        OffenseTeamName="Home",DefenseTeamName="Away",OffensivePlayId="off-spread-verts",DefensivePlayId="",
        StartYardLine=s.YardLine,BallEndYardLine=s.YardLine+yards,Down=s.Down,Distance=s.Distance,
        Yards=yards,Outcome=outcome,IsDropback=true,LiveBallSeconds=4,
        QuarterbackId=s.HomeRoster.First(p=>p.Position==Position.QB).PlayerId,
        TargetId=s.HomeRoster.First(p=>p.Position==Position.WR).PlayerId };

    [Theory]
    [InlineData(false,65)] [InlineData(true,90)]
    public void PassInterferenceUsesLeagueDistanceAndAutomaticFirstDown(bool pro,int spot)
    {
        var s=Session(pro);var blocker=s.HomeRoster.First(p=>p.Position==Position.OL).PlayerId;
        var rusher=s.AwayRoster.First(p=>p.Position==Position.DE).PlayerId;
        var play=Play(s) with {Blocks=[new(blocker,rusher,true,false,1)],PressureRushers=[rusher]};
        var e=GameDayEngine.ApplyScrimmageResult(s,play,[new(FootballFoulKind.DefensivePassInterference,"Away",90)]);
        Assert.Equal(SnapOutcome.Penalty,e.Outcome);Assert.True(e.IsLiveBallPenalty);
        Assert.Equal(spot,s.YardLine);Assert.Equal(1,s.Down);Assert.Equal(10,s.Distance);
        var box=GameDayEngine.BoxScore(s);Assert.Equal(0,box.HomeStats.PassAttempts);
        Assert.All(box.Players,p=>Assert.Equal(0,p.Targets+p.Dropbacks+p.PassBlockSnaps+p.DefensivePressures));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void HoldingNullifiesTouchdownAndItsStatistics(bool pro)
    {
        var s=Session(pro);var e=GameDayEngine.ApplyScrimmageResult(s,Play(s,SnapOutcome.Touchdown,50),
            [new(FootballFoulKind.OffensiveHolding,"Home",47)]);
        Assert.Equal(SnapOutcome.Penalty,e.Outcome);Assert.Equal(40,s.YardLine);Assert.Equal(2,s.Down);
        Assert.Equal(0,s.HomeScore);Assert.False(s.TryPending);Assert.Equal(0,GameDayEngine.BoxScore(s).HomeStats.TotalYards);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ScoringTeamDeclinesPassInterferenceAndKeepsTouchdown(bool pro)
    {
        var s=Session(pro);var e=GameDayEngine.ApplyScrimmageResult(s,Play(s,SnapOutcome.Touchdown,50),
            [new(FootballFoulKind.DefensivePassInterference,"Away",90)]);
        Assert.Equal(6,s.HomeScore);Assert.True(s.TryPending);Assert.Equal(SnapOutcome.Touchdown,e.Outcome);
        Assert.NotNull(e.DeclinedPenaltyName);Assert.Contains("declined",GameDayCommentaryService.Describe(e,s.PlayersById));
    }
    [Theory]
    [InlineData(false,1,false)] [InlineData(false,2,true)] [InlineData(false,3,false)] [InlineData(false,4,true)]
    [InlineData(true,1,true)] [InlineData(true,2,true)] [InlineData(true,3,true)] [InlineData(true,4,true)]
    public void AcceptedDefensiveFoulExtendsTheCorrectPeriodsAndOneRealDownConsumesIt(bool pro,int quarter,bool extended)
    {
        var s=Session(pro,quarter,1);
        GameDayEngine.ApplyScrimmageResult(s,Play(s),[new(FootballFoulKind.DefensivePassInterference,"Away",60)]);
        Assert.Equal(extended,s.UntimedDownPending);
        if(!extended){Assert.Equal(quarter+1,s.Quarter);return;}
        Assert.Equal(quarter,s.Quarter);Assert.Equal(0,s.ClockSeconds);
        var final=GameDayEngine.Step(s,"special-kneel");
        Assert.True(final.IsUntimedDown);Assert.False(s.UntimedDownPending);
        if(quarter==4)Assert.True(s.IsComplete);else Assert.Equal(quarter+1,s.Quarter);
    }
    [Theory]
    [InlineData(false,true)] [InlineData(true,false)]
    public void AcceptedOffensiveHoldingAtHalfEndUsesLeagueExtensionPolicy(bool pro,bool extended)
    {
        var s=Session(pro,2,1);GameDayEngine.ApplyScrimmageResult(s,Play(s,SnapOutcome.Completion,5),
            [new(FootballFoulKind.OffensiveHolding,"Home",47)]);
        Assert.Equal(extended,s.UntimedDownPending);Assert.Equal(extended?2:3,s.Quarter);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OffsettingFoulsReplayTheDownWithoutInventingStats(bool pro)
    {
        var s=Session(pro,2,1);var e=GameDayEngine.ApplyScrimmageResult(s,Play(s,SnapOutcome.Touchdown,50),
            [new(FootballFoulKind.OffensiveHolding,"Home",47),new(FootballFoulKind.DefensivePassInterference,"Away",70)]);
        Assert.True(e.IsOffsettingPenalty);Assert.Equal(50,s.YardLine);Assert.Equal(2,s.Down);
        Assert.Equal(0,s.HomeScore);Assert.True(s.UntimedDownPending);Assert.Equal(0,GameDayEngine.BoxScore(s).HomeStats.TotalYards);
    }
    [Theory]
    [InlineData(false,false)] [InlineData(false,true)] [InlineData(true,false)] [InlineData(true,true)]
    public void GroundingRunoffCanEndAHalfOrBeAvoidedWithOneTimeout(bool pro,bool timeout)
    {
        var s=Session(pro,4,8);s.HomeTimeouts=timeout?1:0;
        var e=GameDayEngine.ApplyScrimmageResult(s,Play(s),[new(FootballFoulKind.IntentionalGrounding,"Home",45)],
            acceptRunoff:true,useTimeout:timeout);
        Assert.Equal(3,s.Down);Assert.False(s.UntimedDownPending);
        Assert.Equal(!timeout,s.IsComplete);Assert.Equal(timeout?0:4,e.ClockRunoffSeconds);
        if(timeout){Assert.Equal(4,s.ClockSeconds);Assert.Equal(0,s.HomeTimeouts);Assert.Equal("Home",e.RunoffAvoidedByTimeoutTeam);}
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void AcceptedEndZoneHoldingIsASafetyWithNoPassingCredit(bool pro)
    {
        var s=Session(pro);s.YardLine=2;
        var e=GameDayEngine.ApplyScrimmageResult(s,Play(s),[new(FootballFoulKind.OffensiveHolding,"Home",-1)]);
        Assert.True(e.IsSafety);Assert.Equal(9,s.AwayScore);Assert.Equal("Away",e.ScoringTeamName);
        Assert.Equal("Home",s.KickoffTeamName);Assert.True(s.SafetyKickPending);
        Assert.Equal(0,GameDayEngine.BoxScore(s).HomeStats.PassAttempts);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LateFalseStartCanRunOffTimeAndEndAHalf(bool pro)
    {
        for(var seed=0;seed<1000;seed++)
        {
            var source=GameDayFixture.Session(GameDayMode.Play,seed:seed);
            var s=GameDayEngine.Start(source.Game,source.HomeProfile,source.AwayProfile,source.Roster,
                GameDayMode.Play,rules:pro?FootballRules.Professional:FootballRules.College);
            s.KickoffPending=false;s.PossessionTeamName="Home";s.ControlledTeamName="Home";
            s.Quarter=2;s.ClockSeconds=100;s.TwoMinuteTimeoutHalf=1;
            s.HomeTimeouts=s.AwayTimeouts=0;s.AwayScore=7;
            s.Events.Add(new() {OffenseTeamName="Home",DefenseTeamName="Away",OffensivePlayId="",DefensivePlayId="",Outcome=SnapOutcome.ClockStoppage});
            var foul=GameDayEngine.Step(s,"off-spread-zone");
            if(foul.Outcome!=SnapOutcome.Penalty||foul.PenalizedTeamName!="Home")continue;
            Assert.Equal(100-foul.PreSnapSeconds-10,s.ClockSeconds);
            return;
        }
        Assert.Fail("No seeded offensive foul found");
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ProductionSnapsObserveBothHoldingAndPassInterference(bool pro)
    {
        var source=GameDayFixture.Session(GameDayMode.Play);var kinds=new HashSet<string>();
        for(var seed=0;seed<1024 && kinds.Count<2;seed++)
        {
            var s=GameDayEngine.Start(source.Game with {SimulationSeed=seed},source.HomeProfile,source.AwayProfile,source.Roster,
                GameDayMode.Play,rules:pro?FootballRules.Professional:FootballRules.College);
            s.KickoffPending=false;s.PossessionTeamName="Home";s.ControlledTeamName="Home";s.ClockStopped=true;
            var e=GameDayEngine.Step(s,"off-spread-verts");
            if(e.IsLiveBallPenalty && e.PenaltyName is "Offensive holding" or "Defensive pass interference")kinds.Add(e.PenaltyName);
        }
        Assert.Equal(2,kinds.Count);
    }

    [Theory]
    [InlineData(false,0,1,-5)] [InlineData(true,1,0,0)]
    public void GroundingKeepsTheLeaguesDifferentStatisticalAccounting(bool pro,int passes,int rushes,int yards)
    {
        var s=Session(pro);var qb=s.HomeRoster.First(p=>p.Position==Position.QB).PlayerId;
        var pressure=s.AwayRoster.First(p=>p.Position==Position.DE).PlayerId;
        var e=GameDayEngine.ApplyScrimmageResult(s,Play(s) with {PressureRushers=[pressure]},
            [new(FootballFoulKind.IntentionalGrounding,"Home",45,qb)]);
        Assert.Equal(SnapOutcome.Penalty,e.Outcome);
        var box=GameDayEngine.BoxScore(s);Assert.Equal(passes,box.HomeStats.PassAttempts);
        Assert.Equal(rushes,box.HomeStats.RushAttempts);Assert.Equal(yards,box.HomeStats.RushYards);
        var passer=Assert.Single(box.Players,p=>p.PlayerId==qb);
        Assert.Equal(passes,passer.PassAttempts);Assert.Equal(rushes,passer.RushAttempts);
        Assert.Equal(1,passer.Penalties);Assert.Equal(pro?10:0,passer.PenaltyYards);
        var defender=box.Players.FirstOrDefault(p=>p.PlayerId==pressure);Assert.Equal(pro?0:1,defender?.Sacks??0);
    }
    [Theory]
    [InlineData(false,98)] [InlineData(true,99)]
    public void EndZonePassInterferenceUsesTheLeagueGoalLineLimit(bool pro,int spot)
    {
        var s=Session(pro);s.YardLine=85;
        GameDayEngine.ApplyScrimmageResult(s,Play(s),[new(FootballFoulKind.DefensivePassInterference,"Away",100)]);
        Assert.Equal(spot,s.YardLine);Assert.Equal(1,s.Down);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void IllegalPassBeyondTheLineRetainsOnlyTheRunToTheFoulSpot(bool pro)
    {
        var s=Session(pro);var e=GameDayEngine.ApplyScrimmageResult(s,Play(s),
            [new(FootballFoulKind.IllegalForwardPass,"Home",53)],acceptPenalty:true);
        var box=GameDayEngine.BoxScore(s);Assert.Equal(48,s.YardLine);Assert.Equal(3,s.Down);
        Assert.Equal(0,box.HomeStats.PassAttempts);Assert.Equal(1,box.HomeStats.RushAttempts);Assert.Equal(3,box.HomeStats.RushYards);
        Assert.Equal(5,e.PenaltyYards);
    }

    [Theory]
    [InlineData(false,10)] [InlineData(true,0)]
    public void DefensiveDeadBallFoulRunoffPreservesLeagueDifference(bool pro,int runoff)
    {
        for(var seed=0;seed<1000;seed++)
        {
            var source=GameDayFixture.Session(GameDayMode.Play,seed:seed);
            var s=GameDayEngine.Start(source.Game,source.HomeProfile,source.AwayProfile,source.Roster,
                GameDayMode.Play,rules:pro?FootballRules.Professional:FootballRules.College);
            s.KickoffPending=false;s.PossessionTeamName="Home";s.ControlledTeamName="Home";
            s.Quarter=2;s.ClockSeconds=100;s.TwoMinuteTimeoutHalf=1;
            s.HomeTimeouts=s.AwayTimeouts=0;s.HomeScore=7;
            s.Events.Add(new() {OffenseTeamName="Home",DefenseTeamName="Away",OffensivePlayId="",DefensivePlayId="",Outcome=SnapOutcome.ClockStoppage});
            var e=GameDayEngine.Step(s,"off-spread-zone");
            if(e.Outcome!=SnapOutcome.Penalty||e.PenalizedTeamName!="Away")continue;
            Assert.Equal(runoff,e.ClockRunoffSeconds);Assert.Equal(100-e.PreSnapSeconds-runoff,s.ClockSeconds);return;
        }
        Assert.Fail("No seeded defensive foul found");
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DecliningRunoffPreservesTimeAndRestartsOnTheSnap(bool pro)
    {
        var s=Session(pro,4,15);s.HomeTimeouts=0;
        var e=GameDayEngine.ApplyScrimmageResult(s,Play(s),[new(FootballFoulKind.IntentionalGrounding,"Home",45)],acceptRunoff:false);
        Assert.Equal(11,s.ClockSeconds);Assert.Equal(0,e.ClockRunoffSeconds);Assert.False(s.ClockRestartsOnReady);
    }

}

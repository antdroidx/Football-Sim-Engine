using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;
namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public sealed class GoalLineAndTryRulesTests
{
    static GameDaySession Session(bool pro)
    {
        var source=GameDayFixture.Session(GameDayMode.Play);
        var s=GameDayEngine.Start(source.Game,source.HomeProfile,source.AwayProfile,source.Roster,GameDayMode.Play,
            rules:pro?FootballRules.Professional:FootballRules.College);
        s.KickoffPending=false;s.PossessionTeamName="Home";s.ControlledTeamName="Home";s.ClockStopped=true;
        return s;
    }
    static GameSnapEvent Pick() => new() {StatsVersion=1,Sequence=1,Quarter=1,ClockSeconds=300,
        OffenseTeamName="Home",DefenseTeamName="Away",OffensivePlayId="off-spread-verts",DefensivePlayId="",
        Outcome=SnapOutcome.Interception,StartYardLine=80,TurnoverYardLine=98,BallEndYardLine=100,ReturnYards=-2};

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OriginalImpetusAndMomentumDoNotBecomeVoluntaryRetreatSafeties(bool pro)
    {
        var rules=pro?FootballRules.Professional:FootballRules.College;
        Assert.Equal(GoalLineDisposition.Touchback,GoalLineService.Resolve(rules,-1,-2).Disposition);
        var momentum=GoalLineService.Resolve(rules,2,-1,originalMomentum:true);
        Assert.Equal(GoalLineDisposition.MomentumException,momentum.Disposition);Assert.Equal(2,momentum.OwnYardLine);
        Assert.Equal(GoalLineDisposition.Safety,GoalLineService.Resolve(rules,2,-1,ownImpetus:true).Disposition);
        Assert.Equal(GoalLineDisposition.Safety,GoalLineService.Resolve(rules,2,-1,originalMomentum:true,ownEndZoneFoul:true).Disposition);
    }
    [Theory]
    [InlineData(false,GoalLineDisposition.Safety)] [InlineData(true,GoalLineDisposition.MomentumException)]
    public void MomentumExceptionUsesTheLeagueCatchSpotBoundary(bool pro,GoalLineDisposition expected)
    {
        Assert.Equal(expected,GoalLineService.Resolve(pro?FootballRules.Professional:FootballRules.College,6,-1,originalMomentum:true).Disposition);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ReturnSafetyCreditsOriginalOffenseAndConcedingReturnTeamFreeKicks(bool pro)
    {
        var s=Session(pro);var e=Pick() with {IsSafety=true,Points=2,SafetyConcedingTeamName="Away",ReturnerProvidedImpetus=true};
        s.Events.Add(e);GameDayEngine.ApplyTurnoverReturn(s,e);
        Assert.Equal(2,s.HomeScore);Assert.Equal(0,s.AwayScore);Assert.Equal("Home",e.ScoringTeamName);
        Assert.Equal("Home",LiveFootballChoreography.NotificationTeam(e));Assert.Equal("Away",s.KickoffTeamName);
        Assert.True(s.SafetyKickPending);Assert.False(s.TryPending);
        var box=GameDayEngine.BoxScore(s);Assert.Equal(2,box.Periods[0].HomePoints);Assert.Equal(0,box.Periods[0].AwayPoints);
        var kick=GameDayEngine.Step(s);Assert.True(kick.IsSafetyKick);Assert.Equal(80,kick.StartYardLine);
        Assert.Contains("Home scores two points",GameDayCommentaryService.Describe(e,s.PlayersById));
        var loaded=JsonSerializer.Deserialize<GameSnapEvent>(JsonSerializer.Serialize(e))!;Assert.Equal("Home",loaded.ScoringTeamName);
    }
    [Theory]
    [InlineData(false,"Home")] [InlineData(false,"Away")]
    [InlineData(true,"Home")] [InlineData(true,"Away")]
    public void TrySafetiesAwardOnePointAndNeverScheduleASafetyFreeKick(bool pro,string conceding)
    {
        var s=Session(pro);s.TryPending=true;s.HomeScore=6;
        var e=Pick() with {IsTry=true,IsSafety=true,Points=1,Outcome=SnapOutcome.FailedTwoPointConversion,
            SafetyConcedingTeamName=conceding};
        var result=GameDayEngine.ApplyTryResult(s,e);
        Assert.Equal(conceding=="Home"?6:7,s.HomeScore);Assert.Equal(conceding=="Home"?1:0,s.AwayScore);
        Assert.False(s.SafetyKickPending);Assert.False(s.TryPending);Assert.Equal("Home",s.KickoffTeamName);
        Assert.Equal(conceding=="Home"?"Away":"Home",result.ScoringTeamName);
        Assert.Equal(0,GameDayEngine.BoxScore(s).HomeStats.PassAttempts);
        Assert.Contains("ONE-POINT SAFETY",GameDayCommentaryService.Describe(result,s.PlayersById));
        Assert.Equal("ONE-POINT SAFETY",GameDayLivePresentation.ScoreHeadline(result));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DefensiveTryReturnAwardsTwoWithoutADefensiveTouchdownStatistic(bool pro)
    {
        var s=Session(pro);s.HomeScore=6;s.TryPending=true;
        var e=Pick() with {IsTry=true,TryScoredByDefense=true,Points=2,Outcome=SnapOutcome.FailedTwoPointConversion};
        GameDayEngine.ApplyTryResult(s,e);var box=GameDayEngine.BoxScore(s);
        Assert.Equal("TWO-POINT RETURN",GameDayLivePresentation.ScoreHeadline(e));
        Assert.Equal(2,s.AwayScore);Assert.False(e.DefensiveScore);Assert.Equal(2,box.Periods[0].AwayPoints);
        Assert.Equal(0,box.AwayStats.TotalYards);Assert.All(box.Players,p=>Assert.Equal(0,p.Interceptions+p.DefensiveTouchdowns));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TrySafetyDoesNotReplaceTheTouchdownDriveOutcome(bool pro)
    {
        var s=Session(pro);s.YardLine=97;
        var td=Pick() with {Outcome=SnapOutcome.Touchdown,TurnoverYardLine=null,ReturnYards=0,StartYardLine=97,Yards=3,BallEndYardLine=100};
        GameDayEngine.ApplyScrimmageResult(s,td,[]);
        GameDayEngine.ApplyTryResult(s,td with {IsTry=true,Outcome=SnapOutcome.FailedTwoPointConversion,IsSafety=true,
            SafetyConcedingTeamName="Away",Points=1,Yards=0});
        s.IsComplete=true; // Final projection of this isolated drive fixture.
        var result=GameDayEngine.Finish(s);Assert.Equal(DriveOutcome.Touchdown,result.Drives[0].Outcome);
        Assert.Equal(7,result.Drives[0].Points);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ProductionReturnResolutionCanProduceBothSafetyAndMomentumCases(bool pro)
    {
        var s=Session(pro);var returner=s.AwayRoster.First(p=>p.Position==Position.CB);
        var outcomes=new HashSet<bool>();
        for(var seed=0;seed<4000 && outcomes.Count<2;seed++)
        {
            var e=TurnoverReturnService.Resolve(Pick() with {StartYardLine=96,TurnoverYardLine=null},returner,s.HomeRoster,
                OffensiveConcept.QuickPass,new Random(seed),s.Rules);
            if(e.IsSafety){Assert.Equal("Home",e.ScoringTeamName);outcomes.Add(true);}
            if(e.IsMomentumException){Assert.False(e.IsSafety);outcomes.Add(false);}
        }
        Assert.Equal(2,outcomes.Count);
    }
    [Fact]
    public void FailedTryResolverIncludesReturnsAndOnePointSafetiesForEitherTeam()
    {
        var sample=Pick() with {IsTry=true,Points=0,Outcome=SnapOutcome.FailedTwoPointConversion};
        var outcomes=new HashSet<string>();
        for(var seed=0;seed<100_000 && outcomes.Count<3;seed++)
        {
            var e=TryReturnService.ResolveFailure(sample,FootballRules.College,new Random(seed));
            if(e.IsSafety)outcomes.Add(e.ScoringTeamName);
            if(e.TryScoredByDefense && e.Points==2)outcomes.Add("defensive return");
        }
        Assert.Contains("Home",outcomes);Assert.Contains("Away",outcomes);Assert.Contains("defensive return",outcomes);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void InFieldPuntLandingsAreNotAutomaticallyTouchbacks(bool pro)
    {
        for(var seed=0;seed<100;seed++)
        {
            var source=GameDayFixture.Session(GameDayMode.Play,seed:seed);
            var s=GameDayEngine.Start(source.Game,source.HomeProfile,source.AwayProfile,source.Roster,GameDayMode.Play,
                rules:pro?FootballRules.Professional:FootballRules.College);
            s.PossessionTeamName="Home";s.KickoffPending=false;s.YardLine=62;s.Down=4;s.ClockStopped=true;
            var punt=GameDayEngine.Step(s,"special-punt");
            if(punt.KickLandingYardLine is <95 or >=100)continue;
            Assert.False(punt.IsTouchback);return;
        }
        Assert.Fail("No in-field landing found");
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FoulSafetyAtHalfExpiryHasALegalUntimedKickPath(bool pro)
    {
        var s=Session(pro);s.YardLine=2;s.Quarter=4;s.ClockSeconds=1;s.TwoMinuteTimeoutHalf=2;
        s.HomeScore=10;s.AwayScore=7;
        var play=Pick() with {Outcome=SnapOutcome.Incompletion,TurnoverYardLine=null,StartYardLine=2,
            BallEndYardLine=2,ReturnYards=0,Quarter=4,ClockSeconds=1,LiveBallSeconds=4,Down=2,Distance=10};
        GameDayEngine.ApplyScrimmageResult(s,play,[new(FootballFoulKind.OffensiveHolding,"Home",-1)],requestSafetyKick:true);
        Assert.Equal(9,s.AwayScore);Assert.False(s.IsComplete);Assert.True(s.UntimedSafetyKickPending);
        var kick=GameDayEngine.Step(s);
        Assert.True(kick.IsUntimedDown);Assert.True(kick.IsSafetyKick);Assert.Equal(80,kick.StartYardLine);
        Assert.False(s.UntimedSafetyKickPending);Assert.False(s.UntimedDownPending);
        if(kick.Points<6)Assert.True(s.IsComplete);
    }
    [Fact]
    public void ProReceiversCanDeclineTheExpiredHalfSafetyKick()
    {
        var s=Session(true);s.YardLine=2;s.Quarter=4;s.ClockSeconds=1;s.HomeScore=10;s.AwayScore=7;
        GameDayEngine.ApplyScrimmageResult(s,Pick() with {Outcome=SnapOutcome.Incompletion,TurnoverYardLine=null,
            StartYardLine=2,LiveBallSeconds=4,Quarter=4,Down=2,Distance=10},
            [new(FootballFoulKind.OffensiveHolding,"Home",-1)],requestSafetyKick:false);
        Assert.True(s.IsComplete);Assert.False(s.KickoffPending);Assert.False(s.UntimedSafetyKickPending);
    }
    [Theory]
    [InlineData(false,false)] [InlineData(true,true)]
    public void ReturnSafetyInOvertimePreservesLeaguePossessionOpportunities(bool pro,bool complete)
    {
        var s=Session(pro);s.Quarter=5;s.OvertimeRound=1;s.OvertimeFirstTeam="Home";
        var e=Pick() with {Quarter=5,IsSafety=true,Points=2,SafetyConcedingTeamName="Away"};s.Events.Add(e);
        GameDayEngine.ApplyTurnoverReturn(s,e);
        Assert.Equal(2,s.HomeScore);Assert.Equal(complete,s.IsComplete);Assert.False(s.KickoffPending);
        if(!complete){Assert.Equal("Away",s.PossessionTeamName);Assert.Equal(75,s.YardLine);}
    }

}

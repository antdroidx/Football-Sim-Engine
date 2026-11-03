using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

public static partial class GameDayEngine
{
    // Common event-commit boundary for generated snaps and explicit referee observations.
    public static GameSnapEvent ApplyScrimmageResult(GameDaySession session, GameSnapEvent play,
        IReadOnlyList<FootballFoul>? fouls=null, bool? acceptPenalty=null, bool? acceptRunoff=null, bool? useTimeout=null, bool? requestSafetyKick=null)
    {
        if(session.IsComplete)throw new InvalidOperationException("The game is complete.");
        if(play.OffenseTeamName!=session.PossessionTeamName || play.StartYardLine!=session.YardLine)
            throw new ArgumentException("Play does not match the current possession and spot.");
        fouls??=FootballPenaltyService.Observe(session,play);
        var decision=fouls.Count>0?FootballPenaltyService.Decide(session.Rules,play,fouls,acceptPenalty):null;
        var untimed=session.UntimedDownPending;session.UntimedDownPending=false;
        play=play with {IsUntimedDown=untimed};
        if(decision is {Accepted:true})
        {
            var grounding=!decision.Offsetting && decision.Foul.Kind==FootballFoulKind.IntentionalGrounding;
            var illegalForward=!decision.Offsetting && decision.Foul.Kind==FootballFoulKind.IllegalForwardPass;
            SnapOutcome? statOutcome=grounding ? session.Rules.SacksCountAsRushing?SnapOutcome.Sack:SnapOutcome.Incompletion
                : illegalForward?SnapOutcome.Scramble:null;
            var foul=play with {StatisticalOutcome=statOutcome,
                StatisticalYards=grounding && session.Rules.SacksCountAsRushing || illegalForward
                    ? Math.Max(-play.StartYardLine,decision.Foul.Spot-play.StartYardLine):0,
                StatisticalPenaltyYards=grounding && session.Rules.SacksCountAsRushing?0:null,
                Outcome=SnapOutcome.Penalty,IsLiveBallPenalty=true,IsOffsettingPenalty=decision.Offsetting,
                PenaltyName=decision.Name,PenalizedTeamName=decision.Offsetting?null:decision.Foul.Team,
                PenalizedPlayerId=decision.Offsetting?null:decision.Foul.PlayerId,
                PenaltyYards=decision.Offsetting||decision.Safety?0:Math.Abs(decision.Spot-(illegalForward?decision.Foul.Spot:play.StartYardLine)),
                Yards=0,Points=0,TurnoverYardLine=null,IsTouchback=false,IsSafety=false,
                BallEndYardLine=decision.Safety?0:decision.Spot,ReturnYards=0};
            session.Events.Add(foul);ConsumeLiveClock(session,play.LiveBallSeconds);
            var home=play.OffenseTeamName==session.Game.HomeTeamName;
            if(decision.Safety)
            {
                AwardSafety(session,home);session.Events[^1]=session.Events[^1] with {Yards=0};
                if(session.OvertimeRound==0 && session.ClockSeconds==0 && session.Quarter is 2 or 4)
                {
                    var scoring=session.Events[^1].ScoringTeamName;
                    var margin=scoring==session.Game.HomeTeamName?session.HomeScore-session.AwayScore:session.AwayScore-session.HomeScore;
                    var mandatory=session.Rules.OffensiveLiveBallFoulExtendsHalf && !decision.LossOfDown;
                    if(mandatory || (requestSafetyKick??(session.Quarter==2 || margin<=0)))
                        session.UntimedDownPending=session.UntimedSafetyKickPending=true;
                }
            }
            else
            {
                session.YardLine=decision.Spot;session.Down=decision.FirstDown?1:play.Down+(decision.LossOfDown?1:0);
                session.Distance=decision.FirstDown?Math.Min(10,100-decision.Spot):play.Distance+play.StartYardLine-decision.Spot;
                if(session.Distance<=0){session.Down=1;session.Distance=Math.Min(10,100-session.YardLine);}
                session.ClockStopped=true;session.ClockRestartsOnReady=play.Outcome is not (SnapOutcome.Incompletion or SnapOutcome.Interception or SnapOutcome.Fumble or SnapOutcome.Turnover);
                session.PlayClockSeconds=25;
                if(session.Down>4)ChangePossession(session,100-session.YardLine);
                if(session.OvertimeRound==0 && session.ClockSeconds==0)
                {
                    var half=session.Quarter is 2 or 4;
                    session.UntimedDownPending=decision.Offsetting?half:!decision.Offense
                        ? half||session.Rules.ExtendAllPeriodsForDefensiveFoul
                        : half && session.Rules.OffensiveLiveBallFoulExtendsHalf && !decision.LossOfDown;
                }
            }
            if(!decision.Offsetting && decision.LossOfDown && (play.Outcome==SnapOutcome.Incompletion || illegalForward && play.IsQuarterbackScramble))
                ApplyFoulRunoff(session,decision.Foul.Team,true,true,acceptRunoff,useTimeout,play.OffenseTeamName);
            CheckTwoMinuteTimeout(session);AdvanceQuarter(session);
            return session.Events[^1];
        }
        if(decision is not null)play=play with {DeclinedPenaltyName=decision.Name};
        session.Events.Add(play);ConsumeLiveClock(session,play.LiveBallSeconds);
        if(play.TurnoverYardLine.HasValue)ApplyTurnoverReturn(session,play);
        else AdvanceState(session,play.OffenseTeamName==session.Game.HomeTeamName,play.Yards,
            play.Outcome is SnapOutcome.Interception or SnapOutcome.Fumble or SnapOutcome.Turnover,
            play.Outcome==SnapOutcome.Touchdown,play.Outcome==SnapOutcome.Incompletion);
        return session.Events[^1];
    }

    private static void ApplyFoulRunoff(GameDaySession s,string team,bool eligible,bool clockWasRunning,
        bool? accept=null,bool? useTimeout=null,string? offenseAtFoul=null)
    {
        offenseAtFoul??=s.PossessionTeamName;
        if(!eligible || !clockWasRunning || s.Quarter is not (2 or 4) || s.ClockSeconds is <=0 or >120 ||
            team!=offenseAtFoul && !s.Rules.DefensiveFoulCanCauseRunoff)return;
        var half=s.Quarter==2?1:2;
        if(s.TwoMinuteTimeoutHalf<half)return;
        var opponent=team==s.Game.HomeTeamName?s.Game.AwayTeamName:s.Game.HomeTeamName;
        int Margin(string t)=>t==s.Game.HomeTeamName?s.HomeScore-s.AwayScore:s.AwayScore-s.HomeScore;
        var take=accept??(opponent==offenseAtFoul?Margin(opponent)>0:Margin(opponent)>=0);
        if(!take){s.ClockRestartsOnReady=false;return;}
        var available=team==s.Game.HomeTeamName?s.HomeTimeouts:s.AwayTimeouts;
        if(available>0 && (useTimeout??Margin(team)<=0))
        {
            if(team==s.Game.HomeTeamName)s.HomeTimeouts--;else s.AwayTimeouts--;
            s.ClockRestartsOnReady=false;
            s.Events[^1]=s.Events[^1] with {RunoffAvoidedByTimeoutTeam=team,HomeTimeoutsRemaining=s.HomeTimeouts,AwayTimeoutsRemaining=s.AwayTimeouts};
        }
        else
        {
            var seconds=Math.Min(10,s.ClockSeconds);s.ClockSeconds-=seconds;s.ClockRestartsOnReady=true;
            s.PlayClockSeconds=s.Rules.DefensiveFoulCanCauseRunoff?25:30;
            s.Events[^1]=s.Events[^1] with {ClockRunoffSeconds=seconds};
        }
    }
}

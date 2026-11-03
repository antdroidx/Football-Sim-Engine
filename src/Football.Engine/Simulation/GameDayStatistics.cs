using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

/// <summary>Keep the referee event intact while applying league statistical exceptions.</summary>
public static class GameDayStatistics
{
    public static GameSnapEvent Project(GameSnapEvent e)
    {
        if(e.StatisticalOutcome is not SnapOutcome outcome)return e;
        return e with {Outcome=outcome,Yards=e.StatisticalYards,TargetId=null,CoverageDefenderId=null,
            IsDrop=false,IsPassBreakup=false,AirYards=0,YardsAfterCatch=0,
            BallCarrierId=outcome is SnapOutcome.Sack or SnapOutcome.Scramble?e.QuarterbackId:null,
            PrimaryDefenderId=outcome==SnapOutcome.Sack?e.PressureRushers.Cast<Guid?>().FirstOrDefault():null,
            SackBlockerId=null,IsQuarterbackScramble=outcome==SnapOutcome.Scramble,
            IsQuarterbackKeep=false,StatisticalOutcome=null};
    }
}

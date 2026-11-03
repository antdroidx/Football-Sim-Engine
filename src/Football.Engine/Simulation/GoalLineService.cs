using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

public enum GoalLineDisposition { InField, Touchback, MomentumException, Safety }
public sealed record GoalLineRuling(GoalLineDisposition Disposition, int OwnYardLine, int Points);
public sealed record ReturnTrajectory(int Yards, GoalLineRuling Ruling, bool OwnImpetus);

public static class GoalLineService
{
    public static GoalLineRuling Resolve(FootballRules rules,int possessionOwnSpot,int deadOwnSpot,
        bool ownImpetus=false,bool originalMomentum=false,bool ownEndZoneFoul=false,bool isTry=false,int touchbackSpot=20)
    {
        if(ownEndZoneFoul)return new(GoalLineDisposition.Safety,0,isTry?1:2);
        if(deadOwnSpot>0)return new(GoalLineDisposition.InField,Math.Clamp(deadOwnSpot,1,100),0);
        if(!ownImpetus && possessionOwnSpot<=0)return new(GoalLineDisposition.Touchback,touchbackSpot,0);
        if(!ownImpetus && originalMomentum && possessionOwnSpot>0 &&
            (!rules.MomentumExceptionInsideFiveOnly || possessionOwnSpot<5))
            return new(GoalLineDisposition.MomentumException,Math.Max(1,possessionOwnSpot),0);
        return new(GoalLineDisposition.Safety,0,isTry?1:2);
    }

    public static ReturnTrajectory Return(FootballRules rules,int catchOwnSpot,int plannedYards,Random random,bool isTry=false)
    {
        var yards=plannedYards;var momentum=false;
        if(catchOwnSpot is >0 and <=5 && random.NextDouble()<.02)
        {
            yards=-random.Next(1,8);momentum=random.NextDouble()<.5;
        }
        var ruling=Resolve(rules,catchOwnSpot,catchOwnSpot+yards,ownImpetus:!momentum,
            originalMomentum:momentum,isTry:isTry);
        return new(ruling.Disposition==GoalLineDisposition.MomentumException?0:
            ruling.Disposition==GoalLineDisposition.Safety?-catchOwnSpot:yards,ruling,!momentum);
    }
}

public static class TryReturnService
{
    public static GameSnapEvent ResolveFailure(GameSnapEvent snap,FootballRules rules,Random random)
    {
        if(snap.Points!=0 || random.NextDouble()>=.035)return snap;
        var catchOwn=snap.KickerId.HasValue?Math.Max(1,100-snap.StartYardLine):random.Next(1,4);
        if(random.NextDouble()<.001)
        {
            // A defensive return is fumbled; the original offense recovers and retreats into its own end zone.
            var ruling=GoalLineService.Resolve(rules,1,-1,ownImpetus:true,isTry:true);
            return snap with {IsSafety=true,Points=ruling.Points,SafetyConcedingTeamName=snap.OffenseTeamName,
                ReturnFumbleRecoveredByOffense=true,TurnoverYardLine=100-catchOwn,
                ReturnYards=99-catchOwn,BallEndYardLine=0,ReturnerProvidedImpetus=true};
        }
        var planned=random.NextDouble()<.08?100-catchOwn:random.Next(0,22);
        var trajectory=GoalLineService.Return(rules,catchOwn,planned,random,isTry:true);
        var safety=trajectory.Ruling.Disposition==GoalLineDisposition.Safety;
        var scored=trajectory.Ruling.OwnYardLine>=100;
        return snap with {TurnoverYardLine=100-catchOwn,ReturnYards=trajectory.Yards,
            BallEndYardLine=100-trajectory.Ruling.OwnYardLine,IsSafety=safety,
            SafetyConcedingTeamName=safety?snap.DefenseTeamName:null,
            IsMomentumException=trajectory.Ruling.Disposition==GoalLineDisposition.MomentumException,
            ReturnerProvidedImpetus=trajectory.OwnImpetus,Points=safety?1:scored?2:0,TryScoredByDefense=scored};
    }
}

using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

public enum FootballFoulKind { OffensiveHolding, DefensivePassInterference, IntentionalGrounding, IllegalForwardPass }
public sealed record FootballFoul(FootballFoulKind Kind, string Team, int Spot, Guid? PlayerId = null);
public sealed record PenaltyDecision(bool Accepted, bool Offense, bool Offsetting, int Spot, bool LossOfDown,
    bool FirstDown, bool Safety, string Name, FootballFoul Foul);

public static class FootballPenaltyService
{
    public static IReadOnlyList<FootballFoul> Observe(GameDaySession s, GameSnapEvent play)
    {
        if(!play.IsDropback)return [];
        var roll=new Random(unchecked(s.Game.SimulationSeed*397 ^ play.Sequence*7919 ^ 0xF04)).NextDouble();
        if(roll<.015)
        {
            var blocker=play.Blocks.FirstOrDefault()?.BlockerId;
            return [new(FootballFoulKind.OffensiveHolding,play.OffenseTeamName,play.StartYardLine-3,blocker)];
        }
        if(roll<.027 && play.TargetId.HasValue && play.Outcome!=SnapOutcome.Sack)
            return [new(FootballFoulKind.DefensivePassInterference,play.DefenseTeamName,
                Math.Min(100,play.StartYardLine+Math.Max(1,play.AirYards>0?play.AirYards:10)),play.CoverageDefenderId)];
        if(roll<.028 && play.Outcome==SnapOutcome.Incompletion && play.PressureRushers.Count>0)
            return [new(FootballFoulKind.IntentionalGrounding,play.OffenseTeamName,play.StartYardLine-5,play.QuarterbackId)];
        if(roll<.030 && play.IsQuarterbackScramble && play.Yards>0)
            return [new(FootballFoulKind.IllegalForwardPass,play.OffenseTeamName,
                Math.Min(99,play.StartYardLine+Math.Min(3,play.Yards)),play.QuarterbackId)];
        return [];
    }

    public static PenaltyDecision Decide(FootballRules rules, GameSnapEvent play, IReadOnlyList<FootballFoul> fouls, bool? accept=null)
    {
        if(fouls.Count==0)throw new ArgumentException("A foul is required.");
        if(fouls.Any(f=>f.Team!=play.OffenseTeamName && f.Team!=play.DefenseTeamName))throw new ArgumentException("Foul team is not in this play.");
        if(fouls.Any(f=>(f.Kind==FootballFoulKind.DefensivePassInterference)!=(f.Team==play.DefenseTeamName)))
            throw new ArgumentException("Foul kind does not match the offending unit.");
        var foul=fouls[0];var offense=foul.Team==play.OffenseTeamName;
        if(fouls.Select(f=>f.Team).Distinct().Count()>1)
            return new(true,false,true,play.StartYardLine,false,false,false,"Offsetting fouls",foul);
        if(fouls.Count>1)
            return fouls.Select(f=>Decide(rules,play,[f],accept)).OrderByDescending(d=>d.Safety?100_000:
                d.LossOfDown?10_000:Math.Abs(d.Spot-play.StartYardLine)).First();
        var loss=foul.Kind is FootballFoulKind.IntentionalGrounding or FootballFoulKind.IllegalForwardPass;
        var safety=offense && foul.Spot<=0;
        var spot=foul.Kind switch {
            FootballFoulKind.DefensivePassInterference => rules.DefensivePassInterferenceIsSpotFoul
                ? Math.Min(99,Math.Max(play.StartYardLine,foul.Spot))
                : play.StartYardLine>=98 ? play.StartYardLine+(100-play.StartYardLine)/2
                    : Math.Min(98,Math.Min(foul.Spot,play.StartYardLine+15)),
            FootballFoulKind.IntentionalGrounding => rules.GroundingMinimumTenYards
                ? Math.Min(foul.Spot,play.StartYardLine-Math.Min(10,play.StartYardLine/2)) : foul.Spot,
            FootballFoulKind.IllegalForwardPass => foul.Spot-Math.Min(5,Math.Max(0,foul.Spot/2)),
            _ => play.StartYardLine-Math.Min(10,play.StartYardLine/2)
        };
        spot=Math.Clamp(spot,1,99);
        var turnover=play.Outcome is SnapOutcome.Interception or SnapOutcome.Fumble or SnapOutcome.Turnover;
        var offensiveScore=play.Points>0 && play.ScoringTeamName==play.OffenseTeamName || play.Outcome==SnapOutcome.Touchdown;
        var beneficial=offense ? safety || play.IsSafety && play.SafetyConcedingTeamName==play.DefenseTeamName || !turnover && !(play.Down==4 && play.Yards<play.Distance && !offensiveScore) && play.Yards>=spot-play.StartYardLine
            : !offensiveScore && (turnover || play.Yards<spot-play.StartYardLine || play.Yards<play.Distance);
        var name=foul.Kind switch {FootballFoulKind.OffensiveHolding=>"Offensive holding",FootballFoulKind.DefensivePassInterference=>"Defensive pass interference",
            FootballFoulKind.IntentionalGrounding=>"Intentional grounding",_=>"Illegal forward pass"};
        return new(accept??beneficial,offense,false,spot,loss,!offense,safety,name,foul);
    }
}

using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

public static class LiveFootballChoreography
{
    public static IReadOnlySet<Guid> AssistIds(GameSnapEvent snap)
    {
        var count=snap.Outcome switch{SnapOutcome.Sack=>0,SnapOutcome.Run when snap.Yards<0=>1,SnapOutcome.Run or SnapOutcome.Scramble or SnapOutcome.Completion or SnapOutcome.Fumble=>snap.Yards>=8?2:1,_=>0};
        return snap.TackleAttemptIds.Where(id=>id!=snap.PrimaryDefenderId).Take(count).ToHashSet();
    }
    public static GameFieldPoint DefensiveDestination(GameFieldPoint start,GameFieldPoint contact,double line,double destination,int index,bool primary,bool assist)
    {
        if(primary||assist)return GameDayAnimationPlanner.PursuitTarget(contact,index,primary);
        return new(Math.Clamp(start.X+(destination-line)*.72,.025,.96),Math.Clamp(start.Y+((index%3)-1)*.025,.07,.93));
    }

    public static bool Zone(DefensivePlayDefinition defense, PlayAssignment? assignment)
    {
        if (assignment is not null) return assignment.Task == AssignmentTask.Zone;
        var name = defense.Name;
        if (name.Contains("Man", StringComparison.OrdinalIgnoreCase) || name.Contains("Cover 0") || name.Contains("Cover 1")) return false;
        return name.Contains("Zone", StringComparison.OrdinalIgnoreCase) || name.Contains("Cover 2") || name.Contains("Cover 3") ||
            name.Contains("Cover 4") || name.Contains("Cover 6") || name.Contains("Quarters", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Tampa", StringComparison.OrdinalIgnoreCase) || defense.Approach == DefensiveApproach.ProtectPass;
    }

    public static string NotificationTeam(GameSnapEvent snap) => snap.IsSafety || snap.IsTry && snap.Points>0 ? snap.ScoringTeamName : snap.Outcome is SnapOutcome.Interception or SnapOutcome.Fumble ||
        snap.Outcome == SnapOutcome.Punt && (snap.IsTouchback || snap.Points >= 6) ? snap.DefenseTeamName : snap.OffenseTeamName;

    public static int KickLandingYardLine(GameSnapEvent snap) => snap.IsTouchback
        ? snap.Outcome == SnapOutcome.Kickoff ? -3 : 103
        : snap.KickLandingYardLine > 0 ? snap.KickLandingYardLine : snap.BallEndYardLine;

    public static GameFieldPoint Coverage(GameFieldPoint start, GameFieldPoint receiver, double progress, bool zone, int direction)
    {
        var reaction = Math.Clamp(progress / .20, 0, 1);
        var lane = zone ? start.Y + (receiver.Y - start.Y) * .35 : receiver.Y;
        // Backpedal/turn and carry deep threats rather than stopping at the statistical gain.
        var x = direction > 0 ? Math.Max(start.X, receiver.X + 2 / 120d) : Math.Min(start.X, receiver.X - 2 / 120d);
        return new(Math.Clamp(start.X + (x - start.X) * reaction, .02, .98), Math.Clamp(start.Y + (lane - start.Y) * reaction, .06, .94));
    }

    public static (GameFieldPoint From, GameFieldPoint To) Punt(
        bool receiving,
        bool specialist,
        int slot,
        int landing,
        int end,
        int direction,
        int kickStartYardLine = 40)
    {
        double fromYard;
        double toYard;
        double fromLane;
        double toLane;
        if (receiving)
        {
            if (specialist)
            {
                fromYard = landing;
                toYard = end;
                fromLane = toLane = .50;
            }
            else if (slot is 1 or 2)
            {
                fromYard = kickStartYardLine + 1;
                toYard = Math.Clamp(landing - 14 - slot % 2 * 5, kickStartYardLine + 5, 98);
                fromLane = slot == 1 ? .04 : .96;
                toLane = slot == 1 ? .18 : .82;
            }
            else if (slot is 3 or 4)
            {
                fromYard = kickStartYardLine + 5 + (slot - 3) * 2;
                toYard = Math.Clamp(landing - 24 - (slot - 3) * 4, kickStartYardLine + 7, 95);
                fromLane = slot == 3 ? .32 : .68;
                toLane = slot == 3 ? .42 : .58;
            }
            else
            {
                fromYard = kickStartYardLine + 2 + slot % 3 * 1.5;
                toYard = Math.Clamp(landing - 8 - slot % 3 * 5, kickStartYardLine + 4, 94);
                fromLane = Math.Clamp(.15 + (slot - 5) * .14, .12, .88);
                toLane = Math.Clamp(fromLane + (slot % 2 == 0 ? .06 : -.06), .10, .90);
            }
        }
        else if (specialist)
        {
            fromYard = kickStartYardLine - 8;
            toYard = kickStartYardLine;
            fromLane = toLane = .50;
        }
        else if (slot == 1)
        {
            fromYard = kickStartYardLine - .5;
            toYard = kickStartYardLine + .5;
            fromLane = toLane = .50;
        }
        else if (slot is >= 2 and <= 6)
        {
            fromYard = kickStartYardLine - .3;
            toYard = kickStartYardLine + 2;
            fromLane = toLane = .34 + (slot - 2) * .08;
        }
        else if (slot is 7 or 8)
        {
            fromYard = kickStartYardLine + .5;
            toYard = Math.Clamp(landing - 6, kickStartYardLine + 8, 98);
            fromLane = slot == 7 ? .04 : .96;
            toLane = slot == 7 ? .18 : .82;
        }
        else
        {
            fromYard = kickStartYardLine + 2;
            toYard = Math.Clamp(landing - 2 - slot % 2 * 3, kickStartYardLine + 8, 98);
            fromLane = slot == 9 ? .24 : .76;
            toLane = slot == 9 ? .34 : .66;
        }

        return (
            new(GameDayAnimationPlanner.ProjectFieldYardLine(fromYard, direction), fromLane),
            new(GameDayAnimationPlanner.ProjectFieldYardLine(toYard, direction), toLane));
    }

    public static (GameFieldPoint From, GameFieldPoint To) Kickoff(bool receiving, bool specialist, int slot, int landing, int end, int direction, int kickStartYardLine = 65, bool dynamicKickoff = false, bool onside = false)
    {
        var lane = specialist ? .5 : .08 + slot % 10 * .093;
        var fromYard = KickoffPolicy.AlignmentYard(receiving, specialist, slot, landing, kickStartYardLine, dynamicKickoff, onside);
        var toYard = receiving ? specialist ? end : Math.Clamp(end + 9 + slot % 3 * 4, 12, 62) : specialist ? 52 : Math.Clamp(end + (slot % 3) * 4, 1, 75);
        return (new(GameDayAnimationPlanner.ProjectFieldYardLine(fromYard, direction), lane),
            new(GameDayAnimationPlanner.ProjectFieldYardLine(toYard, direction), specialist && receiving ? .5 : lane));
    }
}

using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public enum SpecialTeamsDefensiveCall
{
    None,
    PuntSafe,
    PuntBlock,
    FieldGoalSafe,
    FieldGoalBlock
}

public static class SpecialTeamsCallService
{
    public const string PuntSafeId = "special-def-punt-safe";
    public const string PuntBlockId = "special-def-punt-block";
    public const string FieldGoalSafeId = "special-def-field-goal-safe";
    public const string FieldGoalBlockId = "special-def-field-goal-block";

    public static bool IsSpecialDefensiveCall(string? id) => id is
        PuntSafeId or PuntBlockId or FieldGoalSafeId or FieldGoalBlockId;

    public static SpecialTeamsDefensiveCall Resolve(string? id, bool fieldGoal)
    {
        if (fieldGoal)
        {
            return id switch
            {
                FieldGoalBlockId => SpecialTeamsDefensiveCall.FieldGoalBlock,
                FieldGoalSafeId or PuntSafeId or PuntBlockId => SpecialTeamsDefensiveCall.FieldGoalSafe,
                _ => SpecialTeamsDefensiveCall.None
            };
        }

        return id switch
        {
            PuntBlockId => SpecialTeamsDefensiveCall.PuntBlock,
            PuntSafeId or FieldGoalSafeId or FieldGoalBlockId => SpecialTeamsDefensiveCall.PuntSafe,
            _ => SpecialTeamsDefensiveCall.None
        };
    }

    public static double BlockChance(SpecialTeamsDefensiveCall call, bool fieldGoal, double specialTeamsDifference = 0)
    {
        var baseline = fieldGoal ? .035 : .025;
        var selected = call switch
        {
            SpecialTeamsDefensiveCall.PuntBlock or SpecialTeamsDefensiveCall.FieldGoalBlock => fieldGoal ? .105 : .085,
            SpecialTeamsDefensiveCall.PuntSafe or SpecialTeamsDefensiveCall.FieldGoalSafe => baseline * .45,
            _ => baseline
        };
        return Math.Clamp(selected + specialTeamsDifference * .00035, .005, .18);
    }

    public static int PuntReturnYards(SpecialTeamsDefensiveCall call, int plannedYards, Random random) => call switch
    {
        SpecialTeamsDefensiveCall.PuntSafe => plannedYards / 2,
        SpecialTeamsDefensiveCall.PuntBlock => Math.Max(0, plannedYards + random.Next(0, 7)),
        _ => plannedYards
    };

    public static bool FairCatch(SpecialTeamsDefensiveCall call, bool coverageFairCatch, Random random) => call switch
    {
        SpecialTeamsDefensiveCall.PuntSafe => coverageFairCatch || random.NextDouble() < .30,
        SpecialTeamsDefensiveCall.PuntBlock => coverageFairCatch && random.NextDouble() >= .20,
        _ => coverageFairCatch
    };

    public static DynastyPlayer? Blocker(IReadOnlyList<DynastyPlayer> unit, Guid? returnerId, Random random)
    {
        var candidates = unit.Where(player => player.PlayerId != returnerId && player.Position != Position.K).ToArray();
        if (candidates.Length == 0) return null;
        var front = candidates.Where(player => player.Position is Position.DE or Position.DT or Position.OLB or Position.MLB).ToArray();
        var pool = front.Length > 0 ? front : candidates;
        return pool.OrderByDescending(player => (player.StrengthRating + player.AgilityRating) / 2d)
            .ThenBy(player => player.PlayerId).Take(Math.Min(3, pool.Length)).ToArray()[random.Next(Math.Min(3, pool.Length))];
    }
}
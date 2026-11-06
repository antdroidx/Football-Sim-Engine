using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class PreSnapCatalog
{
    public static readonly PreSnapChoice None = new(PreSnapType.None, "None", 0);

    public static IReadOnlyList<PreSnapChoice> Common =>
    [
        None,
        Shift("Left", .5),
        Shift("Right", .5),
        Motion("Left", 2),
        Motion("Right", 2)
    ];

    public static PreSnapChoice Shift(string side, double yards) => new(PreSnapType.Shift, side, yards);

    public static PreSnapChoice Motion(
        string side,
        double yards,
        PreSnapMotionDirection direction = PreSnapMotionDirection.Lateral) =>
        new(PreSnapType.Motion, side, yards) { MotionDirection = direction };

    public static bool IsValid(PreSnapChoice choice) => IsValid(choice, FootballRules.College);

    /// <summary>
    /// Validates sourced NCAA/NFL alignment boundaries plus the repository's internal catalog distance bounds.
    /// The rulebooks do not define a numeric shift or motion yard limit.
    /// </summary>
    public static bool IsValid(PreSnapChoice choice, FootballRules rules)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(rules);
        if (!choice.SetForOneSecond) return false;

        return choice.Type switch
        {
            PreSnapType.None => choice.Yards == 0,
            PreSnapType.Shift => choice.Yards >= 0 && choice.Yards <= rules.MaximumShiftYards,
            PreSnapType.Motion => choice.Yards >= 0 && choice.Yards <= rules.MaximumMotionYards &&
                choice.BackfieldPlayersInMotion > 0 &&
                choice.BackfieldPlayersInMotion <= rules.MaximumBackfieldPlayersInMotion &&
                choice.AllOtherPlayersStationary &&
                (!rules.MotionMustBeParallelOrAwayFromGoalLine ||
                 choice.MotionDirection is PreSnapMotionDirection.Lateral or PreSnapMotionDirection.AwayFromGoalLine) &&
                (!rules.RequiresOneSecondSetAfterShift || !choice.FollowedShift || choice.SetForOneSecond) &&
                (!rules.MotionFromLineRequiresBackAndCompleteStop ||
                 !choice.StartedOnLineOfScrimmage || choice.BecameBackAndStopped),
            _ => false
        };
    }

    /// <summary>The bounded outcome effect of a pre-snap alignment action.</summary>
    public static PreSnapEffect? Effect(PreSnapChoice choice, bool isPass) => choice.Type switch
    {
        PreSnapType.Motion => isPass
            ? new(2, "Motion created separation and added 2 yards.")
            : new(1, "Motion shifted the defense and added 1 yard."),
        PreSnapType.Shift => new(1, "The shift opened a lane and added 1 yard."),
        _ => null
    };

    public static string Label(PreSnapChoice choice) => choice.Type switch
    {
        PreSnapType.None => "None",
        PreSnapType.Shift => "Shift " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion => $"Motion {choice.Side.ToLowerInvariant()} {choice.Yards:0.#} yd",
        _ => choice.Type.ToString()
    };
}

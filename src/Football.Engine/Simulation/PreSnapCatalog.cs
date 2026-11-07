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
        PreSnapMotionDirection direction = PreSnapMotionDirection.Lateral,
        PreSnapMotionKind kind = PreSnapMotionKind.ReturnToFormation) =>
        new(PreSnapType.Motion, side, yards) { MotionDirection = direction, MotionKind = kind };

    public static PreSnapChoice Jet(string side, Position position = Position.WR, int roleIndex = 1) =>
        Motion(side, 3, PreSnapMotionDirection.Lateral, PreSnapMotionKind.JetHandoff) with
        {
            MotionPosition = position,
            MotionRoleIndex = roleIndex,
            SnapDepth = -.75,
            MotionManIsBallCarrier = true,
            MotionLiveAtSnap = true,
            SetForOneSecond = false
        };

    public static PreSnapChoice Set(
        string side,
        Position position = Position.WR,
        int roleIndex = 0,
        double snapLane = .35,
        double snapDepth = -2) =>
        Motion(side, 2, PreSnapMotionDirection.Lateral, PreSnapMotionKind.SetAcross) with
        {
            MotionPosition = position,
            MotionRoleIndex = roleIndex,
            SnapLane = snapLane,
            SnapDepth = snapDepth
        };

    public static PreSnapChoice Orbit(string side, Position position = Position.WR, int roleIndex = 0) =>
        Motion(side, 3, PreSnapMotionDirection.Lateral, PreSnapMotionKind.OrbitAcross) with
        {
            MotionPosition = position,
            MotionRoleIndex = roleIndex,
            SnapLane = side.Equals("Right", StringComparison.OrdinalIgnoreCase) ? .82 : .18,
            SnapDepth = -2.5
        };

    public static PreSnapChoice Swap(
        string side,
        Position position = Position.WR,
        int roleIndex = 0,
        Position swapPosition = Position.WR,
        int swapRoleIndex = 1) =>
        Motion(side, 2, PreSnapMotionDirection.Lateral, PreSnapMotionKind.SwapAlignments) with
        {
            MotionPosition = position,
            MotionRoleIndex = roleIndex,
            SwapPosition = swapPosition,
            SwapRoleIndex = swapRoleIndex
        };

    public static bool IsValid(PreSnapChoice choice) => IsValid(choice, FootballRules.College);

    /// <summary>
    /// Validates sourced NCAA/NFL alignment boundaries plus the repository's internal catalog distance bounds.
    /// The rulebooks do not define a numeric shift or motion yard limit.
    /// </summary>
    public static bool IsValid(PreSnapChoice choice, FootballRules rules)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(rules);
        if (choice.Type == PreSnapType.Shift && !choice.SetForOneSecond) return false;
        if (choice.FollowedShift && !choice.SetForOneSecond) return false;
        if (!Enum.IsDefined(choice.MotionDirection) || !Enum.IsDefined(choice.MotionKind)) return false;

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
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.JetHandoff =>
            new(1, "Jet motion held the edge and added 1 yard."),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SwapAlignments =>
            new(1, "The slot swap created a cleaner exchange and added 1 yard."),
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
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.JetHandoff => "Jet motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.OrbitAcross => "Orbit motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SetAcross => "Set motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SwapAlignments => "Slot swap",
        PreSnapType.Motion => $"Motion {choice.Side.ToLowerInvariant()} {choice.Yards:0.#} yd",
        _ => choice.Type.ToString()
    };

    public static string CardLabel(PreSnapChoice choice) => choice.Type switch
    {
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.JetHandoff => "JET",
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.OrbitAcross => "ORBIT",
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SetAcross => "SET",
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SwapAlignments => "SWAP",
        PreSnapType.Motion => "MOTION",
        _ => ""
    };
}
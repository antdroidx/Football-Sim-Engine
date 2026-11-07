using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class PreSnapCatalog
{
    public static readonly PreSnapChoice None = new(PreSnapType.None, "None", 0);

    public static IReadOnlyList<PreSnapChoice> Common => Options(null);

    public static IReadOnlyList<PreSnapChoice> Options(OffensivePlayDefinition? play)
    {
        var options = new List<PreSnapChoice>
        {
            None,
            ReceiverShift("Left", Position.WR, 0),
            ReceiverShift("Right", Position.WR, 0)
        };

        if (play?.PreSnapAction is { HasAlignmentAction: true } preset)
            options.Add(preset);
        else
        {
            options.Add(ReceiverMotion("Left", Position.WR, 0, PreSnapMotionKind.SetAcross));
            options.Add(ReceiverMotion("Right", Position.WR, 0, PreSnapMotionKind.SetAcross));
            options.Add(Swap("Right", Position.WR, 0, Position.WR, 1));
        }

        return options;
    }

    public static PreSnapChoice Shift(string side, double yards) => new(PreSnapType.Shift, side, yards);

    public static PreSnapChoice ReceiverShift(
        string side,
        Position position = Position.WR,
        int roleIndex = 0,
        double yards = .5) =>
        Shift(side, yards) with
        {
            ShiftPosition = position,
            ShiftRoleIndex = roleIndex,
            SetForOneSecond = true
        };

    public static PreSnapChoice Motion(
        string side,
        double yards,
        PreSnapMotionDirection direction = PreSnapMotionDirection.Lateral,
        PreSnapMotionKind kind = PreSnapMotionKind.ReturnToFormation) =>
        new(PreSnapType.Motion, side, yards) { MotionDirection = direction, MotionKind = kind };

    public static PreSnapChoice ReceiverMotion(
        string side,
        Position position = Position.WR,
        int roleIndex = 0,
        PreSnapMotionKind kind = PreSnapMotionKind.SetAcross) =>
        Motion(side, 2, PreSnapMotionDirection.Lateral, kind) with
        {
            MotionPosition = position,
            MotionRoleIndex = roleIndex,
            BackfieldPlayersInMotion = 1,
            AllOtherPlayersStationary = true,
            SetForOneSecond = true
        };

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
        if (choice.ShiftRoleIndex < 0 || choice.MotionRoleIndex < 0 || choice.SwapRoleIndex < 0) return false;
        if (choice.ShiftPosition is not null && choice.ShiftPosition != Position.WR) return false;
        if (choice.MotionPosition is not null &&
            choice.MotionPosition is not (Position.WR or Position.TE or Position.RB or Position.FB)) return false;
        if (choice.MotionKind == PreSnapMotionKind.SwapAlignments &&
            choice.SwapPosition is not (Position.WR or Position.TE or Position.RB or Position.FB)) return false;

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

    public static string Label(PreSnapChoice choice) => choice.Type switch
    {
        PreSnapType.None => "None",
        PreSnapType.Shift when choice.ShiftPosition == Position.WR =>
            $"WR shift {choice.Side.ToLowerInvariant()}",
        PreSnapType.Shift => "Shift " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.JetHandoff =>
            "Jet motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.OrbitAcross =>
            "Orbit motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SetAcross && choice.SnapLane is null && choice.MotionPosition == Position.WR =>
            $"WR motion {choice.Side.ToLowerInvariant()}",
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SetAcross =>
            "Set motion " + choice.Side.ToLowerInvariant(),
        PreSnapType.Motion when choice.MotionKind == PreSnapMotionKind.SwapAlignments => "WR swap",
        PreSnapType.Motion when choice.MotionPosition == Position.WR =>
            $"WR return motion {choice.Side.ToLowerInvariant()}",
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
        PreSnapType.Shift => "SHIFT",
        _ => ""
    };
}

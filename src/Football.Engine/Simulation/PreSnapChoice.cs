namespace DynastyManager.Core.Simulation;

public enum PreSnapType { None, Shift, Motion }
public enum PreSnapMotionDirection { Lateral, AwayFromGoalLine, TowardGoalLine }

/// <summary>Compact pre-snap intent used by presentation, call planning and recorded snap replay.</summary>
public sealed record PreSnapChoice(PreSnapType Type, string Side, double Yards)
{
    public PreSnapMotionDirection MotionDirection { get; init; } = PreSnapMotionDirection.Lateral;
    public int BackfieldPlayersInMotion { get; init; } = 1;
    public bool AllOtherPlayersStationary { get; init; } = true;
    public bool FollowedShift { get; init; }
    public bool SetForOneSecond { get; init; } = true;
    public bool StartedOnLineOfScrimmage { get; init; }
    public bool BecameBackAndStopped { get; init; }

    public bool Valid => PreSnapCatalog.IsValid(this);
    public bool HasAlignmentAction => Type is not PreSnapType.None;
}

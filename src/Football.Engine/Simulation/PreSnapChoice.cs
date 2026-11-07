using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public enum PreSnapType { None, Shift, Motion }
public enum PreSnapMotionDirection { Lateral, AwayFromGoalLine, TowardGoalLine }
public enum PreSnapMotionKind
{
    ReturnToFormation,
    SetAcross,
    OrbitAcross,
    JetHandoff,
    SwapAlignments
}

/// <summary>Compact pre-snap intent used by presentation, call planning and recorded snap replay.</summary>
public sealed record PreSnapChoice(PreSnapType Type, string Side, double Yards)
{
    public PreSnapMotionDirection MotionDirection { get; init; } = PreSnapMotionDirection.Lateral;
    public PreSnapMotionKind MotionKind { get; init; } = PreSnapMotionKind.ReturnToFormation;
    public Position? MotionPosition { get; init; }
    public int MotionRoleIndex { get; init; }
    public Position? SwapPosition { get; init; }
    public int SwapRoleIndex { get; init; } = 1;
    public double? SnapLane { get; init; }
    public double SnapDepth { get; init; } = -1.5;
    public bool MotionManIsBallCarrier { get; init; }
    public bool MotionLiveAtSnap { get; init; }
    public int BackfieldPlayersInMotion { get; init; } = 1;
    public bool AllOtherPlayersStationary { get; init; } = true;
    public bool FollowedShift { get; init; }
    public bool SetForOneSecond { get; init; } = true;
    public bool StartedOnLineOfScrimmage { get; init; }
    public bool BecameBackAndStopped { get; init; }

    public bool Valid => PreSnapCatalog.IsValid(this);
    public bool HasAlignmentAction => Type is not PreSnapType.None;
}
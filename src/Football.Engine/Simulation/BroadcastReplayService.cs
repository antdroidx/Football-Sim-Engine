namespace DynastyManager.Core.Simulation;

public enum ReplayAngle { Broadcast, FollowBall, ReverseFollow }

/// <summary>Presentation-only replay framing; league engines never consume this for simulation.</summary>
public static class BroadcastReplayService
{
    public static IReadOnlyList<ReplayAngle> Angles { get; } =
    [
        ReplayAngle.Broadcast,
        ReplayAngle.FollowBall,
        ReplayAngle.ReverseFollow
    ];

    public static ReplayAngle AngleFor(string? selected)
    {
        if (Enum.TryParse<ReplayAngle>(selected, ignoreCase: true, out var angle) &&
            Enum.IsDefined(angle))
            return angle;

        return ReplayAngle.Broadcast;
    }
}

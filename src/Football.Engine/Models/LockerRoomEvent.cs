namespace DynastyManager.Core.Models;

public enum LockerRoomEventKind
{
    CoachingChange,
    CaptainChange,
    RosterTurnover,
    Discipline,
    Morale,
    Chemistry
}

/// <summary>A written locker-room note built only from recorded facts.</summary>
public sealed record LockerRoomEvent
{
    public required string Id { get; init; }
    public required string TeamName { get; init; }
    public required string Headline { get; init; }
    public required string Detail { get; init; }
    public LockerRoomEventKind Kind { get; init; }
    public int SeasonYear { get; init; }
    public int Week { get; init; }
    public string? SubjectName { get; init; }
    public Guid? SubjectId { get; init; }
}
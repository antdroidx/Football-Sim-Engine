using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>A factual locker-room fact drawn from a host's own recorded state.</summary>
public sealed record LockerRoomSignal
{
    public required string TeamName { get; init; }
    public required LockerRoomEventKind Kind { get; init; }
    public string? Subject { get; init; }
    public Guid? SubjectId { get; init; }
    public int Value { get; init; }
    public string Detail { get; init; } = "";
    public int SeasonYear { get; init; }
    public int Week { get; init; }
}

/// <summary>
/// Writes locker-room notes from recorded facts only. It never invents quotes, results, injuries or statistics.
/// </summary>
public static class LockerRoomEventService
{
    public static IReadOnlyList<LockerRoomEvent> Build(IReadOnlyList<LockerRoomSignal> signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        return signals
            .Where(signal => !string.IsNullOrWhiteSpace(signal.TeamName))
            .Select(signal => new LockerRoomEvent
            {
                Id = StableId(signal),
                TeamName = signal.TeamName,
                Kind = signal.Kind,
                SeasonYear = signal.SeasonYear,
                Week = signal.Week,
                SubjectName = signal.Subject,
                SubjectId = signal.SubjectId,
                Headline = Headline(signal),
                Detail = Detail(signal)
            })
            .DistinctBy(item => item.Id)
            .OrderBy(item => item.TeamName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.SeasonYear)
            .ThenBy(item => item.Week)
            .ThenBy(item => item.Headline, StringComparer.Ordinal)
            .ToArray();
    }

    public static string Label(LockerRoomEventKind kind) => kind switch
    {
        LockerRoomEventKind.CoachingChange => "Coaching",
        LockerRoomEventKind.CaptainChange => "Leadership",
        LockerRoomEventKind.RosterTurnover => "Roster",
        LockerRoomEventKind.Discipline => "Discipline",
        LockerRoomEventKind.Morale => "Morale",
        _ => "Cohesion"
    };

    private static string Headline(LockerRoomSignal signal) => signal.Kind switch
    {
        LockerRoomEventKind.CoachingChange => "New head coach",
        LockerRoomEventKind.CaptainChange => "Leadership change",
        LockerRoomEventKind.RosterTurnover => "Roster turnover",
        LockerRoomEventKind.Discipline => "Discipline",
        LockerRoomEventKind.Morale => "Room temperature",
        _ => "Cohesion"
    };

    private static string Detail(LockerRoomSignal signal) => signal.Kind switch
    {
        LockerRoomEventKind.CoachingChange =>
            string.IsNullOrWhiteSpace(signal.Subject)
                ? "The program hired a new head coach."
                : $"{signal.Subject} takes over the program.",
        LockerRoomEventKind.CaptainChange =>
            string.IsNullOrWhiteSpace(signal.Subject)
                ? "The captains' group changed."
                : $"{signal.Subject} is no longer a captain.",
        LockerRoomEventKind.RosterTurnover =>
            $"{signal.Value}% of the roster changed since last season.",
        LockerRoomEventKind.Discipline =>
            string.IsNullOrWhiteSpace(signal.Detail)
                ? $"{signal.Subject ?? "A player"} was disciplined."
                : signal.Detail,
        LockerRoomEventKind.Morale =>
            $"Team morale moved {signal.Value:+0;-0;0} points over the season.",
        _ =>
            $"Cohesion ended at {signal.Value}."
    };

    private static string StableId(LockerRoomSignal signal)
    {
        var seed = $"{signal.TeamName}|{signal.SeasonYear}|{signal.Week}|{signal.Kind}|{signal.Subject}|{signal.SubjectId}";
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(seed));
        return "locker/" + Convert.ToHexString(bytes.AsSpan(0, 8)).ToLowerInvariant();
    }
}
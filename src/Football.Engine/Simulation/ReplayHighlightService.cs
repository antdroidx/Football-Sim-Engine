using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public enum ReplayHighlightKind { Score, Turnover, BigPlay, Conversion }

/// <summary>A replayable moment taken from a recorded snap. Nothing here is re-simulated.</summary>
public sealed record ReplayHighlightCandidate
{
    public required string Id { get; init; }
    public required string Headline { get; init; }
    public required string Detail { get; init; }
    public required ReplayHighlightKind Kind { get; init; }
    public required GameSnapEvent Snap { get; init; }
    public int Quarter { get; init; }
    public int ClockSeconds { get; init; }
    public int Sequence { get; init; }
}

/// <summary>
/// Picks the moments worth replaying out of a game's recorded snaps. Every candidate is read from recorded
/// state, so a replay never reruns the simulation.
/// </summary>
public static class ReplayHighlightService
{
    public static IReadOnlyList<ReplayHighlightCandidate> Candidates(IReadOnlyList<GameSnapEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var candidates = new List<ReplayHighlightCandidate>();
        foreach (var snap in events)
        {
            var kind = Kind(snap);
            if (kind is null)
                continue;

            candidates.Add(new ReplayHighlightCandidate
            {
                Id = $"highlight/{snap.Sequence}",
                Kind = kind.Value,
                Headline = Headline(snap, kind.Value),
                Detail = Detail(snap, kind.Value),
                Snap = snap,
                Quarter = snap.Quarter,
                ClockSeconds = snap.ClockSeconds,
                Sequence = snap.Sequence
            });
        }

        return candidates
            .OrderBy(candidate => candidate.Sequence)
            .ToArray();
    }

    public static string Label(ReplayHighlightKind kind) => kind switch
    {
        ReplayHighlightKind.Score => "Score",
        ReplayHighlightKind.Turnover => "Turnover",
        ReplayHighlightKind.Conversion => "Conversion",
        _ => "Big play"
    };

    private static ReplayHighlightKind? Kind(GameSnapEvent snap)
    {
        if (snap.Outcome is SnapOutcome.Touchdown or SnapOutcome.FieldGoal or SnapOutcome.TwoPointConversion)
            return ReplayHighlightKind.Score;
        if (snap.Outcome is SnapOutcome.Interception or SnapOutcome.Fumble or SnapOutcome.Turnover)
            return ReplayHighlightKind.Turnover;
        if (snap.Down == 4 && snap.Yards > 0 && snap.Yards >= snap.Distance)
            return ReplayHighlightKind.Conversion;
        if (snap.Yards >= 20)
            return ReplayHighlightKind.BigPlay;
        return null;
    }

    private static string Headline(GameSnapEvent snap, ReplayHighlightKind kind) => kind switch
    {
        ReplayHighlightKind.Score => snap.Outcome == SnapOutcome.FieldGoal
            ? $"Field goal · {snap.ScoringTeamName}"
            : $"Touchdown · {snap.ScoringTeamName}",
        ReplayHighlightKind.Turnover => $"Turnover · {snap.DefenseTeamName}",
        ReplayHighlightKind.Conversion => $"4th down conversion · {snap.OffenseTeamName}",
        _ => $"{snap.Yards} yard gain · {snap.OffenseTeamName}"
    };

    private static string Detail(GameSnapEvent snap, ReplayHighlightKind kind)
    {
        if (!string.IsNullOrWhiteSpace(snap.Description))
            return snap.Description!;

        return kind switch
        {
            ReplayHighlightKind.Score => $"Scored from the {snap.StartYardLine}.",
            ReplayHighlightKind.Turnover => $"Ball lost near the {snap.StartYardLine}.",
            ReplayHighlightKind.Conversion => $"Converted from the {snap.StartYardLine} on {snap.Down} down.",
            _ => $"{snap.Yards} yards from the {snap.StartYardLine}."
        };
    }
}
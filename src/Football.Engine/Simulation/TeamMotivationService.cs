using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Turns recorded locker-room signals into durable, bounded team motivation.</summary>
public static class TeamMotivationService
{
    public const int Minimum = 25;
    public const int Maximum = 95;

    public static TeamMotivationRecord Advance(
        TeamMotivationRecord? prior,
        IReadOnlyList<LockerRoomSignal> signals,
        string teamName,
        int seasonYear)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var drivers = new List<string>();
        var change = 0.0;
        foreach (var signal in signals.Where(item =>
                     item.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase)))
        {
            switch (signal.Kind)
            {
                case LockerRoomEventKind.CoachingChange:
                    change -= 6;
                    drivers.Add("A new head coach reset expectations.");
                    break;
                case LockerRoomEventKind.CaptainChange:
                    change -= Math.Min(6, 2 * Math.Abs(signal.Value == 0 ? 1 : signal.Value));
                    drivers.Add("Leadership changes unsettled the room.");
                    break;
                case LockerRoomEventKind.RosterTurnover:
                    change -= Math.Min(16, signal.Value * .20);
                    drivers.Add("Roster turnover cost continuity.");
                    break;
                case LockerRoomEventKind.Discipline:
                    change -= Math.Min(18, Math.Max(4, signal.Value * 6));
                    drivers.Add("Discipline and contract friction cost confidence.");
                    break;
                case LockerRoomEventKind.Morale:
                    change += Math.Clamp(signal.Value / 5d, -10, 10);
                    drivers.Add(signal.Value >= 0 ? "Season morale lifted the room." : "Season morale drained the room.");
                    break;
                case LockerRoomEventKind.Chemistry:
                    change += Math.Clamp((signal.Value - 55) / 8d, -6, 6);
                    break;
            }
        }

        var baseline = prior?.Value ?? 60;
        var value = (int)Math.Round(Math.Clamp(baseline * .65 + (60 + change) * .35, Minimum, Maximum),
            MidpointRounding.AwayFromZero);
        return new TeamMotivationRecord
        {
            TeamId = StableId(teamName),
            TeamName = teamName,
            SeasonYear = seasonYear,
            Value = value,
            Momentum = prior is null || value == prior.Value
                ? CohesionMomentum.Stable
                : value > prior.Value ? CohesionMomentum.Rising : CohesionMomentum.Falling,
            Drivers = drivers.Distinct(StringComparer.Ordinal).ToArray()
        };
    }

    public static string Label(TeamMotivationRecord? record) => record is null
        ? "Motivation is not recorded yet."
        : $"Motivation {record.Value}/100 · {TeamChemistrySeasonService.MomentumLabel(record.Momentum)}";

    private static Guid StableId(string teamName)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes("motivation:" + teamName));
        return new Guid(bytes.AsSpan(0, 16));
    }
}

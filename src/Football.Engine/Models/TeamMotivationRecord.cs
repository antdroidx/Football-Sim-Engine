namespace DynastyManager.Core.Models;

/// <summary>Durable team motivation carried with the locker-room record.</summary>
public sealed record TeamMotivationRecord
{
    public Guid TeamId { get; init; }
    public required string TeamName { get; init; }
    public int SeasonYear { get; init; }
    public int Value { get; init; } = 60;
    public DynastyManager.Core.Simulation.CohesionMomentum Momentum { get; init; } =
        DynastyManager.Core.Simulation.CohesionMomentum.Stable;
    public string[] Drivers { get; init; } = [];
}

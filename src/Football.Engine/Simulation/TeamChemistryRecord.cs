namespace DynastyManager.Core.Simulation;

public enum CohesionMomentum { Falling, Rising, Stable }

/// <summary>Shared team-culture state. College and Pro rules decide how these values are earned and applied.</summary>
public sealed record TeamChemistryRecord
{
    public Guid TeamId { get; init; }
    public int SeasonYear { get; init; }
    public int Cohesion { get; init; } = 50;
    public int SchemeFamiliarity { get; init; } = 50;
    public CohesionMomentum Momentum { get; init; } = CohesionMomentum.Stable;
    public Guid[] Captains { get; init; } = [];
}

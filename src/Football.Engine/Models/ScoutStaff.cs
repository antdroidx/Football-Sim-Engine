namespace DynastyManager.Core.Models;

public sealed record ScoutStaff
{
    public ScoutAttributes? Attributes { get; init; }
    public Guid ScoutId { get; init; } = Guid.NewGuid();
    public required string FullName { get; init; }
    public int Age { get; init; }
    public int CareerYears { get; init; }
    public int TenureYears { get; init; }
    public int GrowthPotential { get; init; } = 3;
    public int TalentEvaluation { get; init; }
    public int PotentialEvaluation { get; init; }
    public int RegionalKnowledge { get; init; }
    public int WorkRate { get; init; }
}

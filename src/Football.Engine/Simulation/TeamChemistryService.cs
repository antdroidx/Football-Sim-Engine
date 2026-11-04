namespace DynastyManager.Core.Simulation;

public static class TeamChemistryService
{
    public static TeamChemistryRecord Initialize(string teamKey, int seasonYear) => new() { TeamId = StableId(teamKey, seasonYear) };

    public static TeamChemistryRecord WithCaptions(TeamChemistryRecord record, IReadOnlyList<string> captionValues)
    {
        var captains = new List<Guid>();
        foreach (var captionValue in captionValues)
            captains.Add(StableId(captionValue, record.SeasonYear));

        return record with { Captains = captains.Distinct().ToArray(), Momentum = CohesionMomentum.Rising };
    }

    private static Guid StableId(string seed, int seasonYear)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{seed}:{seasonYear}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}


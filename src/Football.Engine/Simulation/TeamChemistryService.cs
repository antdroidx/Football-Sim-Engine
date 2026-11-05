using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class TeamChemistryService
{
    public static TeamChemistryRecord Initialize(string teamKey, int seasonYear) =>
        new() { TeamId = StableId(teamKey, seasonYear), SeasonYear = seasonYear };

    public static TeamChemistryRecord WithCaptions(TeamChemistryRecord record,
        IReadOnlyList<string> captionValues)
    {
        var captains = new List<Guid>();
        foreach (var captionValue in captionValues)
            captains.Add(StableId(captionValue, record.SeasonYear));

        return record with { Captains = captains.Distinct().ToArray(), Momentum = CohesionMomentum.Rising };
    }

    public static TeamChemistryRecord Evaluate(
        string teamKey,
        int seasonYear,
        IReadOnlyList<DynastyPlayer> roster,
        TeamSimulationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(roster);
        ArgumentNullException.ThrowIfNull(profile);
        var captains = TeamLeadershipService.Captains(roster);
        var captainLeadership = captains
            .Select(id => roster.First(p => p.PlayerId == id))
            .Select(p => p.Skills?.Leadership ?? 60)
            .DefaultIfEmpty(60)
            .Average();
        var cohesion = Math.Clamp(
            55 +
            (captainLeadership - 65) * .20 +
            (profile.Morale - 60) * .10 +
            (profile.HeadCoachLeadership - 60) * .08,
            20,
            95);
        var scheme = Math.Clamp(
            50 +
            (profile.OffensiveCoordinatorScheme - 60) * .18 +
            (profile.DefensiveCoordinatorScheme - 60) * .10,
            20,
            95);
        return new()
        {
            TeamId = StableId(teamKey, seasonYear),
            SeasonYear = seasonYear,
            Cohesion = (int)Math.Round(cohesion, MidpointRounding.AwayFromZero),
            SchemeFamiliarity = (int)Math.Round(scheme, MidpointRounding.AwayFromZero),
            Momentum = cohesion >= 60 ? CohesionMomentum.Rising : CohesionMomentum.Stable,
            Captains = captains.ToArray()
        };
    }

    public static double GameShift(TeamSimulationProfile offense, TeamSimulationProfile defense) =>
        Math.Clamp(
            ((offense.Cohesion - defense.Cohesion) * .012) +
            ((offense.SchemeFamiliarity - defense.SchemeFamiliarity) * .006),
            -1.8,
            1.8);

    private static Guid StableId(string seed, int seasonYear)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{seed}:{seasonYear}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}

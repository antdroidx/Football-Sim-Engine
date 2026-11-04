using DynastyManager.Core.Models;
namespace ProFootball.Core;

/// <summary>
/// Reads a coach scheme label as an engine playbook for identity adaptation. The catalog labels stay the
/// source of truth in saves; this only interprets them, so unmappable or ambiguous labels yield no influence.
/// </summary>
public static class ProTeamPhilosophy
{
    public static OffensivePlaybook? ParseOffense(string? label)
    {
        if (string.IsNullOrWhiteSpace(label)) return null;
        var text = label.ToLowerInvariant();
        if (StartsWithIdentity(text, "multiple", "balanced", "physical", "erhardt")) return OffensivePlaybook.Balanced;
        var powerIndex = FirstIndex(text, "power", "run");
        var playIndex = FirstIndex(text, "play action", "play-action");
        var spreadIndex = text.IndexOf("spread", StringComparison.OrdinalIgnoreCase);
        if (powerIndex >= 0 && (playIndex < 0 || powerIndex < playIndex) && (spreadIndex < 0 || powerIndex < spreadIndex))
            return OffensivePlaybook.PowerRun;
        if (playIndex >= 0 && (spreadIndex < 0 || playIndex < spreadIndex)) return OffensivePlaybook.PlayAction;
        if (text.Contains("air raid") || text.Contains("air-raid")) return OffensivePlaybook.AirRaid;
        if (text.Contains("west coast") || text.Contains("west")) return OffensivePlaybook.PlayAction;
        if (spreadIndex >= 0) return OffensivePlaybook.Spread;
        return null;
    }

    public static DefensivePlaybook? ParseDefense(string? label)
    {
        if (string.IsNullOrWhiteSpace(label)) return null;
        var text = label.ToLowerInvariant();
        if (text.Contains("3-4") || text.Contains("three-four")) return DefensivePlaybook.ThreeFour;
        if (text.Contains("4-3") || text.Contains("four-three")) return DefensivePlaybook.FourThree;
        if (text.Contains("nickel")) return DefensivePlaybook.Nickel;
        return null;
    }

    /// <summary>
    /// Staff influence grows with tenure and leadership, capped so the saved game plan keeps its authority.
    /// </summary>
    public static double Influence(ProCoach? head)
    {
        if (head is null || head.Vacant) return 0;
        if (ParseOffense(head.PreferredOffense) is null && ParseDefense(head.PreferredDefense) is null) return 0;
        return Math.Clamp(.05 * head.TenureYears + (head.Leadership - 55) / 300d, .05, .40);
    }

    /// <summary>
    /// Deterministic personal philosophy for a newly hired head coach, biased by their stronger skills.
    /// </summary>
    public static (string OffenseLabel, string DefenseLabel) HireHeadProfile(ProCoach coach, Guid franchiseId)
    {
        var offense = ParseOffense(coach.PreferredOffense);
        var defense = ParseDefense(coach.PreferredDefense);
        if (offense is not null || defense is not null) return (coach.PreferredOffense, coach.PreferredDefense);
        var seed = ProStaffRandom.Create(franchiseId, 0, 0, coach.TeamId + coach.Name, "coach-philosophy");
        var attributes = coach.Coaching ?? new CoachingAttributes();
        var offenseLean = attributes.OffensePlaycalling - attributes.DefenseScheme;
        var offenseLabels = offenseLean >= 0
            ? new[] { "Spread motion", "West Coast play action", "Play-action spread", "Air raid" }
            : new[] { "Multiple play action", "Balanced play action", "Erhardt-Perkins multiple" };
        var defenseLabels = offenseLean <= 0
            ? new[] { "3-4 pressure", "4-3 quarters", "Multiple fronts" }
            : new[] { "Nickel coverage", "4-3 quarters", "3-4 multiple" };
        return (offenseLabels[seed % offenseLabels.Length], defenseLabels[seed / 7 % defenseLabels.Length]);
    }

    private static bool StartsWithIdentity(string text, params string[] markers) =>
        markers.Any(marker => text.StartsWith(marker, StringComparison.Ordinal));

    private static int FirstIndex(string text, params string[] markers)
    {
        int? best = null;
        foreach (var marker in markers)
        {
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0 && (best is null || index < best)) best = index;
        }
        return best ?? -1;
    }
}

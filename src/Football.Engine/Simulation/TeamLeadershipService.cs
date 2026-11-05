using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Derives deterministic team captains from recorded roster traits. League rules remain separate.</summary>
public static class TeamLeadershipService
{
    public static IReadOnlyList<Guid> Captains(IReadOnlyList<DynastyPlayer> roster, int maximum = 4)
    {
        if (maximum <= 0) return [];
        return roster
            .Where(p => p.PlayingPositionCode is not 0)
            .Select(p => (Player: p, Score: Score(p)))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Player.PlayerId)
            .Take(maximum)
            .Select(item => item.Player.PlayerId)
            .ToArray();
    }

    public static int Score(DynastyPlayer player)
    {
        var skills = player.Skills;
        var score = (skills?.Leadership ?? 60) * 2 + player.AwarenessRating;
        if (player.DepthChartOrder is > 0 and <= 2)
            score += 6;
        if (player.Position is Position.QB or Position.OL)
            score += 4;
        if (player.Position is Position.WR or Position.DE or Position.MLB)
            score += 2;
        return score + player.OverallRating / 5;
    }
}

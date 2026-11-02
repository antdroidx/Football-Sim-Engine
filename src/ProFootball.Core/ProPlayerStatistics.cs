using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

public static class ProPlayerStatistics
{
    public static IReadOnlyList<PlayerSeasonStatLine> Lines(ProFranchise state, bool playoffs, bool career)
    {
        return PlayerStatQuery.FromCompetitiveGames(
            state.Games.Concat(state.History.SelectMany(h=>h.Games))
                .Where(g=>g.Phase is ProPhase.RegularSeason or ProPhase.Playoffs)
                .Select(g=>new PlayerStatGame(g.Football,g.Phase==ProPhase.Playoffs?StatCompetition.Postseason:StatCompetition.RegularSeason)),
            new PlayerStatScope(SeasonYear: career ? null : state.Year,Competition:playoffs?StatCompetition.Postseason:StatCompetition.RegularSeason));
    }
    public static IReadOnlyList<PlayerSeasonStatLine> Totals(ProFranchise state,bool playoffs,bool career,string? team=null)
        => PlayerStatTotals.GroupByPlayer(PlayerStatQuery.Filter(Lines(state,playoffs,career), new PlayerStatScope(TeamName: team)),"Multiple teams");
}

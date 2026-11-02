using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

public static class ProPlayerStatistics
{
    public static IReadOnlyList<PlayerSeasonStatLine> Lines(ProFranchise state, bool playoffs, bool career)
    {
        var phase=playoffs?ProPhase.Playoffs:ProPhase.RegularSeason;
        return PlayerStatQuery.FromCompletedGames(
            state.Games.Concat(state.History.SelectMany(h=>h.Games))
                .Where(g=>g.Phase==phase).Select(g=>g.Football),
            new PlayerStatScope(SeasonYear: career ? null : state.Year));
    }
    public static IReadOnlyList<PlayerSeasonStatLine> Totals(ProFranchise state,bool playoffs,bool career,string? team=null)
        => PlayerStatTotals.GroupByPlayer(PlayerStatQuery.Filter(Lines(state,playoffs,career), new PlayerStatScope(TeamName: team)),"Multiple teams");
}

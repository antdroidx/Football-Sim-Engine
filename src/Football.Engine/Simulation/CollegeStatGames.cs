using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class CollegeStatGames
{
    // Historical aggregate rows cannot recover competition boundaries. Only tag
    // retained game records; callers can combine current and archived schedules.
    public static IEnumerable<PlayerStatGame> Tag(IEnumerable<ScheduledGame> games) => games.Select(game =>
        new PlayerStatGame(game, game.GameType switch
        {
            ScheduledGameType.ConferenceChampionship => StatCompetition.ConferenceChampionship,
            ScheduledGameType.CollegeFootballPlayoff or ScheduledGameType.Bowl => StatCompetition.Postseason,
            _ => StatCompetition.RegularSeason
        }));
}

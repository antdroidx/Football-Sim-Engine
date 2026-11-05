using DynastyManager.App;
using DynastyManager.Core.Models;
using ProFootball.Core;

namespace ProFootball.App;

/// <summary>Pro data/persistence adapter. All live UI and actions are owned by Football.Presentation.</summary>
public sealed class ProLivePage : GameCastLivePage
{
    public ProLivePage(GameDaySession session, ProLeague league, Func<Task> save, bool chooseTeamOnTakeover = false)
        : base(session, new GameCastLiveHost
        {
            Brand = name =>
            {
                var team = league.Teams.Single(t => t.Name == name);
                return new(team.Abbreviation, team.Primary, team.Secondary, team.City);
            },
            Logo = name => TeamBadgeView.Mark(league.Teams.Single(t => t.Name == name), false),
            FieldStyle = game => GameFieldStyle.From(game,
                league.Teams.Single(t => t.Name == game.Game.HomeTeamName) is { } home
                    ? new GameCastLiveBrand(home.Abbreviation, home.Primary, home.Secondary, home.City)
                    : new GameCastLiveBrand("", "#245B38", "#285D3D", ""),
                league.Teams.Single(t => t.Name == game.Game.AwayTeamName) is { } away
                    ? new GameCastLiveBrand(away.Abbreviation, away.Primary, away.Secondary, away.City)
                    : new GameCastLiveBrand("", "#245B38", "#285D3D", "")),
            Save = _ => save(), ChooseTeamOnTakeover = chooseTeamOnTakeover,
            ShowBoxScore = (navigation, box) => GameCastBoxScore.ShowAsync(navigation, box, ProGameCastPresentation.BoxStyle(league))
        }) { }
}

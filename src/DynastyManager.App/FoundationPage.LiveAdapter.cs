using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using DynastyManager.Data.Branding;
using DynastyManager.Data.Persistence;

namespace DynastyManager.App;

public sealed partial class FoundationPage
{
    // College only prepares a session and commits its result. The engine presentation owns all live interactions.
    private async Task OpenGameDayAsync(ScheduledGame game, GameDayMode mode, string? controlledTeamName = null,
        IReadOnlyList<GameSnapEvent>? replay = null, bool autoReplay = false)
    {
        if (replay is not null) { await OpenLegacyHighlightsAsync(game, mode, controlledTeamName, replay, autoReplay); return; }
        if (_currentDynasty is null || !GameDayEngine.IsAvailable(game, _currentDynasty.SeasonYear, _currentDynasty.Week, _currentDynasty.Phase)) return;
        var otherGame = !game.InvolvesTeam(_currentDynasty.UserTeamName);
        if (controlledTeamName is null && mode == GameDayMode.Play && otherGame)
        {
            controlledTeamName = await GameDialogs.ActionSheetAsync(this, "Choose team to control", "Cancel", null, game.AwayTeamName, game.HomeTeamName);
            if (controlledTeamName is null or "Cancel") return;
        }
        controlledTeamName ??= otherGame ? game.HomeTeamName : _currentDynasty.UserTeamName;
        _currentDynasty = PlayerRatingService.EnsureProfiles(_currentDynasty);
        var homeTeam = _teamsByName.GetValueOrDefault(game.HomeTeamName) ?? FcsLeagueService.Find(game.HomeTeamName);
        var awayTeam = _teamsByName.GetValueOrDefault(game.AwayTeamName) ?? FcsLeagueService.Find(game.AwayTeamName);
        if (homeTeam is null || awayTeam is null) return;
        var roster = _currentDynasty.ActiveRoster;
        if (game.IsFcsGame)
        {
            var external = _teamsByName.ContainsKey(homeTeam.Name) ? awayTeam : homeTeam;
            roster = roster.Concat(FcsLeagueService.CreateRoster(_currentDynasty, external)).ToArray();
        }
        var profiles = DynastyRosterSimulationProfileBuilder.Build(_currentDynasty with { ActiveRoster = roster }, new[] { homeTeam, awayTeam });
        var home = GamePlanService.Attach(_currentDynasty, homeTeam, profiles[game.HomeTeamName]);
        var away = GamePlanService.Attach(_currentDynasty, awayTeam, profiles[game.AwayTeamName]);
        var session = GameDayEngine.Start(game, home, away, roster, mode, _currentDynasty.DepthChartAssignments);
        session.ControlledTeamName = controlledTeamName;
        session.UserCoachRole = controlledTeamName == _currentDynasty.UserTeamName ? _currentDynasty.UserCoachRole : StaffRole.HeadCoach;
        if (mode == GameDayMode.Simulate) { await Task.Run(() => GameDayEngine.SimulateToEnd(session)); await CompleteGameDayAsync(session); return; }
        GameCastLiveBrand Brand(string name)
        {
            var brand = TeamBrandCatalog.Get(name);
            var team = name == homeTeam.Name ? homeTeam : awayTeam;
            return new(team.Abbreviation, brand.Primary, brand.Secondary, name);
        }
        await ShowCoinTossAsync(session, controlledTeamName, mode == GameDayMode.Play);
        await Navigation.PushModalAsync(new GameCastLivePage(session, new()
        {
            Brand = Brand, Save = CompleteGameDayAsync, ChooseTeamOnTakeover = otherGame,
            ShowBoxScore = (_, box) => ShowLiveBoxScoreAsync(box)
        }));
    }
}

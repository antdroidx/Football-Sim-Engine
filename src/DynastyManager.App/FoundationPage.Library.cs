using DynastyManager.Core.Models;
using DynastyManager.Data.Branding;
using DynastyManager.Data.Persistence;
using DynastyManager.Core.Seasons;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace DynastyManager.App;

public sealed partial class FoundationPage
{
    private readonly Label _libraryStatus = new() { FontSize = 12, TextColor = DynastyTheme.Muted };
    private readonly Label _newDynastyStatus = new() { FontSize = 12, TextColor = DynastyTheme.Muted };

    private View BuildLibrarySection()
    {
        var options = new Button { Text = "Settings", AutomationId = "startup-options" };
        options.Clicked += async (_, _) => await ShowStartupOptionsAsync();
        var create = new Button { Text = "New dynasty", BackgroundColor = DynastyTheme.Accent,
            TextColor = DynastyTheme.Background, HorizontalOptions = LayoutOptions.End };
        create.Clicked += (_, _) => ShowSection(AppSection.NewDynasty);
        var heading = new Grid { ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
        heading.Add(new Label { Text = "Your dynasties", FontSize = 16, FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center }, 0, 0);
        var topBar = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) } };
        topBar.Add(options, 0, 0); topBar.Add(create, 2, 0);
        return new VerticalStackLayout { Spacing = 12, MaximumWidthRequest = 720, HorizontalOptions = LayoutOptions.Center, Children =
        {
            new Image { Source = "dynasty_logo.png", HeightRequest = 160, Aspect = Aspect.AspectFit, AutomationId = "opening-logo" },
            new Label { Text = "College Football Dynasty Manager", FontSize = 22, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = VersionCaption, FontSize = 12, HorizontalTextAlignment = TextAlignment.Center, TextColor = DynastyTheme.Muted },
            topBar, heading,
            new Label { Text = "Load a saved career or start a new one.", FontSize = 12, TextColor = DynastyTheme.Muted },
            new ActionRow { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Children = { StartingLibraryButton(), RosterImportButton() } },
            _saveList, _libraryStatus
        } };
    }

    private View BuildNewDynastySection()
    {
        _newDynastyButton.BackgroundColor = DynastyTheme.Accent;
        _newDynastyButton.TextColor = DynastyTheme.Background;
        _newDynastyButton.FontAttributes = FontAttributes.Bold;
        _newDynastyButton.FontSize = 16;
        _newDynastyButton.CornerRadius = 12;
        _newDynastyButton.HeightRequest = 52;
        return SetupScreen.Column(
            SetupScreen.Hero("Start a dynasty", "Choose your league and school, then take the reins."),
            SetupScreen.Card("Your dynasty", "The name your program is remembered by.", SetupScreen.Field("Dynasty name", null, _dynastyNameEntry)),
            SetupScreen.Card("League", "The league structure and the players who fill the rosters.",
                SetupScreen.Field("League structure", null, _leaguePicker),
                SetupScreen.Field("Starting roster", "Import a saved roster, or generate one that keeps every program's strengths.", _startingRosterPicker),
                SetupScreen.Toggle("Generate new players", "Turn this off to start from the imported roster instead.", _generatePlayers),
                new Label { Text = "Reusable rosters and league setups can be managed from the main screen.", FontSize = 11, TextColor = DynastyTheme.Muted }),
            SetupScreen.Card("Your school", "The program you take over. Staff and recruiting preferences can be changed after starting.",
                SetupScreen.Field("School", null, _teamPicker), _newDynastyPreview),
            SetupScreen.Card("Career options", "How much of the program you are responsible for.",
                BuildNewCareerOptions()),
            SetupScreen.Footer(_newDynastyButton, _newDynastyStatus));
    }

    private void UpdateNewDynastyPreview()
    {
        if (_teamPicker.SelectedItem is not string school) return;
        var team = _newDynastyTeams.FirstOrDefault(t => t.Name == school);
        var brand = TeamBrandCatalog.Get(school);
        var badge = new Border
        {
            WidthRequest = 66, HeightRequest = 66, BackgroundColor = Color.FromArgb(brand.Primary),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Content = new Label { Text = school.Length > 0 ? school[..1] : "?", FontSize = 30, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(brand.Secondary), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        };
        _newDynastyPreview.Clear();
        _newDynastyPreview.Add(new HorizontalStackLayout { Spacing = 14, Children =
        {
            badge,
            new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children =
            {
                new Label { Text = school, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = DynastyTheme.Text },
                new Label { Text = team is null ? "" : $"{team.ConferenceName}{(string.IsNullOrWhiteSpace(team.DivisionName) ? "" : " · " + team.DivisionName)}",
                    FontSize = 12, TextColor = DynastyTheme.Muted }
            } }
        } });
    }

    private void UpdateShellContext()
    {
        var library = _activeSection is AppSection.Home or AppSection.NewDynasty;
        _screenTitle.IsVisible = _activeSection != AppSection.Home;
        _navigationBar.IsVisible = !library && _currentDynasty is not null;
        _libraryButton.IsVisible = _activeSection == AppSection.NewDynasty;
        _libraryButton.Text = _activeSection == AppSection.NewDynasty ? "‹ Cancel" : "‹ Exit Dynasty";
        _headerContext.IsVisible = !library && _currentDynasty is not null;
        _settingsButton.IsVisible = !library && _currentDynasty is not null && _activeSection != AppSection.Settings;
        if (_currentDynasty is not null)
            _headerContext.Text = $"{_currentDynasty.UserTeamName} · {_currentDynasty.SeasonYear} · {SeasonProgression.PeriodLabel(_currentDynasty.Phase, _currentDynasty.Week)}";
        if (_currentDynasty is not null)
            _advanceWeekButton.Text = SeasonProgression.AdvanceLabel(_currentDynasty.Phase, _currentDynasty.Week);
        _simulatePhaseButton.IsVisible=_currentDynasty?.Phase is SeasonPhase.RegularSeason or SeasonPhase.ConferenceChampionship or SeasonPhase.Postseason;
        _simulatePhaseButton.Text=_currentDynasty?.Phase==SeasonPhase.RegularSeason?"Simulate regular season":"Simulate postseason";
    }

    private async Task ReturnToLibraryAsync()
    {
        if (_isAdvancing || _isBatchSimulating || _advanceOverlay.IsVisible) return;
        try
        {
            if (_activeSection != AppSection.NewDynasty && _currentDynasty is not null)
            {
                await ShowAdvanceProgressAsync("Saving dynasty...", .5);
                await SaveCurrentAsync(SaveKind.AutosaveCurrent);
            }
            ShowSection(AppSection.Home);
        }
        catch (Exception ex) { await GameDialogs.AlertAsync(this, "Could not save dynasty", ex.Message, "Close"); }
        finally { _advanceOverlay.IsVisible = false; }
    }

    protected override bool OnBackButtonPressed()
    {
        if (_advanceOverlay.IsVisible || _isAdvancing) return true;
        if (_navigationExpanded && _managementRoot is { Width: < 600 })
        { _navigationExpanded = false; ArrangeManagementShell(); return true; }
        if (_activeSection == AppSection.Recruiting && _selectedRecruitId is not null)
        {
            _selectedRecruitId=null;_selectedRecruitSource=null;
            _recruitingDetailHost.Content=null;_recruitingDetailHost.IsVisible=false;return true;
        }
        if (_activeSection == AppSection.Roster && _selectedRosterPlayer is not null)
        { _selectedRosterPlayer=null;RenderCurrentDynasty();return true; }
        if (_activeSection == AppSection.Home) return base.OnBackButtonPressed();
        if (_activeSection != AppSection.NewDynasty && RestorePreviousSection()) return true;
        _ = ReturnToLibraryAsync();
        return true;
    }

    private async Task OpenSaveVersionsAsync(IReadOnlyList<DynastySaveInfo> saves)
    {
        var ordered = saves.OrderByDescending(save => save.UpdatedUtc).ToArray();
        var labels = ordered.Select((save, index) => $"{index + 1}. {save.SeasonYear} / {FormatWeek(save)} / {RollingAutosavePolicy.GetDisplayName(save.Kind)} / {save.UpdatedUtc.ToLocalTime():g}").ToArray();
        var selected = await GameDialogs.ActionSheetAsync(this, "Saved versions", "Cancel", null, labels);
        var index = Array.IndexOf(labels, selected);
        if (index < 0) return;
        var action = await GameDialogs.ActionSheetAsync(this, labels[index], "Cancel", null, "Load save", "Delete save");
        if (action == "Load save") await LoadSaveAsync(ordered[index].SaveId);
        else if (action == "Delete save") await DeleteSaveAsync(ordered[index].SaveId);
    }
}
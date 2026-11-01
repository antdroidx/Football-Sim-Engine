using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace DynastyManager.App;

/// <summary>One live screen for every league. Hosts supply branding, a session and persistence only.</summary>
public class GameCastLivePage : ContentPage
{
    readonly GameDaySession session;
    readonly GameCastLiveHost host;
    readonly GameDayLiveControl control;
    readonly GameDayCompletionService completion = new();
    readonly CancellationTokenSource lifetime = new();
    readonly Label homeScore = Text(24, true), awayScore = Text(24, true), homeTimeouts = Text(11), awayTimeouts = Text(11);
    readonly Label situation = Text(13, true), ownership = Text(11), state = Text(12, true), commentary = Text(13), selection = Text(12, true);
    readonly Dictionary<string, Label> teamNames = new();
    readonly Grid header = new();
    readonly Grid footer = new();
    readonly ContentView toolbar = new();
    readonly ActionRow standardToolbar = new();
    readonly HorizontalStackLayout compactToolbar = new() { Spacing = 8, VerticalOptions = LayoutOptions.Center };
    bool? compactFooter;
    readonly VerticalStackLayout headerInfo = new() { Spacing = 2, VerticalOptions = LayoutOptions.Center };
    readonly Label callHeading = TextHeading("NEXT CALL");
    readonly Button pause, call, choose, team, options, takeOver, replaySkip;
    readonly Grid field = new() { AutomationId = "live-field", IsClippedToBounds = true };
    readonly AbsoluteLayout players = new();
    readonly GameFieldDrawable art;
    readonly GraphicsView turf;
    readonly Border highlight, fieldFrame;
    readonly Label highlightText;
    readonly Grid quick = new() { ColumnSpacing = 6, AutomationId = "live-quick-calls" };
    readonly Grid body = new() { RowSpacing = 6, ColumnSpacing = 8 };
    readonly Border sheet;
    readonly Border recap;
    readonly VerticalStackLayout recent = new() { Spacing = 12 };
    readonly Grid picker = new() { IsVisible = false, Padding = 8, BackgroundColor = Color.FromArgb("#B0000000") };
    readonly Dictionary<FootballSpriteView, DynastyPlayer?> spritePlayers = new();
    readonly Dictionary<Border, (DynastyPlayer? Player, Color Primary, Color Secondary, bool Away)> markerHosts = new();
    readonly HashSet<Guid> ballOwners = new();
    Task snapFinished = Task.CompletedTask;
    Action? closePicker;
    string? offense, defense;
    GameSnapEvent? last;
    bool started, menuOpen, closing, replaying, finishing;
    double speed, replaySpeed;
    int camera;
    bool autoReplay, showNames;
    bool landscape;
    bool? arrangedLandscape;
    int presentedEvents;

    static Label Text(int size, bool bold = false) => new() { FontSize = size, TextColor = DynastyTheme.Text,
        FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, VerticalTextAlignment = TextAlignment.Center };
    string Key(string name) => host.PreferencesPrefix + "-" + name;
    GameCastLiveBrand Brand(string name) => host.Brand(name);
    Color Primary(string name) => Color.FromArgb(Brand(name).Primary);
    Color Secondary(string name) => Color.FromArgb(Brand(name).Secondary);
    TeamGamePlan Plan => (session.ControlledTeamName == session.Game.HomeTeamName ? session.HomeProfile : session.AwayProfile).GamePlan
        ?? new() { TeamName = session.ControlledTeamName ?? session.Game.HomeTeamName };

    public GameCastLivePage(GameDaySession session, GameCastLiveHost host)
    {
        this.session = session; this.host = host; control = new(session); presentedEvents = session.Events.Count;
        BackgroundColor = DynastyTheme.Background; AutomationId = "live-game-page";
        speed = Math.Clamp(Preferences.Get(Key("speed"), 1d), .5, 2);
        replaySpeed = Math.Clamp(Preferences.Get(Key("replay-speed"), 1d), .5, 2);
        camera = Math.Clamp(Preferences.Get(Key("camera"), 0), 0, 2);
        autoReplay = Preferences.Get(Key("auto-replay"), true); showNames = Preferences.Get(Key("names"), true);
        var home = Brand(session.Game.HomeTeamName);
        art = new() { HomeName = (home.FieldName ?? home.Abbreviation).ToUpperInvariant(), HomeInitial = home.Abbreviation,
            HomeColor = Primary(session.Game.HomeTeamName), HomeSecondaryColor = Secondary(session.Game.HomeTeamName),
            HashMarkFraction = session.Rules.HashMarkFraction, Weather = session.Weather };
        turf = new() { Drawable = art, InputTransparent = true };
        highlight = GameCastHighlight.Create(out highlightText);
        field.Add(turf); field.Add(players); field.Add(highlight);
        fieldFrame = new() { Content = field, Padding = 4, Stroke = DynastyTheme.Muted, StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 }, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        View TeamScore(string name, Label score, Label timeouts)
        {
            var brand = Brand(name);
            var nameLabel = Text(13, true); nameLabel.Text = brand.Abbreviation; teamNames[name] = nameLabel;
            var badge = new Border { BackgroundColor = Primary(name), Stroke = Secondary(name), Padding = 4,
                Content = new Label { Text = brand.Abbreviation, TextColor = Primary(name).Red * .2126 + Primary(name).Green * .7152 + Primary(name).Blue * .0722 > .55 ? Colors.Black : Colors.White, FontSize = 10, FontAttributes = FontAttributes.Bold } };
            SemanticProperties.SetDescription(badge, name + " team badge");
            return GameCastScoreboard.Team(badge, nameLabel, score, timeouts);
        }
        var scoreboard = GameCastScoreboard.Create((Grid)TeamScore(session.Game.AwayTeamName, awayScore, awayTimeouts),
            (Grid)TeamScore(session.Game.HomeTeamName, homeScore, homeTimeouts));
        situation.HorizontalTextAlignment = TextAlignment.Center;
        ownership.AutomationId = "live-ownership"; state.AutomationId = "live-state";
        selection.AutomationId = "live-selection"; situation.AutomationId = "live-situation";
        pause = Action("Pause", () => { if (control.IsPaused) control.Resume(); else control.Pause(); Update(); return Task.CompletedTask; }, "live-pause", false);
        call = Action("Call play", () => Snap(true), "live-call", false);
        call.BackgroundColor = DynastyTheme.Accent; call.TextColor = DynastyTheme.Background; call.FontAttributes = FontAttributes.Bold;
        choose = Action("Playbook", Playbook, "live-playbook");
        team = Action("Team", TeamMenu, "live-team");
        options = Action("Options", Options, "live-options");
        takeOver = Action("Take over", TakeOver, "live-takeover");
        replaySkip = Action("Skip replay", () => { players.AbortAnimation("snap-movement"); return Task.CompletedTask; }, "live-skip-replay", false);
        var sheetBody = new Grid { RowSpacing = 6, RowDefinitions = new()
            { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) } };
        sheetBody.Add(callHeading, 0, 0); sheetBody.Add(selection, 0, 1);
        sheetBody.Add(new ScrollView { Content = quick }, 0, 2);
        sheetBody.Add(new ActionRow { Children = { choose, call } }, 0, 3);
        sheet = new() { Content = sheetBody, BackgroundColor = DynastyTheme.Surface,
            StrokeThickness = 0, Padding = 8, AutomationId = "live-call-sheet" };
        recap = new() { Content = new ScrollView { Content = recent }, Padding = 12, StrokeThickness = 0,
            BackgroundColor = DynastyTheme.Surface, AutomationId = "live-recent-plays" };
        body.Add(fieldFrame); body.Add(sheet); body.Add(recap);
        standardToolbar.Add(pause); standardToolbar.Add(takeOver); standardToolbar.Add(team); standardToolbar.Add(options); standardToolbar.Add(replaySkip);
        toolbar.Content = standardToolbar;
        footer.Add(commentary); footer.Add(toolbar);
        var status = new Grid { ColumnSpacing = 8, ColumnDefinitions = new() { new(GridLength.Star), new(GridLength.Auto) } };
        status.Add(ownership, 0, 0); status.Add(state, 1, 0);
        headerInfo.Add(situation); headerInfo.Add(status);
        header.Add(scoreboard); header.Add(headerInfo);
        commentary.MaxLines = 2; commentary.LineBreakMode = LineBreakMode.TailTruncation;
        var layout = new Grid { Padding = 6, RowSpacing = 4, RowDefinitions = new()
            { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) } };
        layout.Add(header, 0, 0); layout.Add(body, 0, 1); layout.Add(footer, 0, 2);
        var root = new Grid(); root.Add(layout); root.Add(picker); Content = root;
        SizeChanged += (_, _) => Arrange(); body.SizeChanged += (_, _) => FitField(); field.SizeChanged += (_, _) => FrameCamera();
        Loaded += (_, _) => { if (started) return; started = true; art.HomeLogo = host.Logo(session.Game.HomeTeamName); Update(); _ = Pump(); };
        Disappearing += (_, _) => Dispatcher.Dispatch(() =>
        {
            // Modal menus cover this page too; only stop after the actual viewer leaves navigation.
            if (!Navigation.ModalStack.Contains(this)) { lifetime.Cancel(); players.AbortAnimation("snap-movement"); }
        });
        Arrange(); Update();
    }

    static Label TextHeading(string value) => new() { Text = value, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = DynastyTheme.Accent };
    Button Action(string label, Func<Task> action, string id, bool suspend = true)
    {
        var button = new Button { Text = label, AutomationId = id, FontSize = 12, Padding = new Thickness(12, 6), MinimumHeightRequest = 44, CornerRadius = 9 };
        button.Clicked += async (_, _) => { if (suspend) await MenuAction(action); else await Safe(action); };
        return button;
    }
    async Task Safe(Func<Task> action)
    {
        try { await action(); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception error) { control.Pause(); Update(); await GameDialogs.AlertAsync(this, "Game paused", error.Message, "OK"); }
    }
    async Task MenuAction(Func<Task> action)
    {
        if (menuOpen || closing || finishing || lifetime.IsCancellationRequested) return;
        menuOpen = true;
        using var hold = control.Suspend();
        try { Update(); await snapFinished; await Safe(action); }
        finally { menuOpen = false; hold.Dispose(); Update(); }
    }
    async Task Pump()
    {
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                if (control.ShouldAdvance) { await Safe(() => Snap(false)); await Task.Delay(1100, lifetime.Token); }
                else await Task.Delay(100, lifetime.Token);
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
    }
    async Task Snap(bool manual)
    {
        if (!control.TryBegin(manual)) return;
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); snapFinished = finished.Task;
        try
        {
            Update(); last = control.Step(offense, defense); offense = defense = null;
            var atBreak = await GameCastSnapPresentation.PresentAsync(new()
            {
                Session = session, Snap = last, Highlight = highlight, HighlightText = highlightText,
                TeamColor = Primary, Animate = Animate, Replay = Replay, AutoReplay = autoReplay && !menuOpen
            });
            commentary.Text = GameDayCommentaryService.Describe(last, session.PlayersById);
            presentedEvents = session.Events.Count;
            if (atBreak) { control.Pause(); commentary.Text += " · Break: Team for coach talk or substitutions."; }
            if (session.IsComplete) await Save();
        }
        finally { control.End(); finished.TrySetResult(); Update(); }
    }
    Task Save() => completion.EnsureSavedAsync(() => host.Save(session));

    void Arrange()
    {
        landscape = Width > Height && Width >= 640;
        if (arrangedLandscape != landscape)
        {
            arrangedLandscape = landscape;
            header.RowDefinitions.Clear(); header.ColumnDefinitions.Clear();
            header.ColumnDefinitions.Add(new(GridLength.Star)); header.RowDefinitions.Add(new(GridLength.Auto));
            if (landscape) header.ColumnDefinitions.Add(new(GridLength.Star)); else header.RowDefinitions.Add(new(GridLength.Auto));
            Grid.SetColumn(headerInfo, landscape ? 1 : 0); Grid.SetRow(headerInfo, landscape ? 0 : 1);
            body.RowDefinitions.Clear(); body.ColumnDefinitions.Clear();
            body.RowDefinitions.Add(new(!landscape ? GridLength.Auto : GridLength.Star)); body.ColumnDefinitions.Add(new(GridLength.Star));
            if (landscape) body.ColumnDefinitions.Add(new(new GridLength(0))); else body.RowDefinitions.Add(new(GridLength.Star));
        }
        var fieldOnly = landscape && !sheet.IsVisible;
        if (landscape) body.ColumnDefinitions[1].Width = fieldOnly ? new GridLength(0) : new GridLength(SidebarWidth);
        if (compactFooter != fieldOnly)
        {
            compactFooter = fieldOnly;
            toolbar.Content = null;
            standardToolbar.Children.Clear(); compactToolbar.Children.Clear();
            Layout target = fieldOnly ? compactToolbar : standardToolbar;
            foreach (var button in new[] { pause, takeOver, team, options, replaySkip }) target.Add(button);
            toolbar.Content = target;
            footer.RowDefinitions.Clear(); footer.ColumnDefinitions.Clear();
            footer.RowDefinitions.Add(new(GridLength.Auto)); footer.ColumnDefinitions.Add(new(GridLength.Star));
            if (fieldOnly) footer.ColumnDefinitions.Add(new(GridLength.Auto)); else footer.RowDefinitions.Add(new(GridLength.Auto));
            Grid.SetColumn(toolbar, fieldOnly ? 1 : 0); Grid.SetRow(toolbar, fieldOnly ? 0 : 1);
        }
        var controlsVisible = !fieldOnly || control.IsPaused && !control.IsBusy;
        team.IsVisible = options.IsVisible = controlsVisible;
        takeOver.IsVisible = controlsVisible && !session.IsComplete && !control.Manual(false) && !control.Manual(true);
        callHeading.IsVisible = selection.IsVisible = !landscape;
        commentary.MaxLines = landscape ? 1 : 2;
        Grid.SetColumn(sheet, landscape ? 1 : 0); Grid.SetRow(sheet, landscape ? 0 : 1);
        Grid.SetColumn(recap, landscape ? 1 : 0); Grid.SetRow(recap, landscape ? 0 : 1);
        recap.IsVisible = !landscape && !sheet.IsVisible;
        fieldFrame.VerticalOptions = fieldOnly ? LayoutOptions.Fill : landscape ? LayoutOptions.Center : LayoutOptions.Start;
        fieldFrame.HorizontalOptions = fieldOnly ? LayoutOptions.Fill : LayoutOptions.Center;
        sheet.MaximumHeightRequest = double.PositiveInfinity;
        if (sheet.IsVisible && control.NeedsCall && !control.IsBusy && !menuOpen) UpdateCalls();
        FitField();
    }
    double SidebarWidth => Math.Clamp(Width * .34, 260, 340);
    double FieldRatio => 120 / session.Rules.FieldWidthYards;
    void FitField()
    {
        if (body.Width <= 0 || body.Height <= 0) return;
        if (landscape && !sheet.IsVisible)
        {
            // Live action uses the entire available viewport; no recap/sidebar reserves field space.
            fieldFrame.WidthRequest = fieldFrame.HeightRequest = -1;
            return;
        }
        var width = Math.Max(80, body.Width - (landscape ? SidebarWidth + 8 : 0));
        var height = !landscape ? width / FieldRatio : Math.Max(70, body.Height);
        var fitWidth = Math.Min(width, height * FieldRatio);
        fieldFrame.WidthRequest = fitWidth; fieldFrame.HeightRequest = fitWidth / FieldRatio;
    }
    void Update()
    {
        if (lifetime.IsCancellationRequested) return;
        awayScore.Text = session.AwayScore.ToString(); homeScore.Text = session.HomeScore.ToString();
        foreach (var (name, label) in teamNames) label.Text = (!session.IsComplete && session.PossessionTeamName == name ? "● " : "") + Brand(name).Abbreviation;
        string Dots(int count) => new string('●', count) + new string('○', Math.Max(0, 3 - count));
        awayTimeouts.Text = Dots(session.AwayTimeouts); homeTimeouts.Text = Dots(session.HomeTimeouts);
        var clock = session.OvertimeRound > 0 && !session.Rules.TimedOvertime ? $"OT {session.OvertimeRound} · untimed"
            : $"{(session.Quarter > 4 ? "OT" : "Q" + session.Quarter)} {session.ClockSeconds / 60}:{session.ClockSeconds % 60:00}";
        situation.Text = session.IsComplete ? completion.IsSaved ? "FINAL · Saved" : "FINAL · Save pending"
            : clock + " · " + (session.KickoffPending ? "Kickoff" : session.TryPending ? "Conversion" : $"{session.Down} & {session.Distance} · {GameDayEngine.BallOn(session, name => Brand(name).Abbreviation)}");
        ownership.Text = $"{(control.HasTeam ? Brand(session.ControlledTeamName!).Abbreviation + " · " : "")}Offense: {control.Ownership(false)}  ·  Defense: {control.Ownership(true)}";
        state.Text = replaying ? "Replay" : control.Status;
        SemanticProperties.SetDescription(state, state.Text);
        pause.Text = control.IsPaused ? "Resume" : "Pause"; pause.IsVisible = !session.IsComplete;
        pause.IsEnabled = !menuOpen && !finishing && !closing;
        takeOver.IsVisible = !session.IsComplete && (!control.Manual(false) && !control.Manual(true));
        takeOver.IsEnabled = !menuOpen && !finishing;
        team.IsEnabled = options.IsEnabled = !menuOpen && !finishing && !closing;
        replaySkip.IsVisible = replaying;
        var wasVisible = sheet.IsVisible;
        sheet.IsVisible = control.NeedsCall && !control.IsBusy && !menuOpen && !closing && !finishing;
        if (sheet.IsVisible)
        {
            UpdateCalls();
            call.Text = control.CanCall(true) ? "Call defense" : session.TryPending ? "Attempt conversion" : "Call play";
            if (!wasVisible)
            {
                sheet.Opacity = 0;
                new Animation(p => sheet.Opacity = p, 0, 1).Commit(sheet, "call-sheet-enter", 16, 160);
            }
        }
        else { sheet.AbortAnimation("call-sheet-enter"); sheet.Opacity = 1; }
        recent.Clear(); recent.Add(TextHeading(control.IsBusy ? "LIVE PLAY" : session.IsComplete ? "GAME COMPLETE" : "RECENT PLAYS"));
        var hint = Text(14, true);
        hint.Text = control.IsBusy ? "Play in progress · the call sheet returns at your next decision."
            : session.IsComplete ? completion.IsSaved ? "Final result saved." : "Result pending · Options → Retry save."
            : control.IsPaused ? "Game paused · Resume continues CPU play." : "CPU is calling this unit · Pause or take over at any time.";
        recent.Add(hint);
        foreach (var snap in session.Events.Take(presentedEvents).TakeLast(4).Reverse())
        {
            var line = Text(12); line.Text = $"Q{snap.Quarter} {snap.ClockSeconds / 60}:{snap.ClockSeconds % 60:00} · {GameDayCommentaryService.Describe(snap, session.PlayersById)}";
            recent.Add(line);
        }
        Arrange();
        if (!control.IsBusy && !replaying && !finishing) RenderHuddles();
    }

    List<GameDayCallChoice> Choices()
    {
        if (control.CanCall(true)) return PlayEditorService.DefensiveLibrary(Plan).Select(p => new GameDayCallChoice(p.Id, p.Name,
            GamePlanService.Label(p.Formation), GamePlanService.Label(p.Approach), new PlayArtDrawable(p.Id, p.Name, false, p.Approach.ToString(), defensiveDefinition: p))).ToList();
        var choices = session.TryPending ? new List<GameDayCallChoice>() : PlayEditorService.OffensiveLibrary(Plan).Select(p => new GameDayCallChoice(p.Id, p.Name,
            GamePlanService.Label(p.Formation), p.Concept is OffensiveConcept.InsideRun or OffensiveConcept.OutsideRun ? "RUN" : "PASS", new PlayArtDrawable(p.Id, p.Name, true, p.Concept.ToString(), p))).ToList();
        foreach (var id in control.SpecialCalls())
        {
            var name = id switch { "special-punt" => "Punt", "special-field-goal" => "Field goal", "special-kneel" => "Kneel", "special-spike" => "Spike", "special-extra-point" => "Extra point", _ => "Two-point conversion" };
            choices.Add(new(id, name, "Special plays", "SITUATIONAL", new PlayArtDrawable(id, name, true, "Kick")));
        }
        return choices;
    }
    string Recommended() => control.CanCall(true) ? GamePlanService.RecommendDefensivePlay(Plan, session.Down, session.Distance).Id
        : session.TryPending ? GameDayEngine.ShouldAttemptTwo(session) ? "special-two-point" : "special-extra-point"
        : GameDayRecommendationService.Recommend(session, Plan).PlayId;
    void UpdateCalls()
    {
        var choices = Choices(); if (choices.Count == 0) return;
        var defending = control.CanCall(true);
        var id = defending ? defense : offense;
        var selected = choices.FirstOrDefault(p => p.Id == id) ?? choices.FirstOrDefault(p => p.Id == Recommended()) ?? choices[0];
        if (defending) defense = selected.Id; else offense = selected.Id;
        selection.Text = selected.Name + " · " + Brand(session.ControlledTeamName!).Abbreviation + (defending ? " defense" : " offense");
        quick.Clear(); quick.ColumnDefinitions.Clear(); quick.RowDefinitions.Clear();
        for (var column = 0; column < (landscape ? 1 : 3); column++) quick.ColumnDefinitions.Add(new(GridLength.Star));
        var alternates = choices.Where(p => p.Id != selected.Id).OrderBy(p => p.Type == selected.Type ? 1 : 0);
        var shortList = new[] { selected }.Concat(alternates.Take(2)).ToArray();
        for (var i = 0; i < shortList.Length; i++)
        {
            var choice = shortList[i];
            var card = GameDayCallPicker.Card(choice.Name, choice.Formation, choice.Id == selected.Id ? "SELECTED · " + choice.Type : choice.Type,
                choice.Art, () => { if (!control.NeedsCall || control.IsBusy || menuOpen) return; if (defending) defense = choice.Id; else offense = choice.Id; Update(); }, true, !landscape);
            if (!landscape && card.Content is VerticalStackLayout content && content.Children.FirstOrDefault() is GraphicsView diagram)
                diagram.HeightRequest = Math.Clamp(Height * .12, 58, 120);
            if (landscape && card.Content is Grid row)
            {
                if (row.Children[0] is GraphicsView compactDiagram) compactDiagram.HeightRequest = 42;
                if (row.Children[1] is VerticalStackLayout details && details.Children.LastOrDefault() is View formation) formation.IsVisible = false;
            }
            card.VerticalOptions = LayoutOptions.Start;
            card.AutomationId = "live-quick-call-" + i; card.Stroke = DynastyTheme.Accent; card.StrokeThickness = i == 0 ? 2 : 0;
            if (landscape) quick.RowDefinitions.Add(new(GridLength.Auto));
            quick.Add(card, landscape ? 0 : i, landscape ? i : 0);
        }
    }
    async Task Playbook()
    {
        if (!control.NeedsCall) return;
        var choices = Choices(); var defending = control.CanCall(true);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        closePicker = () => { picker.IsVisible = false; picker.Clear(); closePicker = null; closed.TrySetResult(); };
        picker.Clear(); picker.Add(new GameDayCallPicker(defending ? "Choose defense" : session.TryPending ? "Choose conversion" : "Choose play",
            choices, (defending ? defense : offense) ?? "", id => { if (defending) defense = id; else offense = id; closePicker?.Invoke(); }, () => closePicker?.Invoke()));
        picker.IsVisible = true; await closed.Task;
    }

    async Task Options()
    {
        var actions = new List<GameCastLiveAction>();
        if (!session.IsComplete) actions.Add(new("Play calling", PlayCalling));
        actions.Add(new("Box score", () => host.ShowBoxScore(Navigation, GameDayEngine.BoxScore(session))));
        if (last is not null) actions.Add(new("Replay last play", () => Replay(last)));
        actions.Add(new("Viewing options", ViewingOptions));
        actions.Add(new("Weather", () => GameDialogs.AlertAsync(this, "Game conditions", session.Weather.Summary, "Close")));
        if (!session.IsComplete) actions.Add(new("Finish game", Finish));
        else if (!completion.IsSaved) actions.Add(new("Retry save", Save));
        actions.Add(new("Exit game", Close));
        await GameCastLiveMenu.InvokeAsync(this, "Game options", actions);
    }
    async Task PlayCalling()
    {
        var choices = new List<string>();
        choices.Add(control.CallerLabel(false));
        choices.Add(control.CallerLabel(true));
        choices.Add("Watch both sides"); choices.Add("Take over");
        var selected = await GameDialogs.ActionSheetAsync(this, "Play calling · changes apply before the next snap", "Cancel", null, choices.ToArray());
        if (selected == "Watch both sides") { control.Watch(); offense = defense = null; }
        else if (selected == "Take over") await TakeOver();
        else if (selected?.StartsWith("Offense:", StringComparison.Ordinal) == true || selected?.StartsWith("Defense:", StringComparison.Ordinal) == true)
        {
            var defending = selected.StartsWith("Defense:", StringComparison.Ordinal);
            if (control.Restriction(defending) is { } restriction)
            {
                await GameDialogs.AlertAsync(this, defending ? "Defense play calling" : "Offense play calling", restriction, "Close");
                return;
            }
            var value = await GameDialogs.ActionSheetAsync(this, defending ? "Defense play caller" : "Offense play caller", "Cancel", null, "You", "CPU");
            if (value is "You" or "CPU")
            {
                control.SetManual(defending, value == "You");
                if (defending) defense = null; else offense = null;
                commentary.Text = (defending ? "Defense" : "Offense") + " calls: " + value;
            }
        }
    }
    async Task TakeOver()
    {
        if (host.ChooseTeamOnTakeover && session.Mode == GameDayMode.Watch)
        {
            var selected = await GameDialogs.ActionSheetAsync(this, "Choose team to control", "Cancel", null, session.Game.AwayTeamName, session.Game.HomeTeamName);
            if (selected is null or "Cancel") return;
            session.ControlledTeamName = selected; session.UserCoachRole = StaffRole.HeadCoach;
        }
        control.TakeOver(); offense = defense = null;
        commentary.Text = "Taking over " + Brand(session.ControlledTeamName!).Abbreviation + " · choose your next call.";
    }
    async Task TeamMenu()
    {
        if (!control.HasTeam) return;
        var availability = session.IsComplete ? "Game complete" : GameDayCoachingService.TalkAvailability(session, session.ControlledTeamName!);
        var actions = new List<GameCastLiveAction>
        {
            new("Substitutions", Substitutions),
            new("Coach talk" + (availability == "Available" ? "" : " · " + availability), async () =>
            {
                if (session.IsComplete || availability != "Available") await GameDialogs.AlertAsync(this, "Coach talk", availability, "Close");
                else await CoachTalk();
            })
        };
        if (!session.IsComplete)
        {
            actions.Add(new(session.SecondUnits.Contains(session.ControlledTeamName!) ? "Use starters" : "Use second unit", async () =>
            {
                var second = !session.SecondUnits.Contains(session.ControlledTeamName!);
                if (second && !await GameDialogs.AlertAsync(this, "Use second unit?", "Use eligible backups on the units your role controls for this game. Season depth charts stay unchanged.", "Use backups", "Cancel")) return;
                GameDayCoachingService.UseSecondUnit(session, session.ControlledTeamName!, second);
                commentary.Text = second ? "Second unit ready for the next snap." : "Starters ready for the next snap.";
            }));
            actions.Add(new("Restore automatic rotation", () =>
            {
                foreach (var key in session.ManualStarters.Keys.Where(k => k.Team == session.ControlledTeamName && UserCoachControlService.ControlsPosition(session.UserCoachRole, k.Position)).ToArray()) session.ManualStarters.Remove(key);
                GameDayCoachingService.UseSecondUnit(session, session.ControlledTeamName!, false);
                commentary.Text = "Automatic rotation restored for your units."; return Task.CompletedTask;
            }));
            if (control.Permitted(false)) actions.Add(new(session.OnsideKickRequested ? "Next kickoff: Onside" : "Next kickoff: Deep", async () =>
            {
                var value = await GameDialogs.ActionSheetAsync(this, "Next kickoff", "Cancel", null, "Deep", "Onside");
                if (value is "Deep" or "Onside") session.OnsideKickRequested = value == "Onside";
            }));
            actions.Add(new("Call timeout", async () =>
            {
                if (GameDayEngine.TryCallTimeout(session, session.ControlledTeamName!))
                { control.Pause(); commentary.Text = GameDayCommentaryService.Describe(session.Events[^1], session.PlayersById); }
                else await GameDialogs.AlertAsync(this, "Timeout unavailable", "A timeout is unavailable in this situation or none remain.", "Close");
            }));
        }
        await GameCastLiveMenu.InvokeAsync(this, "Team · " + Brand(session.ControlledTeamName!).Abbreviation, actions);
    }
    async Task Substitutions()
    {
        var positions = session.Roster.Where(p => p.TeamName == session.ControlledTeamName && UserCoachControlService.ControlsPosition(session.UserCoachRole, p.Position))
            .Select(OffensiveLineService.Label).Distinct().OrderBy(p => p).ToArray();
        var position = await GameDialogs.ActionSheetAsync(this, "Substitutions · this game only", "Cancel", null, positions);
        if (position is null or "Cancel") return;
        var starters = GameDayCoachingService.Starters(session, session.ControlledTeamName!);
        var pool = session.Roster.Where(p => p.TeamName == session.ControlledTeamName && OffensiveLineService.Label(p) == position).OrderBy(p => p.DepthChartOrder).ThenByDescending(p => p.OverallRating).ToArray();
        var names = pool.Select(p => $"{p.FullName} · OVR {p.OverallRating}" + (starters.Contains(p.PlayerId) ? " · In game" : "") + (p.CurrentInjury is not null || p.IsRedshirted ? " · Unavailable" : "")).ToArray();
        var value = await GameDialogs.ActionSheetAsync(this, "Choose player", "Cancel", null, names);
        var index = Array.IndexOf(names, value);
        if (index >= 0) commentary.Text = GameDayCoachingService.Substitute(session, session.ControlledTeamName!, pool[index].PlayerId) ? pool[index].FullName + " in for the next snap." : "Player unavailable.";
    }
    async Task CoachTalk()
    {
        var audience = session.UserCoachRole == StaffRole.OffensiveCoordinator ? CoachTalkAudience.Offense : session.UserCoachRole == StaffRole.DefensiveCoordinator ? CoachTalkAudience.Defense : CoachTalkAudience.Team;
        if (session.UserCoachRole == StaffRole.HeadCoach)
        {
            var value = await GameDialogs.ActionSheetAsync(this, "Address", "Cancel", null, Enum.GetNames<CoachTalkAudience>());
            if (!Enum.TryParse(value, out audience)) return;
        }
        var tone = await GameDialogs.ActionSheetAsync(this, "Coach talk", "Cancel", null, Enum.GetNames<CoachTalkTone>());
        if (!Enum.TryParse<CoachTalkTone>(tone, out var selected)) return;
        var talk = GameDayCoachingService.Talk(session, session.ControlledTeamName!, selected, audience);
        if (talk is not null) commentary.Text = $"Talk delivered · {talk.Reactions.Count(r => r.Change > 0)} encouraged · {talk.Reactions.Count(r => r.Change < 0)} reacted negatively.";
    }
    async Task ViewingOptions()
    {
        var value = await GameDialogs.ActionSheetAsync(this, "Viewing options", "Cancel", null, "Camera", "Game speed", "Replay speed",
            "Automatic replays: " + (autoReplay ? "On" : "Off"), "Player names: " + (showNames ? "On" : "Off"));
        if (value == "Camera")
        {
            var names = new[] { "Full field", "Broadcast", "Follow ball" };
            var choice = await GameDialogs.ActionSheetAsync(this, "Camera", "Cancel", null, names); var index = Array.IndexOf(names, choice);
            if (index >= 0) { camera = index; Preferences.Set(Key("camera"), camera); FrameCamera(); }
        }
        else if (value is "Game speed" or "Replay speed")
        {
            var names = new[] { "0.5×", "1×", "1.5×", "2×" };
            var choice = await GameDialogs.ActionSheetAsync(this, value + " · presentation only", "Cancel", null, names); var index = Array.IndexOf(names, choice);
            if (index >= 0) { var rate = new[] { .5, 1, 1.5, 2 }[index]; if (value == "Game speed") speed = rate; else replaySpeed = rate; Preferences.Set(Key(value == "Game speed" ? "speed" : "replay-speed"), rate); }
        }
        else if (value?.StartsWith("Automatic replays:", StringComparison.Ordinal) == true) { autoReplay = !autoReplay; Preferences.Set(Key("auto-replay"), autoReplay); }
        else if (value?.StartsWith("Player names:", StringComparison.Ordinal) == true) { showNames = !showNames; Preferences.Set(Key("names"), showNames); }
    }
    async Task Finish()
    {
        if (!await GameDialogs.AlertAsync(this, "Finish game?", "Simulate the rest of this game and save the final result?", "Simulate remainder", "Keep playing")) return;
        finishing = true; Update();
        try { await Task.Run(() => GameDayEngine.SimulateToEnd(session)); await Save(); }
        finally { finishing = false; }
    }
    async Task Close()
    {
        if (session.IsComplete) await completion.CloseAsync(() => host.Save(session), () => Navigation.PopModalAsync());
        else
        {
            if (!await GameDialogs.AlertAsync(this, "Exit game?", "Discard this unfinished game? Your previously saved season stays unchanged.", "Discard and exit", "Keep playing")) return;
            await Navigation.PopModalAsync();
        }
        lifetime.Cancel();
    }
    protected override bool OnBackButtonPressed()
    {
        if (closePicker is not null) closePicker();
        else if (replaying) players.AbortAnimation("snap-movement");
        else if (!menuOpen && !closing && !finishing) _ = MenuAction(async () => { closing = true; try { await Close(); } finally { closing = false; } });
        return true;
    }
    async Task Replay(GameSnapEvent snap)
    {
        replaying = true; Update();
        var old = commentary.Text;
        try { await Animate(snap, true); }
        finally { replaying = false; commentary.Text = old; FrameCamera(); Update(); }
    }
    void FrameCamera(double x = .5, double y = .5) => GameCastCamera.Apply([turf, players], field.Width, field.Height, camera, x, y);
    Border Marker(DynastyPlayer? player, Color primary, Color secondary, bool away)
    {
        var appearance = PortraitAppearance.ForPlayer(player?.PlayerId ?? Guid.Empty, player?.Appearance);
        var sprite = new FootballSpriteView(primary, secondary, away, appearance.Skin, player?.JerseyNumber?.ToString() ?? player?.Position.DisplayName() ?? "", appearance.SkinColorHex);
        var marker = new Border { WidthRequest = 28, HeightRequest = 28, Padding = 0, StrokeThickness = 0, BackgroundColor = Colors.Transparent, InputTransparent = true, Content = sprite };
        spritePlayers[sprite] = player; markerHosts[marker] = (player, primary, secondary, away); return marker;
    }
    Task Animate(GameSnapEvent snap, bool replay) => GameCastAnimation.PlayAsync(new()
    {
        Session = session, ControlledPlan = Plan, Players = players, Turf = turf, Artwork = art, Commentary = commentary, Highlight = highlight,
        SpritePlayers = spritePlayers, MarkerHosts = markerHosts, ActiveBallOwners = ballOwners, CreateMarker = Marker,
        ClearMarkers = () => { spritePlayers.Clear(); markerHosts.Clear(); ballOwners.Clear(); }, TeamPrimary = Primary, TeamSecondary = Secondary,
        GameSpeed = speed, ReplaySpeed = replaySpeed, UseSprites = () => true, ShowCarrierNames = () => showNames, CameraMode = () => camera, FrameCamera = FrameCamera
    }, snap, replay);
    void RenderHuddles()
    {
        players.Clear(); spritePlayers.Clear(); markerHosts.Clear(); ballOwners.Clear();
        if (session.IsComplete) return;
        var direction = (session.PossessionTeamName == session.Game.HomeTeamName ? 1 : -1) * (session.Quarter % 2 == 0 ? -1 : 1);
        art.SpottingBall = !session.KickoffPending; art.Direction = direction; art.YardLine = session.YardLine; art.Distance = session.Distance; turf.Invalidate();
        foreach (var name in new[] { session.Game.HomeTeamName, session.Game.AwayTeamName })
        {
            var home = name == session.Game.HomeTeamName; var attacking = name == session.PossessionTeamName;
            var profile = home ? session.HomeProfile : session.AwayProfile; var plan = profile.GamePlan ?? new() { TeamName = name };
            var depth = home ? session.HomeDepthChart : session.AwayDepthChart; var fatigue = home ? session.HomeFatigue : session.AwayFatigue;
            var random = new Random(unchecked(session.Game.SimulationSeed ^ session.Events.Count * 31 ^ (home ? 17 : 37)));
            var play = PlayEditorService.OffensiveLibrary(plan).FirstOrDefault(p => p.Id == offense) ?? GamePlanService.RecommendOffensivePlay(plan, session.Down, session.Distance, session.YardLine);
            var defensePlay = PlayEditorService.DefensiveLibrary(plan).FirstOrDefault(p => p.Id == defense) ?? GamePlanService.RecommendDefensivePlay(plan, session.Down, session.Distance);
            var preview = attacking ? PersonnelRotationService.SelectOffense(depth, name, play.Personnel, fatigue, random) : PersonnelRotationService.SelectDefense(depth, name, defensePlay.Personnel, fatigue, random);
            var unit = GameDayCoachingService.ApplySubstitutions(session, preview).PlayerIds.Select(id => session.PlayersById.GetValueOrDefault(id)).OfType<DynastyPlayer>().ToArray();
            for (var i = 0; i < unit.Length; i++)
            {
                var player = unit[i]; var point = session.KickoffPending ? LiveFootballChoreography.Kickoff(attacking, attacking ? i == 0 : player.Position == Position.K, i, 2, 25, direction).From : GameDayLivePresentation.HuddlePoint(session.YardLine, direction, attacking, i);
                var marker = Marker(player, Primary(name), Secondary(name), !home);
                if (marker.Content is FootballSpriteView sprite) sprite.HuddleFacing(.5 - point.X, .5 - point.Y);
                AbsoluteLayout.SetLayoutFlags(marker, AbsoluteLayoutFlags.PositionProportional); AbsoluteLayout.SetLayoutBounds(marker, new Rect(point.X, point.Y, 28, 28)); players.Add(marker);
            }
        }
    }
}

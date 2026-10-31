using DynastyManager.App;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Microsoft.Maui.Controls.Shapes;
using ProFootball.Core;
namespace ProFootball.App;

// League-specific host; animation, field art, play cards, cameras and box scores are shared with college.
public sealed partial class ProLivePage : ContentPage
{
    readonly GameDaySession session; readonly ProLeague league; readonly Func<Task> save; readonly bool chooseTeamOnTakeover;
    readonly Label awayScore = Hud(25, true), homeScore = Hud(25, true), awayTimeouts = Hud(11), homeTimeouts = Hud(11);
    readonly Label situation = Hud(13, true), possession = Hud(11), commentary = Hud(14);
    readonly Label clockLabel = Hud(13, true), downLabel = Hud(13, true), ballLabel = Hud(13, true);
    VerticalStackLayout? playSheetRef; ActionRow? quickActionsRef; Border? liveSheetRef;
    readonly Grid field = new() { IsClippedToBounds = true, AutomationId = "pro-live-field" };
    readonly AbsoluteLayout players = new();
    readonly Dictionary<FootballSpriteView, DynastyPlayer?> spritePlayers = new();
    readonly Dictionary<Border, (DynastyPlayer? Player, Color Primary, Color Secondary, bool Away)> markerHosts = new();
    readonly HashSet<Guid> ballOwners = new();
    readonly Grid pickerLayer = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#B0000000"), Padding = 10 };
    readonly Label replayChip = new() { Text = "INSTANT REPLAY", IsVisible = false, FontSize = 12, FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb("#FFE194"), BackgroundColor = Color.FromArgb("#101F2C"), Padding = new Thickness(12, 4), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, Margin = 6 };
    readonly GameFieldDrawable art; readonly GraphicsView turf;
    readonly Button next = new() { Text = "Next snap" }, pause = new() { Text = "Pause watch" }, skipReplay = new() { Text = "Skip replay", IsVisible = false };
    readonly Button offenseButton, defenseButton, replayButton, takeoverButton, watchingButton, timeoutButton, secondButton, finishButton, talkButton;
    bool active, working, pending, paused, saved, second, initialized, replaying, callConfirmed; bool? pauseAfterAction;
    double speed = Preferences.Get("pro-game-speed", 1d), replaySpeed = Preferences.Get("pro-replay-speed", 1d);
    int cameraMode = Math.Clamp(Preferences.Get("pro-camera", 0), 0, 2);
    bool autoReplays = Preferences.Get("pro-auto-replays", true), showNames = Preferences.Get("pro-player-names", true);
    string? offense, defense; GameSnapEvent? last; Action? closePicker;
    readonly Label selected = Hud(12);
    readonly Grid quickCalls = new() { ColumnSpacing = 6, ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, AutomationId = "quick-play-calls" };
    static Label Hud(int size, bool bold = false) => new() { FontSize = size, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
        TextColor = DynastyTheme.Text, VerticalTextAlignment = TextAlignment.Center };
    public ProLivePage(GameDaySession session, ProLeague league, Func<Task> save, bool chooseTeamOnTakeover = false)
    {
        this.session = session; this.league = league; this.save = save; this.chooseTeamOnTakeover = chooseTeamOnTakeover; BackgroundColor = DynastyTheme.Background;
        speed = Math.Clamp(speed, .5, 2); replaySpeed = Math.Clamp(replaySpeed, .5, 2);
        var home = league.Teams.Single(t => t.Name == session.Game.HomeTeamName); var away = league.Teams.Single(t => t.Name == session.Game.AwayTeamName);
        art = new() { HomeName = home.City.ToUpperInvariant(), HomeInitial = home.Abbreviation, HomeColor = Color.FromArgb(home.Primary), HomeSecondaryColor = Color.FromArgb(home.Secondary), HashMarkFraction = .30, Weather = session.Weather };
        turf = new() { Drawable = art, InputTransparent = true }; field.Add(turf); field.Add(players); field.Add(replayChip);
        var header = new Grid { ColumnSpacing=6, ColumnDefinitions=new(){new(GridLength.Star),new(new GridLength(0))}, RowDefinitions=new(){new(GridLength.Auto),new(GridLength.Auto),new(GridLength.Auto)} };
        var scoreboard=GameCastScoreboard.Create(
            GameCastScoreboard.Team(new TeamBadgeView(away, 30), new Label { Text = away.Abbreviation, FontSize = 13, FontAttributes = FontAttributes.Bold }, awayScore, awayTimeouts),
            GameCastScoreboard.Team(new TeamBadgeView(home, 30), new Label { Text = home.Abbreviation, FontSize = 13, FontAttributes = FontAttributes.Bold }, homeScore, homeTimeouts));
        possession.HorizontalTextAlignment = TextAlignment.Center;
        var situationGrid = new Grid { ColumnSpacing = 8, Padding = new Thickness(8, 2), ColumnDefinitions = new(){new(GridLength.Star),new(GridLength.Star),new(GridLength.Star)} };
        situationGrid.Add(clockLabel,0,0); situationGrid.Add(downLabel,1,0); situationGrid.Add(ballLabel,2,0);
        header.Add(scoreboard,0,0); header.Add(situationGrid,0,1); header.Add(possession,0,2);
        var fieldBorder=new Border { Padding = 5, Stroke = DynastyTheme.Muted, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 8 }, BackgroundColor = DynastyTheme.Surface, Content = field };
        var primary = new ActionRow(); var quickActions = new ActionRow(); quickActionsRef = quickActions;
        var advanced = new List<(Button Button, Func<Task> Action)>();
        void Style(Button button) { button.FontSize = 12; button.Padding = new Thickness(10, 6); button.MinimumHeightRequest = 44; button.CornerRadius = 10; }
        Button Make(string text, Func<Task> action, ActionRow host)
        { var b = new Button { Text = text }; Style(b); b.Clicked += async (_, _) => await Run(action); host.Add(b); return b; }
        Button Hidden(string text, Func<Task> action)
        { var b = new Button { Text = text, IsVisible = false }; Style(b); b.Clicked += async (_, _) => await Run(action); advanced.Add((b, action)); return b; }
        Style(next); Style(pause); primary.Add(next); primary.Add(pause);
        next.Clicked += async (_, _) => await Snap(); pause.Clicked += (_, _) => { if (!pending) { paused = !paused; Update(); } };
        offenseButton = Make("Choose play", () => Playbook(false), quickActions);
        defenseButton = Make("Choose defense", () => Playbook(true), quickActions);
        replayButton = Hidden("Instant replay", async () => { if (last is not null) await Replay(last); });
        Style(skipReplay); skipReplay.Clicked += (_, _) => players.AbortAnimation("snap-movement"); primary.Add(skipReplay);
        Hidden("Viewing options", ViewingOptions); Hidden("Speed", ChooseSpeed);
        takeoverButton = Make("Take over", TakeOver, primary);
        watchingButton = Hidden("Watch CPU vs CPU", () => { session.Mode = GameDayMode.Watch; pauseAfterAction = false; selected.Text = "CPU recommended calls"; return Task.CompletedTask; });
        Hidden("Depth chart", DepthChart);
        timeoutButton = Make("Timeout", () => { if (!GameDayEngine.TryCallTimeout(session, session.ControlledTeamName!)) selected.Text = "Timeout unavailable"; return Task.CompletedTask; }, quickActions);
        secondButton = Hidden("Team · second unit", () => { second = !second; GameDayCoachingService.UseSecondUnit(session, session.ControlledTeamName!, second); selected.Text = second ? "Second team on field" : "Starters on field"; return Task.CompletedTask; });
        talkButton = Hidden("Coach talk", CoachTalk);
        Hidden("Box score", ShowBox);
        finishButton = Hidden("Finish game", async () => { if (!session.IsComplete) await Task.Run(() => GameDayEngine.SimulateToEnd(session)); await Commit(); });
        Hidden("Return to franchise", CloseCore);
        var more = Make("More", async () => {
            var groups = new List<string> { "PLAY & PERSONNEL" };
            if (offenseButton.IsVisible) groups.Add("Call a different play");
            if (defenseButton.IsVisible) groups.Add("Call a different defense");
            groups.Add("Depth chart"); groups.Add("Team · second unit"); groups.Add("Coach talk");
            groups.Add("GAME CONTROL"); groups.Add("Timeout"); groups.Add("Instant replay"); groups.Add("Watch CPU vs CPU"); groups.Add("Take over");
            groups.Add("VIEW & AUDIO"); groups.Add("Viewing options"); groups.Add("Speed"); groups.Add("Box score");
            groups.Add("GAME FLOW"); groups.Add("Finish game"); groups.Add("Return to franchise");
            var pick = await GameDialogs.ActionSheetAsync(this, "Game menu", "Cancel", null, groups.ToArray());
            Func<Task>? action = pick switch {
                "Call a different play" => () => Playbook(false),
                "Call a different defense" => () => Playbook(true),
                "Depth chart" => DepthChart,
                "Team · second unit" => () => { second = !second; GameDayCoachingService.UseSecondUnit(session, session.ControlledTeamName!, second); selected.Text = second ? "Second team on field" : "Starters on field"; return Task.CompletedTask; },
                "Coach talk" => CoachTalk,
                "Timeout" => () => { if (!GameDayEngine.TryCallTimeout(session, session.ControlledTeamName!)) selected.Text = "Timeout unavailable"; return Task.CompletedTask; },
                "Instant replay" => async () => { if (last is not null) await Replay(last); },
                "Watch CPU vs CPU" => () => { session.Mode = GameDayMode.Watch; pauseAfterAction = false; return Task.CompletedTask; },
                "Take over" => TakeOver, "Viewing options" => ViewingOptions, "Speed" => ChooseSpeed,
                "Box score" => ShowBox, "Finish game" => async () => { if (!session.IsComplete) await Task.Run(() => GameDayEngine.SimulateToEnd(session)); await Commit(); },
                "Return to franchise" => CloseCore, _ => null };
            if (action is not null) await Run(action);
        }, quickActions);
        var playHeading = new Label { Text = "NEXT PLAY", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = DynastyTheme.Accent };
        var playSheet = new VerticalStackLayout { Spacing = 6, Children = { playHeading, selected, quickCalls } }; playSheetRef = playSheet;
        var tray = new VerticalStackLayout { Spacing = 6, Children = { playSheet, primary, quickActions } };
        var trayBorder = new Border { AutomationId = "pro-live-controls", StrokeThickness = 0, BackgroundColor = DynastyTheme.Surface, Padding = 8, Content = new ScrollView { Content = tray } }; liveSheetRef = trayBorder;
        commentary.MaxLines=2;commentary.LineBreakMode=LineBreakMode.TailTruncation;
        var commentaryBorder=new Border { StrokeThickness = 0, Padding = new Thickness(10, 4), BackgroundColor = DynastyTheme.Panel, Content = commentary };
        var layout=new Grid{Padding=6,RowSpacing=5,RowDefinitions=new(){new(GridLength.Auto),new(GridLength.Star),new(GridLength.Auto),new(GridLength.Auto)}};
        layout.Add(header,0,0);layout.Add(fieldBorder,0,1);layout.Add(trayBorder,0,2);layout.Add(commentaryBorder,0,3);
        var root = new Grid();root.Add(layout);root.Add(pickerLayer);Content=root;
        SizeChanged += (_, _) => {
            var landscape=Width>Height;
            header.ColumnDefinitions[0].Width=new GridLength(landscape?2:1,GridUnitType.Star);
            header.ColumnDefinitions[1].Width=landscape?GridLength.Star:new GridLength(0);
            // Keep the field near its real 1.76:1 playing-surface ratio instead of stretching to the row.
            if(landscape){fieldBorder.HeightRequest=-1;fieldBorder.WidthRequest=-1;fieldBorder.HorizontalOptions=LayoutOptions.Fill;}
            else
            {
                var ratioHeight=(Math.Max(0,Width-12))/1.76;
                var fieldHeight=Math.Max(140,Math.Min(ratioHeight,Height*0.55));
                fieldBorder.HeightRequest=fieldHeight;
                fieldBorder.WidthRequest=fieldHeight*1.76;
                fieldBorder.HorizontalOptions=LayoutOptions.Center;
            }
            if(initialized&&!working&&!pending)UpdateQuickCalls();
            FrameCamera();
        };
        field.SizeChanged += (_, _) => FrameCamera();
        Loaded += (_, _) => { art.HomeLogo = TeamBadgeView.Mark(home, false); turf.Invalidate(); if (initialized) return; initialized = true; active = true; selected.Text = session.Mode == GameDayMode.Watch ? "CPU recommended calls" : session.KickoffPending ? "Ready for kickoff" : "Coach mode · choose plays, then Next snap"; Update(); _ = Watch(); };
    }
    async Task Run(Func<Task> action)
    {
        if (pending) return; pending = true; var wasPaused = paused; paused = true; pauseAfterAction = null;
        try { while (working) await Task.Delay(30); working = true; await action(); }
        catch (Exception error) { await GameDialogs.AlertAsync(this, "Unable to continue", error.Message, "OK"); }
        finally { working = false; pending = false; paused = pauseAfterAction ?? (session.Mode == GameDayMode.Play && !CoordinatorMode || wasPaused); Update(); }
    }
    protected override bool OnBackButtonPressed()
    {
        if (closePicker is not null) closePicker();
        else if (replaying) players.AbortAnimation("snap-movement");
        else _ = Run(CloseCore);
        return true;
    }
    async Task CloseCore()
    {
        if (session.IsComplete) await Commit();
        else if (!await GameDialogs.AlertAsync(this, "Leave game?", "Unfinished snaps will be discarded. Your saved franchise remains unchanged.", "Leave", "Cancel")) return;
        active = false; players.AbortAnimation("snap-movement"); await Navigation.PopModalAsync();
    }
    bool CoordinatorMode => session.Mode == GameDayMode.Play && session.UserCoachRole is StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator;
    bool CpuUnit => CoordinatorMode && ProCareer.CpuHandlesUnit(session.UserCoachRole, session.PossessionTeamName == session.ControlledTeamName);
    async Task Watch()
    {
        while (active && !session.IsComplete) { if ((session.Mode == GameDayMode.Watch || CpuUnit && !session.KickoffPending) && !working && !pending && !paused) await Snap(); await Task.Delay(paused || session.Mode != GameDayMode.Watch && !CpuUnit ? 250 : 1400); }
    }
    async Task Snap()
    {
        if (working || pending || session.IsComplete) return; working = true; Update();
        try
        {
            last = ProLiveControl.Step(session, offense, defense);
            offense = defense = null;
            await Animate(last, false);
            if (autoReplays && !pending && GameDayLivePresentation.InstantReplay(last)) await Replay(last);
            if (session.Mode == GameDayMode.Play && !session.IsComplete) selected.Text = "Coach mode · choose plays, then Next snap";
            commentary.Text = GameDayCommentaryService.Describe(last, session.PlayersById);
            if (session.IsComplete) await Commit();
        }
        catch (Exception error) { paused = true; await GameDialogs.AlertAsync(this, "Game paused", error.Message, "OK"); }
        finally { working = false; callConfirmed=false; Update(); }
    }
    async Task Replay(GameSnapEvent snap)
    {
        quickCalls.IsVisible=false;
        replaying = replayChip.IsVisible = skipReplay.IsVisible = true;
        try { await Animate(snap, true); }
        finally { replaying = replayChip.IsVisible = skipReplay.IsVisible = false; FrameCamera(); }
    }
    async Task Commit() { if (saved || !session.IsComplete) return; await save(); saved = true; }
    void Update()
    {
        string Short(string team) => league.Teams.Single(t => t.Name == team).Abbreviation;
        awayScore.Text = session.AwayScore.ToString(); homeScore.Text = session.HomeScore.ToString();
        awayTimeouts.Text = $"TO {session.AwayTimeouts}"; homeTimeouts.Text = $"TO {session.HomeTimeouts}";
        clockLabel.Text = session.IsComplete ? "FINAL" : $"{(session.Quarter > 4 ? "OT" : "Q" + session.Quarter)}  {session.ClockSeconds / 60}:{session.ClockSeconds % 60:00}";
        downLabel.Text = session.IsComplete ? (saved ? "Saved" : "Save pending") : session.KickoffPending ? "Kickoff" : $"{session.Down} & {session.Distance}";
        ballLabel.Text = session.IsComplete ? "" : session.KickoffPending ? $"{Short(session.PossessionTeamName)} receives" : GameDayEngine.BallOn(session, Short);
        possession.Text = session.IsComplete ? "Final" : (session.Mode == GameDayMode.Watch ? "" : "Controlling " + Short(session.ControlledTeamName!) + " · ") + Short(session.PossessionTeamName) + " possession · " + (session.Mode == GameDayMode.Watch ? "Watching CPU vs CPU" : CoordinatorMode ? CpuUnit ? "CPU handling opposite unit" : "Your unit · choose calls" : "Coach mode");
        var inPlay = working || pending || replaying; if (liveSheetRef is not null) liveSheetRef.IsVisible = !inPlay;
        var watch = session.Mode == GameDayMode.Watch;
        next.IsVisible = !watch && !session.IsComplete && (!CpuUnit || paused || session.KickoffPending); next.Text = session.KickoffPending ? "Kickoff" : "Call play"; next.IsEnabled = !working && !pending;
        pause.IsVisible = (watch || CoordinatorMode) && !session.IsComplete; pause.Text = CoordinatorMode ? paused ? "Resume CPU unit" : "Pause CPU unit" : paused ? "Resume watch" : "Pause watch";
        takeoverButton.IsVisible = watch && !session.IsComplete; watchingButton.IsVisible = !watch && !session.IsComplete;
        offenseButton.IsVisible = ProLiveControl.CanCall(session, false);
        defenseButton.IsVisible = ProLiveControl.CanCall(session, true);
        var availability = GameDayCoachingService.TalkAvailability(session, session.ControlledTeamName!);
        talkButton.IsEnabled = !session.IsComplete && availability == "Available";
        talkButton.Text = talkButton.IsEnabled ? "Coach talk" : "Coach talk · " + availability;
        replayButton.IsEnabled = last is not null && !session.IsComplete;
        timeoutButton.IsEnabled = !session.IsComplete && (session.ControlledTeamName == session.Game.HomeTeamName ? session.HomeTimeouts : session.AwayTimeouts) > 0;
        secondButton.IsEnabled = !session.IsComplete; secondButton.Text = second ? "Use starters" : "Second team";
        finishButton.IsEnabled = !session.IsComplete || !saved; finishButton.Text = session.IsComplete && !saved ? "Retry save" : "Finish game";
        if (!working && !pending) { UpdateQuickCalls(); RenderHuddles(); }
        else quickCalls.IsVisible=false;
        quickCalls.IsEnabled = !working && !pending;
    }
    void RenderHuddles()
    {
        if (working || pending || session.IsComplete) return;
        players.Clear(); spritePlayers.Clear(); markerHosts.Clear(); ballOwners.Clear();
        var direction = (session.PossessionTeamName == session.Game.HomeTeamName ? 1 : -1) * (session.Quarter % 2 == 0 ? -1 : 1);
        art.SpottingBall = !session.KickoffPending;
        art.Direction = direction; art.YardLine = session.YardLine; art.Distance = session.Distance; turf.Invalidate();
        foreach (var team in new[] { session.Game.HomeTeamName, session.Game.AwayTeamName })
        {
            var attacking = team == session.PossessionTeamName; var home = team == session.Game.HomeTeamName;
            var profile = home ? session.HomeProfile : session.AwayProfile; var plan = profile.GamePlan ?? new TeamGamePlan { TeamName = team };
            var depth = home ? session.HomeDepthChart : session.AwayDepthChart; var fatigue = home ? session.HomeFatigue : session.AwayFatigue;
            var previewRandom = new Random(unchecked(session.Game.SimulationSeed ^ session.Events.Count * 31 ^ (home ? 17 : 37)));
            var play = PlaybookCatalog.OffensivePlays.FirstOrDefault(p => p.Id == offense) ?? GamePlanService.RecommendOffensivePlay(plan, session.Down, session.Distance, session.YardLine);
            var call = PlaybookCatalog.DefensivePlays.FirstOrDefault(p => p.Id == defense) ?? GamePlanService.RecommendDefensivePlay(plan, session.Down, session.Distance);
            var preview = attacking ? PersonnelRotationService.SelectOffense(depth, team, play.Personnel, fatigue, previewRandom) : PersonnelRotationService.SelectDefense(depth, team, call.Personnel, fatigue, previewRandom);
            var unit = GameDayCoachingService.ApplySubstitutions(session, preview).PlayerIds.Select(id => session.PlayersById.GetValueOrDefault(id)).OfType<DynastyPlayer>().ToArray();
            var brand = league.Teams.Single(t => t.Name == team);
            for (var i = 0; i < unit.Length; i++)
            {
                var p = unit[i]; var point = session.KickoffPending ? LiveFootballChoreography.Kickoff(attacking, attacking ? i == 0 : p.Position == Position.K, i, 2, 25, direction).From : GameDayLivePresentation.HuddlePoint(session.YardLine, direction, attacking, i);
                var sprite = new FootballSpriteView(Color.FromArgb(brand.Primary), Color.FromArgb(brand.Secondary), !home, p.PlayerId.ToByteArray()[0] % 4, p.JerseyNumber?.ToString() ?? p.Position.DisplayName());
                sprite.HuddleFacing(.5 - point.X, .5 - point.Y);
                AbsoluteLayout.SetLayoutFlags(sprite, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);
                AbsoluteLayout.SetLayoutBounds(sprite, new Rect(point.X, point.Y, 28, 28)); players.Add(sprite);
            }
        }
    }
    Border Marker(DynastyPlayer? player, Color primary, Color secondary, bool away)
    {
        var sprite = new FootballSpriteView(primary, secondary, away, PortraitAppearance.ForPlayer(player?.PlayerId ?? Guid.Empty, player?.Appearance).Skin, player?.JerseyNumber?.ToString() ?? player?.Position.DisplayName() ?? "",PortraitAppearance.ForPlayer(player?.PlayerId??Guid.Empty,player?.Appearance).SkinColorHex);
        var host = new Border { WidthRequest = 28, HeightRequest = 28, Padding = 0, StrokeThickness = 0, BackgroundColor = Colors.Transparent, InputTransparent = true, Content = sprite };
        spritePlayers[sprite] = player; markerHosts[host] = (player, primary, secondary, away);
        if (player is not null) SemanticProperties.SetDescription(host, $"{player.Position.DisplayName()} {player.FullName}");
        return host;
    }
    Task Animate(GameSnapEvent snap, bool replay) => GameCastAnimation.PlayAsync(new()
    {
        Session = session, ControlledPlan = (session.ControlledTeamName == session.Game.HomeTeamName ? session.HomeProfile : session.AwayProfile).GamePlan ?? new() { TeamName = session.Game.HomeTeamName },
        Players = players, Turf = turf, Artwork = art, Commentary = commentary, Highlight = new Border(),
        SpritePlayers = spritePlayers, MarkerHosts = markerHosts, ActiveBallOwners = ballOwners, CreateMarker = Marker,
        ClearMarkers = () => { spritePlayers.Clear(); markerHosts.Clear(); ballOwners.Clear(); },
        TeamPrimary = team => Color.FromArgb(league.Teams.Single(t => t.Name == team).Primary),
        TeamSecondary = team => Color.FromArgb(league.Teams.Single(t => t.Name == team).Secondary),
        GameSpeed = speed, ReplaySpeed = replaySpeed,
        UseSprites = () => true, ShowCarrierNames = () => showNames, CameraMode = () => cameraMode, FrameCamera = FrameCamera
    }, snap, replay);
}
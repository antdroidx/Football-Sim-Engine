using DynastyManager.App;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;

namespace ProFootball.App;

public sealed partial class FranchisePage
{
    // Copy editors await a modal result. They must not retain the page-wide busy
    // guard while the modal's Save/Cancel actions need that same guard.
    Button PlayEditorAction(string text, Func<Task> action)
    {
        var button = new Button { Text = text };
        button.Clicked += async (_, _) =>
        {
            if (!button.IsEnabled) return;
            button.IsEnabled = false;
            try { await action(); }
            catch (Exception ex) { await GameDialogs.AlertAsync(this, "Unable to continue", ex.Message, "OK"); }
            finally { button.IsEnabled = true; }
        };
        return button;
    }

    View EditorHeading(string title, string subtitle) => new VerticalStackLayout
    {
        Spacing = 4,
        Children = { Text(title, 20, true), Text(subtitle, 12) }
    };

    async Task OpenProPlaybookEditor(TeamGamePlan plan)
    {
        var createdOffense = plan.CreatedOffensivePlays.ToList();
        var createdDefense = plan.CreatedDefensivePlays.ToList();
        var offenseIds = (plan.CustomOffensivePlayIds.Count > 0 ? plan.CustomOffensivePlayIds :
            GamePlanService.AvailableOffensivePlays(plan).Select(p => p.Id)).ToHashSet(StringComparer.Ordinal);
        var defenseIds = (plan.CustomDefensivePlayIds.Count > 0 ? plan.CustomDefensivePlayIds :
            GamePlanService.AvailableDefensivePlays(plan).Select(p => p.Id)).ToHashSet(StringComparer.Ordinal);
        var role = ProCareer.Role(state!);
        var page = new ContentPage { Title = "Playbook Editor", BackgroundColor = DynastyTheme.Background };
        var side = new Picker { ItemsSource = new[] { "Offense", "Defense" }, SelectedIndex = 0 };
        if (role is StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator)
        {
            side.SelectedIndex = role == StaffRole.OffensiveCoordinator ? 0 : 1;
            side.IsEnabled = false;
        }
        var formation = new Picker { Title = "Formation" };
        var count = Text("", 12, true);
        var status = Text("", 12);
        var library = new VerticalStackLayout { Spacing = 8 };
        var save = Action("Save custom playbooks", async () =>
        {
            var next = plan with
            {
                Offense = OffensivePlaybook.Custom, Defense = DefensivePlaybook.Custom,
                CreatedOffensivePlays = createdOffense.ToArray(), CreatedDefensivePlays = createdDefense.ToArray(),
                CustomOffensivePlayIds = offenseIds.ToArray(), CustomDefensivePlayIds = defenseIds.ToArray(),
                OpeningPlayIds = [], OpeningScript = []
            };
            try
            {
                PlayEditorService.Validate(next);
                var limited = ProCareer.LimitPlan(state!, next); state = state! with { GamePlans = state.GamePlans.Where(p => p.TeamName != state.UserTeam).Append(limited).ToList() };
                await Save();
                await Navigation.PopModalAsync();
                await Navigation.PopModalAsync();
                Render();
            }
            catch (Exception ex) { status.Text = ex.Message; }
        });
        save.IsEnabled = false;
        var back = Action("Cancel", async () => await Navigation.PopModalAsync());
        void RenderEditor()
        {
            library.Children.Clear();
            count.Text = $"Offense {offenseIds.Count}/17 · Defense {defenseIds.Count}/14";
            var selected = Math.Max(0, formation.SelectedIndex);
            if (side.SelectedIndex == 0)
            {
                var value = Enum.GetValues<OffensiveFormation>()[selected];
                foreach (var play in PlaybookCatalog.OffensivePlays.Concat(createdOffense).Where(p => p.Formation == value))
                {
                    library.Children.Add(PlayRow(play.Id, play.Name, $"{GamePlanService.Label(play.Formation)} · {GamePlanService.Label(play.Concept)}", play.Description,
                        true, play.Concept.ToString(), offenseIds, RenderEditor, play));
                    var edit = PlayEditorAction($"Edit / copy {play.Name}", async () =>
                    {
                        var result = await EditProOffensivePlay(play);
                        if (result is not null) { createdOffense.Add(result); RenderEditor(); }
                    });
                    library.Children.Add(edit);
                }
            }
            else
            {
                var value = Enum.GetValues<DefensiveFormation>()[selected];
                foreach (var play in PlaybookCatalog.DefensivePlays.Concat(createdDefense).Where(p => p.Formation == value))
                {
                    library.Children.Add(PlayRow(play.Id, play.Name, $"{GamePlanService.Label(play.Formation)} · {GamePlanService.Label(play.Approach)}", play.Description,
                        false, play.Approach.ToString(), defenseIds, RenderEditor, defensive: play));
                    var edit = PlayEditorAction($"Edit / copy {play.Name}", async () =>
                    {
                        var result = await EditProDefensivePlay(play);
                        if (result is not null) { createdDefense.Add(result); RenderEditor(); }
                    });
                    library.Children.Add(edit);
                }
            }
            save.IsEnabled = offenseIds.Count is > 0 and <= 17 && defenseIds.Count is > 0 and <= 14;
        }
        void ChangeSide()
        {
            formation.ItemsSource = side.SelectedIndex == 0
                ? Enum.GetValues<OffensiveFormation>().Select(GamePlanService.Label).ToArray()
                : Enum.GetValues<DefensiveFormation>().Select(GamePlanService.Label).ToArray();
            formation.SelectedIndex = 0;
            RenderEditor();
        }
        side.SelectedIndexChanged += (_, _) => ChangeSide();
        formation.SelectedIndexChanged += (_, _) => RenderEditor();
        ChangeSide();
        page.Content = new ScrollView { Content = new VerticalStackLayout { Padding = 14, Spacing = 10, Children =
        {
            back, EditorHeading("Playbook Editor", "Choose calls for your Custom offense and defense."),
            side, formation, count, save, status, library
        } } };
        await Navigation.PushModalAsync(page);
    }

    View PlayRow(string id, string name, string details, string description, bool offense, string style,
        HashSet<string> selections, Action rerender, OffensivePlayDefinition? offensive = null, DefensivePlayDefinition? defensive = null)
    {
        var selected = selections.Contains(id);
        var toggle = new Button { Text = selected ? "★" : "☆", FontSize = 20, BackgroundColor = Colors.Transparent,
            TextColor = selected ? DynastyTheme.Palette("#E8C979") : DynastyTheme.Muted };
        toggle.Clicked += (_, _) =>
        {
            if (selected) selections.Remove(id);
            else if (selections.Count < (offense ? 17 : 14)) selections.Add(id);
            rerender();
        };
        var art = new GraphicsView { Drawable = new PlayArtDrawable(id, name, offense, style, offensive, defensive),
            HeightRequest = 92, WidthRequest = 150, BackgroundColor = DynastyTheme.Palette("#102A24") };
        var grid = new Grid { ColumnSpacing = 10,
            ColumnDefinitions = { new(new GridLength(150)), new(GridLength.Star), new(GridLength.Auto) } };
        grid.Add(art, 0, 0);
        grid.Add(new VerticalStackLayout { Spacing = 3, Children = {
            Text(name, 13, true), Text(details, 11), Text(description, 11) } }, 1, 0);
        grid.Add(toggle, 2, 0);
        return Panel("", grid);
    }

    async Task<OffensivePlayDefinition?> EditProOffensivePlay(OffensivePlayDefinition source)
    {
        var completion = new TaskCompletionSource<OffensivePlayDefinition?>();
        var page = new ContentPage { Title = "Play Editor", BackgroundColor = DynastyTheme.Background };
        var name = new Entry { Text = source.Name + " Custom", MaxLength = 60 };
        var formation = new Picker { Title = "Formation", ItemsSource = Enum.GetNames<OffensiveFormation>(), SelectedIndex = (int)source.Formation };
        var concept = new Picker { Title = "Concept", ItemsSource = Enum.GetNames<OffensiveConcept>(), SelectedIndex = (int)source.Concept };
        var assignments = source.Assignments.ToDictionary(a => (a.Position, a.RoleIndex));
        var rows = new VerticalStackLayout { Spacing = 8 };
        var preview = new GraphicsView { HeightRequest = 180, BackgroundColor = DynastyTheme.Palette("#102A24") };
        OffensivePlayDefinition Draft() => source with
        {
            Id = "custom-" + Guid.NewGuid().ToString("N"), Name = name.Text?.Trim() ?? "",
            Formation = (OffensiveFormation)Math.Max(0, formation.SelectedIndex),
            Personnel = (OffensiveFormation)formation.SelectedIndex switch
            {
                OffensiveFormation.Flexbone => OffensivePersonnel.TripleBack,
                OffensiveFormation.IForm => OffensivePersonnel.TwentyOne,
                OffensiveFormation.Empty => OffensivePersonnel.Empty,
                _ => source.Personnel is OffensivePersonnel.Empty or OffensivePersonnel.TripleBack ? OffensivePersonnel.Eleven : source.Personnel
            },
            Concept = (OffensiveConcept)Math.Max(0, concept.SelectedIndex),
            Assignments = assignments.Values.ToArray(), Description = "Created in Play Editor"
        };
        void Draw()
        {
            var draft = Draft();
            preview.Drawable = new PlayArtDrawable(draft.Id, draft.Name, true, draft.Concept.ToString(), draft);
            preview.Invalidate();
        }
        void BuildAssignments()
        {
            rows.Children.Clear();
            var positions = new[] { Position.QB, Position.OL, Position.OL, Position.OL, Position.OL, Position.OL }
                .Concat(GameDayAnimationPlanner.EligiblePositions(Draft().Personnel));
            var counts = new Dictionary<Position, int>();
            foreach (var position in positions)
            {
                var role = counts.GetValueOrDefault(position); counts[position] = role + 1;
                var slot = (position, role);
                var alignment = GameDayAnimationPlanner.OffensiveAlignment(Draft(), position, role);
                var original = assignments.GetValueOrDefault(slot) ?? new PlayAssignment { Position = position, RoleIndex = role,
                    Depth = alignment.X, Lane = alignment.Y, Task = position == Position.OL ? AssignmentTask.Block : AssignmentTask.Route,
                    Route = GameDayAnimationPlanner.AssignedRoute(source, position, role).ToString() };
                original = original with { Depth = alignment.X, Lane = alignment.Y };
                assignments[slot] = original;
                var task = new Picker { ItemsSource = new[] { "Route", "Block" }, SelectedIndex = original.Task == AssignmentTask.Block ? 1 : 0 };
                var route = new Picker { ItemsSource = Enum.GetNames<GameDayRouteFamily>(), SelectedIndex = (int)Enum.Parse<GameDayRouteFamily>(original.Route) };
                task.IsEnabled = position != Position.OL && position != Position.QB;
                if (position == Position.OL || position == Position.QB) task.SelectedIndex = 1;
                var label = Text("", 12, true);
                void Update()
                {
                    assignments[slot] = original with { Depth = alignment.X, Lane = alignment.Y,
                        Task = task.SelectedIndex == 1 ? AssignmentTask.Block : AssignmentTask.Route,
                        Route = Enum.GetNames<GameDayRouteFamily>()[Math.Max(0, route.SelectedIndex)] };
                    label.Text = $"{position} {role + 1} · formation alignment locked";
                    route.IsEnabled = task.SelectedIndex == 0; Draw();
                }
                task.SelectedIndexChanged += (_, _) => Update();
                route.SelectedIndexChanged += (_, _) => Update();
                rows.Children.Add(new VerticalStackLayout { Spacing = 3, Children = { label, task, route } });
                Update();
            }
        }
        var status = Text("", 12);
        var save = Action("Save to play library", async () =>
        {
            var draft = Draft();
            try { PlayEditorService.Validate(new TeamGamePlan { TeamName = "Editor", CreatedOffensivePlays = [draft] }); }
            catch (ArgumentException ex) { status.Text = ex.Message; return; }
            completion.TrySetResult(draft); await Navigation.PopModalAsync();
        });
        var cancel = Action("Cancel", async () => { completion.TrySetResult(null); await Navigation.PopModalAsync(); });
        page.Disappearing += (_, _) => Dispatcher.Dispatch(() => { if (!Navigation.ModalStack.Contains(page)) completion.TrySetResult(null); });
        formation.SelectedIndexChanged += (_, _) => { assignments.Clear(); BuildAssignments(); };
        concept.SelectedIndexChanged += (_, _) => Draw();
        BuildAssignments();
        page.Content = new ScrollView { Content = new VerticalStackLayout { Padding = 14, Spacing = 10, Children =
        {
            cancel, EditorHeading("Play Editor", "Edit routes and blocking assignments, then save a custom call."),
            name, formation, concept, preview, save, status, rows
        } } };
        await Navigation.PushModalAsync(page);
        return await completion.Task;
    }

    async Task<DefensivePlayDefinition?> EditProDefensivePlay(DefensivePlayDefinition source)
    {
        var completion = new TaskCompletionSource<DefensivePlayDefinition?>();
        var page = new ContentPage { Title = "Defensive Play Editor", BackgroundColor = DynastyTheme.Background };
        var name = new Entry { Text = source.Name + " Custom", MaxLength = 60 };
        var approach = new Picker { ItemsSource = Enum.GetNames<DefensiveApproach>(), SelectedIndex = (int)source.Approach };
        var rows = new VerticalStackLayout { Spacing = 8 };
        var assignments = source.Assignments.ToList();
        var positions = PersonnelRotationService.DefensivePositions(source.Personnel);
        var slots = positions.Select((position, index) => new DynastyPlayer { PlayerId = new Guid(index + 1, 0, 0, new byte[8]),
            FullName = "Editor", TeamName = "Editor", Position = position }).ToArray();
        var starts = DefensivePursuitService.Alignments(slots, source with { Assignments = [] });
        var counts = new Dictionary<Position, int>();
        var slotIndex = 0;
        foreach (var position in positions)
        {
            var role = counts.GetValueOrDefault(position); counts[position] = role + 1;
            var initial = assignments.FirstOrDefault(a => a.Position == position && a.RoleIndex == role) ?? new PlayAssignment
            {
                Position = position, RoleIndex = role, Depth = position is Position.DE or Position.DT ? 1 : 6,
                Lane = Math.Clamp(.15 + rows.Count * .07, .05, .95), Task = position is Position.DE or Position.DT ? AssignmentTask.Rush : AssignmentTask.Zone
            };
            var alignment = starts[slots[slotIndex++].PlayerId];
            initial = initial with { Depth = alignment.X, Lane = alignment.Y };
            assignments.RemoveAll(a => a.Position == position && a.RoleIndex == role); assignments.Add(initial);
            var task = new Picker { ItemsSource = new[] { "Rush", "Man", "Zone" }, SelectedIndex = initial.Task == AssignmentTask.Rush ? 0 : initial.Task == AssignmentTask.Man ? 1 : 2 };
            var label = Text("", 12, true);
            void Update()
            {
                assignments.RemoveAll(a => a.Position == position && a.RoleIndex == role);
                assignments.Add(initial with { Depth = alignment.X, Lane = alignment.Y, Task = (AssignmentTask)(task.SelectedIndex + 2) });
                label.Text = $"{position} {role + 1} · package alignment locked";
            }
            task.SelectedIndexChanged += (_, _) => Update(); Update();
            rows.Children.Add(new VerticalStackLayout { Spacing = 3, Children = { label, task } });
        }
        var save = Action("Save to library", async () =>
        {
            completion.TrySetResult(source with { Id = "custom-" + Guid.NewGuid().ToString("N"),
                Name = string.IsNullOrWhiteSpace(name.Text) ? source.Name : name.Text.Trim(),
                Approach = (DefensiveApproach)approach.SelectedIndex, Assignments = assignments.ToArray() });
            await Navigation.PopModalAsync();
        });
        var cancel = Action("Cancel", async () => { completion.TrySetResult(null); await Navigation.PopModalAsync(); });
        page.Disappearing += (_, _) => Dispatcher.Dispatch(() => { if (!Navigation.ModalStack.Contains(page)) completion.TrySetResult(null); });
        page.Content = new ScrollView { Content = new VerticalStackLayout { Padding = 14, Spacing = 10, Children =
        {
            cancel, EditorHeading("Defensive Play Editor", "Assign rush, man coverage or zone responsibility to each package slot."),
            name, approach, save, rows
        } } };
        await Navigation.PushModalAsync(page);
        return await completion.Task;
    }

    async Task EditCoachAppearance(ProCoach original)
    {
        if (state is null || state.League.Teams.FirstOrDefault(t => t.Id == original.TeamId)?.Name != state.UserTeam) return;
        var current = state!.Coaches.FirstOrDefault(c => c.TeamId == original.TeamId && c.Role == original.Role && c.Name == original.Name) ?? original;
        var appearance = current.Appearance ?? ProCoachPortraits.Find(current.Name)?.Appearance ?? new NcaaPlayerAppearance();
        var staffAppearance = PortraitAppearance.ForStaff(CoachId(current), Math.Max(30, current.Age), appearance);
        var tone = new Picker { Title = "Skin tone", ItemsSource = new[] { "Current portrait", "Light", "Light medium", "Medium", "Tan", "Brown", "Dark brown", "Deep" }, SelectedIndex = staffAppearance.Skin + 1 };
        var hair = new Picker { Title = "Hair color", ItemsSource = new[] { "Automatic", "Black", "Blonde", "Brown", "Red", "Light brown", "Gray" }, SelectedIndex = (appearance.HairColor ?? -1) + 1 };
        var style = new Picker { Title = "Hair style", ItemsSource = new[] { "Automatic", "Bald", "Cornrows", "Afro", "Flat top", "Buzz cut", "Fade", "Balding", "Close crop", "Bald (hidden)", "Bald 2", "Balding 2", "Buzz cut 2", "Fade 2", "Mullet", "Dreadlocks" }, SelectedIndex = (appearance.HairStyle ?? -1) + 1 };
        var beard = new Picker { Title = "Facial hair", ItemsSource = new[] { "Automatic", "Clean shaven", "Stubble", "Goatee", "Beard" }, SelectedIndex = (appearance.FacialHair ?? -1) + 1 };
        NcaaPlayerAppearance Selected() => appearance with
        {
            SkinTone = tone.SelectedIndex <= 0 ? appearance.SkinTone : tone.SelectedIndex - 1,
            SkinColorHex = tone.SelectedIndex <= 0 ? appearance.SkinColorHex : null,
            HairColor = hair.SelectedIndex <= 0 ? null : hair.SelectedIndex - 1,
            HairStyle = style.SelectedIndex <= 0 ? null : style.SelectedIndex - 1,
            FacialHair = beard.SelectedIndex <= 0 ? null : beard.SelectedIndex - 1
        };
        var preview = new ContentView();
        var team = state.League.Teams.Single(t => t.Id == current.TeamId);
        void Refresh()
        {
            var shown = current with { Appearance = Selected() };
            preview.Content = new HorizontalStackLayout { Spacing = 12, Children =
            {
                PersonPortrait.Create(CoachId(shown), shown.Name, Color.FromArgb(team.Primary), Color.FromArgb(team.Secondary), 120, staff: true, age: shown.Age, appearance: shown.Appearance),
                Text(shown.Name, 20, true)
            } };
        }
        tone.SelectedIndexChanged += (_, _) => Refresh(); hair.SelectedIndexChanged += (_, _) => Refresh();
        style.SelectedIndexChanged += (_, _) => Refresh(); beard.SelectedIndexChanged += (_, _) => Refresh(); Refresh();
        await ShowPage("Edit coach portrait", preview, Text("Skin tone"), tone, Text("Hair color"), hair,
            Text("Hair style"), style, Text("Facial hair"), beard,
            Action("Save portrait", async () =>
            {
                var updated = current with { Appearance = Selected() };
                state = state! with { Coaches = state.Coaches.Select(c => c.TeamId == current.TeamId && c.Role == current.Role && c.Name == current.Name ? updated : c).ToList() };
                await Save();
                await Navigation.PopModalAsync();
                // Replace the old profile so Back returns to the originating directory.
                await Navigation.PopModalAsync();
                Render();
                await CoachProfile(updated);
            }));
    }
}

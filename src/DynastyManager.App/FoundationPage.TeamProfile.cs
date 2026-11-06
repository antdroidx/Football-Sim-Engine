using Football.Presentation;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace DynastyManager.App;
public sealed partial class FoundationPage
{
    private async Task ShowCollegeScoutProfileAsync(ScoutStaff scout)
    {
        if (_currentDynasty is null) return;
        scout=_currentDynasty.ScoutingStaff.FirstOrDefault(s=>s.ScoutId==scout.ScoutId)??scout;
        var assignment=_currentDynasty.ScoutAssignments.FirstOrDefault(a=>a.ScoutId==scout.ScoutId);
        var back=new Button{Text="Back"};
        back.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        await Navigation.PushModalAsync(new ContentPage{Title="Scout profile",AutomationId="college-scout-profile",BackgroundColor=DynastyTheme.Background,
            Content=new ScrollView{Content=new VerticalStackLayout{Padding=16,Spacing=12,Children={back,
                new Label{Text=scout.FullName,FontSize=22,TextColor=DynastyTheme.Text},
                new Label{Text=scout.Age>0?$"Age {scout.Age} · {scout.CareerYears} career years · {scout.TenureYears} with this program":"",TextColor=DynastyTheme.Text},
                new Label{Text=assignment is null?"Unassigned":FormatScoutAssignment(assignment),TextColor=DynastyTheme.Text},
                CoachAttributeViews.Scouting(ScoutAttributes.FromLegacy(scout))}}}});
    }

    private async Task ShowCollegeTeamProfileAsync(string teamName)
    {
        if (_currentDynasty is null || !_teamsByName.TryGetValue(teamName,out var team)) return;
        var state=_currentDynasty;
        var prestige=ProgramPrestigeService.GetCurrentPrestige(state,team);
        var academics=TeamAcademicQualityCatalog.Get(team);
        var profile=DynastyRosterSimulationProfileBuilder.Build(state,[team])[team.Name];
        var plan=GamePlanService.GetPlan(state,profile);
        var staff=CollegeStaffDirectory.Team(state,team.Name).ToArray();
        Label T(string value,bool bold=false)=>new(){Text=value,FontSize=bold?14:12,FontAttributes=bold?FontAttributes.Bold:FontAttributes.None,TextColor=bold?DynastyTheme.Text:DynastyTheme.Muted};
        View Box(string title,params View[] views)
        {
            var stack=new VerticalStackLayout{Spacing=8};
            foreach(var view in views) stack.Children.Add(view);
            return ManagementPanel(title,stack);
        }
        View ChemistrySummary()
        {
            var record=CollegeChemistryService.Record(state,team.Name,state.SeasonYear);
            var stack=new VerticalStackLayout{Spacing=4,AutomationId="college-chemistry-note"};
            if(record is null)
            {
                stack.Children.Add(T("No season culture record yet. Culture values follow the current roster and staff."));
                return stack;
            }
            var prior=CollegeChemistryService.Record(state,team.Name,state.SeasonYear-1);
            stack.Children.Add(T(prior is null
                ? "First recorded season"
                : $"{record.Cohesion-prior.Cohesion:+0;-0;0} vs last season · {TeamChemistrySeasonService.MomentumLabel(record.Momentum)}",true));
            if(record.CaptainNames.Length>0)stack.Children.Add(T($"Captains: {string.Join(", ",record.CaptainNames)}"));
            foreach(var note in record.Notes)stack.Children.Add(T(note));
            return stack;
        }
        View LockerRoomSummary()
        {
            var notes=state.LockerRoomEvents.Where(item=>item.TeamName.Equals(team.Name,StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item=>item.SeasonYear).ThenByDescending(item=>item.Week).Take(6).ToArray();
            if(notes.Length==0)return T("No locker-room notes have been recorded yet.");
            var stack=new VerticalStackLayout{Spacing=4};
            foreach(var item in notes)stack.Children.Add(T($"{LockerRoomEventService.Label(item.Kind)} · {item.Headline} — {item.Detail}"));
            return stack;
        }
        View RivalrySummary()
        {
            var rivalries=RivalryRecordService.For(state.Rivalries,team.Name);
            var stack=new VerticalStackLayout{Spacing=4,AutomationId="college-rivalry-series"};
            if(rivalries.Count==0)
            {
                stack.Children.Add(T("No rivalry series are recorded for this program yet."));
                return stack;
            }
            foreach(var rivalry in rivalries)
                stack.Children.Add(T($"{RivalryRecordService.Opponent(rivalry,team.Name)} · {RivalryRecordService.Series(rivalry,team.Name)}",true));
            return stack;
        }
        View ScoutDirectory()
        {
            if(!team.Name.Equals(state.UserTeamName,StringComparison.OrdinalIgnoreCase))
                return T("See the chief scout in Staff above.");
            if(state.ScoutingStaff.Count==0) return T("No scouting department records are available.");
            var rows=new VerticalStackLayout{Spacing=8};
            foreach(var scout in state.ScoutingStaff)
            {
                var assignment=state.ScoutAssignments.FirstOrDefault(a=>a.ScoutId==scout.ScoutId);
                rows.Children.Add(new Border{StrokeThickness=0,Padding=10,BackgroundColor=DynastyTheme.Palette("#1B2B37"),
                    Content=new VerticalStackLayout{Spacing=4,Children={
                        StaffDirectoryComponents.Name(scout.ScoutId,scout.FullName,(name,open)=>PersonLink(name,open,true),id=>ShowCollegeScoutProfileAsync(_currentDynasty?.ScoutingStaff.FirstOrDefault(s=>s.ScoutId==id)??scout)),
                        T(assignment is null?"Unassigned":FormatScoutAssignment(assignment))}}});
            }
            return rows;
        }
        View RatingBar(string label,int rating)=>StaffProfileComponents.RatingBar(label,rating,"college-"+label.ToLowerInvariant()+"-bar");
        var close=new Button{Text="Close"};
        close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        var content=new VerticalStackLayout{Padding=16,Spacing=12,Children={
            close, SchoolBanner(team.Name,team.ConferenceName),
            Box("Program identity",
                RatingBar("Prestige",prestige), RatingBar("Academics",academics),
                T("Prestige changes with results and recruiting. Academics matters most to recruits with a strong education preference."),
                T($"{GamePlanService.Label(plan.Offense)} offense · {GamePlanService.Label(plan.Defense)} defense · {GamePlanService.Label(plan.Approach)} approach",true)),
            Box("Staff",staff.Length==0?T("No staff records are available."):DataTable([("Role",170),("Name",210),("Age",55),("Overall",75)],staff.Select(item=>new View[]{
                TableText(FormatStaffRole(item.Role)),CoachLink(item),TableText(item.Age.ToString()),TableText(item.OverallRating.ToString(),true)}))),
            Box("Coach performance audit",staff.Length==0?T("No coaching records are available."):DataTable([("Role",170),("Name",210),("Expected",75),("Record",75),("Change",75),("Audit",160)],CollegeCoachPerformanceService.AuditLeague(state,[team]).Select(audit=>new View[]{
                TableText(FormatStaffRole(audit.Role)), CoachLink(state.Staff.First(item=>item.StaffId==audit.StaffId)),
                TableText(audit.ExpectedWins.ToString()), TableText($"{audit.Wins}-{audit.Losses}"),
                TableText($"{audit.PerformanceChange:+0;-0;0}"), TableText(audit.Summary)}))),            Box("Scouting department",ScoutDirectory()),
            Box("Team ratings",T($"Offense {profile.PassOffenseRating:0} pass / {profile.RushOffenseRating:0} run · Defense {profile.PassDefenseRating:0} pass / {profile.RushDefenseRating:0} run · Special teams {profile.SpecialTeamsRating:0}",true)),
            Box("Team chemistry",T($"Cohesion {profile.Cohesion} / 100 · Scheme familiarity {profile.SchemeFamiliarity} / 100",true),

                new Label{Text=TeamMotivationService.Label(state.TeamMotivation.Where(item=>item.TeamName.Equals(team.Name,StringComparison.OrdinalIgnoreCase)).OrderByDescending(item=>item.SeasonYear).FirstOrDefault()),AutomationId="college-team-motivation",FontSize=12,TextColor=DynastyTheme.Muted},
                ChemistrySummary(),
                LockerRoomSummary()),
            Box("Rivalries",RivalrySummary())
        }};
        await Navigation.PushModalAsync(new ContentPage{Title=team.Name+" profile",AutomationId="college-team-profile-page",BackgroundColor=DynastyTheme.Background,Content=new ScrollView{Content=content}});
    }
}

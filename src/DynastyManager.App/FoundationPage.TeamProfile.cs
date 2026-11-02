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
        var staff=state.Staff.Where(item=>item.TeamName.Equals(team.Name,StringComparison.OrdinalIgnoreCase)).OrderBy(item=>item.Role).ToArray();
        Label T(string value,bool bold=false)=>new(){Text=value,FontSize=bold?14:12,FontAttributes=bold?FontAttributes.Bold:FontAttributes.None,TextColor=bold?DynastyTheme.Text:DynastyTheme.Muted};
        View Box(string title,params View[] views)
        {
            var stack=new VerticalStackLayout{Spacing=8};
            foreach(var view in views) stack.Children.Add(view);
            return ManagementPanel(title,stack);
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
                        PersonLink(scout.FullName,()=>ShowCollegeScoutProfileAsync(scout),true),
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
            Box("Scouting department",ScoutDirectory()),
            Box("Team ratings",T($"Offense {profile.PassOffenseRating:0} pass / {profile.RushOffenseRating:0} run · Defense {profile.PassDefenseRating:0} pass / {profile.RushDefenseRating:0} run · Special teams {profile.SpecialTeamsRating:0}",true))
        }};
        await Navigation.PushModalAsync(new ContentPage{Title=team.Name+" profile",AutomationId="college-team-profile-page",BackgroundColor=DynastyTheme.Background,Content=new ScrollView{Content=content}});
    }
}

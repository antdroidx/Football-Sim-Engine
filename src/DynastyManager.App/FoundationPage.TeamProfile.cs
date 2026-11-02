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
                new Label{Text=scout.Age>0?$"Age {scout.Age} · {scout.CareerYears} career years · {scout.TenureYears} with this program":"Age and career history not recorded",TextColor=DynastyTheme.Text},
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
        View RatingBar(string label,int rating)=>new VerticalStackLayout{Spacing=4,Children={
            T($"{label} {rating}",true),new ProgressBar{Progress=Math.Clamp(rating/99d,0,1),ProgressColor=DynastyTheme.Accent,HeightRequest=8,AutomationId="college-"+label.ToLowerInvariant()+"-bar"}}};
        var close=new Button{Text="Close"};
        close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        var content=new VerticalStackLayout{Padding=16,Spacing=12,Children={
            close, SchoolBanner(team.Name,team.ConferenceName),
            Box("Program identity",
                RatingBar("Prestige",prestige), RatingBar("Academics",academics),
                T("Prestige changes with results and recruiting. Academic quality is a stable institutional measure and matters most to recruits with a strong education preference."),
                T($"{GamePlanService.Label(plan.Offense)} offense · {GamePlanService.Label(plan.Defense)} defense · {GamePlanService.Label(plan.Approach)} approach",true)),
            Box("Staff",staff.Length==0?T("No staff records are available."):DataTable([("Role",170),("Name",210),("Age",55),("Overall",75)],staff.Select(item=>new View[]{
                TableText(FormatStaffRole(item.Role)),CoachLink(item),TableText(item.Age.ToString()),TableText(item.OverallRating.ToString(),true)}))),
            Box("Scouting department",team.Name.Equals(state.UserTeamName,StringComparison.OrdinalIgnoreCase)
                ? state.ScoutingStaff.Count==0 ? T("No scouting department records are available.")
                : DataTable([("Scout",210),("Age",55),("Assignment",180)],state.ScoutingStaff.Select(scout=>new View[]{
                    PersonLink(scout.FullName,()=>ShowCollegeScoutProfileAsync(scout),true),TableText(scout.Age>0?scout.Age.ToString():"—"),
                    TableText(state.ScoutAssignments.FirstOrDefault(a=>a.ScoutId==scout.ScoutId) is {} assignment ? FormatScoutAssignment(assignment) : "Unassigned")}))
                : T("Only recorded chief-scout information is available for this program; see Staff above.")),
            Box("Team ratings",T($"Offense {profile.PassOffenseRating:0} pass / {profile.RushOffenseRating:0} run · Defense {profile.PassDefenseRating:0} pass / {profile.RushDefenseRating:0} run · Special teams {profile.SpecialTeamsRating:0}",true))
        }};
        await Navigation.PushModalAsync(new ContentPage{Title=team.Name+" profile",AutomationId="college-team-profile-page",BackgroundColor=DynastyTheme.Background,Content=new ScrollView{Content=content}});
    }
}

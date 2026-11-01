using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace DynastyManager.App;
public sealed partial class FoundationPage
{
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
        var close=new Button{Text="Close"};
        close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        var content=new VerticalStackLayout{Padding=16,Spacing=12,Children={
            close, SchoolBanner(team.Name,$"{team.ConferenceName} · Prestige {prestige}/99 · Academics {academics}/99"),
            Box("Program identity",
                T($"Prestige {prestige}/99 · Academics {academics}/99",true),
                T("Prestige changes with results and recruiting. Academic quality is a stable institutional measure and matters most to recruits with a strong education preference."),
                T($"{GamePlanService.Label(plan.Offense)} offense · {plan.Defense} defense · {plan.Approach} approach",true)),
            Box("Staff",staff.Length==0?T("No staff records are available."):DataTable([("Role",170),("Name",210),("Age",55),("Overall",75)],staff.Select(item=>new View[]{
                TableText(FormatStaffRole(item.Role)),TableText(item.FullName),TableText(item.Age.ToString()),TableText(item.OverallRating.ToString(),true)}))),
            Box("Team ratings",T($"Offense {profile.PassOffenseRating:0} pass / {profile.RushOffenseRating:0} run · Defense {profile.PassDefenseRating:0} pass / {profile.RushDefenseRating:0} run · Special teams {profile.SpecialTeamsRating:0}",true))
        }};
        await Navigation.PushModalAsync(new ContentPage{Title=team.Name+" profile",AutomationId="college-team-profile-page",BackgroundColor=DynastyTheme.Background,Content=new ScrollView{Content=content}});
    }
}

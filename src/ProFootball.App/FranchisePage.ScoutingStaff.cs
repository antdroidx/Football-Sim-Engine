using DynastyManager.App;
using DynastyManager.Core.Models;
using ProFootball.Core;
namespace ProFootball.App;
public sealed partial class FranchisePage
{
    Task ScoutingStaff()
    {
        state=ProScoutDepartment.Ensure(state!);
        var scouts=ProScoutDepartment.Team(state,state.UserTeam);
        var coachTalent=ProTeamStrategyCatalog.CoachingTalentEvaluation(state,state.UserTeam); var scoutQuality=ProScoutDepartment.Rating(state,state.UserTeam); return ShowPage("Scouting staff",Text("Regional and national scouts collaborate on draft reports. Stronger evaluation, communication and knowledge improve confidence in overall and potential ratings. The coaching staff's Talent Evaluation adds 25% of the organization-wide prediction quality.",12),Text($"Organization talent evaluation {ProScouting.StaffRating(state,state.UserTeam)}/100 · scouts {scoutQuality} · coaching staff {coachTalent}",13,true),
            Table([("Scout",170),("Age",55),("Coverage",90),("Evaluation",78),("Communication",100),("Regional Knowledge",125),("National Knowledge",125)],scouts.Select(s=>new View[]{
                Link(s.Staff.FullName,()=>ShowPage("Scout profile",Text(s.Staff.FullName,22,true),Text($"Age {s.Staff.Age} · {s.Staff.CareerYears} career years · {s.Staff.TenureYears} with this club"),Text(s.Regional?"Regional coverage":"National coverage"),CoachAttributeViews.Scouting(ScoutAttributes.FromLegacy(s.Staff)))),
                Text(s.Staff.Age.ToString()),Text(s.Regional?"Regional":"National"),Text(ScoutAttributes.FromLegacy(s.Staff).Evaluation.ToString()),Text(ScoutAttributes.FromLegacy(s.Staff).Communication.ToString()),
                Text(ScoutAttributes.FromLegacy(s.Staff).RegionalKnowledge.ToString()),Text(ScoutAttributes.FromLegacy(s.Staff).NationalKnowledge.ToString())})));
    }
}

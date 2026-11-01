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
        return ShowPage("Scouting staff",Text("Regional and national scouts collaborate on draft reports. Stronger evaluation, communication and knowledge improve confidence in estimated player ratings.",12),
            Table([("Scout",190),("Coverage",100),("Evaluation",85),("Communication",110),("Regional Knowledge",135),("National Knowledge",135)],scouts.Select(s=>new View[]{
                Link(s.Staff.FullName,()=>ShowPage("Scout profile",Text(s.Staff.FullName,22,true),Text(s.Regional?"Regional coverage":"National coverage"),CoachAttributeViews.Scouting(ScoutAttributes.FromLegacy(s.Staff)))),
                Text(s.Regional?"Regional":"National"),Text(ScoutAttributes.FromLegacy(s.Staff).Evaluation.ToString()),Text(ScoutAttributes.FromLegacy(s.Staff).Communication.ToString()),
                Text(ScoutAttributes.FromLegacy(s.Staff).RegionalKnowledge.ToString()),Text(ScoutAttributes.FromLegacy(s.Staff).NationalKnowledge.ToString())})));
    }
}

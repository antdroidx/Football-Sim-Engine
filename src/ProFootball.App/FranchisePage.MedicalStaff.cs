using DynastyManager.App;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
namespace ProFootball.App;
public sealed partial class FranchisePage
{
    static string MedicalMappings => "Availability · overall Medical staff quality affects prevention and recovery. Development · Training, Conditioning and Nutrition support player growth.";
    Task MedicalStaff()
    {
        state = ProMedicalStaffDepartment.Ensure(state!);
        var staff = ProMedicalStaffDepartment.Team(state, state.UserTeam);
        return ShowPage("Medical staff",
            Text("The medical director leads prevention and recovery. The strength and conditioning director adds recovery support and drives development through Training, Conditioning and Nutrition.", 12),
            Table([("Staff",160),("Age",55),("Role",140),("Medical",80),("Training",85),("Conditioning",100),("Prevention",90),("Nutrition",80)],
                staff.Select(item =>
                {
                    var attributes = StaffAttributeService.Medical(item.Staff);
                    return new View[]
                    {
                        Link(item.Staff.FullName, () => MedicalProfile(item)),
                        Text(item.Staff.Age.ToString()),
                        Text(RoleName(item.Staff.Role)),
                        Text(attributes.Medical.ToString()),
                        Text(attributes.Training.ToString()),
                        Text(attributes.Conditioning.ToString()),
                        Text(attributes.Prevention.ToString()),
                        Text(attributes.Nutrition.ToString())
                    };
                })));
    }
    Task MedicalProfile(ProMedicalStaff original)
    {
        var current = state!.MedicalStaff.FirstOrDefault(item => item.Staff.StaffId == original.Staff.StaffId) ?? original;
        var attributes = StaffAttributeService.Medical(current.Staff);
        var edit = Action("Edit attributes", () =>
        {
            if (current.Team != state!.UserTeam) return Task.CompletedTask;
            editorEntity = "Medical · " + RoleName(current.Staff.Role) + " · " + current.Staff.FullName;
            return Editor();
        });
        edit.IsVisible = current.Team == state!.UserTeam;
        return ShowPage("Medical staff profile",
            Text(current.Staff.FullName, 22, true), Text($"Age {current.Staff.Age} · {current.Staff.CareerYears} career years · {current.Staff.TenureYears} with this club"), Text(RoleName(current.Staff.Role)),
            CoachAttributeViews.Medical(attributes),
            Panel("How these ratings are used", Text(MedicalMappings, 12)),
            Text($"Staff overall {attributes.Overall()}/100 · development strength {(attributes.Training + attributes.Conditioning + attributes.Nutrition) / 3}/100", 13, true),
            edit);
    }
}

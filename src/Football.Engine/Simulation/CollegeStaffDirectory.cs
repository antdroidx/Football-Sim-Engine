using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class CollegeStaffDirectory
{
    public static IReadOnlyList<StaffMember> Team(DynastyState state, string teamName) =>
        Enum.GetValues<StaffRole>().Select(role => StaffManagementService.GetStaff(state, teamName, role))
            .OfType<StaffMember>().ToArray();
}

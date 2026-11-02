using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;
namespace DynastyManager.Core.Tests;
[Trait("Owner","College")]
[Trait("Category","Fast")]
public sealed class CollegeStaffDirectoryTests
{
    [Theory]
    [InlineData(StaffRole.HeadCoach)] [InlineData(StaffRole.OffensiveCoordinator)] [InlineData(StaffRole.DefensiveCoordinator)]
    public void UserCoachFillsTheirRoleWithoutDuplicatingAStoredRecord(StaffRole role)
    {
        var user=new StaffMember {FullName="User",TeamName="Home",Role=role};
        var stored=Enum.GetValues<StaffRole>().Select(r=>new StaffMember {FullName=r.ToString(),TeamName="Home",Role=r}).ToArray();
        var opponent=new StaffMember {FullName="Opponent",TeamName="Away",Role=StaffRole.HeadCoach};
        var s=new DynastyState {DynastyName="Directory",UserTeamName="Home",UserCoachRole=role,UserHeadCoach=user,Staff=stored.Append(opponent).ToArray()};
        var directory=CollegeStaffDirectory.Team(s,"Home");
        Assert.Equal(7,directory.Count);Assert.Equal(user,Assert.Single(directory,c=>c.Role==role));
        Assert.Equal(opponent,Assert.Single(CollegeStaffDirectory.Team(s,"Away")));
        Assert.Equal(user,Assert.Single(CollegeStaffDirectory.Team(s with {Staff=stored.Where(c=>c.Role!=role).ToArray()},"Home"),c=>c.Role==role));
    }
}

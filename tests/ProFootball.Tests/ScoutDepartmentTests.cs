using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;
namespace ProFootball.Tests;
public class ScoutDepartmentTests
{
    [Fact] public async Task LegacyFranchiseGetsStableScoutsWithoutLosingReportsAndSavesThem()
    {
        var s=ProLeagueService.Create("Seattle Seahawks",generatedPlayers:true,applyOpeningTrades:false) with{Scouts=[]};
        var once=ProScoutDepartment.Ensure(s);var twice=ProScoutDepartment.Ensure(s);
        Assert.Equal(s.League.Teams.Count*3,once.Scouts.Count);
        Assert.Equal(once.Scouts,twice.Scouts);
        Assert.Same(once,ProScoutDepartment.Ensure(once));
        Assert.Equal(s.Scouting,once.Scouting);Assert.Equal(s.Players,once.Players);
        var scout=once.Scouts.First();
        once=ProScoutDepartment.Ensure(once with{Scouts=once.Scouts.Select(c=>c==scout?c with{Staff=c.Staff with{Attributes=new(){Evaluation=55,Communication=83,RegionalKnowledge=94,NationalKnowledge=38}}}:c).ToList()});
        var coach=once.Coaches.First();
        once=once with{Coaches=once.Coaches.Select(c=>c==coach?ProStaff.Apply(c,new(){OffensePlaycalling=55,DefenseScheme=97,OffenseDevelopment=91,DefenseDevelopment=32}):c).ToList()};
        var dir=Path.Combine(Path.GetTempPath(),"scout-department-"+Guid.NewGuid());
        try
        {
            var store=new ProSaveStore(dir);await store.Save(once);var loaded=await store.Load(once.Id);
            Assert.Equal(once.Scouts,loaded.Scouts);
            Assert.Equal(once.Coaches.First().Coaching,loaded.Coaches.First().Coaching);
            Assert.Equal(55,ProStaff.Profile(loaded.Coaches.First()).Offense);
        }
        finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
    }
    [Fact] public void NewCoachingAttributesKeepUnitDevelopmentIndependent()
    {
        var attributes=new CoachingAttributes{Development=60,OffenseDevelopment=95,DefenseDevelopment=25};
        Assert.True(attributes.DevelopmentFor(Position.QB)>attributes.DevelopmentFor(Position.DE));
    }
}

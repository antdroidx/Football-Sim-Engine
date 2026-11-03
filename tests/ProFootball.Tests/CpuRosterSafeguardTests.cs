using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;
namespace ProFootball.Tests;

[Trait("Category", "Fast")]
public sealed class CpuRosterSafeguardTests
{
    static readonly ProFranchise Template = ProLeagueService.Create("Seattle Seahawks");
    static ProFranchise Setup(int active, long room, int year = 2026)
    {
        var source = Template.Players.First();
        var players = Enumerable.Range(0, active + 40).Select(i => source with {
            Age = 22, Experience = 0, CreditedSeasons = 0, Retired = false, PracticeSquad = false,
            Reserve = false, ReserveInjury = null, RosterStatus = "ACT",
            Football = source.Football with { PlayerId = Guid.Parse($"72000000-0000-0000-0000-{i+1:000000000000}"),
                TeamName = i < active ? Template.UserTeam : "Free agent", Position = Position.QB,
                OverallRating = i < active ? 65 : 40, CurrentInjury = null } }).ToList();
        var contracts = players.Take(active).Select(p => new ProContract(p.Football.PlayerId, Template.UserTeam,
            year, 1, 0, [new(year, 2_000_000, 0)])).ToList();
        return Template with { Year = year, Phase = ProPhase.Preseason, Players = players, Contracts = contracts,
            SalaryCap = active * 2_000_000L + room, TeamCapAllocations = [], DeadCap = [], Bids = [], Waivers = [],
            RolloverElections = [], FinancialHistory = [] };
    }
    [Fact]
    public void UnfundedCapRepairDoesNotLiquidateTheMinimumRoster()
    {
        var s = Setup(45, 10_000_000);
        s = s with { Players = s.Players.Where(p => p.Football.TeamName == s.UserTeam).ToList() };
        var repaired = ProCpu.MakeRoom(s, s.UserTeam, 50_000_000);
        Assert.Equal(45, ProRoster.Active(repaired, s.UserTeam));
        Assert.Empty(repaired.DeadCap);
        Assert.Equal(10_000_000, ProEconomy.Room(repaired, s.UserTeam));
    }
    [Fact]
    public void CurrentMinimumUsesLegalOneYearDealsDespiteFutureDebt()
    {
        var s=Setup(44,1_250_000);
        s=s with { DeadCap=[new(s.UserTeam,s.Year+1,Guid.NewGuid(),500_000_000)] };
        var next=ProCpuRosterSafety.EnsureMinimum(s,s.UserTeam);
        Assert.Equal(45,ProRoster.Active(next,s.UserTeam));
        var signed=Assert.Single(next.Contracts,c=>!s.Contracts.Contains(c));
        Assert.Single(signed.Years); Assert.Equal(0,signed.SigningBonus);
        Assert.True(ProEconomy.Room(next,s.UserTeam)>=0);
        Assert.Equal(s.DeadCap,next.DeadCap);
    }
    [Fact]
    public void LoggedCapRoomCannotBuyTwentyNine2028MinimumPlayers()
    {
        var s=Setup(16,25_997_183,2028);
        var health=ProCpuRosterSafety.Health(s,s.UserTeam);
        Assert.Equal(28_275_000,health.CheapestFill);
        Assert.Equal(2_277_817,health.FundingShortfall);
        var next=ProCpuRosterSafety.EnsureMinimum(s,s.UserTeam);
        Assert.InRange(ProRoster.Active(next,s.UserTeam),17,44);
        Assert.True(ProEconomy.Room(next,s.UserTeam)>=0);
        Assert.Empty(next.DeadCap);
    }

    [Fact]
    public void DiscretionaryContractDoesNotWorsenAnUnfundedFutureYear()
    {
        var s=Setup(45,10_000_000);
        var player=s.Players.First(p=>p.Football.TeamName=="Free agent");
        Assert.Equal(2,ProCpuBudget.ContractYears(s,s.UserTeam,player,5_000_000));
        s=s with {DeadCap=[new(s.UserTeam,s.Year+1,Guid.NewGuid(),500_000_000)]};
        Assert.Equal(1,ProCpuBudget.ContractYears(s,s.UserTeam,player,5_000_000));
    }

}

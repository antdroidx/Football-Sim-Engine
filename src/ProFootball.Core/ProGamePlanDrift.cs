using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

/// <summary>
/// CPU franchise game plans drift one adjacency step per offseason toward their head coach's personal
/// philosophy once its influence is established. User plans, custom playbooks and plans already aligned with
/// the philosophy never drift, and saved formations, tempo and approach are preserved. The influence gate is
/// the shared MinimumDriftInfluence, so plans begin drifting after roughly two seasons of a scheme-divergent
/// head coach.
/// </summary>
public static class ProGamePlanDrift
{
    public static ProFranchise Apply(ProFranchise s)
    {
        var plans = s.GamePlans.ToList();
        var changed = false;
        foreach (var club in s.League.Teams)
        {
            if (club.Name.Equals(s.UserTeam, StringComparison.OrdinalIgnoreCase)) continue;
            var philosophy = ProRosterPlanService.GetIdentity(s, club.Name).StaffPhilosophy;
            if (philosophy is null || philosophy.Weight < RosterPlanService.MinimumDriftInfluence) continue;
            var index = plans.FindIndex(plan => plan.TeamName.Equals(club.Name, StringComparison.OrdinalIgnoreCase));
            var original = index >= 0 ? plans[index] : ProTeamStrategyCatalog.Get(club.Id).GamePlan;
            var drifted = RosterPlanService.DriftPlan(original, philosophy.Offense, philosophy.Defense);
            if (drifted == original) continue;
            if (index >= 0) plans[index] = drifted; else plans.Add(drifted);
            changed = true;
        }
        return changed ? s with { GamePlans = plans } : s;
    }
}

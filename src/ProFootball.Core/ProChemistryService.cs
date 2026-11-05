using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

/// <summary>
/// Builds the Pro season culture record from recorded franchise state: prior-season continuity, roster turnover,
/// coaching and plan changes, morale and contract friction.
/// </summary>
public static class ProChemistryService
{
    public static ProFranchise Refresh(ProFranchise state)
    {
        if(state.League.Teams.Count==0)return state;

        var completedSeason=state.Year-1;
        var prior=state.TeamChemistry.Where(r=>r.SeasonYear==completedSeason).ToDictionary(r=>r.TeamName,StringComparer.OrdinalIgnoreCase);
        var inputs=new List<TeamChemistryInputs>();
        foreach(var team in state.League.Teams)
            inputs.Add(BuildInputs(state,team.Name,completedSeason));

        var records=inputs.Select(i=>TeamChemistrySeasonService.Advance(prior.GetValueOrDefault(i.TeamName),i)).ToArray();

        var history=state.TeamChemistry.Where(r=>r.SeasonYear!=state.Year).Concat(records)
            .OrderBy(r=>r.SeasonYear).ThenBy(r=>r.TeamName,StringComparer.OrdinalIgnoreCase).ToList();

        var events=LockerRoomEventService.Build(Signals(state,records,prior,completedSeason));

        return state with{
            TeamChemistry=history,
            LockerRoomEvents=state.LockerRoomEvents.Where(e=>e.SeasonYear!=state.Year).Concat(events)
                .OrderBy(e=>e.SeasonYear).ThenBy(e=>e.Week).ThenBy(e=>e.TeamName,StringComparer.OrdinalIgnoreCase).ToList()};
    }

    public static TeamChemistryRecord? Record(ProFranchise state,string teamName,int seasonYear)=>
        state.TeamChemistry.FirstOrDefault(r=>r.SeasonYear==seasonYear&&r.TeamName.Equals(teamName,StringComparison.OrdinalIgnoreCase));

    static TeamChemistryInputs BuildInputs(ProFranchise state,string teamName,int completedSeason)
    {
        var profile=ProLeagueService.Profile(state,teamName);
        var club=state.League.Teams.Single(t=>t.Name==teamName);
        var roster=state.Players.Where(p=>p.Football.TeamName==teamName&&!p.Retired&&!p.Reserve&&!p.PracticeSquad).ToArray();
        var newcomers=Newcomers(state,teamName,completedSeason);
        var baseline=Math.Max(1,roster.Length);
        var turnover=(int)Math.Round(newcomers.Count*100.0/baseline);

        var headCoach=state.Coaches.FirstOrDefault(c=>c.TeamId==club.Id&&c.Role==StaffRole.HeadCoach&&!c.Vacant);
        var plan=state.GamePlans.FirstOrDefault(p=>p.TeamName.Equals(teamName,StringComparison.OrdinalIgnoreCase))
            ??(ProTeamStrategyCatalog.TryGet(club.Id,out var identity)?identity.GamePlan:new TeamGamePlan{TeamName=teamName});
        var captains=TeamLeadershipService.Captains(roster.Select(p=>p.Football).ToArray());
        var names=captains.Select(id=>roster.FirstOrDefault(p=>p.Football.PlayerId==id)?.Football.FullName)
            .Where(n=>!string.IsNullOrWhiteSpace(n)).Select(n=>n!).ToArray();

        var moraleSignal=Math.Clamp(((int)roster.Select(p=>(double)p.Morale).DefaultIfEmpty(60).Average()-60)*2,-100,100);
        var friction=roster.Count(p=>p.TradeRequested||p.ContractConcernWeeks>0);
        var disciplineSignal=Math.Clamp(-friction*10,-100,100);

        return new TeamChemistryInputs{
            TeamName=teamName,
            SeasonYear=state.Year,
            EvaluatedCohesion=profile.Cohesion,
            EvaluatedSchemeFamiliarity=profile.SchemeFamiliarity,
            RosterTurnoverPercent=turnover,
            HeadCoachKey=headCoach?.Name??"",
            PlanKey=$"{plan.Offense}/{plan.Defense}",
            MoraleSignal=moraleSignal,
            DisciplineSignal=disciplineSignal,
            DisciplineNote="Contract friction and trade requests cost the room.",
            CaptainIds=captains.ToArray(),
            CaptainNames=names};
    }

    static IReadOnlyList<Guid> Newcomers(ProFranchise state,string teamName,int completedSeason)
    {
        var drafted=state.DraftHistory.Where(d=>d.Year>completedSeason&&d.Team==teamName).Select(d=>d.Player.Football.PlayerId);
        var traded=state.TradeHistory.Where(t=>t.Year>completedSeason).SelectMany(t=>t.ReceivedPlayers);
        return drafted.Concat(traded).Distinct().ToArray();
    }

    static IReadOnlyList<LockerRoomSignal> Signals(ProFranchise state,IReadOnlyList<TeamChemistryRecord> records,
        IReadOnlyDictionary<string,TeamChemistryRecord> prior,int completedSeason)
    {
        var signals=new List<LockerRoomSignal>();
        foreach(var record in records)
        {
            prior.TryGetValue(record.TeamName,out var previous);
            if(previous is not null)
            {
                var club=state.League.Teams.Single(t=>t.Name==record.TeamName);
                var headCoach=state.Coaches.FirstOrDefault(c=>c.TeamId==club.Id&&c.Role==StaffRole.HeadCoach&&!c.Vacant);
                if(headCoach is not null&&!string.IsNullOrEmpty(previous.HeadCoachKey)
                    &&!previous.HeadCoachKey.Equals(headCoach.Name,StringComparison.Ordinal))
                    signals.Add(new LockerRoomSignal{TeamName=record.TeamName,Kind=LockerRoomEventKind.CoachingChange,
                        Subject=headCoach.Name,SeasonYear=state.Year,Week=0});

                foreach(var captainId in previous.Captains.Where(id=>!record.Captains.Contains(id)))
                    signals.Add(new LockerRoomSignal{TeamName=record.TeamName,Kind=LockerRoomEventKind.CaptainChange,
                        Subject=state.Players.FirstOrDefault(p=>p.Football.PlayerId==captainId)?.Football.FullName,
                        SubjectId=captainId,SeasonYear=state.Year,Week=0});
            }

            if(record.RosterTurnoverPercent>0)
                signals.Add(new LockerRoomSignal{TeamName=record.TeamName,Kind=LockerRoomEventKind.RosterTurnover,
                    Value=record.RosterTurnoverPercent,SeasonYear=state.Year,Week=0});

            foreach(var player in state.Players.Where(p=>p.Football.TeamName==record.TeamName&&!p.Retired&&p.TradeRequested))
                signals.Add(new LockerRoomSignal{TeamName=record.TeamName,Kind=LockerRoomEventKind.Discipline,
                    Subject=player.Football.FullName,SubjectId=player.Football.PlayerId,SeasonYear=state.Year,Week=0,
                    Detail=$"{player.Football.FullName} requested a trade."});

            signals.Add(new LockerRoomSignal{TeamName=record.TeamName,Kind=LockerRoomEventKind.Chemistry,
                Value=record.Cohesion,SeasonYear=state.Year,Week=0});
        }
        return signals;
    }
}
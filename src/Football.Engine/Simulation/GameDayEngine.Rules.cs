using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static partial class GameDayEngine
{
    private static void AwardSafety(GameDaySession session, bool homeOnOffense) =>
        AwardSafetyAgainst(session,homeOnOffense?session.Game.HomeTeamName:session.Game.AwayTeamName,false);

    private static void AwardSafetyAgainst(GameDaySession session,string conceding,bool returnSafety)
    {
        var snap=session.Events[^1];
        var scoring=conceding==session.Game.HomeTeamName?session.Game.AwayTeamName:session.Game.HomeTeamName;
        session.Events[^1]=snap with {IsSafety=true,Points=2,SafetyConcedingTeamName=conceding,
            BallEndYardLine=conceding==snap.OffenseTeamName?0:100,
            Yards=returnSafety?snap.Yards:-snap.StartYardLine};
        if(scoring==session.Game.HomeTeamName)session.HomeScore+=2;else session.AwayScore+=2;
        session.TryPending=false;session.OnsideKickRequested=false;
        session.ClockStopped=true;session.ClockRestartsOnReady=false;session.PlayClockSeconds=25;
        session.KickoffPending=session.SafetyKickPending=false;
        if(session.OvertimeRound>0)
        {
            if(!session.Rules.TimedOvertime && scoring==snap.OffenseTeamName)EndOvertimePossession(session);
            else session.IsComplete=true;
            return;
        }
        session.KickoffTeamName=conceding;session.PossessionTeamName=scoring;
        session.PossessionNumber++;session.Down=1;session.Distance=10;session.YardLine=20;
        session.KickoffPending=session.SafetyKickPending=true;
    }

    private static bool RequiresTry(GameDaySession s) => s.Quarter!=4 || s.ClockSeconds>0 || Math.Abs(s.HomeScore-s.AwayScore)<=2;

    public static bool IsScrimmage(GameSnapEvent snap) => !snap.IsTry && (snap.StatisticalOutcome??snap.Outcome) is SnapOutcome.Run or SnapOutcome.Completion or
        SnapOutcome.Incompletion or SnapOutcome.Sack or SnapOutcome.Turnover or SnapOutcome.Touchdown or
        SnapOutcome.Interception or SnapOutcome.Fumble or SnapOutcome.Scramble or SnapOutcome.Kneel or SnapOutcome.Spike;

    public static bool ShouldAttemptTwo(GameDaySession session)
    {
        if (!session.Rules.TimedOvertime && session.OvertimeRound >= 2) return true;
        var margin = session.PossessionTeamName == session.Game.HomeTeamName
            ? session.HomeScore - session.AwayScore : session.AwayScore - session.HomeScore;
        return session.Quarter == 4 && session.ClockSeconds <= 300 && margin is -8 or -5 or -2 or 1 or 5;
    }

    private static IReadOnlyList<DynastyPlayer> PlaceKickTeam(IReadOnlyList<DynastyPlayer> roster, DynastyPlayer? kicker)
    {
        var eligible = roster.Where(p => p.CurrentInjury is null && !p.IsRedshirted)
            .OrderBy(p => p.DepthChartOrder).ThenByDescending(p => p.OverallRating).ToArray();
        var unit = new List<DynastyPlayer>();
        if (kicker is not null) unit.Add(kicker);
        var holder = eligible.FirstOrDefault(p => p.Position == Position.QB && p.PlayerId != kicker?.PlayerId);
        if (holder is not null) unit.Add(holder);
        unit.AddRange(eligible.Where(p => p.Position == Position.OL).Take(5));
        unit.AddRange(eligible.Where(p => p.Position == Position.TE).Take(2));
        return unit.Concat(eligible).DistinctBy(p => p.PlayerId).Take(11).ToArray();
    }

    private static IReadOnlyList<DynastyPlayer> PlaceKickDefense(IReadOnlyList<DynastyPlayer> roster)
    {
        var eligible = roster.Where(p => p.CurrentInjury is null && !p.IsRedshirted)
            .OrderBy(p => p.DepthChartOrder).ThenByDescending(p => p.OverallRating).ToArray();
        return eligible.Where(p => p.Position is Position.DE or Position.DT or Position.MLB or Position.OLB).Take(8)
            .Concat(eligible.Where(p => p.Position is Position.CB or Position.FS or Position.SS))
            .Concat(eligible).DistinctBy(p => p.PlayerId).Take(11).ToArray();
    }

    private static GameSnapEvent ResolveTry(GameDaySession session, string? choice, string? defensiveChoice = null)
    {
        var forcedTwo = !session.Rules.TimedOvertime && session.OvertimeRound >= 2;
        var two = forcedTwo || (choice is null ? ShouldAttemptTwo(session) : choice != "special-extra-point");
        var home = session.PossessionTeamName == session.Game.HomeTeamName;
        var profile = home ? session.HomeProfile : session.AwayProfile;
        var defense = home ? session.AwayProfile : session.HomeProfile;
        var offenseRoster = home ? session.HomeRoster : session.AwayRoster;
        var defenseRoster = home ? session.AwayRoster : session.HomeRoster;
        var offensePlan = GamePlanService.Normalize(profile.GamePlan ?? new TeamGamePlan { TeamName = profile.TeamName });
        var defensePlan = GamePlanService.Normalize(defense.GamePlan ?? new TeamGamePlan { TeamName = defense.TeamName });
        var kicker = GameDayCoachingService.Specialist(session, session.PossessionTeamName, Kicker(offenseRoster));

        OffensivePlayDefinition? conversionPlay = null;
        DefensivePlayDefinition? conversionDefense = null;
        if (two)
        {
            conversionPlay = forcedTwo || choice is null || choice == "special-two-point"
                ? GamePlanService.RecommendOffensivePlay(offensePlan, 1, 2, 97, session.Random.Next())
                : PlayEditorService.OffensiveLibrary(offensePlan).FirstOrDefault(play => play.Id == choice)
                    ?? PlaybookCatalog.OffensivePlays.FirstOrDefault(play => play.Id == choice)
                    ?? throw new ArgumentException("Offensive play is not available for this try.", nameof(choice));
            conversionDefense = GamePlanService.RecommendDefensivePlay(defensePlan, 1, 2);
        }

        SnapPersonnel offensePersonnel;
        SnapPersonnel defensePersonnel;
        DynastyPlayer[] offensePlayers;
        DynastyPlayer[] defensePlayers;
        if (two)
        {
            offensePersonnel = GameDayCoachingService.ApplySubstitutions(session, PersonnelRotationService.SelectOffense(
                home ? session.HomeDepthChart : session.AwayDepthChart, session.PossessionTeamName,
                conversionPlay!.Personnel, home ? session.HomeFatigue : session.AwayFatigue, session.Random));
            defensePersonnel = GameDayCoachingService.ApplySubstitutions(session, PersonnelRotationService.SelectDefense(
                home ? session.AwayDepthChart : session.HomeDepthChart,
                home ? session.Game.AwayTeamName : session.Game.HomeTeamName,
                conversionDefense!.Personnel, home ? session.AwayFatigue : session.HomeFatigue, session.Random));
            offensePlayers = BackfieldRoles.Assign(conversionPlay, Players(session, offensePersonnel)).ToArray();
            defensePlayers = Players(session, defensePersonnel);
        }
        else
        {
            var offenseUnit = PlaceKickTeam(offenseRoster, kicker);
            var defenseUnit = PlaceKickDefense(defenseRoster);
            offensePlayers = offenseUnit.ToArray();
            defensePlayers = defenseUnit.ToArray();
            offensePersonnel = new SnapPersonnel { TeamName = session.PossessionTeamName, PlayerIds = offenseUnit.Select(player => player.PlayerId).ToArray() };
            defensePersonnel = new SnapPersonnel { TeamName = home ? session.Game.AwayTeamName : session.Game.HomeTeamName, PlayerIds = defenseUnit.Select(player => player.PlayerId).ToArray() };
        }

        var isPass = conversionPlay?.Concept is OffensiveConcept.QuickPass or OffensiveConcept.PlayAction or OffensiveConcept.DeepPass;
        PassingRead? read = null;
        DynastyPlayer? runner = null;
        PursuitDecision? pursuit = null;
        BlockingResolution? runBlocking = null;
        if (two && conversionPlay is not null)
        {
            var quarterback = offensePlayers.FirstOrDefault(player => player.Position == Position.QB);
            if (isPass)
            {
                read = PassingReadService.Read(conversionPlay, conversionDefense!, offensePlayers, defensePlayers, quarterback,
                    profile.OffensiveCoordinatorGameManagement, session.Random);
            }
            else
            {
                runner = BackfieldRoles.Runner(conversionPlay, offensePlayers, session.Random) ?? quarterback;
                runBlocking = BlockMatchupService.Resolve(
                    offensePlayers.Where(player => player.PlayerId != runner?.PlayerId && player.Position is Position.OL or Position.TE or Position.FB).ToArray(),
                    defensePlayers.Where(player => player.Position is Position.DE or Position.DT or Position.MLB or Position.OLB).ToArray(),
                    0, false, session.Random);
                pursuit = DefensivePursuitService.Select(defensePlayers, conversionDefense!, 0, .50,
                    false, false, 0, 0, null, 0, session.Random);
            }
        }

        var advantage = profile.OffenseRating - defense.DefenseRating;
        if (conversionPlay is not null && conversionDefense is not null)
        {
            advantage += GameDayMatchupService.FormationAdvantage(conversionPlay, conversionDefense, isPass) * 4;
            advantage += GameDayMatchupService.PersonnelAdvantage(offensePlayers, defensePlayers, isPass) * 4;
        }
        var probability = two
            ? Math.Clamp(.43 + advantage * .004, .15, .78)
            : GameDayMatchupService.FieldGoalProbability(session.Rules.ExtraPointDistance,
                kicker?.Skills?.KickAccuracy ?? profile.SpecialTeamsRating,
                kicker?.Skills?.KickPower ?? profile.SpecialTeamsRating);
        var made = session.Random.NextDouble() < probability;
        var target = read?.Target;
        var defender = read?.Defender ?? pursuit?.Tackler;
        var snap = new GameSnapEvent
        {
            StatsVersion = 1,
            Sequence = session.Events.Count + 1, PossessionNumber = session.PossessionNumber,
            Quarter = session.Quarter, ClockSeconds = session.ClockSeconds,
            OffenseTeamName = session.PossessionTeamName, DefenseTeamName = home ? session.Game.AwayTeamName : session.Game.HomeTeamName,
            OffensivePlayId = conversionPlay?.Id ?? string.Empty,
            DefensivePlayId = conversionDefense?.Id ?? defensiveChoice ?? string.Empty,
            IsTry = true, StartYardLine = two ? 97 : 117 - session.Rules.ExtraPointDistance,
            BallEndYardLine = made ? 100 : 97, Down = 0, Distance = 3, KickerId = two ? null : kicker?.PlayerId,
            CallSource = choice is null ? PlayCallSource.Cpu : PlayCallSource.User,
            DefensiveCallSource = string.IsNullOrWhiteSpace(defensiveChoice) ? PlayCallSource.Cpu : PlayCallSource.User,
            Outcome = two ? made ? SnapOutcome.TwoPointConversion : SnapOutcome.FailedTwoPointConversion
                : made ? SnapOutcome.ExtraPoint : SnapOutcome.MissedExtraPoint,
            Points = made ? two ? 2 : 1 : 0,
            OffensePersonnel = offensePersonnel.PlayerIds,
            DefensePersonnel = defensePersonnel.PlayerIds,
            QuarterbackId = two ? offensePlayers.FirstOrDefault(player => player.Position == Position.QB)?.PlayerId : null,
            BallCarrierId = two && !isPass ? runner?.PlayerId : null,
            TargetId = two && isPass ? target?.PlayerId : null,
            PrimaryDefenderId = two ? defender?.PlayerId : null,
            CarrierLane = two && !isPass ? .50 : null,
            TackleAttemptIds = pursuit?.AttemptIds ?? [],
            AirYards = made && isPass ? 2 : 0,
            YardsAfterCatch = 0,
            PassReleaseProgress = read is null ? 0 : Math.Clamp(read.ReleaseSeconds / 5, .3, .75),
            TargetSeparation = read?.Separation ?? 0,
            IsDropback = two && isPass,
            Blocks = read?.Blocking.Blocks ?? runBlocking?.Blocks ?? [],
            PressureRushers = read?.Blocking.PressureRushers ?? [],
            SackBlockerId = read?.Blocking.SackBlockerId
        };
        if (!made)
        {
            var returner = defensePlayers.OrderByDescending(player => player.SpeedRating).FirstOrDefault();
            snap = TryReturnService.ResolveFailure(snap with {ReturnerId = returner?.PlayerId,
                QuarterbackId = offensePlayers.FirstOrDefault(player => player.Position == Position.QB)?.PlayerId}, session.Rules,
                new Random(unchecked(session.Game.SimulationSeed * 397 ^ snap.Sequence * 7919 ^ 0x7A1)));
            if (!snap.TurnoverYardLine.HasValue) snap = snap with {ReturnerId = null};
        }
        return ApplyTryResult(session, snap);
    }
    public static GameSnapEvent ApplyTryResult(GameDaySession session,GameSnapEvent snap)
    {
        if(session.IsComplete || !session.TryPending)throw new InvalidOperationException("No try is pending.");
        snap=snap with {IsTry=true};session.Events.Add(snap);
        if(snap.ScoringTeamName==session.Game.HomeTeamName)session.HomeScore+=snap.Points;else session.AwayScore+=snap.Points;
        session.TryPending = false;
        ChangePossession(session, 25);
        if (!session.IsComplete && (session.OvertimeRound == 0 || session.Rules.TimedOvertime)) QueueKickoff(session, snap.OffenseTeamName);
        AdvanceQuarter(session);
        return snap;
    }

    private static GameSnapEvent ClockPlay(GameDaySession session, bool spike, bool userCalled = true)
    {
        if (session.OvertimeRound > 0 && !session.Rules.TimedOvertime) throw new InvalidOperationException("Clock plays are unavailable in untimed overtime.");
        var untimed=session.UntimedDownPending;session.UntimedDownPending=false;
        var home = session.PossessionTeamName == session.Game.HomeTeamName;
        var offense=GameDayCoachingService.ApplySubstitutions(session,PersonnelRotationService.SelectOffense(home?session.HomeDepthChart:session.AwayDepthChart,session.PossessionTeamName,OffensivePersonnel.Twelve,home?session.HomeFatigue:session.AwayFatigue,session.Random));
        var defense=GameDayCoachingService.ApplySubstitutions(session,PersonnelRotationService.SelectDefense(home?session.AwayDepthChart:session.HomeDepthChart,home?session.Game.AwayTeamName:session.Game.HomeTeamName,DefensivePersonnel.BaseFourThree,home?session.AwayFatigue:session.HomeFatigue,session.Random));
        var qb = offense.PlayerIds.Select(id=>session.PlayersById[id]).FirstOrDefault(p=>p.Position==Position.QB);
        var snap = new GameSnapEvent
        {
            StatsVersion = 1,
            Sequence = session.Events.Count + 1, PossessionNumber = session.PossessionNumber, Quarter = session.Quarter,
            ClockSeconds = session.ClockSeconds, OffenseTeamName = session.PossessionTeamName,
            DefenseTeamName = home ? session.Game.AwayTeamName : session.Game.HomeTeamName,
            OffensivePlayId = string.Empty, DefensivePlayId = string.Empty, CallSource = userCalled ? PlayCallSource.User : PlayCallSource.Cpu,
            Down = session.Down, Distance = session.Distance, StartYardLine = session.YardLine,
            BallEndYardLine = session.YardLine - (spike ? 0 : 1), Yards = spike ? 0 : -1,
            QuarterbackId = qb?.PlayerId, BallCarrierId = spike ? null : qb?.PlayerId, IsUntimedDown=untimed,
            PreSnapSeconds = session.LastPreSnapSeconds, LiveBallSeconds = spike ? 2 : 4,
            OffensePersonnel=offense.PlayerIds,DefensePersonnel=defense.PlayerIds,
            Outcome = spike ? SnapOutcome.Spike : SnapOutcome.Kneel
        };
        session.Events.Add(snap);
        ConsumeLiveClock(session, spike ? 2 : 4);
        AdvanceState(session, home, snap.Yards, false, false, spike);
        return session.Events[^1];
    }

    private static GameSnapEvent ResolveOnsideKick(GameDaySession session)
    {
        var kicking = session.KickoffTeamName;
        var recovered = session.Random.NextDouble() < .12;
        var receiving = session.PossessionTeamName;
        var safetyKick = session.SafetyKickPending;
        var start = 100 - (safetyKick ? 20 : session.KickoffFromOwnYardLine);
        var landing = start - 10;
        var kickingRoster = kicking == session.Game.HomeTeamName ? session.HomeRoster : session.AwayRoster;
        var receivingRoster = receiving == session.Game.HomeTeamName ? session.HomeRoster : session.AwayRoster;
        var kicker = GameDayCoachingService.Specialist(session, kicking, Kicker(kickingRoster));
        var snap = new GameSnapEvent
        {
            StatsVersion = 1,
            Sequence = session.Events.Count + 1, PossessionNumber = session.PossessionNumber,
            Quarter = session.Quarter, ClockSeconds = session.ClockSeconds, OffenseTeamName = receiving,
            DefenseTeamName = kicking, OffensivePlayId = "", DefensivePlayId = "", Outcome = SnapOutcome.Kickoff,
            IsOnsideKick = true, KickingTeamRecovered = recovered,
            IsSafetyKick = safetyKick,
            StartYardLine = start, KickLandingYardLine = landing, BallEndYardLine = recovered ? 100 - landing : landing,
            KickerId = kicker?.PlayerId,
            OffensePersonnel = KickoffUnit(session, true).Select(p => p.PlayerId).ToArray(),
            DefensePersonnel = KickoffUnit(session, false).Select(p => p.PlayerId).ToArray()
        };
        session.Events.Add(snap);
        session.PossessionTeamName = recovered ? kicking : receiving;
        session.YardLine = snap.BallEndYardLine; session.Down = 1; session.Distance = 10;
        session.PossessionNumber++; session.KickoffPending = false; session.OnsideKickRequested = false;
        session.SafetyKickPending = false;
        session.KickoffFromOwnYardLine = 35;
        return snap;
    }

    private static GameSnapEvent? TryPreSnapPenalty(
        GameDaySession session,
        PreSnapChoice? preSnap,
        TeamGamePlan offensePlan,
        TeamGamePlan defensePlan,
        string? userPlayId,
        string? userDefensivePlayId,
        string? requestedSpecial)
    {
        if (session.Events.Count == 0) return null;
        var random = new Random(unchecked(session.Game.SimulationSeed * 397 ^ session.Events.Count * 7919));
        if (random.NextDouble() >= .025) return null;
        var offense = random.Next(2) == 0;
        // Field spots use whole yards. Do not turn a half-distance foul into a goal-line crossing.
        var enforced = Math.Min(5, Math.Max(0, (offense ? session.YardLine : 100 - session.YardLine) / 2));
        var yards = offense ? -enforced : enforced;
        var homeOnOffense = session.PossessionTeamName.Equals(session.Game.HomeTeamName, StringComparison.OrdinalIgnoreCase);
        var defenseTeam = homeOnOffense ? session.Game.AwayTeamName : session.Game.HomeTeamName;
        var penalizedTeam = offense ? session.PossessionTeamName : defenseTeam;
        var foul = offense ? "False start" : "Offside";

        var offensePlay = requestedSpecial is null
            ? !string.IsNullOrWhiteSpace(userPlayId)
                ? PlayEditorService.OffensiveLibrary(offensePlan).FirstOrDefault(play => play.Id == userPlayId)
                : GamePlanService.RecommendOffensivePlay(
                    offensePlan, session.Down, session.Distance, session.YardLine,
                    session.Events.Count, null, session.Game.SimulationSeed)
            : null;
        var defensePlay = !string.IsNullOrWhiteSpace(userDefensivePlayId)
            ? PlayEditorService.DefensiveLibrary(defensePlan).FirstOrDefault(play => play.Id == userDefensivePlayId)
            : GamePlanService.RecommendDefensivePlay(defensePlan, session.Down, session.Distance);

        var previewRandom = new Random(unchecked(session.Game.SimulationSeed * 104729 ^ session.Events.Count * 15485863));
        var offenseUnit = GameDayCoachingService.ApplySubstitutions(session, PersonnelRotationService.SelectOffense(
            homeOnOffense ? session.HomeDepthChart : session.AwayDepthChart,
            session.PossessionTeamName,
            offensePlay?.Personnel ?? OffensivePersonnel.Eleven,
            homeOnOffense ? session.HomeFatigue : session.AwayFatigue,
            previewRandom));
        var defenseUnit = GameDayCoachingService.ApplySubstitutions(session, PersonnelRotationService.SelectDefense(
            homeOnOffense ? session.AwayDepthChart : session.HomeDepthChart,
            defenseTeam,
            defensePlay?.Personnel ?? DefensivePersonnel.BaseFourThree,
            homeOnOffense ? session.AwayFatigue : session.HomeFatigue,
            previewRandom));

        var unitIds = offense ? offenseUnit.PlayerIds : defenseUnit.PlayerIds;
        var candidates = unitIds
            .Select(id => session.PlayersById[id])
            .Where(p => p.CurrentInjury is null && !p.IsRedshirted &&
                (offense ? p.Position == Position.OL : p.Position is Position.DE or Position.DT))
            .OrderBy(p => p.DepthChartOrder).Take(5).ToArray();
        var culprit = candidates.Length > 0
            ? candidates.OrderBy(p => (p.Skills?.Discipline ?? 60) + random.Next(0, 30)).First().PlayerId
            : (Guid?)null;

        var snap = new GameSnapEvent
        {
            StatsVersion = 1, PenalizedPlayerId = culprit,
            Sequence = session.Events.Count + 1, PossessionNumber = session.PossessionNumber,
            Quarter = session.Quarter, ClockSeconds = session.ClockSeconds,
            PreSnapSeconds = session.LastPreSnapSeconds,
            OffenseTeamName = session.PossessionTeamName,
            DefenseTeamName = defenseTeam,
            PreSnap = preSnap,
            OffensivePlayId = offensePlay?.Id ?? string.Empty,
            DefensivePlayId = defensePlay?.Id ?? string.Empty,
            CallSource = string.IsNullOrWhiteSpace(userPlayId) ? PlayCallSource.Cpu : PlayCallSource.User,
            DefensiveCallSource = string.IsNullOrWhiteSpace(userDefensivePlayId) ? PlayCallSource.Cpu : PlayCallSource.User,
            Down = session.Down, Distance = session.Distance,
            StartYardLine = session.YardLine, BallEndYardLine = session.YardLine + yards,
            PenalizedTeamName = penalizedTeam, PenaltyName = foul, PenaltyYards = enforced,
            Outcome = SnapOutcome.Penalty, IsPreSnapPenalty = true,
            Description = $"{foul} on {penalizedTeam}: {enforced} yards.",
            OffensePersonnel = offenseUnit.PlayerIds,
            DefensePersonnel = defenseUnit.PlayerIds
        };
        var running = !session.ClockStopped || session.ClockRestartsOnReady;
        session.YardLine += yards; session.Distance -= yards;
        if (session.Distance <= 0) { session.Down = 1; session.Distance = Math.Min(10, 100 - session.YardLine); }
        session.ClockRestartsOnReady = !session.ClockStopped || session.ClockRestartsOnReady;
        session.ClockStopped = true; session.PlayClockSeconds = 25; session.Events.Add(snap);
        ApplyFoulRunoff(session, penalizedTeam, true, running);
        var offenseMargin = session.PossessionTeamName == session.Game.HomeTeamName
            ? session.HomeScore - session.AwayScore
            : session.AwayScore - session.HomeScore;
        if (!offense && session.Quarter is 2 or 4 && session.ClockSeconds <= 120 && offenseMargin <= 0)
            session.ClockRestartsOnReady = false;
        AdvanceQuarter(session);
        return session.Events[^1];
    }
    private static void StartProfessionalOvertime(GameDaySession session)
    {
        session.OvertimeRound = 1; session.Quarter = 5;
        session.ClockSeconds = session.Rules.Postseason ? session.Rules.PlayoffOvertimeSeconds : session.Rules.RegularOvertimeSeconds;
        session.HomeTimeouts = session.AwayTimeouts = session.Rules.Postseason ? 3 : 2;
        session.OvertimePossessions = 0;
        session.OvertimeFirstTeam = session.Random.Next(2) == 0 ? session.Game.HomeTeamName : session.Game.AwayTeamName;
        session.PossessionTeamName = session.OvertimeFirstTeam;
        session.PossessionNumber++; session.YardLine = 25; session.Down = 1; session.Distance = 10;
        QueueKickoff(session, session.OvertimeFirstTeam == session.Game.HomeTeamName ? session.Game.AwayTeamName : session.Game.HomeTeamName);
    }

    private static void StartOvertimeRound(GameDaySession session)
    {
        session.OvertimeRound++;
        session.Quarter = 4 + session.OvertimeRound;
        session.ClockSeconds = 0;
        session.KickoffPending = false;
        session.OvertimePossessions = 0;
        session.OvertimeFirstTeam = session.OvertimeRound % 2 == 1 ? session.Game.AwayTeamName : session.Game.HomeTeamName;
        session.PossessionTeamName = session.OvertimeFirstTeam;
        session.PossessionNumber++;
        session.HomeTimeouts = session.AwayTimeouts = session.OvertimeRound <= 3 ? 1 : 0;
        SetOvertimeSpot(session);
    }

    private static void SetOvertimeSpot(GameDaySession session)
    {
        session.YardLine = session.OvertimeRound >= 3 ? 97 : 75;
        session.Down = 1; session.Distance = session.OvertimeRound >= 3 ? 3 : 10;
        session.TryPending = session.OvertimeRound >= 3;
        session.ClockStopped = true;
    }

    private static void EndOvertimePossession(GameDaySession session)
    {
        session.OvertimePossessions++;
        session.PossessionNumber++;
        if (session.OvertimePossessions == 1)
        {
            session.PossessionTeamName = session.OvertimeFirstTeam == session.Game.HomeTeamName
                ? session.Game.AwayTeamName : session.Game.HomeTeamName;
            SetOvertimeSpot(session);
        }
        else if (session.HomeScore != session.AwayScore) session.IsComplete = true;
        else
        {
            var homeTimeouts = session.HomeTimeouts; var awayTimeouts = session.AwayTimeouts;
            StartOvertimeRound(session);
            // Since 2025, each team has only one timeout total from the third overtime onward.
            if (session.OvertimeRound > 3) { session.HomeTimeouts = homeTimeouts; session.AwayTimeouts = awayTimeouts; }
        }
    }
}

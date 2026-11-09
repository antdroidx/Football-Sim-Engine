# Final near-term completion plan — November 6, 2026

The resumed execution is closing the remaining near-term work in three small batches, followed by the already
authorized release sequence.

| Batch | Scope | Status |
|---|---|---|
| F | Persisted motivation and scout replenishment from the resumed work; CPU coach season history with stable staff identity; Pro person-level story identity | Implemented and focused-tested; 63 Pro, 18 Engine, 24 College and both app compiles passed |
| G | Player-authored social reply plus sourced NCAA/NFL pre-snap rule boundaries | Implemented and focused-tested; 26 Engine, 7 College, 7 Pro and both app compiles passed |
| H | Explicit College/Pro action-level device assertions for social, rivalry, chemistry/motivation, replay highlights and sourced pre-snap hold/effect | Implemented and device-tested on College476 and Pro190 |

After H: the final broad checks and both Debug APK packages passed, the established workflows completed, and the
authorized release sequence published `preview-2026-11-06-roadmap-apks` with verified APKs, checksums and a
manifest. Earlier failed/cancelled exact-APK retries were harness-only and are superseded by Pro195 and Pro202.

## Batch G implementation notes

- Both social feeds expose a save-backed player reply action with stable automation IDs and a themed text prompt.
- Generated media replies and the player's "Your reply" content are separate, and the player reply can be
  replaced or removed without mutating decorative engagement counts.
- `PreSnapChoice` and `FootballRules` encode the sourced one-backfield-player, legal-direction, stationary-player,
  line-of-scrimmage-start and one-second-set boundaries from NCAA Rule 7 and NFL Rule 7.
- The shared live view holds for a full second after the alignment animation before the snap.
- The existing one-yard shift and three-yard motion values are internal catalog/animation bounds only.
- Rivalry records remain simulation-neutral.

## Batch F implementation notes

- `ProCoach` and `ProStaffRetirement` carry optional `StaffId` values with safe defaults for older saves.
- `ProCoachHistoryService` records completed CPU club seasons once, uses stable staff IDs and counts all
  same-conference games while keeping tied games out of conference losses.
- Pro injury, player-of-game, weekly leader, award, all-star and showcase stories carry player identity into
  the shared person-thread service.
- `TeamMotivation` is persisted by both hosts and `ProScoutDepartment.Ensure` maintains exactly three unique
  scouts per club.
- The tie-result fix in `ScheduledGame.WinnerTeamName` is shared model behavior and is covered by the coach
  conference-record regression.
# Roadmap completion plan — November 5, 2026

Derived from `docs/ROADMAP.md` (planning source, November 5, 2026) plus a source audit of
`codex/feature-roadmap-plan` at `8438f41`. Status/testing records remain in `docs/current-work.md`
and `docs/testing-scope.md`. This file is the execution outline for the remaining near-term
workstreams. It does not authorize a release, a merge, or a publication.

## Final state

All five batches are delivered on `codex/feature-roadmap-plan`, and the near-term checkpoint in
`docs/ROADMAP.md` is complete apart from the open items listed there.

| Batch | Commit | Delivered |
|---|---|---|
| A | `3710097` | Season-aware chemistry and locker-room events |
| B | `e0edbbd` | Replay highlights and pre-snap choreography |
| C | `ae4544c` | Persistent coach hall of fame |
| D | `da127ed` | Social interaction, replies and rivalry threads |
| E | `1f9f42c` | Persisted rivalries and rivalry series |

Supporting commits: `ed9973b` and `2bb9176` add the plan itself and correct the hall of fame status,
`13aa0af` and `fbad74e` record the branch validation, `2b204f5` rewrites the live game checks against the
shared GameCast view, and `7feb7ba` ignores the local attachment drops and scratch files.

- The branch is pushed and identical to `origin/codex/feature-roadmap-plan`. No pull request exists.
- The published release baseline is still Pro165 / College454 (`preview-2026-11-04-roster-identity`,
  record `8a62a03`). The branch has not been merged and nothing has been published from it.
- The long-term agenda now lives in `docs/ROADMAP.md`, which is the planning source for everything
  beyond the near-term checkpoint.
## Source audit correction to the roadmap

`docs/ROADMAP.md` states the shared coach Hall of Fame service "is rendered in College center and
Pro honors". It is not. `CoachHallOfFameService.Build` has no production caller; the only callers
are `tests/Football.Engine.Tests/CoachHallOfFameTests.cs`. College's Hall of Fame tab renders
`state.HallOfFame` (players only, `FoundationPage.CollegeCenter.cs:170`) and Pro's tab renders
`ProHallOfFameService.Build` (players only, `FranchisePage.Honors.cs:20`). `ProHallOfFameService.CoachSummaries`
produces the coach summaries that the shared service is supposed to score, but nothing calls it.

`docs/ROADMAP.md` and `docs/IMPLEMENTATION-HANDOFF.md` were corrected to match this audit.

A second audit result drives most of the remaining work: none of `TeamChemistryRecord`,
`RivalryRecord`, or `CoachHallOfFameEntry` is persisted in either save model. `DynastyState`
(`src/Football.Engine/Models/DynastyState.cs`) and `ProFranchise`
(`src/ProFootball.Core/Models.cs`) have no collection for any of them, so every one of these
features is recomputed from scratch on load and cannot carry season history.

## Batch A — Season-aware chemistry and locker-room events

**Status: delivered on `codex/feature-roadmap-plan`.** Team culture is recorded per season and carried
forward, both hosts persist it, and locker-room events are written from recorded state. The season offset
reaches the bounded GameCast modifier, and the long College career cases were run with and without the
change and produce the identical pass/fail set.

Recommended first in the roadmap's own ordering, and the state foundation for Batches C and D.

Current state

- `TeamChemistryService.Evaluate` (`src/Football.Engine/Simulation/TeamChemistryService.cs`) derives
  cohesion and scheme familiarity from one snapshot: captain leadership, team morale, head-coach
  leadership, and coordinator scheme ratings. There is no prior-year input, so cohesion can neither
  build nor decay, and roster turnover, coaching change, and plan continuity cannot influence it.
- `TeamChemistryService.GameShift` applies the bounded ±1.8 adjustment inside
  `GameDayEngine.cs:213`. That bound must not move.
- Callers: `DynastyRosterSimulationProfileBuilder.cs:156` (College) and
  `src/ProFootball.Core/ProLeagueService.cs:123` (Pro).
- UI today shows only the current values: `FoundationPage.TeamProfile.cs:70` and
  `ProFootball.App/FranchisePage.cs:229`.
- College discipline (`DynastyState.DisciplineHistory`) and Pro morale
  (`ProFranchise.MoraleEffects`, `ProPlayer.Morale`) exist but never feed chemistry.

Work

1. Persist chemistry. Add a chemistry collection to both save models with an empty default so
   older saves load without a rewrite. Bump `DynastyState.CurrentSchemaVersion` (currently 26)
   and `ProFranchise.SchemaVersion` (currently 2) with backward-compatible defaults.
2. Add a shared season-advance service that takes the prior record plus season inputs and returns
   the next record: roster turnover, head-coach and coordinator changes, plan continuity, captain
   departures, and league-supplied discipline/morale signals. League rules stay in the hosts.
3. Add shared locker-room events: a deterministic event record derived only from recorded state
   (morale movement, discipline incidents, captain departures, turnover). No invented quotes,
   results, or statistics.
4. Surface prior-season delta, momentum, and locker-room notes in both team profiles.

Acceptance

- Same save identity produces the same chemistry timeline and the same locker-room events.
- Older saves load with empty collections and no data rewrite.
- `GameShift` bounds and existing engine seeds are unchanged.
- Focused Engine, College, and Pro tests pass; packaging deferred to the batch's own build step.

## Batch B — Replay highlights and pre-snap choreography

**Status: delivered on `codex/feature-roadmap-plan`.** The shared engine reads a game's recorded snaps and
returns the moments worth replaying, the live game's Options menu replays the chosen one, and a pre-snap
alignment action now carries a bounded, recorded effect on the play it precedes. League rules own the
shift and motion limits.

Current state

- `BroadcastReplayService` (`src/Football.Engine/Simulation/BroadcastReplayService.cs`) exposes
  three angles (Broadcast, FollowBall, ReverseFollow). `GameCastLivePage.cs:53,77,579` persists the
  selection in `Preferences` and passes it into `GameCastAnimation`.
- `GameCastHighlight` shows touchdown, timeout, and penalty banners. There is no highlight
  selection, and no replay consumes a curated list of moments.
- Pre-snap is presentation-only: `PreSnapCatalog.IsValid` accepts shared ranges (shift ≤ 1 yard,
  motion ≤ 3 yards), `PreSnapChoice` is stamped onto the recorded snap, and the ~900 ms hold shows
  the called formation. No outcome effect, no league-specific legality, no marker choreography.
- Both hosts drive the same shared page: `FoundationPage.GameDay.cs`, `FoundationPage.LiveAdapter.cs`,
  `ProFootball.App/ProLivePage.cs`.

Work

1. Replay highlight selection: derive candidate moments (scoring plays, turnovers, long plays) from
   recorded snaps only, never rerolling simulation, and let the user pick or auto-select. Replays
   must replay recorded state.
2. Pre-snap choreography: animate the recorded shift/motion marker path during the hold, driven by
   the recorded `PreSnapChoice` and rendered in the shared field.
3. League-specific legality behind a host-supplied rule interface so the shared catalog stays
   league-neutral.
4. Play-outcome adaptation: a bounded, explicit effect of the pre-snap choice on the outcome,
   recorded on the snap so replays stay consistent with what was simulated.
5. Device acceptance on the exact new APKs in portrait and landscape.

Acceptance

- Replays never reroll the simulation and match the recorded snap outcome.
- The pre-snap effect is bounded, documented, and visible in the recap.
- Both hosts render the same shared behavior; no duplicated presentation logic.

## Batch C — Persistent Hall of Fame lifecycle

**Status: delivered on `codex/feature-roadmap-plan`.** The shared coach hall of fame is rendered in both hosts,
the induction records persist in each save, eligibility stays a league decision, and every team a coach led is
cited with its seasons and record.

Current state

- `CoachHallOfFameService.Build` scores `CoachCareerSummary` lists but is called only from tests.
  `CoachHallOfFameEntry` is never persisted, so nothing survives a save/load cycle and there is no
  induction event to record.
- College's Hall of Fame section (`FoundationPage.CollegeCenter.cs:170`) and Pro's
  (`FranchisePage.Honors.cs:20`) both render players only.
- Retirement records already exist per host: `DynastyState.StaffRetirements` (College) and
  `ProFranchise.StaffRetirements` (Pro), and the coach-history attribution work already stores
  per-team dated tenures for Pro coaches.

Work

1. Wire the shared coach Hall of Fame into both hosts' Hall of Fame views, keeping the player
   sections and eligibility rules as they are.
2. Persist induction records in both save models (empty default, no legacy rewrite) and make
   induction idempotent by coach id plus induction year.
3. Retirement-specific eligibility, with College and Pro eligibility separated at the league
   boundary, sourced from each host's retirement records.
4. Richer citations and team attribution: cite each team with its seasons instead of only the last
   team on the summary.
5. Lifecycle parity between the College and Pro coach paths.

Acceptance

- An inducted coach survives save/load and stays inducted exactly once.
- Older saves load with empty induction collections.
- Player Hall of Fame eligibility and ordering are unchanged.

## Batch D — News and social interaction layer

**Status: delivered on `codex/feature-roadmap-plan`.** The player's likes, reposts and followed desks persist,
replies restate recorded categories, and rivalry threads come from the recorded head-to-head results.

Planned after Batches A-C because it depends on the rivalry, leadership, and discipline state
those batches produce.

Current state

- `FootballNewsSocialService.Build` derives posts from stories and narrative memories. Engagement
  counts are hash-derived from the story id (`likes = 12 + |seed*17| % 89` and similar), so they
  are decorative and carry no player participation.
- `FootballNarrativeMemoryService.Refresh` groups by team plus normalized category. The
  `FootballNarrativeKind` enum already declares Player, Coach, and Rivalry, but kind is only
  inferred from the category name, so rivalry memory never comes from actual recorded rivalry
  results.
- Both hosts call the service: `FoundationPage.Navigation.cs:434-435` and
  `FranchisePage.Profiles.cs:183-185`.

Work

1. Separate derived engagement counts from real player participation: persist the player's own
   likes, reposts, and replies as their own state.
2. Replies and follows: authored in-game media replies derived from recorded stories, with
   independent read state, and a follow list for media desks and teams.
3. Broader memory: derive rivalry memory from `RivalryService.Derive` results and player-level
   memory from recorded player story ids, instead of inferring kind from category names.
4. Broader visual acceptance on both hosts for the News and Social surfaces.

Acceptance

- No invented outcomes, quotes, interviews, or statistics; every post traces to a recorded story.
- Player participation persists per save and never mutates derived counts on other posts.
- Read state stays independent between a story and its social post.

## Batch E — Rivalry completion

**Status: delivered on `codex/feature-roadmap-plan`.** Both games persist the rivalry set, the shared reader
labels a matchup and describes a series the same way in both games, and the team, game-center and history
surfaces show the series. No rivalry effect on a play is included.

Current state

- `RivalryService.Derive` returns only pairings with more than one game and is not persisted.
- `TraditionalCollegeRivalries` supplies deterministic College traditional labels.
- Labels appear only in schedule lists: `FoundationPage.LeagueTables.cs:136-137` and
  `FranchisePage.Schedule.cs:38`. Team profiles, game center, and history views have no rivalry
  exposure, and there is no durable rivalry-history acceptance record.

Work

1. Persist rivalry records and rivalry game records in both save models with empty defaults.
2. Expose rivalry labels and series records in team profiles, schedule, game center, and history
   views in both hosts.
3. Define and run durable rivalry-history acceptance: stable rivalry set for the same save
   identity, correct series records, consistent labels across hosts.
4. Keep the first increment simulation-free, as the roadmap requires.

All five batches are delivered on `codex/feature-roadmap-plan`. The near-term checkpoint in `ROADMAP.md` is
complete apart from the items each batch explicitly left open, which are listed per batch above.

## Sequencing

| Batch | Depends on | Why this order |
|---|---|---|
| A | Save-compatibility conventions | Produces the season state that C and D read; chemistry already has a simulation path to extend. |
| B | Recorded event metadata | Uses the existing recorded snap stream; independent of A. |
| C | Stable career and honor records | Needs trustworthy multi-season history, so it follows A. |
| D | Rivalry, leadership, discipline, records | Threads should be grounded in state that A and E create. |
| E | Save-compatibility conventions | Low risk and independent; can run alongside D. |

## Validation policy for every batch

1. Run the narrowest relevant tests first, using exact filters and recording the actual counts.
   - Engine: `dotnet test tests/Football.Engine.Tests/Football.Engine.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~<Suite>`
   - College: `dotnet test tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~<Suite>`
   - Pro: `dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~<Suite>`
   - CI tiers: `./build/test-tier.ps1 -Owner Engine -Tier Fast`, then `-Tier Balance`.
2. Build only the affected app. Compile first, package only when the batch needs an APK:
   `dotnet build src/DynastyManager.App/DynastyManager.App.csproj -c Debug --no-restore -t:Compile`
   and the same for `src/ProFootball.App/ProFootball.App.csproj`.
3. Device acceptance requires the exact APK that was built. Do not reuse earlier APKs as evidence.
4. Known environment issues to expect: offline `NU1900` package-vulnerability warnings, two
   existing Pro `CS8625` nullable warnings, and an unreliable local `Category=Fast` tier run
   (testhost connection timeouts). Segmented filters or CI cover this instead.
5. Update `docs/current-work.md`, `docs/testing-scope.md`, and `docs/ROADMAP.md` with what was
   actually implemented and what actually ran.

## Git and CI rules for each batch

- Work on `codex/feature-roadmap-plan` or a `codex/` branch cut from it. Preserve untracked
  files and worktrees.
- Commit only the intended change with a concise message. Review the diff for generated artifacts
  and large binaries before committing.
- Push the branch when network and credentials allow. Open a pull request only when asked.
- Dispatch the existing workflows rather than adding new ones:
  `.github/workflows/csharp-ci.yml` (College, `management_scope`) and
  `.github/workflows/pro-android.yml` (Pro, `validation_scope`). Device coverage uses only
  `build/android-ui-smoke.py` and `build/pro-android-smoke.py`; retired per-scope and shared-live
  smoke scripts are not part of current validation.
- Wait for any dispatched run to finish and inspect its real status and logs. Distinguish a CI
  artifact from a release.
- No merge to `main`, no release, and no prerelease publication without explicit instruction.

## Not authorized by this plan

Release publication, merge to `main`, repository renaming, branch deletion, and any change to
unrelated workflows or artifacts.

# Current roadmap — November 4, 2026

This replaces the older forward-looking roadmap. Completed release evidence remains in `current-work.md`, `testing-scope.md`, `pro-completion-checklist.md`, and the individual release/audit records. The plan below is the active planning source and is based on a source-code audit of main at `81b7b28`.

## Planning baseline

- Latest release: Pro165 / College454 (`preview-2026-11-04-roster-identity`).
- Shared engine and presentation are the default home for league-neutral behavior.
- College and Pro keep league-specific roster, financial, career, persistence, branding, and rule adapters.
- Existing factual news systems are retained; do not fabricate statistics, outcomes, interviews, or social activity.
- Existing saves must load without destructive migration. New records default safely and remain backward compatible.
- Do not publish a release or merge without explicit review and approval.

## Audit-based feature inventory

| Area | College | Pro | Planning conclusion |
|---|---|---|---|
| Rivalries and rivalry history | Missing | Missing | Start with shared rivalry records/history; no simulation effect in the first batch. |
| Dynamic news and narratives | Partial | Partial | Reuse existing factual story services; add persistent narrative memory, News Hub redesign, and fictional social posts without inventing stats. |
| Expanded playbooks | Strong named-play foundation | Same shared foundation | Add pre-snap motion, shifts, trick metadata, and defensive counters in shared play definitions. |
| Player morale and discipline | Partial | Partial | Keep league-specific morale/discipline services; add shared locker-room and leadership interactions. |
| Transfers and trade requests | Implemented | Implemented | College uses transfer portal market; Pro uses contract-driven trade requests. Preserve separate rules. |
| GameCast / presentation | Strong shared foundation | Same shared foundation | Share all future UI; hosts provide branding/rules/save adapters only. |
| Broadcast replay | Partial | Partial | Current replays are deterministic and shared; add angle metadata and cinematic replay selection. |
| Team cohesion and scheme familiarity | Missing | Missing | Add shared models with league-specific turnover/scheme inputs. |
| Captaincy and leadership | Partial leadership | Partial leadership | Add actual captain records and leadership hierarchy while retaining existing player/coach leadership effects. |
| Hall of Fame | College player HOF implemented | Missing player HOF | Extract a shared career-score core; preserve College rules and add Pro player/coach HOF. |
| Weather / field conditions | Partial | Partial | Weather affects simulation and visual presentation; add broader venue/turf model and Pro indoor awareness. |
| Weekly prep / opponent scouting | Partial | Partial | Game plans and staff attributes affect simulation; opponent scouting is currently informational only. |
| Historical records / memorable games | Partial | Partial | College has weekly highlights; Pro needs replay/highlight exposure and stronger career-record UI. |

## Updated near-term roadmap

1. **Shared rivalries and rivalry history**
   - Add shared rivalry definitions and recorded rivalry results to both state models.
   - Derive rivalry status from conference, division, geography, and history using league adapters.
   - Show rivalry labels in team, schedule, game-center, and historical-record views.
   - No simulation effect in the first increment.
   - Added a shared rivalry model and service plus schedule labels.
   - College gained a deterministic traditional-rivalry catalog and focused tests.
   - College schedule labels now prefer the traditional catalog and distinguish annual rivalries from other historical pairings.

2. **Expanded playbooks and pre-snap football**
   - Started a shared pre-snap model and catalog focused on non-simulation presentation intent.
   - Add legal situational actions and keep College/Pro personnel and rules separate.
   - Preserve deterministic simulation, mobile call-sheet density, and replay compatibility.

3. **Captains, leadership, cohesion, and scheme familiarity**
   - Added a shared team-chemistry record and deterministic captain/id helper.
   - Add shared captain, player-leadership, team-cohesion, and scheme-familiarity records.
   - Feed from tenure, roster turnover, coaching change, plan usage, injuries, and discipline.
   - Apply bounded simulation modifiers with player-facing explanations.

4. **Morale, discipline, locker room, transfers, and trade requests**
   - Keep College discipline/transfer rules and Pro contract/trade rules separate.
   - Add shared locker-room events, motivation state, and narrative hooks.
   - Preserve existing morale decay, coach-talk, transfer-protection, and trade-market safeguards.

5. **News Hub, narratives, and fictional social media**
   - Add persistent narrative memory keyed by team, player, coach, rivalry, and season.
   - Build a shared News Hub with league adapters, filters, saved/social/story views, and read state.
   - Reference only recorded facts and explicitly label fictional social posts as in-game media.
   - Status: College News Hub now supports All/My-team scope filtering and mobile-friendly card layout.

6. **GameCast immersion and broadcast replay**
   - Improve venue styling, visual markers, replay angles, highlight selection, and mobile readability.
   - Replays must consume recorded state only and never reroll simulation.
   - Preserve portrait/landscape and light/dark usability.
   - Status: Shared GameCast now persists and applies a replay angle; Broadcast, FollowBall, and ReverseFollow are wired into the shared live page and engine preset catalog.

7. **Player and coach Hall of Fame**
   - Create a shared scoring and citation core.
   - Preserve College player HOF eligibility and add Pro player HOF.
   - Add coach HOF with College and Pro eligibility separated at league boundaries.

## Updated long-term roadmap

1. **College NIL system**
   - College-only NIL ledger tied to performance, recruiting profile, market size, academics, and discipline.
   - Preserve existing recruiting and scholarship rules until a save-compatible NIL state is accepted.

2. **College media rights, realignment, and finance**
   - Build on existing realignment/FCS-history records.
   - Add league and school finance, media contracts, competitive balance, and UI summaries.

3. **Pro finance expansion**
   - Extend `ProFinance` with stadium and merchandise revenue.
   - Preserve current cap, cash, dead-money, salary-floor, and contract-accounting rules.

4. **Expansion and relocation**
   - Add shared league-evolution primitives.
   - College uses FBS/FCS and conference constraints; Pro uses league/owner/market rules.
   - Existing league history and schedule builders must remain compatible.

5. **Stadium construction and training facilities**
   - Introduce stable facility and construction records.
   - Tie facilities to finance, performance, player development, recruiting, and staff quality.
   - Use concise mobile summaries and phase-gated decisions.

6. **Additional offseason activities**
   - Extend existing College and Pro phase flows rather than creating parallel calendars.
   - Keep activity outcomes persisted and explainable in team/career UI.

## Recommended implementation order and dependencies

| Stage | Dependencies | Rationale |
|---|---|---|
| Rivalries | Save-compatibility conventions | Low-risk shared foundation for future narrative and history UI. |
| Playbooks/pre-snap | Existing play engine and call editor | Unlocks richer simulation and replay before leadership/cohesion effects. |
| Captains/cohesion | Playbook usage and leadership state | Needs meaningful season context before it affects simulation. |
| Morale/discipline/locker room | Leadership and captain context | Extends current systems without duplicating morale rules. |
| News/narratives/social | Rivalry, leadership, discipline, and records | Generates meaningful arcs from recorded state. |
| GameCast/replay | New event/pre-snap metadata | Avoids rebuilding presentation twice. |
| Hall of Fame | Stable career/honor records | Becomes safer after longer-term history is trustworthy. |
| NIL/Media/Pro finance | Stable league rules and records | Requires season and save-state foundations. |
| Expansion/relocation/facilities | Finance and league evolution | Requires durable market/venue state. |
| Additional offseason activities | Existing phase validation | Must not create a second offseason calendar. |

## First implementation batch: shared rivalries

Scope one session to the rivalry foundation only.

- Define `RivalryRecord` and `RivalryGameRecord` as shared engine records.
- Add safe default collections to both DynastyState and ProFranchise.
- Add a deterministic shared derivation/adaptation service.
- Add league-specific UI labels in Team, Schedule, and Game Center.
- No simulation, recruiting, finance, or scoring behavior is changed in this batch.

Acceptance criteria:

- College SQLite saves and Pro JSON saves round-trip the new records.
- Older saves load with empty rivalry fields and no data rewrite.
- The same save identity produces a stable rivalry set.
- College and Pro UI label the same matchup consistently without duplicating rivalry logic.
- Existing engine seeds and presentation outputs remain unchanged.
- Focused College/Pro/Engine tests pass; Android packaging is deferred to a later milestone unless approved.

## Validation policy

- Run the narrowest relevant Engine, College, and Pro tests first.
- Do not claim simulation, save, mobile-ui, or device acceptance without an actual focused check.
- Android packaging and device acceptance require successful focused tests first.
- No release or merge is part of planning.





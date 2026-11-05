# Current roadmap — November 5, 2026

This replaces the older forward-looking roadmap. Completed release evidence remains in `current-work.md`, `testing-scope.md`, `pro-completion-checklist.md`, and the individual release/audit records. The plan below is the active planning source and is based on a source-code audit of main at `81b7b28`.

The execution outline for the remaining near-term work is [roadmap-completion-plan.md](roadmap-completion-plan.md). It sequences five batches (A season-aware chemistry and locker-room events, B replay highlights and pre-snap choreography, C persistent Hall of Fame lifecycle, D news and social interactions, E rivalry completion) with per-batch acceptance criteria and validation commands, and it records two source-audit corrections to the checkpoint table below.

## Planning baseline

- Latest release: Pro165 / College454 (`preview-2026-11-04-roster-identity`).
- Shared engine and presentation are the default home for league-neutral behavior.
- College and Pro keep league-specific roster, financial, career, persistence, branding, and rule adapters.
- Existing factual news systems are retained; do not fabricate statistics, outcomes, interviews, or social activity.
- Existing saves must load without destructive migration. New records default safely and remain backward compatible.
- Do not publish a release or merge without explicit review and approval.

## User review and implementation checkpoint — November 5, 2026

This checkpoint follows the original review at `9a28a7f`. It applies to both Android hosts through shared Presentation/Engine code. The published release baseline above remains separate, and these local debug APKs are not a release.

1. **Live View field defect: fixed in the shared renderer.** The full-width `HomeColor` rectangle that covered the top 12% of the field was removed from `GameFieldDrawable.Draw`. The exact Pro APK was visually checked in portrait and landscape with end-zone branding, yard lines, hashes and team markers intact. College compiles against the same renderer and packaged successfully; the reported Pro defect has direct device evidence.
2. **News density: compact feed restored.** Both hosts removed the standalone Save story/Remove saved story controls and the Saved filter from the main News page. Existing `SavedNews` payloads remain intact for save compatibility but receive no prominent UI. A persistent Social shortcut now sits beside the filter control.
3. **Social experience: first usable pass delivered.** Social is directly discoverable, uses authored in-game media posts derived from recorded stories, and displays reply/repost/like counts. Posts have independent read state. Each save now retains idempotent team/season/category narrative memory and can produce factual multi-story season threads without inventing results, quotes or statistics. Comments, replies, follows and richer user participation remain open.
4. **News chronology: fixed.** Pro stories now carry season phase and sort by year, phase, playoff/award stage and week. Preseason Week 3 therefore sits below regular-season Week 1. Older records without phase use deterministic category/body fallback. This fixes both the full News feed and the Overview cards that take the first sorted stories.

### Near-term checkpoint

| Workstream | Implemented on the feature branch | Still open |
|---|---|---|
| Rivalries | Shared records/service, schedule labels, College traditional rivalry catalog | Complete team/game-center/history exposure and durable rivalry-history acceptance; no rivalry simulation effect in the first increment. |
| Playbooks / pre-snap | Shared catalog; manual formation hold; animated None/Shift/Motion preview; preset motion entries in every offensive formation family; choices recorded in live/replay without changing randomness | Play-outcome adaptation, league-specific legality, broader marker choreography and remaining device acceptance. |
| Captains / chemistry | Deterministic captains, cohesion/familiarity, bounded GameCast effect, both hosts' team UI | Per-season memory, roster turnover, coaching/plan continuity and discipline inputs. |
| Morale / locker room | Existing league-specific morale, discipline, transfer and trade systems retained; factual narratives now have persistent memory | Shared locker-room events, motivation state and explicit locker-room narrative hooks. |
| News / social | Compact both-host feed, visible Social shortcut, authored posts, engagement labels, independent read state, phase-aware order and persistent season threads | Richer interactions such as replies/follows/user participation, broader player/coach/rivalry memory and broader visual acceptance. |
| GameCast / replay | Shared venue styling, indoor turf, replay angle selection, recorded pre-snap choices and repaired field branding | Replay highlights/selection, pre-snap choreography and broader device coverage. |
| Hall of Fame | Existing College player HOF, Pro player HOF derivation, shared coach scoring service with no production caller yet | Render the shared coach scoring in both hosts' Hall of Fame views; persistent coach induction records, retirement eligibility, richer citations/team attribution and lifecycle parity. |

Recommended next implementation order: season-aware chemistry and locker-room events; replay highlights and pre-snap choreography; persistent HOF lifecycle handling. Within Social, add meaningful interaction and richer player/coach/rivalry threads after those state foundations. These map to batches A, B, and C of [roadmap-completion-plan.md](roadmap-completion-plan.md), with batch D (Social) and batch E (rivalries) following. No release or merge is authorized by this checkpoint.

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
| Weather / field conditions | Partial | Better | Weather affects simulation and visual presentation. Pro indoor venues now skip precipitation and wind effects. Broader venue/turf model remains open. |
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
   - Add legal situational actions and keep College/Pro personnel and rules separate.
   - Preserve deterministic simulation, mobile call-sheet density, and replay compatibility.
   - Status: Manual offensive Call play now holds the formation for roughly 900 ms before the snap. Shift/motion choices animate a pre-snap preview, and the recorded snap starts from that adjusted alignment. The catalog exposes a MOTION-labeled preset play for every offensive formation family; motion remains a play modifier rather than a new simulation outcome category. Play-outcome adaptation, league-specific legality, marker choreography and broader device acceptance remain open.

3. **Captains, leadership, cohesion, and scheme familiarity**
   - Added a shared team-chemistry record and deterministic captain/id helper.
   - Add shared captain, player-leadership, team-cohesion, and scheme-familiarity records.
   - Feed from tenure, roster turnover, coaching change, plan usage, injuries, and discipline.
   - Apply bounded simulation modifiers with player-facing explanations.
   - Status: Shared captain selection, cohesion, scheme familiarity, and a bounded GameCast advantage are wired to both profile builders. Team chemistry is visible in College team profiles and Pro team pages.

4. **Morale, discipline, locker room, transfers, and trade requests**
   - Keep College discipline/transfer rules and Pro contract/trade rules separate.
   - Add shared locker-room events, motivation state, and narrative hooks.
   - Preserve existing morale decay, coach-talk, transfer-protection, and trade-market safeguards.

5. **News Hub, narratives, and fictional social media**
   - Add persistent narrative memory keyed by team, player, coach, rivalry, and season.
   - Build a shared News Hub with league adapters, filters, saved/social/story views, and read state.
   - Reference only recorded facts and explicitly label fictional social posts as in-game media.
   - Status: Both games use a compact shared story feed and a visible Social shortcut. Posts are explicitly labeled authored in-game media derived from recorded facts, with independent read state. Saves retain idempotent season memory and expose factual multi-story season threads. The bookmark UI was removed after user review; existing saved snapshots remain backward-compatible. User replies/follows and broader player/coach/rivalry memory remain open.

6. **GameCast immersion and broadcast replay**
   - Improve venue styling, visual markers, replay angles, highlight selection, and mobile readability. Shared venue styling now reaches the live renderer in both hosts, including indoor turf palettes; device visual acceptance remains open.
   - Replays must consume recorded state only and never reroll simulation.
   - Preserve portrait/landscape and light/dark usability.
   - Status: Shared GameCast removes the unintended team-color field band, persists and applies a replay angle, and records None/Shift/Motion choices without changing simulation randomness. The exact Pro APK passed portrait and landscape field checks. Play-outcome simulation, league-specific legality/adaptation, marker choreography, highlights and broader device coverage remain open.

7. **Player and coach Hall of Fame**
   - Create a shared scoring and citation core.
   - Preserve College player HOF eligibility and add Pro player HOF.
   - Add coach HOF with College and Pro eligibility separated at league boundaries.
   - Status: Player HOF remains league-specific College/Pro derivation. A shared coach Hall of Fame service now scores recorded coach-season summaries and is rendered in College center and Pro honors.

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

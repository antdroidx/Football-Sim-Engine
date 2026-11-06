# Current roadmap — November 5, 2026

This is the active planning source. Completed release evidence remains in `current-work.md`, `testing-scope.md`, `pro-completion-checklist.md`, and the individual release and audit records. The near-term execution outline is [roadmap-completion-plan.md](roadmap-completion-plan.md); the long-term agenda is the last section of this file.

## Planning baseline

- Latest published release: Pro165 / College454 (`preview-2026-11-04-roster-identity`). The next prerelease is pending final APK and CI verification.
- Active branch: `codex/feature-roadmap-plan`. The near-term work below is implemented there and pushed, but it is not merged and not published.
- Shared engine and presentation are the default home for league-neutral behavior.
- College and Pro keep league-specific roster, financial, career, persistence, branding, and rule adapters.
- Existing factual news systems are retained; do not fabricate statistics, outcomes, interviews, or social activity.
- Existing saves must load without destructive migration. New records default safely and remain backward compatible. The College save repository rejects a mismatched schema version, so a schema version is only bumped when the format truly changes.
- The current execution has explicit authorization to merge to `main`, publish one prerelease, clean generated files and delete completed feature branches after final verification.

## Near-term status — November 6, 2026

All seven near-term workstreams were carried through five implementation batches. The batches, their commits, their acceptance criteria and their validation commands are recorded in [roadmap-completion-plan.md](roadmap-completion-plan.md).

| Workstream | Delivered | Still open |
|---|---|---|
| Rivalries | Shared records and service; College traditional rivalry catalog; persisted rivalry set in both saves; labels and series in the College team profile, Pro team page, College game center, Pro matchup card and both schedules | A sourced league rule for a rivalry effect on a play. The first increment deliberately excludes one. |
| Playbooks / pre-snap | Shared catalog; manual formation hold; animated None/Shift/Motion preview; a MOTION preset in every offensive formation family; a bounded outcome effect recorded on the snap; league rules own the shift and motion limits; a motion returns to the formation before the snap | Device acceptance of the pre-snap effect, and any sourced league-specific limit. Both leagues keep shift at one yard and motion at three because no sourced rule differs. |
| Captains / chemistry | Deterministic captains; cohesion and scheme familiarity; a bounded GameCast effect; both hosts' team UI; per-season memory fed by turnover, coaching change, plan continuity, captain changes, discipline and morale; every adjustment explained to the player | Deeper continuity effects and the remaining discipline inputs. |
| Morale / locker room | Existing league morale, discipline, transfer and trade systems retained; persistent factual narrative memory; shared locker-room events recorded each season from real state, with College discipline incidents and Pro contract and trade friction kept separate | Motivation state, and further locker-room narrative hooks. |
| News / social | Compact both-host feed; visible Social shortcut; authored posts with engagement labels and independent read state; phase-aware order; persistent player/coach/team season threads; player likes, reposts, followed desks, generated replies and rivalry threads; Pro player story identity | Player-authored reply action and explicit device acceptance. |
| GameCast / replay | Shared venue styling and indoor turf; replay angle selection; recorded pre-snap choices; replay highlight selection from the recorded snaps; repaired field branding | Device acceptance of the highlight picker and the pre-snap choreography. |
| Hall of Fame | College player HOF; Pro player HOF derivation; shared coach scoring in both hosts; persistent coach induction records with per-team citations; CPU club coach season history with stable staff identity | Final broad validation and device acceptance. |

### What is left in the near term

The durable state and identity work is implemented on the active branch: CPU club coach seasons use distinct
staff identities and conference accounting, Pro player-specific stories carry player identity into persistent
person threads, motivation is a persisted season state, and the Pro scouting department repairs itself to the
required three scouts per club. The remaining near-term work is now release-facing:

1. **Explicit action-level device acceptance of the new surfaces.** Assert a player-authored social reply,
   like/repost state, rivalry series panel, chemistry/motivation note and replay highlight picker on fresh
   College and Pro APKs.
2. **Sourced rule boundary.** Encode the sourced NCAA/NFL motion count, direction and set requirements, and
   document that the repository's one-yard shift and three-yard motion values are internal simulation/catalog
   bounds rather than league rules. Keep rivalry records simulation-neutral because neither rulebook supplies a
   rivalry play modifier.
3. **Final broad validation and packaging.** Run the required Engine, College and Pro checks for the completed
   state work, then build and verify both exact Debug APKs before the authorized merge and prerelease.

## Audit-based feature inventory

| Area | College | Pro | Planning conclusion |
|---|---|---|---|
| Rivalries and rivalry history | Shipped | Shipped | Shared records, persisted set, series exposure. A sourced rule for a play-level effect is still open. |
| Dynamic news and narratives | Partial | Partial | Factual story services reused; persistent memory, a compact feed, authored posts, player activity and replies are in. Player and coach level threads are open. |
| Expanded playbooks | Strong named-play foundation | Same shared foundation | Pre-snap motion, shifts and a bounded outcome effect are in. Trick metadata and defensive counters are open. |
| Player morale and discipline | Partial | Partial | League-specific services kept separate; shared locker-room events are in. Motivation state is open. |
| Transfers and trade requests | Implemented | Implemented | College uses the transfer portal market; Pro uses contract-driven trade requests. Keep the rules separate. |
| GameCast / presentation | Strong shared foundation | Same shared foundation | Share all future UI; hosts provide branding, rules and save adapters only. |
| Broadcast replay | Partial | Partial | Deterministic shared replays with angle selection and highlight selection are in. Cinematic replay treatment is open. |
| Team cohesion and scheme familiarity | Shipped | Shipped | Shared records with league-specific turnover and scheme inputs, carried per season. |
| Captaincy and leadership | Partial leadership | Partial leadership | Actual captain records exist; a full leadership hierarchy is open. |
| Hall of Fame | College player HOF implemented | Pro player HOF and coach HOF implemented | Shared scoring and citation core in both hosts. CPU coach lifecycle parity is open. |
| Weather / field conditions | Partial | Better | Weather affects simulation and presentation; Pro indoor venues skip precipitation and wind. A broader venue and turf model is open. |
| Weekly prep / opponent scouting | Partial | Partial | Game plans and staff attributes affect simulation; opponent scouting is informational only. |
| Historical records / memorable games | Partial | Partial | College has weekly highlights; both hosts now expose replay highlights and rivalry series. Stronger career-record UI is open. |

## Long-term roadmap

The near-term work above is feature completion on existing systems. The long-term work is new systems that need durable records before they can change behavior. The conventions hold for all of it: shared engine and presentation by default, league adapters where the rules differ, save-compatible records that default safely, and no invented facts.

### 1. College NIL system

- College-only. A NIL ledger tied to performance, recruiting profile, market size, academics and discipline.
- Existing recruiting and scholarship rules stay untouched until a save-compatible NIL state is accepted.
- First slice: an NIL earnings ledger keyed to season and player, with a factual program summary. No recruiting or compensation effect yet.

### 2. College media rights, realignment and finance

- Build on the existing realignment and FCS-history records.
- Add league and school finance, media contracts, competitive balance and UI summaries.
- First slice: media-rights records per program with a summary in the program center. No effect on revenue distribution until the ledger is accepted.

### 3. Pro finance expansion

- Extend `ProFinance` with stadium and merchandise revenue.
- Current cap, cash, dead-money, salary-floor and contract-accounting rules must not move.
- First slice: stadium and merchandise revenue lines alongside the existing ledger, with the season summary updated. The cap math stays byte-for-byte identical.

### 4. Expansion and relocation

- Shared league-evolution primitives.
- College uses FBS and FCS and conference constraints; Pro uses league, owner and market rules.
- Existing league history and schedule builders must remain compatible.
- First slice: a candidate-team record with the constraints each league applies, and a read-only review screen. No schedule change until a relocation is accepted.

### 5. Stadium construction and training facilities

- Stable facility and construction records.
- Tie facilities to finance, performance, player development, recruiting and staff quality.
- Concise mobile summaries and phase-gated decisions.
- First slice: facility records with a construction decision in the existing phase flow, persisted and explained. Finance and performance links wait on the ledgers above.

### 6. Additional offseason activities

- Extend the existing College and Pro phase flows rather than creating parallel calendars.
- Activity outcomes stay persisted and explainable in the team and career UI.
- First slice: one new activity per league inside the current phase flow, reusing the phase validation that already exists.

### Order and dependencies

| Stage | Depends on | Rationale |
|---|---|---|
| College NIL | Save-compatible ledger | Independent of the other long-term areas; safe to start first. |
| Pro finance expansion | Stable cap ledger | Must not disturb the cap, so it goes on its own. |
| College media rights and realignment | Existing realignment records | Reuses what is already recorded. |
| Stadium and facilities | Finance and league evolution | Facilities need a money story and a venue story before they matter. |
| Expansion and relocation | Finance and league evolution | A new club changes the ledger and the schedule. |
| Additional offseason activities | Existing phase validation | Must not create a second offseason calendar. |

## Validation policy

- Run the narrowest relevant Engine, College and Pro tests first.
- Do not claim simulation, save, mobile-ui or device acceptance without an actual focused check.
- Android packaging and device acceptance require successful focused tests first.
- A device failure is a finding: diagnose it, fix the cause, and rerun the affected check. Never dismiss an app ANR to make a harness pass.
- No release or merge is part of planning.
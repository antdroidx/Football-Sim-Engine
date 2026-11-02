# Current roadmap — November 1, 2026

Canonical status at main99254c7; supersedes outstanding-work wording in historical plans. Latest release: Pro149 / College439, [dual-app prerelease](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-01-staff-gamecast). Full [parity audit and recommendations](audits/pro-college-parity-2026-11-01.md), with a [standalone HTML report](audits/pro-college-parity-2026-11-01.html).

## Completed and released

| Milestone | Delivered | Evidence |
|---|---|---|
| Management/UI foundation | Responsive shells, tables, actionable draft/retention views, Front Office groups, themes, profiles, staff-name links and rating bars | current-work.md; Pro149/College439 |
| Shared live GameCast | Shared field, scorebug, menus, replay/control lifecycle, coordinator authority, compact landscape footer | current-work.md shared live acceptance; Football.Presentation/GameCastLivePage.cs |
| Player ratings and positions | Canonical19 attributes/overall, K/P separation, FB roles, fit/conversion, editors and Pro save migration | shared ratings/position tests; release changelog |
| Franchise depth | Staff/combine scouting, development, camp, ownership/career context and coordinator opposite-unit management | coordinator plan accepted Pro58; current-work.md |
| Standings, analytics, news | NFL tiebreakers, career/draft comparisons, efficiency, factual league reviews and projections | current-work.md accepted user-selected batch |
| College-to-Pro draft exchange | Export/share full classes, translated import, identity preservation, class completion and normal rookie contracts | PlayerExchangeTests; DraftClassExportTests |
| Practical finance and trades | Real-roster defaults, estimates, cap/bonus/waiver/retention/GM/trade safeguards and future picks | pro-practical-finance-policy.md; current-work.md. Source-perfect auditing explicitly closed. |
| Staff and team identity | Shared coaching/medical/scout attributes, lifecycle, strategies, team directories; College academics and Pro HC calibration | shared-staff-attributes.md; pro-coach-rating-verification.md |
| Simulation foundation checks | Real/generated balance samples, personnel/stat accounting, Pro pacing and staff sensitivity | football-balance-audit.md; pro-pacing-audit.md; coach-rating verification |
| Integration/archive/release | Shared work merged to main; Java retained under Archive; repository renamed; dual APK prerelease and full changelog published | releases/preview-2026-11-01.md |

[Active implementation handoff](IMPLEMENTATION-HANDOFF.md) records the approved sequence, checkpoints and model-resume instructions.

## Remaining, prioritized

- **P1 — Device acceptance:** staff links, editors, portraits, palette and safety/kickoff changes are implemented. Complete the final exact-APK role/theme/orientation/extreme-rating matrix and defensive play save/restart/reload/execution. Prior accepted device checks remain scoped to their APK hashes.
- **P1 — A05:** ordinary safeties/free kicks and modern Pro dynamic kickoff behavior are implemented. Timeout/expiry/two-point/onside/penalty boundary tests pass; the768-game real/generated CPU/controlled mode comparison passes. Live-ball penalties, runoff/untimed-down and return/try safety exceptions still need production coverage. See [rules matrix](audits/rules-conformance-matrix.md).
- **A06–A08:** P/K aging policy and multi-year cohorts pass. Custom plays and persisted coach portraits are implemented; remaining device acceptance is above. A09 remains developer-only.
- **Shared work delivered:** A10 scope/competition/deduplication contracts and league adapters; shared phase-validation/task layout; stable-ID staff-name link components; canonical specialist/fullback adapters; A12 attribute bounds with annual staff characterization. Further extraction is optional when it removes matching behavior, not a requirement to merge league policies.
- **A11:** Engine/College/Pro ownership and explicit Fast/Balance/Career runner are established; major historical College career/calibration classes are tagged. Legacy ownership migration is incremental. Device is a separate exact-APK tier.
- **College.Core evaluation:** completed; physical separation deferred to avoid assembly/save migration before dependency ownership is clearer.
- **Integration/release:** branch changes and final build/device evidence must be accepted before integration/release. Preserve the unrelated workflow edit and deferred local cleanup.
- Routine data refreshes, playtesting and performance checks remain ongoing.

## Release/validation status

Latest published APKs pass401 Pro and760 combined Core/College cases. Prior device and long-run evidence is scoped in testing-scope.md; the final presentation APKs do not claim a fresh all-screen walkthrough. Keep intentional league differences; do not reopen perfect-source finance or repeat already completed exchange/coordinator work because an older plan says pending.

## Repository hygiene policy

Retain permanent release assets/checksums, current accepted build runs, unique UI/performance evidence and recent useful caches. Delete superseded build-only APK artifacts and artifact-free failed/cancelled attempts after inventory. Delete remote branches only after ancestry verification; never discard dirty local worktrees. Keep both primary Android workflows. The additional branch-only build workflow is redundant and should be retired only when its pending local edit is resolved, not silently overwritten.

## Original College roadmap and unresolved design backlog

The original [build232-era roadmap](roadmap-history-build232.md) is preserved. Recruiting balance and long-term dynasty health remain ongoing validation rather than permanently completed tasks. Original design backlog remains: NIL resources and coaching/staff budgets, richer spatial/blocking/progression simulation, advanced play-editor authoring, and direct binary NCAA06/PS2 import/export. Old detailed exit criteria need fresh code verification before being called missing or complete. Modern CSV roster/league setup and illustrated profiles are implemented; CSV support does not imply binary database round-trip.

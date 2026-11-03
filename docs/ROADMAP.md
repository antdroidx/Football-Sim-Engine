# Current roadmap — November 2, 2026

Parity plus roster/rules/test-ownership follow-up merged to main at `cb16217`; implementation runtime `f4c3bbf`. Exact builds/checks and integration status are in [the active handoff](ROSTER-RULES-VALIDATION-HANDOFF.md). This supersedes outstanding-work wording in historical plans. Latest release: Pro149 / College439, [dual-app prerelease](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-01-staff-gamecast). Full [parity audit and recommendations](audits/pro-college-parity-2026-11-01.md), with a [standalone HTML report](audits/pro-college-parity-2026-11-01.html).

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

## Approved parity continuation

- **Staff, editors and portraits:** implemented and device-verified; profile identity/permission, Save/restart/reload/Back and offensive/defensive custom calls are covered. Exact-APK evidence remains separately identified.
- **Palette and analytics:** range colors shared across rating bars and quality comparisons, with light/dark variants; signed efficiency metrics honor their quality callback and lower-is-better direction.
- **A05 scenarios:** timeout/expiry, onside recoveries, modern Pro kickoff setup/touchbacks, ordinary safety/free-kick, current two-point policy and dead-ball penalty accounting covered. The768-game real/generated CPU/controlled mode comparison passes. See the [rules matrix](audits/rules-conformance-matrix.md) for explicit unsupported officiating exceptions and calibration limits.
- **A06:** P/K policy and multi-year cohort validation complete.
- **Shared code:** A10 competition/scope/dedup adapters, phase results/task UI, staff-name components, canonical specialist/fullback adapters, and A12 bounded arithmetic with annual characterization delivered.
- **A11:** ownership and Fast/Balance/Career execution established; Device remains an exact-APK tier. Further legacy-fixture migration is incremental maintenance.
- **College.Core:** evaluation complete; physical separation deferred for compatibility/ownership reasons.
- **Repository:** tested changes merged to main at `cb16217`; successful main APKs Pro162/College450 and draft changelog are available. Main checks pass331 Engine,602 College and419 Pro cases; exact artifacts and limits are in the active handoff. A new prerelease is not being published; preserve unrelated work and defer local cleanup.
- The follow-up reproduces and fixes a destructive CPU cap-repair path, adds minimum-roster/rookie reserves and future-contract safeguards. Its full original intermittent career trace remains unavailable; see [the active follow-up](ROSTER-RULES-VALIDATION-HANDOFF.md). Routine data refresh, playtesting and wider career/balance validation remain ongoing.

## Release/validation status

Latest published APKs pass401 Pro and760 combined Core/College cases. Prior device and long-run evidence is scoped in testing-scope.md; the final presentation APKs do not claim a fresh all-screen walkthrough. Keep intentional league differences; do not reopen perfect-source finance or repeat already completed exchange/coordinator work because an older plan says pending.

## Repository hygiene policy

Retain permanent release assets/checksums, current accepted build runs, unique UI/performance evidence and recent useful caches. Delete superseded build-only APK artifacts and artifact-free failed/cancelled attempts after inventory. Delete remote branches only after ancestry verification; never discard dirty local worktrees. Keep both primary Android workflows. The additional branch-only build workflow is redundant and should be retired only when its pending local edit is resolved, not silently overwritten.

## Original College roadmap and unresolved design backlog

The original [build232-era roadmap](roadmap-history-build232.md) is preserved. Recruiting balance and long-term dynasty health remain ongoing validation rather than permanently completed tasks. Original design backlog remains: NIL resources and coaching/staff budgets, richer spatial/blocking/progression simulation, advanced play-editor authoring, and direct binary NCAA06/PS2 import/export. Old detailed exit criteria need fresh code verification before being called missing or complete. Modern CSV roster/league setup and illustrated profiles are implemented; CSV support does not imply binary database round-trip.

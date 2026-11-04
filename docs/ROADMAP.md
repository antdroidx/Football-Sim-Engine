# Current roadmap — November 4, 2026

Latest release: **Pro165 / College454** — [roster identity, CPU plan drift and Flexbone academies](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-04-roster-identity), published from implementation commit `9c9ff9c` with the release record at `8a62a03`. The shared roster identity system, three-season audit, staff-adapted CPU game-plan drift and Flexbone service academies are merged to main and released. The prior Pro162 / College450 [parity prerelease](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-02-parity-rules) remains the prior accepted baseline.

Evidence for the current release: [roster identity plan](roster-identity-plan.md), [long-term audit](roster-identity-long-term-audit.md), [playbook/scheme coverage audit](playbook-scheme-coverage-audit.md), `docs/data/roster-identity-*-2026-11-03/`, [release changelog](../releases/preview-2026-11-04-roster-identity.md) and manifest. Main CI: Pro165 run `41798956069` passes456 Pro cases; College454 run `41798956072` passes319 Engine Fast +12 Engine Balance +607 Core/College cases. Both builds package successfully.

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
| Roster identity and CPU plan drift | Shared playbook-driven targets/priorities, Pro cap-aware acquisition scoring, College roster-shape scoring, staff-adapted CPU game-plan drift, Flexbone service academies and a covered scheme taxonomy audit | roster-identity-plan.md; roster-identity-long-term-audit.md; playbook-scheme-coverage-audit.md; Pro165/College454 |

[Active implementation handoff](IMPLEMENTATION-HANDOFF.md) records the approved sequence, checkpoints and model-resume instructions.

## Historical parity continuation (released baseline)

- **Staff, editors and portraits:** implemented and device-verified; profile identity/permission, Save/restart/reload/Back and offensive/defensive custom calls are covered. Exact-APK evidence remains separately identified.
- **Palette and analytics:** range colors shared across rating bars and quality comparisons, with light/dark variants; signed efficiency metrics honor their quality callback and lower-is-better direction.
- **A05 scenarios:** timeout/expiry, onside recoveries, modern Pro kickoff setup/touchbacks, ordinary safety/free-kick, current two-point policy and dead-ball penalty accounting covered. The768-game real/generated CPU/controlled mode comparison passes. See the [rules matrix](audits/rules-conformance-matrix.md) for explicit unsupported officiating exceptions and calibration limits.
- **A06:** P/K policy and multi-year cohort validation complete.
- **Shared code:** A10 competition/scope/dedup adapters, phase results/task UI, staff-name components, canonical specialist/fullback adapters, and A12 bounded arithmetic with annual characterization delivered.
- **A11:** ownership and Fast/Balance/Career execution established; Device remains an exact-APK tier. Further legacy-fixture migration is incremental maintenance.
- **College.Core:** evaluation complete; physical separation deferred for compatibility/ownership reasons.
- **Repository:** the parity batch is superseded as the latest baseline by the roster-identity release `9c9ff9c` / `8a62a03`. Main and the local repository were synced on November 4; the branch-dispatch and main Android workflows both pass. Pro162/College450 assets remain published as historical release evidence.
- The parity follow-up fixed a destructive CPU cap-repair path and added minimum-roster/rookie reserves and future-contract safeguards. Its original intermittent career trace remains unavailable; see [the roster/rules handoff](ROSTER-RULES-VALIDATION-HANDOFF.md). Routine data refresh, playtesting and wider career/balance validation remain the ongoing parity work.

## Release/validation status

Latest published APKs are Pro165 / College454 from successful main runs `41798956069` and `41798956072`: 456 Pro cases, 319 Engine Fast, 12 Engine Balance and 607 combined Core/College cases, all passing. This release claims no new device-played GameCast flow; earlier device evidence keeps its exact-APK identity and scope. The three-season identity audits and the scheme-coverage audit are committed with the release evidence.

## Repository hygiene policy

Retain permanent release assets/checksums, current accepted build runs, unique UI/performance evidence and recent useful caches. Delete superseded build-only APK artifacts and artifact-free failed/cancelled attempts after inventory. Delete remote branches only after ancestry verification; never discard unrelated local worktrees. Keep both primary Android workflows. The branch-dispatch workflow remains available for feature-branch APK checks; retire it only with explicit approval. Local main was synced to `origin/main` on November 4 and the superseded uncommitted root changes were verified and removed after backup comparison.

## Current focus after the roster identity release

- Native/device acceptance for Pro165 / College454 if the next release requires a new all-screen walkthrough; the current batch makes no device-flow claim.
- Continue parity follow-ups for explicit officiating extensions and the intermittent multi-season CPU cap/roster trace where reproduced.
- Keep legacy HS-era scheme families (Wing-T, double-wing, wildcat) outside the current catalogs; they remain covered by Custom plays.
- Continue routine data refresh, playtesting and wider career/balance validation.

## Original College roadmap and unresolved design backlog

The original [build232-era roadmap](roadmap-history-build232.md) is preserved. Recruiting balance and long-term dynasty health remain ongoing validation rather than permanently completed tasks. Original design backlog remains: NIL resources and coaching/staff budgets, richer spatial/blocking/progression simulation, advanced play-editor authoring, and direct binary NCAA06/PS2 import/export. Old detailed exit criteria need fresh code verification before being called missing or complete. Modern CSV roster/league setup and illustrated profiles are implemented; CSV support does not imply binary database round-trip.

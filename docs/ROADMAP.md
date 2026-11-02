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

## Remaining, prioritized

- **P1:** A01 College staff links; A02 actual scouting-department visibility; A03 opponent medical editor identity/authority; A04 exact-build UI acceptance.
- **P1:** A05 league-rule conformance and live/fast-mode comparison. Validate scenarios before changing chosen policies.
- **P2:** A06 explicit punter aging curve decision; A07 Pro custom plays and A08 portrait editor product decisions; A09 honest calibration provenance.
- **P2:** A10 shared statistical-query contract; A11 audited Engine/College/Pro test ownership and test tiers.
- **P3:** A12 bounded shared staff-lifecycle primitives; gradual College-specific core separation after tests.
- Routine real-world data refreshes, playtesting and performance checks remain ongoing. A test count is not feature completeness.

## Release/validation status

Latest published APKs pass401 Pro and760 combined Core/College cases. Prior device and long-run evidence is scoped in testing-scope.md; the final presentation APKs do not claim a fresh all-screen walkthrough. Keep intentional league differences; do not reopen perfect-source finance or repeat already completed exchange/coordinator work because an older plan says pending.

## Repository hygiene policy

Retain permanent release assets/checksums, current accepted build runs, unique UI/performance evidence and recent useful caches. Delete superseded build-only APK artifacts and artifact-free failed/cancelled attempts after inventory. Delete remote branches only after ancestry verification; never discard dirty local worktrees. Keep both primary Android workflows. The additional branch-only build workflow is redundant and should be retired only when its pending local edit is resolved, not silently overwritten.

## Original College roadmap and unresolved design backlog

The original [build232-era roadmap](roadmap-history-build232.md) is preserved. Recruiting balance and long-term dynasty health remain ongoing validation rather than permanently completed tasks. Original design backlog remains: NIL resources and coaching/staff budgets, richer spatial/blocking/progression simulation, advanced play-editor authoring, and direct binary NCAA06/PS2 import/export. Old detailed exit criteria need fresh code verification before being called missing or complete. Modern CSV roster/league setup and illustrated profiles are implemented; CSV support does not imply binary database round-trip.

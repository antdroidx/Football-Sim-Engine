# Roster identity plus resource scoring

Builds on the existing `DepthChartService` scheme fit and adds:
- identity-derived positional priorities for Pro and College;
- personnel-adapted offense/defense identity without changing the saved team plan;
- identity-aware CPU draft/free agency/waiver/scouting/trade scoring;
- resource-aware pro acquisition scoring with priority plus cap efficiency;
- college roster-shape scoring using identity-driven targets, class-year upside and depth.

The same identity model powers both games. Pro decisions keep existing salary-cap, roster-limit and coordinator-authority safeguards. College decisions keep the current roster/scholarship limit and recruiting points, but now use scheme-derived roster targets and positional priority.

## Current behavior
- Pro positional priorities blend scheme, depth targets and positional staff expertise.
- Pro acquisition score blends roster priority and cap efficiency.
- Pro CPU draft choices use cap-aware acquisition scoring, not raw OVR.
- Pro free agency and waiver claims rank by acquisition score rather than raw OVR.
- Pro trade value blends standard value, cap surplus, positional need and identity fit.
- Pro retention processes higher-priority players first.
- Pro cuts and trade markets rank interested teams by priority and value.
- Recruiting target counts reflect the team's offensive/defensive identity.

## Personnel adaptation
- A heavily pass-shaped roster leans an existing run-first base toward Spread.
- A run/OL-heavy roster leans existing spread/air-raid bases toward Power Run.
- A coverage-heavy roster leans existing front-heavy bases toward Nickel.
- An edge-heavy roster leans existing front/Nickel bases toward Three-Four.

## Staff philosophy
- Coach scheme labels map deterministically to engine playbooks; ambiguous or unknown labels carry no influence.
- A head coach whose personal philosophy differs from the club baseline adapts positional priorities and depth targets by an influence weight that grows with tenure and leadership, capped so saved game plans stay authoritative.
- Season-zero staff and coordinators keep the club baseline, so existing identities are unchanged until a hire differs.
- Newly hired head coaches carry a stable personal philosophy derived from their name, franchise and stronger skills; the club baseline is no longer automatically adopted.
- Identity blending affects CPU draft, free agency, waiver, scouting, retention and trade ordering through the existing `ProRosterPlanService` scores; cap/roster/rule validation is unchanged.

## Depth chart
- `ProDepthChart.Plan` derives the working plan from the current identity model.
- `ProDepthChart.AutoAssign` uses that identity plan to shape positional slots and fit ordering without changing the saved plan.

## Long-term audit
- A three-season audit across real and generated Pro/College modes (8,832 games) validated identity
  adherence and balance; the report and per-mode JSON live in
  [roster-identity-long-term-audit.md](roster-identity-long-term-audit.md) and
  `docs/data/roster-identity-{2026-11-03,drift-2026-11-03}/`.
- Identity roster-target gaps stay stable across seasons, zero personnel violations, and
  acquisition-scoring-only philosophies keep the Pro/College rosters balanced.

## Game-plan drift
- CPU saved plans drift at most one adjacent playbook step per offseason toward an established head
  coach's mapped philosophy; the user plan, custom playbooks, plans already at the target, and
  teams below the shared influence gate never drift.
- Pro derives the philosophy from engine-mappable coach scheme labels plus the catalog baseline;
  College derives the offense target from the head coach's numeric stronger side (lean ≥ 3).
- Hooks live in `ProGamePlanDrift` (Pro retention advance) and `RosterPlanService.ApplyCpuPlanDrift`
  (college annual staff advance), both covered by dedicated drift tests.
- The drift-enabled re-run of the long-term audit shows bounded drift with stable scoring, stable
  roster-target gaps and an unchanged user plan each season.

## Released
- Pro165 / College454 were published as `preview-2026-11-04-roster-identity` from main implementation
  `9c9ff9c` (record `8a62a03`). Main CI passed456 Pro cases and319 Engine Fast +12 Engine Balance +
  607 Core/College cases. Exact APK hashes, changelog and the build manifest are attached to the release.
- Air Force, Army and Navy now run the Flexbone playbook, completing the triple-option catalog mapping.

# Roster identity long-term audit — November 3, 2026

Method: `tools/FootballBalanceAudit` season modes, three simulated seasons per mode, all four
real + generated Pro and College modes. Each mode runs the full season game simulation plus the
normal CPU offseason/draft/recruiting lifecycle. Baseline JSON (identity acquisition scoring
active, game-plan drift hooks disabled) is in `docs/data/roster-identity-2026-11-03/`; the
re-run with the CPU game-plan drift hooks enabled is in
`docs/data/roster-identity-drift-2026-11-03/`. Every report records season scoring, roster
identity adherence, saved plan distributions, head-coach philosophy counts, drift counts from
the season-zero catalog, and the user team's saved plan.

The identity audit adds `ProIdentitySeasons` (Pro) and `CollegeIdentitySeasons` (College) to the
season report: `PositionTargetGapPerTeam` sums |active count − identity target| across positions
with a positive target, `DriftedFromCatalog` counts CPU saved plans that differ from the
season-zero catalog plan, and the distributions report saved offense/defense playbooks per team.

## Baseline findings (drift hooks disabled)

- All 8,832 games (272 × 3 per Pro mode, 828 × 3 per College mode) completed. Zero invalid or
  duplicate personnel snaps, zero returned practice-squad players in every Pro cycle.
- Pro `DriftedFromCatalog` stayed 0 in both modes across all three seasons, as designed before the
  drift feature exists. Coaching philosophies already influence acquisition scoring (2–5 teams).
- College drift stayed 0, and the roster-target gap stayed 29.3–30.7 across seasons with no
  runaway rosters (identity targets keep tracking the roster shape; full 85-man depth keeps extra
  depth beyond targeted units, so the gap does not go to zero).
- User plans never changed (Seattle Pro PlayAction/FourThree, Washington College Balanced/ThreeFour).

## With CPU game-plan drift enabled

Drift gates are the shared `RosterPlanService.MinimumDriftInfluence` (.10) plus, for Pro, an
existing engine-mappable coach label that differs from the club baseline; College derives the
offense target from the head coach's numeric stronger side (lean ≥ 3). Each eligible CPU plan
moves at most one adjacent playbook step per offseason, so plans never jump or oscillate.

| Mode | Drift by season 2 | Drift by season 3 | Points season 1→3 (baseline vs drift) |
|---|---|---|---|
| Pro real | 2 teams | 2 teams | 23.88→24.26 vs 23.84→23.78 |
| Pro generated | 2 | 3 | 22.64→22.86 vs 22.83→23.27 |
| College real | 49 | 49 | 22.37→20.89 vs 22.62→20.98 |
| College generated | 31 | 31 | 23.23→23.34 vs 23.11→23.28 |

- Drift is bounded and stops once a plan reaches its coach target: College reached the eligible
  set within one drift step (66 eligible real / 53 generated; all drifted teams settled by 2027
  and stayed unchanged in 2028).
- Balance is within run-to-run noise of the baseline in every mode and season.
- The user plan never drifts in any mode or season; Pro depth targets, cap safeguards, roster
  limits and College scholarship/roster models are unchanged.

## Scope and limits

This is an engine-level audit: real/generated CPU lifecycles plus full game simulation. It does
not claim device/native evidence, an APK release, or a merge. The Pro drift counts are small in a
three-season window because Pro coach labels mostly match club baselines at world creation;
College's numeric lean applies to roughly half of programs once their influence weight clears the
gate. Long-horizon behavior beyond three seasons (coach turnover cycles, drift after rehires) is
documented but not further audited here.

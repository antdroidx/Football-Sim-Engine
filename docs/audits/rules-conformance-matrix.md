# College / Pro rules conformance matrix

**Date:** November 1, 2026. **Baseline:** branch `codex/parity-roadmap-audit`. **Scope:** shared `GameDayEngine`, its College/Professional rule adapters, and focused scenarios.

## Sources used

- [2026 NFL rulebook](https://operations.nfl.com/rules-officiating/2026-nfl-rulebook)
- [NCAA approved football rules changes](https://www.ncaa.org/news/media-center-changes-to-injury-timeouts-approved-in-football/)

These links record the rules currently in effect for the implementation. Gameplay screens do not display research or calibration explanations.

## Current matrix

| Area | College | Professional | Current evidence | Status |
|---|---|---|---|---|
| Overtime duration | Untimed overtime rounds | 10-minute regular season; 15-minute postseason | `FootballRules`, `FootballRulesConformanceTests` | Separate behavior locked |
| Overtime possession | Each team receives a paired possession; later rounds alternate two-point attempts | Both teams receive an opportunity subject to time and defensive-score rules | Existing `FoundationTests`, new second-possession test | Locked for scoring possession |
| Regular-season tie | Resolved by overtime rounds | Tie possible when overtime expires | Regular overtime expiry test | Locked |
| Postseason tie | Resolved by overtime rounds | Additional overtime periods after expiry | Postseason expiry test | Locked |
| Overtime defensive score | Ends the possession/game without an unnecessary try | Immediate winner during overtime | Existing turnover tests | Locked |
| First-down clock | Stops late in each half / two-minute handling | Does not automatically stop on a first down | Rule flag plus shared clock tests | Separate behavior locked; full clock matrix remains |
| Sack accounting | Included in rushing totals under College adapter | Excluded from rushing; sack yardage retained separately | New Pro sack test | Locked |
| Kickoff touchback spot | 25-yard line | 35-yard line under professional adapter | Existing kickoff tests | Locked to current game policy |
| Kickoff recovery | User can request; CPU onside logic is situational | User can request; CPU logic is situational | Existing kickoff tests | Needs broader recovery/formation coverage |
| Extra point | 20-yard attempt | 33-yard attempt | Existing foundation test | Locked |
| Postseason flag | College postseason services pass their phase | Pro playoff games set `Postseason=true` | Existing foundation plus new overtime test | Covered at engine boundary |
| Ordinary scrimmage safety | Defense gets2; conceding team free-kicks from20 outside overtime/half-end exceptions | Same core scoring/free-kick behavior | `SafetyRulesTests`25 cases; source36fc92c | Implemented; exact-APK banner/kick acceptance pending |

## Production-path checks still required

The production entry points are `GameDayEngine.Start` for live/watched/user games and `GameDayMode.Simulate` for simulated games; all use the same engine and rule adapter. The next comparison must verify:

1. Equivalent seeded personnel and rules produce compatible event categories and accounting across Simulate, Watch, and Play.
2. Personnel eligibility and rotation do not differ merely because the viewing mode differs.
3. Clock and overtime transitions agree for equivalent user and CPU calls.
4. Stat reconciliation holds for completed and live games, including sacks and returns.
5. Intentional differences remain: College untimed overtime, recruiting/eligibility and postseason rules; Pro cap/contract and NFL timing rules.

Statistical outcomes should not be required to be byte-identical across Play and Simulate because interactive calls and random consumption differ. Inputs, legal outcomes and reconciliation should agree.

## Remaining rule risk

- Full timeout and period-expiry combinations.
- Kickoff setup/onside eligibility and dynamic touchback details.
- Complete penalty timing/enforcement.
- Return/foul/try-generated safeties and complete free-kick formation/flight behavior beyond the implemented scrimmage path.
- Two-point decision completeness in all score/time combinations.
- Full live-versus-fast distribution comparison.

These are explicit follow-ups, not assumptions that current behavior is incorrect. Any correction needs a focused failing scenario and a cited policy.

## November 2 continuation

- `GameDayEngineTests.SimWatchAndCpuDelegatedPlayProduceTheSameStats` now covers both rule adapters, six seeds each and all three modes: 12 passing cases / 36 completed games. With every call CPU-delegated, outcomes, clock/period events, offensive/defensive personnel and final team/player statistics match. This is a controlled mode comparison, not a large real/generated statistical calibration or proof of user-selected-call distributions.
- `RulesBoundaryMatrixTests`: 20 passing cases. Covers timeouts immediately before expiry in all four regulation periods and both rule sets; current two-point policy on both possession sides across quarters2/3/4, five clock boundaries and margins-16 through16; requested onside kicks across64 seeds per league including both recovery outcomes, field position and no invented return statistics; late-game false-start/offside enforcement at midfield and near either goal line with no snap/clock loss.
- These are current-policy characterization checks. Full dynamic-kickoff landing/formation/eligibility, safety free kicks, live-ball penalties, late-half runoff/untimed-down rules and actively controlled-call distributions remain open.
- **Confirmed source gap:** `AdvanceState` clamps negative plays at the one-yard line. There is no safety scoring event/free-kick state in `GameDaySession`. The next safety batch needs a failing own-goal-line loss scenario, two-point defensive accounting, free-kick state/spot, halftime/game-end handling and presentation/stat/save coverage. No safety fix or conformance acceptance is claimed here.
- The official2026 NFL rules page was rechecked during this continuation. Rule6 amendments include onside declarations at any game time and kickoff/safety-kick setup changes. Keep declaration eligibility separate from the CPU's late-game strategy; the existing requested-onside test is not full formation compliance.

## Safety implementation — 36fc92c / runtime53e318a

The earlier confirmed scrimmage-safety gap is fixed. Four narrow reproductions first failed with defense0 instead of2. Runs, sacks and intentional kneels can now reach the offense's goal line; losses stop statistically at that line. `IsSafety` preserves the underlying play type, `ScoringTeamName` credits the defense, drives report Safety with no offensive points, and no defensive touchdown is invented. Optional event flags and an appended DriveOutcome value preserve old saves/numeric IDs.

The conceding team supplies a free kick from its own20 (80 in receiving-team field coordinates); normal and onside kicks carry the safety-kick marker and correct origin/recovery spots. Onside events now retain their actual personnel. Ordinary kicks reset that state. Quarter1 keeps the kick in quarter2; halftime replaces it with the normal second-half kickoff. Game-ending or overtime safeties do not queue an extra kick/try; a safety tying regulation at expiry starts the appropriate league overtime. CPU automatic kneeling avoids conceding a safety at its own1.

25 focused cases cover both scoring sides/rule sets, normal/touchback and both onside recoveries, run/sack accounting, scoring-period totals, drive/highlight serialization, regular/postseason overtime and expiry/tied-game boundaries. Affected shared regressions passed138 cases (including22 initial safety cases); the final25-case safety matrix adds three further boundaries. Pro rules/foundation17 also passed. These are focused engine checks; no new safety-specific device acceptance is claimed.

Policy references: [NFL2026 official rulebook](https://operations.nfl.com/rules-officiating/2026-nfl-rulebook), rules6-1 and11-5; [NCAA official rules portal](https://www.ncaa.org/championships/playing-rules/football-playing-rules/) and [NCAA Rule8 text published by SDCFOA](https://www.sdcfoa.org/ncaa/rule-8-scoring), sections8-1 and8-5. The portal's current PDF exceeded the browser fetch limit; the officials' published rule text was inspected for the College safety/free-kick provisions. No general all-rule certification is implied. Dynamic landing-zone touchbacks and complete modern NFL formation restrictions remain open.

## Dynamic kickoff and mode continuation — runtime5239013

Pro kickoffs now distinguish untouched end-zone touchbacks at35, landing-zone bounce/end-zone touchbacks at20 and midfield-origin touchbacks at20. Coverage/setup alignment follows2026 NFL6-1; non-kickers hold until first touch and kicker advance is bounded. College retains its own policy. Kick events preserve replay flags and actual special-teams IDs; huddle previews use the same selector. Normal/safety/onside origins are independent. Eleven Engine policy cases and three64-seed production-origin cases pass; three College adapter/preview cases also pass.

Policy source: [official2026 NFL PDF](https://static.www.nfl.com/image/upload/fl_attachment/league/tqivdkzt9mu6wdgsh1ku.pdf),6-1-3/5/6. This supersedes the table's older blanket35-yard Pro touchback row. It does not implement every illegal-kick/impetus exception.

[768-game production report](mode-conformance-results.md) now covers both leagues, real/generated rosters, all three modes and CPU/explicit user-call policies. Personnel counts/uniqueness, clock bounds and target accounting pass. CPU completed-game records agree across modes. Controlled results retain intended timeout/kneel authority. CPU kneel provenance was corrected. This supersedes the earlier CPU-only distribution gap.

Expanded boundary tests cover exhausted/unknown-team timeouts and expiry in all four periods plus ready-for-play restarts for both sides/leagues. The focused rules/penalty/safety set passes71 cases. Current two-point policy, not an optimal decision model, is exhaustively characterized across the listed score/time grid. Ordinary safeties/free kicks are covered; live-ball fouls/runoff/untimed-down and return/try safety exceptions remain explicit unsupported cases rather than silently passing scenarios.

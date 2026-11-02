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
- Safety scoring and free-kick behavior.
- Two-point decision completeness in all score/time combinations.
- Full live-versus-fast distribution comparison.

These are explicit follow-ups, not assumptions that current behavior is incorrect. Any correction needs a focused failing scenario and a cited policy.

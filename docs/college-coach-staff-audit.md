# College coach staff audit

The College staff baseline was too flat: a 99-prestige program could still land at about 68 overall because the prestige curve capped too low. The current formula now raises the elite range and uses recent wins versus the prestige expectation for season feedback.

## Audit formula

- `CollegeCoachPerformanceService.ExpectedWins(prestige)` = `4 + (prestige - 50) / 7`, bounded to 2..12.
- `CollegeCoachPerformanceService.PerformanceChange(winDelta)` = `2` at four or more wins above expectation, `1` at +2, `-1` at -2, and `-2` at -4 or worse.
- `StaffManagementService.AdvanceSeason` applies a bounded net change to existing coaching attributes. This keeps the save format and simulation path unchanged.
- New College staff use `58 + (prestige - 45) * .38` and a bounded ±7 attribute roll, rather than the old capped 68-style base.
- The College team profile now exposes a coach performance audit table with role, expected wins, actual record, change and summary.

## What still needs review

This first batch improves the baseline and season feedback. It does not yet:
- connect the audit to real coach career history or the user coach career timeline,
- separately tune Pro staff or Hall of Fame ratings,
- add a dedicated device/mobile view for staff audit filters.

Those can be added in smaller increments after the formula survives full College tests and a runtime sim audit.

## Current validation

- `CollegeCoachPerformanceServiceTests`: 2 passed.
- `StaffAttributeTests`: 7 passed.
- `PrestigeStaffHistoryTests` / `StaffMarketTests` / `CoachSuccessionAndLineRolesTests` / `UserCoachCareerTests`: 40 passed, zero failed.
- `Coach|Staff|Prestige` focused run: 98 passed, zero failed.
- Pro staff/scouting/coaching-impact focused run: 28 passed, zero failed.
- Pro Debug Android build succeeded with the existing two nullable warnings.


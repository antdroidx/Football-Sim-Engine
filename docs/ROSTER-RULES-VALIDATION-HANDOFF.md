# Roster safeguards, additional rules and validation ownership

User-approved follow-up on November 2, 2026, on `codex/parity-roadmap-audit`. Preserve unrelated workflow/root work and deferred local cleanup. No main merge or release is part of this branch batch.

## CPU roster and finance

- Reproduced the destructive repair path: a45-player roster with10m room, no replacement market and an unfunded50m repair request was cut to15 by the old code. The focused test failed before the fix.
- `ProCpuRosterSafety.ReleaseForBudget` preserves the45-player floor and positional depth through an atomic affordable replacement when a required player must go. An unfulfillable repair returns unchanged rather than manufacturing cap room or deleting booked debt.
- Existing four-year planning remains in `ProCpuBudget`. Optional two-year offers fall back to one year if they worsen an unfunded future deficit. Future carryover remains a planning estimate, not earned cap credit.
- Current minimum recruitment precedes discretionary offers, uses normal player acceptance/legal salaries, and preserves funds for unsigned draft picks. An initial career retry exposed rookie money being spent by later recruitment; both paths now reserve it. Failed replacement choices no longer prevent considering another safe cap-saving move.
- Cheapest-fill health diagnostics distinguish roster shortage from affordability/market exhaustion. The recorded2028 room25,997,183 is below29 rookie minimum salaries28,275,000 by2,277,817, even before position/market constraints.
- Four focused safeguards plus the original three-season financial/draft/roster invariant pass (39seconds). The earlier standalone restructure-first test also passes. No invariant weakened or historical charge forgiven.

## Remaining work in this follow-up

1. Move shared simulation/rules validation out of the College project; extract a common fixture and preserve single test ownership, Fast/Balance/Career separation and CI discovery.
2. Implement and verify live-ball enforcement, late-half runoff/untimed downs, return/foul/try safety boundaries in the production engine using current NFL/NCAA policies.
3. Run focused related checks, compile both affected apps, document evidence/limits and push the branch. Any new APK must have its own identity; Pro160/College448 remain earlier accepted builds.

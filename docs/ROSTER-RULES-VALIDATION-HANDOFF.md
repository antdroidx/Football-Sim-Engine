# Roster safeguards, additional rules and validation ownership

Updated November 2, 2026. Active branch: `codex/parity-roadmap-audit`; checkout: `.codex-coach-fix`. The user authorized finishing this work and merging to main after validation. Preserve the unrelated `pro-android-branch.yml` edit and dirty root checkout. Local cleanup remains deferred. No new prerelease is being published in this follow-up.

## Delivered CPU roster and finance safeguards

- Reproduced the destructive repair path: an unfunded repair request cut a 45-player roster to 15. Budget releases now preserve the 45-player floor and positional depth, or commit an affordable same-position replacement atomically. An impossible repair preserves the original state.
- Current minimum recruitment uses legal, accepted one-year contracts before discretionary spending. Both normal recruitment and emergency recruitment reserve unsigned rookie cap charges. Legal restructures can fund a shortage without cuts; their future charges remain booked.
- Existing four-year cap forecasts guide optional contract length. Optional two-year offers become one-year offers when they would worsen an unfunded future deficit. Unearned rollover is never spendable current cap credit.
- Health diagnostics report cheapest-fill affordability and required positions. The logged 2028 room of $25,997,183 cannot buy 29 rookie minimum salaries totaling $28,275,000: at least $2,277,817 short before positional/market constraints. No invented players, forgiven dead cap or automatic cap increase.
- The original intermittent career failure's complete causal trace remains unavailable; the reproduced destructive repair path is fixed. Successful repeated career checks do not prove every possible future career remains solvent.

## Delivered additional rules

- Shared scrimmage-commit boundary supports offensive holding, defensive pass interference, grounding and illegal forward passes, accepted/declined outcomes, supported offsetting fouls and nullified-play accounting. Production generation uses this boundary.
- League-specific DPI distance/goal-line limits, grounding/loss-of-down/statistical treatment, untimed period extensions, ten-second runoff and timeout avoidance.
- Return momentum versus voluntary retreat/impetus; return and foul safeties; correct scoring side/free kick, half-expiry safety-kick options and College versus Pro overtime opportunities.
- Failed tries support defensive two-point returns and one-point safeties. Commentary, highlights, animation ownership, drive/period totals and optional saved-event metadata follow the credited team. Punt landings inside the five no longer become premature touchbacks.
- Decisions use automatic coaching policy with explicit engine overrides; no new interactive penalty-decision screen was added. This is the supported common-foul subset, not a complete officiating catalog. Post-possession-change foul combinations, all personal/illegal-kick fouls and full referee/replay procedures are outside this implementation.

## Validation ownership

Fourteen shared simulation/rules files moved from College to Engine across the two commits/batches. `tests/Shared/GameDayFixture.cs` is source-linked into both test projects. No duplicate assertion owner or production dependency on test code. Engine has 319 Fast and 12 Balance cases. The existing three-season financial invariant is explicitly Career/Pro. The tier runner uses a fresh TRX and fails empty explicit selections; College CI calls that runner for Engine Fast/Balance. Device remains exact-APK acceptance, not a synthetic test category count.

## Evidence and delivery

- Final shared Engine run: 331 passed, including 12 Balance cases.
- Final College scope used on main: 602 passed; the earlier related 99-case run is overlapping evidence.
- Final Pro scoped run: 419 passed, including the original three-season financial invariant. The final focused six safeguard cases plus that career invariant also pass (7 total).
- Fresh production comparison: 768 completed games, 32 per cell across both leagues, real/generated rosters, CPU/controlled calls and Simulate/Watch/Play. CPU completed records agree across modes; personnel, clock bounds and target/coverage accounting pass. Controlled Play distributions can differ because of timeout/kneel authority. Raw results: `audits/roster-rules-mode-conformance.json`.
- No fresh all-screen device walkthrough or twenty-year calibration was run for this follow-up. Earlier Pro160/College448 evidence applies only to their recorded APK hashes.

## Delivery checkpoint

Implementation `f4c3bbf` is pushed. College449 run41574279380 passes Engine Fast319/Balance12 and College parity27, packaging with zero warnings/errors. Pro161 run41574281996 passes33 Fast cases and packages with the two existing nullable warnings, zero errors. Both APKs were downloaded, SHA-256 hashed and package/version identities checked with aapt. Exact artifact links/hashes: [roster-rules-acceptance.json](audits/roster-rules-acceptance.json). Workflow actionlint and git diff whitespace checks pass.

Remaining delivery action: integrate into latest main (ancestry verified before builds), inspect resulting main workflows and record integration. No source change is pending; preserve unrelated workflow/root changes. This checkpoint does not claim a new device walkthrough or prerelease.

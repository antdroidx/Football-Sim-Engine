# Implementation handoff — resume here

Last updated: November 1, 2026 (America/Los_Angeles). Latest implementation commit: `71bcd3a`. This document is the model-independent continuation point; read it before making changes.

## User-approved scope and preferences

Proceed in this order:

1. Staff/profile correctness (A01-A03).
2. Exact-build device verification (A04).
3. Rules and simulation checks (A05).
4. Editor parity and specialist aging (A06-A08).
5. Shared-code consolidation and test ownership (A10-A12), following the dependencies below.

This is the full continuation scope, not just the first four implementation batches. Shared-code work remains planned; do not omit it when handing off to another model.

**Latest direction:** This is a game, not a research report. Do not add player-facing explanations that ages/ratings are estimates, club proxies or assumptions. That direction supersedes audit A09's original recommendation to expose calibration caveats. Developer documentation/data provenance can remain. Preserve useful gameplay information and scouting mechanics.

**Usage constraint:** Keep checks small and specific to changed locations. Do not launch broad suites, long simulations, APK builds or device runs for display-text-only edits. Broader checks remain appropriate when the actual approved rules/lifecycle changes require them.

**Local cleanup is deferred.** Preserve files until the user authorizes resuming it. No new main merge or release is authorized for this implementation batch.

## Working checkout

- Branch: `codex/parity-roadmap-audit`.
- Checkout: `D:/AppProjects/Codex/Football Simulation Engine/.codex-coach-fix`.
- Read `AGENTS.md`, [ROADMAP.md](ROADMAP.md) and [the audit](audits/pro-college-parity-2026-11-01.md).
- The root checkout has unrelated uncommitted changes and a stale local main. Do not reset, clean, overwrite or switch it as a shortcut.
- An unrelated `.github/workflows/pro-android-branch.yml` edit remains in this checkout. Keep it out of implementation commits.
- Inspect current git status and remote state; do not assume the historical commit IDs below are still the branch head.

## Current state

| Area | Status | Next step |
|---|---|---|
| A01 College staff links | Implemented; one coaching profile and Back checked on device | Complete own/opponent medical/chief-scout role matrix |
| A02 actual College scouts and assignments | Implemented; scout identity, attributes and assignment checked on device | Verify remaining scouts and reload |
| A03 Pro opponent medical editing | Implemented; correct opponent identity and no edit action checked on device | Verify user medical editing still works and other roles remain consistent |
| A04 full device matrix | Partially complete on an earlier implementation APK | Test latest packaged source when this stage resumes; remaining themes, orientations, reload and rating-bar endpoints |
| A05 rules/simulation conformance | Preparation and baseline tests only | Build source-backed scenario matrix; no simulation rules changed yet |
| A06 punter aging | Not implemented | Decide explicit Pro P curve; current P falls through default28, K explicitly33 |
| A07 Pro custom-play editor | Not implemented | Trace existing engine support, add/reuse UI if absent; require save/reload and actual execution |
| A08 portrait/editor parity | Audit corrected; existing player appearance editor found | Verify existing player editing; inspect staff appearance customization separately |
| Shared staff-directory/rating components | Planned; not extracted | Stabilize profile identity/permission behavior, then reuse components in both apps |
| A10 statistical-query contracts | Planned; not implemented | Characterize both league queries and extract common scope/aggregation contracts |
| A11 test ownership/categories | Planned; not implemented | Separate shared-engine, College and Pro ownership incrementally |
| A12 staff-lifecycle primitives | Planned; not extracted | Characterize seeded behavior before extracting helpers |
| Phase validation/task components | Planned; not extracted | Share result contracts and task UI, not the leagues' phase graphs |
| Rating/physical/position adapters | Existing canonical calculations shared; consolidation pending | Audit remaining adapters and specialist/import cases |
| College.Core separation | Later architectural option; not started | Evaluate after test ownership is clear; preserve assembly/save compatibility |
| UI modeling caveats | Removed in71bcd3a; focused checks and compilation passed | Do not restore them; no APK/device rerun needed solely for this wording batch |

## What changed and where

### 76dcbe1 — first staff/profile fixes

- `src/DynastyManager.App/FoundationPage.TeamProfile.cs`: staff names use existing `CoachLink` / `ShowCoachProfileAsync`; user scouts are listed separately with assignments and readonly profiles. Opposing departments are not fabricated.
- `src/ProFootball.App/FranchisePage.MedicalStaff.cs`: opponent Edit attributes action hidden and its callback also checks team ownership. The existing editor lists user-team staff, so opponent navigation must not route into it.
- Both Android Compile targets passed. Existing focused staff/scouting regression cases:20 College and34 Pro passed. These are model/regression checks, not assertions of UI navigation.

### 83e2470 — device checkpoint and missing scout ages

- Initial device navigation checks recorded below.
- Old scouts without age metadata no longer display Age0; table uses a dash. An explanatory missing-history label was initially added, then removed by the user's later direction in71bcd3a.
- Missing-age follow-up compiled but was not included in the College APK used for the initial device checks.

### 71bcd3a — remove player-facing modeling caveats

- Removed College approximate-age markers, estimated recruiting-star labels/markers and coach-rating accuracy explanation.
- Removed Pro coach calibration/history panel and team-profile provenance text.
- Simplified the missing-age display, academic/scouting copy, field-goal probability label, contract cap preview and unavailable-statistics wording.
- No rating calculations, stored data, scouting mechanics, rules or aging changed.
- Focused absence checks for the removed text passed. Compile-only checks: College0 warnings/errors; Pro0 errors and2 existing CS8625 warnings.
- No broad tests, APK packaging or device checks were run for this wording-only commit.

## Important audit correction: existing editors

Both games already have basic player attribute editors. **Do not build a duplicate Pro player editor.**

Pro also already has `Player profile -> Edit appearance`, implemented by `EditPlayerAppearance` in `src/ProFootball.App/FranchisePage.Profiles.cs`. It supports skin tone, hair and facial hair and persists player appearance. This was confirmed in source, not by a new device roundtrip. Staff portrait customization remains a separate question. College has its own portrait editor and existing shared customization support.

## Device evidence — limited, exact-build scope

See [staff-device-checkpoint.json](audits/staff-device-checkpoint.json). These APKs correspond to the first staff implementation, not the later missing-age/text changes:

- College APK SHA-256: `818536e36e65ef2a319275ca96ffb61236a8968f86068d79beeb99957bec2390`.
- Pro APK SHA-256: `d730877d1285b85199f1846baf468df369e1ff4c8ae4d6f94537e0f641dc438e`.
- Both full local Debug packages succeeded: College0 warnings/errors; Pro2 existing nullable warnings/0 errors.
- College: loaded a Washington career in a readonly emulator clone; Roster -> Team profile & staff -> Jimmie Dougherty opened the correct profile; Back returned to team profile. Colin Rhodes opened his own scouting attributes and Region: West assignment. Numeric3-4 label and rating values appeared on team profile.
- Pro: loaded Seattle career in the clone; opposing Los Angeles Chargers -> Reese Brooks opened the correct medical profile without Edit attributes.
- Local screenshot/XML evidence: `.artifacts/staff-device`. Coach screenshot visually inspected. No save was deleted or reset.
- The test emulator was shut down. There is **no ongoing packaging job or active test session to resume**. Earlier notes about session32386 being active are obsolete.

When device work resumes, use the Android emulator QA skill and query actual devices first. Installed AVD at the last check: `pixel_9_-_api_36`. It was launched read-only/no-snapshot-save/no-window/no-audio on port5580. Preserve original AVD saves. Build the correct current APK, check package/version/hash, and derive taps from UI-tree bounds.

## Rules preparation — not conformance acceptance

Existing Release binaries passed40 shared `GameDayClockAndYacTests` / `CoinTossAndTurnoverTests` / `DeepCoverageAndKickoffTests` and13 Pro `FoundationTests`. These are baseline checks, not proof of every current league rule.

Sources located for the next stage:

- [Official2026 NFL rulebook](https://operations.nfl.com/rules-officiating/2026-nfl-rulebook). Do not substitute stale2024 overtime summaries.
- [NCAA approved timeout changes](https://www.ncaa.org/news/media-center-changes-to-injury-timeouts-approved-in-football/). Retrieve current full rules as needed.

Next scenarios: OT possession/period expiry and defensive scores, regular/playoff flags, kickoff recovery/touchback spots, first-down clock boundaries, sack accounting and live/fast-mode accounting. Preserve intentional league differences. Write a narrow failing scenario before changing behavior; do not make speculative rules fixes.

## Full planned work and acceptance criteria

### Simulation, rules and realism

- **A05 — College/NFL rules matrix:** cover overtime possession opportunities, period expiry, defensive scores, kickoff recovery/touchbacks, first-down clock boundaries, sack accounting and regular/postseason flags. Link each rule-sensitive change to the intended league policy and a focused scenario test. Keep source discussion in developer documents, not game screens.
- **Production-path comparison:** trace simulated, watched and user-controlled games to their actual production paths. Compare personnel eligibility/rotation, rating inputs, clock/rules and stat accounting for equivalent situations. Use controlled seeds where applicable; compare statistical distributions rather than requiring identical outcomes from paths with different random consumption. Do not compare an unused legacy simulator and call that production parity.
- **A06 — Pro punter aging:** make an explicit design decision for P rather than silently inheriting the default curve. Check K and P independently, ceiling preservation and growth/decline around the chosen peak. No new player-facing realism disclaimers.
- **Lifecycle validation after relevant changes:** use real/generated careers and check development, actual player participation, roster/cap/eligibility constraints, staff replacement, phase advancement and save/reload stability. Run targeted multi-year checks when lifecycle behavior changes; do not rerun broad careers for wording or handoff edits.

### Presentation and usability

- **A04 — Exact-APK acceptance:** check both apps in portrait/landscape and light/dark, rating-bar extremes, numeric formation labels, every relevant staff destination, Back navigation and save reload. Record exact package/hash and distinguish already verified cases from outstanding ones.
- **Shared staff-directory and rating components:** reuse row/profile identity contracts, rating bars and permission/action presentation in `Football.Presentation`. Pass stable IDs/team identifiers through callbacks. Keep app-state mutations in league adapters. Verify both hosts so a fix cannot land in only one game.

### Shared-engine and architecture

- **A10 — Statistical-query contracts:** standardize season/career scope, phase selection, duplicate-game handling, totals versus maxima and missing historical fields. Reuse existing `PlayerStatTotals` rather than adding a second aggregator. Keep College postseason and Pro regular/playoff inclusion policies in adapters. Characterize existing results before extraction and verify no historical totals change unintentionally.
- **A12 — Staff-lifecycle primitives:** extract deterministic seeded rolls, age/growth probability calculations and attribute bounds into `Football.Engine` where genuinely identical. Keep hiring, retirement, succession and department structure league-specific. Characterization tests must preserve seed inputs, random ordering, annual idempotence and reload behavior; do not change simulation balance as an incidental refactor.
- **Phase-validation results and task components:** share blocker/validation result contracts and reusable next-action UI. Keep College portal/recruiting/conference stages separate from Pro retention/FA/draft/camp stages. Verify legal transitions, blocked transitions, saved phase compatibility and coordinator authority.
- **Rating/physical/position adapters:** locate remaining duplicated lookups and delegate to existing canonical services where semantics match. Explicitly cover punters, kickers, fullbacks, conversions, generated players and imported identities. Preserve numeric position IDs, potential semantics, contracts and migration idempotence. Different aging policies are intentional, not duplicate overall formulas.
- **Later College.Core separation:** evaluate moving College-only recruiting/season services out of the nominally shared engine after A11 makes ownership clear. Inventory dependencies first. Preserve the existing `DynastyManager.Core` assembly identity and save/type compatibility unless a separately tested migration is necessary. This is an architectural option, not authorization for a disruptive directory/assembly rename now.

### Testing and maintenance

- **A11 — Test ownership:** introduce clear shared-engine, College and Pro ownership incrementally. Use categories or projects as appropriate; avoid duplicated test execution and avoid moving tests merely for cosmetic naming. Establish fast, balance, career and device tiers so workflows run checks appropriate to changed inputs. Verify discovery/filter behavior and retained coverage after each move.
- **Roadmap upkeep:** update `ROADMAP.md`, this handoff and audit status after each accepted batch. Mark historical plans superseded; distinguish implemented, device-verified, ongoing validation and optional work. Never promote candidate findings or old test totals to fresh acceptance.

### Refactoring order and boundaries

1. Stabilize staff/profile correctness and its device checks; then extract shared directory/rating components while those scenarios are fresh.
2. Establish A05 production behavior and A11 test ownership before broad engine restructuring.
3. Extract A10 stat-query contracts, phase-validation/task contracts and remaining rating adapters in small independent changes.
4. Extract A12 lifecycle primitives only after characterization coverage; run focused lifecycle checks if behavior changes.
5. Reassess optional College.Core separation last.

Do not merge the two save formats, application state/page classes, financial/recruiting economies, playoff systems or aging policies. Pro cap/contracts and College academics/recruiting stay league-specific. This work should reduce duplication without forcing identical gameplay.

## Next actions for the incoming model

1. Read current status/diff and this handoff. Keep the latest caveat-removal preference and usage constraint.
2. Finish the remaining A04 scenarios on the latest changed APKs as a focused device batch; do not relabel earlier APK checks as validation of newer source.
3. Proceed to A05's explicit rules matrix and only demonstrated fixes.
4. Then implement the agreed custom-play/staff-portrait parity and specialist-aging decisions; reuse existing services and preserve IDs/saves.
5. Continue the shared-component, A10/A11/A12 and adapter work above; use the dependency order rather than treating it as omitted or completed. Evaluate College.Core separation only afterward.
6. Update this page after each reviewable batch with commit, exact checks, failures/limitations and the next action. Commit/push to the feature branch; do not merge or publish automatically.

The released401 Pro/760 Core-College test totals are prior evidence and were not rerun for this handoff or the latest wording changes. No production changes are part of this handoff-only update.

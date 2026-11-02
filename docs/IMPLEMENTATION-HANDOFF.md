# Implementation handoff — resume here

Last updated: November 1, 2026 (America/Los_Angeles). Latest implementation commit: `71bcd3a`. This document is the model-independent continuation point; read it before making changes.

## User-approved scope and preferences

Proceed in this order:

1. Staff/profile correctness (A01-A03).
2. Exact-build device verification (A04).
3. Rules and simulation checks (A05).
4. Editor parity and specialist aging (A06-A08).

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

## Next actions for the incoming model

1. Read current status/diff and this handoff. Keep the latest caveat-removal preference and usage constraint.
2. Finish the remaining A04 scenarios on the latest changed APKs as a focused device batch; do not relabel earlier APK checks as validation of newer source.
3. Proceed to A05's explicit rules matrix and only demonstrated fixes.
4. Then implement the agreed custom-play/staff-portrait parity and specialist-aging decisions; reuse existing services and preserve IDs/saves.
5. Update this page after each reviewable batch with commit, exact checks, failures/limitations and the next action. Commit/push to the feature branch; do not merge or publish automatically.

The released401 Pro/760 Core-College test totals are prior evidence and were not rerun for this handoff or the latest wording changes. No production changes are part of this handoff-only update.

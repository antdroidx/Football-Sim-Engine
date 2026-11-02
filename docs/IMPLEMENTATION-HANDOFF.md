# Active implementation handoff

User-approved sequence: staff/profile correctness -> exact-build device verification -> rules and simulation checks -> editor parity and specialist aging. This file is the resume point when changing LLMs. Do not infer completion from the audit or an APK compiling.

## Checkout and safety

Work in the released checkout `.codex-coach-fix`, branch `codex/parity-roadmap-audit`; audit baseline d227250. Read AGENTS.md, git status, docs/ROADMAP.md and docs/audits/pro-college-parity-2026-11-01.md. Root main is stale and dirty; preserve it. Pending `.github/workflows/pro-android-branch.yml` edit is unrelated and must stay out of commits. Local cleanup is deferred by user. Do not merge/release without new authorization.

## Execution checklist

1. IN PROGRESS A01-A03: College staff profile names; actual user scouts and assignments; opposing program data stays limited to stored staff; Pro opponent medical editing must not select another club's staff.
2. PENDING A04: Build exact changed APKs, verify package/hash, use disposable/read-only emulator state. Verify own/opponent coaching/medical/scouting links, Back, read-only permissions, save reload, bar endpoints, portrait/landscape and light/dark. Skill: android-emulator-qa. adb currently reports no attached devices; installed AVD pixel_9_-_api_36 exists. Preserve its saves; use a temporary writable clone or read-only launch.
3. PENDING A05: Obtain current official College/NFL sources, scenario tests for OT/clock/kickoffs/sacks/postseason flags and live/fast accounting. Separate intentional rules from bugs. No speculative rule changes. Record exact scenarios and findings.
4. PENDING A06-A08: Explicit Pro punter aging policy; trace Pro custom play and portrait entry points. Both basic player attribute editors already exist. Reuse engine services, preserve IDs and save compatibility. Custom plays must persist and execute; portrait edits must persist without changing identity.

## Acceptance and records

For each batch record changed files, git commit, commands, pass/fail counts, APK/run IDs and hashes, device scenario evidence, limitations and next command. Commit/push reviewable increments. Mark blocked checks precisely; do not claim a disabled or absent device check passed. Existing401 Pro/760 Core-College release results are prior evidence, not new runs.

## Progress log

- Handoff created before implementation. Source confirmed existing College CoachLink/ShowCoachProfileAsync handles coach, medical and chief-scout attributes. User scouts are separate ScoutStaff objects with ScoutAssignments; they need their own readonly profile.
- Pro MedicalProfile exposes editor for opponent staff while Editor only lists user-team staff: choose readonly opposing medical profiles.

- First staff implementation: College team names use existing CoachLink; user scouts/assignments have a separate readonly profile; opponent scout departments are not invented. Pro opponent medical Edit attributes hidden and callback also guards ownership. Both Android Compile targets pass (College0 warnings/errors, Pro2 pre-existing warnings/0 errors). Focused existing regressions:20 College and34 Pro passed; these are model/regression checks, not UI assertions.
- Device: launched pixel_9_-_api_36 on port5580 in read-only/no-snapshot-save/no-window mode; adb emulator-5580 is online. New APK packaging is running in session32386; logs .artifacts/staff-college-package.log and staff-pro-package.log. Do not install the old output until successful current packaging and hash/timestamps are verified.
- Audit correction: Pro PLAYER portrait editing already exists in FranchisePage.Profiles.cs (Player actions -> Edit appearance -> EditPlayerAppearance, lines50/102). It persists skin/hair/beard and preserves player ID. A08 now means verify existing player behavior and separately inspect STAFF portrait parity; do not add another player editor. Pro basic attribute editor also already exists.

- Rules preparation only:40 shared GameDayClockAndYacTests/CoinTossAndTurnoverTests/DeepCoverageAndKickoffTests and13 Pro FoundationTests passed using existing Release binaries. This does NOT complete A05. Current official NFL source redirects to https://operations.nfl.com/rules-officiating/2026-nfl-rulebook ; use2026 text, not stale2024 summaries. NCAA timeout update source: https://www.ncaa.org/news/media-center-changes-to-injury-timeouts-approved-in-football/ . No simulation rules changed.

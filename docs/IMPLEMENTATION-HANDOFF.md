# Implementation handoff â€” resume here

Last updated: November 2, 2026 (America/Los_Angeles). Latest packaged candidates: Pro156 (portrait Back fix `8a4feb2`) / College444 (shared runtime `730d751`; workflow fix `470caf5`); exact-device acceptance is in progress. See the latest continuation below and `audits/parity-acceptance-current.json`. Palette baseline: `f9597a5`. This document is the model-independent continuation point; read it before making changes.

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
| A01 College staff links | Implemented; own coaching/medical/strength/chief-scout profiles and Back checked, including light theme | Complete opponent/remaining role matrix |
| A02 actual College scouts and assignments | Implemented; scout identity, attributes and assignment checked on device | Verify remaining scouts and reload |
| A03 Pro opponent medical editing | Implemented; correct opponent identity and no edit action checked on device | Verify user medical editing still works and other roles remain consistent |
| A04 full device matrix | New palette APKs packaged; College light portrait/landscape subset checked; Pro editor lock found and fixed on device | Remaining Pro themes, opponent roles and rating extremes |
| A05 rules/simulation conformance | Ordinary scrimmage safeties/free kicks implemented;25 safety scenarios pass alongside focused clock/kick/stat checks | Full dynamic kickoff, return/foul/try safety cases, penalty timing and actively controlled mode distributions |
| A06 punter aging | Implemented: Pro P peak32; K peak33; 64 cohorts per position pass twelve annual steps with ceiling/decline checks | Broader real/generated career integration remains separate from this bounded development check |
| A07 Pro custom-play editor | Modal busy-lock fixed; offensive UI create/save/select/restart/reload/live execution passes on exact APK | Defensive copy-editor device roundtrip remains additional coverage |
| A08 portrait/editor parity | User-team coach portrait save/restart/reload checked; opponent Jim Harbaugh profile has no portrait-edit action on exact APK | Additional role combinations remain in A04's broader matrix |
| Shared staff-directory/rating components | Shared `StaffProfileComponents.RatingBar` extracted and used by College | Migrate remaining Pro rating visuals where semantics match |
| A10 statistical-query contracts | Shared `PlayerStatQuery` owns season/team/player filters and played-game deduplication; Pro and College adapters use it; shared totals retain maxima/counters | Broader competition-phase/missing-history adapters remain league-specific work |
| A11 test ownership/categories | Engine owns shared stat/lifecycle and moved position tests; Fast/Balance/Career traits introduced; Device remains exact-APK tier | Classify remaining legacy cases and migrate shared integration tests incrementally |
| A12 staff-lifecycle primitives | Pro coaches/scouts and College scouts now use shared shift and attribute-bound helpers; bounded annual checks pass | Keep different seed algorithms, growth/retirement policies and annual-call ownership separate |
| Phase validation/task components | Planned; not extracted | Share result contracts and task UI, not the leagues' phase graphs |
| Rating/physical/position adapters | Existing canonical calculations shared; consolidation pending | Audit remaining adapters and specialist/import cases |
| College.Core separation | Initial evaluation recommends deferring physical split while test ownership is incomplete | Inventory College-only dependencies before any move; preserve assembly/save compatibility |
| UI modeling caveats | Removed in71bcd3a; focused checks and compilation passed | Do not restore them; no APK/device rerun needed solely for this wording batch |

## What changed and where

### 76dcbe1 â€” first staff/profile fixes

- `src/DynastyManager.App/FoundationPage.TeamProfile.cs`: staff names use existing `CoachLink` / `ShowCoachProfileAsync`; user scouts are listed separately with assignments and readonly profiles. Opposing departments are not fabricated.
- `src/ProFootball.App/FranchisePage.MedicalStaff.cs`: opponent Edit attributes action hidden and its callback also checks team ownership. The existing editor lists user-team staff, so opponent navigation must not route into it.
- Both Android Compile targets passed. Existing focused staff/scouting regression cases:20 College and34 Pro passed. These are model/regression checks, not assertions of UI navigation.

### 83e2470 â€” device checkpoint and missing scout ages

- Initial device navigation checks recorded below.
- Old scouts without age metadata no longer display Age0; table uses a dash. An explanatory missing-history label was initially added, then removed by the user's later direction in71bcd3a.
- Missing-age follow-up compiled but was not included in the College APK used for the initial device checks.

### 71bcd3a â€” remove player-facing modeling caveats

- Removed College approximate-age markers, estimated recruiting-star labels/markers and coach-rating accuracy explanation.
- Removed Pro coach calibration/history panel and team-profile provenance text.
- Simplified the missing-age display, academic/scouting copy, field-goal probability label, contract cap preview and unavailable-statistics wording.
- No rating calculations, stored data, scouting mechanics, rules or aging changed.
- Focused absence checks for the removed text passed. Compile-only checks: College0 warnings/errors; Pro0 errors and2 existing CS8625 warnings.
- No broad tests, APK packaging or device checks were run for this wording-only commit.

## Important audit correction: existing editors

Both games already have basic player attribute editors. **Do not build a duplicate Pro player editor.**

Pro also already has `Player profile -> Edit appearance`, implemented by `EditPlayerAppearance` in `src/ProFootball.App/FranchisePage.Profiles.cs`. It supports skin tone, hair and facial hair and persists player appearance. This was confirmed in source, not by a new device roundtrip. Staff portrait customization remains a separate question. College has its own portrait editor and existing shared customization support.

## Device evidence â€” limited, exact-build scope

See [staff-device-checkpoint.json](audits/staff-device-checkpoint.json). These APKs correspond to the first staff implementation, not the later missing-age/text changes:

- College APK SHA-256: `818536e36e65ef2a319275ca96ffb61236a8968f86068d79beeb99957bec2390`.
- Pro APK SHA-256: `d730877d1285b85199f1846baf468df369e1ff4c8ae4d6f94537e0f641dc438e`.
- Both full local Debug packages succeeded: College0 warnings/errors; Pro2 existing nullable warnings/0 errors.
- College: loaded a Washington career in a readonly emulator clone; Roster -> Team profile & staff -> Jimmie Dougherty opened the correct profile; Back returned to team profile. Colin Rhodes opened his own scouting attributes and Region: West assignment. Numeric3-4 label and rating values appeared on team profile.
- Pro: loaded Seattle career in the clone; opposing Los Angeles Chargers -> Reese Brooks opened the correct medical profile without Edit attributes.
- Local screenshot/XML evidence: `.artifacts/staff-device`. Coach screenshot visually inspected. No save was deleted or reset.
- The test emulator was shut down. There is **no ongoing packaging job or active test session to resume**. Earlier notes about session32386 being active are obsolete.

When device work resumes, use the Android emulator QA skill and query actual devices first. Installed AVD at the last check: `pixel_9_-_api_36`. It was launched read-only/no-snapshot-save/no-window/no-audio on port5580. Preserve original AVD saves. Build the correct current APK, check package/version/hash, and derive taps from UI-tree bounds.

## Rules preparation â€” not conformance acceptance

Existing Release binaries passed40 shared `GameDayClockAndYacTests` / `CoinTossAndTurnoverTests` / `DeepCoverageAndKickoffTests` and13 Pro `FoundationTests`. These are baseline checks, not proof of every current league rule.

Sources located for the next stage:

- [Official2026 NFL rulebook](https://operations.nfl.com/rules-officiating/2026-nfl-rulebook). Do not substitute stale2024 overtime summaries.
- [NCAA approved timeout changes](https://www.ncaa.org/news/media-center-changes-to-injury-timeouts-approved-in-football/). Retrieve current full rules as needed.

Next scenarios: OT possession/period expiry and defensive scores, regular/playoff flags, kickoff recovery/touchback spots, first-down clock boundaries, sack accounting and live/fast-mode accounting. Preserve intentional league differences. Write a narrow failing scenario before changing behavior; do not make speculative rules fixes.

## Full planned work and acceptance criteria

### Simulation, rules and realism

- **A05 â€” College/NFL rules matrix:** cover overtime possession opportunities, period expiry, defensive scores, kickoff recovery/touchbacks, first-down clock boundaries, sack accounting and regular/postseason flags. Link each rule-sensitive change to the intended league policy and a focused scenario test. Keep source discussion in developer documents, not game screens.
- **Production-path comparison:** trace simulated, watched and user-controlled games to their actual production paths. Compare personnel eligibility/rotation, rating inputs, clock/rules and stat accounting for equivalent situations. Use controlled seeds where applicable; compare statistical distributions rather than requiring identical outcomes from paths with different random consumption. Do not compare an unused legacy simulator and call that production parity.
- **A06 â€” Pro punter aging:** make an explicit design decision for P rather than silently inheriting the default curve. Check K and P independently, ceiling preservation and growth/decline around the chosen peak. No new player-facing realism disclaimers.
- **Lifecycle validation after relevant changes:** use real/generated careers and check development, actual player participation, roster/cap/eligibility constraints, staff replacement, phase advancement and save/reload stability. Run targeted multi-year checks when lifecycle behavior changes; do not rerun broad careers for wording or handoff edits.

### Presentation and usability

- **A04 â€” Exact-APK acceptance:** check both apps in portrait/landscape and light/dark, rating-bar extremes, numeric formation labels, every relevant staff destination, Back navigation and save reload. Record exact package/hash and distinguish already verified cases from outstanding ones.
- **Shared staff-directory and rating components:** reuse row/profile identity contracts, rating bars and permission/action presentation in `Football.Presentation`. Pass stable IDs/team identifiers through callbacks. Keep app-state mutations in league adapters. Verify both hosts so a fix cannot land in only one game.

### Shared-engine and architecture

- **A10 â€” Statistical-query contracts:** standardize season/career scope, phase selection, duplicate-game handling, totals versus maxima and missing historical fields. Reuse existing `PlayerStatTotals` rather than adding a second aggregator. Keep College postseason and Pro regular/playoff inclusion policies in adapters. Characterize existing results before extraction and verify no historical totals change unintentionally.
- **A12 â€” Staff-lifecycle primitives:** extract deterministic seeded rolls, age/growth probability calculations and attribute bounds into `Football.Engine` where genuinely identical. Keep hiring, retirement, succession and department structure league-specific. Characterization tests must preserve seed inputs, random ordering, annual idempotence and reload behavior; do not change simulation balance as an incidental refactor.
- **Phase-validation results and task components:** share blocker/validation result contracts and reusable next-action UI. Keep College portal/recruiting/conference stages separate from Pro retention/FA/draft/camp stages. Verify legal transitions, blocked transitions, saved phase compatibility and coordinator authority.
- **Rating/physical/position adapters:** locate remaining duplicated lookups and delegate to existing canonical services where semantics match. Explicitly cover punters, kickers, fullbacks, conversions, generated players and imported identities. Preserve numeric position IDs, potential semantics, contracts and migration idempotence. Different aging policies are intentional, not duplicate overall formulas.
- **Later College.Core separation:** evaluate moving College-only recruiting/season services out of the nominally shared engine after A11 makes ownership clear. Inventory dependencies first. Preserve the existing `DynastyManager.Core` assembly identity and save/type compatibility unless a separately tested migration is necessary. This is an architectural option, not authorization for a disruptive directory/assembly rename now.

### Testing and maintenance

- **A11 â€” Test ownership:** introduce clear shared-engine, College and Pro ownership incrementally. Use categories or projects as appropriate; avoid duplicated test execution and avoid moving tests merely for cosmetic naming. Establish fast, balance, career and device tiers so workflows run checks appropriate to changed inputs. Verify discovery/filter behavior and retained coverage after each move.
- **Roadmap upkeep:** update `ROADMAP.md`, this handoff and audit status after each accepted batch. Mark historical plans superseded; distinguish implemented, device-verified, ongoing validation and optional work. Never promote candidate findings or old test totals to fresh acceptance.

### Refactoring order and boundaries

1. Stabilize staff/profile correctness and its device checks; then extract shared directory/rating components while those scenarios are fresh.
2. Establish A05 production behavior and A11 test ownership before broad engine restructuring.
3. Extract A10 stat-query contracts, phase-validation/task contracts and remaining rating adapters in small independent changes.
4. Extract A12 lifecycle primitives only after characterization coverage; run focused lifecycle checks if behavior changes.
5. Reassess optional College.Core separation last.

Do not merge the two save formats, application state/page classes, financial/recruiting economies, playoff systems or aging policies. Pro cap/contracts and College academics/recruiting stay league-specific. This work should reduce duplication without forcing identical gameplay.

## New implementation checkpoint â€” source b080ab7

- A04 core exact-build checks covered College rating bars without denominators, numeric `3-4`, coach/scout navigation, portrait and landscape, plus Pro opponent read-only/user-editable medical roles. Narrow-screen College scouting clipping was found; its Age column was removed and the two-column layout compiled.
- A05 matrix is in `audits/rules-conformance-matrix.md`. Four focused NFL tests now lock regular/playoff overtime, second possession after a touchdown, first-down/sack behavior and P/K aging. The matrix deliberately records remaining timeout/kickoff/safety/two-way-mode gaps.
- A06 sets P peak age to32 and K to33.
- A07 adds a Pro custom playbook editor with full defensive assignments and offensive route/block editing through the existing shared play services. It requires final device save/reload/execution verification.
- A08 adds persisted `ProCoach.Appearance` and a user-team coach portrait editor. Player appearance already existed and remains unchanged.
- Shared refactoring started: `StaffProfileComponents`, `PlayerStatTotals.GroupByPlayer` and `StaffLifecycleMath`; a dedicated `Football.Engine.Tests` project now owns two focused tests and is wired into the College/shared CI workflow.
- Checks at this checkpoint: rules/editor Pro tests6 passed; College staff/scouting/academics18 passed; shared project2 passed; both Android Compile targets passed. Final local APK hashes: Pro `e11b93af...81a5c`, College `003e3018...e800b`. Core staff/profile/portrait/navigation checks pass; custom-play save/reload and light-theme checks remain.

## Next actions for the incoming model

1. Read current status/diff and this handoff. Keep the latest caveat-removal preference and usage constraint.
2. Finish the remaining A04 scenarios on the latest changed APKs as a focused device batch; do not relabel earlier APK checks as validation of newer source.
3. Proceed to A05's explicit rules matrix and only demonstrated fixes.
4. Then implement the agreed custom-play/staff-portrait parity and specialist-aging decisions; reuse existing services and preserve IDs/saves.
5. Continue the shared-component, A10/A11/A12 and adapter work above; use the dependency order rather than treating it as omitted or completed. Evaluate College.Core separation only afterward.
6. Update this page after each reviewable batch with commit, exact checks, failures/limitations and the next action. Commit/push to the feature branch; do not merge or publish automatically.

The released401 Pro/760 Core-College test totals are prior evidence and were not rerun for this handoff or the latest wording changes. No production changes are part of this handoff-only update.


## Rating quality color palette

A shared `RatingQualityColors` palette now drives player/staff attribute bars, rating badges, quality comparisons, College conduct meters, conference-power charts, rank charts and Pro efficiency comparisons. Bands: red below50, orange50s, yellow60s, green70s, teal80s, blue85-89 and violet90+. Light and dark themes use contrast-adjusted variants. Neutral volume/cap/draft charts retain their existing semantics; only charts where relative quality is meaningful use the rating palette. Both Android Compile targets pass after this presentation change.

## November 2 continuation â€” runtime source 0138a4a

The latest user scope is **only** the continuation items listed in chat: device/editor acceptance, rules/mode checks, shared-code opportunities and repository/release follow-through. The older design backlog is excluded. The root checkout and unrelated branch workflow edit remain preserved.

### Concrete fixes and verification

- Reproduced A07 on a newly packaged APK: `Edit / copy` used the page-wide `Action` guard and awaited the modal's completion. That retained `busy`, causing modal Save and Cancel actions to be ignored. `PlayEditorAction` now guards only its own button, allowing modal actions to finish. Both offensive and defensive copy entry points use it.
- Pro Game plan pickers now use canonical playbook labels (including numeric `3-4`/`4-3`) and map selections by enum index. Stored enum values are unchanged.
- Both current Debug APKs packaged locally with Visual Studio MSBuild. The `dotnet build -t:SignAndroidPackage` attempts stalled and were canceled; direct MSBuild completed. The final Pro compile reports the two existing CS8625 warnings; College has no reported warnings. APK/package hashes and precise device scenarios are recorded in the continuation checkpoint.
- College exact APK: light-theme portrait program profile visually checked (green75/teal82 bars, no denominator, numeric3-4), landscape profile captured; own medical, strength/conditioning and chief-scout identities plus Back checked. This is a subset of A04, not the complete role/theme/extreme-rating matrix.
- Pro exact fixed APK: custom offensive call created through the UI, saved to the library and franchise, restarted and reloaded; saved JSON retains its ID/assignments and selected-playbook ID. The reloaded `Inside Zone Custom` was selected in the live picker and executed: Charbonnet ran1 yard, moving Q1 15:00/first-and10 at SEA35 to14:56/second-and9 at SEA36. User-coach appearance was saved and its Light/Brown/Buzz cut/Clean shaven selections match after full process restart. No edits were made directly to the saved game.
- **54 focused xUnit cases pass:** Engine Fast12 (including10 moved position cases), mode comparison12 (36 completed CPU-delegated games), rules boundary20, P/K cohort2 (64 seeds each,12 annual steps), Pro annual staff4 and College annual scouting4 (six annual steps per seed). These are bounded fixtures, not a fresh full career/balance suite. See [rules matrix](audits/rules-conformance-matrix.md) and [test ownership/refactoring inventory](audits/test-ownership-and-refactoring.md).
- Pro attribute bars already use shared `CoachAttributeViews`/`PlayerAttributeViews`; no private Pro `ProgressBar` remains. No cosmetic replacement was made merely to use a second shared component.
- Pro light portrait opponent coach profile visually checked with green/teal attribute bars; correct Jim Harbaugh identity and absence of Edit portrait asserted from the UI tree. Light landscape profile captured. The unfinished test game was discarded through the UI; previously saved season/playbooks remained intact.
- Initial College.Core separation evaluation recommends deferring the physical split until shared-test ownership/dependency inventory is clearer. No namespace, assembly or save migration was attempted.

### Remaining at0138a4a checkpoint (historical; superseded below)

- Finish the exact-APK role/theme matrix, extreme rating colors and defensive copy-editor device roundtrip as recorded in the checkpoint. The offensive custom-call end-to-end and one opponent-coach read-only scenario pass; do not generalize them to every role/editor/formation.
- Actively controlled-call distributions and broader real/generated Simulate/Watch/Play integration. The new mode comparison delegates all calls to CPU.
- Safety/free-kick behavior is a confirmed source gap: negative gains are clamped to the one-yard line and no safety/free-kick state exists. Add a failing scenario, confirm league policies and implement scoring/accounting/presentation/save behavior together. Do not mark it covered by the new boundary tests.
- Full dynamic kickoff/recovery/formation/touchback scenarios, broader timeout/period combinations, live-ball penalty/late-half runoff/untimed-down enforcement.
- A10 broader query scope/adapters; phase blocker/task contracts and UI; remaining physical/rating/import/specialist adapter coverage; stable-ID staff-directory row/action contracts where useful.
- Continue A11 legacy ownership/category classification. New Fast/Balance/Career traits do not classify every historical long test. Device remains a separate exact-APK tier.
- A12 now has bounded annual characterization for both leagues; further extraction must preserve each seed algorithm, caller-owned annual application, ordering and distinct hiring/retirement policies.
- The feature branch is not merged and no new prerelease/CI artifact is published. Existing local APKs are development packages, not release assets. Finish acceptance before integration/release; preserve the unrelated workflow edit. Local cleanup remains deferred.

## Safety and shared-code continuation â€” runtime0a4b430

This supersedes the earlier scrimmage-safety, basic query-scope and staff-bound extraction pending notes. Scope remains the user's listed issues; the older design backlog remains excluded.

- **36fc92c:** ordinary run/sack/kneel losses can produce safeties. Defense receives2 points, underlying play statistics remain intact, losses stop at the goal line, drive/period/highlight totals agree, and optional event flags survive serialization. The conceding team free-kicks from20. Normal and onside free kicks preserve their origin and actual personnel; subsequent normal kicks clear safety state. Period/halftime, tied regulation, regular/postseason overtime and CPU kneel boundaries are covered. Shared commentary/banner/animation origins now recognize the safety. **0a4b430** preserves the punt-touchback banner's receiving20 while kickoff banners use their actual awarded spot.
- **53e318a:** `PlayerStatScope`/`PlayerStatQuery` share season/team/player selection and completed-game `(SeasonYear, GameId)` deduplication. First played input wins, retaining current-before-archive precedence. Game-backed queries use game year even if an old line lacks year metadata. Pro's regular/playoff adapter and College's current-year rebuild remain separate. Counter/max totals and missing tracked history remain unchanged.
- **53e318a:** Pro coaching/scouting and College scouting now call the existing shared attribute-bound helper. Seed algorithms, thresholds, random/attribute evaluation order and hiring/retirement/annual-call policies are unchanged.
- **Checks:** four initial safety reproductions failed before the fix. The final safety matrix passes25 cases. The affected shared clock/kick/stat/mode set passes138 cases (including22 initial safety cases), Pro rules/foundation17, Engine14, Pro lifecycle/query6 and College annual scouting4. Result sets overlap; do not sum repeated runs as new coverage. Both Android hosts compile; Pro retains the two existing CS8625 warnings. Final local package details belong in `audits/safety-query-checkpoint.json`; earlier palette device evidence applies to its recorded APK hashes only.
- **Packaging/device boundary:** APKs are local Debug builds, not GitHub Actions artifacts or prereleases. No new safety-specific device scenario or complete all-role/theme matrix has been run for this runtime. Do not relabel the earlier custom-play/palette device evidence as acceptance of these newer binaries.

### Next work, still within the approved list

1. Exact-APK safety banner, score/box/drive views and free-kick playback; remaining role/theme/extreme-rating and defensive editor scenarios.
2. Full dynamic kickoff landing-zone/touchback and modern Pro formation restrictions; loose-ball/return/foul/try safeties; broader live-ball penalty, runoff and untimed-down cases. The implemented safety path is ordinary scrimmage play, not every impetus/penalty exception.
3. Actively controlled-call and broader real/generated mode distributions. CPU-delegated parity is already covered.
4. Broader league competition-phase/historical query adapters; phase blocker/task result contracts and reusable task UI; remaining physical/position/import adapters and stable-ID staff-directory components.
5. Continue incremental legacy test ownership/categories. Keep College.Core physical separation deferred under the recorded evaluation. No shared seed/hash replacement or league-policy merger is planned.
6. Integrate/release only after acceptance; preserve the unrelated Pro workflow edit and deferred local cleanup. No new merge or prerelease has occurred in this continuation.

## Active continuation â€” runtime5239013

Supersedes the pending phase/task, basic directory-contract, competition-query, specialist-adapter and production-mode notes above. Work continues on the same branch; older design backlog excluded. The unrelated workflow edit is preserved.

- **Dynamic Pro kickoffs:** league policy distinguishes an untouched end-zone kick (35), landing-zone bounce into the end zone (20), and a kick from midfield (20). Events retain landing/dynamic flags for replay. Ten coverage players align at receiving40; ten setup players occupy30â€“35 with six at35. Non-kickers wait until first touch; the kicker stays within the allowed advance before first touch. College alignment/touchback policy remains separate. Both huddle preview and kick events now select the same special-teams unit. Ordinary, safety and onside origins remain distinct. Sources: official2026 NFL rules6-1, linked in the rules matrix.
- **A10:** phase-tagged game records share competition/season/team/player selection and duplicate precedence. Pro excludes preseason and tags regular/playoff records. `CollegeStatGames` distinguishes conference championships from bowls/playoffs and regular games. Competition filtering on old aggregate rows throws rather than inventing unavailable phase history. Existing aggregate history is retained.
- **Phase/task extraction:** `PhaseValidation` keeps blocker code, message and next-action key together. Pro guide and transition gates share one result; College's fired-coach gate uses the same contract. `PhaseTaskComponents` supplies the common two-column layout while each app retains styles, actions and authority. Saved phase enums and league phase sequences are unchanged.
- **Staff directories:** `StaffDirectoryComponents.Name` shares stable-ID link metadata/accessibility and host navigation callbacks. College coach/scout links and Pro team-directory coaching/medical/scouting links use it. League permissions and profile content remain local.
- **Adapters:** Pro generated FB/K/P codes delegate to canonical position codes. New specialist/fullback conversion/physical tests preserve identity, measurements, potential bounds and idempotence. Position-group import codes and depth-specific OL/DE/OLB choices remain intentionally distinct.
- **A11:** `build/test-tier.ps1` selects an explicit Fast/Balance/Career category and Engine/College/Pro owner project. Established College career/calibration classes are classified, including inherited twenty-year fixtures; discovery checked. College phase policy tests live in College, not Engine. Unclassified legacy cases are not implicitly fast. Device acceptance remains separate and requires exact APK identity.
- **Production comparison:** [768-game report](audits/mode-conformance-results.md),32 games per league/roster/mode/control cell. CPU completed-game records match across Simulate/Watch/Play; explicit offense/defense calls,11 unique players per side, clock bounds and target accounting pass on real/generated rosters. Reported controlled distributions retain intended Play timeout/kneel authority. The audit exposed CPU kneels incorrectly tagged User; fixed with a two-league regression.
- **Focused checks:** Engine30; initial College kickoff/phase/safety/rules49; College tagged-game/kickoff-preview3; Pro phase/stat/rookie/ratings22; expanded timeout/penalty/safety71 (overlapping sets; do not sum). Both hosts compile, with the two pre-existing Pro nullable warnings.
- **Build/acceptance ongoing:** GitHub College run41481512750 and Pro41481518336 target5239013. They were started, not yet accepted at this checkpoint. Local packages created before the last animation/metadata fixes are not final acceptance evidence. A reproducible production-loader device fixture finds an early safety, preserves its seed, validates the subsequent free kick and saves a separate completed box-score fixture. No app backdoor or original save edit is needed.

Remaining work is exact final-APK device acceptance (staff/theme/orientation/extremes, defensive custom-play roundtrip, safety/free-kick playback), the still-unsupported live-ball penalty/runoff/untimed-down and return/try safety cases, then final repository/release follow-through. Do not mark those complete from engine tests alone. Initial College.Core evaluation remains complete with physical separation deferred. Local cleanup stays deferred.

### Corrected build/device checkpoint â€” runtime2f1eae5 / College1b118a2

- Pro150 failed one obsolete blanket35-yard touchback assertion (other412 passed). Updated it to distinguish the event's landing-zone bounce. Pro151 was superseded/cancelled.
- Device inspection of College440 reproduced opponent coaches exposing a working portrait editor. `PortraitCustomizationService` now rejects non-user-program staff by stable ID; College hides that control and Pro's editor callback also checks team ownership. Twenty focused portrait/scoring cases pass, including all seven opponent staff roles and same-name own/opponent identities. College441 caught an overly broad UI replacement in the player profile;1b118a2 corrects it and the local College Compile target passes.
- **Pro152** run41483346127:413 tests pass; APK packages with the two existing nullable warnings. Artifact13161805763 (`pro-football-apk-152`). Downloaded APK SHA256 `84b686d84d6f9fa0a03da3faa75b18988dbc39ac7a5cee96be57084771ae74cd`.
- **College442** run41483840728: Engine30 and scoped College7 pass; packaging0 warnings/errors. Artifact13162155152 (`dynasty-manager-build-442`). Downloaded APK SHA256 `d2c6251c40f4bf39c0c2ee2dbe0ea2483527e1e65f34890e3d8775ca8386cea3`.
- College442 installed in the read-only emulator clone. Separate palette career captures0/49/50/59/60/79/89/100 coaching bars in dark/light portrait/landscape. Screenshots visually reviewed: readable labels, empty/full endpoints and range colors. Program75/82 bars and numeric3-4 remain present. Role/permission and editor/gameplay acceptance is still running; do not infer it from the palette captures.
- Pro Fast tier now includes27 passing focused cases; Pro Balance discovery lists the two coach-ability cases. Established long College tests were discovered, not rerun. The standalone device fixture reproduces its safety at event23/seed208 after actual `ProSaveStore.Load`, then validates its free kick and separately saves a completed box-score career. Only fixtures use artificial extreme ratings; original saves/AVD data are preserved.

### Continued exact-device findings â€” runtime3d722b5

- College442: all six hired own roles and all seven opponent roles open the correct identity and attribute type, with correct own/opponent portrait controls, in light and dark portrait. Own-role dark landscape attribute captures also pass. Program profile/palette covers both themes/orientations and0/100 endpoints; representative medical/scout screenshots reviewed. User coach Beard/Gray saves and survives full force-stop/relaunch/reload. All five actual department scouts (Colin Rhodes, Owen Bishop, Blake Hart, Evan Kim, Trevor Mercer) resolve individually and Back preserves the program directory's scroll position.
- Device inspection found the College team-profile directory omitted the separately stored user coach.3d722b5 adds `CollegeStaffDirectory.Team`, reusing the existing role lookup for both directory renderers. Three HC/OC/DC fixtures pass with and without a duplicate stored role. Assigned-role profile callbacks retain role context when resolving an ID. Local College Compile passes. College443 run41488693966 succeeded, artifact13163521466, APK SHA256 `53fce6c11b483e5da04371abf2cb7856784752ad0cc1e20a7fa6188d9132f33b`; its new user-row native check is still required.
- Pro153 run41488699569 attempt1 failed the existing three-season cap/roster case (other412 passed):2028 Philadelphia active16, room25,997,183, dead cap170,834,016. Three exact local reproductions with phase/state traces passed through2029. Root cause is unconfirmed; the invariant/test was not weakened and no financial policy was changed. An unchanged-job rerun is in progress; retain the failed attempt as an open intermittent lifecycle finding even if retry passes.
- Pro152 native editor acceptance is underway on a separate staged career. It includes the already saved offensive custom play and user-coach appearance, with no original franchise slot overwritten. Defensive library/save/restart/execution and safety/free-kick native checks remain active.

### Defensive editor completion and live-call correction

- Pro153 installed from successful run41488699569 attempt2:413 tests pass, packaging two existing warnings. Artifact13163664070; SHA256 `0f895a543258a88b1624e0502ccb8ae7a83cc9fa454a73eb0b9b9aa0881c4a31`. Attempt1's intermittent roster/cap failure remains recorded; retry success does not resolve its root cause.
- On Pro152 the UI copied `Cover 3 Buzz Custom`, saved it, deselected its stock source and selected the copy within14 defensive calls. Persisted ID `custom-bcba72001c274b59a424b8ef3da06cbf`,11 assignments,14 selected. After full process restart/update, Pro153 reloads it. In a controlled game the UI called that defense: Q1 14:54/1st-and10 LAC36 became14:50/1st-and10 LAC46 after a10-yard Herbertâ€“Johnston completion, with no error. This completes the basic defensive Save/restart/reload/execute roundtrip on those APKs.
- Native inspection then exposed the shared live picker's full-library bug: it displayed the deselected stock `Cover 3 Buzz`, despite the engine rejecting it. `GameCastLivePage.Choices` and huddle previews now use `GamePlanService.Available*Plays` for the active book. Two College/Pro engine contract cases pass; both hosts compile. Final native verification must confirm the excluded stock call disappears while the custom call remains usable.
- Kickoff flight now starts at the recorded kicking line, rather than the kicker's run-up position five yards behind it. Punt launch remains player-anchored. Final free-kick playback still requires device verification.
- Existing workflows gain opt-in `parity` scope: Pro's explicit Fast tier; College/shared engine ownership plus the focused rule, staff, portrait and custom-call cases. Native acceptance remains the separately recorded exact-APK pass. Existing fast/full/career options and assertions are unchanged. Actionlint passes both modified workflows. The unrelated dirty `pro-android-branch.yml` is still excluded.

### Current acceptance checkpoint â€” Pro156 / College444

- Current exact APKs and test counts are in `audits/parity-acceptance-current.json`. Pro156 run41498375895 succeeds with27 focused cases and the two existing build warnings; artifact13166459226, SHA256 `23dae56791e929d6c5610d5b49f2f48b254ba8d16075dac1b88d9b6664ac0b49`. College444 run41494626705 succeeds with Engine30 + parity103,0 build warnings; artifact13166002329, SHA256 `d2db41c68bfed414c6553b3db10bbc4ccf645b0d8f21df2577d3a3742e2abf9c`.
- Pro153's nine own coaching/medical/scouting entries pass dark portrait/landscape identity/attribute/permission/Back checks. Pro155's same nine entries pass light portrait. Representative0/100 coaching and medical range screenshots visually reviewed. These remain per-APK evidence; the shared profile/palette inputs are unchanged by the later live-picker and private portrait navigation fixes.
- Native Pro155 Save portrait exposed a second profile pushed over the old profile; one Back returned to the stale profile.8a4feb2 pops the old profile before displaying the updated one. Pro156 now returns to the directory in one Back after Save. Full-restart persistence and opponent-role checks are running; do not mark them complete yet.
- The first focused Pro154 build passed its tests/compile but failed copying legacy management fixtures that parity does not generate.470caf5 skips that staging only for parity; existing native/full scopes retain it. Pro155/156 and College444 are successful replacement candidates. No runtime code was changed to silence the intermittent financial test; the successful413-case retry and failed attempt remain documented.
- Live custom-call verification on Pro153: `Cover 3 Buzz Custom` executed with LAC36 â†’ LAC46, Q1 14:54 â†’14:50,10-yard Herbertâ€“Johnston completion. The engine contract rejects the deselected stock source; the next exact-APK picker check must confirm it is no longer exposed after730d751.


### Running-clock penalty accounting follow-up — November 2

The game clock already consumed pre-snap time before a dead-ball foul, but the foul event omitted that elapsed time. Added four failing College/Pro and offense/defense cases, then recorded `LastPreSnapSeconds` on the event. All68 focused clock/penalty scenarios now pass, including unchanged down/first-down enforcement and no play/yardage credit. No new foul types or enforcement policies were added.

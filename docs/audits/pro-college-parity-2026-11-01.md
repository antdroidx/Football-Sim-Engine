# Pro / College parity and shared-engine audit

**Audit date:** November 1, 2026 (America/Los_Angeles). **Baseline:** main99254c7 / released Pro149 and College439; runtime inputs0bf0989. **Method:** source and test inspection, targeted test reruns, existing CI/device evidence review and GitHub inventory. This is an engineering parity audit, not a fresh official-rulebook, all-screen device or full multi-season certification.

## Executive assessment

The games already share their most important on-field simulation and presentation infrastructure. The next productive work is closing navigation/data-visibility holes and strengthening cross-league contracts, not moving every similarly named class into one engine. College recruiting, eligibility, academics and postseason formats must remain separate from Pro cap, contracts, draft and roster obligations.

The previous parity checklist is obsolete: it lists career statistics, medical/staff depth, shared GameCast and financial budgets as missing even though later releases implemented them. The current roadmap below supersedes historical forward-looking text without deleting the historical acceptance record.

## Evidence levels

- **Confirmed source gap:** a concrete current call path/model difference is visible; runtime severity still needs the stated scenario.
- **Candidate:** negative search or policy asymmetry, requiring product intent or runtime confirmation.
- **Intentional:** league-specific behavior; not a missing feature just because another game differs.
- **Opportunity:** maintainability recommendation, not a proven user defect.
- Existing401 Pro /760 Core-College release passes are reused, not described as newly rerun full suites. New focused TRX counts are recorded in audit-evidence.json. No production code changes or fresh APK builds are part of this audit.

## Capability matrix

| Area | College | Pro | Assessment | Evidence under src/ |
|---|---|---|---|---|
| Live simulation | Shared GameDayEngine and live controls | Shared engine; professional FootballRules adapter | Shared; retain league rules | Football.Engine/Simulation/GameDayEngine.cs; ProFootball.Core/ProLiveControl.cs |
| Clock and scoring rules | Late-half first-down clock stop, college PAT/hash/OT | Different pacing, hash/PAT/touchback; timed OT | Intentional; rule conformance needs dedicated audit | Football.Engine/Models/FootballRules.cs; Simulation/GameDayEngine.Clock.cs |
| Fast versus live simulation | DeterministicGameSimulator / WeekSimulation and live engine | ProLeagueService profiles and live production engine | Validation gap: compare statistical distributions across every mode | Football.Engine/Simulation/DeterministicGameSimulator.cs; ProFootball.Core/ProLeagueService.cs |
| Player attributes/overall | Canonical attribute catalog and positional weights | ProPlayerRatingService delegates to canonical weights | Already shared; do not reimplement | Football.Engine/Simulation/PlayerRatingService.cs; ProFootball.Core/ProPlayerRatingService.cs |
| Potential and aging | Student development and class-year lifecycle | Age/position prime, ceiling, decline and retirement | Intentional policy difference; share arithmetic only | Football.Engine/Simulation/PlayerDevelopmentService.cs; ProFootball.Core/ProDevelopment.cs |
| Position fit/conversion | Shared fit and conversion model | Pro adapters/editor use shared player model | Mostly shared; test natural specialist fallback identically | Football.Engine/Simulation/PositionFit.cs; PlayerPositionConversion.cs |
| Specialist careers | Distinct K/P positions | Distinct K/P; P falls through PeakAge default | Review candidate: punter peak age defaults to28 versus kicker33 | ProFootball.Core/ProDevelopment.cs:PeakAge |
| Rosters/depth | Scholarship/eligibility/recruiting and walk-ons | Active/reserve/camp/waiver/contract constraints | Intentional acquisition constraints; share unit eligibility contracts | Football.Engine/Simulation/DepthChartService.cs; ProFootball.Core/ProDepthChart.cs |
| Personnel/injuries | Shared on-field rotations/injury model | Shared field model plus Pro reserve and medicine policies | Share injury events, keep eligibility/calendar policy separate | Football.Engine/Simulation/PersonnelRotationService.cs; InjuryService.cs; ProFootball.Core/ProReserve.cs |
| Season phases | Conference championship, postseason, portal, recruiting, roster management | Preseason, regular season, playoffs; retention/FA/draft/camp | Intentional; common transition guard contract is feasible | Football.Engine/Models/SeasonPhase.cs; ProFootball.Core/Models.cs; ProOffseasonFlow.cs |
| Standings/postseason | Polls, conferences, bowls and college playoff field | Divisions, wild cards, NFL tiebreakers and playoff tree | Intentional; do not merge tournament rules | Football.Engine/Simulation/CollegeFootballPlayoffService.cs; ProFootball.Core/ProStandings.cs |
| Player acquisition | Recruiting, geography, academics, transfer portal | Draft, free agency, trades, waiver claims | Intentional; prospect reports can share presentation | Football.Engine/Simulation/InteractiveRecruitingService.cs; ProFootball.Core/ProScouting.cs |
| Draft exchange | Exports full departing class | Imports/translates attributes, fills small classes | Implemented; preserve IDs/portraits, test class timing | Football.Engine/Simulation/PlayerExportService.cs; ProFootball.Core/PlayerExchange.cs |
| Coaching attributes | Canonical eight attributes and prestige-sensitive starts | Same attributes with role/history-based estimates | Shared schema; different starting calibration intentional | Football.Engine/Models/CoachingAttributes.cs; ProFootball.Core/ProTeamStrategyCatalog.cs |
| Coach calibration | RealCoachCsvImporter changes names while retaining game skills | 26 observed HC samples; other HCs/coordinators club proxies | Explicit limitation, not evidence of equivalent personal realism | DynastyManager.Data/Import/RealCoachCsvImporter.cs; ProFootball.Core/ProCoachHistoryCatalog.cs |
| Staff lifecycle | Coach/medical/scout development, retirement and succession | ProStaff and department-specific annual lifecycle | Similar mechanisms duplicated; extract deterministic primitives first | Football.Engine/Simulation/StaffManagementService.cs; ProFootball.Core/ProStaff.cs |
| Scouting organization | Separate user ScoutingStaff/ScoutAssignments; chief scout in generic staff | Team-wrapped ProScout for all clubs | Structural difference; College directory does not enumerate actual department | Football.Engine/Models/ScoutStaff.cs; Simulation/ScoutingDepartmentService.cs; ProFootball.Core/ProScoutDepartment.cs |
| Staff directory navigation | Team profile staff names are TableText | Coach, medical, scout names now navigate | Confirmed parity gap: College staff links missing | DynastyManager.App/FoundationPage.TeamProfile.cs; ProFootball.App/FranchisePage.cs |
| Staff profile editing | College profile/editor infrastructure | Opponent medical profile exposes Edit attributes; editor lists user club | Confirmed route mismatch risk: opponent selection cannot resolve in editor | ProFootball.App/FranchisePage.MedicalStaff.cs; FranchisePage.Editor.cs |
| Team strategies | 138 sourced College identities | 32 Pro identities | Catalog data league-specific; plan contracts shared | Football.Engine/Simulation/CollegeTeamStrategyCatalog.cs; ProFootball.Core/ProTeamStrategyCatalog.cs |
| Academic quality/prestige | Dynamic prestige; stable academics/recruit pitch; bars | No university academics; club owner expectations instead | Intentional, do not port academics to NFL | Football.Engine/Simulation/TeamAcademicQualityCatalog.cs; ProFootball.Core/ProCareer.cs |
| Finance | College acquisition/roster economy differs | Contracts/cap/bonus/waiver/trade obligations | Intentional; practical estimates accepted, source-perfect phase closed | docs/pro-practical-finance-policy.md; ProFootball.Core/ProFinance.cs |
| Statistics | Shared recorded player/stat/efficiency data with College aggregation | Regular/playoff/current/career filters and deduplication | Shared aggregation primitives exist; unify query specification next | Football.Engine/Simulation/PlayerStatTotals.cs; ProFootball.Core/ProPlayerStatistics.cs |
| Analytics/news | College-specific awards, recruiting and program history | Draft/development comparisons, league review, projections | Shared factual event/query contracts; different editorial topics | Football.Engine/Simulation/DynastyNewsService.cs; ProFootball.Core/ProNews.cs; ProAnalytics.cs |
| Custom plays | Dedicated PlayEditor page and validation service | No Pro app custom-play editor reference found | Candidate missing feature; decide whether deliberate before porting | DynastyManager.App/FoundationPage.PlayEditor.cs; Football.Engine/Simulation/PlayEditorService.cs |
| Portrait editing | Dedicated portrait editor uses shared customization service | Player profile has Edit appearance / EditPlayerAppearance; staff customization still needs review | Player appearance parity already implemented; staff parity unverified | DynastyManager.App/FoundationPage.PortraitEditor.cs; ProFootball.App/FranchisePage.Editor.cs |
| Presentation | Large FoundationPage partials and MAUI management shell | Large FranchisePage/FrontOfficePage partials; same data-grid primitives | Share components, not a giant shared page/state class | Football.Presentation/FootballDataGrid.cs; GameCastLivePage.cs |
| Saves/migration | Dynasty persistence and legacy/current import compatibility | Independent Pro save schema and additive migration | Keep schemas separate; share atomic write/version guard utilities only | DynastyManager.Data; ProFootball.Core/ProSaveStore.cs |
| Testing structure | Core plus College cases in one project | Separate Pro tests reference shared engine | Confirmed organization debt: shared tests are not independently owned | tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj; tests/ProFootball.Tests/ProFootball.Tests.csproj |
| Release validation | 760 cases in final439 CI; no fresh final UI walkthrough | 401 cases in final149 CI; no fresh final UI walkthrough | Evidence gap: final profile changes need exact-APK native checks | docs/releases/preview-2026-11-01.md |

## Prioritized findings and acceptance criteria

### A01 · P1 · College team-directory links

**Evidence:** Confirmed source gap.

Use existing College staff profile navigation for each staff name, with user/CPU cases and Back navigation. Keep department and coaching roles distinct.

### A02 · P1 · College scouting directory completeness

**Evidence:** Confirmed structural gap.

Show actual user ScoutingStaff plus assignments. For opposing programs expose only genuinely persisted chief-scout data; do not fabricate a full department or borrow the user scouts.

### A03 · P1 · Opponent medical edit target

**Evidence:** Confirmed source route mismatch; UI reproduction pending.

Either make opponent profiles read-only or pass explicit staff ID/team into editor. Verify viewing never edits a different club, and define user editing permissions for every staff type.

### A04 · P1 · Final-build UI acceptance

**Evidence:** Validation gap.

Verify both released APKs: bars at0/99, light/dark, portrait/landscape, all staff-name destinations, numeric schemes, Back and save reload. Latest compile/unit passes are not visual proof.

### A05 · P1 · Rules conformance matrix

**Evidence:** Unverified realism risk.

Add explicit College/NFL scenario tables for OT possession/period expiry, kickoff recovery/touchbacks, first-down clock boundaries, sacks and postseason flags. Compare current official sources before changing policy; this audit is not a fresh rulebook certification.

### A06 · P2 · Punter career curve

**Evidence:** Confirmed fallback; design correctness unverified.

PeakAge explicitly handles K=33 but P uses default28. Decide a sourced/design target and test P independently; do not silently equate all specialists.

### A07 · P2 · Custom-play editor parity

**Evidence:** Negative source-search candidate.

Trace supported Pro play selection and decide whether to expose existing PlayEditorService. Require save roundtrip and live simulation before calling it complete.

### A08 · P2 · Portrait customization parity

**Evidence:** Follow-up source tracing corrected the original negative search: `FranchisePage.Profiles.cs` exposes Player -> Edit appearance and persists player skin/hair/facial-hair changes. Do not implement a duplicate player portrait editor. Staff portrait customization remains a separate unverified parity question. Verify existing appearance save/reload and inspect staff identity storage before adding staff customization.

### A09 · P2 · History-calibration transparency

**Evidence:** Known modeling limitation.

College real names retain generated ratings; Pro coordinators use club proxies. Label assumptions consistently and add personal-tenure evidence only if worth the maintenance cost.

### A10 · P2 · Stat query parity

**Evidence:** Architecture opportunity.

Define common season/game-phase/career scope, duplicate policy, maxima and missing-data behavior; keep league-specific inclusion rules in adapters.

### A11 · P2 · Test ownership and long-run coverage

**Evidence:** Confirmed organization debt.

Create Football.Engine.Tests, College.Tests and Pro.Tests categories incrementally. Label fast, career, balance and device scopes; run multi-season audits when lifecycle behavior changes, not on every documentation commit.

### A12 · P3 · Department lifecycle duplication

**Evidence:** Architecture opportunity.

Extract seeded random-roll, age/growth probability and attribute clamp helpers after cross-league characterization tests. Preserve each league's retirement and hiring policies.

## Shared-code extraction plan

| Sequence | Extract/reuse | Destination | Boundary and acceptance |
|---|---|---|---|
| 1 | Staff directory row/profile identity contracts and rating-bar components | Football.Presentation | Callbacks receive stable IDs and team; no imports of app state or Pro core. Test read-only versus editable profiles. |
| 2 | Player-stat query scope and aggregation contracts | Football.Engine | Preserve regular/playoff/college postseason semantics, duplicate IDs, maxima and absent historical fields. |
| 3 | Staff deterministic roll and bounded aging primitives | Football.Engine | Characterize existing outputs first; preserve franchise seed, ordering and reload determinism. Keep hiring/retirement policy in league adapters. |
| 4 | Transition validation results and task presentation | Engine contracts + Presentation | Share blocker messages/results; keep portal/recruiting and cap/FA/draft transition graphs separate. |
| 5 | Rating/physical/position lookup adapters | Existing engine services | Reuse current canonical calculations instead of adding competing overall formulas. Explicitly test P/K/FB and imported identities. |

**Do not centralize:** salary-cap accounting into College, recruiting/academics into Pro, one unified save file, one giant application page, one retirement curve, or one playoff bracket. `Football.Engine` currently contains substantial College-specific season/recruiting code despite its neutral name. A later `College.Core` extraction is reasonable but should follow test separation; avoid a disruptive rename/move merely to make the directory look generic. Existing `DynastyManager.Core` assembly identity is compatibility-sensitive.

## Recommended execution order

1. Close A01-A03 and exact-APK device acceptance A04 as a small management UI batch.
2. Define the rule-conformance scenarios A05 and compare live/fast statistical output; fix only demonstrated differences from chosen league policies.
3. Resolve specialist aging A06, custom plays A07 and portrait editing A08 with explicit product decisions and narrow tests.
4. Separate test ownership A11, then extract stat queries A10 and lifecycle primitives A12 in independently reviewable changes.
5. Re-run real/generated multi-season roster/development/phase audits after lifecycle or rule changes. Include player participation, cap/eligibility, staff replacements and save/reload; do not merely count that a loop completed.

## Verification and limitations

Source evidence is relative to the baseline checkout, with primary paths in the matrix; code existence alone is not complete behavior acceptance. Existing rules were inspected without importing new legal/official-rule claims. Candidate realism fixes require current official sources in their implementation batch. Negative searches establish missing app entry points in the inspected files, not impossibility anywhere in the repository.

The original working checkout is dirty and64 commits behind remote main. It contains significant user changes and is **not** the audit baseline. Those files and the other worktrees are preserved. The audit runs from the released checkout with its unrelated pending branch-workflow edit excluded from commits.

GitHub cleanup and new focused test outcomes are recorded in [audit-evidence.json](audit-evidence.json). Historical links to removed superseded APK artifacts are archival references; current APK downloads use the permanent prerelease. All non-APK Actions evidence, release assets and final149/439 runs are retained.

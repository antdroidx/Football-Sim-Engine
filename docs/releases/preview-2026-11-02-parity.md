# Pro162 & College450 — Parity, Roster Safeguards and Rules

Prerelease for November 2, 2026. Full changelog since `preview-2026-11-01-staff-gamecast` (Pro149 / College439, baseline `99254c7`). Includes all completed parity, editor, shared-code, roster-safety and supported additional-rules changes merged into main.

## Downloads

- **Pro Football Franchise Manager 0.1.162:** `Pro-Football-Franchise-Manager-0.1.162.apk`; package `antdroid.profootball.franchise`, version code162.
- **College Football Dynasty Manager 0.20.450:** `College-Football-Dynasty-Manager-0.20.450.apk`; package `antdroid.cfbcoach.dev`, version code850.
- Development-signed Debug APKs for Android sideloading, retaining their existing package identities. The College development package remains separate from `antdroid.cfbcoach`.
- `SHA256SUMS.txt` and `release-manifest.json` identify the exact APKs and their successful GitHub Actions builds. The application source is unchanged from build commit `cb16217` (runtime `f4c3bbf`); subsequent commits update documentation only.

## Both games

- Use range colors consistently for player/staff ratings and quality-based analytics: red below50, orange50s, yellow60s, green70s, teal80–84, blue85–89 and violet90+. Light/dark variants retain readable text and visible bars. Neutral counts and financial-volume graphs retain their own meaning.
- Improve staff-directory profile destinations, real scout visibility and role-specific coaching/medical/scouting attributes. Shared name links carry stable identities and accessibility labels. Preserve each game's navigation and edit permissions.
- Keep opponent staff portraits read-only, including the College save boundary. Preserve own-program customization.
- Display numeric3-4/4-3 formation labels consistently in profile and playbook pickers.
- Remove player-facing data-model/calibration caveats while retaining useful gameplay information and internal provenance.
- Score ordinary run, sack and kneel safeties for the defense; retain underlying rushing/sack statistics and correct period, drive and highlight accounting. The conceding team free-kicks from20; normal/onside origins, halftime, game-end and overtime boundaries are handled independently.
- Launch kickoff flight at the recorded kicking line, with the kicker starting the run-up behind it.
- Preserve the punt-touchback banner at receiving20; kickoff banners use the actual awarded spot.
- Share special-teams selection between kickoff huddles and the actual kick event. Preserve11 unique personnel per unit.
- Retain CPU provenance for automatically called kneels.
- Remove the College-only sack-accounting caption from the shared box score; league calculations remain separate.
- Record elapsed pre-snap time on dead-ball foul events, matching the game clock without crediting a scrimmage attempt or yardage.
- Honor metric-specific quality colors for signed analytics values, including lower-is-better metrics and intermediate comparison values.

## Pro

- Add custom offensive/defensive playbook authoring using the existing shared play services. Copy plays, edit routes/blocks/defensive assignments, save the library and select calls within17-offense/14-defense limits. Fix the modal busy guard that prevented Save/Cancel from responding. Live call selection now follows the active playbook instead of exposing excluded library calls.
- Add saved coach portrait appearance and an own-team portrait editor. Saving replaces the old profile so one Back returns to its directory. Opponent coach editing is hidden and guarded.
- Set explicit specialist aging peaks: punters32 and kickers33, with separate seeded multi-year cohort checks.
- Lock regular/postseason overtime durations and possession-reply behavior, first-down clock differences and Pro sack accounting.
- Implement the modern dynamic kickoff distinction: untouched end-zone touchback35, landing-zone bounce into end zone20 and midfield-origin touchback20. Align coverage/setup units, freeze non-kickers until first touch and limit early kicker advance. Preserve those flags for replay.

## College

- Improve staff/scout profile navigation and narrow-screen scouting layouts. Include the separately stored user head coach/coordinator in the program directory without duplicate roles.
- Use canonical coaching attributes for Leadership/Development summary tiles so they agree with the Attributes view.
- Keep all opponent staff portraits read-only; own staff and user-coach appearance remain customizable.
- Retain separate conference-title, bowl/playoff, recruiting and season rules rather than adopting Pro policy.

## Shared engine and maintenance

- Add competition/season/team/player statistical-query contracts, deterministic duplicate precedence and Pro regular/playoff adapters. Explicit College game tags distinguish conference championships from postseason. Aggregate-only old history does not invent competition detail.
- Share phase blocker/reason/next-action results and the two-column task layout while retaining each league's phase enums, transition policies, styling and authority.
- Share bounded staff attribute arithmetic after annual reload/idempotence characterization, preserving seed order and league hiring/retirement rules.
- Delegate Pro generated FB/K/P position codes to canonical adapters; cover specialist/fullback identity, measurements, conversion, potential and idempotence.
- Establish Engine/College/Pro test ownership and explicit Fast/Balance/Career execution tiers. Device acceptance remains a separate exact-APK tier. Defer physical College.Core separation after evaluating dependency/save compatibility.
- Add reproducible real/generated Simulate/Watch/Play comparison and production-loader device fixtures. Keep the roadmap, rules matrix and implementation handoff current.

## CPU roster safeguards and additional rules

- Pro CPU cap repairs preserve minimum roster size and positional depth; affordable replacements commit atomically. Emergency recruitment reserves rookie money and uses legal restructures when possible. Optional contract length considers four-year funding.
- Both games gain common live-ball penalty enforcement, league-specific DPI/grounding statistics, late-half runoff/timeout avoidance and untimed downs.
- Correct return/impetus/momentum and foul safeties, defensive try returns and one-point safeties, credited scoring teams, safety kicks, displays and accounting. Punt landings inside the five no longer become early touchbacks.
- Shared simulation/rules tests move from College to Engine, with a common fixture and CI guards against empty test selections.
- Fresh768-game cross-mode comparison passes; earlier APK evidence retains its exact-build scope.

## Validation

- Main Pro162: [run41574780264](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41574780264),419 Pro scoped regressions pass; APK packages with two existing nullable warnings and zero errors.
- Main College450: [run41574780256](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41574780256),319 Engine Fast,12 Engine Balance and602 College checks pass; APK packages with zero warnings/errors.
- A fresh768-game production comparison covers both leagues, real/generated rosters, CPU/controlled calls and Simulate/Watch/Play. CPU-delegated completed records agree across modes; personnel, clock bounds and target/coverage accounting pass. Controlled Play can differ because of timeout/kneel authority. This is bounded comparison evidence, not full statistical calibration.
- Six focused CPU safeguards plus the original three-season financial invariant pass separately. Earlier overlapping test runs are not additional coverage.
- Earlier exact-APK staff/palette/portrait/custom-play acceptance remains scoped to its recorded binaries in [the parity manifest](https://github.com/antdroidx/Football-Sim-Engine/blob/main/docs/audits/parity-acceptance-current.json). These final APKs have automated/build verification; no fresh full-device walkthrough is claimed.

## Known limits and follow-ups

- The reproduced destructive CPU cap-repair path is fixed. The original intermittent career failure's complete trace remains unavailable. No dead cap was forgiven, cap increased or roster assertion weakened; debt or market shortages can still make a minimum roster unaffordable.
- Additional officiating implements the common-foul subset described above. It does not implement every personal/illegal-kick foul, post-possession-change foul combination or full referee/replay procedure. Automatic coaching policy selects penalties/runoff/timeouts with engine overrides; no new interactive penalty-decision screen.
- A new full-device walkthrough and longer career/balance stress tests remain future validation work. Physical College.Core separation remains deferred after its compatibility evaluation.
- Local cleanup remains deferred; unrelated local workflow/root changes are preserved. The older design backlog was excluded from this release scope.

## Source and handoff

[Full commit comparison](https://github.com/antdroidx/Football-Sim-Engine/compare/preview-2026-11-01-staff-gamecast...preview-2026-11-02-parity-rules) · [Implementation handoff](https://github.com/antdroidx/Football-Sim-Engine/blob/main/docs/ROSTER-RULES-VALIDATION-HANDOFF.md) · [Exact validation/build manifest](https://github.com/antdroidx/Football-Sim-Engine/blob/main/docs/audits/roster-rules-acceptance.json)

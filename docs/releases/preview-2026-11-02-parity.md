# Parity continuation — release candidate

Draft changelog for the approved parity work since published main99254c7 (Pro149 / College439). Final acceptance and publication status belongs in the implementation handoff. Current Actions candidates are Pro160 and College448; these are CI artifacts until published as a release.

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

## Validation and artifacts

The current candidate manifest, exact hashes, workflow links/counts and per-APK native acceptance boundaries are in [parity-acceptance-current.json](../audits/parity-acceptance-current.json). Pro160 and College448 package successfully; Pro retains two existing nullable warnings. Focused CI checks pass27 Pro,30 Engine and107 College/shared integration cases. Earlier unchanged-Core Pro153 attempt2 passes413 cases.

The [768-game mode comparison](../audits/mode-conformance-results.md) passes across real/generated rosters, both leagues, all modes and CPU/controlled calls. Native defensive copy save/restart/reload/execution passes on Pro153. Native palette/staff/portrait checks and the current-build live-picker/safety acceptance are recorded separately in the manifest.

One three-season CPU roster/cap case failed in Pro153 attempt1, passed three exact local repeats and passed the unchanged CI retry. Its root cause remains unconfirmed; it is retained as a lifecycle finding. No assertion was weakened or financial policy changed.

Scoped native acceptance is complete; per-APK evidence and reuse boundaries are recorded in the manifest. This draft does not claim a published prerelease or a complete official-rule implementation.

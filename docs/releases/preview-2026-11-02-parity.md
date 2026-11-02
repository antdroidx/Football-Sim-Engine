# Parity continuation — release candidate

Draft changelog for the approved parity work since published main99254c7 (Pro149 / College439). Final acceptance and publication status belongs in the implementation handoff. Current Actions candidates are Pro152 and College442; these are CI artifacts until published as a release.

## Both games

- Use range colors consistently for player/staff ratings and quality-based analytics: red below50, orange50s, yellow60s, green70s, teal80–84, blue85–89 and violet90+. Light/dark variants retain readable text and visible bars. Neutral counts and financial-volume graphs retain their own meaning.
- Improve staff-directory profile destinations, real scout visibility and role-specific coaching/medical/scouting attributes. Shared name links carry stable identities and accessibility labels. Preserve each game's navigation and edit permissions.
- Keep opponent staff portraits read-only, including the College save boundary. Preserve own-program customization.
- Display numeric3-4/4-3 formation labels consistently in profile and playbook pickers.
- Remove player-facing data-model/calibration caveats while retaining useful gameplay information and internal provenance.
- Score ordinary run, sack and kneel safeties for the defense; retain underlying rushing/sack statistics and correct period, drive and highlight accounting. The conceding team free-kicks from20; normal/onside origins, halftime, game-end and overtime boundaries are handled independently.
- Preserve the punt-touchback banner at receiving20; kickoff banners use the actual awarded spot.
- Share special-teams selection between kickoff huddles and the actual kick event. Preserve11 unique personnel per unit.
- Retain CPU provenance for automatically called kneels.

## Pro

- Add custom offensive/defensive playbook authoring using the existing shared play services. Copy plays, edit routes/blocks/defensive assignments, save the library and select calls within17-offense/14-defense limits. Fix the modal busy guard that prevented Save/Cancel from responding.
- Add saved coach portrait appearance and an own-team portrait editor. Opponent coach editing is hidden and guarded.
- Set explicit specialist aging peaks: punters32 and kickers33, with separate seeded multi-year cohort checks.
- Lock regular/postseason overtime durations and possession-reply behavior, first-down clock differences and Pro sack accounting.
- Implement the modern dynamic kickoff distinction: untouched end-zone touchback35, landing-zone bounce into end zone20 and midfield-origin touchback20. Align coverage/setup units, freeze non-kickers until first touch and limit early kicker advance. Preserve those flags for replay.

## College

- Improve staff/scout profile navigation and narrow-screen scouting layouts.
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

- [Pro152 Actions run](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41483346127):413 tests pass; packaging succeeds with two pre-existing nullable warnings. [APK artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41483346127/artifacts/13161805763), SHA256 `84b686d84d6f9fa0a03da3faa75b18988dbc39ac7a5cee96be57084771ae74cd`.
- [College442 Actions run](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41483840728):30 Engine and7 scoped College tests pass; packaging has0 warnings/errors. [APK artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41483840728/artifacts/13162155152), SHA256 `d2c6251c40f4bf39c0c2ee2dbe0ea2483527e1e65f34890e3d8775ca8386cea3`.
- [768-game mode comparison](../audits/mode-conformance-results.md) passes across real/generated rosters, both leagues, all modes and CPU/controlled calls. Focused timeout/penalty/safety71 and portrait/scoring20 pass; overlapping result sets are not summed.
- Native acceptance is ongoing. Do not treat this draft as evidence of completed editor/gameplay/role-matrix acceptance or a published prerelease.

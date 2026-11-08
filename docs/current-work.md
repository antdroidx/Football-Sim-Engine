## GitHub Actions APK verification - November 8, 2026

Run [42462711585](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585) completed successfully on `74dba82ca44d21cc8ff912e8643abbdc49f3ff39`; all five jobs are green.

Remote APKs:

- College `dynasty-manager-build-9`, artifact [13610775614](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585/artifacts/13610775614): `antdroid.cfbcoach.dev-Signed.apk`, 91,584,562 bytes, version code `1409` / `0.20.1009`, SHA-256 `49EDB218AA800F8E819D188699ED6BC6000672E5803FDBE77912F1DB61510407`.
- Pro `pro-football-apk-9`, artifact [13610627106](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585/artifacts/13610627106): `antdroid.profootball.franchise-Signed.apk`, 90,305,033 bytes, version code `1009` / `0.1.1009`, SHA-256 `979B879CBBF26456ADCA9838F1B629003BEF6A1F49EFCC26DF0EEAA3E89A9897`.

Both APKs pass `aapt dump badging` package checks and `apksigner verify` with APK Signature Scheme v2 and v3 and certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`, the same signing key as the published versions. Device evidence is [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585/artifacts/13610783883), [College launch](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585/artifacts/13610603131), and [College career options](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42462711585/artifacts/13610995507). These are Actions artifacts, not GitHub releases.
## Jet Sweep handoff at the quarterback - November 8, 2026

The Jet Sweep now runs the way the play is drawn. The receiver starts in his wide alignment, motions toward the quarterback, the ball is snapped to the quarterback, the exchange happens at the quarterback's hip, and the receiver runs from there.

The root cause was in how the live motion path is applied. `View.TranslateToAsync` replaces the marker translation instead of adding to it, so the pre-snap motion measured every waypoint from the previous one and left the receiver a single segment away from his alignment when it ended. The receiver barely appeared to move, and the snap animation then placed him at the real handoff point, which read as a jump next to the quarterback. `PreSnapMotionPlan.PathOffsets` now returns the offset of every waypoint measured from the player's alignment, and the live motion animates those offsets in order.

The path itself was corrected as well. `PreSnapMotionPlan.JetPath` produces the pre-snap waypoints and `HandoffStartAlignment` returns that path's last point, so the receiver is no longer re-placed at the snap. The path only moves toward the quarterback: the previous shape overshot the mesh by 0.2 yards and stepped back, and the handoff point sat 0.75 yards deeper than the quarterback, which put the receiver behind the quarterback instead of beside him. The handoff now lands at the quarterback's lane.

The ball and the receiver also share one track. `GameDayAnimationPlanner.CarrierTrack` carries the receiver to the mesh and then into the run, and the ball follows that exact path, so the ball changes hands where the handoff happens instead of jumping to a receiver who is already running. The ball transfer moved from 22% to 18% of the play.

Device evidence: a fresh Arizona Cardinals franchise in the Pro app on a read-only API 36 emulator, `Pre-snap: Jet motion left`. Before the fix the receiver moved no measurable distance during the motion and then appeared next to the quarterback at the snap. After the fix the receiver travels the whole path over the pre-snap window and is already beside the quarterback when the ball is snapped, the carrier highlight moves from the quarterback to the motion man at the mesh, and the motion man continues into the run. Frames are under `.artifacts/jet-qa/`.

Validation actually run on the implementation working tree:

- Focused `PreSnapMotionPlanTests|JetSweepChoreographyTests|PreSnapTests|GameDayPreSnapPresentationTests|PreSnapMatchupTests|SpecialTeamsCallTests`: 44 passed.
- Football.Engine full suite: 431 passed.
- College standard suite excluding `DynastyHealthTests|OvrCalibrationDynastyTests`: 629 passed.
- Pro standard suite excluding `TwentySeasons|SourcedFiveSeasons`: 485 passed and the documented intermittent `CapInitializationTests.ThreeSeasonsPreserveImportedCapRolloverDraftAndCpuBudgets` failed once after 3m40s. The exact isolated retry passed 1/1 in 1m24s with no source or assertion change.
- Football.Presentation Debug compile: zero warnings/errors.
- College Android Debug APK packaging: zero errors.

Local Debug APKs:

- College: `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk`, 91,580,466 bytes, SHA-256 `EFE766CCF9B9E0EF5FAFF3408A237BF2CF2D5833849146B5BE2540CE370B7488`.
- Pro: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, 89,966,191 bytes, SHA-256 `D210403ADC4A33EAF8A02071A65C123A72CE402B291D79584C7ACF327D829255`.

Both local APKs pass `apksigner verify` with signer certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`, the same key as the published versions.
## GitHub Actions APK verification - November 8, 2026

Run [42444981350](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350) completed successfully on `c9f0dd458a2e374ec0e9a3a14f84b853e7e4f071`; all five jobs are green. The earlier run [42443560962](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42443560962) failed on the pre-fix Pro staff seed collision and is superseded by this run.

Remote APKs:

- College `dynasty-manager-build-8`, artifact [13603336531](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350/artifacts/13603336531): `antdroid.cfbcoach.dev-Signed.apk`, 91,584,562 bytes, version code `1408` / `0.20.1008`, SHA-256 `C2A53CBDB2BB8C25D7B605B790D134AE7A1DA27EEBF2F609D335B3E97249CFBC`.
- Pro `pro-football-apk-8`, artifact [13603920389](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350/artifacts/13603920389): `antdroid.profootball.franchise-Signed.apk`, 90,305,033 bytes, version code `1008` / `0.1.1008`, SHA-256 `D71CE1C1419E669BC937C9EE39A6F317500115294AB628F971D13E194347FF5A`.

Both APKs pass `aapt dump badging` package checks and `apksigner verify` with certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`. Device evidence is [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350/artifacts/13603618998), [College career](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350/artifacts/13602714411), and [College management](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42444981350/artifacts/13603443353). These are Actions artifacts, not GitHub releases.
## Motion continuity, huddles and punt formations - November 8, 2026

GameCast now clears huddles before every snap sequence, including a pre-snap penalty, so alignment and motion cannot render over a huddle. Pre-snap path segments use incremental translations rather than cumulative offsets. Jet motion stops just short of the quarterback mesh at the snap, continues through the handoff after the snap, and transfers the ball while the receiver is still moving before the run begins.

Punt choreography now has a dedicated return-unit shape: deep returner, split edge gunners, upbacks and a compact front near the line. The coverage unit separately aligns the punter, snapper, interior line and split gunners. Punt defense no longer reuses kickoff or generic defensive coordinates.

Focused motion/timing/kick tests: 29 passed. Football.Engine: 423 passed. College standard suite excluding `DynastyHealthTests|OvrCalibrationDynastyTests`: 629 passed. Pro standard suite excluding `TwentySeasons|SourcedFiveSeasons`: 486 passed. Android UI helpers: 24 passed. Packaged-resource validation and Python smoke compilation passed. Football.Presentation compiled with zero warnings/errors.

Local Debug APKs:

- College: `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk`, version code `400` / `0.20.0`, 91,580,466 bytes, SHA-256 `5ED1726A14CED65C0DBA19481280A058FA126CC4B30F80339B4A861DD880BF58`.
- Pro: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, version code `1` / `0.1.0`, 90,300,937 bytes, SHA-256 `9E8BEC7D8981A767B6CBFD8A1CC70B9B8A74F625E4FB0337B9B7C56C57BC9756`.

College Android Debug packaging: zero warnings/errors. Pro Android Debug packaging: zero errors and the two documented `CS8625` warnings.
## Previous GitHub Actions APK verification - November 8, 2026

Run [42430915440](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440) completed successfully on `323924b06e8ccd08e9d0700a911ea73e6a5f9ecf`; all five jobs are green.

Remote APKs:

- College `dynasty-manager-build-6`, artifact [13597054979](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440/artifacts/13597054979): `antdroid.cfbcoach.dev-Signed.apk`, 91,580,466 bytes, version code `1406` / `0.20.1006`, SHA-256 `5A76C5399588BD4B1DD213B2D1F20D55D2E19B47B75A52A74441165D0542C33A`.
- Pro `pro-football-apk-6`, artifact [13598165735](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440/artifacts/13598165735): `antdroid.profootball.franchise-Signed.apk`, 90,300,937 bytes, version code `1006` / `0.1.1006`, SHA-256 `9AE121F3299751ED3C73AB8D1946ACA438C770F25471ECE944568660B78BE863`.

Both APKs pass `aapt dump badging` package checks and `apksigner verify` with certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`. Device evidence is [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440/artifacts/13598212607), [College career](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440/artifacts/13597955695), and [College management](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42430915440/artifacts/13597952724). These are Actions artifacts, not GitHub releases.
## Game-day timing and special teams - November 7, 2026

The shared pre-snap hold is now 2 seconds from formation visibility to snap. Jet motion still reserves its final 780 ms path segment inside that window and waits for both the formation timer and motion task. Two-point conversions now expose an extra-point choice, a generic two-point call, and playbook run/pass cards. A selected conversion records that play and uses normal personnel, routes, ball ownership and replay animation.

When the CPU is about to punt or attempt a field goal, the defensive call sheet offers `Punt return · strong protection` / `Punt block` or `Field goal safe` / `Field goal block`. Strong protection reduces return risk; block calls increase the chance of a recorded blocked kick and animate the rusher to the kick point.
## Coverage-aware pre-snap motion outcomes - November 7, 2026

Pre-snap movement now changes the play through an explicit coverage response instead of a fixed positive yard bonus. Man coverage carries the assigned defender with the motioning receiver and preserves the original matchup. Zone coverage passes the receiver off to the defender responsible for the new alignment, so the matchup can change. Focused bracket coverage carries the primary outside receiver through motion and denies the intended mismatch. The same reaction drives passing separation/catch risk, run advantage, the bounded recorded yard effect (-2 to +2), and GameCast's defender tracking; the animation uses `CoverageDefenderId` rather than the eventual tackler. Ordinary balanced formations leave the defense set; only meaningful strength or extreme spread/heavy formations trigger a small automatic defensive adjustment.

Generic pre-snap options now expose `Motion left`, `Motion right`, `Motion slot`, and `WR swap`. `Motion left` moves the right-side WR to an open inside/tackle-box spot, `Motion right` mirrors that with the left-side WR, and `Motion slot` sends the slot receiver to the opposite side of the offensive line. Relocated and swapped receivers run their assigned routes from the new role/alignment. `WR shift` is removed from the menu; legacy `Shift` and `ReturnToFormation` records remain only for older saves. Jet motion now reserves its final 780 ms path segment so the receiver reaches the quarterback mesh as the 2-second formation lead expires, and the snap waits for both the formation timer and the motion animation.

Focused and required validation on this implementation:

- Focused `PreSnapMatchupTests|PreSnapTests|PreSnapMotionPlanTests|GameDayPreSnapPresentationTests|SpecialTeamsCallTests`: 35 passed.
- Football.Engine full suite: 418 passed.
- College standard suite excluding `DynastyHealthTests|OvrCalibrationDynastyTests`: 629 passed.
- Pro standard suite excluding `TwentySeasons|SourcedFiveSeasons`: 485 passed and the documented intermittent `CapInitializationTests.ThreeSeasonsPreserveImportedCapRolloverDraftAndCpuBudgets` failed once after 3m40s. The exact isolated retry passed 1/1 in 1m24s with no source or assertion change.
- Android UI synchronization helpers: 24 passed. Packaged-resource validation: passed. Both smoke Python files compile.
- Football.Presentation Debug compile: zero warnings/errors.
- College Android Debug APK packaging: zero warnings/errors.
- Pro Android Debug APK packaging: zero errors and the two documented `CS8625` warnings.

Local Debug APKs:

- College Debug: `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk`, version code `400` / `0.20.0`, 91,241,624 bytes, SHA-256 `7105AAC7175A15BCBA31A27C5FC210AEE10FFA2A2563749F3AF8C61FD72A5D3A`.
- Pro Debug: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, version code `1` / `0.1.0`, 90,296,841 bytes, SHA-256 `ADC7C7BFC8866171F557FC6A8CAE8B30A6D8C6854637226FC4290ABB150A589D`.

Both local APKs pass `apksigner verify` with certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`.

Previous GitHub Actions verification - November 7, 2026

Run [42425206521](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521) completed successfully on `5bb3fa342c25b4542d17aed5433afb4d3fb1e673`; all five jobs are green.

Remote APKs:

- College `dynasty-manager-build-5`, artifact [13595600936](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521/artifacts/13595600936): `antdroid.cfbcoach.dev-Signed.apk`, 91,572,274 bytes, version code `1405` / `0.20.1005`, SHA-256 `98196B65DF9F721ACD4AFD384215DB3B827EECE90C061AD21E0AE2D6D8EA3BD7`.
- Pro `pro-football-apk-5`, artifact [13595144432](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521/artifacts/13595144432): `antdroid.profootball.franchise-Signed.apk`, 90,292,745 bytes, version code `1005` / `0.1.1005`, SHA-256 `192185CBB8131B0E8A0B6E93E2A339D72503D430B68B79B5365411E4F7EE23D0`.

Both APKs pass `aapt dump badging` package checks and `apksigner verify` with certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`. Device evidence is [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521/artifacts/13595573121), [College career](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521/artifacts/13595456922), and [College management](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42425206521/artifacts/13595504115).

The prior GitHub Actions record below is historical. Run [42417369639](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639) completed successfully on `ba9c0479e4408d48d59700bb29c8384e78b847e3` after an isolated retry of a College launcher ANR. All build, test and device jobs are green, including the corrected Pro live flow.

- College `dynasty-manager-build-4`, artifact [13591734358](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639/artifacts/13591734358): `antdroid.cfbcoach.dev-Signed.apk`, 91,564,082 bytes, version code `1404` / `0.20.1004`, SHA-256 `8CCB1E018210ED54B61A6B23AC52A7A647241F42B482223BD094CD5DDF25DC25`.
- Pro `pro-football-apk-4`, artifact [13592755842](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639/artifacts/13592755842): `antdroid.profootball.franchise-Signed.apk`, 90,284,553 bytes, version code `1004` / `0.1.1004`, SHA-256 `99E37F6BDC6892231BA5CCE87E740F4F354CFCEAD28B5C6FAB3ED6131049F3E8`.

Both CI APKs pass `apksigner verify` with the same stable certificate. Device evidence is in [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639/artifacts/13592892689), [College career](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639/artifacts/13592888999), and [College management](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42417369639/artifacts/13592468175). These are Actions artifacts, not GitHub releases. No PR, main merge, release, or long career-suite run is claimed here.
## APK signing and update compatibility - November 7, 2026

The APK signing key did not change. Current, prior CI and published release APKs all report signer certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7` (`CN=CFDM Dev`). The earlier wrapper builds failed to update existing installs because the wrapper workflow had an independent run counter and produced lower version codes: College `401` versus installed `880`, and Pro `1` versus installed `205`. `adb install -r` reproduced `INSTALL_FAILED_VERSION_DOWNGRADE` for both.

Commit `5794061` adds a reusable `version_code_offset`; `presnap-apk-validation.yml` passes `1000` while direct workflow runs retain their normal numbering. Corrected workflow [42384796728](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42384796728) completed successfully across both builds and all College/Pro device jobs.

New update-compatible artifacts:

- College `dynasty-manager-build-2`, artifact [13579291526](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42384796728/artifacts/13579291526): `antdroid.cfbcoach.dev-Signed.apk`, 91,555,890 bytes, version code `1402` / `0.20.1002`, SHA-256 `38226C6B3F7DFB7B55A628A4FA3E892AEAF8F43709B6774E08C27EF731D83BDB`.
- Pro `pro-football-apk-2`, artifact [13579237991](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42384796728/artifacts/13579237991): `antdroid.profootball.franchise-Signed.apk`, 90,276,361 bytes, version code `1002` / `0.1.1002`, SHA-256 `FEA2405CF6FEB1A7D6D668C38981B0B2C0A241C6ADE1488C5AB4051579D9C9F1`.

A disposable API 36 emulator first installed College `880` and Pro `205`, reproduced both downgrade failures with the old wrapper APKs, then successfully installed `1402` and `1002` over the same package data using `adb install -r`. Final `dumpsys package` values matched the new manifests.

Package identity remains important: the College Debug/release asset package is `antdroid.cfbcoach.dev`; the retained non-dev identity is `antdroid.cfbcoach`. Those are separate Android apps and cannot update each other. Pro remains `antdroid.profootball.franchise`.
## Pre-snap formation, motion and APK validation workflow - November 7, 2026

Shared GameCast now keeps a called formation on screen for a minimum of 2 seconds before the snap. Defensive players automatically adjust after the offensive formation is visible, offensive motion/swap is limited to selected receiver alignment actions, and play-specific motion uses the resolved play instead of a stale call-sheet lookup. Jet Sweep animates the selected receiver from his WR alignment toward the quarterback mesh and keeps that receiver as the ball carrier.

Dead-ball false starts and offside now record formation play IDs, selected personnel and an explicit `IsPreSnapPenalty` marker. GameCast renders the aligned formation and completes pre-snap movement before revealing the foul banner; live-ball penalty playback remains unchanged. Older `PreSnapChoice` and `GameSnapEvent` records continue to deserialize with their previous defaults.

`.github/workflows/presnap-apk-validation.yml` reuses the established College and Pro workflows through `workflow_call`. A push to `codex/presnap-formation-motion-fixes` runs the standard tests, builds both signed Debug APK artifacts, and enables the existing College and Pro emulator/live scopes. No duplicate APK jobs or release workflow were added.

Validation actually run on the implementation working tree:

- Engine focused pre-snap/presentation/penalty suite: 34 passed.
- Engine Fast: 392 passed. Engine Balance: 12 passed.
- College `GamePlanTests`: 14 passed.
- Pro `FootballRulesConformanceTests`: 5 passed.
- College standard suite excluding `DynastyHealthTests|OvrCalibrationDynastyTests`: 629 passed.
- Pro standard suite excluding `TwentySeasons|SourcedFiveSeasons`: 485 passed and the documented intermittent `CapInitializationTests.ThreeSeasonsPreserveImportedCapRolloverDraftAndCpuBudgets` failed once after 3m32s. The exact isolated retry passed 1/1 in 1m29s with no source or assertion change.
- Packaged-resource validation: passed.
- Android UI synchronization helpers: 24 passed.
- College Android Debug APK build: passed with zero warnings/errors.
- Pro Android Debug APK build: passed with zero errors and the two existing `CS8625` warnings.

Local APKs were generated at `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk` (91,217,048 bytes, SHA-256 `DCE724671091D8C6D727A64EF0B75A12DA9BBC6FCB2B74B37536F6042CBB451B`) and `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk` (89,937,519 bytes, SHA-256 `069FB775EFEB9D151DE26C48D617F12EB4A9D84891AB90C8153CCCF5E71D02C1`). Both are Debug `net10.0-android` builds.

The `Pre-snap APK validation` wrapper ran as [Actions run 42354394728](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42354394728). College build/tests and both College emulator jobs passed. Pro tests and APK packaging passed. The first Pro device pass stopped on a stale smoke-harness label (`Motion right 2 yd`) after the UI correctly changed to `WR return motion right`; `build/pro-android-smoke.py` was corrected without changing application source.

Exact CI APK artifacts from that run:

- College `dynasty-manager-build-1`, artifact [13567340936](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42354394728/artifacts/13567340936): `antdroid.cfbcoach.dev-Signed.apk`, 91,555,890 bytes, version code 401 / version name 0.20.1, SHA-256 `76B2341FBA5B70D849C7C584F76F780929ED0367638D16980EA9C4FB22311642`.
- Pro `pro-football-apk-1`, artifact [13567213115](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42354394728/artifacts/13567213115): `antdroid.profootball.franchise-Signed.apk`, 90,276,361 bytes, version code 1 / version name 0.1.1, SHA-256 `D82305076AB8504E39FDB73D4926D0F096C565EE6A3CF69480DE7E15F718F78C`.

Both exact downloads pass `apksigner verify` with APK Signature Scheme v2 and v3 and retain the expected application IDs.

The corrected full Pro live/device flow then passed against that exact Pro APK in [Actions run 42356094914](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42356094914), including franchise creation, watch/replay, landscape, save/reload, week advance and coach mode. Its UI evidence is artifact [13568385823](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42356094914/artifacts/13568385823).

A later evidence-only exact-APK run, [42357416270](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42357416270), was cancelled at the 25-minute device timeout after uploading `pro-ui-evidence-207`; it is not acceptance evidence. Focused local Android emulator QA on `pixel_9_-_api_36` then selected `WR return motion right` and captured a 150 ms screenshot burst. Frames from 0.45-1.05 seconds differed only in the player-alignment field region and settled by 1.20 seconds, while the 3.0-second frame remained `Pre-snap formation`.

## Play-aware motion Actions APKs - November 6, 2026

GitHub Actions completed successfully for implementation commit `0613ebab6d8d51feeafb735f01f08bd7ef18281a` on `codex/presnap-gamecast-corrections`:

- College Android run `42328614160` passed 386 Engine Fast, 12 Engine Balance, 629 combined Core/College tests, packaged-resource and UI-helper checks, and APK upload.
- Pro Football Android run `42328614715` passed 486 standard Pro tests, UI-helper and NFLPA normalization checks, and APK upload.

Exact Actions APK artifacts:

- College `dynasty-manager-build-480`, artifact `13556765851`: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42328614160/artifacts/13556765851. APK `antdroid.cfbcoach.dev-Signed.apk`, 91,543,602 bytes, version code 880 / version name 0.20.480, SHA-256 `38D0B21697CF0D9B01129FD2E3D2E280B71EB447794F2B4A3074C2D19EAD8BC2`.
- Pro `pro-football-apk-205`, artifact `13556697357`: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42328614715/artifacts/13556697357. APK `antdroid.profootball.franchise-Signed.apk`, 90,268,169 bytes, version code 205 / version name 0.1.205, SHA-256 `AC385F2E0E28828020D949827BAB652F4AB1FBD46F9874EAFFD377ADFD4DFF5C`.

Both exact downloaded APKs pass `apksigner verify` with APK Signature Scheme v2 and v3. These are CI artifacts, not GitHub releases. No PR was created.

## Play-aware pre-snap motion - November 6, 2026

Pre-snap motion is now play-specific instead of a generic two-yard excursion that always returns to the same spot. The shared motion plan records the selected player role, motion family, final lane/depth, swap partner and handoff responsibility. GameCast and play art use the same plan:

- `JetHandoff` moves a named receiver behind the line to the quarterback mesh, keeps him there through the snap and gives him the ball on the Jet Sweep call.
- `SetAcross` moves a player to a new formation alignment and keeps him there.
- `OrbitAcross` carries a player behind the formation to the opposite-side alignment and keeps him there.
- `SwapAlignments` exchanges two named players and their route roles.
- Older generic choices retain `ReturnToFormation` behavior for save compatibility.

The existing `off-spread-jet-zone` ID remains stable for playbooks and saves, but its call is now `Jet Sweep`; its concept and runner behavior make the motion receiver the ball carrier. Set/orbit/swap calls settle before snap, while Jet motion hands off directly at the snap without an artificial return hold. Each formation family in an active standard playbook now retains its motion call instead of allowing the Hail Mary reservation to displace it.

A related play-script bug was fixed: opening-script position now advances per team and per scrimmage play, so opponent possessions and clock-only events cannot consume the user's scripted call sequence.

Validation actually run:

- Focused motion/rule/timing suite - 16 passed, including Jet Sweep selecting the motion receiver as `BallCarrierId`.
- Focused game-plan suite - 14 passed, including Spread Jet/Set/Swap availability and Play Action Orbit/Set availability.
- Engine Fast - 386 passed; Engine Balance - 12 passed.
- College standard suite excluding `DynastyHealthTests|OvrCalibrationDynastyTests` - 629 passed.
- Pro standard suite excluding `TwentySeasons|SourcedFiveSeasons` - 486 passed.
- Packaged-resource validation and 24 UI synchronization helper tests passed.
- Both Android compile targets passed with zero warnings/errors.

Android QA used local Pro Debug APK SHA-256 `67B2C3B4B8C1BE4057F733C046492458BF7F37EB92FE0C758590E80BF258F62D` on a read-only API 36 emulator. The full scrollable playbook exposed the motion calls below the initial fold. `Orbit Motion PA Boot` auto-selected `Pre-snap: Orbit motion right`; timed frames show the on-field formation, motion to the opposite alignment, the settled `PRE-SNAP FORMATION`, and the following snap/run. A field-only frame diff isolates the moving-player region. Jet handoff is covered by the engine snap test because Seattle's Play Action plan correctly offers Orbit/Set calls rather than the Spread-only Jet Sweep call. Logcat has zero fatal exceptions, application ANRs, application process errors or GREF warnings.

Not claimed: CI completion for this revision, physical-device installation, or a device walkthrough of the Spread-only Jet Sweep call.
## GitHub Actions APK handoff - November 6, 2026

The shared GameCast corrections are pushed on `codex/presnap-gamecast-corrections` at `5d6947c8b42c6703bf81ffe2cd831f4051d8594d`. The existing College and Pro workflows were dispatched for that exact commit; no PR, merge, release, or prerelease was created.

- College Android run `42321359769` passed: 382 Engine Fast, 12 Engine Balance, 629 combined Core/College tests, packaged-resource and UI-helper checks, Android Debug APK packaging and artifact upload.
- Pro Football Android run `42321359983` passed on its failed-job retry: 486 standard Pro tests, UI-helper and NFLPA normalization checks, Android Debug APK packaging and artifact upload. The first attempt failed 485/486 only on the already documented intermittent `CapInitializationTests.ThreeSeasonsPreserveImportedCapRolloverDraftAndCpuBudgets` lifecycle case; the retry passed without a source change.

Exact GitHub Actions APK artifacts:

- College `dynasty-manager-build-479`, artifact `13553371259`: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42321359769/artifacts/13553371259. APK `antdroid.cfbcoach.dev-Signed.apk`, 91,531,314 bytes, version code 879 / version name 0.20.479, SHA-256 `6BDF06060273BBBC7A8154B769DC102CA92AD8A98B0DC49EF55EC8E101F8A458`.
- Pro `pro-football-apk-204`, artifact `13553413817`: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42321359983/artifacts/13553413817. APK `antdroid.profootball.franchise-Signed.apk`, 90,251,785 bytes, version code 204 / version name 0.1.204, SHA-256 `AEA05419DD1B2789A0C20D1012044A58BAD896ABB1AF94CC0C75C28BA9CBC93A`.

Both downloaded APKs pass `apksigner verify` using APK Signature Scheme v2 and v3 with signer certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`. These are CI artifacts, not GitHub releases.

## GameCast pre-snap phase and team-colored end zones - November 6, 2026

Manual GameCast snaps now render the called offensive and defensive formation before any alignment action or snap animation. The previous implementation held the midfield huddle, so the delay and motion appeared in the wrong phase. Pre-snap motion now moves one eligible player within the on-field formation, returns that player to the settled alignment, and keeps the offense set for one full second before the recorded play animation starts. Shifts retain their shifted alignment at the snap. Simulation outcomes and saved `PreSnapChoice` records are unchanged.

Both field end zones now use the home team's primary color before drawing the home field/city wordmark. The prior drawing code reused the turf overlay fill, which made the end zones appear green.

Validation actually run:

- Focused `GameDayPreSnapPresentationTests|PreSnapTests` - 12 passed.
- Engine Fast - 382 passed; Engine Balance - 12 passed.
- Standard College suite with `DynastyHealthTests|OvrCalibrationDynastyTests` excluded - 629 passed.
- Standard Pro suite with `TwentySeasons|SourcedFiveSeasons` excluded - 486 passed.
- Packaged-resource validation and 24 Android UI synchronization helper tests passed.
- Football.Presentation and both Android app compile targets passed. College has no warnings; Pro retains its two documented `CS8625` warnings.
- College and Pro Debug Android APK packaging passed.

Exact local Debug APKs:

- College: `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk`, 91,527,218 bytes, SHA-256 `21B7EA517215F6D106CA71F3DE967751AC0FA7168FB568B0CF16D8A25CD50F4B`, version code 400 / version name 0.20.0.
- Pro: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, 90,247,689 bytes, SHA-256 `A180D279913AA43D713382CB575C997BC1034C1596D2A569ED79BC2962E070B5`, version code 1 / version name 0.1.0.

Android QA used the exact Pro APK on a read-only API 36 emulator and a fresh Seattle franchise. In Seattle's game at the Los Angeles Chargers, both end zones were exact Chargers primary `#007BC7`: 76.98% of each end-zone crop matched that color and only 0.20%/0.19% was green-like. Direct frames at the pre-snap hold show both 11-player units set in their formations with `Pre-snap formation` active. A field-only frame diff localizes the motion change to one marker region while the rest of the formation remains fixed. The logcat evidence has zero fatal exceptions, application ANRs, application process errors or outstanding GREF warnings.

Timing evidence is in `.artifacts/presnap-qa/`. The 50 ms frame was captured before adb finished dispatching the Call play tap and therefore still shows the huddle; subsequent 320 ms and later frames show the on-field formation and motion. A late `uiautomator` dump landed after the snap and is not used as hold-state evidence.

## Roadmap APK release — November 6, 2026

The near-term roadmap is complete for College and Pro. Durable motivation, scout replenishment, CPU coach
histories, person-level news memory, player-authored social replies, sourced pre-snap boundaries, rivalry
series, chemistry/locker-room state, replay highlights and coach Hall of Fame records are implemented with
backward-compatible save defaults. The one-yard shift and three-yard motion values remain internal catalog
bounds; rivalry records remain simulation-neutral.

Release validation actually passed:

- College476, run `42287497915`: 380 Engine Fast, 12 Engine Balance, 629 Core/College tests, Android packaging,
  live replay-highlight checks and career-options device checks.
- College action run `42290592355` on the exact College476 APK: real dynasty creation, phase/save/reload,
  player reply plus like/repost state, rivalry series, chemistry and motivation.
- Pro190, run `42287498698`: 486 standard Pro tests, NFLPA cap normalization, Android packaging and the full
  live GameCast/reload/coordinator device flow.
- Pro pre-snap run `42298345683` on the exact Pro190 APK: `Pre-snap formation` hold screenshot, selected motion,
  recorded `Motion created separation and added 2 yards.` effect and reset state.
- Pro roadmap run `42311170531` on the exact Pro190 APK: social actions and saved reply, rivalry series,
  chemistry/motivation, analytics/news/career/standings and visible portrait/landscape draft and retention
  actions. Scoped fixture regeneration passed 80 Pro cases.
- Local focused checks: 44 Engine, 10 College, 22 Pro and 24 UI synchronization tests. Both Android compile
  targets, packaged-resource validation, NFLPA normalization and `actionlint` 1.7.12 passed.
- Post-merge `main` workflows also passed: College build 478, run `42313407089`, repeated 380 Engine Fast,
  12 Engine Balance and 629 Core/College tests with successful packaging; Pro build 203, run `42313407092`,
  repeated 486 standard Pro tests with successful packaging and the two existing `CS8625` warnings.

Exact APKs:

- College476: `antdroid.cfbcoach.dev-Signed.apk`, 91,527,218 bytes, SHA-256
  `A64210CF82E1FBE5518E17ABA13FA54EED097F8D7C9F323C4EC2DE52B3D2BBF1`.
- Pro190: `antdroid.profootball.franchise-Signed.apk`, 90,247,689 bytes, SHA-256
  `259464436FE5357EF846119725C064E3B13CC6F8B85F17890CE7A2AE4A236124`.

Limits: the established release scope excludes the documented long College career/calibration cases and Pro
`TwentySeasons` / `SourcedFiveSeasons` cases. Earlier exact-APK retries failed or were cancelled on harness-only
predicate, timing and stale-label assumptions; Pro195 and Pro202 are the successful superseding checks. No
simulation claim is made beyond the bounded recorded pre-snap effect already covered by focused tests.

## Player replies and sourced pre-snap boundaries — November 6, 2026

Both remaining implementation gaps are now closed. The in-game social feed has a real player-authored reply
action in College and Pro. A reply is a persisted `FootballSocialInteractionKind.Reply` record with its body,
shown as "Your reply" beside the separately generated media-desk reply. Re-replying replaces the player's saved
text, empty text removes it, and older saves continue to load with empty interaction collections. The action
uses the shared themed prompt and stable `college-social-reply` / `pro-social-reply` automation IDs.

The pre-snap boundary now follows the sourced portions of NCAA Rule 7 and NFL Rule 7:

- One backfield player may be in motion at the snap; all other offensive players remain stationary.
- Motion must be parallel to or away from the opponent goal line / line of scrimmage, never toward it.
- Motion from the line of scrimmage requires the player first become a back and come to a complete stop.
- After the last shift, the offense must be set simultaneously for one full second. The shared live view now
  preserves a full one-second set after the shift/motion animation before snapping.

`PreSnapChoice` records direction, backfield motion count, stationary-player state, prior-shift/set state and
line-of-scrimmage start state. Older recorded choices default to one legal lateral motion player with the
required set complete. `FootballRules` exposes those sourced constraints. The existing one-yard shift and
three-yard motion values remain explicitly internal catalog and animation bounds: neither official rulebook
defines a numeric shift or motion yard limit. `RivalryRecord` remains history-only and has no play effect.

Sources reviewed and rendered:

- NCAA football rules: https://ncaaorg.s3.amazonaws.com/championships/sports/football/rules/PRMFB_RulesBook.pdf,
  especially Rules 2-22-1, 7-1-2 and 7-1-4.
- NFL 2026 rulebook: https://static.www.nfl.com/image/upload/fl_attachment/league/tqivdkzt9mu6wdgsh1ku.pdf,
  especially Rule 7, Sections 4, Articles 2 and 6-8.

Validation actually run:

- Engine `FootballSocialTests|PreSnapTests|NewsStoryTests` — 26 passed.
- College `CollegeSocialTests|CollegeChemistryTests` — 7 passed.
- Pro `ProSocialTests|FootballRulesConformanceTests` — 7 passed.
- College and Pro Android compile targets — passed with the existing offline `NU1900` warnings and two Pro
  `CS8625` warnings.

Not claimed here: action-level device acceptance, full repository suites, APK packaging, CI completion, merge,
prerelease publication, cleanup or branch deletion.

## Roadmap state/identity completion batch — November 6, 2026

This resumed batch closes four near-term state gaps across both games. Team motivation is now a persisted,
season-advanced record in College and Pro, with bounded drivers shown in both team surfaces. The Pro scouting
department repairs duplicate, missing and stale slots to exactly three unique scouts per club; a 512-seed
invariant check covers the generated franchise population.

CPU club coaching now has a real lifecycle record. Completed Pro seasons are recorded once per staff identity
and season, the coach Hall of Fame consumes those histories after employment ends, and per-team citations keep
the record attached to the clubs a coach led. Pro coach and retirement records carry an optional stable
`StaffId`; older saves deserialize with an empty default and existing user-career IDs remain compatible.
Same-name coaches on different clubs or roles no longer share an identity.

Person memory is now fed by Pro player-specific facts as well as College facts. Pro injury, player-of-game,
weekly passing leader, award, all-star and showcase stories carry `PersonKind`, `PersonId` and `PersonName`, so
the shared narrative service forms durable player threads across seasons just as it does for coaches. Refresh also backfills identity onto matching legacy saved stories instead of preserving an untagged duplicate.

One shared result bug surfaced during validation: a tied `ScheduledGame` previously named the away team in
`WinnerTeamName`. It now returns null for ties, keeping standings and coach conference-loss accounting correct.

Validation actually run on this working tree:

- `dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Debug --no-restore --filter 'FullyQualifiedName~ProCoachHallOfFameTests|FullyQualifiedName~ProNewsIdentityTests|FullyQualifiedName~HonorsTests|FullyQualifiedName~CareerSaveTests|FullyQualifiedName~StandingsTiebreakTests|FullyQualifiedName~StaffBalanceTests'` — 63 passed.
- `dotnet test tests/Football.Engine.Tests/Football.Engine.Tests.csproj -c Debug --no-restore --filter 'FullyQualifiedName~NewsStoryTests|FullyQualifiedName~TeamChemistrySeasonTests'` — 18 passed.
- `dotnet test tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj -c Debug --no-restore --filter 'FullyQualifiedName~BowlServiceTests|FullyQualifiedName~SchedulePersistenceTests|FullyQualifiedName~StartingLibraryTests'` — 24 passed.
- `dotnet build src/ProFootball.App/ProFootball.App.csproj -c Debug --no-restore -t:Compile` — passed with one offline `NU1900` warning and the two existing `CS8625` warnings.
- `dotnet build src/DynastyManager.App/DynastyManager.App.csproj -c Debug --no-restore -t:Compile` — passed with three offline `NU1900` warnings.

Not claimed here: final full suites, College/Pro APK packaging, device acceptance, CI completion, merge, release,
branch deletion or repository cleanup.

## Device harness races and the College roster native-handle wedge — November 6, 2026

**What the device jobs found.** Both GitHub Actions device jobs were still failing, for different reasons. The
Pro job's live-game step drove the shared GameCast view by labels the app no longer renders, and its landscape
check demanded quick-call cards in a watched game where the call sheet is deliberately closed until the coach
takes over. The College job wedged on the roster screen after a theme switch: the app stayed alive with no crash,
no ANR and no managed exception, while Mono logged "N outstanding GREFs. Performing a full GC!" roughly every
550 ms. The app was at its global-reference ceiling, so every new native view handle paid a full garbage
collection and the render crawled until the harness gave up.

**College native-handle leak.** `FoundationPage` subscribed a page-capturing handler to the long-lived
`Window.SizeChanged` event every time a page loaded. A theme rebuild replaces the page, but the window kept the
old page — and its entire view tree — alive, so every rebuild piled another set of native handles onto the app.
The handler is now stored, subscribed once, and detached before `RebuildThemeAsync` installs the rebuilt page.
On a local 4 GB / 8-core API 36 emulator the roster step after a Dark to Light switch now settles in 11.8 seconds
with the Morale column rendered, and the run logs zero GREF warnings and 9 explicit garbage collections instead
of 177 warnings and 457 explicit collections.

**Pro live-game harness.** The watch flow now asserts that a watched game does not ask the coach to call, then
takes over to earn the shared call sheet, picks an alternate quick call and snaps it. The landscape check
verifies that the field stays inside the viewport and that the quick calls fit, instead of assuming an older
collapse-after-selection behaviour. The play flow is rewritten against the current GameCast actions — Playbook,
`live-call`, and Options to Exit game to Discard and exit — and finishing now confirms "Finish game?" and
"Simulate remainder" before waiting for the final.

The play flow and the coordinator-career flow were rewritten the same way. Playing a game now waits for the
shared `live-call` sheet, opens the Playbook picker (asserting "Choose play" and "All formations"), snaps, and
leaves through Options to Exit game to "Discard and exit". A coordinator career now asserts that taking over
flips only its own unit ("Offense: You" with "Defense: CPU"), waits for that unit's call sheet instead of an
obsolete "Pause CPU unit" label, holds the game clock while the call is pending, and opens a Playbook that covers
only its own unit.

**Tap races.** The live toolbar relayouts while a play animates: button labels change (Pause becomes Resume) and
a transient Skip replay action can shift every button, so a tap taken from an older capture could land on its
neighbour and the expected menu would never open. `build/android-ui-smoke.py` gains `tap_expect`, which
re-captures before every tap and retries until the expected surface actually appears. The live menus in the Pro
harness use it.

Two harness-side behaviours stay as they were: the College save repository still refuses a mismatched schema
version, so no schema version was bumped, and the harness still refuses to dismiss an application ANR dialog.
A focused emulator walk of the changed Pro surfaces passes end to end on both halves: watch controls, take over,
alternate call selection, snap, box score, landscape quick calls inside the viewport, finished/saved/exited game,
then the played game's Playbook picker, snap, and exit confirmation.

**GitHub Actions.** Both device workflows pass on `7c6b377`. College build 475 (run `42189956817`) passes its
build job and both device jobs; the launch log reaches "System appearance refresh preserved the completed week
while processing was active", so the light/dark theme and roster step that previously wedged now completes.
Pro build 189 (run `42192520288`, `live` scope) passes the standard Pro suite and the full device harness, ending
at "Pro launch, franchise creation, watch/replay, landscape, saved final, reload, advance and coach mode passed",
which includes the rewritten coordinator career. The APKs are Actions artifacts `dynasty-manager-build-475` and
`pro-football-apk-189`. An earlier `full`-scope Pro dispatch (run `42189956967`) was cancelled at the job's
20-minute timeout because that scope also runs the long career tests; `live` is the scope that runs the device
harness inside the job budget.

Not claimed here: no release, merge or publication. The excluded long Pro and College career cases were not
rerun for this batch.

## Roadmap rewrite, harness cleanup, and APK handover — November 5, 2026

**Roadmap rewrite.** `docs/ROADMAP.md` was rewritten to match the state of the branch. It now opens with the
near-term status of all seven workstreams, each with what was delivered and what is still open, followed by an
ordered list of the six near-term items that remain: device acceptance of the new surfaces, coach lifecycle
parity for CPU club coaches, player and coach level narrative memory, motivation state, the two league rules
that need a sourced rule rather than an invented one. The long-term
section replaces a bare list with a first slice, a dependency note and a scope boundary for each of the six
long-term areas: the College NIL system, College media rights and finance, Pro finance expansion, expansion and
relocation, stadium construction and training facilities, and additional offseason activities, with an order that
puts NIL and Pro finance first because neither disturbs an existing ledger. `docs/IMPLEMENTATION-HANDOFF.md` now
points at that agenda, and `docs/roadmap-completion-plan.md` records the delivered batch commits.

**Harness cleanup.** The two device smoke scripts had drifted from the app over several UI rewrites and could not
reach the live game any more. The live-game sections of both were rewritten against the shared GameCast view:
the watch flow now opens the Options sheet, then Viewing options, then the camera choices; the team sheet is
checked during a live game; a play is picked from the call sheet and snapped; the box score is opened from the
live view; the landscape layout is checked against the `live-field` and `live-quick-call` automation ids; and the
game is finished, confirmed saved and exited. The College script's second-unit check now waits for the menu to
re-render instead of capturing it immediately, and the live view is located by its `live-game-page` automation id
rather than by a toolbar label.

Two things were deliberately not "fixed". The College save repository rejects a mismatched schema version, so no
schema version was bumped for any of the new collections. And the harness still refuses to dismiss the app's own
ANR dialog: an app hang is a finding, so the check fails and the cause gets fixed instead.

**Staff fixture.** `StaffBalanceTests` created a franchise with a random id, and the staff lifecycle is seeded
from that id, so the scout count could land on 95 instead of 96 and fail the run about once in twenty. The
fixture now uses a fixed franchise id, which removes the random dimension and keeps every assertion intact. The
underlying intermittency predates this work and is recorded in the November 1 entry.

**Repository hygiene.** The two dead scratch patch scripts from an earlier session were deleted after confirming
their anchors no longer exist in the source, the Codex attachment drop folders are now ignored instead of showing
as untracked noise, and the empty Codex worktree folder for a removed branch was cleared.
## Branch validation on GitHub Actions and smoke harness repairs — November 5, 2026

After the five batches landed, both existing workflows were dispatched against `codex/feature-roadmap-plan` to validate the branch end to end. The College build job runs the shared engine fast and balance tiers and the combined Core/College suite and packages a debug APK with zero warnings; the Pro build job runs the standard Pro suite and packages a debug APK with the two existing nullable warnings. Both build jobs pass on the branch, and the College career-options device job passes.

The device jobs on this branch had never run before, and the first attempt exposed a large amount of pre-existing drift between the smoke harnesses and the app. The app itself never crashed or hung: no fatal exception and no ANR appears in the captured logcat for any run, and both flows walk deep into the app. Every repair was made in the harness and never in the app, and each was diagnosed against the app source and confirmed against `main` where it mattered. A control run of the Pro device job on `main` (run `42122342674`) failed at exactly the same assertion as the branch, which shows the failure predates the batches.

Fifteen harness commits repair what the scripts still asserted after the app moved on:

- The College save library has always shown its own "Settings" button. Commit `09e6e19` on `main` renamed it from "Options" to "Settings", and the College launch check treated any "Settings" text as a leaked career control, so it failed on the library itself. The check now ignores the library's own button by its automation id.
- The Pro weekly coach talk button moved to the Staff page in `bc0d37f` on `main`, but the Pro script still looked for it on the Overview page.
- The player profile is a tabbed modal page, so its title is not rendered; the OVR header and the attribute groups live on the Attributes tab while the Edit appearance action lives on the Overview tab.
- The College shell shows the period inside the header instead of a separate phase label, and the live game clock is now part of the combined situation line.
- The live field is exposed as "live-field" by the shared GameCast page, the live options sheet is titled "Game options", and the camera choices live in their own sheet behind "Viewing options".
- The staff rows use the shared staff directory component, whose accessibility description is "View staff profile", and the coach attribute group is titled "Coaching attributes".
- The College Settings page can open scrolled past its "Save & exit" button, the Pro free agent position list uses the depth chart names and needs a scroll to reach HB, and the Pro cap room reads "season cap space" rather than "available".

The harness repairs carry focused unit tests in `build/test_android_ui_smoke.py`, which pass.

One thing remains outside the batches. The live-game sections of both scripts still expect labels the app no longer renders — "Choose offense", "Next snap", "Game controls", "Pause CPU unit", "Pause watch", "Leave game?" and "Return to franchise" — because the live view was rewritten in the FM touch-style refactor and again in the November 5 live-view batch. Rewriting those sections against the current GameCast actions (Playbook, Call play, Pre-snap, Take over, Options, Skip replay) is a separate piece of work; the College flow now reaches the live view and the Pro flow reaches the contract negotiation, so the remaining gap is the live-game step and what follows it. Separately, `ProFootball.Tests.StaffBalanceTests.EveryFranchiseHasCompleteVariedStaffAndARealSchemeIdentity` now receives the fixed franchise ID before staff generation. `ProLeagueService.Create` accepts that ID directly, so staff identities and lifecycle rolls use the intended seed instead of a fresh random ID; `StaffBalanceTests` passes 26/26 and the complete Pro suite passes 486/486.

Not claimed: no release, no merge, and no artifact publication. The CI runs are build and device evidence, not a release.
## Persisted rivalries and rivalry series in both games — November 5, 2026

Branch `codex/feature-roadmap-plan`: rivalries are now a durable record instead of a label recomputed from the schedule.

Both games persist the rivalry set. College records it on its offseason rollover and Pro when the offseason hands over to preseason, and each save keeps the series with the games behind it. The shared `RivalryRecordService` reads that set, so both games label the same matchup the same way and describe a series the same way, with the number of meetings and the record from one team's point of view.

The exposure is wider now. The College team profile and the Pro team page list each program's rivalry series with its opponent and the recorded record. The College game center and the Pro matchup card name the series for the game in front of you, and the schedule series column reads the persisted set rather than re-deriving it. The same records feed the rivalry threads in the social feed.

Save compatibility: `DynastyState` and `ProFranchise` gained a rivalry collection with an empty default and no schema-version bump, so older saves load with empty fields. As with the earlier batches, nothing about the simulation changes: the first increment carries no rivalry effect on a play.
## Social interaction, replies, and rivalry threads — November 5, 2026

Branch `codex/feature-roadmap-plan`: the in-game social network now records what the player actually does with it.

Player activity: a post carries the player's own likes and reposts, and the player can follow a media desk. These are persisted per save, so the feed keeps them across sessions, and they stay apart from the decorative engagement counts, which remain derived from the story ids as before. Following a desk moves its posts to the top of the feed.

Replies: each post gets an authored reply from a different media desk. A reply restates the recorded category behind the post — "Injury report noted." for an injuries story, "Recruiting board updated." for a recruiting story — and never invents a result, a quote or a statistic. A reply has its own identity, so its read state stays separate from the source story.

Rivalry threads: the narrative memory now records a thread per recorded rivalry series, built from the head-to-head results the schedule already holds. The thread states who met, how often and across how many seasons. Both games feed their recorded games into it, so the social feed can carry a rivalry thread and not only team, coaching, health and recruiting threads.

Both games persist the player's activity in their existing saves with empty defaults and no schema-version bump, so older saves load with empty fields.
## Persistent coach hall of fame in both games — November 5, 2026

Branch `codex/feature-roadmap-plan`: the shared coach hall of fame is now rendered, recorded and persistent. Until now `CoachHallOfFameService` had no production caller, so the roadmap's claim that it appeared in both hosts' views was wrong; the College and Pro Hall of Fame screens showed players only.

The shared core scores a recorded coaching career the same way for both games: at least five head coaching seasons, a .550 win percentage and some recorded honor. It now also produces one citation line per team the coach led, with the seasons and the record at each stop, so a coach who moved schools is attributed to every school rather than only the last one.

Eligibility stays a league decision. A College coach becomes eligible once the program no longer lists them on staff, or once a retirement record exists; a Pro coach becomes eligible once the coaching job is over. College draws the career from the recorded coach season history, Pro from the recorded coaching career plus the league's own playoff and championship results.

Both hosts persist the induction. `DynastyState` and `ProFranchise` gained a coach hall of fame collection with an empty default and no schema-version bump, so older saves load with empty fields and are not rewritten. An induction is recorded once: the coach keeps the year they were first inducted, and a later season refresh does not move it.

Both surfaces show the new section. The College Hall of Fame screen lists the inducted coaches with their season counts, records and per-team citations, and the Pro Hall of Fame tab lists the same for the recorded Pro coaching career.

The excluded long College career cases were run on this branch with and without this change, case by case, and the outcome is identical. Five of the ten fail on both sides, and they fail on the same cases:

| Case | Baseline | With this change |
|---|---|---|
| 12 teams, 5 years, seeds 1 / 2 / 3 | fail, pass, pass | fail, pass, pass |
| 130 teams, 10 years, seeds 4 / 5 / 6 | pass, pass, pass | pass, pass, pass |
| 12 teams, 20 years, seeds 7 / 8 | fail, fail | fail, fail |
| 20-year FBS calibration, seeds 1 / 17 | fail, fail | fail, fail |

The failures are pre-existing on `codex/feature-roadmap-plan` and are not caused by this work.
## Replay highlights, pre-snap outcome effect, and league pre-snap rules — November 5, 2026

Branch `codex/feature-roadmap-plan`: the GameCast live view can now replay the recorded moments worth replaying, and a pre-snap alignment action finally changes the play it precedes.

Replay highlights: the shared engine gains `ReplayHighlightService`, which reads a game's recorded snaps and returns the moments worth replaying — scores, turnovers, fourth-down conversions and big plays. Every candidate carries the recorded snap it came from, so a replay never reruns the simulation. The live game's Options menu gains a "Replay highlights" entry that lists the candidates with their quarter and clock and replays the chosen one.

Historical pre-snap outcome note (superseded by the coverage-aware model above): the first implementation used a small fixed yard effect and left the defensive read untouched. Current behavior resolves man/zone/focused coverage, changes the read matchup and may add or remove up to two recorded yards.

League pre-snap legality: `FootballRules` now carries `MaximumShiftYards` and `MaximumMotionYards`, and `PreSnapCatalog.IsValid` validates against the session's rules instead of a fixed range. Both leagues keep the current limits because the repository holds no sourced rule for a different one; the hook is what a future league rule will use.

Pre-snap choreography: a motion now animates out and returns to the formation before the snap, which is what a motion is, while a shift keeps the shifted alignment at the snap.
## Season-aware team chemistry and locker-room events — November 5, 2026

Branch `codex/feature-roadmap-plan`: team culture is now recorded per season and carried forward. The shared engine gains `TeamChemistrySeasonService` (turns the previous season's record into the next season's baseline), `TeamChemistryInputs` (the season facts a host derives from its own records), and `LockerRoomEventService` (writes locker-room notes from recorded facts only). `TeamChemistryRecord` now stores the team name, the live snapshot the baseline was built from, the continuity fields (head coach, plan, captains, turnover) and notes that explain every adjustment.

Cohesion moves from a single snapshot to a season baseline. Roster turnover, a head-coach change, a changed game plan, departing captains, discipline incidents and morale all adjust it inside the existing 20-95 band, and each adjustment is explained in the record's notes. The season offset is applied on top of the live evaluation, so in-season form still moves the value while the season's continuity carries through. `GameDayEngine`'s `GameShift` bound is unchanged.

Both hosts persist the result. College writes `TeamChemistry` and `LockerRoomEvents` on its offseason rollover (`SeasonProgression.Advance`); Pro writes them when the offseason hands over to preseason (`ProOffseason.Advance`). College feeds recorded departures, transfer-portal exits, discipline incidents and season morale; Pro feeds drafted and traded-in players, contract friction and trade requests. League rules stay separate: College uses discipline incidents, Pro uses contract and trade friction.

Both team surfaces show the new state. The College team profile and the Pro team page show the season delta, the momentum, the current captains, the adjustment notes and the recent locker-room notes. Locker-room notes are derived only from recorded facts and never invent quotes, results, injuries or statistics.

Save compatibility: `DynastyState` and `ProFranchise` gained two collections with empty defaults and no schema-version bump, so older saves load with empty fields and are not rewritten. The College save repository refuses a save whose schema version differs from the current one, so the version was deliberately left alone.
The excluded long College career cases were run on this branch with and without the change, case by case, and the outcome is identical. Five of the ten fail on both sides, and they fail on the same cases:

| Case | Baseline | With this change |
|---|---|---|
| 12 teams, 5 years, seeds 1 / 2 / 3 | fail, pass, pass | fail, pass, pass |
| 130 teams, 10 years, seeds 4 / 5 / 6 | pass, pass, pass | pass, pass, pass |
| 12 teams, 20 years, seeds 7 / 8 | fail, fail | fail, fail |
| 20-year FBS calibration, seeds 1 / 17 | fail, fail | fail, fail |

The failures are pre-existing on `codex/feature-roadmap-plan` and are not caused by this work. The 12-team 5-year failure is the "Assistance left too much of the roster dependent on walk-ons" assertion. The 12-team subset also completed in the same 27 seconds with and without the change, so the season refresh adds no measurable cost.## Roadmap completion plan and stale worktree cleanup — November 5, 2026

Branch `codex/feature-roadmap-plan` carries `docs/roadmap-completion-plan.md`, an execution outline for the remaining near-term workstreams. It records the verified branch baseline, the five batches with acceptance criteria and validation commands, and two source-audit corrections to `docs/ROADMAP.md`.

Source-audit correction: the roadmap stated that the shared coach Hall of Fame service "is rendered in College center and Pro honors". It is not. `CoachHallOfFameService.Build` has no production caller; the only callers are `tests/Football.Engine.Tests/CoachHallOfFameTests.cs`. College renders `state.HallOfFame` and Pro renders `ProHallOfFameService.Build`, both player-only. `ProHallOfFameService.CoachSummaries` produces the coach summaries the shared service is meant to score, and nothing calls it. The audit also found that `TeamChemistryRecord`, `RivalryRecord`, and `CoachHallOfFameEntry` have no collection in `DynastyState` or `ProFranchise`, so all three features are recomputed on load and none can carry season history. The roadmap checkpoint table and `docs/IMPLEMENTATION-HANDOFF.md` now state this accurately.

Stale worktree cleanup: the three registered worktrees for `codex/parity-roadmap-audit` (`6274429`, November 2), `codex/pro-scouting-depth` (`0b00b47`, November 1), and `codex/roster-identity-and-priority` (`81b7b28`, November 4) were each verified as ancestors of `main` — `git log origin/main..<branch>` is empty for every one of them and the merge-base equals the branch tip — so nothing required merging. Their local branches were deleted with `git branch -d` and the worktrees were removed, reclaiming roughly 8.8 GB of build output. No remote change was needed: all three branches were already absent from GitHub, and no pull request had been opened from any of them.

Two pieces of uncommitted content were preserved first, both under the gitignored `.artifacts/stale-worktrees/`: an uncommitted `pro-android-branch.yml` edit from the coach-fix worktree, saved as `pro-android-branch-local.patch`, and six unique device screenshots from the pro-scouting-depth worktree, verified byte-identical by SHA-256 after copying. The workflow patch was deliberately not merged. It re-adds a push trigger for the deleted `codex/pro-scouting-depth` branch plus a staff balance-audit step; `main` deliberately keeps that workflow as a minimal APK-only builder, and the audit tool and its recorded results already exist. It stays available if the balance-audit step is wanted on `pro-android.yml` instead.

`git fsck --no-progress --no-dangling` reported no errors, and HEAD stayed at `8438f41` through the cleanup. Checks run: branch and worktree inventory, ancestor and merge-base verification per branch, working-tree status inside each worktree, SHA-256 comparison of the preserved attachments, `git fsck`, `git ls-remote --heads origin`, `gh pr list --head` for each branch, and `git rev-list --left-right --count` against both remote branches. No tests, builds, APK packaging, device runs, CI dispatch, PR, merge, or release were performed for this documentation and housekeeping change.

## Visible pre-snap formation hold and motion-preset plays — November 5, 2026

Branch `codex/feature-roadmap-plan`: manual offensive snaps now pause for approximately 900 ms after Call play while the called formation is displayed on the field. The status and recap identify the interval as Pre-snap formation. Shift and motion choices animate their alignment preview during the hold, and the recorded snap begins from the adjusted presentation alignment without changing simulation randomness.

Pre-snap motion was previously a modifier rather than a playbook type. It remains a play-level modifier, but every offensive formation family now exposes a first-class preset call: Jet Motion Zone, Orbit Motion Levels, Glide Motion Power, Orbit Motion PA Boot, Motion Crossers and Slot Motion Triple. Their cards are labeled MOTION · RUN/PASS and selecting one applies its pre-snap setting automatically. Normal calls remain selectable and users can still override the standalone Pre-snap control.

Validation: `PreSnapTests` 5 passed, including validation, deterministic unchanged outcomes, legal-situation filtering and playbook motion-preset availability. Football.Presentation, College and Pro Android compile targets passed. The final Pro debug APK packaged successfully. On-device acceptance on the exact final APK showed the Pre-snap formation status and called formation during the hold, Orbit Motion Levels in the full playbook, automatic Pre-snap: Motion right 2 yd selection, and an empty crash buffer after the snap. Timing captured on the exact APK was approximately 1.15 seconds including screenshot capture time for the pre-snap frame and 2.52 seconds for the live animation frame; the status changed from Pre-snap formation to Play in progress at the snap boundary.

Final local Pro debug artifact: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, 90,120,713 bytes, SHA-256 `60D942BC30670A45B2856D01C67A7BC798A8C7A7DF765E959081A334D65E99B7`. This exact artifact was installed for the timing and motion acceptance checks. Exact-APK timing screenshots: `.artifacts/exact-transition-hold-final.png` and `.artifacts/exact-transition-live-final.png`; motion playbook evidence: `.artifacts/playbook-motion-scrolled.xml`. Existing NU1900 warnings and two Pro CS8625 warnings remain. No CI dispatch, PR, merge or release was performed.

## Live field, compact News, Social narratives, and phase ordering — November 5, 2026

Branch `codex/feature-roadmap-plan`: fixed the shared GameCast field by removing the full-width home-color overlay that crossed the top of the playing surface. Revised both News hosts to remove the standalone Save story rows and Saved filter while preserving saved-story data. Added a visible Social shortcut, richer authored posts with engagement labels, deterministic narrative memory in both save models, factual multi-story season threads, and phase-aware Pro News ordering so preseason stories no longer outrank regular-season news.

Validation: Engine `NewsStoryTests` 8 passed, including narrative-memory idempotency, older-save defaults, derived posts, source read state and stored snapshots. Pro `NewsOrdersPlayoffsAndAwardsAfterRegularSeason` 1 passed with preseason/regular/playoff/award ordering. Pro `NewsRefreshIsIdempotentAndPreservesReadState` 1 passed. College `NewsAndTableOrderingTests` 7 passed. Presentation compile and both Android app compile targets passed; Android packaging passed with known NU1900 warnings and two existing Pro CS8625 warnings; xUnit analyzer warnings remain pre-existing.

Exact local debug artifacts from this source working tree:

- Pro: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, 90,116,617 bytes, SHA-256 `41C9596D6FB024790D0E55070BCAD452930AF937C44F79876FF25480E68847E7`.
- College: `src/DynastyManager.App/bin/Debug/net10.0-android/antdroid.cfbcoach.dev-Signed.apk`, 91,090,072 bytes, SHA-256 `CCAA91AE0C297F8A92082F3CBC94FF7A5742B1C1701FE1B357D568916E983EFB`.

Device acceptance on the exact APKs: Pro field checked in portrait and landscape with no top color band; Pro Overview/News checked after simulated preseason for regular-before-preseason ordering, compact cards and populated Social; College News and Social checked for the visible shortcut and compact cards. The first College Social build exposed a reused-picker parent crash; it was fixed by creating the picker per render, rebuilt, reinstalled and retested with an empty crash buffer. Screenshots: `.artifacts/pro-live-field-fixed.png`, `.artifacts/pro-live-field-landscape-fixed.png`, `.artifacts/pro-social-network-feed.png`, `.artifacts/college-social-network-fixed.png`.

No CI dispatch, PR, merge or release was performed. Season-aware chemistry/locker-room events, social replies/follows, replay highlights/choreography and HOF lifecycle work remain next.

## User visual review and roadmap checkpoint — November 5, 2026

Documentation-only review on `codex/feature-roadmap-plan`, starting at `9a28a7f`. Recorded the user's Pro landscape screenshot (`534722.jpg`): a home-team color band crosses the top of the playing field. Source inspection found the matching full-width, 12%-height `HomeColor` fill in shared `GameFieldDrawable.Draw`; both games consume this renderer. This is an open priority defect, not a fix or device reproduction.

Recorded dissatisfaction with the News layout and its separate Save story row, low perceived value of bookmarks, and expectation of a social media element. Both hosts currently expose Social in News filters; posts only derive labeled in-game reactions from recorded headlines. Narrative memory and richer interactions remain open. The updated `ROADMAP.md` records the compact-feed direction, bookmark deprioritization, social discoverability gap, and a seven-workstream near-term checkpoint. Earlier saved-story implementation/evidence below remains historical; it does not override this feedback.

Checks this review: inspected branch/status/remotes/history, current status/testing docs, shared field renderer, both News host layouts, social model/service/tests, Presentation project configuration and all three existing Actions workflows. Documentation diff/whitespace validation only; no application edits, tests, builds, APKs, device runs or CI dispatch. Existing untracked files/worktrees are preserved.

## Feature branch APK verification — November 5, 2026

Source `9f7af9c` on `codex/feature-roadmap-plan` is verified on GitHub. Existing Actions workflows manually dispatched against that exact branch both succeeded:

- College455: run [41991820982](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41991820982), full management scope (extended career/calibration disabled). Engine Fast343, Balance12 and Core/College614 passed. Debug APK packaging: zero warnings/errors. [Exact APK artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41991820982/artifacts/13398433679).
- Pro166: run [41991825930](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41991825930), fast validation scope. Pro458 passed. Debug APK packaging: two existing nullable warnings, zero errors. [Exact APK artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41991825930/artifacts/13398389671).

Both artifact archives downloaded; exact signed APK filenames, sizes and SHA-256 values are recorded in `docs/audits/roadmap-ci-2026-11-05.json`. Local files: `.artifacts/roadmap-ci/college455/antdroid.cfbcoach.dev-Signed.apk` and `.artifacts/roadmap-ci/pro166/antdroid.profootball.franchise-Signed.apk`. Artifacts are CI downloads, not a release. No device jobs, merge, PR or publication performed. Documentation-only follow-up commits do not change these source identities.

## Persistent saved news — November 5, 2026

Both games now offer Save/Remove actions and a Saved news filter. Full shared story snapshots (including in-game media) persist in each existing league save, so retained articles survive feed pruning and season changes. New SavedNews collections default empty for older saves. Pro news refresh now preserves social read IDs while their source remains in the feed, and read IDs of saved snapshots after the source disappears.

Executed checks (Debug, --no-restore): Engine NewsStoryTests 6 passed, including snapshot JSON round-trip and selective bookmark removal; Pro CareerSaveTests.NewsRefreshIsIdempotentAndPreservesReadState 1 passed, including social read retention and Pro snapshot JSON round-trip. An initial Pro filter matched no tests and is superseded by the exact-name run. Both app -t:Compile targets passed with existing NU1900 warnings and Pro CS8625 warnings; existing xUnit analyzer warnings remain. No APK packaging, device walkthrough, CI dispatch, merge or release. Persistent narrative arcs, richer social interactions, and device UI acceptance remain open.

Reproduce: `dotnet test tests/Football.Engine.Tests/Football.Engine.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~NewsStoryTests`; `dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Debug --no-restore --filter FullyQualifiedName~NewsRefreshIsIdempotentAndPreservesReadState`; `dotnet build src/DynastyManager.App/DynastyManager.App.csproj -c Debug --no-restore -t:Compile`; same build command for `src/ProFootball.App/ProFootball.App.csproj`.

## Shared venue styling and social-feed wiring — November 5, 2026

Branch `codex/feature-roadmap-plan`: both College and Pro now consume a shared venue style in GameCast. Indoor turf stays green with a distinct mowing palette and readable markings; the host callback is connected to the renderer. Both news screens expose explicitly labeled in-game Social views derived from recorded headlines. Social posts have independent persistent read IDs; opening a post does not mark its source story read. Fixed College Unread selection resetting to All, refreshed headlines after closing a story, made Pro unread borders visible, and removed internal record identifiers from card previews.

Validation: six focused Engine news/replay tests passed, including independent social/source read state and no reactions-to-reactions. Shared Presentation build and both Android app Compile targets passed. Initial sandboxed app Compile calls failed silently; rerunning outside the sandbox succeeded. A sandboxed testhost failed its connection timeout and aborted; the outside-sandbox run passed. Existing offline NU1900 warnings remain; the initial Pro compile also reported the two existing CS8625 warnings. No APK packaging, emulator visual acceptance, CI dispatch, merge, PR, or release was performed.

Next: persistent narrative memory and saved-story views, season-aware chemistry/locker-room events, replay highlights and pre-snap choreography, then persistent HOF lifecycle handling. These remain open; this checkpoint does not complete the near-term roadmap.

## Published Pro165 / College454 — roster identity, CPU plan drift, Flexbone academies — November 4, 2026

Published November 4: [Pro165 / College454 prerelease](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-04-roster-identity), tag `preview-2026-11-04-roster-identity` (release record `8a62a03`, implementation `9c9ff9c`). Both APKs come from the successful main builds (Pro run 41798956069 with the full validation scope; College run 41798956072 with the full management scope); SHA-256 values and the small build manifest are attached to the release. Batch scope: the shared roster identity model with acquisition scoring, a completed three-season long-term audit, staff-adapted CPU game-plan drift for Pro and College, the Flexbone service academies, and the playbook/scheme coverage audit. No device-played flows are claimed; evidence lives in docs/roster-identity-long-term-audit.md, docs/playbook-scheme-coverage-audit.md, docs/current-work.md history and the two CI runs above.

## Roster identity long-term audit and CPU plan drift — November 3, 2026

Branch `codex/roster-identity-and-priority`. Completed the two roster-identity plan next-work items in order.

1. Long-term roster audit: three-season real/generated audits for Pro and College (8,832 games) through the
   `tools/FootballBalanceAudit` season modes with identity evidence (roster-target gaps, plan distributions,
   philosophy counts, drift counts, user-plan stability). Zero personnel violations, identity roster-target gaps
   stable across seasons, and balance within noise in every season. Baseline with drift hooks disabled:
   `docs/data/roster-identity-2026-11-03/`; report: docs/roster-identity-long-term-audit.md.
2. Staff-adapted CPU game-plan drift: Pro CPU plans now drift one adjacent playbook step per offseason toward the
   established head coach's mapped scheme-label philosophy during retention (`ProGamePlanDrift`), and college CPU
   plans drift toward the head coach's numeric offensive lean during the annual staff advance
   (`RosterPlanService.ApplyCpuPlanDrift`). User plans, custom playbooks and weak-influence coaches never drift;
   the shared gate is `RosterPlanService.MinimumDriftInfluence` (.10, roughly two seasons of tenure with average
   leadership). Dedicated tests pass (5 College + 3 Pro drift cases; Fast Pro 33, College 7, Engine 319) and the
   drift-enabled re-audit (`docs/data/roster-identity-drift-2026-11-03/`) shows bounded drift (College real 49 and
   generated 31 of 132-138 CPU programs, Pro 2-3) with stable scoring and unchanged user plans.

Playbook/scheme coverage verification: audited the 8 offensive playbooks, 5 defensive playbooks,
   80 offensive / 30 defensive named plays, 32 pro and 138 college catalog labels against the real
   football scheme families; report: docs/playbook-scheme-coverage-audit.md. Modern families are covered;
   gaps are pre-2000 HS-scope schemes (Wing-T/double-wing/wildcat) plus retained judgment-call mappings
   (Veer and Shoot to Option, hybrid fronts to nearest base). The dedicated Flexbone playbook now backs
   Air Force, Army and Navy, so the triple-option schools run true flexbone formations.

Flexbone academies: Air Force, Army and Navy now run the dedicated Flexbone playbook with
   flexbone formations (previously the generic Option playbook), matching real triple-option football;
   source labels stay "Option" with defensive identities unchanged.
Performance follow-up: `PortalProcessingTests.FullLeagueHighSchoolMarketUsesActualCompetingOffers` first exceeded
its 30s guard because the identity work added full roster-target calculations inside the competing-recruiting
hot loops. Identity and roster-target memoization restored the market path and the final main CI passes. The
November 3 audit checkpoint itself made no release claim; the November 4 release above supersedes its status.


## Pre-snap call sheet wiring — November 4, 2026

Branch `codex/feature-roadmap-plan` records the first shared pre-snap catalog wiring. The shared live call sheet offers None, left/right shifts, and left/right motion. The choice is stamped on the resulting scrimmage snap and appears in live/replay presentation. Kickoff, try, kneel, spike, punt/field-goal and invalid choices are rejected; the call remains presentation-only and does not consume the simulation random stream.

Focused passing: Engine PreSnap 4; segmented GameDay classes passed 124 cases (Clock/YAC 20, Completion 4, Engine 27, LivePolish 27, Recommendation 11, Situational 12, LiveControl/PreSnap combined 23). Other segmented Engine Fast groups passed 205 cases: goal-line/kick/penalty/phase 48, safety/live-ball 65, stats/position/query/core 20, rules boundary 36, replay/toss/adapters/kickoff 27, rivalry/leadership/chemistry 9. College GameDay-filtered Core cases passed 56; Pro rules/foundation cases passed 17. Football.Engine and Football.Presentation build with 0 errors; the Presentation project has one local offline NU1900 warning. Android app builds did not complete because local restore returned failure under the offline/restricted package-vulnerability path, so College/Pro packaging is not claimed.

A single `Category=Fast` Engine tier run was not successfully completed locally. One 90-second run aborted at testhost connection, and retries stayed quiet/hung. Segmented Owner Engine Fast filters above cover all Fast test classes, but are not claimed as one uninterrupted tier run. No Android package/device acceptance, College/Pro release, merge, or simulation-effect claim is made. Remaining next steps are pre-snap play-outcome adaptation, league-specific legality, marker choreography, mobile device verification, and the remaining roadmap items.

## Shared coach Hall of Fame — November 4, 2026

Branch `codex/feature-roadmap-plan` adds a league-neutral `CoachHallOfFameService` over recorded coach-season summaries. College uses existing coach state history; Pro maps the user's saved coaching history into the same shape. Both render a coach Hall of Fame view alongside their existing player HOF, with no new save schema and no changes to player HOF eligibility.

Executed: Coach HOF Engine tests 2 passed; Pro Honors/Pro HOF tests 4 passed; College Hall of Fame test 1 passed. Football.Engine, Football.Presentation, DynastyManager.App and ProFootball.App Android builds passed with 0 code errors; Presentation may show the known local offline NU1900 package-vulnerability warning. No APK, device walkthrough, release or merge was run.

Remaining: persistent coach HOF records, retirement-specific eligibility, richer citation/team handling, and further coach lifecycle parity.

## Shared captaincy and chemistry wiring — November 4, 2026

Branch `codex/feature-roadmap-plan` adds shared `TeamChemistryService.Evaluate`, deterministic captains from the existing leadership helper, and bounded cohesion/scheme-familiarity modifiers inside `GameDayEngine`. The College and Pro profile builders fill these values from actual roster/staff state. College team profiles and Pro team pages now show the values, making the captaincy/chemistry path reviewable.

Executed: `TeamChemistryTests` 3 passed; combined GameDay engine/control/polish/situational/completion suite 124 passed. Both Android app builds passed with 0 errors. No APK/device acceptance is claimed. This is a bounded first simulation effect; per-season memory, roster turnover, and explicit discipline/plan-continuity inputs remain open.

## Pro indoor venue awareness — November 4, 2026

Branch `codex/feature-roadmap-plan` identifies a fixed list of enclosed Pro homes as indoor. Those games now skip wind/precipitation and use the neutral indoor weather rules. Added focused College tests for Dallas/Atlanta/Detroit and the GameDay weather suite passes 19 cases. No release or merge was run. Broader stadium/turf profiles remain open.

## College News persistent read state — November 4, 2026

Branch `codex/feature-roadmap-plan` adds `ReadNewsIds` to the College dynasty save and a shared `DynastyNewsService.MarkRead` helper. The College news page now supports All/My-team/Unread filters and marks stories read when opened. Focused `NewsReadStateTests` pass 2 cases and the College Android app build passes with 0 errors. No APK/device walkthrough or release is claimed. Social-post generation, shared Pro/College adapter and narrative memory remain open.

## Historical notes below

Release/status wording in the entries below is retained as evidence. Where it says a batch was unmerged,
unreleased or pending, the November 4 published summary above and the release record supersede it.

## Active roster/rules follow-up

Published November 2: [Pro162 / College450 prerelease](https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-11-02-parity-rules), tag `preview-2026-11-02-parity-rules`, target `d888557`. Both APK hashes match the successful main-build manifest; full changelog and checksums attached.

CPU safeguards, supported additional league rules and shared-test migration are complete and merged at `cb16217`. Main Pro162/College450 build successfully; exact checks, artifacts and limits are tracked in [ROSTER-RULES-VALIDATION-HANDOFF.md](ROSTER-RULES-VALIDATION-HANDOFF.md). The user authorized merge after validation. Earlier APK acceptance and historical pending notes below keep their original scope. The preserved root changes were later verified and removed during the November 4 sync.

Final candidates Pro160 / College448, runtime `d3ed91a`, are on `codex/parity-roadmap-audit`. CI passes27 Pro,30 Engine and107 College/shared cases; both APKs package (Pro retains2 existing nullable warnings). The768-game mode comparison, specialist cohorts, annual staff characterization and exact-APK native evidence have separate scopes; overlapping counts are not added together. See [the completion report](audits/parity-completion-2026-11-02.md), [exact manifest](audits/parity-acceptance-current.json) and [full draft changelog](releases/preview-2026-11-02-parity.md).

Historical pending notes below are superseded by the current handoff/report, not deleted evidence. Explicit officiating extensions and the intermittent three-season CPU cap/roster finding remain documented. No main merge or prerelease was published in this branch batch. Remote artifact cleanup reclaimed1,025,437,709 bytes; local cleanup and the unrelated workflow edit remain deferred/preserved.

## Active parity continuation — November 2, 2026

Feature branch runtime5239013 adds dynamic Pro kickoffs, shared phase/task and staff-link contracts, competition-aware statistical queries and canonical specialist adapters. The768-game real/generated CPU/controlled comparison passes; focused timeout/penalty/safety checks pass71 cases. CPU kneels now retain CPU call provenance. See [current handoff](IMPLEMENTATION-HANDOFF.md) and [mode report](audits/mode-conformance-results.md).

A device-discovered College opponent portrait permission gap is fixed at UI/save boundaries in2f1eae5; College-only compile correction1b118a2 preserves player controls. Successful Actions APKs: Pro152 (413 cases) and College442 (Engine30 + scoped College7). Exact hashes, failed/superseded build accounting and the ongoing native acceptance matrix are recorded in the handoff. No final all-scope acceptance, merge, release or local cleanup is claimed yet. The unrelated branch workflow edit remains preserved.

## Safety/query continuation — November 2, runtime0a4b430

Safety36fc92c fixes ordinary run/sack/kneel safety scoring, free kicks from20, scoring/drive/stat reconciliation and period/overtime boundaries. Shared scopes and staff bounds53e318a preserve league phase/history and lifecycle policies. Presentation0a4b430 preserves punt-touchback20 labels. Initial safety reproductions failed before the fix; final25 safety cases and the scoped shared/Pro/query/lifecycle checks pass. Both Android hosts compile. Exact local APK details and device limits: `audits/safety-query-checkpoint.json`; handoff has the remaining authorized scope. Earlier palette/editor device acceptance remains tied to its old APK hashes. No merge/prerelease/cleanup or new safety-specific device acceptance is claimed; unrelated workflow edit preserved.

## Parity continuation — November 2, 2026

Work is on `codex/parity-roadmap-audit` in `.codex-coach-fix`; root main contains unrelated dirty work. Runtime0138a4a fixes the reproducible Pro custom-play modal busy lock and raw formation picker labels. Both local Debug APKs packaged; exact-APK checks and remaining gaps are in `IMPLEMENTATION-HANDOFF.md` and `audits/parity-continuation-checkpoint.json`. College light-theme identity/staff navigation and Pro custom library save/reload/user-coach portrait restart are checked in an isolated read-only emulator clone. Full device acceptance is not claimed.

54 focused cases pass across modes/rules, multi-year specialists, Pro/College staff characterization and shared test ownership. PositionTests moved to Engine; incremental Fast/Balance/Career traits introduced. `audits/test-ownership-and-refactoring.md` records completed inventory and pending extraction boundaries. Safety/free kicks are a confirmed missing engine path, not an accepted rules scenario. Remaining scope is explicitly retained in the handoff; older design backlog excluded. No merge, prerelease, new CI run or local cleanup is claimed. Unrelated workflow edit preserved.

## Accepted Pro115 / College412 — guided offseason, compact overview and backfield corrections

User scope: Pro roster FB order/identity audit, compact College-style overview with weekly tasks, automatic dedicated offseason screens, dense post-draft training camp, draft board/results/order/prospect details, yearly offer/team-room projections, manual HC offseason over-budget extensions with regular-season compliance preserved, contrasting FA actions/visible rival bids and optional non-destructive GM bidding help. Both apps: Settings naming, confirmed save/exit moved into Settings, default FB order beside HB, offseason card near overview top. Save enum IDs and signed contracts are preserved; old Pro RosterCuts value3 is presented as Training Camp, directly after Draft.

Reviewed198 snapshot HB/FB identities with offline source roles and primary club evidence for discrepancies:12 additional FBs and Jack Westover to current-club TE; hybrids use latest explicit club roles, not weight. Stable-ID migrations preserve ownership, contracts, appearance and user depth assignments, and are idempotent. Audit: docs/data/pro-backfield-position-audit-2026-10-30.json.

Accepted Pro115 runtime401bd82: build41110120686, APK13003188026, native evidence13003108943; all13 guided-offseason assertions pass. Accepted College412 source09e6e19: run41112361113, APK13003019975, native evidence13005080737; all5 overview/roster/settings assertions pass. Both package with0 warnings/errors. CI64 targeted Pro cases and17 combined Core/College position/backfield cases pass, plus16 harness helpers. College411 packaged but its native check stopped at an offscreen saved-slot Load button;412 fixes that harness scroll and the remaining library Settings label. Screenshots reviewed. Pro runtime inputs are unchanged by the later College/harness commit. No new long-season or full GameCast run is claimed; prior unchanged evidence is reused.

A short64-game real-Pro backfield regression sample has63.14 plays/22.49 points and zero invalid/duplicate units. Scope: docs/pro-guided-offseason.md. An initial over-broad local filter was stopped after starting a long case; final filters explicitly exclude long cases. No completed new long-run evidence is claimed.

Permanent prerelease: https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-10-30-guided-offseason . Only exact tested APKs, checksums and a small manifest are published.

## Accepted Pro114 — playoff totals and tabbed player profiles

Runtime sourcee34056d, successful run41020845808. Exact tested APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41020845808/artifacts/12962065061 . Two targeted Pro cases pass in1s,16 helper cases and32-team source validator pass; Android packaging0 warnings/errors. Native evidence12961925864 in.artifacts/profile-tabs-pro114-ui passes seven flags: league playoff selector, all five tabs, distinct playoff yardage, playoff career, retained filter state, return to overview and portrait identity bounds. Landscape playoff table and portrait overview screenshots reviewed. Earlier113 was canceled for the final narrow-screen header constraint; it is not an accepted build.

Pro statistics and player profiles now separate regular-season/playoff totals and current season/career. Queries deduplicate current/archive copies by season+game ID, exclude preseason/unplayed games and preserve maximum fields. Profiles have Overview, Attributes, Contract, Statistics and Analytics tabs; fixed compact header/tab bar and one independently scrolled section at a time. Statistics shows one selected category rather than every table stacked vertically. Existing save data/schema and contract actions preserved.

Testing is scoped: no new long simulations, broad Pro suite, shared Core/College suite, full GameCast device flow or College rebuild. Reuse accepted Pro112355-case and Core/College726-case/512-game evidence because simulation/shared/College inputs are unchanged. The new filter has a synthetic multi-year/archive/phase separation test; native fixtures use a completed production game plus an explicitly synthetic distinct playoff row to reveal accidental mixing. Exact-APK profile retry scopes also run only the two relevant cases. Existing long regression tests are retained for changes that need them, not rerun or deleted indiscriminately.

Permanent prerelease https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-10-30-profile-tabs contains the exact tested Pro114 APK/checksum/small manifest. College407 remains current and unchanged.

## Accepted expanded player statistics — Pro112 / College407

Runtime/app source1c6291b. Pro112 run41015659149 passes355 fast Pro cases,16 helpers,32-team validator and Android packaging0 warnings/errors. Exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41015659149/artifacts/12959152937 ; native evidence12959323015 verifies production-game fixture, blocking, horizontal scrolling, playing time, receiving and career totals. Screenshots reviewed in.artifacts/expanded-stats-pro112-ui.

College407 build job in41015662290 passes726 combined Core/College cases,16 helpers and packaging0 warnings/errors. Exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41015662290/artifacts/12959137915 . Exact-APK native retry41017184843 at harness9232bb6 passes actual new dynasty/completed week, blocking, playing time, receiving and career totals; evidence12959718338 downloaded/reviewed in.artifacts/expanded-stats-college410-ui. Source-input guard passes. Original device invocation canceled for harness update; retries408/409 failed only on picker scroll navigation and are superseded. No extra APK builds were needed for these harness repairs.

Adds event-derived OL/TE/FB blocking, all-position snap counts, receiving targets/catch rates, QB/defensive pressures, coverage production, longest plays, broken tackles, kicking distance splits, net punting and individual penalties. Shared dense columns and season/career aggregation preserve maxima and matched denominators. Old untracked values stay missing; no fabricated history.512 fixed-seed games across real/generated Pro/College modes have zero invalid/duplicate units and reconciled target/coverage totals. The audit uses32 Pro and136 roster-backed College teams; initial College rosterless-profile samples are superseded. Definitions, metrics and scope: docs/expanded-player-statistics.md. Existing annual/development and full GameCast layout evidence reused where unchanged; this batch does not claim a new multi-season or complete live-view device run.

Permanent prerelease: https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-10-30-statistics . Only the exact tested signed APKs, checksums and a small release manifest are published. Both package identities and save compatibility preserved.

## Accepted Pro109 / College406 — HB/FB and Pro practice-squad retirement

Runtime source f074974176a1210ff5170525ad87ba332fe21b82. Both current app source trees still match exactly. Pro109 build job in40990388530 passes354 fast Pro cases,16 helpers,32-team source validator and packaging0 warnings/errors. Exact APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40990388530/artifacts/12948493646 . Exact-APK native retry40991908827 at harness42ca071 passes all12 flags; evidence12948959599 in.artifacts/backfield-pro111. Screenshots reviewed: HB roster, natural TE in FB depth, and migrated roster without PS. Guard confirms unchanged app inputs. Original109 device job canceled for harness update; retry110 reached all feature assertions but failed its final navigation on an undismissed Saved dialog, superseded by111. Earlier107/108/College405 builds canceled during final input fixes.

College406 run40990391150 passes721 combined Core/College cases,16 helpers, packaging0 warnings/errors and native generated-world/reload plus FB-identity/filter checks. Exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40990391150/artifacts/12949380974 ; native evidence12948549016 in.artifacts/backfield-college406. Air Force FB table screenshot reviewed (five FBs). Prior full GameCast/layout acceptance is reused; this batch's device scope is roster/depth/migration, not a new full GameCast run.

HB/FB roles, optional TE/HB substitutes, lead blocking, intended handoffs/FB screens and safe numeric-ID/old-save identity migrations are implemented. Pro PS UI/actions/CPU fills retired; default new games and every normal load release legacy PS players through existing obligations accounting, preserve IR and active negotiated contracts, and clear stale squad labels. Historical source-accounting fixtures explicitly retain their legacy basis; new default-path tests and native migration verify the new model independently. Three-season real/generated Pro audits each816 regular games show zero PS throughout and zero invalid sampled units; source-cap-constrained active counts are reported honestly in docs/backfield-roster-update.md. Scoped sampling remains~63 plays/team. No stored roster seed added.

Published permanent prerelease: https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-10-29-backfield . Exact signed APKs, checksums and manifest are uploaded; GitHub SHA-256 asset digests match the downloaded/tested bytes. No private evidence archive was published. Source, audit methods and actual scope: docs/backfield-roster-update.md.

## Accepted Pro106 / College404 — Pro pacing reduction

Pro source 4479e4e19cc76ccd90def0116e22ef9f97613f3c, successful run 40984224442. Exact regression/build-tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40984224442/artifacts/12945439523 . 352 fast Pro cases, 16 helpers and 32-team source validator pass; Android packaging has 0 warnings/errors. College404 source c3465e2106da4e4fa00d02da5c214f75a16f38fb, successful run 40983805957, exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40983805957/artifacts/12945674373 . 714 combined Core/College cases and 16 helpers pass, packaging 0 warnings/errors. Later 4479e4e changes only Pro generation and docs/tests; College runtime inputs are identical. No new emulator flow is claimed; unchanged UI/device evidence from Pro104/College403 is reused. Published prerelease remains Pro104/College403.

Normal Pro pre-snap pacing is 36–39 seconds instead of 31–37; College keeps its prior values. Explicit tempo, stopped clock, 25-second administration and late-game rules are preserved. 160 focused combined clock/GameDay cases pass locally. 256-game before/after samples per real/generated Pro mode show offensive plays/team/game 66.32 -> 63.16 and 66.96 -> 62.82; new points 22.59/21.27 and yards 339.46/326.55. Raw reports and limits are in docs/pro-pacing-audit.md. No new multi-season audit claimed. A random generated-start cap defect exposed by the first CI attempt is fixed by reserving bonus headroom at fictional initialization; deterministic one-year worst-case test passes. Existing saves and real contracts are untouched. First Pro105 failure is superseded by successful Pro106.

## Main integration and prerelease complete — 2026-10-29

Merged PR #23 into main at 482cbac7b2963ffa2186191f89978c1d4be13779. Repository is now antdroidx/Football-Sim-Engine; local origin updated. Only main remains locally and remotely. Legacy Java source, Gradle files and historical packs are preserved in Archive/Java-game; tracked caches and the obsolete Java APK are removed. Both current Android workflows remain, with relevant-path main triggers and manual expensive simulation/device scopes.

Published prerelease: https://github.com/antdroidx/Football-Sim-Engine/releases/tag/preview-2026-10-29 . Permanent assets are the exact accepted Pro104 and College403 signed APKs, SHA256SUMS.txt and release-manifest.json. GitHub asset digests match the tested APK hashes. Current app/shared/data/signing inputs still exactly match tested a53e5f3; no rebuild was needed. Prior accepted evidence: 351 Pro cases, 707 combined Core/College cases and both native generated-world/reload flows. Cleanup validation: actionlint, 16 helper cases, packaged-resource validation and diff checks pass.

Deleted 150 superseded completed Actions runs and the unused latest calibration artifact. Retained successful runs 40918675174 and 40918680239 and their four APK/native-evidence artifacts; the older published prerelease is preserved. Historical links to deleted runs are retired; their recorded source/test summaries remain. A local evidence/inventory backup is retained under .artifacts/release-preview/publish/validation-evidence.zip; it was not published because automatic approval review rejected public disclosure of its repository metadata. Release APKs and manifests contain no such archive.

## Accepted Pro104 / College403 — rotations, statistical audit and unique fictional starts

App/shared/Core sourcea53e5f3. Pro run40918675174, exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40918675174/artifacts/12916863024 .351 scoped Pro cases pass inCI2m12;16 helpers and32-pair source validator pass; packaging0 warnings/errors. Device evidence12916494450 is downloaded in.artifacts/pro-generated104. All six checks pass: two actual new franchises, fresh identities/names/ratings, fictional-mode marker, no real opening-trade replay, exact saved-player/contract reload and53 active players. Creation/roster screenshots reviewed. Real-player starts and contracts remain default.

College run40918680239, exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40918680239/artifacts/12916449428 . Version0.20.403 / Android version code803.707 combined Core/College cases pass inCI18s and16 helpers pass; packaging0 warnings/errors. Device evidence12917568142 in.artifacts/college-generated403 verifies two actual new138-team dynasties with11,730 players each, different IDs/names, enabled generated option and exact roster preservation after reload. Creation screenshot reviewed. Generation follows the selected roster/program strengths; the Pro generator uses league positional baselines and tightly grouped team strength. Both persist the actual generated players only, with no extra roster seed and no regeneration on load. Existing saves are preserved. Fictional Pro starts use generated contracts and a fresh standard pick inventory rather than replaying real-player opening trades/allocations.

Offline three-season reports/method in docs/football-balance-audit.md and docs/data/football-balance-2026-10-29 cover6,600 regular games across real/generated Pro and College worlds, including draft/recruitment/development and actually participating newcomers. Generated Pro averages22.4–23.6 points/348–353 yards; generated College25.5–25.8 points/377–387 yards over2026–2028. All180 sampled full GameDay personnel sessions have11 unique eligible players per side,100% primary-QB snap share, meaningful RB3+ work and~37–38% reserve-DL participation. Rotation respects depth/quality/rest; healthy QB/OL stop arbitrary rest; thin packages use same-unit fallback; kneel/spike units are recorded. Pro sack losses now sit outside rushing in net offense/drive stats, and NFL-specific AI pass/run calibration leaves College rules separate.

Limits: the season statistics use normal production engines; College uses synthetic balanced all-FBS12-game schedules and does not claim actual conference/FCS or College postseason coverage. Personnel evidence is sampled180 sessions, not a census of every season snap. Broad real-world plausibility checks are not exact historical replication or an audit of every emergency/coaching archetype. The external NCAA national-trends PDF could not be fetched; no freshly imported exact NCAA mean is claimed. No extra new full visual GameCast/career run is claimed; unchanged viewer evidence is reused with707 engine/clock/coaching/animation guards and the new personnel/accounting checks. Stable main and both package identities preserved.

## Candidate balance acceptance evidence

Final offline reports in docs/data/football-balance-2026-10-29 and methodology in docs/football-balance-audit.md cover6,600 regular games/three seasons per real/generated Pro/College world. All180 sampled personnel sessions have0 invalid units and100% primary-QB share; actual generated newcomers play in2027/2028. Final generated attributes include valid physical measurements and positional archetypes; Pro practice starters stay young, fresh-world veteran remaining terms are staggered.707 combined Core/College and351 Pro pre-final broad guards pass, plus final focused generated/rotation/clock guards. Both APKs and native two-world uniqueness/reload checks pending. No extra roster seed field is stored.

## Rotation/stat audit and unique generated starts — candidate

User direction: generate a fresh fictional roster once at new-save creation when enabled, persist the actual players normally; do not add a stored roster seed or regenerate on load. College follows selected-roster/program strength; Pro uses tight team parity and position-specific athletic/skill archetypes. UI choices implemented; Android acceptance pending. Old saves/real-player defaults preserved.

Shared audit fixes: reserve rotation samples beyond RB2/first DL backup pair; healthy QB/OL remain stable unless exhausted; thin packages fill with healthy same-unit alternatives to11; kneel/spike events now carry actual personnel. Pro has its own calibrated pass bias/run baseline and excludes sacks from QB rushing while retaining sack yards in net team offense; College sack conventions unchanged. Initial128-game samples exposed the old zero third-back usage and short personnel packages. Three-season audits cover32-team Pro real/generated and138-team College real/generated, including draft/recruitment/development; final generated runs are being refreshed after archetype/physical-metadata refinements. All finished personnel samples have0 invalid/duplicate units and100% primary-QB share; backup DL share~37–38%, third-and-later backs~13–19%.707 combined Core/College regressions pass after replacing a brittle timeout yardage-duration assertion with explicit zero-runoff/live-seconds assertions.349 pre-final Pro guards and two generated-mode guards pass; final Pro run in progress. No APK from this phase is accepted yet.

## Accepted Pro103 — dense retention table and contrasting actions

App sourcebdff738, workflow fixture repair7b7f03d, run40907837678. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40907837678/artifacts/12910789994 . Five scoped retention guards pass (CI4s/local1s),16 helpers and32-pair validator pass; Android packaging0 warnings/errors. Device evidence12912083142 is downloaded in.artifacts/pro-retention103. All six presentation flags pass: dense table, multiple visible rows, compact action heights, light/dark alternating stripes and opening negotiation. Reviewed both theme screenshots: eight actionable rows visible, filled primary/outlined secondary controls legible on both stripes, plain linked names/headers. Synthetic $2B-cap fixture isolates presentation; its inflated demands are not production market evidence. Failed102 fixture staging is superseded; no app bug was found there.

User UI direction supersedes the Pro101 retention card design: prefer dense sortable tables for comparable lists throughout both games, reserve larger cards for summaries/narrative content. Retention now uses one row per player with adjacent actions and aligned sortable statistics; eligibility reasons can be tapped for details. No Core/College behavior changed. Reuse Pro101349-case financial/phase evidence and College40241-case/device evidence; no extra College APK or annual run for this presentation-only batch.

## Retention presentation packaging repair

Pro102/run40907056054 compiled with0 package warnings/errors and passed the five scoped retention guards, helpers and validator, but artifact staging expected unrelated trade/coordinator fixtures omitted by the deliberately narrow test filter. Workflow now stages only the required retention fixture for this scope. App sourcebdff738 is unchanged. New packaging/device run required because the first job failed before APK upload.

## UI density preference and retention table candidate

User preference: use dense sortable tables for comparable lists in both games; reserve cards/large blocks for summaries and narrative content. Player names and headers remain plain contextual links, not outlined boxes. Restoring the expiring-contracts table with a single compact row per player, separate Actions column, filled primary action and contrasting surfaced/outlined secondary action on either stripe in light/dark. Eligibility explanations remain accessible by tapping the reason. Contract rules and simulation systems unchanged; reuse Pro101349-case and College402 evidence. New retention-table scope runs only five affected retention guards plus device light/dark density/actions; pending APK acceptance.

## Accepted Pro101 / College402 — retention usability, phase shortcuts and Simmons trade

Pro app/Core086ab52, run40902693409, exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40902693409/artifacts/12908759960 .349 scoped Pro cases pass inCI1m49 (local1m44),16 helpers and32-pair source validator pass; packaging0 warnings/errors. Device evidence12911241025 is downloaded in.artifacts/pro-inseason101. All seven acceptance flags pass: eligible in-season negotiation opens, contract actions in light/dark, preseason->regular, regular->playoffs, playoffs->offseason and persisted boundaries. Light/dark card screenshots reviewed. Fixture uses synthetic $2B cap for isolation, so illustrated demands are deliberately inflated and are not production market-calibration evidence. Core cases exercise actual veteran extension preserving current salary, staff negotiation/let-expire choice, drafted/undrafted rookie windows, cap refusal and source trade compensation. Device phases use completed-game synthetic32-team fixtures, not whole seasons played on device.

College sourceb55a234 (Pro inputs unchanged), run40903197220, exact APK https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40903197220/artifacts/12909769395 . Version0.20.402 / Android version code802.41 scoped combined Core/College season/save cases and16 helpers pass; packaging0 warnings/errors. Device evidence12909649673 in.artifacts/college-phase402 shows real Air Force creation, preseason stopping at regular Week0, interrupting the regular skip after opening Week0 atWeek1, automatic save and successful reload. Both screenshots reviewed. The script completed every DB/reload assertion; its JSON summary was not uploaded because the initial workflow listed it only on failure. Future uploads now include the summary/log in the always-uploaded UI evidence. Do not rerun the APK just to recover that metadata. Earlier College401 packaging failure (missing namespace) is superseded. Reuse unchanged full GameCast, career and annual evidence; no claim of a new complete device postseason/season.

Retention is now a readable card list with filled high-contrast Negotiate and outlined Let expire controls. Eligible extensions and optional staff delegation work during playing phases; current salaries stay intact and new years follow the deal. Rookie restrictions and same-year change restrictions are explained per player. Tags retain the offseason window. NFL rule reference: https://operations.nfl.com/calendar-events/nfl-free-agency/contract-language . Pro and College simulation shortcuts appear for the current relevant phase, process normal weekly decisions, save completed weeks and allow stopping after a week. Pro retains roster/cap gates; College postseason includes conference championships/bowls/playoffs and stops before the portal.

New-franchise opening snapshot2026-10-29 adds reported Jeffery Simmons TEN->CHI, CHI2027 first/2028 first/2027 fourth toTEN and TEN2028 fifth toCHI, matching https://www.nfl.com/news/jeffery-simmons-bears-titans-trade . All672 standard pick identities retained; old saves preserve prior ownership/contracts. Chicago's54 active camp players are legal; regular-season CPU repair now trims oversized active lists via existing release/PS accounting, preserving obligations. Main and both app/package identities remain unchanged.

## College phase build repair

College401/run40902699603 passed scoped engine/save checks but failed packaging on a missing Models namespace import in FoundationPage.Library.cs (six SeasonPhase references). Import added; no Pro source changes.349 final local fast Pro cases pass in1m44. Pro101/run40902693409 remains in progress. College rebuild required before accepting this batch.

## Candidate: retention layout, in-season extensions, phase simulation and Simmons trade

Implementing clearer unstriped contract cards with high-contrast negotiation actions and explicit eligibility reasons. Eligible veterans can negotiate during playing phases, including delegated staff negotiation; drafted rookies wait through their third regular season, undrafted through their second. Pro and College fast-forward shortcuts reuse normal weekly advancement, save each completed week, expose a stop-after-week control and stop at phase boundaries (College postseason includes conference title games). Pro still enforces cap/roster entry requirements. New-franchise October29 roster rights include reported Jeffery Simmons TEN->CHI for CHI2027/2028 firsts and2027 fourth; TEN2028 fifth goes toCHI. NFL source: https://www.nfl.com/news/jeffery-simmons-bears-titans-trade . Existing saves are not overwritten. Chicago's camp roster may legally exceed53; CPU regular-season oversized-roster repair uses existing cut/PS accounting.39 focused cases pass; final fast Pro run and both APK/phase UI scopes pending. No new APK is accepted yet.

## Accepted Pro100 — drag-and-drop depth chart and out-of-position roles

App/Core sourcefa1de8f, build/device run40839037565. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40839037565/artifacts/12882102178 . All340 scoped Pro cases pass inCI1m43 (local1m59),16 UI helpers and32-pair cap validator pass; Android packaging0 warnings/errors. Evidence12882501969 is downloaded in.artifacts/pro-depth100. All six depth acceptance flags pass: actual held native drag moves QB1 to3, arbitrary rank menu returns him to1, WR assignment toTE, unchanged natural player/contract records, and reload preservation. Native-drag and reloaded-TE screenshots reviewed. The fixture is synthetic32-team through the production save loader, not a device-played season. Core tests verify target positional rating/role in GameDay, inactive exclusion, coordinator permissions and old-save defaults. Shared Core/College source unchanged; reuse accepted College, annual finance and unchanged full GameCast evidence. Push-triggered duplicate full run40839037284 was cancelled in favor of scoped100.

Use Organize depth chart: hold the ≡ handle and drop onto the desired slot, or use Move to for any rank. Select a playing role and search/Assign another active player to play out of position. Natural position/attributes/contracts stay intact; target-role rating is shown. Persistent assignments drive the next game, while ongoing games retain their live substitution controls. Training camp follows assigned role/numeric position order. GM trade acquisitions retain CPU promotion authority independently of manual coaching permissions.

## Pro depth editor implementation — pending Android acceptance

The Pro editor now uses the College shared depth assignment/projection service: native handle drag/drop, arbitrary rank moves, all 21 playing roles and searchable active-player candidates. Natural positions, ratings and contracts remain unchanged. Assignments persist separately and feed Pro profiles/GameDay; camp battles follow playing role order. Coordinator restrictions cover both source/target units; GM/CPU trade acquisition retains its separate promotion authority. Seven local focused cases pass; broader rerun is in progress after repairing the trade-helper interaction. Android depth-editor scope is added with a synthetic production-loader fixture; no APK is accepted yet. Reuse unchanged College and annual finance evidence.

## Accepted Pro97 — single season-cap display

App/Coreab0e53b, packaged APK12880312505 in run40835230475: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40835230475/artifacts/12880312505 . All335 scoped Pro cases,16 helpers,32-pair validator and Android packaging0 warnings/errors pass. Initial device invocation failed a harness partial-vs-full breakdown label lookup, not app behavior. Harness-onlyc214496 retry40836275698 passes exact-APK source guard and all seven cap-display flags; evidence12880703404 downloaded in .artifacts/pro-cap-display98. Front Office/hub/inbox agreement, Overview season balance, absence of temporary balance, light/dark, unchanged saved contracts/counting and positive balance resolving the alert pass. Light cap card and dark inbox screenshots reviewed. Explicit synthetic32-team fixture demonstrates temporary positive camp room versus a negative full-season balance; it is not a device-played season.

One read-only full-season budget now supplies Front Office, expanded breakdown, Financial overview/forecasts, Overview/team cards and trade/contract previews. Inbox has one matching season-cap alert and correct Front Office link. Removed temporary preseason room/exclusion rows and misleading250k rounding of cap balances; contract offer increments are unchanged. Actual transaction cap counting, contracts, saves, simulation, College and stable main are unchanged. Local14 focused cases passed3s; reuse unchanged annual/College/GameCast evidence. No active build/test or automation.

Follow-up visual observation outside cap scope: some retry captures show the unchanged team banner without child logo/name content after repeated navigation/theme changes; header team identity and cap cards remain readable. Do not claim this scope retested/fixed all banner behavior.

## Accepted Pro96 — opening IR and lowest-rated practice cleanup

App/Core6741d4c, run40830670108. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40830670108/artifacts/12879200586 . CI passes329 exact-source scoped Pro tests (including both three-season scenarios),16 helpers,32-pair cap validator and Android packaging0 warnings/errors. Device evidence12878214098 is downloaded in .artifacts/pro-ir-policy96. All reserve flags pass: four-game minimum blocked, season-ending blocked, explicit two-week injury blocked, cleared activation, contract preservation and reload. Four-game/season-ending screenshots reviewed. This is an explicitly synthetic32-team UI fixture, not a device-played season. Local328 pre-final fast cases pass1m4; final20 policy/reserve/inbox cases pass1s, including live/sim Finish retry recovery. Reuse unchanged College/GameCast and prior long financial evidence; no new20-season IR claim.

New/current franchises trim excess ordinary practice depth by lowest OVR, preserving legal international exemption and normal owed/dead-cap accounting; existing squads clean up before calendar advancement. Opening medical reserves default to four completed regular-season team games. Eight public season-ending overrides remain blocked for2026; unknown body areas are not invented. Unsigned rookie rights and non-medical EXE/SUSP restrictions remain separate. Timed eligibility derives from saved completed games, so preseason/byes/reload do not advance/reset it. Existing2026 source reserves get a season-start baseline without a new four-game wait after four saved games. CPU, GameCast, player cards and inbox share availability; actual activation still checks cap/roster/contracts. Details and approximation limits: docs/pro-reserve-policy.md. Main remains fdd3d4898b5bd07eeec8dba346024d61f5289474; College/shared source and app identities unchanged. No active build/test or automation. This CPU roster/IR/cleanup batch is accepted.

## Accepted Pro95 — CPU roster movement audit

App/Core616c897, run40794034836:323 exact-source scoped Pro tests,16 helpers,32-pair validator and Android package0 warnings/errors pass. APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40794034836/artifacts/12862539615 . Device evidence12863940517 (.artifacts/pro-cpu-roster95) passes available reserve activation, injured action blocked, contract preservation and reload. CPU management itself is verified in Core whole-league tests, not claimed as on-device annual simulation. Local full324 pre-final-fallback candidate passed11m7 including20 generated/5 sourced seasons; final34 movement/option guards and12 corrected waiver/movement cases passed, and final CI323 includes both three-season scenarios. Pro94 failed waiver handling and is superseded. Main/College unchanged.

Next user steering: trim oversized practice squads by lowest rating, initialize opening reserve injuries to a four-game default, use verified season-ending public reports where possible, and implement timed recovery/CPU return. Keep unsigned rookie rights and non-medical restrictions distinct. Pro95 is accepted; new IR lifecycle work has not yet changed app inputs.

## Accepted Pro93 — future rights and compensatory allocation

App/Core source56cf7c3. Run40780183506 passed313 scoped Pro cases,16 helpers,32-pair source validator and Android packaging0 warnings/errors. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40780183506/artifacts/12857186828 . Device evidence12857057721 downloaded in .artifacts/pro-roster-update93. All ten acceptance flags pass:672 unique standard opening rights/selected multi-hop ownership, visible reserved rights, correct offseason2027 default, actual UI-created Seattle franchise, Nelson/Buckner contract/roster ownership and trade compensation, Nelson roster row, non-tradable projections, synthetic2027 awarded selections and save/exit. Reserved and awarded screenshots reviewed: Seattle has five opening standard rights (four allocated/one conditional), then ten total selections after five synthetic awards. Synthetic fixture does not claim a season played on-device.

All315 full local Pro cases pass7m35, including20 generated and5 sourced seasons (.artifacts/future-full-final.txt). CPU debt/roster failure is repaired without erasing liabilities. Reuse unchanged College399/398 and Pro44 GameCast evidence; no College/shared source changes. Existing saved careers retain ownership/contracts; historical opening snapshots apply only to new franchises. Private undisclosed conditions use documented published-round approximation, not invented clauses. Main remains fdd3d4898b5bd07eeec8dba346024d61f5289474. No active build/test or automation. This roster/pick/compensatory batch is accepted; broader cleanup/archive/rename remain later roadmap work.

## Accepted Pro92 — roster trades and compensatory allocation

App/Core source ef274e6. Run40774235744 passes305 fast Pro cases,16 helpers/source validator and Android packaging0 warnings/errors. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40774235744/artifacts/12853967833 . Device evidence12855070402 is downloaded in .artifacts/pro-roster-update92. All roster-update flags pass: actual UI-created Seattle franchise, Nelson/Buckner ownership/active contracts and exact three compensation picks in its save, visible Nelson roster row, non-tradable projections, synthetic valid32-team2027 awarded-pick display and save/exit. Nelson and award screenshots reviewed. The award fixture is explicitly synthetic; this does not claim a season played on-device or all real future ownership imported.

Local all305 full cases (before two final additional guards), including generated20 and sourced5 seasons, passed3m44; final11 targeted cases include the two old-save/seed-merge guards. CI's final fast count is305. Reuse unchanged College399/398 and Pro44 GameCast evidence. Main untouched; no automation. The wider real 2027-2029 pick ownership audit remains open; developer candidate JSON is not runtime data. Next reconcile/import all known allocations with conditional rights, preserving existing saved trade history. Pro92 is accepted and no build/test is active.

## Active roster/compensatory allocation batch — October28,2026

Canonical D:/AppProjects/Codex/Football Simulation Engine, feature/pro-franchise-foundation. Latest accepted remains Pro91/c55531f documentation, app5c65aae. No new APK accepted yet. New factory-only confirmed Nelson/Buckner trades and three compensation picks are implemented with existing salary-assignment accounting, preserving existing saved careers. A Pro-only compensatory system tracks eligible expired-contract signings, cancellations/early cuts, public APY bands, four/team and32/league quotas, separate sourced staff entitlement, yearly allocation and stable extra pick IDs. Opening 2027 estimates are projected separately; future simulated markets feed subsequent drafts. Draft numbering/completion and top51-vs-full-roster rookie budget planning are updated; a coordinator three-season regression exposed the latter and passes after the repair.

New Draft picks screen browses teams/years and separates allocated picks from non-tradable projections. Details/limits in pro-compensatory-draft.md. All305 full local Pro cases including generated20 and sourced5 annual scenarios passed3m44 (.artifacts/comp-full.txt), then11 final targeted cases passed including two newly added old-save/seed-merge guards. Final scoped CI should contain305 fast cases. Local Android compile passed0 warnings/errors before final Core/harness refinements;16 helpers and32-pair source validator pass. Dedicated roster-update device scope is prepared; CI/device acceptance pending. Reuse unchanged College399/398 and full GameCast evidence. Main untouched; no automation.

IMPORTANT remaining task: all-team real future ownership audit is still incomplete. docs/data/pro-future-pick-audit-2026-10-28.json is research only, not runtime data. It captures70 candidate rows and source review, but multi-hop identities and conditional rights remain unresolved. Do not claim the full real allocation task complete or apply guessed ownership. Only the two confirmed trades' three picks are imported so far. Next reconcile that ledger/conditional reservation and source conflicts after accepting this batch. Archive/rename remain last.

## Accepted Pro depth/schedule/coach batch — Pro91 / College399 compatibility

Final Pro app source5c65aae; core/shared/catalog source046a1d7. Run40747333613 passes294 fast Pro cases,16 UI helpers/source validator and Android packaging0 warnings/errors. Exact tested APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40747333613/artifacts/12842951177 . Device evidence12843181188 is downloaded in .artifacts/pro-staff-calendar91. All staff-calendar-check flags pass: numeric depth order, exactly three preseason weeks, phase-correct Current marker while browsing regular season, four named playoff rounds, Seattle staff illustrations, actual new Kansas City franchise and Andy Reid light/dark. Preseason picker and Andy Reid light screenshot reviewed. Calendar starts from an explicitly synthetic preseason2 save; Kansas City is created through normal UI. Pro90 source046 build was superseded by the one-line live-position-order follow-up; no further app inputs changed.

College399 source046a1d7 run40746521680 passes703 combined Core/College cases and packaging0 warnings/errors. Compatibility APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40746521680/artifacts/12842041575 . Shared portrait optional flags default to unchanged College/player rendering; reuse accepted College398 UI evidence, without claiming a new College399 device walkthrough. No engine rules/RNG/save schema/package/signing/financial changes. Main remains fdd3d4898b5bd07eeec8dba346024d61f5289474. No active build/test or automation.

Behavior: both Pro roster organizer and live substitution picker use the shared College numeric position sequence, with existing Pro grouped OT/OG/OC labels retained. Schedule choices use phase-qualified week identities: preseason3, regular18, playoffs4. Changing phase clears stale week selection; All phases supports explicitly qualified choices; Current appears only in the actual active phase/week, and offseason has none. Official-headshot-referenced illustrated appearances cover all127 named bundled coaches (one vacant slot excluded), include explicit eyewear/moustaches and stable normalized-name identity across team changes. Unknown/generated staff keep deterministic fallback. No photos are shipped. See pro-coach-portrait-review.md and nfl-coach-appearances.json for primary references. Eight new focused cases pass; reuse unchanged annual, finances, exchange and full GameCast acceptance.

## Pro schedule/depth/coaches implementation — completed — October28

User requests College-order depth positions, phase-aware schedule weeks and closer illustrated real-coach faces. Implementation uses shared College position numbering with Pro grouped OL labels; schedule choices have three preseason weeks,18 regular weeks and four named playoff rounds, phase-qualified selections/current markers and safe reset on phase changes. ProCoachPortraits embeds127 visually reviewed official-headshot-derived art profiles, stable by normalized identity; vacancy/generated fallback unchanged. No photos bundled, source names/ownership/coaching ratings/saves untouched. PersonPortrait gains optional explicit glasses/moustache flags; default College/player rendering stays unchanged.

Eight affected schedule/depth/portrait tests and all294 fast Pro cases pass; local Pro Android build0 warnings/errors before final data/harness refinements. Both final Android packages/device acceptance pending. Dedicated staff-calendar device scope checks preseason2 vs regular2 markers, depth order, playoff options, Seattle staff and real Kansas City new-franchise Andy Reid light/dark portraits. Prior College398/Pro88 remain accepted until this batch passes. Reuse unchanged financial/annual/GameCast evidence; no main or cleanup changes.

## Accepted shared news/roster UI — College398 / Pro88

Final app/shared source9cc34c5. College398 run40739215922/APK12839440855 passes703 combined Core/College cases and packaging0 warnings/errors. Pro88 run40739211829/APK12837753565 passes286 fast Pro cases, helpers/source validator and packaging0 warnings/errors. Exact tested APKs:
- Pro: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40739211829/artifacts/12837753565
- College: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40739215922/artifacts/12839440855

Exact two-app verification40740101064/evidence12839197534 (.artifacts/roster-sync-exchange89) passes source equality guards, actual wide filter bounds (all four College controls share a row), opponent roster selection, no duplicate profile button, restored own-team controls, contextual Pro team navigation, direct native save/share/import, reload and preserved roster/contracts/picks/debt/career. College wide roster screenshot reviewed and confirms one row. Test class/display fixture remains explicitly synthetic, not a season played on-device.

Pro84 presentation40732039464/evidence12837400520 (.artifacts/roster-sync-pro84) supplies unchanged news/card, light/dark, management and landscape evidence; Pro landscape roster screenshot reviewed. Only final compact-filter geometry differs; final89 rechecks that component in both hosts. Earlier functional exchange85/87 and flex-row candidates are superseded for final geometry. No unrelated annual/cap/full GameCast scenarios repeated. No active build/test or automation.

Both apps now use FootballNewsCard for left-aligned thumbnail/metadata/headline/preview/story links and a shared responsive card grid. CompactFilters uses explicit responsive Grid columns to avoid native flex rounding. College team links and legacy Teams navigation route to the unified roster with team picker; redundant Profile column removed. Opponent injuries follow the selected roster, own depth-chart management stays hidden for opponents and returns on own selection. Pro roster filters occupy one row on wide screens; phones use available columns. No simulation/finance rules, save schemas, package IDs, signing or stable main changes. Source workspace remains D:/AppProjects/Codex/Football Simulation Engine; branch feature/pro-franchise-foundation.

## Shared news/roster alignment implementation — completed

User requests identical College/Pro news cards, compact one-row roster filters and College team browsing through the roster instead of redundant Teams screen. Implementation in progress; prior College394/Pro81 remain accepted. Shared FootballNewsCard and CompactFilters added; College contextual team links now choose roster team, with user management hidden for opponents. Verify compilation, team-picker persistence/ownership, news alignment and phone/Fold layouts in both apps. No new build accepted yet.

## Accepted contextual UI — October28,2026

College394 source f2ec0ad/run40724719708/APK12832202972:703 combined Core/College cases, packaging0 warnings/errors. Pro81 app f345f90/core f2ec0ad/run40725321084/APK12833235834:286 fast Pro cases (five new calendar-label cases), helpers/source validator and packaging0 warnings/errors; presentation device acceptance passed. Evidence12833821291 in .artifacts/contextual-pro81. Original Pro80 build is superseded by phone-width headline refinement.

Exact two-app verification40727973325 at harness59127ee/evidence12833838921 (.artifacts/contextual-exchange82) passes all flags, including actual header-to-team-page navigation, native College save/share, Pro import/reload and preserved contracts/roster/career. Team-page screenshot reviewed: unboxed linked names and headers. Existing synthetic class/display fixture scope remains explicit. No unrelated annual/GameCast tests repeated. No active build/test remains.

Changes: removed default button outlines, restoring flat player links, College headers/sidebar while preserving text contrast; Pro team cells and main team banner open a roster/schedule team page, College school banners use existing OpenSchool navigation. Pro game cards use linked team cells. Pro overview contains three actual headline/body previews plus News/Statistics links. Structured transaction phase displays P1/W1/PO1/offseason, with unknown legacy stages labeled honestly. No save schema, financial rules, simulation, package or main changes.

Exact APKs:
- Pro: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40725321084/artifacts/12833235834
- College: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40724719708/artifacts/12832202972

# Active October28 contextual UI correction

User requests unboxed names/headers/sidebar, more team links, actual Pro overview headlines, and P1 preseason transaction labels. Implementation started on feature/pro-franchise-foundation; prior College393/Pro78 remain accepted. Changes are not yet built/device accepted. Global button outline removed to restore prior appearance while retaining contrast colors. College school banners and Pro team cells gain navigation; Pro team page provides linked roster/schedule. Structured transaction phase is used; unknown legacy stages are not invented. Finish compile/tests, contextual-link and overview/device checks, then package both apps before claiming acceptance.

# Current work - Football Sim Engine

Updated October28,2026. Canonical workspace D:/AppProjects/Codex/Football Simulation Engine, feature/pro-franchise-foundation. Preserve main fdd3d4898b5bd07eeec8dba346024d61f5289474, both app identities, College saves and negotiated Pro contracts. No subagents or continuation automation. Exchange and management batches are accepted. New UI/efficiency batch is in implementation; see ui-efficiency-plan.md. No new APK is accepted yet.

## Accepted October28 UI/efficiency — College393 / Pro78

Application/shared source ceeeb34. Pro78 run40648950218 passed281 fast Pro cases,16 helpers/source validator, packaging0 warnings/errors and complete presentation-device acceptance. Exact APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40648950218/artifacts/12798821913 . Evidence12799118166 (.artifacts/ui-efficiency-pro78) passes light efficiency/definitions, trade selection/removal, landscape trade, contract presets/optional yearly details, existing dense grids, Fold and dark theme. Contract screenshot reviewed.

College393 run40648953014 passed703 combined Core/College cases and packaging0 warnings/errors. Exact APK: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40648953014/artifacts/12798946403 . Exact two-app verification40651057545/evidence12800585172 (.artifacts/ui-efficiency-exchange79) passed source guards, College light settings/efficiency, direct native Save/Share, Pro picker/preview/apply, preserved identities/finances/career, reload and unsupported-format rejection. Display box scores/class snapshot are deliberately synthetic; real telemetry/formulas/lifecycle have separate Core tests.36 affected local Core/GameDay/contrast cases pass. No unrelated annual/cap/full GameCast runs were repeated.

Both hosts now share measured contrast, visible control boundaries and disabled states; Pro uses navy/teal. Trades have filters/removable assets; offers have current-deal context and Low/Agent/Strong presets. Efficiency uses documented own-game formulas, not proprietary PFF grades/official DVOA. Old context is not fabricated. Save schemas/identities, signed contracts, RNG/rules and main remain preserved. No active build/test remains; no automation recreated. Archive/rename/release review remain later work.

## Active UI/efficiency batch — source ceeeb34

Implementation is committed/pushed on feature/pro-franchise-foundation. Shared contrast uses WCAG relative sRGB luminance with 4.5:1 text correction, visible button edges and disabled states; Pro now uses navy/teal. Trade workspace has per-team name/position filters and removable selected assets. Offers show current deal/agent demand, Low/Agent/Strong presets and optional yearly accounting. College Save/Share directly prepares its file. Existing responsive navigation and sortable grids remain shared.

Both apps expose game-derived opponent-adjusted efficiency, league charts/ranks, success/explosive/sack/turnover/third-down rates and same-position player rate comparisons. New optional full-play aggregates do not consume RNG or change rules; old save context remains unavailable. Formula/scope in efficiency-metrics.md. No proprietary PFF grades or official DVOA claims.

Local36 affected Core/GameDay/contrast cases,281 fast Pro cases and16 helpers pass. Local College Android packaging passes0 warnings/errors. College393 run40648953014 passes703 combined Core/College cases and packaging0 warnings/errors; packaged APK12798946403. Pro78 build job in40648950218 passes and packages APK12798821913; emulator presentation acceptance is in progress. These packaged artifacts are NOT yet accepted device builds. Next finish Pro light/dark/trade/contract/efficiency screens, then exact two-app College light/export checks without rebuilding app inputs. Reuse unchanged annual/cap/full GameCast evidence. No merge/rename/archive actions.

## Latest accepted - College392 / Pro74 draft-class exchange

College runtime source0215ad4, build run40640733536:693 combined Core/College tests and Android packaging passed with0 warnings/errors. Exact tested APK12795552207:
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40640733536/artifacts/12795552207

Pro runtime/core source0b60dbe, build run40640142353:279 scoped Pro cases,16 helpers/source validator and packaging passed with0 warnings/errors. Exact tested APK12795895212:
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40640142353/artifacts/12795895212

Exact two-app verification40642639385 at harness9b56110 passed independent source-input guards, refreshed core fixtures and the complete Android handoff. Evidence12796481744 is downloaded in .artifacts/exchange77-ui. Every player-exchange-check.json flag is true: real College export, native document save, native share sheet, Android document picker, preview/apply, five source identities/appearances,315 generated filler, unchanged roster/contracts/picks/debt/career, reload and unsupported-format rejection. College saved-file, Pro preview and imported-class screenshots reviewed. The College snapshot is deliberately synthetic (five actual roster seniors placed in the new latest-class field); unit tests separately exercise real departure capture and SQLite round trips. Do not describe this as a full season played on-device. Failed invocations75/76 were harness setup (missing school selection, stopping before explicit save); both are superseded, with no app-source changes required.

Additional final test-only cases passed on unchanged runtime source: complete seven-round small-class draft and2500-player full-class draft (12s,224 selections/2276 remaining,0 filler). Three College export/SQLite cases pass; six initial affected lifecycle/export cases pass. No duplicate long NFL/College/GameCast runs: normal simulation and finances are unchanged; reuse accepted73 annual20/5 and prior field evidence. Both package IDs, save schemas, signing and stable main remain unchanged. No active build/test or continuation automation.

Exchange behavior:
- College retains the latest full graduate/early-declaration class before roster removal, excluding transfers/releases/redshirts. The optional save field uses existing schema26; old summary-only departed cohorts are not fabricated. Export in Season center or Player departures, prepare JSON, then Save JSON file through Android's document picker or share it. Save older class files externally before the next full snapshot replaces the latest one.
- Pro imports during offseason free agency or an unstarted draft. File size/depth/version/IDs/attributes are validated; preview shows source/target years and translated ratings. Explicit apply replaces only prospects/scouting/shortlist. Small files receive enough generated prospects to reach320; larger classes retain all players. Rosters, signed contracts, debt, career and traded pick ownership remain intact.
- All14 skills and physical/base attributes translate deterministically by player ID/year; shared canonical overall is recalculated. Names, IDs, size and illustrated appearances survive. Drafted players receive the existing four-year rookie deal/first-round option lifecycle; no direct roster injection. All coaching roles may configure this global class without changing their management authority. Duplicate/current-roster/historical players and imports after a first selection are rejected.
- Opening a draft with index0 no longer automatically selects players; its explicit Start draft action preserves the setup window. Normal offseason advancement still processes CPU decisions; import before advancing free agency into the draft. Coordinator device harness follows the explicit start action where applicable.

## Prior management acceptance - Pro73


Final application source a04ee62, financial/management core43635c0. All-green run40628330665:
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40628330665

Exact tested APK12789619809:
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40628330665/artifacts/12789619809

Device evidence12790617877 is downloaded in .artifacts/pro73-ui. Every pro-presentation-check.json flag passed: visible phone totals/comparisons, draft grade/action, retention demand/action, camp ratings, Fold layout and light/dark themes. Phone career totals/draft outcomes and Fold light analytics screenshots reviewed. CI271 fast Pro cases,16 helpers,32-pair cap validator and packaging passed. Final refinement changes only Pro view column order/widths, not core/shared/College inputs.

Management foundation app/core43635c0 was accepted as Pro71: APK12789078287/run40625932825, exact-APK verification40627341025 at harnessf4c1f61/evidence12789613796 (.artifacts/pro71-accepted-ui). All analytics categories, weekly news, career expectations, camp, standings, visible portrait/landscape draft/retention actions and Fold/dark checks passed. Landscape retention and Fold screenshots reviewed. Original settings-only emulator rotation timeout is superseded by explicit WindowManager rotation and independent orientation/scroll synchronization; it was not an app error. Reuse this unchanged behavior with73;73 specifically rechecks its altered presentation.

Local271 fast Pro cases pass1m27 (.artifacts/roadmap-fast-tests-final.txt). Both additional long cases passed6m7 (.artifacts/roadmap-long-tests.txt):20 generated-contract and five sourced-contract seasons. All273 Pro cases therefore passed on final core; do not repeat long simulations for view/harness/docs-only changes. New source includes8 standings and7 franchise/scouting/analytics/news/fixture cases. Existing690 combined Core/College cases and College390/full GameCast44 acceptance remain reusable; no College/shared engine/renderer change. Do not relabel690 as exclusively Core or College.

## Completed user-selected batch (#1, #2, #4, #5)

- UI: first-column draft/retention actions, primary numerical columns visible on phones, focused analytics category screens, responsive camp/depth views, phone/landscape/Fold/light/dark checks. Secondary table details still scroll horizontally as in College.
- NFL standings: division and wild-card head-to-head/common/conference chains, aggregate strength of victory/schedule, tied scoring ranks, common/conference net points, net touchdowns and save-stable final coin toss. Multi-club reductions restart; wild-card selection preserves original division order. Rules source: https://www.nfl.com/standings/tie-breaking-procedures . Archived-season draft lookup explicitly uses the archive year, preventing the regression caught during this batch.
- Franchise depth: automatic staff-quality scouting and draft-stage combine confidence upgrades, bounded once-per-season measured staff growth, less-secure-club job-market priority, ownership/career context and history chart. Camp uses existing depth, availability, roster/cap and coaching-authority rules; no duplicate roster model.
- Analytics/news: position development comparisons, same-class/same-round draft outcomes, team/league recorded career leaders, shorter performance/roster/cap categories, completed-week factual reviews and explicitly projected late-season playoff pictures. Regular-season scope/deduplicated history, stable news IDs, no tied-game win streak and no premature weekly review are tested. No fabricated clinching claims.

## Deferred / next roadmap

#3 College-to-Pro draft-class export/import is now implemented and accepted above. Later work remains the audited Core/College/Pro test split, release validation and eventual Java archival/repository rename/branch-artifact cleanup. Do not merge unfinished work into main or perform cleanup ahead of the agreed release review. Further career/scouting/analytics enhancements can build on this accepted batch; source-perfect finance remains closed under the practical-estimate policy.

## Financial phase complete - practical model

Latest user direction: stop financial minutiae, use reasonable assumptions/estimates, keep the game simple and wrap finance promptly. This supersedes historical source-perfect/default-promotion gates. Follow docs/pro-practical-finance-policy.md. Missing/private/conflicting details remain game estimates in developer provenance; exact agreement with every public website is not a release gate. Keep the simulation and College independent of Pro finances.

## Financial-phase acceptance - Pro70 (reused where unchanged)

App/core fabf464, run40613034696 (opening device scope):
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40613034696

All build/device jobs passed. Exact tested APK12783469883:
https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40613034696/artifacts/12783469883

Device evidence12784307093 is downloaded in .artifacts/pro70-ui. Real-roster default, compact/expanded cap panel, three rookie imports,32 allocations/735 opening assessments, contract/void views, light/dark appearance, reinstall/save and original-source/negotiated-save migration all passed. New source allocation, unchanged-source migration, negotiated-deal preservation and booked debt/career preservation proof flags are all true. Compact cap and expanded light-theme screenshots reviewed. No active build/test remains. This financial phase is complete under the user's practical-estimate policy.

Local257 Pro cases excluding unchanged twenty-generated-season case passed1m40, including five sourced/three imported-cap seasons;17 final source/hash guard cases passed1s;16 UI helpers pass. CI passed256 fast cases,16 helpers, the source validator and Android packaging. Reuse unchanged generated20 evidence from66, College390 and GameCast44. No duplicate local Android build for the final UI/resource batch; CI packages it.

## Final batch behavior

- New app franchises default to Real roster contracts; Generated contracts remains optional. The factory's existing generated default is retained for backward-compatible internal/test calls; the UI passes the selected mode explicitly.
- Cap panel shows rounded available room, budget, dead money, pending offers and season-start room. Exact salary/bonus/rollover/adjustment fields are behind Show cap breakdown. Contract accounting stays optional; offers retain250k controls/market defaults.
- Three previously unmatched fully guaranteed first-round deals now reconcile published annual cash/cap/total values: Sadiq, Cooper and Bain. Four playing years; fifth-year decision separate. All prior packed records remain equal. Pack2485 records, SHA447f32fb327df9ca3cf20a712b314bc882f8329067dd1164a48a80f7c17b153a; reviewed metadata hash updated. Existing saved deals are not overwritten with new imports.
- Final device scope verifies the source default, collapsed/expanded cap display, all three persisted rookie deals,32 allowances/735 opening assessments, contract/void UI, themes/reinstall and guarded old-source/negotiated-save handling.

## Four requested priorities - practical implementation

1. Reserve activation accepted63: healthy signed RES/INA/DEV players can activate with roster/cap/role checks and saved ACT state. Explicit unknown medical/league restrictions remain protected; no inferred clearance dates.
2. Contract accounting accepted66/69: cash/bonus/void/guarantee separation, realistic rookie terms/options, signed-deal preservation and guarded corrections. Roquan roster cash no longer becomes duplicate buyer-guaranteed salary. Unknown details use practical estimates under current policy.
3. Financial budgets accepted68, final default/UI in70: all32 adjusted allowances and735 independently reported opening cap assessments initialize new sourced franchises. No invented historical cash or forced website-room balances. Existing saves are not backfilled. Remaining publication differences are ordinary data maintenance.
4. Waiver/termination behavior accepted66/67/68: inherited original contracts, tagged release assessments, claim/award budget/role/roster rechecks, assignment-start salary accrual, June1 holds/splits and final-CBA-year handling. Current scope includes practice release/trade guards and explicit international exemption behavior; broader calendar/elevation details stay game approximations.

Financial acceptance is complete. Do not extend this phase with another bespoke player-by-player audit. Java Initial Base Upload archival and repository rename Football-Sim-Engine remain at the end of the broader project; main is untouched.

## Accepted APK/evidence

-69 app/core d71782a, all-green run40590454472, APK12774172307/evidence12773632366 (.artifacts/pro69-ui).253 fast CI/16 helpers/source validator/packaging and expanded opening device flow passed.254 local cases excluding generated20 passed1m52. Original source migrates; negotiated deal, booked debt and career persist. https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40590454472/artifacts/12774172307
-68 app/core afe8b73, run40587763626, APK12772514300/evidence12773465745 (.artifacts/pro68-ui),249 fast CI/16 helpers/source validator/packaging and opening device checks passed.250 local cases incl five sourced/three imported-cap seasons passed1m55.32 clubs/735 source keys/no historical-cash fabrication, Seattle liabilities/contract/void/themes/reinstall/save verified; compact-old budget screenshot reviewed. https://github.com/antdroidx/Football-Sim-Engine/actions/runs/40587763626/artifacts/12772514300
-67 app faa55a1, run40584307166, APK12770853072/evidence12771961687.243 fast/16 helpers and termination device scope passed; refreshed Free agent card and disabled own/duplicate claims reviewed.
-66 app/core db88f28, run40579419877, APK12768613293/evidence12768544312.245 full cases10m6 incl20 generated/five sourced,243 fast and midseason waiver/assignment release device scope passed;333333 earned buyer cash after one week.
-64 source8b06fa7 APK12754790693/run40547793852; exact verification40573852505 at harness00944a7/evidence12766593020: finance/options/trades/six coordinator/June1/waiver/reload accepted. Initial fixture-glob failure superseded.
-63 reserve sourcea8ad14b, run40530334315, APK12745318137/evidence12745348663;21 focused reserve/camp/depth,220 fast and activation/disabled injury/contracts/ACT reload device checks.
-44 full GameCast/aging/field source4f6bb2c, run40438801865, APK12705098072/evidence12705962978.
-College390 source2acfcc1, run40438515279, APK12705660863/evidence12705718044+12705752478:690 legacy combined Core/College cases and both device flows. Do not relabel690 exclusively Core or College before auditing.

## Developer evidence / later maintenance

Raw source cache stays in data/pro/source-snapshots (32 team pages,507 opening pages,95 bonus pages,10 status-conflict pages and primary roster reviews). Do not refetch completed cohorts or delete this cache as disposable artifacts. Huge .artifacts franchise JSON can be136MB on one line; parse selected fields instead of dumping it.

Source dossiers/proofs are in docs/data, including opening debt, actual runtime comparison, guarantee semantic review, Roquan classification conservation and reviewed rookie cash/cap normalization. Source-perfect all32 room matching is explicitly deferred by user direction. Staged payment candidates remain review aids, not automatic imports. Four primary roster-review findings remain ordinary roster maintenance; they are not part of this financial completion gate.

Remaining requested roadmap: deferred draft-class exchange UI, audited test-category split, final release validation and end-of-project archival/repository rename/release cleanup. Historical parity notes are not current completion status. No automatic continuation exists.

CI Pro first attempt 40983806143: 350/351 passed; fresh random fictional team could exceed budget because salary-only scaling omitted its half-salary signing bonus on short deals. New fictional initialization now reserves that bonus headroom; existing saves and real contracts unchanged. Deterministic worst-case one-year veteran fixture plus two generated-world/save/accounting cases pass locally (3/3). Packaging retry pending.

## Pro scouting depth + UI/UX improvements (codex branch)

Implementation in codex/pro-scouting-depth branch: 3-tier scouting model (basic <50% / priority 50-75% / visit 75-95% confidence), 60-priority/30-visit nested limits, deterministic CPU fill, save-compatible migration, draft-results OVR column, table row/button contrast fixes, offseason stage timeline chips.

Focused checks run locally:
- Built ProFootball.Core+ProFootball.App with MSBuild (net10.0-android): 0 errors, 0 warnings.
- dotnet test filter ScoutingDepthTests|FranchiseDepthAnalyticsTests|ManagementRefinementTests: 13/13 passed, 0 failed, ~5 s.
- APK produced: src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk (~87.6 MB; signed).

dotnet test requires escalated sandbox access in this environment for testhost IPC (see docs/pro-local-validation-runbook.md).

## ProSaveStore load-chain rating migration fix

Added ProPlayerRatingService.Migrate to the Pro save-load chain so legacy saves with RatingSystemVersion=0 migrate to v1 and re-derive OverallRating from the shared rating system (SharedRatingsTests.OldSaveAdoptsSharedRatingsWithoutReplacingContracts now passes). Chain order: Pron.Migrate -> ProScouting.Migrate -> ProCareer -> ProEconomy -> ProContractData -> ProMorale -> ProReserve -> ProPracticeRetirement.

Scoped verification: 27/27 passed across SharedRatingsTests + ScoutingDepthTests + FranchiseDepthAnalyticsTests + ManagementRefinementTests (~10 s). Pro app MSBuild net10.0-android: 0 errors.

Accepted CI run 41150835916 on codex/pro-scouting-depth (commit 5fd00c9): pro-build passed 5m33s; artifact pro-football-apk-118 (id 13023655512, ~103.7 MB). Local focused run before push: 27/27 passed (SharedRatingsTests + ScoutingDepthTests + FranchiseDepthAnalyticsTests + ManagementRefinementTests, ~10 s); MSBuild Pro app (net10.0-android) 0 errors.

## FM Touch-style Pro UI polish (codex branch)

Shared UI primitives in src/ProFootball.App/FranchisePage.Ui.cs (PrimaryAction, PosPill, StatTile, SectionCard, TaskTile). Reworked screens: Overview (big record card, cap/roster/coach tiles, 2x2 task grid, news+transactions cards), Roster (position-group cards with colored pills, plain-text player names), Draft (prospect/selections/depth chip row), Inbox (SectionCard), Standings (per-division sections), Schedule (SectionCard). Draft results OVR column retained. Transparent player-name links in all data-grid tables no longer get the boxed button styling.

Verification: 27/27 focused tests pass (ScoutingDepthTests | FranchiseDepthAnalyticsTests | ManagementRefinementTests | SharedRatingsTests); Pro app MSBuild net10.0-android 0 errors; APK built and installed on emulator; Overview and Roster screenshots in dark theme reviewed (visual only, no automated device checks).

Verified the FM Touch-style Pro UI on a Pixel 9 API 36 emulator in both portrait and landscape (phone aspect): Overview dashboard collapses to stacked cards on narrow and switches to summary + task-grid side-by-side on wide; sidebar navigation appears in landscape; Front Office stretch table, Roster position pills and cards all render correctly. Tablet (tablet-size emulator) not yet verified in this batch.

Front Office reorganized into FM-Touch-style card sections (Money / Trading / Draft / Roster tools) — flat 10-button wall replaced with grouped tile grids. Visual verification from emulator screenshot; build only (no test scope change).

## FM Touch restoration + coach profiles (b543bb5)

Cause of the missing UI: the FM Touch overhaul commits (acce54e, a10b47f, bfaa44e) were on codex/pro-scouting-depth, but the last APK artifact (pro-football-apk-118) was built at 5fd00c9, before them. Fresh artifact built here.

Verified present at HEAD: shared UI primitives (PrimaryAction, PosPill, StatTile, SectionCard, TaskTile, CityLabel), Overview card dashboard with Advance/Open stage, Roster position cards with the Columns toggle removed, Schedule "Season" label, dense inbox with week/year stamp, centered loading overlay, Draft class tile in Front Office for the whole offseason, and Playoffs/Awards news stories in the All filter and Overview feed.

New in b543bb5: ProStaff.Profile derives deterministic coaching abilities and preferred schemes (save-compatible, defaults untouched in the record), hires now store those abilities, coach profile rebuilt with hero header, six stat tiles, five ability meters, scheme pair, and role card. Offseason guide now has an "Open <stage>" shortcut next to Continue.

Checks: Pro app MSBuild net10.0-android 0 errors; 57 focused tests passed; CI run 41175520041 completed success; artifact pro-football-apk-121 (13032906388).

## Pro FM Touch consistency, kicker/punter, and College coach polish (4df29b6..5dd1df3)

Published the previously uncommitted kicker/punter split (Position.P, market/trade/depth integration) with the Reserve-roster compact table.

Pro consistency pass: shared PageHeader and StatRow primitives; Player overview now opens with OVR / ceiling / growth / morale tiles; Staff renders each coordinator as a SectionCard with Overall/Offense/Defense/Leadership tiles and preferred schemes; page headers applied to Standings, Staff, Schedule, Statistics, Analytics, Inbox, Transactions, Honors, Career, Playoffs, Draft, News.

College: coach profile gains a KEY RATINGS card (Overall, Leadership, Development, Recruiting) and the team staff table gains Lead and Scheme columns. College behavior otherwise unchanged.

Checks: Pro app MSBuild 0 errors; 64 focused Pro tests passed; College app MSBuild 0 errors; 34 focused College coach tests passed.

## Final branch state and CI (487c878..9bd88f2)

Pushed everything outstanding to codex/pro-scouting-depth, including the kicker/punter split, Reserve roster table, Pro FM Touch page headers, College coach ratings card, and staff Leadership/Scheme columns.

Two regressions from the kicker/punter split were found by College CI and fixed: College position-need lookup needed a Position.P entry (kept at 0 so College recruiting behavior is unchanged), and GameDayEngine now prefers P for punts and K for field goals.

CI at the final commit: College Android 41178645152 success (artifact dynasty-manager-build-416); Pro Football Android 41178646670 success (artifact pro-football-apk-124).

## Coach ratings wired to gameplay (44c9097)

Audit found the Pro coach ability fields were display-only. Now wired:
- ProLeagueService.Profile blends Pass/Rush offense with OC Offense ability and Pass/Rush defense with DC Defense ability; SpecialTeams blends roster K/P with ST coordinator; HeadCoachLeadership and coordinator scheme fields read the ability profile.
- ProStaff.DevelopmentRating gives position-scoped staff development; ProDevelopment.AgeOneYear accepts it and adds growth chance above 80/90 development.
- Pro scouting already used coach Development for report confidence.

College: fixed KEY RATINGS tile overlap by assigning explicit grid columns (2x2 on phones, 4 across on wide screens). College coaching ratings already drove recruiting (CompetingRecruitingService uses head coach Recruiting) and scheme fit.

Checks: Pro app MSBuild 0 errors; 46 focused Pro tests including new CoachAbilityImpactTests (2 passed); College app MSBuild 0 errors; CI runs 41180456977 (College success) and 41180458621 (Pro success, pro-football-apk-125).

## Pro specialist, news, and depth-chart fixes (17aa44e)

- Coach profile preferred-schemes tile rebuilt with explicit responsive columns (was overlapping at narrow widths).
- News ordering now ranks Awards then Playoffs above regular-season week numbers, so playoff/awards stories reach Overview and the All feed instead of sitting beyond the 60-story window.
- Punter mapping: source kickers with NCAA code 20 migrate to Position.P (Michael Dickson verified by new test). Generated rosters now emit K and P separately.
- Depth chart: per-slot caps (QB4, RB4, FB1, WR8, TE5, OL9, DE/DT/OLB6, MLB4, CB7, FS/SS4, K1, P1) and a CPU auto-assign action for the user team.
- College to Pro draft import explicitly preserves FB (code 2) and P (code 20), including legacy K/code-20 punters.

Checks: Pro app build 0 errors; 80 focused Pro tests passed (including 4 new specialist/depth/news tests); CI 41181925095 College success and 41181923538 Pro success (pro-football-apk-126, dynasty-manager-build-418).

## Position fit, permanent conversions, and editors (e25be3c..bf4b554)

Shared PositionFit applies out-of-position knockdowns: Natural 100%, Close 87% (HB-FB, TE-FB, DT-DE, SS-FS, K-P, OLB-MLB), Medium 68% (OG-OT, SS-OLB, CB-FS, WR-CB, TE-OT, family adjacencies), Far 48%, Extreme 30% (line/front-seven to skill, QB or specialist projections). DepthChartService.PositionalRating now applies the multiplier, so an MLB no longer outranks a natural SS.

Permanent conversion (PlayerPositionConversion) sets the new position/code, drops Awareness by fit tier (6/12/18/24) and scales overall. Pro exposes it through player actions and the editor; College through the player profile and editor. College to Pro draft import already maps FB/P.

Editors added under Options in both games: player or staff selection, player name/position/overall/potential/morale, staff name/rating/development/offense/defense/special teams/leadership/schemes (College staff overall is derived and shown read-only).

Checks: Pro app build 0 errors; focused Pro tests 30 passed including 3 new PositionFitTests; College depth/scouting tests 16 passed. CI: College 41230987566 success (dynasty-manager-build-420), Pro 41231232922 success (pro-football-apk-128).

## Responsive shell and library layout (visual QA pass)

Pro: single compact app header (crest + club + section/season + Settings) replaces the duplicated app header and team banner. Content is capped at 1100dp and centred. Shell now reacts to Window.SizeChanged and switches to a persistent sidebar at >=840dp. Sidebar rows are compact. Season summary switches to 2x2 tiles under 520dp and shows a short cap value. Coach profile renamed Overall tile to Coaching.

College: library is capped at 720dp and centred, with Settings and New dynasty inside a shared top bar so landscape no longer detaches them to the screen corners. Settings section gains the Player & staff editor entry.

Verified on emulator: Pro portrait Overview (single header, 2-column season tiles), Pro tablet (sidebar + all sections + two-column task grid), College landscape library (actions centred within column). Screenshots captured locally during the run.

## College in-dynasty visual QA and responsive sidebar

Created a College dynasty on the emulator (Akron, 2026) and reviewed Overview, Roster, Coaching Hub, and Settings in phone portrait and landscape. Roster filters, depth chart table, coaching hub cards, and settings panels render correctly; the remaining duplicate team banner on Overview/Roster is a candidate for removal.

College management shell now reacts to Window.SizeChanged and uses Window.Width, matching the Pro responsive fix, so tablet widths can switch to the sidebar rather than staying on bottom navigation.

## College visual cleanup

Removed the duplicated school banner from Overview and Roster; roster counts now sit in the status line. Styled roster filter pickers with a panel background and the columns toggle as a bordered button so they read as controls. Responsive shell now follows Window.Width/SizeChanged (matching Pro).

## Pro live GameCast parity with College

Pro live screen now uses the College structure: scoreboard, three-column situation (clock / down-distance / ball), field, NEXT PLAY sheet with status and quick calls, primary next/pause row, quick actions for Call play/Call defense/Timeout/More, and a commentary strip. Extra actions moved into a grouped More sheet (Play & Personnel, Game Control, View & Audio, Game Flow). College layout itself is unchanged apart from the earlier preset removal.

## Live game: field-first during snaps

Both games now hide all interactive chrome during a live snap. Pro hides the entire control sheet; College hides the control tray (replay keeps Skip). The scoreboard, field and play-by-play remain. The control sheet with three quick-call recommendations returns between plays.

## Pro live control labels and portrait field ratio

Primary action between snaps is now "Call play"; the playbook opener is "Choose play" (or "Choose defense"). Kickoff stays contextual. In portrait, the field frame is constrained to the 1.76:1 playing-surface ratio and centred; landscape keeps the full-width field.

## Pro live scoreboard/menu fixes

Timeout counters now render as College-style dots. More-menu actions execute directly inside the open menu guard instead of re-entering Run, fixing menu choices that previously did nothing.

## College live viewer parity

College portrait field frame now uses the same 1.76:1 ratio and height cap as Pro. The redundant More button was removed; its actions are merged into the working Options sheet: Timeout, Instant replay, Watch CPU, Take over, Skip to end, plus existing camera/speed/labels/box score/weather/team entries. Options no longer waits for the current snap before opening.

## Live watch pause and menu cleanup

Pro watch mode keeps the Pause control visible while a CPU snap animates; Take over stays hidden until paused. More is now a flat logical list without category headers: Depth chart, Second unit, Coach talk, Instant replay, Viewing options, Watch CPU/Take over, Finish game, Return to franchise. Removed duplicated Speed and Box score entries (both live inside Viewing options).

College watch mode keeps only Pause reachable during CPU snaps. Options remains the single menu; redundant More is gone, and Options contains all previously buried actions.

## Live menu parity

Both live menus now expose depth chart, box score, weather, view/camera/speed/replay/name options, coaching controls, watch/pause/takeover, timeout, instant replay and exit actions. Pro gains explicit Box score and Weather entries; College gains player markers/style, depth-chart/substitution, offensive/defensive control and watch/takeover entries in its Options sheet. No category headers.

## Live control parity

Pro gains offensive/defensive delegation and a depth-chart/substitution entry. GameDaySession carries DelegateOffense/DelegateDefense; ProLiveControl respects both. College removes sprite/circle and position/number toggles: sprites and jersey numbers are always on. Live menus now share depth chart/substitutions, box score, weather, offensive/defensive control, replay, watch/takeover and exit actions.

## Shared live presentation core

Added GameCastHighlight and GameCastFieldLayout in Football.Presentation. Both live viewers now use the same touchdown/timeout/period banners and the same portrait field aspect policy. Pro gains the College highlight choreography and halftime timeout pause in watch mode.

## Shared live menu and snap presentation

Football.Presentation now owns GameCastLiveMenu (canonical action ordering) and GameCastSnapPresentation (animate → highlight → auto replay → period break). Pro uses both directly; College already uses GameCastHighlight and GameCastFieldLayout. This is the first structural step toward a common live-view shell while app adapters continue to supply data, coaches, playbooks and save/return behavior.

## Live control feedback and row parity

Pro live menu now shows Offense/Defense control state (You vs Coordinator), confirms delegation in status and commentary, and exposes a Team shortcut alongside Choose play, Timeout and Options. College shows the same delegation confirmations in its play-by-play strip. Shared menu supports dynamic control labels.


## Common live-game screen — November 1, 2026

College and Pro now launch Football.Presentation.GameCastLivePage through small data/save adapters. Football.Engine.GameDayLiveControl owns effective unit ownership, role locks, pause, modal holds and snap guards; ProLiveControl forwards to it. Removed Pro's separate UI/options/quick-call implementation. College's former viewer is guarded for recorded highlights only. Both live apps share the call picker, Team/Options commands, confirmations, save retry and snap/replay pipeline. Added the host contract and integration guide in docs/shared-live-experience.md.

Landscape is the primary layout: compact combined scoreboard/status header, side call sheet with fixed actions, and full-width field while plays run. Portrait keeps an anchored field and switches the lower region between calls and recent plays. Pause/Resume remains reachable in every automatic mode; You/CPU ownership and playback state are distinct. Special plays and conversion restrictions use league rules. Shared touchback commentary/banner now uses the recorded/rules-based spot instead of always saying25.

Focused validation:72 combined common-control/coaching/clock/completion/presentation/commentary cases pass;7 Pro current-unit/coordinator compatibility cases pass;16 UI-helper cases and packaged-resource validation pass. Both Android apps have compiled and packaged locally; final landscape refinements and the common device harness are still being checked. Pro has two pre-existing nullable warnings in franchise navigation. The first College harness attempt passed menu/state checks but missed the transient animation because uiautomator waits for idle; the harness now records animation frames directly. Do not treat that attempt as full device acceptance. Final results/artifact identity will be appended after validation completes. No main merge or release authorized.


## Shared live-game acceptance — November 1, 2026

Runtime source8af541f6fa4e29f40e56033ca78f45011edcb2f7 on codex/pro-scouting-depth. Landscape now uses the engine's field dimensions and a fixed field/side-panel arrangement; the panel swaps calls and recap without shifting the field. This supersedes the provisional full-width-during-play description above. Portrait and autorotation remain supported. Only app data/branding/persistence adapters differ; normal live UI/control logic is common.

72 focused Core/College tests,7 Pro compatibility cases and16 UI-helper cases pass, plus packaged-resource validation. Final local Debug APK builds both succeed; College has0 package warnings/errors, Pro has2 existing nullable warnings in franchise navigation and0 errors.

Pro's complete ten-group common device flow passed at491ba3d, including all four ownership combinations, selection/clock preservation, Team substitutions/second unit, box score, cancel paths, replay/watch/pause and both orientations. After the layout-only refinement, the final8af541f Pro APK separately passes field geometry, identical pre/post-snap field bounds, pause during a snap and landscape menu return, plus actual Finish → saved result → franchise return. College's final8af541f APK passes all eleven common groups with the main walkthrough in landscape, including scrolling menus, all four ownership combinations and actual Finish → saved result → College return. Animation frames and screenshots reviewed. The shared harness was corrected to capture animation frames before UI idle, read the selected card in landscape and scroll offscreen choices; earlier partial harness runs are superseded. Role locks/conversion legality are engine-tested, not a claim of every device situational branch.

Exact local APK SHA256: Pro `9c5187b7775067fa116fb52619e4cde8a657aa04ae399b78e18fe9ae198f951e`; College `ce2060918dac91f02ae8de372e5a200c2cf3bdc0e9a72396f54cf1c9b81e515e`. Local evidence and validation-manifest.json are in the original attached checkout under .artifacts/shared-live-stage. Those Debug APKs were installed on a temporary read-only emulator; CI APK bytes were not installed. CI packages use their workflow version metadata.

GitHub final-source builds: [Pro141/run41287123786](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41287123786), [APK artifact13076135467](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41287123786/artifacts/13076135467); [College433/run41287125730](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41287125730), [APK artifact13074978403](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41287125730/artifacts/13074978403). Both successful; artifact metadata verified present/unexpired. Their device jobs were skipped; local device evidence above supplies this UI scope. Earlier140/432 are superseded by the final landscape framing. No new full-season, physical-device, save-failure-injection or archived-highlight walkthrough is claimed. Separate app IDs and save formats retained. No PR, merge or release created.

## Field-first landscape and shared staff attributes — November 1, 2026

Follow-up to user device feedback. During landscape action or watching, the common GameCastLivePage now gives the field the available body width and height, retaining the scorebug above and commentary/Pause below. The side call sheet returns at a manual decision. Team/Options return when paused or making a decision. Portrait and rotation remain supported; landscape is recommended, not forced. This supersedes the fixed side-panel acceptance layout above. Both play-calling entries remain visible: coordinator careers see an explicit explanation for the locked unit; head coaches can call both.

Football.Engine now defines the shared eight CoachingAttributes, five MedicalAttributes and four ScoutAttributes. College alone appends Recruiting to football coaches. Shared Football.Presentation renders/edits these catalogs. Legacy save properties remain for compatibility; missing canonical attributes derive from existing skills. Unit development and simulation adapters consume the canonical attributes. Pro coaches retain their existing stable legacy scheme/profile derivation until explicitly set.

Pro has persisted regional/national scouts, available through Staff and Draft room; old franchises receive deterministic departments on load without replacing existing scouting reports or targets. Scout Evaluation, Communication and the relevant knowledge rating influence prediction confidence/uncertainty, not true player ratings. Pro retains Basic/Priority/Visit reports, 60 priority targets and 30 visits. College retains assignment/progress/capacity rules; regional knowledge applies when an existing assignment matches the recruit, otherwise national knowledge applies. No prospect geography is invented for Pro: regional and national scouts collaborate on every report. Medical staff stay separate from football coaches; this does not add a new Pro medical hiring subsystem.

Validation so far: 753 Core tests pass excluding the extended DynastyHealthTests/OvrCalibrationDynastyTests; 13 focused Pro coaching/scouting tests pass, including deterministic department creation, save/reload, and canonical attribute retention. The 753 include all 69 focused staff/recruiting/injury/development/control tests. Final Android builds and device results are recorded below when completed. No merge or release authorized.

## Final follow-up acceptance — November 1, 2026

Runtime source `8683c07c327652fb2c22ac42f569baa2cbd7371f` on `codex/pro-scouting-depth` is pushed. Both fresh GitHub builds succeeded: [Pro144/run41331880655](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41331880655), [APK artifact13093392627](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41331880655/artifacts/13093392627); [College435/run41331883297](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41331883297), [APK artifact13093488536](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41331883297/artifacts/13093488536). Artifacts verified present/unexpired and the exact CI APKs downloaded and installed. Device versions: Pro0.1.144/code144; College0.20.435/code835. These supersede Pro143/College434 for field sizing. Pro142 was cancelled while superseded; no success claimed for that run.

Both final CI APKs pass all five strengthened field/role head-coach device groups: both caller menu entries, full-width and usable-height field with scorebug/Pause beneath, return to the between-play call sheet, actual defensive-call availability after delegating offense, and a defensive snap followed by sheet return. XML geometry and screenshots reviewed: the paused field is2203×536px in each game on the2424×1080 landscape emulator, instead of the rejected2203×62px footer-wrap layout. Pro144 additionally verified Staff → Scouting staff → scout profile (all four attributes) → Back and Coach profile (all eight attributes; portrait also checked). College offensive-coordinator career created through normal UI: both entries, explanation dialog and Take over retaining the defense lock all pass (on434 before the footer-only refinement; same control code in435). This is not a claim of a separate final-build defensive-coordinator career walkthrough.

753 Core tests passed with extended DynastyHealthTests/OvrCalibrationDynastyTests excluded;381 Pro tests passed with TwentySeasons/SourcedFiveSeasons excluded. The narrower69 Core and13 Pro staff/control/scouting checks are included in those totals. Existing save skills, report/target limits and true player ratings are covered. Both apps compiled locally before the final footer adjustment; final shared presentation compiles cleanly and both final APK packages are verified by GitHub builds. No new physical-device, extended multi-season or final-build full-game-finish run is claimed. Prior complete menu/finish acceptance remains documented above; this follow-up covers the changed field/control/staff behavior.

Exact CI APK SHA256: Pro `8ac79df643ce962f0ff21afd3f67e75d1346872ea2816827fbbf63d9f6ea6e7d`; College `22226da9633ee0d9a7504e8bc6ad74c7f6e39b49718b6683e4bdcd5079479480`. Local evidence, downloaded APKs and validation-manifest.json are under the original attached checkout's `.artifacts/live-field-followup`. The original working checkout and user saves were preserved; emulator tests used a temporary read-only clone plus a disposable new coordinator career. Shared staff integration/migration is documented in `docs/shared-staff-attributes.md`. No PR, main merge or release created.


## Team identities, staff quality and Pro medicine — November 1, 2026

College season-zero team strategies now come from the 138-team Playbook Gamer College Football 27 reference supplied by the user and retrieved 2026-11-01. The shared catalog preserves the source offense/defense labels while mapping them onto the engine's existing playbooks, formations, tempo and defensive approaches. Schemes remain fixed at dynasty creation; custom user plans are preserved. Existing dynasties without a saved team plan receive the catalog plan when their game profile is attached.

College generated staff now targets roughly 65 overall for a mid-prestige program. Program prestige raises the attribute center, while a dynasty/team/role seed creates stable per-save variation. Coach, medical and chief-scout attributes are stored in canonical shared objects. Coordinator promotion eligibility was lowered to 64 to match the intentional College distribution. Real-coach import continues to change identities only and retains game ratings.

Pro now hydrates every sourced coach with the shared eight coaching attributes. Franchise identities cover all 32 clubs, with best-effort 2026 scheme labels and an organization-quality index centered at about 70.3. The Seahawks specifically use play-action balanced offense and 4-3 defense. An explicit Play Action playbook was added to the core. New franchises and legacy loads receive missing scheme plans without replacing edited plans.

Pro now contains two persisted medical staff roles per club. Medical and Prevention contribute through the medical overall used for prevention/recovery; Training, Conditioning and Nutrition contribute to player development and the medical overall. Post-game recovery and a rare, bounded injury-prevention roll use the same mapping. Pro development combines head coach, relevant coordinator and strength/conditioning quality. Player growth now uses bounded probability instead of guaranteed annual gains, preventing both universal growth and staff-driven star inflation.

Coaching Talent Evaluation contributes 25% of Pro draft prediction quality; scouts contribute 75%. Draft reports now persist overall confidence plus low/high potential projections and potential confidence. The draft list and prospect page display these estimates. Scouting never changes a prospect's true potential.

Validation: 755 standard College tests and 388 standard Pro tests passed. Focused tests verify 138-team College strategy coverage, prestige and per-save variation, unique staff vectors, Pro averages in the 65–75 staff band, coach Talent Evaluation effects on potential estimates, medical development/prevention/recovery, legacy migration, and five-year growth bounds. Both Android apps build; Pro retains its two pre-existing nullable warnings in franchise navigation.


## Staff ages, aging and succession — November 1, 2026

College already had coach/medical ages, annual development/decline, retirement, movement history and retired-player succession. This pass adds ages and annual lifecycle to College scouts, includes last-season team performance in coach/medical staff change, and replenishes retiring scouts while retaining assignment framework.

Pro coaches now have age, career years, tenure and growth potential. Pro medical staff already used StaffMember ages. Pro scouts now carry the same age/career metadata. An annual Pro staff lifecycle runs once per year: age advances, attribute changes use growth potential, age, team performance and deterministic random rolls, and eligible staff retire. CPU coaching vacancies are filled through the existing hiring flow. Medical and scouting departments generate replacements. A yearly generated candidate pool includes retired Pro players with strong leadership/awareness and generic position-coach coordinator prospects.

Ages and career context are visible in College scout cards and Pro coach, scout and medical screens. Pro Staff also shows recent retirements. User-controlled coaching roles are protected from automatic retirement. Save migration initializes missing ages without changing existing names, contracts, schemes, reports or assignments.

Standard results after this pass: 756 College and 390 Pro tests. The existing 20-season Pro integrity test passed after the final lifecycle changes (1 test, 6m21s).


## Team directories, performance history and academics — November 1, 2026

Every Pro team profile now includes coaching, medical and scouting departments plus identity, team ratings, roster and schedule. College roster views now offer a Team profile & staff screen for any school, with the same staff directory concept, current scheme, team ratings, 0–99 prestige and 0–99 academic quality.

Pro initial coach quality now uses nflverse `games.csv` regular-season evidence from 2023–2025. Head coaches use their own observed record and scoring for/against percentiles when available; coordinators use their club's relevant unit, with the primary side weighted most heavily. Twenty-six current head coaches have observed samples. Mike Macdonald's observed 24–10 record, 25.2 points scored and 19.4 allowed produces a high-70s/low-80s initial profile. The Coach profile displays the observed/estimated basis. Teams without a head-coach sample fall back to the club's weighted offense, defense and win history.

College AcademicQuality is now a persisted 0–99 Team field. Selective institutions have explicit estimates; other programs use a prestige-informed fallback. Academic quality is stable and does not follow season results. The Academics recruiting pitch uses it directly, and a deterministic 30% subset of recruits values education more strongly. Prestige remains dynamic and all clamps now preserve the full 0–99 scale. Custom league editing exposes both values.


## Team history, directories and academic recruiting — November 1, 2026

Pro and College team pages now expose every opposing team's coaching, medical and scouting staff. College team profiles also show current schemes, team ratings, prestige and academic quality. Pro coach initial ratings use nflverse 2023–2025 regular-season scoring and observed head-coach records where available; coordinators weight their primary unit most. Academic quality is a stable 0–99 institutional value and the Academics pitch now uses it directly. Prestige is dynamic on the same 0–99 scale.

Standard validation after these changes: 760 College tests and 391 Pro tests passed. Both Android debug apps built successfully; College has zero warnings and Pro retains two pre-existing nullable warnings. The final 20-season Pro stress rerun passed separately (1 test, 4m43s).

## Coach history attribution correction — November 1, 2026

Pro coach history now stores each observed record as dated team tenures, separate from the coach's current employer. This corrects profiles that previously attached a Cleveland record to Kevin Stefanski's Atlanta entry, a Baltimore record to John Harbaugh's Giants entry, a Dallas record to Mike McCarthy's Pittsburgh entry, and a Jets record to Robert Saleh's Tennessee entry. Mike Vrabel retains separate Tennessee 2023 and New England 2025 spans.

`nflverse games.csv` assigns every 2024 Jets game to Robert Saleh even though he was dismissed after five games. The catalog follows the NFL transaction record for that interruption and uses Saleh's 22-game Jets sample: 9–13, 16.4 points scored and 20.0 allowed. His normalized unit ratings now produce an initial head-coach history rating of 65 instead of the earlier 60. The other historical samples and ratings remain unchanged.

Focused `StaffBalanceTests`: 16 passed, 0 failed. Regression cases cover current-versus-historical club separation, Saleh's exact cutoff, Vrabel's two employers and the existing Mike Macdonald high-coach band.

Standard Pro regression rerun after the focused correction: 397 passed, 0 failed, 0 skipped (2m24s), excluding the two existing long career cases. Pro Android Debug build succeeded with 0 errors and the two pre-existing nullable warnings. Local signed APK: `src/ProFootball.App/bin/Debug/net10.0-android/antdroid.profootball.franchise-Signed.apk`, SHA-256 `234684e1e2c90405ca2c67c0ea63d34484cb4e2ec010ca9b268f3b9e8acbe12b`.


## Independent Pro coach-rating verification — November 1, 2026

Verified all 816 regular-season games from 2023–2025, all 32 team rows, all 26 observed head-coach histories and all 127 runtime starting-rating formulas: zero final discrepancies. Corrected Schottenheimer/LaFleur ties counted as losses, made tied percentiles equal, regenerated Saleh's 22-game unit percentiles and clarified missing-history/club-proxy labels. Individual coordinator tenures and six head coaches without an observed HC sample remain unverified estimates, explicitly disclosed. Existing saved abilities are preserved.

Actual final checks: 3 Python cases, 24 focused staff cases, 401 standard Pro cases (0 failures/skips, 2m37s; TwentySeasons/SourcedFiveSeasons excluded), 192 paired games with zero personnel/rating violations, and a 906-player five-year development cohort under three staff qualities. Pro Debug Android build passed with zero errors and two existing nullable warnings. No fresh College, extended-career or native-device run. Reproducible commands, intermediate failures, source hash, detailed limits and machine-readable evidence: [pro-coach-rating-verification.md](pro-coach-rating-verification.md).


## GitHub Actions Pro148 and College438 — November 1, 2026

Both workflows succeeded from runtime source `a230948f372be61181bc6f4a9b74942d268e50a8` on `codex/coach-history-attribution`. Pro148 run41357049271 passes401 standard Pro cases,16 UI helpers and32 NFLPA source pairs; APK packaging has0 errors and2 existing CS8625 warnings. Exact artifact: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41357049271/artifacts/13104225055 . College438 run41357140126 passes760 combined Core/College cases,16 UI helpers and packaged-resource validation; APK packaging has0 warnings/errors. Exact artifact: https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41357140126/artifacts/13104090628 .

Both exact APKs were downloaded, manifest-checked and successfully verified with apksigner (v2/v3; same stable development certificate). Pro is `antdroid.profootball.franchise` version0.1.148/code148; College is `antdroid.cfbcoach.dev` version0.20.438/code838. College includes the branch's staff aging,138-team strategies,directories and academic-quality updates. Existing coach identities/skills remain preserved; no new individual College performance-history calibration is claimed. Source, artifact IDs and APK SHA-256 values: [build manifest](data/staff-coach-ci-builds-2026-11-01.json). Local APKs/logs are under `.artifacts/coach-ci`.

These are GitHub Actions build/test artifacts, not a release or new device acceptance. Pro TwentySeasons/SourcedFiveSeasons and College DynastyHealthTests/OvrCalibrationDynastyTests remain excluded in these standard scopes. Both native/device jobs were intentionally disabled; no PR or main merge was created.


## Team profile presentation corrections

College prestige and academics now use labeled progress bars without /99; the team browser banner also drops the denominator. College and Pro team profiles use shared numeric 3-4/4-3 labels. Pro team-directory coaching, medical and scout names link to profiles, including opposing teams. Both Android Compile targets pass; College zero warnings/errors, Pro zero errors and two existing nullable warnings. Full packaging was stopped to keep this presentation check scoped; no new APK or native visual acceptance is claimed.


## Team-profile presentation APKs

Runtime source0bf0989: removed redundant Pro History column, linked staff profiles, numeric defensive formation labels in both team profiles and College prestige/academics bars without denominators. Pro149 run41360960044 succeeds:401 tests,0 build errors,2 existing warnings; artifact13105995227 (pro-football-apk-149). College439 run41360962102 succeeds:760 tests,0 build warnings/errors; artifact13105261974 (dynasty-manager-build-439). Existing standard long-career exclusions apply; no new device acceptance claimed.


## Roadmap/parity audit and GitHub hygiene — November 1, 2026

Canonical completion status: ROADMAP.md. The30-area source matrix,12 prioritized findings and shared-code extraction plan are in audits/pro-college-parity-2026-11-01.md, with an HTML companion. New checks:41 Pro and43 Core/College passed; release401/760 counts are reused, not rerun here. No production edits, new APK, fresh rulebook certification, device walkthrough or extended-career acceptance claimed. Removed59 superseded APK artifacts,20 caches,9 artifact-free failed/cancelled runs and both merged remote feature branches. Release assets/non-APK evidence and dirty local worktrees retained. Redundant branch workflow disabled, primary Android workflows active. Exact accounting: audits/audit-evidence.json.


## Approved parity implementation sequence started

Resume at IMPLEMENTATION-HANDOFF.md. College staff links/scout department and Pro readonly opposing medical profiles implemented; both Android Compile targets pass. Existing focused staff/scouting regression cases:20 College/34 Pro pass. Baseline rules scenarios40 shared/13 Pro pass; not a complete rules certification. Device verification and later editor/aging work remain open. Pro player appearance editor was found during follow-up; audit A08 corrected to avoid a duplicate implementation.


## User direction: keep modeling caveats out of game UI

Remove player-facing research/provenance explanations about approximate ages, generated/estimated ratings and club-proxy calibration in both apps. This supersedes A09's recommendation to add assumption labels to profiles. Keep engineering evidence in developer docs, not gameplay screens. Scouting mechanics and stored data/calculations are unchanged. Removed College estimated-star/age markers and coach caveat, Pro calibration/history panels and team provenance, and simplified associated display text. Focused string-presence checks passed; Compile-only checks pass for both Android apps (College0 warnings/errors; Pro0 errors/2 existing warnings). No broad tests, simulations, device run or APK packaging for this text-only batch, per user's usage constraint.


## Parity implementation checkpoint — branch development

Added a source-backed rules matrix, four focused NFL conformance tests, explicit Pro P/K aging peaks, a Pro custom offensive/defensive playbook editor, and persisted user-coach portrait editing. College scouting table clipping was corrected. Shared code now includes a rating-bar component, stat aggregation, staff lifecycle arithmetic, and a separate Football.Engine.Tests project wired into shared CI. Focused results:6 Pro rules/editor,18 College staff/academics,2 shared tests; both Android Compile targets pass. Final package/device checks remain for the editor and portrait changes.


Shared rating palette: player/staff attribute bars, rating badges and quality-based analytics are now color-coded across red/orange/yellow/green/teal/blue/violet bands, including College rank/conference-power visuals and Pro efficiency/overall comparisons. Both Android Compile targets pass; neutral volume and financial charts retain their non-quality color semantics.

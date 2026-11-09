# Pro210 / College483 - setup screens, live motion and repository cleanup

This prerelease contains the current main builds of both games from `bd085d1`.

- Modern, labelled opening setup screens in College and Pro.
- Continuous pre-snap motion, visible pre-snap penalties, and corrected conversion scoring and goal-line animation.
- Reduced CI to the two per-app workflows and retained only the active launch harnesses and their helper tests.
- Removed obsolete scoped/shared-live smoke scripts and the duplicate Pro branch workflow.

Validation:

- College [run 42656100793](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42656100793): 424 Engine Fast, 12 Engine Balance and 629 Core/College tests passed. APK packaging plus the full management and career-options emulator flows passed.
- Pro [run 42656101290](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42656101290): 486 standard Pro tests and 32 NFLPA source pairs passed. APK packaging plus the full franchise/GameCast/save-reload/coordinator device flow passed.
- Local focused checks: 24 UI-helper tests, packaged-resource validation, NFLPA normalization, Python compilation and actionlint 1.7.12 passed. Both Debug APK packages were built and locally signature-verified.

APKs:

- College: `antdroid.cfbcoach.dev-Signed.apk` - package `antdroid.cfbcoach.dev`, version code `1883`, version name `0.20.1483`, 91,588,658 bytes, SHA-256 `07FFEA757B2D3560281BA656789A985AA92CCEB7DF039F184CB854F1D9137BDE`.
- Pro: `antdroid.profootball.franchise-Signed.apk` - package `antdroid.profootball.franchise`, version code `1210`, version name `0.1.1210`, 90,309,129 bytes, SHA-256 `04295364D8234A9CC363988E9BBF33DFCEE1F68ABEF40931350B79F8D0CE3379`.

Both exact downloads pass apt dump badging and pksigner verify with APK Signature Scheme v2 and v3 and signer certificate SHA-256 `97D88E54DD2835D3622E50D4CE8BB72F1D0A3C7C932FBEFAB6A5B113A4BF8BF7`.

Device evidence:

- [College management](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42656100793/artifacts/13692444188)
- [College career options](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42656100793/artifacts/13692513181)
- [Pro live](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42656101290/artifacts/13694411213)

Limits: the established release scope excludes the long College career/calibration cases and Pro TwentySeasons / SourcedFiveSeasons cases.
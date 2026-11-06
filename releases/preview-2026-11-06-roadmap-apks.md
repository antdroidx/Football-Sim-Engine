# Pro190 / College476 — roadmap completion and device acceptance

This prerelease completes the near-term College and Pro roadmap.

- Durable motivation, locker-room state, rivalry series, coach Hall of Fame records and CPU coach histories.
- Player-authored social replies plus persisted likes, reposts and follows in both games.
- Replay highlights sourced from recorded snaps and sourced NCAA/NFL pre-snap motion/set boundaries.
- Stable Pro person-level news identity, repaired scout populations and save-compatible legacy loading.
- Exact-APK device acceptance for social actions, rivalry/chemistry/motivation, replay highlights, the
  pre-snap hold/effect and portrait/landscape draft and retention actions.

Validation:

- College476 run `42287497915`: 380 Engine Fast, 12 Engine Balance and 629 Core/College tests passed; packaging,
  live replay and career-options device jobs passed.
- Pro190 run `42287498698`: 486 standard Pro tests passed; NFLPA normalization, packaging and the full live
  GameCast/reload/coordinator device job passed.
- Exact-APK action runs `42290592355`, `42298345683` and `42311170531` passed the new roadmap device assertions.
- Local focused checks passed 44 Engine, 10 College, 22 Pro and 24 UI helper cases; both compile targets,
  packaged-resource validation, NFLPA normalization and actionlint 1.7.12 passed.
- Post-merge main workflows passed: College build 478 (`42313407089`) and Pro build 203 (`42313407092`), including
  their repeated broad test scopes and APK packaging.

APK source artifacts:

- [College476 APK](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42287497915/artifacts/13538990919)
- [Pro190 APK](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/42287498698/artifacts/13538912317)

Limits: the established release scope excludes the documented long College career/calibration cases and Pro
`TwentySeasons` / `SourcedFiveSeasons` cases. Earlier failed/cancelled exact-APK retries were harness-only and
are superseded by the successful runs above.

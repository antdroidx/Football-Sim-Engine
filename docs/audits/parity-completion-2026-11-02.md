# Approved parity continuation: implementation and acceptance

November 2, 2026. Branch `codex/parity-roadmap-audit`. The Older Design Backlog was excluded. Full evidence and exact APK identities: [acceptance manifest](parity-acceptance-current.json). Historical checkpoints remain in the [handoff](../IMPLEMENTATION-HANDOFF.md); their pending statements are superseded by its current table.

| Approved work | Delivered and checked |
|---|---|
| Staff/profile correctness | College user-coach directory role, actual scouts, stable-ID/team routing, correct role attributes and opponent edit guards. Own/opponent roles checked in both themes, with landscape attributes and rating endpoints. |
| Custom-play completion | Offensive and defensive Save/restart/reload/live execution. Pro158 confirms excluded stock calls disappear from the active-book picker and the saved custom defense executes. |
| Portrait completion | College Beard/Gray and Pro Buzz cut/Stubble survive full process restart/reload. Pro portrait Save returns to the directory with one Back; opponent role matrix is read-only. |
| Palette/analytics | Shared range colors; Pro OVR comparisons; signed metric callbacks and continuous comparison scale; higher/lower-is-better directions. Both theme variants and responsive layouts inspected. |
| Rules and simulation | Timeout/period boundaries, onside outcomes, dynamic kickoff formation/touchback policy, ordinary safeties/free kicks, current two-point policy and dead-ball penalties. Four failing elapsed-time cases led to a penalty-event metadata correction;68 focused clock/penalty checks pass. |
| Production modes |768 completed games across both leagues, real/generated rosters, Simulate/Watch/Play and CPU/controlled calls. Personnel uniqueness, clock bounds and statistical reconciliation pass. [Results](mode-conformance-results.md). |
| Specialist aging | P32/K33 peaks;64 cohorts per position over12 annual steps, with ceilings and growth/decline checks. |
| Shared code | Competition/scope/deduplication queries and adapters, staff arithmetic after annual characterization, phase results/task UI, staff-name links and canonical FB/K/P/physical/position boundaries. [Ownership and extraction decisions](test-ownership-and-refactoring.md). |
| Test ownership | Engine/College/Pro projects and explicit Fast/Balance/Career runner; Device requires an exact hashed APK. Further legacy fixture moves remain incremental maintenance. |
| College.Core | Evaluation complete: defer physical separation until dependency/test ownership supports compatible migration. |
| Repository | Branch pushed; final CI APKs and full draft changelog produced. Eleven superseded build artifacts removed, freeing1,025,437,709 bytes. Main/active branch, release assets, useful evidence and local files retained. [Cleanup record](parity-cleanup-2026-11-02.json). |

## Final packages and checks

- [Pro160 Actions artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41560752104/artifacts/13196404017):27 focused Pro checks; packaging0 errors and2 existing nullable warnings.
- [College448 Actions artifact](https://github.com/antdroidx/Football-Sim-Engine/actions/runs/41560740255/artifacts/13196798689):30 Engine and107 College/shared checks; packaging0 warnings/errors.
- Final runtime source `d3ed91a`. Compared with the device-tested159/447 runtime, the only application change is the neutral box-score caption. Per-APK native evidence is retained separately; a later build number does not retroactively expand an earlier walkthrough.
- Pro160 final upgrade/install and launcher version0.1.160 pass. Disposable read-only emulator shut down after QA.
- College448 final native smoke: preseason-to-Week1 transition, active-book picker, Levels incompletion at WASH25 with clock15:00 to14:56, one pass attempt and corrected box-score caption.
- [Full draft changelog](../releases/preview-2026-11-02-parity.md). These are CI artifacts; this batch does not merge main or publish a prerelease. Local cleanup remains deferred, and the unrelated branch-workflow edit remains uncommitted.

## Explicit limits and retained finding

The bounded scenarios do not implement every officiating exception. Live-ball fouls, late-half runoff/untimed-down and return/try safety exceptions remain unsupported extensions, detailed in the [rules matrix](rules-conformance-matrix.md). The768-game comparison is not a new balance calibration or complete real/generated multi-decade career audit.

`CapInitializationTests.ThreeSeasonsPreserveImportedCapRolloverDraftAndCpuBudgets` failed once in Pro153 attempt1 (2028 Philadelphia active16, room25,997,183, dead cap170,834,016). Three exact local repeats and the unchanged413-case CI retry passed. Root cause is unconfirmed; no assertion was weakened and no financial policy changed. This remains a lifecycle finding, not a claimed fix.

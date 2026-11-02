# Shared-code continuation: ownership and extraction boundaries

Updated November 2, 2026. Scope is the approved implementation handoff; the older design backlog is excluded.

## Test ownership and tiers

| Owner | Project | Current incremental boundary |
|---|---|---|
| Engine | `tests/Football.Engine.Tests` | Shared stat totals, lifecycle arithmetic and position model. PositionTests moved here with no duplicate source/link in the College project. |
| College | `tests/DynastyManager.Core.Tests` | College/data integration plus legacy shared simulation coverage. Shared engine ownership migration remains incremental. |
| Pro | `tests/ProFootball.Tests` | Pro adapters, franchise policies and Pro lifecycle behavior. |
| Device | `build/*-smoke.py` and exact-APK evidence | Android UI/navigation/save behavior; not represented as passing xUnit cases. |

Newly classified tests use xUnit's `Category` trait: `Fast` for deterministic boundary checks, `Balance` for seeded mode comparisons, `Career` for multi-year specialist cohorts. Shared classes additionally use `Owner=Engine`. Device is an execution tier with its own APK/package/hash prerequisites, not a synthetic xUnit test.

Verified commands (run from the feature worktree):

```powershell
dotnet test tests/Football.Engine.Tests/Football.Engine.Tests.csproj -c Release --filter 'Category=Fast'
dotnet test tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj -c Release --filter 'FullyQualifiedName~RulesBoundaryMatrixTests'
dotnet test tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj -c Release --filter 'FullyQualifiedName~SimWatchAndCpuDelegatedPlayProduceTheSameStats'
dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Release --filter 'FullyQualifiedName~SpecialistCohortsKeepTheirCeilingsAndDeclineAfterTheirPeak'
```

These categories do **not** classify every legacy case. Do not treat `Category!=Career` as a guaranteed fast whole-repository suite: unclassified historical long-running cases still exist. The College overview filter drops the moved PositionTests reference; the existing shared step runs those cases in Engine instead. Main-branch CI now also triggers for changes to the Engine test directory. Existing long-suite exclusions are unchanged. Actionlint1.7.12 passes for the modified College workflow.

## Presentation inventory

Pro coaching, medical and scouting attributes already use shared `CoachAttributeViews`, which delegates to shared `PlayerAttributeViews`. Player profiles also use `PlayerAttributeViews`. A source search found no private `ProgressBar` in Pro app code. Therefore these bars already receive the shared quality palette; replacing them with the differently sized `StaffProfileComponents.RatingBar` would change layout without removing a duplicate calculation. Summary number tiles are not additional rating bars.

College program identity uses `StaffProfileComponents.RatingBar`. Team/profile identities, role authority and state mutation remain in league page adapters. Further staff-directory extraction should share a stable-ID row/action contract, not transplant a league's page class or infer edit authority from its displayed name.

## Remaining extraction boundaries

| Work | Evidence / next boundary |
|---|---|
| A10 query scope | `ProPlayerStatistics.Lines` owns played-game, phase, season and `(SeasonYear, GameId)` deduplication. College's `VisiblePlayerStatistics` replaces current-year stored lines with rebuilt current totals. Those source-selection policies differ; share explicit query scope/adapters while retaining those policies. Shared `PlayerStatTotals` already owns counter/max aggregation. |
| A12 lifecycle | `StaffLifecycleMath.Shift` is used by Pro coaches/scouts and College scouts. Four Pro cases pass: six annual staff updates per fixed franchise ID, complete coaching/medical/scout/candidate/retirement snapshots match after JSON reload, and repeating the same year returns the same state. Four College cases pass: six annual scout updates, forced replacements, unique IDs, valid assignments, attribute bounds and matching output after reload. Pro games are marked-complete fixtures, not simulated career evidence. College's `AdvanceSeason` is a once-per-transition operation rather than a standalone idempotent API; preserve caller ownership when extracting. Preserve league seed keys/random ordering and keep hiring/replacement/retirement policies separate. |
| Phase results/task UI | Inventory actual College/Pro phase entry points and role checks first. Extract blockers/next-action contracts after characterization; do not merge phase enums or saved numeric values. Still pending. |
| Rating/physical/position adapters | `ProPlayerPositions` already delegates conversion to `PlayerPositionConversion`; `ProDevelopment` uses canonical overall calculation and deliberately separate aging. `PlayerPhysicalService` supplies shared deterministic measurements and preserves supplied measurements. Specialist fallback measurements and all import adapters still need explicit coverage before consolidation. |

## College.Core evaluation

The shared engine currently has 97 simulation source files and retains College-specific scouting, recruiting, dynasty history and season services alongside GameDay/rating services. A directory/assembly split now would move interdependent APIs while shared-test ownership remains incomplete. Recommendation: defer the physical split; finish test ownership and inventory callers of the College-only services first. Keep the `DynastyManager.Core` namespace/assembly identity and serialized types compatible. This completes the initial evaluation, **not** a core extraction or migration.

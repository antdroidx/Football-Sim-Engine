# Pro coach rating verification — November 1, 2026

Verified the existing coach-attribution correction against all 816 completed 2023–2025 nflverse regular-season games. All 26 observed head coaches retain the clubs they actually coached in this window, including coaches now employed elsewhere and Vrabel's two employers. The source's 2024 Saleh attribution is corrected at week five using the [Jets' dismissal announcement](https://www.newyorkjets.com/news/jeff-ulbrich-interim-jets-interim-head-coach-10-08-2024). No postseason or 2026 results enter these initial ratings.

The audit found two incorrect loss counts: Brian Schottenheimer's Dallas 2025 record was 7–9–1, and Matt LaFleur's Green Bay 2023–2025 sample was 29–21–1. Ties now remain separate and count as half a win. Scoring and club-win percentiles now give tied results the same average ordinal rank; the former ordering could give equal performances different values. Saleh's unit percentiles are reproducibly calculated from his 22 games. His rounded initial history rating remains 65.

The reproducible table generation changes 32 team rows and 11 coach rows, mostly through this explicit tie convention. Those are recalibrated entries, not 43 incorrect employer assignments. The rating formulas and their weights are preserved. Club history uses season weights 20% / 30% / 50%; individual head-coach unit percentiles are weighted by actual games coached. Each season's scoring comparison uses all 32 clubs and a 20–80 percentile scale. The generator checks complete, unique source games and all bundled head-coach identities before accepting a match.

The independent Python verifier also compares the production C# history-rating result for all 127 named, non-vacant coaches against its own calculation. There are zero remaining table or runtime-formula discrepancies. Actual ability profiles add role adjustments and small franchise-specific variation; saved ability values are preserved.

Six head coaches have no observed head-coach sample in this window: Jesse Minter, Klint Kubiak, Jeff Hafley, Mike LaFleur, Joe Brady and Todd Monken. They and the coordinators still use current-club estimates. Their earlier coordinator tenures are not identified by this dataset and have not been individually verified. Profiles explicitly identify the club proxy and distinguish missing head-coach history from unnamed coordinators. This audit verifies historical inputs and formula consistency; it does not establish an objective ranking of individual coaching skill or turn club proxies into personal career records.

## Completed checks

- Three Python rank/record checks: passed.
- Focused `StaffBalanceTests`, `ScoutDepartmentTests`, `CoachAbilityImpactTests`: 24 passed, zero failed.
- Standard Pro suite, excluding `TwentySeasons` and `SourcedFiveSeasons`: 401 passed, zero failed/skipped, 2m37s. These extended careers were not rerun for this data correction.
- Source audit: 816 games, 32 team rows, 26 observed head coaches and 127 runtime baselines; zero discrepancies.
- Paired staff audit: 64 identical matchups/seeds per arm, 192 games total. Current staff: 24.46 points/team, 366.37 yards/team, 59.50% completions. Neutral-65 staff: 24.71 points/team. Extreme 95-versus-35 staff improved the favored margin from −2.98 to +9.97. Zero malformed/duplicate personnel units or development rating violations; no audit findings.
- Five-year development: the same 906 initially young players under quality 40/65/95, without replacement. Final mean overall 72.85/73.32/74.31; final 90+ share 2.87%/3.31%/4.64%. This is a bounded development cohort, not a new multi-season franchise run.
- Pro Debug Android packaging: succeeded, zero errors, two existing CS8625 warnings. No new emulator or physical-device check; College inputs unchanged.

One interim focused run had an incorrect newly written Saleh assertion of 66; production correctly returned 65 after rounding. The assertion was corrected and all 24 focused cases then passed. The initial source-verifier failure is retained locally as before/correction evidence, and its final run passes.

The compact [machine-readable evidence](data/pro-coach-rating-verification-2026-11-01.json) includes source hash, all dated head-coach records, game-arm summaries and the development cohort. Raw downloaded games, detailed runtime profiles, TRX and local APK remain in `.artifacts/ratings-audit` or the app's ignored build output. This is local test/build evidence, not a published release.

## Reproduce

Run from the feature checkout root:

```powershell
New-Item -ItemType Directory -Force .artifacts/ratings-audit
curl.exe -L --fail --silent --show-error https://raw.githubusercontent.com/nflverse/nfldata/master/data/games.csv -o .artifacts/ratings-audit/nflverse-games.csv
python -m unittest discover -s build -p test_pro_coach_history.py
dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Release --filter 'FullyQualifiedName~StaffBalanceTests|FullyQualifiedName~ScoutDepartmentTests|FullyQualifiedName~CoachAbilityImpactTests'
dotnet run --project tools/FootballBalanceAudit -c Release -- staff-sensitivity 64 .artifacts/ratings-audit/staff-sensitivity-final.json
python build/verify-pro-coach-history.py --games .artifacts/ratings-audit/nflverse-games.csv --runtime-audit .artifacts/ratings-audit/staff-sensitivity-final.json --output .artifacts/ratings-audit/coach-history-verified.json
dotnet test tests/ProFootball.Tests/ProFootball.Tests.csproj -c Release --no-build --no-restore --filter 'FullyQualifiedName!~TwentySeasons&FullyQualifiedName!~SourcedFiveSeasons'
dotnet build src/ProFootball.App/ProFootball.App.csproj -c Debug -f net10.0-android -p:AndroidPackageFormats=apk -v:minimal
```

The verifier normally only reads the catalog and fails on discrepancies. The explicit `--update-catalog` option regenerates its two tables after review. The public games file may change later; compare its SHA-256 with the recorded source before interpreting new differences. Staff metadata varies between new franchises, so repeated current-staff simulation summaries can differ; each audit's three arms share one roster and fixed game seeds.


## GitHub Actions APKs

Source `a230948` is now packaged and tested in successful Pro148 run41357049271 and College438 run41357140126. Their401 Pro /760 Core-College cases pass. Both downloaded APK manifests and v2/v3 signatures verify. Exact artifact links, application versions and SHA-256 values are in the [CI build manifest](data/staff-coach-ci-builds-2026-11-01.json). No new device or extended-career result is claimed.

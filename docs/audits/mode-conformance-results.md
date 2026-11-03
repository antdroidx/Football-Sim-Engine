# Production mode comparison — November 2, 2026

32 games per cell, 768 completed games total. Fixed seeds; real and generated rosters from the production adapters. This is a bounded comparison, not a balance calibration.

CPU-delegated completed game records match across Simulate, Watch and Play for each seed. All scrimmage events have 11 distinct players per side; clock bounds and targets/coverage targets reconcile. Controlled runs supply offensive recommendations and defensive calls for the home team. Play retains its own timeout/kneel authority, so controlled distributions are compared rather than required to match CPU outcomes.

| League | Roster | Calls | Mode | Games | Points/team | Yards/team | Events/game |
|---|---|---|---|---:|---:|---:|---:|
| College | Real | CPU | Simulate | 32 | 24.02 | 347.34 | 168.50 |
| College | Real | CPU | Watch | 32 | 24.02 | 347.34 | 168.50 |
| College | Real | CPU | Play | 32 | 24.02 | 347.34 | 168.50 |
| College | Real | Controlled | Simulate | 32 | 22.91 | 352.39 | 170.44 |
| College | Real | Controlled | Watch | 32 | 22.91 | 352.39 | 170.44 |
| College | Real | Controlled | Play | 32 | 22.64 | 351.67 | 163.88 |
| College | Generated | CPU | Simulate | 32 | 24.27 | 365.09 | 168.47 |
| College | Generated | CPU | Watch | 32 | 24.27 | 365.09 | 168.47 |
| College | Generated | CPU | Play | 32 | 24.27 | 365.09 | 168.47 |
| College | Generated | Controlled | Simulate | 32 | 26.58 | 373.50 | 170.31 |
| College | Generated | Controlled | Watch | 32 | 26.58 | 373.50 | 170.31 |
| College | Generated | Controlled | Play | 32 | 26.00 | 371.61 | 164.84 |
| Pro | Real | CPU | Simulate | 32 | 25.86 | 377.25 | 183.22 |
| Pro | Real | CPU | Watch | 32 | 25.86 | 377.25 | 183.22 |
| Pro | Real | CPU | Play | 32 | 25.86 | 377.25 | 183.22 |
| Pro | Real | Controlled | Simulate | 32 | 26.11 | 377.31 | 183.12 |
| Pro | Real | Controlled | Watch | 32 | 26.11 | 377.31 | 183.12 |
| Pro | Real | Controlled | Play | 32 | 24.77 | 369.77 | 178.22 |
| Pro | Generated | CPU | Simulate | 32 | 22.86 | 352.33 | 178.78 |
| Pro | Generated | CPU | Watch | 32 | 22.86 | 352.33 | 178.78 |
| Pro | Generated | CPU | Play | 32 | 22.86 | 352.33 | 178.78 |
| Pro | Generated | Controlled | Simulate | 32 | 20.70 | 339.58 | 178.62 |
| Pro | Generated | Controlled | Watch | 32 | 20.70 | 339.58 | 178.62 |
| Pro | Generated | Controlled | Play | 32 | 20.75 | 331.36 | 174.09 |

Reproduce from the feature worktree:

```powershell
dotnet run --project tools/FootballBalanceAudit -c Release -- mode-conformance 32 .artifacts/mode-conformance.json
```

The audit found CPU kneels tagged as user calls. The production path now retains CPU provenance; a focused two-league regression locks it. Mode labels do not create separate football physics engines.

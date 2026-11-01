# Shared staff attributes

`Football.Engine` owns `CoachingAttributes`, `MedicalAttributes`, and `ScoutAttributes`. `Football.Presentation.CoachAttributeViews` and `CoachAttributeEditor` provide the common ordered labels and controls. New game hosts should use these catalogs rather than define their own ability lists.

Football coaches use Leadership, Development, Offense Playcalling, Offense Development, Defense Scheme, Defense Development, Special Teams, Talent Evaluation. College appends Recruiting; Pro does not. General Development and the relevant unit's Development are averaged for player development. Simulation adapters read the relevant playcalling/scheme/leadership skills.

Medical staff use Medical, Training, Conditioning, Prevention, Nutrition. They have no football-coach skill list. College injury/recovery uses medical staff overall; strength development uses Training, Conditioning and Nutrition. This change does not introduce a new Pro medical-staff hiring subsystem.

Scouts use Evaluation, Communication, Regional Knowledge, National Knowledge. Prediction quality is 50% Evaluation, 20% Communication, and 30% applicable knowledge. College uses regional knowledge for a prospect matching an existing state/region assignment, national otherwise. Pro departments contain two regional and one national scout; their reports collaborate across the draft class because Pro prospects currently have no reliable origin-region field. No geography is fabricated.

Pro's existing confidence tiers and Basic/Priority/Visit workflow remain authoritative, including 60 priority targets and 30 visits. Better prediction quality raises confidence inside the existing tier and reduces grade error. College quality adjusts uncertainty around rating estimates while preserving reveal thresholds, assignment capacity/progress and residual uncertainty. Neither game changes a prospect's true ratings as a result of scouting.

## Save compatibility

Canonical attribute objects are nullable in existing staff records. Missing objects are projected from legacy saved skills. College maps general/unit development from PlayerDevelopment, offense calling from the old coordinator Scheme (head coaches use GameManagement), and defense from Scheme. Medical Training derives from PlayerDevelopment, Prevention from Medical, Nutrition from Conditioning. Pro retains its stable legacy name-based profile derivation until canonical abilities exist; explicit canonical ratings such as 55 are never treated as missing.

Legacy scouts map TalentEvaluation to Evaluation, WorkRate to Communication, PotentialEvaluation to National Knowledge, and preserve RegionalKnowledge. Chief scouts map StaffManagement to Communication. These mappings retain saved variation without rerolling staff. Existing capacity fields remain available for the current assignment framework.

`ProScoutDepartment.Ensure` supplies deterministic missing departments on new franchise creation and load; existing scouts/reports/targets are retained. Scouts are persisted separately from coaches. Edits write canonical coach/medical/chief-scout objects and mirror legacy fields for existing consumers. Annual College staff progression updates canonical objects as well.

Regression coverage: `StaffAttributeTests`, `ScoutDepartmentTests`, existing `ScoutingDepthTests`, `ScoutingDepartmentTests`, and `CoachAbilityImpactTests` cover labels, migration/roundtrip, unit-specific development, estimate quality, determinism, true-rating preservation and existing target limits.


## Team strategy and staff-generation sources

- College offense/defense identities: [Playbook Gamer College Football 27 playbooks](https://playbookgamer.com/college-football-27-playbooks/), retrieved 2026-11-01. All 138 packaged programs have a deterministic source entry. The raw source labels remain in `CollegeTeamStrategyCatalog`; enum mappings intentionally approximate the current engine's smaller playbook vocabulary.
- College staff ratings: generated from current program prestige with a dynasty, team and role seed. The mid-prestige target is about 65. Schemes do not reroll after dynasty creation.
- Pro identities and coach names: packaged official team-site coach snapshot. Scheme labels are best-effort 2026 coaching profiles. Staff quality is a curated identity index centered around 70, not a historical grade.
- Pro scouting quality: a per-club best-effort drafting index centered around 69, used as the scout attribute baseline. It is explicitly replaceable when a reviewed historical draft metric is added.
- Pro medical and scout names are generated. Their ratings use franchise, team and role seeds, so new saves vary while reloading remains stable.

These are simulation design inputs, not claims of exact current coaching expertise. Schemes describe intended identity; staff-quality values create initial differentiation that subsequently evolves through the existing staff, player and simulation systems.


## Staff lifecycle rules

College coaches and medical staff retain their existing age-based growth/decline and retirement flow. Team performance now adds a small upward/downward chance, and College scouts age annually with growth, decline, retirement and replacement. Graduated players with strong leadership/awareness can replace retiring College coaches when the role fits.

Pro coaches, medical staff and scouts age once each offseason. Attributes use independent deterministic rolls around growth potential, age, performance and decline. User-controlled coaching roles do not retire automatically. Retiring Pro staff are recorded, vacancies pass through existing hiring, and new scouts/medical staff are generated when departments are below strength. Retired Pro players who meet the leadership threshold are added to the coaching candidate pool with a retired-player source label.

All random rolls are stable for a franchise, season, staff member and attribute, so reloading does not reroll careers. Legacy records receive a first age from stable staff/franchise data, then advance normally.


## Performance-history and academic sources

Pro coach calibration uses `nflverse/nfldata` `games.csv`, regular seasons 2023–2025, retrieved 2026-11-01: https://github.com/nflverse/nfldata/blob/master/data/games.csv. Offense and defense are weighted scoring percentiles with season weights 20% / 30% / 50%. Head-coach samples retain historical club tenures separately from the coach's current employer. Coordinator unit history is a club-level estimate because nflverse game rows name head coaches only. The profile labels whether the basis is observed or estimated.

The game file also assigns a club's whole season to its opening head coach in some midseason changes. Robert Saleh is therefore capped at the 22 Jets games he coached before the 2024 dismissal, matching the NFL transaction record: https://www.nfl.com/news/jets-fire-robert-saleh-grateful-for-time-as-jets-coach-in-first-comments-since-firing. Any future manual corrections should be attached to the affected dated tenure rather than relabeling the coach's current club.

College academic quality is an estimated institutional index, not an official NCAA metric. Highly selective schools use explicit values; other schools use a prestige-informed fallback. It remains static through season progression to avoid pretending institutional academics change annually. Prestige and academics both use 0–99 scales. Academic pitch importance is boosted for a deterministic subset of recruits so education is a meaningful but selective preference.

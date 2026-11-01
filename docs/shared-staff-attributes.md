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

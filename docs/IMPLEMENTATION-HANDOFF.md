# Implementation handoff

Current planning source: [ROADMAP.md](ROADMAP.md). The execution outline is
[roadmap-completion-plan.md](roadmap-completion-plan.md). Historical release, APK and test evidence remains in
`current-work.md`, `testing-scope.md`, `pro-completion-checklist.md` and the matching audit documents.

## Where the work stands

The resumed roadmap branch now includes the durable-state and identity completion pass: persisted team
motivation, deterministic Pro scouting replenishment, CPU club coach season history, collision-resistant Pro
coach identity, conference record accounting and Pro player identity on injury, result, award and all-star
stories. Shared narrative memory already carries stable person threads across seasons.

Focused validation for this pass is green: 63 Pro regressions, 18 Engine regressions, 24 College regressions and
both Android compile targets. `ScheduledGame.WinnerTeamName` also now returns null for a tie after the coach-record test exposed the
previous away-team fallback.

## What to pick up next

1. Add the player-authored social reply action and explicit action-level smoke assertions for social state,
   rivalry series, chemistry/motivation and replay highlights.
2. Rework the pre-snap rule boundary around sourced NCAA/NFL requirements. Keep the repository's numeric
   shift/motion limits labeled as internal simulation bounds and keep rivalry records simulation-neutral.
3. Run the final broad Engine, College and Pro checks, package and verify both exact Debug APKs, and use the
   existing College and Pro workflows for CI/device evidence.
4. Merge to `main`, publish the authorized prerelease with both APKs and checksums, clean generated files and
   delete only the completed feature branches locally and remotely.

The long-term agenda remains in `ROADMAP.md`: College NIL, College media rights and finance, Pro finance
expansion, expansion and relocation, stadium construction and training facilities, and additional offseason
activities.

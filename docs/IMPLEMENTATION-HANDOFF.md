# Implementation handoff

Current planning source: [ROADMAP.md](ROADMAP.md). The execution outline is
[roadmap-completion-plan.md](roadmap-completion-plan.md). Historical release, APK and test evidence remains in
`current-work.md`, `testing-scope.md`, `pro-completion-checklist.md` and the matching audit documents.

## Where the work stands

All near-term implementation work is complete on `codex/feature-roadmap-plan`: durable motivation and scout
replenishment, CPU coach season history with stable identities, Pro person-level story threads, save-backed
player-authored social replies, and sourced NCAA/NFL pre-snap alignment boundaries. The existing one-yard shift
and three-yard motion values are internal catalog/animation bounds, not league limits. Rivalry records remain
simulation-neutral.

Focused validation through Batch G is green: 26 Engine, 7 College and 7 Pro social/pre-snap tests, plus both
Android compile targets. Earlier state/identity checks passed 63 Pro, 18 Engine and 24 College tests.

## What to pick up next

1. Add explicit action-level smoke assertions for the player-authored social reply and like/repost state,
   rivalry series, chemistry/motivation and replay highlights on fresh College and Pro APKs.
2. Run the final broad Engine, College and Pro checks, package and verify both exact Debug APKs, and use the
   existing College and Pro workflows for CI/device evidence.
3. Merge to `main`, publish the authorized prerelease with both APKs and checksums, clean generated files and
   delete only the completed feature branches locally and remotely.

The long-term agenda remains in `ROADMAP.md`: College NIL, College media rights and finance, Pro finance
expansion, expansion and relocation, stadium construction and training facilities, and additional offseason
activities.

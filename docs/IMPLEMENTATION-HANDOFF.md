# Implementation handoff

Current planning source: [ROADMAP.md](ROADMAP.md). It holds the near-term status, what is left in the near
term, and the long-term agenda. The execution outline for the delivered near-term batches is
[roadmap-completion-plan.md](roadmap-completion-plan.md). Historical releases, APK evidence and completed
checks remain in `current-work.md`, `testing-scope.md`, `pro-completion-checklist.md` and the matching
release and audit documents.

## Where the work stands

The GitHub Actions College and Pro device jobs pass end to end on `7c6b377`; the run and artifact details are in
`testing-scope.md`. All five near-term batches are delivered on `codex/feature-roadmap-plan` and pushed: season-aware chemistry
and locker-room events, replay highlights and pre-snap choreography, the persistent coach hall of fame, the
social interaction layer, and the persisted rivalry set with series exposure. The batch commits, their
acceptance criteria and their validation commands are in `roadmap-completion-plan.md`.

## What to pick up next

The near-term items that remain, in the order `ROADMAP.md` lists them:

1. Explicit action-level device acceptance for the new surfaces. The GitHub Actions device jobs pass end to end
   (`7c6b377`) and cover the coach Hall of Fame and the screens where the new panels render; the remaining gap is
   asserting the new actions themselves — a social like/repost/reply, a rivalry series panel, a chemistry note and
   the replay highlight picker.
2. Coach lifecycle parity for CPU club coaches, which needs a recorded CPU coach season history.
3. Player and coach level narrative memory, which needs a stable person identity across seasons.
4. Motivation state behind the locker-room events.
5. Sourced league rules for the two open limits: a rivalry effect on a play, and a league-specific pre-snap
   limit. Neither should be invented.
6. The intermittent Pro staff-balance scout-count test, which predates this work and needs a deterministic
   staff fixture or a replenished scout department.

After those, the long-term agenda in `ROADMAP.md` is the next body of work: the College NIL system, College
media rights and finance, Pro finance expansion, expansion and relocation, stadium construction and training
facilities, and additional offseason activities. Each has a first slice scoped there, and each follows the
same conventions: shared engine and presentation by default, league adapters where the rules differ,
save-compatible records that default safely, and no invented facts.

No release, merge or publication is authorized by this handoff.
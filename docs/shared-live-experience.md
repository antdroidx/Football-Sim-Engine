# Shared live-game experience

College and Pro live games now use `GameCastLivePage` in `Football.Presentation`. Interface changes belong there, not in league applications. `GameDayLiveControl` in `Football.Engine` owns unit permissions, You/CPU delegation, pause/resume, menu holds, duplicate-snap guards and filtering of user calls. A future game supplies the same host contract and automatically uses the same interface.

## Host boundary

`GameCastLiveHost` supplies team abbreviations/colors/field names and optional logos, a final-result save callback, the league's box-score adapter, and whether takeover requires choosing a team. The `GameDaySession` supplies rosters, plans, career role and `FootballRules`. The common UI reads league-specific rules for hashes, overtime, conversions and kicking; it does not reference Pro Core or College Data.

```csharp
await Navigation.PushModalAsync(new GameCastLivePage(session, new GameCastLiveHost
{
    Brand = name => new GameCastLiveBrand(abbreviation(name), primary(name), secondary(name)),
    Save = SaveCompletedSession,
    ShowBoxScore = (navigation, box) => GameCastBoxScore.ShowAsync(navigation, box, boxStyle),
    ChooseTeamOnTakeover = neutralSpectator
}));
```

Prepare the session and coin toss before opening the page. The save callback is invoked only after completion, through `GameDayCompletionService`; it must remain retryable. Normal app navigation, package IDs, saved-season formats and league management remain in their respective apps. College's archived highlight-show entry is retained separately and cannot be called for a live game.

## Screen behavior

- Toolbar: Pause/Resume stays reachable during CPU snaps. Team, Options and conditional Take over are available at decisions or while paused; landscape playback keeps the footer compact. A pause request completes the current play and blocks the next one.
- Persistent state: controlled team, Offense You/CPU, Defense You/CPU, role locks, and Running/Paused/Waiting/Play in progress. Pause and delegation are independent.
- Portrait: a full-width field anchored beneath the scoreboard. The space below switches between a call sheet and recent-play recap. Landscape: a compact combined header, with a side call sheet at manual decisions. During action/watching the side panel closes and the field fills the available body; play-by-play and Pause sit beneath it. The call sheet returns at the next manual decision. Landscape is the primary layout; automatic rotation and portrait remain supported.
- Play cards select only. The fixed Call play/Call defense/Attempt conversion button executes one snap. The sheet fades in at the next user decision; it does not appear between automated snaps. Scrollable cards cannot push its action buttons off-screen.
- Playbook includes legal punt, field-goal, kneel, spike, extra-point and two-point calls. College overtime restrictions remain distinct from professional timed overtime.
- Options: Play calling, Box score, Replay last play, Viewing options, Weather, Finish game, Exit game. Viewing options expose the Broadcast/FollowBall/ReverseFollow replay angle; replays remain deterministic and never reroll simulation. Team: substitutions, coach talk with availability, second unit/starters, automatic rotation, next-kickoff strategy and timeout.
- Each unit has explicit You/CPU choices. Watch both sides delegates both units; Take over restores the units the career role permits. Menu labels reflect effective control, not unused state flags. Both unit entries are always shown; selecting a locked one explains the current career restriction.
- Menus and replay suspend advancement, await the current snap and preserve the selected play and previous pause state. Finish/Exit ask for confirmation. Save failure exposes Retry save.

## Validation

Core cases in `GameDayLiveControlTests` cover all four ownership combinations, real CPU call sources after delegation, nested menu holds, pause during a busy play, reclaiming a unit, coordinator locks, kickoff/conversion automation, both leagues' conversion rules, completion guards and league-correct touchback commentary.

The same `build/shared-live-smoke.py` is used for every host. Enter a live game in a disposable emulator, put adb on PATH, and run:

```powershell
python build/shared-live-smoke.py --label college --serial emulator-5556
python build/shared-live-smoke.py --label pro --serial emulator-5556
```

Run each command with its corresponding app foregrounded. The script checks selection/clock preservation through menus, playbook return, substitutions and second unit, box score return, Finish/Exit cancellation, snap-to-sheet transition, replay invariants, watch/pause, all four ownership combinations and portrait/landscape tap-target bounds. It writes XML, screenshots, a check summary and logcat to the current directory. Use the newly built APKs; earlier live scripts expect retired app-specific labels and do not certify this interface.

No release or merge is part of this implementation. Exact verification results and artifact identity are recorded in `current-work.md` and `testing-scope.md` after checks complete.

Focused field-first/role-lock check: `python build/shared-live-field-smoke.py --label college --serial emulator-5556 --role head` (or `--role offense` for the coordinator lock explanation). Start in an unfinished live game.

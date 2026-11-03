# Roster identity plus resource scoring

Builds on the existing `DepthChartService` scheme fit and adds:
- identity-derived positional priorities for Pro and College;
- identity-aware CPU draft/free agency/waiver/scouting/trade scoring;
- resource-aware pro acquisition scoring with priority plus cap efficiency;
- college roster-shape scoring using identity-driven targets, class-year upside and depth.

The same identity model powers both games. Pro decisions keep existing salary-cap, roster-limit and coordinator-authority safeguards. College decisions keep the current roster/scholarship limit and recruiting points, but now use scheme-derived roster targets and positional priority.

## Current behavior
- Pro positional priorities blend scheme, depth targets and positional staff expertise.
- Pro acquisition score blends roster priority and cap efficiency.
- Pro CPU draft choices use cap-aware acquisition scoring, not raw OVR.
- Pro free agency and waiver claims rank by acquisition score rather than raw OVR.
- Pro trade value blends standard value, cap surplus, positional need and identity fit.
- Pro retention processes higher-priority players first.
- Pro cuts and trade markets rank interested teams by priority and value.
- recruiting target counts reflect the team's offensive/defensive identity.

## Staff UI
- Front Office > Staff now shows a compact priority strip for the team's highest positional weights.

## Next work
- staff-adapted philosophy changes;
- pro depth/extension resource accounting;
- long-term roster audits.


# Roster identity plus resource scoring

Builds on the existing `DepthChartService` scheme fit and adds:
- identity-derived positional priorities for Pro and College;
- identity-aware CPU draft/free agency/waiver/scouting/trade scoring;
- resource-aware pro acquisition scoring with priority plus cap efficiency;
- college roster-shape scoring using identity-driven targets, class-year upside and depth.

The same identity model powers both games. Pro decisions keep existing salary-cap, roster-limit and coordinator-authority safeguards. College decisions keep the current roster/scholarship limit and recruiting points, but now use scheme-derived roster targets and positional priority.

## Current behavior
- Pro acquisition score blends roster priority and cap efficiency.
- Pro trade value blends standard value, cap surplus, positional need and identity fit.
- waiver claims rank by acquisition score rather than raw OVR.
- draft picks and free agencies use fit/need-weighted priority.
- trade markets rank interested teams by priority and value.
- recruiting target counts reflect the team's offensive/defensive identity.

## Next work
- deeper cap/roster planning for extensions, cuts and trades;
- staff-adapted philosophy changes and UI display;
- long-term roster audits.

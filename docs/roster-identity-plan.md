# Roster identity plus resource scoring

Builds on the existing `DepthChartService` scheme fit and adds:
- identity-derived positional priorities for Pro and College;
- personnel-adapted offense/defense identity without changing the saved team plan;
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

- Personnel adaptation
- A heavily pass-shaped roster leans an existing run-first base toward Spread.
- A run/OL-heavy roster leans existing spread/air-raid bases toward Power Run.
- A coverage-heavy roster leans existing front-heavy bases toward Nickel.
- An edge-heavy roster leans existing front/Nickel bases toward Three-Four.

## Staff UI
- Front Office > Staff now shows a compact priority strip for the team's highest positional weights.

## Staff philosophy adaptation
- Coach scheme labels map deterministically to engine playbooks; ambiguous or unknown labels carry no influence.
- A head coach whose personal philosophy differs from the club baseline adapts positional priorities and depth
  targets by an influence weight that grows with tenure and leadership (capped so saved game plans stay authoritative).
- Season-zero staff and coordinators keep the club baseline, so existing identities are unchanged until a hire differs.
- Newly hired head coaches carry a stable personal philosophy derived from their name, franchise and stronger skills;
  the club baseline is no longer automatically adopted. User-set game plans are never rewritten.
- Identity blending affects CPU draft, free agency, waiver, scouting, retention and trade ordering through the
  existing ProRosterPlanService scores; cap/roster/rule validation is unchanged.

## Next work
- pro depth/extension resource accounting;
- long-term roster audits;
- staff-adapted CPU game-plan drift over seasons (saved plans only change through the user or a future CPU phase).



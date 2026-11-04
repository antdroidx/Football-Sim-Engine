# Pro165 / College454 — roster identity, CPU plan drift, Flexbone academies

Roster identity: Pro and College use one identity model - playbook-driven positional targets and
priority, personnel-adapted identity, and staff-philosophy influence - that feeds CPU draft, free
agency, waivers, scouting, retention, trades and depth autoassignment. Pro acquisition scoring
blends roster priority with cap efficiency under existing salary-cap, roster-limit and
coordinator-authority safeguards; College scoring keeps roster/scholarship limits and recruiting
points.

Long-term validation: three-season real/generated audits across both games (8,832 games) show
stable identity-target adherence, zero personnel violations and balance within run-to-run noise.
See docs/roster-identity-long-term-audit.md and the per-mode JSON reports.

CPU game-plan drift: established head coaches now drift their CPU team's saved offense/defense one
adjacent playbook step per offseason toward their mapped scheme (Pro uses coach labels plus the
club baseline; College uses the head coach's numeric stronger side). User plans, custom playbooks
and weak-influence coaches never drift; the shared influence gate is .10 (about two seasons of
average tenure). The drift-enabled re-audit confirmed bounded drift with stable scoring.

Flexbone academies: Air Force, Army and Navy now run the dedicated Flexbone playbook with flexbone
formations instead of the generic Option playbook, matching real triple-option football.

Playbook/scheme coverage audit: documented the mapping of both catalogs against real football
scheme families in docs/playbook-scheme-coverage-audit.md.

Builds and checks: Pro165 (run 41798956069, validation scope full) and College454 (run 41798956072,
management scope full) both pass their packaging checks; the combined Core/College suites, engine
Fast/Balance tiers and Pro test scopes pass. Exact APK SHA-256 values are in SHA256SUMS.txt and in
the manifest JSON attached to this release.

Limits: this batch claims no new device-played GameCast flows; long-horizon Pro/College device
audits keep their previous scope, and legacy HS-era scheme families (Wing-T, double-wing,
wildcat) remain intentionally outside the catalogs.

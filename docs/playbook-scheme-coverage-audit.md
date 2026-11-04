# Playbook and scheme coverage audit — November 4, 2026

Question: do the two games' playbooks and schemes cover the different types of offense/defense
schemes found in football? Method: enumerate the engine enums, both team strategy catalogs
(32 NFL franchises, 138 FBS programs) and the 110-play catalog, then map every real-world scheme
family onto them. Source scheme labels are retained in both catalogs, so mapping decisions stay
inspectable (ProTeamStrategyCatalog.OffenseIdentity/DefenseIdentity,
CollegeTeamStrategyCatalog.SourceOffense/SourceDefense).

## Engine inventory

- Offensive playbooks (both games): Balanced, PlayAction, Spread, AirRaid, PowerRun, Option,
  Flexbone, Custom. Formations: Shotgun Spread, Pistol, Singleback, I-Form, Empty, Flexbone.
  Concepts: Inside Run, Outside Run, Quick Pass, Play Action, Deep Pass (80 named plays; RPOs
  exist as Quick/Run concepts, e.g. RPO Alert Bubble, RPO Read Counter).
- Defensive playbooks (both games): Balanced, FourThree, ThreeFour, Nickel, Custom. Formations:
  4-3, 3-4, Nickel, Dime, Goal-Line (30 named plays across four approaches: Balanced 7,
  StopRun 6, ProtectPass 9, Pressure 8).
- The play editor exposes Custom playbooks (up to 17 offensive / 14 defensive plays) for anything
  the curated catalogs omit.

## Offense coverage vs real football

| Real scheme family | Engine playbook | Coverage |
|---|---|---|
| Multiple / balanced pro (Erhardt-Perkins, Shanahan-style) | Balanced | Covered (Pro labels: Erhardt-Perkins multiple, multiple play action, physical play action) |
| West Coast timing / play-action pro | PlayAction | Covered (West Coast play action, timing, motion labels) |
| Spread / RPO era offense | Spread | Covered (Spread RPO, Spread motion labels) |
| Air raid (Leach, run-and-shoot lineage) | AirRaid | Covered (College: Air Raid 11, Run & Shoot 2; no pro team, accurate to the NFL) |
| Power/gap run | PowerRun | Covered (Pro power run play action × 3, zone run play action; College Power Spread 26) |
| Wide-zone play action | PlayAction | Covered (Pro wide-zone play action × 2) |
| Veer and shoot | Option | Mapped loosely (12 College teams); source labels retained for future refinement |
| Veer / midline option | Option | Covered (Spread Option 5, Option 3) |
| Triple option (flexbone: Air Force, Navy, Army) | Flexbone | Covered (academies run the dedicated Flexbone playbook with flexbone formations since November 4) |
| Pistol | Formation, inside Spread playbook | Covered (4 College "Pistol" teams run Spread with Pistol formations) |
| Wing-T, double wing, single wing, wildcat | none | Not covered; nothing at current FBS/NFL scope uses them and Custom plays can approximate them |
| Vertical "Air Coryell" passing identity | PlayAction/DeepPass concepts | Partial: deep passing exists as a concept, no dedicated identity label |

## Defense coverage vs real football

| Real scheme family | Engine playbook/formation | Coverage |
|---|---|---|
| 4-3 even fronts (over/under), zone/man/quarters | FourThree | Covered (Cover 3 Buzz, Cover 1 Robber, Tampa 2, Under Front, Quarters labels) |
| 3-4 odd/even fronts | ThreeFour | Covered (Cross Dog, Cloud 3, Quarters, Cover 2 Sink) |
| Bear / 46 front | ThreeFour | Covered (Bear Front stop-run play) |
| Nickel 4-2-5 | Nickel | Covered (College 4-2-5 labels: 47 teams mapped to Nickel) |
| 3-3-5 | Nickel | Covered (College 3-3-5 labels: 34 teams mapped to Nickel) |
| 3-2-6 / dime / dollar packages | Dime formation inside Nickel playbook | Covered (Dime plays incl. Drop Eight, Two-Man, Simulated Pressure; College 3-2-6 label retained) |
| Goal-line / short yardage | GoalLine | Covered (six plays, four approaches) |
| Mug / pressure looks | Nickel, Dime | Covered (Mug Double A, Double A-Gap, Simulated Pressure, Sam Fire Zone) |
| Hybrid multiple fronts (3-4 hybrid, 4-3 hybrid) | catalog picks nearest base | Mapped with a judgment call; source labels retained |
| Cover-0 / all-out blitzes | Pressure approach + pressure teams | Approximated by approach, no dedicated all-out play |
| Defensive-line technique nuance (0/1/2i/4i, 1-gap vs 2-gap) | approach tendencies | Intentionally not modeled |

## Season-zero distribution evidence

- Pro offense: Balanced 4, PlayAction 14, Spread 9, PowerRun 5; no pro team uses AirRaid,
  Option or Flexbone - accurate for the NFL. Pro defense: 4-3 12, 3-4 13, Nickel 7.
- College offense: Balanced 22, Spread 57, AirRaid 13, PowerRun 26, Option 17, Flexbone 3 (from
  134 catalog teams plus generated worlds); College defense: 4-3 25, 3-4 28, Nickel 81, Balanced 10. The
  Nickel playbook absorbs 4-2-5 and 3-3-5 teams as intended.
- Every one of the 138 college and 32 pro team labels maps to a defined playbook enum; none fall
  through to Custom. Drift adjacency (added November 3) can reach every playbook from every other
  playbook, and custom playbooks are never drifted.

## Verdict

The curated catalogs cover every mainstream modern offense and defense family at NFL/FBS scope,
with named coverage/blitz concepts for scheme flavor. Real gaps are the scheme families that no
current FBS/NFL program runs (Wing-T/double-wing/wildcat) and a few judgment-call mappings
(Veer & Shoot -> Option, hybrid fronts -> nearest base) that stay documented through retained
source labels. The dedicated Flexbone playbook is used by the three service academies
since November 4.

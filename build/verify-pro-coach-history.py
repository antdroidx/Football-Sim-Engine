"""Verify bundled coach history against a downloaded nflverse games.csv.

Run from the repository root. This command never downloads data. --update-catalog
regenerates only the two static tables; ordinary runs fail on any discrepancy.
"""
import argparse
import collections
import csv
import hashlib
import json
import math
import re
from pathlib import Path

CATALOG = Path("src/ProFootball.Core/ProCoachHistoryCatalog.cs")
SNAPSHOT = Path("src/ProFootball.Core/nfl-snapshot.json")
SEASONS = (2023, 2024, 2025)
WEIGHTS = {2023: .2, 2024: .3, 2025: .5}
TEAM_PATTERN = re.compile(r'new\("([A-Z]+)", ([\d.]+), ([\d.]+), ([\d.]+)\)')
COACH_PATTERN = re.compile(
    r'new\("([^"\n]+)", ([\d.]+), ([\d.]+),\s*'
    r'(new\("[A-Z]+".*?\)\)),', re.S)
TENURE_PATTERN = re.compile(
    r'new(?: ProCoachTeamHistory)?\("([A-Z]+)", (\d+), (\d+), (\d+), '
    r'([\d.]+), ([\d.]+), (\d+), (\d+)\)')


def percentile(value, pool, reverse=False):
    """20-80 rank scale; equal results share their average ordinal rank."""
    below = sum(x > value if reverse else x < value for x in pool)
    equal = sum(x == value for x in pool)
    rank = below + max(0, equal - 1) / 2
    return 20 + 60 * min(len(pool) - 1, rank) / (len(pool) - 1)


def stats(games):
    n = len(games)
    return dict(Games=n, Wins=sum(pf > pa for pf, pa in games),
                Losses=sum(pf < pa for pf, pa in games),
                PointsForPerGame=round(sum(pf for pf, pa in games) / n, 1),
                PointsAgainstPerGame=round(sum(pa for pf, pa in games) / n, 1))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--games", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--update-catalog", action="store_true")
    parser.add_argument("--runtime-audit", type=Path,
                        help="Also compare every game's initial coach rating from staff-sensitivity")
    args = parser.parse_args()
    source = CATALOG.read_text(encoding="utf-8")
    snapshot = json.loads(SNAPSHOT.read_text(encoding="utf-8"))
    with args.games.open(encoding="utf-8", newline="") as handle:
        rows = [row for row in csv.DictReader(handle)
                if row["game_type"] == "REG" and int(row["season"]) in SEASONS]
    if len(rows) != 816 or len({r["game_id"] for r in rows}) != 816:
        raise ValueError("Expected all 816 unique 2023-2025 regular-season games")
    teams = collections.defaultdict(list)
    coaches = collections.defaultdict(list)
    for row in rows:
        season = int(row["season"])
        for side, other in (("home", "away"), ("away", "home")):
            result = int(row[side + "_score"]), int(row[other + "_score"])
            team, coach = row[side + "_team"], row[side + "_coach"]
            teams[season, team].append(result)
            # Primary NFL dismissal record: Saleh coached only weeks 1-5 in 2024.
            if coach == "Robert Saleh" and season == 2024 and int(row["week"]) > 5:
                coach = "Jeff Ulbrich"
            coaches[coach, team, season].append(result)
    if len(teams) != 96 or any(len(g) != 17 for g in teams.values()):
        raise ValueError("Expected 32 complete 17-game team records per season")

    def unit(season, games, index):
        mean = sum(g[index] for g in games) / len(games)
        pool = [sum(g[index] for g in results) / len(results)
                for (year, _), results in teams.items() if year == season]
        return percentile(mean, pool, reverse=index == 1)

    expected_teams = {}
    for team in sorted({team for _, team in teams}):
        units = [round(sum(WEIGHTS[s] * unit(s, teams[s, team], i)
                           for s in SEASONS), 1) for i in (0, 1)]
        wins = sum(WEIGHTS[s] * percentile(
            sum((pf > pa) + .5 * (pf == pa) for pf, pa in teams[s, team]),
            [sum((pf > pa) + .5 * (pf == pa) for pf, pa in results)
             for (year, _), results in teams.items() if year == s]) for s in SEASONS)
        expected_teams[team] = [*units, round(wins, 1)]

    heads = {c["Name"]: c["TeamId"] for c in snapshot["Coaches"] if c["Role"] == "HeadCoach"}
    expected_coaches = {}
    for name, current in heads.items():
        samples = [(team, season, games) for (coach, team, season), games in coaches.items()
                   if coach == name]
        if not samples:
            continue
        count = sum(len(g) for _, _, g in samples)
        units = [round(sum(unit(s, g, i) * len(g) for _, s, g in samples) / count, 1)
                 for i in (0, 1)]
        tenures = []
        for team in sorted({team for team, _, _ in samples},
                           key=lambda t: min(s for club, s, _ in samples if club == t)):
            club_samples = [(s, g) for club, s, g in samples if club == team]
            record = stats([game for _, games in club_samples for game in games])
            tenures.append(dict(TeamId=team, **record,
                                FirstSeason=min(s for s, _ in club_samples),
                                LastSeason=max(s for s, _ in club_samples)))
        expected_coaches[name] = dict(CurrentTeam=current, Units=units, Tenures=tenures)

    team_section, coach_section = source.split("TeamRows =", 1)[1].split("CoachRows =", 1)
    actual_teams = {m[0]: [float(v) for v in m[1:]] for m in TEAM_PATTERN.findall(team_section)}
    actual_coaches = {}
    for name, offense, defense, raw in COACH_PATTERN.findall(coach_section):
        tenures = []
        for team, games, wins, losses, pf, pa, first, last in TENURE_PATTERN.findall(raw):
            tenures.append(dict(TeamId=team, Games=int(games), Wins=int(wins), Losses=int(losses),
                                PointsForPerGame=float(pf), PointsAgainstPerGame=float(pa),
                                FirstSeason=int(first), LastSeason=int(last)))
        actual_coaches[name] = dict(Units=[float(offense), float(defense)], Tenures=tenures)
    differences = []
    for key in sorted(expected_teams.keys() | actual_teams.keys()):
        if expected_teams.get(key) != actual_teams.get(key):
            differences.append(dict(Team=key, Actual=actual_teams.get(key), Expected=expected_teams.get(key)))
    for key in sorted(expected_coaches.keys() | actual_coaches.keys()):
        expected = {k: v for k, v in expected_coaches.get(key, {}).items() if k != "CurrentTeam"}
        if expected != actual_coaches.get(key):
            differences.append(dict(Coach=key, Actual=actual_coaches.get(key), Expected=expected))

    if args.update_catalog:
        if set(actual_teams) != set(expected_teams) or set(actual_coaches) != set(expected_coaches):
            raise ValueError("Catalog membership differs; review before updating")
        def team_row(match):
            return 'new("' + match[1] + '", ' + ', '.join(f'{v:.1f}' for v in expected_teams[match[1]]) + ')'
        def coach_row(match):
            data = expected_coaches[match[1]]
            lines = []
            for i, t in enumerate(data["Tenures"]):
                constructor = "new" if i == 0 else "new ProCoachTeamHistory"
                lines.append(f'{constructor}("{t["TeamId"]}", {t["Games"]}, {t["Wins"]}, {t["Losses"]}, '
                             f'{t["PointsForPerGame"]:.1f}, {t["PointsAgainstPerGame"]:.1f}, '
                             f'{t["FirstSeason"]}, {t["LastSeason"]})')
            return f'new("{match[1]}", {data["Units"][0]:.1f}, {data["Units"][1]:.1f}, ' + ', '.join(lines) + '),'
        head, rest = source.split("TeamRows =", 1)
        team_part, coach_part = rest.split("CoachRows =", 1)
        source = head + "TeamRows =" + TEAM_PATTERN.sub(team_row, team_part) + "CoachRows =" + COACH_PATTERN.sub(coach_row, coach_part)
        CATALOG.write_text(source, encoding="utf-8")
    runtime_count = 0
    if args.runtime_audit:
        runtime = json.loads(args.runtime_audit.read_text(encoding="utf-8"))["InitialCoaching"]
        bundled = {(c["TeamId"], c["Role"], c["Name"]) for c in snapshot["Coaches"] if not c.get("Vacant")}
        if len(runtime) != len(bundled) or {(c["TeamId"], c["Role"], c["Name"]) for c in runtime} != bundled:
            raise ValueError("Runtime staff does not match the bundled staff identities")
        for coach in runtime:
            team = "LA" if coach["TeamId"] == "LAR" else coach["TeamId"]
            offense, defense, wins = expected_teams[team]
            role, name = coach["Role"], coach["Name"]
            if role == "HeadCoach" and name in expected_coaches:
                sample = expected_coaches[name]
                offense, defense = sample["Units"]
                games = sum(t["Games"] for t in sample["Tenures"])
                wins = sum(t["Wins"] for t in sample["Tenures"])
                ties = sum(t["Games"] - t["Wins"] - t["Losses"] for t in sample["Tenures"])
                raw = 68 + .25 * (offense - 50) + .30 * (defense - 50) + .20 * 60 * ((wins + .5 * ties) / games - .5)
            else:
                weights = {"OffensiveCoordinator": (.50, .15, .05),
                           "DefensiveCoordinator": (.15, .50, .05),
                           "SpecialTeamsCoordinator": (.10, .10, .35)}.get(role, (.30, .30, .15))
                raw = 68 + sum(w * (v - 50) for w, v in zip(weights, (offense, defense, wins)))
            expected = max(48, min(90, math.floor(raw + .5)))
            if coach["HistoryRating"] != expected:
                differences.append(dict(RuntimeCoach=name, Actual=coach["HistoryRating"], Expected=expected))
            runtime_count += 1
    report = dict(Source="https://github.com/nflverse/nfldata/blob/master/data/games.csv",
                  SourceSha256=hashlib.sha256(args.games.read_bytes()).hexdigest(),
                  CatalogSha256=hashlib.sha256(CATALOG.read_bytes()).hexdigest(),
                  RegularSeasonGames=len(rows), TeamRows=len(expected_teams),
                  ObservedHeadCoaches=len(expected_coaches),
                  RuntimeRatingsChecked=runtime_count,
                  HeadCoachesWithoutObservedSample={n: t for n, t in heads.items() if n not in expected_coaches},
                  CoordinatorLimit="Club proxies; this dataset does not identify coordinator tenures.",
                  Method="20-80 season scoring percentile with average tied ranks; club weights .2/.3/.5; coach weights by games actually coached; ties count as half a win.",
                  Updated=args.update_catalog, Differences=differences, Coaches=expected_coaches)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({**{k: v for k, v in report.items() if k not in ("Coaches", "Differences")},
                      "DifferenceCount": len(differences)}, indent=2))
    if differences and not args.update_catalog:
        raise SystemExit(1)


if __name__ == "__main__":
    main()

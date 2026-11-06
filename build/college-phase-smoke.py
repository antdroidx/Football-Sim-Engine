"""Real College creation, preseason phase boundary and interruptible regular-season skip."""
import importlib.util,json,sqlite3,time
from pathlib import Path
spec=importlib.util.spec_from_file_location("shared",Path(__file__).with_name("android-ui-smoke.py"));s=importlib.util.module_from_spec(spec);spec.loader.exec_module(s)
package="antdroid.cfbcoach.dev"
def click(name,label):
    root=s.scroll_to_text("college-phase-"+name,label,max_swipes=20,toward_start=True);s.tap(root,label)
def ready(name,predicate,timeout=300):return s.wait_for("college-phase-"+name,predicate,timeout=timeout)
def has_id(root,suffix):
    return any((n.get("resource-id") or "").endswith(suffix) for n in root.iter("node"))
def roadmap_actions():
    click("roadmap-overview","Overview")
    ready("roadmap-overview-ready",s.overview_ready)
    click("roadmap-news","News & headlines ›")
    news=ready("roadmap-news",lambda r:has_id(r,"college-news-social") and (any((t or "").startswith("Read story") for t in s.texts(r)) or has_id(r,"college-social-like")))
    s.tap_id(news,"college-news-social")
    social_actions()
    click("roadmap-roster","Roster")
    roster=ready("roadmap-roster",lambda r:has_id(r,"college-team-profile"))
    s.tap_id(roster,"college-team-profile")
    ready("roadmap-team",lambda r:"Program identity" in s.texts(r))
    chemistry=s.scroll_to_text("roadmap-team-chemistry","Team chemistry",max_swipes=20)
    assert has_id(chemistry,"college-chemistry-note") and has_id(chemistry,"college-team-motivation")
    assert any((t or "").startswith("Motivation ") for t in s.texts(chemistry))
    rivalry=s.scroll_to_text("roadmap-team-rivalry","Rivalries",max_swipes=20)
    assert has_id(rivalry,"college-rivalry-series")
    assert any("meetings" in (t or "") or "meeting" in (t or "") for t in s.texts(rivalry))
    s.capture("college-phase-team-surfaces")
    close=s.scroll_to_text("roadmap-team-close","Close",max_swipes=20,toward_start=True)
    s.tap(close,"Close")

def social_actions():
    root=ready("social-actions",lambda r:all(has_id(r,suffix) for suffix in ("college-social-like","college-social-repost","college-social-reply","college-social-follow")))
    like_label="Like" if s.has_id_label(root,"college-social-like","Like") else "Liked"
    s.tap_id_label(root,"college-social-like",like_label)
    root=ready("social-liked",lambda r:s.has_id_label(r,"college-social-like","Like" if like_label=="Liked" else "Liked"))
    repost_label="Repost" if s.has_id_label(root,"college-social-repost","Repost") else "Reposted"
    s.tap_id_label(root,"college-social-repost",repost_label)
    root=ready("social-reposted",lambda r:s.has_id_label(r,"college-social-repost","Repost" if repost_label=="Reposted" else "Reposted"))
    reply_label="Reply" if s.has_id_label(root,"college-social-reply","Reply") else "Edit reply"
    s.tap_id_label(root,"college-social-reply",reply_label)
    prompt=ready("social-reply-prompt",lambda r:"Reply to post" in s.texts(r) and has_id(r,"game-dialog-input"))
    if reply_label=="Reply":s.input_text(prompt,"game-dialog-input","Roadmap device reply")
    root=ready("social-reply-submit",lambda r:"Post reply" in s.texts(r))
    s.tap(root,"Post reply")
    root=ready("social-replied",lambda r:"Your reply · Roadmap device reply" in s.texts(r) and s.has_id_label(r,"college-social-reply","Edit reply"))
    s.capture("college-phase-social-actions")
if "--actions-only" in __import__("sys").argv:
    try:
        roadmap_actions()
        Path("college-phase-check.json").write_text(json.dumps({"actionsOnly":True,"socialActions":True,"rivalrySeries":True,"chemistryMotivation":True},indent=2),encoding="utf-8")
        print("PASS: College social actions and team rivalry/chemistry/motivation surfaces.")
    finally:
        Path("college-phase-logcat.txt").write_bytes(s.adb("logcat","-d","-t","1000"))
    raise SystemExit(0)

try:
    ready("library",lambda r:"New dynasty" in s.texts(r) and "Your dynasties" in s.texts(r))
    click("new","New dynasty");click("picker","Choose your team");click("school","Air Force");click("start","Start dynasty")
    root=ready("overview",s.overview_ready);assert "Start season" in s.texts(root)
    s.tap(root,"Start season")
    root=ready("preseason-boundary",lambda r:s.overview_ready(r) and "Simulate regular season" in s.texts(r));s.capture("college-phase-preseason-boundary")
    s.tap(root,"Simulate regular season");click("confirm-regular","Simulate")
    root=ready("stop-control",lambda r:"Stop after this week" in s.texts(r));s.tap(root,"Stop after this week")
    root=ready("stopped",s.overview_ready,timeout=600);assert "Simulate regular season" in s.texts(root);s.capture("college-phase-stopped-regular")
    s.adb("shell","am","force-stop",package);db=Path("college-phase.db3");db.write_bytes(s.adb("exec-out","run-as",package,"cat","files/dynasties.db3"))
    conn=sqlite3.connect(db);row=conn.execute("select rowid,snapshot_json from dynasty_saves order by updated_utc desc limit 1").fetchone();assert row
    state=json.loads(row[1]);assert state["phase"]==1 and state["week"]<18
    state["rivalries"]=[{"id":"Air Force|Colorado State","teamAName":"Air Force","teamBName":"Colorado State","games":[
        {"seasonYear":state["seasonYear"]-2,"week":5,"homeTeamName":"Air Force","awayTeamName":"Colorado State","winnerTeamName":"Air Force","isNeutralSite":False,"bowlName":None},
        {"seasonYear":state["seasonYear"]-1,"week":11,"homeTeamName":"Colorado State","awayTeamName":"Air Force","winnerTeamName":"Air Force","isNeutralSite":False,"bowlName":None}]}]
    state["teamMotivation"]=[{"teamId":"55555555-5555-5555-5555-555555555555","teamName":"Air Force","seasonYear":state["seasonYear"],"value":73,"momentum":1,"drivers":["Synthetic device fixture"]}]
    conn.execute("update dynasty_saves set snapshot_json=? where rowid=?",(json.dumps(state),row[0]));conn.commit();conn.close()
    s.adb("push",str(db),"/data/local/tmp/college-phase.db3");s.adb("shell","run-as",package,"cp","/data/local/tmp/college-phase.db3","files/dynasties.db3")
    s.adb("shell","monkey","-p",package,"-c","android.intent.category.LAUNCHER","1");ready("saved-library",lambda r:"Load" in s.texts(r));click("load","Load");ready("reloaded",s.overview_ready);s.capture("college-phase-reloaded")
    roadmap_actions()
    Path("college-phase-check.json").write_text(json.dumps({"realCreatedDynasty":True,"startSeasonReachesRegularSeason":True,"stopAfterWeek":True,"weeklyAutosave":True,"reload":True,"savedWeek":state["week"],"socialActions":True,"rivalrySeries":True,"chemistryMotivation":True},indent=2),encoding="utf-8")
finally:Path("college-phase-logcat.txt").write_bytes(s.adb("logcat","-d","-t","1000"))

"""Focused field-first landscape and role-locked menu checks. Start an unfinished live game first."""
import argparse, importlib.util, json, os, re, time
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('--label',required=True)
parser.add_argument('--serial')
parser.add_argument('--role',choices=['head','offense','defense'],default='head')
args=parser.parse_args()
if args.serial:os.environ['ANDROID_SERIAL']=args.serial
spec=importlib.util.spec_from_file_location('ui',Path(__file__).with_name('android-ui-smoke.py'))
s=importlib.util.module_from_spec(spec);spec.loader.exec_module(s)
prefix='field-followup-'+args.label+'-'
checks=[]
def has(r,id):return any(n.get('resource-id','').endswith(id) for n in r.iter('node'))
def text(r,id):return next(n.get('text') for n in r.iter('node') if n.get('resource-id','').endswith(id))
def box(r,id):return tuple(map(int,re.findall(r'\d+',next(n.get('bounds') for n in r.iter('node') if n.get('resource-id','').endswith(id)))))
def wait(name,predicate):
    def safe(r):
        assert 'Game paused' not in s.texts(r),'Unexpected app exception'
        return predicate(r)
    return s.wait_for(prefix+name,safe,timeout=180)
def click(label):
    root=s.capture(prefix+'before-'+re.sub(r'\W+','-',label))
    if label not in s.texts(root):root=s.scroll_to_text(prefix+'scroll',label,max_swipes=10)
    node=next(n for n in root.iter('node') if n.get('text')==label)
    b=list(map(int,re.findall(r'\d+',node.get('bounds'))))
    if b[2]<=b[0] or b[3]<=b[1]:root=s.scroll_to_text(prefix+'scroll',label,max_swipes=10)
    s.tap(root,label)
def action(id):
    root=wait('before-'+id,lambda r:has(r,id));s.tap_id(root,id)
def menu():action('live-options');click('Play calling');return wait('caller-menu',lambda r:'Watch both sides' in s.texts(r))
def choose(unit,value):
    r=menu();click(next(t for t in s.texts(r) if (t or '').startswith(unit+':')));click(value)
def pause():
    r=wait('pause-ready',lambda r:has(r,'live-pause'))
    if text(r,'live-pause')=='Pause':s.tap_id(r,'live-pause')
    return wait('paused',lambda r:has(r,'live-state') and text(r,'live-state')=='Paused')
def check(name):checks.append(name);print(name+' passed',flush=True)
try:
    s.adb('shell','settings','put','system','accelerometer_rotation','0')
    s.adb('shell','settings','put','system','user_rotation','1');time.sleep(2)
    wait('entry',lambda r:has(r,'live-game-page'))
    pause();r=menu()
    assert any((t or '').startswith('Offense:') for t in s.texts(r))
    assert any((t or '').startswith('Defense:') for t in s.texts(r))
    check('both-caller-entries-visible')
    if args.role!='head':
        unit='Defense' if args.role=='offense' else 'Offense'
        locked=next(t for t in s.texts(r) if (t or '').startswith(unit+':'))
        assert 'role locked' in locked
        click(locked)
        reason=wait('role-explanation',lambda r:any('Your career role is' in (t or '') for t in s.texts(r)))
        assert any(('Offensive coordinator' if args.role=='offense' else 'Defensive coordinator') in (t or '') for t in s.texts(reason))
        click('Close');menu();click('Take over')
        r=wait('takeover-still-locked',lambda r:has(r,'live-ownership') and unit+': CPU' in text(r,'live-ownership'))
        assert 'role locked' in text(r,'live-ownership')
        check('career-lock-explained-and-preserved')
    else:
        assert 'Offense: You' in s.texts(r) and 'Defense: You' in s.texts(r)
        click('Take over');r=wait('between-plays',lambda r:has(r,'live-call-sheet'))
        manual=box(r,'live-field');s.capture(prefix+'between-plays')
        menu();click('Watch both sides');r=pause()
        wide=box(r,'live-field')
        assert wide[2]-wide[0] > (manual[2]-manual[0])*1.35,(manual,wide)
        page=box(r,'live-game-page')
        assert wide[3]-wide[1] >= (manual[3]-manual[1])*.95,(manual,wide)
        assert wide[3]-wide[1] >= (page[3]-page[1])*.35,(page,wide)
        assert (page[3]-box(r,'live-pause')[1]) < (page[3]-page[1])*.25,(page,box(r,'live-pause'))
        assert not has(r,'live-call-sheet') and not has(r,'live-recent-plays')
        assert has(r,'game-day-score') and box(r,'live-pause')[1]>=wide[3]
        s.capture(prefix+'full-field-paused');check('full-field-with-scorebug-and-pause-below')
        menu();click('Take over');r=wait('sheet-restored',lambda r:has(r,'live-call-sheet'))
        assert abs((box(r,'live-field')[2]-box(r,'live-field')[0])-(manual[2]-manual[0]))<8
        check('between-play-call-sheet-restored')
        choose('Offense','CPU')
        r=wait('defensive-call-sheet',lambda r:has(r,'live-call') and text(r,'live-call')=='Call defense')
        s.capture(prefix+'defensive-call-sheet');check('head-coach-defensive-playcalling')
        before=text(r,'live-situation');s.tap_id(r,'live-call')
        for frame in range(3):
            time.sleep(.3);Path(prefix+f'active-{frame}.png').write_bytes(s.adb('exec-out','screencap','-p'))
        wait('after-defense',lambda r:has(r,'live-call-sheet') and text(r,'live-situation')!=before)
        check('defensive-snap-and-call-sheet-return')
finally:
    Path(prefix+'result.json').write_text(json.dumps({'checks':checks},indent=2))
    Path(prefix+'logcat.txt').write_bytes(s.adb('logcat','-d'))
print(json.dumps({'passed':len(checks),'checks':checks},indent=2))

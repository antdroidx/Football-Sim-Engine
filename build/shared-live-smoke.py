"""Common live-screen acceptance for every engine host.

Start any unfinished live game on a disposable emulator, then run this script.
Example: python build/shared-live-smoke.py --label pro --serial emulator-5556
Uses the same actions/assertions for College, Pro and future host applications.
"""
import argparse
import importlib.util
import json
import os
import re
import time
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--label', required=True)
parser.add_argument('--serial')
args = parser.parse_args()
if args.serial: os.environ['ANDROID_SERIAL'] = args.serial
spec = importlib.util.spec_from_file_location('android_ui_helpers', Path(__file__).with_name('android-ui-smoke.py'))
s = importlib.util.module_from_spec(spec); spec.loader.exec_module(s)
prefix = 'shared-live-' + args.label + '-'
checks = []

def has(root, suffix): return any(n.get('resource-id', '').endswith(suffix) for n in root.iter('node'))
def text(root, suffix): return next(n.get('text') for n in root.iter('node') if n.get('resource-id', '').endswith(suffix))
def ready(name, condition):
    def safe(root):
        assert 'Game paused' not in s.texts(root), 'Unexpected application exception'
        return condition(root)
    return s.wait_for(prefix + name, safe, timeout=150)
def click(label):
    root = ready('before-' + re.sub(r'\W+', '-', label), lambda r: label in s.texts(r))
    node = next(n for n in root.iter('node') if n.get('text') == label)
    left,top,right,bottom = map(int,re.findall(r'\d+',node.get('bounds')))
    if right <= left or bottom <= top:
        root = s.scroll_to_text(prefix+'scroll-'+re.sub(r'\W+','-',label),label,max_swipes=8)
    s.tap(root, label)
def action(suffix):
    root = ready('before-' + suffix, lambda r: has(r, suffix))
    s.tap_id(root, suffix)
def manual(): return ready('manual', lambda r: has(r, 'live-call-sheet') and has(r, 'live-call'))
def options(item): action('live-options'); click(item)
def fit(root, suffix):
    node = next(n for n in root.iter('node') if n.get('resource-id', '').endswith(suffix))
    x1,y1,x2,y2 = map(int, re.findall(r'\d+', node.get('bounds')))
    maxx = max(int(re.findall(r'\d+', n.get('bounds'))[2]) for n in root.iter('node'))
    maxy = max(int(re.findall(r'\d+', n.get('bounds'))[3]) for n in root.iter('node'))
    assert 0 <= x1 < x2 <= maxx and 0 <= y1 < y2 <= maxy, suffix + ' is clipped'

try:
    ready('entry', lambda r: has(r, 'live-game-page'))
    options('Play calling'); click('Take over')
    root = manual()
    assert 'Offense: You' in text(root,'live-ownership')
    assert 'Defense: You' in text(root,'live-ownership')
    checks.append('takeover-ownership')

    initial = text(root, 'live-selection')
    s.tap_id(root, 'live-quick-call-1')
    root = ready('alternate-selected', lambda r: has(r,'live-selection') and text(r,'live-selection') != initial)
    selected = text(root,'live-selection'); clock = text(root,'live-situation')
    options('Weather'); click('Close'); root = manual()
    assert text(root,'live-selection') == selected and text(root,'live-situation') == clock
    options('Viewing options'); click('Camera'); click('Broadcast'); root = manual()
    assert text(root,'live-selection') == selected and text(root,'live-situation') == clock
    checks.append('selection-and-clock-preserved-by-menus')

    action('live-playbook')
    ready('playbook', lambda r: has(r,'game-day-call-picker'))
    click('Close playbook'); manual()
    checks.append('playbook-close')

    action('live-team'); click('Substitutions')
    ready('substitutions', lambda r: 'Substitutions · this game only' in s.texts(r))
    click('Cancel'); manual()
    action('live-team'); click('Use second unit'); click('Use backups'); manual()
    action('live-team'); click('Use starters'); manual()
    checks.append('team-substitutions-and-second-unit')

    options('Box score'); ready('box', lambda r: 'Live box score' in s.texts(r))
    click('Player statistics'); click('Return to game'); root = manual()
    assert text(root,'live-selection') == selected and text(root,'live-situation') == clock
    checks.append('box-return-with-state-preserved')
    options('Finish game'); click('Keep playing'); root = manual()
    assert text(root,'live-situation') == clock
    options('Exit game'); click('Keep playing'); manual()
    checks.append('finish-and-exit-cancel')

    action('live-call')
    # uiautomator waits for UI idle and can miss the entire animated state.
    # Capture frames directly; use the subsequent stable hierarchy for action assertions.
    for frame in range(3):
        time.sleep(.35)
        Path(prefix+f'active-frame-{frame}.png').write_bytes(s.adb('exec-out','screencap','-p'))
    root = ready('after-snap', lambda r: has(r,'live-call-sheet') and text(r,'live-situation') != clock)
    fit(root,'live-pause'); fit(root,'live-options')
    after = text(root,'live-situation')
    options('Replay last play')
    root = ready('replay-finished', lambda r: has(r,'live-call-sheet'))
    assert text(root,'live-situation') == after
    checks.append('snap-sheet-transition-and-replay-invariant')

    options('Play calling'); click('Watch both sides')
    playing = ready('watch-running', lambda r: has(r,'live-ownership') and 'Offense: CPU' in text(r,'live-ownership') and 'Defense: CPU' in text(r,'live-ownership'))
    action('live-pause')
    paused = ready('paused', lambda r: has(r,'live-state') and text(r,'live-state') == 'Paused')
    held = text(paused,'live-situation'); time.sleep(2)
    paused = s.capture(prefix+'paused-held'); assert text(paused,'live-situation') == held
    options('Weather'); click('Close')
    paused = ready('paused-after-menu', lambda r: has(r,'live-state') and text(r,'live-state') == 'Paused')
    assert text(paused,'live-situation') == held
    checks.append('watch-pause-and-modal-pause-preservation')
    for offense, defense in [('You','CPU'), ('CPU','You'), ('CPU','CPU'), ('You','You')]:
        for unit, value in [('Offense',offense), ('Defense',defense)]:
            options('Play calling')
            menu = ready('ownership-menu', lambda r: any((t or '').startswith(unit+':') for t in s.texts(r)))
            click(next(t for t in s.texts(menu) if (t or '').startswith(unit+':'))); click(value)
        root = ready('ownership-state', lambda r: has(r,'live-ownership') and f'Offense: {offense}' in text(r,'live-ownership') and f'Defense: {defense}' in text(r,'live-ownership'))
        assert text(root,'live-situation') == held
    checks.append('all-four-ownership-combinations-through-menu')
    options('Play calling'); click('Take over'); manual()

    for rotation, name in [('0','portrait'),('1','landscape')]:
        s.adb('shell','settings','put','system','accelerometer_rotation','0')
        s.adb('shell','settings','put','system','user_rotation',rotation); time.sleep(2)
        root = manual(); root = s.capture(prefix+name)
        for suffix in ('live-call','live-pause','live-options','live-field'): fit(root,suffix)
        if name == 'landscape':
            before = text(root,'live-situation'); action('live-call')
            time.sleep(.5); Path(prefix+'landscape-active.png').write_bytes(s.adb('exec-out','screencap','-p'))
            ready('landscape-after-snap',lambda r:has(r,'live-call-sheet') and text(r,'live-situation')!=before)
            action('live-options'); s.capture(prefix+'landscape-menu'); click('Cancel')
    checks.append('portrait-landscape-actions-fit')
finally:
    Path(prefix+'result.json').write_text(json.dumps({'checks':checks},indent=2),encoding='utf-8')
    Path(prefix+'logcat.txt').write_bytes(s.adb('logcat','-d'))
print(json.dumps({'passed':len(checks),'checks':checks},indent=2))

#!/usr/bin/env python3
# NIGHT74 builder B (slate B1, weak74, rule W: on a boss, cyan means "shoot here" and nothing else; FORGE74 D-4). The lead's meter for the cyan a boss frame holds OUTSIDE the
# open weak point's screen box. Extended from n73-hue.py (same HSV conversion, same 480 x 270 working size, same danger band), written here, never moved after a run.
#
#   python -I Tools/night/n74-cyan.py <image> [<image> ...] [--log boss63-crown37.txt] [--weak x0,y0,x1,y1] [--body x0,y0,x1,y1] [--no-mask]
#   python -I Tools/night/n74-cyan.py --pair <on.png> <off.png> [--log-on <txt>] [--log-off <txt>] [--weak ...] [--body ...] [--no-mask]
#   python -I Tools/night/n74-cyan.py --selftest
#
# The boxes come from the boss63 reel's [weak74] line (FeelReel.N74.B.cs: `[weak74] shot-N30-B63-crown37-idle.png | boss crown37 | row on | open no | weak box none | body box
# x0,y0,x1,y1 | frame 1920x1080 | ...`, image pixels, rows from the top): --log reads the line whose file name is the image's own. A box of "none" / "off" / "behind" means no weak box
# (everything counts as outside it); "partial:..." spans the frame (everything counts as inside). --weak / --body give a box by hand (full-frame pixels, or fractions of the frame when
# every value is at most 1).
#
# CYAN: hue 160-205 deg (colours63's player band), saturation >= 0.35, value >= 0.25 (the same 0.35 floor n73-hue.py uses for the danger band). The HUD (the boss bar and its OPEN line, the
# score, the health block, the weapon panel) and the player's own gun (the lower right: the TORCH is the player's cyan, outside the rule) are MASKED by default, in fractions of the frame, the same
# mask on both sides of a pair: the share is of the frame's unmasked pixels. --no-mask counts the whole frame.
# DANGER (n73-hue.py's): hue <= 40 or >= 345, S >= 0.35, V >= 0.20, over the frame minus the HUD bands (top 12 %, bottom 18 %); the pair prints both sides' share and the change in points.
#
# The pair's lines answer B1's doneWhen: the cyan px OUTSIDE the weak box down by >= 70 % (ON vs OFF), the cyan px INSIDE it within 10 % of OFF, the danger share not up by more than 2 points.
import argparse
import os
import re
import sys

import numpy as np
from PIL import Image

W, H = 480, 270
CYAN_LO, CYAN_HI, CYAN_S, CYAN_V = 160.0, 205.0, 0.35, 0.25
# the mask, fractions of the frame (x0, y0, x1, y1): the boss bar / OPEN line / score band, the health block, the weapon panel, the gun
MASK = {
    'top HUD': (0.0, 0.0, 1.0, 0.15),
    'health': (0.0, 0.88, 0.25, 1.0),
    'weapon panel': (0.82, 0.84, 1.0, 1.0),
    'gun': (0.52, 0.55, 1.0, 1.0),
}


def load(path):
    im = Image.open(path).convert('RGB').resize((W, H))
    hsv = np.asarray(im.convert('HSV')).astype(np.float64)
    return hsv[..., 0] * 360.0 / 255.0, hsv[..., 1] / 255.0, hsv[..., 2] / 255.0


def rect_mask(box):
    """box in fractions (x0, y0, x1, y1) -> a bool array W x H rows."""
    m = np.zeros((H, W), dtype=bool)
    x0, y0, x1, y1 = box
    m[int(round(y0 * H)):int(round(y1 * H)), int(round(x0 * W)):int(round(x1 * W))] = True
    return m


def masked(use_mask):
    m = np.zeros((H, W), dtype=bool)
    if use_mask:
        for box in MASK.values():
            m |= rect_mask(box)
    return m


def cyan_mask(h, s, v):
    return (h >= CYAN_LO) & (h <= CYAN_HI) & (s >= CYAN_S) & (v >= CYAN_V)


def danger_share(h, s, v):
    y0, y1 = int(H * 0.12), int(H * 0.82)
    d = ((h <= 40) | (h >= 345)) & (s >= 0.35) & (v >= 0.20)
    return 100.0 * d[y0:y1].sum() / d[y0:y1].size


def parse_box(text, frame):
    """'x0,y0,x1,y1' (px of `frame` = (w, h), or fractions when all <= 1) -> fractions, or None."""
    if text is None:
        return None
    vals = [float(t) for t in text.split(',')]
    if len(vals) != 4:
        raise SystemExit('a box is x0,y0,x1,y1: ' + text)
    if max(vals) <= 1.0:
        return tuple(vals)
    fw, fh = frame
    return (vals[0] / fw, vals[1] / fh, vals[2] / fw, vals[3] / fh)


LINE = re.compile(r'\[weak74\] (\S+) \| boss (\S+) \| row (\w+) \| open ([\w ]+?) \| weak box (\S+) \| body box (\S+) \| frame (\d+)x(\d+)')


def from_log(path, image):
    """The [weak74] line for the image's own file name: dict with boss, row, open, weak, body (fractions or None), frame."""
    want = os.path.basename(image)
    last = None
    with open(path, encoding='utf-8', errors='replace') as f:
        for line in f:
            m = LINE.search(line)
            if m and m.group(1) == want:
                last = m
    if last is None:
        return None
    fw, fh = int(last.group(7)), int(last.group(8))

    def box(t):
        if t in ('none', 'off', 'behind'):
            return None
        full = t.startswith('partial:')
        t = t.split(':', 1)[-1]
        return (0.0, 0.0, 1.0, 1.0) if full else parse_box(t, (fw, fh))

    return {'boss': last.group(2), 'row': last.group(3), 'open': last.group(4), 'weak': box(last.group(5)), 'body': box(last.group(6)), 'frame': (fw, fh),
            'weak_text': last.group(5), 'body_text': last.group(6)}


def measure(path, weak, body, use_mask):
    h, s, v = load(path)
    cy = cyan_mask(h, s, v)
    skip = masked(use_mask)
    live = ~skip
    wk = rect_mask(weak) if weak else np.zeros((H, W), dtype=bool)
    bd = rect_mask(body) if body else np.zeros((H, W), dtype=bool)
    n = int(live.sum())
    inside = int((cy & live & wk).sum())
    outside = int((cy & live & ~wk).sum())
    in_body_outside_weak = int((cy & live & bd & ~wk).sum())
    return {
        'path': path, 'px': n, 'cyan': inside + outside, 'inside': inside, 'outside': outside, 'body_outside': in_body_outside_weak,
        'cyan_all': int(cy.sum()), 'danger': danger_share(h, s, v),
        'share_outside': 100.0 * outside / max(1, n), 'share_inside': 100.0 * inside / max(1, n),
    }


def describe(r, info):
    t = '%s: cyan %.2f %% of %d unmasked px (inside the weak box %d px, outside %d px; outside AND inside the body box %d px); whole frame unmasked %d px | danger %.1f %%' % (
        r['path'], 100.0 * r['cyan'] / max(1, r['px']), r['px'], r['inside'], r['outside'], r['body_outside'], r['cyan_all'], r['danger'])
    if info:
        t += ' | log: boss %s, row %s, open %s, weak box %s, body box %s' % (info['boss'], info['row'], info['open'], info['weak_text'], info['body_text'])
    return t


def resolve(path, log, weak_arg, body_arg):
    info = from_log(log, path) if log else None
    if log and info is None:
        print('NO [weak74] LINE for %s in %s (the boxes are not known: nothing is treated as inside a weak box)' % (os.path.basename(path), log))
    frame = info['frame'] if info else (1920, 1080)
    weak = parse_box(weak_arg, frame) if weak_arg else (info['weak'] if info else None)
    body = parse_box(body_arg, frame) if body_arg else (info['body'] if info else None)
    return weak, body, info


def pair(on, off, args):
    wk_on, bd_on, info_on = resolve(on, args.log_on, args.weak, args.body)
    wk_off, bd_off, info_off = resolve(off, args.log_off, args.weak, args.body)
    # the weak point is the same place on both sides (a same-run pair): the OFF side's box is the one measured (the ON side's log may differ by a pixel)
    weak = wk_off if wk_off is not None else wk_on
    body = bd_off if bd_off is not None else bd_on
    a = measure(off, weak, body, not args.no_mask)
    b = measure(on, weak, body, not args.no_mask)
    print('OFF ' + describe(a, info_off))
    print('ON  ' + describe(b, info_on))
    down = 100.0 * (1.0 - b['outside'] / a['outside']) if a['outside'] > 0 else float('nan')
    print('cyan OUTSIDE the weak box: OFF %d px, ON %d px: down %.1f %% (B1 doneWhen: >= 70)%s' % (a['outside'], b['outside'], down, '  PASS' if a['outside'] > 0 and down >= 70.0 else '  not met' if a['outside'] > 0 else '  (OFF has no cyan outside: nothing to take)'))
    if weak is not None and a['inside'] > 0:
        within = 100.0 * abs(b['inside'] - a['inside']) / a['inside']
        print('cyan INSIDE the weak box: OFF %d px, ON %d px: within %.1f %% (B1 doneWhen: <= 10)%s' % (a['inside'], b['inside'], within, '  PASS' if within <= 10.0 else '  not met'))
    else:
        print('cyan INSIDE the weak box: no weak box (closed frame) or no cyan in it: not measured')
    dd = b['danger'] - a['danger']
    print('danger share (n73-hue.py): OFF %.2f %%, ON %.2f %%: %+.2f points (B1 doneWhen: not up by more than 2)%s' % (a['danger'], b['danger'], dd, '  PASS' if dd <= 2.0 else '  not met'))


def selftest():
    """The planted case: a synthetic frame with cyan at known places; the meter must read exactly those."""
    import tempfile
    ok = True

    def check(name, cond, got):
        nonlocal ok
        print(('PASS ' if cond else 'FAIL ') + name + ' (' + got + ')')
        ok = ok and cond

    d = tempfile.mkdtemp(prefix='n74cyan-')
    cyan = (25, 211, 230)  # colors.usable #19D3E6
    grey = (70, 66, 90)

    def frame(boxes, name):
        im = Image.new('RGB', (1920, 1080), grey)
        px = im.load()
        for (x0, y0, x1, y1, c) in boxes:
            for y in range(y0, y1):
                for x in range(x0, x1):
                    px[x, y] = c
        p = os.path.join(d, name)
        im.save(p)
        return p

    # a ring-like cyan block outside the weak box (400 x 300 px at 1080p), another inside it (200 x 100), the weak box around the second
    weak = (0.50, 0.30, 0.60, 0.40)  # x 960-1152, y 324-432
    f_off = frame([(300, 400, 700, 700, cyan), (1000, 340, 1100, 420, cyan)], 'off.png')
    f_on = frame([(1000, 340, 1100, 420, cyan)], 'on.png')  # the outside block gone, the inside one kept
    f_none = frame([], 'none.png')
    a = measure(f_off, weak, None, True)
    b = measure(f_on, weak, None, True)
    n = measure(f_none, weak, None, True)
    fa = (400 * 300) / (1920 * 1080) * W * H
    fi = (100 * 80) / (1920 * 1080) * W * H
    check('outside block read (within 4 %)', abs(a['outside'] - fa) / fa < 0.04, '%d px, planted about %.0f' % (a['outside'], fa))
    check('inside block read (within 6 %)', abs(a['inside'] - fi) / fi < 0.06, '%d px, planted about %.0f' % (a['inside'], fi))
    check('ON side: outside is 0 and inside unchanged', b['outside'] == 0 and b['inside'] == a['inside'], 'outside %d, inside %d vs %d' % (b['outside'], b['inside'], a['inside']))
    check('a frame with no cyan reads 0', n['cyan'] == 0, '%d px' % n['cyan'])
    # the mask: cyan in the gun's corner and the HUD is not counted with the mask on, counted with --no-mask
    f_gun = frame([(1300, 800, 1800, 1000, cyan), (700, 60, 1200, 120, cyan)], 'gun.png')
    g1 = measure(f_gun, None, None, True)
    g0 = measure(f_gun, None, None, False)
    check('gun and top HUD masked by default', g1['cyan'] == 0, '%d px' % g1['cyan'])
    check('--no-mask counts them', g0['cyan'] > 2000, '%d px' % g0['cyan'])
    # a danger-hue frame does not read as cyan, a pale cyan-white pulse does
    f_red = frame([(300, 400, 900, 800, (255, 74, 28))], 'red.png')
    r = measure(f_red, None, None, True)
    check('red-orange is not cyan, and reads as danger', r['cyan'] == 0 and r['danger'] > 5.0, 'cyan %d px, danger %.1f %%' % (r['cyan'], r['danger']))
    f_pale = frame([(300, 400, 700, 700, (153, 255, 255))], 'pale.png')
    pl = measure(f_pale, None, None, True)
    check('a pale cyan-white pulse (S 0.4) reads as cyan', pl['cyan'] > 1000, '%d px' % pl['cyan'])
    f_iv = frame([(300, 400, 700, 700, (232, 223, 200))], 'ivory.png')
    iv = measure(f_iv, None, None, True)
    check('the crown ivory identity colour is not cyan', iv['cyan'] == 0, '%d px' % iv['cyan'])
    # the log line parse
    lp = os.path.join(d, 'boss63-test.txt')
    with open(lp, 'w', encoding='utf-8') as f:
        f.write('[weak74] off.png | boss crown37 | row off | open yes | weak box 960,324,1152,432 | body box 100,100,1500,1000 | frame 1920x1080 | parts cyan 5 of 40 | identity #E8DFC8 | x\n')
    info = from_log(lp, os.path.join(d, 'off.png'))
    check('the [weak74] line parses to fractions', info is not None and abs(info['weak'][0] - 0.5) < 1e-9 and abs(info['weak'][3] - 0.4) < 1e-9, str(info and info['weak']))
    check('a missing line is None', from_log(lp, os.path.join(d, 'nope.png')) is None, 'nope.png')
    print('SELFTEST ' + ('PASS' if ok else 'FAIL'))
    return 0 if ok else 1


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('images', nargs='*')
    ap.add_argument('--pair', nargs=2, metavar=('ON', 'OFF'))
    ap.add_argument('--log')
    ap.add_argument('--log-on')
    ap.add_argument('--log-off')
    ap.add_argument('--weak')
    ap.add_argument('--body')
    ap.add_argument('--no-mask', action='store_true')
    ap.add_argument('--selftest', action='store_true')
    args = ap.parse_args()
    if args.selftest:
        return selftest()
    if args.pair:
        pair(args.pair[0], args.pair[1], args)
        return 0
    if not args.images:
        ap.print_usage()
        return 2
    for p in args.images:
        weak, body, info = resolve(p, args.log, args.weak, args.body)
        print(describe(measure(p, weak, body, not args.no_mask), info))
    return 0


if __name__ == '__main__':
    sys.exit(main())

#!/usr/bin/env python3
# NIGHT74 builder B (slate B3, kill74): the lead's meter for the boss kill's flash on the framed bosskill72 strip. "the strip's kill frame +1 and +2 show the flash (mean luma inside the boss box
# up >= 40 points ON vs OFF)". Frames are the kept jpgs of a bosskill72 side (n74-reels.sh ... --keep; Logs/feel/<tag>/<side>/bosskill72/f%04d.jpg), the kill frame f is the dying beat's frame the
# reel prints (`CUT the dying beat (frame f525, bossDown at f...)`), the box the head's screen box the same reel prints (`AT THE DYING BEAT (f525): ... box x X0..X1 y Y0..Y1`: image x, y from the
# BOTTOM, as WorldToScreenPoint gives it).
#
#   python -I Tools/night/n74-kill.py --on <on dir> --off <off dir> --log <bosskill72-wyrm.txt of the OFF side> [--k 0,1,2,3] [--box x0,y0,x1,y1 [--ybottom]] [--frame f]
#   python -I Tools/night/n74-kill.py --selftest
#
# Per frame f+k: the mean luma (0.2126 R + 0.7152 G + 0.0722 B, 0-255) inside the box, ON and OFF, and the difference; the verdict line: the difference at +1 and +2 against 40 points (the slate's bar).
# --log gives the frame and the box (the dying-beat box); --box / --frame override it by hand (full-frame pixels of --W x --H, default 1920 x 1080, scaled to each jpg's own size). The OFF side's
# frame f+k may differ from the ON side's in WHAT the sim has done (the hold slows the ON side's stepping), never in which rendered frame it is: both sides are the same reel at the same frame count.
import argparse
import os
import re
import sys

from PIL import Image
import numpy as np

BAR = 40.0


def luma_in(path, box, ref):
    im = Image.open(path).convert('RGB')
    w, h = im.size
    rw, rh = ref
    x0, y0, x1, y1 = box
    x0, x1 = int(round(x0 * w / rw)), int(round(x1 * w / rw))
    y0, y1 = int(round(y0 * h / rh)), int(round(y1 * h / rh))
    x0, y0 = max(0, x0), max(0, y0)
    x1, y1 = min(w, x1), min(h, y1)
    if x1 <= x0 or y1 <= y0:
        return float('nan')
    a = np.asarray(im).astype(np.float64)[y0:y1, x0:x1]
    return float((0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]).mean())


def from_log(path):
    """(frame, box in image px with y from the top, (W, H)) from a bosskill72 log: the CUT line's dying frame and the AT THE DYING BEAT box. None when a part is missing."""
    txt = open(path, encoding='utf-8', errors='replace').read()
    m = re.search(r'CUT the dying beat \(frame f(\d+)', txt)
    b = re.search(r'AT THE DYING BEAT \(f\d+\): [^|]*?box x (\d+)\.\.(\d+) y (\d+)\.\.(\d+) = \d+ x \d+ px of (\d+) x (\d+)', txt)
    if not m or not b:
        return None
    x0, x1, yb0, yb1, w, h = (int(b.group(i)) for i in range(1, 7))
    return int(m.group(1)), (x0, h - yb1, x1, h - yb0), (w, h)


def jpg(d, f):
    return os.path.join(d, 'f%04d.jpg' % f)


def measure(on, off, frame, box, ref, ks):
    rows = []
    for k in ks:
        a, b = jpg(off, frame + k), jpg(on, frame + k)
        miss = [p for p in (a, b) if not os.path.exists(p)]
        if miss:
            rows.append((k, None, None, 'missing ' + ', '.join(miss) + ' (a pruned side? re-run it with --keep)'))
            continue
        la, lb = luma_in(a, box, ref), luma_in(b, box, ref)
        rows.append((k, la, lb, ''))
    return rows


def report(rows, frame, box):
    print('kill frame f%04d, box x %d..%d y %d..%d (rows from the top)' % (frame, box[0], box[2], box[1], box[3]))
    ok = True
    seen = {}
    for k, la, lb, why in rows:
        if la is None:
            print('  f+%d: %s' % (k, why))
            continue
        d = lb - la
        seen[k] = d
        print('  f+%d: mean luma OFF %.1f, ON %.1f, ON - OFF %+.1f%s' % (k, la, lb, d, '' if k not in (1, 2) else ('  >= %d' % BAR if d >= BAR else '  < %d' % BAR)))
    for k in (1, 2):
        if k not in seen or seen[k] < BAR:
            ok = False
    print('KILL74 FLASH %s (the bar: ON - OFF >= %d points at f+1 and f+2%s)' % ('PASS' if ok else 'NOT MET', BAR, '' if all(k in seen for k in (1, 2)) else '; a frame is missing'))
    return ok


def selftest():
    import tempfile
    d = tempfile.mkdtemp(prefix='n74kill-')
    ok = True

    def frame(path, lum_in_box, box, size=(320, 180), bg=40):
        im = Image.new('RGB', size, (bg, bg, bg))
        px = im.load()
        for y in range(box[1], box[3]):
            for x in range(box[0], box[2]):
                px[x, y] = (lum_in_box, lum_in_box, lum_in_box)
        im.save(path)

    on, off = os.path.join(d, 'on'), os.path.join(d, 'off')
    os.makedirs(on)
    os.makedirs(off)
    box = (100, 50, 200, 130)  # in the 320 x 180 frames
    f = 525
    for k in range(0, 4):
        frame(jpg(off, f + k), 80, box, bg=40)
        frame(jpg(on, f + k), 230 if k in (1, 2) else 80, box, bg=40)
    rows = measure(on, off, f, box, (320, 180), [0, 1, 2, 3])
    got = {k: lb - la for k, la, lb, _ in rows}

    def check(name, cond, got_s):
        nonlocal ok
        print(('PASS ' if cond else 'FAIL ') + name + ' (' + got_s + ')')
        ok = ok and cond

    check('+1 and +2 read the planted flash', abs(got[1] - 150) < 1 and abs(got[2] - 150) < 1, '%.1f %.1f' % (got[1], got[2]))
    check('+0 and +3 read no flash', abs(got[0]) < 1 and abs(got[3]) < 1, '%.1f %.1f' % (got[0], got[3]))
    check('the verdict passes on the planted flash', report(rows, f, box), 'see above')
    rows2 = measure(on, off, f, box, (320, 180), [0, 1, 2, 3, 9])
    check('a missing frame is said, not read', rows2[-1][1] is None and 'missing' in rows2[-1][3], rows2[-1][3][:40])
    # a flash outside the box does not count: ON bright everywhere but the box
    for k in range(0, 4):
        frame(jpg(on, f + k), 80, box, bg=40 if k == 0 else 200)
    rows3 = measure(on, off, f, box, (320, 180), [1, 2])
    check('a bright background outside the box is not the flash', all(abs(lb - la) < 1 for _, la, lb, _ in rows3), '%s' % [round(lb - la, 1) for _, la, lb, _ in rows3])
    # the log parse
    lp = os.path.join(d, 'bosskill72-wyrm.txt')
    with open(lp, 'w', encoding='utf-8') as fh:
        fh.write('[bosskill72] wyrm framing: ... | AT THE DYING BEAT (f525): 25.0 m, box x 700..1200 y 300..800 = 500 x 500 px of 1920 x 1080 (26 % of the width, 46 % of the height; 3 of 3 pieces\' centres in the frame) | half ...\n'
                 '[bosskill72] wyrm CUT the dying beat (frame f525, bossDown at f700): f519 through f585: python ...\n')
    r = from_log(lp)
    check('the log gives the frame and the box (y flipped to rows from the top)', r is not None and r[0] == 525 and r[1] == (700, 280, 1200, 780) and r[2] == (1920, 1080), str(r))
    print('SELFTEST ' + ('PASS' if ok else 'FAIL'))
    return 0 if ok else 1


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--on')
    ap.add_argument('--off')
    ap.add_argument('--log')
    ap.add_argument('--k', default='0,1,2,3')
    ap.add_argument('--box')
    ap.add_argument('--ybottom', action='store_true')
    ap.add_argument('--frame', type=int)
    ap.add_argument('--W', type=int, default=1920)
    ap.add_argument('--H', type=int, default=1080)
    ap.add_argument('--selftest', action='store_true')
    a = ap.parse_args()
    if a.selftest:
        return selftest()
    if not (a.on and a.off):
        ap.print_usage()
        return 2
    frame, box, ref = None, None, (a.W, a.H)
    if a.log:
        got = from_log(a.log)
        if got is None:
            print('NO dying-beat frame / box in %s (is it the bosskill72 line of a framed THE KILN WYRM run?)' % a.log)
        else:
            frame, box, ref = got
    if a.frame is not None:
        frame = a.frame
    if a.box:
        vals = [int(float(v)) for v in a.box.split(',')]
        box = (vals[0], ref[1] - vals[3], vals[2], ref[1] - vals[1]) if a.ybottom else tuple(vals)
    if frame is None or box is None:
        print('need a frame and a box (--log, or --frame and --box)')
        return 2
    rows = measure(a.on, a.off, frame, box, ref, [int(k) for k in a.k.split(',')])
    return 0 if report(rows, frame, box) else 1


if __name__ == '__main__':
    sys.exit(main())

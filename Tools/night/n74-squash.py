#!/usr/bin/env python3
# NIGHT74 builder B (leftover 3, spring74): the lead's meter for "an enemy's hit squash a glance sees". spring72's flinch read NOTHING OBVIOUS at hit + 1 on the close crops (NIGHT73 M5): its squash was reached
# about five frames after the hit and crunch36's own 15 % pop was all the frames showed. The body is the danger-hue pill (rule 8: enemies are red-orange): the meter finds its pixels in a crop of the frame,
# takes the bounding box of the 2nd-98th percentile of those pixels (a spark, a stray lava pixel does not stretch it) and prints its height and width in px; the pair compares the ON side's height with the OFF side's
# on the hit frame and the frames after it.
#   python -I Tools/night/n74-squash.py --on <on dir> --off <off dir> --hit <f> [--k 0,1,2,3,5] [--crop x0,y0,x1,y1] [--log <off dir>/spring73.txt]
#   python -I Tools/night/n74-squash.py --on-img <png> --off-img <png> [--crop ...]        (one frame each)
#   python -I Tools/night/n74-squash.py --selftest
# Frames are the kept jpgs of a spring73 side (n74-reels.sh ... --keep: <dir>/f%04d.jpg); --hit is the telemetry frame f of a non-lethal hit (spring73's line 1: `telemetry frames f a,b,c,d`; --log takes the
# first). The crop is fractions of the frame (default: the middle, x 0.30-0.70, y 0.12-0.88: the husk stands 5 m ahead with the camera on it); on the kiln map the floor and the walls are orange too, so look at the OFF
# idle frame first: a crop that holds only the husk is the honest one (the lines print the box so a reader can check it).
# DANGER PIXELS: hue <= 35 or >= 345 deg, saturation >= 0.45, value >= 0.25 (n73-hue.py's danger family with a higher saturation floor: the body is the most saturated thing in the crop).
# THE BAR (written here, never moved after a run): at hit + 1 AND hit + 2 the ON body's height is at most 85 % of the OFF body's (the squash is at least 15 points deeper than the game's own pop shows on the OFF
# side), and the ON height is back within 8 % of the OFF height by hit + 12 (it settles: the spring is quiet).
import argparse
import os
import re
import sys

import numpy as np
from PIL import Image

BAR_DOWN = 0.85
BAR_BACK = 0.08


def danger_box(img, crop):
    im = img.convert('RGB')
    w, h = im.size
    x0, y0, x1, y1 = [int(round(v)) for v in (crop[0] * w, crop[1] * h, crop[2] * w, crop[3] * h)]
    hsv = np.asarray(im.convert('HSV')).astype(float)[y0:y1, x0:x1]
    hh, s, v = hsv[..., 0] * 360 / 255, hsv[..., 1] / 255, hsv[..., 2] / 255
    m = ((hh <= 35) | (hh >= 345)) & (s >= 0.45) & (v >= 0.25)
    ys, xs = np.nonzero(m)
    if ys.size < 40:
        return None
    ylo, yhi = np.percentile(ys, [2, 98])
    xlo, xhi = np.percentile(xs, [2, 98])
    return (x0 + xlo, y0 + ylo, x0 + xhi, y0 + yhi, int(ys.size))


def describe(box, size):
    if box is None:
        return 'no danger pixels in the crop'
    return 'box x %d..%d y %d..%d = %d x %d px (%d px of danger hue)' % (box[0], box[2], box[1], box[3], box[2] - box[0], box[3] - box[1], box[4])


def hit_from_log(path):
    txt = open(path, encoding='utf-8', errors='replace').read()
    m = re.search(r'telemetry frames f ([0-9]+(?:,[0-9]+)*)', txt)
    return int(m.group(1).split(',')[0]) if m else None


def jpg(d, f):
    return os.path.join(d, 'f%04d.jpg' % f)


def pair_frames(on_dir, off_dir, hit, ks, crop):
    rows = []
    for k in ks:
        a, b = jpg(off_dir, hit + k), jpg(on_dir, hit + k)
        miss = [p for p in (a, b) if not os.path.exists(p)]
        if miss:
            rows.append((k, None, None, 'missing ' + ', '.join(miss) + ' (a pruned side? re-run with --keep)'))
            continue
        rows.append((k, danger_box(Image.open(a), crop), danger_box(Image.open(b), crop), ''))
    return rows


def report(rows, hit):
    print('hit frame f%04d' % hit if hit is not None else 'one frame each')
    h = {}
    for k, a, b, why in rows:
        if why:
            print('  hit+%d: %s' % (k, why))
            continue
        if a is None or b is None:
            print('  hit+%d: OFF %s | ON %s' % (k, describe(a, None), describe(b, None)))
            continue
        ha, hb = a[3] - a[1], b[3] - b[1]
        wa, wb = a[2] - a[0], b[2] - b[0]
        h[k] = hb / ha if ha > 0 else float('nan')
        print('  hit+%d: OFF height %.0f x width %.0f px | ON height %.0f x width %.0f px | ON/OFF height %.2f, width %.2f' % (k, ha, wa, hb, wb, hb / ha, wb / wa))
    ok = all(k in h and h[k] <= BAR_DOWN for k in (1, 2))
    settled = [k for k in h if k >= 12]
    back = all(abs(1 - h[k]) <= BAR_BACK for k in settled) if settled else None
    print('SPRING74 SQUASH %s (the bar: ON height <= %.0f %% of OFF at hit + 1 and hit + 2%s)' % ('PASS' if ok else 'NOT MET', BAR_DOWN * 100, '' if all(k in h for k in (1, 2)) else '; a frame is missing'))
    print('  settles: %s' % ('not measured (no frame at hit + 12 or later)' if back is None else ('PASS: ON within %.0f %% of OFF by hit + 12' % (BAR_BACK * 100) if back else 'NOT MET: ON is not back by hit + 12')))
    return ok


def selftest():
    import tempfile
    ok = True

    def check(name, cond, got):
        nonlocal ok
        print(('PASS ' if cond else 'FAIL ') + name + ' (' + got + ')')
        ok = ok and cond

    def frame(path, hpx, wpx, lava=False):
        im = Image.new('RGB', (1920, 1080), (60, 70, 100))
        px = im.load()
        cx, base = 960, 760
        for y in range(base - hpx, base):
            for x in range(cx - wpx // 2, cx + wpx // 2):
                px[x, y] = (255, 74, 28)
        if lava:  # an orange floor strip outside the crop's width but inside its rows (x 0..500)
            for y in range(700, 800):
                for x in range(0, 500):
                    px[x, y] = (255, 120, 20)
        im.save(path)

    d = tempfile.mkdtemp(prefix='n74squash-')
    on, off = os.path.join(d, 'on'), os.path.join(d, 'off')
    os.makedirs(on)
    os.makedirs(off)
    hit = 300
    # OFF: the game's own 15 % pop on the hit frame and the next, back by +3; ON: the held 0.68 through +2, free after
    off_h = {0: 220, 1: 224, 2: 232, 3: 260, 12: 260}
    on_h = {0: 177, 1: 177, 2: 177, 3: 230, 12: 261}
    for k in off_h:
        frame(jpg(off, hit + k), off_h[k], 100, lava=True)
        frame(jpg(on, hit + k), on_h[k], int(100 * (260 / on_h[k]) ** 0.5), lava=True)
    crop = (0.30, 0.12, 0.70, 0.88)
    b = danger_box(Image.open(jpg(off, hit + 3)), crop)
    check('the husk box height is read (260 px planted, the 2nd-98th percentile)', b is not None and abs((b[3] - b[1]) - 260 * 0.96) < 10, describe(b, None))
    check('the orange floor strip outside the crop is not read (width stays 100)', b is not None and abs((b[2] - b[0]) - 96) < 8, '%.0f px' % ((b[2] - b[0]) if b else -1))
    rows = pair_frames(on, off, hit, [0, 1, 2, 3, 12], crop)
    check('the pair passes on the planted squash', report(rows, hit), 'see above')
    # a squash as deep as the OFF side's own does not pass
    for k in off_h:
        frame(jpg(on, hit + k), off_h[k], 100, lava=True)
    rows2 = pair_frames(on, off, hit, [0, 1, 2, 3, 12], crop)
    check('no change is NOT MET', not report(rows2, hit), 'see above')
    check('a missing frame is said', pair_frames(on, off, hit, [40], crop)[0][3].startswith('missing'), 'hit+40')
    lp = os.path.join(d, 'spring73.txt')
    with open(lp, 'w', encoding='utf-8') as fh:
        fh.write('GREEN spring73 | husk id 7 at 5 m, hp 275 before the hits, hits landed 5 of 5 non-lethal (at reel frames n 40,70,100; telemetry frames f 300,330,360; gaps 30,30; aimed at 30) + the kill\n')
    check('the hit frame is read from the log', hit_from_log(lp) == 300, str(hit_from_log(lp)))
    print('SELFTEST ' + ('PASS' if ok else 'FAIL'))
    return 0 if ok else 1


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--on')
    ap.add_argument('--off')
    ap.add_argument('--hit', type=int)
    ap.add_argument('--k', default='0,1,2,3,5,12')
    ap.add_argument('--crop', default='0.30,0.12,0.70,0.88')
    ap.add_argument('--log')
    ap.add_argument('--on-img')
    ap.add_argument('--off-img')
    ap.add_argument('--selftest', action='store_true')
    a = ap.parse_args()
    if a.selftest:
        return selftest()
    crop = tuple(float(v) for v in a.crop.split(','))
    if a.on_img and a.off_img:
        rows = [(0, danger_box(Image.open(a.off_img), crop), danger_box(Image.open(a.on_img), crop), '')]
        report(rows, None)
        return 0
    if not (a.on and a.off):
        ap.print_usage()
        return 2
    hit = a.hit if a.hit is not None else (hit_from_log(a.log) if a.log else None)
    if hit is None:
        print('need --hit <frame> or --log <spring73.txt>')
        return 2
    return 0 if report(pair_frames(a.on, a.off, hit, [int(k) for k in a.k.split(',')], crop), hit) else 1


if __name__ == '__main__':
    sys.exit(main())

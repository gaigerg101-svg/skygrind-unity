#!/usr/bin/env python3
# NIGHT74 builder B (leftover 2, crownroom74): the lead's meter for "one palette": how many hue FAMILIES a boss frame holds at once, and how much of it is violet and magenta (magenta is RIVAL's, rule 8).
# gg-proxy NIGHT73 close #2: THE CROWN's room is "olive, magenta, brown, red and cyan at once". The vivid pixels (saturation >= 0.35, value >= 0.20, the HUD bands out: top 15 %, bottom 12 %) are binned into
# eight families by hue (degrees, HSV on the sRGB frame, the working size 480 x 270 as n73-hue.py's):
#   red-orange 345-35 | amber / yellow 36-59 | yellow-green 60-110 | green 111-159 | cyan 160-205 | blue 206-229 | violet 230-284 | magenta 285-344
# A family "holds" the frame when it has >= 3 % of the frame's pixels (the same 3 % the spotter's colour reads start at). Written here, never moved after a run.
#   python -I Tools/night/n74-palette.py <image> [<image> ...]
#   python -I Tools/night/n74-palette.py --pair <on.png> <off.png> [<on2.png> <off2.png> ...]
#   python -I Tools/night/n74-palette.py --selftest
# The pair's lines answer the crownroom74 card's bars: violet + magenta down by >= 60 % (OFF -> ON), the red-orange share not up by more than 2 points (danger stays the tells'), the families holding the
# frame not more than OFF's. NIGHT73's baseline on the five boss63 frames of THE CROWN is in Research/2026-10-27-wave74/B/card-crownroom74.md.
import argparse
import sys

import numpy as np
from PIL import Image

W, H = 480, 270
BANDS = [('red-orange', 345, 35), ('amber/yellow', 36, 59), ('yellow-green', 60, 110), ('green', 111, 159), ('cyan', 160, 205), ('blue', 206, 229), ('violet', 230, 284), ('magenta', 285, 344)]
HOLD = 3.0
TOP, BOTTOM = 0.15, 0.88


def shares(img):
    """({family: percent of the frame's rows between the HUD bands}, vivid percent, families holding the frame)."""
    im = img.convert('RGB').resize((W, H))
    hsv = np.asarray(im.convert('HSV')).astype(float)
    h, s, v = hsv[..., 0] * 360 / 255, hsv[..., 1] / 255, hsv[..., 2] / 255
    y0, y1 = int(H * TOP), int(H * BOTTOM)
    m = (s >= 0.35) & (v >= 0.20)
    m[:y0] = False
    m[y1:] = False
    tot = (y1 - y0) * W
    out = {}
    for name, lo, hi in BANDS:
        sel = m & (((h >= lo) & (h <= hi)) if lo <= hi else ((h >= lo) | (h <= hi)))
        out[name] = 100.0 * sel.sum() / tot
    vivid = 100.0 * m.sum() / tot
    return out, vivid, sum(1 for k in out if out[k] >= HOLD)


def line(label, sh, vivid, hold):
    return '%s: vivid %.1f %% | %s | families >= %.0f %%: %d (%s)' % (label, vivid, ' | '.join('%s %.1f' % (k, sh[k]) for k, _, _ in BANDS), HOLD, hold, ', '.join(k for k, _, _ in BANDS if sh[k] >= HOLD))


def pair(on, off):
    a, av, ah = shares(Image.open(off))
    b, bv, bh = shares(Image.open(on))
    print(line('OFF ' + off, a, av, ah))
    print(line('ON  ' + on, b, bv, bh))
    vm_a, vm_b = a['violet'] + a['magenta'], b['violet'] + b['magenta']
    down = 100.0 * (1 - vm_b / vm_a) if vm_a > 0 else float('nan')
    ok1 = vm_a > 0 and down >= 60.0
    dd = b['red-orange'] - a['red-orange']
    print('  violet + magenta: OFF %.1f %%, ON %.1f %%: down %.1f %% (bar: >= 60)%s' % (vm_a, vm_b, down, '  PASS' if ok1 else '  not met' if vm_a > 0 else '  (OFF has none)'))
    print('  red-orange: OFF %.1f %%, ON %.1f %%: %+.1f points (bar: not up by more than 2)%s' % (a['red-orange'], b['red-orange'], dd, '  PASS' if dd <= 2.0 else '  not met'))
    print('  families holding the frame: OFF %d, ON %d (bar: ON not above OFF)%s' % (ah, bh, '  PASS' if bh <= ah else '  not met'))
    return ok1 and dd <= 2.0 and bh <= ah


def selftest():
    ok = True

    def check(name, cond, got):
        nonlocal ok
        print(('PASS ' if cond else 'FAIL ') + name + ' (' + got + ')')
        ok = ok and cond

    def frame(blocks):
        im = Image.new('RGB', (1920, 1080), (90, 90, 90))  # a grey frame: no vivid pixels
        px = im.load()
        for (x0, y0, x1, y1, c) in blocks:
            for y in range(y0, y1):
                for x in range(x0, x1):
                    px[x, y] = c
        return im

    violet, magenta, slate, green, red = (60, 30, 99), (116, 22, 108), (95, 100, 115), (29, 60, 22), (255, 74, 28)
    f_off = frame([(0, 200, 960, 700, violet), (960, 200, 1500, 700, magenta), (1500, 200, 1900, 700, green)])
    f_on = frame([(0, 200, 960, 700, slate), (960, 200, 1500, 700, slate), (1500, 200, 1900, 700, green)])
    a, av, ah = shares(f_off)
    b, bv, bh = shares(f_on)
    rows = int(1080 * BOTTOM) - int(1080 * TOP)  # the rows between the HUD bands at 1080p
    want_v, want_m = 100.0 * 960 * 500 / (1920 * rows), 100.0 * 540 * 500 / (1920 * rows)
    check('violet block read (960 x 500 of the frame between the bands, about %.1f %%)' % want_v, abs(a['violet'] - want_v) < 2.0, '%.1f %%' % a['violet'])
    check('magenta block read (540 x 500, about %.1f %%)' % want_m, abs(a['magenta'] - want_m) < 2.0, '%.1f %%' % a['magenta'])
    check('a slate block is no family (S under 0.35)', b['violet'] + b['magenta'] + b['blue'] < 0.5, '%.2f %%' % (b['violet'] + b['magenta'] + b['blue']))
    check('three families hold the OFF frame (violet, magenta, green), one the ON frame', ah == 3 and bh == 1, '%d and %d' % (ah, bh))
    f_red = frame([(0, 300, 900, 700, red)])
    r, rv, rh = shares(f_red)
    check('red-orange is its own family', r['red-orange'] > 20 and r['magenta'] < 0.5, '%.1f %%' % r['red-orange'])
    import tempfile, os
    d = tempfile.mkdtemp(prefix='n74pal-')
    f_off.save(os.path.join(d, 'off.png'))
    f_on.save(os.path.join(d, 'on.png'))
    check('the pair passes on the planted change', pair(os.path.join(d, 'on.png'), os.path.join(d, 'off.png')), 'see above')
    print('SELFTEST ' + ('PASS' if ok else 'FAIL'))
    return 0 if ok else 1


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('images', nargs='*')
    ap.add_argument('--pair', action='store_true')
    ap.add_argument('--selftest', action='store_true')
    a = ap.parse_args()
    if a.selftest:
        return selftest()
    if not a.images:
        ap.print_usage()
        return 2
    if a.pair:
        if len(a.images) % 2:
            print('--pair wants on / off pairs')
            return 2
        res = [pair(a.images[i], a.images[i + 1]) for i in range(0, len(a.images), 2)]
        return 0 if all(res) else 1
    for p in a.images:
        sh, v, h = shares(Image.open(p))
        print(line(p, sh, v, h))
    return 0


if __name__ == '__main__':
    sys.exit(main())

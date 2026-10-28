#!/usr/bin/env python
# NIGHT72 builder B (slate B1, body72): THE KILN WYRM against its wall, on saved Player frames (1920x1080 PNG).
#   python -I Tools/night/n72-wyrmpx.py <label> <frame.png> <x0,y0,x1,y1> [<frame.png> <x0,y0,x1,y1> ...]     the box is the worm's region on that frame, read off the frame
#   python -I Tools/night/n72-wyrmpx.py --pair <label> <on_idle> <off_idle> <on_windup> <off_windup> <x0,y0,x1,y1>      the ON / OFF pair (the verdict's form: the glow as a delta)
#   python -I Tools/night/n72-wyrmpx.py --planted                                                        the meter's own planted case (exit 0 when it reads right)
# WHAT IT PRINTS per frame (the slate's bars, fixed BEFORE any body72 frame existed; never loosened after one):
#   luma     the median luma (Rec.709 on the sRGB values, 0-255) of the box, and of the WALL: the same columns, the bands MARGIN px above and below the box (4 px gap),
#            and their difference; the bar is >= LUMA_DIFF (25) in the idle frame
#   danger   the pixels of the box in the danger hue (345-35 deg, saturation >= 0.5, value >= 0.5): the tell glow shows in the windup frame and not in the idle one
#            (the bar: the windup frame's count is at least GLOW_RATIO x the idle frame's, and at least GLOW_MIN px)
# The box is the worm's own region: a median over it is the worm's body only when the box is drawn tight round the pieces; the lead draws it from the OFF frame of the
# same pose (the reel is deterministic: the same sim state at the same reel time), and uses the SAME box for the ON frame.
import sys
import numpy as np
from PIL import Image

LUMA_DIFF = 25.0
MARGIN = 60
GLOW_RATIO, GLOW_MIN = 3.0, 600


def load(path):
    return np.asarray(Image.open(path).convert('RGB'), dtype=np.float32)


def luma(a):
    return 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]


def danger(a):
    r, g, b = a[..., 0] / 255.0, a[..., 1] / 255.0, a[..., 2] / 255.0
    mx = np.maximum(np.maximum(r, g), b)
    mn = np.minimum(np.minimum(r, g), b)
    d = mx - mn
    s = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0)
    h = np.zeros_like(mx)
    m = d > 1e-6
    rc = (mx == r) & m
    gc = (mx == g) & m & ~rc
    bc = m & ~rc & ~gc
    h[rc] = 60 * (((g - b)[rc] / d[rc]) % 6)
    h[gc] = 60 * ((b - r)[gc] / d[gc] + 2)
    h[bc] = 60 * ((r - g)[bc] / d[bc] + 4)
    h = h % 360
    return ((h >= 345) | (h <= 35)) & (s >= 0.5) & (mx >= 0.5)


def measure(path, box):
    a = load(path)
    H, W = a.shape[:2]
    x0, y0, x1, y1 = [int(v) for v in box]
    x0, x1 = max(0, x0), min(W, x1)
    y0, y1 = max(0, y0), min(H, y1)
    L = luma(a)
    inside = L[y0:y1, x0:x1]
    up = L[max(0, y0 - 4 - MARGIN):max(0, y0 - 4), x0:x1]
    down = L[min(H, y1 + 4):min(H, y1 + 4 + MARGIN), x0:x1]
    wall = np.concatenate([up.ravel(), down.ravel()]) if up.size + down.size else np.array([0.0])
    mi, mw = float(np.median(inside)) if inside.size else 0.0, float(np.median(wall))
    dg = int(danger(a[y0:y1, x0:x1]).sum())
    return {'worm': mi, 'wall': mw, 'diff': abs(mi - mw), 'danger': dg}


def planted():
    """A frame with a mid-grey worm band on a dark wall (diff 50: reads PASS) and one whose band equals the wall (diff 0: reads FAIL); and a red glow band."""
    import tempfile, os
    d = tempfile.mkdtemp()
    wall = np.full((1080, 1920, 3), (40, 28, 22), np.uint8)
    a = wall.copy()
    a[500:560, 100:900] = (110, 96, 90)
    b = wall.copy()
    b[500:560, 100:900] = (40, 28, 22)
    c = wall.copy()
    c[500:560, 100:900] = (240, 60, 20)
    pa, pb, pc = (os.path.join(d, n) for n in ('lit.png', 'blend.png', 'glow.png'))
    Image.fromarray(a).save(pa)
    Image.fromarray(b).save(pb)
    Image.fromarray(c).save(pc)
    box = (100, 500, 900, 560)
    ma, mb, mc = measure(pa, box), measure(pb, box), measure(pc, box)
    print('planted lit   :', ma)
    print('planted blend :', mb)
    print('planted glow  :', mc)
    ok = ma['diff'] >= LUMA_DIFF and mb['diff'] < LUMA_DIFF and mc['danger'] >= GLOW_MIN and ma['danger'] == 0
    print('planted case', 'PASS' if ok else 'FAIL')
    return 0 if ok else 1


HUE_MAX, YELLOW_MAX = 35.0, 42.0   # (41.1 % rounded up to 42) the worm's glow hue (deg: the danger band's edge) and its share of the parry-yellow band (45-70 deg): no more than the PLAIN config's own at the same box
                                   # (t1 OFF read 41.1 %: the seams' HDR cores clip yellow in the plain game too; a first bar of 15 % failed the OFF frame itself, so it was set to the OFF reading
                                   # BEFORE any round-2 frame; evidence/wyrmpx-t1-hue.txt). Added 2026-10-27 after the t1 verdict.


def hue_stats(path, box):
    """The bright pixels of the box (value >= 0.47): their median hue (deg), the share of them in the parry-yellow band 45-70 deg, and their count."""
    import colorsys
    a = load(path)
    x0, y0, x1, y1 = [int(v) for v in box]
    p = a[y0:y1, x0:x1].reshape(-1, 3)
    b = p[p.max(axis=1) > 120]
    if len(b) == 0:
        return {'n': 0, 'median': 0.0, 'yellow': 0.0}
    hs = np.array([colorsys.rgb_to_hsv(*(c / 255.0))[0] * 360 for c in b])
    return {'n': int(len(b)), 'median': float(np.median(hs)), 'yellow': 100.0 * float(((hs >= 45) & (hs <= 70)).mean())}


def hue_mode(argv):
    """--hue <label> <frame.png> <x0,y0,x1,y1> [<frame.png> <x0,y0,x1,y1> ...]: the worm's lit column is in the danger band, not the parry tell's yellow (t1: the column read hue 46-49)."""
    label = argv[0]
    bad = 0
    for k in range(1, len(argv), 2):
        s = hue_stats(argv[k], argv[k + 1].split(','))
        ok = s['n'] > 0 and s['median'] <= HUE_MAX and s['yellow'] <= YELLOW_MAX
        bad += 0 if ok else 1
        name = argv[k].replace('\\', '/').split('/')[-1]
        print(f"{label}\t{name}\tbox {argv[k + 1]}\tbright px {s['n']}\tmedian hue {s['median']:.1f} deg (bar <= {HUE_MAX:.0f})\tyellow-band share {s['yellow']:.1f}% (bar <= {YELLOW_MAX:.0f}%)\t{'PASS' if ok else 'FAIL'}")
    return 0 if bad == 0 else 1


CHANGED_MIN_DELTA, CHANGED_HUE_MAX, CHANGED_YELLOW_MAX, CHANGED_MIN_PX = 40, 35.0, 15.0, 800


def changed_stats(on_path, off_path, box):
    """The pixels body72 CHANGED (ON against OFF, the same box and pose: the reel is deterministic): where any channel differs by more than CHANGED_MIN_DELTA and the ON pixel is bright
    (value >= 0.47). Their count, the median hue (deg) of the ON pixels and the share of them in the parry-yellow band 45-70 deg. This isolates the worm's change from the arena: the t2 box
    (858,590,912,760) holds the arena's own yellow pillar beam (x 882-905, bright yellow in BOTH frames), which made the box's yellow share 41 % in the plain frame and left the box-hue bar
    (yellow <= 42 %) no room: the first box-hue reading is kept in evidence/wyrmpx-t1-hue.txt and evidence/wyrmpx-t2-hue.txt; this mode was added after the t2 reading and before any round-3 frame."""
    import colorsys
    a, b = load(on_path), load(off_path)
    x0, y0, x1, y1 = [int(v) for v in box]
    pa, pb = a[y0:y1, x0:x1].reshape(-1, 3), b[y0:y1, x0:x1].reshape(-1, 3)
    diff = np.abs(pa - pb).max(axis=1) > CHANGED_MIN_DELTA
    sel = pa[diff & (pa.max(axis=1) > 120)]
    if len(sel) == 0:
        return {'n': 0, 'median': 0.0, 'yellow': 0.0}
    hs = np.array([colorsys.rgb_to_hsv(*(c / 255.0))[0] * 360 for c in sel])
    return {'n': int(len(sel)), 'median': float(np.median(hs)), 'yellow': 100.0 * float(((hs >= 45) & (hs <= 70)).mean())}


def changed_mode(argv):
    """--changed <label> <on.png> <off.png> <x0,y0,x1,y1>: the changed pixels' hue (bars: >= CHANGED_MIN_PX of them, median hue <= CHANGED_HUE_MAX deg, yellow-band share <= CHANGED_YELLOW_MAX %)."""
    label, on, off, box = argv[0], argv[1], argv[2], argv[3].split(',')
    s = changed_stats(on, off, box)
    ok = s['n'] >= CHANGED_MIN_PX and s['median'] <= CHANGED_HUE_MAX and s['yellow'] <= CHANGED_YELLOW_MAX
    print(f"{label}\tbox {','.join(box)}\tchanged bright px {s['n']} (bar >= {CHANGED_MIN_PX})\tmedian hue {s['median']:.1f} deg (bar <= {CHANGED_HUE_MAX:.0f})\tyellow-band share {s['yellow']:.1f}% (bar <= {CHANGED_YELLOW_MAX:.0f}%)\t{'PASS' if ok else 'FAIL'}")
    return 0 if ok else 1


GLOW_DELTA_MIN, GLOW_IDLE_MAX = 600, 300


def pair(argv):
    """--pair <label> <on_idle> <off_idle> <on_windup> <off_windup> <x0,y0,x1,y1>: the tell glow as a DIFFERENCE (the ON frame's danger-hue px minus the OFF frame's, the same
    box, the same pose: the reel is deterministic), because a kiln arena already holds danger-hued stripes and floor lines inside any box. The bars (fixed before any ON frame):
    windup delta >= GLOW_DELTA_MIN and idle delta <= GLOW_IDLE_MAX; the windup frame's body band at least LUMA_DIFF from its wall."""
    label, on_i, off_i, on_w, off_w, box = argv[0], argv[1], argv[2], argv[3], argv[4], argv[5].split(',')
    mi_on, mi_off, mw_on, mw_off = measure(on_i, box), measure(off_i, box), measure(on_w, box), measure(off_w, box)
    d_idle, d_wind = mi_on['danger'] - mi_off['danger'], mw_on['danger'] - mw_off['danger']
    print(f"{label}\tbox {','.join(box)}\tidle: danger px ON {mi_on['danger']} OFF {mi_off['danger']} delta {d_idle} (bar <= {GLOW_IDLE_MAX}) {'PASS' if d_idle <= GLOW_IDLE_MAX else 'FAIL'}")
    print(f"{label}\tbox {','.join(box)}\twindup: danger px ON {mw_on['danger']} OFF {mw_off['danger']} delta {d_wind} (bar >= {GLOW_DELTA_MIN}) {'PASS' if d_wind >= GLOW_DELTA_MIN else 'FAIL'}")
    print(f"{label}\twindup body vs wall: ON median {mw_on['worm']:.1f} wall {mw_on['wall']:.1f} diff {mw_on['diff']:.1f} (bar >= {LUMA_DIFF:.0f}) {'PASS' if mw_on['diff'] >= LUMA_DIFF else 'FAIL'}; OFF diff {mw_off['diff']:.1f}")
    print(f"{label}\tidle body vs wall: ON median {mi_on['worm']:.1f} wall {mi_on['wall']:.1f} diff {mi_on['diff']:.1f}; OFF diff {mi_off['diff']:.1f}")


if __name__ == '__main__':
    if len(sys.argv) >= 2 and sys.argv[1] == '--planted':
        sys.exit(planted())
    if len(sys.argv) >= 2 and sys.argv[1] == '--changed':
        sys.exit(changed_mode(sys.argv[2:]))
    if len(sys.argv) >= 2 and sys.argv[1] == '--hue':
        sys.exit(hue_mode(sys.argv[2:]))
    if len(sys.argv) >= 2 and sys.argv[1] == '--pair':
        pair(sys.argv[2:])
        sys.exit(0)
    if len(sys.argv) < 5 or (len(sys.argv) - 2) % 2:
        print(__doc__)
        sys.exit(2)
    label = sys.argv[1]
    rows = []
    for k in range(2, len(sys.argv), 2):
        p, box = sys.argv[k], sys.argv[k + 1].split(',')
        m = measure(p, box)
        rows.append((p, m))
        name = p.replace('\\', '/').split('/')[-1]
        print(f"{label}\t{name}\tbox {','.join(box)}\tworm median luma {m['worm']:6.1f}\twall {m['wall']:6.1f}\tdiff {m['diff']:5.1f} (bar >= {LUMA_DIFF:.0f}) {'PASS' if m['diff'] >= LUMA_DIFF else 'FAIL'}\tdanger px {m['danger']}")
    if len(rows) == 2:
        # the tell glow: the SAME box (the first frame's) in both frames; the idle frame first, the windup frame second
        box1 = sys.argv[3].split(',')
        idle = rows[0][1]['danger']
        wind = measure(rows[1][0], box1)['danger']
        ok = wind >= GLOW_MIN and wind >= GLOW_RATIO * max(1, idle)
        print(f"{label}\tTELL GLOW in the first box (idle frame, windup frame)\tidle {idle} px, windup {wind} px (bar: windup >= {GLOW_MIN} and >= {GLOW_RATIO:.0f} x idle) {'PASS' if ok else 'FAIL'}")

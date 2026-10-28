#!/usr/bin/env python
# NIGHT72 builder B (slate B2, bossfx72): THE CROWN's fight frames, measured. A frame meter; it reads saved Player frames (1920x1080 PNG) and counts pixels.
#   python -I Tools/night/n72-crownfx.py <label> <frame.png> [<frame.png> ...]      (the crown's five boss63 frames: idle, flinch, tell, windup, stagger)
#   python -I Tools/night/n72-crownfx.py --planted                                   (the meter's own planted case: synthetic frames, exit 0 when it reads right)
# WHAT IT COUNTS (the slate's three doneWhen lines; numbers fixed BEFORE any bossfx72 frame exists, never loosened after one):
#  1. EFFECT COVERAGE at the crosshair: the share of the DISC of DISC_R px round the crosshair (960, 540) that is a saturated bright glow (value >= GLOW_V,
#     saturation >= GLOW_S, hue cyan 160-200 or red-orange 340-40 deg). The viewmodel's corner (x >= GUN_X and y >= GUN_Y: the gun and its cyan trim) is cut out
#     of the disc and of its area. The slate says "no effect covers > 15 % of the frame inside 300 px of the crosshair": a 300 px disc is 13.6 % of a 1080p
#     frame, so that line read literally can never fail; it is read here as 15 % OF THE DISC (the bar COVER_MAX).
#  2. THE CROWN'S BODY by its own colour band: with bossfx72 the ring, its blades and its rim are CYAN (usable, rule 8) in all five states (idle, flinch, a red tell,
#     a wind-up, the open window); the cyan band is hue 165-200, sat >= 0.5, value >= 0.45. The danger band (hue 345-35, same sat and value) is printed, not counted.
#     A MASS is what survives an opening of the band by MASS_OPEN px (an effect burst, the danger column, a window's flood: wide); the ring's segments (about 30 px thick
#     at 6.6 m) and its blades (about 23 px) do not survive it. The BODY is the band outside the mass (the mass grown by 6 px), in pieces of >= PIECE_MIN px. HUD strips
#     (the boss bar, the bottom HUD) and the viewmodel's corner are cut out. The Crown is VISIBLE in a frame when its body holds >= BODY_MIN px AND the coverage of 1.
#     is within the bar (a burst over the crosshair hides the ring even where some of its pixels show). The ledger holds the n71-polish8 (bossfx72 off) reading.
#  The meter prints per frame: coverage %, body px by band, mass px by band, visible yes / no; and the totals "visible k of n, worst coverage x %".
import sys
import cv2
import numpy as np
from PIL import Image
from scipy import ndimage

W, H = 1920, 1080
CX, CY = 960, 540
DISC_R = 300
AIM_OPEN = 41                     # px: the opening square for the aim disc's burst mass (a gem's blotch survives it, a ring segment or a blade does not)
AIM_R, AIM_MAX = 120, 5.0        # the aim point's own disc and its bar (percent glow): added after the t1 verdict, before the new ON frames
GUN_X, GUN_Y = 1020, 600          # the viewmodel's corner (lower right): cut out
GLOW_V, GLOW_S = 0.75, 0.55
COVER_MAX = 15.0                  # percent of the disc
CONE_MAX = 20.0                   # percent of the frame: the tell's danger-hue share in the tell and windup frames (the slate: the tell cone <= 20 % of the frame)
BODY_MIN = 15000                  # px of the cyan band outside the mass (the ring's segments and 20 blades at 6.6 m: tens of thousands; the floor's cyan trims: about 5000)
PIECE_MIN, MASS_OPEN = 300, 61    # a piece smaller than this is noise; the opening square that keeps only wide masses
HUD_TOP, HUD_BOTTOM = 175, 950    # y of the boss bar's end, the bottom HUD's start (the ring may reach under them; the HUD's own cyan is not the crown's)


def hsv(img):
    a = np.asarray(img.convert('RGB'), dtype=np.float32) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = a.max(axis=2)
    mn = a.min(axis=2)
    d = mx - mn
    s = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0)
    h = np.zeros_like(mx)
    m = d > 1e-6
    rc = (mx == r) & m
    gc = (mx == g) & m & ~rc
    bc = m & ~rc & ~gc
    h[rc] = (60 * (((g - b)[rc] / d[rc]) % 6))
    h[gc] = 60 * ((b - r)[gc] / d[gc] + 2)
    h[bc] = 60 * ((r - g)[bc] / d[bc] + 4)
    h = h % 360
    return h, s, mx


def hue_in(h, lo, hi):
    return (h >= lo) & (h <= hi) if lo <= hi else (h >= lo) | (h <= hi)


def measure(path):
    img = Image.open(path).convert('RGB')
    if img.size != (W, H):
        img = img.resize((W, H))
    h, s, v = hsv(img)
    yy, xx = np.mgrid[0:H, 0:W]
    disc = ((xx - CX) ** 2 + (yy - CY) ** 2 <= DISC_R ** 2) & ~((xx >= GUN_X) & (yy >= GUN_Y))
    glow = (v >= GLOW_V) & (s >= GLOW_S) & (hue_in(h, 160, 200) | hue_in(h, 340, 40))
    cover = 100.0 * (glow & disc).sum() / max(1, disc.sum())
    # 1b. AIM CLEARANCE (added 2026-10-27 12:25 after the t1 verdict "the bursts must come off the crosshair"; the bar is fixed before the new frames): the same glow share inside the
    # small disc of AIM_R px round the crosshair: the aim point itself stays clear of a burst
    # A BURST is a MASS (wide), not a line: the share counts only glow that survives an opening by AIM_OPEN px (a ring's segment is ~30 px, a blade ~23 px: lines, the Crown's body; a gem's
    # blotch of 4-5 touching gems, a halo annulus: masses). The first reading (plain glow share, 27.3 % on bfalone-sp idle) is in evidence/crownfx-t1-bfalone-bpoff.txt.
    aimd = ((xx - CX) ** 2 + (yy - CY) ** 2 <= AIM_R ** 2)
    gopen = cv2.morphologyEx(glow.astype(np.uint8), cv2.MORPH_OPEN, np.ones((AIM_OPEN, AIM_OPEN), np.uint8)).astype(bool)
    aim = 100.0 * (gopen & aimd).sum() / max(1, aimd.sum())
    hud = (yy < HUD_TOP) | (yy > HUD_BOTTOM) | ((xx >= GUN_X) & (yy >= GUN_Y))
    # 3. the tell's CONE: the share of the frame in the danger hue (345-35 deg, sat >= 0.5, value >= 0.3: the column's translucent red over the arena), HUD and gun cut out
    danger_any = hue_in(h, 345, 35) & (s >= 0.5) & (v >= 0.3) & ~((yy < HUD_TOP) | (yy > HUD_BOTTOM) | ((xx >= GUN_X) & (yy >= GUN_Y)))
    out = {'cover': cover, 'aim': aim, 'cone': 100.0 * danger_any.sum() / (W * H)}
    for name, (lo, hi) in (('cyan', (165, 200)), ('danger', (345, 35))):
        band = hue_in(h, lo, hi) & (s >= 0.5) & (v >= 0.45) & ~hud
        # the MASS: what survives an opening by a MASS_OPEN px square (a burst, the danger column, a window's flood: wide); the ring's segments (about 30 px thick)
        # and its blades (about 23 px) do not survive it. The BODY: the band outside the mass (grown by 6 px), in pieces of at least PIECE_MIN px.
        b8 = band.astype(np.uint8)
        opened = cv2.morphologyEx(b8, cv2.MORPH_OPEN, np.ones((MASS_OPEN, MASS_OPEN), np.uint8))
        mass = int(opened.sum())
        rest = band & (cv2.dilate(opened, np.ones((13, 13), np.uint8)) == 0)
        lab, n = ndimage.label(rest)
        sizes = ndimage.sum(rest, lab, index=np.arange(1, n + 1)) if n else np.array([])
        body = int(sizes[sizes >= PIECE_MIN].sum()) if n else 0
        out[name] = (body, mass)
    # visible = the body is there AND no burst drowns the crosshair (the coverage bar): the Crown's own band while it may be passed (bossfx72's contract) is cyan; the
    # danger band is printed, not counted
    out['visible'] = out['cyan'][0] >= BODY_MIN and cover <= COVER_MAX
    return out


def report(label, paths):
    rows = []
    for p in paths:
        m = measure(p)
        rows.append(m)
        name = p.replace('\\', '/').split('/')[-1]
        print(f"{label}\t{name}\tcover {m['cover']:5.1f}%\taim {m['aim']:5.1f}%\tdanger-hue share {m['cone']:5.1f}%\tcyan body {m['cyan'][0]:6d} mass {m['cyan'][1]:7d}\tdanger body {m['danger'][0]:6d} mass {m['danger'][1]:7d}\tvisible {'yes' if m['visible'] else 'NO'}")
    k = sum(1 for m in rows if m['visible'])
    worst = max((m['cover'] for m in rows), default=0.0)
    aimw = max((m['aim'] for m in rows), default=0.0)
    print(f"{label}\tAIM CLEARANCE\tworst glow share of the {AIM_R} px disc round the crosshair {aimw:.1f}% (bar {AIM_MAX:.0f}%) {'PASS' if aimw <= AIM_MAX else 'FAIL'}")
    cone = max((m['cone'] for m, p in zip(rows, paths) if 'tell' in p.lower() or 'windup' in p.lower()), default=0.0)
    print(f"{label}\tTOTAL\tvisible {k} of {len(rows)}\tworst coverage {worst:.1f}% (bar {COVER_MAX:.0f}%)\tcoverage {'PASS' if worst <= COVER_MAX else 'FAIL'}\tbody {'PASS' if k >= 4 and len(rows) >= 5 else 'FAIL'} (bar 4 of 5)"
          f"\ttell frames' danger-hue share max {cone:.1f}% (bar {CONE_MAX:.0f}%) {'PASS' if cone <= CONE_MAX else 'FAIL'}")
    return rows


def planted():
    """The meter's own planted case: a frame with a solid cyan annulus (a burst) and one with a thin cyan ring of separate pieces. The meter must call the
    first a mass (not visible, coverage over the bar) and the second the body (visible, coverage under the bar)."""
    import tempfile, os
    yy, xx = np.mgrid[0:H, 0:W]
    r = np.sqrt((xx - 960.0) ** 2 + (yy - 780.0) ** 2)
    bg = np.zeros((H, W, 3), np.uint8)
    bg[...] = (20, 24, 20)
    burst = bg.copy()
    burst[(r > 120) & (r < 420)] = (0, 230, 255)                       # a 300 px-wide solid cyan annulus: a burst
    ring = bg.copy()
    ang = np.arctan2(yy - 780.0, xx - 960.0)
    seg = ((ang + np.pi) / (2 * np.pi) * 16).astype(int) % 2 == 0
    ring[(r > 280) & (r < 320) & seg] = (0, 230, 255)                  # 8 separate 40 px-thick arcs of a ring: the body
    d = tempfile.mkdtemp()
    pa, pb = os.path.join(d, 'burst.png'), os.path.join(d, 'ring.png')
    Image.fromarray(burst).save(pa)
    Image.fromarray(ring).save(pb)
    a, b = measure(pa), measure(pb)
    print('planted burst:', a)
    print('planted ring :', b)
    ok = (not a['visible']) and a['cover'] > COVER_MAX and b['visible'] and b['cover'] <= COVER_MAX
    print('planted case', 'PASS' if ok else 'FAIL')
    return 0 if ok else 1


if __name__ == '__main__':
    if len(sys.argv) >= 2 and sys.argv[1] == '--planted':
        sys.exit(planted())
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(2)
    report(sys.argv[1], sys.argv[2:])

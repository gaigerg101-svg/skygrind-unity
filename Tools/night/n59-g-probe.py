# NIGHT59 builder G (G1 kill55's marks after the flash): the planted probe on EXISTING a258 frames (no Player).
# Reads <dir>/{g1on,g1off}/a258/shot-N55-K-{hit-before,hit-next,flash-end,hit-after}.jpg (default Logs/feel/n58-freezek).
# Prints, per side and frame, the reel's MAIN replica (FeelReel.N55.Kill.cs K55Marks: of the before frame's warm body pixels in the body
# rect, sampled every 2 px, the share that changed by > 0.12 in a channel AND is dark, V < 0.12), the mean V of the 7x7 probe at the mark's
# centre (a258's line: px (958, 551) bottom-up), and the mark's visible footprint (on darker than off by > 0.12 at the same pixel).
# The planted frame: the off side's flash-end frame with the mark painted at its OWN colours (the hole #070303 inside Kill55HoleR, the
# crack #160807 around it) at the probe point, i.e. the mark as it would land if drawn after the fog and the grade; the same MAIN replica
# must read it (>= 5 %), else the measure, not the draw, is the gap.
# Usage: python -I Tools/night/n59-g-probe.py [dir [onSide offSide [probeX probeY]]] (the probe px: a258's line, "the mark's centre at px")
import sys
import numpy as np
from PIL import Image

D = sys.argv[1] if len(sys.argv) > 1 else 'Logs/feel/n58-freezek'
ON, OFF = (sys.argv[2], sys.argv[3]) if len(sys.argv) > 3 else ('g1on', 'g1off')  # the sides (e.g. g1on g1old: the NIGHT58 draw as the off side)
W, H = 1920, 1080
PX, PY = (int(sys.argv[4]), H - int(sys.argv[5])) if len(sys.argv) > 5 else (958, H - 551)  # a258's probe px from its line (bottom-up y -> rows)
RECT_W, RECT_H = 113, 181  # a258's body rect (the line: 113x181 px)
DELTA, MARKV, WANT = 0.12, 0.12, 0.05  # UnityTuning.Kill55ReelPixelDelta, Kill55ReelMarkV, Kill55ReelMarkNextShare
RADIUS_M, HOLE_M, CRACK_M = 0.4, 0.21, 0.165 + 0.36 * 0.5  # husk.radius (config), Kill55HoleR, Kill55ScorchR + half Kill55CrackLen


def load(side, name):
    return np.asarray(Image.open(f'{D}/{side}/a258/shot-N55-K-{name}.jpg').convert('RGB')).astype(float) / 255.0


def hsv(a):
    v = a.max(2)
    mn = a.min(2)
    d = v - mn
    s = np.where(v > 0, d / np.maximum(v, 1e-6), 0)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    h = np.where(d <= 1e-6, 0, np.where(v == r, ((g - b) / np.maximum(d, 1e-6)) % 6, np.where(v == g, (b - r) / np.maximum(d, 1e-6) + 2, (r - g) / np.maximum(d, 1e-6) + 4))) / 6.0
    return h, s, v


def body_rect(next_frame):
    # the victim's body: the flash's white on the hit's next frame (S < 0.2, V > 0.85) near the probe point
    _, s, v = hsv(next_frame)
    m = (s < 0.2) & (v > 0.85)
    ys, xs = np.nonzero(m[PY - 200:PY + 200, PX - 120:PX + 120])
    cx = int(np.median(xs)) + PX - 120
    bottom = ys.max() + PY - 200
    top = ys.min() + PY - 200
    hpx = bottom - top
    # K55BodyRect: feet + 0.15 h .. 0.8 h, the half width = the radius on screen
    y0 = int(bottom - 0.8 * hpx)
    return cx - RECT_W // 2, y0, cx + RECT_W // 2 + 1, y0 + RECT_H, hpx


def main_replica(before, after, rect):
    x0, y0, x1, y1, _ = rect
    b = before[y0:y1:2, x0:x1:2]
    a = after[y0:y1:2, x0:x1:2]
    h, s, v = hsv(b)
    warm = (s > 0.35) & (v > 0.15) & ((h < 0.12) | (h > 0.94))
    changed = warm & (np.abs(b - a).max(2) > DELTA)
    _, _, v2 = hsv(a)
    dark = changed & (v2 < MARKV)
    n = warm.sum()
    med = float(np.median(v2[changed])) if changed.any() else -1
    return dark.sum() / max(n, 1), int(n), med


def patch_v(a):
    _, _, v = hsv(a[PY - 3:PY + 4, PX - 3:PX + 4])
    return float(v.mean())


def plant(frame, rect):
    out = frame.copy()
    pxm = RECT_W / 2 / RADIUS_M  # px per m at the husk (the rect's half width is the radius)
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.hypot(xx - PX, yy - PY)
    crack = np.array([0x16, 0x08, 0x07]) / 255.0
    hole = np.array([0x07, 0x03, 0x03]) / 255.0
    # the crack under the hole: a scorch ring of thin strips (half its area: alternate 30-degree sectors)
    ang = (np.degrees(np.arctan2(yy - PY, xx - PX)) + 360) % 60
    out[(d <= CRACK_M * pxm) & (d > HOLE_M * pxm) & (ang < 12)] = crack
    out[d <= HOLE_M * pxm] = hole
    return out, HOLE_M * pxm


print(f'== n59-g-probe on {D} (the a258 frames; the reel line: on 0.69 %, off 0.41 % of the body at the flash end)')
res = {}
for side in (ON, OFF):
    before, nxt = load(side, 'hit-before'), load(side, 'hit-next')
    rect = body_rect(nxt)
    for name in ('flash-end', 'hit-after'):
        fr = load(side, name)
        share, n, med = main_replica(before, fr, rect)
        res[(side, name)] = fr
        print(f'{side} {name}: MAIN replica {share * 100:.2f} % of {n} body px (changed median V {med:.2f}) | probe V {patch_v(before):.2f} before -> {patch_v(fr):.2f} | rect x {rect[0]}-{rect[2]} y {rect[1]}-{rect[3]} (body {rect[4]} px tall)')
    res[(side, 'rect')] = rect
    res[(side, 'before')] = before
for name in ('flash-end', 'hit-after'):
    on, off = res[(ON, name)], res[(OFF, name)]
    x0, y0, x1, y1, _ = res[(ON, 'rect')]
    _, _, von = hsv(on[y0:y1, x0:x1])
    _, _, voff = hsv(off[y0:y1, x0:x1])
    foot = (voff - von) > DELTA
    print(f'footprint {name}: on darker than off by > {DELTA} on {foot.mean() * 100:.2f} % of the rect; those px median V on {float(np.median(von[foot])) if foot.any() else -1:.2f} (off {float(np.median(voff[foot])) if foot.any() else -1:.2f}); V < {MARKV} among them {(von[foot] < MARKV).mean() * 100 if foot.any() else 0:.1f} %')
pl, rpx = plant(res[(OFF, 'flash-end')], res[(OFF, 'rect')])
share, n, med = main_replica(res[(OFF, 'before')], pl, res[(OFF, 'rect')])
print(f'PLANTED (off flash-end + the mark at its own colours, hole r {rpx:.0f} px): MAIN replica {share * 100:.2f} % (want >= {WANT * 100:.0f} %) -> {"GREEN" if share >= WANT else "RED"} | probe V {patch_v(pl):.2f}')
share0, _, _ = main_replica(res[(OFF, 'before')], res[(OFF, 'flash-end')], res[(OFF, 'rect')])
print(f'UNPLANTED (the same frame): MAIN replica {share0 * 100:.2f} % -> {"GREEN" if share0 >= WANT else "RED"}')

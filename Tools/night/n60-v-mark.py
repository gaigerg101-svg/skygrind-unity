# NIGHT60 builder V (B2, kill55's marks near-black): why a mark drawn #070303 / #160807 reads V ~0.5 on the a258 frames, measured on EXISTING
# frames (no Player). The cause: the enemy x-ray (EnemyViews.MakeXray: SKYGRIND/Enemy's SRPDefaultUnlit pass, ZTest Greater, ZWrite Off, normal
# blending, queue Transparent + UnityTuning.EnemyXrayQueueOffset; the player's setting "xray", true by default) draws the husk's body wherever
# something NEARER than the body hides it. The marks write depth in front of the body (NIGHT59's Kill59Mark pull, NIGHT58's lift), and they draw
# in the opaque queue (Geometry+10), so the x-ray then lays aim.xray.color at aim.xray.alpha (+ its rim term) over every mark pixel.
# This meter prints:
#   1. the model: each mark colour (UnityTuning.N55K.cs) under the x-ray (config aim.xray.*), in sRGB before the post chain;
#   2. the frame: the on side's mark pixels (on darker than off by > the reel's delta, at +0.5 s and at the flash end), their median H S V;
#   3. the planted cases (L-111: at the colour the draw really shows): the off flash-end frame with the mark planted (n59-g-probe.py's shape)
#      (a) at the x-ray composite: the reel's MAIN replica must read RED (the reel's 0.46 %), (b) at the marks' own colours (what a draw AFTER the
#      x-ray shows): GREEN (>= 5 %), and the unplanted frame RED.
# Usage, from the tree root: python -I Tools/night/n60-v-mark.py [dir [onSide offSide [probeX probeY]]] (dir: a258's tag dir, e.g. the MAIN repo's
# Logs/feel/n59-b2; probe px from a258's line "the mark's centre at px", bottom-up y)
import json
import re
import sys
import numpy as np
from PIL import Image

D = sys.argv[1] if len(sys.argv) > 1 else 'Logs/feel/n59-b2'
ON, OFF = (sys.argv[2], sys.argv[3]) if len(sys.argv) > 3 else ('g1on', 'g1off')
W, H = 1920, 1080
PX, PY = (int(sys.argv[4]), H - int(sys.argv[5])) if len(sys.argv) > 5 else (958, H - 551)
RECT_W, RECT_H = 113, 181  # a258's body rect (its line: 113x181 px)

cfg = json.load(open('PortExport/config.json', encoding='utf-8'))
TUNE = open('Assets/Scripts/Game/UnityTuning.N55K.cs', encoding='utf-8').read()


def tune(name):
    m = re.search(name + r'\s*=\s*(0x[0-9A-Fa-f]+|[0-9.]+)', TUNE)
    if m is None:
        raise SystemExit(f'not found in UnityTuning.N55K.cs: {name}')
    s = m.group(1)
    return int(s, 16) if s.startswith('0x') else float(s)


DELTA, MARKV, WANT = tune('Kill55ReelPixelDelta'), tune('Kill55ReelMarkV'), tune('Kill55ReelMarkNextShare')
HOLE_M, SCORCH_M, CRACK_LEN = tune('Kill55HoleR'), tune('Kill55ScorchR'), tune('Kill55CrackLen')
CRACK_M = SCORCH_M + CRACK_LEN * 0.5
RADIUS_M = cfg['husk']['radius']  # husk.radius (Kill55Damage reads cfg.Num("husk.radius"))
XR = cfg['aim']['xray']
XCOL, XALPHA, XEDGE = XR['color'], XR['alpha'], XR['edge']


def hex_rgb(h):
    return np.array([(h >> 16) & 255, (h >> 8) & 255, h & 255]) / 255.0


def to_lin(c):
    c = np.asarray(c, float)
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def to_srgb(c):
    c = np.clip(np.asarray(c, float), 0, 1)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * c ** (1 / 2.4) - 0.055)


def hsv1(c):
    r, g, b = c
    v = max(c)
    d = v - min(c)
    s = d / v if v > 0 else 0
    if d <= 1e-6:
        h = 0
    elif v == r:
        h = ((g - b) / d) % 6
    elif v == g:
        h = (b - r) / d + 2
    else:
        h = (r - g) / d + 4
    return h * 60, s, v


def xray_over(mark_srgb, rim):
    # SKYGRIND/Enemy's x-ray fragment (xray.js): a = clamp((alpha + rim edge alpha) fade boost, 0, 0.9), rgb = color (1 + rim edge 0.6);
    # Blend SrcAlpha OneMinusSrcAlpha in linear; fade 1 and boost 1 (a husk a few metres away)
    a = min(max(XALPHA + rim * XEDGE * XALPHA, 0), 0.9)
    x = to_lin(hex_rgb(XCOL)) * (1 + rim * XEDGE * 0.6)
    return to_srgb(a * x + (1 - a) * to_lin(mark_srgb)), a


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
    # n59-g-probe.py's: the flash's white on the hit's next frame near the probe; K55BodyRect's feet + 0.15 h .. 0.8 h, the radius as the half width
    _, s, v = hsv(next_frame)
    m = (s < 0.2) & (v > 0.85)
    ys, xs = np.nonzero(m[PY - 200:PY + 200, PX - 120:PX + 120])
    cx = int(np.median(xs)) + PX - 120
    bottom = ys.max() + PY - 200
    hpx = bottom - (ys.min() + PY - 200)
    y0 = int(bottom - 0.8 * hpx)
    return cx - RECT_W // 2, y0, cx + RECT_W // 2 + 1, y0 + RECT_H


def main_replica(before, after, rect):
    # FeelReel.N55.Kill.cs K55Marks: of the before frame's warm body px (every 2 px), the share changed by > DELTA in a channel AND V < MARKV
    x0, y0, x1, y1 = rect
    b = before[y0:y1:2, x0:x1:2]
    a = after[y0:y1:2, x0:x1:2]
    h, s, v = hsv(b)
    warm = (s > 0.35) & (v > 0.15) & ((h < 0.12) | (h > 0.94))
    changed = warm & (np.abs(b - a).max(2) > DELTA)
    _, _, v2 = hsv(a)
    return (changed & (v2 < MARKV)).sum() / max(warm.sum(), 1)


def plant(frame, hole_c, crack_c):
    # n59-g-probe.py's shape: the hole disc (Kill55HoleR) and the crack's strips (alternate 12 of 60 degrees) out to the scorch + half a crack
    out = frame.copy()
    pxm = RECT_W / 2 / RADIUS_M
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.hypot(xx - PX, yy - PY)
    ang = (np.degrees(np.arctan2(yy - PY, xx - PX)) + 360) % 60
    out[(d <= CRACK_M * pxm) & (d > HOLE_M * pxm) & (ang < 12)] = crack_c
    out[d <= HOLE_M * pxm] = hole_c
    return out


def fmt(c):
    h, s, v = hsv1(c)
    return '#%02X%02X%02X (H %.0f S %.2f V %.2f)' % (round(c[0] * 255), round(c[1] * 255), round(c[2] * 255), h, s, v)


hole, crack = hex_rgb(tune('Kill55HoleHex')), hex_rgb(tune('Kill55CrackHex'))
print(f'== n60-v-mark on {D} ({ON} vs {OFF}; the reel MAIN counts changed px with V < {MARKV}; want >= {WANT * 100:.0f} %)')
print(f'1. the model (config aim.xray: color #{XCOL:06X} alpha {XALPHA} edge {XEDGE}; sRGB before the post chain):')
for name, c in (('hole', hole), ('crack', crack)):
    c0, a0 = xray_over(c, 0.0)
    c1, a1 = xray_over(c, 0.25)
    print(f'   {name} {fmt(c)} -> under the x-ray, facing (rim 0, a {a0:.2f}): {fmt(c0)}; toward the edge (rim 0.25, a {a1:.2f}): {fmt(c1)}')
rects = {}
frames = {}
for side in (ON, OFF):
    frames[(side, 'before')] = load(side, 'hit-before')
    rects[side] = body_rect(load(side, 'hit-next'))
    for name in ('flash-end', 'hit-after'):
        frames[(side, name)] = load(side, name)
x0, y0, x1, y1 = rects[ON]
print('2. the frame: the mark px (on darker than off by > the delta inside the body rect), their median colour:')
for name in ('hit-after', 'flash-end'):
    on, off = frames[(ON, name)][y0:y1, x0:x1], frames[(OFF, name)][y0:y1, x0:x1]
    foot = (off.max(2) - on.max(2)) > DELTA
    med = np.median(on[foot], axis=0) if foot.any() else np.zeros(3)
    dark = (on.max(2)[foot] < MARKV).mean() * 100 if foot.any() else 0
    print(f'   {name}: {foot.mean() * 100:.2f} % of the rect, median {fmt(med)}; V < {MARKV} among them {dark:.1f} %')
fr = frames[(ON, 'hit-after')]
print(f'   the whole {ON} hit-after frame: 1st percentile V {np.percentile(fr.max(2), 1):.3f} (the post keeps black black)')
before, fe = frames[(OFF, 'before')], frames[(OFF, 'flash-end')]
rect = rects[OFF]
hx, _ = xray_over(hole, 0.0)
cx_, _ = xray_over(crack, 0.0)
p_x = main_replica(before, plant(fe, hx, cx_), rect)
p_own = main_replica(before, plant(fe, hole, crack), rect)
p_none = main_replica(before, fe, rect)
print('3. planted on the off flash-end frame (MAIN replica):')
print(f'   (a) the mark at the x-ray composite (today\'s draw: the x-ray over it): {p_x * 100:.2f} % -> {"GREEN" if p_x >= WANT else "RED"} (must be RED: the reel read 0.46 %)')
print(f'   (b) the mark at its own colours (a draw AFTER the x-ray): {p_own * 100:.2f} % -> {"GREEN" if p_own >= WANT else "RED"} (must be GREEN)')
print(f'   (c) unplanted: {p_none * 100:.2f} % -> {"GREEN" if p_none >= WANT else "RED"} (must be RED)')
ok = p_x < WANT and p_own >= WANT and p_none < WANT
print(f'meter {"OK" if ok else "BROKEN"}: the x-ray composite misses the MAIN, the own colours meet it')

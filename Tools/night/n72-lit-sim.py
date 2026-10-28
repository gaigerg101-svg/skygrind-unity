"""NIGHT72 builder L (L3, lit72): the offline model of lit72's display-space shadow lift, run on the Player frames that already exist, so the
parameters are chosen from numbers and the Player's pair only confirms them (no Player in a builder's tree).
The lift is the one Assets/Shaders/Resources/Crt29.shader applies (Lit72 block) before the CRT's scanlines: per pixel, with l = the pixel's luma
(0.299 R + 0.587 G + 0.114 B on the display values, the same weights PIL's "L" mode uses, i.e. the measure sheet.py's "dark < 30" reads):
    w = 1 - smoothstep(l0, l1, l)                      1 in the shadows, 0 from l1 up (a bright pixel is left alone)
    k = min(kmax, (l + A * w) / max(l, 1e-4))          the luma added is A * w, taken in the pixel's OWN hue (the colour is scaled, never replaced)
    rgb' = min(1, rgb * k)
so a boss's arena keeps its palette (hue unchanged except where a channel clips at 1) and only its shadows come up.
Frames are 1920x1080 PNGs with the HUD and the gun drawn in; the lift runs on the world camera's colour only, so --mask leaves those rectangles
out of the lift (a conservative reading: their dark pixels stay dark).
Usage (python -I):
  python -I Tools/night/n72-lit-sim.py <png>... [--A 0.12] [--l0 0.12] [--l1 0.42] [--kmax 9] [--out DIR] [--nomask]
(--out writes one before | after JPG per frame; the defaults are UnityTuning.L72LitA / L0 / L1 / KMax)
Prints per frame: mean luma, dark share (< 30 of 255), blown share (> 235), the chroma-weighted circular mean hue in degrees, before and after, and the
hue shift; then a SUMMARY line (worst dark share after, worst hue shift, worst blown share).
"""
import math
import os
import sys

import numpy as np
from PIL import Image

HUD_RECTS = [  # (x0, y0, x1, y1) at 1920 x 1080: where the HUD and the gun draw over the world (not lifted by the pass); read off the boss63 frames
    (200, 10, 1100, 70),    # the boss bar and its name
    (1550, 10, 1900, 100),  # the counters, the weapon letter
    (20, 930, 450, 1060),   # health
    (1080, 600, 1500, 1080),  # the gun (the viewmodel camera draws after the pass): its box, wider than the gun itself
    (1500, 900, 1900, 1060),  # the weapon panel
]


def luma(a):
    return 0.299 * a[..., 0] + 0.587 * a[..., 1] + 0.114 * a[..., 2]


def smoothstep(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def lift(a, A, l0, l1, kmax):
    """a: float32 [h, w, 3] in 0-1 display values. Returns the lifted copy."""
    l = luma(a)
    w = 1.0 - smoothstep(l0, l1, l)
    k = np.minimum(kmax, (l + A * w) / np.maximum(l, 1e-4))
    return np.minimum(1.0, a * k[..., None])


def mean_hue_deg(a):
    """The chroma-weighted circular mean hue of a frame, in degrees (0 red, 120 green, 240 blue); weights = chroma * value, so grey and black count for little."""
    mx = a.max(axis=-1)
    mn = a.min(axis=-1)
    c = mx - mn
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    h = np.zeros_like(mx)
    m = c > 1e-6
    rc = (mx == r) & m
    gc = (mx == g) & m & ~rc
    bc = m & ~rc & ~gc
    h[rc] = ((g - b)[rc] / c[rc]) % 6
    h[gc] = (b - r)[gc] / c[gc] + 2
    h[bc] = (r - g)[bc] / c[bc] + 4
    ang = h * (math.pi / 3)
    wt = c * mx
    x = (wt * np.cos(ang)).sum()
    y = (wt * np.sin(ang)).sum()
    return (math.degrees(math.atan2(y, x)) % 360.0)


def hue_diff(a, b):
    d = abs(a - b) % 360.0
    return min(d, 360.0 - d)


def measure(a):
    l = luma(a) * 255.0
    return l.mean(), 100.0 * (l < 30).mean(), 100.0 * (l > 235).mean(), mean_hue_deg(a)


def mask_for(h, w):
    m = np.ones((h, w), bool)  # True where the lift applies
    sx, sy = w / 1920.0, h / 1080.0
    for (x0, y0, x1, y1) in HUD_RECTS:
        m[int(y0 * sy):int(y1 * sy), int(x0 * sx):int(x1 * sx)] = False
    return m


def main(argv):
    A, l0, l1, kmax = 0.12, 0.12, 0.42, 9.0  # UnityTuning.L72LitA / L72LitL0 / L72LitL1 / L72LitKMax
    out = None
    nomask = False
    paths = []
    i = 0
    while i < len(argv):
        a = argv[i]
        if a == "--A":
            A = float(argv[i + 1]); i += 1
        elif a == "--l0":
            l0 = float(argv[i + 1]); i += 1
        elif a == "--l1":
            l1 = float(argv[i + 1]); i += 1
        elif a == "--kmax":
            kmax = float(argv[i + 1]); i += 1
        elif a == "--out":
            out = argv[i + 1]; i += 1
        elif a == "--nomask":
            nomask = True
        else:
            paths.append(a)
        i += 1
    if out:
        os.makedirs(out, exist_ok=True)
    print("MODEL A %.3f l0 %.3f l1 %.3f kmax %.2f mask %s" % (A, l0, l1, kmax, "off" if nomask else "HUD + gun rects"))
    worst_dark = worst_hue = worst_blown = 0.0
    for p in paths:
        a = np.asarray(Image.open(p).convert("RGB")).astype(np.float32) / 255.0
        h, w = a.shape[:2]
        lifted = lift(a, A, l0, l1, kmax)
        if not nomask:
            m = mask_for(h, w)
            lifted = np.where(m[..., None], lifted, a)
        m0, d0, b0, h0 = measure(a)
        m1, d1, b1, h1 = measure(lifted)
        dh = hue_diff(h0, h1)
        worst_dark = max(worst_dark, d1)
        worst_hue = max(worst_hue, dh)
        worst_blown = max(worst_blown, b1)
        name = os.path.basename(p)
        print("%s | mean %.1f -> %.1f | dark<30 %.1f%% -> %.1f%% | blown>235 %.2f%% -> %.2f%% | hue %.1f -> %.1f (shift %.1f deg)" % (name, m0, m1, d0, d1, b0, b1, h0, h1, dh))
        if out:
            # one JPG per frame: the unlifted frame on the left, the lifted one on the right, each at 960 x 540
            pair = np.concatenate([a, np.clip(lifted, 0, 1)], axis=1)
            im = Image.fromarray((pair * 255.0 + 0.5).astype(np.uint8)).resize((1920, 540), Image.LANCZOS)
            im.save(os.path.join(out, name.replace(".png", "") + ".pair.jpg"), quality=85)
    print("SUMMARY lit-sim | frames %d | worst dark after %.1f%% | worst hue shift %.1f deg | worst blown after %.2f%%" % (len(paths), worst_dark, worst_hue, worst_blown))


if __name__ == "__main__":
    main(sys.argv[1:])

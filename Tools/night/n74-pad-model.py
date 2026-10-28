"""NIGHT74 builder L (pads74): an offline MODEL of why the steady stream should flicker less, run through the meter's own test (Tools/night/n74-flick.py detect). A model, not a Player
frame: the lead's ON / OFF pair (n74-reels.sh ... pan74, n74-flick.py pads) is the measure, and a model that renders pixels itself overstates (NIGHT74 L-151: the crown / ice models
did, about 3 times); what this settles is the SIGN and the size dependence, not the number.
What it renders: a jump pad's stream seen from D metres on a flat dark floor (display level 40), a camera yawing 0.2 deg a frame (UnityTuning.N74PanRateDeg: 2.5 px a frame at 1080p and a
74 degree vertical field of view) on a 60 fps clock for 180 frames (a pan74 sweep), the stream's particles as the game draws them:
  shipped  Pad27: 12 a second, life 1.5 s, speed 2 m/s, a HARD square of 0.13 m x a size curve (0.2 at birth, 1 at 0.2 of the life, 0 at death), additive full cyan; drawn with no
           anti-aliasing (a pixel is lit when its centre is inside) and with 4x MSAA (the share of four samples inside)
  steady   Pad74: 5 a second, life 2 s, speed 1.5 m/s, a SOFT sprite (1 - r^2)^2 of 0.34 m x a size curve (0.6 at birth and death, 1 at 0.25), its brightness the sheet's FrameLevel
           (32 frames; fade in 0.30, out 0.45) x the tint 0.7, sampled at the pixel centre (the texture filter's job: a continuous profile)
and measures, with the meter's own steps (block means of the frame and of the compensated residual, cur - prev warped by exactly the pan, bilinear: Pan74.Warp's job; detect(): a flip is a
sign change over STEP_MIN with |step| over TAU, a block is flagged on 2 flips in 6 frames): the flagged block-windows in the stream's blocks per 1000 block-windows, shipped against steady.
  python -I Tools/night/n74-pad-model.py [--seeds N] [--dist 12,20,30,45]
"""
import importlib.util
import os
import sys

import numpy as np
from scipy import ndimage

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n74flick", os.path.join(HERE, "n74-flick.py"))
nf = importlib.util.module_from_spec(spec)
spec.loader.exec_module(nf)

W, H, BLOCK, FRAMES, FPS = 640, 360, 8, 180, 60
FOCAL = 540.0 / np.tan(np.radians(74.0) / 2)   # px per radian-ish at 1080p: px per metre at distance D is FOCAL / D
PAN_PX = np.radians(0.2) * FOCAL               # px a frame at the centre
FLOOR, AMP = 40.0, 130.0                       # display level of the floor; what a fully lit hard square adds (the cyan HDR on the dark floor, after the display curve: a guess, the same for both)
RADIUS = 1.0                                   # jumpPad.radius (m)

SHIPPED = dict(rate=12.0, life=1.5, speed=2.0, size=0.13)
STEADY = dict(rate=5.0, life=2.0, speed=1.5, size=0.34)
FADE_IN, FADE_OUT, FRAMES_SHEET, TINT, FLOOR_SIZE = 0.30, 0.45, 32, 0.7, 0.6


def smooth(a, b, x):
    t = min(max((x - a) / (b - a), 0.0), 1.0)
    return t * t * (3 - 2 * t)


def shipped_size(u):      # sizeOverLifetime: 0.2 at 0, 1 at 0.2, 0 at 1 (JumpPadView27.MakeStream's keys, linear segments: Unity's default tangents are smooth, close enough)
    return 0.2 + 0.8 * (u / 0.2) if u < 0.2 else 1.0 - (u - 0.2) / 0.8


def steady_size(u):       # the steady twin's keys: 0.6 at 0, 1 at 0.25, 0.6 at 1, flat tangents (a smoothstep each way)
    return FLOOR_SIZE + (1 - FLOOR_SIZE) * (smooth(0, 0.25, u) if u < 0.25 else 1 - smooth(0.25, 1.0, u))


def frame_level(u):       # Pad74Math.FrameLevel on the frame the life fraction u lands in
    i = min(int(u * FRAMES_SHEET), FRAMES_SHEET - 1)
    t = (i + 0.5) / FRAMES_SHEET
    return smooth(0, FADE_IN, t) * (1 - smooth(1 - FADE_OUT, 1, t))


def particles(cfg, dist, seed):
    """[(birth offset s, x offset m)] per cycle is drawn on the fly: -> a function giving, per frame k, the live particles (cx px, cy px, size m, life fraction)."""
    rng = np.random.default_rng(seed)
    n = int(round(cfg["rate"] * cfg["life"]))
    phase = rng.random(n) * cfg["life"]
    xs = rng.uniform(-1, 1, size=(n, 64)) * 0.7 * RADIUS   # a fresh x for each cycle of each slot (a cone of radius 0.7 R, seen from the side: the x projection)
    scale = FOCAL / dist

    def at(k):
        out = []
        t = k / FPS
        for i in range(n):
            age = phase[i] + t
            cycle = int(age // cfg["life"])
            u = (age % cfg["life"]) / cfg["life"]
            rise = cfg["speed"] * u * cfg["life"] + 0.19
            cx = 320 + xs[i, cycle % 64] * scale - PAN_PX * k
            cy = 300 - rise * scale
            out.append((cx, cy, u))
        return out
    return at, scale


def render(kind, dist, seed, aa):
    cfg = SHIPPED if kind == "shipped" else STEADY
    at, scale = particles(cfg, dist, seed)
    frames = np.empty((FRAMES, H, W), np.float32)
    ys, xs = np.mgrid[0:H, 0:W]
    msaa = [(-0.125, -0.375), (0.375, -0.125), (-0.375, 0.125), (0.125, 0.375)]
    for k in range(FRAMES):
        img = np.full((H, W), FLOOR, np.float32)
        for cx, cy, u in at(k):
            if kind == "shipped":
                s = cfg["size"] * shipped_size(u) * scale / 2     # half size in px
                lvl = 1.0
            else:
                s = cfg["size"] * steady_size(u) * scale / 2
                lvl = frame_level(u) * TINT * 1.8                 # one soft sprite holds about 1.8 x a hard square's light at its peak (UnityTuning.N74PadStreamTint's reason)
            x0, x1 = int(max(0, np.floor(cx - s - 2))), int(min(W, np.ceil(cx + s + 2)))
            y0, y1 = int(max(0, np.floor(cy - s - 2)), ), int(min(H, np.ceil(cy + s + 2)))
            if x1 <= x0 or y1 <= y0 or s <= 1e-6:
                continue
            px = xs[y0:y1, x0:x1] + 0.5
            py = ys[y0:y1, x0:x1] + 0.5
            if kind == "shipped":
                if aa:
                    cov = sum((np.abs(px + dx - cx) <= s) & (np.abs(py + dy - cy) <= s) for dx, dy in msaa) / 4.0
                else:
                    cov = ((np.abs(px - cx) <= s) & (np.abs(py - cy) <= s)).astype(np.float32)
                val = AMP * cov
            else:
                r = np.sqrt(((px - cx) / s) ** 2 + ((py - cy) / s) ** 2)
                prof = np.where(r < 1, (1 - r * r) ** 2, 0.0)
                val = AMP * lvl * prof
            img[y0:y1, x0:x1] += val.astype(np.float32)
        frames[k] = np.clip(img, 0, 255)
    return frames


def blocks(frames):
    n = frames.shape[0]
    means = frames.reshape(n, H // BLOCK, BLOCK, W // BLOCK, BLOCK).mean(axis=(2, 4))
    resid = np.zeros_like(frames)
    for k in range(1, n):
        warped = ndimage.shift(frames[k - 1], (0, -PAN_PX), order=1, mode="nearest")   # out(x) = prev(x + pan): the previous frame warped to this camera
        resid[k] = frames[k] - warped
    rm = resid.reshape(n, H // BLOCK, BLOCK, W // BLOCK, BLOCK).mean(axis=(2, 4))
    return means, rm


def flagged_share(frames):
    means, rm = blocks(frames)
    _flip, flagged, amp = nf.detect(nf.first_differences(means, rm))
    flagged[:, :, :1] = False
    flagged[:, :, -1:] = False
    region = (np.abs(means - means[0:1].mean()) > 0).any(axis=0)   # blocks any particle ever touched
    region = ndimage.binary_dilation(region, iterations=1)
    n = int(flagged[:, region].sum())
    total = flagged.shape[0] * int(region.sum())
    return n, total, float(amp[flagged].mean()) if flagged.any() else 0.0


def main(argv):
    seeds, dists = 4, [12, 20, 30, 45]
    i = 0
    while i < len(argv):
        if argv[i] == "--seeds":
            seeds = int(argv[i + 1]); i += 1
        elif argv[i] == "--dist":
            dists = [int(v) for v in argv[i + 1].split(",")]; i += 1
        i += 1
    print("pad stream model: %dx%d px crop, block %d, %d frames, pan %.2f px a frame, floor %d, hard square adds %d levels, %d seeds" % (W, H, BLOCK, FRAMES, PAN_PX, FLOOR, AMP, seeds))
    tot = {"shipped-noaa": [0, 0], "shipped-msaa": [0, 0], "steady": [0, 0]}
    for d in dists:
        row = {}
        for label, kind, aa in (("shipped-noaa", "shipped", False), ("shipped-msaa", "shipped", True), ("steady", "steady", False)):
            n = t = 0
            amps = []
            for sd in range(seeds):
                a, b, am = flagged_share(render(kind, d, 100 + sd, aa))
                n += a
                t += b
                amps.append(am)
            row[label] = (n, t, float(np.mean(amps)))
            tot[label][0] += n
            tot[label][1] += t
        rate = {k: 1000.0 * v[0] / max(1, v[1]) for k, v in row.items()}
        chg = lambda base: ("%+.0f %%" % (100.0 * (rate["steady"] - rate[base]) / rate[base])) if rate[base] > 0 else "n/a (base 0)"
        print("D %2d m (%.1f px a metre; hard square %.1f px, soft sprite %.1f px across): flagged block-windows per 1000 | shipped, no AA %.1f | shipped, 4x MSAA %.1f | steady %.1f | steady vs shipped no AA %s, vs MSAA %s" % (
            d, FOCAL / d, 0.13 * FOCAL / d, 0.34 * FOCAL / d, rate["shipped-noaa"], rate["shipped-msaa"], rate["steady"], chg("shipped-noaa"), chg("shipped-msaa")))
    print("TOTAL flagged / block-windows | shipped no AA %d / %d | shipped MSAA %d / %d | steady %d / %d" % (
        tot["shipped-noaa"][0], tot["shipped-noaa"][1], tot["shipped-msaa"][0], tot["shipped-msaa"][1], tot["steady"][0], tot["steady"][1]))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

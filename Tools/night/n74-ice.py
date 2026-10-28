"""NIGHT74 builder L (leftover 4): ice74's meter, on the boss63 frames of THE ICEBREAKER (shot-N30-B63-brawl40-{idle,windup,stagger,flinch}.png) of one ON / OFF pair.
THE BARS (proposed 2026-10-27 before any measurement, from NIGHT73-REPORT's "THE ICEBREAKER 19.8 % near-black against a 15 % aim"; never loosened after one):
  - the DARK share (display luma under 30, lit72's weights 0.299 / 0.587 / 0.114 on the 8-bit bytes, the whole frame): at most 15 % on every frame ON, and down by at least 5 points
    on the idle frame ON against OFF (the frame the 19.8 % was read on);
  - NO WASH: the pixels over luma 115 in the OFF frame (the lit floor, the boss, the lights) move by at most +3 levels of mean luma ON against OFF (a local fill lifts the dark, not the lit);
  - the DANGER share (n73-hue.py's band: hue 0-40 or 345-360 deg, saturation >= 0.35, value >= 0.20, the HUD bands out): not up by more than 2 points ON against OFF.
  python -I Tools/night/n74-ice.py <on dir> <off dir>               pairs shot-N30-B63-brawl40-*.png by name (a boss63 side's folder: Logs/feel/<tag>/<side>/boss63)
  python -I Tools/night/n74-ice.py --pair <on.png> <off.png> ...    explicit pairs
  python -I Tools/night/n74-ice.py --one <png> ...                  single frames (the baseline: how dark each is today)
Prints one line per frame and `ICE74: PASS` / `ICE74: FAIL (which bars)`. Exit 0 on PASS (or --one), 1 on FAIL, 2 on no frames.
"""
import os
import sys

import numpy as np
from PIL import Image

DARK_BELOW = 30.0
DARK_MAX_PCT = 15.0
IDLE_DOWN_PTS = 5.0
WASH_ABOVE = 115.0
WASH_MAX = 3.0
DANGER_UP_MAX = 2.0
PREFIX = "shot-N30-B63-brawl40-"


def luma(path):
    im = np.asarray(Image.open(path).convert("RGB"), dtype=np.float32)
    return 0.299 * im[..., 0] + 0.587 * im[..., 1] + 0.114 * im[..., 2]


def danger_share(path):
    """n73-hue.py's measure on the same 480 x 270 reduction (PIL's HSV)."""
    im = Image.open(path).convert("RGB").resize((480, 270))
    hsv = np.asarray(im.convert("HSV"), dtype=np.float32)
    h = hsv[..., 0] * 360.0 / 255.0
    s, v = hsv[..., 1], hsv[..., 2]
    y0, y1 = int(270 * 0.12), int(270 * 0.82)
    band = ((h[y0:y1] <= 40) | (h[y0:y1] >= 345)) & (s[y0:y1] >= 0.35 * 255) & (v[y0:y1] >= 0.20 * 255)
    return 100.0 * float(band.mean())


def stats(path):
    y = luma(path)
    h = y.shape[0]
    core = y[int(h * 0.12):int(h * 0.82)]
    return {"dark": 100.0 * float((y < DARK_BELOW).mean()), "core": 100.0 * float((core < DARK_BELOW).mean()), "mean": float(y.mean()), "danger": danger_share(path), "y": y}


def frames_of(d):
    return {n: os.path.join(d, n) for n in sorted(os.listdir(d)) if n.startswith(PREFIX) and n.endswith(".png")}


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__)
        return 0
    pairs, singles = [], []
    if argv[0] == "--one":
        singles = argv[1:]
    elif argv[0] == "--pair":
        rest = argv[1:]
        pairs = [(os.path.basename(rest[i]), rest[i], rest[i + 1]) for i in range(0, len(rest) - 1, 2)]
    elif len(argv) == 2 and os.path.isdir(argv[0]) and os.path.isdir(argv[1]):
        on, off = frames_of(argv[0]), frames_of(argv[1])
        pairs = [(n[len(PREFIX):-4], on[n], off[n]) for n in on if n in off]
    if not pairs and not singles:
        print("n74-ice: no frames to read (a boss63 folder of THE ICEBREAKER with %s*.png in both)" % PREFIX)
        return 2
    if singles:
        for p in singles:
            s = stats(p)
            print("FRAME %s | dark<30 whole %.1f %%, core %.1f %% | mean luma %.1f | danger %.1f %%" % (os.path.basename(p), s["dark"], s["core"], s["mean"], s["danger"]))
        return 0
    fails = []
    for name, on_p, off_p in pairs:
        a, b = stats(on_p), stats(off_p)
        bright = b["y"] > WASH_ABOVE
        wash = float((a["y"][bright] - b["y"][bright]).mean()) if bright.any() else 0.0
        dd = a["danger"] - b["danger"]
        ok_dark = a["dark"] <= DARK_MAX_PCT
        ok_idle = name != "idle" or (b["dark"] - a["dark"]) >= IDLE_DOWN_PTS
        ok_wash = wash <= WASH_MAX
        ok_danger = dd <= DANGER_UP_MAX
        print("PAIR %-8s | dark<30 ON %.1f %% OFF %.1f %% (bar <= %.0f %%: %s%s) | bright pixels (OFF > %.0f) move %+.1f levels (bar <= +%.0f: %s) | mean luma ON %.1f OFF %.1f | danger ON %.1f %% OFF %.1f %% (%+.1f, bar <= +%.0f: %s)" % (
            name, a["dark"], b["dark"], DARK_MAX_PCT, "ok" if ok_dark else "FAIL", ("" if name != "idle" else ("; idle down %.1f pts, bar >= %.0f: %s" % (b["dark"] - a["dark"], IDLE_DOWN_PTS, "ok" if ok_idle else "FAIL"))),
            WASH_ABOVE, wash, WASH_MAX, "ok" if ok_wash else "FAIL", a["mean"], b["mean"], a["danger"], b["danger"], dd, DANGER_UP_MAX, "ok" if ok_danger else "FAIL"))
        if not ok_dark:
            fails.append("%s dark %.1f %%" % (name, a["dark"]))
        if not ok_idle:
            fails.append("%s idle down %.1f pts" % (name, b["dark"] - a["dark"]))
        if not ok_wash:
            fails.append("%s wash %+.1f" % (name, wash))
        if not ok_danger:
            fails.append("%s danger %+.1f" % (name, dd))
    print("ICE74: %s" % ("PASS" if not fails else "FAIL (" + "; ".join(fails) + ")"))
    return 0 if not fails else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

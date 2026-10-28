"""NIGHT74 builder L (L3): crown74's meter. The slate's doneWhen for THE CROWN's ceiling lift, read off the boss63 frames of one ON / OFF pair:
  - the DARK share (display luma under 30, lit72's weights 0.299 / 0.587 / 0.114 on the 8-bit bytes, the whole frame): at most 15 % on every crown frame ON;
  - the REGION 900-1300 x 150-450 (gg-proxy-close #9: the ceiling centre): the mean luma up by at least 25 ON against OFF;
  - the DANGER share (n73-hue.py's band, written here the same: hue 0-40 or 345-360 deg, saturation >= 0.35, value >= 0.20, the HUD bands out: top 12 % and bottom 18 %):
    not up by more than 2 points ON against OFF;
  - NO WASH (added at the retake): the whole frame's mean luma up by at most 12 levels ON against OFF.
THE BARS were written before any measurement (the slate, FORGE74) and are never loosened after one. This tool only reads them off the frames.
  python -I Tools/night/n74-crown.py <on dir> <off dir>              pairs shot-N30-B63-crown37-*.png by name (a boss63 side's folder: Logs/feel/<tag>/<side>/boss63)
  python -I Tools/night/n74-crown.py --pair <on.png> <off.png> ...   explicit pairs
  python -I Tools/night/n74-crown.py --one <png> ...                 no pair: the numbers of single frames (the baseline: how dark each frame is today)
It prints one line per frame and `CROWN74: PASS` / `CROWN74: FAIL (which bars)`. Exit 0 on PASS (or --one), 1 on FAIL, 2 on no frames.
"""
import os
import sys

import numpy as np
from PIL import Image

REGION = (900, 150, 1300, 450)   # x0, y0, x1, y1 on a 1920 x 1080 frame
DARK_BELOW = 30.0
DARK_MAX_PCT = 15.0
REGION_UP = 25.0
DANGER_UP_MAX = 2.0
WASH_MAX = 12.0  # the retake's bar (written 2026-10-27 23:5x, after n74-t2, before the retake's measurement): the whole frame's mean luma up by at most 12 levels ON against OFF


def load(path):
    return np.asarray(Image.open(path).convert("RGB"), dtype=np.float32)


def luma(im):
    return 0.299 * im[..., 0] + 0.587 * im[..., 1] + 0.114 * im[..., 2]


def danger_share(path):
    """n73-hue.py's measure, on the same 480 x 270 reduction (PIL's HSV), so the two tools read one number."""
    im = Image.open(path).convert("RGB").resize((480, 270))
    hsv = np.asarray(im.convert("HSV"), dtype=np.float32)
    h = hsv[..., 0] * 360.0 / 255.0
    s, v = hsv[..., 1], hsv[..., 2]
    y0, y1 = int(270 * 0.12), int(270 * 0.82)
    band = ((h[y0:y1] <= 40) | (h[y0:y1] >= 345)) & (s[y0:y1] >= 0.35 * 255) & (v[y0:y1] >= 0.20 * 255)
    return 100.0 * float(band.mean())


def stats(path):
    im = load(path)
    y = luma(im)
    h, w = y.shape
    sx = w / 1920.0
    x0, y0, x1, y1 = (int(REGION[0] * sx), int(REGION[1] * sx), int(REGION[2] * sx), int(REGION[3] * sx))
    reg = y[y0:y1, x0:x1]
    return {
        "dark": 100.0 * float((y < DARK_BELOW).mean()),
        "region": float(reg.mean()),
        "regionDark": 100.0 * float((reg < DARK_BELOW).mean()),
        "mean": float(y.mean()),
        "danger": danger_share(path),
    }


def frames_of(d):
    out = {}
    for n in sorted(os.listdir(d)):
        if n.startswith("shot-N30-B63-crown37-") and n.endswith(".png"):
            out[n] = os.path.join(d, n)
    return out


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__)
        return 0
    pairs = []
    singles = []
    if argv[0] == "--one":
        singles = argv[1:]
    elif argv[0] == "--pair":
        rest = argv[1:]
        pairs = [(os.path.basename(rest[i]), rest[i], rest[i + 1]) for i in range(0, len(rest) - 1, 2)]
    elif len(argv) == 2 and os.path.isdir(argv[0]) and os.path.isdir(argv[1]):
        on, off = frames_of(argv[0]), frames_of(argv[1])
        pairs = [(n.replace("shot-N30-B63-crown37-", "").replace(".png", ""), on[n], off[n]) for n in on if n in off]
    if not pairs and not singles:
        print("n74-crown: no frames to read (a boss63 folder of the crown with shot-N30-B63-crown37-*.png in both)")
        return 2
    if singles:
        for p in singles:
            s = stats(p)
            print("FRAME %s | dark<30 %.1f %% | region 900-1300 x 150-450 mean luma %.1f, dark %.1f %% | frame mean luma %.1f | danger %.1f %%" % (
                os.path.basename(p), s["dark"], s["region"], s["regionDark"], s["mean"], s["danger"]))
        return 0
    fails = []
    for name, on_p, off_p in pairs:
        a, b = stats(on_p), stats(off_p)
        up = a["region"] - b["region"]
        dd = a["danger"] - b["danger"]
        ok_dark = a["dark"] <= DARK_MAX_PCT
        ok_up = up >= REGION_UP
        ok_danger = dd <= DANGER_UP_MAX
        wash = a["mean"] - b["mean"]
        ok_wash = wash <= WASH_MAX
        print("PAIR %-10s | dark<30 ON %.1f %% OFF %.1f %% (bar <= %.0f %%: %s) | region mean luma ON %.1f OFF %.1f up %+.1f (bar >= %.0f: %s) | region dark ON %.1f %% OFF %.1f %% | danger ON %.1f %% OFF %.1f %% (%+.1f, bar <= +%.0f: %s) | frame mean luma ON %.1f OFF %.1f (%+.1f, wash bar <= +%.0f: %s)" % (
            name, a["dark"], b["dark"], DARK_MAX_PCT, "ok" if ok_dark else "FAIL", a["region"], b["region"], up, REGION_UP, "ok" if ok_up else "FAIL",
            a["regionDark"], b["regionDark"], a["danger"], b["danger"], dd, DANGER_UP_MAX, "ok" if ok_danger else "FAIL", a["mean"], b["mean"], wash, WASH_MAX, "ok" if ok_wash else "FAIL"))
        if not ok_wash:
            fails.append("%s wash %+.1f" % (name, wash))
        if not ok_dark:
            fails.append("%s dark %.1f %%" % (name, a["dark"]))
        if not ok_up:
            fails.append("%s region up %+.1f" % (name, up))
        if not ok_danger:
            fails.append("%s danger %+.1f" % (name, dd))
    print("CROWN74: %s" % ("PASS" if not fails else "FAIL (" + "; ".join(fails) + ")"))
    return 0 if not fails else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

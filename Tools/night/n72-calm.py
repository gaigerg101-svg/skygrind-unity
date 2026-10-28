"""NIGHT72 builder L (L4, calm72): the red-border meter. gg-proxy #10: "a red speckled noise frame 150-250 px deep eats all four edges at 9999 health".
A scene's own colours (a red arena, an orange floor) make any absolute "how red is the edge" test lie, so the meter measures a DIFFERENCE: the same pose
(same level, same seed, same fixed clock, enemies frozen: tools run through still72's PNGs or boss63's shots) with and without the border, and reports how
deep the difference runs in from each edge, in px at 1080p.
  python -I Tools/night/n72-calm.py --ref <frame without the border> <frame>...      depth of |frame - ref| from each edge
  python -I Tools/night/n72-calm.py --selftest                                       a planted band of known depth on a textured frame
Method: d = max over R, G, B of |frame - ref| (levels); for each side the mean of d along the side's middle 40 % (a left / right edge over y 30-70 %,
a top / bottom edge over x 30-70 %) is a profile by depth; the depth is the farthest px at which that mean is at least SENSE (8 levels). The HUD sits
at the same place in both frames (the same pose), so it drops out; a boss or an enemy that moved between the two frames would show as a depth too, which
is why a pair is two sides of one deterministic run. Bars (NIGHT72 slate L4): with calm72 ON, health 100 % and 45 %: depth 0 against the reference
frame; under 30 % health: at most 60 px. With calm72 OFF the same frames read the old border (the OFF side's number is the baseline).
"""
import json
import sys

import numpy as np
from PIL import Image

SENSE = 8.0
PROFILE_MAX = 400


def depths(frame, ref, sense=SENSE):
    d = np.abs(frame.astype(np.float32) - ref.astype(np.float32)).max(axis=2)
    h, w = d.shape
    y0, y1, x0, x1 = int(h * 0.30), int(h * 0.70), int(w * 0.30), int(w * 0.70)
    sides = {
        "left": d[y0:y1, :PROFILE_MAX].mean(axis=0),
        "right": d[y0:y1, w - PROFILE_MAX:].mean(axis=0)[::-1],
        "top": d[:PROFILE_MAX, x0:x1].mean(axis=1),
        "bottom": d[h - PROFILE_MAX:, x0:x1].mean(axis=1)[::-1],
    }
    out = {}
    for name, prof in sides.items():
        over = np.nonzero(prof >= sense)[0]
        out[name] = int(over.max()) + 1 if len(over) else 0
    out["max"] = max(out["left"], out["right"], out["top"], out["bottom"])
    out["meanDiff"] = round(float(d.mean()), 2)
    return out


def selftest():
    rng = np.random.default_rng(3)
    h, w = 1080, 1920
    gy, gx = np.mgrid[0:h, 0:w]
    ref = np.stack([(90 + 40 * np.sin(gx / 60.0)), (60 + 30 * np.cos(gy / 50.0)), np.full((h, w), 50.0)], axis=-1).astype(np.float32)
    ok = True
    for depth, want in ((0, 0), (60, 60), (200, 200)):
        f = ref.copy()
        if depth:
            # a red frame: alpha ramps from 0.8 at the edge to 0 at `depth` px in, on all four sides
            ax = np.minimum(gx, w - 1 - gx).astype(np.float32)
            ay = np.minimum(gy, h - 1 - gy).astype(np.float32)
            e = np.minimum(ax, ay)
            a = np.clip(1.0 - e / depth, 0, 1) * 0.8
            red = np.array([220.0, 40.0, 30.0], np.float32)
            f = f * (1 - a[..., None]) + red * a[..., None]
            f = f + rng.normal(0, 2, f.shape)
        r = depths(np.clip(f, 0, 255).astype(np.uint8), np.clip(ref, 0, 255).astype(np.uint8))
        good = (r["max"] == 0) if want == 0 else (0.5 * want <= r["max"] <= want + 4)
        print("%s planted band %d px: measured %d (L %d R %d T %d B %d)" % ("ok  " if good else "FAIL", depth, r["max"], r["left"], r["right"], r["top"], r["bottom"]))
        ok = ok and good
    print("SELFTEST n72-calm: %s" % ("PASS" if ok else "FAIL"))
    return 0 if ok else 1


def main(argv):
    if "--selftest" in argv:
        return selftest()
    ref_path = None
    jpath = None
    paths = []
    i = 0
    while i < len(argv):
        if argv[i] == "--ref":
            ref_path = argv[i + 1]; i += 1
        elif argv[i] == "--json":
            jpath = argv[i + 1]; i += 1
        else:
            paths.append(argv[i])
        i += 1
    if not ref_path or not paths:
        print("usage: n72-calm.py --ref <frame without the border> <frame>...   |   --selftest")
        return 2
    ref = np.asarray(Image.open(ref_path).convert("RGB"))
    res = {}
    for p in paths:
        f = np.asarray(Image.open(p).convert("RGB"))
        if f.shape != ref.shape:
            print("%s | ERROR size %s differs from the reference %s" % (p, f.shape, ref.shape))
            continue
        d = depths(f, ref)
        name = p.replace("\\", "/").split("/")[-1]
        res[name] = d
        print("%s | depth px left %d right %d top %d bottom %d | max %d | mean |diff| %.2f" % (name, d["left"], d["right"], d["top"], d["bottom"], d["max"], d["meanDiff"]))
    if jpath:
        with open(jpath, "w", encoding="utf-8") as fh:
            json.dump({"ref": ref_path, "sense": SENSE, "frames": res}, fh, indent=1)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

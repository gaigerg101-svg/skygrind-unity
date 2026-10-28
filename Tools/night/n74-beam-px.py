"""NIGHT74 builder L (pads74's beam part): how bright and how clipped the cyan light columns (the pad beams) are in a frame, for an ON frame against an OFF frame of the same room and pose.
A pixel counts as CYAN LIGHT when it is saturated cyan: red under 0.35 x green, green and blue both over 120 (the lit wall's teal is (12, 75, 68): out; the beam is (8, 245, 245) low, (1, 222, 228) in the
middle, (2, 185, 192) at the top in the n74-t1 foundry-b1 mid shot). The neon strips are cyan too and count in both frames the same, so the DIFFERENCE between the frames is the beam's.
Prints per frame: cyan pixels (share of the frame), the CLIPPED ones (green or blue 250 or more: what reads as blinding), the mean and the 95th percentile luma of the cyan pixels (0.299 R + 0.587 G + 0.114 B),
and, for two frames, the change. Optional boxes name=x0,y0,x1,y1 (px of the 1920 x 1080 frame) add the mean RGB and the clipped share of that box (the beam's own place, read off the OFF frame).
  python -I -B Tools/night/n74-beam-px.py <on.png> <off.png> [name=x0,y0,x1,y1 ...]
  python -I -B Tools/night/n74-beam-px.py <frame.png> [name=x0,y0,x1,y1 ...]
The bar (L's, written before any pads74 beam Player frame): the clipped cyan pixels at least 40 % fewer ON than OFF, the cyan pixels' mean luma lower ON, and the column still there: cyan pixels ON at least half of OFF.
"""
import sys

import numpy as np
from PIL import Image


def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float32)


def luma(im):
    return 0.299 * im[..., 0] + 0.587 * im[..., 1] + 0.114 * im[..., 2]


def cyan_mask(im):
    r, g, b = im[..., 0], im[..., 1], im[..., 2]
    return (r < 0.35 * g) & (g > 120) & (b > 120)


def stats(im):
    m = cyan_mask(im)
    y = luma(im)[m]
    return {"cyan": int(m.sum()), "share": 100.0 * float(m.mean()), "clipped": int((m & ((im[..., 1] >= 250) | (im[..., 2] >= 250))).sum()),
            "mean": float(y.mean()) if y.size else 0.0, "p95": float(np.percentile(y, 95)) if y.size else 0.0}


def line(name, s):
    return "%s | cyan pixels %d (%.2f %% of the frame) | clipped (G or B >= 250) %d | luma of the cyan pixels: mean %.1f, p95 %.1f" % (name, s["cyan"], s["share"], s["clipped"], s["mean"], s["p95"])


def box_line(name, im, spec):
    label, box = spec.split("=")
    x0, y0, x1, y1 = [int(v) for v in box.split(",")]
    r = im[y0:y1, x0:x1]
    m = r.reshape(-1, 3).mean(axis=0)
    clip = 100.0 * float(((r[..., 1] >= 250) | (r[..., 2] >= 250)).mean())
    return "  %s %s x %d-%d y %d-%d | mean RGB (%.0f, %.0f, %.0f) | clipped %.0f %%" % (name, label, x0, x1, y0, y1, m[0], m[1], m[2], clip)


def main(argv):
    pngs = [a for a in argv if a.lower().endswith(".png")]
    boxes = [a for a in argv if "=" in a]
    if not pngs or len(pngs) > 2:
        print(__doc__)
        return 2
    names = ["ON", "OFF"] if len(pngs) == 2 else ["frame"]
    ims = [load(p) for p in pngs]
    st = [stats(im) for im in ims]
    for n, s in zip(names, st):
        print(line(n, s))
    for n, im in zip(names, ims):
        for spec in boxes:
            print(box_line(n, im, spec))
    if len(pngs) == 2:
        on, off = st
        clip = 100.0 * (on["clipped"] - off["clipped"]) / off["clipped"] if off["clipped"] else None
        ok_clip = off["clipped"] == 0 or on["clipped"] <= 0.6 * off["clipped"]
        ok_mean = on["mean"] < off["mean"]
        ok_there = on["cyan"] >= 0.5 * off["cyan"]
        print("CHANGE clipped %s | mean luma of the cyan pixels %+.1f | cyan pixels %+.1f %% | BAR clipped down >= 40 %%: %s | mean lower: %s | the column still there (cyan pixels ON >= half of OFF): %s" % (
            ("%+.1f %%" % clip) if clip is not None else "n/a (OFF clips none)", on["mean"] - off["mean"], 100.0 * (on["cyan"] - off["cyan"]) / max(1, off["cyan"]),
            "PASS" if ok_clip else "FAIL", "PASS" if ok_mean else "FAIL", "PASS" if ok_there else "FAIL"))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

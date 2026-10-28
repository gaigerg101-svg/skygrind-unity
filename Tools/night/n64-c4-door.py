# NIGHT64 builder D (slate C4, THE EXIT THAT READS DANGER; view row design.n64.goal64): THE DOOR METER on a157's clear shots
# (-skygrindClearShots60: shot-N29-L57-<level>-<room>-clear.png + its sidecar .json, whose "box" is the exit door's screen rect, image px
# [x, y, w, h] from the top-left, FeelReel.N60.Lamp.cs). gg-proxy (Logs/n63/gg-proxy.md #4): "a red-orange vertical line drops from a ceiling
# lamp down the cyan door" on 5 of 5 spire frames: THE CROWN's goal column (Route47View, red-gold #FF8A3A, the danger band) seen through the
# walls at x -10, the centre of every spire door.
# Classes (HSV on the frame's stored sRGB values, colours63's own bands, design.n63.colours63.numbers.bands):
#   DANGER   vivid (S >= 0.55, V >= 0.50: n63-c1-meter.py's floor) and hue in 345..35 (red-orange through red)
#   BOUNDARY the boundary's pale violet-white: hue 230..300, S < 0.40, V >= 0.75
#   PALE     any other pale bright pixel (S < 0.40, V >= 0.75): the column's light added over the cyan door reads pale blue-white, not violet
# Modes:
#   python -I Tools/night/n64-c4-door.py red <a157 dir> [...]        per door box: DANGER px inside it (the slate's red, row off)
#   python -I Tools/night/n64-c4-door.py pair <on dir> <off dir>     same-run pair (<side>-on vs <side>-off, -skygrindSameRun63 n64.goal64):
#                                                                    inside each box, the CHANGED px (a channel differs by > 24, the
#                                                                    meters' rule) are the column's (only the row differs); per box their
#                                                                    DANGER on / off, BOUNDARY and PALE on, and the whole box's DANGER on /
#                                                                    off (the window shaft over the door keeps its red-gold: GG's call)
#   python -I Tools/night/n64-c4-door.py planted                     synthetic doors: the line red, the line pale, the shaft kept, a no-op
#   --box x,y,w,h   a box for frames without a sidecar (e.g. a verbs37 frame); --level n52-spire (default) picks the shots
# The C4 verdict per box (pair): PASS when the column's DANGER on is 0, its DANGER off > 0 and its BOUNDARY + PALE on > 0 (the column still
# drawn, pale); NO LINE when the off side has no danger among the changed px (the column is not in that door's box on that shot).
# THE TV FRINGE (builder D, before any GATE frame; read on n64-red v2 and NIGHT63's c1vp / c1xp spire pair): Crt29's flat split
# (UnityTuning.Look35FlatChromaPx: one virtual px, about 2-3 screen px, red one way and blue the other, across the whole screen) gives EVERY
# bright line a red-only edge on its left: on c1vp f0400 the pale column's left edge reads (128, 72, 39), hue 16, S 0.70, V 0.50, a DANGER px,
# beside its pale core. That edge is the screen's split of a pale line, not the column's colour; the meter prints it apart so the reader sees
# both: FRINGE = a changed DANGER px with a changed CORE px (the line's own light: every channel >= 64 and S < 0.50) within FRINGE_PX on its
# row. Two verdicts per box and in the summary: STRICT (every changed DANGER px, fringe included, is 0) and CORE (the DANGER px off the fringe
# is 0). Neither loosens the slate: the critic reads both, with the fringe count.
import glob
import json
import os
import sys

import numpy as np
from PIL import Image

SMIN, VMIN = 0.55, 0.50
DANGER = (345.0, 35.0)
BOUNDARY = (230.0, 300.0)
PALE_S, PALE_V = 0.40, 0.75
DIFF = 24
CORE_MIN, CORE_S = 64, 0.50  # the line's own light: every channel >= 64 (of 255) and S < 0.50
FRINGE_PX = 4  # Crt29's flat split is one virtual px (about 2-3 screen px at 1080p); 4 covers it with a px to spare


def hsv(rgb):
    a = rgb.astype(np.float32) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = a.max(-1)
    mn = a.min(-1)
    d = mx - mn
    s = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0)
    h = np.zeros_like(mx)
    nz = d > 1e-6
    rm = nz & (mx == r)
    gm = nz & (mx == g) & ~rm
    bm = nz & ~rm & ~gm
    h[rm] = (60 * ((g - b)[rm] / d[rm]) + 360) % 360
    h[gm] = 60 * ((b - r)[gm] / d[gm]) + 120
    h[bm] = 60 * ((r - g)[bm] / d[bm]) + 240
    return h, s, mx


def in_arc(h, lo, hi):
    return (h >= lo) & (h <= hi) if lo <= hi else (h >= lo) | (h <= hi)


def classes(rgb):
    h, s, v = hsv(rgb)
    danger = (s >= SMIN) & (v >= VMIN) & in_arc(h, *DANGER)
    pale = (s < PALE_S) & (v >= PALE_V)
    boundary = pale & in_arc(h, *BOUNDARY)
    return danger, boundary, pale & ~boundary


def box_mask(shape, box):
    H, W = shape[0], shape[1]
    m = np.zeros((H, W), bool)
    x, y, w, h = box
    x0, y0 = max(0, int(round(x))), max(0, int(round(y)))
    x1, y1 = min(W, int(round(x + w))), min(H, int(round(y + h)))
    if x1 > x0 and y1 > y0:
        m[y0:y1, x0:x1] = True
    return m


def load(f):
    return np.asarray(Image.open(f).convert("RGB"))


def sidecar(png, dirs):
    name = os.path.splitext(os.path.basename(png))[0] + ".json"
    for d in dirs:
        p = os.path.join(d, name)
        if os.path.isfile(p):
            with open(p, "r", encoding="utf-8") as fh:
                j = json.load(fh)
            b = j.get("box")
            if isinstance(b, list) and len(b) == 4:
                return [float(x) for x in b], j
    return None, None


def twin_dirs(d):
    """The sidecars live beside the reel's own frame (<side>/a157); a same-run half is <side>-on/a157 or <side>-off/a157."""
    d = os.path.normpath(d)
    out = [d]
    parts = d.split(os.sep)
    for i, p in enumerate(parts):
        for suf in ("-on", "-off"):
            if p.endswith(suf) and len(p) > len(suf):
                q = list(parts)
                q[i] = p[: -len(suf)]
                out.append(os.sep.join(q))
    return out


def shots(d, level):
    files = sorted(glob.glob(os.path.join(d, "shot-N29-L57-%s-*-clear.png" % level)))
    if not files:
        files = sorted(glob.glob(os.path.join(d, "*.png")) + glob.glob(os.path.join(d, "*.jpg")))
    return files


def red_one(rgb, box):
    m = box_mask(rgb.shape, box)
    danger, boundary, pale = classes(rgb)
    return {"box": int(m.sum()), "danger": int((danger & m).sum()), "boundary": int((boundary & m).sum()), "pale": int((pale & m).sum())}


def core_mask(rgb):
    _, s, _ = hsv(rgb)
    return (rgb.min(axis=-1) >= CORE_MIN) & (s < CORE_S)


def near_row(mask, px):
    """True where `mask` holds within px on the same row (either side)."""
    out = mask.copy()
    for k in range(1, px + 1):
        out[:, k:] |= mask[:, :-k]
        out[:, :-k] |= mask[:, k:]
    return out


def pair_one(a, b, box):
    m = box_mask(a.shape, box)
    ch = (np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF) & m
    da, ba, pa = classes(a)
    db, _, _ = classes(b)
    core = core_mask(a) & ch
    fringe = da & ch & near_row(core, FRINGE_PX)
    h, s, v = hsv(a)
    r = {"box": int(m.sum()), "changed": int(ch.sum()), "colDangerOn": int((da & ch).sum()), "colFringeOn": int(fringe.sum()),
         "colDangerCoreOn": int((da & ch & ~fringe).sum()), "colDangerOff": int((db & ch).sum()), "colCoreOn": int(core.sum()),
         "colBoundaryOn": int((ba & ch).sum()), "colPaleOn": int((pa & ch).sum()), "boxDangerOn": int((da & m).sum()), "boxDangerOff": int((db & m).sum()),
         "changedOutside": int(((np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF) & ~m).sum()),
         "coreHSV": "-" if not core.any() else "%.0f/%.2f/%.2f" % (float(np.median(h[core])), float(np.median(s[core])), float(np.median(v[core])))}
    drawn = r["colBoundaryOn"] + r["colPaleOn"] > 0
    if r["colDangerOff"] == 0:
        r["strict"] = r["core"] = "NO LINE"
    else:
        r["strict"] = "PASS" if r["colDangerOn"] == 0 and drawn else "FAIL"
        r["core"] = "PASS" if r["colDangerCoreOn"] == 0 and drawn else "FAIL"
    r["verdict"] = r["strict"]
    return r


def fmt(d):
    return " ".join("%s %s" % (k, v) for k, v in d.items())


def arg_box(args):
    if "--box" in args:
        i = args.index("--box")
        v = [float(x) for x in args[i + 1].split(",")]
        del args[i:i + 2]
        return v
    return None


def arg_level(args):
    if "--level" in args:
        i = args.index("--level")
        v = args[i + 1]
        del args[i:i + 2]
        return v
    return "n52-spire"


def run_red(dirs, box, level):
    tot = 0
    n = 0
    for d in dirs:
        for f in shots(d, level):
            b, j = (box, None) if box else sidecar(f, twin_dirs(d))
            if b is None:
                print("%s | no door box (no sidecar .json; pass --box)" % os.path.basename(f))
                continue
            r = red_one(load(f), b)
            tot += r["danger"]
            n += 1
            print("%s | door %s | %s" % (os.path.basename(f), j.get("door") if j else "-", fmt(r)))
    print("RED %s: %d door boxes, DANGER px inside them %d in all" % (",".join(dirs), n, tot))
    return tot


def run_pair(on, off, box, level):
    names = {os.path.basename(f): f for f in shots(off, level)}
    res = []
    for f in shots(on, level):
        g = names.get(os.path.basename(f))
        if g is None:
            print("%s | no off frame" % os.path.basename(f))
            continue
        b, j = (box, None) if box else sidecar(f, twin_dirs(on) + twin_dirs(off))
        if b is None:
            print("%s | no door box (no sidecar .json; pass --box)" % os.path.basename(f))
            continue
        r = pair_one(load(f), load(g), b)
        res.append(r)
        print("%s | door %s | %s" % (os.path.basename(f), j.get("door") if j else "-", fmt(r)))
    lines = [r for r in res if r["strict"] != "NO LINE"]
    sf = [r for r in lines if r["strict"] == "FAIL"]
    cf = [r for r in lines if r["core"] == "FAIL"]

    def word(fails):
        return "PASS" if lines and not fails else "FAIL" if fails else "NO LINE (no shot shows it)"
    print("PAIR %s vs %s: %d shots, %d with the column's line in the door box; the column's DANGER px on %d (TV fringe %d, off the fringe %d) / off %d; "
          "its BOUNDARY on %d, PALE on %d, CORE on %d -> C4 STRICT %s (%d / %d boxes), C4 CORE %s (%d / %d boxes)" % (
              on, off, len(res), len(lines), sum(r["colDangerOn"] for r in res), sum(r["colFringeOn"] for r in res), sum(r["colDangerCoreOn"] for r in res),
              sum(r["colDangerOff"] for r in res), sum(r["colBoundaryOn"] for r in res), sum(r["colPaleOn"] for r in res), sum(r["colCoreOn"] for r in res),
              word(sf), len(lines) - len(sf), len(lines), word(cf), len(lines) - len(cf), len(lines)))
    return res


# --- planted ---

def frame(h=200, w=300):
    a = np.zeros((h, w, 3), np.uint8)
    a[...] = (40, 60, 45)  # a dark green room
    a[60:160, 120:180] = (0, 200, 220)  # the cyan door
    return a


def paint(a, x0, x1, y0, y1, rgb, add=True):
    out = a.astype(np.int16)
    if add:
        out[y0:y1, x0:x1] += np.array(rgb, np.int16)
    else:
        out[y0:y1, x0:x1] = rgb
    return np.clip(out, 0, 255).astype(np.uint8)


def planted():
    box = [116, 56, 68, 108]
    ok = True

    def check(name, cond, got):
        nonlocal ok
        ok = ok and cond
        print("%s %s | %s" % ("PASS" if cond else "FAIL", name, got))

    base = frame()
    shaft = paint(base, 140, 160, 58, 70, (255, 138, 58), add=False)  # the red-gold window shaft over the door, inside the box (kept)
    red_line = paint(paint(shaft, 148, 152, 0, 200, (255, 120, 40), add=False), 148, 152, 60, 160, (255, 138, 58), add=False)  # #FF8A3A down the door
    pale_line = paint(paint(shaft, 148, 152, 0, 200, (190, 182, 240), add=False), 148, 152, 60, 160, (200, 245, 255), add=False)  # pale violet; over the door pale cyan-white
    r = red_one(red_line, box)
    check("red: the red-gold line down the door is DANGER inside the box", r["danger"] >= 4 * 100, fmt(r))
    r = red_one(base, box)
    check("red: a door with no line has no DANGER", r["danger"] == 0, fmt(r))
    p = pair_one(pale_line, red_line, box)
    check("pair: the line turned pale -> the column's DANGER on 0, off > 0, PALE / BOUNDARY on > 0, PASS; the shaft's DANGER kept in the box",
          p["verdict"] == "PASS" and p["boxDangerOn"] > 0 and p["colDangerOff"] > 0, fmt(p))
    p = pair_one(red_line, red_line, box)
    check("pair: no change (a no-op toggle) -> NO LINE, changed 0", p["verdict"] == "NO LINE" and p["changed"] == 0, fmt(p))
    still_red = paint(red_line, 148, 152, 60, 160, (255, 60, 30), add=False)  # a 'fix' that keeps the line red-orange
    p = pair_one(still_red, red_line, box)
    check("pair: a change that keeps the line red-orange -> FAIL", p["verdict"] == "FAIL", fmt(p))
    gone = paint(shaft, 148, 152, 0, 200, (40, 60, 45), add=False)
    gone[56:164, 116:184] = shaft[56:164, 116:184]
    gone[60:160, 148:152] = (0, 200, 220)
    p = pair_one(gone, red_line, box)
    check("pair: the line removed (not drawn at all) -> FAIL (the column must still be drawn, pale)", p["verdict"] == "FAIL", fmt(p))
    # THE TV FRINGE: Crt29's flat split puts the pale line's red alone on its left edge (2 px) and its blue alone on its right edge
    tv = paint(pale_line, 146, 148, 60, 160, (128, 72, 39), add=False)
    tv = paint(tv, 152, 154, 60, 160, (20, 60, 130), add=False)
    p = pair_one(tv, red_line, box)
    check("pair: the pale line with the TV split's red left edge -> STRICT FAIL (the edge counted), CORE PASS (every danger px is fringe)",
          p["strict"] == "FAIL" and p["core"] == "PASS" and p["colFringeOn"] == p["colDangerOn"] > 0, fmt(p))
    tvred = paint(still_red, 146, 148, 60, 160, (200, 20, 10), add=False)
    p = pair_one(tvred, red_line, box)
    check("pair: a line still red-orange with its own red edge -> CORE FAIL too (no pale core: nothing is fringe)",
          p["core"] == "FAIL" and p["colDangerCoreOn"] > 0, fmt(p))
    far = paint(pale_line, 130, 132, 60, 160, (200, 20, 10), add=False)  # a red mark 16 px from the pale line: not its fringe
    p = pair_one(far, red_line, box)
    check("pair: a red mark beyond FRINGE_PX of the pale line -> CORE FAIL (only the line's own edge is fringe)",
          p["core"] == "FAIL" and p["colDangerCoreOn"] > 0, fmt(p))
    bpx = np.zeros((4, 4, 3), np.uint8)
    bpx[...] = (201, 188, 255)  # #C9BCFF, the boundary colour
    d, bd, pl = classes(bpx)
    check("classes: #C9BCFF is BOUNDARY", bool(bd.all()) and not d.any() and not pl.any(), "boundary %d" % int(bd.sum()))
    rg = np.zeros((4, 4, 3), np.uint8)
    rg[...] = (255, 138, 58)  # #FF8A3A, the red-gold
    d, bd, pl = classes(rg)
    check("classes: #FF8A3A (hue 23) is DANGER", bool(d.all()), "danger %d" % int(d.sum()))
    print("C4 DOOR METER PLANTED %s" % ("GREEN" if ok else "RED"))
    return ok


def main(argv):
    args = list(argv)
    box = arg_box(args)
    level = arg_level(args)
    if not args:
        print(__doc__ or "usage: red <dirs> | pair <on> <off> | planted")
        return 2
    mode = args[0]
    if mode == "planted":
        return 0 if planted() else 1
    if mode == "red" and len(args) >= 2:
        run_red(args[1:], box, level)
        return 0
    if mode == "pair" and len(args) == 3:
        run_pair(args[1], args[2], box, level)
        return 0
    print("usage: red <a157 dir> [...] | pair <on dir> <off dir> | planted  [--box x,y,w,h] [--level n52-spire]")
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

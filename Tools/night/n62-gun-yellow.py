# NIGHT62 (the forge FORGE62b, 2026-10-22): P5's meter (NIGHT61 C3, design.n61.gunYellow61), written because builder V's notes say
# "Which tool: not checked" (Research/2026-10-22-wave61/V-C3.md step 4) and n56-colour.py finds no frames in an a157 folder (no
# telemetry.jsonl there). Counts yellow pixels in yellow56's band (hue 45-70 deg, S > 0.5, V >= 0.15: the same band as n56-colour.py's
# V3) inside a rectangle given as fractions of the frame, for each image named, and prints one line per image plus a share of the rect.
# For the verbs37 poses use n56-colour.py's yellowGunPct (it reads the gun's own rect from telemetry.jsonl); this tool is for a PNG shot.
#   python -I Tools/night/n62-gun-yellow.py <image> [<image> ...] [--rect x0,y0,x1,y1]   (default rect 0.55,0.55,1.0,1.0: the gun's corner)
#   python -I Tools/night/n62-gun-yellow.py planted    (synthetic frames: a yellow slab inside the rect counts, one outside does not,
#                                                       a hue-30 orange slab does not, a dark yellow under V 0.15 does not; and the pairs)
#   python -I Tools/night/n62-gun-yellow.py --pair <on image> <off image> [--rect ...]   (the P5 reading, red team 2h finding 7)
#     Same build and pose, only -skygrindNoGunYellow61 between them, so the pixels that differ are the recoloured gun. It prints the yellow
#     px among the CHANGED pixels on / off (on must be 0, off above 0: "recoloured yes") and the yellow LEFT among the unchanged pixels of the
#     rect on the on frame with its bounding box: the world's own yellow, or a part of the gun the row did not recolour. Open the frame at that
#     box before calling P5's "= 0 on the viewmodel" met.
import sys
import numpy as np
from PIL import Image

LO, HI, SMIN, VMIN = 45.0, 70.0, 0.5, 0.15


def yellow_mask(rgb):
    a = rgb.astype(np.float64) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = a.max(axis=-1)
    mn = a.min(axis=-1)
    d = mx - mn
    s = np.where(mx > 0, d / np.where(mx > 0, mx, 1), 0)
    h = np.zeros_like(mx)
    nz = d > 1e-9
    rm = nz & (mx == r)
    gm = nz & (mx == g) & ~rm
    bm = nz & ~rm & ~gm
    h[rm] = (60 * ((g[rm] - b[rm]) / d[rm])) % 360
    h[gm] = 60 * ((b[gm] - r[gm]) / d[gm]) + 120
    h[bm] = 60 * ((r[bm] - g[bm]) / d[bm]) + 240
    return (h >= LO) & (h <= HI) & (s > SMIN) & (mx >= VMIN)


def count(rgb, rect):
    H, W = rgb.shape[:2]
    x0, y0, x1, y1 = int(rect[0] * W), int(rect[1] * H), int(rect[2] * W), int(rect[3] * H)
    m = yellow_mask(rgb[y0:y1, x0:x1])
    return int(m.sum()), (x1 - x0) * (y1 - y0)


def pair(on, off, rect, diff=24):
    H, W = on.shape[:2]
    x0, y0, x1, y1 = int(rect[0] * W), int(rect[1] * H), int(rect[2] * W), int(rect[3] * H)
    a, b = on[y0:y1, x0:x1], off[y0:y1, x0:x1]
    changed = np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > diff
    ya, yb = yellow_mask(a), yellow_mask(b)
    left = ya & ~changed
    box = None
    if left.any():
        ys, xs = np.nonzero(left)
        box = [int(xs.min()) + x0, int(ys.min()) + y0, int(xs.max()) + x0, int(ys.max()) + y0]
    return {"changedOn": int((ya & changed).sum()), "changedOff": int((yb & changed).sum()), "leftOn": int(left.sum()),
            "leftOff": int((yb & ~changed).sum()), "leftBox": box, "changedPx": int(changed.sum())}


def pair_line(r):
    rec = r["changedOn"] == 0 and r["changedOff"] > 0
    return ("changed px %d; yellow among them on %d / off %d -> recoloured %s; yellow left in the rect on %d (off %d)%s" % (
        r["changedPx"], r["changedOn"], r["changedOff"], "yes" if rec else "NO", r["leftOn"], r["leftOff"],
        (" at x0,y0,x1,y1 " + ",".join(str(v) for v in r["leftBox"]) + ": open the frame there") if r["leftBox"] else ""))


def planted():
    rect = (0.55, 0.55, 1.0, 1.0)
    cases = []
    base = np.full((1080, 1920, 3), 40, np.uint8)
    a = base.copy(); a[800:900, 1300:1500] = (230, 200, 20); cases.append(("yellow slab inside the rect", a, True))
    b = base.copy(); b[100:200, 100:300] = (230, 200, 20); cases.append(("yellow slab outside the rect", b, False))
    c = base.copy(); c[800:900, 1300:1500] = (220, 120, 30); cases.append(("orange (hue ~28) slab inside", c, False))
    d = base.copy(); d[800:900, 1300:1500] = (30, 26, 4); cases.append(("dark yellow (V 0.12) inside", d, False))
    e = base.copy(); e[800:900, 1300:1500] = (40, 220, 230); cases.append(("cyan slab inside", e, False))
    bad = 0
    for name, img, want in cases:
        n, _ = count(img, rect)
        ok = (n > 0) == want
        bad += 0 if ok else 1
        print(("PASS " if ok else "FAIL ") + name + ": " + str(n) + " yellow px (want " + (">0" if want else "0") + ")")
    # the pairs: (on, off, want recoloured, want yellow left on > 0)
    off = base.copy(); off[800:900, 1300:1500] = (230, 200, 20)
    full = base.copy(); full[800:900, 1300:1500] = (40, 220, 230)
    half = off.copy(); half[800:900, 1300:1400] = (40, 220, 230)
    world_off = off.copy(); world_off[650:700, 1100:1150] = (230, 200, 20)
    world_on = full.copy(); world_on[650:700, 1100:1150] = (230, 200, 20)
    pairs = [("pair 1 the whole slab recoloured", full, off, True, False), ("pair 2 nothing recoloured", off, off, False, True),
             ("pair 3 half the slab recoloured (yellow left on the gun)", half, off, True, True),
             ("pair 4 recoloured, a yellow world patch in the rect", world_on, world_off, True, True)]
    for name, a, b, want_rec, want_left in pairs:
        r = pair(a, b, rect)
        rec = r["changedOn"] == 0 and r["changedOff"] > 0
        ok = rec == want_rec and (r["leftOn"] > 0) == want_left
        bad += 0 if ok else 1
        print(("PASS " if ok else "FAIL ") + name + ": " + pair_line(r))
    total = len(cases) + len(pairs)
    print("n62-gun-yellow planted: %d / %d as planted" % (total - bad, total))
    return 1 if bad else 0


def main(argv):
    if argv[:1] == ["planted"]:
        return planted()
    rect = (0.55, 0.55, 1.0, 1.0)
    files = []
    pair_mode = False
    i = 0
    while i < len(argv):
        if argv[i] == "--rect":
            rect = tuple(float(v) for v in argv[i + 1].split(","))
            i += 2
            continue
        if argv[i] == "--pair":
            pair_mode = True
            i += 1
            continue
        files.append(argv[i])
        i += 1
    if pair_mode:
        if len(files) != 2:
            print("usage: n62-gun-yellow.py --pair <on image> <off image> [--rect x0,y0,x1,y1]")
            return 2
        on, off = (np.asarray(Image.open(f).convert("RGB")) for f in files)
        print("%s vs %s: %s" % (files[0], files[1], pair_line(pair(on, off, rect))))
        return 0
    if not files:
        print(__doc__ if __doc__ else "usage: n62-gun-yellow.py <image> [...] [--rect x0,y0,x1,y1] | planted")
        return 2
    for f in files:
        rgb = np.asarray(Image.open(f).convert("RGB"))
        n, area = count(rgb, rect)
        print("%s: %d yellow px in rect %s (%.3f %% of the rect)" % (f, n, ",".join("%.2f" % v for v in rect), 100.0 * n / max(area, 1)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

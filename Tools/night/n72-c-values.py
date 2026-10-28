# NIGHT72 builder C (slate C2 palette72, C5 blank walls): the VALUES meter. It reads reel frames (1920x1080 stills; tour65's
# shot-*.png or any png / jpg) and prints, per frame and per level, what C2's doneWhen asks:
#   1. FLOOR vs WALL value: the median luma (Rec. 709 on the sRGB values, 0-255) in two FIXED screen masks (fractions of the frame, from the
#      top-left; set before the first measurement and never changed after one):
#        floor mask  x 0.30-0.70, y 0.70-0.92   (the ground in front of the player; clear of the HUD's corners and the gun)
#        wall mask   x 0.00-0.22 and 0.80-1.00, y 0.22-0.60   (the side walls at head height; clear of the HUD's top corners)
#      doneWhen: |floor - wall| >= 20 in every tour pose (FLOOR_WALL_MIN).
#   2. TOP HUE share per level: the pixels outside the HUD corners with S >= 0.25 and V >= 0.15 ("coloured"; HSV on the sRGB frame), binned in
#      30-degree hue bins; the biggest 60-degree window (two neighbouring bins, wrapping: the first cut read single bins and split the spire's
#      olive across a bin edge, 42 % where the window reads it higher: a stricter measure, made before any ON frame existed) as a share of the
#      coloured pixels of the level's frames pooled (and of the whole frame). doneWhen: no level's top window covers more than 60 % of its
#      coloured pixels (TOP_HUE_MAX).
#   3. CYAN FIRST: among the vivid pixels (S >= 0.50, V >= 0.40) of the NON-role hues (everything outside danger 345-35, parry 45-70, rival
#      285-335 and cyan 160-205), the 99th-percentile luma must not exceed the cyan's (a family with under 0.05 % of the frame is ignored).
#   4. BLANK FACE (C5): the share of the frame (outside the HUD) in the biggest 4-connected region of near-flat colour (16 x 16 block means
#      within BLANK_TOL of the seed block's and BLANK_STEP of the neighbour reached from): doneWhen < 25 % in the named rooms, and falling on vs off.
#   5. RED ON THE WORLD (C1 / C2's "red-orange only on danger"): the share of vivid danger-band pixels (hue 345-35, S >= 0.55, V >= 0.35) that
#      sit OUTSIDE the enemy / danger blobs is not separable from a still; the meter prints the share of the frame's vivid danger-band pixels
#      and the share of them in the wall mask (an enemy is rarely in the side walls at head height): the number C1 / C2 must lower.
#   python -I Tools/night/n72-c-values.py <folder or image> [...]            per frame lines, then per folder (level) lines
#   python -I Tools/night/n72-c-values.py --pair <on> <off>                   the same, the two sides' per-level numbers side by side
#   python -I Tools/night/n72-c-values.py selftest                            planted frames: every number above has a frame that must read it
import glob
import json
import os
import sys

import numpy as np
from PIL import Image

FLOOR = (0.30, 0.70, 0.70, 0.92)  # x0, y0, x1, y1
WALLS = [(0.00, 0.22, 0.22, 0.60), (0.80, 0.22, 1.00, 0.60)]
HUD = [(0.0, 0.84, 0.26, 1.0), (0.80, 0.82, 1.0, 1.0), (0.0, 0.0, 0.26, 0.13), (0.90, 0.0, 1.0, 0.10)]
FLOOR_WALL_MIN = 20.0
TOP_HUE_MAX = 0.60
COLOURED_S, COLOURED_V = 0.25, 0.15
VIVID_S, VIVID_V = 0.50, 0.40
ROLE = [("danger", 345.0, 35.0), ("parry", 45.0, 70.0), ("player", 160.0, 205.0), ("rival", 285.0, 335.0)]
BLANK_BLOCK, BLANK_TOL, BLANK_STEP = 16, 14, 8  # calibrated on the baseline's iceline g3 (34 %) and foundry a8 (24 %) before any ON frame existed
BLANK_MAX = 0.25
FAMILY_MIN = 0.0005


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
    h[rm] = (60 * (g[rm] - b[rm]) / d[rm]) % 360
    h[gm] = 60 * (b[gm] - r[gm]) / d[gm] + 120
    h[bm] = 60 * (r[bm] - g[bm]) / d[bm] + 240
    return h, s, mx


def in_arc(h, lo, hi):
    return (h >= lo) & (h <= hi) if lo <= hi else (h >= lo) | (h <= hi)


def luma(rgb):
    a = rgb.astype(np.float32)
    return 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]


def hud_mask(shape):
    H, W = shape[:2]
    m = np.ones((H, W), bool)
    for x0, y0, x1, y1 in HUD:
        m[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)] = False
    return m


def box(shape, r):
    H, W = shape[:2]
    m = np.zeros((H, W), bool)
    m[int(r[1] * H):int(r[3] * H), int(r[0] * W):int(r[2] * W)] = True
    return m


def blank_share(rgb, ok):
    """The biggest region of near-flat colour as a share of the ok pixels: 16 x 16 block MEANS (the CRT's scanlines and the dither put +-15 on
    every pixel of a flat wall, so a per-pixel flatness reads 0 % everywhere: first cut, measured on iceline g3 and foundry a8), a region grown
    from a seed block over 4-connected blocks within BLANK_TOL of the seed mean and BLANK_STEP of the neighbour it was reached from."""
    H, W = rgb.shape[:2]
    bs = BLANK_BLOCK
    nby, nbx = H // bs, W // bs
    m = rgb[:nby * bs, :nbx * bs].astype(np.float32).reshape(nby, bs, nbx, bs, 3).mean(axis=(1, 3))
    okb = ok[:nby * bs, :nbx * bs].reshape(nby, bs, nbx, bs).all(axis=(1, 3))
    seen = np.zeros((nby, nbx), bool)
    best = 0
    for y in range(nby):
        for x in range(nbx):
            if seen[y, x] or not okb[y, x]:
                continue
            seed = m[y, x]
            stack = [(y, x)]
            seen[y, x] = True
            n = 0
            while stack:
                cy, cx = stack.pop()
                n += 1
                for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1)):
                    if 0 <= ny < nby and 0 <= nx < nbx and okb[ny, nx] and not seen[ny, nx]:
                        if np.abs(m[ny, nx] - seed).max() <= BLANK_TOL and np.abs(m[ny, nx] - m[cy, cx]).max() <= BLANK_STEP:
                            seen[ny, nx] = True
                            stack.append((ny, nx))
            best = max(best, n)
    return best * bs * bs / max(1, int(ok.sum()))


def measure(rgb):
    H, W = rgb.shape[:2]
    ok = hud_mask(rgb.shape)
    L = luma(rgb)
    h, s, v = hsv(rgb)
    fm = box(rgb.shape, FLOOR) & ok
    wm = (box(rgb.shape, WALLS[0]) | box(rgb.shape, WALLS[1])) & ok
    floor_l = float(np.median(L[fm]))
    wall_l = float(np.median(L[wm]))
    col = ok & (s >= COLOURED_S) & (v >= COLOURED_V)
    bins = np.zeros(12)
    for i in range(12):
        bins[i] = (col & (h >= i * 30) & (h < (i + 1) * 30)).sum()
    # the biggest 60-degree window (two neighbouring 30-degree bins, wrapping): a family such as the spire's olive (hue 80-110) straddles a bin edge
    win = bins + np.roll(bins, -1)
    wtop = int(win.argmax())
    viv = ok & (s >= VIVID_S) & (v >= VIVID_V)
    cyan = viv & in_arc(h, 160, 205)
    nonrole = viv.copy()
    for _, lo, hi in ROLE:
        nonrole &= ~in_arc(h, lo, hi)
    total = float(ok.sum())

    def p99(m):
        return float(np.percentile(L[m], 99)) if m.sum() >= FAMILY_MIN * total else None
    cyan_p99 = p99(cyan)
    nonrole_p99 = p99(nonrole)
    cyan_first = True if nonrole_p99 is None or cyan_p99 is None else (cyan_p99 >= nonrole_p99)
    dang = ok & (s >= 0.55) & (v >= 0.35) & in_arc(h, 345, 35)
    out = {
        "floor_luma": round(floor_l, 1), "wall_luma": round(wall_l, 1), "floor_wall": round(abs(floor_l - wall_l), 1),
        "coloured_share": round(float(col.sum()) / total, 4), "bins": bins.tolist(),
        "top_bin": wtop * 30, "top_share_of_coloured": round(float(win.max() / max(1.0, bins.sum())), 3),
        "top_share_of_frame": round(float(win.max()) / total, 3),
        "floor_rgb": [int(x) for x in np.median(rgb[fm], axis=0)], "wall_rgb": [int(x) for x in np.median(rgb[wm], axis=0)],
        "cyan_p99": None if cyan_p99 is None else round(cyan_p99, 1), "nonrole_p99": None if nonrole_p99 is None else round(nonrole_p99, 1),
        "cyan_first": bool(cyan_first),
        "danger_vivid_share": round(float(dang.sum()) / total, 4),
        "danger_vivid_in_walls": round(float((dang & wm).sum()) / max(1.0, float(wm.sum())), 4),
        "blank_share": round(float(blank_share(rgb, ok)), 3),
    }
    return out, bins


def load(path):
    return np.asarray(Image.open(path).convert("RGB"))


def frames_of(arg):
    if os.path.isdir(arg):
        got = sorted(glob.glob(os.path.join(arg, "shot-*.png"))) or sorted(glob.glob(os.path.join(arg, "*.png"))) or sorted(glob.glob(os.path.join(arg, "*.jpg")))
        return got
    return [arg]


def run_folder(arg, quiet=False):
    files = frames_of(arg)
    rows, pooled = [], np.zeros(12)
    for f in files:
        m, bins = measure(load(f))
        pooled += bins
        rows.append((os.path.basename(f), m))
        if not quiet:
            print("%-44s floor %5.1f %s wall %5.1f %s gap %5.1f | top hue %3d-%3d %3.0f%% | cyan first %s | danger vivid %.2f%% (walls %.2f%%) | blank %.0f%%" % (
                rows[-1][0][14:-4][:44], m["floor_luma"], m["floor_rgb"], m["wall_luma"], m["wall_rgb"], m["floor_wall"], m["top_bin"], (m["top_bin"] + 60) % 360,
                100 * m["top_share_of_coloured"], m["cyan_first"], 100 * m["danger_vivid_share"], 100 * m["danger_vivid_in_walls"], 100 * m["blank_share"]))
    if not rows:
        return None
    gaps = [m["floor_wall"] for _, m in rows]
    pwin = pooled + np.roll(pooled, -1)  # the biggest 60-degree window of the pooled bins (see measure)
    top_pool = float(pwin.max() / max(1.0, pooled.sum()))
    summ = {
        "folder": arg, "frames": len(rows), "worst_gap": min(gaps), "mean_gap": round(sum(gaps) / len(gaps), 1),
        "frames_gap_ok": sum(1 for g in gaps if g >= FLOOR_WALL_MIN), "top_hue_bin": int(pwin.argmax()) * 30, "top_hue_pooled_share": round(top_pool, 3),
        "cyan_first_frames": sum(1 for _, m in rows if m["cyan_first"]),
        "danger_vivid_mean": round(sum(m["danger_vivid_share"] for _, m in rows) / len(rows), 4),
        "danger_vivid_walls_mean": round(sum(m["danger_vivid_in_walls"] for _, m in rows) / len(rows), 4),
        "blank_mean": round(sum(m["blank_share"] for _, m in rows) / len(rows), 3), "blank_worst": max(m["blank_share"] for _, m in rows),
        "verdict_gap": "PASS" if min(gaps) >= FLOOR_WALL_MIN else "RED (%d / %d poses under %d)" % (sum(1 for g in gaps if g < FLOOR_WALL_MIN), len(gaps), FLOOR_WALL_MIN),
        "verdict_hue": "PASS" if top_pool <= TOP_HUE_MAX else "RED (top hue %.0f%% > %.0f%%)" % (100 * top_pool, 100 * TOP_HUE_MAX),
    }
    return rows, summ


def print_summary(s):
    print("== %s: %d frames | floor-wall gap worst %.1f mean %.1f, %d / %d >= %d -> %s | top hue window %d-%d deg pooled %.0f%% -> %s | cyan first %d / %d | danger vivid mean %.2f%% (in walls %.2f%%) | blank mean %.0f%% worst %.0f%%" % (
        s["folder"], s["frames"], s["worst_gap"], s["mean_gap"], s["frames_gap_ok"], s["frames"], FLOOR_WALL_MIN, s["verdict_gap"], s["top_hue_bin"], (s["top_hue_bin"] + 60) % 360,
        100 * s["top_hue_pooled_share"], s["verdict_hue"], s["cyan_first_frames"], s["frames"], 100 * s["danger_vivid_mean"], 100 * s["danger_vivid_walls_mean"],
        100 * s["blank_mean"], 100 * s["blank_worst"]))


def selftest():
    H, W = 1080, 1920
    fails = []

    def check(name, cond, detail=""):
        print(("ok   " if cond else "FAIL ") + name + (" " + detail if detail else ""))
        if not cond:
            fails.append(name)

    def frame(floor_rgb, wall_rgb, base=(40, 40, 50)):
        a = np.zeros((H, W, 3), np.uint8)
        a[:, :] = base
        a[int(0.62 * H):, :] = floor_rgb
        a[:int(0.62 * H), :int(0.25 * W)] = wall_rgb
        a[:int(0.62 * H), int(0.78 * W):] = wall_rgb
        return a
    # planted 1: floor and wall the same colour: gap 0 (RED); one hue: top share 1.0
    same = frame((80, 140, 70), (80, 140, 70), base=(80, 140, 70))
    m, _ = measure(same)
    check("same-tone green frame: gap < 20", m["floor_wall"] < FLOOR_WALL_MIN, str(m["floor_wall"]))
    check("same-tone green frame: top hue share of coloured >= 0.95", m["top_share_of_coloured"] >= 0.95, str(m["top_share_of_coloured"]))
    check("same-tone green frame: blank share > 0.5 (a flat frame)", m["blank_share"] > 0.5, str(m["blank_share"]))
    # planted 2: a dark violet wall over a light tan floor: gap >= 20, two hues, top share under 0.9
    two = frame((200, 170, 120), (40, 30, 90), base=(60, 60, 70))
    m, _ = measure(two)
    check("tan floor / violet wall: gap >= 20", m["floor_wall"] >= FLOOR_WALL_MIN, str(m["floor_wall"]))
    check("tan floor / violet wall: top hue share of coloured < 0.9", m["top_share_of_coloured"] < 0.9, str(m["top_share_of_coloured"]))
    # planted 3: a bright vivid green patch brighter than a cyan patch: cyan not first; then a brighter cyan: first
    f = frame((90, 90, 100), (60, 60, 70))
    f[300:420, 900:1100] = (60, 255, 80)
    f[300:420, 1200:1260] = (25, 160, 170)
    m, _ = measure(f)
    check("bright green over a dimmer cyan: cyan_first False", m["cyan_first"] is False, "cyan p99 %s non-role p99 %s" % (m["cyan_p99"], m["nonrole_p99"]))
    f = frame((90, 90, 100), (60, 60, 70))
    f[300:420, 900:1100] = (40, 120, 60)
    f[300:420, 1200:1400] = (60, 240, 250)
    m, _ = measure(f)
    check("dim green under a bright cyan: cyan_first True", m["cyan_first"] is True, "cyan p99 %s non-role p99 %s" % (m["cyan_p99"], m["nonrole_p99"]))
    # planted 4: a red-orange stripe on the left wall counts as danger vivid in the walls
    f = frame((90, 90, 100), (60, 60, 70))
    f[400:440, 0:230] = (230, 70, 25)
    m, _ = measure(f)
    check("red-orange stripe on the wall: danger vivid in walls > 0.02", m["danger_vivid_in_walls"] > 0.02, str(m["danger_vivid_in_walls"]))
    # planted 5: a frame of 48 px patches in random colours (a busy wall) is not blank; a flat frame with scanline noise (+-15) still is; the HUD corner is ignored
    rng = np.random.default_rng(7)
    patch = rng.integers(20, 200, (H // 48 + 1, W // 48 + 1, 3), dtype=np.uint8)
    f = np.kron(patch, np.ones((48, 48, 1), dtype=np.uint8))[:H, :W]
    m, _ = measure(f)
    check("busy 48 px patches: blank share < 0.1", m["blank_share"] < 0.1, str(m["blank_share"]))
    flat = np.zeros((H, W, 3), np.int16)
    flat[:, :] = (30, 50, 80)
    flat += (np.arange(H)[:, None, None] % 2) * 15 - 7  # a scanline pattern on every other row
    flat = np.clip(flat, 0, 255).astype(np.uint8)
    mf, _ = measure(flat)
    check("flat navy with scanlines (+-7 / 15): blank share > 0.9", mf["blank_share"] > 0.9, str(mf["blank_share"]))
    f2 = f.copy()
    f2[int(0.86 * H):, :int(0.2 * W)] = (255, 0, 0)  # the HUD's bottom-left corner
    m2, _ = measure(f2)
    check("HUD corner red ignored: danger vivid unchanged", abs(m2["danger_vivid_share"] - m["danger_vivid_share"]) < 1e-6)
    print("selftest: %s" % ("PASS" if not fails else "FAIL " + ", ".join(fails)))
    return 0 if not fails else 1


def main(argv):
    if len(argv) >= 2 and argv[1] == "selftest":
        return selftest()
    if len(argv) >= 4 and argv[1] == "--pair":
        a, b = run_folder(argv[2], True), run_folder(argv[3], True)
        for tag, r in (("ON ", a), ("OFF", b)):
            if r:
                print(tag, end=" ")
                print_summary(r[1])
        return 0
    if len(argv) < 2:
        print("usage: n72-c-values.py <folder|image>... | --pair <on> <off> | selftest")
        return 2
    for arg in argv[1:]:
        r = run_folder(arg)
        if r:
            print_summary(r[1])
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

"""NIGHT74 builder L (L1): the MOVING-CAMERA flicker meter (a flip-flop test on 8 x 8 block means of a motion-compensated difference; Research/2026-10-27-forge74/
scout-flicker.md's fallback, with the warp its "Recommended" path asks for: a yaw sweep is a pure rotation, so the previous frame can be warped to this camera exactly).
Reads the pan74 reel's stack files (Assets/Scripts/Game/Pan74.cs documents the layout: "P74B", version 2, blocksWide, blocksHigh, frames, block, planes, then a gzip
stream, per frame: blocksWide * blocksHigh uint16 LUMA SUMS of one block x block pixel block (mean = sum / 64), then as many int16 RESIDUAL SUMS: the block's sum of
luma(this frame) - luma(the previous frame warped to this camera); frame 0 has none) and prints, per sweep:
  - THE TEST. r[k] is a block's mean residual (levels, 0-255; 0 when the picture is exactly what the camera's turn predicts). A FLIP at frame k is counted when r[k] and
    r[k+1] have OPPOSITE signs (each over STEP_MIN levels: a smaller step has no sign) and |r[k+1] - r[k]| > TAU ("changed, then returned": US6154258's sign-reversal
    test on the compensated first differences). A block is FLAGGED in the 6-frame window ending at k when it flipped at least twice in it (fades and slides are
    monotone, so they never flag). The first and last block column are left out (the turn brings them from outside the picture). With --raw the test runs on the plain
    differences of the block means instead (no compensation: a thin line or hard edge crossing a block is a sawtooth the test reads as flicker; a synthetic calm pan
    flagged 18 % of its block-windows that way, 0 compensated).
  - FLICKER SHARE: flagged block-windows over all block-windows of the sweep, in per cent; also the per-frame share (p50 / p95 / max), the share of blocks flagged
    at least once, the mean flip amplitude (min(|r|) / 2: a +-A alternation reads A), the flips of the frame-mean luma (informational), the COMPENSATION figure (mean |r|
    against the mean plain |dY|, levels: how much of the camera's turn the warp explained; a weak figure means the warp's conventions failed and the share is not to be
    trusted), and the top REGIONS: blocks flagged in at least REGION_MIN_FRAMES windows joined across neighbours, with their boxes in pixels (where to look).
  - a HEAT MAP PNG per sweep (--out): the sweep's middle frame (shot-N74-pan74-<level>-<room>-mid.png beside the file, when it exists) dimmed, blocks flagged in
    1-2 windows in blue, 3 or more in orange rising to white at 20.
--world (every command; NIGHT74, the meter sharpened): the screen-fixed HUD and gun rectangles (WORLD_MASK_RECTS) are left out of the share, the denominators and the compensation figure: the warp moves them
although they do not move, so their residual (and the gun's idle sway) was counted as the level's flicker. Off by default so the n74-t1 / t2 numbers stay comparable; use it on BOTH sides of a compare.
THE BARS (written 2026-10-27 before any measurement; never loosened after one, NIGHT74.md rule 6): TAU = 7.65 levels = 3 % of the range: an alternation of +-A levels
gives r = +-2A and |r[k+1] - r[k]| = 4 A, so TAU fires from A = 1.91 levels (0.75 %) up and the planted +-3 % control (A = 7.65, 30.6) has a factor 4 to spare; STEP_MIN =
1 level; 2 flips in 6 frames (the scout). The meter ships on a RATIO (candidate / baseline), never an absolute number; the planted control proves the chain sees +-3 %,
and `floor` (two baseline runs of the same build, same path) gives the run-to-run spread a ratio must leave before it means anything.
  python -I Tools/night/n74-flick.py measure <dir | file.p74>... [--out DIR] [--json FILE] [--plant x,y,size] [--expect-plant] [--raw] [--delete-raw]
        one line per sweep, a LEVEL line per level (pooled), REGION lines, PLANT lines when --plant or --expect-plant (default x,y,size 160,544,96 =
        UnityTuning.N74Plant*; the sweeps carry the planted block when the reel ran with -skygrindFlickerPlant74): FIRES when at least 90 % of its blocks flag;
        exit 3 when --expect-plant and any sweep does not fire
  python -I Tools/night/n74-flick.py compare <baseline dir> <candidate dir> [--out DIR] [--json FILE] [--raw]
        pairs the sweeps by name; per sweep and per level the candidate's share against the baseline's; SUMMARY: levels down by a third or more, levels worse
  python -I Tools/night/n74-flick.py floor <run A dir> <run B dir> [--raw]
        the same pairs for two runs of ONE build: the run-to-run ratio spread per level and the block-wise |A - B| (a deterministic build reads 0)
  python -I Tools/night/n74-flick.py rank <plain dir> <label>=<dir> [<label>=<dir> ...] [--json FILE]
        L4's ranking: each labelled run has ONE flicker source switched off (-skygrindFlickerOff72=<source>) or a row on; sorted by the pooled share each took off the plain
        run, per level too; TOP2 names the two that removed the most
  python -I Tools/night/n74-flick.py pads <baseline dir> <candidate dir> [--world] [--out DIR] [--json FILE]
        (NIGHT74 pads74) the flicker on the jump pads, a row ON against the same build OFF. The pan74 reel writes `pan74-<level>-<room>.pads` beside each sweep when the level has pads
        (Assets/Scripts/Game/Pan74PadsMath.cs: per frame and pad, the screen rectangle of the pad volume (halo, stream, chevrons) and of the stream column, and whether the pad is in
        view). Per sweep both runs are tested as `measure` does; a window's pad region is every block a seen pad's rectangle covered in the 8 frames the window spans (the baseline's
        file is used for both runs: the camera path is the same). Prints, per sweep and pooled: the flagged block-windows inside the pad volume and inside the stream column, base
        against cand, the pooled share of all sweeps both ways, and the two bars (pad regions down >= 50 %, pooled share not worse; a baseline that flags fewer than MIN_PAD_FLAGS
        block-windows on the pads is THIN: no ratio). --out writes <sweep>.pads.png: the base heat map and the cand's side by side with the pad rectangles of the middle frame.
  python -I Tools/night/n74-flick.py selftest
        synthetic sweeps through the stack file's own layout: calm / +-1 level noise / a slow fade read ~0; the planted +-3 % block, a +-1 % block and a thin z-fight
        strip FIRE; a +-0.5 % block does not (under TAU); the raw test's false flags on the calm pan; the file round trip; two identical runs read 0 apart
        (python, numpy, scipy and PIL are the machine's)
"""
import gzip
import json
import os
import struct
import sys
import zlib

import numpy as np

MAGIC = b"P74B"
VERSION = 2
HEADER = 28
TAU = 7.65             # levels: |r[k+1] - r[k]| over this (see the header)
STEP_MIN = 1.0         # levels: a residual of this size or less has no sign
WINDOW = 6             # frames
FLIPS_NEEDED = 2       # flips in a window to flag a block
EDGE = 1               # block columns left out at each side of the compensated picture
REGION_MIN_FRAMES = 3  # flagged windows for a block to belong to a region
GLOBAL_TAU = 5.1       # levels: |d2| of the frame-mean luma for a (informational) global flip: 2 % of the range
PLANT_DEFAULT = (160, 544, 96)  # UnityTuning.N74PlantX / N74PlantY / N74PlantSize
FIRE_SHARE = 0.9       # the planted block FIRES when this share of its blocks flag at least once
DOWN_BY = 0.33         # the slate's L4: share down by at least a third
RAW = False            # --raw: no compensation
WORLD = False          # --world: the screen-fixed HUD and gun left out of the share (WORLD_MASK_RECTS); off by default so the n74-t1 / t2 numbers stay comparable
# NIGHT74 (the meter sharpened): rectangles of a 1920 x 1080 frame the warp moves although they do not move (the HUD, the gun): x0, y0, x1, y1 in px, scaled to the grid. From the n74-t2 spire frames:
# the room counter top-left, the score and style rank top-right, the boss bar's strip, the health plate bottom-left, the gun and the weapon card lower right.
WORLD_MASK_RECTS = ((0, 0, 440, 160), (1700, 0, 1920, 260), (600, 30, 1320, 140), (0, 900, 500, 1080), (1100, 640, 1920, 1080))


def world_keep(bh, bw, block):
    """[bh, bw] bool: True = a block the world share counts (outside every WORLD_MASK_RECTS rectangle)."""
    keep = np.ones((bh, bw), bool)
    s = bw * block / 1920.0
    for x0, y0, x1, y1 in WORLD_MASK_RECTS:
        keep[int(y0 * s) // block:-(-int(y1 * s) // block), int(x0 * s) // block:-(-int(x1 * s) // block)] = False
    return keep


def read_stack(path):
    """-> (blocksWide, blocksHigh, block, means float32 [frames, bh, bw], resid float32 [frames, bh, bw], partial). A killed Player leaves a truncated gzip: the whole
    frames in it are read."""
    with open(path, "rb") as f:
        head = f.read(HEADER)
        body = f.read()
    if len(head) < HEADER or head[:4] != MAGIC:
        raise ValueError("not a pan74 file: " + path)
    version, bw, bh, frames, block, planes = struct.unpack("<iiiiii", head[4:28])
    if version != VERSION or planes != 2:
        raise ValueError("pan74 version %d planes %d in %s" % (version, planes, path))
    d = zlib.decompressobj(16 + zlib.MAX_WBITS)
    try:
        raw = d.decompress(body)
    except zlib.error as e:
        raise ValueError("bad gzip in %s: %s" % (path, e))
    n = bw * bh
    have = len(raw) // (4 * n)
    partial = have < frames
    frames = min(frames, have)
    if frames < WINDOW + 3:
        raise ValueError("only %d frames in %s" % (frames, path))
    arr = np.frombuffer(raw, dtype="<u1", count=frames * 4 * n).reshape(frames, 4 * n)
    luma = np.ascontiguousarray(arr[:, :2 * n]).view("<u2").reshape(frames, bh, bw)
    resid = np.ascontiguousarray(arr[:, 2 * n:]).view("<i2").reshape(frames, bh, bw)
    k = float(block * block)
    return bw, bh, block, luma.astype(np.float32) / k, resid.astype(np.float32) / k, partial


def first_differences(means, resid):
    """The series the test runs on: the compensated first differences r[1:], or (--raw) the plain differences of the block means."""
    return np.diff(means, axis=0) if RAW else resid[1:]


def detect(d1):
    """-> (flip, flagged, amp), each [len(d1) - 1, bh, bw]: a flip between two consecutive steps, a block flagged in the window ending there, the flip's amplitude."""
    s = np.sign(d1) * (np.abs(d1) > STEP_MIN)
    d2 = d1[1:] - d1[:-1]
    flip = (s[1:] * s[:-1] < 0) & (np.abs(d2) > TAU)
    amp = np.minimum(np.abs(d1[1:]), np.abs(d1[:-1])) / 2.0
    c = np.cumsum(flip, axis=0, dtype=np.int32)
    win = c.copy()
    if c.shape[0] > WINDOW:
        win[WINDOW:] -= c[:-WINDOW]
    return flip, win >= FLIPS_NEEDED, amp


def regions(count, block, top=3):
    """The `top` biggest groups of blocks flagged in REGION_MIN_FRAMES windows or more (8-connected): [(flaggedFrames, blocks, (x0, y0, x1, y1) px)]."""
    from scipy import ndimage
    mask = count >= REGION_MIN_FRAMES
    if not mask.any():
        return []
    lab, n = ndimage.label(mask, structure=np.ones((3, 3), int))
    sums = ndimage.sum(count, lab, index=np.arange(1, n + 1))
    sizes = ndimage.sum(mask, lab, index=np.arange(1, n + 1))
    boxes = ndimage.find_objects(lab)
    res = []
    for i in np.argsort(-sums)[:top]:
        sy, sx = boxes[i]
        res.append((int(sums[i]), int(sizes[i]), (int(sx.start * block), int(sy.start * block), int(sx.stop * block), int(sy.stop * block))))
    return res


def measure(means, resid, block, plant=None):
    frames, bh, bw = means.shape
    d1 = first_differences(means, resid)
    flip, flagged, amp = detect(d1)
    edge = 0 if RAW else EDGE
    if edge:
        flagged[:, :, :edge] = False
        flagged[:, :, bw - edge:] = False
        flip[:, :, :edge] = False
        flip[:, :, bw - edge:] = False
    if WORLD:
        keep = world_keep(bh, bw, block)
        flagged &= keep[None, :, :]
        flip &= keep[None, :, :]
    else:
        keep = np.ones((bh, bw), bool)
    keep[:, :edge] = False
    keep[:, bw - edge:] = False
    inner = int(keep.sum())
    nflag = int(flagged.sum())
    total = flagged.shape[0] * inner
    by_frame = flagged.sum(axis=(1, 2)) * 100.0 / inner
    count = flagged.sum(axis=0).astype(np.int32)
    g = means.mean(axis=(1, 2))
    gd1 = np.diff(g)
    gs = np.sign(gd1)
    gflips = int(((gs[1:] * gs[:-1] < 0) & (np.abs(gd1[1:] - gd1[:-1]) > GLOBAL_TAU)).sum())
    plain = float(np.abs(np.diff(means, axis=0))[:, keep].mean())
    comp = float(np.abs(resid[1:])[:, keep].mean())
    res = {
        "frames": int(frames), "bw": bw, "bh": bh, "block": block, "raw": RAW,
        "flagged": nflag, "windows": total,
        "sharePct": round(100.0 * nflag / total, 5),
        "frameP50": round(float(np.percentile(by_frame, 50)), 4), "frameP95": round(float(np.percentile(by_frame, 95)), 4), "frameMax": round(float(by_frame.max()), 4),
        "everPct": round(100.0 * float((count > 0).sum()) / inner, 4),
        "flips": int(flip.sum()),
        "ampMean": round(float(amp[flagged].mean()), 3) if nflag else 0.0,
        "globalFlips": gflips,
        "meanPlainDiff": round(plain, 4), "meanResidual": round(comp, 4),
        "regions": regions(count, block),
    }
    if plant:
        px, py, ps = plant
        x0, y0, x1, y1 = px // block, py // block, (px + ps) // block, (py + ps) // block
        sub = count[y0:y1, x0:x1]
        fl = flagged[:, y0:y1, x0:x1]
        am = amp[:, y0:y1, x0:x1]
        res["plant"] = {
            "rect": [px, py, ps], "blocks": int(sub.size), "flaggedEver": int((sub > 0).sum()),
            "everShare": round(float((sub > 0).mean()), 4) if sub.size else 0.0,
            "ampMean": round(float(am[fl].mean()), 3) if fl.any() else 0.0,
        }
    return res, count


def heat_png(count, block, bg_path, out_path):
    from PIL import Image
    Image.fromarray(heat_rgb(count, block, bg_path)).save(out_path, optimize=False, compress_level=3)


def heat_rgb(count, block, bg_path):
    """The heat map as an RGB array [h, w, 3] (heat_png saves it)."""
    from PIL import Image
    bh, bw = count.shape
    w, h = bw * block, bh * block
    if bg_path and os.path.exists(bg_path):
        im = Image.open(bg_path).convert("L").resize((w, h))
        base = (np.asarray(im, dtype=np.float32) * 0.35).astype(np.uint8)
    else:
        base = np.zeros((h, w), np.uint8)
    big = np.repeat(np.repeat(count, block, axis=0), block, axis=1)
    img = np.stack([base, base, base], axis=-1)
    one = (big >= 1) & (big < REGION_MIN_FRAMES)
    img[one] = (60, 100, 255)
    hot = big >= REGION_MIN_FRAMES
    t = np.clip((big.astype(np.float32) - REGION_MIN_FRAMES) / 17.0, 0, 1)[hot]
    img[hot] = np.stack([np.full_like(t, 255), 140 + 115 * t, 20 + 235 * t], axis=-1).astype(np.uint8)
    return img


def files_of(args):
    out = []
    for a in args:
        if os.path.isdir(a):
            for root, _d, names in os.walk(a):
                for n in sorted(names):
                    if n.endswith(".p74"):
                        out.append(os.path.join(root, n))
        elif a.endswith(".p74"):
            out.append(a)
    return out


def level_room(base):
    """pan74-<level>-<room> -> (level, room); a level id may hold hyphens (n52-foundry), a room id never does."""
    s = base[len("pan74-"):] if base.startswith("pan74-") else base
    if "-" in s:
        lv, room = s.rsplit("-", 1)
        return lv, room
    return "?", s


def region_text(rs):
    if not rs:
        return "none"
    return "; ".join("%d flagged windows in %d blocks at x %d-%d y %d-%d" % (f, n, b[0], b[2], b[1], b[3]) for f, n, b in rs)


def sweep_line(name, r):
    comp = "compensation: mean |r| %.3f vs plain |dY| %.3f levels (%s)" % (
        r["meanResidual"], r["meanPlainDiff"],
        "raw test, no compensation" if r["raw"] else ("x%.1f less" % (r["meanPlainDiff"] / r["meanResidual"]) if r["meanResidual"] > 0 else "no residual"))
    return ("SWEEP %s | frames %d | grid %dx%d of %d px | flicker share %.4f %% (%d of %d block-windows) | by frame p50 %.3f p95 %.3f max %.3f %% | blocks flagged ever %.2f %% | "
            "flips %d, amp %.2f levels | frame-mean flips %d | %s | regions: %s") % (
        name, r["frames"], r["bw"], r["bh"], r["block"], r["sharePct"], r["flagged"], r["windows"], r["frameP50"], r["frameP95"], r["frameMax"], r["everPct"],
        r["flips"], r["ampMean"], r["globalFlips"], comp, region_text(r["regions"]))


def load(path, plant):
    bw, bh, block, means, resid, partial = read_stack(path)
    r, count = measure(means, resid, block, plant)
    r["file"] = path
    r["partial"] = partial
    return r, count, means


def pooled(rows):
    """Level -> [flagged, windows, sweeps] summed over the sweeps."""
    lv = {}
    for name, r in rows.items():
        if "flagged" not in r:
            continue
        level, _room = level_room(name)
        a = lv.setdefault(level, [0, 0, 0])
        a[0] += r["flagged"]
        a[1] += r["windows"]
        a[2] += 1
    return lv


def pct(a):
    return 100.0 * a[0] / a[1] if a[1] else 0.0


def cmd_measure(argv):
    out_dir, json_path, plant, expect = None, None, None, False
    paths = []
    delete = False
    i = 0
    while i < len(argv):
        a = argv[i]
        if a == "--out":
            out_dir = argv[i + 1]; i += 1
        elif a == "--json":
            json_path = argv[i + 1]; i += 1
        elif a == "--plant":
            plant = tuple(int(v) for v in argv[i + 1].split(",")); i += 1
        elif a == "--expect-plant":
            expect = True
            if plant is None:
                plant = PLANT_DEFAULT
        elif a == "--delete-raw":
            delete = True
        elif a == "--raw":
            pass  # read by main
        else:
            paths.append(a)
        i += 1
    files = files_of(paths)
    if not files:
        print("n74-flick: no .p74 files in " + " ".join(paths))
        return 2
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
    rows = {}
    bad = 0
    for f in files:
        base = os.path.basename(f)[:-4]
        try:
            r, count, _m = load(f, plant)
        except Exception as e:
            print("SWEEP %s | ERROR %s" % (base, e))
            rows[base] = {"error": str(e)}
            bad += 1
            continue
        rows[base] = r
        print(sweep_line(base, r) + (" | PARTIAL (the Player was cut short)" if r["partial"] else ""))
        if plant and "plant" in r:
            p = r["plant"]
            print("PLANT %s | rect x %d y %d size %d | %d of %d blocks flagged at least once (%.1f %%) | amplitude %.2f levels (planted %.2f) | %s" % (
                base, p["rect"][0], p["rect"][1], p["rect"][2], p["flaggedEver"], p["blocks"], 100 * p["everShare"], p["ampMean"], 0.03 * 255,
                "FIRES" if p["everShare"] >= FIRE_SHARE else "DOES NOT FIRE"))
        if out_dir:
            lv, room = level_room(base)
            bg = os.path.join(os.path.dirname(f), "shot-N74-pan74-%s-%s-mid.png" % (lv, room))
            heat_png(count, r["block"], bg, os.path.join(out_dir, base + ".heat.png"))
        if delete:
            os.remove(f)
    for level, a in sorted(pooled(rows).items()):
        mine = [(n, r["sharePct"]) for n, r in rows.items() if "flagged" in r and level_room(n)[0] == level]
        worst = max(mine, key=lambda t: t[1])
        print("LEVEL %s | sweeps %d | pooled flicker share %.4f %% | worst %s %.4f %%" % (level, a[2], pct(a), worst[0], worst[1]))
        regs = []
        for n, r in rows.items():
            if "regions" in r and level_room(n)[0] == level:
                for f, nb, b in r["regions"]:
                    regs.append((f, n, nb, b))
        for rank, (f, n, nb, b) in enumerate(sorted(regs, reverse=True)[:5], 1):
            print("REGION %s #%d | %s | %d flagged windows in %d blocks at x %d-%d y %d-%d" % (level, rank, level_room(n)[1], f, nb, b[0], b[2], b[1], b[3]))
    fired = [rows[n]["plant"]["everShare"] >= FIRE_SHARE for n in rows if "plant" in rows[n]]
    summ = "SUMMARY n74-flick measure | sweeps %d | errors %d" % (len(rows), bad)
    if plant:
        summ += " | planted control: %s" % ("FIRES on %d of %d sweeps" % (sum(fired), len(fired)) if fired else "no sweep")
    print(summ + " | TAU %.2f levels, %d flips in %d frames, %s" % (TAU, FLIPS_NEEDED, WINDOW, "raw (no compensation)" if RAW else "motion-compensated"))
    if json_path:
        with open(json_path, "w", encoding="utf-8") as fh:
            json.dump({"tau": TAU, "stepMin": STEP_MIN, "window": WINDOW, "flipsNeeded": FLIPS_NEEDED, "raw": RAW, "sweeps": rows}, fh, indent=1, default=str)
    if bad:
        return 2
    if expect and (not fired or not all(fired)):
        return 3
    return 0


def paired(a_dir, b_dir):
    a = {os.path.basename(f)[:-4]: f for f in files_of([a_dir])}
    b = {os.path.basename(f)[:-4]: f for f in files_of([b_dir])}
    names = sorted(set(a) & set(b))
    return names, a, b, sorted(set(a) ^ set(b))


def cmd_compare(argv, floor=False):
    out_dir, json_path = None, None
    paths = []
    i = 0
    while i < len(argv):
        if argv[i] == "--out":
            out_dir = argv[i + 1]; i += 1
        elif argv[i] == "--json":
            json_path = argv[i + 1]; i += 1
        elif argv[i] == "--raw":
            pass
        else:
            paths.append(argv[i])
        i += 1
    if len(paths) != 2:
        print("usage: n74-flick.py %s <%s dir> <%s dir>" % ("floor" if floor else "compare", "run A" if floor else "baseline", "run B" if floor else "candidate"))
        return 2
    names, a, b, odd = paired(paths[0], paths[1])
    if not names:
        print("n74-flick: no sweep is in both directories")
        return 2
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
    A, B, X = {}, {}, {}
    for n in names:
        try:
            ra, ca, ma = load(a[n], None)
            rb, cb, mb = load(b[n], None)
        except Exception as e:
            print("PAIR %s | ERROR %s" % (n, e))
            continue
        A[n], B[n] = ra, rb
        change = (100.0 * (rb["sharePct"] - ra["sharePct"]) / ra["sharePct"]) if ra["sharePct"] > 0 else None
        line = "PAIR %s | %s %.4f %% | %s %.4f %% | change %s" % (n, "A" if floor else "base", ra["sharePct"], "B" if floor else "cand", rb["sharePct"],
                                                              ("%+.1f %%" % change) if change is not None else "n/a (base 0)")
        if floor:
            m = min(ma.shape[0], mb.shape[0])
            d = np.abs(ma[:m] - mb[:m])
            X[n] = (float(np.percentile(d, 50)), float(np.percentile(d, 95)), float(np.percentile(d, 99)), float(d.max()))
            line += " | |A-B| per block-frame p50 %.3f p95 %.3f p99 %.3f max %.2f levels" % X[n]
        else:
            drop = (ca - cb)
            line += " | candidate flags %d fewer / %d more block-windows than base" % (int(drop[drop > 0].sum()), int(-drop[drop < 0].sum()))
        print(line)
        if out_dir and not floor:
            lv, room = level_room(n)
            bg = os.path.join(os.path.dirname(b[n]), "shot-N74-pan74-%s-%s-mid.png" % (lv, room))
            heat_png(cb, rb["block"], bg, os.path.join(out_dir, n + ".cand.heat.png"))
            heat_png(ca, ra["block"], bg, os.path.join(out_dir, n + ".base.heat.png"))
    pa, pb = pooled(A), pooled(B)
    down = worse = 0
    spread = []
    for lv in sorted(set(pa) & set(pb)):
        sa, sb = pct(pa[lv]), pct(pb[lv])
        ratio = (sb / sa) if sa > 0 else None
        chg = (100.0 * (sb - sa) / sa) if sa > 0 else None
        verdict = ""
        if not floor:
            if ratio is not None and ratio <= 1 - DOWN_BY:
                down += 1
                verdict = " | DOWN by a third or more"
            if sb > sa:
                worse += 1
                verdict += " | WORSE"
        elif chg is not None:
            spread.append(abs(chg))
        print("LEVEL %s | sweeps %d | %s %.4f %% | %s %.4f %% | ratio %s | change %s%s" % (
            lv, pa[lv][2], "A" if floor else "base", sa, "B" if floor else "cand", sb, ("%.3f" % ratio) if ratio is not None else "n/a",
            ("%+.1f %%" % chg) if chg is not None else "n/a", verdict))
    levels = len(set(pa) & set(pb))
    if floor:
        sweeps = [abs(100.0 * (B[n]["sharePct"] - A[n]["sharePct"]) / A[n]["sharePct"]) for n in A if n in B and A[n]["sharePct"] > 0]
        print("SUMMARY n74-flick floor | levels %d | the run-to-run change of a level's pooled share: max %.1f %%, mean %.1f %% | of one sweep: max %.1f %%, median %.1f %%%s" % (
            levels, max(spread) if spread else 0.0, (sum(spread) / len(spread)) if spread else 0.0, max(sweeps) if sweeps else 0.0, float(np.median(sweeps)) if sweeps else 0.0,
            (" | |A-B| p95 over all sweeps up to %.3f levels (0 = the same pixels both runs)" % max(v[1] for v in X.values())) if X else ""))
    else:
        print("SUMMARY n74-flick compare | levels %d | down by a third or more on %d | worse on %d | only in one directory: %s" % (levels, down, worse, ", ".join(odd) if odd else "none"))
    if json_path:
        with open(json_path, "w", encoding="utf-8") as fh:
            json.dump({"a_or_base": A, "b_or_cand": B, "xrun": X}, fh, indent=1, default=str)
    return 0


def cmd_rank(argv):
    """rank <plain dir> <label>=<dir> ...: each labelled side is a run with ONE flicker source switched off (-skygrindFlickerOff72=<source>) or a row on;
    the table sorts them by how much of the plain run's pooled flicker share each took off, on the sweeps both runs have. Run `floor` on two plain runs first:
    a change under its run-to-run spread is not a source."""
    json_path = None
    rest = []
    i = 0
    while i < len(argv):
        if argv[i] == "--json":
            json_path = argv[i + 1]; i += 1
        elif argv[i] != "--raw":
            rest.append(argv[i])
        i += 1
    if len(rest) < 2 or any("=" not in a for a in rest[1:]):
        print("usage: n74-flick.py rank <plain dir> <label>=<dir> [<label>=<dir> ...]   (a label is the source switched off: bloom, ssao, quant ...)")
        return 2
    plain_dir = rest[0]
    sides = [(a.split("=", 1)[0], a.split("=", 1)[1]) for a in rest[1:]]
    pfiles = {os.path.basename(f)[:-4]: f for f in files_of([plain_dir])}
    plain = {}
    for n, f in pfiles.items():
        try:
            plain[n] = load(f, None)[0]
        except Exception as e:
            print("PLAIN %s | ERROR %s" % (n, e))
    if not plain:
        print("n74-flick: no readable sweep in " + plain_dir)
        return 2
    rows = []
    for label, d in sides:
        sfiles = {os.path.basename(f)[:-4]: f for f in files_of([d])}
        common = [n for n in sorted(plain) if n in sfiles]
        got = {}
        for n in common:
            try:
                got[n] = load(sfiles[n], None)[0]
            except Exception as e:
                print("SIDE %s %s | ERROR %s" % (label, n, e))
        common = [n for n in common if n in got]
        if not common:
            rows.append((label, None))
            continue
        pa = pooled({n: plain[n] for n in common})
        pb = pooled({n: got[n] for n in common})
        tot_a = [sum(v[0] for v in pa.values()), sum(v[1] for v in pa.values())]
        tot_b = [sum(v[0] for v in pb.values()), sum(v[1] for v in pb.values())]
        sa, sb = pct(tot_a), pct(tot_b)
        perlv = {lv: (pct(pa[lv]), pct(pb[lv])) for lv in pa if lv in pb}
        rows.append((label, {"sweeps": len(common), "missing": len(plain) - len(common), "plain": sa, "side": sb, "drop": sa - sb,
                             "dropPct": 100.0 * (sa - sb) / sa if sa > 0 else 0.0, "perLevel": perlv}))
    rows.sort(key=lambda r: -(r[1]["drop"] if r[1] else -1e9))
    print("RANK plain: %d sweeps, pooled flicker share %.4f %%" % (len(plain), pct([sum(r["flagged"] for r in plain.values()), sum(r["windows"] for r in plain.values())])))
    for label, r in rows:
        if r is None:
            print("RANK %-10s | no sweep in common with the plain run" % label)
            continue
        lvs = "; ".join("%s %.3f -> %.3f %%" % (lv, a, b) for lv, (a, b) in sorted(r["perLevel"].items()))
        print("RANK %-10s | pooled %.4f -> %.4f %% | removed %.4f points (%.1f %% of the plain share) | sweeps %d%s | per level: %s" % (
            label, r["plain"], r["side"], r["drop"], r["dropPct"], r["sweeps"], (", " + str(r["missing"]) + " missing") if r["missing"] else "", lvs))
    good = [label for label, r in rows if r and r["drop"] > 0]
    print("TOP2 %s" % (", ".join(good[:2]) if good else "none removed anything"))
    if json_path:
        with open(json_path, "w", encoding="utf-8") as fh:
            json.dump({"rows": [(label, r) for label, r in rows]}, fh, indent=1, default=str)
    return 0


# ---- pads: the flicker on the jump pads, a row ON against the same build OFF (NIGHT74 pads74) ----

MIN_PAD_FLAGS = 40   # block-windows the baseline must flag inside the pad regions for a ratio to mean anything (under it: THIN)
PAD_BAR = 0.5        # the bar (the lead's, 2026-10-28 01:00): the pad regions' flagged block-windows down by at least half
PAD_SPAN = 5         # a window ending at flip index j spans frames j - PAD_SPAN .. j + 2 (the flip test's 6 flips, each over three frames)


def read_pads(path):
    """-> (width, height, {frame: [(pad, seen, vol, stream)]}); vol and stream are (x0, y0, x1, y1) px of the capture, row 0 at the top. A foreign file raises ValueError."""
    frames = {}
    with open(path, "r", encoding="utf-8") as fh:
        head = fh.readline().split()
        if len(head) != 4 or head[0] != "P74PADS" or head[1] != "1":
            raise ValueError("not a pan74 pads file: " + path)
        w, h = int(head[2]), int(head[3])
        for ln in fh:
            p = ln.split()
            if len(p) != 12 or p[0] != "f":
                continue
            v = [int(x) for x in p[1:]]
            frames.setdefault(v[0], []).append((v[1], v[2] == 1, tuple(v[3:7]), tuple(v[7:11])))
    return w, h, frames


def flagged_masked(means, resid, block):
    """measure()'s flagged array [windows, bh, bw] with the masks it applies (the edge columns; --world's HUD and gun rectangles), and the [bh, bw] bool of blocks the share counts."""
    _frames, bh, bw = means.shape
    _flip, flagged, _amp = detect(first_differences(means, resid))
    keep = world_keep(bh, bw, block) if WORLD else np.ones((bh, bw), bool)
    edge = 0 if RAW else EDGE
    if edge:
        flagged[:, :, :edge] = False
        flagged[:, :, bw - edge:] = False
        keep[:, :edge] = False
        keep[:, bw - edge:] = False
    flagged &= keep[None, :, :]
    return flagged, keep


def rect_blocks(mask, rect, block, scale):
    """Marks the blocks a pixel rectangle (x0, y0, x1, y1) touches (scale: grid pixels per capture pixel)."""
    x0, y0, x1, y1 = rect
    bh, bw = mask.shape
    a, b = max(0, int(x0 * scale) // block), min(bw, -(-int(x1 * scale) // block))
    c, d = max(0, int(y0 * scale) // block), min(bh, -(-int(y1 * scale) // block))
    if b > a and d > c:
        mask[c:d, a:b] = 1


def pad_window_masks(frames, nframes, bh, bw, block, width, which):
    """-> (bool [nframes - 2, bh, bw], frames with a pad in view): for the window ending at flip index j (frames j - PAD_SPAN .. j + 2), the blocks any seen pad's rectangle covered
    in any of those frames. `which` 0: the pad volume, 1: the stream column."""
    scale = bw * block / float(width)
    per = np.zeros((nframes, bh, bw), np.uint8)
    seen_frames = 0
    for k in range(nframes):
        got = False
        for _pad, seen, vol, stream in frames.get(k, []):
            if seen:
                rect_blocks(per[k], (vol, stream)[which], block, scale)
                got = True
        seen_frames += 1 if got else 0
    cum = np.cumsum(per, axis=0, dtype=np.int32)
    nwin = nframes - 2
    out = np.zeros((nwin, bh, bw), bool)
    for j in range(nwin):
        lo, hi = max(0, j - PAD_SPAN), min(nframes - 1, j + 2)
        out[j] = (cum[hi] - (cum[lo - 1] if lo > 0 else 0)) > 0
    return out, seen_frames


def pad_sweep(path_a, path_b, pads_path):
    """One sweep, both runs: -> a dict of the pad regions' and the sweep's flagged block-windows (base `a`, cand `b`), and the two flag-count maps for the heat maps."""
    bw, bh, block, ma, ra, _pa = read_stack(path_a)
    bw2, bh2, block2, mb, rb, _pb = read_stack(path_b)
    if (bw, bh, block) != (bw2, bh2, block2):
        raise ValueError("the two runs' grids differ (%dx%d of %d against %dx%d of %d)" % (bw, bh, block, bw2, bh2, block2))
    n = min(ma.shape[0], mb.shape[0])
    ma, ra, mb, rb = ma[:n], ra[:n], mb[:n], rb[:n]
    fa, keep = flagged_masked(ma, ra, block)
    fb, _keep = flagged_masked(mb, rb, block)
    width, _height, frames = read_pads(pads_path)
    res = {"frames": n, "windows": int(fa.shape[0] * keep.sum()),
           "pooled": {"base": int(fa.sum()), "cand": int(fb.sum())}}
    for which, name in ((0, "volume"), (1, "stream")):
        mask, seen_frames = pad_window_masks(frames, n, bh, bw, block, width, which)
        mask &= keep[None, :, :]
        res[name] = {"seenFrames": seen_frames, "blockWindows": int(mask.sum()), "base": int((fa & mask).sum()), "cand": int((fb & mask).sum())}
        if which == 0:
            res["outside"] = {"blockWindows": int(fa.shape[0] * keep.sum() - mask.sum()), "base": int((fa & ~mask).sum()), "cand": int((fb & ~mask).sum())}
    mid = n // 2
    res["midRects"] = [(vol, stream) for _pad, seen, vol, stream in frames.get(mid, []) if seen]
    res["scale"] = bw * block / float(width)
    return res, fa.sum(axis=0).astype(np.int32), fb.sum(axis=0).astype(np.int32), block


def pads_png(ca, cb, block, bg_path, rects, scale, out_path):
    """The base heat map and the cand's side by side (half size), with the pad volume (yellow) and stream column (cyan) rectangles of the middle frame."""
    from PIL import Image, ImageDraw
    ia, ib = heat_rgb(ca, block, bg_path), heat_rgb(cb, block, bg_path)
    h, w, _c = ia.shape
    img = Image.fromarray(np.concatenate([ia, ib], axis=1))
    dr = ImageDraw.Draw(img)
    for off in (0, w):
        for vol, stream in rects:
            dr.rectangle([off + vol[0] * scale, vol[1] * scale, off + vol[2] * scale, vol[3] * scale], outline=(255, 230, 0))
            dr.rectangle([off + stream[0] * scale, stream[1] * scale, off + stream[2] * scale, stream[3] * scale], outline=(0, 220, 255))
    dr.text((8, 8), "base (row OFF)", fill=(255, 255, 255))
    dr.text((w + 8, 8), "cand (row ON)", fill=(255, 255, 255))
    img.resize((w, h // 2)).save(out_path, optimize=False, compress_level=3)


def change_text(a, b):
    return ("%+.1f %%" % (100.0 * (b - a) / a)) if a > 0 else "n/a (base 0)"


def cmd_pads(argv):
    out_dir, json_path = None, None
    paths = []
    i = 0
    while i < len(argv):
        if argv[i] == "--out":
            out_dir = argv[i + 1]; i += 1
        elif argv[i] == "--json":
            json_path = argv[i + 1]; i += 1
        elif argv[i] != "--raw":
            paths.append(argv[i])
        i += 1
    if len(paths) != 2:
        print("usage: n74-flick.py pads <baseline dir> <candidate dir> [--world] [--out DIR] [--json FILE]")
        return 2
    names, a, b, odd = paired(paths[0], paths[1])
    if not names:
        print("n74-flick: no sweep is in both directories")
        return 2
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
    tot = {"pooled": [0, 0, 0], "volume": [0, 0, 0], "stream": [0, 0, 0], "outside": [0, 0, 0]}  # base flagged, cand flagged, block-windows
    rows, nopads, withpads, errors = {}, [], 0, 0
    for n in names:
        pads_path = a[n][:-4] + ".pads"
        try:
            if not os.path.exists(pads_path):
                ra, _ca, _ma = load(a[n], None)
                rb, _cb, _mb = load(b[n], None)
                tot["pooled"][0] += ra["flagged"]
                tot["pooled"][1] += rb["flagged"]
                tot["pooled"][2] += ra["windows"]
                nopads.append(n)
                print("PADS %s | no .pads file (no pad on this level, or an older run) | pooled base %.4f %% cand %.4f %%" % (n, ra["sharePct"], rb["sharePct"]))
                continue
            r, ca, cb, block = pad_sweep(a[n], b[n], pads_path)
        except Exception as e:
            print("PADS %s | ERROR %s" % (n, e))
            errors += 1
            continue
        rows[n] = r
        withpads += 1
        tot["pooled"][0] += r["pooled"]["base"]
        tot["pooled"][1] += r["pooled"]["cand"]
        tot["pooled"][2] += r["windows"]
        for k in ("volume", "stream", "outside"):
            tot[k][0] += r[k]["base"]
            tot[k][1] += r[k]["cand"]
            tot[k][2] += r[k]["blockWindows"]
        v, s, o = r["volume"], r["stream"], r["outside"]
        print("PADS %s | pads in view in %d of %d frames | pad volume (%d block-windows): base %d -> cand %d flagged (%s) | stream column (%d): base %d -> cand %d (%s) | outside the pads (%d): base %d -> cand %d (%s) | sweep share base %.4f %% cand %.4f %%" % (
            n, v["seenFrames"], r["frames"], v["blockWindows"], v["base"], v["cand"], change_text(v["base"], v["cand"]), s["blockWindows"], s["base"], s["cand"], change_text(s["base"], s["cand"]),
            o["blockWindows"], o["base"], o["cand"], change_text(o["base"], o["cand"]), 100.0 * r["pooled"]["base"] / r["windows"], 100.0 * r["pooled"]["cand"] / r["windows"]))
        if out_dir:
            lv, room = level_room(n)
            bg = os.path.join(os.path.dirname(b[n]), "shot-N74-pan74-%s-%s-mid.png" % (lv, room))
            pads_png(ca, cb, block, bg, r["midRects"], r["scale"], os.path.join(out_dir, n + ".pads.png"))
    pb = tot["pooled"]
    sa, sb = (100.0 * pb[0] / pb[2]) if pb[2] else 0.0, (100.0 * pb[1] / pb[2]) if pb[2] else 0.0
    v, s, o = tot["volume"], tot["stream"], tot["outside"]
    if withpads == 0:
        verdict_pads = "NO PAD IN ANY SWEEP (no .pads file: the level has no pad in a toured room, or the run predates the sidecar)"
    elif v[0] < MIN_PAD_FLAGS:
        verdict_pads = "THIN (the baseline flags only %d block-windows on the pads, under %d: no ratio is evidence)" % (v[0], MIN_PAD_FLAGS)
    else:
        verdict_pads = "PASS" if v[1] <= (1 - PAD_BAR) * v[0] else "FAIL"
    verdict_pool = "PASS" if sb <= sa else "FAIL"
    print("SUMMARY n74-flick pads | sweeps %d (with pads %d, without %d, errors %d) | pad volume: base %d -> cand %d flagged of %d block-windows (%s) | stream column: base %d -> cand %d of %d (%s) | "
          "outside the pads: base %d -> cand %d of %d (%s) | pooled share of every paired sweep: base %.4f %% -> cand %.4f %% (%s) | BAR pad regions down >= %d %%: %s | BAR pooled share not worse: %s | only in one directory: %s" % (
              len(names), withpads, len(nopads), errors, v[0], v[1], v[2], change_text(v[0], v[1]), s[0], s[1], s[2], change_text(s[0], s[1]), o[0], o[1], o[2], change_text(o[0], o[1]),
              sa, sb, change_text(sa, sb), int(PAD_BAR * 100), verdict_pads, verdict_pool, ", ".join(odd) if odd else "none"))
    if json_path:
        with open(json_path, "w", encoding="utf-8") as fh:
            json.dump({"sweeps": rows, "totals": tot, "withoutPads": nopads, "barPads": verdict_pads, "barPooled": verdict_pool}, fh, indent=1, default=str)
    return 0 if errors == 0 else 2


# ---- the self-test: synthetic sweeps through the stack file's own layout ----

W_PX, H_PX, FRAMES, SHIFT = 320, 176, 120, 3  # a 40 x 22 grid of 8 px blocks; the picture slides 3 px a frame


def scene(seed=11):
    """A wide texture the camera pans across: a smooth gradient and sinusoids, hard vertical edges, and thin bright lines every 24 px."""
    rng = np.random.default_rng(seed)
    wide = W_PX + SHIFT * FRAMES + 8
    gy, gx = np.mgrid[0:H_PX, 0:wide]
    t = 70 + 50 * np.sin(gx / 37.0) * np.cos(gy / 29.0) + 30 * (gy / H_PX)
    t = t + 40 * ((gx // 160) % 2)                      # hard vertical edges every 160 px
    t = t + 60 * (((gx % 24) < 2) & (gy % 40 < 30))      # thin lines
    t = t + rng.integers(0, 3, size=t.shape)             # fixed texture grain
    return t.astype(np.float32)


def render(wide, kind, plant_rect=(64, 48, 64), amp=7.65, seed=3):
    """The frames of a sweep as uint8 luma [FRAMES, H, W], with the case on top (applied to the picture: the world slides under it)."""
    rng = np.random.default_rng(seed)
    out = np.empty((FRAMES, H_PX, W_PX), np.uint8)
    for k in range(FRAMES):
        f = wide[:, SHIFT * k:SHIFT * k + W_PX].copy()
        if kind == "noise1":
            f += rng.integers(-1, 2, size=f.shape)
        elif kind == "slow":
            f[40:120, 40:200] += 30 * np.sin(k / FRAMES * np.pi)
        elif kind == "plant":
            x, y, s = plant_rect
            f[y:y + s, x:x + s] = 127.5 + (amp if k % 2 == 0 else -amp)
        elif kind == "zfight":
            f[100:104, 40:240] += 24 if (k // 2) % 2 == 0 else -24
        out[k] = np.clip(np.rint(f), 0, 255).astype(np.uint8)
    return out


def pool(frames, block=8):
    n, h, w = frames.shape
    return frames.reshape(n, h // block, block, w // block, block).astype(np.float64).sum(axis=(2, 4))


def residual(frames):
    """The ideal compensated residual: this frame minus the previous one shifted by exactly SHIFT px (zero where the shift has no source); frame 0 none."""
    r = np.zeros(frames.shape, np.float64)
    for k in range(1, frames.shape[0]):
        r[k, :, :W_PX - SHIFT] = frames[k, :, :W_PX - SHIFT].astype(np.float64) - frames[k - 1, :, SHIFT:].astype(np.float64)
    return r


def write_p74(path, frames, block=8):
    sums = pool(frames, block).astype("<u2")
    res = pool(residual(frames), block)
    res = np.rint(res).astype("<i2")
    n, bh, bw = sums.shape
    with open(path, "wb") as fh:
        fh.write(MAGIC + struct.pack("<iiiiii", VERSION, bw, bh, n, block, 2))
        fh.write(gzip.compress(b"".join(sums[k].tobytes() + res[k].tobytes() for k in range(n))))
    return sums, res


def selftest():
    global RAW
    import tempfile
    wide = scene()
    d = tempfile.mkdtemp()
    plant = (64, 48, 64)
    cases = [
        ("calm", None, lambda r: r["sharePct"] < 0.5, "share under 0.5 %"),
        ("noise1", None, lambda r: r["sharePct"] < 0.5, "+-1 level of noise under 0.5 %"),
        ("slow", None, lambda r: r["sharePct"] < 0.5, "a slow fade under 0.5 %"),
        ("plant", plant, lambda r: r["plant"]["everShare"] >= FIRE_SHARE and 6.5 <= r["plant"]["ampMean"] <= 8.5, "the planted +-3 % block FIRES (>= 90 % of its blocks, amplitude 7.65 +- 1)"),
        ("plant1", plant, lambda r: r["plant"]["everShare"] >= FIRE_SHARE, "a planted +-1 % block (2.55 levels) FIRES"),
        ("plant05", plant, lambda r: r["plant"]["everShare"] < 0.1, "a planted +-0.5 % block (1.28 levels) does NOT fire (4 A = 5.1 is under TAU 7.65)"),
        ("zfight", None, lambda r: r["flagged"] > 0 and any(b[0] < 240 and 96 <= b[1] and b[3] <= 112 for _f, _n, b in r["regions"]), "a thin z-fight strip is flagged and found at its place"),
    ]
    bad = 0
    for kind, pr, check, what in cases:
        k2 = {"plant1": "plant", "plant05": "plant"}.get(kind, kind)
        amp = {"plant1": 0.01 * 255, "plant05": 0.005 * 255}.get(kind, 7.65)
        fr = render(wide, k2, plant, amp)
        path = os.path.join(d, "pan74-selftest-%s.p74" % kind)
        sums, _res = write_p74(path, fr)
        bw, bh, block, means, resid, partial = read_stack(path)
        ok_rt = means.shape == sums.shape and bool((means * 64 == sums.astype(np.float32)).all()) and not partial
        r, _count = measure(means, resid, block, pr)
        ok = ok_rt and check(r)
        print("%s %s: %s | share %.4f %% | flips %d | amp %.2f | mean |r| %.3f vs plain %.3f%s" % (
            "ok  " if ok else "FAIL", kind, what, r["sharePct"], r["flips"], r["ampMean"], r["meanResidual"], r["meanPlainDiff"],
            (" | plant %d of %d blocks, amp %.2f" % (r["plant"]["flaggedEver"], r["plant"]["blocks"], r["plant"]["ampMean"])) if "plant" in r else ""))
        bad += 0 if ok else 1
    # the raw test on the calm pan: the false flags the compensation removes
    RAW = True
    path = os.path.join(d, "pan74-selftest-calm.p74")
    bw, bh, block, means, resid, partial = read_stack(path)
    rraw, _c = measure(means, resid, block, None)
    RAW = False
    rcomp, _c = measure(means, resid, block, None)
    ok = rraw["sharePct"] > 5.0 and rcomp["sharePct"] < 0.5
    print("%s the calm pan: raw test flags %.2f %% of its block-windows, the compensated test %.4f %% (the reason for the warp)" % ("ok  " if ok else "FAIL", rraw["sharePct"], rcomp["sharePct"]))
    bad += 0 if ok else 1
    # --world: a planted block in the lower right (where the gun is) is counted without the mask and left out with it
    global WORLD
    lr = (224, 112, 56)
    pathw = os.path.join(d, "pan74-selftest-lr.p74")
    write_p74(pathw, render(wide, "plant", lr, 7.65))
    _bw, _bh, blockw, meansw, residw, _pw = read_stack(pathw)
    WORLD = False
    r_off, _c = measure(meansw, residw, blockw, lr)
    WORLD = True
    r_on, _c = measure(meansw, residw, blockw, lr)
    WORLD = False
    ok = r_off["plant"]["everShare"] >= FIRE_SHARE and r_on["plant"]["flaggedEver"] == 0 and r_on["windows"] < r_off["windows"] and r_on["flagged"] < r_off["flagged"]
    print("%s --world leaves the lower-right gun / HUD rectangle out: a planted block there fires %.0f %% without it, %d blocks with it; the denominator %d -> %d block-windows" % (
        "ok  " if ok else "FAIL", 100 * r_off["plant"]["everShare"], r_on["plant"]["flaggedEver"], r_off["windows"], r_on["windows"]))
    bad += 0 if ok else 1
    # a killed Player: the gzip is cut
    p = os.path.join(d, "pan74-selftest-trunc.p74")
    write_p74(p, render(wide, "calm"))
    with open(p, "rb") as fh:
        raw = fh.read()
    with open(p, "wb") as fh:
        fh.write(raw[:len(raw) - 700])
    try:
        _bw, _bh, _bl, m, _r, partial = read_stack(p)
        ok = partial and 6 < m.shape[0] < FRAMES
        print("%s truncated file reads its whole frames (%d of %d), flagged partial" % ("ok  " if ok else "FAIL", m.shape[0], FRAMES))
    except Exception as e:
        ok = False
        print("FAIL truncated file: %s" % e)
    bad += 0 if ok else 1
    # two identical runs: floor reads 0 apart; compare of a baseline with a planted candidate shows the plant
    da, db = os.path.join(d, "A"), os.path.join(d, "B")
    os.makedirs(da)
    os.makedirs(db)
    write_p74(os.path.join(da, "pan74-n52-selftest-r1.p74"), render(wide, "calm"))
    write_p74(os.path.join(db, "pan74-n52-selftest-r1.p74"), render(wide, "calm"))
    names, a, b, odd = paired(da, db)
    _ra, _ca, ma = load(a[names[0]], None)
    _rb, _cb, mb = load(b[names[0]], None)
    ok = float(np.abs(ma - mb).max()) == 0.0 and not odd
    print("%s two identical runs: |A-B| max %.3f levels" % ("ok  " if ok else "FAIL", float(np.abs(ma - mb).max())))
    bad += 0 if ok else 1
    # rank: a plain run with a z-fight strip and a planted block, a side that lost the strip ("zfight"), a side that lost the block ("plant"), a side that lost nothing ("same")
    import contextlib
    import io
    dr = {k: os.path.join(d, "R" + k) for k in ("plain", "zfight", "plant", "same")}
    for p in dr.values():
        os.makedirs(p)

    both = render(wide, "zfight")
    pl = render(wide, "plant")
    mix = both.copy()
    mix[:, 48:112, 64:128] = pl[:, 48:112, 64:128]       # the planted block over the z-fight picture: both flickers in one run
    calm = render(wide, "calm")
    only_plant = calm.copy()
    only_plant[:, 48:112, 64:128] = pl[:, 48:112, 64:128]
    only_z = both
    write_p74(os.path.join(dr["plain"], "pan74-n52-selftest-r1.p74"), mix)
    write_p74(os.path.join(dr["zfight"], "pan74-n52-selftest-r1.p74"), only_plant)  # the strip's source off: the block stays
    write_p74(os.path.join(dr["plant"], "pan74-n52-selftest-r1.p74"), only_z)       # the block's source off: the strip stays
    write_p74(os.path.join(dr["same"], "pan74-n52-selftest-r1.p74"), mix)
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        rc = cmd_rank([dr["plain"], "zfight=" + dr["zfight"], "plant=" + dr["plant"], "same=" + dr["same"]])
    out = buf.getvalue()
    top = [ln for ln in out.splitlines() if ln.startswith("TOP2")]
    drops = {ln.split()[1]: float(ln.split("removed ")[1].split(" points")[0]) for ln in out.splitlines() if ln.startswith("RANK ") and "removed" in ln}
    ok = rc == 0 and top and top[0].split()[1] == "plant," and abs(drops["same"]) < 1e-9 and drops["plant"] > drops["zfight"] > 0
    print("%s rank: the side that lost the bigger flicker ranks first, an identical side removes 0 (%s; removed %s)" % ("ok  " if ok else "FAIL", top[0] if top else "no TOP2", {k: round(v, 4) for k, v in drops.items()}))
    bad += 0 if ok else 1
    for p in dr.values():
        for f in os.listdir(p):
            os.remove(os.path.join(p, f))
        os.rmdir(p)
    # pads: a run with a flickering block on the "pad" and a z-fight strip elsewhere, against runs that lost the pad's flicker / lost nothing / whose pad was never in view
    dp = {k: os.path.join(d, "P" + k) for k in ("base", "gone", "same", "unseen")}
    for p in dp.values():
        os.makedirs(p)
    pad_rect = (64, 16, 64)
    plp = render(wide, "plant", pad_rect, 7.65)
    mixp = both.copy()
    mixp[:, 16:80, 64:128] = plp[:, 16:80, 64:128]
    vol, stream = (56, 8, 136, 88), (80, 24, 112, 72)  # round the planted block (rows 1-10; the z-fight strip is row 12)

    def write_pads(path, seen):
        with open(path, "w", encoding="utf-8") as fh:
            fh.write("P74PADS 1 %d %d\n" % (W_PX, H_PX))
            for k in range(FRAMES):
                fh.write("f %d 0 %d %s %s\n" % (k, 1 if seen else 0, " ".join(str(v) for v in (vol if seen else (0, 0, 0, 0))), " ".join(str(v) for v in (stream if seen else (0, 0, 0, 0)))))

    name = "pan74-n52-selftest-r1"
    write_p74(os.path.join(dp["base"], name + ".p74"), mixp)
    write_p74(os.path.join(dp["gone"], name + ".p74"), both)    # the pad's flicker gone, the strip stays
    write_p74(os.path.join(dp["same"], name + ".p74"), mixp)
    write_p74(os.path.join(dp["unseen"], name + ".p74"), mixp)
    write_pads(os.path.join(dp["base"], name + ".pads"), True)
    jp = os.path.join(d, "pads.json")
    results = {}
    for side in ("gone", "same"):
        with contextlib.redirect_stdout(io.StringIO()):
            rc = cmd_pads([dp["base"], dp[side], "--json", jp])
        with open(jp, "r", encoding="utf-8") as fh:
            results[side] = (rc, json.load(fh))
    os.remove(os.path.join(dp["base"], name + ".pads"))
    write_pads(os.path.join(dp["base"], name + ".pads"), False)
    with contextlib.redirect_stdout(io.StringIO()):
        rc = cmd_pads([dp["base"], dp["unseen"], "--json", jp])
    with open(jp, "r", encoding="utf-8") as fh:
        results["unseen"] = (rc, json.load(fh))
    g, s, u = results["gone"][1], results["same"][1], results["unseen"][1]
    gv, sv = g["totals"]["volume"], s["totals"]["volume"]
    ok = (results["gone"][0] == 0 and gv[0] >= MIN_PAD_FLAGS and gv[1] <= 0.1 * gv[0] and g["barPads"] == "PASS" and g["barPooled"] == "PASS"
          and g["totals"]["outside"][0] > 0 and g["totals"]["outside"][1] == g["totals"]["outside"][0] and g["totals"]["pooled"][1] < g["totals"]["pooled"][0])
    print("%s pads: the flicker taken off the pad is read as such (pad volume %d -> %d flagged block-windows, the z-fight strip outside the pads unchanged at %d, pooled %d -> %d; bars %s / %s)" % (
        "ok  " if ok else "FAIL", gv[0], gv[1], g["totals"]["outside"][0], g["totals"]["pooled"][0], g["totals"]["pooled"][1], g["barPads"], g["barPooled"]))
    bad += 0 if ok else 1
    ok = sv[0] >= MIN_PAD_FLAGS and sv[1] == sv[0] and s["barPads"] == "FAIL" and s["barPooled"] == "PASS" and s["totals"]["stream"][0] > 0
    print("%s pads: an identical run changes nothing (pad volume %d -> %d, stream column %d -> %d; the pad bar reads %s)" % (
        "ok  " if ok else "FAIL", sv[0], sv[1], s["totals"]["stream"][0], s["totals"]["stream"][1], s["barPads"]))
    bad += 0 if ok else 1
    ok = u["totals"]["volume"][2] == 0 and u["barPads"].startswith("THIN")
    print("%s pads: a pad never in view leaves an empty region and no ratio (%d block-windows; bar %s)" % ("ok  " if ok else "FAIL", u["totals"]["volume"][2], u["barPads"][:4]))
    bad += 0 if ok else 1
    for p in dp.values():
        for f in os.listdir(p):
            os.remove(os.path.join(p, f))
        os.rmdir(p)
    os.remove(jp)
    lv, room = level_room("pan74-n52-selftest-r1")
    ok = (lv, room) == ("n52-selftest", "r1") and level_room("pan74-foundry-a2") == ("foundry", "a2")
    print("%s level / room from a file name (hyphens in the level id)" % ("ok  " if ok else "FAIL"))
    bad += 0 if ok else 1
    for sub in (da, db, d):
        for f in os.listdir(sub):
            fp = os.path.join(sub, f)
            if os.path.isfile(fp):
                os.remove(fp)
    for sub in (da, db, d):
        os.rmdir(sub)
    print("SELFTEST n74-flick: %s" % ("PASS" if bad == 0 else "FAIL (%d)" % bad))
    return bad


def main(argv):
    global RAW, WORLD
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__)
        return 0
    cmd, rest = argv[0], argv[1:]
    if "--raw" in rest:
        RAW = True
    if "--world" in rest:
        WORLD = True
        rest = [a for a in rest if a != "--world"]
    if cmd == "selftest":
        return 1 if selftest() else 0
    if cmd == "measure":
        return cmd_measure(rest)
    if cmd == "compare":
        return cmd_compare(rest, False)
    if cmd == "floor":
        return cmd_compare(rest, True)
    if cmd == "rank":
        return cmd_rank(rest)
    if cmd == "pads":
        return cmd_pads(rest)
    print("n74-flick: unknown command " + cmd + " (measure | compare | floor | rank | pads | selftest)")
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

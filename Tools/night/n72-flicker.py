"""NIGHT72 builder L (L1): the flicker meter. Reads the still72 reel's stack files (Assets/Scripts/Game/Still72.cs documents the layout:
"S72L", version, width, height, frames, then a gzip stream of frame 0's luma and every later frame as the byte difference to the one before)
and prints, per pose, the numbers NIGHT72's slate L1 asks for:
  - FLIP SHARE: the share of the frame's pixels whose luma REVERSES direction 3 or more times in the 60 frames, each swing larger than 6 levels
    (a swing is counted when the value has moved MORE than 6 levels back from its last extreme: hysteresis, so noise of 6 levels peak to peak
    or less counts for nothing);
  - p99.9 |dY| : the 99.9th percentile of |luma[k] - luma[k-1]| over every pixel and every step;
  - a HEATMAP PNG (the pose's first frame dimmed, flipping pixels in orange to white by their flip count, 1-2 flip pixels in blue) and the three
    biggest blobs of flipping pixels with their boxes, to say WHERE;
  - the pose's VERDICT: RED when the flip share is at or over BAR_SHARE_PCT, else GREEN.
THE BAR (written 2026-10-27 before any measurement; never loosened after one, NIGHT72.md rule 6): BAR_SHARE_PCT = 0.01 % of the frame, 207 px at
1920x1080, about a 14 x 14 patch. The slate's planted case is a 32 x 32 block (1024 px, 0.049 %), 4.9 times the bar.
Usage (python -I; numpy, scipy and PIL are the machine's):
  python -I Tools/night/n72-flicker.py <dir | file.s72>... [--out DIR] [--plant x,y,size] [--delete-raw] [--json FILE]
  python -I Tools/night/n72-flicker.py --selftest        (synthetic stacks: calm GREEN, a planted 32x32 block RED, sub-threshold noise GREEN, thin z-fight RED)
--plant x,y,size: also count the flipping pixels inside that square (the planted block of -skygrindFlickerPlant72; default 160,540,32 = UnityTuning
L72PlantX / L72PlantY / L72PlantSize). --delete-raw removes a stack file after it is measured (a noisy pose is tens of MB).
"""
import gzip
import json
import os
import struct
import sys

import numpy as np

BAR_SHARE_PCT = 0.01   # RED at or over this share of pixels (see the header)
SWING = 6              # levels: a reversal needs the value to move back this far from its last extreme
MIN_FLIPS = 3          # reversals for a pixel to count as flickering
PLANT_DEFAULT = (160, 540, 32)  # UnityTuning.L72PlantX / L72PlantY / L72PlantSize

MAGIC = b"S72L"
VERSION = 1
HEADER = 20


def read_stack(path):
    """-> (width, height, luma array uint8 [frames, h, w])."""
    with open(path, "rb") as f:
        head = f.read(HEADER)
        body = f.read()
    if len(head) < HEADER or head[:4] != MAGIC:
        raise ValueError("not a still72 file: " + path)
    version, w, h, frames = struct.unpack("<iiii", head[4:20])
    if version != VERSION:
        raise ValueError("still72 version %d in %s" % (version, path))
    raw = gzip.decompress(body)
    n = w * h
    have = len(raw) // n
    if have < frames:
        frames = have
    arr = np.frombuffer(raw, dtype=np.uint8, count=frames * n).reshape(frames, h, w)
    # frame 0 is luma, later frames are differences mod 256: a cumulative sum in uint8 undoes them (it wraps mod 256)
    return w, h, np.cumsum(arr, axis=0, dtype=np.uint8)


def count_flips(y, swing=SWING):
    """Per pixel, the number of direction reversals each larger than `swing` levels. y: uint8 [frames, h, w]."""
    frames, h, w = y.shape
    ext = y[0].astype(np.int16)
    dirn = np.zeros((h, w), np.int8)
    flips = np.zeros((h, w), np.uint8)
    for k in range(1, frames):
        v = y[k].astype(np.int16)
        d = v - ext
        unk = dirn == 0
        pos = dirn == 1
        neg = dirn == -1
        a = unk & (d > swing)       # "by more than 6 levels": strictly over the swing
        b = unk & (d < -swing)
        c_ext = pos & (d > 0)
        c_rev = pos & (d < -swing)
        e_ext = neg & (d < 0)
        e_rev = neg & (d > swing)
        move = a | b | c_ext | e_ext | c_rev | e_rev
        ext = np.where(move, v, ext)
        dirn[a | e_rev] = 1
        dirn[b | c_rev] = -1
        flips += (c_rev | e_rev).astype(np.uint8)
    return flips


def step_percentiles(y, qs=(99.9, 99.99)):
    """The percentiles of |luma[k] - luma[k-1]| over every pixel and step, from a histogram (no 120 M-element sort); also the max and the mean."""
    frames = y.shape[0]
    hist = np.zeros(256, np.int64)
    for k in range(1, frames):
        d = np.abs(y[k].astype(np.int16) - y[k - 1].astype(np.int16)).astype(np.uint8)
        hist += np.bincount(d.ravel(), minlength=256)
    total = int(hist.sum())
    cum = np.cumsum(hist)
    out = {}
    for q in qs:
        out[q] = int(np.searchsorted(cum, q / 100.0 * total, side="left")) if total else 0
    nz = np.nonzero(hist)[0]
    mx = int(nz[-1]) if len(nz) else 0
    mean = float((hist * np.arange(256)).sum() / total) if total else 0.0
    return out, mx, mean


def blobs(mask, top=3):
    """The `top` biggest connected groups of flipping pixels (joined across a one-pixel gap): [(pixels, (x0, y0, x1, y1))]."""
    from scipy import ndimage
    if not mask.any():
        return []
    joined = ndimage.binary_dilation(mask, structure=np.ones((3, 3), bool))
    lab, n = ndimage.label(joined, structure=np.ones((3, 3), int))
    counts = ndimage.sum(mask, lab, index=np.arange(1, n + 1))
    boxes = ndimage.find_objects(lab)
    order = np.argsort(-counts)[:top]
    res = []
    for i in order:
        sy, sx = boxes[i]
        res.append((int(counts[i]), (int(sx.start), int(sy.start), int(sx.stop), int(sy.stop))))
    return res


def heatmap(y0, flips):
    """RGB uint8: frame 0 dimmed to a third, 1-2 reversal pixels blue, 3+ orange rising to white at 20."""
    base = (y0.astype(np.float32) * 0.35).astype(np.uint8)
    img = np.stack([base, base, base], axis=-1)
    one = (flips >= 1) & (flips < MIN_FLIPS)
    img[one] = (60, 100, 255)
    hot = flips >= MIN_FLIPS
    t = np.clip((flips.astype(np.float32) - MIN_FLIPS) / 17.0, 0, 1)[hot]
    img[hot] = np.stack([np.full_like(t, 255), 140 + 115 * t, 20 + 235 * t], axis=-1).astype(np.uint8)
    return img


def measure(y, plant=None):
    frames, h, w = y.shape
    flips = count_flips(y)
    mask = flips >= MIN_FLIPS
    n = int(mask.sum())
    share = 100.0 * n / (w * h)
    pct, mx, mean = step_percentiles(y)
    res = {
        "w": w, "h": h, "frames": int(frames),
        "flipPx": n, "flipSharePct": round(share, 5),
        "p999": pct[99.9], "p9999": pct[99.99], "maxStep": mx, "meanStep": round(mean, 4),
        "oneTwoPx": int(((flips >= 1) & (flips < MIN_FLIPS)).sum()),
        "blobs": blobs(mask),
        "verdict": "RED" if share >= BAR_SHARE_PCT else "GREEN",
    }
    if plant:
        px, py, ps = plant
        res["plantRect"] = [px, py, ps]
        res["plantRectFlipPx"] = int(mask[py:py + ps, px:px + ps].sum())
        res["plantRectPx"] = int(mask[py:py + ps, px:px + ps].size)
    return res, flips


def save_png(path, rgb):
    from PIL import Image
    Image.fromarray(rgb).save(path, optimize=False, compress_level=3)


def line(name, r):
    b = r["blobs"]
    bs = "; ".join("%d px at x %d-%d y %d-%d" % (c, bx[0], bx[2], bx[1], bx[3]) for c, bx in b) if b else "none"
    s = "POSE %s | frames %d | flip share %.4f %% (%d px) | p99.9 |dY| %d | p99.99 %d | max %d | mean %.3f | blobs: %s | %s" % (
        name, r["frames"], r["flipSharePct"], r["flipPx"], r["p999"], r["p9999"], r["maxStep"], r["meanStep"], bs, r["verdict"])
    if "plantRect" in r:
        s += " | plant rect %d of %d px flip" % (r["plantRectFlipPx"], r["plantRectPx"])
    return s


def files_of(args):
    out = []
    for a in args:
        if os.path.isdir(a):
            for root, _d, names in os.walk(a):
                for n in sorted(names):
                    if n.endswith(".s72"):
                        out.append(os.path.join(root, n))
        elif a.endswith(".s72"):
            out.append(a)
    return out


def level_of(name):
    # still72-T72-<levelid>-<room>-<pose>.s72
    parts = name.split("-")
    return parts[2] if len(parts) > 3 and parts[1] == "T72" else "?"


def run(files, out_dir, plant, delete_raw, json_path):
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
    rows = {}
    for f in files:
        base = os.path.basename(f)[:-4]
        try:
            w, h, y = read_stack(f)
        except Exception as e:  # a short or foreign file is reported, never skipped silently
            print("POSE %s | ERROR %s | RED" % (base, e))
            rows[base] = {"error": str(e), "verdict": "RED"}
            continue
        r, flips = measure(y, plant)
        r["file"] = f
        rows[base] = r
        print(line(base, r))
        if out_dir:
            save_png(os.path.join(out_dir, base + ".heat.png"), heatmap(y[0], flips))
        if delete_raw:
            os.remove(f)
    worst = {}
    for name, r in rows.items():
        if "flipSharePct" not in r:
            continue
        lv = level_of(name)
        if lv not in worst or r["flipSharePct"] > rows[worst[lv]]["flipSharePct"]:
            worst[lv] = name
    for lv in sorted(worst):
        mine = [rows[n]["flipSharePct"] for n in rows if "flipSharePct" in rows[n] and level_of(n) == lv]
        print("LEVEL %s | poses %d | worst %s %.4f %% | mean %.4f %%" % (lv, len(mine), worst[lv], rows[worst[lv]]["flipSharePct"], sum(mine) / len(mine)))
    red = sum(1 for r in rows.values() if r["verdict"] == "RED")
    print("SUMMARY n72-flicker | poses %d | RED %d | GREEN %d | bar %.3f %% of the frame" % (len(rows), red, len(rows) - red, BAR_SHARE_PCT))
    if json_path:
        with open(json_path, "w", encoding="utf-8") as fh:
            json.dump({"barSharePct": BAR_SHARE_PCT, "swing": SWING, "minFlips": MIN_FLIPS, "poses": rows, "worst": worst}, fh, indent=1, default=str)
    return rows


def synth(kind, frames=60, w=1920, h=1080, seed=7):
    """Synthetic stacks for the self-test: a textured still scene, then the case on top of it."""
    rng = np.random.default_rng(seed)
    gy, gx = np.mgrid[0:h, 0:w]
    base = (60 + 80 * np.sin(gx / 90.0) * np.cos(gy / 70.0) + 40 * (gx / w)).astype(np.float32)
    y = np.empty((frames, h, w), np.uint8)
    for k in range(frames):
        frame = base.copy()
        if kind == "noise3":      # grain well under the swing: +-3 levels
            frame += rng.integers(-3, 4, size=(h, w))
        if kind == "plant":       # the slate's planted case: a 32 x 32 block toggled every frame
            px, py, ps = PLANT_DEFAULT
            frame[py:py + ps, px:px + ps] = 220 if k % 2 == 0 else 20
        if kind == "zfight":      # a thin strip flipping every 2 frames by 24 levels, 300 x 4 px = 1200 px
            frame[800:804, 400:700] += 24 if (k // 2) % 2 == 0 else -24
        if kind == "slow":        # a slow fade of a big area (one swing in 60 frames): not a flicker
            frame[200:600, 200:800] += 30 * np.sin(k / 60.0 * np.pi)
        y[k] = np.clip(frame, 0, 255).astype(np.uint8)
    return y


def selftest():
    cases = [("calm", "GREEN"), ("noise3", "GREEN"), ("slow", "GREEN"), ("plant", "RED"), ("zfight", "RED")]
    bad = 0
    for kind, want in cases:
        y = synth(kind)
        r, _flips = measure(y, PLANT_DEFAULT)
        ok = r["verdict"] == want
        if kind == "plant":
            ok = ok and r["plantRectFlipPx"] == 1024 and r["flipPx"] == 1024
        if kind == "zfight":
            ok = ok and r["flipPx"] == 1200
        print("%s %s: want %s, got %s | flip px %d (%.4f %%) | plant rect %d/%d" % ("ok  " if ok else "FAIL", kind, want, r["verdict"], r["flipPx"], r["flipSharePct"], r["plantRectFlipPx"], r["plantRectPx"]))
        bad += 0 if ok else 1
    # the stack file round trip: write a stack as Still72.cs does (header, gzip of frame 0 then deltas), read it back
    y = synth("plant", frames=8, w=320, h=180)
    import tempfile
    d = tempfile.mkdtemp()
    p = os.path.join(d, "still72-T72-selftest-r1-p1.s72")
    with open(p, "wb") as fh:
        fh.write(MAGIC + struct.pack("<iiii", VERSION, 320, 180, 8))
        buf = [y[0].tobytes()] + [((y[k].astype(np.int16) - y[k - 1].astype(np.int16)) & 0xFF).astype(np.uint8).tobytes() for k in range(1, 8)]
        fh.write(gzip.compress(b"".join(buf)))
    w, h, back = read_stack(p)
    rt = (w, h) == (320, 180) and back.shape == (8, 180, 320) and bool((back == y).all())
    print("%s round trip of the stack file (8 frames, 320x180)" % ("ok  " if rt else "FAIL"))
    bad += 0 if rt else 1
    os.remove(p)
    os.rmdir(d)
    print("SELFTEST n72-flicker: %s" % ("PASS" if bad == 0 else "FAIL (%d)" % bad))
    return bad


def main(argv):
    if "--selftest" in argv:
        return 1 if selftest() else 0
    out_dir = None
    plant = PLANT_DEFAULT
    delete_raw = "--delete-raw" in argv
    json_path = None
    paths = []
    i = 0
    while i < len(argv):
        a = argv[i]
        if a == "--out":
            out_dir = argv[i + 1]
            i += 1
        elif a == "--json":
            json_path = argv[i + 1]
            i += 1
        elif a == "--plant":
            plant = tuple(int(v) for v in argv[i + 1].split(","))
            i += 1
        elif a == "--delete-raw":
            pass
        else:
            paths.append(a)
        i += 1
    files = files_of(paths)
    if not files:
        print("n72-flicker: no .s72 files in " + " ".join(paths))
        return 2
    rows = run(files, out_dir, plant, delete_raw, json_path)
    return 0 if rows else 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

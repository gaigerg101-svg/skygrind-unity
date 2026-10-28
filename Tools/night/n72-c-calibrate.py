# NIGHT72 builder C (slate C2 palette72): the calibration of the palette's stone multipliers from the BASELINE frames. The stone's vertex
# colours are linear albedo (Stone.shader: albedo = vertex colour x light), so a per-channel multiplier m on the vertex colour moves the lit,
# graded frame by about m ^ (1 / 2.2) in sRGB. For each level the script reads the baseline's tour frames, takes the median floor and wall
# colours (the meter's fixed masks; frames whose floor mask lies on a wall, floor / wall gap under 15, are left out) and, for the TARGET
# (hue, S, luma) of the palette's table (TARGETS below), prints the multipliers m = (target / current) ^ 2.2 per channel, clamped.
#   python -I Tools/night/n72-c-calibrate.py [--unorm8] <baseline root with tp tf ti tk ts folders>     e.g. Logs/feel/n71-forge72
#   python -I Tools/night/n72-c-calibrate.py --refine <baseline root> spire=<ON tour65 folder> foundry=<...>   the second round: corrected multipliers from the ON frames
# The printed table is pasted into UnityTuning.N72C.cs with its comment. Not a verdict: a hypothesis until a Player frame shows it (L-094).
import colorsys
import glob
import os
import sys

import numpy as np
from PIL import Image

sys.dont_write_bytecode = True  # no Tools/night/__pycache__ from loading the meter
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import importlib.util

spec = importlib.util.spec_from_file_location("n72v", os.path.join(os.path.dirname(os.path.abspath(__file__)), "n72-c-values.py"))
n72v = importlib.util.module_from_spec(spec)
spec.loader.exec_module(n72v)

LEVELS = {"spire": "tp", "foundry": "tf", "iceline": "ti", "kiln": "tk", "span": "ts"}
# target (hue deg, S, luma 0-255) of the floor and the wall per level: floors warm-neutral on every level (the walkable surface's one language),
# walls carry the level's own hue. The luma is the baseline's own (a target of None keeps the measured luma): palette72 moves hue and chroma, not
# brightness (lit72 and Bright71 own the brightness).
TARGETS = {
    "spire": {"floor": (42, 0.27, None), "wall": (135, 0.55, None)},
    "foundry": {"floor": (40, 0.27, None), "wall": (150, 0.42, None)},
    "iceline": {"floor": (38, 0.26, None), "wall": (225, 0.62, None)},
    "kiln": {"floor": (36, 0.25, None), "wall": (272, 0.40, None)},
    "span": {"floor": (40, 0.26, None), "wall": (245, 0.52, None)},
}
CLAMP = (0.25, 4.0)
# the floor's vertex colour as the stone holds it when palette72 reads it: config colors.pillarTop (0x877D92) through look43's drain (35 % toward
# the grey, x 0.92 in value: UnityTuning.Look43StoneDesat / Look43StoneValue); a ledge's top (foundry's tier x 1.45) has less headroom still
FLOOR_VERTEX = np.array([0.487, 0.464, 0.514])


def luma(rgb):
    return 0.2126 * rgb[0] + 0.7152 * rgb[1] + 0.0722 * rgb[2]


def target_rgb(h, s, y):
    """The sRGB colour (0-255) of hue h, saturation s whose Rec. 709 luma is y (value solved by bisection)."""
    lo, hi = 0.0, 1.0
    for _ in range(40):
        v = (lo + hi) / 2
        r, g, b = colorsys.hsv_to_rgb(h / 360.0, s, v)
        if luma((r * 255, g * 255, b * 255)) < y:
            lo = v
        else:
            hi = v
    r, g, b = colorsys.hsv_to_rgb(h / 360.0, s, (lo + hi) / 2)
    return np.array([r * 255, g * 255, b * 255])


def main(root):
    for lvl, side in LEVELS.items():
        files = sorted(glob.glob(os.path.join(root, side, "tour65", "shot-*.png")))
        floors, walls, used = [], [], 0
        for f in files:
            m, _ = n72v.measure(n72v.load(f))
            if m["floor_wall"] < 15:
                continue
            floors.append(m["floor_rgb"])
            walls.append(m["wall_rgb"])
            used += 1
        if not used:
            print(lvl, "no usable frame")
            continue
        cf, cw = np.median(np.array(floors), axis=0), np.median(np.array(walls), axis=0)
        print("%-8s frames %d/%d | current floor %s (luma %.0f) wall %s (luma %.0f)" % (lvl, used, len(files), cf.astype(int).tolist(), luma(cf), cw.astype(int).tolist(), luma(cw)))
        for part, cur in (("floor", cf), ("wall", cw)):
            h, s, y = TARGETS[lvl][part]
            y = luma(cur) if y is None else y
            # Mesh.SetColors(Color[]) stores Float32 colours (the evidence: no [decor54] "re-toned" line in the n72-h0 Player logs: decor54's probe compares
            # the float colour it wrote with the one it reads back; a UNorm8 mesh would never match), so there is no headroom limit by default. With
            # --unorm8 (the hedge) a floor vertex is about FLOOR_VERTEX after look43's drain and a channel's multiplier is at most 1 / FLOOR_VERTEX; a
            # target the headroom cannot reach is lowered in luma (hue and S kept) until it can
            cap = (1.0 / FLOOR_VERTEX) if (part == "floor" and UNORM8) else np.array([CLAMP[1]] * 3)
            while True:
                t = target_rgb(h, s, y)
                raw = (np.maximum(t, 1.0) / np.maximum(cur, 1.0)) ** 2.2
                if (raw <= cap + 1e-6).all() or y < 20:
                    break
                y -= 1.0
            mul = np.clip(raw, CLAMP[0], cap)
            pred = np.minimum(cur * mul ** (1 / 2.2), 255)
            note = "" if y >= (luma(cur) if TARGETS[lvl][part][2] is None else TARGETS[lvl][part][2]) - 0.5 else "  [luma lowered to %.0f: the multiplier's cap (%.1f)]" % (y, cap.max())
            print("    %-5s target hue %3d S %.2f luma %3.0f -> %s | multiplier %s | predicted on screen %s (luma %.0f)%s" % (part, h, s, y, t.astype(int).tolist(), ", ".join("%.2f" % x for x in mul), pred.astype(int).tolist(), luma(pred), note))


UNORM8 = "--unorm8" in sys.argv

TUNING = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Scripts", "Game", "UnityTuning.N72C.cs")
ORDER = ["foundry", "iceline", "kiln", "span", "spire"]  # UnityTuning.Palette72Levels


def table_of(name):
    """The 3 x 5 multipliers of UnityTuning.Palette72<name>Mul as printed in UnityTuning.N72C.cs."""
    import re
    src = open(TUNING, encoding="utf-8").read()
    body = re.search(r"Palette72%sMul\s*=\s*\{(.*?)\};" % name, src, re.S).group(1)
    body = re.sub(r"//[^\n]*", "", body)
    nums = [float(x) for x in re.findall(r"(\d+\.\d+)f", body)]
    return {lvl: np.array(nums[i * 3:i * 3 + 3]) for i, lvl in enumerate(ORDER)}


def refine(baseline_root, pairs):
    """A second build round: the floor and wall the ON frames really show vs the targets the baseline calibration aimed at; prints corrected multipliers
    (new = old x (target / achieved) ^ 2.2, clamped). `pairs`: level=<ON tour65 folder>. Not a verdict: the corrected table is the next hypothesis (L-094)."""
    floor_tab, wall_tab = table_of("Floor"), table_of("Wall")
    out = {"floor": dict(floor_tab), "wall": dict(wall_tab)}
    for pair in pairs:
        lvl, folder = pair.split("=", 1)
        base = sorted(glob.glob(os.path.join(baseline_root, LEVELS[lvl], "tour65", "shot-*.png")))
        on = sorted(glob.glob(os.path.join(folder, "shot-*.png"))) or sorted(glob.glob(os.path.join(folder, "tour65", "shot-*.png")))
        b_f, b_w, a_f, a_w = [], [], [], []
        for f in base:
            m, _ = n72v.measure(n72v.load(f))
            if m["floor_wall"] >= 15:
                b_f.append(m["floor_rgb"]); b_w.append(m["wall_rgb"])
        for f in on:
            m, _ = n72v.measure(n72v.load(f))
            if m["floor_wall"] >= 15:
                a_f.append(m["floor_rgb"]); a_w.append(m["wall_rgb"])
        if not b_f or not a_f:
            print(lvl, "no usable frames (baseline %d, on %d)" % (len(b_f), len(a_f)))
            continue
        for part, base_med, on_med in (("floor", np.median(b_f, axis=0), np.median(a_f, axis=0)), ("wall", np.median(b_w, axis=0), np.median(a_w, axis=0))):
            h, s, y = TARGETS[lvl][part]
            y = luma(base_med) if y is None else y
            t = target_rgb(h, s, y)
            old = (floor_tab if part == "floor" else wall_tab)[lvl]
            new = np.clip(old * (np.maximum(t, 1.0) / np.maximum(on_med, 1.0)) ** 2.2, CLAMP[0], CLAMP[1])
            out[part][lvl] = new
            print("%-8s %-5s baseline %s -> on %s (luma %.0f) | target %s (luma %.0f) | multiplier %s -> %s" % (
                lvl, part, base_med.astype(int).tolist(), on_med.astype(int).tolist(), luma(on_med), t.astype(int).tolist(), luma(t),
                ", ".join("%.2f" % x for x in old), ", ".join("%.2f" % x for x in new)))
    for part, name in (("floor", "Floor"), ("wall", "Wall")):
        print("// Palette72%sMul (corrected):" % name)
        for lvl in ORDER:
            print("    %sf, %sf, %sf, // %s" % (*["%.2f" % x for x in out[part][lvl]], lvl))
    return 0


if __name__ == "__main__":
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if "--refine" in sys.argv:
        sys.exit(refine(args[0], args[1:]))  # <baseline root> level=<ON folder> ...
    sys.exit(main(args[0] if args else "Logs/feel/n71-forge72") or 0)

# NIGHT63 builder V (leftover 7, C2 "the enemy glow up close"; gg-proxy-start.md #9: "enemies read as glow domes and flat beans"). Read only:
# writes nothing. Two meters and a pair count, each with planted cases (--selftest).
#   python -I Tools/night/n63-v-near.py --telemetry <telemetry.jsonl> [...]
#       FeelProbe telemetry: per file, the frames, the frames with an enemy AT THE EYE (an enemy's screen rect at least the frame's height or
#       width: the eye within about 0.7 m of a 1.8 m body at 74 deg, or inside it), the longest such run, the first such frames; and the
#       frames with an enemy NEAR (rect height >= half the frame: within about 2.4 m). The rect is the sim body's projection (the probe's), so
#       this meter is the red only: it reads the same with any view change.
#   python -I Tools/night/n63-v-near.py --pixels <image> [...]
#       per image, the share of the frame in the enemy body's hue band (colors.enemy #ff4a1c's hue +- 30 deg, S >= 0.65, V >= 0.12): a frame
#       an enemy's body fills reads near 1.
#   python -I Tools/night/n63-v-near.py --pair <on image> <off image>
#       the enemy-hue share on and off, and the changed pixels (a channel moving more than 24 of 255: n62-gun-yellow.py's rule; the visible
#       rule's count, >= 41,472 at 1920 x 1080).
#   python -I Tools/night/n63-v-near.py --selftest      the planted cases (PASS / FAIL each; exit 1 on a FAIL)
import json
import sys

import numpy as np
from PIL import Image

ENEMY_HEX = 0xFF4A1C  # PortExport/config.json colors.enemy (read at NIGHT63: ff4a1c)
HUE_TOL = 30.0
SAT_MIN = 0.65  # measured on n62-freeze vf/verbs37: the foundry floor S median 0.38, the enemy bodies p10 0.72 (f0100), the dome p10 0.93 (f0400)
VAL_MIN = 0.12
FRAME_W, FRAME_H = 1920.0, 1080.0
DIFF = 24


def hue_of(hex_rgb):
    r, g, b = ((hex_rgb >> 16) & 255) / 255.0, ((hex_rgb >> 8) & 255) / 255.0, (hex_rgb & 255) / 255.0
    mx, mn = max(r, g, b), min(r, g, b)
    if mx == mn:
        return 0.0
    if mx == r:
        h = ((g - b) / (mx - mn)) % 6
    elif mx == g:
        h = (b - r) / (mx - mn) + 2
    else:
        h = (r - g) / (mx - mn) + 4
    return h * 60.0


ENEMY_HUE = hue_of(ENEMY_HEX)


def enemy_mask(rgb):
    a = rgb.astype(np.float32) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = a.max(axis=-1)
    mn = a.min(axis=-1)
    d = mx - mn
    s = np.where(mx > 0, d / np.where(mx > 0, mx, 1), 0)
    safe = np.where(d > 1e-9, d, 1)
    h = np.where(mx == r, ((g - b) / safe) % 6, np.where(mx == g, (b - r) / safe + 2, (r - g) / safe + 4)) * 60.0
    h = np.where(d > 1e-9, h, 0)
    dh = np.abs(((h - ENEMY_HUE) + 180.0) % 360.0 - 180.0)
    return (dh <= HUE_TOL) & (s >= SAT_MIN) & (mx >= VAL_MIN) & (d > 1e-9)


def share(rgb):
    m = enemy_mask(rgb)
    return float(m.sum()) / float(m.size)


def load(path):
    return np.asarray(Image.open(path).convert("RGB"))


def changed(a, b):
    if a.shape != b.shape:
        raise ValueError("the two images differ in size: %s vs %s" % (a.shape, b.shape))
    return int((np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF).sum())


def telemetry_lines(lines):
    """frames, at-eye frames (list of frame indices), near frames, the longest at-eye run"""
    frames = 0
    at_eye, near = [], 0
    run = best = 0
    for line in lines:
        try:
            o = json.loads(line)
        except ValueError:
            continue
        if o.get("type") != "frame":
            continue
        idx = frames
        frames += 1
        hit_eye = hit_near = False
        for e in o.get("enemies") or []:
            rect = e.get("rect") or [0, 0, 0, 0]
            w, h = abs(rect[2]), abs(rect[3])
            if h >= FRAME_H or w >= FRAME_W:
                hit_eye = True
            if h >= FRAME_H / 2:
                hit_near = True
        if hit_eye:
            at_eye.append(idx)
            run += 1
            best = max(best, run)
        else:
            run = 0
        if hit_near:
            near += 1
    return frames, at_eye, near, best


def selftest():
    fails = 0

    def check(name, ok, got):
        nonlocal fails
        if not ok:
            fails += 1
        print(("PASS " if ok else "FAIL ") + name + ": " + str(got))

    lines = [
        json.dumps({"type": "frame", "enemies": [{"rect": [600, 300, 700, 1600]}]}),   # taller than the frame: at the eye
        json.dumps({"type": "frame", "enemies": [{"rect": [-650, -500, 3200, 7300]}]}),  # the eye inside: at the eye
        json.dumps({"type": "frame", "enemies": [{"rect": [800, 400, 300, 600]}]}),   # near (half the frame), not at the eye
        json.dumps({"type": "frame", "enemies": [{"rect": [900, 500, 60, 140]}]}),    # far
        json.dumps({"type": "frame", "enemies": []}),
        json.dumps({"type": "event", "name": "x"}),                                   # not a frame
    ]
    frames, at_eye, near, best = telemetry_lines(lines)
    check("telemetry frames", frames == 5, frames)
    check("telemetry at the eye (frames 0 and 1)", at_eye == [0, 1], at_eye)
    check("telemetry near (frames 0, 1, 2)", near == 3, near)
    check("telemetry longest run", best == 2, best)
    full = np.zeros((40, 80, 3), np.uint8)
    full[...] = (255, 74, 28)  # colors.enemy
    check("pixels: a frame of colors.enemy", abs(share(full) - 1.0) < 1e-9, share(full))
    half = np.zeros((40, 80, 3), np.uint8)
    half[:, :40] = (200, 40, 10)  # a darker red-orange body
    check("pixels: half the frame a red-orange body", abs(share(half) - 0.5) < 1e-9, share(half))
    cyan = np.zeros((40, 80, 3), np.uint8)
    cyan[...] = (25, 211, 230)  # colors.usable
    check("pixels: cyan is not an enemy", share(cyan) == 0.0, share(cyan))
    grey = np.full((40, 80, 3), 120, np.uint8)
    check("pixels: grey is not an enemy", share(grey) == 0.0, share(grey))
    a = np.zeros((1080, 1920, 3), np.uint8)
    b = a.copy()
    check("pair: identical frames change 0 px", changed(a, b) == 0, changed(a, b))
    b[100:110, 200:210] = (255, 255, 255)
    check("pair: a planted 10 x 10 change is exactly 100 px", changed(a, b) == 100, changed(a, b))
    c = a.copy()
    c[100:110, 200:210] = (20, 20, 20)
    check("pair: a change of 20 of 255 does not count (n62-gun-yellow.py's 24)", changed(a, c) == 0, changed(a, c))
    total = 11
    print("selftest: %d / %d PASS" % (total - fails, total))
    return 1 if fails else 0


def main(argv):
    if "--selftest" in argv:
        return selftest()
    if len(argv) >= 2 and argv[0] == "--telemetry":
        for path in argv[1:]:
            with open(path, "r", encoding="utf-8") as f:
                frames, at_eye, near, best = telemetry_lines(f)
            print("%s: %d frames; an enemy AT THE EYE in %d (longest run %d, first %s); NEAR (half the frame) in %d"
                  % (path, frames, len(at_eye), best, at_eye[:12], near))
        return 0
    if len(argv) >= 2 and argv[0] == "--pixels":
        for path in argv[1:]:
            print("%s: enemy-hue share %.3f" % (path, share(load(path))))
        return 0
    if len(argv) == 3 and argv[0] == "--pair":
        on, off = load(argv[1]), load(argv[2])
        print("%s vs %s: enemy-hue share on %.3f, off %.3f; changed %d px (%.2f %% of the frame)"
              % (argv[1], argv[2], share(on), share(off), changed(on, off), 100.0 * changed(on, off) / (on.shape[0] * on.shape[1])))
        return 0
    print(__doc__ or "usage: --telemetry <jsonl> ... | --pixels <image> ... | --pair <on> <off> | --selftest")
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

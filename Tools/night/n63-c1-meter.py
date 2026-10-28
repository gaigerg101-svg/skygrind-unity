# NIGHT63 builder D (slate C1, THE COLOUR TABLE): the frame meter. The OFF-PALETTE share of a frame: of the frame's pixels outside the HUD's
# corners, the share that is VIVID (HSV on the sRGB frame: S >= SMIN and V >= VMIN, the tier effects, glows, lines and accents live in; the
# world under colour56's cap of 0.65-0.70 reaches it only through its accents and lights) and whose hue is in NO role band:
#   danger red-orange 345-35 (colors.enemy #FF4A1C is 11), parry yellow 45-70 (DECIDE63 D-1), player cyan 160-205 (colors.usable #19D3E6 is 186;
#   its bloom fringe runs to 160), rival magenta 285-335. (Boundary pale violet-white is S < 0.40: never vivid.)
# So the off-palette hues are amber 35-45, lime / green / teal 70-160, blue / violet 205-285 and rose 335-345. The HUD's corners are left out
# (GG 2026-10-20: "the UI looks good as it is, so don't touch that"): the bottom-left vitals, the bottom-right weapon box, the top-left line and
# the top-right score. Per image one line; per folder (a level) the mean share, its role shares and the off-palette hue groups.
#   python -I Tools/night/n63-c1-meter.py <folder or image> [...]               e.g. Logs/feel/n62-freeze/vs/verbs37
#   python -I Tools/night/n63-c1-meter.py --levels <reel tag folder>             vf vi vk vs vp sides' verbs37 folders: one line per level
#   python -I Tools/night/n63-c1-meter.py --pair <on folder|image> <off folder|image>   per level / frame: off-palette share on vs off, the
#                                                                                drop (C1's MAIN: >= 50 % per level) and the changed px
#                                                                                (n62-gun-yellow.py's rule: a channel differs by > 24)
#   python -I Tools/night/n63-c1-meter.py planted                                synthetic frames: each role band, each off hue, the floors, the HUD mask
import glob
import os
import sys

import numpy as np
from PIL import Image

SMIN, VMIN = 0.55, 0.50
BANDS = [("danger", 345.0, 35.0), ("parry", 45.0, 70.0), ("player", 160.0, 205.0), ("rival", 285.0, 335.0)]
OFF_GROUPS = [("amber", 35.0, 45.0), ("lime", 70.0, 100.0), ("green", 100.0, 160.0), ("blue", 205.0, 250.0), ("violet", 250.0, 285.0), ("rose", 335.0, 345.0)]
HUD = [(0.0, 0.84, 0.26, 1.0), (0.80, 0.82, 1.0, 1.0), (0.0, 0.0, 0.26, 0.13), (0.90, 0.0, 1.0, 0.10)]  # x0, y0, x1, y1 from the top-left
DIFF = 24


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


def mask_hud(shape):
    H, W = shape[:2]
    m = np.ones((H, W), bool)
    for x0, y0, x1, y1 in HUD:
        m[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)] = False
    return m


def measure(rgb):
    h, s, v = hsv(rgb)
    keep = mask_hud(rgb.shape)
    vivid = keep & (s >= SMIN) & (v >= VMIN)
    n = int(keep.sum())
    roles = {}
    inrole = np.zeros_like(vivid)
    for name, lo, hi in BANDS:
        m = vivid & in_arc(h, lo, hi)
        roles[name] = int(m.sum())
        inrole |= m
    off = vivid & ~inrole
    groups = {name: int((off & in_arc(h, lo, hi)).sum()) for name, lo, hi in OFF_GROUPS}
    return {"n": n, "vivid": int(vivid.sum()), "off": int(off.sum()), "roles": roles, "groups": groups, "offMask": off}


def frames(p):
    if os.path.isdir(p):
        return sorted(glob.glob(os.path.join(p, "*.png")) + glob.glob(os.path.join(p, "*.jpg")))
    return [p]


def load(f):
    return np.asarray(Image.open(f).convert("RGB"))


def pct(x, n):
    return 100.0 * x / max(n, 1)


def folder(p, quiet=False):
    tot = {"n": 0, "vivid": 0, "off": 0, "roles": {}, "groups": {}}
    worst = (None, -1.0)
    files = frames(p)
    for f in files:
        r = measure(load(f))
        share = pct(r["off"], r["n"])
        if share > worst[1]:
            worst = (f, share)
        if not quiet:
            print("%s | off-palette %.3f %% (%d px) | vivid %.2f %% | %s" % (os.path.basename(f), share, r["off"], pct(r["vivid"], r["n"]),
                  " ".join("%s %d" % (k, v) for k, v in r["groups"].items() if v)))
        tot["n"] += r["n"]
        tot["vivid"] += r["vivid"]
        tot["off"] += r["off"]
        for k, v in r["roles"].items():
            tot["roles"][k] = tot["roles"].get(k, 0) + v
        for k, v in r["groups"].items():
            tot["groups"][k] = tot["groups"].get(k, 0) + v
    return tot, len(files), worst


def level_line(name, tot, count, worst):
    return ("%s: %d frames | OFF-PALETTE %.3f %% of the frame (vivid %.2f %%) | roles %s | off hues %s | worst %s %.3f %%" % (
        name, count, pct(tot["off"], tot["n"]), pct(tot["vivid"], tot["n"]),
        " ".join("%s %.2f%%" % (k, pct(v, tot["n"])) for k, v in tot["roles"].items()),
        " ".join("%s %.3f%%" % (k, pct(v, tot["n"])) for k, v in sorted(tot["groups"].items(), key=lambda kv: -kv[1]) if v),
        os.path.basename(worst[0]) if worst[0] else "-", worst[1]))


def pair(on, off):
    fa, fb = frames(on), frames(off)
    names = {os.path.basename(f): f for f in fb}
    to = tf = n = 0
    best = (None, -1)
    for f in fa:
        g = names.get(os.path.basename(f))
        if g is None:
            print("%s | no off frame" % os.path.basename(f))
            continue
        a, b = load(f), load(g)
        ra, rb = measure(a), measure(b)
        changed = int((np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF).sum())
        to += ra["off"]
        tf += rb["off"]
        n += ra["n"]
        if changed > best[1]:
            best = (os.path.basename(f), changed)
        print("%s | off-palette on %.3f %% off %.3f %% | changed px %d" % (os.path.basename(f), pct(ra["off"], ra["n"]), pct(rb["off"], rb["n"]), changed))
    drop = 100.0 * (1 - to / tf) if tf > 0 else 0.0
    print("PAIR %s: off-palette on %.3f %% vs off %.3f %%: drop %.1f %% (MAIN >= 50 %%: %s) | the most changed frame %s (%d px; rule 8 wants >= 41472)" % (
        os.path.basename(os.path.normpath(on)), pct(to, n), pct(tf, n), drop, "PASS" if drop >= 50 else "FAIL", best[0], best[1]))


def planted():
    bad = 0
    H, W = 540, 960
    base = np.full((H, W, 3), (40, 38, 52), np.uint8)  # a dark muted room (S 0.27, V 0.20: not vivid)

    def case(name, img, want_off, want_role=None):
        nonlocal bad
        r = measure(img)
        ok = (r["off"] > 0) == want_off and (want_role is None or r["roles"].get(want_role, 0) > 0)
        bad += 0 if ok else 1
        print(("PASS " if ok else "FAIL ") + name + ": off %d px, roles %s" % (r["off"], r["roles"]))

    def stripe(rgb, y=300, x0=300, x1=700):
        img = base.copy()
        img[y:y + 10, x0:x1] = rgb
        return img

    case("the muted room alone counts nothing", base, False)
    case("a lime accent stripe #8DD957 is off-palette", stripe((141, 217, 87)), True)
    case("the green fixture light #3CFF9A is off-palette", stripe((60, 255, 154)), True)
    case("iceline's accent #86D9A2 as authored (S 0.38) is under the vivid floor", stripe((134, 217, 162)), False)
    case("a blue light #5A82E0 is off-palette", stripe((90, 130, 224)), True)
    case("the usable cyan #19D3E6 is the player's", stripe((25, 211, 230)), False, "player")
    case("the enemy red-orange #FF4A1C is danger's", stripe((255, 74, 28)), False, "danger")
    case("the parry yellow #FFD000 is parry's", stripe((255, 208, 0)), False, "parry")
    case("RIVAL magenta #FF2BD6 is rival's", stripe((255, 43, 214)), False, "rival")
    case("a lime stripe muted to S 0.35 (C1's world tier) is not vivid", stripe((176, 217, 141)), False)
    case("a dark lime (V 0.40) is not vivid", stripe((70, 102, 46)), False)
    case("pale violet-white #C9BCFF (the boundary) is not vivid", stripe((201, 188, 255)), False)
    img = base.copy()
    img[H - 30:H - 20, 20:200] = (141, 217, 87)  # inside the bottom-left HUD corner
    case("a lime mark inside the HUD's corner is left out", img, False)
    a = stripe((141, 217, 87))
    b = stripe((176, 217, 141))
    ra, rb = measure(a), measure(b)
    changed = int((np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF).sum())
    ok = ra["off"] == 4000 and rb["off"] == 0 and changed == 4000
    bad += 0 if ok else 1
    print(("PASS " if ok else "FAIL ") + "pair: the 10 x 400 lime stripe muted: off-palette %d -> %d px, changed px %d (want 4000 -> 0, 4000)" % (ra["off"], rb["off"], changed))
    print("C1 METER PLANTED %s" % ("GREEN" if bad == 0 else "RED (%d wrong)" % bad))
    return 1 if bad else 0


def main(argv):
    if argv[:1] == ["planted"]:
        return planted()
    if argv[:1] == ["--pair"] and len(argv) == 3:
        pair(argv[1], argv[2])
        return 0
    if argv[:1] == ["--levels"] and len(argv) == 2:
        for side in ["vf", "vi", "vk", "vs", "vp"]:
            p = os.path.join(argv[1], side, "verbs37")
            if not os.path.isdir(p):
                print("%s: no %s" % (side, p))
                continue
            tot, count, worst = folder(p, quiet=True)
            print(level_line(side, tot, count, worst))
        return 0
    if not argv:
        print("usage: n63-c1-meter.py <folder|image> ... | --levels <tag folder> | --pair <on> <off> | planted")
        return 2
    for p in argv:
        tot, count, worst = folder(p)
        print(level_line(p, tot, count, worst))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

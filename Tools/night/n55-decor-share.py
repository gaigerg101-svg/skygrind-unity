# NIGHT55 builder C (decor55, slate P0-decor / OPEN O-07, O-08): the level accent's share of each a452 entry frame, read from the PNGs.
# Run with python -I (Pillow + numpy):
#   python -I Tools/night/n55-decor-share.py <reel dir> [<reel dir> ...] [--config Design/unity-config.json] [--frames entry]
# Each <reel dir> holds an a452 run's shots (shot-N29-K52-<level>-<room>-<frame>.png, searched recursively). Per frame: the level key (the id
# after its "nNN-" prefix), then for the accent of decor55 (numbers.levels.<key>.accentHex when the row has it, else decor54's) and for
# decor54's own accent: the share of the frame's pixels in the accent's class (hue within HUE_TOL deg of the accent's hue, saturation at least
# max(S_FLOOR, S_SHARE x the accent's), value at least V_MIN), the usable cyan class's share (hue within 20 deg of the usable token's 186,
# saturation >= 0.35, value >= 0.35), each class's 95th-percentile luminance (usable cyan must stay the brightest), and the accent hue's
# least gap to the role tokens (parry yellow 44, danger 12, usable cyan 186, RIVAL magenta 312). Prints one line per frame and a level summary.
import sys, os, re, json, glob, colorsys
import numpy as np
from PIL import Image

HUE_TOL = 12.0
S_FLOOR, S_SHARE, V_MIN = 0.30, 0.75, 0.20  # S_SHARE 0.75 (was 0.6 in the first read): the foundry's olive-grey ramps (hue ~70, s 0.3-0.45 dithered) stay out
TOKENS = {"parry": 0xFFC93A, "danger": 0xFF4A1C, "cyan": 0x19D3E6, "magenta": 0xFF2BD6}


def hex_hsv(h):
    r, g, b = ((h >> 16) & 255) / 255.0, ((h >> 8) & 255) / 255.0, (h & 255) / 255.0
    hh, s, v = colorsys.rgb_to_hsv(r, g, b)
    return hh * 360.0, s, v


def gap(a, b):
    d = abs(a - b) % 360.0
    return 360.0 - d if d > 180.0 else d


def hsv_arrays(img):
    a = np.asarray(img.convert("RGB"), dtype=np.float32) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = np.max(a, axis=2)
    mn = np.min(a, axis=2)
    d = mx - mn
    s = np.where(mx > 1e-6, d / np.maximum(mx, 1e-6), 0)
    h = np.zeros_like(mx)
    m = d > 1e-6
    rc = np.where(m, (mx - r) / np.maximum(d, 1e-6), 0)
    gc = np.where(m, (mx - g) / np.maximum(d, 1e-6), 0)
    bc = np.where(m, (mx - b) / np.maximum(d, 1e-6), 0)
    h = np.where(r == mx, bc - gc, np.where(g == mx, 2.0 + rc - bc, 4.0 + gc - rc))
    h = (h / 6.0) % 1.0 * 360.0
    lum = 0.2126 * r + 0.7152 * g + 0.0722 * b
    return h, s, mx, lum


def cls(h, s, v, hue, sat_min, v_min, tol):
    dh = np.abs(h - hue) % 360.0
    dh = np.minimum(dh, 360.0 - dh)
    return (dh <= tol) & (s >= sat_min) & (v >= v_min)


def main():
    args = sys.argv[1:]
    cfg_path = "Design/unity-config.json"
    which = "entry"
    if "--config" in args:
        i = args.index("--config"); cfg_path = args[i + 1]; del args[i:i + 2]
    if "--frames" in args:
        i = args.index("--frames"); which = args[i + 1]; del args[i:i + 2]
    cfg = json.load(open(cfg_path, encoding="utf-8"))
    d54 = cfg["design"]["n54"]["decor54"]["numbers"]["levels"]
    n55 = cfg["design"].get("n55", {}).get("decor55", {}).get("numbers", {})
    d55 = n55.get("levels", {})
    cy_h, _, _ = hex_hsv(TOKENS["cyan"])
    summary = {}
    for root in args:
        for p in sorted(glob.glob(os.path.join(root, "**", "shot-N29-K52-*-" + which + ".png"), recursive=True)):
            m = re.match(r"shot-N29-K52-(n\d+-[a-z]+)-([a-z0-9]+)-" + which + r"\.png", os.path.basename(p))
            if not m:
                continue
            level, room = m.group(1), m.group(2)
            key = level.split("-", 1)[1]
            if key not in d54:
                continue
            img = Image.open(p)
            h, s, v, lum = hsv_arrays(img)
            n = h.size
            out = []
            accents = []
            if key in d55 and "accentHex" in d55[key]:
                accents.append(("decor55", int(d55[key]["accentHex"])))
            accents.append(("decor54", int(d54[key]["accentHex"])))
            cyan = cls(h, s, v, cy_h, 0.35, 0.35, 20.0)
            cy_share = cyan.sum() / n
            cy_p95 = float(np.percentile(lum[cyan], 95)) if cyan.any() else 0.0
            for name, ah in accents:
                hh, ss, vv = hex_hsv(ah)
                a = cls(h, s, v, hh, max(S_FLOOR, S_SHARE * ss), V_MIN, HUE_TOL)
                share = a.sum() / n
                p95 = float(np.percentile(lum[a], 95)) if a.any() else 0.0
                gaps = {k: gap(hh, hex_hsv(t)[0]) for k, t in TOKENS.items()}
                least = min(gaps, key=gaps.get)
                out.append(f"{name} #{ah:06x} (hue {hh:.0f}, s {ss:.2f}; least role gap {gaps[least]:.0f} deg to {least}; parry {gaps['parry']:.0f}): share {100 * share:.2f} %, p95 lum {p95:.3f}")
                summary.setdefault((root, key, name), []).append(share)
            print(f"[n55decor] {root} | {level} {room} {which} | {' | '.join(out)} | usable cyan share {100 * cy_share:.2f} %, p95 lum {cy_p95:.3f}")
    for (root, key, name), v in sorted(summary.items()):
        print(f"[n55decor] SUMMARY {root} | {key} {name}: {len(v)} frames, share min {100 * min(v):.2f} %, mean {100 * sum(v) / len(v):.2f} %")


if __name__ == "__main__":
    main()

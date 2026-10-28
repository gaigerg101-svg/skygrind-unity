# NIGHT57 builder V (slate V1-colour56's GUARDS; NIGHT57.md section 5 V1; GAPS.md 4 "greyscale and colour-blind checks on every look row"):
# two frame checks, each with planted cases.
#   python -I Tools/night/n57-v-cvd.py grey <frame dir> [...] [--kinds entry,mid]
#       E-3 (DESIGN-RULES: "Check with a greyscale screenshot: enemies and hazards in the top values"; research/design.md 5, Angstadt's grayscale
#       test). Per frame (a356 / a357 shots with their sidecars; regions from n56-colour.py's own reader): the Rec. 709 luminance of the sRGB
#       frame; ENEMIES = the warm body pixels of each enemy rect (hue 330-70, S >= 0.25, V >= 0.12: n56-colour.py's body mask); HAZARDS = the
#       danger-family pixels outside the rects, HUD and gun (hue 350-35, S >= 0.5, V >= 0.35: lava, floor tells, spits); WORLD = the rest
#       (n56-colour.py's world: the 150 px screen edge out). A frame PASSES when the enemies' and hazards' pixels together have a median
#       luminance >= the world's 75th percentile (the top quarter of the values). The meter's threshold, not a source's: colors.md method 5 asks
#       each enemy's median outside the world's middle 50 %; E-3 asks for the top. Each enemy's own median is printed beside it.
#   python -I Tools/night/n57-v-cvd.py cvd <frame.png | dir> [...]
#       Deuteranopia (and protanopia, printed beside): the frame through Machado, Oliveira and Fernandes (2009) "A physiologically-based model for
#       simulation of color vision deficiency" (IEEE TVCG 15(6)), severity 1.0, applied to linear RGB. The 3x3 matrices are the paper's published
#       table (no code ported; DaltonLens-Python, MIT, github.com/DaltonLens/DaltonLens-Python, was read for the method and uses the same table).
#       USABLE CYAN = pixels at hue 170-200, S >= 0.5, V >= 0.4 on the original frame; RED-ORANGE = hue 350-35 at the same S / V. Each class's
#       median colour after the simulation, in CIELAB (D65); PASS when the two medians stay >= 20 Delta-E (CIE76) apart (the meter's threshold:
#       ~2x the ~10 often read as "clearly different at a glance"), a class with < 200 px is "not in frame" (n/a).
#   python -I Tools/night/n57-v-cvd.py planted
import sys, os, json, math, importlib.util
sys.dont_write_bytecode = True  # no __pycache__ beside the tools
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(M)

# Machado et al. 2009, Table "severity 1.0" (linear RGB)
DEUTAN = np.array([[0.367322, 0.860646, -0.227968],
                   [0.280085, 0.672501, 0.047413],
                   [-0.011820, 0.042940, 0.968881]])
PROTAN = np.array([[0.152286, 1.052583, -0.204868],
                   [0.114503, 0.786281, 0.099216],
                   [-0.003882, -0.048116, 1.051998]])
DE_MIN = 20.0
GREY_Q = 75.0
MIN_PX = 200


def to_lin(a):
    return np.where(a <= 0.04045, a / 12.92, ((a + 0.055) / 1.055) ** 2.4)


def to_srgb(a):
    a = np.clip(a, 0.0, 1.0)
    return np.where(a <= 0.0031308, a * 12.92, 1.055 * a ** (1 / 2.4) - 0.055)


def simulate(rgb_u8, mat):
    lin = to_lin(rgb_u8.astype(np.float64) / 255.0)
    return np.clip(lin @ mat.T, 0.0, 1.0)  # linear


def lab(lin):
    xyz = lin @ np.array([[0.4124564, 0.3575761, 0.1804375], [0.2126729, 0.7151522, 0.0721750], [0.0193339, 0.1191920, 0.9503041]]).T
    xyz = xyz / np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > (6 / 29) ** 3, np.cbrt(xyz), xyz / (3 * (6 / 29) ** 2) + 4 / 29)
    return np.stack([116 * f[..., 1] - 16, 500 * (f[..., 0] - f[..., 1]), 200 * (f[..., 1] - f[..., 2])], axis=-1)


def cvd_frame(rgb):
    h, s, v, _ = M.hsv(rgb)
    cyan = M.band(h, 170.0, 200.0) & (s >= 0.5) & (v >= 0.4)
    red = M.band(h, 350.0, 35.0) & (s >= 0.5) & (v >= 0.4)
    out = {"cyanPx": int(cyan.sum()), "redPx": int(red.sum())}
    if cyan.sum() < MIN_PX or red.sum() < MIN_PX:
        out["ok"] = None
        return out
    for name, mat in (("deutan", DEUTAN), ("protan", PROTAN), ("normal", np.eye(3))):
        L = lab(simulate(rgb, mat))
        a, b = np.median(L[cyan], axis=0), np.median(L[red], axis=0)
        out[name] = float(np.linalg.norm(a - b))
        out[name + "Cyan"] = a.tolist()
        out[name + "Red"] = b.tolist()
    out["ok"] = bool(out["deutan"] >= DE_MIN)
    out["okProtan"] = bool(out["protan"] >= DE_MIN)
    return out


def grey_frame(rgb, meta):
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, lum = M.hsv(rgb)
    hud = np.zeros((H, W), bool)
    for r in meta.get("hud", []):
        if r[2] * r[3] <= 0.25 * W * H:
            M.rect_mask(hud, r)
    en = np.zeros((H, W), bool)
    body = np.zeros((H, W), bool)
    per = []
    for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
        m = np.zeros((H, W), bool)
        x0, y0, x1, y1 = M.rect_mask(m, r)
        en |= m
        if x1 - x0 < 12 or y1 - y0 < 12:
            continue
        b = m & M.band(h, *M.WARM) & (s >= 0.25) & (v >= 0.12)
        if b.sum() >= 0.15 * m.sum():
            body |= b
            per.append(float(np.median(lum[b])))
    gun = np.zeros((H, W), bool)
    if meta.get("gun"):
        M.rect_mask(gun, meta["gun"])
    eb = int(round(M.EDGE_PX * H / 1080.0))
    edge = np.zeros((H, W), bool)
    if eb > 0:
        edge[:eb, :] = True; edge[-eb:, :] = True; edge[:, :eb] = True; edge[:, -eb:] = True
    free = ~hud & ~en & ~gun & ~edge
    haz = free & M.band(h, 350.0, 35.0) & (s >= 0.5) & (v >= 0.35)
    world = free & ~haz
    out = {"enemyPx": int(body.sum()), "hazardPx": int(haz.sum()), "enemies": per}
    if world.sum() < 100 or (body | haz).sum() < MIN_PX:
        out["ok"] = None
        return out
    out["worldQ"] = float(np.percentile(lum[world], GREY_Q))
    out["worldP90"] = float(np.percentile(lum[world], 90))
    out["tellsMed"] = float(np.median(lum[body | haz]))
    out["enemyMed"] = float(np.median(lum[body])) if body.sum() >= MIN_PX else None
    out["hazardMed"] = float(np.median(lum[haz])) if haz.sum() >= MIN_PX else None
    out["ok"] = bool(out["tellsMed"] >= out["worldQ"])
    return out


def fmt(x):
    return "-" if x is None else "%.3f" % x


def run_grey(roots, kinds):
    n = ok = na = 0
    for d in M.scenario_dirs(roots):
        items, head = M.frame_list(d, kinds, False)
        for name, path, lv, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
            if kind == "jpg":
                continue
            rgb = np.asarray(Image.open(path).convert("RGB"))
            o = grey_frame(rgb, M.meta_of(fr, rgb.shape[0], extra))
            if o["ok"] is None:
                na += 1
                print("  %s: n/a (enemy body %d px, hazards %d px)" % (name, o["enemyPx"], o["hazardPx"]))
                continue
            n += 1
            ok += 1 if o["ok"] else 0
            print("  %s: enemies+hazards median luma %.3f vs world p75 %.3f (p90 %.3f) -> %s | enemies %s (each: %s), hazards %s (%d px)"
                  % (name, o["tellsMed"], o["worldQ"], o["worldP90"], "PASS" if o["ok"] else "FAIL", fmt(o["enemyMed"]),
                     ", ".join("%.2f" % x for x in o["enemies"]) or "-", fmt(o["hazardMed"]), o["hazardPx"]))
    print("GREY TOTAL: %d / %d frames with the enemies and hazards in the top values (median >= world p%.0f); %d n/a" % (ok, n, GREY_Q, na))
    return ok, n


def run_cvd(paths):
    files = []
    for p in paths:
        if os.path.isdir(p):
            for dp, dn, fn in os.walk(p):
                files += [os.path.join(dp, f) for f in fn if f.startswith("shot-") and f.endswith(("-entry.png", "-mid.png"))]
        else:
            files.append(p)
    n = ok = 0
    for f in sorted(files):
        o = cvd_frame(np.asarray(Image.open(f).convert("RGB")))
        if o["ok"] is None:
            print("  %s: n/a (cyan %d px, red-orange %d px: < %d)" % (os.path.basename(f), o["cyanPx"], o["redPx"], MIN_PX))
            continue
        n += 1
        ok += 1 if o["ok"] else 0
        print("  %s: cyan %d px vs red-orange %d px: Delta-E normal %.1f, DEUTERANOPIA %.1f -> %s, protanopia %.1f -> %s (>= %.0f)"
              % (os.path.basename(f), o["cyanPx"], o["redPx"], o["normal"], o["deutan"], "PASS" if o["ok"] else "FAIL", o["protan"], "PASS" if o["okProtan"] else "FAIL", DE_MIN))
    print("CVD TOTAL: %d / %d frames keep the usable cyan and the red-orange apart under deuteranopia" % (ok, n))
    return ok, n


def planted():
    bad = 0
    # cvd: the game's tokens (usable cyan ~186, danger ~12) PASS; the closest pair in the two bands under deuteranopia (a scratch search over
    # hue 170-180 x 0-35, S 0.5-0.7, V 0.4-0.9: teal 175 / red 0 at S 0.5 V 0.4, normal Delta-E 44.8) FAILS
    W, H = 64, 32
    img = np.zeros((H, W, 3), np.uint8)
    img[:, :32] = M.hsv_px(186, 0.9, 0.9)
    img[:, 32:] = M.hsv_px(12, 0.9, 0.95)
    o = cvd_frame(img)
    good = o["ok"] is True
    print("planted cvd the role tokens (cyan 186 / red-orange 12): deutan Delta-E %.1f -> %s (want PASS) %s" % (o["deutan"], "PASS" if o["ok"] else "FAIL", "as planted" if good else "WRONG"))
    bad += 0 if good else 1
    img2 = np.zeros((H, W, 3), np.uint8)
    img2[:, :32] = M.hsv_px(175, 0.5, 0.4)
    img2[:, 32:] = M.hsv_px(0, 0.5, 0.4)
    o2 = cvd_frame(img2)
    good2 = o2["ok"] is False
    print("planted cvd a dull teal 175 vs a dull red 0 at one value (normal Delta-E %.1f): deutan Delta-E %.1f -> %s (want FAIL) %s" % (o2["normal"], o2["deutan"], "PASS" if o2["ok"] else "FAIL", "as planted" if good2 else "WRONG"))
    bad += 0 if good2 else 1
    # grey: bright enemies on a dark world PASS; the same enemies on a brighter world FAIL; no enemy and no hazard -> n/a
    W, H = 480, 270
    meta = {"enemies": [[200, 100, 60, 90]], "bosses": [], "hud": [], "gun": None}
    base = np.zeros((H, W, 3), np.uint8)
    for x in range(0, W, 40):
        base[:, x:x + 40] = M.hsv_px(230, 0.15, 0.15 + 0.25 * ((x // 40) % 3) / 2)
    a = base.copy()
    a[100:190, 200:260] = M.hsv_px(14, 0.85, 0.95)
    oa = grey_frame(a, meta)
    ga = oa["ok"] is True
    print("planted grey a bright enemy on dusk walls: median %.3f vs world p75 %.3f -> %s (want PASS) %s" % (oa["tellsMed"], oa["worldQ"], "PASS" if oa["ok"] else "FAIL", "as planted" if ga else "WRONG"))
    bad += 0 if ga else 1
    b = a.copy()
    for x in range(0, W, 40):
        if not (200 <= x < 260):
            b[:, x:x + 40] = M.hsv_px(230, 0.1, 0.9)
    b[100:190, 200:260] = M.hsv_px(14, 0.85, 0.55)
    ob = grey_frame(b, meta)
    gb = ob["ok"] is False
    print("planted grey a dim enemy on bright walls: median %.3f vs world p75 %.3f -> %s (want FAIL) %s" % (ob["tellsMed"], ob["worldQ"], "PASS" if ob["ok"] else "FAIL", "as planted" if gb else "WRONG"))
    bad += 0 if gb else 1
    oc = grey_frame(base, {"enemies": [], "bosses": [], "hud": [], "gun": None})
    gc = oc["ok"] is None
    print("planted grey no enemy, no hazard -> %s (want n/a) %s" % ("n/a" if oc["ok"] is None else oc["ok"], "as planted" if gc else "WRONG"))
    bad += 0 if gc else 1
    print("planted: %d / 5 as planted" % (5 - bad))
    return bad


def main():
    args = sys.argv[1:]
    if not args:
        print("usage: n57-v-cvd.py grey <dirs> [--kinds entry,mid] | cvd <frames or dirs> | planted")
        sys.exit(2)
    if args[0] == "planted":
        sys.exit(1 if planted() else 0)
    kinds = {"entry", "mid"}
    if "--kinds" in args:
        i = args.index("--kinds"); kinds = set(args[i + 1].split(",")); del args[i:i + 2]
    if args[0] == "grey":
        run_grey(args[1:], kinds)
    elif args[0] == "cvd":
        run_cvd(args[1:])
    else:
        sys.exit(2)


if __name__ == "__main__":
    main()

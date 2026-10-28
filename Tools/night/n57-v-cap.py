# NIGHT57 builder V (slate V1-colour56): THE WORLD SATURATION CAP, simulated on frames that already exist, and the cap's maths as the shader
# runs it (Assets/Shaders/Resources/Cap57.shader; Game/Render/Cap57Pass.cs). Valve's Left 4 Dead rule (research/design.md lesson 7; colors.md
# method 3): "a saturation threshold. Anything below that moves towards gray": every pixel whose hue is OUTSIDE the role families (danger /
# yellow 350-70, cyan 150-200, magenta / FUSION 285-335) and whose sRGB saturation is over the level's cap is moved to the cap along its own
# hue toward the grey of its own luminance (hue and Rec. 709 luminance kept: never a tint, the greyscale frame unchanged); the role families keep
# theirs, feathered over a few degrees inside each family's edge.
#   python -I Tools/night/n57-v-cap.py simulate <a356 dir> [...] --cap foundry=0.7,kiln=0.6 [--config C] [--kinds entry,mid]
#       each frame measured by n56-colour.py's own measure() as shot, then with the cap applied to the frame outside the HUD and the gun rects
#       (the overlay camera / canvas draw those after the pass): V1 sat (the guard: world p99.5 S <= the tells' median S), world over the tells
#       (% of the frame), the MAIN fallback (key|accent at S >= 0.35, % of the frame), and the share of the frame the cap moved
#   python -I Tools/night/n57-v-cap.py planted      (a frame the cap must change: a green wall at S 1.0 -> the cap; a red-orange enemy, a cyan strip
#       and a magenta blob it must leave alone; a near-black and a low-S wall it must not move; the feather's half-way pixel)
# A simulation on the final PNG is an estimate (the shader runs before the viewmodel and the HUD, on the post's linear colour converted to sRGB
# for the test): the Player's a357 frames (their -nocap twins) give the real numbers.
import sys, os, json, math, importlib.util
sys.dont_write_bytecode = True  # no __pycache__ beside the tools
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(M)

# the protected role families (deg, inclusive, wrapping) and the feather inside each edge: the defaults are design.n56.colour56.numbers.cap's
ROLE = [(350.0, 70.0), (150.0, 200.0), (285.0, 335.0)]
FEATHER = 6.0


def load_cap(cfg_path):
    try:
        with open(cfg_path, encoding="utf-8") as fh:
            c = json.load(fh)["design"]["n56"]["colour56"]["numbers"].get("cap", {})
    except (OSError, KeyError, ValueError):
        return {}, ROLE, FEATHER
    lv = {k: float(v) for k, v in (c.get("levels") or {}).items() if not k.startswith("_") and isinstance(v, (int, float))}
    ra = c.get("roleArcs")
    role = [(float(ra[i]), float(ra[i + 1])) for i in range(0, len(ra) - 1, 2)] if isinstance(ra, list) and len(ra) >= 2 else ROLE
    return lv, role, float(c.get("featherDeg", FEATHER))


def arc_weight(h, lo, hi, f):
    """1 deep inside [lo, hi] (wrapping), 0 outside, a smoothstep over f deg inside each edge (Cap57.shader's ArcWeight)."""
    span = (hi - lo) % 360.0
    d = (h - lo) % 360.0  # 0..360 from the arc's start
    inside = d <= span
    edge = np.minimum(d, span - d)  # distance to the nearer edge, inside the arc
    t = np.clip(edge / max(f, 1e-6), 0.0, 1.0)
    t = t * t * (3.0 - 2.0 * t)
    return np.where(inside, t, 0.0)


def protect(h, role, f):
    p = np.zeros_like(h)
    for lo, hi in role:
        p = np.maximum(p, arc_weight(h, lo, hi, f))
    return p


def load_yellow(cfg_path):
    """yellow56's dimYellow (NIGHT57 V4) as cap_rgb's y: (sMax, vFull, vNone, hueLo, hueHi, featherDeg), or None."""
    try:
        with open(cfg_path, encoding="utf-8") as fh:
            y = json.load(fh)["design"]["n56"]["yellow56"]["numbers"]["dimYellow"]
        return tuple(float(y[k]) for k in ("sMax", "vFull", "vNone", "hueLo", "hueHi", "featherDeg"))
    except (OSError, KeyError, ValueError, TypeError):
        return None


def cap_rgb(rgb_u8, cap, role=ROLE, f=FEATHER, mask=None, y=None):
    """The shader's maths on an sRGB image (uint8): returns (new uint8 image, bool map of the pixels it moved by >= 1/255). y: yellow56's dim
    yellow (sMax, vFull, vNone, hueLo, hueHi, featherDeg[, sKeep[, sKeepVMin]]) or None (off); sKeep (NIGHT59): a yellow at or over that S
    (and at or over sKeepVMin in value) is untouched."""
    a = rgb_u8.astype(np.float32) / 255.0
    mx = a.max(axis=2)
    mn = a.min(axis=2)
    s = np.where(mx > 1e-6, (mx - mn) / np.maximum(mx, 1e-6), 0.0)
    h, _, _, _ = M.hsv(rgb_u8)
    p = protect(h, role, f)
    st = s - (1.0 - p) * np.maximum(0.0, s - cap)
    if y is not None:
        ys, yv0, yv1, ylo, yhi, yf = y[:6]
        t = np.clip((mx - yv0) / max(yv1 - yv0, 1e-6), 0.0, 1.0)
        wy = arc_weight(h, ylo, yhi, yf) * (1.0 - t * t * (3.0 - 2.0 * t))
        if len(y) > 6 and y[6] > 0:  # NIGHT59 V2: the keep bound (dimYellow58.sKeep, the shader's _Cap57YH.w): S at or over it untouched,
            vmin = y[7] if len(y) > 7 else 0.0  # when the value is at least sKeepVMin (the shader's _Cap57.w)
            wy = np.where((s >= y[6]) & (mx >= vmin), 0.0, wy)
        st = np.where(st > ys, st - wy * (st - ys), st)
    # toward the grey of the same luminance (Rec. 709 on the sRGB value, the meter's lum): k solves S(L + k (c - L)) = st
    d = mx - mn
    L = 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]
    k = np.clip(st * L / np.maximum(d - st * (mx - L), 1e-6), 0.0, 1.0)
    k = np.where(st < s - 1e-7, k, 1.0)
    out = L[..., None] + k[..., None] * (a - L[..., None])
    if mask is not None:
        out = np.where(mask[..., None], out, a)
    o8 = np.clip(np.round(out * 255.0), 0, 255).astype(np.uint8)
    moved = np.abs(o8.astype(np.int16) - rgb_u8.astype(np.int16)).max(axis=2) >= 1
    return o8, moved


def simulate(roots, caps, cfg_path, kinds, with_yellow=False, with_jpg=False):
    lv_cfg, role, f = load_cap(cfg_path)
    yel = load_yellow(cfg_path) if with_yellow else None
    print("yellow56's dim yellow: %s" % (str(yel) if yel else "off (--yellow to simulate it)"))
    pal = M.load_palette(cfg_path)
    pal_h = {k: (M.hex_hue(a)[0], M.hex_hue(b)[0]) for k, (a, b) in pal.items()}
    tot = {}
    for d in M.scenario_dirs(roots):
        items, head = M.frame_list(d, kinds, False)
        items = [it for it in items if it[4] != "jpg" or with_jpg]  # --jpg: verbs37 x5 frames (f%04d.jpg + telemetry) too
        if not items:
            continue
        mp = M.reel_map(d)
        for name, path, lvid, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
            rgb = np.asarray(Image.open(path).convert("RGB"))
            meta = M.meta_of(fr, rgb.shape[0], extra)
            key = M.level_key(lvid or mp)
            cap = caps.get(key, lv_cfg.get(key))
            if cap is None:
                print("  %s: no cap for '%s' (skipped)" % (name, key))
                continue
            H, W = rgb.shape[0], rgb.shape[1]
            keep = np.zeros((H, W), bool)
            for r in meta.get("hud", []):
                if r[2] * r[3] <= 0.25 * W * H:
                    M.rect_mask(keep, r)
            if meta.get("gun"):
                M.rect_mask(keep, meta["gun"])
            pe = pal_h.get(key)
            o0 = M.measure(rgb, meta, pe, True)
            img, moved = cap_rgb(rgb, cap, role, f, ~keep, yel)
            if yel is not None and meta.get("gun"):
                # 1850d877: the viewmodel's overlay camera runs the dim yellow alone on the gun (colour56's world cap never touches it)
                gm = np.zeros((H, W), bool)
                M.rect_mask(gm, meta["gun"])
                for r in meta.get("hud", []):  # the HUD over the gun draws after every camera
                    if r[2] * r[3] <= 0.25 * W * H:
                        M.rect_mask(gm, r, False)
                img, mv2 = cap_rgb(img, 1.0, role, f, gm, yel)
                moved |= mv2
            o1 = M.measure(img, meta, pe, True)
            T = tot.setdefault(key, {"n": 0, "sat0": 0, "sat1": 0, "cov0": [], "cov1": [], "moved": [], "over0": [], "over1": [], "en": 0, "body0": 0, "body1": 0, "orig0": 0, "orig1": 0, "y0": [], "y1": []})
            T["n"] += 1
            T["y0"].append(o0["yellowPct"]); T["y1"].append(o1["yellowPct"])
            T["sat0"] += 0 if o0["satOk"] else 1
            T["sat1"] += 0 if o1["satOk"] else 1
            T["cov0"].append(o0.get("keyOrAccentPct", 0.0))
            T["cov1"].append(o1.get("keyOrAccentPct", 0.0))
            T["moved"].append(100.0 * moved.sum() / (W * H))
            T["over0"].append(o0["worldOverTellsPct"])
            T["over1"].append(o1["worldOverTellsPct"])
            # V4 rim56's numbers on the same frame (n56-colour.py's V2): the enemies in view whose body beats the environment's p90 V and S, and
            # the original rule (the rect's mean V and S over the world's p90): the cap lowers the world's saturation, never its value
            T["en"] += o0["enemiesInView"]
            T["body0"] += o0["enemiesBodyOk"]; T["body1"] += o1["enemiesBodyOk"]
            T["orig0"] += o0["enemiesOk"]; T["orig1"] += o1["enemiesOk"]
            print("  %s cap %.2f | world S p99.5 %.2f -> %.2f vs tells %.2f (%s): %s -> %s | world over the tells %.2f%% -> %.2f%% | key|accent (S >= 0.35) %.1f%% -> %.1f%% | the cap moved %.1f%% of the frame"
                  % (name, cap, o0["worldS995"], o1["worldS995"], o1["tellS"], o1["tellSrc"], M.fmt_ok(o0["satOk"]), M.fmt_ok(o1["satOk"]),
                     o0["worldOverTellsPct"], o1["worldOverTellsPct"], o0.get("keyOrAccentPct", -1), o1.get("keyOrAccentPct", -1), 100.0 * moved.sum() / (W * H)))
    print("== per level (simulated)")
    for k in sorted(tot):
        T = tot[k]
        print("  %s: %d frames | world out-saturates the tells (sat FAIL) %d -> %d | key|accent cover median %.1f%% -> %.1f%% (min %.1f%% -> %.1f%%) | moved median %.1f%% (max %.1f%%) | world over the tells median %.2f%% -> %.2f%%"
              % (k, T["n"], T["sat0"], T["sat1"], float(np.median(T["cov0"])), float(np.median(T["cov1"])), min(T["cov0"]), min(T["cov1"]),
                 float(np.median(T["moved"])), max(T["moved"]), float(np.median(T["over0"])), float(np.median(T["over1"]))))
    n = sum(T["n"] for T in tot.values())
    print("TOTAL %d frames | sat FAIL %d -> %d" % (n, sum(T["sat0"] for T in tot.values()), sum(T["sat1"] for T in tot.values())))
    for k in sorted(tot):
        T = tot[k]
        print("  V2 %s: enemies in view %d | body over the environment's p90 V and S %d -> %d | the original rule (rect mean over the world's p90) %d -> %d" % (k, T["en"], T["body0"], T["body1"], T["orig0"], T["orig1"]))
    for k in sorted(tot):
        T = tot[k]
        y0, y1 = np.array(T["y0"]), np.array(T["y1"])
        print("  V3 yellow %s: frames over 0.5 %% %d -> %d of %d (median %.2f%% -> %.2f%%, max %.2f%% -> %.2f%%; the gun's share is not touched: it draws after the pass)"
              % (k, int((y0 > 0.5).sum()), int((y1 > 0.5).sum()), len(y0), float(np.median(y0)), float(np.median(y1)), float(y0.max()), float(y1.max())))
    en = sum(T["en"] for T in tot.values())
    print("V2 TOTAL enemies in view %d | body over env p90 %d -> %d | original %d -> %d" % (en, sum(T["body0"] for T in tot.values()), sum(T["body1"] for T in tot.values()), sum(T["orig0"] for T in tot.values()), sum(T["orig1"] for T in tot.values())))
    return tot


def planted():
    W, H = 64, 16
    img = np.zeros((H, W, 3), np.uint8)
    img[:, 0:8] = M.hsv_px(120, 1.0, 0.9)    # a green wall at S 1.0: must drop to the cap
    img[:, 8:16] = M.hsv_px(12, 0.95, 0.95)  # a red-orange enemy: kept
    img[:, 16:24] = M.hsv_px(186, 0.9, 0.9)  # the usable cyan: kept
    img[:, 24:32] = M.hsv_px(312, 0.85, 0.9)  # RIVAL magenta: kept
    img[:, 32:40] = M.hsv_px(240, 0.2, 0.5)  # a low-S blue wall under the cap: not moved
    img[:, 40:48] = M.hsv_px(220, 0.0, 0.02)  # near black: not moved
    img[:, 48:56] = M.hsv_px(150 + FEATHER / 2, 1.0, 0.9)  # half way into the cyan family's feather: part way to the cap
    img[:, 56:64] = M.hsv_px(44, 0.9, 0.95)  # the parry yellow token's hue: kept
    cap = 0.6
    out, moved = cap_rgb(img, cap)
    h, s, _, v = M.hsv(out)  # the 4th is the Rec. 709 luminance: the cap keeps it (the greyscale frame)
    h0, s0, _, v0 = M.hsv(img)
    col = lambda x: (float(h[0, x]), float(s[0, x]), float(v[0, x]), float(h0[0, x]), float(s0[0, x]), float(v0[0, x]))
    checks = []
    a = col(4)
    checks.append(("green S 1.0 -> the cap 0.60, hue and luma kept", abs(a[1] - cap) < 0.01 and M.gap(a[0], a[3]) < 2 and abs(a[2] - a[5]) < 0.006, a))
    img2 = img.copy()
    img2[:, 0:8] = M.hsv_px(230, 0.95, 0.8)  # a saturated blue wall: its luma must stay (the V-keeping cap lifted a blue's luma ~4x)
    o2, _ = cap_rgb(img2, cap)
    hb, sb, _, lb = M.hsv(o2)
    _, _, _, lb0 = M.hsv(img2)
    b0 = (float(hb[0, 4]), float(sb[0, 4]), float(lb[0, 4]), 230.0, 0.95, float(lb0[0, 4]))
    checks.append(("blue S 0.95 -> the cap, luma kept", abs(b0[1] - cap) < 0.01 and abs(b0[2] - b0[5]) < 0.006, b0))
    for x, nm in ((12, "red-orange enemy"), (20, "usable cyan"), (28, "RIVAL magenta"), (36, "low-S blue wall"), (44, "near black"), (60, "parry yellow")):
        b = col(x)
        checks.append((nm + " unchanged", not moved[0, x], b))
    c = col(52)
    checks.append(("feather half way: S between the cap and 1.0", cap + 0.05 < c[1] < 0.98, c))
    bad = 0
    for nm, ok, vals in checks:
        bad += 0 if ok else 1
        print("planted %-48s -> %s (now H %.0f S %.2f luma %.3f; was H %.0f S %.2f luma %.3f)" % (nm, "as planted" if ok else "WRONG", *vals))
    # the negative control: a cap of 1.0 moves nothing (the row's numbers can never brighten or saturate)
    _, mv = cap_rgb(img, 1.0)
    print("planted cap 1.00 moves %d pixels (want 0) -> %s" % (int(mv.sum()), "as planted" if mv.sum() == 0 else "WRONG"))
    bad += 0 if mv.sum() == 0 else 1
    # yellow56's dim yellow (NIGHT57 V4): a dim mustard wall drops under the meter's 0.5; the parry token and a bright yellow stay; off moves nothing
    y = (0.45, 0.5, 0.75, 40.0, 76.0, 4.0)
    yi = np.zeros((4, 32, 3), np.uint8)
    yi[:, 0:8] = M.hsv_px(55, 0.8, 0.35)   # a dim mustard wall
    yi[:, 8:16] = M.hsv_px(44, 0.77, 1.0)  # the parry token #FFC93A
    yi[:, 16:24] = M.hsv_px(60, 0.9, 0.9)  # a bright yellow (a lava edge, a pickup)
    yi[:, 24:32] = M.hsv_px(20, 0.9, 0.35)  # a dim danger red-orange: not in the yellow arc
    yo, ym = cap_rgb(yi, 1.0, y=y)
    _, ysat, _, ylum = M.hsv(yo)
    _, _, _, ylum0 = M.hsv(yi)
    yc = [("dim mustard 55 S 0.80 V 0.35 -> S <= 0.46, luma kept", float(ysat[0, 4]) <= 0.46 and abs(float(ylum[0, 4]) - float(ylum0[0, 4])) < 0.006),
          ("the parry token 44 V 1.0 unchanged", not ym[0, 12]),
          ("a bright yellow 60 V 0.9 unchanged", not ym[0, 20]),
          ("a dim red-orange 20 V 0.35 unchanged", not ym[0, 28])]
    _, ym0 = cap_rgb(yi, 1.0, y=None)
    yc.append(("yellow56 off (y None) moves nothing", int(ym0.sum()) == 0))
    for nm, ok in yc:
        bad += 0 if ok else 1
        print("planted yellow %-52s -> %s" % (nm, "as planted" if ok else "WRONG"))
    print("planted: %d / %d as planted" % (len(checks) + 1 + len(yc) - bad, len(checks) + 1 + len(yc)))
    return bad


def main():
    args = sys.argv[1:]
    if not args:
        print("usage: n57-v-cap.py simulate <dirs> --cap level=S,... | planted")
        sys.exit(2)
    if args[0] == "planted":
        sys.exit(1 if planted() else 0)
    if args[0] != "simulate":
        sys.exit(2)
    args = args[1:]
    cfg = "Design/unity-config.json"
    kinds = {"entry", "mid"}
    caps = {}
    if "--config" in args:
        i = args.index("--config"); cfg = args[i + 1]; del args[i:i + 2]
    if "--kinds" in args:
        i = args.index("--kinds"); kinds = set(args[i + 1].split(",")); del args[i:i + 2]
    with_yellow = "--yellow" in args
    with_jpg = "--jpg" in args
    if with_jpg:
        args.remove("--jpg")
    if with_yellow:
        args.remove("--yellow")
    if "--cap" in args:
        i = args.index("--cap")
        for kv in args[i + 1].split(","):
            k, v = kv.split("=")
            caps[k] = float(v)
        del args[i:i + 2]
    simulate(args, caps, cfg, kinds, with_yellow, with_jpg)


if __name__ == "__main__":
    main()

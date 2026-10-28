# NIGHT58 builder V, slate V1 (rim56): SIMULATE the fix on the kept a357 stills (no Player): the level ACCENT's pixels (colour56's accent hue
# +-20 deg at S >= 0.35, the env's top on foundry: n58-v-rimcause.py) at x k of their sRGB value, the env p90 V re-read, each body judged as the
# meter does (body V and S over the env p90 V and S). Prints per level and overall, for each k. An estimate on stills, said so on every line.
#   python -I Tools/night/n58-v-rimsim.py <a357 dir> [<a357 dir> ...] [--k 1,0.85,0.75,0.6]
import sys, os, importlib.util
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n58rimcause", os.path.join(HERE, "n58-v-rimcause.py"))
C = importlib.util.module_from_spec(spec)
spec.loader.exec_module(C)
M = C.M


def env_mask(rgb, meta):
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, lum = M.hsv(rgb)
    m = np.zeros((H, W), bool)
    hud = np.zeros((H, W), bool)
    for r in meta.get("hud", []):
        if r[2] * r[3] <= 0.25 * W * H:
            M.rect_mask(hud, r)
    en = np.zeros((H, W), bool)
    for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
        M.rect_mask(en, r)
    gun = np.zeros((H, W), bool)
    if meta.get("gun"):
        M.rect_mask(gun, meta["gun"])
    edge = np.zeros((H, W), bool)
    eb = int(round(M.EDGE_PX * H / 1080.0))
    edge[:eb, :] = edge[-eb:, :] = True
    edge[:, :eb] = edge[:, -eb:] = True
    sky = np.zeros((H, W), bool)
    hy = meta.get("horizon")
    if hy is not None and hy > 0:
        sky[:hy, :] = True
    grid = meta.get("skyGrid")
    if grid and grid.get("bits"):
        cols, rows, bits = int(grid["cols"]), int(grid["rows"]), grid["bits"]
        g = np.array([c == "1" for c in bits[:cols * rows]], bool).reshape(rows, cols)
        gy = np.minimum((np.arange(H) * rows) // H, rows - 1)
        gx = np.minimum((np.arange(W) * cols) // W, cols - 1)
        sky &= g[gy][:, gx]
    role = M.band(h, 350.0, 360.0) | M.band(h, 0.0, 35.0) | M.band(h, *M.YELLOW) | M.band(h, 150.0, 200.0) | M.band(h, *M.MAGENTA) | M.band(h, 35.0, 45.0)
    return ~hud & ~en & ~gun & ~edge & ~role & ~sky, h, s, v


def main():
    args = sys.argv[1:]
    ks = [1.0, 0.85, 0.75, 0.6]
    if "--k" in args:
        i = args.index("--k")
        ks = [float(x) for x in args[i + 1].split(",")]
        del args[i:i + 2]
    pal = M.load_palette(os.path.join(HERE, "..", "..", "Design", "unity-config.json"))
    pal_h = {k: (M.hex_hue(a)[0], M.hex_hue(b)[0]) for k, (a, b) in pal.items()}
    res = {k: {} for k in ks}
    for root in args:
        sh = C.shots(root)
        for name, (rgb, meta) in sorted(sh.items()):
            lv = name.split("-")[3] if name.count("-") >= 4 else "?"
            o = M.measure(rgb, meta, None, True)
            vis = [e for e in o["enemies"] if e["inView"]]
            env, h, s, v = env_mask(rgb, meta)
            if env.sum() <= 100:
                continue
            acc = pal_h.get(lv, (None, None))[1]
            am = env & (M.near(h, acc, 20.0) if acc is not None else np.zeros_like(env)) & (s >= 0.35)
            # colour56's MAIN as a guard (the lead, 14:58): the key or accent at S >= 0.35, V >= 0.08 on >= 3 % of the frame, with the accent's
            # pixels (everywhere outside hud / enemies / gun, as the meter counts them) at x k of their value
            pe = pal_h.get(lv)
            cov = {}
            if pe is not None:
                free = M.measure(rgb, meta, pe, True)
                hud = np.zeros_like(env)
                for r in meta.get("hud", []):
                    if r[2] * r[3] <= 0.25 * hud.size:
                        M.rect_mask(hud, r)
                en = np.zeros_like(env)
                for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
                    M.rect_mask(en, r)
                gun = np.zeros_like(env)
                if meta.get("gun"):
                    M.rect_mask(gun, meta["gun"])
                fr_ = ~hud & ~en & ~gun & (s >= M.S_KEY)
                km = fr_ & M.near(h, pe[0], M.KEY_TOL) & (v >= M.V_KEY)
                amk = fr_ & M.near(h, pe[1], M.KEY_TOL)
                for k in ks:
                    cov[k] = 100.0 * (km | (amk & (v * k >= M.V_KEY))).sum() / env.size
            for k in ks:
                vv = np.where(am, v * k, v)
                v90 = float(np.percentile(vv[env], 90))
                ok = sum(1 for e in vis if e["bodyV"] > v90 and e["bodyS"] > o["envS90"])
                a = res[k].setdefault(lv, [0, 0, 0, 0])
                a[0] += ok
                a[1] += len(vis)
                if k in cov:
                    a[2] += 1 if cov[k] >= 3.0 else 0
                    a[3] += 1
    for k in ks:
        tot = [sum(x[0] for x in res[k].values()), sum(x[1] for x in res[k].values())]
        cov = [sum(x[2] for x in res[k].values()), sum(x[3] for x in res[k].values())]
        print("SIMULATED (stills) accent value x %.2f: %s | overall %d / %d (%.0f%%) | colour56 MAIN (key|accent >= 3 %%) %d / %d frames" % (
            k, ", ".join("%s %d/%d" % (lv, a[0], a[1]) for lv, a in sorted(res[k].items())), tot[0], tot[1], 100.0 * tot[0] / max(1, tot[1]), cov[0], cov[1]))
    return 0


if __name__ == "__main__":
    sys.exit(main())

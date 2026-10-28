# NIGHT58 builder V, slate V1 (rim56 on foundry): THE CAUSE, printed from the kept a357 stills (no Player). Uses n56-colour.py's own measure()
# (the meter the row is judged by) and adds, per a357 shot:
#   - the bodies' V / S (each enemy in view) against the environment's p90 V / S (env = the world outside every role band)
#   - WHAT sets the env p90 V: the env pixels at or over it, by hue bin (top 3) and by where they sit (upper / lower half of the frame)
#   - with a second <dir> (the same level, rim56 off: v1none): the same room's bodies' V on that side, so the rim's lift per shot is printed
#   python -I Tools/night/n58-v-rimcause.py <a357 dir on> [<a357 dir off>]
import sys, os, importlib.util
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)


def shots(root):
    out = {}
    for d in M.scenario_dirs([root]):
        items, _ = M.frame_list(d, ["entry", "mid"], False)
        for name, path, lv, room, kind, fr, extra, how in items:
            rgb = np.asarray(Image.open(path).convert("RGB"))
            meta = M.meta_of(fr, rgb.shape[0], extra)
            out[os.path.basename(path)] = (rgb, meta)
    return out


def cause(rgb, meta):
    o = M.measure(rgb, meta, None, True)
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, lum = M.hsv(rgb)
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
    role = M.band(h, 350.0, 360.0) | M.band(h, 0.0, 35.0) | M.band(h, *M.YELLOW) | M.band(h, 150.0, 200.0) | M.band(h, *M.MAGENTA) | M.band(h, 35.0, 45.0)
    # the sky exactly as measure() takes it (the horizon row AND the a356 sidecar's open ray cells)
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
    env = ~hud & ~en & ~gun & ~edge & ~role & ~sky
    top = env & (v >= o["envV90"])
    hist, ed = np.histogram(h[top], bins=36, range=(0, 360))
    idx = np.argsort(hist)[::-1][:3]
    bins = ["%d-%d %.0f%%" % (ed[i], ed[i + 1], 100.0 * hist[i] / max(1, top.sum())) for i in idx if hist[i] > 0]
    ys = np.nonzero(top)[0]
    upper = 100.0 * (ys < H // 2).sum() / max(1, len(ys))
    sm = float(np.median(s[top])) if top.sum() else 0.0
    return o, bins, upper, sm


def main():
    if len(sys.argv) < 2:
        print(__doc__ or "usage: n58-v-rimcause.py <a357 dir on> [<a357 dir off>]")
        return 2
    on = shots(sys.argv[1])
    off = shots(sys.argv[2]) if len(sys.argv) > 2 else {}
    tot_in = tot_ok = 0
    need = []
    lifts = []
    for name in sorted(on):
        rgb, meta = on[name]
        o, bins, upper, sm = cause(rgb, meta)
        vis = [e for e in o["enemies"] if e["inView"]]
        tot_in += len(vis)
        tot_ok += sum(1 for e in vis if e["bodyOk"])
        line = "%s | env p90 V %.3f S %.3f | env pixels at/over p90 V: hues %s, upper half %.0f%%, S median %.2f" % (name, o["envV90"], o["envS90"], ", ".join(bins), upper, sm)
        offv = None
        if name in off:
            o2 = M.measure(off[name][0], off[name][1], None, True)
            v2 = [e for e in o2["enemies"] if e["inView"]]
            offv = [round(e["bodyV"], 3) for e in v2]
            line += " | rim off: env p90 V %.3f, bodies V %s" % (o2["envV90"], offv)
        print(line)
        for e in vis:
            short = o["envV90"] - e["bodyV"]
            print("    body V %.3f S %.3f (warm %.0f%%, rect %s) -> %s%s" % (e["bodyV"], e["bodyS"], 100 * e["warm"], e["rect"], "over" if e["bodyOk"] else "UNDER",
                                                                 "" if e["bodyOk"] else " by V %.3f%s" % (short, " and S" if e["bodyS"] <= o["envS90"] else "")))
            if not e["bodyOk"]:
                need.append(short)
        if offv and vis:
            vo = np.mean([e["bodyV"] for e in vis])
            lifts.append(vo - np.mean(offv))
    print("RIM CAUSE: enemies in view %d, body over env p90 %d (%.0f%%); the shortfalls in V (env p90 - body): %s" % (
        tot_in, tot_ok, 100.0 * tot_ok / max(1, tot_in), ", ".join("%.3f" % x for x in sorted(need))))
    if lifts:
        print("RIM LIFT (rim56 on vs off, the same shots, mean body V per shot): %s" % ", ".join("%+.3f" % x for x in lifts))
    return 0


if __name__ == "__main__":
    sys.exit(main())

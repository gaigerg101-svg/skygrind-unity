# NIGHT60 builder V (B7, rim56's trims): WHICH DRAW PATH the accent on screen takes, and whether rim56's accent dim (NIGHT58, Rim58.AccentMax inside
# Colour56.Clamp) reaches it. Reads a357 shot pairs at the same pose, rim56 on vs off (NIGHT58's r1: <root>/v1on-<level>/a357 and v1off-<level>/a357).
# Per shot:
#   - the accent pixels: the level's accent hue (design.n56.colour56.numbers.levels.<key>.accentHex) +-ACC_TOL deg, S >= 0.25, V >= 0.2, on the OFF frame;
#     REACHED = darker on the ON frame by > SAME (rim56's dim landed), UNREACHED = the same on and off within SAME (it did not)
#   - n56-colour.py's own environment p90 V (its measure(): env = world minus the role hues), off and on, and the share of the env pixels at or
#     over the off p90 that are UNREACHED accent pixels (what sets the p90 and whether the dim can move it)
#   - the median V ratio on / off of the REACHED accent pixels (what rim56's dim does to a piece it reaches, measured on the frame)
# PLANTED (L-111: at the colour the change really draws): the ON frame with its UNREACHED accent pixels scaled by k in RGB (k = the band scales
# to try; the reached ratio is printed beside them), measured by n56-colour.py's measure(): the bodies over the env p90 and the V1 cover check,
# per shot and in total, beside the unplanted ON and OFF frames. An estimate on the final PNG, never a Player number.
#   python -I Tools/night/n60-v-rimpath.py <root> [--levels foundry,iceline,kiln,span,spire] [--k 0.45,0.6,0.75] [--quiet]
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)

ACC_TOL = 15.0  # deg around the level's accent hue
SAME = 0.03  # max channel difference that reads as the same pixel on and off (frame noise; the reel's poses are fixed)


def main():
    args = sys.argv[1:]
    if not args:
        print(__doc__ if __doc__ else "usage: n60-v-rimpath.py <root>")
        return 2
    root = args[0]
    levels = ["foundry", "iceline", "kiln", "span", "spire"]
    ks = [0.45, 0.6, 0.75]
    if "--levels" in args:
        i = args.index("--levels"); levels = args[i + 1].split(",")
    if "--k" in args:
        i = args.index("--k"); ks = [float(x) for x in args[i + 1].split(",")]
    quiet = "--quiet" in args
    pal = M.load_palette(os.path.join(HERE, "..", "..", "Design", "unity-config.json"))
    tot = {"off": [0, 0], "on": [0, 0]}
    totk = {k: [0, 0] for k in ks}
    cover = {"on": [0, 0]}
    coverk = {k: [0, 0] for k in ks}
    for lvl in levels:
        on_root, off_root = os.path.join(root, "v1on-" + lvl, "a357"), os.path.join(root, "v1off-" + lvl, "a357")
        if not os.path.isdir(on_root) or not os.path.isdir(off_root):
            print("== %s: no v1on / v1off a357 folders under %s" % (lvl, root))
            continue
        acc_hex = pal.get(lvl, (None, None))[1]
        acc_h = M.hex_hue(acc_hex)[0] if acc_hex is not None else None
        pe = (M.hex_hue(pal[lvl][0])[0], acc_h) if lvl in pal else None
        offs = {}
        for d in M.scenario_dirs([off_root]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for name, path, lv, room, kind, fr, extra, how in items:
                offs[os.path.basename(path)] = (path, fr, extra)
        lv_tot = {"off": [0, 0], "on": [0, 0]}
        lv_k = {k: [0, 0] for k in ks}
        print("== %s (accent #%06x hue %.0f): on %s vs off %s" % (lvl, acc_hex or 0, acc_h if acc_h is not None else -1, on_root, off_root))
        for d in M.scenario_dirs([on_root]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for name, path, lv, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
                b = os.path.basename(path)
                if b not in offs:
                    continue
                a = np.asarray(Image.open(path).convert("RGB"))
                o_path, o_fr, o_extra = offs[b]
                z = np.asarray(Image.open(o_path).convert("RGB"))
                meta_on = M.meta_of(fr, a.shape[0], extra)
                meta_off = M.meta_of(o_fr, z.shape[0], o_extra)
                ra = M.measure(a, meta_on, pe, True)
                rz = M.measure(z, meta_off, pe, True)
                ha, sa, va, _ = M.hsv(a)
                hz, sz, vz, _ = M.hsv(z)
                af, zf = a.astype(float) / 255.0, z.astype(float) / 255.0
                same = np.abs(af - zf).max(axis=2) <= SAME
                acc = (M.near(hz, acc_h, ACC_TOL) if acc_h is not None else np.zeros_like(same)) & (sz >= 0.25) & (vz >= 0.2)
                unreached = acc & same
                reached = acc & ~same & (va < vz - SAME)
                ratio = float(np.median(va[reached] / np.maximum(vz[reached], 1e-3))) if reached.sum() > 50 else float("nan")
                # what sets the env p90 on the OFF frame: n56-colour's env (world minus the role hues), recomputed here from its own pieces
                envm = env_mask(z, meta_off)
                p90 = rz["envV90"]
                top = envm & (vz >= p90)
                top_un = 100.0 * (top & unreached).sum() / max(1, top.sum())
                n_in, ok_off, ok_on = rz["enemiesInView"], rz["enemiesBodyOk"], ra["enemiesBodyOk"]
                lv_tot["off"][0] += ok_off; lv_tot["off"][1] += n_in
                lv_tot["on"][0] += ok_on; lv_tot["on"][1] += ra["enemiesInView"]
                cover["on"][0] += 1 if ra["coverOk"] else 0; cover["on"][1] += 1
                line = "%s | accent px %d: unreached (same on / off) %.1f %%, reached %.1f %% (V on/off median %.2f) | env p90 V off %.3f on %.3f; of the env px at or over it (off) %.1f %% are unreached accent | bodies over env p90: off %d / %d, on %d / %d" % (
                    b, acc.sum(), 100.0 * unreached.sum() / max(1, acc.sum()), 100.0 * reached.sum() / max(1, acc.sum()), ratio, p90, ra["envV90"], top_un, ok_off, n_in, ok_on, ra["enemiesInView"])
                for k in ks:
                    pl = af.copy()
                    pl[unreached] *= k
                    rp = M.measure((pl * 255.0 + 0.5).astype(np.uint8), meta_on, pe, True)
                    lv_k[k][0] += rp["enemiesBodyOk"]; lv_k[k][1] += rp["enemiesInView"]
                    coverk[k][0] += 1 if rp["coverOk"] else 0; coverk[k][1] += 1
                    line += " | PLANTED x%.2f: env p90 %.3f, bodies %d / %d, V1 cover %s" % (k, rp["envV90"], rp["enemiesBodyOk"], rp["enemiesInView"], M.fmt_ok(rp["coverOk"]))
                if not quiet:
                    print("  " + line)
        for s in ("off", "on"):
            tot[s][0] += lv_tot[s][0]; tot[s][1] += lv_tot[s][1]
        for k in ks:
            totk[k][0] += lv_k[k][0]; totk[k][1] += lv_k[k][1]
        print("  %s: bodies over env p90 off %d / %d, on %d / %d; planted %s" % (lvl, lv_tot["off"][0], lv_tot["off"][1], lv_tot["on"][0], lv_tot["on"][1],
                                                                              ", ".join("x%.2f %d / %d" % (k, lv_k[k][0], lv_k[k][1]) for k in ks)))
    pct = lambda p: "%d / %d (%.1f %%)" % (p[0], p[1], 100.0 * p[0] / max(1, p[1]))
    print("TOTAL bodies over env p90: off %s, on %s; PLANTED (the unreached accent x k on the ON frames): %s (want >= 90 %%)" % (
        pct(tot["off"]), pct(tot["on"]), "; ".join("x%.2f %s" % (k, pct(totk[k])) for k in ks)))
    print("TOTAL V1 cover (n56-colour's key|accent >= 3 %%): on %d / %d; planted %s" % (cover["on"][0], cover["on"][1], ", ".join("x%.2f %d / %d" % (k, coverk[k][0], coverk[k][1]) for k in ks)))
    return 0


def env_mask(rgb, meta):
    """n56-colour.py's environment (measure(): world = free minus sky and the edge band; env = world minus the role hues), from its pieces."""
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, _ = M.hsv(rgb)
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
    edge = np.zeros((H, W), bool)
    eb = int(round(M.EDGE_PX * H / 1080.0))
    if eb > 0:
        edge[:eb, :] = True; edge[-eb:, :] = True; edge[:, :eb] = True; edge[:, -eb:] = True
    world = ~hud & ~en & ~gun & ~sky & ~edge
    role = M.band(h, 350.0, 360.0) | M.band(h, 0.0, 35.0) | M.band(h, *M.YELLOW) | M.band(h, 150.0, 200.0) | M.band(h, *M.MAGENTA) | M.band(h, 35.0, 45.0)
    return world & ~role


if __name__ == "__main__":
    sys.exit(main())

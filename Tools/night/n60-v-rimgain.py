# NIGHT60 builder V (B7 attempt 2, rim56's lit gain): what lit gain keeps the brightest bodies' saturation over the environment's, estimated on
# a357 shot pairs at the same pose, rim56 on (the lit gain Rim56LitGain x Rim57Gain = 2.205) vs off (skin52's own, Skin52LitGain 1.0)
# (NIGHT58's r1: <root>/v1on-<level>/a357 and v1off-<level>/a357).
# THE PLANTED FRAME for a gain g: the ON frame, with every pixel inside an enemy rect replaced by the off and on pixels mixed in LINEAR light at
# t = (g - 1.0) / (2.205 - 1.0) (t 1 = the ON frame exactly, t 0 = the OFF body), and with B7's band fix planted too (the accent pixels that are
# the same on and off x the band scale, n60-v-rimpath.py's rule) when --band k is given; measured by n56-colour.py's own measure(): the bodies
# over the env p90 (V and S), the V1 cover. An estimate between two measured renders: it moves rim56's whole body change (the lit gain, the
# shade, the near rim) by the same t, the post chain is not modelled between them, and it is never a Player number (uncalibrated).
# PLANTED CHECK first: g 2.205 must give the ON frame's own count on every shot (the plumbing), else the run stops.
#   python -I Tools/night/n60-v-rimgain.py <root> [--gains 2.205,2.0,1.8,1.6,1.5,1.3,1.0] [--band 0.45] [--levels foundry,...] [--quiet]
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n60rimpath", os.path.join(HERE, "n60-v-rimpath.py"))
R = importlib.util.module_from_spec(spec)
spec.loader.exec_module(R)
M = R.M

G_ON, G_OFF = 2.205, 1.0  # UnityTuning.Rim56LitGain 1.5 x Rim57Gain 1.47 (the r1 ON reels' "lit side's gain written 2.21"); Skin52LitGain 1.0


def lin(c):
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def srgb(c):
    c = np.clip(c, 0.0, 1.0)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * c ** (1 / 2.4) - 0.055)


def main():
    args = sys.argv[1:]
    root = args[0]
    gains = [2.205, 2.0, 1.8, 1.6, 1.5, 1.3, 1.0]
    band = None
    levels = ["foundry", "iceline", "kiln", "span", "spire"]
    if "--gains" in args:
        i = args.index("--gains"); gains = [float(x) for x in args[i + 1].split(",")]
    if "--band" in args:
        i = args.index("--band"); band = float(args[i + 1])
    if "--levels" in args:
        i = args.index("--levels"); levels = args[i + 1].split(",")
    quiet = "--quiet" in args
    pal = M.load_palette(os.path.join(HERE, "..", "..", "Design", "unity-config.json"))
    tot = {g: [0, 0] for g in gains}
    cov = {g: [0, 0] for g in gains}
    sat_fail = {g: 0 for g in gains}
    val_fail = {g: 0 for g in gains}
    check_bad = 0
    print("n60-v-rimgain on %s: gains %s, the band fix %s" % (root, ", ".join("%.3f" % g for g in gains), ("x%.2f" % band) if band is not None else "not planted"))
    for lvl in levels:
        on_root, off_root = os.path.join(root, "v1on-" + lvl, "a357"), os.path.join(root, "v1off-" + lvl, "a357")
        if not os.path.isdir(on_root) or not os.path.isdir(off_root):
            continue
        acc_h = M.hex_hue(pal[lvl][1])[0] if lvl in pal else None
        pe = (M.hex_hue(pal[lvl][0])[0], acc_h) if lvl in pal else None
        offs = {}
        for d in M.scenario_dirs([off_root]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for it in items:
                offs[os.path.basename(it[1])] = it
        lv = {g: [0, 0] for g in gains}
        for d in M.scenario_dirs([on_root]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for name, path, lvid, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
                b = os.path.basename(path)
                if b not in offs:
                    continue
                a8 = np.asarray(Image.open(path).convert("RGB"))
                z8 = np.asarray(Image.open(offs[b][1]).convert("RGB"))
                meta = M.meta_of(fr, a8.shape[0], extra)
                a, z = a8.astype(float) / 255.0, z8.astype(float) / 255.0
                H, W = a.shape[0], a.shape[1]
                en = np.zeros((H, W), bool)
                for r in meta.get("enemies", []):
                    M.rect_mask(en, r)
                base = a.copy()
                if band is not None and acc_h is not None:
                    hz, sz, vz, _ = M.hsv(z8)
                    same = np.abs(a - z).max(axis=2) <= R.SAME
                    acc = M.near(hz, acc_h, R.ACC_TOL) & (sz >= 0.25) & (vz >= 0.2)
                    base[acc & same & ~en] *= band
                la, lz = lin(a), lin(z)
                ref = M.measure(a8, meta, pe, True)
                line = "%s | in view %d, on %d" % (b, ref["enemiesInView"], ref["enemiesBodyOk"])
                for g in gains:
                    t = (g - G_OFF) / (G_ON - G_OFF)
                    pl = base.copy()
                    pl[en] = srgb(lz[en] + t * (la[en] - lz[en]))
                    o = M.measure((pl * 255.0 + 0.5).astype(np.uint8), meta, pe, True)
                    if abs(g - G_ON) < 1e-9 and band is None and o["enemiesBodyOk"] != ref["enemiesBodyOk"]:
                        check_bad += 1
                    lv[g][0] += o["enemiesBodyOk"]; lv[g][1] += o["enemiesInView"]
                    cov[g][0] += 1 if o["coverOk"] else 0; cov[g][1] += 1
                    for e in o["enemies"]:
                        if e["inView"] and not e["bodyOk"]:
                            sat_fail[g] += 1 if e["bodyS"] <= o["envS90"] else 0
                            val_fail[g] += 1 if e["bodyV"] <= o["envV90"] else 0
                    line += " | g %.2f: %d (%s)" % (g, o["enemiesBodyOk"], ", ".join("V %.2f S %.2f" % (e["bodyV"], e["bodyS"]) for e in o["enemies"] if e["inView"] and not e["bodyOk"]) or "all over")
                if not quiet:
                    print("  " + line)
        for g in gains:
            tot[g][0] += lv[g][0]; tot[g][1] += lv[g][1]
        print("  %s: %s" % (lvl, ", ".join("g %.2f %d / %d" % (g, lv[g][0], lv[g][1]) for g in gains)))
    if band is None:
        print("PLANTED CHECK (g %.3f reproduces the ON frames' own count on every shot): %s" % (G_ON, "GREEN" if check_bad == 0 else "RED (%d shots differ)" % check_bad))
    for g in gains:
        print("TOTAL g %.3f: bodies over env p90 %d / %d (%.1f %%); failing on S %d, on V %d (a body can fail both); V1 cover %d / %d" % (
            g, tot[g][0], tot[g][1], 100.0 * tot[g][0] / max(1, tot[g][1]), sat_fail[g], val_fail[g], cov[g][0], cov[g][1]))
    return 0 if check_bad == 0 else 1


if __name__ == "__main__":
    sys.exit(main())

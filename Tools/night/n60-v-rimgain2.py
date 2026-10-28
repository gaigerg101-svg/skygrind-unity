# NIGHT60 builder V (B7 attempt 3): rim56's lit gain against BOTH the MAIN (bodies over the env p90) and colour56's guard (the world's S p99.5
# never over the tells' median S: n56-colour.py's satOk), estimated on two Player builds that differ ONLY in the lit gain: NIGHT60's n60-b4 (the band
# fix, gain 2.205) and n60-b5 (the band fix, gain 1.3), both rim56 on, the same reels and poses (<b4 root>/v1on-<level>/a357, <b5 root>/v1on-...).
# THE PLANTED FRAME for a gain g: the b5 ON frame with every pixel inside an enemy rect moved along the gain axis in LINEAR light,
# lin(g) = lin(b5) + t (lin(b4) - lin(b5)), t = (g - 1.3) / (2.205 - 1.3) (t < 0 extrapolates below 1.3), clipped to 0-1; measured by
# n56-colour.py's own measure(). Rim56's shade and near / far rim are the same on both builds, so only the gain moves. An estimate from two
# measured renders: uncalibrated (the post chain is not modelled; below 1.3 it extrapolates).
# PLANTED CHECK first: g 1.3 must give the b5 ON frames' own counts (the plumbing).
#   python -I Tools/night/n60-v-rimgain2.py <b4 root> <b5 root> [--gains 1.3,1.2,1.1,1.0]
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)
G4, G5 = 2.205, 1.3  # n60-b4's lit gain (Rim56LitGain x Rim57Gain) and n60-b5's (Rim60LitGain)


def lin(c):
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def srgb(c):
    c = np.clip(c, 0.0, 1.0)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * c ** (1 / 2.4) - 0.055)


def main():
    a = sys.argv[1:]
    r4, r5 = a[0], a[1]
    gains = [1.3, 1.2, 1.1, 1.0]
    if "--gains" in a:
        gains = [float(x) for x in a[a.index("--gains") + 1].split(",")]
    pal = M.load_palette(os.path.join(HERE, "..", "..", "Design", "unity-config.json"))
    tot = {g: [0, 0, 0, 0] for g in gains}  # bodies ok, in view, sat FAIL frames, frames
    own = [0, 0, 0]
    check_bad = 0
    for lvl in ["foundry", "iceline", "kiln", "span", "spire"]:
        d4, d5 = os.path.join(r4, "v1on-" + lvl, "a357"), os.path.join(r5, "v1on-" + lvl, "a357")
        if not (os.path.isdir(d4) and os.path.isdir(d5)):
            print("== %s: missing %s or %s" % (lvl, d4, d5))
            continue
        pe = (M.hex_hue(pal[lvl][0])[0], M.hex_hue(pal[lvl][1])[0]) if lvl in pal else None
        b4 = {}
        for d in M.scenario_dirs([d4]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for it in items:
                b4[os.path.basename(it[1])] = it[1]
        lv = {g: [0, 0, 0, 0] for g in gains}
        for d in M.scenario_dirs([d5]):
            items, _ = M.frame_list(d, ["entry", "mid"], False)
            for name, path, lvid, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
                b = os.path.basename(path)
                if b not in b4:
                    continue
                x5 = np.asarray(Image.open(path).convert("RGB"))
                x4 = np.asarray(Image.open(b4[b]).convert("RGB"))
                meta = M.meta_of(fr, x5.shape[0], extra)
                ref = M.measure(x5, meta, pe, True)
                own[0] += ref["enemiesBodyOk"]; own[1] += ref["enemiesInView"]; own[2] += 0 if ref["satOk"] else 1
                H, W = x5.shape[0], x5.shape[1]
                en = np.zeros((H, W), bool)
                for r in meta.get("enemies", []):
                    M.rect_mask(en, r)
                l5, l4 = lin(x5.astype(float) / 255.0), lin(x4.astype(float) / 255.0)
                line = "%s:" % b
                for g in gains:
                    t = (g - G5) / (G4 - G5)
                    pl = x5.astype(float) / 255.0
                    pl[en] = srgb(l5[en] + t * (l4[en] - l5[en]))
                    o = M.measure((pl * 255.0 + 0.5).astype(np.uint8), meta, pe, True)
                    if abs(g - G5) < 1e-9 and (o["enemiesBodyOk"] != ref["enemiesBodyOk"] or o["satOk"] != ref["satOk"]):
                        check_bad += 1
                    lv[g][0] += o["enemiesBodyOk"]; lv[g][1] += o["enemiesInView"]; lv[g][2] += 0 if o["satOk"] else 1; lv[g][3] += 1
                    line += " | g %.2f bodies %d / %d, world S p99.5 %.3f vs tells %.3f %s" % (g, o["enemiesBodyOk"], o["enemiesInView"], o["worldS995"], o["tellS"], "PASS" if o["satOk"] else "FAIL")
                print("  " + line)
        for g in gains:
            for k in range(4):
                tot[g][k] += lv[g][k]
        print("  %s: %s" % (lvl, "; ".join("g %.2f bodies %d / %d, sat FAIL %d / %d" % (g, lv[g][0], lv[g][1], lv[g][2], lv[g][3]) for g in gains)))
    print("PLANTED CHECK (g %.2f reproduces n60-b5 ON's own bodies and sat verdict on every shot): %s; n60-b5 ON itself: bodies %d / %d, sat FAIL %d frames" % (
        G5, "GREEN" if check_bad == 0 else "RED (%d shots differ)" % check_bad, own[0], own[1], own[2]))
    for g in gains:
        print("TOTAL g %.2f: bodies over env p90 %d / %d (%.1f %%); world over the tells (sat FAIL) %d / %d frames" % (
            g, tot[g][0], tot[g][1], 100.0 * tot[g][0] / max(1, tot[g][1]), tot[g][2], tot[g][3]))
    return 0 if check_bad == 0 else 1


if __name__ == "__main__":
    sys.exit(main())

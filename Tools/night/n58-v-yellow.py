# NIGHT58 builder V, slate V2 (yellow56): the non-parry yellow per frame, with the parry orb's FRINGE folded into the orb by a rule and a planted
# case (gate-r3 E4: spire v5-mid's 1.28 % "outside the orbs" is the orbs' own fringe: a meter artifact). n56-colour.py is not changed: this script
# imports its measure() pieces and adds, per frame:
#   - the orbs exactly as n56-colour.py finds them (bright V >= 0.85 yellow blobs >= 1500 px, >= 55 % of their box, aspect 0.4-2.5), then each
#     orb's FRINGE: the yellow pixels (the meter's yellow: 45-70 deg, S > 0.5, V >= 0.15) within FRINGE_PX (at 1080p) of the orb's blob, connected
#     to it through yellow (a glow ring round an emissive orb, never a strip across the room)
#   - "non-parry yellow" = the meter's yellow (hud and enemy / boss rects out) minus the orbs and their fringes, as a share of the frame (<= 0.5 %)
#   - the largest non-parry yellow components (px, box, H / S / V medians, the upper / lower half): what is left, named
#   python -I Tools/night/n58-v-yellow.py planted
#   python -I Tools/night/n58-v-yellow.py <dir> [<dir> ...] [--kinds entry,mid] [--all-jpg] [--top 3]
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)
from scipy import ndimage

FRINGE_PX = 24  # at 1080p: an orb's glow ring (n57-r3 spire v5-mid: the orbs' bloom halo, 10-20 px wide at 1080p); scaled by frame height
LIMIT = 0.5


SIM = None  # (sMax, vFull, vNone, hueLo, hueHi, feather): --sim applies yellow56's dim-yellow rule (Cap57.shader's, on HSV: S toward sMax, V kept) to the frame


def smooth(e0, e1, x):
    t = np.clip((x - e0) / max(1e-6, e1 - e0), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def masks(rgb, meta):
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, lum = M.hsv(rgb)
    if SIM is not None:
        smax, vf, vn, lo, hi, fe = SIM
        inarc = smooth(lo, lo + fe, h) * (1.0 - smooth(hi - fe, hi, h))
        keep = smooth(vf, vn, v)  # 0 = the cap whole, 1 = untouched
        cap = smax + (s - smax) * keep
        s = np.where((inarc > 0) & (s > cap), s - (s - cap) * inarc, s)
    hud = np.zeros((H, W), bool)
    for r in meta.get("hud", []):
        if r[2] * r[3] <= 0.25 * W * H:
            M.rect_mask(hud, r)
    en = np.zeros((H, W), bool)
    for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
        M.rect_mask(en, r)
    yel = ~hud & ~en & M.band(h, *M.YELLOW) & (s > 0.5) & (v >= 0.15)
    return yel, h, s, v


def orbs_and_fringe(yel, v):
    H = yel.shape[0]
    orb = np.zeros_like(yel)
    lab, nl = ndimage.label(yel & (v >= 0.85))
    n = 0
    if nl:
        sizes = ndimage.sum(np.ones_like(lab), lab, np.arange(1, nl + 1))
        for k, (sz, bx) in enumerate(zip(sizes, ndimage.find_objects(lab))):
            if bx is None or sz < 1500:
                continue
            bh, bw = bx[0].stop - bx[0].start, bx[1].stop - bx[1].start
            if sz >= 0.55 * bh * bw and 0.4 <= bw / max(1, bh) <= 2.5:
                orb |= lab == k + 1
                n += 1
    fringe = np.zeros_like(yel)
    if n:
        r = max(1, int(round(FRINGE_PX * H / 1080.0)))
        near = ndimage.distance_transform_edt(~orb) <= r  # a round ring (a cross-shaped dilation falls short on the diagonals)
        # connected to the orb through yellow inside the ring: label yellow within the ring, keep the components touching the orb
        ring = yel & near
        lab2, n2 = ndimage.label(ring)
        keep = np.unique(lab2[orb & ring])
        keep = keep[keep > 0]
        fringe = np.isin(lab2, keep) & ~orb
    return orb, fringe, n


def frame(rgb, meta, top=3):
    yel, h, s, v = masks(rgb, meta)
    orb, fringe, n = orbs_and_fringe(yel, v)
    H, W = yel.shape
    rest = yel & ~orb & ~fringe
    o = {"yellowPct": 100.0 * yel.sum() / (W * H), "orbs": n, "orbPct": 100.0 * orb.sum() / (W * H), "fringePct": 100.0 * fringe.sum() / (W * H),
         "nonParryPct": 100.0 * rest.sum() / (W * H)}
    lab, nl = ndimage.label(rest)
    comps = []
    if nl:
        sizes = ndimage.sum(np.ones_like(lab), lab, np.arange(1, nl + 1))
        for k in np.argsort(sizes)[::-1][:top]:
            bx = ndimage.find_objects(lab)[k]
            m = lab == k + 1
            comps.append("%d px at x %d-%d y %d-%d (H %.0f S %.2f V %.2f)" % (sizes[k], bx[1].start, bx[1].stop, bx[0].start, bx[0].stop,
                                                                           np.median(h[m]), np.median(s[m]), np.median(v[m])))
    o["comps"] = comps
    o["ok"] = o["nonParryPct"] <= LIMIT
    return o


def planted():
    W, H = 960, 540
    meta = {"hud": [], "enemies": [], "bosses": [], "gun": None, "horizon": None}
    base = np.zeros((H, W, 3), np.uint8) + np.array([40, 50, 60], np.uint8)
    yy, xx = np.mgrid[0:H, 0:W]
    cases = []
    # 1: an orb (bright yellow disc) with a dimmer yellow glow ring: everything is parry -> non-parry 0 %
    img = base.copy()
    d = np.hypot(xx - 300, yy - 270)
    img[d < 40] = [255, 220, 40]   # V 1.0, hue ~51
    img[(d >= 40) & (d < 50)] = [170, 140, 30]  # the fringe: V 0.67, S 0.82, hue ~49
    cases.append(("orb + its fringe ring -> non-parry 0", img, lambda o: o["orbs"] == 1 and o["nonParryPct"] < 0.01 and o["fringePct"] > 0.1))
    # 2: the same orb and a yellow STRIP far from it (a non-parry plate): the strip stays counted
    img2 = img.copy()
    img2[100:110, 600:900] = [200, 170, 30]
    cases.append(("orb + a far strip -> the strip counted", img2, lambda o: o["orbs"] == 1 and abs(o["nonParryPct"] - 100.0 * 3000 / (W * H)) < 0.05))
    # 3: a yellow strip touching nothing round, no orb: counted whole (the fringe rule never fires without an orb)
    img3 = base.copy()
    img3[300:320, 100:500] = [200, 170, 30]
    cases.append(("a strip alone -> counted, FAIL", img3, lambda o: o["orbs"] == 0 and o["nonParryPct"] > LIMIT and not o["ok"]))
    # 4: a strip that RUNS INTO the orb's ring: only its part within the ring is fringe, the rest counted
    img4 = img.copy()
    img4[268:273, 335:700] = [200, 170, 30]
    cases.append(("a strip into the ring -> its far part counted", img4, lambda o: o["orbs"] == 1 and o["nonParryPct"] > 0.2))
    bad = 0
    for name, im, want in cases:
        o = frame(im, meta)
        ok = want(o)
        bad += 0 if ok else 1
        print("planted %-46s orbs %d orb %.2f%% fringe %.2f%% non-parry %.2f%% -> %s" % (name, o["orbs"], o["orbPct"], o["fringePct"], o["nonParryPct"], "as planted" if ok else "WRONG"))
    print("PLANTED %s" % ("GREEN (4 / 4 as planted)" if bad == 0 else "RED (%d wrong)" % bad))
    return 0 if bad == 0 else 1


def main():
    args = sys.argv[1:]
    if args and args[0] == "planted":
        return planted()
    kinds, all_jpg, top = ["entry", "mid"], False, 3
    if "--kinds" in args:
        i = args.index("--kinds"); kinds = args[i + 1].split(","); del args[i:i + 2]
    if "--all-jpg" in args:
        args.remove("--all-jpg"); all_jpg = True
    if "--top" in args:
        i = args.index("--top"); top = int(args[i + 1]); del args[i:i + 2]
    if "--sim" in args:
        global SIM
        i = args.index("--sim"); SIM = tuple(float(x) for x in args[i + 1].split(",")); del args[i:i + 2]
        print("SIMULATED on the stills: the dim-yellow rule sMax %.2f, whole at V <= %.2f, none from %.2f, hue %.0f-%.0f (feather %.0f)" % SIM)
    if "--verbose" in args:
        args.remove("--verbose")
    n = over = 0
    worst = (0.0, "")
    for d in M.scenario_dirs(args):
        items, _ = M.frame_list(d, kinds, all_jpg)
        for name, path, lv, room, kind, fr, extra, how in sorted(items, key=lambda x: x[0]):
            rgb = np.asarray(Image.open(path).convert("RGB"))
            meta = M.meta_of(fr, rgb.shape[0], extra)
            o = frame(rgb, meta, top)
            n += 1
            over += 0 if o["ok"] else 1
            if o["nonParryPct"] > worst[0]:
                worst = (o["nonParryPct"], path)
            if not o["ok"] or o["orbs"] or "--verbose" in sys.argv:
                print("%s | yellow %.2f%% | orbs %d (%.2f%%) + fringe %.2f%% | NON-PARRY %.2f%% %s | left: %s" % (
                    os.path.relpath(path), o["yellowPct"], o["orbs"], o["orbPct"], o["fringePct"], o["nonParryPct"], "PASS" if o["ok"] else "FAIL", "; ".join(o["comps"])))
    print("NON-PARRY YELLOW: frames over %.1f %%: %d / %d (worst %.2f%% %s)" % (LIMIT, over, n, worst[0], os.path.relpath(worst[1]) if worst[1] else "-"))
    return 0


if __name__ == "__main__":
    sys.exit(main())

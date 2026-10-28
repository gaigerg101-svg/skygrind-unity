# NIGHT60 builder V (B3 b, yellow56 keeps the halo): THE NON-PARRY YELLOW METER with the enemies' tells folded in. n58-v-yellow.py stays as it
# is (the comparison); this script imports its masks and its orb + fringe rule and adds ONE rule:
#   - an ENEMY'S TELL: a 4-connected component of the meter's yellow (45-70 deg, S > 0.5, V >= 0.15; the hud out, the rects in) that touches
#     an enemy / boss rect (or its 1 px border) AND lies >= NEAR_SHARE (90 %) within TELL_PX (at 1080p, scaled by the frame's height) of a
#     rect: its part outside the rects is folded (the tell is drawn ON the enemy: a glow that spills past its rect). NIGHT60 B16: the share
#     bound is the fresh review's (Research/2026-10-21-wave60/review-b3-meter.md); without it, world yellow passing behind an enemy lost its
#     near part. The yellow inside a rect was never counted (n58's rule) and still is not.
#   - non-parry yellow = n58's (the meter's yellow, hud and enemy rects out, minus the orbs and their fringes) minus the enemies' tells.
# Why (NIGHT60 B3): foundry a5-entry's "parry halo" is an enemy whose rect the probe never reported (the swarmer / spitter views did not call
# FeelProbe.Enemy before NIGHT60 B3 a), and the halo's glow ring sits OUTSIDE any rect even when reported; n58 counted both as non-parry.
# Limits, by design (the planted cases pin them): a strip that runs out of a tell's ring makes ONE component with it, mostly beyond the ring,
# so nothing of it is folded (an over-count: it can fail the row, never pass it falsely); yellow near a rect but not connected to it is
# counted; world yellow behind an enemy is counted; frames shot before NIGHT60 B3 a have no swarmer / spitter rects, so this rule cannot fold
# their tells.
#   python -I Tools/night/n60-v-yellow.py planted
#   python -I Tools/night/n60-v-yellow.py <dir> [<dir> ...] [--kinds entry,mid] [--all-jpg] [--top 3] [--sim sMax,vFull,vNone,lo,hi,feather]
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image
from scipy import ndimage

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n58vyellow", os.path.join(HERE, "n58-v-yellow.py"))
Y = importlib.util.module_from_spec(spec)
spec.loader.exec_module(Y)
M = Y.M  # n56-colour.py

TELL_PX = Y.FRINGE_PX  # at 1080p: the orb fringe's radius (24 px) for an enemy's tell glow too ("like the orb fringe rule")
LIMIT = Y.LIMIT  # 0.5 % of the frame


def enemy_mask(meta, H, W):
    en = np.zeros((H, W), bool)
    for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
        M.rect_mask(en, r)
    return en


NEAR_SHARE = 0.9  # NIGHT60 B16 (the meter's fresh review, Research/2026-10-21-wave60/review-b3-meter.md): a component is a tell only when this
# share of its pixels lies within TELL_PX of a rect; a floor band or a plate that merely passes behind an enemy reaches far past the ring


def tells(yel_all, en):
    """The enemies' tells outside their rects. NIGHT60 B16 (the reviewer's bound): a 4-connected yellow component of the WHOLE frame (the
    rects in, the hud out) that touches a rect (or its 1 px border) is a tell only when >= NEAR_SHARE of its pixels lie within TELL_PX of a
    rect; then its part outside the rects is folded. (B3's first rule folded the ring part of ANY yellow touching a rect: a floor band or a
    tell-coloured plate behind an enemy lost its near part, enough to pass a frame that should fail.)"""
    if not en.any():
        return np.zeros_like(yel_all)
    H = yel_all.shape[0]
    r = max(1, int(round(TELL_PX * H / 1080.0)))
    near = ndimage.distance_transform_edt(~en) <= r  # a round ring round each rect (the rect itself at distance 0)
    touch = ndimage.binary_dilation(en)  # the rect and its 1 px border
    lab, n = ndimage.label(yel_all)
    if not n:
        return np.zeros_like(yel_all)
    ids = np.unique(lab[yel_all & touch])
    ids = ids[ids > 0]
    if not len(ids):
        return np.zeros_like(yel_all)
    total = ndimage.sum(np.ones_like(lab), lab, ids)
    inside = ndimage.sum(near, lab, ids)
    keep = ids[inside >= NEAR_SHARE * total]
    return np.isin(lab, keep) & ~en


def frame(rgb, meta, top=3):
    yel, h, s, v = Y.masks(rgb, meta)  # n58's yellow: hud and enemy rects out (its --sim honoured)
    yel_all, _, _, _ = Y.masks(rgb, dict(meta, enemies=[], bosses=[]))  # the same yellow with the enemy rects in (seeds and connectivity)
    H, W = yel.shape
    orb, fringe, n = Y.orbs_and_fringe(yel, v)
    tell = tells(yel_all, enemy_mask(meta, H, W)) & yel & ~orb & ~fringe
    rest = yel & ~orb & ~fringe & ~tell
    o = {"yellowPct": 100.0 * yel.sum() / (W * H), "orbs": n, "orbPct": 100.0 * orb.sum() / (W * H), "fringePct": 100.0 * fringe.sum() / (W * H),
         "tellPct": 100.0 * tell.sum() / (W * H), "nonParryPct": 100.0 * rest.sum() / (W * H)}
    lab, nl = ndimage.label(rest)
    comps = []
    if nl:
        sizes = ndimage.sum(np.ones_like(lab), lab, np.arange(1, nl + 1))
        objs = ndimage.find_objects(lab)
        for k in np.argsort(sizes)[::-1][:top]:
            bx = objs[k]
            m = lab == k + 1
            comps.append("%d px at x %d-%d y %d-%d (H %.0f S %.2f V %.2f)" % (sizes[k], bx[1].start, bx[1].stop, bx[0].start, bx[0].stop,
                                                                           np.median(h[m]), np.median(s[m]), np.median(v[m])))
    o["comps"] = comps
    o["ok"] = o["nonParryPct"] <= LIMIT
    return o


def planted():
    W, H = 960, 540
    empty = {"hud": [], "enemies": [], "bosses": [], "gun": None, "horizon": None}
    base = np.zeros((H, W, 3), np.uint8) + np.array([40, 50, 60], np.uint8)
    yy, xx = np.mgrid[0:H, 0:W]
    r = max(1, int(round(TELL_PX * H / 1080.0)))  # 12 px on this 540 p frame
    cases = []
    # n58-v-yellow.py's four, unchanged (no enemy rect: the tell rule never fires)
    img = base.copy()
    d = np.hypot(xx - 300, yy - 270)
    img[d < 40] = [255, 220, 40]
    img[(d >= 40) & (d < 50)] = [170, 140, 30]
    cases.append(("1 orb + its fringe ring -> non-parry 0", img, empty, lambda o: o["orbs"] == 1 and o["nonParryPct"] < 0.01 and o["fringePct"] > 0.1))
    img2 = img.copy()
    img2[100:110, 600:900] = [200, 170, 30]
    cases.append(("2 orb + a far strip -> the strip counted", img2, empty, lambda o: o["orbs"] == 1 and abs(o["nonParryPct"] - 100.0 * 3000 / (W * H)) < 0.05))
    img3 = base.copy()
    img3[300:320, 100:500] = [200, 170, 30]
    cases.append(("3 a strip alone -> counted, FAIL", img3, empty, lambda o: o["orbs"] == 0 and o["nonParryPct"] > LIMIT and not o["ok"]))
    img4 = img.copy()
    img4[268:273, 335:700] = [200, 170, 30]
    cases.append(("4 a strip into the ring -> its far part counted", img4, empty, lambda o: o["orbs"] == 1 and o["nonParryPct"] > 0.2))
    # NEW: an enemy (rect x 600-680, y 200-360) whose tell is a dim, very saturated yellow ring (a5-entry's halo: V ~0.45, S ~0.94) spilling
    # 6 px past the rect on each side (inside the 12 px ring)
    rect = [600, 200, 80, 160]
    en_meta = dict(empty, enemies=[rect])
    e = base.copy()
    e[((xx - 640) / 34.0) ** 2 + ((yy - 280) / 74.0) ** 2 <= 1] = [90, 20, 10]  # the body: dark red, not yellow
    ell_out = ((xx - 640) / 46.0) ** 2 + ((yy - 280) / 86.0) ** 2 <= 1
    ell_in = ((xx - 640) / 36.0) ** 2 + ((yy - 280) / 76.0) ** 2 <= 1
    e[ell_out & ~ell_in] = [115, 101, 7]  # the ring: H 52 S 0.94 V 0.45
    cases.append(("5 an enemy's dim saturated ring -> parry (non-parry 0)", e, en_meta, lambda o: o["tellPct"] > 0.05 and o["nonParryPct"] < 0.01))
    e6 = e.copy()
    e6[450:460, 100:400] = [200, 170, 30]
    cases.append(("6 the enemy + a far strip -> the strip counted, FAIL", e6, en_meta,
                  lambda o: abs(o["nonParryPct"] - 100.0 * 3000 / (W * H)) < 0.05 and not o["ok"]))
    e7 = e.copy()
    e7[278:283, 680:900] = [200, 170, 30]  # from the ring's right edge out across the floor
    # NIGHT60 B16: the ring and the strip are ONE component, most of it beyond the ring, so nothing is folded: the ring is counted with the
    # strip (an over-count: it can fail the row, never pass it falsely; the reviewer's rewrite of this case)
    cases.append(("7 a strip from the enemy's ring out -> nothing folded", e7, en_meta,
                  lambda o: o["tellPct"] == 0 and abs(o["nonParryPct"] - o["n58"]) < 0.01))
    e8 = base.copy()
    e8[((xx - 640) / 34.0) ** 2 + ((yy - 280) / 74.0) ** 2 <= 1] = [90, 20, 10]  # an enemy with no yellow at all
    e8[250:300, 686:720] = [200, 170, 30]  # a plate 6 px past the rect: within the ring, touching nothing yellow of the enemy's
    cases.append(("8 a plate near an enemy, not touching it -> counted", e8, en_meta,
                  lambda o: abs(o["nonParryPct"] - 100.0 * 50 * 34 / (W * H)) < 0.03 and o["tellPct"] == 0))
    # NIGHT60 B16, the reviewer's two cases: world yellow that passes BEHIND an enemy (through its rect) must stay counted and FAIL
    e9 = e8.copy()
    e9[:, :] = base[0, 0]
    e9[((xx - 640) / 34.0) ** 2 + ((yy - 280) / 74.0) ** 2 <= 1] = [90, 20, 10]
    e9[340:352, 520:860] = [200, 170, 30]  # a floor band through the rect's lower part
    cases.append(("9 a floor band behind an enemy -> not folded, FAIL", e9, en_meta, lambda o: o["tellPct"] == 0 and not o["ok"]))
    e10 = base.copy()
    e10[300:330, 560:760] = [115, 101, 7]  # a plate in the tell's own colour (H 52 S 0.94 V 0.45), half behind the rect
    e10[((xx - 640) / 34.0) ** 2 + ((yy - 280) / 74.0) ** 2 <= 1] = [90, 20, 10]
    cases.append(("10 a tell-coloured plate behind an enemy -> not folded, FAIL", e10, en_meta, lambda o: o["tellPct"] == 0 and not o["ok"]))
    bad = 0
    for name, im, meta, want in cases:
        old = Y.frame(im, meta)
        o = frame(im, meta)
        o["n58"] = old["nonParryPct"]
        ok = want(o)
        bad += 0 if ok else 1
        print("planted %-58s n58 non-parry %.2f%% -> n60 non-parry %.2f%% (orbs %d, fringe %.2f%%, tell %.2f%%) %s -> %s" % (
            name, old["nonParryPct"], o["nonParryPct"], o["orbs"], o["fringePct"], o["tellPct"], "PASS" if o["ok"] else "FAIL", "as planted" if ok else "WRONG"))
    print("PLANTED %s" % ("GREEN (%d / %d as planted)" % (len(cases), len(cases)) if bad == 0 else "RED (%d wrong)" % bad))
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
        i = args.index("--sim"); Y.SIM = tuple(float(x) for x in args[i + 1].split(",")); del args[i:i + 2]
        print("SIMULATED on the stills: the dim-yellow rule sMax %.2f, whole at V <= %.2f, none from %.2f, hue %.0f-%.0f (feather %.0f)" % Y.SIM)
    verbose = "--verbose" in args
    if verbose:
        args.remove("--verbose")
    n = over = over58 = 0
    worst = (0.0, "")
    for d in M.scenario_dirs(args):
        items, _ = M.frame_list(d, kinds, all_jpg)
        for name, path, lv, room, kind, fr, extra, how in sorted(items, key=lambda x: x[0]):
            rgb = np.asarray(Image.open(path).convert("RGB"))
            meta = M.meta_of(fr, rgb.shape[0], extra)
            o = frame(rgb, meta, top)
            o58 = Y.frame(rgb, meta, top)
            n += 1
            over += 0 if o["ok"] else 1
            over58 += 0 if o58["ok"] else 1
            if o["nonParryPct"] > worst[0]:
                worst = (o["nonParryPct"], path)
            if not o["ok"] or o["orbs"] or o["tellPct"] > 0 or verbose:
                print("%s | yellow %.2f%% | enemies %d | orbs %d (%.2f%%) + fringe %.2f%% | tell %.2f%% | NON-PARRY %.2f%% %s (n58 %.2f%%) | left: %s" % (
                    os.path.relpath(path), o["yellowPct"], len(meta.get("enemies", [])) + len(meta.get("bosses", [])), o["orbs"], o["orbPct"],
                    o["fringePct"], o["tellPct"], o["nonParryPct"], "PASS" if o["ok"] else "FAIL", o58["nonParryPct"], "; ".join(o["comps"])))
    print("NON-PARRY YELLOW (n60: the enemies' tells folded): frames over %.1f %%: %d / %d (n58's meter: %d / %d) (worst %.2f%% %s)" % (
        LIMIT, over, n, over58, n, worst[0], os.path.relpath(worst[1]) if worst[1] else "-"))
    return 0


if __name__ == "__main__":
    sys.exit(main())

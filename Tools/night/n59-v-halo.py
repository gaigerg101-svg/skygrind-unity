# NIGHT59 builder V, slate V2 (yellow56's halo): THE HALO METER, the GUARD "the parry halo keeps >= 80 % of its yellow pixels in the ON run"
# (NIGHT59.md section 3 V2). gate-x.md's method (its halo3.py is not in the repo; this reproduces its numbers on the same frames): a frame and its
# "-nocap" twin shot at the same instant (FeelReel.N57.Colour.cs: Cap57.Hide for one frame, the world cap AND yellow56's dim yellow hidden), so
# the pose and the game state match and only the cap differs.
#   - yellow = the meter's yellow (n56-colour.py: hue 45-70, S > 0.5, V >= 0.15), nothing masked (the halo sits on a husk: its enemy rect is the point)
#   - the halo = the LARGEST 4-connected yellow component inside the box (default x 560-650, y 540-700 at 1080p: foundry a5-entry's left husk;
#     scaled by the frame's height) in the TWIN (no cap)
#   - kept = the main frame's yellow pixels inside that halo; kept share = kept / the halo's pixels. GUARD: >= 80 % (NIGHT58 ON 20 %, OFF 91 %)
#   python -I Tools/night/n59-v-halo.py planted
#   python -I Tools/night/n59-v-halo.py <main.png> [<twin.png>] [--box x0,x1,y0,y1] [--sim] [--skeep S] [--config C]
#   python -I Tools/night/n59-v-halo.py <dir> [...]   (every shot-*foundry-a5-entry.png under the dirs, with its -nocap twin)
# --sim: ALSO apply the dim-yellow rule as the shader runs it (n57-v-cap.py's cap_rgb, the numbers Cap57.ForMap reads: n59-v-yellow.py's
# yellow_args, plus the foundry's world cap) to the twin, with the configured rule and with NIGHT58's (no sKeep), and print the kept share of each:
# an estimate on the final PNG (the shader runs on the main camera's colour before the viewmodel and the HUD), never the Player's number.
import sys, os, glob, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image
from scipy import ndimage

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))


def _load(name, file):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, file))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


Y = _load("n59vyellow", "n59-v-yellow.py")
C = Y.C  # n57-v-cap.py
M = Y.M  # n56-colour.py
BOX = (560, 650, 540, 700)  # x0, x1, y0, y1 at 1080p: foundry a5-entry's left husk's parry halo (gate-x.md)
GUARD = 80.0


def yellow(rgb):
    h, s, v, _ = M.hsv(rgb)
    return M.band(h, *M.YELLOW) & (s > 0.5) & (v >= 0.15)


def halo_kept(main, twin, box=BOX):
    """(kept px, halo px, kept %): the twin's largest 4-connected yellow component in the box, and the main frame's yellow inside it."""
    H = twin.shape[0]
    k = H / 1080.0
    x0, x1, y0, y1 = (int(round(b * k)) for b in box)
    yt = yellow(twin)
    inbox = np.zeros_like(yt)
    inbox[y0:y1, x0:x1] = True
    lab, n = ndimage.label(yt & inbox)
    if not n:
        return 0, 0, 0.0
    sizes = ndimage.sum(np.ones_like(lab), lab, np.arange(1, n + 1))
    halo = lab == int(np.argmax(sizes)) + 1
    kept = int((yellow(main) & halo).sum())
    tot = int(halo.sum())
    return kept, tot, 100.0 * kept / max(1, tot)


def sim_main(twin, cfg, skeep=None, off59=False):
    lv, role, f = C.load_cap(cfg)
    y = Y.yellow_args(cfg, off59=off59)
    if skeep is not None and not off59:
        y = y[:6] + (float(skeep),) + tuple(y[7:8])
    out, _ = C.cap_rgb(twin, lv.get("foundry", 1.0), role, f, None, y)
    return out, y


def report(main_p, twin_p, box, sim, skeep, cfg):
    main = np.asarray(Image.open(main_p).convert("RGB"))
    twin = np.asarray(Image.open(twin_p).convert("RGB"))
    kept, tot, pct = halo_kept(main, twin, box)
    print("%s | the halo (twin's largest yellow in x %d-%d y %d-%d) %d px | main keeps %d px = %.1f %% | GUARD >= %.0f %%: %s"
          % (os.path.relpath(main_p), box[0], box[1], box[2], box[3], tot, kept, pct, GUARD, "PASS" if pct >= GUARD else "FAIL"))
    if sim:
        for label, off in (("NIGHT58's rule (no sKeep)", True), ("the configured rule", False)):
            sm, y = sim_main(twin, cfg, skeep, off)
            k2, t2, p2 = halo_kept(sm, twin, box)
            print("   SIMULATED on the twin, %s %s: keeps %d / %d px = %.1f %%" % (label, "sKeep %.2f" % y[6] if len(y) > 6 else "(no keep bound)", k2, t2, p2))
    return pct


def planted():
    bad = 0
    lines = []
    for W, H in ((1920, 1080), (960, 540)):
        k = H / 1080.0
        twin = np.zeros((H, W, 3), np.uint8) + np.array([40, 44, 52], np.uint8)
        yy, xx = np.mgrid[0:H, 0:W]
        d = np.hypot(xx - 605 * k, yy - 620 * k)
        ring = (d >= 25 * k) & (d < 38 * k)
        twin[ring] = M.hsv_px(50, 0.99, 0.84)  # the halo
        twin[(np.abs(xx - 640 * k) < 4 * k) & (np.abs(yy - 560 * k) < 4 * k)] = M.hsv_px(52, 0.9, 0.9)  # a small separate yellow in the box: not the halo
        twin[int(300 * k):int(310 * k), int(100 * k):int(500 * k)] = M.hsv_px(49, 0.64, 0.81)  # a strip outside the box: never counted
        n_ring = int(ring.sum())
        same = twin.copy()
        half = twin.copy()
        half[ring & (xx < 605 * k)] = M.hsv_px(50, 0.3, 0.84)  # the left half of the halo capped
        gone = twin.copy()
        gone[ring] = M.hsv_px(50, 0.3, 0.84)
        want_half = 100.0 * int((ring & (xx >= 605 * k)).sum()) / n_ring
        for name, main, want, guard in (("main = twin -> 100 %, PASS", same, 100.0, True),
                                        ("the halo's left half capped -> ~50 %, FAIL", half, want_half, False),
                                        ("the halo capped whole -> 0 %, FAIL", gone, 0.0, False)):
            kept, tot, pct = halo_kept(main, twin)
            ok = tot == n_ring and abs(pct - want) < 0.5 and (pct >= GUARD) == guard
            bad += 0 if ok else 1
            lines.append("planted %dx%d %-44s halo %d px (ring %d) kept %d = %.1f %% (want %.1f) -> %s"
                         % (W, H, name, tot, n_ring, kept, pct, want, "as planted" if ok else "WRONG"))
    for l in lines:
        print(l)
    print("PLANTED %s" % (("GREEN (%d / %d as planted)" % (len(lines), len(lines))) if bad == 0 else ("RED (%d of %d wrong)" % (bad, len(lines)))))
    return 0 if bad == 0 else 1


def main():
    args = sys.argv[1:]
    if args and args[0] == "planted":
        return planted()
    cfg = os.path.join(ROOT, "Design", "unity-config.json")
    box, sim, skeep = BOX, False, None
    if "--config" in args:
        i = args.index("--config"); cfg = args[i + 1]; del args[i:i + 2]
    if "--box" in args:
        i = args.index("--box"); box = tuple(int(x) for x in args[i + 1].split(",")); del args[i:i + 2]
    if "--skeep" in args:
        i = args.index("--skeep"); skeep = float(args[i + 1]); del args[i:i + 2]
    if "--sim" in args:
        args.remove("--sim"); sim = True
    if not args:
        print("usage: n59-v-halo.py planted | <main.png> [<twin.png>] | <dir> [...] [--box x0,x1,y0,y1] [--sim] [--skeep S] [--config C]")
        return 2
    pairs = []
    if args[0].lower().endswith(".png"):
        main_p = args[0]
        twin_p = args[1] if len(args) > 1 else main_p[:-4] + "-nocap.png"
        pairs.append((main_p, twin_p))
    else:
        for d in args:
            for p in sorted(glob.glob(os.path.join(d, "**", "shot-*foundry-a5-entry.png"), recursive=True)):
                pairs.append((p, p[:-4] + "-nocap.png"))
    n = fails = 0
    for main_p, twin_p in pairs:
        if not os.path.exists(twin_p):
            print("%s: no -nocap twin (%s): not measured" % (os.path.relpath(main_p), os.path.relpath(twin_p)))
            continue
        pct = report(main_p, twin_p, box, sim, skeep, cfg)
        n += 1
        fails += 0 if pct >= GUARD else 1
    print("HALO KEPT >= %.0f %%: %d / %d frame pairs pass" % (GUARD, n - fails, n))
    return 0


if __name__ == "__main__":
    sys.exit(main())

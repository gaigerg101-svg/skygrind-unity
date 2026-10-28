# NIGHT60 builder V (B8, the exit lamp; design.n60.lamp60): THE LAMP METER on a157's clear shots (FeelReel.N60.Lamp.cs, -skygrindClearShots60):
# shot-N29-L57-<level>-<room>-clear.png + its sidecar .json (the door's "box": the opening grown by the lamp frame, image px [x, y, w, h]).
#   MAIN  (per room, the ON side): the exit door's luma (the mean luma inside the box) / the room's median luma (the frame outside the box) >= 1.5
#   GUARD (per room, ON vs OFF at the same pose): the p95 luma outside the box within TOL of the off side (the lamp lights the door, not the room)
# Luma: Rec. 709 on the frame's stored sRGB values (as Shot29's own measures). TOL: the planted cases' bound (a lamp painted only inside the box
# moves the p95 outside by 0.000; a room-wide x1.10 spill moves it by more than TOL; the point light's local spill on the floor near the door,
# planted, stays under it).
#   python -I Tools/night/n60-v-lamp.py <on a157 dir> <off a157 dir>     (lamp60 on vs -skygrindNoExit58, the same build and poses)
#   python -I Tools/night/n60-v-lamp.py planted [<a157 dir with n57-kiln entry frames>]   (synthetic cases, and the lamp planted on real frames)
import sys, os, json, glob
import numpy as np
from PIL import Image

RATIO = 1.5  # the MAIN's bound (NIGHT60-BACKLOG.md B8)
TOL = 0.02  # the GUARD's bound on the p95 luma outside the box (0-1 luma), set by the planted cases below
LAMP = (0.0, 0.9, 1.0)  # the lamp on screen in the planted cases: colors.usable cyan x Exit58Glow 3.2 (HDR, additive) clips to a bright cyan
BAND = 0.37 / 4.74  # the lamp frame's share of the box's width: (Exit58FrameOut 0.15 + FrameW 0.22) / a 4 m door grown by both on each side


def luma(rgb):
    a = rgb.astype(float) / 255.0 if rgb.dtype == np.uint8 else rgb
    return 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]


def box_mask(shape, box):
    H, W = shape[0], shape[1]
    m = np.zeros((H, W), bool)
    x, y, w, h = box
    x0, y0 = max(0, int(round(x))), max(0, int(round(y)))
    x1, y1 = min(W, int(round(x + w))), min(H, int(round(y + h)))
    if x1 > x0 and y1 > y0:
        m[y0:y1, x0:x1] = True
    return m


def measure(rgb, box):
    L = luma(rgb)
    m = box_mask(L.shape, box)
    if m.sum() < 50 or (~m).sum() < 50:
        return None
    door, room = float(L[m].mean()), float(np.median(L[~m]))
    return {"door": door, "room": room, "ratio": door / max(room, 1e-4), "p95": float(np.percentile(L[~m], 95)), "px": int(m.sum())}


def paint_lamp(rgb, box, band=BAND):
    """The lamp as the frame shows it: the box's two posts and its lintel (a band of `band` x the box's width) in LAMP."""
    out = rgb.astype(float) / 255.0 if rgb.dtype == np.uint8 else rgb.copy()
    x, y, w, h = box
    bw = max(1, int(round(w * band)))
    x0, y0, x1, y1 = int(round(x)), int(round(y)), int(round(x + w)), int(round(y + h))
    out[y0:y1, x0:x0 + bw] = LAMP
    out[y0:y1, x1 - bw:x1] = LAMP
    out[y0:y0 + bw, x0:x1] = LAMP
    return (np.clip(out, 0, 1) * 255.0 + 0.5).astype(np.uint8)


def verdict(on, off):
    main = on["ratio"] >= RATIO
    guard = abs(on["p95"] - off["p95"]) <= TOL
    return main, guard


def own_gain(on, off):
    # FORGE62b (the 2-hour NIGHT62's red team, finding 6): NIGHT62 P1's MAIN is the lamp's OWN gain on the door, door luma on / off >= RATIO in
    # >= 4 of n57-kiln's 5 rooms (slate-seed-2h.json P1), not lamp60's door / room ratio above (the red already passes that one 4 / 5)
    return on["door"] / max(off["door"], 1e-4)


def run(on_dir, off_dir):
    n = ok_main = ok_guard = ok_own = 0
    shots, skipped = 0, []  # NIGHT63 (the fresh review's S6): every clear shot found is counted, and each skip is named in the summary
    for p in sorted(glob.glob(os.path.join(on_dir, "**", "shot-N29-*-clear.png"), recursive=True)):
        shots += 1
        side = json.load(open(p[:-4] + ".json", encoding="utf-8"))
        q = os.path.join(off_dir, os.path.relpath(p, on_dir))
        name = os.path.basename(p)
        if side.get("box") is None:
            print("%s | the door is not on the screen (box null): not measured" % name)
            skipped.append(name + " (box null)")
            continue
        # NIGHT63 (the lead; T-146's fourth item, the w60 review's S5): the sidecar's `cleared` was written (FeelReel.N60.Lamp.cs) but never
        # read, so an uncleared room's frame (enemies alive) could be scored as a clear's lamp; such a shot is now skipped and counted as one
        if side.get("cleared") is not True:
            print("%s | not a clear (cleared %s, alive %s): not measured" % (name, side.get("cleared"), side.get("alive")))
            skipped.append(name + " (not a clear on)")
            continue
        if not os.path.exists(q):
            print("%s | no off frame at %s" % (name, q))
            skipped.append(name + " (no off frame)")
            continue
        # NIGHT63 (S6): the OFF side's own sidecar, when it has one (a two-run pair), must be a clear too; a same-run held OFF frame has none
        qs = q[:-4] + ".json"
        if os.path.exists(qs):
            off_side = json.load(open(qs, encoding="utf-8"))
            if off_side.get("cleared") is not True:
                print("%s | the OFF shot is not a clear (cleared %s): not measured" % (name, off_side.get("cleared")))
                skipped.append(name + " (not a clear off)")
                continue
        a = np.asarray(Image.open(p).convert("RGB"))
        b = np.asarray(Image.open(q).convert("RGB"))
        mon, moff = measure(a, side["box"]), measure(b, side["box"])
        if mon is None or moff is None:
            print("%s | the box is under 50 px or the whole frame: not measured" % name)
            continue
        main, guard = verdict(mon, moff)
        n += 1
        ok_main += 1 if main else 0
        ok_guard += 1 if guard else 0
        ok_own += 1 if own_gain(mon, moff) >= RATIO else 0
        # the OFF ratio beside it: an open door onto a bright room can pass 1.5x with no lamp (the planted real frames: 0.95-1.91x unpainted)
        print("%s | door %s opened %s | ON door luma %.3f / room median %.3f = %.2fx %s | OFF %.2fx (the lamp's own gain: door luma on / off %.2fx) | p95 outside on %.3f off %.3f (|d| %.3f, TOL %.2f) %s | lamp: %s" % (
            name, side.get("door"), side.get("opened"), mon["door"], mon["room"], mon["ratio"], "PASS" if main else "FAIL", moff["ratio"], mon["door"] / max(moff["door"], 1e-4),
            mon["p95"], moff["p95"], abs(mon["p95"] - moff["p95"]), TOL, "PASS" if guard else "FAIL", side.get("lamp", "")))
    print("LAMP: MAIN (door / room median >= %.1fx) %d / %d rooms; GUARD (p95 outside within %.2f of off) %d / %d" % (RATIO, ok_main, n, TOL, ok_guard, n))
    print("OWN GAIN (door luma on / off >= %.1fx: NIGHT62 P1's MAIN, >= 4 of 5 rooms) %d / %d rooms" % (RATIO, ok_own, n))
    print("SHOTS: %d clear shots found, %d measured, %d not measured%s" % (shots, n, len(skipped), (": " + "; ".join(skipped)) if skipped else ""))
    return 0


def planted(real_dir=None):
    bad = 0
    rng = np.random.default_rng(60)
    H, W = 540, 960
    yy, xx = np.mgrid[0:H, 0:W]
    room = np.stack([0.10 + 0.20 * yy / H, 0.12 + 0.15 * xx / W, 0.16 + 0.05 * yy / H], -1) + rng.normal(0, 0.01, (H, W, 3))
    off = (np.clip(room, 0, 1) * 255.0 + 0.5).astype(np.uint8)
    box = [420, 200, 120, 160]
    on = paint_lamp(off, box)
    spill = (np.clip(off.astype(float) / 255.0 * 1.10, 0, 1) * 255.0 + 0.5).astype(np.uint8)
    spill[box_mask(spill.shape, box)] = on[box_mask(on.shape, box)]
    local = on.astype(float) / 255.0
    near = (np.hypot(xx - 480, yy - 380) < 90) & ~box_mask((H, W), box)  # the point light's pool on the floor in front of the door
    local[near] = np.clip(local[near] + 0.08 * np.array(LAMP), 0, 1)
    local = (local * 255.0 + 0.5).astype(np.uint8)
    dim = paint_lamp(off, box)
    dimf = off.astype(float) / 255.0
    m = box_mask((H, W), box)
    band = m & ~box_mask((H, W), [box[0] + box[2] * BAND, box[1] + box[2] * BAND, box[2] * (1 - 2 * BAND), box[3]])
    dimf[band] = dimf[band] * 1.2
    dim = (np.clip(dimf, 0, 1) * 255.0 + 0.5).astype(np.uint8)
    cases = [
        ("1 the lamp lit in the box -> MAIN PASS, GUARD PASS", on, off, (True, True)),
        ("2 no lamp (the off frame as on) -> MAIN FAIL", off, off, (False, True)),
        ("3 the lamp + a room-wide x1.10 spill -> GUARD FAIL", spill, off, (None, False)),
        ("4 the lamp + its point light's pool on the floor -> GUARD PASS", local, off, (True, True)),
        ("5 a lamp at 1.2x the wall, not the cyan -> MAIN FAIL", dim, off, (False, True)),
    ]
    for name, a, b, want in cases:
        mon, moff = measure(a, box), measure(b, box)
        got = verdict(mon, moff)
        ok = (want[0] is None or got[0] == want[0]) and got[1] == want[1]  # None: the MAIN not judged by this case
        bad += 0 if ok else 1
        print("planted %-62s ratio %.2fx (off %.2fx), p95 outside |d| %.3f -> MAIN %s GUARD %s -> %s" % (
            name, mon["ratio"], moff["ratio"], abs(mon["p95"] - moff["p95"]), "PASS" if got[0] else "FAIL", "PASS" if got[1] else "FAIL", "as planted" if ok else "WRONG"))
    # FORGE62b: the own gain (NIGHT62 P1's MAIN): the lamp painted in the box must reach >= RATIO over the same frame unlit; the unlit frame against
    # itself must not; a room-wide x1.10 spill with no lamp must not either (the door brightens only 1.10x)
    nolamp = (np.clip(off.astype(float) / 255.0 * 1.10, 0, 1) * 255.0 + 0.5).astype(np.uint8)
    for name, a, b, want in [("6 own gain: the lamp lit vs the same frame unlit -> PASS", on, off, True),
                             ("7 own gain: the unlit frame vs itself -> FAIL", off, off, False),
                             ("8 own gain: a room-wide x1.10 spill, no lamp -> FAIL", nolamp, off, False)]:
        g = own_gain(measure(a, box), measure(b, box))
        ok = (g >= RATIO) == want
        bad += 0 if ok else 1
        print("planted %-62s own gain %.2fx -> %s -> %s" % (name, g, "PASS" if g >= RATIO else "FAIL", "as planted" if ok else "WRONG"))
    if real_dir:
        # on real n57-kiln a157 entry frames: a door-sized box at the frame's centre (a 4 m door at ~12 m: ~130 x 150 px at 1080p), the lamp painted
        for p in sorted(glob.glob(os.path.join(real_dir, "**", "shot-N29-L57-n57-kiln-*-entry.png"), recursive=True)):
            a = np.asarray(Image.open(p).convert("RGB"))
            hh, ww = a.shape[0], a.shape[1]
            bx = [ww / 2 - 65, hh / 2 - 90, 130, 150]
            lit = paint_lamp(a, bx)
            mon, moff = measure(lit, bx), measure(a, bx)
            got = verdict(mon, moff)
            print("planted real %-48s lamp painted: ratio %.2fx (unpainted %.2fx), p95 outside |d| %.3f -> MAIN %s GUARD %s" % (
                os.path.basename(p), mon["ratio"], moff["ratio"], abs(mon["p95"] - moff["p95"]), "PASS" if got[0] else "FAIL", "PASS" if got[1] else "FAIL"))
            bad += 0 if got[1] else 1
    print("PLANTED %s" % ("GREEN" if bad == 0 else "RED (%d wrong)" % bad))
    return 0 if bad == 0 else 1


def main():
    a = sys.argv[1:]
    if a and a[0] == "planted":
        return planted(a[1] if len(a) > 1 else None)
    if len(a) != 2:
        print("usage: n60-v-lamp.py <on a157 dir> <off a157 dir> | planted [<a157 dir>]")
        return 2
    return run(a[0], a[1])


if __name__ == "__main__":
    sys.exit(main())

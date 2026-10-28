# NIGHT60 builder V (B24 = audit A-V1): the PULSE gauntlet's HUD pips (UI/HUD 33/Weapon 52/Rail 34/PULSE pip 0-4, #FF6FB0) as the frame shows them,
# against n56-colour.py's role arcs (magenta 285-330 = RIVAL; danger 5-35, and the role mask's danger family 350-35). For each sidecar'd shot
# (shot-N56-*.png + .json with the telemetry frame "f"; FeelProbe's ui[] gives each pip's rect [x, y, w, h] from the top-left, its alpha and
# colour on that frame): the pips' pixels' H / S / V (median, p10-p90) and the share inside each arc, with n56-colour.py's own hsv() and band().
# PLANTED, on a copy of each frame: (a) the pip rects painted solid at RIVAL's magenta (UnityTuning.Look49RivalHex 0xFF2BD6) must land inside
# the magenta arc with the same code; (b) RIVAL's magenta laid at the pips' own alpha over the frame's pixels (the dim pip a RIVAL pip would be).
#   python -I Tools/night/n60-v-pips.py <shot.png> [<shot.png> ...]
import sys, os, json, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)
RIVAL = np.array([0xFF, 0x2B, 0xD6], float)  # UnityTuning.Look49RivalHex


def pips_of(png):
    side = json.load(open(png[:-4] + ".json", encoding="utf-8"))
    tel = os.path.join(os.path.dirname(png), "telemetry.jsonl")
    for line in open(tel, encoding="utf-8"):
        try:
            o = json.loads(line)
        except ValueError:
            continue
        if o.get("f") == side.get("f"):
            return [e for e in o.get("ui") or [] if "PULSE pip" in (e.get("path") or "") and (e.get("alpha") or 0) > 0.05]
    return []


def mask_of(shape, pips):
    m = np.zeros(shape[:2], bool)
    for p in pips:
        M.rect_mask(m, p["rect"])
    return m


def stats(rgb, m):
    h, s, v, _ = M.hsv(rgb)
    hm, sm, vm = h[m], s[m], v[m]
    mag = M.band(hm, *M.MAGENTA)
    dan = M.band(hm, *M.DANGER)
    fam = M.band(hm, 350.0, 360.0) | M.band(hm, 0.0, 35.0)
    return {"n": int(m.sum()), "h": np.percentile(hm, [10, 50, 90]), "s": np.percentile(sm, [10, 50, 90]), "v": np.percentile(vm, [10, 50, 90]),
            "mag": 100.0 * mag.mean(), "dan": 100.0 * dan.mean(), "fam": 100.0 * fam.mean()}


def line(tag, st):
    return "%s: %d px | H p10 / median / p90 %.0f / %.0f / %.0f | S %.2f / %.2f / %.2f | V %.2f / %.2f / %.2f | in magenta (285-330) %.0f %%, danger (5-35) %.0f %%, danger family (350-35) %.0f %%" % (
        tag, st["n"], st["h"][0], st["h"][1], st["h"][2], st["s"][0], st["s"][1], st["s"][2], st["v"][0], st["v"][1], st["v"][2], st["mag"], st["dan"], st["fam"])


def main():
    bad = 0
    allpx = []
    for png in sys.argv[1:]:
        pips = pips_of(png)
        if not pips:
            print("%s: no PULSE pip visible on its telemetry frame" % png)
            continue
        rgb = np.asarray(Image.open(png).convert("RGB"))
        m = mask_of(rgb.shape, pips)
        st = stats(rgb, m)
        allpx.append(rgb[m])
        print("== %s (%d pips, alpha %s, colour %s, rects %s)" % (png, len(pips), sorted({p["alpha"] for p in pips}), sorted({p["color"] for p in pips}), [p["rect"] for p in pips][:2]))
        print("  " + line("the pips as shown", st))
        solid = rgb.copy()
        solid[m] = RIVAL.astype(np.uint8)
        sa = stats(solid, m)
        ok = sa["mag"] >= 95.0
        bad += 0 if ok else 1
        print("  " + line("PLANTED (a) RIVAL #FF2BD6 solid", sa) + " -> %s" % ("inside the magenta arc, as planted" if ok else "WRONG"))
        a = float(np.median([p["alpha"] for p in pips]))
        blend = rgb.astype(float)
        blend[m] = blend[m] * (1 - a) + RIVAL * a
        print("  " + line("PLANTED (b) RIVAL at the pips' alpha %.2f over the frame" % a, stats(blend.astype(np.uint8), m)))
    if allpx:
        px = np.concatenate(allpx)[None, :, :]
        print(line("ALL FRAMES' PIPS", stats(px, np.ones(px.shape[:2], bool))))
    print("PLANTED %s" % ("GREEN" if bad == 0 else "RED (%d frames)" % bad))
    return 0 if bad == 0 else 1


if __name__ == "__main__":
    sys.exit(main())

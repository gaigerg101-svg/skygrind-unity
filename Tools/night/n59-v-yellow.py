# NIGHT59 builder V, slate V2 (yellow56's halo): the planted case for the dim-yellow rule's SATURATION split, before any fix (NIGHT59.md
# section 3 V2; gate-x.md: the foundry a5 parry halo keeps 20 % of its yellow with yellow56 on, because its V 0.84 sits on
# dimYellow58.vFull 0.84, "at or under which the cap is whole"). Value cannot split the halo from the lava edge (V 0.81) and the plate (V 0.82);
# saturation can (the halo S 0.99, the lava edge 0.64, the plate 0.56). n58-v-yellow.py (the MAIN meter) is not changed.
# The rule is run as the shader runs it: Tools/night/n57-v-cap.py's cap_rgb (Cap57.shader's Frag on the sRGB value), with the numbers read
# from Design/unity-config.json the way Cap57.ForMap reads them (dimYellow's sMax / hue arc, dimYellow58's vFull / vNone unless
# -skygrindOff58, dimYellow58.sKeep unless -skygrindOff59 or absent) and the foundry's world cap (colour56.numbers.cap.levels.foundry).
#   python -I Tools/night/n59-v-yellow.py planted [--config Design/unity-config.json]
# The planted pixels (H / S / V as named in gate-x.md and Research/2026-10-21-wave58/V.md):
#   - the husk's parry halo   H 48, S 0.99, V 0.84: must STAY yellow by the meter (S > 0.5)
#   - the lava trench's edge  H 49, S 0.64, V 0.81: must be CAPPED (S <= 0.5)
#   - the column top plate    H 48, S 0.56, V 0.82: must be CAPPED
#   - an orb's emissive core  H 50, S 0.99, V 0.95: untouched (the control: over vNone)
#   - the off side (-skygrindOff59: no sKeep): the halo capped, as NIGHT58 shipped it (the control the fix must not change)
# plus two source checks: Cap57.shader's dim-yellow block and Cap57Math (its C# mirror) both test the keep bound (_Cap57YH.w / y[6]).
import sys, os, json, importlib.util
sys.dont_write_bytecode = True
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))


def _load(name, file):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, file))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


C = _load("n57vcap", "n57-v-cap.py")
M = C.M  # n56-colour.py, as n57-v-cap.py loads it


def yellow_args(cfg_path, off58=False, off59=False):
    """Cap57.ForMap's yellow numbers: (sMax, vFull, vNone, hueLo, hueHi, featherDeg[, sKeep]) or None when there are none."""
    with open(cfg_path, encoding="utf-8") as fh:
        y = json.load(fh)["design"]["n56"]["yellow56"]["numbers"]
    d = y["dimYellow"]
    vf, vn = float(d["vFull"]), float(d["vNone"])
    d58 = y.get("dimYellow58") or {}
    if not off58 and "vFull" in d58 and "vNone" in d58:
        vf, vn = float(d58["vFull"]), float(d58["vNone"])
    a = (float(d["sMax"]), vf, vn, float(d["hueLo"]), float(d["hueHi"]), float(d["featherDeg"]))
    if not off58 and not off59 and isinstance(d58.get("sKeep"), (int, float)):
        a = a + (float(d58["sKeep"]), float(d58.get("sKeepVMin", 0.0)))
    return a


def run(cfg_path):
    lv, role, f = C.load_cap(cfg_path)
    cap = lv.get("foundry", 1.0)
    px = [("the husk's parry halo", 48, 0.99, 0.84, "yellow"),
          ("the lava trench's edge", 49, 0.64, 0.81, "capped"),
          ("the column top plate", 48, 0.56, 0.82, "capped"),
          ("an orb's emissive core", 50, 0.99, 0.95, "untouched"),
          ("spire's near-black olive", 52, 1.0, 0.19, "capped")]  # added with the fix: the keep bound alone let it through (sim)
    img = np.zeros((4, 8 * len(px), 3), np.uint8)
    for i, p in enumerate(px):
        img[:, 8 * i:8 * i + 8] = M.hsv_px(p[1], p[2], p[3])
    h0, s0, v0, _ = M.hsv(img)
    bad = 0
    lines = []
    for side, y in (("as configured", yellow_args(cfg_path)), ("-skygrindOff59 (no sKeep)", yellow_args(cfg_path, off59=True))):
        out, moved = C.cap_rgb(img, cap, role, f, None, y)
        h1, s1, v1, _ = M.hsv(out)
        keep = y[6] if len(y) > 6 else None
        lines.append("rule %s: sMax %.2f, whole at V <= %.2f, none from %.2f, hue %.0f-%.0f (feather %.0f), sKeep %s; foundry world cap %.2f"
                     % (side, y[0], y[1], y[2], y[3], y[4], y[5], ("%.2f at V >= %.2f" % (keep, y[7])) if keep is not None else "none", cap))
        for i, (nm, H, S, V, want) in enumerate(px):
            x = 8 * i + 4
            sa, sb = float(s0[0, x]), float(s1[0, x])
            if side.startswith("-skygrindOff59"):
                want = "capped" if nm.startswith("the husk") else want  # NIGHT58's rule: the halo is capped (gate-x.md's 20 %)
            if want == "yellow":
                ok = sb > 0.5 and float(h1[0, x]) >= 45 and float(v1[0, x]) >= 0.15
            elif want == "capped":
                ok = sb <= 0.5
            else:
                ok = not bool(moved[0, x])
            bad += 0 if ok else 1
            lines.append("planted %-26s H %.0f S %.2f V %.2f -> S %.2f (%s) | want %-9s -> %s"
                         % (nm, H, sa, float(v0[0, x]), sb, "moved" if moved[0, x] else "untouched", want, "as planted" if ok else "WRONG"))
    with open(os.path.join(ROOT, "Assets", "Shaders", "Resources", "Cap57.shader"), encoding="utf-8") as fh:
        sh = fh.read()
    blk = sh[sh.find("if (yOn"):]
    blk = blk[:blk.find("}") + 1]
    ok = "_Cap57YH.w" in blk and "_Cap57.w" in blk
    bad += 0 if ok else 1
    lines.append("planted the shader's dim-yellow block tests the keep bound (_Cap57YH.w, floor _Cap57.w) -> %s" % ("as planted" if ok else "WRONG (not read: the halo is capped in a Player)"))
    with open(os.path.join(ROOT, "Assets", "Scripts", "Game", "Render", "Cap57Pass.cs"), encoding="utf-8") as fh:
        cs = fh.read()
    ok = "y.Length > 6" in cs
    bad += 0 if ok else 1
    lines.append("planted Cap57Math (the shader's C# mirror) tests the keep bound (y[6]) -> %s" % ("as planted" if ok else "WRONG"))
    n = sum(1 for l in lines if l.startswith("planted"))
    for l in lines:
        print(l)
    print("PLANTED %s" % (("GREEN (%d / %d as planted)" % (n, n)) if bad == 0 else ("RED (%d of %d wrong)" % (bad, n))))
    return 0 if bad == 0 else 1


def main():
    args = sys.argv[1:]
    cfg = os.path.join(ROOT, "Design", "unity-config.json")
    if "--config" in args:
        i = args.index("--config"); cfg = args[i + 1]; del args[i:i + 2]
    if not args or args[0] != "planted":
        print("usage: n59-v-yellow.py planted [--config C]")
        return 2
    return run(cfg)


if __name__ == "__main__":
    sys.exit(main())

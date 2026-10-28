"""NIGHT72 builder L (L2): ranks the flicker sources. Input: the --json files of Tools/night/n72-flicker.py, one per still72 side, the plain side first and then one side
per source switched off alone (-skygrindFlickerOff72=<source>, Game/Fix72.cs), plus the fix72 side (-skygrindRows72 fix72:1). Output: a markdown table
(Research/2026-10-27-wave72/L/flicker-rank.md) of the flip share on the same poses, sorted by how much each source's removal took off the plain side.
  python -I Tools/night/n72-flicker-rank.py --base plain=<plain.json> grain=<g.json> crtgrain=<c.json> ... [--out flicker-rank.md] [--header "<command, build, time>"]
  python -I Tools/night/n72-flicker-rank.py --selftest
Columns: the mean flip share over the poses both sides share, the worst pose's share (and its name), the share removed against the plain side (points of the
frame and % of the plain share), per level the worst pose. Poses are matched by name, so a side that lost a pose is compared on the rest and says so.
The slate's bar for fix72 (NIGHT72 L2): the flip share falls by >= 50 % on each level's worst pose against the plain side, same build.
"""
import json
import sys


def load(path):
    with open(path, encoding="utf-8") as fh:
        d = json.load(fh)
    return {k.replace("still72-", ""): v for k, v in d["poses"].items() if "flipSharePct" in v}


def level_of(name):
    parts = name.split("-")  # T72-<levelid>-<room>-<pose>
    return parts[1] if len(parts) > 3 and parts[0] == "T72" else "?"


def table(base, sides):
    names = sorted(base)
    rows = []
    for label, poses in sides:
        common = [n for n in names if n in poses]
        if not common:
            rows.append((label, None))
            continue
        b = [base[n]["flipSharePct"] for n in common]
        s = [poses[n]["flipSharePct"] for n in common]
        mean_b, mean_s = sum(b) / len(b), sum(s) / len(s)
        worst_i = max(range(len(common)), key=lambda i: s[i])
        lv = {}
        for n in common:
            L = level_of(n)
            lv.setdefault(L, []).append((base[n]["flipSharePct"], poses[n]["flipSharePct"], n))
        per_level = {}
        for L, vals in lv.items():
            w = max(vals, key=lambda t: t[0])  # the plain side's worst pose on that level
            per_level[L] = (w[2], w[0], w[1], 100.0 * (w[0] - w[1]) / w[0] if w[0] > 0 else 0.0)
        rows.append((label, {
            "poses": len(common), "missing": len(names) - len(common), "mean": mean_s, "meanBase": mean_b,
            "drop": mean_b - mean_s, "dropPct": 100.0 * (mean_b - mean_s) / mean_b if mean_b > 0 else 0.0,
            "worst": s[worst_i], "worstName": common[worst_i], "perLevel": per_level,
        }))
    rows.sort(key=lambda r: -(r[1]["drop"] if r[1] else -1e9))
    return rows


def render(base_label, base, rows, header):
    out = []
    if header:
        out.append(header)
        out.append("")
    mean_base = sum(v["flipSharePct"] for v in base.values()) / len(base)
    worst = max(base, key=lambda n: base[n]["flipSharePct"])
    out.append("# Flicker rank (n72-flicker.py flip share: pixels whose luma reverses 3+ times by more than 6 levels in 60 frames; bar 0.01 % of the frame)")
    out.append("")
    out.append("Plain side `%s`: %d poses, mean %.4f %%, worst `%s` %.4f %%." % (base_label, len(base), mean_base, worst, base[worst]["flipSharePct"]))
    out.append("")
    out.append("| side | poses | mean share % | removed (points) | removed % of plain | worst pose % | worst pose |")
    out.append("|---|---|---|---|---|---|---|")
    for label, r in rows:
        if r is None:
            out.append("| %s | 0 | no pose in common with the plain side | | | | |" % label)
            continue
        out.append("| %s | %d%s | %.4f | %.4f | %.0f | %.4f | %s |" % (label, r["poses"], " (%d missing)" % r["missing"] if r["missing"] else "", r["mean"], r["drop"], r["dropPct"], r["worst"], r["worstName"]))
    out.append("")
    out.append("Per level, the plain side's worst pose and what each side reads on it (share %, and the fall against plain):")
    out.append("")
    levels = sorted({L for _l, r in rows if r for L in r["perLevel"]})
    for L in levels:
        out.append("- level %s:" % L)
        for label, r in rows:
            if r and L in r["perLevel"]:
                n, b, s, pct = r["perLevel"][L]
                out.append("  - %s: `%s` plain %.4f -> %.4f (%.0f %% fall)" % (label, n, b, s, pct))
    return "\n".join(out) + "\n"


def selftest():
    def mk(shares):
        return {"T72-lv-r%d-p" % i: {"flipSharePct": s} for i, s in enumerate(shares)}
    base = mk([10.0, 4.0, 0.0])
    rows = table(base, [("grain", mk([2.0, 1.0, 0.0])), ("bloom", mk([9.5, 4.0, 0.0])), ("fix72", mk([4.0, 1.5, 0.0])), ("lost", {})])
    order = [r[0] for r in rows]
    ok = order[0] == "grain" and order[1] == "fix72" and order[2] == "bloom" and order[3] == "lost"
    ok = ok and abs(rows[0][1]["dropPct"] - 100.0 * (14.0 - 3.0) / 14.0) < 1e-9
    ok = ok and abs(rows[1][1]["perLevel"]["lv"][3] - 60.0) < 1e-9
    print("%s rank order %s; grain removes %.1f %%; fix72's worst-pose fall %.0f %%" % ("ok  " if ok else "FAIL", order, rows[0][1]["dropPct"], rows[1][1]["perLevel"]["lv"][3]))
    text = render("plain", base, rows, "header")
    ok = ok and "| grain |" in text and "no pose in common" in text
    print("SELFTEST n72-flicker-rank: %s" % ("PASS" if ok else "FAIL"))
    return 0 if ok else 1


def main(argv):
    if "--selftest" in argv:
        return selftest()
    out = None
    header = None
    base = None
    base_label = "plain"
    sides = []
    i = 0
    while i < len(argv):
        a = argv[i]
        if a == "--out":
            out = argv[i + 1]; i += 1
        elif a == "--header":
            header = argv[i + 1]; i += 1
        elif a == "--base":
            base_label, path = argv[i + 1].split("=", 1)
            base = load(path); i += 1
        elif "=" in a:
            label, path = a.split("=", 1)
            sides.append((label, load(path)))
        i += 1
    if base is None or not sides:
        print("usage: n72-flicker-rank.py --base plain=<plain.json> <label>=<json> ... [--out FILE] [--header TEXT]   |   --selftest")
        return 2
    text = render(base_label, base, table(base, sides), header)
    if out:
        with open(out, "w", encoding="utf-8") as fh:
            fh.write(text)
    print(text)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

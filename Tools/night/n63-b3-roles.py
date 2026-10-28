# NIGHT63 builder D (B3c, B3's one fix round; Logs/n63/verdict-B3.md: "the meter cited is a whole-frame palette meter that cannot judge
# 'attack effects' or 'weak points'"): B3's PER-BOSS ROLE MEASURE, on the boss's own pixels. A boss63 same-run pair (the row bossColours63 ON vs
# OFF, the same instant) differs only where B3 recoloured the boss, so the CHANGED PIXELS (a channel differs by > 24, n62-gun-yellow.py's rule;
# the HUD's corners left out) are the boss parts B3 routed, on screen. Each changed pixel is classed ON and OFF with the colour table's bands
# (n63-c1-meter.py's own: vivid = S >= 0.55 and V >= 0.50; danger 345-35, parry 45-70, player 160-205, rival 285-335; else an off-palette group:
# amber 35-45, lime, green, blue, violet, rose; not vivid = dull). The slate's doneWhen ("attack effects in the danger band, its weak points in
# one colour, no boss effect in cyan or magenta") read on those pixels:
#   (a) OFF-ROLE: changed pixels that are vivid ON but in no role B3 routes to (parry yellow, rival magenta, any off-palette group): an attack or
#       a weak point left off its role, or routed into the wrong one. FAIL above max(300 px, 2 % of the change).
#   (b) A CYAN EFFECT: changed pixels that were danger OFF and are the player's cyan ON (an attack turned cyan). FAIL above the same floor.
#   (c) A WEAK POINT IN TWO COLOURS: the vivid, non-danger ON pixels of the change fall in two or more families (player, parry, rival, each
#       off group) each above max(300 px, 2 % of the WHOLE change). FAIL. NIGHT64 B3 (Logs/n63/verdict-B3.md: on THE CROWN's tell and windup
#       the old floor, 5 % of the non-danger px, counted the 1-2 px pale-pink / white streak EDGES that saturate when the red-orange behind them
#       changes, 1,309 px of a 126,327 px change, 94.7 % within 3 px of a danger px, as "a weak point in two colours"): the floor is (a)'s own,
#       2 % of the change, so blend edges under it never count while two real colours of a weak point (the wyrm's 36,491 player + 27,240 green
#       of 187,052 on b11) still do.
# NIGHT64 B3: THE HEAD'S HUE (the slate's tuning loop on THE KILN WYRM's open head):
#   python -I Tools/night/n63-b3-roles.py --hue <x0,y0,x1,y1> <on.png> <off.png>   the median hue (and p10 / p90, the count, the median RGB) of
#       the CHANGED, vivid (S >= 0.55, V >= 0.50) ON pixels inside the rect (image px from the top-left, x1 / y1 exclusive), and the OFF hue of
#       the same pixels. The head's rect on boss63's stagger shot: 795,376,1121,684 (NIGHT63's GATE: the blob at x 795-1120, y 376-683).
# A pair whose change is under 100 px is EMPTY (B3 drew nothing on that frame). Beside it, the REGION (the change's box grown by 15 % a side):
# its ON danger / amber / parry / player pixels, B3's or not (the GATE's amber tell ring changed 77 px: a region count shows what was left).
# The cyan's bloom: a green (135-160) or blue (205-230) pixel within 3 px of a player-cyan pixel is that cyan's FRINGE (one colour with it; the
# table's player band already runs to 160 for the bloom); no other role gets a fringe (an amber pixel beside a danger one stays amber).
#   (d) per BOSS (its shots): B3 changed something but no shot shows a weak point in the player's cyan (300 px or more): FAIL (the wyrm on b7).
#       A boss whose every shot is EMPTY reads EMPTY (B3 changed nothing on its shots: its parts as they were).
#   python -I Tools/night/n63-b3-roles.py --pair <on.png> <off.png> [label] [--pair ...]
#   python -I Tools/night/n63-b3-roles.py --boss <on dir> <off dir> [label]       every PNG in both (a boss63 side and its twin), one line
#                                                                                  each and the boss's verdict (the worst of its shots)
#   python -I Tools/night/n63-b3-roles.py --selftest                              planted cases: an attack effect off the danger band, a weak
#                                                                                  point in two colours, a cyan effect, a good route, no change
import importlib.util
import os
import sys

import numpy as np
from PIL import Image

sys.dont_write_bytecode = True  # no __pycache__ beside the tools
_here = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("c1meter", os.path.join(_here, "n63-c1-meter.py"))
c1 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(c1)

DIFF = c1.DIFF
EMPTY = 100
FLOOR_PX, FLOOR_SHARE, FAMILY_SHARE = 300, 0.02, 0.05
FAMILIES = ["player", "parry", "rival"] + [g[0] for g in c1.OFF_GROUPS]
# THE CYAN'S BLOOM FRINGE: a weak point's cyan is drawn HDR (x2.2) and blooms; on a warm scene its halo reads green (135-160) or blue (205-230).
# A vivid pixel in those arcs within FRINGE_PX of a player-cyan pixel of the same frame is that cyan's fringe ("fringe", counted with the cyan),
# as the table's own player band already runs down to 160 for the bloom. Only the cyan gets a fringe: an amber pixel beside a danger one stays
# amber (the GATE's tell ring).
FRINGE_ARCS, FRINGE_PX = [(135.0, 160.0), (205.0, 230.0)], 3


def near(mask, r):
    """mask grown by r pixels (a square), numpy only."""
    out = mask.copy()
    H, W = mask.shape
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            if dx == 0 and dy == 0:
                continue
            src = mask[max(0, -dy):H - max(0, dy), max(0, -dx):W - max(0, dx)]
            out[max(0, dy):H - max(0, -dy), max(0, dx):W - max(0, -dx)] |= src
    return out


def classes(rgb):
    """Per pixel: 'dull', a band name, the cyan's 'fringe', or an off-palette group name (n63-c1-meter.py's vivid floor and arcs)."""
    h, s, v = c1.hsv(rgb)
    vivid = (s >= c1.SMIN) & (v >= c1.VMIN)
    out = np.full(rgb.shape[:2], "dull", dtype=object)
    done = np.zeros(rgb.shape[:2], bool)
    for name, lo, hi in c1.BANDS:
        m = vivid & ~done & c1.in_arc(h, lo, hi)
        out[m] = name
        done |= m
    cyan = out == "player"
    if cyan.any():
        around = near(cyan, FRINGE_PX)
        for lo, hi in FRINGE_ARCS:
            m = vivid & ~done & around & c1.in_arc(h, lo, hi)
            out[m] = "fringe"
            done |= m
    for name, lo, hi in c1.OFF_GROUPS:
        m = vivid & ~done & c1.in_arc(h, lo, hi)
        out[m] = name
        done |= m
    return out


def counts(cls, mask):
    vals, n = np.unique(cls[mask], return_counts=True)
    return {str(k): int(c) for k, c in zip(vals, n)}


def measure(on, off):
    if on.shape != off.shape:
        raise ValueError("the two frames differ in size")
    keep = c1.mask_hud(on.shape)
    diff = np.abs(on.astype(np.int16) - off.astype(np.int16)).max(-1) > DIFF
    m = diff & keep
    n = int(m.sum())
    r = {"changed": n}
    if n < EMPTY:
        r["verdict"] = "EMPTY"
        r["reasons"] = ["the change is under %d px: B3 drew nothing on this frame" % EMPTY]
        return r
    con, coff = classes(on), classes(off)
    r["on"], r["off"] = counts(con, m), counts(coff, m)
    offrole = sum(r["on"].get(k, 0) for k in FAMILIES if k != "player")
    cyaneffect = int((m & (coff == "danger") & (con == "player")).sum())
    floor = max(FLOOR_PX, FLOOR_SHARE * n)
    ffloor = max(FLOOR_PX, FLOOR_SHARE * n)  # NIGHT64 B3: (a)'s floor, 2 % of the change (was 5 % of the non-danger px: the crown's streak edges)
    fam = [k for k in FAMILIES if r["on"].get(k, 0) >= ffloor]
    r["offrole"], r["cyaneffect"], r["families"] = offrole, cyaneffect, fam
    ys, xs = np.nonzero(m)
    H, W = m.shape
    x0, x1, y0, y1 = xs.min(), xs.max(), ys.min(), ys.max()
    gx, gy = int(0.15 * (x1 - x0 + 1)), int(0.15 * (y1 - y0 + 1))
    reg = np.zeros_like(m)
    reg[max(0, y0 - gy):min(H, y1 + gy + 1), max(0, x0 - gx):min(W, x1 + gx + 1)] = True
    reg &= keep
    rc = counts(con, reg)
    r["region"] = {k: rc.get(k, 0) for k in ("danger", "amber", "parry", "player")}
    reasons = []
    if offrole > floor:
        reasons.append("(a) %d changed px off every role B3 routes to ON (%s)" % (offrole, ", ".join("%s %d" % (k, r["on"][k]) for k in FAMILIES if k != "player" and r["on"].get(k, 0))))
    if cyaneffect > floor:
        reasons.append("(b) %d px danger OFF -> cyan ON (a cyan effect)" % cyaneffect)
    if len(fam) >= 2:
        reasons.append("(c) a weak point in %d colours (%s)" % (len(fam), ", ".join(fam)))
    r["verdict"] = "FAIL" if reasons else "PASS"
    r["reasons"] = reasons
    return r


ALL_S, ALL_V = 0.30, 0.30  # NIGHT64 B3 round 2 (Logs/n64/verdict-B3.md): --hue-all, the critic's own floor: every saturated changed px


def hue_in(on, off, rect, smin=None, vmin=None):
    """NIGHT64 B3: the median hue of the changed, vivid ON px inside rect (x0, y0, x1, y1; x1 / y1 exclusive), with the OFF hue of the same px.
    Round 2: smin / vmin (--hue-all: 0.30 / 0.30) so a dim part of the weak point (the translucent box, V 0.45) is read too, and the share of
    the read px over 200 deg."""
    x0, y0, x1, y1 = rect
    m = np.zeros(on.shape[:2], bool)
    m[max(0, y0):max(0, y1), max(0, x0):max(0, x1)] = True
    diff = np.abs(on.astype(np.int16) - off.astype(np.int16)).max(-1) > DIFF
    h, s, v = c1.hsv(on)
    ho, _, _ = c1.hsv(off)
    sel = m & diff & (s >= (c1.SMIN if smin is None else smin)) & (v >= (c1.VMIN if vmin is None else vmin))
    n = int(sel.sum())
    r = {"px": n, "rect": int(m.sum()), "changed": int((m & diff).sum())}
    if n == 0:
        return r
    hh = h[sel]
    r["over200"] = float((hh > 200.0).mean())
    r["under175"] = float((hh < 175.0).mean())
    r["p10"], r["p50"], r["p90"] = (float(np.percentile(hh, q)) for q in (10, 50, 90))
    r["rgb"] = [int(x) for x in np.median(on[sel], axis=0)]
    r["v50"] = float(np.median(v[sel]))
    r["off50"] = float(np.median(ho[sel]))
    r["offrgb"] = [int(x) for x in np.median(off[sel], axis=0)]
    return r


def hue_line(label, r, band=(175.0, 200.0)):
    if r["px"] == 0:
        return "%s: HUE none (0 changed vivid px in the rect; %d changed px of %d)" % (label, r["changed"], r["rect"])
    ok = band[0] <= r["p50"] <= band[1]
    return ("%s: HUE median %.1f (p10 %.1f, p90 %.1f; %.1f %% over 200, %.1f %% under 175) over %d changed px read (of %d changed in the rect), median RGB %s, V %.2f | OFF median %.1f RGB %s | "
            "the slate's band %.0f-%.0f: %s" % (label, r["p50"], r["p10"], r["p90"], 100 * r["over200"], 100 * r["under175"], r["px"], r["changed"], r["rgb"], r["v50"], r["off50"], r["offrgb"],
                                                band[0], band[1], "IN" if ok else "OUT (%+.1f deg to %.0f)" % ((band[0] if r["p50"] < band[0] else band[1]) - r["p50"], band[0] if r["p50"] < band[0] else band[1])))


def line(label, r):
    if r["verdict"] == "EMPTY":
        return "%s: changed px %d | EMPTY (%s)" % (label, r["changed"], r["reasons"][0])
    def cs(d):
        keys = ["player", "fringe", "danger", "parry", "rival"] + [g[0] for g in c1.OFF_GROUPS] + ["dull"]
        return ", ".join("%s %d" % (k, d[k]) for k in keys if d.get(k, 0))
    return ("%s: changed px %d | ON %s | OFF %s | (a) off-role ON %d, (b) danger -> cyan %d, (c) weak families %s | region ON danger %d, amber %d, parry %d, player %d | %s%s"
            % (label, r["changed"], cs(r["on"]), cs(r["off"]), r["offrole"], r["cyaneffect"], "[" + ", ".join(r["families"]) + "]",
               r["region"]["danger"], r["region"]["amber"], r["region"]["parry"], r["region"]["player"], r["verdict"],
               (": " + "; ".join(r["reasons"])) if r["reasons"] else ""))


def load(p):
    return np.asarray(Image.open(p).convert("RGB"))


def verdict_of(results):
    """A boss's verdict from its shots: FAIL if a shot FAILs, or (d) if B3 changed something but no shot shows its weak point in the player's
    cyan (above the floor); EMPTY if B3 changed nothing on any shot; else PASS."""
    shown = [r for r in results if r["verdict"] != "EMPTY"]
    if not shown:
        return "EMPTY", ["B3 changed nothing on any shot (the boss's parts as they were)"]
    reasons = []
    if any(r["verdict"] == "FAIL" for r in results):
        reasons.append("a shot FAILs")
    if not any(r["on"].get("player", 0) >= FLOOR_PX for r in shown):
        reasons.append("(d) no shot shows a weak point in the player's cyan (the most: %d px)" % max(r["on"].get("player", 0) for r in shown))
    return ("FAIL" if reasons else "PASS"), reasons


def boss(ondir, offdir, label):
    names = sorted(f for f in os.listdir(ondir) if f.lower().endswith(".png") and os.path.exists(os.path.join(offdir, f)))
    if not names:
        print("%s: no PNG in both %s and %s" % (label, ondir, offdir))
        return "EMPTY"
    results = []
    for f in names:
        r = measure(load(os.path.join(ondir, f)), load(os.path.join(offdir, f)))
        print(line("%s %s" % (label, f), r))
        results.append(r)
    v, why = verdict_of(results)
    print("%s: BOSS %s (%d shots: %s)%s" % (label, v, len(results), ", ".join("%s %s" % (n, r["verdict"]) for n, r in zip(names, results)), (": " + "; ".join(why)) if why else ""))
    return v


def selftest():
    W, H = 1000, 600
    base = np.full((H, W, 3), 30, np.uint8)
    ok = True

    def blob(img, x0, x1, y0, y1, c):
        img[y0:y1, x0:x1] = c

    def check(name, cond, r):
        nonlocal ok
        ok &= bool(cond)
        print(("PASS " if cond else "FAIL ") + name + " | " + line("planted", r))

    red, amber, cyan, yellow, white = (255, 74, 28), (255, 170, 40), (25, 211, 230), (255, 210, 58), (235, 235, 230)
    # 1. an attack effect off the danger band: red OFF, amber ON
    on, off = base.copy(), base.copy()
    blob(off, 400, 600, 200, 400, red)
    blob(on, 400, 600, 200, 400, amber)
    r = measure(on, off)
    check("an attack effect off the danger band is (a)", r["verdict"] == "FAIL" and r["reasons"][0].startswith("(a)") and r["on"].get("amber", 0) == 40000, r)
    # 2. a weak point in two colours: dull OFF, half cyan and half yellow ON
    on, off = base.copy(), base.copy()
    blob(off, 400, 600, 200, 400, (60, 60, 60))
    blob(on, 400, 500, 200, 400, cyan)
    blob(on, 500, 600, 200, 400, yellow)
    r = measure(on, off)
    check("a weak point in two colours is (c)", r["verdict"] == "FAIL" and any(x.startswith("(c)") for x in r["reasons"]) and set(r["families"]) == {"player", "parry"}, r)
    # 3. a cyan effect: danger OFF, cyan ON
    on, off = base.copy(), base.copy()
    blob(off, 400, 600, 200, 400, red)
    blob(on, 400, 600, 200, 400, cyan)
    r = measure(on, off)
    check("a cyan effect is (b)", r["verdict"] == "FAIL" and any(x.startswith("(b)") for x in r["reasons"]) and r["cyaneffect"] == 40000, r)
    # 4. a good route: an amber attack goes danger, a white weak point goes cyan
    on, off = base.copy(), base.copy()
    blob(off, 200, 400, 200, 400, amber)
    blob(on, 200, 400, 200, 400, red)
    blob(off, 600, 800, 200, 400, white)
    blob(on, 600, 800, 200, 400, cyan)
    r = measure(on, off)
    check("a good route passes", r["verdict"] == "PASS" and r["on"].get("danger", 0) == 40000 and r["on"].get("player", 0) == 40000, r)
    # 5. no change
    r = measure(base.copy(), base.copy())
    check("no change is EMPTY", r["verdict"] == "EMPTY" and r["changed"] == 0, r)
    # 6. the region: an unchanged amber blob beside a changed one is counted
    on, off = base.copy(), base.copy()
    blob(off, 400, 500, 200, 300, white)
    blob(on, 400, 500, 200, 300, cyan)
    blob(on, 505, 512, 200, 300, amber)  # inside the region (the change's 100 px box grown 15 px a side), unchanged
    blob(off, 505, 512, 200, 300, amber)
    blob(on, 540, 560, 200, 300, amber)  # outside it
    blob(off, 540, 560, 200, 300, amber)
    r = measure(on, off)
    check("an unchanged amber blob beside the change is in the region (one past it is not)", r["verdict"] == "PASS" and r["region"]["amber"] == 700, r)
    # 7. the HUD's corners never count
    on, off = base.copy(), base.copy()
    blob(on, 0, 100, 560, 600, cyan)
    r = measure(on, off)
    check("the HUD's corners are left out", r["verdict"] == "EMPTY" and r["changed"] == 0, r)
    # 8. the cyan's bloom fringe: a cyan blob with a 2 px green-cyan ring (hue ~150) is one colour; the same green 20 px away is not its fringe
    green = (40, 230, 130)  # hue ~150
    on, off = base.copy(), base.copy()
    blob(off, 398, 602, 198, 402, (60, 60, 60))
    blob(on, 398, 602, 198, 402, green)
    blob(on, 400, 600, 200, 400, cyan)
    r = measure(on, off)
    check("the cyan's 2 px bloom ring is its fringe (one colour)", r["verdict"] == "PASS" and r["on"].get("fringe", 0) == 204 * 204 - 200 * 200 and r["on"].get("green", 0) == 0, r)
    on, off = base.copy(), base.copy()
    blob(off, 400, 600, 200, 400, (60, 60, 60))
    blob(on, 400, 600, 200, 400, cyan)
    blob(off, 620, 700, 200, 400, (60, 60, 60))
    blob(on, 620, 700, 200, 400, green)
    r = measure(on, off)
    check("a green part 20 px from the cyan is not its fringe (two colours)", r["verdict"] == "FAIL" and r["on"].get("green", 0) == 16000, r)
    # 9. a boss: B3 changed its shots but none shows the weak point cyan (the wyrm on b7): (d)
    on, off = base.copy(), base.copy()
    blob(off, 400, 600, 200, 400, amber)
    blob(on, 400, 600, 200, 400, red)
    v, why = verdict_of([measure(on, off), measure(base.copy(), base.copy())])
    ok &= v == "FAIL" and why and why[0].startswith("(d)")
    print(("PASS " if v == "FAIL" and why and why[0].startswith("(d)") else "FAIL ") + "a boss whose changed shots never show the weak point cyan is (d) | " + v + ": " + "; ".join(why))
    v, why = verdict_of([measure(base.copy(), base.copy())])
    ok &= v == "EMPTY"
    print(("PASS " if v == "EMPTY" else "FAIL ") + "a boss B3 changed nothing on is EMPTY | " + v + ": " + "; ".join(why))
    # 10. NIGHT64 B3: THE CROWN's pale-pink streak edges (Logs/n63/verdict-B3.md): a large amber -> red change with scattered 1-2 px rose and
    #     rival edges, each family over 300 px but together about 1 % of the change: not a weak point in two colours (old (c): FAIL)
    on, off = base.copy(), base.copy()
    blob(off, 100, 400, 100, 300, amber)
    blob(on, 100, 400, 100, 300, red)  # 60,000 px danger
    rose, rival = (232, 73, 126), (232, 73, 200)  # hue 340 and 312, vivid (the verdict's median ON (232, 73, 136))
    for k in range(0, 300, 2):  # 150 streak edges of 2 x 1 px each colour, along the blob's right edge and below it
        blob(off, 402 + (k % 20), 404 + (k % 20), 100 + k // 2, 101 + k // 2, (200, 170, 180))
        blob(on, 402 + (k % 20), 404 + (k % 20), 100 + k // 2, 101 + k // 2, rose)
        blob(off, 430 + (k % 20), 432 + (k % 20), 100 + k // 2, 101 + k // 2, (200, 170, 200))
        blob(on, 430 + (k % 20), 432 + (k % 20), 100 + k // 2, 101 + k // 2, rival)
    r = measure(on, off)
    check("THE CROWN's pale-pink streak edges (rose 300 + rival 300 px, 1 % of a 60,600 px change) are not a weak point in two colours",
          r["verdict"] == "PASS" and r["on"].get("rose", 0) == 300 and r["on"].get("rival", 0) == 300 and r["families"] == [], r)
    # 11. NIGHT64 B3: the head's hue (--hue): a cyan patch at hue 185 reads 185, a spring green one at 158 reads 158, the unchanged px are left out
    import colorsys

    def hue_px(hdeg):
        return tuple(int(round(255 * x)) for x in colorsys.hsv_to_rgb(hdeg / 360.0, 0.95, 0.92))
    for want in (185.0, 158.0):
        on, off = base.copy(), base.copy()
        blob(off, 300, 500, 200, 400, (60, 60, 60))
        blob(on, 300, 500, 200, 400, hue_px(want))
        blob(on, 520, 560, 200, 400, (255, 74, 28))  # unchanged red beside it, inside the rect: never counted
        blob(off, 520, 560, 200, 400, (255, 74, 28))
        hr = hue_in(on, off, (290, 190, 570, 410))
        good = hr["px"] == 40000 and abs(hr["p50"] - want) <= 0.6
        ok &= good
        print(("PASS " if good else "FAIL ") + "a patch at hue %.0f reads %.0f | " % (want, want) + hue_line("planted", hr))
    on, off = base.copy(), base.copy()
    hr = hue_in(on, off, (0, 0, 100, 100))
    ok &= hr["px"] == 0
    print(("PASS " if hr["px"] == 0 else "FAIL ") + "no change reads no hue | " + hue_line("planted", hr))
    # 12. NIGHT64 B3 round 2 (Logs/n64/verdict-B3.md): a bright cyan cube (hue 185, V 0.92) inside a bigger dim navy box (hue 222, S 0.6,
    #     V 0.45): --hue (the vivid floor) reads the cube only, IN; --hue-all (S, V >= 0.30) reads the box too, OUT, most px over 200
    navy = tuple(int(round(255 * x)) for x in colorsys.hsv_to_rgb(222 / 360.0, 0.6, 0.45))
    on, off = base.copy(), base.copy()
    blob(off, 200, 600, 100, 500, (60, 60, 60))
    blob(on, 200, 600, 100, 500, navy)
    blob(on, 330, 470, 230, 370, hue_px(185.0))
    vivid, every = hue_in(on, off, (200, 100, 600, 500)), hue_in(on, off, (200, 100, 600, 500), ALL_S, ALL_V)
    good = abs(vivid["p50"] - 185) <= 0.6 and vivid["px"] == 140 * 140 and abs(every["p50"] - 222) <= 1.0 and every["over200"] > 0.8
    ok &= good
    print(("PASS " if good else "FAIL ") + "a bright cyan cube in a dim navy box: --hue reads the cube (185), --hue-all reads the box (222) | "
          + hue_line("vivid", vivid) + " || " + hue_line("all", every))
    cyanbox = tuple(int(round(255 * x)) for x in colorsys.hsv_to_rgb(187 / 360.0, 0.6, 0.45))
    on2 = on.copy()
    blob(on2, 200, 600, 100, 500, cyanbox)
    blob(on2, 330, 470, 230, 370, hue_px(185.0))
    every = hue_in(on2, off, (200, 100, 600, 500), ALL_S, ALL_V)
    good = 175 <= every["p50"] <= 200 and every["over200"] == 0.0
    ok &= good
    print(("PASS " if good else "FAIL ") + "the same cube in a dim CYAN box: --hue-all reads 175-200, none over 200 | " + hue_line("all", every))
    print("B3 ROLES PLANTED " + ("GREEN" if ok else "RED"))
    return ok


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__ or "see the head of this file")
        return 1
    if argv[0] == "--selftest":
        return 0 if selftest() else 1
    if argv[0] in ("--hue", "--hue-all"):
        rect = tuple(int(float(x)) for x in argv[1].split(","))
        floors = (ALL_S, ALL_V) if argv[0] == "--hue-all" else (None, None)
        print(hue_line("%s %s %s" % (argv[0], os.path.basename(argv[2]), argv[1]), hue_in(load(argv[2]), load(argv[3]), rect, *floors)))
        return 0
    i = 0
    while i < len(argv):
        if argv[i] in ("--pair", "--boss"):
            a, b = argv[i + 1], argv[i + 2]
            given = i + 3 < len(argv) and not argv[i + 3].startswith("--")
            if argv[i] == "--pair":
                label = argv[i + 3] if given else os.path.basename(a)
                print(line(label, measure(load(a), load(b))))
            else:
                label = argv[i + 3] if given else os.path.basename(os.path.dirname(a.rstrip("/\\")))
                boss(a, b, label)
            i += 4 if given else 3
        else:
            print("unknown argument: " + argv[i])
            return 1
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

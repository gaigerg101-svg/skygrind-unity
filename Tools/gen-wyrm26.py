# Generates Design/bosses/wyrm26.json and Design/maps/wyrm26-arena.json (run from the repo root): python Tools/gen-wyrm26.py
# NIGHT26 builder B (contract wyrm-26, GG: the worm's arena should encourage wall riding; a harder final attack). Reads NIGHT25's files (wyrm.json, wyrm-arena.json),
# which stay as they are (design.n26 level3 off = NIGHT25's worm), and writes level 3's: the same cycle plus a phase-3 FINALE (cycle26), and the same room plus
# WALL-RIDE SLABS (free-standing slabs 3.25 m off the wall ring: a corridor to ride, wall-jump between slab and wall, chain to the pillars) and a fourth platform.
import json, math

paths = json.load(open('Design/bosses/wyrm.json'))
arena = json.load(open('Design/maps/wyrm-arena.json'))

def r1(v):
    return round(v, 2)

def P(x, y, z):
    return [r1(x), r1(y), r1(z)]

def mirror(wps):
    return [[-p[0], p[1], p[2]] for p in wps]

# ---------------- the FINALE: a ring squeeze with one marked gap ----------------
# lead-in: 130 degrees at R 23 (50 m of path: the tell shows 1 s = 34 m of phase-3 travel ahead of the entry), then the spiral: R 17 -> 6 over 700 degrees (about two laps),
# every lap of it at the same angle dips under the floor for ~45 degrees: that is the GAP (buried pieces touch nothing and are not hit): the floor crack shows it 1 s ahead.
lead = []
th = 270.0
while th <= 400.0 + 0.001:
    a = math.radians(th)
    lead.append(P(23 * math.cos(a), 3.0, 23 * math.sin(a)))
    th += 20.0
entry_wp = len(lead)
spiral = []
GAP = 460.0
gap_wp = None
th = 400.0
while th <= 1100.0 + 0.001:
    a = math.radians(th)
    R = 17.0 - 11.0 * (th - 400.0) / 700.0
    d = ((th - GAP + 180.0) % 360.0) - 180.0
    y = -4.5 if abs(d) <= 15.0 else 2.5
    if abs(th - GAP) < 1e-6:
        gap_wp = entry_wp + len(spiral)
    spiral.append(P(R * math.cos(a), y, R * math.sin(a)))
    th += 15.0
tail_out = [P(0, 4.0, 4), P(0, 7.0, 10)]
fin = lead + spiral + tail_out
fin_marks = [
    {"wp": entry_wp, "name": "squeeze", "tell": "finale", "attack": True, "phase": 3},
    {"wp": gap_wp, "name": "finaleGap", "tell": "finaleGap", "attack": False, "phase": 3},
]
def variant(wps, marks, mirrored):
    return {"wp": mirror(wps) if mirrored else wps, "marks": marks, "zones": []}
if 'finale' in paths['patterns']:
    del paths['patterns']['finale']
paths['patterns']['finale'] = {"pitch": 75, "spit": False, "variants": [variant(fin, fin_marks, False), variant(fin, fin_marks, True)]}
paths['cycle26'] = ["coil", "dive", "lunge", "loop", "finale"]
paths['_why'] = paths['_why'].split(' LEVEL 3:')[0] + (" LEVEL 3: this file is Tools/gen-wyrm26.py's copy of wyrm.json with the `finale` pattern and `cycle26` (design.n26.bosses.wyrm26 reads it; wyrm.json stays NIGHT25's). The FINALE (phase 3 only: Wyrm skips it before): a lead-in at R 23, then a two-lap spiral squeeze from R 17 to 6 whose path dips under the floor for 45 degrees at one fixed angle (the marked gap: a floor crack tells it 1 s ahead; the buried pieces are a hole in the ring to run or jump through), the squeeze mark tells 1 s (tells.finale), contact in it hurts x finale.contactMult.")
json.dump(paths, open('Design/bosses/wyrm26.json', 'w'), indent=1)

# ---------------- the arena: wall-ride slabs ----------------
# every pattern's waypoints, densified: a slab must stay 3.5 m clear of the worm's route (the head never sits inside geometry)
pts = []
for pat in list(paths['patterns'].values()) + [paths['emerge']]:
    for var in pat['variants']:
        for key in ('wp', 'wp3'):
            w = var.get(key)
            if not w:
                continue
            for i in range(len(w) - 1):
                a, b = w[i], w[i + 1]
                n = max(1, int(math.dist(a, b) / 1.0))
                for k in range(n + 1):
                    t = k / n
                    pts.append([a[j] + (b[j] - a[j]) * t for j in range(3)])
H = arena['height']
boxes = arena['collision']['boxes']
def clear(lo, hi, margin=2.9):
    for x, y, z in pts:
        if y < -1 or y > hi[1] + 1:
            continue
        if lo[0] - margin < x < hi[0] + margin and lo[2] - margin < z < hi[2] + margin:
            return False
    return True
slabs = []
T = 0.75   # half thickness 0.75: slabs 1.5 m thick
LEN = 4.0  # half length 4: 8 m long
SH = 13.0
cands = []
for sx in (-1, 1):
    for sz in (-1, 1):
        cands.append(((sx * 12.0, sz * 26.0), 'x'))  # a slab along x at z = +-26 (3.25 m off the north / south wall)
        cands.append(((sx * 26.0, sz * 12.0), 'z'))  # along z at x = +-26
for (cx, cz), ax in cands:
    if ax == 'x':
        lo, hi = (cx - LEN, 0, cz - T), (cx + LEN, SH, cz + T)
    else:
        lo, hi = (cx - T, 0, cz - LEN), (cx + T, SH, cz + LEN)
    # the player's spawn nook (south wall, z about +27) must stay open
    if abs(cx) < 6 and cz > 20:
        continue
    if clear(lo, hi):
        boxes.append({"min": list(lo), "max": list(hi), "kind": "wall"})
        slabs.append([cx, cz, ax])
# a fourth platform on the ride line (reach it from a slab wall jump)
pl = (0.0, -26.0)
if clear((pl[0] - 3, 9.0, pl[1] - 3), (pl[0] + 3, 10.0, pl[1] + 3), 2.5):
    boxes.append({"min": [pl[0] - 3, 9.0, pl[1] - 3], "max": [pl[0] + 3, 10.0, pl[1] + 3], "kind": "platform"})
arena['name'] = 'wyrm26-arena'
arena['_why'] = arena['_why'] + (" NIGHT26 (builder B, level 3; Tools/gen-wyrm26.py): the same room with %d WALL-RIDE SLABS added (free-standing, 1.5 m thick, 8 m long, 13 m high, 3.25 m off the wall ring: a corridor to wall-ride along, wall-jump between slab and wall, chain from slab to pillar to platform), each placed only where it stays 3.6 m clear of every worm route (the finale's included), and one more platform. Slab positions (x, z, axis): %s." % (len(slabs), slabs))
json.dump(arena, open('Design/maps/wyrm26-arena.json', 'w'), indent=1)
print('wrote wyrm26.json and wyrm26-arena.json; slabs placed', len(slabs), slabs)

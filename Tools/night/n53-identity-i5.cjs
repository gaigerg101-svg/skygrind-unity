// NIGHT53 P0 (the lead): I5's central mass redefined (DECIDE53 D-4): the largest single box missed masses made of a pit and its decks
// (2 rings in 18 rooms at u3-n52, where the reviewers saw four rooms share the shape). Now: the room's central 40 % x 40 % core is >= 30 %
// occupied (solids or pits) and the 2.5 m band round the occupied core's bounds (from 0.5 m out) is >= 70 % walkable floor.
const fs = require("fs");
const f = __dirname + "/n53-identity.py";
let s = fs.readFileSync(f, "utf8");
const a = s.indexOf("def ring_room(p, rid):"), b = s.indexOf("def i5(p):");
if (a < 0 || b < 0 || s.indexOf("def ring_room(p, rid):", a + 1) >= 0) throw new Error("anchors");
const fn = `def ring_room(p, rid):
    room = room_of(p, rid)
    x0, z0, x1, z1 = room['rect']
    w, d, cx, cz = x1 - x0, z1 - z0, (x0 + x1) / 2.0, (z0 + z1) / 2.0
    S = solids(p, rid)
    pits = zone_items(p, 'pits', rid)
    occ = lambda x, z: any(t(x, z) for _, t, _ in S) or any(q['rect'][0] <= x <= q['rect'][2] and q['rect'][1] <= z <= q['rect'][3] for q in pits)
    cells = grid(room)
    core = [(x, z) for x, z in cells if abs(x - cx) <= 0.2 * w and abs(z - cz) <= 0.2 * d]
    oc = [(x, z) for x, z in core if occ(x, z)]
    f = 100.0 * len(oc) / max(1, len(core))
    if not oc:
        return False, 'core occupied 0 %'
    bx0, bx1 = min(x for x, _ in oc), max(x for x, _ in oc)
    bz0, bz1 = min(z for _, z in oc), max(z for _, z in oc)
    band = [not occ(x, z) for x, z in cells if 0.5 <= math.hypot(max(bx0 - x, 0, x - bx1), max(bz0 - z, 0, z - bz1)) <= 3.0]
    fr = 100.0 * sum(band) / max(1, len(band))
    return f >= 30.0 and fr >= 70.0, 'core (central 40 %% x 40 %%) occupied %.0f %% (>= 30 = a mass), the band round it %.0f %% walkable (>= 70 = a ring)' % (f, fr)


`;
s = s.slice(0, a) + fn + s.slice(b);
s = s.replace("#   I5 Not one shape: a room is RING-ROUND-A-MASS when its largest solid box (footprint area, max y >= 1 m) has its centre within 20 % of\n#      the room's centre on each axis and the 2.5 m band round it (from 0.5 m out) is >= 70 % walkable floor (no solid, no pit). Per level\n#      at most two such rooms PASS.",
  "#   I5 Not one shape: a room is RING-ROUND-A-MASS when its central 40 % x 40 % core is >= 30 % occupied (solids or pits: the mass) and the\n#      2.5 m band round the occupied core's bounds (from 0.5 m out) is >= 70 % walkable floor (the ring; DECIDE53 D-4). Per level at most\n#      two such rooms PASS.");
if (!s.includes("DECIDE53 D-4). Per level")) throw new Error("doc anchor");
fs.writeFileSync(f, s);
console.log("I5 redefined");

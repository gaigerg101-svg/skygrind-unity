#!/usr/bin/env python3
"""NIGHT25 builder C: writes Design/levels/level06..level10.plan.json from one room template (layer1's r1 shape: 52 x 48 m, a mezzanine tier on the east
wall with a ramp, four cover blocks, spawn at the south, exit at the north, a 6 m wall gap and an 8 m door between rooms). Re-run after editing WAVES / ROOMS.
The par of every room is the headless bot's median over 10 seeds (Tools/night/gen-levels610.py --pars reads the campaign output and writes them: never hand-typed
except the first guess here)."""
import json, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import importlib.util
_spec = importlib.util.spec_from_file_location('levels67kits', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'levels67-kits.py'))
_kits = importlib.util.module_from_spec(_spec); _spec.loader.exec_module(_kits)
KITS = _kits.KITS
# Levels 8-10 are builder L's now (authored from these plan files): this generator writes only these.
OWNED = {'level06', 'level07'}

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, '..', '..', 'Design', 'levels')
if len(sys.argv) > 2 and sys.argv[1] == '--pars':
    PARS = json.load(open(sys.argv[2]))
else:
    PARS = {}

ESTIMATE = json.load(open(os.path.join(HERE, 'levels610-estimate.json')))
H, S, SP, C, L, D, W = 'husk', 'swarmer', 'spitter', 'charger', 'lancer', 'drone', 'warden'


def room(i, rid, name, z0, waves, limit=None, award=None, par=45, mirror=False, lanes=False, note=None, kit=None, kind='room', boss=None):
    """One combat room whose north edge is z0 (it spans z0 .. z0 + 48)."""
    m = -1 if mirror else 1
    def x(v): return v * m
    z1 = z0 + 48
    est = ESTIMATE
    waves = waves or []
    combat = sum(est["killSeconds"][k] for w in waves for k in w) + est["waveGap"] * max(0, len(waves) - 1) + est["walkSeconds"]
    cap = round(est["capMult"] * combat, 1)
    botMedian = PARS.get(rid, par)
    r = {"id": rid, "name": name, "kind": "room", "rect": [-26, z0, 26, z1], "par": min(botMedian, cap), "botMedian": botMedian, "parCap": cap,
         "spawn": {"at": [0, 0, z1 - 6], "yawDeg": 0}, "exit": [0, 0, z0 + 4]}
    if limit: r["limit"] = limit
    if award: r["award"] = award
    if kind == 'boss':
        r["kind"] = "boss"; r["boss"] = boss; r["par"] = par
        for k in ("botMedian", "parCap"): r.pop(k, None)
        r["nodes"] = []; r["highNodes"] = []; r.pop("waves", None)
        bx = [{"zone": rid, "kind": "hull", "min": [a, 0, z0 + d], "max": [a + 4, 6, z0 + d + 4], "note": "pillar"} for a, d in [(-20, 12), (16, 12), (-20, 32), (16, 32)]]
        return r, bx, [], []
    if kit:
        K = KITS[kit]
        r["nodes"] = [[a, 0, z0 + d] for a, d in K["nodes"]]
        r["highNodes"] = [[a, y, z0 + d] for a, y, d in K["high"]]
        r["waves"] = waves
        if note: r["_note"] = note
        kb = [{"zone": rid, "kind": b[0], "min": [b[1], b[5], z0 + b[3]], "max": [b[2], b[6], z0 + b[4]], "note": kit} for b in K["boxes"]]
        kr = [{"zone": rid, "from": [a, z0 + b], "to": [c, z0 + d], "width": w, "rise": rs, "note": kit} for a, b, c, d, w, rs in K.get("ramps", [])]
        kp = [{"zone": rid, "rect": [a, z0 + b, c, z0 + d], "floorY": fy, "note": kit} for a, b, c, d, fy in K.get("pits", [])]
        return r, kb, kr, kp
    nodes = [(-21, 6), (-21, 22), (-21, 40), (-6, 6), (6, 10), (-8, 28), (-2, 16), (6, 40)]
    high = [(19, 14), (19, 34), (22, 24)]
    r["nodes"] = [[x(a), 0, z0 + b] for a, b in nodes]
    r["highNodes"] = [[x(a), 4, z0 + b] for a, b in high]
    r["waves"] = waves
    if note: r["_note"] = note
    boxes = []
    ex0, ex1 = sorted((x(12), x(26)))
    boxes.append({"zone": rid, "kind": "deck", "min": [ex0, 0, z0 + 8], "max": [ex1, 4, z0 + 40], "note": "the mezzanine along the east wall (tier 2)"})
    for (a0, a1, b0, b1) in [(-16, -12, 10, 14), (-16, -12, 28, 32), (-2, 2, 26, 30), (4, 8, 4, 8)]:
        c0, c1 = sorted((x(a0), x(a1)))
        boxes.append({"zone": rid, "kind": "hull", "min": [c0, 0, z0 + b0], "max": [c1, 3, z0 + b1], "note": "cover"})
    if lanes:  # narrow safe lanes: two long low walls across the floor leaving 6 m lanes (area denial: the Wardens' wall of shields herds you)
        for (a0, a1, b0, b1) in [(-14, -11, 30, 44), (9, 11, 4, 18)]:
            c0, c1 = sorted((x(a0), x(a1)))
            boxes.append({"zone": rid, "kind": "hull", "min": [c0, 0, z0 + b0], "max": [c1, 3, z0 + b1], "note": "lane wall"})
    ramps = [{"zone": rid, "from": [0, z0 + 24], "to": [x(12), z0 + 24], "width": 6, "rise": 4, "note": "up to the mezzanine"}]
    return r, boxes, ramps, []


def level(lid, name, why, specs, extra=None):
    rooms, boxes, ramps, doors, pits = [], [], [], [], []
    z0 = 0
    for k, sp in enumerate(specs):
        r, b, rp, pt = room(k, sp['id'], sp['name'], z0, sp.get('waves'), sp.get('limit'), sp.get('award'), sp.get('par', 45), sp.get('mirror', k % 2 == 1), sp.get('lanes', False), sp.get('note'), sp.get('kit'), sp.get('kind', 'room'), sp.get('boss'))
        rooms.append(r); boxes += b; ramps += rp; pits += pt
        z0 -= 54
    # the first room is the south-most: its spawn faces north; rooms were laid northwards (z decreasing) so shift everything to positive z
    zlast = z0 + 54  # the last room's north edge
    shift = -(zlast + 48) / 2.0  # centred on the origin: the map's extent is symmetric about it (spawn nodes are sampled inside floorSize / 2 - 2)
    def sh(v): return [v[0], v[1], v[2] + shift]
    for r in rooms:
        r["rect"] = [r["rect"][0], r["rect"][1] + shift, r["rect"][2], r["rect"][3] + shift]
        r["spawn"]["at"] = sh(r["spawn"]["at"]); r["exit"] = sh(r["exit"])
        r["nodes"] = [sh(n) for n in r["nodes"]]; r["highNodes"] = [sh(n) for n in r["highNodes"]]
    for b in boxes:
        b["min"] = sh(b["min"]); b["max"] = sh(b["max"])
    for pt in pits:
        pt["rect"] = [pt["rect"][0], pt["rect"][1] + shift, pt["rect"][2], pt["rect"][3] + shift]
    for rp in ramps:
        rp["from"] = [rp["from"][0], rp["from"][1] + shift]; rp["to"] = [rp["to"][0], rp["to"][1] + shift]
    for k in range(len(rooms) - 1):
        a, b = rooms[k], rooms[k + 1]
        doors.append({"id": f"d{k + 1}", "between": [a["id"], b["id"]], "rect": [-4, b["rect"][3], 4, a["rect"][1]], "checkpoint": True})
    size = 52 * 2 + 16
    total = 48 - zlast + 16
    plan = {"_why": why, "id": lid, "name": name, "floorSize": int(total) + 16, "killPlaneY": -30, "wallHeight": 14,
            "rooms": rooms, "doors": doors, "boxes": boxes, "ramps": ramps, "pads": [], "grapple": [], "pits": pits, "tunnels": []}
    if extra: plan.update(extra)
    if lid not in OWNED:
        print(lid, 'is builder L\'s now: not written'); return
    json.dump(plan, open(os.path.join(OUT, lid + '.plan.json'), 'w'), indent=2)
    print(lid, len(rooms), 'rooms, z span', total)


WAVES = json.load(open(os.path.join(HERE, 'levels610-waves.json')))

level('level06', 'LEVEL 6 THE BULWARK',
      "LEVEL 6, THE BULWARK (NIGHT25, builder C; Design/levels/CURRICULUM.md, authored geometry in Tools/night/levels67-kits.py): the ONE new thing is opening a shield (the web's WARDEN is the shield kind, config.warden: headshot over a low wall, flank round it, or punch it from a pedestal). Room 1, SCRAP YARD, is a breath: cover lanes, husks and swarmers. Room 2, THE SHIELD WALL: a low wall across the room, 6 m flank gaps at both ends, a pedestal in front of the wall; one warden at a time behind husks. Room 3, BULWARK ANTE: a long approach, pillars and a raised catwalk, wardens with chargers, leading to the boss door. Room 4: THE BULWARK (kind boss: Core/Enemies/Bulwark.cs through Campaign24's BeginBoss).",
      [dict(id='b1', name='SCRAP YARD', waves=WAVES['06']['b1'], par=40, kit='yard'),
       dict(id='b2', name='THE SHIELD WALL', waves=WAVES['06']['b2'], par=50, kit='shieldwall'),
       dict(id='b3', name='BULWARK ANTE', waves=WAVES['06']['b3'], par=60, kit='ante'),
       dict(id='boss', name='THE BULWARK', kind='boss', boss='bulwark', par=60)])

level('level07', 'LEVEL 7 WARDENS',
      "LEVEL 7, WARDENS (NIGHT25, builder C; authored geometry in Tools/night/levels67-kits.py): the shield kind in earnest and a recap. Room 1, THE GATEHOUSE, is a breath (a gate wall with a 12 m gap). Room 2, THE WARD LANES: dense N-S cover lanes where wardens walk. Room 3, THE FISTWORKS, is meleeOnly: a grid of tight pillars, and a pit on the east side whose platform is reached by the ramp from the middle or by a pulse flick across the pit. Room 4, THE MINI GAUNTLET: every kind met so far in one arena with three heights (floor, a 2 m central deck, 4 m mezzanines).",
      [dict(id='w1', name='THE GATEHOUSE', waves=WAVES['07']['w1'], par=40, kit='gatehouse'),
       dict(id='w2', name='THE WARD LANES', waves=WAVES['07']['w2'], par=60, kit='lanes'),
       dict(id='w3', name='THE FISTWORKS', waves=WAVES['07']['w3'], par=50, limit=['meleeOnly'], kit='fistworks'),
       dict(id='w4', name='THE MINI GAUNTLET', waves=WAVES['07']['w4'], par=80, kit='mini')])

level('level08', 'LEVEL 8 SPEED',
      "LEVEL 8, SPEED (NIGHT25, builder C): no new enemy; the lesson is the par and the rank (Campaign24.RankOf: S within par, A 1.5x, B 2.5x). Three rooms of husks, swarmers, chargers and drones, each with its mezzanine ramp; the fast routes (wall / pad / grapple lines) need plan rows the pits / pads rows give: this first cut is the stepped route only (SHORT: no wall route, see FORMAT-REQUESTS.md R3).",
      [dict(id='p1', name='THE STARTING LINE', waves=WAVES['08']['p1'], par=30),
       dict(id='p2', name='THE CHICANE', waves=WAVES['08']['p2'], par=35),
       dict(id='p3', name='THE LONG STRAIGHT', waves=WAVES['08']['p3'], par=40)])

level('level09', 'LEVEL 9 THE MONOLITH',
      "LEVEL 9, THE MONOLITH (NIGHT25, builder C): a boss that changes the arena. The MONOLITH (Core/Enemies/Monolith.cs, SurvivalSim.Monolith24.cs) has its own arena and no campaign hook (BeginBoss starts only the WYRM): the last room is a gauntlet with the boss's recycled adds (husks, drones, spitters) and a logged SHORT marker (bossMarker). Rooms 1-2 are the run-up.",
      [dict(id='m1', name='THE APPROACH', waves=WAVES['09']['m1'], par=40),
       dict(id='m2', name='THE STACKS', waves=WAVES['09']['m2'], par=55),
       dict(id='m3', name='THE MONOLITH GATE', waves=WAVES['09']['m3'], par=70, note='SHORT monolith: no Campaign24 hook for the MONOLITH; the boss room is this gauntlet')],
      {"bossMarker": {"boss": "monolith", "status": "SHORT", "why": "no campaign hook for the MONOLITH (FORMAT-REQUESTS.md R1)"}})

level('level10', 'LEVEL 10 FINALE',
      "LEVEL 10, THE FINALE (NIGHT25, builder C): every verb and enemy. Room 1 pistolOnly, room 2 noDash, room 3 noSlide (each mixing every kind), then the DUEL room: no adds, a small pack of the heaviest kinds one at a time (warden, lancer, charger) as the stand-in for the final boss (Research/2026-10-08-wave24/DESIGN24-bosses.md: not hookable: SHORT marker bossMarker).",
      [dict(id='f1', name='FINALE ONE', waves=WAVES['10']['f1'], par=50, limit=['pistolOnly']),
       dict(id='f2', name='FINALE TWO', waves=WAVES['10']['f2'], par=55, limit=['noDash']),
       dict(id='f3', name='FINALE THREE', waves=WAVES['10']['f3'], par=55, limit=['noSlide']),
       dict(id='f4', name='THE DUEL', waves=WAVES['10']['f4'], par=70, note='SHORT final boss: the duel stand-in is a no-adds room of heavy kinds')],
      {"bossMarker": {"boss": "final", "status": "SHORT", "why": "no boss hook (FORMAT-REQUESTS.md R1)"}})

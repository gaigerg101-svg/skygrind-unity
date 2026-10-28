#!/usr/bin/env python3
"""NIGHT26 builder M: the PLATFORMING KIT (a wall-jump shaft, a dash gap, a pad-hop chain, a wall-ride hall), as plan fragments.

  python Tools/night/gen-platforming-kits.py                  writes Design/levels/kits/platforming.kits.json (fragments in LOCAL coordinates)
                                                              and Design/levels/kits/platforming.plan.json (the four kits chained: a level the
                                                              headless levelgen can build and validate)
  python Tools/night/gen-platforming-kits.py --place KIT OX OZ PREFIX
                                                              prints the kit's fragment moved by (OX, OZ), zone ids prefixed (what builder L pastes)

Local frame: the kit's start is z = 0, the player walks NORTH (-z) through it; every room is 28 m wide (x -14..14, GG: the first map was too wide)
and `len` m long (rect [-14, -len, 14, 0]); a kit placed at OZ occupies z OZ-len..OZ. Between two chained kits: a 6 m wall gap and a door
(Design/levels/LEVEL-FORMAT.md). Kits: shaft (wall jump), dash (dash only), hop (pad chain), hall (wall-ride hall; a `hallRoute` row the validate
report times against the floor route). Every pit has its recovery stair and a pit floor (no soft-lock); the walk-back is the stair.
"""
import copy, io, json, os, sys

W = 14          # half width: 28 m wide rooms
PIT_Y = -3.5    # a pit's floor (the tutorial's)
NOTE = lambda s: s


def room(id, name, ln, par, exit_z, **extra):
    r = {"id": id, "name": name, "kind": "link", "rect": [-W, -ln, W, 0], "par": par,
         "spawn": {"at": [0, 0, -2], "yawDeg": 0}, "exit": [0, 0, exit_z]}
    r.update(extra)
    return r


def stair(zone, z_edge, toward_north=False, x_side=1):
    """Two steps (pit floor -> -2.4 -> -1.2 -> floor 0), at the pit's edge, east side. z_edge: the pit edge the stair is cut against."""
    if toward_north:   # the pit's NORTH edge (z_edge is its z0): the stair climbs north onto the far floor
        z0, z1 = z_edge, z_edge + 2
    else:              # the pit's SOUTH edge (z_edge is its z1): the stair climbs south, back to where you came from
        z0, z1 = z_edge - 2, z_edge
    return [
        {"zone": zone, "kind": "deck", "min": [10, -4.5, z0], "max": [14, -2.4, z1], "note": "recovery stair, step 1 (from the pit floor)"},
        {"zone": zone, "kind": "deck", "min": [6, -4.5, z0], "max": [10, -1.2, z1], "note": "recovery stair, step 2 (then up to the floor)"},
    ]


def kit_shaft(H=3.4):
    """A wall-jump shaft: two facing walls 5 m apart, a deck H m up at the top, a long ramp down the far side. One verb: the wall jump."""
    ramp_len = round(H / 0.34)
    ln = 24 + 10 + ramp_len + 6
    deck_z0, deck_z1 = -34, -24
    ramp_lo = deck_z0 - ramp_len
    return {
        "len": ln,
        "rooms": [room("shaft", "PLATFORMING 1: WALL JUMP", ln, 4, ramp_lo - 2,
                       routeVia=[[[0, 0, -6], [0, H, -29]]], wallRoute={"required": True, "goalY": H, "note": "the shaft's wall line is the only way up: the deck is %.1f m, a jump reaches 1.5 m (the wall-jump climber tops out near 3.8 m: a 9 m deck is NOT reachable; the walls are 9+ m high)" % H})],
        "boxes": [
            {"zone": "shaft", "kind": "slab", "min": [-4.5, 0, -24], "max": [-2.5, H + 6, -8], "note": "wall-jump wall west (the corridor between the walls is 5 m)"},
            {"zone": "shaft", "kind": "slab", "min": [2.5, 0, -24], "max": [4.5, H + 6, -8], "note": "wall-jump wall east"},
            {"zone": "shaft", "kind": "deck", "min": [-W, 0, deck_z0], "max": [W, H, deck_z1], "note": "the top deck (%.1f m, only the wall line reaches it)" % H},
        ],
        "ramps": [{"zone": "shaft", "from": [0, ramp_lo], "to": [0, deck_z0], "width": 8, "rise": H, "note": "down the north side of the deck (the way on; a long gentle ramp)"}],
        "pads": [], "pits": [], "grapple": [],
    }


def kit_dash(gap=float(os.environ.get("DASH_GAP", "6"))):
    """A dash gap: two decks gap m apart over a pit, no jump needed: a dash from the lip crosses it. One verb: the dash."""
    ln = 44
    z1 = -18
    z0 = z1 - gap
    return {
        "len": ln,
        "rooms": [room("dash", "PLATFORMING 2: DASH", ln, 4, -(ln - 2),
                       dashGap={"from": [0, 0, z1 + 1], "to": [0, 0, z0 - 1], "gap": gap, "note": "validate: a plain running jump falls short, a dash from the lip lands"})],
        "boxes": stair("dash", z1),
        "ramps": [], "pads": [], "grapple": [],
        "pits": [{"zone": "dash", "rect": [-W, z0, W, z1], "floorY": PIT_Y, "note": "a %.0f m pit across the hall: dash it (the stair at its south-east corner is the walk-back)" % gap}],
    }


def kit_hop():
    """A pad-hop chain over a pit: three 6 x 6 platforms, a pad on each throws you to the next, a pad on the near floor throws you onto the first. One verb: the pad."""
    ln = 66
    pit = [-W, -54, W, -12]
    plats = [(-21, -15), (-35, -29), (-49, -43)]       # z0, z1 of each platform (8 m apart)
    boxes = stair("hop", pit[3]) + stair("hop", pit[1], toward_north=True)
    pads = [{"zone": "hop", "at": [0, 0, -9], "target": [0, 0, -18], "apexAbove": 3, "note": "the first pad: throws you onto platform 1"}]
    for i, (a, b) in enumerate(plats):
        boxes.append({"zone": "hop", "kind": "deck", "min": [-3, -4.5, a], "max": [3, 0, b], "note": "hop platform %d (6 x 6, top at floor level)" % (i + 1)})
        mid = (a + b) / 2
        nxt = (plats[i + 1][0] + plats[i + 1][1]) / 2 if i < 2 else -58
        pads.append({"zone": "hop", "at": [0, 0, mid], "target": [0, 0, nxt], "apexAbove": 3,
                     "note": "the pad on platform %d: throws you to %s" % (i + 1, "platform %d" % (i + 2) if i < 2 else "the far floor")})
    return {
        "len": ln,
        "rooms": [room("hop", "PLATFORMING 3: PAD HOP", ln, 6, -(ln - 2))],
        "boxes": boxes, "ramps": [], "pads": pads, "grapple": [],
        "pits": [{"zone": "hop", "rect": pit, "floorY": PIT_Y, "note": "a 42 m pit: three platforms, a pad on each; stairs at both edges (walk-back and way on)"}],
    }


def kit_hall(H=7.0):
    """The WALL-RIDE HALL: a launch deck H m up (a long ramp), then two facing walls 7 m apart, 33 m of wall in three runs (12, 12, 9 m) with two 4.5 m
    gaps (a dash bridges each; a ride sinks about 0.15 g, so the deck's height pays for the three rides and a last dash onto the far floor), over a pit.
    The floor route is the pit floor: two partitions (low enough that the ride passes over them) with alternating openings make it a serpentine."""
    ramp_len = round(H / 0.34)
    deck_z1 = -(1 + ramp_len)
    deck_z0 = deck_z1 - 4
    D = deck_z0 + 14                      # everything below is the first design (near edge at -14) shifted by D
    ln = 76 - D
    pit = [-W, -64 + D, W, -14 + D]
    runs = [(-18 + D, -30 + D), (-34.5 + D, -46.5 + D), (-51 + D, -60 + D)]     # (z_start, z_end) of each wall run (12, 12, 9 m = 33 m), walking north
    boxes = stair("hall", pit[3]) + stair("hall", pit[1], toward_north=True)
    for i, (a, b) in enumerate(runs):
        boxes.append({"zone": "hall", "kind": "slab", "min": [-5.5, -4.5, b], "max": [-3.5, H + 12, a], "note": "rideable wall, west, run %d (%d m)" % (i + 1, abs(b - a))})
        boxes.append({"zone": "hall", "kind": "slab", "min": [3.5, -4.5, b], "max": [5.5, H + 12, a], "note": "rideable wall, east, run %d (%d m)" % (i + 1, abs(b - a))})
    boxes.append({"zone": "hall", "kind": "deck", "min": [-W, 0, deck_z0], "max": [5.5, H, deck_z1], "note": "the launch deck (%.0f m up): the wall route starts high and sinks along the walls; the far floor is back at 0" % H})
    boxes.append({"zone": "hall", "kind": "hull", "min": [-W, -4.5, -25 + D], "max": [5.5, -0.8, -23 + D], "note": "floor-route partition 1 (opening at the east edge): the pit floor is a serpentine; low enough for the wall route to pass over"})
    boxes.append({"zone": "hall", "kind": "hull", "min": [-5.5, -4.5, -41 + D], "max": [W, -0.8, -39 + D], "note": "floor-route partition 2 (opening at the west edge)"})
    boxes.append({"zone": "hall", "kind": "hull", "min": [-5.5, -4.5, -57 + D], "max": [W, -0.8, -55 + D], "note": "floor-route partition 3 (opening at the west edge): the way to the far stair doubles back"})
    gaps = [[-30 + D, -34.5 + D], [-46.5 + D, -51 + D], [-60 + D, -64 + D]]
    return {
        "len": ln,
        "rooms": [room("hall", "PLATFORMING 4: WALL-RIDE HALL", ln, 8, -(ln - 2),
                       hallRoute={"from": [-3.1, 0, -2], "jumpAtZ": deck_z0 + 0.8, "lip": [-3.1, H, deck_z0 + 0.3], "to": [0, 0, -(ln - 2)], "gaps": gaps, "minFloorOverWall": 2.0,
                                  "note": "validate: the scripted wall ride along the walls (dashing the gaps) against RIVAL's graph on the pit floor; the wall route must be at least 2x faster"})],
        "boxes": boxes, "pads": [],
        "ramps": [{"zone": "hall", "from": [0, -1], "to": [0, deck_z1], "width": 8, "rise": H, "note": "up to the launch deck (%d m, a gentle run-up)" % ramp_len}],
        "grapple": [],
        "pits": [{"zone": "hall", "rect": pit, "floorY": PIT_Y, "note": "a 48 m pit under the walls; the stairs at both ends, the partitions make the floor route a serpentine"}],
        "rideable": [[x, a, b] for x in (-4.5, 4.5) for a, b in runs],
    }


def kits():
    return {"shaft": kit_shaft(), "dash": kit_dash(), "hop": kit_hop(), "hall": kit_hall()}


def shift_xyz(v, ox, oz):
    return [v[0] + ox, v[1], v[2] + oz]


def place(kit, ox, oz, prefix):
    """The kit moved by (ox, oz) with its zone ids prefixed: {rooms, boxes, ramps, pads, grapple, pits, rideable, len}."""
    k = copy.deepcopy(kit)
    zid = lambda z: prefix + z
    for r in k["rooms"]:
        r["id"] = zid(r["id"])
        r["rect"] = [r["rect"][0] + ox, r["rect"][1] + oz, r["rect"][2] + ox, r["rect"][3] + oz]
        r["spawn"]["at"] = shift_xyz(r["spawn"]["at"], ox, oz)
        r["exit"] = shift_xyz(r["exit"], ox, oz)
        for key in ("hallRoute", "dashGap"):
            if key in r:
                r[key]["from"] = shift_xyz(r[key]["from"], ox, oz)
                r[key]["to"] = shift_xyz(r[key]["to"], ox, oz)
                if "lip" in r[key]:
                    r[key]["lip"] = shift_xyz(r[key]["lip"], ox, oz)
                if "jumpAtZ" in r[key]:
                    r[key]["jumpAtZ"] += oz
                if "gaps" in r[key]:
                    r[key]["gaps"] = [[a + oz, b + oz] for a, b in r[key]["gaps"]]
    for b in k["boxes"]:
        b["zone"] = zid(b["zone"])
        b["min"] = shift_xyz(b["min"], ox, oz)
        b["max"] = shift_xyz(b["max"], ox, oz)
    for r in k["ramps"]:
        r["zone"] = zid(r["zone"])
        r["from"] = [r["from"][0] + ox, r["from"][1] + oz]
        r["to"] = [r["to"][0] + ox, r["to"][1] + oz]
    for p in k["pads"]:
        p["zone"] = zid(p["zone"])
        p["at"] = shift_xyz(p["at"], ox, oz)
        p["target"] = shift_xyz(p["target"], ox, oz)
    for p in k["pits"]:
        p["zone"] = zid(p["zone"])
        p["rect"] = [p["rect"][0] + ox, p["rect"][1] + oz, p["rect"][2] + ox, p["rect"][3] + oz]
    if "rideable" in k:
        k["rideable"] = [[x + ox, a + oz, b + oz] for x, a, b in k["rideable"]]
    return k


def chain_plan(order, gap=6):
    ks = kits()
    plan = {"_why": "NIGHT26 builder M: the platforming kit chained (Tools/night/gen-platforming-kits.py): a test level for levelgen, not a campaign level.",
            "id": "platforming", "name": "PLATFORMING KIT", "floorSize": 640, "killPlaneY": -30, "wallHeight": 14,
            "rooms": [], "doors": [], "boxes": [], "ramps": [], "pads": [], "grapple": [], "pits": [], "tunnels": [], "rideable": []}
    oz = 0
    prev = None
    for name in order:
        p = place(ks[name], 0, oz, "")
        plan["rooms"] += p["rooms"]
        for key in ("boxes", "ramps", "pads", "pits", "rideable"):
            plan[key] += p.get(key, [])
        if prev:
            plan["doors"].append({"id": "d%d" % len(plan["doors"] + [0]), "between": [prev, p["rooms"][0]["id"]], "rect": [-4, oz - 0, 4, oz + gap], "checkpoint": True})
        prev = p["rooms"][0]["id"]
        oz = oz - ks[name]["len"] - gap
    # the door rect covers the gap between two rooms: from the next room's z1 (its start) up to the previous room's z0
    return plan


def icecourse(tiles=8, tile=40):
    """THE ICE LINE's course (builder B's glacier chase): a straight 320 m course (8 recycled 40 m tiles) on ICE, 3 lanes (28 m wide: left x -14..-4.8, centre
    -4.2..4.2, right 4.8..14) split by low rails with a 4 m lane-change opening at every tile joint, a normal-ground start pad (z 0..-12), BOOST strips every 60 m
    (alternating lanes, pushing north), a mud patch, a 40 m rideable wall (tile 5), and two gaps (6 m: a dash alone; 7 m: a boosted jump) with recovery stairs.
    One room, one `surfaces` list (kind ice | mud | boost, rect [x0, z0, x1, z1]); the course runs NORTH (-z) from the spawn at z = -2 to the exit at z = -318."""
    ln = tiles * tile
    boxes, surf = [], []
    surf.append({"kind": "ice", "rect": [-W, -ln + 2, W, -12], "note": "the whole course past the start pad"})
    surf.append({"kind": "mud", "rect": [4.8, -214, W, -194], "note": "a 20 m mud patch in the right lane: the slow lane (tile 5), for contrast"})
    lanes = [(-9.4, 0), (0, 1), (9.4, 2)]
    for i, z in enumerate(range(-60, -ln, -60)):
        cx = [0, -9.4, 9.4][i % 3]
        surf.append({"kind": "boost", "rect": [cx - 3, z - 4, cx + 3, z], "dir": [0, -1], "note": "boost strip %d (every 60 m, lanes alternate)" % (i + 1)})
    for t in range(tiles):
        z0, z1 = -t * tile - 2, -(t + 1) * tile + 2          # a rail segment per tile, a 4 m opening at each joint
        for x in (-4.8, 4.2):
            boxes.append({"zone": "course", "kind": "hull", "min": [x, 0, z1], "max": [x + 0.6, 1, z0 - 2 if t == 0 else z0], "note": "lane rail (1 m), tile %d" % (t + 1)})
    boxes.append({"zone": "course", "kind": "slab", "min": [7.5, 0, -214 + 4], "max": [9.5, 8, -174], "note": "the wall line: 40 m of rideable wall (tile 5), light strips come with design.n26.wallRide"})
    pits = []
    for (z1, gap, name) in ((-100, 6, "dash gap (6 m: a dash alone crosses)"), (-250, 7, "boost gap (7 m: a jump off a boost strip)")):
        pits.append({"zone": "course", "rect": [-W, z1 - gap, W, z1], "floorY": PIT_Y, "note": name})
        boxes += stair("course", z1)
    # the 60 m boost strip nearest each gap would sit inside it: they are at z -60..-64 (before gap 1 at -100) and z -240..-244 (6 m before gap 2): fine
    room_ = {"id": "course", "name": "THE ICE LINE", "kind": "link", "rect": [-W, -ln, W, 0], "par": 40, "spawn": {"at": [0, 0, -2], "yawDeg": 0}, "exit": [0, 0, -(ln - 2)]}
    return {"_why": "NIGHT26 builder M: THE ICE LINE's course for builder B's glacier chase (Tools/night/gen-platforming-kits.py icecourse): a standalone plan (levelgen builds and validates it) with a top-level `surfaces` list that LevelPlan24 copies into the `campaign` block (SurvivalSim.Surface26 reads it).",
            "id": "icecourse", "name": "THE ICE LINE (course)", "floorSize": 800, "killPlaneY": -30, "wallHeight": 14,
            "rooms": [room_], "doors": [], "boxes": boxes, "ramps": [], "pads": [], "grapple": [], "pits": pits, "tunnels": [], "surfaces": surf, "len": ln}


def main():
    a = sys.argv[1:]
    if a and a[0] == "--place":
        k = place(kits()[a[1]], float(a[2]), float(a[3]), a[4])
        print(json.dumps(k, indent=1))
        return
    out = os.path.join("Design", "levels", "kits")
    os.makedirs(out, exist_ok=True)
    doc = {"_why": "NIGHT26 builder M (GG: the first map is too wide; he wants platforming sections that use the wall jump and the dash). Plan FRAGMENTS in LOCAL coordinates (start z = 0, walk north, 28 m wide), one per verb: shaft (the wall jump), dash (the dash only), hop (the pad chain), hall (the wall-ride hall). Written by Tools/night/gen-platforming-kits.py; `--place KIT OX OZ PREFIX` moves one into a level plan. See the script's docstring for the splice rules.",
           "kits": kits(), "order": ["shaft", "dash", "hop", "hall"]}
    io.open(os.path.join(out, "platforming.kits.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(doc, indent=1) + "\n")
    plan = chain_plan(doc["order"])
    io.open(os.path.join(out, "platforming.plan.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(plan, indent=1) + "\n")
    ic = icecourse()
    io.open(os.path.join(out, "icecourse.kits.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(ic, indent=1) + "\n")
    print("kits:", {k: v["len"] for k, v in doc["kits"].items()}, "rooms in plan:", len(plan["rooms"]), "icecourse len:", ic["len"])


if __name__ == "__main__":
    main()

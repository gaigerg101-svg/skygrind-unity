"""NIGHT25 builder C: authored geometry kits for the rooms of levels 6 and 7 (read by gen-levels610.py). Room-local coordinates: x in -26..26 (east +),
dz = metres SOUTH of the room's north edge (0 = north wall, 48 = south wall); the spawn is at dz 42 (south), the exit at dz 4 (north) on x 0.
Each kit: boxes (kind, x0, x1, dz0, dz1, y0, y1), ramps (fromX, fromDz, toX, toDz, width, rise), pits (x0, dz0, x1, dz1, floorY), nodes (x, dz) on the floor,
high (x, y, dz) on a tier. The WARDEN is the web's shield kind (config.warden): the geometry teaches how to open it: a low wall to peek and shoot over, flank lanes
so it cannot face two ways, a pedestal to punch from."""

H3 = 3.0   # a solid cover block (the player's height: no peeking)
LOW = 1.3  # a low wall: the eyes clear it, a walker (step 0.6 m) does not


def hull(x0, x1, d0, d1, h=H3): return ("hull", x0, x1, d0, d1, 0, h)
def deck(x0, x1, d0, d1, h): return ("deck", x0, x1, d0, d1, 0, h)


KITS = {
    # LEVEL 6 ------------------------------------------------------------------------------------------------------------------------------
    "yard": dict(  # SCRAP YARD: a breath, three N-S cover lanes made of scrap walls and crates, no tier
        boxes=[hull(-16, -14, 10, 30), hull(14, 16, 10, 30), hull(-8, -5, 18, 21), hull(5, 8, 24, 27), hull(-2, 2, 8, 11),
               hull(-26, -18, 20, 22, LOW), hull(18, 26, 20, 22, LOW)],
        nodes=[(-21, 8), (-21, 36), (-8, 6), (0, 14), (8, 6), (21, 8), (21, 36), (0, 30), (-10, 38), (10, 38)], high=[]),
    "shieldwall": dict(  # THE SHIELD WALL: a low wall across the room (peek and shoot over it: a headshot breaks the shield), 6 m flank gaps at both ends (the warden
        # comes round and cannot face the wall and the flank at once), a pedestal in front of the wall (stand on it and punch the shield)
        boxes=[hull(-20, 20, 18, 20, LOW), deck(-3, 3, 22, 26, 1.0), hull(-8, -5, 10, 13), hull(5, 8, 10, 13)],
        ramps=[(0, 30, 0, 26, 4, 1.0)],
        nodes=[(-12, 6), (12, 6), (0, 8), (-20, 12), (20, 12), (-23, 22), (23, 22), (-14, 34), (14, 34)], high=[]),
    "ante": dict(  # BULWARK ANTE: the long approach to the boss door: pillars down the middle, a raised catwalk at the north end with a ramp up each side
        boxes=[hull(-10, -7, 22, 25), hull(7, 10, 22, 25), hull(-3, 3, 30, 33), deck(-22, -9, 4, 12, 3.0), deck(9, 22, 4, 12, 3.0)],
        ramps=[(-15, 18, -15, 12, 6, 3.0), (15, 18, 15, 12, 6, 3.0)],
        nodes=[(-9, 16), (9, 16), (0, 18), (-18, 30), (18, 30), (0, 26), (-6, 38), (6, 38)], high=[(-12, 3, 8), (12, 3, 8), (-18, 3, 8), (18, 3, 8)]),
    # LEVEL 7 ------------------------------------------------------------------------------------------------------------------------------
    "gatehouse": dict(  # THE GATEHOUSE: a breath: a gate wall across the room with a 12 m gap, husks come through it
        boxes=[hull(-26, -6, 22, 24), hull(6, 26, 22, 24), hull(-16, -13, 32, 35), hull(13, 16, 32, 35)],
        nodes=[(-21, 10), (-10, 8), (10, 8), (21, 10), (0, 12), (-20, 30), (20, 30)], high=[]),
    "lanes": dict(  # THE WARD LANES: dense N-S cover lanes (serpentine: each wall leaves an end open), a warden walks each lane
        boxes=[hull(-18, -16, 8, 40), hull(-6, -4, 8, 34), hull(6, 8, 14, 40), hull(18, 20, 8, 34)],
        nodes=[(-22, 12), (-22, 30), (-11, 14), (-11, 36), (1, 12), (1, 28), (13, 12), (13, 36), (23, 12), (23, 30)], high=[]),
    "fistworks": dict(  # THE FISTWORKS (MELEE ONLY): a grid of tight pillars (fists matter at 2 m), a pit on the east side with a platform beyond it:
        # the walk-round is the ramp from the middle, the shortcut is the pulse flick across the pit
        boxes=[hull(x - 1, x + 1, d - 1, d + 1) for x in (-18, -9, 0, 9, 18) for d in (26, 34)] + [hull(-19, -17, 13, 15), hull(-10, -8, 13, 15),
               deck(8, 26, 4, 12, 2.0),
               ],
        ramps=[(2, 8, 8, 8, 6, 2.0)],
        pits=[],
        nodes=[(-21, 22), (-12, 20), (4, 18), (-4, 14), (0, 40), (-22, 8), (-12, 8)], high=[(14, 2, 8), (22, 2, 8)]),
    "mini": dict(  # THE MINI GAUNTLET: three heights: the floor, a central deck (2 m), mezzanines on both walls (4 m), a ramp to each
        boxes=[deck(-10, 10, 16, 32, 2.0), deck(-26, -18, 8, 40, 4.0), deck(18, 26, 8, 40, 4.0), hull(-6, -3, 40, 42), hull(3, 6, 40, 42)],
        ramps=[(0, 38, 0, 32, 6, 2.0), (0, 10, 0, 16, 6, 2.0), (-10, 36, -18, 36, 6, 4.0), (10, 36, 18, 36, 6, 4.0)],
        nodes=[(-12, 8), (12, 8), (0, 6), (-14, 24), (14, 24), (-8, 44), (8, 44)],
        high=[(-4, 2, 24), (4, 2, 20), (0, 2, 28), (-22, 4, 14), (-22, 4, 30), (22, 4, 14), (22, 4, 30)]),
}

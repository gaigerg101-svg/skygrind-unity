# L8 reshape71 (moonshot: the arena reshapes at wave 3) - SHORT, not built: a sim change the night's view row cannot hold
**Why not tonight (read from the code):**
- A wall dropping or a pit opening changes collision. The level's own geometry is static triangles (TriangleWorld / ThreeOctree). The only geometry that leaves the world mid-run is a sim BoxLayer box, as arena42's breakable cover does (Core/Arena42.cs:11-16: "the box leaves the world").
- So the feature is a SIM row. Its bit must be fixed at StartRun, and no sim system may call ViewOn (NightRows.cs: ReelOverride's contract). design.n71.reshape71 is pre-placed in the n71 block, whose _why says "all VIEW rows (no sim, no tape bit)". Builders may not add or retype a row.
- The wall needs a place: a room, a wave, a box that blocks no door, spawn or walk line. That is room data, which R owns, and it must pass the room guards (gen-slots, levelgen --walk, n70-red). Choosing it blind, without a Player, would risk GG's play build.
**The design for NIGHT72 (about 2 h with a Player):**
1. A sim row `reshape72` with a run-constant bit, read at StartRun.
2. Its numbers in config: `{level, room, wave, min[3], max[3], dropS}`.
3. At the room's wave N (Campaign24's wave counter, whole steps), a BoxLayer "reshape72" box placed at load leaves the world. A view draws it sinking over dropS.
4. Proof: a reel enters the room with DOORFREE, logs `[reshape72] wall <room> dropped at wave 3, step <n>`, and checks the walk bot crosses where it stood.
5. Gates: golden 0 mismatches and the 16 replays exact with it OFF; fingerprints EQUAL.
**Owner:** R (the room) and B or F (the row). The design.n71.reshape71 placeholder stays OFF and unused.

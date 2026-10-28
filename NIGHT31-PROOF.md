# NIGHT31 PROOF: one line per row, RED once and GREEN once (Player unless marked "harness only")

## Freeze checks (final build n31-r7, every NIGHT31 row ON)
- Player smoke: `[smoke] 199.3s DONE: all steps passed` (also r6 199.2 s, r1 201.0 s).
- `-skygrindLevelSelect29`: `DONE 20 presses, 20 PASS, 0 FAIL`, 0 NullReference, 0 FormatException (RED on r6: timed out at press 1 with 470888 NREs from the PropsView29 boxed-float pickup31 size; fixed in the r7 commit).
- `-skygrindDoorFree29`: `[doorfree29] PASS` (r6 and r7).
- Suite: run 1 (24278f39) 1870 / 1871 (Lesson25TextTests word limit, fixed, 6/6); run 2 (d67dc294) 1871 / 1871 passed (batch, 735 s).
- Gates at every integration: fingerprint EQUAL and --off31 EQUAL (N31OnPath false), golden 97 / 97, trajectories 22 / 22, Shared 54 / 54, Tape 37 / 37, Editor compile 0 errors.

## P0
- P0 a charge-shot smoke: RED n31-r0 + chargeShots27 on: `[smoke] 247.3s DONE: 3 step(s) failed` (SHOCK PLATE, virtual pad "the trigger killed False", attract "0 kills") (Logs/player/n31-smoke-r0c-chargeON.log). GREEN: n31-r1 (6229c4a6, design.n31.charge + unlocks ON; W's fix: a full charge still held fires itself after autoReleaseSeconds 0.5, then the held trigger auto-fires): `[smoke] 201.0s DONE: all steps passed` (Logs/player/n31-smoke-r1.log, idle 123 s before). Harness charge31 (W): off = one 6 s hold 1 shot 0 kills; on = KESTREL 12 shots / 6 kills, THUNDERCLAP 8 / 7.
- P0 b node ring visible: RED n31-r2 -skygrindOff31 ring31 "the old cubes hidden in 0 of 60 frames"; GREEN n31-r2 ring31 "rings drawn 15, aimed in 60 of 61 frames, the old cubes hidden in 61 of 61 frames, the cyan mark on the node in 61 frames". Frame f0028 LOOKED AT: no cube, but at 4.5 m the node reads as a cyan block in front of a bright white plate, not as a ring with a diamond (legibility: sent to G).
- P0 c spawn 6 m from the entry: validator spawn31 (L, harness) 32 violations -> 0; Player spawn31 f9 "the nearest enemy is 8.0 m from the entry (86 deg off its facing)" (RED side: NIGHT30 frames showed husks beside the entry; the reel with -skygrindOff31 also reads 8.0 m because the node moves are unconditional). Frame shot-N30-spawn31-f9.png LOOKED AT: no enemy in view at the entry; the f1 lesson "MOVE. LOOK. WALK TO THE LIGHT." still on screen in f9 after the respawn, and the room title "1.9 PULSE" (sent to G / L / W). Open: rooms31 read nearest enemies at 5.3 m (i1a), 4.9 m (i2), 1.0 m (s1) in the Player (sent to L).
- P0 d low-health red at the edges: edge31 n31-r2 at 20 % health: row OFF centre redness 0.132 / edge 0.339; row ON centre 0.025 / edge 0.336. Frame f0107 LOOKED AT: the middle is clean, the red band at the edges is thick (about 15 % of the width each side): thinner band sent to G.
- P0 e reel on the pad: harness padreel31 (G): REEL is on X (shared with INTERACT) in all five default pad layouts; no binding change. SHORT: harness only (no pad Player line).

## Items
- 1 pull-in grapple: RED n31-r2 -skygrindOff31 grapple31 "presses 1, pulls 0, arrivals 0 ... zip speed 34"; GREEN n31-r2 grapple31 "presses 3, pulls 3, arrivals 3; the best while roped 44.0 / 44.0 / 44.0 m/s; out at each arrival 50.8 / 48.5 / 40.3 m/s; best carried into a next hook 41.0; FOV kick max 8.0 deg". Harness (G): a TAP today 0.00 m/s toward the node at 0.3 s (a hold 4.49), with the row 44.00; tape of 8 pulls replays EQUAL. Frames f0018 (mark before press) and f0034 (in the pull: FOV kick, speed streaks, the node filling the view) LOOKED AT.
- 2a charge shot: RED n31-r2 -skygrindOff31 charge31 "RT held never charges"; GREEN n31-r2 charge31 PASS: KESTREL tap fired, early release cancels (0 cooldown), full charge arc peak 1.00 slot word CHARGING, cooldown 3.5 s shown, held 3 s auto-released after 0.5 s then 6 shots; THUNDERCLAP the same.
- 2b LT always: RED -skygrindOff31 lt31 "NIGHT30's LT"; GREEN lt31 PASS: KESTREL zoom 1.50, THUNDERCLAP 1.25, NAILGUN ZOOM 1.25 then DETONATE (12 nails), gauntlet PARRY opened then COOLING on a second press. Harness ltalways31 (W): 105 LT presses, 0 did nothing (off: 49 did nothing), 0 fire/melee overlaps.
- 3 one weapon per level: RED -skygrindOff31 unlocks31 "NIGHT30's KESTREL + gauntlet start"; GREEN n31-r2: foundry start [revolver], f1 hotbar [revolver]; iceline start [revolver], i1a owned [revolver, gauntlet]; span start [revolver, gauntlet, shotgun], s1 award nailgun. Frame spawn31-f9 shows a one-slot HUD (ZOOM): LOOKED AT. Kiln: start [revolver, gauntlet], k1 award shotgun (pickup k1-P2) -> [revolver, gauntlet, shotgun].

## The camp table (item 4; PASS = every camper dies or loses 50 % in 30 s, and deals under a third of the pilot's damage rate)
| boss | build | camper | alive / 30 s | health lost | damage rate vs pilot | verdict |
| --- | --- | --- | --- | --- | --- | --- |
| THE STAG (harness, B) | 41186eee+ | centre / bank top / island top / half a lap / wall corner | all lose | | centre 0.33-0.36 %/s vs bar 0.34-0.44 (thin) | PASS 5/5 (harness only) |
| THE STAG (Player) | n31-r4 | centre | DIED 17.3 s | 100 % | 0.27 %/s | loses |
| THE STAG (Player) | n31-r4 | edge (bank top) | 30.0 s | 90 % | 0.22 %/s | loses |
| THE STAG (Player) | n31-r4 | highest (island top) | 30.0 s | 0 % | 0.00 %/s (no weapons) | SAFE |
| THE STAG (Player) | n31-r4 | farthest (half a lap) | 30.0 s | 0 % | 0.00 %/s (no weapons) | SAFE |
| THE STAG (Player) | n31-r4 | wall corner | DIED 13.1 s | 100 % | 0.26 %/s | loses |
| NIGHT30 lantern30 (harness, B) | u3-n30 rules | 4 campers | ~20 s (lava) | | <= 0.37 %/s vs pilot 2.55 | PASS 4/4 |
| NIGHT30 glacier30 (harness, B) | u3-n30 rules | 4 campers | 15-30 s | | 0 shots in sight | PASS 4/4 |
| NIGHT30 wyrm30 (harness, B) | u3-n30 rules | 4 campers | | centre 71 % | centre 3.75 %/s vs pilot 0.38 | RED 0/4 |
| NIGHT30 loom (harness, B) | u3-n30 rules | 4 campers | | corners 90 % | corners 1.96 %/s vs pilot 0.95 | RED 0/4 |
| Wyrm31 / Lantern31 / Loom31 (harness, B) | 12:08 | 4 campers each | | | Wyrm centre 0.14 vs 0.15 (thin) | PASS 4/4 each (harness only) |

| THE STAG (Player, freeze) | n31-r5 | centre | DIED 17.3 s | 100 % | 0.27 %/s (charges 3, hit 3) | loses |
| THE STAG (Player, freeze) | n31-r5 | edge (bank top) | 30.0 s | 90 % | 0.22 %/s (stomps 3 hit) | loses |
| THE STAG (Player, freeze) | n31-r5 | highest (island top) | 30.0 s | 0 % | 0.00 (no line of sight; shards 12, "11 hit", yet 0 % lost) | SAFE |
| THE STAG (Player, freeze) | n31-r5 | farthest (half a lap) | 30.0 s | 0 % | 0.00 (shards 12, "11 hit", yet 0 % lost) | SAFE |
| THE STAG (Player, freeze) | n31-r5 | wall corner | DIED 13.1 s | 100 % | 0.26 %/s (stun) | loses |
**Verdict (forge experiment):** THE STAG is SHORT in the Player, 3 of 5 campers lose; the two "safe" campers deal 0 damage (they cannot win by camping either), and the reel counts 11 shard hits on each with 0 % health lost: a shard-damage or reel-accounting discrepancy, not traced (NIGHT32 item 1). The other three bosses have no Player camp scenario: harness only.

## Bosses (pilot kill times, frames)
- **Freeze build n31-r5 (14:53, every row ON), Player:** lantern31 PASS "KILLED at 25.1 s, shield breaks 3, sparks 10, sink tell frames 60, sunk yes" (below the 45-75 s band: too easy now with the charged shot); stag31 PASS "KILLED at 115.4 s (band 80-130), wall slams 12, back punches 7, melted" ; wyrm31 "NOT killed in 240 s (bar 0.31), staggers 5, feints 11 (jumps 22), crumbles 1, full hits 13" (the reel's PASS is its drawing checks; the kill is over the 3-4 min band); loom31 PASS "joints in sight 8/8, deck camper DIED at 18.3 s, drop tells 4 (mark drawn on every tell frame)". NRE 0 in every log; frames failed to write 420 + 851 (NIGHT32 item 7).
- Player n31-r4 (12:27-12:42), every row ON: lantern31 RED "NOT killed in 200 s (bar 0.50), shield breaks 0, sparks 197"; stag31 RED "NOT killed in 180 s (bar 1.00), charges 19 (wall slams 17), back punches 0, punches 101"; wyrm31 RED "NOT killed in 240 s (bar 1.00), feints 11, staggers 0, full hits 0"; loom31 SKIPPED (needs a campaign boot; set added). Harness pilots (B): Stag 76.6 s, Lantern 137.8 s, Loom 102-124 s, Wyrm 475 s (8 deaths). The Player contradicts the harness on every boss's damage state: sent to B 12:40-12:44.

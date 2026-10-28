# NIGHT34 REPORT (for GG): the grapple bends time, the arsenal on one rail, calmer rooms built for movement

**Outcome:** everything you asked for on 2026-10-11 is built and switched ON in `Builds/n34-r4/SKYGRIND.exe` (freeze a88fb8d6). Each part
was checked in the real Player, and I looked at the frames. Smoke passed all steps, level select 20 / 20 (the three rebuilt levels
included), door-free PASS, 0 exceptions. `-skygrindOff34` gives NIGHT33's game for a side-by-side; every part is its own config switch.
Start with FEEL-PLAYTEST.md (ten minutes, the bend loop first). Two things to know first: THE STAG now dies in 10.1 s because of the
bend (below the 15-22 s band; your call, DECIDE34 N-11), and levels 4-5 were not rebuilt for movement lines tonight.

**What changed, with the frames to compare (all under `Logs/feel/`; RED = before, GREEN = now):**

| Your ask | What it is now | Frames: before / now |
| --- | --- | --- |
| The grapple triggers "a dramatic slow-motion effect" so you "switch weapons in real time" | Every latch (a node, a wall, the whip on an enemy) gives a 1 s SHORT BEND; with the meter full it is a 2.5 s FULL BEND, +0.5 s per kill, up to 5 s. The world runs at a quarter speed; you don't: look, movement, firing and swaps stay real time (the world steps 1 in 4 while you step every step). A swap lands on the press itself (0 steps, against 15 before). Kills refill the meter; time never does. | `n34-r2/red/bend34/shot-N30-bend34-f2-plus0.2s.png` / `n34-r2/bend/bend34/shot-N30-bend34-f3-kill.png` |
| "A clear visual overlay ... distinct visual cues or color overlays" in slow motion | The world goes cold grey and the sky dims; enemies, danger and cyan keep their colour; one step deeper per kill; a small FOV thump on entry. No border lines. Colour returns over 0.3 s. | `n34-r1/red/hud34/shot-N30-hud34-1080-bend.png` / `n34-r1/hud/hud34/shot-N30-hud34-1080-bend.png` |
| "Switch weapons ... chain fluid, expressive combos" | Kills with two or more different guns inside one bend name a combo in the top slot (BEND BREAKER, TRIPLE BEND, FULL ARSENAL) with a style bonus. Airborne and sliding kills pay up to 3x points and style (never damage). | (none) / `n34-r3/hud/hud34/shot-N30-hud34-1080-combo.png` |
| "Remove that little diamond indicator" | Gone. The node's ring is the only grapple marker. | `n34-r1/red/hud34/shot-N30-hud34-1080-nomark-wall.png` / `n34-r3/hud/hud34/shot-N30-hud34-1080-nomark-wall.png` |
| "An ULTRAKILL-style weapon display that shows your full arsenal" | The ARSENAL RAIL, bottom right: every gun you own as an outline; the one in hand cyan, larger, underlined, its name and LT action above. The highlight slides on the press; a holstered gun coming off cooldown pulses once. (ULTRAKILL itself shows only the current gun, so this is its look plus an always-on strip.) | `n34-r1/red/hud34/shot-N30-hud34-1080-rail5.png` / `n34-r1/hud/hud34/shot-N30-hud34-1080-rail5.png` |
| "Cluttered with detail for the sake of detail" | 73-81 % of the decoration props are gone on all five levels (ribs, banners, pipes, skirts, cornices, edge lights); cover, climbs, devices and lights stay. | `n34-r3/redlines/lines34/shot-N29-N34-lines34-f5b-a.png` / `n34-r3/lines/lines34/shot-N29-N34-lines34-f5b-a.png` |
| "The arenas don't incentivize creative movement" | Levels 1-3 (foundry, iceline, kiln) rebuilt as n34 files: 17 fight rooms, 15 with grapple nodes (none before), and 11 passing every movement-line check (a ride wall >= 9.6 m, nodes in reach, two or three floors, a 21 m clear lane, an enemy climb route to every floor). The six short ones: three lava-lesson rooms and the slide corridor (lane too short; the corridor also has one floor), and iceline i2 / i5 (short lanes, no nodes: their scripted lessons). Walk bot 10 / 10 on all three. | `n34-r3/redlines/.../f5b-c.png` / `n34-r3/lines/.../f5b-c.png` |
| Movement, responsiveness and flow "dramatically better" | From open-source patterns (ideas, no code copied): a wall jump just after a ride ends still works (0 -> 12 of 16 late presses), an early press waits for the wall (0 -> 10 of 14), a zip release keeps your sideways speed (-1.6 -> 10.4 m/s), wall rides hold height at the start, one air steer after a jump. Every press already acts on its own step (measured). Bunny hops keep 100 % speed. | (numbers) |
| The Pulse Gauntlet "a slightly larger active hitbox when striking bosses like the worm" | The punch and blast land 0.6 m outside any boss's body (a hit at that edge now damages and staggers the worm; it missed before). | (numbers) |

**Addendum (2026-10-11 ~18:00, after REA connected): what ULTRAKILL's own code taught us.** Your installed ULTRAKILL was read for
numbers and rule shapes only, with no code copied (Research/2026-10-11-wave34/research/rea-ultrakill.md). REA classified the file, but its
.NET reader stops at a 64 MB limit on ULTRAKILL's code, so ILSpy did the reading. Four things went in, each a switch, all in
`Builds/n34-r5/SKYGRIND.exe`:
- **Slide into a wall:** a jump while sliding into a wall now bounces off it at full speed, as in ULTRAKILL. At 45 degrees: 11.6 m/s
  away from the wall, where it used to be a 9.8 m/s slide jump along it (`design.n34.flow34.slideBounce34`).
- **ARSENAL:** killing an enemy that two or more of your guns hit pays style and shows "ARSENAL" (`design.n34.combo34.arsenal`).
- **QUICKDRAW:** a hit within 0.25 s of swapping in a gun pays style and shows "QUICKDRAW" (`...combo34.quickdraw`).
- **The whip readies the gauntlet:** a whip latch makes the punch ready in 0.09 s instead of 0.28 s, so grapple -> punch always
  links (`...combo34.whipReady`).
Also learned: ULTRAKILL never slows time on its grapple or in boss fights (its slow-downs slow everything, briefly), its weapon wheel
slows time to a quarter while held, and its swaps are instant. n34-r5: smoke all steps passed, hud34 and flow34 reels PASS, 0
exceptions; golden 0 mismatches, fingerprint EQUAL.

**Bosses in the Player (bot):** Lantern 21.4 s, Stag 10.1 s, Wyrm 18.6 s, Loom 21.3 s, Crown 19.6 s (NIGHT33: 18.3 / 17.6 / 19.0 s).

**How I worked:** one shared commit made every change switchable, then four builders (bend, look + HUD, arenas, movement) worked in
parallel for three rounds. In every round I ran a Player build and looked at the frames. Things the Player caught that the tests did not:
my own reel script booted no campaign level (fixed), the flow reel's bhop press became a double jump (fixed), and grapple nodes made
the walk bot's span pilot take over rooms (A fixed the pilot and put the nodes back). Research (four web passes) is in
Research/2026-10-11-wave34/.

**Not done or not checked:**
- Levels 4-5 (span, spire) keep NIGHT33's rooms with the calm pass (span with nodes broke the walk bot, 8 / 10).
- The bend's length was not measured in game seconds in the Player (the reels' wall time includes writing frames).
- The REA reverse-engineering server never connected, so no reverse engineering was used; DECIDE34 R-1.
- A held-bumper weapon wheel (leftover) was not built.

**What you decide (DECIDE34.md, every default already in the build):** the bend's speed (N-1, a quarter); a bend on every latch or
only the full one (N-2); split or global (N-3); the Stag's 10 s (N-11); the calm pass's 73-81 % (N-7); the air lurch's speed cost (N-8);
levels 4-5 next (N-9); REA targets (R-1).

**The one thing to play first:** SURVIVAL or level 1, grapple to a node with enemies near, swap twice and shoot during the grey, and
see if it feels "heavy and intentional".

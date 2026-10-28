# CODE-FACTS A (wave 43): movement and level 1 (G1-G5, G10, G11, G13), read from code and config (no game run)

Read-only sweep by the lead's agent, 2026-10-16 ~16:20. Paths relative to the repo; 120 Hz. "derived" = arithmetic from code numbers.

## 0. Sources
- Design/unity-config.json is the source (Assets/Editor/SurvivalSetup.cs:116-120 copies it to StreamingAssets; byte-identical today).
- Level 1 = n42-foundry (Curriculum25 N41Levels :174-189 foundry41, N42Levels :193-206 foundry42). Built from Design/levels/n42/foundry.plan.json,
  written by Tools/night/gen-n42-foundry.cjs (copies the n41 plan, edits only b2). Level 3 = n41-kiln (kiln41).

## A1. The ride: attach, detach, snap
Rows on: wallride35, flow34, wallgrip36, ride39, ride40, wallfling42 (ride38 off; ride40 feeds its hold numbers, SurvivalSim.N40.Ride.cs:25).
- Contact entry: a capsule contact with |normal.y| < 0.3 (Mover.cs:1690-1693) -> CheckRideEntry (:1508-1518) -> Ride35Entry (:1523-1549) ->
  Ride40Intent (:2094-2129; unity-config :5620): the stick INTO the wall (dot >= 0.1; no stick = no ride), horizontal speed >= 4, >= 1 m clear
  of the ground, the face 2 m above the feet (minWallHeight 2), a 0.2 s lockout on walls facing within 30 deg. Jump need not be held.
- Re-ride: a wall within 30 deg of the last one is refused within 3 m of where that ride ended, until a landing (:1528-1536; :3666-3669).
- Entry gate: along-wall speed > 1 m/s with jump held, else > 4. A head-on held entry rides the way the camera faces (:1537-1548).
- Near-miss draw (jump held): Ride35Attach (:1886-1920) rays reach 0.35 + 0.5 m; raises the into-wall speed to 4 m/s (attachPull) that step.
- Probe 0.12 m (PortExport :94; ProbeWall :1787-1807; FindNearWall :1810-1821).
- The snap: StartRide (:1114-1167) turns the full horizontal speed onto the wall line (max(clipped, preMoveSpeed) :1128); a falling vy = 0;
  wallgrip36 clamps a rise to 1.5 m/s (attachUpMax :1143-1150, :3892): a jump at ~8 m/s up stops at once. +1 m/s into the wall per step (:1449-1453).
- Vertical: a rising entry brakes at full gravity; then Ride38Hold (:2065-2088) gravity 0, vy by look pitch (Ride38.cs:60-70): 0 inside +-15 deg,
  +-2.5 m/s at 45 deg. A rise is cut at the wall's top (topProbe 0.25). No max time (ride40's 60 s guard).
- Ends: away (stick dot > 0.5, :1433); lost (no wall > 0.05 s, lostGrace :1989-1994); corner (> 100 deg, or any before 0.35 s, cornerMinHold
  :1972-1980); blocked (along speed under the gate :2006-2010); land; dash; slam; launch; jump release on a held ride = the jump-off (:1874);
  **B = "drop"** (:1867-1872); no-ride boxes (:1526, :1906).
- After a non-jump end: that wall locked until out of probe reach (rideLockActive :1172-1176, :1497); the 0.2 s late-jump window (:1198-1203).
- Ride floor speed 8 x 1.15 = 9.2 m/s (:1445-1446; :5407).

## A2. Wall jumps and the cap
- (a) A pressed wall jump (tap ride or air near a wall): TryJump -> ResolveJump / AirJump (MovementMath.cs:183-206) -> Mover.cs:1009-1024:
  along speed kept + 5 out (wallfling42; web 8), up 1.0 x v0 = 8.33; spends a wall jump (:1015), refreshes the double jump, locks that wall 0.15 s.
- (b) The held-ride jump-off (Ride35JumpOff :1924-1956): 8 out, up 4.6 (45 deg down) to 10 (45 deg up) (Ride35.JumpUp :155-163), +1 exit boost,
  capped at max(16, ride speed) (Boost39 :179-187); a ride under 0.3 s caps out at 5 (rideOutMax :1932-1937). **Spends no wall jump.**
- **The cap**: maxPerAirtime 3 (PortExport/config.json:103; MaxWallJumps Mover.cs:264), reset ONLY by a landing (Land() :1599). StartRide refreshes
  only the double jump (:1132). Spent by (a) and the slide bounce (:1842). wallCoyote / wallBuffer / slideBounce need wallJumpsLeft > 0 (:927, :959, :968).
- Cap empty: a press on a tap ride / near a wall becomes the double jump (:204): EndRide("jump") + 7.5 straight up, no push off (:1025-1030); then
  presses do nothing until a landing. **G13: THE STACK's sections of 6 panels between ledges (ledgeAfter [2,8], :4184) allow only 3 pressed
  wall jumps.** Held-ride releases are unlimited.
- Other limits: re-ride (30 deg / 3 m), ride40 lockout 0.2 s, rideLock, the 0.15 s same-wall lock; THE STACK's hot shaft wall throws you off
  (Launch 5 m/s, 5 dmg, 0.5 s; SurvivalSim.Wick37.cs:226-252); the shaft air clamp 12.4 m/s (:253-261).
- wallfling42 (:6171-6188): an air press near an unridden wall wall-jumps only with the stick into it (dot >= 0.35) and no landing predicted within
  0.1 s (Wall42Allows Mover.cs:348-361, :921-925); a slide bounce needs 30 deg into the wall (:339-344).
- Coyote / buffer: floor coyote 0.1, jump buffer 0.1 (PortExport :42-53); wallCoyote 0.1 (:1178-1182); wallBuffer (:959-964, :2146-2157);
  lateJump 0.2 s (:3670); predicted-landing hold 0.1 s.

## A3. Pad B, slide, dash on a ride
- Pad v2 (design.pad.n19; Core/Gamepad.cs:276-292): B = Slide (ground slide, air slam), LB = Dash, LT = AltFire (zoom; "Aim on a wall slows time").
- B on a ride: drop at once (Ride35Actions before other actions :516, :1867-1872), press consumed; a jump within 0.2 s still jumps off.
- **B just after leaving a wall = a SLAM** unless a landing is predicted within 0.1 s: velocity (0, -48, 0), every horizontal m/s lost
  (TrySlam :1071-1093, :524). The likely "conflict with B".
- slideBounce34: a slide into a wall within 4 steps + jump reflects at full speed, spends a wall jump (:967-973, :1826-1857).
- Dash on a ride: allowed (:861); direction = the stick reflected off the wall, or 45 deg off it toward the camera (MovementMath.WallRideDashDir
  :228-249); ends the ride ("dash", :879, locks the wall); 24 m/s x 0.25 s (~6 m), no vy (StepDash :1424); after: speed max(8, before) (:902-913).
  Stamina bar 1.0 (+1 bonus), a dash 0.5, floor 0.25, refill 0.5/s after 0.4 s (:491-499).

## A4. Speeds
Walk 8 (accel 10, friction 6); sprint 10.5; air accel 1.5; max 40; jump 1.5 m, apex 0.36 s (g 23.15, v0 8.33, fall x1.35); double jump 7.5;
dash 24 x 0.25 s; slide floor 11.6, slide jump cap 24; ride floor 9.2; chainCap 16. Leaving a wall (derived): held jump-off ~13.2 m/s (16 on a
chain; 16.52 measured once, NIGHT41-LOG.md:218); pressed wall jump ~10.5.

## B5. Level 1 rooms (n42-foundry)
a1 1.1 INTAKE link 12x31 h6 · a2 1.2 KESTREL 14x20 h7 [husk,husk] · a3 1.3 RIDE 16x26 h9 [husk,husk] lava pit 14x9 · a4 1.4 PARRY 8x19 h6
[spitter],[spitter x2] · a5 1.5 CRUCIBLE 20x24 h10 3 waves lava 12x11 · b1 1.6 THE YARD 36x40 h12 3 waves · **b2 1.7 THE GANTRY** link 20x104 h14 ·
a6 1.8 REST 10x10 · **a7 1.9 THE FLUE** link 12x32 h13 · **a8 1.10 PIERCE** link 14x22 h9 · a9 1.11 THE STACK boss 28x28 h50. Doors d1-d10
4 x 4.5 m, each a checkpoint.
- THE GANTRY (b2; plan zone b2): two gates z -192..-229 and -241..-278, each **37 m**, a **dry** pit 4 m deep (floor y -4), **no lava**; lane 6 m
  (faces x +-3); W1 west -6..+22 m from the lip, W2 east 8-36.5, W3 west 26-36.5 (W1 / W2 overlap 14 m); a ceiling 3.6 m over each pit; NORIDE liners.
  No respawn line; a fall costs nothing; out by a RETURN STAIR (tops -2.8 / -1.6 / -0.4) + side steps (gen-n42-foundry.cjs:101-117).
  Prompt "HOLD {jump}: CHAIN THE WALLS."

## B6. THE FLUE (a7), the three-wall climb
- F1 x -3.5 y 0-3.5 z -316..-308; F2 x +3.5 y 2.8-6.3 z -323..-315; F3 x -3.5 y 5.6-9.1 z -330..-322 (8 x 3.5 m each); each 2.8 m higher,
  **7 m apart plane to plane, overlapping only 1 m along z**. A deck at y 8 (z -335..-332). Verb: the held-ride release ("LOOK UP, RELEASE {jump}:
  LEAP"); cleared at y >= 7.5.
- Derived: 6.3 m across at ~8 m/s out = ~0.79 s, while the along speed (9.2-10) carries ~7.3 m along z: a leap from F1's far end lands past
  F2's end. Under 0.3 s on a wall the out speed caps at 5: ~1.26 s, ~11.6 m along. Attach bands: F1 feet 1.0-1.5 m, F2 1.35-4.3; a level-look
  jump-off (7.3 up) falls below F2; ~+45 deg (10 up) arrives at the band's bottom.

## B7. The ride-and-shoot lesson (G4) = a8 PIERCE
- Campaign24.Enter spawns 3 **stand-in husks** (Campaign24.cs:570-572; roomRules29) **floating at y 1.8 over the 4 m dry pit** at (-2.15,-354),
  (-1.4,-357), (-0.65,-360); never tick (Husks.cs:355; SurvivalSim.Systems29.cs:41-48). Lesson "RIDE, HOLD {fire}: PIERCE" on entry. Panels P1
  x -4 z -354..-346, P2 x +4 z -361..-353, y -4..3.2.
- a3 RIDE: arena36 waves 1 s after entry (SurvivalSim.Level36.cs:7-11, :169-201); "HOLD {jump} ON A WALL: RIDE" on entry; teach35 l1.zoom on P1.
- In arena36 rooms spawns33's trigger lines are bypassed (Level36.cs:161): waves start at entry, not on a verb.

## B8. a8's exit and the pink (G10)
- d10 (z -368..-364) is the boss door: pale violet-white 0xCFC6FF (UnityTuning.Doors38.cs:9), a violet light beyond (Doors38View.cs:383); the
  code notes it "read pale white / pink" in a NIGHT39 frame (Doors38View.cs:341-343). doors41On draws it.
- THE STACK light 0xd8d0ff (UnityTuning.Wick37.cs:22); the boss entrance flicker splits red / blue (UnityTuning.Juice38.cs:22,27) = a magenta
  fringe, on bossRise at room entry (Wick37.cs:221; Juice38.cs:146, :195-208). The LT focus CRT tint (SlowAim27.cs): not checked.
- Past the door: a tunnel ~4.4 x 4.5 x 5.8 m (Wick37Stack.cs:115, :159-163), then the shaft wall (r 9, centre z -382). On entry a 0.12 s hitstop
  + shake 2.5 (juice38 :5341); inside the shaft the 12.4 m/s air clamp. Brick-wall candidates: the tunnel, the hitstop, the clamp.

## B9. Room cleared -> door (G5): Campaign24.StepFight (:735-790), Clear() (:872-873); opens when alive == 0 and lessons done
1. **alive counts every enemy in the LEVEL** (EnemiesAlive = enemies.AliveCount, SurvivalSim.Campaign24.cs:75; Combat.cs:136-144) + pending33.
2. Stand-ins count as alive (Systems29.cs:43).
3. **Bodies that never die**: kill plane y -30 (killPlaneY; Husks.cs:367); a husk knocked into a dry 4 m pit (b2, a8) or stuck under geometry stays
   alive; only lava kills (Lava30.cs:7).
4. The top-up loop: an enemy-needing lesson not done respawns the last wave up to 12 times (Campaign24.cs:762-777; maxTopUps :1320).
5. Queued / tell enemies count (:745; Level36.cs:165); a failed queued spawn is dropped silently (Level36.cs:197).
6. Gaps: waveGap 0.8 + nextWaveS 1 + switchOn 1; start40 holds the level's first wave (6 m or 4 s; N40.Start.cs:95-111).
7. A retry kills all and restarts at wave 0 (:918-936) without re-spawning stand-ins.
(CODE-FACTS-B adds: a lesson that needs a verb can block the clear forever.)

## B10. Backtracking (G11)
- Entering the next room's zone (feet + 1 m) locks the door behind (Campaign24.cs:551): a gate collider + nav block (SurvivalSim.Campaign24.cs:102-106).
- Inside a room nothing stops a turn back: a7 clears at its deck (y >= 7.5) and the climb (F3 -> F1) stays open behind it.

## B11. The boss THE WICK (Wick37.cs, Wick37Stack.cs, SurvivalSim.Wick37.cs; design.n37.wick37 :4179)
A round shaft r 9, 15 ride panels in a spiral (4 per turn, each 2.8 m higher, 8 x 3.5 m), ledges after panels 2 and 8, a cap; the hot shaft
wall; slag rising 0.6 m/s (30 dmg). Relies on riding panel to panel: "RIDE A WALL, HOLD {fire}: SHOOT THROUGH A CRACK".

## B12. Dash first taught: a1, teach35 l1.dash (z -11..-8, feet y >= 0.5): "PRESS {dash} to DASH. Works in the air." before the 7 m DASH trench.

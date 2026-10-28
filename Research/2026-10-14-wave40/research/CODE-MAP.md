# Wave 40 code map (read 2026-10-13 ~22:00 at a659d7d0, u3-n39; two read-only explorers + the forge's spike). Re-check before use.

Config = `Assets/StreamingAssets/Design/unity-config.json` (and its byte-identical twin `Design/unity-config.json`); web = `PortExport/config.json`.

## The campaign (Core/Curriculum25.cs:12-36, swaps N35 campaign6 -> N36 -> N37 -> N38Levels :131-144)
| slot | level id | row | boss (room) | brain |
|---|---|---|---|---|
| 1 | n38-foundry | foundry38 ON | THE WICK (a9) | Boss37 |
| 2 | n37-iceline | iceline38 OFF (n38-iceline built, unused) | THE STAG (s4 THE CANAL) | Core/Enemies/Stag37.cs + Stag37Arena.cs |
| 3 | n38-kiln | kiln38 ON (K4, NIGHT39) | THE KILN WYRM (wyrm39-arena.json) | Wyrm (AI frozen) |
| 4 | n38-span | span38 ON | THE LOOM (o3 THE CANYON) | Core/Enemies/Loom37.cs on Boss37 |
| 5 | n38-spire | spire38 ON | THE CROWN (v11) | Core/Enemies/Crown37.cs + Core/Orbit37.cs |
| 6 | n35-finale | finale38 OFF | "glacier" = Stag31 on Rink28Arena | Core/Enemies/Stag31.cs |
- Live unlock table: `design.n31.unlocks` (start `["revolver"]`); unlocks.json and design.maps.list are legacy. Level select:
  `n26.levelSelect` slots 10, playable 6 (UiLevelSelect26.cs:71, UnityTuning.Lead35.cs:6); slots 7-10 show LOCKED. maps.list still
  labels legacy entries "LEVEL 6 CHARGERS AND LANCERS" .. "LEVEL 11 FINALE". After the last level: "CAMPAIGN COMPLETE"
  (UiCampaignResults25.cs:191).
- Weapons by level (Unlocks31.StartFor, Core/Unlocks26.cs:79-88): L2 starts KESTREL, PULSE gauntlet awarded entering i1a
  (SurvivalSim.Weapons31.cs:140-144 -> Campaign24.Award31 :494). L3 starts KESTREL + gauntlet; THUNDERCLAP in kb1 (unlock row 24 + the
  room's `"award":"shotgun"`). L4 starts KESTREL + gauntlet + THUNDERCLAP; STITCHER (`nailgun`) in p1 "4.1 STITCH YARD" (pickup
  [0,1.1,122.5] + `"award":"nailgun"`, lesson "STITCHER: HOLD TO STREAM.", teach35 line [13]). A room award fires on clear
  (Campaign24.cs:850-853). Weapon ids: revolver, shotgun, gauntlet, nailgun.

## The start of a level (ask 1)
- Campaign24.cs:630-634 enters room 0 on the first step the feet are in its zone (once `run.Live`; the break phase is skipped,
  SurvivalSim.Campaign24.cs:177,184). Enter (:546-579) sets `gapLeft=0`, so StepFight (:746-758) spawns wave 0 on the first fight step:
  **no delay**. waveGap 0.8 s (`design.n24.campaign.waveGap`) only between waves.
- spawns33 (ON; SurvivalSim.Spawns33.cs:140-154, 215-260): tellSeconds 0.6, waitSeconds 12, minSpawnDist 8, tellStagger false;
  `triggers33` keyed by room id (f9, f10, k2, i6b, s7, v8: only i6b is on a live level). Rooms whose lesson is not "kill them all"
  never hold a wave (:227).
- L1: a1 has no waves; a2's [husk,husk] spawn on entering a2. L3 and L4: room 0 has waves, so enemies spawn at once.

## The THUNDERCLAP boost and its text (ask 2)
- Live path: Core/Guns/Sandbox19.cs:227 (Arsenal19On via design.arsenal.n20) `if (!mover.grounded) mover.velocity -= aim * sgJump`;
  the slug: Core/Guns/ChargeShots27.cs:301. jumpSpeed 10 (web weapons.shotgun.jumpSpeed). Level 3's climbs: three shots + the double
  jump (NIGHT39 K4, gates 12 m).
- Strings today: kb1 "GRAB THE THUNDERCLAP", "JUMP, SHOOT {fire} DOWN: BOOST"; kb1b "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN."; kb2 "JUMP IN.
  SHOOT {fire} DOWN. FLY."; kb2b "CLIMB."; kb3b "CLIMB IT ON THE RUN."; teach35 [12] "THE THUNDERCLAP. PRESS {fire} to BLAST.",
  [7] l3.charge "HOLD {fire} to CHARGE a SLUG. Release to fire.". No line explains the boost's steps.

## Weapons, charge, the HUD (asks 3-6)
- KESTREL (p39 ON): tapS 0.15 (tap fires on release; Charge35.cs:159, 378-505); interval 0.4 x intervalMult 1.2 = 0.483 s
  (:554-555); full charge `design.n31.charge.revolver.fullSeconds` 0.5 (x4 damage, pierce 3), held full +1.0 s fires itself
  (`design.n35.charge35.holdMaxS`); charged cooldown 2.5 s (`design.n35.charge35.revolver.cooldownS`).
- KESTREL spin: UiHud38.cs:265 -> UiHud38.Kestrel39.cs (p39 + `UnityTuning.Hud39KestrelSpin`): one turn through the charge then a snap; a
  quarter turn (`Hud39TapSpinDeg` 90) through each tap interval. hud38 base spin 360 deg per cooldown, 0.12 s snap (UnityTuning.Hud38.cs:30-33).
- KESTREL view: Game/Pistol39View.cs (kick :205-231; numbers UnityTuning.Pistol39.cs): tap kick 9 deg, attack 0.03 s, settle 0.32 s;
  camera 1.2 deg / 0.22 s; cock 0.05 / 0.14 s; cannon 24 deg / 0.6 s, camera 3.5 deg / 0.45 s, FOV +7 deg 0.3 s. The NIGHT36 hammer
  still runs (UnityTuning.Guns36.cs:41): fall 38 deg, hold 0.05 s, re-cock 0.2 s.
- **The THUNDERCLAP already charges** (tap36 rule, NIGHT36): the tap fires on the PRESS; a hold past holdStartS 0.18 s charges;
  fullSeconds 0.7 (`design.n31.charge.shotgun`), slug x3, cooldown 3.0 s (`design.n35.charge35.shotgun.cooldownS`). GG did not find it:
  the ask is to make it real to the player (rule, look, HUD), not to invent it.
- THUNDERCLAP interval 0.9 x 1.3 (design.n24.shotgunTune) x 1.3 (design.n29.shotgun.fireIntervalMult) = ~1.52 s (Charge35.cs:560).
  Its view: the pump in Game/Viewmodel.cs:619-625, 839-914 (pumpDelay 0.45 s after the shot, pumpDuration 0.3 s, travel 0.06, tilt 5 deg;
  web viewmodel.*); kick springs UnityTuning.Hands.cs:178-179 (A 4.2 Hz zeta 0.5 pitch -7 deg back 0.17 m; B 11 Hz).
- PULSE gauntlet: no hold charge; PulseCharge27 (`design.n27.pulse27`): 5 kill pips, the LT blast, 15 s cooldown.
- The three bars: Game/UiHud38.cs:160-176 (BuildHud38, `UiStyle33.SlashBar` Images, `fillAmount` at :328); UnityTuning.Hud38.cs:13-27
  (`Hud38Segments` 3, 108 x 10 px, 8 px gaps, Y -30; bone charging, cyan full, white flash 0.35 s). `design.n38.hud38` ON.

## The wall ride (asks 11, 12)
- Live: wallride35, wallgrip36, ride39 ON; **ride38 OFF** (NIGHT38 M: the hold ride keeps its height, holdSink 0, a 60 s guard, the
  pitch past +-15 deg drifts up / down at up to 2.5 m/s, never past the wall's top; OFF for "level 1 a5 walk deaths 2 -> 9, unexplained";
  its `_why` quotes GG's w37 "you don't actually fall down you just stay on forever"). Core/Ride38.cs.
- The slide today: Grip36.Gravity (Mover.cs:508-513): a rise brakes, the grip holds level 0.35 s, then a tap ride sinks (sinkAccel 1.5,
  sinkMax 1.5 m/s), a hold ride sinks at sinkSpeed 0.1 m/s (:499-506). Ends: a tap ride at `wallRide.maxTime` 1.2 s (:1070, :1489); a hold
  ride at safetyCap 8 s (:1094, :1885-1888).
- Attach: airborne (:1417-1421); |normal.y| < 0.3, contact inside the capsule height (:1617-1620); along-wall speed > entrySpeed (1 m/s
  with jump held, 4 m/s without; :1466-1467); a head-on hold entry redirects to the camera side (:1468-1476); Ride35Attach (:1814-1846)
  ray-casts radius + attachReach 0.5 m and pulls at attachPull 4 m/s with jump held and speed >= 1; attach up speed <= 1.5 (:1085-1088);
  blockers: noride boxes (:1455, :1832), the re-ride rule (30 deg, 3 m); riding probe 0.12 m (:1719). **No minimum wall height, no
  ground clearance, no input-toward-wall test**: why stair risers and platform faces grab.
- ride39: speedBuff 0.15 (floor 9.2 m/s, :1375), exitBoost 1, chainCap 16 + capJumpOff (:1855), upLow 4.6, upHigh 10.
- noride: a campaign box `"noride": true` (Ride38.NoRideFrom :107; Mover.cs :1455, 1832, 1963, 2012), set per run
  (SurvivalSim.N38.Ride.cs:23). Counts: span 7, spire 8, kiln 10, foundry 0, iceline 0, finale 0; no arena json has any. The look
  NoRide39View.cs (UnityTuning.NoRide39.cs) behind `n39.ride39.noRideLook=false`: OFF, never seen on screen.

## The bosses (asks 8-10, 16, 20)
- **THE LOOM** (L4; Loom37.cs, SurvivalSim.Loom37*.cs, Loom37View.cs, Bot/Loom37Pilot*.cs; design.n37.loom37 + boss39 loom: stagger
  [6,7.5,7.5], rest 2.5 s). Room o3 x[-39,5] z[-390.5,-250.5]; stations [-17,20,-302.5] [-17,20,-338.5] [-17,18,-359.5]; body forced to
  y 20; `stationSpeed` [36,40,36] m/s (code default {5,8,0}). **Why it leaves the map** (Loom37.cs:425-430): the anti-hide sets
  `stations[leg].z = player.z - 6`, unclamped to the room, never restored; the hide timer fills when the player is > 15 m below or > 20 m
  away (common with the body at y 20 and ledges at 0-8). Death: Fall() 23 m/s2 for 2.5 s (~72 m), under killPlaneY -40. Model: code
  primitives (Loom37View.cs:60-63), no Animator, a bob + a limp torn knee + loom38 breathing; 10 Boss37State states. Damage: full only in
  the CYAN window; knees torn by `nailsToTear` STITCHER nails; o2 has a practice knee (kneeTarget37); a pull-away tear exists. Its
  spool projectile is parried in Loom37.Punch. **The STITCHER is the LOOM's key: removing it (ask 7) moves the tear to the grapple.**
- **THE CROWN** (L5; Crown37.cs, Orbit37.cs, SurvivalSim.Crown37*.cs, Crown37View*.cs; design.n37.crown37; arena = v11 spire37 data from
  Tools/night/gen-n37-spire-boss.cjs). Hovers between the player and the next piece (HoverPoint :498; moveSpeed 11, hoverDistance 5,
  lockRadius 8); the anti-hide snaps onto the player (:407); no clamp. Damage, "THE PASS": the chest crosses the ring plane within 2.4 m,
  airborne, >= 12 m/s (a jump-dash): passGems 1 (2 with the TORCH), stagger -2; weapons do nothing outside the CYAN window; parrying the
  YELLOW gemShard staggers; leg 3's finisher a KESTREL charge >= 60 or a slug >= 300 through the ring. boss39: gems 8 -> 20, legGemsLeft
  [13,6], stagger [4,4,4]. **No boss line explains any of it** (bossLine30 is used only by Lantern30); v10 THE RING has an empty prompt.
- **THE STAG** (L2; Stag37.cs, Stag37Arena.cs, SurvivalSim.Stag37*.cs, Stag37View*.cs; design.n37.stag37): pool 900 x4.5 (boss39), stagger
  [6,7,8], window 2 s; runs 23.5 m/s, leash 50; damage = catch it skating >= 8 m/s within 4 m and punch (stagger -2), the CYAN window's
  launch punch; guns chip x0.1; leg 3 parry the antler charge.

## Level 2 today (ask 13)
- n37-iceline: i1a TANK ALONE (the gauntlet pickup [0,1,-4.5]), i1b, i1r RIDE HALL, i2 PULSE ROOM, i3 REST, i4, i5, i6, i6b, i7 REST (no
  job tags), s1 LANES / s2 SLIPSTREAM (rehearse), s3 THE GATE (link), s4 the boss. n38-iceline (OFF) has a beat sheet
  (Design/levels/n38/iceline.beats.md: j1 THE GAUNTLET teach, j2 THE RINK combine, j3 THE RUN test, j4 REST, s1-s4).

## Enemies (asks 15, 19)
| kind | brain | speed | attack | tell (web -> n36.tells36) |
|---|---|---|---|---|
| husk | Core/Husk.cs + HuskBrain.cs | 6 m/s | melee 15, hp 40 | 0.5 -> 1.0 s |
| swarmer | Core/Swarmer.cs | 10 m/s | bite 8, hp 10, no parry | 0.3 -> 0.6 s |
| spitter | Core/Spitter.cs | 4.5 m/s | 3 lobbed globs 12 dmg at 16 m/s, puddle 10 dps | 0.6 -> 1.0 s |
| tank | husk + n29.tank | - | hp 600, scale 1.4 | - |
- **Ported web enemies no campaign level 1-5 places** (found by the red-team; only legacy level4-level10.json use them): DRONE
  (Core/Enemies/Drone.cs, NIGHT19: hovers 4-8 m, keeps 12-18 m, charges 0.4 s, an orb every 2 s, a punch reflects the orb at the nearest
  enemy), WARDEN (Warden.cs, NIGHT20: 200 HP, a front 120 deg shield; flank, from above, ricochet, or punch it = a 1 s stun; a 0.8 s heavy
  tell, swings for 20), CHARGER (Charger.cs, NIGHT19: "the heavy", 150 HP, a 0.8 s tell with a ground line, a 22 m/s charge for 25 + a
  knockback; a wall stuns it 1.5 s with its back x3), LANCER (Lancer.cs, NIGHT20: a perched sniper, a lagging laser 1.2 s then a 25 beam).
  BULWARK is a ported BOSS id (Core/Enemies/Bulwark.cs, Campaign24.cs:324): never reuse the name for a kind.
- **"heavy"** (in kiln and spire waves) maps to nothing: a plain husk (Husks.cs:220, 225, 278). A silent fallback (L-059's kind).
- Models: Game/EnemyViews.cs from primitives (husk = a capsule + a cube head :430-435; swarmer three boxes; spitter ~:756).
- Parry: Core/Combat.cs:1347 -> SpitterAcid.TryReflect (Spitter.cs:531) -> a Glob, else TryReflectOrb (drone orbs :581). Parry36
  (design.n36, L3851): 0.12 s buffer, 0.25 s freeze, the glob returns to its shooter, 40 dmg in 3 m; a punch in a melee wind-up staggers
  and heals 15. Boss projectiles parry in their own code (Loom spool, Crown gemShard, Stag chunk / antlers).

## "White rods" candidates (ask 18; all view only, no collider; identify on a frame first)
- dress33 decor (Tools/night/dress33.cjs:5,66 writes `decor:true, collider:false`; drawn by Game/Kit33Props.cs; calm34 keeps chain, truss,
  lamp): chain 0.12 x 1.95 m; truss 17.8-25.8 m spans at y 8.7-11 across rooms (chords 0.25 m); edgeLight 0.1 x 0.06 m warm-white bars at
  3 m; lampPost 0.16 m; wallPipe. Present in n35-finale, n37-iceline, n38-kiln (whose climbs reach 12 m: a truss at 8.7-11 m is in the
  flight path). Colours UnityTuning.Kit33.cs:11-32.
- Pole lamps: Game/LightPlan33.cs:645-672 via Fixtures33.cs: 0.12 x 2.6 m poles in room corners (inset 0.6 m), rooms >= 12 m.
- Light shafts: warm crossed quads from the ceiling panel to the floor, 0.35 m wide (LightPlan33.cs:438-443, LightKit29.Beams33): a pale
  vertical "rod" the player walks through is the likeliest match.

## Disk and memory (22:00)
- C: 1.9 T, 22 GB free (99 %). One Player build is 275 MB (Builds/n39-r4); the Builds folder holds ~80 old builds; 103 old worktrees
  under .claude/worktrees (a checkout ~230 MB plus build outputs). Free memory 22 GB.
- tasks.json has no rows past U3-24 / U4-U6: recent nights kept their task ids in the prompt only.

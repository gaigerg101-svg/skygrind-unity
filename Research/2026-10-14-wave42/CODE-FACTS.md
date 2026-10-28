# CODE-FACTS (wave 42): today's numbers for GG's asks, read from code and config (no game run)

Source: the forge's read-only sweep (returned 2026-10-14 ~19:05, written here by the lead in NIGHT42 G0). Paths relative to the repo.
The sim is 120 Hz (PortExport/config.json:3). Nothing below was measured in a game; "not checked" where the sweep did not confirm.

## G3: the PULSE gauntlet's swing
- The web's gauntlet cycle is 0.42 s (PortExport/config.json:839). The override is design.n27.pulse27.swingSeconds 2.5
  (Design/unity-config.json:2001), applied at Core/Combat.cs:926 through PulseCharge27.cs:105 (SwingSteps). There is no n36 override.
- One swing = 2.5 s = 300 steps. Damage per swing: 32 x damageMult 6 = 192.
- G3 = 2.5 -> 1.8 s (216 steps) in design.n42.gauntlet42 (DECIDE42 D-2).

## G4: the parry and the DRONE orb
- `CombatMath.IsParry` (Core/CombatMath.cs:178) = `windupSteps > 0`: any melee hit during a wind-up parries.
- The DRONE orb reflect is a distance test only: orb centre within 2 m of the eye (orb.reflectRange 2, PortExport/config.json:4586;
  Core/Enemies/Drone.cs:517-519). No aim cone.
- The orb flies 12 m/s (foes40, Design/unity-config.json:5960): it crosses the 2 m reach in ~0.17 s (~20 steps).
- The punch: a whiff opens parry36's early buffer 0.12 s (~15 steps; Design/unity-config.json:3862; Combat.cs:1370; Parry36.cs:41),
  so the punch window is ~0.29 s.
- The gauntlet swing (foes40): `SwingReflect40` (Core/Enemies/Foes40.cs:31; called at Combat.cs:1284) never opens the buffer: ~0.17 s
  window, no buffer; a miss costs the 2.5 s swing cooldown, and the punch is blocked while the gauntlet is in hand.

## G5: the THUNDERCLAP boost
- The push: Combat.cs:991-992 (and ChargeShots27.cs:301 for the charged slug): `velocity -= aim x 10` (weapons.shotgun.jumpSpeed 10,
  PortExport/config.json:590). Additive to the current velocity, only when not grounded.
- No pitch threshold in the physics: every air shot pushes opposite the aim. kiln40's lookDownDeg 60 / boostPitchDeg 45
  (Design/unity-config.json:5581 / 5583) only drive the teaching chain's checks (Game/SurvivalSim.N40.Kiln.cs:53-54, 105).
- charge40: in the air a press fires at once; on the ground a hold charges the slug (Core/Guns/Charge35.cs:26-31).
- Reading: looking straight down gives +10 m/s up; at 45 deg only +7.07 up. A falling player (vy -8) nets ~+2 up from a 45 deg shot.

## G2: the wall jump and the ride jump-off
- Wall jump (Core/Mover.cs:962-972): the along-wall speed kept, +8 m/s out (PortExport/config.json:100-104), up 0.9 x the jump v0;
  at most 3 per airtime.
- **The accident:** `AirJump` (Core/MovementMath.cs:200-202) makes ANY air jump press a wall jump instead of the double jump whenever a
  wall is touched or probed near (Mover.cs:878, FindNearWall :1760). flow34 (design.n34.flow34) widens it: wallCoyote 0.1 s after a ride
  (Mover.cs:880), wallBuffer holds a double-jump press if a wall is predicted soon (Mover.cs:912), slideBounce34 turns a slide into a
  wall into a bounce (Mover.cs:920).
- The ride jump-off: on jump release mid-ride (Mover.cs:1879-1880; Ride35.cs:154-183): 8 m/s out (wallride35, :3641) + exitBoost 1;
  the total speed capped at chainCap 16 (ride39, :5412-5418) or the ride's own speed if faster; up 4.6-10 m/s by pitch. lateJump 0.2 s
  (:3669) still counts as a jump-off after a ride ends. Measured once: a 16.52 m/s jump-off (NIGHT41-LOG.md:218).

## G10 / G8: the charge rules and the reticle
- Core/Guns/Charge35.cs: the revolver uses p39; the shotgun p39 on the ground, tap36 in the air. FULL at 0.9 s (KESTREL) / 1.2 s
  (THUNDERCLAP); the tap split 0.15 s (:5606-5610); a full charge held 1.0 s longer fires itself (holdMaxS :3614).
- After a charged shot: the charge cooldown 2.5 s revolver / 3.0 s shotgun (:3617 / :3621). **During it a held fire auto-fires normal
  shots** (Charge35.cs:550-560) and a press fires a tap; a new charge needs a new press after the cooldown (G10's complaint).
- The reticle spin (Game/UiHud38.Kestrel40.cs; Game/UnityTuning.Pistol40.cs:29-35): tap 0.28 s spin + 0.06 s snap = 0.34 s (inside the
  0.48 s interval = 0.4 x 1.2); cannon 1.7 s + 0.12 s = 1.82 s; the charge snap 0.08 s. **View only: nothing gates on it**; a tap can fire
  mid-cannon-spin and the cannon spin keeps priority (:36-45).

## G7: the arc40 meter (Game/UiHud38.Arc40.cs:73-110)
- States: CHARGING (eased fill, level^1.6), COOLDOWN (draining, bone), FULL (cyan), HEAT (STITCHER only), PIPS.
- PULSE: 5 pip segments + a thin line for the 15 s blast cooldown. **The 2.5 s swing cooldown is not shown.**
- The eased fill (level^1.6) means the arc under-reads the true charge mid-way (at half charge it shows 33 %).

## G13: the style meter and the health bar (Game/UiHud33.cs; Hud33Layout.cs; UiStyle33.cs)
- Health (Health33, :454): a 64 px number + 10 slash ticks; snaps, no tween; red on a 150 ms hit flash or at <= 25 %.
- Style (Points33, :604): the rank letter + a 160 x 3 line filled per rank (rounded 0.01), shown only above 0; snaps, no tween.
- The old Game/UiStyleMeter.cs (NIGHT17's meter): whether hud33 hides it is not checked.

## G1 / G6: level 1 = n41-foundry (foundry41; Design/levels/n41-foundry.json)
- b2 "1.7 THE GANTRY": two dry pits 37 m long, 4 m deep, wall to wall (z -229..-192 and -278..-241), ride-only; no-ride bands 4 m at
  z -214..-218 and -263..-267 (6 noride boxes in the room).
- Other ride gates: a3 lava pit 14 x 9 m and a8 PIERCE pit 16 m (ride-only crossings); a5 lava 12 x 11 m (ride over it).
- Teaching in the first minute (par a1+a2+a3 ~52 s; showS 5, Design/unity-config.json:3691): a1 lesson25 "MOVE. LOOK. JUMP.", teach35
  l1.dash, lesson25 "HOLD SLIDE"; a2 teach35 l1.fire, l1.charge; a3 teach35 l1.zoom, lesson25 "HOLD JUMP ON A WALL: RIDE" = **7 lines**
  (10 with the 3 room titles in the message slot). The start40 hold (4 s) adds no text.

## G14: THE ICEBREAKER (Core/Enemies/Brawl40.cs:1-24; Design/unity-config.json:5672-5880)
- HP 6000 x difficulty scale (Brawl40.cs:256); phase 2 at 50 %. Damage band x1 within 4 m fading to x0.2 at 12 m+; melee always full.
- Attacks: HAYMAKER (red, 1.0 / 0.9 s windup, 30 dmg, the fist stuck 1.2 s), SLAM (red ring, 0.9 / 0.8 s, 20 dmg, chains after a
  haymaker in phase 2 at 0.5), BLOCK (yellow, parryable: 15 dmg, parry reach 3 m in a 60 deg cone, 0.12 s buffer, returns 150), RUSH
  (red, 0.65 s windup, 16 m/s, 25 dmg). Floes: an ice hazard, 12 dps. The bot band today 52-54 s (NIGHT41).

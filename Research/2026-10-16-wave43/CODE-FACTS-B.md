# CODE-FACTS B (wave 43): combat, bosses, enemies, UI, level 3 (G17-G28), read from code and config (no game run)

Read-only sweep by the lead's agent, 2026-10-16 ~16:15. Paths relative to the repo. 120 Hz sim (1 step = 8.33 ms). Read, not measured.

Campaign order today (Core/Curriculum25.cs:13-206): 1 n42-foundry, 2 n40-iceline, 3 n41-kiln, 4 n40-span, 5 n41-spire (level 6 cut).

## G17 the PULSE swing
- Today 1.8 s (216 steps): design.n42.gauntlet42.swingSeconds 1.8 (Design/unity-config.json:6212, row ON :6210), read at
  Game/SurvivalSim.N42.Weapons.cs:17 into PulseCharge27.swingSeconds42; PulseCharge27.cs:110 SwingSteps; Core/Combat.cs:926 the cooldown.
- 1.25 s = 150 steps, 1.3 s = 156. HUD (HudWeapons30.cs:314), meter42 and Bot/Brawl40Pilot.cs:185 read the live cooldown.
- Damage per swing 32 x 6 = 192; DPS 107 -> 154 (+44 %) at 1.25 s. impact42 caps gauntlet freezes at 8 % of a minute (gauntletPerMinute :6247).

## G18 the DRONE orb parry
- Today (parry42 OFF): the orb centre within 2 m of the eye POINT (orb.reflectRange 2, PortExport/config.json:4586; Core/Enemies/Drone.cs:557,
  NearestHostile :523-538). Eye = feet + 1.6 (Game/SurvivalSim.cs:406-414). No aim cone.
- The orb hurts within 0.35 + 0.35 = 0.70 m of the capsule axis (Drone.cs:673-674). The drone aims at feet + 1.28 m (Drone.cs:202-209), 12 m/s.
- So a level orb enters the 2 m eye sphere at ~1.97 m and hits at 0.70 m: a ~1.27 m / ~0.11 s (~13 step) window. Chest-aimed orbs are the hardest.
- The punch's miss opens parry36's buffer 0.12 s (Combat.cs:1370, Parry36.cs:126-147); the gauntlet's SwingReflect40 (Enemies/Foes40.cs:31-48,
  Combat.cs:1284) opens none with parry42 off, once per 1.8 s; the punch is blocked while the gauntlet is in hand (Combat.cs:1339-1350).
- Melee reach on bodies: gauntlet 2.4 m, 35 deg, +1 m parry peek (config.json:837-845; Combat.cs:776, :1289); punch 2 m, 35 deg.
- parry42 (Core/Parry42.cs; unity-config :6219-6224): reach 3 m from the orb SURFACE to the body axis (Drone.cs:505-521), mercy 0.1 s
  (Drone.cs:615-620, :675-680), an orb buffer 0.24 s the gauntlet's miss opens too (Foes40.cs:38; Parry42.cs:43-64). Window ~0.22 s + 0.1 + 0.24.

## G26 the THUNDERCLAP boost
- Today (boost42 OFF): velocity -= aim x 10 (weapons.shotgun.jumpSpeed 10, config.json:590) only when !mover.grounded at the shot
  (Combat.cs:991-992; Guns/Sandbox19.cs:227; slug Guns/ChargeShots27.cs:301; all via Boost42.cs:70-78). No pitch rule.
- Order: the mover runs before combat (Game/SurvivalSim.Run.cs:311 vs :333); a ground jump sets grounded false that step (Mover.cs:1067).
- Jump + fire within ~0.1 s on the ground (Core/Guns/Charge35.cs:203-321):
  - same step: airborne, tap36 fires on the press, boost: vy 8.33 + 10 = 18.3.
  - jump first, fire later: airborne, boosts.
  - fire first on the ground: charge40's ground rule (C40Ground :307-321; StepP39 :481-608): nothing on the press; the volley on RELEASE within
    shotgunTapS 0.15 s (:5611). Released before leaving the ground = a ground shot, no boost; hold past 0.15 s = a charge.
  - **the jump cut**: jumpCutArmed stays armed after the shot; releasing jump while rising multiplies vy by releaseMult 0.5 (config.json:50;
    MovementMath.cs:168-177; Mover.cs:1237-1239): tap jump then shoot: 18.3 -> ~9.2 m/s. Boost42 measured 16.8 -> 8.4.
  - the 0.9 s fire interval (config.json:586); a shot that became a charge waits its 3.0 s charge cooldown.
- boost42 (Core/Boost42.cs; :6194-6199): lift 11 m/s at 40 deg or steeper, a fall cut to 3 m/s down first, Mover.Boost42Launch() disarms the cut
  (Mover.cs:468). It does not touch the ground hold rule or the press / release timing.

## G19 / G20 impact and feel
- No player dash-into-enemy damage. Closest: momentum (melee x3 with closing speed 8-32 m/s, momentum shock 6-14 m/s, hitstop 0.045 s;
  config.json:1976-1996, :269-270; whether loadout.momentum is on in the campaign: not checked); Core/SlideBite21.cs (slide bites).
- Hitstop: Core/ImpactRules.cs (:41-52), Game/ImpactFrames.cs; impact42 ON (:6233-6267): gauntlet hit 0.16 / big 0.20 / boss 0.24 / kill 0.22;
  gun kill 0.085, headshot kill 0.12, heavy hit 0.06; body hits revolver 0.024, shotgun 0.036, nailgun 0.012.
- Shake: Game/CameraFeel.cs (trauma + CrunchShake, <= 2 deg pitch / yaw, 3 deg roll); Juice38 (boss entrance, big moves, hurt).
- Screen: Game/PostFx.cs (the crush frame ~-2 EV on headshot / punch kills, every third multikill, perfect parry; chroma kicks on hurt, dash,
  rank-up; FlashGate <= 3 per s); parry flash UiFeel.cs; no white flash.
- Haptics exist: Game/Input/Rumble.cs:85 SetMotorSpeeds; web pulses hurt / kill / parry (SurvivalGame.Init.cs:258-266); Haptics22 (ON,
  :1022-1036) shockwave, attach, yank, kill, down; ImpactFrames strong on perfect parry / gauntlet42 hit, medium on headshot / heavy / punch
  (ImpactFrames.cs:201-203). **None for gun fire, the TORCH, charged shots or dashes.**
- The flamethrower is the TORCH: Core/Guns/Torch.cs (7 m, 18 deg, 6.5 per 0.1 s tick, 4 s heat; config.json:640-651); view
  Game/GunFx.Spray20.cs:243-249 from a 256-particle pool shared with the sprays (UnityTuning.Fx20.cs:138). Granted at level 5 (THE CROWN).
- The charged slug: one ray, 8 x 8 = 64 damage + pierce33 (ChargeShots27.cs:284-301); one tracer (Game/WeaponFx.cs:179-185, :253-259).

## Bosses
| lvl | boss | room / file | HP | phases | parryable |
|---|---|---|---|---|---|
| 1 | THE WICK | n42-foundry a9 "1.11 THE STACK" (28 x 28 m, 50 m tall); Core/Enemies/Wick37.cs, Wick37Stack.cs | 600 x boss39 2 = 1200 | 3 legs; HP locked at 60 % and 30 % until each leg ends | EMBER (yellow, 3 globs 9 m/s; reach 3 m, 50 deg, 0.12 s) |
| 2 | THE ICEBREAKER | n40-iceline x9; Brawl40.cs, .N42.cs | 6000 x difficulty | phase 2 at 50 % | BLOCK (yellow, 3 m, 60 deg) |
| 3 | THE KILN WYRM | n41-kiln k5; Wyrm.cs / Wyrm31.cs / WyrmArena.cs | ~5620 | 66 % / 33 % | none; a head punch staggers |
| 4 | THE LOOM | n40-span o3; Loom37.cs | 750 | legs | lash / spool (3.5 m, 60 deg) |
| 5 | THE CROWN | n41-spire v11; Crown37.cs | 20 gems x 100 | 13 and 6 gems | not checked |

- THE WICK: full damage only from a charged KESTREL shot fired from a ride (within 0.35 s) through a cracked plate; chip otherwise; the CYAN
  stagger takes everything (Wick37.cs:4-8). EMBER 1.2 s, FLARE 1.4 s (wipes your half of the shaft wall), SLAM 1.0 s, DIVE (leg 3), DRAFT;
  the slag rises 0.6 m/s while it attacks (30 dmg).
- Its bar: Game/UiBossBar41.cs (bars41 ON), 640 x 6, ticks at 0.6 / 0.3, a stagger meter. Hidden when bars41 off, hud33 not drawing,
  HudReads20.ThreatNow invalid (HudReads20.cs:148; BossBarHidden30 = room not kind boss, HudReads20.Live.cs:123, SurvivalSim.Systems30.cs:52), or
  the body not alive (Wick37.Bar().valid = body.alive, Wick37.cs:247; the body is off before its rise and after Stop :241). Why GG saw it
  vanish: not checked (suspect: a restart after the slag, restartCost 20). The HP locks read as "not taking damage".
- Intro: no camera control. entrance41 (ON :6122): a 1.5 s beat: the name card (56 px, 170 px above centre, 1.6 s; UnityTuning.Juice38.cs:42-49),
  Juice38's hitstop / shake / flicker, the rule line "RIDE A WALL, HOLD {fire}: SHOOT THROUGH A CRACK", the body rising from -3 m over 2 s.
- **G13**: wallJump.maxPerAirtime 3 (config.json:100-104), spent at Mover.cs:1015 (wall jump) and :1842 (slide bounce), refilled only on
  landing (:1598). Whether ride jump-offs spend it: not checked.
- The mini-ICEBREAKER is the CHARGER in n40-iceline g8 "2.8 THE REHEARSAL" ([charger], then [charger, husk, husk]; "{dash} ASIDE, THEN {fire}
  ITS BACK"). Core/Enemies/Charger.cs: 150 HP, 0.8 s telegraph, 22 m/s, 25 dmg, a wall stuns it 1.5 s, back x3. Elsewhere only kb2 (1 charger).

## Enemies
- Models: Unity primitives, Game/EnemyViews.cs (:1-24) + EnemyViews.Charger / Drone / N20 / Bulwark / Climb. Husk = capsule + tilted head box;
  swarmer 3 boxes; drone a sphere; charger cubes. models40 ON (2-3 primitive parts per kind, Game/Enemy40View.cs, UnityTuning.Models40.cs; roles
  DARK / BRIGHT / DANGER / PARRY / WEAK). models41 OFF (:6163-6166). Shader SKYGRIND/Enemy (rim, hit flash, dissolve).
- Roster: husk 40 HP r0.4 h1.8 speed 6; swarmer 10; spitter 60; charger 150; drone 30 (flies); warden 200 (shield); lancer 70 (not in waves);
  tank 600. colors.enemy #ff4a1c x shade.
- **"heavy" is a plain husk (suspect)**: levels 3 / 5 list "heavy" (kb3 x2, v2, v6) but Husks.ListOf / Make (Core/Husks.cs:220-228) map unknown
  kinds to a 40 HP husk; SurvivalSim.Campaign24.cs:77-83 passes the kind as written.
- No small worm exists; THE KILN WYRM is the only worm.
- "Fails to move" suspects: 2 melee tokens (ai.tokens.base.melee) leave the rest on the 10 m ring; the watchdog (HuskBrain.cs:1095-1152) runs only
  while the enemy wants to move, hop / sidestep / repath at 0.5 / 1 / 1.5 s, relocates after 3 s ONLY OFF SCREEN (:1141); chargers cannot jump;
  lava / grinder nav masks (ai33 :3281) can close nodes; climb routes (climbMax 5 m) per level navGrid not checked; drones skip the watchdog.
- **G5 door**: Core/Campaign24.cs:736-790: a room clears when nothing is alive (plus spawn33's queued tells) AND its lessons are done.
  Triggered waves (waveReady33) wait for their line. **A lesson that needs a verb (not an enemy) can block the clear forever**; only
  enemy-needing lessons get top-ups and the skip cap. Failed spawns are only counted (spawnFails).

## UI
- HUD: Game/UiHud33.cs, Hud33Layout.cs, UnityTuning.Hud33.cs. Health: a 64 px number + 10 slash ticks (+ health42's chunk / heal glow). Style:
  rank letter + line (+ style42). The old UiStyleMeter.cs still exists.
- Fonts are NOT TMP's default (CLAUDE.md rule 8 is stale): Archivo ExtraCondensed Black Italic (display), Archivo SemiBold / ExtraBold (body),
  Tourney Black Italic (rank) (UnityTuning.Theme.cs:11-28); the old Ui uses ChakraPetch / BarlowSemiCondensed; SDFs in Assets/Resources/Fonts/.
- Teaching: teach35 (ON, 5 s) in the top-centre message slot, 24 px, 640 px wide, 54 px from the top (UnityTuning.Hud33.cs:85-110; UiTeach35.cs);
  text36 (ON) a lower-middle slot at 62 % height, 32 px, <= 2 lines, 528 px (UnityTuning.Move36.cs:11-14; Hud33Layout.cs:62). UiLesson25.cs.
- Shop: UiShop33.cs, one full-screen scrim alpha 0.86 (UnityTuning.Shop33.cs:9), no panels. Results: UiCampaignResults25.cs; UiResults.cs /
  UiPayoff.cs (survival). No blur anywhere (TravelBlur.shader is motion blur).
- Enemies-remaining: survival only ("ROUND n · N LEFT", UiHud33.cs:672); none in the campaign or bosses.
- Post: a URP Volume built in code (Look.cs:22-178): Bloom, ACES, ColorAdjustments, Vignette, ChromaticAberration, FilmGrain (PostFx.cs);
  CRT / PS2: Shaders/Resources/Crt29.shader + look35.

## Level 3 = n41-kiln
- kb1 "3.1 THE CHARGE" 20 x 26 m (x -10..10, z -26..0), roof 10 m, THUNDERCLAP in hand; built by Tools/night/gen-n40-kiln.cjs:104-130 (kiln41
  kept the geometry). THE RIM: a 5 m deck over the west half (x -10..-1, z -22..-18), the first boost's landing.
- **The strange ramp**: the wedge {x -8.5, z -5, length 13, width 3, rise 5} along the west wall up to the rim (gen :119): "the rim's stair
  ... the walkers' route and the slow way up", so the lesson can be walked past. Also 3 static stand-ins 8-11 m from the door, one wave of
  2 husks, lessons "BOOST: FOLLOW THE STEPS" / "JUMP, SHOOT {fire} DOWN: BOOST", a health pickup on the rim.
- kb1b "3.1b THE FLUE": a 12 m cliff (gate G1) between noride walls, "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN."
- Rosters: kb2 husk 8, swarmer 4, spitter 2, charger 1; kb3 husk 7, "heavy" 2, spitter 4, swarmer 2; k5 THE KILN WYRM. **No drones in level 3.**

# DESIGN33: the polish night: a HUD that looks made, real light, dressed arenas, smarter enemies, a whip and a piercer

GG, 2026-10-11 (verbatim with screenshot notes in Research/2026-10-11-wave33/GG-REQUEST.md): more elements in every arena, better effects
and lighting (lights "placed and floating"), the arena design is poor, the HUD "looks so AI generated" with "way too much space", the zoom is
"a forced angle", enemies run into lava, ULTRAKILL-style trigger spawns, bosses "still way too tanky", the grapple looks and feels bad (pull
me to the enemy, stop at it, a little damage), a stronger piercing pistol charge, campaign points that buy something.

Evidence: U1-hud.md (HUD spec), L1-lighting-vfx.md (lighting spec), K1-arena-kit.md (39 elements, 8 devices, density rules, 15 checks),
G1-feel-ai-economy.md (viewmodel, whip, piercer, encounters, hazards, shop), A3-hud-view-light-points.md and A4-ai-spawn-grapple-pierce.md
(the code, file:line).

## The rules for every card

1. **A spec, a check, a frame.** Each card ships its spec's automated checks (a harness command or a reel line) and at least one Player frame
   per changed thing that the lead looks at. "Looks better" is never a result; a number or a frame pair (RED -skygrindOff33 / GREEN) is.
2. **Not AI-looking (U1):** no filled boxes behind readouts, one shape motif (the 20-degree slash), a clear type hierarchy (64 / 24 / 16 px),
   asymmetry, restraint; nothing shown that the player cannot use right now.
3. **Every light has a fixture (L1):** a light or emissive bar exists only on a visible mount touching geometry (gap <= 0.02 m).
4. **Dress the edges, keep the floor clean (K1):** decor above 3 m and along walls; the run surface stays readable; decor adds no colliders.
5. **Overrides behind design.n33 rows;** `--off33` / `-skygrindOff33` is NIGHT32's game (fingerprint EQUAL with the rows off).

## U: the HUD and front UI rebuilt (row hud33; U1 spec)

- **Remove every panel fill** (the teal health box, the weapon card, the points box, the rank box). Text with a 2 px shadow on the world.
- **Health** bottom-left (54 px margin): a 64 px number over 10 slash ticks; the stamina as two thin slash ticks under it (never mid-screen).
- **Weapon** bottom-right, OFF the gun model: the held weapon's name in 24 px caps, its LT action under it in 16 px (the only LT label on
  screen: no "ZOOM ZOOM ZOOM" row), a strip of five 12 px slash pips for the owned weapons that expands to names for 700 ms on a switch.
  The card no longer follows the gun's sway.
- **Points and rank** top-right in bone white (not yellow): points 32 px; the rank letter and meter show only while the meter is above 0.
- **Reticle:** a 4 px dot and four ticks; the charge closes arcs on the dot, a hit flashes the ticks, a kill rotates them 45 degrees; the
  grapple charges become two slash ticks under the reticle (no diamonds); the momentum ring and cooldown arc fold into the ticks.
- **One message slot** top-centre with priority danger > boss line > room title > door hint > lesson; one message at a time, each held at
  least 1.2 s; the door hint "7 m AHEAD" becomes a world marker on the door (A3: the overlap is the zone banner queue vs CueWait25's 2.0 s).
- **Boss bar** top-centre 640 x 6 px with the name above in 16 px.
- **Front UI:** the title, level select and results screens in the same language (no fills, the slash motif, the theme fonts already
  converted: Archivo / Tourney, A3); the results screen shows the points banked for the shop (card P).
- Keep the reel hooks working (`Lesson25TextNow`, `Weapon30Ids`, `weaponBlock`, A3 risks) or replace them in the same commit.
- **Checks (U1's 12 rules):** no element wider than 360 px except the boss bar; no repeated label; text >= 16 px at 1080p; margin >= 54 px;
  a Play capture at 1080p and 720p asserting no text rectangles overlap; motion only on events.

## L: lighting and effects (row look33; L1 spec)

- **No floating light:** the look30 floor shafts with nothing at their top (LightKit29.cs:210-271) get a ceiling source or go; the room edge
  strips skip pits and lava (CampaignLighting24.cs:55-65: 31 crossings); the white ceiling panels become flush fixtures with a frame; the
  roof strips sit under each room's own roof (RoofView27.cs:73-76); CampaignLighting24's moving room light at a fixed height in each room's
  middle becomes mounted fixtures; null-guard CampaignLighting24.cs:110 (an Editor NRE every frame after a reload).
- **Six fixture types** built from boxes (wall sconce, recessed strip, pole lamp, flush ceiling panel, brazier, lava light) with L1's light
  parameters; 3-6 lights per room, one key; emissive HDR <= 4 (fixtures), <= 2.2 (lava).
- **Post profile:** Neutral tonemapping (ACES pushes cyan to white), bloom threshold 1.0 / intensity 0.4-0.6 / clamp 10, SSAO on, per-theme
  ambient, fog and exposure (L1 table); lava light local (a point light 1.2 m over the lava, range 8), no screen-wide orange wash, and the
  low-health edge red never covers more than the outer 12 % of the frame.
- **Effects:** dust motes, embers over lava, heat haze in the kiln, sparks at hazards, a flicker on danger fixtures only, scrolling emissive
  panels; a muzzle flash and impact sparks pass for every gun (no new gun models).
- **Checks:** no light without a fixture within 0.6 m (a harness scan of every level); no unmounted emissive bar; mean frame luminance
  0.10-0.30; <= 3 % pixels at 0.98+; orange <= 35 % of saturated pixels in lava rooms; at least 15 % dark pixels.

## K: the arena kit and the dressing pass (row kit33; K1 catalogue)

- **A prop library:** new PropsView cases for K1's 39 elements (skirt, cornice, pilaster, rib, door frame, panel recess, ceiling truss, pipe
  bundle, chain, banner, floor inlay, grate, hazard stripe, chimney, kiln arch, ingot pile, stalactite, crystal cluster, snow drift, pylon,
  cable, lamp post, halo, fin, hero landmark, debris, scorch, crate stack, low wall, cover pillar ...), built from boxes / cylinders with
  theme materials, and the unknown props (heatVent, checkpoint, plinth, slot) drawn as themselves, not as crates (A3).
- **Wall surfaces:** triplanar noise or vertex-colour gradients and panel lines so no wall is a flat plane.
- **A dressing pass in the generators** (`Tools/night/dress33.cjs`, run on every n32 plan) that places decor by K1's rules: skirt and cornice
  on all wall length, a rib / recess / pilaster every 6-8 m, every doorway framed, 8-14 decor pieces per 100 m2 with 40 % above 3 m, exactly
  one hero landmark per room outside the play area, no cyan in decor. Decor adds 0 colliders; cover pieces are gameplay and do collide.
- **Checks:** K1's 15 plan checks as `kit33 <level>` in the harness (density, coverage, clear lanes, no decor in the run area).

## R: arena overhauls and devices (row arenas33)

- Overhaul the weakest rooms by render and frame judgement: at least two per level across levels 1-5 (THE SPIRE's straight run included),
  each with K's kit, a landmark, two or more heights, cover by K1's rule (every point within 8 m of cover), a device.
- **Devices** (Core where they move the player; deterministic, Mulberry32): the jump pad (exists), an updraft fan, a hazard vent with a tell,
  a moving platform; each with a tell and a refuge within 6 m; at most 3 device types per room.
- Keep each level's weapon pickup, boss room, lesson lines (<= 6 words), spawn rule, room ids the bots use; walk bot 10/10 per level.

## E: enemies and encounters (rows ai33, spawns33, bosses33)

- **Lava:** a lava mask merged into `NavGrid.SetBlocked` (NavField.cs:155) filled in Lava30StartRun (Systems30.cs:94), and the same mask in
  LevelValidate24.cs:113; an edge guard after husks.Step for crowd pushes, the last straight run at the player and the charger's dash (stun
  it at the edge). Knockback by the player can still throw enemies in (that stays fun). Check: a 60 s bot run on every lava level shows 0
  enemy lava deaths not caused by the player.
- **Trigger spawns:** a per-room `triggers33` key (a rect, its wave, its own spawn spots on the far side, an early flag) read in StepFight;
  a 0.6 s red-orange spawn tell (`enemySpawnTell33`) before each enemy appears; the first wave of lava rooms waits for the trigger line, so the
  room pulls the player across. Seeded, minimum distance from the player.
- **Bosses still too tanky:** pilot band now 15-22 s (harness p50 about 18 s; a person about 40 s): `hpMult` per boss in bosses33 overrides
  and the Crown pool; keep the anti-camp (campers die before dealing 40 %).

## W: the gun feel, the whip and the piercer (rows view33, whip33, pierce33)

- **Viewmodel:** LT zoom changes the WORLD FOV only; the gun never slides to the centre or rolls: it lowers 4-6 cm and eases in 120 ms (A3:
  the ADS pose slides it ~330 px and rolls 4-5 degrees; ZoomLookTests pins the web default: override in Unity config and update the test
  for the n33 path only). Idle sway, speed-scaled bob, a landing dip, a recoil spring per weapon; the viewmodel FOV stays 54-60.
- **The whip (GG: "grapple you towards the enemy ... you don't go through it ... a little damage"):** a TAP latches the first enemy on the
  ray: light enemies (husk, swarmer, drone) are pulled to you, heavier ones pull you and you STOP 1.5 m short of the hit volume, never inside;
  arrival gives a small hop, a hit through Combat.Damage and 2-3 steps of hitstop; jump, dash or a second press cancels with momentum
  kept. Rings and boss joints stay pull targets. Charges stay (grapple32), but a whip kill gives NO charge back (A4: an endless chain). No
  Mover.cs edit (G31OnMover / N31GrappleStep, A4).
- **The rope:** a hook head, a slight sag while flying (the web's sag numbers are unused, Arcline21View.cs:98-113), a taut line on latch,
  a snap on release; it leaves from the gun's muzzle, not below the camera.
- **The piercer:** the charged KESTREL shot pierces everything in a line (pierce 24, the LONGWAVE's value) at much higher damage
  (damageMult 8-10), the tracer ends at the wall and uses the LONGWAVE beam (WeaponFx.cs:149) under a new event name, hitstop and a camera
  kick; cooldown 4 s. Run the survival fingerprint (the script fires the pistol).

## P: points that buy something (row shop33)

- Points carry across the campaign (Economy.Start(bank), saved in Profile.cs) and are spent in a SHOP screen between levels, opened from
  `ContinueCampaign()` (SurvivalGame.Campaign25.cs:25) after RESULTS.
- Eight upgrades in four slots (2 weapon mods, 1 movement perk, 1 wildcard), prices 1x / 2x / 4x a level's measured typical earn
  (measure it first: the walk bot's points per level): KESTREL charge -30 %, THUNDERCLAP slug +1 pierce, NAILGUN detonate +25 %, PULSE parry
  window +40 %, +1 grapple charge, dash cooldown -20 %, +20 max health, a kill heals 5. Each is recorded on the run's tape (sim-changing:
  the fingerprint must stay EQUAL with shop33 off and with no upgrades bought).
- The shop in U's visual language; pad-navigable; the results screen shows the points banked.

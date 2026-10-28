# PORT-STATUS (Unity stage 3, `u3-n18`)

What the Unity port has of the web game, system by system, against `skygrind/PORT-INVENTORY.md` section 1. The web
game is frozen at `p17-night6` and is the reference. Unity is the only build from here on. Updated 2026-10-06 (NIGHT18:
feel in motion, all Unity-only and all in the Game layer (the fingerprint EQUAL at every commit; Core changed only in
`ImpactRules.cs`, which takes its freeze table as a prefix, golden 0 mismatches):
- a same-build A / B switch, `FeelAB` (`-skygrindFeelAB off`; F6 and the LAB row FEEL N18 in a developer build): off
  draws NIGHT17's feel exactly (named `...N17` numbers; `design.impact.n17` for the freeze ladder);
- the measured reel: `-skygrindReel <dir>` is FeelReel (every frame a JPG on a fixed 1/60 s clock, per-frame telemetry
  from `FeelProbe`, `Tools/feel-schema.md`), read by `Tools/feel-metrics.py` (metrics M1-M11, flipbooks, blind A / B
  sheets, side-by-side GIFs);
- the kill (a crisp white-hot frame 1 with hard streaks and a dark disc, blocks thrown across and up, a freeze ladder by
  the kill's weight), the hit (a damped-spring shove sized for the screen, an impact flare and chips), movement (the
  boost's kick, sprint and slide widen, strafe roll, the landing's dip and nod, speed lines out of the middle and the HUD's
  corners), HUD entrances and a white kill marker, menus that enter along the slant, FOV KICK / CAMERA SHAKE settings,
  HUD STYLE ATTACHED, the low-HP vignette pulse, the spawn ring back in DANGER.

No new bus event tonight: the reel's moments (`killFx`, `land`, `flourish`, ...) are FeelProbe markers, not events.)
(NIGHT17:
the overhaul night, all Unity-only on top of the web's rules:
- an identity (IDENTITY.md: Archivo + Tourney, a 12° slant, opaque INK, a cyan ladder, gold for points; every token in
  `UnityTuning.Theme.cs` through `UiTheme`);
- one stamina bar with a sprint and a partial boost (design.stamina, flags on; the fingerprint re-recorded once);
- kills that break into their own blocks, with a death signature per gun and a freeze that grows with the kill count
  (design.impact);
- the hands' recoil, carries, aura and flourish;
- the HUD rebuilt in three tiers with a style meter, merged points pops and a round-end band;
- the menus cut to TITLE 4 / PAUSED 4 / SETTINGS ≤ 5 tabs × 8 rows / RESULTS 3, with one focus state;
- the world's neon answering the rank;
- the feel reel (`-skygrindReel`).

See the UI row and the "Stamina and crunch" events.) (NIGHT16:
the feel night: the slide made a skill behind design.move (Unity-only, on by default, LAB rows); tiered impact freezes,
a little slow-mo and a freeze budget (design.impact, the Game layer only); the hands: per-gun draws, the web's left arm
(viewmodel.arm) and cooldown glint (weaponReady) ported, the web's FOV kick stack (player.js: multikill, melee lunge,
rank-up) ported; the crosshair movement HUD, transitions, the strip tool; see the Player row and the events). (NIGHT15:
map night 1: the web's createNavGrid and resolveZones ported line for line, a Unity map pipeline proven on the old map,
DRYDOCK's greybox heart, TITLE's MAP row; the aim assist's Unity defaults (mouse OFF, pad LIGHT); the pad's weapon wheel,
Nintendo glyphs, rumble strength; see the Maps row). (NIGHT14:
THE GAME PLAYS ITSELF: the web's bot (botBrain.js) and nav (astar.js, reach.js, follower.js, navGraph.js) ported line
for line with the p22 oracle (5 new golden sets, 39 in all); kiosk.js ported; on top, Unity-only: the AUTOPILOT, THE
FARM, the attract loop, THE TAPE, WHAT GOT YOU, THE ROUTE and its card, the GHOST and splits; a clean slate per run).
NIGHT13: the web's input stack (gamepad.js, holdToggle.js, bindings.js, menus.js' focus helpers) and quality.js /
qualityMath.js ported with their web tests; framePace.js superseded by `targetFrameRate`; `SurvivalSim` pinned by a
composed fingerprint.

**The oracle (NIGHT9-NIGHT11):** 34 golden vector sets in `PortExport/vectors/`, every one consumed by a
`Skygrind.Core` Golden checker (`Core/Golden/`) that the EditMode tests, the Player's `-skygrindGolden` and
`Tools/CoreHarness` run; 16 + 6 trial-route trajectories replayed bit for bit; V8's maths in JsMath (no System.Math
transcendental in Core, `DeterminismTests`). NIGHT10 added `projectiles` (projectiles.js sessions: nails, rockets,
fragments, steer, homing) and `weapons` (weapons.js fireRail on the real loadout). NIGHT11 added `roundType`
(directorMath.js roundType: the real rounds and 22 variants), `runMoments` (gradeOf, scoreLine), `objectives` (4
sessions of objectives.js on fakes) and `traps` (a session of traps.js on fakes): `Golden/GoldenRunShape.cs`. The
export scripts are `skygrind/port-export-p20/scripts` (untracked; p19 plus `runShape.mjs`). **NIGHT14 (p22,
`skygrind/port-export-p22/`, untracked):** `astar` (526 paths on 9 graphs + 198 nearestNode), `reach` (473 jumpReach /
dropReach), `navGraph` (the duel spire's graph and the Survival map's: 1013 / 848 nodes), `botBrain` (6 sessions x 1200
steps, normal and hard: the frame, look, state, visible every step), `follower` (12 paths, one per edge kind):
`Golden/GoldenBot.cs`; plus `maps/spire-duel-navgrid.json` (the duel's towers-core nav grid, V8-exported).

**Ported** means behaviour-equal to the web; the pure parts have golden vectors, compared bit for bit. **Partial** means
some of the system is in, and the rest is listed. **Not started** means nothing yet. **Unity-only** marks mechanics the
web never had; their numbers are in `Design/unity-config.json` (the `design.` prefix), never in code.

## Systems

| System (web) | Status | Unity files | What's in / what's missing |
|---|---|---|---|
| Movement (`movement/`: mover, movementMath) | **Ported** (+ Unity-only rules) | `Core/Mover.cs`, `MovementMath.cs`, `MovementConfig.cs`, `MovementPresets.cs`; NIGHT16: `Core/MoveBalance.cs`, `Game/SurvivalSim.MoveBalance.cs` | Bit-exact: movementMath golden vectors and 16 / 16 recorded trajectories. Ceiling corner correction (NIGHT5 F6), `Impulse`, `RestoreStamina`; the per-instance perk bonuses (pips, ride time, wall jumps) are set by VIGOR / GRIP. **Unity-only (NIGHT16, design.move, on by default; each rule a LAB row; with the rules off the Mover is the web's bit for bit):** the slide's burst needs a charge (back 2 s after a slide ends; an uncharged slide keeps its entry speed, never below a walk), a flat-ground slide decays to a walk after 0.6 s (downhill untouched), chained slide-jumps fall off (x1.2, 1.05, 1.0); the `slideCharged` event; THE TAPE format 2 records the flags. Measured: mashed / held slides 1.24-1.45 x a walk -> 1.00-1.06 x; skilled lines unchanged (FEEL-DESIGN.md 3.1). |
| ARCLINE swing grapple (swingMath, arcline, rope, grappleNodes) | Partial | `Core/SwingMath.cs`, `GrappleAim.cs`, the rope part of `Mover.cs` | The mover's rope and swing are exact (swingMath vectors, replays), and the Arena test scene uses them. In Survival the ARCLINE is a 5500 gadget nobody owns at the start: no gadget buy, no rope visual and no enemy pull yet. |
| Combat / weapons (`combat/`) | Partial | `Core/Combat.cs` (Loadout), `CombatMath.cs`, `Arsenal.cs`, `ArsenalMath.cs`, `Momentum.cs`, `MomentumMath.cs`, `Impact.cs`, `GunMath.cs`, `WeaponMath.cs`, `Health.cs` (health, regen, regenMath), `AssistMath.cs`, `AimAssist.cs`, `Perks.cs` (weaponMath perkPrice / perkEffects); `Game/CombatReader.cs`, `Viewmodel.cs`, `BlockyKit.cs`, `ImpactFx.cs` | **In:** KESTREL, THUNDERCLAP, PULSE GAUNTLET, punch / parry, the swap timeline, health and regen, aim assist (SNAP default; all four modes in SETTINGS), weaponMath's fireInterval / swapTime (QUICK), the run's bonus hook (weapons.js loadout.bonus mult / after: FORGE x MODS), the punch's reflect on spitter globs. **NIGHT9:** the arsenal (arsenal.js: FRESH x lerp(0.65, 1.25) on damage and points, PRIMED, CROSS / COLLAPSE / OVERLOAD, BURN / SHOCK / PULL / STUCK / STAGGER with burn ticks, the SWAP kill, OVERCLOCK, FLIP's mods; a forged gun's status lands); momentum (momentum.js: melee x lerp(1, 3, M), guns x lerp(1, 1.35, M), IMPACT, the momentum hitstop); impact.js's ring wave (GAUNTLET, punch); aim assist's V lock (lockOn / lockOff) and the melee lunge; DEADEYE's headshotMult; gunMath.js and weaponMath.js pure functions (golden; their guns come later). **NIGHT10:** STITCHER (nailgun: heat and the overheat lock, nails grouped into one shot within style.hitShotGroup, the detonate special, the pierce variant), COMET (rocket: splash with falloff, the rocket jump, the steer special, the fragments variant, momentum's blast radius) and LONGWAVE (rail: the charge, the cancel, the cooldown, pierce, the overcharge special with its kick back, the walls variant), as weapons.js fires them (`Core/Projectiles.cs`: projectiles.js's nail / rocket / stuck / fragment paths and shared core, golden; `Game/ProjectileViews.cs`); the SPECIAL key (B) and right mouse as the special on guns that don't zoom (ads.js routeSpecial); QUICK's charge; aim assist's homing (homingTarget: nails and rockets steer at the lock, assisted past aim.homingAssistAngle); the three guns' kit models and viewmodel rigs. **Unity-only:** PERFECT PARRY (design.parry); the glob reflect; NIGHT10's ULTRAKILL alignment (design.ultra, `Core/Ultra.cs`, ON: FRESH pays points and style but not damage, a parry freezes 0.25 s; the parry's dash refill is the web's own rule). **Missing:** the other 12 guns (`guns.js`) and the KESTREL / THUNDERCLAP specials and momentum variants; projectiles.js's grenades, discs, vortexes and darts (stubbed for N11); Drone orbs (N13); the impact crescent / echo / quake / crack (their melee weapons), healthOrbs, staticAura. A web bug ported as is: a stuck nail's offset is always (0, 0, 0) (projectiles.js stick() aliases its scratch vector). |
| Enemies + bosses (`enemies/`) | Partial | `Core/Husk.cs`, `Husks.cs`, `Swarmer.cs`, `Spitter.cs` (Spitter, SpitterAcid), `NavJumper.cs`; `Game/EnemyViews.cs` (HuskView, SwarmerView, SpitterView, Particles), `TelegraphView.cs`; `Shaders/Acid` | **In:** HUSK, SWARMER, SPITTER (spitter.js: tell, a 3-glob ballistic volley leading your velocity, a deliberate miss, back-off, acid puddles; pooled globs and puddles), the nav jump, spawn telegraphs, the web's looks as they are. **NIGHT12** (`Shaders/Enemy.shader`, EnemyViews' EnemyLook): one SKYGRIND/Enemy shader on every enemy: rimGlow.js's rim with a distance boost (fx.enemyRim*), the hit flash, a death dissolve (the web has none for enemies: dissolve.js's own numbers, 0.22 s), xray.js's pass (ZTest Greater, settings.xray, X toggles it with the web's hint); a port-only flinch and vibration on hits. **Missing:** drone, charger, lancer, warden, affixes, the bosses, RIVAL PRIME. |
| AI + nav (`ai/`, `nav/`) | Partial | `Core/HuskBrain.cs`, `NavField.cs`, `Director.cs`, `Spitter.cs` (AimMath) | **Exact:** the exported nav grid, flowField, navField, tokens (melee, swarm, ranged with line of sight), watchdog, crowd (husk, swarmer, spitter), directorMath (golden vectors), the director's pacing, spawn tiers, camping / hunters, ai/aim.js (hitChance, leadPoint, missPoint). **Unity-only:** PEAK FADE (design.director: no decay while engaged, PEAK -> PEAK_FADE -> RELAX; the web's StepDirector kept for the flag off). **Simplified (logged):** steering is Arrive blended at steer.gain (no Yuka), ring waiting without slots, squads or flankers. **NIGHT14:** `nav/` ported line for line (`Core/Nav/Astar.cs`, `Reach.cs`, `Follower.cs`, `NavGraph.cs` + `NavTypes.cs`, golden: astar, reach, navGraph, follower) with three's Octree.sphereIntersect added to the collider (`ThreeOctree` / `GameWorld.SphereIntersect`, new methods only); navGraph.js builds on the Survival map unchanged (848 nodes, 3-4 s on CoreCLR): `Game/Bot/BotNav.cs` builds it from the map data, caches it by content hash (memory + `persistentDataPath/botnav`) and pre-builds it on a worker at boot (navWorker's role). **Unity-only:** the clean slate per run (`SurvivalSim.CleanSlate`: the enemy pools and ids, the shot ids, the director's pre-round streams reset at StartRun; the web carries them over in one page). |
| Survival mode + economy (`survival/`, `modes/survival.js`) | Partial | `Core/SurvivalRun.cs`, `Economy.cs`, `Zones.cs`, `ZoneMath.cs`, `Director.cs` (Buyables), `SurvivalMap.cs`, `Perks.cs`, `PowerUps.cs`, `Mods.cs`, `DeathRecap.cs` (Records); `Game/WallBuys.cs`, `PerkMachines.cs`, `PowerUpView.cs`, `SurvivalGame.cs` | **In:** the run loop, points, zones and gates, ARMORY's THUNDERCLAP, run stats, perks.js (the machines; VIGOR, MEND, PLATE, QUICK, GRIP at their map spots), powerUps.js + powerUpMath.js (kill drops and the style drop; DOUBLE POINTS, OVERCLOCK on the style freshness, SHOCKWAVE, STASIS, REFUND), modMath.js + mods.js (drafts; 10 of 19 mods: SKYDANCER, WALLBANGER, LANCE, STYLE TAX, VARIETY, BLOOD PRICE, INTEREST, SCAVENGER, ADRENALINE, BOUNTY HUNTER; locked ones wait for their flags), records.js (Survival personal bests), forge.js (THE FORGE: the cabinet on THE GRINDER's core pillar, hold BUY to forge the gun in hand for 5000, once per gun per run, x1.8 and the family bonus; the statuses' rolls run on the run's seeded stream but land nothing until the arsenal is ported), READY (main.js NIGHT6 B6: hold BUY in a break with nothing for sale in reach, from survival.readyFrom). **Unity-only:** the LAB (design.lab, NIGHT8 U2-16: five opt-in experiments for Demo Day, all off: CLOSE-KILL HEAL, FRESHNESS TEETH, STYLE RAMP, READY FROM ROUND 1, SQUADS TIGHTEN; a LAB run is stamped on the results and never stored as a best). **NIGHT9:** FLIP, BOUNTY, DEADEYE, FLOW and SLIPSTREAM machines (ten of 14 perks; economy.js setPerks for BOUNTY / DEADEYE points); the forge's statuses land through the arsenal; OVERCLOCK through the arsenal; spawnMath.js pure (golden; CLASSIC ARENA uses it later). **NIGHT10:** the ARMORY wall's STITCHER (1250), COMET (2000) and LONGWAVE (3000) at their armoryWall slots (wallBuys.js order). **Unity-only (NIGHT10, D-20):** the Mystery Box (design.box, `Core/MysteryBox.cs`, `Game/MysteryBoxView.cs`; CoD's: 950, the teddy ladder 0 / 15 / 30 / 50% by pull, a teddy refunds and moves the box; 3 spots (LANDING, THE GRINDER, SLAB GARDENS) chosen by collider casts; the pool is every ported gun you don't own; a taken gun joins as a wall buy does; a seeded stream per run); the style meter's break pause (design.ultra.styleBreakPause). **NIGHT11, the run's shape (ported):** objectives.js (`Core/Objectives.cs`, golden sessions: HOLD rounds 6, 14, 22, 26 ... stand in a 4 m ring for 30 s, points x1.5 inside, a bonus of 50 x round, the round can't clear until it's held: survival.js holdRound; GOLD RUSH 7, 11, 19, 27 ...: kills x2, a 3-kill chain within 3 s pays 25 x chain; the ring's look), directorMath.js roundType (golden), protocol.js (`Core/Protocol.cs`: THE PROTOCOL's four relays on the movement showpieces, awake at round 6, hold BUY at each, the next one's guide column and the HUD's line, the SURGE through `Director.AddSquads` (unported kinds play as husks: logged), its clear forges every owned gun free and pays 2500, the hidden fifth relay; finishes not ported), traps.js (`Core/Traps.cs`, a golden session: SHOCK PLATE 750, CRUSHER 1000, GRAVITY FAN 900, ARC PYLON 1250 at their switches, the kill / elite / boss rules, TRAPPED; `Game/TrapViews.cs`, the pylon's arcs drawn simply), runMoments.js (`Core/RunMoments.cs`, golden: the grade S..D, the score line, the best moments; the multi-kill window on sim time, not the web's wall clock), buyables.js's `owned` / `remove`, the HUD's FREE / OWNED. **Unity-only (NIGHT11, design flags ON):** the power switch (design.power, `Core/Power.cs`: hold E, free, on HEIGHTS' west skybridge (-42, 9, 4.5), two gates deep, found by collider casts; the traps and the relays need it: they wake at the first round begin with the power on and the round at least protocol.wakeRound), the swarm round's banner and drop (design.zombies.swarmDrop, `Core/SwarmDrop.cs`: its last kill drops a power-up from the run's stream), exfil (design.exfil, `Core/Exfil.cs`: a radio at the spawn in the breaks before rounds 11, 16, 21 ...; the next round's site (SLAB GARDENS, THE GRINDER, a HEIGHTS spire), 90 s to reach it, then 35 kills (+10 per later exfil) in 85 s with the director at full pace; success ends the run as a win (RESULTS: EXFIL), failure as a death; records keep the flag). **Missing:** STEADY, ANVIL, STATIC, PHOENIX, nine mods (they need blasts and the other guns), the other 8 wall guns (guns.js), gadgets; the Mystery Box's Fire Sale. (NIGHT12: the PROTOCOL's and the secret relay's finishes, unlock flags and tips are in: see Other modes; the JSON profile holds the records now, migrated once from PlayerPrefs.) |
| Other modes (`modes/`) | Partial | `Core/Contracts.cs` (challenges.js + unlocks.js), `Heat.cs` (mutators.js), `Seeds.cs` (seeds.js; NIGHT24: `Daily.cs` / daily.js removed), `Mastery.cs` (finishes.js, trialMath.js medal times), `Tips.cs` (ui/tips.js), `Profile.cs`; `Game/UiMeta.cs`, `UiContracts.cs`, `UiWeapons.cs`, `UiPayoff.cs`, `UiOnboarding.cs` | **NIGHT12:** challenges.js (the 24 badges, their progress in the JSON profile; 11 can't progress yet: no bosses, ARCLINE, affixes or Thumper in the port), unlocks.js (flags in the profile; main.js's earn() triggers), mutators.js (LOW GRAVITY listed but unavailable: the Mover has no gravity factor), daily.js and seeds.js (a local DAILY: today's UTC seed, its twist (a mutator and a first mod: Mods.Give = mods.js give), the first finished run counts, the streak; the seed code on RESULTS, RETRY SAME SEED), finishes.js, tips.js. Their web tests ported (runModes.test.js mutators, night6.test.js DAILY and the PROTOCOL finish, finishes.test.js). **Unity-only (design flags):** CONTRACTS to 39 (design.contracts: 15 port-only, variety unlocks only, the first in rounds 1-2), HEAT rungs 1-3 (design.heat: GLASS CANNON x1.25, + SPEED DEMON x1.5, + NO REGEN x2; an EXFIL on rung N opens N+1; opt-in), MASTERY (design.mastery: 3 tiers per gun, a finish each), the P grade (design.grade.p), the PAYOFF strip (design.payoff), the first-run script (design.onboarding). **NIGHT14:** kiosk.js (`Game/Kiosk.cs`, `UiKiosk.cs`, `SurvivalGame.Kiosk.cs`: `-skygrindKiosk` = the web's `?demo=1`; the hints panel with the live bindings' labels; 60 s idle to TITLE (`kioskReset`); PAUSED / TITLE without a way out) and its attract loop adapted (no Duel arena: the AUTOPILOT plays SURVIVAL behind TITLE, god mode as the web's, the flythrough orbit of main.js :2337; **Unity-only:** on a normal TITLE too after design.attract.idleSeconds, D-54). **Missing:** tutorial, boss rush, duel, trials, the web's trial ghosts. **NIGHT21:** endless (`modes/endless.js`) is ported as THE GRIND (`Core/Endless21.cs`, `Records21.cs` for records.js' best wave per HEAT; behind `design.n21.grind`, on TITLE's MODE row; not yet in the smoke). deathRecap.js and runMoments.js (NIGHT11) are ported. |
| Run tools and readability (Unity-only, NIGHT14) | **Unity-only** | `Game/Tape/*` (RunTape, TapeRecorder, TapeReplay, TapeScript, TapeFiles), `SurvivalGame.Tape.cs`; `Game/Recap/*` (FlightRecorder, RecapStage), `SurvivalGame.Recap.cs`, `UiRecap.cs`; `Game/Route/*` (RouteTrack, RouteMap, RouteCard, GhostTrack, GhostStore, GhostRunner), `SurvivalGame.Route.cs`, `UiRoute.cs`, `UiGhost.cs`; `Tools/SimHarness` | **THE TAPE** (design.tape, D-56): every player run's inputs and the sim's non-input answers (onScreen, the assist mode / cap, the clock, the profile at the start), ~159 KB per minute, the last 20 beside the run logs; `-skygrindReplay <tape>` prints EQUAL or the first differing step. **WHAT GOT YOU** (design.recap + the RECAP setting): a flight recorder of the last 6 s; on DOWN the last 4 s replayed from an orbit camera, the killer outlined in red-orange and named, the last 0.5 s slowed, skippable; then DOWN / RESULTS as before. **THE ROUTE** (design.route): the run's track on RESULTS (a cyan speed ramp, kills, the DOWN mark, round ticks), SAVE CARD (a 1200 x 630 PNG to Pictures/SKYGRIND), COPY SEED. **THE GHOST and SPLITS** (design.ghost, D-53): a seed's best route as a faint cyan runner, round splits against it. **Tools/SimHarness:** the sim composition compiled outside Unity (dotnet) for headless runs (fingerprint, twice, slate, tape, autopilot, farm). |
| Style meter (`style/`) | **Ported** | `Core/Style.cs` (StyleMath), `StyleCore.cs` (styleCore.js), `StyleMeter.cs` (styleMeter.js), `StyleSim.cs` (styleSim.js); `Game/Ui.cs` (the rank widget) | styleMath, styleCore (a 107-op golden session), styleMeter, styleSim (paceReport golden). Ranks multiply points, regen and stamina; the assist payout applies; the arsenal's FRESH (getFresh), CROSS / COLLAPSE / OVERLOAD / SWAP and IMPACT awards; FLOW's perk mods. **Unity-only:** the PERFECT PARRY award; the LAB's FRESHNESS TEETH (FRESH x1.5, a DULL tier x0) and STYLE RAMP (the state multiplier 1.2 -> 3 over 1 s), both off by default; NIGHT10's design.ultra (ON): no drain in a break or with no enemy within style.trickEnemyRange, and FRESH counts for style and points but not damage. |
| Bot (RIVAL, Duel) | **Core ported (no Duel yet)** | `Core/Bot/BotBrain.cs`, `BotTypes.cs`; `Game/Bot/AutopilotInput.cs` (+ AutopilotTarget, AutopilotShopper, AutopilotGuns, AutopilotParams, AutopilotNavView, AutopilotStandInBrain), `Farm.cs`, `BotNav.cs`; `Tools/farm-report.cjs` | **NIGHT14:** botBrain.js ported line for line (ENGAGE / CHASE / EVADE / RECOVER, the reaction-delayed perception, the speed-scaled aim error, the nav override, RIVAL PRIME's pickWeapon / directChase hooks), `rng` a Mulberry32 stream, golden (6 sessions, 7200 steps, 0 mismatches), zero allocation per step. The web's quirks kept (reset() keeps the aim error, strafe and look; a failed CHASE plan keeps the old path; every gun but the shotgun uses the revolver's ranges). **Unity-only (design.autopilot, design.farm):** the AUTOPILOT (BotBrain on the player's body in SURVIVAL: a target adapter for the horde, a shopper, guns by distance, HOLD rounds, no aim assist, the meta silent), THE FARM (`-skygrindFarm`, one JSONL line per run, `farm-report.cjs` + its diff mode). **Missing:** RIVAL as an enemy, the DUEL mode and arena, RIVAL PRIME, healthOrbs (getOrbs is empty). |
| Players (co-op readiness; no web module) | Partial | `Core/Players.cs` (Player, Players); `Husks.cs`, `Spitter.cs` (SpitterAcid), `HuskBrain.cs`, `NavField.cs`, `Combat.cs` (Loadout.owner); `Game/SurvivalGame.cs` (players, p1) | **NIGHT9 (new design, COOP-RESEARCH's getTarget(from)):** each enemy hunts its nearest living player (a tie keeps the earlier, nobody alive: the first); a spitter leads its own target; a glob hits the player it touches, each stands in the acid alone; the brain thinks, checks line of sight and flanks against its enemy's nearest; the flow field seeds one goal per living player; the loadout's owner id on its hits, kills, parries, shots, swings, IMPACT bonuses, impact waves and arsenal events. One player plays exactly as before (golden, replays). **Missing (N10, U4-02):** the director per player, one wallet each, buyables, power-up grabs, zones, the mods' hooks, owner on the bare-string events (fire, weaponSwap, ...), a second player's input / camera / HUD. |
| Player + input (`player/`, `input/`) | Partial | `Game/InputReader.cs`, `CombatReader.cs`, `Game/Input/*` (ReaderInput, PadReader, WeaponSelect, KeyCodes, Glyphs, Rumble, InputPrefs), `Core/Gamepad.cs` (GamepadMath), `HoldToggle.cs`, `Bindings.cs`, `MenusMath.cs`, `Viewmodel.cs`, `CameraFeel.cs`, `GameSettings.cs`, `SettingsView.cs`, `Core/KickStack.cs`, `Trauma.cs`, `Settings.cs` | **NIGHT13:** gamepad.js (dead zones, curve, the button table at 0.35, menu navigation, MergePad, the pad remap; 20 web tests + a zero-allocation one), holdToggle.js (SLIDE / ARCLINE toggles, 6 tests), bindings.js (live key rebinding with the swap, UNBINDABLE, keyName, fillKeys; 6 + 2), menus.js' focus helpers (controlRows, primaryIndex, keyGrid), the pad's breakaway cap in AimAssist (default +infinity), SNAP MODE (PAD), the CONTROLS screen (keys and the GAMEPAD table), pad menu navigation (UiNav), the hotbar input (1-0, the wheel cooldown, quick swap, the Tab radial), glyphs (neutral / Xbox / PlayStation in words), rumble (off by default), the settings in a JSON file with a typed view. **In:** keyboard and mouse, pointer lock, the camera feel (FOV ramp, roll, kicks, landing dip), the trauma shake (fx/trauma.js, render only), ADS sensitivity x settings.adsSensitivity, the settings (settings.js: the web's keys, migrations, saved in PlayerPrefs). **NIGHT12:** viewmodel.js's life (breath, bob, sway with weight-class lag, strafe roll, slide dip and roll, wall tilt, air lift, dash pull, land dip, the swap / new-run raise, the wall pullback, the KESTREL's cylinder step, the THUNDERCLAP's pump, the gem's FRESH dim and flash, trauma's share, ADS damping, the gauntlet's afterimages), CameraFeel's aim-lock FOV pinch, the wall-ride pre-tilt and the speed FOV cap, the dash edge tint (MovementFx); finishes on the viewmodel (BlockyKit.Finish: the four body tones only). **NIGHT16 (ported):** viewmodel.js's left arm (`armRest` / `armPunch`, :157-161, :682-689, :925-928; the fist's out-travel reaches full extension inside a punch's freeze, port-only) and the cooldown-ready glint with its `weaponReady` event (:845-862); player.js's FOV kick stack (:166-212: a multikill and a rank-up widen, a melee hit narrows; `Game/ImpactFrames.cs`). **NIGHT16 (Unity-only):** a draw per gun inside the sim's swap timer (KESTREL spin + flick, THUNDERCLAP rack, COMET shoulder, LONGWAVE sweep, GAUNTLET clench + gem; a shot cancels it the same frame; `weaponDraw`), the holster to the hip, a quick-swap flick, an idle flourish in a break, the hands on take-off / landing / the wall-ride wall / the slam, the recoil kicking inside its own freeze, knuckle flashes on a parry (`Game/Viewmodel.Draw.cs`, `Viewmodel.Arm.cs`); the crosshair movement HUD (the dash pips and the slide charge, the web's pip flash `hud.pipFlashTime` ported). **Missing:** the STEADY recoil; Steam Input glyphs (N17); a Switch Pro pad's swapped confirm / back. |
| World + collision (`world/`) | Partial | `Core/ThreeOctree.cs`, `TriangleWorld.cs`, `GameWorld.cs` (boxLayer), `CoreGrid.cs`, `Collision.cs`, `SurvivalMap.cs`, `ArenaMap.cs`, `PortData.cs`; `Game/SurvivalBuilder.cs`, `Look.cs`, `MeshKit.cs`, `CoreView.cs`; `Shaders/Stone`, `Sky`, `Field` | **Exact:** the collider, box layers, the core grid. **Approximated:** the render (URP). **NIGHT12:** Stone.shader receives URP's SSAO and Forward+ point lights (the _CLUSTER_LIGHT_LOOP), the web's height fog coloured like the sky's gradient. **Missing:** aoVolume, skyline, the full worldMaterial. |
| Maps (Unity-only, NIGHT15; the web's maps.mjs / navGrid.js / zoneMath.js resolveZones ported for it) | **Pipeline + a greybox** | `Core/Nav/NavGridBuilder.cs` (createNavGrid, line for line), `Core/ZoneMath.cs` (ResolveZones); `Game/MapAuthoring/*` (markers, MapModel, MapModelJson, MapJsonWriter, MapRoundTrip, MapValidate, MapImport / MapExport / MapAuthor); `Editor/Maps/MapMenu.cs`, `Build_drydock.cs`; `Game/SurvivalGame.Maps.cs` (the MAP choice); `Design/maps/` (drydock.plan.json, drydock.json); `Tools/SimHarness` mapgen / mapvalidate / newmap / verbs | **NIGHT15:** the C# nav grid equals the exported grid of survival, spire and training bit for bit (and survival's 114 zone parts); a map authored in a scene (markers) or from a plan file exports the web exporter's schema with its navGrid / zoneParts (survival.json round-trips byte-identical in its sim keys, its fingerprint EQUAL); a validator (zones in price order and spot times on RIVAL's graph, spawn nodes per zone, pads, islands, the kill plane; designed verb gates as VERB rows); TITLE's MAP row (ARENA | DRYDOCK; a pick reloads the scene; records keyed survival / survival@<id>; automation boots the arena). DRYDOCK (MAP-DESIGN.md, D-59 GG's): the heart built (gangway, cradle, ribs, slipway), validator 0 findings, the AUTOPILOT 5 rounds in 300 s, the zones' moves measured on the Mover. **Missing:** DRYDOCK's other four zones, its real FIREBOX / HOOK relays, per-run variation, the look; arbitrary triangle meshes (NavMap). |
| UI (`ui/`) | Partial | `Game/Ui.cs` + partials `UiSettings.cs`, `UiPerks.cs`, `UiPowerUps.cs`, `UiMods.cs`, `UiFeel.cs`, `UiMarkers.cs`; NIGHT17: `UiMenus.cs`, `UiKit.cs`, `UiTheme.cs`, `UiTween.cs`, `UiShape.cs`, `UiStyleMeter.cs`, `UiPointsPops.cs`, `UiCeremony.cs`, `UiStaminaBar.cs` | **NIGHT17 (Unity-only, the overhaul; NIGHT17-LOG.md):** every screen restyled through `UiTheme` (IDENTITY.md); the HUD in three tiers (HP and points as big numerals on INK plates, the style meter on its own plate with a slam, named callouts and kill streaks, merged points pops flying into a punching counter, the round's hero numeral and its end band, the stamina bar, no key hints, speed or mod chips at rest); TITLE 4 items + a setup card, PAUSED 4, SETTINGS at most 5 tabs and 8 rows with sliders and footers, RESULTS one card with 3 buttons, MODS cards with glyphs; one focus state everywhere; drawn key chips. **In:** the HUD (four nested canvases; texts only on change), TITLE / PAUSED / DOWN / RESULTS, SETTINGS (17 settings in 4 tabs, RESET TAB; Unity-only: the LAB tab), the DOWN recap (deathRecap.js), the personal-best line (or a LAB run's stamp), the results' FORGED row, a forged gun's "+", the break line's HOLD E: READY, R to restart, damage-direction arcs (hitDirection.js), enemy markers, the perk row, the running power-ups, the MODS draft, the freshness tier by the weapon name (the arsenal's FRESH, NIGHT9), speed lines, the parry flash, the momentum ring round the crosshair (ui/momentumArc.js, NIGHT9). **NIGHT11:** the web's `banner` event shown (queued behind a round's own banner: the web overwrote them in the same frame; port-only), the HUD line's objective / PROTOCOL / exfil parts (main.js topText), RESULTS' grade, best moments, PROTOCOL row, score line and the EXFIL / EXFIL FAILED head, the buy prompt's FREE and OWNED (hud.js buyLabel), ULTRA HEALTH's grey cap on the HP bar (LAB). **NIGHT12:** the theme fonts (Chakra Petch + Barlow Semi Condensed, OFL TTFs, static TMP atlases made by `Editor/ThemeFonts.cs`, no imported kerning), hud.js's hitmarker grammar (body / head / kill / elite) and kill flash, hud.js's panel skew, damageNumbers.js (off by default), SETTINGS' QUALITY / TONE MAPPING / FILM GRAIN / ENEMY X-RAY / DAMAGE NUMBERS rows, the run setup on TITLE, CONTRACTS, WEAPONS, the toasts, tips' hint toast, RETRY SAME SEED. **Unity-only (design.ceremony, design.payoff, ON):** the round ceremony (the number flashes at the clear; the new one burns in and flies to its place; tally strokes for rounds 1-5), RESULTS as a staged tally (counting cards, the grade slam, a HEAT badge, NEW BEST stamp, the seed code), the HP ghost and the points roll, a living title (a camera path over the map, pulsing strips), the PAYOFF strip on DOWN / RESULTS, the first-run lesson cards. **NIGHT16 (u3-n16), hud.js ports the port never had:**
the rank-up pop (letter and name), the crosshair's spread (dash, THUNDERCLAP), the "+N" points pops (`UiPointsPops.cs`),
the damage vignette, EXPOSED's pulsing edge, the buy prompt's poor / denied states, the regen sheen and REGEN tag
(`UiFeelEdges.cs`), the move row: speed and wall-jump ticks (`UiMoveRow.cs`), the ADS ring, the LONGWAVE's scope and
rangefinder, the arms fading while zoomed (`UiAds.cs`), the crosshair tag (PRIMED / CROSS / SWAP) and the charge edge
(`UiFeelTags.cs`), the award feed's "+ NAME n" in the rank's colour and zero-point tricks dimmed, the rank bar's pulse,
R from PAUSED, RESULTS' RETRY (R), the 0.15 s fade-in on every run start and return to TITLE (the "in" half), the mod
chips in the perk row (`UiPerks.cs`). **Unity-only (NIGHT16):** the movement ticks at the crosshair, the headshot's diamond, the screens'
transitions, the rank plate's zero state, the menu focus slide and press dip (`UiFeelHud.cs`, `UiNav.cs`, `UiPress.cs`).
**Not ported (NIGHT16, with reasons in FEEL-DESIGN.md section 5):** the rank-up screen-edge glow (rule 8), the style
multiplier chip, the feed's slide-in. **Missing:** CONTROLS, tooltips, boss card, the hotbar and radial, the weapon bar, COPY SCORE LINE. |
| FX (`fx/`) | Partial | `Game/EnemyViews.cs` (Particles), `TelegraphView.cs`, `SurvivalBuilder.cs` (ZonesView), `ImpactFx.cs`, `Core/KickStack.cs`, `Trauma.cs` (+ cosmetic noise), URP bloom (Look.cs) | **In:** hit, death and impact bursts (pooled cubes), the spawn telegraph beams, the gate fields, camera kicks, the trauma shake, bloom; NIGHT9: the impact wave's ring (simple: an expanding flat ring + the web's burst; the full impactFx.js look is N12). kicks.js and springs.js are golden-checked. **NIGHT10** (`Game/WeaponFx.cs`, `Decals.cs`): weaponFx.js (the LONGWAVE's charge laser and dot, the beam flash, the ricochet bounce line; not the Prism preview) and effects.js's decals (fx.decalCount / decalSize / decalLift, colors.decal; hitscan world hits only, none on a force field; one per THUNDERCLAP shot where the web leaves one per pellet). **NIGHT12:** post.js on the URP Volume (`Look.cs`, `PostFx.cs`: bloom and the vignette from config, grade.js's split tone, chroma pulses on hurt / dash / rank / speed, the low-HP vignette and desaturation, grain and SMAA by quality), lightPool.js (`LightPools.cs`: pooled Forward+ point lights at the machines, THE FORGE, the box, the relays, the radio, the power switch; blast flashes; effects.js's muzzle flash light and kill / impact pops; the first-run guide pulse), motes.js (`Motes.cs`, one draw), blobShadows.js (`BlobShadows.cs`, one instanced draw), killConfirm.js (`KillConfirm.cs`, one instanced draw), instanced particles (one RenderMeshInstanced; no GameObject per cube), the movement FX (`MovementFx.cs`: land puffs, slam rings, wall-ride streaks + port-only sparks), WeaponFx's tracer per ray, the impact flare and port-only shells, the Mystery Box reel. **NIGHT16 (u3-n16):** particle bursts at the web's sizes (particles.js's scale), the quality's particle cap (oldest
recycled), the impact ring's round(18 x detail) burst, the melee hit's family sparks, swarmer / spitter death bursts at
mid-body, the muzzle light at the drawn muzzle, OVERLOAD's volt bursts (a stand-in for arcs.js), a THUNDERCLAP full miss
without mid-air sparks, the spawn beams' own pulses, REDUCED MOTION on the port-only flinch and shells (`FxGates.cs`).
**Missing:** aura, arcs, rope, god rays, barrierMath ripples, statuses' looks (the web has none), the per-gun muzzle
stars, the world's rank energy (FEEL-DESIGN.md section 6). |
| Assets (`assets/`) | Partial | `Game/BlockyKit.cs`, `MeshKit.cs`, `PerkMachines.cs`; `Assets/Fonts`, `Assets/Resources/Fonts` | The blocky kit viewmodels (a forged gun's gem burns brighter; NIGHT12: the THUNDERCLAP's missing pieces, finishes by palette); the perk and FORGE cabinets (machineModels.js simplified); the theme fonts (NIGHT12). No GLB drop-ins. |
| Audio | Not started (GG owns it) | none | No audio code by design. The event list below is the hook map. |
| Core (`core/` events, flow) | **Ported** | `Core/EventBus.cs`; the flow is `SurvivalGame.Flow` | The bus no longer copies its handler list per emit (the web's semantics kept, tested); int payloads are boxed once per value. |
| Boot / loop / config (`main.js`, `config.js`) | Partial | `Core/GameConfig.cs` (config.json + `Design/unity-config.json` as `design.`), `Game/SurvivalGame*.cs` (boot, the frame loop, the views), `Game/SurvivalSim*.cs` (NIGHT13: the sim composition, no UnityEngine; FingerprintTests runs it headless), `UnityTuning*.cs`; NIGHT16: `Core/ImpactRules.cs`, `Game/ImpactFrames.cs`, `Game/SurvivalGame.Impact.cs`, `Game/FlashGate.cs` | **NIGHT16 (Unity-only, design.impact; the Game layer: time and pictures, the composed fingerprint unchanged):** tiered freezes (kill 0.04, heavy hit 0.03, headshot 0.05, headshot kill 0.08, punch hit 0.06, punch kill 0.09, parry 0.25, PERFECT PARRY 0.30), slow-mo (the round's last kill 0.3 x, the perfect parry's tail, a 3+ multi-kill), a budget (0.12 s frozen in any second, 6 % a minute, freezes 0.5 s apart unless a bigger moment, a 0.5 s cap, parry repeats short), the loop's time scale (fewer 120 Hz steps per real second, never a different step), the sim's own hitstop drained into it; HIT FREEZE / SLOW-MO settings; one flash gate (WCAG 2.3.1); `impactFreeze` / `slowMo`. **In:** the 120 Hz fixed step, hitstop, the simulate order for the ported systems (with a ProfilerMarker each; NIGHT13: pinned by the composed fingerprint, `FingerprintBaseline.cs`), save-and-quit on a run snapshot (design.save, port-only), every gameplay number from config.json, the WebGL boot (the JSON by URL). **Missing:** everything main.js wires that isn't ported. |
| Debug (`debug/`) | Not ported (by design) | `Game/SliceSmoke.cs`, `PlayerChecks.cs`; F3 overlay, F8 opens every gate, F9 skips to the next round | The slice smoke (now: a perk buy, the Mystery Box rolled and taken, a kill with each of STITCHER, COMET and LONGWAVE bought on the wall, the recap, R from DOWN and RESULTS, the spitter round; NIGHT11: the power switch, a SHOCK PLATE kill, round 6's HOLD held with the four relays lit and THE PROTOCOL completed, exfil at round 11 to RESULTS; NIGHT12: a contract completed with its unlock in the profile, a DAILY on HEAT 1 with the badge on RESULTS; the profile snapshotted and never written) and the Player's `-skygrindSmoke` / `-skygrindBench` (a Web build: `?skygrindSmoke`, `?skygrindBench=N`). NIGHT13: the smoke adds the run log, save and resume, the radial, a rebind and a virtual pad (play and menus); `-skygrindFingerprint`; the bench's per-frame breakdown, `play` and `-skygrindFpsCap N`; `-skygrindPerfOff <names | all>`; F3 / F8 / F9 only in the Editor or with `-skygrindDev`. |

**Web systems with no row above** (the web added them after `PORT-INVENTORY.md` was taken: 29 files, 5,542 LOC):
- modes: trials, ghosts + ghostCodec: **Not started**; finishes, daily (**removed NIGHT24**), seeds, unlocks: **ported NIGHT12** (trialMath's
  medal times with finishes); runMoments: ported NIGHT11
- Survival: tips: **ported NIGHT12** (objectives: ported NIGHT11)
- input: gamepad, holdToggle: **ported NIGHT13** (with bindings.js and menus.js' focus helpers); core: framePace:
  **superseded** by Unity's `targetFrameRate` (the FPS CAP row; `SurvivalGame.ApplyFrameCap`)
- FX: quality.js / qualityMath.js: **ported NIGHT13** (`Core/Quality*.cs`, quality.test.js's 6 cases + an AUTO test;
  `Game/Quality.cs` QualityRuntime: AUTO, render scale, per-preset shadows on a runtime copy of the URP asset, pinned to
  HIGH under `-skygrindBench`); the presets' particle cap, aura, impact, telegraph, AO, sheen and shadow radius not
  ported (nothing reads them yet)
- FX / UI: killConfirm: **ported NIGHT12**; momentumArc: ported NIGHT9

`PORT-PLAN.md` section 2 tracks every item against the web, night by night, and `PORT-RESEARCH.md` §0 has the sizes.

## Events the port emits (for the audio map)

The names are the web's (`events.emit` in the web source) unless marked **port-only**. Payloads are C# classes with
the web's field names where they exist (HitEvent, KillEvent, HurtEvent, RoundEvent, SpawnEvent, AimLockEvent, ...).
Some high-rate payloads (HuskAttackEvent, AimLockEvent) are reused: read them in the handler, don't keep them.

**Player movement** (Mover.Events, mover.js `_emit`): `jump`, `doubleJump`, `dashJump`, `boostJump`, `wallJump`, `dash`,
`dashFail`, `land`, `slam`, `slideStart`, `slideEnd`, `wallRideStart`, `wallRideEnd`, `jumpPad`, `ropeFire`,
`ropeAttach`, `ropeRelease`. Also on the bus: `jumpPad` (the pad index).

**Weapons and combat:**
- shots and swings: `fire`, `shotResult`, `ricochet`, `meleeSwing`, `punch`
- hits: `hit`, `kill`, `parry` (port-only field: `perfect` on a PERFECT PARRY)
- loadout: `weaponSwap`, `weaponRaise`, `adsStart`, `adsEnd`
- pad (NIGHT24, port-only, design.n24.padY): `weaponSwitch` ({fromSlot, toSlot}; the pad's NEXT WEAPON press, a Y tap on layout v4, moved the slot in hand; `ReaderInput.ApplyPad` emits it, the lead wires `playerInput.events`). The loadout's own `weaponSwap` follows when the sim equips the gun.
- aim assist: `aimLock`, `aimRelease`, `lockOn`, `lockOff` (the V key's manual lock, NIGHT9), `lunge` (NIGHT9)
- the arsenal (NIGHT9): `primed`, `crossHit`, `swapKill`, `status`, `overload`
- momentum (NIGHT9): `impactWave`, `styleBonus` (IMPACT); `hitstop` cause `momentum`
- the ARMORY guns (NIGHT10; the web's names, checked in weapons.js :217 / :226 / :266 / :283 / :745 and
  projectiles.js :160 / :165 / :308 / :460): `overheat` (STITCHER), `nailDetonate`, `rocketBlast`, `rocketJump`,
  `rocketFragment`, `railCharge` (`over` on the overcharge), `railCancel`, `railFire`; a projectile's hit is a
  `shotResult` with `rayCount` 0 (no tracer)
- feel: `hitstop`
- port-only: `shotHit`, `enemyStagger`, `weaponBought`, `globReflect` (a punch sent a spitter glob back)

**Player health:** `hurt`, `heal`, `death`, `regenStart`, `regenEnd`.

**Enemies:**
- `enemySpawn`, `spawnTelegraph`
- husk: `huskWindup`, `huskAttack`, `huskLeap`
- swarmer: `swarmerCrouch`, `swarmerPounce`
- spitter: `spitterTell`, `spitterVolley`, `acidPuddle`
- `enemyCommit`, `enemyAttackDone`
- nav: `navJump`, `navStuck`, `navRelocate`

**Run, director, economy:**
- run: `roundBreak`, `roundBegin`, `roundClear`, `ready`, `runEnd`
- director: `directorPhase` (port-only value: `PEAK_FADE`), `campStart`, `campEnd`, `hunters`
- zones and buys: `zoneEnter`, `zoneClear`, `gateOpen`, `buy`, `buyDenied`
- points: `points`, `pointsSpent`
- perks: `perk`; THE FORGE: `forge` ({owner, weapon, price, free})
- the Mystery Box (NIGHT10, all **port-only**: the web has no box): `boxOpen` (a paid roll), `boxRoll` (the gun),
  `boxTeddy` (the bear; the refund), `boxMove` (from, to, zone), `boxTake`, `boxSink` (an untaken gun goes back)
- power-ups: `powerUpSpawn`, `powerUpGrab`, `powerUpEnd`, `powerUpStyleDrop`, `powerShockwave`
- mods: `modDraft`, `modPick`, `modReroll`
- the run's shape (NIGHT11, the web's names): `banner` ({text}), `objectiveStart`, `objectiveDone` (HOLD / GOLD RUSH),
  `relayLit`, `protocolSurge`, `protocolComplete`, `secretRelay` (THE PROTOCOL), `trap` ({id, on}), `trapKill`,
  `trapSlam`, `styleBonus` (TRAPPED)
- the run's shape, **port-only** (NIGHT11 design flags): `powerOn` (the power switch), `swarmRound`, `swarmDrop` (the
  swarm round's banner and its last kill's drop), `exfilCall`, `exfilStart`, `exfilReach`, `exfilDone`, `exfilFail`
- port-only: `roundPlan`
- the meta (NIGHT12; Core/MetaEvents.cs): the web's `challengeDone` ({id, name}), `mutatorUnlock` ({id}),
  `fx:killConfirm` ({owner, tier, chain, position}); **port-only:** `contractDone` ({id, name, unlock, unlockName, web}),
  `unlockEarned` ({flag, name}), `masteryTier` ({weapon, tier, finish}), `heatUnlock` ({rung}), ~~`dailyDone`~~ (removed NIGHT24 with the DAILY), `tip` ({id, text}), `onboardingStep` ({round, id}); `modPick` gains `free` (mods.js give)
- the ceremonies (NIGHT12, **port-only**): `roundCeremony` ({number, phase: clear | burn}), `resultsStage` ({stage:
  outcome | round | card | grade | best | line | done, index})

**Input and quality (NIGHT13):** the web's `input:device` (payload "pad" | "keyboard") and `input:padConnected`
("connected" | "disconnected"); the web's `qualityPicked` and `renderScale` (QualityRuntime).

**The kiosk (NIGHT14):** the web's `kioskReset` ({}) when 60 s of no input sends a run back to TITLE. The bot, the tape,
WHAT GOT YOU, THE ROUTE and the ghost emit no events (they observe the sim; a sound for the recap or a split is GG's).

**Style:** `styleAward` (port-only award name: PERFECT PARRY), `rankChange`. Port-only: `trick` and `slamUse`, the
meter's own inputs; the web's meter reads mover events directly.

**Feel (NIGHT16; names in `Core/FeelKeys.cs`):**
- the web's `weaponReady` (viewmodel.js :850: a slow gun's cycle ended, the glint; payload the weapon id)
- port-only `impactFreeze` (ImpactFreezeEvent: owner, cause kill | heavyHit | headshot | headshotKill | punchHit |
  punchKill | parry | perfectParry, seconds granted, skipped over the budget or a cooldown)
- port-only `slowMo` (SlowMoEvent: owner, cause lastKill | perfectParry | multiKill, scale, seconds held)
- port-only `slideCharged` (the slide boost's charge came back, design.move.slideCharge; payload the owner id)
- port-only `weaponDraw` (a gun's draw started on the viewmodel; payload the weapon id)

**Stamina and crunch (NIGHT17; all port-only):**
- `staminaFull` (the bar refilled to full; payload `"player"`), `sprintStart` / `sprintEnd` (the held boost key ran on
  into a sprint / it ended: key up, empty, standing still, a new press; payload `"player"`), `boostPartial` (a boost
  under its full cost; payload the percent of a full boost, 25 .. 89): design.stamina, forwarded by
  `SurvivalSim.StaminaOnMover` from `MoverEventType.StaminaFull / SprintStart / SprintEnd / BoostPartial` (names in
  `Core/Stamina.cs`).
- `crunchShed` (an enemy under 50 % HP sheds a block; a reused `EnemyLook.CrunchShedEvent`, on the frame it flies) and
  `impactFrame` (the one-frame impact flash; payload `"multikill"` or `"perfectParry"`): Game layer (EnemyViews /
  PostFx). The kill's break into blocks lands on the web's `kill`, the same frame.

**The sandbox (NIGHT19; names in `Core/N19Events.cs`):** port-only `gust` (GustEvent: the KESTREL's NIGHT19 gust; off
with design.shockwave on), the web's `zap` and `discharge` (the ZAPPER), port-only `combo` (ComboEvent: the combo's name,
the payoff gun, the bonus), port-only `plateBreak` (a SHIELDED front plate broke; payload the enemy id).

**NIGHT20 (names in `Core/N20Events.cs` unless named; every one is a moment for GG's sounds):**
- Guns (builder G): the web's `doubleBlast` and `coneWave` (the THUNDERCLAP's LT), port-only `shockwave` (ShockwaveEvent:
  origin, aim, the self push, enemies pushed, charges left, the cooldown) and `shockwaveReady` (a charge came back; payload
  the charges); `adsStart` / `adsEnd` now on every zoom gun; `combo` names POP, LAUNCH + SNIPE, CONDUCTOR, NAILBOMB,
  SWAP-CANCEL and SENT (D1: a trap finished your push); the FORGE's `tier`; the TORCH's `flame`, `fireWall` (the web's)
  and `overheat` ("torch").
- Routes (builder N): port-only `climb` (ClimbEvent: the enemy, the kind, the face's foot and lip, phase start | up | top |
  knocked | land, the link kind).
- Enemies and regen (builder E): port-only `roundHeal` (the round-end refill) and `earnedHeal` (RoundHealEvent: closeKill
  | punchKill); port-only `enemyRecycle` (a stuck enemy moved out of sight, its HP kept); the web's `lancerAim`,
  `lancerFire` ({hit, offset}) and `lancerBlink` ({blind}); port-only `specialRound` (SpecialRoundEvent: name hunt |
  elite | twist:<FOG | SWARMAGEDDON | LETHAL | GOLDEN>, phase announce | start | last | end); the affixes' `affixBlink`,
  `affixVolatile`, `affixBurst`; the WARDEN's `enemyBlocked`, `wardenTell`, `wardenSwing`, `wardenShieldBreak`.
- HUD (builder H): port-only `debutCard` (DebutCardEvent: the kind, the card's seconds; the world slows <= 1.5 s),
  `rewardReveal` and `rewardPick` (the reward reveal's three slots and the pick).
- Effects (builder V): port-only `skygrindTime` (payload "blast" | "headshot": SKYGRIND TIME starts).
- Pad and onboarding (builder P): port-only `firstFifteenPrompt` (payload the verb's id) and `padHint` (the first-pad
  hint card).
- The BULWARK boss (builder EN, `Core/Enemies/Bulwark.cs` and `BossKit.cs`; behind `design.adds.bulwark`, OFF tonight):
  the web's boss events `bossIntro`, `bossBeat`, `bossTell`, `bossPunish`, `bossStun`, `bossHit`, `bossPart` (a plate or
  the vent), `bossPhase`, `bossRise`, `bossDying` (the finisher's slow-mo) and `bossDown`.
- The DRONE (builder E's rescue of NIGHT19's, `Core/Enemies/Drone.cs`): the web's `droneCharge`, `enemyShot`, `orbFire`
  and `orbDestroyed`; port-only `orbReflect` (a punched orb sent back, punch.js's orb parry; payload the shooter's id;
  `Core/Spitter.cs`).
- NIGHT21 (each behind its `design.n21.<row>` switch; `--off21` emits none of them):
  - THE ARCLINE (`Core/Arcline21*.cs`, builder M): the web's `arclineOwned` (payload bool), `arclinePull` (a yanked enemy:
    the pull event), `slingshot` (the swing's release; also `trick` "slingshot" on the bus) and `gadget` ("arcline", the
    terminal buy); port-only `arclineMiss` (a press with nothing in range). The mover's `ropeFire` / `ropeAttach` /
    `ropeRelease` carry the zip flag.
  - SLIDE BITES (`Core/SlideBite21*.cs`): port-only `slideBite` (SlideBite21Event: id, kind, damage, killed, x/y/z) and
    `styleBonus` "SLIDE BITE"; a slide kill also pays CLOSE CALL.
  - THE STAMP (`Core/Grades21.cs`, builder S): port-only `roundGrade` (RoundGradeEvent: round, grade, letter, p, score, damage, time,
    peakRank, kills).
  - DEADLINE (`Core/Deadline21.cs`): port-only `deadlineTick` (DeadlineEvent: fuse seconds left, its full length, the share left;
    every `tickEvery` s and at each refill) and `deadlineBurn` (DeadlineEvent, hit set: the fuse ran out; never lethal); `specialRound` name `twist:deadline`.
  - THE GRIND (`Core/Endless21.cs`, builder W, the web's endless.js): the web's `waveBreak`, `waveStart`, `waveClear`
    (WaveEvent: number) and `runEnd`; `banner` carries the WAVE banner text.
- NIGHT22 (each behind its `design.n22.<row>` switch or the NIGHT21 row it finishes; `--off22` / `--off21` emit none of them): no new event
  names. Reused: TRICK CHAIN (`Core/Trick22.cs`, `design.n22.trickChain`, off) pays through the existing `styleBonus` with `name` "CHAIN x<links>";
  the stamp, the DEADLINE bar and its "+N" pop, the directed cue (THE HUNT / THE BEACON steps), the dusk clock, the haptics (`Core/Haptics22.cs`
  on `Rumble`: shockwave, ropeAttach, arclinePull, kill, death) and the INSTANT REPLAY view are pictures of existing events
  (`roundGrade`, `deadlineTick`, `deadlineBurn`, `specialRound`, `shockwave`, `kill`, `death`). Tape format 7 carries the NIGHT22 byte.
- Older, listed here because it was missing: the web's `airburst` (`Core/Projectiles.cs`, the rocket's mid-air burst).
- NIGHT24 (each behind its `design.n24.<row>` switch; `--off24` emits none of them), port-only, every one a moment for GG's sounds:
  - THE BIG PICK (`Core/BigPick24.cs`): `bigPick` (BigPickEvent: round, offer = the card ids) when the offer opens; `bigPickTake` (round, id).
  - THE CHARGED FIST (`Core/Fist24.cs`): `fistCharge` ({charge, tier, gain, why: hit | fistKill | otherKill | hurt}); `fistFlick` ({tier, spent,
    distance, speed, air}); `fistFlickBlocked` (the room in metres: less than minRoom, nothing spent).
  - CAMPAIGN (`Core/Campaign24.cs`; RoomEvent: room, id, name, kind, par, time, limits, withinPar): `roomEnter`, `checkpoint` (a door that saves),
    `roomClear`, `roomRetry` (a death in the room), `layerClear` (the layer's last room; time = the layer's total), `weaponEarned`
    (WeaponEarnedEvent: weapon, name, room); `gateLock` (GateLockEvent: zone, name; `Core/Zones.cs`, a campaign door that shut behind you; the
    web's `gateOpen` is the opposite).
  - THE KILN WYRM (`Core/Enemies/Wyrm.cs`, `design.n24.wyrm`): the web's boss names only: `bossRise`, `bossPhase`, `bossTell` ({name: spit | lunge |
    dive | crack | squeeze | roar | shed, seconds}), `bossHit` ({damage, part: head | body | tail | core}), `bossPart` ({part: core | tail, reason:
    exposed | closed | shed | regrown | destroyed}); the kit's `bossIntro`, `bossBeat`, `bossDying`, `bossDown`.
  - REMOVED in NIGHT24 (GG: "Remove the replay feature", "Remove the daily and weekly day mode"; no audio hook depended on them): `dailyDone`
    (`DailyEvent`, `MetaEvents.DailyDone`) and its RunLog line; the DAILY's `banner` ("DAILY · <mutator> · FIRST MOD <mod>") and the WEEKLY's twist; the
    DAILY ghost's AHEAD / BEHIND verdict (no event; `GhostDaily22`); INSTANT REPLAY emitted no event of its own (its ring read the sim; `Replay21Ring`,
    `Replay22View`, the VIEW prompt and `design.n21.replay` are gone). `RunSetup.Daily` is gone; `RunStats.daily` / `dailyDay` / `dailyCounted` too (the run
    snapshot reads keys by name, so an old save with them still loads); the contracts kind `daily` and the `dailyHabit` row are gone (the flag id `daily`
    stays as the INTEREST mod's unlock). Tape bits kept so old tapes read: NIGHT21 bit 5 (`retired5`, was replay) and NIGHT22 bit 0 (`retired0`, was ghostDaily).
  - The pad's `weaponSwitch` is listed with the pad above; `boxRoll` is the Mystery Box's (the BIG PICK's free spin reuses it).

**NIGHT42 (the feel night: GG's fifteen asks from n41-r7; names from the builders' reports and their commits on port/stage3 at the freeze
a523f935 (not re-read from every `Emit` call); port-only; `--off42` / `-skygrindOff42` emits none of them; GG's sounds hook here):**
  - the gauntlet's hit (design.n42.impact42, Core/ImpactRules.cs + Game/SurvivalSim.N42.Impact.cs): `gauntletHit42` (a gauntlet hit;
    payload GauntletHit42Event with its class husk / big / boss), the impact freeze cause `gauntlet42`; `killTier42` (a kill's tier: 1
    plain, 2 heavy, 3 melee, 4 big; payload KillTier42Event).
  - THE ICEBREAKER (design.n42.brawl42, Brawl40 additions): `brawl42Sweep` (stages tell / swing / hit / recover), `brawl42Leap` (stages
    tell / lock / jump / land / hit / ring / stuck / free).
  - the charge (design.n42.charge42, Core/Guns/Charge35.cs): `charge42Rearm` (a held fire started the next charge after the cooldown).
  - OFF tonight (named for later): `parry42Buffer` (parry42: an orb caught by the early buffer / mercy), `arena42Ground` (arena42: an enemy
    knocked into THE GRINDER; styleBonus GROUND 80), `arena42CoverHit` / `arena42CoverBreak` (breakable cover; SHATTER 60).
  - wallfling42, foundry42, gauntlet42, meter42, style42, health42, kills42 add no event.

**NIGHT41 (the finish night: the faster pump, the doors, the boss entrance and bars, the ice canal's light, THE LOOM's floor sweep, the
token cap, the gauntlet's heavy hit, level 5's grapple nodes; names read by the lead from the `Emit` / `Say` calls on port/stage3 at the
freeze; port-only; `--off41` / `-skygrindOff41` emits none of them; GG's sounds hook here):**
  - THE LOOM's floor sweep (design.n41.sweep41, `Core/Enemies/Loom37.N41.cs`): `loom41Sweep` (payload Loom37Event; stages "tell" (the
    red-orange sheet lights, 2.0 s before the line), "hit" (the line caught the player), "ride" / "rope" / "high" (the player answered it
    by a wall ride, the grapple or height)).
  - the gauntlet's heavy hit (design.n41.heavy41, `Core/Heavy41.cs`): `heavy41` (a gauntlet hit on a big kind: tank, WARDEN, CHARGER,
    THE ICEBREAKER; payload the enemy's id; with it the sim's 0.14 s hitstop, cause "heavy41").
  - juice41On / doors41On / pump41On turn NIGHT38 / NIGHT40 views on (their events as listed there: `boss38Entrance`, `hurt38Static`,
    `hurt38Tick`); entrance41, bars41, look41, brawlLook41, tokens41, foundry41, kiln41 and spire41 add no event.

**NIGHT40 (the build night: the start grace, the boost taught, one charge arc, the endless ride, level 2 + THE ICEBREAKER, THE LOOM and
THE CROWN made fair, the DRONE / WARDEN / CHARGER in the campaign; names read by the lead from the `Emit` calls and event-name strings on
port/stage3 at 864def74; port-only; `--off40` / `-skygrindOff40` emits none of them; GG's sounds hook here):**
  - the start (design.n40.start40, `Game/SurvivalSim.N40.Start.cs`): `start40Release` (the first wave is let go: "move" or "timer").
  - the THUNDERCLAP's charge (design.n40.charge40, `Core/Guns/Charge35.cs`): `charge40Press` (the trigger went down under charge40's rule).
  - the boss line (`Core/BossLine40.cs`, used by THE CROWN, THE LOOM, THE ICEBREAKER): `bossLine40` (a boss's rule line shown).
  - THE ICEBREAKER (design.n40.brawl40, `Core/Enemies/Brawl40.cs`): `brawl40Rise` (the walk-in reveal), `brawl40Haymaker` (the heavy
    punch's tell / strike), `brawl40Slam` (the ground slam's ring), `brawl40Block` (the thrown ice block), `brawl40Rush` (the charge when
    the player is far), `brawl40Stagger` (the stagger window opens), `brawl40Phase` (phase 2), `brawl40Floe` (phase 2's floes: "crack",
    "open", "bite").
  - THE LOOM (design.n40.loom40, `Core/Enemies/Loom37.N40.cs`): `loom40Latch` (the grapple latched a lit knee), `loom40Practice` (o2's
    practice knee: "latch" / "tear" / "back"), `loom40NoRide` (the no-ride phase walls: "warn" / "on" / "off").
  - the arc (design.n40.arc40): NIGHT38's `hud38Ready` now also fires with the id "nailgun" (the STITCHER's lock ends; arc40 only).
  - kiln40's lesson chain rides on teach35's events (trigger "kiln40"); crown40, foes40, ride40, rods40, cut40, models40, pump40 add no event.

**NIGHT39 (the feel night: the pistol's hold, the calmer ride, the ride + aim slow, 1-2 minute bosses, level 3 rebuilt; names read by
the lead from the `Emit` calls and event-name strings on port/stage3 at the freeze 92eb280a; port-only; `--off39` / `-skygrindOff39`
emits none of them; GG's sounds hook here):**
  - the pistol (design.n39.p39; `Core/Guns/Charge35.cs`): `p39Press` (the trigger went down on the KESTREL: the hammer-cock sound; payload
    ChargeShot27Event). Named in `Game/Pistol39View.cs` for the view: `p39Hit` (a KESTREL hit freeze), `p39Cannon` (the charged shot's
    blast).
  - the damage static (design.n38.juice38, `Game/Juice39Hurt.cs`; name in `Game/UnityTuning.Juice38.cs`): `hurt38Tick` (a damage-over-time
    tick: a faint static, no shake), next to NIGHT38's `hurt38Static` (a real hit's full burst).
  - ride39, slow39 and boss39 add no event (slow39 is loop pacing; boss39 is numbers).

**NIGHT38 (the flight night: every level rebuilt for flight, walls you stay on, doors you can see, the HUD's charge bars, the boss
juice; names read by the lead from the `Emit` calls and event-name strings on port/stage3 at u3-n38, written in NIGHT39 (C1 row 11);
port-only; `--off38` / `-skygrindOff38` emits none of them; GG's sounds hook here):**
  - the spire gate meter (design.n38.spire38; `Game/SurvivalSim.N38.Spire.cs`): `spire38Meter` (a gate pad set the dash bar / the bar
    emptied in a gate room).
  - THE LOOM's entrance (builder O2's loom38 on n38-span; `Game/SurvivalSim.Loom37.Boss.cs`): `boss38Entrance` (a boss's fight started:
    the entrance sound; payload the boss's display name).
  - the HUD (design.n38.hud38, a view row; name in `Game/UnityTuning.Hud38.cs`): `hud38Ready` (a gun's charge bar filled: the ready ping).
  - the juice (design.n38.juice38, a view row; names in `Game/UnityTuning.Juice38.cs`): `boss38Entrance` (also raised by every boss's
    `bossRise`), `boss38Move` (a big boss move: the worm's dig / erupt, THE STAG's crash, THE LOOM's fall, THE CROWN's shatter, THE WICK's
    dive), `hurt38Static` (a hit's full-screen static).
  - ride38, doors38, stag38, crown38 and the six level rows add no event.

**NIGHT37 (the boss night: THE WICK, THE STAG, THE LOOM and THE CROWN restarted from scratch as moving fights on the shared Boss37 brain; names read by the lead from the `Emit` calls and event-name strings on port/stage3 during round 3, port-only; `--off37` / `-skygrindOff37` emits none of them; GG's sounds hook here):**
  - the shared brain (design.n37.boss37; `Core/Boss37.cs`; payload Boss37Event {boss, attack, stage, kind, leg, seconds, stagger, budget, at}): `boss37Tell` (a wind-up began: the wind-up sound), `boss37Attack` (the attack fires), `boss37Rest` (a forced rest: stage burst / budget / window / leg / restart), `boss37Stagger` (the stagger meter drained: drain / parry), `boss37Window` (the CYAN window opened / closed: the "now!" cue), `boss37Leg` (a leg starts / changes / restarts), `boss37Hide` (the anti-hide fired: the boss comes to you).
  - THE WICK (wick37; level 1's climb): `wick37Ember` (the yellow ember volley), `wick37Flare` (the red flare sweep), `wick37Slam` (the plate slam), `wick37Draft` (the draft, the slag rises), `wick37Crack` (a parried ember cracked a plate), `wick37Dive` (the cap leg's dive), `wick37Launch` (the launch punch into the slag), `wick37Slag` (the slag touched you), `wick37HotWall` (a burn from the hot shaft wall), `wick37Checkpoint` (a ledge reached).
  - THE STAG (stag37; level 2's chase): `stag37Bolt` (it bolts), `stag37Shards` / `stag37Chunk` / `stag37Cut` / `stag37Kick` / `stag37Charge` (its attacks), `stag37LegPunch` (a hind-leg punch landed), `stag37Skid` (it falls and skids: the window), `stag37Launch` (launched into the canal wall), `stag37Floe` (a floe dropped), `stag37Crash` (the parried charge's crash: the kill), `stag37Fall` (you fell: the leg restarts), `stag37Pacer` (the rehearsal pacer).
  - THE LOOM (loom37; level 4's crossing): `loom37Lash`, `loom37Spool`, `loom37Web` (its attacks), `loom37Tear` (a knee torn), `loom37Open` (the spinneret opens: the window), `loom37Cut` / `loom37Sever` (threads cut behind you / a tower's anchors severed), `loom37Snap` (your rope snapped), `loom37Fall` / `loom37Ledge` (a fall and the return to the ledge), `loom37RideRefill` (a wall ride gave a grapple charge back), `loom37Down` (it falls into the canyon), `loom37Leg`.
  - THE CROWN (crown37; level 5's descent): `crown37Shatter` (the summit shatters), `crown37Pass` (a jump-dash through the ring), `crown37Gem` (a gem burned off), `crown37Shard` (the gem shard), `crown37Cut` (the beam cut / a piece drops), `crown37Piece` (a piece's state), `crown37Fall`, `crown37Leg`.
  - Some strings above double as layer or room names (`stag37Canal`, `crown37Spire`, `wick37Stack` are layers, not events).

**NIGHT36 (the overhaul's first slice, movement is the weapon: the tap on the press, the crunch, the fist, yellow / red tells, the parry, the launch, close kills heal, walls that grip, level 1 from scratch with ULTRAKILL's arena flow, THE LANTERN on Boss36 with the walk-in; names read by the lead from the `Emit` calls and event-name strings on port/stage3 at the freeze f53ad261, port-only; `--off36` / `-skygrindOff36` emits none of the new ones):**
  - tells (design.n36.tells36; `Core/Tell36.cs`): `tell36` (an attack's wind-up began; payload Tell36: source, attack, kind YELLOW parryable / RED dodge, wind-up steps: the cue for a wind-up sound), `tell36End` (the wind-up ended: attack, parry or death).
  - the parry (parry36; `Core/Combat.cs`, `Game/SurvivalSim.Core36.cs`, THE LANTERN's orbs in `SurvivalSim.Boss36.cs`): `parry36` (a punch met a yellow attack: the freeze, the flash; GG's parry sound), `parry36Burst` (the returned projectile exploded at its shooter).
  - the launch (launch36): `launch36` (a moving punch launched an enemy), `launch36Burst` (the launched enemy hit another enemy or a wall and burst).
  - heal by aggression (heal36): `heal36` (a close kill healed; payload: the amount).
  - the arena flow on n36 levels (`Game/SurvivalSim.Level36.cs`): `arena36Lock` (the room's doors locked red), `arena36SpawnIn` (one enemy switched on, with its bubble), `arena36LastKill` (the room's last kill: the short slow-down), `arena36Unlock` (the doors opened).
  - the boss shape (boss36 / walkin36; `Core/Boss36.cs`, `Core/Enemies/Lantern36.cs`): `boss36Lock` (the hall's slab closed), `boss36Reveal` (the boss rises in view), `boss36Tell`, `boss36Attack`, `boss36Punish` (the punish window opened), `boss36Stun` (the 50 % stun), `boss36Phase`, `boss36Camp` (the anti-camp slam); THE LANTERN's own: `lantern36Floor` (the floor turns to lava), `lantern36Sweep` (the panel sweep), `lantern36Shield` (a crack or a break).
  - tap36, crunch36, muzzle36, fist36, wallgrip36, text36, bossmodel36 and level36 add no new names (they change when and how the existing `fire`, hit, punch and WallRide events read).

**NIGHT35 (the fundamentals night: the charge comes back, pickups that show what they are, teaching text, the Lucio wall ride, the grapple that yanks from level 4, the PS2 look, the TV HUD, six levels; names read by the lead from the `Emit("...")` calls on port/stage3 at the freeze 2be19aef, port-only; `--off35` / `-skygrindOff35` emits none of the new ones):**
  - the charge (design.n35.charge35; `Core/Guns/Charge35.cs`, `ChargeShots27.cs`): `charge35Start` (a hold on the KESTREL or THE THUNDERCLAP began charging, no shot fired), `charge35Full` (the charge is full: the cue for GG's "ready" sound), `charge35Fire` (the charged shot left, on the release or by itself 1.0 s after full). The old `chargeShot27*` events still fire for the same charge.
  - teaching (design.n35.teach35; `Game/SurvivalSim.Teach35.cs`): `teach35` (a teaching line was shown; payload: its id, e.g. `l1.charge`).
  - Sound hooks named for GG, not emitted tonight: `wallride35Chain` (a rising pitch on chained wall rides, with the chain count: NIGHT35.md leftover 6).
  - pickup35, look35, lights35, tv35, ridelook35, wallride35, grapple35 and campaign6 add no new names (the ride and the grapple use the Mover's existing WallRideStart / WallRideEnd / RopeAttach events).

**NIGHT34 (the flow night: the grapple BEND, the arsenal rail, no grapple diamond, movement-line arenas, the calm pass, flow34, the gauntlet's boss reach, the air multiplier, the bend combo; names read by the lead from the `Emit("...")` calls on port/stage3 at the freeze, port-only; `--off34` / `-skygrindOff34` emits none of the new ones):**
  - the bend (design.n34.bend34; `Game/SurvivalSim.Bend34.cs`, `Core/Bend34.cs`): `bend34Start` (a latch began a bend, or a SHORT bend turned FULL; payload: tier "short" | "full", the meter, kills 0), `bend34Kill` (a player kill inside a bend; payload: tier, kills so far, whether it extended the bend), `bend34End` (the hold ended and the snap back begins; payload: tier, kills, the meter).
  - the bend combo (design.n34.combo34; `Game/SurvivalSim.Combo34.cs`): `bend34Combo` (kills with 2, 3 or 4+ different guns inside one bend; payload: name BEND BREAKER / TRIPLE BEND / FULL ARSENAL, guns, kills, points, the gun list). The HUD shows the name in hud33's message slot.
  - switch rewards (design.n34.combo34 arsenal / quickdraw; `Game/SurvivalSim.Combo34.cs`): `arsenal34` (a kill on an enemy two or more guns hit; payload with `name` "ARSENAL"), `quickdraw34` (a hit within 0.25 s of a swap; `name` "QUICKDRAW"). The whip readying the gauntlet (whipReady) and the slide bounce (flow34.slideBounce34) add no names.
  - The rail (rail34), the look (bendLook), nomark34, lines34, calm34, flow34, reach34 and air34 add no new names (reach34's hits are the existing gauntlet hit events; air34 multiplies existing awards).

**NIGHT33 (the polish night: HUD, light, dressed arenas, devices, lava-safe enemies, trigger spawns, the boss cut, the whip, the piercer, the shop; names read by the lead from the `Emit("...")` calls on port/stage3 at the freeze, port-only; `--off33` / `-skygrindOff33` emits none of the new ones):**
  - the whip (design.n33.whip33; `Game/SurvivalSim.Whip33.cs`, `Core/Arcline21.cs`): `whip33Latch` (a tap latched an enemy or a boss body: the pull starts), `whip33Arrive` (the pull stopped 1.5 m short: the hop, the hit through Combat.Damage as weapon `whip33`, 3 steps of hitstop; payload says the gap, the damage and whether it killed). The NIGHT31 grapple names (`grapplePull`, `grappleArrive`, `grappleCancel`) still fire for rings, joints and surfaces.
  - the piercer (design.n33.pierce33; `Core/Guns/Pierce33.cs`): `kestrelPierce33` (a RailFireEvent: owner, from = the eye, to = the wall the tracer ends on; the charged KESTREL shot went through every body in its line; the view draws the LONGWAVE beam from it). NIGHT27's `chargeShot27Fire` still fires first.
  - devices (design.n33.devices33; `Core/Devices33.cs`): `fanLift` (an updraft fan lifted the player), `ventTell` (a hazard vent's 0.8 s warning), `ventBurst` (the vent fires its column), `ventBurn` (the burst hit the player: the damage in the payload). The moving platform emits nothing.
  - trigger spawns (design.n33.spawns33; `Core/Campaign24.cs`): `enemySpawnTell33` (the 0.6 s red-orange column where an enemy will appear; the enemy's own spawn follows).
  - the boss anti-camp (design.n33.bosses33; `Game/SurvivalSim.Bosses33.cs`): `bossStillArmor33` (the player stood still near a boss: the boss hardens and takes half damage; fires when the armor goes on and when it goes off).
  - The shop (shop33), the HUD (hud33), the light (look33), the kit (kit33) and the lava-safe enemies (ai33) add no new names.

**NIGHT32 (bosses in a minute, the grapple's charges, THE CROWN, the round ring, level 4 rebuilt, LEVEL 5; names read by the lead from the `Emit("...")` calls on port/stage3, port-only; `--off32` / `-skygrindOff32` emits none of the new ones):**
  - grapple charges (design.n32.grapple32; `Game/SurvivalSim.Grapple32.cs`): `grappleCharges` (payload int: the charges now; whenever the count changes: a pull spends one, a recharge or a non-boss kill gives one back), `grappleEmpty` (a press was cleared: no charge left, or inside the 0.8 s gap between pulls).
  - THE CROWN, level 5's boss (design.n32.boss5; `Core/Enemies/Crown32.cs`, `Game/SurvivalSim.Crown32.cs`): `crownBeat`, `crownSweep`, `crownOrb`, `crownMortar`, `crownDrop`, `crownCharge`, `crownSlam`, `crownPlate` (each with a stage string: tell / fire / land / open / shut ...); a parried orb is the existing `parry` event with kind `crownOrb`; the shared `bossTell`, `bossHit`, `bossPhase`, `bossRise` as the other bosses.
  - The four retuned bosses (design.n32.bosses32) and the levels (arenas32, span32, level5) add no new names: their existing events fire with the new numbers and rooms.

**NIGHT31 (the pull-in grapple, the charge shot back, the LT on every weapon, the boss parry, THE STAG and the rebuilt bosses; names read by the lead from the `Emit("...")` calls on port/stage3 at the freeze, port-only; `--off31` / `-skygrindOff31` emits none of the new ones):**
  - grapple (design.n31.grapplePull; `Game/SurvivalSim.Grapple31.cs`, `Core/GrappleTap31.cs`): `grapplePull` (a tap turned the hook into a pull-in), `grappleArrive` (the pull let go at its anchor; speed capped by arriveKeepMax, plus the pop), `grappleCancel` (a jump, dash or second press ended the pull, speed kept), `grappleHang` (a pull to a Loom joint turned into the short-rope hang).
  - charge (design.n31.charge; `Core/Guns/ChargeShots27.cs`): NIGHT27's `chargeShot27Start`, `chargeShot27Fire`, `chargeShot27Cancel` (now also fired by the auto-release and by a melee press), and `chargeCombo31` (a charged KESTREL shot set off stuck nails).
  - the boss parry (design.n31.ltAlways; `Game/SurvivalSim.Weapons31.cs`): the existing `parry` event; the returned projectile hits its boss as weapon `parry31`.
  - bosses (`Core/Enemies/Stag31.cs`, `Wyrm31.cs`, `Lantern31.cs`, `Loom31.cs`, `Game/SurvivalSim.Boss31.cs`): `stag31`, `wyrm31`, `lantern31`, `loom31` (each with a stage string in its payload: the boss's tells, slams, crumbles, sinks, breaks), plus the shared `bossTell`, `bossHit`, `bossPhase`, `bossRise` on THE STAG.

**NIGHT30 (lava, glide, the weapon rules, the teaching lines, the ring LANTERN, the GLACIER rebuilt, the WYRM stagger, LEVEL 4 and THE LOOM, the weapon-state HUD; names read from the emits on port/stage3 by builder S, port-only; `--off30` / `-skygrindOff30` emits none of the new ones):**
  - `lavaBurn` (`Core/Lava30.cs`; payload `Lava30Event {id -1, kind "player", damage, flashSeconds, x, y, z}`; `design.n30.lava`): the player's feet touched a lava surface (a level's `lava` row): 25 % of max health, the upward knock, the 0.6 s hurt flash; at most once per 0.5 s grace. The damage is also a `hurt` with `source` `lava`.
  - `lavaEnemyBurn` (`Core/Lava30.cs`; `Lava30Event {id, kind, x, y, z}`; `design.n30.lava`): an enemy's feet touched lava: it starts to burn (0.8 s).
  - `lavaEnemyDeath` (`Core/Lava30.cs`; `Lava30Event {id, kind, x, y, z}`; `design.n30.lava`): the burning enemy died (no player kill credit, no `kill` event from this path).
  - `bossLine30` (`Core/Enemies/Lantern30.cs` and `Game/SurvivalSim.Boss30.cs`; payload `BossLine30 {boss, key, text, seconds, show}`; `design.n30.teach` plus the boss's row): a boss's teaching line is shown (the lesson line): boss `lantern` keys `tell` ("GET ABOVE THE LAVA") and `core`; `glacier` key `volley`; `wyrm` key `punch`; `loom` key `start`. At most 2 shows per line per boss room (`design.n30.teach.maxShows`; a retry of the same room does not reset it).
  - `lessonBegin` / `lessonClear` (NIGHT25's names and `LessonEvent`; new uses with `design.n30.teach`): a boss line also goes through them (id `boss`, kind `boss`, the line's text as prompt); a room's lesson line now gets a `lessonClear` when its room is cleared or left (it used to stay up into the next room).
  - `lanternSweep` (`Core/Enemies/Lantern30.cs`; payload `LanternSweep30 {stage, panel, seconds}`; `design.n30.lantern30`): the ring's panel sweep: stage `tell` (the panel two ahead of the rider is marked, 1.2 s), `burn` (it burns, 1.5 s), `end`.
  - `lanternLob` (`Core/Enemies/Lantern30.cs`; payload `LanternLob30 {stage, target, seconds}`; `design.n30.lantern30`): the arcing orb at a platform camper: stage `tell` (its landing spot is marked), `fire` (thrown).
  - `wyrmStagger` (`Game/SurvivalSim.Boss30.cs`; payload `Wyrm30Event {position, bonus, recoilSeconds}`; `design.n30.wyrm30`): a PULSE gauntlet hit on the KILN WYRM's head inside the charge's punch window: the head recoils, bonus damage; the 6-step hitstop carries the reason `wyrmStagger`.
  - `glacierVolley` (`Core/Enemies/Glacier30.cs`; payload `Glacier30Event {stage, seconds}`; `design.n30.glacier30`): the homing shard volley at a player too far or out of sight: stage `tell` (0.8 s), `fire`.
  - `glacierSweep` (`Core/Enemies/Glacier30.cs`; `Glacier30Event {stage, seconds}`; `design.n30.glacier30`): the lane-wide frost sweep: stage `tell`, `live`, `end`.
  - `loomCut` (`Core/Enemies/Loom30.cs`; payload `LoomCut30 {stage, anchor, seconds}`; `design.n30.span`): THE LOOM plucks the line the player hangs on: stage `tell` (1.0 s red tell on the line), `cut` (the player drops). The Loom also emits the kit's `bossTell` (name `cut` / `sweep`), `bossRise`, `bossPhase`, `bossHit`.
  - `loomSweep` (`Core/Enemies/Loom30.cs`; `LoomCut30 {stage, anchor, seconds}`; `design.n30.span`): a leg sweeps the deck: stage `tell`, `live`, `end` (anchor = the leg's grip).
  - `arclineOwned` (NIGHT21's name and payload, a bool): now also fired at the first step of a CAMPAIGN level with grapple nodes when `design.n30.span` is on (the ARCLINE is the level's verb, owned from its start); the rope's own `ropeFire` / `ropeAttach` / `ropeRelease` / `slingshot` / `arclinePull` keep NIGHT21's names.
  - `weaponReady` (view only, not on the sim bus: `Game/HudWeapons30.cs` raises the static `Hud30.WeaponReady(id)` and a FeelProbe event; `design.n30.hud30`): a weapon-state slot turned READY: id = the weapon, or `punch` (the gauntlet's heavy swing) / `blast` (its LT blast). The viewmodel's older `weaponReady` on the bus (the web's audio hook) is unchanged.
  - No new event: the weapon rules (`design.n30.rules`: a switch waits for the action, a queued switch, a dropped press are silent), glide (`design.n30.glide`: no boost event; `Glide30.Gliding` is a state for the skate trail), the pulse27 events (unchanged names and payloads).

**NIGHT29 (levels 1-3 rebuilt, the reworked bosses, the lockout, DOORFREE; names read from the emits in the source, port-only; `--off29` emits none of the new ones):**
  - `pickup29` (`Core/Campaign24.cs`; payload `PickupEvent29 {id, kind, weapon}`): a plan's weaponPickup / healthPickup prop was collected (the THUNDERCLAP pad in the kiln's first room, the rest rooms' health); the weapon also fires the normal `weaponEarned` once.
  - `lantern29Stage` (`Core/Enemies/Lantern29.cs`; payload `Lantern29Event {stage}`): THE LANTERN's floor cycle changed stage (tell, live, rest); the sound-free cue is the arena glow, the sounds are GG's.
  - `bossTell` (`Core/Enemies/Glacier29.cs`, the web kit's name): each of the Glacier's attacks (shard volley, frost trail, lane charge) tells at least 0.8 s before it hurts.
  - DOORFREE has no event (a state: `sim.DoorFreeOn`, `sim.Unranked29`; the HUD tag reads it). The lockout drops the second input silently (no event).

**NIGHT27 (tight levels, THE LANTERN, the pulse-only GLACIER, the 5-kill PULSE, charge shots; names read from the emits in the source, port-only; `--off27` emits none of the new ones):**
  - The PULSE (`Core/PulseCharge27.cs`, `design.n27.pulse27`): `pulse27Pip` ({pips, kills, why}: a kill filled a pip), `pulse27Blast` ({hits, boostSteps, pips}: the LT blast + the 0.4 s boost), `pulse27Denied` ({why: pips | cooldown}: LT pressed with no full glove or inside the 15 s cooldown). The 2.5 s heavy swing keeps NIGHT24's melee names.
  - Charge shots (`design.n27.chargeShots27`): `chargeShot27Start` (a hold on RT past the tap threshold begins to charge), `chargeShot27Fire` (the charged shot left on release), `chargeShot27Cancel` (released early: nothing fired, no cooldown). A tap is the normal `fire`.
  - The floor hazard (`Game/SurvivalSim.Level27.cs`, `design.n27.hazardFloor27`): `hazardTelegraph27` (1.5 s warning), `hazardLive27` (the strip hurts 12 HP/s for 8 s), `hazardOff27`, `hazardHurt27` (a tick of damage to the player).
  - THE LANTERN (`Core/Enemies/Lantern27.cs`) and THE GLACIER chase (`Core/Enemies/Glacier27.cs`) use the boss kit's names (`bossIntro`, `bossTell`, `bossPhase`, `bossHit`, `bossPunish`, `bossRise`, `bossDown`, `bossPart`) plus `bossShield` (a hit was BLOCKED by the LANTERN's shield: the face was not lit by a wall ride) and `bossWindow` (the GLACIER's PULSE-ready window opened). Payloads are the kit's; read the emits in the two files before wiring sounds.

**NIGHT26 (the first three levels; names read from the emits in the source, port-only; `--off26` emits none of the new ones):**
  - `weaponEarned` ({weapon, name, room}; `Core/Campaign24.cs`: a room's `award`, and at a level's end the weapon `Design/levels/unlocks.json` gives it): the sound for unlocking a gun. The level select itself emits nothing (it is a view over the profile).
  - THE SLAG HULK (`Core/Enemies/Hulk26.cs`) and THE GLACIER (`Core/Enemies/Glacier26.cs`) use the boss kit's names, each with its own payload: `bossPhase`, `bossTell` (the slam ring, the charge, a glacier lane attack), `bossRise`, `bossHit`; the Hulk adds `bossStun` (it ran into a wall: hittable 2 s); the Glacier adds `bossPunish` (it punished a missed dodge). The gap meter is a view, no event. grep of `events.Emit("boss...` in the two files; other names inside them were not checked.
  - Weapons (`Core/Guns/Weapon26.cs`, `design.n26.weaponIdentity` / `pulseLT`): no new event name. The PULSE's blast on LT is NIGHT24's `fistCharge`, `fistFlick`, `fistFlickBlocked`; a blast of yours that hurts you is a `hurt` with `source` `selfBlast` (rocket at point blank); the cooked BOUNCER grenade is the existing `grenadeBlast` earlier; the STITCHER's detonate on LT is the existing `nailDetonate`.

**NIGHT25 (the teaching campaign; every one is a moment for GG's sounds; names read from the emits in the source, port-only unless a web name is said):**
  - Lessons (`Core/Campaign24.cs` EmitLesson, `Core/Lesson25.cs`; payload `LessonEvent`: room, id, index, count, kind, prompt, promptPad, time): `lessonBegin` (a tutorial stage's lesson is current: the prompt shows) and `lessonClear` (its verb was done: the prompt leaves); `stageRespawn` (RoomEvent: a stage's pit or respawn line put you back at its start, no death; there is no fail state in the tutorial). The Campaign24 events of NIGHT24 (`roomEnter`, `roomClear`, `roomRetry`, `checkpoint`, `layerClear`, `weaponEarned`, `gateLock`, `limitOn` / `limitOff`) are unchanged.
  - Boss rooms (`Game/SurvivalSim.Boss25.cs`; no new event name: a boss room starts its fight through the kit, so the sounds are the boss events): the web's kit names `bossIntro` ({name: the card; the ELDER's reads ELDER KILN WYRM}), `bossBeat`, `bossDying`, `bossDown`, and each boss's own `bossRise`, `bossPhase`, `bossTell` ({label / name, seconds >= 0.5}), `bossHit` ({damage, part}), `bossPart`. RIVAL PRIME (`Core/Enemies/RivalPrime.cs`, port-only) emits `bossRise`, `bossPhase`, `bossTell` ({label: dash | volley | burst, seconds}), `bossHit` ({part: head | body}), and the kit's `bossBeat`, `bossDying`, `bossDown`. The ELDER KILN WYRM is the worm's code: its events are the worm's, plus `bossPhase` p4 and a `bossBeat` label "PHASE 4". A player death in a boss room emits `roomRetry` and restores the boss to the start of its phase (no `bossIntro` again).
  - The PULSE (`design.n25.pulseCharge`): the charge and the launch are NIGHT24's `fistCharge`, `fistFlick`, `fistFlickBlocked` (the charge now rises with the PULSE's own hits; no new name); the quick melee with the gun is the existing `meleeSwing` and `fire` in one step (`design.n25.meleeFire`, pad layout v5: PUNCH on RB). The shotgun's shockwave events are gone with `design.n25.shockwaveOff` (its LT is the zoom: no event).
  - `weaponSwitch` (NIGHT24, port-only: the pad's NEXT WEAPON press): its HUD cue is `Game/UiWeaponSwitch25.cs` (a view of the event; no new emit).
  - NIGHT47 (new names for GG's sounds, each emitted only while its design.n47 row is on; the old events still fire): THE WICK `wick47Phase`,
    `wick47Geyser`, `wick47Curtain`, `wick47Ring`, `wick47Charge`, `wick47CrackOpen`, `wick47CorePunch`, `wick47Updraft`, `wickTop47Seal`;
    the CHARGER `chargerLock47`, `chargerSkid47`, `chargerBurst47`, `chargerEnrage47`; THE ICEBREAKER `brawl47Phase3`, `brawl47Gates`,
    `brawl47ChargerHit`, `brawl47DoubleRush`, `brawl47PlateShed`, `brawl47Armor`; THE KILN WYRM `wyrm47Crack`, `wyrm47Trail`,
    `wyrm47BreachTell`, `wyrm47Breach`, `wyrm47CoreFromAbove`, `wyrm47Glimpse`, `wyrm47Pass`, `wyrm47Cameo`, `wyrm47WindowSpent`,
    `wyrm47TrailBurn`; THE LOOM `loom47Dive`, `loom47Cage`, `loom47Bobbin`, `loom47Seam`, `loom47Cut`, `loom47Swing`, `loom47Net`,
    `loom47Finale`; THE CROWN `crown47Fire`, `crown47Toll`, `crown47Spiral`, `crown47Crack`, `crown47Catch`, `crown47Volley`, `crown47Finale`;
    the player `pulse47Blast`, `overheal47Gain`, `overheal47Spent`, `hud47LastOne`, `hud47Clear`; SURVIVAL `stampede47` (row off). Names as the
    builders reported them; that each one fires in the Player is not checked.
  - EXT53 (builder N, design.n53.rideKill53, emitted only while the row is on): `rideKill53`, a player's kill on a wall ride or within its
    0.4 s grace paid a boost back (payload Core/RideKill53.cs RideKill53Event: onWall, stepsSinceRide, before / after, count, weapon); the HUD's
    RIDE KILL callout is its view (UiRideKill53.cs). Fired headless (n53ridekill ride); in the Player only by the a253 reel (the lead's run).

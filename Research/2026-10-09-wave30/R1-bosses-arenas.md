# R1: Boss arenas, parry and stagger windows, lava rooms

Web-only research, 2026-10-09, about 15 minutes. Every claim has its URL. "Fetched" means I read the page through
WebFetch's summary. "Search snippet" means the claim came from a search result, and the page itself was not opened or
did not load. Quotes are under 15 words. Pages that failed (403/404/402): doomwiki.org Marauder and Doom hunter,
doom.fandom Marauder, the ULTRAKILL Hitstop page, the PC Gamer Hugo Martin piece (body truncated). Their numbers are
marked "not checked".

## 1. Arenas, and how bosses punish fleeing or camping

### ULTRAKILL

- **V2 (1-4) uses a patience gauge, not a low-HP enrage.** "Keeping a certain minimum distance from V2 for a
  prolonged period depletes their patience." A gauge under the boss bar shows it filling. When enraged, V2
  "has a faster base movement speed than the player and aggressively advances". Fetched:
  https://ultrakill.wiki.gg/wiki/V2
- **V2 shows its movement mode by wing colour.** Gold: runs one way until it hits a wall. Blue: circle-strafes while
  closing in. Red: rushes (its only mode when enraged). Green: retreats to its preferred range, and attacks slower
  while doing so. https://ultrakill.wiki.gg/wiki/V2
- **V2's attacks by range.** Revolver: 3 shots x 10 damage, parryable. Piercer charged shot: 30, parryable. Shotgun
  up close: 12 pellets, 25. Core-eject grenade: 35, unparryable but you can shoot it. 1-4 has water on the floor. The
  4-4 rematch is in a throne room with four rising pillars; in phase 2 V2 breaks a wall and the fight slides down a
  pyramid. https://ultrakill.wiki.gg/wiki/V2
- **Minos Prime announces every attack with a voice line.** "Judgement!" is the dropkick (50 + an explosion).
  "Thy end is now!" is the boxing combo (4 x 30). "Prepare thyself!" is the kick combo plus a snake. "Die!" is the
  rider kick (30 + a 25 shockwave). "Crush!" is the slam (50 + a 25 shockwave). He closes distance by moving
  near-instantly to the player. At 65 HP or less he says "WEAK.", attack intervals shorten, and his stamina goes from
  6 to 12. Fetched: https://ultrakill.wiki.gg/wiki/Minos_Prime
- **Gabriel teleports above a far player.** He "will track the player for a brief period before lunging." His thrown
  spears, axes and swords (30 to 35) have "minor tracking". The weapon he summons tells you the attack: spear means a
  lunge, rapier a close combo, greatsword a dash then a spinning throw, twin axes tracking throws. At half HP he is
  "launched into the air and stunned", and hits bounce him higher, a free damage window before phase 2. He "will
  quickly cut down overly aggressive players" outside his taunt windows. Fetched:
  https://ultrakill.wiki.gg/wiki/Gabriel,_Judge_of_Hell
- **Cerberus has an attack for each range.** An orb (20 + an explosion) that it does "not use ... if the player is
  too close". Up close, a stomp shockwave (25, unparryable, you jump it) and a dash (25 + knockback). Before dashing
  it "can instantly change direction" to predict where you will be. It cannot dash off ledges unless enraged. Killing
  one Cerberus enrages the other (faster attacks and movement). Fetched: https://ultrakill.wiki.gg/wiki/Cerberus
- **Hideous Mass punishes range and closeness differently.** At range, Hell Mortars: 2 homing shots, 60 damage close
  and 40 far. Up close, a clap (20, or 40 between the arms) and a harpoon (25) that "severely handicaps the player's
  movement". Its belly and tail take **300 %** damage; the belly counts as a limb, so a revolver does 4.5x. Below
  roughly 20 to 33 % HP it flips onto its back, shows both weak points, and fires only mortars. Standing on its back
  disables the mortars. Fetched: https://ultrakill.wiki.gg/wiki/Hideous_Mass
- **Flesh Prison is a fixed turret in the middle of the room.** It "stays put in one place, rotating
  counter-clockwise". It fires 50 homing Hell Seekers (75 after half HP), which you dodge by ducking under it. Beams
  are marked by "an insignia that follows the player" until just before they fire. A slow black hole punishes
  standing still (10 damage + 99 Hard Damage). Spawns you leave alive heal it 10 each. The wiki's advice:
  "sliding around the arena". Fetched: https://ultrakill.wiki.gg/wiki/Flesh_Prison

### DOOM Eternal

- **Marauder: each distance has its own threat.** Long range, he throws his axe; mid range, a spirit wolf; close, the
  axe and the super shotgun. You can hit him only when his eyes flash green. Fetched:
  https://www.gamerevolution.com/?p=639744
- **Marauder green flash:** "Attack too soon" and the shield deflects it; wait too long and the window is gone.
  Search snippet: https://slayersclub.bethesda.net/en-AU/news/doom-eternal-boss-tips . The window's length in ms or
  frames: not checked (no source I reached gave a number).
- **Doom Hunter.** A shield that regenerates while its hover sled is intact; plasma strips the shield; Blood Punch
  hurts the sled. When the sled breaks ("CRITICAL DAMAGE") it loses the shield, the plasma guns and the missiles, and
  becomes "faster and slightly more aggressive". Search snippet from https://doomwiki.org/wiki/Doom_hunter (the page
  itself returned 403).
- **Id's arena rule.** Enemy types are "chess pieces" placed to push the player off ledges and keep them moving (push-
  forward combat). Search snippets: https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
  and https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal

### What makes a chase boss fun or frustrating (general sources)

- **critpoints, "FPS Boss Fights Don't Have to Suck".** The failures: a single bullet-sponge target, puzzle gimmicks,
  and circular arenas you can circle-strafe forever without a real threat. The fixes: adds that are a real threat
  ("If the adds pose no real threat ... may as well not exist"), "several overlapping attacks that vary where and
  when" you should jump, push and pull (the boss wants the opposite range to the player), weak points that reward
  explosives, and sound cues for attacks from off-screen. DOOM 2016's bosses "spam the entire arena with telegraphed
  airstrikes" and use height changes. Fetched: https://critpoints.net/2021/01/27/fps-boss-fights-dont-have-to-suck
- **Mike Birkhead (combat designer).** Wind-ups should last **1 to 1.5 s, never more than 2 s** (beyond that the
  player counts instead of reacting). A vulnerability window must fit one full player combo **plus** the time to get
  to the boss. Each attack needs its own wind-up animation and sound. "Show what matters, and only what matters": a
  prop that looks usable but isn't is a defect. Fetched:
  https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer
- **Dusk, Prodeus, Cruelty Squad: not checked.** My searches returned only general reviews, with no boss or arena
  detail.

**Pattern.** None of these bosses enrage on a timer. Fleeing is punished by (a) a visible meter that fills while you
stay far away (V2), (b) an attack only used at range: homing, tracking or lobbed (Hell Mortar, Gabriel's throws,
Cerberus' orb, Marauder's axe), (c) a teleport or gap closer (Minos, Gabriel), or (d) area denial that moves (Flesh
Prison's beam insignia and black hole). Camping close is punished by a shockwave you jump (Cerberus stomp, Minos
slam, Hideous Mass slam) or a grab or slow (the harpoon).

## 2. Weak points, parry and stagger

- **ULTRAKILL parry.** You punch a projectile or a melee attack at the right moment. Parrying an enemy deals 5 base
  damage; a parried projectile flies back and explodes in a moderately large area, usually cancelling the attack.
  Search snippet: https://ultrakill.wiki.gg/wiki/Whiplash (results page).
- **Parry feedback.** "briefly freeze and flash the screen white". It then heals you fully up to the Hard Damage cap
  and refills all stamina. Yellow flashes mark attacks you can parry; blue flashes mark attacks you cannot. A
  Swordsmachine takes 150 % damage from a shotgun parry. Fetched: https://ultrakill.wiki.gg/wiki/Parry . The page
  gives no frame numbers for the window.
- **Hitstop length.** Major hitstops (parries, interruptions, punching a cannonball) freeze time and music for
  **0.25 s**. Search snippet from https://breezewiki.discard.no/ultrakill/wiki/Hitstop ; the page itself returned
  access denied. The minor-hitstop tiers: not checked.
- **Mercy frames.** Projectiles have **3 mercy frames** between hitting you and the damage applying, so a late punch
  still parries. One punch can parry two projectiles. Search snippet, from the same search (I could not confirm which
  page); not checked on a page.
- **Parry resets the boss.** Parrying Minos' melee "stuns him temporarily". Parrying his snake heals you but gives no
  stamina. On higher difficulties in phase 2 his melee becomes unparryable. https://ultrakill.wiki.gg/wiki/Minos_Prime .
  Gabriel's Feedbacker parries "consistently provide" a full heal.
  https://ultrakill.wiki.gg/wiki/Gabriel,_Judge_of_Hell
- **Marauder.** A Super Shotgun or Ballista hit during the green flash knocks him off balance and cancels his attack.
  Search snippet: https://slayersclub.bethesda.net/en-AU/news/doom-eternal-boss-tips . Then "only afforded a few
  seconds" before the shield comes back; search snippet: https://twinfinite.net/guides/doom-eternal-beat-marauder/ .
  Players report the shield's eyes blink before the green-eye attack; search snippet from Steam discussions:
  https://steamcommunity.com/app/782330/discussions/0/5312592307008404550
- **Weak-point multipliers.** Hideous Mass belly and tail at 300 % (4.5x for a revolver).
  https://ultrakill.wiki.gg/wiki/Hideous_Mass
- **Stagger as a phase gate.** Gabriel at 50 % (stunned in the air, juggled); Doom Hunter's sled breaking (the shield
  is gone for good and it gets faster); Hideous Mass flipping over (weak points shown, mortars only). Sources above.

**Pattern.** Tell, window, punish, reset. The tell is colour plus sound (yellow or blue flash, green eyes, a voice
line). The success effect is a quarter-second freeze, a white flash, a resource refill and the attack cancelled.
Then the boss resumes on a new pattern, and the window shuts quickly ("a few seconds") so you cannot farm it.

## 3. Lava and floor-is-lava rooms

- **ULTRAKILL separates "fall" from "burn".** A Death Zone (pit, grinder, fan) deals a set amount and "teleport[s]
  the player to the nearest safe ground", leaving you at 1 HP if it would have killed you. Lava, described as
  "bubbling hot orange liquid", deals **50** to the player and 25 to enemies, does not teleport you, puts you
  underwater with lower gravity, and sets flammable enemies on fire. Acid looks like lava but yellow and steaming,
  and deals 10. Fetched: https://ultrakill.wiki.gg/wiki/Pit
- **Sea of Souls / hurt zone (5-2, 5-4).** 20 damage plus upward knockback; enemies that touch it die instantly
  (+ CRUSHED). You can slide across it. Search snippet: https://ultrakill.wiki.gg/wiki/Pit . (Layer 5 is the water
  layer; I found no lava in 5-x.)
- **ULTRAKILL lava arenas.** 1-E has a lava version of the Dual Tower Arena; 6-1 has bridges and archways over a lava
  moat (search snippet). 6-1's Lava Chasm is two platforms over a large pit of lava; the wiki gives no contact
  damage there. Fetched: https://ultrakill.wiki.gg/wiki/6-1
- **Making lava read as lava.** An emissive map multiplied by scrolling noise is "what turns a static image into
  something that reads as flowing". Without it, lava "reads as an orange rock". Pulse the emissive with a sine or
  noise. Add heat distortion: it "sells motion even where the texture itself is static". Fetched:
  https://www.sharetextures.com/blog/6-free-lava-magma-textures
- **Heat shimmer method.** A screen-space refraction: offset the frame's UVs with upward-scrolling noise, masked to
  near the ground. Search snippet: https://gamedev.net/shaderlab/38-heat-haze-distortion/ and
  https://www.moddb.com/games/blockadillo/news/devlog-about-lava-and-heat-haze-effect
- **Pits versus hazards (community level feedback).** A pit the same colour as a wall is unreadable; make pits darker
  and give fall edges a glowing red rim. Search snippets: https://www.doomworld.com/forum/post/2152707 and
  https://itch.io/post/3307728 (weak sources).
- **Mirror's Edge.** Runner Vision tints usable route objects (pipes, ramps, springboards) red, and "white things fade
  to red as you approach". It works on rooftops but fails indoors, where it points through walls; stopping to look
  "destroys the level's pacing". Search snippets:
  https://worldofleveldesign.com/categories/level_design_tutorials/one-mistake-in-mirrors-edge.php ,
  https://ea.com/news/runners-vision-in-mirrors-edge-catalyst
- **Neon White.** Carter Piccollo: "If you're ever confused on what to do, the game doesn't feel good". Search
  snippet: https://80.lv/articles/breakdown-level-design-in-annapurna-s-neon-white . Its lava or hazard specifics: not
  checked.
- **Titanfall 2.** Into the Abyss is moving platforms, crushing presses and flipping arms, not lava. Search snippet:
  https://store.epicgames.com/en-US/news/five-levels-that-show-why-titanfall-2-is-one-of-the-best-shooters-ever-made .
  Players report fire on the ground in wall-run sections; search snippet:
  https://steamcommunity.com/app/1237970/discussions/0/600767061860293580 . Lava combined with wall-running: not
  checked.

**Pattern.** Lava has to read as a *surface* (it glows, pulses, flows and shimmers), never as a *hole* (a dark void).
A fall has its own visual language: a dark drop and a rim. Lava that damages without killing and keeps your momentum
(ULTRAKILL: 50, no teleport) turns it into a cost-versus-route choice rather than a death.

## What SKYGRIND should take (10 bullets)

1. **A patience meter instead of a timer enrage.** Copy V2: a gauge under the boss bar fills while the player stays
   beyond X m, and drains when close. When it is full, the boss rushes faster than the player. The fleeing is
   punished visibly, so it never feels like a cheat. X, the fill rate and the drain go in a Unity-side config file
   (the web has no bosses).
2. **One threat per range band.** Far gets a homing or lobbed shot (like Hell Mortar or the Marauder's axe), mid a
   chaser add (the wolf), close a shockwave you jump over (Cerberus or Minos). Never let any one distance be safe.
3. **Wind-ups of 1 to 1.5 s, never over 2 s.** At 120 Hz that is 120 to 180 steps, cap 240 (`stepsFor`). Give each
   attack its own colour plus sound tell. Sound is GG's: ship named events only, no audio.
4. **Parry and stagger feedback.** A 0.25 s major hitstop (30 steps), a white or cyan screen flash, the attack
   cancelled, and a resource refill. Use the existing hitstop path, not a new one.
5. **Mercy frames for late parries.** About 3 frames (roughly 50 ms at 60 fps; ULTRAKILL's frame basis not checked)
   between contact and damage, so a slightly late press still parries. On pad this matters more.
6. **Weak-point windows that close.** A green-eye-style flash (but keep the palette: a red-orange tell on the enemy,
   with the hit window confirmed in cyan on the reticle). A hit in the window staggers the boss; after it, a "few
   seconds" of exposure, then a new pattern. Weak-point multipliers like 3x on an exposed part.
7. **Arena shape: a ring with height, not a flat circle.** Critpoints names the infinite circle-strafe as the failure.
   Use a ring route with ramps up (GG: "a route that enemies can climb up") and pillars or rising platforms (V2 4-4),
   plus arena-wide telegraphed area denial (Flesh Prison's insignia, DOOM's airstrikes) so circling has a cost.
8. **Stagger as the phase gate.** At set HP thresholds, the boss is stunned and juggled (Gabriel at 50 %), loses a
   part (Doom Hunter's sled), or flips to show weak points (Hideous Mass). Phase 2 is faster with fewer, harsher
   attacks. Make every threshold a staged event with a web-style event name.
9. **Lava reads as a surface, pits as a void.** Red-orange emissive (danger colour, fits the palette), scrolling noise,
   a sine pulse, a screen-space heat shimmer masked near the surface, rising embers, and a bright rim where it meets
   the floor. Pits get a dark drop and a pale violet edge only if they are the boundary. Never draw a dark lava pool.
10. **Lava damages; it doesn't end the run.** Use ULTRAKILL's split: falls teleport you to safe ground with fixed
    damage; lava deals a big fixed hit (their 50 of 100), keeps your momentum and burns enemies. That makes wall-runs
    over lava a risky shortcut rather than a death trap, and lets enemies be knocked in. Contact damage and burn
    rules go in config, not hand-typed.

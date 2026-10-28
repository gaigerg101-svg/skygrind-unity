# SKYGRIND design rules (GG 2026-10-16: "establish actual design rules and principles that future levels, arenas, enemies, bosses, mechanics, and UI must follow so that we stop forgetting")

These extend docs/PRINCIPLES.md (the eight principles) with checkable rules. Every night's plan, card and review is checked against them;
a rule broken on purpose names the rule and GG's word for it. Sources: GG's words (docs/memory/OWNER.md; Research/2026-10-16-wave43/
GG-REQUEST.md, rows G1-G30) and the research with URLs in Research/2026-10-16-wave43/research/ (movement-web, combat-web,
encounters-arenas-web, ui-art-web, bosses-web) and Research/2026-10-14-wave42/research/ (hitfeel-web, arenas-web). Numbers marked
"(tonight)" were set in NIGHT43 and are checked in its log. Written by the lead, NIGHT43 (2026-10-16).

How to use: before a card is built, list the rule ids it touches; at review, check each. A new rule goes here with its source.

## M. Movement (the foundation everything else stands on)

- **M-1 Fix the system, never design around it.** A movement complaint is first reproduced in a harness (a scripted approach, the share
  that succeeds) and fixed in the mover; only then are rooms tuned. (GG G3, G13: "Fix this at the movement-system level rather than
  designing around a broken mechanic.")
- **M-2 No silent limits.** A verb never quietly turns into another verb. If a count limits it (wall jumps, dashes, stamina), the limit is
  either generous enough to never bite in a built room, or shown on screen. (G13; ULTRAKILL's 3 wall jumps are learned, ours were never
  shown: movement-web.md.) Tonight: a wall jump off a different wall refills the count (chimney43, DECIDE43 D-1).
- **M-3 Forgiveness on every edge.** Every transition has a window: floor coyote 0.1 s, jump buffer 0.1 s, wall coyote >= 0.1 s, a lost
  wall forgiven >= 0.1 s, a press up to ~0.15 s before the moment it needs (movement-web.md: Celeste, Titanfall reports, a parkour mod's
  120 ms). Attach probes are wider than one step's travel at top speed (at 16 m/s one 120 Hz step is 0.13 m: the entry probe >= 0.35 m).
- **M-4 One input, one meaning per state.** A button pressed in the moment just after a state ends keeps the meaning it had in that
  state, or does nothing harmful: B just after a ride is not a slam that kills your speed (G3; DECIDE43 D-2). A press never throws the
  player backwards unless they pushed into the wall (wallfling42).
- **M-5 Speed and spacing are designed together.** For a chain of walls / platforms, the flight time between two surfaces at the speed
  the game expects is at most ~0.5 s and the next surface is in view before the jump; the along-wall drift during that flight is less
  than the next surface's length (G2: "you can fly past the next wall"). Write the numbers in the generator beside the geometry.
- **M-6 Combined inputs combine.** Two inputs meant to be used together (jump + shoot down, dash + gauntlet) work in either order inside
  ~0.15 s and give the full effect (G26; TF2 / Quake: a self-push resolved on the ground is weak: movement-web.md). No later input (a jump
  release) cuts a boost.
- **M-7 Movement stays exact.** Old rows off = the web's movement bit for bit (CLAUDE.md rule 3): golden 0 mismatches, the replays exact.

## C. Combat feedback ("the player can physically feel the difference")

- **C-1 Every hit answers in four channels:** the screen (flash, sparks, the enemy's flinch), the camera (trauma-squared shake: shake =
  trauma^2, decaying), time (hitstop in whole sim steps), the hands (rumble). Plus an event name for GG's sound. (G20; combat-web.md.)
- **C-2 A ladder, not a flat line.** Hitstop in 120 Hz steps: a bullet 0-1, a shotgun hit 2-4, a charged shot ~6, the gauntlet ~12-20,
  a FUSION or a hit on an OPEN boss 24-34 (0.2-0.28 s). Shake: normal +0.15, charged +0.3, FUSION +0.6. Each tier visibly bigger than
  the one below it (L-001: judged on frames).
- **C-3 Sustained weapons feel sustained:** a held weapon (the TORCH) has a held rumble, a ramp-in (~0.2 s), light on the walls and an
  effect on what it hits (a burning state), not one particle stream (G20; Doom Eternal's flame belch).
- **C-4 Empowered moments look different, not just bigger:** a FUSION (speed + impact) has its own colour and shape: cyan and
  red-orange meeting into hot pink / white (G19; DECIDE43 D-3). Normal hits never use that colour.
- **C-5 Parry reads as intent:** a parry window is measured in time to impact (>= ~0.2 s at the projectile's speed), with a catch
  volume ahead of the player, an early buffer (~0.12-0.15 s) and a mercy after contact (ULTRAKILL: a 3 m sphere 3 m ahead, ~0.12 s early,
  3 mercy frames; combat-web.md). Parryable = yellow, always (PRINCIPLES 3). (G18.)
- **C-6 Flashes are budgeted** (FlashGate: <= 3 a second); no white full-screen flashes; a crush frame (world darkened) is the strong read.

## E. Enemies (look and behaviour)

- **E-1 Roles, not individuals.** The roster is a few roles, each with one silhouette rule all its variants keep: fodder (husk, swarmer),
  ranged (spitter), flyer (drone), charger (the mini-ICEBREAKER), shield / elite (warden, tank), burrower (BURROWER), mini-boss, boss
  (Darktide: readability by layer; encounters-arenas-web.md A4).
- **E-2 The pill stays; the class lives in the top third:** the body is a pill (GG G21: "I like the pill-shaped enemy concept"); the class
  is read from the visor shape (slit = gunner / fodder, round lens = flyer / ranged, chevron brow = charger, plate = shield) plus one back
  part (fins, a tank, an antenna). No enemy is a stack of boxes (G21). (ui-art-web.md C11.)
- **E-3 Colour, then silhouette, then detail.** Enemies are the most saturated warm objects on screen: dark body, bright emissive visor
  that ignores scene light, red-orange danger, yellow only where parryable; a distance-scaled rim keeps them readable at range. Check
  with a greyscale screenshot: enemies and hazards in the top values. (TF2 / L4D; ui-art-web.md C10-C13.)
- **E-4 Every attack has a tell the player can see in one frame**, red-orange for "avoid", yellow for "parry", long enough to answer
  (>= 0.6 s for a melee hit, >= 0.8 s for a big one, longer for bosses).
- **E-5 Enemies never stand still by accident.** No enemy is idle > 3 s while the player is in reach; a stuck enemy is moved (on screen
  too) after ~2 s with a visible step; every room has a climb route for enemies (PRINCIPLES 4). (G22.)
- **E-6 Enemies arrive believably.** No enemy floats waiting for a tutorial: enemies enter through a spawn tell, a door, a hatch, a lift,
  from below or above, at the moment the room needs them (G4).
- **E-7 Bosses are foreshadowed.** A boss's language (its tells, its weak point) is shown 2-3 times by smaller enemies before the boss
  (the CHARGER before THE ICEBREAKER; G23).

## R. Encounters (how a room's fight is written)

- **R-1 Three loops:** the 3 s verb (shoot / dash / parry), the 30 s problem (which enemy first, from where), the 3 min arc (wave 1 teaches,
  wave 2 combines, the last wave twists). No two rooms share the 30 s problem. (Halo; encounters-arenas-web.md A1.)
- **R-2 Introduce -> reinforce -> combine -> escalate -> master:** a new role arrives alone (1-3, visible before it can hurt, the verb
  that beats it taught in the room before); then with one known fodder role; then with a role whose answer conflicts; then the arena
  twists with the same roster; then a mini-boss or boss uses its tell. (A6.)
- **R-3 Budget:** at most one new role per room; at most three roles at once before level 4; five or more only in finales. Escalate by
  role, never by hit points alone (Hakita; GG G27).
- **R-4 Place each enemy where its ability is hardest to handle:** flyers over the hazard, chargers on the long straight, ranged on high
  ground, burrowers under the open floor, pushers near ledges. Geometry and roster are designed together. (A5.)
- **R-5 A door always opens when its fight is over.** A room counts only its own enemies; a body that leaves the room or falls into a pit
  ends; a lesson can never hold a door shut after its enemies are dead; a small counter shows how many are left (G5).
- **R-6 The room table is the memory.** docs/ROOMS.md (per level x room: verbs taught, verbs required, roles present, new item, the
  room's 30 s problem) is updated with every level change and checked before a new room is built (Naughty Dog's spreadsheet; G12).

## A. Arenas and levels

- **A-1 Every piece has a reason.** Every ramp, wall, platform, hazard and piece of cover carries a one-line gameplay reason in its
  generator (which verb it serves, which enemy, which sightline, which rhythm beat); a piece with no reason is cut (G25; Neon White).
- **A-2 One main loop, no dead ends:** the player can run the arena's loop without stopping (for slides and rides), with 1-2 vertical
  cut-throughs (ride walls, pads, boost steps); enemies spawn at loop nodes the player can see. (A13.)
- **A-3 Looks open, plays forward:** rooms chain one way. A cleared room's way back closes (a shutter, a drop, a door that seals behind)
  unless the room is built for backtracking; the completed room may stay visible but unreachable (G11; Hakita on 4-2; DECIDE43 D-8).
- **A-4 Failure costs time and a little health, not a restart:** a missed jump over a hazard returns you to the lip with a hit (lava:
  DECIDE43 D-5), never a long walk back; a failed jump never redoes a cleared fight (ULTRAKILL; G1).
- **A-5 Hazards are real and read:** lava glows, moves and lights the geometry from below; a hazard both punishes the player and can be
  used against enemies (knock them in).
- **A-6 A landmark shows forward:** one landmark per arena visible from its entrance; the exit is lit; the next door is lit when the
  room clears (G11).
- **A-7 Teaching rooms are real rooms:** in the level's art, under ~30 s each, with the lesson the only new thing (encounters-arenas-web.md
  A7: Doom Eternal's featureless tutorial rooms "slow down a game that should be going very fast").
- **A-8 Transitions keep the flow:** a door carries the player's speed into the next space (no wall within a few metres, no hard speed
  clamp, no freeze at the doorway); the boss door is in the boss's language, never a colour that reads as a bug (G10).
- **A-9 Identity before geometry (GG 2026-10-17: "it needs intension and personality"):** no piece is placed until the arena's identity
  card is filled (docs/ROOMS.md, under the room's row); the geometry is then derived from the card, and a piece no field asks for is cut.
  Sources: Research/2026-10-17-wave45/research/arena-identity.md (Shadow of the Colossus arena-first design, Ravenholm's "Traptown", Hades'
  one spatial rule per biome, Halo's "every solid forces a decision", ULTRAKILL's arena state changes, the Level Design Book).
  The card (every field one line):
  | field | asks |
  |---|---|
  | Title | a 2-4 word name of the PLACE, not its function |
  | Place fantasy | who built it, for what, who is here now |
  | Spatial law | one: walled / islands / shaft / ring / nave / yard / bridge |
  | Verb it celebrates | the one verb this arena exists for (from the level's verbs) |
  | The 30 s problem | a puzzle sentence: threat X at Y; verb Z reaches it (no other room's sentence) |
  | Molecule | the nodes, the one-way links, the one shortcut that skilled movement earns |
  | Landmark | the useful thing seen from the entry view, and how it stands out (not the generic exit pylons) |
  | Hero prop | the one object that says what happened here, and what it does in play |
  | Hazard both ways | what it does to the player, and what it does to enemies (or "none" with the reason) |
  | State change | the player action or wave that changes the arena, and the light / colour flip that shows it |
  | Light / colour story | the entry (tight, dark) vs the arena (open, bright); what turns hot |
  | Every-solid rule | for each solid: the route or the encounter decision it forces |
  | Tell-a-friend line | the one sentence a player would say about this arena |
  | Cut list | what the walk / the frames showed nobody used |

## T. Tutorials and teaching

- **T-1 Teach where first needed, once** (PRINCIPLES 2): a trigger volume, the player's own bound glyph, about 4-5 s or until the verb
  is done, once per save (ULTRAKILL's HudMessage; ui-art-web.md A1).
- **T-2 The centre card:** important teaching text sits centred, above the crosshair (never on it), big, opening fast (~0.16 s,
  horizontal then vertical, in unscaled time so it plays in slow motion) and leaving on its own (G7).
- **T-3 The words:** VERB IN CAPS + the bound input + at most one rule; at most ~6 words; at most two lines in a level's first minute
  (DECIDE42 D-7).
- **T-4 Teach, confirm, then test:** a lesson's room waits for the player to perform the verb (the game confirms it) before the enemies
  that need it arrive (G4: `afterLesson43`); the verb is then used safely before it is tested under pressure.
- **T-5 Small, then big:** a verb's first use is the easy version (a small boost step, a short wall gap), then larger (G26).
- **T-6 Introduce a verb before a room demands it:** the dash is taught before any room needs it (G1).

## P. Progression

- **P-1 One new idea per level, six levels** (PRINCIPLES 4); the boss is the level's final exam of its verb.
- **P-2 The player gets stronger as the rooms get richer:** difficulty rises by richer combinations of roles while the player's kit
  grows; never by HP alone (G27).
- **P-3 Nothing lost** (PRINCIPLES 1): every verb in docs/VERBS.md is performed in a Player at every close.

## U. UI

- **U-1 One visual language:** the type scale, font roles, colour tokens, panel shape and motion rules in the UI spec
  (Research/2026-10-16-wave43/U1.md, folded in here once ON) are used by every screen; a new screen uses the tokens, never its own.
- **U-2 Flat fills, few colours:** dusk navy panels, pale violet-white text, cyan for the player's state, red-orange only for danger;
  gradients only inside meters (Persona 5's flat fills; ui-art-web.md B5).
- **U-3 Hierarchy:** a full screen has one hero number (2-3x anything else), a supportive band, the rest small; the selected item
  scales (~1.08) and lights its cyan edge (B6).
- **U-4 The HUD eases play; menus carry personality:** the HUD is minimal and corner-tucked; full screens (shop, results) blur and
  darken the game behind them and use angled panels and motion (Sakurai; B7, B8; G24).
- **U-5 Everything moves a little:** every change animates (a punch, an after-image, a sweep); nothing pops statically; the old-TV
  motion (PRINCIPLES 7). No filled HUD boxes, no mid-screen stamina bar (w33's "never again").
- **U-6 The health bar** (**changed by hud47, NIGHT47**: GG 2026-10-18 "a single continuous bar instead of a series of tiny segments",
  green -> lime -> yellow -> orange -> red in OKLab, overheal blue -> purple; the text below is the NIGHT46 rule, `-skygrindOff47`) is a segmented, framed bar with a damage after-image (waits ~0.15 s, drains), a cyan heal sweep, the frame (not
  the fill) pulsing on a hit; low health breathes the frame, never a full-screen tint (B9).
- **U-7 Readable sizes:** UI text >= 16 px at 1080p (CLAUDE.md rule 8); the fonts are the theme's (UnityTuning.Theme.cs: Archivo
  ExtraCondensed Black Italic display, Archivo body, Tourney rank).

## V. Art direction (the world)

- **V-1 The saturation budget belongs to gameplay:** the environment is desaturated mid-low value (navy / violet dusk); enemies, hazards
  and pickups carry the emissive; the player-usable is the only cyan (L4D; ui-art-web.md C10).
- **V-2 Hue ownership:** cyan = the player can use / stand on / ride; red-orange-yellow = enemy or danger (lava fits); magenta = RIVAL;
  FUSION hot pink = the empowered impact only; pale violet-white = the boundary (CLAUDE.md rule 8; DECIDE43 D-3).
- **V-3 Ridable walls look ridable:** one consistent cyan edge strip and a texture rhythm along the ride direction; a wall that cannot be
  ridden never has it.
- **V-4 Decoration must tell one of three things:** the route, a threat, or the world (foundry, ice line, kiln). Never "a neon strip
  because the wall was empty" (G8: no meaningless clutter).
- **V-5 Lights hang from real fixtures and light the room** (PRINCIPLES 7); light marks the way forward and the hazards.
- **V-6 The PS2-era treatment stays visible** in every frame (low internal resolution, dither, colour steps, vertex snap; PRINCIPLES 7).

## B. Bosses

- **B-1 The bar tells the truth for the whole fight:** HP with an after-image, the state (LOCKED: what to do to open it; OPEN: full damage
  and for how long; STAGGER), the phase / leg; chip damage visibly counts; the bar never vanishes mid-fight (G14).
- **B-2 Phases are beats:** each phase change is a readable moment (the bar's tick, the arena changes, the boss recoils, a name or
  line) and adds one variation on the language already taught, never more HP (G16; GG w31-w39: bosses 1-2 min for a person).
- **B-3 The intro is cinematic and short:** in the world, the player keeps control (or a skippable ~3 s), letterbox + framing + lights;
  the first time <= 6-8 s, retries <= 1.5 s (bosses-web.md; G15; DECIDE43 D-7).
- **B-4 Fundamentals first:** a boss built on a verb is only as good as the verb; its fight is checked with the verb's harness (the climb
  share, the parry share) before its attacks are tuned (G13, G16).
- **B-5 Keep what GG likes:** THE WICK's parry (EMBER), the mini-boss foreshadowing (CHARGER), the intro's foundation (G15, G16, G23).

## Checklist for a new room (copy into the card)
1. Its row in docs/ROOMS.md: verbs taught / required, roles, new item, the 30 s problem (R-1, R-6).
2. Every piece's reason in the generator (A-1); the loop and its cut-throughs (A-2); the way back closed (A-3); a fall's cost (A-4).
3. Flight times and drift for any chain at the expected speed (M-5).
4. Enemies' entrances (E-6), placements (R-4), budget (R-3); the door logic (R-5).
5. The lesson order (T-4) and its card text (T-3).
6. The landmark and the lit exit (A-6); the hue ownership (V-2); a greyscale frame (E-3).

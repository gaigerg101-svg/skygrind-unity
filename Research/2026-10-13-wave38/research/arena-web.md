# Arena, forced-verb rooms, flow, doors, readiness HUD, boss juice: web research (wave 38)

Date: 2026-10-13. Time box ~12 min. Every claim has its URL.
Tags: **[fetched]** = I read the page itself (via WebFetch's summariser); **[snippet]** = from a search-result summary only
(lower trust; confirm before quoting to GG). "not checked" = I could not find a source in the time box.

---

## 1. Open arena design in fast shooters

### Doom 2016 / Doom Eternal (id Software)
- Doom's combat "takes place in medium to large scale arenas that are asymmetrical in nature, carry a lot of verticality";
  with demons added it becomes "combat chess: where your job is to figure out which demon to smash first and how best to
  get there". GDC 2018, Kurt Loudy and Jake Campbell, "Embracing Push Forward Combat in DOOM".
  [snippet] https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in ,
  https://gdconf.com/news/see-doom-devs-break-push-forward-combat-design-gdc-2018
- Push-forward rule: "If the player had to hide behind a wall, run away, or wait for something to recover, then that
  needed to be changed." Enemies are the resource dispensers (health, armour, ammo by kill type); "rule-breaker" enemies
  (Marauder) stop one dominant strategy. [fetched]
  https://www.gamedeveloper.com/game-platforms/pushing-push-forward-combat-with-gameplay
- Hugo Martin on Doom 2016's weakness: "skate parks and arena fights; hallways to the next arena, fight, hallways to the
  next arena, fight". Eternal had to "engage the player just as much with the spaces outside of the arenas"; traversal
  (double dash, monkey bars, climb walls) are "skill-based traversal puzzles" where "what would be challenging in the
  beginning of the game becomes second nature at the end". [snippet]
  https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/ (403 on fetch),
  https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- Arena taxonomy (a designer's analysis of Eternal): arenas are "flat-ish or tall-ish", tall-ish in "two flavors: jungle
  gyms and canyons". [snippet] https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal (403 on fetch)

### ULTRAKILL (Arsi "Hakita" Patala)
- "The levels are linear to make sure the fast pace is kept up and the player doesn't get lost or bored, since there's
  always more action right around the corner." "The game moves at a constant forward momentum with only occasional
  downtime." [fetched] https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- Readability: "As soon as you look at a room it's easy to understand it with no details or decorations distracting or
  obfuscating it"; geometry "simple and clean" so the player "can move at a high speed with no issues". [fetched] same URL.
- Threat comes from count and mix, not single-enemy smarts: the player "has a lot of movement tools to easily outmaneuver
  anything, but that becomes less of an issue with how many enemies there are in each encounter". [fetched] same URL.
- Open-but-linear: "a level that seems very open but without actually sacrificing the constant forward momentum of a
  linear level, so you're actually still moving along a single direct path, it just doesn't feel like a hallway because it
  doesn't look like one." Mixes over numbers: "Certain combinations of enemies will force the player to approach a
  situation differently"; "one Hideous Mass at the same time as 2 virtues is pretty much as difficult as fighting a ton of
  husks". Pacing: a hard encounter straight into a harder one exhausts players; avoid points "where you have to just wait
  for something". [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/

### Neon White (Ben Esposito et al.)
- Five level rules: "The player always knows where to go next"; "The player always has the right tool for the next
  encounter"; "There is at least one non-trivial shortcut"; "The shortcut path cannot be hidden"; "The shortcut path must
  only use cards from the main path". Encounters are "blocked out in isolation, then assembled"; assembly creates
  shortcuts. Levels "never require players to do all three at the same time" (platforming, shooting, card management);
  on the main path "you almost never need to swap cards". [fetched]
  https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
- Cards: fire mode + a discard movement ability (yellow pistol = double jump on discard); the team kept "the seven
  simplest ability ideas". [snippet] same URL.

### Titanfall 2 (Respawn)
- Campaign built with "action blocks", a rapid-prototyping technique: small gameplay set-pieces prototyped, then assembled
  into levels (GDC 2018, Christopher Dionne). The article holds no wall-run spacing numbers. [fetched]
  https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks ; talk:
  https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player
- Wall-run spacing metrics: not checked (not public in what I found).

### Turbo Overkill (Sam Ridley / Apogee)
- "Some platform sections have jump-pads linked to wall-runs, followed by dashing into another wall-run, all before
  leap-dashing straight into an elevated combat arena full of thugs." "wall-running can be used to reach the other side of
  a combat arena." Weapon-as-traversal: the shotgun alt-fire's splash is "a rocket-propelled jump to get to a higher
  vantage point (or even a secret area in-between combat arenas)". [fetched]
  https://gamingbolt.com/turbo-overkill-interview-inspirations-level-design-weapons-and-more

### Mullet MadJack
- 10 s life timer on normal (15 s on easy), refilled by kills; levels "usually last no more than a minute"; a boss every
  ten floors; new elements (laser trip wires, wall running, acid) introduced over floors. [snippet]
  https://www.analogstickgaming.com/game-reviews/2024/5/17/mullet-madjack , https://paulsemel.com/mullet-madjack-review/

### Severed Steel, DUSK
- Severed Steel: dive, slide and wall-run, each "inspired from different games"; arenas from factories to marble rooms.
  [snippet] https://www.unrealengine.com/developer-interviews/severed-steel-is-an-fps-inspired-by-superhot-and-hotline-miami-made-mostly-by-a-solo-dev?lang=en-gb
- DUSK: not checked.

### Quake / Quake 3 geometry
- Verticality: "Most maps tend to max-out at three different floor planes for any given area." Q3DM17 uses "a dozen jump
  pads and teleporters to create strong upward flow"; high-ground snipers can shoot "players taking predictable flight
  arcs" (a pad arc is a readable, punishable line). Console/pad shooters "rarely incorporate very tall height changes"
  because pad aiming up/down is taxing. One-way drops "force the player into a combat encounter, or else the player can
  simply backtrack to pull enemies". [fetched] https://book.leveldesignbook.com/process/layout/flow/verticality
- Q3 maps built around "loops" that hit every major item without stopping or turning around; mega items on 25 s / 35 s
  respawns. [snippet] https://library.peoplespharmacy.com/quake-3-arena-map-layout (low-trust site)

### Halo
- "30 seconds of fun": a single encounter = enter a space, plan an approach, execute; then re-run it in new spaces, with
  new weapons and enemy mixes. Griesemer says people misuse the quote. [snippet]
  https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html

---

## 2. Forced-verb platforming that can't be cheesed

- **Measure the envelopes first.** Quake's measured limits: running jump 244 u, standing jump height 43 u, ramp jump
  ~96 u high, grenade/rocket jump "~320 long - ~128 high"; "Build for a shorter jump distance / height" since most players
  lack perfect timing. Ceilings: 64 u bare minimum, 128 u recommended, 256 u+ tall. [fetched]
  https://book.leveldesignbook.com/process/blockout/metrics/quake
  -> The rocket-jump gate is the gap *between* envelopes: a gap of ~270-300 u cannot be run-jumped (244) but is
  comfortable for the rocket jump (320). That is the technique: every gate sits above the best envelope of every
  non-intended verb (and every combo of them) and below ~85-90 % of the intended verb's envelope.
- Build a "metrics zoo test map ... various rooms, hallways, and floor heights" and iterate; hallway width at least 2x
  the player width. [fetched] https://book.leveldesignbook.com/process/blockout/metrics
- Gap at the exact max distance = "pixel-perfect" frustration; one rule of thumb: safe gap = 70 % of max, hard gap = 95 %
  (optional rewards only). [snippet, low-trust aggregator] https://claudeskills.info/skills/gamedev-skills/awesome-gamedev-agent-skills/level-design/
  ; see also "Lessons from Suzy Cube: measuring up" [snippet] https://www.gamedeveloper.com/design/lessons-from-suzy-cube-measuring-up
- **Resource caps stop spam.** ULTRAKILL: 3 stamina, dash costs 1, dash-jump costs 2, no stamina regen while sliding,
  regen 0.7/s on Standard, a parry refills all stamina; wall jumps "a maximum of three times in the air, after which
  further wall jump attempts will fail and do nothing". [fetched] https://ultrakill.wiki.gg/wiki/Movement
  -> A wall-ride chain gate can be made unspammable by the chain cap itself, plus walls spaced beyond the cap's reach.
- Neon White: gates are resource decisions ("save the first card in the level, and do some slight ammo management" to
  reach the shortcut in Dasher); shortcuts use only main-path cards, so the intended verb is always on hand. [fetched]
  https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
- ULTRAKILL 0-1 "Layered Walkway": "A glass wall can be broken to be able to wall jump up to the upper floor" (a surface
  that changes which verb works). [fetched] https://ultrakill.wiki.gg/wiki/0-1
- Celeste: each chapter introduces one rule-changing mechanic, introduced slowly; compact rooms keep "the end goal in
  sight always" so the path is visualised on entry. Critique: some rooms extend length instead of adding a twist.
  [snippet] https://game-wisdom.com/analysis/celeste
- Doom Eternal: slime "prevents players from jumping" (a floor that removes a verb). [snippet]
  https://giantbomb.com/user-reviews/id/250760/dissapointment-thy-name-is-eternal (user review; low trust)

---

## 3. Flow between rooms without stopping

- ULTRAKILL 0-1: the first arena is "in darkness until the player picks up the Revolver, with picking it up causing the
  lights to turn on" (pickup = trigger); the Pipes Hallway spawns "6 Filth (After reaching the third corner in the
  hallway)" (enemies placed on the route, triggered by position); checkpoints sit before the next arena's door. [fetched]
  https://ultrakill.wiki.gg/wiki/0-1 ; waves: drones die -> four more spawn "one in each corner" [snippet]
  https://ultrakill.fandom.com/wiki/1-E:_...THEN_FELL_THE_ASHES
- Doom Eternal: "once you have killed all of the demons in a hallway, the door at the other end will unlock"; a Hell
  barrier blocks the way until the arena's demons die. [snippet]
  https://www.trueachievements.com/game/Doom-Eternal/walkthrough/6
- ULTRAKILL S-rank time = Hakita's own run "lower my results by a bit" (time pressure rewards not stopping). [fetched]
  https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- Mullet MadJack: a draining 10 s timer refilled by kills makes stopping lethal. [snippet] (URLs in section 1)
- One-way drops push the player forward into the next fight. [fetched]
  https://book.leveldesignbook.com/process/layout/flow/verticality

---

## 4. Doors and room boundaries

- ULTRAKILL boss door: "The door is lit in yellow with a skull in the center, signifiying a boss encounter." [fetched]
  https://ultrakill.wiki.gg/wiki/0-1
- See-through barrier: "A blue force field around a gate shows players that an area is off limits while allowing them to
  see what is out of reach"; lit objects draw the eye "even from behind barriers"; colour locks so states read fast.
  [snippet] https://rafaelpineiro.gamedesignskills.com/?p=376
- Readability rules (Brendon Chung): "Light to draw the player's eye"; "Cutaway that gives line of sight to the adjoining
  area"; elevation or texture change "to communicate the boundary of a separate area"; consistent metrics so you
  "instantly identify what is climbable ... what are the exits"; TF2's "big shapes, muted/flat textures"; the "squint
  test". [fetched] https://blendogames.com/news/post/2026-06-xx-leveldesign_readability/
- Doom Eternal red barrier visuals, Halo / Titanfall door shields: not checked beyond the "Hell barrier" snippet above.

---

## 5. Readiness HUD and damage feedback

- ULTRAKILL stamina: 3 segments on the HUD; "V1's wings also flash colors depending on the amount of stamina" (the
  readiness is also on the body). [fetched] https://ultrakill.wiki.gg/wiki/Movement , https://ultrakill.wiki.gg/wiki/V1
- ULTRAKILL railcannon: "a large lightning bolt icon next to the player's weapon icon"; cooldown "indicated by a distinct
  sound effect and a marker on the HUD"; a hum that "increases its pitch based on the charge level"; neon screws on the gun
  that "deactivate after firing" and "reactivate one at a time" (diegetic quarter-step charge). [fetched]
  https://ultrakill.wiki.gg/wiki/Electric_Railcannon
- Modders added a crosshair charge bar with per-weapon options "Charge Bar, Sound, and Flashing Icon" (players wanted
  readiness at the crosshair). [snippet] https://thunderstore.io/c/ultrakill/p/daemon47/UltraCooldownInfo/v/1.0.6/
- Doom Eternal: 20+ HUD options, six colour profiles. Dash pips / equipment rings near the crosshair: not checked
  (search found no source). [snippet] https://slayersclub.bethesda.net/en/article/2V0erykuLGuV8a8GsmribP/doom-eternal-custom-huds
- Apex: readiness is announced by a ping prompt; players asked for a slim bar that "can glow and have a small spark at
  the end" when full. [snippet] https://forums.ea.com/discussions/apex-legends-feedback-en/consider-making-teammates-ultstactical-more-visible/13609655
- Damage glitch: chromatic aberration splits RGB channels and can be scaled by events "such as increasing aberration when
  a character takes damage"; Black Mesa uses it as a damage indicator. [snippet]
  https://blog.imagineersystems.com/blog/what-is-chromatic-aberration-in-games

---

## 6. Boss entrance juice

- ULTRAKILL telegraphs bosses at the door (yellow light + skull, above). [fetched] https://ultrakill.wiki.gg/wiki/0-1
- ULTRAKILL uses hit-stop for big impacts. [snippet] https://wegotthiscovered.com/?p=870522
- Name cards / title-card freeze in ULTRAKILL and Doom Eternal: not checked (no source found in the time box; do not
  claim them as facts).
- Generic boss reveal elements: name, epithet, silhouette, arena backdrop, health/phase bar. [snippet, prompt gallery,
  low trust] https://carat.im/prompt-gallery/game-hud

---

## 12 rules for SKYGRIND (derived from the above)

1. **Envelope table first.** Measure every verb and every legal combo (jump, double jump, dash, jump-dash, wall-ride exit,
   grapple, shotgun boost, slide-jump) as max distance and max height in metres from `config.json`; gate gaps between
   envelopes, as Quake's 244 u run jump vs ~320 u rocket jump (book.leveldesignbook.com metrics/quake).
2. **A forced-verb gap is > best non-intended envelope + margin and <= ~85-90 % of the intended one.** Never at 100 %
   (pixel-perfect frustration).
3. **Kill the alternatives physically:** a ceiling under the double-jump apex blocks height cheese; a visibly
   non-rideable wall blocks the ride; wall spacing beyond the chain cap kills chain spam (ULTRAKILL caps air wall
   jumps at 3).
4. **Resource caps beat geometry for anti-spam** (ULTRAKILL: 3 stamina, dash-jump costs 2, no regen while sliding);
   the intended verb's resource must be on hand at the gate (Neon White: "the right tool for the next encounter").
5. **Open-looking but one direct path** (Hakita): wide roofed arenas whose exit is visible from the entrance, max three
   floor planes per area (LDB verticality), with short height changes because of pad aiming.
6. **Threat by mix, not count or HP:** each arena pairs a few types that force a change of approach (Hakita; Doom's
   "combat chess"); one rule-breaker at most.
7. **Enemies on the route:** trigger spawns by position and pickups (ULTRAKILL 0-1: third corner, revolver pickup),
   ahead of the player's line, so the fight starts while moving.
8. **Never make the player wait:** no recover-behind-cover, no idle door timers (Doom push-forward; Hakita); refill
   from kills; one-way drops into fights.
9. **Doors read at a glance:** see-through coloured barrier while locked, colour + light change on unlock, a boss door
   with its own colour and symbol (ULTRAKILL yellow + skull); a cutaway shows the next room (Blendo).
10. **Teach then test then twist:** one new verb per room sequence, compact rooms with the goal in sight (Celeste,
    Doom's "second nature" traversal); never ask for platforming + shooting + weapon management at once (Neon White).
11. **Readiness lives at the crosshair and on the body:** segmented pips, a charge-pitch cue, a flash when ready
    (ULTRAKILL wings / railcannon screws; mod demand for crosshair bars); the damage cue is a short RGB split / static
    scaled by the hit (Black Mesa).
12. **Boss entrance:** a telegraph at the door, then hit-stop + shake on the reveal with the name on the bar; exact
    ULTRAKILL / Doom title-card behaviour not checked, so prototype and judge blind.

# Boss research (web): bosses you fight while moving (wave 37)

Researched 2026-10-12 in about 12 minutes. GG asked for four boss fights rebuilt from scratch, built around constant
movement ("consistently wall riding, jumping across platforming as you're killing this boss"), and each one has to force
its level's mechanic and weapon.

The tags mean:
- **[fetched]**: the page itself was read with WebFetch.
- **[snippet]**: the claim comes from search-result text only; the page was not opened.
- **[gh]**: repository metadata from `gh search repos`.

Numbers are as the source gives them. Anything I did not find is marked "not checked".

---

## 1. Bosses built on traversal, chases or a moving arena

### ULTRAKILL 7-4 "...Like Antennas to Heaven": the 1000-THR Earthmover (the boss is the level)
- The player comes up out of a bunker and climbs the Earthmover's outside in sections: up the front leg on hookpoints,
  up the torso on platforms and catwalks, across a front balcony, then into a circular drop on the front-left leg. Then a
  "Defence System" arena, then the inside, then the brain. [fetched] https://ultrakill.wiki.gg/wiki/7-4:_...Like_Antennas_to_Heaven
- The Defence System comes in stages. A Mainframe stays shielded until the other parts are destroyed. There are 2 rocket
  launchers. 2 mortars spawn after any enemy dies, and 2 towers spawn after any 2 are destroyed. A cyan barrier keeps the
  player in. [fetched] same URL
- On the inside, boiling blood rises (25 damage, and it launches the player up). Six Idols spread across the metal
  walkways power a ceiling barrier. The brain sits in a force field (10 damage on contact) and fires magenta lasers that
  deal 20 even if you dash through them. Killing it starts an escape timer: 100 s on Harmless, 50 s on Brutal.
  [fetched] same URL
- "The Earthmover itself is unable to move or attack V1 ... acts as the terrain of its level." [snippet] https://ultrakill.wiki.gg/wiki/Earthmover
- **What the player does:** climb the boss, then destroy its parts in order. Movement is how you get to the damage. The
  shape is vertical: you climb outside, then fall inside. The pacing goes climb, then fight, then climb, then a
  centre-room kill, then a timed escape.

### ULTRAKILL 5-4: the Leviathan (platforms over water, and the arena changes each phase)
- It has 240 HP: phase 1 runs from 100 to 75 %, phase 2 from 75 to 50 %, and phase 3 from 50 to 0 %. Phase 1 alternates
  the head and the tail; phase 2 uses both at once from different places; in phase 3 the head attacks from the centre
  while the tail swings from outside. [fetched] https://ultrakill.wiki.gg/wiki/Leviathan
- The head lunge (35 damage) can be parried. If it misses, you can get in close or climb onto the head. A barrage of 80
  Hell Orbs (25 each) ends with the Leviathan looking down, which exposes the heart. The heart takes triple damage.
  [fetched] same URL
- The tail sweeps either high (you slide or duck under) or low (you jump it), and its rotation direction tells you which.
  Neither sweep can be parried. [fetched] same URL
- **Phase 3 throws the arena away:** the boat is destroyed and the floor becomes the Leviathan's own body, a ring that
  turns slowly clockwise. The player grapples green hookpoints and the Leviathan itself. [fetched] same URL
- **What the player does:** grapple between scraps of floor and read the sweeps to choose a jump or a slide. Movement is
  how you reach the damage, because the heart opens only after a barrage and the head is reachable only after a lunge
  misses.

### ULTRAKILL Minos Prime, Gabriel and V2 (bosses that move like the player)
- Minos Prime has 130 HP in two phases of 65, with 6 stamina in phase 1 and 12 in phase 2. Each attack costs stamina,
  and when it runs out he is locked out for 2 s. Every attack has a voice cue ("Judgement!", "Die!", "Crush!") and a
  colour flash: blue means "dodge", and the yellow / orange snake projectile can be parried and fully heals the player.
  The uppercut launches the player into the air and chains into an overhead strike, a ground slam or a rider kick.
  Parrying gives him 5 stamina back. In phase 2 the gap between attacks gets much shorter. [fetched] https://ultrakill.wiki.gg/wiki/Minos_Prime
- Gabriel has 100 HP in two phases of 50. He teleports above the player and lunges, or throws spears and axes, and the
  thrown weapons can be parried. A yellow flash means the spiral swords are about to launch. At 50 % he is thrown into
  the air stunned, and the hits he takes while falling bounce him back up (the juggle is the phase change). He taunts
  between combos, which gives the player a window to heal. "Standing still invites heavy punishment"; "diagonal dodging
  more effective than backward". [fetched] https://ultrakill.wiki.gg/wiki/Gabriel,_Judge_of_Hell
- V2 "can dash, wall jump, and slide just like the player". It throws coins and the player can shoot them back as a
  counter-ricoshot. [snippet] https://ultrakill.fandom.com/wiki/V2_(Second_Encounter)
- **What the player does:** a duel in an open arena where the boss has the player's own moves. The damage windows come
  from the boss's stamina and from parries; there is no armour-plate puzzle.

### Ghostrunner: Mara (the floor turns deadly and the walls become the arena)
- There are 3 phases and 3 tentacles to break. In phase 1 she swings tentacles (jump over), smashes down, and sends a
  ground flame that follows you (dash sideways). Her stab gets stuck in a console if you stand in front of one, and then
  you cut the stuck tentacle. [fetched] https://gamerwalkthroughs.com/ghostrunner/level-16-the-summit/
- In phase 2 part of the platform is electrified, and then all of it, so you "grapple up to the side panels and wall run
  around" before dropping back down for the stuck tentacle. [fetched] same URL
- In phase 3, two electrified sections rotate clockwise and then anticlockwise, and the full-floor wall-ride rounds come
  back. [fetched] same URL
- **What the player does:** the arena state forces wall running. The damage window is made by the player's position:
  the player is the bait, and the boss gets stuck on the scenery.

### Ghostrunner: Hel, and the Golem in Project_Hel
- The Hel fight asks the player to "commit each and every dodge, block and counter to memory". [snippet] https://www.geekyhobbies.com/ghostrunner-project_hel-dlc-video-game-review/
- The Golem fight is on platforms under a monorail carrying vats of molten metal. The player hits the vats so they drop
  on the Golem: damage that comes from the environment, not from the gun. [snippet] same URL

### Shadow of the Colossus (climbing the boss)
- "Each colossus is effectively a moving level"; the player climbs it while it "bucks and swipes"; a grip stamina bar
  limits how long you can hold on. [snippet] https://gamedeveloper.com/design/the-art-of-shadow-of-the-colossus-2-6-gameplay-and-story
- **For SKYGRIND:** the boss's body is the route, and a resource that runs out (grip) stops you holding a perch forever.

### Titanfall 2
- Viper's fight is on the IMS Malta, a ship in the air with "numerous platforms, slim walkways, and open areas". Viper
  attacks from the air with a rocket barrage, the Flight Core. [snippet] https://janettemalloryinteriors.bigrigmedia.com/viper-the-apex-predator-of-titanfall-2-2
  (a low-quality page; not checked anywhere better)
- "Effect and Cause" uses two maps that line up exactly, one per time period, and the player switches between them
  during traversal and combat. It "was the most labor and time-intensive level". [snippet] https://en.wikipedia.org/wiki/Effect_and_Cause
  The mapcore breakdown returned 403 (not checked).
- Ash and Kane: "defeat her in short order"; I found no arena details (not checked). [snippet] https://titanfall.wiki.gg/wiki/Into_The_Abyss

### Sonic (bosses you chase)
- In Generations, the player chases the Egg Dragoon "as it flies backward through a long tunnel", across grind rails and
  pits, and skydives down a hole. [snippet] https://info.sonicretro.org/Egg_Dragoon_(Sonic_Generations_boss)
- In Unleashed (the Egg Beetle), Sonic runs automatically the whole fight, and boosting closes the distance to the mech.
  [snippet] same search
- **For SKYGRIND:** the boss runs away down a lane and you have to catch it; speed decides when you can hit it.

### Neon White: Neon Green
- In the clocktower fight you have to kill Green before he reaches the top. In the final fight, crystals appear one at a
  time, and the player crosses the gaps between them using Green's own projectiles. [snippet] https://en.wikipedia.org/wiki/Neon_White

### Doom Eternal: Icon of Sin
- Phase 1 has 8 armour parts (both pecs, both shoulders, both hands, the abdomen, the head). The Icon "becomes faster and
  more aggressive as its parts are broken". From time to time it ejects Argent energy that "damage[s] the player if they
  stay in place". [snippet] https://www.doomwiki.org/wiki/Icon_of_Sin_(Doom_Eternal) (fetch: 403)

### Hades: the Bone Hydra; Furi
- The Hydra's arena is ringed by lava. At 66 % it becomes immune and summons 3 extra heads, and at 33 % it summons 6.
  [snippet] https://primagames.com/gaming/hades-boss-guide-lernaean-bone-hydra
- In Furi, each boss has 3 or more health pips. Fights switch between a bullet-hell phase and a close-combat phase
  inside a ring, and later phases cover "more and more of the small arena". [snippet] https://itch.io/blog/772886/game-analysis-of-core-systems-furi
  and https://godisageek.com/reviews/furi-review/

### Cuphead
- From GMTK's "How Cuphead's Bosses (Try to) Kill You": the attacks exist "to test you on your ability to run, jump,
  dash, and parry while simultaneously shooting". [snippet] https://amara.org/v/C3BEq (the transcript returned 403)
- Not checked: the airplane stages, Ratchet & Clank's on-rails bosses, and the Mario Galaxy Bowser fights (only a
  snippet: "gravity-based tricks", https://mariowiki.com/Bowser's_Dark_Matter_Plant).

---

## 2. Design rules

- **Telegraph, attack, recovery as reusable "maneuvers"** (Psychonauts 2, GDC 2023, Rebecca Vessal). A maneuver is a
  data-driven unit made of a telegraph, the attack and a recovery. Maneuvers chain into sequences, are tuned per boss and
  per phase, and attacks can be reused with new parameters. Each boss got about 2 months of concept and prototype work.
  [fetched] https://www.gamedeveloper.com/marketing/using-a-modular-system-of-maneuvers-to-design-i-psychonauts-2-i-s-boss-fights-in-a-hurry
- **The eight beats of a boss (Mike Stout):**
  1. Build-up: the skill is taught beforehand.
  2. Reveal.
  3. "Business as usual": the attacks you already know.
  4. Escalation: new attacks or complications.
  5. Midpoint: a turn and a short rest.
  6. "It's ON!": everything at once.
  7. Kill sequence: "dies spectacularly".
  8. Victory.

  "A boss is a test" and "a boss is a story". [fetched] https://www.gamedeveloper.com/design/boss-battle-design-and-structure
  This is close to kishotenketsu (introduce, develop, twist, resolve); I found no source that uses that word for bosses
  (not checked).
- **Making Less Bad Bosses (Damion Schubert):**
  - "Show me my progress": armour falling off, pain reactions.
  - "Get me back to the fray": fast retries.
  - "Teach me how to beat the boss": a new mechanic is taught before the fight.
  - Build from the core mechanics.
  - Avoid "tank and spank" stat inflation.

  [fetched] https://zenofdesign.com/making-less-bad-bosses
- **Every attack telegraphed, then a punish window:** "Every attack must be telegraphed, the player must see the windup
  and understand the timing, and have a window to respond"; the rhythm is attack window, punish window, recovery window.
  [snippet] https://www.wayline.io/blog/crafting-legendary-boss-battles
- **Colour code for parry versus dodge:** ULTRAKILL flashes blue for a dodge and yellow for a parry or projectile, and
  adds a voice cue to each attack. [fetched] Minos / Gabriel pages above
- **Stamina caps how long the boss can keep attacking:** Minos is locked out for 2 s when his stamina runs out, and a
  parry gives him +5. This makes a damage window without adding health. [fetched] Minos page
- **Anti-camping:**
  - A floor that turns deadly (Mara's electricity).
  - A ground pulse that hurts anyone who stays put (the Icon of Sin's Argent energy).
  - Rising lava or blood (the 7-4 interior).
  - A floor that turns (Leviathan phase 3).
  - Weak points that open only to a player in the right place (the Leviathan's heart).

  [fetched / snippet] as cited above
- **Short health plus phase gates:** Minos 130, Gabriel 100 in two halves, Leviathan 240 in three thirds. The phases
  change attacks or the arena, never just the health. [fetched]

---

## 3. GitHub: patterns worth borrowing (shape and numbers only)

| Repo | Licence | Pattern |
|---|---|---|
| meniku/NPBehave (1,275 stars) | MIT [gh][fetched] | An event-driven behaviour tree. Nodes stay in their current state instead of re-ticking from the root. Blackboard observers trigger re-evaluation. Each decorator has a **Stops** rule (`SELF`, `LOWER_PRIORITY`, `IMMEDIATE_RESTART`, `BOTH`), so "HP < 50 %" can abort the running attack and jump to the phase-change branch. A `Service` updates the blackboard on an interval, and `Wait` takes a random variance. https://github.com/meniku/NPBehave |
| igiagkiozis/CrystalAI (473) | MIT [gh][fetched] | Utility AI in C#: score each option and pick the best. Could choose the next maneuver from the player's distance, height and wall-ride state. https://github.com/igiagkiozis/CrystalAI |
| adammyhre/Unity-Utility-AI (105) | Unlicense [gh] | A small Unity utility AI (the README was not read). https://github.com/adammyhre/Unity-Utility-AI |
| emillon/bulletml (98) | BSD-2-Clause [gh] | BulletML: bullet patterns written as data (repeat, fire, changeDirection, wait). This is the shape for SKYGRIND's yellow (parryable) volleys. https://github.com/emillon/bulletml |
| eatfrog/bulletmllib4unity (40) | MIT [gh] | A BulletML runner for Unity. https://github.com/eatfrog/bulletmllib4unity |
| ivuecode/RadialBulletSpread (14) | MIT [gh] | A radial spread: N projectiles over an arc, with a step angle. https://github.com/ivuecode/RadialBulletSpread |
| Yecats/UnityBehaviorTreeVisualizer (196) | CC0 [gh] | Draws the running behaviour tree for debugging. Useful for a dev overlay that shows the boss's state. |

Every pattern has to be rewritten in Skygrind.Core: 120 Hz whole-step timers and Mulberry32. The repos above use
Unity's Time or Random, so take the structure only, never the code.

---

## The 12 most useful patterns for SKYGRIND

1. **The boss is the level (7-4).** You climb or ride the boss's body in sections, and each section is a damage gate.
   Best fit for the L4 grapple boss (hook points on its body).
2. **The arena changes at each phase gate (Leviathan phase 3).** The floor is destroyed and replaced by the boss's
   turning body, so phase 3 is a new movement puzzle.
3. **A deadly floor forces the walls (Mara).** The floor is lethal for a few seconds, so the player wall-rides the rim
   and then drops in to punish. Best fit for the wall-ride boss.
4. **The boss gets stuck on the scenery (Mara's consoles).** The player baits an attack into a wall or pylon, and the
   stuck limb is the weak point. Movement chooses where the window opens.
5. **A weak point that opens after a pattern (the Leviathan heart, x3 damage after the barrage).** Pair it with the
   KESTREL's charged pierce shot or the THUNDERCLAP slug.
6. **Projectiles you parry to get across (Neon Green).** The boss's yellow shots are both ammunition and stepping stones
   (parry or launch-punch them). This forces the parry verb.
7. **Chase the boss (Sonic Egg Dragoon / Egg Beetle).** The boss flees down a lane, and only a dash or a jump-dash gets
   you into range. Best fit for L5 (jump-dash + TORCH).
8. **Environmental damage (Golem vats).** Shoot a hazard so it drops on the boss. STITCHER nails stuck in a hazard
   detonate onto the boss.
9. **Boss stamina instead of boss health (Minos: 6 then 12 stamina, a 2 s lockout, +5 on parry).** A short fight with
   windows the player earns. This fits the 15-22 s bot / ~40 s human target.
10. **Telegraph, attack, recovery as data (Psychonauts maneuvers, BulletML).** Each attack is a config row (wind-up
    steps, attack, recovery steps), chained per phase, with every number from config.
11. **Colour and sound for each answer (ULTRAKILL).** Red-orange means dodge, yellow means parry, and cyan is the
    player's open window (palette rule 8). GG owns the sound, so the event names come first.
12. **Phase gates on short health, with event-driven aborts (Stout's eight beats + NPBehave Stops).** Two or three
    gates, each changing the arena or the attacks, with a midpoint rest; a health-threshold observer aborts the running
    maneuver and starts the transition. Anti-camping comes from the floor and pulses, never from more health.

# FORGE68 research: make the verb the faster line (parry, ride, launch, hazards)

2026-10-24. RESEARCH scout for FORGE68. Web only. Read with `docs/memory/ledger/wave-67.md` (what failed). Google Drive and
Linear were searched for related material and nothing turned up. No project data went into a query.

**Labels.** [F] = page fetched and read. [S] = search-engine snippet only; the page refused the fetch (403/402), so the
claim is not checked against the page. "Inference" = my reasoning with no source behind it.

**The problem, from w67 (ledger/wave-67.md).** Time ratio, walk bot vs verb pilot, then the bar it had to reach:
- Parry: g7 1.32 vs 1.39. Cover cannot separate the spit, the gun and the returned glob.
- Ride: g8 0.77 vs 1.25 (the pilot rode every face, 92 vs 55); kb3c 1.06 vs 1.25.
- Launch: v2 1.01 vs 1.25; kb3 1.02 vs 1.35; v6 0.96 vs 1.33 (about 21 launches per 10 seeds).
- Lava beside the fight: walk-bot deaths 19 / 13 (kb3), 15 / 12 (v6). Dry pits fixed the deaths but raised the stall.

## 0. What every source has in common

A verb wins on time only when at least one of these holds:
- **(a) It pays something no other action pays:** health, stamina, currency or a stagger.
- **(b) The other answer is weakened:** armour, resistance, AI that denies the rush, starved ammo.
- **(c) The geometry makes the non-verb route longer:** a gap, a detour, a target out of reach.
- **(d) The verb is a safe or fast state:** a speed boost, fire that misses you, a hazard that kills enemies but not you.

The w67 rooms mostly tried (c) alone, and only on the fighting floor. Nothing in them paid (a), weakened the gun (b), or
made the ride or the launch fast or safe (d). (Inference from the ledger lines above.)

## 1. Encounter rules that make a verb the answer

### Parry
- **ULTRAKILL, what a parry pays.** A parry "heals the player up to the Hard Damage limit" and "fully restores Stamina".
  Parrying your own shots shows the effect but heals nothing. A parried projectile is redirected, gets faster, explodes
  and does more damage. A **yellow** flash marks a parryable attack, a **blue** flash an unparryable one. Swordsmachines
  take 150% from shotgun parries; a melee parry kills a Mannequin outright. [F] https://ultrakill.wiki.gg/wiki/Parry
- **ULTRAKILL, Malicious Face.** A parry interrupts its beam, and "the explosion from the beam damages the Malicious Face
  itself"; parry plus a point-blank shotgun "will deal massive amounts of damage". It is immune to explosions unless
  gasoline strips the resistance. [F] https://ultrakill.wiki.gg/wiki/Malicious_Face
- **ULTRAKILL, Guttertank.** Its punch cannot be parried, but its mines can, and parried mines are "a valuable source of
  healing" because closing in to heal is dangerous. [F] https://ultrakill.wiki.gg/wiki/Guttertank
- **ULTRAKILL, Style.** A parry is worth 100 style. Every attack costs its weapon 0.7 "freshness" and switching restores
  it, so repeating one answer is taxed (FRESH 1.5x down to DULL 0.0x). [F] https://ultrakill.wiki.gg/wiki/Style
- **DOOM: The Dark Ages.** "only one color, green, is parryable"; red and blue attacks can only be blocked. A parried shot
  "will automatically adjust its course to fly back toward the demon that sent it to you", so no aiming is needed, and a
  single-attack melee enemy is staggered. [F] https://www.shacknews.com/article/144239/how-to-parry-doom-the-dark-ages
  Shield runes fire on a Hell-surge parry (Heaven Splitter: lightning, stuns nearby).
  [S] https://doomwiki.org/wiki/Rune_(Doom:_The_Dark_Ages)
- **Sekiro.** You "attack and deflect ... to drain the enemy's posture"; one enemy "is designed to teach you to stand
  your ground and realize deflection is also your best attack".
  [F] https://www.superjumpmagazine.com/the-art-and-science-of-sekiros-combat/
- **DOOM Eternal: deny the dominant answer.** "Many AI have now been tuned to no longer allow you to rush up to them ...
  You have to create openings"; "If one weapon is unbalanced, that takes away from everything else."
  [F] https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=4
  A grenade into a Cacodemon's open mouth leaves it open to a glory kill at once. [S] https://doomwiki.org/wiki/Destructible_demons
- **The Level Design Book.** "Players aren't using the gravity gun? Starve the player of ammo for all their other
  weapons, place physics props everywhere, and use numerous slow moving enemies."
  [F] https://book.leveldesignbook.com/process/combat/encounter
- **Rule for SKYGRIND (inference):** our gun is hitscan, so parry can only win if it pays heal or stamina, its return
  reaches the spitter where the gun cannot (homes to the sender, round cover), or the spitter shrugs off the gun but not
  its own glob.

### Ride (wall-run)
- **Titanfall's answer to "why ride".** "Why would you wall-run when you can just run around?" The answer was a speed
  boost: "Wall-running itself gives a boost, and jumping off a wall boosts still further" (about 30 mph, more than double
  sprint). Players "wouldn't trust the wall-run" until training sections and a forced early run. Runs starting near a
  wall's top arced over and dropped the player, which needed its own check; the view tilts on the jump toward the wall.
  [F] https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right
- **Titanfall 2 tech test.** Wall-running got faster, so players would "accumulate and retain more speed when chaining
  wall runs"; Respawn also fixed bugs that lost momentum mid-run.
  [S] https://gamingbolt.com/titanfall-2-dev-to-increase-wallrunning-speed-reintroduce-titan-timer
- **Titanfall, War Games map.** Coloured panels: "These are meant to be obvious paths to help encourage players that
  haven't embraced this new way of traversing maps ... we even used arrows to help guide players". They "sometimes
  stretch across long distances, above battling Titans below".
  [F] https://pcgamesn.com/theres-titanfall-dlc-map-set-simulated-world-smooth-clean-and-abstract-parkour-playground
  Respawn compared wall-run heatmaps for War Games and Rise. [F] https://www.slashgear.com/titanfall-dlc-map-detailed-war-games-for-parkour-24326398/
- **Severed Steel: the verb is a safe state.** "While Steel is doing a stunt, all incoming enemy fire misses", which
  "encourage[s] players to enjoy the movement system". Wall-run, slide, dive and flip are stunts.
  [F] https://blog.playstation.com/?p=347121
- **Neon White: separate the tasks, guarantee the shortcut.** Levels "never require players to do all three at the same
  time" (platforming, shooting, card management) because players "had a hard time juggling" them. "The player always
  has the right tool for the next encounter." "If you follow the obvious path, you won't get a great score." Fights are
  blocked out alone, then joined, and the joins create shortcuts.
  [F] https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity
  "Each level had at least one shortcut that saved a significant amount of time"; levels without one were reworked.
  [F] https://www.vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems
- **DOOM Eternal: fight in the transit too.** "The combat between arenas is awesome; it's as good as the arenas."
  [F] https://www.shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6

### Launch (knockback into hazards)
- **ULTRAKILL.** "Enemies dropped into Death Zones are instantly killed", even Blessed ones; dropping one down a
  bottomless pit scores FALL (80). [F] https://ultrakill.wiki.gg/wiki/Pit, https://ultrakill.wiki.gg/wiki/Style
- **Dead Cells.** "Lava instantly kills enemies"; almost every trap hurts both sides. [F] https://deadcells.wiki.gg/wiki/Hazards
- **Dark Messiah.** "the most effective way to kill an enemy is to kick that enemy over a cliff or into a fire"; spike
  walls are "everywhere". [F] https://www.gamedeveloper.com/game-platforms/analysis-paradoxes-in-i-dark-messiah-of-might-magic-i-
  The repetition cost: "until you've kicked your hundredth goblin into one of the many spike walls".
  [F] https://www.gamebanshee.com/8a39
- **Bulletstorm.** Skill-shot points "can be used to buy ammo and upgrades"; "environmental deaths are the easiest to
  get a high score from"; long-corridor levels limited experiments.
  [F] https://www.gamedeveloper.com/business/balls-and-everything-else-to-the-wall-action-a-bulletstorm-analysis
- **Hades.** Dodging late "can cause enemies to walk on top of a spike trap"; smacking them into pillars is "constantly
  satisfying". [F] https://www.pcgamesn.com/hades/hades-impressions Wall-slam damage scales with momentum, distance to
  the wall and biome. [S] https://hades.fandom.com/wiki/Unblockable
- **Dishonored.** Windblast knocks enemies off ledges and kills them even un-upgraded; Tier II slams them into walls
  "with lethal force". [S] https://dishonored.fandom.com/wiki/File:Windblast.jpg
- **Titanfall, Runoff: a non-lethal pit.** "If your Titan falls down into these trenches, you will have to travel back to
  either end of the map to find a ramp back up." [F] https://www.thesixthaxis.com/?p=218682
  This fits GG's rule that enemies always have a route to climb back (NIGHT20).

## 2. Where ride walls go

- **Ride walls link places; they are not the fighting surface.** War Games' panels run "above battling Titans below";
  Neon White keeps traversal and shooting apart and joins separate fights by routes (both above).
- **The wall must beat the floor.** A boost on the ride and a bigger one on the jump-off (Titanfall); speed kept across a
  chain, never bled mid-run (tech test). A ride that doesn't save time is a detour; Neon White reworked levels until each
  had a shortcut that saved significant time.
- **Make the wall obvious.** A colour and arrows on the panels (War Games). Frame the next location in view, never make
  the player stop, let light draw the player.
  [F] https://worldofleveldesign.com/categories/level_design_tutorials/flow-tips-mirrors-edge.php
- **Entries and exits.** Tilt the view on approach and keep run starts away from the top edge (Titanfall). Land the exit
  at the next fight (inference from "right tool for the next encounter").
- **Length.** No source gave a wall-run duration or length. Inference: length = ride speed × the time saved, and the
  floor route must be longer by more than that saving. Take both from our Mover metrics in a "metrics zoo"
  ([F] https://book.leveldesignbook.com/process/blockout/metrics), not by guessing.
- **Face count.** g8's pilot rode 92 faces against 55; Titanfall marks a few panels on purpose. Inference: fewer
  ridable faces, each one a route.

## 3. Hazards: when they serve the launch and when they only punish

- **Make hazards asymmetric.** ULTRAKILL pits deal a set damage and "teleport the player to the nearest safe ground",
  leaving the player "at 1 HP if the damage would otherwise kill them"; enemies die outright. Only a few Encore pits and
  fans kill the player. [F] https://ultrakill.wiki.gg/wiki/Pit Dead Cells pits "damage and teleport the player to the
  last platform", hazard damage "is capped at 30% of the player's HP per hit", and lava kills enemies outright.
  [F] https://deadcells.wiki.gg/wiki/Hazards
- **Not every ULTRAKILL hazard favours the player.** Lava: 50 to the player, 25 to enemies; acid: 10 and 0.5. The
  minefield (50) and the Sea of Souls (20 plus upward knockback) kill enemies. Each has its own cue: "bubbling hot orange
  liquid", "steam hissing out of the crusher before it activates", a 3 s lightning column before the strike.
  [F] https://ultrakill.wiki.gg/wiki/Pit
- **Readability is proven by playtest, in combat.** "I find playtests to be the only surefire way to verify whether
  something reads or not"; layouts are judged by "having to negotiate this kind of geometry during the heat of combat",
  with a camera-wiggle test; Skin Deep banned sloped walls for ambiguous clearance.
  [F] https://blendogames.com/news/post/2026-06-xx-leveldesign_readability/
  Portal went to white chambers because "playtesters found it hard to identify the key puzzle elements".
  [F] https://gmtk.substack.com/p/valves-secret-weapon
- **Showing depth.** A long shaft with fog (start 512, second fog 4000), or light blocks that darken toward the bottom.
  [F] https://github.com/BEEmod/BEE2-items/issues/150; [S] https://wiki.srb2.org/wiki/Death_pit
- **Gap: no source on a dry pit reading as a "black hole".** Inference: a lit rim in the boundary pale violet (rule 8)
  and a coloured fog gradient inside, so it reads as a drop and not a shadow; a motion cue like ULTRAKILL's hazards;
  visible feedback when an enemy falls in (ULTRAKILL's FALL).

## 4. Measuring whether a verb is "the better line"

- **Valve: a design is a hypothesis.** "We see our game designs as hypotheses and our playtests as experiments to
  validate these hypotheses." Valve tracks deaths ("players are routinely getting themselves killed" led to a change) and
  looks for "common trends", not outliers. [F] https://gmtk.substack.com/p/valves-secret-weapon
- **Neon White: noisy ghosts and par tiers.** Dev ghosts with "small mistakes and hesitations" added gave "estimated
  times that were surprisingly close to real-world player times". Silver = "intended path with no optimizations and some
  mistakes"; Gold = same, "no mistakes"; Ace = "the designed shortcut, with no optimizations and no mistakes".
  [F] https://www.vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems
- **ULTRAKILL: par time.** "S-rank time requirements are generally balanced by adding a certain amount of extra time to
  Hakita's average run." [F] https://ultrakill.wiki.gg/wiki/Ranking
- **Heatmaps.** Halo 3 turned "kills and deaths ... into heatmaps", filterable by weapon.
  [F] https://www.coolinfographics.com/blog/2009/1/12/halo-3-heatmaps.html Respawn used wall-run heatmaps (above).
- **Bots.**
  - Procedural personas (MCTS with evolved selection) "enact different play styles" as synthetic playtesters.
    [F] https://arxiv.org/abs/1802.06881
  - EA SEED's deep-RL agents "increase test coverage, find exploits, test map difficulty" in FPS testing.
    [F] https://arxiv.org/abs/2103.15819
  - Curiosity-driven agents learned "the complex navigation mechanics required to reach the different areas".
    [S] https://arxiv.org/pdf/2103.13798
- **For SKYGRIND (inference).** w67 already takes a time ratio, verb pilot vs walk bot. What it lacks: Neon White's noise
  on both bots; a per-room par from the pilot's average plus a margin (ULTRAKILL); a ride-face and launch-point heatmap
  (g8's "92 vs 55" is what a heatmap shows at a glance); a per-run count of what the verb paid (heal, points, stagger).

## 5. Recommendations: five per verb, each one testable

New gameplay numbers go in a Unity-side config file (CLAUDE.md rule 5). Each "Test" is a proposal for the w67
instrument (time ratio on the same seeds plus guards); thresholds are not set here.

### Parry (g7-type rooms)
1. **The parry pays heal and stamina, which the gun can't** (ULTRAKILL Parry; Guttertank mines as healing).
   *Test:* HP at room end and deaths, parry pilot vs gun-only pilot; the time ratio is the MAIN.
2. **The returned glob homes to its sender** (Dark Ages). This attacks w67's cover finding. *Test:* returned-glob hit
   rate on spitters behind cover vs the gun's hit rate on the same spitters.
3. **Spitters resist the gun but not their own glob:** a self-hit multiplier (Malicious Face self-damage, Swordsmachine
   150%, Mannequin instakill; LD Book's "starve the other answer"). *Test:* time-to-kill per spitter, gun vs glob.
4. **One readable cue on parryable shots, a different one on the rest** (ULTRAKILL yellow/blue, Dark Ages green). Rule 8
   makes cyan player-usable, so the hue is GG's call. *Test:* a blind critic sorts parryable from unparryable shots in
   paired frames.
5. **One threat only the parry stops:** a beam or volley the gun can't interrupt, ended by a parry that hurts the source
   (Malicious Face), plus AI that denies the rush (DOOM Eternal). *Test:* damage taken and clear time, gun-only vs parry
   pilot; the gun-only run prints that it was refused the interrupt (L-104).

### Ride (g8 / kb3c-type rooms)
1. **Riding beats running: a boost on the ride, a larger one on the jump-off** (Titanfall). Needs GG / Mover sign-off
   (rule 3: change Mover.cs only when a task names it). *Test:* exit vs entry speed in the sim; ride time vs floor detour.
2. **Ride walls cross gaps between fight pockets; none stand on the fighting floor,** and the floor route detours (Neon
   White's shortcut; War Games panels above the fight). *Test:* floor path over ride path length, plus the walk bot's time.
3. **Never ask for a ride and a fight at the same moment** (Neon White). Alternative: make the ride safe as in Severed
   Steel, a rule change for GG. *Test:* damage taken while riding; kills in the 1-2 s after leaving a wall.
4. **Fewer, marked faces: one colour and an arrow-like cue on route walls only** (War Games; Mirror's Edge framing).
   *Test:* ride-face heatmap; the pilot rides the marked faces, not 92.
5. **A reason at the top of every ride: a ledge enemy, a pickup or the exit, reachable only by riding** ("right tool for
   the next encounter"; DOOM Eternal's fights between arenas). *Test:* share of ledge targets killed near a ride exit;
   the walk bot's time to reach them.

### Launch (v2 / kb3 / v6-type rooms) and hazards
1. **Pits kill enemies and only return the player:** set damage, back to safe ground, never lethal (ULTRAKILL Death
   Zones, Dead Cells). No lava where the player can be knocked on the fighting floor; if lava stays, cap its damage per
   hit (Dead Cells 30%). *Test:* walk-bot hazard deaths fall to 0; launch kills per run.
2. **Enemy lanes hug the pit edge:** spawns and approaches cross bridges or ledges over the drop (Dark Messiah's drops;
   Hades' "walk onto the trap"). *Test:* median enemy-to-edge distance at launch; share of launches ending in a pit kill.
3. **A launch kill pays points, the round currency** (Bulletstorm skill shots buy ammo and upgrades; ULTRAKILL FALL +80).
   *Test:* points per minute, verb pilot vs walk bot, same seeds.
4. **Starve the gun in launch rooms:** heavier, slower enemies, so one launch is worth N seconds of shooting (LD Book,
   Ravenholm). *Test:* time-to-kill per enemy, gun vs launch, logged in the room's run.
5. **Rooms with no pit still pay the launch:** a wall slam that does momentum damage (Hades, Dishonored), or a non-lethal
   trench with a long climb out (Titanfall's Runoff; GG's NIGHT20 climb rule). Pits get a lit violet rim, fog depth and a
   fall cue (section 3, inference). *Test:* time a launched enemy is out of the fight; a fresh critic's blind read of the pit.

## Limits
- No source gave wall-run lengths or durations, a parry window in ms, or a pit-readability study.
- [S] claims are unchecked because the pages refused the fetch.
- Some queries named public game developers (interview searches); nothing personal or project-specific was sent.

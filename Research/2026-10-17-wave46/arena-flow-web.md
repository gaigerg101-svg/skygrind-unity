# Combat arena layout and flow: web research for wave 46

Date: 2026-10-17. Scope: how arenas are laid out and how a player moves through a fight, for a ULTRAKILL-style movement
shooter with CoD-Zombies waves in small (~30 x 30 m) arenas with grind rails, grapple points and lava.

## How this was gathered (read this first)

- **Primary transcripts (read in full):** the GDC 2018 talk "Embracing Push Forward Combat in DOOM" (Kurt Loudy, Jake
  Campbell; auto-captions pulled with yt-dlp, 49 min); GMTK "The 4 'P's of DOOM's Amazing Combat" (auto-captions);
  GDC 2018 "Designing Unforgettable Titanfall Single Player Levels with Action Blocks" (Christopher Dionne;
  captions, searched by keyword and two sections read); two "What Makes This Map Great" episodes (Q3DM6, Q3DM17).
  Auto-captions mishear names ("f-22 Ferrari", "doom rain" = Doom Marine); quotes below are fixed only where the meaning
  is plain.
- **Primary slides (text extracted with pdftotext):** Butcher and Griesemer, "The Illusion of Intelligence" (GDC 2002);
  Damian Isla, "Building a Better Battle: Halo 3 AI Objectives" (GDC 2008).
- **Read through a summarising fetch** (so wording is the fetch model's paraphrase unless in quotes): The Level Design
  Book pages, Andrew Yoder's "Door Problem", the HL2 Episode Two developer commentary transcript (first 100k of 118k
  characters), Hakita interviews, Game Informer on Neon White, the ULTRAKILL wiki, Xbox Wire on Sunset Overdrive,
  Jokey's Quake guide, World of Level Design on Mirror's Edge, Game Developer pieces.
- **Search-snippet only** (not opened, or opened and empty): see "Not opened" at the end.
- "SKYGRIND mapping" lines are my synthesis, not the source's claim. Metric conversions are mine and approximate.

---

## Part A: findings (28)

### A1. Size and shape

**F1. A right-sized arena makes the player feel faster than a big one.**
- Rule: in DOOM 2016 "your acceleration and your agility actually exceeds your top speed, so in order to feel fast ... it
  comes from the speed at which you pass geo, change direction and weave between other monsters." They fought "a
  misconception that epic fights required epically sized arenas."
- Source: Loudy and Campbell, "Embracing Push Forward Combat in DOOM", GDC 2018, 06:57-07:29 and 23:24-23:56.
  https://www.youtube.com/watch?v=2KQNpQD8Ayo (Vault: https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in)
- SKYGRIND mapping: a 30 x 30 m box is the right instinct. Speed is felt as geometry passing close by, so rails, pillars
  and the grapple target should pass within a few metres of the player's path, not sit at the far wall. Fill the box
  with near-field geometry rather than enlarging it.

**F2. Circles (and other fully symmetric, single-loop shapes) failed in DOOM.**
- Rule: "we tried all sorts of layouts but circles were one that just never panned out for us. It's a symmetrical
  shape which makes options unclear, has limited lateral movement, and AI often take the same route, resulting in the
  player and the AI just chasing after each other. Racing games have to solve landmarking at 200 miles an hour and they
  realize the power of asymmetry."
- Source: GDC 2018 DOOM talk, 07:29-08:00 (same URL).
- Counterpoint: GMTK's analysed Doom Eternal fight (Nekravol II, first arena) is "a pretty large and totally
  symmetrical arena" (GMTK 00:01:21). Symmetry of the floor plan is not the failure; a single round track with no
  distinguishable sides is.
- SKYGRIND mapping: never make the arena a ring with a pillar in the middle and nothing else. Give each side a
  different job (a rail side, a grapple side, a lava side) so "left or right" is a real choice.

**F3. The "combat bowl" (small open area, full cover in the middle) produces circling; break it on purpose.**
- Rule: "Ring Around The Rosie / Combat Bowl": a "Small open area with cover in the middle", usually a pillar, which
  "produces a rotating flow". To change it: "add a one-way drop to force a specific flow, or add verticality."
  Arena / POI: "an open area with several lanes of circulation and some cover in the middle." Single-player arenas
  "usually need gating": the exit so players can't leave early, the entrance so players can't pull all enemies into a
  chokepoint.
- Source: The Level Design Book, "Typology". https://book.leveldesignbook.com/process/layout/typology
- SKYGRIND mapping: a 30 x 30 m wave arena is a combat bowl by default. Add one height break (a drop-off ledge or a
  rail that leaves the ground loop) so the circling has an exit and an entry, not just a radius.

**F4. Three floor planes is the practical maximum.**
- Rule: most areas "max-out at three different floor planes" (bottom, middle, top); "Don't try to hold 10 different
  distinct overlapping layers of floorplans in your mind." Q3DM6 has "three floors in total" (Grit Gaming, 01:38).
- Source: The Level Design Book, "Verticality".
  https://book.leveldesignbook.com/process/layout/flow/verticality
- SKYGRIND mapping: floor, a mid tier (ledges, rail height), a top tier (grapple perches / the landmark top). A fourth
  height (a pit under the floor) only as a hazard, not a play floor.

**F5. Gamepad players need gentler height and fewer 180-degree surprises.**
- Rule: gamepad aiming is slower, so console levels use "wide open landscapes with shallow slopes" and "relatively flat"
  floor planes; backspawning "feels especially unfair because players can't turn 180 degrees quickly."
- Source: The Level Design Book, "Verticality" (same URL).
- SKYGRIND mapping: SKYGRIND is pad-first (Switch). Keep the three tiers but keep the main threats in front of the
  player's probable facing (see F15) and avoid sheer tier walls that force a look straight up.

### A2. Routes, loops, flow

**F6. Offer two clear options at each moment, not four or five.**
- Rule: "MP map design hinges on repeated traversal of a space with clear navigation options. Humans are excellent at
  making split-second binary decisions, but when you had four or five options it starts to get dicey: players start
  evaluating the pros and cons of each route on the fly."
- Source: GDC 2018 DOOM talk, 07:29 (same URL). Supporting: most maps use one to three lanes; too many lanes "or no
  clear hierarchy makes a map feel like a maze" (Level Design Book, "Circulation",
  https://book.leveldesignbook.com/process/layout/flow/circulation).
- SKYGRIND mapping: from any landing point the player should see exactly two outs: e.g. "grind the rail" or "dash to
  the ledge". A node with five exits reads as clutter at speed.

**F7. Give the arena separate ground and air routes, and let the air route link the high tiers without touching the
floor.**
- Rule: in the Nekravol II arena "the arena's floor plan contains a number of distinct pathways both on ground and in
  the air. In fact these monkey bars allow you to move between the three most elevated areas without ever touching the
  ground", "picking between different routes depending on where you need to go."
- Source: GMTK, "The 4 'P's of DOOM's Amazing Combat" (2020), 07:47-08:25. https://www.youtube.com/watch?v=I9ZsFT_eqXY
- SKYGRIND mapping: this is the single most direct model for grind rails. The rail network should be the air route
  that strings the high perches together, with the floor as the second, slower route underneath. Lava below makes the
  air route the attractive one without forcing it.

**F8. No dead ends; a camping spot is a design bug.**
- Rule: Andrew Yoder's value diagrams: "Dead ends and killzones repel players by limiting their ability to move and
  shoot." Q3DM6 is "extremely hard to camp ... so many connecting halls ... if you camped, you'd miss out on valuable
  pickups" (Grit Gaming, 23:48). Jokey: a strong camping spot over a key weapon lets someone "stay there for the
  duration of the level."
- Sources: Yoder, "The Door Problem of Combat Design" (2019),
  https://andrewyoderdesign.blog/2019/08/04/the-door-problem-of-combat-design/ ; "What Makes This Map Great Ep.33:
  Q3DM6", https://www.youtube.com/watch?v=TBD5R2ww_vU ; Jokey's Guide to Quake Level Design,
  https://dondeq2.com/2017/07/29/jokeys-guide-to-quake-level-design/
- SKYGRIND mapping: every perch needs a second way off (a rail end, a drop, a grapple out). A perch with one way on and
  one way off is a camping spot for a wave mode; the Zombies "training" loop then collapses into "stand on the box".

**F9. Arena geometry should support a constant lateral and circular "dance".**
- Rule: the DOOM team's nickname for the combat loop was "the dance": "you're always moving, there's a lot of lateral
  and circular motion, and there's a natural back-and-forth between you and your partners, the demons." The arenas are
  "skate parks ... that the player improvises and dances in."
- Source: GDC 2018 DOOM talk, 24:48-25:22.
- SKYGRIND mapping: test a layout by whether a bot or a player can strafe-circle the whole fight without stopping.
  Any spot where the circle must reverse (a wall in the strafe path, a lava pool across the loop with no crossing) is
  a stop.

**F10. Make the curves smooth and the traversables interconnected so momentum carries.**
- Rule: Sunset Overdrive rebuilt its city around constant forward movement with "more organic shapes and fewer hard
  right angles", and linked traversable objects "so players could move from one to the next while keeping their
  speed." Grinding began when a designer threw telephone wires into a level and asked to grind them. The goal was
  "We wanted the player to look at where they want to go, and then figure out how to get there on their own."
  (Cameron Christian).
- Source: Xbox Wire, 2014-11-21, https://news.xbox.com/en-us/2014/11/21/game-sunset-overdrive-24-hour-trial/amp/
- SKYGRIND mapping: every rail end should land the player either on another rail start, a wall to wall-jump, a grapple
  point in view, or open floor. A rail that ends in a wall face is a dead end in the air.

**F11. "Bad" flow is a tool, but only where you want a stop.**
- Rule: "less efficient flow is not necessarily bad"; dust2's doors "force sharp, cautious turns", which adds drama.
  Arcade games suit "wide, smooth turns"; horror suits narrow turns, blind intersections, dead ends.
- Source: The Level Design Book, "Flow". https://book.leveldesignbook.com/process/layout/flow
- SKYGRIND mapping: SKYGRIND is arcade; reserve sharp turns and narrow choke for the arena's entry and exit (the gates),
  never inside the fight.

### A3. Height and cover

**F12. Half walls are the best cover for a movement shooter: they block shots but reward a jump.**
- Rule: DOOM has "very few props"; "combat is designed for medium range; creative line of sight blockers are crucial.
  Half walls were actually our secret weapon since they're impossible to miss: a single jump will hit it and mantle
  over, while a well-timed double jump lets you clear it cleanly." Their movement test map contained half walls and "a
  full double jump gap".
- Source: GDC 2018 DOOM talk, 06:26-08:00.
- SKYGRIND mapping: cover in the arena should be jump-height blocks, not tall walls: one jump mantles, a dash-jump
  clears. Derive the height from SKYGRIND's own jump arc (config numbers, not typed by hand).

**F13. The arena has to protect a player who is fighting in the open from all 360 degrees.**
- Rule: "we don't have a front. It's 360-degree combat ... we had more in common with MP map design than single-player
  ... they can attack from any direction and they have the same accuracy on and off screen, so it's important that the
  arena can actually protect the player." Hard / soft / half / full cover: for PvE "err on too much cover".
- Sources: GDC 2018 DOOM talk, 05:54-06:26; The Level Design Book, "Balance",
  https://book.leveldesignbook.com/process/combat/balance
- SKYGRIND mapping: with waves spawning from several sides, each quadrant of the 30 x 30 m box needs at least one line
  of sight blocker within a dash of the strafe loop.

**F14. Height is power; balance every perch with exposure and an easy way off.**
- Rule: a higher attacker beats half cover; a position is strong when it has height and approaches that are hard to
  make unseen. Q3DM17's railgun island is reached by one jump pad, so it is defensible, "but the risk? Everyone can see
  you. There is no cover", and "two jump pads provide a quick option to get off the island." Q3DM6's balcony to the
  railgun is "very susceptible to rocket splash damage, but the risk is so worth it."
- Sources: Level Design Book "Balance"; Grit Gaming Ep.29 (Q3DM17) 13:46-15:35, https://www.youtube.com/watch?v=OsPO3rw39-4 ;
  Ep.33 (Q3DM6) 11:31-12:20.
- SKYGRIND mapping: grapple perches should be open (no cover on top) and have a fast way down (rail start or drop), so
  they are a place to pass through and fire from, not to live on. The enemy climb routes GG asked for ("a route that
  enemies can climb up") are the pressure that makes perches temporary.

### A4. Enemy placement and push-forward

**F15. Enemies hold their positions; the player has to go to them.**
- Rule: "the secret to selling this fantasy is having enemies hold their positions so it's on the player to rush
  forward and engage them; limit the amount of active attackers so the player isn't overwhelmed." Ranged units "maintain
  some distance if possible" because "we want the player to be the one bringing the fight to the enemies." Ranged AI
  stand at "exposed cover": positions picked for being in the open, with visibility to the player.
- Source: GDC 2018 DOOM talk, 10:48-11:19 and 37:31-38:41. Also 80.lv summary of "Cyber Demons: The AI of DOOM",
  https://80.lv/articles/cyber-demons-the-ai-of-doom
- SKYGRIND mapping: place ranged enemies on the far tiers and perches (visible, in the open), not at the player's entry.
  Melee fodder comes to you; ranged units are the reason to cross the arena and ride the rails.

**F16. The four failure symptoms of an arena: retreat, doorway, sniping, kiting.**
- Rule: "we identified a number of fail conditions for combat chess that we could test: players retreat from enemies,
  players post up in a doorway, players snipe from a distance, and worse, players kite enemies around the space."
  Martin (Doom Eternal): "We use our chess pieces to move the player around and get them off those ledges and don't let
  them kite enemies" (snippet via search, Noclip interview not opened).
- Source: GDC 2018 DOOM talk, 23:56-24:48.
- SKYGRIND mapping: these four are directly measurable by the bot and the Player logs: time spent within N m of the
  entry, time spent on one perch, mean engagement distance, and time with every enemy behind the player. Note the
  tension with CoD Zombies, where "training" (kiting a horde in a loop) is the core skill: decide whether kiting is
  allowed fodder behaviour and punished only for heavies.

**F17. Fix the doorway problem with a foothold, hidden information and leashed enemies, not a wider door.**
- Rule: widening the threshold doesn't help because "there is nothing about the arena that draws the player in, nor
  anything about the hall that pushes the player out." Solutions: a foothold of cover inside the arena; rewards placed in
  the arena; partitioning so no single position sees everything; AI leashing (enemies on territory they can't leave, e.g.
  behind height); one-way paths (a drop-in). They work best combined.
- Source: Yoder, "The Door Problem of Combat Design" (same URL above).
- Also: let the player "survey the arena before the fight actually begins", otherwise "the player will just fight
  unimaginatively from the doorway without committing"; a vista with a one-way entry ("Once the player drops down into the
  arena, they are committed"). Level Design Book, "Encounter", https://book.leveldesignbook.com/process/combat/encounter
- SKYGRIND mapping: enter each arena from a high lip with a one-way drop (or a rail that dumps you in), with the first
  wave visible from the lip and the first foothold 5-10 m inside, not at the door.

**F18. Spawn timing: trigger reinforcements on heavy HP %, and end the fight on "last heavy standing".**
- Rule: DOOM arena fights are "a steady stream of AI reinforcements so the encounter never lost momentum. We targeted
  five minutes and three waves for big encounters." "10 max minions felt good, with 20 being our cap for a big
  encounter." "Encounters hinge around heavies": reinforcements trigger off a heavy's HP percentage, "as high as 50%",
  instead of death; "players that went straight for the fiercest combatants could actually end a scenario sooner";
  when the last heavy dies "we actually zerg rush all the remaining fodder to clean up". Fodder "were never introduced
  later than mid fight". "Encounter should only have one captain per wave."
- Source: GDC 2018 DOOM talk, 20:18-23:24.
- Supporting: GMTK shows the same in Eternal: phase changes fire when the Baron reaches about a third HP, the Pain
  Elemental about a third, the Doom Hunter about half (GMTK 02:44-03:23); fodder zombies "infinitely respawn during the
  fight" as resource pinatas (07:07).
- SKYGRIND mapping: for CoD-style rounds, keep a round's budget but gate each new spawn group on the current captain's
  HP, not on a body count. That rewards pushing the big enemy and keeps 10-ish bodies alive at once.

**F19. Choose spawn pairs that don't stack pressure: never two chasers at once.**
- Rule: "you're never allowed to pair a charger such as the Pinky with a chaser like the Hell Knight, and we always had
  to have a cool-down between melee unit spawns." "If you could move two or three pieces at the same time of chess it
  would ruin the game." Use few enemy types at once: 2 is the "regular" relationship, 3 complex, 5+ "chaotic slaughter".
- Sources: GDC 2018 DOOM talk, 22:23-22:54 and 23:56; Level Design Book, "Encounter".
- SKYGRIND mapping: a round's spawn table should carry a rule "at most one fast melee pursuer type active", with a
  stagger between melee spawns. This is a spawn rule, so its numbers go in the Unity-side config (CLAUDE.md rule 5).

**F20. Where you put an enemy decides how strong it is.**
- Rule: DOOM's "fat guy in a hallway": "the AI appears more formidable with the snug fit, but really the only thing
  that's changed is the player doesn't have the same movement options." ULTRAKILL Prelude encore: a Virtue placed
  "right in front of the player without anything else to really take immediate attention away from it ... can't get the
  chance to actually be strong"; the others are "hidden away in some far corner, spawn in behind you, or have another
  threat present". ULTRAKILL introduces ranged Strays "at the opposite ends of a room so that you're more likely to
  actually get a chance to see what they do". Hakita: tuning is often "removing one tricky enemy or placing one in the
  right spot at the right time".
- Sources: GDC 2018 DOOM talk, 23:24; Professor Sock, "ULTRATOUR /// PRELUDE (an Ultrakill level design analysis, part 0)",
  https://www.youtube.com/watch?v=sVV4Nej7YlU (05:11, 46:54); Hakita, Into the Blue Sky follow-up interview (2021),
  https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- SKYGRIND mapping: spawn points per tier, chosen by role: support/ranged on the far perch or behind a rail line, melee on
  the floor near lava edges, the "captain" where the player has to cross the arena to reach it.

**F21. Territory with two fallback lines makes an enemy group read as smart.**
- Rule: Halo 3's canonical encounter: "Enemies occupy a territory. Pushed to 'fallback' point. Pushed to 'last-stand'
  point. Player 'breaks' them. Player finishes them off. ... plus a little 'spice': snipers, turrets, dropships." The task
  thresholds in the slide example are "forward >75%", "fallback >50%", then "laststand" (squad strength alive).
  "Designer provides the strategic intelligence; AI acts smart within the confines of the plan." Design owns the
  "3 minute scope", code owns the "30 second scope". Griesemer: "a 3-second loop inside of a 30-second loop inside of a
  3-minute loop that is always different."
- Sources: Isla, "Building a Better Battle" (GDC 2008) slides, https://web.cs.wpi.edu/~rich/courses/imgd4000-d09/lectures/halo3.pdf ;
  Butcher and Griesemer, "The Illusion of Intelligence" (GDC 2002) slides,
  https://www.jmeiners.com/shamans/papers/ai/the_illusion_of_intelligence.pdf ; Engadget interview with Griesemer (2011),
  https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html
- SKYGRIND mapping: for a 30 x 30 m arena the "territory" can be a tier: a ranged group holds the top perch at >75%,
  drops to the mid tier at >50%, last-stands on the floor. That makes the player climb at the start of a round and fight
  low at the end of it.

### A5. Resources and pickups steering movement

**F22. Resources come out of enemies, so healing means moving toward danger.**
- Rule: "if you're not moving you're dead ... the player restores health by engaging in combat not abstaining from it."
  Health drops "rubber band" with low HP, and "heavy characters actually do a lot more rewards to incentivize tackling
  the most challenging foe while you're in dire straits." Glory kills "always drop something even if you're topped off."
  After the chainsaw became the ammo pinata "we then lowered drop rates, started removing ammo crates from the game".
  Last-heavy-standing "also curtailed players from disengaging to scour the arena for pickups."
- Source: GDC 2018 DOOM talk, 02:15, 13:34-16:12, 20:47.
- SKYGRIND mapping: SKYGRIND already has no passive in-round regen (GG, "Never again" list). The arena should therefore
  carry very few fixed health pickups; health should come from kills, so the place to heal is wherever the enemies are.

**F23. The few fixed pickups are bait: put them where the player should go next, and make the best ones risky.**
- Rule: Doom chainsaw fuel became "a rare currency ... something there was an enticing reward for level designers to
  place around the maps." Doom Eternal arenas have "lots of items scattered around ... but there's only a few and once
  they're gone they're gone" (GMTK 06:27). Quake: high-value items in exposed spots (Q3DM17's red armour "is placed in a
  very open and vulnerable location"; the quad on the top platform via a two-stage jump pad); Jokey: don't place the quad
  "in a position that would enable a player to easily wait for it to re-appear", and spread items so one area doesn't
  become the hot spot. Level Design Book: collectibles visible from far act as "weenies".
- Sources: GDC 2018 DOOM talk 16:12; GMTK; Grit Gaming Ep.29; Jokey's guide; Level Design Book "Wayfinding",
  https://book.leveldesignbook.com/process/blockout/wayfinding
- SKYGRIND mapping: put the one big pickup of a round (an overdrive, an ammo refill, a weapon box) at a grapple perch or
  the far end of a rail, in the open, so collecting it is a crossing under fire; never at the entry or in a corner.

### A6. Pacing inside an arena

**F24. Five minutes is the ceiling for a big fight; vary the beats around it.**
- Rule: DOOM targeted "five minutes and three waves for big encounters"; GMTK's analysed Eternal fight is a single
  "five-minute fight" in four phases. The Level Design Book: "Any intense boss fight will feel like a slog after 10+
  minutes"; use downtime "as a contrast and palette cleanser, otherwise the player will simply go numb." Hakita spaces hard
  fights with simpler, larger fights "plus occasional light encounters as a breather" and avoids "forcing players to
  wait".
- Sources: GDC 2018 DOOM talk 20:18; GMTK 00:01; Level Design Book "Pacing",
  https://book.leveldesignbook.com/process/preproduction/pacing ; Hakita 2021 interview.
- SKYGRIND mapping: a CoD-style round in a small arena should be well under five minutes at early rounds, with the
  between-round break as the "breather". GG's boss band ("one to two minutes, maybe three") sits inside this.

**F25. A state change mid-fight works only if the player can still read the space.**
- Rule: Titanfall 2 spent "weeks" on a town arena where "floors drop out" and buildings rise mid-fight to open flanks. It
  "didn't add anything to the fight"; bigger versions were "too challenging"; "once you get underneath you didn't
  understand what was going on up above"; "a little bit too random, there wasn't enough reward". It was cut and became a
  scripted intro. Counter-example: Doom Eternal's Cultist Base arena "where the floor falls down and you fight in a tight
  arena ... while the floor slowly fills with blood" (JP's Value Judgment, 01:42:39; secondary). ULTRAKILL's Cyber Grind
  changes the layout only between waves.
- Sources: Dionne, GDC 2018 Titanfall action blocks talk, 37:31-40:21, https://www.youtube.com/watch?v=CkHGuHd9BgU ;
  JP's Value Judgment, "Doom Eternal In-Depth Analysis/Retrospective", https://www.youtube.com/watch?v=fXTJpdVgxUE ;
  ULTRAKILL wiki, "The Cyber Grind", https://ultrakill.wiki.gg/wiki/The_Cyber_Grind
- SKYGRIND mapping: change lava or rails between rounds, or as one telegraphed one-way event (the arena shrinks); don't
  re-shape the floor continuously during a round.

### A7. Readability

**F26. Clean, simple geometry is what lets the player move fast.**
- Rule: Hakita: "visuals are very easy to read at a glance. The player can move at a high speed with no issues since the
  geometry of a level is simple and clean"; "The levels are linear to make sure the fast pace is kept up and the player
  doesn't get lost or bored." DOOM: "the feeling of skilled movement trumps any sort of environmental storytelling." Neon
  White: "Your first time through needs to feel really clear"; removing ambiguity "was the main work of building levels";
  "every card has to have an obvious purpose in the moment that you pick it up." Jokey: "Each room should be defined by
  what is contained within it."
- Sources: 80.lv, "ULTRAKILL devs on the game's mechanics", https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ ;
  GDC 2018 DOOM talk 06:26; Game Informer, "How A Neon White Level Is Made" (2022),
  https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made ; Jokey's guide.
- SKYGRIND mapping: this is the answer to GG's "random lava pits random pillars that serve no purpose": each pillar is
  a named thing (cover, wall-jump post, rail anchor), each lava pool has a job (cuts a loop, guards a pickup), and
  anything that does neither is removed.

**F27. Colour-code what the player can use, consistently, and keep the next target in view.**
- Rule: Mirror's Edge Runner Vision recolours usable objects red; designers tune its strength, fade-in speed and fade-in
  distance (Owen O'Brien). Elisabetta Silli later said a sequel should make the path clearer from the start instead of
  "relying on the red flashes". Doom Eternal climbable walls are pale with fine creases; swing bars are yellow (guides and
  reviews; no id rationale found). ULTRAKILL hookpoints are colour-typed: green pulls and releases on contact, blue pulls
  and launches past, yellow suspends and triggers a linked object. Mirror's Edge critique: "Placing Where the Player Needs
  to Go Next, Outside Their View" is the mistake; "having to stop mid-run to look around destroys the level's pacing";
  "As the player enters into a new area ... give an indication where they should head to."
- Sources: Game Developer, "Living On The Edge: DICE's Owen O'Brien Speaks",
  https://www.gamedeveloper.com/design/living-on-the-edge-dice-s-owen-o-brien-speaks ; Engadget, "Overheard@GDC: Seeing
  red in Mirror's Edge", https://www.engadget.com/2010-08-17-overheard-gdc-mirrors-edges-odd-go-here-color.html ;
  ULTRAKILL wiki "Whiplash", https://ultrakill.wiki.gg/wiki/Whiplash ; World of Level Design, "One Mistake in Mirror's
  Edge", https://worldofleveldesign.com/categories/level_design_tutorials/one-mistake-in-mirrors-edge.php ; Doom Eternal
  wall look: thegamer.com guide (snippet only).
- SKYGRIND mapping: CLAUDE.md already reserves cyan for player-usable. Rails, grapple points and wall-jump surfaces in
  cyan, lava and enemy telegraphs in red-orange, and if grapple points behave differently, one colour per behaviour like
  ULTRAKILL's. From each landing spot the next rail or grapple point should be within the forward view.

**F28. Landmarks, light and contrast tell the player where they are at speed.**
- Rule: players "don't look upwards unless something draws their eye", "look in the direction they are moving", and
  "focus on contrast (in color, shape, lighting, and movement)." "Big important exits should have more important looking
  lighting"; "strong spotlight draws attention to enemy silhouettes"; lighting can "foreshadow encounters (enemy approach,
  battle line, possible strategies and flanks)". Q3DM6 gives each floor its own floor texture (brown stone / threaded
  metal / pale stone) so the player knows which tier they are on (Grit Gaming 09:44, 14:52). HL2 Episode Two replaced
  compass directions with landmarks (White Forest Peak, the cranes, the water tower) because "Many players had a hard time
  orienting themselves"; a compound building is "bright and eye-catching, with the intention of drawing the player there
  first."
- Sources: Level Design Book "Wayfinding" and "Lighting", https://book.leveldesignbook.com/process/blockout/lighting ;
  Grit Gaming Ep.33; HL2 Episode Two developer commentary nodes 234 and 112 (transcript),
  https://combineoverwiki.net/wiki/Developer_commentary/Half-Life_2:_Episode_Two
- SKYGRIND mapping: one tall landmark per arena (visible from every tier) to anchor orientation; a different floor
  material per tier; key perches lit brighter than the walls. Racing-game landmarking (F2) applies: the two sides of the
  arena must look different.

---

## Part B: model arenas

### B1. Doom Eternal, Nekravol part II, first arena (the GMTK fight)
- **Shape:** "a pretty large and totally symmetrical arena with different levels of elevation including a raised platform
  at the bottom and two areas on the sides which are accessible via ramps or acrobatic bars." Built around a central soul
  fountain. "Lower down you'll find pools of toxic soul juice which hurt pretty bad if you tumble in."
- **Tiers:** floor with hazard pools, a raised platform, two raised side areas; three "most elevated areas".
- **Loops:** ground pathways plus an air route: the monkey bars "allow you to move between the three most elevated areas
  without ever touching the ground."
- **Flow:** four phases. Phase 1: 2 Imps, a Mancubus, a Revenant, zombies. Phase 2 (after a handful of kills): 2 Imps at
  the bottom, 4 Gargoyles at the top, then a Baron of Hell. Phase 3 when the Baron is at about a third HP: a Pain
  Elemental; at its third, 2 Whiplashes. Phase 4 on a kill: a Doom Hunter; at its half, 2 Prowlers. Zombies respawn all
  fight as resource pinatas. Pickups (ammo, armour, chainsaw fuel, an overdrive) are few: "once they're gone they're gone."
- **Why it's memorable:** "almost half of the entire Doom Eternal cast" in one five-minute fight with a "predictable rhythm
  across all of your restarts" that is "never going to be exactly the same each time."
- **Use for SKYGRIND:** the closest template for one wave arena: hazard on the low floor, three high perches strung by an
  air route (rails instead of bars), phase spawns keyed to a captain's HP, fodder that keeps respawning as healing.
- Source: GMTK, https://www.youtube.com/watch?v=I9ZsFT_eqXY (00:42-03:23, 07:47-08:25). No dimensions given.

### B2. Quake III Arena, Q3DM6 "The Camping Grounds" (Tim Willits, Christian Antkow)
- **Shape:** a central hub (the "pentagram room") with the quad and "a long jump pad that leads to the rocket launcher";
  from it "many branching paths that lead to wide and tight corridors all around it." The Quake wiki snippet: two wings
  around the middle bounce-pad area. A large map for 6-8 players.
- **Tiers:** three floors, each with its own floor texture. The "pillars" room spans all three: tall pillars give
  tight-quarters fighting on the ground and "vertical cover from enemies above"; a jump pad from the first floor goes
  "straight up to the red armor and mega health"; pickups sit on the pillar tops for a careful jump line.
- **Loops:** every major room has several ways in; the outer halls curve so the bend itself is cover; the "bridge" is
  cover in the vertical plane ("cover options don't have to just be on the ground"); the railgun walkway is a long
  sightline paid for with splash exposure.
- **Flow:** item control: the red armour + mega health room is the one to hold, but it is reached by a contested jump pad
  and a rocket jump; the hub's quad is visible from "so many ways to access this spot."
- **Why it's memorable:** "interconnected and overlapping spaces"; "you can't really camp it out"; the three floors "wrap
  around each other".
- **Use for SKYGRIND:** jump-pad-to-prize as the model for "rail or grapple to the reward perch"; per-tier materials;
  pillars that are cover and a jump route at once.
- Source: Grit Gaming, https://www.youtube.com/watch?v=TBD5R2ww_vU ; Quake wiki page (snippet only, page returned 402).

### B3. Quake III Arena, Q3DM17 "The Longest Yard"
- **Shape:** "mostly a mirrored map" of floating platforms in a void with "very little in regards to cover"; a central
  lower base with a multi-directional jump pad cluster. Medium size, 4-6 players.
- **Tiers:** bottom base; middle level each side (rocket launcher, teleporter, jump pad); third level (shotgun, "one of
  the safest parts of the map", minimal sightlines); the top platform (quad) via a two-stage jump pad; a far railgun
  island reached by one jump pad.
- **Loops:** each pad's direction tells you where it lands; three teleporters all exit near the red armour, which "is a
  great way to get you back to the middle of all the action"; a long cross pad flies over the centre to the mirrored
  side, "creating potential collision moments."
- **Flow:** weapons placed by space: rockets on the mid floor, the shotgun where sightlines are shortest, the railgun on
  the exposed island. The void is the danger; the red armour is in "a very open and vulnerable location."
- **Why it's memorable:** "every space is mostly visible from any angle. And it forces players to engage"; air time is
  the risk (air rockets, rails).
- **Use for SKYGRIND:** the template for an "over the void / over lava" arena: launch routes whose direction is readable,
  every launch a commitment under fire, teleports or rails that return the player to the middle, and the best reward on
  the most exposed perch. Level Design Book credits it with "a dozen jump pads and teleporters to create strong upward flow".
- Source: Grit Gaming, https://www.youtube.com/watch?v=OsPO3rw39-4 ; Level Design Book "Verticality".

### B4. ULTRAKILL, The Cyber Grind (the game's own wave mode)
- **Shape:** "a 16x16 grid arena suspended in a void." Every cell has its own height and prefab; 38 official patterns,
  shuffled per run, one per wave; a zap grid at heights -4 to -7 that kills most enemies. Patterns carry jump pads and
  stairs (e.g. "Pits" 64 jump pads, "Cliffs" 104).
- **Flow:** the layout changes between waves only. At each wave start "the player regains all of their health and
  stamina" and cooldowns reset. Wave budget: starts at 10 points, each wave adds 3 + floor(wave / 3) (wiki examples: 28 at
  wave 5, 55 at wave 10). Spawn gates by progress: Deathcatchers caged until 33.3% of the wave is dead, cages open at 75%;
  one Mirror Reaper per wave, only after 50% dead. Spawn order: heavies first, then uncommon, specials, then commons at
  0.1 s intervals. Uncommon enemies from wave 12, specials 16, radiance 25.
- **Use for SKYGRIND:** the closest genre match to "CoD rounds in an ULTRAKILL arena". Two ideas worth stealing: the
  arena re-forms between waves (a fresh layout per round in one fixed footprint), and percentage-of-wave-dead gates for
  dangerous spawns. Its full refill at each wave start matches GG's "regen only at round end" (no in-wave regen).
- Source: https://ultrakill.wiki.gg/wiki/The_Cyber_Grind . Cell size in metres not given.

### B5. ULTRAKILL boss arenas (P-1, 3-2, 4-2, 5-2)
- **P-1 Minos Prime / Flesh Prison:** "A large, octagonal fleshy room", a "tiny, player-sized entrance", ceiling rising
  in the centre; it starts dark and lights up when the player steps deeper in. No cover or hazards described: the
  bosses are the threat. https://ultrakill.wiki.gg/wiki/P-1
- **3-2 Gabriel:** "a single, circular platform" with an eye in the centre of the floor, bordered by teeth, over an
  "endless black pit"; a beating heart nearby can be wall-jumped. https://ultrakill.wiki.gg/wiki/3-2
- **4-2 Insurrectionist:** "an open air arena featuring four sandstone platforms" in a desert of hazardous sand (10 dmg on
  touch), boss falls in when both skull keys are placed; Hakita designed 4-2 to "seem very open but without actually
  sacrificing the constant forward momentum of a linear level". https://ultrakill.wiki.gg/wiki/4-2
- **5-2 Ferryman:** phase 1 on the ship's bow inside a barrier; phase 2 on the roof, "a much smaller arena than before."
  https://ultrakill.wiki.gg/wiki/5-2
- **Common pattern (my reading):** one simple, symmetric, uncluttered shape per boss (octagon, disc, four pads, a deck),
  a narrow entry, a terminal before every boss so loadout can change (Professor Sock), and phases that shrink the space.
  No dimensions on any page.
- **Use for SKYGRIND:** boss arenas should be simpler than wave arenas, not bigger; size the arena to the boss's attacks
  (Sock: Swords Machine's "cramped arena" made the early fight harder). Contrast with DOOM's circle warning (F2): the
  ULTRAKILL boss shapes are round or symmetric because the boss, not the route choice, is the content.

### B6. Titanfall 2, "Effect and Cause" (Jake Keating)
- **Shape:** two complete maps of one facility, past and present, "perfectly aligned"; the player swaps between them at
  will and appears in the same spot. Early versions stacked the two periods vertically and playtesters were warped out of
  bounds.
- **Flow:** traversal and combat use both maps: soldiers in the past, decayed wildlife and robots in the present.
  Enemies originally kept moving in the other period; that was changed. Two major revisions; testers found it confusing
  and Keating resisted hand-holding.
- **Why it's memorable:** a single mechanic carried a whole level.
- **Use for SKYGRIND:** limited as a layout model (no geometry published); useful as proof that one strong arena mechanic
  (here the time swap; for SKYGRIND a rail network or a lava tide) can be an arena's identity. Respawn's own lesson from
  the same game (F25): environment changes during a fight confused players.
- Source: Wikipedia, https://en.wikipedia.org/wiki/Effect_and_Cause . Game Developer's "close analysis" page was opened
  but its content is a video intro with spoilers redacted.

### B7 (sequence model, not one arena). ULTRAKILL 1-1 "Heart of the Sunrise"
- A string of rooms with small waves keyed to positions (crossing a bridge spawns 4 Schisms and 2 Drones; passing the
  second pillars spawns 14 Filth; the third, 4 Strays and 4 Drones), then a return pass through the first rooms with
  new waves (first room: 10 Filth, 3 Strays). Shows how one space is reused by changing its enemies, the same move as a
  CoD round. https://ultrakill.wiki.gg/wiki/1-1

---

## Part C: the five specific questions

### C1. Flow and dead ends
- No corner the player must back out of: Yoder's "dead ends and killzones repel players" (F8); every perch needs two ways
  off (F14); every rail end lands on another traversable (F10).
- Two options per node, not five (F6). Separate ground and air loops (F7). Avoid a single round track (F2, F3); break a
  bowl with one one-way drop or height break (F3).
- Test: the four DOOM failure symptoms (retreat, doorway, sniping, kiting) as bot/Player metrics (F16).

### C2. Spawn placement relative to entry (push forward)
- Let the player see the arena before it starts; enter by a committed drop (F17).
- Enemies hold positions away from the entry; ranged units keep distance in exposed spots, so the player must cross
  (F15).
- Spawn waves keyed to a captain's HP (up to 50%), not deaths; fodder only early; one captain per wave; never two
  chasers (F18, F19).
- Enemy power depends on placement: spawning behind the player or beside another threat makes support units strong;
  directly in front of the player makes them free kills (F20). For a pad-first game, use behind-spawns sparingly (F5).
- Halo's territory plus fallback at 75% / 50% gives a group a direction of retreat the level designer chooses (F21).

### C3. Pickups and health steering movement
- Health from kills, with bigger enemies paying more when the player is low: healing is toward the threat (F22).
- Few fixed pickups, placed as bait at exposed or far points (rail ends, grapple perches), never at the entry; don't let
  a camper wait on a respawn (F23).
- Doom's three resources from three verbs (glory kill = health, chainsaw = ammo, flame belch = armour; Game Developer,
  https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i-) means each resource need
  sends the player toward a different enemy type.

### C4. Boss arenas
- DOOM: bigger is not more epic (F1). ULTRAKILL: one simple shape per boss, narrow entry, terminal before, phases
  that shrink the arena (B5).
- Marauder: Hugo Martin's defence (PC Gamer via search snippet; the article body did not load): the Marauder "falters for
  a specific amount of time to allow for at least one shot with most weapons, but just long enough for two shots if the
  player swaps"; in mixed arenas the player must "invert tactics at a moment's notice". Not opened directly.
- Doom 2016 critique: "in boss fights, there are no minions to glory kill!" - a boss arena with no fodder cuts the
  healing loop (Game Developer, "Design Notes: DOOM 2016", https://www.gamedeveloper.com/design/design-notes-doom-2016-).
  For SKYGRIND, a boss arena should still feed a trickle of fodder if healing comes from kills.
- Level Design Book: avoid maximum-intensity final encounters; Dark Souls' last boss has "a relatively simple combat
  style with obvious tells" (F24 source). God of War Valkyries: ~2-3 s tells, consistent attack speeds "to allow for
  consistent parry timing" (Level Design Book "Encounter") - relevant to GG's parry ask.
- HL2 Ep2: a Hunter that burst through a door into the player's room was in "his least viable tactical position"; the
  multi-exit Hunter arena means "hunters will never completely lose them" (commentary nodes 110, 186). A boss needs room
  to show its attack and the arena needs exits so the chase never stalls.

### C5. Readability
- Simple geometry, no clutter, every object with a purpose (F26).
- One colour per usable verb, one colour for danger, applied the same everywhere (F27); the next route in view on entry.
- One landmark visible from every tier; per-tier floor materials; light the important perches and enemy silhouettes (F28).
- Highlight state, not only space: DOOM shades staggered enemies (blue = vulnerable, yellow = glory-killable) because
  "in the chaos players weren't getting as much information as we wanted them to" (GDC 2018 talk, 31:38-32:09).

---

## Part D: a checklist for one ~30 x 30 m SKYGRIND wave arena (my synthesis from Parts A-C)

1. Entry: a high lip with a view of the whole arena and of wave 1, then a one-way drop or rail that dumps the player in.
   First foothold (jump-height cover) 5-10 m inside. (F17)
2. Three tiers: lava/hazard floor edges, mid ledges at rail height, two or three open high perches. (F4, B1)
3. Two loops: a ground strafe loop around the central landmark, and an air loop of rails linking the perches without
   touching the floor. Each node offers two outs. (F6, F7, F9)
4. Every perch: no cover on top, two ways off, the round's one big pickup on one of them. (F14, F23)
5. Cover: jump-height blocks only (one jump mantles, dash-jump clears), at least one per quadrant. No decorative props.
   (F12, F13, F26)
6. Lava with a job: it guards a pickup, or cuts one loop so the rail becomes the faster way. Lava never fills the strafe
   loop. (F9, B3)
7. Asymmetric sides: each side looks and plays different (rail side, grapple side, lava side), plus one landmark visible
   from every tier. (F2, F28)
8. Spawns: ranged on the far perches in the open, melee on the floor, the captain across the arena from the entry;
   behind-spawns rare; never two chaser types at once; new groups keyed to the captain's HP. (F15, F18-F21)
9. Healing: from kills; heavies pay more when the player is low; fodder keeps trickling for the whole round, including
   boss rounds. (F22, C4)
10. Between rounds: the arena may re-form (lava tide, rail on/off), never mid-round. (F25, B4)
11. Measure: time near the entry, time on one perch, mean engagement distance, and kiting time, per round, by bot and in
    the Player. (F16)

---

## Not opened (or opened with no usable content)

- GDC Vault pages (members-gated): "Embracing Push Forward Combat in DOOM" (watched on YouTube instead); "Designing
  Unforgettable Titanfall Single Player" (watched on YouTube instead); Sunset Overdrive "Level Design in a Day"
  (https://gdcvault.com/play/1022114/Level-Design-in-a-Day) and "Transitioning from Linear to Open World Design" - not opened.
- Adam Atomic, "Design of Doom Eternal" (jungle-gym vs canyon arenas): HTTP 403 on both URLs; only the search snippet
  was seen. https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
- PC Gamer, "Doom Eternal director says the Marauder is good, actually": page loaded without the article body; quotes
  above are from the search snippet only.
- Noclip / Bethesda "Designing Demons" (Hugo Martin on spawns and kiting): the Bethesda page is a video intro only; the
  video was not watched. Martin's "chess pieces ... ledges ... kite" quote is from a search snippet.
- GDC 2018 speaker Q&A at gdconf.com: HTTP 404.
- Quake wiki (fandom) Q3DM6 / Q3DM17 pages: HTTP 402. Liquipedia Quake map pages: HTTP 403.
- Game Developer, "A close analysis of Titanfall 2's level Effect and Cause": opened, a video intro with no detail.
- Mackey McCandlish interview (thesixthaxis): opened, no layout content.
- 80.lv Neon White breakdown: opened, no content.
- Halo 3 territory: only the Isla slides were read; Butcher / Griesemer 2002 slides read; no transcript of either talk.
- HL2 Episode Two commentary: first 100k of 118k characters read; the remainder not read.
- Doom Eternal climbable-wall look: guide sites via search snippet only; no id statement found.
- No source gave arena dimensions in metres for any Doom, ULTRAKILL or Quake arena.

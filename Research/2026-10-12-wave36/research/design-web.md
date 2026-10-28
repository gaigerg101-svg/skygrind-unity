# Design research for wave 36: arenas, onboarding, bosses, wall ride, gun feel, melee

Written 2026-10-12 for SKYGRIND (ULTRAKILL movement x CoD Zombies rounds, a boss per level, six levels, pad-first).
It answers GG's playtest verdict: random props (lava pits, pillars) with no job; bosses that are ugly block figures,
hard to read and don't test the level's mechanic; "teleporting" into boss rooms; level 2 opening with an enemy running
at you; a wall ride that feels like you "hook onto a wall and start sliding up it"; guns that aren't crunchy; and his
question about pivoting toward fists, explosions and 3D-platforming movement.

How to read the sources: **[fetched]** means I read the page itself. **[snippet]** means the claim comes from a
search-result summary and the page itself would not load (403, 402 or no text). Treat [snippet] claims as leads to
check, not as facts. Where no developer source exists I say so.

---

## 1. Arena and combat-space design

### What the sources say

- **Doom 2016 is built Corridor-Arena-Corridor-Arena.** Lead level designer Jerry Keehan: tightening spaces creates the
  perception of speed ("when you get close to the ground it matters a whole lot"). [snippet]
  https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i- (GDC 2018, Kurt Loudy & Jake Campbell,
  "push forward combat")
- **Push-forward combat** (id, GDC 2018): no regenerating health, no reload, no cover. The player takes ammo, health and
  armour from enemies, so the fight itself is the supply line. Enemy placement and resource placement are meant to keep
  the player engaging, never camping. [fetched]
  https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i-
- **Doom Eternal's chainsaw loop:** ammo capacity was cut and the chainsaw on fodder became the main resupply, "the
  gameplay loop has been adjusted to ensure that the player always has small fodder targets to chainsaw during
  battles." [snippet] https://doomwiki.org/wiki/Chainsaw_(Doom_Eternal)
- **Hugo Martin on "chess pieces":** "We use our chess pieces to move the player around and get them off those ledges
  and don't let them kite enemies." Levels are designed to avoid safe corners from which the player can pick demons
  apart. [snippet; source pages: GamesBeat / Intel interview, not loaded]
- **Arena shapes (Adam Saltsman's Doom Eternal analysis):** arenas are "flat-ish" or "tall-ish". Tall ones are either
  "jungle gyms" (distinct floors, separate combat levels) or "canyons" (open, no firm divisions between heights). You
  want all three types in a game because each changes the tactics. [snippet; the page returned 403]
  https://blog.adamatomic.com/post/613311014289768448/design-of-doom-eternal
- **Traversal kit as level parts:** Doom Eternal arenas have monkey bars, boost pads and climbable walls. Each traversal
  tool was introduced on its own before they were combined. Martin: "The combat between arenas is awesome; it's as good
  as the arenas." [fetched] https://shacknews.com/article/116635/hell-razer-the-making-of-doom-eternal?page=6
- **Every demon has a battlefield role.** Marty Stratton: "the purpose of all the demons ... is what's its role on the
  battlefield, and making sure that that comes through in everything it does, the way it attacks or the way it moves,
  or how easy it is to kill or what its weak point is." [snippet]
  https://slayersclub.bethesda.net/en-AU/article/designing-demons-noclip (the page only embeds the Noclip video)
- **ULTRAKILL (Hakita):** simple, clean geometry so the player can read the space at high speed; levels are linear so
  "there's always more action right around the corner"; AI "kept purposefully simple to make the enemies predictable and
  easy to keep track of"; threat comes from how many enemies there are, because the player's movement outclasses any
  one enemy. [fetched]
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience
- **ULTRAKILL layout:** "a level that seems very open but without actually sacrificing the constant forward momentum of a
  linear level". Hard encounters are spaced as peaks and valleys. Composition beats count: one Hideous Mass and two
  Virtues can be as hard as a horde. [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- **ULTRAKILL wave sizes (measured from the wiki).** 0-1: first arena 3 Filth, then 8 Filth. The new projectile enemy
  (Stray) arrives as a 2-Stray first wave, then 6 Filth + 4 Strays. Parry is taught in a hallway: 2 Strays, then
  5 Filth + 2 Strays. [fetched] https://ultrakill.wiki.gg/wiki/0-1 . 3-1 opens on a single Malicious Face in a narrow
  corridor as a warm-up. Its first arena chains waves by trigger (4 Strays + 1 Mindflayer -> a Cerberus wakes -> 2
  Streetcleaners), and acid is a hazard that "damages both players and enemies". [fetched]
  https://ultrakill.wiki.gg/wiki/3-1
- **Titanfall 2:** Steve Fukuda moved from "Swiss cheese" verticality to a "window pane" approach: lanes with set paths
  (left, middle, right). [snippet] https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs
  Respawn prototyped levels with "action blocks" (GDC 2018, Christopher Dionne). [fetched, summary only]
  https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks ;
  talk: https://gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player
- **Neon White (Ben Esposito et al.):** two core rules, "The player always knows where to go next" and "The player always
  has the right tool for the next encounter". Colour codes: "Doors are always red and always on the main path, green ivy
  means you're going in the right direction". No level requires platforming, shooting and card management at the same
  time. "The shortcut path must only use cards from the main path." [fetched]
  https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity
- **Neon White, the "Smackdown" level:** "Every card has to have an obvious purpose in the moment that you pick it up."
  An enemy was moved to the top of a staircase, shooting down, so that its position showed the path. Encounters are
  blocked out on their own and then assembled; the seams become shortcuts. More than 50 iterations over 3 years.
  [fetched] https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made
- **The Level Design Book** (combat chapter): ask "Where is safe, and where is high risk? Where can players fight or
  recover?" Block out a test arena (open and/or 3-lane), place enemies, playtest, and build encounters only after the
  enemy designs are settled. It gives no size numbers. [fetched] https://book.leveldesignbook.com/process/combat

### Rules for SKYGRIND (derived from the sources above)

1. **Every object has a job, or it goes.** A prop must be one of: a movement line (wall-ride wall, grapple point, jump
   pad, monkey-bar-style rail), cover that breaks one sightline, a resource spot, a hazard that also hurts enemies (as
   ULTRAKILL's acid does), or a path marker. A lava pit with no route over it and no enemy pushed into it is noise.
   (Stratton's "role on the battlefield" applied to geometry; Neon White's "obvious purpose".)
2. **Make safe ledges cost something.** Any high perch needs an enemy that can reach it or a flyer that punishes it
   (Martin's "get them off those ledges"). This matches GG's own rule: "There always really needs to be a route that
   enemies can climb up."
3. **Vary arena shape across the six levels:** at least one flat, one jungle gym (separate floors), one canyon
   (Saltsman's taxonomy).
4. **Resources live in the fight.** Put pickups where the player has to cross danger to reach them, and keep a supply
   of weak fodder (the chainsaw loop). Never put them in a safe corner.
5. **Keep the read clean.** Simple geometry, one colour language for "main path" and "usable" (Neon White's red doors;
   in SKYGRIND's palette, cyan = usable).
6. **Size is not published by any of these sources.** I found no developer-published room-size range. The useful
   measure is time: ULTRAKILL waves of 2-11 enemies, triggered in sequence. Measure your own rooms in seconds of
   crossing at full speed.

---

## 2. Onboarding and pacing (new enemies, new levels)

- **Kishotenketsu (Koichi Hayashida, Super Mario 3D Land):** introduce in a safe environment, develop, twist, conclude.
  A mechanic is "taught, developed, twisted and then thrown away in about five minutes flat." [fetched]
  https://www.mcvuk.com/development/video-nintendos-level-design-secrets-in-four-steps
- **Half-Life 2, Ravenholm:** you see a zombie cut in half by a saw blade, pull a blade from the wall with the gravity
  gun, and the next zombie walks into view: "In just 10 seconds, Valve taught the player that circular saws are great
  against zombies." Barnacles are shown eating something before they can catch you. [snippet]
  https://wnhub.io/news/other/item-11774
- **Titanfall 2, "Effect and Cause":** the time-shift is practised in low-stakes rooms before combat demands it. [fetched,
  thin summary] https://www.gamedeveloper.com/design/a-close-analysis-of-i-titanfall-2-s-i-level-effect-and-cause
- **ULTRAKILL's first meetings (from the wave data above):** a new enemy type's first wave is that type alone and small
  (2 Strays; 1 Malicious Face in a corridor), then it is mixed with known fodder. [fetched]
  https://ultrakill.wiki.gg/wiki/0-1 , https://ultrakill.wiki.gg/wiki/3-1
- **Doom Eternal's Marauder introduction:** a tutorial window appears before the first fight. The first fight is a
  boss-like duel in a tight arena, with fodder zombies around for glory kills and resupply. [snippet]
  https://gamespot.com/articles/doom-eternal-marauder-boss-guide-how-to-kill-the-g/1100-6475129/
  The May 2020 update added tooltips to the demon tutorials. [snippet]
  https://windowscentral.com/doom-eternal-update-adds-empowered-demons-and-single-player-traversal-tweaks
  I could not confirm from a source that Doom Eternal shows a freeze-frame name card; not checked.
- **Hakita on pacing:** back-to-back hard encounters exhaust players, so levels alternate peaks (bosses) and valleys.
  [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- **Neon White:** "You can't be creative in the middle of a tense level you've never seen before." [fetched]
  https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity

**Rules:** (a) Every level opens with 10-20 s and no threat: a walk, a view of the arena, the level's new verb or
prop used once. (b) A new enemy's first appearance is that enemy alone, or in a pair, in a confined space, ideally seen
before it engages (the Ravenholm saw, the 3-1 corridor). After that it gets mixed with known fodder. (c) Teach the
level's mechanic as intro -> development -> twist, and let the boss be the conclusion (section 3). (d) Name cards and
tooltips are accepted practice (Doom Eternal), but show them while the player is safe, never mid-wave.

---

## 3. Bosses that test the level's mechanic

- **Zelda's "final exam":** the dungeon item arrives about halfway through; the rest of the dungeon teaches it; "the peak
  of this test is normally at the boss fight." [snippet] https://sourcegaming.info/2021/09/17/big-baddies-breakdown-arrghus-the-legend-of-zelda-a-link-to-the-past/ ;
  https://tropedia.fandom.com/wiki/Final_Exam_Boss
- **Counter-example, Titanfall 2:** "Enemies like Richter and Kane weren't great or memorable, because they were wholly
  removed from the level's gimmick, with nothing interesting to replace it." Viper and Slone fared better because they
  had their own mechanics. [fetched, forum essay] https://destiny.bungie.org/forum/index.php?id=158975 . PC World:
  "tedious Titan-on-Titan boss battles kill the pacing". [snippet]
  https://www.pcworld.com/article/410849/titanfall-2-review-prepare-for-more-mech-dropping-wall-running-action.html
  This is the closest published match to GG's "doesn't test the level's mechanic".
- **ULTRAKILL V2:** "the first major test of all the player's abilities; they test their movement and stamina
  management, their aim, and their weapon usage." V2 mirrors the player (it slides, dashes, uses the Piercer and the
  shotgun). Its wing colour shows its intent: gold = running, blue = circling closer, red = rushing, green = retreating,
  each paired with a vocal cue. 40 HP, a single phase. [fetched] https://ultrakill.wiki.gg/wiki/V2
- **ULTRAKILL Gabriel tests the parry:** his thrown spear and greatsword can be parried back. "By observing which weapon
  he manifests, his attacks can be accurately predicted before he even starts them." Taunts pause his attacks ("his
  pride stops him from attacking while taunting"), giving a healing window. At 50 % HP he is launched and stunned, a free
  damage window, before a harder phase 2. His intro: a monologue, then he appears from a beam of light in the arena.
  [fetched] https://ultrakill.wiki.gg/wiki/Gabriel
- **Minos Prime:** shouts a callout before each attack ("Judgement!" before a dropkick). A stamina pool (6 in phase 1,
  12 in phase 2) caps his combo length, which creates readable openings. [snippet] https://ultrakill.wiki.gg/wiki/Minos_Prime
- **Doom Eternal Marauder:** the opening is "a sound cue and flashing green eyes". Hugo Martin: it teaches weapon
  swapping; it falters long enough for one shot with most weapons, or two if you swap. [snippet]
  https://www.pcgamer.com/uk/doom-eternals-director-says-the-marauder-is-good-actually/ ;
  https://gamespot.com/articles/doom-eternal-marauder-boss-guide-how-to-kill-the-g/1100-6475129/
- **Telegraph timing (Mike Birkhead, combat designer):** wind-ups work best at **1-1.5 s**; "Anything beyond 2 seconds I
  need to start counting. I no longer can react based upon the feel." Tells must not overlap: two attacks with similar
  startups muddy the read. The punish window must allow at least one full combo, plus travel time to reach the boss.
  "Bosses are not about subtlety." "Show what matters, and only what matters." [fetched]
  https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer
- **Beat structure:** Build-up -> Intro/Reveal -> Business as usual -> Escalation -> Midpoint -> "It's ON!" -> Kill
  sequence -> Victory. "Make a list of attacks or challenges that will test the skills you want to test." Give the boss a
  defensive move so it can't be stun-locked. [fetched]
  https://www.gamedeveloper.com/design/boss-battle-design-and-structure
- **Readable silhouettes (Valve, TF2):** characters were designed to be identified by silhouette alone, "even with no
  lighting cues"; rim highlights rather than dark outlines; "solving design problems using silhouette only". [snippet;
  the PDF downloaded but has no text]
  https://cdn.fastly.steamstatic.com/apps/valve/2008/GDC2008_StylizationWithAPurpose_TF2.pdf ;
  https://www.cs.princeton.edu/courses/archive/fall07/cos597B/papers/mitchell-team-fortress.pdf
- **Doom Eternal's Doom Hunter** is used first as a skill-check boss, then reused in normal encounters. [fetched,
  critic's analysis] https://theowluc.com/story/doom-eternal-perfecting-the-action-fps-chess-game/

**Rules:** (a) Write each boss as a final exam: name the level's verb (wall ride, grapple, charge shot) and make the
boss's main vulnerability reachable only, or best, through that verb. (b) Each attack's wind-up is 1-1.5 s, with its own
distinct pose plus one colour flash. On the dusk palette, red-orange = danger; ULTRAKILL also uses a second colour for
"parryable vs not" (yellow vs blue). (c) After each big attack, leave a punish window long enough for one full combo
plus the time to close the distance. (d) Phase change at 50 %, with a free-hit stun (Gabriel). (e) Reveal: walk the
player into the arena and let them see the boss arrive (Gabriel's beam, V2 waiting in the arena, the Build-up and
Intro/Reveal beats), with no teleport cut. (f) Block-figure bosses can still read: design the silhouette first, give each
attack a distinct limb or prop pose, and add a rim or emissive accent rather than more detail. (g) Audio callouts (Minos,
V2's vocal cues) are GG's domain (sound). Keep the events named so he can hook them.

---

## 4. Wall-run feel

- **Published Titanfall 2 numbers: none found.** Respawn never published its wall-run constants. The best public
  recreation is a UCSD course project's `TitanfallConstants.hpp` (origin not stated, treat as one team's tuning):
  minimum horizontal speed to attach 200 u/s; drop below 100 u/s; max 800 u/s; forward acceleration along the wall
  500 u/s^2; push toward the wall 800 u/s^2; **zero-gravity "grip" phase 1.0 s, then gravity ramps 0 -> full over
  2.0 s**; forced detach after 5 s on the same wall; enter only if dot(wishDir, -wallNormal) >= 0.1, stay while >= -0.26.
  [fetched] https://cse125.ucsd.edu/2026/cse125g2/docs/TitanfallConstants_8hpp.html
  (A search summary quoted 0.5 s grip / 1.0 s ramp / 1.75 s kickoff for the same file; the page I read says 1.0 / 2.0 /
  5.0. Trust the page.)
- **Titanfall 2 tech test feedback** led Respawn to raise wall-running speed for launch. [snippet]
  https://gamingbolt.com/titanfall-2-dev-to-increase-wallrunning-speed-reintroduce-titan-timer
- **Apex dropped wall running** because "it was engaging, but the counterplay was often impossible" (Moy Parra). Wall
  running suits linear maps with set lanes, not open ones. [snippet]
  https://press-start.com.au/news/playstation/2019/02/05/heres-why-apex-legends-doesnt-have-titans-and-wall-running/
- **Lucio (Overwatch):** hold jump on any wall and you ride it, facing any direction, without looking at the wall. You
  can't ride the same wall (or a wall facing the same way) twice without touching the ground or a different wall. The
  speed bonus during the ride has its own value. [snippet]
  https://gamerjournalist.com/how-to-wall-ride-with-lucio-in-overwatch-2/ ; https://overwatch.fandom.com/wiki/L%C3%BAcio
- **Mirror's Edge:** the goal was to feel momentum. Visible body, camera sway, edge blur as speed rises. [snippet]
  https://gamedeveloper.com/design/the-philosophy-of-faith-a-mirror-s-edge-interview
- **Ghostrunner** took Titanfall 2 and Mirror's Edge as references for fast first-person movement that is "manageable and
  not nauseating"; a "distinct smoothness in how the player connects to walls". [snippet]
  https://www.unrealengine.com/developer-interviews/ghostrunner-is-one-of-the-best-looking-coolest-cyberpunk-games-on-the-market
- **Practitioner advice (indie, lower authority):** tilt the camera a little each frame toward the target roll rather
  than snapping it; a small FOV increase sells acceleration; attach automatically when airborne against a wall instead of
  needing a button. [snippet] https://dude123code.medium.com/finally-a-good-wall-run-in-unity-4de42bcb7289
  The 25-degree Titanfall roll appeared in a search summary only, with no source; not checked.

**Diagnosis of GG's "hook onto a wall and start sliding up it":** every reference keeps the **horizontal** speed you
arrived with and runs **along** the wall. Height comes only from a short zero-gravity grip that decays, never from a
climb. Upward motion on attach (or a pull into the wall that reads as a hook) is the clunky signature. **Rules:**
(a) on attach, project velocity onto the wall plane and keep its horizontal part; clamp any upward speed to a small
value, or zero it; (b) gravity 0 for a short grip, then ramp to full (the UCSD values give a shape to start from);
(c) need a minimum entry speed and an intent dot product, so brushing a wall doesn't grab you; (d) ease the camera roll
in over several frames and ease it out on exit; (e) the exit jump goes away from the wall and up (the UCSD file's
constants); (f) block same-wall re-ride until ground or a different wall (Lucio). Movement stays exact (CLAUDE.md rule 3):
these are proposals for a named Mover task only.

---

## 5. Gun feel and juice

- **Vlambeer, "The Art of Screenshake" (Jan Willem Nijman):** the talk's list: animation, lower time-to-kill, higher fire
  rate, bigger bullets, muzzle flash, faster bullets, less accuracy, impact effect, hit reaction, enemy knockback,
  permanence, camera lerp, screenshake, player knockback, hit pause, weapon recoil, random enemy explosions. [snippet]
  https://dkliao.itch.io/the-art-of-screenshake-recreation ;
  Nuclear Throne numbers: on pistol fire, kick the camera back 6 px from the aim, add 4 to screenshake (which decays
  fast), weapon kick 2 (the sprite pulls back and slides home), hit pause "about 10-20 milliseconds whenever you hit
  something"; props stay damaged ("permanence"). [fetched] https://infovore.org/?p=5275
- **"Juice it or lose it" (Martin Jonasson & Petri Purho, 2012):** tweening, squash and stretch, particles, screenshake,
  screen flash, things that react ("add eyes"). [snippet] https://www.gamedeveloper.com/design/video-is-your-game-juicy-enough-
- **Hitstop (Sakurai):** "When you strike the opponent, both parties momentarily freeze." More damage means longer
  hitstop, under a hard cap. The victim shakes during it (side to side on the ground, up and down in the air), and the
  attacker shakes slightly too. [fetched]
  https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/
  Street Fighter 2 froze both characters for about 10 frames. [fetched]
  https://critpoints.net/2017/05/17/hitstophitfreezehitlaghitpausehitshit/
- **Doom:** "we spend time, effort and iteration making every single animation, every single effect and every single
  impact. All of it has to work together." [snippet] https://bethesda.net/ko-KR/news/the-art-of-doom-weapons .
  Glory kills last "hundreds of milliseconds, just because you want to keep the player moving." [fetched]
  https://bethesda.net/en-AU/news/the-guts-and-gore-of-doom-glory-kills
- **Tracer origin vs hit ray:** "It is critical for the trace to come from the camera for the controlling player to have
  a positive play experience." Monday Night Combat used three traces: hand to barrel tip, camera forward, barrel tip to
  the camera's hit point. Halo: Reach tracers "originate from the forehead plain as day". [fetched]
  https://gamedeveloper.com/design/opinion-adventures-with-guns-in-a-third-person-shooter .
  The common fix: resolve the hit with the camera ray, then draw the tracer from the muzzle to that hit point. [snippet]
  https://bugnet.io/blog/how-to-fix-tracer-rounds-not-following-the-actual-bullet-path
- **Hakita:** "The main focus was just on the game feel. If the base combat and functionality are fun and satisfying just
  by themselves, the rest is just an added bonus." Loud, raw audio (from God Hand) does much of ULTRAKILL's feel (GG's
  domain). [fetched]
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience ;
  https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- **Enemies shouldn't be sponges:** "Making enemies tanky would detract from the high tension do-or-die quick action and
  would make the enemies feel bulletspongey." [fetched] https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/

**Rules:** fire = muzzle flash plus a view kick that returns within about 100 ms (frame-count numbers not published for
FPS; not checked). Hit = a hit-flash on the enemy plus a short knockback or flinch. Kill = 10-20 ms hitstop (Nijman),
longer for heavier hits under a cap (Sakurai), with the victim shaking during the freeze. Draw tracers from the muzzle to
the point the camera ray hit. Leave permanence (scorch and debris that stay). SKYGRIND already has a sim-step hitstop;
these numbers are tuning targets for it, through config rows.

---

## 6. Melee-first and movement-first references (for the pivot question)

- **ULTRAKILL Feedbacker:** a punch does 1 damage with "very little knockback", about 0.35 s animation cooldown, and an
  exhaustion pool (max 2, refills 1.25/s). A **parry** (timed punch on an attack or projectile) does 5 damage, sends
  projectiles back as explosions, freezes the game and flashes the screen white, and refills health (up to hard damage)
  and stamina. Enemy projectiles get 3 mercy frames before damage applies. Yellow flash = parryable, blue = not.
  Punching right after a shotgun shot "projectile boosts" a pellet (+3.5 explosive damage). [fetched]
  https://ultrakill.wiki.gg/wiki/Arms , https://ultrakill.wiki.gg/wiki/Parry
- **Knuckleblaster:** 2.5 damage, "high knockback" that launches light and medium enemies; its shockwave launches them
  into the air. **Whiplash:** pulls light enemies to you and pulls you to heavy ones, 0.5 s cooldown. [fetched]
  https://ultrakill.wiki.gg/wiki/Arms
- **Hakita on the parry:** "I wanted to do parrying in a way that was (almost) purely offensive rather than (almost)
  purely defensive, since that felt like it would fit the hyperaggressive gameplay." He prefers high-commitment moves
  where "you have to weigh their risk and reward". [fetched]
  https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ ;
  https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- **Doom glory kills and the blood punch:** melee finishers came out of push-forward combat. Each glory kill gives a
  small health boost and charges the blood punch. Kept to hundreds of milliseconds. [fetched / snippet]
  https://bethesda.net/en-AU/news/the-guts-and-gore-of-doom-glory-kills ; https://doomwiki.org/wiki/Glory_kill
- **Anger Foot (Free Lives):** "It feels amazing to simply kick a door. It flies off its hinges, splats a guy against the
  wall and you shoot him." The risk: "You can't just do that same thing with a thousand rooms for 10 hours." They kept it
  fresh with sneaker power-ups, bosses, short levels and 160 BPM music. [fetched]
  https://superjumpmagazine.com/kickin-it-the-making-of-anger-foot
- **Severed Steel:** the player can't be hurt while stunting, wall-running or diving: the movement *is* the defence.
  [snippet] https://www.unrealengine.com/developer-interviews/severed-steel-is-an-fps-inspired-by-superhot-and-hotline-miami-made-mostly-by-a-solo-dev
- **First-person melee practice:** land the effects on the same frame the hit registers ("hit-stop"); knockback makes a
  hit feel weighty; THE FINALS raised its melee lunge from about 3 m to about 4.5 m on average. [snippet]
  https://www.reachthefinals.com/patchnotes/s11-melee-rework . ACE Team (Zeno Clash): trigger attack variety from
  context, not complex inputs. [fetched] https://www.gamedeveloper.com/design/5-tips-for-first-person-face-punching
- **Neon White:** never ask for platforming, shooting and resource management at once. [fetched]
  https://www.gamedeveloper.com/business/-neon-white-and-designing-for-player-creativity

**Pivot reading:** the references don't support melee *instead of* guns. ULTRAKILL and Doom make melee an
**offensive resource tool** inside a gun game (parry = heal + reflect; glory kill = health; chainsaw = ammo). Anger Foot
shows a kick-first game needs physics payoff (things fly and splat) and constant variety to last. The cheapest strong
step for SKYGRIND is a punch/parry that reflects red-orange projectiles and refunds health or ammo, with launch knockback
and hitstop. Explosions and bigger knockback fit "crunchy" without any change of genre.

---

## The 10 most actionable rules

1. **Every prop has a job** (movement line, cover against one sightline, resource spot, a hazard that also hurts enemies,
   path marker), or it is deleted. (Stratton; Neon White; ULTRAKILL acid)
2. **No safe perch:** each high spot has an enemy route up or a flyer that punishes it. (Martin's "chess pieces";
   GG NIGHT20)
3. **Level opening = 10-20 s with no threat**, then the new enemy alone or as a pair, then mixed with fodder.
   (Kishotenketsu; ULTRAKILL 0-1 and 3-1 waves; Ravenholm)
4. **Boss = final exam of the level's verb:** its main weak point is reachable best through that level's mechanic.
   (Zelda; Titanfall 2's failure; V2, Gabriel)
5. **Telegraph wind-ups of 1-1.5 s**, each attack with a distinct pose and colour, and a punish window of at least one
   full combo plus travel. (Birkhead)
6. **Walk-in reveal, not a teleport:** the player enters the arena and sees the boss arrive; phase change at 50 % with a
   free-hit stun. (Gabriel; boss beat structure)
7. **Silhouette first** for block bosses: readable with no lighting, a distinct pose per attack, a rim or emissive accent
   instead of more detail. (TF2)
8. **Wall ride keeps horizontal speed and kills the upward hook:** project onto the wall plane, a short zero-gravity grip
   then a gravity ramp, minimum entry speed and intent, eased camera roll, no same-wall re-ride. (UCSD Titanfall
   recreation; Lucio; Ghostrunner)
9. **Crunch stack:** muzzle flash + view kick; a camera-ray hit with a tracer drawn from the muzzle; enemy flash +
   knockback; 10-20 ms hitstop scaled by damage under a cap, with the victim shaking; permanence. (Nijman; Sakurai;
   Monday Night Combat)
10. **Melee as an offensive resource tool, not a replacement:** a parry/punch that reflects projectiles and refunds health
    or ammo, with launch knockback. (ULTRAKILL Feedbacker and Knuckleblaster; Doom glory kill and chainsaw; Anger Foot's
    variety warning)

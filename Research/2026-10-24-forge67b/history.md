# FORGE67b history scout: area / room / arena design across every level (2026-10-24)

Read-only scout for FORGE67b. GG's ask (2026-10-24, the forge's brief, verbatim): "need to build a another detailed 8 hour build prompt
following same methods, area deisgn need t be better in alot of levels thats the mian focus". Not yet in any repo file.

Read: docs/memory/{CORE,OWNER,LESSONS,THREADS,FEATURES}.md, ledger/wave-29..66.md; Research/*/GG-REQUEST.md (w29-w49); NIGHT47.md,
NIGHT54.md, NIGHT56.md, NIGHT57.md (GG's sections); NIGHT29/32/33/34/35/36/37/38/40/43/51/52/53/56/57/66-REPORT.md; DECIDE52/53/65/66/67.md;
docs/ROOMS.md (headers, as-shipped table, 30 s problem lines); Logs/n47/ASKS.md; Logs/n56/bot-r5/identity.txt; Logs/n60/flow-base.txt;
Logs/n60/L/b15/room-c1.txt; Logs/n61-n65 gg-proxy reports; Research/2026-10-21-wave57/GAPS.md; Research/2026-10-23-forge66/history.md;
Research/2026-10-24-forge67/DECISIONS.md; Tools/SimHarness/Harness.N65.Verb.cs and Harness.N65.VerbPilot.cs (grep only).
Measured here: `df -BG /c` = **14 GB free**; the level rows read from Design/unity-config.json with a read-only node script. Nothing run.
Not searched: Google Drive, Gmail, Calendar, Linear (FORGE67 DECISIONS.md:6 found this game's material lives in the repo).

## 0. The short answer
- **Every level's rooms have been rebuilt 11-22 times since w29, but only level 1 has had a rooms night since w57.** NIGHT65, NIGHT66 and
  the NIGHT67 plan are all level 1, and none shipped a room (wave-65.md:15-16, wave-66.md:16, HANDOFF.md:4-8). Levels 2, 4 and 5 last
  changed rooms in w52-w56, level 3 in w57-w60 (section 3).
- **GG has not played any room built after EXT53** (CORE.md:5: last reaction 2026-10-20 ~14:05). So no person has judged level 3's
  rebuild (n57-kiln) or NIGHT65-66's level 1. Every verdict since then comes from the walk bot, the computed checks or gg-proxy.
- **gg-proxy has ranked "the rooms are big empty boxes" #1 on four nights running** (Logs/n61/gg-proxy-start.md:26, n62/gg-proxy.md:22,
  n63/gg-proxy-start.md:31, n64/gg-proxy.md:21). Its worst frames come from **level 3** (kb3 THE PRESS HALL, kiln corridors), then
  **level 2** (flat navy walls, low step blocks), **level 5** (a straight green corridor) and **level 4** (flat banded walls, corners).
- **The worst corner, repeated since w20:** a verdict from an instrument that cannot see the design. First the walk bot (L-013,
  T-118, L-150), then a stills judge (L-105), then a skip test whose refusal never fired (L-089). **The verb pilots exist only for level
  1's verbs** (launch, pierce, ride, parry, slide: Tools/SimHarness/Harness.N65.VerbPilot.cs:289-337; skip refuses launch | pierce |
  parry | ride: Harness.N65.Verb.cs:13-17). A rooms night on levels 2-5 would have no verb MAIN unless it builds one first.
- **What worked is measured and simple:** a card first, every solid with a job, one builder per level, a baseline-relative shipping
  rule and a critic. NIGHT57 built level 3's five rooms that way and shipped them ON in one night (wave-57.md:18-24). NIGHT36's level 1
  beat sheet was followed by GG's "Gameplay feels a little bit better" (wave38 GG-REQUEST.md:5).

## 1. The campaign's levels today (read from Design/unity-config.json's level rows at this scout)
| # | level (weapon, boss) | shipped file (row ON) | combat rooms | last room change | state, one line |
|---|---|---|---|---|---|
| 1 | THE FOUNDRY (KESTREL; THE WICK) | n52-foundry (design.n52.foundry52) | 6: a2 a3 a4 a5 b1 a8 (ROOMS.md:17-25) | EXT53 (b1, a5), w56 spawns (wave-56.md:26-27) | a3 / a5 / b1 are contract rooms (w52); a2 / a4 / a8 never rebuilt to the contract (NIGHT52-REPORT.md:83-84); a3 fails I1 69 % (Logs/n56/bot-r5/identity.txt:6); n65-foundry (six new rooms) OFF, every slot `old` (HANDOFF.md:18-19) |
| 2 | THE ICE LINE (PULSE gauntlet; THE ICEBREAKER) | n52-iceline (design.n52.iceline52) | 8: g1-g8 (ROOMS.md:27-39) | w56 (g8's ASH PIT, NIGHT56-REPORT.md:21-24) | the biggest level, the highest walk-bot stall (34.7 %) and lowest air (37.3 %) (Logs/n60/flow-base.txt:37); g3 / g4 fail I2 (identity.txt:10-11); g1 / g6 never contract rooms (NIGHT52-REPORT.md:83) |
| 3 | THE KILN (THUNDERCLAP; THE KILN WYRM) | n57-kiln (design.n57.kiln57) | 5: kb2 kb2c kb3 kb3c kb3d (ROOMS.md:1311-1420) | w60 B13-B15 (wave-60.md:15-16) | rebuilt from scratch at w57 (GG's "3. yes", NIGHT57.md:10); every computed room check PASS (Logs/n60/L/b15/room-c1.txt:4-8); 10-14 solids a room (same file); never played by GG. ROOMS.md's as-shipped table (:41-48) still lists n52-kiln: stale |
| 4 | THE SPAN (grapple; THE LOOM) | n52-span (design.n52.span52) | 3: p1 p3 p5 (ROOMS.md:50-57) | w56 (p5's pad, wave-56.md:26-27) | 3 rooms and level 1's 3 enemy kinds (GAPS.md:20, :24-25); the bot falls 107 times per 10 walks (flow-base.txt:82); p2 THE GAP NEST built, OFF (design.n58.span58 false; T-144, THREADS.md:50) |
| 5 | THE SPIRE (TORCH; THE CROWN) | n52-spire (design.n52.spire52) | 3: v2 v5 v6 (ROOMS.md:59-66) | EXT53 (v2, pads; NIGHT53-REPORT.md:99, :103) | v2 fails I2 1.17 (identity.txt:15); the level's premise written at w47 (ROOMS.md:995-997) |
| 6 | (none) | finale OFF (design.n38.finale38 / n41.finale41 false) | 0 | w41 plan only (wave-41.md:11-13) | GG: "The sixth level should be deleted entirely ... rebuilt from scratch later because it looks awful" (2026-10-13, wave40 GG-REQUEST.md:5); T-57 parked for GG (THREADS.md:13) |

Design rows that are ON for levels: n52.foundry52 / iceline52 / span52 / spire52, n57.kiln57 (each level row since n37 chains levelFrom ->
levelTo). OFF: n38.iceline38, n38.finale38, n41.finale41, n47.finale47, n48.kilnBoost48, n58.span58, n65.foundry65.

## 2. GG's words (verbatim; dictation errors kept)

### 2a. On area design in general (chronological)
- 2026-10-05: the map should feel "like CoD zombies and ULTRAKILL with personality" (OWNER.md:22).
- 2026-10-06: "There always really needs to be a route that enemies can climb up, or just find a way to pass by normally." (OWNER.md:14)
- 2026-10-08: a LINEAR boomer shooter with "arenas that limit weapons or movement, closed rooms" (OWNER.md:45, the writer's summary).
- 2026-10-09: "Sometimes the objects are randomly placed and it just looks ugly"; "I'd like a lot of the rooms to be a lot smaller and
  more action packed like think about the design of Ultra Kilikow"; "these rooms don't encourage any unique gameplay"; "don't make up these
  big boxes make it UA unique style that forces the player to use the in game mechanics"; "every single level needs a complete arena
  overhaul completely from scratch so don't preserve anything OK that is an order" (all Research/2026-10-09-wave29/GG-REQUEST.md:5).
- 2026-10-09 ~19:05: "The rooms still don't incentivize wall‑riding or creative use of the weapons; many of them feel very boring and need
  more variety and complexity in their layouts"; "look at actual images of arenas and map layouts to see how everything is meticulously
  placed with intentional object variety"; "every level should revolve around a mechanic and a weapon" (wave30 GG-REQUEST.md:5).
- 2026-10-10: "the arenas are so boring for these stages"; "just add some new arenas on existing levels or just again encourage the
  gameplay more" (wave32 GG-REQUEST.md:3).
- 2026-10-11 ~10:00: "there needs to be more elements in each arena"; "I think the arena design is really poor"; "I need more types of
  tiles environment game design elements"; "overall the arenas are better" (wave33 GG-REQUEST.md:3).
- 2026-10-11 ~13:35: "the game is cluttered with detail for the sake of detail, and the arenas don't incentivize creative movement at all"
  (wave34 GG-REQUEST.md:5).
- 2026-10-11 ~19:28: "I only want like there to be six levels total and we just have to polish them a ton"; "a lot of the arenas and boss
  fights and levels to have more arenas or more stages that incorporate wall running elements and like almost semi platforming" (wave35
  GG-REQUEST.md:5).
- 2026-10-12: "my thing is you're really struggling to make detailed rooms and that really incentivize mechanics"; "A lot of the rooms
  just have random stuff in it just random lava pits random pillars that serve no purpose and it's very evident throughout all the
  levels The only one that sort of shows is the tutorial at the beginning that clearly incentivized the movement" (wave36 GG-REQUEST.md:5).
- 2026-10-12 ~19:25: "I need actually good arenas encourage the gameplay movement"; "all of them don't have to be a boss fight in a room"
  (wave37 GG-REQUEST.md:9).
- 2026-10-13 ~00:10 (the room brief NIGHT65-67 build on): "I really like the the layout of the platforming before the final pause"; "maybe
  you're forced to shotgun boost ... or a dash a far dash jump"; "I don't mind if they're wide open arenas with a roof"; "there's a time and
  a place for the tight rooms and keep them"; "one might encourage the dash jump another 1 I encourage wall riding more and you know they
  can't be too exploitable ... I don't want it so you can just dash jump over every single gap"; "you're leaving a lot of the old assets or
  old rooms that look very ugly with just random boxes and and lava that are just randomly placed with no meaning and the rooms are too
  tight"; "you're encouraged to just start shooting immediately and I don't want that gameplay"; "you should be flying through and and you
  know killing the enemies before you enter the next room in a stylish fashionable way" (wave38 GG-REQUEST.md:5).
- 2026-10-14 ~18:35: "we need to have way better arena deisgn for the gameplay. more elements that are not just decoration. research
  ultrakill, doom and other boomer shooters and actually look at real mmodels and images" (wave42 GG-REQUEST.md:11-12).
- 2026-10-16: "The arenas themselves remain one of the weakest parts of the game"; "a reusable set of arena-design principles so future
  arenas are not simply collections of platforms, ramps, walls, and enemies placed without purpose" (wave43 GG-REQUEST.md:19); "I do not
  want us to solve that by returning to meaningless decorative clutter" (:17); "Every ramp, wall, platform, obstacle, hazard, and piece of
  cover should have a gameplay reason" (:39); players "should not be able to accidentally wander backward into completed content" (:23).
- 2026-10-17: "if it arena related please dont just randomly plce stuff it needs intension and personality and lets cut fluff and old
  stuff, dont cut anything meaningful" (wave45 GG-REQUEST.md:3).
- 2026-10-19 ~11:20: "a visual polish all across the game whether it's lighting whether it's design whether the arenas it's just the
  colors and stuff are way too bland" (wave49 GG-REQUEST.md:3-4).
- 2026-10-20 ~14:05 (his last recorded play): "a lot of the decorative elements in several arenas are poorly executed and lack strong
  color direction"; "the UI looks good as it is ... don't change the core mechanics either" (NIGHT54.md:5-7).
- 2026-10-20 ~23:08: "what should overnight prmpt focus on to imporve levels specifically" (NIGHT56.md:4).
- 2026-10-23 ~13:08: "dont run into corners or repeat past mistakes, i wnat real progress and itterations and not just small polishes"
  (OWNER.md:120).

### 2b. Per level
- **Level 1 THE FOUNDRY.** 2026-10-09: "The first intro kind of rooms are very boring they're not dynamic"; "a section in the tutorial or
  one of the early levels where it forces you to wall ride no matter what" (wave29:5). 10-09 ~19:05: "one or two more environmental
  elements in that room to reinforce the idea of wall‑riding or avoiding hot lava"; "I'd love another wall‑riding room that uses lava as a
  core mechanic"; the boss arena "circular" (wave30:5). 10-13 ~14:05: "in the room that forces you to wall ride three times in a row you
  move too fast"; "a new wall material ... you cannot wall ride on it" (wave39:5). 10-14: "the waallride section on level one is awful and
  relaly difficult and not fun" (wave42:3), read as **THE GANTRY b2**, not a3 (wave42:21; forge66 history.md:127-129). 10-16: "the early
  pure wall-riding section and its entire arena need to be completely overhauled" (wave43:5); the three-wall climb "you can fly past the
  next wall" (:7); the ride-and-shoot arena's enemies "awkwardly floating" (:11); the door after it "runs into what feels like a brick
  wall" (:21). 10-18: "A first boss fight in the first stage is something I genuinely like a lot" but the climb "far too unforgiving"
  (NIGHT47.md:4-10). Nothing from GG on NIGHT52's a3 / a5 / b1 or NIGHT65's rooms (not found).
- **Level 2 THE ICE LINE.** 10-09: of the boss arena "I actually like the design a lot of how the arena is" and "this arenas maybe a little
  too small"; "there needs to be more level before it to teach you the ice mechanic" (wave29:5). 10-09 ~19:05: "The second arena also
  needs to be overhauled ... because it's too small"; "too easy to just run outside the area into the slope section" (wave30:5). 10-13
  ~00:10: "The middle walls are a little too short and clunky" (THE STAG; wave38:5). 10-13 ~22:00: **"Nearly all rooms in the second level
  need a complete overhaul to better support the new gauntlet mechanic, because the current layouts feel randomly placed rather than
  intentionally designed"** (wave40:5). 10-16: "When the player gets the gun again and enters the room containing additional enemies, I like
  the encounter concept" (wave43:29); "The enemy immediately before the Icebreaker fight ... is a great addition" (:35). 10-18: "a mini
  challenge with three or more of them plus other enemies before the boss" (NIGHT47.md:35-36).
- **Level 3 THE KILN.** 10-13 ~14:05: **"The 3rd level needs to be completely overhauled because right now it just isn't good ... it needs
  to encourage far more mechanics, have a new arena"; "all the rooms in the 3rd level feel boring, uninspired, and randomly placed"**
  (wave39:5). 10-13 ~22:00: "I love the mechanic itself" (the boost; wave40:5). 10-16: "The first room is confusing ... a strange ramp and
  other geometry that feels placed simply because an object needed to be there" (wave43:39); "The worm boss arena itself also needs a major
  visual redesign" (:45). 10-18: the worm's "arena should be altered to incentivize shotgun boosting more" (NIGHT47.md:44). 10-21: "3. yes"
  to "THE KILN rebuilt from scratch to 5-6 rooms" (NIGHT57.md:6-10). Nothing since (not played).
- **Level 4 THE SPAN.** 10-09 ~19:05: "I'd also like the fourth level to be made ... full creative freedom" (wave30:5). 10-10: "I also want
  the 4th level to be completely overhauled again" (wave32:3). 10-13 ~00:10: "the 4th boss fight needs to repulse a little more I like it"
  (wave38:5). 10-13 ~22:00: the STITCHER out, "the level focusing on mastering that tool"; select no-ride zones in the boss arena (wave40:5).
  10-18: "the arena isn't fun and doesn't support the intended gameplay" (the boss arena; NIGHT47.md:47-48). Nothing on p1 / p3 / p5 (not found).
- **Level 5 THE SPIRE.** 10-10: "I also want the 5th stage to be made" (wave32:3). 10-13 ~00:10: the platforming before the final boss
  (the forge's reading: level 5's rehearsal rooms, wave38:8) praised. 10-13 ~22:00: "the arena looks good, but players have no idea what to
  do" (the boss; wave40:5). 10-18: "it's not fun, the level doesn't make sense" (NIGHT47.md:50-51). Nothing on v2 / v5 / v6 (not found).
- **Level 6.** 10-11 ~19:28: six levels total (wave35:5); 10-13 ~22:00: delete it, "rebuilt from scratch later because it looks awful" (wave40:5).

### 2c. His reversals that bear on rooms
- **Size:** 10-09 "a lot smaller", "more condensed ... more claustrophobic" (wave29:5) -> the same evening "The first and second levels
  feel a bit too condensed" (wave30:5) -> 10-13 "wide open arenas with a roof" preferred, "a time and a place for the tight rooms", "the
  rooms are too tight" (wave38:5). DECIDE65 re-banded the long side to 14-32 m for both (DECIDE65.md:11-12).
- **Elements vs clutter:** 10-11 "more elements in each arena" (wave33:3) -> w33's kit33 dressed all five levels (wave-33.md:25) -> 10-11
  "cluttered with detail for the sake of detail" (wave34:5) -> calm34 cut 73-81 % of props (wave-34.md:20) -> 10-14 "more elements that are
  not just decoration" (wave42:12) -> 10-20 "decorative elements ... poorly executed" (NIGHT54.md:5-6). "More elements" meant pieces with jobs.
- **Lava:** 10-09 ~19:05 "replacing those gaps with actual lava" and a lava wall-ride room (wave30:5) -> 10-12 "random lava pits random
  pillars that serve no purpose" (wave36:5). Wanted with a job, not as filler.
- **Force vs offer:** 10-09 "forces you to wall ride no matter what" (wave29:5); 10-09 ~19:05 rails should "encourage skating, not force
  it" (wave30:5); 10-13 "builds in place that can't force you to just skip it" (wave38:5). The movement: 10-16 "Fix this at the movement-
  system level" (wave43:25) vs 10-20 "don't change the core mechanics" (NIGHT54.md:7). DECIDE65 D-5: a dash-jump crosses ~25 m (37 m with
  a double jump), so no flat gap forces a ride without a Mover change (DECIDE65.md:74-81); still GG's open call (DECIDE67.md:26-27).
- **Teach text:** 10-11 "the first level will have text overlay explaining mechanics" (wave35:5) vs 10-14 "way too much instruction at
  the tsrat of the level" (wave42:6).

### 2d. What he praised (rooms and arenas only)
"the lighting in the II ... a closed room" and the second boss arena's design (10-09, wave29:5); "A game is looking much better overall"
(wave30:5); "overall the arenas are better" (10-11, wave33:3); "the simple style we have now is fine" (wave34:5); "the tutorial at the
beginning that clearly incentivized the movement" (10-12, wave36:5); "Gameplay feels a little bit better", THE STAG's wall design, "the
layout of the platforming before the final" (10-13, wave38:5); level 3's boost "I love the mechanic itself", level 5's boss "arena looks
good" (wave40:5); level 2's "gun again" room and the mini-Icebreaker (10-16, wave43:29, :35); the first boss "conceptually very cool"
(wave43:25) and "something I genuinely like a lot" (NIGHT47.md:4); "The shotgun boost feels great" (NIGHT47.md:39).

## 3. Every night that rebuilt or redesigned areas
| night (date) | levels / rooms | method | measured | GG or gg-proxy after |
|---|---|---|---|---|
| w29 (10-09) | L1-L3 from nothing: n29-foundry 12 rooms, iceline 8, kiln 5 | GG's order; LevelValidate29 rows; walk29 | walks 11/11, 7/7, 4/4 (wave-29.md:3) | "much better overall"; rooms "very boring", "rooms worth wall riding in" asked (wave-29.md:8) |
| w30 (10-09) | L1 f5b lava room; L4 THE SPAN created (s1-s8) | Chrome images + hand SVG, one reviewer | walk30 10/10 x4; f5b's 6 m ledge unreachable (wave-30.md:3-5) | GG w31 / w32: "arenas are so boring" (wave32:3) |
| w32 (10-10) | L4 rebuilt from scratch (nine rooms); L1 1.9b LAVA BANKS, 1.10 CRUCIBLE; L2 FIGURE EIGHT, SKATE RING; L3 POUR HALL, SPLIT LEVEL; L5 created | research subagents + six builders | "level 5's rooms all sit at one floor height" (NIGHT32-REPORT.md:15, :19) | 10-11 "arena design is really poor", "overall the arenas are better" (wave33:3) |
| w33 (10-11) | ten rooms rebuilt (f8 f9 i2 i5 k1 k4 s1 s6 v2 v4) + kit33 dressing on all five levels | devices (fan, vent, platform), 36 prop kinds | f5 deaths 291 -> 17, k4 146 -> 34 (NIGHT33-REPORT.md:12) | "cluttered with detail for the sake of detail" (wave34:5) |
| w34 (10-11) | n34-foundry / iceline / kiln with grapple nodes and lanes; calm34 on all five | lines34 checker | lines34 11 / 17 fight rooms GREEN; props -73-81 % (wave-34.md:19-23); L4-L5 not rebuilt, six short rooms (:26) | 10-12 "the gameplay just feels awful", "random lava pits random pillars" (wave36:5) |
| w35 (10-11) | a wall-ride room on L1-L3; L6 finale (three wall-ride rooms) | builder A | walks 10/10 (NIGHT35-REPORT.md:33-34) | same 10-12 reaction |
| w36 (10-12) | **L1 from scratch** (8 rooms); L2-L6 untouched | **a beat sheet, one job per room (Design/levels/n36/foundry.beats.md), a job tag on every solid, a checker that fails a prop without one** | level36 PASS, walk 10/10 (NIGHT36-REPORT.md:4, :17, :33) | 10-13 "Gameplay feels a little bit better"; the old rooms of other levels "very ugly with just random boxes" (wave38:5) |
| w37 (10-12) | four boss arenas from scratch (THE WICK's stack, THE STAG's canal, THE LOOM's canyon, THE CROWN's spire) | one builder per boss (L-060) | 3 of 4 killed in a Player; "the rooms before each boss in levels 2, 4 and 5 are still the old ones" (NIGHT37-REPORT.md:52) | as above |
| w38 (10-13) | flight rooms: foundry38 (THE YARD, THE GANTRY), span38, spire38 ON; iceline38, kiln38, finale38 OFF | E's gate checker (forced verbs, skip moves) | gates L1 2/2, L4 3/3, L5 4/4, L3 0/4 ("blast and dash beat them"), L6 0/4 (wave-38.md:9-12; NIGHT38-REPORT.md:14-15) | 10-13: level 3 "just isn't good" (n35-kiln, kiln38 OFF) (wave39:5) |
| w39 (10-13) | kiln38 ON (K4: 12 m boost climbs) | checker re-run when it grew (L-061) | verbs 29/30 on n38-kiln (wave-39.md:13-17) | 10-13 ~22:00 the boost "I love the mechanic"; level 2 "randomly placed" (wave40:5) |
| w40 (10-13) | **L2 rebuilt from a plan, one job per room** (TANK ALONE ... THE REHEARSAL) | beat sheet | walk 10/10, verbs 20/20 (NIGHT40-REPORT.md:21-22) | 10-16 two level 2 rooms praised (wave43:29, :35) |
| w41-w43 (10-14..16) | L1 THE GANTRY (foundry41 band, then foundry42 forgiving, then n43 over lava, a chimney, a8 teach-then-spawn); L3 kiln43 first room (three boost steps); L5 spire41 | code-facts sweeps (L-069); DESIGN-RULES.md and ROOMS.md written (wave-43.md:9) | a person's ride per GANTRY wall 40.4 -> 100 % (wave-42.md:14); kiln landing 36 -> 82 % (NIGHT43-REPORT.md:44) | 10-14 GANTRY "awful" (after foundry41); 10-16 overhaul it, the arenas "one of the weakest parts" (wave43:5, :19) |
| w45 (10-17) | b1 THE SPILL, g7 THE THAW LOCK, THE KILN'S HEART (52 of 76 props cut) | **identity cards (A-9)** | walks 10/10 (wave-45.md:9-11, :16) | not played |
| w46 (10-17) | L1-L2 smoothed, g3 THE TIPPLE; L4 p1 / p3 / p5; L5 v2 / v6, the belfry gates | cards + a card gate (4 PASS in 10 min, L-083) + the n46flow meter (L-085) | L3 walks 7 -> 19 / 20, clear 218.5 -> 123 s; L4 stall 18.7 -> 14.5 % (wave-46.md:19-22) | 10-18: level 4's boss arena "isn't fun", level 5 "doesn't make sense" (NIGHT47.md:47-51) |
| w47 (10-18) | THE STACK's three states; g1 / g2 / g5 / g8 THE ROUNDHOUSE; kiln beats + islands; o3 THE LOOM FRAME; spire v1-v11 premise | cards + flow plans | THE LOOM FRAME falls 21.2 -> 2.6 a fight; spire's way-on windows seen from 10/10 rooms (Logs/n47/ASKS.md:46, :49) | 10-20 "decorative elements ... poorly executed" (NIGHT54.md:5-6) |
| w51 (10-19) | none ("Still the biggest problem, and not touched tonight: the rooms themselves") | view-only | (NIGHT51-REPORT.md:20-21) | - |
| **w52 (10-19/20)** | **18 rooms on all five levels** (kb2 kb2c kb3, a3 a5 b1, g2-g5 g7 g8, v2 v5 v6, p1 p3 p5); 37 jump pads | **THE ROOM CONTRACT** (20-32 m, >= 2.5 pieces / 100 m2, 3 planes, a 17 m ride, a pad, spawn rules, dwell, hold probe) + two persona reviews per level | density 0.33-1.81 -> 2.50-3.08; pads 0 -> 37 (NIGHT52-REPORT.md:9-10, :21-40) | persona GG: "none of the five would satisfy GG as built" (DECIDE52.md:165-171); "Every contract number is the walk pilot's" (NIGHT52-REPORT.md:81-82) |
| w53 + EXT53 (10-20) | in place: kb3, p1, a3, g4, g3, g7's pad; EXT53 g2, b1, g8, v2, kb2c, p3, a5, spire pads | the identity checks I1-I7 (n53-identity.py, planted FAILs) | identity FAIL items 33 -> 27 -> 11; rooms passing all 0 -> 9 (NIGHT53-REPORT.md:104-105) | reviewers: "none would satisfy GG" (:57-59); "the rooms break their mirrors by cutting one side; few cuts add a piece with a job" (:134; DECIDE53.md:109-113) |
| w55-w56 (10-20/21) | g7, g5 re-cut; spawns in view on all 18; p5's pad; g8's ASH PIT | the room checker (n56-room.py), a156 | in view 1 -> 16 / 18 (4 / 18 strict); identity FAIL rooms 9 -> 7 -> 5 (wave-55.md:19; wave-56.md:17-22) | critic as GG on g8: "It's a random lava pit, and it keeps killing me" (NIGHT56-REPORT.md:22-24) |
| **w57 (10-21)** | **L3 rebuilt from scratch: 5 rooms** | cards before any piece (A-9), one builder L, baseline-relative shipping rule + a batched critic | rooms 3 -> 5; stall 25.5 -> 19.0 %; deaths 8 -> 2; verbs37 31/31; shipped ON (wave-57.md:18-24) | GG: not played. gg-proxy later: the kiln's PRESS HALL "tall plain dark blocks", "the same corridor in 4 of its 6 frames" (Logs/n61/gg-proxy-start.md:28-31; n63/gg-proxy-start.md:34-36) |
| w58-w60 (10-21/22) | L3 exits (L0, B13-B15); L4 p2 THE GAP NEST (OFF) | one-row critics (L-109) | put at the next room 14/15/14 -> 0/0/0; GAP NEST contract 7 -> 12 / 14 (wave-59.md:13-16; wave-60.md:29-35) | not played |
| w63 (10-22) | kiln's presses as presses (S7) | spotter + critic | SHORT: the spotter saw "columns" twice (wave-63.md:34-35; THREADS.md:41) | - |
| **w65 (10-23)** | **L1's six fight rooms from scratch** (n65-foundry, OFF) | cards, a card gate (three passes), three reviewed rounds, verb pilots, the GATE pack | GATE 0 / 6 (MAIN on the walk bot's DULL); verb pilots 1 -> 6 / 6; skip GREEN 3 / 6 (wave-65.md:19-31) | gg-proxy keep-over-old: b1 YES; a3, a5 UNSURE; a2, a8, a4 NO (Logs/n65/gg-proxy-final.md:176-189) |
| **w66 (10-23/24)** | L1 again (OFF) | skip v2 as MAIN, verb plans, four rounds planned | a2 M1 1.52, a4 1.57 (one guard each); no GATE; "0 piece(s) added or moved" in six rooms through r2 (NIGHT66-REPORT.md:9-20) | no gg-proxy |

## 4. The corners (mistakes that repeated), with evidence
1. **The verdict from an instrument that cannot see the design.** L-013 (w20, LESSONS.md:17: "A judge can't measure what its bot can't
   play"); w52: every contract number the walk pilot's (NIGHT52-REPORT.md:81-82, T-118 THREADS.md:39); w57: a stills judge picked the
   "awful" n35 over the "a little bit better" n37 (L-105, LESSONS.md:107); w65: MAIN on the walk bot's DULL, GATE 0 / 6 (L-150,
   LESSONS.md:150); w65: a4's skip GREEN with "the refusal never fired" (L-089). **Next time:** levels 2-5 have no verb pilot or refusal at
   all (Harness.N65.VerbPilot.cs:289-337; Harness.N65.Verb.cs:13-17): the boost, blast, grapple, whip, jump-dash and TORCH would be judged by
   the walk bot again unless a tools slice builds them first.
2. **Decoration in place of pieces with jobs.** w33 kit33 dressed every level -> GG "cluttered" (wave34:5); w45's audit: k5 THE RING 0 of
   76 props with a reason, b1 3 of 8 pieces reasonless (L-057, LESSONS.md:62); w65's density round added props with no collider (a2 10 vs 0,
   b1 18 vs 3) while a3 fell to 5 boxes vs 12 (gg-proxy-final.md:102-118); w66: 0 colliders changed in six rooms (NIGHT66-REPORT.md:20).
3. **Cuts-only re-cuts.** EXT53: "each room breaks its mirror by deleting pieces from one side; none adds a piece that has a job"
   (DECIDE53.md:109-110); T-131's pattern "cuts alone break mirrors and give nothing" (THREADS.md:43). A mirror check (I1) can be passed by
   deleting.
4. **Patch-on-patch generators.** n29-n35 levels were gen-nN = gen-n(N-1) + passes (L-057, LESSONS.md:62); ROOMS.md became "a 1250-line
   changelog of five generations of the same rooms" (GAPS.md:42-43). Every level GG called "random" (L2 10-13, L3 10-13, "all the levels"
   10-12) came from that lineage; the beat-sheet levels drew narrower complaints (section 5).
5. **One level eats the nights.** Level 1's rooms were touched in about 20 nights; NIGHT65, NIGHT66 and the NIGHT67 plan are all level 1 with
   nothing shipped (wave-65.md:15, wave-66.md:16, HANDOFF.md:4-8), while levels 3-5 had three combat rooms each at w57 (GAPS.md:15-23) and
   levels 4-5 still do.
6. **The skip by the dash-jump.** w31: the pull crossed every level-4 gap in one tap (L-042, LESSONS.md:46); w38: level 3's boost gates
   "not proven (blast and dash beat them)" (NIGHT38-REPORT.md:15); w38-w39: a gate GREEN until the checker learned a move (L-061); w65: a
   dash-jump carries ~25 m, 37 m with a double jump, under any ceiling (DECIDE65.md:75-78). GG: "I don't want it so you can just dash jump
   over every single gap" (wave38:5).
7. **Card-gate cycles.** w65: three passes, 49 min against 10, 6 / 6 FAIL on the first; the open items were numbers only built geometry
   measures; the scouts never handed over jumpUp 2.2 m or ClipInto (forge66 history.md:52-56, :99-102). w46's single pass took 10 min (L-083).
8. **A moved spawn gaming an instrument.** w66: a3's spawn move lifted M1 by putting the skip's start "where a first entry never starts"
   (NIGHT66-REPORT.md:16; L-150). Earlier the same move fixed walk-bot numbers: kb3's retry spawn moved off the door's axis for dwell
   (wave-52.md:40-41), a3's 9 m in (ROOMS.md:142-143).
9. **Player-only lines in a night with no Player.** w61 (C: 4.5 GB), w65 (13.2 -> 8.7 GB), w66 (7.7 GB at t0): no room seen in the game;
   both w65 critics capped every room at UNSURE on a NOT CHECKED Player line (wave-61.md:26; wave-65.md:11-13, :25; wave-66.md:13).
   Today: 14 GB (this scout), under the guard's 12 GB plus headroom for a build.
10. **Rounds that stop moving the MAIN.** w65: a2 / a3 / a8 skip REDs identical r3 -> r5 (gg-proxy-final.md:32); w66: b1 1.33 vs 1.53 two
    rounds, dropped (NIGHT66-REPORT.md:19); a5's pour never took a body in five rounds (gg-proxy-final.md:67-72).
11. **A hazard for identity's sake.** g8's ASH PIT made the ride the fast line (I2) and killed the bot 7 times in 10 walks (0 before); the
    critic as GG: "a random lava pit" (NIGHT56-REPORT.md:21-24). Still shipped (GG's call, THREADS.md:41).
12. **A slot that cannot fit its card.** THE GAP NEST: a 50 m slot against a 20-32 m band, 11 / 14 after three cuts (L-112, LESSONS.md:116).
13. **Late rooms shipped unreviewed.** g2 rebuilt after the last persona review and shipped on its contract print (NIGHT52-REPORT.md:80).
14. **The usage limit in room nights.** w55 (2:28 in), w56 (3:12), w60 (~3:00), w63, w66 twice (4:35 of 8 h lost) (L-003, LESSONS.md:7).
15. **A quote read onto the wrong room.** The "awful" ride section is THE GANTRY b2; NIGHT65 kept b2 bit for bit and called a3 "GG's 'awful'
    section rebuilt" (forge66 history.md:127-129).
16. **Instructions read as "more stuff".** "More elements" became dressing twice (section 2c); "make the ride the way" became cutting the
    walk (a3 at w53: the far floor raised so walking is longer, ROOMS.md:137-143) rather than adding a reason to ride.

## 5. What worked, measured
- **A beat sheet with one job per room and a job tag on every solid** (w36 level 1, NIGHT36-REPORT.md:17; w40 level 2, NIGHT40-REPORT.md:21;
  L-057, LESSONS.md:62):
  the only rebuilds followed by GG's positive words on rooms: "Gameplay feels a little bit better" (wave38:5) and two level 2 rooms praised
  (wave43:29, :35). Not proof of cause: other rows changed the same nights.
- **Cards before geometry, one builder per level, a baseline-relative shipping rule and a critic** (w57): level 3's five rooms built and
  ON in one night; stall 25.5 -> 19.0 %, deaths 8 -> 2, verbs37 31/31 (wave-57.md:18-24); rows ON 7 / 11 vs 1 / ~12 the night before (L-103).
- **A computed per-room contract** (w52): 18 rooms "done" by one command (L-086); density 2.50-3.08, pads 0 -> 37 (NIGHT52-REPORT.md:21-40).
  It raised the floor, not the ceiling: the persona reviews still said templates (DECIDE52.md:165).
- **Computed identity checks with planted FAILs** (w53): FAIL items 33 -> 11, rooms passing all 0 -> 9 (NIGHT53-REPORT.md:104-105); they
  caught real sameness (mirrors, rides beside walks, rings round a mass).
- **Spawns in view** (w56): 1 -> 16 / 18 rooms; the bot cleared every level faster (NIGHT56-REPORT.md:17-21).
- **Fresh persona reviews of late rooms** found what the numbers missed: pads on walk lines, a vent in front of a respawn, a ride into a
  dead end (L-087; DECIDE52.md:177-187).
- **One-row critics** (w59-w60): kiln's B13-B15 fixed with put-ats 14/15/14 -> 0/0/0 and kb3c deaths 8 -> 0 (wave-60.md:29-31; L-109).
- **The verb instruments, for level 1** (w65-w66): verb pilots 1 -> 6 / 6 GREEN, skip GREEN 0 -> 3 / 6 (wave-65.md:21-22); skip v2 passed
  its own critic in three rounds and moved real numbers (a3 1.64 -> 1.13 once the refusal's bias came out; wave-66.md:21-24).
- **A code-facts sweep before the cards** (w42-w43, L-069): it named the causes the cards then fixed.

## 6. Ranking: which levels' area design is worst, and which rooms
| rank | level | GG's words | gg-proxy frames | computed / bot (guards only) | nights since its rooms changed |
|---|---|---|---|---|---|
| 1 | **2 THE ICE LINE** | "Nearly all rooms ... randomly placed" (10-13, wave40:5); later two rooms praised (wave43:29, :35) | "one flat navy wall", "low white step blocks along the walls, and nothing in the middle" (n63/gg-proxy-start.md:40-43; n64/gg-proxy.md:34) | g3 I2 1.28, g4 I2 1.18 FAIL (identity.txt:10-11); level stall 34.7 %, air 37.3 % (flow-base.txt:37); g3 stall 67.2 %, air 0.9 % (:30); g1 / g6 never contract rooms | 10 (w56) |
| 2 | **3 THE KILN** | the most repeated level-specific complaint: "just isn't good", "boring, uninspired, and randomly placed" (wave39:5), "The first room is confusing" (wave43:39); rebuilt w57, unplayed | the worst frames four nights running: kb3 THE PRESS HALL's presses "tall plain dark blocks", a corridor in 4 of 6 frames, a near-black void (n61:28-31; n62:26-30; n63:34-36; n64:33-36) | every room check PASS (room-c1.txt:4-8); 10-14 solids a room; entry in view kb3c 3.6 / 6, kb3d 3.0 / 5 (:7-8); level stall 18.8 % (flow-base.txt:62) | 6 (w60) |
| 3 | **4 THE SPAN** | the boss arena "isn't fun" (NIGHT47.md:47-48); nothing on its rooms | "two flat walls with green and cyan bands", "a corner" (n64/gg-proxy.md:35, :39) | 3 rooms, 3 kinds (GAPS.md:20); falls 107 per 10 walks (flow-base.txt:82); identity all PASS (identity.txt:18-20); p2 OFF (T-144) | 10 (w56) |
| 4 | **5 THE SPIRE** | "the level doesn't make sense" (NIGHT47.md:50-51), answered w47; its platforming praised (wave38:5) | "a straight green corridor with flat walls" in 3 of 6 frames (n63/gg-proxy-start.md:39; n64/gg-proxy.md:37) | 3 rooms; v2 I2 1.17 FAIL (identity.txt:15); stall 17.5 % (flow-base.txt:100) | 13 (EXT53) |
| 5 | **1 THE FOUNDRY** | the ride links (b2 "awful", a7's climb), the intro rooms "very boring" (10-09); nothing on its current fight rooms | "a long empty hall with one ramp block" (n63/gg-proxy-start.md:37-38) | a3 I1 69 % FAIL (identity.txt:6); a8 stall 45.6 % (flow-base.txt:9); a2 / a4 / a8 old; n65 rooms OFF: gg-proxy NO for a2, a8, a4 | 0 (three nights, OFF) |
Caveat for every frame line: verbs37 is a bot tour; 6 of 30 frames face a bare wall by the bot's pose, and corridors between rooms (links)
appear (n63/gg-proxy-start.md:18-20). Which frames are links and which fight rooms: not checked. The links were never rebuilt
(NIGHT52-REPORT.md:84).

**Rooms to start from, by the record:** L2 g3 THE TIPPLE II (I2, air 0.9 %), g4 THE REACH II (I2), g8 THE ROUNDHOUSE II (the ASH PIT),
g1 TANK ALONE and g6 THE ARMOURY (never contract rooms); L3 kb3 THE PRESS HALL (the proxy's worst frames, press63 SHORT), kb3c / kb3d
(in view at the door); L4 the missing room (p2's slot, split into p1b + d1b + a 28 x 32 m room at w60, wave-60.md:35; THREADS.md:50) and p5 (pads);
L5 v2 THE FALLEN BELL II (I2); L1 a3 (I1) and b2 THE GANTRY only on GG's word (DECIDE67.md:28).

## 7. Recommendations for an 8-hour night aimed at better area design in many levels
1. **Breadth this time: levels 2, 4 and 5, plus level 3's worst room.** GG's "in alot of levels" (2026-10-24) reads against a fourth level-1
   night. Level 1 already has NIGHT67's plan and a play build waiting for GG (Builds/n65-play-latest, DECIDE67.md:24). The forge must say
   whether NIGHT67's level-1 plan is replaced or runs later.
2. **How many rooms:** about 6-8 rooms that GATE, in 2-3 levels. Evidence: w57, 5 rooms, 1 builder, t0 09:34 to the freeze build 12:53
   (about 3.3 h) and shipped (wave-57.md:1, :13-15, :18-24); w52, 18 rooms in 8 h with 2 room builders plus the lead, but only to a walk-bot contract (wave-52.md:8, :15-18);
   w65, 6 rooms in 7 h with 2 builders, 0 GATE (wave-65.md:23-25). The usage limit allows two builders and one review slot (L-003; FORGE67
   DECISIONS.md:8-11). One builder per level (L-060, w57), 3-4 rooms each.
3. **Method (keep):** a card per room (A-9: spatial law, verb, the 30 s problem, every solid's job); a code-facts sheet in the brief before
   the cards (ai.grid.jumpUp 2.2 m, the dash-jump's ~25 / 37 m, ClipInto, each slot's size vs the 14-32 m band, the level's own verb
   numbers) so one card pass suffices; geometry as collider pieces with jobs; the identity checks (I1-I7, planted FAILs), the room checker
   (footholds, in view, dead ends) and the contract as guards; a fresh persona review (P-rank runner + GG) of every changed room; the
   baseline-relative shipping rule with a per-room off switch so the rooms reach GG.
4. **The MAIN problem, decided before t0:** the verb-aware skip test covers launch / pierce / parry / ride only. For levels 2-5 either (a) a
   tools slice first builds pilots and refusals for the level's verb (blast for L2, boost for L3, grapple / whip for L4, jump-dash / TORCH
   for L5; older harness checks exist for boost, grapple / whip and dash-jump: Harness.N43.Move.cs:41, Harness.N34.Latency.cs:97-101,
   Harness.N38.Gates.cs:147-151; skip v2 took three critic rounds at w66, wave-66.md:21-24), or (b) the MAIN is a computed geometry
   measure that already exists and sees the design (I2's walk-vs-verb ratio on the nav grid, I3's teach verb) plus a piece-count with
   colliders, with the walk bot only a guard. Do not set any aim on the walk bot's DULL / stall / air (L-150).
5. **The disk decides the branch at t0:** 14 GB now = branch C (sim only). Write no Player-only doneWhen line, or GG frees space before t0
   (DECIDE67.md:8-19 lists the commands). Without a Player, no room is seen; with one, a per-room entry / mid / high frame (a452) plus
   verbs37 on every changed level on the shipping build (L-090).
6. **Corners to design out (section 4):** MAIN on the walk bot; dressing counted as building (count colliders with a verb job, apart from
   dressing); mirror fixes by deletion; patching a room GG ordered rebuilt; card-gate cycles (one pass, open items into round 1); spawns
   moved toward the verb line (the spawn is the retry point); two rounds with no MAIN move = redesign or drop; a hazard with no job (no lava
   without a way out, a reason and a body it takes); a slot that cannot fit its card (check L-112 first); rooms changed after their last
   review shipping unreviewed; writes or commits during a round; more than two builders; quotes read onto the wrong room (quote the room).
7. **Report for GG's play, not for the instruments:** each changed room named with what to try in it, and an off switch per room. GG has
   not played anything since 2026-10-20 (CORE.md:5); the next real verdict is his.

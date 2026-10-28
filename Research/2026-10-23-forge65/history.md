# FORGE65 history scout: rooms, movement and "real progress" (2026-10-23)

Read-only scout for FORGE65. Sources: docs/memory/{CORE,OWNER,THREADS,LESSONS,FEATURES}.md, ledger/wave-46..64, Logs/n63|n64/gg-proxy.md,
NIGHT52/53/57/63/64-REPORT.md, EXT53.md, DECIDE52/63/64.md, Research/2026-10-13-wave38/GG-REQUEST.md, Research/2026-10-21-wave57/{GAPS,calib}.md.
Everything here was read from those files; "not checked" where it was not. GG has not played a build since 2026-10-20 ~14:05 (CORE.md:5):
n54-n64, eleven nights, are unplayed.

## 1. GG's words on rooms, levels, movement and real progress (newest first)
- 2026-10-23 (FORGE65 brief): "follow same methods on finding what to focus on, dont run into corners or repeat past mistakes, i want real
  progress and iterations and not just small polishes".
- 2026-10-22 ~22:11: "be honets and critcal on where we need to improve, i would lead towards bosses ... boss animations are too rigid" (OWNER.md:124).
- 2026-10-22 ~08:1x: "need to see real results and it needs to be honest and critical where things are not good" (OWNER.md:122).
- 2026-10-21: level 3 "yes" to a rebuild from scratch; "start it immediatley, building" (OWNER.md:121; wave-57.md:3-6).
- 2026-10-20 ~23:08: "what should overnight prmpt focus on to imporve levels specifically, gameplay, eniemies, colors" (wave-55.md:34-35).
- 2026-10-20 ~14:05 (his LAST PLAY, of e53, after the w52/w53 room rebuilds): "a lot of the decorative elements in several arenas are poorly
  executed and lack strong color direction"; flicker bugs; "the UI looks good as it is, so don't touch that, and don't change the core
  mechanics either, but the movement needs to feel cleaner, more fluid, and less choppy" (OWNER.md:118). He did NOT say "rooms boring" here.
- 2026-10-17: "if it arena related please dont just randomly plce stuff it needs intension and personality ... cut fluff ... dont cut
  anything meaningful" (OWNER.md:113).
- 2026-10-16: "treat this as a challenge to make a major leap in quality rather than a collection of tiny patches"; wall riding "clunky and
  buggy ... Fix this at the movement-system level"; "Do not spend the majority of the session repeatedly testing tiny changes" (OWNER.md:111).
- 2026-10-14: level 1 "waallride section ... awful and relaly difficult and not fun"; "pressing a near a wall you fly bcak so fast"; "shoutgun
  jumping is too difficult"; "way too much instruction at the tsrat of the level"; arenas "more elements that are not just decoration ...
  actually look at real mmodels and images" (OWNER.md:110).
- 2026-10-13 ~14:05: level 3 "just isn't good ... encourage far more mechanics, have a new arena"; the ride "you gain speed too quickly"
  (OWNER.md:109).
- 2026-10-13 ~00:10 (the most specific room brief on record, GG-REQUEST w38:5): "I don't mind if they're wide open arenas with a roof ... I
  prefer that side of game play ... there's a time and a place for the tight rooms and keep them"; "the rooms are too tight where ... you're
  not encouraged to wall ride you're encouraged to just start shooting immediately and I don't want that gameplay"; "one might encourage the
  dash jump another 1 encourage wall riding"; "they can't be too exploitable ... I don't want it so you can just dash jump over every single
  gap"; "you're leaving a lot of the old ... rooms that look very ugly with just random boxes and lava"; "you should be flying through and ...
  killing the enemies before you enter the next room in a stylish fashionable way"; wall rides "you just stay on forever" (OWNER.md:108).
- 2026-10-12: "the gameplay just feels awful"; "you're really struggling to make detailed rooms that really incentivize mechanics"; "random
  lava pits random pillars that serve no purpose" (CORE.md:7).
- 2026-10-11: "the arenas don't incentivize creative movement at all"; "the movement, UI, responsiveness, and overall flow need to be
  dramatically better"; "overhaul the wall riding to feel a lot more like Lucio"; "we're taking two steps forward and a step backward"; "I
  expect way more building and iteration"; "I only want like there to be six levels total and we just have to polish them a ton";
  "I expect major major changes" (OWNER.md:89, 95, 98, 104, 106, 107).
- 2026-10-09: "every single level needs a complete arena overhaul completely from scratch so don't preserve anything OK that is an order";
  "a lot of the rooms to be a lot smaller and more action packed"; "it needs to feel like speed skating"; rails "slippery and
  momentum-driven" (reversal); "The rooms still don't incentivize wall-riding ... many of them feel very boring" (OWNER.md:61-74).
- 2026-10-06 / 10-05: "I'm not seeing enough building"; "you move too fast ... We need a little bit of balance"; "we're going in circles,
  find new methods" (OWNER.md:10, 36, 37).
**Praise:** "A game is looking much better overall" (2026-10-09, OWNER.md:67); "Gameplay feels a little bit better"; "I really like the
layout of the platforming before the final [boss]" (= level 5's rehearsal rooms, GG-REQUEST w38:5 and its readings) (2026-10-13, OWNER.md:108);
"the simple style we have now is fine" (OWNER.md:98); "the UI looks good as it is" (OWNER.md:118); "great" for NIGHT52 (not a play, OWNER.md:116).
**Note:** gg-proxy's #1 ("smaller", "boring") quotes 2026-10-09..10-17 only (Logs/n64/gg-proxy.md:21-31); it never cites the 2026-10-13 open-arena
brief, and GG's one play after the w52 rebuild named decor and colour, not emptiness.

## 2. Every attempt at the rooms (w46-w64) and the numbers that passed while the result was still disliked
Before w46 (context): w29 three levels from scratch, w33 ten rooms, w35-w38 "overhaul for flight", w43 levels 1-3, w45 the identity night
(L-057: b1 3 of 8 pieces reasonless, k5 0 of 76 props with a reason). docs/ROOMS.md is "a 1250-line changelog of five generations of the
same rooms" (GAPS.md:42-43). About fifteen nights have touched rooms.
| wave | what it did | numbers that PASSED | what still failed, and the corner |
|---|---|---|---|
| w46 | flow night: levels 1-5 smoothed in data, the flow meter n46flow | L3 walks 7 -> 19/20, dwell 14.2 -> 6.2 s; L1 dead 53.3 -> 30.2 s (wave-46.md:19-22) | GG never tested; v2's bell "a stepped stack" (T-81); every number a walk pilot's |
| w52 | THE ARENA NIGHT: 18 rooms rebuilt to THE ROOM CONTRACT (20-32 m, >= 2.5 pieces/100 m2, 3 planes, 17 m ride, a pad) + 2 persona reviewers | 18/18 contract (0.33-1.81 -> 2.50-3.08), pads 0 -> 37 (wave-52.md:14-18) | D-28 "none of the five would satisfy GG as built": g4 a mirror, g3 a giant hopper, kb3's ride beside a walkable bank, p1's ramp makes the grapple optional, four rooms "a ring round a central mass"; g2 shipped after the last review, unreviewed; links never rebuilt (NIGHT52-REPORT.md:76-84; DECIDE52.md:165). Corner: the contract measures density and parts, not sameness or play |
| w53 | THE SECOND PASS: identity checks I1-I7 (mirror, ride shortcut, pads off lines, one shape); re-cut kb3, p1, a3, g4, g3 | identity FAIL items 33 -> 27; kb3, p1 all PASS (wave-53.md:28-31) | 09:00 reviews: "none of g2 / g3 / g4 / kb3 would satisfy you"; building share 53 % vs 75 % target, "the log's CHECK lines outweighed BUILD" (wave-53.md:32-36) |
| EXT53 | 4 builders: mirrors (g2, b1, g8, v2, kb2c, p3), RIDE KILL | identity FAIL items 27 -> 11 | "the rooms break their mirrors by cutting one side; few cuts add a piece with a job" (NIGHT53-REPORT.md:134, D-22); g8 THE ASH PIT: critic-as-GG "a random lava pit, and it keeps killing me", deaths 0 -> 7 (THREADS.md:41) |
| w55 | g7 / g5 re-cut in place | I1 99 -> 52 %, 76 -> 50 % | usage limit at 2:28, 25 of 26 entries never gated (wave-55.md:12-28) |
| w56 | LEVELS FIRST: first wave in view, seven rooms planned | in view 1/18 -> 16/18 (checker) | strict reading 4/18 (13 rooms no foothold, T-134); stall iceline 32.0 -> 36.5 %, foundry deaths 15 -> 21; 5 rooms left; only clear55 ON (wave-56.md:17-32) |
| w57 | level 3 n57-kiln from scratch: five rooms from identity cards | stall 25.5 -> 19.0 %, deaths 8 -> 2; verbs37 31/31 (wave-57.md:18-24) | the bot stuck at doors (moved on 15/17 vs 3/4), kb3c 3/6 and kb3d 2/5 in view, the 4.5 m tiers never checked by a person; judge calibration FAIL (L-105); GG has never played it |
| w58-w60 | three nights fixing level 3's exits / spawns / pocket (L0, B13-B15) and level 4 THE GAP NEST | put-ats 14/15/14 -> 0/0/0; kb3c deaths 8 -> 0 (wave-60.md:29-31) | GAP NEST 7 -> 11 -> 12/14, OFF: a 50 m slot vs a 20-32 m band (L-112; wave-59.md:15) |
| w61 | S2 (level data) planned | - | not started: no Player could be built (wave-61.md:26-29) |
| w63 | S7 press63 (kiln's presses) | 468,517 px changed | the blind spotter read "columns" twice: SHORT (wave-63.md:36) |
| w63-w64 | rooms "not tonight" (NIGHT64.md:24) | - | gg-proxy #1 both nights: 12/16 then 8/10 level frames "empty boxes" (Logs/n63/gg-proxy.md:35-55; n64:21-40) |
**The corners, in one line each:** (a) the contract and identity numbers went green (18/18, FAIL 33 -> 5) while every human-proxy review said
"none would satisfy GG"; (b) re-cuts in place made by deleting one side, not adding pieces with jobs; (c) every room number is the walk
pilot's: no human, no route pilot, ever rode these rooms (T-118); (d) the contract encodes "smaller, denser" (2026-10-09) and not the later
"open arenas with a roof, a verb you cannot skip" (2026-10-13); (e) fixing a rebuilt level's exits ate three nights (w58-w60); (f) gg-proxy
judges verbs37's fixed frames (f0400 / f1200), several of them links and corridors ("one straight green corridor to the door", n64:36-37),
so a room rebuild can be invisible to it; which room each frame shows: not checked.

## 3. Mistakes this night must not repeat (rules, with evidence)
1. **No room is "done" on the walk bot, the contract or the identity checks.** They are guards. w52 18/18 at 14/14 vs D-28; T-118; L-087.
2. **No cuts-only re-cuts.** A change adds a piece with a job (a verb it forces). NIGHT53-REPORT.md:134; T-131 (THREADS.md:43).
3. **No in-place patching of a room GG ordered rebuilt.** "don't preserve anything" (OWNER.md:61) vs w53 / EXT53 / w55 re-cuts in place;
   patch-on-patch generators accrete props (L-057).
4. **No slate of small view-only polishes.** w62's list ran out at 0:22 of 2:00 (wave-62.md:29); w63's slate was dry at 4:43 of 7:00
   (NIGHT63-REPORT.md:176); w64 shipped three text / colour rows; T63 passed on 0.6 % of pixels. GG today: "not just small polishes".
   The slate must hold more building than the box, with builds (not measurements) as the leftovers.
5. **No night where checks outweigh building.** w53 53 %, w52 68 % building share; a room cycle in a timed loop fits only a pad move; a
   re-cut is a builder's round with its own runs (L-092).
6. **Never build into a slot that cannot fit the card.** Check the slot's size first (L-112: GAP NEST, three cuts, 12/14, OFF).
7. **No stills judge stands in for GG.** calib.md: the blind judge picked n35 ("awful") over n37 ("a little bit better") in both orders
   (L-105); pairwise wins never predicted GG (L-002). Judge rooms on played route reels and room-interior poses, not verbs37's f0400 / f1200.
8. **No fix after a PASS ships unreviewed.** T63c (w63) and g2 (w52, NIGHT52-REPORT.md:80); a PASS binds to its sha (DECIDE64 D-6).
9. **Two builders never share a scratch folder.** w64: D and G ran dn-check in one folder; G's GREEN read 3384 false errors (wave-64.md:41; L-119).
10. **No meter built before it is seen on a frame.** kill55 missed its own meter three nights (L-111); rim56 took ~4 h over three attempts
    (wave-60.md:50); lamp60 3 h, blocked; B2's meter misjudged three times (L-136); a simulated fix is a hypothesis (L-108).
11. **No doneWhen the baseline already fails** (L-100: w56 "stall <= 25 %" against baselines 26-32 %). Write it relative (L-103).
12. **A room change re-runs the level checks on the build that ships:** verbs37's longest-wall pick (L-090, L-095), a new id into
    design.maps.list (L-065: a missing id boots SURVIVAL), the Editor never loads a campaign level (L-044), BaseId routes (L-074).
13. **No hazard added for identity's sake.** g8 THE ASH PIT is GG's banned "random lava pit" (THREADS.md:41; CORE.md:7).
14. **At most three builders and one review slot; commit every slice; keep a verified RC.** w55's 5 builders + 6 agents hit the limit at
    2:28 with 1 of 26 gated (L-098); w63's stop ate the close (L-003); w64's RC chain met D-8 (wave-64.md:30-33).
15. **Read disk and commit free before every heavy job; prune reels at once.** w61 built nothing (disk 4.5 GB); w63 commit free 1.0 GB from
    another session (L-128, L-146); n63-b4 11 GB unpruned (L-140).
16. **A builder never edits the checker that judges its own row without a fresh critic** (L-106); **a close table never claims rows the repo
    lacks** (L-107, w58's A0).

## 4. The movement complaint (gg-proxy #2 every night)
- **GG:** seven asks 2026-10-09..10-20 (section 1), the last "cleaner, more fluid, and less choppy" with "don't change the core mechanics"
  (OWNER.md:118). CLAUDE.md rule 3 pins Mover.cs and the golden vectors.
- **Built before:** ride40 / ride43 (the endless ride, attach intent), wallfling42 (back push 8 -> 0 m/s), capJumpOff (L-064), move54 (a354
  held 0.250 -> 0.150 s, jitter 0.246 -> 0.191, VSync default on for new profiles; wave-54.md:22-23); step55 x0.74 vs want x0.50, not retried
  (THREADS.md:45); move43's pilot never rides a wall in a Player (T-66).
- **Measured (w63 M1, DECIDE63 D-2):** unsynced bench p50 3.70 / p99 10.10 / max 32.10 ms, 6.32 % of frames changing interval by > 2x,
  spikes in main-thread engine time; the camera smooth between events (rms ~2 cm) but 13-16 designed eye snaps per 30 s (the slide's eye
  drop 0.70 m in 0.10 s, the dip 0.15 m in 0.04 s). M1 SHORT (two GATE FAILs). rc2 (w64): p99 9.03, max 22.48 ms, unsynced again (THREADS.md:57
  says VSYNC is "off by default", move54 says "on for new profiles": the default was not checked).
- **Waiting on GG:** eye63 (the eased eye: 64 / 64 events halved, view only 1.00 mm) is built and its blind A / B kit is ready
  (Builds/n64-abx, n64-aby; HANDOFF.md:33, :53); D-2 asks "the picture stuttering, or the camera's jolt? Is your VSYNC on?" Unanswered.
- **Without GG's answer:** only the frame-pacing half can move: a bench VSYNC switch, a Development Player profile of the engine spike, then a
  fix judged by p99 / max / the > 2x share. The camera half is blocked by L-001 / L-105 (eye63 already MET by its own measure); the ride half is
  a sim change GG fenced off. A movement-led night now would likely be another measurement night (w63's M1 was "measured only").

## 5. Open calls GG has not answered that bear on a rooms or a movement night
- **Rooms or movement first** (DECIDE64.md:15-16; DECIDE63.md:18). Today's words delegate the pick ("follow same methods on finding what to focus on").
- **eye63's X / Y and D-2's "which choppy"** (DECIDE64.md:9; DECIDE63.md:46): gate any camera change.
- **D-9 iceline's blue and spire's teal, keep or re-light** (DECIDE63.md:123; T-153): a level rebuild re-lights or not.
- **D-1 the TV fringe on thin lines** (DECIDE64.md:40-43): every thin bright line in a room gets a red edge.
- **fixture61 keep / drop** (DECIDE63.md:10): the floating lamps (gg-proxy #5) in any rebuilt room.
- **T-136 g8 THE ASH PIT revert** (THREADS.md:41), **T-134 footholds or the rule** (:46), **T-144 level 4's ride wall / flyer roof** (:50),
  **T-57 level 6** (:13), **T-139 / T-132 GG plays level 3 and RIDE KILL** (:48, :44); **D-14** a late lesson in the next room or dropped.

## 6. Recommendation: one focus, the rooms, rebuilt for flight on ONE level, judged by play-like evidence
**Why rooms:** gg-proxy #1 two nights running; GG's most repeated ask (seven-plus dated quotes); not fenced by rule 7 (bosses' shapes are
GG's) nor by "don't change the core mechanics" (movement); FORGE64 already named it "an 8-hour level night" (DECIDE64.md:15). Movement
cannot move visibly without GG's A / B (section 4); bosses had w63 + w64 and their biggest problem (the crown's ring, the loom's sphere, no
readable body) is enemy design. **Honest caveat:** the rooms GG last played (e53) drew a decor / colour complaint, not "boring"; the "empty
box" evidence is a stills proxy on verbs37 frames; and nothing built since 2026-10-20 has met GG's hands. A rooms night must end in a play
card he can judge in minutes, with an off switch.
**Shape that avoids the corners:**
- **Scope: one level's combat rooms AND its links, from scratch, under a new level id with an off switch** (5-6 rooms; not w52's 18 in one
  night, not one room). Level 1 THE FOUNDRY is the one GG sees first in every build, is where "waallride section ... awful" came from, and
  its a2 / a4 / a8 and all links were never rebuilt (NIGHT52-REPORT.md:84); level 2 (stall 36.5 %, the worst) is the alternative. Keep its
  teach beats (VERBS.md, nothing lost); reference level 5's rehearsal rooms (GG's praise) and the 2026-10-13 brief.
- **Before geometry (<= 30 min):** each room a card: the one verb it forces and cannot be skipped (ride line, shotgun boost over a blocked
  wall, far dash jump), why it is not exploitable, a job for every piece, the slot measured against the card (L-112), roofed and open where
  the card asks for flight. A card gate PASS before building (L-083).
- **Iteration: >= 3 rounds per room**, each = build -> Player route reel (a route pilot that rides, boosts and pads, T-118's missing reel) +
  room-interior poses (entry / mid / high, a157) -> two fresh persona reviewers on the reel's frame sequence -> re-cut by adding or moving
  pieces with jobs. Planned rebuild budget ~70 % of the box; the close <= 35 min (w64 D-8).
- **MAIN per room (fixed before any number, relative to the old room, L-103):** the forced verb used in N / N route runs; the share of the
  clear spent off the floor; a walk-only pilot slower or failing (the "can't skip it" test). **GUARDS:** the contract, identity I1-I7, deaths,
  stall, put-ats, verbs37 on the level, in view at the door. The bot is a guard, never the verdict.
- **Agents:** three builders split by room files (never one scratch folder), one review slot; commit every slice; an RC always verified.
- **The deliverable for GG:** a 5-minute play card (level 1 new vs `-skygrindOff65`) and the eye63 X / Y in the same sitting, so the next
  night has his answer on both #1 and #2.

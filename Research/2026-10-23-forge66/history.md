# FORGE66 history scout: what NIGHT65 did, where it fell short, what never to repeat (2026-10-23)

Read-only scout for FORGE66 (NIGHT66, 8 hours). Read: docs/memory/{CORE,OWNER,LESSONS,THREADS}.md, ledger/wave-63..65.md, NIGHT65-REPORT.md,
NIGHT65-LOG.md, NIGHT65.md, DECIDE65.md (untracked), HANDOFF.md, Logs/n65/gg-proxy-final.md, Logs/n65/r3b/review-ld.md (last section),
Logs/n65/verdict-*.md (line 1 and FIX lines), Logs/n65/cardgate.md (first lines), Research/2026-10-23-forge65/{history,DECISIONS,redteam}.md,
GOAL.txt, slate-seed.json, Research/2026-10-13-wave38/GG-REQUEST.md, Research/2026-10-14-wave42/GG-REQUEST.md. Measured here: `df -BG /c` at
21:05:59 = **9 GB free**; commit free 21 GB. GG's last recorded play is 2026-10-20 ~14:05: "none of n54-n65 has one" (CORE.md:5). GG's
FORGE66 ask is not in these files (OWNER.md ends with the FORGE65 ask, line 120): the forge holds it.

## 1. GG's words that should steer NIGHT66 (verbatim, most relevant first)
- **Real progress** (2026-10-23 ~13:08, OWNER.md:120, NIGHT65.md:3-4): "follow same methods on finding what to focus on, dont run into corners
  or repeat past mistakes, i wnat real progress and itterations and not just small polishes". Also "need to see real results and it needs to be
  honest and critical where things are not good" (2026-10-22 ~08:1x, OWNER.md:120); "I expect way more building and iteration", "we're taking
  two steps forward and a step backward" (2026-10-11, :107); "treat this as a challenge to make a major leap in quality rather than a collection
  of tiny patches", "Do not spend the majority of the session repeatedly testing tiny changes" (2026-10-16, :111).
- **Rooms that make you use the mechanics** (2026-10-13 ~00:10, Research/2026-10-13-wave38/GG-REQUEST.md:5, one dictated line; OWNER.md:108):
  "it forces you to do a shotgun boost or a dash a far dash jump like I prefer that like I don't mind if they're wide open arenas with a roof";
  "one might encourage the dash jump another 1 I encourage wall riding more and you know they can't be too exploitable you can't like I don't
  want it so you can just dash jump over every single gap and then it's not fun"; "you're not like encouraged to wall ride you're encouraged to
  just start shooting immediately and I don't want that gameplay"; "you should be flying through and and you know killing the enemies before
  you enter the next room in a stylish fashionable way"; "random boxes and and lava that are just randomly placed with no meaning".
- The same ask on other dates: "The rooms still don't incentivize wall-riding or creative use of the weapons; many of them feel very boring"
  (2026-10-09, OWNER.md:74; gg-proxy's predicted first words on n65, gg-proxy-final.md:192); "the arenas don't incentivize creative movement at
  all" (2026-10-11, :95); "you're really struggling to make detailed rooms that really incentivize mechanics", "random lava pits random pillars
  that serve no purpose" (2026-10-12, CORE.md:7).
- **Not just decoration:** "way better arena deisgn for the gameplay. more elements that are not just decoration. research ultrakill, doom and
  other boomer shooters and actually look at real mmodels and images" (2026-10-14, wave42 GG-REQUEST.md:12; OWNER.md:110); "dont just randomly
  plce stuff it needs intension and personality and lets cut fluff and old stuff, dont cut anything meaningful" (2026-10-17, OWNER.md:113);
  "decorative elements in several arenas are poorly executed and lack strong color direction" (2026-10-20, his last play, :116).
- **The ride section:** "the waallride section on level one is awful and relaly difficult and not fun, eneeds to be more balanced" (2026-10-14,
  wave42 GG-REQUEST.md:3); that file's own reading maps it to **THE GANTRY (b2)** (:21), not a3 (section 3, N15). The ride's feel: "it needs to
  feel like speed skating" (OWNER.md:64), rails "slippery and momentum-driven" (:71), "a lot more like Lucio" (:106), "you just stay on forever"
  (:108), "you gain speed too quickly" (:109); "Fix this at the movement-system level rather than designing around a broken mechanic"
  (2026-10-16, :111) against the later "don't change the core mechanics either, but the movement needs to feel cleaner, more fluid, and less
  choppy" (2026-10-20, :116).
- **Rebuild, don't patch:** "every single level needs a complete arena overhaul completely from scratch so don't preserve anything OK that is an
  order" (2026-10-09, OWNER.md:61); "a lot of the rooms to be a lot smaller and more action packed" (:62); "every level should revolve around a
  mechanic and a weapon with a fun, unique boss at the end that's inspired by that level's design" (:76); "six levels total and we just have to
  polish them a ton" (2026-10-11, :104).
- **Bosses:** "i would lead towards bosses, effect colores everything doenst feel alligned, boss animations are too rigid to start, this is one
  of many many things the night prompt hsould focus on" (2026-10-22 ~22:11, OWNER.md:120); "one to two minutes, maybe three if you're slow"
  (2026-10-13 ~14:05, :109); "each boss should have its own distinct color palette and lighting identity" (2026-10-20, :116); level 2's boss
  "more attacks ... more difficult" (2026-10-14, :110). w63-w64 shipped nine boss / colour / text rows (wave-63.md:15-16, wave-64.md:14-15);
  T-154 and T-155 (the boss text: two orders at once, a weapon not held) are open (THREADS.md:59-60).
- **Text:** "way too much instruction at the tsrat of the level" (2026-10-14, wave42 GG-REQUEST.md:6); n65's a2 / a3 / a4 keep their teach strips.

## 2. What NIGHT65 did and where it fell short, by the numbers
- **Branch C all night (the disk):** C: 13.2 GB at t0 against the plan's 20 (DECIDE65 D-1; NIGHT65.md:39), 11.2 at 0:13 (three worktrees, 659-673
  MB each), 9.17 at 2:26, 8.7 at 3:54, 8.3 at 6:00 (NIGHT65-LOG; D-6); about 1.5 GB of the fall outside the night's files, never found (D-6).
  The guard refused removing the night's own worktrees at 2:40 (D-6) and the freeze build, the six-level verbs37 set and the smoke at the close
  (NIGHT65-LOG "Blocked"). No Player ran; nothing was seen in the game (NIGHT65-REPORT.md:32-33).
- **The card gate, three passes** (Logs/n65/cardgate*.md; NIGHT65-LOG 0:29-1:18): pass 1 6 / 6 FAIL (0:45); pass 2 a2, b1 PASS (1:03, 1:10); pass
  3 a5 PASS (1:18); a3, a8, a4 failed three passes and entered round 1 with their lists (D-4). 49 minutes against the plan's 10 (NIGHT65.md:126).
  Real catches: a3's dash-jump skip (Mover.cs:1080-1088, ClipInto :1903-1913; D-5), a4's peek post, the enemies' 2.2 m grid jumps
  (NavGridBuilder.cs:431) where the cards used 0.4 m, depth from the room edge vs the checker's door centre (NIGHT65-LOG 0:45, 1:14). It also
  pushed the DULL aims tighter ("the red already meets DULL <= 23.6", cardgate.md:1), which deepened the GATE's miss.
- **The GATE 0 / 6** (verdict-*.md line 1, 3:51-3:56): a2 DULL 48.7 / 50.3 vs < 18.6 / < 21.1, stall 30.2 / 22.9 vs the red's 12.4 / 11.9; a3 DULL
  A 63.4 vs < 51.8; b1 air 57.9 / 55.9 vs >= 59, DULL 48.5 / 57.3 vs <= 40, stall B 34.4 vs 30.1-33.8; a4 DULL 87.1 / 84.0 vs <= 50, deaths 1 / 1
  vs 0 / 0; a8 skip RED 14.5 vs 23.0 s; a5 the SPOT "mostly empty, with one ramp and a single pad arc". Both critics: a NOT CHECKED Player line
  caps a room at UNSURE (D-8), so no PASS was possible from 0:05. FIX lines: "none that one round is shown to fix" for a2 (:95), a3 (:90), a8
  (:68), b1's MAIN (:83); named one-round fixes only for a4's deaths (:80) and a5's TAP BEAM (:102). **The MAIN cause is the plan's, not only the
  cards':** slate-seed.json's ROOM list says "MAIN ... n46flow air % and STYLE at or above the card's aim" (ROOM[2]), red-team fix 9's wording
  (redteam.md:73-74), while DECISIONS D-3 called the bot a guard; the walk bot never charges, rides, slides, launches or parries (L-150, D-8).
  L-013 said it in w20: "A judge can't measure what its bot can't play".
- **What the sim showed improving** (red -> final; HANDOFF.md:38-41; NIGHT65-REPORT.md:91-93; NIGHT65-LOG 3:19, 4:50): room checks PASS 1 -> 5 / 6
  (a4 FAIL "IN VIEW held"); verb pilots GREEN 1 (r1) -> 5 (r2) -> 6 / 6 (r3 to r5); skip --verbs65 RED 6 / 6 -> GREEN 3 / 6 (a5 22.7 vs 31.7 s, b1
  27.6 vs 39.1 s, a4 17.6 vs 25.2 s with "the refusal never fired", Logs/n65/r5/verbs.md:28); STYLE above DULL with vs without the verb a2 67.3 %,
  a3 35.7 vs 16, b1 79.1, a4 40.2 vs 13; air hand-offs a2 1 (red 0), a3 2 (3), a4 1 (0), a5 2 (4), b1 3 (3), a8 2 (2) (wave-65.md:21-22).
  **Not improved:** a2 / a3 / a8 skip RED identical r3 -> r5 (gg-proxy-final.md:32); a5's pour never took a body (NIGHT65-REPORT.md:94-96); five of
  six rooms clear slower than the old (a8 17.1 / 18.1 vs 6.4 / 5.4 s, a4 33.7 / 26.6 vs 15.1 / 15.2; gg-proxy-final.md:131-136): not "flying through".
- **gg-proxy on the final rooms** (gg-proxy-final.md:176-189): keep over the old room: b1 YES; a3, a5 UNSURE; a2, a8, a4 NO ("standing on THE
  PRESS", "still the old shape ... beside a black pit", "an 8 m wide corridor with no ride"). The level-design review's one weakness at r3b
  (review-ld.md:377-410): the husks stand where the flight arrives but walk off at once, and every verb pilot starts from a standing pose, so
  "the flight's arrival has never printed a contact".
- **The fix and density rounds** (D-8, D-9; NIGHT65-LOG 4:02-4:50): a4 deaths 1 / 1 -> 0 / 0; b1's launch from the rail GREEN, skip 22.7 vs 37.0
  s; a5's round-2 critic FAIL (its two fix pieces were dressing, verdict-a5-r2.md:1). The density round added props with no collider (a2 10 vs 0,
  a3 13 vs 1, a5 13 vs 3, b1 18 vs 3, a8 16 vs 0, a4 11 vs 0) while a3 kept 5 boxes vs the old 12 and a5 lost its 5.0 m plane
  (gg-proxy-final.md:104-118); no skip result moved. D-9's building share was MET at 75 % (188 added, 129 moved, 106 dropped over 49 commits,
  Logs/n65/d9-final.txt) while the proxy says the additions were mostly decoration: a number green while the review said no.
- **Shipped:** nothing. design.n65.foundry65 OFF, slots65.json all old; slate 1 PASS (T1) / 9 SHORT (NIGHT65-REPORT.md:137-140). GG's
  Builds/n65-play-latest is a hard-link copy of n64-freeze with the final rooms (level 62559c9d); **its boot of n65-foundry was never run**
  (read only, Logs/n65/review-n65.md; NIGHT65-REPORT.md:42-43).
- **Tools that now exist** (NIGHT65-REPORT.md:60-81): gen-n65-foundry.cjs + n65-slot-set.cjs (T1 PASS); `bot43: false`; iso65; tour65 (Player-
  unproven, T2 SHORT); `n65verb launch | pierce | parry | skip` (exercise rule 71f6577a; launch --wave / --target 252bffc3); `n46flow --verbs65`;
  air65; the GATE pack (blind A / B by a seeded coin); n65-play-build.sh; n65-red / round / round-verbs / table / blind / d9.

## 3. The corners never to repeat
**Forge65's sixteen** (Research/2026-10-23-forge65/history.md:75-100) still bind: the bot / contract / identity as the verdict; cuts-only re-cuts;
patching a room GG ordered rebuilt; a slate of small polishes that runs dry; checks outweighing building; a slot that cannot fit its card
(L-112); a stills judge in GG's place (L-105); a fix after a PASS unreviewed; a shared scratch folder (L-119); a meter not seen on a frame
(L-108); a doneWhen the baseline fails (L-100); a room change without verbs37 on the shipping build (L-090); a hazard for identity's sake;
more than three builders + one review slot (L-098); disk and commit free unread (L-104, L-128); a builder editing its own checker (L-106).
NIGHT65 hit the first through its MAIN (N1), the fourth after 5:06 (N11) and the disk one at t0. **New, with evidence:**
- **N1 MAIN on an instrument that cannot see the verb** (L-150; L-013 since w20): section 2. Rule: MAIN on the verb instruments (verb pilot from
  the flight, the skip test with its refusal fired, air65), aims set on that instrument's red; walk-bot DULL / stall are guards.
- **N2 A Player-only doneWhen line in a night that cannot run a Player:** the ROOM list kept "verbs37 n65-foundry 27 / 27 on the GATE build" and
  the SPOT on "the same tour65 pose" (slate-seed.json ROOM[3-4]) though the forge saw 14 GB (DECISIONS D-10); both critics capped all six at
  UNSURE (D-8). Rule: 20 GB at t0, or a sim-side PASS path written before t0 (what ships without a Player, and what the play build proves first).
- **N3 Card-gate cycles:** three passes, 49 min against 10; by the third the open items were numbers only built geometry measures (D-4). The
  forge's scouts never handed over ai.grid.jumpUp 2.2 m, the dash-jump's ~25 m carry or ClipInto (grep of research-rooms.md, code-scout.md,
  builder-brief.md: no "jumpUp", no "ClipInto"; research-rooms.md:14-21 gives the dash's 24 m/s only). Rule: a code-facts sheet in the brief
  (L-069), one card pass, its items into round 1.
- **N4 Decoration in place of pieces with jobs:** the density round (section 2). Rule: an addition counts only with a collider and a verb job
  (stood on, ridden, cover on the verb line, a target's perch); dressing is counted apart, and D-9-style shares count only the former.
- **N5 A skip test whose refusal never fires:** a4 GREEN 17.6 vs 25.2 s with "presses refused 0" (L-089, fixed 71f6577a); r5 still prints GREEN
  beside the WARN (verbs.md:28). Rule: a WARN seed is a RED.
- **N6 Verb pilots from a standing pose, not from the flight:** b1's launch "thrown 0.0 m", a8's pierce a floor shot, a3's ride pilot against
  line65's direction (review-ld.md:398-402; NIGHT65-LOG 3:51): "verbs GREEN 6 / 6" did not show the room's line works. Rule: start at line65's
  first waypoint, print what is in reach on arrival; targets that hold their spot (ranged enemies, stand-ins, a wave released by a "reached"
  rect: review-ld.md:407-410).
- **N7 A room's named verb never proven:** a5's pour, five rounds, no body (NIGHT65-REPORT.md:94-96; gg-proxy-final.md:67-72).
- **N8 The red measured on the wrong geometry:** the red's a3 ride GREEN rode a wall inside a2's rect (NIGHT65-LOG 0:30). Rule: every pilot prints
  the room it acted in.
- **N9 Aims finer than the noise:** identical geometry moved DULL about 8 pts (builder S, NIGHT65-LOG 1:29), against aims 2-17 pts from the red.
- **N10 Rounds that stop moving the MAIN:** skip REDs identical r3 -> r5; a8 "none flipped the skip" (verdict-a8.md:68). Rule: two rounds with no
  MAIN move = redesign the room's line or drop the slot, not another polish.
- **N11 The night ran out of work before the close:** no commit between 19:04:19 (5:06) and 19:59:10 (6:01) (`git log`); the builders stopped
  ~4:40, T at 4:58; the leftovers (the links, the bench switch, T-155; NIGHT65.md:129-131) never started (capped "never past 5:15", all T's);
  then 19 minutes waiting for the 6:30 minimum. The same corner as w62 (dry at 0:22) and w63 (4:43).
- **N12 The tag before the clock stop:** u3-n65 at 6:11 (ee2e4839), the stop at 6:30; section 8 and HANDOFF committed after, as the Stop hook
  wanted the close files committed after the close began; the guard refuses moving a tag (NIGHT65-LOG 6:32; L-107). Rule: stop the clock at or
  after the minimum, write the stop line, commit, tag last, chained with `&&` (L-133).
- **N13 Round tools without a dry run:** n65-round.sh hung on `| head -1` (1:31-1:39); r3's skip ran the plain pilot (3:12); the round's levelgen
  report was overwritten by the shipped run (3:51); T1 FAILed on a slots file that existed nowhere (verdict-T1.md:3).
- **N14 Process slips:** five agents alive at 1:43 (stopped); log times ahead of the clock twice (0:30, 2:36; L-123); a2's two round-3 BLOCKING
  lines disagree and the lead must pick one (verdict-a2.md:100).
- **N15 A misattributed GG quote:** the "awful" wall-ride section is THE GANTRY b2 in its own source (wave42 GG-REQUEST.md:21; wave-42.md:5);
  NIGHT65 kept b2 bit for bit (NIGHT65.md:31) and called a3 "GG's 'awful' section rebuilt" (NIGHT65-REPORT.md:47; DECISIONS.md:22). Rule:
  quote the reading's room beside the quote.
- **N16 The worktree trap:** a night's own worktrees (about 685 MB each) cannot be removed (the guard: "GG's call", D-6): count them as permanent.

## 4. What NIGHT66 must decide before it starts
- **The disk** (DECIDE65 D-1, D-6; T-149): 9 GB free now; HANDOFF.md:22 wants 20 GB before any night with a Player; the guard refuses builds,
  reels and smokes under 12 GB; Builds/ ~79 GB and ~235 worktrees are GG's (HANDOFF.md:51-52); DECIDE65's offer ("yes, delete the superseded
  builds listed") has no answer in the file. **If unanswered:** branch C from minute 0, said in the prompt's first lines; no Player line in any
  doneWhen; a sim-side PASS path written before t0 (N2), or work whose proof is sim-side (fingerprints, pilots, harness tests) by design. Do
  not plan another rooms night whose verdict needs a Player it cannot run.
- **D-5, forcing the ride:** a dash-jump carries ~25 m (37 m with a double jump) and slides under any ceiling (D-5); level 1's slots are 14-32 m,
  so no geometry forces the ride. GG's own words: "I don't want it so you can just dash jump over every single gap" (wave38 GG-REQUEST.md:5) and
  "Fix this at the movement-system level" (OWNER.md:111), against the later "don't change the core mechanics" (:116). **If unanswered:**
  Mover.cs stays (CLAUDE.md rule 3); the ride is made the faster, rewarded line by geometry and held targets (N6), proven by the exercised skip
  test; the report says "offered, not forced".
- **GG's play of Builds/n65-play-latest** (NIGHT65-REPORT.md:35-58: b1 first, then a5, a3, then a2, a8, a4). HANDOFF.md:4-6 says the next forge
  should come after it; the report says the next level by the same recipe waits for it (:127). **If unanswered:** gg-proxy stays the only judge
  and does not predict GG (L-105); no second level by the recipe; if the disk allows, the first Player job is to boot n65-play-latest on
  n65-foundry (tour65 + verbs37 n65-foundry), so GG's build is at least known to boot.
- **eye63's X / Y** (Builds/n64-abx, n64-aby; DECIDE64.md:9; NIGHT64-REPORT.md:57-61; T-152): open since w64. **If unanswered:** the camera half
  of "less choppy" stays blocked (eye63 MET by its own measure; L-001, L-105); only frame pacing can move (a bench VSYNC switch and a
  Development Player profile, NIGHT65's leftover 2), and that needs a Player.
- Also GG's, unanswered: DECIDE64 D-1 the TV fringe on thin lines (L-147); DECIDE65's room band 14-32 m; T-150's and T-153's calls.

## 5. Predicted vs actual for the forge65 plan
| forge65 predicted (file) | actual (file) | correction for NIGHT66 |
|---|---|---|
| 20 GB at t0; branch C under 16 GB (NIGHT65.md:39, :73-75); red team 11: the disk runs out | 13.2 GB, branch C all night (D-1) | the disk is a go / no-go before t0, not a branch |
| cards by 0:50 (NIGHT65.md:120) | 0:26 / 0:28 (LOG) | cards take ~25 min |
| card gate 0:50-1:00, ~10 min (NIGHT65.md:126; L-083) | 0:29-1:18, three passes, 49 min; passes 16, 9, 15, 6, 4 min (LOG) | budget ~45 min, or one pass + round-1 items |
| T1 by 1:00, T2 by 2:00 (NIGHT65.md:110-118) | 0:32 (PASS 2:23 after the slots file), 1:02 (SHORT) (LOG; verdict-T1/T2) | tools land early; run their proofs from the main tree |
| rounds at 1:45, 2:55, 4:05 (NIGHT65.md:146) | reviewed 1:41, 2:30, 3:21; round scripts 671 s / 346 s (LOG) | a sim-only round ~50 min; a Player round: not measured |
| reviewers per round (D-5) | level design 19-23 min, gg-proxy 3-5 min (LOG 1:41-2:04, 2:33-2:53, 3:21-3:40) | size review slots from these |
| GATE R ~4:10, S ~4:30 (NIGHT65.md:147) | both 3:42 -> 3:51-3:56: 9-14 min per three-room set (LOG) | ~15 min per set |
| "four or more rooms PASS" (NIGHT65.md:31-32) | 0 / 6 (verdict-*.md) | the instrument decided it (N1, N2) |
| BLOCKING becomes evidence after R3 (NIGHT65.md:62-63; red team 10) | critics failed rooms on the cards' own numbers, re-ran them (wave-65.md:42), ruled NOT CHECKED = UNSURE | the critic holds every number to the card: write numbers the instrument can reach |
| fix rounds by 5:00, rc1 / rc2 by 5:40, two play builds (D-11 (1)) | fix picked 4:12, a5 round-2 FAIL 4:26; no build; one hard-link copy, unbooted (D-7) | a plan for "no PASS" from the start |
| close 6:00-6:15, a 45-min box (D-1) | start 6:00, commit 6:11, stop 6:30 (waiting for the minimum) (LOG) | sim-only close ~11 min; NIGHT64's Player close 16 min (NIGHT64-REPORT.md:160) |
| D-9 building >= 70 % (D-9) | 75 % MET, dressing counted (d9-final.txt; gg-proxy-final.md:104-118) | count pieces with colliders and jobs |
| leftovers until 5:15 (NIGHT65.md:129) | none started; no commit 5:06-6:01 (`git log`) | the slate holds lead-alone work past the last GATE |
| at most four agents (D-6) | five for a minute at 1:43 (LOG) | the cap holds; check before every resume |

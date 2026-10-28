# Retro data: NIGHT55-NIGHT60 (evidence for FORGE61's retrospective)

Read-only extraction, 2026-10-22. Sources: NIGHT55-60 prompts, logs, reports; ledgers wave-55..60; DECIDE55-60; docs/memory LESSONS,
FEATURES, THREADS, OWNER; NIGHT60-BACKLOG / -PROPOSALS; the critics' gate files under Research/2026-10-2x-wave5x/. Short forms:
`N56-LOG:12` = NIGHT56-LOG.md line 12; `-REPORT` likewise; `w57` = ledger/wave-57.md. "bot" = the n46flow / n47human pilot, not a
person. A number with no source is not given; "not in the logs" means I found none.

## A. Every design row turned ON (and every shipped level-data change), with how visible it is

**Count:** 11 config rows ON (N56: 1, N57: 7, N60: 3; N55, N58, N59: none) and 9 level-data changes with no switch (N55 1, N56 4, N59 1,
N60 3). No row reached GG's hands with a play reaction (A-end note).

| # | change | night, commit | measured before -> after | what changes on screen | visibility evidence (quoted) | proposed class |
|---|---|---|---|---|---|---|
| 1 | iceline g7 / g5 re-cut (data) | N55, b224e7bb / 0a82414b (N55-LOG:117-118), in place (DECIDE55 D-4) | I1 mirror overlap 99 -> 52 %, 76 -> 50 % (N55-LOG:117-118) | two iceline rooms' halves differ | gate-r5a: "g5's piece not in its frame (contract 14 / 14)" (N56-LOG:201); P0-g7g5 FAIL (N56-LOG:140-141) | SUBTLE: only the identity meter moved; the critic found no piece in the frame |
| 2 | clear55 room-clear beat | N56, ON 04bef918 (N56-LOG:145-148) | a455 OFF "holds 0 / cards 0" -> GREEN on 5 levels (N56-LOG:190) | the last kill of a room holds 0.5 s and shows "ROOM CLEAR · UNDER PAR / 16.3 S · PAR 30 S" (N56-REPORT:22) | "the card is there ON and absent OFF, seen without being told" (gate-r5a.md:113) | VISIBLE: a text card at every room clear, inside minute one |
| 3 | spawns in view, L2 (data) | N56, shipped despite gate FAIL (DECIDE56 D-27) | in view 1/18 -> 16/18 entry eye, 4/18 strict; bot iceline stall 32.0 -> 36.5 %, foundry deaths 15 -> 21 (N56-LOG:171) | each room's wave 1 spawns where the door sees it | "a3's far enemies are now close, and g2's single tell is now three enemies. kb2 changes less" (gate-r5a.md:47); "the first frame of the room shows the fight" (:50); gate L2 FAIL (:52) | VISIBLE (first room of each level); the kiln part was replaced by kiln57 at N57 |
| 4 | span p5 pad 1 m off the walkway (data) | N56 | not measured: "g8 / p5 contracts and per-piece bot contacts not in evidence" (gate-r5a.md:81-82) | a pad moves 1 m | "p5's 1 m pad move is not in the frame" (gate-r5a.md:67) | INVISIBLE |
| 5 | iceline g8 THE ASH PIT (data) | N56, d33f9feb (DECIDE56:95) | bot g8 deaths 0 -> 7 (N56-LOG:172) | a lava strip along g8's west lane | "g8's pit is a sliver behind the charger's lane" (gate-r5a.md:67); GG-persona: "It's a random lava pit, and it keeps killing me" (:76) | SUBTLE to see, felt as deaths (GG's own named dislike); revert attempt aborted (D-28) |
| 6 | door lamps (data) | N56 | flat verbs37 frames 7 -> 4 (N56-LOG:182) | wall lamps by some doors | "the spire door is still near-black" (gate-r5a.md:100); S1-doors FAIL (:99) | SUBTLE |
| 7 | kiln57: level 3 rebuilt | N57; gen d1885d9a..b4047e43 (N57-LOG:9); in the repo only at N58's 14546f9a (N58-LOG:5-6) | combat rooms 3 -> 5 new; bot clear 92.2 -> 168.2 / 171.9 s; stall 25.5 -> 19.0 %; deaths 8 -> 2 (N57-LOG:10) | five new combat rooms, new identity cards | gate-r3b PASS; "the presses read as plain dark pillars in the entry frame ... the shape of GG's 'random pillars that serve no purpose' complaint" (gate-r3b.md:130-131); "'spawns in view' goal is weaker at the door of the two last rooms" (:120) | VISIBLE (a new level) |
| 8 | wyrm55 (THE KILN WYRM pacing) | N57 D-9 | deaths per fight 1.70 -> 0.80; median 122.6 -> 86.8 s (bot) (N57-LOG:48-50) | a shorter worm fight, fewer barrages | "the row draws nothing new" (gate-r1.md:30); fastest bot kill 45.6 -> 27.0 s, under GG's 1-2 min (N57-LOG:51) | SUBTLE: felt as length at level 3's end; bot-only |
| 9 | tokens56 (enemies attack from view) | N57 D-10, 14430e73 | attack starts inside the cone 44.7 -> 85.0 % (bot) (N57-LOG:11) | out-of-view enemies wait; idle ones walk into view | "no new drawing, tell or text" (gate-r1.md:42) | SUBTLE: behaviour only, no Player frame or person |
| 10 | brief55 (lesson text) | N57 D-14 | vs NIGHT54's text: 3 bad lines + 99 overlap frames -> 0 / 0; vs N56: overlap 1 frame -> 0 (DECIDE57:69-73; N57-LOG:17) | level-1 lesson lines <= 6 words with the button glyph | "vkoff55 shows NIGHT54's text, vkoff the 1-frame overlap" (gate-r1.md:59) | VISIBLE vs GG's last-played text (minute one of level 1); the overlap fix (1 frame, 2.4 px) INVISIBLE |
| 11 | wick55 (THE WICK windows) | N57 D-11, ab3f973b | damage in windows 6 -> 57 % (bot) (N57-LOG:14) | none drawn | "the 6 -> 57 % mostly relabels the bare core as a window. The like-for-like shift is chip 51 -> 43-44 %" (gate-r3.md:162); "The only visible change is the existing OPEN bar state" (:160); no Player pilot climbs the stack (THREADS:14, T-59) | INVISIBLE |
| 12 | grade55 (boss palettes) | N57 D-17 | CROWN tells kept 0.68 -> 0.99; WICK ride cue 13 % cyan (N57-LOG:18) | THE WICK's violet grade and hot wall | "on THE CROWN the row does almost nothing (differs False). Its visible value is THE WICK's ride cue and walls" (gate-r3.md:69-70) | SUBTLE: VISIBLE in THE WICK's fight (a boss, not minute one), INVISIBLE on THE CROWN |
| 13 | colour56 (a colour key per level) | N57 D-17 | key / accent >= 3 %: 22 -> 36 / 36 frames; world over tells 3 / 36 (N57-LOG:16) | coloured wall bands and accent stripes in every room | "new against the OFF frame: lighter lime-olive bands and crimson-rose stripes on both walls" (gate-r1.md:71); "the walls get brighter with the row on; the enemies do not change" (:77) | VISIBLE side by side; unpaired noticing not checked; N60's rim56 halves these strips (gate-b7.md:54) |
| 14 | L0: kb2c spawn re-cut + far-bridge arch (data) | N59, 4fdcf8e6 (N59-LOG:14) | bot put at the next room 14/15/14 -> 1/2/5; deaths 9 -> 9 (N59-LOG:31) | the respawn moves (yaw 45); an arch under the far bridge | "the only change is under the far bridge (an opening below the red slope)" (gate-l0.md:9); "NOT CHECKED: no frame from the spawn exists" (:9); N60 B1: the spawn faces "the west wall ... seen 0 of 5" (N60-LOG:13-14); kb2c "dead" 1.0 -> 3.0 s (gate-l0.md:14) | SUBTLE; the respawn view got worse until #18 |
| 15 | kill55 (the kill wound) | N60, fix 284a4783, ON 3e396a2e (N60-REPORT:39-40) | marks at the flash end 0.42 % OFF -> 15.29 % ON; centre V 0.76 -> 0.03 (N60-LOG:37-38) | a near-black cracked hole on a hit body; chain holds | "a near-black, ragged hole with dark cracks on the victim's pale chest, under the crosshair" (gate-b2.md:23); flash end ON vs old differs on 4.67 % of the whole frame (:18); wound 21.86 % of the body at +0.5 s (:38) | VISIBLE (every marked kill, minute one) |
| 16 | rim56 (enemies over the world) | N60, ba80cfee (N60-LOG:203) | bodies over the world's p90 75.7 -> 90.1 % (N60-LOG:88-89); body V spread 0.50 -> 0.52 (DECIDE60:84-85) | accent strips at ~half brightness; a thin warm edge on bodies | judged on "side-by-side ON / OFF composites and 2x crops" (gate-b7.md:39); "a few points over the run noise" (:37); "Enemies stand out mostly because the strips stop competing with them, not because the enemies glow more" (:56) | SUBTLE |
| 17 | focus60 (Player pauses on focus loss) | N60, f923bf57 (N60-REPORT:44) | probe DONE False -> True (N60-LOG:179-181) | the pause screen on alt-tab or a pop-up | "Alt-tab, or any other window taking the focus during a fight, now opens the pause screen" (gate-b18.md:61) | INVISIBLE in normal play; VISIBLE only on a focus loss |
| 18 | B13: kb2c respawn faces the fight (data) | N60, 33ef339f | wave 1 seen from the spawn 0 -> 5 of 5 (N60-LOG:133-134) | after a death in room 2 you face the middle channel | after: "The player can see where the fight is" (gate-b13.md:66); before: "No enemy is visible" (:68) | VISIBLE, only after a death in level 3's second room |
| 19 | B14: kb3c pocket opened (data) | N60, 612530f0 / bdb2ec0b | NO PATH lines 1012/1591/2171 -> 0/0/0; kb3c deaths 8 -> 0 (bot) (N60-LOG:66-67) | the tap hood and the lava run 2 m shorter | "Nothing I can see changed. Neither pose shows the opened pocket or the shorter lava end" (gate-b14.md:57) | INVISIBLE in frames; bot-only |
| 20 | B15: blow pipe off the exit line (data) | N60, 8419921a | kb3c put-ats 0/1/1 -> 0/0/0 (bot) (N60-LOG:102) | the pad leaves d6b's doorway for the furnace top | "The pad and beam are gone and the lane to d6b is clear" (gate-b15.md:106); "A tall cyan beam rises from the furnace's top edge at frame centre" (:112) | VISIBLE in level 3's fourth room |

Proposed tally: config rows VISIBLE 5 (clear55, kiln57, brief55, colour56, kill55), SUBTLE 4 (wyrm55, tokens56, grade55, rim56),
INVISIBLE 2 (wick55, focus60). Level data VISIBLE 3 (L2, B13, B15), SUBTLE 4 (g7/g5, g8, lamps, L0), INVISIBLE 2 (p5, B14).
Bot-only evidence (no Player frame of the change): wyrm55, tokens56, wick55, B14, g8 / p5's purpose. **No play reaction** to any build
after 2026-10-20 ~14:05 (CORE.md; OWNER.md:118); every ledger reads "Reaction: (none yet)" (w56:41, w57:41, w58:38, w59:33, w60:60).

## B. Hours per night by class

**How split:** the lead's wall clock from t0 (or A0, if it came first) to the last close step, cut at the log's timestamps; each span
goes to its main activity. "Measuring" includes picks, gates, builds for a check, reels, bot runs and critic waits (each pick was
followed at once by a check). Builders worked in parallel worktrees; their windows (spawn -> last commit or report) are in the last
column and are not added to the lead's hours. `~` = the log has no timestamp at that edge.

| night | wall clock | setup | tools / harness | measuring | building (lead) | waiting | close | builders in parallel |
|---|---|---|---|---|---|---|---|---|
| N55 | 17:05:42 -> 22:14 = 5:08 (N55-LOG:3, :9) | 0:45 | 0:18 | 0:43 | 0 | 3:07 | 0:13 | 5 builders, 17:42-17:50 -> 19:33 (~1:45 each) |
| N56 | 23:57:12 -> 06:57:42 = 7:00 (N56-LOG:3, :214) | 0:27 | 0:16 | 3:29 | 0 | 1:11 | 1:34 | 3 builders, 00:17-00:21 -> 04:33-04:38, minus the stop (~3:05 each) |
| N57 | 09:13 A0 -> 13:3x = ~4:20 (N57-LOG:1, :3, :32) | 0:23 | 0:16 | 3:00 | 0 | 0 | ~0:40 | 3 builders, 09:34 -> V 11:4x, G 12:04, L 12:48 |
| N58 | 13:59:29 -> 16:09 = 2:10 (N58-LOG:1, :34); EXT 19:40-20:20 | 0:23 | 0:25 | 0:31 | 0 (EXT 0:24) | ~0:30 | ~0:20 | 3 builders, 14:20 -> 15:12 (~0:50); EXT builder L 19:51-20:20 |
| N59 | 21:11:11 -> 23:1x = ~2:05 (N59-LOG:1, :4) | 0:01 | 0 | 0:51 | 0:01 | 0:44 | 0:22 | L 21:12, V 21:21, G 21:33 -> 22:02 |
| N60 | 23:05:36 -> 07:47 = 8:42 (N60-LOG:3, :224) | 0:17 | 0:58 | 5:19 | 0 | 1:27 | 0:41 | L and V, 23:22 -> ~06:15 minus the stop (~5:50 each) |
| **sum** | **~29:35 (+0:40 EXT)** | **2:16** | **2:13** | **13:53** | **~0:25** | **6:59** | **3:50** | **~42 builder-hours of windows** |

The spans behind each cell:
- **N55:** setup 17:05-17:50 (A0, the A0 smoke 17:07-17:16, six research / critic agents, slate, shared commit, spawns; N55-LOG:53-93);
  tools 18:55-19:13 (pick + the SimHarness build fix 3b71c610; :113-121); measuring 17:52-18:16 guns48 and 19:14-19:33 r1 reels
  (:96-128); waiting 18:16-18:55 for the first builder commit (:110-112) + the usage limit 19:33-22:01 (:5-6); close 22:01-22:14.
- **N56:** setup 23:57-00:24 (baselines, 13 picks, shared commit; N56-LOG:6-29); tools the steal fix 01:26-01:34 (DECIDE56 D-17) and the
  focus-pause function 04:29 (:122); measuring r1 00:24-01:26, r2-r4 01:36-03:09, r4 numbers 04:20-04:29, r5 04:37-05:22; waiting the
  limit 03:09-04:20; close the freeze 05:23-05:37, then **1:10 waiting for the 6:50 floor** with the suite and the docs (DECIDE56 D-30),
  the close table 06:47-06:57.
- **N57:** setup 09:13-09:36 (GG restarted the Editor; red team; shared commit; N57-LOG:32-45); tools the disk-out prune 11:58-12:08 and
  the verbs37 staging fix 12:48-12:52 (:64-66, :76-77); measuring 09:36-11:58 and 12:08-12:46 (shelf, calibration, r1, r2, r3); close
  12:52 -> 13:3x. The report says the night ran to "14:3x" (N57-REPORT:1), but NIGHT58's t0 is 13:59:29 in the same session (N58-LOG:1;
  DECIDE60:63).
- **N58:** setup 13:59-14:22 (~0:09 of it finishing NIGHT57's close: the tag and four rows; N58-LOG:5-7); tools the Core suite + a pin fix
  14:26-14:46 and the 20.7 GB prune (:19-22); measuring 14:47-15:18; waiting an API outage (~30 min: the critic died twice, the smoke
  failed in 582.6 s; :46, w58:34); close to 16:09. EXT: smoke + critic 0:25, the lead's L0 cuts 19:55-20:19 (:50-58).
- **N59:** measuring 21:12-22:19 in five spans (N59-LOG:6-20); waiting 21:17-21:19 (the Editor in Play mode) and 21:5x-22:03, plus 0:32
  of the 1:55 floor spent forging NIGHT60.md after GG's mid-run ask (:24-26); close 22:22-22:34 and 23:06-23:1x.
- **N60:** setup 23:05-23:22; tools B1 23:23-23:42, B12 00:46-01:00, L-113 01:50-02:07, B16 01:52-01:57, the probe timing 05:50-05:58
  (N60-LOG:11, :153, :147, :148, :177); measuring the rest of 23:22-03:00 and 04:01-06:40; waiting the limit ~03:00-04:01 (:118, :75)
  and 06:40-07:06 (no new S after 06:35); close 07:06-07:47 (:217-227). The lead built no game change; task times: w60:48-51.

Reading: measuring is ~47 % of the lead's clock; waiting ~24 % (the usage limit alone 4:40: 2:28 + 1:11 + 1:01); the lead itself
built game changes for ~25 min in six nights; every game change came from builders. N55's builder windows (~8:45) turned 0 rows ON
that night (25 of 26 entries never gated, w55:13-14); their branches were picked at N56 (N56-LOG:21) and clear55, wyrm55, brief55,
wick55 and grade55 went ON at N56-N57. N58 turned 0 ON (w58:28); its EXT cut 4 shipped at N59 as L0.

## C. Lessons added w55-w60 and whether the mistake came back

| L | night | the rule (short) | after it was written |
|---|---|---|---|
| L-098 | w56 | 3 builders + 1 batched critic still hit the limit; resume by SendMessage (LESSONS:102) | **came back**: w60 stop ~03:00-04:01 with 2 builders (N60-LOG:118); w58 critic died on API errors (LESSONS:102) |
| L-099 | w56 | a planted focus steal must take the foreground and say so (:103) | held at w57 (N57-LOG:82), w59 (N59-LOG:23), w60 freeze (N60-REPORT:93); **partly** at w60 B18: "only the first smoke steal took the foreground from the Player" (N60-LOG:208) |
| L-100 | w56 | check each doneWhen line against the A0 baseline (:104) | **came back**: w60 B8's absolute 1.5x MAIN "is mostly the scene's own light (OFF already passes 2 / 5)" (N60-LOG:241-243) |
| L-101 | w56 | a reel's GREEN follows the slate's doneWhen (:105) | not seen again (not in the logs) |
| L-102 | w56 | build the next round while a critic reads (:106) | held w57 (:106) |
| L-103 | w57 | the baseline-relative shipping rule (:107) | held w58-w60 (:107) |
| L-104 | w57 | prune reel frames after every batch (:108) | **came back twice**: w58 n52-freeze's 20.7 GB unpruned, 14 GB free at t0 (N58-LOG:4, :22); w60 C: 15 -> 9.7 GB from a prune pattern miss (N60-LOG:147, L-113) |
| L-105 | w57 | a stills judge does not predict GG; calibrate on played reels (:109) | never re-calibrated: "The judge: still a bot plus an evidence critic" (N58-REPORT:43); open follow-up, not a repeat |
| L-106 | w57 | a builder's checker change gets a fresh critic (:110) | held: the B13 critic re-ran B12's geometry fix (gate-b13.md:58); B3's meter got a fresh review (N60-LOG:62-64) |
| L-107 | w58 | print `git tag -l` and each ON row's enabled before the close ends (:111) | held: w59 A0 (N59-LOG:4), w60 A0 (N60-LOG:7), w60 close step 4 (N60-LOG:227) |
| L-108 | w58 | a simulated view fix is a hypothesis until a Player frame (:112) | **the over-read came back**: w59 halo simulated 70.8 % -> Player 56.9 % (:112; w59:29); w60 B7 estimates 88.8 / 91.9 / 92.2 % -> Player 86.3 / 90.8 / 90.1 % (N60-LOG:73 / :128, :131 / :114, :111 / :88). No row went ON on a simulation |
| L-109 | w59 | one slim critic per entry, started when its evidence lands (:113) | held w60: six critics, 15-40 min each (:113) |
| L-110 | w59 | read a number's `_why` before moving it (:114) | not seen again (not in the logs) |
| L-111 | w59 | a pixel MAIN and the change meet in one range (:115) | fixed w60 (B2: 0.42 -> 15.29 %) |
| L-112 | w59 | check a slot against the contract's size band first (:119) | **partly**: w60 B4 met size but failed the height band ("a 16 m roof capped 32 of 80 runs", N60-LOG:42-44) |
| L-113 | w60 | a copied tool's patterns must match tonight's inputs (:116) | n/a (last night) |
| L-114 | w60 | patch with Edit, never a heredoc (:117) | was itself a break of CLAUDE.md rule 9 and the memory "patch-scripts-not-node-e" |
| L-115 | w60 | never time a planted Player action off the Player's log (:118) | n/a; same family as the memory "Eval latency vs timing" |

w55's "a harness file must build in the DEFAULT SimHarness project" (w55:32) has no L-row in LESSONS.md lines 1-119. Older rules broken
again in w55-w60: L-003 (the limit stopped everyone at w55, w56, w60); L-014 ("the focus pause was pinned only in EditMode since w56; the
first Player probe found it never fired", LESSONS:18); L-087 (B15's pad 2.5 m before kb3c's exit "found by the bot's put-ats, not the
contract", LESSONS:91).

**Usage-limit stops:**

| night | stop -> resume | length | source |
|---|---|---|---|
| N55 | 19:33 -> 22:01 (no building after: past the cap) | 2:28 | N55-LOG:5-6, :129; DECIDE55 D-11 |
| N56 | 03:09 -> 04:20 | 1:11 | N56-LOG:100, :108; DECIDE56 D-23 |
| N57 | none: "No usage-limit stop this time" | 0 | N57-REPORT:7 |
| N58 | none recorded; an API outage ~30 min; the critic died twice (ENOTFOUND 15:28) | ~0:30 | N58-LOG:46; w58:34 |
| N59 | none recorded (not in the logs) | 0 | - |
| N60 | ~03:00 -> 04:01; then a compaction before "Session 3 (05:06)" | 1:01 | N60-LOG:118, :75, :164; w60:51 |

**The Game M hang (MapChoiceTests):** w41 "Game M / N past 600 s twice" (LESSONS:72); N56 "M and N returned no JSON twice each"
(N56-LOG:155-156, :215); N57 the Editor's Pipeline was wedged before the run ("a run_tests stuck ... an orphaned n41-suite runner; GG
restarted it", w57:8-9) and Game M deferred "AFTER THE TAG" (N57-LOG:21); N58 Game N-Z 432 / 434, M not reported (N58-LOG:12); N59 "Game
M suite" SHORT / not checked (w59:26); N60 "MapChoiceTests TIMEOUT after 606 s ... 9 M classes not run; the runner stopped by PID 07:47
(TaskStop left it alive)" (N60-LOG:224). Never fixed; T-147 (THREADS:59).

**The focus pause and the tools:** N55 A0 the smoke failed 19 steps from 62.6 s because "the window lost focus (the six A1-A3 agents
were spawned ~60 s after the smoke started)" (N55-LOG:58-63); N55's planted steals never took focus ("focus changes 0"; DECIDE56:53-57);
N60 B9 a standalone Player never paused on a focus loss (Cursor.lockState stays Locked; N60-LOG:168-172); the probe's steal timed off the
Player log missed its window three times (L-115; N60-LOG:177-179); with focus60 ON a focus loss can pause the Player tours, "run them with
-skygrindNoFocus60 until it is fixed" (N60-REPORT:79-80; THREADS:60, T-146); "An unfocused Editor stalls the first session" (DECIDE60:55).

## D. Said to be bad by a critic or report, still not fixed at N60

| what was said (quote, source) | later nights | state at N60 |
|---|---|---|
| "Rooms show 1-4 of the 7-14 enemies the counter promises" (N55-REPORT:41) | N56 L2 (in view 4/18 strict, gate FAIL); N57 kiln57 "3 of 6 ... kb3c and 2 of 5 in kb3d" (N57-REPORT:29) | unchanged: kb3c 3/6, kb3d 2/5 at N59 (N59-LOG:31) and N60 (gate-b15.md:114); T-134 open |
| "The camera reacts to button presses, not to speed" (N55-REPORT:41) | P1-fov not started N55 / N56 (N56-REPORT:56) | not touched after (not in the logs) |
| bosses "ask for no movement", "boss rules as text", "no swap combos" (N55-LOG:71-72) | not in the later logs | not touched |
| "no boss says what it wants" (N55-REPORT:43) | P1-bossText / P2-brief "not started" (N56-REPORT:56-57) | not touched |
| "The style meter reads DULL in every frame" (N55-REPORT:42) | rank56 retired: "it never changed how often you leave DULL" (N57-REPORT:39) | not fixed |
| "Lights float" (N55-REPORT:42) | P1-fixtures not started (N55-LOG:35; N56-REPORT:56) | not touched |
| "Doors read as a dark screen" (N55-REPORT:42) | N56 lamps: "the spire door is still near-black" (gate-r5a.md:100); N58 exit lamp OFF; N60 lamp60 blocked after 2 attempts (N60-LOG:240-243) | not fixed (T-140) |
| "Yellow is everywhere, not just on what you can parry" (N55-REPORT:44) | yellow56 OFF at N57, N58 (halo 20 %), N59 (56.9 %), N60 blocked after 3 attempts (N60-LOG:237-239) | not fixed; needs a mask (PROPOSALS P1) |
| "a random lava pit, and it keeps killing me" (g8, gate-r5a.md:76) | revert aborted (DECIDE56 D-28) | still in the level; GG's call (T-136) |
| "GG would see no change in the first minute of any of the 7 rooms" (gate-r5a.md:70); identity FAIL a3, g3, g4, v2 (:73-77) | no later night re-cut foundry / iceline / spire rooms (not in the logs) | T-127 "5 rooms left" (CORE.md) |
| "the presses read as plain dark pillars ... GG's 'random pillars that serve no purpose'" (gate-r3b.md:130-131) | not in the N58-N60 logs | not touched |
| "Not checked: whether a person rides onto the 4.5 m tiers in kb2, kb2c and kb3" (N57-REPORT:30-31) | not in the later logs | still not checked (T-139) |
| THE KILN WYRM's best bot fight "27 s ... under your 1-minute floor" (N57-REPORT:32) | GG's call | not touched |
| wick55: "mostly relabels the bare core as a window"; "the stagger window still earns 0 %" (gate-r3.md:157, :162) | Player pilot never climbs the stack (THREADS:14, T-59) | proposal P2 only (PROPOSALS:16-55) |
| kill55: "the camera jitter with it on 0.230 (0.191 the limit)" (N56-REPORT:37) | "a354 never fires kill55 (guard untested)" (N57-LOG:13; N58-LOG:41) | turned ON at N60 on colour guards and verbs37 (gate-b2.md:3-4); a jitter line with kill55 firing: not in the logs |
| THE CROWN's palette: "the row does almost nothing" (gate-r3.md:69) | T-133 "THE CROWN by GG's eye" | open |
| tokens56: iceline enemies idle > 20 %: 18.4 % (N58-LOG:43) | N60 idle60 on a branch: MAIN missed, deaths 6 -> 9 (N60-LOG:93-98) | not shipped (T-145) |
| "Next: calibrate on played reels of the same room" (N57-REPORT:44) | not done (N58-REPORT:43) | open (L-105) |
| levels 3-5 "had 3 combat rooms each" (w57:10) | level 3 rebuilt (N57); level 4's GAP NEST 11 -> 12 / 14, OFF (N60-LOG:41-45); level 5 not in the logs | levels 4-5 unchanged |
| iceline grade: combat frame 2 / 4 (T-104) | measured again N60 B23: "still 2 / 4" (N60-LOG:212-213) | open |
| the N60 review's 6 should-fix items, 0 must-fix (N60-LOG:197-202) | deferred (DECIDE60 D-15) | open (T-146) |

Fixed later (for balance): "no shipped frame shows a kill" (N55-REPORT:42) -> kill55's wound frames (gate-b2.md:23); THE WYRM's "acid
green" turn (N55-REPORT:42) -> grade55's turn at 22 deg (N56-LOG:112); L0's emptier kb2c start ("dead" 1.0 -> 3.0 s, gate-l0.md:14) ->
B13 "3.0 -> 1.0 s" (gate-b13.md:80); kb3c's stall and lava deaths (gate-l0.md:13) -> B14 / B15 (N60-LOG:66-67, :102).

## E. "Done" / "shipped" claims with no Player frame or number behind them

| claim (quote, source) | what was missing | what later showed |
|---|---|---|
| "Your play build: Builds/n57-freeze (tag u3-n57)"; "7 rows ON tonight" (N57-REPORT:9, :14) | the repo: no tag, four of the rows OFF | "`git tag -l u3-n57` printed nothing; the repo config had kiln57 / wick55 / grade55 / colour56 OFF" (N58-LOG:5-6); L-107 |
| "one fix ships (yellow56: VERDICT_PENDING)" (N58-REPORT:4) | a verdict; the row was OFF in the play build ("yellow56 off in its copy", N58-LOG:46) | the same report: "No new row is ON" (:14); the EXT critic FAIL (N58-LOG:52-54) |
| PLAY note: "the yellow on screen should only be the parry tells ... Foundry's lava edge is no longer yellow" (N58-REPORT:36) | the row was OFF in n58-freeze (N58-LOG:46) | describes a change GG could not see |
| "On entering, you face the near bridge ... the fight starts to your left" (N59-REPORT:16) | any frame from the spawn: "NOT CHECKED: no frame from the spawn exists" (gate-l0.md:9); "'w1 in view 4/4' should not be quoted as proof" (:12) | N60 B1: the spawn "faces the west wall ... seen 0 of 5"; "the harness's 'w1 in view 4/4' is wrong" (N60-LOG:13-16); the spawn is the respawn after a death (N60-BACKLOG:66-67), not the entry |
| g8: "it makes the wall ride the faster way through" (N56-REPORT:25) | a ride-time number; "g8 / p5 contracts and per-piece bot contacts not in evidence" (gate-r5a.md:81-82) | only bot deaths 0 -> 7 were measured (N56-LOG:172) |
| "span p5's jump pad moved 1 m off the walkway" (N56-REPORT:24) | a frame or a use number: "p5's 1 m pad move is not in the frame" (gate-r5a.md:67) | not measured later (not in the logs) |
| tokens56: "enemies attack from where you can see them; idle ones walk into view" (N57-REPORT:21) | a Player frame or a person: "no new drawing, tell or text" (gate-r1.md:42) | bot only to N60 |
| wick55: "more of the fight is punishing its open windows, less chip" (N57-REPORT:23) | a Player frame: "No frames, as briefed" (gate-r3.md:152) | no Player pilot climbs the stack (T-59) |
| grade55: "THE CROWN's ring tells now kept" 0.68 -> 0.99 (N57-REPORT:24) | a visible change: "The tells are kept because the grade is close to a no-op on spire" (gate-r3.md:67) | open (T-133) |
| B14: "The corner between the tap hood and the run wall is open on foot now" (N60-REPORT:35) | a frame: "Neither pose shows the opened pocket" (gate-b14.md:57) | bot NO PATH 0 only |
| N56's focus-pause rule, "a pure function + N56FocusPauseTests 3/3" (N56-LOG:122), logged SHORT (:193) | a Player run | N60: "alt-tab never paused a Player (the Editor drops the lock, so only EditMode pinned it)" (N60-LOG:170-172) |

## F. GG's repeated asks (two or more times), with dates

| ask | GG's words, dates (source) | times |
|---|---|---|
| More building, real building | "I'm not seeing enough building" 2026-10-06 (OWNER:10); "I WANT BUILDING" 10-07 (:8); "I expect way more building and iteration" 10-11 (:107); "need to build for next 3 hours" 10-19 (:114); "all relate dto building and itterating" 10-19 (:115); "build not just basic things" 10-20 (NIGHT55.md:7); "start it immediatley, building" 10-21 (NIGHT57.md:12); "need to build for next 2 hours" 10-21 (NIGHT58.md:4); "need real building" 10-21 ~19:40 (w59:4-5) | 9 |
| Follow the best methods | 10-06 (OWNER:21); 10-07 (:8, :27); 10-17 "dont repeat same mmistakes" (:113); 10-19 (:115); 10-20 (:116, :117); "following the best methods using Opus 5.5" 10-20 (NIGHT55.md:4-5); "follow best methods to take learning" 10-21 (NIGHT59.md:4) | 9 |
| Research / study game design / GitHub | "Study game design seriously" 10-06 (OWNER:10); "actually study game theory" 10-09 (:75); "pulling insights from GitHub repos" 10-11 (:97); 10-17 (:113); "high github stars, experts" 10-20 (:116); "Research find areas to iterate and prove" 10-20 (NIGHT55.md:6); "resrecah plan for next 20 mins" 10-20 (NIGHT56.md:4); "study game deisgn ... look at github" 10-21 (NIGHT57.md:4); "resrecah and itterate first" 10-21 (w59:5) | 9 |
| Iterate | 10-19 (OWNER:115); 10-20 "polish and build, itterate" (:116); 10-20 (NIGHT55.md:6); 10-21 "itterated upon" (NIGHT58.md:4); 10-21 (w59:5) | 5 |
| Go beyond the prompt | 10-06 (OWNER:10); 10-07 (:9); 10-20 "work on new things outside of origional prompt" (:117); 10-20 "also go well beyond this prompt" (NIGHT55.md:7) | 4 |
| Levels / arenas with intent | "every single level needs a complete arena overhaul" 10-09 (OWNER:61); "rooms ... don't incentivize wall-riding" 10-09 (:74); "arenas don't incentivize creative movement" 10-11 (:95); "random lava pits random pillars" 10-12 (CORE.md); level 3 "just isn't good ... a new arena" 10-13 (:109); "dont just randomly plce stuff it needs intension" 10-17 (:113); "imporve levels specifically" 10-20 (NIGHT56.md:4); level 3 rebuild "3. yes" 10-21 (NIGHT57.md:10) | 8 |
| Colour direction | "colours are 'bland'" 10-05 (OWNER:37); "the colors ... way too bland" 10-19 (:119); "lack strong color direction"; "each boss ... its own distinct color palette" 10-20 (:118); "colors" 10-20 (NIGHT56.md:4) | 4 |
| Lights and dark doors | lights "placed and floating" 10-11 (OWNER:85); "the lights look crappy" 10-11 (:105); doors "it's like dark" 10-13 (:108); "lighting and flickering bugs" 10-20 (:118) | 4 |
| Kills that feel good | kills aren't "crunchy" 10-05 (OWNER:37); "hits and kills need to feel more satifying" 10-14 (:110) | 2 |
| Enemies (climb, behaviour) | "a route that enemies can climb up" 10-06 (OWNER:14); "struggles to go above floors" 10-06 (:32); "eniemies" 10-20 (NIGHT56.md:4); enemy attack change "2. yes" 10-21 (NIGHT57.md:10) | 4 |
| Bosses (feel, length, colour) | worm asked twice 10-08 (OWNER:60); "not a fan of a lot of the bosses" 10-10 (:77); "still way too tanky" 10-11 (:87); "one to two minutes" 10-13 (:109); the first boss "much harder than it should be" 10-20 (:118) | 5 |
| Honest, critical review | 10-19 ~19:40 (OWNER:114); 10-19 ~22:00 (:115); "honetsly and critically, where are we lacking" 10-21 (:120) | 3 |
| Subagents and skills | 10-07 "skills" (OWNER:8); 10-19 "use skills and roleplay" (:115); 10-20 "spinning up some subagents" (:117); "spin up subagents ... skills everything" 10-20 (NIGHT55.md:4); "use skills" 10-21 (NIGHT57.md:4) | 5 |
| Finish what is not done | "finish whats not done" 10-16 (OWNER:112); "finish and continue anything that it wasn't able to finish" 10-20 (NIGHT55.md:4) | 2 |
| The next prompt as the deliverable | "it should be a loop and a goal prompt" 10-20 (OWNER:116); "build out 7 hour night prompt" 10-20 (NIGHT56.md:4); "create goal prompt" 10-21 (NIGHT58.md:4); "build next 2 hour prompt" 10-21 (NIGHT59.md:4); "next copty pastable goal pormpt" 10-21 (NIGHT60.md:4-5) | 5 |
| Time boxes with a hard cap | "6 hour cap 5 hour minimum" 10-07 (OWNER:8); "hard cap 8 minimun 7" 10-19 (:115); "minimum 2 hours, hard cap at 3 hours" 10-20 (:117); "4 hours minimum 4.5 hours hard cap" 10-20 (NIGHT55.md:7); "7 hour" 10-20 (NIGHT56.md:4); "2 hours 2.25 hard cap" 10-21 (NIGHT58.md:4) | 6 |

GG could not play during these nights: "i cant play it now" (2026-10-21, NIGHT57.md:4); the last recorded play reaction is 2026-10-20
~14:05 (OWNER.md:118; CORE.md).

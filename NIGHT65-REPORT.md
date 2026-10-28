# NIGHT65-REPORT: THE FLIGHT FOUNDRY (2026-10-23, t0 13:58:02; 7 hours)

Skeleton committed at 1:20 (NIGHT65.md rule 11: the report committed by 3:00 and by 5:30). Every claim names its command or file; anything
unchecked says "not checked". **The night ran branch C** (DECIDE65 D-1: C: had 13.2 GB free at t0, under the plan's 16 GB line), so no
Player ran during the rounds: the rooms were built and judged in the sim (the room checker, n46flow, hold52, the ride pilot, the verb
pilots) and on iso65's pictures (Tools/night/n65-iso.cjs), not on Player frames.

## 1. What is still not good (from the GATE, the round-3 reviews and gg-proxy on the final rooms, Logs/n65/gg-proxy-final.md)
- **gg-proxy on the final rooms (after the fix and density rounds), would GG keep it over the old room:** b1 YES ("the old yard's centre
  was the lava strip he named"); a3 UNSURE, a5 UNSURE (refusing the launch costs 11.3 s, but THE POUR is still a lava pit no launch has
  filled); **a2 NO** ("the new room's one line is standing on THE PRESS"), **a8 NO** ("still the old shape, a lane you shoot down from the
  floor beside a black pit"), **a4 NO** ("an 8 m wide corridor with no ride"). The first thing it predicts he says: "The rooms still don't
  incentivize wall-riding or creative use of the weapons" (2026-10-09). A prediction from his words; nobody has played these rooms.
- **The last two rounds moved little.** The skip REDs in a2, a3 and a8 are identical to round 3's; the density round (DECIDE65 D-9) added
  dressing without a collider in every room (a3 13 props against the old room's 1), while a3 and a5 still have fewer pieces you can stand
  on or ride than the old rooms: against your "more elements that are not just decoration" (2026-10-14; gg-proxy-final.md).
- **No room passed its GATE (0 / 6).** Logs/n65/verdict-<id>.md: every room FAILs on the sim side too. Most of it is the cards' own MAIN
  aims, set at 0:50 on the walk bot's STYLE (DULL) share, which the bot cannot move because it never charges, rides, slides, launches or
  parries (a2 DULL 48.7 / 50.3 % against the card's < 18.6 / < 21.1; a3 63.4 on set A against < 51.8; b1 48.5 / 57.3 against <= 40; a4 87.1 /
  84.0 against <= 50). That is a measuring mistake made before the night measured anything (DECIDE65 D-8), but it is still a miss.
- **Walking and shooting still clears half the rooms as well as using their verb.** The verb-aware skip test (round r3b): GREEN in a5
  (22.7 s with the slide-launch vs 31.7 s refused), b1 (27.6 vs 39.1 s) and a4 (17.6 vs 25.2 s), RED in a2 (19.7 vs 18.9 s), a3 (33.7 vs
  24.2 s: the ride is slower) and a8 (23.0 vs 14.5 s). And a4's GREEN is not proof: the critic found its "refused" runs never refused a
  press. GG has asked four times for rooms that make you use the mechanics (gg-proxy, Logs/n65/r2/review-gg.md).
- **The rooms read sparse.** The blind spotter, shown the old and the new a5, called the new one "mostly empty, with one ramp and a single
  pad arc"; a2 has 17 pieces, a4 14 (Logs/n65/gate/d9.txt). gg-proxy's #1 for three nights was "big empty boxes".
- **Flights still hand on less than the old rooms in a3 (2 vs 3) and a5 (2 vs 4)** (air65, Logs/n65/r3b/air.txt).
- **Guards broken on the walk bot:** a2's stall 30.2 / 22.9 % (the red 12.4 / 11.9), a4's deaths 1 / 1 (the red 0 / 0), b1's set-B stall 0.6
  over the red's band.
- **Would GG keep it over the old room** (gg-proxy, round 3): b1 YES; a2, a3, a5, a8 UNSURE; a4 NO. The level-design review: better for
  flight in a2, a3, b1, a8, a4; not in a5.
- **Nothing was seen in the game.** No Player ran (the disk: section 5); every judgement is on the sim and on iso65's diagrams. The rooms
  are a tested guess until GG plays them.

## 2. GG's play card (about five minutes)
- **What you play:** `Builds/n65-play-latest/SKYGRIND.exe`: level 1 is n65-foundry with every room's final file (level sha 62559c9d,
  Logs/n65/latest/n65-foundry.json), **unreviewed: no room passed its GATE** (section 1). Then `Builds/n64-freeze/SKYGRIND.exe` (or the
  play build with `-skygrindOff65`) for the old level 1. In CAMPAIGN pick **LEVEL 1** in the level select: CAMPAIGN from the title resumes
  at your saved next level. The play build shares save data with n64-freeze, so level 1's best and rank start fresh under the new id.
- **How the play build was made (DECIDE65 D-7):** no Player could be built tonight (the disk), so it is a hard-link copy of n64-freeze
  with NIGHT65's config (the level row on) and level file. What differs from a real build: look52's stone tiers do not apply on
  n65-foundry (they are code). **Whether it boots n65-foundry was not run** (the integrated-diff review read the code and says it does:
  Logs/n65/review-n65.md). If it does not boot, play the old level and read the rooms in Logs/n65/r5/sheets/.
- **What to try in each room (its verb):**
  - a2 THE RANGE (KESTREL): climb THE PRESS (cyan mark on its top), charge the KESTREL fully and put one shot through the three stand-ins
    on THE BREECH.
  - a3 THE RUNNER (RIDE, GG's "awful" section rebuilt): run up the ramp, ride THE RUNNER the length of the room, land among wave 2. A
    dash-jump also crosses (DECIDE65 D-5: the ride is offered and rewarded, not forced).
  - a5 THE POUR (CRUCIBLE): slide through the 1.2 m slot under THE TAP BEAM and punch the husk you meet into THE POUR.
  - b1 THE KNOCKOUT YARD (THE YARD): dash or ride along THE WEST RAIL and punch the pack bunched at a gate (the burst); ride the ladle's
    canyon.
  - a8 THE SPILLWAY (PIERCE): ride THE SPILLWAY WALL beside the lane and pierce the file of husks with a charged shot.
  - a4 THE SPITTING GALLERY (PARRY): parry the spitters' globs back at them from the gallery.
- **The one question:** better, worse or the same as the old level 1, and which room?
- **In what order, if you have less than five minutes** (added at the close from Logs/n65/gg-proxy-final.md): b1 first (the one room your
  proxy says you would keep), then a5 and a3 (UNSURE), then a2, a8 and a4 (your proxy says NO). Tell us where the proxy is wrong: that is
  the next night's best evidence.
- **Beside it:** eye63's blind X / Y (Builds/n64-abx, n64-aby; NIGHT64-REPORT.md section 2): which feels smoother, X or Y, or no difference?

## 3. Tools proven
Builder T built eight tools tonight, each picked onto port/stage3 with its saved proof (Research/2026-10-23-wave65/T/):
- **The assembler** `Tools/night/gen-n65-foundry.cjs` and **the slot tool** `n65-slot-set.cjs` (T1): level 1 assembled from six room
  files in slots; `--check` with every slot old equals n52-foundry apart from its names (Logs/n65/check-t1-main.txt: "all PASS (levelgen
  runs: 3)"); unknown keys, drops with no reason and widened rects refused; the slot tool's self-test 5 / 5. **T1: PASS**
  (Logs/n65/verdict-T1.md), after the lead committed the all-old Design/levels/n65/slots65.json (51d3306d).
- **The bot opt-out** (`bot43: false`, T1): the old id scripts drove a8 1393 -> 0, b2 1438 -> 0, a7 645 -> 0 steps; n46flow on n52-foundry
  EQUAL to the flow base; gates all GREEN 14:48:56 with --flow; tests N65 9 / 9 (Logs/n65/gates-t1.txt, tests-t1.txt).
- **iso65** `n65-iso.cjs`: the sim-side picture (two isometric views from cameras fixed by the slot, two elevations, role colours); byte-
  identical reruns; the reviewers' and the spotter's eyes in branch C.
- **tour65** (T2): the reel built, EditMode tests 14 / 14 (Logs/n65/gates-t2.txt); its headless twin GREEN; **T2 SHORT**: every Player
  line not checked (branch C; Logs/n65/verdict-T2.md UNSURE).
- **The verb pilots** `n65verb launch | pierce | parry | skip`, and **the verb-aware autopilot** (`verb65` on a room row, `n46flow
  --verbs65`): the red's skip RED 6 / 6 (every old room clears as fast without its verb); the autopilot found the launch punch dropped by
  NIGHT30's no-melee-mid-shot rule. Harness only; gates GREEN 15:42:52 (Logs/n65/gates-verbs.txt).
- **air65** `n65-air.cjs`: a room's floor-free hand-off graph from the config's movement numbers; planted pair GREEN.
- **The GATE evidence pack** `n65-gate-pack.sh` (blind A / B by a seeded coin, its key kept apart).
- The lead's: `n65-red.sh`, `n65-round.sh` + `n65-round-verbs.sh`, `n65-table.cjs`, `n65-blind.py`, `n65-play-build.sh` (hard-link play
  builds, the source checked unchanged), `n65-refresh.cs`, `n65-d9.cjs`.
- **The critics' three tool findings, fixed by T at 4:58** (87bd9e7e, 252bffc3; gates GREEN): the skip test's exercise rule (a seed whose
  refusal never fired does not count); iso65 draws cyan floor strips and cyan lights in the cyan role (a planted strip proves it); the
  launch pilot's `--wave <n>` / `--target x,y,z`; the GATE pack's open findings read every round's reviews.

## 4. What failed, with numbers
- **The disk, from the first minute:** C: 13.2 GB at t0 (the plan needed 20 for the Player path, 16 for its lean branch), 8.7 GB at 3:54;
  about 1.5 GB of the fall is outside this night's files (DECIDE65 D-6). The guard refused builds, reels and smokes all night (rule 2) and
  refused removing this night's own worktrees ("GG's call"). So branch C: no Player.
- **The card gate:** all six cards FAIL the first pass (Logs/n65/cardgate.md); a2 and b1 PASS on the second, a5 on the third; a3, a8, a4 FAIL
  three passes and enter round 1 with their open items (DECIDE65 D-4). Its real catches: a3's dash-jump skip (a dash-jump keeps 24 m/s and
  slides under any ceiling: Mover.cs:1080, ClipInto), a4's peek post, and the enemies' 2.2 m jump links the cards had not used.
- **The GATE:** 0 PASS, 6 FAIL (section 1; Logs/n65/verdict-<id>.md).
- **The verb pilots:** round 1 GREEN in 1 room (a5), round 2 in 5, round 3 in 6 of 6 (Logs/n65/r1, r2, r3b/verbs.md); the skip test: the red
  0 / 6, round 3 3 / 6 (one of them, a4, not shown to exercise the refusal). Through the fix and density rounds (Logs/n65/r4, r5) the
  numbers held but did not move: a2, a3, a8 stay RED.
- **a5's pour has never taken a body:** the last run, with the new `--wave 2 --slide` from the slot's south mouth, punched while sliding
  and threw nothing (launch36 0; Research/2026-10-23-wave65/T/verb-launch-a5-wave2-slide-4332130b.txt). The room's named verb (the
  launch into the pour) is unproven; the launch that works throws into THE TAP WALL.

## 5. Not checked
- Every Player line of the rounds (branch C): tour65's frames, verbs37 n65-foundry, NRE 0, the SPOT on a Player frame.
- Whether n64-freeze's code boots n65-foundry in a Player (the integrated-diff review read the code and says it does, Logs/n65/review-n65.md;
  not run).
- A parry while riding; a ride's jump-off onto a4's gallery or a8's ledge; a launch into a5's pour (no pilot has put a body in it).

## 6. Your calls (DECIDE65.md)
- D-1 the disk (branch C); D-2 what branch C changes; D-3 a5's slide (offered, not forced); D-4 three card passes; D-5 a3's ride (offered,
  not forced: the dash-jump crosses any gap the room can hold; **your call: should a level-1 room force the ride? only a movement change
  can**); D-6 the disk keeps falling (**your call: free 3 GB or more**; the older builds and the 232 older worktrees are yours); D-7 the play
  builds as hard-link copies of n64-freeze; D-8 every room FAILed its GATE, one fix round for your play build.

## 7. The next prompt's seed (drafted 3:56)
- **Plan vs actual:** cards by 0:50 (actual 0:26 / 0:28) but the card gate took three passes (0:29-1:18), so geometry started at about 1:05;
  rounds reviewed at 1:41 (R1), 2:30 (R2), 3:21 (R3, the GATE round); the GATE 3:42-3:56 (plan 4:10-4:30). Builder T finished eight tools
  by 2:35 (T1 PASS by 0:32, the plan's 1:00).
- **D-9, the building share** (Logs/n65/d9-final.txt, the final rooms): 188 pieces added, 129 moved, 106 dropped over 49 changing commits:
  75 % of the changes added or moved a piece (the hypothesis: at least 70 % building: MET). Rounds reached: three reviewed rounds per room,
  a fix round (D-8) and a density round (D-9).
- **The one change that matters most next time:** a room card's MAIN must be measured by an instrument that performs the verb (tonight's
  verb pilots, the verb-aware autopilot `n46flow --verbs65`, the skip test with its refusal proven, air65), with the aims set against that
  instrument's red. The walk bot's STYLE share is a guard at most.
- **Second:** the targets in the verb's reach at its moment (the level-design review's round-2 finding) should be a card line and a
  pilot: the pilots tonight started at line65's first point and a wave's nearest enemy.
- **Third:** disk before t0 (20 GB, or the night is branch C again); worktree pruning is GG's call (the guard enforces it).
- **Leftovers, ranked:** (1) a Player run of the six rooms (tour65 on n65-foundry, verbs37 n65-foundry) to close the GATE's Player lines
  on the room files as they are; (2) the GATE again on a Player build, with MAIN on the verb instruments (the three tool fixes the critics
  named are done: section 3); (3) the rooms gg-proxy says GG would not keep (a2, a8, a4: "no ride", "still the old shape") and a5's pour
  (no launch has filled it); (4) the links between the rooms (not started); (5) the frame-pacing bench switch and T-155 (not started).
- **The next level by the same recipe:** only after GG has played level 1's rebuild (section 2's question).

## 8. The close lines (NIGHT65-LOG.md "The close" and "Blocked")
- The clock at the close's start: "clock n65: 6:00 / 7:00 since t0 13:58:02; phase BUILD (FREEZE 6:15, CAP 7:00)" (19:58:07).
- **The freeze build, the six-level verbs37 set and the smoke: Blocked** (the guard: "8.3 GB free on the project drive, under 12 GB"); no
  n65-freeze exists, so no verbs37 line and no smoke line for it. The shipped game is NIGHT64's (the row is off); its last verified
  numbers are n64-freeze's: verbs37 27 / 29 / 31 / 31 / 31, the smoke "DONE: all steps passed" (NIGHT64-REPORT.md section 8).
- The final gates (`bash Tools/night/n65-gates.sh --flow`, Logs/n65/gates-flow-close.txt): "gate flow --off65: EQUAL to flow-base";
  golden "0 failing, 0 mismatches" (22 / 22 replays exact); every fingerprint plain and --off57 .. --off65 EQUAL; the Editor compile
  "compilationFailed":false, 0 console errors; **"gates: all GREEN 20:08:14"**.
- The slate: `slate: 10 slices: 1 PASS (passes true), 9 SHORT, 0 BLOCKED, 0 in progress, 0 todo`.
- **The level row**, both config copies (byte-identical): design.n65.foundry65 `{"enabled":false,"levelFrom":"n52-foundry",
  "levelTo":"n65-foundry"}`; **Design/levels/n65/slots65.json**: a2, a3, a4, a5, b1, a8 all `old`. So the shipped n65-foundry is n52-foundry
  apart from its names, and nothing in the shipped game changed tonight.
- The play build: Builds/n65-play-latest (level 62559c9dabd5cdaa, foundry65 on in its config only).
- The clock: `node Tools/night/n65-clock.cjs stop`: "clock stopped at 6:30 since t0 (Fri Oct 23 2026 20:28:23 ...)", inside GG's 6.5-hour
  minimum and 7:00 hard stop. The tag u3-n65 is on the close commit ee2e4839 (6:10); these two lines and HANDOFF.md's close lines were
  committed after the stop, as docs only (moving a tag is refused by the guard, L-107).

# NIGHT66 REPORT: THE FOUNDRY EARNED (8 hours; t0 2026-10-23 22:26:11; branch C, sim only)

The close started at 7:16 (Logs/n66/close-start.txt). Every claim names its command or file; "not checked" means not checked.
**The short version:** the instruments got better: the room skip test's refusal was shown fair, after three critic rounds. No room
reached its GATE. Two of the night's eight hours went to builds, and **4:35 went to two usage-limit stops** (0:20-2:15 and 4:35-7:15,
NIGHT66-LOG.md). The shipped game is unchanged: design.n65.foundry65 is OFF and every slot is old.

## 1. What is still not good
- **No room PASSed a GATE; none was judged.** The second stop hit as round 4 was building. Numbers, all from skip v2 on its PASSed
  version (Logs/n66/r2/verbs-skip.txt and the builders' own trees):
  - a2: `M1 PASS 1.52 vs 1.25 (10/10)` on main r2; M2 PASS. But stall on the verb-aware flow is 33.1 %, against the reds' 12.4 / 23.9, a
    GUARD FAIL. The r2 review says about half of the 1.52 is a tax on the refuser (the 8-kill lesson gives a refuser wave 2 again).
  - a4: `M1 PASS 1.57 vs 1.42` in builder S's round 2 (its own tree; picked into main, never measured there), with THE BELLOWS FIN: a fin
    that blocks 24 / 51 spitter sights and was touched on 7 / 10 seeds. But one walk-bot death on seed 13 fails a GUARD. S's fix was
    uncommitted when the limit hit.
  - a3: M1 1.13 vs 1.25. The spawn move that lifted it puts the skip's start where a first entry never starts (Logs/n66/r2/review-level.md).
  - a5: M1 1.25 vs 1.31 with deaths 1 / 4 (a GUARD FAIL), then 0.80 UNSURE with the deaths back in band. THE POUR has never taken a body.
  - a8: UNSURE 1.03 (4 / 10 seeds exercised). The plain walker beats the pierce, 15.4 s against 18.6.
  - b1: 1.33 vs 1.53 for two rounds; the slot was dropped (DECIDE66 D-3).
- **No collider changed in any room through r2** (the decoration checker on r2: "0 piece(s) added or moved" in all six). Every M1 move came
  from stand-ins, lesson counts, spawns and waves; S's a4 fin (round 2) is the first real piece.
- **The lead's own mistakes:** a second dotnet job beside r2 (`gen --check`, rule 6), and a slot write plus three commits during r2. The
  second failed T1c's line 3 (Logs/n66/verdict-T1c-r2.md). Rule 6 holds even with `--out`.
- gg-proxy was not run tonight (the review slot went to the instrument critics).

## 2. GG's play card (about five minutes)
- **Nothing new to play tonight.** No room PASSed, so no slot is new. The shipped level 1 is n52-foundry's rooms (plus b2 as a slot, still
  old), and the freeze build Builds/n66-freeze is that game with NIGHT66's tool code (section 8). Tonight's rooms exist only as room files
  in the builders' branches (n66-R, n66-S) and on main's Design/levels/n65/rooms/; they never went into a play build.
- **What is waiting for you** (unchanged since NIGHT65, DECIDE65 / DECIDE66):
  - Builds/n65-play-latest: NIGHT65's rooms. Pick LEVEL 1; better, worse or the same as the old one, and which room? (b1 first, then a5,
    a3, then a2, a8, a4; whether it boots n65-foundry was not run.)
  - eye63's blind X / Y (Builds/n64-abx, n64-aby).
  - DECIDE66: the disk (the night never deletes; your commands are listed), the levers (a dash-pip refund, a charger in a8), b2 (a slot
    tonight; a room only on your word).
- **The one question for tonight:** a2 and a4 are the two rooms closest to a PASS. Should NIGHT67 finish them (the guards: a2's stall, a4's
  death) before anything else?

## 3. Tools proven
- **T1a PASS** (Logs/n66/verdict-T1a.md): tour65's GREEN needs unframed 0 and WARN 0, with wedges in Solids (planted RED / GREEN;
  N65TourTests 7 / 7 in the live Editor); `gen-n65-foundry.cjs --run` never writes the shipped level on a levelgen error.
- **T1b PASS on round 3** (verdict-T1b.md FAIL, verdict-T1b-r2.md FAIL, verdict-T1b-r3.md PASS): skip v2, the room test MAIN reads.
  - Three runs per seed: the verb-aware pilot, the same pilot with the verb refused inside it, and the plain walker.
  - The ride refusal is the backstop only. A rideless plant reads identical either way on 10 / 10 seeds, and 42 rides started give 42
    one-step endings.
  - Pierce uses are KESTREL-only. The M1 line names its bar's source, and the GATE pack refuses a null bar.
  - Planted: forced 3.84 PASS, null 0.95 FAIL, no-fire UNSURE.
  - Each fix moved real numbers: a3 1.64 -> 1.13 once the refused runs stopped losing jumps and air control.
- **The reds and the bars** (Logs/n66/red, red65, red65-fix2): mainBar = max(1.25, red65 + 0.20): a2 1.25, a3 1.25, a4 1.42, a5 1.31,
  b1 1.53, a8 1.25.
- **T1c, two critic rounds, FAIL on line 3** (verdict-T1c-r2.md): `ride --shoot`, the decoration checker v2 (planted 8 / 8 and 7 / 7), and
  `--out`. The round assembles into Logs/n66/<tag>/ and never writes the tracked level; r2's tracked-status.txt shows the same untracked
  files before and after. Line 3 failed on the lead's mid-round writes, and T's fix 2 for the table and `marks` (n66-T 2fa9c5a6) was never
  picked.
- **T2 (no critic):** b2 is a seventh slot; `gen --check` all PASS with seven slots (Logs/n66/t2-check-main.txt).
- The close-check (`n66-close-check.cjs --selftest`: 23 cases) and the n66 round tools (dry0, 527 s).

## 4. What failed, with numbers
- The two usage stops: 0:20-2:15 and 4:35-7:15, 4:35 of 8 hours.
- T1b round 1: the ride refusal cut every jump. Round 2: the stick cut cost air control at walls that could never be ridden.
- T1c: test (d) passed a corner light pointing at nothing; the round's iso read the tracked plan. Both fixed; line 3 unproven.
- A main-tree round took 1324-1354 s under three builders' load (dry0 took 527 s alone).
- The rooms: section 1's numbers. Builders R and S each had three rounds of their own; the main tree measured R's rooms twice (r1, r2)
  and S's once (r1, plus r2 on the same files).

## 5. Not checked
- Every Player line except the close's freeze set (section 8). No tour65 in a Player; GG's play build's boot not run.
- a4's 1.57 on main; S's seed-13 fix; R's a5 redesign (DECIDE66 D-2); the a8 skip hang's cause (S killed four hung runs; T's fix never
  landed); the charge-rule fix for a2's stall; whether the game's loader ignores `marks` (only read: JsJson.Parse is generic).

## 6. Your calls (DECIDE66.md)
- D-1 the disk: branch C (7.7 GB at t0). D-2 a5's pour moved to where launches land (unfinished). D-3 b1's slot dropped for tonight.
- Still yours: the disk, the levers (D-12), b2, and NIGHT65's play card.

## 7. The next prompt's seed
- **Finish a2 and a4 first.** a2's stall (T's charge-rule fix: count only bodies the shot's 0.4 m reach crosses), a4's seed-13 death
  (S's uncommitted fix in agent-aeb9fc116ba05ef4d). Then each GATE on a clean main round, then LV if the disk allows.
- **Pick and judge the open tool fixes:** T's table rooms and exact `marks` (2fa9c5a6); the a8 skip hang plus `timeout 900` in
  n65-verbs.sh; one clean round as T1c's line 3 proof, with no pick, commit or slot write during it.
- **The usage limit is now the binding cap:** three builders + one critic + the lead hit it twice. Plan two builders, or shorter turns, and
  put a resume plan in the prompt (L-003).
- Rooms should change colliders, not only waves: the decoration checker saw 0 pieces in six rooms through r2.

## 8. The close lines and the ROW line
- ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
- The slate: `slate: 12 slices: 2 PASS (passes true), 10 SHORT, 0 BLOCKED, 0 in progress, 0 todo` (`node Tools/night/n66-slate.cjs`).
- The freeze build Builds/n66-freeze (72abcfe1; the unchanged game with NIGHT66's tool code): "build exit 0".
- verbs37 (Logs/feel/n66-freeze/summary47.txt):
  - PASS 27 / 27 n52-foundry, 29 / 29 n52-iceline, 31 / 31 n57-kiln, 31 / 31 n52-span, 31 / 31 n52-spire, as n64-freeze; NRE 0.
  - **vn n65-foundry RED 22 / 27** (rows 17, 18, 19, 22, 23: "no grapple node in clear sight"). n65-foundry, loaded while its row is OFF,
    has no campaign place, so the reel gives the grapple, which n52-foundry at place 1 does not own (D-2). With the row ON the rows
    would read as n52-foundry's (not run: the guard refused further reels at 11.6 GB).
  - The forge's close plan listed vn for every branch; it is meaningful only with the row ON.
- The smoke: "[smoke] 234.5s DONE: all steps passed" (Logs/player/n56-smoke-steal-n66-freeze.log).
- The final gates: "gates: all GREEN 06:03:45" (Logs/n66/gates-flow-close.txt: flow --off66 EQUAL, golden 0 mismatches, fingerprints
  EQUAL, the Editor 0 errors).
- **The close-check** can therefore not print a full PASS (its player line needs all six sides). The night ends on section 7's minimum
  close, tagged with `--tag --minimum` after 8:00, because the usage stops ate the close window. The close-check prints its failed
  checks (clock, player) on the line before `CLOSE-CHECK n66: MINIMUM PASS`.

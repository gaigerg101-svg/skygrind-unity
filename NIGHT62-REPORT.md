# NIGHT62-REPORT: THE PROOF SPRINT (2026-10-22, 21:46-22:xx; 2-hour hard cap, branch A)

**Outcome, plainly:** one of NIGHT61's five built features is ON and in tonight's play build: **the guns carry no yellow**
(`design.n61.gunYellow61`). The other four stay OFF, each for a measured reason: two because a blind spotter saw nothing on their best pair,
one because its pair was spoiled by a hint line that changes between runs, one because its own proof reel is RED. NIGHT61's code now has its
first Editor run (all GREEN) and its first Player run (no errors). The list ran out at 0:22, so the close started early.

## 1. What is still not good (gg-proxy's ranking on tonight's frames, Logs/n62/gg-proxy.md; the critic's notes beside it)
Ranked on the P0 build's frames (all five NIGHT61 rows ON in its copy): verbs37 x5 frames f0300 / f0600 / f0900 / f1200 on all five levels,
and the five kiln room-entry shots (c3on-k a157).
1. **The rooms are big boxes with nothing in them** (7 asks): flat walls, blank dark blocks, kiln's presses still plain blocks
   (vk f0300 / f0600, vi f0900 / f1200, vp f0900, kb3-entry).
2. **The movement feels choppy and the wall ride is clunky** (7 asks; a frame cannot show this).
3. **The fight is hidden**: 12-23 LEFT on the counter, 1-5 small enemies far away on screen (kb3 / kb2 / kb2c entries, vk f0900 / f1200).
4. **Instruction text outside level 1, a recharge label that stays up, key prompts on the room name.**
5. **Bosses never say what they want** (no boss frame was in the ranked set; bossRule61 stays OFF tonight, section 4).
6. **Dark, flat-lit views, and a lamp on a red-orange line.**
7. **The style meter sits at D / DULL, the score never moves from 500.**
8. **Enemies read as red glow balls and flat red beans, not pills with visors.**
9. **Kills and hits are not satisfying** (a frame cannot show this).
10. **Colours without direction**: lime floor stripes, a danger-red line on spire's exit, yellow edge lines. The red-orange line through
    spire's exit door is not new: NIGHT60's freeze frame (n60-freeze vp f0300) shows the same line.
Gone from NIGHT61's start ranking on tonight's frames: the yellow on the guns (ON tonight), the stacked text on kiln's room entries and the dark
exit door (both seen with their rows on in the P0 build only; those rows stay OFF, section 4).

## 2. What you can see tonight
- **No yellow on the guns** (`design.n61.gunYellow61`, ON in both config copies at 6aa16a71; one-row switch `-skygrindNoGunYellow61`).
  Frames: `Logs/feel/n62-p0/c3on-k/a157/shot-N29-L57-n57-kiln-kb2c-entry.png` (on: the THUNDERCLAP's top slab and sight cyan) vs
  `Logs/feel/n62-p0/c3off-k/a157/shot-N29-L57-n57-kiln-kb2c-entry.png` (off: yellow). Critic PASS (Logs/n62/verdict-P5.md): yellow on the
  viewmodel 0 in all four poses on, 17597 / 11273 / 4899 / 2390 px off; the spotter, blind: "The gun's top panel and arrow marker are bright
  yellow in B, but cyan in A." The proof lines recolour the shotgun, the bouncer and the torch to #19D3E6.
  **PLAY:** pick up the THUNDERCLAP (level 3) or the TORCH (level 5) and look at the gun: its panels and sight are cyan now, not yellow.
  Yellow on screen should now mean a parry tell, not your own gun. Keep or drop: DECIDE62.md.

## 3. Bugs fixed, tests proven and dead code checked
- **NIGHT61's code in the Editor for the first time, all GREEN** (Logs/n62/editor-tests.txt, 21:50-21:53): the eleven N61 classes 74 / 74
  (every NIGHT61 row's tests), FocusAutomation61Tests 36 / 36, and the eleven classes NIGHT61's dead-code batches touched: Ui43Tests 11 / 11,
  Hud30Tests 3 / 3, FirstFifteenVerbsTests 1 / 1, Arcline22Tests 5 / 5, Health42Tests 3 / 3, Style42Tests 4 / 4, Stamp22Tests 5 / 5,
  RunSnapshotTests 8 / 8, FingerprintTests 6 / 6, MenusTests 20 / 20, MenuMotion18Tests 14 / 14.
- **The Player with all five rows on plays as before:** verbs37 x5 27 / 29 / 31 / 31 / 31, RED rows none, 0 NREs, 0 exceptions (n62-p0;
  NIGHT60's freeze numbers).
- **B1 (focus60 never pauses an automation run), P6: PASS** (Logs/n62/verdict-P6.md): the probe paused with the row on ("paused on the focus
  loss True"), not with it off (False), and not with the planted no-pause flag (False); the smoke with two planted steals that took the
  foreground: "[smoke] 232.5s DONE: all steps passed", "focus changes 1".
- **D1 (370 lines of dead code removed by NIGHT61), P7: PASS** (Logs/n62/verdict-P7.md): its eleven classes GREEN, verbs37 unchanged on both
  builds, the smoke's TITLE steps pass.

## 4. What failed, with its numbers
- **P1, the exit door frame (exitFrame61): SHORT.** Proof right (8 `[exit61] door lit: room` lines on, 0 off), and by eye the far door's
  cyan frame is brighter on. But the blind spotter named the hint line under DOOR OPEN ("Boost: SPACE, aim down, LEFT CLICK twice" on vs "At
  the top, press LEFT CLICK again" off): two runs of the same build and pose are not identical (the kb3 entry's nearest enemy 15.3 m vs
  15.2 m), so the clear shot caught a different lesson. The pair was dirty, not the row; the row stays OFF by the rule.
- **P2, every boss says what it wants (bossRule61): SHORT.** Its own proof reel is RED on THE KILN WYRM: "no line at 1.0 s (alpha 0.00)".
  The line does show ("Swing the gauntlet LEFT CLICK at its charging head.", 8 words, button glyph, 40 px at 1080p, 4.77 s on screen) but it
  rises at 2.45 s, after the boss's name card, and the doneWhen asks for it at 1.0 s. The other four bosses were not filmed.
- **P3, one line of text at a time (text61): SHORT.** The spotter: NOTHING OBVIOUS on the kb2c entry pair. In this run no instruction stood
  under the room's name at kb2c even with the row off, so the stacked-text pose of NIGHT60's frames did not recur. Measured beside it: no
  instruction line was shown twice in any verbs37 run (outside level 1: 2 / 10 / 2 / 1 shown).
- **P4, lights hang from real fixtures (fixture61): SHORT.** The spotter: NOTHING OBVIOUS on iceline's most-changed entry pair (g4, 19242 px
  changed). Proof right ("ceiling lights 10, housed 10, held by a beam 10, unmounted 0"); by eye the top light has a housing on, a bare panel
  off: too small against a dark ceiling at 1080p, as its builder warned.

## 5. Not checked
- The other four bosses' rule lines (P2 cut after the wyrm's RED); P1's own-gain meter (its s1off reel was not run after the SPOT); P4's
  other four levels and its flicker guard; MapChoiceTests (P8, a later night).
- Why two reel runs of one build and pose differ (where the nondeterminism enters); the third T-146 item (the cursor-never-locked pause).
- GG's own play of tonight's build.

## 6. Your calls (DECIDE62.md)
- **Keep or drop** the guns without yellow (ON tonight).
- **P2:** the boss rule line works but rises at 2.45 s after the name card; should it come before the card (a code change), or is 2.45 s fine
  (the doneWhen re-written)?
- **P1, P3, P4** stay OFF: a re-judge needs clean pairs (P1, P3) or a bigger fixture (P4); or drop them.
- The parked threads (T-133..T-147) and the disk (37-44 GB free tonight; Logs/ 200 GB, Builds/ 70 GB).

## 7. The next prompt's seed
- **Estimate vs actual** (the plan's minutes were NIGHT60's): gates 1:27 -> 2:00; the P0 build 0:33-1:16 -> 1:45; verbs37 x5 ~7:00 -> 5:08;
  a reel 63-121 s -> 30-65 s; a GATE 17-23 min -> 3:18 (P5; a one-row brief with every file named and the meters pre-run); a spotter -> 5-13 s;
  gg-proxy 15 min budget -> 2:52; the flow base ~5 min -> 9:11 (beside the build and reels); the Editor batch 13 classes -> 2:53. The
  evidence phase was planned at 0:10-1:05 (55 min) and took 0:04-0:22 (18 min, 17 reels instead of up to 35): SPOT first cut 18 reels and four
  GATEs. Ratio about 3x, as APEX S19 measured.
- **Leftovers, ranked:** (1) make a reel's on / off runs identical (or capture on and off from one run), then re-SPOT P1 and P3 on clean
  pairs; (2) P2's timing call (above), then the five bosses' reels; (3) P4 made visible (a bigger, lit housing) or dropped; (4) P8,
  MapChoiceTests alone in async mode; (5) T-146's three items as code; (6) R1's small fixes (N1, N3, N5, N11, N12; Logs/n62/R1.md).
- **Threads aged two nights or more:** T-133, T-134, T-135, T-139, T-142, T-143, T-144, T-145, T-146, T-147 (parked in DECIDE62.md for GG).
- **Three research questions:** (1) how do game teams make a recorded capture bit-identical across runs (fixed timestep plus input replay,
  seeded effects) so an on / off pair differs only by the switch; (2) how to choose a "would a player notice it" pair so run-to-run noise
  (hint text, pickups, particles) cannot decide it (one-run toggles, masked HUD); (3) what makes a small lighting change noticeable in
  normal play (contrast, size, motion) before it is built.
- **The commands that worked, as run:** `bash Tools/night/n41-build.sh n62-p0`; `node Tools/night/n46-build-rows.cjs n62-p0 on n61.gunYellow61
  n61.exitFrame61 n61.text61 n61.bossRule61 n61.fixture61`; `bash Tools/night/n62-reels.sh n62-p0 <sides>` (one set at a time, in the
  background); `node Tools/night/n62-spot-pair.cjs <id> <on> <off>` then a fresh skygrind-spotter with the two paths; `python -I
  Tools/night/n62-gun-yellow.py --pair <on> <off> [--rect 0,0,1,1]` (also picks a SPOT pair by the most changed pixels); `bash
  Tools/night/n60-focus-probe.sh n62-p0 on | off -skygrindNoFocus60 | planted -skygrindFocusProbe60NoPause`; the Editor batch `for c in ...;
  do bash Tools/unity-loop.sh tests $c; done` in the background; `node Tools/night/n62-freeze-rows.cjs "n61.<row>=<reason>"`.
- **The forge experiment (D-23, the cap held by the guard):** the close started at 0:22 by LIST EXHAUSTED, not by the clock; the guard
  refused nothing during the run (Logs/guard-blocks.log: its only clock lines are the forge's planted tests at 20:46). The close's end: (8).

## 8. The close (NIGHT62-LOG.md "The close"; started 22:08 = 0:22 by LIST EXHAUSTED)
| step | result | evidence |
|---|---|---|
| (a) GATEs due | none alive; no flip after 22:08 | NIGHT62-LOG.md |
| (b) `n62-gates.sh --flow` | "gate flow --off62: EQUAL to flow-base" and **gates: all GREEN 22:16:12** (config, golden 0 mismatches, 22 replays exact, fingerprints plain / --off57..--off62 EQUAL, Editor 0 errors) | Logs/n62/gates-final.txt |
| (c) n62-freeze | built 22:17 from 6aa16a71 (gunYellow61 ON); drift GG's files only | Logs/n62/build-freeze.txt, Builds/n62-freeze |
| (d) verbs37 x5 on n62-freeze | 27 / 29 / 31 / 31 / 31, RED rows none, NRE 0, exceptions 0 | Logs/feel/n62-freeze/summary47.txt |
| (e) the smoke | "[smoke] 232.5s DONE: all steps passed"; two planted steals took the foreground; "focus changes 1" (run 22:25-22:29, after the FORGE63 red team ended: no agent alive) | Logs/n62/smoke-freeze.txt |
| (f) the slate | "slate: 8 slices: 3 PASS (passes true), 5 SHORT, 0 BLOCKED, 0 in progress, 0 todo" | Logs/n62/slate.json |
| (g) L-107, tag | gunYellow61 `enabled true` in both config copies, the other four n61 rows false (printed); **u3-n62 points at 1b0696b5, one commit before the close commit cea57d63**: a failed first commit with the tag chained by `;` (L-133); the game state is the same as n62-freeze; moving the tag is refused by the guard and is your call (`git tag -f u3-n62 cea57d63`) | NIGHT62-LOG.md |
- **The play build for GG: `Builds/n62-freeze/SKYGRIND.exe`.**
- The forge experiment's end number: the close ended well inside the cap (about 0:45 of 2:00); the guard refused nothing during the run.
- Not done in the close, on purpose: the memory harvest of w62 (left to the next forge, FORGE62b D-24; FORGE63 put it in NIGHT63's leftovers).

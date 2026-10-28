# NIGHT50 REPORT (for GG)

**Outcome first.** The charge bar is back under the reticle, and it won its blind pair 3-0 over NIGHT46's placement. The charge also shows on the
gun in four steps, on KESTREL and THUNDERCLAP (PULSE is off: its steps were hidden inside the fist). The new grade is on for two levels only,
foundry and spire; the other three stay as they were, because their meters or their saturated band failed. The impact kit is on, but the
enemy kill's wash is still just over the bar (+0.27 against +0.25 on two frames). The smoke failed on four gamepad steps, and NIGHT49's build
fails the same four steps in this session, so this is the connected controller, not the code. Verbs37 passes on all five levels with the
same counts as NIGHT49 (26 / 20 / 25 / 30 / 30).

**Play build: `Builds/n50-freeze/SKYGRIND.exe`** (commit 22adc1d9; the test fixes after it, 8ec0a1c0, touch no game code).

## What to look at first
1. **The charge bar (G1, GREEN on the pair).** Frames: Logs/feel/n50-r2/h1080/a150/shot-N30-H50-on-50-1920x1080.png (the new bar, 50 %) and
   shot-N30-H50-old46-50-1920x1080.png (NIGHT46's bar at the same pose). The bar sits about 30 px below centre, in the usable cyan.
2. **The gun's charge steps (G2, KESTREL and THUNDERCLAP).** Frames: Logs/feel/n50-r2/h1080/a150/shot-N30-H50-glow-revolver-{50,100}-1920x1080.png.
   Two of four segments at 50 %, all four at 100 %; the gun does not move. PULSE is off (its glow sat inside the fist's block).
3. **The grade (G3, foundry and spire only).** Frames: Logs/n50/sheet/sheet.png (before and after per level). On foundry and spire the new grade
   is a strong teal and yellow-green. Judges called it TIE against the old look, and I'd call it a bigger shift than TIE suggests. Your call:
   keep it, or pull it back to the old look on those two levels (one line in the config).
4. **The kit (G4, SHORT).** Every tier but the kill passes its wash bar. The kill peaks at +0.27 with the kit on (one frame; another sits at the
   +0.25 bar) and +0.28 with it off. I tried the heavy tier's size and brightness and neither moved the kill's wash, so the source is elsewhere;
   those tries are reverted. The death-burst scale (0.5) stays in the freeze: it moved the kill from +0.28 to +0.27, not enough.

## Shipped by default (Builds/n50-freeze)
- ON: charge50 (the bar under the reticle), gunglow50 (KESTREL and THUNDERCLAP steps), fx49 (the kit, kill SHORT), look49 on n48-foundry and
  n47-spire, lights49 (NIGHT49's), and NIGHT49's and NIGHT47's rows.
- OFF: look49 on n47-kiln (its meter fails, 2 of 4), n47-iceline and n47-span (the saturated band fails; one attempt each did not pass);
  PULSE's steps.
- The switches: `-skygrindOff50` (the Player) or `--off50` (the harness) gives NIGHT49's game.

## Rows (the close table is generated from Tools/night/n50-rows.json by Tools/night/n50-close.py)
| row | state |
| --- | --- |
| G1 charge bar | GREEN on the pair (3-0) and the frames; the Player RED run was not run |
| G2 gun charge | SHORT, fallback shipped (KESTREL and THUNDERCLAP; PULSE off) |
| G3 grade per level | SHORT, fallback shipped (foundry and spire on; kiln, iceline, span off) |
| G4 impact kit | SHORT (kit on; the kill peaks +0.27 on one frame, bar +0.25) |
| G5 lights and verbs | GREEN: verbs37 26 / 20 / 25 / 30 / 30, RED rows none, NRE 0 |
| G6 before / after sheet | SHORT: the grade-only sheet is built (2 of 5 changed); the full sheet was not built; not judged |

## What was not verified
- **The smoke did not pass.** Four gamepad steps fail on the freeze build (virtual pad, virtual pad menus, pad only, grind22). The NIGHT49
  control build fails the same four steps right now, and this session has a gamepad connected. The smoke must be rerun with the controller
  unplugged. I could not do that from here.
- **The flow table (n47-flow-table) was not run** on this night's build.
- **The G6 full before / after sheet was not built** (only the grade-only sheet, Logs/n50/sheet/sheet.png).
- **The smoke's cause** is probable, not shown: a 'GAMEPAD CONNECTED' toast is in the reel frames, but no device line names a gamepad.
- **G1's Player RED run was not run** (the off state is the reel's own forced off in the same run). G2's frames at 0, 25 and 75 % were not
  viewed by the lead (50 % and 100 % were).
- **STITCHER heat on the arc (G1)** was not checked. The grayscale audit (G3) is logged, not gated.
- **The G6 sheet** compares the grade only (the combat frames); the charge and the kit are not in those frames, and it was not judged.

## Numbers
- Golden 22 / 22 exact (0 mismatches); fingerprints plain, `--off49`, `--off50` EQUAL (freeze commit 22adc1d9).
- Verbs37 on n50-freeze: foundry 26/26, iceline 20/20, kiln 25/25, span 30/30, spire 30/30; RED rows none; NRE 0 on each.
- Judges (fresh subagents, both orders, 3 judges per pair, rubric in Tools/night/n50-rubric.md): controls PASS (same frame TIE, 6 px shift
  TIE, the sharp frame beats the blur 3-0). NIGHT49's pairs re-judged: the H pair (arc40 vs c3) went TIE; the kit held 3-0; the grade held
  on kiln (2-0) and flipped on spire (TIE) and iceline (2-0 for the grade, but its meter failed).
- Blind pairs this night: g1-charge (re-placed bar vs NIGHT46) NEW 3-0.
- Suite: see the close table's final line (the batch run on 8ec0a1c0).

## Forge experiment (the one change this night)
Calibrated judges, a default per row, and one fallback each. Measured at the close: rows that ended shipped (ON) out of G1-G5: G1 yes, G2 yes
(two of three guns), G3 yes (two of five levels), G4 yes (with the kill SHORT), G5 yes. Last night 1 of 5. The calibration changed what we
trusted: the NIGHT49 pair re-judge flipped five of its eight verdicts (DECIDE50 D-3). The G3 default changed on the meter (kiln off), not on a flip.

Logs and frames: Logs/feel/n50-r1, n50-r2, n50-freeze; Logs/n50/judge-keys/ (keys), Logs/n50/judge/ (the pairs), Logs/n50/sheet/. The tag
`u3-n50` marks the close.

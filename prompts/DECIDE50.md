# DECIDE50 (the lead's calls for NIGHT50; GG did not answer. Each call uses the row's default and fallback in NIGHT50.md section 1.)

Kickoff t0 15:43 MST, 2026-10-19. Hard cap 19:43. Close starts by 19:15.

## D-1 The judge protocol (NIGHT50.md section 0; PROMPT-RESEARCH.md B)
- Judges are fresh subagents (model sonnet, chosen for cost on a burst of judge calls; the calibration is what checks them, not the model).
- Three judges, each in both orders: one subagent call per (judge, order). Each call judges every pair in its order, so a judge sees each pair
  once per call, and the two orders are separate calls.
- A pick counts only when both orders name the same frame (`Tools/night/n50-pair.py tally`); else TIE. A pair wins when at least two judges pick it.
- The question and the three criteria: `Tools/night/n50-rubric.md`. Judges were told not to open the key files. That is an instruction, not enforced.
- Content check before judges see a pair: `Tools/night/n50-pair-check.py` (not black, not blank, same size, key present). Every label PASSed.

## D-2 P0: calibration PASS (before any look)
- Controls from NIGHT49's arc40 frame (`Tools/night/n50-control.py`), anonymous labels cal-1..cal-3:
  - cal-1, the same frame twice: TIE (3 of 3 judges, both orders). Required: TIE.
  - cal-2, the frame moved 6 px: TIE (one judge picked B in one order and TIE in the other; the protocol gives TIE). Required: TIE.
  - cal-3, the sharp frame against a blurred copy: the sharp frame wins (3 of 3, both orders consistent). Required: the sharp frame.
- So the protocol can return TIE for identical frames and a loss for a plainly worse frame. Judges are usable.
- Caveat: the controls are 480 x 480 crops from NIGHT49's frame. A judge's 6 px read is the weakest point (one judge picked B once).

## D-3 NIGHT49's deciding pairs, re-judged through the protocol (from the saved frames, before anything was built)
| pair | NIGHT49 verdict | NIGHT50 verdict (both orders) | flip? |
|---|---|---|---|
| rej-1 H: charge candidate c3 vs arc40 | arc40 won 3-0 | TIE (judges: TIE, old, TIE; one judge for arc40, two TIE) | **FLIP** (arc40 no longer beats c3) |
| rej-2 L: lights on vs off, n47 kiln | tied | TIE | holds |
| rej-3 V: grade on vs off, n47 kiln | kiln won | new 2-0 | holds |
| rej-4 X: kit on vs off (the x49 kit pair) | kit won 3-0 | new 3-0 | holds |
| rej-5 V: grade on vs off, n47 spire | new won (sheet) | TIE | **FLIP** to TIE |
| rej-6 V: grade on vs off, n48 foundry | old won (sheet) | TIE | **FLIP** to TIE (not a loss) |
| rej-7 V: grade on vs off, n47 span | old won (sheet) | TIE | **FLIP** to TIE |
| rej-8 V: grade on vs off, n47 iceline | old won (sheet) | new 2-0 | **FLIP** to new |
- The NIGHT49 sheets are not the same test as these pairs: NIGHT49 judged one sheet per row; NIGHT50's pairs are one level each, both orders.
- Effect on the defaults: H (G1) default stays arc40 re-placed (GG asked for it; no candidate beats it). G3 (grade) default changes for iceline (see D-4).

## D-4 G3 defaults: look49 levels
- Rule (NIGHT50.md section 1): ship a level's grade only if its meter is in band AND its pair is won or tied (the grayscale audit comes with V's round).
- Meters: NIGHT49's numbers (not re-measured this night, not checked again): foundry and spire pass; kiln and span near; iceline short.
- Pairs (NIGHT50): kiln won (new), iceline won (new), foundry TIE, spire TIE, span TIE.
- Default ship: `look49.numbers.levels` = [n48-foundry, n47-spire, n47-kiln] (foundry and spire meter-pass with a TIE; kiln won).
- Iceline: the pair now won, but its meter failed. One attempt (V, round 1): lower saturation lift, darker floors, accent >= 20 deg from role hues.
  If it does not pass its meter and its pair again, OFF.
- Span: near meter, TIE pair. One attempt as well (the same recipe). OFF if it does not pass.
- Implementation: the look49 row stays a bool (enabled: false until the freeze). The list is read per level. The freeze sets enabled true
  and the list. Empty list = off everywhere. That is what the shared commit already has (the switch is `enabled` plus `numbers.levels`, not the
  list alone; NIGHT50.md's "the row is ON when the list is non-empty" is not used, because the night's checks read `enabled`).

## D-5 G1 default: the charge bar
- Ship arc40 re-placed outside the 18 px reticle box (GG asked for the bar back, 2026-10-19 ~11:20). No new element.
- Fallback if the re-placed bar loses its pair to the old placement: ship the old placement.
- NIGHT49's c3 candidate is not shipped (it was never the default). Its flip (D-3 rej-1) is recorded, not acted on.

## D-6 No X builder this night
- The impact kit (fx49) exists and its pair won 3-0 (rej-4). The one remaining X edit (the enemy kill's web death burst) is the lead's, per
  NIGHT50.md section 1 (G4: "the lead (not X) edits the web death burst"). A builder with no open file would only add a round.
- Two builders instead of three: H (charge bar and gun) and V (grade per level and iceline / span attempts).

## D-7 G4: the kill wash
- The lead edits the enemy death burst in `SurvivalGame.Init.cs`, behind fx49 only, sizes and alpha only, never an enemy colour.
- Fallback (if the wash stays above +0.25): fx49 ON with the kill's old path behind a per-effect gate.

## D-8 G2 scope
- Four steps on KESTREL, THUNDERCLAP and PULSE per NIGHT50.md. The Railcannon / ULTRAKILL research supports a gun-side read; the fallback
  (KESTREL and THUNDERCLAP only, PULSE OFF) ships if PULSE cannot pass its own frames in round 1.

## D-9 Rounds and the clock
- Builders H and V: round 1 by 17:00 (frames and meters), round 2 by 18:20 (fixes and fallbacks only). No round 3.
- Lead: integrate, build `n50-r1` (a Player build, one reel script), look at every frame, log the path.
- Quiet freeze 18:30-19:15: builders stopped; `n50-freeze`; verbs37 x5; smoke; full batch suite; flow table (n50-flow-table.sh if absent).
- Close 19:15-19:43: reviewer subagent on the close table; NIGHT50-LOG close table; NIGHT50-REPORT.md; HANDOFF; tag u3-n50.

## D-11 Round 1 results (n50-r1, 16:24-16:30) and the calls they forced
- G1 charge50 (arc40 re-placed, radius 46): the reel shows the bar 22.5 px from centre at 1080p (box 18 px), 10 px tall; the blind pair
  (re-placed vs NIGHT46's placement, 3 judges x 2 orders) went 3-0 for the re-placed bar. GREEN candidate (pair won).
- G2 gunglow50: KESTREL and THUNDERCLAP read in the reel (steps 0 / 25 / 50 / 75 / 100 %). PULSE failed (its steps sit inside the fist's
  dark block; 0 % and 75 % read alike). PULSE OFF in code (commit 9bd04af8), the D-8 fallback.
- G3 look49 per level (n50-r1 a349, the meter floor): n48-foundry 4/4, n47-spire 4/4 (both ON); n47-kiln 2/4 (mid and saturated fail,
  OFF: the meter fails, though its pair won, DECIDE50 D-4 amended); n47-iceline 3/4 and n47-span 3/4 (the saturated band fails: both OFF,
  the one attempt did not pass). The pairs tied on foundry and spire (rej-5, rej-6), so the rule ships them.
- G4 fx49 (the kit): the wash peaks per tier with the kit ON: light +0.18, heavy +0.27, critical +0.20 (OFF: +0.34, +0.48, +0.50). The
  enemy kill stays at +0.27 with the kit ON, +0.28 OFF, on two frames over the +0.25 bar. Tried in r2 and r3: the Fx49KillBurstScale on the
  web's death burst (0.5), the heavy tier's size 0.40 and its brightness share 0.24: none moved the kill's wash, so its source is elsewhere.
  Those numbers went back to the r1 values (d05bf5f1). The kit ships ON with the kill SHORT (said in fx49's enabled_why). The per-effect
  fallback (the kill on its old path) reads +0.28 and does not help, so it is not used.
- Pairs run this round (both orders, 3 judges): g1-charge (re-placed bar vs NIGHT46) NEW 3-0. The NIGHT49 re-judge (P0, D-3) stands.

## D-12 The freeze (16:42, commit 22adc1d9; Tools/night/n50-freeze.cjs)
- ON: charge50, gunglow50 (PULSE OFF in code), fx49 (kill SHORT, said), look49 with numbers.levels = [n48-foundry, n47-spire]; lights49
  unchanged (ON).
- OFF: look49 on kiln, iceline and span (their meters or the saturated band failed); PULSE's steps.
- The n50-freeze build is Builds/n50-freeze (commit 22adc1d9). Gates green on it: golden 22/22, fingerprints EQUAL (plain, --off49,
  --off50), the Editor compile clean.

## D-10 Not checked (said now so the report does not claim them)
- The grayscale audit (NIGHT50.md G3): not yet run on any level.
- The iceline and span attempts: not built.
- The G6 before / after sheet: not built.
- The Player build, the reels, the freeze, the smoke, the suite, and the flow table: not run.

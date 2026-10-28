// NIGHT55 close (the lead): every slate entry's final state, from the measured record in NIGHT55-LOG.md. The run stopped at the usage
// limit (19:33) and resumed at 22:01, past the hard cap (21:35:42): no build, reel or gate ran after 19:33. An entry not gated by a fresh
// critic is SHORT, never passes true (NIGHT55.md section 2: only a critic's pass flips it).
//   node Tools/night/n55-close-slate.cjs
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Logs/n55/slate.json";
const d = JSON.parse(fs.readFileSync(file, "utf8"));
const CUT = "the usage limit stopped every agent at 19:33 and the run resumed at 22:01, past the hard cap: no further build, reel or gate";
const S = {
  "P0-smoke": "SHORT: measured GREEN, not gated. n55-r1 smoke with planted focus steals at +71 s and +211 s: DONE: all steps passed (228.0 s; Logs/n55/smoke-steal-n55-r1.out) vs A0 on n54-freeze2 19 failed; the outside-smoke focus pause not re-checked in a Player; " + CUT,
  "P0-wickGrade": "SHORT: r1 a155 THE WICK GRADE GREEN (walls 149 / 140 / 122 deg from danger, was 1 / 12 / 58; tells S 0.978 kept; cyan 5.7:1), THE CROWN RED (ring tells 282 of 533 px kept); round 2 fd71daf1 on B's branch never built; " + CUT,
  "P0-hotWall": "SHORT: r1 a155 HOT WALL RED (burn patch S 0.482 vs walls 0.741; Body 0.917 the most saturated); no-burn red-orange 0.001; round 2 fd71daf1 (a hotter burn) never built; " + CUT,
  "P0-step": "SHORT: r1 a255 RED on input (the line picked a 0.40 m fall and never crossed it); builder C's measure: kiln kb2c's 0.225 m step UP, camera per frame raw 0.225 / move54 0.090 / step55 0.035 m at 60 fps (bot harness, not a Player); the reel fix 47c64d8b + 5fe9b6cc never built; " + CUT,
  "P0-ssao": "SHORT: built on C's branch (91f9c48e: SSAO's blue noise held still, a runtime pass, no asset edit), never in a Player; " + CUT,
  "P0-decor": "SHORT: built on C's branch (283f3c41: accents off the walls' hue, foundry 41 -> 72 deg = parry gap 3 -> 28 deg, O-08; OFF accent share 0.03-0.35 % per entry frame), never in a Player; " + CUT,
  "P0-mirrors": "SHORT: identity table (Logs/n55/identity-r1.txt, the lead's run) FAIL rooms 9 -> 7 by g7 / g5 (P0-g7g5); left: a3 I1 69 %, g3 I2 1.28, g4 I2 1.18, g8 I2 1.08 / 1.10, v2 I2 1.17, kb2 / p5 I4; the asked pieces with jobs not built; " + CUT,
  "P0-g7g5": "SHORT: measured GREEN, not gated. Identity (the lead's run): g7 I1 99 -> 52 %, I2 1.22 / 1.75 PASS; g5 I1 76 -> 50 %, I2 1.12 / 4.66 / 1.25 / 2.85 PASS; n55-r1 verbs37 iceline 21/21, a452 iceline GREEN 18/18 frames; builder R's walk bot 10/10 (its report, not re-run by the lead); the fresh review of the frames not run; " + CUT,
  "P0-t125": "SHORT: measured GREEN, not gated. n55-r1 a555: THUNDERCLAP fire events of 6 taps before 0, after 6 (the reel's input handed back from the campaign bot); " + CUT,
  "P0-t99": "SHORT: closed in code at NIGHT49 (c5a6b7bf, FeelReel.cs skips a frame whose index did not advance: OPEN O-59); tonight's longest reel (a748) wrote 1800 frames, 0 failed: not past 1800, so not re-proven",
  "P0-wyrm": "SHORT: not built (builder S was writing wyrm55's Core numbers when the limit cut it); baseline not re-measured; " + CUT,
  "P1-wickWindow": "SHORT: bot only, on B's branch (26121bd5: n47human 40 seeds with wick55 forced on, dealt inside windows 6 -> 57 %, hot-wall hits 75 % -> lower: B's commit line), a155 FIGHT MODE proof (169e162c) never built; " + CUT,
  "P1-bossText": "SHORT: not started (B worked its P0s and P1-wickWindow first); " + CUT,
  "P1-fov": "SHORT: not started (C worked its P0s first); " + CUT,
  "P1-kill": "SHORT: r1 a355 RED on input (chain 2 of 6 kills, 4.4 s apart; hit mark 0.5 % next frame); debris 10 per kill at +1.5 s, spaced holds 0.150 s, cap ok; round 2 3c8fd885 never built; " + CUT,
  "P1-ride": "SHORT by decision (DECIDE55 D-8): GG 2026-10-13 'you gain speed too quickly'; not built, row off",
  "P1-wyrmTurn": "SHORT: not started; " + CUT,
  "P1-teachText": "SHORT: built on K's branch (1b996462; RED measured by K on n54-freeze verbs37 x5: 13 of 16 lines bad, a card over the counter in 99-227 frames), never in a Player; " + CUT,
  "P1-fixtures": "SHORT: not started; " + CUT,
  "P1-edges": "SHORT: not started; " + CUT,
  "P1-doorDark": "SHORT: slice 1 on R's branch (e25d89bb: kiln kb2b lit below its roof; n55-r-flat.py RED baseline), never in a Player; " + CUT,
  "P2-finish": "SHORT: not started; " + CUT,
  "P2-brief": "SHORT: not started (K's brief55 row carries P1-teachText only); " + CUT,
  "P2-speed": "SHORT: not started; " + CUT,
  "P2-clear": "SHORT: built on R's branch (edfee264 + 876dc97e: the rules, N55RoomClearTests 6, the HUD33 card, a455), never in a Player; " + CUT,
};
for (const e of d.entries) {
  if (e.id === "P1-guns48") continue; // passes true at 18:16 (the gate critic's pairs)
  if (!S[e.id]) throw new Error("no close state for " + e.id);
  e.state = "SHORT";
  e.passes = false;
  e.notes = (e.notes ? e.notes + " | " : "") + "CLOSE: " + S[e.id];
}
fs.writeFileSync(file, JSON.stringify(d, null, 2) + "\n");
for (const e of d.entries) console.log(e.id + " | " + e.owner + " | " + e.state + " | passes " + e.passes);

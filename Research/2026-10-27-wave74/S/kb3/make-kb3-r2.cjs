// NIGHT74 builder S, slice kb3 r2: r1 (make-kb3-r1.cjs, run first: idempotent, from the t0b blob) plus ONE low piece on the west wall, read from two of r1's readings:
//   1. n70-area OPEN read FAIL, "largest open square 13.5 m (x -10.0..3.5, z -212.0..-198.5)" (12 m or less passes; the shipped file read 11.5 m with the three blocks);
//   2. the open-zone wall probe (Research/2026-10-27-wave74/S/kb3/verbs-wall.txt, a first r2 with a plinth mid-apron): with the west block gone the west wall of kb3 is ONE 23 m run, and the
//      Player's verbs37 pick moved from kb2c's 18 m cooling wall to it (23 m in kb3 at (-9.3, -221.5), rows 9 / 11 PASS 1.73 s, but the lead's kiln pick is kb2c's 18 m): a 1.0 m ray
//      stops at any piece 1.0 m or more tall, so a piece on the wall cuts the run (a piece at z -211..-209 leaves 11.5 m and 14.5 m).
// THE DIE SILL: a 2 x 2 m, 1.2 m plinth against the west wall, z -211..-209, 6 m ahead of the spawn: the first thing the spawn's lane crosses (hop it, or take the lane's east side), waist-high
// cover from the far floor's spitters, below the eye (1.6 m) so the spawn still sees up the west wall and across to wave 1's nodes. It stands where the shipped west block stood (its west end).
//   node Research/2026-10-27-wave74/S/kb3/make-kb3-r2.cjs        run from the worktree root
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/kiln/rooms/kb3.json";
cp.execSync("node Research/2026-10-27-wave74/S/kb3/make-kb3-r1.cjs", { stdio: "inherit" });
const room = JSON.parse(fs.readFileSync(OUT, "utf8"));
const W = "NIGHT74 S kb3 r2 (Research/2026-10-27-wave74/S/card-kb3.md): ";
room.boxes.push({
  zone: "kb3", kind: "hull", min: [-10, 0, -211], max: [-8, 1.2, -209],
  job: "cover: THE DIE SILL (1.2 m plinth, 2 x 2 m, against the west wall 6 m ahead of the spawn: hop it, shelter behind it from the far floor's spitters)",
  why: W + "r1 left the apron an open 13.5 m square (n70-area OPEN, 12 m or less passes) and the west wall one 23 m run (the Player's wall pick moved off kb2c's 18 m wall); a waist-high 2 x 2 m plinth at the west block's old end cuts both (runs 11.5 m and 14.5 m, open square 11.5 m), is the first cover on the spawn's lane, and stands under the eye (1.6 m) so the spawn still sees wave 1's nodes; a body punched west bursts on its face.",
  note: "THE DIE SILL (1.2 m, 2 x 2 m)", noride: true,
});
room._why = W + "r1 plus THE DIE SILL (a 1.2 m plinth on the west wall). " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("r2 wrote " + OUT + ": boxes " + room.boxes.length);

// NIGHT74 builder S, slice g8 r2: r1 (make-g8-r1.cjs, run first: idempotent, from the git blob of the shipped export) with three changes read from r1's labels
// (Research/2026-10-27-wave74/S/g8/r1/g8/room-check.txt and r1/area.txt):
//   1. THE WEST DOOR COLUMN moves east to x -4.5..-3 (the east door column's mirror across the spawn): r1's column (x -6.75..-5.25) hid wave 1's third node (-8, 0, -230.5) but
//      left the shipped node (-4.5, 0, -231.5) in a straight line to the entry corridor (n56 IN VIEW "a line into the entry corridor", the red read PASS because its decor pillar
//      stood at (-5, -228)); the mirrored column hides it exactly as the east column hides (4.5, 0, -231.5).
//   2. Wave 1 keeps the two shipped near-floor nodes (-4.5, 0, -231.5) and (4.5, 0, -231.5): the third node (shipped: 2 m behind the spawn, r1: (-8, 0, -230.5)) is gone, so the
//      meter's REAR stays at 0 and the west column does not have to hide a node 8 m out.
//   3. THE SIGNAL BOX is 5.5 x 7 m (x 5.5..11), not 6.5 x 7: a plane of 6 m or more on a short side needs two climb tops 8 m apart (n70-area P9), and the box's stair, hoist and ride landing
//      are all within 3 m of each other; the shipped box (8 x 4 m) was not a plane of that size either.
//   node Research/2026-10-27-wave74/S/g8/make-g8-r2.cjs        run from the worktree root
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g8.json";
cp.execSync("node Research/2026-10-27-wave74/S/g8/make-g8-r1.cjs", { stdio: "inherit" });
const room = JSON.parse(fs.readFileSync(OUT, "utf8"));
const W = "NIGHT74 S g8 r2 (Research/2026-10-27-wave74/S/card-g8.md): ";

const westCol = room.boxes.find((b) => /^THE WEST DOOR COLUMN/.test(b.note || ""));
if (!westCol || westCol.min[0] !== -6.75) throw new Error("west door column not as r1 wrote it");
westCol.min[0] = -4.5;
westCol.max[0] = -3;
westCol.job = "30 s problem: THE WEST DOOR COLUMN (the west lane's near column, the east door column's mirror across the spawn: a charge from the door end is stopped by it)";
westCol.why = W + "r1's column at x -6.75..-5.25 left the shipped wave-1 node (-4.5, 0, -231.5) in a straight line to the entry corridor; at x -4.5..-3 it hides that node as the east door column hides (4.5, 0, -231.5).";

const box = room.boxes.find((b) => /^THE SIGNAL BOX/.test(b.note || ""));
if (!box || box.min[0] !== 4.5) throw new Error("signal box not as r1 wrote it");
box.min[0] = 5.5;
box.note = "THE SIGNAL BOX (5 m, 5.5 x 7 m, north-east)";
box.why = W + "r1's box was 6.5 x 7 m: a plane of 6 m or more needs two climb tops 8 m apart (n70-area P9) and the stair, hoist and ride landing are within 3 m of each other; 5.5 x 7 m keeps all three and the east lane wall's landing.";

const row = room.row;
// the first proof walk went down the west lane at x -3.5 through (-3.5, 0, -228): the mirrored column (x -4.5..-3, z -229..-227.5) stands there (levelgen: "walk STOPPED at (-3.56, 0, -227.15)");
// it now goes round the column's west side and down the lane at x -4 (3.25 m between the far column and the pit)
row.proofs[0].points = [[0, 0, -222], [-2.5, 0, -225], [-6, 0, -228.3], [-4, 0, -233], [-4, 0, -243], [0, 0, -247], [1.5, 0, -248.5]];
row.waveNodes43[0] = [[-4.5, 0, -231.5], [4.5, 0, -231.5]];
row.waveNodes43_why = W + "wave 1 on the two shipped near-floor nodes (the third, 2 m behind the spawn, is gone: the meter's REAR); waves 2 and 3 on the far floor, the east lane and both tiers (the shipped nodes stood on decks and stairs that are gone).";
room._why = W + "r1 with the west door column mirrored (x -4.5..-3), wave 1 on its two shipped nodes, the signal box 5.5 m wide. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("r2 wrote " + OUT + ": boxes " + room.boxes.length + ", wave 1 nodes " + row.waveNodes43[0].length);

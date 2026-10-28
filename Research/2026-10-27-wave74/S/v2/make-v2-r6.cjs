// NIGHT74 builder S, slice v2 (NIGHT71 R's THE NAVE r5, left out of the play build with v5: the spire's verbs37 read RED, and the rooms-rules meter reads
// core 1 of 5: PLANES 4 (the crypt -2, the floor, the aisle 2.4, the loft 3.5), COVER (no free-standing piece), REAR (1 of 5 wave-1 nodes behind the
// entry's facing), VIEW (no enemy in view within 2 s: wave 1 stands in the hold at the player's sides)). Writes Design/levels/n67/spire/rooms/v2.json from
// t0b's file (idempotent: from the git blob).   node Research/2026-10-27-wave74/S/v2/make-v2-r6.cjs        run from the worktree root
// The verb stays LAUNCH (a husk at the rood screen's door, dash or slide into it and punch: it bursts on the queue or drops into THE CRYPT) and the room keeps
// the crypt, the screen, the loft, the aisles, the three pads, the stairs, the waves. Changes, each with a job:
//   1. PLANES: the aisle's three decks 2.4 -> 2.5 m (with their ramps' rise and the two pads' heights), so the aisle and the loft (3.5) are one tier by the
//      meter's rule (a plane is the cluster within 1.0 m of its lowest member): planes -2 / 0 / 2.5-3.5.
//   2. COVER: THE PIER (a 2.4 m pillar, 1.6 x 1.6 m, the hold's west) and THE FONT (1.2 m low cover, 2 x 2 m, the hold's east).
//   3. VIEW + REAR: wave 1's east-most node (0.5, 0, 75.5), 0.5 m behind the spawn's line, becomes (-10.5, 0, 57.5): straight ahead through the rood
//      screen's door and over the crypt, 17.5 m from the spawn, so an enemy is in the entry's view (levelgen's spawn31 refused (-10.5, 0, 67.8): 7.2 m).
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/spire/rooms/v2.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S v2 r6 (Research/2026-10-27-wave74/S/card-v2.md): ";
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const row = room.row;

// ---- 1. the aisle tier 2.4 -> 2.5
let n24 = 0;
for (const b of room.boxes) {
  if (b.kind === "deck" && Math.abs(b.max[1] - 2.4) < 1e-9) {
    b.max[1] = 2.5;
    b.why = W + "the deck's top 2.4 -> 2.5 m (the room's planes: the aisle and the loft's 3.5 m are one tier within 1.0 m). " + (b.why || "");
    n24++;
  }
}
for (const r of room.ramps) {
  if (Math.abs(r.rise - 2.4) < 1e-9) { r.rise = 2.5; r.why = W + "rise 2.4 -> 2.5 (the aisle's tier). " + (r.why || ""); n24++; }
}
for (const p of room.pads) {
  if (Math.abs(p.at[1] - 2.4) < 1e-9) { p.at[1] = 2.5; n24++; p.why = W + "the pad's standing height 2.4 -> 2.5 (the aisle's tier). " + (p.why || ""); }
  if (Math.abs(p.target[1] - 2.4) < 1e-9) { p.target[1] = 2.5; n24++; p.why = W + "the pad's landing height 2.4 -> 2.5 (the aisle's tier). " + (p.why || ""); }
}
if (n24 !== 7) throw new Error("expected 3 decks + 2 ramps + 2 pad heights at 2.4, found " + n24);
// anything else at 2.4 / 2.425 in the geometry is a miss
const left = JSON.stringify([room.lights, room.props, row.nodes, row.highNodes, row.waveNodes43, row.proofs, row.line65, row.frames52, row.frames36]).match(/\b2\.4(25)?\b/g);
if (left) throw new Error("a 2.4 is left outside the boxes / ramps / pads: " + left);

// ---- 2. cover
room.boxes.push(
  {
    zone: "v2", kind: "hull", min: [-14, 0, 72], max: [-12.4, 2.4, 73.6],
    job: "cover: THE PIER (a 2.4 m pillar in the hold's west: high cover to circle while the queue forms)",
    why: W + "the hall had no free-standing piece (the meter's COVER RED): a 1.6 x 1.6 x 2.4 m pillar 2.4 m east of wave 1's west nodes and 2.4 m west of the walk line (x -10), clear of the narthex stair (z 76-79) and of the organ pad's flight.",
    note: "THE PIER (2.4 m, 1.6 x 1.6 m, the hold's west)",
  },
  {
    zone: "v2", kind: "hull", min: [-6, 0, 76.5], max: [-4, 1.2, 78.5],
    job: "cover: THE FONT (1.2 m low cover in the hold's east: hop it or shelter behind it from the queue)",
    why: W + "2 x 2 x 1.2 m low cover (1.0-1.25 m), 4 m east of the spawn's line, off the walk line and the east lane.",
    note: "THE FONT (1.2 m, 2 x 2 m, the hold's east)",
  },
);

// ---- 3. wave 1 on the far side of the crypt, in the entry's view
// R5 put wave 1 in the hold at the player's sides (7-11 m from the commit feet: none 12 m out, so each enemy took any node, and none was in view at the
// entry: the meter's VIEW RED, one node 0.5 m behind the spawn's line: REAR RED). Only a node 12 m or more from the spawn may stand in the first view cone
// (levelgen's spawn31 refused a single replacement at (-10.5, 0, 67.8): 7.22 m), and one far node alone would stack all five enemies on it: wave 1 takes
// five far-side nodes (17-23 m out, straight ahead through the rood screen's door, over the crypt). The original design (the card: "wave 1 comes up the
// east lane to the screen's door: dash-punch the queue there") comes back: they are seen across the crypt, walk round it by the east lane and queue at the door.
const W1 = [[-10.5, 0, 57.5], [-7.5, 0, 57.5], [-13.5, 0, 57.5], [-12.5, 0, 55.5], [-9.5, 0, 55.5]];
const HOLD = [[-17.5, 0, 72.5], [-16.5, 0, 72.5], [-3.5, 0, 72.5], [-2.5, 0, 73.5], [0.5, 0, 75.5]];
if (!eq(row.waveNodes43[0], HOLD)) throw new Error("wave 1 is not NIGHT71's hold list");
row.waveNodes43[0] = W1.map((n) => n.slice());
const far = row.nodes.filter((n) => !HOLD.some((h) => eq(h, n)));
row.nodes = far.concat(W1.filter((n) => !far.some((f) => eq(f, n))).map((n) => n.slice()));
const moved = W1.length;
row.nodes_why = W + "wave 1 moved from the hold's sides to the crypt's far side ((-10.5 | -7.5 | -13.5, 0, 57.5), (-12.5 | -9.5, 0, 55.5): 17-23 m from the spawn, in the entry's view through the screen's door); the hold nodes leave the list. " + (row.nodes_why || "");
room._why = W + "the aisle tier 2.5 m (planes -2 / 0 / 2.5-3.5), THE PIER and THE FONT, wave 1's east-most node in the screen's door; the verb (launch), the crypt, the screen, the loft, the pads and the waves are NIGHT71 R's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", 2.4 -> 2.5 in " + n24 + " places, node moved " + moved);

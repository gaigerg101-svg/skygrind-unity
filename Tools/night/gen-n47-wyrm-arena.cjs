// NIGHT47 builder B (G33; Research/2026-10-18-wave47/B-cards.md section 4.2, the card gate PASS): THE KILN WYRM's arena for n47-kiln, rebuilt
// to reward the THUNDERCLAP's boost. From Design/maps/wyrm46-arena.json (NIGHT46 B1's 35 boxes, untouched; Tools/night/gen-n46-wyrm-arena.cjs
// is not run or edited) -> Design/maps/wyrm47-arena.json (+ the StreamingAssets copy). Every box keeps its index (crumble31's 27 / 28, keep 23,
// plates45's 29-32, the door's 2 / 33 / 34); what is added, each for one card line:
//   THE SLAG ISLANDS (boxes 35-37): three 4 x 4 m slag plates, top 6.0 m, hung over the inner ring at R 12 (bearings 90 / 210 / 330 deg from
//   the arena's +x axis, so none is on the door's axis). The height comes from the measured reach (n47b reach, kb3's floor, rows off): a jump
//   1.58 m, jump + double jump 2.88 m, jump + one shot down 4.12 m, jump + shot + double jump + shot 7.92 m: 6.0 m is 3.1 m over the double
//   jump and 1.9 m over one shot, 1.9 m under the two-shot chain: only the boost chain lands it from the floor ("heights only the boost
//   clears"). Island to island: 20.8 m apart (R 12 at 120 deg): a boost-hop between them is not a plan line tonight (measured in round 2).
//   No ramp, no ride wall reaches them. They are the air under THE BURROW's breach (sim, wyrm47, round 2) and the angle on the core.
// Run (from the worktree root): node Tools/night/gen-n47-wyrm-arena.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const IN = "Design/maps/wyrm46-arena.json";
const OUTS = ["Design/maps/wyrm47-arena.json", "Assets/StreamingAssets/Design/maps/wyrm47-arena.json"];
const ISLAND = { r: 12, half: 2, top: 6.0, thick: 1.0, bearings: [90, 210, 330] };

const text = fs.readFileSync(path.join(ROOT, IN), "utf8");
const A = JSON.parse(text);
const boxes = A.collision.boxes;
if (boxes.length !== 35) throw new Error(`${IN}: ${boxes.length} boxes, expected 35 (NIGHT46 B1's)`);
const first = boxes.length;
for (const deg of ISLAND.bearings) {
  const a = (deg * Math.PI) / 180;
  const cx = +(ISLAND.r * Math.cos(a)).toFixed(2), cz = +(ISLAND.r * Math.sin(a)).toFixed(2);
  const min = [+(cx - ISLAND.half).toFixed(2), ISLAND.top - ISLAND.thick, +(cz - ISLAND.half).toFixed(2)];
  const max = [+(cx + ISLAND.half).toFixed(2), ISLAND.top, +(cz + ISLAND.half).toFixed(2)];
  // nothing else may stand in an island's volume
  for (let i = 0; i < first; i++) {
    const b = boxes[i];
    if (i === 0) continue;
    const hit = b.min[0] < max[0] && b.max[0] > min[0] && b.min[1] < max[1] && b.max[1] > min[1] && b.min[2] < max[2] && b.max[2] > min[2];
    if (hit) throw new Error(`island at ${deg} deg overlaps box ${i}`);
  }
  boxes.push({ min, max, kind: "platform" });
  A.solids45.boxes.push({ index: boxes.length - 1, kind: "platform", job: "route",
    why: `THE SLAG ISLAND at ${deg} deg (NIGHT47 B, G33, B-cards.md 4.2): a 4 x 4 m slag plate hung from the crucible at ${ISLAND.top} m over the inner ring (R ${ISLAND.r}): above the double jump's 2.88 m and one shot's 4.12 m, under the two-shot chain's 7.92 m (n47b reach): reached only by the THUNDERCLAP boost; the air under the burrow's breach and the angle on the core` });
}
A._why = "NIGHT47 B (G33; Tools/night/gen-n47-wyrm-arena.cjs): wyrm46-arena.json with THE SLAG ISLANDS added (boxes 35-37; every other index unchanged). Before: " + A._why;
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const out = JSON.stringify(A, null, 2).replace(/\n/g, eol) + eol;
for (const f of OUTS) fs.writeFileSync(path.join(ROOT, f), out);
console.log(`gen-n47-wyrm-arena: ${boxes.length} boxes (islands ${first}-${boxes.length - 1} at ${ISLAND.top} m, R ${ISLAND.r}) -> ${OUTS.join(", ")}`);

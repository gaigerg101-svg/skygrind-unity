// NIGHT53 the loop, cycle 2 (the lead's; queue q3 + q2's frames): iceline g4 THE REACH II breaks its mirror (I1 77 % at u3-n52; the
// final GG-persona review, DECIDE52 D-28: "a mirror down the door's axis with the drone parked on it, the canal does not read as a drop").
//   1. The EAST LOCK WALL runs on to the NE gallery (z -112.5 -> -119): one 24 m ride from the door floor over the canal to the third tier,
//      its top 1 m over the gallery's: the east side's way up is the ride (the gallery ramp stays the walkers' route).
//   2. The WEST LOCK WALL and the WEST RUN-UP BLOCK are cut: the west canal is crossed by a jump (a stepping stone in the canal, 0.4 m under
//      the floor) or the ledge hoist; the WEST SLUICE HEAD (a 1.2 m block where the run-up stood) is the west drone's cover on the door floor.
//   3. The WEST ICE POST moves 2.6 m west (x -8.6..-7.4): the two posts no longer mirror.
//   4. The first drone's node moves off the door's axis, west, onto the canal's far lip ([0, 0, -118] -> [-6, 0, -113.5]).
//   5. The canal reads as a drop: two white pool lights on its floor (a glow from below; light colours are the plan's white / cyan /
//      amber / orange: the pale violet-white lips are not a light colour, not done).
//   6. iceline g2 THE SHIELD gets its frames52 spots (q2: the a452 reel frames it at the next build): entry at the spawn, mid in the door
//      half facing the anvil, high on the west gantry looking back at the room (off the pads).
// Asserts g4 is NIGHT52's (fix 6) and g2 has no frames52; every other room bit for bit.
// Run: node Tools/night/n53-loop-g4.cjs [--dry], then bash Tools/night/n52-loop-regen.sh iceline g4 g2
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const g4 = plan.rooms.find((r) => r.id === "g4"), g2 = plan.rooms.find((r) => r.id === "g2");
if (!/Fix 6/.test(g4.loop52_why || "") || /NIGHT53/.test(g4.loop52_why)) throw new Error("g4 is not NIGHT52's fix 6 (or already run)");
if (g2.frames52) throw new Error("g2 already has frames52");

// 1. the east lock wall to the gallery
const east = one(plan.boxes, (b) => b.zone === "g4" && b.ride && same(b.min, [11, -1, -112.5]) && same(b.max, [12, 5, -95]), "the east lock wall");
east.min = [11, -1, -119];
east.ride36 = Object.assign({}, east.ride36, { lz: [1, 25] });
east.why = "the EAST LOCK WALL (ride, cyan, 24 m x 5 m): from the door floor over the canal's east end on to the NE gallery, its top 1 m over the gallery's: the east side's way up is the ride (NIGHT53 n53-loop-g4.cjs: run on 6.5 m to the gallery, the mirror broken; the gallery ramp stays the walkers' route)";
east.note = east.why;
// 2. the west lock wall and its run-up cut; the sluice head and the stepping stone
const cut = [
  one(plan.boxes, (b) => b.zone === "g4" && b.ride && same(b.min, [-12, -1, -112.5]) && same(b.max, [-11, 5, -95]), "the west lock wall"),
  one(plan.boxes, (b) => b.zone === "g4" && same(b.min, [-11, 0, -99]) && same(b.max, [-8, 1.2, -96]), "the west run-up block"),
];
const at = plan.boxes.indexOf(cut[0]);
plan.boxes = plan.boxes.filter((b) => !cut.includes(b));
const add = [
  { zone: "g4", kind: "hull", min: [-12, 0, -101], max: [-9, 1.2, -98.5], job: "cover", why: "the WEST SLUICE HEAD (1.2 m, 3 x 2.5 m, on the door floor's west side): cover from the west drone's orbs over the canal and a vault step to see the far lip (NIGHT53: where the run-up stood; the west has no ride now)" },
  { zone: "g4", kind: "deck", min: [-8, -1, -110], max: [-5, -0.4, -108], job: "climb", why: "the CANAL STONE (3 x 2 m, 0.4 m under the floor, mid-canal west of the bridge): the jump's middle stone across the west canal, under the west drone (NIGHT53: the west crossing is a jump, not a ride)" },
];
for (const b of add) b.note = b.why;
plan.boxes.splice(at, 0, ...add);
// 2b. (tried and reverted, 08:24: cutting the east bridge left the gallery ramp's foot on the canal lip, unreached by walkers: the
//     contract's tier item FAILed, and I2 only moved 1.18 -> 1.22)
// 3. the west post off the mirror
const post = one(plan.boxes, (b) => b.zone === "g4" && same(b.min, [-5.6, 0, -101.6]) && same(b.max, [-4.4, 4, -100.4]), "the west ice post");
post.min = [-8.6, 0, -101.6];
post.max = [-7.4, 4, -100.4];
post.why = "the WEST ICE POST (1.2 m x 4 m) on the door floor, west of the ledge hoist: breaks the west drone's orb line to the hoist and the sluice head (NIGHT53: 3 m west, off the east post's mirror)";
post.note = post.why;
// 4. the first drone off the axis
if (!same(g4.nodes[0], [0, 0, -118])) throw new Error("g4's first node " + JSON.stringify(g4.nodes[0]));
g4.nodes[0] = [-6, 0, -113.5];
g4.nodes_why = (g4.nodes_why || "") + "; NIGHT53: the first drone off the door's axis, west, on the canal's far lip (the review: the drone parked on the axis)";
// 5. the canal's glow
const li = plan.lights.findIndex((l) => l.id === "g4-L5");
if (li < 0 || plan.lights.some((l) => l.id === "g4-L6")) throw new Error("g4's lights");
plan.lights.splice(li + 1, 0,
  { id: "g4-L6", room: "g4", kind: "pool", at: [-6, -0.975, -109], normal: [0, 1, 0], size: [5, 4, 0.05], color: "white", range: 6, intensity: 1.3, why: "NIGHT53: the canal's west floor lit from below: the canal reads as a drop from the door" },
  { id: "g4-L7", room: "g4", kind: "pool", at: [5, -0.975, -109], normal: [0, 1, 0], size: [4, 4, 0.05], color: "white", range: 6, intensity: 1.1, why: "NIGHT53: the canal's east floor lit from below (dimmer than the west: the west is the jump)" });
g4.loop52_why += " NIGHT53 (n53-loop-g4.cjs): the mirror broken: the east lock wall runs on to the gallery (24 m), the west wall and run-up cut, a sluice head and a canal stone in the west, the west post 3 m west, the first drone off-axis west, the canal lit from below.";
// 6. g2's frames
g2.frames52 = { _why: "NIGHT53 (n53-loop-g4.cjs, queue q2): the room frames reel a452: entry at the spawn, mid in the door half facing the anvil, high on the west gantry looking back (off the pads)", entry: [0, 0, -34.5, 0, 0.02], mid: [-4, 0, -40, 10, 0.05], high: [-9.5, 4, -55.5, -140, -0.12] };

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (Array.isArray(before[k]) && k !== "rooms") {
    const keep = (x) => x.zone !== "g4" && x.room !== "g4";
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g4 changed");
  } else if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (!["g4", "g2"].includes(before.rooms[i].id) && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
    const a = JSON.parse(JSON.stringify(plan.rooms.find((r) => r.id === "g2"))); delete a.frames52;
    if (!same(a, before.rooms.find((r) => r.id === "g2"))) throw new Error("g2 changed beyond frames52");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
const n = (k) => plan[k].filter((x) => x.zone === "g4").length - before[k].filter((x) => x.zone === "g4").length;
console.log("g4: boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads") + "; lights +2; g2 frames52 added");
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, text.includes("\r\n") ? "\r\n" : "\n") + (text.endsWith("\n") ? (text.includes("\r\n") ? "\r\n" : "\n") : ""));
console.log("wrote " + PLAN);

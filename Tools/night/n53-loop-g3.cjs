// NIGHT53 the loop, cycle 3 (the lead's; queue q4): iceline g3 THE SWARM. The final reviews (DECIDE52 D-28 / D-30): the hopper is a giant
// box, the chute opens toward the door (not into the west lane), the spill-wall pocket, wave 2's nodes spread on three sides (the swarm never
// packs where the blast lesson wants it); I5 (Logs/n53/identity-p0.md): g3 is one of iceline's three ring-round-a-mass rooms.
//   1. THE HOPPER narrows 6 x 8 -> 4 x 6 m (x -6..-2, z -81..-75), still floor to roof: NIGHT53.md's "stops at ~6 m with a funnel top" was
//      read and not taken (DECIDE53 D-10): a 6 m top is a fourth standing height no walker reaches (a 0-hit perch, or a ramp that makes four
//      planes); the giant-box read is answered by the size, the chute by the cheeks.
//   2. THE CHUTE opens west: the two spill walls move from the hopper's door face (the pocket toward the door) to its west face as the
//      chute's cheeks, 2 m stubs floor to roof at z -80.5 and -75.5 (x -8..-6): the swarm comes out of the hopper into the west lane.
//   3. Wave 2's nodes pack at the west lane's north end (the two centre nodes and the NE floor node move west): the swarm bunches where the
//      blast lesson wants it; the tipple head's high node stays (the two heights).
// Asserts g3 is NIGHT52's; every other room bit for bit.   node Tools/night/n53-loop-g3.cjs [--dry]; bash Tools/night/n52-loop-regen.sh iceline g3
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const g3 = plan.rooms.find((r) => r.id === "g3");
if (/NIGHT53/.test(g3.loop52_why || "")) throw new Error("already run");
const hop = one(plan.boxes, (b) => b.zone === "g3" && same(b.min, [-7, 0, -82]) && same(b.max, [-1, 10, -74]), "the hopper");
hop.min = [-6, 0, -81]; hop.max = [-2, 10, -75];
hop.why = "THE HOPPER (card: Landmark): the tipple's coal bin, 4 x 6 m floor to roof (NIGHT53 n53-loop-g3.cjs: from 6 x 8 m, the review's giant box), its chute on the west face: the swarm's source and the cover the lanes turn round";
hop.note = hop.why;
const ws = one(plan.boxes, (b) => b.zone === "g3" && same(b.min, [-7.5, 0, -74]) && same(b.max, [-7, 10, -71]), "the west spill wall");
const es = one(plan.boxes, (b) => b.zone === "g3" && same(b.min, [-1, 0, -74]) && same(b.max, [-0.5, 10, -71]), "the east spill wall");
ws.min = [-8, 0, -81]; ws.max = [-6, 10, -80.5];
ws.why = "the CHUTE's NORTH CHEEK (2 x 0.5 m, floor to roof, off the hopper's west face; NIGHT53: the west spill wall moved): the chute opens west into the lane, so the swarm packs there for the blast (the lane ride runs over it)";
es.min = [-8, 0, -75.5]; es.max = [-6, 10, -75];
es.why = "the CHUTE's SOUTH CHEEK (2 x 0.5 m, floor to roof; NIGHT53: the east spill wall moved, its door-side pocket gone): the chute's other cheek, the west lane's door-end corner to round";
ws.note = ws.why; es.note = es.why;
const want = [[-8.5, 0, -75], [-9.5, 0, -77.5], [0, 0, -76.5], [0.5, 0, -78.5], [-6, 0, -86.5], [4, 0, -86], [8.5, 2, -88]];
if (!same(g3.nodes, want)) throw new Error("g3's nodes " + JSON.stringify(g3.nodes));
g3.nodes = [[-8.5, 0, -82], [-9.5, 0, -84.5], [-7.5, 0, -86.5], [-9.5, 0, -79], [-6, 0, -86.5], [-4, 0, -87], [8.5, 2, -88]];
g3.nodes_why = (g3.nodes_why || "") + "; NIGHT53 (n53-loop-g3.cjs): wave 2's floor nodes packed at the west lane's north end, out of the chute (the review: spread on three sides, the swarm never packed); the tipple head's 2 m node kept";
g3.loop52_why = (g3.loop52_why || "") + " NIGHT53 (n53-loop-g3.cjs): the hopper 4 x 6 m, the chute's cheeks on its west face, wave 2's nodes in the west lane's north end.";
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== "g3" && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => x.zone !== "g3" && x.room !== "g3";
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g3 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
console.log("g3: the hopper 4 x 6, the cheeks on its west face, the nodes packed west");
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote " + PLAN);

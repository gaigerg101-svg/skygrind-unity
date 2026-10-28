// EXT53 builder M (row M, THE MIRRORS, the fourth room): spire v2 THE NAVE breaks its mirror. NIGHT52 F built it a mirror across the door's
// axis x = -10 (the two narthex cheeks and wings, the two 4 m aisles along the side walls with their north ramps, the bell and its beam on
// the axis, the two apse posts): I1 98 %.
//   1. THE BELL lies under the hole it fell through (the vault's hole, the white roof light at x -16): the bell (6 x 3 m, 5 m), its beam,
//      its lip / crown lights, its lamp, its ring and the health beside it 2 m west (bell x -15..-9): the west lane between the west aisle
//      and the bell is 3 m, the east floor wide (the grapple side).
//   2. THE EAST PORCH is gone (the narthex's east wing cut): the door's east side is floor; wave 3's two east-porch nodes on that floor.
//   3. THE EAST AISLE broke at the arcade pier: its south half cut (x -2..2, z 59..67 now): the east aisle ring (kept, 2.5 m south of the
//      broken end) is the way up its end, the north ramp the walkers'; wave 2's two south-aisle nodes onto what stands.
//   The west keeps the air loop's porch, aisle, gap ramp and the clerestory ride; the east is the grapple side: the floor, the ring, the
//   short aisle.
//   4. The walk proofs follow (the loop's west leg in the 3 m west lane; the beam walk 2 m west); the hoists retarget onto the moved bell.
// Asserts v2 is u3-n53's + EXT53 W's pad moves (e53-loop-pads.cjs); every other room bit for bit.
// Run: node Tools/night/e53-m-v2.cjs [--dry], then bash Tools/night/n52-loop-regen.sh spire v2
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v2";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const Z = (b) => b.zone === R;
const room = plan.rooms.find((r) => r.id === R);
if (!same(room.rect, [-22, 51, 2, 83]) || room.ext53_why) throw new Error("v2 is not u3-n53's (or already run)");
const DX = -2;
const sx = (v) => [v[0] + DX, ...v.slice(1)];
const swapIn = (list, a, b, what) => { const i = list.findIndex((n) => same(n, a)); if (i < 0) throw new Error(what + ": no " + JSON.stringify(a)); list[i] = b.slice(); };

// 1. the bell 2 m west
const bell = one(plan.boxes, (b) => Z(b) && same(b.min, [-13, 0, 64]) && same(b.max, [-7, 5, 67]), "the bell");
bell.min = sx(bell.min); bell.max = sx(bell.max);
bell.why = bell.why.replace("THE BELL (deck 5 m, 6 x 3 m:", "THE BELL (deck 5 m, 6 x 3 m; EXT53 e53-m-v2.cjs: 2 m west, under the hole in the vault it fell through, the east floor the wide side:");
bell.note = bell.why;
const beam = one(plan.ramps, (r) => Z(r) && same(r.from, [-10, 54]) && same(r.to, [-10, 64]), "the bell beam");
beam.from = [-12, 54]; beam.to = [-12, 64];
for (const id of ["v2-P1", "v2-bell-lip-s", "v2-bell-lip-n", "v2-bell-crown"]) {
  const q = one(plan.props, (p) => p.room === R && p.id === id, id);
  if (q.at[0] !== -10) throw new Error(id + " at " + JSON.stringify(q.at));
  q.at = sx(q.at);
}
const lamp = one(plan.lights, (l) => l.room === R && l.id === "v2-bell-lamp" && same(l.at, [-10, 0.025, 71.5]), "the bell's lamp");
lamp.at = sx(lamp.at);
if (!same(room.grappleNodes, [[-10, 8.5, 69.5], [0, 5.5, 69.5], [-15.5, 6.5, 62]])) throw new Error("v2's rings " + JSON.stringify(room.grappleNodes));
room.grappleNodes[0] = [-12, 8.5, 69.5];
room.grapple = room.grapple.replace("the BELL RING 8.5 m up,", "the BELL RING 8.5 m up (EXT53: 2 m west with the bell),").replace("the EAST AISLE RING 3 m over the east aisle (the east lane -> the aisle's spitters)", "the EAST AISLE RING 2.5 m south of the east aisle's broken end (EXT53: the east lane -> up the broken end onto the aisle's spitters)");
const bh = one(plan.pads, (p) => Z(p) && same(p.at, [-6.75, 0, 61]) && same(p.target, [-8.5, 5, 66]), "the bell hoist");
bh.target = [-10.5, 5, 66];
const ah = one(plan.pads, (p) => Z(p) && same(p.at, [-18, 2.4, 72]) && same(p.target, [-11.5, 5, 65.5]), "the aisle hoist");
ah.target = [-13.5, 5, 65.5];

// 2. the east porch cut
const ew = one(plan.boxes, (b) => Z(b) && same(b.min, [-4, 0, 75]) && same(b.max, [2, 2.4, 83]), "the narthex's east wing");
plan.boxes = plan.boxes.filter((b) => b !== ew);
// 3. the east aisle's south half cut
const ea = one(plan.boxes, (b) => Z(b) && same(b.min, [-2, 0, 59]) && same(b.max, [2, 2.4, 75]), "the east aisle");
ea.max = [2, 2.4, 67];
ea.why = "the EAST AISLE (2.4 m, card: Molecule node AE; EXT53 e53-m-v2.cjs: broken at the arcade pier, its south half and the east porch gone): the spitters' stand along the east wall's north half, the grapple side: its north ramp the walkers' climb, its ring the way up its broken end";
ea.note = ea.why;

// the nodes
const N0 = [-15.5, 0, 63.5], N0b = [-16.5, 0, 63.5];
swapIn(room.nodes, N0, N0b, "nodes");
swapIn(room.waveNodes43[0], N0, N0b, "wave 1");
const moves = [[[-1, 2.4, 79], [-1, 0, 79]], [[-1, 2.4, 76.5], [-1, 0, 76.5]], [[0.5, 2.4, 71], [1, 2.4, 63.5]], [[0.5, 2.4, 68.5], [-1, 2.4, 60]]];
for (const [a, b] of moves) {
  swapIn(room.highNodes, a, b, "highNodes");
  for (const w of room.waveNodes43) { const i = w.findIndex((n) => same(n, a)); if (i >= 0) w[i] = b.slice(); }
}
// the floor ones out of highNodes into nodes
room.highNodes = room.highNodes.filter((n) => n[1] > 0);
room.nodes.push([-1, 0, 79], [-1, 0, 76.5]);
room.waveNodes43_why = (room.waveNodes43_why || "") + "; EXT53 (e53-m-v2.cjs): the east porch's two wave-3 nodes on the floor where it stood, the east aisle's two south nodes onto its standing north half, the west lane's node 1 m west of the moved bell";

// 4. the proofs
const P = room.proofs;
if (!same(P[1].points, [[-15.5, 0, 74], [-15.5, 0, 62], [-15, 0, 55], [-10, 0, 52.5], [-5, 0, 55], [-4.5, 0, 62], [-4.5, 0, 74], [-10, 0, 73.5]])) throw new Error("v2's loop walk");
if (!same(P[3].points, [[-10, 0, 52.5], [-10, 0, 53.5], [-10, 5, 63.7], [-8.5, 5, 65.8]])) throw new Error("v2's beam walk");
P[1].points = [[-16.5, 0, 74], [-16.5, 0, 62], [-15.5, 0, 55], [-10, 0, 52.5], [-5, 0, 55], [-4.5, 0, 62], [-4.5, 0, 74], [-10, 0, 73.5]];
P[3].points = [[-12, 0, 52.5], [-12, 0, 53.5], [-12, 5, 63.7], [-10.5, 5, 65.8]];
room.ext53_why = "EXT53 builder M (Tools/night/e53-m-v2.cjs; row M, THE MIRRORS): the mirror broken (I1 98 %): the bell 2 m west under its vault hole, the east porch cut, the east aisle broken at the arcade pier (its south half cut): the west keeps the air loop and the ride, the east is the grapple side";

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== R && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => !x || (x.zone !== R && x.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside v2 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
console.log("v2: bell " + JSON.stringify(bell.min) + "; east wing cut; east aisle " + JSON.stringify(ea.min) + ".." + JSON.stringify(ea.max) + "; nodes " + room.nodes.length + ", high " + room.highNodes.length);
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);

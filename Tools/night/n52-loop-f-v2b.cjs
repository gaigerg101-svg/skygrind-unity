// NIGHT52 the room loop, builder F (round 4): spire v2 THE FALLEN BELL II (Tools/night/n52-loop-f-v2.cjs) from the GG-persona review, in
// Design/levels/n52/spire.plan.json:
//   the clerestory ride ran beside the walkable west aisle at deck height (riding bought nothing): a 5 m gap cut in the aisle deck under the
//   ride's middle (lz 13..18), so riding is the way along the aisle; a walkers' ramp in the gap against the wall up to the aisle's south part
//   (the narthex wing and the south aisle keep their walkers' way; the floor lane below is the walkers' route along); the west arcade pier
//   (it would stand in the gap beside the ramp: a slot) cut
//   the fallen bell reads as a plain cube: its flared lip and crown edge drawn (edgeLight props on the same collision footprint)
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v2b.cjs, then bash Tools/night/n52-loop-regen.sh spire v2
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v2";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !same(room.rect, [-22, 51, 2, 83]) || room.f52b_why) throw new Error("v2 is not round 3's THE FALLEN BELL II (already run?)");
const X = (lx) => lx - 10, Z = (lz) => 83 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];

const ai = plan.boxes.findIndex((b) => b.zone === R && same(b.min, [X(-12), 0, Z(24)]) && same(b.max, [X(-8), 2.4, Z(8)]));
if (ai < 0) throw new Error("the west aisle moved upstream");
const aisle = plan.boxes[ai];
const south = Object.assign({}, aisle, { min: [X(-12), 0, Z(13)], max: [X(-8), 2.4, Z(8)] });
south.why = "the WEST AISLE's south part (2.4 m, card: Molecule node AW): joined to the narthex wing, the clerestory ride's take-off; (round 4, GG review) a 5 m gap under the ride's middle splits the aisle, so riding is the way along it";
south.note = south.why;
const north = Object.assign({}, aisle, { min: [X(-12), 0, Z(24)], max: [X(-8), 2.4, Z(18)] });
north.why = "the WEST AISLE's north part (2.4 m, card: Molecule node AW): the spitters' stand under the ride's far end, reached by its north ramp; the ride lands you on it over the gap";
north.note = north.why;
plan.boxes.splice(ai, 1, south, north);
const pi = plan.boxes.findIndex((b) => b.zone === R && same(b.min, [X(-9), 0, Z(16)]));
if (pi < 0) throw new Error("the west arcade pier moved upstream");
plan.boxes.splice(pi, 1);
const ri = plan.ramps.findIndex((r) => r.zone === R && same(r.from, [X(-10), Z(30)]));
const gapWhy = "the AISLE GAP's RAMP (round 4, GG review; GG NIGHT20 'a route that enemies can climb up'): in the 5 m gap under the clerestory ride, against the west wall, 2.4 m over 5 m up to the aisle's south part: the walkers' way to the narthex wing and the south aisle (the ride passes over it; walking the aisle means dropping into the gap and climbing out)";
plan.ramps.splice(ri + 1, 0, { zone: R, from: [X(-10.75), Z(18)], to: [X(-10.75), Z(13)], width: 2.5, rise: 2.4, job: "climb", why: gapWhy, note: gapWhy });
// a spitter spot that stood in the gap moves onto the north part
const hi = room.highNodes.findIndex((n) => same(n, P(-10.5, 2.4, 17.5)));
if (hi < 0) throw new Error("v2's H3 moved upstream");
room.highNodes[hi] = P(-10.5, 2.4, 20);
for (const w of room.waveNodes43) for (let i = 0; i < w.length; i++) if (same(w[i], P(-10.5, 2.4, 17.5))) w[i] = P(-10.5, 2.4, 20);
// wave 2 (the spitters' wave) all on the east aisle, the grapple side: on the split west aisle's short north part its bodies dropped
// straight to the floor and pushed the walk pilot back into the entry mouth (isolated dwell 1.0 -> 2.05 s)
const E1 = P(10.5, 2.4, 12), E2 = P(10.5, 2.4, 14.5);
room.highNodes.push(E1, E2);
room.waveNodes43[1] = [P(10, 2.4, 21.5), P(10.5, 2.4, 17.5), E1, E2];
// the bell's lip and crown edge drawn (no collider: the footprint unchanged)
const edge = (id, lx, y, lz, w, why) => ({ id, room: R, kind: "edgeLight", at: P(lx, y, lz), size: [w, 0.08, 0.05], yawDeg: 0, collider: false, decor: true, job: "marker", why });
const pAt = plan.props.findIndex((q) => q.id === "v2-P1");
plan.props.splice(pAt + 1, 0,
  edge("v2-bell-lip-s", 0, 0.6, 12.97, 6.4, "THE BELL's flared lip (round 4, GG review: it read as a plain cube): the rim of its mouth on the floor, drawn along its south face"),
  edge("v2-bell-lip-n", 0, 0.6, 19.03, 6.4, "THE BELL's flared lip along its north face, either side of the beam"),
  edge("v2-bell-crown", 0, 4.95, 12.97, 5.6, "THE BELL's crown edge (its shoulder at 5 m) seen from the mouth"));
room.f52b_why = "NIGHT52 F round 4 (Tools/night/n52-loop-f-v2b.cjs): the west aisle split by a 5 m gap under the clerestory ride (a walkers' ramp in it to the south part), the west arcade pier cut, the bell's lip drawn";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v2 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v2b: v2 fixed (the aisle gap and its ramp, the west pier cut, the bell's lip drawn)");

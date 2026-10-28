// NIGHT52 the room loop (the lead's; NIGHT52.md section 5's queue, D-25): iceline g3 2.3 THE SWARM (NIGHT46's THE TIPPLE: 26 x 28 x 9 m,
// 8 pieces, 1.10 per 100 m2, the hopper, a 12 m ride panel, the 2 m trestle, four coal cars) rebuilt in THE ROOM CONTRACT's spirit as THE
// TIPPLE II in Design/levels/n52/iceline.plan.json. A teach room (D-12): the lesson "FIVE KILLS: {altFire} BLASTS THE PACK", the crew (3 husks)
// and the swarm (8 swarmers) are kept; the contract's items are printed (types: husk + swarmer = 2).
// The card (docs/ROOMS.md "NIGHT52 loop cards"): the tipple keeps its identity (THE HOPPER and its chute on the door's side, the west lane
// where the swarm packs, the trestle along the east wall, the coal cars); the room is 2 m narrower (24 x 28 m: x -12..12, the trestle 1 m in)
// and 1 m taller (10 m: the hopper still meets the roof); every new piece answers a verb: two spill walls at the chute's mouth pack the swarm
// into the west lane (the blast's place); the ride panel runs 18 m from a run-up block; a 4 m SE loft by its ramp is the third tier (the air
// over the crew); the tipple head (2 m) runs the trestle to the exit wall; the tipple hoist (pad) lifts you out of the swarm onto the trestle's
// health, the loft hoist from the west lane onto the loft. Every other room, door and piece of n52-iceline bit for bit (asserted).
// Run: node Tools/night/n52-loop-g3.cjs, then bash Tools/night/n52-loop-regen.sh iceline g3
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-iceline") throw new Error("unexpected plan " + plan.id);
const g3 = plan.rooms.find((r) => r.id === "g3");
if (!g3 || JSON.stringify(g3.rect) !== "[-13,-90,13,-62]" || g3.loop52_why) throw new Error("g3 is not NIGHT46's (already run?)");
const Z = "g3";
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });
const near = (a, b) => JSON.stringify(a) === JSON.stringify(b);

g3.rect = [-12, -90, 12, -62];
g3.height = 10;
g3.verb36 = "the LT blast: fill the glove on the crew, then blast the swarm where the spill walls pack it into the west lane; ride the panel past the pack, hoist out of it onto the trestle, or hold the loft and drop on it";
g3.nodes = [[-8.5, 0, -75], [-9.5, 0, -77.5], [0, 0, -76.5], [0.5, 0, -78.5], [-6, 0, -86.5], [4, 0, -86], [8.5, 2, -88]];
g3.nodes_why = "NIGHT46 A3 (THE TIPPLE): the chute's mouth and the far floor's corners; NIGHT52 loop: the NE corner's node on the tipple head (2 m: spawns on two heights)";
g3.waveNodes43 = [[[-9.5, 0, -74], [-10, 0, -76], [5, 0, -77], [8, 0, -75]], [[-8.5, 0, -75], [-9.5, 0, -77.5], [0, 0, -76.5], [0.5, 0, -78.5], [-6, 0, -86.5], [4, 0, -86], [8.5, 2, -88]]];
g3.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [0, 0, -64, 0, 0.02], mid: [-3, 0, -68, 12, 0.04], high: [7, 4, -64.5, -20, -0.14],
};
g3.proofs = [
  { kind: "walk", points: [[0, 0, -64], [-8.5, 0, -71], [-8.5, 0, -81], [-3.5, 0, -84], [0, 0, -88]] },
  { kind: "walk", points: [[0, 0, -64], [3, 0, -73], [3, 0, -82], [0, 0, -88]] },
  { kind: "walk", points: [[10.5, 0, -64.5], [10.5, 2, -71], [10.5, 2, -84.5]] },
  { kind: "walk", points: [[5.5, 0, -75], [5.5, 4, -66], [7, 4, -64]] },
];
g3.loop52_why = "NIGHT52 the room loop (Tools/night/n52-loop-g3.cjs; THE ROOM CONTRACT's spirit, a teach room): NIGHT46's THE TIPPLE rebuilt as THE TIPPLE II: 24 x 28 x 10 m; 17 pieces; three tiers (floor, the 2 m trestle and tipple head, the 4 m SE loft), each on a walkers' ramp; the 18 m ride panel; the tipple and loft hoists (pads); two spill walls pack the swarm into the west lane; the lesson and waves kept.";

const old = plan.boxes.filter((b) => b.zone === Z);
const pick = (pred, what) => { const h = old.filter(pred); if (h.length !== 1) throw new Error(what + ": " + h.length); return h[0]; };
const hopper = pick((b) => near(b.min, [-7, 0, -82]), "hopper");
const panel = pick((b) => b.ride && near(b.min, [-11.6, 0, -78]), "ride panel");
const trestle = pick((b) => near(b.min, [10, 0, -86]), "trestle");
const cars = old.filter((b) => b.max[1] === 0.9);
if (cars.length !== 4 || old.length !== 7) throw new Error("g3 boxes are not NIGHT46's seven");
hopper.max = [-1, 10, -74];
hopper.why += " (NIGHT52 loop: to the 10 m roof)"; hopper.note = hopper.why;
panel.min = [-11.6, 0, -84];
panel.ride36 = { face: "E", planeX: -11, lz: [4, 22] };
panel.why += " (NIGHT52 loop: 18 m, z -66..-84, from the run-up block past the whole west lane: the ride over the packed swarm)"; panel.note = panel.why;
trestle.min = [9, 0, -86]; trestle.max = [12, 2, -70];
trestle.why += " (NIGHT52 loop: 1 m in with the narrower room, x 9..12)"; trestle.note = trestle.why;
const se = pick((b) => b.max[1] === 0.9 && near(b.min, [3.75, 0, -70.75]), "SE car");
se.min = [7.5, 0, -80]; se.max = [10, 0.9, -78.5];
se.why += " (NIGHT52 loop: moved off the loft ramp's foot to the east lane, flush with the trestle: no slot)"; se.note = se.why;

const boxes = old.concat([
  box("deck", [4, 0, -66], [9, 4, -62], "line", "THE LOFT (4 m, 5 x 4 m on the door wall; the third tier): the tipple's weigh loft over the door landing: hold it and drop on the crew, or blast down into the swarm as it rounds the hopper; the walkers follow you up its ramp (no safe ledge, A12)"),
  box("hull", [4, 4, -64], [6, 6.5, -62], "cover", "the LOFT's WEIGH HOUSE (2 x 2 m on its back corner): breaks the line from the far floor to the loft's west half: the loft is a stand you hold by moving, not a nest"),
  box("deck", [6.5, 0, -90], [12, 2, -86], "line", "THE TIPPLE HEAD (2 m, 5.5 x 4 m): the trestle runs on to the exit wall: the second wave's NE spawn stands on it, you drop off it onto the exit floor or blast down from it"),
  box("hull", [-7.5, 0, -74], [-7, 10, -71], "line", "the WEST SPILL WALL (0.5 x 3 m, floor to roof at the chute's mouth: a 3 m top was a perch no walker reached, hold52 2 hits): the swarm pouring from the chute turns west into the west lane and packs there: the blast's place (the lesson)"),
  box("hull", [-1, 0, -74], [-0.5, 10, -71], "line", "the EAST SPILL WALL (the chute's other cheek, floor to roof: its 3 m top was a 0-hit perch): keeps the swarm off the centre lane on its first pour, so the pack reaches you through the west lane"),
  box("hull", [-11, 0, -66], [-9, 1.2, -63], "line", "the WEST RUN-UP BLOCK (1.2 m, 2 x 3 m at the ride panel's door end; a solid block, not a stand): hop on, jump onto the panel, ride north past the packed swarm"),
]);
const ramps = [
  ramp([10.5, -64], [10.5, -70], 3, 2, "the TRESTLE's RAMP (2 m over 6 m, at the door end): the walkers' way up (NIGHT52 loop: 1 m in with the trestle)"),
  ramp([5.5, -74], [5.5, -66], 3, 4, "THE LOFT's RAMP (4 m over 8 m, its foot at the hopper's east lane): your way up and the walkers' (GG NIGHT20: a route enemies can climb)"),
];
const pads = [
  tag({ at: [-10.5, 0, -88.5], target: [10.5, 2, -83], apexAbove: 4, job: "line", why: "THE TIPPLE HOIST (a jump pad in the NW corner, the upward line, A5): out of the packed west lane over the far floor onto the trestle's north end and its health", note: "the tipple hoist" }),
  tag({ at: [-8, 0, -66.5], target: [6.5, 4, -64.5], apexAbove: 3, job: "line", why: "THE LOFT HOIST (a jump pad at the west lane's door end): over the door landing onto the loft (4 m): the high ground before the swarm pours", note: "the loft hoist" }),
];
const props = (plan.props || []).filter((q) => q.room === Z).map((q) => {
  if (q.kind === "healthPickup") return Object.assign({}, q, { at: [10.5, 2.9, -84.5], why: q.why + " (NIGHT52 loop: 1 m in with the trestle)" });
  return q;
});
const lights = (plan.lights || []).filter((b) => b.room === Z || b.zone === Z).map((l) => {
  const o = Object.assign({}, l, { at: l.at.slice() });
  if (o.kind === "ceiling" && o.at[1] === 8.95) o.at[1] = 9.95;
  if (o.id === "g3-L3") o.at[0] = 10.5;
  return o;
});
lights.push({ id: "g3-L7", room: Z, kind: "ceiling", at: [6.5, 9.95, -64], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 9, intensity: 1, why: "the loft's lamp: the third tier reads from the far floor" });

function swap(arr, isZ, fresh) {
  const at = arr.findIndex(isZ);
  const rest = arr.filter((x) => !isZ(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === Z, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === Z, ramps);
plan.props = swap(plan.props, (b) => b.room === Z, props);
plan.lights = swap(plan.lights, (b) => b.room === Z || b.zone === Z, lights);
plan.pads = (plan.pads || []).filter((b) => b.zone !== Z).concat(pads);

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple", "surfaces"]) {
  const keepA = (before[k] || []).filter((b) => b.zone !== Z && b.room !== Z), keepB = (plan[k] || []).filter((b) => b.zone !== Z && b.room !== Z);
  if (JSON.stringify(keepA) !== JSON.stringify(keepB)) throw new Error(k + ": a row outside g3 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-g3: g3 THE TIPPLE II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads)");

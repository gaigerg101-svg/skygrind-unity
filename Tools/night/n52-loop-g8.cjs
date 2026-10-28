// NIGHT52 the room loop, cycle 3 (the lead's): iceline g8 2.8 THE STAMPEDE rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2), keeping
// NIGHT47's card THE ROUNDHOUSE (six columns making three lanes, THE PIT along the centre lane, the gallery, the turntable gate, the lessons
// "Hit its back with {fire}." / "Lead one into the pit.", the waves of chargers), in Design/levels/n52/iceline.plan.json (n47's g8: 24 x 30 x 9 m,
// 0.97 per 100 m2 by the forge's count, two 0.9 m coal carts (waist-high cover: cut, RESEARCH A11), one ride panel, no upward line, one tier).
// THE ROUNDHOUSE II: 22 x 30 x 12 m; the columns and the pit kept; two 20 m ride panels (both lanes' walls); the west gallery (3 m) and THE SIGNAL
// BOX (5.5 m, the third tier) over the exit, each on a walkers' ramp; two full-height sand towers in the outer lanes (more columns a charge can
// end in: the stun); the cross hoist (a pad: lane W -> the signal box over the pit); a health pickup at the pit's far end (the stampede's lane).
// Every other room, door and piece of n52-iceline is as before this script (asserted). Run: node Tools/night/n52-loop-g8.cjs, then
// bash Tools/night/n52-loop-regen.sh iceline g8
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-iceline") throw new Error("unexpected plan " + plan.id);
const Z = "g8";
const room = plan.rooms.find((r) => r.id === Z);
if (!room || JSON.stringify(room.rect) !== "[-12,-250,12,-220]") throw new Error("g8 is not n47's (already run?)");
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });

room.rect = [-11, -250, 11, -220];
room.height = 12;
room.verb36 = "the late dodge into a column or a sand tower (the charge ends in it: the stun), then the punish from behind with the right tool; ride the lane walls past the stampede, take the cross hoist to the signal box";
room.nodes = [[-5, 0, -245], [5, 0, -245], [0, 0, -244.5]];
room.highNodes = [[-7, 3, -248], [6, 5.5, -248]];
room.highNodes_why = "NIGHT52 loop (R-4): the spitters' stands: the west gallery (3 m) and THE SIGNAL BOX (5.5 m) over the exit, 25-28 m from the door";
room.waveNodes43 = [
  [[-5, 0, -245], [5, 0, -245]],
  [[-5, 0, -245], [5, 0, -245], [-8.3, 0, -236], [8.3, 0, -233]],
  [[-5, 0, -245], [5, 0, -245], [0, 0, -244.5], [-7, 3, -248], [6, 5.5, -248]],
];
room.waveNodes43_why = "NIGHT52 loop (THE ROUNDHOUSE II): wave 1's charger at a lane's far end in view from the door past the columns; wave 2's two chargers at the lane ends and the husks in the outer lanes between a sand tower and a ramp's foot; wave 3's three chargers at the three lane ends (the centre one's charge crosses THE PIT) and the spitters on the gallery and the signal box. Never behind the door";
room.frames36 = { mid: [6, 0, -223, 17.526, -0.03], exit: [-1.5, 0, -243.5, 0, -0.06] };
room.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [0, 0, -222, 0, 0.02], mid: [-3, 0, -225, 5, 0.05], high: [7, 5.5, -248, 170, -0.15],
};
room.proofs = [
  { kind: "walk", points: [[0, 0, -222], [-7, 0, -226], [-7, 0, -238], [-3, 0, -245], [0, 0, -248]] },
  { kind: "walk", points: [[0, 0, -222], [7, 0, -226], [7, 0, -233], [3, 0, -245], [0, 0, -248]] },
  { kind: "walk", points: [[-8.9, 0, -238.5], [-8.9, 3, -246], [-7, 3, -248]] },
  { kind: "walk", points: [[8.9, 0, -234.5], [8.9, 5.5, -246], [7, 5.5, -248]] },
];
room.loop52_why = "NIGHT52 the room loop, cycle 3 (Tools/night/n52-loop-g8.cjs; THE ROOM CONTRACT): NIGHT47's THE ROUNDHOUSE kept and finished as THE ROUNDHOUSE II: 22 x 30 x 12 m; 17 pieces; three tiers (floor, 3 m gallery, 5.5 m signal box) each on a walkers' ramp; two 20 m ride panels; the cross hoist (a pad); the coal carts (waist-high) cut; the columns, the pit, the lessons and the waves kept.";

const boxes = [
  box("deck", [-11, 0, -250], [-3, 3, -246], "line", "THE GALLERY (west, 3 m, 8 x 4 m): the coaling gallery, a spitter stand 25 m from the door (the KESTREL's range); the walkers' ramp climbs to it along the west lane wall (no safe ledge)"),
  box("deck", [3, 0, -250], [11, 5.5, -246], "line", "THE SIGNAL BOX (east, 5.5 m, 8 x 4 m; the third tier): the roundhouse's signal box over the exit, the second spitter stand and your high ground over the stampede: its ramp, the cross hoist, or a jump off the east ride's end"),
  box("wall", [-11, 0, -243], [-10.4, 5, -223], "line", "the WEST LANE WALL (ride, cyan, 20 m x 5 m): ride lane W past the charging pack (a charge that misses you ends in the wall: the stun) and off its end up the gallery's ramp", { ride: true, ride36: { face: "E", planeX: -10.4, lz: [3, 23] } }),
  box("wall", [10.4, 0, -243], [11, 5, -223], "line", "the EAST LANE WALL (ride, cyan, 20 m x 5 m; NIGHT47's east panel, now with its mirror): ride lane E and jump off its end onto the signal box's ramp", { ride: true, ride36: { face: "W", planeX: 10.4, lz: [3, 23] } }),
  box("hull", [-9.05, 0, -232.75], [-7.55, 4, -231.25], "cover", "the WEST SAND TOWER (1.5 x 1.5 x 4 m, full height, replacing the waist-high coal cart): a column in lane W a charge ends in (the stun: hit its back), the lane's pivot"),
  box("hull", [7.55, 0, -230.75], [9.05, 4, -229.25], "cover", "the EAST SAND TOWER (offset from the west one: asymmetric lanes): the charge's end in lane E, the pivot you dodge round"),
  box("hull", [-11, 3, -250], [-9, 5.5, -248], "cover", "the GALLERY's SAND HOPPER (2 x 2 m on its back corner): the gallery spitter's shield from the door; flank it off the west ride or up the ramp"),
  box("hull", [9, 5.5, -250], [11, 7.5, -248], "cover", "the SIGNAL BOX's LEVER FRAME (2 x 2 m on its back corner): the signal box spitter's shield; the cross hoist lands you round it"),
];
const ramps = [
  ramp([-8.9, -239], [-8.9, -246], 3, 3, "the GALLERY's walkers' ramp (3 m over 7 m) against the west lane wall: the walkers' and spitters' way up (GG NIGHT20), yours"),
  ramp([8.9, -235], [8.9, -246], 3, 5.5, "THE SIGNAL BOX's walkers' ramp (5.5 m over 11 m) against the east lane wall: the climb route enemies use to the third tier (no safe ledge, A12)"),
];
const pads = [
  tag({ at: [-8.3, 0, -237.5], target: [6.5, 5.5, -248], apexAbove: 3, job: "line", why: "THE CROSS HOIST (a jump pad in lane W, the upward line, A5): over the pit and lane C onto the signal box (5.5 m): out of the stampede onto the spitters' high stand, between the column pairs", note: "the cross hoist" }),
];
const props = (plan.props || []).filter((p) => p.room === Z).map((p) => {
  if (p.kind === "pillar") return p; // the six columns (2.5 x 2.5 x 5 m, collider): kept as NIGHT47 placed them (x +-5)
  if (p.id === "g8-P7") return p; // THE TURNTABLE GATE (decor): kept
  throw new Error("unexpected g8 prop " + p.id);
});
props.push({ id: "g8-P8", room: Z, kind: "healthPickup", at: [0, 0.9, -242], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health at THE PIT's far end in lane C, where the centre charger runs (A20: the best at the riskiest spot): cross the pit's lane under the stands to take it" });
const lights = [
  { id: "g8-L1", room: Z, kind: "ceiling", at: [0, 11.95, -223], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 12, intensity: 1, why: "the door end's panel" },
  { id: "g8-L2", room: Z, kind: "ceiling", at: [0, 11.95, -234], normal: [0, -1, 0], size: [4, 10, 0.1], color: "white", range: 12, intensity: 1, why: "the lanes and the pit read" },
  { id: "g8-L3", room: Z, kind: "ceiling", at: [0, 11.95, -243], normal: [0, -1, 0], size: [6, 3, 0.1], color: "amber", range: 12, intensity: 1.1, why: "the far end's warm panel: the stampede's start" },
  { id: "g8-L5", room: Z, kind: "wall", at: [-7.5, 5, -249.95], normal: [0, 0, 1], size: [1.2, 0.5, 0.1], color: "white", range: 9, intensity: 1.2, why: "the gallery's lamp (white): the spitters' stand reads" },
  { id: "g8-L6", room: Z, kind: "wall", at: [7.5, 8, -249.95], normal: [0, 0, 1], size: [1.2, 0.5, 0.1], color: "white", range: 9, intensity: 1.2, why: "the signal box's lamp, above its deck" },
  { id: "g8-L4", room: Z, kind: "pool", at: [0, 0.025, -248], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6, why: "the exit's cyan pool (A-6)" },
];

function swap(arr, isZ, fresh) {
  const at = arr.findIndex(isZ);
  if (at < 0) throw new Error("no g8 row to replace");
  const rest = arr.filter((x) => !isZ(x));
  rest.splice(at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === Z, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === Z, ramps);
plan.props = swap(plan.props, (b) => b.room === Z, props);
plan.lights = swap(plan.lights, (b) => b.room === Z || b.zone === Z, lights);
plan.pads = (plan.pads || []).filter((b) => b.zone !== Z).concat(pads);
// THE PIT stays (n47's row, unchanged)
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const a = (before[k] || []).filter((b) => b.zone !== Z && b.room !== Z), b = (plan[k] || []).filter((x) => x.zone !== Z && x.room !== Z);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside g8 changed");
}
if (JSON.stringify(before.pits) !== JSON.stringify(plan.pits)) throw new Error("the pits changed");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-g8: g8 THE ROUNDHOUSE II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pad, " + props.length + " props)");

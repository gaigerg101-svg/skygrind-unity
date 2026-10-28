// NIGHT52 the room loop (the lead's; NIGHT52.md section 5's queue: iceline g4 0.83 per 100 m2): iceline g4 2.4 THE REACH rebuilt in THE ROOM
// CONTRACT's spirit as THE REACH II, in Design/levels/n52/iceline.plan.json. g4 is a teach room (the drone's orb: "{fire} AS THE YELLOW ORB
// ARRIVES"; waves drone / drone + drone), so the contract's enemy-types item is exempt (one type is the lesson, A-7); every other item is printed.
// The card (docs/ROOMS.md "NIGHT52 loop cards"): the room keeps its identity (the frozen canal across the middle, the centre bridge and the east
// bridge, the canal's steps, the NW ledge, the drones over the far side, the lesson); every new piece answers a verb: two lock walls ride you
// along the side walls over the canal (the crossing for a rider, not a bridge-walker), each with a 1.2 m run-up block at the door end; the NW
// ledge rises to 2 m; a 4 m NE gallery is the third tier (the drones' high side: you take the hoist pad up and parry from above, or punch them
// at the same height); a canal hoist and a return hoist (pads) are the upward lines across the canal; two ice posts on the door floor break the
// first drone's orb line to the landing; covers on the ledge and the gallery's back corners; the health on the centre bridge (the riskiest spot,
// under the orbs). Every other room, door and piece of n52-iceline bit for bit (asserted).
// Run: node Tools/night/n52-loop-g4.cjs, then bash Tools/night/n52-loop-regen.sh iceline g4
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-iceline") throw new Error("unexpected plan " + plan.id);
const g4 = plan.rooms.find((r) => r.id === "g4");
if (!g4 || JSON.stringify(g4.rect) !== "[-12,-124,12,-94]" || g4.loop52_why) throw new Error("g4 is not n47's (already run?)");
if (plan.boxes.filter((b) => b.zone === "g4").length !== 5) throw new Error("g4 boxes are not n47's five");
const Z = "g4";
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });

g4.verb36 = "the glove cannot reach a flyer: punch its orb back at it; ride the lock walls over the canal, take the hoist to the gallery and meet the drones at their height";
g4.nodes = [[0, 0, -118], [-3, 0, -119], [4.5, 0, -120], [3, 0, -121]];
g4.nodes_why = "n47's four drone nodes over the far floor; NIGHT52 loop: the east one 1.5 m off the NE gallery's corner (it was on it)";
g4.highNodes = [[-9, 2, -121], [8, 4, -121.5]];
g4.highNodes_why = "NIGHT52 loop: the NW ledge (2 m) and the NE gallery (4 m): the drones' high side across the canal";
g4.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [0, 0, -96, 0, 0.02], mid: [-3.5, 0, -101, 10, 0.06], high: [9, 4, -121.5, 160, -0.12],
};
g4.proofs = [
  { kind: "walk", points: [[0, 0, -96], [0, 0, -104], [0, 0, -114], [0, 0, -122]] },
  { kind: "walk", points: [[-10, -1, -110.5], [-10, -0.5, -107.5], [-10, 0, -104]], jump: true, maxSeconds: 8 },
];
g4.loop52_why = "NIGHT52 the room loop (Tools/night/n52-loop-g4.cjs; THE ROOM CONTRACT's spirit, a teach room: the enemy-types item exempt): n47's THE REACH rebuilt as THE REACH II: 24 x 30 x 10 m; 19 pieces; three tiers (floor, the 2 m NW ledge, the 4 m NE gallery, each on a walkers' ramp); two 17.5 m lock walls over the canal; the canal hoist and the return hoist (pads); the lesson and the waves kept.";

const boxes = [
  box("deck", [-2, -1, -112], [2, 0, -106], "line", "the BRIDGE (4 m wide) over the canal: the one walk across, under the drone's orbs (the parry's place); the health sits on it (n47's)"),
  box("deck", [6, -1, -112], [9, 0, -106], "line", "the EAST BRIDGE (3 m wide; NIGHT46 A3 I-1): the canal's second crossing, a loop with the centre bridge; it lands at the hoist pad"),
  box("deck", [-11, -1, -109], [-9, -0.5, -106], "climb", "the canal's west step (0.5 m under the floor, 2 m wide beside the west lock wall): the way back up from a fall"),
  box("deck", [9, -1, -109], [11, -0.5, -106], "climb", "the canal's east step (the west's mirror, beside the east lock wall): the way back up from a fall"),
  box("deck", [-12, 0, -124], [-6, 2, -118], "line", "the NW LEDGE (2 m; NIGHT52: from n47's 1.5 m): the far side's high ground under the drones (their orbs come from over it); the west lock wall's ride ends at its ramp"),
  box("wall", [-12, -1, -112.5], [-11, 5, -95], "line", "the WEST LOCK WALL (ride, cyan, 17.5 m x 5 m standing in the canal's west end): ride it from the run-up block over the canal to the NW ledge's ramp: the crossing for a rider, not a bridge-walker (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "E", planeX: -11, lz: [1, 18.5] } }),
  box("wall", [11, -1, -112.5], [12, 5, -95], "line", "the EAST LOCK WALL (ride, cyan, the west's mirror): ride it over the canal's east end and jump off its end at the hoist pad onto the gallery", { ride: true, ride36: { face: "W", planeX: 11, lz: [1, 18.5] } }),
  box("deck", [6, 0, -124], [12, 4, -119], "line", "the NE GALLERY (4 m, 6 x 5 m; the third tier): the drones' high side: the canal hoist puts you on it to parry an orb from above or punch a drone at its own height; the exit is below you"),
  box("hull", [10, 4, -124], [12, 6.5, -122], "cover", "the GALLERY's WINCH HOUSE (2 x 2 m on its back corner): your shield from the far drone's orb on the gallery (a stand you must move to break)"),
  box("hull", [-12, 2, -124], [-10, 4.5, -122], "cover", "the NW LEDGE's ICE HOPPER (2 x 2 m on its back corner): the ledge's shield from the orbs over the canal"),
  box("hull", [-5.6, 0, -101.6], [-4.4, 4, -100.4], "cover", "the WEST ICE POST (1.2 m x 4 m) on the door floor: breaks the first drone's orb line to the landing's west half; the pivot you step round while you read the orb"),
  box("hull", [4.4, 0, -101.6], [5.6, 4, -100.4], "cover", "the EAST ICE POST (the west's mirror): breaks the orb line to the landing's east half"),
  box("hull", [-11, 0, -99], [-8, 1.2, -96], "line", "the WEST RUN-UP BLOCK (1.2 m, 3 x 3 m against the west lock wall's door end; a solid block, not a stand): hop on, jump onto the wall, ride north over the canal"),
  box("hull", [8, 0, -99], [11, 1.2, -96], "line", "the EAST RUN-UP BLOCK (the west's mirror): onto the east lock wall; the return hoist lands on it"),
];
const ramps = [
  ramp([-9, -113], [-9, -118], 3, 2, "the NW ledge's ramp (2 m over 5 m): your way up, and the west lock wall's landing"),
  ramp([7.5, -112], [7.5, -119], 3, 4, "the NE GALLERY's ramp (4 m over 7 m) straight off the east bridge: the walkers' route onto the third tier (GG NIGHT20: a route enemies can climb), and yours without the hoist"),
];
const pads = [
  tag({ at: [4, 0, -116], target: [8.5, 4, -121.5], apexAbove: 3, job: "line", why: "THE CANAL HOIST (a jump pad, the upward line, A5): from the far floor off the east bridge onto the NE gallery (4 m): meet the drones at their height", note: "the canal hoist" }),
  tag({ at: [-4, 0, -116], target: [9.5, 1.2, -97.5], apexAbove: 4, job: "line", why: "THE RETURN HOIST (a jump pad on the far floor): back over the canal onto the east run-up block (1.2 m) when the second wave's drones come over the door floor", note: "the return hoist" }),
  tag({ at: [-7, 0, -103], target: [-9, 2, -121], apexAbove: 3, job: "line", why: "THE LEDGE HOIST (a jump pad on the door floor, off the walk line): over the canal onto the NW ledge (2 m), the drones' near side", note: "the ledge hoist" }),
];
const props = (plan.props || []).filter((q) => q.room === Z);
if (props.some((q) => q.collider)) throw new Error("g4 had a solid prop");
props.push({ id: "g4-P1", room: Z, kind: "healthPickup", at: [0, 0.9, -109], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health on the centre bridge over the canal, under both drones' orbs (A20: the best at the riskiest spot)" });
const lights = (plan.lights || []).filter((b) => b.room === Z || b.zone === Z);
lights.push({ id: "g4-L5", room: Z, kind: "ceiling", at: [9, 9.95, -121.5], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 9, intensity: 1, why: "the NE gallery's lamp: the third tier reads from the door" });

function swap(arr, isG4, fresh) {
  const at = arr.findIndex(isG4);
  const rest = arr.filter((x) => !isG4(x));
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
  if (JSON.stringify(keepA) !== JSON.stringify(keepB)) throw new Error(k + ": a row outside g4 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-g4: g4 THE REACH II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads)");

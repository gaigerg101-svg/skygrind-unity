// NIGHT52 the room loop, cycle 2 (the lead's): iceline g5 2.5 THE COOLDOWN rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2) as THE ICEHOUSE,
// in Design/levels/n52/iceline.plan.json (n47's g5: 26 x 30 x 9 m, 6 pieces, 0.77 per 100 m2, 9 m ride panels, no upward line, decks at 1.5 m).
// The card (docs/ROOMS.md "NIGHT52 loop cards"): the ice line's cold store, where the cut blocks wait for the barges; the swarm nests on the
// shelves. The lesson stays ("SAVE {altFire} FOR THE SECOND PACK": punch the bait, blast the pack that pours off the shelf) and the waves stay;
// the room is 22 x 30 x 12 m, a ring round THE ICE STACK (roof-high, the circuit's centre), with three tiers: the floor, the west shelf (2 m: the
// second pack's pour) and the east crane deck (4.5 m: the warden's drones' perch), both on walkers' ramps; 18 m ride walls along both sides
// (the west one ends on the shelf, the east one jumps to the crane), two inner ice walls (short rides, the three lanes), ice posts (LOS), and two
// pads: the ice hoist (floor -> crane) and the shelf hoist (shelf -> crane over the exit: the high circuit).
// Every other room, door and piece of n52-iceline is as before this script (asserted). Run: node Tools/night/n52-loop-g5.cjs, then
// bash Tools/night/n52-loop-regen.sh iceline g5
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-iceline") throw new Error("unexpected plan " + plan.id);
const Z = "g5";
const room = plan.rooms.find((r) => r.id === Z);
if (!room || JSON.stringify(room.rect) !== "[-13,-158,13,-128]") throw new Error("g5 is not n47's (already run?)");
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });

room.rect = [-11, -158, 11, -128];
room.height = 12;
room.verb36 = "the blast's 15 s cooldown: punch the bait on the floor, blast the second pack where it pours off the west shelf; ride the side walls or take the ice hoist to the crane when the warden's drones come";
room.nodes = [[-4.5, 0, -146.5], [4.5, 0, -146.5], [0, 0, -149], [-2, 0, -153], [2, 0, -153]];
room.nodes_why = "NIGHT52 loop (R-4, A18): the far floor round THE ICE STACK, the first two nodes outside the stack's shadow from the door (in view on entry: the bait pulls you into the ring; with them behind the stack the walk pilot waited at the door, dwell 0.9 -> 1.5 s)";
room.highNodes = [[-8, 2, -152], [-8, 2, -156], [7.5, 4.5, -152], [7.5, 4.5, -155.5]];
room.highNodes_why = "NIGHT52 loop (R-4): the west shelf (2 m: the second pack pours off it, the blast's target) and the east crane deck (4.5 m: the drones' perch over the warden)";
room.frames36 = { mid: [3, 0, -131, 15, -0.045], exit: [0, 0, -150, 0, -0.075] };
room.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [0, 0, -130, 0, 0.02], mid: [-2.5, 0, -137.5, 10, 0.04], high: [8, 4.5, -153, 161, -0.12],
};
room.proofs = [
  { kind: "walk", points: [[0, 0, -130], [0, 0, -138], [-3.5, 0, -143], [0, 0, -149], [0, 0, -156]] },
  { kind: "walk", points: [[-8.5, 0, -139], [-8.5, 2, -146], [-8, 2, -152]] },
];
room.loop52_why = "NIGHT52 the room loop, cycle 2 (Tools/night/n52-loop-g5.cjs; THE ROOM CONTRACT): n47's THE COOLDOWN rebuilt as THE ICEHOUSE: 22 x 30 x 12 m; 17 pieces; three tiers (floor, 2 m shelf, 4.5 m crane deck), each on a walkers' ramp; two 18 m ride walls; the ice hoist and the shelf hoist (pads); a ring round the ice stack; the waves and the lesson kept.";

const boxes = [
  box("hull", [-2, 0, -145], [2, 12, -141], "cover", "THE ICE STACK (4 x 4 m, floor to roof; the landmark dead ahead of the door): the cut blocks waiting for the barges; the pillar the room's ring runs round (circle it while the blast cools) and the block that hides the far floor's bait from the door until you commit"),
  box("deck", [-11, 0, -158], [-5, 2, -146], "line", "the WEST SHELF (2 m, 6 x 12 m): the second pack pours off it at once (the blast's target: stand off its ramp and blast the pour); the west ride lands on it"),
  box("deck", [5, 0, -158], [11, 4.5, -149], "line", "THE CRANE DECK (4.5 m, 6 x 9 m; the third tier): the drones' perch over the warden in wave 3; reached by its walkers' ramp, the ice hoist, the shelf hoist, or a jump off the east ride's end (no safe ledge: the walkers follow up the ramp)"),
  box("wall", [-11, 0, -146.5], [-10, 6, -128.5], "line", "the WEST RIDE WALL (cyan, 18 m x 6 m along the side wall): ride it from the door end past the stack and drop onto the shelf: the fast line onto the pour, over the bait's heads (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "E", planeX: -10, lz: [0.5, 18.5] } }),
  box("wall", [10, 0, -146.5], [11, 6, -128.5], "line", "the EAST RIDE WALL (cyan, the west's mirror): ride it and jump off its end onto the crane deck (2.5 m on, 4.5 m up), or wall-jump back into the ring", { ride: true, ride36: { face: "W", planeX: 10, lz: [0.5, 18.5] } }),
  box("wall", [-5.3, 0, -139], [-4.7, 4, -131], "line", "the WEST ICE WALL (cyan, 8 m x 4 m): the short ride from the door into the west lane, and the wall that splits the first pack (the bait) into lanes for the punch", { ride: true, ride36: { face: "W", planeX: -5.3, lz: [3, 11] } }),
  box("wall", [4.7, 0, -139], [5.3, 4, -131], "line", "the EAST ICE WALL (cyan, the west's mirror): the short ride into the east lane toward the crane's ramp", { ride: true, ride36: { face: "E", planeX: 5.3, lz: [3, 11] } }),
  box("hull", [-8.1, 0, -134.6], [-6.9, 3.5, -133.4], "cover", "the WEST ICE POST (1.2 m x 3.5 m) in the west lane by the door: the bait splits round it; the punch's pivot; it breaks the shelf's line to the door"),
  box("hull", [6.9, 0, -134.6], [8.1, 3.5, -133.4], "cover", "the EAST ICE POST (the west's mirror): breaks the crane deck's line to the door; the pivot in the east lane"),
  box("hull", [-4.1, 0, -151.6], [-2.9, 3.5, -150.4], "cover", "the FAR WEST POST (1.2 m x 3.5 m) on the far floor: the pack pouring off the shelf rounds it into the blast's cone; it breaks the crane's line onto the shelf's foot"),
  box("hull", [2.9, 0, -151.6], [4.1, 3.5, -150.4], "cover", "the FAR EAST POST (the west's mirror): the crane's ramp foot; the warden circles it to reach you"),
  box("hull", [-11, 2, -158], [-9, 4.5, -155], "cover", "THE ICE SAW (2 x 3 m on the shelf's back corner): the shelf's one shield; a spitter-free corner the pack bunches in (the blast's best target), flanked off the west ride"),
  box("hull", [9, 4.5, -158], [11, 6.5, -156], "cover", "THE CRANE CAB (2 x 2 m on the crane deck's back corner): the drones' cover from the door; the ice hoist lands you beside it"),
];
const ramps = [
  ramp([-8.5, -140], [-8.5, -146], 3, 2, "the west shelf's walkers' ramp (2 m over 6 m) against the west ride wall: the second pack's way down to you and yours up (the blast down its throat)"),
  ramp([8.5, -140], [8.5, -149], 3, 4.5, "THE CRANE DECK's walkers' ramp (4.5 m over 9 m) against the east ride wall: the warden walks up after you (no safe ledge, A12); the climb route enemies use (GG NIGHT20)"),
];
const pads = [
  tag({ at: [2.5, 0, -136], target: [8, 4.5, -154], apexAbove: 3, job: "line", why: "THE ICE HOIST (a jump pad, the upward line, A5): from the floor past the east ice wall and the stack onto the crane deck (4.5 m): the way up when the drones come, over the pack's heads", note: "the ice hoist" }),
  tag({ at: [-7.5, 2, -150], target: [7.5, 4.5, -152.5], apexAbove: 2.5, job: "line", why: "THE SHELF HOIST (a jump pad on the west shelf): from the shelf over the exit to the crane deck: the high circuit (shelf, crane, the ramp down, the ring, the west ride, the shelf) so you never stop on a tier", note: "the shelf hoist" }),
];
const props = [
  { id: "g5-P1", room: Z, kind: "healthPickup", at: [0, 0.9, -147.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health right behind the stack on the far floor, under the shelf's pour and the crane's drones (A20: the best at the riskiest spot): the ring's far side pays" },
];
const lights = [
  { id: "g5-L1", room: Z, kind: "ceiling", at: [0, 11.95, -132], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 12, intensity: 1, why: "the door end's panel: the lanes and the ice walls read" },
  { id: "g5-L2", room: Z, kind: "ceiling", at: [0, 11.95, -147.5], normal: [0, -1, 0], size: [4, 4, 0.1], color: "amber", range: 12, intensity: 1.1, why: "the far floor behind the stack: the bait's ground (the room's one warm accent)" },
  { id: "g5-L3", room: Z, kind: "ceiling", at: [-8, 11.95, -152], normal: [0, -1, 0], size: [4, 6, 0.1], color: "white", range: 12, intensity: 1, why: "the west shelf's lamp: the pour reads from the door" },
  { id: "g5-L4", room: Z, kind: "ceiling", at: [8, 11.95, -153.5], normal: [0, -1, 0], size: [4, 6, 0.1], color: "white", range: 12, intensity: 1, why: "the crane deck's lamp: the third tier reads" },
  { id: "g5-L5", room: Z, kind: "pool", at: [0, 0.025, -156], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6, why: "the exit's cyan pool (A-6)" },
];

function swap(arr, isZ, fresh) {
  const at = arr.findIndex(isZ);
  if (at < 0) throw new Error("no g5 row to replace");
  const rest = arr.filter((x) => !isZ(x));
  rest.splice(at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === Z, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === Z, ramps);
plan.props = swap(plan.props, (b) => b.room === Z, props);
plan.lights = swap(plan.lights, (b) => b.room === Z || b.zone === Z, lights);
plan.pads = (plan.pads || []).filter((b) => b.zone !== Z).concat(pads);
if ((plan.pits || []).some((b) => b.zone === Z)) throw new Error("g5 has pits: re-read");

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const a = (before[k] || []).filter((b) => b.zone !== Z && b.room !== Z), b = (plan[k] || []).filter((x) => x.zone !== Z && x.room !== Z);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside g5 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-g5: g5 THE ICEHOUSE written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads)");

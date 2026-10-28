// NIGHT52 the room loop, cycle 4 (the lead's): spire v6 5.6 THE CHOIR rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2), keeping NIGHT46's
// card THE ROPE CHOIR (the cantors' island in view from the door, the high stalls along the west, the organ loft and THE ORGAN, the bell-rope
// rings, the east ride wall, the waves), in Design/levels/n52/spire.plan.json (n47's v6: 28 x 42 x 16 m, 12 pieces, 1.02 per 100 m2, two 1.0 m
// half walls (waist-high: cut), the island 2.4 m and the stalls / loft 3.2 m in one cluster: two standing planes).
// THE ROPE CHOIR II: 24 x 32 x 16 m (x -23..1, z -101..-69: 4 m narrower, 10 m shorter, the door side fixed); three planes: the floor, the
// island (2.4 m) and the stalls (3.2 m), THE ORGAN LOFT raised to 6 m (the top tier) on a 12 m walkers' ramp; the island gets a north ramp (a
// loop over it), two roof-high bell posts on its front corners (LOS, the TORCH's cover); full-height rood screens replace the half walls; the
// bell hoist (pad, east lane -> loft) and the island hoist (pad, island -> loft); the three rings moved with their tiers.
// The link v7 (5.7 THE LONG BELL, a jump-dash exam: its gap, ceiling, liners, stair and meter untouched) is re-cut to meet v6's moved edge:
// its rect gains 10 m of plain floor before its takeoff (z1 -115 -> -105) and the door d6 moves with it (z -111..-115 -> -101..-105).
// Every other room, door and piece of n52-spire is as before this script (asserted). Run: node Tools/night/n52-loop-v6.cjs, then
// bash Tools/night/n52-loop-regen.sh spire v6
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-spire") throw new Error("unexpected plan " + plan.id);
const Z = "v6";
const room = plan.rooms.find((r) => r.id === Z);
if (!room || JSON.stringify(room.rect) !== "[-27,-111,1,-69]") throw new Error("v6 is not n47's (already run?)");
const v7 = plan.rooms.find((r) => r.id === "v7");
if (!v7 || JSON.stringify(v7.rect) !== "[-16,-150,-4,-115]") throw new Error("v7 is not n47's");
const d6 = plan.doors.find((d) => d.id === "d6");
if (!d6 || JSON.stringify(d6.rect) !== "[-12,-115,-8,-111]") throw new Error("d6 is not n47's");
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });

room.rect = [-23, -101, 1, -69];
room.exit = [-10, 0, -99];
room.verb36 = "a roofed choir in three heights: torch the file coming up the island, parry the drones' orbs, jump-dash or hoist to the organ loft, grapple the bell ropes between the tiers";
room.nodes = [[-9, 2.4, -84], [-4, 2.4, -84], [-9, 2.4, -89], [-3.5, 2.4, -85.5], [-6.5, 2.4, -86.5], [-6.5, 2.4, -90], [-1, 0, -93], [-3, 0, -97], [-12, 0, -97.5]];
room.highNodes = [[-20, 3.2, -92], [-20, 3.2, -88], [-19, 6, -98], [-21, 6, -97.5], [-16, 6, -97]];
room.grappleNodes = [[-6.5, 5.4, -86], [-20, 6.2, -86], [-15, 9, -98], [-0.5, 8, -81]];
room.grapple = "FOUR rings (the card's BELL ROPES' iron grips), moved with their tiers (NIGHT52): one 3 m over the island (floor -> island), one 3 m over the high stalls, one 3 m over the organ loft's east end (island -> loft, the top tier), one 8 m over the east lane's door end (the pull onto the east ride's line from the door)";
room.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["heavy", "husk", "drone", "drone"], ["heavy", "swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"]];
room.waves52_why = "NIGHT52 loop (A15: 2-4 roles in a room; the room had five): wave 2's spitter -> a husk; the drones are wave 2's ranged threat (their orbs are parried), the heavy its captain";
room.waveNodes43 = [
  [[-9, 2.4, -84], [-4, 2.4, -84], [-9, 2.4, -89], [-3.5, 2.4, -85.5], [-6.5, 2.4, -86.5], [-6.5, 2.4, -90]],
  [[-20, 3.2, -92], [-1, 0, -93], [-20, 3.2, -88], [-3, 0, -97]],
  [[-19, 6, -98], [-21, 6, -97.5], [-16, 6, -97], [-12, 0, -97.5], [-3, 0, -97], [-1, 0, -93]],
];
room.waveNodes43_why = "NIGHT52 loop (the card's flow plan, re-placed): wave 1 on the island's top in view from the door (the file comes down its south ramp into the TORCH); wave 2 on the stalls' north end and the east lane's far floor (the spitter / the heavy across from the door; the drones rise there); wave 3 from the organ loft (6 m) and the apse: the island is attacked from above and the way out is past them";
room.frames36 = { mid: [-10, 0, -72, 0, 0.06], exit: [-4, 0, -95, -20, -0.05] };
room.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [-10, 0, -71, 0, 0.04], mid: [-13, 0, -79, -10, 0.08], high: [-17, 6, -98, 160, -0.15],
};
room.proofs = [
  { kind: "walk", points: [[-10, 0, -71], [-10, 0, -75]] },
  { kind: "walk", points: [[-1, 0, -79], [-1, 0, -86], [-1, 0, -95], [-10, 0, -97], [-10, 0, -99]] },
  { kind: "walk", points: [[-21.5, 0, -70], [-21.5, 3.2, -77], [-20, 3.2, -90]] },
  { kind: "walk", points: [[-15.5, 0, -82.5], [-15.5, 6, -95], [-17, 6, -98]] },
];
room.loop52_why = "NIGHT52 the room loop, cycle 4 (Tools/night/n52-loop-v6.cjs; THE ROOM CONTRACT): NIGHT46's THE ROPE CHOIR kept and finished as THE ROPE CHOIR II: 24 x 32 x 16 m; 20 pieces; three planes (floor, island 2.4 / stalls 3.2, organ loft 6 m) each on a walkers' ramp; a 24 m ride wall; two pads; three rings; the half walls (waist-high) cut for full-height rood screens; v7's rect and d6 re-cut to meet the moved edge (v7's exam untouched).";

const boxes = [
  box("deck", [-10.5, 0, -91], [-2.5, 2.4, -81], "line", "THE CANTORS' ISLAND (2.4 m, 8 x 10 m; the card's Every-solid rule): wave 1's stand in view from the door; the mid tier; the ground loop runs round it and over it (its south and north ramps)"),
  box("deck", [-23, 0, -95], [-17, 3.2, -77], "line", "THE HIGH STALLS (3.2 m, 6 x 18 m along the west wall): the ranged stand in the open, the air loop's west leg, 6.5 m across from the island (a jump-dash, 0.8 m up)"),
  box("deck", [-23, 0, -101], [-13, 6, -95], "line", "THE ORGAN LOFT (6 m, 10 x 6 m; the top tier, raised from 3.2 m): wave 3's stand over the apse; reached by its 12 m walkers' ramp, the bell hoist, the island hoist, or the east ring; its east end drops one way into the apse by the exit"),
  box("hull", [-23, 6, -101], [-13, 16, -100], "cover", "THE ORGAN (the landmark): the pipe stack on the loft's back from 6 m to the 16 m roof, the only roof-high solid on the far wall, seen from the door left of the island: the loft's back wall the drones rise against"),
  box("wall", [1, 0, -97], [1.6, 6, -73], "line", "the EAST RIDE WALL (cyan, 24 m x 6 m; the card's flow plan 3 'the ride wall up'): ride the east lane past the island to the apse, or jump off it onto the island's east edge", { ride: true, ride36: { face: "W", planeX: 1, lz: [4, 28] } }),
  box("wall", [-23, 3.2, -94.5], [-22.4, 9, -77.5], "line", "THE STALLS' RIDE WALL (cyan, 17 m x 5.8 m on the west wall above the stalls): the air loop's west leg: ride from the stalls' south end along the wall and jump off its end up onto the organ loft (6 m)", { ride: true, ride36: { face: "E", planeX: -22.4, lz: [8.5, 25.5] } }),
  box("hull", [-13.5, 0, -80.6], [-11, 3.5, -80], "cover", "the WEST ROOD SCREEN (2.5 x 3.5 m, full height, replacing the 1.0 m half wall) 11 m in, beside the island's front: the corner the file rounds into your TORCH, a LOS break between the stalls and the island's ramp; the door landing stays open to the stalls (no camping at the door: with the screens 7 m in, the walk pilot's door dwell rose 3.7 -> 5.8 s)"),
  box("hull", [-2.4, 0, -80.6], [-0.9, 3.5, -80], "cover", "the EAST ROOD SCREEN (full height) beside the island's front in the east lane: the drones' orbs break on it; 1.5 m clear of the east ride's line"),
  box("hull", [-10.5, 2.4, -82], [-9.5, 16, -81], "cover", "the WEST BELL POST (1 x 1 m on the island's front corner, island to roof): the island's cover from the stalls while you torch the file on its ramp; a LOS break, a wall-jump post"),
  box("hull", [-3.5, 2.4, -82], [-2.5, 16, -81], "cover", "the EAST BELL POST (the west's mirror): cover from the east lane's far floor; the drones' orbs break on it"),
];
const ramps = [
  ramp([-21.5, -69], [-21.5, -77], 3, 3.2, "the stalls' walkers' ramp (3.2 m over 8 m) in the south-west corner by the door: the walkers' way to the stalls (GG NIGHT20)"),
  ramp([-15.5, -83], [-15.5, -95], 3, 6, "THE ORGAN LOFT's walkers' ramp (6 m over 12 m) beside the stalls: the climb route enemies use to the top tier (no safe ledge, A12); yours when the loft's wave holds it"),
  ramp([-6.5, -75], [-6.5, -81], 3, 2.4, "the island's SOUTH RAMP (2.4 m over 6 m; the card: 'the fodder's file into the TORCH'): wave 1 comes down it at you; the walkers' way up"),
  ramp([-6.5, -97], [-6.5, -91], 3, 2.4, "the island's NORTH RAMP (2.4 m over 6 m, from the apse): the loop over the island (south ramp, top, north ramp, the east lane back); wave 3 climbs it at you"),
];
const pads = [
  tag({ at: [-0.5, 0, -89], target: [-17, 6, -98.5], apexAbove: 3, job: "line", why: "THE BELL HOIST (a jump pad in the east lane, the upward line, A5): over the island onto the organ loft (6 m): the way to wave 3's stand without the ramp", note: "the bell hoist" }),
  tag({ at: [-4, 2.4, -89.5], target: [-15.5, 6, -98.5], apexAbove: 2, job: "line", why: "THE ISLAND HOIST (a jump pad on the island's north-east corner): island -> loft, the air loop's top leg (island, loft, the stalls' drop, the stalls, the jump-dash back to the island)", note: "the island hoist" }),
];
const props = [
  { id: "v6-P1", room: Z, kind: "healthPickup", at: [-14.5, 6.9, -98], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the organ loft's east end pays health (A20: the riskiest spot, wave 3's stand, the drop into the apse beside it)" },
];
const keepLight = (id) => {
  const l = plan.lights.find((x) => x.id === id);
  if (!l) throw new Error("no light " + id);
  return JSON.parse(JSON.stringify(l));
};
const L1 = keepLight("v6-L1"), L2 = keepLight("v6-L2"), organ = keepLight("v6-organ-lamp"), L4 = keepLight("v6-L4"), crown = keepLight("v6-crown47");
organ.at = [-18, 11, -99.95];
organ.why = (organ.why || "") + " (NIGHT52: on the organ's face over the 6 m loft)";
L4.at = [-10, 0.025, -99];
crown.at = [-10, 10, -100.95];
const lights = [L1, L2, organ, L4, crown];

function swap(arr, isZ, fresh) {
  const at = arr.findIndex(isZ);
  if (at < 0) throw new Error("no v6 row to replace");
  const rest = arr.filter((x) => !isZ(x));
  rest.splice(at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === Z, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === Z, ramps);
plan.props = swap(plan.props, (b) => b.room === Z, props);
plan.lights = swap(plan.lights, (b) => b.room === Z || b.zone === Z, lights);
plan.pads = (plan.pads || []).filter((b) => b.zone !== Z).concat(pads);
if ((plan.pits || []).some((b) => b.zone === Z)) throw new Error("v6 has pits: re-read");
// the link v7 and the door d6 re-cut to meet v6's moved edge
v7.rect = [-16, -150, -4, -105];
v7.loop52_why = "NIGHT52 loop cycle 4: the link's rect gains 10 m of plain floor before its takeoff (z1 -115 -> -105) to meet v6's moved edge; the exam (the gap, the ceiling, the liners, the stair, the meter rect) untouched";
d6.rect = [-12, -105, -8, -101];

const changedRooms = [Z, "v7"];
for (let i = 0; i < plan.rooms.length; i++) if (!changedRooms.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (let i = 0; i < plan.doors.length; i++) if (plan.doors[i].id !== "d6" && JSON.stringify(plan.doors[i]) !== JSON.stringify(before.doors[i])) throw new Error("door changed: " + plan.doors[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const a = (before[k] || []).filter((b) => b.zone !== Z && b.room !== Z), b = (plan[k] || []).filter((x) => x.zone !== Z && x.room !== Z);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside v6 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "doors", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-v6: v6 THE ROPE CHOIR II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads, 3 rings); v7 and d6 re-cut");

// NIGHT52 the room loop, builder F (round 3): spire v2 5.2 THE NAVE rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2), keeping NIGHT46's
// card THE FALLEN BELL (the nave between raised side aisles, entered from a raised narthex, the great bell fallen in its middle, the clerestory
// ride, the rings; docs/ROOMS.md), recast in Design/levels/n52/spire.plan.json. n47's v2: 30 x 38 x 14 m, 1.67 pieces per 100 m2 (P0), two
// planes (the narthex and aisles at 2.4 m; the bell a 4.6 m stepped hull), a 12 m ride, four 1.0 m pews (waist-high: cut), no upward line.
// THE FALLEN BELL II: 24 x 32 x 14 m (x -22..2, z 51..83): the exit side fixed (d2, v3 untouched), the entry side moved 6 m north; entered at
// floor level through a 4 m mouth between cheeks (n46s channel and lip cut: the walk pilot kited down the channel into the door band); three
// planes: the nave floor, the narthex and aisles (2.4 m, a U), THE BELL's crown (5 m) up the fallen bell beam; the clerestory ride (17 m)
// over the west aisle; two pads onto the bell; three rings; two arcade piers; the apse posts.
// The link v1 (5.1 THE LANDING: the TORCH's pick-up and its stand-ins untouched) is re-cut to meet v2's moved edge: its rect gains 6 m of
// plain floor past its exit at its far end (z0 93 -> 87; z1, the side its scripts measure from, kept); its CROWN'S WINDOW moves with the
// wall; the door d1 moves with it (z 89..93 -> 83..87).
// Every other room, door and piece of n52-spire is as before this script (asserted). Run: node Tools/night/n52-loop-f-v2.cjs, then
// bash Tools/night/n52-loop-regen.sh spire v2
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-spire") throw new Error("unexpected plan " + plan.id);
const R = "v2";
const room = plan.rooms.find((r) => r.id === R);
if (!room || JSON.stringify(room.rect) !== "[-25,51,5,89]") throw new Error("v2 is not n47's (already run?)");
const v1 = plan.rooms.find((r) => r.id === "v1");
if (!v1 || JSON.stringify(v1.rect) !== "[-17,93,-3,115]") throw new Error("v1 is not n47's");
const d1 = plan.doors.find((d) => d.id === "d1");
if (!d1 || JSON.stringify(d1.rect) !== "[-12,89,-8,93]") throw new Error("d1 is not n47's");
const d2 = plan.doors.find((d) => d.id === "d2");
if (!d2 || JSON.stringify(d2.rect) !== "[-12,47,-8,51]") throw new Error("d2 moved: v2's exit side is fixed");

// room-local: lx = x + 10 (the doors' axis x -10), lz = metres from the entry edge z 83 toward the exit (z = 83 - lz)
const X = (lx) => lx - 10, Z = (lz) => 83 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];
const tag = (o) => Object.assign({ zone: R }, o);
const box = (kind, l0, l1, job, why, extra) => tag(Object.assign({ kind, min: [X(l0[0]), l0[1], Z(l1[2])], max: [X(l1[0]), l1[1], Z(l0[2])], job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from: [X(from[0]), Z(from[1])], to: [X(to[0]), Z(to[1])], width, rise, job: "climb", why, note: why });

const boxes = [
  box("hull", [-6, 0, 0], [-2, 14, 4], "line", "the narthex's west CHEEK (card: Molecule node D; contract item 4, the entrance gated), floor to roof: d1 opens into a 4 m mouth, the entry lane onto the nave floor, so the fight is not pulled back along the south wall to the door (round 3: with n46's channel and lip the walk pilot kited back down the channel into d1's band)"),
  box("hull", [2, 0, 0], [6, 14, 4], "line", "the narthex's east CHEEK (card: Molecule node D), floor to roof: the entry lane's other side"),
  box("deck", [-12, 0, 0], [-6, 2.4, 8], "line", "THE NARTHEX's west wing (2.4 m, card: Molecule node L): the raised porch beside the mouth, the U's south-west corner joined to the west aisle: the clerestory ride's take-off and the air loop's south leg"),
  box("deck", [6, 0, 0], [12, 2.4, 8], "line", "THE NARTHEX's east wing (2.4 m, card: Molecule node L): the porch's east corner joined to the east aisle (the air loop's south leg)"),
  box("deck", [-12, 0, 8], [-8, 2.4, 24], "line", "the WEST AISLE (2.4 m, card: Molecule node AW): the spitters' stand along the west wall under the clerestory ride, the air loop's west leg"),
  box("deck", [8, 0, 8], [12, 2.4, 24], "line", "the EAST AISLE (2.4 m, card: Molecule node AE): the spitters' stand along the east wall, the grapple side (its ring), the air loop's east leg"),
  box("deck", [-3, 0, 13], [3, 5, 19], "line", "THE BELL (deck 5 m, card: Landmark, Hero prop): the great bell fallen mouth-down in the middle of the nave, its crown the room's top tier and exposed perch (the health): the ground loop's centre; reached by its beam (walkers), the pads, the bell ring, the clerestory ride's jump-off"),
  box("wall", [-12, 0, 7], [-11.4, 9, 24], "line", "THE CLERESTORY WALL (cyan, 17 m x 9 m on the west wall over the west aisle; contract item 5): ride it from the narthex over the aisle's spitters and jump off east onto the bell (the card's arcade ride, made 17 m)", { ride: true, ride36: { face: "E", planeX: X(-11.4), lz: [7, 24] } }),
  box("hull", [-9, 0, 15], [-8, 14, 16], "cover", "the west ARCADE PIER (card: Every-solid rule), floor to roof on the aisle's edge: breaks the line of sight along the aisle and from the aisle to the bell's south lane; a corner to circle a husk round on the aisle"),
  box("hull", [8, 0, 15], [9, 14, 16], "cover", "the east ARCADE PIER (card: Every-solid rule): the same on the east aisle; the drones' and spitters' line along the aisle broken"),
  // (cut in the build: THE BELL's HANGER, a roof-high post on the crown: at the centre it hid the tier row's grid node, at the corner the
  // bell hoist's landing and arc ran into it; the crown stays bare and exposed, the card's perch)
  box("hull", [-4, 0, 30], [-2, 14, 32], "cover", "the APSE's west POST (card: State change), floor to roof beside d2: frames the lit way on (A-6), breaks the bell's line onto the exit, so wave 3's TANK comes out round it"),
  box("hull", [2, 0, 30], [4, 14, 32], "cover", "the APSE's east POST (card: State change): the apse's other side; the swarmers come round it"),
];
const ramps = [
  ramp([-10, 30], [-10, 24], 4, 2.4, "the west aisle's NORTH RAMP (card: Every-solid rule, kept; GG NIGHT20 'a route that enemies can climb up'): the walkers' way onto the aisle and the narthex"),
  ramp([10, 30], [10, 24], 4, 2.4, "the east aisle's NORTH RAMP: the walkers' way onto the east aisle; wave 3's swarmers come down it"),
  ramp([0, 29], [0, 19], 3, 5, "THE BELL BEAM (card: Hero prop): the bell's broken headstock lying from the apse up to its crown, 5 m over 10 m: the walkers' way onto the bell (no safe perch, A12) and a TORCH line as a file climbs it"),
];
const pads = [
  tag({ at: P(5.5, 0, 22), target: P(1.5, 5, 17), apexAbove: 1.5, job: "line", why: "the BELL HOIST (card: Molecule; contract item 5, the upward line): from the east lane's north end up onto the bell's crown (the grapple side's pad)", note: "the bell hoist" }),
  tag({ at: P(-10, 2.4, 11), target: P(-1.5, 5, 15), apexAbove: 1.5, job: "line", why: "the AISLE HOIST (card: Molecule): from the west aisle onto the bell: the air loop's cross-link (aisle, bell, the east aisle's ring or the drop)", note: "the aisle hoist" }),
];
const props = [
  { id: "v2-P1", room: R, kind: "healthPickup", at: P(1.5, 5.9, 14), size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the bell's crown pays health (card flow plan 11; A20: the riskiest spot, open to both aisles)" },
];
const keepLight = (id) => {
  const l = plan.lights.find((x) => x.id === id);
  if (!l) throw new Error("no light " + id);
  return JSON.parse(JSON.stringify(l));
};
const L1 = keepLight("v2-L1"), L2 = keepLight("v2-L2"), L3 = keepLight("v2-L3"), lamp = keepLight("v2-bell-lamp"), L4 = keepLight("v2-L4"), L5 = keepLight("v2-L5"), L6 = keepLight("v2-L6"), crown = keepLight("v2-crown47");
L1.at = [X(0), 13.95, Z(9)];
L2.at = [X(0), 13.95, Z(26)];
L3.at = [X(-6), 13.95, Z(16)];
lamp.at = [X(0), 0.025, Z(11.5)];
L4.at = [X(-11.95), 3.4, Z(27.5)];
L4.size = [5, 0.2, 0.1]; // the clerestory ride wall covers the west wall from lz 7 to 24: the strip only past it
L5.at = [X(11.95), 3.4, Z(17)];
L6.at = [X(0), 0.025, Z(31)];
if (JSON.stringify(crown.at) !== "[-10,9,51.05]") throw new Error("v2's crown window moved upstream");
const strip = { id: "v2-RW", room: R, kind: "wall", at: [X(-11.35), 3.6, Z(15.5)], normal: [1, 0, 0], size: [17, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1, job: "light", why: "the clerestory wall's cyan ride strip (V-3)", note: "the clerestory wall's cyan ride strip" };
const lights = [L1, L2, L3, lamp, L4, L5, L6, crown, strip];

function swap(arr, isR, fresh) {
  const at = arr.findIndex(isR);
  const rest = arr.filter((x) => !isR(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === R, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === R, ramps);
plan.props = swap(plan.props, (b) => b.room === R, props);
plan.lights = swap(plan.lights, (b) => b.room === R || b.zone === R, lights);
plan.pads = swap(plan.pads || [], (b) => b.zone === R, pads);
if ((plan.pits || []).some((b) => b.zone === R)) throw new Error("v2 has pits: re-read");

room.rect = [X(-12), Z(32), X(12), Z(0)];
room.verb36 = "the TORCH round the fallen bell, the clerestory ride onto it, the pads and rings between the aisles and the bell's crown";
// the respawn point 5.5 m in, just past the mouth: past the door's 4 m band (n46flow's walk pilot returns to its room's spawn when no
// enemy is in sight; foundry a5 / b1 measured it), and the card's entry view (the bell 7.5 m ahead through the mouth)
room.spawn = { at: P(0, 0, 5.5), yawDeg: 0 };
room.exit = P(0, 0, 31);
const F1 = P(-5.5, 0, 19.5), F2 = P(5.5, 0, 19.5), F3 = P(-5.5, 0, 24), F4 = P(5.5, 0, 25), F5 = P(-4.5, 0, 28);
const H1 = P(-10, 2.4, 21.5), H2 = P(10, 2.4, 21.5), H3 = P(-10.5, 2.4, 17.5), H4 = P(10.5, 2.4, 17.5);
const A1 = P(-9, 2.4, 4), A2 = P(9, 2.4, 4), A3 = P(-9, 2.4, 6.5), A4 = P(9, 2.4, 6.5);
room.nodes = [F1, F2, F3, F4, F5];
room.highNodes = [H1, H2, H3, H4];
room.highNodes.push(A1, A2, A3, A4);
room.waveNodes43 = [[F1, F2, F3, F4, F5], [H1, H2, H3, H4], [A1, A2, A3, A4]];
room.waveNodes43_why = "NIGHT52 loop F (NIGHT46's card flow plan 9-10, re-placed): wave 1 on the nave floor round the bell's north half (12+ m from the spawn: spawn31), they come round both lanes into the TORCH; wave 2 the aisles (the spitters in the open, the husks off the aisles); wave 3 on the narthex wings, the side you came in (the TANK and its swarmers drop off the porch behind you: the nave turns round; round 3 measured the apse: the walk pilot backed into the entry mouth, isolated dwell 1.0 -> 1.75 s)";
room.grappleNodes = [P(0, 8.5, 13.5), P(10, 5.5, 13.5), P(-5.5, 6.5, 21)];
room.grapple = "THREE rings (card: the grapple side east, the rings over the bell and the east aisle; >= 2 where the grapple is owned): the BELL RING 3.5 m over the bell's south edge (floor -> the crown), the EAST AISLE RING 3 m over the east aisle (the east lane -> the aisle's spitters), the WEST LANE RING over the west lane's north end (floor -> the west aisle or the bell's beam)";
room.frames36 = { mid: P(0, 0, 5.5).concat([0, 0.06]), exit: P(4, 0, 25).concat([20, -0.05]) };
room.frames52 = {
  _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = just inside d1 in the mouth looking down the nave at the bell; mid = the west lane at the bell's south-west corner, the fight round the bell; high = on the bell's crown looking south-east at the aisles and the narthex",
  entry: P(0, 0, 1.5).concat([0, 0.08]),
  mid: P(-5.5, 0, 11).concat([-15, 0.1]),
  high: P(-1.5, 5, 18).concat([-150, -0.2]),
};
room.proofs = [
  { kind: "walk", points: [P(0, 0, 0.5), P(0, 0, 5.5)] },
  { kind: "walk", points: [P(-5.5, 0, 9), P(-5.5, 0, 21), P(-5, 0, 28), P(0, 0, 30.5), P(5, 0, 28), P(5.5, 0, 21), P(5.5, 0, 9), P(0, 0, 9.5)] },
  { kind: "walk", points: [P(-6, 0, 30.5), P(-10, 0, 30.5), P(-10, 2.4, 23.6), P(-10, 2.4, 10), P(-9, 2.4, 4)] },
  { kind: "walk", points: [P(0, 0, 30.5), P(0, 0, 29.5), P(0, 5, 19.3), P(1.5, 5, 14.5)] },
];
delete room.spire46_why;
delete room.grapple46_why;
room.loop52_why = "NIGHT52 the room loop, builder F (Tools/night/n52-loop-f-v2.cjs; THE ROOM CONTRACT): NIGHT46's THE FALLEN BELL kept and recast as THE FALLEN BELL II: 24 x 32 x 14 m from 30 x 38 m; three planes (the floor, the narthex / aisles U at 2.4 m, the bell's crown at 5 m up its fallen beam); the clerestory ride 17 m; two pads; three rings; the pews (waist-high) cut; v1's rect and d1 re-cut to meet the moved edge (v1's TORCH lesson untouched)";

// the link v1 and the door d1 re-cut to meet v2's moved edge (v1's z1, every script's origin, kept)
v1.rect = [-17, 87, -3, 115];
v1.loop52_why = "NIGHT52 loop F (v2's re-cut): the link's rect gains 6 m of plain floor past its exit at its far end (z0 93 -> 87) to meet v2's moved edge; the TORCH's pick-up, its stand-ins, its cover blocks and z1 untouched; its CROWN'S WINDOW on the moved wall";
const vc = plan.lights.find((x) => x.id === "v1-crown47");
if (!vc || JSON.stringify(vc.at) !== "[-10,6.5,93.05]") throw new Error("v1-crown47 moved upstream");
vc.at = [-10, 6.5, 87.05];
d1.rect = [-12, 83, -8, 87];

const changedRooms = [R, "v1"];
for (let i = 0; i < plan.rooms.length; i++) if (!changedRooms.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (let i = 0; i < plan.doors.length; i++) if (plan.doors[i].id !== "d1" && JSON.stringify(plan.doors[i]) !== JSON.stringify(before.doors[i])) throw new Error("door changed: " + plan.doors[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R && b.id !== "v1-crown47";
  const a = (before[k] || []).filter(keep), b = (plan[k] || []).filter(keep);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside v2 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "doors", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a v2 piece without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v2: v2 THE FALLEN BELL II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads, 3 rings); v1 and d1 re-cut");

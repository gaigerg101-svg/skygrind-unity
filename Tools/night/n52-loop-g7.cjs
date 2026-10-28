// NIGHT52 the room loop, cycle 1 (the lead's): iceline g7 2.7 THE MIX rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2) as THE THAW LOCK II,
// in Design/levels/n52/iceline.plan.json (n47's g7 is NIGHT45's THE THAW LOCK: 30 x 32 m, 7 pieces, 0.52 per 100 m2, no ride wall, no upward line).
// The card (docs/ROOMS.md "NIGHT52 loop cards"): the lock's identity stays (the burning grate across the middle, THE BOILER on the door's axis,
// two 3 m bridges, the spitters' gantries across the fire, the lesson "{quickSwap}: KESTREL FAR, GLOVE CLOSE", the waves); the room is 6 m
// narrower (24 x 32 m: x -12..12), and every new piece answers a verb: two ride walls carry you over the fire along the side walls, a 4 m keeper's
// house is the third tier (the spitters' high stand, reached by the walkers' ramp, the coal hoist pad, or off the east ride), two coal bunkers on
// the door side (2 m) are wave 3's spawn and the ride's run-up, mooring posts and sluice posts break the gantries' lines, two coal islands in the
// middle fire are a hop line for a player without the dash.
// Every other room, door and piece of n52-iceline is n47's bit for bit (asserted). Run: node Tools/night/n52-loop-g7.cjs, then
// bash Tools/night/n52-loop-regen.sh iceline g7
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-iceline") throw new Error("unexpected plan " + plan.id);
const g7 = plan.rooms.find((r) => r.id === "g7");
if (!g7 || JSON.stringify(g7.rect) !== "[-15,-216,15,-184]") throw new Error("g7 is not n47's (already run?)");
const Z = "g7";
const tag = (o) => Object.assign({ zone: Z }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });

// the room row: rect 24 x 32 (x -12..12), the doors (x -2..2) unchanged; height 10 (the contract's floor)
g7.rect = [-12, -216, 12, -184];
g7.name = "2.7 THE MIX";
g7.verb36 = "switch: the KESTREL for the far gantry and the keeper's house, the glove for the bridge; ride the side walls over the fire or take the coal hoist up, punch a body off a bridge into the grate";
g7.nodes = [[-1.5, 0, -204], [1.5, 0, -204], [-4.5, 0, -209], [4.5, 0, -209], [-9.8, 0, -205], [9.8, 0, -205]];
g7.nodes_why = "NIGHT52 loop (R-4): the far floor beyond the grate: the tank and the pack spawn across the fire and must come over a bridge or up the ramps (the posts and the chute's mouth)";
g7.highNodes = [[-9, 2, -213], [-6, 2, -213.5], [7.5, 4, -213], [10, 4, -213.5]];
g7.highNodes_why = "NIGHT52 loop (R-4: ranged on high ground): the west coal gantry (2 m) and THE KEEPER'S HOUSE (4 m), ~27 m from the door across the fire";
g7.waveNodes43 = [null, null, [[-9, 2, -186], [9, 2, -186], [-9.5, 2, -187], [9.5, 2, -187], [-8, 2, -185.5], [8, 2, -185.5]]];
g7.waveNodes43_why = "NIGHT52 loop (the card's state change, data only): wave 3 (tank, spitter, 4 swarmers) comes out of the two COAL BUNKERS on the door side (2 m), 9 m off the door's axis: the fight turns around and the grate is crossed back (spawn33's tells show each body before it lands)";
g7.fallReturn43 = {
  belowY: -0.3, to: [-2.75, 0, -193.5], yawDeg: 0, damage: 10,
  by: [
    { rect: [-12, -202.5, -8.5, -195.5], to: [-10, 0, -193.5], yawDeg: 0 },
    { rect: [-5.5, -202.5, 0, -195.5], to: [-2.75, 0, -193.5], yawDeg: 0 },
    { rect: [0, -202.5, 5.5, -195.5], to: [2.75, 0, -193.5], yawDeg: 0 },
    { rect: [8.5, -202.5, 12, -195.5], to: [10, 0, -193.5], yawDeg: 0 },
  ],
  _why: "NIGHT45 A2's rule (A-4, DECIDE43 D-5) on NIGHT52's narrower grate: feet under -0.3 m in a pit -> back on the quay 2 m before that pit, facing the grate, 10 health",
};
g7.frames36 = { mid: [5, 0, -189, 20, -0.03], exit: [-2.5, 0, -205, 0, -0.06] };
g7.frames52 = {
  _why: "NIGHT52 the room frames reel a452 (FeelReel.N52.Rooms.cs): entry, the fight live, the highest tier",
  entry: [0, 0, -186, 0, 0.02], mid: [-3.5, 0, -192.5, 8, 0.04], high: [9, 4, -212, 158, -0.12],
};
g7.proofs = [
  { kind: "walk", points: [[0, 0, -186], [-7, 0, -193], [-7, 0, -199], [-4.5, 0, -203.5], [-1, 0, -209.5], [0, 0, -214]] },
  { kind: "walk", points: [[7, 0, -194], [7, 0, -199], [7, 0, -202.8], [7, 4, -211], [9, 4, -213]] },
];
g7.loop52_why = "NIGHT52 the room loop, cycle 1 (Tools/night/n52-loop-g7.cjs; THE ROOM CONTRACT): n47's THE THAW LOCK rebuilt as THE THAW LOCK II: 24 x 32 x 10 m; 21 pieces; three tiers (floor, 2 m gantry and bunkers, 4 m keeper's house), every perch on a walkers' ramp; two 18 m ride walls over the fire; the coal hoist and the return hoist (pads); a circuit round the boiler over both bridges; the waves, lesson and lights' roles kept.";

const boxes = [
  box("hull", [-1.5, -0.8, -200.5], [1.5, 10, -197.5], "cover", "THE BOILER (the landmark, A-6): the stack in the middle of the grate on the door's axis, fire to roof; the pillar you circle (the room's loop: quay, a bridge, the far floor, the other bridge) and the block that cuts the far stands' line to the door landing"),
  box("deck", [-12, 0, -216], [-4, 2, -210], "line", "the WEST COAL GANTRY (2 m, 8 x 6 m): the spitters' stand across the fire: the KESTREL's range from the quay, the glove's only up its ramp or off the west ride's end (R-4)"),
  box("deck", [4, 0, -216], [12, 4, -210], "line", "THE KEEPER'S HOUSE (4 m, 8 x 6 m; the third tier): the lock-keeper's high floor over the chute, the second spitter stand; you reach it by its ramp off the east bridge, the coal hoist pad from the quay, or a jump off the east ride's end; the walkers follow you up the ramp (no safe ledge, A12)"),
  box("wall", [-12, -0.8, -205.5], [-11, 5, -187.5], "line", "the WEST SLUICE WALL (ride, cyan, 18 m x 5 m standing in the west fire): ride it from the bunker over the grate to the gantry's foot: the crossing for a player who rides, not dashes (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "E", planeX: -11, lz: [3.5, 21.5] } }),
  box("wall", [11, -0.8, -205.5], [12, 5, -187.5], "line", "the EAST SLUICE WALL (ride, cyan, the west's mirror): ride it over the east fire and jump off its end onto the keeper's house (4 m), or wall-jump back", { ride: true, ride36: { face: "W", planeX: 11, lz: [3.5, 21.5] } }),
  box("hull", [-6.6, 0, -191.6], [-5.4, 3.5, -190.4], "cover", "the WEST MOORING POST (1.2 m x 3.5 m): breaks the gantry spitters' line to the quay's west half; the pivot you circle while the pack comes off the west bridge into your glove (no waist-high crate: a full-height post)"),
  box("hull", [5.4, 0, -191.6], [6.6, 3.5, -190.4], "cover", "the EAST MOORING POST (the west's mirror): breaks the keeper's house's line to the quay's east half; the pack off the east bridge rounds it into the glove"),
  box("deck", [-11, 0, -188], [-7, 2, -184], "line", "the WEST COAL BUNKER (2 m, 4 x 4 m): wave 3 comes out on it (the fight turns around); the run-up for the west ride (the sluice wall's face starts at the bunker's edge)"),
  box("deck", [7, 0, -188], [11, 2, -184], "line", "the EAST COAL BUNKER (2 m, the west's mirror): wave 3's other spawn; the run-up onto the east ride; the return hoist lands here"),
  box("hull", [-3.6, 0, -207.1], [-2.4, 4, -205.9], "cover", "the WEST SLUICE POST (1.2 m x 4 m) on the far floor: breaks the gantry's line down onto the far floor, the corner the pack rounds to reach the west bridge"),
  box("hull", [2.4, 0, -207.1], [3.6, 4, -205.9], "cover", "the EAST SLUICE POST (the west's mirror): the house's line down onto the far floor; the return hoist sits between the two"),
  box("deck", [-4.5, -0.8, -200], [-3, 0.6, -198], "line", "the WEST COAL ISLAND (1.5 x 2 m, 0.6 m over the floor, in the middle fire): a hop line over the grate for a player without the dash (jump, island, jump); the bodies you punch off it burn"),
  box("deck", [3, -0.8, -200], [4.5, 0.6, -198], "line", "the EAST COAL ISLAND (the west's mirror): the other hop across the middle fire"),
  box("hull", [-12, 2, -216], [-10, 4.5, -213.5], "cover", "the WEST GANTRY's COAL HOPPER (2 x 2.5 m on the gantry's back corner): the spitter's shield from the quay: you flank it off the west ride or up the ramp (a stand you must move to break)"),
  box("hull", [4, 4, -216], [6, 6, -214], "cover", "the KEEPER'S HUT (2 x 2 m on the house's back corner): the house spitter's shield from the quay and the bunkers: the hoist or the east ride puts you round it"),
];
const ramps = [
  ramp([-9.5, -192], [-9.5, -187.5], 3, 2, "the west bunker's walkers' ramp (2 m over 4.5 m): wave 3's way down to you, yours up to the ride's run-up"),
  ramp([9.5, -192], [9.5, -187.5], 3, 2, "the east bunker's walkers' ramp (the west's mirror)"),
  ramp([-7, -202.5], [-7, -210], 3, 2, "the west gantry's walkers' ramp (2 m over 7.5 m) straight off the west bridge: the spitters' way down and yours up; the climb the pack takes when you hold the gantry"),
  ramp([7, -202], [7, -210], 3, 4, "THE KEEPER'S HOUSE's walkers' ramp (4 m over 8 m, its foot on the east bridge) straight off the bridge: the walkers follow you onto the third tier (no safe ledge, A12); the climb route enemies use (GG NIGHT20)"),
];
const pads = [
  tag({ at: [3.5, 0, -193.5], target: [8, 4, -213], apexAbove: 3, job: "line", why: "THE COAL HOIST (a jump pad, the upward line, A5): from the quay over the middle fire and the east bridge onto the keeper's house (4 m): the fast way to the spitters' high stand, the KESTREL on the gantry from above", note: "the coal hoist" }),
  tag({ at: [0, 0, -208], target: [9, 2, -186], apexAbove: 4, job: "line", why: "THE RETURN HOIST (a jump pad between the sluice posts): from the far floor back over the fire onto the east coal bunker (2 m): when wave 3 comes out of the bunkers, the pad takes you to them (the fight turns around)", note: "the return hoist" }),
];
const pits = [
  tag({ rect: [-12, -202.5, -8.5, -195.5], floorY: -0.8, lava: true, job: "hazard", why: "THE GRATE, west pit (3.5 x 7 m, lava 0.8 m down, under the west ride): both ways (A-5): you lose health and are knocked up; a body punched off the west bridge burns", note: "the grate, west pit" }),
  tag({ rect: [-5.5, -202.5, 5.5, -195.5], floorY: -0.8, lava: true, job: "hazard", why: "THE GRATE, middle pit (11 x 7 m, the boiler and the two coal islands in it): crossed by the dash-jump or the island hops; the widest fire to punch a body into", note: "the grate, middle pit" }),
  tag({ rect: [8.5, -202.5, 12, -195.5], floorY: -0.8, lava: true, job: "hazard", why: "THE GRATE, east pit (3.5 x 7 m, under the east ride): the pack crossing the east bridge has fire on both sides", note: "the grate, east pit" }),
];
const props = [
  { id: "g7-P1", room: Z, kind: "healthPickup", at: [0, 0.9, -204.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health right behind the boiler on the far floor, under both stands' fire (A20: the best at the riskiest spot): the dash-jump or an island hop lands on it" },
  { id: "g7-P2", room: Z, kind: "hero", at: [0, 9.2, -199], size: [3.6, 1.2, 3.6], yawDeg: 0, collider: false, decor: true, job: "landmark", why: "the BOILER's collar under the roof (decor, no collider: out of reach): the stack reads as a stack from the door" },
];
const lights = [
  { id: "g7-L1", room: Z, kind: "ceiling", at: [0, 9.95, -191.75], normal: [0, -1, 0], size: [4, 4, 0.1], color: "white", range: 10, intensity: 0.7, why: "the quay's one dim panel: the run-up reads, the fire ahead is the brighter light" },
  { id: "g7-L2", room: Z, kind: "pool", at: [0, -0.775, -196.5], normal: [0, 1, 0], size: [5, 1.6, 0.05], color: "orange", range: 10, intensity: 1.4, danger: true, why: "the grate's fire from below (danger: the room's one orange accent)" },
  { id: "g7-L3", room: Z, kind: "wall", at: [0, 4, -197.45], normal: [0, 0, 1], size: [1.6, 0.6, 0.1], color: "white", range: 8, intensity: 1, why: "the BOILER's gauge lamp (on its door-side face at 4 m): the landmark lit" },
  { id: "g7-L4", room: Z, kind: "ceiling", at: [-8, 9.95, -213], normal: [0, -1, 0], size: [5, 4, 0.1], color: "white", range: 10, intensity: 1, why: "the west gantry's coal lamp: the spitters' high ground reads" },
  { id: "g7-L5", room: Z, kind: "ceiling", at: [8, 9.95, -213], normal: [0, -1, 0], size: [5, 4, 0.1], color: "white", range: 10, intensity: 1, why: "the keeper's house lamp: the third tier reads from the quay" },
  { id: "g7-L6", room: Z, kind: "pool", at: [0, 0.025, -214], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6, why: "the exit's cyan pool (A-6: the exit is lit)" },
];

// replace g7's rows, keep every other row in its place (g7's rows go where n47's first g7 row was)
function swap(arr, isG7, fresh) {
  const at = arr.findIndex(isG7);
  if (at < 0) throw new Error("no g7 row to replace");
  const rest = arr.filter((x) => !isG7(x));
  rest.splice(at, 0, ...fresh);
  return rest;
}
plan.boxes = swap(plan.boxes, (b) => b.zone === Z, boxes);
plan.ramps = swap(plan.ramps, (b) => b.zone === Z, ramps);
plan.pits = swap(plan.pits, (b) => b.zone === Z, pits);
plan.props = swap(plan.props, (b) => b.room === Z, props);
plan.lights = swap(plan.lights, (b) => b.room === Z || b.zone === Z, lights);
plan.pads = (plan.pads || []).filter((b) => b.zone !== Z).concat(pads);

// the guard: every other room and every other row bit for bit
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keepA = (before[k] || []).filter((b) => b.zone !== Z && b.room !== Z), keepB = (plan[k] || []).filter((b) => b.zone !== Z && b.room !== Z);
  if (JSON.stringify(keepA) !== JSON.stringify(keepB)) throw new Error(k + ": a row outside g7 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pits", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-g7: g7 THE THAW LOCK II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads, " + pits.length + " lava pits)");

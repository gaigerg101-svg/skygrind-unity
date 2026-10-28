// NIGHT52 the room loop, builder F (round 3; spire is F's from 00:20): v5 5.5 THE RINGERS' STAIR rebuilt to THE ROOM CONTRACT (NIGHT52.md
// section 2) from its identity card (docs/ROOMS.md, "Level 5 as n52-spire: v5"), in Design/levels/n52/spire.plan.json. n47's v5: 12 x 35 x 9 m,
// a straight corridor with two 1.0 m half walls (waist-high: cut) and two noride liners, one wave, 0.95 pieces per 100 m2 (P0), one plane.
// THE RINGERS' STAIR II: 24 x 26 x 12 m (x -22..2, z -65..-39): the exit side fixed (d5 and v6 are the lead's v6, untouched), the entry side
// moved 9 m north; three planes: the floor round the NEWEL, the ROPE GALLERY (2.5 m, east, two walkers' ramps) and the RINGING CHAMBER
// (5 m, north-west) up THE RINGERS' STAIR (a 10 m walkers' ramp under the west ride wall); three bell-rope rings; two hoist pads; the cheeks.
// The link v4 (5.4 THE STEP UP, a jump-dash exam: its gap, ceiling, liners, stair, landing and meter untouched) is re-cut to meet v5's moved
// edge: its rect gains 9 m of plain floor past its landing at its far end (z0 -26 -> -35; z1, the side every v4 script measures from, kept);
// its two CROWN'S WINDOW lights move with the exit wall; the door d4 moves with them (z -30..-26 -> -39..-35).
// Every other room, door and piece of n52-spire is as before this script (asserted). Run: node Tools/night/n52-loop-f-v5.cjs, then
// bash Tools/night/n52-loop-regen.sh spire v5
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-spire") throw new Error("unexpected plan " + plan.id);
const R = "v5";
const room = plan.rooms.find((r) => r.id === R);
if (!room || JSON.stringify(room.rect) !== "[-16,-65,-4,-30]") throw new Error("v5 is not n47's (already run?)");
const v4 = plan.rooms.find((r) => r.id === "v4");
if (!v4 || JSON.stringify(v4.rect) !== "[-16,-26,-4,9]") throw new Error("v4 is not n47's");
const d4 = plan.doors.find((d) => d.id === "d4");
if (!d4 || JSON.stringify(d4.rect) !== "[-12,-30,-8,-26]") throw new Error("d4 is not n47's");
const d5 = plan.doors.find((d) => d.id === "d5");
if (!d5 || JSON.stringify(d5.rect) !== "[-12,-69,-8,-65]") throw new Error("d5 moved: v5's exit side is fixed");

// room-local: lx = x + 10 (the doors' axis x -10), lz = metres from the entry edge z -39 toward the exit (z = -39 - lz)
const X = (lx) => lx - 10, Z = (lz) => -39 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];
const tag = (o) => Object.assign({ zone: R }, o);
const box = (kind, l0, l1, job, why, extra) => tag(Object.assign({ kind, min: [X(l0[0]), l0[1], Z(l1[2])], max: [X(l1[0]), l1[1], Z(l0[2])], job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from: [X(from[0]), Z(from[1])], to: [X(to[0]), Z(to[1])], width, rise, job: "climb", why, note: why });

const boxes = [
  box("hull", [-6, 0, 0], [-2, 12, 4], "line", "the stair hall's west CHEEK (card: Molecule node E; contract item 4, the entrance gated), floor to roof: d4 opens into a 4 m mouth, so the fight is not pulled back along the south wall to the door (foundry a5 / b1: the walk pilot kited that wall through the door's band)"),
  box("hull", [2, 0, 0], [6, 12, 4], "line", "the stair hall's east CHEEK (card: Molecule node E): the mouth's other side; the corner beyond it is the rope gallery's south ramp"),
  box("wall", [-12, 0, 1], [-11.4, 7, 18], "line", "THE STAIR WALL (cyan, 17 m x 7 m on the west wall; contract item 5): ride it up beside THE RINGERS' STAIR and over the file coming down it, and step off its end onto the ringing chamber (5 m) or onto the stair", { ride: true, ride36: { face: "E", planeX: X(-11.4), lz: [1, 18] } }),
  box("deck", [-12, 0, 19], [-5, 5, 26], "line", "THE RINGING CHAMBER (deck 5 m, card: Molecule node C, the 30 s problem): the ringers' floor where the ropes came down, the spitter's perch over the hall; reached by the stair, the hoist pads, the north ring or the stair wall's ride"),
  box("hull", [-12, 5, 24], [-10, 12, 26], "cover", "the chamber's BELL FRAME (card: Place fantasy), chamber to roof in its north-west corner: a corner to circle on the chamber (a pad landing is not a dead stand) and cover from the gallery"),
  box("hull", [-2, 0, 9], [2, 12, 14], "cover", "THE NEWEL (card: Spatial law; contract item 4, a loop round a central mass), floor to roof: the stair hall's centre post the stair once wound round; the loop runs round it (the west lane 6.9 m, the east lane 4 m), it breaks the chamber's line of sight onto the entry mouth and the bell ropes hang beside it"),
  box("deck", [6, 0, 8], [12, 2.5, 18], "line", "THE ROPE GALLERY (deck 2.5 m, card: Molecule node G): the ringers' east gallery along the ropes' line, the middle tier: the husks' high lane, a double jump from the east lane, two walkers' ramps (no safe ledge); its pad throws you over the hall to the chamber"),
  box("hull", [10.5, 2.5, 12], [12, 12, 13.5], "cover", "the gallery's PIER (card: Every-solid rule), gallery to roof against the east wall: breaks the line along the gallery (a corner to circle up there), so the gallery is not one long firing step"),
];
const ramps = [
  ramp([-10.15, 9], [-10.15, 19], 2.5, 5, "THE RINGERS' STAIR (card: Title, Molecule; GG NIGHT20 'a route that enemies can climb up'): 5 m over 10 m under the stair wall, the walkers' way to the chamber: the file comes down it at you (torch it at its foot) and a rider over it steps off onto it"),
  ramp([10.75, 2], [10.75, 8], 2.5, 2.5, "the gallery's SOUTH RAMP (2.5 m over 6 m) beside the mouth: the walkers' way up from the entry side"),
  ramp([10.75, 24], [10.75, 18], 2.5, 2.5, "the gallery's NORTH RAMP (2.5 m over 6 m, from the north floor): the second walkers' way up, so the gallery's husks come at you from both ends"),
];
const pads = [
  tag({ at: P(-3.5, 0, 17), target: P(-8.5, 5, 22.5), apexAbove: 2.5, job: "line", why: "THE CHAMBER HOIST (card: Molecule; contract item 5, the upward line): from the north floor beside the newel up onto the ringing chamber behind its spitter (apex 2.5 m: over the chamber's corner at 5.9 m)", note: "the chamber hoist" }),
  tag({ at: P(9, 2.5, 16), target: P(-8.5, 5, 23.5), apexAbove: 1.5, job: "line", why: "THE GALLERY HOIST (card: Molecule): from the rope gallery's north end 18 m over the hall onto the chamber: the high loop (gallery, chamber, the stair down)", note: "the gallery hoist" }),
];
const props = [
  { id: "v5-P1", room: R, kind: "healthPickup", at: P(-7, 5.9, 21.5), size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the riskiest spot pays (A20): the ringing chamber's front edge, the spitter's perch at the stair's head" },
];
const keepLight = (id) => {
  const l = plan.lights.find((x) => x.id === id);
  if (!l) throw new Error("no light " + id);
  return JSON.parse(JSON.stringify(l));
};
const L1 = keepLight("v5-L1"), L2 = keepLight("v5-L2"), L3 = keepLight("v5-L3"), crown = keepLight("v5-crown47");
L1.at = [X(0), 11.95, Z(6)];
L2.at = [X(0), 11.95, Z(20)];
L3.at = [X(0), 0.025, Z(24.5)];
if (JSON.stringify(crown.at) !== "[-10,6.5,-64.95]") throw new Error("v5's crown window moved upstream");
const strip = { id: "v5-RW", room: R, kind: "wall", at: [X(-11.35), 1.2, Z(9.5)], normal: [1, 0, 0], size: [17, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1, job: "light", why: "the stair wall's cyan ride strip (V-3)", note: "the stair wall's cyan ride strip" };
const lights = [L1, L2, L3, crown, strip];

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
if ((plan.pits || []).some((b) => b.zone === R)) throw new Error("v5 has pits: re-read");

// the room row
room.rect = [X(-12), Z(26), X(12), Z(0)];
room.height = 12;
room.verb36 = "the grapple's pull UP the bell ropes: from the hall floor ring to ring onto the ringing chamber over the stair's file; the TORCH at the newel and the stair's foot";
room.spawn = { at: P(0, 0, 5.5), yawDeg: 0 };
room.exit = P(0, 0, 24.5);
room.waves = [["husk", "husk", "husk", "spitter"]];
room.waves52_why = "NIGHT52 loop F (the card's 30 s problem): one wave still (the breather between the nave and the choir), a spitter on the chamber so the hall has its height problem; the report's budget allows 2 kinds a wave (no heavy here): n47's two swarmers -> a husk (5 points: husks 3, the spitter 2)";
const N1 = P(-1, 0, 20), N2 = P(3, 0, 22), N3 = P(-7.5, 5, 21), N4 = P(8, 2.5, 16), N5 = P(-9, 5, 23.5);
room.nodes = [N1, N2];
room.highNodes = [N3, N4, N5];
room.waveNodes43 = [[N3, N5, N1, N2, N4]];
room.waveNodes43_why = "NIGHT52 loop F: the chamber (2), the north floor (2), the gallery's north end (1): the file comes down the stair and round the newel, the spitter (where it lands) holds a height";
room.grappleNodes = [P(-4, 7.5, 6), P(-3, 9, 20.5), P(4, 7.5, 19.5)];
room.grapple = "THREE bell-rope rings (the card's Verb it celebrates, the ropes' iron grips; >= 2 where the grapple is owned): the SOUTH RING over the west lane by the mouth (the first pull, off the floor toward the stair), the NORTH RING over the north floor beside the chamber's east edge (the pull UP onto the chamber), the EAST RING over the gallery's north end (floor -> gallery, the gallery's way back to the chamber's line)";
room.frames36 = { mid: P(-4, 0, 5.5).concat([10, 0.08]), exit: P(0, 0, 22).concat([0, -0.05]) };
room.frames52 = {
  _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = just inside d4 in the mouth looking at the newel, the stair and chamber up left, the gallery right; mid = the west lane at the stair's foot looking up the stair at the chamber; high = on the ringing chamber looking south-east over the hall",
  entry: P(0, 0, 1.5).concat([0, 0.1]),
  mid: P(-6, 0, 6).concat([15, 0.2]),
  high: P(-9, 5, 23).concat([-135, -0.25]),
};
room.proofs = [
  { kind: "walk", points: [P(0, 0, 1), P(0, 0, 4.6), P(-5, 0, 5.5), P(-5, 0, 13), P(-3, 0, 16), P(4, 0, 15.5), P(4, 0, 8), P(0, 0, 5.2)] },
  { kind: "walk", points: [P(-5, 0, 7), P(-10.15, 0, 8.6), P(-10.15, 2.5, 14), P(-10.15, 4.9, 19.3), P(-8, 5, 22)] },
  { kind: "walk", points: [P(8, 0, 1.5), P(10.75, 0, 1.6), P(10.75, 2.5, 8.4), P(8, 2.5, 11)] },
  { kind: "walk", points: [P(7, 0, 24.5), P(10.75, 0, 24.4), P(10.75, 2.5, 17.6), P(8, 2.5, 15)] },
  { kind: "walk", points: [P(4, 0, 16), P(0, 0, 20), P(0, 0, 24.5)] },
];
delete room.spire46_why;
delete room.waves46_why;
room.loop52_why = "NIGHT52 the room loop, builder F (Tools/night/n52-loop-f-v5.cjs; THE ROOM CONTRACT): THE RINGERS' STAIR II, 24 x 26 x 12 m from the corridor (12 x 35 x 9 m, 0.95): the newel's loop, the rope gallery (2.5 m) and the ringing chamber (5 m) each on walkers' ramps, the stair wall's 17 m ride, three bell-rope rings, two hoist pads, the cheeks; the half walls and liners cut; v4's rect and d4 re-cut to meet the moved edge (v4's exam untouched).";

// the link v4 and the door d4 re-cut to meet v5's moved edge (v4's z1, every script's origin, kept)
v4.rect = [-16, -35, -4, 9];
v4.loop52_why = "NIGHT52 loop F (v5's re-cut): the link's rect gains 9 m of plain floor past its landing at its far end (z0 -26 -> -35) to meet v5's moved edge; the exam (the gap, the ceiling, the liners, the recovery stair, the landing, the meter rect) and z1 untouched; its two CROWN'S WINDOW lights moved with its exit wall";
for (const id of ["v4-crown47-w", "v4-crown47-e"]) {
  const l = plan.lights.find((x) => x.id === id);
  if (!l || l.at[2] !== -25.95) throw new Error(id + " moved upstream");
  l.at = [l.at[0], l.at[1], -34.95];
}
d4.rect = [-12, -39, -8, -35];

// the guard: every other room, door and piece as before
const changedRooms = [R, "v4"];
for (let i = 0; i < plan.rooms.length; i++) if (!changedRooms.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (let i = 0; i < plan.doors.length; i++) if (plan.doors[i].id !== "d4" && JSON.stringify(plan.doors[i]) !== JSON.stringify(before.doors[i])) throw new Error("door changed: " + plan.doors[i].id);
const v4lights = ["v4-crown47-w", "v4-crown47-e"];
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R && !v4lights.includes(b.id);
  const a = (before[k] || []).filter(keep), b = (plan[k] || []).filter(keep);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside v5 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "doors", "boxes", "ramps", "props", "lights", "pads"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a v5 piece without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v5: v5 THE RINGERS' STAIR II written (" + boxes.length + " boxes, " + ramps.length + " ramps, " + pads.length + " pads, 3 rings); v4 and d4 re-cut");

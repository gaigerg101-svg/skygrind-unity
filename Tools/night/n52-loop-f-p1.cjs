// NIGHT52 the room loop, builder F (round 7; the lead's queue: span p1 4.1 HOOK YARD, 32 x 36 m, 1.39 per 100 m2): p1 rebuilt in THE ROOM
// CONTRACT's spirit as THE HOOK YARD II, in Design/levels/n52/span.plan.json. p1 is a teach room (D-12; the grapple's first pull, "{grapple}
// PULL TO THE RING.", husksDead 3): its lesson and its waves are kept (husk 3 / spitter 2 + swarmer 4); every other item is printed.
// The card (docs/ROOMS.md "p1 4.1 HOOK YARD -> THE HOOK YARD II"): the room keeps its identity (the GREAT HOOK over the loading terrace's
// edge, the cable drum in the middle, the hoist well with its ride panel, the two hoist houses at the start); it shrinks to 24 x 32 m (the
// north wall and d1 fixed; 4 m off the south, 4 m off each side) and every new piece answers a verb.
// Room-local: lx = x, lz = metres north of the new south wall z 123.5 (travel toward -z): z = 123.5 - lz.
// Every other room, door and piece of n52-span bit for bit (asserted). Run: node Tools/night/n52-loop-f-p1.cjs, then bash Tools/night/n52-loop-regen.sh span p1
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-span") throw new Error("unexpected plan " + plan.id);
const R = "p1";
const room = plan.rooms.find((r) => r.id === R);
if (!room || JSON.stringify(room.rect) !== "[-16,91.5,16,127.5]" || room.loop52_why) throw new Error("p1 is not n47's (already run?)");
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const Z = (lz) => 123.5 - lz;
const P = (lx, y, lz) => [lx, y, Z(lz)];
const tag = (o) => Object.assign({ zone: R }, o);
const box = (kind, min, max, job, why, extra) => tag(Object.assign({ kind, min, max, job, why, note: why }, extra || {}));
const ramp = (from, to, width, rise, why) => tag({ from, to, width, rise, job: "climb", why, note: why });
const pad = (at, target, apexAbove, why) => tag({ at, target, apexAbove, job: "pad", why, note: why });
// a box from room-local corners (lx0, y0, lz0) .. (lx1, y1, lz1), lz0 < lz1 (z flips)
const B = (kind, lx0, y0, lz0, lx1, y1, lz1, job, why, extra) => box(kind, [lx0, y0, Z(lz1)], [lx1, y1, Z(lz0)], job, why, extra);

room.rect = [-12, Z(32), 12, Z(0)];
const boxes = [
  B("hull", -12, 0, 0, -5, 12, 3, "line", "the WEST HOIST HOUSE (card: Every-solid rule), roof-high at the start: with its pair the start is a 10 m passage (the level's first room: no door behind), so the first look is the drum and the hook beyond, and the hoist houses' corners are the lanes' first cover"),
  B("hull", 5, 0, 0, 12, 12, 3, "line", "the EAST HOIST HOUSE (card: Every-solid rule), roof-high: the start passage's east side; its corner is the well pad's floor and the east lane's way north"),
  B("wall", -12, 0, 3, -11.4, 12, 16, "line", "the CHAIN WALL (card: Verb it celebrates, the air loop's first link; contract item 5's second ride), floor to roof on the west wall from the start passage to the terrace's south face (13 m): ride it north and jump off onto the terrace or pull to the hook from it (the grapple's pull from a ride); roof-high, so its top is never a perch", { ride: true, ride36: { face: "E", planeX: -11.4, lz: [3, 16] } }),
  B("deck", -12, 0, 16, -6, 4, 32, "line", "THE LOADING TERRACE (deck 4 m, card: Molecule's T, the high tier): 16 m along the west wall to the north wall, the spitters' stand over the north apron; THE GREAT HOOK hangs over its front edge (the lesson: a pull from the floor lands you on it); walkers reach it up its ramp and its back ramp"),
  B("hull", -12, 4, 24.5, -10.5, 12, 26, "cover", "the terrace's CRANE POST (card: Place fantasy, the loading crane), terrace to roof against the west wall: a corner on the terrace to circle (a spitter there must step out to lob; a hook landing has cover)"),
  B("hull", -2.5, 0, 11, 2.5, 5.5, 19, "cover", "THE CABLE DRUM (card: Landmark, the circuit's centre), standing on its flange, 5 x 8 m, 5.5 m (out of a double jump; the drum ring 3.5 m over it is the way up): the loop round it is the room's floor fight (A-2), it hides the north apron from the start, so wave 1 comes round its flanks"),
  B("hull", 6.7, 0, 12, 7, 1, 24, "line", "the HOIST WELL's west RAILING (card: Hazard both ways; flow plan 6), 1 m on the well's west lip: the east lane's edge over the void (a punch launches a husk over it; you vault it to take the ride)"),
  B("hull", 7, 0, 11.7, 11.4, 1, 12, "line", "the HOIST WELL's south RAILING, 1 m on the well's south lip: the ride's run-up line along it to the panel"),
  B("wall", 11.4, -14, 10, 12, 12, 28, "line", "THE WELL PANEL (card: Verb it celebrates; NIGHT52 contract item 5, a ride wall >= 17 m): the east wall over the hoist well, 18 m from its south lip over the void to the engine deck, floor to roof (cyan): ride it over the well (a fall is the well's fall return), land on the engine deck or pull to the hoist block from it", { ride: true, ride36: { face: "W", planeX: 11.4, lz: [10, 28] } }),
  B("deck", 7, 0, 24, 12, 2, 32, "line", "THE ENGINE DECK (deck 2 m, card: Molecule's E, the middle tier): the well's north lip and the ride's landing in the north-east corner; a spitter on it covers the east lane, walkers reach it up the engine ramp"),
  B("hull", 10.5, 2, 29.5, 12, 12, 32, "cover", "the HOIST ENGINE (card: Place fantasy), deck to roof in the engine deck's corner: a corner on the deck to circle (a ride landing has cover from the terrace)"),
];
const ramps = [
  ramp([-7.5, Z(6)], [-7.5, Z(16)], 3, 4, "the terrace's RAMP (card: Every-solid rule; GG NIGHT20 'a route that enemies can climb up'): 4 m over 10 m from the start floor beside the chain wall onto the terrace's south end: the walkers' way up and your walk"),
  ramp([-4.75, Z(22)], [-4.75, Z(32)], 2.5, 4, "the terrace's BACK RAMP (card: Molecule): 4 m over 10 m up from the north apron beside the terrace's east edge onto its north end: the second walkers' way on (no single choke), the swarmers' run down at the exit apron"),
  ramp([2, Z(30.5)], [7, Z(30.5)], 3, 2, "the ENGINE RAMP (card: Molecule): 2 m over 5 m along the north wall from the exit apron onto the engine deck: the walkers' way up and the spitter's way down"),
];
const pads = [
  pad([-1, 0, Z(24)], [-9, 4, Z(28)], 2, "the HOOK PAD (card: Molecule; NIGHT52 contract item 5, the upward line): on the north apron behind the drum, it throws you onto the terrace's north end behind its spitters (the second way up after the hook's pull)"),
  pad([9.5, 0, 119], [9.5, 2, Z(26)], 2, "the WELL PAD (card: Molecule): against the east hoist house's north face (round 9, D-30: off the east lane's walk line, where every lap round the drum stepped on it), it throws you over the void onto the engine deck (the walkers go round; the ride and this pad go over)"),
];
const pits = [
  tag({ rect: [7, Z(24), 12, Z(12)], floorY: -14, job: "hazard", why: "the HOIST WELL (card: Hazard both ways, flow plan 6; 5 x 12 m, was 8 x 14): the void under the ride panel: to you a fall return (the south lip, 5 health); to a husk the end of it (punch it over the railing)", note: "" }),
];
pits[0].note = pits[0].why;
const lights = [
  { room: R, normal: [0, -1, 0], id: "p1-yard", kind: "ceiling", at: [0, 11.95, Z(14)], size: [4, 4, 0.1], color: "white", range: 10, intensity: 1 },
  { room: R, normal: [0, -1, 0], id: "p1-hook", kind: "ceiling", at: [-6, 11.95, Z(21)], size: [3, 3, 0.1], color: "amber", range: 12, intensity: 1.5 },
  { room: R, normal: [0, 1, 0], id: "p1-land", kind: "pool", at: [-9, 4.025, Z(21)], size: [2.5, 2.5, 0.05], color: "cyan", range: 6, intensity: 1.2 },
  { room: R, normal: [0, 1, 0], id: "p1-pool", kind: "pool", at: [0, 0.025, Z(30)], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 },
  { room: R, normal: [1, 0, 0], id: "p1-RW", kind: "wall", at: [-11.35, 1.2, Z(9.5)], size: [13, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
  { room: R, normal: [-1, 0, 0], id: "p1-RE", kind: "wall", at: [11.35, 1.2, Z(19)], size: [18, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
];
const props = [
  { id: "p1-P1", room: R, kind: "healthPickup", at: [2, 0.9, Z(21)], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
    why: "the riskiest spot pays (A20; card: The 30 s problem): on the north apron in front of the drum's north face, under the terrace's and the engine deck's spitters both" },
];
const swap = (arr, isMine, rows) => { const at = arr.findIndex(isMine); const kept = arr.filter((o) => !isMine(o)); kept.splice(at < 0 ? kept.length : at, 0, ...rows); arr.length = 0; arr.push(...kept); };
const mineZ = (o) => o.zone === R, mineR = (o) => o.room === R;
swap(plan.boxes, mineZ, boxes);
swap(plan.ramps, mineZ, ramps);
plan.pads = plan.pads || [];
swap(plan.pads, mineZ, pads);
swap(plan.pits, mineZ, pits);
swap(plan.lights, mineR, lights);
plan.props = plan.props || [];
swap(plan.props, mineR, props);

// the room row: the lesson, the waves, the doors kept; the rings (four: two pairs at one lz each, a ring for the span pilot's rule), the nodes, the proofs rebuilt
room.verb36 = "the first pull: the GREAT HOOK's eye from the floor onto the terrace; the air loop: the chain wall -> the hook, the drum ring -> the hoist block -> the well panel -> the engine deck";
room.spawn = { at: [0, 0, Z(2)], yawDeg: 0 };
room.exit = [0, 0, Z(30)];
room.grappleNodes = [[-6, 9, Z(21)], [0, 9, Z(18)], [9.5, 8, Z(18)], [9, 7, Z(28.5)]];
room.grapple = "FOUR rings (card: Landmark, Molecule): the GREAT HOOK's eye over the terrace's front edge (9 m; the lesson's ring: a pull from the floor lands you on the terrace), the DRUM RING 3.5 m over the cable drum (floor -> the drum's top), the HOIST BLOCK over the well (8 m: the drum -> the hoist -> the well panel's ride) and the ENGINE RING over the engine deck (the well's north end -> the deck): the air loop";
const W1 = [P(-7, 4, 22), P(8, 2, 26), P(5, 0, 14)];
const H = [P(-9, 4, 29), P(-9, 4, 23), P(9, 2, 27)];
const G = [P(-1, 0, 28.5), P(3.5, 0, 26.5), P(-2.5, 0, 22.5)];
room.nodes = [W1[2], ...G];
room.highNodes = [W1[0], W1[1], ...H];
room.waveNodes43 = [W1, [...H, ...G]];
room.waveNodes43_why = "NIGHT52 F loop (THE HOOK YARD II): wave 1's three husks in view from the start (3/3): one on the terrace's front edge and one on the engine deck (21-25 m: they come down the ramps at you, the pull to the hook meets the terrace's), one in the east lane past the drum (13 m), the lesson's three kills; wave 2 the spitters on the terrace's north end and the engine deck, the swarmers from the north apron (ahead of a player who cleared wave 1)";
room.fallReturn43 = { belowY: -1.5, to: [9.5, 0, Z(10)], yawDeg: 0, damage: 5, by: [{ rect: [7, Z(24), 12, Z(12)], to: [9.5, 0, Z(10)], yawDeg: 0 }], _why: "NIGHT46 A1 (THE HOOK YARD's Hazard both ways; A-4), NIGHT52 F loop: the well 5 x 12 m: feet under the fall line -> back on its south lip, 5 health" };
room.proofs = [
  { kind: "walk", points: [P(0, 0, 2), P(0, 0, 8), P(4.75, 0, 10), P(4.75, 0, 21), P(0, 0, 22), P(0, 0, 30)] },
  { kind: "walk", points: [P(0, 0, 8), P(-4.75, 0, 9), P(-4.75, 0, 20.5), P(-1, 0, 21.5), P(0, 0, 30)] },
  { kind: "walk", points: [P(-4, 0, 4), P(-7.5, 0, 5.5), P(-7.5, 4, 16.5), P(-9, 4, 25)] },
  { kind: "walk", points: [P(0, 0, 27), P(1.2, 0, 30.5), P(7, 2, 30.5), P(8.5, 2, 26)] },
  { kind: "walk", points: [P(-2.5, 0, 21), P(-4.75, 0, 21.6), P(-4.75, 4, 31.8), P(-8, 4, 31)] },
];
room.frames36 = { mid: [0, 0, Z(3), 8.973, 0.1], exit: [4, 0, Z(22), 21.801, 0] };
room.frames52 = {
  _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = the start passage looking at the drum, the hook over the terrace upper left, the well panel right; mid = the east lane's south end beside the well looking up the panel at the engine deck; high = on the terrace's north end looking south-east over the yard",
  entry: [0, 0, Z(1.5), 0, 0.1],
  mid: [4.75, 0, Z(9), -15, 0.15],
  high: [-9, 4, Z(30), -150, -0.25],
};
room.span46_why = room.span46_why; // NIGHT46 A1's row note kept as it was
room.loop52_why = "NIGHT52 F round 7 (Tools/night/n52-loop-f-p1.cjs; the room loop, D-12 teach room, D-24's way): THE HOOK YARD II at 24 x 32 m (36 m was over the contract's 32), the lesson and waves kept; the four 1.2 m crates (waist-high cover) and the 32 x 36 m yard's 12 m start passage cut; the chain wall, the engine deck and its ramp, the terrace's back ramp, two pads and two more rings added";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside p1 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pads", "pits", "lights", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-p1: p1 THE HOOK YARD II (24 x 32 m)");

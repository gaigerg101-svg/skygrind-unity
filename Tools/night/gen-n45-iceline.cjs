// NIGHT45 builder A2 (design.n45.iceline45; NIGHT45.md card A2; GG 2026-10-17 J4: "if it arena related please dont just randomly plce stuff it needs
// intension and personality"; DESIGN-RULES A-9 identity before geometry, A-1 every piece has a reason, R-3 / R-4, A-5 hazards both ways):
// Level 2 as n45-iceline: n43-iceline's plan (Design/levels/n43/iceline.plan.json, written by Tools/night/gen-n43-iceline.cjs, which this file does not run
// or edit) copied whole, with ONLY g7 THE MIX rebuilt from its identity card (docs/ROOMS.md, "NIGHT45 identity cards (level 2)": THE THAW LOCK). Every
// other room, door, light and piece is n43-iceline's bit for bit (the generator throws if a piece outside g7 would change), so `--off45` (n43-iceline) and
// this file differ in g7 only.
//
// THE THAW LOCK (the card in one breath): the ice line carried coal north on barges; at this lock its keepers burned the cargo in a grate under the channel
// so the lock never froze. Now the grate burns under the tank crews and the swarm; spitters stand on the keepers' coal gantries. The law: a BRIDGE room:
// the burning grate cuts it in two; walkers cross only on its two bridges, the player anywhere a jump + air dash carries him.
//
// The room (x across, z into the room; the door d6 at z -184, the exit at z -214, door d7 at z -216; rect, doors, roster and lesson are n43's):
//   z -184 .. -188   THE DOOR LANDING: bare, unlit (the compression before the release; nothing spawns in view of it)
//   z -188 .. -195.5 THE QUAY: the stone apron before the grate, the run-up (dry: the ice apron of the first cut is gone, see section 2)
//   z -195.5 .. -202.5 THE GRATE: three lava pits (Lava30) W / M / E with two 3 m floor BRIDGES between them (x -10.5..-7.5, 7.5..10.5);
//                    THE BOILER (a 3 x 3 m stack, the landmark) stands in the middle pit on the door's axis, up to the roof
//   z -202.5 .. -216 THE FAR FLOOR; the COAL GANTRIES (2 m decks, x -15..-4 and 4..15, z -210..-216) with a ramp each; the chute between them to the exit
// Movement numbers for the shortcut (M-5; PortExport/config.json: jump.height 1.5, apexTime 0.36 -> 0.72 s from a jump to the same floor; ground.walkSpeed
// 8; dash.speed 24, duration 0.25): a plain run's jump carries 8 x 0.72 = 5.8 m (short of the 7 m grate: the fall returns you to the lip); a jump with an
// air dash adds 6 m (measured by `n45mix jump`: lands 2.2 m past the far edge). The dash's 24 m/s is over FUSION's 14 m/s (design.n43.fusion43.speedMin):
// a gauntlet hit out of that dash is a FUSION.
// The roster stays n43's (heavy, ranged, fodder: R-3's three) with one data change: wave 2 (the escalate) spawns from the door side's coal bunkers
// (waveNodes43), the state change data can make (the fight turns around; the grate is crossed back).
// Run from the repo (or worktree) root: bash Tools/night/n45-iceline-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n43/iceline.plan.json");
const OUT = path.join(ROOT, "Design/levels/n45/iceline.plan.json");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n43-iceline") throw new Error(`unexpected source id ${plan.id}`);
const before = JSON.parse(JSON.stringify(plan));

const ROOM = "g7";
const room = plan.rooms.find((q) => q.id === ROOM);
if (!room) throw new Error("g7 missing");
const [RX0, RZ0, RX1, RZ1] = room.rect; // [-15, -216, 15, -184]
if (JSON.stringify(room.rect) !== JSON.stringify([-15, -216, 15, -184]) || room.height !== 10) throw new Error("g7's rect / height changed upstream: " + JSON.stringify(room.rect));
if (JSON.stringify(room.waves) !== JSON.stringify([["tank", "spitter", "spitter"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "spitter"], ["tank", "spitter", "swarmer", "swarmer", "swarmer", "swarmer"]]))
  throw new Error("g7's waves changed upstream: " + JSON.stringify(room.waves));

// ---- the card's numbers (every one used below; m) -------------------------------------------------------------------------------------------------
const DOOR_Z = RZ1; // -184: the door side
const LANDING_D = 4; // the door landing's depth (bare, unlit)
const APRON_Z1 = DOOR_Z - LANDING_D; // -188: the ice starts
const GRATE_Z1 = -195.5, GRATE_Z0 = -202.5; // the grate's near / far edges: 7 m across (the shortcut's number above)
const LAVA_Y = -0.8; // the lava surface 0.8 m under the floor (LEVEL-FORMAT: 0.6-1.0, a touch's 9 m/s knock clears the rim)
const BRIDGE_W = 3, BRIDGE_X = 9; // the bridges' width and centre |x|: x 7.5..10.5 either side
const BOILER = 3; // the boiler's side (x / z), centred in the grate on x 0
const GANTRY_H = 2, GANTRY_D = 6, GANTRY_INNER = 4; // the gantries: 2 m high (the KESTREL's height, not the glove's), 6 m deep, x |4|..15
const RAMP_X = 12.5, RAMP_W = 3, RAMP_LEN = 6; // the ramps onto the gantries (2 m over 6 m: n43's perch ramps' slope)
const bx0 = BRIDGE_X - BRIDGE_W / 2, bx1 = BRIDGE_X + BRIDGE_W / 2; // 7.5, 10.5
const gz = RZ0 + GANTRY_D; // -210: the gantries' front edge
const boilerZ = (GRATE_Z0 + GRATE_Z1) / 2; // -199

const why = (o, job, text) => Object.assign(o, { job, why: text, note: text });

// ---- 1. the old g7 pieces out (the cut list: n43's NW / NE perches + their ramps, the two side ride panels, the five lights, the pickup) -------
const cut = {};
const keep = (key, pred) => {
  const a = plan[key] || [];
  const n0 = a.length;
  plan[key] = a.filter((e) => !pred(e));
  cut[key] = n0 - plan[key].length;
};
keep("boxes", (b) => b.zone === ROOM);
keep("ramps", (r) => r.zone === ROOM);
keep("pits", (p) => p.zone === ROOM);
keep("lights", (l) => l.room === ROOM);
keep("props", (p) => p.room === ROOM);
plan.surfaces = plan.surfaces || [];
keep("surfaces", (s) => s.room === ROOM);
if (cut.boxes !== 4 || cut.ramps !== 2 || cut.lights !== 5 || cut.props !== 1 || cut.pits !== 0 || cut.surfaces !== 0) throw new Error("g7's n43 pieces changed upstream: " + JSON.stringify(cut));

// insert the new rows where the old g7 rows stood (the array order of the other rooms' rows is unchanged)
const insertAt = (key, beforeArr, rows) => {
  const old = beforeArr[key] || [];
  const first = old.findIndex((e) => (e.zone || e.room) === ROOM);
  const at = first < 0 ? plan[key].length : first;
  plan[key].splice(at, 0, ...rows);
};

// ---- 2. (cut) the ice apron. The first cut laid an ice surface (Glide30) over the quay as the dash-jump's run-up. Measured: the walk bot slid off it into
// the grate 250-430 times in a failed run (5/10 cleared); the 30 s problem asks the player to shoot the gantries from here, strafing, so a person would
// too (A-4: failure must be chosen). The quay is dry stone; the shortcut is the air dash. Kept below as data for the record (not inserted).
const CUT_ICE = [
  { kind: "ice", rect: [RX0, GRATE_Z1, RX1, APRON_Z1], minY: -0.5, maxY: 0.5, room: ROOM,
    note: "THE ICE APRON (A2, the card's run-up; design.n26.surfaces + design.n30.glide): the lock's frozen basin between the landing and the grate. A dash here ends at ~15 m/s (glide.dashAdd 7) and its jump clears the 7 m grate (the earned shortcut); no input glides you toward the fire (the tension). Enemies do not slide (player-only rule): it is the player's tool, not a hazard." },
];
void CUT_ICE;

// ---- 3. the grate: three lava pits, the two bridges between them (floor left uncut) ---------------------------------------------------------------
const pit = (x0, x1, text) => why({ zone: ROOM, rect: [x0, GRATE_Z0, x1, GRATE_Z1], floorY: LAVA_Y, lava: true }, "hazard", text);
insertAt("pits", before, [
  pit(RX0, -bx1, "THE GRATE, west pit (4.5 x 7 m, lava 0.8 m down): the keepers' fire under the lock. Both ways (A-5): you lose 25 % and are knocked up; a body punched off the west bridge's outer side burns and dies. With the middle and east pits it leaves the walkers only the two bridges."),
  pit(-bx0, bx0, "THE GRATE, middle pit (15 x 7 m, the boiler standing in it): the widest fire, crossed only by the dash-jump (15 m/s x 0.72 s = 10.8 m; a plain run's jump 5.8 m falls in): the shortcut skill earns. A body launched off either bridge's inner side lands here."),
  pit(bx1, RX1, "THE GRATE, east pit (4.5 x 7 m): the east bridge's outer fire (the west pit's mirror): the pack crossing the east bridge has fire on both sides."),
]);

// ---- 4. the solids (each one's route / encounter decision) ---------------------------------------------------------------------------------------
const box = (kind, min, max, job, text, extra) => why(Object.assign({ zone: ROOM, kind, min, max }, extra || {}), job, text);
// (tried and cut, A2's notes: a 0.9 m coping along the grate's edges. The walk bot still dashed over it and the pit became 1.7 m deep, past the lava's
// 9 m/s knock-up (1.75 m): a burn spiral, 12 deaths in 150 s. The player's cost is fallReturn43 instead, section 7.)
insertAt("boxes", before, [
  box("hull", [-BOILER / 2, LAVA_Y, boilerZ - BOILER / 2], [BOILER / 2, room.height, boilerZ + BOILER / 2], "cover",
    "THE BOILER (the landmark and the hero prop; A-6, A-9): the stack that kept the lock open, standing in the middle of the grate on the door's axis, 3 x 3 m from the fire to the roof: the only roof-high solid, lit from below. In play it splits the middle fire into two 6 m lanes for the dash-jump and cuts the gantries' line of fire to the door landing (the refuge)."),
  box("deck", [RX0, 0, RZ0], [-GANTRY_INNER, GANTRY_H, gz], "line",
    "the WEST COAL GANTRY (2 m, 11 x 6 m): the keepers' coal stand, the spitters' high ground ~29 m from the door across the fire: the KESTREL's range, not the glove's (R-4: ranged on high ground)"),
  box("deck", [GANTRY_INNER, 0, RZ0], [RX1, GANTRY_H, gz], "line",
    "the EAST COAL GANTRY (the west's mirror): the second spitter stand, so the two bridges are each covered from the gantry beyond them"),
]);
insertAt("ramps", before, [
  why({ zone: ROOM, from: [-RAMP_X, gz + RAMP_LEN], to: [-RAMP_X, gz], width: RAMP_W, rise: GANTRY_H }, "climb",
    "the west gantry's ramp (2 m over 6 m): the spitters' way down and yours up: the glove reaches a gantry spitter only here or by a jump off the far floor"),
  why({ zone: ROOM, from: [RAMP_X, gz + RAMP_LEN], to: [RAMP_X, gz], width: RAMP_W, rise: GANTRY_H }, "climb", "the east gantry's ramp (the west's mirror)"),
]);

// ---- 5. lights (the light story: the landing dark, the apron dim, the grate hot from below, the gantries amber, the exit cyan) --------------------
const light = (id, kind, at, normal, size, color, range, intensity, extra) => Object.assign({ id: `${ROOM}-${id}`, room: ROOM, kind, at, normal, size, color, range, intensity }, extra || {});
const poolY = LAVA_Y + 0.025;
insertAt("lights", before, [
  light("L1", "ceiling", [0, room.height - 0.05, (APRON_Z1 + GRATE_Z1) / 2], [0, -1, 0], [4, 4, 0.1], "white", 10, 0.7),
  // the room's ONE accent (LevelValidate29: one amber / orange light per room): the grate's orange pool at the boiler's door-side foot (the lava's own
  // shader glows in every pit; this light throws the fire onto the boiler and the bridges)
  light("L2", "pool", [0, poolY, GRATE_Z1 - 1], [0, 1, 0], [5, 1.6, 0.05], "orange", 10, 1.4, { danger: true }),
  light("L3", "wall", [0, 4, boilerZ + BOILER / 2 + 0.05], [0, 0, 1], [1.6, 0.6, 0.1], "white", 8, 1.0),
  light("L4", "ceiling", [-(RX1 + GANTRY_INNER) / 2, room.height - 0.05, (RZ0 + gz) / 2], [0, -1, 0], [5, 4, 0.1], "white", 10, 1.0),
  light("L5", "ceiling", [(RX1 + GANTRY_INNER) / 2, room.height - 0.05, (RZ0 + gz) / 2], [0, -1, 0], [5, 4, 0.1], "white", 10, 1.0),
  light("L6", "pool", [room.exit[0], 0.025, room.exit[2]], [0, 1, 0], [2.5, 2.5, 0.05], "cyan", 8, 1.6),
]);
const LIGHT_WHY = {
  L1: "the quay's one dim panel: the run-up reads, the fight's light is the fire's",
  L2: "the grate's fire from below (danger; the room's one accent): lights the boiler's foot and the bridges' edges",
  L3: "the BOILER's gauge lamp (on its door-side face at 4 m): the landmark lit as you enter",
  L4: "the west gantry's coal lamp: the spitters' high ground reads", L5: "the east gantry's coal lamp", L6: "the exit's cyan pool (A-6: the exit is lit)",
};
for (const l of plan.lights) if (l.room === ROOM) { const k = l.id.slice(ROOM.length + 1); l.why = LIGHT_WHY[k]; }

// ---- 6. props: the health past the boiler (the shortcut's prize) and the boiler's collar (the landmark's silhouette) ---------------------------
insertAt("props", before, [
  { id: `${ROOM}-P1`, room: ROOM, kind: "healthPickup", at: [0, 0.9, GRATE_Z0 - 3], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
    why: "the health 3 m past the grate behind the boiler: the dash-jump's landing pays (a walker's route over a bridge reaches it 9 m later)" },
  { id: `${ROOM}-P2`, room: ROOM, kind: "hero", at: [0, room.height - 0.8, boilerZ], size: [BOILER + 0.6, 1.2, BOILER + 0.6], yawDeg: 0, collider: false, decor: true, job: "landmark",
    why: "the BOILER's collar under the roof (decor, no collider: out of reach): the stack reads as a stack from the door, not a pillar (V-4: the world)" },
]);

// ---- 7. the room row: nodes on the far floor, the gantries' high nodes, wave 2 from the door side's bunkers, the walk proofs ----------------------
const farZ = GRATE_Z0 - 3; // -205.5
room.nodes = [[-BRIDGE_X, 0, farZ], [BRIDGE_X, 0, farZ], [-4, 0, farZ - 1.5], [4, 0, farZ - 1.5], [-2, 0, farZ - 3], [2, 0, farZ - 3]];
room.nodes_why = "A2 (R-4): the far floor beyond the grate: the tank and the pack spawn across the fire and must come over a bridge (the bridgeheads x +-9, then the floor before the chute)";
room.highNodes = [[-12, GANTRY_H, RZ0 + 3], [-7, GANTRY_H, RZ0 + 2.5], [12, GANTRY_H, RZ0 + 3], [7, GANTRY_H, RZ0 + 2.5]];
room.highNodes_why = "A2 (R-4: ranged on high ground): the coal gantries, 2 m up, ~29 m from the door, across the fire";
const bunkers = [[-13, 0, APRON_Z1 - 1.5], [13, 0, APRON_Z1 - 1.5], [-13, 0, GRATE_Z1 + 2.5], [13, 0, GRATE_Z1 + 2.5], [-10, 0, APRON_Z1 - 3], [10, 0, APRON_Z1 - 3]];
room.waveNodes43 = [null, null, bunkers];
room.waveNodes43_why = "A2 (the card's state change, data only): wave 2 (tank, spitter, 4 swarmers) comes out of the coal bunkers in the door side's corners, 10-13 m off the door's axis: the fight turns around and the grate is crossed back (spawn33's tells show each body before it lands)";
// the player's cost of the fire (A-4, DECIDE43 D-5; L1's GANTRY rule): the feet under -0.3 m in the grate -> back to the apron's lip 2 m before that
// pit, 10 health, before the lava's touch (-0.75 m) can burn; enemies have no such rule: in the fire they burn and die (Lava30)
const LIP_Z = GRATE_Z1 + 2; // -193.5
const lipX = { W: -(RX1 + bx1) / 2, M: -(bx0 + BOILER / 2) / 2, E: (RX1 + bx1) / 2 };
room.fallReturn43 = {
  belowY: -0.3, to: [lipX.M, 0, LIP_Z], yawDeg: 0, damage: 10,
  by: [
    { rect: [RX0, GRATE_Z0, -bx1, GRATE_Z1], to: [lipX.W, 0, LIP_Z], yawDeg: 0 },
    { rect: [-bx0, GRATE_Z0, 0, GRATE_Z1], to: [lipX.M, 0, LIP_Z], yawDeg: 0 },
    { rect: [0, GRATE_Z0, bx0, GRATE_Z1], to: [-lipX.M, 0, LIP_Z], yawDeg: 0 },
    { rect: [bx1, GRATE_Z0, RX1, GRATE_Z1], to: [lipX.E, 0, LIP_Z], yawDeg: 0 },
  ],
  _why: "NIGHT45 A2 (A-4, DECIDE43 D-5; the GANTRY's rule): feet under -0.3 m in the grate (the lava at -0.8) -> back on the apron 2 m before that pit's near edge, facing the grate again, 10 health, a short fade: a missed dash-jump costs the run-up, never the fight. Enemies burn (Lava30). The 0.9 m coping tried first trapped the bot (a 1.7 m pit, past the lava's knock-up).",
};
room.proofs = [
  { kind: "walk", points: [[0, 0, DOOR_Z - 2], [-BRIDGE_X, 0, APRON_Z1 - 2], [-BRIDGE_X, 0, boilerZ], [-BRIDGE_X, 0, farZ], [0, 0, farZ - 4], [0, 0, room.exit[2]]] },
  { kind: "walk", points: [[BRIDGE_X, 0, farZ], [RAMP_X, 0, gz + RAMP_LEN + 1], [RAMP_X, GANTRY_H, gz - 1], [12, GANTRY_H, RZ0 + 3]] },
];
room.name = "2.7 THE MIX";
room.verb36 = "switch: the KESTREL for the gantries across the fire, the glove for the bridge; the dash-in punches a body into the grate";
room.iceline45_why = "NIGHT45 A2 (DESIGN-RULES A-9; docs/ROOMS.md's card THE THAW LOCK): rebuilt from its identity card: the quay, the grate (three lava pits, two 3 m bridges), THE BOILER in the fire, the two coal gantries with ramps; nodes across the fire, the spitters on the gantries, wave 2 from the door side. n43's rect, doors, waves and lesson kept.";

// ---- 8. guard: nothing outside g7 changed ----------------------------------------------------------------------------------------------------
const outside = (key) => (a) => (a || []).filter((e) => (e.zone || e.room) !== ROOM).map((e) => JSON.stringify(e));
for (const key of ["boxes", "ramps", "pits", "lights", "props", "surfaces", "pads", "grapple", "tunnels", "doors"]) {
  const a = outside(key)(before[key]), b = outside(key)(plan[key]);
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(`a piece outside g7 changed in ${key}`);
}
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== ROOM && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (before.surfaces === undefined && plan.surfaces.length === 0) delete plan.surfaces;

plan.id = "n45-iceline";
plan._why = "NIGHT45 builder A2 (design.n45.iceline45 swaps n43-iceline for this file while the row is on; GG 2026-10-17 J4; DESIGN-RULES A-9): Tools/night/gen-n45-iceline.cjs copies n43-iceline's plan whole and rebuilds g7 THE MIX only, from its identity card THE THAW LOCK (docs/ROOMS.md): a stone quay, a burning grate (Lava30) with two bridges and THE BOILER standing in it, two coal gantries; wave 2 from the door side. n43-iceline's _why: " + before._why;

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
const mine = (k) => (plan[k] || []).filter((e) => (e.zone || e.room) === ROOM).length;
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms; g7 cut", JSON.stringify(cut), "; g7 now boxes", mine("boxes"), "ramps", mine("ramps"), "pits", mine("pits"), "lights", mine("lights"), "props", mine("props"), "surfaces", mine("surfaces"));

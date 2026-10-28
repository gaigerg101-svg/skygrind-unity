// NIGHT47 builder B (design.n47.iceline47 swaps n46-iceline for n47-iceline while the row is on; NIGHT47.md G25 / G27; the card gate PASS:
// Research/2026-10-18-wave47/B-cards.md section 2; DECIDE47 D-10). Level 2 as n47-iceline: n46-iceline's plan (Design/levels/n46/iceline.plan.json,
// written by Tools/night/gen-n46-iceline.cjs, which this file does not run or edit) copied whole, with:
//   G25 the CHARGER route: g1 2.1 TANK ALONE gets a second wave, ONE charger alone after the tank (the first third; R-3 broken on purpose,
//       DECIDE47 D-10: GG "needs to appear earlier and more frequently"); g2 2.2 THE SHIELD a third wave [charger, husk, husk]; g5 2.5 THE
//       COOLDOWN's second wave + 1 charger (a charge through the swarm bursts it: charger47); g8 rebuilt below. Chargers before THE BRAWL:
//       4 in 1 room -> 9 in 4 rooms.
//   G27 g8 2.8 THE REHEARSAL -> 2.8 THE STAMPEDE, the card THE ROUNDHOUSE: six 5 m columns (three pairs) making three lanes, THE PIT along the
//       centre lane (a charger that charges across it falls in and is held), THE GALLERY across the far end in two halves either side of the
//       exit with a ramp each (the spitters' high ground and the walkers' way up), two coal carts, an east ride panel, the turntable gate over
//       the exit (the landmark), two red-orange gallery lamps; waves [charger], [charger x2, husk x2], [charger x3, spitter x2].
// Every other room, door, light and piece is n46-iceline's bit for bit (asserted below).
// Run from the worktree root: bash Tools/night/n47-iceline-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n46/iceline.plan.json");
const OUT = path.join(ROOT, "Design/levels/n47/iceline.plan.json");

const srcText = fs.readFileSync(SRC, "utf8");
const plan = JSON.parse(srcText);
const before = JSON.parse(srcText);
if (plan.id !== "n46-iceline") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n47-iceline";
const room = (id) => { const r = plan.rooms.find((q) => q.id === id); if (!r) throw new Error(`room ${id} missing`); return r; };
const same = (a, b, what) => { if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(`${what} changed upstream: ${JSON.stringify(a)}`); };
const zoneOf = (e) => e.zone || e.room;
const MINE = ["g1", "g2", "g5", "g8"];
const log = [];
const why = (o, job, text) => Object.assign(o, { job, why: text, note: text });
const lesson = (text) => ({ promptPad: text, kind: "husksDead", n: 0, prompt: text });

plan._why = "NIGHT47 builder B (design.n47.iceline47 swaps n46-iceline for this file while the row is on; GG 2026-10-18 \"the charging enemy in the second stage needs to appear earlier and more frequently before the boss fight ... a mini challenge with three or more of them plus other enemies\"): "
  + "Tools/night/gen-n47-iceline.cjs copies n46-iceline's plan whole: a charger alone in g1 after the tank, one in g2's third wave and g5's second, and g8 rebuilt as THE STAMPEDE (the card THE ROUNDHOUSE). n46-iceline's _why: " + plan._why;

// ---- G25 g1 2.1 TANK ALONE: wave 2, the charger alone, through the exit door, its first charge down the room's 20 m axis
{
  const g1 = room("g1");
  same(g1.waves, [["tank"]], "g1's waves");
  same(g1.lessons.length, 1, "g1's lessons");
  g1.waves = [["tank"], ["charger"]];
  g1.waveNodes43 = [null, [[0, 0, -22]]];
  g1.waveNodes43_why = "NIGHT47 B (G25): wave 2's CHARGER comes in at the exit door's mouth (0, -22), 20 m down the room's axis from the spawn and in view, "
    + "alone; its first charge runs at you down the axis: sidestep late and it breaks on the entry wall or a ride panel (x +-8), its back open";
  const L2 = Object.assign(lesson("Dodge late. It can't turn."), { n: 2 });
  g1.lessons = g1.lessons.concat([L2]);
  if (g1.lessons31) g1.lessons31 = g1.lessons31.concat([Object.assign({}, L2)]);
  g1.iceline47_why = "NIGHT47 B G25 (DECIDE47 D-10): a second wave, the level's first CHARGER alone after the tank (R-3 broken on purpose: GG's \"appear earlier\"); its lesson (copy for F) after the tank's";
  log.push("g1 charger alone");
}

// ---- G25 g2 2.2 THE SHIELD: wave 3 [charger, husk, husk] (the charger with known fodder: R-2 reinforce)
{
  const g2 = room("g2");
  same(g2.waves, [["warden"], ["warden", "husk", "husk"]], "g2's waves");
  g2.waves = g2.waves.concat([["charger", "husk", "husk"]]);
  g2.iceline47_why = "NIGHT47 B G25: wave 3 [charger, husk, husk]: the flank pillars (x +-9..9.6) break its charge; at most 3 roles at once (warden, husk, charger)";
  log.push("g2 wave 3");
}

// ---- G25 g5 2.5 THE COOLDOWN: wave 2 = the swarm + 1 charger (bait it through the swarm while the blast cools)
{
  const g5 = room("g5");
  same(g5.waves[1], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"], "g5's wave 2");
  g5.waves[1] = ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "charger"];
  g5.iceline47_why = "NIGHT47 B G25: wave 2 is the six swarmers + 1 charger (its two husks out: LevelValidate's budget is 2 kinds a wave): with charger47 a charge through the swarm bursts it (the charger as your tool while the blast cools)";
  log.push("g5 charger");
}

// ---- G27 g8 -> 2.8 THE STAMPEDE (THE ROUNDHOUSE; B-cards.md section 2, flow plan points 1-11)
{
  const R = "g8";
  const g8 = room(R);
  same(g8.rect, [-12, -250, 12, -220], "g8's rect");
  same(g8.spawn, { at: [0, 0, -222], yawDeg: 0 }, "g8's spawn");
  same(g8.exit, [0, 0, -248], "g8's exit");
  if (plan.boxes.some((b) => b.zone === R) || plan.ramps.some((r) => r.zone === R) || plan.pits.some((p) => p.zone === R)) throw new Error("g8 had boxes / ramps / pits upstream");
  same(plan.props.filter((p) => p.room === R).map((p) => p.id), ["g8-P1", "g8-P2", "g8-P3", "g8-P4"], "g8's props");
  g8.name = "2.8 THE STAMPEDE";
  const GAL = { y: 3, z0: -250, z1: -247 }; // the gallery deck
  const box = (kind, min, max, job, text, extra) => why(Object.assign({ zone: R, kind, min, max }, extra || {}), job, text);
  const firstBox = plan.boxes.findIndex((b) => b.zone === "gr");
  plan.boxes.splice(firstBox < 0 ? plan.boxes.length : firstBox, 0,
    box("deck", [-12, 0, GAL.z0], [-3, GAL.y, GAL.z1], "cover",
      "THE GALLERY, west half (3 m deck, x -12..-3; card: Every-solid rule): the coaling gallery: the spitters' high ground 20-28 m from the door (the KESTREL's range, out of the glove's reach), cover from the lanes; stops 1 m short of the exit door (x -2..2)"),
    box("deck", [3, 0, GAL.z0], [12, GAL.y, GAL.z1], "cover",
      "THE GALLERY, east half (3 m deck, x 3..12): the second spitter stand; the two halves leave the exit door's 6 m clear"),
    box("deck", [-12, 0, -236], [-9.5, 0.9, -234.5], "cover",
      "the WEST COAL CART (0.9 m, 2.5 x 1.5 m, against the west wall; flow plan 5): jump-height cover in lane W between the column pairs; one jump clears it; the lane's walk line (x -8) passes it"),
    box("deck", [8.5, 0, -231], [11, 0.9, -229.5], "cover",
      "the EAST COAL CART (0.9 m): jump-height cover in lane E, offset from the west one (asymmetric sides, flow plan 7)"),
    box("wall", [11.4, 0, -243], [12, 5, -223], "line",
      "the EAST RIDE PANEL (5 m, 20 m, face west; card: Molecule's earned shortcut): from the door along lane E to the gallery's east half without touching the floor the chargers own",
      { ride: true, ride36: { face: "W", planeX: 11.4, lz: [3, 23] } }));
  const firstRamp = plan.ramps.findIndex((r) => r.zone === "gr");
  plan.ramps.splice(firstRamp < 0 ? plan.ramps.length : firstRamp, 0,
    why({ zone: R, from: [-10.5, -240], to: [-10.5, GAL.z1], width: 3, rise: GAL.y }, "climb",
      "the GALLERY's WEST RAMP (3 m over 7 m, against the west wall): the walkers' and spitters' way up (GG NIGHT20), yours"),
    why({ zone: R, from: [9.5, -240], to: [9.5, GAL.z1], width: 3, rise: GAL.y }, "climb",
      "the GALLERY's EAST RAMP (3 m over 7 m, inside the ride panel): the second way up; each gallery half has two outs (its ramp, the drop)"));
  const firstPit = plan.pits.findIndex((p) => zoneOf(p) === "gr");
  plan.pits.splice(firstPit < 0 ? plan.pits.length : firstPit, 0,
    why({ zone: R, rect: [-1.5, -240, 1.5, -229], floorY: -1.5 }, "hazard",
      "THE PIT (3 x 11 m, 1.5 m deep, along lane C; card: Hero prop, Hazard both ways): the shed's inspection pit. A charger whose charge crosses it falls in and cannot climb out (ai.jumps.charger.jumps false: it stands dazed): the free punish from above; a player who falls in jumps or double-jumps out (a jump reaches 1.58 m, a double 2.88 m: n47b reach). It cuts lane C only: the strafe ring round it stays whole"));
  // the six columns (three pairs), replacing n46's four
  plan.props = plan.props.filter((p) => p.room !== R);
  const firstProp = plan.props.findIndex((p) => p.room === "gr");
  const col = (n, x, z, side) => ({ id: `${R}-P${n}`, room: R, kind: "pillar", at: [x, 2.5, z], size: [2.5, 5, 2.5], yawDeg: 0, collider: true, job: "cover",
    why: `the ${side} COLUMN (2.5 x 2.5 x 5 m; card: Every-solid rule): a charge breaks on it (stunned 2.0 s, its back open); cover from the gallery's spitters; with its pair it makes the three lanes` });
  plan.props.splice(firstProp < 0 ? plan.props.length : firstProp, 0,
    col(1, -5, -228, "SW"), col(2, 5, -228, "SE"), col(3, -5, -235, "W"), col(4, 5, -235, "E"), col(5, -5, -242, "NW"), col(6, 5, -242, "NE"),
    { id: `${R}-P7`, room: R, kind: "hero", at: [0, 6, -249.8], size: [7, 5, 0.3], yawDeg: 0, collider: false, decor: true, job: "marker",
      why: "THE TURNTABLE GATE (card: Landmark): the engine shed's round door over the exit, its plough-blade frame red-orange, lit from behind: seen from the door, the only round shape in the room" });
  // lights (card: Light / colour story): the three white panels kept, two white gallery lamps (the room keeps LevelValidate's one accent; the
  // red-orange flip at wave 3 is shift46's fixture rule, named for the view), the exit pool kept
  same(plan.lights.filter((l) => l.room === R).map((l) => l.id), ["g8-L1", "g8-L2", "g8-L3", "g8-L4"], "g8's lights");
  const L4 = plan.lights.findIndex((l) => l.id === "g8-L4");
  const lamp = (id, x, text) => ({ id: `${R}-${id}`, room: R, kind: "wall", at: [x, GAL.y + 2, GAL.z0 + 0.05], normal: [0, 0, 1], size: [1.2, 0.5, 0.1], color: "white", range: 9, intensity: 1.2, why: text });
  plan.lights.splice(L4, 0,
    lamp("L5", -7.5, "the gallery's west lamp (white; the red-orange flip when wave 3 comes is shift46's fixture rule, named for the view): the spitters' high ground reads"),
    lamp("L6", 7.5, "the gallery's east lamp (the same rule)"));
  g8.waves = [["charger"], ["charger", "charger", "husk", "husk"], ["charger", "charger", "charger", "spitter", "spitter"]];
  g8.waveNodes43 = [
    [[-8, 0, -246], [6.5, 0, -246]],
    [[-8, 0, -246], [6.5, 0, -246], [-10.5, 0, -239], [9.5, 0, -239]],
    [[-8, 0, -246], [6.5, 0, -246], [0, 0, -246], [-8, GAL.y, -248.5], [8, GAL.y, -248.5]],
  ];
  g8.waveNodes43_why = "NIGHT47 B G27 (THE ROUNDHOUSE, flow plan 8): wave 1's charger at a side lane's far end (+-8, -246), in view from the door past the columns; "
    + "wave 2's two chargers at both lane ends and the husks at the ramps' feet; wave 3's three chargers at the three lane ends (the centre one's charge crosses THE PIT) "
    + "and two spots on the gallery for the spitters (a wave's spots are a pool: which body takes which spot is not fixed). Never behind the door";
  g8.nodes = [[-8, 0, -246], [6.5, 0, -246], [0, 0, -246]];
  g8.lessons = [lesson("Hit its back with {fire}."), lesson("Lead one into the pit.")].map((l, i) => Object.assign(l, { n: i + 1 }));
  if (g8.lessons31) g8.lessons31 = g8.lessons.map((l) => Object.assign({}, l));
  g8.verb36 = "the late dodge into a column, then the punish from behind with the right tool (the KESTREL on the gallery and through backs, the punch-launch of a stunned one)";
  g8.job36 = "the stampede: three chargers and the gallery's spitters before THE BRAWL";
  g8.proofs = [
    { kind: "walk", points: [[0, 0, -222], [-8, 0, -226], [-8, 0, -238], [-3, 0, -246], [0, 0, -248]] },
    { kind: "walk", points: [[0, 0, -222], [7.5, 0, -226], [7.5, 0, -236], [3, 0, -246], [0, 0, -248]] },
    { kind: "walk", points: [[-10.5, 0, -238], [-10.5, GAL.y, -248], [-6, GAL.y, -248.5]] },
  ];
  g8.iceline47_why = "NIGHT47 B G27: rebuilt as THE STAMPEDE from its card THE ROUNDHOUSE (B-cards.md section 2): 6 columns, THE PIT, THE GALLERY (two halves + ramps), two carts, the east ride panel, "
    + "the turntable gate, two gallery lamps; waves 1 / 2 / 3 chargers (6) with husks and spitters; the door lip of the card cut (the spawn and retry point stand at the door: a lip there holds them in a solid)";
  log.push("g8 THE STAMPEDE");
}

// ---- the guard: nothing outside g1 / g2 / g5 / g8 changed; in g1 / g2 / g5 only the room rows above
for (const k of ["boxes", "ramps", "pits", "lights", "props", "surfaces", "pads", "grapple", "tunnels", "doors"]) {
  const outside = (arr) => (arr || []).filter((e) => zoneOf(e) !== "g8").map((e) => JSON.stringify(e));
  if (JSON.stringify(outside(before[k])) !== JSON.stringify(outside(plan[k]))) throw new Error(`a piece outside g8 changed in ${k}`);
}
for (let i = 0; i < plan.rooms.length; i++) if (!MINE.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) if (!["_why", "id", "rooms", "boxes", "ramps", "pits", "lights", "props"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
const count = (id) => room(id).waves.flat().filter((k) => k === "charger").length;
console.log("wrote", path.relative(ROOT, OUT), `| ${log.join("; ")} | chargers before x9: ${MINE.map((id) => id + " " + count(id)).join(", ")} (total ${MINE.reduce((a, id) => a + count(id), 0)})`);

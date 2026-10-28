// NIGHT46 builder A3 (design.n46.iceline46; NIGHT46.md card A3 "levels 1 and 2 smoothed"; GG 2026-10-17: "need to feel and flow better ... study real game
// deisgn and areana layouts"; DESIGN-RULES A-1 / A-6 / A-9; arena-flow-web.md Part D). Level 2 as n46-iceline: n45-iceline's plan
// (Design/levels/n45/iceline.plan.json, written by Tools/night/gen-n45-iceline.cjs, which this file does not run or edit) copied whole, with:
//   I-1 g4 THE REACH (FLOW-AUDIT row 10, a linear room): a second crossing, the EAST BRIDGE over the canal (a loop round the canal's middle).
//   I-2 g8 THE REHEARSAL (FLOW-AUDIT row 12, wave 1 out of sight): the first charger at the far end of the lane between the pillar rows, in view.
//   I-3 g3 THE SWARM rebuilt from its identity card THE TIPPLE (docs/ROOMS.md, "NIGHT46 identity cards", the card gate PASS 23:20): the HOPPER in the
//       middle (the ring), the TRESTLE on the east (the air line, broken by its one-way drop), the west ride panel kept, four coal cars (cover).
// Every other room, door, light and piece is n45-iceline's bit for bit (asserted below); g7 THE THAW LOCK stays NIGHT45's.
// Run from the repo (or worktree) root: bash Tools/night/n46-iceline-regen.sh
// Probe: N46I_PLAIN=1 writes n45's plan with only the outer name changed (the plan-copy check); it must write to N46I_OUT.
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n45/iceline.plan.json");
const OUT = path.join(ROOT, "Design/levels/n46/iceline.plan.json");
const OUT_FILE = process.env.N46I_OUT ? path.resolve(ROOT, process.env.N46I_OUT) : OUT;
const PLAIN = process.env.N46I_PLAIN === "1";
if (PLAIN && OUT_FILE === OUT) throw new Error("a probe (N46I_PLAIN) must write to N46I_OUT, not the shipped plan");

const srcText = fs.readFileSync(SRC, "utf8");
const plan = JSON.parse(srcText);
const before = JSON.parse(srcText);
if (plan.id !== "n45-iceline") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n46-iceline";
const room = (id) => { const r = plan.rooms.find((q) => q.id === id); if (!r) throw new Error(`room ${id} missing`); return r; };
const same = (a, b, what) => { if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(`${what} changed upstream: ${JSON.stringify(a)}`); };
const zoneOf = (e) => e.zone || e.room;
const MINE = ["g3", "g4", "g8"];
const log = [];
const why = (o, job, text) => Object.assign(o, { job, why: text, note: text });

if (!PLAIN) {
  plan._why = "NIGHT46 builder A3 (design.n46.iceline46 swaps n45-iceline for this file while the row is on; GG 2026-10-17 \"need to feel and flow better\"): "
    + "Tools/night/gen-n46-iceline.cjs copies n45-iceline's plan whole: g4 gets a second bridge (a loop round the canal), g8's first charger stands in view, "
    + "and g3 THE SWARM is rebuilt from its identity card THE TIPPLE (docs/ROOMS.md, NIGHT46). n45-iceline's _why: " + plan._why;

  // ---- I-1 g4 THE REACH (FLOW-AUDIT row 10): the EAST BRIDGE, x 6..9 over the canal (z -112..-106), its top flush with the floor
  {
    const g4 = room("g4");
    const at = plan.boxes.findIndex((b) => b.zone === "g4");
    same(plan.boxes.filter((b) => b.zone === "g4").length, 4, "g4's box count");
    same(plan.pits.filter((p) => p.zone === "g4").map((p) => p.rect), [[-12, -112, 12, -106]], "g4's canal");
    plan.boxes.splice(at + 1, 0, why({ zone: "g4", kind: "deck", min: [6, -1, -112], max: [9, 0, -106] }, "line",
      "the EAST BRIDGE (3 m wide; NIGHT46 A3 I-1, FLOW-AUDIT row 10): the canal's second crossing, 4 m east of the centre bridge, so the floor loops round "
      + "the canal's middle (a hole of 4 x 6 m) and a walker or a dodge has two ways over under the drones' orbs; the canal's ice (y -1.5..-0.5) stays under it"));
    g4.iceline46_why = "NIGHT46 A3 I-1: the east bridge x 6..9 over the canal (2 crossings, a loop); the drones, their nodes, the lesson and the NW ledge unchanged";
    log.push("g4 east bridge");
  }

  // ---- I-2 g8 THE REHEARSAL (FLOW-AUDIT row 12): wave 1's charger at the lane's far end, in line of sight
  {
    const g8 = room("g8");
    same(g8.waveNodes43[0], [[-10, 0, -236], [10, 0, -236]], "g8's wave 1 nodes");
    g8.waveNodes43[0] = [[-10, 0, -232], [10, 0, -232]];
    // round 2 (n46flow --rooms g8,g8, seeds 1-10, entered fresh; n45: dwell 0.4 s, clear 30.1 s): round 1's lane end (+-2.5, -246) dwell 5.5 s / clear
    // 27.8 s; the lane's middle (+-2.5, -240) 5.9 / 26.2; these side-wall spots 3.7 / 25.3 (a visible charger holds the pilot at the door: the lane did most)
    g8.waveNodes43_why = "NIGHT46 A3 I-2 (FLOW-AUDIT row 12): wave 0's CHARGER 2 m off a side wall as n43 meant (x +-10), moved 4 m toward the door (z -232) so it "
      + "stands in front of the near pillars: 14.1 m @ +-45 deg, in line of sight from the door (n46-a3-los.cjs; n43's z -236 was behind them, 0 of 2). Its "
      + "first charge comes along the wall: dash aside and it hits the wall (\"{dash} ASIDE: IT HITS THE WALL\"). Was: " + g8.waveNodes43_why;
    g8.iceline46_why = "NIGHT46 A3 I-2: wave 1's charger nodes in view; the pillars, waves and lessons unchanged";
    log.push("g8 charger in view");
  }

  // ---- I-3 g3 THE SWARM -> THE TIPPLE (the card: docs/ROOMS.md "NIGHT46 identity cards"; Part D flow plan below it)
  {
    const R = "g3";
    const g3 = room(R);
    same(g3.rect, [-13, -90, 13, -62], "g3's rect");
    same(g3.waves, [["husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"]], "g3's waves");
    same(g3.spawn, { at: [0, 0, -64], yawDeg: 0 }, "g3's spawn");
    same(g3.exit, [0, 0, -88], "g3's exit");
    const H = g3.height; // 9
    // the cut (card: Cut list): the PEN (two 3 m hulls), the east ride panel (the trestle's place), the three middle ceiling panels; the west ride panel kept
    const old = plan.boxes.filter((b) => b.zone === R);
    if (old.length !== 4) throw new Error("g3's n45 boxes changed upstream: " + old.length);
    const keepW = old.find((b) => b.ride && b.ride36 && b.ride36.face === "E");
    same([keepW.min, keepW.max], [[-11.6, 0, -78], [-11, 5, -66]], "g3's west ride panel");
    const firstBox = plan.boxes.findIndex((b) => b.zone === R);
    plan.boxes = plan.boxes.filter((b) => b.zone !== R);
    keepW.why = "the WEST RIDE PANEL RW (kept from n43; THE TIPPLE's card: Molecule's earned shortcut): from the SW coal car up its east face and north over the "
      + "4 m west lane, the funnel: a rider is over the pack as it fills the lane, and blasts down into it (VERBS.md 28)";
    keepW.note = keepW.why;
    const box = (kind, min, max, job, text, extra) => why(Object.assign({ zone: R, kind, min, max }, extra || {}), job, text);
    // the hopper: 6 x 8 m, its east face 1 m west of the door's axis (round 1 built it 5 x 5 m: F1's n46flow read its ring as a 31.6 m2 hole, under the
    // meter's 50 m2 loop line; FLOW-AUDIT's central loops were 105-177 m2)
    const HOP = { x0: -7, x1: -1, z0: -82, z1: -74 };
    const TR = { x0: 10, x1: 13, z0: -86, z1: -70, y: 2 }; // the trestle deck
    // 0.9 m: a half wall one jump clears; under LevelValidate24's 1 m tier line (round 1: at 1.2 m the four cars were tiers no walker reaches, 2 errors)
    const CAR_Y = 0.9;
    const car = (cx, cz, name, text) => box("deck", [cx - 1.25, 0, cz - 0.75], [cx + 1.25, CAR_Y, cz + 0.75], "cover",
      `the ${name} COAL CAR (${CAR_Y} m, 2.5 x 1.5 m; card: Every-solid rule, flow plan 5: jump-height cover, one per quadrant, one jump clears it): ${text}`);
    plan.boxes.splice(firstBox, 0,
      box("hull", [HOP.x0, 0, HOP.z0], [HOP.x1, H, HOP.z1], "cover",
        "THE HOPPER (card: Landmark, Spatial law ring): the tipple's coal bin, 6 x 8 m from the floor to the roof, the only roof-high solid; 1 m west of the "
        + "door's axis so the exit's cyan pool shows past its east face (A-6). The ring's centre: the 4 m west lane (the funnel the blast takes) and the 11 m east "
        + "lane (the trestle beside it); its chute faces the door (wave 2 pours from it)"),
      keepW,
      box("deck", [TR.x0, 0, TR.z0], [TR.x1, TR.y, TR.z1], "line",
        "THE TRESTLE (2 m deck, x 10..13, z -86..-70; card: Molecule's T, flow plan 3-4): the coal cars' track on the east wall: the air line off the floor, the "
        + "health's perch at its north end, and the room's one one-way drop (2 m, onto the exit floor); climbed only by its ramp at the door end"),
      car(-5, -69, "SW", "the first foothold 7.1 m in, by the west lane's mouth and RW's foot"),
      car(5, -70, "SE", "the first foothold on the east side, 7.8 m in, by the trestle's ramp"),
      car(-9, -86, "NW", "the west lane's north end: cover where the funnel opens onto the exit floor"),
      car(5, -84, "NE", "the exit floor's east side, under the trestle's drop"));
    // the ramp onto the trestle (2 m over 6 m: g7's gantry ramps' slope)
    const firstRamp = plan.ramps.findIndex((r) => r.zone === R);
    if (plan.ramps.some((r) => r.zone === R)) throw new Error("g3 had ramps upstream");
    plan.ramps.push(why({ zone: R, from: [11.5, -64], to: [11.5, TR.z1], width: 3, rise: TR.y }, "climb",
      "the TRESTLE's RAMP (2 m over 6 m, at the door end): the only way up (the drop at the north end is one way): the walkers' route to the perch (GG NIGHT20) and yours"));
    void firstRamp;
    // lights (card: Light / colour story): the door landing dim; two white panels (the west lane, the far floor); the hopper's amber lamp the one accent;
    // the trestle one white lamp; the exit cyan pool
    const oldL = plan.lights.filter((l) => l.room === R).map((l) => l.id);
    same(oldL, ["g3-L1", "g3-L2", "g3-L3", "g3-L4", "g3-L5"], "g3's lights");
    const firstLight = plan.lights.findIndex((l) => l.room === R);
    const L1 = plan.lights.find((l) => l.id === "g3-L1"), L5 = plan.lights.find((l) => l.id === "g3-L5");
    plan.lights = plan.lights.filter((l) => l.room !== R);
    const light = (id, kind, at, normal, size, color, range, intensity, text, extra) =>
      Object.assign({ id: `${R}-${id}`, room: R, kind, at, normal, size, color, range, intensity }, extra || {}, { why: text });
    L1.intensity = 0.7;
    L1.why = "the door landing's one dim panel (card: Light / colour story: the entry tight and dim)";
    L5.why = "the exit's cyan pool (A-6: the exit is lit; seen past the hopper's east face from the door)";
    plan.lights.splice(firstLight, 0,
      L1,
      light("L2", "wall", [(HOP.x0 + HOP.x1) / 2, 4, HOP.z1 + 0.05], [0, 0, 1], [1.6, 0.6, 0.1], "amber", 10, 1.4,
        "THE HOPPER's amber lamp over the chute (card: Landmark; the room's one accent, LevelValidate29): the brightest point, seen from the door; shift46's fixture (amber -> red-orange as the swarm comes)"),
      light("L3", "ceiling", [11.5, H - 0.05, -78], [0, -1, 0], [3, 4, 0.1], "white", 9, 1.0, "the trestle's one white lamp: the air line reads"),
      light("L4", "ceiling", [3, H - 0.05, -84], [0, -1, 0], [4, 3, 0.1], "white", 9, 1.0, "the far floor's white panel (the exit floor, where the lanes meet)"),
      light("L6", "ceiling", [-8.5, H - 0.05, -75], [0, -1, 0], [3, 4, 0.1], "white", 9, 1.0, "the west lane's white panel: the funnel reads"),
      L5);
    // props (card: Hero prop; flow plan 9: the pickup on the trestle's north end)
    if (plan.props.some((p) => p.room === R)) throw new Error("g3 had props upstream");
    const firstProp = plan.props.findIndex((p) => p.room === "g4");
    plan.props.splice(firstProp < 0 ? plan.props.length : firstProp, 0,
      { id: `${R}-P1`, room: R, kind: "hero", at: [(HOP.x0 + HOP.x1) / 2, 1.5, HOP.z1 + 0.15], size: [2.5, 1.5, 0.3], yawDeg: 0, collider: false, decor: true, job: "hero",
        why: "THE CHUTE (card: Hero prop): the hopper's dark mouth on its door-side face: where the swarm nests (in the coal); wave 2's spawn tells stand at it" },
      { id: `${R}-P2`, room: R, kind: "healthPickup", at: [11.5, TR.y + 0.9, -84.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
        why: "the room's one health (flow plan 9; arena-flow-web F23): on the trestle's north end, the far end from the door, exposed above the one-way drop" });
    // the fight (card: The 30 s problem, State change; flow plan 8)
    const W1 = [[-9.5, 0, -74], [-10, 0, -76], [5, 0, -77], [8, 0, -75]];
    // the chute's two sides (round 1: spots at its mouth, 9.2-10.1 m from the door, broke spawn31's 12 m in the first view cone): 12.5-16.5 m
    const CHUTE = [[-8.5, 0, -75], [-9.5, 0, -77.5], [0, 0, -76.5], [0.5, 0, -78.5]];
    const FAR = [[-6, 0, -86.5], [4, 0, -86], [8, 0, -87]];
    g3.nodes = CHUTE.concat(FAR);
    g3.nodes_why = "NIGHT46 A3 (THE TIPPLE): the chute's mouth and the far floor's corners (the room's default spots: the lesson's top-up wave)";
    g3.waveNodes43 = [W1, CHUTE.concat(FAR)];
    g3.waveNodes43_why = "NIGHT46 A3 (THE TIPPLE, flow plan 8): wave 1 (the crew, 3 husks) either side of the hopper 13.5-14 m from the door, in view; wave 2 (the swarm) "
      + "pours from the chute's two sides (12.5-16.5 m from the door), or from the far floor's corners when you stand within 12 m of the chute (minSpawnDist): always ahead, never behind the door";
    g3.proofs = [
      { kind: "walk", points: [[0, 0, -64], [-8.5, 0, -71], [-8.5, 0, -81], [-3.5, 0, -84], [0, 0, -88]] },
      { kind: "walk", points: [[0, 0, -64], [3, 0, -73], [3, 0, -82], [0, 0, -88]] },
      { kind: "walk", points: [[11.5, 0, -64.5], [11.5, TR.y, -71], [11.5, TR.y, -84.5]] },
    ];
    const yawTo = (x, z, tx, tz) => +(-Math.atan2(tx - x, -(tz - z)) * 180 / Math.PI).toFixed(3);
    g3.frames36 = { mid: [5, 0, -65, yawTo(5, -65, -4, -78), -0.04], exit: g3.frames36.exit };
    g3.verb36 = "the LT blast: fill the glove on the crew, then blast the swarm where the hopper's west lane packs it";
    g3.iceline46_why = "NIGHT46 A3 I-3 (docs/ROOMS.md THE TIPPLE): n43's PEN and east ride panel cut; THE HOPPER (6 x 8 m to the roof, off the door's axis), THE TRESTLE "
      + "(2 m deck + ramp, one-way drop north), four 0.9 m coal cars, the west ride panel kept; wave 1 in view either side of the hopper, wave 2 from the chute; "
      + "rect, doors, waves and lesson unchanged";
    log.push("g3 THE TIPPLE");
  }

  // ---- the guard: nothing outside g3 / g4 / g8 changed; inside g4 / g8 only the rows named above
  for (const k of ["boxes", "ramps", "pits", "lights", "props", "surfaces", "pads", "grapple", "tunnels", "doors"]) {
    const outside = (arr) => (arr || []).filter((e) => !MINE.includes(zoneOf(e))).map((e) => JSON.stringify(e));
    if (JSON.stringify(outside(before[k])) !== JSON.stringify(outside(plan[k]))) throw new Error(`a piece outside g3 / g4 / g8 changed in ${k}`);
    const inG8 = (arr) => (arr || []).filter((e) => zoneOf(e) === "g8").map((e) => JSON.stringify(e));
    if (JSON.stringify(inG8(before[k])) !== JSON.stringify(inG8(plan[k]))) throw new Error(`a g8 piece changed in ${k}`);
    const inG4 = (arr) => (arr || []).filter((e) => zoneOf(e) === "g4" && !(e.why || "").startsWith("the EAST BRIDGE")).map((e) => JSON.stringify(e));
    if (JSON.stringify(inG4(before[k])) !== JSON.stringify(inG4(plan[k]))) throw new Error(`a g4 piece changed in ${k}`);
  }
  for (let i = 0; i < plan.rooms.length; i++) if (!MINE.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
  for (const k of Object.keys(before)) if (!["_why", "id", "rooms", "boxes", "ramps", "lights", "props"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
}

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
const mine = (k) => (plan[k] || []).filter((e) => zoneOf(e) === "g3").length;
console.log("wrote", path.relative(ROOT, OUT_FILE), PLAIN ? "(PLAIN: n45-iceline's plan, the outer name only)" : `| ${log.join("; ")} | g3 boxes ${mine("boxes")} ramps ${mine("ramps")} lights ${mine("lights")} props ${mine("props")}`);

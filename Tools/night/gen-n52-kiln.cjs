// NIGHT52 builder K (row kiln52: n47-kiln -> n52-kiln; NIGHT52.md section 2, THE ROOM CONTRACT; NIGHT52-BRIEF.md section 3). LEVEL 3 as n52-kiln.
// n47-kiln, its plan and its generator are untouched (this file reads n47's plan only), so the row off loads n47-kiln bit for bit.
// From n47's PLAN (Design/levels/n47/kiln.plan.json -> Design/levels/n52/kiln.plan.json), then levelgen, then --finish (n47's finish step, pointed
// at n52). Every room this file does not rebuild is n47's bit for bit (asserted on the plan: its row, boxes, ramps, pits, lights, props).
// Run (from the worktree root):
//   node Tools/night/gen-n52-kiln.cjs
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/kiln.plan.json Design/levels/n52/kiln.json --walk
//   node Tools/night/gen-n52-kiln.cjs --finish
//   node Tools/night/gen-n52-kiln.cjs --check        (n52-kiln.json against n47-kiln.json: equal except the names, while no room is rebuilt)
// NIGHT53 K: kb3's second pass is an in-place edit of the plan this file writes: after this file, run node Tools/night/n53-k-kb3.cjs
// (it asserts kb3 is this file's), then node Tools/night/n53-k-kb3b.cjs (the free-standing ladle), node Tools/night/n53-k-kb3c.cjs (the
// reviews' cut), then levelgen and --finish as above.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n47/kiln.plan.json", PLAN_OUT = "Design/levels/n52/kiln.plan.json";
const GEN_OUT = "Design/levels/n52/kiln.json";
const OUTS = ["Design/levels/n52-kiln.json", "Assets/StreamingAssets/Design/levels/n52-kiln.json"];
const GENERATED_BY = "Tools/night/gen-n52-kiln.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";

// ---------------------------------------------------------------- the finish step (after levelgen): the game's two copies
if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n52-kiln") throw new Error(`${src}: name ${level.name}, expected n52-kiln`);
  level.data.name = "n40-kiln"; // the older systems' name for level 3 (as n47-kiln's: MapModelJson reads the outer name first)
  level.generatedBy = GENERATED_BY;
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  for (const f of OUTS) fs.writeFileSync(path.join(ROOT, f), out);
  console.log(`n52-kiln: finished ${path.relative(ROOT, src)} -> ${OUTS.join(", ")} (${out.length} bytes)`);
  process.exit(0);
}

// ---------------------------------------------------------------- the check: n52-kiln.json against n47-kiln.json (the pipeline's proof)
if (process.argv[2] === "--check") {
  const a = fs.readFileSync(path.join(ROOT, "Design/levels/n47-kiln.json"), "utf8");
  const b = fs.readFileSync(path.join(ROOT, OUTS[0]), "utf8");
  const b2 = fs.readFileSync(path.join(ROOT, OUTS[1]), "utf8");
  if (b !== b2) throw new Error("the two n52-kiln.json copies differ");
  const ja = JSON.parse(a), jb = JSON.parse(b);
  const names = { name: ["n47-kiln", "n52-kiln"], generatedBy: [ja.generatedBy, GENERATED_BY] };
  const diffs = [];
  for (const k of new Set([...Object.keys(ja), ...Object.keys(jb)])) {
    if (names[k]) {
      if (ja[k] !== names[k][0] || jb[k] !== names[k][1]) diffs.push(`${k}: ${JSON.stringify(ja[k])} -> ${JSON.stringify(jb[k])} (not the expected name)`);
      continue;
    }
    if (JSON.stringify(ja[k]) !== JSON.stringify(jb[k])) diffs.push(k);
  }
  // byte check: n47's text with its two names swapped for n52's must be n52's text (the line end normalized: git's autocrlf checks a level
  // file out with "\r\n", levelgen writes "\n"; the committed blobs are "\n" both)
  const lf = (s) => s.replace(/\r\n$/, "\n");
  const swapped = JSON.stringify(Object.assign({}, ja, { name: "n52-kiln", generatedBy: GENERATED_BY })) + (/\r?\n$/.test(a) ? "\n" : "");
  const same = swapped === lf(b);
  console.log(`n52-kiln --check: copies identical (${b.length} bytes); keys differing from n47-kiln.json beyond the names: ${diffs.length ? diffs.join(", ") : "none"}; `
    + `byte-equal after the name swap (line end normalized): ${same ? "YES" : "NO"}`);
  if (diffs.length || !same) process.exit(1);
  process.exit(0);
}

// ---------------------------------------------------------------- the plan
const text = fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n47-kiln") throw new Error(`${PLAN_IN}: id ${plan.id}`);
plan.id = "n52-kiln";
plan._why = "NIGHT52 K (row kiln52; Tools/night/gen-n52-kiln.cjs): n47-kiln's plan with the combat rooms rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2). Before it: " + plan._why;

// The rooms rebuilt (each by its own function below, from its A-9 card in docs/ROOMS.md, "NIGHT52 K") and the links re-cut to meet a moved door.
// The default ships kb2, kb2c and kb3. kb3 THE MOULD LINE missed THE ROOM CONTRACT's dwell item under D-15 after six tries in round 2
// (isolated dwell 1.05 -> 1.90 s, limit 1.55) and went back to n47's (the fallback, 9e8eda0f); round 5 (T-119) found the cause with
// hold52 --dwelltrace (the walk pilot backed straight off the pack onto the door from a spawn on the door's axis) and moved the retry
// spawn off the axis: dwell 1.05 -> 0.80 s, C 14/14. `--rooms=kb2,kb2c` builds the fallback (kb3, d6 and kb3b n47's, asserted).
const ONLY = (process.argv.find((a) => a.startsWith("--rooms=")) || "--rooms=kb2,kb2c,kb3").slice(8).split(",").filter(Boolean);
const REBUILT = [];
const RECUT = [];
const CARD = "docs/ROOMS.md, NIGHT52 K identity cards";
const room = (id) => {
  const r = plan.rooms.find((x) => x.id === id);
  if (!r) throw new Error("no room " + id);
  return r;
};
const door = (id) => {
  const d = plan.doors.find((x) => x.id === id);
  if (!d) throw new Error("no door " + id);
  return d;
};
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (pred, what) => {
  const i = plan.boxes.findIndex(pred);
  if (i < 0 || plan.boxes.findIndex((b, j) => j !== i && pred(b)) >= 0) throw new Error("expected exactly one " + what);
  return plan.boxes[i];
};
// a room's rows replaced in each list, at the place its first old row had (every other row keeps its place: the guard below compares them)
function swap(key, isMine, fresh) {
  const arr = plan[key] || [];
  const at = arr.findIndex(isMine);
  const rest = arr.filter((x) => !isMine(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  plan[key] = rest;
}
function rebuild(Z, rows) {
  const mineZ = (b) => b.zone === Z;
  const mineR = (b) => b.room === Z;
  swap("boxes", mineZ, rows.boxes);
  swap("ramps", mineZ, rows.ramps);
  swap("pads", mineZ, rows.pads);
  swap("pits", mineZ, rows.pits);
  swap("tunnels", mineZ, rows.tunnels);
  swap("props", mineR, rows.props);
  swap("lights", mineR, rows.lights);
  REBUILT.push(Z);
}
const piece = (Z) => ({
  box: (kind, min, max, job, why, extra) => Object.assign({ zone: Z, kind, min, max, job, why, note: why }, extra || {}),
  ramp: (from, to, width, rise, why) => ({ zone: Z, from, to, width, rise, job: "climb", why, note: why }),
});

// ================================================================ kb2 3.2 POUR HALL as THE TIPPED CRUCIBLE (card: docs/ROOMS.md, NIGHT52 K)
// n47: 36 x 44 x 16 m, 7 pieces (0.44 per 100 m2), two 4 x 22 m pours. Now 24 x 28 x 14 m (x -12..12, z -103.5..-75.5): the entry door d2
// stays; the exit door d3 moves 16 m south to z -103.5 and kb2b is re-cut to meet it (its entry floor 16 m longer, its liners extended).
// Tiers: floor 0; A 2.5 m (the spill galleries, the north-east lip); B 4.6 m (the crucible and its spout). The crucible tips east: its spill
// is THE RUNNER, the one lava channel, along the east wall, with the runner wall (the ride) standing in it.
function kb2() {
  const Z = "kb2", P = piece(Z);
  const r = room(Z);
  if (!eq(r.rect, [-18, -119.5, 18, -75.5])) throw new Error("kb2 is not n47's");
  r.rect = [-12, -103.5, 12, -75.5];
  r.height = 14;
  r.exit = [0, 0, -101.5];
  // NIGHT52 K, the midpoint review (round 4): n47's retry spawn (0, 0, -77.5) sat exactly on the 4 m dwell band round the door d2 (0, -73.5)
  // (as iceline g7's and g8's did); with the posts gone and the runner wall to the roof the isolated dwell went 1.35 -> 2.30 s (FAIL, grace 0.9).
  // 1.5 m further in: 5.5 m from the door's centre, outside the lesson's rect (z -80.5), 12.1 m from the nearest node in view (spawn31)
  r.spawn = { at: [0, 0, -79], yawDeg: 0 };
  r.spawn_why = "NIGHT52 K (round 4): the retry point 3.5 m in from the door wall (outside the door's 4 m dwell band): a death never restarts the fight in the doorway";
  r.verb36 = "the boost OUT of the pack onto the crucible (4.6 m); the runner wall's ride over the spill; the slide through the tap hole; the drones over the runner, the charger baited into a post";
  r.nodes = [[-6, 0, -89.5], [7, 0, -93], [-6, 0, -96], [6, 0, -93.5], [0.5, 0, -95.5], [2.5, 0, -100], [-5.5, 0, -102]];
  r.highNodes = [[-10, 2.5, -85], [-10, 2.5, -95], [9, 2.5, -101.5], [2.5, 4.6, -91.5]]; // round 4: the crucible node 2 m north (12.7 m from the moved spawn: spawn31)
  r.nodes_why = "NIGHT52 K (" + CARD + ", kb2): the pack in the two lanes at the crucible's flanks (it comes round to you at the south face: wave 1's boost lesson), the north lane behind the crucible, the galleries, the lip and the crucible (R-4); every node in the entry's view cone at 12 m or more (the report's spawn31), none within 6 m of the door";
  r.rims36 = { rects: [[8, -99.5, 8.5, -79.5]], why: "NIGHT52 K: the runner's west rim: the east lane's walkers pass along it; a blast or a punch puts them in" };
  const lesson = { promptPad: "JUMP IN. SHOOT {fire} DOWN. FLY.", kind: "reached", rect: [-4, -84.5, 4, -80.5], minY: -0.2, prompt: "JUMP IN. SHOOT {fire} DOWN. FLY." };
  r.lessons = [lesson];
  r.lessons31 = [Object.assign({}, lesson)];
  r.proofs = [
    { kind: "walk", points: [[0, 0, -78], [-6, 0, -84], [-6, 0, -97], [-5, 0, -102], [0, 0, -102], [4.5, 0, -99], [7, 0, -93], [7, 0, -86], [0, 0, -78]] },
    { kind: "walk", points: [[-2.5, 0, -102.3], [-2.5, 4.6, -91.5], [2.5, 4.6, -89.5]] },
    { kind: "walk", points: [[-10, 0, -76.6], [-10, 2.5, -83.5], [-10, 2.5, -88]] },
    { kind: "walk", points: [[7.25, 0, -93.5], [7.25, 2.5, -100.5], [9.5, 2.5, -102]] },
    { kind: "walk", points: [[-6, 0, -102.5], [-10, 0, -102.6], [-10, 2.5, -95]] },
  ];
  r.frames36 = { mid: [0, 0, -77.5, 0, 0.15], exit: [0, 4.6, -91.5, 0, -0.3] };
  r.frames52 = { _why: "NIGHT52 the room frames reel a452: the door just crossed (the crucible and the pack framed by the two posts), the fight from the apron's east corner, the crucible's top", entry: [0, 0, -77, 0, 0.06], mid: [6, 0, -79, 40, 0.02], high: [0, 4.6, -91.5, 180, -0.25] };
  r.kiln52_why = "NIGHT52 K (Tools/night/gen-n52-kiln.cjs; " + CARD + ", kb2 THE TIPPED CRUCIBLE): n47's 36 x 44 m POUR HALL rebuilt to THE ROOM CONTRACT: 24 x 28 x 14 m, 17 pieces, three tiers (0 / 2.5 / 4.6 m) each on a walkers' ramp, the runner wall (an 18 m ride over the spill), the bellows (a pad onto the crucible), the tap hole (a slide slot under it); the waves, the lesson, the worm's pass (kiln47's beat 2: by room id) kept.";
  const boxes = [
    P.box("deck", [-4, 0, -92.5], [-1, 4.6, -86.5], "line", "THE CRUCIBLE, west half (4.6 m; the room's mass, the circuit runs round it): the boost from the pack at its south foot lands on its lip (jump + THUNDERCLAP down: over every non-boost apex); the charger cannot follow you up; the husks come up its cradle ramp (no safe ledge)"),
    P.box("deck", [1, 0, -92.5], [4, 4.6, -86.5], "line", "THE CRUCIBLE, east half (4.6 m): the boost's other landing; the bellows (the pad) puts you back on it; the drones circle its lip at your height"),
    P.box("deck", [-1, 3.2, -92.5], [1, 4.6, -86.5], "line", "THE CRUCIBLE's lid over THE TAP HOLE (4.6 m top, the slot 1.2 m under it): the crucible's top stays one floor for the walkers while you slide through underneath"),
    P.box("deck", [-12, 0, -89], [-8, 2.5, -82.5], "line", "THE SPILL GALLERY, south (2.5 m, the founders' walk on the west wall): a double jump from it reaches the crucible's lip over the 4 m west lane; the drones dive past it; the west ride starts on it"),
    P.box("deck", [-12, 0, -96.5], [-8, 2.5, -92], "line", "THE SPILL GALLERY, north (2.5 m): the walk the worm broke (a 3 m gap to the south gallery: the bellows sit in it): ride the back wall across the gap or drop and take the pad"),
    P.box("wall", [-12, 2.5, -96.5], [-11.4, 9, -82.5], "line", "THE GALLERY WALL (ride, cyan, 14 m, 2.5-9 m on the west wall): ride it along the galleries at the drones' height and over the broken walk's gap; jump off its end onto the crucible or down into the pack", { ride: true, ride36: { face: "E", planeX: -11.4, lz: [7, 21] } }),
    P.box("wall", [11, -0.8, -97.5], [12, 14, -79.5], "line", "THE RUNNER WALL (ride, cyan, 18 m standing in the runner's lava, to the roof since the midpoint review: the flue ledge hangs off its face, and its 6 m top under the ledge was a 0-hit perch): ride it north over the molten spill from the apron's east corner, out of the pack and the charger's line; jump off its end onto the north-east lip (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "W", planeX: 11, lz: [4, 22] } }),
    P.box("deck", [6, 0, -103.5], [12, 2.5, -99.5], "line", "THE NORTH-EAST LIP (2.5 m, where the runner's spill ends): the runner wall's landing (a jump off the ride's end); a stand level with the drones over the runner; the walkers come up its ramp (no safe ledge)"),
    // NIGHT52 K, the midpoint review (round 4): the room's lesson (SHOOT DOWN. FLY) landed nowhere: THE FLUE LEDGE hangs on the east wall at 8 m (north of the crucible:
    // over the runner, 3.4 m over the crucible and 6.5 m out from it: a double jump (2.8 m) does not reach it, the shot-jump off the crucible does;
    // off it you drop into a ride down the runner wall (now to the roof). The two posts beside the spawn lane are cut (they served the lane nothing).
    P.box("hull", [9.5, 7.4, -98.5], [11, 8, -94], "line", "THE FLUE LEDGE (8 m, 1.5 x 4.5 m off the runner wall's face over the lava, north of the crucible; a perch for the shot-jump, not a walkers' tier: no ramp, the drones reach it): the shot-jump's landing (SHOOT DOWN. FLY: off the crucible, look down, fire, fly ~7 m north-east and 3.4 m up); drop off it into a ride down the runner wall's face; the drones' height"),
    P.box("deck", [-4, 0, -86.5], [-1.5, 0.7, -84.5], "line", "THE SLAG HEAP, west (0.7 m, against the crucible's south face beside the tap hole): the small boost (jump, look down, fire) from it reaches the lip; the big boost from the floor; the step that turns the pack's corner into a launch (T-5: small, then big)"),
    P.box("deck", [1.5, 0, -86.5], [4, 0.7, -84.5], "line", "THE SLAG HEAP, east (0.7 m): the other launch step for the boost onto the crucible; the tap hole's mouth between the two heaps (slide in from the pack)"),
  ];
  const ramps = [
    P.ramp([-2.5, -101], [-2.5, -92.5], 3, 4.6, "THE CRADLE RAMP (4.6 m over 8.5 m) up the crucible's north face: the walkers' climb to you on the high tier (no safe ledge, A12; GG: a route enemies can climb); the slow way up for a player, through them"),
    P.ramp([-10, -77.5], [-10, -82.5], 3, 2.5, "the south gallery's walkers' ramp (2.5 m over 5 m, its foot 2 m off the south wall: no slot) from the apron's west corner: the husks' way onto the founders' walk; yours with the pack behind you"),
    P.ramp([-10, -101.5], [-10, -96.5], 3, 2.5, "the north gallery's walkers' ramp (2.5 m over 5 m, its foot 2 m off the north wall: no slot) from the north-west corner: the climb onto the broken walk's far half"),
    P.ramp([7.25, -94.5], [7.25, -99.5], 2.5, 2.5, "the north-east lip's walkers' ramp (2.5 m over 5 m) beside the runner: the walkers follow you onto the lip (no safe ledge)"),
  ];
  const pads = [
    { zone: Z, at: [-10, 0, -90.5], target: [-2.5, 4.6, -89.5], apexAbove: 2, job: "line", why: "THE BELLOWS (a jump pad, the upward line, A5): the kiln's bellows vent in the floor of the broken walk's gap blasts you east over the west lane onto the crucible (4.6 m): knocked off, you are back on top in one launch", note: "the bellows" },
    // NIGHT52 K, the midpoint review (round 4): the spout's wedge ran 2 m up the crucible's east face and stopped 2.2 m under its lip (a ramp to
    // nowhere): a pad in its place, 1.5 m off the east face (the east lane's walk and the health clear of its 1 m trigger)
    { zone: Z, at: [5.5, 0, -89.5], target: [2.5, 4.6, -89.5], apexAbove: 2, job: "line", why: "THE SPOUT (a jump pad where the tipped crucible's spout pours into the east lane): from the east lane onto the crucible's lip (4.6 m), the second way up beside the bellows", note: "the spout" },
  ];
  const pits = [
    { zone: Z, rect: [8.5, -99.5, 12, -79.5], floorY: -0.8, lava: true, job: "hazard", why: "THE RUNNER (3.5 x 20 m, lava 0.8 m down, the crucible's spill down the east wall, the runner wall standing in it): both ways (A-5): it burns and knocks you up; a blast or a punch from the east lane puts the walkers in; a charger that misses you at its rim goes in", note: "the runner" },
  ];
  const tunnels = [
    { zone: Z, rect: [-1, -92.5, 1, -86.5], ceiling: 1.2, job: "line", why: "THE TAP HOLE (2 x 6 m, 1.2 m high, through the crucible's middle): slide from the pack's corner to the north lane under them (the loop's cut-through, A-2); swarmers follow you through, husks go round", note: "the tap hole" },
  ];
  const props = [
    { id: "kb2-P1", room: Z, kind: "healthPickup", at: [8, 0.9, -90], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health on the east lane's floor at the runner's edge, under the sloped spout (the review: the risky leg of the loop; A20: the best at the riskiest spot): the pack, the drones over the runner and the lava beside it" },
  ];
  const lights = [
    { id: "kb2-L1", room: Z, kind: "ceiling", at: [0, 13.95, -80.5], normal: [0, -1, 0], size: [4, 4, 0.1], color: "white", range: 12, intensity: 0.9 },
    { id: "kb2-L2", room: Z, kind: "ceiling", at: [0, 13.95, -89.5], normal: [0, -1, 0], size: [6, 4, 0.1], color: "white", range: 12, intensity: 1.1 },
    { id: "kb2-L3", room: Z, kind: "ceiling", at: [0, 13.95, -99.5], normal: [0, -1, 0], size: [4, 4, 0.1], color: "white", range: 12, intensity: 1 },
    { id: "kb2-L4", room: Z, kind: "pool", at: [0, 4.625, -89.5], normal: [0, 1, 0], size: [3, 3, 0.05], color: "white", range: 9, intensity: 1 },
    { id: "kb2-L5", room: Z, kind: "pool", at: [10.25, -0.775, -89.5], normal: [0, 1, 0], size: [2.5, 6, 0.05], color: "orange", range: 9, intensity: 1.4, danger: true },
    { id: "kb2-L6", room: Z, kind: "pool", at: [-10, 2.525, -85], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "white", range: 7, intensity: 0.9 },
    { id: "kb2-L7", room: Z, kind: "pool", at: [0, 0.025, -101.5], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.7 },
  ];
  rebuild(Z, { boxes, ramps, pads, pits, tunnels, props, lights });
  // d3 moves with kb2's north wall; kb2b is re-cut to meet it (its lesson, cliff, gate and run-up panel untouched)
  const d3 = door("d3");
  if (!eq(d3.rect, [-2, -123.5, 2, -119.5])) throw new Error("d3 is not n47's");
  d3.rect = [-2, -107.5, 2, -103.5];
  recutLinkEntry("kb2b", -123.5, -107.5);
}

// A link's entry edge (its rect's z1) moved to meet a moved door: the rect grows, the spawn and the entry frame follow the door, the noride
// liners that ran to the old edge run to the new one (no new ride face: the link's climb stays its lesson), one ceiling light over the new floor.
function recutLinkEntry(Z, oldZ1, newZ1) {
  const r = room(Z);
  if (r.kind !== "link" || r.rect[3] !== oldZ1) throw new Error(Z + ": not the n47 link");
  const dz = newZ1 - oldZ1;
  r.rect = [r.rect[0], r.rect[1], r.rect[2], newZ1];
  r.spawn = Object.assign({}, r.spawn, { at: [r.spawn.at[0], r.spawn.at[1], r.spawn.at[2] + dz] });
  if (r.frames36 && r.frames36.mid) r.frames36 = Object.assign({}, r.frames36, { mid: [r.frames36.mid[0], r.frames36.mid[1], r.frames36.mid[2] + dz, r.frames36.mid[3], r.frames36.mid[4]] });
  r.recut52_why = `NIGHT52 K: the entry edge moved ${dz} m (z ${oldZ1} -> ${newZ1}) to meet the moved door (the room before it shrank to THE ROOM CONTRACT's size); the spawn and the entry frame follow the door; the liners run to the new edge; the climb, its gate, the run-up panel and the lesson are n47's`;
  let n = 0;
  for (const b of plan.boxes) {
    if (b.zone !== Z || b.max[2] !== oldZ1) continue;
    if (!(b.noride && b.kind === "hull")) throw new Error(Z + ": a non-liner box reaches the old edge: " + JSON.stringify(b));
    b.max = [b.max[0], b.max[1], newZ1];
    b.why = b.why + ` (NIGHT52 K: re-cut to the moved door at z ${newZ1})`;
    b.note = b.why;
    n++;
  }
  // the east liner above the run-up panel stops at the old jamb (z oldZ1 - 2): it runs to the new edge too (above the jamb)
  for (const b of plan.boxes) {
    if (b.zone !== Z || !(b.noride && b.kind === "hull") || b.max[2] !== oldZ1 - 2 || b.min[1] < 5.9) continue;
    b.max = [b.max[0], b.max[1], newZ1];
    b.why = b.why + ` (NIGHT52 K: and over the moved jamb to z ${newZ1})`;
    b.note = b.why;
    n++;
  }
  if (n < 3) throw new Error(Z + ": expected the west liner, the east jamb and the east liner above the panel, found " + n);
  plan.lights.push({ id: Z + "-L52", room: Z, kind: "ceiling", at: [0, r.height - 0.05, (oldZ1 + newZ1) / 2], normal: [0, -1, 0], size: [3, 2, 0.1], color: "white", range: 14, intensity: 1, why: "NIGHT52 K: the re-cut entry floor's light" });
  RECUT.push(Z);
}

// ================================================================ kb2c 3.2c THE ASH BED as THE ASH BED (card: docs/ROOMS.md, NIGHT52 K)
// n47: 28 x 30 x 12 m, 5 plates of 0.4 m (0.60 per 100 m2), one plane. Now 20 x 30 x 12 m (x -10..10; the doors unchanged): the open ash is
// the burrower's ground; tiers A 2.5 m (THE SLAG STACK in the middle, the east ledge) and B 4.5 m (THE CLINKER LOFT), each on a walkers' ramp;
// four metal plates (burrow-safe); the cooling wall (the ride) off the ash; the ash vent (a pad) onto the loft.
function kb2c() {
  const Z = "kb2c", P = piece(Z);
  const r = room(Z);
  if (!eq(r.rect, [-14, -193.5, 14, -163.5])) throw new Error("kb2c is not n47's");
  r.rect = [-10, -193.5, 10, -163.5];
  r.verb36 = "the BURROWER: leave its ash (a plate, the slag stack, the loft, the cooling wall's ride) or hit it in its counter window; the swarmers follow you up, the stokers come down";
  // the spawn 5 m in from the door (n47: 2 m): you start on the open ash, not in the doorway (the entrance gated); the walk pilot's home is
  // the spawn, and at 2 m it fought the swarmers in the doorway (n46flow dwell 2.9 s against n47's 1.1)
  r.spawn = { at: [0, 0, -168.5], yawDeg: 0 };
  r.spawn_why = "NIGHT52 K: 5 m in from the door d4 (n47: 2 m): a retry starts you on the burrower's ash, not in the doorway; the commit gate (3 m from here) brings wave 1";
  const ground = [[-5.5, 0, -180], [8, 0, -178.5], [8, 0, -183], [3, 0, -190], [-0.5, 0, -189]]; // every one in the entry's view cone at 12 m or more from the spawn (spawn31): the open ash in front of the door is the hunt's ground, not a spawn
  const high = [[-1, 2.5, -181], [-7.5, 4.5, -188], [8, 2.5, -191.5]];
  r.nodes = ground;
  r.highNodes = high;
  r.waves = [["burrower", "swarmer", "swarmer"], ["husk", "husk", "husk"]];
  r.roles43 = ["burrower + fodder (the swarmers follow you off the ash)", "fodder from the tiers (the stokers come down)"];
  r.waves_why = "NIGHT52 K (" + CARD + ", kb2c): the burrower comes with two swarmers: it can only hit a player grounded on its ash (Burrower43: its floor is its spawn's), so a lone burrower made every tier a 0-hit camping spot (THE ROOM CONTRACT 7, the hold probe); the swarmers follow you up the ramps. Wave 2 (three husks) is released by overlap46 when the swarmers are dead and the burrower lives, from the tiers: they walk you off the plates and the stack onto its ash (n43's w2 problem kept)";
  r.waveNodes43 = [ground, high];
  r.waveNodes43_why = "NIGHT52 K: wave 1 (the burrower) only on the ash: a burrower hunts on its spawn's height and would never surface from a tier; wave 2 (the stokers) on the three tiers, 13-25 m from the door (overlap46's entryM 12)";
  r.proofs = [
    { kind: "walk", points: [[0, 0, -165.5], [-5.5, 0, -174], [-4, 0, -183], [-3.5, 0, -186], [-1, 0, -188.8], [0.5, 0, -189], [4, 0, -188], [4, 0, -183], [8.4, 0, -182.5], [8.4, 0, -174], [3.8, 0, -174.5], [3.8, 0, -172], [0, 0, -166]] },
    { kind: "walk", points: [[0, 0, -189], [0, 2.5, -181], [0, 2.5, -177]] },
    { kind: "walk", points: [[-8.5, 0, -174], [-8.5, 4.5, -185], [-7, 4.5, -189]] },
    { kind: "walk", points: [[7.2, 0, -183.5], [7.2, 2.5, -190.5], [8.5, 2.5, -192]] },
  ];
  r.frames36 = { mid: [0, 0, -166.5, 0, -0.096], exit: [0, 2.5, -180, 0, -0.12] };
  r.frames52 = { _why: "NIGHT52 the room frames reel a452: the door just crossed (the open ash, the slag stack, the loft behind), the fight from the south-west plate, the loft's top over the ash", entry: [0, 0, -165, 0, 0.02], mid: [-5.5, 0, -169, -30, 0.05], high: [-7.5, 4.5, -188, -155, -0.3] };
  r.kiln52_why = "NIGHT52 K (Tools/night/gen-n52-kiln.cjs; " + CARD + ", kb2c THE ASH BED): n47's 28 x 30 m ash bed rebuilt to THE ROOM CONTRACT: 20 x 30 x 12 m, 16 pieces, three tiers (0 / 2.5 / 4.5 m) each on a walkers' ramp, four burrow-safe plates, the cooling wall (a 25 m ride off the ash), the ash vent (a pad onto the loft); the burrower's lesson kept (leave its ground or hit it in the window).";
  const plate = (min, max, where) => P.box("deck", min, max, "cover", `a METAL PLATE (${where}, 0.4 m): burrow-safe ground (it cannot surface through metal: A1's rule): the island you hop to when the ash ripples; the burrower's tell under you is answered by one hop`, { burrowSafe43: true });
  const boxes = [
    P.box("deck", [-3, 0, -182], [3, 2.5, -175], "line", "THE SLAG STACK (2.5 m, 6 x 7 m, the room's middle: the loop runs round it): off the ash in one double jump; the swarmers follow you up its ramp (no safe ledge); the stokers of wave 2 come down off it"),
    P.box("hull", [1.5, 2.5, -182], [3, 12, -180.5], "cover", "THE STACK'S FLUE (on the stack's north-east corner, to the roof): the pivot you circle on the stack top while the swarmers come up; it cuts the loft's line across the stack to the ledge"),
    P.box("deck", [-10, 0, -191.5], [-4.5, 4.5, -184], "line", "THE CLINKER LOFT (4.5 m, the third tier, the north-west corner): the high ground over the ash: the ash vent (the pad), the wall-jump chimney or the small boost off the spoil heap puts you up; the walkers follow up its long ramp (no safe ledge)"),
    P.box("hull", [-3, 0, -191.5], [-1.5, 12, -190], "cover", "THE NORTH FLUE (floor to roof): a 1.5 m chimney against the loft's east face: wall-jump up it onto the loft (the second verb onto the top tier); it hides the exit's approach from the loft"),
    P.box("deck", [-7, 0, -184], [-4.5, 0.9, -182], "line", "THE SPOIL HEAP (0.9 m, at the loft's south face): the small boost (jump, look down, fire) from it reaches the loft; a step out of the ash at the loft's foot (T-5: small, then big)"),
    // NIGHT52 K (verbs37 n52-r10 row 9 RED: rides 0.93 s, ends `away`): the wall ended 6 m short of the room's south wall, where its face stepped
    // 0.6 m back to the room's east wall (x 10): a ride from the middle south lost the wall there. Now it runs into the south-east corner: one face.
    P.box("wall", [9.4, 0, -188.5], [10, 6, -163.5], "line", "THE COOLING WALL (ride, cyan, 25 m x 6 m on the east wall, into the south-east corner: one face, no step back): ride it the length of the room off the ash (the burrower cannot track a rider); jump off its end onto the east ledge (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "W", planeX: 9.4, lz: [0, 25] } }),
    P.box("deck", [5, 0, -193.5], [10, 2.5, -189.5], "line", "THE EAST LEDGE (2.5 m, the north-east corner): the cooling wall's landing; a stand over the north ash; the walkers come up its ramp (no safe ledge)"),
    plate([-9.5, 0, -173], [-6, 0.4, -169.5], "south-west, off the spawn's line (the review)"),
    plate([6, 0, -173], [9.4, 0.4, -169.5], "south-east, at the cooling wall's foot (the ride's mount), off the spawn's line (the review)"),
    plate([4, 0, -181.5], [7.5, 0.4, -178], "east lane"),
    plate([-4.5, 0, -190], [-2.2, 0.4, -187.8], "north-west, the chimney's footing"),
  ];
  const ramps = [
    P.ramp([0, -187], [0, -182], 3, 2.5, "the slag stack's walkers' ramp (2.5 m over 5 m) up its north face: the swarmers' climb to you (no safe ledge); the stokers' way down"),
    P.ramp([-8.5, -175], [-8.5, -184], 3, 4.5, "THE LOFT RAMP (4.5 m over 9 m) along the west wall: the walkers' climb onto the third tier (no safe ledge, A12; GG: a route enemies can climb)"),
    P.ramp([7.2, -184.5], [7.2, -189.5], 4.4, 2.5, "the east ledge's walkers' ramp (2.5 m over 5 m, 4.4 m wide, flush to the cooling wall; the review: no slot beside it): the walkers follow you onto the ledge"),
  ];
  const pads = [
    { zone: Z, at: [-5, 0, -177.5], target: [-7, 4.5, -188], apexAbove: 2.5, job: "line", why: "THE ASH VENT (a jump pad, the upward line, A5): the kiln's draught through the ash in the west lane blasts you over the spoil heap onto the clinker loft (4.5 m): off the burrower's ash in one launch", note: "the ash vent" },
  ];
  const props = [
    { id: "kb2c-P1", room: Z, kind: "healthPickup", at: [-7.5, 5.4, -189], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health on THE CLINKER LOFT (4.5 m, the third tier; the midpoint review, round 4: near the spawn it was free): the climb (the ash vent, the chimney, the boost off the spoil heap, the loft ramp) is its price, the stokers of wave 2 stand on the tiers round it" },
  ];
  const lights = [
    { id: "kb2c-L1", room: Z, kind: "pool", at: [-7.75, 0.425, -171.25], normal: [0, 1, 0], size: [1.6, 1.6, 0.05], color: "cyan", range: 5, intensity: 1 },
    { id: "kb2c-L2", room: Z, kind: "pool", at: [7.7, 0.425, -171.25], normal: [0, 1, 0], size: [1.6, 1.6, 0.05], color: "cyan", range: 5, intensity: 1 },
    { id: "kb2c-L3", room: Z, kind: "pool", at: [5.75, 0.425, -179.75], normal: [0, 1, 0], size: [1.6, 1.6, 0.05], color: "cyan", range: 5, intensity: 1 },
    { id: "kb2c-L4", room: Z, kind: "pool", at: [-3.35, 0.425, -188.9], normal: [0, 1, 0], size: [1.4, 1.4, 0.05], color: "cyan", range: 5, intensity: 1 },
    { id: "kb2c-L5", room: Z, kind: "pool", at: [-7.25, 4.525, -187.75], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "white", range: 7, intensity: 0.9 },
    { id: "kb2c-L6", room: Z, kind: "ceiling", at: [0, 11.95, -167.5], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 10, intensity: 1 },
    { id: "kb2c-L7", room: Z, kind: "ceiling", at: [0, 11.95, -178.5], normal: [0, -1, 0], size: [6, 4, 0.1], color: "white", range: 11, intensity: 1.1 },
    { id: "kb2c-L8", room: Z, kind: "ceiling", at: [0, 11.95, -189.5], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 10, intensity: 1 },
    { id: "kb2c-L9", room: Z, kind: "pool", at: [0, 0.025, -191.5], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6 },
  ];
  rebuild(Z, { boxes, ramps, pads, pits: [], tunnels: [], props, lights });
}

// ================================================================ kb3 3.3 CASTING FLOOR as THE MOULD LINE (card: docs/ROOMS.md, NIGHT52 K)
// n47: 40 x 46 x 18 m, 6 pieces (0.33 per 100 m2), a 16 x 10 m pour, two 9.6 m rides in it, the 8 m ladle, the 4 m gantry. Now 22 x 28 x 14 m
// (x -11..11, z -225.5..-197.5): the entry door d5 stays; d6 moves 18 m south to z -225.5 and kb3b is re-cut to meet it. The pour is THE MOULD
// LINE down the middle (4 x 20 m of lava), the two mould walls (the rides) standing in it 2.8 m apart; tiers A 2.5 m (the west gantry) and
// B 4.2 m (THE LADLE and its pour lip), each on walkers' ramps. kiln47's beat 3 (THE CAMEO) is keyed by the room id and reads only the room's
// floor height and the player's feet: unchanged (its card, the lesson "When the floor glows, jump.", is kept).
function kb3() {
  const Z = "kb3", P = piece(Z);
  const r = room(Z);
  if (!eq(r.rect, [-20, -243.5, 20, -197.5])) throw new Error("kb3 is not n47's");
  if (!r.lessons || r.lessons[0].prompt !== "When the floor glows, jump.") throw new Error("kb3's cameo card is not n47's");
  r.rect = [-11, -225.5, 11, -197.5];
  r.height = 14;
  r.exit = [0, 0, -223.5];
  r.spawn = { at: [-5.5, 0, -202], yawDeg: 0 }; // round 5 (T-119): off the door's axis (see spawn_why)
  r.spawn_why = "NIGHT52 K round 5 (T-119, hold52 --dwelltrace: the walk pilot held its home and backed straight away from the pack coming over the apron, onto the door; at (0, -202.5) a 3 m back-step put it inside the door's 4 m band, isolated dwell 1.5 / 1.95 s): the retry point 5.5 m west of the door's axis, 4.5 m in (8.5 m from the door's centre: a back-step ends at the wall beside the door, 6.3 m out). Before: 5 m in from the door d5 on the apron, 3 m short of the mould line (n47: 2 m): a retry starts you facing the mould walls, not in the doorway (the walk pilot fought from the doorway: isolated dwell 1.05 -> 3.05 s at 2 m)";
  r.verb36 = "ride the mould walls over the pour (the health hangs between them), climb to the ladle; the charger drives you off the floor into the spitters' globs (parry them back), the worm's breach under you (jump when the floor glows), then the husks under the drones";
  r.nodes = [[-4.5, 0, -214], [4, 0, -215], [-4.5, 0, -220], [6.5, 0, -219], [2.5, 0, -223.5], [-6.5, 0, -222.5]];
  r.highNodes = [[-9, 2.5, -214], [8.5, 4.2, -212], [5, 4.2, -213.5]];
  r.nodes_why = "NIGHT52 K (" + CARD + ", kb3): both banks of the mould past its middle, the north floor, the gantry, the ladle and its lip (R-4: ranged on high ground); every node in the entry's view cone at 12 m or more (spawn31), none within 6 m of the door";
  r.rims36 = { rects: [[-2.5, -222.5, -2, -205.5], [2, -222.5, 2.5, -205.5]], why: "NIGHT52 K: the mould's two banks: the walkers' lines along the mould walls; a blast or a punch over the wall's end puts them in" };
  r.proofs = [
    { kind: "walk", points: [[0, 0, -200], [-3, 0, -203], [-3, 0, -221], [0, 0, -223], [2.8, 0, -222.5], [2.8, 0, -216], [2.8, 0, -207], [2.8, 0, -203], [0, 0, -200]] },
    { kind: "walk", points: [[8.5, 4.2, -212], [9.5, 4.2, -216.5], [9.5, 0, -224.4]] },
    { kind: "walk", points: [[-9.5, 0, -224.2], [-9.5, 2.5, -218], [-9, 2.5, -210]] },
  ];
  r.frames36 = { mid: [0, 0, -200, 0, 0.1], exit: [8.5, 4.2, -214, 0, -0.3] };
  r.frames52 = { _why: "NIGHT52 the room frames reel a452: the door just crossed (the mould line, its two walls and the ladle beyond), the fight from the blast vent's corner, the ladle's top", entry: [0, 0, -199, 0, 0.05], mid: [-5, 0, -203, -35, 0.05], high: [9, 4.2, -214.5, 90, -0.04] };
  r.kiln52_why = "NIGHT52 K (Tools/night/gen-n52-kiln.cjs; " + CARD + ", kb3 THE MOULD LINE): n47's 40 x 46 m CASTING FLOOR rebuilt to THE ROOM CONTRACT: 22 x 28 x 14 m, 16 pieces, three tiers (0 / 2.5 / 4.2 m) on walkers' ramps, the mould line (lava) with its two walls (17 m rides, the health between them), the blast vent (a pad onto the ladle), four charger posts; the waves, the cameo's card and beat (by room id) kept.";
  const boxes = [
    P.box("wall", [-2, -0.8, -222.5], [-1.4, 4, -205.5], "line", "THE WEST MOULD WALL (ride, cyan, 17 m standing in the mould's lava): ride its inner face north over the pour (the health hangs between the walls); wall-jump across to the east wall (2.8 m) to climb; its outer face is the west bank's ride, out of the charger's line (17.3 m = the 1.48 s ride at 11.7 m/s)", { ride: true, ride36: { face: "E", planeX: -1.4, lz: [8, 25] } }),
    P.box("wall", [1.4, -0.8, -222.5], [2, 1, -205.5], "line", "THE MOULD's EAST LIP (1 m over the floor, standing in the lava along the east bank; the review: the east wall split the room for 17 m and hid wave 1 from the door): the channel's rim: the east bank sees across the mould; jump it into the pour's line (the health) or off the west wall's ride onto it"),
    P.box("deck", [6, 0, -216], [11, 4.2, -208], "line", "THE LADLE (4.2 m, the east bank's high stand, the hero): the spitters' roost over the mould; the boost (or the small one off the ingot stack), the blast vent, the tapping spring or its north ramp put you on it; the charger cannot follow you up (no safe ledge: the walkers take the ramps)"),
    P.box("deck", [4.5, 3.2, -214], [6, 4.2, -211], "line", "THE LADLE's POUR LIP (4.2 m, 1.5 m out over the east lane, 3.2 m clear under it; the review: trimmed from 3 m, the lane 2.5 m beside it): the ladle's mouth over the mould: shoot down into the east bank and the pour; the spitter on it lobs over the mould walls (parry its globs from the west bank)"),
    P.box("deck", [-11, 0, -219], [-7, 2.5, -205], "line", "THE WEST GANTRY (2.5 m along the west wall): the second spitter stand, level with the mould walls' tops: its globs come over the walls at the east bank; a double jump or the wall jumps up the chimney of a west post put you on it"),
    P.box("hull", [-7, 0, -210], [-5.5, 14, -208.5], "cover", "THE WEST POST, south (floor to roof, flush to the gantry's face; the review: no 1.5 m slot): the charger's stun post on the west bank (bait, side-step: it hits the post, stunned 1.5 s, back open); a wall to wall-jump off onto the gantry"),
    P.box("hull", [-7, 0, -217], [-5.5, 14, -215.5], "cover", "THE WEST POST, north (flush to the gantry's face) (floor to roof): the second stun post on the west bank; it cuts the ladle's line along the west bank (the spitters cannot hold the whole bank)"),
    P.box("hull", [4.5, 0, -206.5], [6, 14, -205], "cover", "THE EAST POST, south (2.5 m off the mould's lip) (floor to roof): the charger's stun post on the east bank's south end; it cuts the lip's line to the door (the entry is not a shooting gallery)"),
    P.box("hull", [4.5, 0, -220.5], [6, 14, -219], "cover", "THE EAST POST, north (2.5 m off the mould's lip) (floor to roof): the stun post on the east bank's north end, the pivot of the loop's north turn"),
    P.box("deck", [4.5, 0, -210.5], [6, 0.9, -208.5], "line", "THE INGOT STACK (0.9 m at the ladle's west face): the small boost (jump, look down, fire) from it reaches the ladle; the big boost from the floor (T-5: small, then big)"),
    P.box("hull", [10, 4.2, -213.5], [11, 14, -210.5], "cover", "THE LADLE's HANGER (from the ladle's top to the roof, against the east wall): the spitter's cover on the ladle: you come round it (a stand you must move to break); it keeps the ladle's top from being one open firing deck"),
    P.box("hull", [-6, 0, -198.5], [-2, 14, -197.5], "line", "the DOOR's west LINER (1 m thick, 4 m along the entry wall, floor to roof: a dull wall that can't be ridden, noride, as v7 THE LONG BELL's): no ride across the doorway, so the fight goes into the room along the mould wall (the isolated walk pilot rode the door wall: dwell)", { noride: true }),
    P.box("hull", [2, 0, -198.5], [6, 14, -197.5], "line", "the DOOR's east LINER (1 m thick, 4 m along the entry wall: noride): as the west's (the room's ride is the west mould wall)", { noride: true }),
    P.box("hull", [-11, 2.5, -213.5], [-10, 14, -210.5], "cover", "THE GANTRY's HOPPER (from the gantry's top to the roof, against the west wall): the gantry spitter's cover from the east bank: flank it up the chimney or the ramp"),
  ];
  const ramps = [
    P.ramp([9.5, -223.5], [9.5, -216], 3, 4.2, "THE LADLE's NORTH RAMP (4.2 m over 7.5 m, its foot 2 m off the north wall: no slot) from the north floor: the second way up: the ladle is never a dead end, the walkers come from both ends"),
    P.ramp([-9.5, -199.5], [-9.5, -205], 3, 2.5, "the west gantry's south ramp (2.5 m over 5.5 m, its foot 2 m off the south wall; the review: the gantry had one ramp, a 14 m dead end): the walkers' way on from the apron, the north ramp the other end"),
    P.ramp([-9.5, -223.5], [-9.5, -219], 3, 2.5, "the west gantry's walkers' ramp (2.5 m over 4.5 m, from the north floor, its foot 2 m off the north wall): the husks' way onto the gantry from the far end, so the walkers never come down beside the door; yours with the pack behind you"),
  ];
  const pads = [
    { zone: Z, at: [9.5, 0, -201.5], target: [8.5, 4.2, -209.8], apexAbove: 2, job: "line", why: "THE TAPPING SPRING (a jump pad, the upward line, A5): from the apron's east corner over the ladle's south face onto its top (4.2 m): the player's way up from the door side (the walkers' ramp is the north one)", note: "the tapping spring" },
    // NIGHT52 round 7 (D-30, the final P-rank review): the vent stood 1.58 m from the retry spawn (-5.5, 0, -202) and 0.5 m off its forward
    // line (every restart walked onto it and was thrown onto the ladle): 2 m west, its 1 m disc to the gantry ramp's east edge (x -8), 1.5 m
    // off the spawn's line; its target and job unchanged
    { zone: Z, at: [-7, 0, -203.5], target: [8.5, 4.2, -212], apexAbove: 3, job: "line", why: "THE BLAST VENT (a jump pad, the upward line, A5): from the apron's west corner over both mould walls onto the ladle (4.2 m): straight into the spitters' roost, the aggressive way in", note: "the blast vent" },
  ];
  const pits = [
    { zone: Z, rect: [-2, -222.5, 2, -205.5], floorY: -0.8, lava: true, job: "hazard", why: "THE MOULD LINE (4 x 17 m, 8 m in from the door past the apron: the loop's south crossing out of the doorway; lava 0.8 m down, the two mould walls standing in it): both ways (A-5): it burns you and knocks you up (a missed ride); a blast or a punch over a wall's end puts the walkers in; the charger that misses you at its end goes in", note: "the mould line" },
  ];
  const props = [
    { id: "kb3-P1", room: Z, kind: "healthPickup", at: [0, 0.6, -214], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health hangs over the mould between the two walls, 1.4 m over the lava (A20: the best at the riskiest spot; the report's pickup rule): ride a wall's inner face low or jump across the pour to take it (feet within 1.2 m of the floor's height: design.n29.pickups reach)" },
  ];
  const lights = [
    { id: "kb3-L1", room: Z, kind: "ceiling", at: [0, 13.95, -201.5], normal: [0, -1, 0], size: [4, 4, 0.1], color: "white", range: 12, intensity: 0.9 },
    { id: "kb3-L2", room: Z, kind: "ceiling", at: [0, 13.95, -211.5], normal: [0, -1, 0], size: [6, 4, 0.1], color: "white", range: 13, intensity: 1.1 },
    { id: "kb3-L3", room: Z, kind: "ceiling", at: [0, 13.95, -221.5], normal: [0, -1, 0], size: [4, 4, 0.1], color: "white", range: 12, intensity: 1 },
    { id: "kb3-L4", room: Z, kind: "pool", at: [0, -0.775, -214], normal: [0, 1, 0], size: [2.5, 14, 0.05], color: "orange", range: 10, intensity: 1.4, danger: true },
    { id: "kb3-L5", room: Z, kind: "pool", at: [8.5, 4.225, -212], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "white", range: 8, intensity: 0.9 },
    { id: "kb3-L6", room: Z, kind: "pool", at: [-9, 2.525, -212], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "white", range: 8, intensity: 0.9 },
    { id: "kb3-L7", room: Z, kind: "pool", at: [0, 0.025, -224.2], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.7 },
  ];
  rebuild(Z, { boxes, ramps, pads, pits, tunnels: [], props, lights });
  const d6 = door("d6");
  if (!eq(d6.rect, [-2, -247.5, 2, -243.5])) throw new Error("d6 is not n47's");
  d6.rect = [-2, -229.5, 2, -225.5];
  recutLinkEntry("kb3b", -247.5, -229.5);
}

const BUILD = { kb2, kb2c, kb3 };
for (const id of ONLY) {
  if (!BUILD[id]) throw new Error("no rebuild for " + id);
  BUILD[id]();
}

// ---------------------------------------------------------------- asserts: every other room is n47's bit for bit
const CHANGED = new Set([...REBUILT, ...RECUT]);
for (let i = 0; i < before.rooms.length; i++) {
  const id = before.rooms[i].id;
  const now = plan.rooms.find((r) => r.id === id);
  if (!now) throw new Error("room lost: " + id);
  if (!CHANGED.has(id) && JSON.stringify(now) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + id);
}
if (JSON.stringify(plan.rooms.map((r) => r.id)) !== JSON.stringify(before.rooms.map((r) => r.id))) throw new Error("the room order changed");
const zoneOf = (o) => o.zone || o.room;
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (list) => (list || []).filter((o) => !CHANGED.has(zoneOf(o))).map((o) => JSON.stringify(o));
  if (JSON.stringify(keep(before[k])) !== JSON.stringify(keep(plan[k]))) throw new Error(`${k}: a row of a room not rebuilt changed`);
}
for (const k of Object.keys(before)) if (!["_why", "id", "rooms", "doors", "boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
for (const d of before.doors) {
  const now = plan.doors.find((x) => x.id === d.id);
  if (!now) throw new Error("door lost: " + d.id);
  if (!d.between.some((r) => CHANGED.has(r)) && JSON.stringify(now) !== JSON.stringify(d)) throw new Error("door changed: " + d.id);
}

fs.mkdirSync(path.dirname(path.join(ROOT, PLAN_OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n52-kiln: wrote ${PLAN_OUT} (rebuilt: ${REBUILT.join(", ") || "none"}; links re-cut: ${RECUT.join(", ") || "none"})`);

// NIGHT57 builder L (row kiln57: n52-kiln -> n57-kiln; NIGHT57.md section 5 L; GG 2026-10-21 "3. yes": level 3 rebuilt from scratch). LEVEL 3 as n57-kiln.
// n52-kiln, its plan and its generators are untouched (this file reads n52's plan only), so the row off loads n52-kiln bit for bit.
// THE SLOTS (NIGHT57.md L2): the level has five combat-room slots in order: A kb2 (z -75.5..-103.5), B kb2c (z -163.5..-193.5), C kb3
// (z -197.5..-225.5), and DE (z -229.5..-277.5: n52's link kb3b, or the two new rooms kb3c + kb3d with the door d6b between them). A slot
// holds its NEW room (built below FROM SCRATCH from its A-9 card in docs/ROOMS.md "Level 3 as n57-kiln": no n52 box copied into it) once that
// room passes L3's checks (NEW, below); until then it holds n52's room bit for bit (kb2 / kb2c / kb3 / the link kb3b), so the level always
// plays end to end. The start (kb1, kb1b), the link kb2b (its revolver award), kb4 and k5 THE KILN'S HEART (the boss room; THE KILN WYRM's AI
// unchanged) are n52's bit for bit (asserted). Every piece of a new room carries a `job` naming the card field it serves and a `why`.
// The worm's lead-in beats (SurvivalSim.N47.Kiln.cs) key on room ids: the glimpse in kb1, the pass in kb2, the cameo in kb3: slots A and C keep
// those ids (and kb3 keeps kiln40's gauntlet award); `n47b beats --level n57-kiln` prints them.
// Run (from the worktree root):
//   node Tools/night/gen-n57-kiln.cjs [--slots=kb2,kb2c,kb3,de]   (default: the slots in NEW; --slots= with no ids = every slot n52's)
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n57/kiln.plan.json Design/levels/n57/kiln.json --walk
//   node Tools/night/gen-n57-kiln.cjs --finish
//   node Tools/night/gen-n57-kiln.cjs --check        (with every slot n52's: n57-kiln.json equals n52-kiln.json except the names)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n52/kiln.plan.json", PLAN_OUT = "Design/levels/n57/kiln.plan.json";
const GEN_OUT = "Design/levels/n57/kiln.json";
const OUTS = ["Design/levels/n57-kiln.json", "Assets/StreamingAssets/Design/levels/n57-kiln.json"];
const GENERATED_BY = "Tools/night/gen-n57-kiln.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";
const CARD = "docs/ROOMS.md, Level 3 as n57-kiln";
// The slots whose NEW room passed L3 (n56-room PASS, identity PASS, the contract 14/14, walk 10/10): the default build. A slot leaves this list
// (and goes back to n52's room) when its room fails a later check.
const NEW = ["kb2", "kb2c", "kb3", "de"];

// ---------------------------------------------------------------- the finish step (after levelgen): the game's two copies
if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n57-kiln") throw new Error(`${src}: name ${level.name}, expected n57-kiln`);
  level.data.name = "n40-kiln"; // the older systems' name for level 3 (as n52-kiln's: MapModelJson reads the outer name first)
  level.generatedBy = GENERATED_BY;
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  for (const f of OUTS) fs.writeFileSync(path.join(ROOT, f), out);
  console.log(`n57-kiln: finished ${path.relative(ROOT, src)} -> ${OUTS.join(", ")} (${out.length} bytes)`);
  process.exit(0);
}

// ---------------------------------------------------------------- the check: with every slot n52's, n57-kiln.json against n52-kiln.json
if (process.argv[2] === "--check") {
  const a = fs.readFileSync(path.join(ROOT, "Design/levels/n52-kiln.json"), "utf8");
  const b = fs.readFileSync(path.join(ROOT, OUTS[0]), "utf8");
  const b2 = fs.readFileSync(path.join(ROOT, OUTS[1]), "utf8");
  if (b !== b2) throw new Error("the two n57-kiln.json copies differ");
  const ja = JSON.parse(a), jb = JSON.parse(b);
  const skip = new Set(["name", "generatedBy", "_why", "source"]);
  const diffs = [];
  for (const k of new Set([...Object.keys(ja), ...Object.keys(jb)])) if (!skip.has(k) && JSON.stringify(ja[k]) !== JSON.stringify(jb[k])) diffs.push(k);
  console.log(`n57-kiln --check: copies identical (${b.length} bytes); keys differing from n52-kiln.json beyond the names: ${diffs.length ? diffs.join(", ") : "none"}`);
  process.exit(diffs.length ? 1 : 0);
}

// ---------------------------------------------------------------- the plan
const text = fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-kiln") throw new Error(`${PLAN_IN}: id ${plan.id}`);
plan.id = "n57-kiln";
plan._why = "NIGHT57 L (row kiln57; Tools/night/gen-n57-kiln.cjs; GG 2026-10-21 '3. yes'): level 3 rebuilt from scratch, five combat-room slots, each new room from its A-9 card (" + CARD + "). Before it: " + plan._why;
const arg = process.argv.find((a) => a.startsWith("--slots="));
const ONLY = arg ? arg.slice(8).split(",").filter(Boolean) : NEW;
const REBUILT = [], ADDED = [], REMOVED = [];

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
// a room's rows in each list replaced at the place its first old row had (every other row keeps its place: the guard below compares them)
function swap(key, isMine, fresh) {
  const arr = plan[key] || [];
  const at = arr.findIndex(isMine);
  const rest = arr.filter((x) => !isMine(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  plan[key] = rest;
}
function place(Z, rows, oldZ) {
  const old = oldZ || Z;
  const mineZ = (b) => b.zone === old;
  const mineR = (b) => b.room === old;
  for (const k of ["boxes", "ramps", "pads", "pits", "tunnels"]) swap(k, mineZ, rows[k] || []);
  for (const k of ["props", "lights"]) swap(k, mineR, rows[k] || []);
}
// the pieces: every one names the card field it serves (`job`, "<field>: <what>") and why it is there (`why`, with the verb it serves)
const piece = (Z) => ({
  box: (kind, min, max, job, why, extra) => Object.assign({ zone: Z, kind, min, max, job, why, note: why }, extra || {}),
  ramp: (from, to, width, rise, job, why) => ({ zone: Z, from, to, width, rise, job, why, note: why }),
  pad: (at, target, apexAbove, job, why, note) => ({ zone: Z, at, target, apexAbove, job, why, note }),
  pit: (rect, job, why, note) => ({ zone: Z, rect, floorY: -0.8, lava: true, job, why, note }),
  ceil: (id, x, z, h, w, d, range, intensity) => ({ id: Z + "-" + id, room: Z, kind: "ceiling", at: [x, h - 0.05, z], normal: [0, -1, 0], size: [w, d, 0.1], color: "white", range, intensity }),
  pool: (id, x, y, z, w, d, color, range, intensity, danger) => Object.assign({ id: Z + "-" + id, room: Z, kind: "pool", at: [x, y + 0.025, z], normal: [0, 1, 0], size: [w, d, 0.05], color, range, intensity }, danger ? { danger: true } : {}),
  health: (id, x, y, z, why) => ({ id: Z + "-" + id, room: Z, kind: "healthPickup", at: [x, y + 0.9, z], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", card57: "30 s problem: the reward on the risky leg", why }),
});

// ================================================================ slot A: kb2 3.2 THE CHARGING FLOOR (card: docs/ROOMS.md, Level 3 as n57-kiln, kb2)
// 20 x 28 x 14 m (x -10..10, z -103.5..-75.5; the doors d2 / d3 where n52's were). Depth d = -75.5 - z (0 at the entry door d2).
// The floor is the pack's; the west barrow run (2.5 m) and the charging deck (4.5 m, the far east corner over the exit) are the tiers. The
// verb: the THUNDERCLAP boost straight up out of the pack (the deck's walkers' ramp is a long way round); the flue wall's ride carries you down
// the east lane onto the deck; the bellows throw you up from the lane's end. kiln47's beat 2 (the worm's pass under the hall) keys on the id.
function slotA() {
  const Z = "kb2", P = piece(Z);
  const r = {
    id: Z, name: "3.2 THE CHARGING FLOOR", kind: "room", rect: [-10, -103.5, 10, -75.5], height: 14, roof: true, par: 60, job36: "teach",
    card57: "THE CHARGING FLOOR",
    verb36: "the THUNDERCLAP boost out of the pack onto the charging deck (4.5 m: its walkers' ramp is the long way round); the flue wall's ride down the east lane onto the deck; the bellows from the lane's end",
    spawn: { at: [1.5, 0, -80.5], yawDeg: 0 },
    spawn_why: "NIGHT57 L (card kb2, Molecule: the apron): 5 m in and 1.5 m off the door's axis (T-119: a back-step stays out of the door's 4 m band)",
    exit: [0, 0, -101.5],
    arena36: true,
    waves: [["husk", "husk", "husk", "swarmer", "swarmer", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "spitter", "spitter"]],
    roles43: ["fodder (the pack floods the charging floor: boost out of it)", "ranged + fodder (the spitters take the deck and the barrow run: boost up to them)", "fodder + ranged (the swarmers chase you up while the spitters hold)"],
    roles43_why: "NIGHT57 L (card kb2, The 30 s problem; R-2 / R-3): the level's first fight uses the roles level 1 taught (fodder, ranged): the boost is the only new thing; wave 2 puts the ranged where the boost reaches them first",
  };
  const ground = [[-2.5, 0, -92.5], [0.5, 0, -93], [-3.5, 0, -96.5], [-1, 0, -100], [-1.5, 0, -95], [-4.5, 0, -93]];
  const high = [[4.5, 4.5, -99.5], [3.5, 4.5, -102], [-8, 2.5, -88], [-8, 2.5, -83.5]];
  r.nodes = ground;
  r.highNodes = high;
  r.nodes_why = "NIGHT57 L (card kb2, Molecule; R-4): the pack on the open charging floor 13-25 m from the door (spawn31: the entry's cone only past 12 m), the ranged on the deck and the barrow run";
  r.waveNodes43 = [ground, high, ground.concat(high)];
  r.waveNodes43_why = "NIGHT57 L (card kb2): wave 1 (the pack) on the floor in front of the door (the opening: a vista over it); wave 2 (spitters + husks) on the two tiers; wave 3 anywhere";
  const lesson = { promptPad: "JUMP IN. SHOOT {fire} DOWN. FLY.", kind: "reached", rect: [-2, -86, 4, -82], minY: -0.2, prompt: "JUMP IN. SHOOT {fire} DOWN. FLY." };
  r.lessons = [lesson];
  r.lessons31 = [Object.assign({}, lesson)];
  r.proofs = [
    { kind: "walk", points: [[1.5, 0, -81], [0.5, 0, -91], [-3, 0, -95], [-3.5, 0, -100], [-0.5, 0, -102], [1, 0, -99.5], [1.2, 0, -94], [1, 0, -86], [1.5, 0, -81]] },
    { kind: "walk", points: [[3.9, 0, -85], [3.9, 4.5, -96.5], [6, 4.5, -99]] },
    { kind: "walk", points: [[-8, 0, -99], [-8, 2.5, -92], [-8, 2.5, -85]] },
    { kind: "walk", points: [[8, 0, -79], [8, 0, -90]] },
  ];
  r.frames36 = { mid: [0, 0, -77.5, 0, 0.1], exit: [5, 4.5, -98, 0, -0.3] };
  r.frames52 = { _why: "NIGHT57 L the room frames (a452 / a157): the door just crossed (the pack's floor, the charging deck and the flue wall beyond), the fight from the apron, the deck's top", entry: [0, 0, -77, 0, 0.04], mid: [-1, 0, -84, 15, 0.05], high: [5, 4.5, -97, 160, -0.25] };
  r.kiln57_why = "NIGHT57 L (Tools/night/gen-n57-kiln.cjs; " + CARD + ", kb2 THE CHARGING FLOOR): built from scratch (no n52 piece): 20 x 28 x 14 m, three planes (0 / 2.5 / 4.5 m), the flue wall (a 17 m ride, also the foothold), the bellows and the draught (pads), the stack; the boost card and the worm's pass (kiln47 beat 2, by room id) kept";
  const boxes = [
    P.box("hull", [-6, 0, -84], [-2.5, 14, -80], "every-solid: THE STACK, the kiln's flue column (the room's landmark from the door)", "THE STACK (3.5 x 4 m, floor to roof, 4.5-8.5 m in, west of the spawn): the kiln's flue column through the hall: the entry's cover from the barrow run's spitters (a line-of-sight break you leave to fight); the pack comes round both its sides to you (the loop's pivot near the door)"),
    P.box("wall", [5.4, 0, -95.5], [6, 14, -78.5], "molecule: the one-way ride, the flue wall (and the foothold)", "THE FLUE WALL (ride, cyan, 17 m, 3-20 m in, floor to roof): ride its east face down the east lane out of the pack and off its end onto the charging deck (the walk round is the deck ramp's long way); its near end is the foothold beside the spawn (R5: 3 m in, above the eye)", { ride: true, ride36: { face: "E", planeX: 6, lz: [3, 20] } }),
    P.box("deck", [-10, 0, -93], [-6, 2.5, -79], "spatial law: the barrow run (the mid tier)", "THE BARROW RUN (2.5 m, 4 x 14 m along the west wall): the founders' barrow track: a stand over the pack's floor, the spitters' second perch in wave 2; a double jump from the floor reaches it; the walkers come up its ramp at the far end (no safe ledge)"),
    P.box("hull", [-10, 2.5, -88.5], [-8.5, 14, -86.5], "every-solid: the run's post (cover on the mid tier)", "THE RUN's POST (1.5 x 2 m on the barrow run, to the roof): the cover a spitter on the run stands behind (you come round it: a stand you must move to break); it cuts the run's line down to the door"),
    P.box("deck", [2, 0, -103.5], [10, 4.5, -95.5], "30 s problem: THE CHARGING DECK (the high tier, the spitters' roost)", "THE CHARGING DECK (4.5 m, 8 x 8 m over the exit's east side): where the barrows were tipped into the kiln: the spitters' roost; the boost from the pack reaches its lip (jump + THUNDERCLAP down), the flue wall's ride lands on it, the bellows throw you up; the walkers take its ramp, the long way (no safe ledge)"),
    P.box("deck", [0.5, 3.4, -98.5], [2, 4.5, -95.5], "hero prop: the deck's tipping lip", "THE TIPPING LIP (4.5 m, 1.5 m out over the floor, 3.4 m clear under it): the barrows' tipping edge: shotgun the pack below from it after the boost; the spitter on it lobs into the floor"),
    P.box("hull", [8.5, 4.5, -102.5], [10, 14, -99.5], "every-solid: the deck's flue (cover on the high tier)", "THE DECK's FLUE (1.5 x 3 m on the charging deck, to the roof): the spitters' cover on the deck: you come round it after the boost; it hides the deck's back corner from the door"),
    P.box("deck", [-1, 0, -98.5], [0.5, 0.9, -96], "molecule: the small boost (T-5)", "THE SLAG CAKE (0.9 m beside the tipping lip): the small boost (jump, look down, fire) from it reaches the deck's lip; the big one from the floor (T-5: small, then big)"),
    P.box("hull", [-6, 0, -76.5], [-2, 14, -75.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (1 m thick, noride): no ride across the doorway: the fight goes into the room (the walk pilot rode the door wall: dwell, NIGHT52 kb3)", { noride: true }),
    P.box("hull", [2, 0, -76.5], [5.4, 14, -75.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (1 m thick, noride): as the west's: the room's ride is the flue wall", { noride: true }),
  ];
  const ramps = [
    P.ramp([3.9, -86], [3.9, -95.5], 3, 4.5, "molecule: the walkers' climb to the charging deck", "THE DECK RAMP (4.5 m over 9.5 m, flush to the flue wall's west face): the walkers' climb onto the charging deck (no safe ledge; GG: a route enemies can climb); the long way up for a player, through the pack"),
    P.ramp([-8, -98], [-8, -93], 3, 2.5, "molecule: the walkers' climb to the barrow run", "the barrow run's walkers' ramp (2.5 m over 5 m, from the far floor beside the exit): the walkers' way onto the run; the run is a stand, not a dead end (it drops anywhere to the floor)"),
  ];
  const pads = [
    P.pad([8, 0, -91.5], [7, 4.5, -98.5], 2, "molecule: the upward line, the bellows", "THE BELLOWS (a jump pad at the east lane's end, A5): the kiln's bellows vent: from the lane onto the charging deck (4.5 m): the pack that follows you down the lane is left on the floor", "the bellows"),
    P.pad([-4, 0, -88], [-8, 2.5, -91], 2, "molecule: the upward line, the draught", "THE DRAUGHT (a jump pad in the middle of the floor): the kiln's draught throws you out of the pack onto the barrow run (2.5 m): the second way up, toward the run's spitters in wave 2", "the draught"),
  ];
  const props = [
    P.health("P1", 8, 0, -86, "the health in the east lane, halfway down the flue wall (the riskiest leg: the pack follows you in, and the lane is closed at its end but for the bellows): ride or run for it"),
  ];
  const lights = [
    P.ceil("L1", 0, -80.5, 14, 4, 4, 12, 0.9),
    P.ceil("L2", -2, -90.5, 14, 6, 4, 12, 1.1),
    P.ceil("L3", -2, -99.5, 14, 4, 4, 12, 1),
    P.pool("L4", 6, 4.5, -99, 3, 3, "white", 9, 1),
    P.pool("L5", -8, 2.5, -90.5, 2.5, 2.5, "white", 7, 0.9),
    P.pool("L6", 8, 0, -91.5, 1.6, 1.6, "cyan", 5, 1.2),
    P.pool("L7", -1.5, 0, -101.5, 2.5, 2.5, "cyan", 8, 1.7),
  ];
  return { row: r, boxes, ramps, pads, pits: [], tunnels: [], props, lights };
}

// ================================================================ slot DE, room 1: kb3c 3.3c THE TAP RUN (card: docs/ROOMS.md, Level 3 as n57-kiln, kb3c)
// 22 x 24 x 12 m (x -11..11, z -253.5..-229.5; the entry door d6 where n52's was; the exit d6b at x -8..-4). Depth d = -229.5 - z.
// A ring round the tapping furnace (2.5 m, 8-14 m in: the ring's north crossing 8 m deep, out of the door's 4 m dwell band) with the tap run
// (lava) down the east wall under the free-standing run wall (the ride, the foothold) that lands on the spout ledge (2.5 m); the fettlers'
// walk (4.5 m, the high tier) on the west wall. The drones (level 2's flyer) circle over the run.
// Round 2 of this room (the contract's dwell 0.40 -> 1.35 s, then 1.75 s with the spawn moved): hold52 --dwelltrace showed the walk pilot
// running the ring's north crossing past the door (the furnace's near face was 5 m in): the furnace 8 m in and down to the mid tier (its ramp
// 5 m), the foothold the run wall standing free of the east wall, the fettlers' walk the high tier on its 9 m ramp.
function roomTapRun() {
  const Z = "kb3c", P = piece(Z);
  const r = {
    id: Z, name: "3.3c THE TAP RUN", kind: "room", rect: [-11, -253.5, 11, -229.5], height: 12, roof: true, par: 60, job36: "test",
    card57: "THE TAP RUN",
    verb36: "the ride: the run wall over the tap run up to the drones' height and onto the spout ledge; the tap vent onto the furnace, the blow pipe onto the fettlers' walk",
    spawn: { at: [-6.5, 0, -234], yawDeg: 0 },
    spawn_why: "NIGHT57 L (card kb3c, Molecule: the apron's west side): 4.5 m in and 6.5 m off the door's axis: a back-step ends at the west liner, 4.5 m from the door (T-119), not in the doorway",
    exit: [-6, 0, -251.5],
    arena36: true,
    waves: [["drone", "drone", "husk", "husk", "husk", "husk"], ["drone", "drone", "swarmer", "swarmer", "swarmer", "swarmer"]],
    waves_why: "NIGHT57 L (the level walk, r1: deaths 13 / 9 over seeds 1-10 / 11-20 against n52-kiln's 8 / 11, kb3c 7 / 5 of them): wave 2 one drone fewer (two drones a wave; wave 1 cut by a husk too read dwell 0.40 -> 1.25 s against the 1.2 s limit: kept at four husks)",
    roles43: ["flyer + fodder (the drones over the tap run, the husks round the ring)", "flyer + fodder (two drones again, the swarmers run the ring)"],
    roles43_why: "NIGHT57 L (card kb3c; R-2 / R-3): the flyer (level 2's drone) comes to level 3 with a known fodder role; the ride is the verb that reaches it over the lava",
  };
  const w1 = [[2.5, 0, -247.5], [3, 0, -246], [0, 0, -249.5], [8.5, 2.5, -251], [7.5, 2.5, -249]];
  const west = [[-6.5, 0, -248]];
  r.nodes = [[2.5, 0, -247.5], [3, 0, -246], [0, 0, -249.5], [-6.5, 0, -248]];
  r.highNodes = [[8.5, 2.5, -251], [7.5, 2.5, -249]];
  r.nodes_why = "NIGHT57 L (card kb3c, Molecule; R-4: flyers over the hazard): the far east floor and the spout ledge at the run's end (wave 1 in view down the run from the run wall's flank), the far west floor; the fettlers' walk and the furnace top stand within 12 m of the spawn in its cone (spawn31): no node there";
  r.waveNodes43 = [w1, w1.concat(west)];
  r.waveNodes43_why = "NIGHT57 L (card kb3c): wave 1 where the run wall's rider meets it (the far east floor, the spout ledge: a vista down the run); wave 2 anywhere on the ring";
  r.rims36 = { rects: [[6.5, -245.5, 7, -233.5]], why: "NIGHT57 L (card kb3c, Hazard both ways): the tap run's west rim: the walkers on the east floor pass along it; a blast puts them in" };
  r.proofs = [
    { kind: "walk", points: [[-6.5, 0, -234], [-6.5, 0, -247.5], [-3, 0, -250.5], [0, 0, -249.5], [3, 0, -247], [3, 0, -236], [-3, 0, -233], [-6.5, 0, -234]] },
    { kind: "walk", points: [[-2.5, 0, -249], [-2.5, 2.5, -243], [-2, 2.5, -239]] },
    { kind: "walk", points: [[0.5, 0, -252], [6, 2.5, -252], [7.5, 2.5, -249.5]] },
    { kind: "walk", points: [[-9.5, 0, -252], [-9.5, 4.5, -242], [-9.5, 4.5, -234]] },
  ];
  r.frames36 = { mid: [0, 0, -231.5, 0, 0.1], exit: [7.5, 2.5, -250, 0, -0.3] };
  r.frames52 = { _why: "NIGHT57 L the room frames (a452 / a157): the door just crossed (the furnace ahead, the tap run's glow and the run wall to the east), the fight from the east floor, the fettlers' walk looking over the room to the run", entry: [0, 0, -231, 0, 0.04], mid: [3, 0, -236, 10, 0.05], high: [-9.5, 4.5, -236, -45, -0.2] };
  r.kiln57_why = "NIGHT57 L (Tools/night/gen-n57-kiln.cjs; " + CARD + ", kb3c THE TAP RUN): built from scratch in the slot DE (n52's link kb3b removed): 22 x 24 x 12 m, three planes (0 / 2.5 / 4.5 m), the run wall (an 18 m ride over the tap run, the foothold), the furnace (the ring's mass), two pads";
  const boxes = [
    P.box("wall", [10.4, -0.8, -249.5], [11, 12, -231.5], "verb it celebrates: the run wall's ride over the tap run", "THE RUN WALL (ride, cyan, 18 m on the east wall standing in the tap run's lava, to the roof): ride it over the molten run up to the drones' height and off its end onto the spout ledge (the walk round is the ledge ramp's long way)", { ride: true, ride36: { face: "W", planeX: 10.4, lz: [2, 20] } }),
    P.box("hull", [2, 0, -234.5], [5, 12, -230.5], "hero prop: THE TAP HOOD (the run's head; the foothold)", "THE TAP HOOD (3 x 4 m, floor to roof, 1-5 m in, flush to the door's east jamb, at the head of the tap run; round 2: the ring's north crossing goes south of it, 5-8 m from the door, out of the dwell band; NIGHT60 L B14: 2 m narrower on its east side (x 7 -> 5), so the corner between it, the run wall's north end and the run's head (x 7..10.4, z -233.5..-229.5) opens on foot onto the east floor: it was a closed 3.4 x 4 m pocket, reached only out of the lava's knock-up or off the ride and left only through the lava (n46flow's pilot: NO PATH, put at the next room)): where the iron came out of the furnace's tap into the run: the entry's foothold of cover (R5) beside the door; you step out round it to fight down the run"),
    P.box("deck", [6, 0, -253.5], [11, 2.5, -247.5], "molecule: the spout ledge (the mid tier, the ride's landing)", "THE SPOUT LEDGE (2.5 m, the far east corner where the run poured into the spout): the run wall's landing (jump off the ride's end); a stand level with the drones over the run; the walkers come up its ramp (no safe ledge)"),
    P.box("hull", [9, 2.5, -253.5], [11, 12, -251.5], "every-solid: the spout (cover on the mid tier)", "THE SPOUT (on the ledge's back corner, to the roof: the run poured into it): the ledge's cover from the fettlers' walk: you come round it"),
    P.box("deck", [4, 0, -250.5], [6, 0.9, -248], "molecule: the small boost (T-5)", "THE INGOT PILE (0.9 m at the ledge's west face): the small boost (jump, look down, fire) from it onto the ledge; a step out of the pack on the east floor"),
    P.box("deck", [-5, 0, -243.5], [1, 2.5, -237.5], "hero prop: THE TAPPING FURNACE (the ring's mass, a mid-tier stand)", "THE TAPPING FURNACE (2.5 m, 6 x 6 m, 8-14 m in): the ring's mass the loop runs round (its north crossing 8 m from the door); a stand over the ring (the tap vent throws you on it, the walkers take its ramp)"),
    P.box("hull", [-5, 2.5, -241.5], [-3.5, 12, -239.5], "every-solid: the furnace's stack (cover on the furnace)", "THE FURNACE STACK (on the furnace top, to the roof): the cover on the furnace you come round; it cuts the furnace top's line to the fettlers' walk"),
    P.box("deck", [-11, 0, -242.5], [-8, 4.5, -229.5], "spatial law: the fettlers' walk (the high tier on the west wall)", "THE FETTLERS' WALK (4.5 m, 3 x 13 m on the west wall from the entry wall): the walk the fettlers skimmed from: the high ground over the ring, level with the drones; the blow pipe throws you up, the walkers come up its long ramp (no safe ledge)"),
    P.box("hull", [-8, 0, -230.5], [-2, 12, -229.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (1 m thick, noride, to the fettlers' walk): no ride across the doorway: the fight goes into the room", { noride: true }),
    P.box("hull", [2, 0, -230.5], [7, 12, -229.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (1 m thick, noride, behind the tap hood): as the west's: the room's ride is the run wall", { noride: true }),
  ];
  const ramps = [
    P.ramp([1, -252], [6, -252], 3, 2.5, "molecule: the walkers' climb to the spout ledge", "THE LEDGE RAMP (2.5 m over 5 m, along the far wall east onto the ledge): the walkers' climb to the ride's landing (no safe ledge); the long way round for a player"),
    P.ramp([-2.5, -248.5], [-2.5, -243.5], 3, 2.5, "molecule: the walkers' climb to the furnace", "THE FURNACE RAMP (2.5 m over 5 m, up the furnace's far face): the walkers' climb onto the furnace (GG: a route enemies can climb)"),
    P.ramp([-9.5, -251.5], [-9.5, -242.5], 3, 4.5, "molecule: the walkers' climb to the fettlers' walk", "THE WALK RAMP (4.5 m over 9 m along the west wall): the walkers' climb onto the high tier (no safe ledge); the slow way up for a player"),
  ];
  const pads = [
    P.pad([5.5, 0, -239], [-1, 2.5, -240.5], 2, "molecule: the upward line, the tap vent", "THE TAP VENT (a jump pad on the east floor beside the run): the furnace's tap vent throws you onto the furnace top, over the husks on the ring", "the tap vent"),
    P.pad([0, 2.5, -238.5], [-9.5, 4.5, -236], 2.5, "molecule: the upward line, the blow pipe", "THE BLOW PIPE (a jump pad on the tapping furnace's top, its north-east corner): the furnace's blow pipe launches you from the furnace (2.5 m) over the west lane onto the fettlers' walk (4.5 m), the high ground level with the drones: the ring's mass turned into the climb to the high tier (the walk ramp is the lane's slow way up). NIGHT60 L B15: it stood on the far west lane's floor at (-6.5, -251), 2.5 m in front of d6b's mouth, on every line out of the room (L-087: a pad on a walk line): n46flow's bot leaving for kb3d was thrown onto the walk past its path and stood at d6b's west jamb (put at the next room, seeds 14 / 17 / 20 / 27 over two levels); on the furnace it is off every floor line, 2 m from the furnace ramp's top and from the tap vent's landing", "the blow pipe"),
  ];
  const pits = [
    P.pit([7, -245.5, 11, -233.5], "hazard", "THE TAP RUN (4 x 12 m of lava down the east wall, the run wall standing in it; NIGHT60 L B14 cut 2: its south end 2 m short of the spout ledge (was z -247.5, against the ledge's 3.3 m face: a corner of lava, the run wall and the face, where every kb3c bot death but one was): a 2 m floor strip there, so the run's south end is left on foot like its sides): both ways (A-5): it burns you and knocks you up (a missed ride); a blast from the east floor puts the walkers in; the drones circle over it where no walker can follow", "the tap run"),
  ];
  const props = [
    P.health("P1", 6, 0, -241.5, "the health on the tap run's west rim, halfway down (the riskiest leg: the drones over the run, the lava beside it)"),
  ];
  const lights = [
    P.ceil("L1", 0, -232.5, 12, 4, 3, 11, 0.9),
    P.ceil("L2", 2, -241.5, 12, 4, 4, 11, 1.1),
    P.ceil("L3", -2, -250, 12, 4, 4, 11, 1),
    P.pool("L4", -1, 2.5, -240.5, 2.5, 2.5, "white", 8, 1),
    P.pool("L5", 8, 2.5, -250.5, 2, 2, "white", 7, 0.9),
    P.pool("L6", -9.5, 4.5, -236, 2, 2, "white", 7, 0.9),
    P.pool("L7", 8.25, -0.8, -240.5, 2, 10, "orange", 10, 1.4, true),
    P.pool("L8", -6, 0, -251.5, 2.5, 2.5, "cyan", 8, 1.7),
  ];
  return { row: r, boxes, ramps, pads, pits, tunnels: [], props, lights };
}

// ================================================================ slot DE, room 2: kb3d 3.3d THE CRACKED FLOOR (card: docs/ROOMS.md, Level 3 as n57-kiln, kb3d)
// 24 x 20 x 12 m (x -12..12, z -277.5..-257.5; the entry d6b at x -8..-4, the exit d7 where n52's was, into kb4 REST). Depth d = -257.5 - z.
// The worm's breach (lava) splits the ash (west: the burrower's, three iron plates) from the straight (east: the charger's, the kiln wall's ride
// onto the mouth ledge); two kiln posts are the charger's stun posts (the first is the foothold); the clinker heap (4.5 m) in the far west.
function roomCrackedFloor() {
  const Z = "kb3d", P = piece(Z);
  const r = {
    id: Z, name: "3.3d THE CRACKED FLOOR", kind: "room", rect: [-12, -277.5, 12, -257.5], height: 12, roof: true, par: 60, job36: "combine",
    card57: "THE CRACKED FLOOR",
    verb36: "the worm's two moves, small: hop to an iron plate when the ash glows (the burrower), side-step the charge into a kiln post and punch its back (the charger); the kiln wall's ride over the charge onto the mouth ledge",
    spawn: { at: [-7.5, 0, -262], yawDeg: 0 },
    spawn_why: "NIGHT57 L (card kb3d, Molecule: the apron): 4.5 m in, 1.5 m off the door's axis (T-119)",
    exit: [0, 0, -275.5],
    arena36: true,
    waves: [["burrower", "husk", "husk", "husk", "husk"], ["charger", "swarmer", "swarmer", "swarmer", "swarmer"], ["burrower", "charger"]],
    roles43: ["burrower + fodder (the breach under you on the ash)", "charger + fodder (the line down the straight)", "burrower + charger (the worm's two moves together: E-7)"],
    roles43_why: "NIGHT57 L (card kb3d; R-2 combine, E-7): both roles were introduced before (the burrower in kb2c, the charger in kb3); here they combine, the boss's language shown small before the boss",
  };
  const w1 = [[-6, 0, -275], [-3.5, 0, -275.5], [-0.5, 0, -274.5], [3, 0, -272.5], [5, 0, -272]];
  const east = [[8, 0, -266], [9, 0, -263]];
  r.nodes = w1.concat(east);
  r.highNodes = [[-10, 4.5, -274.5], [9.5, 2.5, -275.5]];
  r.nodes_why = "NIGHT57 L (card kb3d, Molecule; R-4: chargers on the long straight, burrowers under the open floor): wave 1 across the far floor in the entry's view past the first kiln post, the charger's straight (east, out of the entry's cone), the heap and the ledge";
  r.waveNodes43 = [w1, east.concat(w1), w1.concat(east)];
  r.waveNodes43_why = "NIGHT57 L (card kb3d): wave 1 (the burrower and the husks) on the far floor in view; wave 2 (the charger) from the straight's end; wave 3 anywhere on the floor";
  r.rims36 = { rects: [[-2, -270.5, -1.5, -261.5], [1, -270.5, 1.5, -261.5]], why: "NIGHT57 L (card kb3d, Hazard both ways): the breach's two rims: a charger that misses you at its edge goes in; a blast puts the walkers in" };
  r.proofs = [
    { kind: "walk", points: [[-7.5, 0, -262], [-0.5, 0, -260], [2.5, 0, -260.5], [2.5, 0, -268.5], [1.5, 0, -273], [-2.5, 0, -274], [-7, 0, -272], [-7.5, 0, -266], [-7.5, 0, -262]] },
    { kind: "walk", points: [[-9.5, 0, -262.2], [-9.5, 4.5, -271], [-9, 4.5, -274.5]] },
    { kind: "walk", points: [[1, 0, -275.5], [6, 2.5, -275.5], [9, 2.5, -276]] },
    { kind: "walk", points: [[9.5, 0, -260], [9.5, 0, -270]] },
  ];
  r.frames36 = { mid: [-6, 0, -259.5, 0, 0.1], exit: [8.5, 2.5, -275, 0, -0.3] };
  r.frames52 = { _why: "NIGHT57 L the room frames (a452 / a157): the door just crossed (the breach's crack, the ash and the straight either side), the fight from the apron's east, the clinker heap's top", entry: [-6, 0, -259, 0, 0.04], mid: [-1, 0, -261, -20, 0.05], high: [-9.5, 4.5, -273, -60, -0.25] };
  r.kiln57_why = "NIGHT57 L (Tools/night/gen-n57-kiln.cjs; " + CARD + ", kb3d THE CRACKED FLOOR): built from scratch in the slot DE: 24 x 20 x 12 m, three planes (0 / 2.5 / 4.5 m), the kiln wall (a 17 m ride), the breach, two kiln posts, three iron plates, the breach vent";
  const plate = (min, max, where) => P.box("deck", min, max, "spatial law: an iron plate on the ash (burrow-safe)", `AN IRON PLATE (${where}, 0.4 m): burrow-safe ground (the burrower cannot surface through iron): the hop you make when the ash glows under you`, { burrowSafe43: true });
  const boxes = [
    P.box("wall", [11.4, 0, -275.5], [12, 12, -258.5], "verb it celebrates: the kiln wall's ride over the charger's straight", "THE KILN WALL (ride, cyan, 17 m on the east wall, to the roof): ride it down the charger's straight out of its line and off its end onto the mouth ledge (the walk round is the ledge ramp's long way)", { ride: true, ride36: { face: "W", planeX: 11.4, lz: [1, 18] } }),
    P.box("deck", [6, 0, -277.5], [11.4, 2.5, -272.5], "molecule: the mouth ledge (the mid tier, the ride's landing)", "THE MOUTH LEDGE (2.5 m, the far east corner at the kiln mouth): the kiln wall's landing; a stand over the straight; the charger cannot climb, the walkers come up its ramp (no safe ledge)"),
    P.box("deck", [-12, 0, -277.5], [-7.5, 4.5, -270.5], "spatial law: the clinker heap (the high tier)", "THE CLINKER HEAP (4.5 m, the far west corner over the ash): the high ground the breach vent throws you onto; the burrower cannot reach you there, the husks come up its ramp (no safe ledge)"),
    P.box("hull", [-12, 4.5, -276], [-10.5, 12, -273.5], "every-solid: the heap's stack (cover on the high tier)", "THE HEAP's STACK (on the heap's back corner, to the roof): the cover on the high tier you come round; it cuts the heap's line to the door"),
    P.box("hull", [-3.5, 0, -264.5], [-2, 12, -261.5], "30 s problem: the first kiln post (the charger's stun post; the foothold)", "THE FIRST KILN POST (floor to roof, 4-7 m in beside the breach's head): the entry's foothold of cover (R5); bait the charge into it (the charger stuns 1.5 s, its back open: punch it)"),
    P.box("hull", [4, 0, -266.5], [5.5, 12, -264.5], "30 s problem: the second kiln post (the charger's stun post on the straight)", "THE SECOND KILN POST (floor to roof, on the straight): the charger's stun post where its line runs; side-step the charge into it, then punch"),
    plate([-11.5, 0, -261.5], [-8.5, 0.4, -259], "west, by the heap ramp's foot"),
    plate([-6.5, 0, -270], [-3.5, 0.4, -267], "the middle of the ash, beside the breach vent"),
    plate([3.5, 0, -271], [6.5, 0.4, -268.5], "east of the breach, on the straight"),
    P.box("hull", [-12, 0, -258.5], [-8, 12, -257.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (1 m thick, noride): no ride across the doorway: the fight goes into the room", { noride: true }),
    P.box("hull", [-4, 0, -258.5], [0, 12, -257.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (1 m thick, noride): as the west's: the room's ride is the kiln wall", { noride: true }),
  ];
  const ramps = [
    P.ramp([1.5, -275.5], [6, -275.5], 3, 2.5, "molecule: the walkers' climb to the mouth ledge", "THE LEDGE RAMP (2.5 m over 4.5 m, from beside the exit east onto the ledge): the walkers' climb to the ride's landing (no safe ledge)"),
    P.ramp([-9.5, -262.5], [-9.5, -270.5], 3, 4.5, "molecule: the walkers' climb to the clinker heap", "THE HEAP RAMP (4.5 m over 8 m, along the west wall): the walkers' climb onto the high tier (GG: a route enemies can climb); the slow way up for a player, over the burrower's ash"),
  ];
  const pads = [
    P.pad([-5, 0, -266], [-10, 4.5, -274], 2.5, "molecule: the upward line, the breach vent", "THE BREACH VENT (a jump pad on the ash beside the breach): the worm's heat through the crack throws you out of the burrower's ash onto the clinker heap (4.5 m)", "the breach vent"),
  ];
  const pits = [
    P.pit([-1.5, -270.5, 1, -261.5], "hazard", "THE BREACH (2.5 x 9 m of lava from the door's side into the room: where the worm came up): both ways (A-5): it burns you and knocks you up; a charger that misses you at its edge goes in; a blast puts the walkers in; it splits the burrower's ash from the charger's straight", "the breach"),
  ];
  const props = [
    P.health("P1", 2.5, 0, -266, "the health on the breach's east rim, on the charger's straight (the riskiest spot: the charge comes down the straight, the lava beside it)"),
  ];
  const lights = [
    P.ceil("L1", -6, -261, 12, 4, 3, 10, 0.9),
    P.ceil("L2", 0, -267, 12, 6, 4, 11, 1.1),
    P.ceil("L3", 2, -274.5, 12, 4, 4, 10, 1),
    P.pool("L4", -9.5, 4.5, -272.5, 2, 2, "white", 8, 1),
    P.pool("L5", 8.5, 2.5, -275, 2, 2, "white", 7, 0.9),
    P.pool("L6", -0.25, -0.8, -266, 1.5, 6, "orange", 9, 1.4, true),
    P.pool("L7", -5, 0.4, -268.5, 1.6, 1.6, "cyan", 5, 1),
    P.pool("L8", 5, 0.4, -269.75, 1.6, 1.6, "cyan", 5, 1),
    P.pool("L9", 0, 0, -275.5, 2.5, 2.5, "cyan", 8, 1.7),
  ];
  return { row: r, boxes, ramps, pads, pits, tunnels: [], props, lights };
}

// ================================================================ slot B: kb2c 3.2c THE CLINKER BRIDGES (card: docs/ROOMS.md, Level 3 as n57-kiln, kb2c)
// 18 x 30 x 12 m (x -9..9, z -193.5..-163.5; the doors d4 / d5 where n52's were). Depth d = -163.5 - z.
// The ash bay (the burrower's) crossed by two iron bridges: the near bridge (1.5 m) from the west wall, its ramp up its far face; the far
// bridge (4.5 m, the crane's run) from the east wall, its ramp rising from the west passage. The bridges are the burrow-safe high ground the
// swarmers contest; the crane mast is the foothold. The cooling wall's ride runs the east lane onto the far bridge.
// Round 2 (the first cut: IN VIEW 1.2 of 5, I1 74 %, the spawn on a plate unreached): the near bridge down to 1.5 m with its ramp on its far
// face (the view down the middle channel is clear over it; the mirror broken), the spawn on the ash south of the apron plate.
function slotB() {
  const Z = "kb2c", P = piece(Z);
  const r = {
    id: Z, name: "3.2c THE CLINKER BRIDGES", kind: "room", rect: [-9, -193.5, 9, -163.5], height: 12, roof: true, par: 60, job36: "teach",
    card57: "THE CLINKER BRIDGES",
    verb36: "the BURROWER: stay off its ash on the iron (the bridges, the plates) or hit it as it surfaces; the boost / the ash vent onto the far bridge; the cooling wall's ride down the east lane onto it",
    spawn: { at: [-2, 0, -168], yawDeg: -10 },
    spawn_why: "NIGHT57 L (card kb2c, Molecule: the apron): 4.5 m in from the door (NIGHT57: 5.5 m, T-119), 2 m west of its axis (was 7.5 m, south of the iron apron): the walk from the link kb2b's exit (0, -157.5) to this spawn ran into the entry wall west of the door (n46flow put the bot at the next room 8 times of 29 in kb2b, at (-6.6, -159.2) and the like); now the line goes through the door and clears the west liner by 0.8 m. NIGHT58 EXT: out of n46flow's 4 m door circle (NIGHT58 L0's (-1, -166) sat 2.5 m from the door). NIGHT60 L B13: facing the fight down the middle channel (yaw -10: wave 1's nodes ahead at 12.75-13.3 m, spawn31's 12 m in the cone met by moving them 2 m south); NIGHT58 EXT's yaw 45 faced the west wall and the near bridge's face 2-4 m off, 0 of wave 1's 5 seen in the a157 spawn frame (NIGHT60 B1)",
    exit: [0, 0, -191.5],
    arena36: true,
    waves: [["burrower", "swarmer", "swarmer", "swarmer", "swarmer"], ["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["burrower", "husk", "husk", "husk"]],
    roles43: ["burrower + fodder (it hunts the ash, the swarmers come up onto the bridges after you)", "fodder (the stokers off the far bridge walk you down onto its ash)", "burrower + fodder (again, with the husks)"],
    roles43_why: "NIGHT57 L (card kb2c; R-2 introduce, R-3: the level's one new role): the burrower arrives with a known fodder role whose answer (climb the bridges) is the burrower's too; melee only (no ranged: the level walk's deaths, r1)",
  };
  // NIGHT60 L B13: wave 1 2 m further south on the middle channel's ash (was z -178.5..-180: 10.8-12.4 m from the spawn), 12.75-13.3 m
  // from it, so the spawn can face them (spawn31: a node in the first view cone from 12 m)
  const w1 = [[0.5, 0, -180.5], [-1, 0, -180.75], [1, 0, -181], [2, 0, -180.5]];
  const far = [[-5, 0, -188], [6, 0, -190.5], [-8, 0, -184]];
  const high = [[3, 4.5, -183], [6.5, 4.5, -183]];
  r.nodes = w1.concat(far);
  r.highNodes = high;
  r.nodes_why = "NIGHT57 L (card kb2c, Molecule; R-4: burrowers under the open floor): wave 1 on the middle channel's ash between the bridges, in view past the crane mast (NIGHT60 L B13: at its south side, just north of the far ramp, 12.75-13.3 m from the spawn, ahead of its facing); the far bay; the far bridge; the near bridge stands within 12 m of the spawn in its cone (spawn31): no node there";
  r.waveNodes43 = [w1, w1.concat(far, high), w1.concat(far)];
  r.waveNodes43_why = "NIGHT57 L (card kb2c): wave 1 (the burrower and the swarmers) only on the ash: a burrower hunts on its spawn's height; wave 2 (the stokers) also on the far bridge; wave 3 on the ash";
  r.proofs = [
    { kind: "walk", points: [[-2, 0, -168], [-1.5, 0, -170.5], [-0.5, 0, -177], [-8, 0, -178], [-8, 0, -186], [0, 0, -192], [7, 0, -192], [7, 0, -186]] },
    { kind: "walk", points: [[-3.5, 0, -178], [-3.5, 1.5, -174], [-7, 1.5, -173]] },
    { kind: "walk", points: [[-8, 0, -183], [1, 4.5, -183], [6, 4.5, -183]] },
    { kind: "walk", points: [[6.5, 0, -165.5], [6.5, 0, -174]] },
  ];
  r.frames36 = { mid: [0, 0, -164.5, 0, 0.1], exit: [5, 4.5, -183, 0, -0.3] };
  r.frames52 = { _why: "NIGHT57 L the room frames (a452 / a157): the door just crossed (the crane mast, the ash channel between the two bridges, the far bridge), the fight from the east lane, the far bridge looking back over the bay", entry: [1.5, 0, -165, -10, 0.05], mid: [6.5, 0, -170, 10, 0.05], high: [3, 4.5, -183, 150, -0.25] };
  r.kiln57_why = "NIGHT57 L (Tools/night/gen-n57-kiln.cjs; " + CARD + ", kb2c THE CLINKER BRIDGES): built from scratch (no n52 piece): 18 x 30 x 12 m, three planes (0 / 1.5 / 4.5 m), two bridges across the ash, the cooling wall (a 17 m ride), the crane mast (the foothold), four iron plates, the ash vent; the burrower's lesson cards (kiln43, by room id) kept";
  const plate = (min, max, where, why) => P.box("deck", min, max, "spatial law: iron on the ash (burrow-safe)", `${where} (0.4 m): burrow-safe ground (the burrower cannot surface through iron): ${why}`, { burrowSafe43: true });
  const boxes = [
    P.box("wall", [8.4, 0, -181.5], [9, 12, -164.5], "verb it celebrates: the cooling wall's ride off the ash", "THE COOLING WALL (ride, cyan, 17 m on the east wall, to the roof): ride the east lane off the burrower's ash and off its end onto the far bridge (the walk round is the far ramp's long way, by the west passage)", { ride: true, ride36: { face: "W", planeX: 8.4, lz: [1, 18] } }),
    P.box("deck", [1, 2.5, -184.5], [7, 4.5, -181.5], "spatial law: THE FAR BRIDGE (the crane's run, the high tier)", "THE FAR BRIDGE (4.5 m, 8 x 3 m from the east wall across the bay: the crane's run; NIGHT58 L0: x 1..7 a 2 m span, its underside 2.5 m up, the bay walks under it to the exit; x 7..9 THE LANDING PIER, solid, the ride's landing): the burrow-safe high ground; the ride's landing, the ash vent's goal; the stokers come up its ramp from the west passage (no safe ledge). NIGHT58 L0 why: n46flow put the bot at the next room 12 times of 29 here, pinned at (6.4, 0, -181.2) (the cooling wall's end and the bridge's north face): the bridge on the floor and the far ramp walled the room off from x -7 to the east wall; a 2 m east arch alone left 9 of 20"),
    P.box("deck", [7, 0, -184.5], [9, 4.5, -181.5], "spatial law: THE FAR BRIDGE's landing pier (the ride's end)", "THE LANDING PIER (NIGHT58 L0: the far bridge's east 2 m, solid to the floor, 4.5 m): the cooling wall's ride lands on it (the ride stays a shortcut: I2), the bridge's span west of it walks under"),
    P.box("deck", [-9, 0, -174.5], [-2, 1.5, -171.5], "spatial law: THE NEAR BRIDGE (the mid tier)", "THE NEAR BRIDGE (1.5 m, 7 x 3 m from the west wall over the ash): the burrow-safe stand one jump off the ash; the swarmers come up its ramp from the middle channel (no safe ledge)"),
    P.box("hull", [3, 0, -168.5], [5, 12, -165.15], "hero prop: THE CRANE MAST (the foothold)", "THE CRANE MAST (2 x 3.35 m, floor to roof, 1.65-5 m in, east of the door, 0.65 m off the east liner: no way round its north side; round 2: on the door's axis the walk pilot circled it in the doorway, dwell 0.20 -> 1.40 s): the clinker crane's mast: the entry's foothold of cover (R5); step out past it onto the ash"),
    plate([-9, 0, -167], [-6.5, 0.4, -164.5], "THE IRON APRON", "by the door: the restart's back-step lands on iron, the burrower's tell never blooms there"),
    plate([5.5, 0, -178], [8, 0.4, -175.5], "AN IRON PLATE (the east lane)", "the hop off the ash under the cooling wall, before the ride"),
    plate([-6, 0, -181], [-3, 0.4, -178.5], "AN IRON PLATE (the middle bay, west)", "the island in the middle bay: hop to it when the ash glows under you"),
    plate([2, 0, -191], [5, 0.4, -188.5], "AN IRON PLATE (the far bay)", "the island by the exit: the last hop off the ash"),
    P.box("deck", [4, 0, -187], [7, 0.9, -184.5], "molecule: the small boost (T-5)", "THE SLAG STEP (0.9 m against the far bridge's south face; NIGHT58 L0: 2.5 m east, so the walk out from under the arch is clear between it and the far ramp's top (round 2: the bot pinned at (2.2, 0, -184.2) between them, 5 of 20)): the small boost (jump, look down, fire) onto the far bridge; a step off the ash"),
    P.box("hull", [-6, 0, -164.5], [-2, 12, -163.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (1 m thick, noride): no ride across the doorway: the fight goes into the room", { noride: true }),
    P.box("hull", [2, 0, -164.5], [6, 12, -163.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (1 m thick, noride): as the west's: the room's ride is the cooling wall", { noride: true }),
  ];
  const ramps = [
    P.ramp([-3.5, -177.5], [-3.5, -174.5], 3, 1.5, "molecule: the walkers' climb to the near bridge", "THE NEAR RAMP (1.5 m over 3 m, from the middle channel up the near bridge's far face): the swarmers' climb to you (no safe ledge)"),
    P.ramp([-7, -183], [1, -183], 3, 4.5, "molecule: the walkers' climb to the far bridge", "THE FAR RAMP (4.5 m over 8 m, from the west passage east up onto the far bridge): the stokers' climb (GG: a route enemies can climb); the long way round for a player"),
  ];
  const pads = [
    P.pad([4, 0, -179], [5, 4.5, -183], 2, "molecule: the upward line, the ash vent", "THE ASH VENT (a jump pad on the ash under the far bridge's north face): the kiln's draught through the ash throws you up onto the far bridge (4.5 m): off the burrower's ground in one launch", "the ash vent"),
  ];
  const props = [
    P.health("P1", -1, 0, -177, "the health on the middle channel's open ash between the bridges (the burrower's ground: the riskiest spot)"),
  ];
  const lights = [
    P.ceil("L1", -4, -167, 12, 3, 2, 10, 0.9),
    P.ceil("L2", -2, -178, 12, 6, 4, 11, 1.1),
    P.ceil("L3", 0, -189, 12, 4, 4, 11, 1),
    P.pool("L4", 6.5, 4.5, -183, 2, 2, "white", 8, 1),
    P.pool("L5", -5.5, 1.5, -173, 2, 2, "white", 7, 0.9),
    P.pool("L6", -4.5, 0.4, -179.75, 1.6, 1.6, "cyan", 5, 1),
    P.pool("L7", 3.5, 0.4, -189.75, 1.6, 1.6, "cyan", 5, 1),
    P.pool("L8", 0, 0, -191.5, 2.5, 2.5, "cyan", 8, 1.7),
  ];
  return { row: r, boxes, ramps, pads, pits: [], tunnels: [], props, lights };
}

// ================================================================ slot C: kb3 3.3 THE PRESS HALL (card: docs/ROOMS.md, Level 3 as n57-kiln, kb3)
// 20 x 28 x 14 m (x -10..10, z -225.5..-197.5; the doors d5 / d6 where n52's were). Depth d = -197.5 - z.
// A nave (Recipe C, THE CHINA SHOP): the charger's open middle between two staggered rows of drop presses (its stun posts); the press gantry
// (2.5 m) on the west wall, the far gallery (4.5 m, the spitters') across the east of the far end; the forge wall's ride down the east aisle
// onto the gallery. The id kb3 keeps kiln40's gauntlet award (on entry) and kiln47's beat 3, THE CAMEO, with its card ("When the floor glows,
// jump.", n52's lesson row kept as it was).
function slotC() {
  const Z = "kb3", P = piece(Z);
  const lesson = { promptPad: "When the floor glows, jump.", kind: "husksDead", n: 1, prompt: "When the floor glows, jump." };
  const r = {
    id: Z, name: "3.3 THE PRESS HALL", kind: "room", rect: [-10, -225.5, 10, -197.5], height: 14, roof: true, par: 70, job36: "combine",
    card57: "THE PRESS HALL",
    verb36: "the gauntlet (awarded on entry): side-step the charger into a drop press (it stuns) and punch its open back; the forge wall's ride down the east aisle over the next charge onto the far gallery; jump when the floor glows (the worm's cameo)",
    spawn: { at: [-8.5, 0, -203], yawDeg: 0 },
    spawn_why: "NIGHT57 L (card kb3, Molecule: the west aisle under the press gantry's near end): 5.5 m in and 8.5 m off the door's axis, 8.5 m from the door (n52 kb3's T-119 distance; rounds 1-4 at 6.4 m read dwell 1.15-2.50 s)",
    exit: [0, 0, -224],
    arena36: true,
    waves: [["husk", "husk", "husk", "husk", "husk", "spitter", "spitter"], ["charger", "husk", "husk", "husk", "husk"], ["charger", "spitter", "spitter"], ["husk", "husk", "husk", "husk", "swarmer", "swarmer", "swarmer", "swarmer"]],
    waves_why: "NIGHT57 L (card kb3; the level walk r2: n52's kb3 was the level's deadliest room, 6 / 6 deaths): no drones here (they are kb3c's role); the charger alone with fodder first, then with the spitters",
    roles43: ["fodder + ranged (the spitters on the far gallery)", "charger + fodder (the charger introduced to level 3: bait it into a press, punch)", "charger + ranged (the charge with the globs coming in)", "fodder (the hall floods: the last push down the nave)"],
    roles43_why: "NIGHT57 L (card kb3; R-2: the charger reinforced with fodder before it combines with ranged; the gauntlet arrives in this room)",
    lessons: [lesson],
    lessons31: [Object.assign({}, lesson)],
    kiln47_why: "NIGHT47 B G35 (kept by NIGHT57 L): the cameo's card (<= 6 words): the worm's breach tell is taught here before the fight",
  };
  const w1 = [[0, 0, -213], [2, 0, -214], [-2, 0, -216], [1, 0, -219], [6, 4.5, -223], [7, 4.5, -220.5]];
  const ground = [[-8.5, 0, -222], [7.5, 0, -207], [0, 0, -213], [2, 0, -214], [-2, 0, -216], [1, 0, -219]];
  const high = [[6, 4.5, -223], [7, 4.5, -220.5]];
  r.nodes = ground;
  r.highNodes = high;
  r.nodes_why = "NIGHT57 L (card kb3, Molecule; R-4: chargers on the long straight, ranged on high ground): the far end of the nave (the charger's line runs back from it), the east aisle, the far west corner, the far gallery (the spawn 9.5 m in: every node in its cone past 12 m, spawn31; none on the press gantry)";
  r.waveNodes43 = [w1, ground, ground.concat(high), ground];
  r.waveNodes43_why = "NIGHT57 L (card kb3): wave 1 down the nave and on the far gallery in view past the first press (held); waves 2-3 the nave, the aisles and the tiers";
  r.proofs = [
    { kind: "walk", points: [[-6.5, 0, -202], [0, 0, -203], [2.5, 0, -210], [2, 0, -218], [-6, 0, -220], [-6, 0, -225], [1, 0, -224.5]] },
    { kind: "walk", points: [[-8.5, 0, -220], [-8.5, 2.5, -214], [-8.5, 2.5, -207]] },
    { kind: "walk", points: [[-6, 0, -221], [3, 4.5, -221], [6, 4.5, -223]] },
    { kind: "walk", points: [[8, 0, -201], [8, 0, -208]] },
  ];
  r.frames36 = { mid: [0, 0, -199.5, 0, 0.1], exit: [6, 4.5, -223, 0, -0.3] };
  r.frames52 = { _why: "NIGHT57 L the room frames (a452 / a157): the door just crossed (the nave between its presses, the far gallery), the fight from the apron's west, the gallery looking back up the nave", entry: [0, 0, -199, 0, 0.05], mid: [-3, 0, -203, -20, 0.05], high: [6, 4.5, -223, 180, -0.25] };
  r.kiln57_why = "NIGHT57 L (Tools/night/gen-n57-kiln.cjs; " + CARD + ", kb3 THE PRESS HALL): built from scratch (no n52 piece): 20 x 28 x 14 m, three planes (0 / 2.5 / 4.5 m), four drop presses (the charger's stun posts, the first the foothold), the forge wall (a 17 m ride), two pads; the cameo's card and beat (by room id) and the gauntlet award (by room id) kept";
  const press = (min, max, which, extra) => P.box("hull", min, max, "hero prop: a drop press (the charger's stun post)" + (extra ? "; " + extra : ""), `THE ${which} DROP PRESS (1.5 x 2-2.5 m, floor to roof): where the castings were stamped: the charger's stun post (side-step its charge into it: stunned 1.5 s, its back open: punch it with the gauntlet); a line break down the nave`);
  const boxes = [
    Object.assign(press([4, 0, -202], [5.5, 14, -199.15], "FIRST (east, 1.65-4.5 m in, 0.65 m off the east liner: no pocket north of it; round 2: at 3-5.5 m the walk pilot stuck in the corner between it and the liner, dwell 0.40 -> 2.50 s; noride: the pilot rode it in the doorway)", "the foothold (R5)"), { noride: true }),
    press([6, 0, -211.5], [7.5, 14, -209.5], "SECOND (east aisle)"),
    press([-5.5, 0, -207.5], [-4, 14, -205.5], "THIRD (west)"),
    press([-5.5, 0, -215.5], [-4, 14, -213.5], "FOURTH (west)"),
    P.box("wall", [9.4, 0, -217.5], [10, 14, -200.5], "verb it celebrates: the forge wall's ride over the charge", "THE FORGE WALL (ride, cyan, 17 m on the east wall, to the roof): ride the east aisle over the charger's next charge and off its end onto the far gallery (the walk round is the gallery ramp's long way, by the west of the far end)", { ride: true, ride36: { face: "W", planeX: 9.4, lz: [3, 20] } }),
    P.box("deck", [5, 0, -225.5], [10, 4.5, -218.5], "30 s problem: THE FAR GALLERY (the spitters' roost, the high tier)", "THE FAR GALLERY (4.5 m, 7 x 7 m across the far end's east side): the sprayers' roost lobbing down the nave; the forge wall's ride lands on it, the boost or the step reaches its lip; the walkers take its long ramp (no safe ledge)"),
    P.box("deck", [3, 2.5, -225.5], [5, 4.5, -218.5], "spatial law: THE FAR GALLERY's west span (the exit lane under it)", "THE GALLERY ARCH (NIGHT58 L0: the gallery's west 2 m on a 2 m span, its underside 2.5 m up, where the gallery ramp lands): the ramp still reaches the gallery at 4.5 m; the nave walks under it to the door's corridor (n46flow: the bot was put at the next room 6 times of 29 pinned on the ramp's north face at (0.6, -219.2) / (2.7, -217.6): the ramp and the gallery walled the door off from x -5 to the east wall)"),
    P.box("hull", [8.5, 4.5, -224.5], [10, 14, -221.5], "every-solid: the gallery's hood (cover on the high tier)", "THE GALLERY HOOD (on the gallery's back corner, to the roof): the spitters' cover you come round after the ride"),
    P.box("deck", [-10, 0, -214.5], [-7, 2.5, -205.5], "spatial law: THE PRESS GANTRY (the mid tier along the west wall)", "THE PRESS GANTRY (2.5 m, 3 x 9 m on the west wall, from 8 m in): the press operators' walk over the west aisle: a stand out of the charger's line (it cannot climb), the walkers come up its ramp (no safe ledge)"),
    P.box("deck", [5.5, 0, -218.5], [8, 0.9, -216.5], "molecule: the small boost (T-5)", "THE INGOT STACK (0.9 m at the far gallery's north face; NIGHT58 L0: 2.5 m east, off the gallery arch's mouth): the small boost (jump, look down, fire) onto the gallery's lip"),
    P.box("hull", [-10, 0, -198.5], [-2, 14, -197.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (1 m thick, the entry wall's whole west half, noride; round 4: the walk pilot rode the bare entry wall into the doorway): no ride across the doorway: the fight goes into the hall", { noride: true }),
    P.box("hull", [2, 0, -198.5], [10, 14, -197.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (1 m thick, the entry wall's whole east half, noride): as the west's: the hall's ride is the forge wall", { noride: true }),
  ];
  const ramps = [
    P.ramp([-5, -221], [3, -221], 3, 4.5, "molecule: the walkers' climb to the far gallery", "THE GALLERY RAMP (4.5 m over 8 m, from the far end's west east onto the gallery): the walkers' climb (GG: a route enemies can climb); the long way up for a player"),
    P.ramp([-8.5, -219.5], [-8.5, -214.5], 3, 2.5, "molecule: the walkers' climb to the press gantry", "the press gantry's ramp (2.5 m over 5 m, up its far end): the walkers' way onto the gantry"),
  ];
  const pads = [
    P.pad([-2, 0, -210.5], [-8.5, 2.5, -210.5], 2, "molecule: the upward line, the hammer vent", "THE HAMMER VENT (a jump pad in the nave, between the two west presses): the steam hammer's vent throws you out of the charger's line onto the press gantry (2.5 m) (round 2: from 3.5 m north its arc met the third press)", "the hammer vent"),
    P.pad([-8.5, 0, -224.5], [5, 4.5, -224], 2.5, "molecule: the upward line, the quench vent", "THE QUENCH VENT (a jump pad in the far west corner): throws you over the gallery ramp onto the far gallery: the nave's far end turned into a climb (the loop's second end)", "the quench vent"),
  ];
  const props = [
    P.health("P1", 0, 0, -212, "the health in the middle of the nave (the charger's line: the riskiest spot)"),
  ];
  const lights = [
    P.ceil("L1", 0, -201, 14, 4, 3, 12, 0.9),
    P.ceil("L2", 0, -211, 14, 4, 4, 12, 1.1),
    P.ceil("L3", 0, -220, 14, 4, 4, 12, 1),
    P.pool("L4", -8.5, 2.5, -209, 2, 2, "white", 7, 0.9),
    P.pool("L5", 6, 4.5, -223.5, 2, 2, "white", 8, 1),
    P.pool("L6", -2, 0, -210.5, 1.6, 1.6, "cyan", 5, 1.2),
    P.pool("L7", 0, 0, -224, 2.5, 2.5, "cyan", 8, 1.7),
  ];
  return { row: r, boxes, ramps, pads, pits: [], tunnels: [], props, lights };
}

// slot DE: n52's link kb3b (its rows and its place) replaced by the two rooms and the door d6b between them; d6 / d7 keep their rects
function slotDE() {
  const i = plan.rooms.findIndex((x) => x.id === "kb3b");
  if (i < 0) throw new Error("no kb3b");
  if (!eq(plan.rooms[i].rect, [-8, -277.5, 8, -229.5])) throw new Error("kb3b is not n52's");
  const D = roomTapRun(), E = roomCrackedFloor();
  plan.rooms.splice(i, 1, D.row, E.row);
  for (const k of ["boxes", "ramps", "pads", "pits", "tunnels"]) {
    const at = plan[k].findIndex((o) => o.zone === "kb3b");
    const rest = plan[k].filter((o) => o.zone !== "kb3b");
    rest.splice(at < 0 ? rest.length : at, 0, ...(D[k] || []), ...(E[k] || []));
    plan[k] = rest;
  }
  for (const k of ["props", "lights"]) {
    const at = plan[k].findIndex((o) => o.room === "kb3b");
    const rest = plan[k].filter((o) => o.room !== "kb3b");
    rest.splice(at < 0 ? rest.length : at, 0, ...(D[k] || []), ...(E[k] || []));
    plan[k] = rest;
  }
  const d6 = door("d6"), d7 = door("d7");
  if (!eq(d6.between, ["kb3", "kb3b"]) || !eq(d7.between, ["kb3b", "kb4"])) throw new Error("d6 / d7 are not n52's");
  d6.between = ["kb3", "kb3c"];
  d7.between = ["kb3d", "kb4"];
  plan.doors.splice(plan.doors.indexOf(d6) + 1, 0, { id: "d6b", between: ["kb3c", "kb3d"], rect: [-8, -257.5, -4, -253.5], height: 4.5, checkpoint: true });
  REMOVED.push("kb3b");
  ADDED.push("kb3c", "kb3d");
}

const BUILD = { kb2: slotA, kb2c: slotB, kb3: slotC, de: slotDE };
for (const id of ONLY) {
  if (!BUILD[id]) throw new Error("no new room for slot " + id + " yet");
  if (id === "de") { slotDE(); continue; }
  const out = BUILD[id]();
  const i = plan.rooms.findIndex((x) => x.id === id);
  if (i < 0) throw new Error("no slot room " + id);
  plan.rooms[i] = out.row;
  place(id, out);
  REBUILT.push(id);
}

// ---------------------------------------------------------------- asserts: every other room is n52's bit for bit
const CHANGED = new Set([...REBUILT, ...ADDED, ...REMOVED]);
for (let i = 0; i < before.rooms.length; i++) {
  const id = before.rooms[i].id;
  if (REMOVED.includes(id)) continue;
  const now = plan.rooms.find((r) => r.id === id);
  if (!now) throw new Error("room lost: " + id);
  if (!CHANGED.has(id) && JSON.stringify(now) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + id);
}
for (const id of ["kb1", "kb1b", "kb2b", "kb4", "k5"]) if (CHANGED.has(id)) throw new Error("a kept room was rebuilt: " + id);
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
// the new rooms: no n52 box copied (no box of a rebuilt slot equals one of n52's boxes anywhere in the level)
const n52boxes = new Set(before.boxes.map((b) => JSON.stringify([b.min, b.max])));
for (const b of plan.boxes) if ((REBUILT.includes(b.zone) || ADDED.includes(b.zone)) && n52boxes.has(JSON.stringify([b.min, b.max]))) throw new Error("a new room copies an n52 box: " + JSON.stringify(b));
for (const k of ["boxes", "ramps", "pads", "pits", "tunnels"]) for (const o of plan[k] || []) if ((REBUILT.includes(o.zone) || ADDED.includes(o.zone)) && !(String(o.job || "").includes(":") || o.job === "hazard")) throw new Error("a new piece without its card field: " + JSON.stringify(o));

fs.mkdirSync(path.dirname(path.join(ROOT, PLAN_OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n57-kiln: wrote ${PLAN_OUT} (new rooms: ${REBUILT.concat(ADDED).join(", ") || "none"}; n52's link removed: ${REMOVED.join(", ") || "none"}; every other room n52's)`);

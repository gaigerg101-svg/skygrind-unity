// NIGHT45 builder A3 (NIGHT45.md card A3, J4 / J5, T-65): LEVEL 3 as n45-kiln. design.n45.kiln45 swaps n43-kiln for it (Core/N45Levels.cs;
// N45Levels.BaseId maps n45-kiln -> n43-kiln, so every older system that knows level 3 by id keeps it). n43-kiln, its plan and its generator
// (Tools/night/gen-n43-kiln.cjs) are untouched, so --off45 loads n43-kiln bit for bit.
//
// What changes against n43-kiln: ONLY k5 3.5 THE RING, rebuilt from its identity card (docs/ROOMS.md "NIGHT45 identity cards": THE KILN'S HEART;
// DESIGN-RULES A-9). Every other room, door, box, light, prop, nav cell and spawn is n43-kiln's, bit for bit: this generator copies n43's PLAN
// (Design/levels/n43/kiln.plan.json -> Design/levels/n45/kiln.plan.json) and n43's FINISHED level (Design/levels/n43-kiln.json -> n45-kiln.json +
// the StreamingAssets copy) and edits the k5 row and the k5 props in both. k5 has no collider, box, ramp, pit or pad of its own (gen-n43-kiln.cjs
// throws if it had), so levelgen's nav grid and collision are unchanged by these edits (checked: `levelgen` on the n45 plan, see A3.md).
//   1. THE ARENA: the k5 row's `wyrmArena` names Design/maps/wyrm45-arena.json (Tools/night/gen-n45-wyrm-arena.cjs: wyrm43's 30 boxes in order +
//      four slag plates, each tied to the attack it answers; every box's reason), and crumble31 is re-pointed at it as gen-n43-kiln.cjs did
//      (the same indices: the two 13 m perches crumble at half health, the 6 m perch stays; checked against the arena's boxes here).
//   2. THE 76 PROPS (the audit: 0 of 76 with a reason). The fight is NOT in this 26 x 26 m room: Campaign24.Enter starts the boss on the first
//      step past door d8 and Respawns the player into the worm's arena (far away, design.n24.wyrm.room.origin). So the room is only what the
//      player sees THROUGH THE BOSS DOOR from 3.4 REST. Each prop is measured: SEEN when a straight line from an eye in REST (a 1 m grid over
//      kb4's floor at the eye height 1.6 m, and the door's mouth) passes through d8's opening (its rect and height, read from the plan) to a
//      point of the prop's box (5 x 5 x 3 samples) and meets the face that faces the room (a wall prop's face; a hung prop has none). Then:
//        KEEP   a SEEN prop whose kind tells the kiln's world (V-4): the hero furnace, the banners, the feed pipe, the hung lamps (V-5: a light on
//               a real fixture, with its chain and truss), the spill and the castings at the furnace's foot; each gets `job` and `why`.
//        CUT    a prop nobody sees (no line from REST reaches it), or dress33's generic trim (wash, skirt, cornice, rib, recess, pilaster, the
//               edge light: "a neon strip because the wall was empty", V-4's never), seen or not.
//      The list (kept with why / cut with reason) is written into the k5 row as `props45` and printed.
//   Lights: k5's four lights (campaign.lights) are kept as they are (not props; the floor pool's danger orange is what the door view lights).
// Run (from the repo or worktree root): node Tools/night/gen-n45-wyrm-arena.cjs && node Tools/night/gen-n45-kiln.cjs
// Checks: dotnet run --project Tools/SimHarness -c Release -- n45ring wyrm --level n45-kiln (Harness.N45.Ring.cs); levelgen on the n45 plan.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const r3 = (v) => Math.round(v * 1000) / 1000;

const ARENA = "Design/maps/wyrm45-arena.json";
const A = JSON.parse(fs.readFileSync(path.join(ROOT, ARENA), "utf8"));
const PLAN_IN = "Design/levels/n43/kiln.plan.json", PLAN_OUT = "Design/levels/n45/kiln.plan.json";
const LEVEL_IN = "Design/levels/n43-kiln.json";
const LEVEL_OUTS = ["Design/levels/n45-kiln.json", "Assets/StreamingAssets/Design/levels/n45-kiln.json"];
const EYE = 1.6; // PortExport/config.json body.eyeHeight

const plan = JSON.parse(fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8"));
const levelText = fs.readFileSync(path.join(ROOT, LEVEL_IN), "utf8");
const level = JSON.parse(levelText);
if (level.name !== "n43-kiln") throw new Error(LEVEL_IN + ": name " + level.name);

// ---------------------------------------------------------------- the door view: which k5 props a player in 3.4 REST can see through d8
const kb4 = plan.rooms.find((q) => q.id === "kb4"), k5 = plan.rooms.find((q) => q.id === "k5");
const d8 = plan.doors.find((d) => d.between && d.between[0] === "kb4" && d.between[1] === "k5");
if (!kb4 || !k5 || !d8) throw new Error("the plan has no kb4 / k5 / their door");
const [dx0, dz0, dx1, dz1] = d8.rect; // x0, z0 (the k5 side), x1, z1 (the REST side)
const [kx0, kz0, kx1, kz1] = k5.rect;
const eyes = [];
{
  const [x0, z0, x1, z1] = kb4.rect;
  for (let x = x0 + 1; x <= x1 - 1 + 1e-9; x += 1) for (let z = z0 + 1; z <= z1 - 1 + 1e-9; z += 1) eyes.push([x, EYE, z]);
  for (let x = dx0 + 0.5; x <= dx1 - 0.5 + 1e-9; x += 0.5) eyes.push([x, EYE, dz1]); // standing at the door's mouth
}
function throughDoor(e, q) {
  for (const zp of [dz0, dz1]) {
    const t = (zp - e[2]) / (q[2] - e[2]);
    if (!(t > 0 && t < 1)) return false;
    const x = e[0] + (q[0] - e[0]) * t, y = e[1] + (q[1] - e[1]) * t;
    if (x < dx0 || x > dx1 || y < 0 || y > d8.height) return false;
  }
  return true;
}
// a wall prop's face toward the room (the wall it sits on), or null (hung in the room)
function faceOf(p) {
  const [x, , z] = p.at;
  if (Math.abs(z - kz1) < 0.6) return [0, 0, -1]; // the door's wall: its props face into k5, away from REST
  if (Math.abs(z - kz0) < 1.2) return [0, 0, 1]; // the far wall: faces the door
  if (Math.abs(x - kx0) < 0.6) return [1, 0, 0];
  if (Math.abs(x - kx1) < 0.6) return [-1, 0, 0];
  return null;
}
function seen(p) {
  const yaw = ((p.yawDeg || 0) % 180 + 180) % 180;
  const sx = yaw === 90 ? p.size[2] : p.size[0], sz = yaw === 90 ? p.size[0] : p.size[2];
  const n = faceOf(p);
  let hits = 0;
  for (let i = 0; i < 5; i++) for (let j = 0; j < 3; j++) for (let k = 0; k < 5; k++) {
    const q = [p.at[0] + sx * (i / 4 - 0.5), p.at[1] + p.size[1] * (k / 4 - 0.5), p.at[2] + sz * (j / 2 - 0.5)];
    for (const e of eyes) {
      if (n && n[0] * (e[0] - q[0]) + n[2] * (e[2] - q[2]) <= 0) continue;
      if (throughDoor(e, q)) { hits++; break; }
    }
  }
  return hits;
}

// ---------------------------------------------------------------- each prop's job (V-4) or its cut
const GENERIC = new Set(["wash", "skirt", "cornice", "rib", "recess", "pilaster", "edgeLight"]);
const WORLD = {
  hero: { job: "hero", why: "the kiln furnace on the far wall, framed by the boss door from REST: the KILN'S HEART the worm guards (the arena's KILN MOUTH answers it inside)" },
  banner: { job: "world", why: "a founders' banner on the far wall, seen through the boss door: whose kiln this was" },
  pipes: { job: "world", why: "the kiln's feed pipe along the far wall, seen through the boss door" },
  lamp: { job: "world", why: "a lamp hung over the firing floor (V-5: a light on a real fixture), seen through the boss door" },
  chain: { job: "world", why: "the chain the lamp hangs from (V-5)" },
  truss: { job: "world", why: "the roof truss the lamp's chain hangs from (V-5)" },
  debris: { job: "world", why: "slag spilled at the furnace's foot: the crew ran" },
  scorch: { job: "world", why: "a scorch mark at the furnace's foot: the worm broke out here" },
  ingots: { job: "world", why: "castings left stacked at the furnace's foot: the firing was cut short" },
  kilnArch: { job: "world", why: "a kiln arch in the side wall" },
  doorFrame: { job: "route", why: "the boss door's frame (A-8)" },
  lintelLight: { job: "route", why: "the boss door's lit lintel (A-8)" },
};
const UNSEEN = "unseen: no line from 3.4 REST through door d8 reaches it (Campaign24.Enter moves the player into the worm's arena on the first step past d8)";
const GENERIC_WHY = "generic dress33 trim: tells no route, threat or world (V-4: never a strip or a skin because the wall was empty)";

const k5Props = plan.props.filter((p) => p.room === "k5");
if (k5Props.length !== 76) throw new Error("n43's k5 has " + k5Props.length + " props, expected the audit's 76");
const verdict = new Map();
for (const p of k5Props) {
  const s = seen(p);
  if (GENERIC.has(p.kind)) verdict.set(p.id, { keep: false, seen: s, reason: GENERIC_WHY });
  else if (!WORLD[p.kind]) throw new Error(p.id + ": kind " + p.kind + " has no rule");
  else if (s === 0) verdict.set(p.id, { keep: false, seen: s, reason: UNSEEN });
  else verdict.set(p.id, { keep: true, seen: s, job: WORLD[p.kind].job, why: WORLD[p.kind].why });
}
// a hung lamp is one fixture: its truss, chain and lamp (the same z) stay or go together (a lamp never floats without its chain)
const hung = (p) => ["truss", "chain", "lamp"].includes(p.kind);
for (const p of k5Props.filter(hung)) {
  const group = k5Props.filter((q) => hung(q) && Math.abs(q.at[2] - p.at[2]) < 0.01);
  const any = group.some((q) => verdict.get(q.id).keep);
  for (const q of group) {
    const v = verdict.get(q.id);
    if (any && !v.keep) verdict.set(q.id, { keep: true, seen: v.seen, job: WORLD[q.kind].job, why: WORLD[q.kind].why + " (kept with its fixture's seen part)" });
  }
}
const kept = k5Props.filter((p) => verdict.get(p.id).keep);
const cut = k5Props.filter((p) => !verdict.get(p.id).keep);
const props45 = {
  why: "NIGHT45 A3 (A-9, V-4; the audit's k5: 0 of 76 props with a reason): each of n43's 76 k5 props kept with its job (SEEN through the boss door from 3.4 REST and telling the kiln's world) or cut with its reason. `seen` = the prop's sample points visible from REST (of 75).",
  kept: kept.map((p) => ({ id: p.id, kind: p.kind, seen: verdict.get(p.id).seen, job: verdict.get(p.id).job, why: verdict.get(p.id).why })),
  cut: cut.map((p) => ({ id: p.id, kind: p.kind, seen: verdict.get(p.id).seen, reason: verdict.get(p.id).reason })),
};
const tagged = (p) => Object.assign({}, p, { job: verdict.get(p.id).job, why: verdict.get(p.id).why });

// ---------------------------------------------------------------- the k5 row (both files)
const cr39 = A.crumble39;
for (const i of cr39.boxes.concat(cr39.keep)) if (!A.collision.boxes[i]) throw new Error("crumble39 index " + i + " is not in " + ARENA);
function editRow(row) {
  if (row.id !== "k5" || row.boss !== "wyrm") throw new Error("not the worm's room");
  if (row.wyrmArena !== "Design/maps/wyrm43-arena.json") throw new Error("k5's arena is " + row.wyrmArena + ", expected wyrm43");
  row.wyrmArena = ARENA;
  row.wyrmArena_why = "NIGHT45 A3 (T-65; the identity card THE KILN'S HEART, docs/ROOMS.md): " + A._why + " Before it: " + row.wyrmArena_why;
  const cr = row.crumble31;
  cr.arena = ARENA;
  cr.boxes = cr39.boxes.slice();
  cr.keep = cr39.keep.slice();
  cr._why = cr._why + " NIGHT45 A3: re-pointed at " + ARENA + " (wyrm43's boxes in order: the same indices).";
  cr.draw.boxes = cr39.draw.map((b) => Object.assign({}, b, { rect: [r3(b.rect[0] + cr.draw.center[0]), r3(b.rect[1] + cr.draw.center[1]), r3(b.rect[2] + cr.draw.center[0]), r3(b.rect[3] + cr.draw.center[1])] }));
  row.identity45 = "docs/ROOMS.md, NIGHT45 identity cards: k5 3.5 THE RING: THE KILN'S HEART (DESIGN-RULES A-9); the arena " + ARENA + " (plates45, solids45)";
  row.props45 = props45;
  return row;
}
function editLevel(obj, propsKey) {
  const rooms = obj.rooms;
  const i = rooms.findIndex((q) => q.id === "k5");
  editRow(rooms[i]);
  const before = obj[propsKey].length;
  obj[propsKey] = obj[propsKey].filter((p) => p.room !== "k5" || verdict.get(p.id).keep).map((p) => (p.room === "k5" ? tagged(p) : p));
  return before - obj[propsKey].length;
}

// the plan
plan.id = "n45-kiln";
plan._why = "NIGHT45 builder A3 (design.n45.kiln45 swaps n43-kiln for this level): Tools/night/gen-n45-kiln.cjs, n43-kiln's plan with k5 3.5 THE RING rebuilt from its identity card (THE KILN'S HEART: the arena "
  + ARENA + ", the 76 props kept with a job or cut). Before it: " + plan._why;
const cutPlan = editLevel(plan, "props");
fs.mkdirSync(path.join(ROOT, "Design/levels/n45"), { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");

// the finished level (n43-kiln.json is levelgen's output + gen-n43-kiln.cjs --finish: data.name n40-kiln, the older systems' name, kept)
level.name = "n45-kiln";
level.generatedBy = "Tools/night/gen-n45-kiln.cjs (n43-kiln.json with k5 3.5 THE RING rebuilt: the arena " + ARENA + ", the props kept with a job or cut; plan " + PLAN_OUT + ")";
const cutLevel = editLevel(level.campaign, "props");
if (cutLevel !== cutPlan) throw new Error("the plan and the level cut different props (" + cutPlan + " / " + cutLevel + ")");
const eol = levelText.includes("\r\n") ? "\r\n" : "\n";
const out = JSON.stringify(level) + (levelText.endsWith(eol) ? eol : "");
for (const f of LEVEL_OUTS) fs.writeFileSync(path.join(ROOT, f), out);

const count = (list) => list.reduce((m, p) => ((m[p.kind] = (m[p.kind] || 0) + 1), m), {});
console.log(`n45-kiln: k5 -> ${ARENA}; props: kept ${kept.length} ${JSON.stringify(count(kept))}, cut ${cut.length} (unseen ${props45.cut.filter((c) => c.reason === UNSEEN).length}, generic ${props45.cut.filter((c) => c.reason === GENERIC_WHY).length}) ${JSON.stringify(count(cut))}`);
for (const p of props45.kept) console.log(`  KEEP ${p.id} (${p.kind}, seen ${p.seen}/75): ${p.job}`);
console.log(`wrote ${PLAN_OUT}, ${LEVEL_OUTS.join(", ")} (${out.length} bytes; data.name ${level.data.name})`);

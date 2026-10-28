// NIGHT46 builder B1 (NIGHT46.md card B1, DECIDE46 N-10; docs/ROOMS.md "NIGHT46 identity cards", k5 THE KILN'S HEART, walked into):
// THE KILN WYRM's arena for n46-kiln, where the worm's room IS level 3's k5 (walkin46). From Design/maps/wyrm45-arena.json (NIGHT45 A3's 33
// boxes, untouched) -> Design/maps/wyrm46-arena.json (+ the StreamingAssets copy). Every box keeps its index (crumble31's 27 / 28 and keep 23,
// plates45's 29-32 still point at the same solids). What changes, each for one card line:
//   1. box 0, the floor: UNCHANGED ([-32,-0.5,-32]..[32,0,32]). n46-kiln's k5 floor is a pit floor at -0.5 (gen-n46-kiln.cjs), so the ring's
//      floor box sits ON it: its top (y 0) is the walk surface in the fight, and no face of it lies in the level floor's plane (no z-fight when
//      both are drawn).
//   2. box 2, the south shell wall: cut at d8's opening (x -2..2, 4.5 m tall: the level's door): box 2 keeps the west part, the east part and
//      the lintel are appended (33, 34). Card: "the south shell wall gets a 4 x 4.5 m door gap ... the player now walks through it".
//   3. `walkIn46`: where the arena sits in the world when walkin46 is on (SurvivalSim.N46.Wyrm.cs reads it): origin (0, 0, -327.5), so the
//      arena's south wall's outer face (local z 32) is k5's door edge (z -295.5) and the spawn (local z 27.5) is (0, 0, -300).
// Run (from the repo or worktree root): node Tools/night/gen-n46-wyrm-arena.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const IN = "Design/maps/wyrm45-arena.json";
const OUTS = ["Design/maps/wyrm46-arena.json", "Assets/StreamingAssets/Design/maps/wyrm46-arena.json"];
const DOOR = { x0: -2, x1: 2, top: 4.5 }; // n45-kiln's d8: rect [-2,-295.5,2,-291.5], height 4.5 (campaign.doors)
const K5_DOOR_Z = -295.5; // k5's door edge: d8's rect z min (= the k5 rect's z1 in n45-kiln)

const text = fs.readFileSync(path.join(ROOT, IN), "utf8");
const A = JSON.parse(text);
const boxes = A.collision.boxes;
if (boxes.length !== 33) throw new Error(`${IN}: ${boxes.length} boxes, expected 33 (NIGHT45 A3's)`);
const south = boxes[2];
if (JSON.stringify(south.min) !== "[-32,0,30]" || JSON.stringify(south.max) !== "[32,30,32]") throw new Error("box 2 is not the south shell wall");
const outerZ = south.max[2]; // 32
const origin = [0, 0, K5_DOOR_Z - outerZ]; // -327.5
// the doorway must be clear inside the ring: no box in x -2..2, y 0..4.5 from the wall to the spawn
for (let i = 0; i < boxes.length; i++) {
  if (i === 0 || i === 2 || i === 5) continue;
  const b = boxes[i];
  const hit = b.min[0] < DOOR.x1 && b.max[0] > DOOR.x0 && b.min[1] < DOOR.top && b.max[1] > 0 && b.max[2] > A.playerSpawn.z - 0.5 && b.min[2] < outerZ;
  if (hit) throw new Error(`box ${i} stands in the doorway`);
}

boxes[2] = { min: [-32, 0, 30], max: [DOOR.x0, 30, 32], kind: "wall" };
boxes.push({ min: [DOOR.x1, 0, 30], max: [32, 30, 32], kind: "wall" });
boxes.push({ min: [DOOR.x0, DOOR.top, 30], max: [DOOR.x1, 30, 32], kind: "wall" });

const S = A.solids45.boxes;
const s2 = S.find((s) => s.index === 2);
const why2 = "the ring's south wall, WEST of d8's opening (NIGHT46 B1: the player walks in through it; k5 card: Every-solid rule)";
if (s2) s2.why = why2 + "; before: " + s2.why;
S.push({ index: 33, kind: "wall", job: "route", why: "the ring's south wall, EAST of d8's opening (NIGHT46 B1, walkin46: the door gap, x -2..2)" });
S.push({ index: 34, kind: "wall", job: "route", why: "the lintel over d8's opening (4.5 m: the level's door height) (NIGHT46 B1)" });

A._why = "NIGHT46 B1 (walkin46; Tools/night/gen-n46-wyrm-arena.cjs): wyrm45-arena.json with d8's door cut in the south wall (box 2 = its west part; 33 east, 34 the lintel; every other index unchanged) and walkIn46 (where it sits on n46-kiln's k5). Before it: " + A._why;
A.walkIn46 = {
  _why: "NIGHT46 B1 (DECIDE46 N-10, docs/ROOMS.md k5 card): with design.n46.walkin46 on, the worm's room is placed here (world) instead of design.n24.wyrm.room.origin, so n46-kiln's k5 IS the arena (rect [-32,-359.5,32,-295.5]) and Campaign24 keeps the player where he walked in (SurvivalSim.Boss36WalkIn: no Respawn, the speed kept). Read by SurvivalSim.N46.Wyrm.cs.",
  origin,
  door: { x0: DOOR.x0, x1: DOOR.x1, top: DOOR.top, z: K5_DOOR_Z },
};

const eol = text.includes("\r\n") ? "\r\n" : "\n";
const out = JSON.stringify(A, null, 2).replace(/\n/g, eol) + eol;
for (const f of OUTS) fs.writeFileSync(path.join(ROOT, f), out);
console.log(`gen-n46-wyrm-arena: ${boxes.length} boxes (door cut: 2 / 33 / 34), origin [${origin}] -> ${OUTS.join(", ")}`);

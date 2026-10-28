// NIGHT46 builder B1 (NIGHT46.md card B1; DECIDE46 N-10; FLOW-AUDIT rows 2 and 7; docs/ROOMS.md "NIGHT46 identity cards", k5): LEVEL 3 as n46-kiln.
// design.n46.kiln46 swaps n45-kiln for it (NightRows.BaseId: n46-kiln -> n45-kiln -> n43-kiln, so every older system that knows level 3 by id
// keeps it). n45-kiln, its plan and its generator are untouched, so --off46 loads n45-kiln bit for bit.
//
// From n45's PLAN (Design/levels/n45/kiln.plan.json -> Design/levels/n46/kiln.plan.json), then levelgen, then --finish. What changes, each named
// by its card line:
//   K5 (walkin46, card "The walk-in choice: option A"): k5's rect becomes the worm arena's footprint [-32,-359.5,32,-295.5] (64 x 64 m outside its
//      shell; nothing of level 3 lies past z -295.5), 32 m tall (the arena's roof at 30-32 stays under the level's), its spawn the arena's
//      (0, 0, -300); its floor a pit floor at -0.5 (the arena's floor box [-.5, 0] sits on it: no shared face); wyrmArena =
//      Design/maps/wyrm46-arena.json (Tools/night/gen-n46-wyrm-arena.cjs: d8's door cut in its south wall, walkIn46.origin (0, 0, -327.5));
//      crumble31 re-pointed at it (the same indices). The 24 props and lights L1 / L2 / L4 of the old 26 x 26 m room are cut (their walls and
//      floor are gone: they would stand in the ring or under its floor); L3 moves to the ring's centre (the ceiling light). floorSize 668 -> 728.
//   KB1B / KB2B / KB3B (card "Level 3's cliff doors", FLOW-AUDIT row 7): each east noride liner is opened from 2 m past the door to the cliff face
//      for a ride panel (ride: true, cyan strip, top 6 m, 0.6 m proud of the wall) that ends flush with the cliff: the climb (the THUNDERCLAP's
//      shot / double / shot, VERBS rows 29 / 30) starts from a ride at speed (VERBS rows 9 / 11), not from a stop. The liner stays above the
//      panel and past the cliff (no wall jumps up the side: the gate's 12 m stays the climb's). kb2b's second cliff (G2b, the 24 m tier) is cut:
//      one 12 m climb, then the drop to the exit.
// Run (from the repo or worktree root):
//   node Tools/night/gen-n46-wyrm-arena.cjs && node Tools/night/gen-n46-kiln.cjs
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n46/kiln.plan.json Design/levels/n46/kiln.json
//   node Tools/night/gen-n46-kiln.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n45/kiln.plan.json", PLAN_OUT = "Design/levels/n46/kiln.plan.json";
const GEN_OUT = "Design/levels/n46/kiln.json";
const ARENA = "Design/maps/wyrm46-arena.json";
const CARD = "docs/ROOMS.md, NIGHT46 identity cards: k5 3.5 THE RING: THE KILN'S HEART, walked into (builder B1)";

// ---------------------------------------------------------------- the finish step (after levelgen): the game's copies
if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n46-kiln") throw new Error(`${src}: name ${level.name}, expected n46-kiln`);
  level.data.name = "n40-kiln"; // the older systems' name for level 3 (as n45-kiln's: MapModelJson reads the outer name first)
  level.generatedBy = "Tools/night/gen-n46-kiln.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  const outs = ["Design/levels/n46-kiln.json", "Assets/StreamingAssets/Design/levels/n46-kiln.json"];
  for (const f of outs) fs.writeFileSync(path.join(ROOT, f), out);
  console.log(`n46-kiln: finished ${path.relative(ROOT, src)} -> ${outs.join(", ")} (${out.length} bytes)`);
  process.exit(0);
}

const plan = JSON.parse(fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8"));
if (plan.id !== "n45-kiln") throw new Error(`${PLAN_IN}: id ${plan.id}`);
plan.id = "n46-kiln";
plan._why = "NIGHT46 B1 (Tools/night/gen-n46-kiln.cjs; " + CARD + "): n45-kiln's plan with k5 rebuilt as the worm's own arena (walked into) and the three cliff links given a ride run-up. Before it: " + plan._why;
const room = (id) => {
  const r = plan.rooms.find((x) => x.id === id);
  if (!r) throw new Error("no room " + id);
  return r;
};
const one = (pred, what) => {
  const i = plan.boxes.findIndex(pred);
  if (i < 0 || plan.boxes.findIndex((b, j) => j !== i && pred(b)) >= 0) throw new Error("expected exactly one " + what);
  return i;
};
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);

// ---------------------------------------------------------------- K5: the worm's arena, walked into
const A = JSON.parse(fs.readFileSync(path.join(ROOT, ARENA), "utf8"));
const O = A.walkIn46.origin; // [0, 0, -327.5]
const half = 32; // the shell's outer faces (the arena's boxes reach +-32)
const k5 = room("k5");
const d8 = plan.doors.find((d) => d.id === "d8");
if (!eq(k5.rect, [-13, -321.5, 13, -295.5]) || !eq(d8.rect, [-2, -295.5, 2, -291.5])) throw new Error("k5 / d8 moved since n45: re-read");
if (O[2] + half !== d8.rect[1]) throw new Error("the arena's south face is not d8's edge");
k5.rect = [O[0] - half, O[2] - half, O[0] + half, O[2] + half];
k5.height = 32;
k5.spawn = { at: [A.playerSpawn.x + O[0], 0, A.playerSpawn.z + O[2]], yawDeg: A.playerSpawn.yawDeg };
// the map's square (floorSize: the nav grid and the ground span +-floorSize / 2): n45's 668 (+-334) ends inside the ring (z -359.5, its shell
// to -361.5); the square grows to reach it (+-364)
if (plan.floorSize !== 668) throw new Error("floorSize " + plan.floorSize);
plan.floorSize = 2 * Math.ceil(Math.max(-(O[2] - half - 2), 0) + 2);
plan.floorSize_why = "NIGHT46 B1: the map's square grows from 668 to reach k5's ring (z to -361.5 with its shell): the nav grid must cover the worm's room now that it is the level's own";
k5.exit = [0, 0, O[2] - 20]; // the boss room's exit spot (the clear comes from the worm's death); 20 m past the core, on the nav grid
k5.wyrmArena = ARENA;
k5.wyrmArena_why = "NIGHT46 B1 (walkin46; " + CARD + "; Tools/night/gen-n46-wyrm-arena.cjs): wyrm45-arena.json with d8's door cut in its south wall (box 2 west, 33 east, 34 the lintel; every other index unchanged) and walkIn46.origin " + JSON.stringify(O) + ": with design.n46.walkin46 on the worm's room is THIS room (no teleport, the speed kept). Before it: " + k5.wyrmArena_why;
const c31 = k5.crumble31;
c31.arena = ARENA;
c31._why += " NIGHT46 B1: re-pointed at " + ARENA + " (the same indices); draw at the walk-in origin.";
const dz = O[2] - c31.draw.center[1];
c31.draw.center = [O[0], O[2]];
for (const b of c31.draw.boxes) b.rect = [b.rect[0], b.rect[1] + dz, b.rect[2], b.rect[3] + dz];
k5.proofs = [{ kind: "walk", points: [[0, 0, k5.spawn.at[2]], [0, 0, O[2] + 10]] }];
k5.frames36 = { mid: [0, 0, k5.spawn.at[2], 0, 0.154], exit: [0, 0, O[2] - 20, 0, -0.149] };
k5.identity46 = CARD;
// the old room's props: every one stood on a wall or floor spot of the 26 x 26 m room that the ring replaces
const cut = plan.props.filter((q) => q.room === "k5");
if (cut.length !== 24) throw new Error(`k5 props ${cut.length}, expected 24 (NIGHT45's kept list)`);
plan.props = plan.props.filter((q) => q.room !== "k5");
k5.props46 = { why: "NIGHT46 B1: the old 26 x 26 m room's 24 kept props are cut: their walls and floor spots are inside the ring now (the worm's routes and the fight's floor); the ring is dressed by its own views (WyrmRoomView, WyrmRing43View: THE KILN MOUTH, the crucible)", cut: cut.map((q) => q.id) };
const L = (id) => plan.lights.find((l) => l.id === id);
for (const id of ["k5-L1", "k5-L2", "k5-L3", "k5-L4"]) if (!L(id)) throw new Error("no light " + id);
// L1 / L2 were on the old room's walls (x +-12.95): in the ring now; L4 was its floor pool: the ring's floor is the arena's box over the
// level's floor, so a pool on the level's floor would light from under it (the ring's core light is WyrmRing43View's)
plan.lights = plan.lights.filter((l) => l.id !== "k5-L1" && l.id !== "k5-L2" && l.id !== "k5-L4");
Object.assign(L("k5-L3"), { at: [O[0], 31.95, O[2]], size: [8, 8, 0.1], range: 40, why: "NIGHT46 B1: the ring's ceiling light at its centre (the old room's moved with the room)" });
// kb2b's three lights on G2b (its lip strip, its face lamp, its top pool) go with the cut cliff
const g2bLights = ["kb2b-L6", "kb2b-L7", "kb2b-L8"];
for (const id of g2bLights) if (!L(id) || L(id).at[1] < 18) throw new Error("kb2b light " + id + " is not G2b's");
plan.lights = plan.lights.filter((l) => !g2bLights.includes(l.id));
plan.pits.push({
  zone: "k5", rect: k5.rect.slice(), floorY: -0.5, job: "line",
  why: "NIGHT46 B1: k5's floor 0.5 m low under the ring's own floor box (wyrm46-arena.json box 0, top 0, on in the fight): no two floors share a plane (no z-fight when the ring is drawn); before the fight nobody stands here (the fight begins on the first step past d8)",
  note: "NIGHT46 B1: the floor under the ring's floor",
});

// ---------------------------------------------------------------- the cliff links: a ride run-up into each climb
function rideRunUp(zone, cliffFaceZ, yLow, why) {
  const r = room(zone);
  const z1 = r.rect[3], zFrom = z1 - 2; // 2 m past the door (the door's own jamb keeps its liner)
  const li = one((b) => b.zone === zone && b.kind === "hull" && b.noride && b.min[0] === 7.7, zone + " east liner");
  const liner = plan.boxes[li];
  const top = 6;
  const keep = (min, max, note) => ({ ...liner, min, max, why: liner.why + " (" + note + ")", note: liner.note + " (" + note + ")" });
  const parts = [
    keep([7.7, liner.min[1], liner.min[2]], [8, 32, cliffFaceZ], "NIGHT46 B1: the cliff and the landing"),
    keep([7.7, top, cliffFaceZ], [8, 32, zFrom], "NIGHT46 B1: above the ride panel"),
    keep([7.7, liner.min[1], zFrom], [8, top, z1], "NIGHT46 B1: the door's jamb"),
  ];
  const panel = {
    zone, kind: "wall", min: [7.4, yLow, cliffFaceZ], max: [8, top, zFrom], job: "line",
    why: why + " (" + CARD + "; VERBS rows 9 / 11 into 29 / 30)", note: why, ride: true,
    ride36: { face: "W", planeX: 7.4, lz: [z1 - zFrom, z1 - cliffFaceZ] },
  };
  plan.boxes.splice(li, 1, ...parts, panel);
  return panel;
}
rideRunUp("kb1b", -57, 0, "kb1b THE FLUE's RUN-UP (cyan): ride the east wall from the door into G1's cliff face, jump off, then the climb (shot, double, shot) from speed");
// kb2b: G2b (the 24 m tier) cut first, then the run-up into G2a
const g2b = one((b) => b.zone === "kb2b" && b.kind === "deck" && eq(b.min, [-8, -1, -152]) && eq(b.max, [8, 24, -147]), "kb2b G2b");
plan.boxes.splice(g2b, 1);
rideRunUp("kb2b", -135, 0, "kb2b THE STACK's RUN-UP (cyan): ride the east wall from the door into G2a's cliff face, then one 12 m climb (G2b's second 12 m cut: FLOW-AUDIT row 7)");
const kb2b = room("kb2b");
kb2b.verb36 = "one climb from a ride: 12 m, then the drop to the door (NIGHT46 B1: G2b cut)";
kb2b.routeVia = [kb2b.routeVia[0]];
kb2b.proofs = kb2b.proofs.filter((q) => !q.points.some((pt) => pt[1] === 24)).concat([{ kind: "walk", points: [[0, 0, -149], [0, 0, -157.5]] }]);
kb2b.frames36 = { mid: kb2b.frames36.mid, exit: [0, 12, -145.5, 0, -0.4] };
rideRunUp("kb3b", -260.5, -4.9, "kb3b THE SPOUT's RUN-UP (cyan): ride the east wall from the door over the 4 m pit into G3's cliff face: CLIMB IT ON THE RUN, from a run");

// ---------------------------------------------------------------- KB3: no burrower across the pour (FLOW-AUDIT row 11; F1's meter 2026-10-17)
// The burrower's head moves only in a straight line over floor at its own height (Core/Enemies/Burrower43.cs Hunt / FloorAt) and must come
// within triggerRange (1.2 m) of a grounded player's feet: kb3's 16 x 10 m pour in the middle of the room stops it whenever the player stands
// on its other side, and the room's door waits for it. n46flow --rooms kb3 (n46-kiln, 10 seeds): 8 / 10 capped at 240 s with the last body a
// burrower at y -6 on the pour's edge. The burrower is taught and tested in kb2c (no pour there); kb3 keeps its two roles a wave with a body
// that can reach the player round the pour: wave 2 the charger with the spitters (the charge that misses goes into the pour: kb2's rule),
// wave 3 husks under the drones.
{
  const kb3 = room("kb3");
  if (!eq(kb3.waves, [["spitter", "spitter", "husk", "husk", "husk"], ["burrower", "spitter", "spitter"], ["burrower", "drone", "drone", "drone"]])) throw new Error("kb3's waves moved since n45");
  kb3.waves = [kb3.waves[0], ["charger", "spitter", "spitter"], ["husk", "husk", "drone", "drone", "drone"]];
  kb3.roles43 = ["ranged + fodder", "charger + ranged", "fodder + flyer"];
  kb3.waves_why = "NIGHT46 B1 (FLOW-AUDIT row 11; n46flow: 8 / 10 seeds held 240 s by a burrower under the floor at the pour's edge): no burrower across the pour (its head cannot go round it); the burrower stays kb2c's";
  kb3.verb36 = "ride over the pour, climb to the ladle; the charger drives you off the floor into the spitters' globs, then the husks under the drones (NIGHT46 B1: the burrower is kb2c's)";
}

const dir = path.join(ROOT, path.dirname(PLAN_OUT));
if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n46-kiln: ${PLAN_OUT} (k5 ${JSON.stringify(k5.rect)}, spawn ${JSON.stringify(k5.spawn.at)}; ${cut.length} props cut; 3 ride run-ups; kb2b G2b cut)`);

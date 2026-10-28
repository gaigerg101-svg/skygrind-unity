// NIGHT47 builder L (design.n47.span47; NIGHT47.md G39 "a new arena that makes movement the answer", G40; Research/2026-10-18-wave47/L-cards.md
// section 3 THE LOOM FRAME, the card gate's PASS; DESIGN-RULES A-9 + arena-flow-web.md Part D): level 4 as n47-span: n46-span's plan
// (Design/levels/n46/span.plan.json, written by Tools/night/gen-n46-span.cjs, which this file does not run or edit) copied whole, with ONLY o3 (THE
// LOOM's room) rebuilt: the 44 x 140 m canyon crossing (three tower pairs along a straight void, the fight travelling 100 m) becomes THE LOOM FRAME,
// a 44 x 56 m well (GG 2026-10-09: "a lot of the rooms to be a lot smaller and more action packed"). Every other room, door, light and piece is
// n46-span's bit for bit (the guard at the end throws otherwise).
//
// What the card says and what round 1 builds (the measured adaptations, each named):
//   - the well: the same 44 m width (THE LOOM's feet span 21 m from the centre line x -17: Loom37Params.footSpanX), 56 m long (z -250.5 ..
//     -306.5): an 8 m start ledge (the door d7, unchanged), a 48 m void to the north wall (no far ledge: see Z_END; the exit is the start ledge).
//   - the holds: three PAIRS of posts, one pair per phase, in the order the fight goes forward: P1 the near posts (z -268), P2 the side galleries on
//     the walls (z -282), P3 the far posts (z -296). Adapted from the card's "four corner posts, the far two falling at P2": THE LOOM's phase rule
//     and its pilot work by pairs (the leg's pair of knees faces the leg's pair of ledges: Loom37.ActiveKnee, Loom37Pilot's t<leg>w / t<leg>e), and
//     its anchor cut rule cuts the anchors behind the ledge you last stood on (SurvivalSim.Loom37.Boss.cs), so the pairs run forward. The posts'
//     fall at a phase change (a layer swap) is round 2.
//   - every pair's tops are 6 m over the ledges (y 6: the double jump's 2.7 m does not reach them; the first hook is the commit) and under the
//     floor sweep's phase-1 top (design.n41.sweep41.topY 3? no: P1's sweep covers up to y 3, so the posts are its answer; P2-P3's 11 covers them:
//     the answer is a hook or a ride).
//   - THE LOOM hangs in the middle over the void: its stations (one per phase) are where its leg's pair of knees sits 7-8 m in front of that phase's
//     holds (Loom37.Pose: pair p's knees at the station + (1 - p) x kneeSpreadZ 7 along z, +-kneeSpanX 9 across, kneeDropY 6 under the body).
// Coordinates: web (x east, y up, z south); the level's floor is y 0 (the void cuts it); levelgen doors carry no height.
// Run from the repo (or worktree) root: bash Tools/night/n47-span-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n46/span.plan.json");
const OUT = path.join(ROOT, "Design/levels/n47/span.plan.json");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n46-span") throw new Error(`unexpected source id ${plan.id}`);
const before = JSON.parse(JSON.stringify(plan));
plan.id = "n47-span";
plan._why = "NIGHT47 builder L (design.n47.span47 swaps n46-span for this file while the row is on): Tools/night/gen-n47-span.cjs copies n46-span's plan "
  + "whole and rebuilds only o3, THE LOOM's room, as THE LOOM FRAME (L-cards.md section 3). n46-span's _why: " + plan._why;

const REBUILT = new Set(["o3"]);
const ownerOf = (e) => e.zone || e.room || null;
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const why = (o, job, text) => Object.assign(o, { job, why: text, note: text });

// ---- the frame's numbers (world metres) -------------------------------------------------------------------------------------------------------------
const X0 = -39, X1 = 5, CX = -17;            // the well's walls (unchanged width) and its centre line
const Z_DOOR = -250.5, Z_START = -258.5;     // the start ledge (the door d7 at z -250.5, unchanged)
// the room's north wall. No far ledge (round 1, MEASURED: a dive knocked the pilot onto a far ledge behind the last pair, where the anchor cut
// rule had darkened every ring behind it: 7 of 10 seeds capped in phase 3); the void runs to the north wall and the exit is the start ledge
// (the room is the level's last: the kill ends it)
const Z_END = -306.5;
const VOID_Y = -24, TOP = 6, ROOF_H = 30;    // the void's floor (the canyon's), the holds' tops, the open room's walls
const PAIRS = [
  // pair, ledge ids, rects [x0, z0, x1, z1] (z0 the north edge), the anchor ring over each (x, y, z), the station THE LOOM hangs at for the phase
  // every hold 8 m across x 6 m (round 2; round 1 had 5 x 5: MEASURED, the lash dodge (a 4 m sidestep across the line) and the spool strafe stepped
  // off a 5 m post: 88 falls in n47lhuman seed 21, a cap); the canyon's towers were 5 x 14 along its line, the frame's holds face the machine across x
  { pair: 1, z: -268, w: [-35, -271, -27, -265], e: [-7, -271, 1, -265], station: [CX, 20, -282.5],
    why: "P1 THE FRAME's near posts: the first hook's landing from the start ledge (7 m of void), the phase-1 rest (the sweep's top y 3 is under them)" },
  // P2's holds are posts 20 m apart in the middle (round 1 had wall galleries 40 m apart: MEASURED, the across pull a tear needs, gallery to gallery,
  // was out of the grapple's 40 m and the human-model pilot sat 250 s on one gallery with one knee left)
  { pair: 2, z: -282, w: [-31, -285, -23, -279], e: [-11, -285, -3, -279], station: [CX, 20, -289],
    why: "P2 THE WEAVE's inner posts (20 m apart, under THE LOOM's flank): the posts behind are cut off (the anchor cut rule), the sweep covers them" },
  { pair: 3, z: -296, w: [-35, -299, -27, -293], e: [-7, -299, 1, -293], station: [CX, 20, -296.5],
    why: "P3 THE UNRAVEL's far posts: the last pair, the void and the frame beam behind them" },
];

// ---- o3: the room row --------------------------------------------------------------------------------------------------------------------------------
const o3 = plan.rooms.find((r) => r.id === "o3");
if (!o3) throw new Error("o3 missing");
const oldO3 = JSON.parse(JSON.stringify(o3));
o3.name = "4.10 THE NARROWS";
o3.rect = [X0, Z_END, X1, Z_DOOR];
o3.height = ROOF_H;
// roofed at the walls' 30 m (round 1, MEASURED: open, a wall pull put the human-model pilot on the north wall's top: the canyon's walls were far from
// its play, the frame's are 4-6 m from the holds); the well under it stays dark-to-dusk by its lights
o3.roof = true;
o3.exit = [CX, 0, Z_START + 2];
o3.verb36 = "the BEND: hook a lit knee, shoot its seam in the slow motion or pull away (the exam of level 4's grapple)";
o3.span47_why = "L-cards.md section 3 THE LOOM FRAME (G39): the canyon crossing (140 m) cut to a 56 m well round THE LOOM; three pairs of holds, one a phase, forward";

const anchors = [], ledges = [], towers = [], webWalls = [], nodes = [];
const node = (at) => { nodes.push(at); return nodes.length - 1; };
// NO mid-gap pair before the near posts (round 2, MEASURED: n47lhuman seed 14 sat 170 s on the start ledge after a fall: the hook's cone took the
// mid-gap ring over the void (a0w, 15 m) instead of the post's (t1w, 20.7 m), the pull ended over the void, the lip return put it back on the start
// ledge: 70 bends in phase 1, 7 deaths). From the start ledge the only rings in the cone are the near posts' own.
for (const P of PAIRS) {
  for (const [side, r] of [["w", P.w], ["e", P.e]]) {
    const id = `t${P.pair}${side}`;
    const cx = (r[0] + r[2]) / 2, cz = (r[1] + r[3]) / 2;
    const at = [cx, TOP + 5, cz + 1.5]; // over the hold's south half (the canyon's: 5 m over the top, toward the player)
    anchors.push({ id, node: node(at), at, pair: P.pair, ledge: id });
    ledges.push({ id, at: [cx, TOP, cz], rect: [r[0], r[1], r[2], r[3]], y: TOP, pair: P.pair });
  }
  towers.push({ pair: P.pair, rectW: P.w, rectE: P.e, top: TOP });
  webWalls.push({ min: [P.w[2], VOID_Y, P.z - 0.3], max: [P.e[0], 16, P.z + 0.3] });
}
ledges.unshift({ id: "start", at: [CX, 0, Z_START + 4], rect: [X0, Z_START, X1, Z_DOOR], y: 0, pair: 0 });
o3.grappleNodes = nodes;
o3.grapple = "THE FRAME's rings (L-cards.md section 3): one ring over each hold of the three pairs; THE LOOM's six knees join them as moving nodes (Game/SurvivalSim.Loom37.cs)";
o3.loom37 = {
  _why: "THE LOOM FRAME's data for Game/SurvivalSim.Loom37.cs (NIGHT47 builder L, gen-n47-span.cjs): anchors = the room's grappleNodes in order; ledges = the holds (a fall comes back to the last one stood on); stations = where THE LOOM hangs per phase (its leg's pair of knees 7-8 m in front of that phase's holds); webWalls = the sheet across each pair; all world metres",
  anchors, ledges, stations: PAIRS.map((P) => P.station), webWalls, towers,
};
o3.frames36 = { mid: [CX, 0, Z_START + 4, 0, 0.32], exit: [-31, TOP, -268, -20, 0.2] };
o3.loom40NoRide = {
  _why: "THE LOOM FRAME (NIGHT47): no ride wall refuses a ride (the card: the side walls and the holds' faces always ride; the frame's footing is the hook and the ride, never the floor). Kept as data with empty phase lists so loom40's switch finds the row.",
  rideWalls: 0, phase1: [], phase2: [],
};

// ---- o3: the pieces ------------------------------------------------------------------------------------------------------------------------------------
for (const key of ["boxes", "ramps", "pits", "lights", "props", "surfaces", "tunnels", "pads", "grapple"]) plan[key] = (plan[key] || []).filter((e) => ownerOf(e) !== "o3");
const boxes = [];
// THE STATE CHANGE (round 2, the card's "it cuts its frame: the posts fall"): P1's near posts are not level boxes but o3.loom47Posts, a world layer
// the sim stands at the run's start and takes away at P1 -> P2 with loom47 on (SurvivalSim.N47.Loom.cs; Loom37View draws them and their fall); with
// loom47 off they stand for the whole fight (the arena NIGHT46's LOOM would need)
o3.loom47Posts = PAIRS.filter((P) => P.pair === 1).flatMap((P) => [P.w, P.e]).map((r) => ({ min: [r[0], VOID_Y, r[1]], max: [r[2], TOP, r[3]] }));
o3.loom47Posts_why = "P1's near posts (L-cards.md section 3 State change): they stand until THE LOOM cuts its frame at P1 -> P2, then fall into the void (loom47 on)";
for (const P of PAIRS) for (const [side, r] of [["w", P.w], ["e", P.e]]) {
  if (P.pair === 1) continue;
  boxes.push(why({ zone: "o3", kind: "wall", min: [r[0], VOID_Y, r[1]], max: [r[2], TOP, r[3]], ride: true }, "line",
    `HOLD t${P.pair}${side}: ${P.why}; its faces ride (the post-to-post route), its top a hold (a fall comes back here)`));
}
// THE FRAME BEAM is a prop (no collider): MEASURED round 1 as a box, the human-model pilot's wall pull landed on its top (y 24) and climbed onto the
// open room's north wall top (y 30): 1 of 5 capped seeds stood there 280 s
const props = [{ id: "o3-beam", room: "o3", kind: "truss", at: [CX, 25, Z_END + 0.5], size: [X1 - X0, 2, 1], yawDeg: 0, collider: false, decor: true,
  job: "landmark", why: "THE FRAME BEAM (the card's landmark): the iron truss across the far wall that carries the warp up to the machine; seen from the door" }];
const pits = [why({ zone: "o3", rect: [X0, Z_END, X1, Z_START], floorY: VOID_Y }, "hazard",
  "THE VOID under the frame (the card's hazard): no floor past the start ledge; a fall is a lip return to the last hold (THE LOOM's fall rule)")];
const lights = [];
for (const P of PAIRS) for (const [side, r] of [["w", P.w], ["e", P.e]]) {
  if (P.pair === 1) continue; // P1's posts fall at P2: no fixed light floats over the void after (Loom37View lights them while they stand)
  lights.push({ id: `o3-t${P.pair}${side}`, room: "o3", kind: "pool", at: [(r[0] + r[2]) / 2, TOP + 0.025, (r[1] + r[3]) / 2], normal: [0, 1, 0], size: [3, 3, 0.05], color: P.pair === 1 ? "amber" : "white", range: 7, intensity: 1.3,
    job: "light", why: `the hold t${P.pair}${side}'s pool: a hold reads as a hold from the door` });
}
lights.push({ id: "o3-start", room: "o3", kind: "pool", at: [CX, 0.025, Z_START + 4], normal: [0, 1, 0], size: [4, 4, 0.05], color: "amber", range: 8, intensity: 1.3, job: "light", why: "the start ledge's pool, amber as the canyon's o3-L7 (round 2c: cyan here flooded the 44 x 8 m ledge floor and filled the lead's n47-r1c reveal and posts-fall frames with a flat cyan plane; cyan is for player-usable trims only)" });
const insertAt = (key, rows) => {
  const old = before[key] || [];
  const first = old.findIndex((e) => ownerOf(e) === "o3");
  const at = first < 0 ? plan[key].length : old.slice(0, first).filter((e) => ownerOf(e) !== "o3").length;
  plan[key].splice(at, 0, ...rows);
};
insertAt("boxes", boxes);
insertAt("pits", pits);
insertAt("lights", lights);
insertAt("props", props);

// ---- the reach check: every hook a phase needs, from the hold the pilot stands on, under 0.9 x the grapple's range -----------------------------
const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const LIM = 0.9 * cfg.design.n32.grapple32.range;
const eye = (l) => [l.at[0], l.at[1] + 1.6, l.at[2]];
const d3 = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1], a[2] - b[2]);
const knee = (st, pair, side) => [st[0] + side * 9, st[1] - 6, st[2] + (1 - pair) * 7];
let bad = 0;
const L = (id) => ledges.find((l) => l.id === id);
const checks = [["start -> t1w", eye(L("start")), anchors.find((a) => a.id === "t1w").at]];
PAIRS.forEach((P, i) => {
  for (const side of ["w", "e"]) {
    const l = L(`t${P.pair}${side}`);
    checks.push([`t${P.pair}${side} -> its knee`, eye(l), knee(P.station, i, side === "w" ? -1 : 1)]);
    if (side === "w") checks.push([`t${P.pair}w -> t${P.pair}e (the across pull)`, eye(l), anchors.find((a) => a.id === `t${P.pair}e`).at]);
    if (i + 1 < PAIRS.length) checks.push([`t${P.pair}${side} -> t${P.pair + 1}${side}`, eye(l), anchors.find((a) => a.id === `t${P.pair + 1}${side}`).at]);
  }
});
console.log(`reach: a hook must be under ${LIM.toFixed(1)} m (0.9 x design.n32.grapple32.range)`);
for (const [label, a, b] of checks) { const d = d3(a, b); if (d > LIM) bad++; console.log(`  ${d <= LIM ? "ok  " : "FAIL"} ${d.toFixed(1).padStart(5)} m  ${label}`); }

// ---- the guard: nothing outside o3 moves ------------------------------------------------------------------------------------------------------------
for (const key of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props", "surfaces"]) {
  if (!same((before[key] || []).filter((e) => ownerOf(e) !== "o3"), (plan[key] || []).filter((e) => ownerOf(e) !== "o3"))) throw new Error(`guard: ${key} outside o3 changed`);
}
for (const r of before.rooms) if (r.id !== "o3" && !same(r, plan.rooms.find((q) => q.id === r.id))) throw new Error(`guard: room ${r.id} changed`);
if (!same(before.doors, plan.doors)) throw new Error("guard: the doors changed");
for (const k of Object.keys(before)) {
  if (["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props", "surfaces", "rooms", "doors", "id", "_why"].includes(k)) continue;
  if (!same(before[k], plan[k])) throw new Error(`guard: top-level ${k} changed`);
}
void oldO3;

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms; o3 THE LOOM FRAME:", boxes.length, "boxes,", anchors.length, "anchors,", ledges.length, "ledges,", lights.length, "lights");
if (bad) { console.log(`RED: ${bad} hook(s) over ${LIM.toFixed(1)} m`); process.exit(1); }
console.log("GREEN: every hook under the margin");

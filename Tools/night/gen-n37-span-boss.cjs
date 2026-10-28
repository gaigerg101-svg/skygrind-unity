// NIGHT37 builder O (NIGHT37.md O1; Research/2026-10-12-wave37/BOSSES.md "L4 THE LOOM"): writes level 4 as n37-span: n33-span's rooms up to
// (not including) its old boss room s8, COPIED AS THEY ARE from the plan json n33-span was built from (Design/levels/n33/span.plan.json: levelgen
// of that plan reproduces Design/levels/n33/span.json byte for byte but the trailing newline, measured 2026-10-12), then two NEW rehearsal rooms
// and THE CANYON (boss id "loom37"). No older generator is required; Tools/night/n36-lib.cjs (the emit helpers: every new solid has job + why)
// lays out the new rooms. The copied solids keep their geometry; only a `job` / `why` tag is added (their note is the why) so n36-jobs reads them.
//
// The new rooms (BOSSES.md rule 9, "taught before it is tested"; D-4 two rehearsal rooms):
//   o1 4.8 THREAD LINE   a node chain over a void: pull to a thread, ride a tower wall, pull to the far ledge (a fall: back to the room's start).
//   o2 4.9 SWING ANCHOR  a moving node (an anchor that swings east-west: Game/SurvivalSim.Loom37.cs moves it) you pull to, a post to land on,
//                        and a practice knee: nail it, then pull AWAY to the far thread so the nails tear it (the canyon's damage verb).
//   o3 4.10 THE CANYON   120 m of void between a start ledge and an end ledge, three tower pairs (ride walls, anchor ledges on top), thread
//                        anchors as grapple nodes; THE LOOM hangs over it (its six knees are moving nodes the sim adds). A fall = back to the
//                        last anchor ledge you stood on, -20 HP (the sim's leg checkpoint; the room's respawnY is the campaign's fallback).
//
// Every node-to-node and ledge-to-node distance is printed and checked against the grapple's reach (design.n32.grapple32.range 40 m with the row
// on): the script FAILS if a link of the intended route is longer than the reach minus a 2 m margin.
//
// Usage, from the repo (or worktree) root:  node Tools/night/gen-n37-span-boss.cjs   then  bash Tools/night/n37o-regen.sh
const fs = require("fs");
const path = require("path");
const { Level, r3 } = require("./n36-lib.cjs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n33/span.plan.json");
const OUT = path.join(ROOT, "Design/levels/n37/span.plan.json");
const KEEP = ["s1", "s2", "s3", "s3b", "s4", "s5", "s6", "s7"];
const KEEP_DOORS = ["d1", "d2", "d3", "d4", "d5", "d6", "d7"];

const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const REACH = (cfg.design.n32 && cfg.design.n32.grapple32 && cfg.design.n32.grapple32.range) || 40;
const MARGIN = 2;

const src = JSON.parse(fs.readFileSync(SRC, "utf8"));
const kept = (z) => KEEP.includes(z);

// ---- the copied part (n33-span's s1..s7, unchanged geometry)
const tagJob = (o, job) => {
  const c = Object.assign({}, o);
  if (!c.job) c.job = job;
  if (!c.why) c.why = (c.note && String(c.note).length >= 8 ? c.note : `n33-span's ${job} in ${c.zone || c.room}, copied as it was`) + " (n33-span, copied as is by gen-n37-span-boss.cjs)";
  return c;
};
const boxJob = (b) => (b.kind === "hull" ? "cover" : b.kind === "wall" ? "line" : "line");
const old = {
  rooms: src.rooms.filter((r) => kept(r.id)),
  doors: src.doors.filter((d) => KEEP_DOORS.includes(d.id)),
  boxes: src.boxes.filter((b) => kept(b.zone)).map((b) => tagJob(b, boxJob(b))),
  ramps: src.ramps.filter((b) => kept(b.zone)).map((b) => tagJob(b, "climb")),
  pits: src.pits.filter((b) => kept(b.zone)).map((b) => tagJob(b, "hazard")),
  tunnels: (src.tunnels || []).filter((b) => kept(b.zone)).map((b) => tagJob(b, "line")),
  lights: src.lights.filter((l) => kept(l.room)),
  props: src.props.filter((p) => kept(p.room)).map((p) => (p.collider ? tagJob(p, p.pickup ? "reward" : "cover") : p)),
};
const s7 = old.rooms.find((r) => r.id === "s7");

// ---- the new rooms (n36-lib), laid north from s7's north wall; s7's exit is at x -17
const L = new Level("n37-span", "LEVEL 4 THE SPAN", "n37-span");
L.prev = { id: "s7", w: s7.rect[2] - s7.rect[0], h: s7.height, s: {}, z0: s7.rect[1], outX: -17 };
const CX = -17;

const links = []; // [from label, from point, to label, to point, route?]
const near = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1], a[2] - b[2]);

// o1: THREAD LINE (a node chain over a void: pull, ride a tower wall, pull)
const o1 = L.room({ id: "o1", name: "4.8 THREAD LINE", kind: "link", w: 24, l: 50, h: 20, cx: CX, par: 20, job: "rehearse", verb: "grapple pull + tower ride + pull" });
o1.row.roof = true;
o1.row.loomRoom37 = true; // a loom37 room: Game/SurvivalSim.Loom37.cs runs its ride refill here
o1.row.respawnY = -5;
o1.row.respawnY_why = "a fall into the void is back at the room's start (Campaign24's respawnY, the share of health design.n29.roomRules.fallCostFraction): rehearsal, not punishment";
o1.pit(-12, 6, 12, 44, -14, "the void the thread line crosses: no floor between the ledges (a fall lands 14 m down and is taken back to the start)");
o1.box("wall", 3, 19, 7, 37, -14, 6, "line", "TOWER: the ride wall between two threads (its west face is ridden north, 18 m = ~1.7 s of ride); its top (y 6) is a ledge to land on").ride = true;
const o1n = [o1.pt(0, 9, 15), o1.pt(-1, 9, 47)];
o1.row.grappleNodes = o1n;
o1.row.grapple = "two threads: one over the void before the tower (pull 1), one over the far ledge (pull 2, from the tower's ride or its top)";
o1.light("pool", o1.pt(0, 0.025, 47), [0, 1, 0], [3, 3, 0.05], "amber", 6, 1.2);
o1.lesson({ kind: "reached", prompt: "{grapple} PULL. RIDE. PULL.", rect: [-12, 44, 12, 50], minY: -0.2 });
o1.frames([0, 0, 3], [0, 9, 15], [5, 6, 36], [-1, 9, 47]);
links.push(["o1 ledge", o1.pt(0, 1.6, 5), "o1 thread 1", o1n[0], true]);
links.push(["o1 thread 1 (arrival)", o1.pt(0, 9, 12.4), "o1 tower face start", o1.pt(3, 4, 20), true]);
links.push(["o1 tower top N", o1.pt(5, 7.6, 36), "o1 thread 2", o1n[1], true]);
links.push(["o1 ledge", o1.pt(0, 1.6, 5), "o1 tower face (direct)", o1.pt(3, 4, 20), false]);

// o2: SWING ANCHOR (a moving node + a nail-and-tear target)
const o2 = L.room({ id: "o2", name: "4.9 SWING ANCHOR", kind: "link", w: 28, l: 46, h: 20, cx: CX, par: 20, job: "rehearse", verb: "pull to a moving anchor + nail a knee and pull away (the tear)" });
o2.row.roof = true;
o2.row.loomRoom37 = true;
o2.row.respawnY = -5;
o2.row.respawnY_why = o1.row.respawnY_why;
o2.pit(-14, 6, 14, 38, -14, "the void under the swinging anchor: no floor between the ledges and the post");
o2.box("deck", -3, 24, 3, 31, -14, 4, "line", "THE POST: a 6 x 7 m stone top at y 4 to land on after the swing; the practice knee hangs off its east side");
const swingRest = o2.pt(0, 11, 17);
const o2n = [swingRest, o2.pt(0, 8, 27), o2.pt(-2, 9, 42)];
o2.row.grappleNodes = o2n;
o2.row.grapple = "the SWING ANCHOR (node 1: Game/SurvivalSim.Loom37.cs moves it east-west), a thread over the post (node 2), the far thread over the exit ledge (node 3)";
o2.row.loomSwing37 = { node: 0, rest: swingRest, ampX: 7, periodS: 3.2, _why: "the anchor's swing: +- ampX m east-west about its rest, one period in periodS (design.n37.loom37.swingAmpM / swingPeriodS win when present)" };
o2.row.kneeTarget37 = { at: o2.pt(4, 7, 28), why: "the practice knee: nail it from the post, then pull away to the far thread (node 3): the nails tear it (a cyan burst)" };
o2.light("pool", o2.pt(0, 0.025, 42), [0, 1, 0], [3, 3, 0.05], "amber", 6, 1.2);
o2.lesson({ kind: "reached", prompt: "NAIL THE KNEE. {grapple} PULL AWAY.", rect: [-14, 38, 14, 46], minY: -0.2 });
o2.frames([0, 0, 3], [0, 11, 17], [0, 4, 29], [4, 7, 28]);
links.push(["o2 ledge", o2.pt(0, 1.6, 4), "o2 swing (rest)", swingRest, true]);
links.push(["o2 ledge", o2.pt(0, 1.6, 4), "o2 swing (east end)", o2.pt(7, 11, 17), true]);
links.push(["o2 swing (arrival)", o2.pt(0, 10.5, 14.4), "o2 post thread", o2n[1], true]);
links.push(["o2 post", o2.pt(0, 5.6, 29), "o2 far thread", o2n[2], true]);

// o3: THE CANYON (boss "loom37")
const CL = 140; // lz 0..8 start ledge, 8..130 the void, 130..140 the end ledge
const o3 = L.room({ id: "o3", name: "4.10 THE CANYON", kind: "boss", w: 44, l: CL, h: 30, cx: CX, par: 60, job: "boss", verb: "grapple crossing + nail and tear (the exam)" });
o3.row.roof = false;
o3.row.loomRoom37 = true;
o3.row.boss = "loom37";
o3.row.respawnY = -12;
o3.row.respawnY_why = "the campaign's fallback only: with design.n37.loom37 on, Game/SurvivalSim.Loom37.cs catches a fall first (feet under design.n37.loom37.fallY) and puts the player on the last anchor ledge for -20 HP";
o3.pit(-22, 8, 22, 130, -24, "THE VOID: the canyon has no floor between its start ledge and its end ledge (BOSSES.md: the grapple is your footing)");
const towerLz = [[30, 44], [66, 80], [102, 116]];
const TOP = 8;
const ledges = [{ id: "start", at: o3.pt(0, 0, 4), rect: o3.rect(-22, 0, 22, 8), y: 0, pair: 0 }];
towerLz.forEach(([a, b], i) => {
  const n = i + 1;
  const why = (side) => `TOWER ${n}${side}: a stone tower out of the void (5 x 14 m); its faces are ride walls (north-south, ~1.3 s a face), its top (y ${TOP}) an ANCHOR LEDGE (a fall comes back here)`;
  o3.box("wall", -14, a, -9, b, -24, TOP, "line", why("W")).ride = true;
  o3.box("wall", 9, a, 14, b, -24, TOP, "line", why("E")).ride = true;
  ledges.push({ id: `t${n}w`, at: o3.pt(-11.5, TOP, (a + b) / 2), rect: o3.rect(-14, a, -9, b), y: TOP, pair: n });
  ledges.push({ id: `t${n}e`, at: o3.pt(11.5, TOP, (a + b) / 2), rect: o3.rect(9, a, 14, b), y: TOP, pair: n });
  o3.light("pool", o3.pt(-11.5, TOP + 0.025, (a + b) / 2), [0, 1, 0], [3, 3, 0.05], "amber", 7, 1.3);
  o3.light("pool", o3.pt(11.5, TOP + 0.025, (a + b) / 2), [0, 1, 0], [3, 3, 0.05], "amber", 7, 1.3);
});
ledges.push({ id: "end", at: o3.pt(0, 0, 135), rect: o3.rect(-22, 130, 22, 140), y: 0, pair: 4 });
// thread anchors (grapple nodes), in order; `pair` = the tower pair they lead to (the Loom cuts the ones behind it), `ledge` = the ledge under one
const anchors = [
  ["a0w", -6, 10, 18, 1, null], ["a0e", 6, 10, 18, 1, null],
  ["t1w", -11.5, 13, 34, 1, "t1w"], ["t1e", 11.5, 13, 34, 1, "t1e"],
  ["m1w", -4, 15, 55, 2, null], ["m1e", 4, 15, 55, 2, null],
  ["t2w", -11.5, 13, 70, 2, "t2w"], ["t2e", 11.5, 13, 70, 2, "t2e"],
  ["m2w", -4, 15, 91, 3, null], ["m2e", 4, 15, 91, 3, null],
  ["t3w", -11.5, 13, 106, 3, "t3w"], ["t3e", 11.5, 13, 106, 3, "t3e"],
  ["end", 0, 8, 134, 4, "end"],
];
o3.row.grappleNodes = anchors.map((a) => o3.pt(a[1], a[2], a[3]));
o3.row.grapple = "THREAD ANCHORS (BOSSES.md L4): one pair before each tower pair and one pair over each tower top (the anchor ledges), one over the end ledge; THE LOOM's six knees join them as moving nodes for the fight (Game/SurvivalSim.Loom37.cs) and it cuts the ones behind it";
o3.row.loom37 = {
  _why: "THE CANYON's data for Game/SurvivalSim.Loom37.cs (builder O): anchors = the room's grappleNodes in order (id, pair, the ledge under it); ledges = the anchor ledges (a fall comes back to the last one stood on); stations = where THE LOOM hangs for each leg (leg 3 = the nest); webWalls = the sheet each leg can weave between a tower pair (a layer swap); all world metres",
  anchors: anchors.map((a, i) => ({ id: a[0], node: i, at: o3.pt(a[1], a[2], a[3]), pair: a[4], ledge: a[5] })),
  ledges,
  stations: [o3.pt(0, 20, 52), o3.pt(0, 20, 88), o3.pt(0, 18, 109)],
  webWalls: towerLz.map(([a, b]) => ({ min: [o3.X(-9), -24, o3.Z((a + b) / 2 + 0.3)], max: [o3.X(9), 16, o3.Z((a + b) / 2 - 0.3)] })),
  towers: towerLz.map(([a, b], i) => ({ pair: i + 1, rectW: o3.rect(-14, a, -9, b), rectE: o3.rect(9, a, 14, b), top: TOP })),
};
o3.light("pool", o3.pt(0, 0.025, 4), [0, 1, 0], [4, 4, 0.05], "amber", 8, 1.3);
o3.light("pool", o3.pt(0, 0.025, 135), [0, 1, 0], [4, 4, 0.05], "amber", 8, 1.3);
o3.frames([0, 0, 4], [0, 14, 60], [-11.5, TOP, 72], [0, 16, 109]);
// the intended route, every link
links.push(["canyon start ledge", o3.pt(0, 1.6, 7), "a0 thread (W)", o3.pt(-6, 10, 18), true]);
links.push(["canyon start ledge", o3.pt(0, 1.6, 7), "t1w anchor (direct)", o3.pt(-11.5, 13, 34), true]);
links.push(["a0w (arrival)", o3.pt(-5, 10, 15.6), "t1w anchor", o3.pt(-11.5, 13, 34), true]);
for (let i = 1; i <= 3; i++) {
  const [a, b] = towerLz[i - 1];
  const topN = o3.pt(-11.5, TOP + 1.6, b - 1);
  if (i < 3) {
    const [na] = towerLz[i];
    links.push([`t${i}w top (N end)`, topN, `m${i}w thread`, o3.pt(-4, 15, (b + na) / 2 + 1), true]);
    links.push([`t${i}w top (N end)`, topN, `t${i + 1}w anchor (direct)`, o3.pt(-11.5, 13, na + 4), true]);
    links.push([`m${i}w (arrival)`, o3.pt(-4, 14.5, (b + na) / 2 - 1.6), `t${i + 1}w anchor`, o3.pt(-11.5, 13, na + 4), true]);
    links.push([`t${i}w top (N end)`, topN, `t${i}e anchor (across)`, o3.pt(11.5, 13, a + 4), false]);
  } else {
    links.push([`t3w top (N end)`, topN, "end anchor", o3.pt(0, 8, 134), true]);
    links.push([`t3e top (N end)`, o3.pt(11.5, TOP + 1.6, b - 1), "end anchor", o3.pt(0, 8, 134), true]);
  }
}

// ---- merge and write
const fresh = L.finish();
fresh.doors.forEach((d, i) => { d.id = `d${KEEP_DOORS.length + 1 + i}`; }); // n36-lib numbers its own doors from d1: continue n33-span's d1..d7
const plan = {
  _why: "LEVEL 4 THE SPAN, NIGHT37 (builder O, NIGHT37.md O1; BOSSES.md L4): n33-span's rooms s1..s7 as they were (copied from Design/levels/n33/span.plan.json), then two rehearsal rooms (o1 THREAD LINE, o2 SWING ANCHOR) and THE CANYON (o3, boss loom37), from Tools/night/gen-n37-span-boss.cjs. design.n37.loom37 swaps n33-span for this file while it is on.",
  id: "n37-span", name: "LEVEL 4 THE SPAN",
  n29: true, n30: true, n36: true, n37: true,
  floorSize: 0, killPlaneY: -40, killPlaneY_why: "under the canyon's void floor (-24) and its pit slab: a fall is caught by the room's respawnY / the sim's ledge rule first",
  wallHeight: src.wallHeight, rank: src.rank, n32: src.n32,
  rooms: [...old.rooms, ...fresh.rooms],
  doors: [...old.doors, ...fresh.doors],
  boxes: [...old.boxes, ...fresh.boxes],
  ramps: [...old.ramps, ...fresh.ramps],
  pads: [], grapple: [],
  pits: [...old.pits, ...fresh.pits],
  tunnels: [...old.tunnels, ...fresh.tunnels],
  lights: [...old.lights, ...fresh.lights],
  props: [...old.props, ...fresh.props],
};
if (fresh.teach35) plan.teach35 = fresh.teach35;
let maxAbs = 0;
for (const r of plan.rooms) for (const v of r.rect) maxAbs = Math.max(maxAbs, Math.abs(v) + 4);
plan.floorSize = Math.ceil((maxAbs * 2 + 16) / 2) * 2;

// ---- the reach check (real numbers)
let bad = 0;
console.log(`grapple reach ${REACH} m (design.n32.grapple32.range), margin ${MARGIN} m`);
for (const [fa, pa, fb, pb, route] of links) {
  const d = near(pa, pb);
  const ok = d <= REACH - MARGIN;
  if (route && !ok) bad++;
  console.log(`  ${ok ? "ok  " : route ? "FAIL" : "far "} ${d.toFixed(1).padStart(5)} m  ${fa} -> ${fb}${route ? "" : " (optional)"}`);
}
fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms", plan.boxes.length, "boxes", plan.pits.length, "pits", plan.lights.length, "lights", plan.props.length, "props",
  "| canyon nodes", o3.row.grappleNodes.length, "| floorSize", plan.floorSize);
if (bad) { console.log(`RED: ${bad} route link(s) beyond the reach`); process.exit(1); }
console.log("GREEN: every route link within the reach");

// NIGHT39 builder K4 (NIGHT39.md card K4; GG 2026-10-13: level 3 needs "a new arena, and keep the worm AI the same"): THE KILN WYRM's NEW arena for
// the rebuilt level 3, Design/maps/wyrm39-arena.json. Read by the worm only when design.n38.kiln38 is on and its `wyrmArena` names this file (the
// lead's one-line hook in SurvivalSim.Wyrm24.cs Wyrm24Ensure; off: Design/maps/wyrm26-arena.json as NIGHT26-38).
// What stays (the AI the same): the SHELL (wyrm26-arena.json's floor, four walls, ceiling and the twelve stepped corner blocks: boxes 0-17 copied in
// order), so `half` 30 and `height` 30 are the same and Wyrm.cs's clamp to arenaHalf (Wyrm.cs:330) is unchanged; the player's spawn nook; the paths
// file (Design/bosses/wyrm26.json: every route, the cycle, the finale) is not touched or re-read by the worm differently.
// What is new (built for the THUNDERCLAP and its climb, the level's verbs): the four full-height pillars and NIGHT26's six ride slabs are gone; in
// their place a ring of BOOST PERCHES at three heights (6 m: one shot; 9.5 m: the climb, shot, double, shot; 13 m: the climb from a 6 m perch or a
// slab's ride), RIDE SLABS paired as corridors along the ring (a ride carries you from perch to perch), and two CRUCIBLE STACKS. Every piece is
// kept 2.9 m clear, in 3D, of every densified worm route (the head never sits inside geometry; buried route points, the finale's gap, touch nothing).
// The crumble (NIGHT31 C3.5: the camp test's highest perches fall at half health) is re-pointed at this file's two highest perches: `crumble39`
// (boxes, keep, draw) is written here and copied into n38-kiln's k5 row by Tools/night/gen-n38-kiln.cjs.
// Run (from the repo root): node Tools/night/gen-n39-wyrm-arena.cjs   (writes the file and its StreamingAssets copy)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const r2 = (v) => Math.round(v * 100) / 100;

const SRC = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/maps/wyrm26-arena.json"), "utf8"));
const PATHS = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/bosses/wyrm26.json"), "utf8"));
const MARGIN = 2.9;

// ---- every worm route, densified to 1 m (gen-wyrm26.py's `pts`)
const pts = [];
const addWps = (w) => {
  for (let i = 0; i + 1 < w.length; i++) {
    const a = w[i], b = w[i + 1];
    const n = Math.max(1, Math.floor(Math.hypot(b[0] - a[0], b[1] - a[1], b[2] - a[2]) / 1.0));
    for (let k = 0; k <= n; k++) { const t = k / n; pts.push([0, 1, 2].map((j) => a[j] + (b[j] - a[j]) * t)); }
  }
};
for (const pat of [...Object.values(PATHS.patterns), PATHS.emerge]) for (const v of pat.variants) for (const key of ["wp", "wp3"]) if (v[key]) addWps(v[key]);
/// clear of every route point by `margin` in 3D (the distance from the point to the box): the head never touches a piece. (gen-wyrm26.py's
/// test also refused a route point anywhere UNDER a piece; under that rule no 6 m perch fits anywhere in the room: round 1. A worm passing under a
/// perch is the fight GG asked for, "if the boss is close it might just go after you".)
function clear(lo, hi, margin = MARGIN) {
  for (const [x, y, z] of pts) {
    if (y < -1) continue; // buried pieces touch nothing (wyrm26.json's finale gap)
    const dx = Math.max(lo[0] - x, 0, x - hi[0]), dy = Math.max(lo[1] - y, 0, y - hi[1]), dz = Math.max(lo[2] - z, 0, z - hi[2]);
    if (Math.hypot(dx, dy, dz) < margin) return false;
  }
  return true;
}

// ---- the shell: wyrm26-arena.json's boxes 0-17 (floor, walls, ceiling, the stepped corners), copied in order
const src = SRC.collision.boxes;
const shellN = 18;
for (let i = 0; i < shellN; i++) if (!["floor", "wall", "ceiling"].includes(src[i].kind)) throw new Error(`box ${i} is not shell (${src[i].kind}): the shell is boxes 0-17`);
const boxes = src.slice(0, shellN).map((b) => ({ min: b.min.slice(), max: b.max.slice(), kind: b.kind }));
const spawn = SRC.playerSpawn;
// the spawn nook (south wall, z about +27.5) stays open at floor level (a perch over it, its underside 5 m up, blocks nothing)
const nookClear = (lo, hi) => lo[1] >= 3 || !(hi[0] > -7 && lo[0] < 7 && hi[2] > 20);
/// inside the ring (|x|, |z| <= 29.5) and in no shell box (the stepped corners): a piece never cuts into the shell
function inShell(lo, hi) {
  if (Math.max(Math.abs(lo[0]), Math.abs(hi[0]), Math.abs(lo[2]), Math.abs(hi[2])) > 29.5) return true;
  for (let i = 6; i < shellN; i++) {
    const b = boxes[i];
    if (lo[0] < b.max[0] && hi[0] > b.min[0] && lo[2] < b.max[2] && hi[2] > b.min[2] && lo[1] < b.max[1] && hi[1] > b.min[1]) return true;
  }
  return false;
}

const placed = [];
function tryPlace(lo, hi, kind, what, margin) {
  if (!nookClear(lo, hi) || inShell(lo, hi)) return false;
  if (!clear(lo, hi, margin)) return false;
  for (const p of placed) if (lo[0] < p.hi[0] + 2 && hi[0] > p.lo[0] - 2 && lo[2] < p.hi[2] + 2 && hi[2] > p.lo[2] - 2 && lo[1] < p.hi[1] + 4 && hi[1] > p.lo[1] - 4) return false; // 2 m apart on the plan, or 4 m of headroom over a lower piece
  const b = { min: lo.map(r2), max: hi.map(r2), kind };
  boxes.push(b);
  placed.push({ lo, hi, kind, what, index: boxes.length - 1 });
  return true;
}

// ---- RIDE SLABS: free-standing walls 1.5 m thick, 10 m long, 12 m high, tangent to the ring at R 27 (3 m off the wall ring's corners): a ride
// along one carries you from a perch to the next; the wall behind gives the wall jump back
const slabs = [];
for (const R of [27, 26, 24]) {
  for (let deg = 5; deg < 360 && slabs.length < 6; deg += 10) {
    const a = deg * Math.PI / 180, cx = R * Math.cos(a), cz = R * Math.sin(a);
    if (slabs.some((s) => Math.abs(((deg - s + 540) % 360) - 180) < 40)) continue; // spread round the ring
    const alongX = Math.abs(Math.sin(a)) > 0.7; // near north / south: the slab runs along x
    const lo = alongX ? [cx - 4, 0, cz - 0.75] : [cx - 0.75, 0, cz - 4], hi = alongX ? [cx + 4, 12, cz + 0.75] : [cx + 0.75, 12, cz + 4];
    if (tryPlace(lo, hi, "wall", "a ride slab", MARGIN)) slabs.push(deg);
  }
}

// ---- two CRUCIBLE STACKS: 3 x 3 m columns to 18 m, the kiln's chimneys (cover from the spit, a wall to kick from)
const stacks = [];
for (const R of [27.5, 26, 24]) {
  for (let deg = 0; deg < 360 && stacks.length < 2; deg += 10) {
    const a = deg * Math.PI / 180, cx = r2(R * Math.cos(a)), cz = r2(R * Math.sin(a));
    if (stacks.some((s) => Math.hypot(s[0] - cx, s[1] - cz) < 30)) continue; // the two stacks on opposite sides
    if (tryPlace([cx - 1.5, 0, cz - 1.5], [cx + 1.5, 18, cz + 1.5], "pillar", "a crucible stack", MARGIN)) stacks.push([cx, cz]);
  }
}

// ---- the ring of BOOST PERCHES: 6 x 6 m platforms 1 m thick, three heights; candidates on rings of radius R every 10 degrees, taken in a fixed
// order (deterministic), each tier spread round the ring (the next candidate must stand at least `spread` degrees from the tier's others)
const TIERS = [
  { top: 6, want: 3, spread: 70, why: "one THUNDERCLAP shot (6.76 m, envelopes38)" },
  { top: 9.5, want: 3, spread: 70, why: "the climb (shot, double, shot: 13.36 m); the PULSE blast tops at 7.75 m" },
  { top: 13, want: 2, spread: 90, why: "the climb from a 6 m perch, or a slab's ride and a shot" },
];
const perchAngles = {};
for (const tier of TIERS) {
  const got = [];
  for (const R of [27, 25, 22, 19]) {
    for (let deg = 0; deg < 360 && got.length < tier.want; deg += 10) {
      const a = (deg + (tier.top === 9.5 ? 5 : 0)) * Math.PI / 180;
      if (got.some((g) => Math.abs(((deg - g + 540) % 360) - 180) < tier.spread)) continue;
      const cx = R * Math.cos(a), cz = R * Math.sin(a);
      if (tryPlace([cx - 3, tier.top - 1, cz - 3], [cx + 3, tier.top, cz + 3], "platform", `a ${tier.top} m perch: ${tier.why}`)) got.push(deg);
    }
  }
  perchAngles[tier.top] = got;
  if (got.length < 1) throw new Error(`only ${got.length} perches clear of the routes at ${tier.top} m`);
}

// ---- the crumble: the two highest perches fall at half health (NIGHT31's rule on its new perches); the highest 6 m perch is kept
const perches = placed.filter((p) => p.kind === "platform");
const byTop = perches.slice().sort((a, b) => b.hi[1] - a.hi[1] || a.index - b.index);
const crumble = byTop.slice(0, 2).map((p) => p.index);
const keep = byTop.filter((p) => p.hi[1] <= 6).slice(0, 1).map((p) => p.index);
const draw = placed.map((p) => ({ index: p.index, kind: p.kind, top: r2(p.hi[1]), crumble: crumble.includes(p.index), rect: [r2(p.lo[0]), r2(p.lo[2]), r2(p.hi[0]), r2(p.hi[2])] }));

const out = {
  _why: "THE KILN WYRM's arena for the rebuilt level 3 (NIGHT39 builder K4; Tools/night/gen-n39-wyrm-arena.cjs; GG 2026-10-13: 'a new arena, and keep the worm AI the same'). "
    + "The SHELL of wyrm26-arena.json (boxes 0-17: floor, walls, ceiling, stepped corners; half 30, height 30: Wyrm.cs's arenaHalf clamp unchanged) and its spawn nook; "
    + "the worm's paths are wyrm26.json's, untouched. New inside: " + perches.length + " BOOST PERCHES (6 m: one shot; 9.5 m: the climb; 13 m: the climb from a 6 m perch or a ride), "
    + slabs.length + " RIDE SLABS tangent to the ring at R 26.5, " + stacks.length + " CRUCIBLE STACKS; every piece " + MARGIN + " m clear of every densified worm route (a route point at or under a piece's top + 1 m counts). "
    + "Perch angles by height: " + JSON.stringify(perchAngles) + "; slab angles: " + JSON.stringify(slabs) + "; stacks: " + JSON.stringify(stacks) + ".",
  name: "wyrm39-arena",
  half: SRC.half,
  height: SRC.height,
  floorSize: SRC.floorSize,
  killPlaneY: SRC.killPlaneY,
  center: SRC.center,
  playerSpawn: spawn,
  collision: { boxes },
  crumble39: {
    _why: "the two highest perches (indices " + crumble.join(", ") + ") crumble at half health, the highest 6 m perch (" + keep.join(", ") + ") stays: NIGHT31's camp test on the new perches",
    boxes: crumble, keep, draw,
  },
};
if (out.half !== 30 || out.height !== 30) throw new Error("the shell's half / height changed");
const file = path.join(ROOT, "Design/maps/wyrm39-arena.json");
fs.writeFileSync(file, JSON.stringify(out, null, 1) + "\n");
const sa = path.join(ROOT, "Assets/StreamingAssets/Design/maps/wyrm39-arena.json");
fs.mkdirSync(path.dirname(sa), { recursive: true });
fs.copyFileSync(file, sa);
console.log(`wrote ${path.relative(ROOT, file)} (+ the StreamingAssets copy): ${boxes.length} boxes (shell ${shellN}), perches ${perches.length} ${JSON.stringify(perchAngles)}, slabs ${slabs.length} ${JSON.stringify(slabs)}, stacks ${JSON.stringify(stacks)}, crumble ${JSON.stringify(crumble)} keep ${JSON.stringify(keep)}; route points ${pts.length}`);

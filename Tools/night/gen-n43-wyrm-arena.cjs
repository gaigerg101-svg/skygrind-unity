// NIGHT43 builder K1 (NIGHT43.md card K1, G28: "The worm boss arena itself also needs a major visual redesign so it feels like an intentional
// boss location"; the lead's round 3: readable hazards, worm-proof raised plates that matter for its attacks, a clear loop, a landmark at the far
// side, every piece with its reason; THE KILN WYRM's fight kept working). THE RING's arena for n43-kiln: Design/maps/wyrm43-arena.json (+ the
// StreamingAssets copy), named by n43-kiln's k5 row (`wyrmArena`; gen-n43-kiln.cjs). wyrm39-arena.json (NIGHT39 K4) is untouched: --off43 and
// n41-kiln still read it.
// It is wyrm39-arena.json's boxes IN ORDER (the shell, the ride slabs, the crucible stacks, the boost perches: the crumble's indices stay valid)
// plus, appended, under the same rule as K4's (every piece 2.9 m clear in 3D of every densified worm route in Design/bosses/wyrm26.json: the head
// never sits inside geometry; the paths, the cycle and the AI are untouched):
//   - THE SLAG PLATES: low 1.2 m islands on the inner ring (a jump lands on them: apex 1.58 m). The LUNGE passes at slide height and the DIVE
//     cracks the floor: on a plate your body is over both (a hypothesis from Wyrm.cs's header: the lunge "at slide height"; not measured), and
//     every plate is a step up to the 6 m perches' boosts. Each is the place to stand while the worm's tell runs.
//   - THE KILN MOUTH: the landmark on the far (north) wall, opposite the spawn nook: a broad block against the wall, as wide and tall as the
//     route clearance allows; what you see first from the spawn and the place the room "faces" (A1's look lights it from the data: `landmark43`).
//   - `loop43`: the proof that the floor ring between the plates and the walls is one loop with no dead end: at every 5 degrees round the
//     centre some radius 14-27 m is free floor (no piece under 1.8 m within 0.5 m), written into the file with the radii found.
// Not built: lava. The worm's arena is boxes only (Core/Enemies/WyrmArena.cs: no lava, no surfaces): a lava floor here would be paint that never
// burns; the hazard read is the worm itself (its tells) and A1's look.
// Run (from the repo root): node Tools/night/gen-n43-wyrm-arena.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const r2 = (v) => Math.round(v * 100) / 100;

const SRC = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/maps/wyrm39-arena.json"), "utf8"));
const PATHS = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/bosses/wyrm26.json"), "utf8"));
const MARGIN = 2.9; // K4's clearance from every worm route point (gen-n39-wyrm-arena.cjs)

// ---- every worm route, densified to 1 m (gen-n39-wyrm-arena.cjs's `pts`)
const pts = [];
const addWps = (w) => {
  for (let i = 0; i + 1 < w.length; i++) {
    const a = w[i], b = w[i + 1];
    const n = Math.max(1, Math.floor(Math.hypot(b[0] - a[0], b[1] - a[1], b[2] - a[2]) / 1.0));
    for (let k = 0; k <= n; k++) { const t = k / n; pts.push([0, 1, 2].map((j) => a[j] + (b[j] - a[j]) * t)); }
  }
};
for (const pat of [...Object.values(PATHS.patterns), PATHS.emerge]) for (const v of pat.variants) for (const key of ["wp", "wp3"]) if (v[key]) addWps(v[key]);
function clear(lo, hi, margin = MARGIN) {
  for (const [x, y, z] of pts) {
    if (y < -1) continue;
    const dx = Math.max(lo[0] - x, 0, x - hi[0]), dy = Math.max(lo[1] - y, 0, y - hi[1]), dz = Math.max(lo[2] - z, 0, z - hi[2]);
    if (Math.hypot(dx, dy, dz) < margin) return false;
  }
  return true;
}

const boxes = SRC.collision.boxes.map((b) => ({ min: b.min.slice(), max: b.max.slice(), kind: b.kind }));
const n39 = boxes.length;
const overlaps = (lo, hi, gap) => boxes.some((b, i) => i >= 6 && lo[0] < b.max[0] + gap && hi[0] > b.min[0] - gap && lo[2] < b.max[2] + gap && hi[2] > b.min[2] - gap && lo[1] < b.max[1] && hi[1] > b.min[1] - 0.01);
const nook = (lo, hi) => hi[0] > -7 && lo[0] < 7 && hi[2] > 20; // the spawn nook (south, z ~ +27.5) stays open
const added = [];
function place(lo, hi, kind, why) {
  if (Math.max(Math.abs(lo[0]), Math.abs(hi[0]), Math.abs(lo[2]), Math.abs(hi[2])) > 29.5) return false;
  if (nook(lo, hi) || overlaps(lo, hi, 2) || !clear(lo, hi)) return false;
  boxes.push({ min: lo.map(r2), max: hi.map(r2), kind });
  added.push({ index: boxes.length - 1, kind, why, rect: [r2(lo[0]), r2(lo[2]), r2(hi[0]), r2(hi[2])], top: r2(hi[1]) });
  return true;
}

// ---- THE SLAG PLATES: 5 x 5 m, 1.2 m high, on the outer ring between the routes and the walls (candidates every 10 degrees at R 23 / 21 / 25 / 19)
const PLATE = { size: 4, top: 1.2, want: 4, spread: 45 }; // the routes run low through the middle (R ~ 4-20: `k1-routes` probe): the plates stand on the outer ring
const plateDeg = [];
for (const R of [23, 21, 25, 19, 17]) {
  for (let deg = 45; deg < 405 && plateDeg.length < PLATE.want; deg += 5) {
    const d = deg % 360;
    if (plateDeg.some((g) => Math.abs(((d - g + 540) % 360) - 180) < PLATE.spread)) continue;
    const a = d * Math.PI / 180, cx = R * Math.cos(a), cz = R * Math.sin(a), h = PLATE.size / 2;
    if (place([cx - h, 0, cz - h], [cx + h, PLATE.top, cz + h], "platform",
      `a SLAG PLATE (${PLATE.top} m, ${PLATE.size} x ${PLATE.size} m, R ${R}): an island one jump high (apex 1.58 m) on the outer ring, off every route by 2.9 m: where the LUNGE (slide height) and the DIVE's floor crack cannot reach you; a step toward the boost perches`)) plateDeg.push(d);
  }
}

// ---- THE KILN MOUTH: the landmark on the far (north, -z) wall: the widest / tallest block against the wall that the routes allow
let mouth = null;
for (const w of [14, 12, 10, 8, 6]) {
  for (const top of [24, 20, 16, 12]) {
    for (const depth of [3, 2]) {
      if (mouth) break;
      const lo = [-w / 2, 0, -30], hi = [w / 2, top, -30 + depth];
      if (place(lo, hi, "wall", `THE KILN MOUTH (${w} x ${top} m, ${depth} m deep, the north wall): the landmark opposite the spawn nook, what the room faces; its face a wall to ride or kick from`)) mouth = { w, top, depth };
    }
  }
}

// ---- the loop: at every 5 degrees some radius 14-27 m is free floor (no piece under 1.8 m within 0.5 m of the point)
const freeAt = (x, z) => !boxes.some((b, i) => i >= 6 && b.min[1] < 1.8 && b.max[1] > 0.01 && x > b.min[0] - 0.5 && x < b.max[0] + 0.5 && z > b.min[2] - 0.5 && z < b.max[2] + 0.5)
  && !boxes.slice(6, 18).some((b) => x > b.min[0] - 0.5 && x < b.max[0] + 0.5 && z > b.min[2] - 0.5 && z < b.max[2] + 0.5);
const loop = [];
for (let deg = 0; deg < 360; deg += 5) {
  const a = deg * Math.PI / 180;
  const free = [];
  for (let R = 14; R <= 27; R += 0.5) if (freeAt(R * Math.cos(a), R * Math.sin(a))) free.push(R);
  loop.push({ deg, free: free.length, inner: free.length ? free[0] : null, outer: free.length ? free[free.length - 1] : null });
}
const blocked = loop.filter((l) => l.free === 0).map((l) => l.deg);
if (blocked.length) throw new Error("the loop is cut at " + blocked.join(", ") + " degrees");
const narrowest = loop.reduce((m, l) => (l.free < m.free ? l : m), loop[0]);

const out = {
  _why: "THE KILN WYRM's arena for n43-kiln (NIGHT43 K1, G28; Tools/night/gen-n43-wyrm-arena.cjs): wyrm39-arena.json's " + n39 + " boxes in order (the shell, slabs, stacks, perches; the crumble's indices unchanged) "
    + "plus " + added.length + " pieces 2.9 m clear of every worm route: " + plateDeg.length + " SLAG PLATES (1.2 m islands at " + JSON.stringify(plateDeg) + " deg) and "
    + (mouth ? "THE KILN MOUTH (" + mouth.w + " x " + mouth.top + " m on the north wall)" : "no KILN MOUTH (no clear spot)") + ". The worm's paths and AI are wyrm26.json's, untouched. No lava (WyrmArena is boxes only).",
  name: "wyrm43-arena",
  half: SRC.half,
  height: SRC.height,
  floorSize: SRC.floorSize,
  killPlaneY: SRC.killPlaneY,
  center: SRC.center,
  playerSpawn: SRC.playerSpawn,
  collision: { boxes },
  crumble39: SRC.crumble39, // the same indices (the new pieces are appended), the same rule: n43-kiln's k5 row copies it as gen-n40-kiln.cjs did
  pieces43: added,
  // the landmark: a solid KILN MOUTH where the routes allow one; else (measured: no block 6-14 m wide, 12-24 m tall, 2-3 m deep on the north
  // wall clears the coil by 2.9 m) the same place as FLUSH data on the north wall's face (no collision, nothing the worm can touch): A1's look
  // draws and lights it there
  landmark43: mouth ? { kind: "kilnMouth", solid: true, rect: [-mouth.w / 2, -30, mouth.w / 2, -30 + mouth.depth], top: mouth.top, why: "THE RING's landmark: A1's look lights it (the far side, opposite the spawn)" }
    : { kind: "kilnMouth", solid: false, rect: [-6, -30, 6, -29.9], top: 16, why: "THE RING's landmark, flush on the north wall's face (no solid fits the worm's 2.9 m clearance): the far side, opposite the spawn nook; A1's look draws and lights it" },
  loop43: { why: "every 5 degrees round the centre: the free floor radii (14-27 m) at that angle: one loop, no dead end", narrowest: narrowest, angles: loop },
};
if (out.half !== 30 || out.height !== 30) throw new Error("the shell's half / height changed");
const file = path.join(ROOT, "Design/maps/wyrm43-arena.json");
fs.writeFileSync(file, JSON.stringify(out, null, 1) + "\n");
const sa = path.join(ROOT, "Assets/StreamingAssets/Design/maps/wyrm43-arena.json");
fs.copyFileSync(file, sa);
console.log(`wrote ${path.relative(ROOT, file)} (+ StreamingAssets): ${boxes.length} boxes (${n39} from wyrm39), plates ${plateDeg.length} at ${JSON.stringify(plateDeg)}, the mouth ${JSON.stringify(mouth)}, the loop's narrowest ${narrowest.free * 0.5} m at ${narrowest.deg} deg`);

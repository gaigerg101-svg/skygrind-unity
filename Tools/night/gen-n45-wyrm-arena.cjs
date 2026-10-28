// NIGHT45 builder A3 (NIGHT45.md card A3, T-65; GG 2026-10-17 J4: "if it arena related please dont just randomly plce stuff it needs intension
// and personality"): THE RING's home, from its identity card (docs/ROOMS.md "NIGHT45 identity cards", k5 THE KILN'S HEART; DESIGN-RULES A-9).
// Design/maps/wyrm45-arena.json (+ the StreamingAssets copy), named by n45-kiln's k5 row (Tools/night/gen-n45-kiln.cjs). wyrm43-arena.json
// (NIGHT43 K1, Tools/night/gen-n43-wyrm-arena.cjs) is untouched: n43-kiln and --off45 still read it.
//
// It is wyrm43-arena.json's 30 boxes IN ORDER (so crumble39's indices 27 / 28 and its keep 23 stay valid: checked below) plus, appended,
// the plates the card asks for, each under K1's rule (2.9 m clear in 3D of every densified worm route in Design/bosses/wyrm26.json: the head never
// sits inside geometry; the paths, the cycle and the AI are untouched) AND measured in the sim: the worm's pieces never reach the plate's
// standing volume (its top + 1.8 m) in a whole fight, any attack, the aggro included (`n45ring wyrm --cands`, Harness.N45.Ring.cs: 3 seeds of
// WyrmPilot31 fighting, 85 candidate spots that pass K1's rule; Research/2026-10-17-wave45/A3.md). The written routes are not the worm's real
// track: the aggro dives at the player, the aimed lunge re-aims, the patterns join by curves, so K1's rule alone let the worm through
// three of round 1's five plates (K1's own by 2.5 m). Each plate is tied to the attack it answers (the card's "The plates" row):
//   - the two DIVE plates (north wall, flush, either side of THE KILN MOUTH): the dive cracks the floor on the south (z ~ +12) and erupts from a
//     marked spot on the east or west wall (z ~ -5, y ~ 9, 5 shards): the north rim is the far side from both (measured nearest 1.9 m, finale).
//   - the COIL plate (west wall, flush, z 13): outside the coil's R 19 ring and the finale's R 23 ring while they sweep round (round 1's
//     candidates there: nearest 1.1-1.5 m).
//   - RE-CUT, kept: K1's slag plate (wyrm43's index 29, south-east, R 25): the coil reached it by 2.5 m, the dive by 1.1 m, the aggro by 2.5 m
//     (round 1, `n45ring wyrm --level n43-kiln`): its claim ("where the LUNGE and the DIVE cannot reach you") is false in the sim, so it is no
//     shelter; it stays as the ROUTE step beside the spawn up to the 6 m perch (box 23) above it (removing it changed the bot's fight: round 2,
//     seed 3 not killed in 300 s, 8 deaths, against 161.1 s, 3 deaths with it; A3.md). Round 1's two LUNGE
//     plates (the walls at z -9) are not built: the lunge aims at the player (wyrmAim: it locks and re-aims), so no plate answers it (crossed by
//     1.8 / 2.1 m): the answer is to step off its line while it shows; the LOOP needs no plate (the head low in the middle is the opening).
// Each plate is 4 x 4 m, 1.2 m high (a jump lands on it: apex 1.58 m) and a step toward the boost perches; `pieces43` lists it so look43's
// WyrmRing43View draws it as K1's plate (riveted steel, Extras43). `solids45` gives EVERY box its reason (route / encounter decision, A-1 / V-4).
// Not built: lava (WyrmArena is boxes only: a lava floor here would be paint that never burns); a third crumble or a light that flips (sim /
// view code: next night).
// Run (from the repo or worktree root): node Tools/night/gen-n45-wyrm-arena.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const r2 = (v) => Math.round(v * 100) / 100;
const r1 = (v) => Math.round(v * 10) / 10;

const SRC_FILE = "Design/maps/wyrm43-arena.json";
const SRC = JSON.parse(fs.readFileSync(path.join(ROOT, SRC_FILE), "utf8"));
const PATHS = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/bosses/wyrm26.json"), "utf8"));
const MARGIN = 2.9; // K4's / K1's clearance from every worm route point (gen-n39-wyrm-arena.cjs, gen-n43-wyrm-arena.cjs)

// ---- every worm route, densified to 1 m (gen-n43-wyrm-arena.cjs's `pts`), kept per pattern so each plate's nearest attack is measured
const byPat = {};
const addWps = (w, out) => {
  for (let i = 0; i + 1 < w.length; i++) {
    const a = w[i], b = w[i + 1];
    const n = Math.max(1, Math.floor(Math.hypot(b[0] - a[0], b[1] - a[1], b[2] - a[2]) / 1.0));
    for (let k = 0; k <= n; k++) { const t = k / n; out.push([0, 1, 2].map((j) => a[j] + (b[j] - a[j]) * t)); }
  }
};
for (const [name, pat] of [...Object.entries(PATHS.patterns), ["emerge", PATHS.emerge]]) {
  byPat[name] = [];
  for (const v of pat.variants) for (const key of ["wp", "wp3"]) if (v[key]) addWps(v[key], byPat[name]);
}
const pts = Object.values(byPat).flat();
function dist(lo, hi, list) {
  let m = Infinity;
  for (const [x, y, z] of list) {
    if (y < -1) continue; // underground (the dive's tunnel, the emerge's rise below the floor)
    const dx = Math.max(lo[0] - x, 0, x - hi[0]), dy = Math.max(lo[1] - y, 0, y - hi[1]), dz = Math.max(lo[2] - z, 0, z - hi[2]);
    m = Math.min(m, Math.hypot(dx, dy, dz));
  }
  return m;
}

if (SRC.collision.boxes.length !== 30) throw new Error(SRC_FILE + ": " + SRC.collision.boxes.length + " boxes, expected wyrm43's 30 (the reasons below are by index)");
const k1 = SRC.collision.boxes[29];
if (k1.kind !== "platform" || k1.max[1] !== 1.2) throw new Error("wyrm43's index 29 is not K1's slag plate");
const RECUT45 = [{ index: 29, kind: k1.kind, rect: [k1.min[0], k1.min[2], k1.max[0], k1.max[2]], top: k1.max[1],
  was: "K1: where the LUNGE (slide height) and the DIVE's floor crack cannot reach you",
  measured: "reached in the sim by the coil (2.5 m into its standing volume), the dive (1.1 m) and the aggro (2.5 m): `n45ring wyrm --level n43-kiln`, 3 seeds",
  now: "the route step beside the spawn up to the 6 m perch above it; no shelter" }];
const boxes = SRC.collision.boxes.map((b) => ({ min: b.min.slice(), max: b.max.slice(), kind: b.kind }));
const n43 = boxes.length; // wyrm43's boxes kept, in order (0..29)
const overlaps = (lo, hi, gap) => boxes.some((b, i) => i >= 6 && lo[0] < b.max[0] + gap && hi[0] > b.min[0] - gap && lo[2] < b.max[2] + gap && hi[2] > b.min[2] - gap && lo[1] < b.max[1] && hi[1] > b.min[1] - 0.01);
const nook = (lo, hi) => hi[0] > -7 && lo[0] < 7 && hi[2] > 20; // the spawn nook (south, z ~ +27.5) stays open (K1's rule)

// ---- the plates the card asks for (centre x, z; flush with the wall where the card says so). A plate that fails a rule throws: the card is
// then wrong for this arena, not quietly skipped (K1's generator skipped: 4 wanted, 1 placed).
const PLATE = { size: 4, top: 1.2 };
const WANT = [
  { id: "diveWest", attack: "dive", at: [-12, -28], why: "a DIVE plate, north wall west of THE KILN MOUTH: the far side from the dive's southern floor cracks (z ~ +12) and from both of its wall exits (east / west, z ~ -5): stand here while the marked wall erupts" },
  { id: "diveEast", attack: "dive", at: [12, -28], why: "a DIVE plate, north wall east of THE KILN MOUTH (beside the 9.5 m perch: the boost up from it is the card's shortcut): the far side from the dive's cracks and exits" },
  { id: "coilWest", attack: "coil", at: [-28, 13], why: "the COIL plate, west wall (under the 13 m perch that crumbles): outside the coil's R 19 ring and the finale's R 23 ring while they sweep round the floor" },
];
const plates45 = [];
for (const w of WANT) {
  const h = PLATE.size / 2, lo = [w.at[0] - h, 0, w.at[1] - h], hi = [w.at[0] + h, PLATE.top, w.at[1] + h];
  if (Math.max(Math.abs(lo[0]), Math.abs(hi[0]), Math.abs(lo[2]), Math.abs(hi[2])) > 30) throw new Error(w.id + ": outside the shell");
  if (nook(lo, hi)) throw new Error(w.id + ": in the spawn nook");
  if (overlaps(lo, hi, 2)) throw new Error(w.id + ": within 2 m of another piece (K1's gap)");
  const d = dist(lo, hi, pts);
  if (d < MARGIN) throw new Error(w.id + ": " + r2(d) + " m from a worm route (< " + MARGIN + ")");
  const per = {};
  for (const [n, list] of Object.entries(byPat)) per[n] = r1(dist(lo, hi, list));
  // the attack it answers must pass within reach of the plate (the plate is the step OFF that attack's line, not a spot it never comes near)
  const reach = { coil: 12, dive: 30 }[w.attack]; // the coil ring passes ~10 m inside a rim plate; the dive spans the room
  if (!(per[w.attack] <= reach)) throw new Error(w.id + ": its attack (" + w.attack + ") passes " + per[w.attack] + " m away (> " + reach + ")");
  boxes.push({ min: lo.map(r2), max: hi.map(r2), kind: "platform" });
  plates45.push({ index: boxes.length - 1, id: w.id, attack: w.attack, centre: w.at, clearance: r2(d), nearest: per, why: w.why });
}
// the DIVE plates' claim: farther from the dive's danger (its two wall exits, PATHS.exits, and its floor cracks, each variant's diveCrack
// waypoint) than any other plate: the nearest of those points to each plate, measured
const diveSpots = PATHS.exits.map((e) => [e[0], e[1], e[2]]);
for (const v of PATHS.patterns.dive.variants) for (const m of v.marks || []) if (m.name === "diveCrack") diveSpots.push(v.wp[m.wp]);
if (diveSpots.length < 4) throw new Error("wyrm26.json: the dive's exits / cracks not found");
for (const p of plates45) {
  const b = boxes[p.index];
  p.fromDive = r1(Math.min(...diveSpots.map((e) => dist(b.min, b.max, [e]))));
}
const diveMin = Math.min(...plates45.filter((p) => p.attack === "dive").map((p) => p.fromDive));
const otherMax = Math.max(...plates45.filter((p) => p.attack !== "dive").map((p) => p.fromDive));
if (!(diveMin > otherMax)) throw new Error("the DIVE plates are not the farthest from the dive's exits and cracks (" + diveMin + " vs " + otherMax + ")");

// ---- every box's reason (A-1 / V-4: the route or the encounter decision it forces), by index (wyrm39 K4's layout, K1's plate, tonight's)
const why = (i) => {
  if (i === 0) return { job: "route", why: "the ring's floor: the worm breaks it where its routes cross (the seams look43 draws hot)" };
  if (i >= 1 && i <= 4) return { job: "route", why: "the ring's wall (the shell, half 30 m: Wyrm.cs's arenaHalf clamp): a wall to kick from round the whole rim" };
  if (i === 5) return { job: "route", why: "the roof at 30 m (the loop's top at 22 m stays under it)" };
  if (i >= 6 && i <= 17) return { job: "route", why: "a stepped corner (three boxes a corner): the ring reads round, and each step is a corner wall to wall-jump up from the rim" };
  if (i >= 18 && i <= 20) return { job: "encounter", why: "a ride slab (8 m long, 12 m tall): when the coil comes round at body height, ride it off the floor (V-3: its cyan strip)" };
  if (i === 21 || i === 22) return { job: "encounter", why: "a crucible stack (3 x 3 m, 18 m): cover from the spit barrage (phases 2-3) and a wall to ride up" };
  if (i === 23) return { job: "encounter", why: "the 6 m boost perch (crumble keep): the boost's first step above the coil; it stays after the crumble" };
  if (i >= 24 && i <= 26) return { job: "encounter", why: "a 9.5 m boost perch: above the coil and the lunge; the boost from a rim plate reaches it (the card's shortcut)" };
  if (i === 27 || i === 28) return { job: "encounter", why: "a 13 m perch: the high camp that crumbles at half health (crumble31: the arena's state change)" };
  if (i === 29) return { job: "route", why: "K1's slag plate, re-cut: the step beside the spawn up to the 6 m perch above it (1.2 m, a jump); NOT a shelter: the coil, the dive and the aggro reach it (recut45)" };
  const p = plates45.find((q) => q.index === i);
  if (p) return { job: "encounter", why: p.why };
  throw new Error("box " + i + " has no reason");
};
const solids45 = boxes.map((b, i) => Object.assign({ index: i, kind: b.kind }, why(i)));

// ---- crumble39 stays valid: its boxes and its keep index the same perches (13 m crumble, 6 m keep)
const cr = SRC.crumble39;
for (const i of cr.boxes) if (!(boxes[i] && boxes[i].kind === "platform" && boxes[i].max[1] === 13)) throw new Error("crumble39 box " + i + " is not a 13 m perch");
for (const i of cr.keep) if (!(boxes[i] && boxes[i].kind === "platform" && boxes[i].max[1] === 6)) throw new Error("crumble39 keep " + i + " is not the 6 m perch");
for (const d of cr.draw) {
  const b = boxes[d.index];
  if (!b || r2(b.max[1]) !== d.top || r2(b.min[0]) !== d.rect[0] || r2(b.min[2]) !== d.rect[1]) throw new Error("crumble39 draw " + d.index + " no longer matches its box");
}

// ---- the loop (gen-n43-wyrm-arena.cjs's proof, re-run on the new boxes): at every 5 degrees some radius 14-27 m is free floor
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

const pieces43 = SRC.pieces43.slice(); // K1's plate (drawn as riveted steel) + tonight's
for (const p of plates45) if (p.index >= n43) pieces43.push({ index: p.index, kind: "platform", why: p.why, rect: [boxes[p.index].min[0], boxes[p.index].min[2], boxes[p.index].max[0], boxes[p.index].max[2]], top: PLATE.top });

const out = {
  _why: "THE KILN WYRM's arena for n45-kiln (NIGHT45 A3, T-65; the identity card THE KILN'S HEART, docs/ROOMS.md; Tools/night/gen-n45-wyrm-arena.cjs): "
    + SRC_FILE + "'s " + n43 + " boxes in order (the shell, the stepped corners, the ride slabs, the crucible stacks, the perches, K1's plate re-cut as the step to the 6 m perch: recut45; crumble39's indices unchanged) plus "
    + (boxes.length - n43) + " slag plates, each 2.9 m clear of every worm route, never reached by the worm in the sim (n45ring wyrm) and tied to the attack it answers (plates45). Every box's reason: solids45. The worm's paths and AI are wyrm26.json's, untouched. No lava (WyrmArena is boxes only).",
  name: "wyrm45-arena",
  half: SRC.half,
  height: SRC.height,
  floorSize: SRC.floorSize,
  killPlaneY: SRC.killPlaneY,
  center: SRC.center,
  playerSpawn: SRC.playerSpawn,
  collision: { boxes },
  crumble39: SRC.crumble39, // the same indices (the plates are appended), the same rule: n45-kiln's k5 row copies it (gen-n45-kiln.cjs)
  pieces43, // look43's Extras43 draws each as riveted steel (K1's plate and tonight's)
  landmark43: SRC.landmark43, // THE KILN MOUTH, flush on the north wall (no solid clears the coil by 2.9 m there: K1's measure)
  plates45: { why: "the card's plates: each plate's index, the attack it answers, its clearance to every route (m) and to each pattern's route (nearest), and to the dive's nearest exit / crack (fromDive)", plates: plates45 },
  recut45: { why: "wyrm43's pieces whose stated reason the sim disproved, kept with a new one", pieces: RECUT45 },
  solids45: { why: "every box's reason (DESIGN-RULES A-1, V-4): job route = where the player goes, encounter = the decision it forces in the fight", boxes: solids45 },
  loop43: { why: "every 5 degrees round the centre: the free floor radii (14-27 m) at that angle: one loop, no dead end (re-run on wyrm45's boxes)", narrowest, angles: loop },
};
if (out.half !== 30 || out.height !== 30) throw new Error("the shell's half / height changed");
const file = path.join(ROOT, "Design/maps/wyrm45-arena.json");
fs.writeFileSync(file, JSON.stringify(out, null, 1) + "\n");
const sa = path.join(ROOT, "Assets/StreamingAssets/Design/maps/wyrm45-arena.json");
fs.copyFileSync(file, sa);
console.log(`wrote ${path.relative(ROOT, file)} (+ StreamingAssets): ${boxes.length} boxes (${n43} from wyrm43), plates ${plates45.map((p) => p.id + "@" + p.index + " clr " + p.clearance + " " + p.attack + " " + p.nearest[p.attack] + " dive " + p.fromDive).join("; ")}; the loop's narrowest ${narrowest.free * 0.5} m at ${narrowest.deg} deg`);

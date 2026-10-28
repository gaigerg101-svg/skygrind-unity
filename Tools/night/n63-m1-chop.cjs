// NIGHT63 M1 (the lead; measurement only, the red team's scope): THE CAMERA'S CHOP on a fixed-clock reel. Reads a reel set's verbs37
// telemetry (Logs/feel/<tag>/v*/verbs37/telemetry.jsonl: one frame line per 1/60 s on the reel's FeelClock, 2 sim steps each) and measures,
// frame to frame, the camera's position (cam.x / y / z), its pitch (cam.pitch) and its heading from the x / z path (no yaw in the telemetry)
// against a smoothed path (a centred quadratic Savitzky-Golay fit over WIN frames: constant velocity and constant acceleration, a jump's
// gravity arc, leave 0 residual, so only a change of acceleration inside 0.15 s counts; a moving average flagged every airborne frame,
// ~2.8 cm at 30 m/s2). Every number is labelled "fixed-clock camera path; real frame pacing not checked".
// Round 2 (the M1 GATE's FAIL, Logs/n63/verdict-M1.md): the eye-height snaps (cam.height, the view's height above the feet, moving over
// EYE_CM in one frame) are counted as EVENTS (a run of consecutive snap frames), each named by the telemetry's events within +-3 frames
// (slide / land / none) and its direction; every landing's largest eye step is listed against the line; the worst list names DISTINCT
// events (one per +-4 frames: the 9-frame fit spreads one step over its neighbours), splits each frame's vertical step into the feet's
// (cam.y - cam.height) and the eye's, and says "cause: none found in the telemetry" when no event or step explains it.
//   node Tools/night/n63-m1-chop.cjs [tag]                       (default n62-freeze)
//   node Tools/night/n63-m1-chop.cjs --bench <frames.csv> [skip]  the bench's frame CSV (PlayerChecks' frames.csv), the first `skip` rows
//                                                                (default 30: PlayerChecks.cs:481 skips them as warm-up) left out
//   node Tools/night/n63-m1-chop.cjs --pair <onTag> <offTag>      NIGHT63 eye63 (builder D, DECIDE63 D-10): two runs of the same reel set, a
//   node Tools/night/n63-m1-chop.cjs --pair <tag>                 view row on and off (e.g. verbs37 with design.n63.eye63 on, and again with
//                                                                -skygrindNoEye63); a tag is Logs/feel/<tag> or a directory path; with one
//                                                                tag each side S is paired with its twin S-off in that tag. Per level:
//     - VIEW ONLY: the feet (cam.y - cam.height), x and z frame by frame on both runs: the largest difference (the telemetry rounds y to 1 mm,
//       height to 0.1 mm), so a view row that moved the sim shows here;
//     - per EVENT of the off run (every eye-snap event and every landing, as above; an event within -4..+8 frames of the reel placing the player
//       is left out: the feet step over 10 cm with no vertical speed, M1 round 2's f267 / f342), the peak within +-4 frames of: the EYE'S OWN
//       residual (cam.height through the same fit: the residual is linear, so the vertical total is exactly the feet's plus the eye's), off ->
//       on, HALVED when on <= off / 2; the 3D total off -> on; the feet's own (a landing's total is mostly the feet's stop, which no camera
//       ease changes: M1 leaves the slam's stop out for that reason). An event whose eye's own residual is under the 2 cm chop line on the
//       off run is no hitch: listed, not counted;
//     - the whole path: rms, p95 and frames over the chop line, off -> on.
//   The measure (D-10): the eye's own residual halved at every slide and landing event; the last line says whether it is.
const fs = require("fs");
const path = require("path");

const pct = (a, p) => (a.length ? a.slice().sort((x, y) => x - y)[Math.min(a.length - 1, Math.floor(p * a.length))] : NaN);
const rms = (a) => (a.length ? Math.sqrt(a.reduce((s, v) => s + v * v, 0) / a.length) : NaN);
const wrap = (d) => ((((d + 180) % 360) + 360) % 360) - 180;
const f2 = (v) => (Number.isFinite(v) ? v.toFixed(2) : "n/a");

if (process.argv[2] === "--bench") {
  const csv = process.argv[3];
  const skip = Number(process.argv[4] || 30);
  const lines = fs.readFileSync(csv, "utf8").trim().split(/\r?\n/);
  const head = lines[0].split(",");
  const col = head.indexOf("interval_ms");
  const all = lines.slice(1).map((l) => Number(l.split(",")[col]));
  const v = all.slice(skip).filter(Number.isFinite);
  const n = v.length;
  const over = (t) => v.filter((x) => x > t).length;
  let jump2 = 0, jump4 = 0;
  for (let k = 1; k < n; k++) {
    const r = v[k] / v[k - 1];
    if (r > 2 || r < 0.5) jump2++;
    if (Math.abs(v[k] - v[k - 1]) > 4) jump4++;
  }
  console.log("n63-m1-chop --bench " + csv + ": rows " + all.length + ", the first " + skip + " left out (warm-up), " + n + " frames measured");
  console.log("  interval ms: p50 " + f2(pct(v, 0.5)) + ", p95 " + f2(pct(v, 0.95)) + ", p99 " + f2(pct(v, 0.99)) + ", p99.9 " + f2(pct(v, 0.999)) + ", max " + f2(pct(v, 1)));
  console.log("  over 8.33 ms (a 120 Hz refresh): " + over(8.33) + " (" + f2((100 * over(8.33)) / n) + " %); over 16.67 ms (60 Hz): " + over(16.67) + "; over 25 ms: " + over(25));
  console.log("  frame-to-frame: interval over 2x or under 0.5x the frame before " + jump2 + " (" + f2((100 * jump2) / n) + " %); a jump over 4 ms " + jump4);
  process.exit(0);
}

// a tag (Logs/feel/<tag>) or a directory path
const tagDir = (t) => (fs.existsSync(t) && fs.statSync(t).isDirectory() ? t : path.join(__dirname, "..", "..", "Logs", "feel", t));

if (process.argv[2] === "--pair") {
  const onTag = process.argv[3], offTag = process.argv[4] || process.argv[3];
  if (!onTag) {
    console.log("usage: node Tools/night/n63-m1-chop.cjs --pair <onTag> <offTag>   or   --pair <tag> (each side S against its twin S-off in the same tag)");
    process.exit(1);
  }
  const SGW = [-21, 14, 39, 54, 59, 54, 39, 14, -21].map((v) => v / 231);
  const evN = (e) => (typeof e === "string" ? e : e.type || e.name || "?");
  const load = (file) => {
    const byF = new Map();
    for (const line of fs.readFileSync(file, "utf8").split("\n")) {
      if (!line.trim()) continue;
      const j = JSON.parse(line);
      if (j.type === "frame" && j.cam) byF.set(j.f, j);
    }
    return byF;
  };
  const onRoot = tagDir(onTag), offRoot = tagDir(offTag);
  const has = (r, d) => fs.existsSync(path.join(r, d, "verbs37", "telemetry.jsonl"));
  const one = !process.argv[4]; // one tag: each side S with telemetry against its twin S-off (a second run with the row's switch)
  const pairs = !fs.existsSync(offRoot) ? []
    : one ? fs.readdirSync(onRoot).filter((d) => !d.endsWith("-off") && has(onRoot, d) && has(onRoot, d + "-off")).sort().map((d) => ({ side: d, on: path.join(onRoot, d), off: path.join(onRoot, d + "-off") }))
    : fs.readdirSync(offRoot).filter((d) => has(offRoot, d) && has(onRoot, d)).sort().map((d) => ({ side: d, on: path.join(onRoot, d), off: path.join(offRoot, d) }));
  console.log("n63-m1-chop --pair on " + onTag + (one ? " <side> / off <side>-off" : " / off " + offTag) + ": fixed-clock camera path; real frame pacing not checked (window 9 frames; the eye's own residual is cam.height through the same fit)");
  if (!pairs.length) {
    console.log("no level with verbs37 telemetry on both runs (" + onRoot + (one ? ": no side S with S-off" : ", " + offRoot) + ")");
    process.exit(1);
  }
  let allN = 0, allHalved = 0, allWorse = 0, allTotHalved = 0, allLeft = 0, allUnder = 0, worstView = 0;
  for (const pr of pairs) {
    const side = pr.side;
    let map = "?";
    try {
      map = JSON.parse(fs.readFileSync(path.join(pr.off, "reel.json"), "utf8")).map;
    } catch (e) {}
    const A = load(path.join(pr.off, "verbs37", "telemetry.jsonl")), B = load(path.join(pr.on, "verbs37", "telemetry.jsonl"));
    const fr = [...A.keys()].filter((f) => B.has(f)).sort((a, b) => a - b).map((f) => ({ off: A.get(f), on: B.get(f) }));
    const n = fr.length;
    const feet = (j) => j.cam.y - j.cam.height;
    let dFeet = 0, dX = 0, dZ = 0;
    for (const p of fr) {
      dFeet = Math.max(dFeet, Math.abs(feet(p.on) - feet(p.off)));
      dX = Math.max(dX, Math.abs(p.on.cam.x - p.off.cam.x));
      dZ = Math.max(dZ, Math.abs(p.on.cam.z - p.off.cam.z));
    }
    worstView = Math.max(worstView, dFeet, dX, dZ);
    // segments at the off run's reel teleports
    const seg = new Array(n).fill(0);
    for (let i = 1; i < n; i++) {
      const a = fr[i - 1].off, b = fr[i].off;
      const cut = Math.hypot(b.cam.x - a.cam.x, b.cam.y - a.cam.y, b.cam.z - a.cam.z) > 2.5 || b.f - a.f !== 1;
      seg[i] = seg[i - 1] + (cut ? 1 : 0);
    }
    const resid = (xs) => {
      const out = new Array(n).fill(null);
      for (let i = 4; i < n - 4; i++) {
        if (seg[i - 4] !== seg[i] || seg[i + 4] !== seg[i]) continue;
        let s = 0;
        for (let k = 0; k < 9; k++) s += SGW[k] * xs[i - 4 + k];
        out[i] = xs[i] - s;
      }
      return out;
    };
    const col = (side2, f) => fr.map((p) => f(p[side2]));
    const rEyeOff = resid(col("off", (j) => j.cam.height)), rEyeOn = resid(col("on", (j) => j.cam.height));
    const rFeet = resid(col("off", feet));
    const rYOff = resid(col("off", (j) => j.cam.y)), rYOn = resid(col("on", (j) => j.cam.y));
    const rXOff = resid(col("off", (j) => j.cam.x)), rXOn = resid(col("on", (j) => j.cam.x));
    const rZOff = resid(col("off", (j) => j.cam.z)), rZOn = resid(col("on", (j) => j.cam.z));
    const tot = (rx, ry, rz, i) => (ry[i] === null ? null : Math.hypot(rx[i] || 0, ry[i], rz[i] || 0));
    const peak = (fn, i) => {
      let best = 0;
      for (let k = Math.max(0, i - 4); k <= Math.min(n - 1, i + 4); k++) {
        const v = fn(k);
        if (v !== null) best = Math.max(best, Math.abs(v));
      }
      return best * 100;
    };
    // the off run's events: eye-snap runs (> 5 cm a frame) and every landing
    const ev = [];
    let cur = null;
    for (let i = 1; i < n; i++) {
      const d = seg[i] === seg[i - 1] ? (fr[i].off.cam.height - fr[i - 1].off.cam.height) * 100 : 0;
      if (Math.abs(d) <= 5) continue;
      const dir = d < 0 ? "down" : "up";
      if (cur && cur.dir === dir && cur.last === i - 1) cur.last = i;
      else {
        cur = { i, dir, last: i, snap: true };
        ev.push(cur);
      }
    }
    for (let i = 0; i < n; i++)
      if ((fr[i].off.events || []).some((e) => evN(e) === "land") && !ev.some((e) => Math.abs(e.i - i) <= 3)) ev.push({ i, dir: "", snap: false });
    ev.sort((a, b) => a.i - b.i);
    // the reel placing the player: the feet step over 10 cm with |up| under 2 m/s on the frame and the one before
    const placed = [];
    for (let k = 1; k < n; k++) {
      if (seg[k] !== seg[k - 1]) continue;
      const fy = (feet(fr[k].off) - feet(fr[k - 1].off)) * 100;
      const up0 = Math.abs((fr[k].off.vel || {}).up || 0), up1 = Math.abs((fr[k - 1].off.vel || {}).up || 0);
      if (Math.abs(fy) > 10 && up0 < 2 && up1 < 2) placed.push(k);
    }
    console.log("");
    console.log("== " + side + " " + map + ": " + n + " frames on both runs; VIEW ONLY: the feet differ by at most " + f2(dFeet * 1000) + " mm, x " + f2(dX * 1000) + " mm, z " + f2(dZ * 1000) + " mm" + (Math.max(dFeet, dX, dZ) <= 0.0011 ? " (the telemetry's rounding: the sim the same)" : " (MORE than the telemetry's rounding: the sim moved)"));
    let N = 0, halved = 0, worse = 0, totHalved = 0, left = 0, under = 0;
    for (const e of ev) {
      const near = new Set();
      for (let k = Math.max(0, e.i - 3); k <= Math.min(n - 1, e.i + 3); k++) for (const x of fr[k].off.events || []) near.add(evN(x));
      const kind = (near.has("slide") ? "slide" : near.has("land") ? "landing" : e.dir === "up" ? "slide exit" : "no event") + (e.dir ? " " + e.dir : "");
      const f = fr[e.i].off.f;
      if (placed.some((k) => e.i >= k - 4 && e.i <= k + 8)) {
        left++;
        console.log("    f" + f + " " + kind + ": left out (the reel placing the player)");
        continue;
      }
      const eOff = peak((k) => rEyeOff[k], e.i), eOn = peak((k) => rEyeOn[k], e.i);
      const tOff = peak((k) => tot(rXOff, rYOff, rZOff, k), e.i), tOn = peak((k) => tot(rXOn, rYOn, rZOn, k), e.i);
      const fp = peak((k) => rFeet[k], e.i);
      const h = eOn <= eOff / 2;
      if (eOff <= 2.0) {
        under++;
        console.log("    f" + f + " " + kind + ": the eye's own " + f2(eOff) + " -> " + f2(eOn) + " cm: under the 2 cm chop line on the off run (not a hitch: listed, not counted)");
        continue;
      }
      N++;
      if (h) halved++;
      if (eOn > eOff) worse++;
      if (tOn <= tOff / 2) totHalved++;
      console.log("    f" + f + " " + kind + ": the eye's own " + f2(eOff) + " -> " + f2(eOn) + " cm" + (h ? " HALVED" : eOn > eOff ? " WORSE" : " not halved") + " | total " + f2(tOff) + " -> " + f2(tOn) + " cm | the feet's own " + f2(fp) + " cm");
    }
    const whole = (rx, ry, rz) => {
      const a = [];
      for (let i = 0; i < n; i++) {
        const v = tot(rx, ry, rz, i);
        if (v !== null) a.push(v * 100);
      }
      return a;
    };
    const wOff = whole(rXOff, rYOff, rZOff), wOn = whole(rXOn, rYOn, rZOn);
    console.log("  events " + N + " (left out " + left + ", under the chop line " + under + "): the eye's own residual halved at " + halved + ", worse at " + worse + "; the total halved at " + totHalved);
    console.log("  whole path cm: rms " + f2(rms(wOff)) + " -> " + f2(rms(wOn)) + ", p95 " + f2(pct(wOff, 0.95)) + " -> " + f2(pct(wOn, 0.95)) + ", frames over 2 cm " + wOff.filter((v) => v > 2).length + " -> " + wOn.filter((v) => v > 2).length);
    allN += N; allHalved += halved; allWorse += worse; allTotHalved += totHalved; allLeft += left; allUnder += under;
  }
  console.log("");
  console.log("ALL " + pairs.length + " levels: events " + allN + " (left out " + allLeft + ", under the chop line " + allUnder + "): the eye's own residual halved at " + allHalved + ", worse at " + allWorse + "; the total halved at " + allTotHalved + "; view only: " + (worstView <= 0.0011 ? "yes" : "NO") + " (largest feet / x / z difference " + f2(worstView * 1000) + " mm)");
  console.log("eye63 measure (D-10: the eye's own residual halved at every slide and landing event): " + (allN > 0 && allHalved === allN ? "MET" : "NOT MET (" + (allN - allHalved) + " of " + allN + " not halved)"));
  process.exit(0);
}

const tag = process.argv[2] || "n62-freeze";
const root = tagDir(tag);
const WIN = 9; // frames in the smoothing window (0.15 s at 60 fps): slower motion than this is the path, faster is chop
const MINV = 1.0; // m/s: the heading of a near-still camera is noise
const TELEPORT_M = 2.5; // a frame-to-frame move over this (150 m/s) is a reel reset, not motion
const CHOP_CM = 2.0; // a residual over 2 cm in one frame at 1.5-3 m from the view's near content is a visible hitch (our line, labelled)
const EYE_CM = 5.0; // an eye-height step over 5 cm in one 1/60 s frame (3 m/s of pure camera motion) is a snap (our line, labelled)
const SG = [-21, 14, 39, 54, 59, 54, 39, 14, -21].map((v) => v / 231); // the 9-point quadratic Savitzky-Golay smoothing weights
const evName = (e) => (typeof e === "string" ? e : e.type || e.name || "?");

console.log("n63-m1-chop " + tag + ": fixed-clock camera path; real frame pacing not checked (window " + WIN + " frames, chop line " + CHOP_CM + " cm, eye snap line " + EYE_CM + " cm / frame)");
const dirs = fs.existsSync(root) ? fs.readdirSync(root).filter((d) => fs.existsSync(path.join(root, d, "verbs37", "telemetry.jsonl"))).sort() : [];
if (!dirs.length) {
  console.log("no Logs/feel/" + tag + "/v*/verbs37/telemetry.jsonl");
  process.exit(1);
}
for (const side of dirs) {
  let map = "?";
  try {
    map = JSON.parse(fs.readFileSync(path.join(root, side, "reel.json"), "utf8")).map;
  } catch (e) {}
  const byF = new Map();
  for (const line of fs.readFileSync(path.join(root, side, "verbs37", "telemetry.jsonl"), "utf8").split("\n")) {
    if (!line.trim()) continue;
    const j = JSON.parse(line);
    if (j.type === "frame" && j.cam) byF.set(j.f, j); // a frame logged twice keeps its last line
  }
  const fr = [...byF.values()].sort((a, b) => a.f - b.f);
  const n = fr.length;
  const evAt = (i, r) => {
    const s = new Set();
    for (let k = Math.max(0, i - r); k <= Math.min(n - 1, i + r); k++) for (const e of fr[k].events || []) s.add(evName(e));
    return [...s];
  };
  // segments split at reel teleports (a reset between verbs): never smooth across one
  const cut = new Array(n).fill(false);
  for (let i = 1; i < n; i++) {
    const a = fr[i - 1].cam, b = fr[i].cam;
    if (Math.hypot(b.x - a.x, b.y - a.y, b.z - a.z) > TELEPORT_M || fr[i].f - fr[i - 1].f !== 1) cut[i] = true;
  }
  const seg = new Array(n).fill(0);
  for (let i = 1; i < n; i++) seg[i] = seg[i - 1] + (cut[i] ? 1 : 0);
  const half = (WIN - 1) / 2;
  const res = [], resY = [], resP = [], resH = [], worst = [];
  let steps2 = 0, held = 0, chopFrames = 0;
  // the eye-height snaps, as events (consecutive snap frames of one direction)
  const snapEvents = [];
  let cur = null;
  for (let i = 1; i < n; i++) {
    const d = seg[i] === seg[i - 1] ? (fr[i].cam.height - fr[i - 1].cam.height) * 100 : 0;
    const snap = Math.abs(d) > EYE_CM;
    const dir = d < 0 ? "down" : "up";
    if (snap && cur && cur.dir === dir && cur.last === i - 1) {
      cur.frames++; cur.total += d; cur.peak = Math.max(cur.peak, Math.abs(d)); cur.last = i;
    } else if (snap) {
      cur = { f: fr[i].f, i, last: i, dir, frames: 1, total: d, peak: Math.abs(d) };
      snapEvents.push(cur);
    }
  }
  for (const e of snapEvents) {
    const ev = evAt(e.i, 3);
    const what = ev.includes("slide") ? "slide" : ev.includes("land") ? "landing" : "no event";
    e.kind = what + " " + e.dir;
    e.ev = ev;
  }
  const kinds = {};
  for (const e of snapEvents) kinds[e.kind] = (kinds[e.kind] || 0) + 1;
  // every landing's largest eye step in its next 4 frames
  const lands = [];
  for (let i = 0; i < n; i++) {
    if (!(fr[i].events || []).some((e) => evName(e) === "land")) continue;
    let big = 0;
    for (let k = i; k <= Math.min(n - 1, i + 4); k++) if (k > 0 && seg[k] === seg[k - 1]) big = Math.max(big, Math.abs(fr[k].cam.height - fr[k - 1].cam.height) * 100);
    lands.push(big);
  }
  const heading = (i) => {
    if (i < 1 || seg[i] !== seg[i - 1]) return NaN;
    const dx = fr[i].cam.x - fr[i - 1].cam.x, dz = fr[i].cam.z - fr[i - 1].cam.z;
    return Math.hypot(dx, dz) * 60 >= MINV ? (Math.atan2(dx, dz) * 180) / Math.PI : NaN;
  };
  for (let i = 0; i < n; i++) {
    if (fr[i].steps === 2) steps2++;
    if (fr[i].held) held++;
    if (i - half < 0 || i + half >= n || seg[i - half] !== seg[i] || seg[i + half] !== seg[i]) continue;
    let sx = 0, sy = 0, sz = 0, sp = 0, hc = 0, sinS = 0, cosS = 0;
    for (let k = i - half; k <= i + half; k++) {
      const c = fr[k].cam, w = SG[k - i + half];
      sx += w * c.x; sy += w * c.y; sz += w * c.z; sp += w * c.pitch;
      const h = heading(k);
      if (Number.isFinite(h)) { sinS += Math.sin((h * Math.PI) / 180); cosS += Math.cos((h * Math.PI) / 180); hc++; }
    }
    const c = fr[i].cam;
    const dx = c.x - sx, dy = c.y - sy, dz = c.z - sz;
    const r = Math.hypot(dx, dy, dz) * 100;
    res.push(r);
    resY.push(Math.abs(dy) * 100);
    resP.push(Math.abs(c.pitch - sp));
    const h = heading(i);
    if (Number.isFinite(h) && hc >= half) resH.push(Math.abs(wrap(h - (Math.atan2(sinS, cosS) * 180) / Math.PI)));
    if (r > CHOP_CM) chopFrames++;
    worst.push({ i, r, dy: dy * 100 });
  }
  worst.sort((a, b) => b.r - a.r);
  const distinct = [];
  for (const w of worst) {
    if (distinct.some((d) => Math.abs(d.i - w.i) <= 4)) continue;
    distinct.push(w);
    if (distinct.length === 10) break;
  }
  console.log("");
  console.log("== " + side + " " + map + ": frames " + n + " (2 steps each " + steps2 + ", held " + held + "), segments " + (seg[n - 1] + 1) + " (reel teleports cut)");
  console.log("  position residual cm: rms " + f2(rms(res)) + ", p95 " + f2(pct(res, 0.95)) + ", max " + f2(pct(res, 1)) + "; vertical cm rms " + f2(rms(resY)) + ", p95 " + f2(pct(resY, 0.95)));
  console.log("  pitch residual deg: rms " + f2(rms(resP)) + ", p95 " + f2(pct(resP, 0.95)) + "; heading residual deg (moving >= " + MINV + " m/s, " + resH.length + " frames): rms " + f2(rms(resH)) + ", p95 " + f2(pct(resH, 0.95)));
  console.log("  frames over the " + CHOP_CM + " cm chop line: " + chopFrames + " of " + res.length + " (" + f2((100 * chopFrames) / Math.max(1, res.length)) + " %)");
  console.log("  eye snaps over " + EYE_CM + " cm / frame: " + snapEvents.reduce((s, e) => s + e.frames, 0) + " frames in " + snapEvents.length + " events: " + Object.entries(kinds).map(([k, v]) => k + " " + v).join(", "));
  for (const e of snapEvents) console.log("    f" + e.f + " " + e.kind + ": " + e.frames + " frames, eye " + f2(e.total) + " cm in all, peak " + f2(e.peak) + " cm / frame" + (e.ev.length ? " (events " + e.ev.join(" / ") + ")" : ""));
  const landOver = lands.filter((v) => v > EYE_CM).length;
  console.log("  landings: " + lands.length + "; the largest eye step in a landing's next 4 frames: over " + EYE_CM + " cm in " + landOver + ", at most " + EYE_CM + " cm in " + (lands.length - landOver) + " (range " + f2(Math.min(...lands)) + "-" + f2(Math.max(...lands)) + " cm)");
  console.log("  worst " + distinct.length + " distinct events (frame, residual cm, vertical cm; the frame's vertical step split feet / eye; the cause from the telemetry):");
  for (const w of distinct) {
    const i = w.i, a = fr[Math.max(0, i - 1)], c = fr[i];
    const feet = (c.cam.y - c.cam.height - (a.cam.y - a.cam.height)) * 100, eye = (c.cam.height - a.cam.height) * 100;
    const ev = evAt(i, 3);
    const cause = [];
    if (ev.length) cause.push("events +-3 frames: " + ev.join(" / "));
    let upMax = 0;
    for (let k = Math.max(0, i - 3); k <= Math.min(n - 1, i + 3); k++) upMax = Math.max(upMax, Math.abs((fr[k].vel || {}).up || 0));
    if (upMax >= 30) cause.push("a vertical speed of " + f2(upMax) + " m/s within +-3 frames (a slam or a launch: the feet's motion)");
    let dSpd = 0, feetStep = 0;
    for (let k = Math.max(1, i - 2); k <= Math.min(n - 1, i + 2); k++) {
      if (seg[k] !== seg[k - 1]) continue;
      const s1 = (fr[k].vel || {}).speed || 0, s0 = (fr[k - 1].vel || {}).speed || 0;
      if (Math.abs(s1 - s0) > Math.abs(dSpd)) dSpd = s1 - s0;
      const fy = (fr[k].cam.y - fr[k].cam.height - (fr[k - 1].cam.y - fr[k - 1].cam.height)) * 100;
      // a step: the feet move while the sim's vertical speed is ~0 on both frames (a landing's frame has fallen the frame before)
      if (Math.abs((fr[k].vel || {}).up || 0) < 2 && Math.abs((fr[k - 1].vel || {}).up || 0) < 2 && Math.abs(fy) > Math.abs(feetStep)) feetStep = fy;
    }
    if (Math.abs(dSpd) > 3) cause.push("a horizontal speed change of " + f2(dSpd) + " m/s in one frame within +-2 (not named: a dash, a wall hit or the reel placing a state)");
    if (Math.abs(feetStep) > 10) cause.push("the feet moved " + f2(feetStep) + " cm vertically in one frame with a vertical speed under 2 m/s on it and the frame before (not named: a step, or the reel placing the player; M1 round 2's GATE: f267 / f342 are the reel's 0.2 m placement)");
    if (Math.abs(eye) > EYE_CM) cause.push("an eye snap " + f2(eye) + " cm");
    if (c.flow !== "Playing") cause.push("flow " + c.flow);
    if (!cause.length) cause.push("cause: none found in the telemetry");
    const v = c.vel || {};
    console.log("    f" + c.f + " t " + f2(c.t) + " s: " + f2(w.r) + " cm (y " + f2(w.dy) + "); step feet " + f2(feet) + " / eye " + f2(eye) + " cm; speed " + f2(v.speed) + " m/s, up " + f2(v.up) + "; " + cause.join("; "));
  }
}

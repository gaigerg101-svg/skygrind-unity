// NIGHT36 builder L (NIGHT36.md L1, PLAN.md 3.3: "every prop has a job, or it doesn't exist"): the n36 level checker. It reads a level PLAN
// (Design/levels/n36/<name>.plan.json, the file gen-n36-*.cjs writes) and FAILS (exit 1) when:
//   1. a solid (a box, a ramp, a pit or lava row, a tunnel, a prop with a collider) has no `job` in JOBS or no `why`;
//   2. a lava pool has no enemy spot (a room node / high node) and no enemy route rect (a room's bridge36 / rims36) within REACH metres of its
//      edge: lava an enemy can never be launched into is decoration (design-web.md rule 1; lava kills enemies: Core/Lava30.cs);
//   3. a fight room (one with waves) has an enemy spot above 0.6 m, or a deck top above 0.6 m, and no `climb` ramp / box in the room: a perch
//      with no enemy route up (design-web.md rule 2; GG NIGHT20: "There always really needs to be a route that enemies can climb up").
// REACH: design.n36.launch36.reachM when builder P's row has it, else design.n36.level36.launchReachM (this builder's row), else 4 m (the beat sheet).
// Usage: node Tools/night/n36-jobs.cjs [plan.json ...]     (default: Design/levels/n36/foundry.plan.json)
//        node Tools/night/n36-jobs.cjs --selftest           (plants one error of each kind in a copy: all three must FAIL, the clean plan PASS)
const fs = require("fs");
const path = require("path");

const JOBS = ["line", "cover", "climb", "hazard", "marker", "reward"];
const ROOT = path.join(__dirname, "../..");

function reach() {
  try {
    const d = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8")).design.n36 || {};
    if (d.launch36 && typeof d.launch36.reachM === "number") return { m: d.launch36.reachM, src: "design.n36.launch36.reachM" };
    if (d.level36 && typeof d.level36.launchReachM === "number") return { m: d.level36.launchReachM, src: "design.n36.level36.launchReachM" };
  } catch (e) { /* no config: the default */ }
  return { m: 4, src: "the beat sheet's 4 m (no config row)" };
}

/// distance on the floor plan from a point (x, z) to a rect [x0, z0, x1, z1]
function distRect(x, z, r) {
  const dx = Math.max(r[0] - x, 0, x - r[2]), dz = Math.max(r[1] - z, 0, z - r[3]);
  return Math.hypot(dx, dz);
}
/// distance between two rects (0 when they touch or overlap)
function rectRect(a, b) {
  const dx = Math.max(a[0] - b[2], 0, b[0] - a[2]), dz = Math.max(a[1] - b[3], 0, b[1] - a[3]);
  return Math.hypot(dx, dz);
}
const inRect = (x, z, r) => x >= r[0] && x <= r[2] && z >= r[1] && z <= r[3];

function check(plan, REACH) {
  const fails = [], notes = [];
  const tag = (what, o) => {
    if (!JOBS.includes(o.job)) fails.push(`1 untagged: ${what} has job "${o.job}" (not one of ${JOBS.join("/")})`);
    else if (!o.why || String(o.why).trim().length < 8) fails.push(`1 no why: ${what} (job ${o.job})`);
  };
  (plan.boxes || []).forEach((b, i) => tag(`box ${i} (${b.zone}, ${b.kind})`, b));
  (plan.ramps || []).forEach((b, i) => tag(`ramp ${i} (${b.zone})`, b));
  (plan.pits || []).forEach((b, i) => tag(`${b.lava ? "lava" : "pit"} ${i} (${b.zone})`, b));
  (plan.tunnels || []).forEach((b, i) => tag(`tunnel ${i} (${b.zone})`, b));
  (plan.props || []).filter((p) => p.collider).forEach((p) => tag(`solid prop ${p.id} (${p.kind})`, p));

  const rooms = plan.rooms || [];
  const byId = Object.fromEntries(rooms.map((r) => [r.id, r]));
  // 2. lava within REACH of an enemy spot or route
  (plan.pits || []).filter((p) => p.lava).forEach((p, i) => {
    const r = byId[p.zone];
    const spots = r ? [...(r.nodes || []), ...(r.highNodes || [])] : [];
    const routes = r ? [...((r.bridge36 && [r.bridge36.rect]) || []), ...((r.rims36 && r.rims36.rects) || [])] : [];
    let best = Infinity, by = "";
    for (const s of spots) { const d = distRect(s[0], s[2], p.rect); if (d < best) { best = d; by = `spot (${s[0]}, ${s[1]}, ${s[2]})`; } }
    for (const q of routes) { const d = rectRect(q, p.rect); if (d < best) { best = d; by = `route rect [${q.join(", ")}]`; } }
    if (!(r && r.waves && r.waves.length)) fails.push(`2 lava ${i} (${p.zone}): the room has no enemies: no enemy can ever be launched into it`);
    else if (best > REACH.m) fails.push(`2 lava ${i} (${p.zone}): the nearest enemy spot / route is ${best === Infinity ? "none" : best.toFixed(2) + " m (" + by + ")"}: more than ${REACH.m} m (${REACH.src})`);
    else notes.push(`lava ${i} (${p.zone}): an enemy ${by} is ${best.toFixed(2)} m from it (reach ${REACH.m} m)`);
  });
  // 3. a climb route in every fight room with a raised enemy spot or a raised deck
  for (const r of rooms) {
    if (!(r.waves && r.waves.length)) continue;
    const high = [...(r.nodes || []), ...(r.highNodes || [])].filter((n) => n[1] > 0.6);
    const decks = (plan.boxes || []).filter((b) => b.zone === r.id && b.kind === "deck" && b.max[1] > 0.6 && b.min[1] <= 0.6);
    if (!high.length && !decks.length) { notes.push(`${r.id}: flat (no raised spot or deck): its floor is the route`); continue; }
    const climbs = [...(plan.ramps || []).filter((q) => q.zone === r.id && q.job === "climb"), ...(plan.boxes || []).filter((b) => b.zone === r.id && b.job === "climb")];
    if (!climbs.length) fails.push(`3 no climb: ${r.id} has ${high.length} raised enemy spot(s) and ${decks.length} raised deck(s) but no climb ramp / stair`);
    else notes.push(`${r.id}: ${high.length} raised spot(s), ${decks.length} deck(s), ${climbs.length} climb route(s)`);
  }
  return { fails, notes };
}

function report(name, plan, REACH) {
  const { fails, notes } = check(plan, REACH);
  const solids = (plan.boxes || []).length + (plan.ramps || []).length + (plan.pits || []).length + (plan.tunnels || []).length + (plan.props || []).filter((p) => p.collider).length;
  console.log(`n36-jobs ${name}: ${solids} solids, ${(plan.pits || []).filter((p) => p.lava).length} lava pools, ${(plan.rooms || []).length} rooms`);
  for (const n of notes) console.log("  ok   " + n);
  for (const f of fails) console.log("  FAIL " + f);
  console.log(`  ${fails.length ? "RED" : "GREEN"}: ${fails.length} failure(s)`);
  return fails;
}

const args = process.argv.slice(2);
const REACH = reach();
if (args[0] === "--selftest") {
  const file = path.join(ROOT, "Design/levels/n36/foundry.plan.json");
  const clean = JSON.parse(fs.readFileSync(file, "utf8"));
  const ok = report("clean", clean, REACH).length === 0;
  const plant = (label, f) => { const p = JSON.parse(JSON.stringify(clean)); f(p); const n = check(p, REACH).fails.length; console.log(`  plant ${label}: ${n ? "caught (" + n + ")" : "MISSED"}`); return n > 0; };
  const a = plant("an untagged box", (p) => { delete p.boxes[0].job; });
  const b = plant("lava far from every enemy", (p) => { const r = p.rooms.find((q) => (p.pits || []).some((l) => l.lava && l.zone === q.id)); r.nodes = [[r.rect[0] + 0.5, 0, r.rect[1] + 0.5]]; delete r.highNodes; delete r.bridge36; delete r.rims36; p.pits.filter((l) => l.lava && l.zone === r.id).forEach((l) => { l.rect = [r.rect[2] - 2, r.rect[3] - 2, r.rect[2] - 1, r.rect[3] - 1]; }); });
  const c = plant("a perch with no climb", (p) => { const r = p.rooms.find((q) => (q.highNodes || []).length && q.waves); p.ramps = p.ramps.filter((q) => q.zone !== r.id); p.boxes.forEach((x) => { if (x.zone === r.id && x.job === "climb") x.job = "line"; }); });
  const all = ok && a && b && c;
  console.log(`n36-jobs selftest: ${all ? "GREEN" : "RED"} (clean ${ok ? "passes" : "FAILS"}, three plants ${a && b && c ? "caught" : "NOT all caught"})`);
  process.exit(all ? 0 : 1);
}
const files = args.length ? args : [path.join(ROOT, "Design/levels/n36/foundry.plan.json")];
let bad = 0;
for (const f of files) bad += report(path.basename(f), JSON.parse(fs.readFileSync(f, "utf8")), REACH).length;
process.exit(bad ? 1 : 0);

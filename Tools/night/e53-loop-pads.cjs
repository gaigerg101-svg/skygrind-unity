// EXT53 row W (the lead): the I4 pads moved to the nearest spot Tools/night/e53-padspot.py found (2.0 m off every through walk line at the
// pad's height, 0.6 m off solids, off ramps, on a nav node at the pad's height, throw <= 1.15 x today's). DECIDE53 D-12 had answered spire's
// as having no such spot: the search found them (D-17). Each pad keeps its target; hold52 --pads (n53-contract-all.sh) decides each.
// Tried and reverted (11:04, Logs/n53/e53-pads-contract.txt): v5's chamber hoist -> [-14, 0, -56.75] (pad probe: landed 4.77 m short),
// kb2's east pad -> [5, 0, -89.5] (landed no), p5's pad -> [-5.5, 0, -130.5] (the room's dwell 2.75 -> 3.70 s past its 0.7 s grace);
// 11:56 retries: v5's chamber hoist -> [-12.25, 0, -57.75] KEPT (lands 0.02 m, 15/15); p5 -> [-6, 0, -130.5] reverted again (dwell 3.90 s).
//   node Tools/night/e53-loop-pads.cjs [--dry]   (idempotent: a pad already at its new spot is skipped)
const fs = require("fs");
const path = require("path");
const moves = [
  ["spire", "v2", [-4.5, 0, 61], [-6.75, 0, 61]],
  ["spire", "v2", [-20, 2.4, 72], [-18, 2.4, 72]],
  ["spire", "v5", [-1, 2.5, -55], [-2.25, 2.5, -55.25]],
  ["spire", "v6", [-0.5, 0, -94.5], [-4, 0, -94.25]],
  ["spire", "v5", [-13.5, 0, -56], [-12.25, 0, -57.75]], // 11:55 retry: the same throw (7.3 m) and bearing (17 deg off), 2.2 m east
];
const dry = process.argv.includes("--dry");
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
for (const lv of [...new Set(moves.map((m) => m[0]))]) {
  const file = path.join(__dirname, "../../Design/levels/n52/" + lv + ".plan.json");
  const text = fs.readFileSync(file, "utf8");
  const plan = JSON.parse(text), before = JSON.parse(text);
  const done = [];
  for (const [l, room, from, to] of moves) {
    if (l !== lv) continue;
    const pads = (plan.pads || []).filter((p) => p.zone === room);
    if (pads.some((p) => same(p.at, to))) { console.log(lv + " " + room + " " + JSON.stringify(to) + ": already moved"); continue; }
    const f = pads.filter((p) => same(p.at, from));
    if (f.length !== 1) throw new Error(lv + " " + room + " pad " + JSON.stringify(from) + ": found " + f.length);
    f[0].at = to;
    f[0].why += " (EXT53 e53-loop-pads.cjs, I4: off the walk line, " + JSON.stringify(from) + " -> " + JSON.stringify(to) + ")";
    if (f[0].note !== undefined) f[0].note = f[0].why;
    done.push(room);
  }
  const rooms = new Set(moves.filter((m) => m[0] === lv).map((m) => m[1]));
  for (const k of Object.keys(before)) {
    if (k === "pads") {
      const keep = (p) => !rooms.has(p.zone);
      if (!same(before.pads.filter(keep), plan.pads.filter(keep))) throw new Error(lv + ": pads outside the rooms changed");
    } else if (!same(before[k], plan[k])) throw new Error(lv + ": " + k + " changed");
  }
  console.log(lv + ": moved in " + (done.join(", ") || "none"));
  if (!dry && done.length) fs.writeFileSync(file, JSON.stringify(plan, null, 1) + "\n");
}
if (dry) console.log("DRY RUN");

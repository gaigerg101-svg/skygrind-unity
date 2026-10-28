// NIGHT53 the loop, cycle 4 (the lead's; queue q4b): I4 on the lead's levels: jump pads off the walk pilot's lines (L-087: a pad whose 1 m
// trigger sits within 1 m of a walk line fires on every pass). Each pad keeps its target and its job; it moves the least that clears 2.0 m
// from every through line at its height (Tools/night/n53-identity.py I4) and keeps 0.5 m from every solid's footprint (asserted here).
//   iceline g7 THE RETURN HOIST [0, 0, -208] -> [1.3, 0, -208]   (1.62 m off line 0's sluice-post leg)
//   (iceline g8's cross hoist was moved to [-9.6, 0, -228.5] and reverted: hold52 --pads 0/1, the 25 m throw to the far gallery missed)
// Not moved (answered in DECIDE53 D-12): v2's bell hoist (on line 1's 2.5 m lane beside the east aisle deck), v5's chamber hoist (its spot 2 m off the circuit is the chamber ramp's foot), v2's aisle hoist and v5's gallery hoist (each on a 2-3 m wide walkway: no spot 2 m off its line),
// v6's bell hoist (1.02 m; the east ride wall 1 m east of it).
//   node Tools/night/n53-loop-i4.cjs [--dry]; then bash Tools/night/n52-loop-regen.sh iceline g7 g8
const fs = require("fs");
const path = require("path");
const moves = [
  ["iceline", "g7", [0, 0, -208], [1.3, 0, -208]],
];
const dry = process.argv.includes("--dry");
for (const lv of ["iceline"]) {
  const file = path.join(__dirname, "../../Design/levels/n52/" + lv + ".plan.json");
  const text = fs.readFileSync(file, "utf8");
  const plan = JSON.parse(text), before = JSON.parse(text);
  const mine = [];
  for (const [l, room, from, to] of moves) {
    if (l !== lv) continue;
    const f = plan.pads.filter((p) => p.zone === room && JSON.stringify(p.at) === JSON.stringify(from));
    if (f.length === 0 && plan.pads.some((p) => p.zone === room && JSON.stringify(p.at) === JSON.stringify(to))) { console.log(lv + " " + room + ": already moved"); continue; }
    if (f.length !== 1) throw new Error(lv + " " + room + " pad " + JSON.stringify(from) + ": " + f.length);
    for (const b of plan.boxes.filter((b) => b.zone === room && b.max[1] > to[1] + 0.3 && b.min[1] < to[1] + 2)) {
      const dx = Math.max(b.min[0] - to[0], 0, to[0] - b.max[0]), dz = Math.max(b.min[2] - to[2], 0, to[2] - b.max[2]);
      if (Math.hypot(dx, dz) < 0.5) throw new Error(lv + " " + room + ": the new pad spot " + JSON.stringify(to) + " is within 0.5 m of a solid " + JSON.stringify([b.min, b.max]));
    }
    for (const r of plan.ramps.filter((r) => r.zone === room)) {
      const [fx, fz] = r.from, [tx, tz] = r.to, L = Math.hypot(tx - fx, tz - fz), ux = (tx - fx) / L, uz = (tz - fz) / L;
      const a = (to[0] - fx) * ux + (to[2] - fz) * uz, s = Math.abs(-(to[0] - fx) * uz + (to[2] - fz) * ux);
      if (a > -0.5 && a < L + 0.5 && s < r.width / 2 + 0.5) throw new Error(lv + " " + room + ": the new pad spot is on a ramp");
    }
    f[0].at = to;
    f[0].why += " (NIGHT53 n53-loop-i4.cjs, I4: moved off the walk line, " + JSON.stringify(from) + " -> " + JSON.stringify(to) + ")";
    if (f[0].note !== undefined) f[0].note = f[0].why;
    mine.push(room);
  }
  for (const k of Object.keys(before)) {
    if (k === "pads") {
      const keep = (p) => !mine.includes(p.zone);
      if (JSON.stringify(before.pads.filter(keep)) !== JSON.stringify(plan.pads.filter(keep))) throw new Error("pads outside the rooms changed");
    } else if (JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(lv + ": " + k + " changed");
  }
  console.log(lv + ": pads moved in " + mine.join(", "));
  if (!dry) fs.writeFileSync(file, JSON.stringify(plan, null, 1) + "\n");
}
if (dry) console.log("DRY RUN");

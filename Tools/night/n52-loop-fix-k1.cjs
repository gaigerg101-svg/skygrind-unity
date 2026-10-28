// NIGHT52 the room loop, fix K1 (builder K's QA sweep, the lead's yes to all four): iceline g8 and g5 in Design/levels/n52/iceline.plan.json.
//   1. g8 THE CROSS HOIST: its arc from [-8.3, 0, -237.5] to [6.5, 5.5, -248] clipped the NW column's corner (x -3.75, z -240.75; 0.04 m from the
//      line): hold52 --pads landed it on the east ramp at [8.46, 2.74, -240.4], 7.85 m off. The line now threads the W / NW columns' gap
//      1.10 m clear of every column and tower (a search over lane W pads and signal-box targets: the gap's window allows ~1.1 m at most).
//   2. g8: no 0-hit perch (wave 1 is one charger: hold52 --perches found 6 of 9 tops with 0 hits in 20 s): the two lane ride walls, the two
//      sand towers, the gallery's hopper and the signal box's lever frame run to the 12 m roof (the rides stay rides, the towers charge stops).
//   3. g5: the two far posts stood 0.9 m off the shelf's / crane deck's inner faces (slots): flush to them now.
//   4. g8: the sand towers stood 1.35 m off the lane walls' faces (and the east one 1.3 m off the SE column): 1.0 m wide now, centred between
//      the lane wall's face (x -10.4 / 10.4) and the columns' line (x -6.25 / 6.25): 1.575 m both sides.
// Asserts the state it fixes (port/stage3 1fbca8be). Run: node Tools/night/n52-loop-fix-k1.cjs, then bash Tools/night/n52-loop-regen.sh iceline g5 g8
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const TAG = "NIGHT52 fix K1 (n52-loop-fix-k1.cjs, the QA sweep)";
const box = (zone, min, max) => {
  const b = plan.boxes.find((x) => x.zone === zone && eq(x.min, min) && eq(x.max, max));
  if (!b) throw new Error(`${zone}: no box ${JSON.stringify(min)}..${JSON.stringify(max)} (already run?)`);
  return b;
};
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};
// 1. the cross hoist
const pad = plan.pads.find((p) => p.zone === "g8" && eq(p.at, [-8.3, 0, -237.5]) && eq(p.target, [6.5, 5.5, -248]));
if (!pad) throw new Error("g8's cross hoist is not 1fbca8be's (already run?)");
pad.at = [-8.5, 0, -236];
pad.target = [7, 5.5, -247];
note(pad, "moved 1.5 m north and aimed 1 m in on the signal box: the arc clipped the NW column's corner (hold52 --pads: landed 7.85 m off on the east ramp); its line now clears every column and tower by 1.10 m");
// 2. the perches to the roof
const ROOF = 12;
for (const [min, max, why] of [
  [[-11, 0, -246], [-10.4, 5, -223], "to the roof: its 5 m top was a 0-hit perch against wave 1's charger"],
  [[10.4, 0, -246], [11, 5, -223], "to the roof: its 5 m top was a 0-hit perch against wave 1's charger"],
  [[-11, 3, -250], [-9, 5.5, -248], "to the roof: its top was a 0-hit perch"],
  [[9, 5.5, -250], [11, 7.5, -248], "to the roof: its top was a 0-hit perch"],
]) {
  const b = box("g8", min, max);
  b.max = [b.max[0], ROOF, b.max[2]];
  note(b, why);
}
// 2 + 4. the sand towers: to the roof, 1.0 m wide, centred between the lane wall's face and the columns' line
const tw = box("g8", [-9.05, 0, -232.75], [-7.55, 4, -231.25]);
tw.min = [-8.825, 0, -232.75];
tw.max = [-7.825, ROOF, -231.25];
note(tw, "to the roof (its 4 m top was a 0-hit perch) and 1.0 m wide, centred between the lane wall's face (x -10.4) and the columns' line (x -6.25): 1.575 m both sides (it stood 1.35 m off the wall: a slot)");
const te = box("g8", [7.55, 0, -228], [9.05, 4, -226.5]);
te.min = [7.825, 0, -228];
te.max = [8.825, ROOF, -226.5];
note(te, "to the roof (its 4 m top was a 0-hit perch) and 1.0 m wide, centred between the SE column (x 6.25) and the lane wall's face (x 10.4): 1.575 m both sides (it stood 1.3 m / 1.35 m off them: two slots)");
// 3. g5's far posts flush to the decks' inner faces
const pw = box("g5", [-4.1, 0, -151.6], [-2.9, 3.5, -150.4]);
pw.min = [-5, 0, -151.6];
pw.max = [-3.8, 3.5, -150.4];
note(pw, "flush to the west shelf's inner face (x -5): it stood 0.9 m off it, a slot");
const pe = box("g5", [2.9, 0, -151.6], [4.1, 3.5, -150.4]);
pe.min = [3.8, 0, -151.6];
pe.max = [5, 3.5, -150.4];
note(pe, "flush to the crane deck's inner face (x 5): it stood 0.9 m off it, a slot");
// 5 (the contract kept): g8's retry spawn 5.5 m in, as the lead's fix 4 did for g7: at (0, 0, -222) it sat exactly on the 4 m dwell band
// round the door (0, -218); the walk pilot and the chargers dance along the door wall (the charges end on it). 1fbca8be's g8 sat at the
// dwell limit (isolated 1.3 / 1.5 s against n47's 0.90 + 0.5); with the perches gone 2.4 / 1.4 s, the spawn at -225.5 1.1 / 2.1 s, at
// -227.5 1.6 / 0.5 s (mean 1.05: PASS; per seed the same two-mode spread, 0.3-10 s, every version). 9.5 m from the door's centre, between
// the SW / SE columns (3.75 m to each), 1.5 m short of THE PIT (1.5 m deep); the nodes 17-22 m ahead (spawn31)
const g8 = plan.rooms.find((x) => x.id === "g8");
if (!eq(g8.spawn.at, [0, 0, -222])) throw new Error("g8's spawn is not 1fbca8be's");
g8.spawn = { at: [0, 0, -227.5], yawDeg: 0 };
g8.spawn_why = "NIGHT52 fix K1: the retry point 7.5 m from the door wall, between the south columns (outside the door's 4 m dwell band, as fix 4 for g7): a death never restarts the fight at the door, where the charges end";
for (const id of ["g5", "g8"]) {
  const r = plan.rooms.find((x) => x.id === id);
  r.loop52_why = (r.loop52_why || "") + ` ${TAG}: ${id === "g8" ? "the cross hoist clear of the NW column; the lane walls, towers, hopper and lever frame to the roof (no 0-hit perch); the towers 1.0 m wide and centred (no slot); the retry spawn 5.5 m in (off the dwell band)" : "the far posts flush to the decks (no slot)"}.`;
}
// the guard: only g5 / g8 rows changed
for (let i = 0; i < plan.rooms.length; i++) if (!["g5", "g8"].includes(plan.rooms[i].id) && !eq(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "pads"]) {
  const keep = (l) => (l || []).filter((b) => b.zone !== "g5" && b.zone !== "g8");
  if (!eq(keep(before[k]), keep(plan[k]))) throw new Error(k + ": a row outside g5 / g8 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "pads"].includes(k) && !eq(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix-k1: g8 cross hoist [-8.5, 0, -236] -> [7, 5.5, -247]; g8 lane walls, towers, hopper, lever frame to 12 m; towers 1.0 m wide, centred; g5 far posts flush");

// NIGHT52 the room loop, fix 5 (the lead's; DECIDE52 D-23): the midpoint P-rank review's iceline findings.
//   g7 finding 2: "from the door both bunker climbs face backwards: run past, U-turn, climb into a pocket against the south wall, U-turn again
//     to take the side ride north". The review's swap (one ramp in place of each bunker) drops g7 to 19 pieces (2.47 per 100 m2, under the
//     contract's 2.5) and moves wave 3's six bunker nodes into the air; so the bunkers stay and their ramps turn to face the door: each rises
//     from beside the door (x +-2.5) outward onto its bunker (2 m over 4.5 m, the old slope), 4 m wide and flush with the door wall and the
//     bunker's north face (no slot). From the door: bear left or right up the ramp, turn north on the bunker, take the ride (one 90 deg turn).
//   g8 finding 1: "the inner lanes between THE PIT and the inner columns are ~2.1-2.3 m; dodging the charger sideways puts you in the void or a
//     column": the two east columns beside THE PIT (the charge stripe runs down the east inner lane) move 1 m out (x 5 -> 6; the east inner
//     lane 3.25 m, the outer 3.15 m). Measured and kept: moved, the SW column stopped the validator's completable walk at (-7.07, 0, -226.4)
//     and the W column at (-7.02, 0, -233.4) (the west lane is the cross hoist's walk line); the SE one would leave a 0.6 m slot beside the
//     east sand tower (the pocket fix 2 closed elsewhere). THE PIT stays (the charger trap; the lava swap rejected).
// Asserts fix 4's state; every other room bit for bit.
// Run: node Tools/night/n52-loop-fix5.cjs, then bash Tools/night/n52-loop-regen.sh iceline g7 g8
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const near = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const g7 = plan.rooms.find((r) => r.id === "g7");
const g8 = plan.rooms.find((r) => r.id === "g8");
if (!/Fix 4/.test(g7.loop52_why || "")) throw new Error("g7 is not fix 4's");
if (/Fix 5/.test(g7.loop52_why)) throw new Error("already run");

// ---- g7: the bunker ramps face the door ----
const ramps = plan.ramps.filter((r) => r.zone === "g7" && r.rise === 2 && Math.abs(r.from[0]) === 9.5 && r.from[1] === -192 && r.to[1] === -187.5);
if (ramps.length !== 2) throw new Error("g7 bunker ramps: " + ramps.length);
for (const r of ramps) {
  const s = Math.sign(r.from[0]);
  r.from = [2.5 * s, -186];
  r.to = [7 * s, -186];
  r.width = 4;
  r.why = r.why + " (NIGHT52 fix 5: turned to rise from beside the door outward onto the bunker, 4 m wide flush with the door wall: the first climb from the door faces the room, not the wall)";
  if (r.note !== undefined) r.note = r.why;
}
g7.loop52_why += " Fix 5 (n52-loop-fix5.cjs): the bunker ramps face the door.";

// ---- g8: the columns 1 m out ----
const cols = plan.props.filter((q) => q.room === "g8" && q.kind === "pillar");
if (cols.length !== 6 || cols.some((c) => Math.abs(c.at[0]) !== 5)) throw new Error("g8 columns not at x +-5");
for (const c of cols) {
  if (c.at[2] === -228 || c.at[0] < 0) continue;
  c.at = [6, c.at[1], c.at[2]];
  c.why = c.why + " (NIGHT52 fix 5: 1 m out, x 6: the east inner lane beside THE PIT 3.25 m, the outer 3.15 m)";
}
g8.loop52_why += " Fix 5 (n52-loop-fix5.cjs): the two east columns beside THE PIT 1 m out.";

for (let i = 0; i < plan.rooms.length; i++) if (!["g7", "g8"].includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
const others = (arr, k) => arr.filter((b) => !["g7", "g8"].includes(b.zone || b.room));
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "grapple", "tunnels"]) if (JSON.stringify(others(before[k] || [])) !== JSON.stringify(others(plan[k] || []))) throw new Error(k + " outside g7 / g8 changed");
for (const k of ["boxes", "pits", "lights", "pads"]) if (JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
for (const k of Object.keys(before)) if (!["rooms", "ramps", "props"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix5: g7 bunker ramps face the door; g8 east columns E / NE out (x 6)");

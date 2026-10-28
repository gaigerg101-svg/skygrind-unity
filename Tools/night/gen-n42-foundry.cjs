// NIGHT42 builder L1 (foundry42; NIGHT42.md L1, GG G1: "the waallride section on level one is awful and relaly difficult and not fun, eneeds to
// be more balanced"; DECIDE42 D-1: rebuild it forgiving, the chain stays the answer; DECIDE41 D-9: the no-ride band visible or gone).
// Level 1 as n42-foundry: n41-foundry's plan (Design/levels/n41/foundry.plan.json, written by Tools/night/gen-n41-foundry.cjs, which this file
// does not run or edit) copied whole, with THE GANTRY's (b2) two gates rebuilt on the copy, so `--off42` keeps n41-foundry bit for bit:
//   1. the id `n42-foundry` and its _why
//   2. per gate (b2-g1, b2-g2): the NO-RIDE BAND is GONE (W1's ride panel simply ends W1_TO m past the lip, open air beyond: nothing that looks
//      like a wall refuses a ride). Measured: a held ride on W1 alone still falls short (the chain is still the answer, `n38 gates --ride40`)
//   3. the lane narrower (the walls' planes at +-PLANE, n41: +-4): the run-up from the room's spawn line (x 0) meets W1 without a long steer, and
//      the hop across to W2 is shorter (less height lost under the 3.6 m ceiling)
//   4. W2 starts sooner (W2_FROM m past the lip, n41: 20): a long overlap with W1, so the hop lands on W2 from most of W1's length
//   5. a THIRD wall W3 (west, W1's plane, W3_FROM m to W2's end): a hop off W2 that comes early still has a wall to catch (W1 -> W2 -> W3);
//      without it a person who hops after under 0.75 s on each wall fell 0-2 m short (n42level sweep, lane 6 m: T 0.3-0.6 s 0/72 -> with W3 24/72)
//   6. the way back: n41 had steps up the pit's east side only; here a RETURN STAIR across the whole pit at the lip (3 steps of 1.2 m, the n41
//      steps' rise), so a fall anywhere runs straight back to the takeoff, plus the west side's mirror of the east steps
// Measured and NOT built (Harness.N42.Level.cs / `n38 gates --gate ...`): a CATCH LEDGE in the pit under the chain (2.4 m under the floor, a short
// retry on W2) is a cheat: from it 52 tries crossed with no ride (the double jump's apex is 2.87 m once the 3.6 m ceiling does not cap it), and a
// ride from standing on it did not cross. A landing ramp or a shallower pit would be the same cheat (the best dash jump lands 37.15 m out at -4 m).
// The checker: `n38 gates Design/levels/n42/foundry.plan.json --ride40` (both gates GREEN, the chain the answer) and `n42level sweep` (the share
// of a person-like sweep of ride tries that cross, before / after).
// Probes: N42F_<KNOB> env vars override the numbers below; a probe must write to N42F_OUT (never the shipped plan).
// Run from the repo (or worktree) root: bash Tools/night/n42-foundry-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n41/foundry.plan.json");
const OUT = path.join(ROOT, "Design/levels/n42/foundry.plan.json");
const env = (k, d) => (process.env["N42F_" + k] !== undefined ? Number(process.env["N42F_" + k]) : d);
const KNOBS = {
  /// the walls' planes at x = +-PLANE (n41: 4, a lane 8 m wide)
  PLANE: env("PLANE", 3),
  /// m past the lip where W1's ride panel ends (n41: 22, then the 4 m band): the band gone
  W1_TO: env("W1_TO", 22),
  /// m past the lip where W2 starts (n41: 20)
  W2_FROM: env("W2_FROM", 8),
  /// m past the lip where W2 ends (n41: 36.5, 0.5 m short of the landing's near edge at 37 m)
  W2_TO: env("W2_TO", 36.5),
  /// m past the lip where W3 (west, W1's plane) starts, to W2's end (0: no W3)
  W3_FROM: env("W3_FROM", 26),
  /// 1: the return stair across the pit at the lip (0: none, n41's east steps only)
  RETURN: env("RETURN", 1),
};
const OUT_FILE = process.env.N42F_OUT ? path.resolve(ROOT, process.env.N42F_OUT) : OUT;
const probe = Object.keys(process.env).some((k) => k.startsWith("N42F_") && k !== "N42F_OUT");
if (probe && OUT_FILE === OUT) throw new Error("a probe (N42F_* knobs) must write to N42F_OUT, not the shipped plan");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n41-foundry") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n42-foundry";
plan._why = "NIGHT42 builder L1 (design.n42.foundry42 swaps n41-foundry for this file while the row is on; GG G1, DECIDE42 D-1): Tools/night/gen-n42-foundry.cjs "
  + `copies n41-foundry's plan whole and changes only THE GANTRY (b2): per gate the no-ride band gone (W1 ends in open air), the lane ${2 * KNOBS.PLANE} m wide, `
  + `W2 from ${KNOBS.W2_FROM} m, ${KNOBS.W3_FROM > 0 ? `a third wall W3 from ${KNOBS.W3_FROM} m, ` : ""}a return stair across the pit at the lip and the west steps. n41-foundry's _why: ` + plan._why;

const b2 = plan.rooms.find((r) => r.id === "b2");
if (!b2) throw new Error("room b2 (THE GANTRY) missing");
const RZ1 = b2.rect[3]; // the room's south edge (z max): ride36.lz is metres from it
const [RX0, , RX1] = b2.rect;
const changed = [];
for (const gid of ["b2-g1", "b2-g2"]) {
  const land = plan.boxes.find((b) => b.zone === "b2" && b.gate && b.gate.id === gid);
  if (!land) throw new Error(`gate ${gid} missing`);
  const lipZ = land.gate.from[2] - 0.5; // as gen-n41-foundry.cjs: the plan's `from` is 0.5 m short of the lip
  const landZ = land.max[2]; // the landing's near edge
  const gap = +(lipZ - landZ).toFixed(2);
  const pitY = land.min[1];
  const at = (m) => +(lipZ - m).toFixed(2); // world z m past the lip
  const inPit = (b) => b.max[2] > landZ && b.min[2] < lipZ;
  // the band (noride, on W1's line): gone
  const bi = plan.boxes.findIndex((b) => b.zone === "b2" && b.noride && inPit(b) && b.max[0] < 0 && b.min[0] > RX0 + 1);
  if (bi < 0) throw new Error(`gate ${gid}: the band missing`);
  const band = plan.boxes.splice(bi, 1)[0];
  const w1 = plan.boxes.find((b) => b.zone === "b2" && b.ride && b.ride36 && b.ride36.face === "E" && inPit(b));
  const w2 = plan.boxes.find((b) => b.zone === "b2" && b.ride && b.ride36 && b.ride36.face === "W" && inPit(b));
  if (!w1 || !w2) throw new Error(`gate ${gid}: W1 / W2 missing`);
  const thick = +(w1.max[0] - w1.min[0]).toFixed(2); // 0.6
  const P = KNOBS.PLANE;
  const setWall = (w, x0, x1, planeX, fromM, toM, why) => {
    w.min = [x0, w.min[1], at(toM)];
    w.max = [x1, w.max[1], at(fromM)];
    w.ride36 = Object.assign({}, w.ride36, { planeX, lz: [+(RZ1 - at(fromM)).toFixed(2), +(RZ1 - at(toM)).toFixed(2)] });
    w.why = why;
    w.note = why;
  };
  const w1From = +(lipZ - w1.max[2]).toFixed(2); // -6: it starts on the takeoff floor
  setWall(w1, -P - thick, -P, -P, w1From, KNOBS.W1_TO, `${gid} ride wall 1 of 2 (west, its east face at x ${-P}; NIGHT42 L1, foundry42): from ${-w1From} m before the lip to `
    + `${KNOBS.W1_TO} m past it, then open air (n41's no-ride band gone, DECIDE41 D-9: a rider sees where the wall ends); the lane ${2 * P} m wide (n41: 8)`);
  setWall(w2, P, P + thick, P, KNOBS.W2_FROM, KNOBS.W2_TO, `${gid} ride wall 2 of 2 (east, its west face at x ${P}; NIGHT42 L1, foundry42): ${KNOBS.W2_FROM} m past the lip `
    + `(n41: 20) to ${KNOBS.W2_TO} m (the landing's near edge at ${gap} m): ${+(KNOBS.W1_TO - KNOBS.W2_FROM).toFixed(2)} m of overlap with W1, so the hop across lands on it from most of W1`);
  if (KNOBS.W3_FROM > 0) {
    // W3: a third wall, west again (W1's plane), from W3_FROM to W2's end: a rider who leaves W2 early catches it (the chain's third link)
    const w3 = JSON.parse(JSON.stringify(w1));
    plan.boxes.splice(plan.boxes.indexOf(w2) + 1, 0, w3);
    setWall(w3, -P - thick, -P, -P, KNOBS.W3_FROM, KNOBS.W2_TO, `${gid} ride wall 3 of 3 (west, its east face at x ${-P}; NIGHT42 L1, foundry42): ${KNOBS.W3_FROM} m past the lip to `
      + `${KNOBS.W2_TO} m: the chain's third link (W1 -> W2 -> W3, faces alternating), so a hop off W2 that comes early still has a wall to catch`);
  }
  // the gate's line stays 0.8 m off W1's face (n41: x -3.2 by the plane at -4)
  const gx = +(land.gate.from[0] + (4 - P)).toFixed(2);
  land.gate = Object.assign({}, land.gate, { from: [gx, land.gate.from[1], land.gate.from[2]], to: [gx, land.gate.to[1], land.gate.to[2]] });
  const parts = [`band out (z ${band.min[2]}..${band.max[2]})`, `planes +-${P}`, `W1 to ${KNOBS.W1_TO} m`, `W2 ${KNOBS.W2_FROM}..${KNOBS.W2_TO} m`];
  if (KNOBS.W3_FROM > 0) parts.push(`W3 ${KNOBS.W3_FROM}..${KNOBS.W2_TO} m`);
  // the way back: n41's east steps (x 7.8..10, 1.5 m each, tops -2.8 / -1.6 / -0.4 from the pit out)
  const east = plan.boxes.filter((b) => b.zone === "b2" && b.kind === "deck" && b.job === "climb" && b.min[0] > 7 && b.max[2] <= lipZ + 0.01 && b.min[2] >= lipZ - 5);
  if (east.length !== 3) throw new Error(`gate ${gid}: expected 3 east steps, found ${east.length}`);
  for (const s of east) {
    const ww = `${s.why} (NIGHT42 L1: the west side's mirror, so the way back is on both sides)`;
    plan.boxes.push({ zone: "b2", kind: "deck", min: [-s.max[0], s.min[1], s.min[2]], max: [-s.min[0], s.max[1], s.max[2]], job: "climb", why: ww, note: ww });
  }
  parts.push("west steps");
  if (KNOBS.RETURN) {
    // the return stair: the same three steps across the pit between the side steps (x -7.8..7.8), W1 standing through it (a wall to the roof)
    for (const s of east) {
      const rw = `${gid}'s RETURN STAIR (NIGHT42 L1, foundry42; GG G1): n41's east step (top ${s.max[1]} m) carried across the pit at the lip, so a fall anywhere `
        + "runs straight back up to the takeoff (n41: round to the east wall's steps); a short way back, never a long walk";
      plan.boxes.push({ zone: "b2", kind: "deck", min: [-s.min[0], s.min[1], s.min[2]], max: [s.min[0], s.max[1], s.max[2]], job: "climb", why: rw, note: rw });
    }
    parts.push("return stair");
  }
  changed.push(`${gid}: ${parts.join(", ")}`);
}
b2.foundry42_why = "L1 (foundry42): " + changed.join("; ");
b2.flight38Why = (b2.flight38Why || "") + " NIGHT42 L1: forgiving (the band gone, a narrower lane, W2 sooner, a return stair at each lip).";

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT_FILE), "|", changed.join(" | "));

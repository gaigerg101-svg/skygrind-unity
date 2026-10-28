// NIGHT47 builder W: the one-line wick47 hooks in THE WICK's own files (Core/Enemies/Wick37.cs, Wick37.N43.cs, Game/SurvivalSim.Wick37.cs),
// each anchored on an exact line; every hook is skipped with the row off (w47 false / wick47 off). Idempotent: a hook already in is reported.
// Usage (worktree root): node Tools/night/w47-hooks.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const dry = process.argv.includes("--dry");
const EDITS = [
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            StartFight43(); // NIGHT43 B1 (wick43; Wick37.N43.cs): nothing with the row off\n",
    "            StartFight47(); // NIGHT47 W (wick47; Wick37.N47.cs): the phases' kit and gate (nothing with the row off)\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (brain.Open) { fulls++; return amount * brain.Scale(true); }\n",
    "            if (w47 && Filter47(amount, weapon, from, out double share47)) return share47; // NIGHT47 W (wick47): the bare core, the floor crack, the chip\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (stalled) { stallPunches++; if (launch) stallLaunchPunches++; lastStallPunchDist = Vec3.DistanceTo(eye, Centre); }\n",
    "            if (w47 && CorePunch47(eye)) { punchUsed = true; return true; } // NIGHT47 W (wick47): the open core's punch\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (brain == null || !body.alive) return;\n            int want = y >= stack.p.capY - 0.1",
    null, "            if (w47) { ReachedFloor47(y); return; } // NIGHT47 W (wick47): the legs are phases\n", true],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            string id = AttackId;\n            if (id == Flare)\n            {\n                flareAngle = PlayerAngle(pw);",
    null, "            if (w47 && StartTell47(id, pw)) return; // NIGHT47 W (wick47): the new attacks' tells\n", true, "            string id = AttackId;\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            string id = AttackId;\n            if (id == Ember) { FireEmbers(pw); Volley43Arm(); }",
    null, "            if (w47 && StartActive47(id, pw)) return; // NIGHT47 W (wick47): the new attacks\n", true, "            string id = AttackId;\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (n43) Step43(pw); // NIGHT43 B1 (wick43; Wick37.N43.cs): the second volley, the leg beat, the gift crack, bossOpen43\n",
    "            if (w47) Step47(pw, dt); // NIGHT47 W (wick47; Wick37.N47.cs): the phase gate, the ring, the charge's hit, the open clock\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "                if (brain.state != Boss37State.Active) flareLive = false;\n",
    "                if (w47 && brain.state == Boss37State.Recover && before == Boss37State.Active) Recover47(); // NIGHT47 W (wick47): the charge cracks open\n"],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (brain.leg < 3)\n            {\n                double want = Math.Min(stack.p.capY - 2",
    null, "            if (Move47(pw, dt)) return; // NIGHT47 W (wick47): P2's height, the charge, the geyser's sink\n", true],
  ["Assets/Scripts/Core/Enemies/Wick37.cs",
    "            if (r < stack.p.radius - p.flareBand || Math.Abs(Wrap(a - flareAngle)) > FlareHalfDeg43 * Math.PI / 180) return;",
    "REPLACE",
    "            if ((!(w47 && brain.leg != 2) && r < stack.p.radius - p.flareBand) || Math.Abs(Wrap(a - flareAngle)) > FlareHalfDeg43 * Math.PI / 180) return;"],
  ["Assets/Scripts/Core/Enemies/Wick37.N43.cs",
    "else if (AttackId == Dive && ++dives43Since >= p43.capFlareEvery && p43.flareLeg3) SetFlareLeg3(true);",
    "REPLACE",
    "else if ((AttackId == Dive || (w47 && AttackId == Charge47)) && ++dives43Since >= p43.capFlareEvery && p43.flareLeg3) SetFlareLeg3(true); // NIGHT47 W (wick47): the CHARGE counts as the DIVE"],
  ["Assets/Scripts/Game/SurvivalSim.Wick37.cs",
    "            Wick43Arm(); // NIGHT43 B1 (wick43): the fight takes the row's bit and numbers (off: wick37.n43 false, NIGHT42's fight)\n",
    "            Wick47Arm(); // NIGHT47 W (wick47; SurvivalSim.N47.Wick.cs): the fight takes the row's bit, numbers and vents (off: wick37.w47 false)\n"],
  ["Assets/Scripts/Game/Wick37View.cs",
    "Render43(sim, w, o, false); return; }",
    "REPLACE",
    "Render43(sim, w, o, false); Render47(sim, w, o, false); return; }"],
  ["Assets/Scripts/Game/Wick37View.cs",
    "            Render43(sim, w, o, true); // NIGHT43 B1 (wick43; Wick37View.N43.cs): the reveal's frame, the lights, the leg beat, leg 3's cap flare\n",
    "            Render47(sim, w, o, true); // NIGHT47 W (Wick37View.N47.cs): the panels folded in (P1), the glass (P3), wickLook47's indicators\n"],
];

const files = {};
const get = (f) => (files[f] ??= fs.readFileSync(path.join(ROOT, f), "utf8"));
let applied = 0, already = 0;
for (const [f, anchor, after, before, isBefore, prefix] of EDITS) {
  let t = get(f);
  const crlf = t.includes("\r\n");
  const norm = (s) => (crlf ? s.replace(/\r?\n/g, "\r\n") : s);
  const a = norm(anchor);
  const n = t.split(a).length - 1;
  if (after === "REPLACE") {
    const b = norm(before);
    if (t.includes(b)) { already++; continue; }
    if (n !== 1) throw new Error(`MISS (${n}) ${f}: ${anchor.slice(0, 80)}`);
    t = t.replace(a, b);
  } else if (isBefore) {
    const ins = norm(before);
    if (t.includes(ins)) { already++; continue; }
    if (n !== 1) throw new Error(`MISS (${n}) ${f}: ${anchor.slice(0, 80)}`);
    const p = prefix ? norm(prefix) : "";
    t = t.replace(a, p + ins + a.slice(p.length));
  } else {
    const ins = norm(after);
    if (t.includes(ins)) { already++; continue; }
    if (n !== 1) throw new Error(`MISS (${n}) ${f}: ${anchor.slice(0, 80)}`);
    t = t.replace(a, a + ins);
  }
  files[f] = t;
  applied++;
}
if (!dry) for (const [f, t] of Object.entries(files)) fs.writeFileSync(path.join(ROOT, f), t);
console.log(`${applied} hook(s) ${dry ? "planned (dry run)" : "applied"}, ${already} already in`);

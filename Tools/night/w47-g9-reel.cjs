// NIGHT47 builder W (G9 round 4): the G9 frames in reel a147 (Game/FeelReel.N47.Wick.cs, W's own file; anchored; idempotent).
// Usage (worktree root): node Tools/night/w47-g9-reel.cjs
const fs = require("fs");
const path = require("path");
const p = path.join(__dirname, "../../Assets/Scripts/Game/FeelReel.N47.Wick.cs");
let t = fs.readFileSync(p, "utf8");
const edits = [
  ['            if (force) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true).Set("climb47", true).Set("wickTop47", true);',
   '            if (force) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true).Set("climb47", true).Set("wickTop47", true).Set("wickRideParry47", true).Set("wickIntro47", true); // G9: the two G9 sim rows too (wickFinale47 is a view row: the build copy\'s config)'],
  ['            while (sim.SimTime - t0 < 1.5) { God(g); yield return null; }\n',
   '            while (sim.SimTime - t0 < 0.6) { God(g); yield return null; }\n            yield return Shot29(dir, "W47-g9-intro", st); // G9 idea 2: the name card "THE WICK. THE FURNACE WAKES."\n            string introNow = sim.wick47Line != null ? sim.wick47Line.Now : null;\n            while (sim.SimTime - t0 < 1.5) { God(g); yield return null; }\n'],
  ['            // P3: the cap\n',
   '            // G9 idea 1: a ride-parried ember (forced: one returning with the ride flag) cracks two plates\n            int rp0 = w.rideCracksMade47;\n            { Vec3 ec = w.Centre + new Vec3(0, 0, 3.5); w.embers.Add(new Wick37Ember { pos = ec, prev = ec, vel = new Vec3(0, 0, -33), life = 600, alive = true, back = true, ride47 = w.rideParry47 }); }\n            for (int k = 0; k < 120 && w.rideCracksMade47 == rp0; k++) { God(g); yield return null; }\n            yield return Wick47Look(g, c + new Vec3(0, 0.05, 4), w.Centre, dir, "W47-g9-rideparry", st);\n            int cracked = w.CrackedCount;\n            // P3: the cap\n'],
  ['            yield return Shot29(dir, "W47-p3-land", st);\n',
   '            yield return Shot29(dir, "W47-p3-land", st);\n            // G9 idea 3: the finisher (forced: the last core punch from 3 m): the slow motion, the glass cracking, the slag flaring (wickFinale47 view row)\n            int fin0 = Wick37View.Current != null ? Wick37View.Current.Finales47 : -1;\n            if (w.w47 && w.p47 != null && w.Alive)\n            {\n                w.corePunches47 = Math.Max(w.corePunches47, w.p47.punches - 1);\n                w.body.hp = Math.Min(w.body.hp, w.body.maxHp * (w.p47.finishAt + 0.05));\n                Vec3 nf = w.Centre + new Vec3(0, 0, 3.0);\n                g.Respawn(new Vec3(nf.x, c.y + s.p.capY + 0.05, nf.z), 0);\n                w.coreOpen47 = w.coreOpenSteps47;\n                yield return Frames(2);\n                Vec3 e2 = sim.Eye, d2 = w.Centre - e2;\n                w.Punch(e2, d2 * (1 / Math.Max(1e-6, d2.Length)), false);\n                double tfin = sim.SimTime;\n                while (sim.SimTime - tfin < 0.25) yield return null;\n                yield return Shot29(dir, "W47-g9-finale", st);\n                while (sim.SimTime - tfin < 0.9) yield return null;\n                yield return Shot29(dir, "W47-g9-finale-2", st);\n            }\n            int finales = Wick37View.Current != null ? Wick37View.Current.Finales47 - fin0 : -1;\n'],
  ['            res[0] = $"{(ok ? "GREEN" : "RED")}: rows',
   '            string g9 = $" | G9: intro line \\"{introNow}\\"; ride-parry cracks {w.rideCracksMade47 - rp0} (plates cracked {cracked}); finales drawn {finales} (wickFinale47 view row {NightRows.ViewOn(g.cfg, "wickFinale47")})";\n            res[0] = $"{(ok ? "GREEN" : "RED")}{g9}: rows'],
];
for (const [a, b] of edits) {
  if (t.includes(b)) continue;
  if (!t.includes(a)) throw new Error("MISS " + a.slice(0, 80));
  t = t.replace(a, b);
}
t = t.replace("//   W47-p3-land      the landing: on the glass, never in the shaft (the feet over the cap's top).",
  "//   W47-p3-land      the landing: on the glass, never in the shaft (the feet over the cap's top).\n//   G9 (round 4): W47-g9-intro (0.6 s in: the name card \"THE WICK. THE FURNACE WAKES.\"), W47-g9-rideparry (a ride-parried ember's two cracked\n//   plates, from the floor), W47-g9-finale / -finale-2 (0.25 / 0.9 s after the forced finishing punch: the slowed world, the glass's yellow-white\n//   cracks, the slag flaring; wickFinale47 must be on in the build copy's config).");
fs.writeFileSync(p, t);
console.log("patched FeelReel.N47.Wick.cs");

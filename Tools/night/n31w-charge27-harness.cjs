// NIGHT31 builder W round 3 (2): `weapons --charge27` reads the ACTIVE charge object's numbers (design.n31.charge while that row is on, else
// design.n27.chargeShots27) and the loadout's charge (n31's replaces n27's), and its RED side (--off) runs with the NIGHT31 rows off.
// Run once from the repo (or worktree) root: node Tools/night/n31w-charge27-harness.cjs
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "SimHarness", "Harness.N27.Weapons.cs");
const raw = fs.readFileSync(file, "utf8");
const crlf = raw.includes("\r\n");
let s = raw.replace(/\r\n/g, "\n");
if (s.includes("ActiveChargePrefix31")) throw new Error("already patched");
function rep(a, b) {
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error("expected once: " + a + " (" + n + ")");
  s = s.replace(a, b);
}
rep("        sim.n27Override = new N27Blocks { pulse27 = pulse, chargeShots27 = charge };",
    "        sim.n27Override = new N27Blocks { pulse27 = pulse, chargeShots27 = charge };\n        if (!charge) sim.n31Override = new N31Blocks { ltAlways = N31Blocks.ForceOff ? false : N31Blocks.From(cfg).ltAlways }; // NIGHT31 W: a run without the charge has no n31 charge either (the other n31 rows as the config)");
rep("        return (fires, hits, sim.charge27);", "        return (fires, hits, sim.loadout.charge27 ?? sim.charge27); // NIGHT31 W: the active charge (n31's replaces n27's on the loadout)");
rep("        foreach (string gun in new[] { \"revolver\", \"shotgun\" })\n        {\n            int gi = gun == \"revolver\" ? 0 : 1;\n            double holdS = cfg.Num(\"design.n27.chargeShots27.holdSeconds\"), fullS = cfg.Num(\"design.n27.chargeShots27.\" + gun + \".fullSeconds\"), cdS = cfg.Num(\"design.n27.chargeShots27.\" + gun + \".cooldownSeconds\");",
    "        string P = ActiveChargePrefix31(cfg, map); // NIGHT31 W: the active charge's numbers\n        Console.WriteLine($\"charge27: the active charge's numbers: {P}\");\n        foreach (string gun in new[] { \"revolver\", \"shotgun\" })\n        {\n            int gi = gun == \"revolver\" ? 0 : 1;\n            double holdS = cfg.Num(P + \"holdSeconds\"), fullS = cfg.Num(P + gun + \".fullSeconds\"), cdS = cfg.Num(P + gun + \".cooldownSeconds\");");
rep("double want = gun == \"revolver\" ? cfg.Num(\"design.n27.chargeShots27.revolver.damageMult\") : cfg.Num(\"design.n27.chargeShots27.shotgun.damageMult\");",
    "double want = gun == \"revolver\" ? cfg.Num(P + \"revolver.damageMult\") : cfg.Num(P + \"shotgun.damageMult\");");
rep("int pierce = cfg.Int(\"design.n27.chargeShots27.revolver.pierce\");", "int pierce = cfg.Int(P + \"revolver.pierce\");");
// the helper, before Charge27Command
rep("    static void Charge27Command(string[] args, ref int code)\n",
    "    /// <summary>NIGHT31 W: the config block of the charge a charge-on run uses: design.n31.charge. while that row is on (its object replaces n27's on the\n    /// loadout), else design.n27.chargeShots27.</summary>\n    static string ActiveChargePrefix31(GameConfig cfg, SurvivalMap map)\n    {\n        SurvivalSim probe = Sim27(cfg, map, false, true, 2702);\n        return probe.charge31 != null && ReferenceEquals(probe.loadout.charge27, probe.charge31) ? \"design.n31.charge.\" : \"design.n27.chargeShots27.\";\n    }\n\n    static void Charge27Command(string[] args, ref int code)\n");
fs.writeFileSync(file, crlf ? s.replace(/\n/g, "\r\n") : s);
console.log("Harness.N27.Weapons.cs patched");

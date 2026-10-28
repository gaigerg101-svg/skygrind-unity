// NIGHT34 builder M (design.n34.reach34): the patch for the two files builder M does not own.
//   Core/Combat.cs (builder B's tonight): the PULSE GAUNTLET's target lists come from Loadout.GauntletTargets34() (Core/Guns/Pulse25.cs) instead of
//     enemies.PunchTargets(): the click swing's sweep, its assist lunge and lunge re-sweep (SwingGauntlet), the gem's parry peek (Step), and the
//     F punch's targets (StepPunch: "F is the Pulse Gauntlet", POWER P3). Torch.cs keeps PunchTargets (not the gauntlet). Off (margin 0):
//     GauntletTargets34 returns PunchTargets untouched, so NIGHT33's path exactly.
//   Game/SurvivalSim.Boss30.cs (the lead's): the KILN WYRM head-punch stagger's reach adds the run's Reach34Margin (Game/SurvivalSim.Reach34.cs; 0 off),
//     so the stagger lands on every swing the margin lands.
// Each anchor must occur exactly once (asserted before any write); a file already patched is refused (its marker found).
// Run: node Tools/night/n34m-reach-patch.cjs [repoRoot] [--only combat|boss30] [--dry]
//   repoRoot defaults to this script's repo; --dry prints what would change and writes nothing.
const fs = require("fs");
const path = require("path");
const args = process.argv.slice(2);
const dry = args.includes("--dry");
const oi = args.indexOf("--only");
const only = oi >= 0 ? args[oi + 1] : null;
const root = args.find((a, i) => !a.startsWith("--") && (oi < 0 || i !== oi + 1)) || path.join(__dirname, "..", "..");

const patches = {
  combat: {
    file: "Assets/Scripts/Core/Combat.cs",
    marker: "GauntletTargets34()",
    edits: [
      [
        "foreach (TargetPick p in CombatMath.SweepTargets(o, d, enemies.PunchTargets(), gRange + gParryPeek, gCone * Math.PI / 180, found))",
        "foreach (TargetPick p in CombatMath.SweepTargets(o, d, GauntletTargets34(), gRange + gParryPeek, gCone * Math.PI / 180, found)) // NIGHT34 reach34: the boss margin (Pulse25.cs)",
      ],
      [
        "foreach (TargetPick p in CombatMath.SweepTargets(origin, dir, enemies.PunchTargets(), gRange, gCone * Math.PI / 180, found))",
        "foreach (TargetPick p in CombatMath.SweepTargets(origin, dir, GauntletTargets34(), gRange, gCone * Math.PI / 180, found)) // NIGHT34 reach34: the boss margin (Pulse25.cs)",
      ],
      [
        "TargetPick? near = assist.LungeTarget(origin, dir, enemies.PunchTargets(), gRange, gCone * Math.PI / 180);",
        "TargetPick? near = assist.LungeTarget(origin, dir, GauntletTargets34(), gRange, gCone * Math.PI / 180); // NIGHT34 reach34",
      ],
      [
        "List<TargetPick> reach = CombatMath.SweepTargets(origin, dir, enemies.PunchTargets(), gRange + lunge, gCone * Math.PI / 180, found);",
        "List<TargetPick> reach = CombatMath.SweepTargets(origin, dir, GauntletTargets34(), gRange + lunge, gCone * Math.PI / 180, found); // NIGHT34 reach34",
      ],
      [
        "List<BodyTarget> targets = enemies.PunchTargets();",
        "List<BodyTarget> targets = GauntletTargets34(); // NIGHT34 reach34: the F punch is the Pulse Gauntlet's (POWER P3): the boss margin (Pulse25.cs)",
      ],
    ],
  },
  boss30: {
    file: "Assets/Scripts/Game/SurvivalSim.Boss30.cs",
    marker: "Reach34Margin",
    edits: [
      [
        "double reach = cfg.Num(\"weapons.gauntlet.range\") + cfg.Num(\"design.n30.wyrm30.headReachMargin\");",
        "double reach = cfg.Num(\"weapons.gauntlet.range\") + cfg.Num(\"design.n30.wyrm30.headReachMargin\");\n" +
          "            if (Reach34Margin > 0) reach += Reach34Margin; // NIGHT34 reach34: the gauntlet's boss margin (SurvivalSim.Reach34.cs; 0 off: NIGHT33)",
      ],
    ],
  },
};

let changed = 0;
for (const [name, p] of Object.entries(patches)) {
  if (only && only !== name) continue;
  const file = path.join(root, p.file);
  const raw = fs.readFileSync(file, "utf8");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  if (raw.includes(p.marker)) throw new Error(`${p.file}: already patched (${p.marker} found)`);
  let out = raw;
  for (const [from] of p.edits) {
    const n = raw.split(from).length - 1;
    if (n !== 1) throw new Error(`${p.file}: anchor found ${n} times (want 1): ${from}`);
  }
  for (const [from, to] of p.edits) out = out.replace(from, () => to.split("\n").join(eol));
  console.log(`${p.file}: ${p.edits.length} edit(s)${dry ? " (dry run, not written)" : ""}`);
  if (!dry) fs.writeFileSync(file, out);
  changed++;
}
if (changed === 0) throw new Error("nothing patched (check --only)");

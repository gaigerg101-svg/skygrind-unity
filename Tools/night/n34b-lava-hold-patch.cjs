// NIGHT34 builder B -> the lead (SurvivalSim.Systems30.cs is lead-only): the split bend's one misclassified call. N30SystemsStep runs on every step
// (the player's side: the lava's touch on the player, the glide rail), but Lava30.Step also runs the ENEMIES' lava burn clocks (enemyBurnSeconds,
// whole steps): on a held world step they ran at the player's pace, 4x faster inside a split bend. The patch holds the enemies' part on a held step:
// - Core/Lava30.cs: a public bool holdEnemies34 (false unless set) and, after the player's touch, `if (holdEnemies34) return;`.
// - Game/SurvivalSim.Systems30.cs Lava30Step: lava30.holdEnemies34 = n34HoldWorld before the Step (false with bendSplit34 off: NIGHT33's game).
// Run from the repo root: node Tools/night/n34b-lava-hold-patch.cjs   (refuses to run twice). Then fingerprint + --off34 EQUAL, CoreHarness 0 mismatches.
const fs = require("fs");
const path = require("path");
function patch(rel, edits) {
  const file = path.join(process.cwd(), rel);
  let s = fs.readFileSync(file, "utf8");
  const eol = s.includes("\r\n") ? "\r\n" : "\n";
  for (const [from, to] of edits) {
    const a = from.split("\n").join(eol), b = to.split("\n").join(eol);
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(`${rel}: anchor found ${n} times: ${from.slice(0, 60)}`);
    s = s.replace(a, b);
  }
  fs.writeFileSync(file, s);
  console.log("patched " + rel);
}
const lava = path.join(process.cwd(), "Assets/Scripts/Core/Lava30.cs");
if (fs.readFileSync(lava, "utf8").includes("holdEnemies34")) throw new Error("already patched");
patch("Assets/Scripts/Core/Lava30.cs", [
  [
    "        public void Step(Vec3 feet, double maxHp, bool playing, Func<double, bool> hurt, Action<double> knock, List<Enemy> enemies, EventBus events)\n",
    "        /// <summary>NIGHT34 B (design.n34.bend34.split): true on a step whose world block is held (SurvivalSim.Systems30 sets it from n34HoldWorld): the\n" +
      "        /// player's touch runs, the enemies' burns wait for the world's step. False with the row off.</summary>\n" +
      "        public bool holdEnemies34;\n" +
      "\n" +
      "        public void Step(Vec3 feet, double maxHp, bool playing, Func<double, bool> hurt, Action<double> knock, List<Enemy> enemies, EventBus events)\n",
  ],
  [
    "            // enemies: a touch starts the burn; the burn runs out and kills\n",
    "            if (holdEnemies34) return; // NIGHT34 B: a held world step (the split bend): the enemies' part waits for the world\n" +
      "            // enemies: a touch starts the burn; the burn runs out and kills\n",
  ],
]);
patch("Assets/Scripts/Game/SurvivalSim.Systems30.cs", [
  [
    "            lava30.Step(mover.position, health.max, flow == Flow.Playing, lavaHurt30, lavaKnock30, enemies.all, events);\n",
    "            lava30.holdEnemies34 = n34HoldWorld; // NIGHT34 B: the split bend's held step holds the enemies' burns (Lava30.holdEnemies34)\n" +
      "            lava30.Step(mover.position, health.max, flow == Flow.Playing, lavaHurt30, lavaKnock30, enemies.all, events);\n",
  ],
]);

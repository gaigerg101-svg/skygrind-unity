// NIGHT30 (the lead): pre-placed dispatch hooks so the builders never touch the lead's files.
//   - Tools/SimHarness/Program.cs: N30Systems / N30Boss / N30Levels / N30View partial methods (Harness.N30.<Owner>.cs implement them)
//   - Assets/Scripts/Game/FeelReel.N27.cs: the NIGHT30 reel scenario names and four partial bodies (FeelReel.N30.<Owner>.cs implement them)
// Run once: node Tools/night/n30-hooks-patch.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a, b] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 80));
    s = s.replace(a, b);
  }
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}

patch("Tools/SimHarness/Program.cs", [
  ["        N29Walk(cmd, args, ref code); // NIGHT29 R: walk29 <foundry|iceline|kiln> [--seeds n] (Harness.N29.Walk.cs)\n",
   "        N29Walk(cmd, args, ref code); // NIGHT29 R: walk29 <foundry|iceline|kiln> [--seeds n] (Harness.N29.Walk.cs)\n" +
   "        N30Systems(cmd, args, ref code); // NIGHT30 S: rules30, lava30, glide30, arcline30, lesson30 ... (Harness.N30.Systems.cs)\n" +
   "        N30Boss(cmd, args, ref code); // NIGHT30 B: boss30 [lantern|glacier|wyrm|loom], seam30 (Harness.N30.Boss.cs)\n" +
   "        N30Levels(cmd, args, ref code); // NIGHT30 L: walk30 / level checks for the n30 plans (Harness.N30.Levels.cs)\n" +
   "        N30View(cmd, args, ref code); // NIGHT30 V: quantise30 and other headless view checks (Harness.N30.View.cs)\n"],
  ["    static partial void Bot(string cmd, string[] args, ref int code);\n",
   "    static partial void Bot(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT30: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N30Systems(string cmd, string[] args, ref int code);\n" +
   "    static partial void N30Boss(string cmd, string[] args, ref int code);\n" +
   "    static partial void N30Levels(string cmd, string[] args, ref int code);\n" +
   "    static partial void N30View(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs)\n",
   "        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30: below\n\n" +
   "        // NIGHT30: the reel scenarios by owner; each owner's FeelReel.N30.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN30S = { \"rules30\", \"lava30\", \"glide30\", \"lesson30\", \"hudfx30\", \"arcline30\" };\n" +
   "        public static readonly string[] PhaseN30B = { \"lantern30\", \"glacier30\", \"wyrm30\", \"loom30\" };\n" +
   "        public static readonly string[] PhaseN30V = { \"weaponhud30\", \"menus30\", \"layout30\", \"look30\" };\n" +
   "        public static readonly string[] PhaseN30L = { \"walk30\", \"rooms30\", \"span30\" };\n" +
   "        static bool IsPhaseN30(string s) => Array.IndexOf(PhaseN30S, s) >= 0 || Array.IndexOf(PhaseN30B, s) >= 0 || Array.IndexOf(PhaseN30V, s) >= 0 || Array.IndexOf(PhaseN30L, s) >= 0;\n" +
   "        static partial void N30SystemsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N30BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N30ViewBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N30LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                case \"lantern29\": case \"glacier29\": N29BossesBody(s, g, dir, res, offs, pre, ref body); break;\n",
   "                case \"lantern29\": case \"glacier29\": N29BossesBody(s, g, dir, res, offs, pre, ref body); break;\n" +
   "                default: // NIGHT30: by owner\n" +
   "                    if (Array.IndexOf(PhaseN30S, s) >= 0) N30SystemsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN30B, s) >= 0) N30BossesBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN30V, s) >= 0) N30ViewBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN30L, s) >= 0) N30LevelsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    break;\n"],
]);

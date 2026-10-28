// NIGHT32 shared commit 3 (the lead): builder C (THE CROWN, level 5's boss) gets its own hooks so no two builders implement one partial:
// SurvivalSim N32CrownStartRun / N32CrownStep, the harness partial N32Crown, the reel group PhaseN32C (boss5 moves there from PhaseN32B).
// Run once: node Tools/night/n32-shared-patch2.cjs
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

patch("Assets/Scripts/Game/SurvivalSim.N32.cs", [
  ["            N32FinaleStartRun();\n        }\n",
   "            N32FinaleStartRun();\n            N32CrownStartRun();\n        }\n"],
  ["        partial void N32FinaleStep(double dt);\n",
   "        partial void N32FinaleStep(double dt);\n        partial void N32CrownStartRun(); // NIGHT32 C: THE CROWN, level 5's boss (Game/SurvivalSim.Crown32.cs)\n        partial void N32CrownStep(double dt);\n"],
  ["// - A (arenas), S (span), F (finale) implement",
   "// - C (THE CROWN) implements N32CrownStartRun / N32CrownStep (Game/SurvivalSim.Crown32.cs): boss5.\n// - A (arenas), S (span), F (finale) implement"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Run.cs", [
  ["            N32FinaleStep(step);\n",
   "            N32FinaleStep(step);\n            N32CrownStep(step);\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N32Finale(cmd, args, ref code); // NIGHT32 F: level5 (Harness.N32.Finale.cs)\n",
   "        N32Finale(cmd, args, ref code); // NIGHT32 F: level5 (Harness.N32.Finale.cs)\n        N32Crown(cmd, args, ref code); // NIGHT32 C: crown32 (THE CROWN's sweep and campers) (Harness.N32.Crown.cs)\n"],
  ["    static partial void N32Finale(string cmd, string[] args, ref int code);\n",
   "    static partial void N32Finale(string cmd, string[] args, ref int code);\n    static partial void N32Crown(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["        public static readonly string[] PhaseN32B = { \"bosses32\", \"camp32\", \"boss5\" };\n",
   "        public static readonly string[] PhaseN32B = { \"bosses32\", \"camp32\" };\n        public static readonly string[] PhaseN32C = { \"boss5\" };\n"],
  ["Array.IndexOf(PhaseN32B, s) >= 0 || Array.IndexOf(PhaseN32G, s) >= 0",
   "Array.IndexOf(PhaseN32B, s) >= 0 || Array.IndexOf(PhaseN32C, s) >= 0 || Array.IndexOf(PhaseN32G, s) >= 0"],
  ["        static partial void N32GrappleBody(",
   "        static partial void N32CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n        static partial void N32GrappleBody("],
  ["                    else if (Array.IndexOf(PhaseN32G, s) >= 0) N32GrappleBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN32C, s) >= 0) N32CrownBody(s, g, dir, res, offs, pre, ref body);\n                    else if (Array.IndexOf(PhaseN32G, s) >= 0) N32GrappleBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

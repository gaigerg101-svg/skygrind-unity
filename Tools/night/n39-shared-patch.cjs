// NIGHT39 shared commit (the lead): hooks for N39Blocks in the sim (StartRun, the step hook after N38Step, the fingerprint's off path),
// tape format 22 (the NIGHT39 value, a VarI after the NIGHT38 value), the --off39 / -skygrindOff39 switches (and -skygrindOff39 in
// N29Fixes.IsAutomationArg), the builders' pre-placed harness partials (P1 pistol, R1 ride, F1 slow, B1 boss, V views), and the test chain
// (N37TestBaseline also forces NIGHT39 off; AsFormat21 joins the format chain; the format pins move to 22).
// Run once after n39-config-block.cjs: node Tools/night/n39-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a, b] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 90));
  }
  for (const [a, b] of pairs) s = s.replace(a, b);
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}
const I = "            ";

patch("Assets/Scripts/Game/SurvivalSim.Run.cs", [
  [I + "N38StartRun(); // NIGHT38: the run's NIGHT38 blocks (a tape's, or off with --off38 and older), the same way (SurvivalSim.N38.cs)\n",
   I + "N38StartRun(); // NIGHT38: the run's NIGHT38 blocks (a tape's, or off with --off38 and older), the same way (SurvivalSim.N38.cs)\n" +
   I + "N39StartRun(); // NIGHT39: the run's NIGHT39 blocks (a tape's, or off with --off39 and older), the same way (SurvivalSim.N39.cs)\n"],
  [I + "N38Step(step); // NIGHT38 M / S2: ride38, stag38 (SurvivalSim.N38.cs; nothing until a builder fills its hook)\n",
   I + "N38Step(step); // NIGHT38 M / S2: ride38, stag38 (SurvivalSim.N38.cs; nothing until a builder fills its hook)\n" +
   I + "N39Step(step); // NIGHT39 P1 / B1: p39, boss39 (SurvivalSim.N39.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N38OnPath) n38Override = N38Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N38OnPath) n38Override = N38Blocks.Off;\n" +
   I + "N39Blocks? n39Saved = n39Override; // NIGHT39: and every NIGHT39 block off until the freeze (FingerprintBaseline.N39OnPath)\n" +
   I + "if (!FingerprintBaseline.N39OnPath) n39Override = N39Blocks.Off;\n"],
  ["                n38Override = n38Saved;\n", "                n38Override = n38Saved;\n                n39Override = n39Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N38Init(); // NIGHT38: the NIGHT38 design blocks, read once (SurvivalSim.N38.cs)\n",
   I + "N38Init(); // NIGHT38: the NIGHT38 design blocks, read once (SurvivalSim.N38.cs)\n" + I + "N39Init(); // NIGHT39: the NIGHT39 design blocks, read once (SurvivalSim.N39.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N38OnPath = false;\n",
   "        public const bool N38OnPath = false;\n\n        /// <summary>NIGHT39: false unless the freeze records a run with a sim-changing NIGHT39 default on; until then the composed\n        /// fingerprint runs with every NIGHT39 block off.</summary>\n        public const bool N39OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 21; // NIGHT38:",
   "        public const int FormatVersion = 22; // NIGHT39: 22 adds the NIGHT39 blocks as a VarI (formats 1-21: off); NIGHT38:"],
  ["        public int n38;\n",
   "        public int n38;\n\n        /// <summary>NIGHT39: the NIGHT39 design blocks the run read (N39Blocks.Bits; formats 1-21: 0, every block off, NIGHT38's game).</summary>\n        public int n39;\n"],
  ["            w.VarI(n38); // format 21: the NIGHT38 blocks\n",
   "            w.VarI(n38); // format 21: the NIGHT38 blocks\n            w.VarI(n39); // format 22: the NIGHT39 blocks\n"],
  ["            t.n38 = version >= 21 ? r.VarI() : 0; // formats 1-20: recorded on NIGHT37's game (or before), so every NIGHT38 block OFF (explicitly)\n",
   "            t.n38 = version >= 21 ? r.VarI() : 0; // formats 1-20: recorded on NIGHT37's game (or before), so every NIGHT38 block OFF (explicitly)\n" +
   "            t.n39 = version >= 22 ? r.VarI() : 0; // formats 1-21: recorded on NIGHT38's game (or before), so every NIGHT39 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n38 = sim.n38.Bits; // NIGHT38: the NIGHT38 blocks the run read\n",
   "            tape.n38 = sim.n38.Bits; // NIGHT38: the NIGHT38 blocks the run read\n            tape.n39 = sim.n39.Bits; // NIGHT39: the NIGHT39 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n38Override = N38Blocks.FromBits(tape.n38); // NIGHT38: the NIGHT38 blocks the run read (formats 1-20: all off, NIGHT37's game)\n",
   "            sim.n38Override = N38Blocks.FromBits(tape.n38); // NIGHT38: the NIGHT38 blocks the run read (formats 1-20: all off, NIGHT37's game)\n" +
   "            sim.n39Override = N39Blocks.FromBits(tape.n39); // NIGHT39: the NIGHT39 blocks the run read (formats 1-21: all off, NIGHT38's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N38OnPath) sim.n38Override = N38Blocks.Off;\n",
   "            if (!FingerprintBaseline.N38OnPath) sim.n38Override = N38Blocks.Off;\n            N39Blocks? n39Saved = sim.n39Override; // NIGHT39: and every NIGHT39 block off until the freeze (N39OnPath)\n            if (!FingerprintBaseline.N39OnPath) sim.n39Override = N39Blocks.Off;\n"],
  ["                sim.n38Override = n38Saved;\n", "                sim.n38Override = n38Saved;\n                sim.n39Override = n39Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N38Blocks.ForceOff = off38;\n",
   "        N38Blocks.ForceOff = off38;\n        // NIGHT39: --off39 runs every command with every NIGHT39 design block off (NIGHT38's game); every older switch turns them off as well\n        bool off39 = off38 || Array.IndexOf(args, \"--off39\") >= 0;\n        N39Blocks.ForceOff = off39;\n"],
  ["                if (off38) sim.n38Override = N38Blocks.Off; // NIGHT38: the same\n",
   "                if (off38) sim.n38Override = N38Blocks.Off; // NIGHT38: the same\n                if (off39) sim.n39Override = N39Blocks.Off; // NIGHT39: the same\n"],
  ["        N38Spire(cmd, args, ref code); // NIGHT38 C2: level 5 (Harness.N38.Spire.cs)\n",
   "        N38Spire(cmd, args, ref code); // NIGHT38 C2: level 5 (Harness.N38.Spire.cs)\n" +
   "        N39Pistol(cmd, args, ref code); // NIGHT39 P1: the KESTREL (Harness.N39.Pistol.cs)\n" +
   "        N39Ride(cmd, args, ref code); // NIGHT39 R1: ride39 (Harness.N39.Ride.cs)\n" +
   "        N39Slow(cmd, args, ref code); // NIGHT39 F1: slow39 and the parry (Harness.N39.Slow.cs)\n" +
   "        N39Boss(cmd, args, ref code); // NIGHT39 B1: boss39 bands (Harness.N39.Boss.cs)\n" +
   "        N39Views(cmd, args, ref code); // NIGHT39 V: the NIGHT38 views and the two tests (Harness.N39.Views.cs)\n"],
  ["    static partial void N38Spire(string cmd, string[] args, ref int code);\n",
   "    static partial void N38Spire(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT39: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N39Pistol(string cmd, string[] args, ref int code);\n" +
   "    static partial void N39Ride(string cmd, string[] args, ref int code);\n" +
   "    static partial void N39Slow(string cmd, string[] args, ref int code);\n" +
   "    static partial void N39Boss(string cmd, string[] args, ref int code);\n" +
   "    static partial void N39Views(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o38 = 19; o38 <= 38; o38++) if (Array.IndexOf(args, \"-skygrindOff\" + o38) >= 0) N38Blocks.ForceOff = true; // NIGHT38: -skygrindOff38 and every older switch turn the NIGHT38 blocks off\n",
   "            for (int o38 = 19; o38 <= 38; o38++) if (Array.IndexOf(args, \"-skygrindOff\" + o38) >= 0) N38Blocks.ForceOff = true; // NIGHT38: -skygrindOff38 and every older switch turn the NIGHT38 blocks off\n" +
   "            for (int o39 = 19; o39 <= 39; o39++) if (Array.IndexOf(args, \"-skygrindOff\" + o39) >= 0) N39Blocks.ForceOff = true; // NIGHT39: -skygrindOff39 and every older switch turn the NIGHT39 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff38\" && ", "a != \"-skygrindOff38\" && a != \"-skygrindOff39\" && "],
]);

patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was, was38;\n", "        bool was, was38, was39;\n"],
  ["was38 = N38Blocks.ForceOff; N38Blocks.ForceOff = true; } // NIGHT38: and every NIGHT38 block off (N38SharedTests)\n",
   "was38 = N38Blocks.ForceOff; N38Blocks.ForceOff = true; was39 = N39Blocks.ForceOff; N39Blocks.ForceOff = true; } // NIGHT38 / NIGHT39: and every NIGHT38 / NIGHT39 block off (N38SharedTests, N39SharedTests)\n"],
  ["        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; }\n",
   "        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; N39Blocks.ForceOff = was39; }\n"],
  ["Is.EqualTo(21), \"NIGHT38: the current format is 21 (N38SharedTests.AsFormat20 strips its value)\");\n",
   "Is.EqualTo(22), \"NIGHT39: the current format is 22 (N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n"],
]);

patch("Assets/Tests/EditMode/Game/N38SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(21));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(22), \"NIGHT39: the current format is 22 (N39SharedTests.AsFormat21 strips its value)\");\n"],
  ["        public static byte[] AsFormat20(byte[] b21)\n        {\n",
   "        public static byte[] AsFormat20(byte[] b21)\n        {\n            b21 = N39SharedTests.AsFormat21(b21); // NIGHT39: a format 22 tape first loses its NIGHT39 value\n"],
]);

console.log("NIGHT39 shared patch done. Next: the Editor compile, CoreHarness golden, SimHarness fingerprint --off39, the shared tests.");

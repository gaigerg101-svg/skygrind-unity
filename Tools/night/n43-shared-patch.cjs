// NIGHT43 shared commit (the lead): hooks for N43Blocks in the sim (Init, StartRun, the step hook after N42Step, the fingerprint's off
// path), tape format 26 (the NIGHT43 value, a VarI after the NIGHT42 value), the --off43 / -skygrindOff43 switches (and -skygrindOff43 in
// N29Fixes.IsAutomationArg), the builders' pre-placed harness partials, the reel dispatch (FeelReel.N43.cs), the level swaps
// (Curriculum25.N43Levels after N42Levels; N42BaseId asks N43BaseId first), and the test chain (N37SharedTests and Lesson25 force NIGHT43
// off; AsFormat25 joins the format chain; the format pins move to 26).
// Run once after n43-config-block.cjs: node Tools/night/n43-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 90));
  }
  for (const [a, b] of pairs) s = s.replace(a, () => b);
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}
const I = "            ";

patch("Assets/Scripts/Game/SurvivalSim.Run.cs", [
  [I + "N42StartRun(); // NIGHT42: the run's NIGHT42 blocks (a tape's, or off with --off42 and older), the same way (SurvivalSim.N42.cs)\n",
   I + "N42StartRun(); // NIGHT42: the run's NIGHT42 blocks (a tape's, or off with --off42 and older), the same way (SurvivalSim.N42.cs)\n" +
   I + "N43StartRun(); // NIGHT43: the run's NIGHT43 blocks (a tape's, or off with --off43 and older), the same way (SurvivalSim.N43.cs)\n"],
  [I + "N42Step(step); // NIGHT42 M1 / W1 / F1 / B1 / K1 (SurvivalSim.N42.cs; nothing until a builder fills its hook)\n",
   I + "N42Step(step); // NIGHT42 M1 / W1 / F1 / B1 / K1 (SurvivalSim.N42.cs; nothing until a builder fills its hook)\n" +
   I + "N43Step(step); // NIGHT43 M1 / W1 / E1 / A1 / B1 / K1 (SurvivalSim.N43.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N42OnPath) n42Override = N42Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N42OnPath) n42Override = N42Blocks.Off;\n" +
   I + "N43Blocks? n43Saved = n43Override; // NIGHT43: and every NIGHT43 block off until the freeze (FingerprintBaseline.N43OnPath)\n" +
   I + "if (!FingerprintBaseline.N43OnPath) n43Override = N43Blocks.Off;\n"],
  ["                n42Override = n42Saved;\n", "                n42Override = n42Saved;\n                n43Override = n43Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N42Init(); // NIGHT42: the NIGHT42 design blocks and level rows, read once (SurvivalSim.N42.cs)\n",
   I + "N42Init(); // NIGHT42: the NIGHT42 design blocks and level rows, read once (SurvivalSim.N42.cs)\n" + I + "N43Init(); // NIGHT43: the NIGHT43 design blocks and level rows, read once (SurvivalSim.N43.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.N42.cs", [
  ["        public string N42BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n",
   "        public string N42BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n            levelId = N43BaseId(levelId); // NIGHT43 (the lead's hook): an n43 level is its levelFrom first (n43-foundry -> n42-foundry)\n"],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N42Levels(cfg, res);\n", "            N42Levels(cfg, res);\n            N43Levels(cfg, res);\n"],
  ["        static readonly string[] N42LevelRows = { \"foundry42\" };\n",
   "        static readonly string[] N42LevelRows = { \"foundry42\" };\n\n" +
   "        /// <summary>NIGHT43 (the lead's hook; L1 / E1 / K1 own the rows): foundry43, iceline43 and kiln43 on swap their `levelFrom`\n" +
   "        /// (n42-foundry, n40-iceline, n41-kiln) for their `levelTo` (n43-*), after N42Levels. Off with -skygrindOff43 / --off43\n" +
   "        /// (N43Blocks.ForceOff) or the row off.</summary>\n" +
   "        public static void N43Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N43Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N43LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n43.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n43.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n43.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "        }\n\n" +
   "        static readonly string[] N43LevelRows = { \"foundry43\", \"iceline43\", \"kiln43\" };\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N42OnPath = false;\n",
   "        public const bool N42OnPath = false;\n\n        /// <summary>NIGHT43: false unless the freeze records a run with a sim-changing NIGHT43 default on; until then the composed\n        /// fingerprint runs with every NIGHT43 block off.</summary>\n        public const bool N43OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 25; // NIGHT42:",
   "        public const int FormatVersion = 26; // NIGHT43: 26 adds the NIGHT43 blocks as a VarI (formats 1-25: off); NIGHT42:"],
  ["        public int n42;\n",
   "        public int n42;\n\n        /// <summary>NIGHT43: the NIGHT43 design blocks the run read (N43Blocks.Bits; formats 1-25: 0, every block off, NIGHT42's game).</summary>\n        public int n43;\n"],
  ["            w.VarI(n42); // format 25: the NIGHT42 blocks\n",
   "            w.VarI(n42); // format 25: the NIGHT42 blocks\n            w.VarI(n43); // format 26: the NIGHT43 blocks\n"],
  ["            t.n42 = version >= 25 ? r.VarI() : 0; // formats 1-24: recorded on NIGHT41's game (or before), so every NIGHT42 block OFF (explicitly)\n",
   "            t.n42 = version >= 25 ? r.VarI() : 0; // formats 1-24: recorded on NIGHT41's game (or before), so every NIGHT42 block OFF (explicitly)\n" +
   "            t.n43 = version >= 26 ? r.VarI() : 0; // formats 1-25: recorded on NIGHT42's game (or before), so every NIGHT43 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n42 = sim.n42.Bits; // NIGHT42: the NIGHT42 blocks the run read\n",
   "            tape.n42 = sim.n42.Bits; // NIGHT42: the NIGHT42 blocks the run read\n            tape.n43 = sim.n43.Bits; // NIGHT43: the NIGHT43 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n42Override = N42Blocks.FromBits(tape.n42); // NIGHT42: the NIGHT42 blocks the run read (formats 1-24: all off, NIGHT41's game)\n",
   "            sim.n42Override = N42Blocks.FromBits(tape.n42); // NIGHT42: the NIGHT42 blocks the run read (formats 1-24: all off, NIGHT41's game)\n" +
   "            sim.n43Override = N43Blocks.FromBits(tape.n43); // NIGHT43: the NIGHT43 blocks the run read (formats 1-25: all off, NIGHT42's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N42OnPath) sim.n42Override = N42Blocks.Off;\n",
   "            if (!FingerprintBaseline.N42OnPath) sim.n42Override = N42Blocks.Off;\n            N43Blocks? n43Saved = sim.n43Override; // NIGHT43: and every NIGHT43 block off until the freeze (N43OnPath)\n            if (!FingerprintBaseline.N43OnPath) sim.n43Override = N43Blocks.Off;\n"],
  ["                sim.n42Override = n42Saved;\n", "                sim.n42Override = n42Saved;\n                sim.n43Override = n43Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N42Blocks.ForceOff = off42;\n",
   "        N42Blocks.ForceOff = off42;\n        // NIGHT43: --off43 runs every command with every NIGHT43 design block off (NIGHT42's game); every older switch turns them off as well\n        bool off43 = off42 || Array.IndexOf(args, \"--off43\") >= 0;\n        N43Blocks.ForceOff = off43;\n"],
  ["                if (off42) sim.n42Override = N42Blocks.Off; // NIGHT42: the same\n",
   "                if (off42) sim.n42Override = N42Blocks.Off; // NIGHT42: the same\n                if (off43) sim.n43Override = N43Blocks.Off; // NIGHT43: the same\n"],
  ["        N42Arena(cmd, args, ref code); // NIGHT42 K1: arena42 (Harness.N42.Arena.cs)\n",
   "        N42Arena(cmd, args, ref code); // NIGHT42 K1: arena42 (Harness.N42.Arena.cs)\n" +
   "        N43Move(cmd, args, ref code); // NIGHT43 M1: chimney43, ride43, boost43 (Harness.N43.Move.cs)\n" +
   "        N43Level(cmd, args, ref code); // NIGHT43 L1: n43-foundry (Harness.N43.Level.cs)\n" +
   "        N43Weapons(cmd, args, ref code); // NIGHT43 W1: gauntlet43, parry43, fusion43 (Harness.N43.Weapons.cs)\n" +
   "        N43Rooms(cmd, args, ref code); // NIGHT43 E1: rooms43, iceline43 (Harness.N43.Rooms.cs)\n" +
   "        N43Kiln(cmd, args, ref code); // NIGHT43 K1: kiln43 (Harness.N43.Kiln.cs)\n" +
   "        N43Burrow(cmd, args, ref code); // NIGHT43 A1: burrow43 (Harness.N43.Burrow.cs)\n" +
   "        N43Wick(cmd, args, ref code); // NIGHT43 B1: wick43 (Harness.N43.Wick.cs)\n"],
  ["    static partial void N42Arena(string cmd, string[] args, ref int code);\n",
   "    static partial void N42Arena(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT43: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N43Move(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Level(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Rooms(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Kiln(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Burrow(string cmd, string[] args, ref int code);\n" +
   "    static partial void N43Wick(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o42 = 19; o42 <= 42; o42++) if (Array.IndexOf(args, \"-skygrindOff\" + o42) >= 0) N42Blocks.ForceOff = true; // NIGHT42: -skygrindOff42 and every older switch turn the NIGHT42 blocks off\n",
   "            for (int o42 = 19; o42 <= 42; o42++) if (Array.IndexOf(args, \"-skygrindOff\" + o42) >= 0) N42Blocks.ForceOff = true; // NIGHT42: -skygrindOff42 and every older switch turn the NIGHT42 blocks off\n" +
   "            for (int o43 = 19; o43 <= 43; o43++) if (Array.IndexOf(args, \"-skygrindOff\" + o43) >= 0) N43Blocks.ForceOff = true; // NIGHT43: -skygrindOff43 and every older switch turn the NIGHT43 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff42\" && ", "a != \"-skygrindOff42\" && a != \"-skygrindOff43\" && "],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN42, s) >= 0; // NIGHT42:", "|| Array.IndexOf(PhaseN42, s) >= 0 || Array.IndexOf(PhaseN43, s) >= 0; // NIGHT43: the builders' proof reels (FeelReel.N43.cs); NIGHT42:"],
  ["                    else if (Array.IndexOf(PhaseN42, s) >= 0) N42ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT42 (FeelReel.N42.cs)\n",
   "                    else if (Array.IndexOf(PhaseN42, s) >= 0) N42ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT42 (FeelReel.N42.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN43, s) >= 0) N43ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT43 (FeelReel.N43.cs)\n"],
]);

// the tests
patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was, was38, was39, was40, was41, was42;\n", "        bool was, was38, was39, was40, was41, was42, was43;\n"],
  ["was42 = N42Blocks.ForceOff; N42Blocks.ForceOff = true; } // NIGHT38-NIGHT42: and every NIGHT38-NIGHT42 block off (N38SharedTests ... N42SharedTests)\n",
   "was42 = N42Blocks.ForceOff; N42Blocks.ForceOff = true; was43 = N43Blocks.ForceOff; N43Blocks.ForceOff = true; } // NIGHT38-NIGHT43: and every NIGHT38-NIGHT43 block off (N38SharedTests ... N43SharedTests)\n"],
  ["N42Blocks.ForceOff = was42; }\n", "N42Blocks.ForceOff = was42; N43Blocks.ForceOff = was43; }\n"],
  ["Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, ", "Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, "],
]);
patch("Assets/Tests/EditMode/Game/N38SharedTests.cs", [
  ["Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, ", "Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, "],
]);
patch("Assets/Tests/EditMode/Game/N39SharedTests.cs", [
  ["Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, ", "Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, "],
]);
patch("Assets/Tests/EditMode/Game/N40SharedTests.cs", [
  ["Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24 and ", "Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24 and "],
]);
patch("Assets/Tests/EditMode/Game/N41SharedTests.cs", [
  ["Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24 strips its value)\");\n",
   "Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25 and N42SharedTests.AsFormat24 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N42SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(25));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(26), \"NIGHT43: the current format is 26 (N43SharedTests.AsFormat25 strips its value)\");\n"],
  ["        public static byte[] AsFormat24(byte[] b25)\n        {\n",
   "        public static byte[] AsFormat24(byte[] b25)\n        {\n            b25 = N43SharedTests.AsFormat25(b25); // NIGHT43: a format 26 tape first loses its NIGHT43 value\n"],
]);
patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  ["            N42Blocks.ForceOff = true; // NIGHT42 (L-027):",
   "            N43Blocks.ForceOff = true; // NIGHT43 (L-027): foundry43 / iceline43 / kiln43 swap their n43 levels in once ON; this test pins NIGHT39's and earlier orders (N43SharedTests pins its own swaps)\n            N42Blocks.ForceOff = true; // NIGHT42 (L-027):"],
]);

console.log("NIGHT43 shared patch done. Next: the Editor compile, golden, SimHarness fingerprint plain and --off43, the shared tests.");

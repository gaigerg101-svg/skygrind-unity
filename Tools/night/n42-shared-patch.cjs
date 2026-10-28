// NIGHT42 shared commit (the lead): hooks for N42Blocks in the sim (StartRun, the step hook after N41Step, the fingerprint's off path),
// tape format 25 (the NIGHT42 value, a VarI after the NIGHT41 value), the --off42 / -skygrindOff42 switches (and -skygrindOff42 in
// N29Fixes.IsAutomationArg), the builders' pre-placed harness partials, the reel dispatch (FeelReel.N42.cs), the level swap
// (Curriculum25.N42Levels after N41Levels; N41BaseId asks N42BaseId first), and the test chain (N37SharedTests and Lesson25 force NIGHT42
// off; AsFormat24 joins the format chain; the format pins move to 25).
// Run once after n42-config-block.cjs: node Tools/night/n42-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N41StartRun(); // NIGHT41: the run's NIGHT41 blocks (a tape's, or off with --off41 and older), the same way (SurvivalSim.N41.cs)\n",
   I + "N41StartRun(); // NIGHT41: the run's NIGHT41 blocks (a tape's, or off with --off41 and older), the same way (SurvivalSim.N41.cs)\n" +
   I + "N42StartRun(); // NIGHT42: the run's NIGHT42 blocks (a tape's, or off with --off42 and older), the same way (SurvivalSim.N42.cs)\n"],
  [I + "N41Step(step); // NIGHT41 B8 / E6 / P3 (SurvivalSim.N41.cs; nothing until a builder fills its hook)\n",
   I + "N41Step(step); // NIGHT41 B8 / E6 / P3 (SurvivalSim.N41.cs; nothing until a builder fills its hook)\n" +
   I + "N42Step(step); // NIGHT42 M1 / W1 / F1 / B1 / K1 (SurvivalSim.N42.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N41OnPath) n41Override = N41Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N41OnPath) n41Override = N41Blocks.Off;\n" +
   I + "N42Blocks? n42Saved = n42Override; // NIGHT42: and every NIGHT42 block off until the freeze (FingerprintBaseline.N42OnPath)\n" +
   I + "if (!FingerprintBaseline.N42OnPath) n42Override = N42Blocks.Off;\n"],
  ["                n41Override = n41Saved;\n", "                n41Override = n41Saved;\n                n42Override = n42Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N41Init(); // NIGHT41: the NIGHT41 design blocks and level rows, read once (SurvivalSim.N41.cs)\n",
   I + "N41Init(); // NIGHT41: the NIGHT41 design blocks and level rows, read once (SurvivalSim.N41.cs)\n" + I + "N42Init(); // NIGHT42: the NIGHT42 design blocks and level rows, read once (SurvivalSim.N42.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.N41.cs", [
  ["        public string N41BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n",
   "        public string N41BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n            levelId = N42BaseId(levelId); // NIGHT42 (the lead's hook): an n42 level is its levelFrom first (n42-foundry -> n41-foundry)\n"],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N41Levels(cfg, res);\n", "            N41Levels(cfg, res);\n            N42Levels(cfg, res);\n"],
  ["        static readonly string[] N41LevelRows = { \"foundry41\", \"kiln41\", \"spire41\" };\n",
   "        static readonly string[] N41LevelRows = { \"foundry41\", \"kiln41\", \"spire41\" };\n\n" +
   "        /// <summary>NIGHT42 (the lead's hook; L1 owns the row): foundry42 on swaps its `levelFrom` (n41-foundry) for its `levelTo`\n" +
   "        /// (n42-foundry), after N41Levels. Off with -skygrindOff42 / --off42 (N42Blocks.ForceOff) or the row off.</summary>\n" +
   "        public static void N42Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N42Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N42LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n42.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n42.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n42.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "        }\n\n" +
   "        static readonly string[] N42LevelRows = { \"foundry42\" };\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N41OnPath = false;\n",
   "        public const bool N41OnPath = false;\n\n        /// <summary>NIGHT42: false unless the freeze records a run with a sim-changing NIGHT42 default on; until then the composed\n        /// fingerprint runs with every NIGHT42 block off.</summary>\n        public const bool N42OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 24; // NIGHT41:",
   "        public const int FormatVersion = 25; // NIGHT42: 25 adds the NIGHT42 blocks as a VarI (formats 1-24: off); NIGHT41:"],
  ["        public int n41;\n",
   "        public int n41;\n\n        /// <summary>NIGHT42: the NIGHT42 design blocks the run read (N42Blocks.Bits; formats 1-24: 0, every block off, NIGHT41's game).</summary>\n        public int n42;\n"],
  ["            w.VarI(n41); // format 24: the NIGHT41 blocks\n",
   "            w.VarI(n41); // format 24: the NIGHT41 blocks\n            w.VarI(n42); // format 25: the NIGHT42 blocks\n"],
  ["            t.n41 = version >= 24 ? r.VarI() : 0; // formats 1-23: recorded on NIGHT40's game (or before), so every NIGHT41 block OFF (explicitly)\n",
   "            t.n41 = version >= 24 ? r.VarI() : 0; // formats 1-23: recorded on NIGHT40's game (or before), so every NIGHT41 block OFF (explicitly)\n" +
   "            t.n42 = version >= 25 ? r.VarI() : 0; // formats 1-24: recorded on NIGHT41's game (or before), so every NIGHT42 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n41 = sim.n41.Bits; // NIGHT41: the NIGHT41 blocks the run read\n",
   "            tape.n41 = sim.n41.Bits; // NIGHT41: the NIGHT41 blocks the run read\n            tape.n42 = sim.n42.Bits; // NIGHT42: the NIGHT42 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n41Override = N41Blocks.FromBits(tape.n41); // NIGHT41: the NIGHT41 blocks the run read (formats 1-23: all off, NIGHT40's game)\n",
   "            sim.n41Override = N41Blocks.FromBits(tape.n41); // NIGHT41: the NIGHT41 blocks the run read (formats 1-23: all off, NIGHT40's game)\n" +
   "            sim.n42Override = N42Blocks.FromBits(tape.n42); // NIGHT42: the NIGHT42 blocks the run read (formats 1-24: all off, NIGHT41's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N41OnPath) sim.n41Override = N41Blocks.Off;\n",
   "            if (!FingerprintBaseline.N41OnPath) sim.n41Override = N41Blocks.Off;\n            N42Blocks? n42Saved = sim.n42Override; // NIGHT42: and every NIGHT42 block off until the freeze (N42OnPath)\n            if (!FingerprintBaseline.N42OnPath) sim.n42Override = N42Blocks.Off;\n"],
  ["                sim.n41Override = n41Saved;\n", "                sim.n41Override = n41Saved;\n                sim.n42Override = n42Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N41Blocks.ForceOff = off41;\n",
   "        N41Blocks.ForceOff = off41;\n        // NIGHT42: --off42 runs every command with every NIGHT42 design block off (NIGHT41's game); every older switch turns them off as well\n        bool off42 = off41 || Array.IndexOf(args, \"--off42\") >= 0;\n        N42Blocks.ForceOff = off42;\n"],
  ["                if (off41) sim.n41Override = N41Blocks.Off; // NIGHT41: the same\n",
   "                if (off41) sim.n41Override = N41Blocks.Off; // NIGHT41: the same\n                if (off42) sim.n42Override = N42Blocks.Off; // NIGHT42: the same\n"],
  ["        N41Finale(cmd, args, ref code); // NIGHT41 L6: the level 6 greybox (Harness.N41.Finale.cs)\n",
   "        N41Finale(cmd, args, ref code); // NIGHT41 L6: the level 6 greybox (Harness.N41.Finale.cs)\n" +
   "        N42Move(cmd, args, ref code); // NIGHT42 M1: wallfling42, boost42 (Harness.N42.Move.cs)\n" +
   "        N42Level(cmd, args, ref code); // NIGHT42 L1: n42-foundry, teach42 (Harness.N42.Level.cs)\n" +
   "        N42Weapons(cmd, args, ref code); // NIGHT42 W1: gauntlet42, parry42, charge42, reticle42 (Harness.N42.Weapons.cs)\n" +
   "        N42Impact(cmd, args, ref code); // NIGHT42 F1: impact42, kills42 (Harness.N42.Impact.cs)\n" +
   "        N42Ui(cmd, args, ref code); // NIGHT42 U1: meter42, style42, health42 (Harness.N42.Ui.cs)\n" +
   "        N42Brawl(cmd, args, ref code); // NIGHT42 B1: brawl42 (Harness.N42.Brawl.cs)\n" +
   "        N42Arena(cmd, args, ref code); // NIGHT42 K1: arena42 (Harness.N42.Arena.cs)\n"],
  ["    static partial void N41Finale(string cmd, string[] args, ref int code);\n",
   "    static partial void N41Finale(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT42: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N42Move(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Level(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Impact(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Ui(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Brawl(string cmd, string[] args, ref int code);\n" +
   "    static partial void N42Arena(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o41 = 19; o41 <= 41; o41++) if (Array.IndexOf(args, \"-skygrindOff\" + o41) >= 0) N41Blocks.ForceOff = true; // NIGHT41: -skygrindOff41 and every older switch turn the NIGHT41 blocks off\n",
   "            for (int o41 = 19; o41 <= 41; o41++) if (Array.IndexOf(args, \"-skygrindOff\" + o41) >= 0) N41Blocks.ForceOff = true; // NIGHT41: -skygrindOff41 and every older switch turn the NIGHT41 blocks off\n" +
   "            for (int o42 = 19; o42 <= 42; o42++) if (Array.IndexOf(args, \"-skygrindOff\" + o42) >= 0) N42Blocks.ForceOff = true; // NIGHT42: -skygrindOff42 and every older switch turn the NIGHT42 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff41\" && ", "a != \"-skygrindOff41\" && a != \"-skygrindOff42\" && "],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN41T, s) >= 0; // NIGHT41 T2:", "|| Array.IndexOf(PhaseN41T, s) >= 0 || Array.IndexOf(PhaseN42, s) >= 0; // NIGHT42: the builders' proof reels (FeelReel.N42.cs); NIGHT41 T2:"],
  ["                    else if (Array.IndexOf(PhaseN41T, s) >= 0) N41ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT41 T2 (FeelReel.N41.cs)\n",
   "                    else if (Array.IndexOf(PhaseN41T, s) >= 0) N41ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT41 T2 (FeelReel.N41.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN42, s) >= 0) N42ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT42 (FeelReel.N42.cs)\n"],
]);

// the tests
patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was, was38, was39, was40, was41;\n", "        bool was, was38, was39, was40, was41, was42;\n"],
  ["was41 = N41Blocks.ForceOff; N41Blocks.ForceOff = true; } // NIGHT38-NIGHT41: and every NIGHT38-NIGHT41 block off (N38SharedTests ... N41SharedTests)\n",
   "was41 = N41Blocks.ForceOff; N41Blocks.ForceOff = true; was42 = N42Blocks.ForceOff; N42Blocks.ForceOff = true; } // NIGHT38-NIGHT42: and every NIGHT38-NIGHT42 block off (N38SharedTests ... N42SharedTests)\n"],
  ["N41Blocks.ForceOff = was41; }\n", "N41Blocks.ForceOff = was41; N42Blocks.ForceOff = was42; }\n"],
  ["Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23, N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n",
   "Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, N41SharedTests.AsFormat23, N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N38SharedTests.cs", [
  ["Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23, N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)\");\n",
   "Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, N41SharedTests.AsFormat23, N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N39SharedTests.cs", [
  ["Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23 and N40SharedTests.AsFormat22 strip their values)\");\n",
   "Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24, N41SharedTests.AsFormat23 and N40SharedTests.AsFormat22 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N40SharedTests.cs", [
  ["Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23 strips its value)\");\n",
   "Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24 and N41SharedTests.AsFormat23 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N41SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(24));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(25), \"NIGHT42: the current format is 25 (N42SharedTests.AsFormat24 strips its value)\");\n"],
  ["        public static byte[] AsFormat23(byte[] b24)\n        {\n",
   "        public static byte[] AsFormat23(byte[] b24)\n        {\n            b24 = N42SharedTests.AsFormat24(b24); // NIGHT42: a format 25 tape first loses its NIGHT42 value\n"],
]);
patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  ["            N41Blocks.ForceOff = true; // NIGHT41 (L-027):",
   "            N42Blocks.ForceOff = true; // NIGHT42 (L-027): foundry42 swaps n42-foundry in once ON; this test pins NIGHT39's and earlier orders (N42SharedTests pins its own swap)\n            N41Blocks.ForceOff = true; // NIGHT41 (L-027):"],
]);

console.log("NIGHT42 shared patch done. Next: the Editor compile, golden, SimHarness fingerprint plain and --off42, the shared tests.");

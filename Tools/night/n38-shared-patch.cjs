// NIGHT38 shared commit (the lead): hooks for N38Blocks in the sim (StartRun, the step hook after N37Step, the fingerprint's off path),
// tape format 21 (the NIGHT38 value, a VarI after the NIGHT37 value), the --off38 / -skygrindOff38 switches, Curriculum25.N38Levels (each
// level row's levelFrom -> levelTo, after N37Levels), the builders' pre-placed harness partials (E gates, M ride, H hud, F foundry + finale,
// S2 iceline, K3 kiln, O2 span, C2 spire), and the test baseline (N37TestBaseline also forces NIGHT38 off; AsFormat20 joins the chain).
// Run once: node Tools/night/n38-shared-patch.cjs   (every replace asserts its anchor exists exactly once; design.n38 first: n38-config-block.cjs)
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
const I = "            ";

patch("Assets/Scripts/Game/SurvivalSim.Run.cs", [
  [I + "N37StartRun(); // NIGHT37: the run's NIGHT37 blocks (a tape's, or off with --off37 and older), the same way (SurvivalSim.N37.cs)\n",
   I + "N37StartRun(); // NIGHT37: the run's NIGHT37 blocks (a tape's, or off with --off37 and older), the same way (SurvivalSim.N37.cs)\n" +
   I + "N38StartRun(); // NIGHT38: the run's NIGHT38 blocks (a tape's, or off with --off38 and older), the same way (SurvivalSim.N38.cs)\n"],
  [I + "N37Step(step); // NIGHT37 W / S / O / C: THE WICK, THE STAG, THE LOOM, THE CROWN (SurvivalSim.N37.cs)\n",
   I + "N37Step(step); // NIGHT37 W / S / O / C: THE WICK, THE STAG, THE LOOM, THE CROWN (SurvivalSim.N37.cs)\n" +
   I + "N38Step(step); // NIGHT38 M / S2: ride38, stag38 (SurvivalSim.N38.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N37OnPath) n37Override = N37Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N37OnPath) n37Override = N37Blocks.Off;\n" +
   I + "N38Blocks? n38Saved = n38Override; // NIGHT38: and every NIGHT38 block off until the freeze (FingerprintBaseline.N38OnPath)\n" +
   I + "if (!FingerprintBaseline.N38OnPath) n38Override = N38Blocks.Off;\n"],
  ["                n37Override = n37Saved;\n", "                n37Override = n37Saved;\n                n38Override = n38Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N37Init(); // NIGHT37: the NIGHT37 design blocks, read once (SurvivalSim.N37.cs)\n",
   I + "N37Init(); // NIGHT37: the NIGHT37 design blocks, read once (SurvivalSim.N37.cs)\n" + I + "N38Init(); // NIGHT38: the NIGHT38 design blocks, read once (SurvivalSim.N38.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N37OnPath = false;\n",
   "        public const bool N37OnPath = false;\n\n        /// <summary>NIGHT38: false unless the freeze records a run with a sim-changing NIGHT38 default on; until then the composed\n        /// fingerprint runs with every NIGHT38 block off.</summary>\n        public const bool N38OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 20; // NIGHT37:",
   "        public const int FormatVersion = 21; // NIGHT38: 21 adds the NIGHT38 blocks as a VarI (formats 1-20: off); NIGHT37:"],
  ["        public int n37;\n",
   "        public int n37;\n\n        /// <summary>NIGHT38: the NIGHT38 design blocks the run read (N38Blocks.Bits; formats 1-20: 0, every block off, NIGHT37's game).</summary>\n        public int n38;\n"],
  ["            w.VarI(n37); // format 20: the NIGHT37 blocks\n",
   "            w.VarI(n37); // format 20: the NIGHT37 blocks\n            w.VarI(n38); // format 21: the NIGHT38 blocks\n"],
  ["            t.n37 = version >= 20 ? r.VarI() : 0; // formats 1-19: recorded on NIGHT36's game (or before), so every NIGHT37 block OFF (explicitly)\n",
   "            t.n37 = version >= 20 ? r.VarI() : 0; // formats 1-19: recorded on NIGHT36's game (or before), so every NIGHT37 block OFF (explicitly)\n" +
   "            t.n38 = version >= 21 ? r.VarI() : 0; // formats 1-20: recorded on NIGHT37's game (or before), so every NIGHT38 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n37 = sim.n37.Bits; // NIGHT37: the NIGHT37 blocks the run read\n",
   "            tape.n37 = sim.n37.Bits; // NIGHT37: the NIGHT37 blocks the run read\n            tape.n38 = sim.n38.Bits; // NIGHT38: the NIGHT38 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n37Override = N37Blocks.FromBits(tape.n37); // NIGHT37: the NIGHT37 blocks the run read (formats 1-19: all off, NIGHT36's game)\n",
   "            sim.n37Override = N37Blocks.FromBits(tape.n37); // NIGHT37: the NIGHT37 blocks the run read (formats 1-19: all off, NIGHT36's game)\n" +
   "            sim.n38Override = N38Blocks.FromBits(tape.n38); // NIGHT38: the NIGHT38 blocks the run read (formats 1-20: all off, NIGHT37's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N37OnPath) sim.n37Override = N37Blocks.Off;\n",
   "            if (!FingerprintBaseline.N37OnPath) sim.n37Override = N37Blocks.Off;\n            N38Blocks? n38Saved = sim.n38Override; // NIGHT38: and every NIGHT38 block off until the freeze (N38OnPath)\n            if (!FingerprintBaseline.N38OnPath) sim.n38Override = N38Blocks.Off;\n"],
  ["                sim.n37Override = n37Saved;\n", "                sim.n37Override = n37Saved;\n                sim.n38Override = n38Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N37Blocks.ForceOff = off37;\n",
   "        N37Blocks.ForceOff = off37;\n        // NIGHT38: --off38 runs every command with every NIGHT38 design block off (NIGHT37's game); every older switch turns them off as well\n        bool off38 = off37 || Array.IndexOf(args, \"--off38\") >= 0;\n        N38Blocks.ForceOff = off38;\n"],
  ["                if (off37) sim.n37Override = N37Blocks.Off; // NIGHT37: the same\n",
   "                if (off37) sim.n37Override = N37Blocks.Off; // NIGHT37: the same\n                if (off38) sim.n38Override = N38Blocks.Off; // NIGHT38: the same\n"],
  ["        N37Crown(cmd, args, ref code); // NIGHT37 C: THE CROWN (Harness.N37.Crown.cs)\n",
   "        N37Crown(cmd, args, ref code); // NIGHT37 C: THE CROWN (Harness.N37.Crown.cs)\n" +
   "        N38Gates(cmd, args, ref code); // NIGHT38 E: the envelopes and the gate checker (Harness.N38.Gates.cs)\n" +
   "        N38Ride(cmd, args, ref code); // NIGHT38 M: ride38 and doors38 probes (Harness.N38.Ride.cs)\n" +
   "        N38Hud(cmd, args, ref code); // NIGHT38 H: hud38 and juice38 probes (Harness.N38.Hud.cs)\n" +
   "        N38Foundry(cmd, args, ref code); // NIGHT38 F: level 1 and level 6 (Harness.N38.Foundry.cs)\n" +
   "        N38Iceline(cmd, args, ref code); // NIGHT38 S2: level 2 and stag38 (Harness.N38.Iceline.cs)\n" +
   "        N38Kiln(cmd, args, ref code); // NIGHT38 K3: level 3 (Harness.N38.Kiln.cs)\n" +
   "        N38Span(cmd, args, ref code); // NIGHT38 O2: level 4 (Harness.N38.Span.cs)\n" +
   "        N38Spire(cmd, args, ref code); // NIGHT38 C2: level 5 (Harness.N38.Spire.cs)\n"],
  ["    static partial void N37Crown(string cmd, string[] args, ref int code);\n",
   "    static partial void N37Crown(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT38: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N38Gates(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Ride(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Hud(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Foundry(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Iceline(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Kiln(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Span(string cmd, string[] args, ref int code);\n" +
   "    static partial void N38Spire(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o37 = 19; o37 <= 37; o37++) if (Array.IndexOf(args, \"-skygrindOff\" + o37) >= 0) N37Blocks.ForceOff = true; // NIGHT37: -skygrindOff37 and every older switch turn the NIGHT37 blocks off\n",
   "            for (int o37 = 19; o37 <= 37; o37++) if (Array.IndexOf(args, \"-skygrindOff\" + o37) >= 0) N37Blocks.ForceOff = true; // NIGHT37: -skygrindOff37 and every older switch turn the NIGHT37 blocks off\n" +
   "            for (int o38 = 19; o38 <= 38; o38++) if (Array.IndexOf(args, \"-skygrindOff\" + o38) >= 0) N38Blocks.ForceOff = true; // NIGHT38: -skygrindOff38 and every older switch turn the NIGHT38 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff37\" && ", "a != \"-skygrindOff37\" && a != \"-skygrindOff38\" && "],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N37Levels(cfg, res);\n", "            N37Levels(cfg, res);\n            N38Levels(cfg, res);\n"],
  ["        public static void N33Levels(GameConfig cfg, List<string> res)\n",
   "        /// <summary>NIGHT38 (the lead; each level builder owns its row): a level row on (design.n38.foundry38 / iceline38 / kiln38 / span38 /\n" +
   "        /// spire38 / finale38) swaps its `levelFrom` for its `levelTo` (after N37Levels: the order with NIGHT37's boss rows). A flow choice\n" +
   "        /// (the level a run loads is on its tape). Off with -skygrindOff38 / --off38 (N38Blocks.ForceOff) or with the row off: NIGHT37's order.</summary>\n" +
   "        public static void N38Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N38Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N38LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n38.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n38.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n38.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "        }\n\n" +
   "        static readonly string[] N38LevelRows = { \"foundry38\", \"iceline38\", \"kiln38\", \"span38\", \"spire38\", \"finale38\" };\n\n" +
   "        public static void N33Levels(GameConfig cfg, List<string> res)\n"],
]);

patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was;\n", "        bool was, was38;\n"],
  ["        public void Up() { was = N37Blocks.ForceOff; N37Blocks.ForceOff = true; }\n",
   "        public void Up() { was = N37Blocks.ForceOff; N37Blocks.ForceOff = true; was38 = N38Blocks.ForceOff; N38Blocks.ForceOff = true; } // NIGHT38: and every NIGHT38 block off (N38SharedTests)\n"],
  ["        public void Down() { N37Blocks.ForceOff = was; }\n",
   "        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; }\n"],
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(20));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(21), \"NIGHT38: the current format is 21 (N38SharedTests.AsFormat20 strips its value)\");\n"],
  ["        public static byte[] AsFormat19(byte[] b20)\n        {\n",
   "        public static byte[] AsFormat19(byte[] b20)\n        {\n            b20 = N38SharedTests.AsFormat20(b20); // NIGHT38: a format 21 tape first loses its NIGHT38 value\n"],
]);

console.log("NIGHT38 shared patch done. Next: the Editor compile (unity-loop.sh compile), CoreHarness golden, SimHarness fingerprint --off38.");

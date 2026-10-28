// NIGHT37 shared commit (the lead): hooks for N37Blocks in the sim (StartRun, the boss builders' Step hooks after N36BossStep, the
// fingerprint's off path), tape format 20 (the NIGHT37 value, a VarI), the --off37 / -skygrindOff37 switches, the four bosses' begins
// first in BossRoomBegin and their systems in BossRoomSystem, the walk-in flag (Boss36WalkIn includes boss37WalkInHere), Curriculum25
// .N37Levels (each boss row's levelFrom -> levelTo), the builders' pre-placed dispatch hooks (harness partials N37Kit / Wick / Stag /
// Loom / Crown, reel scenarios by owner K / W / S / O / C), the old tests' NIGHT37 pins (N36SharedTests' format).
// Run once: node Tools/night/n37-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N36StartRun(); // NIGHT36: the run's NIGHT36 blocks (a tape's, or off with --off36 and older), the same way (SurvivalSim.N36.cs)\n",
   I + "N36StartRun(); // NIGHT36: the run's NIGHT36 blocks (a tape's, or off with --off36 and older), the same way (SurvivalSim.N36.cs)\n" +
   I + "N37StartRun(); // NIGHT37: the run's NIGHT37 blocks (a tape's, or off with --off37 and older), the same way (SurvivalSim.N37.cs)\n"],
  [I + "N36BossStep(step); // NIGHT36 B: walkin36, boss36 (SurvivalSim.Boss36.cs)\n",
   I + "N36BossStep(step); // NIGHT36 B: walkin36, boss36 (SurvivalSim.Boss36.cs)\n" +
   I + "N37Step(step); // NIGHT37 W / S / O / C: THE WICK, THE STAG, THE LOOM, THE CROWN (SurvivalSim.N37.cs)\n"],
  [I + "if (!FingerprintBaseline.N36OnPath) n36Override = N36Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N36OnPath) n36Override = N36Blocks.Off;\n" +
   I + "N37Blocks? n37Saved = n37Override; // NIGHT37: and every NIGHT37 block off until the freeze (FingerprintBaseline.N37OnPath)\n" +
   I + "if (!FingerprintBaseline.N37OnPath) n37Override = N37Blocks.Off;\n"],
  ["                n36Override = n36Saved;\n", "                n36Override = n36Saved;\n                n37Override = n37Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N36Init(); // NIGHT36: the NIGHT36 design blocks, read once (SurvivalSim.N36.cs)\n",
   I + "N36Init(); // NIGHT36: the NIGHT36 design blocks, read once (SurvivalSim.N36.cs)\n" + I + "N37Init(); // NIGHT37: the NIGHT37 design blocks, read once (SurvivalSim.N37.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Boss25.cs", [
  [I + "bool n36Handled = false, n36Ok = false;\n",
   I + "if (N37BossBegin(id, ref spawn, ref yawDeg, out bool n37Ok)) return n37Ok; // NIGHT37 W / S / O / C: the four bosses own their ids (SurvivalSim.N37.cs)\n" +
   I + "bool n36Handled = false, n36Ok = false;\n"],
  [I + "N36BossSystem(id, ref b36); // NIGHT36 B: boss36's boss for this id while it owns the fight (SurvivalSim.Boss36.cs)\n",
   I + "N36BossSystem(id, ref b36); // NIGHT36 B: boss36's boss for this id while it owns the fight (SurvivalSim.Boss36.cs)\n" +
   I + "if (b36 == null) b36 = N37BossSystem(id); // NIGHT37 W / S / O / C (SurvivalSim.N37.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.N36.cs", [
  ["        public bool Boss36WalkIn => n36.walkin36 && boss36WalkInHere;\n",
   "        public bool Boss36WalkIn => (n36.walkin36 && boss36WalkInHere) || boss37WalkInHere; // NIGHT37: a NIGHT37 boss on its level fights where the player walked in too (SurvivalSim.N37.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N36OnPath = false;\n",
   "        public const bool N36OnPath = false;\n\n        /// <summary>NIGHT37: false unless the freeze records a run with a sim-changing NIGHT37 default on; until then the composed\n        /// fingerprint runs with every NIGHT37 block off.</summary>\n        public const bool N37OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 19; // NIGHT36:",
   "        public const int FormatVersion = 20; // NIGHT37: 20 adds the NIGHT37 blocks as a VarI (formats 1-19: off); NIGHT36:"],
  ["        public int n36;\n",
   "        public int n36;\n\n        /// <summary>NIGHT37: the NIGHT37 design blocks the run read (N37Blocks.Bits; formats 1-19: 0, every block off, NIGHT36's game).</summary>\n        public int n37;\n"],
  ["            w.VarI(n36); // format 19: the NIGHT36 blocks (nine bits)\n",
   "            w.VarI(n36); // format 19: the NIGHT36 blocks (nine bits)\n            w.VarI(n37); // format 20: the NIGHT37 blocks\n"],
  ["            t.n36 = version >= 19 ? r.VarI() : 0; // formats 1-18: recorded on NIGHT35's game (or before), so every NIGHT36 block OFF (explicitly)\n",
   "            t.n36 = version >= 19 ? r.VarI() : 0; // formats 1-18: recorded on NIGHT35's game (or before), so every NIGHT36 block OFF (explicitly)\n" +
   "            t.n37 = version >= 20 ? r.VarI() : 0; // formats 1-19: recorded on NIGHT36's game (or before), so every NIGHT37 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n36 = sim.n36.Bits; // NIGHT36: the NIGHT36 blocks the run read\n",
   "            tape.n36 = sim.n36.Bits; // NIGHT36: the NIGHT36 blocks the run read\n            tape.n37 = sim.n37.Bits; // NIGHT37: the NIGHT37 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n36Override = N36Blocks.FromBits(tape.n36); // NIGHT36: the NIGHT36 blocks the run read (formats 1-18: all off, NIGHT35's game)\n",
   "            sim.n36Override = N36Blocks.FromBits(tape.n36); // NIGHT36: the NIGHT36 blocks the run read (formats 1-18: all off, NIGHT35's game)\n" +
   "            sim.n37Override = N37Blocks.FromBits(tape.n37); // NIGHT37: the NIGHT37 blocks the run read (formats 1-19: all off, NIGHT36's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N36OnPath) sim.n36Override = N36Blocks.Off;\n",
   "            if (!FingerprintBaseline.N36OnPath) sim.n36Override = N36Blocks.Off;\n            N37Blocks? n37Saved = sim.n37Override; // NIGHT37: and every NIGHT37 block off until the freeze (N37OnPath)\n            if (!FingerprintBaseline.N37OnPath) sim.n37Override = N37Blocks.Off;\n"],
  ["                sim.n36Override = n36Saved;\n", "                sim.n36Override = n36Saved;\n                sim.n37Override = n37Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N36Blocks.ForceOff = off36;\n",
   "        N36Blocks.ForceOff = off36;\n        // NIGHT37: --off37 runs every command with every NIGHT37 design block off (NIGHT36's game); every older switch turns them off as well\n        bool off37 = off36 || Array.IndexOf(args, \"--off37\") >= 0;\n        N37Blocks.ForceOff = off37;\n"],
  ["                if (off36) sim.n36Override = N36Blocks.Off; // NIGHT36: the same\n",
   "                if (off36) sim.n36Override = N36Blocks.Off; // NIGHT36: the same\n                if (off37) sim.n37Override = N37Blocks.Off; // NIGHT37: the same\n"],
  ["        N36Boss(cmd, args, ref code); // NIGHT36 B: walkin36, boss36, THE LANTERN (Harness.N36.Boss.cs)\n",
   "        N36Boss(cmd, args, ref code); // NIGHT36 B: walkin36, boss36, THE LANTERN (Harness.N36.Boss.cs)\n" +
   "        N37Kit(cmd, args, ref code); // NIGHT37 K: the Boss37 brain (Harness.N37.Kit.cs)\n" +
   "        N37Wick(cmd, args, ref code); // NIGHT37 W: THE WICK (Harness.N37.Wick.cs)\n" +
   "        N37Stag(cmd, args, ref code); // NIGHT37 S: THE STAG (Harness.N37.Stag.cs)\n" +
   "        N37Loom(cmd, args, ref code); // NIGHT37 O: THE LOOM (Harness.N37.Loom.cs)\n" +
   "        N37Crown(cmd, args, ref code); // NIGHT37 C: THE CROWN (Harness.N37.Crown.cs)\n"],
  ["    static partial void N36Boss(string cmd, string[] args, ref int code);\n",
   "    static partial void N36Boss(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT37: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N37Kit(string cmd, string[] args, ref int code);\n" +
   "    static partial void N37Wick(string cmd, string[] args, ref int code);\n" +
   "    static partial void N37Stag(string cmd, string[] args, ref int code);\n" +
   "    static partial void N37Loom(string cmd, string[] args, ref int code);\n" +
   "    static partial void N37Crown(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N36Blocks.ForceOff = true; // NIGHT36: -skygrindOff36 and every older switch turn the NIGHT36 blocks off\n",
   "N36Blocks.ForceOff = true; // NIGHT36: -skygrindOff36 and every older switch turn the NIGHT36 blocks off\n            for (int o37 = 19; o37 <= 37; o37++) if (Array.IndexOf(args, \"-skygrindOff\" + o37) >= 0) N37Blocks.ForceOff = true; // NIGHT37: -skygrindOff37 and every older switch turn the NIGHT37 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff36\" && ", "a != \"-skygrindOff36\" && a != \"-skygrindOff37\" && "],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N36Levels(cfg, res);\n", "            N36Levels(cfg, res);\n            N37Levels(cfg, res);\n"],
  ["        public static void N33Levels(GameConfig cfg, List<string> res)\n",
   "        /// <summary>NIGHT37 (the lead; each boss builder owns its row): a boss row on (design.n37.wick37 / stag37 / loom37 / crown37) swaps\n" +
   "        /// its `levelFrom` for its `levelTo` (after N36Levels: the level with the new boss). A flow choice (the level a run loads is on\n" +
   "        /// its tape). Off with -skygrindOff37 / --off37 (N37Blocks.ForceOff) or with the row off: NIGHT36's order.</summary>\n" +
   "        public static void N37Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N37Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N37LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n37.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n37.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n37.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "        }\n\n" +
   "        static readonly string[] N37LevelRows = { \"wick37\", \"stag37\", \"loom37\", \"crown37\" };\n\n" +
   "        public static void N33Levels(GameConfig cfg, List<string> res)\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| IsPhaseN35(s) || IsPhaseN36(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35, NIGHT36: below\n",
   "|| IsPhaseN35(s) || IsPhaseN36(s) || IsPhaseN37(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35, NIGHT36, NIGHT37: below\n\n" +
   "        // NIGHT37: the reel scenarios by owner; each owner's FeelReel.N37.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN37K = { \"verbs37\" };\n" +
   "        public static readonly string[] PhaseN37W = { \"wick37\" };\n" +
   "        public static readonly string[] PhaseN37S = { \"stag37\" };\n" +
   "        public static readonly string[] PhaseN37O = { \"loom37\" };\n" +
   "        public static readonly string[] PhaseN37C = { \"crown37\" };\n" +
   "        static bool IsPhaseN37(string s) => Array.IndexOf(PhaseN37K, s) >= 0 || Array.IndexOf(PhaseN37W, s) >= 0 || Array.IndexOf(PhaseN37S, s) >= 0 || Array.IndexOf(PhaseN37O, s) >= 0 || Array.IndexOf(PhaseN37C, s) >= 0;\n" +
   "        static partial void N37KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N37WickBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N37StagBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N37LoomBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N37CrownBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN36B, s) >= 0) N36BossBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN36B, s) >= 0) N36BossBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN37K, s) >= 0) N37KitBody(s, g, dir, res, offs, pre, ref body); // NIGHT37: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN37W, s) >= 0) N37WickBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN37S, s) >= 0) N37StagBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN37O, s) >= 0) N37LoomBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN37C, s) >= 0) N37CrownBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

patch("Assets/Tests/EditMode/Game/N36SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(19));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT37: the current format is 20; the n36 value is read below\n"],
  ["            if (BitConverter.ToInt32(b19, RunTape.Magic.Length) < 19) return b19;\n",
   "            b19 = N37SharedTests.AsFormat19(b19); // NIGHT37: a format 20 tape first loses its NIGHT37 value\n            if (BitConverter.ToInt32(b19, RunTape.Magic.Length) < 19) return b19;\n"],
]);

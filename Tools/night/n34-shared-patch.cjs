// NIGHT34 shared commit (the lead): hooks for N34Blocks in the sim, the split bend's WORLD calls wrapped in `if (!n34HoldWorld)` in
// place (nothing reordered), tape format 17 (the NIGHT34 byte), the --off34 / -skygrindOff34 switches, the test chain, the loop's
// Bend34Share line, Curriculum25.N34Levels (design.n34.lines34.levelSwap), and the builders' pre-placed dispatch hooks (harness partials
// N34Bend / N34Lines / N34Flow, reel scenarios by owner B / H / A / M).
// Run once: node Tools/night/n34-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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

// the WORLD calls of Step (SurvivalSim.N34.cs header): each prefixed in place
const I = "            ";
const world = [
  "core?.Step(mover);",
  "N33DevicesMove(step);",
  "N20RoutesStep(step);",
  "projectiles.Step(step);",
  "husks.Step(players, step);",
  "N19EnemiesStep(step);",
  "N20DirectorStep(step);",
  "N21DeadlineStep(step);",
  "N21StampStep(step);",
  "N21GrindStep(step);",
  "N22DirectedStep(step);",
  "N22DuskStep(step);",
  "N24CampaignStep(step);",
  "N24WyrmStep(step);",
  "N26BossStep(step);",
  "N26LevelStep(step);",
  "N27BossStep(step);",
  "N27LevelStep(step);",
  "N29SystemsStep(step);",
  "N29BossStep(step);",
  "N30BossStep(step);",
  "N31BossStep(step);",
  "N32BalanceStep(step);",
  "N32BossStep(step);",
  "N32ArenaStep(step);",
  "N32SpanStep(step);",
  "N32FinaleStep(step);",
  "N32CrownStep(step);",
  "N33AiStep(step);",
  "N33SpawnsStep(step);",
  "N33BossStep(step);",
  "N25WyrmStep(step);",
  "N25LevelStep(step);",
  "brain.Separate(husks);",
  "N33AfterCrowd(step);",
  "traps.Step(step);",
  "StepMode(step);",
];
const runPairs = world.map((c) => ["\n" + I + c, "\n" + I + "if (!n34HoldWorld) " + c]);
runPairs.push(
  ["\n" + I + "if (run.Live) brain.Step(step, players, run.round, director.Hunted);", "\n" + I + "if (!n34HoldWorld && run.Live) brain.Step(step, players, run.round, director.Hunted);"],
  ["\n" + I + "N33DevicesStep(step);\n",
   "\n" + I + "if (!n34HoldWorld) N33DevicesStep(step);\n" + I + "N34BendStep(step); // NIGHT34 B: every step (the player's clock), after this step's latches (SurvivalSim.Bend34.cs)\n"],
  [I + "simStep++;\n", I + "simStep++;\n" + I + "N34BendBeginStep(); // NIGHT34 B: sets n34HoldWorld for the split bend (SurvivalSim.N34.cs: the world calls below skip on a held step)\n"],
  [I + "N33StartRun(); // NIGHT33: the run's NIGHT33 blocks (a tape's, or off with --off33 and older), the same way (SurvivalSim.N33.cs)\n",
   I + "N33StartRun(); // NIGHT33: the run's NIGHT33 blocks (a tape's, or off with --off33 and older), the same way (SurvivalSim.N33.cs)\n" + I + "N34StartRun(); // NIGHT34: the run's NIGHT34 blocks (a tape's, or off with --off34 and older), the same way (SurvivalSim.N34.cs)\n"],
  [I + "shop33Owned = bank33 = 0;\n",
   I + "shop33Owned = bank33 = 0;\n" + I + "N34Blocks? n34Saved = n34Override; // NIGHT34: and every NIGHT34 block off until the freeze (FingerprintBaseline.N34OnPath)\n" + I + "if (!FingerprintBaseline.N34OnPath) n34Override = N34Blocks.Off;\n"],
  ["                bank33 = bank33Saved;\n", "                bank33 = bank33Saved;\n                n34Override = n34Saved;\n"],
);
patch("Assets/Scripts/Game/SurvivalSim.Run.cs", runPairs);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N33Init(); // NIGHT33: the NIGHT33 design blocks, read once (SurvivalSim.N33.cs)\n",
   I + "N33Init(); // NIGHT33: the NIGHT33 design blocks, read once (SurvivalSim.N33.cs)\n" + I + "N34Init(); // NIGHT34: the NIGHT34 design blocks, read once (SurvivalSim.N34.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N33OnPath = false;\n",
   "        public const bool N33OnPath = false;\n\n        /// <summary>NIGHT34: false unless the freeze records a run with a sim-changing NIGHT34 default on; until then the composed\n        /// fingerprint runs with every NIGHT34 block off.</summary>\n        public const bool N34OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 16; // NIGHT33:",
   "        public const int FormatVersion = 17; // NIGHT34: 17 adds the NIGHT34 blocks (formats 1-16: off); NIGHT33:"],
  ["        public int bank33;\n",
   "        public int bank33;\n\n        /// <summary>NIGHT34: the NIGHT34 design blocks the run read (N34Blocks.Bits; formats 1-16: 0, every block off, NIGHT33's game).</summary>\n        public int n34;\n"],
  ["            w.VarI(bank33); // ... and the banked points\n",
   "            w.VarI(bank33); // ... and the banked points\n            w.Byte((byte)n34); // format 17: the NIGHT34 blocks\n"],
  ["                t.bank33 = r.VarI();\n            }\n",
   "                t.bank33 = r.VarI();\n            }\n            t.n34 = version >= 17 ? r.Byte() : 0; // formats 1-16: recorded on NIGHT33's game (or before), so every NIGHT34 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.bank33 = sim.bank33;\n", "            tape.bank33 = sim.bank33;\n            tape.n34 = sim.n34.Bits; // NIGHT34: the NIGHT34 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.bank33 = tape.bank33;\n", "            sim.bank33 = tape.bank33;\n            sim.n34Override = N34Blocks.FromBits(tape.n34); // NIGHT34: the NIGHT34 blocks the run read (formats 1-16: all off, NIGHT33's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            sim.shop33Owned = sim.bank33 = 0;\n",
   "            sim.shop33Owned = sim.bank33 = 0;\n            N34Blocks? n34Saved = sim.n34Override; // NIGHT34: and every NIGHT34 block off until the freeze (N34OnPath)\n            if (!FingerprintBaseline.N34OnPath) sim.n34Override = N34Blocks.Off;\n"],
  ["                sim.bank33 = bank33Saved;\n", "                sim.bank33 = bank33Saved;\n                sim.n34Override = n34Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N33Blocks.ForceOff = off33;\n",
   "        N33Blocks.ForceOff = off33;\n        // NIGHT34: --off34 runs every command with every NIGHT34 design block off (NIGHT33's game); every older switch turns them off as well\n        bool off34 = off33 || Array.IndexOf(args, \"--off34\") >= 0;\n        N34Blocks.ForceOff = off34;\n"],
  ["                if (off33) sim.n33Override = N33Blocks.Off; // NIGHT33: the same\n",
   "                if (off33) sim.n33Override = N33Blocks.Off; // NIGHT33: the same\n                if (off34) sim.n34Override = N34Blocks.Off; // NIGHT34: the same\n"],
  ["        N33Shop(cmd, args, ref code); // NIGHT33 P: shop33 (the typical earn, prices, upgrades on the tape) (Harness.N33.Shop.cs)\n",
   "        N33Shop(cmd, args, ref code); // NIGHT33 P: shop33 (the typical earn, prices, upgrades on the tape) (Harness.N33.Shop.cs)\n" +
   "        N34Bend(cmd, args, ref code); // NIGHT34 B: bend34 (the short / full bend, the meter, the instant swap, the split's world steps) (Harness.N34.Bend.cs)\n" +
   "        N34Lines(cmd, args, ref code); // NIGHT34 A: lines34 / calm34 (movement lines per fight room, the prop count) (Harness.N34.Lines.cs)\n" +
   "        N34Flow(cmd, args, ref code); // NIGHT34 M: flow34, reach34, latency34 (Harness.N34.Flow.cs)\n"],
  ["    static partial void N33Shop(string cmd, string[] args, ref int code);\n",
   "    static partial void N33Shop(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT34: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N34Bend(string cmd, string[] args, ref int code);\n" +
   "    static partial void N34Lines(string cmd, string[] args, ref int code);\n" +
   "    static partial void N34Flow(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N33Blocks.ForceOff = true; // NIGHT33: -skygrindOff33 and every older switch turn the NIGHT33 blocks off\n",
   "N33Blocks.ForceOff = true; // NIGHT33: -skygrindOff33 and every older switch turn the NIGHT33 blocks off\n            for (int o34 = 19; o34 <= 34; o34++) if (Array.IndexOf(args, \"-skygrindOff\" + o34) >= 0) N34Blocks.ForceOff = true; // NIGHT34: -skygrindOff34 and every older switch turn the NIGHT34 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff33\" && ", "a != \"-skygrindOff33\" && a != \"-skygrindOff34\" && "],
]);

patch("Assets/Scripts/Game/SurvivalGame.SlowAim27.cs", [
  ["share = Math.Min(share, sim.Lantern29SlowScale);\n",
   "share = Math.Min(share, sim.Lantern29SlowScale);\n            // NIGHT34 B (design.n34.bend34, the global path): the grapple BEND's world share (the sim's own eased value, 1 outside a bend); the slower wins\n            if (flow == Flow.Playing && impactFrames != null && impactFrames.rules.slowMoOn) share = Math.Min(share, sim.Bend34Share);\n"],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N33Levels(cfg, res);\n", "            N33Levels(cfg, res);\n            N34Levels(cfg, res);\n"],
  ["        public static void N33Levels(GameConfig cfg, List<string> res)\n",
   "        /// <summary>NIGHT34 (the lead): design.n34.lines34 (on) swaps each level id in its `levelSwap` map ({old id: new id}) for its\n" +
   "        /// rebuilt movement-line n34 file (builder A). Runs after N33Levels, so it maps NIGHT33's ids; a level not in the map keeps its n33\n" +
   "        /// file. A flow choice (the level a run loads is on its tape). Off with -skygrindOff34 / --off34 (N34Blocks.ForceOff) or with the row off.</summary>\n" +
   "        public static void N34Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N34Blocks.ForceOff || res.Count == 0) return;\n" +
   "            if (!cfg.Design(\"n34.lines34.enabled\") || !(At(cfg, \"design.n34.lines34.levelSwap\") is Dictionary<string, object> map)) return;\n" +
   "            for (int i = 0; i < res.Count; i++)\n" +
   "                if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;\n" +
   "        }\n\n" +
   "        public static void N33Levels(GameConfig cfg, List<string> res)\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| IsPhaseN32(s) || IsPhaseN33(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33: below\n",
   "|| IsPhaseN32(s) || IsPhaseN33(s) || IsPhaseN34(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34: below\n\n" +
   "        // NIGHT34: the reel scenarios by owner; each owner's FeelReel.N34.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN34B = { \"bend34\" };\n" +
   "        public static readonly string[] PhaseN34H = { \"hud34\" };\n" +
   "        public static readonly string[] PhaseN34A = { \"lines34\" };\n" +
   "        public static readonly string[] PhaseN34M = { \"flow34\" };\n" +
   "        static bool IsPhaseN34(string s) => Array.IndexOf(PhaseN34B, s) >= 0 || Array.IndexOf(PhaseN34H, s) >= 0 || Array.IndexOf(PhaseN34A, s) >= 0 || Array.IndexOf(PhaseN34M, s) >= 0;\n" +
   "        static partial void N34BendBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N34HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N34LinesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N34FlowBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN33P, s) >= 0) N33ShopBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN33P, s) >= 0) N33ShopBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN34B, s) >= 0) N34BendBody(s, g, dir, res, offs, pre, ref body); // NIGHT34: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN34H, s) >= 0) N34HudBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN34A, s) >= 0) N34LinesBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN34M, s) >= 0) N34FlowBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

patch("Assets/Tests/EditMode/Game/N33SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(16));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT34: the current format is 17; the n33 bytes are read below\n"],
  ["            if (BitConverter.ToInt32(b16, RunTape.Magic.Length) < 16) return b16;\n",
   "            b16 = N34SharedTests.AsFormat16(b16); // NIGHT34: a format 17 tape first loses its NIGHT34 byte\n            if (BitConverter.ToInt32(b16, RunTape.Magic.Length) < 16) return b16;\n"],
]);

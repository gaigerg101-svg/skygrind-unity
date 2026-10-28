// NIGHT35 shared commit (the lead): hooks for N35Blocks in the sim (StartRun, the builders' Step hooks after N34BendStep, the fingerprint's
// off path), tape format 18 (the NIGHT35 byte), the --off35 / -skygrindOff35 switches, the grapple35 gate at the auto-own
// (SurvivalSim.Systems30.cs N30SystemsStep), Curriculum25.N35Levels (design.n35.campaign6), the level select's six slots, the builders'
// pre-placed dispatch hooks (harness partials N35Weapons / Teach / Move / Look / Levels, reel scenarios by owner W / T / M / V / A), the
// old tests' NIGHT35 pins (N34SharedTests' format, Lesson25Tests' NIGHT34 order with N35 forced off).
// Run once: node Tools/night/n35-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N34StartRun(); // NIGHT34: the run's NIGHT34 blocks (a tape's, or off with --off34 and older), the same way (SurvivalSim.N34.cs)\n",
   I + "N34StartRun(); // NIGHT34: the run's NIGHT34 blocks (a tape's, or off with --off34 and older), the same way (SurvivalSim.N34.cs)\n" +
   I + "N35StartRun(); // NIGHT35: the run's NIGHT35 blocks (a tape's, or off with --off35 and older), the same way (SurvivalSim.N35.cs)\n"],
  [I + "N34BendStep(step); // NIGHT34 B: every step (the player's clock), after this step's latches (SurvivalSim.Bend34.cs)\n",
   I + "N34BendStep(step); // NIGHT34 B: every step (the player's clock), after this step's latches (SurvivalSim.Bend34.cs)\n" +
   I + "N35WeaponsStep(step); // NIGHT35 W: charge35 (SurvivalSim.Weapons35.cs); the player's side\n" +
   I + "N35MoveStep(step); // NIGHT35 M: wallride35 / grapple35 (SurvivalSim.Move35.cs)\n" +
   I + "N35TeachStep(step); // NIGHT35 T: teach35 (SurvivalSim.Teach35.cs)\n"],
  [I + "if (!FingerprintBaseline.N34OnPath) n34Override = N34Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N34OnPath) n34Override = N34Blocks.Off;\n" +
   I + "N35Blocks? n35Saved = n35Override; // NIGHT35: and every NIGHT35 block off until the freeze (FingerprintBaseline.N35OnPath)\n" +
   I + "if (!FingerprintBaseline.N35OnPath) n35Override = N35Blocks.Off;\n"],
  ["                n34Override = n34Saved;\n", "                n34Override = n34Saved;\n                n35Override = n35Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N34Init(); // NIGHT34: the NIGHT34 design blocks, read once (SurvivalSim.N34.cs)\n",
   I + "N34Init(); // NIGHT34: the NIGHT34 design blocks, read once (SurvivalSim.N34.cs)\n" + I + "N35Init(); // NIGHT35: the NIGHT35 design blocks, read once (SurvivalSim.N35.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Systems30.cs", [
  ["if (n30.span && campaignOn && arcline21 != null && n21.arcline && map.grappleNodes.Count > 0 && !arcline21.Owned)\n",
   "if (n30.span && campaignOn && arcline21 != null && n21.arcline && map.grappleNodes.Count > 0 && !arcline21.Owned && Grapple35AutoOwn) // NIGHT35: grapple35 owns it from design.n35.grapple35.fromLevel only (SurvivalSim.N35.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N34OnPath = false;\n",
   "        public const bool N34OnPath = false;\n\n        /// <summary>NIGHT35: false unless the freeze records a run with a sim-changing NIGHT35 default on; until then the composed\n        /// fingerprint runs with every NIGHT35 block off.</summary>\n        public const bool N35OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 17; // NIGHT34:",
   "        public const int FormatVersion = 18; // NIGHT35: 18 adds the NIGHT35 blocks (formats 1-17: off); NIGHT34:"],
  ["        public int n34;\n",
   "        public int n34;\n\n        /// <summary>NIGHT35: the NIGHT35 design blocks the run read (N35Blocks.Bits; formats 1-17: 0, every block off, NIGHT34's game).</summary>\n        public int n35;\n"],
  ["            w.Byte((byte)n34); // format 17: the NIGHT34 blocks\n",
   "            w.Byte((byte)n34); // format 17: the NIGHT34 blocks\n            w.Byte((byte)n35); // format 18: the NIGHT35 blocks\n"],
  ["            t.n34 = version >= 17 ? r.Byte() : 0; // formats 1-16: recorded on NIGHT33's game (or before), so every NIGHT34 block OFF (explicitly)\n",
   "            t.n34 = version >= 17 ? r.Byte() : 0; // formats 1-16: recorded on NIGHT33's game (or before), so every NIGHT34 block OFF (explicitly)\n" +
   "            t.n35 = version >= 18 ? r.Byte() : 0; // formats 1-17: recorded on NIGHT34's game (or before), so every NIGHT35 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n34 = sim.n34.Bits; // NIGHT34: the NIGHT34 blocks the run read\n",
   "            tape.n34 = sim.n34.Bits; // NIGHT34: the NIGHT34 blocks the run read\n            tape.n35 = sim.n35.Bits; // NIGHT35: the NIGHT35 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n34Override = N34Blocks.FromBits(tape.n34); // NIGHT34: the NIGHT34 blocks the run read (formats 1-16: all off, NIGHT33's game)\n",
   "            sim.n34Override = N34Blocks.FromBits(tape.n34); // NIGHT34: the NIGHT34 blocks the run read (formats 1-16: all off, NIGHT33's game)\n" +
   "            sim.n35Override = N35Blocks.FromBits(tape.n35); // NIGHT35: the NIGHT35 blocks the run read (formats 1-17: all off, NIGHT34's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N34OnPath) sim.n34Override = N34Blocks.Off;\n",
   "            if (!FingerprintBaseline.N34OnPath) sim.n34Override = N34Blocks.Off;\n            N35Blocks? n35Saved = sim.n35Override; // NIGHT35: and every NIGHT35 block off until the freeze (N35OnPath)\n            if (!FingerprintBaseline.N35OnPath) sim.n35Override = N35Blocks.Off;\n"],
  ["                sim.n34Override = n34Saved;\n", "                sim.n34Override = n34Saved;\n                sim.n35Override = n35Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N34Blocks.ForceOff = off34;\n",
   "        N34Blocks.ForceOff = off34;\n        // NIGHT35: --off35 runs every command with every NIGHT35 design block off (NIGHT34's game); every older switch turns them off as well\n        bool off35 = off34 || Array.IndexOf(args, \"--off35\") >= 0;\n        N35Blocks.ForceOff = off35;\n"],
  ["                if (off34) sim.n34Override = N34Blocks.Off; // NIGHT34: the same\n",
   "                if (off34) sim.n34Override = N34Blocks.Off; // NIGHT34: the same\n                if (off35) sim.n35Override = N35Blocks.Off; // NIGHT35: the same\n"],
  ["        N34Flow(cmd, args, ref code); // NIGHT34 M: flow34, reach34, latency34 (Harness.N34.Flow.cs)\n",
   "        N34Flow(cmd, args, ref code); // NIGHT34 M: flow34, reach34, latency34 (Harness.N34.Flow.cs)\n" +
   "        N35Weapons(cmd, args, ref code); // NIGHT35 W: charge35 (Harness.N35.Weapons.cs)\n" +
   "        N35Teach(cmd, args, ref code); // NIGHT35 T: teach35 (Harness.N35.Teach.cs)\n" +
   "        N35Move(cmd, args, ref code); // NIGHT35 M: wallride35, grapple35, the bots (Harness.N35.Move.cs)\n" +
   "        N35Look(cmd, args, ref code); // NIGHT35 V: the PS2 pass's maths (Harness.N35.Look.cs)\n" +
   "        N35Levels(cmd, args, ref code); // NIGHT35 A: campaign6, the wall-ride rooms, the walk (Harness.N35.Levels.cs)\n"],
  ["    static partial void N34Flow(string cmd, string[] args, ref int code);\n",
   "    static partial void N34Flow(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT35: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N35Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N35Teach(string cmd, string[] args, ref int code);\n" +
   "    static partial void N35Move(string cmd, string[] args, ref int code);\n" +
   "    static partial void N35Look(string cmd, string[] args, ref int code);\n" +
   "    static partial void N35Levels(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N34Blocks.ForceOff = true; // NIGHT34: -skygrindOff34 and every older switch turn the NIGHT34 blocks off\n",
   "N34Blocks.ForceOff = true; // NIGHT34: -skygrindOff34 and every older switch turn the NIGHT34 blocks off\n            for (int o35 = 19; o35 <= 35; o35++) if (Array.IndexOf(args, \"-skygrindOff\" + o35) >= 0) N35Blocks.ForceOff = true; // NIGHT35: -skygrindOff35 and every older switch turn the NIGHT35 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff34\" && ", "a != \"-skygrindOff34\" && a != \"-skygrindOff35\" && "],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N34Levels(cfg, res);\n", "            N34Levels(cfg, res);\n            N35Levels(cfg, res);\n"],
  ["        public static void N33Levels(GameConfig cfg, List<string> res)\n",
   "        /// <summary>NIGHT35 (the lead; builder A owns the row): design.n35.campaign6 (on) makes the campaign six levels. It swaps each id in\n" +
   "        /// its `levelSwap` map ({old id: new id}, after N34Levels: A's n35 files), inserts `finaleId` (LEVEL 6) right after `after` (the\n" +
   "        /// swapped order's level 5), then drops every id after the finale: the legacy levels leave the order (their files stay in the tree).\n" +
   "        /// Without `after` in the order nothing is inserted or dropped. A flow choice (the level a run loads is on its tape). Off with\n" +
   "        /// -skygrindOff35 / --off35 (N35Blocks.ForceOff) or with the row off: NIGHT34's order.</summary>\n" +
   "        public static void N35Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N35Blocks.ForceOff || res.Count == 0 || !cfg.Design(\"n35.campaign6.enabled\")) return;\n" +
   "            if (At(cfg, \"design.n35.campaign6.levelSwap\") is Dictionary<string, object> map)\n" +
   "                for (int i = 0; i < res.Count; i++)\n" +
   "                    if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;\n" +
   "            int at = res.IndexOf(At(cfg, \"design.n35.campaign6.after\") as string ?? \"\");\n" +
   "            if (at < 0) return;\n" +
   "            string fin = At(cfg, \"design.n35.campaign6.finaleId\") as string;\n" +
   "            int keep = at + 1;\n" +
   "            if (!string.IsNullOrEmpty(fin))\n" +
   "            {\n" +
   "                res.Remove(fin);\n" +
   "                res.Insert(at + 1, fin);\n" +
   "                keep = at + 2;\n" +
   "            }\n" +
   "            if (res.Count > keep) res.RemoveRange(keep, res.Count - keep);\n" +
   "        }\n\n" +
   "        public static void N33Levels(GameConfig cfg, List<string> res)\n"],
]);

patch("Assets/Scripts/Game/UiLevelSelect26.cs", [
  ["            if (N32Fixes.ViewRowOn(cfg, \"level5\") && !N32Blocks.ForceOff && p.playable < UnityTuning.LevelSel32Level5Playable) p.playable = UnityTuning.LevelSel32Level5Playable;\n",
   "            if (N32Fixes.ViewRowOn(cfg, \"level5\") && !N32Blocks.ForceOff && p.playable < UnityTuning.LevelSel32Level5Playable) p.playable = UnityTuning.LevelSel32Level5Playable;\n" +
   "            // NIGHT35 A: LEVEL 6 (the finale) playable while design.n35.campaign6 is on (Curriculum25.N35Levels); -skygrindOff35 or the row off: the 5 above\n" +
   "            if (N35Fixes.ViewRowOn(cfg, \"campaign6\") && !N35Blocks.ForceOff && p.playable < UnityTuning.LevelSel35Playable) p.playable = UnityTuning.LevelSel35Playable;\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| IsPhaseN33(s) || IsPhaseN34(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34: below\n",
   "|| IsPhaseN33(s) || IsPhaseN34(s) || IsPhaseN35(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35: below\n\n" +
   "        // NIGHT35: the reel scenarios by owner; each owner's FeelReel.N35.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN35W = { \"charge35\", \"pickup35\" };\n" +
   "        public static readonly string[] PhaseN35T = { \"teach35\", \"verbs35\" };\n" +
   "        public static readonly string[] PhaseN35M = { \"wallride35\", \"grapple35\" };\n" +
   "        public static readonly string[] PhaseN35V = { \"look35\" };\n" +
   "        public static readonly string[] PhaseN35A = { \"levels35\" };\n" +
   "        static bool IsPhaseN35(string s) => Array.IndexOf(PhaseN35W, s) >= 0 || Array.IndexOf(PhaseN35T, s) >= 0 || Array.IndexOf(PhaseN35M, s) >= 0 || Array.IndexOf(PhaseN35V, s) >= 0 || Array.IndexOf(PhaseN35A, s) >= 0;\n" +
   "        static partial void N35WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N35TeachBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N35MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N35LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N35LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN34M, s) >= 0) N34FlowBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN34M, s) >= 0) N34FlowBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN35W, s) >= 0) N35WeaponsBody(s, g, dir, res, offs, pre, ref body); // NIGHT35: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN35T, s) >= 0) N35TeachBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN35M, s) >= 0) N35MoveBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN35V, s) >= 0) N35LookBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN35A, s) >= 0) N35LevelsBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

patch("Assets/Tests/EditMode/Game/N34SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(17));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT35: the current format is 18; the n34 bytes are read below\n"],
  ["            if (BitConverter.ToInt32(b17, RunTape.Magic.Length) < 17) return b17;\n",
   "            b17 = N35SharedTests.AsFormat17(b17); // NIGHT35: a format 18 tape first loses its NIGHT35 byte\n            if (BitConverter.ToInt32(b17, RunTape.Magic.Length) < 17) return b17;\n"],
]);

patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  ["            // NIGHT34: with lines34 on, levels 1-3 as their n34 files (span and spire keep their n33 files), in the same order\n" +
   "            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && c.Design(\"n33.kit33.enabled\") && c.Design(\"n34.lines34.enabled\"))\n" +
   "                Assert.That(",
   "            // NIGHT34: with lines34 on, levels 1-3 as their n34 files (span and spire keep their n33 files), in the same order (NIGHT35's campaign6 pinned off here)\n" +
   "            bool was35 = N35Blocks.ForceOff;\n" +
   "            N35Blocks.ForceOff = true; // NIGHT35: design.n35.campaign6 swaps levels 1-3 for the n35 files and adds LEVEL 6; NIGHT34's order is pinned with it forced off\n" +
   "            try\n" +
   "            {\n" +
   "            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && c.Design(\"n33.kit33.enabled\") && c.Design(\"n34.lines34.enabled\"))\n" +
   "                Assert.That("],
  ["\"NIGHT34: n34-foundry first, n34-iceline next, n33-span after n34-kiln, n33-spire after n33-span\");\n",
   "\"NIGHT34: n34-foundry first, n34-iceline next, n33-span after n34-kiln, n33-spire after n33-span\");\n" +
   "            }\n" +
   "            finally { N35Blocks.ForceOff = was35; }\n" +
   "            // NIGHT35: with campaign6 on, six levels: the swapped levels 1-5, then the finale, then nothing (the legacy levels leave the order)\n" +
   "            if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && !N35Blocks.ForceOff && c.Design(\"n35.campaign6.enabled\"))\n" +
   "            {\n" +
   "                var six = Curriculum25.Order(c, all30);\n" +
   "                Assert.That(six.Count == 6 && six[5] == (string)c.Get(\"design.n35.campaign6.finaleId\") && Curriculum25.Next(c, all30, six[5]) == null, Is.True, \"NIGHT35: six levels, the finale last: \" + string.Join(\",\", six));\n" +
   "            }\n"],
]);

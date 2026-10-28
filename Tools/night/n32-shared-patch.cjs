// NIGHT32 shared commit (the lead): hooks for N32Blocks in the sim, tape format 15, the --off32 / -skygrindOff32 switches, the test chain,
// and the builders' pre-placed dispatch hooks (harness partials N32Boss / N32Grapple / N32Arenas / N32Span / N32Finale, reel scenarios by owner).
// Run once: node Tools/night/n32-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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

patch("Assets/Scripts/Game/SurvivalSim.Run.cs", [
  ["            N31StartRun(); // NIGHT31: the run's NIGHT31 blocks (a tape's, or off with --off31 and older), the same way (SurvivalSim.N31.cs)\n",
   "            N31StartRun(); // NIGHT31: the run's NIGHT31 blocks (a tape's, or off with --off31 and older), the same way (SurvivalSim.N31.cs)\n            N32StartRun(); // NIGHT32: the run's NIGHT32 blocks (a tape's, or off with --off32 and older), the same way (SurvivalSim.N32.cs)\n"],
  ["            N31BossStep(step);\n",
   "            N31BossStep(step);\n            N32GrappleStep(step); // NIGHT32: the lead's hooks for the NIGHT32 rows (SurvivalSim.N32.cs; nothing without a body)\n            N32BalanceStep(step);\n            N32BossStep(step);\n            N32ArenaStep(step);\n            N32SpanStep(step);\n            N32FinaleStep(step);\n"],
  ["            N31GrappleFrame(ref frame); // NIGHT31 G: the grapple's tap / hold latch, before the Mover reads the frame (SurvivalSim.Grapple31.cs)\n",
   "            N31GrappleFrame(ref frame); // NIGHT31 G: the grapple's tap / hold latch, before the Mover reads the frame (SurvivalSim.Grapple31.cs)\n            N32GrappleFrame(ref frame); // NIGHT32 G: the grapple's charges, after the NIGHT31 latch (SurvivalSim.Grapple32.cs)\n"],
  ["            N31Blocks? n31Saved = n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (FingerprintBaseline.N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) n31Override = N31Blocks.Off;\n",
   "            N31Blocks? n31Saved = n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (FingerprintBaseline.N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) n31Override = N31Blocks.Off;\n            N32Blocks? n32Saved = n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (FingerprintBaseline.N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) n32Override = N32Blocks.Off;\n"],
  ["                n31Override = n31Saved;\n",
   "                n31Override = n31Saved;\n                n32Override = n32Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  ["            N31Init(); // NIGHT31: the NIGHT31 design blocks, read once (SurvivalSim.N31.cs)\n",
   "            N31Init(); // NIGHT31: the NIGHT31 design blocks, read once (SurvivalSim.N31.cs)\n            N32Init(); // NIGHT32: the NIGHT32 design blocks, read once (SurvivalSim.N32.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N31OnPath = false;\n",
   "        public const bool N31OnPath = false;\n\n        /// <summary>NIGHT32: false unless the freeze records a run with a sim-changing NIGHT32 default on; until then the composed\n        /// fingerprint runs with every NIGHT32 block off.</summary>\n        public const bool N32OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 14; // NIGHT31: 14 adds the NIGHT31 blocks (formats 1-13: off);",
   "        public const int FormatVersion = 15; // NIGHT32: 15 adds the NIGHT32 blocks (formats 1-14: off); NIGHT31: 14 adds the NIGHT31 blocks (formats 1-13: off);"],
  ["        public int n31;\n",
   "        public int n31;\n\n        /// <summary>NIGHT32: the NIGHT32 design blocks the run read (N32Blocks.Bits; formats 1-14: 0, every block off, NIGHT31's game).</summary>\n        public int n32;\n"],
  ["            w.Byte((byte)n31); // format 14: the NIGHT31 blocks\n",
   "            w.Byte((byte)n31); // format 14: the NIGHT31 blocks\n            w.Byte((byte)n32); // format 15: the NIGHT32 blocks\n"],
  ["            t.n31 = version >= 14 ? r.Byte() : 0; // formats 1-13: recorded on NIGHT30's game (or before), so every NIGHT31 block OFF (explicitly)\n",
   "            t.n31 = version >= 14 ? r.Byte() : 0; // formats 1-13: recorded on NIGHT30's game (or before), so every NIGHT31 block OFF (explicitly)\n            t.n32 = version >= 15 ? r.Byte() : 0; // formats 1-14: recorded on NIGHT31's game (or before), so every NIGHT32 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n31 = sim.n31.Bits; // NIGHT31: the NIGHT31 blocks the run read\n",
   "            tape.n31 = sim.n31.Bits; // NIGHT31: the NIGHT31 blocks the run read\n            tape.n32 = sim.n32.Bits; // NIGHT32: the NIGHT32 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n31Override = N31Blocks.FromBits(tape.n31); // NIGHT31: the NIGHT31 blocks the run read (formats 1-13: all off, NIGHT30's game)\n",
   "            sim.n31Override = N31Blocks.FromBits(tape.n31); // NIGHT31: the NIGHT31 blocks the run read (formats 1-13: all off, NIGHT30's game)\n            sim.n32Override = N32Blocks.FromBits(tape.n32); // NIGHT32: the NIGHT32 blocks the run read (formats 1-14: all off, NIGHT31's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            N31Blocks? n31Saved = sim.n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) sim.n31Override = N31Blocks.Off;\n",
   "            N31Blocks? n31Saved = sim.n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) sim.n31Override = N31Blocks.Off;\n            N32Blocks? n32Saved = sim.n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) sim.n32Override = N32Blocks.Off;\n"],
  ["                sim.n31Override = n31Saved;\n",
   "                sim.n31Override = n31Saved;\n                sim.n32Override = n32Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N31Blocks.ForceOff = off31;\n",
   "        N31Blocks.ForceOff = off31;\n        // NIGHT32: --off32 runs every command with every NIGHT32 design block off (NIGHT31's game); every older switch turns them off as well\n        bool off32 = off31 || Array.IndexOf(args, \"--off32\") >= 0;\n        N32Blocks.ForceOff = off32;\n"],
  ["                if (off31) sim.n31Override = N31Blocks.Off; // NIGHT31: the same\n",
   "                if (off31) sim.n31Override = N31Blocks.Off; // NIGHT31: the same\n                if (off32) sim.n32Override = N32Blocks.Off; // NIGHT32: the same\n"],
  ["        N31Levels(cmd, args, ref code); // NIGHT31 L: arena and weapon-room checks (Harness.N31.Levels.cs)\n",
   "        N31Levels(cmd, args, ref code); // NIGHT31 L: arena and weapon-room checks (Harness.N31.Levels.cs)\n" +
   "        N32Boss(cmd, args, ref code); // NIGHT32 B: boss32 sweeps (kill-time p10 / p50 / p90), camp32, boss5 (Harness.N32.Boss.cs)\n" +
   "        N32Grapple(cmd, args, ref code); // NIGHT32 G: grapple32 (charges, spam bot), balance32 (Harness.N32.Grapple.cs)\n" +
   "        N32Arenas(cmd, args, ref code); // NIGHT32 A: arenas32, ring32 (Harness.N32.Arenas.cs)\n" +
   "        N32Span(cmd, args, ref code); // NIGHT32 S: span32 (Harness.N32.Span.cs)\n" +
   "        N32Finale(cmd, args, ref code); // NIGHT32 F: level5 (Harness.N32.Finale.cs)\n"],
  ["    static partial void N31Levels(string cmd, string[] args, ref int code);\n",
   "    static partial void N31Levels(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT32: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N32Boss(string cmd, string[] args, ref int code);\n" +
   "    static partial void N32Grapple(string cmd, string[] args, ref int code);\n" +
   "    static partial void N32Arenas(string cmd, string[] args, ref int code);\n" +
   "    static partial void N32Span(string cmd, string[] args, ref int code);\n" +
   "    static partial void N32Finale(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["for (int o31 = 19; o31 <= 31; o31++) if (Array.IndexOf(args, \"-skygrindOff\" + o31) >= 0) N31Blocks.ForceOff = true; // NIGHT31: -skygrindOff31 and every older switch turn the NIGHT31 blocks off\n",
   "for (int o31 = 19; o31 <= 31; o31++) if (Array.IndexOf(args, \"-skygrindOff\" + o31) >= 0) N31Blocks.ForceOff = true; // NIGHT31: -skygrindOff31 and every older switch turn the NIGHT31 blocks off\n            for (int o32 = 19; o32 <= 32; o32++) if (Array.IndexOf(args, \"-skygrindOff\" + o32) >= 0) N32Blocks.ForceOff = true; // NIGHT32: -skygrindOff32 and every older switch turn the NIGHT32 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff31\" && ", "a != \"-skygrindOff31\" && a != \"-skygrindOff32\" && "],
]);

patch("Assets/Tests/EditMode/Game/N31SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(14));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT32: the current format is 15; the n31 byte is read below\n"],
  ["            if (BitConverter.ToInt32(b14, RunTape.Magic.Length) < 14) return b14;\n",
   "            b14 = N32SharedTests.AsFormat14(b14); // NIGHT32: a format 15 tape first loses its NIGHT32 byte\n            if (BitConverter.ToInt32(b14, RunTape.Magic.Length) < 14) return b14;\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s) || IsPhaseN31(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31: below\n",
   "        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s) || IsPhaseN31(s) || IsPhaseN32(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32: below\n\n" +
   "        // NIGHT32: the reel scenarios by owner; each owner's FeelReel.N32.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN32B = { \"bosses32\", \"camp32\", \"boss5\" };\n" +
   "        public static readonly string[] PhaseN32G = { \"grapple32\", \"balance32\", \"hud32\" };\n" +
   "        public static readonly string[] PhaseN32A = { \"arenas32\", \"ring32\" };\n" +
   "        public static readonly string[] PhaseN32S = { \"span32\" };\n" +
   "        public static readonly string[] PhaseN32F = { \"level5\" };\n" +
   "        static bool IsPhaseN32(string s) => Array.IndexOf(PhaseN32B, s) >= 0 || Array.IndexOf(PhaseN32G, s) >= 0 || Array.IndexOf(PhaseN32A, s) >= 0 || Array.IndexOf(PhaseN32S, s) >= 0 || Array.IndexOf(PhaseN32F, s) >= 0;\n" +
   "        static partial void N32BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N32GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N32ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N32SpanBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N32FinaleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN31L, s) >= 0) N31LevelsBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN31L, s) >= 0) N31LevelsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN32B, s) >= 0) N32BossesBody(s, g, dir, res, offs, pre, ref body); // NIGHT32: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN32G, s) >= 0) N32GrappleBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN32A, s) >= 0) N32ArenasBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN32S, s) >= 0) N32SpanBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN32F, s) >= 0) N32FinaleBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

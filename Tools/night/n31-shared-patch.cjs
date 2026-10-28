// NIGHT31 shared commit (the lead): hooks for N31Blocks in the sim, tape format 14, the --off31 / -skygrindOff31 switches, the test chain,
// and the builders' pre-placed dispatch hooks (harness partials N31Grapple / N31Weapons / N31Boss / N31Levels, reel scenarios by owner).
// Run once: node Tools/night/n31-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  ["            N30StartRun(); // NIGHT30: the run's NIGHT30 blocks (a tape's, or off with --off30 and older), the same way (SurvivalSim.N30.cs)\n",
   "            N30StartRun(); // NIGHT30: the run's NIGHT30 blocks (a tape's, or off with --off30 and older), the same way (SurvivalSim.N30.cs)\n            N31StartRun(); // NIGHT31: the run's NIGHT31 blocks (a tape's, or off with --off31 and older), the same way (SurvivalSim.N31.cs)\n"],
  ["            N30BossStep(step);\n",
   "            N30BossStep(step);\n            N31GrappleStep(step); // NIGHT31: the lead's hooks for the NIGHT31 rows (SurvivalSim.N31.cs; nothing without a body)\n            N31WeaponsStep(step);\n            N31BossStep(step);\n"],
  ["            if (!FingerprintBaseline.N30OnPath) n30Override = N30Blocks.Off;\n",
   "            if (!FingerprintBaseline.N30OnPath) n30Override = N30Blocks.Off;\n            N31Blocks? n31Saved = n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (FingerprintBaseline.N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) n31Override = N31Blocks.Off;\n"],
  ["                n30Override = n30Saved;\n",
   "                n30Override = n30Saved;\n                n31Override = n31Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  ["            N30Init(); // NIGHT30: the NIGHT30 design blocks, read once (SurvivalSim.N30.cs)\n",
   "            N30Init(); // NIGHT30: the NIGHT30 design blocks, read once (SurvivalSim.N30.cs)\n            N31Init(); // NIGHT31: the NIGHT31 design blocks, read once (SurvivalSim.N31.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N30OnPath = false;\n",
   "        public const bool N30OnPath = false;\n\n        /// <summary>NIGHT31: false unless the freeze records a run with a sim-changing NIGHT31 default on; until then the composed\n        /// fingerprint runs with every NIGHT31 block off.</summary>\n        public const bool N31OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 13; // NIGHT30: 13 adds the NIGHT30 blocks (formats 1-12: off);",
   "        public const int FormatVersion = 14; // NIGHT31: 14 adds the NIGHT31 blocks (formats 1-13: off); NIGHT30: 13 adds the NIGHT30 blocks (formats 1-12: off);"],
  ["        public int n30;\n",
   "        public int n30;\n\n        /// <summary>NIGHT31: the NIGHT31 design blocks the run read (N31Blocks.Bits; formats 1-13: 0, every block off, NIGHT30's game).</summary>\n        public int n31;\n"],
  ["            w.Byte((byte)n30); // format 13: the NIGHT30 blocks\n",
   "            w.Byte((byte)n30); // format 13: the NIGHT30 blocks\n            w.Byte((byte)n31); // format 14: the NIGHT31 blocks\n"],
  ["            t.n30 = version >= 13 ? r.Byte() : 0; // formats 1-12: recorded on NIGHT29's game (or before), so every NIGHT30 block OFF (explicitly)\n",
   "            t.n30 = version >= 13 ? r.Byte() : 0; // formats 1-12: recorded on NIGHT29's game (or before), so every NIGHT30 block OFF (explicitly)\n            t.n31 = version >= 14 ? r.Byte() : 0; // formats 1-13: recorded on NIGHT30's game (or before), so every NIGHT31 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n30 = sim.n30.Bits; // NIGHT30: the NIGHT30 blocks the run read\n",
   "            tape.n30 = sim.n30.Bits; // NIGHT30: the NIGHT30 blocks the run read\n            tape.n31 = sim.n31.Bits; // NIGHT31: the NIGHT31 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n30Override = N30Blocks.FromBits(tape.n30); // NIGHT30: the NIGHT30 blocks the run read (formats 1-12: all off, NIGHT29's game)\n",
   "            sim.n30Override = N30Blocks.FromBits(tape.n30); // NIGHT30: the NIGHT30 blocks the run read (formats 1-12: all off, NIGHT29's game)\n            sim.n31Override = N31Blocks.FromBits(tape.n31); // NIGHT31: the NIGHT31 blocks the run read (formats 1-13: all off, NIGHT30's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N30OnPath) sim.n30Override = N30Blocks.Off;\n",
   "            if (!FingerprintBaseline.N30OnPath) sim.n30Override = N30Blocks.Off;\n            N31Blocks? n31Saved = sim.n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (N31OnPath)\n            if (!FingerprintBaseline.N31OnPath) sim.n31Override = N31Blocks.Off;\n"],
  ["                sim.n30Override = n30Saved;\n",
   "                sim.n30Override = n30Saved;\n                sim.n31Override = n31Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N30Blocks.ForceOff = off30;\n",
   "        N30Blocks.ForceOff = off30;\n        // NIGHT31: --off31 runs every command with every NIGHT31 design block off (NIGHT30's game); every older switch turns them off as well\n        bool off31 = off30 || Array.IndexOf(args, \"--off31\") >= 0;\n        N31Blocks.ForceOff = off31;\n"],
  ["                if (off30) sim.n30Override = N30Blocks.Off; // NIGHT30: the same\n",
   "                if (off30) sim.n30Override = N30Blocks.Off; // NIGHT30: the same\n                if (off31) sim.n31Override = N31Blocks.Off; // NIGHT31: the same\n"],
  ["        N30View(cmd, args, ref code); // NIGHT30 V: quantise30 and other headless view checks (Harness.N30.View.cs)\n",
   "        N30View(cmd, args, ref code); // NIGHT30 V: quantise30 and other headless view checks (Harness.N30.View.cs)\n" +
   "        N31Grapple(cmd, args, ref code); // NIGHT31 G: grapple31 (pull speed, chains) ... (Harness.N31.Grapple.cs)\n" +
   "        N31Weapons(cmd, args, ref code); // NIGHT31 W: charge31, ltalways31, unlocks31 (Harness.N31.Weapons.cs)\n" +
   "        N31Boss(cmd, args, ref code); // NIGHT31 B: camp31 [lantern|stag|glacier|wyrm|loom], boss31 (Harness.N31.Boss.cs)\n" +
   "        N31Levels(cmd, args, ref code); // NIGHT31 L: arena and weapon-room checks (Harness.N31.Levels.cs)\n"],
  ["    static partial void N30View(string cmd, string[] args, ref int code);\n",
   "    static partial void N30View(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT31: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N31Grapple(string cmd, string[] args, ref int code);\n" +
   "    static partial void N31Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N31Boss(string cmd, string[] args, ref int code);\n" +
   "    static partial void N31Levels(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N30Blocks.ForceOff = true; // NIGHT30: -skygrindOff30 and every older switch turn the NIGHT30 blocks off\n",
   "N30Blocks.ForceOff = true; // NIGHT30: -skygrindOff30 and every older switch turn the NIGHT30 blocks off\n            for (int o31 = 19; o31 <= 31; o31++) if (Array.IndexOf(args, \"-skygrindOff\" + o31) >= 0) N31Blocks.ForceOff = true; // NIGHT31: -skygrindOff31 and every older switch turn the NIGHT31 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff30\" && ", "a != \"-skygrindOff30\" && a != \"-skygrindOff31\" && "],
]);

patch("Assets/Tests/EditMode/Game/N30SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(13));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT31: the current format is 14; the n30 byte is read below\n"],
  ["            if (BitConverter.ToInt32(b13, RunTape.Magic.Length) < 13) return b13;\n",
   "            b13 = N31SharedTests.AsFormat13(b13); // NIGHT31: a format 14 tape first loses its NIGHT31 byte\n            if (BitConverter.ToInt32(b13, RunTape.Magic.Length) < 13) return b13;\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30: below\n",
   "        static bool IsPhaseJ(string s) => Array.IndexOf(PhaseJ, s) >= 0 || IsPhaseN29(s) || IsPhaseN30(s) || IsPhaseN31(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31: below\n\n" +
   "        // NIGHT31: the reel scenarios by owner; each owner's FeelReel.N31.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN31G = { \"grapple31\", \"edge31\", \"spawn31\", \"ring31\" };\n" +
   "        public static readonly string[] PhaseN31W = { \"charge31\", \"lt31\", \"unlocks31\" };\n" +
   "        public static readonly string[] PhaseN31B = { \"camp31\", \"lantern31\", \"stag31\", \"wyrm31\", \"loom31\" };\n" +
   "        public static readonly string[] PhaseN31L = { \"arenas31\", \"rooms31\" };\n" +
   "        static bool IsPhaseN31(string s) => Array.IndexOf(PhaseN31G, s) >= 0 || Array.IndexOf(PhaseN31W, s) >= 0 || Array.IndexOf(PhaseN31B, s) >= 0 || Array.IndexOf(PhaseN31L, s) >= 0;\n" +
   "        static partial void N31GrappleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N31WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N31BossesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N31LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN30L, s) >= 0) N30LevelsBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN30L, s) >= 0) N30LevelsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN31G, s) >= 0) N31GrappleBody(s, g, dir, res, offs, pre, ref body); // NIGHT31: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN31W, s) >= 0) N31WeaponsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN31B, s) >= 0) N31BossesBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN31L, s) >= 0) N31LevelsBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

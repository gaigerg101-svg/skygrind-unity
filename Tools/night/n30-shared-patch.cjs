// NIGHT30 shared commit (the lead): hooks for N30Blocks in the sim, tape format 13, the --off30 / -skygrindOff30 switches, the test chain.
// Run once: node Tools/night/n30-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  ["            N29StartRun(); // NIGHT29: the run's NIGHT29 blocks (a tape's, or off with --off29 and older), the same way (SurvivalSim.N29.cs)\n",
   "            N29StartRun(); // NIGHT29: the run's NIGHT29 blocks (a tape's, or off with --off29 and older), the same way (SurvivalSim.N29.cs)\n            N30StartRun(); // NIGHT30: the run's NIGHT30 blocks (a tape's, or off with --off30 and older), the same way (SurvivalSim.N30.cs)\n"],
  ["            N29BossStep(step);\n",
   "            N29BossStep(step);\n            N30SystemsStep(step); // NIGHT30: the lead's hooks for the NIGHT30 rows (SurvivalSim.N30.cs; nothing without a body)\n            N30BossStep(step);\n"],
  ["            if (!FingerprintBaseline.N29OnPath) n29Override = N29Blocks.Off;\n",
   "            if (!FingerprintBaseline.N29OnPath) n29Override = N29Blocks.Off;\n            N30Blocks? n30Saved = n30Override; // NIGHT30: and every NIGHT30 block off until the freeze (FingerprintBaseline.N30OnPath)\n            if (!FingerprintBaseline.N30OnPath) n30Override = N30Blocks.Off;\n"],
  ["                n29Override = n29Saved;\n",
   "                n29Override = n29Saved;\n                n30Override = n30Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  ["            N29Init(); // NIGHT29: the NIGHT29 design blocks, read once (SurvivalSim.N29.cs)\n",
   "            N29Init(); // NIGHT29: the NIGHT29 design blocks, read once (SurvivalSim.N29.cs)\n            N30Init(); // NIGHT30: the NIGHT30 design blocks, read once (SurvivalSim.N30.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N29OnPath = false;\n",
   "        public const bool N29OnPath = false;\n\n        /// <summary>NIGHT30: false unless the freeze records a run with a sim-changing NIGHT30 default on; until then the composed\n        /// fingerprint runs with every NIGHT30 block off.</summary>\n        public const bool N30OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 12; // NIGHT29: 12 adds the NIGHT29 blocks (formats 1-11: off);",
   "        public const int FormatVersion = 13; // NIGHT30: 13 adds the NIGHT30 blocks (formats 1-12: off); NIGHT29: 12 adds the NIGHT29 blocks (formats 1-11: off);"],
  ["        public int n29;\n",
   "        public int n29;\n\n        /// <summary>NIGHT30: the NIGHT30 design blocks the run read (N30Blocks.Bits; formats 1-12: 0, every block off, NIGHT29's game).</summary>\n        public int n30;\n"],
  ["            w.Byte((byte)n29); // format 12: the NIGHT29 blocks\n",
   "            w.Byte((byte)n29); // format 12: the NIGHT29 blocks\n            w.Byte((byte)n30); // format 13: the NIGHT30 blocks\n"],
  ["            t.n29 = version >= 12 ? r.Byte() : 0; // formats 1-11: recorded on NIGHT28's game (or before), so every NIGHT29 block OFF (explicitly)\n",
   "            t.n29 = version >= 12 ? r.Byte() : 0; // formats 1-11: recorded on NIGHT28's game (or before), so every NIGHT29 block OFF (explicitly)\n            t.n30 = version >= 13 ? r.Byte() : 0; // formats 1-12: recorded on NIGHT29's game (or before), so every NIGHT30 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n29 = sim.n29.Bits; // NIGHT29: the NIGHT29 blocks the run read\n",
   "            tape.n29 = sim.n29.Bits; // NIGHT29: the NIGHT29 blocks the run read\n            tape.n30 = sim.n30.Bits; // NIGHT30: the NIGHT30 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n29Override = N29Blocks.FromBits(tape.n29); // NIGHT29: the NIGHT29 blocks the run read (formats 1-11: all off, NIGHT28's game)\n",
   "            sim.n29Override = N29Blocks.FromBits(tape.n29); // NIGHT29: the NIGHT29 blocks the run read (formats 1-11: all off, NIGHT28's game)\n            sim.n30Override = N30Blocks.FromBits(tape.n30); // NIGHT30: the NIGHT30 blocks the run read (formats 1-12: all off, NIGHT29's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N29OnPath) sim.n29Override = N29Blocks.Off;\n",
   "            if (!FingerprintBaseline.N29OnPath) sim.n29Override = N29Blocks.Off;\n            N30Blocks? n30Saved = sim.n30Override; // NIGHT30: and every NIGHT30 block off until the freeze (N30OnPath)\n            if (!FingerprintBaseline.N30OnPath) sim.n30Override = N30Blocks.Off;\n"],
  ["                sim.n29Override = n29Saved;\n",
   "                sim.n29Override = n29Saved;\n                sim.n30Override = n30Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N29Blocks.ForceOff = off29;\n",
   "        N29Blocks.ForceOff = off29;\n        // NIGHT30: --off30 runs every command with every NIGHT30 design block off (NIGHT29's game); every older switch turns them off as well\n        bool off30 = off29 || Array.IndexOf(args, \"--off30\") >= 0;\n        N30Blocks.ForceOff = off30;\n"],
  ["                if (off29) sim.n29Override = N29Blocks.Off; // NIGHT29: the same\n",
   "                if (off29) sim.n29Override = N29Blocks.Off; // NIGHT29: the same\n                if (off30) sim.n30Override = N30Blocks.Off; // NIGHT30: the same\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N29Blocks.ForceOff = true; // NIGHT29: -skygrindOff29 and every older switch turn the NIGHT29 blocks off\n",
   "N29Blocks.ForceOff = true; // NIGHT29: -skygrindOff29 and every older switch turn the NIGHT29 blocks off\n            for (int o30 = 19; o30 <= 30; o30++) if (Array.IndexOf(args, \"-skygrindOff\" + o30) >= 0) N30Blocks.ForceOff = true; // NIGHT30: -skygrindOff30 and every older switch turn the NIGHT30 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff29\" && ", "a != \"-skygrindOff29\" && a != \"-skygrindOff30\" && "],
]);

patch("Assets/Tests/EditMode/Game/N29SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(12));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT30: the current format is 13; the n29 byte is read below\n"],
  ["            if (BitConverter.ToInt32(b12, RunTape.Magic.Length) < 12) return b12;\n",
   "            b12 = N30SharedTests.AsFormat12(b12); // NIGHT30: a format 13 tape first loses its NIGHT30 byte\n            if (BitConverter.ToInt32(b12, RunTape.Magic.Length) < 12) return b12;\n"],
]);

// the batch tests script: P0 h (the XML landed at the repo root as `Logs$TAG-tests.xml`: `\$` inside double quotes kept the dollar literal)
patch("Tools/night/n27-batch-tests.sh", [
  ["-testResults \"C:\\Users\\gaige\\dev\\SKYGRIND-unity\\Logs\\$TAG-tests.xml\" -logFile \"C:\\Users\\gaige\\dev\\SKYGRIND-unity\\Logs\\$TAG-tests.log\"",
   "-testResults \"C:/Users/gaige/dev/SKYGRIND-unity/Logs/${TAG}-tests.xml\" -logFile \"C:/Users/gaige/dev/SKYGRIND-unity/Logs/${TAG}-tests.log\""],
]);

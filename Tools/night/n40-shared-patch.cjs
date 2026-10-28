// NIGHT40 shared commit (the lead): hooks for N40Blocks in the sim (StartRun, the step hook after N39Step, the fingerprint's off path),
// tape format 23 (the NIGHT40 value, a VarI after the NIGHT39 value), the --off40 / -skygrindOff40 switches (and -skygrindOff40 in
// N29Fixes.IsAutomationArg), the builders' pre-placed harness partials, the test chain (N37TestBaseline also forces NIGHT40 off; AsFormat22
// joins the format chain; the format pins move to 23), and NIGHT40's pre-placed hooks (L-021 / L-050): Curriculum25.N40Levels (the level
// rows' swaps and cut40's cut), Campaign24.waveReady40 (S1's first-wave hold), N31UnlocksStep's NIGHT40 start sets / awards / drops
// (SurvivalSim.N40.cs), and the n38 walk pilots answering for their n40 levels.
// Run once after n40-config-block.cjs: node Tools/night/n40-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N39StartRun(); // NIGHT39: the run's NIGHT39 blocks (a tape's, or off with --off39 and older), the same way (SurvivalSim.N39.cs)\n",
   I + "N39StartRun(); // NIGHT39: the run's NIGHT39 blocks (a tape's, or off with --off39 and older), the same way (SurvivalSim.N39.cs)\n" +
   I + "N40StartRun(); // NIGHT40: the run's NIGHT40 blocks (a tape's, or off with --off40 and older), the same way (SurvivalSim.N40.cs)\n"],
  [I + "N39Step(step); // NIGHT39 P1 / B1: p39, boss39 (SurvivalSim.N39.cs; nothing until a builder fills its hook)\n",
   I + "N39Step(step); // NIGHT39 P1 / B1: p39, boss39 (SurvivalSim.N39.cs; nothing until a builder fills its hook)\n" +
   I + "N40Step(step); // NIGHT40 S1 / B2 / B4 / B5 / E3 (SurvivalSim.N40.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N39OnPath) n39Override = N39Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N39OnPath) n39Override = N39Blocks.Off;\n" +
   I + "N40Blocks? n40Saved = n40Override; // NIGHT40: and every NIGHT40 block off until the freeze (FingerprintBaseline.N40OnPath)\n" +
   I + "if (!FingerprintBaseline.N40OnPath) n40Override = N40Blocks.Off;\n"],
  ["                n39Override = n39Saved;\n", "                n39Override = n39Saved;\n                n40Override = n40Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N39Init(); // NIGHT39: the NIGHT39 design blocks, read once (SurvivalSim.N39.cs)\n",
   I + "N39Init(); // NIGHT39: the NIGHT39 design blocks, read once (SurvivalSim.N39.cs)\n" + I + "N40Init(); // NIGHT40: the NIGHT40 design blocks and level rows, read once (SurvivalSim.N40.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Weapons31.cs", [
  ["                List<string> start = unlocks31Table.StartFor(campaignLevel.id);\n",
   "                List<string> start = N40UnlocksBegin(campaignLevel.id, unlocks31Table.StartFor(N40TableId(campaignLevel.id))); // NIGHT40: a level row's start set, loom40's drops (SurvivalSim.N40.cs; all off: the table's)\n"],
  ["            var (weapon, roomId) = unlocks31Table.RoomAward(campaignLevel.id);\n",
   "            N40RoomAwardsStep(); // NIGHT40: a level row's room awards (none with every NIGHT40 block off)\n" +
   "            var (weapon, roomId) = unlocks31Table.RoomAward(N40TableId(campaignLevel.id));\n" +
   "            if (weapon != null && N40Dropped(weapon)) return; // NIGHT40 loom40: the STITCHER is out of the campaign\n"],
]);

patch("Assets/Scripts/Core/Campaign24.cs", [
  ["                bool ready = waveReady33 == null || waveReady33(r, wave); // NIGHT33 E: a triggered wave waits for its line\n",
   "                bool ready = (waveReady33 == null || waveReady33(r, wave)) && (waveReady40 == null || waveReady40(r, wave)); // NIGHT33 E: a triggered wave waits for its line; NIGHT40 S1: the start grace\n"],
  ["        public Func<RoomDef, int, bool> waveReady33;\n",
   "        public Func<RoomDef, int, bool> waveReady33;\n        /// <summary>NIGHT40 S1 (start40): a wave waits while this says false (null: never waits; NIGHT39's game). Set by S1's sim partial.</summary>\n        public Func<RoomDef, int, bool> waveReady40;\n"],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N38Levels(cfg, res);\n            if (res.Count == 0) res.Add(\"layer1\");\n",
   "            N38Levels(cfg, res);\n            N40Levels(cfg, res);\n            if (res.Count == 0) res.Add(\"layer1\");\n"],
  ["        static readonly string[] N38LevelRows = { \"foundry38\", \"iceline38\", \"kiln38\", \"span38\", \"spire38\", \"finale38\" };\n",
   "        static readonly string[] N38LevelRows = { \"foundry38\", \"iceline38\", \"kiln38\", \"span38\", \"spire38\", \"finale38\" };\n\n" +
   "        /// <summary>NIGHT40 (the lead's hook; each level builder owns its row): a level row on (design.n40.kiln40 / iceline40 / loom40 /\n" +
   "        /// crown40) swaps its `levelFrom` for its `levelTo` (after N38Levels); cut40 on removes every id in design.n40.cut40.cut (level 6).\n" +
   "        /// A flow choice (the level a run loads is on its tape). Off with -skygrindOff40 / --off40 (N40Blocks.ForceOff) or the row off.</summary>\n" +
   "        public static void N40Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N40Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N40LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n40.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n40.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n40.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "            if (cfg.Design(\"n40.cut40.enabled\") && At(cfg, \"design.n40.cut40.cut\") is List<object> cut)\n" +
   "                foreach (object o in cut)\n" +
   "                    if (o is string id && res.Count > 1) res.Remove(id);\n" +
   "        }\n\n" +
   "        static readonly string[] N40LevelRows = { \"kiln40\", \"iceline40\", \"loom40\", \"crown40\" };\n"],
]);

patch("Assets/Scripts/Game/Bot/Walk38Kiln.cs", [
  ["        bool Level38K => sim.campaignLevel != null && sim.campaignLevel.id == \"n38-kiln\";\n",
   "        bool Level38K => sim.campaignLevel != null && (sim.campaignLevel.id == \"n38-kiln\" || sim.campaignLevel.id == \"n40-kiln\"); // NIGHT40: and its n40 level (kiln40)\n"],
]);
patch("Assets/Scripts/Game/Bot/Walk38Span.cs", [
  ["        bool Level38Span => sim.campaignLevel != null && (sim.campaignLevel.id ?? \"\") == \"n38-span\";\n",
   "        bool Level38Span => sim.campaignLevel != null && ((sim.campaignLevel.id ?? \"\") == \"n38-span\" || sim.campaignLevel.id == \"n40-span\"); // NIGHT40: and its n40 level (loom40)\n"],
]);
patch("Assets/Scripts/Game/Bot/Walk38Spire.cs", [
  ["        bool Level38Spire => sim.campaignLevel != null && (sim.campaignLevel.id ?? \"\") == \"n38-spire\";\n",
   "        bool Level38Spire => sim.campaignLevel != null && ((sim.campaignLevel.id ?? \"\") == \"n38-spire\" || sim.campaignLevel.id == \"n40-spire\"); // NIGHT40: and its n40 level (crown40)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N39OnPath = false;\n",
   "        public const bool N39OnPath = false;\n\n        /// <summary>NIGHT40: false unless the freeze records a run with a sim-changing NIGHT40 default on; until then the composed\n        /// fingerprint runs with every NIGHT40 block off.</summary>\n        public const bool N40OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 22; // NIGHT39:",
   "        public const int FormatVersion = 23; // NIGHT40: 23 adds the NIGHT40 blocks as a VarI (formats 1-22: off); NIGHT39:"],
  ["        public int n39;\n",
   "        public int n39;\n\n        /// <summary>NIGHT40: the NIGHT40 design blocks the run read (N40Blocks.Bits; formats 1-22: 0, every block off, NIGHT39's game).</summary>\n        public int n40;\n"],
  ["            w.VarI(n39); // format 22: the NIGHT39 blocks\n",
   "            w.VarI(n39); // format 22: the NIGHT39 blocks\n            w.VarI(n40); // format 23: the NIGHT40 blocks\n"],
  ["            t.n39 = version >= 22 ? r.VarI() : 0; // formats 1-21: recorded on NIGHT38's game (or before), so every NIGHT39 block OFF (explicitly)\n",
   "            t.n39 = version >= 22 ? r.VarI() : 0; // formats 1-21: recorded on NIGHT38's game (or before), so every NIGHT39 block OFF (explicitly)\n" +
   "            t.n40 = version >= 23 ? r.VarI() : 0; // formats 1-22: recorded on NIGHT39's game (or before), so every NIGHT40 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n39 = sim.n39.Bits; // NIGHT39: the NIGHT39 blocks the run read\n",
   "            tape.n39 = sim.n39.Bits; // NIGHT39: the NIGHT39 blocks the run read\n            tape.n40 = sim.n40.Bits; // NIGHT40: the NIGHT40 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n39Override = N39Blocks.FromBits(tape.n39); // NIGHT39: the NIGHT39 blocks the run read (formats 1-21: all off, NIGHT38's game)\n",
   "            sim.n39Override = N39Blocks.FromBits(tape.n39); // NIGHT39: the NIGHT39 blocks the run read (formats 1-21: all off, NIGHT38's game)\n" +
   "            sim.n40Override = N40Blocks.FromBits(tape.n40); // NIGHT40: the NIGHT40 blocks the run read (formats 1-22: all off, NIGHT39's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N39OnPath) sim.n39Override = N39Blocks.Off;\n",
   "            if (!FingerprintBaseline.N39OnPath) sim.n39Override = N39Blocks.Off;\n            N40Blocks? n40Saved = sim.n40Override; // NIGHT40: and every NIGHT40 block off until the freeze (N40OnPath)\n            if (!FingerprintBaseline.N40OnPath) sim.n40Override = N40Blocks.Off;\n"],
  ["                sim.n39Override = n39Saved;\n", "                sim.n39Override = n39Saved;\n                sim.n40Override = n40Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N39Blocks.ForceOff = off39;\n",
   "        N39Blocks.ForceOff = off39;\n        // NIGHT40: --off40 runs every command with every NIGHT40 design block off (NIGHT39's game); every older switch turns them off as well\n        bool off40 = off39 || Array.IndexOf(args, \"--off40\") >= 0;\n        N40Blocks.ForceOff = off40;\n"],
  ["                if (off39) sim.n39Override = N39Blocks.Off; // NIGHT39: the same\n",
   "                if (off39) sim.n39Override = N39Blocks.Off; // NIGHT39: the same\n                if (off40) sim.n40Override = N40Blocks.Off; // NIGHT40: the same\n"],
  ["        N39Views(cmd, args, ref code); // NIGHT39 V: the NIGHT38 views and the two tests (Harness.N39.Views.cs)\n",
   "        N39Views(cmd, args, ref code); // NIGHT39 V: the NIGHT38 views and the two tests (Harness.N39.Views.cs)\n" +
   "        N40Start(cmd, args, ref code); // NIGHT40 S1: start40, kiln40, cut40, rods40 (Harness.N40.Start.cs)\n" +
   "        N40Weapons(cmd, args, ref code); // NIGHT40 W1: charge40 (Harness.N40.Weapons.cs)\n" +
   "        N40Ride(cmd, args, ref code); // NIGHT40 R2: ride40 (Harness.N40.Ride.cs)\n" +
   "        N40Iceline(cmd, args, ref code); // NIGHT40 L2: n40-iceline (Harness.N40.Iceline.cs)\n" +
   "        N40Brawl(cmd, args, ref code); // NIGHT40 B2: the brawler (Harness.N40.Brawl.cs)\n" +
   "        N40Loom(cmd, args, ref code); // NIGHT40 B4: THE LOOM (Harness.N40.Loom.cs)\n" +
   "        N40Crown(cmd, args, ref code); // NIGHT40 B5: THE CROWN (Harness.N40.Crown.cs)\n" +
   "        N40Foes(cmd, args, ref code); // NIGHT40 E3: the campaign roster (Harness.N40.Foes.cs)\n"],
  ["    static partial void N39Views(string cmd, string[] args, ref int code);\n",
   "    static partial void N39Views(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT40: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N40Start(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Ride(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Iceline(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Brawl(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Loom(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Crown(string cmd, string[] args, ref int code);\n" +
   "    static partial void N40Foes(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o39 = 19; o39 <= 39; o39++) if (Array.IndexOf(args, \"-skygrindOff\" + o39) >= 0) N39Blocks.ForceOff = true; // NIGHT39: -skygrindOff39 and every older switch turn the NIGHT39 blocks off\n",
   "            for (int o39 = 19; o39 <= 39; o39++) if (Array.IndexOf(args, \"-skygrindOff\" + o39) >= 0) N39Blocks.ForceOff = true; // NIGHT39: -skygrindOff39 and every older switch turn the NIGHT39 blocks off\n" +
   "            for (int o40 = 19; o40 <= 40; o40++) if (Array.IndexOf(args, \"-skygrindOff\" + o40) >= 0) N40Blocks.ForceOff = true; // NIGHT40: -skygrindOff40 and every older switch turn the NIGHT40 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff39\" && ", "a != \"-skygrindOff39\" && a != \"-skygrindOff40\" && "],
]);

patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was, was38, was39;\n", "        bool was, was38, was39, was40;\n"],
  ["was39 = N39Blocks.ForceOff; N39Blocks.ForceOff = true; } // NIGHT38 / NIGHT39: and every NIGHT38 / NIGHT39 block off (N38SharedTests, N39SharedTests)\n",
   "was39 = N39Blocks.ForceOff; N39Blocks.ForceOff = true; was40 = N40Blocks.ForceOff; N40Blocks.ForceOff = true; } // NIGHT38 / NIGHT39 / NIGHT40: and every NIGHT38 / NIGHT39 / NIGHT40 block off (N38SharedTests, N39SharedTests, N40SharedTests)\n"],
  ["        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; N39Blocks.ForceOff = was39; }\n",
   "        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; N39Blocks.ForceOff = was39; N40Blocks.ForceOff = was40; }\n"],
  ["Is.EqualTo(22), \"NIGHT39: the current format is 22 (N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n",
   "Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n"],
]);

patch("Assets/Tests/EditMode/Game/N38SharedTests.cs", [
  ["Is.EqualTo(22), \"NIGHT39: the current format is 22 (N39SharedTests.AsFormat21 strips its value)\");\n",
   "Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)\");\n"],
]);

patch("Assets/Tests/EditMode/Game/N39SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(22));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22 strips its value)\");\n"],
  ["        public static byte[] AsFormat21(byte[] b22)\n        {\n",
   "        public static byte[] AsFormat21(byte[] b22)\n        {\n            b22 = N40SharedTests.AsFormat22(b22); // NIGHT40: a format 23 tape first loses its NIGHT40 value\n"],
]);

console.log("NIGHT40 shared patch done. Next: the Editor compile, CoreHarness golden, SimHarness fingerprint --off40, the shared tests.");

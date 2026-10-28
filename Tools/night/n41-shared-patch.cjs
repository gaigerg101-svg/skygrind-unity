// NIGHT41 shared commit (the lead): hooks for N41Blocks in the sim (StartRun, the step hook after N40Step, the fingerprint's off path),
// tape format 24 (the NIGHT41 value, a VarI after the NIGHT40 value), the --off41 / -skygrindOff41 switches (and -skygrindOff41 in
// N29Fixes.IsAutomationArg), the builders' pre-placed harness partials, the test chain (N37TestBaseline and Lesson25 force NIGHT41 off;
// AsFormat23 joins the format chain; the format pins move to 24), and NIGHT41's pre-placed hooks (L-021 / L-050; NIGHT41.md G0):
// Curriculum25.N41Levels (the level rows' swaps, finale41's return), N41BaseId at every place an older system knows a level by its id (the
// unlock table / NIGHT40 start sets and awards, kiln40's boost chain, the walk pilots, the verbs reel's start rules), Enemy.weakPoint41
// (aim41 reads it), Loadout.Heavy41 + hitstop41 at the gauntlet's knock (E6), the old view rows on through n41 rows (juice38 / doors38 /
// pump40), and the HUD33 boss bar + the Boss37 stagger bar hidden while bars41's bar draws (N41View.BossBar41Live).
// Run once after n41-config-block.cjs: node Tools/night/n41-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N40StartRun(); // NIGHT40: the run's NIGHT40 blocks (a tape's, or off with --off40 and older), the same way (SurvivalSim.N40.cs)\n",
   I + "N40StartRun(); // NIGHT40: the run's NIGHT40 blocks (a tape's, or off with --off40 and older), the same way (SurvivalSim.N40.cs)\n" +
   I + "N41StartRun(); // NIGHT41: the run's NIGHT41 blocks (a tape's, or off with --off41 and older), the same way (SurvivalSim.N41.cs)\n"],
  [I + "N40Step(step); // NIGHT40 S1 / B2 / B4 / B5 / E3 (SurvivalSim.N40.cs; nothing until a builder fills its hook)\n",
   I + "N40Step(step); // NIGHT40 S1 / B2 / B4 / B5 / E3 (SurvivalSim.N40.cs; nothing until a builder fills its hook)\n" +
   I + "N41Step(step); // NIGHT41 B8 / E6 / P3 (SurvivalSim.N41.cs; nothing until a builder fills its hook)\n"],
  [I + "if (!FingerprintBaseline.N40OnPath) n40Override = N40Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N40OnPath) n40Override = N40Blocks.Off;\n" +
   I + "N41Blocks? n41Saved = n41Override; // NIGHT41: and every NIGHT41 block off until the freeze (FingerprintBaseline.N41OnPath)\n" +
   I + "if (!FingerprintBaseline.N41OnPath) n41Override = N41Blocks.Off;\n"],
  ["                n40Override = n40Saved;\n", "                n40Override = n40Saved;\n                n41Override = n41Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N40Init(); // NIGHT40: the NIGHT40 design blocks and level rows, read once (SurvivalSim.N40.cs)\n",
   I + "N40Init(); // NIGHT40: the NIGHT40 design blocks and level rows, read once (SurvivalSim.N40.cs)\n" + I + "N41Init(); // NIGHT41: the NIGHT41 design blocks and level rows, read once (SurvivalSim.N41.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Weapons31.cs", [
  ["List<string> start = N40UnlocksBegin(campaignLevel.id, unlocks31Table.StartFor(N40TableId(campaignLevel.id))); // NIGHT40: a level row's start set, loom40's drops (SurvivalSim.N40.cs; all off: the table's)\n",
   "List<string> start = N40UnlocksBegin(N41BaseId(campaignLevel.id), unlocks31Table.StartFor(N40TableId(N41BaseId(campaignLevel.id)))); // NIGHT40: a level row's start set, loom40's drops (SurvivalSim.N40.cs; all off: the table's); NIGHT41: an n41 level reads its levelFrom's (N41BaseId)\n"],
  ["var (weapon, roomId) = unlocks31Table.RoomAward(N40TableId(campaignLevel.id));",
   "var (weapon, roomId) = unlocks31Table.RoomAward(N40TableId(N41BaseId(campaignLevel.id))); // NIGHT41: an n41 level reads its levelFrom's awards"],
]);

patch("Assets/Scripts/Game/SurvivalSim.N40.Kiln.cs", [
  ["campaign.room < 0 || campaignLevel.id != k40LevelTo || k40Lines.Count == 0) return;",
   "campaign.room < 0 || N41BaseId(campaignLevel.id) != k40LevelTo || k40Lines.Count == 0) return; // NIGHT41: n41-kiln keeps the boost chain (N41BaseId)"],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N40Levels(cfg, res);\n            if (res.Count == 0) res.Add(\"layer1\");\n",
   "            N40Levels(cfg, res);\n            N41Levels(cfg, res);\n            if (res.Count == 0) res.Add(\"layer1\");\n"],
  ["        static readonly string[] N40LevelRows = { \"kiln40\", \"iceline40\", \"loom40\", \"crown40\" };\n",
   "        static readonly string[] N40LevelRows = { \"kiln40\", \"iceline40\", \"loom40\", \"crown40\" };\n\n" +
   "        /// <summary>NIGHT41 (the lead's hook; each level builder owns its row): a level row on (design.n41.foundry41 / kiln41 / spire41)\n" +
   "        /// swaps its `levelFrom` for its `levelTo` (after N40Levels); finale41 on puts its `levelTo` back as the last level (level 6; OFF\n" +
   "        /// tonight, DECIDE41 D-3). A flow choice (the level a run loads is on its tape). Off with -skygrindOff41 / --off41\n" +
   "        /// (N41Blocks.ForceOff) or the row off.</summary>\n" +
   "        public static void N41Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N41Blocks.ForceOff || res.Count == 0) return;\n" +
   "            foreach (string row in N41LevelRows)\n" +
   "            {\n" +
   "                if (!cfg.Design(\"n41.\" + row + \".enabled\")) continue;\n" +
   "                string from = At(cfg, \"design.n41.\" + row + \".levelFrom\") as string, to = At(cfg, \"design.n41.\" + row + \".levelTo\") as string;\n" +
   "                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;\n" +
   "                int i = res.IndexOf(from);\n" +
   "                if (i >= 0) res[i] = to;\n" +
   "            }\n" +
   "            if (cfg.Design(\"n41.finale41.enabled\") && At(cfg, \"design.n41.finale41.levelTo\") is string last && last.Length > 0 && !res.Contains(last))\n" +
   "                res.Add(last);\n" +
   "        }\n\n" +
   "        static readonly string[] N41LevelRows = { \"foundry41\", \"kiln41\", \"spire41\" };\n"],
]);

patch("Assets/Scripts/Game/Bot/Walk38Kiln.cs", [
  ["sim.campaignLevel.id == \"n40-kiln\"); // NIGHT40: and its n40 level (kiln40)",
   "sim.N41BaseId(sim.campaignLevel.id) == \"n40-kiln\"); // NIGHT40: and its n40 level (kiln40); NIGHT41: and n41-kiln (N41BaseId)"],
]);
patch("Assets/Scripts/Game/Bot/Walk38Spire.cs", [
  ["sim.campaignLevel.id == \"n40-spire\"); // NIGHT40:",
   "sim.N41BaseId(sim.campaignLevel.id) == \"n40-spire\"); // NIGHT41: and n41-spire (N41BaseId); NIGHT40:"],
]);
patch("Assets/Scripts/Game/Bot/Walk38Foundry.cs", [
  ["        string LevelId => sim.campaignLevel != null ? sim.campaignLevel.id ?? \"\" : \"\";\n",
   "        string LevelId => sim.campaignLevel != null ? sim.N41BaseId(sim.campaignLevel.id) ?? \"\" : \"\"; // NIGHT41: n41-foundry walks as n38-foundry (N41BaseId)\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N35.Verbs.cs", [
  ["verbs40GauntletOnly = !gun40 && sim.campaignLevel != null && (sim.campaignLevel.id ?? \"\").StartsWith(\"n40-\");",
   "verbs40GauntletOnly = !gun40 && sim.campaignLevel != null && (sim.N41BaseId(sim.campaignLevel.id) ?? \"\").StartsWith(\"n40-\"); // NIGHT41: an n41 level as its levelFrom"],
  ["bool n40Start = sim.campaignLevel != null && (sim.campaignLevel.id ?? \"\").StartsWith(\"n40-\");",
   "bool n40Start = sim.campaignLevel != null && (sim.N41BaseId(sim.campaignLevel.id) ?? \"\").StartsWith(\"n40-\"); // NIGHT41: an n41 level as its levelFrom"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N40OnPath = false;\n",
   "        public const bool N40OnPath = false;\n\n        /// <summary>NIGHT41: false unless the freeze records a run with a sim-changing NIGHT41 default on; until then the composed\n        /// fingerprint runs with every NIGHT41 block off.</summary>\n        public const bool N41OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 23; // NIGHT40:",
   "        public const int FormatVersion = 24; // NIGHT41: 24 adds the NIGHT41 blocks as a VarI (formats 1-23: off); NIGHT40:"],
  ["        public int n40;\n",
   "        public int n40;\n\n        /// <summary>NIGHT41: the NIGHT41 design blocks the run read (N41Blocks.Bits; formats 1-23: 0, every block off, NIGHT40's game).</summary>\n        public int n41;\n"],
  ["            w.VarI(n40); // format 23: the NIGHT40 blocks\n",
   "            w.VarI(n40); // format 23: the NIGHT40 blocks\n            w.VarI(n41); // format 24: the NIGHT41 blocks\n"],
  ["            t.n40 = version >= 23 ? r.VarI() : 0; // formats 1-22: recorded on NIGHT39's game (or before), so every NIGHT40 block OFF (explicitly)\n",
   "            t.n40 = version >= 23 ? r.VarI() : 0; // formats 1-22: recorded on NIGHT39's game (or before), so every NIGHT40 block OFF (explicitly)\n" +
   "            t.n41 = version >= 24 ? r.VarI() : 0; // formats 1-23: recorded on NIGHT40's game (or before), so every NIGHT41 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n40 = sim.n40.Bits; // NIGHT40: the NIGHT40 blocks the run read\n",
   "            tape.n40 = sim.n40.Bits; // NIGHT40: the NIGHT40 blocks the run read\n            tape.n41 = sim.n41.Bits; // NIGHT41: the NIGHT41 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n40Override = N40Blocks.FromBits(tape.n40); // NIGHT40: the NIGHT40 blocks the run read (formats 1-22: all off, NIGHT39's game)\n",
   "            sim.n40Override = N40Blocks.FromBits(tape.n40); // NIGHT40: the NIGHT40 blocks the run read (formats 1-22: all off, NIGHT39's game)\n" +
   "            sim.n41Override = N41Blocks.FromBits(tape.n41); // NIGHT41: the NIGHT41 blocks the run read (formats 1-23: all off, NIGHT40's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N40OnPath) sim.n40Override = N40Blocks.Off;\n",
   "            if (!FingerprintBaseline.N40OnPath) sim.n40Override = N40Blocks.Off;\n            N41Blocks? n41Saved = sim.n41Override; // NIGHT41: and every NIGHT41 block off until the freeze (N41OnPath)\n            if (!FingerprintBaseline.N41OnPath) sim.n41Override = N41Blocks.Off;\n"],
  ["                sim.n40Override = n40Saved;\n", "                sim.n40Override = n40Saved;\n                sim.n41Override = n41Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N40Blocks.ForceOff = off40;\n",
   "        N40Blocks.ForceOff = off40;\n        // NIGHT41: --off41 runs every command with every NIGHT41 design block off (NIGHT40's game); every older switch turns them off as well\n        bool off41 = off40 || Array.IndexOf(args, \"--off41\") >= 0;\n        N41Blocks.ForceOff = off41;\n"],
  ["                if (off40) sim.n40Override = N40Blocks.Off; // NIGHT40: the same\n",
   "                if (off40) sim.n40Override = N40Blocks.Off; // NIGHT40: the same\n                if (off41) sim.n41Override = N41Blocks.Off; // NIGHT41: the same\n"],
  ["        N40Foes(cmd, args, ref code); // NIGHT40 E3: the campaign roster (Harness.N40.Foes.cs)\n",
   "        N40Foes(cmd, args, ref code); // NIGHT40 E3: the campaign roster (Harness.N40.Foes.cs)\n" +
   "        N41Proof(cmd, args, ref code); // NIGHT41 T2: the proof tooling (Harness.N41.Proof.cs)\n" +
   "        N41Juice(cmd, args, ref code); // NIGHT41 J2: the boss moment, doors, bars, the select (Harness.N41.Juice.cs)\n" +
   "        N41Look(cmd, args, ref code); // NIGHT41 K2: the levels' look (Harness.N41.Look.cs)\n" +
   "        N41Bosses(cmd, args, ref code); // NIGHT41 B8: sweep41, THE ICEBREAKER (Harness.N41.Bosses.cs)\n" +
   "        N41Enemies(cmd, args, ref code); // NIGHT41 E6: tokens41, heavy41, kiln41 (Harness.N41.Enemies.cs)\n" +
   "        N41Pad(cmd, args, ref code); // NIGHT41 P3: aim41 (Harness.N41.Pad.cs)\n" +
   "        N41Levels(cmd, args, ref code); // NIGHT41 R5: spire41, foundry41 (Harness.N41.Levels.cs)\n" +
   "        N41Finale(cmd, args, ref code); // NIGHT41 L6: the level 6 greybox (Harness.N41.Finale.cs)\n"],
  ["    static partial void N40Foes(string cmd, string[] args, ref int code);\n",
   "    static partial void N40Foes(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT41: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N41Proof(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Juice(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Look(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Bosses(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Enemies(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Pad(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Levels(string cmd, string[] args, ref int code);\n" +
   "    static partial void N41Finale(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o40 = 19; o40 <= 40; o40++) if (Array.IndexOf(args, \"-skygrindOff\" + o40) >= 0) N40Blocks.ForceOff = true; // NIGHT40: -skygrindOff40 and every older switch turn the NIGHT40 blocks off\n",
   "            for (int o40 = 19; o40 <= 40; o40++) if (Array.IndexOf(args, \"-skygrindOff\" + o40) >= 0) N40Blocks.ForceOff = true; // NIGHT40: -skygrindOff40 and every older switch turn the NIGHT40 blocks off\n" +
   "            for (int o41 = 19; o41 <= 41; o41++) if (Array.IndexOf(args, \"-skygrindOff\" + o41) >= 0) N41Blocks.ForceOff = true; // NIGHT41: -skygrindOff41 and every older switch turn the NIGHT41 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff40\" && ", "a != \"-skygrindOff40\" && a != \"-skygrindOff41\" && "],
]);

// the gauntlet's knock (E6's heavy41) and the weak point (aim41)
patch("Assets/Scripts/Core/Combat.cs", [
  ["        public bool standIn29;\n",
   "        public bool standIn29;\n        /// <summary>NIGHT41 (the shared commit): this body's weak point, the spot aim41 steers to (Core/WeakPoint41.cs); inactive unless a boss builder fills it.</summary>\n        public WeakPoint41 weakPoint41;\n"],
  ["        readonly double gDamage, gCycle, gRange, gCone, gKnock, gKnockTime, gParryPeek;\n",
   "        readonly double gDamage, gCycle, gRange, gCone, gKnock, gKnockTime, gParryPeek;\n" +
   "        /// <summary>NIGHT41 E6 heavy41 (the sim sets it at a run's start from sim.n41): the gauntlet's swing asks Heavy41 for a big enemy's answer.</summary>\n" +
   "        public bool heavy41;\n" +
   "        /// <summary>NIGHT41 E6 heavy41: the sim's hitstop (SurvivalSim.Hitstop, set at the sim's Init); a Heavy41 answer's hitstop goes through it.</summary>\n" +
   "        public System.Action<double, string> hitstop41;\n" +
   "        /// <summary>NIGHT41 E6 heavy41 (the body in E6's Core/Heavy41.cs): a gauntlet hit on `enemy`; a big kind may raise the knock-back\n" +
   "        /// (speed, time) and ask for a hitstop (s, 0: none). No body: the old knock, no hitstop. Called only while heavy41 is on.</summary>\n" +
   "        partial void Heavy41(Enemy enemy, ref double knock, ref double knockTime, ref double hitstop);\n"],
  ["                if (!launch) MeleeKnock40(enemy, enemy.position.x - origin.x, enemy.position.z - origin.z, gKnock, gKnockTime, dt); // NIGHT40 E3: a melee knock (Foes40.cs: the same knock; a WARDEN knows it is melee)\n",
   "                double knock41 = gKnock, knockTime41 = gKnockTime, hitstop41s = 0;\n" +
   "                if (heavy41) Heavy41(enemy, ref knock41, ref knockTime41, ref hitstop41s); // NIGHT41 E6 heavy41 (Core/Heavy41.cs): a big enemy's harder answer (off: the old knock, no hitstop)\n" +
   "                if (!launch) MeleeKnock40(enemy, enemy.position.x - origin.x, enemy.position.z - origin.z, knock41, knockTime41, dt); // NIGHT40 E3: a melee knock (Foes40.cs: the same knock; a WARDEN knows it is melee)\n" +
   "                if (hitstop41s > 0 && hitstop41 != null) hitstop41(hitstop41s, \"heavy41\"); // NIGHT41 E6 heavy41: the sim's hitstop (ImpactRules keeps a cause it does not know at the sim's length)\n"],
]);

// the old view rows on through n41 rows, never in place
patch("Assets/Scripts/Game/Juice38.cs", [
  ["            on = Hud38.RowOn(config, \"juice38\");\n",
   "            on = Hud38.RowOn(config, \"juice38\") || N41View.RowOn(config, \"juice41On\"); // NIGHT41 (the lead's hook): or design.n41.juice41On (the n38 row untouched; off with --off41)\n"],
]);
patch("Assets/Scripts/Game/Doors38View.cs", [
  ["            On = !N38Blocks.ForceOff && game.cfg.Design(\"n38.doors38.enabled\");\n",
   "            On = (!N38Blocks.ForceOff && game.cfg.Design(\"n38.doors38.enabled\")) || N41View.RowOn(game.cfg, \"doors41On\"); // NIGHT41 (the lead's hook): or design.n41.doors41On (the n38 row untouched; off with --off41)\n"],
]);
patch("Assets/Scripts/Game/Viewmodel.Pump40.cs", [
  ["        bool pump40Design;\n", "        bool pump40Design;\n        bool pump41Design; // NIGHT41 (the lead's hook): design.n41.pump41On, read at Init\n"],
  ["        bool Pump40Now => Pump40Live = pump40Design && !N40Blocks.ForceOff;\n",
   "        bool Pump40Now => Pump40Live = (pump40Design && !N40Blocks.ForceOff) || (pump41Design && !N41View.Off); // NIGHT41: or design.n41.pump41On (T2's row; off with --off41)\n"],
  ["            pump40Design = cfg != null && cfg.Design(\"n40.pump40.enabled\");\n            pump40Spec = pump40Design\n",
   "            pump40Design = cfg != null && cfg.Design(\"n40.pump40.enabled\");\n            pump41Design = cfg != null && cfg.Design(\"n41.pump41On.enabled\"); // NIGHT41 (the lead's hook)\n            pump40Spec = pump40Design || pump41Design\n"],
]);

// the boss bar: bars41's UiBossBar41 replaces the HUD33 bar's drawing while it runs (J2 sets N41View.BossBar41Live)
patch("Assets/Scripts/Game/UiHud33.cs", [
  ["            if (h33BossRt.gameObject.activeSelf != on)\n            {\n                h33BossRt.gameObject.SetActive(on);\n                h33BossAge = on ? 0 : -1;\n",
   "            bool drawn = on && !N41View.BossBar41Live; // NIGHT41 (the lead's hook): bars41's bar draws instead while it runs (the slot keeps its place: Hud33BossBarShown = on)\n" +
   "            if (h33BossRt.gameObject.activeSelf != drawn)\n            {\n                h33BossRt.gameObject.SetActive(drawn);\n                h33BossAge = drawn ? 0 : -1;\n"],
]);
patch("Assets/Scripts/Game/Boss37Kit.View.cs", [
  ["        public static void Show(Boss37Brain brain)\n        {\n            if (brain == null) return;\n",
   "        public static void Show(Boss37Brain brain)\n        {\n            if (brain == null || N41View.BossBar41Live) return; // NIGHT41 (the lead's hook): bars41's bar draws the stagger meter while it runs\n"],
]);

// the tests
patch("Assets/Tests/EditMode/Game/N37SharedTests.cs", [
  ["        bool was, was38, was39, was40;\n", "        bool was, was38, was39, was40, was41;\n"],
  ["was40 = N40Blocks.ForceOff; N40Blocks.ForceOff = true; } // NIGHT38 / NIGHT39 / NIGHT40: and every NIGHT38 / NIGHT39 / NIGHT40 block off (N38SharedTests, N39SharedTests, N40SharedTests)\n",
   "was40 = N40Blocks.ForceOff; N40Blocks.ForceOff = true; was41 = N41Blocks.ForceOff; N41Blocks.ForceOff = true; } // NIGHT38-NIGHT41: and every NIGHT38-NIGHT41 block off (N38SharedTests ... N41SharedTests)\n"],
  ["N40Blocks.ForceOff = was40; }\n", "N40Blocks.ForceOff = was40; N41Blocks.ForceOff = was41; }\n"],
  ["Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n",
   "Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23, N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N38SharedTests.cs", [
  ["Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)\");\n",
   "Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23, N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N39SharedTests.cs", [
  ["Is.EqualTo(23), \"NIGHT40: the current format is 23 (N40SharedTests.AsFormat22 strips its value)\");\n",
   "Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23 and N40SharedTests.AsFormat22 strip their values)\");\n"],
]);
patch("Assets/Tests/EditMode/Game/N40SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(23));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(24), \"NIGHT41: the current format is 24 (N41SharedTests.AsFormat23 strips its value)\");\n"],
  ["        public static byte[] AsFormat22(byte[] b23)\n        {\n",
   "        public static byte[] AsFormat22(byte[] b23)\n        {\n            b23 = N41SharedTests.AsFormat23(b23); // NIGHT41: a format 24 tape first loses its NIGHT41 value\n"],
]);
patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  ["            N40Blocks.ForceOff = true; // NIGHT40 (L-027):",
   "            N41Blocks.ForceOff = true; // NIGHT41 (L-027): foundry41 / kiln41 / spire41 swap n41 ids in once ON; this test pins NIGHT39's and earlier orders, so NIGHT41 is forced off here (N41SharedTests pins its own swap)\n            N40Blocks.ForceOff = true; // NIGHT40 (L-027):"],
]);

console.log("NIGHT41 shared patch done. Next: the Editor compile, CoreHarness golden, SimHarness fingerprint plain and --off41, the shared tests.");

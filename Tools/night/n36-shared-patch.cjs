// NIGHT36 shared commit (the lead): hooks for N36Blocks in the sim (StartRun, the builders' Step hooks after N35TeachStep, the
// fingerprint's off path), tape format 19 (the NIGHT36 value, a VarI), the --off36 / -skygrindOff36 switches, the walkin36 gate in
// Campaign24 (ICampaignWorld.BossWalkIn: no Respawn into a far arena), B's N36BossBegin first in BossRoomBegin, Curriculum25.N36Levels
// (design.n36.level36's swap), the builders' pre-placed dispatch hooks (harness partials N36Guns / Core / Move / Level / Boss, reel
// scenarios by owner F / P / M / L / B), the old tests' NIGHT36 pins (N35SharedTests' format).
// Run once: node Tools/night/n36-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  [I + "N35StartRun(); // NIGHT35: the run's NIGHT35 blocks (a tape's, or off with --off35 and older), the same way (SurvivalSim.N35.cs)\n",
   I + "N35StartRun(); // NIGHT35: the run's NIGHT35 blocks (a tape's, or off with --off35 and older), the same way (SurvivalSim.N35.cs)\n" +
   I + "N36StartRun(); // NIGHT36: the run's NIGHT36 blocks (a tape's, or off with --off36 and older), the same way (SurvivalSim.N36.cs)\n"],
  [I + "N35TeachStep(step); // NIGHT35 T: teach35 (SurvivalSim.Teach35.cs)\n",
   I + "N35TeachStep(step); // NIGHT35 T: teach35 (SurvivalSim.Teach35.cs)\n" +
   I + "N36GunsStep(step); // NIGHT36 F: tap36, crunch36 (SurvivalSim.Guns36.cs)\n" +
   I + "N36CoreStep(step); // NIGHT36 P: tells36, parry36, launch36, heal36 (SurvivalSim.Core36.cs)\n" +
   I + "N36MoveStep(step); // NIGHT36 M: wallgrip36 (SurvivalSim.Move36.cs)\n" +
   I + "N36LevelStep(step); // NIGHT36 L: the arena flow on n36 levels (SurvivalSim.Level36.cs)\n" +
   I + "N36BossStep(step); // NIGHT36 B: walkin36, boss36 (SurvivalSim.Boss36.cs)\n"],
  [I + "if (!FingerprintBaseline.N35OnPath) n35Override = N35Blocks.Off;\n",
   I + "if (!FingerprintBaseline.N35OnPath) n35Override = N35Blocks.Off;\n" +
   I + "N36Blocks? n36Saved = n36Override; // NIGHT36: and every NIGHT36 block off until the freeze (FingerprintBaseline.N36OnPath)\n" +
   I + "if (!FingerprintBaseline.N36OnPath) n36Override = N36Blocks.Off;\n"],
  ["                n35Override = n35Saved;\n", "                n35Override = n35Saved;\n                n36Override = n36Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  [I + "N35Init(); // NIGHT35: the NIGHT35 design blocks, read once (SurvivalSim.N35.cs)\n",
   I + "N35Init(); // NIGHT35: the NIGHT35 design blocks, read once (SurvivalSim.N35.cs)\n" + I + "N36Init(); // NIGHT36: the NIGHT36 design blocks, read once (SurvivalSim.N36.cs)\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Boss25.cs", [
  ["            spawn = default;\n            yawDeg = 0;\n            if (id == \"crown\")",
   "            spawn = default;\n            yawDeg = 0;\n" +
   "            bool n36Handled = false, n36Ok = false;\n" +
   "            N36BossBegin(id, ref n36Handled, ref n36Ok, ref spawn, ref yawDeg); // NIGHT36 B: boss36 / walkin36 own the begin when they say so (SurvivalSim.Boss36.cs)\n" +
   "            if (n36Handled) return n36Ok;\n" +
   "            if (id == \"crown\")"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Campaign24.cs", [
  ["            public bool BossIsDefeated(string boss) => s.BossRoomDefeated(boss);\n",
   "            public bool BossIsDefeated(string boss) => s.BossRoomDefeated(boss);\n" +
   "            public bool BossWalkIn => s.Boss36WalkIn; // NIGHT36 B (walkin36): the fight is in the level's last room, no Respawn (SurvivalSim.N36.cs)\n"],
]);

patch("Assets/Scripts/Core/Campaign24.cs", [
  ["        bool BossIsDefeated(string boss) => BossDefeated;\n",
   "        bool BossIsDefeated(string boss) => BossDefeated;\n" +
   "        /// <summary>NIGHT36 (walkin36): the boss fights in the room the player walked into, so its begin keeps the player where they are\n" +
   "        /// (no Respawn into a far arena). Default false: every boss before NIGHT36 takes the player to its own arena.</summary>\n" +
   "        bool BossWalkIn => false;\n"],
  ["                if (bossOn) world.Respawn(bossSpawn, bossYaw); // the fight is in its own room\n",
   "                if (bossOn && !world.BossWalkIn) world.Respawn(bossSpawn, bossYaw); // the fight is in its own room (NIGHT36 walkin36: or here, where the player walked in)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N35OnPath = false;\n",
   "        public const bool N35OnPath = false;\n\n        /// <summary>NIGHT36: false unless the freeze records a run with a sim-changing NIGHT36 default on; until then the composed\n        /// fingerprint runs with every NIGHT36 block off.</summary>\n        public const bool N36OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 18; // NIGHT35:",
   "        public const int FormatVersion = 19; // NIGHT36: 19 adds the NIGHT36 blocks as a VarI (nine bits; formats 1-18: off); NIGHT35:"],
  ["        public int n35;\n",
   "        public int n35;\n\n        /// <summary>NIGHT36: the NIGHT36 design blocks the run read (N36Blocks.Bits; formats 1-18: 0, every block off, NIGHT35's game).</summary>\n        public int n36;\n"],
  ["            w.Byte((byte)n35); // format 18: the NIGHT35 blocks\n",
   "            w.Byte((byte)n35); // format 18: the NIGHT35 blocks\n            w.VarI(n36); // format 19: the NIGHT36 blocks (nine bits)\n"],
  ["            t.n35 = version >= 18 ? r.Byte() : 0; // formats 1-17: recorded on NIGHT34's game (or before), so every NIGHT35 block OFF (explicitly)\n",
   "            t.n35 = version >= 18 ? r.Byte() : 0; // formats 1-17: recorded on NIGHT34's game (or before), so every NIGHT35 block OFF (explicitly)\n" +
   "            t.n36 = version >= 19 ? r.VarI() : 0; // formats 1-18: recorded on NIGHT35's game (or before), so every NIGHT36 block OFF (explicitly)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n35 = sim.n35.Bits; // NIGHT35: the NIGHT35 blocks the run read\n",
   "            tape.n35 = sim.n35.Bits; // NIGHT35: the NIGHT35 blocks the run read\n            tape.n36 = sim.n36.Bits; // NIGHT36: the NIGHT36 blocks the run read\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n35Override = N35Blocks.FromBits(tape.n35); // NIGHT35: the NIGHT35 blocks the run read (formats 1-17: all off, NIGHT34's game)\n",
   "            sim.n35Override = N35Blocks.FromBits(tape.n35); // NIGHT35: the NIGHT35 blocks the run read (formats 1-17: all off, NIGHT34's game)\n" +
   "            sim.n36Override = N36Blocks.FromBits(tape.n36); // NIGHT36: the NIGHT36 blocks the run read (formats 1-18: all off, NIGHT35's game)\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            if (!FingerprintBaseline.N35OnPath) sim.n35Override = N35Blocks.Off;\n",
   "            if (!FingerprintBaseline.N35OnPath) sim.n35Override = N35Blocks.Off;\n            N36Blocks? n36Saved = sim.n36Override; // NIGHT36: and every NIGHT36 block off until the freeze (N36OnPath)\n            if (!FingerprintBaseline.N36OnPath) sim.n36Override = N36Blocks.Off;\n"],
  ["                sim.n35Override = n35Saved;\n", "                sim.n35Override = n35Saved;\n                sim.n36Override = n36Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N35Blocks.ForceOff = off35;\n",
   "        N35Blocks.ForceOff = off35;\n        // NIGHT36: --off36 runs every command with every NIGHT36 design block off (NIGHT35's game); every older switch turns them off as well\n        bool off36 = off35 || Array.IndexOf(args, \"--off36\") >= 0;\n        N36Blocks.ForceOff = off36;\n"],
  ["                if (off35) sim.n35Override = N35Blocks.Off; // NIGHT35: the same\n",
   "                if (off35) sim.n35Override = N35Blocks.Off; // NIGHT35: the same\n                if (off36) sim.n36Override = N36Blocks.Off; // NIGHT36: the same\n"],
  ["        N35Levels(cmd, args, ref code); // NIGHT35 A: campaign6, the wall-ride rooms, the walk (Harness.N35.Levels.cs)\n",
   "        N35Levels(cmd, args, ref code); // NIGHT35 A: campaign6, the wall-ride rooms, the walk (Harness.N35.Levels.cs)\n" +
   "        N36Guns(cmd, args, ref code); // NIGHT36 F: tap36, crunch36 (Harness.N36.Guns.cs)\n" +
   "        N36Core(cmd, args, ref code); // NIGHT36 P: tells36, parry36, launch36, heal36 (Harness.N36.Core.cs)\n" +
   "        N36Move(cmd, args, ref code); // NIGHT36 M: wallgrip36, the bots (Harness.N36.Move.cs)\n" +
   "        N36Level(cmd, args, ref code); // NIGHT36 L: level 1 from scratch, the jobs check, the arena flow (Harness.N36.Level.cs)\n" +
   "        N36Boss(cmd, args, ref code); // NIGHT36 B: walkin36, boss36, THE LANTERN (Harness.N36.Boss.cs)\n"],
  ["    static partial void N35Levels(string cmd, string[] args, ref int code);\n",
   "    static partial void N35Levels(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT36: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N36Guns(string cmd, string[] args, ref int code);\n" +
   "    static partial void N36Core(string cmd, string[] args, ref int code);\n" +
   "    static partial void N36Move(string cmd, string[] args, ref int code);\n" +
   "    static partial void N36Level(string cmd, string[] args, ref int code);\n" +
   "    static partial void N36Boss(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["N35Blocks.ForceOff = true; // NIGHT35: -skygrindOff35 and every older switch turn the NIGHT35 blocks off\n",
   "N35Blocks.ForceOff = true; // NIGHT35: -skygrindOff35 and every older switch turn the NIGHT35 blocks off\n            for (int o36 = 19; o36 <= 36; o36++) if (Array.IndexOf(args, \"-skygrindOff\" + o36) >= 0) N36Blocks.ForceOff = true; // NIGHT36: -skygrindOff36 and every older switch turn the NIGHT36 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff35\" && ", "a != \"-skygrindOff35\" && a != \"-skygrindOff36\" && "],
]);

patch("Assets/Scripts/Core/Curriculum25.cs", [
  ["            N35Levels(cfg, res);\n", "            N35Levels(cfg, res);\n            N36Levels(cfg, res);\n"],
  ["        public static void N33Levels(GameConfig cfg, List<string> res)\n",
   "        /// <summary>NIGHT36 (the lead; builder L owns the row): design.n36.level36 (on) swaps each id in its `levelSwap` map ({old id: new\n" +
   "        /// id}, after N35Levels: L's from-scratch n36 files; tonight level 1 only). A flow choice (the level a run loads is on its tape).\n" +
   "        /// Off with -skygrindOff36 / --off36 (N36Blocks.ForceOff) or with the row off: NIGHT35's order.</summary>\n" +
   "        public static void N36Levels(GameConfig cfg, List<string> res)\n" +
   "        {\n" +
   "            if (N36Blocks.ForceOff || res.Count == 0 || !cfg.Design(\"n36.level36.enabled\")) return;\n" +
   "            if (At(cfg, \"design.n36.level36.levelSwap\") is Dictionary<string, object> map)\n" +
   "                for (int i = 0; i < res.Count; i++)\n" +
   "                    if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;\n" +
   "        }\n\n" +
   "        public static void N33Levels(GameConfig cfg, List<string> res)\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| IsPhaseN34(s) || IsPhaseN35(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35: below\n",
   "|| IsPhaseN34(s) || IsPhaseN35(s) || IsPhaseN36(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33, NIGHT34, NIGHT35, NIGHT36: below\n\n" +
   "        // NIGHT36: the reel scenarios by owner; each owner's FeelReel.N36.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN36F = { \"tap36\", \"crunch36\", \"fist36\" };\n" +
   "        public static readonly string[] PhaseN36P = { \"parry36\", \"launch36\", \"heal36\" };\n" +
   "        public static readonly string[] PhaseN36M = { \"wallgrip36\", \"text36\", \"verbs36\" };\n" +
   "        public static readonly string[] PhaseN36L = { \"level36\" };\n" +
   "        public static readonly string[] PhaseN36B = { \"boss36\" };\n" +
   "        static bool IsPhaseN36(string s) => Array.IndexOf(PhaseN36F, s) >= 0 || Array.IndexOf(PhaseN36P, s) >= 0 || Array.IndexOf(PhaseN36M, s) >= 0 || Array.IndexOf(PhaseN36L, s) >= 0 || Array.IndexOf(PhaseN36B, s) >= 0;\n" +
   "        static partial void N36GunsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N36CoreBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N36MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N36LevelBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N36BossBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN35A, s) >= 0) N35LevelsBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN35A, s) >= 0) N35LevelsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN36F, s) >= 0) N36GunsBody(s, g, dir, res, offs, pre, ref body); // NIGHT36: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN36P, s) >= 0) N36CoreBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN36M, s) >= 0) N36MoveBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN36L, s) >= 0) N36LevelBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN36B, s) >= 0) N36BossBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

patch("Assets/Tests/EditMode/Game/N35SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(18));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT36: the current format is 19; the n35 byte is read below\n"],
  ["            if (BitConverter.ToInt32(b18, RunTape.Magic.Length) < 18) return b18;\n",
   "            b18 = N36SharedTests.AsFormat18(b18); // NIGHT36: a format 19 tape first loses its NIGHT36 value\n            if (BitConverter.ToInt32(b18, RunTape.Magic.Length) < 18) return b18;\n"],
]);

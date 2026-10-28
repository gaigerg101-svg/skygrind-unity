// NIGHT33 shared commit (the lead): hooks for N33Blocks in the sim, tape format 16 (the NIGHT33 byte, the shop's owned upgrades and the
// banked points), the --off33 / -skygrindOff33 switches, the test chain, the shop hook in ContinueCampaign, and the builders' pre-placed
// dispatch hooks (harness partials N33Look / N33Kit / N33Arenas / N33Enemies / N33Weapons / N33Shop, reel scenarios by owner).
// Run once: node Tools/night/n33-shared-patch.cjs   (every replace asserts its anchor exists exactly once)
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
  ["            N32StartRun(); // NIGHT32: the run's NIGHT32 blocks (a tape's, or off with --off32 and older), the same way (SurvivalSim.N32.cs)\n",
   "            N32StartRun(); // NIGHT32: the run's NIGHT32 blocks (a tape's, or off with --off32 and older), the same way (SurvivalSim.N32.cs)\n            N33StartRun(); // NIGHT33: the run's NIGHT33 blocks (a tape's, or off with --off33 and older), the same way (SurvivalSim.N33.cs)\n"],
  ["            heat.ApplyRung(mutTargets, rung, st.mutators);\n            Hitch24.Mark(\"sr:60 heat.ApplyRung\");\n",
   "            heat.ApplyRung(mutTargets, rung, st.mutators);\n            Hitch24.Mark(\"sr:60 heat.ApplyRung\");\n            N33ShopAfterStart(); // NIGHT33 P: the shop's upgrades and the banked points, once the economy and the health are set (SurvivalSim.Shop33.cs)\n"],
  ["            N32GrappleFrame(ref frame); // NIGHT32 G: the grapple's charges, after the NIGHT31 latch (SurvivalSim.Grapple32.cs)\n",
   "            N32GrappleFrame(ref frame); // NIGHT32 G: the grapple's charges, after the NIGHT31 latch (SurvivalSim.Grapple32.cs)\n            N33WhipFrame(ref frame); // NIGHT33 W: the whip, after the NIGHT32 charges (SurvivalSim.Whip33.cs)\n"],
  ["            if (pad >= 0) events.Emit(\"jumpPad\", pad);\n",
   "            if (pad >= 0) events.Emit(\"jumpPad\", pad);\n            N33DevicesMove(step); // NIGHT33 R: the devices that move the player (fan, vent, platform), right after the Mover and the pads (SurvivalSim.Devices33.cs)\n"],
  ["            N32CrownStep(step);\n",
   "            N32CrownStep(step);\n            N33AiStep(step); // NIGHT33: the lead's hooks for the NIGHT33 rows (SurvivalSim.N33.cs; nothing without a body)\n            N33SpawnsStep(step);\n            N33BossStep(step);\n            N33WhipStep(step);\n            N33PierceStep(step);\n            N33DevicesStep(step);\n"],
  ["            brain.Separate(husks); // ai/crowd.js: once per step, after every enemy moved\n",
   "            brain.Separate(husks); // ai/crowd.js: once per step, after every enemy moved\n            N33AfterCrowd(step); // NIGHT33 E: the lava edge guard, after the crowd's pushes (SurvivalSim.Ai33.cs)\n"],
  ["            N32Blocks? n32Saved = n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (FingerprintBaseline.N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) n32Override = N32Blocks.Off;\n",
   "            N32Blocks? n32Saved = n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (FingerprintBaseline.N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) n32Override = N32Blocks.Off;\n            N33Blocks? n33Saved = n33Override; // NIGHT33: and every NIGHT33 block off until the freeze (FingerprintBaseline.N33OnPath)\n            if (!FingerprintBaseline.N33OnPath) n33Override = N33Blocks.Off;\n            int shop33Saved = shop33Owned, bank33Saved = bank33; // NIGHT33 P: the fingerprint buys nothing and banks nothing\n            shop33Owned = bank33 = 0;\n"],
  ["                n32Override = n32Saved;\n",
   "                n32Override = n32Saved;\n                n33Override = n33Saved;\n                shop33Owned = shop33Saved;\n                bank33 = bank33Saved;\n"],
]);

patch("Assets/Scripts/Game/SurvivalSim.cs", [
  ["            N32Init(); // NIGHT32: the NIGHT32 design blocks, read once (SurvivalSim.N32.cs)\n",
   "            N32Init(); // NIGHT32: the NIGHT32 design blocks, read once (SurvivalSim.N32.cs)\n            N33Init(); // NIGHT33: the NIGHT33 design blocks, read once (SurvivalSim.N33.cs)\n"],
]);

patch("Assets/Scripts/Game/FingerprintBaseline.cs", [
  ["        public const bool N32OnPath = false;\n",
   "        public const bool N32OnPath = false;\n\n        /// <summary>NIGHT33: false unless the freeze records a run with a sim-changing NIGHT33 default on; until then the composed\n        /// fingerprint runs with every NIGHT33 block off.</summary>\n        public const bool N33OnPath = false;\n"],
]);

patch("Assets/Scripts/Game/Tape/RunTape.cs", [
  ["        public const int FormatVersion = 15; // NIGHT32: 15 adds the NIGHT32 blocks (formats 1-14: off);",
   "        public const int FormatVersion = 16; // NIGHT33: 16 adds the NIGHT33 blocks, the shop's owned upgrades and the banked points (formats 1-15: off, none, 0); NIGHT32: 15 adds the NIGHT32 blocks (formats 1-14: off);"],
  ["        public int n32;\n",
   "        public int n32;\n\n        /// <summary>NIGHT33: the NIGHT33 design blocks the run read (N33Blocks.Bits; formats 1-15: 0, every block off, NIGHT32's game).</summary>\n        public int n33;\n\n        /// <summary>NIGHT33 P: the shop's owned upgrades the run started with (SurvivalSim.shop33Owned; formats 1-15: 0, none).</summary>\n        public int shop33;\n\n        /// <summary>NIGHT33 P: the campaign's banked points the run started with (SurvivalSim.bank33; formats 1-15: 0).</summary>\n        public int bank33;\n"],
  ["            w.Byte((byte)n32); // format 15: the NIGHT32 blocks\n",
   "            w.Byte((byte)n32); // format 15: the NIGHT32 blocks\n            w.Byte((byte)n33); // format 16: the NIGHT33 blocks ...\n            w.VarI(shop33); // ... the shop's owned upgrades ...\n            w.VarI(bank33); // ... and the banked points\n"],
  ["            t.n32 = version >= 15 ? r.Byte() : 0; // formats 1-14: recorded on NIGHT31's game (or before), so every NIGHT32 block OFF (explicitly)\n",
   "            t.n32 = version >= 15 ? r.Byte() : 0; // formats 1-14: recorded on NIGHT31's game (or before), so every NIGHT32 block OFF (explicitly)\n            if (version >= 16) // formats 1-15: recorded on NIGHT32's game (or before): every NIGHT33 block OFF, nothing bought, nothing banked (explicitly)\n            {\n                t.n33 = r.Byte();\n                t.shop33 = r.VarI();\n                t.bank33 = r.VarI();\n            }\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeRecorder.cs", [
  ["            tape.n32 = sim.n32.Bits; // NIGHT32: the NIGHT32 blocks the run read\n",
   "            tape.n32 = sim.n32.Bits; // NIGHT32: the NIGHT32 blocks the run read\n            tape.n33 = sim.n33.Bits; // NIGHT33: the NIGHT33 blocks the run read, the shop's upgrades and the banked points it started with\n            tape.shop33 = sim.shop33Owned;\n            tape.bank33 = sim.bank33;\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeReplay.cs", [
  ["            sim.n32Override = N32Blocks.FromBits(tape.n32); // NIGHT32: the NIGHT32 blocks the run read (formats 1-14: all off, NIGHT31's game)\n",
   "            sim.n32Override = N32Blocks.FromBits(tape.n32); // NIGHT32: the NIGHT32 blocks the run read (formats 1-14: all off, NIGHT31's game)\n            sim.n33Override = N33Blocks.FromBits(tape.n33); // NIGHT33: the NIGHT33 blocks the run read (formats 1-15: all off, NIGHT32's game) ...\n            sim.shop33Owned = tape.shop33; // ... the shop's upgrades and the banked points it started with (formats 1-15: none, 0)\n            sim.bank33 = tape.bank33;\n"],
]);

patch("Assets/Scripts/Game/Tape/TapeScript.cs", [
  ["            N32Blocks? n32Saved = sim.n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) sim.n32Override = N32Blocks.Off;\n",
   "            N32Blocks? n32Saved = sim.n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (N32OnPath)\n            if (!FingerprintBaseline.N32OnPath) sim.n32Override = N32Blocks.Off;\n            N33Blocks? n33Saved = sim.n33Override; // NIGHT33: and every NIGHT33 block off until the freeze (N33OnPath); nothing bought or banked\n            if (!FingerprintBaseline.N33OnPath) sim.n33Override = N33Blocks.Off;\n            int shop33Saved = sim.shop33Owned, bank33Saved = sim.bank33;\n            sim.shop33Owned = sim.bank33 = 0;\n"],
  ["                sim.n32Override = n32Saved;\n",
   "                sim.n32Override = n32Saved;\n                sim.n33Override = n33Saved;\n                sim.shop33Owned = shop33Saved;\n                sim.bank33 = bank33Saved;\n"],
]);

patch("Tools/SimHarness/Program.cs", [
  ["        N32Blocks.ForceOff = off32;\n",
   "        N32Blocks.ForceOff = off32;\n        // NIGHT33: --off33 runs every command with every NIGHT33 design block off (NIGHT32's game); every older switch turns them off as well\n        bool off33 = off32 || Array.IndexOf(args, \"--off33\") >= 0;\n        N33Blocks.ForceOff = off33;\n"],
  ["                if (off32) sim.n32Override = N32Blocks.Off; // NIGHT32: the same\n",
   "                if (off32) sim.n32Override = N32Blocks.Off; // NIGHT32: the same\n                if (off33) sim.n33Override = N33Blocks.Off; // NIGHT33: the same\n"],
  ["        N32Crown(cmd, args, ref code); // NIGHT32 C: crown32 (THE CROWN's sweep and campers) (Harness.N32.Crown.cs)\n",
   "        N32Crown(cmd, args, ref code); // NIGHT32 C: crown32 (THE CROWN's sweep and campers) (Harness.N32.Crown.cs)\n" +
   "        N33Look(cmd, args, ref code); // NIGHT33 L: light33 (every light has a fixture) (Harness.N33.Look.cs)\n" +
   "        N33Kit(cmd, args, ref code); // NIGHT33 K: kit33 <level> (K1's 15 plan checks) (Harness.N33.Kit.cs)\n" +
   "        N33Arenas(cmd, args, ref code); // NIGHT33 R: arenas33 (rooms, cover, devices, walk 10/10) (Harness.N33.Arenas.cs)\n" +
   "        N33Enemies(cmd, args, ref code); // NIGHT33 E: ai33 (lava deaths), spawns33, bosses33 (Harness.N33.Enemies.cs)\n" +
   "        N33Weapons(cmd, args, ref code); // NIGHT33 W: whip33, pierce33 (Harness.N33.Weapons.cs)\n" +
   "        N33Shop(cmd, args, ref code); // NIGHT33 P: shop33 (the typical earn, prices, upgrades on the tape) (Harness.N33.Shop.cs)\n"],
  ["    static partial void N32Crown(string cmd, string[] args, ref int code);\n",
   "    static partial void N32Crown(string cmd, string[] args, ref int code);\n" +
   "    // NIGHT33: the builders' harness commands (a partial with no body costs nothing)\n" +
   "    static partial void N33Look(string cmd, string[] args, ref int code);\n" +
   "    static partial void N33Kit(string cmd, string[] args, ref int code);\n" +
   "    static partial void N33Arenas(string cmd, string[] args, ref int code);\n" +
   "    static partial void N33Enemies(string cmd, string[] args, ref int code);\n" +
   "    static partial void N33Weapons(string cmd, string[] args, ref int code);\n" +
   "    static partial void N33Shop(string cmd, string[] args, ref int code);\n"],
]);

patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["for (int o32 = 19; o32 <= 32; o32++) if (Array.IndexOf(args, \"-skygrindOff\" + o32) >= 0) N32Blocks.ForceOff = true; // NIGHT32: -skygrindOff32 and every older switch turn the NIGHT32 blocks off\n",
   "for (int o32 = 19; o32 <= 32; o32++) if (Array.IndexOf(args, \"-skygrindOff\" + o32) >= 0) N32Blocks.ForceOff = true; // NIGHT32: -skygrindOff32 and every older switch turn the NIGHT32 blocks off\n            for (int o33 = 19; o33 <= 33; o33++) if (Array.IndexOf(args, \"-skygrindOff\" + o33) >= 0) N33Blocks.ForceOff = true; // NIGHT33: -skygrindOff33 and every older switch turn the NIGHT33 blocks off\n"],
]);

patch("Assets/Scripts/Game/N29Fixes.cs", [
  ["a != \"-skygrindOff32\" && ", "a != \"-skygrindOff32\" && a != \"-skygrindOff33\" && "],
]);

patch("Assets/Tests/EditMode/Game/N32SharedTests.cs", [
  ["            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(15));\n",
   "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT33: the current format is 16; the n32 byte is read below\n"],
  ["            if (BitConverter.ToInt32(b15, RunTape.Magic.Length) < 15) return b15;\n",
   "            b15 = N33SharedTests.AsFormat15(b15); // NIGHT33: a format 16 tape first loses its NIGHT33 bytes\n            if (BitConverter.ToInt32(b15, RunTape.Magic.Length) < 15) return b15;\n"],
]);

patch("Assets/Scripts/Game/SurvivalGame.Campaign25.cs", [
  ["            string next = CampaignNextLevel25();\n            if (next == null)\n",
   "            string next = CampaignNextLevel25();\n            bool shopOpened = false;\n            N33ShopBeforeContinue(next, ref shopOpened); // NIGHT33 P: with design.n33.shop33 on, the SHOP opens first (its CONTINUE calls back here); nothing without a body (SurvivalGame.Shop33.cs)\n            if (shopOpened) return next;\n            if (next == null)\n"],
  ["        public static bool SkipSceneReload25;\n",
   "        public static bool SkipSceneReload25;\n\n        /// <summary>NIGHT33 P: the shop between levels. Sets `opened` when it showed the SHOP instead of continuing (the shop's own CONTINUE\n        /// calls ContinueCampaign again once it has been visited for this level).</summary>\n        partial void N33ShopBeforeContinue(string next, ref bool opened);\n"],
]);

patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| IsPhaseN31(s) || IsPhaseN32(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32: below\n",
   "|| IsPhaseN31(s) || IsPhaseN32(s) || IsPhaseN33(s); // NIGHT29: lantern29, glacier29 (FeelReel.N29.Bosses.cs); NIGHT30, NIGHT31, NIGHT32, NIGHT33: below\n\n" +
   "        // NIGHT33: the reel scenarios by owner; each owner's FeelReel.N33.<Owner>.cs implements its body (no body: NOT WRITTEN, RED)\n" +
   "        public static readonly string[] PhaseN33U = { \"hud33\" };\n" +
   "        public static readonly string[] PhaseN33L = { \"look33\" };\n" +
   "        public static readonly string[] PhaseN33K = { \"kit33\" };\n" +
   "        public static readonly string[] PhaseN33R = { \"arenas33\" };\n" +
   "        public static readonly string[] PhaseN33E = { \"ai33\" };\n" +
   "        public static readonly string[] PhaseN33W = { \"feel33\" };\n" +
   "        public static readonly string[] PhaseN33P = { \"shop33\" };\n" +
   "        static bool IsPhaseN33(string s) => Array.IndexOf(PhaseN33U, s) >= 0 || Array.IndexOf(PhaseN33L, s) >= 0 || Array.IndexOf(PhaseN33K, s) >= 0 || Array.IndexOf(PhaseN33R, s) >= 0 || Array.IndexOf(PhaseN33E, s) >= 0 || Array.IndexOf(PhaseN33W, s) >= 0 || Array.IndexOf(PhaseN33P, s) >= 0;\n" +
   "        static partial void N33HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33EnemiesBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n" +
   "        static partial void N33ShopBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n"],
  ["                    else if (Array.IndexOf(PhaseN32F, s) >= 0) N32FinaleBody(s, g, dir, res, offs, pre, ref body);\n",
   "                    else if (Array.IndexOf(PhaseN32F, s) >= 0) N32FinaleBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33U, s) >= 0) N33HudBody(s, g, dir, res, offs, pre, ref body); // NIGHT33: by owner\n" +
   "                    else if (Array.IndexOf(PhaseN33L, s) >= 0) N33LookBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33K, s) >= 0) N33KitBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33R, s) >= 0) N33ArenasBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33E, s) >= 0) N33EnemiesBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33W, s) >= 0) N33WeaponsBody(s, g, dir, res, offs, pre, ref body);\n" +
   "                    else if (Array.IndexOf(PhaseN33P, s) >= 0) N33ShopBody(s, g, dir, res, offs, pre, ref body);\n"],
]);

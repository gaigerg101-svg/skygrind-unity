// NIGHT45 shared commit (written by the NIGHT45 forge, dry-run in a scratch worktree before the night; GG 2026-10-17: "cut fluff"):
// a LEVEL-ONLY night needs no N4xBlocks, no tape bump and no rewrite of older shared tests. A tape already records the campaign's level
// id (RunTape format 8), so a replay loads n45-* by id; the n45 -> n43 base mapping is unconditional (an n45 id is only ever loaded when its
// row swapped it in), so it needs no per-run bit. What it adds: design.n45 (foundry45 / iceline45 / kiln45 level rows + the view row
// look45, all off), the three maps.list rows + placeholders (copies of n43-foundry / n43-iceline / n43-kiln, outer name only),
// Core/N45Levels.cs (ForceOff, Swap, BaseId), one line each in Curriculum25 / SurvivalSim.N43 (BaseId) / PlayerChecks / N29Fixes /
// SimHarness Program, Game/N45View.cs, Tests N45SharedTests, and the old order test forcing it off.
// Run from the repo root (or a worktree root): node Tools/night/n45-levels.cjs [rootDir]   (refuses to run twice; asserts every anchor)
const fs = require("fs");
const path = require("path");
const root = (process.argv[2] || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }
function writeNew(p, text) {
  if (fs.existsSync(R(p))) throw new Error(p + " exists");
  fs.writeFileSync(R(p), text);
  if (p.startsWith("Assets/")) fs.writeFileSync(R(p) + ".meta", "fileFormatVersion: 2\nguid: " + guid() + "\n");
  console.log("wrote " + p);
}
function patch(file, pairs) {
  let s = fs.readFileSync(R(file), "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 100));
  }
  for (const [a, b] of pairs) s = s.replace(a, () => b);
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(R(file), s);
  console.log("patched " + file);
}

// 1. config: design.n45 + maps.list rows + placeholders
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  let raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n45) throw new Error("design.n45 already exists");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const levels = [["n45-foundry", "LEVEL 1 THE FOUNDRY", "n43-foundry"], ["n45-iceline", "LEVEL 2 THE ICE LINE", "n43-iceline"], ["n45-kiln", "LEVEL 3 THE KILN", "n43-kiln"]];
  const anchorRe = /(\{\s*"id":\s*"n43-kiln",[^}]*\})/;
  const m = raw.match(anchorRe);
  if (!m) throw new Error("the n43-kiln maps.list row was not found");
  const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
  const add = levels.map(([id, label]) => "," + eol + indent + JSON.stringify({ id, label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
  raw = raw.replace(anchorRe, m[1] + add);
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const off = "Off until the lead has looked at its arena's frames (the identity card's landmark from the entrance, the 30 s problem in play) and its walk / gates pass (NIGHT45.md method).";
  const n45 = {
    _why: "NIGHT45, the arena identity night (GG 2026-10-17, verbatim in Research/2026-10-17-wave45/GG-REQUEST.md: 'if it arena related please dont just randomly plce stuff it needs intension and personality and lets cut fluff and old stuff'). Level rows only: each swaps an n43 level for its n45 rebuild (Core/N45Levels.cs); no sim bit and no tape change (a tape records the level id; n45 ids map to their n43 bases unconditionally). look45 is a view row (N45View). Off (--off45 / -skygrindOff45 or any older off switch): NIGHT44's game.",
    foundry45: { _why: "Builder A1: level 1's THE YARD (b1) rebuilt from its arena identity card (docs/DESIGN-RULES A-9), in n45-foundry (from n43-foundry; n43-foundry untouched).", enabled: false, enabled_why: off, levelFrom: "n43-foundry", levelTo: "n45-foundry" },
    iceline45: { _why: "Builder A2: level 2's THE MIX (g7) (and THE SWARM g3 if time) rebuilt from identity cards, in n45-iceline (from n43-iceline; untouched).", enabled: false, enabled_why: off, levelFrom: "n43-iceline", levelTo: "n45-iceline" },
    kiln45: { _why: "Builder A3: level 3's THE RING (k5) given a home: its 76 reasonless props cut or given a job, its identity card, the worm's routes kept; in n45-kiln (from n43-kiln; untouched).", enabled: false, enabled_why: off, levelFrom: "n43-kiln", levelTo: "n45-kiln" },
    look45: { _why: "Builder A3 (view): a room with its own landmark (level data) draws it instead of look43's generic exit pylons (DESIGN-RULES A-6, A-9).", enabled: false, enabled_why: off },
  };
  const block = JSON.stringify(n45, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n45\": " + block + eol + "  }" + eol + "}";
  JSON.parse(out);
  for (const [id, , from] of levels) {
    for (const dir of ["Design/levels/", "Assets/StreamingAssets/Design/levels/"]) {
      const dst = R(dir + id + ".json");
      if (fs.existsSync(dst)) throw new Error(dst + " exists");
      let lv = fs.readFileSync(R("Design/levels/" + from + ".json"), "utf8");
      const key = '"name":"' + from + '"';
      const n = lv.split(key).length - 1;
      if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
      lv = lv.split(key).join('"name":"' + id + '"');
      if (JSON.parse(lv).name !== id) throw new Error(dst + ": the name did not change");
      fs.writeFileSync(dst, lv);
    }
  }
  fs.writeFileSync(file, out);
  fs.copyFileSync(file, copy);
  console.log("design.n45 added; maps.list + placeholders: " + levels.map((l) => l[0]).join(", "));
}

// 2. new files
writeNew("Assets/Scripts/Core/N45Levels.cs", `// NIGHT45 (the shared commit; Tools/night/n45-levels.cjs): the level-only rows foundry45, iceline45, kiln45 (design.n45.<row>: enabled,
// levelFrom, levelTo). No sim bit: a tape records the level id it loaded, and an n45 id maps to its base unconditionally (it is only ever
// loaded when its row swapped it in). ForceOff (-skygrindOff45 / --off45 and every older off switch) leaves NIGHT44's order.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class N45Levels
    {
        /// <summary>The level rows in design.n45.</summary>
        public static readonly string[] Rows = { "foundry45", "iceline45", "kiln45" };

        /// <summary>Every NIGHT45 level row off, whatever the config says (the harnesses' and the Player checks' switch).</summary>
        public static bool ForceOff;

        /// <summary>After Curriculum25.N43Levels: each row on swaps its levelFrom for its levelTo.</summary>
        public static void Swap(GameConfig cfg, List<string> res)
        {
            if (ForceOff || res.Count == 0) return;
            foreach (string row in Rows)
            {
                if (!cfg.Design("n45." + row + ".enabled")) continue;
                string from = cfg.Has("design.n45." + row + ".levelFrom") ? cfg.Get("design.n45." + row + ".levelFrom") as string : null;
                string to = cfg.Has("design.n45." + row + ".levelTo") ? cfg.Get("design.n45." + row + ".levelTo") as string : null;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        /// <summary>The id older systems know an n45 level by: its row's levelFrom (unconditional); any other id is itself.</summary>
        public static string BaseId(GameConfig cfg, string levelId)
        {
            if (levelId == null || cfg == null || !levelId.StartsWith("n45-", System.StringComparison.Ordinal)) return levelId;
            foreach (string row in Rows)
            {
                string to = cfg.Has("design.n45." + row + ".levelTo") ? cfg.Get("design.n45." + row + ".levelTo") as string : null;
                if (to == levelId) return cfg.Has("design.n45." + row + ".levelFrom") ? cfg.Get("design.n45." + row + ".levelFrom") as string : levelId;
            }
            return levelId;
        }
    }
}
`);
writeNew("Assets/Scripts/Game/N45View.cs", `// NIGHT45 (the shared commit): the NIGHT45 view row's switch (look45). On when design.n45.look45 is enabled and the session is not
// -skygrindOff45 (or any older off switch, or N45Levels.ForceOff). The command line is read here directly, once (NIGHT40's pump40 lesson).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class N45View
    {
        static int cliOff = -1;

        public static bool Off
        {
            get
            {
                if (N45Levels.ForceOff) return true;
                if (cliOff < 0)
                {
                    cliOff = 0;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                            for (int n = 19; n <= 45; n++)
                                if (a == "-skygrindOff" + n) cliOff = 1;
                    }
                    catch (Exception) { cliOff = 0; }
                }
                return cliOff == 1;
            }
        }

        public static bool RowOn(GameConfig cfg, string row) => cfg != null && !Off && cfg.Design("n45." + row + ".enabled");
    }
}
`);
writeNew("Assets/Tests/EditMode/Game/N45SharedTests.cs", `// NIGHT45 (the shared commit): the level-only rows. With N45Levels forced off the order is NIGHT44's; every row has its design row and its
// level file (both copies); an n45 id maps to its n43 base, any other id is itself.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N45SharedTests
    {
        static GameConfig cfg;
        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "PortExport", "config.json"), Path.Combine(Root, "Design", "unity-config.json"));

        [Test]
        public void TheRows_AreListed_WithTheirLevels()
        {
            var c = Cfg;
            foreach (string row in N45Levels.Rows)
            {
                Assert.That(c.Has("design.n45." + row + ".enabled"), Is.True, row);
                string to = c.Str("design.n45." + row + ".levelTo");
                Assert.That(File.Exists(Path.Combine(Root, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Root, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(c.Str("design.n45." + row + ".levelFrom")), row + "'s base");
            }
            Assert.That(N45Levels.BaseId(c, "n43-foundry"), Is.EqualTo("n43-foundry"));
        }

        [Test]
        public void TheSwap_OnlyWhenOn()
        {
            var c = Cfg;
            var order = new List<string> { "n43-foundry", "n43-iceline", "n43-kiln", "n40-span", "n41-spire" };
            bool was = N45Levels.ForceOff;
            try
            {
                N45Levels.ForceOff = true;
                var forced = new List<string>(order);
                N45Levels.Swap(c, forced);
                Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT44's order");
                N45Levels.ForceOff = false;
                var on = new List<string>(order);
                N45Levels.Swap(c, on);
                foreach (string row in N45Levels.Rows)
                {
                    int at = order.IndexOf(c.Str("design.n45." + row + ".levelFrom"));
                    Assert.That(on[at], Is.EqualTo(c.Design("n45." + row + ".enabled") ? c.Str("design.n45." + row + ".levelTo") : order[at]), row);
                }
            }
            finally { N45Levels.ForceOff = was; }
        }
    }
}
`);

// 3. one-line hooks
patch("Assets/Scripts/Core/Curriculum25.cs", [["            N43Levels(cfg, res);\n", "            N43Levels(cfg, res);\n            N45Levels.Swap(cfg, res); // NIGHT45: the level-only rows (Core/N45Levels.cs)\n"]]);
patch("Assets/Scripts/Game/SurvivalSim.N43.cs", [
  ["        public string N43BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n",
   "        public string N43BaseId(string levelId)\n        {\n            if (levelId == null) return null;\n            levelId = N45Levels.BaseId(cfg, levelId); // NIGHT45 (the lead's hook): an n45 level is its n43 base first (unconditional; Core/N45Levels.cs)\n"],
]);
patch("Assets/Scripts/Game/PlayerChecks.cs", [
  ["            for (int o43 = 19; o43 <= 43; o43++) if (Array.IndexOf(args, \"-skygrindOff\" + o43) >= 0) N43Blocks.ForceOff = true;",
   "            for (int o45 = 19; o45 <= 45; o45++) if (Array.IndexOf(args, \"-skygrindOff\" + o45) >= 0) N45Levels.ForceOff = true; // NIGHT45: -skygrindOff45 and every older switch: NIGHT44's level order\n            for (int o43 = 19; o43 <= 43; o43++) if (Array.IndexOf(args, \"-skygrindOff\" + o43) >= 0) N43Blocks.ForceOff = true;"],
]);
patch("Assets/Scripts/Game/N29Fixes.cs", [["a != \"-skygrindOff43\" && ", "a != \"-skygrindOff43\" && a != \"-skygrindOff44\" && a != \"-skygrindOff45\" && "]]);
patch("Tools/SimHarness/Program.cs", [
  ["        N43Blocks.ForceOff = off43;\n",
   "        N43Blocks.ForceOff = off43;\n        // NIGHT45: --off45 (and --off44, and every older switch): NIGHT44's level order (the level-only rows off)\n        N45Levels.ForceOff = off43 || Array.IndexOf(args, \"--off44\") >= 0 || Array.IndexOf(args, \"--off45\") >= 0;\n"],
]);
// the five direct n43-id checks (the NIGHT45 forge's reviewer): each now asks the n45 -> n43 base, so an n45 level keeps level 3's boost
// lessons, the level 1 walk pilot and the NIGHT43 reels (with the n45 rows off nothing changes: an n45 id is never loaded)
patch("Assets/Scripts/Game/SurvivalSim.N43.Kiln.cs", [
  ["campaign.room < 0 || campaignLevel.id != k43LevelTo) return;", "campaign.room < 0 || N45Levels.BaseId(cfg, campaignLevel.id) != k43LevelTo) return; // NIGHT45: n45-kiln is n43-kiln here"],
]);
patch("Assets/Scripts/Game/Bot/Walk43Foundry.cs", [
  ["if (sim.campaignLevel == null || sim.campaignLevel.id != \"n43-foundry\") return false;", "if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) != \"n43-foundry\") return false; // NIGHT45: n45-foundry walks as n43-foundry"],
]);
patch("Assets/Scripts/Game/FeelReel.N43.Kiln.cs", [
  ["if (sim.campaignLevel == null || sim.campaignLevel.id != \"n43-kiln\") {", "if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) != \"n43-kiln\") { // NIGHT45: n45-kiln too\n               "],
]);
patch("Assets/Scripts/Game/FeelReel.N43.Rooms.cs", [
  ["if (g.cfg.Has(key) && g.cfg.Str(key) == g.MapId) bits |=", "if (g.cfg.Has(key) && g.cfg.Str(key) == N45Levels.BaseId(g.cfg, g.MapId)) bits |="],
  ["if (g.MapId == (g.cfg.Has(\"design.n43.iceline43.levelTo\")", "if (N45Levels.BaseId(g.cfg, g.MapId) == (g.cfg.Has(\"design.n43.iceline43.levelTo\")"],
]);

// the arena builders' reel hooks, pre-placed (L-050): one partial per builder in its own file, so FeelReel.N27.cs is touched once tonight
writeNew("Assets/Scripts/Game/FeelReel.N45.cs", `// NIGHT45 (the shared commit): the arena builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the lead's). Each builder
// fills its own partial in its own file: a145 (A1: FeelReel.N45.Yard.cs), a245 (A2: FeelReel.N45.Mix.cs), a345 (A3: FeelReel.N45.Ring.cs).
// A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN45 = { "a145", "a245", "a345" };

        static void N45ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a145") N45Yard(g, dir, res, offs, pre, ref body);
            else if (s == "a245") N45Mix(g, dir, res, offs, pre, ref body);
            else if (s == "a345") N45Ring(g, dir, res, offs, pre, ref body);
        }

        static partial void N45Yard(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A1
        static partial void N45Mix(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A2
        static partial void N45Ring(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A3
    }
}
`);
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN44V2, s) >= 0;", "|| Array.IndexOf(PhaseN44V2, s) >= 0 || Array.IndexOf(PhaseN45, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN44V2, s) >= 0) N44Look(g, dir, res, offs, pre, ref body); // NIGHT44 V2 (FeelReel.N44.Look.cs)\n",
   "                    else if (Array.IndexOf(PhaseN44V2, s) >= 0) N44Look(g, dir, res, offs, pre, ref body); // NIGHT44 V2 (FeelReel.N44.Look.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN45, s) >= 0) N45ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT45 A1 / A2 / A3 (FeelReel.N45.cs)\n"],
]);

patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  ["            N43Blocks.ForceOff = true; // NIGHT43 (L-027):",
   "            N45Levels.ForceOff = true; // NIGHT45 (L-027): the n45 level rows pin nothing older\n            N43Blocks.ForceOff = true; // NIGHT43 (L-027):"],
]);
console.log("NIGHT45 level-only shared commit done. Next: the Editor compile, golden, fingerprint plain and --off45, N45SharedTests + Lesson25.");

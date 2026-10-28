// NIGHT52 shared commit (the lead's; modeled on n48-shared.cjs for the level rows and n50-shared.cjs for the view rows; NIGHT52.md section 3
// step 3). Three builders: K = level 3's rooms (kiln52), F = level 1's rooms (foundry52), U = the polish lane (hud52, skin52, guns52); the
// lead's room loop takes iceline52 / spire52 / span52. All five level rows exist now so the loop never needs a second config commit (L-049, L-065).
//   node Tools/night/n52-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n52 (five level rows + three view rows, every one enabled:false with a _why), five
//      design.maps.list rows after n48-foundry's, and design.n49.look49.numbers.levels gains n52-foundry / n52-spire beside n48-foundry /
//      n47-spire (the grade follows its level's levelFrom: with the rows off nothing loads an n52 id, so NIGHT51's game is unchanged).
//   2. Five placeholder levels (byte copies of n47-kiln, n48-foundry, n47-iceline, n47-spire, n47-span with only "name" changed), both copies,
//      + the StreamingAssets .meta.
//   3. Assets/Scripts/Game/SurvivalSim.N52.cs: N52LevelFrom (an n52 level id -> its levelFrom), used by the hook below.
//   4. SurvivalSim.N47.Kiln.cs: kiln47's beats (G35: the glimpse, the pass, the cameo) were keyed on the raw id "n47-kiln"; n52-kiln keeps them.
//   5. Assets/Scripts/Game/FeelReel.N52.cs: reel phases a152 (U hud52), a252 (U skin52), a352 (U guns52), a452 (K: the room frames reel, any
//      level: each room row's frames52 spots: entry, mid-fight, the highest tier), routed from FeelReel.N27.cs.
//   6. Assets/Tests/EditMode/N52SharedTests.cs: the rows exist and are off; every row off = NIGHT51's order; --off52 = NIGHT51's order; each n52
//      level reads as its levelFrom's base; the look49 list follows the levelFrom; N50SharedTests' campaign list gains the n52 ids.
//   7. Tools/night/n52-rows.json (every row passes:false).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/SurvivalSim.N48.cs"), "utf8").includes("\r\n");
const csEol = (t) => (crlfCs ? t.replace(/\r?\n/g, "\r\n") : t);
function addNew(p, text, meta) {
  if (fs.existsSync(R(p))) throw new Error(p + " exists (already run?)");
  writes.push([p, text]);
  if (p.startsWith("Assets/")) {
    if (fs.existsSync(R(p) + ".meta")) throw new Error(p + ".meta exists (already run?)");
    writes.push([p + ".meta", meta || csEol("fileFormatVersion: 2\nguid: " + guid() + "\n")]);
  }
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
  writes.push([file, s]);
}

const GG = "GG 2026-10-19 ~22:00 (NIGHT52.md): an 8 h night, honest and critical about where the game still needs to improve and polish, built and iterated. NIGHT52 answers GG's standing asks on the rooms (every level needs a complete arena overhaul; smaller, more action packed, like ULTRAKILL; intention and personality; a route enemies can climb) with THE ROOM CONTRACT (NIGHT52.md section 2), and his HUD ask ('looks so AI generated') with the polish lane";
const off = "Off until its check in Tools/night/n52-rows.json passes (printed by the lead) and the freeze gates pass (NIGHT52.md section 7: rows ON at the freeze by their checks; verbs37 on the five levels).";
const levels = [ // [row, levelFrom, levelTo, why]
  ["kiln52", "n47-kiln", "n52-kiln", "Builder K (level row): level 3's combat rooms kb2, kb2c and kb3 rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2: 20-32 m, >= 2.5 pieces per 100 m2, 3 tiers enemies reach, a circuit, a 17 m ride wall, an upward line, no camping spot) in n52-kiln (from n47-kiln, untouched); the boss room k5 untouched; links re-cut only where a moved door needs it."],
  ["foundry52", "n48-foundry", "n52-foundry", "Builder F (level row): level 1's combat rooms a5 (1.5 CRUCIBLE) and b1 (1.6 THE YARD) rebuilt to THE ROOM CONTRACT in n52-foundry (geometry from Design/levels/n46/foundry.plan.json, a9's n47 / n48 data re-applied bit-equal; n48-foundry untouched); the teach rooms and the boss a9 keep their lessons."],
  ["iceline52", "n47-iceline", "n52-iceline", "The room loop (NIGHT52.md section 5; the lead, then K / F): level 2's lowest-density combat rooms rebuilt one a cycle to THE ROOM CONTRACT in n52-iceline (from n47-iceline, untouched)."],
  ["spire52", "n47-spire", "n52-spire", "The room loop (NIGHT52.md section 5): level 5's lowest-density combat rooms rebuilt one a cycle to THE ROOM CONTRACT in n52-spire (from n47-spire, untouched)."],
  ["span52", "n47-span", "n52-span", "The room loop (NIGHT52.md section 5): level 4's lowest-density combat rooms rebuilt one a cycle to THE ROOM CONTRACT in n52-span (from n47-span, untouched)."],
];
const viewRows = [ // [row, why]
  ["hud52", "Builder U (the polish lane, section 4): the health and charge block rebuilt: health in the palette (bone / cyan, red-orange only below 30 %), a hard-edged ULTRAKILL-style panel with a hit shake and a heal pulse; the weapon block in the same family. Off = NIGHT51's HUD."],
  ["skin52", "Builder U: the enemy surface: the rim cut close up (the jelly read), a ramp-lit body, ink51's outline gaps on flat-shaded corners closed. Off = NIGHT51's enemies."],
  ["guns52", "Builder U: the KESTREL and THUNDERCLAP viewmodels: chamfered parts, two-tone panels, a lit edge; no new silhouette (GG keeps the guns' shapes). Off = NIGHT51's guns."],
];

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  let raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n52) throw new Error("design.n52 already exists (already run?)");
  if (!d.design.n50) throw new Error("design.n50 is missing");
  if (d.design.n51) throw new Error("design.n51 exists: NIGHT51 had no config rows (re-read)");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
  for (const [row, from, to] of levels) {
    if (!listed[from]) throw new Error(row + ": its levelFrom " + from + " is not in design.maps.list");
    if (listed[to]) throw new Error(row + ": " + to + " is already in design.maps.list");
  }
  // 1a. maps.list rows after n48-foundry's
  const anchorRe = /(\{\s*"id":\s*"n48-foundry",[^}]*\})/;
  const m = raw.match(anchorRe);
  if (!m || raw.split('"id": "n48-foundry"').length - 1 !== 1) throw new Error("the n48-foundry maps.list row was not found exactly once");
  const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
  const add = levels.map(([, from, id]) => "," + eol + indent + JSON.stringify({ id, label: listed[from].label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
  raw = raw.replace(anchorRe, m[1] + add);
  // 1b. look49's level list: each n52 level beside its levelFrom
  const lookOld = '"levels": ["n48-foundry", "n47-spire"]';
  if (raw.split(lookOld).length - 1 !== 1) throw new Error("look49's levels list was not found exactly once as " + lookOld);
  raw = raw.replace(lookOld, '"levels": ["n48-foundry", "n47-spire", "n52-foundry", "n52-spire"]');
  // 1c. design.n52 after n50 (n50 must be the last key of design)
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending: design.n50 must be the last key of design");
  const n52 = { _why: GG };
  for (const [row, from, to, why] of levels) n52[row] = { _why: why, enabled: false, enabled_why: off, levelFrom: from, levelTo: to };
  for (const [row, why] of viewRows) n52[row] = { _why: "View row (NightRows.ViewOn(cfg, \"" + row + "\")). " + why, enabled: false, enabled_why: off, numbers: {} };
  const block = JSON.stringify(n52, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n52\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n52) !== JSON.stringify(n52)) throw new Error("design.n52 did not round-trip");
  for (const k of Object.keys(d.design)) {
    if (k === "maps") continue;
    if (k === "n49") {
      const a = JSON.parse(JSON.stringify(d.design.n49)), b = JSON.parse(JSON.stringify(back.design.n49));
      a.look49.numbers.levels = b.look49.numbers.levels = null;
      if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("design.n49 changed beyond look49.levels");
      if (JSON.stringify(back.design.n49.look49.numbers.levels) !== JSON.stringify(["n48-foundry", "n47-spire", "n52-foundry", "n52-spire"])) throw new Error("look49.levels");
      continue;
    }
    if (JSON.stringify(back.design[k]) !== JSON.stringify(d.design[k])) throw new Error("design." + k + " changed");
  }
  for (const k of Object.keys(d.design.maps)) if (k !== "list" && JSON.stringify(back.design.maps[k]) !== JSON.stringify(d.design.maps[k])) throw new Error("design.maps." + k + " changed");
  if (back.design.maps.list.length !== d.design.maps.list.length + levels.length) throw new Error("maps.list did not gain " + levels.length + " rows");
  for (const k of Object.keys(d)) if (k !== "design" && JSON.stringify(back[k]) !== JSON.stringify(d[k])) throw new Error(k + " changed");
  for (const [, from, id] of levels) {
    const src = fs.readFileSync(R("Design/levels/" + from + ".json"), "utf8");
    if (src !== fs.readFileSync(R("Assets/StreamingAssets/Design/levels/" + from + ".json"), "utf8")) throw new Error(from + ": Design/levels and its StreamingAssets copy differ");
    const key = '"name":"' + from + '"';
    const n = src.split(key).length - 1;
    if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
    const lv = src.split(key).join('"name":"' + id + '"');
    if (JSON.parse(lv).name !== id) throw new Error(id + ": the name did not change");
    if (lv.length !== src.length) throw new Error(id + ": the placeholder's length differs (the ids must be the same length)");
    addNew("Design/levels/" + id + ".json", lv);
    addNew("Assets/StreamingAssets/Design/levels/" + id + ".json", lv,
      "fileFormatVersion: 2\r\nguid: " + guid() + "\r\nDefaultImporter:\r\n  externalObjects: {}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n");
  }
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the n52 level helper
addNew("Assets/Scripts/Game/SurvivalSim.N52.cs", csEol(`// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): tonight has no sim row. The five level rows (design.n52.kiln52, foundry52, iceline52,
// spire52, span52) swap a level for its n52 copy (data: Curriculum25 / NightRows.SwapLevels); older systems read an n52 id by NightRows.BaseId.
// A system that keyed on a raw level id (kiln47's beats: SurvivalSim.N47.Kiln.cs) asks N52LevelFrom, so the n52 copy keeps it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The level an n52 level row swaps out (its levelFrom) for an n52 id; any other id is returned as is (no lookup: an id that
        /// does not start with "n52-" never reads the config, so every older level costs one string compare).</summary>
        internal string N52LevelFrom(string levelId)
        {
            if (levelId == null || !levelId.StartsWith("n52-", StringComparison.Ordinal) || cfg == null || !cfg.Has("design.n52")) return levelId;
            foreach (KeyValuePair<string, object> kv in cfg.Obj("design.n52"))
                if (kv.Value is Dictionary<string, object> row && row.TryGetValue("levelTo", out object to) && to as string == levelId && row.TryGetValue("levelFrom", out object from) && from is string f)
                    return f;
            return levelId;
        }
    }
}
`));

// 3. kiln47's beats on n52-kiln
patch("Assets/Scripts/Game/SurvivalSim.N47.Kiln.cs", [[
  '            k47On = w47On && campaignLevel != null && campaignLevel.id == "n47-kiln" && campaign != null;\n',
  '            k47On = w47On && campaignLevel != null && (campaignLevel.id == "n47-kiln" || N52LevelFrom(campaignLevel.id) == "n47-kiln") && campaign != null; // NIGHT52 (n52-shared.cjs): n52-kiln keeps the beats\n',
]]);

// 4. the reel hooks
addNew("Assets/Scripts/Game/FeelReel.N52.cs", csEol(`// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// a152 (U: FeelReel.N52.Hud.cs, hud52), a252 (U: FeelReel.N52.Skin.cs, skin52), a352 (U: FeelReel.N52.Guns.cs, guns52), a452 (K:
// FeelReel.N52.Rooms.cs, the room frames reel: on any level, each room row's frames52 spots {entry, mid, high} as [x, y, z, yawDeg, pitch],
// the mid frame with the room's fight live; F and the loop add frames52 to their rooms and use it). A partial with no body leaves body null
// (res[0] stays NOT WRITTEN, RED). View rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs <tag> on n52.<row>)
// or by a reel-only switch the builder reads; a campaign level boots only via reel.sh + -skygrindMap <id> -skygrindReelOnly <phase> (L-062).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN52 = { "a152", "a252", "a352", "a452" };

        static void N52ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a152") N52Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a252") N52Skin(g, dir, res, offs, pre, ref body);
            else if (s == "a352") N52Guns(g, dir, res, offs, pre, ref body);
            else if (s == "a452") N52Rooms(g, dir, res, offs, pre, ref body);
        }

        static partial void N52Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Skin(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Guns(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Rooms(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K (F and the loop use it)
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN50, s) >= 0;", "|| Array.IndexOf(PhaseN50, s) >= 0 || Array.IndexOf(PhaseN52, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN50, s) >= 0) N50ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT50 H (FeelReel.N50.cs)\n",
   "                    else if (Array.IndexOf(PhaseN50, s) >= 0) N50ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT50 H (FeelReel.N50.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN52, s) >= 0) N52ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT52 U / K (FeelReel.N52.cs)\n"],
]);

// 5. the shared tests (+ N50SharedTests' campaign list)
patch("Assets/Tests/EditMode/N50SharedTests.cs", [[
  'string[] campaign = { "n48-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire" };',
  'string[] campaign = { "n48-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire", "n52-foundry", "n52-iceline", "n52-kiln", "n52-span", "n52-spire" }; // NIGHT52 (n52-shared.cjs): the n52 copies',
]]);
addNew("Assets/Tests/EditMode/N52SharedTests.cs", csEol(`// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n52 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the level and view rows carry no tape bit; ViewOn reads the view rows and --off52 turns them off; each level row's files exist
//      (both copies) and are listed
//   2. with every n52 row off, the campaign order is NIGHT51's (the config without design.n52)
//   3. the off switch: ForceOffFrom 52 (-skygrindOff52 / --off52) with every n52 level row ON leaves NIGHT51's order; 53 and no switch swap
//   4. every n52 level reads as its levelFrom's base to every older system; the look49 grade list holds an n52 id exactly when it holds its levelFrom
// Switches() clears / restores N38-N43Blocks.ForceOff and N45Levels.ForceOff from the start: Lesson25Tests leaves them set (L-082).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N52SharedTests
    {
        static readonly string[] LevelRows = { ${levels.map(([r]) => '"' + r + '"').join(", ")} };
        static readonly string[] ViewRows = { ${viewRows.map(([r]) => '"' + r + '"').join(", ")} };

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N52(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n52"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N52(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN52()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n52");
            return c;
        }

        static List<string> Order(GameConfig c) => Curriculum25.Order(c, N25Blocks.From(c));

        static void Switches(int offFrom, System.Action body)
        {
            int was = NightRows.ForceOffFrom;
            bool was45 = N45Levels.ForceOff;
            bool w38 = N38Blocks.ForceOff, w39 = N39Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                N45Levels.ForceOff = false;
                // Lesson25Tests leaves these set (no restore); the orders here need them as the config says (L-082)
                N38Blocks.ForceOff = N39Blocks.ForceOff = N40Blocks.ForceOff = N41Blocks.ForceOff = N42Blocks.ForceOff = N43Blocks.ForceOff = false;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                N45Levels.ForceOff = was45;
                N38Blocks.ForceOff = w38; N39Blocks.ForceOff = w39; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43;
            }
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n52._why"), Is.True, "design.n52._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            foreach (string row in all)
            {
                string p = "design.n52." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n52." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in all) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " has no tape bit (level and view rows are data)");
            foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(c.Design("n52." + row + ".enabled")), row + ": ViewOn reads design.n52");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 52; // -skygrindOff52 / --off52
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off52: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n52." + row + ".levelTo"), from = c.Str("design.n52." + row + ".levelFrom");
                Assert.That(to.StartsWith("n52-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight51sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night51 = Order(WithoutN52());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night51), "every n52 level row off: NIGHT51's order (" + string.Join(", ", night51.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n52." + row + ".levelFrom");
                    Assert.That(night51.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT51's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight51sOrder()
        {
            List<string> night51 = null;
            Switches(NightRows.NoOff, () => { night51 = Order(WithoutN52()); });
            Switches(52, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night51), "ForceOffFrom 52 (--off52) with every n52 level row on: NIGHT51's order"));
            foreach (int from in new[] { 53, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night51.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night51.IndexOf(c.Str("design.n52." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == c.Str("design.n52." + row + ".levelTo"), Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN52Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            var look = c.Get("design.n49.look49.numbers.levels") as List<object>;
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n52." + row + ".levelFrom"), to = c.Str("design.n52." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
                Assert.That(look.Contains(to), Is.EqualTo(look.Contains(from)), row + ": the look49 grade list holds " + to + " exactly when it holds " + from);
            }
        }
    }
}
`));

// 6. the rows (the lead flips passes; check is never edited)
addNew("Tools/night/n52-rows.json", JSON.stringify({
  _why: "NIGHT52 row list (the lead's; written at P0 by Tools/night/n52-shared.cjs). One entry per row of NIGHT52.md section 1. Only the lead flips passes, only after printing the check's output in the transcript; `check` is never edited. `state`, `evidence_paths` and `note` are the close's own fields. Builders only report.",
  rows: [
    { id: "P0", title: "Baselines + the shared commit", owner: "lead", switch: "-", check: "density, flow table, verbs37 and campaign clear rates printed for kiln and foundry; the shared commit's gates green", fallback: "none", passes: false },
    { id: "K", title: "Level 3, THE KILN, rebuilt rooms (kb2, kb2c, kb3 to the ROOM CONTRACT; the boss room untouched; links re-cut only where a moved door needs it)", owner: "builder K", switch: "design.n52.kiln52 (n47-kiln -> n52-kiln)", check: "every rebuilt room passes the ROOM CONTRACT (section 2) with its numbers printed; verbs37 25/25 on n52-kiln; levelgen report clean; the bot clears every room 10/10 seeds eventually, first-try 50-95 %", fallback: "each rebuilt room that passes ships in n52-kiln; a room that misses the contract twice is put back to its n47 version inside n52-kiln; SHORT with 'N of 3 rooms rebuilt'; the row ON at the freeze only if >= 1 room was rebuilt and verbs37 holds", passes: false },
    { id: "F", title: "Level 1, THE FOUNDRY, rebuilt combat rooms a5 (1.5 CRUCIBLE) and b1 (1.6 THE YARD); teach rooms and the boss a9 exempt", owner: "builder F", switch: "design.n52.foundry52 (n48-foundry -> n52-foundry)", check: "the same for a5 and b1, verbs37 26/26 on n52-foundry, a9 bit-equal to n48-foundry's", fallback: "as K: N of 2 rooms rebuilt; a room missing twice reverts to its n46 / n48 version inside n52-foundry", passes: false },
    { id: "U", title: "The polish lane (hud52, skin52, guns52)", owner: "builder U", switch: "design.n52.hud52, skin52, guns52 (view rows)", check: "each item: before / after frames at 1080p looked at by the lead and logged, its Player proof line RED once and GREEN once (L-014), nothing in verbs37 lost", fallback: "hud52: the colour change only; skin52: the rim's near cut only; guns52: KESTREL only", passes: false },
    { id: "L", title: "The room loop (the other levels' worst rooms, one room a cycle)", owner: "lead, then K and F after their rows", switch: "design.n52.iceline52, spire52, span52", check: "per room: the ROOM CONTRACT printed and passing; the level's verbs37 unchanged", fallback: "a room missing twice is reverted and logged SHORT; a level row ON only if >= 1 room rebuilt and verbs37 holds", passes: false },
    { id: "R", title: "The persona reviews (the P-rank runner and GG, fresh read-only subagents)", owner: "fresh subagents", switch: "-", check: "each level row gets the two reviews; every concrete finding fixed or answered in DECIDE52", fallback: "SHORT if a review did not run, with the reason", passes: false },
    { id: "Z", title: "Quiet freeze and close", owner: "lead", switch: "-", check: "NIGHT52.md section 8 / 11: rows ON by their checks; n52-freeze; verbs37 x5 (26 / 20 / 25 / 30 / 30, nothing lost); the flow table on the changed levels; the full batch suite green; the smoke (the controller's state logged); golden / fingerprints EQUAL; the report with density before / after and the building share; tag u3-n52", fallback: "whatever is unfinished at 7:30 closes SHORT with its reason", passes: false },
  ],
}, null, 2) + "\n");

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT52 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprint plain / --off50 / --off52, the Editor compile, N52SharedTests + N50SharedTests + N48SharedTests.");

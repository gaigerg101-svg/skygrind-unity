// NIGHT57 shared commit (the lead's; modeled on n56-shared.cjs; NIGHT57.md section 4). SHIP AND DEEPEN: the rows of Logs/n57/rows-def.json
// (design.n57.<row>), each off: press57 (sim, G) with a numbers object its builder fills (L-049 / L-065), and kiln57 (L), a LEVEL row
// (levelFrom n52-kiln, levelTo n57-kiln, as design.n52.kiln52) with no numbers. `-skygrindOff57` / `--off57` gives NIGHT56's game.
//   node Tools/night/n57-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n57 = { _why, <rows> }, all enabled:false.
//   2. Assets/Scripts/Game/FeelReel.N57.cs: reel phases a157 (L level 3), a257 (G enemy pressure / kills), a357 (V colour / near misses),
//      routed from FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N57SharedTests.cs: the rows exist, off by default (or ON with an "ON" reason), the sim row has a tape bit,
//      --off57 / --off56 turn press57 off, kiln57 is the only level row and, off, leaves the campaign order NIGHT56's.
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N56.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-21 (NIGHT57.md, verbatim there): he cannot play now; find where the game is missing things (Research/2026-10-21-wave57/GAPS.md); his calls: the shipping rule is the lead's, enemy attack behaviour may change, level 3 rebuilt from scratch. NIGHT57: SHIP AND DEEPEN.";
const off = "Off until its slate entry (Logs/n57/slate.json) passes NIGHT57.md section 0's rule and the freeze gates pass.";
const rows = JSON.parse(fs.readFileSync(R("Logs/n57/rows-def.json"), "utf8")).rows;

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n57) throw new Error("design.n57 already exists (already run?)");
  if (Object.keys(d.design).pop() !== "n56") throw new Error("design.n56 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n57 = { _why: GG };
  for (const r of rows) {
    const row = {};
    if (r.sim) row.sim = true;
    Object.assign(row, { _why: r.why, enabled: false, enabled_why: off });
    if (r.levelFrom) Object.assign(row, { levelFrom: r.levelFrom, levelTo: r.levelTo });
    else row.numbers = {};
    n57[r.id] = row;
  }
  const block = JSON.stringify(n57, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  let out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n57\": " + block + eol + "  }" + eol + "}";
  // 1b. maps.list: each level row's levelTo listed after its levelFrom's row (the red team: an unlisted level ends the campaign,
  //     SurvivalGame.Campaign25.cs: CAMPAIGN COMPLETE), as n52-shared.cjs step 1a
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
  const levelRows = rows.filter((r) => r.levelFrom);
  for (const r of levelRows) {
    if (!listed[r.levelFrom]) throw new Error(r.id + ": its levelFrom " + r.levelFrom + " is not in design.maps.list");
    if (listed[r.levelTo]) throw new Error(r.id + ": " + r.levelTo + " is already in design.maps.list");
    const re = new RegExp('(\\{\\s*"id":\\s*"' + r.levelFrom + '",[^}]*\\})');
    const m = out.match(re);
    if (!m || out.split('"id": "' + r.levelFrom + '"').length - 1 !== 1) throw new Error("the " + r.levelFrom + " maps.list row was not found exactly once");
    const indent = out.slice(out.lastIndexOf(eol, m.index) + eol.length, m.index);
    const rowText = JSON.stringify({ id: r.levelTo, label: listed[r.levelFrom].label, file: "Design/levels/" + r.levelTo + ".json", campaignOnly: true })
      .replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"');
    out = out.replace(re, m[1] + "," + eol + indent + rowText);
  }
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n57) !== JSON.stringify(n57)) throw new Error("design.n57 did not round-trip");
  if (back.design.maps.list.length !== d.design.maps.list.length + levelRows.length) throw new Error("maps.list did not gain " + levelRows.length + " row(s)");
  const a = JSON.parse(out); delete a.design.n57;
  a.design.maps.list = a.design.maps.list.filter((m) => !levelRows.some((r) => r.levelTo === m.id));
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n57 and the maps.list row(s) changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
  // 1c. a placeholder per level row: a byte copy of its levelFrom with only "name" changed (same-length ids), both copies, as n52-shared.cjs
  for (const r of levelRows) {
    const src = fs.readFileSync(R("Design/levels/" + r.levelFrom + ".json"), "utf8");
    if (src !== fs.readFileSync(R("Assets/StreamingAssets/Design/levels/" + r.levelFrom + ".json"), "utf8")) throw new Error(r.levelFrom + ": the two copies differ");
    const key = '"name":"' + r.levelFrom + '"';
    if (src.split(key).length - 1 !== 1) throw new Error(r.levelFrom + ": the name key not found exactly once");
    const lv = src.split(key).join('"name":"' + r.levelTo + '"');
    if (JSON.parse(lv).name !== r.levelTo || lv.length !== src.length) throw new Error(r.levelTo + ": the placeholder is wrong");
    addNew("Design/levels/" + r.levelTo + ".json", lv);
    addNew("Assets/StreamingAssets/Design/levels/" + r.levelTo + ".json", lv,
      "fileFormatVersion: 2\r\nguid: " + guid() + "\r\nDefaultImporter:\r\n  externalObjects: {}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n");
  }
}

// 1d. kiln47's beats (glimpse / pass / cameo, SurvivalSim.N47.Kiln.cs) follow the level chain n57 -> n52 -> n47 (the red team: they keyed on
//     N52LevelFrom, which returns an n57 id unchanged)
addNew("Assets/Scripts/Game/SurvivalSim.N57.cs", csEol(`// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): design.n57.kiln57 swaps n52-kiln for n57-kiln (data: NightRows.SwapLevels; older systems
// read it by NightRows.BaseId). A system that keyed on a raw level id (kiln47's beats: SurvivalSim.N47.Kiln.cs) asks N57LevelFrom then N52LevelFrom.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The level an n57 level row swaps out (its levelFrom) for an n57 id; any other id is returned as is (an id that does not start
        /// with "n57-" never reads the config).</summary>
        internal string N57LevelFrom(string levelId)
        {
            if (levelId == null || !levelId.StartsWith("n57-", StringComparison.Ordinal) || cfg == null || !cfg.Has("design.n57")) return levelId;
            foreach (KeyValuePair<string, object> kv in cfg.Obj("design.n57"))
                if (kv.Value is Dictionary<string, object> row && row.TryGetValue("levelTo", out object to) && to as string == levelId && row.TryGetValue("levelFrom", out object from) && from is string f)
                    return f;
            return levelId;
        }
    }
}
`));
patch("Assets/Scripts/Game/SurvivalSim.N47.Kiln.cs", [[
  '(campaignLevel.id == "n47-kiln" || N52LevelFrom(campaignLevel.id) == "n47-kiln") && campaign != null; // NIGHT52 (n52-shared.cjs): n52-kiln keeps the beats\n',
  '(campaignLevel.id == "n47-kiln" || N52LevelFrom(campaignLevel.id) == "n47-kiln" || N52LevelFrom(N57LevelFrom(campaignLevel.id)) == "n47-kiln") && campaign != null; // NIGHT52 (n52-shared.cjs): n52-kiln keeps the beats; NIGHT57 (n57-shared.cjs): n57-kiln too\n',
]]);

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N57.cs", csEol(`// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff57 (NIGHT56's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n57.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a157 L level 3 as n57-kiln (per room: entry frame, enemies in view, the room id)
//   a257 G enemy pressure (press57's tell and its hit on a still player, tokens56) and kills (kill55's marks)    a357 V colour (colour56 / grade55 / brief55)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN57 = { "a157", "a257", "a357" };

        static void N57ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a157") N57Level(g, dir, res, offs, pre, ref body);
            else if (s == "a257") N57Play(g, dir, res, offs, pre, ref body);
            else if (s == "a357") N57Colour(g, dir, res, offs, pre, ref body);
        }

        static partial void N57Level(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N57Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N57Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN56, s) >= 0;", "|| Array.IndexOf(PhaseN56, s) >= 0 || Array.IndexOf(PhaseN57, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN56, s) >= 0) N56ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT56 L / G / V (FeelReel.N56.cs)\n",
   "                    else if (Array.IndexOf(PhaseN56, s) >= 0) N56ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT56 L / G / V (FeelReel.N56.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN57, s) >= 0) N57ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT57 L / G / V (FeelReel.N57.cs)\n"],
]);

// 3. the shared tests
const simIds = rows.filter((r) => r.sim).map((r) => '"' + r.id + '"').join(", ");
const levelRows = rows.filter((r) => r.levelFrom);
if (levelRows.length !== 1 || levelRows[0].id !== "kiln57") throw new Error("expected exactly one level row, kiln57");
addNew("Assets/Tests/EditMode/N57SharedTests.cs", csEol(`// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n57's rows exist, each with a _why and a bool enabled; off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows have a tape bit and a numbers object; --off57 and --off56 turn every sim row off
//   2. kiln57 is a level row (levelFrom n52-kiln, levelTo n57-kiln) and, while off, the campaign order is the same with and without design.n57
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N57SharedTests
    {
        static readonly string[] Sims = { ${simIds} };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n57." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n57." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n57._why"), Is.True, "design.n57._why");
            foreach (string s in Sims)
            {
                CheckRow(c, s);
                Assert.That(c.Get("design.n57." + s + ".numbers") is Dictionary<string, object>, Is.True, s + ".numbers is an object");
                Assert.That(NightRows.From(c).IndexOf(s), Is.GreaterThanOrEqualTo(0), s + " is a sim row (a tape bit)");
            }
            CheckRow(c, "kiln57");
            Assert.That(c.Get("design.n57.kiln57.levelFrom") as string, Is.EqualTo("n52-kiln"), "kiln57.levelFrom");
            Assert.That(c.Get("design.n57.kiln57.levelTo") as string, Is.EqualTo("n57-kiln"), "kiln57.levelTo");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void Kiln57_IsListed_AndItsFilesExist()
        {
            GameConfig c = Load();
            var listed = new List<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            Assert.That(listed.Contains("n57-kiln"), Is.True, "n57-kiln is in design.maps.list (an unlisted level ends the campaign)");
            string repo = System.IO.Path.GetDirectoryName(Port.Root);
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Design", "levels", "n57-kiln.json")), Is.True, "Design/levels/n57-kiln.json");
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Assets", "StreamingAssets", "Design", "levels", "n57-kiln.json")), Is.True, "the Player copy");
        }

        [Test]
        public void Kiln57_Off_KeepsNight56sCampaign()
        {
            GameConfig c = Load();
            if (c.Design("n57.kiln57.enabled")) Assert.Ignore("kiln57 is ON (a freeze turned it on): the order now names n57-kiln");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n57");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n57 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT57 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprints plain / --off56 / --off57, the compile, N57SharedTests.");

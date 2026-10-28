// NIGHT58 shared commit (the lead's; modeled on n57-shared.cjs; NIGHT58.md section 3). FINISH LEVEL 3, START LEVEL 4, LAND THE HITS: the
// rows of Logs/n58/rows-def.json (design.n58.<row>), each off: span58 (L), a LEVEL row (levelFrom n52-span, levelTo n58-span, as
// design.n57.kiln57) with no numbers. No sim row tonight (kill55 / press57 / rim56 / yellow56 keep their rows). `-skygrindOff58` / `--off58`
// gives NIGHT57's game.
//   node Tools/night/n58-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n58 = { _why, <rows> }, all enabled:false; the maps.list row for n58-span; the
//      placeholder Design/levels/n58-span.json (+ Player copy), a byte copy of n52-span with only its name changed.
//      No N58LevelFrom (DECIDE58 D-6): no game system keys on a raw span id; the old ones ask NightRows.BaseId, which follows the chain
//      n58-span -> n52-span -> ... by itself.
//   2. Assets/Scripts/Game/FeelReel.N58.cs: reel phases a158 (L levels), a258 (G hits / stillness), a358 (V colour / exits), routed from
//      FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N58SharedTests.cs: the rows exist, off by default (or ON with an "ON" reason), span58 is the only level row,
//      it is listed and its files exist, and, off, the campaign order is NIGHT57's.
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N57.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-21 ~13:45 (NIGHT58.md, verbatim there): build for the next 2 hours what else can be built and iterated upon. NIGHT58: FINISH LEVEL 3 (n57-kiln's exits and in view), START LEVEL 4 (n58-span), LAND THE HITS (kill55, press57's measure, rim56, yellow56).";
const off = "Off until its slate entry (Logs/n58/slate.json) passes NIGHT57.md section 0's rule and the freeze gates pass.";
const rows = JSON.parse(fs.readFileSync(R("Logs/n58/rows-def.json"), "utf8")).rows;
const levelRows = rows.filter((r) => r.levelFrom);
if (levelRows.length !== 1 || levelRows[0].id !== "span58") throw new Error("expected exactly one level row, span58");
if (rows.some((r) => r.sim)) throw new Error("expected no sim row tonight (NIGHT58.md section 3)");

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n58) throw new Error("design.n58 already exists (already run?)");
  if (Object.keys(d.design).pop() !== "n57") throw new Error("design.n57 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n58 = { _why: GG };
  for (const r of rows) {
    const row = {};
    Object.assign(row, { _why: r.why, enabled: false, enabled_why: off });
    if (r.levelFrom) Object.assign(row, { levelFrom: r.levelFrom, levelTo: r.levelTo });
    else row.numbers = {};
    n58[r.id] = row;
  }
  const block = JSON.stringify(n58, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  let out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n58\": " + block + eol + "  }" + eol + "}";
  // 1b. maps.list: the levelTo listed after its levelFrom's row (an unlisted level ends the campaign), as n57-shared.cjs step 1b
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
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
  if (JSON.stringify(back.design.n58) !== JSON.stringify(n58)) throw new Error("design.n58 did not round-trip");
  if (back.design.maps.list.length !== d.design.maps.list.length + levelRows.length) throw new Error("maps.list did not gain " + levelRows.length + " row(s)");
  const a = JSON.parse(out); delete a.design.n58;
  a.design.maps.list = a.design.maps.list.filter((m) => !levelRows.some((r) => r.levelTo === m.id));
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n58 and the maps.list row(s) changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
  // 1c. a placeholder per level row: a byte copy of its levelFrom with only "name" changed (same-length ids), both copies
  for (const r of levelRows) {
    const src = fs.readFileSync(R("Design/levels/" + r.levelFrom + ".json"), "utf8");
    if (src !== fs.readFileSync(R("Assets/StreamingAssets/Design/levels/" + r.levelFrom + ".json"), "utf8")) throw new Error(r.levelFrom + ": the two copies differ");
    const key = '"name":"' + r.levelFrom + '"';
    // n52-span names itself twice (the top level and its "data" block; n52-kiln once): both renamed, nothing else
    const nKey = src.split(key).length - 1, srcJ = JSON.parse(src);
    if (nKey !== 1 + (srcJ.data && srcJ.data.name === r.levelFrom ? 1 : 0)) throw new Error(r.levelFrom + ": the name key found " + nKey + " times");
    const lv = src.split(key).join('"name":"' + r.levelTo + '"');
    const lvJ = JSON.parse(lv);
    if (lvJ.name !== r.levelTo || (lvJ.data && lvJ.data.name !== r.levelTo) || lv.length !== src.length) throw new Error(r.levelTo + ": the placeholder is wrong");
    addNew("Design/levels/" + r.levelTo + ".json", lv);
    addNew("Assets/StreamingAssets/Design/levels/" + r.levelTo + ".json", lv,
      "fileFormatVersion: 2\r\nguid: " + guid() + "\r\nDefaultImporter:\r\n  externalObjects: {}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n");
  }
}

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N58.cs", csEol(`// NIGHT58 (the shared commit; Tools/night/n58-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff58 (NIGHT57's game) where the row is n58's, GREEN on.
// A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node
// Tools/night/n46-build-rows.cjs <tag> on <night>.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a158 L levels (n57-kiln's exits / in view at the door; n58-span's new room)
//   a258 G hits and stillness (kill55's marks after the flash; press57)    a358 V colour and the exits lit (rim56, yellow56, the exit lamp)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN58 = { "a158", "a258", "a358" };

        static void N58ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a158") N58Level(g, dir, res, offs, pre, ref body);
            else if (s == "a258") N58Play(g, dir, res, offs, pre, ref body);
            else if (s == "a358") N58Colour(g, dir, res, offs, pre, ref body);
        }

        static partial void N58Level(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N58Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N58Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN57, s) >= 0;", "|| Array.IndexOf(PhaseN57, s) >= 0 || Array.IndexOf(PhaseN58, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN57, s) >= 0) N57ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT57 L / G / V (FeelReel.N57.cs)\n",
   "                    else if (Array.IndexOf(PhaseN57, s) >= 0) N57ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT57 L / G / V (FeelReel.N57.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN58, s) >= 0) N58ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT58 L / G / V (FeelReel.N58.cs)\n"],
]);

// 3. the shared tests
addNew("Assets/Tests/EditMode/N58SharedTests.cs", csEol(`// NIGHT58 (the shared commit; Tools/night/n58-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n58's rows exist, each with a _why and a bool enabled; off unless a freeze turned it on (its enabled_why then starts with "ON")
//   2. span58 is a level row (levelFrom n52-span, levelTo n58-span), listed, its two files exist, and, while off, the campaign order is the
//      same with and without design.n58
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N58SharedTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n58." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n58." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n58._why"), Is.True, "design.n58._why");
            CheckRow(c, "span58");
            Assert.That(c.Get("design.n58.span58.levelFrom") as string, Is.EqualTo("n52-span"), "span58.levelFrom");
            Assert.That(c.Get("design.n58.span58.levelTo") as string, Is.EqualTo("n58-span"), "span58.levelTo");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void Span58_IsListed_AndItsFilesExist()
        {
            GameConfig c = Load();
            var listed = new List<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            Assert.That(listed.Contains("n58-span"), Is.True, "n58-span is in design.maps.list (an unlisted level ends the campaign)");
            string repo = System.IO.Path.GetDirectoryName(Port.Root);
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Design", "levels", "n58-span.json")), Is.True, "Design/levels/n58-span.json");
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Assets", "StreamingAssets", "Design", "levels", "n58-span.json")), Is.True, "the Player copy");
            Assert.That(NightRows.BaseId(c, "n58-span"), Is.Not.EqualTo("n58-span"), "older systems know n58-span by its chain (NightRows.BaseId)");
        }

        [Test]
        public void Span58_Off_KeepsNight57sCampaign()
        {
            GameConfig c = Load();
            if (c.Design("n58.span58.enabled")) Assert.Ignore("span58 is ON (a freeze turned it on): the order now names n58-span");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n58");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n58 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT58 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprints plain / --off57 / --off58, the compile, N58SharedTests.");

// NIGHT55 shared commit (the lead's; modeled on n54-shared.cjs; NIGHT55.md section 2). The catch-up and push night: the rows of
// Logs/n55/slate.json (design.n55.<row>), each off, each with a numbers object its builder fills (L-049 / L-065: builders patch only
// their own row). `-skygrindOff55` / `--off55` gives NIGHT54's game.
//   node Tools/night/n55-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n55 = { _why, <rows> }, all enabled:false.
//   2. Assets/Scripts/Game/FeelReel.N55.cs: reel phases a155 (B bosses), a255 (C camera / look), a355 (K kills), a455 (R rooms),
//      a555 (S smoke / harness), routed from FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N55SharedTests.cs: the rows exist, off by default (or ON with an "ON" reason), sim rows have a tape bit,
//      view rows none, --off55 / --off54 turn every one off, design.n55 holds no level row.
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N54.cs"), "utf8").includes("\r\n");
const csEol = (t) => (crlfCs ? t.replace(/\r?\n/g, "\r\n") : t);
function addNew(p, text) {
  if (fs.existsSync(R(p))) throw new Error(p + " exists (already run?)");
  writes.push([p, text]);
  if (p.startsWith("Assets/")) {
    if (fs.existsSync(R(p) + ".meta")) throw new Error(p + ".meta exists (already run?)");
    writes.push([p + ".meta", csEol("fileFormatVersion: 2\nguid: " + guid() + "\n")]);
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

const GG = "GG 2026-10-20 (NIGHT55.md, verbatim there): finish and continue anything not finished; spin up subagents following the best methods; research, find areas to iterate and prove; build not just basic things; four hours minimum, 4.5 h hard cap; go well beyond the prompt.";
const off = "Off until its slate entry (Logs/n55/slate.json) passes the critic gate and the freeze gates pass.";
// The rows (builder, kind, why). Filled from the slate at A5; a row's builder fills its numbers.
const rows = JSON.parse(fs.readFileSync(R("Logs/n55/rows-def.json"), "utf8")).rows;

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n55) throw new Error("design.n55 already exists (already run?)");
  if (Object.keys(d.design).pop() !== "n54") throw new Error("design.n54 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n55 = { _why: GG };
  for (const r of rows) {
    const row = {};
    if (r.sim) row.sim = true;
    Object.assign(row, { _why: r.why, enabled: false, enabled_why: off, numbers: {} });
    n55[r.id] = row;
  }
  const block = JSON.stringify(n55, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n55\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n55) !== JSON.stringify(n55)) throw new Error("design.n55 did not round-trip");
  const a = JSON.parse(out); delete a.design.n55;
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n55 changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N55.cs", csEol(`// NIGHT55 (the shared commit; Tools/night/n55-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff55 (NIGHT54's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n55.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a155 B bosses (palettes, the brief, the finish)    a255 C camera / look (step-down, SSAO, decor)
//   a355 K kills (speed kills, chains, ink)            a455 R rooms (the mirrors, rides, pads)    a555 S smoke / harness
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN55 = { "a155", "a255", "a355", "a455", "a555" };

        static void N55ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a155") N55Boss(g, dir, res, offs, pre, ref body);
            else if (s == "a255") N55Look(g, dir, res, offs, pre, ref body);
            else if (s == "a355") N55Kill(g, dir, res, offs, pre, ref body);
            else if (s == "a455") N55Room(g, dir, res, offs, pre, ref body);
            else if (s == "a555") N55Smoke(g, dir, res, offs, pre, ref body);
        }

        static partial void N55Boss(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N55Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // C
        static partial void N55Kill(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K
        static partial void N55Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // R
        static partial void N55Smoke(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // S
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN54, s) >= 0;", "|| Array.IndexOf(PhaseN54, s) >= 0 || Array.IndexOf(PhaseN55, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN54, s) >= 0) N54ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT54 L / G / M / W (FeelReel.N54.cs)\n",
   "                    else if (Array.IndexOf(PhaseN54, s) >= 0) N54ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT54 L / G / M / W (FeelReel.N54.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN55, s) >= 0) N55ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT55 B / C / K / R / S (FeelReel.N55.cs)\n"],
]);

// 3. the shared tests
const viewIds = rows.filter((r) => !r.sim).map((r) => '"' + r.id + '"').join(", ");
const simIds = rows.filter((r) => r.sim).map((r) => '"' + r.id + '"').join(", ");
addNew("Assets/Tests/EditMode/N55SharedTests.cs", csEol(`// NIGHT55 (the shared commit; Tools/night/n55-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n55's rows exist, each with a _why, a bool enabled and a numbers object; off unless a freeze turned it on (its enabled_why
//      then starts with "ON"); sim rows have a tape bit, view rows none; ViewOn reads the view rows; --off55 and --off54 turn every one off
//   2. design.n55 holds no level row: the campaign order is the same with and without it
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N55SharedTests
    {
        static readonly string[] Views = { ${viewIds} };
        static readonly string[] Sims = { ${simIds} };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n55." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n55." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, row + ".numbers is an object");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n55._why"), Is.True, "design.n55._why");
            foreach (string s in Sims)
            {
                CheckRow(c, s);
                Assert.That(NightRows.From(c).IndexOf(s), Is.GreaterThanOrEqualTo(0), s + " is a sim row (a tape bit)");
            }
            var on = new Dictionary<string, bool>();
            foreach (string v in Views)
            {
                on[v] = CheckRow(c, v);
                Assert.That(NightRows.From(c).IndexOf(v), Is.EqualTo(-1), v + " has no tape bit (a view row)");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.EqualTo(on[v]), "ViewOn reads design.n55." + v);
                foreach (int from in new[] { 55, 54 })
                {
                    NightRows.ForceOffFrom = from;
                    foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.False, "--off" + from + ": " + v + " off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N55_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n55"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": design.n55 holds no level row");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n55");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n55 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT55 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprints plain / --off54 / --off55, the compile, N55SharedTests.");

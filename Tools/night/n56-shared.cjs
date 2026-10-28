// NIGHT56 shared commit (the lead's; modeled on n55-shared.cjs; NIGHT56.md section 4). LEVELS FIRST: the rows of Logs/n56/rows-def.json
// (design.n56.<row>), each off, each with a numbers object its builder fills (L-049 / L-065: builders patch only their own row). Rooms
// (builder L) are level data in place: no row. `-skygrindOff56` / `--off56` gives NIGHT55's game (rooms aside).
//   node Tools/night/n56-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n56 = { _why, <rows> }, all enabled:false.
//   2. Assets/Scripts/Game/FeelReel.N56.cs: reel phases a156 (L rooms: the room meter), a256 (G gameplay / enemies), a356 (V colour),
//      routed from FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N56SharedTests.cs: the rows exist, off by default (or ON with an "ON" reason), sim rows have a tape bit,
//      view rows none, --off56 / --off55 turn every one off, design.n56 holds no level row.
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N55.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-20 ~23:08 (NIGHT56.md, verbatim there): what should the overnight prompt focus on to improve levels specifically, gameplay, enemies, colors; research, then a 7 hour night prompt. NIGHT56: LEVELS FIRST, rooms that make you move, enemies that come at you, a style rank that pays, one colour per level.";
const off = "Off until its slate entry (Logs/n56/slate.json) passes the critic gate and the freeze gates pass.";
const rows = JSON.parse(fs.readFileSync(R("Logs/n56/rows-def.json"), "utf8")).rows;

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n56) throw new Error("design.n56 already exists (already run?)");
  if (Object.keys(d.design).pop() !== "n55") throw new Error("design.n55 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n56 = { _why: GG };
  for (const r of rows) {
    const row = {};
    if (r.sim) row.sim = true;
    Object.assign(row, { _why: r.why, enabled: false, enabled_why: off, numbers: {} });
    n56[r.id] = row;
  }
  const block = JSON.stringify(n56, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n56\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n56) !== JSON.stringify(n56)) throw new Error("design.n56 did not round-trip");
  const a = JSON.parse(out); delete a.design.n56;
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n56 changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N56.cs", csEol(`// NIGHT56 (the shared commit; Tools/night/n56-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff56 (NIGHT55's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n56.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a156 L rooms (the room meter: enemies in the frustum, nearest / second-nearest, tells up, room id, per a452 entry frame)
//   a256 G gameplay / enemies (the rank leaving DULL on a combat frame, tokens, kills)    a356 V colour (keys, accents, rims, yellow)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN56 = { "a156", "a256", "a356" };

        static void N56ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a156") N56Room(g, dir, res, offs, pre, ref body);
            else if (s == "a256") N56Play(g, dir, res, offs, pre, ref body);
            else if (s == "a356") N56Colour(g, dir, res, offs, pre, ref body);
        }

        static partial void N56Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N56Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N56Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN55, s) >= 0;", "|| Array.IndexOf(PhaseN55, s) >= 0 || Array.IndexOf(PhaseN56, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN55, s) >= 0) N55ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT55 B / C / K / R / S (FeelReel.N55.cs)\n",
   "                    else if (Array.IndexOf(PhaseN55, s) >= 0) N55ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT55 B / C / K / R / S (FeelReel.N55.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN56, s) >= 0) N56ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT56 L / G / V (FeelReel.N56.cs)\n"],
]);

// 3. the shared tests
const viewIds = rows.filter((r) => !r.sim).map((r) => '"' + r.id + '"').join(", ");
const simIds = rows.filter((r) => r.sim).map((r) => '"' + r.id + '"').join(", ");
addNew("Assets/Tests/EditMode/N56SharedTests.cs", csEol(`// NIGHT56 (the shared commit; Tools/night/n56-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n56's rows exist, each with a _why, a bool enabled and a numbers object; off unless a freeze turned it on (its enabled_why
//      then starts with "ON"); sim rows have a tape bit, view rows none; ViewOn reads the view rows; --off56 and --off55 turn every one off
//   2. design.n56 holds no level row: the campaign order is the same with and without it
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N56SharedTests
    {
        static readonly string[] Views = { ${viewIds} };
        static readonly string[] Sims = { ${simIds} };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n56." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n56." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, row + ".numbers is an object");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n56._why"), Is.True, "design.n56._why");
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
                foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.EqualTo(on[v]), "ViewOn reads design.n56." + v);
                foreach (int from in new[] { 56, 55 })
                {
                    NightRows.ForceOffFrom = from;
                    foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.False, "--off" + from + ": " + v + " off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N56_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n56"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": design.n56 holds no level row");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n56");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n56 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT56 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprints plain / --off55 / --off56, the compile, N56SharedTests.");

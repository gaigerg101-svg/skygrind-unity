// NIGHT54 shared commit (the lead's; modeled on n53-shared.cjs; NIGHT54.md section 3, DECIDE54). THE LOOK PASS: one sim row (wick54) and
// three view rows (light54, grade54, move54), each with a numbers object its builder fills (L-049 / L-065: builders patch only their own row).
//   node Tools/night/n54-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n54 = { _why, wick54 (sim), light54, grade54, move54 }, all enabled:false.
//      `-skygrindOff54` / `--off54` gives NIGHT53's game.
//   2. Assets/Scripts/Game/FeelReel.N54.cs: reel phases a154 (L), a254 (G), a354 (M), a454 (W), routed from FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N54SharedTests.cs: the rows exist, off by default (or ON with an "ON" reason), wick54 is a sim row with a tape
//      bit, the view rows have none, --off54 / --off53 turn every one off, design.n54 holds no level row.
//   4. Tools/night/n54-rows.json (every row passes:false).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N53.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-20 ~14:05 (NIGHT54.md, verbatim there): improve the visuals across the game; fix the lighting and flickering bugs; the movement cleaner, more fluid, less choppy (the core mechanics unchanged: the view only, DECIDE54 D-3); THE WICK made relatively easy with its half-arena attack (FLARE) removed and the rest unchanged; each boss its own palette and lighting identity that shifts during the fight (JoJo's colour turns); the guns and effects in boss fights by colour theory. The UI is not touched (D-4).";
const off = "Off until its check in Tools/night/n54-rows.json passes (printed by the lead) and the freeze gates pass.";
const rows = {
  wick54: { sim: true, why: "Sim row (NightRows.From: a tape bit). Builder W: THE WICK's FLARE leaves the kit in every leg (the cap flare too); the fight made relatively easy without changing any other attack (its numbers in numbers: W fills them). Off = NIGHT53's fight." },
  light54: { sim: false, why: "View row (NightRows.ViewOn(cfg, \"light54\")). Builder L: the flicker fixes (lights popping, strobing tells, z-fighting) and the lighting pass. Off = NIGHT53's lights." },
  grade54: { sim: false, why: "View row (NightRows.ViewOn(cfg, \"grade54\")). Builder G: each boss's palette and lighting identity, its phase turns (the JoJo shift), and the guns' boss-fight colours by colour theory. Off = NIGHT53's look." },
  move54: { sim: false, why: "View row (NightRows.ViewOn(cfg, \"move54\")). Builder M: the camera's smoothness (the view only: hitstop's camera hold, the step-down pop, stepped views). Off = NIGHT53's camera." },
};

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n54) throw new Error("design.n54 already exists (already run?)");
  if (Object.keys(d.design).pop() !== "n53") throw new Error("design.n53 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n54 = { _why: GG };
  for (const [k, r] of Object.entries(rows)) {
    const row = {};
    if (r.sim) row.sim = true;
    Object.assign(row, { _why: r.why, enabled: false, enabled_why: off, numbers: {} });
    n54[k] = row;
  }
  const block = JSON.stringify(n54, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n54\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n54) !== JSON.stringify(n54)) throw new Error("design.n54 did not round-trip");
  const a = JSON.parse(out); delete a.design.n54;
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n54 changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N54.cs", csEol(`// NIGHT54 (the shared commit; Tools/night/n54-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// Each phase prints that its input happened (L-089), then its number; RED with -skygrindOff54 (NIGHT53's game), GREEN on. A partial with no
// body leaves body null (res[0] stays NOT WRITTEN, RED). Rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs
// <tag> on n54.<row>); a campaign level boots only via reel.sh + -skygrindMap (L-062).
//   a154 L light54 (the flicker count, a still camera)   a254 G grade54 (each boss's grade and its turns)
//   a354 M move54 (the camera's jitter)                    a454 W wick54 (THE WICK's fight in the Player)
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN54 = { "a154", "a254", "a354", "a454" };

        static void N54ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a154") N54Light(g, dir, res, offs, pre, ref body);
            else if (s == "a254") N54Grade(g, dir, res, offs, pre, ref body);
            else if (s == "a354") N54Move(g, dir, res, offs, pre, ref body);
            else if (s == "a454") N54Wick(g, dir, res, offs, pre, ref body);
        }

        static partial void N54Light(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N54Grade(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // G
        static partial void N54Move(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // M
        static partial void N54Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN53, s) >= 0;", "|| Array.IndexOf(PhaseN53, s) >= 0 || Array.IndexOf(PhaseN54, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN53, s) >= 0) N53ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT53 U (FeelReel.N53.cs)\n",
   "                    else if (Array.IndexOf(PhaseN53, s) >= 0) N53ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT53 U (FeelReel.N53.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN54, s) >= 0) N54ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT54 L / G / M / W (FeelReel.N54.cs)\n"],
]);

// 3. the shared tests
addNew("Assets/Tests/EditMode/N54SharedTests.cs", csEol(`// NIGHT54 (the shared commit; Tools/night/n54-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n54's four rows exist, each with a _why, a bool enabled and a numbers object; off unless a freeze turned it on (its
//      enabled_why then starts with "ON"); wick54 is a sim row with a tape bit, the view rows have none; ViewOn reads the view rows;
//      --off54 and --off53 turn every one off
//   2. design.n54 holds no level row: the campaign order is the same with and without it
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N54SharedTests
    {
        static readonly string[] Views = { "light54", "grade54", "move54" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n54." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n54." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, row + ".numbers is an object");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n54._why"), Is.True, "design.n54._why");
            CheckRow(c, "wick54");
            Assert.That(NightRows.From(c).IndexOf("wick54"), Is.GreaterThanOrEqualTo(0), "wick54 is a sim row (a tape bit)");
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
                foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.EqualTo(on[v]), "ViewOn reads design.n54." + v);
                foreach (int from in new[] { 54, 53 })
                {
                    NightRows.ForceOffFrom = from;
                    foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.False, "--off" + from + ": " + v + " off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N54_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n54"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": NIGHT54 changes no level file");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n54");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n54 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

// 4. the rows
addNew("Tools/night/n54-rows.json", JSON.stringify({
  _why: "NIGHT54 row list (the lead's; written at P0 by Tools/night/n54-shared.cjs). One entry per row of NIGHT54.md section 3. Only the lead flips passes (Tools/night/n54-flip.cjs), only after printing the check's output in the transcript; `check` is never edited. Builders only report.",
  rows: [
    { id: "P0", title: "Baselines + the shared commit", owner: "lead", switch: "-", check: "the WICK baseline (n47human 40 seeds) printed; design.n54 rows + N54SharedTests committed; gates green (golden, fingerprints plain / --off53 / --off54, the compile)", fallback: "-", passes: false },
    { id: "W", title: "THE WICK: FLARE removed, the fight relatively easy", owner: "builder W", switch: "design.n54.wick54 (sim row)", check: "n47human --boss wick --seeds 40 with the row on: 0 flares; the median fight time and the deaths per fight below the baseline's (DECIDE54's targets); every other attack's numbers unchanged (a test); a454 in the Player GREEN on / RED off", fallback: "the row ships OFF", passes: false },
    { id: "L", title: "The flicker fixes + the lighting pass", owner: "builder L", switch: "design.n54.light54 (view row)", check: "a154: the still-camera flicker count in the Player RED off / GREEN on, on THE WICK's arena and two levels; frames looked at", fallback: "the row ships OFF", passes: false },
    { id: "G", title: "Boss palettes, their turns, the guns' boss colours", owner: "builder G", switch: "design.n54.grade54 (view row)", check: "a254: each boss's grade measured distinct (hue / value), the turn at each phase change printed with its input, cyan's contrast held against each palette; frames looked at", fallback: "the row ships OFF", passes: false },
    { id: "M", title: "The camera's smoothness", owner: "builder M", switch: "design.n54.move54 (view row)", check: "a354: the camera-jitter number in the Player RED off / GREEN on; golden + fingerprints EQUAL", fallback: "the row ships OFF", passes: false },
    { id: "Z", title: "Freeze and close", owner: "lead", switch: "-", check: "rows ON by their checks; Builds/n54-freeze; verbs37 x5 nothing lost; the smoke; golden / fingerprints EQUAL; the report; tag u3-n54", fallback: "whatever is unfinished at 16:54 closes SHORT", passes: false },
  ],
}, null, 2) + "\n");

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT54 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprints plain / --off53 / --off54, the compile, N54SharedTests.");

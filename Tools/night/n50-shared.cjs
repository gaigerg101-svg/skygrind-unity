// NIGHT50 shared commit (the lead's; modeled on n49-shared.cjs; NIGHT50.md section 2 step 3). View rows only: no sim row, no Core change.
//   node Tools/night/n50-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n50 with two view rows, charge50 (H, G1: the charge bar back under the reticle)
//      and gunglow50 (H, G2: the charge on the gun, four steps), both enabled:false with a _why and enabled_why.
//   2. Design/unity-config.json (+ copy): design.n49.look49.numbers.levels = [] (G3: look49 is ON only for the levels listed here; the row's
//      enabled stays false until the freeze; PostFx reads the list per level, see the V builder's round 1).
//   3. Assets/Scripts/Game/UnityTuning.H50.cs: the H builder's numbers file (an empty partial until H fills it).
//   4. Assets/Scripts/Game/FeelReel.N50.cs: the a150 reel phase (H), routed from FeelReel.N27.cs.
//   5. Assets/Tests/EditMode/N50SharedTests.cs: the rows exist and are off; --off50 turns them all off; look49.levels is an empty list.
//      (The two config copies: check with cmp after the run, not in the test.)
//   6. Patches (one anchor each, asserted): FeelReel.N27.cs (the routing of PhaseN50).
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

const GG = "GG 2026-10-19 (~11:20): the charge bar under the AIM reticle needs to come back; the visual polish across the game (lighting, design, arena colours) is too bland; the effects must be unified; inspired by ULTRAKILL and DOOM (NIGHT50.md section 0)";
const off = "Off until the lead has looked at its frames and its blind pair is won or tied, and the freeze gates pass (NIGHT50.md sections 4 and 6). A row is ON at the freeze only with its frames in NIGHT50-LOG and verbs37 passing.";
const viewRows = [ // [row, why]
  ["charge50", "Builder H (view row, G1): the charge bar back under the AIM reticle: arc40 (and meter42's gate) re-gated over hud47's reticle-alone rule, its nearest pixel at least 18 px from the centre, in the weapon's token colour (UnityTuning.Look49*). Its numbers: UnityTuning.H50.cs. Off = NIGHT49's game (reticle alone)."],
  ["gunglow50", "Builder H (view row, G2): the charge shown on the gun in four steps (25 / 50 / 75 / 100 %) on KESTREL, THUNDERCLAP and PULSE, in the weapon's token colour; the gun never moves. Its numbers: UnityTuning.H50.cs. Off = NIGHT49's gun."],
];

// 1. config: design.n50 (the two rows) and design.n49.look49.numbers.levels
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n50) throw new Error("design.n50 already exists (already run?)");
  if (!d.design.n49) throw new Error("design.n49 is missing");
  if (!d.design.n49.look49 || !d.design.n49.look49.numbers) throw new Error("design.n49.look49.numbers is missing");
  if (d.design.n49.look49.numbers.levels !== undefined) throw new Error("look49.levels already exists (already run?)");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";

  // 1a. look49.levels: the list is the per-level switch (empty = the grade is off on every level)
  const lookAt = raw.indexOf('"look49": {');
  if (lookAt < 0 || raw.indexOf('"look49": {', lookAt + 1) >= 0) throw new Error('"look49": { must appear exactly once');
  const numAt = raw.indexOf('"numbers": {', lookAt);
  if (numAt < 0) throw new Error("look49 has no numbers block");
  const insertAt = numAt + '"numbers": {'.length;
  const lineIndent = raw.slice(raw.lastIndexOf(eol, numAt) + eol.length, numAt); // the indent of the "numbers" line
  const inner = lineIndent + "  ";
  const withLevels = raw.slice(0, insertAt) + eol + inner + '"levels": [],' + " " + '"_why": "NIGHT50 G3 (DECIDE50 D-4): the level ids the grade is ON for, when look49 is ON; empty = off on every level. The default ships the levels that passed their blind pair."' + "," + raw.slice(insertAt);
  const backLevels = JSON.parse(withLevels);
  if (JSON.stringify(backLevels.design.n49.look49.numbers.levels) !== "[]") throw new Error("look49.levels did not round-trip");
  const restLevels = JSON.stringify(backLevels.design.n49.look49.numbers) === undefined ? "" : "";
  if (restLevels !== "") throw new Error("unreachable");

  // 1b. design.n50 after n49 (n49 must be the last key of design; the look49 splice above sits earlier in the file, so re-find the tail)
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!withLevels.endsWith(tail)) throw new Error("unexpected config ending: design.n49 must be the last key of design");
  const n50 = { _why: GG };
  for (const [row, why] of viewRows) n50[row] = { _why: "View row (NightRows.ViewOn(cfg, \"" + row + "\")). " + why, enabled: false, enabled_why: off, numbers: {} };
  const block = JSON.stringify(n50, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = withLevels.slice(0, withLevels.length - tail.length) + eol + "    }," + eol + "    \"n50\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n50) !== JSON.stringify(n50)) throw new Error("design.n50 did not round-trip");
  if (JSON.stringify(back.design.n49) !== JSON.stringify(backLevels.design.n49)) throw new Error("design.n49 changed beyond levels");
  if (JSON.stringify(back.design.n48) !== JSON.stringify(d.design.n48)) throw new Error("design.n48 changed");
  for (const row of ["lights49", "fx49", "charge49"]) {
    if (JSON.stringify(back.design.n49[row]) !== JSON.stringify(d.design.n49[row])) throw new Error("design.n49." + row + " changed");
  }
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the H numbers file (empty until H fills it)
addNew("Assets/Scripts/Game/UnityTuning.H50.cs", csEol(`// NIGHT50 shared commit (Tools/night/n50-shared.cjs): builder H's Unity-only numbers for charge50 (the charge bar back under the reticle, G1)
// and gunglow50 (the charge on the gun, four steps, G2). Each number carries a comment; each gameplay number comes from config, never here.
// Colours come from UnityTuning.Look49* (the one palette, UnityTuning.Look49.cs); nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
    }
}
`));

// 3. the reel phase (a150 = H's proof reel): the builder fills the partial in FeelReel.N50.Gun.cs
addNew("Assets/Scripts/Game/FeelReel.N50.cs", csEol(`// NIGHT50 shared commit (Tools/night/n50-shared.cjs): the proof reel of tonight's builder H, dispatched from FeelReel.N27.cs (the lead's line).
// a150 = H (the charge bar under the reticle and the charge on the gun: FeelReel.N50.Gun.cs). A partial with no body leaves body null (res[0]
// stays NOT WRITTEN, RED). A reel forces the view rows with the build copy's config (node Tools/night/n39-build-rows.cjs <tag> on n50.<row>).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN50 = { "a150" };

        static void N50ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a150") N50Gun(g, dir, res, offs, pre, ref body);
        }

        static partial void N50Gun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // H
    }
}
`));

// 4. the shared tests
addNew("Assets/Tests/EditMode/N50SharedTests.cs", csEol(`// NIGHT50 shared commit (Tools/night/n50-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n50 row exists with a _why and a bool enabled, is off unless a freeze turned it on (its enabled_why then starts with "ON"),
//      and NightRows.ViewOn reads the same switch; the checker finds no problem in design
//   2. the off switch: ForceOffFrom 50 (-skygrindOff50 / --off50) turns every n50 view row off whatever the config says
//   3. look49.levels exists and is an empty list in the shared commit (the grade is on for no level until the freeze lists one)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N50SharedTests
    {
        static readonly string[] ViewRows = { ${viewRows.map(([r]) => '"' + r + '"').join(", ")} };

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n50._why"), Is.True, "design.n50._why");
            foreach (string row in ViewRows)
            {
                string p = "design.n50." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n50." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
                Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(on), row + ": NightRows.ViewOn reads the design.n50 switch");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void OffSwitch50_TurnsEveryViewRowOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 50; // -skygrindOff50 / --off50
                GameConfig c = Load();
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off50: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Look49Levels_IsAnEmptyListInTheSharedCommit()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n49.look49.numbers.levels"), Is.True, "look49.numbers.levels exists");
            var levels = c.Get("design.n49.look49.numbers.levels") as List<object>;
            Assert.That(levels, Is.Not.Null, "look49.numbers.levels is a list");
            Assert.That(levels.Count, Is.EqualTo(0), "the shared commit lists no level (the freeze decides, DECIDE50 D-4)");
        }

    }
}
`));

// 5. the routing of PhaseN50 in FeelReel.N27.cs (one anchor each)
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN49, s) >= 0;", "|| Array.IndexOf(PhaseN49, s) >= 0 || Array.IndexOf(PhaseN50, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN49, s) >= 0) N49ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT49 H / X / V / L (FeelReel.N49.cs)\n",
   "                    else if (Array.IndexOf(PhaseN49, s) >= 0) N49ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT49 H / X / V / L (FeelReel.N49.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN50, s) >= 0) N50ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT50 H (FeelReel.N50.cs)\n"],
]);

if (dry) {
  for (const [p] of writes) console.log("would write " + p);
  console.log("DRY RUN: " + writes.length + " file(s), every anchor found once");
  process.exit(0);
}
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT50 shared commit done: " + writes.length + " file(s). Next: cmp the two config copies, dn-check, the Editor compile, N50SharedTests, golden, fingerprints plain / --off49 / --off50.");

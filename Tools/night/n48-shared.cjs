// NIGHT48 shared commit (the lead's, modeled on n47-shared.cjs; docs/NIGHT-ROWS.md section 2; NIGHT48.md section 5). Four builders, NIGHT47's split:
//   W = THE WICK (G1, G5, G9), B = levels 2 + 3 (G29-G35), L = level 4 (G36, G37), F = enemy looks, weapons, the lineup reel (G12-G14, G20, G24, G43 TORCH).
//   node Tools/night/n48-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n48, every row enabled:false with a _why; one level row (climbTop48: n47-foundry ->
//      n48-foundry) + its design.maps.list row after n47-spire's; the sim rows (numbers {} for their builder); the view rows (lineup48 is reel-only).
//      design.n47 is not touched.
//   2. One placeholder level (a copy of n47-foundry with only the outer "name" changed), both copies, + the StreamingAssets .meta.
//   3. Assets/Scripts/Game/SurvivalSim.N48.cs: Night48StartRun / Night48Step / N48Campaign -> each builder's partials (N48W*, N48B*, N48L*, N48F*).
//   4. Assets/Scripts/Game/FeelReel.N48.cs: reel phases a148 (W), a248 / a348 (B), a448 / a548 (L), a648 / a748 / a848 / a948 (F; a948 = the
//      lineup reel, lineup48), routed from FeelReel.N27.cs.
//   5. Assets/Tests/EditMode/N48SharedTests.cs: the rows exist and are off; every row off = NIGHT47's order; --off48 = NIGHT47's order, --off47 =
//      NIGHT46's; bases. Its Switches() clears / restores N38-N43Blocks.ForceOff from the start (Lesson25Tests leaves them set; L-082).
//   6. Hook sites: SurvivalSim.Campaign24.cs calls N48Campaign() after N47Campaign().
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/SurvivalSim.N47.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-18 (after playing n46-r3; NIGHT47's 45 asks, G1-G45): NIGHT48 finishes the 18 SHORT rows of NIGHT47 as GREEN (built, measured and seen on Player frames by the lead), with the 8 R4 rows framed and judged (NIGHT48.md section 1)";
const off = "Off until the lead has looked at its frames and its gates pass (NIGHT48.md sections 0 and 3: no row closes without its frames in the ledger; a row is ON at the freeze when its frames and meters are GREEN, verbs37 passes on levels 1-5 and the smoke passes).";

// 1. config
const levels = [ // [row, levelFrom, levelTo, why]
  ["climbTop48", "n47-foundry", "n48-foundry", "Builder W (level row): G1's climb top third: a ledge lip at every top-third panel's end (a catch shelf the late release lands on), or the top third as panel -> ledge -> panel, in n48-foundry (from n47-foundry; untouched)."],
];
const simRows = [ // [row, why]
  ["wyrmTell48", "Builder B (sim row): G31, THE KILN WYRM's head-charge tell: the counter changes, not the timing (the head glows yellow for the whole tell and a punch anywhere in it, or a parry, staggers)."],
  ["wyrmDive48", "Builder B (sim row): G34, the burrow as the punish: after a stagger the worm dives (it flees under the floor), so burrows rise with the player's success."],
  ["kilnBoost48", "Builder B (sim row): G33, the islands pay for the THUNDERCLAP's boost: the core's x3 only from an island or mid-boost."],
];
const viewRows = [ // [row, why]
  ["loomHull48", "Builder L (view row): G36 / G37, THE LOOM's bronze / brass shuttle hull, its legs and its brass strips (the fixes its frames ask for)."],
  ["blast48", "Builder F (view row): G20, the picked LT-blast variant (A ring vs B lance); the lead picks the winner from the blind pair."],
  ["guns48", "Builder F (view row): G24, per-gun weight on the ladder: no gun lighter than today (DECIDE47 D-14)."],
  ["foes48", "Builder F (view row): G12-G14, the WARDEN's shield, the visors and the small robots' fixes, from the lineup reel's frames."],
  ["lineup48", "Builder F (view row, a reel-only switch): the lineup reel (a948, G12-G14) hides the viewmodel and the HUD and stands each kind alone on a neutral plate. It changes no game state, only the reel's camera and staging."],
];
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  let raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n48) throw new Error("design.n48 already exists (already run?)");
  if (!d.design.n47) throw new Error("design.n47 is missing");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
  for (const [row, from, to] of levels) {
    if (!listed[from]) throw new Error(row + ": its levelFrom " + from + " is not in design.maps.list");
    if (listed[to]) throw new Error(row + ": " + to + " is already in design.maps.list");
  }
  const anchorRe = /(\{\s*"id":\s*"n47-spire",[^}]*\})/;
  const m = raw.match(anchorRe);
  if (!m || raw.split('"id": "n47-spire"').length - 1 !== 1) throw new Error("the n47-spire maps.list row was not found exactly once");
  const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
  const add = levels.map(([, from, id]) => "," + eol + indent + JSON.stringify({ id, label: listed[from].label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
  raw = raw.replace(anchorRe, m[1] + add);
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n48 = { _why: GG };
  for (const [row, from, to, why] of levels) n48[row] = { _why: why, enabled: false, enabled_why: off, levelFrom: from, levelTo: to };
  for (const [row, why] of simRows) n48[row] = { sim: true, _why: why, enabled: false, enabled_why: off, numbers: {} };
  for (const [row, why] of viewRows) n48[row] = { _why: "View row (NightRows.ViewOn(cfg, \"" + row + "\")). " + why, enabled: false, enabled_why: off, numbers: {} };
  const block = JSON.stringify(n48, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n48\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n48) !== JSON.stringify(n48)) throw new Error("design.n48 did not round-trip");
  if (JSON.stringify(back.design.n47) !== JSON.stringify(d.design.n47)) throw new Error("design.n47 changed");
  if (JSON.stringify(back.design.n46) !== JSON.stringify(d.design.n46)) throw new Error("design.n46 changed");
  if (back.design.maps.list.length !== d.design.maps.list.length + levels.length) throw new Error("maps.list did not gain " + levels.length + " rows");
  for (const [, from, id] of levels) {
    const src = fs.readFileSync(R("Design/levels/" + from + ".json"), "utf8");
    if (src !== fs.readFileSync(R("Assets/StreamingAssets/Design/levels/" + from + ".json"), "utf8")) throw new Error(from + ": Design/levels and its StreamingAssets copy differ");
    const key = '"name":"' + from + '"';
    const n = src.split(key).length - 1;
    if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
    const lv = src.split(key).join('"name":"' + id + '"');
    if (JSON.parse(lv).name !== id) throw new Error(id + ": the name did not change");
    addNew("Design/levels/" + id + ".json", lv);
    addNew("Assets/StreamingAssets/Design/levels/" + id + ".json", lv,
      "fileFormatVersion: 2\r\nguid: " + guid() + "\r\nDefaultImporter:\r\n  externalObjects: {}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n");
  }
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the night's sim hooks
addNew("Assets/Scripts/Game/SurvivalSim.N48.cs", csEol(`// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the night's sim rows (design.n48.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   W (SurvivalSim.N48.Wick.cs):  no sim row (climbTop48 is a level row) -> N48WStartRun / N48WStep / N48WCampaign
//   B (SurvivalSim.N48.Brawl.cs): wyrmTell48, wyrmDive48, kilnBoost48   -> N48BStartRun / N48BStep / N48BCampaign
//   L (SurvivalSim.N48.Loom.cs):  no sim row (loomHull48 is a view row) -> N48LStartRun / N48LStep / N48LCampaign
//   F (SurvivalSim.N48.Feel.cs):  no sim row (blast48, guns48, foes48, lineup48 are view rows) -> N48FStartRun / N48FStep / N48FCampaign
// Off: -skygrindOff48 / --off48 (or any older switch): every id reads off. The level row (climbTop48) and the view rows are data only:
// Curriculum25 swaps the level; a view asks NightRows.ViewOn(cfg, "<row>"); lineup48 is read by the lineup reel only.
// A builder resolves its ids once a run in its StartRun and reads rows.On(id) per step (an array read, no allocation). Each builder owns its bosses'
// Core files (Core/Enemies/*) and calls them from its own partial; nobody edits SurvivalSim.Rows.cs, FeelReel.N27.cs or this file tonight.
// A builder that needs a new row asks the lead (NIGHT48.md section 0).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idWyrmTell48 = -1, idWyrmDive48 = -1, idKilnBoost48 = -1; // B

        partial void Night48StartRun()
        {
            idWyrmTell48 = rowsRegistry.IndexOf("wyrmTell48");
            idWyrmDive48 = rowsRegistry.IndexOf("wyrmDive48");
            idKilnBoost48 = rowsRegistry.IndexOf("kilnBoost48");
            N48WStartRun(); // builder W
            N48BStartRun(); // builder B
            N48LStartRun(); // builder L
            N48FStartRun(); // builder F
        }

        partial void Night48Step(double dt)
        {
            N48WStep(dt); // builder W
            N48BStep(dt); // builder B
            N48LStep(dt); // builder L
            N48FStep(dt); // builder F
        }

        /// <summary>Once a run, right after N47Campaign (the campaign is made): the builders set their Campaign24 delegates here, only for rows
        /// that are on (a null delegate is NIGHT47's rule).</summary>
        partial void N48Campaign()
        {
            N48WCampaign();
            N48BCampaign();
            N48LCampaign();
            N48FCampaign();
        }

        partial void N48Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N48WStartRun(); // W: reset every per-run field here (rule 6); numbers from design.n48.<row>.numbers
        partial void N48WStep(double dt);
        partial void N48WCampaign();
        partial void N48BStartRun(); // B
        partial void N48BStep(double dt);
        partial void N48BCampaign();
        partial void N48LStartRun(); // L
        partial void N48LStep(double dt);
        partial void N48LCampaign();
        partial void N48FStartRun(); // F
        partial void N48FStep(double dt);
        partial void N48FCampaign();
    }
}
`));

// 3. the reel hooks
addNew("Assets/Scripts/Game/FeelReel.N48.cs", csEol(`// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partials in its own file: a148 (W: FeelReel.N48.Wick.cs), a248 / a348 (B: FeelReel.N48.Brawl.cs /
// FeelReel.N48.Wyrm.cs), a448 / a548 (L: FeelReel.N48.Loom.cs / FeelReel.N48.Crown.cs), a648 / a748 / a848 / a948 (F: FeelReel.N48.Hud.cs /
// FeelReel.N48.Fx.cs / FeelReel.N48.Look.cs / FeelReel.N48.Lineup.cs). a948 is the lineup reel (lineup48: no viewmodel, no HUD, each kind
// alone on a neutral plate; G12-G14); a848 stays F's look reel as a847 was. A partial with no body leaves body null (res[0] stays NOT
// WRITTEN, RED). A reel forces a sim row before the run starts with sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wyrmTell48", true);
// view rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs <tag> on n48.<row>) or by a reel-only switch the
// builder reads.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN48 = { "a148", "a248", "a348", "a448", "a548", "a648", "a748", "a848", "a948" };

        static void N48ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a148") N48Wick(g, dir, res, offs, pre, ref body);
            else if (s == "a248") N48Brawl(g, dir, res, offs, pre, ref body);
            else if (s == "a348") N48Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a448") N48Loom(g, dir, res, offs, pre, ref body);
            else if (s == "a548") N48Crown(g, dir, res, offs, pre, ref body);
            else if (s == "a648") N48Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a748") N48Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a848") N48Look(g, dir, res, offs, pre, ref body);
            else if (s == "a948") N48Lineup(g, dir, res, offs, pre, ref body);
        }

        static partial void N48Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
        static partial void N48Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N48Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N48Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N48Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N48Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Lineup(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F: the lineup reel (lineup48)
    }
}
`));

// 4. the shared tests
addNew("Assets/Tests/EditMode/N48SharedTests.cs", csEol(`// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n48 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows are in the registry with a numbers object, --off48 turns them all off; the level and view rows carry no tape bit;
//      the level row's files exist (both copies) and are listed
//   2. with every n48 row off, the campaign order is NIGHT47's (the config without design.n48)
//   3. the off switch: ForceOffFrom 48 (-skygrindOff48 / --off48) with every n48 level row ON leaves NIGHT47's order; ForceOffFrom 47 leaves
//      NIGHT46's (the config without design.n47 and design.n48); 49 and no switch swap
//   4. every n48 level reads as its levelFrom's base to every older system (n48-foundry -> n47-foundry -> n46-foundry -> n45-foundry -> n43-foundry)
// Switches() clears / restores N38-N43Blocks.ForceOff and N45Levels.ForceOff from the start: Lesson25Tests leaves them set (no restore),
// and the orders here need them as the config says (NIGHT46's and NIGHT47's fix, L-082).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N48SharedTests
    {
        static readonly string[] LevelRows = { ${levels.map(([r]) => '"' + r + '"').join(", ")} };
        static readonly string[] ViewRows = { ${viewRows.map(([r]) => '"' + r + '"').join(", ")} };
        static readonly string[] SimRows = { ${simRows.map(([r]) => '"' + r + '"').join(", ")} };

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N48(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n48"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N48(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN48()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n48");
            return c;
        }

        static GameConfig WithoutN47N48()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n47");
            PortData.Obj(c.root["design"]).Remove("n48");
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
            Assert.That(c.Has("design.n48._why"), Is.True, "design.n48._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n48." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n48." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n48." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n48." + row + ".numbers") && c.Get("design.n48." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 48; // -skygrindOff48: every n48 sim row off whatever the config says
                NightRowState d = NightRows.From(c).Defaults();
                foreach (string row in SimRows) Assert.That(d.On(reg.IndexOf(row)), Is.False, "--off48: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n48." + row + ".levelTo"), from = c.Str("design.n48." + row + ".levelFrom");
                Assert.That(to.StartsWith("n48-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight47sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night47 = Order(WithoutN48());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night47), "every n48 level row off: NIGHT47's order (" + string.Join(", ", night47.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n48." + row + ".levelFrom");
                    Assert.That(night47.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT47's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight47sOrder()
        {
            List<string> night46 = null, night47 = null;
            Switches(NightRows.NoOff, () => { night46 = Order(WithoutN47N48()); night47 = Order(WithoutN48()); });
            Switches(47, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night46), "ForceOffFrom 47 (--off47) with every n48 level row on: NIGHT46's order"));
            Switches(48, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night47), "ForceOffFrom 48 (--off48) with every n48 level row on: NIGHT47's order"));
            foreach (int from in new[] { 49, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night47.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night47.IndexOf(c.Str("design.n48." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == c.Str("design.n48." + row + ".levelTo"), Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN48Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n48." + row + ".levelFrom"), to = c.Str("design.n48." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n48-foundry"), Is.EqualTo("n43-foundry"), "n48 -> n47 -> n46 -> n45 -> n43");
        }
    }
}
`));

// 5. hook sites
patch("Assets/Scripts/Game/SurvivalSim.Campaign24.cs", [[
  "                N47Campaign(); // NIGHT47 (n47-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N47.cs; rows off: none)\n",
  "                N47Campaign(); // NIGHT47 (n47-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N47.cs; rows off: none)\n" +
  "                N48Campaign(); // NIGHT48 (n48-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N48.cs; rows off: none)\n",
]]);
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN47, s) >= 0;", "|| Array.IndexOf(PhaseN47, s) >= 0 || Array.IndexOf(PhaseN48, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN47, s) >= 0) N47ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT47 W / B / L / F (FeelReel.N47.cs)\n",
   "                    else if (Array.IndexOf(PhaseN47, s) >= 0) N47ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT47 W / B / L / F (FeelReel.N47.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN48, s) >= 0) N48ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT48 W / B / L / F (FeelReel.N48.cs)\n"],
]);

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT48 shared commit done: " + writes.length + " file(s). Next: dn-check, golden, fingerprint plain / --off47 / --off48, n45rows, cmp the configs, the Editor compile, N48SharedTests + N47SharedTests + N46SharedTests + NightRowsTests.");

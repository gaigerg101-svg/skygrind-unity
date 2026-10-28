// NIGHT46 shared commit (written by the NIGHT46 forge, dry-run twice in its worktree on top of the rows switch-on; docs/NIGHT-ROWS.md
// section 2, the first night with rows as data). Run AFTER Tools/night/rows-switch-on.cjs (it refuses otherwise):
//   node Tools/night/n46-shared.cjs [rootDir]     (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes (32 files: 23 new, 9 one-line patches; it prints each):
//   1. Design/unity-config.json (+ its byte copy in Assets/StreamingAssets/Design/): design.n46, every row enabled:false, each with a _why:
//      level rows foundry46 / iceline46 (A3), kiln46 (B1) (levels 1-3, flow fixes; from the n45 levels), span46 (A1, from n40-span) and
//      spire46 (A2, from n41-spire); the sim rows retry46 / overlap46 / teach46 / reveal46 (F1) and wyrm46 / walkin46 (B1), each with a
//      numbers {} for its builder; the view rows shift46 / flow46 / feel46. Five design.maps.list rows after n45-kiln's, as NIGHT45's were.
//   2. Five placeholder levels: a copy of each row's levelFrom file with only the outer "name" changed (Design/levels and the StreamingAssets
//      copy, + a DefaultImporter .meta for the StreamingAssets one, as the committed n45 metas are).
//   3. Assets/Scripts/Game/SurvivalSim.N46.cs (+ .meta): Night46StartRun / Night46Step / N46Campaign -> F1's N46Flow* and B1's N46Wyrm*
//      partials; the six row ids; walkIn46.
//   3b. Assets/Scripts/Game/FeelReel.N46.cs (+ .meta): the reel partials N46Span (A1, a146), N46Spire (A2, a246), N46Smooth (A3, a346),
//      N46Wyrm (B1, a446), N46Feel (V1, a546), routed from FeelReel.N27.cs (two edits, here).
//   4. Assets/Tests/EditMode/N46SharedTests.cs (+ .meta): the rows exist, off by default; every row off = NIGHT45's campaign order; the off
//      switch (ForceOffFrom 46, -skygrindOff46) leaves NIGHT45's order; every n46 level's base id equals its levelFrom's.
//   4b. The builders' hook sites (null / false = NIGHT45's game, so the fingerprint stays EQUAL): Core/Campaign24.cs gains three null delegates
//      (retryWave46 in Retry(), waveRelease46 beside waveEarly33, teachHold46 at the three rooms43 holds); SurvivalSim.Campaign24.cs calls
//      N46Campaign() once the campaign is made; SurvivalSim.N36.cs's Boss36WalkIn also reads walkIn46.
//   5. One-time fixes found by the forge (each one line):
//      - NightRowsTests.TheRealConfig_HasNoSimRows_AndNoProblems pinned "NIGHT45 adds no sim rows": from tonight the real config has six,
//        so the pin becomes "every row is a well-formed NIGHT46+ row and the defaults round-trip through the tape record" (never again).
//      - Two direct level-id checks that would not see n46-span (NightRows.BaseId resolves the chain, but these two never ask it):
//        SurvivalSim.Loom37.cs (THE LOOM's rooms o1-o3 on level 4: loom40's levelTo n40-span) and Bot/Walk38Span.cs (the level 4 walk
//        pilot). Each now asks N45Levels.BaseId (-> NightRows.BaseId first): n46-span reads as n40-span. With the rows off an n46 id is never
//        loaded, so nothing changes (fingerprint EQUAL).
//      - Two NIGHT45 reels' level checks (FeelReel.N45.Yard.cs a145, FeelReel.N45.Mix.cs a245) ask NightRows.BaseId, so n46-foundry /
//        n46-iceline still run as their n45 rooms.
// Not part of a night any more (docs/NIGHT-ROWS.md): no N46Blocks, no tape bump, no AsFormat link, no older shared test's pins.
const fs = require("fs");
const root = (process.argv[2] || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = []; // [path, text]: nothing is written until every check below passed
const crlfCs = fs.readFileSync(R("Assets/Scripts/Core/N45Levels.cs"), "utf8").includes("\r\n");
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

// 0. preconditions: the rows switch-on is in (else the level rows would swap nothing and Night46StartRun would never run)
for (const [f, needle] of [["Assets/Scripts/Core/Curriculum25.cs", "NightRows.SwapLevels(cfg, res);"], ["Assets/Scripts/Core/N45Levels.cs", "levelId = NightRows.BaseId(cfg, levelId);"],
  ["Assets/Scripts/Game/SurvivalSim.cs", "RowsInit();"], ["Assets/Scripts/Game/SurvivalSim.Run.cs", "RowsStep(step);"]])
  if (!fs.readFileSync(R(f), "utf8").includes(needle)) throw new Error(f + " lacks '" + needle + "': run node Tools/night/rows-switch-on.cjs first");

// 1. config: design.n46 + maps.list rows + placeholders
const levels = [ // [row, levelFrom, levelTo, its _why]
  ["foundry46", "n45-foundry", "n46-foundry", "Builder A3 (level row): level 1's flow fixes (GG 2026-10-17: 'feel and flow better'), in n46-foundry (from n45-foundry; n45-foundry untouched)."],
  ["iceline46", "n45-iceline", "n46-iceline", "Builder A3 (level row): level 2's flow fixes, in n46-iceline (from n45-iceline; untouched)."],
  ["kiln46", "n45-kiln", "n46-kiln", "Builder B1 (level row): level 3's flow fixes and THE KILN WYRM's walk-in room, in n46-kiln (from n45-kiln; untouched; the fight's numbers are wyrm46's / walkin46's)."],
  ["span46", "n40-span", "n46-span", "Builder A1 (level row): level 4 rebuilt from its identity cards (HOOK YARD, THE TIE-OFF, SPOOL HALL; docs/ROOMS.md, DESIGN-RULES A-9), in n46-span (from n40-span; untouched)."],
  ["spire46", "n41-spire", "n46-spire", "Builder A2 (level row): level 5 rebuilt from its identity cards (THE NAVE, THE CHOIR; docs/ROOMS.md, DESIGN-RULES A-9), in n46-spire (from n41-spire; untouched)."],
];
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  let raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n46) throw new Error("design.n46 already exists (already run?)");
  if (!d.design.n45) throw new Error("design.n45 is missing");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
  for (const [row, from, to] of levels) {
    if (!listed[from]) throw new Error(row + ": its levelFrom " + from + " is not in design.maps.list");
    if (listed[to]) throw new Error(row + ": " + to + " is already in design.maps.list");
  }
  const anchorRe = /(\{\s*"id":\s*"n45-kiln",[^}]*\})/;
  const m = raw.match(anchorRe);
  if (!m || raw.split('"id": "n45-kiln"').length - 1 !== 1) throw new Error("the n45-kiln maps.list row was not found exactly once");
  const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
  const add = levels.map(([, from, id]) => "," + eol + indent + JSON.stringify({ id, label: listed[from].label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
  raw = raw.replace(anchorRe, m[1] + add);
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const off = "Off until the lead has looked at its frames and its gates pass (NIGHT46.md method).";
  const n46 = { _why: "GG 2026-10-17: 'need to feel and flow better ... study real game deisgn and areana layouts' (NIGHT46.md)" };
  for (const [row, from, to, why] of levels) n46[row] = { _why: why, enabled: false, enabled_why: off, levelFrom: from, levelTo: to };
  const sim = (why) => ({ sim: true, _why: why, enabled: false, enabled_why: off, numbers: {} });
  n46.retry46 = sim("Builder F1 (sim row): a death restarts the room at the wave you died in, not wave 1 (Research/2026-10-17-wave46/FLOW-AUDIT.md row 1, DESIGN-RULES A-4). Hook: Campaign24.retryWave46 (set in N46FlowCampaign). Its numbers go in numbers (the builder's).");
  n46.overlap46 = sim("Builder F1 (sim row): the next wave releases when 1-2 bodies of the current wave remain (FLOW-AUDIT row 5). Hook: Campaign24.waveRelease46. Its numbers go in numbers.");
  n46.teach46 = sim("Builder F1 (sim row): teaching holds capped by their own numbers instead of design.n43.rooms43's 20 s (FLOW-AUDIT row 4). Hook: Campaign24.teachHold46 (the stand-in hold, the afterLesson wait, the lesson cap). Its numbers go in numbers.");
  n46.reveal46 = sim("Builder F1 (sim row): boss reveal / phase / die windows ~1 s (FLOW-AUDIT row 8). No Campaign24 hook: the windows live in each boss's own module (Core/Enemies/*). Its numbers go in numbers.");
  n46.wyrm46 = sim("Builder B1 (sim row): THE KILN WYRM's fight length (GG 2026-10-13: bosses 'one to two minutes, maybe three if you're slow'; the bot band 30-60 s, DECIDE39 D-1; NIGHT45 A3's harness killed it in 134.3 / 185.5 / 161.1 s on n45-kiln). SurvivalSim.N46.cs: N46WyrmStartRun / N46WyrmStep read rows.On(idWyrm46). Its numbers go in numbers (the builder's).");
  n46.walkin46 = sim("Builder B1 (sim row): THE KILN WYRM's fight is walked into at k5 instead of the teleport that zeroes velocity (Campaign24.Enter's boss Respawn; FLOW-AUDIT row 2). Hook: SurvivalSim.walkIn46 (read by Boss36WalkIn = ICampaignWorld.BossWalkIn). Its numbers go in numbers.");
  n46.shift46 = { _why: "View row: a room's state change lights its fixtures (NightRows.ViewOn(cfg, \"shift46\")).", enabled: false, enabled_why: off };
  n46.flow46 = { _why: "View row: the flow meter's own view probes, if any (NightRows.ViewOn(cfg, \"flow46\")).", enabled: false, enabled_why: off };
  n46.feel46 = { _why: "View row: camera / feedback feel (NightRows.ViewOn(cfg, \"feel46\")).", enabled: false, enabled_why: off };
  const block = JSON.stringify(n46, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n46\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n46) !== JSON.stringify(n46)) throw new Error("design.n46 did not round-trip");
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

// 2. the night's hooks file (docs/NIGHT-ROWS.md section 2, item 3): builders F1 (flow) and B1 (the wyrm)
addNew("Assets/Scripts/Game/SurvivalSim.N46.cs", csEol(`// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the night's sim rows (design.n46.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   F1 (SurvivalSim.N46.Flow.cs): retry46, overlap46, teach46, reveal46 -> N46FlowStartRun / N46FlowStep / N46FlowCampaign
//   B1 (SurvivalSim.N46.Wyrm.cs): wyrm46, walkin46                   -> N46WyrmStartRun / N46WyrmStep / N46WyrmCampaign
// Off: -skygrindOff46 / --off46 (or any older switch): every id reads off. The level rows (foundry46 ... spire46) and the view rows (shift46,
// flow46, feel46) are data only: no hook here (Curriculum25 swaps the levels; a view asks NightRows.ViewOn).
// Campaign24 (Core) is never edited by a builder: its NIGHT46 hooks are null delegates (retryWave46, waveRelease46, teachHold46; null = NIGHT45's
// rooms bit for bit), set in N46FlowCampaign / N46WyrmCampaign, which run once a run right after the campaign is made
// (SurvivalSim.Campaign24.cs, N24CampaignStep). walkIn46 (false = NIGHT45) joins Boss36WalkIn (ICampaignWorld.BossWalkIn: no boss Respawn).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idRetry46 = -1, idOverlap46 = -1, idTeach46 = -1, idReveal46 = -1; // F1
        internal int idWyrm46 = -1, idWalkin46 = -1; // B1

        /// <summary>NIGHT46 B1 (walkin46): the boss room keeps the player where they walked in (Boss36WalkIn reads it). Reset every run here;
        /// B1 sets it (N46WyrmStartRun / N46WyrmCampaign) only while rows.On(idWalkin46).</summary>
        internal bool walkIn46;

        partial void Night46StartRun()
        {
            idRetry46 = rowsRegistry.IndexOf("retry46");
            idOverlap46 = rowsRegistry.IndexOf("overlap46");
            idTeach46 = rowsRegistry.IndexOf("teach46");
            idReveal46 = rowsRegistry.IndexOf("reveal46");
            idWyrm46 = rowsRegistry.IndexOf("wyrm46");
            idWalkin46 = rowsRegistry.IndexOf("walkin46");
            walkIn46 = false;
            N46FlowStartRun(); // builder F1
            N46WyrmStartRun(); // builder B1
        }

        partial void Night46Step(double dt)
        {
            N46FlowStep(dt); // builder F1
            N46WyrmStep(dt); // builder B1
        }

        /// <summary>Once a run, right after N24CampaignStep made the campaign (campaign is not null): the builders set its NIGHT46 delegates
        /// here, only for rows that are on (a null delegate is NIGHT45's rule).</summary>
        partial void N46Campaign()
        {
            N46FlowCampaign(); // builder F1: campaign.retryWave46 / waveRelease46 / teachHold46
            N46WyrmCampaign(); // builder B1
        }

        partial void N46Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N46FlowStartRun(); // F1: reset every per-run field here (rule 6); numbers from design.n46.<row>.numbers
        partial void N46FlowStep(double dt); // F1: rows.On(idRetry46) ... per step (an array read, no allocation)
        partial void N46FlowCampaign(); // F1
        partial void N46WyrmStartRun(); // B1: reset every per-run field here (rule 6); numbers from design.n46.wyrm46.numbers / walkin46.numbers
        partial void N46WyrmStep(double dt); // B1: rows.On(idWyrm46), rows.On(idWalkin46)
        partial void N46WyrmCampaign(); // B1
    }
}
`));

// 2b. the reel hooks, pre-placed (L-050, as NIGHT45's FeelReel.N45.cs): one partial per builder, routed from FeelReel.N27.cs once
addNew("Assets/Scripts/Game/FeelReel.N46.cs", csEol(`// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partial in its own file: a146 (A1: FeelReel.N46.Span.cs), a246 (A2: FeelReel.N46.Spire.cs), a346 (A3:
// FeelReel.N46.Smooth.cs), a446 (B1: FeelReel.N46.Wyrm.cs), a546 (V1: FeelReel.N46.Feel.cs). A partial with no body leaves body null
// (res[0] stays NOT WRITTEN, RED). A reel forces a sim row before the run starts with
// sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wyrm46", true); (docs/NIGHT-ROWS.md).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN46 = { "a146", "a246", "a346", "a446", "a546" };

        static void N46ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a146") N46Span(g, dir, res, offs, pre, ref body);
            else if (s == "a246") N46Spire(g, dir, res, offs, pre, ref body);
            else if (s == "a346") N46Smooth(g, dir, res, offs, pre, ref body);
            else if (s == "a446") N46Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a546") N46Feel(g, dir, res, offs, pre, ref body);
        }

        static partial void N46Span(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A1
        static partial void N46Spire(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A2
        static partial void N46Smooth(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A3
        static partial void N46Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B1
        static partial void N46Feel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V1
    }
}
`));

// 3. the shared tests (Core, headless-runnable: no UnityEngine; Port.Root is the harness's or Unity's)
addNew("Assets/Tests/EditMode/N46SharedTests.cs", csEol(`// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n46 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts
//      with "ON", as Tools/night/n45-rows-on.cjs writes); the six sim rows are in the registry with a numbers object, --off46 turns them all
//      off; the level and view rows carry no tape bit; the level rows' files exist (both copies) and are listed
//   2. with every n46 row off, the campaign order is NIGHT45's (the config without design.n46)
//   3. the off switch (NightRows.ForceOffFrom 46 = -skygrindOff46 / --off46, or 45) leaves NIGHT45's order with every level row ON; 47 swaps
//   4. every n46 level reads as its levelFrom's base to every older system (n46-foundry -> n45-foundry -> n43-foundry; n46-span -> n40-span)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N46SharedTests
    {
        static readonly string[] LevelRows = { "foundry46", "iceline46", "kiln46", "span46", "spire46" };
        static readonly string[] ViewRows = { "shift46", "flow46", "feel46" };
        static readonly string[] SimRows = { "retry46", "overlap46", "teach46", "reveal46", "wyrm46", "walkin46" };

        static string Repo => Path.GetDirectoryName(Port.Root);

        // a fresh config each time: GameConfig caches lookups and NightRows caches level rows by config, so a variant is edited before any read
        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N46(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n46"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N46(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN46()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n46");
            return c;
        }

        static List<string> Order(GameConfig c) => Curriculum25.Order(c, N25Blocks.From(c));

        static void Switches(int offFrom, System.Action body)
        {
            int was = NightRows.ForceOffFrom;
            bool was45 = N45Levels.ForceOff;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                N45Levels.ForceOff = false;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                N45Levels.ForceOff = was45;
            }
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n46._why"), Is.True, "design.n46._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n46." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n46." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n46." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n46." + row + ".numbers") && c.Get("design.n46." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 46; // -skygrindOff46: every n46 sim row off whatever the config says
                Assert.That(NightRows.From(c).Defaults().Any, Is.False, "--off46: no NIGHT46+ sim row on");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n46." + row + ".levelTo"), from = c.Str("design.n46." + row + ".levelFrom");
                Assert.That(to.StartsWith("n46-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight45sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night45 = Order(WithoutN46());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night45), "every n46 level row off: NIGHT45's order (" + string.Join(", ", night45.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n46." + row + ".levelFrom");
                    Assert.That(night45.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT45's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight45sOrder()
        {
            List<string> night45 = null;
            Switches(NightRows.NoOff, () => night45 = Order(WithoutN46()));
            foreach (int from in new[] { 46, 45, 19 })
                Switches(from, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night45), "ForceOffFrom " + from + " with every level row on: NIGHT45's order"));
            foreach (int from in new[] { 47, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night45.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night45.IndexOf(c.Str("design.n46." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == c.Str("design.n46." + row + ".levelTo"), Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN46Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n46." + row + ".levelFrom"), to = c.Str("design.n46." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(from), row + ": NightRows.BaseId");
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n46-foundry"), Is.EqualTo("n43-foundry"), "n46 -> n45 -> n43 (N43BaseId then continues)");
            Assert.That(N45Levels.BaseId(c, "n46-span"), Is.EqualTo("n40-span"), "level 4: THE LOOM's rooms and the walk pilot");
            Assert.That(N45Levels.BaseId(c, "n46-spire"), Is.EqualTo("n41-spire"), "level 5 (N41BaseId then continues to n40-spire)");
            Assert.That(N45Levels.BaseId(c, "n45-kiln"), Is.EqualTo("n43-kiln"), "NIGHT45's mapping unchanged");
            Assert.That(NightRows.BaseId(c, "n40-span"), Is.EqualTo("n40-span"), "any other id is itself");
        }
    }
}
`));

// 4. the forge's one-time fixes (see the header)
patch("Assets/Tests/EditMode/NightRowsTests.cs", [[
  '            Assert.That(NightRows.From(c).Count, Is.EqualTo(0), "NIGHT45 adds no sim rows (the registry is unwired tonight)");\n            Assert.That(NightRowsTape.Decode(NightRowsTape.Encode(NightRows.From(c).Defaults()), NightRows.From(c)).Any, Is.False);\n',
  '            // NIGHT46 (Tools/night/n46-shared.cjs): from NIGHT46 the real config has sim rows; NIGHT45\'s "no sim rows" pin is now: every row is a\n' +
  '            // well-formed NIGHT46+ row and its default round-trips through the tape record (no later night edits this test)\n' +
  '            NightRows reg = NightRows.From(c);\n' +
  '            foreach (NightRow r in reg.rows) Assert.That(r.night >= NightRows.FirstNight && NightRows.NightOfName(r.name) == r.night, Is.True, r.ToString());\n' +
  '            NightRowState d = reg.Defaults(), back = NightRowsTape.Decode(NightRowsTape.Encode(d), reg);\n' +
  '            for (int i = 0; i < reg.Count; i++) Assert.That(back.On(i), Is.EqualTo(d.On(i)), reg.rows[i].name);\n',
]]);
patch("Assets/Scripts/Game/SurvivalSim.Loom37.cs", [[
  "            string levelId = LevelDef.From(map.json).id;\n",
  "            string levelId = N45Levels.BaseId(cfg, LevelDef.From(map.json).id); // NIGHT46 (n46-shared.cjs): n46-span is n40-span here (loom40's o1-o3); an n46 id is only loaded when its row swapped it in\n",
]]);
patch("Assets/Scripts/Game/Bot/Walk38Span.cs", [[
  '|| sim.campaignLevel.id == "n40-span"); // NIGHT40: and its n40 level (loom40)',
  '|| N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) == "n40-span"); // NIGHT40: and its n40 level (loom40); NIGHT46: and n46-span (NightRows.BaseId)',
]]);
patch("Assets/Scripts/Game/FeelReel.N45.Yard.cs", [['bool n45 = lv.id == "n45-foundry";', 'bool n45 = NightRows.BaseId(g.cfg, lv.id) == "n45-foundry"; // NIGHT46: n46-foundry runs as its n45 rooms']]);
patch("Assets/Scripts/Game/FeelReel.N45.Mix.cs", [['bool onN45 = id == "n45-iceline";', 'bool onN45 = NightRows.BaseId(sim.cfg, id) == "n45-iceline"; // NIGHT46: n46-iceline runs as its n45 room']]);

// 5. the builders' hook sites, pre-placed so nobody edits Campaign24.cs / FeelReel.N27.cs tonight (every hook null / false = NIGHT45's game)
patch("Assets/Scripts/Core/Campaign24.cs", [
  // the retry path (Retry(): a death restarts the room): F1's retry46 picks the wave to restart at
  ["                wave = 0;\n                gapLeft = 0;\n                SeedRoom(Math.Max(0, room));\n",
   "                wave = retryWave46 != null ? retryWave46(r, wave) : 0; // NIGHT46 F1 (retry46): the wave to restart at (null: wave 1, NIGHT45)\n                gapLeft = 0;\n                SeedRoom(Math.Max(0, room));\n"],
  // the stand-in hold, the afterLesson wait, the lesson cap: F1's teach46 gives its own steps for each
  ["if (rooms43 && roomSteps < standInHoldSteps43) alive +=", "if (rooms43 && roomSteps < Hold46(Hold46StandIn, standInHoldSteps43)) alive +="],
  ["|| roomSteps >= afterLessonMaxSteps43)", "|| roomSteps >= Hold46(Hold46AfterLesson, afterLessonMaxSteps43))"],
  ["if (++lessonIdle43 >= lessonCapSteps43)", "if (++lessonIdle43 >= Hold46(Hold46LessonCap, lessonCapSteps43))"],
  // the wave release: F1's overlap46 releases the next wave on top of the living (as waveEarly33 does)
  ["bool early = ready && alive > 0 && waveEarly33 != null && waveEarly33(r, wave); // NIGHT33 E:",
   "bool early = ready && alive > 0 && ((waveEarly33 != null && waveEarly33(r, wave)) || (waveRelease46 != null && waveRelease46(r, wave, alive))); // NIGHT46 F1 (overlap46: waveRelease46; null = NIGHT45); NIGHT33 E:"],
  // the declarations, beside NIGHT33's delegates
  ["        public Func<int> pending33;\n",
   "        public Func<int> pending33;\n\n" +
   "        // ---- NIGHT46 (Tools/night/n46-shared.cjs; design.n46 sim rows, set by builder F1 in SurvivalSim.N46.Flow.cs's N46FlowCampaign). All null = NIGHT45's\n" +
   "        // rooms bit for bit (the fingerprint's and every older tape's).\n" +
   "        /// <summary>NIGHT46 retry46: after a death, the wave the room restarts at (room, waves spawned so far); null: 0 (wave 1).</summary>\n" +
   "        public Func<RoomDef, int, int> retryWave46;\n" +
   "        /// <summary>NIGHT46 overlap46: release the next wave now, on top of the living (room, next wave, alive)? Null: never (only at 0 alive).</summary>\n" +
   "        public Func<RoomDef, int, int, bool> waveRelease46;\n" +
   "        /// <summary>NIGHT46 teach46: a teaching hold's steps (which: Hold46StandIn / Hold46AfterLesson / Hold46LessonCap; rooms43's steps); null: rooms43's.</summary>\n" +
   "        public Func<int, int, int> teachHold46;\n" +
   "        public const int Hold46StandIn = 0, Hold46AfterLesson = 1, Hold46LessonCap = 2;\n" +
   "        int Hold46(int which, int steps43) => teachHold46 != null ? teachHold46(which, steps43) : steps43;\n"],
]);
patch("Assets/Scripts/Game/SurvivalSim.Campaign24.cs", [[
  "                campaign.commitOff43 = rooms43CommitOff; // NIGHT43 E1: the harness's control (false in the game)\n",
  "                campaign.commitOff43 = rooms43CommitOff; // NIGHT43 E1: the harness's control (false in the game)\n" +
  "                N46Campaign(); // NIGHT46 (n46-shared.cjs): the night's builders set their Campaign24 hooks (SurvivalSim.N46.cs; rows off: none)\n",
]]);
patch("Assets/Scripts/Game/SurvivalSim.N36.cs", [[
  "public bool Boss36WalkIn => (n36.walkin36 && boss36WalkInHere) || boss37WalkInHere; // NIGHT37:",
  "public bool Boss36WalkIn => (n36.walkin36 && boss36WalkInHere) || boss37WalkInHere || walkIn46; // NIGHT46 B1 (walkin46, SurvivalSim.N46.cs; false = NIGHT45); NIGHT37:",
]]);
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN45, s) >= 0;", "|| Array.IndexOf(PhaseN45, s) >= 0 || Array.IndexOf(PhaseN46, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN45, s) >= 0) N45ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT45 A1 / A2 / A3 (FeelReel.N45.cs)\n",
   "                    else if (Array.IndexOf(PhaseN45, s) >= 0) N45ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT45 A1 / A2 / A3 (FeelReel.N45.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN46, s) >= 0) N46ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT46 A1 / A2 / A3 / B1 / V1 (FeelReel.N46.cs)\n"],
]);

// every check passed: write
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT46 shared commit done: " + writes.length + " file(s). Next: dn-check, golden, fingerprint plain / --off45 / --off46, n45rows, cmp the configs, the Editor compile, N46SharedTests + NightRowsTests + Lesson25.");

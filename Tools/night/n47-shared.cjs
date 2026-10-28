// NIGHT47 shared commit (the lead's, modeled on n46-shared.cjs; docs/NIGHT-ROWS.md section 2). Four builders tonight (GG's /goal, 2026-10-18):
//   W = THE WICK (G1-G9), B = levels 2 + 3 (G25-G35), L = levels 4 + 5 (G36-G43 boss parts), F = feel / HUD / words / looks / render (G10-G24, TORCH).
//   node Tools/night/n47-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n47, every row enabled:false with a _why; five level rows (n46-* -> n47-*) + their
//      design.maps.list rows after n46-spire's; the sim rows (numbers {} for their builder); the view rows.
//   2. Five placeholder levels (copies of the n46 files with only the outer "name" changed), both copies, + the StreamingAssets .meta.
//   3. Assets/Scripts/Game/SurvivalSim.N47.cs: Night47StartRun / Night47Step / N47Campaign -> each builder's partials (N47W*, N47B*, N47L*, N47F*).
//   4. Assets/Scripts/Game/FeelReel.N47.cs: reel phases a147 (W), a247 / a347 (B), a447 / a547 (L), a647 / a747 / a847 (F), routed from FeelReel.N27.cs.
//   5. Assets/Tests/EditMode/N47SharedTests.cs: the rows exist and are off; every row off = NIGHT46's order; --off47 = NIGHT46's order; bases.
//   6. Hook sites: SurvivalSim.Campaign24.cs calls N47Campaign() after N46Campaign().
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/SurvivalSim.N46.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-18 (after playing n46-r3): the WICK's climb 'far too unforgiving', three phases, 'two to three minutes maximum', the chargers' mini challenge, the worm 'appear earlier ... burrow', THE LOOM and THE CROWN overhauled, level 5 'doesn't make sense', heavier weapons, a new HUD, human words (NIGHT47.md section 2, G1-G45)";
const off = "Off until the lead has looked at its frames and its gates pass (NIGHT47.md section 1: a GG ask is ON at the freeze when its gates are GREEN, verbs37 passes on levels 1-5, the smoke passes and the frames show the change).";

// 1. config
const levels = [ // [row, levelFrom, levelTo, why]
  ["foundry47", "n46-foundry", "n47-foundry", "Builder W (level row): THE WICK's arena: P1's modified layout, P2's climb platforms, P3's top and the translucent floor (G1-G3, G8), in n47-foundry (from n46-foundry; untouched)."],
  ["iceline47", "n46-iceline", "n47-iceline", "Builder B (level row): the CHARGER's teaching beat in level 2's first third, CHARGERS in 3+ more rooms, the stampede room before THE ICEBREAKER (G25, G27), in n47-iceline (from n46-iceline; untouched)."],
  ["kiln47", "n46-kiln", "n47-kiln", "Builder B (level row): THE KILN WYRM foreshadowed in 3 beats before its fight and its arena rebuilt for the THUNDERCLAP's boost (G33, G35), in n47-kiln (from n46-kiln; untouched)."],
  ["span47", "n46-span", "n47-span", "Builder L (level row): THE LOOM's new arena, built so the BEND is the star (G39, G40), in n47-span (from n46-span; untouched)."],
  ["spire47", "n46-spire", "n47-spire", "Builder L (level row): level 5 made to make sense (a premise, a readable route, each room's job) and THE CROWN's arena (G41, G42), in n47-spire (from n46-spire; untouched)."],
];
const simRows = [ // [row, why]
  ["wick47", "Builder W (sim row): THE WICK as a three-phase fight (P1 modified arena, P2 the climb, P3 the remix), 2+ new attacks, P3's crack-open charge with a yellow weak point, a melee hit 20-25 % of the bar + a 12-18 m launch, the human band 90-150 s (G3, G4, G6, G7). SurvivalSim.N47.Wick.cs."],
  ["climb47", "Builder W (sim row, Mover): the climb's forgiveness: a wall jump off a climb wall SETS the vertical speed, a wall coyote, a jump buffer onto a wall, a generous catch, the wall jump count refilled on a climb ledge (G1, G2; DECIDE47: N-5 lifted for this row only). Off = NIGHT46's movement bit for bit."],
  ["wickTop47", "Builder W (sim row): the translucent floor that fills the shaft when the player reaches the top (G8): it holds the player and enemies."],
  ["charger47", "Builder B (sim row): the CHARGER's new behavior: a 0.8-1.0 s wind-up with a lit path, a charge that turns badly, a ~2 s wall stun with open back vents (the weak spot), counters for every verb, a CHARGER that hits another enemy bursts it (G26)."],
  ["brawl47", "Builder B (sim row): THE ICEBREAKER's fight: the boss card's attacks and phases, the human band 90-150 s (G28-G30)."],
  ["wyrm47", "Builder B (sim row): THE KILN WYRM's tells (0.8-1.5 s, saying where), the burrow (dive, travel under the floor, breach) brought back, the foreshadow beats' sim side (G31, G34, G35); the fight's core and length kept."],
  ["loom47", "Builder L (sim row): THE LOOM rebuilt: 6+ attacks across a multi-phase fight that makes the BEND the star, the human band 90-150 s (G36-G40)."],
  ["crown47", "Builder L (sim row): THE CROWN rebuilt to be fun: new attacks and phases, its flame attack with weight, the human band 90-150 s (G41, G43 boss part)."],
  ["pulse47", "Builder F (sim row): the gauntlet's LT blast: more damage and at least 2x the knockback on a husk (G20)."],
  ["overheal47", "Builder F (sim row, the lead's call; DECIDE47): health above 100 from kill spills up to a cap, decaying to 100 slowly (decay, not regen: the w19 rule holds) (G17)."],
];
const viewRows = [ // [row, why]
  ["wickLook47", "Builder W (view row): THE WICK's phase looks, the crack-open weak point's indicators, the transition beat, the fight's words (G3, G5, G7)."],
  ["chargerLook47", "Builder B (view row): the CHARGER's new look (armored triangle front plate, vents, the lit path) and its animation (G26)."],
  ["brawlLook47", "Builder B (view row): THE ICEBREAKER's new look, spring animation and per-attack hit effects (G28-G30)."],
  ["wyrmLook47", "Builder B (view row): THE KILN WYRM's molten yellow-red-orange look made cooler, and its tells' looks (G31, G32)."],
  ["loomLook47", "Builder L (view row): THE LOOM's new design, level 4's colors, fluid spring animation (G36, G37)."],
  ["crownLook47", "Builder L (view row): THE CROWN's look and its flame's look (red-orange family, weight) (G41, G43 boss part)."],
  ["fx47", "Builder F (view row): every weapon's effects, animation and impact (KESTREL, THUNDERCLAP, PULSE, TORCH, fist, swap), the LT blast's look, the punch aura (fist blue, gauntlet pink) (G20, G21, G24, G43 TORCH)."],
  ["grapple47", "Builder F (view row): the grapple's one long glowing aura line instead of the dotted line (G23)."],
  ["zoom47", "Builder F (view row): the THUNDERCLAP's LT as a proper shouldered zoom (G22)."],
  ["hud47", "Builder F (view row): one continuous health bar (green to red, OKLab), overheal blue to purple, the enemy counter, the reticle alone (arc40 / meter42's info moved to the gun and the weapon panel), fight text readable (G5, G16-G19)."],
  ["copy47", "Builder F (view row): every player-facing string rewritten to sound human (docs/COPY47.md) (G15)."],
  ["visor47", "Builder F (view row): every enemy's visor sharper, angular, thicker, futuristic glasses (G13)."],
  ["foes47", "Builder F (view row): the WARDEN's shield placed right, the small robots' cohesive design, enemy punch auras in the red-orange family (G12, G14, G21 enemy side)."],
  ["door47", "Builder F (render row): the door's pink / broken frames fixed at their cause (G10)."],
  ["lava47", "Builder F (render row): one lava light rule on levels 1-5, no popping (G11)."],
];
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  let raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n47) throw new Error("design.n47 already exists (already run?)");
  if (!d.design.n46) throw new Error("design.n46 is missing");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const listed = {};
  for (const m of d.design.maps.list) listed[m.id] = m;
  for (const [row, from, to] of levels) {
    if (!listed[from]) throw new Error(row + ": its levelFrom " + from + " is not in design.maps.list");
    if (listed[to]) throw new Error(row + ": " + to + " is already in design.maps.list");
  }
  const anchorRe = /(\{\s*"id":\s*"n46-spire",[^}]*\})/;
  const m = raw.match(anchorRe);
  if (!m || raw.split('"id": "n46-spire"').length - 1 !== 1) throw new Error("the n46-spire maps.list row was not found exactly once");
  const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
  const add = levels.map(([, from, id]) => "," + eol + indent + JSON.stringify({ id, label: listed[from].label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
  raw = raw.replace(anchorRe, m[1] + add);
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const n47 = { _why: GG };
  for (const [row, from, to, why] of levels) n47[row] = { _why: why, enabled: false, enabled_why: off, levelFrom: from, levelTo: to };
  for (const [row, why] of simRows) n47[row] = { sim: true, _why: why, enabled: false, enabled_why: off, numbers: {} };
  for (const [row, why] of viewRows) n47[row] = { _why: "View row (NightRows.ViewOn(cfg, \"" + row + "\")). " + why, enabled: false, enabled_why: off, numbers: {} };
  const block = JSON.stringify(n47, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n47\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n47) !== JSON.stringify(n47)) throw new Error("design.n47 did not round-trip");
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
addNew("Assets/Scripts/Game/SurvivalSim.N47.cs", csEol(`// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the night's sim rows (design.n47.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   W (SurvivalSim.N47.Wick.cs):  wick47, climb47, wickTop47 -> N47WStartRun / N47WStep / N47WCampaign
//   B (SurvivalSim.N47.Brawl.cs): charger47, brawl47, wyrm47 -> N47BStartRun / N47BStep / N47BCampaign
//   L (SurvivalSim.N47.Loom.cs):  loom47, crown47            -> N47LStartRun / N47LStep / N47LCampaign
//   F (SurvivalSim.N47.Feel.cs):  pulse47, overheal47        -> N47FStartRun / N47FStep / N47FCampaign
// Off: -skygrindOff47 / --off47 (or any older switch): every id reads off. The level rows (foundry47 ... spire47) and the view rows are data only:
// Curriculum25 swaps the levels; a view asks NightRows.ViewOn(cfg, "<row>").
// A builder resolves its ids once a run in its StartRun and reads rows.On(id) per step (an array read, no allocation). Each builder owns its bosses'
// Core files (Core/Enemies/*) and calls them from its own partial; nobody edits SurvivalSim.Rows.cs, FeelReel.N27.cs or this file tonight.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idWick47 = -1, idClimb47 = -1, idWickTop47 = -1; // W
        internal int idCharger47 = -1, idBrawl47 = -1, idWyrm47 = -1; // B
        internal int idLoom47 = -1, idCrown47 = -1; // L
        internal int idPulse47 = -1, idOverheal47 = -1; // F

        partial void Night47StartRun()
        {
            idWick47 = rowsRegistry.IndexOf("wick47");
            idClimb47 = rowsRegistry.IndexOf("climb47");
            idWickTop47 = rowsRegistry.IndexOf("wickTop47");
            idCharger47 = rowsRegistry.IndexOf("charger47");
            idBrawl47 = rowsRegistry.IndexOf("brawl47");
            idWyrm47 = rowsRegistry.IndexOf("wyrm47");
            idLoom47 = rowsRegistry.IndexOf("loom47");
            idCrown47 = rowsRegistry.IndexOf("crown47");
            idPulse47 = rowsRegistry.IndexOf("pulse47");
            idOverheal47 = rowsRegistry.IndexOf("overheal47");
            N47WStartRun(); // builder W
            N47BStartRun(); // builder B
            N47LStartRun(); // builder L
            N47FStartRun(); // builder F
        }

        partial void Night47Step(double dt)
        {
            N47WStep(dt); // builder W
            N47BStep(dt); // builder B
            N47LStep(dt); // builder L
            N47FStep(dt); // builder F
        }

        /// <summary>Once a run, right after N46Campaign (the campaign is made): the builders set their Campaign24 delegates here, only for rows
        /// that are on (a null delegate is NIGHT46's rule).</summary>
        partial void N47Campaign()
        {
            N47WCampaign();
            N47BCampaign();
            N47LCampaign();
            N47FCampaign();
        }

        partial void N47Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N47WStartRun(); // W: reset every per-run field here (rule 6); numbers from design.n47.<row>.numbers
        partial void N47WStep(double dt);
        partial void N47WCampaign();
        partial void N47BStartRun(); // B
        partial void N47BStep(double dt);
        partial void N47BCampaign();
        partial void N47LStartRun(); // L
        partial void N47LStep(double dt);
        partial void N47LCampaign();
        partial void N47FStartRun(); // F
        partial void N47FStep(double dt);
        partial void N47FCampaign();
    }
}
`));

// 3. the reel hooks
addNew("Assets/Scripts/Game/FeelReel.N47.cs", csEol(`// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partials in its own file: a147 (W: FeelReel.N47.Wick.cs), a247 / a347 (B: FeelReel.N47.Brawl.cs /
// FeelReel.N47.Wyrm.cs), a447 / a547 (L: FeelReel.N47.Loom.cs / FeelReel.N47.Crown.cs), a647 / a747 / a847 (F: FeelReel.N47.Hud.cs /
// FeelReel.N47.Fx.cs / FeelReel.N47.Look.cs). A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). A reel forces a sim row
// before the run starts with sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true); view rows are forced by the build copy's config
// (node Tools/night/n46-build-rows.cjs <tag> on n47.<row>) or by a reel-only switch the builder reads.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN47 = { "a147", "a247", "a347", "a447", "a547", "a647", "a747", "a847" };

        static void N47ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a147") N47Wick(g, dir, res, offs, pre, ref body);
            else if (s == "a247") N47Brawl(g, dir, res, offs, pre, ref body);
            else if (s == "a347") N47Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a447") N47Loom(g, dir, res, offs, pre, ref body);
            else if (s == "a547") N47Crown(g, dir, res, offs, pre, ref body);
            else if (s == "a647") N47Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a747") N47Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a847") N47Look(g, dir, res, offs, pre, ref body);
        }

        static partial void N47Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
        static partial void N47Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N47Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N47Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N47Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N47Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N47Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N47Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
    }
}
`));

// 4. the shared tests
addNew("Assets/Tests/EditMode/N47SharedTests.cs", csEol(`// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n47 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows are in the registry with a numbers object, --off47 turns them all off; the level and view rows carry no tape bit;
//      the level rows' files exist (both copies) and are listed
//   2. with every n47 row off, the campaign order is NIGHT46's (the config without design.n47)
//   3. the off switch (NightRows.ForceOffFrom 47 = -skygrindOff47 / --off47, or 46) leaves NIGHT46's order with every n47 level row ON
//   4. every n47 level reads as its levelFrom's base to every older system (n47-foundry -> n46-foundry -> n45-foundry -> n43-foundry)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N47SharedTests
    {
        static readonly string[] LevelRows = { "foundry47", "iceline47", "kiln47", "span47", "spire47" };
        static readonly string[] ViewRows = { ${viewRows.map(([r]) => '"' + r + '"').join(", ")} };
        static readonly string[] SimRows = { ${simRows.map(([r]) => '"' + r + '"').join(", ")} };

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N47(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n47"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N47(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN47()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n47");
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
            Assert.That(c.Has("design.n47._why"), Is.True, "design.n47._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n47." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n47." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n47." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n47." + row + ".numbers") && c.Get("design.n47." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 47; // -skygrindOff47: every n47 sim row off whatever the config says
                NightRowState d = NightRows.From(c).Defaults();
                foreach (string row in SimRows) Assert.That(d.On(reg.IndexOf(row)), Is.False, "--off47: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n47." + row + ".levelTo"), from = c.Str("design.n47." + row + ".levelFrom");
                Assert.That(to.StartsWith("n47-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight46sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night46 = Order(WithoutN47());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night46), "every n47 level row off: NIGHT46's order (" + string.Join(", ", night46.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n47." + row + ".levelFrom");
                    Assert.That(night46.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT46's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight46sOrder()
        {
            List<string> night46 = null;
            Switches(NightRows.NoOff, () => night46 = Order(WithoutN47()));
            foreach (int from in new[] { 47 })
                Switches(from, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night46), "ForceOffFrom " + from + " with every n47 level row on: NIGHT46's order"));
            foreach (int from in new[] { 48, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night46.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night46.IndexOf(c.Str("design.n47." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == c.Str("design.n47." + row + ".levelTo"), Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN47Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n47." + row + ".levelFrom"), to = c.Str("design.n47." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n47-foundry"), Is.EqualTo("n43-foundry"), "n47 -> n46 -> n45 -> n43");
            Assert.That(N45Levels.BaseId(c, "n47-span"), Is.EqualTo("n40-span"), "level 4: THE LOOM's rooms and the walk pilot");
            Assert.That(N45Levels.BaseId(c, "n47-spire"), Is.EqualTo("n41-spire"), "level 5");
        }
    }
}
`));

// 5. hook sites
patch("Assets/Scripts/Game/SurvivalSim.Campaign24.cs", [[
  "                N46Campaign(); // NIGHT46 (n46-shared.cjs): the night's builders set their Campaign24 hooks (SurvivalSim.N46.cs; rows off: none)\n",
  "                N46Campaign(); // NIGHT46 (n46-shared.cjs): the night's builders set their Campaign24 hooks (SurvivalSim.N46.cs; rows off: none)\n" +
  "                N47Campaign(); // NIGHT47 (n47-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N47.cs; rows off: none)\n",
]]);
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN46, s) >= 0;", "|| Array.IndexOf(PhaseN46, s) >= 0 || Array.IndexOf(PhaseN47, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN46, s) >= 0) N46ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT46 A1 / A2 / A3 / B1 / V1 (FeelReel.N46.cs)\n",
   "                    else if (Array.IndexOf(PhaseN46, s) >= 0) N46ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT46 A1 / A2 / A3 / B1 / V1 (FeelReel.N46.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN47, s) >= 0) N47ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT47 W / B / L / F (FeelReel.N47.cs)\n"],
]);

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT47 shared commit done: " + writes.length + " file(s). Next: dn-check, golden, fingerprint plain / --off46 / --off47, n45rows, cmp the configs, the Editor compile, N47SharedTests + N46SharedTests + NightRowsTests.");

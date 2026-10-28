// NIGHT49 shared commit (the lead's; modeled on n48-shared.cjs; docs/NIGHT-ROWS.md section 2). Tonight's rows are all VIEW rows (no sim row):
//   charge49 (H, G1), look49 (V, G2-G4), lights49 (L, G2-G4), fx49 (X, G5), drone49 (the lead, P0: NIGHT48 F's drone part behind foes48).
//   node Tools/night/n49-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n49 with the five view rows, every one enabled:false with a _why and enabled_why.
//      design.n48 and design.n47 are not touched.
//   2. Assets/Scripts/Game/UnityTuning.Look49.cs: THE ONE PALETTE (the role tokens; the only colour table tonight; DECIDE49 D-3).
//   3. Assets/Scripts/Game/FeelReel.N49.cs: the reel phases a149 (H), a249 (X), a349 (V), a449 (L), routed from FeelReel.N27.cs.
//   4. Assets/Tests/EditMode/N49SharedTests.cs: the rows exist and are off; --off49 turns them all off; the checker passes.
//   5. Patches (one anchor each, asserted): FeelReel.N27.cs (the routing), EnemyViews.Drone.cs (the drone49 gate), FeelReel.cs (the Capture fix:
//      a frame whose index did not advance is skipped, so no f{index}.jpg is written twice: NIGHT48's f1799 overwrite).
//   6. Tools/night/n49-lookmeter.py: a copy of Research/2026-10-19-wave49/lookmeter.py (the look meter).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const copies = [];
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

const GG = "GG 2026-10-19 (~11:20, after NIGHT48's close): the charge bar back under the reticle, one unified look across the five levels, one impact kit for every effect, the level lights in their roles, in the ULTRAKILL / DOOM spirit (NIGHT49.md section 0)";
const off = "Off until the lead has looked at its frames and its blind pair is won or tied, and the freeze gates pass (NIGHT49.md sections 4 and 6). A row is ON at the freeze only with its frames in NIGHT49-LOG and verbs37 passing.";
const viewRows = [ // [row, why]
  ["charge49", "Builder H (view row, G1): the one charge element under the reticle for every charge source (KESTREL charge, THUNDERCLAP slug, PULSE blast, BEND, STITCHER heat). Its colours come from UnityTuning.Look49*; its sizes from UnityTuning.H49.cs."],
  ["look49", "Builder V (view row, G2-G4): the one grade and colour script per level (Look49's per-level values, PostFx's grade, fog, ambient, key and fill). Its numbers: UnityTuning.V49.cs."],
  ["lights49", "Builder L (view row, G2-G4): the level lights in their roles (cyan only on usable fixtures, white only on the boundary, each landmark lit as the room's strongest light). Its numbers: UnityTuning.L49.cs."],
  ["fx49", "Builder X (view row, G5): one impact kit (tiers light / heavy / critical x owners; one timing table in whole sim steps). Its numbers: UnityTuning.Fx49.cs."],
  ["drone49", "The lead (P0; NIGHT48 F's round 6 drone, DECIDE49 D-2): the drone's own-scale rim band (EnemyViews.Drone.cs), read only with foes48 on. OFF = the drone draws its NIGHT47 look with foes48 on (the old drone won 3-0 three times). Named drone49, not foes48drone: NightRows.ViewOn reads the night from the row's name."],
];

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n49) throw new Error("design.n49 already exists (already run?)");
  if (!d.design.n48) throw new Error("design.n48 is missing");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending: design.n48 must be the last key of design");
  const n49 = { _why: GG };
  for (const [row, why] of viewRows) n49[row] = { _why: "View row (NightRows.ViewOn(cfg, \"" + row + "\")). " + why, enabled: false, enabled_why: off, numbers: {} };
  const block = JSON.stringify(n49, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n49\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n49) !== JSON.stringify(n49)) throw new Error("design.n49 did not round-trip");
  if (JSON.stringify(back.design.n48) !== JSON.stringify(d.design.n48)) throw new Error("design.n48 changed");
  if (JSON.stringify(back.design.n47) !== JSON.stringify(d.design.n47)) throw new Error("design.n47 changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the one palette (DECIDE49 D-3): the role tokens. Seeded from the web's own colours and the constants in use, so the look on screen does not move at G0.
addNew("Assets/Scripts/Game/UnityTuning.Look49.cs", csEol(`// NIGHT49 shared commit (Tools/night/n49-shared.cjs): THE ONE PALETTE. These role tokens are the only colour table tonight: charge49, look49,
// lights49 and fx49 read them and hold only their own sizes and timings (UnityTuning.H49.cs, V49, L49, Fx49). View only: nothing here touches the sim.
// CLAUDE.md rule 8 roles: cyan = player-usable; red-orange = enemies and danger only; yellow = punch it / parryable; magenta = RIVAL only;
// pale violet-white = the boundary. The first four values are the web's own colours (config colors.usable, colors.enemy) or the constants in use
// (Rival25 RivalMagentaHex, roof27 boundaryHex), so the screen does not move at the shared commit. The parry yellow is a Proposal (UNIFY.md's
// role bands; hue 44 deg, 31 deg from the danger's 12 deg), not measured: V / X may propose a change in their report.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Player-usable cyan (config colors.usable 0x19D3E6; hue about 186 deg).</summary>
        public const uint Look49UsableHex = 0x19D3E6;
        /// <summary>Enemies and danger, red-orange (config colors.enemy 0xFF4A1C; hue about 12 deg). Nothing else takes this hue at high saturation.</summary>
        public const uint Look49DangerHex = 0xFF4A1C;
        /// <summary>Parryable / punch it: yellow (a Proposal, DECIDE49 D-3; hue about 44 deg, at least 20 deg from the danger and the usable).</summary>
        public const uint Look49ParryHex = 0xFFC93A;
        /// <summary>RIVAL only (Rival25 RivalMagentaHex 0xff2bd6; hue about 312 deg). The gauntlet's pink (Fx47BodyHex, Feel43 pink) stays apart from it.</summary>
        public const uint Look49RivalHex = 0xFF2BD6;
        /// <summary>The boundary: pale violet-white (roof27 boundaryHex 0xd8d0ff; hue about 250 deg).</summary>
        public const uint Look49BoundaryHex = 0xD8D0FF;
        /// <summary>A level accent must sit at least this many degrees of hue from every role token above (rule 8; V and L read it as the guard).</summary>
        public const float Look49AccentMinHueGapDeg = 20f;
    }
}
`));

// 3. the reel phases (a149 H, a249 X, a349 V, a449 L): the builders fill their own partials
addNew("Assets/Scripts/Game/FeelReel.N49.cs", csEol(`// NIGHT49 shared commit (Tools/night/n49-shared.cjs): the proof reels of tonight's builders, dispatched from FeelReel.N27.cs (the lead's two
// lines). a149 = H (the charge element: FeelReel.N49.Charge.cs), a249 = X (the impact kit: FeelReel.N49.Fx.cs), a349 = V (the grade and the
// colour script: FeelReel.N49.Look.cs), a449 = L (the level lights: FeelReel.N49.Lights.cs). A partial with no body leaves body null (res[0]
// stays NOT WRITTEN, RED). A reel forces a view row with the build copy's config (node Tools/night/n39-build-rows.cjs <tag> on n49.<row>).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN49 = { "a149", "a249", "a349", "a449" };

        static void N49ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a149") N49Charge(g, dir, res, offs, pre, ref body);
            else if (s == "a249") N49Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a349") N49Look(g, dir, res, offs, pre, ref body);
            else if (s == "a449") N49Lights(g, dir, res, offs, pre, ref body);
        }

        static partial void N49Charge(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // H
        static partial void N49Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // X
        static partial void N49Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // V
        static partial void N49Lights(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
    }
}
`));

// 4. the shared tests
addNew("Assets/Tests/EditMode/N49SharedTests.cs", csEol(`// NIGHT49 shared commit (Tools/night/n49-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n49 row exists with a _why and a bool enabled, is off unless a freeze turned it on (its enabled_why then starts with "ON"),
//      and NightRows.ViewOn reads the same switch; the checker finds no problem in design
//   2. the off switch: ForceOffFrom 49 (-skygrindOff49 / --off49) turns every n49 view row off whatever the config says
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N49SharedTests
    {
        static readonly string[] ViewRows = { ${viewRows.map(([r]) => '"' + r + '"').join(", ")} };

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n49._why"), Is.True, "design.n49._why");
            foreach (string row in ViewRows)
            {
                string p = "design.n49." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n49." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
                Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(on), row + ": NightRows.ViewOn reads the design.n49 switch");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void OffSwitch49_TurnsEveryViewRowOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 49; // -skygrindOff49 / --off49
                GameConfig c = Load();
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off49: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

// 5. patches (one anchor each)
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN48, s) >= 0;", "|| Array.IndexOf(PhaseN48, s) >= 0 || Array.IndexOf(PhaseN49, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN48, s) >= 0) N48ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT48 W / B / L / F (FeelReel.N48.cs)\n",
   "                    else if (Array.IndexOf(PhaseN48, s) >= 0) N48ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT48 W / B / L / F (FeelReel.N48.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN49, s) >= 0) N49ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT49 H / X / V / L (FeelReel.N49.cs)\n"],
]);
patch("Assets/Scripts/Game/EnemyViews.Drone.cs", [[
  "                if (b.l43.on && b.l43.extras != null && b.l43.extras.parts.Length > 0) Band47(b, b.l43.extras.parts[0], f47 && !Foes48.On(game.cfg));\n",
  "                // NIGHT49 P0 (DECIDE49 D-2): the own-scale band is NIGHT48's drone part, read only with drone49 on; off, the drone keeps its NIGHT47 look with foes48 on\n" +
  "                if (b.l43.on && b.l43.extras != null && b.l43.extras.parts.Length > 0) Band47(b, b.l43.extras.parts[0], f47 && !(Foes48.On(game.cfg) && NightRows.ViewOn(game.cfg, \"drone49\")));\n",
]]);
patch("Assets/Scripts/Game/FeelReel.cs", [
  ["            var end = new WaitForEndOfFrame();\n            while (capturing)\n",
   "            var end = new WaitForEndOfFrame();\n            int lastCaptured = -1; // NIGHT49 shared (the lead's Capture fix)\n            while (capturing)\n"],
  ["                if (f < 0) continue;\n                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();\n",
   "                if (f < 0) continue;\n" +
   "                // NIGHT49 shared (the lead's Capture fix; NIGHT48's f1799.jpg was overwritten 20-110 times a long reel): a frame whose index did not advance is\n" +
   "                // skipped, so no f{index}.jpg is written twice; a stop still ends the recording\n" +
   "                if (f <= lastCaptured)\n                {\n                    if (stopAsked) { capturing = false; FeelProbe.End(); }\n                    continue;\n                }\n" +
   "                lastCaptured = f;\n" +
   "                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();\n"],
]);

// 6. the look meter (a copy)
copies.push(["Research/2026-10-19-wave49/lookmeter.py", "Tools/night/n49-lookmeter.py"]);

if (dry) {
  for (const [p] of writes) console.log("would write " + p);
  for (const [a, b] of copies) console.log("would copy " + a + " -> " + b);
  console.log("DRY RUN: " + writes.length + " file(s), " + copies.length + " copy(ies), every anchor found once");
  process.exit(0);
}
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [a, b] of copies) fs.copyFileSync(R(a), R(b));
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT49 shared commit done: " + writes.length + " file(s), " + copies.length + " copy. Next: dn-check, golden, fingerprint plain / --off48 / --off49, the Editor compile, N49SharedTests.");

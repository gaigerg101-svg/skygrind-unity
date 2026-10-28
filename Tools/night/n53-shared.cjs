// NIGHT53 shared commit (the lead's; modeled on n52-shared.cjs; NIGHT53.md section 3 step 4, DECIDE53 D-1). Rooms are edited IN PLACE in
// Design/levels/n52/*.plan.json (no n53 level ids: L-074 / L-077), so tonight has one config row only: the polish lane's view row.
//   node Tools/night/n53-shared.cjs [rootDir] [--dry]   (refuses to run twice; asserts every anchor exactly once BEFORE writing anything)
// What it writes:
//   1. Design/unity-config.json (+ its byte copy): design.n53 = { _why, polish53 } (a view row, enabled:false, numbers with one placeholder
//      object per U item: builder U fills only its own keys, L-049 / L-065). `-skygrindOff53` / `--off53` gives NIGHT52's look.
//   2. Assets/Scripts/Game/FeelReel.N53.cs: reel phase a153 (U: FeelReel.N53.Polish.cs, polish53's proofs), routed from FeelReel.N27.cs.
//   3. Assets/Tests/EditMode/N53SharedTests.cs: the row exists, off by default (or ON with an "ON" reason), ViewOn reads it, --off53 and
//      --off52 turn it off, its numbers hold U's placeholders; design.n53 has no level row (the campaign order is NIGHT52's).
//   4. Tools/night/n53-rows.json (every row passes:false) and Tools/night/n53-queue.json (the loop's queue, seeded from section 5 and 8).
const fs = require("fs");
const dry = process.argv.includes("--dry");
const rootArg = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = (rootArg || process.cwd()).replace(/\\/g, "/").replace(/\/?$/, "/");
const R = (p) => root + p;
function guid() { return [...Array(32)].map(() => "0123456789abcdef"[Math.floor(Math.random() * 16)]).join(""); }

const writes = [];
const crlfCs = fs.readFileSync(R("Assets/Scripts/Game/FeelReel.N52.cs"), "utf8").includes("\r\n");
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

const GG = "GG 2026-10-20 ~07:05 (NIGHT53.md): another 3.5 h polish and build, iterate, same methods as the previous prompt, research real proven data. NIGHT53 is THE SECOND PASS: the rooms NIGHT52 rebuilt are edited in place (design.n52's level rows; tag u3-n52 is the comparison side) to pass THE IDENTITY CHECKS, and the polish lane closes NIGHT52's SHORT items behind one view row";
const off = "Off until its check in Tools/night/n53-rows.json passes (printed by the lead) and the freeze gates pass (NIGHT53.md section 7: rows ON at the freeze by their checks).";
const items = [ // U's placeholders, in NIGHT53.md section 4's order
  ["firstShot", "T-122 the first-shot hitch: what is prewarmed (U)"],
  ["killWash", "T-111 the kill wash's f3-f4 source and its cap (U)"],
  ["scorchCore", "T-120 the scorch core shown hot (U; NIGHT52 shipped scorchGlow 0)"],
  ["icelineGrade", "T-116 iceline's grade to 4/4 on the look meter (U)"],
  ["killPop", "the kill-confirm HUD pop with UIEffect + LitMotion (U)"],
];

// 1. config
{
  const file = R("Design/unity-config.json"), copy = R("Assets/StreamingAssets/Design/unity-config.json");
  const raw = fs.readFileSync(file, "utf8");
  if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
  const d = JSON.parse(raw);
  if (d.design.n53) throw new Error("design.n53 already exists (already run?)");
  if (!d.design.n52) throw new Error("design.n52 is missing");
  if (Object.keys(d.design).pop() !== "n52") throw new Error("design.n52 must be the last key of design");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const tail = eol + "    }" + eol + "  }" + eol + "}";
  if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
  const numbers = {};
  for (const [k, why] of items) { numbers[k] = {}; numbers[k + "_why"] = why; }
  const n53 = {
    _why: GG,
    polish53: { _why: "View row (NightRows.ViewOn(cfg, \"polish53\")). Builder U (NIGHT53.md section 4): NIGHT52's SHORT polish items (T-122, T-111, T-120, T-116) and a kill-confirm HUD pop; each item reads its own numbers object. Off = NIGHT52's look.", enabled: false, enabled_why: off, numbers },
  };
  const block = JSON.stringify(n53, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
  const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n53\": " + block + eol + "  }" + eol + "}";
  const back = JSON.parse(out);
  if (JSON.stringify(back.design.n53) !== JSON.stringify(n53)) throw new Error("design.n53 did not round-trip");
  const a = JSON.parse(out); delete a.design.n53;
  if (JSON.stringify(a) !== JSON.stringify(d)) throw new Error("something beyond design.n53 changed");
  writes.push(["Design/unity-config.json", out], ["Assets/StreamingAssets/Design/unity-config.json", out]);
}

// 2. the reel hook
addNew("Assets/Scripts/Game/FeelReel.N53.cs", csEol(`// NIGHT53 (the shared commit; Tools/night/n53-shared.cjs): builder U's proof reel, dispatched from FeelReel.N27.cs (the lead's line).
// a153 (U: FeelReel.N53.Polish.cs, polish53): each item prints that its input happened (L-089), then its number; RED with -skygrindOff53
// (NIGHT52's look), GREEN on. A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). The row is forced by the build copy's
// config (node Tools/night/n46-build-rows.cjs <tag> on n53.polish53); a campaign level boots only via reel.sh + -skygrindMap (L-062).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN53 = { "a153" };

        static void N53ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a153") N53Polish(g, dir, res, offs, pre, ref body);
        }

        static partial void N53Polish(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
    }
}
`));
patch("Assets/Scripts/Game/FeelReel.N27.cs", [
  ["|| Array.IndexOf(PhaseN52, s) >= 0;", "|| Array.IndexOf(PhaseN52, s) >= 0 || Array.IndexOf(PhaseN53, s) >= 0;"],
  ["                    else if (Array.IndexOf(PhaseN52, s) >= 0) N52ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT52 U / K (FeelReel.N52.cs)\n",
   "                    else if (Array.IndexOf(PhaseN52, s) >= 0) N52ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT52 U / K (FeelReel.N52.cs)\n" +
   "                    else if (Array.IndexOf(PhaseN53, s) >= 0) N53ProofBody(s, g, dir, res, offs, pre, ref body); // NIGHT53 U (FeelReel.N53.cs)\n"],
]);

// 3. the shared tests
addNew("Assets/Tests/EditMode/N53SharedTests.cs", csEol(`// NIGHT53 (the shared commit; Tools/night/n53-shared.cjs): the night's one row as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n53.polish53 exists, has a _why, a bool enabled and a numbers object holding U's placeholders; off unless a freeze turned it on
//      (its enabled_why then starts with "ON"); no tape bit; ViewOn reads it; --off53 and --off52 turn it off
//   2. design.n53 holds no level row: the campaign order is the same with and without it (rooms are edited in place in the n52 levels)
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N53SharedTests
    {
        static readonly string[] Items = { ${items.map(([k]) => '"' + k + '"').join(", ")} };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void ThePolishRow_Exists_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n53._why"), Is.True, "design.n53._why");
            const string p = "design.n53.polish53.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "polish53._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "polish53.enabled is a bool");
            bool on = c.Design("n53.polish53.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "polish53.numbers is an object");
            foreach (string k in Items) Assert.That(c.Get(p + "numbers." + k) is Dictionary<string, object>, Is.True, "polish53.numbers." + k + " (U's placeholder)");
            Assert.That(NightRows.From(c).IndexOf("polish53"), Is.EqualTo(-1), "polish53 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "polish53"), Is.EqualTo(on), "ViewOn reads design.n53");
                foreach (int from in new[] { 53, 52 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "polish53"), Is.False, "--off" + from + ": polish53 off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N53_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n53"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": NIGHT53 edits the n52 levels in place (no n53 level ids)");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n53");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n53 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
`));

// 4. the rows and the queue
addNew("Tools/night/n53-rows.json", JSON.stringify({
  _why: "NIGHT53 row list (the lead's; written at P0 by Tools/night/n53-shared.cjs). One entry per row of NIGHT53.md section 1. Only the lead flips passes (Tools/night/n53-flip.cjs), only after printing the check's output in the transcript; `check` is never edited. `state`, `evidence_paths` and `note` are the close's own fields. Builders only report.",
  rows: [
    { id: "P0", title: "Debt + baselines + the shared commit", owner: "lead", switch: "-", check: "T-126 fixed at its source (Lesson25Tests restores every flag it sets; the batch pair L then N green); iceline g2 has frames52 and its two reviews; the identity checks (section 2) built, tested with a planted failure each, and printed for all 18 rooms (the baseline); design.n53 rows + n53-rows.json committed; gates green", fallback: "a check without its planted-failure test ships OFF", passes: false },
    { id: "K", title: "Rooms' second pass: kiln kb3, span p1, foundry a3 (then the queue)", owner: "builder K", switch: "design.n52.kiln52 / span52 / foundry52 (the n52 levels, edited in place)", check: "each changed room passes THE ROOM CONTRACT and the identity checks with numbers printed; verbs37 on its level unchanged or better; its frames looked at; a fresh review of the change", fallback: "the room reverted to u3-n52's (it shipped)", passes: false },
    { id: "L", title: "The loop: iceline g4, g3, then the queue (section 5)", owner: "lead", switch: "design.n52.iceline52 / spire52 (edited in place)", check: "same as K, per room; the levels' verbs37 unchanged", fallback: "the room reverted to u3-n52's", passes: false },
    { id: "U", title: "Polish: the SHORT items (section 4)", owner: "builder U", switch: "design.n53.polish53 (view row; -skygrindOff53)", check: "each item: a Player proof line RED once (off) and GREEN once (on) that prints its input happened (L-089); before / after frames looked at; nothing in verbs37 lost", fallback: "the item stays at NIGHT52's fallback", passes: false },
    { id: "R", title: "Reviews", owner: "lead", switch: "-", check: "every changed room gets a P-rank and a GG-persona review (fresh, read-only); every concrete finding fixed (the room re-passes) or answered in DECIDE53", fallback: "answered, not fixed", passes: false },
    { id: "Z", title: "Freeze and close", owner: "lead", switch: "-", check: "polish53 ON by its checks; Builds/n53-freeze; verbs37 x5 nothing lost; the flow table on the changed levels; the full batch suite green; the smoke; golden / fingerprints EQUAL; the report with before / after and the building share; tag u3-n53", fallback: "whatever is unfinished at 3:00 closes SHORT", passes: false },
  ],
}, null, 2) + "\n");
addNew("Tools/night/n53-queue.json", JSON.stringify({
  _why: "NIGHT53 the loop's queue (NIGHT53.md section 5, then section 8's leftovers). The lead takes the top item whose state is open and whose owner is lead; K's items are builder K's queue (kiln, span, foundry plans). A cycle marks its item done / reverted / short with the commit and the log line's time. Extend it, never run out.",
  items: [
    { id: "q1", owner: "lead", state: "open", what: "I2 / I3 computed: path lengths from the bot's nav grid (else DotRecast in a 45-minute box, else 'not computed'); their planted cases a3 (I2) and p1 (I3) FAIL" },
    { id: "q2", owner: "lead", state: "open", what: "iceline g2 THE SHIELD: frames52 spots + its two reviews (P-rank, GG persona); findings become queue items" },
    { id: "q3", owner: "lead", state: "open", what: "iceline g4: break the mirror (keep the east lock wall running north to the gallery as its way up; cut the west wall + run-up; the first drone off-axis west over the canal); the canal reads as a drop (pale violet-white lips, a glow from below)" },
    { id: "q4", owner: "lead", state: "open", what: "iceline g3: the hopper stops at ~6 m with a funnel top (a walkers' ramp to it, or a 0-hit perch); the chute opens west into the lane; wave 2's nodes at the west lane's north end" },
    { id: "q5", owner: "lead", state: "open", what: "I5 on iceline and spire: vary one ring-round-a-mass room per level (the centre mass a place to fight on, or off-centre)" },
    { id: "q6", owner: "lead", state: "open", what: "the flow table's crosses (T-124): iceline's clear time (one cause, from the per-room flow files)" },
    { id: "q7", owner: "lead", state: "open", what: "iceline g1 / g6, the links" },
    { id: "k1", owner: "K", state: "open", what: "kiln kb3: the west bank cut by a 4 m lava inlet (the ride is the west side's way north); the posts stop at the gantry top +1 m; the north crossing 2 m roomier (the mould line 2 m south)" },
    { id: "k2", owner: "K", state: "open", what: "span p1: the south ramp cut; walkers keep the back ramp; density re-earned with pieces that serve the hook" },
    { id: "k3", owner: "K", state: "open", what: "foundry a3: the far floor +1.5 m, its only walk-ups the shelf ramp and the crane stair (the ride is the flat way on)" },
    { id: "k4", owner: "K", state: "open", what: "I5 on kiln / span / foundry: vary one ring-round-a-mass room per level; foundry's door dwell (T-124); foundry a8 with Walk43Foundry.cs's targets moved in step (DECIDE52 D-26)" },
    { id: "L1", owner: "lead", state: "open", what: "T-125 a152's THUNDERCLAP that never fires on the reel's taps" },
    { id: "L2", owner: "lead", state: "open", what: "kiln's teach line that ended before 8 s in a252 (the queue rule?)" },
    { id: "L3", owner: "lead", state: "open", what: "T-121 spire's tiers" },
    { id: "L5", owner: "lead", state: "open", what: "THREADS.md consolidate (over its 60-line cap)" },
  ],
}, null, 2) + "\n");

if (dry) { for (const [p] of writes) console.log("would write " + p); console.log("DRY RUN: " + writes.length + " file(s), every anchor found once"); process.exit(0); }
for (const [p, text] of writes) fs.writeFileSync(R(p), text);
for (const [p] of writes) console.log("wrote " + p);
console.log("NIGHT53 shared commit done: " + writes.length + " file(s). Next: cmp the configs, golden, fingerprint plain / --off52 / --off53, the Editor compile, N53SharedTests + N52SharedTests.");

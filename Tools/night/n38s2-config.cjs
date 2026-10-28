// NIGHT38 builder S2 (NIGHT38-BRIEF section 4, card S2): writes builder S2's own rows in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json. L-049: text edits only (never a whole-file JSON.stringify); only S2's rows are touched; the file
// must still parse; idempotent.
//   1. design.n38.stag38: the row's text replaced (its `enabled` and any key the lead added are kept): THE STAG's arena numbers under the row.
//   2. design.n38.iceline38: the row's text replaced (`enabled`, levelFrom, levelTo kept unless --iceline-on / --iceline-off).
//   3. --level: design.maps.list gets an n38-iceline line right after n37-iceline's, and design.n31.unlocks.levels.rows an n38-iceline row right
//      after n37-iceline's (n 2, the gauntlet in the level's first room: mirrors it). Only once the level file exists.
// Flags: --stag-on / --stag-off set design.n38.stag38.enabled; --iceline-on / --iceline-off set design.n38.iceline38.enabled.
// Run from the repo (or worktree) root: node Tools/night/n38s2-config.cjs [--level] [--stag-on] [--iceline-on]
const fs = require("fs");
const path = require("path");
const ROOT = ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const argv = process.argv.slice(2);
const flag = (f) => argv.includes(f);

const G = "GG 2026-10-13 (Research/2026-10-13-wave38/GG-REQUEST.md: 'The middle walls are a little too short and clunky you can't really use them')";
const STAG = {
  _why: "Builder S2 (PLAN.md section 5; " + G + "): THE STAG's canal with tall, long lane walls you can ride (Core/Enemies/Stag37Arena.cs reads these over design.n37.stag37.arena while the row is on: "
    + "Stag37ArenaParams.From(cfg, stag38); Game/SurvivalSim.N38.Stag.cs rebuilds the canal at a run's start), the island's walls marked as ride walls (cyan trims), and the read pass "
    + "(Game/Stag37View.Read38.cs: an off-screen arrow to THE STAG, brighter lane warnings; render numbers in UnityTuning.Stag38.cs). THE STAG's fight logic (Stag37*) is unchanged. "
    + "Off (row off, --off38 / -skygrindOff38): NIGHT37's canal bit for bit.",
  laneWallHeight: 5,
  laneWallHeight_why: "PLAN.md section 5: lane walls 5 m tall. Mover's wall contact band is feet+0.35 .. feet+1.45 (Core/Mover.cs), so NIGHT37's 2.6 m wall held a ride only while the feet were under ~2.25 m (GG: 'too short and clunky'); "
    + "5 m holds the feet up to ~3.5 m, the bank wall's height (design.n37.stag37.arena.wallHeight 5): the lane walls and the banks read as one family of ride walls.",
  laneWallRuns: [10, 40, 50, 80],
  laneWallRuns_why: "PLAN.md section 5: runs of 30 m+ so a held ride lasts ~3 s (30 m at the held ride's floor speed 10.45 m/s, Design/levels/n35-ride-distances.json rideSpeed: 2.9 s; harness `iceline38 stag`). "
    + "[from, to, from, to] metres along straight B from its north end, as design.n37.stag37.arena.laneWallRuns: one 30 m run in the open, one 30 m run through the tunnel (tunnelFrom 50 .. tunnelTo 75, 5 m past each mouth); "
    + "the gaps (0-10, 40-50, 80-90) are where you change lanes. Measured (harness `iceline38 band`, THE STAG's bot band, 10 seeds): these runs p50 20.9 s / p90 22.8 s against NIGHT37's 20.9 / 24.8; "
    + "longer runs [6, 38, 46, 82] made it 24.5 / 28.4 and [6, 36, 50, 80] 23.2 / 24.7 (fewer places to follow its lane: past the 15-22 s band), so the runs stop at 30 m.",
  islandRide: true,
  islandRide_why: "PLAN.md section 5: 'ride walls on the island'. The island's faces were always solid walls the Mover could ride (it rides any vertical face); the row adds them to Stag37Arena.rideFaces so the view "
    + "draws their cyan trims (rule 8: cyan = player-usable) and the read says 'ride me': straight A's inside is 90 m of wall beside the inner lane.",
};

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }
function closeOf(t, open, o = "{", c = "}") {
  let depth = 0, inStr = false;
  for (let i = open; i < t.length; i++) {
    const ch = t[i];
    if (inStr) { if (ch === "\\") i++; else if (ch === "\"") inStr = false; continue; }
    if (ch === "\"") inStr = true;
    else if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  throw new Error("unbalanced");
}

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);

/// replace design.n38.<name>'s object text: `make(old)` returns the new object (key order kept as given)
function patchRow(name, make) {
  const n38 = t.indexOf("\"n38\": {");
  if (n38 < 0) throw new Error("no design.n38 block: merge port/stage3 first");
  const n38Close = closeOf(t, t.indexOf("{", n38));
  const key = t.indexOf(`"${name}": {`, n38);
  if (key < 0 || key > n38Close) throw new Error(`no ${name} row inside design.n38`);
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const row = make(old);
  for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v; // a key the lead added stays
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}

// 1. stag38
patchRow("stag38", (old) => {
  const on = flag("--stag-on") ? true : flag("--stag-off") ? false : old.enabled === true;
  return Object.assign({ _why: STAG._why, enabled: on }, Object.fromEntries(Object.entries(STAG).filter(([k]) => k !== "_why")));
});

// 2. iceline38
const ICE_WHY = "Builder S2 (PLAN.md section 2 L2; " + "GG 2026-10-13: 'I want overhauls on all the levels'; 'wide open arenas with a roof'; 'they can't be too exploitable'): level 2 THE ICE LINE for flight: "
  + "Tools/night/gen-n37-iceline-boss.cjs --n38 writes n38-iceline from its beat sheet (Design/levels/n38/iceline.beats.md): NIGHT37's copied rooms i1a..i7 replaced by new rooms (the GAUNTLET, skating, ride chains; "
  + "open roofed arenas with enemies on the line; one flight room of skate-lane + ride gates), the rehearsal rooms s1-s3 and THE CANAL s4 kept as NIGHT37 built them. "
  + "Flow only: a levelFrom -> levelTo swap in Curriculum25.N38Levels while on (the level file must exist first).";
patchRow("iceline38", (old) => ({
  _why: ICE_WHY,
  enabled: flag("--iceline-on") ? true : flag("--iceline-off") ? false : old.enabled === true,
  levelFrom: old.levelFrom || "n37-iceline",
  levelTo: old.levelTo || "n38-iceline",
}));

// 3. the level's lines (only with --level, once the file exists)
if (flag("--level")) {
  if (!fs.existsSync(path.join(ROOT, "Design/levels/n38-iceline.json"))) throw new Error("--level: Design/levels/n38-iceline.json does not exist yet");
  if (!t.includes("{ \"id\": \"n38-iceline\"")) {
    const a = "{ \"id\": \"n37-iceline\", \"label\": \"LEVEL 2 THE ICE LINE\", \"file\": \"Design/levels/n37-iceline.json\", \"campaignOnly\": true },";
    const at = t.indexOf(a);
    if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n37-iceline's line not found exactly once");
    const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
    const line = "{ \"id\": \"n38-iceline\", \"label\": \"LEVEL 2 THE ICE LINE\", \"file\": \"Design/levels/n38-iceline.json\", \"campaignOnly\": true },";
    t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
  }
  if (!/"id": "n38-iceline",\s*"earns"/.test(t)) {
    const idAt = t.search(/"n": 2,\s*"id": "n37-iceline",\s*"earns"/);
    if (idAt < 0) throw new Error("unlocks: n37-iceline's row not found");
    const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
    const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
    const inner = objIndent + "  ";
    const row = ["{", `${inner}"n": 2,`, `${inner}"id": "n38-iceline",`, `${inner}"earns": "gauntlet",`, `${inner}"room": "j1",`,
      `${inner}"note": "LEVEL 2 THE ICE LINE for flight (NIGHT38 S2, design.n38.iceline38 swaps n37-iceline for it): new rooms j1..j4 from Design/levels/n38/iceline.beats.md, then NIGHT37's rehearsal rooms and THE CANAL. The PULSE gauntlet in j1."`,
      `${objIndent}}`].join(eol);
    t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
  }
}

JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("stag38:", JSON.stringify(Object.keys(c.design.n38.stag38)), "enabled", c.design.n38.stag38.enabled);
console.log("iceline38: enabled", c.design.n38.iceline38.enabled, c.design.n38.iceline38.levelFrom, "->", c.design.n38.iceline38.levelTo);
console.log("maps.list n38-iceline:", c.design.maps.list.some((m) => m.id === "n38-iceline"), "; unlocks row n38-iceline:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n38-iceline"));
console.log("copied to", SA);

// NIGHT37 builder S (NIGHT37.md S1): writes builder S's own rows in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json. L-049: text edits only (never a whole-file JSON.stringify); the file must still parse; idempotent.
//   1. design.n37.stag37: the row's text replaced (its `enabled`, levelFrom, levelTo and any key the lead added are kept): THE STAG's numbers;
//   2. design.maps.list: an n37-iceline line right after n35-iceline's (mirrors it);
//   3. design.n31.unlocks.levels.rows: an n37-iceline row right after n35-iceline's (n 2, the gauntlet in i1a: mirrors it).
// Run from the repo (or worktree) root: node Tools/night/n37s-config.cjs [root]
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const B = "BOSSES.md 'L2 THE STAG'";

const ROW = {
  _why: "Builder S (NIGHT37.md S1; " + B + "): THE STAG from scratch as a chase around a frozen canal loop (Core/Enemies/Stag37.cs on K's Boss37 brain, Core/Enemies/Stag37Arena.cs, Game/SurvivalSim.Stag37.cs): "
    + "the slipstream, lane changes, catching it to punch its hind legs (the stagger), the CYAN skid and the launch punch into the canal wall (one per leg), the floes in leg 2, the head-on antler charge parried into the tunnel wall in leg 3. "
    + "With the row on, Curriculum25.N37Levels swaps levelFrom for levelTo (Tools/night/gen-n37-iceline-boss.cjs: n35-iceline's rooms up to its rest, two rehearsal rooms, THE CANAL). Level 6's old Stag (plan id glacier) is untouched. "
    + "Off (row off, --off37 / -skygrindOff37): the campaign plays n35-iceline and nothing here runs.",
  name: "THE STAG",
  name_why: "DECIDE37 D-3: THE STAG keeps its name with a new fight.",
  arena: {
    _why: "THE CANAL (" + B + ": 'a canal loop, ~260 m, three lanes 5 m wide each, with banked turns'): built at load by Stag37Arena at the room row's stag37.centre.",
    straight: 90, straight_why: B + ": straight A (90 m) and straight B (90 m).",
    turnR: 12.75, turnR_why: "The turns' centre-line radius, picked so the loop is 2 x 90 + 2 pi 12.75 = 260.1 m (" + B + ": ~260 m).",
    laneWidth: 5, laneWidth_why: B + ": three lanes 5 m wide each.",
    lanes: 3, lanes_why: B + ": three skate lanes.",
    wallHeight: 5, wallHeight_why: "The outer (bank) walls: taller than a jump and the 3.5 m ride panels of the n36 rooms (Tools/night/n36-lib.cjs panels 6-7 m tall), so a rider never tops out.",
    islandHeight: 6, islandHeight_why: "The loop's inside: taller than any jump + wall-ride climb (wallgrip36 never lifts you), so nobody cuts the loop.",
    turnFacets: 24, turnFacets_why: "Facets per half-circle turn wall (7.5 deg, 2.7 m each at the bank). Lantern36's 64-facet ring (5.6 deg) rode cleanly (BOSSES.md engine audit: curved walls work); 24 keeps the layer small (BOSSES.md 'keep each moving layer small').",
    gapFacets: 2, gapFacets_why: "The entry gap at the south apex: 2 facets = 5.3 m (the 4 m door of the level's rooms fits).",
    gapHeight: 4.5, gapHeight_why: "The gap's height = the levels' door height (n36-lib.cjs: doors at most 4.5 m).",
    tunnelRoofY: 4.5, tunnelRoofY_why: "The tunnel's roof: over the lane walls (2.6 m), under the bank wall's top (5 m).",
    tunnelFrom: 50, tunnelFrom_why: "Metres along straight B (from its north end) where the tunnel starts (" + B + ": 'straight B: lane walls, a tunnel'); leg 3 turns the Stag at its south mouth.",
    tunnelTo: 75, tunnelTo_why: "Where the tunnel ends: 25 m of roof, the leg-3 charge's lane.",
    laneWallHeight: 2.6, laneWallHeight_why: "Lane walls on straight B: ride panels (a 2.6 m face is above a standing player's 1.8 m capsule, so the Mover rides it) with gaps to change lanes.",
    laneWallThick: 0.4, laneWallThick_why: "Thin dividers (a lane stays 4.6 m of ice).",
    laneWallRuns: [10, 30, 50, 75], laneWallRuns_why: "The lane-wall runs along straight B [from, to, from, to] (metres from its north end): one in the open, one through the tunnel; the gaps (0-10, 30-50, 75-90) are where you change lanes.",
    floeLength: 15, floeLength_why: "Straight B's ice in 15 m floes per lane (6 per lane): a floe dropped in your lane is too long to jump, so you change lanes (" + B + ": 'per-lane drops').",
    floeThick: 0.5, floeThick_why: "A floe's slab thickness (visual and collision).",
    floeGap: 0.6, floeGap_why: "The cracks between floes along B (read as floes, small enough not to trip the Mover).",
    waterY: -2, waterY_why: "The lane walls reach down to the water (visual); a fall below fallY in the canal restarts the leg (D-6).",
  },
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

// the boss's own numbers: a separate file so the arena and the fight can be patched apart
const extra = path.join(__dirname, "n37s-config-fight.cjs");
if (fs.existsSync(extra)) Object.assign(ROW, require(extra)(B));

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);

// 1. the stag37 row
{
  const n37 = t.indexOf("\"n37\": {");
  if (n37 < 0) throw new Error("no design.n37 block: merge port/stage3 first");
  const n37Close = closeOf(t, t.indexOf("{", n37));
  const key = t.indexOf("\"stag37\": {", n37);
  if (key < 0 || key > n37Close) throw new Error("no stag37 row inside design.n37");
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const row = { _why: ROW._why, enabled: old.enabled === true, levelFrom: old.levelFrom, levelTo: old.levelTo };
  for (const [k, v] of Object.entries(ROW)) if (k !== "_why") row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}

// 2. design.maps.list
if (!t.includes("{ \"id\": \"n37-iceline\"")) {
  const a = "{ \"id\": \"n35-iceline\", \"label\": \"LEVEL 2 THE ICE LINE\", \"file\": \"Design/levels/n35-iceline.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n35-iceline's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n37-iceline\", \"label\": \"LEVEL 2 THE ICE LINE\", \"file\": \"Design/levels/n37-iceline.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}

// 3. design.n31.unlocks.levels.rows
if (!/"id": "n37-iceline",\s*"earns"/.test(t)) {
  const idAt = t.search(/"n": 2,\s*"id": "n35-iceline",\s*"earns"/);
  if (idAt < 0) throw new Error("unlocks: n35-iceline's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 2,`, `${inner}"id": "n37-iceline",`, `${inner}"earns": "gauntlet",`, `${inner}"room": "i1a",`,
    `${inner}"note": "LEVEL 2 THE ICE LINE with THE STAG from scratch (NIGHT37 S, design.n37.stag37 swaps n35-iceline for it): n35-iceline's rooms up to its rest, two rehearsal rooms (lanes, slipstream) and THE CANAL. The PULSE gauntlet in i1a."`,
    `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}

JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("stag37:", JSON.stringify(Object.keys(c.design.n37.stag37)), "enabled", c.design.n37.stag37.enabled);
console.log("maps.list n37-iceline:", c.design.maps.list.some((m) => m.id === "n37-iceline"));
console.log("unlocks row n37-iceline:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n37-iceline"));
console.log("copied to", SA);

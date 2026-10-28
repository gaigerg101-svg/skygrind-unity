// NIGHT38 builder C2 (spire38 + crown38): writes ONLY builder C2's rows in Design/unity-config.json (text edits, the file's layout kept), then copies
// the file to Assets/StreamingAssets/Design/unity-config.json:
//   1. design.n38.spire38: its numbers (each with a _why), keeping `enabled`, `levelFrom`, `levelTo` and any key the lead added;
//   2. design.n38.crown38 (a view row, no bit): THE CROWN's look pass numbers (Game/Crown37View*), added after spire38 when absent, else rewritten
//      keeping its `enabled`;
//   3. design.maps.list: an n38-spire line right after n37-spire's;
//   4. design.n31.unlocks.levels.rows: an n38-spire row right after n37-spire's (n 5, earns the TORCH).
// Run: node Tools/night/n38c-config.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const SPIRE = {
  _why: "Builder C2 (PLAN.md section 2 L5; NIGHT38.md L5): level 5's copied rooms v1-v8 replaced (Tools/night/gen-n38-spire.cjs, beat sheet Design/levels/n38/spire.beats.md): "
    + "THE LANDING (the TORCH), two open roofed arenas (THE NAVE, THE CHOIR), four gate rooms (BELFRY I-III, THE LONG BELL: one far jump-dash gate each under a ceiling) "
    + "with a GATE METER on each pad, THE VESTRY; v9-v11 kept. While on: the levelFrom -> levelTo swap (Curriculum25.N38Levels) and the gate meter "
    + "(Game/SurvivalSim.N38.Spire.cs, only in rooms with flight38 / meter38 rows). Off: n37-spire, no meter.",
  meter: {
    _why: "GG 2026-10-13: 'a far dash jump ... they can't be too exploitable'; PLAN.md: 'a limited dash meter, so the dash-jump can't be spammed gap to gap'. ULTRAKILL: a dash-jump costs 2 of 3 stamina (research/arena-web.md).",
    perGate: 1,
    perGate_why: "jump-dashes a pad's meter holds when its row names none: the bar is SET to perGate x one jump-dash's cost (design.stamina.boost.cost 0.5 + dashJumpExtra 0.25 = 0.75 of the 1.0 bar): one jump-dash and nothing left to air-dash with. Ours.",
    padReach: 0.3,
    padReach_why: "m: the feet count as on a pad within this of its height (the Mover's grounded feet sit within its skin of the top). Ours.",
    holdRefill: true,
    holdRefill_why: "in a flight38 room the bar never refills by itself (design.stamina.refill's 0.5 / s would give a second dash 0.4 s after a spend): only a pad fills it, so each gap gets one jump-dash.",
    slack: 0.05,
    slack_why: "bar over the jump-dash's cost a pad holds: a sprint drains design.stamina.sprint.drain 0.4 / s on the takeoff step before the dash, and a dash-jump needs the whole dashJumpExtra left after the boost (harness spire38 gates: without it the jump inside the dash fell back to a plain jump). Under the boost's floor (0.25 x 0.5 = 0.125): no second dash from it.",
    oneDash: true,
    oneDash_why: "in a flight38 room, once the first dash since the pad has ended (its jump-dash or its run-out) the meter is spent: the bar to 0 and the double jump gone until a pad or the ground outside. Measured (harness spire38 gates, under the 4.0 m ceiling): a jump + a late dash + the double jump crossed 18 m by 0.47 m, and with the bar's 0.25 left a second half dash + the double crossed by 3 m: without this rule no gap is a jump-dash gate.",
    noHook: true,
    noHook_why: "in a flight38 room the grapple is off (Mover.ropeEnabled false; it comes back at the room's edge): design.n35.grapple35 pulls to any surface within 40 m (design.n32.grapple32.range), so it would cross every gate (builder E's GATES38.md).",
  },
  gate: {
    _why: "the gate rooms' shape (Tools/night/gen-n38-spire.cjs reads these; every gate goes through builder E's checker `n38 gates` and builder C2's `spire38 gates`).",
    gapTeach: 17,
    gapTeach_why: "m, BELFRY I's gap (the first gate): the jump-dash (dash, then jump inside the dash) flies 21 m on the flat (Harness crown37 jumps); a whole metre (the nav grid's cells sit on half metres).",
    gap: 18,
    gap_why: "m, every other gate: D-6 wants >= 10 % margin on the jump-dash (21 m: at most 18.9 m) and every line without it short (builder E's checker under the ceiling: best 16.53 m, jump-dash + double).",
    ceilingOverTakeoff: 4.0,
    ceilingOverTakeoff_why: "m: the ceiling's underside over the takeoff: the jump-dash's apex 1.85 m (Harness crown37 jumps) + the 1.8 m capsule (collision.capsuleHeight) + 0.35 m; it caps the double jump (apex 2.87 m in the open, envelopes38.json) at 2.2 m.",
    pitDepth: 5,
    pitDepth_why: "m: the dry pit under each gap: three 1.25 m recovery steps (under jump.height 1.5) back to the pad; a miss costs a climb, never health (as n37-spire's v9 / v10).",
    pad: 10,
    pad_why: "m of pad before the edge (the room's spawn and the meter): a sprint run-up (builder E's checker runs 8 m).",
    landing: 7,
    landing_why: "m of landing floor past the gap to the door: the jump-dash overshoots an 18 m gap by about 3 m.",
  },
};

const CROWN = {
  _why: "View row (builder C2, NIGHT38.md L5 'crown38: the look pass'; GG 2026-10-13: 'polish the final boss' / 'make it look a little bit better'): THE CROWN's look, every number here (Game/Crown37View*.cs reads them while on; "
    + "render-only: no sim change, no bit). Off (or --off38): NIGHT37's look. Tune from the lead's n37-r7 / n38 frames.",
  enabled: false,
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
const crownLook = (() => { try { return require("./n38c-crown-look.cjs"); } catch (e) { return null; } })();
if (crownLook) Object.assign(CROWN, crownLook);

function rowAt(name) {
  const n38 = t.indexOf("\"n38\": {");
  if (n38 < 0) throw new Error("no design.n38 block: merge port/stage3 first");
  const n38Close = closeOf(t, t.indexOf("{", n38));
  const key = t.indexOf(`"${name}": {`, n38);
  if (key < 0 || key > n38Close) return null;
  const open = t.indexOf("{", key);
  return { key, open, close: closeOf(t, open) };
}
function writeRow(name, ROW, keep) {
  const at = rowAt(name);
  const old = JSON.parse(t.slice(at.open, at.close + 1));
  const indent = t.slice(t.lastIndexOf("\n", at.key) + 1, at.key);
  const row = { _why: ROW._why };
  for (const k of keep) if (k in old) row[k] = old[k];
  for (const [k, v] of Object.entries(ROW)) if (!(k in row)) row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row) && !(k in ROW)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, at.open) + body + t.slice(at.close + 1);
}
writeRow("spire38", SPIRE, ["enabled", "levelFrom", "levelTo"]);
if (!rowAt("crown38")) {
  const at = rowAt("spire38");
  const indent = t.slice(t.lastIndexOf("\n", at.key) + 1, at.key);
  t = t.slice(0, at.close + 1) + "," + eol + indent + "\"crown38\": {}" + t.slice(at.close + 1);
}
writeRow("crown38", CROWN, ["enabled"]);

if (!t.includes("{ \"id\": \"n38-spire\"")) {
  const a = "{ \"id\": \"n37-spire\", \"label\": \"LEVEL 5 THE SPIRE\", \"file\": \"Design/levels/n37-spire.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n37-spire's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n38-spire\", \"label\": \"LEVEL 5 THE SPIRE\", \"file\": \"Design/levels/n38-spire.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}
if (!/"id": "n38-spire",\s*"earns"/.test(t)) {
  const idAt = t.search(/"n": 5,\s*"id": "n37-spire",\s*"earns"/);
  if (idAt < 0) throw new Error("unlocks: n37-spire's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 5,`, `${inner}"id": "n38-spire",`, `${inner}"earns": "torch",`,
    `${inner}"note": "LEVEL 5 THE SPIRE rebuilt for flight (NIGHT38 C2, design.n38.spire38 swaps n37-spire for it): THE LANDING hands over the TORCH (a weaponPickup), two open arenas, four jump-dash gate rooms with a gate meter, then FALLING STONES, THE RING and THE SPIRE as NIGHT37. Earns the TORCH (the pickup gives it first)."`,
    `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}
JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("spire38 keys:", Object.keys(c.design.n38.spire38).filter((k) => !k.endsWith("_why")).join(" "), "enabled", c.design.n38.spire38.enabled);
console.log("crown38 keys:", Object.keys(c.design.n38.crown38).filter((k) => !k.endsWith("_why")).join(" "), "enabled", c.design.n38.crown38.enabled);
console.log("maps.list n38-spire:", c.design.maps.list.some((m) => m.id === "n38-spire"));
console.log("unlocks row n38-spire:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n38-spire"));

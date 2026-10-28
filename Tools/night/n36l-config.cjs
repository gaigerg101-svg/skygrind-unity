// NIGHT36 builder L (NIGHT36.md L1): writes builder L's own rows in Design/unity-config.json and copies the file to Assets/StreamingAssets/Design/unity-config.json.
// L-049: text edits only (never a whole-file JSON.stringify), the file must still parse; idempotent. Three edits:
//   1. design.n36.level36: the row's text replaced (its `enabled` and any key the lead added are kept): levelSwap + the arena flow's numbers;
//   2. design.maps.list: an n36-foundry line right after n35-foundry's (the lead: "mirror n35-foundry's entries");
//   3. design.n31.unlocks.levels.rows: an n36-foundry row right after n35-foundry's (n 1: the KESTREL only).
// Run from the repo (or worktree) root: node Tools/night/n36l-config.cjs [root]
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const ROW = {
  _why: "Flow (builder L, NIGHT36.md L1, PLAN.md section 3): the campaign's level 1 becomes the from-scratch n36-foundry (levelSwap, applied after N35Levels by Curriculum25.N36Levels); levels 2-6 unchanged tonight (D-2 depth first). "
    + "The level file (Tools/night/gen-n36-foundry.cjs, bash Tools/night/n36-regen.sh) marks its fight rooms arena36: true; there Game/SurvivalSim.Level36.cs runs ULTRAKILL's arena flow (the numbers below); "
    + "older levels carry no arena36 room and are untouched. Off (row off, --off36 / -skygrindOff36): the campaign plays n35-foundry and no arena flow runs.",
  levelSwap: { "n35-foundry": "n36-foundry" },
  switchOnDelayS: 1.0,
  switchOnDelayS_why: "ULTRAKILL: enemies start appearing 1 s after the doors lock (ActivateArena.cs:107-110, 126-151; Research/2026-10-12-wave36/research/rea-ultrakill-combat.md section 4). A wave's first enemy switches on this long after the room's entry.",
  staggerS: 0.1,
  staggerS_why: "ULTRAKILL: then one more enemy every 0.1 s (ActivateArena.cs:126-151; rea-ultrakill-combat.md section 4).",
  nextWaveS: 1.0,
  nextWaveS_why: "ULTRAKILL: the next wave starts 1 s after the last enemy of a wave dies (ActivateNextWave.cs:31-91). Measured from the death: the campaign's own waveGap (design.n24.campaign.waveGap) runs first, the rest of this is added to the switch-on.",
  bubbleS: 0.5,
  bubbleS_why: "ULTRAKILL's spawn bubble shrinks at 2 per second (SpawnEffect.cs; rea-ultrakill-combat.md section 4): a unit bubble is gone in 0.5 s. The view's bubble and light last this long (the event arena36SpawnIn carries it).",
  lastKillSlowS: 0.15,
  lastKillSlowS_why: "ULTRAKILL: after the last wave a 0.15 slow-down, then the doors unlock (ActivateNextWave.cs:68-142; rea-ultrakill-combat.md section 4). Carried by the event arena36LastKill for the Game's time layer.",
  lastKillSlowScale: 0.25,
  lastKillSlowScale_why: "The world's speed during that slow-down. ULTRAKILL's TimeController scale for it was not read (not checked): 0.25 is ours (a third of design.n27.slowAim27.scale 0.75, so it reads as a different, sharper beat), to tune on Player frames.",
  unlockStaggerS: 0.1,
  unlockStaggerS_why: "ULTRAKILL: the doors unlock one every 0.1 s (ActivateNextWave.cs:68-142). The view's door lights turn back in this order (event arena36Unlock).",
  launchReachM: 4,
  launchReachM_why: "Tools/night/n36-jobs.cjs: a lava pool must have an enemy spot or route within this many metres (lava an enemy can never be launched into is decoration). PLAN.md section 1 (heal36's ~4 m close-kill radius, the punch's reach plus a launch's carry); builder P's design.n36.launch36.reachM wins when it exists.",
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

// 1. the level36 row
{
  const n36 = t.indexOf("\"n36\": {");
  if (n36 < 0) throw new Error("no design.n36 block: merge port/stage3 first");
  const n36Close = closeOf(t, t.indexOf("{", n36));
  const key = t.indexOf("\"level36\": {", n36);
  if (key < 0 || key > n36Close) throw new Error("no level36 row inside design.n36");
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const row = { _why: ROW._why, enabled: old.enabled === true };
  for (const [k, v] of Object.entries(ROW)) if (k !== "_why") row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}

// 2. design.maps.list
if (!t.includes("{ \"id\": \"n36-foundry\"")) {
  const a = "{ \"id\": \"n35-foundry\", \"label\": \"LEVEL 1 THE FOUNDRY\", \"file\": \"Design/levels/n35-foundry.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n35-foundry's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n36-foundry\", \"label\": \"LEVEL 1 THE FOUNDRY\", \"file\": \"Design/levels/n36-foundry.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}

// 3. design.n31.unlocks.levels.rows
if (!/"id": "n36-foundry",\s*"note"/.test(t)) {
  const idAt = t.search(/"n": 1,\s*"id": "n35-foundry",\s*"note"/);
  if (idAt < 0) throw new Error("unlocks: n35-foundry's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 1,`, `${inner}"id": "n36-foundry",`, `${inner}"note": "LEVEL 1 THE FOUNDRY from scratch (NIGHT36 L, design.n36.level36 swaps n35-foundry for it): six rooms (calm intake, KESTREL, RIDE, PARRY, CRUCIBLE, rest) and THE LANTERN's ring. The KESTREL only."`, `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}

JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("level36:", JSON.stringify(Object.keys(c.design.n36.level36)), "enabled", c.design.n36.level36.enabled);
console.log("maps.list n36-foundry:", c.design.maps.list.some((m) => m.id === "n36-foundry"));
console.log("unlocks row n36-foundry:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n36-foundry"));
console.log("copied to", SA);

// NIGHT36 builder M: writes builder M's rows design.n36.wallgrip36 (and text36's _why) into Design/unity-config.json (this repo or worktree: the
// path is relative to this script). L-049: only the text of each row's object inside `"n36": {` is replaced (brace matched, strings skipped); every
// other byte of the file is kept. Re-runnable. The row's `enabled` is kept as the file has it (the lead turns rows on).
// Run: node Tools/night/n36m-config.cjs   then: cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && cmp both
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {};

rows.wallgrip36 = {
  _why:
    "Builder M (NIGHT36.md M1; REVIEW.md root cause 4: a rising entry keeps its rise and climbs ~7 m at 0.15 gravity, Mover.cs StartRide's 'rising entries keep rising', Flow34.cs RideGravity): on attach the upward speed is clamped to a small kick, the arrival speed along the wall is kept, a grip near level then a slow sink (ULTRAKILL: 'There is no upward motion from touching a wall', rea-ultrakill-combat.md section 5, NewMovement.cs Cling :820-858). Core/Grip36.cs; Mover.cs branches marked NIGHT36 wallgrip36, inside the NIGHT35 wallride35 branches only (needs wallride35 on). Off: wallride35's ride bit for bit.",
  enabled: false,
  attachUpMax: 1.5,
  attachUpMax_why:
    "m/s: the most upward speed a ride keeps at the attach (NIGHT36.md M1 and PLAN.md F5's start value; ULTRAKILL keeps none: its cling applies only while already falling faster than 1 m/s, rea-ultrakill-combat.md section 5). A jump's 8.33 m/s (jump.height 1.5 / apexTime 0.36) at the wall kept its rise before; at 1.5 m/s braked at full gravity (23.1 m/s2) it rises 0.05 m.",
  riseGravityMult: 1,
  riseGravityMult_why:
    "The gravity multiplier while the kept kick still rises: full gravity, so the ride never climbs (the web's ride climbs at wallRide.gravityMult 0.15: 7 m from a 7 m/s entry; wallride35's hold ride already braked at 1 = its riseGravityMult). Same value as design.n35.wallride35.riseGravityMult, now for every ride.",
  gripTime: 0.35,
  gripTime_why:
    "Seconds from the attach that the ride holds its height (no gravity, no sink): design.n34.flow34.ridePhase.gripTime (0.35 s, 42 steps: the Titanfall-style recreation's 0.5 s grip scaled to SKYGRIND's 1.2 s ride), the grip GG's ride already had for a falling entry.",
  sinkAccel: 1.5,
  sinkAccel_why:
    "m/s per s: a timed ride (jump not held: the web's 1.2 s ride) sinks after the grip, its sink speed growing by this much each second (ULTRAKILL's cling slide grows ~8 m/s per s, rea-ultrakill-combat.md section 5: ours is the card's 'slow sink'). 1.5 gives about 0.5 m over the rest of a 1.2 s ride, the drop ridePhase's ramp gave a falling entry before (0 g for 0.35 s, then 0.15 g ramped in over 0.7 s: ~0.5 m). A HOLD ride keeps design.n35.wallride35.sinkSpeed (0.1 m/s) after the grip.",
  sinkMax: 1.5,
  sinkMax_why: "m/s: the timed ride's sink never passes this (reached 1 s after the grip, past the ride's 1.2 s end: a cap for bonusRideTime's longer rides). Ours.",
};

rows.text36 = {
  _why:
    "View only (builder M; REVIEW.md root cause 10: every teaching line sits at 24 px in the top slot, 54 px from the top; GG 2026-10-12: 'the tutorial text is way too high up'): the teaching lines (teach35), the lessons and a boss's teaching line move to their own slot in the lower middle (about 62 % down the screen), 32 px at 1080p on a soft plate, apart from the room name (which keeps the top slot). UI numbers in UnityTuning.Move36.cs (Hud36Teach*). Off: NIGHT35's top slot.",
  enabled: false,
};

/** The text span [start, end) of the JSON object that starts at the `{` at `open` (strings skipped). */
function objectEnd(text, open) {
  let depth = 0;
  let inStr = false;
  for (let i = open; i < text.length; i++) {
    const ch = text[i];
    if (inStr) {
      if (ch === "\\") i++;
      else if (ch === '"') inStr = false;
      continue;
    }
    if (ch === '"') inStr = true;
    else if (ch === "{") depth++;
    else if (ch === "}") {
      depth--;
      if (depth === 0) return i + 1;
    }
  }
  throw new Error("unbalanced object at " + open);
}

function serialize(obj, indent) {
  const s = JSON.stringify(obj, null, 2);
  return s.split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
}

const n36At = raw.indexOf('"n36": {');
if (n36At < 0) throw new Error("design.n36 not found (the shared commit adds it)");
const n36Open = raw.indexOf("{", n36At);
let n36End = objectEnd(raw, n36Open);
for (const name of Object.keys(rows)) {
  const key = '"' + name + '": {';
  const at = raw.indexOf(key, n36Open);
  if (at < 0 || at > n36End) throw new Error("design.n36." + name + " not found inside design.n36 (the shared commit adds the rows)");
  const lineStart = raw.lastIndexOf("\n", at) + 1;
  const indent = raw.slice(lineStart, at);
  const open = raw.indexOf("{", at);
  const end = objectEnd(raw, open);
  const cur = JSON.parse(raw.slice(open, end));
  if (typeof cur.enabled === "boolean") rows[name].enabled = cur.enabled; // the row's switch is the lead's
  raw = raw.slice(0, open) + serialize(rows[name], indent) + raw.slice(end);
  n36End = objectEnd(raw, n36Open);
  console.log("design.n36." + name + " written");
}
JSON.parse(raw); // still valid JSON
fs.writeFileSync(file, raw);

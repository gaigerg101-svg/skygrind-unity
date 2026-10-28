// NIGHT35 builder M: writes builder M's rows design.n35.wallride35 (and, once written, design.n35.grapple35) into Design/unity-config.json (this repo
// or worktree: the path is relative to this script). L-049: only the text of each row's object inside `"n35": {` is replaced (brace matched, strings
// skipped); every other byte of the file is kept. Re-runnable. A row that is not in the file yet is an error (the shared commit adds the rows),
// The row's `enabled` is kept as the file has it (the lead turns rows on), unless --local-insert (scratch testing before the shared commit: a whole design.n35 block is appended; never committed).
// Run: node Tools/night/n35m-config.cjs   then: cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && cmp both
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const localInsert = process.argv.includes("--local-insert");
const only = process.argv.find((a) => a.startsWith("--only="));

const rows = {};

rows.wallride35 = {
  _why:
    "Builder M (NIGHT35.md M1, DECIDE35 D-6: the new ride on, the old ride is this row off): the Lucio wall ride (Research/2026-10-11-wave35/research/lucio.md, its spec 1-10; Core/Ride35.cs, Mover.cs branches marked NIGHT35 wallride35; with the row off every golden vector and trajectory is bit-exact). A ride entered with jump HELD rides while it stays held (no 1.2 s timer), the release is the jump-off (up speed from the look pitch, an exit boost), crouch drops off; corners and pillars are followed; a re-ride rule; a late jump. A ride entered without jump held is the web's timed ride. The web's ride: wallRide.maxTime 1.2 s, entrySpeed 4 m/s along the wall, press = wall jump, crouch = slam.",
  enabled: false,
  enabled_why: "Kept as the file has it (the lead turns rows on; D-6's default is on).",
  entrySpeed: 1,
  entrySpeed_why:
    "LOGGED (the entry gate lowered, NIGHT35.md M1): a HOLD ride starts at 1 m/s along the wall (the web's wallRide.entrySpeed 4), and a head-on hold entry rides the way the camera faces at the approach speed. Lucio has no documented minimum speed (lucio.md spec 9: 'not found'; 'jump at a nearby wall and hold the Jump key', overwatch.fandom.com/wiki/Wall_Ride). A ride without jump held keeps the web's 4.",
  speedBuff: 0.3,
  speedBuff_why:
    "The ride's speed floor is wallRide.minSpeed (8) x (1 + 0.3) = 10.4 m/s on any ride: Lucio's 'speed buff reduced from 40% to 30%' (late 2019, west-games.com/overwatch-lucio-changes) and the wiki's '+30% movement speed buff' (lucio.md section 1). The web's floor is 8 m/s.",
  exitBoost: 2,
  exitBoost_why:
    "Metres per second added to the horizontal speed at a jump-off (never past chainCap), so a chain of rides builds speed: Lucio's 'burst of speed when leaping off a wall' (April 2017) raised 65 % in Oct 2017 (overwatch.blizzard.com patch notes 2017/10; lucio.md section 1, spec 3). Blizzard published no magnitude: 2 m/s is ours (about a fifth of the ride's 10.4 m/s floor), to be tuned on Player frames.",
  chainCap: 20,
  chainCap_why:
    "The exit boost never lifts the horizontal speed past 20 m/s (a faster entry keeps its own speed): ours, under the slide jump's cap (slide.jumpBoostMaxSpeed 24) so a slide-hop stays the fastest takeoff; Lucio's air speed thresholds (11 / 13.76 / 17.6 m/s, the wiki, lucio.md section 1) are of the same order.",
  outSpeed: 8,
  outSpeed_why: "The jump-off's speed straight out from the wall: the web's wallJump.outSpeed (8), unchanged.",
  upLow: 4.12,
  upLow_why: "The jump-off's up speed looking down (at pitchLowDeg): the wiki's 4.12 m/s (overwatch.fandom.com/wiki/Lucio, lucio.md section 1, spec 4; not verified against a datamine).",
  upHigh: 9.12,
  upHigh_why: "The jump-off's up speed looking up (at pitchHighDeg): the wiki's 9.12 m/s (same source). The web's wall jump: jump v0 8.33 x wallJump.v0Mult 0.9 = 7.5 m/s whatever the pitch.",
  pitchLowDeg: -45,
  pitchLowDeg_why: "The look pitch (degrees, + is up) that gives upLow; between the two the up speed is linear, clamped outside. Ours: the wiki says only 'looking down' / 'looking up'.",
  pitchHighDeg: 45,
  pitchHighDeg_why: "The look pitch that gives upHigh (ours, as pitchLowDeg). Level look (0): 6.62 m/s up.",
  riseGravityMult: 1,
  riseGravityMult_why:
    "A HOLD ride entered rising brakes at full gravity until it stops rising (a 7 m/s rising entry lifts about 1 m more), so a hold ride is a level line at about the height it met the wall. The web's ride climbs at wallRide.gravityMult 0.15 (7 m from a 7 m/s entry): round 2, a hold ride met a 4 m wall rising and rode off its top in 0.7-1.2 s (verbs35 row 9 on n35-kiln). A ride WITHOUT jump held keeps the web's 0.15 climb, and a flue is climbed by the release's jump-off looking up (up to upHigh 9.12 m/s; Walk29Pilot's flue script does). Lucio's on-wall gravity was not found (lucio.md spec 8).",
  sinkSpeed: 0.1,
  sinkSpeed_why:
    "Once not rising, a HOLD ride sinks at a constant 0.1 m/s (0.3 m over a 30 m wall at 10.4 m/s): Lucio rides level for as long as jump is held ('as long as it is held', lucio.md section 1; on-wall gravity not found). Round 2: 0.3 m/s landed a ride begun 0.6 m up in 2 s (verbs35 row 9's low entry); 0.1 still reads as a ride, not a hover.",
  attachReach: 0.5,
  attachReach_why:
    "In the air with jump held, a valid wall within 0.5 m beside the motion (a ray left and right of the horizontal velocity) draws the player onto it, so a near miss rides ('jump at a nearby wall and hold the Jump key', overwatch.fandom.com/wiki/Wall_Ride; lucio.md spec 1, 9). Ours (round 2: verbs35 row 9 ran along a wall 0.35 m off it and landed before touching). The web needs a contact.",
  attachPull: 4,
  attachPull_why: "The speed into that wall while it draws (m/s): 0.5 m closes in about 0.13 s. Ours.",
  safetyCap: 8,
  safetyCap_why:
    "LOGGED (the 1.2 s cap removed, NIGHT35.md M1): a HOLD ride ends by itself only after 8 s (83 m at 10.4 m/s; the event's reason 'safety', counted), a guard against a stuck ride, never a meter. Lucio: no time limit ('as long as it is held'; an official numeric limit not found, lucio.md section 1).",
  cornerMinHold: 0.35,
  cornerMinHold_why:
    "A HOLD ride (jump held) follows corners, only after 0.35 s on the wall (a ride without jump held keeps the web's corner end; round 2: following corners on every ride stalled the walk bot 0 / 4 in n33-spire v6): OW May 3 2018, 'Added a minimum time that the jump key must be held before Lucio can ride around corners' (over.gg/8166; lucio.md spec 5). Blizzard's value is not published: 0.35 s (42 steps) is ours, round 2: at 0.1 s a ride into a wall-jump flue's back wall turned out of the flue at its corner before the climb's wall jump (n35-foundry f4, the walk bot 0 / 10); a flue's corner comes inside 0.35 s, a lane's corner after it.",
  followMaxDeg: 100,
  followMaxDeg_why:
    "Inside corners (a wall ahead) and outside corners (the wall ends, the next face behind the ride's direction) up to 100 degrees off the ridden wall turn the ride onto the new face at the same speed (OW May 3 2018: 'Can now go around corners (both outside corners, and inside corners) without having to leave the wall', lucio.md spec 5). 100 covers a box's 90 degree corners and any pillar's facets; a sharper V ends the ride ('corner'). The web ends a ride at any contact more than wallRide.cornerAngleDeg (30) off.",
  reRideDeg: 30,
  reRideDeg_why:
    "The re-ride rule: a wall facing within 30 degrees of the last ridden one counts as the same wall (the community's 'same-facing wall', mobalytics.gg lucio-guide, lucio.md section 1; 30 = the web's wallRide.cornerAngleDeg).",
  reRideDistance: 3,
  reRideDistance_why:
    "The same (or same-facing) wall starts no new ride until a landing or until the player is 3 m (horizontal) from where the last ride ended: OW May 3 2018, 'Can now land back on the same wall after leaping away, provided his leap takes him far enough away' (lucio.md spec 6). Blizzard's distance is not published: 3 m is ours (a jump-off carries about 4.9 m out before it falls to its takeoff height: the harness `n35move ride`).",
  lateJump: 0.2,
  lateJump_why:
    "0.2 s (24 steps) after a ride ends on its own (away, lost, corner, blocked, timeout, safety, a crouch drop), a jump press or the release of a hold ride's jump is still the jump-off: OW Oct 23 2020, 'within 0.2 seconds will provide a jump boost' (crouch then release; overwatch.blizzard.com patch notes ptr 2020/10) and the community's late jump (lucio.md spec 7).",
  lostGrace: 0.05,
  lostGrace_why:
    "A ride whose wall probe finds nothing for up to 0.05 s (6 steps) keeps riding: the web's collider reports a capsule touching a vertical box or pillar edge as an edge contact, never a wall (its point is at the edge's end), so a ride sliding over a pillar's facet edge or clearing an outside corner would otherwise end 'lost' for one step. Ours (an engine fact, not a Lucio number).",
  stick: 1,
  stick_why:
    "A speed of 1 m/s into the wall on every ride step (the contact clips it: no speed is lost on a flat wall) keeps the capsule on a curved wall's face (a pillar) while the ride's normal is re-sampled every step (lucio.md spec 5: 're-sample the wall normal each step'). Ours: no source documents a stick force (lucio.md section 3: 'not found').",
};

rows.grapple35 = {
  _why:
    "Builder M (NIGHT35.md M1, D-2, D-9; audit.md item 3; GG 2026-10-11: the grapple 'shouldn't go limp ... it needs to take you to wherever you fire' and unlock 'like the 4th or 5th level'): every hook that goes out (a node, a bare wall, a big enemy's zip; a small enemy is the whip's) is a straight pull at once, held or not, at the NIGHT31 pull's constant speed (design.n31.grapplePull.speed), and stops at the hit point (ULTRAKILL's HookArm: a constant pull, no swing, rea-ultrakill.md :1545). A bare wall is never refused (design.n32.grapple32.tapNodesOnly is cleared). A second press, a jump in the air or a dash still let go. Outside the Mover (GrappleTap31.pullOnFire, SurvivalSim.Move35.cs; the Mover's own zip is the pull). The web has no grapple pull.",
  enabled: false,
  fromLevel: 4,
  fromLevel_why: "D-2: the grapple is owned in the campaign from the level at this place in the campaign order (1-based) on; before it (levels 1-3) the auto-own leaves it unowned (grapple35Here false), and with it the bend (bend34 fires on a rope latch). Off: NIGHT34's rule (owned on any level with nodes).",
  swing: false,
  swing_why: "D-9: the hold-to-swing (NIGHT30's pendulum, NIGHT31's hold past the 0.2 s tap window) is gone under grapple35; true keeps NIGHT31 / 32's tap-pull / hold-swing split (a bare wall still not refused).",
  arriveKeepMax: 4,
  arriveKeepMax_why: "'Stops there': the horizontal speed kept when the pull lets go at the anchor (the Mover's zip release, arcline.zip.arrive 2.6 m short) is clamped to 4 m/s (NIGHT31's design.n31.grapplePull.arriveKeepMax 12 slid on past the point: audit.md item 3, 'arriveKeepMax decides how dead the arrival feels'). 4 m/s: half the walk speed, enough to touch a wall the pull hit (a wall ride can start from it). A cancel before the arrival keeps its speed (NIGHT32's cancelKeepMax 20).",
  arrivePop: 3,
  arrivePop_why: "Metres per second added upward at the arrival (NIGHT31's arrivePop 5 cleared a ledge after a long carry): 3 m/s (about 0.2 m up) so the arrival is a stop with a small lift, not a hop over the point.",
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

let n35At = raw.indexOf('"n35": {');
if (n35At < 0) {
  if (!localInsert) throw new Error("design.n35 not found (the shared commit adds it); --local-insert for scratch testing only");
  const designAt = raw.indexOf('"design": {');
  if (designAt < 0) throw new Error("design block not found");
  const brace = raw.indexOf("{", designAt);
  raw = raw.slice(0, brace + 1) + eol + '    "n35": {' + eol + "    }," + raw.slice(brace + 1);
  n35At = raw.indexOf('"n35": {');
}
const n35Open = raw.indexOf("{", n35At);
let n35End = objectEnd(raw, n35Open);
for (const name of Object.keys(rows)) {
  if (only && only.slice(7) !== name) continue;
  const key = '"' + name + '": {';
  const at = raw.indexOf(key, n35Open);
  const lineStart = raw.lastIndexOf("\n", at) + 1;
  const indent = at >= 0 ? raw.slice(lineStart, at) : "      ";
  if (at < 0 || at > n35End) {
    if (!localInsert) throw new Error("design.n35." + name + " not found inside design.n35 (the shared commit adds the rows)");
    const text = indent + '"' + name + '": ' + serialize(rows[name], indent);
    const closeLine = raw.lastIndexOf("\n", n35End - 1) + 1; // the n35 block's closing line
    const body = raw.slice(n35Open + 1, closeLine).trim();
    raw = raw.slice(0, closeLine) + (body.length ? "" : "") + text + eol + raw.slice(closeLine);
    if (body.length) throw new Error("--local-insert into a non-empty n35 block is not supported");
  } else {
    const open = raw.indexOf("{", at);
    const end = objectEnd(raw, open);
    const cur = JSON.parse(raw.slice(open, end));
    if (typeof cur.enabled === "boolean") rows[name].enabled = cur.enabled; // the row's switch is the lead's
    raw = raw.slice(0, open) + serialize(rows[name], indent) + raw.slice(end);
  }
  n35End = objectEnd(raw, n35Open);
  console.log("design.n35." + name + " written");
}
JSON.parse(raw); // still valid JSON
fs.writeFileSync(file, raw);

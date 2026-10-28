// NIGHT63 builder V: writes ONLY design.n63.bossMotion63.numbers (builder V's row; the lead's A1 wrote the row with numbers {}) into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file
// with that one object put back equals the old one, keys, values and order) and that the two copies are byte-identical (L-049, L-065).
//   node Tools/night/n63-v-rows.cjs [--dry]            first fill: refused unless the numbers are {} (A1's empty object)
//   node Tools/night/n63-v-rows.cjs --replace [--dry]  a later tuning round: replaces builder V's own numbers object (still only that object)
// The row is read with NightRows.ViewOn(cfg, "bossMotion63") every frame (BossMotion63.On); -skygrindOff63 (or older) and
// -skygrindNoBossMotion63 turn it off. Its `enabled` stays false here: only a freeze turns it on.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const replace = process.argv.includes("--replace");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const NUMBERS = {
  _why: "Builder V's numbers for BossMotion63 (Game/BossMotion63.Layer.cs; NIGHT63.md section 3 V; Research/2026-10-22-forge63/research-boss-animation.md). View only: the sim never reads them (no tape bit; the hitboxes, the damage frames and every fingerprint untouched). Distances are SHARES of each boss's own size, its collider radius from the sim's own config (THE WICK design.n37.wick37.boss.cageR 1.7 m, THE ICEBREAKER design.n40.brawl40.body.radius 1.2 m, THE KILN WYRM half its head design.n24.wyrm.body.headW / 2 = 1.5 m, THE LOOM design.n37.loom37.bodyHalfH 1.6 m, THE CROWN design.n37.crown37.crown.ringRadius 3 m), so one table fits five bosses of different sizes. The springs are t3ssel8r's second-order system (k1 = z / (pi f), k2 = 1 / (2 pi f)^2, k3 = r z / (2 pi f); f the natural frequency in Hz, z the damping, r the initial response: r > 1 overshoots, r < 0 dips first), stepped on the world's frame time (slow motion and the sim's hitstop apply: the view freezes with the sim) in sub-steps under the system's safe step; the flinch's envelope runs on the frame clock so it shows inside the sim's hitstop (no second freeze). The web has no such numbers: it has no campaign bosses.",
  follow: {
    _why: "The drawn body follows the sim's rigid pose through a spring instead of copying it: it lags a little as the boss moves off, overshoots as it stops (momentum).",
    f: 2.2,
    f_why: "Hz. The research's 1.5-3 Hz band for a body that trails; 2.2 Hz settles in about 0.4 s, quick enough that a walk's lag stays a few centimetres.",
    z: 0.5,
    z_why: "Damping: under-damped (the research's 0.4-0.7) so a stop overshoots once and settles, never wobbles.",
    r: 1.6,
    r_why: "Initial response > 1: the body answers a start or a stop with a small overshoot (the research's r 1-2 for trailing parts).",
    snap: 4,
    snap_why: "A rigid jump larger than this share of the boss's size in one frame (a teleport, a leg change, a retry) resets the spring to the rigid pose: never a long slide across the room.",
  },
  lean: {
    _why: "The lean spring the wind-up and the strike drive: -1 is the full wind-up lean back, + the strike's lunge forward. Its overshoot past rest after the strike is the follow-through.",
    f: 2.8,
    f_why: "Hz: faster than the body's follow so the pose reads as the boss's own action.",
    z: 0.45,
    z_why: "Damping: lightly under-damped so the strike snaps through rest and swings back once (follow-through, overshoot). Simulated (the layer's own step, 60 fps): a 0.55 s wind-up's pose shows 0.45 s before its hit; the strike peaks at 1.18 of the full lean forward and swings back only to -0.22 (at z 0.42, r 1.5, hold 0.16 s it swung to 1.29 and -0.34: a wobble).",
    r: 1.2,
    r_why: "Initial response > 1: the strike's step change kicks the lean forward at once (a snap, not an ease); 1.2, not 1.5, so the snap does not slam into the guard's tilt.",
  },
  windup: {
    _why: "The anticipation pose before each attack, driven by the sim's own attack state (BossAttackClock63: the wind-up's progress toward its hit): the boss leans back away from the player, pulls back and crouches. Eased in over the first riseShare of the wind-up and then held, so the pose is up long before the hit (B1's doneWhen: visible >= 250 ms before every hit; the shortest wind-up in config is about 0.55 s).",
    riseShare: 0.3,
    riseShare_why: "The share of the wind-up over which the pose eases in (then it is held to the hit): 0.3 of a 0.55 s wind-up is 165 ms, so the pose is visible about 0.35 s or more before the hit.",
    deg: 13,
    deg_why: "Degrees the boss leans back (its top away from the player) at the full wind-up: readable at 20 m (a 4 m boss's top moves about 0.9 m).",
    back: 0.14,
    back_why: "The pull back away from the player at the full wind-up, a share of the boss's size.",
    crouch: 0.1,
    crouch_why: "The squash at the full wind-up (height x (1 - crouch), the width kept volume-equal): a coiled spring.",
    sideDeg: 10,
    sideDeg_why: "Degrees the boss cocks to one side at the full wind-up (the side alternates attack by attack). A lean toward or away from the player turns mostly in depth from where the player stands; a side lean turns in the screen's plane, so the wind-up reads on screen.",
    breathMute: 1,
    breathMute_why: "How much of the idle breathing the wind-up mutes (1: all of it), so the wind-up is the one motion on screen (the research's pitfall: wobble drowning the telegraph).",
  },
  strike: {
    _why: "The strike: the lean target jumps forward for holdS, then back to rest; the lean spring overshoots and settles (follow-through).",
    lunge: 0.75,
    lunge_why: "The lean target at the strike, a share of the wind-up's full lean, forward (top toward the player, body lunged by lunge x windup.back).",
    holdS: 0.25,
    holdS_why: "s the strike's forward target holds after the strike's first step (15 frames at 60 fps): long enough that the lean has reached it before the release, so the swing back stays small.",
  },
  flinch: {
    _why: "A hit flinch on each damage event (the boss's health falls): a volume-keeping squash, a recoil away from the player and a tip back, all on Quilez's impulse envelope h exp(1 - h), h = t / peakS, which peaks at 1 at peakS and is near 0 by lifeS. Its clock is the frame clock: it shows inside the sim's hitstop, adding no second freeze.",
    peakS: 0.04,
    peakS_why: "s from the hit to the squash's peak (the research's 40 ms): 2.4 frames at 60 fps, so the first drawn frame already shows 0.75 of it (B1's doneWhen: within 2 frames).",
    lifeS: 0.4,
    lifeS_why: "s the envelope lives (it is 0.001 of its peak by then).",
    squash: 0.26,
    squash_why: "The squash at the peak: height x (1 - squash), width x 1 / sqrt(1 - squash) (the volume kept). 0.26, not 0.2 (NIGHT63's rule-8 round: THE WICK's flinch changed 22,253 px on n63-b4's same-run pair; the squash is the flinch's one part in the screen's plane).",
    recoil: 0.12,
    recoil_why: "The recoil away from the player at the peak, a share of the boss's size.",
    deg: 7,
    deg_why: "Degrees the boss tips back (away from the player) at the peak.",
    minShare: 0.55,
    minShare_why: "The smallest flinch a hit draws, a share of the full one: a chip still reads as a hit.",
    fullAt: 0.02,
    fullAt_why: "A hit that takes this share of the boss's pool or more draws the full flinch (between, linear from minShare).",
  },
  slump: {
    _why: "A slump while staggered (a CYAN window, THE ICEBREAKER stuck or dazed, THE KILN WYRM staggered): the boss sinks, droops forward and squashes, and sways slowly side to side, through its own spring (it sags in, and rises back when the stagger ends). B1's SPOT pose: 0.6 s into a stagger.",
    f: 1.6,
    f_why: "Hz: a heavy, slow sag (settled by about 0.6 s, the reel's stagger frame).",
    z: 0.55,
    z_why: "Damping: one soft bounce at the bottom of the sag.",
    r: 1.2,
    r_why: "Initial response: a little drop past the hold as it gives way.",
    sink: 0.6,
    sink_why: "The sink at the full slump, a share of the boss's size (THE WICK 1.02 m, its raised limit; THE ICEBREAKER into its knees, capped at its 0.6 m): the slump's biggest on-screen move. NIGHT63's rule-8 round: 0.6, not 0.45 (n63-b4's same-run pair on THE WICK's stagger changed 37,345 px, under 41,472; the cage is about 200 x 300 px at boss63's 12.5 m).",
    deg: 12,
    deg_why: "Degrees the boss droops forward (its top toward the player: beaten, its weak point offered). 12, not 22: a droop toward the player turns about the screen's horizontal axis, mostly in depth from where the player stands, so the rule-8 round moved the tilt into the side keel.",
    sideDeg: 26,
    sideDeg_why: "Degrees the boss keels to one side at the full slump (the side alternates stagger by stagger): the keel turns in the screen's plane, so a player sees it. 26, not 14 (the rule-8 round); with the droop and the sway it stays under THE WICK's 30 deg guard and is scaled to a tighter boss's.",
    squash: 0.22,
    squash_why: "The squash at the full slump (height x (1 - squash), the volume kept): in the screen's plane. 0.22, not 0.12 (the rule-8 round).",
    swayDeg: 4,
    swayDeg_why: "Degrees of the slow side-to-side sway while slumped (dazed).",
    swayHz: 0.6,
    swayHz_why: "Hz of the sway.",
  },
  breath: {
    _why: "Idle breathing: a slow pulse in height and lift, so a boss is never a statue (muted through a wind-up: windup.breathMute).",
    hz: 0.4,
    hz_why: "Breaths a second: a slow, heavy breath (2.5 s).",
    scale: 0.04,
    scale_why: "The pulse in height (x 1 +- scale, the width kept volume-equal).",
    lift: 0.04,
    lift_why: "The rise and fall with the breath, a share of the boss's size.",
  },
  limit: {
    _why: "THE GUARD (B1's doneWhen): every view offset capped so the drawn body never leaves its collider: the body's offset from its rigid pose at most `share` of the boss's size (its collider radius), its tilt at most `deg`, its squash at most `squash` from 1 on any axis. Per boss below where its hitboxes need it tighter.",
    share: 0.5,
    share_why: "The largest offset, a share of the boss's collider radius: the drawn centre stays inside the inner half of its collider.",
    deg: 26,
    deg_why: "The largest tilt in degrees (a full slump 22 deg plus a sway).",
    squash: 0.32,
    squash_why: "The largest squash or stretch on any axis (a full flinch 0.26 on a breath; a full slump 0.22 on a breath). 0.32, not 0.28 (the rule-8 round).",
  },
  bosses: {
    _why: "Per boss: gain (every amplitude x gain), follow (the follow spring's share: 0 = the drawn body sits on its sim pose), limitShare / limitDeg (the guard, tighter where the drawn body is its hitbox). Read by the bossRise id.",
    wick: {
      gain: 1, follow: 1, limitShare: 0.9, limitDeg: 45,
      slump: {
        _why: "NIGHT63 B1d (Logs/n63/verdict-B1.md): THE WICK's own slump. b8's same-run stagger pair (boss63 n52-foundry, 12.5 m) changed 40,503 px, under rule 8's 41,472, with the shared slump pinned at its guard (limit share 1.00: the clamp set the size), and B1c's raise had gone mostly into the clamp (b4 37,345 -> b8 40,503) while its 1.02 m sink already sat the orb on the floor. So: a shallower sink, a deeper keel, a stronger squash and a slide in the screen's plane the way it keels, the full slump (0.86 of the radius) under the raised guard (0.9). A crude 2D model of the cage calibrated on b8's pair (the orb, the cap and the petals; Research/2026-10-22-wave63/V-B1d.md) put this at about 1.5 x b8's change: about 60,000 px. A model, not a measurement.",
        sink: 0.5,
        sink_why: "The sink at the full slump, a share of the 1.7 m radius: 0.85 m (the shared 0.6, 1.02 m, sank the orb through the floor at boss63's stagger frame).",
        deg: 14,
        deg_why: "Degrees of the droop toward the player at the full slump (the shared 12): under a 40 deg keel the droop's share of the top's lean toward the player shrinks (cos 40 = 0.77), so 14 keeps the weak point offered as before.",
        sideDeg: 40,
        sideDeg_why: "Degrees of the side keel at the full slump (the shared 26): the cage topples to one side in the screen's plane (with the droop and the sway, held at limitDeg 45).",
        squash: 0.3,
        squash_why: "The squash at the full slump (the shared 0.22): the height x 0.7, the width x 1.2, inside the shared squash guard 0.32.",
        slide: 0.7,
        slide_why: "The slide at the full slump, a share of the radius: 1.19 m sideways, the way the top keels (it falls over that way), alternating stagger by stagger. With the sink, the drawn centre sits 1.46 m (0.86 x 1.7) from the sim's centre: inside the 1.7 m collider and under the 0.9 guard (1.53 m).",
      },
      _why: "THE WICK: a floating cage (collider radius cageR 1.7 m, the sim's hit sphere) round its core; the whole cage moves. The rule-8 round raised its guard to 0.6 of the radius and 30 deg; B1d (its stagger pair still under rule 8 with every slump pinned at the guard) raises it to 0.9 of the radius (1.53 m: the drawn centre stays inside the collider) and 45 deg for its own slump (slump: the sink, the keel, the squash, the slide). Its other motions (the follow, the wind-up, the flinch, the breath) stay well under the guard as before.",
    },
    brawl40: { gain: 1, follow: 1, limitShare: 0.5, limitDeg: 26, _why: "THE ICEBREAKER: the torso and legs move; its fists stay on the sim's fist spheres (the arms re-span from the moved shoulders)." },
    wyrm: {
      gain: 0.6, follow: 0, limitShare: 0.25, limitDeg: 14,
      _why: "THE KILN WYRM: its drawn boxes ARE its hitboxes (WyrmView: 'a shot finds what you see'): no follow lag along its fast path, a smaller pose on the head and the pieces, the tightest guard (0.375 m on the head; each body piece's own guard is limitShare x its half width). The head takes the whole motion; the 24 body pieces and the tail take the flinch as a ripple running down the body, the slump's sag and the breath as a wave, never a tilt.",
      rippleS: 0.025,
      rippleS_why: "s the flinch's ripple takes per piece down the body (the tail's piece about 0.6 s after the head's).",
      tail: 0.35,
      tail_why: "The share of the head's ripple, sag and breath the tail keeps (linear from 1 at the neck).",
      wave: 0.45,
      wave_why: "Radians of the breath's phase per piece: a slow swell travelling down the body.",
    },
    loom37: { gain: 1, follow: 1, limitShare: 0.5, limitDeg: 20, _why: "THE LOOM: the hull (loomLook47 / loomHull48) moves; the knees are the sim's grapple nodes and never move (the upper legs re-span)." },
    crown37: { gain: 0.8, follow: 1, limitShare: 0.25, limitDeg: 18, _why: "THE CROWN: the ring is the pass's target, so its guard is tight (0.75 m of a 3 m ring, inside its 2.4 m pass radius)." },
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const row = before.design && before.design.n63 && before.design.n63.bossMotion63;
if (!row) throw new Error("no design.n63.bossMotion63 (the lead's A1 writes it first)");
if (typeof row.numbers !== "object" || row.numbers === null) throw new Error("design.n63.bossMotion63.numbers is not an object");
if (!replace && Object.keys(row.numbers).length > 0) throw new Error("design.n63.bossMotion63.numbers is not empty (use --replace for a tuning round)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

// the numbers object's text span: from `"numbers": ` inside the bossMotion63 row to its matching closing brace (strings skipped)
const rowAt = raw.indexOf('"bossMotion63": {');
if (rowAt < 0 || raw.indexOf('"bossMotion63": {', rowAt + 1) >= 0) throw new Error("design.n63.bossMotion63 not found exactly once");
const key = '"numbers": ';
const numAt = raw.indexOf(key, rowAt);
const nextRow = raw.indexOf('"bossTell63"', rowAt);
if (numAt < 0 || (nextRow >= 0 && numAt > nextRow)) throw new Error("bossMotion63's numbers not found inside its row");
let i = numAt + key.length;
if (raw[i] !== "{") throw new Error("numbers does not open with {");
let depth = 0, inStr = false, end = -1;
for (let k = i; k < raw.length; k++) {
  const ch = raw[k];
  if (inStr) { if (ch === "\\") { k++; continue; } if (ch === '"') inStr = false; continue; }
  if (ch === '"') { inStr = true; continue; }
  if (ch === "{") depth++;
  else if (ch === "}") { depth--; if (depth === 0) { end = k + 1; break; } }
}
if (end < 0) throw new Error("numbers' closing brace not found");
const lineStart = raw.lastIndexOf("\n", numAt) + 1;
const indent = raw.slice(lineStart, numAt); // the numbers key's own indent
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the numbers key on its line");
const block = JSON.stringify(NUMBERS, null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, i) + block + raw.slice(end);

const after = JSON.parse(out);
if (JSON.stringify(after.design.n63.bossMotion63.numbers) !== JSON.stringify(NUMBERS)) throw new Error("the numbers not written as the script says");
after.design.n63.bossMotion63.numbers = row.numbers;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63.bossMotion63.numbers changed (keys, values or order)");
if (after.design.n63.bossMotion63.enabled !== false && !replace) throw new Error("bossMotion63 must stay OFF here");
if (dry) {
  console.log("n63-v-rows --dry: design.n63.bossMotion63.numbers would be written (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-v-rows: design.n63.bossMotion63.numbers written (" + Object.keys(NUMBERS).filter((k) => k !== "_why").join(", ") + "); enabled " + after.design.n63.bossMotion63.enabled + "; both copies byte-identical; nothing else changed");

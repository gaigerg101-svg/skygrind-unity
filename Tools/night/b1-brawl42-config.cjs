// NIGHT42 builder B1 (brawl42): writes design.n42.brawl42 (THE ICEBREAKER's new attacks, phase 2 harder) in Design/unity-config.json and
// copies the file to Assets/StreamingAssets/Design/unity-config.json (the two byte-identical before and after). Patches ONLY the brawl42
// row's text (L-049): the row from `"brawl42": {` to its closing brace at the same indent is replaced; `enabled` stays false (the lead turns
// it on after the frames). Re-runnable (it replaces the row each time). Run from the repo (or a worktree) root: node Tools/night/b1-brawl42-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");

const off = "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT42.md method 2; closing_reminder).";
const row = {
  _why: "Builder B1 (G14, GG: 'level 2 boss might needs more attacks and needs to be more difficult'; DECIDE42 D-5): THE ICEBREAKER gains two new attacks with fair tells (RED tell36, red-orange, >= 0.9 s), each with its own answer: the SWEEP (a low spinning arm up close: JUMP it; the haymaker's answer is a dash, so up close the player must read which) and the LEAP (from mid range it jumps onto the spot you stood on: DASH out of the locked circle; it lands stuck, the punish). Phase 2 harder: shorter rests, a haymaker chains into a sweep, the leap's landing sends the slam's ring. Never more HP (GG w31-32): pool, stagger and the damage rule are NIGHT40's. Sweep / Sekiro's read-the-answer grammar (sweep = jump, thrust = deflect, research/boss-web.md section 1 'Others'); the leap after Minos Prime's 'Judgement!' dropkick (a tell, a committed landing). Numbers measured with `n42brawl` (Tools/SimHarness/Harness.N42.Brawl.cs); Research/2026-10-14-wave42/B1.md.",
  enabled: false,
  enabled_why: off,
  rest2: 0.55,
  rest2_why: "Phase 2's rest between attacks (NIGHT40's design.n40.brawl40.rest.seconds2 0.8): faster chains, every wind-up still >= 0.9 s (DECIDE42 D-5 'phase 2 faster').",
  sweep: {
    _why: "THE SWEEP: up close it drops, cocks its right arm low and back (RED tell36 at the fist, a red-orange ring on the ice at its reach), then spins once with the arm at shin height. Answer: JUMP as the arm comes (feet above sweep.height when it passes you), or dash out of its reach. Then it is dizzy (sweep.recover): the punish.",
    windup: 0.9,
    windup_why: "The wind-up (boss_rules 1: heavy >= 0.9 s; DECIDE42 D-5 'red-orange >= 0.9 s').",
    windup2: 0.9,
    windup2_why: "Phase 2's wind-up: kept at the 0.9 s floor (phase 2 is harder by its rests and chains, never by a shorter tell).",
    active: 0.4,
    active_why: "Seconds the arm takes for one full turn. It starts on the far side from you, so it reaches you after half of it (0.2 s after the tell ends): a jump pressed from ~0.3 s before the tell ends to ~0.1 s after it clears (the jump holds the feet above 0.5 m for ~0.5 s: config jump.height 1.5, apexTime 0.36).",
    recover: 0.7,
    recover_why: "Seconds it stays dizzy after the spin (no attack, it turns slowly): about one gauntlet swing to punish.",
    reach: 4,
    reach_why: "Metres past its body's surface the arm reaches (the haymaker's lunge + reach is 5.1): the skilled pilot circles at 2.5 m, the casual at 3.5 m, both inside it.",
    startDist: 4,
    startDist_why: "Metres off its body within which it may pick the sweep instead of the haymaker.",
    height: 0.5,
    height_why: "Metres above the floor the arm reaches (the slam ring's 0.5): feet higher pass over it.",
    armDeg: 14,
    armDeg_why: "Degrees of slack on each side of the arm's line for the hit test (the arm is a thick box, not a line).",
    damage: 22,
    damage_why: "Health a sweep takes (a medium: under the haymaker's 30, over the slam's 20).",
    knock: 11,
    knock_why: "Metres per second outward (away from its body) on a hit.",
    knockUp: 4,
    knockUp_why: "Metres per second up on a hit (it trips you).",
    chance: 0.45,
    chance_why: "Within sweep.startDist: the chance it sweeps instead of the haymaker (never two sweeps running): up close the player must read which one is coming (a high cocked fist: dash; a low arm and a ring: jump).",
    chain2: 0.6,
    chain2_why: "Phase 2: the chance a haymaker up close is chained into a sweep after its stuck fist (when the slam chain did not fire): the punish window ends in a tell you must jump.",
    spins2: 2,
    spins2_why: "Phase 2: the sweep spins this many times (each turn sweep.active long, each pass can hit once): the arm passes you at 0.2 s and 0.6 s after the tell, one jump stays over 0.5 m ~0.5 s, so the second pass wants a second jump (the double jump, config jump.doubleJumps 1) or a dash out of reach. Phase 2 harder by a new answer, not a shorter tell (round 2: with gauntlet42's 1.8 s swing the skilled band fell to p50 43.7 s)."
  },
  leap: {
    _why: "THE LEAP: from mid range it crouches (RED tell36 at its head) and a red-orange landing circle tracks you on the ice; at leap.trackShare of the wind-up the circle LOCKS (it brightens), then it jumps and lands on it: a hit inside the circle, then its fists are stuck in the ice (the punish, every hit x stagger.stuckMult). Answer: DASH (or run) out of the locked circle. Phase 2: the landing also sends the slam's ring (jump it). The answer to keeping your distance (NIGHT40's block / rush are far answers; the leap closes the gap).",
    windup: 1.1,
    windup_why: "The wind-up: 1.1 s, the circle tracking for the first 0.66 s then locked for 0.44 s + the air time (~1 s from the lock to the landing; boss_rules 1 heavy >= 0.9 s).",
    trackShare: 0.6,
    trackShare_why: "The circle follows you for the first 60 % of the wind-up, then locks (the haymaker locks at 70 %, the rush at 60 %).",
    air: 0.55,
    air_why: "Seconds in the air from the jump to the landing.",
    apex: 4.5,
    apex_why: "Metres up at the top of the arc (above the 3.6 m body: it reads as a jump).",
    startDist: 7,
    startDist_why: "Metres off its body past which it may leap (between the haymaker's 5 and the block's 9).",
    chance: 0.5,
    chance_why: "Past leap.startDist: the chance it leaps rather than its NIGHT40 pick (never two leaps running).",
    radius: 3.2,
    radius_why: "Metres round the landing spot it hits (the body's 1.2 + 2 m of shock): one dash (24 m/s x 0.25 s = 6 m) clears it from anywhere inside.",
    hitHeight: 2.5,
    hitHeight_why: "Metres above the floor the landing reaches: a hop does not answer it, leaving the circle does.",
    damage: 28,
    damage_why: "Health a landing takes (a heavy, under the haymaker's 30).",
    knock: 10,
    knock_why: "Metres per second outward from the landing spot on a hit.",
    knockUp: 6,
    knockUp_why: "Metres per second up on a hit.",
    stuckSeconds: 1,
    stuckSeconds_why: "Seconds its fists stay in the ice after the landing, hit or miss: the punish (the haymaker's stuck fist is 1.2).",
    recover: 0.5,
    recover_why: "Seconds to pull free before it walks again.",
    ring2: 1,
    ring2_why: "Phase 2 (1 = on): the landing sends the slam's shockwave ring (design.n40.brawl40.slam's ring numbers): dash out of the circle, then jump the ring."
  }
};

// find the row
const key = '"brawl42": {';
const at = raw.indexOf(key);
if (at < 0) throw new Error("design.n42.brawl42 not found");
const lineStart = raw.lastIndexOf("\n", at) + 1;
const indent = raw.slice(lineStart, at);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the row's key");
const eol = raw.slice(at + key.length, at + key.length + 2) === "\r\n" ? "\r\n" : "\n";
const closeRe = new RegExp("\\r?\\n" + indent + "\\}", "g");
closeRe.lastIndex = at;
const cm = closeRe.exec(raw);
if (!cm) throw new Error("the row's closing brace was not found");
const end = cm.index + cm[0].length;
const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, at) + '"brawl42": ' + body + raw.slice(end);
JSON.parse(out); // still valid JSON
fs.writeFileSync(file, out);
fs.writeFileSync(copy, out);
console.log("design.n42.brawl42 written (" + (end - at) + " -> " + ('"brawl42": '.length + body.length) + " chars), copied to StreamingAssets");

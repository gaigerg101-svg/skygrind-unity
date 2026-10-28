// NIGHT47 builder W: writes the numbers of W's rows (design.n47.<row>.numbers: climb47, wick47, wickTop47, wickLook47) into BOTH config copies
// (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json), touching ONLY those rows' "numbers" objects (a text splice
// at the row's own "numbers" key; the file's CRLF and indentation kept), then checks: both copies parse, every other byte of design (n27-n47,
// every other row) round-trips unchanged, and the two copies are byte-identical.
// Usage (from the worktree root): node Tools/night/w47-config.cjs <row> [<row> ...] [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const COPIES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

const NUMBERS = {
  climb47: {
    setUp: 10.0,
    setUp_why: "m/s: a jump-off or a wall jump off a CLIMB WALL sets the up speed to at least this (Celeste's WallJump assigns Speed.Y, never adds: Research/2026-10-18-wave47/W.md, Verified). Measured today (W.md, n47wick reach): a +45 deg jump-off gains 2.16 m, a level one 1.15 m, the panels rise 2.80 m: each attach lands 0.56 m lower (GG's 'slipping further down each time'). Tuned on n47wick reach --rows climb47 and the sloppy pilot.",
    catchBelow: 2.0,
    catchBelow_why: "m: THE CATCH reaches this far under the catch line (an attach with the feet up to 1.2 m under it is lifted): the measured slip is 0.56 m a gap, so two slips in a row are still caught (W.md stackchain).",
    catchLine: 1.0,
    catchLine_why: "m over the climb panel's bottom: where every caught attach starts (the feet), so each jump-off leaves from the same height on its panel and gains the same (G2: +-5 %); 0.5 m keeps the face 3.0 m over the feet (ride40 asks 2 m) on the 3.5 m panel.",
    catchAbove: 0,
    catchAbove_why: "OFF (0): round 1b measured 2.5 (with ride40's face-height rule waived on climb panels): the control hand 0 / 20 to the cap (it settled every high attach down and fell short); was: m: THE CATCH settles an attach whose feet are up to this far OVER the catch line down to it (eased, as from below): measured with setUp 12 (W.md round 1), attaches crept up 0.3 m a gap until a jump-off flew over the next panel's top; settling both ways makes every jump leave from the same height.",
    catchEase: 0.12,
    catchEase_why: "s: the catch's lift is eased over this (a speed, never a snap: the contact resolves it); 0.12 s = 14 steps, about Celeste's corner-correction feel (W.md: described there without a number, so ours).",
    coyote: 0.15,
    coyote_round4_why: "Round 4: 0.25 s measured NO change (kept 0.15; the coyote now also covers a ride that ends past its panel, Climb47.State.onClimb): every top-third ride miss of the sloppy pilot (n47wick sloppy --trace, RIDE MISS lines) was a release 150 ms late that ran past the panel's end: the ride ended on its own and the 0.15 s window closed before the late release.",
    coyote_why: "s: a ride on a climb wall that ends on its own (the panel's end, a lost contact) leaves the jump-off open this long (wallride35's late window, lengthened only on a climb wall; NIGHT47.md section 3: 0.15 s; Celeste's ledge coyote 0.1 s, JumpGraceTime: W.md, Verified).",
    buffer: 0.15,
    buffer_why: "s: a jump let go in the air up to this long before a ride starts on a climb wall is that ride's jump-off on its first step (NIGHT47.md section 3 / DESIGN-RULES M-3: 0.15 s; the sloppy pilot's +-120 ms press jitter fits inside).",
    catchReach: 0.9,
    catchReach_why: "m past the capsule: the near-miss draw onto a climb wall (ride40's attachReach 0.5 elsewhere); on a climb wall jump held is the intent (ride40's stick rule waived), so a look up to ~15 deg off the face still attaches (NIGHT47.md section 3; Celeste: a wall jump from 3 px off the wall, W.md Verified).",
    guideOver: 1.6,
    guideOver_why: "m: SET UP is GUIDED: a jump-off off a climb wall rises to the next hold's height (the next panel's bottom, a ledge, the cap) + this over the feet, never short and never over the next panel's top (measured round 1: a fixed 10 m/s flew the casual hand over a 2.2 m step's 3.5 m panel: 0 / 10 first tries on gap 1 -> 2); 1.6 m: the catch line (1.0) + a margin, under the panel's top - 1.8 (the capsule).",
    guideMin: 6.0,
    guideMin_why: "m/s: the guided up speed never under this (a jump still feels like a jump: 6 m/s rises ~0.8 m).",
    guideMax: 0,
    guideMax_why: "OFF (0 = no guidance, setUp alone): round 1b measured 12.5: the control 20 / 20 in 15.5 s but the sloppy hand 8 / 20, bottom first tries 15-40 % (unguided: 7 / 20, 40-65 %): R2 retunes it. When on: m/s: nor over this (a 3.4 m rise at the game's 23 m/s2: a step taller than the top band's 2.8 m + 1.6 never asks more).",
    faceAbove: 0,
    faceAbove_why: "m: on a climb panel (jump held) ride40's attach asks the face only this far over the feet (design.n40.ride40.minWallHeight 2 elsewhere): measured round 1, a jump-off rising 2.16 m from a low step arrived with the feet over the 3.5 m panel's top - 2 m and never rode it (0 / 220 on a 1.15 m step); 0.9 m keeps the capsule's middle on the face.",
    retrySpeed: 10.4,
    retrySpeed_why: "m/s: a ride on a climb wall runs at least this along it, so a retry from a catch ledge starts at full climb speed (G2); 10.4 m/s = THE STACK's measured ride speed (Tools/SimHarness/Harness.N37.Wick.cs, wick37 gaps).",
  },
};

const atk = (o, why) => Object.assign(o, { _why: why });
NUMBERS.wick47 = {
  gate: [0.45, 0.40, 0.15],
  gate_round1_why: "Round 1 measured (n47human --rows wick47,climb47,wickTop47, 40 seeds) with gate [0.65, 0.40] and chip 0.35: median 76.0 s (P1 28 s, P2 26 s, P3 19 s; the band 90-150: OUT, short); now P1 floors at 45 % (round 4b at 50 %: the skilled pilot 89.1 s, OUT by 0.9 s; round 1b at 60 %: median 79.9 s, P3 16 s, one charge; round 4 at 55 % with the steps: the skilled pilot 84.2 s OUT, P2 17.9 s), and P3's guns stop at 15 %: only the open core's punch (the finisher, finishAt) ends it, so P3 is two charges, not one (the punch is P3's answer: GG 'hit it with melee').",
  gate_why: "The phase gates (W-cards.md section 1): the bar floors at 55 % in P1 and reaching it starts P2 at once; it floors at 40 % in P2 until the player stands on the cap (P3); in P3 the guns stop at 15 % (the core punch finishes it). Replaces design.n37.wick37.boss.legLock [0.6, 0.3, 0] (the bar silently stopped: DESIGN-RULES B-1).",
  chip: 0.30,
  chip_why: "A hit outside a window lands 30 % of itself (round 1: 0.35, the fight too short) (design.n37.boss37.chipScale is 0.1: GG 'you can barely damage it even while firing consistently'; NIGHT47.md section 3: chip >= 30 % of full). The pool stays 600 (pools only go down).",
  above2: 4,
  above2_why: "m: in P2 THE WICK floats 4 m over the climber (was 2, design.n37.wick37.boss.aboveP): in view while the look is up on a ride, where the climb goes.",
  groundCrack: true,
  groundCrack_why: "In P1 and P3 (floors) a KESTREL shot through a cracked plate lands in full without a ride (the ride rule is P2's, where the panels are): the parry -> pierce combo GG likes, on every floor.",
  netDrop: 3.0,
  netDrop_why: "m: THE NET (G2: 'a failed jump never drops the player more than one segment'): a fall this far under the last ledge stood on returns the player to it (wick47Updraft); 3 m = a bit over one panel's rise (1.15-1.7 m) twice.",
  netCost: 10,
  ledgeChip: 0.04,
  ledgeChip_why: "Round 3 (the lead: P2 must not be a pure wait): a ledge reached in P2 lands 4 % of the bar (floored at the P2 gate): the sloppy climb + boss p90 was 185.1 s.",
  netCost_why: "Health a net catch costs in the fight (DESIGN-RULES A-4: a fall costs time and a little health; the slag's touch costs 30).",
  stack: {
    _why: "Round 3: 5 plain steps (stepCount) then 8 ride panels (2.5 / 2.8 m; ledge 2 after panel 3), the cap at 26.3 m. Earlier: n47-foundry's STACK (W-cards.md section 3): Wick37Stack with these over design.n37.wick37.stack (every other number kept). Gaps as ratios of the measured reach (W.md: a +45 deg jump-off gains 2.16 m): bottom 1.15 m (0.53), middle 1.5 m (0.69), top 1.7 m (0.79): under 55 / 70 / 80 % (NIGHT47.md section 3), none in 0.8-1.1; with climb47's catch every gap needs less than its rise.",
    riseBySection: [2.5, 2.5, 2.8],
    stepCount: 5,
    stepCount_why: "Round 3 (the lead; GG: \"start much more forgiving at the bottom, with LOWER jumps\"): the bottom section is 5 plain STEP ledges, no ride: a jump and a double jump climb it (the ride is taught from ledge 1 up, the top as before). Round 2 measured the bottom with rides: sloppy first tries 39-65 %.",
    stepRise: 1.4,
    stepRise_why: "m each step over the last (the first over the floor): 0.49 of the double jump's measured 2.88 m reach (W.md), under the lead's 55 %; a single held jump (1.58 m) clears it too.",
    stepDeg: 50,
    stepDeg_why: "deg round the shaft from one step's centre to the next: 7.0 m of arc at 8 m, a short run then a jump (the edge-to-edge gap 6 deg, ~0.8 m).",
    stepArcDeg: 48,
    stepArcDeg_why: "deg each step spans (6.1 m of arc): room to land and run.",
    stepDepth: 2.6,
    stepArcs: [48, 48, 48, 62, 62],
    stepDepths: [2.6, 2.6, 2.6, 4.2, 4.2],
    stepDepths_why: "m per step (round 4): steps 3 and 4 4.2 m deep (3.6 measured 94.3 % first tries over the 5 steps) (their undersides 5.2 / 6.6 m up: no floor pocket under them; the 2.6 m steps 0-2 keep the floor clear): measured, the misses flew a straight chord over the step's inner edge (landing 5-6 m from the axis).",
    stepArcs_why: "deg per step (round 4): steps 3 and 4 widened to 62 (8.7 m of arc at 8 m): measured (n47wick sloppy --trace, 11 misses in 20 seeds) every miss at steps 3 / 4 flew OVER the step: a run built up over steps 0-2 plus a late press (0-240 ms) carries a jump 7+ m, past a 48 deg (6.7 m) step.",
    stepGapDeg: 2,
    stepGapDeg_why: "deg between one step's end and the next's start (the 50 deg spacing of 48 deg steps had the same 2).",
    stepToPanelDeg: 75,
    stepToPanelDeg_why: "deg from ledge 1 (the last step) to the first ride panel's centre: 75 (not the spiral's 90) so the panel's face starts 9 deg (~1.3 m) past the ledge's end: the mount is a short hop (round 3 measured at 90: the sloppy hand walked off the ledge's end, 12 / 20 reached the rides); its bottom 7.3 m stands over the 4.5 m entry tunnel.",
    stepDepth_why: "m deep from the wall (the lead: 1.4 m; 2.0 so the feet stand 1 m clear of the hot wall's band).",
    riseBySection_round2_why: "Round 2 (the lead: the bottom third's rises at <= 45 % of the reach, since two Mover fixes measured worse): section 1 rises 1.0 m (0.46 of the 2.16 m reach), with its panels 4.4 m tall so a jump-off from a 1.0 m step still finds 2 m of face over the feet (ride40), and 10 m long (an early release still lands). Round 1: [2.2, 2.5, 2.8] (sloppy bottom first tries 39-65 %).",
    heightBySection: [],
    heightBySection_why: "m: the panels' height per section: 4.4 at the bottom (a 1.0 m step + the 2.16 m jump-off rise + ride40's 2 m of face; the slot's next panel starts 3.7 m up: 0.7 m of overlap, the same face plane), 3.5 above (design.n37.wick37.stack.panelHeight).",
    lengthBySection: [],
    lengthBySection_why: "m: the ride faces per section: 10 m at the bottom (wider: a release anywhere 35-85 % of the face still meets the next one), 8 above (design.n37.wick37.stack.panelLength).",
    riseBySection_why: "m per panel in section 1 (floor to ledge 1), 2 (to ledge 2), 3 (to the cap): lower jumps at the bottom, slightly harder near the top (GG).",
    ledgeAfter: [3],
    ledgeAfter_why: "Ledges (the catch ledges and the net's floors) after panels 4 and 9: three sections of 5 panels.",
    panelCount: 8,
    panelCount_why: "15 panels as today (the sloppy pilot climbed today's 15 in a median 28.6 s with climb47: G1's 25-45 s); the cap at ~22.5 m (was 38.4).",
  },
  vents: [5.196, 3.0, -5.196, 3.0, 0.0, -6.0],
  vents_why: "P1's floor vents (x, z pairs, arena-local, 6 m from the axis at 30 / 150 / 270 deg): 3 m clear of the 9 m wall and never at the entry gap (+z, 90 deg): the SLAG GEYSER's places, learnable (fixed).",
  capVents: [11.0, 0.0, 0.0, 11.0, -11.0, 0.0, 0.0, -11.0],
  capVents_why: "P3's vents on the cap's catwalk (11 m, between the shaft's rim 9 m and the cap wall 13 m; the glass covers the floor's).",
  geyser: { radius: 1.8, damage: 18, launch: 9, sink: 1.5, count: 2, _why: "SLAG GEYSER (G6): radius m of a vent's eruption, damage, the launch up (m/s: ~1.75 m), the sink window s (THE WICK level with the player's head, bare: full damage), the vents that erupt (the nearest 2)." },
  curtain: { height: 6, speed: 5, radius: 2.5, count: 5, _why: "EMBER CURTAIN (G6, P2 at a ledge: the camp habit): globs start 6 m over the camper, fall 5 m/s (1.2 s to the head: parry one up), on a 2.5 m ring, 5 of them." },
  ring: { radius: 3, damage: 22, knock: 6, below: 0.9, above: 1.4, bare: 1.5, speed: 12, _why: "PLATE RING (G6, P3): the plates sweep out at 12 m/s across the cap, a band from 0.9 to 1.4 m over the floor (jump it: the capsule's foot sphere over 1.4 m; a slide passes under), 22 damage + a 6 m/s knock; the core bare for 1.5 s." },
  charge: { speed: 16, damage: 25, knock: 10, reach: 1.6, openSeconds: 3.0, punchShare: 0.22, punchReach: 3.5, launchDist: 16, launchDeg: 30, finishAt: 0.15, rim: 1.5, punches: 2, hitstop: 0.22, _why: "CRACK-OPEN CHARGE (G7; hitstop s on the core punch: DESIGN-RULES C-2 a hit on an OPEN boss 24-34 steps = 0.20-0.28 s; punches: the finisher only from the 2nd core punch on, so P3 is two charges: round 1 measured P3 16 s with one): 16 m/s along the lane (23 m in 1.44 s), 25 damage + a 10 m/s knock; it cracks open for 3.0 s (G7: >= 2.5 s); a melee hit within 3.5 m + the cage takes 22 % of the bar (G7: 20-25 %) and launches the player back at 30 deg: launchDist 16 is the flat-ground target (round 1 measured at 15: the landings 10.1-16.5 m, median 13.9, over 80 punches: the fall gravity and the cap wall shorten it; G7: 12-18 m); finishAt 15 % (design.n37's dive.finishAt); the lane's ends 1.5 m in from the cap wall." },
  lines: {
    _why: "G5: THE WICK's fight lines (round 4: F's docs/COPY47.md fixes: sentence case, the glyph shown; W-cards.md section 1's table; NIGHT47.md section 3's copy rules: second person, verb first, at most 6 words, {glyph} tokens for the bound buttons), shown once each per fight through BossLine40 (the lesson widget; F's hud47 sizes them: 44 px), held 2.0 s + 0.3 s a word (secondsBase / secondsPerWord).",
    reveal: "{punch} the yellow embers back.",
    crack: "{fire} through the crack.",
    phase2: "It's climbing. Get up there.",
    ride: "Hold {jump} to ride. Look up.",
    net: "Caught. Keep climbing.",
    phase3: "No way down. Finish it.",
    open: "",
    open_why: "Empty: F's hud47 posts \"{punch} the core!\" itself on the first crack-open (docs/COPY47.md; no double line).",
    secondsBase: 2.0,
    secondsPerWord: 0.3,
  },
  attacks: {
    _why: "The new attacks (Boss37Attack.From's keys); the DIVE leaves the kit (the CHARGE takes its place), the SLAM stays in P1 only (Wick37.N47.cs StartFight47).",
    geyser: atk({ id: "wickGeyser", kind: "red", windup: 1.0, active: 0.2, recover: 1.5, cost: 2, weight: 1.2, legs: [1, 3] }, "RED, 1.0 s tell (the vents glow and hiss; NIGHT47.md section 3: 1-1.5 s), the recover = the sink window."),
    curtain: atk({ id: "wickCurtain", kind: "yellow", windup: 1.0, active: 0.3, recover: 0.6, cost: 2, weight: 0.5, legs: [2], habit: "ground" }, "YELLOW (parry it), 1.0 s of glow round the climber standing on a ledge or the floor (habit ground), P2 (round 2: habit any, weight 0.6: 203 curtains in 40 sloppy fights, P2 median 59.6 s, p90 194.7 OUT; weight 1.0 it fired 210 times in 40 sloppy fights and P2 took a median 55.7 s, p90 182.7 OUT; habit any; with the camp habit it fired 0 times in 40 seeds: a climber never stands 3 s on a ledge)."),
    ring: atk({ id: "wickRing", kind: "red", windup: 1.0, active: 0.6, recover: 1.0, cost: 2, weight: 1.2, legs: [3] }, "RED, 1.0 s (the plates lift and spin up), P3."),
    charge: atk({ id: "wickCharge", kind: "red", windup: 1.2, active: 1.45, recover: 3.0, cost: 3, weight: 1.8, legs: [3] }, "RED, 1.2 s (back to the rim, the lane lit), the run 1.45 s, the recover = the crack-open (3.0 s), P3's main opening."),
  },
};

NUMBERS.wickIntro47 = {
  line: "The Wick. The furnace wakes.",
  line_why: "<= 6 words (5): the name, then what it is; sentence case (F's docs/COPY47.md: copy47 drops forced capitals for 3+ word lines).",
  seconds: 2.5,
  seconds_why: "s held (2.0 s + 0.3 s a word would be 3.5: the reveal is 2 s; the hint follows at once).",
};

const dry = process.argv.includes("--dry");
const rows = process.argv.slice(2).filter((a) => !a.startsWith("--"));
if (rows.length === 0) throw new Error("name a row: " + Object.keys(NUMBERS).join(", "));
for (const r of rows) if (!NUMBERS[r]) throw new Error("no numbers for " + r);

function splice(raw, row, nums) {
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const n47 = raw.indexOf('"n47": {');
  if (n47 < 0) throw new Error("design.n47 not found");
  const key = raw.indexOf('"' + row + '": {', n47);
  if (key < 0) throw new Error(row + " not found under n47");
  const numKey = raw.indexOf('"numbers": {', key);
  const nextRow = raw.indexOf('": {' + eol, key + row.length + 5);
  if (numKey < 0) throw new Error(row + ": no numbers key");
  // the numbers object: from its "{" to the matching "}"
  let i = raw.indexOf("{", numKey), depth = 0, end = -1;
  for (let j = i; j < raw.length; j++) {
    const ch = raw[j];
    if (ch === '"') { j++; while (raw[j] !== '"') { if (raw[j] === "\\") j++; j++; } continue; }
    if (ch === "{") depth++;
    else if (ch === "}") { depth--; if (depth === 0) { end = j; break; } }
  }
  if (end < 0) throw new Error(row + ": unbalanced numbers");
  const lineStart = raw.lastIndexOf(eol, numKey) + eol.length;
  const indent = raw.slice(lineStart, numKey);
  const body = JSON.stringify(nums, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
  return raw.slice(0, i) + body + raw.slice(end + 1);
}

const outs = [];
for (const rel of COPIES) {
  const p = path.join(ROOT, rel);
  const raw = fs.readFileSync(p, "utf8");
  const before = JSON.parse(raw);
  let text = raw;
  for (const r of rows) text = splice(text, r, NUMBERS[r]);
  const after = JSON.parse(text);
  for (const r of rows) {
    if (JSON.stringify(after.design.n47[r].numbers) !== JSON.stringify(NUMBERS[r])) throw new Error(rel + ": " + r + " numbers did not round-trip");
    before.design.n47[r].numbers = NUMBERS[r];
  }
  if (JSON.stringify(before) !== JSON.stringify(after)) throw new Error(rel + ": something besides the named rows' numbers changed");
  outs.push([p, text]);
  console.log(`${rel}: ${rows.join(", ")} numbers written (${Object.keys(NUMBERS[rows[0]]).filter((k) => !k.endsWith("_why")).length}+ keys)`);
}
if (outs[0][1] !== outs[1][1]) throw new Error("the two copies differ");
if (!dry) for (const [p, t] of outs) fs.writeFileSync(p, t);
console.log(dry ? "dry run: nothing written" : "written; the two copies byte-identical");

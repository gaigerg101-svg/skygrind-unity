// NIGHT34 builder M: writes builder M's numbers into design.n34.reach34 and design.n34.flow34 of Design/unity-config.json (this repo or worktree:
// the path is relative to this script). Only the design.n34 block (the file's last block) is re-serialized; everything before it is kept byte for byte.
// The rows' `enabled` stays as it is (false: the lead turns rows on). Re-runnable: it sets the same fields each time.
// Run: node Tools/night/n34m-config.cjs   then: cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && cmp both
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const anchor = eol + "    \"n34\": {" + eol;
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("design.n34 anchor not found exactly once");
const tail = eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const n34 = d.design.n34;
if (!n34 || !n34.reach34 || !n34.flow34) throw new Error("design.n34.reach34 / flow34 missing (the shared commit e7bead72 adds them)");

// ---- reach34 -------------------------------------------------------------------------------------------------------------------------------
const r = n34.reach34;
r.bossMargin = 0.6;
r.bossMargin_why =
  "Metres added to a BOSS body's radius (its capsule's axis kept) for the Pulse Gauntlet only: the click swing's sweep, its parry peek, the F punch, the LT blast (NIGHT27 pulse27) and the NIGHT25 release launch, and the KILN WYRM head-punch stagger's reach (design.n30.wyrm30.headReachMargin's test). Start 0.6 m from NIGHT34.md M1 (GG 2026-10-11: 'a slightly larger active hitbox when striking bosses like the worm'); method: inflate the enemy's hurtbox, keep the player's small, sphere / capsule test (RESEARCH.md finding 7, research/swap-hud.md section 6; no ULTRAKILL melee numbers were found). The web has no boss margin: its gauntlet hit a boss on the body's own radius. Bosses: every IBossBody27 (Lantern, Stag, Loom, Crown, Glacier 27 / 28), the Wyrm, the Glacier 26, the Hulk, the Monolith, Rival Prime and the Bulwark (the blast only: the Bulwark has no punch targets).";

// ---- flow34 --------------------------------------------------------------------------------------------------------------------------------
const f = n34.flow34;
f.found_why =
  "Grepped first (the mechanism, not the name; Core/Flow34.cs's header): table M 2, no ground friction on a buffered landing jump, ALREADY EXISTS in the web's mover (the buffered jump fires in TryJump before StepGround on the first grounded step, and StepGround skips friction while groundedSteps == 0, SPEC 3.2): not rebuilt, measured by the harness `flow34 bhop`. Coyote and buffer on slides, the buffer onto a wall ride, apex hang (jump.apexHangMult) and corner correction (collision.cornerCorrect) exist too. Each sub-switch below counts only while this row is enabled.";
f.wallCoyote = {
  _why:
    "RESEARCH.md table M 1 (research/movement-oss.md pattern 1: 'apply coyote after leaving a wall too'; Celeste JumpGraceTime 0.1 s, dot-player-controller coyote_time 0.1 s, the TF recreation's k_coyoteTime 0.15 s 'ground or wall'): a wall ride that ends on its own (away, timeout, corner, lost, blocked) keeps the wall jump off that wall for `time` s, even past the 0.12 m probe. The web has no wall coyote: its coyote (jump.coyoteTime) starts only when walking or sliding off a floor edge.",
  enabled: true,
  time: 0.1,
  time_why: "0.1 s (12 steps at 120 Hz), the same as the web's floor coyote (jump.coyoteTime 0.1) and Celeste's / dot's 0.1 s.",
};
f.wallBuffer = {
  _why:
    "RESEARCH.md table M 1 (research/movement-oss.md pattern 1: 'let a buffered jump fire on wall contact as a wall jump'): in the air with a double jump left, a press while a valid wall is predicted within the jump buffer (jump.bufferTime, along the horizontal velocity) waits for the wall instead of spending the double jump; on the buffer's last step it is the double jump after all. The web holds a press only for a predicted FLOOR landing (landingSoon); it has no wall prediction. No new number: the window is the web's jump.bufferTime, the facing test the web's wallRide.awayDot.",
  enabled: true,
};
f.zipCarry = {
  _why:
    "RESEARCH.md table M 7 (research/movement-oss.md pattern 7: 'add, never replace, velocity on release so grapple chains into slide and bhop'; ULTRAKILL / Titanfall 2 keep momentum, DESIGN31 R2): the ZIP pull (the NIGHT31 tap grapple) replaces the velocity with the pull every substep; its release adds back the pre-pull horizontal velocity's part across the release direction, x carryMult. The web's zip (an enemy hook only) has no carry: the release keeps the pull's velocity alone.",
  enabled: true,
  carryMult: 1,
  carryMult_why: "1 = all of the erased sideways momentum comes back (the research's 'add, never replace'); the pull's own speed along its line is unchanged. NIGHT31's arriveKeepMax (design.n31.grapplePull) still clamps the horizontal speed after an ARRIVAL; a cancel (jump, dash) keeps it all.",
};
// leftover 5 (techniques 4 and 5; technique 6, corner correction and apex hang, exists: collision.cornerCorrect, jump.apexHangMult)
f.ridePhase = {
  _why:
    "RESEARCH.md table M 4 (research/movement-oss.md pattern 4: wallrun gravity 'starts at zero and ramps back'; the UCSD CSE125 Titanfall-style student recreation's grip 0 g for 0.5 s then a ramp to full over 1.0 s, no licence: its numbers are tuning hints only, no code): the wall ride's gravity is 0 for gripTime s (only while not rising, so the grip never climbs a wall), then ramps linearly back to the web's wallRide.gravityMult (0.15) over rampTime s. The web's ride has one constant multiplier from its first step.",
  enabled: true,
  gripTime: 0.35,
  gripTime_why: "The recreation's 0.5 s grip scaled by SKYGRIND's ride length against its ride (wallRide.maxTime 1.2 s vs its 1.75 s: x 0.686 = 0.34 s), rounded to 0.35 s (42 steps).",
  rampTime: 0.7,
  rampTime_why: "The recreation's 1.0 s ramp scaled the same way (0.69 s), 0.7 s (84 steps): grip + ramp = 1.05 s, inside the 1.2 s ride.",
};
f.lurch = {
  _why:
    "RESEARCH.md table M 6 (research/movement-oss.md pattern 6, Titanfall 2's lurch as an idea; the TF recreation: max at 0.2 s, off at 0.5 s, capped, no licence: tuning hints only): ONE steering correction per takeoff (a ground or slide jump): within `window` s of the jump, the first air step whose wish is at least minAngleDeg off the motion turns the horizontal velocity toward it by at most maxTurnDeg, its speed x speedKeep (a lurch costs speed, so it is not free air control). A landing ends it. The web has only its constant air acceleration.",
  enabled: true,
  window: 0.3,
  window_why: "Between the recreation's full-strength 0.2 s and its 0.5 s cut-off: 0.3 s (36 steps) after the jump.",
  minAngleDeg: 45,
  minAngleDeg_why: "A change of mind, not a strafe: 45 degrees off the motion before the lurch spends itself (tuning; no source number).",
  maxTurnDeg: 60,
  maxTurnDeg_why: "At most 60 degrees of turn in the one step (a full reversal stays impossible; tuning, no source number: the recreation caps speed at 400 u/s instead).",
  speedKeep: 0.85,
  speedKeep_why: "85 % of the horizontal speed kept through the turn (tuning; the cost keeps bhop speed from being steered for free).",
};
// the REA addendum (Research/2026-10-11-wave34/research/rea-ultrakill.md, Movement: ULTRAKILL's NewMovement.cs read with ILSpy, numbers and
// rule shapes only, no code)
f.wallJumps34_why =
  "NOT a sub-switch: ULTRAKILL's 3 wall jumps per airtime reset on landing (currentWallJumps < 3, NewMovement.cs :1038 / :1287) ALREADY is the web's rule: PortExport/config.json wallJump.maxPerAirtime is 3 (Mover.MaxWallJumps = maxPerAirtime + the GRIP perk's bonus; Land() refills it). The addendum's 'movement.wallJumps 1' does not exist in the config. Measured by the harness `flow34 wallJumps`: 3 wall jumps chained between two parallel walls, the row off and on.";
f.slideBounce34 = {
  _why:
    "rea-ultrakill.md, Movement (ULTRAKILL NewMovement.cs WallJump :1754-1763, read from GG's installed copy with ILSpy; the rule's shape and its numbers only): while sliding, or within startSteps of a slide start, a jump at a wall the slide heads into is a WALL jump that REFLECTS the slide direction off the wall normal, adds push x the normal before renormalising, keeps the full horizontal speed and at least the wall jump's up speed (jump v0 x wallJump.v0Mult; ULTRAKILL keeps at least 15 up in its own units). A normal wall jump keeps the web's rule. The web has no slide-into-wall rule: on the ground a slide's jump is the slide jump.",
  enabled: true,
  push: 0.35,
  push_why: "ULTRAKILL's 0.35 of the unit wall normal added to the reflected direction before renormalising (NewMovement.cs :1754-1763).",
  startSteps: 4,
  startSteps_why: "ULTRAKILL's 'within 4 frames of a slide start' (its physics frames), read as 4 sim steps at 120 Hz (33 ms).",
};

const block = JSON.stringify(n34, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, at);
fs.writeFileSync(file, head + eol + "    \"n34\": " + block + tail);
const back = JSON.parse(fs.readFileSync(file, "utf8"));
if (back.design.n34.reach34.bossMargin !== 0.6) throw new Error("write check failed");
if (raw.slice(0, at) !== fs.readFileSync(file, "utf8").slice(0, at)) throw new Error("the head changed");
console.log("design.n34.reach34: bossMargin " + back.design.n34.reach34.bossMargin + ", enabled " + back.design.n34.reach34.enabled + "; flow34 enabled " + back.design.n34.flow34.enabled);

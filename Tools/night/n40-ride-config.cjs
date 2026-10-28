// NIGHT40 builder R2: writes design.n40.ride40's numbers (and their _why) into Design/unity-config.json, replacing ONLY the ride40 row's text
// (L-049: the other rows stay byte for byte), then copies the file to Assets/StreamingAssets/Design/ (the Player's copy, byte-identical).
// Run from the worktree root: node Tools/night/n40-ride-config.cjs [--on]   (--on writes enabled true; default false). Re-runnable.
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const on = process.argv.includes("--on");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const ind = "      ";
const start = raw.indexOf(ind + '"ride40": {' + eol);
if (start < 0) throw new Error("design.n40.ride40 not found");
const endMark = eol + ind + "},";
const end = raw.indexOf(endMark, start);
if (end < 0) throw new Error("the ride40 row's end not found");
const row = {
  _why: "Builder R2 (NIGHT40.md R2; GG 2026-10-13: 'Infinite wall-riding should also be implemented so players don't slowly slide down after a few seconds; the wall-ride must feel crisp, smooth, and reliable'; 'Wall-riding currently triggers too easily when climbing random platforms or stairs'; DECIDE40 D-9): EVERY ride holds (ride38's hold rule for a tap ride as for a hold ride: no sink, no 1.2 s timer, the look pitch past +-deadzoneDeg drifts it up / down, never past the wall's top); a ride ends only by the player (jump, crouch, the stick away), the wall's end or a landing; a 60 s guard against a stuck state. These numbers override design.n38.ride38's while the row is on (Core/Ride40.cs Hold; the n38 row is untouched). Core/Ride40.cs; Mover.cs branches marked NIGHT40 ride40, inside the NIGHT35 wallride35 branches only. Off: NIGHT39's ride bit for bit. Changes VERBS rows 9 / 11 on purpose. The harness: n40ride measure / walk.",
  enabled: on,
  enabled_why: on ? "On (the lead, after the Player frames)." : "Off until the lead has looked at its Player frames once and iterated once (NIGHT40.md method 2).",
  holdSink: 0,
  holdSink_why: "m/s: a ride inside the pitch dead zone sinks at this speed. 0 = the height is kept exactly (GG: 'so players don't slowly slide down'). Same value as design.n38.ride38.holdSink, now for every ride (a tap ride sank 1.5 m/s after wallgrip36's 0.35 s grip and ended at 1.2 s).",
  guard: 60,
  guard_why: "Seconds: a ride ends on its own only after this long (the event's reason 'safety'), a guard against a stuck state, never a meter: the MIT Godot controller's infinite_wallrun_time ends only on wall loss, floor contact or releasing forward (Research/2026-10-14-wave40/research/feel-web.md section 2). Same value as design.n38.ride38.guard.",
  deadzoneDeg: 15,
  deadzoneDeg_why: "Degrees of look pitch (+ up) within which a ride holds its height, so aiming at enemies near the horizon does not move it. design.n38.ride38's value (NIGHT38 PLAN.md section 1).",
  fullDeg: 45,
  fullDeg_why: "Degrees of look pitch at which the drift reaches driftMax (linear from deadzoneDeg, clamped past it). design.n38.ride38's value (= design.n35.wallride35.pitchHighDeg, so one look reads both).",
  driftMax: 2.5,
  driftMax_why: "m/s: the most a ride rises or sinks from the look pitch. design.n38.ride38's value (2.5 m/s climbs a 5 m wall in 2 s, about a quarter of the ride's 9.2 m/s floor along the wall with ride39).",
  topProbe: 0.25,
  topProbe_why: "m past the capsule radius: a rise is cut when a ray toward the ridden wall from the capsule's upper contact sample (the feet + 1.45 m) moved up by this step's rise finds no wall this close (never past the wall's top). design.n38.ride38's value; an engine reach, not a feel number.",
  minClearance: 1.0,
  minClearance_why: "THE ATTACH ASKS FOR INTENT (round 2; a hypothesis): no ride starts while the ground below the feet is closer than 1.0 m (a ray down from the feet). A jump at a ride wall from the floor still rides: it attaches once the jump has carried the feet 1.0 m up (jump.height 1.5). Source: the CSE125 'Titanfall Movement System' enter rule, ground clearance >= 50 Quake units (about 1.3 m at 1 u = 1 inch; 1.0 is NIGHT40.md R2's lower start so a floor jump still rides), and the common Unity tutorial's AboveGround() ray (Research/2026-10-14-wave40/research/feel-web.md section 1). Measured before (n40ride walk, levels 1-5, 3 seeds): 432 of 606 ride starts had ground within 1.0 m.",
  minSpeed: 4.0,
  minSpeed_why: "m/s of horizontal speed (before the wall clipped it) for a ride to start: slowly climbing onto a platform never rides. TMS's enter rule: >= 200 u/s (about 5 m/s at 1 u = 1 inch); 4 = the web's wallRide.entrySpeed for a timed ride (feel-web.md section 1). A hypothesis.",
  inputDot: 0.1,
  inputDot_why: "The stick must point toward the wall: dot(move direction, -wall normal) >= 0.1 (about 6 deg into it; a stick held along a wall at any angle into it counts, a neutral stick or one along or away from the wall does not). TMS's intent rule dot(wishDir, -wallNormal) >= 0.1 (feel-web.md section 1). A hypothesis.",
  minWallHeight: 2.0,
  minWallHeight_why: "m: the wall's face must still be there 2.0 m above the feet (a ray toward the wall from the body's axis at that height): stair risers, platform faces and rails are shorter, so they never ride. No source has a minimum wall height (feel-web.md: 'not found'); ours, NIGHT40.md R2's hypothesis. Measured before: 122 of 606 ride starts on a face under 2.0 m above the feet.",
  attachReach: 0.5,
  attachReach_why: "m past the capsule radius: wallride35's near-miss draw (jump held beside a wall pulls onto it), KEPT at wallride35's 0.5. NIGHT40.md R2's hypothesis was 0.25 (GG w29: 'almost like you snap on to it'); measured (n40ride n38f foundry --probe reach=..., 3 seeds): THE GANTRY's chain clears 3/3 at 0.5 and 0/3 at 0.4, 0.35 and 0.25 (its 8 m apart walls are caught by the draw after each jump-off). The draw now asks for the same intent as a contact (the stick toward the wall, 1.0 m of clearance, a 2.0 m face, 4 m/s), which is what stops the snap onto risers and platform faces.",
  lockout: 0.2,
  lockout_why: "Seconds: after a ride ends in the air, its wall (or one facing within wallride35's reRideDeg) starts no ride for 0.2 s: no re-snap after a jump-off or a drop. TMS's 0.2 s 'exiting wall' flag; the MIT Godot controller's time_bef_can_wallrun_again 0.2 s (feel-web.md section 1).",
};
const json = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : ind + l)).join(eol);
const out = raw.slice(0, start) + ind + '"ride40": ' + json + raw.slice(end + endMark.length - 1);
JSON.parse(out);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
const d = JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n40.ride40 written (enabled " + d.design.n40.ride40.enabled + "): " + Object.keys(d.design.n40.ride40).filter((k) => !k.endsWith("_why")).join(", "));

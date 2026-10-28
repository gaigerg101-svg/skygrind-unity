// NIGHT38 builder M: writes design.n38.ride38's numbers (and their _why) into Design/unity-config.json, replacing ONLY the ride38 row's text
// (L-049: the other rows stay byte for byte), then copies the file to Assets/StreamingAssets/Design/ (the Player's copy, byte-identical).
// Run from the worktree root: node Tools/night/n38-ride-config.cjs [--on]   (--on writes enabled true; default false). Re-runnable.
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const on = process.argv.includes("--on");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const ind = "      ";
const start = raw.indexOf(ind + '"ride38": {' + eol);
if (start < 0) throw new Error("design.n38.ride38 not found");
const endMark = eol + ind + "},";
const end = raw.indexOf(endMark, start);
if (end < 0) throw new Error("the ride38 row's end not found");
const row = {
  _why: "Builder M (NIGHT38 PLAN.md section 1, decision D-3; NIGHT38.md M1; GG 2026-10-13 ~00:10: wall rides 'you don't actually fall down you just stay on forever ... you can choose ... where you're aiming up and down'): a HELD wall ride (jump held: design.n35.wallride35's Lucio ride) keeps its height (no sink, a 60 s stuck guard instead of the 8 s safety cap); the look pitch beyond +-15 deg drifts the ride up or down at up to 2.5 m/s at +-45 deg, never past the wall's top (the capsule's upper contact sample stays on the wall). The timed (tap) ride keeps the 1.2 s rule; every end stays (release = the jump-off, crouch, away, a corner, the wall ending, a landing). Core/Ride38.cs; Mover.cs branches marked NIGHT38 ride38, inside the NIGHT35 wallride35 branches only. Off: NIGHT37's ride bit for bit (golden 0 mismatches). Changes VERBS rows 9 / 11 on purpose. The harness: n38ride hold.",
  enabled: on,
  enabled_why: on ? "On (builder M): n38ride hold PASS (the harness rows in Tools/SimHarness/Harness.N38.Ride.cs)." : "Off until the lead turns it on (n38ride hold PASS with the row forced on).",
  holdSink: 0,
  holdSink_why: "m/s: a hold ride inside the pitch dead zone sinks at this speed (PLAN.md section 1: 'wallride35.sinkSpeed 0.1 -> 0'). 0 = the height is kept exactly (GG: 'stay on forever'). design.n35.wallride35.sinkSpeed (0.1) is not used by a hold ride while ride38 is on.",
  guard: 60,
  guard_why: "Seconds: a hold ride ends on its own only after this long (the event's reason 'safety'), a guard against a stuck state, never a meter (PLAN.md section 1: 'a long cap, 60 s, only against stuck states'). Replaces design.n35.wallride35.safetyCap (8 s) on a hold ride.",
  deadzoneDeg: 15,
  deadzoneDeg_why: "Degrees of look pitch (+ up) within which a hold ride holds its height (PLAN.md section 1: 'pitch above +15 deg rises, below -15 deg sinks'), so aiming at enemies near the horizon does not move the ride.",
  fullDeg: 45,
  fullDeg_why: "Degrees of look pitch at which the drift reaches driftMax (PLAN.md section 1: 'up to ~2.5 m/s at +-45 deg'); linear from deadzoneDeg, clamped past it. 45 = design.n35.wallride35.pitchHighDeg (the jump-off's full up speed), so one look reads both.",
  driftMax: 2.5,
  driftMax_why: "m/s: the most a hold ride rises or sinks from the look pitch (PLAN.md section 1; ours: beyond ULTRAKILL, whose cling always slides, Research/2026-10-13-wave38/research/uk-hud-feel.md). 2.5 m/s climbs a 5 m wall in 2 s, about a quarter of the ride's 10.4 m/s floor along the wall.",
  topProbe: 0.25,
  topProbe_why: "m past the capsule radius: a rise is cut when a ray toward the ridden wall from the capsule's upper contact sample (the feet + body.capsuleHeight - body.capsuleRadius: 1.45 m) moved up by this step's rise finds no wall this close. The ride's stick (design.n35.wallride35.stick) keeps the capsule on the wall, so the wall sits one radius away; 0.25 is about twice wallRide.probeDistance (0.12) for a faceted wall. An engine reach, not a feel number.",
};
const json = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : ind + l)).join(eol);
const out = raw.slice(0, start) + ind + '"ride38": ' + json + raw.slice(end + endMark.length - 1);
JSON.parse(out);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
const d = JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n38.ride38 written (enabled " + d.design.n38.ride38.enabled + "): " + Object.keys(d.design.n38.ride38).filter((k) => !k.endsWith("_why")).join(", "));

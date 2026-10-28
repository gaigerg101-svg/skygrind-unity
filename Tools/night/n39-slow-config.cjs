// NIGHT39 builder F1: writes design.n39.slow39's numbers (and their _why) into Design/unity-config.json, replacing ONLY the slow39 row's text
// (L-049: the other rows stay byte for byte), then copies the file to Assets/StreamingAssets/Design/ (the Player's copy, byte-identical).
// Run from the worktree root: node Tools/night/n39-slow-config.cjs [--on]   (--on writes enabled true; default false). Re-runnable.
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const on = process.argv.includes("--on");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const ind = "      ";
const start = raw.indexOf(ind + '"slow39": {' + eol);
if (start < 0) throw new Error("design.n39.slow39 not found");
const endMark = eol + ind + "}";
const end = raw.indexOf(endMark, start);
if (end < 0) throw new Error("the slow39 row's end not found");
const row = {
  _why: "Builder F1 (NIGHT39.md F1; GG 2026-10-13: 'when you're wall riding and zooming in ... it needs to be extremely slow every time'; 'if you grapple onto an enemy the game should slow down with the same effect'; the gauntlet's parry 'emphasized more and slowed down'). Loop pacing (SurvivalGame.SlowAim27.cs -> Slow39 in SurvivalSim.N39.Slow.cs): the sim still steps at 120 Hz, the tape and the fingerprint never see it, Time.timeScale untouched, aim reads the real frame delta (Max Payne's rule: research/feel-web.md section 1). No N39Blocks bit; off (the row, --off39 / -skygrindOff39): slowAim27's NIGHT38 behaviour (design.n27.slowAim27, scale 0.75). Gated by SLOW-MO (REDUCED MOTION turns it off) as slowAim27. No meter and no drain (Severed Steel's meter does not drain while wall running). Harness: n39slow.",
  enabled: on,
  enabled_why: on ? "On (the lead, after the Player frames)." : "Off until the lead has looked at the Player frames (NIGHT39.md method 2); n39slow forces it on.",
  scale: 0.3,
  scale_why: "World time share at full slow while riding a wall AND zoomed (and while latched on an enemy). No shipped game's time scale was found (research/feel-web.md section 1: not found for Ghostrunner, Max Payne, Mirror's Edge, Severed Steel), so a Unity-only design number in the card's 0.25-0.35 range: 0.3 = a 3.3x slow-down ('extremely slow', against design.n27.slowAim27.scale 0.75, which GG felt as 'removed'). Tune on Player frames.",
  easeInS: 0.15,
  easeInS_why: "Real seconds for the slow's depth to go 0 -> 1 (linear depth, smoothstep share: ease in and out, no snap). slowAim27's 0.13 s, a little longer for the deeper drop. Not found in any source (research/feel-web.md: 'ramp-in / ramp-out times ... not found').",
  easeOutS: 0.25,
  easeOutS_why: "Real seconds back to real time (smoothstep). Longer than the way in so letting go of the zoom or leaving the wall reads as a release, not a jolt; short enough that a chain's air time between walls is mostly at speed.",
  latch: true,
  latch_why: "The whip latch (VERBS 20): the same slow while the hook is bitten into an enemy (the rope Attached on an enemy zip: SurvivalSim.Slow39Latched). A yank (a light enemy pulled to you, no rope) is not a latch.",
  parry: true,
  parry_why: "A player parry ('parry' or 'parry36', owner player) opens a slow window AFTER the parry's freeze (design.ultra.parryHitstop 0.25 s, ULTRAKILL's figure, unchanged): the emphasis is the slow after the freeze, not a longer freeze (NIGHT39.md F1).",
  parryScale: 0.25,
  parryScale_why: "World time share in the parry's window: deeper than the ride's 0.3 so the returned projectile's flight reads (Parry31View's streak runs on world time). Ours (ULTRAKILL documents no slow after its freeze: research/feel-web.md section 2).",
  parryHoldS: 0.3,
  parryHoldS_why: "Real seconds the parry window holds parryScale after the freeze: with the 0.25 s freeze about 0.55 s at under a quarter speed before the ease. Ours (no source gives a post-freeze slow); about the fighting games' heavy hitstop (16 frames, 267 ms: research/feel-web.md section 2) again, so it reads as one longer beat without stalling the fight.",
  parryOutS: 0.35,
  parryOutS_why: "Real seconds the parry window eases back to real time (smoothstep).",
};
const json = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : ind + l)).join(eol);
const out = raw.slice(0, start) + ind + '"slow39": ' + json + raw.slice(end + endMark.length);
JSON.parse(out);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
const d = JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n39.slow39 written (enabled " + d.design.n39.slow39.enabled + "): " + Object.keys(d.design.n39.slow39).filter((k) => !k.endsWith("_why")).join(", "));

// NIGHT39 builder R1: writes design.n39.ride39 (R1's own row, L-049) in Design/unity-config.json, then copies the file to
// Assets/StreamingAssets/Design/ (the two stay byte-identical). Only the ride39 row's text changes; rerunning it rewrites the same row.
// Run from the worktree root: node Tools/night/n39-ride39-config.cjs [--on | --off] [--look-on | --look-off]
//   (defaults: keep the row's current `enabled` and `noRideLook`)
const fs = require("fs");
const file = "Design/unity-config.json";
const copy = "Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const old = d.design.n39 && d.design.n39.ride39;
if (!old) throw new Error("design.n39.ride39 missing (the lead's shared commit adds it)");
const on = process.argv.includes("--on") ? true : process.argv.includes("--off") ? false : old.enabled;
const look = process.argv.includes("--look-on") ? true : process.argv.includes("--look-off") ? false : old.noRideLook === true;
const row = {
  _why: old._why,
  enabled: on,
  enabled_why: "On only when THE GANTRY's two gates pass builder E's checker and the walk clears level 1's ride rooms with the row on (NIGHT39.md R1).",
  speedBuff: 0.15,
  speedBuff_why: "The ride's speed floor is wallRide.minSpeed (8) x (1 + 0.15) = 9.2 m/s (wallride35: 0.3, 10.4 m/s). GG 2026-10-13: 'you gain speed too quickly'. NIGHT39.md R1's lever (~0.15); Lucio's own buff (30 %, lucio.md) is not the target any more: the feel is GG's. Measured: n39ride chain (the first wall's ride speed 10.4 -> 9.2 m/s).",
  exitBoost: 1,
  exitBoost_why: "Metres per second added at each jump-off (wallride35: 2). Half the gain per wall (NIGHT39.md R1's lever ~1). Blizzard published no magnitude (lucio.md); ours, measured in n39ride chain.",
  chainCap: 16,
  chainCap_why: "The chain's top speed (wallride35: 20). With capJumpOff the cap holds the whole jump-off speed, so a chain climbs to 16 m/s and stays there (before: 10.4 -> 15.2 -> 19.1 -> 20.7 -> 22.2 -> 23.6 m/s on six walls, n39ride chain). 16 is NIGHT39.md R1's lever, under the slide jump's 24 so a slide-hop stays the fastest takeoff.",
  capJumpOff: true,
  capJumpOff_why: "Measured (n39ride chain, NIGHT38's numbers): wallride35's chainCap never held a chain, because a jump-off pushes outSpeed 8 m/s straight out from the wall and the next wall's ride start keeps the whole horizontal speed (the web's SPEC 1.3: 'no loss from the approach angle'), so every wall adds about hypot(along, 8) - along on top of the exit boost (24.9 m/s off the sixth wall with a cap of 20). With this on, the jump-off's whole horizontal speed is held to max(chainCap, the ride's own speed): a ride already faster than the cap (a slide-hop entry) keeps its speed, a chain never gains past the cap (Ride35.Boost39, Mover.cs's wallride35 jump-off).",
  upLow: 4.6,
  upLow_why: "The jump-off's up speed looking down (wallride35: 4.12, the wiki's Lucio). GG 2026-10-13: 'the jump height should be increased slightly'. About +12 %; measured rise in n39ride chain's jump rows. The ground jump (PortExport jumpSpeed 10) is a web number and not changed (NIGHT39.md R1).",
  upHigh: 10,
  upHigh_why: "The jump-off's up speed looking up (wallride35: 9.12). The same ask, about +10 %; level look (pitch 0): (4.6 + 10) / 2 = 7.3 m/s (wallride35: 6.62). Measured rise: n39ride chain's jump rows.",
  noRideLook: look,
  noRideLook_why: "VIEW ONLY (no sim number; independent of `enabled`): NoRide39View draws every campaign box tagged noride (the boxes the Mover refuses, NIGHT38's noride) as a dark plate with vertical fins (UnityTuning.NoRide39.cs), and hides Doors38View's NIGHT38 ribs. GG 2026-10-13: 'a new wall material with a clear visual texture that indicates you cannot wall ride on it'. Off until the lead has looked at frames (NIGHT39.md method 2).",
};
const block = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const key = '      "ride39": {';
const a = raw.indexOf(key);
if (a < 0 || raw.indexOf(key, a + 1) >= 0) throw new Error("ride39 row not found once");
let depth = 0, b = -1;
for (let i = a + key.length - 1; i < raw.length; i++) {
  if (raw[i] === "{") depth++;
  else if (raw[i] === "}") { depth--; if (depth === 0) { b = i + 1; break; } }
}
const out = raw.slice(0, a) + '      "ride39": ' + block + raw.slice(b);
const check = JSON.parse(out);
for (const k of Object.keys(d.design.n39)) if (k !== "ride39" && JSON.stringify(check.design.n39[k]) !== JSON.stringify(d.design.n39[k])) throw new Error("another row changed: " + k);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n39.ride39 written: enabled " + on + ", noRideLook " + look);

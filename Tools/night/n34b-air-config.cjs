// NIGHT34 builder B (leftover 1): adds design.n34.air34 (the air / slide multiplier on points and style, never damage) to Design/unity-config.json,
// enabled false. Run from the repo (or worktree) root: node Tools/night/n34b-air-config.cjs   (refuses to run twice), then cp to
// Assets/StreamingAssets/Design/unity-config.json and cmp.
const fs = require("fs");
const path = require("path");
const file = path.join(process.cwd(), "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (d.design.n34.air34) throw new Error("design.n34.air34 already exists");
const src = "Research/2026-10-11-wave34/research/arenas.md section 6 (ULTRAKILL's movement multiplier, wiki.gg Style: 'rises 2.0x per second up to 3x while sliding or airborne, falls back to 1x in about 0.2 s on landing')";
const web = "The web has no such number: its style meter's state multiplier is a flat style.stateMult (1.2) in the air states, and its points have no movement multiplier.";
d.design.n34.air34 = {
  _why: "Builder B (NIGHT34.md leftover 1, C-13): an air / slide multiplier on POINTS and STYLE only, never damage (Core/Air34.cs, Game/SurvivalSim.Air34.cs). It climbs while airborne or sliding and falls back fast on landing; every style award and every combat points payout (hits, kills, parries) is multiplied by it. While on it replaces the web's flat style.stateMult. Off (--off34): NIGHT33's points and style. Source: " + src + ". " + web,
  enabled: false,
  max: 3,
  max_why: "The multiplier's cap. " + src + ": up to 3x. " + web,
  risePerSecond: 2,
  risePerSecond_why: "Per second of air or slide, added to the multiplier (1x to 3x in 1 s). " + src + ": 2.0x per second. " + web,
  fallSeconds: 0.2,
  fallSeconds_why: "Seconds for a full max multiplier to fall back to 1x once grounded and not sliding (linear, per fixed step). " + src + ": about 0.2 s on landing. " + web,
};
const block = JSON.stringify(d.design.n34, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const start = raw.indexOf("\"n34\": {");
const tail = eol + "  }" + eol + "}";
if (start < 0 || !raw.endsWith(tail)) throw new Error("unexpected config layout");
fs.writeFileSync(file, raw.slice(0, start) + "\"n34\": " + block + tail);
const back = JSON.parse(fs.readFileSync(file, "utf8"));
if (back.design.n34.air34.enabled !== false) throw new Error("air34 must stay off");
console.log("design.n34.air34 added (max, risePerSecond, fallSeconds; enabled false)");

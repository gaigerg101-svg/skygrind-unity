// NIGHT43 builder A1: writes design.n43.burrow43's numbers (each with its _why) into Design/unity-config.json and copies it to
// Assets/StreamingAssets/Design/ (byte-identical). Only the burrow43 row is touched: its lines after "enabled_why" are replaced by the
// numbers below (re-runnable: the old numbers go first). Run from the worktree root: node Tools/night/n43-a1-burrow-row.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "../..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let s = fs.readFileSync(file, "utf8");
if (s !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");

const nums = [
  ["hp", 220, "Hit points: 'enough to take a few shotgun blasts' (G28 card): the THUNDERCLAP's volley is 8 x 8 = 64 (83 with shotgunTune's x1.3), so ~3 volleys out of the window, 2 with a counter (x2) in it; the gauntlet's swing (192) nearly kills one, a countered swing kills it. Port-only: the web has no burrower."],
  ["radius", 0.55, "Its body's radius out of the ground (m): a fat worm column, wider than a husk (0.4) so a surfaced one is an easy target in the 0.5 s window."],
  ["height", 2.2, "Its body's height out of the ground (m): it rears up above a standing player's eye (1.6 m) so the emergence fills the view."],
  ["huntSpeed", 7.5, "Its head's speed under the floor (m/s): above the husk's 6 (it is the room's pressure on a player who stands still) and below the walk speed 8, so a player who keeps moving outruns it and a tell only lands on one who stops or slows."],
  ["digSeconds", 0.6, "The spawn's dig-in (s): it is seen screwing itself into the floor (visible and hittable) before it vanishes, so the player knows a burrower is in the room (encounters-arenas-web.md: introduce, never already hidden)."],
  ["tellSeconds", 0.8, "The tell (s) between the ring under the player and the emergence (G28 card: 0.8 s; research: 0.7-0.9 s): a jump, a dash out of the ring or a step onto a plate in time."],
  ["windowSeconds", 0.5, "The counter window (s) from the emergence (G28 card: ~0.5 s; research: a 0.4-0.6 s exposed apex): every hit in it deals x counterMult, the first staggers it."],
  ["surfacedSeconds", 1.1, "How long it stands out of the ground after the emergence before it dives (s): the window plus 0.6 s of plain damage, so a late shot still lands."],
  ["diveSeconds", 0.35, "Its sink back into the floor (s), still hittable (a parting shot)."],
  ["restSeconds", 1.4, "Buried and still after a dive (s) before it hunts again: the room breathes between attacks (one burrower attacks at most every ~3.3 s with the tell and the surface time)."],
  ["triggerRange", 1.2, "Its head within this of a grounded player's feet (m, horizontal) starts the tell (it locks the spot under the player then)."],
  ["emergeRadius", 1.6, "The emergence hurts a grounded player within this of the locked spot (m): larger than its body (0.55) so standing at the ring's edge is still a hit, small enough that one dash (or two steps at walk speed in the 0.8 s tell) clears it."],
  ["damage", 25, "Damage to the player on an emergence: the charger's 25 (the heavy hit that is telegraphed and dodgeable)."],
  ["launchUp", 11, "The emergence throws a hit player up (m/s): about a jump and a half (jump 8.33): 'emerge aggressively' (G28), and the player lands away from it."],
  ["launchOut", 4, "... and out from the spot (m/s, horizontal), away from its body."],
  ["floorTolerance", 0.35, "The player's feet within this of its floor's height (m) while grounded count as its ground (it hunts and strikes there); airborne or on anything raised more than this (a metal plate, a deck, a step) the player is safe (G28 card: 'airborne or on metal plates you are safe'). K1: a plate 0.4 m or higher is safe ground."],
  ["counterMult", 2, "Damage x this on every hit inside the counter window (G28 card: x2)."],
  ["counterStaggerSeconds", 1.2, "A counter (the first hit in the window) leaves it out this much longer (s), staggered (N43Events.BurrowCounter): the special reaction (G28: 'a timing window with a special reaction')."],
  ["buryDepth", 6, "Buried, its body is this far under its floor (m): beyond every punch, blast and sphere test, so nothing touches it until it surfaces (the trail the view draws is on the floor)."],
  ["maxCount", 6, "The burrowers' pool (bodies at once): level 3's two burrower rooms place 1-3 (DESIGN-RULES: never more than one worm tell on screen before level 4 is the level's call, K1)."],
];

const key = '"burrow43": {';
const at = s.indexOf(key, s.indexOf('"n43": {'));
if (at < 0) throw new Error("design.n43.burrow43 not found");
const why = s.indexOf('"enabled_why": ', at);
const close = s.indexOf("\n      }", why); // the row's closing brace (6-space indent)
if (why < 0 || close < 0) throw new Error("burrow43's enabled_why / end not found");
const lineEnd = s.indexOf("\n", why);
const cr = s[lineEnd - 1] === "\r" ? "\r" : "";
let whyLine = s.slice(why, lineEnd).replace(/\r$/, "");
if (!whyLine.trimEnd().endsWith(",")) whyLine = whyLine + ",";
const ind = "        ";
const body = nums.map(([k, v, w], i) => ind + JSON.stringify(k) + ": " + JSON.stringify(v) + "," + cr + "\n" + ind + JSON.stringify(k + "_why") + ": " + JSON.stringify(w) + (i < nums.length - 1 ? "," : "") + cr).join("\n");
s = s.slice(0, why) + whyLine + cr + "\n" + body + s.slice(close);
const chk = JSON.parse(s).design.n43.burrow43;
for (const [k, v] of nums) if (chk[k] !== v) throw new Error("burrow43." + k + " not written");
if (chk.enabled !== false) throw new Error("burrow43 must stay off (the lead turns rows on)");
fs.writeFileSync(file, s);
fs.copyFileSync(file, copy);
console.log("burrow43: " + nums.length + " numbers written; both copies identical");

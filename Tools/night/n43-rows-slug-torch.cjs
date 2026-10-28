// NIGHT43 lead (round 1): two more sim rows for W1's round 2: slug43 (G20: the charged THUNDERCLAP slug's impact in the sim) and torch43
// (G20: the TORCH's burning state). Adds them to design.n43 (off) in both config copies; N43Blocks gains bits 12 and 13 in the same commit.
// Run once: node Tools/night/n43-rows-slug-torch.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json", copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (raw.includes('"slug43"')) throw new Error("slug43 already present");
const anchor = '      "wick43": {';
const n = raw.split(anchor).length - 1;
if (n !== 1) throw new Error("wick43 anchor found " + n + " times");
const off = "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT43.md method 2).";
const rows = {
  slug43: { _why: "Builder W1 round 2 (G20, GG: 'Charged attacks, especially charged shotgun attacks, should produce more dramatic particles, explosions, enemy reactions'): the charged slug hits like a cannon in the sim: a knockback and a stagger on what it does not kill, a small blast at the impact point. W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  torch43: { _why: "Builder W1 round 2 (G20, GG: 'The flamethrower currently feels weak'; combat-web.md: DOOM Eternal's flame belch works because burning enemies change state): an enemy the TORCH hits ignites for a few seconds (a burn tick, a state the views can show), the TORCH's reach ramps in over ~0.2 s. W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
};
const text = Object.entries(rows).map(([k, v]) => '      "' + k + '": ' + JSON.stringify(v, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol) + "," + eol).join("");
raw = raw.replace(anchor, () => text + anchor);
const d = JSON.parse(raw);
if (!d.design.n43.slug43 || !d.design.n43.torch43) throw new Error("rows missing after the patch");
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("design.n43 += slug43, torch43 (off)");

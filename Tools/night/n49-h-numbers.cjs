// NIGHT49 builder H: H's numbers in its OWN pre-placed row design.n49.charge49 (the shared commit placed it, off, "numbers": {}) in
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json. A text edit of the row's empty "numbers": {}
// (nothing else in the file moves); re-running is a no-op once the row has its numbers. Only the row's acceptance bars live here (the a149
// reel reads them: FeelReel.N49.Charge.cs); the element's sizes and timings are UnityTuning.H49.cs's, its colours UnityTuning.Look49.cs's.
// Run (from the worktree root): node Tools/night/n49-h-numbers.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const NIGHT = "n49";
const NUMBERS = {
  charge49: [
    ["minCentrePx", 18, "px at 1080p: the element's nearest drawn pixel (its dark under-stroke included) must be at least this far from the screen centre: outside the reticle's centre box, whose ticks end 18 px out (NIGHT49.md G1 acceptance and section 1 'Two boxes'; UnityTuning.Hud38BarY's comment). The a149 reel checks the mesh's geometry and the changed pixels against it at 1920x1080 and 1280x720."],
    ["minHeightPx", 4, "px at 1080p: the element's band (the arc's thickness, a pip's height) must be at least this tall (NIGHT49.md G1 acceptance: '>= 4 px at 1080p'); thinner reads as a hairline at 720p (2.7 px). The a149 reel checks it."],
  ],
};

const texts = FILES.map((rel) => fs.readFileSync(path.join(ROOT, rel), "utf8"));
if (texts[0] !== texts[1]) throw new Error("the two config copies differ: refusing to write");
let text = texts[0];
const eol = text.includes("\r\n") ? "\r\n" : "\n";
for (const [row, nums] of Object.entries(NUMBERS)) {
  const head = '      "' + row + '": {' + eol;
  const at = text.indexOf(head);
  if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("expected exactly one " + row + " row");
  const close = text.indexOf(eol + "      }", at);
  const empty = '        "numbers": {}';
  const ne = text.indexOf(empty, at);
  if (ne < 0 || ne > close) { console.log("n49-h-numbers: " + row + " already has numbers"); continue; }
  let body = '        "numbers": {';
  nums.forEach(([k, v, why], i) => {
    body += eol + '          "' + k + '": ' + JSON.stringify(v) + "," + eol + '          "' + k + '_why": ' + JSON.stringify(why) + (i < nums.length - 1 ? "," : "");
  });
  body += eol + "        }";
  text = text.slice(0, ne) + body + text.slice(ne + empty.length);
  console.log("n49-h-numbers: " + row + " numbers " + nums.map((n) => n[0]).join(", "));
}
const cfg = JSON.parse(text);
for (const [row, nums] of Object.entries(NUMBERS)) {
  if (cfg.design[NIGHT][row].enabled !== false) throw new Error(row + " must stay enabled:false (the lead turns rows on)");
  for (const [k, v] of nums) if (cfg.design[NIGHT][row].numbers[k] !== v) throw new Error(row + "." + k + " did not round-trip");
}
for (const rel of FILES) {
  fs.writeFileSync(path.join(ROOT, rel), text);
  console.log("n49-h-numbers: wrote " + rel);
}

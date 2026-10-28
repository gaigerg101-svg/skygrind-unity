// NIGHT48 builder F: F's numbers in its OWN pre-placed rows of design.n48 (blast48, guns48, foes48, lineup48; the shared commit placed them,
// all off, "numbers": {}) in Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json. A text edit of each
// row's empty "numbers": {} (nothing else in the file moves); re-running is a no-op once a row has its numbers. Only gameplay-side choices the
// lead makes live here (the blast's picked variant); render numbers are UnityTuning.F48.cs's.
// Run (from the worktree root): node Tools/night/n48-f-numbers.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const NUMBERS = {
  blast48: [
    ["variant", 0, "the LT blast's look with blast48 on: 0 = A \"ring\" (the floor shockwave leads), 1 = B \"lance\" (a cone of pink streaks along the aim leads, the floor ring at 0.4). The lead sets it from the blind pair \"which LT blast hits harder?\" (NIGHT48.md G20; a748's X48-pair-blast frames); 0 until then (NIGHT47's default, UnityTuning.Fx47BlastVariant)."],
  ],
};

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  if (out === null) {
    for (const [row, nums] of Object.entries(NUMBERS)) {
      const head = '      "' + row + '": {' + eol;
      const at = text.indexOf(head);
      if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("expected exactly one " + row + " row");
      const close = text.indexOf(eol + "      }", at);
      const empty = '        "numbers": {}';
      const ne = text.indexOf(empty, at);
      if (ne < 0 || ne > close) { console.log("n48-f-numbers: " + row + " already has numbers"); continue; }
      let body = '        "numbers": {';
      nums.forEach(([k, v, why], i) => {
        body += eol + '          "' + k + '": ' + JSON.stringify(v) + "," + eol + '          "' + k + '_why": ' + JSON.stringify(why) + (i < nums.length - 1 ? "," : "");
      });
      body += eol + "        }";
      text = text.slice(0, ne) + body + text.slice(ne + empty.length);
      console.log("n48-f-numbers: " + row + " numbers " + nums.map((n) => n[0]).join(", "));
    }
    const cfg = JSON.parse(text);
    for (const [row, nums] of Object.entries(NUMBERS))
      for (const [k, v] of nums) if (cfg.design.n48[row].numbers[k] !== v) throw new Error(row + "." + k + " did not round-trip");
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n48-f-numbers: wrote " + rel);
}

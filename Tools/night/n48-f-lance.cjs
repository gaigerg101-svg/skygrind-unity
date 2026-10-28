// NIGHT48 builder F, round 3: the G20 blind pair's winner (DECIDE48 D-28: the LANCE, B, 2 to 1) into design.n48.blast48.numbers.variant in
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json. A text edit inside the blast48 row only (its
// "variant" value and its _why); re-running is a no-op once it reads 1.
// Run (from the worktree root): node Tools/night/n48-f-lance.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const WHY = "the LT blast's look with blast48 on: 0 = A \"ring\" (the floor shockwave leads), 1 = B \"lance\" (a cone of pink streaks along the aim leads, the floor ring at 0.4). 1: the blind pair \"which LT blast hits harder?\" picked the LANCE 2 to 1 (3 fresh judges on a748's X48-pair-blast frames, n48-r2; DECIDE48 D-28).";

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  if (out === null) {
    const eol = text.includes("\r\n") ? "\r\n" : "\n";
    const head = '      "blast48": {' + eol;
    const at = text.indexOf(head);
    if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("expected exactly one blast48 row");
    const close = text.indexOf(eol + "      }", at);
    const row = text.slice(at, close);
    let next = row.replace(/"variant": \d+,/, '"variant": 1,').replace(/"variant_why": "(?:[^"\\]|\\.)*"/, '"variant_why": ' + JSON.stringify(WHY));
    if (next === row) console.log("n48-f-lance: blast48 already reads the lance");
    text = text.slice(0, at) + next + text.slice(close);
    const cfg = JSON.parse(text);
    if (cfg.design.n48.blast48.numbers.variant !== 1) throw new Error("variant did not round-trip");
    if (cfg.design.n48.blast48.enabled !== false) throw new Error("blast48's enabled moved");
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n48-f-lance: wrote " + rel);
}

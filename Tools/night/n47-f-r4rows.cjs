// NIGHT47 builder F, R4 (the beyond round, G9 / G44 / G45): add F's two R4 view rows to design.n47 in Design/unity-config.json and its byte copy
// Assets/StreamingAssets/Design/unity-config.json, OFF, right after F's last round-1 row (lava47). A text insert (nothing else in the file moves);
// re-running is a no-op once the rows are in. Their numbers are Unity-only render numbers (UnityTuning.F47.cs): the rows carry none.
// Run (from the repo or worktree root): node Tools/night/n47-f-r4rows.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const OFF_WHY = "Off until the lead has looked at its frames and its gates pass (NIGHT47.md section 1: a GG ask is ON at the freeze when its gates are GREEN, verbs37 passes on levels 1-5, the smoke passes and the frames show the change).";
const ROWS = [
  ["finale47", "View row (NightRows.ViewOn(cfg, \"finale47\")). Builder F, R4 idea 1 (NIGHT47.md section 6's list: \"a boss finale beat (a short slow motion on the last hit, the arena reacting)\"): on bossDown the arena answers the kill (a white-cyan light flare from the boss, a floor shockwave ring out to the room, a camera hit) and the boss's name comes down in big type (\"<NAME> DOWN\"); the slow motion is bossKit's own (bossDying)."],
  ["hitzone47", "View row (NightRows.ViewOn(cfg, \"hitzone47\")). Builder F, R4 idea 2 (NIGHT47.md section 6's list: \"enemy hit reactions that read the hit's direction\"): where a player hit lands on a body is read by the body: a head hit snaps it back (away from the shot), a leg hit buckles it, a hit on one side twists it away; on top of NIGHT18 / NIGHT36's sideways kick and stagger."],
];

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  if (out === null) {
    const head = '      "lava47": {' + eol;
    const at = text.indexOf(head);
    if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("expected exactly one lava47 row");
    const close = text.indexOf(eol + "      }", at);
    if (close < 0) throw new Error("lava47: no close");
    let insertAt = close + (eol + "      }").length;
    let add = "";
    for (const [name, why] of ROWS) {
      if (text.includes('      "' + name + '": {' + eol)) { console.log("n47-f-r4rows: " + name + " already in"); continue; }
      add += "," + eol + '      "' + name + '": {' + eol +
        '        "_why": ' + JSON.stringify(why) + "," + eol +
        '        "enabled": false,' + eol +
        '        "enabled_why": ' + JSON.stringify(OFF_WHY) + "," + eol +
        '        "numbers": {}' + eol +
        "      }";
      console.log("n47-f-r4rows: " + name + " added");
    }
    text = text.slice(0, insertAt) + add + text.slice(insertAt);
    JSON.parse(text);
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n47-f-r4rows: wrote " + rel);
}

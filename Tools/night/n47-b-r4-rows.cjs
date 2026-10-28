// NIGHT47 builder B, round 4 (the beyond round, G44 / G45; the lead's OK in his round-4 message): adds B's two new sim rows, off, at the end
// of design.n47 in both config copies (byte-identical): stampede47 (THE CHARGER's stampede as a SURVIVAL event) and trail47 (THE KILN
// WYRM's burrow trail burns the floor over it). Their numbers are written by Tools/night/n47-b-rows.cjs. Idempotent; touches nothing else.
// Run from the worktree root: node Tools/night/n47-b-r4-rows.cjs && node Tools/night/n47-b-rows.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const OFF = "Off until the lead has looked at its frames and its gates pass (NIGHT47.md section 1; the beyond round, section 6).";
const ROWS = [
  ["stampede47", "Builder B (sim row, round 4: the lead's list, \"the CHARGER's stampede as a SURVIVAL event\"): from a SURVIVAL round on, every few rounds a herd of chargers comes in together from one side of the map at the round's start (event stampede47), and with charger47 on they burst the husks in their way: a SURVIVAL beat in the CHARGER's language (CoD Zombies' special rounds, B.md R-B5)."],
  ["trail47", "Builder B (sim row, round 4): THE KILN WYRM's burrow trail burns the floor over its buried pieces (a grounded player within its radius takes a tick, event wyrm47TrailBurn): the floor is the worm's, the air (the boost, the islands) is yours: G33's arena ask carried into the burrow itself (Diablos' burrow, B.md R-B2)."],
];

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  if (out === null) {
    const head = '      "lava47": {' + eol;
    const at = text.indexOf(head);
    if (at < 0) throw new Error("no lava47 row (the end of design.n47)");
    const close = text.indexOf(eol + "      }", at);
    if (close < 0) throw new Error("lava47's end not found");
    const insertAt = close + (eol + "      }").length;
    let add = "";
    for (const [name, why] of ROWS) {
      if (text.includes('      "' + name + '": {' + eol)) { console.log("n47-b-r4-rows: " + name + " already in"); continue; }
      add += "," + eol + '      "' + name + '": {' + eol
        + '        "sim": true,' + eol
        + '        "_why": ' + JSON.stringify(why) + "," + eol
        + '        "enabled": false,' + eol
        + '        "enabled_why": ' + JSON.stringify(OFF) + "," + eol
        + '        "numbers": {}' + eol
        + "      }";
      console.log("n47-b-r4-rows: " + name + " added");
    }
    text = text.slice(0, insertAt) + add + text.slice(insertAt);
    const j = JSON.parse(text);
    for (const [name] of ROWS) if (!j.design.n47[name]) throw new Error(name + " missing after the insert");
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n47-b-r4-rows: wrote " + rel);
}

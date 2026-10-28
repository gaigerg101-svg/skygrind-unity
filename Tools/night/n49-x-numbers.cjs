// NIGHT49 builder X: X's numbers in its OWN pre-placed row design.n49.fx49 (the shared commit placed it, off, "numbers": {}) in
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json. A text edit of the row's empty "numbers": {}
// (nothing else in the file moves); re-running is a no-op once the row has its numbers. These are the kit's tier rules (which damage is a heavy
// or a critical effect): they follow the game's damage numbers, so they live beside them as data; the kit's sizes, brightness and timings are
// render numbers (UnityTuning.Fx49.cs). Fx49.cs reads each with UnityTuning's default when the key is missing.
// Run (from the worktree root): node Tools/night/n49-x-numbers.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const NUMBERS = {
  fx49: [
    ["hitHeavyAt", 40, "a player hit (HitEvent.damage) at or over this draws the kit's HEAVY tier (more damage = bigger, brighter, sharper: UNIFY.md section 4). 40 sits over a KESTREL tap (weapons.revolver.damage 25), a THUNDERCLAP pellet (pelletDamage 8) and a punch (punch.damage 10), and under a KESTREL headshot (25 x headshotMult 2 = 50) and the charged shots: the common hits stay light so the big ones read over them (\"if everything is important, nothing is important\")."],
    ["hitCriticalAt", 150, "a player hit at or over this draws the CRITICAL tier (a headshot raises a hit one tier on its own): the slug's volley (8 x 8 x its damageMult) and a charged headshot reach it; a tap never does."],
    ["hurtHeavyAt", 15, "an enemy's hit on the player (HurtEvent.amount) at or over this draws the enemy's HEAVY shards; under it LIGHT (a chip). Not checked against every enemy's damage: the a249 reel prints the tier it drew."],
    ["hurtCriticalAt", 35, "an enemy's hit on the player at or over this draws the enemy's CRITICAL shards (a boss's blow, a heavy kind's slam)."],
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
      if (ne < 0 || ne > close) { console.log("n49-x-numbers: " + row + " already has numbers"); continue; }
      let body = '        "numbers": {';
      nums.forEach(([k, v, why], i) => {
        body += eol + '          "' + k + '": ' + JSON.stringify(v) + "," + eol + '          "' + k + '_why": ' + JSON.stringify(why) + (i < nums.length - 1 ? "," : "");
      });
      body += eol + "        }";
      text = text.slice(0, ne) + body + text.slice(ne + empty.length);
      console.log("n49-x-numbers: " + row + " numbers " + nums.map((n) => n[0]).join(", "));
    }
    const cfg = JSON.parse(text);
    for (const [row, nums] of Object.entries(NUMBERS))
      for (const [k, v] of nums) if (cfg.design.n49[row].numbers[k] !== v) throw new Error(row + "." + k + " did not round-trip");
    if (cfg.design.n49.fx49.enabled !== false) throw new Error("fx49 must stay enabled false");
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n49-x-numbers: wrote " + rel);
}

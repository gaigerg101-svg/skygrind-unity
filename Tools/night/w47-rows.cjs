// NIGHT47 builder W (G9, round 4; the lead's OK in round 4's message: "each behind its own row (add them to design.n47 with your own .cjs, enabled
// false, a _why each)"): adds W's three G9 rows to design.n47 in BOTH config copies, right after wickLook47, touching nothing else:
//   wickRideParry47 (sim)  a parry punched on (or just off) a ride sends the ember back 1.5x faster and it cracks TWO plates (the ride and the parry
//                          combine: DESIGN-RULES M-6; ULTRAKILL boosts parried projectiles, Research/2026-10-18-wave47/W.md, Reported)
//   wickIntro47 (sim)      the walk-in's name card: "THE WICK. THE FURNACE WAKES." through BossLine40 before the first hint (B-3)
//   wickFinale47 (view)    the finisher: a slow motion (the game's parry slow window, Slow39), the glass cracking from under THE WICK, the slag flaring
// Checks: both copies parse, everything but design.n47's three new keys round-trips, the copies are byte-identical. Idempotent (already in: skip).
// Usage (worktree root): node Tools/night/w47-rows.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const COPIES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const off = "Off until the lead has looked at its frames and its gates pass (NIGHT47.md section 1; G9's ideas are W's own, R4).";
const ROWS = {
  wickRideParry47: {
    sim: true,
    _why: "Builder W (G9 idea 1, sim row): a parry punched while riding a panel (or within design.n37.wick37.boss.rideGrace of one) sends the ember back faster and it cracks the plate it hits AND the next one (two gaps): the ride and the parry combine (DESIGN-RULES M-6: combined inputs combine), the climb rewards the parry GG likes. ULTRAKILL boosts a parried projectile's speed and damage (W.md, Reported). Core/Enemies/Wick37.N47.cs.",
    enabled: false, enabled_why: off,
    numbers: { speedMult: 1.5, speedMult_why: "x the ember's return speed (design.n37.wick37.boss.ember.returnSpeed 22 m/s -> 33): it reads as a harder return.", cracks: 2, cracks_why: "plates cracked by a ride-parried ember: the hit one and the next (crackSeconds each)." },
  },
  wickIntro47: {
    sim: true,
    _why: "Builder W (G9 idea 2, sim row; the lead's list: 'boss names and one line of intro at the walk-in, big and readable'; DESIGN-RULES B-3): at the fight's begin a name card line through BossLine40 (F's hud47 sizes it: 44 px), then the first hint when it ends. SurvivalSim.N47.Wick.cs.",
    enabled: false, enabled_why: off,
    numbers: { line: "The Wick. The furnace wakes.", line_why: "<= 6 words (5): the name, then what it is (the copy rules: plain words, second person not needed for a title card).", seconds: 2.5, seconds_why: "s held (2.0 s + 0.3 s a word would be 3.5: the reveal is 2 s; the hint follows at once)." },
  },
  wickFinale47: {
    _why: "View row (NightRows.ViewOn(cfg, \"wickFinale47\")). Builder W (G9 idea 3; the lead's list: 'a boss finale beat (a short slow motion on the last hit, the arena reacting)'): the finishing core punch slows the world (the game's parry slow window, Slow39.OnParry, NIGHT39 F1), the glass cracks in yellow-white lines from under THE WICK and the slag below flares. Wick37View.N47.cs; numbers in UnityTuning.Wick47.cs.",
    enabled: false, enabled_why: off,
    numbers: {},
  },
};

const dry = process.argv.includes("--dry");
const outs = [];
for (const rel of COPIES) {
  const p = path.join(ROOT, rel);
  const raw = fs.readFileSync(p, "utf8");
  const before = JSON.parse(raw);
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  let text = raw;
  const n47 = text.indexOf('"n47": {');
  const key = text.indexOf('"wickLook47": {', n47);
  if (n47 < 0 || key < 0) throw new Error(rel + ": design.n47.wickLook47 not found");
  let todo = Object.keys(ROWS).filter((k) => !before.design.n47[k]);
  if (todo.length === 0) { console.log(rel + ": already in"); outs.push([p, raw]); continue; }
  // the wickLook47 object's end
  let i = text.indexOf("{", key), depth = 0, end = -1;
  for (let j = i; j < text.length; j++) {
    const ch = text[j];
    if (ch === '"') { j++; while (text[j] !== '"') { if (text[j] === "\\") j++; j++; } continue; }
    if (ch === "{") depth++;
    else if (ch === "}") { depth--; if (depth === 0) { end = j; break; } }
  }
  const lineStart = text.lastIndexOf(eol, key) + eol.length;
  const indent = text.slice(lineStart, key);
  let add = "";
  for (const k of todo) add += "," + eol + indent + JSON.stringify(k) + ": " + JSON.stringify(ROWS[k], null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol);
  text = text.slice(0, end + 1) + add + text.slice(end + 1);
  const after = JSON.parse(text);
  for (const k of todo) { if (JSON.stringify(after.design.n47[k]) !== JSON.stringify(ROWS[k])) throw new Error(k + " did not round-trip"); delete after.design.n47[k]; }
  if (JSON.stringify(before) !== JSON.stringify(after)) throw new Error(rel + ": something else changed");
  outs.push([p, text]);
  console.log(rel + ": added " + todo.join(", "));
}
if (outs[0][1] !== outs[1][1]) throw new Error("the two copies differ");
if (!dry) for (const [p, t] of outs) fs.writeFileSync(p, t);
console.log(dry ? "dry run" : "written; the copies byte-identical");

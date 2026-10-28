// NIGHT45 C1: T-62, the one logged in-place edit of an older level (NIGHT45.md, C1's card; NIGHT45-BRIEF.md section 2).
// n40-span's o2 lesson prompt was 7 words ("{grapple} HOOK THE KNEE. {grapple} PULL AWAY."); Lesson25TextTests allows 6.
// Text only: both {grapple} glyphs and both actions stay (hook the knee, then grapple away to tear it); "THE" goes.
// Exact string replace in the five places, nothing else in the bytes changes. Run from the repo root: node Tools/night/n45-c1-t62.cjs
"use strict";
const fs = require("fs");
const OLD = "{grapple} HOOK THE KNEE. {grapple} PULL AWAY.";
const NEW = "{grapple} HOOK KNEE. {grapple} PULL AWAY.";
const files = [
  ["Tools/night/gen-n40-span.cjs", 1],
  ["Design/levels/n40/span.plan.json", 4],
  ["Design/levels/n40/span.json", 4],
  ["Design/levels/n40-span.json", 4],
  ["Assets/StreamingAssets/Design/levels/n40-span.json", 4],
];
let bad = 0;
for (const [f, want] of files) {
  const s = fs.readFileSync(f, "utf8");
  const n = s.split(OLD).length - 1;
  if (n !== want) { console.log(`${f}: found ${n}, want ${want}; skipped`); bad++; continue; }
  fs.writeFileSync(f, s.split(OLD).join(NEW), "utf8");
  console.log(`${f}: ${n} replaced`);
}
const words = NEW.replace(/\{[a-zA-Z]+\}/g, "X").split(" ").filter((w) => w.length).length;
console.log(`new prompt: ${words} words`);
process.exit(bad ? 1 : 0);

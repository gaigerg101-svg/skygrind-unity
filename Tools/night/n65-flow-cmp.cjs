// NIGHT65 builder T: one level's n46flow lines against the flow base (Logs/n65/flow-base.txt), as n65-gates.sh --flow compares them: every
// line of the level's block (from "n46flow <id> (loaded" to the line before the next level's), wall-time and header lines aside, equal.
//   node Tools/night/n65-flow-cmp.cjs <level> <base.txt> <run.txt>
const fs = require("fs");
const [lv, baseF, runF] = process.argv.slice(2);
if (!runF) { console.error("usage: n65-flow-cmp.cjs <level> <base.txt> <run.txt>"); process.exit(2); }
const block = (f) => {
  const lines = fs.readFileSync(f, "utf8").replace(/\r\n/g, "\n").split("\n");
  const a = lines.findIndex((l) => l.startsWith("n46flow " + lv + " (loaded"));
  if (a < 0) return null;
  let b = lines.findIndex((l, i) => i > a && /^n46flow \S+ \(loaded/.test(l) && !l.startsWith("n46flow " + lv + " "));
  if (b < 0) b = lines.length;
  return lines.slice(a, b).filter((l) => !/wall time|^n46flow: seeds/i.test(l) && l.trim() !== "");
};
const A = block(baseF), B = block(runF);
if (!A || !B) { console.log("n65-flow-cmp " + lv + ": no block in " + (!A ? baseF : runF)); process.exit(1); }
const diff = [];
for (let i = 0; i < Math.max(A.length, B.length); i++) if (A[i] !== B[i]) diff.push("  line " + (i + 1) + "\n    base: " + (A[i] || "(none)") + "\n    run:  " + (B[i] || "(none)"));
console.log("n65-flow-cmp " + lv + ": " + B.length + " lines against " + A.length + " in " + baseF + ": " + (diff.length ? diff.length + " differ\n" + diff.slice(0, 10).join("\n") : "EQUAL line for line (wall-time lines aside)"));
process.exit(diff.length ? 1 : 0);

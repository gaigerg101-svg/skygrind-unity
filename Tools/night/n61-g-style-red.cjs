// NIGHT61 builder G (S5, the style meter's red; read only): which style-meter texts the HUD showed, frame by frame, in FeelProbe telemetry.
// For each telemetry.jsonl given: the frames, the frames whose visible HUD texts name a style rank (DULL .. SKYGRIND), the count per rank,
// the style-like HUD paths with visible text and their most frequent texts.
//   node Tools/night/n61-g-style-red.cjs Logs/feel/n60-freeze/vf/verbs37/telemetry.jsonl [...]
//   node Tools/night/n61-g-style-red.cjs --selftest     NIGHT63 V (R1's N12): the rank matcher on planted texts (PASS / FAIL each; exit 1 on a FAIL)
// Writes nothing.
const fs = require("fs");
const RANKS = ["DULL", "COOL", "BOLD", "AIRBORNE", "SAVAGE", "SKYGRIND"];

/** The ranks a HUD text names, as whole words (NIGHT63 V, R1's N12: "COOL" is not in "COOLDOWN", "BOLD" not in "BOLDER"). */
const RANK_RE = RANKS.map((r) => [r, new RegExp("\\b" + r + "\\b")]);
function ranksIn(text) {
  const t = String(text).toUpperCase();
  const out = [];
  for (const [r, re] of RANK_RE) if (re.test(t)) out.push(r);
  return out;
}

if (process.argv.includes("--selftest")) {
  // NIGHT63 V (R1's N12; NIGHT61 review N12): a rank counts as a whole word only ("COOL" is not in "COOLDOWN", "BOLD" not in "BOLDER")
  const cases = [
    ["COOL", ["COOL"]], ["cool x3", ["COOL"]], ["COOLDOWN 4s", []], ["BOLDER", []], ["BOLD!", ["BOLD"]], ["SKYGRINDING", []],
    ["SKYGRIND", ["SKYGRIND"]], ["style: dull", ["DULL"]], ["DULLNESS", []], ["AIRBORNE / SAVAGE", ["AIRBORNE", "SAVAGE"]], ["", []],
  ];
  let fails = 0;
  for (const [text, want] of cases) {
    const got = ranksIn(text);
    const ok = JSON.stringify(got) === JSON.stringify(want);
    if (!ok) fails++;
    console.log((ok ? "PASS " : "FAIL ") + JSON.stringify(text) + " -> " + JSON.stringify(got) + (ok ? "" : " (want " + JSON.stringify(want) + ")"));
  }
  console.log("selftest: " + (cases.length - fails) + " / " + cases.length + " PASS");
  process.exit(fails ? 1 : 0);
}

const files = process.argv.slice(2);
if (files.length === 0) throw new Error("usage: node Tools/night/n61-g-style-red.cjs <telemetry.jsonl> [...]");
let all = 0;
const allRanks = {};
for (const file of files) {
  const lines = fs.readFileSync(file, "utf8").split("\n").filter(Boolean);
  const paths = new Map();
  const texts = new Map();
  const rankFrames = {};
  let frames = 0, withRank = 0;
  for (const l of lines) {
    let o;
    try { o = JSON.parse(l); } catch (e) { continue; }
    if (o.type !== "frame") continue;
    frames++;
    const seen = new Set();
    for (const u of o.ui || []) {
      if (!u.path || u.text == null || !(u.alpha > 0)) continue;
      if (!/style|rank|combo|meter|award/i.test(u.path)) continue;
      paths.set(u.path, (paths.get(u.path) || 0) + 1);
      const t = String(u.text).trim();
      if (t) texts.set(u.path + " = " + t, (texts.get(u.path + " = " + t) || 0) + 1);
      for (const r of ranksIn(t)) seen.add(r);
    }
    if (seen.size) withRank++;
    for (const r of seen) { rankFrames[r] = (rankFrames[r] || 0) + 1; allRanks[r] = (allRanks[r] || 0) + 1; }
  }
  all += withRank;
  console.log("== " + file + ": " + frames + " frames; frames naming a rank: " + withRank + "; by rank " + JSON.stringify(rankFrames));
  console.log("   style paths with visible text (frames): " + JSON.stringify([...paths.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8)));
  console.log("   top texts: " + JSON.stringify([...texts.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8)));
}
console.log("TOTAL frames naming a rank: " + all + "; by rank " + JSON.stringify(allRanks));

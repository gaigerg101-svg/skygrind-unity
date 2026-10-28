// NIGHT46 builder F1: design.n46.reveal46.numbers (the shared commit's {} -> F1's numbers), as text into Design/unity-config.json (only that line
// changes), then the StreamingAssets copy as a byte copy. Refuses when the numbers are not {}. The row stays enabled: false.
//   node Tools/night/n46-f1-reveal.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const A = path.join(ROOT, "Design", "unity-config.json");
const B = path.join(ROOT, "Assets", "StreamingAssets", "Design", "unity-config.json");
const dry = process.argv.includes("--dry");
const NUMS = {
  revealS: 1,
  revealS_why: "FLOW-AUDIT row 8 ('shorten the reveal / phase seconds in config, about 1 s each'): a boss's Reveal (untouchable, not attacking, the player keeps control) is held to at most 1 s: a9 2 s, x9 1.5 s, o3 2 s, v11 2 s today. DECIDE44 D-3 keeps every reveal: 1 s still shows the boss its name and its first pose (the view's own camera ease is not changed).",
  phaseS: 1,
  phaseS_why: "The same row's leg / phase change (Boss37 LegChange, Brawl40 PhaseChange: 0.8-1.5 s today) held to at most 1 s (a Boss37 leg change keeps its own 2-step floor). The death windows are not touched (the finisher plays; the room clears on the boss's defeat).",
};
let text = fs.readFileSync(A, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const n46 = text.indexOf('"n46": {');
const at = text.indexOf('"reveal46": {', n46);
if (n46 < 0 || at < 0) throw new Error("design.n46.reveal46 not found");
const nAt = text.indexOf('"numbers": ', at);
const close = text.indexOf("}", text.indexOf("{", nAt));
if (text.slice(nAt, close + 1) !== '"numbers": {}') throw new Error("design.n46.reveal46.numbers is not {} (already run?)");
const indent = text.slice(text.lastIndexOf("\n", nAt) + 1, nAt);
const body = JSON.stringify(NUMS, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
text = text.slice(0, nAt) + '"numbers": ' + body + text.slice(close + 1);
const back = JSON.parse(text).design.n46.reveal46;
if (JSON.stringify(back.numbers) !== JSON.stringify(NUMS) || back.enabled !== false) throw new Error("reveal46 did not round-trip");
if (dry) { console.log("n46-f1-reveal: planned (dry run)"); process.exit(0); }
fs.writeFileSync(A, text);
fs.writeFileSync(B, text);
console.log("n46-f1-reveal: design.n46.reveal46.numbers <- revealS 1, phaseS 1 (both config copies)");

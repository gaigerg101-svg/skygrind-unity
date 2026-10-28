// NIGHT46 builder F1: the numbers of F1's sim rows (design.n46.retry46 / overlap46 / teach46 .numbers), patched as text into
// Design/unity-config.json (only the three rows' `"numbers": {}` lines change; every other byte stays), then the StreamingAssets copy written as a
// byte copy. Refuses when a row's numbers are not the shared commit's empty `{}` (already run, or someone else's edit). The rows stay enabled: false.
//   node Tools/night/n46-f1-rows.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const A = path.join(ROOT, "Design", "unity-config.json");
const B = path.join(ROOT, "Assets", "StreamingAssets", "Design", "unity-config.json");
const dry = process.argv.includes("--dry");

const NUMBERS = {
  retry46: {
    wavesBack: 0,
    wavesBack_why: "DECIDE46 N-6 ('you restart at the wave you died in (the waves already won stay won), with the same 1.2 s retry'): 0 = the wave being fought when the player died comes in again; the waves cleared before it are not refought. FLOW-AUDIT row 1: b1's bot died 10 times in 10 walks, each time back to wave 1 (max 263 s). The retry pause (design.n24.campaign.retryDelay 1.2 s) and start40's grace after a death are unchanged.",
  },
  overlap46: {
    bodiesLeft: 2,
    bodiesLeft_why: "DECIDE46 N-7 ('the next wave starts when 1-2 bodies of this one remain'; Doom GDC 2018: waves keyed to the fight, not the last kill): the next wave is released while at most 2 of the room's bodies stand, instead of at 0 plus waveGap 0.8 s (FLOW-AUDIT row 5: 29 gaps of ~1 s then a >= 12 m walk to the new bodies).",
    shareLeft: 0.5,
    shareLeft_why: "And at most half of the wave just spawned (rounded down): a 1-body wave (g1's tank, a lone charger) is never overlapped, a 3-body wave releases at 1 left, a 4+ body wave at 2 left. ULTRAKILL's Cyber Grind holds its next spawns to a share of the wave dead (arena-flow-web.md); half keeps two waves from stacking into one double wave.",
    fromWave: 1,
    fromWave_why: "Only waves 2 and later are released early: wave 1 keeps the commit gate (rooms43 commitM / commitMaxS) and a2's stand-ins, so the player still steps in before anything spawns (arena-flow-web.md Part D: commit, then the fight).",
  },
  teach46: {
    standInHoldS: 8,
    standInHoldS_why: "DECIDE46 N-11 ('holds capped at ~8 s'): a2 KESTREL's stand-ins hold wave 1 at most 8 s after the entry (rooms43 standInHoldS 20). FLOW-AUDIT row 4: the doorway pilot's first wave came at 20.19 s in a2. 8 s is teach35's 5 s line plus three seconds to try the verb.",
    afterLessonMaxS: 8,
    afterLessonMaxS_why: "DECIDE46 N-11: a wave that waits for its lesson (afterLesson43: a8 PIERCE's perch) comes in at most 8 s after the entry (rooms43 afterLessonMaxS 20; FLOW-AUDIT row 4: a8's first wave at 20.19 s). The same 8 s as the stand-ins: one rule for every teaching hold.",
    lessonCapS: 8,
    lessonCapS_why: "The door's lesson cap after the last kill stays 8 s (rooms43 lessonCapS 8 already meets N-11's ~8 s cap); listed so the three holds read from one row.",
  },
};

let text = fs.readFileSync(A, "utf8");
const eol = text.includes("\r\n") ? "\r\n" : "\n";
const n46At = text.indexOf('"n46": {');
if (n46At < 0) throw new Error("design.n46 not found (the shared commit is not in)");
let changed = 0;
for (const [row, nums] of Object.entries(NUMBERS)) {
  const key = '"' + row + '": {';
  const at = text.indexOf(key, n46At);
  if (at < 0) throw new Error("design.n46." + row + " not found");
  const nAt = text.indexOf('"numbers": ', at);
  const next = text.indexOf("}", text.indexOf("{", nAt));
  if (nAt < 0 || text.slice(nAt, next + 1) !== '"numbers": {}') throw new Error("design.n46." + row + ".numbers is not the shared commit's {} (already run?)");
  const lineStart = text.lastIndexOf("\n", nAt) + 1;
  const indent = text.slice(lineStart, nAt);
  const body = JSON.stringify(nums, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  text = text.slice(0, nAt) + '"numbers": ' + body + text.slice(next + 1);
  changed++;
  console.log("n46-f1-rows: design.n46." + row + ".numbers <- " + Object.keys(nums).filter((k) => !k.endsWith("_why")).map((k) => k + " " + nums[k]).join(", "));
}
const back = JSON.parse(text).design.n46;
for (const [row, nums] of Object.entries(NUMBERS)) if (JSON.stringify(back[row].numbers) !== JSON.stringify(nums) || back[row].enabled !== false) throw new Error("design.n46." + row + " did not round-trip (or is not enabled: false)");
if (dry) { console.log("n46-f1-rows: " + changed + " row(s) planned (dry run, nothing written)"); process.exit(0); }
fs.writeFileSync(A, text);
fs.writeFileSync(B, text);
console.log("n46-f1-rows: " + changed + " row(s) written to Design/unity-config.json and its StreamingAssets copy");

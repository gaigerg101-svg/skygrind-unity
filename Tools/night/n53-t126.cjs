// NIGHT53 P0 (the lead; T-126 at its source, NIGHT53.md section 3 step 3): Lesson25Tests.TheOrder_FollowsTheBlocks sets N38 / N40-N43Blocks.ForceOff,
// N45Levels.ForceOff and NightRows.ForceOffFrom and never restored them, so every later test in the same domain read the campaign with those
// nights forced off (NIGHT52's batch suite: 2237 / 2238, pinned test-only in N52SharedTests). The test now saves every static it sets and
// restores it in a finally; its body moves to TheOrder_Body unchanged.   node Tools/night/n53-t126.cjs [--dry]
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "../../Assets/Tests/EditMode/Lesson25Tests.cs");
let s = fs.readFileSync(file, "utf8");
const crlf = s.includes("\r\n");
if (crlf) s = s.replace(/\r\n/g, "\n");
const a = "        public void TheOrder_FollowsTheBlocks()\n        {\n            GameConfig c = Cfg;\n";
const n = s.split(a).length - 1;
if (n !== 1) throw new Error("anchor found " + n + " times");
const b =
  "        public void TheOrder_FollowsTheBlocks()\n        {\n" +
  "            // NIGHT53 (T-126, n53-t126.cjs): the body forces older nights off; every static it sets is restored here, so no later test in the\n" +
  "            // domain reads the campaign with them forced off (NIGHT52's batch suite: N52SharedTests read the ON view rows as off)\n" +
  "            int wasFrom = NightRows.ForceOffFrom;\n" +
  "            bool w38 = N38Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff, w45 = N45Levels.ForceOff;\n" +
  "            try { TheOrder_Body(); }\n" +
  "            finally\n            {\n" +
  "                NightRows.ForceOffFrom = wasFrom;\n" +
  "                N38Blocks.ForceOff = w38; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43; N45Levels.ForceOff = w45;\n" +
  "            }\n        }\n\n" +
  "        static void TheOrder_Body()\n        {\n            GameConfig c = Cfg;\n";
s = s.replace(a, () => b);
// the body sets exactly these and no other ForceOff without a restore (asserted so a later edit to the body is seen)
const body = s.slice(s.indexOf("static void TheOrder_Body()"), s.indexOf("// --- the stage rules in Campaign24"));
const sets = [...body.matchAll(/(N\d\dBlocks|N45Levels|NightRows)\.(ForceOff|ForceOffFrom) = /g)].map((m) => m[1]);
const restored = new Set(["N38Blocks", "N40Blocks", "N41Blocks", "N42Blocks", "N43Blocks", "N45Levels", "NightRows", "N30Blocks", "N32Blocks", "N33Blocks", "N34Blocks", "N35Blocks", "N37Blocks"]);
const loose = sets.filter((x) => !restored.has(x));
if (loose.length) throw new Error("the body sets statics not restored: " + loose.join(", "));
if (/static GameConfig Cfg|GameConfig Cfg =>/.test(s) === false) console.log("note: Cfg's declaration not seen as static (check the compile)");
if (process.argv.includes("--dry")) { console.log("DRY RUN: anchor found once; statics set in the body: " + [...new Set(sets)].join(", ")); process.exit(0); }
if (crlf) s = s.replace(/\n/g, "\r\n");
fs.writeFileSync(file, s);
console.log("Lesson25Tests: TheOrder_FollowsTheBlocks restores " + [...new Set(sets)].join(", "));

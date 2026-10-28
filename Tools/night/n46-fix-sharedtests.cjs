// NIGHT46 (the lead): N46SharedTests' Switches() also saves, clears and restores the older nights' ForceOff flags that Lesson25Tests
// leaves set (Lesson25Tests.cs:93-99: N38 / N40-N43 forced off with no restore), so the "N" batch after "L" sees NIGHT45's real order.
// Measured: Core suite 00:17 N46SharedTests 2 FAIL (EveryRowOff_IsNight45sOrder, TheOffSwitch_LeavesNight45sOrder) vs 4 / 4 alone at G0.
// Run: node Tools/night/n46-fix-sharedtests.cjs
const fs = require("fs");
const f = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/Tests/EditMode/N46SharedTests.cs";
let s = fs.readFileSync(f, "utf8");
const crlf = s.includes("\r\n");
if (crlf) s = s.replace(/\r\n/g, "\n");
const a1 = "            bool was45 = N45Levels.ForceOff;\n            try\n            {\n                NightRows.ForceOffFrom = offFrom;\n                N45Levels.ForceOff = false;\n";
const b1 = "            bool was45 = N45Levels.ForceOff;\n" +
  "            bool w38 = N38Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff;\n" +
  "            try\n            {\n                NightRows.ForceOffFrom = offFrom;\n                N45Levels.ForceOff = false;\n" +
  "                // Lesson25Tests leaves these set (no restore); NIGHT45's order needs them as the config says\n" +
  "                N38Blocks.ForceOff = N40Blocks.ForceOff = N41Blocks.ForceOff = N42Blocks.ForceOff = N43Blocks.ForceOff = false;\n";
const a2 = "                NightRows.ForceOffFrom = was;\n                N45Levels.ForceOff = was45;\n";
const b2 = a2 + "                N38Blocks.ForceOff = w38; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43;\n";
for (const a of [a1, a2]) if (s.split(a).length !== 2) throw new Error("anchor not found once: " + a.slice(0, 60));
s = s.replace(a1, () => b1).replace(a2, () => b2);
if (crlf) s = s.replace(/\n/g, "\r\n");
fs.writeFileSync(f, s);
console.log("patched " + f);

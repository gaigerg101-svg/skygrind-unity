// NIGHT53 close (the lead; the batch suite at 77c4d995: V50Tests 2 FAILs): V50Tests pins NIGHT49 / NIGHT52's look49 grade lists; polish53
// (ON at the NIGHT53 freeze) adds THE ICE LINE's ids under its icelineGrade item (U's T-116; pinned by N53PolishTests), so with no off
// switch "n47-iceline" reads ON. V50Tests' Clean() now runs with NightRows.ForceOffFrom = 53 (every NIGHT52 and older row reads as the
// config says; NIGHT53's polish off) instead of NoOff. Test-only; the game is unchanged.   node Tools/night/n53-v50pin.cjs
const fs = require("fs");
const path = require("path");
const f = path.join(__dirname, "../../Assets/Tests/EditMode/Game/V50Tests.cs");
let s = fs.readFileSync(f, "utf8");
const crlf = s.includes("\r\n");
if (crlf) s = s.replace(/\r\n/g, "\n");
const a = "                PostFx.Look49Force = null;\n                NightRows.ForceOffFrom = NightRows.NoOff;\n";
if (s.split(a).length - 1 !== 1) throw new Error("anchor found " + (s.split(a).length - 1) + " times");
s = s.replace(a, "                PostFx.Look49Force = null;\n                NightRows.ForceOffFrom = 53; // NIGHT53 (n53-v50pin.cjs): polish53's iceline grade off: these pins are NIGHT49 / NIGHT52's lists (N53PolishTests pins polish53's)\n");
if (crlf) s = s.replace(/\n/g, "\r\n");
fs.writeFileSync(f, s);
console.log("V50Tests: Clean() pins ForceOffFrom 53");

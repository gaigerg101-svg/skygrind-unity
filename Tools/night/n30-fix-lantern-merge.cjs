// NIGHT30 (the lead): resolves the Lantern30.cs cherry-pick conflict between B's lineGate (fd10785e) and S's sayLine30 (9c6cbf58).
// Both are a gate for ONE budget; only one is asked (lineGate first, else sayLine30), so a line is never counted twice.
const fs = require("fs");
const f = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/Scripts/Core/Enemies/Lantern30.cs";
let s = fs.readFileSync(f, "utf8");
const a = s.indexOf("<<<<<<< HEAD"), m = s.indexOf("=======", a), z = s.indexOf(">>>>>>>", m);
if (a < 0 || m < 0 || z < 0) throw new Error("no conflict block");
const zEnd = s.indexOf("\n", z) + 1;
const nl = s.includes("\r\n") ? "\r\n" : "\n";
const merged = [
  "        /// <summary>NIGHT30 (builder S, item 12): the ONE boss-line budget (SurvivalSim.BossLine30 via Teach30, design.n30.teach.maxShows per fight): asked before a",
  "        /// line is said; false = over the budget, nothing said. Null (teach off): the Lantern's own lineMaxShows count alone. (The lead's merge: B's lineGate and",
  "        /// S's sayLine30 are the same gate; only one is asked, lineGate first.)</summary>",
  "        public Func<string, string, bool> sayLine30;",
  "",
  "        /// <summary>One place for the budget: the sim's gate when it has one (design.n30.teach: Teach30's per-fight count), else lineMaxShows per fight here.</summary>",
  "        void TryLine(string key, string text, ref int shows)",
  "        {",
  "            Func<string, string, bool> gate = lineGate ?? sayLine30;",
  "            if (gate != null) { if (!gate(key, text)) return; shows++; }",
  "            else { if (shows >= p.lineMaxShows) return; shows++; }",
  "            Line(key, text, shows);",
  "        }",
].join(nl) + nl;
s = s.slice(0, a) + merged + s.slice(zEnd);
if (s.includes("<<<<<<<") || s.includes(">>>>>>>")) throw new Error("conflict markers left");
fs.writeFileSync(f, s);
console.log("Lantern30.cs merged");

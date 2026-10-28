// NIGHT47 builder W (G9 round 4): wires wickRideParry47 / wickIntro47 in Game/SurvivalSim.N47.Wick.cs (W's own partial; anchored; idempotent).
// Usage (worktree root): node Tools/night/w47-g9-sim.cjs
const fs = require("fs");
const path = require("path");
const p = path.join(__dirname, "../../Assets/Scripts/Game/SurvivalSim.N47.Wick.cs");
let t = fs.readFileSync(p, "utf8");
const edits = [
  ["            bool climb = rows.On(idClimb47);\n",
   "            idRideParry47 = rowsRegistry.IndexOf(\"wickRideParry47\"); // NIGHT47 W G9 (round 4): the two G9 sim rows (-1: this config lacks them)\n            idIntro47 = rowsRegistry.IndexOf(\"wickIntro47\");\n            wick47IntroDue = false;\n            bool climb = rows.On(idClimb47);\n"],
  ["            wick37.p47 = wick47P;\n",
   "            wick37.p47 = wick47P;\n            wick37.rideParry47 = rows.On(idRideParry47); // G9 idea 1\n            if (wick37.rideParry47) { wick37.rideMult47 = cfg.Num(\"design.n47.wickRideParry47.numbers.speedMult\"); wick37.rideCracks47 = cfg.Int(\"design.n47.wickRideParry47.numbers.cracks\"); }\n"],
  ["        bool wick47Rode;\n",
   "        bool wick47Rode, wick47IntroDue;\n        internal int idRideParry47 = -1, idIntro47 = -1;\n        public int wick47Intros;\n"],
  ["            Wick47Say(\"reveal\");\n",
   "            if (rows.On(idIntro47)) // G9 idea 2: the name card first, the reveal hint when it ends (N47WStep)\n            {\n                wick47Line.Say(\"intro\", cfg.Str(\"design.n47.wickIntro47.numbers.line\"), cfg.Num(\"design.n47.wickIntro47.numbers.seconds\"));\n                wick47Intros++;\n                wick47IntroDue = true;\n            }\n            else Wick47Say(\"reveal\");\n"],
  ["            if (wick47Line != null) { if (fight) wick47Line.Step(); else if (wick47Line.Now != null) wick47Line.Clear(); }\n",
   "            if (wick47Line != null) { if (fight) wick47Line.Step(); else if (wick47Line.Now != null) wick47Line.Clear(); }\n            if (wick47IntroDue && fight && wick47Line != null && wick47Line.Now == null) { wick47IntroDue = false; Wick47Say(\"reveal\"); } // G9 idea 2: the hint after the name card\n"],
];
for (const [a, b] of edits) {
  if (t.includes(b)) continue;
  if (!t.includes(a)) throw new Error("MISS " + a.slice(0, 80));
  t = t.replace(a, b);
}
fs.writeFileSync(p, t);
console.log("patched SurvivalSim.N47.Wick.cs");

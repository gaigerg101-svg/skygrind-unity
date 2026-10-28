// NIGHT47 builder W (G9 round 4): the one-line hooks for wickRideParry47 in Core/Enemies/Wick37.cs and the ride-parry members in Wick37.N47.cs
// (anchored; idempotent). Usage (worktree root): node Tools/night/w47-g9-patch.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
function patch(rel, edits) {
  const p = path.join(ROOT, rel);
  let t = fs.readFileSync(p, "utf8");
  const E = t.includes("\r\n") ? "\r\n" : "\n";
  for (const [a0, b0] of edits) {
    const a = a0.replace(/\n/g, E), b = b0.replace(/\n/g, E);
    if (t.includes(b)) continue;
    if (!t.includes(a)) throw new Error("MISS " + rel + ": " + a0.slice(0, 80));
    t = t.replace(a, b);
  }
  fs.writeFileSync(p, t);
  console.log("patched " + rel);
}
patch("Assets/Scripts/Core/Enemies/Wick37.cs", [
  ["        public bool alive, back;\n", "        public bool alive, back;\n        public bool ride47; // NIGHT47 W (wickRideParry47): parried from a ride: it cracks two plates\n"],
  ["            ob.vel = home * (p.returnSpeed / len);\n", "            ob.vel = home * (p.returnSpeed / len);\n            if (w47 && rideParry47 && RidingNow) RideParry47(ob); // NIGHT47 W (wickRideParry47; Wick37.N47.cs): the ride + parry combine\n"],
  ["                        crackLeft[plate] = S(p.crackSeconds);\n", "                        crackLeft[plate] = S(p.crackSeconds);\n                        if (o.ride47) RideCrack47(plate); // NIGHT47 W (wickRideParry47): the next plate cracks too\n"],
]);
patch("Assets/Scripts/Core/Enemies/Wick37.N47.cs", [
  ["        void StartFight47()\n        {", `        /// <summary>NIGHT47 W G9 (wickRideParry47; the sim sets these before StartFight): a parry from a ride returns the ember speedMult x faster
        /// and it cracks \`cracks\` plates (the one it hits and the next).</summary>
        public bool rideParry47;
        public double rideMult47 = 1.5;
        public int rideCracks47 = 2, rideParries47, rideCracksMade47;

        void RideParry47(Wick37Ember ob)
        {
            ob.vel = ob.vel * rideMult47;
            ob.ride47 = true;
            rideParries47++;
        }

        void RideCrack47(int plate)
        {
            for (int k = 1; k < rideCracks47; k++)
            {
                int q = (plate + k) % Plates;
                crackLeft[q] = S(p.crackSeconds);
                rideCracksMade47++;
                fx.stage = "crack";
                fx.plate = q;
                fx.at = Centre;
                events.Emit("wick37Crack", fx);
            }
        }

        void StartFight47()
        {`],
  ["            phases47 = geysers47 =", "            rideParries47 = rideCracksMade47 = 0;\n            phases47 = geysers47 ="],
]);

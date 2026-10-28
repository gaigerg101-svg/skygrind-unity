// NIGHT74 R: variants of the iceline g5 room file (the winner is written into rooms/g5.json by hand with the Edit tool).
// Usage: node Research/2026-10-27-wave74/R/g5-exp.cjs <name[+name]> <in.json> <out.json>
const fs = require("fs");
const [name, inp, out] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(inp, "utf8"));
const r = f.row;
const stack = () => f.boxes.find((b) => b.max[1] === 12 && b.min[0] === -1.5 && b.max[0] === 2.5);
const E = {
  base() {},
  // seed 6 of the verb-aware flow capped with one drone alive at (4.6, 11.4, -146.4), against the north-east corner of THE ICE STACK (a pillar to the roof):
  // the stack back to the shipped room's place (z -145..-141), one metre south
  stackShipped() { const s = stack(); s.min[2] = -145; s.max[2] = -141; },
  // the stack 10 m, a 2 m gap under the 12 m roof for the drones
  stack10() { const s = stack(); s.max[1] = 10; },
  stack8() { const s = stack(); s.max[1] = 8; },
};
for (const n of name.split("+")) {
  if (!E[n]) { console.error("unknown experiment " + n + " (" + Object.keys(E).join(", ") + ")"); process.exit(2); }
  E[n]();
}
fs.writeFileSync(out, JSON.stringify(f, null, 1) + "\n");
console.log("wrote " + out + " (" + name + ")");

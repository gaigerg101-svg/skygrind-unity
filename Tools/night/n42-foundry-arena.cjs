// NIGHT42 builder L1 (G12 through K1's arena kit, design.n42.arena42; the forms in Research/2026-10-14-wave42/K1.md): level 1's arena elements, written
// into the GENERATED level JSON (top-level keys `grinders42` / `cover42`; levelgen does not carry them from the plan). Read by K1's
// SurvivalSim.N42.Arena.cs only while arena42 is on; with it off the keys are never read (n42-foundry otherwise unchanged).
// Placed in b1 THE YARD (rect x -18..18, z -180..-140; galleries x +-12..18 at z -172..-152, 2.5 m; their ramps x +-15 at z -146..-152; walls
// x -4.6..-4 z -164..-148 and x 4..4.6 z -174..-160; door d6 x -2..2 at z -180), checked against those boxes: none overlaps.
// Usage (Tools/night/n42-foundry-regen.sh runs it after levelgen): node Tools/night/n42-foundry-arena.cjs Design/levels/n42/foundry.json
const fs = require("fs");
const file = process.argv[2];
if (!file) throw new Error("usage: n42-foundry-arena.cjs <level.json>");
const lv = JSON.parse(fs.readFileSync(file, "utf8"));
if (lv.id !== "n42-foundry" && lv.name !== "n42-foundry" && !JSON.stringify(lv).slice(0, 4000).includes("n42-foundry")) console.warn("note: the file does not name n42-foundry");
lv.grinders42 = [
  // the far south-east corner (x 14..17.5, z -179..-175.5), flush on the floor, south of the east gallery's end (z -172): off every walker's
  // straight line to the player (K1: a walker whose line crosses a grinder waits at its rim). Measured (`n42level walk --arena42`, 5 seeds):
  // at [6, -179, 10, -175] (2 m off the gallery, the first placement) b1's median 43.4 -> 87.3 s (3 seeds) with 2 deaths in 10; here 43.5 s, 0 deaths
  { rect: [14, -179, 17.5, -175.5], y: 0, room: "b1" },
];
lv.cover42 = [
  // west of the west wall (its west face x -4.6), 1.5 x 1.4 x 1.5 m: cover from the west gallery, 3.5 m from the spot at (-8, -156)
  { min: [-7.5, 0, -161], max: [-6, 1.4, -159.5], hits: 6, room: "b1" },
  // east of the centre line near the entry, south of the east ramp's foot (z -152) and clear of the east wall (z -174..-160)
  { min: [4.5, 0, -152], max: [6, 1.4, -150.5], hits: 6, room: "b1" },
];
fs.writeFileSync(file, JSON.stringify(lv));
console.log(`arena42 elements written into ${file}: 1 grinder, 2 cover boxes (room b1)`);

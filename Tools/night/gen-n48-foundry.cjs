// NIGHT48 builder W (level row climbTop48: n47-foundry -> n48-foundry; NIGHT48.md G1, the climb's top third). Written from gen-n47-foundry.cjs.
// Level 1 as n48-foundry: n47-foundry's level (Design/levels/n47-foundry.json, untouched) copied whole, with DATA added to one room, 1.11 THE
// STACK (a9). THE STACK's geometry is not in the level file: Core/Enemies/Wick37Stack.cs builds it at load from config. The new key
//   climbTop48: the room's marker: SurvivalSim.N48.Wick.cs gives the stack its LIPS (a catch shelf at the far end of each top-third panel, the
//               numbers in design.n48.climbTop48.numbers) when the loaded level's room row carries it. Only this file carries it, so with the
//               level row off (n47-foundry loaded), --off48 or any older switch, the stack is NIGHT47's bit for bit.
// Every other room, door, light and prop, and a9's every other key, is n47-foundry's bit for bit (asserted below). Writes both copies
// (Design/levels and StreamingAssets), byte-identical. Run from the worktree root: node Tools/night/gen-n48-foundry.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n47-foundry.json");
const OUTS = ["Design/levels/n48-foundry.json", "Assets/StreamingAssets/Design/levels/n48-foundry.json"].map((p) => path.join(ROOT, p));

const raw = fs.readFileSync(SRC, "utf8");
const lvl = JSON.parse(raw);
const before = JSON.parse(raw);
if (lvl.name !== "n47-foundry") throw new Error("unexpected source " + lvl.name);
lvl.name = "n48-foundry";
lvl._why = "NIGHT48 builder W (design.n48.climbTop48 swaps n47-foundry for this file while the row is on; NIGHT48.md G1: the climb's top third, GG 2026-10-18 'gradually becoming slightly harder near the top but always remaining fully possible and fluid'): Tools/night/gen-n48-foundry.cjs copies n47-foundry's level whole and adds one key to one room, 1.11 THE STACK (a9): climbTop48, the marker that gives THE STACK its top-third lips. n47-foundry's _why: " + lvl._why;
const rooms = lvl.campaign.rooms;
const a9 = rooms.find((r) => r.id === "a9");
if (!a9 || a9.boss !== "wick" || !a9.wick37 || !a9.wick47) throw new Error("a9 (THE STACK, wick, n47's wick47 note) missing");
a9.climbTop48 = {
  _why: "NIGHT48 W (level row climbTop48; G1): THE STACK's top third gets a LIP at the far end of each top-third panel (after panels 4, 5 and 6 of n47's stack): a catch shelf a little under the rider's feet at the panel's end, against the wall, so a release held past the panel's end lands on it and a short hop rides the next panel. The numbers are config, not this row: design.n48.climbTop48.numbers (lipAfter, lipOver, lipArcDeg, lipStartDeg, lipDepth); SurvivalSim.N48.Wick.cs reads this key's presence only.",
  lips: true,
};
// the guard: every other room, a9's other keys and every top-level key bit for bit
for (let i = 0; i < rooms.length; i++) if (rooms[i].id !== "a9" && JSON.stringify(rooms[i]) !== JSON.stringify(before.campaign.rooms[i])) throw new Error("room changed: " + rooms[i].id);
const a9b = before.campaign.rooms.find((r) => r.id === "a9");
for (const k of Object.keys(a9b)) if (JSON.stringify(a9b[k]) !== JSON.stringify(a9[k])) throw new Error("a9." + k + " changed");
for (const k of Object.keys(a9)) if (!(k in a9b) && k !== "climbTop48") throw new Error("a9." + k + " added");
for (const k of Object.keys(before)) if (!["_why", "name", "campaign"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(lvl[k])) throw new Error(k + " changed");
for (const k of Object.keys(before.campaign)) if (k !== "rooms" && JSON.stringify(before.campaign[k]) !== JSON.stringify(lvl.campaign[k])) throw new Error("campaign." + k + " changed");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
// the source's own layout: compact when it is one line, else 1-space indentation (gen-n47-foundry's writer)
const compact = !raw.trimEnd().includes("\n");
const text = compact ? JSON.stringify(lvl) + (raw.endsWith("\n") ? "\n" : "") : JSON.stringify(lvl, null, 1).replace(/\n/g, eol) + eol;
for (const o of OUTS) fs.writeFileSync(o, text);
if (fs.readFileSync(OUTS[0], "utf8") !== fs.readFileSync(OUTS[1], "utf8")) throw new Error("copies differ");
console.log("wrote n48-foundry (both copies identical): a9's climbTop48 marker; every other room and key bit for bit (n47-foundry's)");

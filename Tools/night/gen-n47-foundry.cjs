// NIGHT47 builder W (level row foundry47: n46-foundry -> n47-foundry; NIGHT47.md G1-G3 / G8; W-cards.md section 3, the arena card + flow plan).
// Level 1 as n47-foundry: n46-foundry's GENERATED level (Design/levels/n46-foundry.json, untouched) copied whole, with DATA changes in one room,
// 1.11 THE STACK (a9). THE STACK's geometry is not in the level file: Core/Enemies/Wick37Stack.cs builds it at load at the room's
// wick37.centre from config (design.n37.wick37.stack, and with wick47 on design.n47.wick47.numbers.stack: the rise per section, 12 panels, the
// cap at 27.3 m; the P1 floor's vents; the glass), so no levelgen run is needed and no box, ramp or lava of the level moves. Changed in a9:
//   name / job / verb / par: the three phases (P1 THE FLOOR: the parry; P2 THE CLIMB: the ride; P3 THE CAP: the punch); par 22 -> 150 s
//     (the human band's top: NIGHT47.md section 3, 90-150 s median)
//   wick47: the room's note of the three-phase fight and where each number lives (the sim reads config, not this row)
//   frames47: the reel a147's camera spots (feet x, y, z, yaw deg, pitch): P1 from the entry, P1 mid-floor, P2 mid-climb, the cap edge, P3
// Every other room, door, light and prop is n46-foundry's bit for bit (asserted below). Writes both copies (Design/levels and StreamingAssets).
// Run from the worktree root: node Tools/night/gen-n47-foundry.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n46-foundry.json");
const OUTS = ["Design/levels/n47-foundry.json", "Assets/StreamingAssets/Design/levels/n47-foundry.json"].map((p) => path.join(ROOT, p));

const raw = fs.readFileSync(SRC, "utf8");
const lvl = JSON.parse(raw);
const before = JSON.parse(raw);
if (lvl.name !== "n46-foundry") throw new Error("unexpected source " + lvl.name);
lvl.name = "n47-foundry";
lvl._why = "NIGHT47 builder W (design.n47.foundry47 swaps n46-foundry for this file while the row is on; GG 2026-10-18: the WICK fight in three phases, the climb forgiving): Tools/night/gen-n47-foundry.cjs copies n46-foundry's level whole and changes the data of one room, 1.11 THE STACK (a9): its job, verb, par, the wick47 note and frames47. n46-foundry's _why: " + lvl._why;
const rooms = lvl.campaign.rooms;
const a9 = rooms.find((r) => r.id === "a9");
if (!a9 || a9.boss !== "wick" || !a9.wick37) throw new Error("a9 (THE STACK, wick) missing");
const c = a9.wick37.centre; // [0, 0, -382]
a9.job36 = "boss";
a9.verb36 = "P1 the parry on the floor, P2 the climb (ride, leap), P3 the punch at the open core (the exam)";
a9.par = 150;
a9.par_why = "NIGHT47 W: the human band's top (NIGHT47.md section 3: median 90-150 s), was 22 (the bot's).";
a9.wick47 = {
  _why: "NIGHT47 W (rows wick47 / climb47 / wickTop47; W-cards.md section 1): THE WICK in three phases in this shaft: P1 THE FLOOR (the panels folded into the wall, 3 slag vents in the 9 m floor), P2 THE CLIMB (at 55 % the steps and the panels come out of the wall: 5 plain STEPS 1.4 m apart (a jump, no ride: round 3), then 8 ride panels from ledge 1 (the last step, 7 m) rising 2.5 / 2.8 m, ledge 2 at 16.2 m, the net), P3 THE CAP at 26.3 m (the glass slides over the shaft; the crack-open charge). The numbers are config, not this row: design.n47.wick47.numbers (the stack, the vents, the attacks) and design.n47.climb47.numbers; this row is the room's note for the reels and docs/ROOMS.md.",
  phases: ["P1 THE FLOOR", "P2 THE CLIMB", "P3 THE CAP"],
};
// the reel's camera spots: [x, y, z, yawDeg, pitch] in level coordinates (feet), from the stack's centre (0, 0, -382)
const at = (x, y, z, yaw, pitch) => [c[0] + x, c[1] + y, c[2] + z, yaw, pitch];
a9.frames47 = {
  _why: "NIGHT47 W: reel a147's spots (the lead's frames): RED on n47-r1 with the rows off, GREEN with them on.",
  p1Entry: at(0, 0, 10.5, 0, 0.15),
  p1Floor: at(0, 0, 5, 0, 0.35),
  p2Climb: at(-7.5, 1.45, 0, 180, 0.3),
  capEdge: at(0, 26.3, 11, 0, 0.1),
  p3Cap: at(0, 26.3, 6, 0, 0.1),
};
// the guard: every other room and every top-level key bit for bit
for (let i = 0; i < rooms.length; i++) if (rooms[i].id !== "a9" && JSON.stringify(rooms[i]) !== JSON.stringify(before.campaign.rooms[i])) throw new Error("room changed: " + rooms[i].id);
for (const k of Object.keys(before)) if (!["_why", "name", "campaign"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(lvl[k])) throw new Error(k + " changed");
for (const k of Object.keys(before.campaign)) if (k !== "rooms" && JSON.stringify(before.campaign[k]) !== JSON.stringify(lvl.campaign[k])) throw new Error("campaign." + k + " changed");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
// the source's own layout: compact when it is one line, else 1-space indentation (gen-n46-foundry's plan writer)
const compact = !raw.trimEnd().includes("\n");
const text = compact ? JSON.stringify(lvl) + (raw.endsWith("\n") ? "\n" : "") : JSON.stringify(lvl, null, 1).replace(/\n/g, eol) + eol;
for (const o of OUTS) fs.writeFileSync(o, text);
if (fs.readFileSync(OUTS[0], "utf8") !== fs.readFileSync(OUTS[1], "utf8")) throw new Error("copies differ");
console.log("wrote n47-foundry (both copies identical): a9's job / verb / par 150 / wick47 / frames47; every other room bit for bit");

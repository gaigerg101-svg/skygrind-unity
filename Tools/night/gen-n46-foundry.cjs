// NIGHT46 builder A3 (design.n46.foundry46; NIGHT46.md card A3 "levels 1 and 2 smoothed"; GG 2026-10-17: "need to feel and flow better ... study real game
// deisgn and areana layouts"; DECIDE46 N-11; DESIGN-RULES A-1 / A-4 / A-5 / A-9). Level 1 as n46-foundry: n45-foundry's plan (Design/levels/n45/foundry.plan.json,
// written by Tools/night/gen-n45-foundry.cjs, which this file does not run or edit) copied whole, with DATA changes in four rooms, each named by its
// FLOW-AUDIT row (Research/2026-10-17-wave46/FLOW-AUDIT.md) and its line in docs/ROOMS.md's "NIGHT46 A3" change list:
//   F-1 a3 RIDE (row 10, a linear room): the lava stops 2 m short of the west wall, so the walk goes round both sides (a loop round the pit).
//   F-2 b1 THE SPILL (row 12, wave 1 out of sight; the lead's card-gate ruling 23:20): wave 1 on the ladle's rim front, in line of sight from the door
//       (the crane rails hide both bridges from the door: los.cjs 0 of 10 spots); its husks come down the ramps and over the bridges. NIGHT45's card stays.
//   F-3 a8 PIERCE (row 4's data half, DECIDE46 N-11): lesson 0's "reached" rect moves from the perch to P1's foot (the ride's start, 4.3 m in).
//   F-4 b2 THE GANTRY (row 9, 104 m with no fight): three husks on gate 1's landing, after its lesson (the landing reached).
// Every other room, door, light and prop is n45-foundry's bit for bit (asserted below).
// Run from the repo (or worktree) root: bash Tools/night/n46-foundry-regen.sh
// Probes: N46F_PLAIN=1 writes n45's plan with only the outer name changed (the plan-copy check); N46F_B1W1=mixed puts two of wave 1's nodes back on the
// bridges' far ends; a probe must write to N46F_OUT (never the shipped plan).
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n45/foundry.plan.json");
const OUT = path.join(ROOT, "Design/levels/n46/foundry.plan.json");
const OUT_FILE = process.env.N46F_OUT ? path.resolve(ROOT, process.env.N46F_OUT) : OUT;
const PLAIN = process.env.N46F_PLAIN === "1";
const B1W1 = process.env.N46F_B1W1 || "rim";
const probe = PLAIN || B1W1 !== "rim";
if (probe && OUT_FILE === OUT) throw new Error("a probe (N46F_PLAIN / N46F_B1W1) must write to N46F_OUT, not the shipped plan");

const srcText = fs.readFileSync(SRC, "utf8");
const plan = JSON.parse(srcText);
const before = JSON.parse(srcText);
if (plan.id !== "n45-foundry") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n46-foundry";
const room = (id) => { const r = plan.rooms.find((q) => q.id === id); if (!r) throw new Error(`room ${id} missing`); return r; };
const same = (a, b, what) => { if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(`${what} changed upstream: ${JSON.stringify(a)}`); };
const MINE = ["a3", "b1", "a8", "b2"];
const log = [];

if (!PLAIN) {
  plan._why = "NIGHT46 builder A3 (design.n46.foundry46 swaps n45-foundry for this file while the row is on; GG 2026-10-17 \"need to feel and flow better\"; "
    + "DECIDE46 N-11): Tools/night/gen-n46-foundry.cjs copies n45-foundry's plan whole and smooths four rooms by data, each named by its FLOW-AUDIT row "
    + "(docs/ROOMS.md, NIGHT46 A3): a3's lava 2 m short of the west wall, b1's wave 1 on the ladle's rim in view, a8's ride lesson at P1's foot, a small fight "
    + "on b2's gate 1 landing. n45-foundry's _why: " + plan._why;

  // ---- F-1 a3 RIDE (FLOW-AUDIT row 10): the pit x -8..6 -> -6..6; the 2 m dry strip on the west wall + the 2 m east bridge = a walk round both sides
  {
    const a3 = room("a3");
    const p = plan.pits.filter((q) => q.zone === "a3");
    if (p.length !== 1) throw new Error("a3: expected one pit");
    same(p[0].rect, [-8, -81, 6, -72], "a3's pit");
    p[0].rect = [-6, -81, 6, -72];
    p[0].why = "the RIDE's hazard (12 x 9 m; NIGHT46 A3 F-1, FLOW-AUDIT row 10: 2 m short of the west wall, so a walk goes round both sides: the 2 m west strip "
      + "and the 2 m east bridge, a loop round the pit) and the husks' grave: a dash-punch launches them in from either walk (A-5); the ride over it on P2 "
      + "stays the short way";
    p[0].note = p[0].why;
    a3.foundry46_why = "NIGHT46 A3 F-1 (FLOW-AUDIT row 10): the lava x -8..6 -> -6..6 (z -81..-72 kept): a 2 m walk on each side (loop y); P1 / P2, the lesson and the waves unchanged";
    log.push("a3 pit x -6..6");
  }

  // ---- F-2 b1 THE SPILL (FLOW-AUDIT row 12; the card gate's ruling): wave 1 in line of sight from the door
  {
    const b1 = room("b1");
    same(b1.waves, [["husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk"], ["husk", "husk", "husk", "husk"]], "b1's waves");
    same(b1.waveNodes43[0], [[-11.5, 0, -172], [11.5, 0, -172], [-11.5, 0, -176], [11.5, 0, -176], [-15, 0, -169], [15, 0, -169]], "b1's wave 1 nodes");
    // los.cjs on n45-foundry.json, eye (0, 1.6, -142) / the commit point (0, 1.6, -145), body centre 1.0 m over the feet: every rim spot below LOS yes
    // (16.6-17.4 m @ 15.7-18.4 deg from the door); the bridges' far ends (+-11.5, -164.5) NO LOS (the 7 m crane rails x +-8.8..9.4, z -169..-151)
    const RIM = [[-5.5, 2.5, -158.5], [5.5, 2.5, -158.5], [-4.5, 2.5, -158], [4.5, 2.5, -158]];
    const BRIDGE_ENDS = [[-11.5, 0, -164.5], [11.5, 0, -164.5]];
    b1.waveNodes43[0] = B1W1 === "mixed" ? RIM.concat(BRIDGE_ENDS) : RIM;
    b1.waveNodes43_why = "NIGHT46 A3 F-2 (FLOW-AUDIT row 12; the lead's card gate 23:20: >= 2 of wave 1 in line of sight from the door): wave 1 on the LADLE's RIM front "
      + "(4 spots at x +-4.5..5.5, z -158..-158.5, 2.5 m up: all in line of sight from the door and the commit point, los.cjs), so the room reads occupied on entry; "
      + "the husks come down the rim ramps (ai33: never into the slag) and over the bridges as the card's files. The far bank and both bridges are hidden from the "
      + "door by the crane rails (the card's ride line, kept). Waves 2 / 3 as NIGHT45 A1: " + b1.waveNodes43_why;
    b1.foundry46_why = `NIGHT46 A3 F-2: wave 1's nodes ${B1W1 === "mixed" ? "rim front x4 + the bridges' far ends x2" : "the rim front x4"} (was the far bank x6, 0 of 6 in line of sight); no geometry change`;
    log.push(`b1 wave 1 ${B1W1}`);
  }

  // ---- F-3 a8 PIERCE (FLOW-AUDIT row 4, DECIDE46 N-11): lesson 0 at P1's foot (grounded on the entry floor, 4.3 m from the spawn)
  {
    const a8 = room("a8");
    for (const k of ["lessons", "lessons31"]) {
      same(a8[k][0].rect, [-1, -357.5, 4, -353], `a8's ${k}[0] rect`);
      if (a8[k][0].kind !== "reached") throw new Error("a8: lesson 0 is not a reached lesson");
      a8[k][0].rect = [-7, -345.2, -2.5, -343];
    }
    same(a8.afterLesson43, 0, "a8's afterLesson43");
    a8.foundry46_why = "NIGHT46 A3 F-3 (FLOW-AUDIT row 4, DECIDE46 N-11): lesson 0 (\"HOLD {jump} ON A WALL: RIDE\", reached = grounded in the rect) from the perch "
      + "[-1,-357.5,4,-353] to P1's foot on the entry floor [-7,-345.2,-2.5,-343] (4.3 m from the spawn): the husks leave the hatch as the ride starts "
      + "(afterLesson43 0), not 20 s later; VERBS.md row 9 (wall ride) unchanged: only this prompt shortens (the ride is taught in a3, chained in b2)";
    log.push("a8 lesson 0 at P1's foot");
  }

  // ---- F-4 b2 THE GANTRY (FLOW-AUDIT row 9): three husks on gate 1's landing (the ledge between the lavas), after its lesson
  {
    const b2 = room("b2");
    if (b2.waves || b2.waveNodes43 || b2.afterLesson43 !== undefined) throw new Error("b2 already has a fight");
    same(b2.lessons[0].rect, [-10, -241, 10, -229], "b2's lesson 0 rect");
    // the landing: deck x -10..10, z -241..-229, top 0 (gate 1's long wall lands at x -2.2, z ~-231); gate 2's lava from z -241, gate 1's to z -229
    // los.cjs from the long wall's landing (-2.2, 0, -231): 4 of these in view and in line of sight (11.3-14.4 m @ -37..51 deg); round 1's fifth spot
    // (8, -230.5) stood 93 deg beside the landing player (a behind-spawn, Part D 8): replaced by (2, -240)
    // round 2 (n46smooth b2): Campaign24.SpawnWave skips spots under minSpawnDist (12 m) from the feet, and only (9, -240) was 12 m from the landing
    // point: all three husks came out of one spot. These four are 12.2-14.4 m from (-2.2, -231), in view (-38..51 deg), 0.4 m short of gate 2's lava
    const LANDING = [[9, 0, -240], [7.5, 0, -240.6], [6, 0, -240.6], [-9.8, 0, -240.6]];
    b2.waves = [["husk", "husk", "husk"]];
    b2.nodes = LANDING;
    b2.waveNodes43 = [LANDING];
    b2.afterLesson43 = 0;
    b2.afterLesson43_why = "NIGHT46 A3 F-4 (FLOW-AUDIT row 9: b1 -> a8 was 162 m with no fight): the husks come when lesson 0 (\"HOLD {jump} ON THE WALL\", the "
      + "landing reached) is met, so they meet you on the ledge the long wall lands on, never wait at its far lip while you are still on the wall";
    b2.waveNodes43_why = "NIGHT46 A3 F-4: gate 1's LANDING (20 x 12 m between the two lavas), its far half (z -240..-240.6, the lip of gate 2), all 12 m or more from and in view of where "
      + "the long wall lands (los.cjs: 12.2-14.4 m): a small fight with lava both ways: punch or launch them in (A-5, Lava30); they never "
      + "walk into it themselves (ai33). each body picks one at random (none skipped by minSpawnDist 12)";
    b2.foundry46_why = "NIGHT46 A3 F-4: waves [husk x3] on gate 1's landing (5 spots), afterLesson43 0; no geometry change; the lessons, the gates and fallReturn43 kept";
    log.push("b2 husk x3 on gate 1's landing");
  }

  // ---- the guard: every other room and every piece outside the four rooms bit for bit; inside them, only the rows named above
  for (const k of ["boxes", "ramps", "pits", "tunnels", "pads", "grapple", "lights", "props", "surfaces"]) {
    const zone = (e) => e.zone || e.room;
    const a = (before[k] || []).map((e) => JSON.stringify(e)), b = (plan[k] || []).map((e) => JSON.stringify(e));
    if (a.length !== b.length) throw new Error(`${k}: a row was added or removed`);
    for (let i = 0; i < a.length; i++) if (a[i] !== b[i] && !(k === "pits" && zone(plan[k][i]) === "a3")) throw new Error(`${k}[${i}] changed (${zone(plan[k][i])})`);
  }
  for (let i = 0; i < plan.rooms.length; i++) if (!MINE.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
  for (const k of Object.keys(before)) if (!["_why", "id", "rooms", "pits"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
}

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT_FILE), PLAIN ? "(PLAIN: n45-foundry's plan, the outer name only)" : "| " + log.join("; "));

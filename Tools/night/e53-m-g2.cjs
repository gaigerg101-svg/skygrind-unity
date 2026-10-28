// EXT53 builder M (row M, THE MIRRORS): iceline g2 THE SHIELD breaks its mirror. NIGHT52 built it a mirror across the door's axis x = 0 in
// every big piece (two ride panels, two 1.5 m decks with their ramps, two 4 m gantries with their ramps, two springs, two lane springs, two
// pillars): I1 100 %, the worst in the level (DECIDE53 D-13 / D-14). The 09:00 reviews: the P-rank one (cut the east panel; the east 1.5 m
// ramp and its deck to floor; a loop over the warden), the GG persona (cut the east pillar so the west is the flanking side; the south wall
// 5 m north: "smaller and more action packed"). The lesson ("{fire} THE SHIELD, OR FLANK IT") gets a side for each answer:
//   THE WEST IS THE FLANK (kept as built): the one ride panel, its hidden lane behind it up the gantry ramp, the spring over the shield onto
//   the north deck, the gantry over the warden's back with the health, the pillar to step round.
//   THE EAST IS THE FACE (re-cut): no panel, no pillar, no deck: the warden's yard is open floor where you meet the shield and punch it.
//   1. The EAST WALL moves 3 m in (rect x 12 -> 9; the room 24 -> 21 x 28 m): the GG persona's "smaller" taken on the wall with no door
//      (the south wall carries d1 and its link: not moved, as the lead's brief allows).
//   2. Cut: the east ride panel, the east 1.5 m deck and its ramp, the east spring (its landing deck gone), the east pillar.
//   3. The EAST GANTRY narrows to 4 x 5 m (x 5..9) against the new wall, its ramp along that wall (x 6.6..9), its walk proof, the lane
//      spring and the high node with it: the husks' file still comes down the east at you (every tier keeps its walkers' route).
//   4. Added: THE QUENCH TANK (1.2 m, east door half: the face side's vault cover, the charger's stop that the east pillar was), THE RUN-UP
//      STEP (0.8 m, west door half: the jump into the one ride), THE YARD SPRING (a pad in the warden's back yard over the exit onto the west
//      gantry: the P-rank's loop over the warden, by air: a ledge along the north wall would cross the exit door d2, 4.5 m high).
//   Not taken: moving the west panel onto the west pillar's face (P-rank): the panel would cross the west spring's arc and the west deck's
//   ramp (three pieces re-cut with their proofs); its lane behind it is already the hidden flank.
// Asserts g2 is NIGHT52's fix K2 (+ NIGHT53's frames52); every other room bit for bit.
// Run: node Tools/night/e53-m-g2.cjs [--dry], then bash Tools/night/n52-loop-regen.sh iceline g2
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const g2 = plan.rooms.find((r) => r.id === "g2");
if (!/fix K2/.test(g2.loop52_why || "") || /EXT53/.test(g2.loop52_why)) throw new Error("g2 is not NIGHT52's fix K2 (or already run)");
if (!same(g2.rect, [-12, -58, 12, -30])) throw new Error("g2's rect " + JSON.stringify(g2.rect));
const Z = (b) => b.zone === "g2";

// 1. the east wall 3 m in
g2.rect = [-12, -58, 9, -30];

// 2. the cuts
const cutBoxes = [
  one(plan.boxes, (b) => Z(b) && b.ride && same(b.min, [9, 0, -53]) && same(b.max, [9.6, 10, -35]), "the east ride panel"),
  one(plan.boxes, (b) => Z(b) && same(b.min, [2, 0, -58]) && same(b.max, [7, 1.5, -53]), "the east 1.5 m deck"),
];
const cutRamp = one(plan.ramps, (r) => Z(r) && same(r.from, [4.5, -48]) && same(r.to, [4.5, -53]), "the east 1.5 m ramp");
const cutPad = one(plan.pads, (p) => Z(p) && same(p.at, [6.5, 0, -37]), "the east spring");
const cutProp = one(plan.props, (q) => q.id === "g2-P2" && q.room === "g2" && same(q.at, [4, 2, -40]), "the east pillar");
plan.boxes = plan.boxes.filter((b) => !cutBoxes.includes(b));
plan.ramps = plan.ramps.filter((r) => r !== cutRamp);
plan.pads = plan.pads.filter((p) => p !== cutPad);
plan.props = plan.props.filter((q) => q !== cutProp);

// 3. the east gantry, its ramp, its walk proof, its lane spring, its high node
const eg = one(plan.boxes, (b) => Z(b) && same(b.min, [7, 0, -58]) && same(b.max, [12, 4, -53]), "the east gantry");
eg.min = [5, 0, -58];
eg.why = "THE EAST GANTRY (4 m, 4 x 5 m against the east wall; EXT53 e53-m-g2.cjs: narrowed with the wall 3 m in): the husks' perch over the face side; they file down its ramp at you while the warden holds the yard; drop off it behind the warden";
eg.note = eg.why;
const er = one(plan.ramps, (r) => Z(r) && same(r.from, [10.8, -44]) && same(r.to, [10.8, -53]), "the east gantry ramp");
er.from = [7.8, -44];
er.to = [7.8, -53];
er.why = "the EAST GANTRY RAMP (4 m over 9 m, along the east wall; EXT53: moved 3 m in with the wall): the husks' climb and file down the face side, the walkers' way to the east gantry";
er.note = er.why;
const ep = one(plan.pads, (p) => Z(p) && same(p.at, [10.8, 0, -36.5]), "the east lane spring");
ep.at = [7.8, 0, -36.5];
ep.target = [7, 4, -55.5];
ep.why = "the EAST LANE SPRING (a jump pad at the east lane's mouth, beside the quench tank): up onto the east gantry over the husks' ramp: the face side's way to the height (EXT53: 3 m in with the wall)";
const pf = g2.proofs[3];
if (!same(pf.points, [[10.8, 0, -39.5], [10.8, 0, -43], [10.8, 4, -53.5], [9.5, 4, -55.5]])) throw new Error("g2's east gantry walk proof " + JSON.stringify(pf.points));
pf.points = [[7.8, 0, -39.5], [7.8, 0, -43], [7.8, 4, -53.5], [7, 4, -55.5]];
if (!same(g2.highNodes, [[-9.5, 4, -55.5], [9.5, 4, -55.5]])) throw new Error("g2's high nodes " + JSON.stringify(g2.highNodes));
g2.highNodes = [[-9.5, 4, -55.5], [7, 4, -55.5]];
g2.nodes_why = (g2.nodes_why || "") + "; EXT53 (e53-m-g2.cjs): the east gantry's high node 2.5 m west with the gantry (the east wall 3 m in)";

// 4. the added pieces (after the room's last box / pad)
const lastBox = plan.boxes.map((b, i) => (Z(b) ? i : -1)).filter((i) => i >= 0).pop();
const addBoxes = [
  { zone: "g2", kind: "hull", min: [2.5, 0, -39], max: [6, 1.2, -36.5], job: "cover", why: "THE QUENCH TANK (1.2 m, 3.5 x 2.5 m, the east door half; EXT53): the face side's vault cover: the warden comes round it at you and you punch the shield over its lip; the charger (wave 3) breaks its charge on it (the east pillar's job, NIGHT47 B G25)" },
  { zone: "g2", kind: "hull", min: [-9, 0, -34], max: [-7.6, 0.8, -31.5], job: "line", why: "THE RUN-UP STEP (0.8 m, 1.4 x 2.5 m, the west door half; EXT53): hop it and jump into the west panel's ride: the fast flank starts at the door" },
];
for (const b of addBoxes) b.note = b.why;
plan.boxes.splice(lastBox + 1, 0, ...addBoxes);
const lastPad = plan.pads.map((p, i) => (Z(p) ? i : -1)).filter((i) => i >= 0).pop();
plan.pads.splice(lastPad + 1, 0, { zone: "g2", at: [3.5, 0, -55.5], target: [-9.5, 4, -55.5], apexAbove: 3, job: "loop", why: "THE YARD SPRING (a jump pad in the warden's back yard, east of the exit; EXT53): over the exit onto the west gantry and its health: the loop over the warden for whoever got behind it (the P-rank's loop, by air)", note: "the yard spring" });

// the room's record
g2.loop52_why += " EXT53 (e53-m-g2.cjs, builder M): the mirror broken: the west is the flank (its panel, hidden lane, springs, gantry and health kept), the east is the face: the east wall 3 m in (21 x 28 m), the east panel, deck 1.5, its ramp, spring and pillar cut, the east gantry 4 x 5 m with its ramp on the new wall, the quench tank, the run-up step and the yard spring added.";

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== "g2" && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => !x || (x.zone !== "g2" && x.room !== "g2");
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g2 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
const n = (k) => plan[k].filter(Z).length - before[k].filter(Z).length;
console.log("g2: rect " + JSON.stringify(g2.rect) + "; boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads") + ", props " + (plan.props.filter((q) => q.room === "g2").length - before.props.filter((q) => q.room === "g2").length));
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);

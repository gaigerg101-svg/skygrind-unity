// NIGHT52 the room loop, fix K2 (builder K, round 6): iceline g2 2.2 THE SHIELD (24 x 28 m, 1.19 per 100 m2) to THE ROOM CONTRACT, its lesson
// ("{fire} THE SHIELD, OR FLANK IT") and waves (warden; warden + 2 husks; charger + 2 husks) kept. Every piece serves the flank:
//   - the two ride panels run 18 m and to the roof (10 m): ride past the warden on either side; behind each a 2.4 m FLANK LANE runs from the
//     door's end to a walkers' ramp onto a 4 m GANTRY in the north corner (the third tier), each lane with a spring (a pad) at its mouth
//   - the 1.5 m north deck halves keep their ramps (moved 1.5 m in, clear of the gantries); THE ANVIL (a roof-high block mid-room) is the mass
//     the warden turns on: you loop round it to its back; two springs at the south pillars throw you over the warden onto the north decks
//   - the spawn 2.5 m further in (it sat on the door's 4 m dwell band), the warden's nodes north of the anvil, the health on the west gantry
// Asserts 507b49d0's g2. Run: node Tools/night/n52-loop-fix-k2.cjs, then bash Tools/night/n52-loop-regen.sh iceline g2
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const Z = "g2";
const r = plan.rooms.find((x) => x.id === Z);
if (!eq(r.rect, [-12, -58, 12, -30]) || r.height !== 9 || plan.boxes.filter((b) => b.zone === Z).length !== 4) throw new Error("g2 is not 507b49d0's (already run?)");
const TAG = "NIGHT52 fix K2 (n52-loop-fix-k2.cjs)";
const box = (kind, min, max, job, why, extra) => Object.assign({ zone: Z, kind, min, max, job, why, note: why }, extra || {});
const ramp = (from, to, width, rise, why) => ({ zone: Z, from, to, width, rise, job: "climb", why, note: why });
r.height = 10;
r.spawn = { at: [0, 0, -34.5], yawDeg: 0 };
r.spawn_why = `${TAG}: 4.5 m in from the door d1 (it stood 2 m in, on the door's 4 m dwell band): a retry starts you facing the anvil, not in the doorway`;
r.nodes = [[0, 0, -49.5], [0, 0, -51.5]];
r.highNodes = [[-9.5, 4, -55.5], [9.5, 4, -55.5]];
r.nodes_why = `${TAG}: the warden's ground north of the anvil (it turns on the anvil while you loop round); the husks of waves 2 / 3 from the two gantries (4 m), down their ramps and lanes`;
r.proofs = [
  { kind: "walk", points: [[0, 0, -34.5], [0, 0, -38], [-2, 0, -42], [-2.5, 0, -47.5], [0, 0, -51], [0, 0, -56]] },
  { kind: "walk", points: [[-4.5, 0, -46], [-4.5, 1.5, -54], [-3, 1.5, -55.5]] },
  { kind: "walk", points: [[-10.8, 0, -39.5], [-10.8, 0, -43], [-10.8, 4, -53.5], [-9.5, 4, -55.5]] },
  { kind: "walk", points: [[10.8, 0, -39.5], [10.8, 0, -43], [10.8, 4, -53.5], [9.5, 4, -55.5]] },
];
r.loop52_why = `${TAG}: n47's THE SHIELD (24 x 28 x 9 m, 8 pieces 1.19 per 100 m2, the lead's D-25: an open floor with two pillars) rebuilt to THE ROOM CONTRACT: 24 x 28 x 10 m, 17 pieces, three planes (0 / 1.5 / 4 m) on walkers' ramps, two 18 m ride panels with a flank lane behind each, THE ANVIL to loop round, four springs (two over the warden onto the north decks, two up the flank lanes); the lesson and the waves kept`;
const boxes = [
  box("wall", [-9.6, 0, -53], [-9, 10, -35], "line", "the WEST RIDE PANEL (cyan, 18 m, to the roof): ride past the warden on its left; behind it the west flank lane (2.4 m) runs to the gantry ramp", { ride: true, ride36: { face: "E", planeX: -9, lz: [5, 23] } }),
  box("wall", [9, 0, -53], [9.6, 10, -35], "line", "the EAST RIDE PANEL (cyan, 18 m, to the roof): ride past the warden on its right; behind it the east flank lane", { ride: true, ride36: { face: "W", planeX: 9, lz: [5, 23] } }),
  box("deck", [-7, 0, -58], [-2, 1.5, -53], "line", "the north deck's west half (1.5 m): wave 2's husks come down its ramp; the west spring lands you on it behind the warden"),
  box("deck", [2, 0, -58], [7, 1.5, -53], "line", "the north deck's east half (1.5 m): the east spring's landing behind the warden"),
  box("deck", [-12, 0, -58], [-7, 4, -53], "line", "THE WEST GANTRY (4 m, the third tier, the north-west corner): the end of the west flank lane (its ramp, its spring); the high ground over the warden's back; the health"),
  box("deck", [7, 0, -58], [12, 4, -53], "line", "THE EAST GANTRY (4 m, the north-east corner): the end of the east flank lane; drop off it behind the warden"),
  box("hull", [-1.5, 0, -46], [1.5, 10, -43], "line", "THE ANVIL (3 x 3 m, to the roof, mid-room): the mass the warden turns on: loop round it to its back (the flank), or let it come round and punch the shield"),
];
const ramps = [
  ramp([-4.5, -48], [-4.5, -53], 3, 1.5, "the west half's ramp (1.5 m over 5 m): the husks' way down to you"),
  ramp([4.5, -48], [4.5, -53], 3, 1.5, "the east half's ramp (1.5 m over 5 m)"),
  ramp([-10.8, -44], [-10.8, -53], 2.4, 4, "the WEST GANTRY RAMP (4 m over 9 m, up the west flank lane behind the panel): the walkers' climb to the third tier (no safe ledge); the flank route to the warden's back"),
  ramp([10.8, -44], [10.8, -53], 2.4, 4, "the EAST GANTRY RAMP (4 m over 9 m, up the east flank lane)"),
];
const pads = [
  { zone: Z, at: [-6.5, 0, -37], target: [-4.5, 1.5, -55.5], apexAbove: 3, job: "line", why: "the WEST SPRING (a jump pad by the west pillar): over the warden's shield onto the north deck behind it: the flank by air", note: "the west spring" },
  { zone: Z, at: [6.5, 0, -37], target: [4.5, 1.5, -55.5], apexAbove: 3, job: "line", why: "the EAST SPRING (a jump pad by the east pillar): over the warden onto the north deck behind it", note: "the east spring" },
  { zone: Z, at: [-10.8, 0, -36.5], target: [-9.5, 4, -55.5], apexAbove: 3, job: "line", why: "the WEST LANE SPRING (a jump pad at the west flank lane's mouth): up the lane onto the west gantry: the fast flank", note: "the west lane spring" },
  { zone: Z, at: [10.8, 0, -36.5], target: [9.5, 4, -55.5], apexAbove: 3, job: "line", why: "the EAST LANE SPRING (a jump pad at the east flank lane's mouth): onto the east gantry", note: "the east lane spring" },
];
const swap = (key, isMine, fresh) => {
  const arr = plan[key] || [];
  const at = arr.findIndex(isMine);
  const rest = arr.filter((x) => !isMine(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  plan[key] = rest;
};
swap("boxes", (b) => b.zone === Z, boxes);
swap("ramps", (b) => b.zone === Z, ramps);
swap("pads", (b) => b.zone === Z, pads);
for (const p of plan.props) if (p.room === Z && p.kind === "healthPickup") { p.at = [-9.5, 4.9, -55.5]; p.why = `the high ground pays health: on the west gantry (4 m) at the flank lane's end (${TAG})`; }
for (const l of plan.lights) {
  if (l.room !== Z) continue;
  if (l.kind === "ceiling") l.at = [l.at[0], 9.95, l.at[2]];
  if (l.id === "g2-L2") l.at = [0, 9.95, -50];
}
// the guard: only g2's rows changed
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && !eq(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pads", "props", "lights"]) {
  const keep = (l) => (l || []).filter((b) => b.zone !== Z && b.room !== Z);
  if (!eq(keep(before[k]), keep(plan[k]))) throw new Error(k + ": a row outside g2 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pads", "props", "lights"].includes(k) && !eq(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix-k2: g2 THE SHIELD to the contract (17 pieces)");

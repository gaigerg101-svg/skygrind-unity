// EXT53 builder M (row M, THE MIRRORS): foundry b1 THE SPILL breaks its mirror and opens differently. NIGHT52 F built it a mirror across the
// door's axis x = 0 (two crane rails, two bridges, two 4.5 m crane cabs with their masts, ramps and pads, two gate cheeks, the ladle on the
// axis; only the furnace post off it): I1 99 %; I7: a5 and b1 both open "held" (Design/levels/n52/openings53.json); I4: both cab pads sat
// on the circuit's walk line (0.36 m).
//   THE STORY: the ladle tipped WEST and took the west crane down with it; the east crane still stands. So each side has its own height:
//   1. THE TIPPED LADLE (its 6 m body, its 2.5 m lip, its bail's two arms, the hero props on its mouth) lies 3.5 m west (x -6.5..-0.5):
//      the west rail's jump-off now lands straight on the lip; the exit walk passes east of it.
//   2. THE WEST CRANE is cut (its 4.5 m cab, its mast, its ramp and its pad): the far bank's west is the ladle's lane.
//   3. THE OPENING (I7 held -> ambushed): wave 1's three husks start behind the ladle's body (out of the door's sight: its 2.5 m lip
//      covers them) and come round both its ends and over the bridges at you: the enemies cross the midfield line (the river) to you.
//      Wave 2's west-cab spitter node goes (the cab is gone; its two lip nodes move with the ladle); its two husk nodes leave the ladle's
//      new footprint (the west lane, the east of the far bank).
//   4. THE CAB PAD moves to the river's north bank (x 1.5): onto the east cab behind its spitter, clear of the circuit (I4).
//   5. THE SPILL PAD (new; the room's 16th piece, density 2.50): on the near bank's west, over the slag onto the ladle's lip: meet the file
//      from above, the lip's second way up beside the west rail's jump-off (the walkers' way is the ladle's body).
//   6. The walk proofs follow the new far bank (the circuit round the ladle's back; the ladle walk from the exit 3.5 m west; the west cab's
//      ramp walk cut with its ramp).
// Asserts b1 is u3-n53's (gen-n52-foundry.cjs rebuildB1, round 6); every other room, door and row bit for bit.
// Re-running gen-n52-foundry.cjs rebuilds b1 from rebuildB1 and wipes this: run this script again after it (and n53-k-a3.cjs).
// Run: node Tools/night/e53-m-b1.cjs [--dry]
//      dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/foundry.plan.json Design/levels/n52/foundry.json --walk
//      node Tools/night/gen-n52-foundry.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/foundry.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "b1";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const Z = (o) => o.zone === R;
const room = plan.rooms.find((r) => r.id === R);
if (!same(room.rect, [-10, -180, 10, -148]) || room.ext53_why) throw new Error("b1 is not u3-n53's (or already run)");
const DX = -3.5;
const sx = (v) => [v[0] + DX, ...v.slice(1)];

// 1. the ladle 3.5 m west
const lip = one(plan.boxes, (b) => Z(b) && same(b.min, [-3, 0, -170]) && same(b.max, [3, 2.5, -168.5]), "the ladle's lip");
lip.min = sx(lip.min); lip.max = sx(lip.max);
lip.why = lip.why.replace("THE LADLE's LIP (deck 2.5 m,", "THE LADLE's LIP (deck 2.5 m; EXT53 e53-m-b1.cjs: tipped west, x -6.5..-0.5: the west rail's jump-off lands on it, the spill pad throws onto it;");
lip.note = lip.why;
const armW = one(plan.boxes, (b) => Z(b) && same(b.min, [-3.6, 0, -173]) && same(b.max, [-3, 12, -172]), "the bail's west arm");
const armE = one(plan.boxes, (b) => Z(b) && same(b.min, [3, 0, -173]) && same(b.max, [3.6, 12, -172]), "the bail's east arm");
for (const a of [armW, armE]) { a.min = sx(a.min); a.max = sx(a.max); }
armW.why = armW.why.replace("in play a post beside the ladle's back to circle a husk round in the west lane", "in play a post beside the ladle's back to circle a husk round in the west lane (EXT53: 3.5 m west with the ladle; the lane behind it is where the first file hides)");
armW.note = armW.why;
armE.why = "THE LADLE's BAIL, east arm (card: Landmark): the bail's other arm to the roof (EXT53: 3.5 m west with the ladle), a post to circle on the exit's walk east of the ladle";
armE.note = armE.why;
const body = one(plan.ramps, (r) => Z(r) && same(r.from, [0, -176]) && same(r.to, [0, -170]) && r.width === 6, "the ladle's body");
body.from = [DX, -176]; body.to = [DX, -170];
body.why = body.why.replace("THE TIPPED LADLE's BODY (card: Hero prop;", "THE TIPPED LADLE's BODY (card: Hero prop; EXT53: tipped west, x -6.5..-0.5, its 2.5 m lip hides the first file behind it;");
body.note = body.why;
for (const id of ["b1-hero", "b1-lip"]) {
  const q = one(plan.props, (p) => p.room === R && p.id === id, id);
  if (q.at[0] !== 0) throw new Error(id + " at " + JSON.stringify(q.at));
  q.at = sx(q.at);
}

// 2. the west crane cut
const cut = [
  one(plan.boxes, (b) => Z(b) && same(b.min, [-10, 0, -180]) && same(b.max, [-5.5, 4.5, -174.5]), "the west cab"),
  one(plan.boxes, (b) => Z(b) && same(b.min, [-10, 4.5, -180]) && same(b.max, [-8.5, 12, -178.5]), "the west mast"),
];
plan.boxes = plan.boxes.filter((b) => !cut.includes(b));
const wramp = one(plan.ramps, (r) => Z(r) && same(r.from, [-8.5, -165.5]) && same(r.to, [-8.5, -174.5]), "the west cab's ramp");
plan.ramps = plan.ramps.filter((r) => r !== wramp);
const wpad = one(plan.pads, (p) => Z(p) && same(p.at, [-5.5, 0, -168]), "the west cab pad");
const epad = one(plan.pads, (p) => Z(p) && same(p.at, [5.5, 0, -168]), "the east cab pad");
const padAt = plan.pads.indexOf(wpad);

// 4. the cab pad off the circuit; 5. the spill pad in the west pad's row slot
epad.at = [1.5, 0, -166.5];
epad.apexAbove = 2;
epad.why = "THE CAB PAD (card: Molecule; NIGHT52 contract item 5, the upward line; EXT53: from the river's north bank, x 1.5, clear of the circuit's walk line): up onto the east cab behind its spitter: the high tier's second way up beside its ramp (the east rail's ride ends at the ramp's foot)";
epad.note = epad.why;
plan.pads[padAt] = { zone: R, at: [-4.5, 0, -158], target: [-3.5, 2.5, -169.5], apexAbove: 2, job: "pad", why: "THE SPILL PAD (EXT53 e53-m-b1.cjs: the west crane's pad gone with it): on the near bank's west, a jump pad over the slag onto the ladle's lip: meet the first file from above when it comes round the ladle; the lip's second way up beside the west rail's jump-off (the walkers climb the ladle's body)", note: "THE SPILL PAD (EXT53): over the slag onto the ladle's lip" };

// 3. the opening: the nodes
const W1 = [[-7.2, 0, -167], [7.2, 0, -167], [-5, 0, -173], [5, 0, -173]];
if (!same(room.waveNodes43[0], W1)) throw new Error("b1's wave-1 nodes " + JSON.stringify(room.waveNodes43[0]));
if (!same(room.highNodes, [[-2, 2.5, -169.5], [2, 2.5, -169.5], [-7.5, 4.5, -176.5], [7.5, 4.5, -176.5]])) throw new Error("b1's high nodes");
if (!same(room.waveNodes43[1], [[-2, 2.5, -169.5], [2, 2.5, -169.5], [-7.5, 4.5, -176.5], [7.5, 4.5, -176.5], [-5, 0, -173], [5, 0, -173]])) throw new Error("b1's wave-2 nodes");
const HID = [[-8.2, 0, -177.5], [-4.5, 0, -178], [-1.5, 0, -177.8]];
const LANE_W = [-8.2, 0, -171], BANK_E = [3, 0, -173];
room.nodes = room.nodes.filter((n) => !same(n, [-7.2, 0, -167]) && !same(n, [-5, 0, -173]) && !same(n, [5, 0, -173]));
room.nodes.unshift(...HID, LANE_W, BANK_E);
room.highNodes = [[-5.5, 2.5, -169.5], [-1.5, 2.5, -169.5], [7.5, 4.5, -176.5]];
room.waveNodes43[0] = HID.slice();
room.waveNodes43[1] = [[-5.5, 2.5, -169.5], [-1.5, 2.5, -169.5], [7.5, 4.5, -176.5], LANE_W, BANK_E];
room.waveNodes43_why += "; EXT53 (e53-m-b1.cjs, the opening held -> ambushed): wave 1's three husks start behind the tipped ladle (west, its 2.5 m lip hides them from the door) and come round both its ends and over the bridges at you; wave 2's spitters on the lip (moved west with it) and the east cab (the west cab is gone), its husks from the west lane and the far bank's east";

// 6. the proofs
const P = room.proofs;
if (P.length !== 5 || !same(P[3].points, [[-8.5, 0, -165.2], [-8.5, 0, -165.6], [-8.5, 4.5, -174.8], [-7.5, 4.5, -176.5]])) throw new Error("b1's proofs");
if (!same(P[2].points, [[0, 0, -178], [0, 0, -176.4], [0, 1.25, -173], [0, 2.45, -170.1], [0, 2.5, -169.2]])) throw new Error("b1's ladle walk");
P[0].points = [[0, 0, -153.5], [-7.9, 0, -155], [-7.9, 0, -157], [-7.9, 0, -164.8], [-8.2, 0, -167], [-8.2, 0, -176.5], [-4, 0, -178.3], [0, 0, -177.5], [4, 0, -175.5], [6, 0, -166.5], [7.9, 0, -164.8], [7.9, 0, -157], [0, 0, -153.5]];
P[2].points = P[2].points.map(sx);
P.splice(3, 1);
room.verb36 = room.verb36.replace("the rail's jump-off to the ladle's rim; the cab pads", "the west rail's jump-off onto the ladle's lip (EXT53: the ladle tipped west), the east rail onto the cab's ramp; the spill pad and the cab pad");
room.ext53_why = "EXT53 builder M (Tools/night/e53-m-b1.cjs; row M, THE MIRRORS): the mirror broken and the opening changed (I1 99 %, I7 a5 / b1 both held): the ladle tipped 3.5 m west, the west crane (cab, mast, ramp, pad) cut, wave 1 hidden behind the ladle (ambushed), the cab pad off the circuit, THE SPILL PAD added";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of Object.keys(before)) {
  if (k === "rooms") continue;
  if (Array.isArray(before[k])) {
    const keep = (o) => !o || (o.zone !== R && o.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside b1 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
for (const k of ["boxes", "ramps", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a b1 row without job / why");
const n = (k) => plan[k].filter(Z).length + "/" + before[k].filter(Z).length;
console.log("b1: boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads") + "; nodes " + room.nodes.length + ", high " + room.highNodes.length + "; proofs " + P.length);
// the opening's sidecar (I7, NIGHT53.md section 2): b1 held -> ambushed, its card line, in the same change
const OPEN = path.join(ROOT, "Design/levels/n52/openings53.json");
const otext = fs.readFileSync(OPEN, "utf8");
const ops = JSON.parse(otext);
if (ops.foundry.b1 !== "held" || !/walk the bridges at you \(held\)/.test(ops._cards.b1)) throw new Error("openings53.json b1 is not NIGHT53's");
// two exact line edits (the file's compact one-line level objects kept as they are), then parsed back and checked
const swap = (t, a, b) => { if (t.split(a).length !== 2) throw new Error("openings53.json: " + a + " not found once"); return t.replace(a, b); };
let onew = swap(otext, '"a5": "held", "b1": "held" }', '"a5": "held", "b1": "ambushed" }');
onew = swap(onew, JSON.stringify(ops._cards.b1), JSON.stringify("the first file comes out from behind the tipped ladle, round both its ends and over the bridges at you: past the river, the midfield line (ambushed; EXT53 e53-m-b1.cjs, was held: spitters held the rim and the cabs)"));
const ochk = JSON.parse(onew);
if (ochk.foundry.b1 !== "ambushed") throw new Error("openings53.json edit");
ops.foundry.b1 = "ambushed"; ops._cards.b1 = ochk._cards.b1;
if (!same(ochk, ops)) throw new Error("openings53.json: more than b1 changed");
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);
fs.writeFileSync(OPEN, onew);
console.log("wrote " + OPEN + " (b1 ambushed)");

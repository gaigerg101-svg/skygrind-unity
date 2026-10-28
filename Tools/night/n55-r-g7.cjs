// NIGHT55 builder R (P0-g7g5, OPEN O-14; T-127, T-128, T-131): iceline g7 THE THAW LOCK II breaks its mirror (I1 99 %) and makes a ride a
// shortcut (I2 1.22 / 1.26) by giving each half its own job instead of cutting one side bare.
//   THE WEST IS THE WALKERS' LOCK (kept as built): the west bridge, the west coal gantry (2 m) and its ramp off the bridge, the west bunker
//   and its ramp, the west sluice wall's ride over the 3.5 m west pit, the mooring and sluice posts, the west coal island's hop, the hopper.
//   THE EAST IS THE RIDERS' FIREBOX (re-cut, pieces added and moved, each with its job):
//   1. THE FIREBOX: the east pit widened west from 3.5 to 7.5 m (x 8.5 -> 4.5; no walker crosses it, a dash-jump barely does) and the
//      middle pit narrowed to the boiler's east face (x 5.5 -> 1.5). The walkers' east crossing becomes THE STOKER'S BRIDGE (x 1.5..4.5)
//      against the boiler: fire on both sides of it, the room's punch spot (the verb the card celebrates: "punch a body off a bridge into
//      the fire"). The east coal island is cut (it would stand on the new bridge); the west island keeps the middle fire's hop.
//   2. The EAST SLUICE WALL ends at the firebox's far edge (z -205.5 -> -202.5) and starts at the coal stage's end (3b; 18 -> 11.5 m; the
//      west's 18 m keeps the contract's 17 m ride): the only quick way over 7.5 m of fire: the walk round by the stoker's bridge is the
//      ride's detour (I2).
//   3. THE KEEPER'S HOUSE (4 m) moved from the SE corner (8 x 6 m, the gantry's twin) to THE KEEPER'S TOWER at the firebox's far edge
//      against the east wall (x 8.5..12, z -211..-204.5, 3.5 x 6.5 m): the spitter's stand over the fire, 2 m past the ride's end (jump off
//      the ride onto it), its hut on its back corner. Its walkers' ramp becomes THE COAL CONVEYOR: 8 m rising east along the far floor
//      (x 0.5..8.5, z -210..-207) from the exit lane up to the tower's west face (enemies reach every tier).
//   3b. THE COAL STAGE (added, 2 m, 4 x 3 m): the east bunker run 3 m south along the east wall: the riders' run-up straight onto the east
//      sluice wall (now 11.5 m from the stage's end to the firebox's far edge); the east bunker ramp single file (4 -> 2.5 m wide); the east
//      mooring post onto the tower's line to the quay.
//   4. Added: THE STOKER'S POST (1.2 x 1.2 x 4 m) at the stoker's bridge's south mouth: the corner the pack rounds off the bridge, the
//      break on the tower spitter's line down onto the bridge (the east sluice post's job, moved where the walkers now cross).
//   5. The pads re-sited for the new fire: THE COAL HOIST from the quay in front of the firebox onto the tower (it stood at the new bridge's
//      mouth: a pad on the walkers' line, I4); THE RETURN HOIST off the conveyor (the tank's spawn moved into the keeper's yard, the freed SE corner)
//      onto the firebox's far lip, over the fire back onto the east bunker when wave 3 comes out of the bunkers (the card's state change kept).
//   The waves, the lesson, the doors, the bunkers and wave 3's nodes, the boiler, the health and the lights are kept.
// Asserts g7 is NIGHT52's (+ NIGHT53's g7 pad move) and not yet run; every other room bit for bit.
// Run: node Tools/night/n55-r-g7.cjs [--dry] [--out <path>], then bash Tools/night/n52-loop-regen.sh iceline g7
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const oi = process.argv.indexOf("--out");
const OUT = oi > 0 ? process.argv[oi + 1] : PLAN;
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const R = "g7";
const g7 = plan.rooms.find((r) => r.id === R);
if (/NIGHT55/.test(g7.loop52_why || "")) throw new Error("g7 already re-cut by NIGHT55");
if (!same(g7.rect, [-12, -216, 12, -184])) throw new Error("g7's rect " + JSON.stringify(g7.rect));
const Z = (b) => b.zone === R;
const why = (o, w) => { o.why = w; o.note = w; };

// 1. the firebox and the stoker's bridge (pits: the middle narrowed, the east widened)
const pm = one(plan.pits, (q) => Z(q) && same(q.rect, [-5.5, -202.5, 5.5, -195.5]), "the middle pit");
pm.rect = [-5.5, -202.5, 1.5, -195.5];
why(pm, "THE GRATE, middle pit (7 x 7 m, the boiler and the west coal island in it; NIGHT55 n55-r-g7.cjs: narrowed to the boiler's east face, x 5.5 -> 1.5): crossed by the island hops or the dash-jump; the stoker's bridge runs along its east edge, fire on that side too");
const pe = one(plan.pits, (q) => Z(q) && same(q.rect, [8.5, -202.5, 12, -195.5]), "the east pit");
pe.rect = [4.5, -202.5, 12, -195.5];
why(pe, "THE FIREBOX (the east pit widened 3.5 -> 7.5 m; NIGHT55 n55-r-g7.cjs): no walker crosses it; the east sluice wall's ride is the quick way over, the coal hoist the high one; the pack on the stoker's bridge has the firebox at its east side: punch them in");
const isle = one(plan.boxes, (b) => Z(b) && same(b.min, [3, -0.8, -200]) && same(b.max, [4.5, 0.6, -198]), "the east coal island");
plan.boxes = plan.boxes.filter((b) => b !== isle);

// the grate's fire light stays on the middle pit's floor (it would hang over the new bridge: levelgen's "not flush to a surface")
const gl = one(plan.lights, (l) => l.id === "g7-L2" && l.room === R && same(l.at, [0, -0.775, -196.5]), "the grate's fire light");
gl.at = [-2, -0.775, -196.5];
gl.why += " (NIGHT55: 2 m west, inside the narrowed middle pit)";

// 2. the east sluice wall ends at the firebox's far edge
const ew = one(plan.boxes, (b) => Z(b) && same(b.min, [11, -0.8, -205.5]) && same(b.max, [12, 5, -187.5]), "the east sluice wall");
ew.min = [11, -0.8, -202.5];
why(ew, "the EAST SLUICE WALL (ride, cyan, 15 m x 5 m standing in the firebox; NIGHT55: ends at the firebox's far edge): the quick way over 7.5 m of fire (on foot it is the stoker's bridge, round the boiler: the ride's detour); jump off its end onto the keeper's tower 2 m on (4 m), or wall-jump back");

// 3. the keeper's house -> the keeper's tower, its ramp -> the coal conveyor, its hut
const hs = one(plan.boxes, (b) => Z(b) && same(b.min, [4, 0, -216]) && same(b.max, [12, 4, -210]), "the keeper's house");
hs.min = [8.5, 0, -211];
hs.max = [12, 4, -204.5];
why(hs, "THE KEEPER'S TOWER (4 m, 3.5 x 6.5 m against the east wall at the firebox's far edge; the third tier; NIGHT55: moved from the SE corner, where it was the west gantry's twin): the spitter's stand over the fire and the quay; you reach it off the east ride's end (2 m on), by the coal hoist over the firebox, or up the coal conveyor; the walkers follow you up the conveyor (no safe ledge, A12)");
const hr = one(plan.ramps, (r) => Z(r) && same(r.from, [7, -202]) && same(r.to, [7, -210]), "the keeper's house ramp");
hr.from = [0.5, -208.5];
hr.to = [8.5, -208.5];
hr.width = 3;
why(hr, "THE COAL CONVEYOR (the tower's walkers' ramp, 4 m over 8 m, rising east along the far floor from the exit lane to the tower's west face; NIGHT55: moved with the tower): the pack's climb off the stoker's bridge onto the third tier, yours too; the climb route enemies use (GG NIGHT20)");
const hut = one(plan.boxes, (b) => Z(b) && same(b.min, [4, 4, -216]) && same(b.max, [6, 6, -214]), "the keeper's hut");
hut.min = [10.5, 4, -211];
hut.max = [12, 6, -209];
why(hut, "the KEEPER'S HUT (1.5 x 2 m on the tower's back corner; NIGHT55: moved with the tower): the tower spitter's shield from the quay: the hoist or the east ride's jump puts you beside it");

// 4. the east sluice post -> THE STOKER'S POST at the bridge's south mouth (the added job)
const sp = one(plan.boxes, (b) => Z(b) && same(b.min, [2.4, 0, -207.1]) && same(b.max, [3.6, 4, -205.9]), "the east sluice post");
sp.min = [4.4, 0, -204.6];
sp.max = [5.6, 4, -203.4];
why(sp, "THE STOKER'S POST (1.2 m x 4 m at the stoker's bridge's south mouth; NIGHT55: the east sluice post moved where the walkers now cross): the corner the pack rounds off the bridge into your glove, and the break on the tower spitter's line down onto the bridge: stand behind it and the tower cannot see you");

// 4b. the east quay: the riders' run-up (THE COAL STAGE), the east bunker ramp single-file, the east mooring post on the tower's line
const eb = one(plan.boxes, (b) => Z(b) && same(b.min, [7, 0, -188]) && same(b.max, [12, 2, -184]), "the east coal bunker");
plan.boxes.splice(plan.boxes.indexOf(eb) + 1, 0, { zone: R, kind: "deck", min: [8, 0, -191], max: [12, 2, -188], job: "line", why: "THE COAL STAGE (2 m, 4 x 3 m: the east bunker run 3 m south along the east wall; NIGHT55, added): the riders' run-up: run the stage's length south and step onto the east sluice wall at 2 m, no turn, straight over the firebox; wave 3's pack comes off its lip at you" });
plan.boxes[plan.boxes.indexOf(eb) + 1].note = plan.boxes[plan.boxes.indexOf(eb) + 1].why;
ew.min = [11, -0.8, -202.5];
ew.max = [12, 5, -191];
why(ew, "the EAST SLUICE WALL (ride, cyan, 11.5 m x 5 m from the coal stage's end over the firebox; NIGHT55: off the stage, to the firebox's far edge): the quick way over 7.5 m of fire (on foot it is the stoker's bridge round the boiler: the ride's detour); jump off its end onto the keeper's tower 2 m on (4 m), or wall-jump back");
const er = one(plan.ramps, (r) => Z(r) && same(r.from, [2.5, -186]) && same(r.to, [7, -186]), "the east bunker ramp");
er.from = [2.5, -185.25];
er.to = [7, -185.25];
er.width = 2.5;
why(er, "the east bunker's walkers' ramp (2 m over 4.5 m, single file 2.5 m wide; NIGHT55: narrowed from 4 m): wave 3 comes down it one at a time into your glove, yours up to the coal stage's run-up (NIGHT52 fix 5: it rises from beside the door outward onto the bunker, flush with the door wall)");
const mp = one(plan.boxes, (b) => Z(b) && same(b.min, [5.4, 0, -191.6]) && same(b.max, [6.6, 3.5, -190.4]), "the east mooring post");
mp.min = [3.9, 0, -192.6];
mp.max = [5.1, 3.5, -191.4];
why(mp, "the EAST MOORING POST (1.2 m x 3.5 m; NIGHT55: 1.5 m west and 1 m south, onto the keeper's tower's line): breaks the tower spitter's line to the quay's east half; the pack off the stoker's bridge rounds it into the glove");

// 5. the pads
const hoist = one(plan.pads, (p) => Z(p) && same(p.at, [3.5, 0, -193.5]), "the coal hoist");
hoist.at = [7, 0, -193.5];
hoist.target = [10, 4, -206.5];
hoist.why = "THE COAL HOIST (a jump pad, the upward line, A5; NIGHT55: moved off the stoker's bridge's mouth, a walkers' line): from the quay in front of the firebox over the fire onto the keeper's tower (4 m): the fast way to the spitter's stand, the KESTREL on the gantry from above";
const ret = one(plan.pads, (p) => Z(p) && same(p.at, [1.3, 0, -208]), "the return hoist");
ret.at = [7, 0, -205.2];
ret.why = "THE RETURN HOIST (a jump pad on the firebox's far lip between the stoker's post and the keeper's tower; NIGHT55: moved off the conveyor's foot; in the keeper's yard its arc hit the conveyor, pad probe 24.2 m miss): over the firebox back onto the east coal bunker (2 m): when wave 3 comes out of the bunkers, the pad takes you to them (the fight turns around)";

// the room row: spawn nodes off the tower and the conveyor, the high nodes onto the tower, the east walk proof, the fall returns, the frame
if (!same(g7.nodes, [[-1.5, 0, -204], [1.5, 0, -204], [-4.5, 0, -209], [4.5, 0, -209], [-9.8, 0, -205], [9.8, 0, -205], [-10, 0, -192.8], [10, 0, -192.8]])) throw new Error("g7's nodes " + JSON.stringify(g7.nodes));
g7.nodes = [[-1.5, 0, -204], [1.5, 0, -204], [-4.5, 0, -209], [4.5, 0, -213.5], [-9.8, 0, -205], [9, 0, -213.5], [-10, 0, -192.8], [10, 0, -192.8]];
if (!same(g7.highNodes, [[-9, 2, -213], [-6, 2, -213.5], [7.5, 4, -213], [10, 4, -213.5]])) throw new Error("g7's high nodes " + JSON.stringify(g7.highNodes));
g7.highNodes = [[-9, 2, -213], [-6, 2, -213.5], [10, 4, -205.5], [9.5, 4, -209.5]];
g7.nodes_why += "; NIGHT55 (n55-r-g7.cjs): the two east far-floor nodes moved off the new tower and conveyor into the keeper's yard (the tank comes out of the yard round the conveyor's foot), the house's high nodes onto the keeper's tower";
const pf = g7.proofs[1];
if (!same(pf.points, [[7, 0, -194], [7, 0, -199], [7, 0, -202.8], [7, 4, -211], [9, 4, -213]])) throw new Error("g7's east walk proof " + JSON.stringify(pf.points));
pf.points = [[3, 0, -193], [3, 0, -199], [3, 0, -203.5], [0, 0, -208.5], [9.5, 4, -208.5], [10, 4, -205.5]];
const fr = g7.fallReturn43;
if (fr.by.length !== 4) throw new Error("g7's fall returns " + fr.by.length);
fr.by = [
  { rect: [-12, -202.5, -8.5, -195.5], to: [-10, 0, -193.5], yawDeg: 0 },
  { rect: [-5.5, -202.5, -1.5, -195.5], to: [-2.75, 0, -193.5], yawDeg: 0 },
  { rect: [-1.5, -202.5, 1.5, -195.5], to: [0, 0, -193.5], yawDeg: 0 },
  { rect: [4.5, -202.5, 8.5, -195.5], to: [5.5, 0, -193.5], yawDeg: 0 },
  { rect: [8.5, -202.5, 12, -195.5], to: [10, 0, -193.5], yawDeg: 0 },
];
fr._why += "; NIGHT55: the middle pit narrowed and the firebox widened (n55-r-g7.cjs)";
if (!same(g7.frames52.high, [9, 4, -212, 158, -0.12])) throw new Error("g7's high frame " + JSON.stringify(g7.frames52.high));
g7.frames52.high = [10, 4, -206.5, 150, -0.12];
g7.loop52_why += " NIGHT55 (n55-r-g7.cjs, builder R; O-14): the mirror broken by giving each half a job: the west is the walkers' lock (kept), the east the riders' firebox: the east pit 7.5 m wide, the walkers' east crossing the stoker's bridge against the boiler, the east sluice wall 15 m to the firebox's far edge, the keeper's house moved to the keeper's tower over the fire with the coal conveyor, the stoker's post added at the bridge's mouth, the coal and return hoists re-sited, the east coal island cut.";

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== R && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => !x || (x.zone !== R && x.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g7 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
const n = (k) => plan[k].filter(Z).length - before[k].filter(Z).length;
console.log("g7: boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads") + ", pits " + n("pits"));
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + OUT);

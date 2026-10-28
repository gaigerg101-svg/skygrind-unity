// NIGHT55 builder R (P0-g7g5, OPEN O-15; T-127, T-128, T-131): iceline g5 THE ICEHOUSE breaks its mirror (I1 76 %) and makes a ride a
// shortcut (I2 1.07 / 1.11 / 1.25 / 1.26). NIGHT52 built the far end as twins: the west shelf (2 m) in the SW corner, the crane deck (4.5 m)
// in the SE corner, each with its ramp against its side's ride wall, so each ride wall ran beside its own walk lane (walk = ride).
//   THE WEST IS THE POUR (kept; its ramp moved, 6): the shelf (the second pack pours off it: the blast's target), the west ride onto it, the
//   ice saw, the west hoist; the far west post.
//   THE EAST IS THE CRANE (re-cut, pieces moved and added, each with its job):
//   1. THE CRANE RUNWAY: the crane deck (4.5 m, 5 x 9 m in the SE corner) becomes a 3.5 x 10 m runway along the east wall over the ring's
//      east side (x 6.5..10, z -148..-138): the drones' perch over the warden's ring in wave 3, in the middle of the fight, not in a corner.
//   2. The EAST RIDE WALL runs from the door end INTO the runway (z -146.5 -> -138; 18 -> 9.5 m): ride it and step off its end onto the
//      runway (4.5 m): on foot the runway is the long way round the ring to its ramp in the far corner (the ride's detour, I2).
//   3. THE RUNWAY RAMP (moved): 4.5 m over 9 m from 1 m off the far SE corner (the crane deck's old floor) north up to the runway: the warden walks
//      up after you (no safe ledge, A12); the climb route enemies use (GG NIGHT20).
//   4. Added: THE CRANE HOOK (a 1.2 x 1.2 m, 3.5 m block in the freed far floor, x 3.8..5: the far east post moved off the old ramp foot
//      onto the runway ramp's foot): the corner the warden circles to reach you and the break on the runway's line down to the exit.
//   5. The crane cab and the crane hoist moved onto the runway (the cab: the drones' cover from the door; the hoist: runway -> shelf over
//      the ring: the high circuit kept); the ice hoist re-aimed onto the runway.
//   6. THE POUR CHUTE: the shelf's ramp moved off the west lane (where it ran beside the west ride, the runway's twin) to the shelf's east
//      face, rising west from the far floor: the second pack pours down it into the ring behind the stack past the far west post (the
//      blast's target); the west lane is the ride's alone.
//   The waves, the lesson, the doors, the stack, the short ice walls, the health and the lights are kept.
// Asserts g5 is NIGHT52's (fix 1 + K1) and not yet run; every other room bit for bit.
// Run: node Tools/night/n55-r-g5.cjs [--dry] [--out <path>], then bash Tools/night/n52-loop-regen.sh iceline g5
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
const R = "g5";
const g5 = plan.rooms.find((r) => r.id === R);
if (/NIGHT55/.test(g5.loop52_why || "")) throw new Error("g5 already re-cut by NIGHT55");
if (!same(g5.rect, [-11, -158, 10, -128])) throw new Error("g5's rect " + JSON.stringify(g5.rect));
const Z = (b) => b.zone === R;
const why = (o, w) => { o.why = w; o.note = w; };

// 1. the crane deck -> the crane runway
const cd = one(plan.boxes, (b) => Z(b) && same(b.min, [5, 0, -158]) && same(b.max, [10, 4.5, -149]), "the crane deck");
cd.min = [6.5, 0, -148];
cd.max = [10, 4.5, -138];
why(cd, "THE CRANE RUNWAY (4.5 m, 3.5 x 10 m along the east wall over the ring's east side; the third tier; NIGHT55 n55-r-g5.cjs: the crane deck moved out of the SE corner, where it was the west shelf's twin): the drones' perch over the warden's ring in wave 3; reached off the east ride's end, by the ice hoist, or up its ramp from the far corner (no safe ledge: the walkers follow up the ramp)");

// 2. the east ride wall into the runway
const ew = one(plan.boxes, (b) => Z(b) && b.ride && same(b.min, [9, 0, -146.5]) && same(b.max, [10, 6, -128.5]), "the east ride wall");
ew.min = [9, 0, -138];
why(ew, "the EAST RIDE WALL (cyan, 9.5 m x 6 m along the side wall from the door end into the crane runway; NIGHT55: ends at the runway's face): ride it and step off its end onto the runway (4.5 m): on foot the runway is the long way round the ring to its ramp in the far corner (the ride is the shortcut onto the drones' perch)");

// 3. the runway ramp
const cr = one(plan.ramps, (r) => Z(r) && same(r.from, [7.5, -140]) && same(r.to, [7.5, -149]), "the crane deck ramp");
cr.from = [8.25, -157];
cr.to = [8.25, -148];
cr.width = 3.5;
why(cr, "THE RUNWAY RAMP (4.5 m over 9 m from 1 m off the far SE corner north up to the crane runway; NIGHT55: moved with the runway): the warden walks up after you (no safe ledge, A12); the climb route enemies use (GG NIGHT20)");

// 4. the far east post -> THE CRANE HOOK at the runway ramp's foot
const fp = one(plan.boxes, (b) => Z(b) && same(b.min, [3.8, 0, -151.6]) && same(b.max, [5, 3.5, -150.4]), "the far east post");
fp.min = [3.4, 0, -156.6];
fp.max = [4.6, 3.5, -155.4];
why(fp, "THE CRANE HOOK (1.2 m x 3.5 m, the crane's block lowered to the far floor at the runway ramp's foot; NIGHT55: the far east post moved off the old ramp's foot): the corner the warden circles to reach you, and the break on the runway's line down to the exit: duck behind it while the drones are up");

// 5. the cab, the crane hoist, the ice hoist
const cab = one(plan.boxes, (b) => Z(b) && same(b.min, [8, 4.5, -158]) && same(b.max, [10, 6.5, -156]), "the crane cab");
cab.min = [9, 4.5, -144];
cab.max = [10, 6.5, -142];
why(cab, "THE CRANE CAB (1 x 2 m against the east wall at the runway's middle; NIGHT55: moved with the runway, off its ramp's top): the drones' cover from the door; the ice hoist lands you beside it");
const ch = one(plan.pads, (p) => Z(p) && same(p.at, [6.5, 4.5, -151]), "the crane hoist");
ch.at = [7.75, 4.5, -145.5];
ch.why = "THE CRANE HOIST (a jump pad on the crane runway; NIGHT55: moved with the runway): runway -> shelf over the ring: the high circuit (the ride or the ice hoist up, the crane hoist across, the shelf ramp down, the west lane, the door) so no tier is a dead end";
const ih = one(plan.pads, (p) => Z(p) && same(p.at, [2.5, 0, -136]), "the ice hoist");
ih.target = [8, 4.5, -141.5];
ih.why = "THE ICE HOIST (a jump pad, the upward line, A5): from the floor past the east ice wall onto the crane runway (4.5 m; NIGHT55: re-aimed at the runway): the way up when the drones come, over the pack's heads";

// the room row: the high nodes onto the runway, a walk proof up the runway ramp, the high frame
if (!same(g5.highNodes, [[-8, 2, -152], [-8, 2, -156], [8.5, 4.5, -152], [6.5, 4.5, -155]])) throw new Error("g5's high nodes " + JSON.stringify(g5.highNodes));
g5.highNodes = [[-8, 2, -152], [-8, 2, -156], [8, 4.5, -140.5], [7.5, 4.5, -142.5]];
g5.highNodes_why += "; NIGHT55 (n55-r-g5.cjs): the crane's two nodes onto the crane runway over the ring";
if (g5.proofs.length !== 2) throw new Error("g5's proofs " + g5.proofs.length);
if (!same(g5.proofs[1].points, [[-8.5, 0, -139], [-8.5, 2, -146], [-8, 2, -152]])) throw new Error("g5's shelf walk proof " + JSON.stringify(g5.proofs[1].points));
g5.proofs[1].points = [[0, 0, -150], [-0.5, 0, -155], [-5.5, 2, -155], [-8, 2, -152]];

// 6. the shelf's ramp -> THE POUR CHUTE (off the west lane, where it ran beside the west ride and was the runway's twin)
const sr = one(plan.ramps, (r) => Z(r) && same(r.from, [-8.5, -140]) && same(r.to, [-8.5, -146]), "the west shelf ramp");
sr.from = [-1, -155];
sr.to = [-5, -155];
why(sr, "THE POUR CHUTE (the west shelf's walkers' ramp, 2 m over 4 m, rising west from the far floor into the shelf's east face; NIGHT55: moved off the west lane, where it ran beside the west ride): the second pack pours down it into the ring behind the stack, past the far west post: stand off its foot and blast the pour (the room's verb); yours up onto the shelf; the west lane is the ride's alone");
if (!same(g5.frames52.high, [7, 4.5, -153, 161, -0.12])) throw new Error("g5's high frame " + JSON.stringify(g5.frames52.high));
g5.frames52.high = [8.5, 4.5, -147.5, 146, -0.12];
g5.loop52_why += " NIGHT55 (n55-r-g5.cjs, builder R; O-15): the mirror broken: the west is the pour (the shelf and the west ride kept, its ramp moved to the pour chute facing the ring), the east the crane: the crane deck moved out of the SE corner onto a 3.5 x 10 m runway along the east wall over the ring, the east ride wall 9.5 m into it (the ride the shortcut onto the drones' perch), its ramp from the far corner, the crane hook at the ramp's foot (the far east post moved), the cab and the crane hoist on the runway, the ice hoist re-aimed.";

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== R && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => !x || (x.zone !== R && x.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g5 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
const n = (k) => plan[k].filter(Z).length - before[k].filter(Z).length;
console.log("g5: boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads"));
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + OUT);

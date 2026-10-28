// EXT53 builder A5: foundry a5 THE POURING FLOOR re-cut in place. NIGHT52 F built it a mirror across the door's axis x = 0 (the 16 m
// gantry west / the 13 m skim step east, both 6 m deep; the gantry ramp / the skim ramp; the two back ramps; the two cheeks): I1 87 %;
// I2 1.08 (the 18 m crane rail at x 9.4..10 ran beside the east lane and the skim ramp under its north end: a walker reached the ride's
// landing in 19.5 m); I4 FAIL (the gantry pad 0.49 m off the circuit, the skim pad 0.70 m off the skim ramp's walk).
//   THE STORY: the east side is the pour's side (one rail, one narrow ledge at its end), the west the charging side (the gantry and its arm):
//   1. THE SKIM RAMP is cut: the lane beside the rail now ends at a 2.5 m face (the walkers' way onto the ledge is the trench's back ramp,
//      ~40 m round): the ride is the fast way from the entry corner to the ledge (I2).
//   2. THE SKIM STEP becomes THE SKIM LEDGE: 3 m deep (x 7..10, was 4..10), still 13 m along the east wall: the ride's landing, the skim
//      pad, the ladle; the freed 3 m strip (x 4..7) is floor: the exit trench's east side opens toward the rail's landing.
//   3. THE LADLE moves onto the ledge (x 8.5..10, against the wall): still the beat of cover between the gantry and the skim pad.
//   4. THE SKIM LEDGE'S BACK RAMP turns east (x 2..7 along z -141, its top on the ledge's west edge: the walkers' one way up; measured:
//      the ramp only moved 3 m east, x 5..7 beside the ledge, levelgen's "enemies reach every tier" found the ledge unreached); the spitter node on the old step edge (7, 2.5, -132) moves
//      behind the ladle (8, 2.5, -138.5); the return pad lands on the ledge (8.5, 2.5, -141).
//   5. THE CHARGING BRIDGE (new; deck 5 m, 4.5..5): the gantry's arm 3 m out over the north floor (x -4..-1, z -132..-128.5): a place to
//      fight from (the spitters' forward perch, face to face with the gantry pad's landing), walkers reach it off the gantry.
//   6. THE GANTRY PAD moves to the east floor (3.5, 0, -130), clear of the circuit and the trench walk (I4), and throws you onto the bridge
//      (6.3 m, was 7.8 m onto the gantry's middle): the upward line from the side the husks hold to the side the spitters hold.
//   7. The walk proofs: the skim ramp walk cut with it; the ledge's back ramp walk 3 m east.
// Asserts a5 is NIGHT52 F's (gen-n52-foundry.cjs, u3-n53); every other room, door and row bit for bit.
// Re-running gen-n52-foundry.cjs rebuilds a5 and wipes this: run this script again after it (and e53-m-b1.cjs, e53-m-b1-ride.cjs).
// Run: node Tools/night/e53-a5.cjs [--dry] [--out <path>]
//      dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/foundry.plan.json Design/levels/n52/foundry.json --walk
//      node Tools/night/gen-n52-foundry.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/foundry.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "a5";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const Z = (o) => o.zone === R;
const room = plan.rooms.find((r) => r.id === R);
if (!same(room.rect, [-10, -144, 10, -112]) || room.ext53_why) throw new Error("a5 is not NIGHT52 F's (or already run)");

// 1. the skim ramp cut
const sramp = one(plan.ramps, (r) => Z(r) && same(r.from, [8.15, -125]) && same(r.to, [8.15, -131]) && r.width === 2.5, "the skim ramp");
plan.ramps = plan.ramps.filter((r) => r !== sramp);

// 2. the skim step -> the skim ledge
const step = one(plan.boxes, (b) => Z(b) && same(b.min, [4, 0, -144]) && same(b.max, [10, 2.5, -131]), "the skim step");
step.min = [7, 0, -144];
step.why = "THE SKIM LEDGE (deck 2.5 m, card: Molecule node S; EXT53 e53-a5.cjs: 3 m deep, x 7..10, was the 6 m skim step, and its skim ramp cut): a 13 m ledge along the east wall, the RIDE's landing at the crane rail's north end (ride up, release onto it); the walkers' one way up is the trench's back ramp (~40 m round), so the rail is the fast way from the entry corner; the pad on it throws you across to the gantry";
step.note = step.why;

// 3. the ladle onto the ledge
const ladle = one(plan.boxes, (b) => Z(b) && same(b.min, [4, 2.5, -136]) && same(b.max, [5.5, 10, -134.5]), "the ladle");
ladle.min = [8.5, 2.5, -136.5]; ladle.max = [10, 10, -135];
ladle.why = "THE LADLE hung over the skim ledge (card: Every-solid rule; EXT53: on the 3 m ledge against the east wall), ledge to roof: it breaks the gantry spitters' line of sight onto the ledge's pad, so a rider who lands has one beat of cover before the pad launches him, and the ledge's spitter stands behind it";
ladle.note = ladle.why;

// 4. the ledge's back ramp 3 m east; the spitter node; the return pad's landing
const bramp = one(plan.ramps, (r) => Z(r) && same(r.from, [3, -138]) && same(r.to, [3, -144]) && r.width === 2, "the skim step's back ramp");
bramp.from = [2, -141]; bramp.to = [7, -141];
bramp.why = "the SKIM LEDGE'S BACK RAMP (card: Molecule; round 2; EXT53: turned east, x 2..7 along z -141, its top on the 3 m ledge's west edge): 2.5 m over 5 m from the exit trench's east side straight onto the ledge's north end, the walkers' one way onto the ledge (the skim ramp beside the rail is cut: husks reach the ledge the long way, from the trench; a file on it is a pierce line from the bridge) and your walk up from the exit side";
bramp.note = bramp.why;
const rpad = one(plan.pads, (p) => Z(p) && same(p.at, [-7, 5, -141]) && same(p.target, [7, 2.5, -141]), "the return pad");
rpad.target = [8.5, 2.5, -141];
rpad.why = "the RETURN PAD (card: Molecule; round 2; EXT53: onto the 3 m ledge, 15.5 m): from the gantry gallery's north end back over the exit trench onto the skim ledge, so the high side is a loop (rail -> ledge -> skim pad -> gantry -> return pad -> ledge) and the gantry is never a dead end to hold";
rpad.note = rpad.why;
const OLD = [7, 2.5, -132], NEW = [8, 2.5, -138.5];
const hi = room.highNodes.findIndex((n) => same(n, OLD));
if (hi < 0) throw new Error("a5's high node " + JSON.stringify(OLD));
room.highNodes[hi] = NEW;
let moved = 0;
for (const w of room.waveNodes43) for (let i = 0; i < w.length; i++) if (same(w[i], OLD)) { w[i] = NEW; moved++; }
if (moved !== 1) throw new Error("a5's wave nodes: " + moved + " at " + JSON.stringify(OLD));
room.waveNodes43_why += "; EXT53 (e53-a5.cjs): the ledge's spitter node from the old step's edge (7, 2.5, -132) to behind the ladle (8, 2.5, -138.5) on the 3 m ledge";

// 5. the charging bridge (new): placed after the gantry's mast
const mast = one(plan.boxes, (b) => Z(b) && same(b.min, [-10, 5, -136]) && same(b.max, [-8, 10, -134]), "the gantry's mast");
const bridge = { zone: R, kind: "deck", min: [-4, 4.5, -132], max: [-1, 5, -128.5], job: "perch",
  why: "THE CHARGING BRIDGE (deck 5 m, new: EXT53 e53-a5.cjs, the mirror broken by a piece with a job): the gantry's arm 3 m out over the north floor at its south end, a place to fight from: the gantry spitters' forward perch (they lob from it over the floor their husks hold) and the gantry pad's landing (you arrive on it face to face with them; punch them off, a 5 m drop); walkers reach it off the gantry, and the floor runs under it (4.5 m clear)" };
bridge.note = bridge.why;
plan.boxes.splice(plan.boxes.indexOf(mast) + 1, 0, bridge);

// 6. the gantry pad to the east floor, onto the bridge
const gpad = one(plan.pads, (p) => Z(p) && same(p.at, [-1.5, 0, -127]) && same(p.target, [-7, 5, -132.5]), "the gantry pad");
gpad.at = [3.5, 0, -130]; gpad.target = [-2.5, 5, -130.2];
gpad.why = "the GANTRY PAD (card: Molecule; NIGHT52 contract item 5, the upward line: the game's first pad in a campaign room; EXT53 e53-a5.cjs: on the east floor, x 3.5, clear of the circuit and the trench walk): from the side the husks hold up onto THE CHARGING BRIDGE (6.3 m, apex 2 m over the deck), face to face with the gantry's spitters";
gpad.note = gpad.why;

// 7. the proofs
const P = room.proofs;
if (P.length !== 6 || !same(P[2].points, [[5.5, 0, -124], [8.15, 0, -124.5], [8.15, 2.5, -131.5], [7, 2.5, -134]])) throw new Error("a5's skim ramp walk");
if (!same(P[5].points, [[0, 0, -136.5], [3, 0, -137.3], [3, 2.45, -143.6], [5.5, 2.5, -143.2]])) throw new Error("a5's back ramp walk");
P[5].points = [[0, 0, -136.5], [1.4, 0, -141], [6.8, 2.45, -141], [8.5, 2.5, -141.5]];
P.splice(2, 1);
room.verb36 = room.verb36.replace("the pads,", "the pads (EXT53: the gantry pad from the east floor onto the charging bridge),");
room.ext53_why = "EXT53 builder A5 (Tools/night/e53-a5.cjs; I1 87 %, I2 1.08, I4 FAIL): the mirror broken (the skim step a 3 m ledge, its ramp cut, THE CHARGING BRIDGE added off the gantry, the gantry pad moved to the east floor onto it), the ride the fast way to the ledge (the lane's ramp cut), the pads off the walk lines";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of Object.keys(before)) {
  if (k === "rooms") continue;
  if (Array.isArray(before[k])) {
    const keep = (o) => !o || (o.zone !== R && o.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside a5 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
for (const k of ["boxes", "ramps", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": an a5 row without job / why");
const n = (k) => plan[k].filter(Z).length + "/" + before[k].filter(Z).length;
console.log("a5: boxes " + n("boxes") + ", ramps " + n("ramps") + ", pads " + n("pads") + "; nodes " + room.nodes.length + ", high " + room.highNodes.length + "; proofs " + P.length);
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const oi = process.argv.indexOf("--out");
const dest = oi > 0 ? process.argv[oi + 1] : PLAN;
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(dest, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + dest);

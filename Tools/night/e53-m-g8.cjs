// EXT53 builder M (row M, THE MIRRORS, the third room): iceline g8 THE ROUNDHOUSE II breaks its mirror and clears its hoist off the walk
// line. NIGHT52 built it a mirror across the door's axis x = 0 in its big pieces (the 3 m gallery and the 5.5 m signal box, 8 x 4 m each
// at the north wall; two 23 m lane walls; each deck's ramp against its lane wall): I1 90 %; I2 1.07 / 1.10 (each ride's walk round is about
// its own length); I4 FAIL: the cross hoist 1.50 m off the west lane's walk line (DECIDE53 D-12: the only clear spot in lane W failed its
// pad probe, reverted).
//   1. THE WEST WALL 1 m in (rect x -11 -> -10; 21 x 30 m: GG's "smaller"; no door on it): the west lane wall moves with it.
//   2. THE GALLERY's RAMP moves from the west lane wall to the gallery's east end (x -6..-3, z -246..-239), in the NW column's place (the
//      column is cut): the walkers climb to the spitters' stand out of the stampede's start, beside the pit's far end; the east ramp keeps
//      its lane wall: the two decks are climbed differently. The gallery is 5 x 4 m over it (x -8..-3; try 1's 7 m gallery left its west
//      end a camping spot, hold52 0 hits), its sand hopper on its new west back corner, its lamp and spitter node 2.5-3 m east with it.
//   3. THE HOIST BAY: the west lane's north end behind the ramp (x -9.4..-6, z -246..-239) holds the cross hoist (-8, 0, -243): out of the
//      stampede onto the signal box from cover; the west ride's end drops you beside it (I4: the pad 5.6 m off the walk line).
//   4. The far-end charger node in the ramp's place moves into the bay (the hoist is contested); the walk proofs follow (the west lane's
//      walk round the ramp's foot; the gallery's walk up the new ramp).
// Asserts g8 is NIGHT52's (fix K1 / cycle 3); every other room bit for bit.
// Run: node Tools/night/e53-m-g8.cjs [--dry], then bash Tools/night/n52-loop-regen.sh iceline g8
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "g8";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const one = (list, pred, what) => { const f = list.filter(pred); if (f.length !== 1) throw new Error(what + ": found " + f.length); return f[0]; };
const Z = (b) => b.zone === R;
const room = plan.rooms.find((r) => r.id === R);
if (!same(room.rect, [-11, -250, 11, -220]) || /EXT53/.test(room.loop52_why || "")) throw new Error("g8 is not NIGHT52's (or already run)");

// 1. the west wall 1 m in
room.rect = [-10, -250, 11, -220];
const ww = one(plan.boxes, (b) => Z(b) && b.ride && same(b.min, [-11, 0, -246]) && same(b.max, [-10.4, 12, -223]), "the west lane wall");
ww.min = [-10, 0, -246]; ww.max = [-9.4, 12, -223];
if (ww.ride36 && ww.ride36.planeX !== undefined) { if (ww.ride36.planeX !== -10.4) throw new Error("the west lane wall's ride36 " + JSON.stringify(ww.ride36)); ww.ride36 = Object.assign({}, ww.ride36, { planeX: -9.4 }); }
ww.why = "the WEST LANE WALL (ride, cyan, 23 m; EXT53 e53-m-g8.cjs: 1 m in with the wall): ride lane W past the charging pack (a charge that misses you ends in the wall: the stun) and off its end into the hoist bay, onto the cross hoist";
ww.note = ww.why;
// the gallery 5 x 4 m (x -8..-3) over its new ramp: try 1 kept it 7 m long (x -10..-3) and hold52 found its west end a camping spot
// (wave 1's lone charger never reached [-8.5, 3, -247]: 0 hits in 3 of 3 seeds; the gallery's centre 0 in 1 of 3)
const gal = one(plan.boxes, (b) => Z(b) && same(b.min, [-11, 0, -250]) && same(b.max, [-3, 3, -246]), "the gallery");
gal.min = [-8, 0, -250];
gal.why = gal.why.replace("THE GALLERY (west, 3 m, 8 x 4 m)", "THE GALLERY (west, 3 m, 5 x 4 m; EXT53: over its ramp at its east end, out of the stampede's start; its far west end was a camping spot, cut)").replace("the walkers' ramp climbs to it along the west lane wall (no safe ledge)", "the walkers' ramp climbs to it from the pit's far end (no safe ledge)");
gal.note = gal.why;
const hop = one(plan.boxes, (b) => Z(b) && same(b.min, [-11, 3, -250]) && same(b.max, [-9, 12, -248]), "the gallery's sand hopper");
hop.min = [-8, 3, -250]; hop.max = [-6, 12, -248];
const lamp = one(plan.lights, (l) => l.id === "g8-L5" && same(l.at, [-7.5, 5, -249.95]), "the gallery's lamp");
lamp.at = [-4.5, 5, -249.95];
if (!same(room.highNodes, [[-7, 3, -248], [6, 5.5, -248]])) throw new Error("g8's high nodes " + JSON.stringify(room.highNodes));
room.highNodes = [[-4.5, 3, -248], [6, 5.5, -248]];
for (const w of room.waveNodes43) for (let i = 0; i < w.length; i++) if (same(w[i], [-7, 3, -248])) w[i] = [-4.5, 3, -248];
room.highNodes_why = (room.highNodes_why || "") + "; EXT53 (e53-m-g8.cjs): the gallery's spitter node 2.5 m east with the gallery's new west end";

// 2. the gallery's ramp to its east end; the NW column cut
const gr = one(plan.ramps, (r) => Z(r) && same(r.from, [-8.9, -239]) && same(r.to, [-8.9, -246]), "the gallery's ramp");
gr.from = [-4.5, -239]; gr.to = [-4.5, -246];
gr.why = "the GALLERY's walkers' ramp (3 m over 7 m; EXT53: moved to the gallery's east end, in the NW column's place): the walkers' and spitters' climb out of the stampede's start beside the pit's far end (GG NIGHT20), yours to the spitters' stand; its high side walls the hoist bay";
gr.note = gr.why;
const col = one(plan.props, (q) => q.room === R && q.id === "g8-P5" && same(q.at, [-5, 6, -242]), "the NW column");
plan.props = plan.props.filter((q) => q !== col);

// 3. the hoist bay
const pad = one(plan.pads, (p) => Z(p) && same(p.at, [-8.5, 0, -236]), "the cross hoist");
pad.at = [-8, 0, -243];
pad.why = "THE CROSS HOIST (a jump pad in THE HOIST BAY, the west lane's north end behind the gallery's ramp; EXT53 e53-m-g8.cjs, off the walk line): over the ramp, lane C and the exit onto the signal box (5.5 m): out of the stampede onto the spitters' high stand from cover; the west ride's end drops you beside it";
pad.note = "the cross hoist (EXT53: in the hoist bay)";

// 4. the node in the ramp's place; the proofs
const OLD = [-5, 0, -245], NEW = [-7.5, 0, -245.2];
const swap = (list) => { const i = list.findIndex((n) => same(n, OLD)); if (i < 0) throw new Error("no node " + JSON.stringify(OLD)); list[i] = NEW.slice(); };
swap(room.nodes);
for (const w of room.waveNodes43) swap(w);
room.waveNodes43_why = (room.waveNodes43_why || "") + "; EXT53 (e53-m-g8.cjs): the west far-end charger node from the ramp's place into the hoist bay";
const P = room.proofs;
if (!same(P[0].points, [[0, 0, -222], [-7, 0, -226], [-7, 0, -238], [-3, 0, -245], [0, 0, -248]])) throw new Error("g8's west walk");
if (!same(P[2].points, [[-8.9, 0, -238.5], [-8.9, 3, -246], [-7, 3, -248]])) throw new Error("g8's gallery walk");
P[0].points = [[0, 0, -222], [-7, 0, -226], [-7, 0, -237.5], [-2.3, 0, -238.3], [-2.3, 0, -244.5], [0, 0, -248]];
P[2].points = [[-4.5, 0, -238.5], [-4.5, 3, -246], [-4.5, 3, -248]];
room.verb36 = room.verb36.replace("take the cross hoist to the signal box", "take the cross hoist out of the hoist bay to the signal box");
room.loop52_why += " EXT53 (e53-m-g8.cjs, builder M): the mirror broken: the west wall 1 m in (21 x 30 m), the gallery's ramp at its east end in the NW column's place, the cross hoist in the hoist bay behind it (off the walk line), the far-end charger node in the bay.";

// every other room bit for bit
for (const k of Object.keys(before)) {
  if (k === "rooms") {
    for (let i = 0; i < before.rooms.length; i++) if (before.rooms[i].id !== R && !same(before.rooms[i], plan.rooms[i])) throw new Error("room " + before.rooms[i].id + " changed");
  } else if (Array.isArray(before[k])) {
    const keep = (x) => !x || (x.zone !== R && x.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside g8 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
console.log("g8: rect " + JSON.stringify(room.rect) + "; west wall, gallery, hopper 1 m in; the gallery ramp at x -6..-3; NW column cut; the hoist at " + JSON.stringify(pad.at));
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);

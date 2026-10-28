// EXT53 builder K2 (DECIDE53 D-19): span p3 4.3 KNOT YARD (THE TIE-OFF II), its second pass, edited IN PLACE in
// Design/levels/n52/span.plan.json. What it answers: I1 95 % (n53-identity at port/stage3 da7c9a1e: the room was a mirror across the door's
// axis: two anchorages, two cheeks, two ramp pairs, two posts). The lesson is "KILLS GIVE PULLS." (pulls between balconies in a fight), so
// the mirror breaks where a pull earns its place:
//   1. THE VOID RUNS TO THE EAST WALL (z 13.5..23.5, x 7..12): AE, the 13 x 5 m high stand with a walkers' ramp from each deck, is cut to
//      THE WINCH BALCONY, a 3 x 4 m iron balcony on the east wall over the void's north-east corner (4.2 m, x 9..12, z 13..17, its pier
//      from the void's floor): the spitter's high perch, reached on foot only by its north ramp from the exit deck. From the entry deck
//      the balcony is across the void: a kill buys the pull at RE (over it) or the KNOT, the walk is the long way round over AW. The west
//      stays the walkers' side: AW (2.5 m, both ramps) is the low balcony and the only walked crossing, so the room is a low west bridge
//      and a high east perch facing each other over the void, not two stands.
//   2. AE's SOUTH RAMP is cut (it would land in the void); the east cheek stands free in the void (a blade both sides), still the east
//      crossing by a ride; THE WINCH moves onto the balcony's south end (the spitter's cover); the winch spring's target onto the balcony.
//   The proofs follow (proof 0 no longer loops: it crosses on AW and climbs to the balcony by the north ramp); the fall return's halves
//   reach the east wall; wave 2's east nodes and the high frame onto the balcony.
// Asserts p3 is u3-n53's (gen-n52-span.cjs + NIGHT52's fixes) and leaves every other room, door and row bit for bit.
// Re-running gen-n52-span.cjs wipes p3: run this script again after it.
// Run: node Tools/night/e53-k2-p3.cjs, then bash Tools/night/n52-loop-regen.sh span p3
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "p3";
const TAG = "EXT53 K2 (e53-k2-p3.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
if (plan.id !== "n52-span") throw new Error("unexpected plan " + plan.id);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !same(room.rect, [-12, 3.5, 12, 33.5]) || room.e53_why) throw new Error("p3 is not u3-n53's (already run?)");
const one = (arr, pred, what) => {
  const hits = arr.filter((o) => o.zone === R && pred(o));
  if (hits.length !== 1) throw new Error(`p3: ${hits.length} ${what} (not u3-n53's?)`);
  return hits[0];
};
const box = (min, max) => one(plan.boxes, (b) => same(b.min, min) && same(b.max, max), "boxes " + JSON.stringify(min));
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};

// ---- 1. AE -> THE WINCH BALCONY; the void to the east wall
const ae = box([7, 0, 13], [12, 4.2, 23.5]);
ae.min = [9, -14, 13];
ae.max = [12, 4.2, 17];
ae.why = "THE WINCH BALCONY (4.2 m, the third plane, 3 x 4 m on the east wall over the void's north-east corner, its pier from the void's floor; it was AE, a 13 x 5 m stand with a ramp from each deck): the spitter's high perch, walked to only by its north ramp from the exit deck; from the entry deck it is across the void: a kill buys the pull at RE over it (or the KNOT), the walk is the long way round over AW (KILLS GIVE PULLS.); the winch spring puts you on it from the exit deck (" + TAG + "; I1 95 %: the room was a mirror of two stands)";
ae.note = ae.why;
const voidPit = one(plan.pits, (q) => same(q.rect, [-7, 13.5, 7, 23.5]), "void pits");
voidPit.rect = [-7, 13.5, 12, 23.5];
voidPit.why = voidPit.why.replace("(14 x 10 m between the decks,", "(19 x 10 m between the decks, to the east wall since EXT53 K2 (AE cut to the winch balcony: the high perch is across the void from the entry deck, a pull's job),");
if (!voidPit.why.includes("to the east wall since EXT53 K2")) throw new Error("the void's why is not u3-n53's");
voidPit.note = "the void";
const cheek = box([6.4, -14, 11], [7, 5.2, 28]);
note(cheek, "the void runs to the east wall now: the cheek stands free in it, a blade with the drop on both sides; its ride is still the east crossing (the walk round is over AW, the far side); the winch balcony stands 2 m east of its north end, the pull at RE takes you onto it");
const winch = box([11, 4.2, 17], [12, 14, 19.5]);
winch.min = [11, 4.2, 15.5];
winch.max = [12, 14, 17];
note(winch, "onto the winch balcony's south end (x 11..12, z 15.5..17), from the balcony's top to the roof: the spitter's cover on the perch, its back to the void");

// ---- 2. AE's south ramp cut; the winch spring onto the balcony
const aeS = one(plan.ramps, (q) => same(q.from, [10.5, 31.5]) && same(q.to, [10.5, 23.5]), "AE south ramps");
plan.ramps.splice(plan.ramps.indexOf(aeS), 1);
const aeN = one(plan.ramps, (q) => same(q.from, [10.5, 5.5]) && same(q.to, [10.5, 13]), "AE north ramps");
note(aeN, "the winch balcony's only walkers' ramp now (AE's south ramp is cut: it would land in the void): the husks' way up to the spitter and yours on foot; never a dead end, the pull at RE and the drop are the other ways off");
const spring = one(plan.pads, (q) => same(q.at, [8, 0, 10]), "pads");
if (!same(spring.target, [9.5, 4.2, 16.5])) throw new Error("the winch spring's target is not u3-n53's");
spring.target = [10, 4.2, 14.5];
spring.why += " (" + TAG + ": its target onto the winch balcony's open half, west of the winch)";

// ---- the room row: the fall return, nodes, proofs, frames, notes
const fr = room.fallReturn43;
if (!same(fr.by.map((b) => b.rect), [[-7, 18.5, 7, 23.5], [-7, 13.5, 7, 18.5]])) throw new Error("p3's fall return is not u3-n53's");
fr.by[0].rect = [-7, 18.5, 12, 23.5];
fr.by[1].rect = [-7, 13.5, 12, 18.5];
fr._why += " (EXT53 K2: both halves to the east wall with the void)";
const swap = (arr, from, to, what) => {
  const i = arr.findIndex((n) => same(n, from));
  if (i < 0) throw new Error("p3: " + what + " " + JSON.stringify(from) + " is not u3-n53's");
  arr[i] = to;
};
for (const arr of [room.highNodes, room.waveNodes43[1]]) {
  swap(arr, [9, 4.2, 21], [10.2, 4.2, 13.8], "east node"); // was on AE's south half (the void now): the balcony's north end
  swap(arr, [9, 4.2, 15.5], [9.6, 4.2, 16.4], "east node"); // the balcony's open south-west corner, beside the winch
}
if (!same(room.proofs[0].points, [[0, 0, 31], [-10.5, 0, 31], [-10.5, 2.5, 24], [-9.5, 2.5, 13], [-10.5, 2.5, 12.3], [-10.5, 0, 6], [3, 0, 6], [5, 0, 4.6], [10.5, 0, 4.9], [10.5, 4.2, 14], [10.5, 4.2, 22], [10.5, 0, 32.3], [0, 0, 31]])) throw new Error("p3's walk proof 0 is not u3-n53's");
// walk proof 0: the walked crossing (AW: up its south ramp, along it, down its north ramp), the exit deck, the balcony by its north ramp
room.proofs[0].points = [[0, 0, 31], [-10.5, 0, 31], [-10.5, 2.5, 24], [-9.5, 2.5, 13], [-10.5, 2.5, 12.3], [-10.5, 0, 6], [3, 0, 6], [5, 0, 4.6], [10.5, 0, 4.9], [10.5, 4.2, 13.5], [10, 4.2, 15.2]];
if (!same(room.proofs[2].points, [[9.5, 4.2, 16], [10.5, 4.2, 13.5], [10.5, 0, 4.3]])) throw new Error("p3's walk proof 2 is not u3-n53's");
room.proofs[2].points = [[9.6, 4.2, 16.2], [10.5, 4.2, 13.5], [10.5, 0, 4.3]];
if (!same(room.frames52.high, [9.5, 4.2, 21, 120, -0.2])) throw new Error("p3's high frame is not u3-n53's");
room.frames52.high = [10, 4.2, 14.2, 125, -0.2]; // from the balcony over the void to AW and the entry deck
room.grapple = room.grapple.replace("RW over AW and RE over AE:", "RW over AW and RE over the winch balcony (EXT53 K2: the high perch across the void from the entry deck, RE its pull):");
if (!room.grapple.includes("RE over the winch balcony")) throw new Error("p3's grapple text is not u3-n53's");
room.e53_why = "EXT53 K2 (Tools/night/e53-k2-p3.cjs; DECIDE53 D-19): the mirror broken by the pull: the void runs to the east wall, AE cut to THE WINCH BALCONY (3 x 4 m, 4.2 m, its north ramp only), AE's south ramp cut, the east cheek a free blade; AW the walkers' low bridge";

// ---- the guards: every other room, door and row bit for bit
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (o) => o.zone !== R && o.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside p3 changed");
}
for (const k of ["props", "lights", "tunnels", "grapple"]) if (!same(before[k], plan[k])) throw new Error(k + " changed (p3's stay)");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pads", "pits"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps", "pits", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a p3 row without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("e53-k2-p3: p3 KNOT YARD second pass: the void to the east wall, AE -> THE WINCH BALCONY [9..12 x 13..17, 4.2 m], AE's south ramp cut, the winch on the balcony");

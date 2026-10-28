// EXT53 builder M, b1's ride fix (the lead, 11:4x: foundry's verbs37 row 9 RED on n53-r3 with e53-m-b1.cjs: "WallRideStart 3, longest single
// ride 1.15 s on a 25 m wall 11.5 m high at the start; rides [0.23-0.94, 0.98-2.13, 2.17-2.97], end reasons [away, jump, away]").
// The cause: e53-m-b1.cjs cut the west crane (its cab x -10..-5.5 and its ramp x -10..-7 stood against the west wall north of the river),
// which bared the room's west wall (x -10) from z -180 to -165. The reel's wall pick (Bot/VerbsWall39.Pick: rays from the room's centre, the
// longest straight run 0.7 m off a wall) then found a 25 m run: the bare room wall (face x -10) from z ~-174, on to the WEST CRANE RAIL (face
// x -9.4, z -165..-148). At z -165 the rail's north end stands 0.6 m proud of the room wall, square across the rider's lane: the rider
// (along x ~-9.6) meets that end face and is pushed off the wall ("away", 0.71 s into the first ride), re-attaches on the rail, and rides
// off its south end ("away" again).
// The fix: THE WEST CRANE RAIL runs the whole west wall, north wall to south wall (z -180..-148, 32 m; ride36 lz [0, 32]), its cyan strip
// with it: one flush face, no step, no gap. In play it is the crane's rail with its cab gone: from the entry corner over the west bridge
// and on behind the tipped ladle into the lane where the first file hides (ride into the ambush), or jump off east onto the ladle's lip.
// Asserts b1 is e53-m-b1.cjs's; every other room bit for bit.
// Run: node Tools/night/e53-m-b1-ride.cjs [--dry]; levelgen + gen-n52-foundry.cjs --finish (as e53-m-b1.cjs)
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
const room = plan.rooms.find((r) => r.id === R);
if (!room.ext53_why || /ride fix/.test(room.ext53_why)) throw new Error("b1 is not e53-m-b1.cjs's (or the ride fix already ran)");
const rail = one(plan.boxes, (b) => b.zone === R && b.ride && same(b.min, [-10, 0, -165]) && same(b.max, [-9.4, 12, -148]), "the west crane rail");
if (!same(rail.ride36, { face: "E", planeX: -9.4, lz: [0, 17] })) throw new Error("the west rail's ride36 " + JSON.stringify(rail.ride36));
// nothing else may stand on the new length of the face (x -10..-9.4, z -180..-165)
const hit = plan.boxes.filter((b) => b.zone === R && b !== rail && b.max[0] > -10 && b.min[0] < -9.4 && b.max[2] > -180 && b.min[2] < -165 && b.max[1] > 0);
const hitR = plan.ramps.filter((r) => r.zone === R && Math.min(r.from[0], r.to[0]) - (r.width || 2) / 2 < -9.4 && Math.max(r.from[1], r.to[1]) > -180 && Math.min(r.from[1], r.to[1]) < -165);
if (hit.length || hitR.length) throw new Error("something stands on the new rail's line: " + JSON.stringify(hit.concat(hitR).map((b) => b.min || b.from)));
rail.min = [-10, 0, -180];
rail.ride36 = { face: "E", planeX: -9.4, lz: [0, 32] };
rail.why = "THE WEST CRANE RAIL (card: Verb it celebrates; NIGHT52 contract item 5; EXT53 e53-m-b1-ride.cjs: the whole west wall, z -180..-148, 32 m, one flush face: with the west cab and its ramp gone the bare room wall north of the river met the rail's 0.6 m proud end at z -165 and knocked a rider off, verbs37 row 9 RED on n53-r3): from the entry corner over the west bridge and on behind the tipped ladle into the lane where the first file hides (ride into the ambush); its jump-off east lands on the ladle's lip; to the roof (its 7 m and 10 m tops were camps, NIGHT52 K)";
rail.note = rail.why;
const strip = one(plan.lights, (l) => l.room === R && l.id === "b1-RW" && same(l.at, [-9.35, 1.2, -156.5]) && same(l.size, [17, 0.2, 0.1]), "the west rail's cyan strip");
strip.at = [-9.35, 1.2, -164];
strip.size = [32, 0.2, 0.1];
room.ext53_why += "; the ride fix (e53-m-b1-ride.cjs): the west crane rail runs the whole west wall (32 m, one flush face)";
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) {
  if (k === "rooms") continue;
  if (Array.isArray(before[k])) {
    const keep = (o) => !o || (o.zone !== R && o.room !== R);
    if (!same(before[k].filter(keep), plan[k].filter(keep))) throw new Error(k + ": a row outside b1 changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
console.log("b1: the west crane rail " + JSON.stringify(rail.min) + ".." + JSON.stringify(rail.max) + ", ride36 lz " + JSON.stringify(rail.ride36.lz) + "; strip " + JSON.stringify(strip.at) + " size " + strip.size[0]);
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);

// NIGHT52 the room loop, builder F (round 4): spire v5 from the P-rank review (the lead's items 5-7), in Design/levels/n52/spire.plan.json:
//   (5) the two roof-high 4 x 4 m entry cheeks made a 4 m slot 5 m from the newel and trapped the gallery's south ramp foot in a pocket:
//       cut to 2.5 m tall (two steps up beside the mouth)
//   (6) the stair wall rode over the ringers' stair (the ride landed on the ramp ~9 m in): the stair 2.5 m east, so the ride runs its 18 m
//       clear over a floor lane and ends at the ringing chamber beside the stair's top
//   (7) the newel's loop was a 26 m2 circle with the east lane pinched to 4 m by the gallery: the rope gallery 4 m west to meet the newel,
//       so the loop runs over the gallery (its two ramps) round the newel and the gallery together
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v5c.cjs, then bash Tools/night/n52-loop-regen.sh spire v5
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v5";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = plan.rooms.find((r) => r.id === R);
if (!room || room.waves.length !== 2 || room.f52c_why) throw new Error("v5 is not round 4's (n52-loop-f-v5b.cjs) state (already run?)");
const X = (lx) => lx - 10, Z = (lz) => -39 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];
const find = (pred, what) => { const b = plan.boxes.find((q) => q.zone === R && pred(q)); if (!b) throw new Error("not found: " + what); return b; };

// (5) the cheeks 2.5 m tall
for (const lx0 of [-6, 2]) {
  const c = find((b) => same(b.min, [X(lx0), 0, Z(4)]) && b.max[1] === 12, "a cheek at lx " + lx0);
  c.max = [c.max[0], 2.5, c.max[2]];
  c.why += " (round 4, P-rank review: 2.5 m tall, was roof-high: the roof-high pair made a 4 m slot 5 m from the newel and pocketed the gallery ramp's foot; now two steps up beside the mouth)";
  c.note = c.why;
}
// (6) the ringers' stair 2.5 m east
const stair = plan.ramps.find((r) => r.zone === R && same(r.from, [X(-10.15), Z(9)]));
if (!stair) throw new Error("the stair moved upstream");
stair.from = [X(-7.65), Z(9)];
stair.to = [X(-7.65), Z(19)];
stair.why = stair.why.replace("under the stair wall", "2.5 m off the stair wall (round 4, P-rank review: the wall's 18 m ride runs clear over the floor lane beside it and ends at the chamber beside the stair's top)");
stair.note = stair.why;
// (7) the rope gallery 4 m west to the newel
const gal = find((b) => same(b.min, [X(6), 0, Z(18)]) && same(b.max, [X(12), 2.5, Z(8)]), "the rope gallery");
gal.min = [X(2), 0, Z(18)];
gal.why = gal.why.replace("THE ROPE GALLERY (deck 2.5 m,", "THE ROPE GALLERY (deck 2.5 m, 10 x 10 m: round 4, P-rank review: 4 m west to meet the newel, so the loop runs over it round the newel and the gallery together; ") ;
gal.note = gal.why;
// the second wave's east spot stood on the gallery's new footprint: north of it
const oldB2 = P(2.5, 0, 17.8), newB2 = P(2.5, 0, 19.2);
room.nodes = room.nodes.map((n) => (same(n, oldB2) ? newB2 : n));
for (const w of room.waveNodes43) for (let i = 0; i < w.length; i++) if (same(w[i], oldB2)) w[i] = newB2;
// the proofs: the stair walk on its new line; the loop walk round the newel and over the gallery
room.proofs = room.proofs.map((pr) => ({ ...pr, points: pr.points.map((q) => (q[0] === X(-10.15) ? [X(-7.65), q[1], q[2]] : q)) }));
room.proofs[0] = { kind: "walk", points: [P(0, 0, 1), P(0, 0, 4.6), P(-4.5, 0, 5.5), P(-4.5, 0, 13), P(-3, 0, 16), P(3, 0, 19.3), P(8, 0, 21), P(10.75, 0, 24.4), P(10.75, 2.5, 17.6), P(8, 2.5, 12), P(10.75, 2.5, 8.4), P(10.75, 0, 1.6), P(7, 0, 5), P(0, 0, 5.2)] };
room.f52c_why = "NIGHT52 F round 4 (Tools/night/n52-loop-f-v5c.cjs; the P-rank review): the cheeks 2.5 m tall, the stair 2.5 m east (the ride clear), the gallery 4 m west to the newel (the loop over it)";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v5 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v5c: v5 P-rank fixes (cheeks 2.5 m, the stair 2.5 m east, the gallery to the newel)");

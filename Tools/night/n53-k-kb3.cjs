// NIGHT53 builder K (row K, queue item k1; NIGHT53.md section 4; DECIDE52 D-28 / D-30, the final GG-persona and P-rank reviews of kb3):
// kiln kb3 3.3 CASTING FLOOR (THE MOULD LINE), its second pass, edited IN PLACE in Design/levels/n52/kiln.plan.json.
//   1. THE INLET: the west bank is cut by a 4 m lava inlet off the mould (x -7..-2, z -213.5..-209.5, the gantry's face to the mould wall),
//      so the west mould wall's outer face is the west side's way north (D-28: "kb3's west ride runs beside a walkable bank": you could walk
//      the bank beside the ride, so the ride was pointless). Walkers keep the west side over the gantry (its two ramps); you ride the wall
//      over the inlet, or jump it.
//   2. THE POSTS: the four floor-to-roof posts stop at the gantry's top + 1 m (3.5 m; D-28: they read as "random pillars that serve no
//      purpose", GG w35's words): each keeps its job (the charger's stun post, the wall jump onto the gantry, the cut of a line) and none
//      stands to the roof; the west south post moves 0.5 m south to stand flush on the inlet's south lip (the jump-off marker).
//   3. THE NORTH CROSSING: the mould line (the lava, both mould walls, the health, its light and rims) moves 2 m south (z -222.5..-205.5 ->
//      -220.5..-203.5): the ride's end, the loop's turn and the exit shared one 3 m strip (D-30); the strip is 5 m now, the apron 6 m.
//   The proofs follow (walk proof 1's west leg goes over the gantry: the bank is cut), wave 1's west-bank node moves off the inlet's lip.
// Asserts kb3 is u3-n52's (gen-n52-kiln.cjs round 5 + D-30's vent) and leaves every other room, door and row bit for bit.
// Re-running gen-n52-kiln.cjs rebuilds kb3 from its function and wipes this: run this script again after it.
// Run: node Tools/night/n53-k-kb3.cjs
//      dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/kiln.plan.json Design/levels/n52/kiln.json --walk
//      node Tools/night/gen-n52-kiln.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/kiln.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const Z = "kb3";
const TAG = "NIGHT53 K (n53-k-kb3.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
if (plan.id !== "n52-kiln") throw new Error("unexpected plan " + plan.id);
const room = plan.rooms.find((r) => r.id === Z);
if (!room || !same(room.rect, [-11, -225.5, 11, -197.5]) || room.n53_why) throw new Error("kb3 is not u3-n52's (already run?)");
const box = (min, max) => {
  const hits = plan.boxes.filter((b) => b.zone === Z && same(b.min, min) && same(b.max, max));
  if (hits.length !== 1) throw new Error(`kb3: ${hits.length} boxes ${JSON.stringify(min)}..${JSON.stringify(max)} (not u3-n52's?)`);
  return hits[0];
};
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};
const S = 2; // the mould line's move south (+z)
const south = (v) => [v[0], v[1], v[2] + S];

// ---- 3. the mould line 2 m south
const westWall = box([-2, -0.8, -222.5], [-1.4, 4, -205.5]);
const eastLip = box([1.4, -0.8, -222.5], [2, 1, -205.5]);
for (const b of [westWall, eastLip]) {
  b.min = south(b.min);
  b.max = south(b.max);
}
if (!same(westWall.ride36, { face: "E", planeX: -1.4, lz: [8, 25] })) throw new Error("the west mould wall's ride36 moved");
westWall.ride36 = { face: "E", planeX: -1.4, lz: [6, 23] };
note(westWall, "the mould line 2 m south (z -220.5..-203.5: the north crossing 5 m, the apron 6 m); its OUTER face is now the west side's way north: the inlet cuts the west bank under it, so a rider on its west face crosses the lava where a walker cannot (ride it, or jump the 4 m inlet)");
note(eastLip, "2 m south with the mould line");
const pit = plan.pits.filter((q) => q.zone === Z && same(q.rect, [-2, -222.5, 2, -205.5]));
if (pit.length !== 1 || plan.pits.filter((q) => q.zone === Z).length !== 1) throw new Error("kb3's mould pit is not u3-n52's");
pit[0].rect = [-2, -220.5, 2, -203.5];
pit[0].why = pit[0].why.replace("8 m in from the door past the apron", "6 m in from the door past the apron (NIGHT53 K: 2 m south, so the north crossing is 5 m, was 3 m: the ride's end, the loop's turn and the exit no longer share one strip, D-30)");
pit[0].note = "the mould line";
const health = plan.props.filter((q) => q.room === Z && q.kind === "healthPickup");
if (health.length !== 1 || !same(health[0].at, [0, 0.6, -214])) throw new Error("kb3's health is not u3-n52's");
health[0].at = [0, 0.6, -212];
health[0].why += ` (${TAG}: 2 m south with the mould line, level with the inlet: the rider over the inlet sees it between the walls)`;
const lava = plan.lights.find((l) => l.id === "kb3-L4");
if (!lava || !same(lava.at, [0, -0.775, -214])) throw new Error("kb3-L4 is not u3-n52's");
lava.at = [0, -0.775, -212];
if (!same(room.rims36.rects, [[-2.5, -222.5, -2, -205.5], [2, -222.5, 2.5, -205.5]])) throw new Error("kb3's rims are not u3-n52's");

// ---- 1. the inlet (x -7..-2: the gantry's face to the mould wall; z -213.5..-209.5: 4 m, centred on the moved mould's middle -212)
const INLET = [-7, -213.5, -2, -209.5];
const inlet = {
  zone: Z, rect: INLET, floorY: -0.8, lava: true, job: "hazard",
  why: "THE INLET (NIGHT53 K; D-28: the west bank beside the ride was walkable, so the ride was pointless): a 4 x 5 m arm of the mould's lava cut across the west bank from the gantry's face to the mould wall, 0.8 m down: the west side's way north is the west mould wall's outer face (ride it over the inlet) or a jump across; the walkers go over the gantry (its two ramps); both ways (A-5): it burns you and knocks you up out to the nearest bank, and a blast or a punch off either bank puts a walker in",
  note: "the inlet",
};
plan.pits.splice(plan.pits.indexOf(pit[0]) + 1, 0, inlet);
room.rims36 = {
  rects: [[-2.5, -220.5, -2, -213.5], [-2.5, -209.5, -2, -203.5], [2, -220.5, 2.5, -203.5], [-7, -214, -2, -213.5], [-7, -209.5, -2, -209]],
  why: room.rims36.why + " (NIGHT53 K: 2 m south with the mould; the west bank's rim broken by the inlet, whose two lips are rims too: a walker on either bank half beside it can be punched in)",
};
// no second light: the level report allows one accent per room (amber / orange); the inlet is lava, its glow the lava's own, and the
// mould's orange pool kb3-L4 (2.5 x 14 m) stands 2 m beside it

// ---- 2. the posts: the gantry's top (2.5 m) + 1 m
const POST = 3.5;
const posts = [
  [box([-7, 0, -210], [-5.5, 14, -208.5]), [-7, 0, -209.5], [-5.5, POST, -208], "floor to roof", "3.5 m (the gantry's top + 1 m; D-28: floor-to-roof posts read as random pillars), 0.5 m south, flush on the inlet's south lip: the marker where the west bank stops (ride the wall or jump the inlet here); still the charger's stun post (a charger that misses you here goes into the lava) and a wall-jump onto the gantry"],
  [box([-7, 0, -217], [-5.5, 14, -215.5]), [-7, 0, -217], [-5.5, POST, -215.5], "floor to roof", "3.5 m (the gantry's top + 1 m): the stun post on the north bank half, where the west ride lands; its top is a wall jump's step onto the gantry, and it still cuts the ladle's line along the bank"],
  [box([4.5, 0, -206.5], [6, 14, -205]), [4.5, 0, -206.5], [6, POST, -205], "floor to roof", "3.5 m (the gantry's top + 1 m): the stun post at the east bank's south end; its top cuts the lip's line to the door at a standing player's eye, not to the roof"],
  [box([4.5, 0, -220.5], [6, 14, -219]), [4.5, 0, -220.5], [6, POST, -219], "floor to roof", "3.5 m (the gantry's top + 1 m): the pivot of the loop's north turn (now a 5 m crossing), a stun post you can still see over from the ladle"],
];
for (const [b, mn, mx, old, why] of posts) {
  if (!b.why.includes(old)) throw new Error("a post's why changed: " + b.why);
  b.min = mn;
  b.max = mx;
  b.why = b.why.replace(old, "post, 3.5 m");
  note(b, why);
}

// ---- the room row: nodes, proofs, notes
const n0 = room.nodes.findIndex((n) => same(n, [-4.5, 0, -214]));
if (n0 < 0) throw new Error("kb3's west-bank node is not u3-n52's");
room.nodes[n0] = [-4, 0, -217.5]; // was 0.5 m off the inlet's north lip: now 4 m north of it on the north bank half, 1.5 m east of the north post
if (!same(room.proofs[0].points, [[0, 0, -200], [-3, 0, -203], [-3, 0, -221], [0, 0, -223], [2.8, 0, -222.5], [2.8, 0, -216], [2.8, 0, -207], [2.8, 0, -203], [0, 0, -200]])) throw new Error("kb3's walk proof 1 is not u3-n52's");
// walk proof 1: the loop, its west leg over the gantry (the inlet cuts the bank): the apron, the gantry's south ramp, the gantry, its north
// ramp, the north crossing (5 m now), the east bank, the apron
room.proofs[0].points = [[0, 0, -200], [-9.5, 0, -198.8], [-9.5, 2.5, -205.5], [-9, 2.5, -218.5], [-9.5, 0, -224], [-3, 0, -223.5], [0, 0, -223], [2.8, 0, -221.5], [2.8, 0, -216], [2.8, 0, -207], [2.8, 0, -202], [0, 0, -200]];
// walk proof 4: the north bank half from the north crossing to the inlet's north lip (the rider's landing side is reachable on foot)
room.proofs.push({ kind: "walk", points: [[-3, 0, -223], [-3.5, 0, -219], [-3.5, 0, -214.5]] });
room.nodes_why += " (NIGHT53 K: the west-bank node off the inlet's lip, on the north bank half)";
room.verb36 = room.verb36.replace("ride the mould walls over the pour", "ride the mould walls over the pour (the west wall's outer face over the inlet: the west side's way north)");
room.n53_why = "NIGHT53 K (Tools/night/n53-k-kb3.cjs; DECIDE52 D-28 / D-30, the second pass): the inlet cuts the west bank (the ride is the west side's way north), the four posts 3.5 m (the gantry's top + 1 m), the mould line 2 m south (the north crossing 5 m)";

// ---- the guards: every other room, door and row bit for bit
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== Z && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
  const keep = (o) => o.zone !== Z && o.room !== Z;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside kb3 changed");
}
for (const k of ["ramps", "pads"]) if (!same(before[k], plan[k])) throw new Error(k + " changed (kb3's ramps and pads stay)");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "pits", "lights", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "pits"]) for (const o of plan[k]) if (o.zone === Z && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a kb3 row without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n53-k-kb3: kb3 THE MOULD LINE second pass: the inlet " + JSON.stringify(INLET) + ", the four posts 3.5 m, the mould line 2 m south (north crossing 5 m)");

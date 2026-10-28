// NIGHT52 the room loop, the persona reviews' fixes for iceline (the lead's; DECIDE52 D-14). The checks decided which findings are real:
//   g8 (the P-rank runner's finding 5, confirmed by `hold52 --room g8`: the column tops at 5 m took 0 hits in 20 s on 3 seeds: a camping spot no
//      walker reaches): the six columns run to the 12 m roof; the east sand tower leaves the signal box ramp's run-out (finding 4: it stood
//      4.25 m past the ramp's foot in its line) for lane E's door end.
//   g5 (GG's finding 4 "random pillars", the P-rank runner's findings 2 and 3): the two door-side ice posts cut; the two inner ice walls cut to
//      their north 4 m (the door chute opens onto both side lanes, the ride walls reached from the door); the shelf hoist turned round (crane ->
//      shelf: the high circuit floor -> crane -> shelf -> ramp -> lane -> door); a west hoist added (floor -> shelf); the east side comes in 1 m
//      (21 x 30 m) so the density holds with two pieces fewer; GG's finding 3 (the ice stack has no identity): a frost lamp on its door face.
// Asserts the rooms are this night's cycle-2 / cycle-3 versions (n52-loop-g5.cjs, n52-loop-g8.cjs); every other room bit for bit.
// Run: node Tools/night/n52-loop-fix1.cjs, then bash Tools/night/n52-loop-regen.sh iceline g5 g8
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/iceline.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const g5 = plan.rooms.find((r) => r.id === "g5"), g8 = plan.rooms.find((r) => r.id === "g8");
if (JSON.stringify(g5.rect) !== "[-11,-158,11,-128]" || !g5.loop52_why) throw new Error("g5 is not cycle 2's (already run?)");
if (JSON.stringify(g8.rect) !== "[-11,-250,11,-220]" || !g8.loop52_why) throw new Error("g8 is not cycle 3's");
const one = (arr, pred, what) => {
  const hits = arr.filter(pred);
  if (hits.length !== 1) throw new Error(what + ": found " + hits.length);
  return hits[0];
};
const near = (a, b) => JSON.stringify(a) === JSON.stringify(b);

// ---- g8 ----
for (const p of plan.props.filter((q) => q.room === "g8" && q.kind === "pillar")) {
  if (!near(p.size, [2.5, 5, 2.5]) || p.at[1] !== 2.5) throw new Error("g8 column " + p.id + " is not 5 m");
  p.at = [p.at[0], 6, p.at[2]];
  p.size = [2.5, 12, 2.5];
  p.why = p.why + " (NIGHT52 loop fix 1: floor to the 12 m roof: at 5 m its top was a perch no walker reaches, hold52 0 hits)";
}
const eTower = one(plan.boxes, (b) => b.zone === "g8" && near(b.min, [7.55, 0, -230.75]), "g8 east sand tower");
eTower.min = [7.55, 0, -228];
eTower.max = [9.05, 4, -226.5];
eTower.why = eTower.why.replace("the pivot you dodge round", "the pivot you dodge round, at lane E's door end (NIGHT52 fix 1: out of the signal box ramp's run-out)");
eTower.note = eTower.why;
g8.waveNodes43[1] = [[-5, 0, -245], [5, 0, -245], [-8.3, 0, -236], [8.3, 0, -232]];

// ---- g5 ----
g5.rect = [-11, -158, 10, -128];
const drop = (b) => b.zone === "g5" && (near(b.min, [-8.1, 0, -134.6]) || near(b.min, [6.9, 0, -134.6]));
if (plan.boxes.filter(drop).length !== 2) throw new Error("g5's door-side posts not found");
plan.boxes = plan.boxes.filter((b) => !drop(b));
const wIce = one(plan.boxes, (b) => b.zone === "g5" && near(b.min, [-5.3, 0, -139]), "g5 west ice wall");
wIce.max = [-4.7, 4, -135];
wIce.ride36 = { face: "W", planeX: -5.3, lz: [7, 11] };
wIce.why = "the WEST ICE WALL (cyan, 4 m x 4 m, 7-11 m in): the short ride off the door's diagonal into the west lane; it splits the bait into lanes for the punch (cut to its north 4 m: the door chute opens onto the west ride wall)";
wIce.note = wIce.why;
const eIce = one(plan.boxes, (b) => b.zone === "g5" && near(b.min, [4.7, 0, -139]), "g5 east ice wall");
eIce.max = [5.3, 4, -135];
eIce.ride36 = { face: "E", planeX: 5.3, lz: [7, 11] };
eIce.why = "the EAST ICE WALL (cyan, 4 m, the west's mirror): the short ride into the east lane toward the crane's ramp";
eIce.note = eIce.why;
// the east side 1 m in
const eRide = one(plan.boxes, (b) => b.zone === "g5" && near(b.min, [10, 0, -146.5]), "g5 east ride wall");
eRide.min = [9, 0, -146.5]; eRide.max = [10, 6, -128.5]; eRide.ride36 = { face: "W", planeX: 9, lz: [0.5, 18.5] };
const crane = one(plan.boxes, (b) => b.zone === "g5" && near(b.min, [5, 0, -158]), "g5 crane deck");
crane.max = [10, 4.5, -149];
const cab = one(plan.boxes, (b) => b.zone === "g5" && near(b.min, [9, 4.5, -158]), "g5 crane cab");
cab.min = [8, 4.5, -158]; cab.max = [10, 6.5, -156];
const cRamp = one(plan.ramps, (r) => r.zone === "g5" && near(r.from, [8.5, -140]), "g5 crane ramp");
cRamp.from = [7.5, -140]; cRamp.to = [7.5, -149];
const pads = plan.pads.filter((p) => p.zone === "g5");
if (pads.length !== 2) throw new Error("g5 pads");
const hoist = one(pads, (p) => near(p.at, [2.5, 0, -136]), "the ice hoist");
hoist.target = [7, 4.5, -154];
const shelfHoist = one(pads, (p) => near(p.at, [-7.5, 2, -150]), "the shelf hoist");
shelfHoist.at = [6.5, 4.5, -151];
shelfHoist.target = [-7.5, 2, -153];
shelfHoist.apexAbove = 2;
shelfHoist.why = "THE CRANE HOIST (a jump pad on the crane deck, turned round from the shelf's: NIGHT52 fix 1): crane -> shelf over the exit: the high circuit (the ice hoist up, the crane hoist across, the shelf ramp down, the west lane, the door) so no tier is a dead end";
shelfHoist.note = "the crane hoist";
plan.pads.push({ zone: "g5", at: [-7.5, 0, -131.5], target: [-8, 2, -154], apexAbove: 3, job: "line", why: "THE WEST HOIST (a jump pad at the west lane's door end): floor -> the west shelf over the lane, so the shelf (the second pack's pour) has an upward line from the door too", note: "the west hoist" });
g5.highNodes = [[-8, 2, -152], [-8, 2, -156], [8.5, 4.5, -152], [6.5, 4.5, -155]];
g5.frames52.high = [7, 4.5, -153, 161, -0.12];
const L4 = one(plan.lights, (l) => l.id === "g5-L4", "g5-L4");
L4.at = [7.5, 11.95, -153.5];
const L1i = plan.lights.findIndex((l) => l.id === "g5-L1");
plan.lights.splice(L1i + 1, 0, { id: "g5-frost", room: "g5", kind: "wall", at: [0, 5, -140.95], normal: [0, 0, 1], size: [3, 0.6, 0.1], color: "white", range: 9, intensity: 1.3, why: "the ICE STACK's frost lamp on its door face (GG's review: the centrepiece names the room): the landmark lit from the door" });
g5.loop52_why += " Fix 1 (n52-loop-fix1.cjs, the persona reviews): 21 x 30 m; the door-side posts cut, the inner ice walls 4 m, the crane hoist turned round, the west hoist added, the frost lamp.";
g8.loop52_why += " Fix 1 (n52-loop-fix1.cjs): the columns to the roof (their 5 m tops were 0-hit perches), the east tower out of the ramp's run-out.";

for (let i = 0; i < plan.rooms.length; i++) if (!["g5", "g8"].includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads"]) {
  const a = (before[k] || []).filter((b) => !["g5", "g8"].includes(b.zone || b.room)), b = (plan[k] || []).filter((x) => !["g5", "g8"].includes(x.zone || x.room));
  if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error(k + ": a row outside g5 / g8 changed");
}
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-fix1: g8 columns to the roof, east tower moved; g5 21 x 30 m, posts cut, ice walls 4 m, crane hoist turned, west hoist, frost lamp");

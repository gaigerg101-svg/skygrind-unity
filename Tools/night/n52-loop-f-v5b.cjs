// NIGHT52 the room loop, builder F (round 4): spire v5 THE RINGERS' STAIR II (Tools/night/n52-loop-f-v5.cjs) fixed from the lead's midpoint
// flow table and the GG-persona review, in Design/levels/n52/spire.plan.json:
//   (A) chained seed 17 capped 240 s in v5: the walk pilot wedged in a 0.6 x 1 m notch between the west wall, the stair's west side and
//       the stair wall's north end (lz 18..19): the stair wall runs on to the ringing chamber (lz 1..19, 18 m), the notch closed
//   (B) the newel bare and the ride bar behind the walkable stair: the newel becomes a ride wall too (its 5 m faces cyan): circling the
//       newel is a ride; a second wave (2 from the ringing chamber, 2 from behind the newel; overlap46 releases it as wave 1 thins)
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v5b.cjs, then bash Tools/night/n52-loop-regen.sh spire v5
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
if (!room || !same(room.rect, [-22, -65, 2, -39]) || room.waves.length !== 1) throw new Error("v5 is not round 3's THE RINGERS' STAIR II (already run?)");
const X = (lx) => lx - 10, Z = (lz) => -39 - lz;
const P = (lx, y, lz) => [X(lx), y, Z(lz)];

// (A) the stair wall on to the chamber
const wall = plan.boxes.find((b) => b.zone === R && b.ride && same(b.min, [X(-12), 0, Z(18)]));
if (!wall) throw new Error("the stair wall moved upstream");
wall.min = [X(-12), 0, Z(19)];
wall.ride36 = { face: "E", planeX: X(-11.4), lz: [1, 19] };
wall.why = wall.why.replace("17 m x 7 m on the west wall", "18 m x 7 m on the west wall, on to the ringing chamber's face (round 4: at 17 m a 0.6 x 1 m notch between its end, the wall and the stair held the walk pilot 240 s, chained seed 17)");
wall.note = wall.why;
const strip = plan.lights.find((l) => l.id === "v5-RW");
strip.at = [X(-11.35), 1.2, Z(10)];
strip.size = [18, 0.2, 0.1];
// (B) the newel a ride wall
const newel = plan.boxes.find((b) => b.zone === R && same(b.min, [X(-2), 0, Z(14)]) && same(b.max, [X(2), 12, Z(9)]));
if (!newel || newel.kind !== "hull") throw new Error("the newel moved upstream");
newel.kind = "wall";
newel.ride = true;
newel.job = "line";
newel.why = "THE NEWEL (card: Spatial law; contract item 4, the loop's central mass; round 4, GG review: 'circling the newel is a ride'), floor to roof, its 5 m east and west faces cyan ride faces: ride round it from the mouth (a wall jump at each corner), the stair's centre post the loop runs round; it breaks the chamber's line of sight onto the entry mouth";
newel.note = newel.why;
const at = plan.lights.findIndex((l) => l.id === "v5-RW");
plan.lights.splice(at + 1, 0,
  { id: "v5-NW", room: R, kind: "wall", at: [X(-2.05), 1.2, Z(11.5)], normal: [-1, 0, 0], size: [5, 0.2, 0.1], color: "cyan", range: 5, intensity: 1, job: "light", why: "the newel's west ride face's cyan strip (V-3)", note: "the newel's west ride strip" },
  { id: "v5-NE", room: R, kind: "wall", at: [X(2.05), 1.2, Z(11.5)], normal: [1, 0, 0], size: [5, 0.2, 0.1], color: "cyan", range: 5, intensity: 1, job: "light", why: "the newel's east ride face's cyan strip (V-3)", note: "the newel's east ride strip" });
// (B) the second wave
const B1 = P(-1.5, 0, 17.8), B2 = P(2.5, 0, 17.8);
room.nodes = room.nodes.concat([B1, B2]);
room.waves = [room.waves[0], ["swarmer", "swarmer", "husk", "husk"]];
room.waveNodes43 = [room.waveNodes43[0], [P(-7.5, 5, 21), P(-9, 5, 23.5), B1, B2]];
room.waves52_why += "; round 4 (GG review): a second wave, two from the ringing chamber and two from behind the newel (overlap46 releases it as wave 1 thins)";
room.f52b_why = "NIGHT52 F round 4 (Tools/night/n52-loop-f-v5b.cjs): the stair wall on to the chamber (the notch that held chained seed 17 closed), the newel a ride wall, a second wave";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v5 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "lights"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v5b: v5 fixed (stair wall 18 m, the newel a ride wall, a second wave)");

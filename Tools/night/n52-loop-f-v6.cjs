// NIGHT52 the room loop, builder F (round 4; v6 handed over by the lead): spire v6 THE ROPE CHOIR II (the lead's, Tools/night/n52-loop-v6.cjs)
// fixed from the persona reviews (DECIDE52 D-16), in Design/levels/n52/spire.plan.json:
//   1. the report's "completable: v6": proof 2 walked x -1 through the east rood screen: re-routed (along the east lane's west curb)
//   2. P-rank: the two rood screens fenced the entry third (1.5-2 m floor slots, the lap wrapped the west screen): both cut, the bell posts kept;
//      the island's north ramp (a duplicate route) -> a ring over the island -> loft gap (the rope the short way up); GG: a lava strip in the
//      east lane beside the island so the east ride is the crossing there, with a 1 m curb each side (the walkers' single files kept, and every
//      lava edge a walkable floor: the walk pilot leaves lava by the nearest edge, foundry a5's round 1) and a ring over the strip
//   3. the health off the organ loft (a camp with health) onto the east curb by the ride wall over the lava (the riskiest floor spot)
//   4. frames52.mid re-aimed from the west screen's blank face at the island and the fight
// Every other room, door and piece is as before (asserted). Run: node Tools/night/n52-loop-f-v6.cjs, then bash Tools/night/n52-loop-regen.sh spire v6
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/spire.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "v6";
const room = plan.rooms.find((r) => r.id === R);
if (!room || JSON.stringify(room.rect) !== "[-23,-101,1,-69]" || !/ROPE CHOIR II/.test(room.loop52_why || "")) throw new Error("v6 is not the lead's THE ROPE CHOIR II");
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const take = (arr, pred, what) => { const i = arr.findIndex(pred); if (i < 0) throw new Error("not found: " + what); return arr.splice(i, 1)[0]; };

// 2. the rood screens cut, the island's north ramp cut
take(plan.boxes, (b) => b.zone === R && same(b.min, [-13.5, 0, -80.6]), "the west rood screen");
take(plan.boxes, (b) => b.zone === R && same(b.min, [-2.4, 0, -80.6]), "the east rood screen");
take(plan.ramps, (b) => b.zone === R && same(b.from, [-6.5, -97]) && same(b.to, [-6.5, -91]), "the island's north ramp");
const southRamp = plan.ramps.find((b) => b.zone === R && same(b.from, [-6.5, -75]));
southRamp.why = "the island's SOUTH RAMP (2.4 m over 6 m; the card: 'the fodder's file into the TORCH'): wave 1 comes down it at you; the walkers' one way up (NIGHT52 F: the north ramp, a duplicate route, cut for the island -> loft ring)";
southRamp.note = southRamp.why;
// the lava strip, its two curbs (pieces: the walkers' single files) and the moved bell hoist
const at = plan.boxes.findIndex((b) => b.zone === R && same(b.min, [-10.5, 2.4, -82]));
const curb = (x0, x1, why) => ({ zone: R, kind: "deck", min: [x0, -0.8, -91], max: [x1, 0, -81], job: "line", why, note: why });
plan.boxes.splice(at, 0,
  curb(-2.5, -1.5, "the island's CURB (flush, 1 m wide, cast iron, card: Hazard both ways): the walkers' single file between the island's face and the lava strip (GG: walker routes kept); a husk on it is one punch from the lava; every lava edge a floor (the walk pilot leaves lava by the nearest edge)"),
  curb(0, 1, "the RIDER'S CURB (flush, 1 m wide, cast iron, under the east ride wall): the other single file beside the strip, the health's ledge (the riskiest floor spot); the ride over it is the crossing"));
plan.pits = plan.pits || [];
const pitWhy = "the BELL-METAL STRIP (lava 1.5 x 10 m in the east lane beside the island; GG, DECIDE52 D-16: the east ride is the crossing there): to you a burn and a knock-up out to a curb; to an enemy death (Lava30): punch a husk off a curb or the island's edge into it";
plan.pits.push({ zone: R, rect: [-1.5, -91, 0, -81], floorY: -0.8, lava: true, job: "hazard", why: pitWhy, note: pitWhy });
const hoist = plan.pads.find((p) => p.zone === R && same(p.at, [-0.5, 0, -89]));
if (!hoist) throw new Error("the bell hoist moved upstream");
hoist.at = [-0.5, 0, -94.5];
hoist.why = "THE BELL HOIST (a jump pad, the upward line, A5): from the east lane's north end (past the lava strip) over to the organ loft (6 m): the way to wave 3's stand without the ramp";
hoist.note = "the bell hoist";
// the rings: + the island -> loft gap, + the lava strip
room.grappleNodes = room.grappleNodes.concat([[-11.5, 8, -93], [-0.75, 6, -86]]);
room.grapple = "SIX rings (the card's BELL ROPES' iron grips): one 3 m over the island (floor -> island), one 3 m over the high stalls, one 3 m over the organ loft's east end (island -> loft), one 8 m over the east lane's door end (the pull onto the east ride's line from the door), (NIGHT52 F, D-16) one over the island -> loft gap (the rope the short way up, the island's north ramp cut) and one over the lava strip (the rope over the bell-metal beside the ride)";
// 3. the health
const hp = plan.props.find((q) => q.id === "v6-P1");
if (!hp || !same(hp.at, [-14.5, 6.9, -98])) throw new Error("v6-P1 moved upstream");
hp.at = [0.5, 0.9, -86];
hp.why = "the riskiest floor spot pays (A20; D-16): the rider's curb beside the lava strip under the east ride, in the island's and the drones' fire (off the organ loft: a camp with health on it)";
// 1. + 4. the proofs and frames
room.proofs = [
  { kind: "walk", points: [[-10, 0, -71], [-10, 0, -75]] },
  { kind: "walk", points: [[-2, 0, -78], [-2, 0, -82], [-2, 0, -90], [-2, 0, -93.5], [-1, 0, -96], [-10, 0, -97], [-10, 0, -99]] },
  { kind: "walk", points: [[-21.5, 0, -70], [-21.5, 3.2, -77], [-20, 3.2, -90]] },
  { kind: "walk", points: [[-15.5, 0, -82.5], [-15.5, 6, -95], [-17, 6, -98]] },
];
room.frames52 = Object.assign({}, room.frames52, { mid: [-13, 0, -79, -43, 0.1] });
// the east ride wall 6 -> 10 m: its 0.6 m top at 6 m was a perch a ride's jump-off reaches (hold52: 2 hits in 20 s, the fewest of 3 seeds)
const eastRide = plan.boxes.find((b) => b.zone === R && b.ride && same(b.min, [1, 0, -97]));
if (!eastRide || eastRide.max[1] !== 6) throw new Error("the east ride wall moved upstream");
eastRide.max = [eastRide.max[0], 10, eastRide.max[2]];
eastRide.why += " (NIGHT52 F: 10 m tall, its top out of a jump-off's reach: at 6 m it was a near-camp, 2 hits in hold52's 20 s)";
eastRide.note = eastRide.why;
room.f52_why = "NIGHT52 F round 4 (Tools/night/n52-loop-f-v6.cjs; DECIDE52 D-16): the rood screens and the island's north ramp cut; the lava strip with its two curbs and ring, the island -> loft ring; the health on the rider's curb; proof 2 and frames52.mid fixed; the east ride wall 10 m tall";

for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (b) => b.zone !== R && b.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside v6 changed");
}
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "props", "pads", "pits"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n52-loop-f-v6: v6 fixed (screens and north ramp cut; lava strip, two curbs, two rings; health, proof 2, frames52.mid)");

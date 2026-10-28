// NIGHT53 builder K (row K, queue item k2; NIGHT53.md section 4; DECIDE52 D-28 / D-30): span p1 4.1 HOOK YARD (THE HOOK YARD II), the
// grapple's teach room, its second pass, edited IN PLACE in Design/levels/n52/span.plan.json (p1 as n52-loop-f-p1.cjs + p1b + p1c left it).
//   1. THE GRAPPLE REQUIRED (D-28: "p1's south ramp makes the grapple optional in the room that teaches it"): the terrace's south ramp is
//      cut (4 m over 10 m beside the chain wall: the walk onto the terrace from the start was as short as the pull); the walkers keep the
//      back ramp (the north apron -> the terrace's north end), which now tops out on THE RAMP HEAD (a 4 m landing beside the terrace: the
//      ramp ran into the north wall, so levelgen's walkers never used it; the south ramp had carried "enemies reach every tier"). The HOOK PAD is cut too: it stood on the exit's walk line (walk proof 2,
//      0.29 m: I4, the D-30 bug class: every walk to the exit launched you) and was the other non-grapple way onto the terrace.
//   2. THE DENSITY RE-EARNED BY THE HOOK (not filler): the JIB RING, 8.6 m up over the well panel's south end: pull up the wall and let
//      go onto the panel's face high, into the ride over the void (the card's air loop: the hoist block -> the well panel -> the engine
//      deck). Round 1's HOIST JIB (a 5.6 m hook-only ledge under it) was cut: hold52 found its top a 0-hit camping spot. With the ramp head
//      and the winch base: 20 -> 21 pieces (2.60 -> 2.73 per 100 m2). The engine deck 2.4 m (was 2: inside the nav's 2.2 m jump, so I3
//      found the engine ring's pull walkable in 1.22 x its length).
//   3. D-30's ride line (the well panel's ride was crossed by the south railing's end, the engine deck's 2 m face and the hoist engine):
//      THE WINCH BASE, a 1.2 m run-up block at the panel's south end (the ride starts above the railing and lands on the deck, not its
//      face); the south railing stops 1 m short of the panel's face; the hoist engine 1.5 m west (the ride's run-out along the wall clear).
// Asserts p1 is u3-n52's; every other room, door and row bit for bit. Re-running gen-n52-span.cjs wipes p1: run n52-loop-f-p1.cjs, p1b,
// p1c, then this script.  Run: node Tools/night/n53-k-p1.cjs, then bash Tools/night/n52-loop-regen.sh span p1
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN = path.join(ROOT, "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const R = "p1";
const TAG = "NIGHT53 K (n53-k-p1.cjs)";
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const Z = (lz) => 123.5 - lz;
if (plan.id !== "n52-span") throw new Error("unexpected plan " + plan.id);
const room = plan.rooms.find((r) => r.id === R);
if (!room || !same(room.rect, [-12, 91.5, 12, 123.5]) || !room.f52c_why || room.n53_why) throw new Error("p1 is not u3-n52's (round 10; already run?)");
const one = (arr, pred, what) => {
  const hits = arr.filter(pred);
  if (hits.length !== 1) throw new Error(`p1: ${hits.length} x ${what} (not u3-n52's?)`);
  return hits[0];
};
const box = (min, max, what) => one(plan.boxes, (b) => b.zone === R && same(b.min, min) && same(b.max, max), what);
const note = (b, why) => {
  b.why = `${b.why} (${TAG}: ${why})`;
  if (b.note !== undefined) b.note = b.why;
};

// ---- 1. the south ramp and the hook pad cut
const southRamp = one(plan.ramps, (r) => r.zone === R && same(r.from, [-7.5, Z(10)]) && same(r.to, [-7.5, Z(20)]), "the terrace's south ramp");
plan.ramps.splice(plan.ramps.indexOf(southRamp), 1);
const hookPad = one(plan.pads, (q) => q.zone === R && same(q.at, [-1, 0, Z(24)]) && same(q.target, [-9, 4, Z(28)]), "the hook pad");
plan.pads.splice(plan.pads.indexOf(hookPad), 1);
const rampProof = room.proofs.findIndex((pr) => same(pr.points, [[-4, 0, Z(4)], [-7.5, 0, Z(9.5)], [-7.5, 4, Z(20.5)], [-9, 4, Z(25)]]));
if (rampProof < 0) throw new Error("p1's south-ramp walk proof is not round 10's");
room.proofs.splice(rampProof, 1);
const back = one(plan.ramps, (r) => r.zone === R && same(r.from, [-4.75, Z(22)]) && same(r.to, [-4.75, Z(32)]), "the back ramp");
// the back ramp ran up beside the terrace's east face into the north wall: its top met no deck, so levelgen's walker links never used
// it (the south ramp alone carried "enemies reach every tier"; with it cut the report found the terrace's nodes unreached). Now it tops
// out 1.5 m short of the wall onto THE RAMP HEAD, a 4 m landing beside the terrace's north end: ramp -> landing -> terrace
back.to = [-4.75, Z(30.5)];
note(back, "now the terrace's only walk: the walkers' way up and yours on foot, from the north apron; 4 m over 8.5 m onto THE RAMP HEAD (it ran into the north wall: no walker link); the pull to the GREAT HOOK is the way up from the yard (the south ramp beside the chain wall is cut: it made the grapple optional)");
const rampProof5 = room.proofs.find((pr) => same(pr.points, [[-2.5, 0, Z(21)], [-4.75, 0, Z(21.6)], [-4.75, 4, Z(31.8)], [-8, 4, Z(31)]]));
if (!rampProof5) throw new Error("p1's back-ramp walk proof is not round 10's");
rampProof5.points = [[-2.5, 0, Z(21)], [-4.75, 0, Z(21.6)], [-4.75, 4, Z(30.8)], [-4.75, 4, Z(31.2)], [-8, 4, Z(31)]];
const terrace = box([-12, 0, Z(32)], [-6, 4, Z(20)], "the terrace");
note(terrace, "its south ramp and the hook pad cut: from the yard it is the GREAT HOOK's (the lesson's pull) or the chain wall's jump-off; walkers come up the back ramp only");

// ---- 3. D-30's ride line
const railS = box([7, 0, Z(12)], [11.4, 1, Z(11.7)], "the south railing");
railS.max = [10.4, 1, Z(11.7)];
note(railS, "1 m short of the well panel's face (x 10.4): D-30, its end crossed the panel's ride line (a rider from the floor caught it); the gap is the ride's mount beside the winch base");
const engine = box([10.5, 2, Z(32)], [12, 12, Z(29.5)], "the hoist engine");
engine.min = [9, 2.4, Z(32)];
engine.max = [10.5, 12, Z(29.5)];
note(engine, "1.5 m west (x 9..10.5): D-30, it stood on the well panel's ride line at the deck's corner; the 1.5 m along the wall is the ride's run-out");
const panel = box([11.4, -14, Z(28)], [12, 12, Z(10)], "the well panel");
note(panel, "its ride starts off THE WINCH BASE (1.2 m) or from a pull to the JIB RING over its south end, so it comes down on the engine deck (2.4 m) above its face");

// ---- I3 (the lead's identity check: every ring of the grapple's teach room needs the pull): the engine deck stood 2 m up, inside the
// nav's 2.2 m jump, so the ENGINE RING over it was walked in 3.7 m against a 3.0 m pull (1.22). The deck, its corner engine, its ramp
// and its nodes go to 2.4 m (out of a jump from the floor; still its own plane: 4.0 - 2.4 > 1.5 m), the well pad's target with them
const deck = box([7, 0, Z(32)], [12, 2, Z(24)], "the engine deck");
deck.max = [12, 2.4, Z(24)];
note(deck, "2.4 m (was 2): out of a jump from the floor (the nav's jump is 2.2 m), so the engine ring's pull and the well panel's ride are its ways up, the engine ramp the walk");
const eramp = one(plan.ramps, (r) => r.zone === R && same(r.from, [2, Z(30.5)]) && same(r.to, [7, Z(30.5)]) && r.rise === 2, "the engine ramp");
eramp.rise = 2.4;
note(eramp, "2.4 m over 5 m, with the deck");
const wellPad = one(plan.pads, (q) => q.zone === R && same(q.at, [9.5, 0, 119]) && same(q.target, [9.5, 2, Z(26)]), "the well pad");
wellPad.target = [9.5, 2.4, Z(26)];
const up = (v) => (v[1] === 2 && v[0] >= 7 ? [v[0], 2.4, v[2]] : v);
room.highNodes = room.highNodes.map(up);
room.waveNodes43 = room.waveNodes43.map((w) => w.map(up));
const pr4 = room.proofs.find((pr) => same(pr.points, [[0, 0, Z(27)], [1.2, 0, Z(30.5)], [7, 2, Z(30.5)], [8.5, 2, Z(26)]]));
if (!pr4) throw new Error("p1's engine-ramp walk proof is not round 10's");
pr4.points = [[0, 0, Z(27)], [1.2, 0, Z(30.5)], [7, 2.4, Z(30.5)], [8.5, 2.4, Z(26)]];

const piece = (kind, lx0, y0, lz0, lx1, y1, lz1, job, why) => ({ zone: R, kind, min: [lx0, y0, Z(lz1)], max: [lx1, y1, Z(lz0)], job, why, note: why });
const winch = piece("hull", 10.4, 0, 7, 12, 1.2, 10, "line",
  "THE WINCH BASE (NIGHT53 K; D-30's run-up block): a 1.2 m block against the east wall at the well panel's south end: run up onto it and jump north onto the panel, so the ride starts above the railing and comes down on the engine deck, not into its face; the well pad's arc passes 0.9 m west of it");
// THE HOIST JIB (a 5.6 m hook-only ledge over the panel's south end) was tried in round 1 and cut: hold52 --perches found its top a
// 0-hit camping spot (20 s with both waves live). Its ring stays: a pull up the panel's south end, released onto the wall's face: the ride.
const head = piece("deck", -6, 0, 30.5, -3.5, 4, 32, "climb",
  "THE RAMP HEAD (NIGHT53 K): the back ramp's 4 m landing against the north wall beside the terrace's north end: the walkers climb the back ramp onto it and step west onto the terrace (GG NIGHT20: a route enemies can climb; the terrace's only walk now that its south ramp is cut)");
const engineAt = plan.boxes.indexOf(engine);
plan.boxes.splice(engineAt + 1, 0, winch, head);

// ---- 2. the jib ring (an anchor; the room's `grapple` sentence is its reason)
if (!same(room.grappleNodes, [[-6, 9, Z(21)], [0, 9, Z(18)], [9.5, 8, Z(18)], [9, 7, Z(28.5)]])) throw new Error("p1's rings are not round 10's");
room.grappleNodes.push([10.5, 8.6, Z(8.5)]);
room.grapple += "; NIGHT53 K: a FIFTH, the JIB RING 8.6 m up over the well panel's south end: pull up the wall from the floor or the winch base and let go onto the panel's face high, into the ride over the void (the hook's way into the air loop; 27 deg off the hoist block from the start, so the aims do not overlap)";

// ---- the start 5 m into the yard (round 2; T-119's cause, as kb3's retry spawn in NIGHT52 round 5): with the south ramp cut, the walk
// pilot no longer left the start up it, and backing off wave 1 it pinned itself in the 3 m start passage within 4 m of its spawn (THE ROOM
// CONTRACT's dwell 1.30 -> 2.45 s, limit 1.8; a wave-1 husk moved onto the floor made it 2.45 again: the fight came to the passage). The
// spawn at lz 7 (was 2; lz 5 measured 1.5 / 2.1 = 1.80, on the limit): the passage is behind you, a step back is out of the doorway band
if (!same(room.spawn, { at: [0, 0, Z(2)], yawDeg: 0 })) throw new Error("p1's spawn is not round 10's");
room.spawn = { at: [0, 0, Z(7)], yawDeg: 0 };
room.spawn_why = "NIGHT53 K (n53-k-p1.cjs, round 2): the level's start 5 m into the yard (lz 7, was 2), 4 m short of the drum, the start passage behind you: a fight that comes at you is met in the yard, not pinned against the south wall (the walk pilot's dwell)";

// ---- the room row
room.verb36 =room.verb36.replace("the first pull: the GREAT HOOK's eye from the floor onto the terrace", "the first pull: the GREAT HOOK's eye from the floor onto the terrace (the terrace's only other way up is the back ramp at the far end: NIGHT53 K)");
room.n53_why = "NIGHT53 K (Tools/night/n53-k-p1.cjs; DECIDE52 D-28 / D-30, the second pass): the terrace's south ramp and the hook pad cut (the grapple required in its teach room; the pad stood on the exit's walk line), the JIB RING (the hook into the well panel's ride), the engine deck 2.4 m (I3: out of a jump, so its ring's pull is needed), THE RAMP HEAD (the back ramp's landing: the walkers' way onto the terrace), THE WINCH BASE (the well panel's run-up), the south railing 1 m short of the panel, the hoist engine 1.5 m west";

// ---- the guards
for (let i = 0; i < plan.rooms.length; i++) if (plan.rooms[i].id !== R && !same(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
if (!same(plan.doors, before.doors)) throw new Error("a door changed");
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple"]) {
  const keep = (o) => o.zone !== R && o.room !== R;
  if (!same((before[k] || []).filter(keep), (plan[k] || []).filter(keep))) throw new Error(k + ": a row outside p1 changed");
}
for (const k of ["pits", "props", "lights", "tunnels", "grapple"]) if (!same(before[k], plan[k])) throw new Error(k + " changed (p1's stay)");
for (const k of Object.keys(before)) if (!["rooms", "boxes", "ramps", "pads", "pits", "lights", "props"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
for (const k of ["boxes", "ramps", "pads"]) for (const o of plan[k]) if (o.zone === R && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a p1 row without job / why");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log("n53-k-p1: p1 second pass: the south ramp and the hook pad cut; the ramp head, the jib ring, the winch base; the engine deck 2.4 m; the railing 1 m short, the engine 1.5 m west");

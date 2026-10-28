// NIGHT43 builder E1 (design.n43.iceline43; NIGHT43.md E1, GG G23 "more charger rehearsals before the ICEBREAKER", G19 the FUSION taught;
// round 2: the lead's card, DESIGN-RULES R-3 "at most three roles in a room before level 4"):
// Level 2 as n43-iceline: n40-iceline's plan (Design/levels/n40/iceline.plan.json, written by Tools/night/gen-n40-iceline.cjs, which this file
// does not run or edit) copied whole, with four rooms' ROWS changed on the copy (data only: no geometry moves, so every gate, walk and the
// BRAWL's arena are n40-iceline's), so `--off43` keeps n40-iceline bit for bit:
//   1. the id `n43-iceline` and its _why
//   2. g5 THE COOLDOWN: a third wave, a WARDEN with two DRONES (moved from g7: both roles taught before, g2 the shield, g4 the flyer; the room's
//      roles become fodder, shield, flyer: 3, the escalate beat after the two packs)
//   3. g6 THE ARMOURY (the gun-again room: the KESTREL returns): after its KESTREL lesson one husk comes in at the FAR END OF A DASH LANE (wave 1:
//      afterLesson43 waits for lesson 0; waveNodes43 gives it its own spot) with the lesson "{dash} INTO IT. {fire} THE GAUNTLET." (W1's FUSION:
//      a gauntlet hit at dash speed). The lane: x = LANE_X along the east side, from the door line to the far wall, LANE_LEN m with nothing in it
//      (the KESTREL's plinth stands on the axis, x 0): the husk spawns at its far end and walks down it at you.
//   4. g7 THE MIX, cut to three roles (R-3; n40: tank, spitter, warden, drone, swarmer = 5): introduce (tank + 2 spitters: the far + the big),
//      combine (6 swarmers + a spitter: the pack under fire), escalate (tank + spitter + 4 swarmers: all three at once). The warden and the drones
//      went to g5 (no kind lost from the level: the WARDEN is in g2 / g5, the DRONE in g4 / g5).
//   5. g8 THE REHEARSAL (G23, the two new CHARGER rehearsals): wave 0 a CHARGER alone beside a wall (its own spots, waveNodes43, 2 m off the side
//      walls) with the lesson "{dash} ASIDE: IT HITS THE WALL"; wave 1 a CHARGER with a husk escort (n40's wave 2); wave 2 TWO CHARGERS (escalate:
//      THE BRAWL's double threat); n40's lesson after the first.
// Without rooms43 the afterLesson43 / waveNodes43 fields are not read: g6's second husk then comes with the first (still a gauntlet target)
// and the spots are the room's nodes.
// Lessons are written to `lessons` and `lessons31` (design.n31.unlocks swaps lessons31 in).
// Run from the repo (or worktree) root: bash Tools/night/n43-iceline-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n40/iceline.plan.json");
const OUT = path.join(ROOT, "Design/levels/n43/iceline.plan.json");
/// g6's dash lane: x (east of the plinth's axis) and its far spot's depth (m in from the door side)
const LANE_X = 4.5, LANE_FAR = 15.5, LANE_NEAR = 2;

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n40-iceline") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n43-iceline";
plan._why = "NIGHT43 builder E1 (design.n43.iceline43 swaps n40-iceline for this file while the row is on; GG G23 / G19; DESIGN-RULES R-3): Tools/night/gen-n43-iceline.cjs "
  + "copies n40-iceline's plan whole and changes four rooms' rows only: g5 a WARDEN + 2 DRONES (from g7), g6 a FUSION husk at the far end of a "
  + `${LANE_FAR - LANE_NEAR} m dash lane after the KESTREL lesson ('{dash} INTO IT. {fire} THE GAUNTLET.'), g7 THE MIX cut to 3 roles (tank, spitter, swarmer: introduce, combine, escalate), `
  + "g8 a CHARGER alone beside a wall ('{dash} ASIDE: IT HITS THE WALL'), a CHARGER with an escort, two CHARGERS. n40-iceline's _why: " + plan._why;

const room = (id) => {
  const r = plan.rooms.find((q) => q.id === id);
  if (!r) throw new Error("room missing: " + id);
  return r;
};
const lesson = (prompt, n) => ({ promptPad: prompt, kind: "husksDead", n, prompt });
const addLesson = (r, l, atStart) => {
  for (const key of ["lessons", "lessons31"]) {
    if (!r[key]) continue;
    if (atStart) r[key].unshift(l);
    else r[key].push(l);
  }
};
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const changed = [];

// 2. g5 THE COOLDOWN: + a WARDEN with two DRONES (from g7; both roles known by then: the shield from g2, the flyer from g4)
{
  const r = room("g5");
  if (r.waves.length !== 2) throw new Error("g5's waves changed upstream: " + JSON.stringify(r.waves));
  r.waves.push(["warden", "drone", "drone"]);
  r.iceline43_why = "NIGHT43 E1 round 2 (R-3; one new role per room, no role before its own room): a third wave, a WARDEN with two DRONES, moved from g7 THE MIX: after the two packs, the shield with flyers over it while the blast cools (punch the shield or flank it, punch the orbs back). Both roles were taught before (g2 the shield, g4 the flyer); the room's roles: fodder, shield, flyer (3).";
  changed.push("g5: +wave [warden, drone, drone]");
}

// 3. g6 THE ARMOURY: the FUSION husk at the far end of a dash lane
{
  const r = room("g6");
  if (!same(r.waves, [["husk", "husk"]])) throw new Error("g6's waves changed upstream: " + JSON.stringify(r.waves));
  const near = r.rect[3] - LANE_NEAR, far = r.rect[3] - LANE_FAR;
  if (LANE_X > r.rect[2] - 1) throw new Error("the lane is outside g6");
  r.waves.push(["husk"]);
  r.afterLesson43 = [-1, 0];
  r.afterLesson43_why = "rooms43 (E1): wave 1 (the FUSION husk) waits for lesson 0 (the two KESTREL kills), so the gauntlet lesson comes after the gun's, never on top of it";
  r.waveNodes43 = [null, [[LANE_X, 0, far]]];
  r.waveNodes43_why = `rooms43 (E1 round 2): wave 1's own spot, the far end of the dash lane (x ${LANE_X}, ${LANE_FAR} m in; the lane from the door line ${LANE_NEAR} m in is ${LANE_FAR - LANE_NEAR} m long and empty: the plinth stands on the axis): the husk walks down the lane at you, a dash (6 m at 24 m/s) plus a swing closes it`;
  addLesson(r, lesson("{dash} INTO IT. {fire} THE GAUNTLET.", 3), false);
  r.iceline43_why = "NIGHT43 E1 (G19): the gun-again room teaches the dash into a gauntlet hit (W1's FUSION) on one husk that comes in down a straight lane after the KESTREL's two; lesson 1 counts 3 kills from the stage's start (husksDead counts from the stage's start: the KESTREL's 2 + this one)";
  changed.push(`g6: +wave [husk] (afterLesson43 [-1,0], waveNodes43 [${LANE_X},0,${far}]), +lesson '{dash} INTO IT. {fire} THE GAUNTLET.' (husksDead 3)`);
}

// 4. g7 THE MIX: three roles (R-3)
{
  const r = room("g7");
  if (!same(r.waves, [["tank", "spitter", "spitter"], ["warden", "drone", "drone"], ["tank", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"]]))
    throw new Error("g7's waves changed upstream: " + JSON.stringify(r.waves));
  r.waves = [
    ["tank", "spitter", "spitter"],
    ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "spitter"],
    ["tank", "spitter", "swarmer", "swarmer", "swarmer", "swarmer"],
  ];
  r.iceline43_why = "NIGHT43 E1 round 2 (DESIGN-RULES R-3, at most three roles before level 4; n40 had five: tank, spitter, warden, drone, swarmer): introduce (tank + 2 spitters: KESTREL far, glove close), combine (6 swarmers + a spitter: the pack under fire from the perch), escalate (tank + spitter + 4 swarmers: the three at once). The warden and the drones moved to g5; n40's lesson ('{quickSwap}: KESTREL FAR, GLOVE CLOSE', 3 kills) kept.";
  changed.push("g7: waves [tank, spitter x2] [swarmer x6, spitter] [tank, spitter, swarmer x4] (roles heavy, ranged, fodder)");
}

// 5. g8 THE REHEARSAL: a CHARGER beside a wall, a CHARGER with an escort, two CHARGERS
{
  const r = room("g8");
  if (!same(r.waves, [["charger"], ["charger", "husk", "husk"]])) throw new Error("g8's waves changed upstream: " + JSON.stringify(r.waves));
  const [x0, , x1] = r.rect;
  const z = r.rect[1] + 14; // 16 m in from the door side (the pillars' far line is at 18 m): the charge runs along the wall's line
  r.waves = [["charger"], ["charger", "husk", "husk"], ["charger", "charger"]];
  r.waveNodes43 = [[[x0 + 2, 0, z], [x1 - 2, 0, z]], null, null];
  r.waveNodes43_why = "rooms43 (E1): wave 0's CHARGER spawns 2 m off a side wall (either side), so its first charge comes along a wall: step aside and it hits the wall";
  addLesson(r, lesson("{dash} ASIDE: IT HITS THE WALL", 1), true);
  for (const key of ["lessons", "lessons31"]) if (r[key] && r[key][1] && r[key][1].kind === "husksDead") r[key][1].n += 1; // husksDead counts from the stage's start: n40's lesson now needs the wall charger's kill + its own
  r.iceline43_why = "NIGHT43 E1 (G23): the CHARGER rehearsed three ways before THE BRAWL: alone beside a wall (the lesson names the stun: a charge that ends in a wall stuns it 1.5 s with its back open, Charger.cs), with a husk escort (n40's wave), two at once (escalate). 4 chargers in the room (n40: 2).";
  changed.push(`g8: waves [charger (spots ${x0 + 2}/${x1 - 2},0,${z})] [charger, husk x2] [charger x2], +lesson 0 '{dash} ASIDE: IT HITS THE WALL'`);
}

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms;", changed.join("; "));

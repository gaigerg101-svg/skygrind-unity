// NIGHT39 builder K4 (NIGHT39.md card K4; Design/levels/n38/kiln.beats.md, committed before this file): LEVEL 3 THE KILN rebuilt a second time,
// replacing NIGHT38 K3's n38-kiln (gates RED 0/4 on builder E's checker: the PULSE blast and the dash jump beat every low ledge and long void).
// Written around the THUNDERCLAP and its CLIMB: jump, shoot straight down, double jump at the apex, shoot down again (envelopes38
// `jump+double+boost x2`: 13.36 m; a third shot 16.26 m). EVERY GATE IS A CLIMB: a noride cliff wall to wall 12 m high (round 2), over every
// non-boost apex (the blast + a wall jump off the face: 11.31 m) and under the climb. No void the boost crosses is longer than the blast or the dash jump crosses, so no
// horizontal gate is provable on this level (the beat sheet). Gates live only in FLIGHT rooms (links, no enemies); a fight room's raised decks
// all have a walkers' ramp, so there the boost is the fast way up, never a gate.
// It requires ONLY ./n36-lib.cjs. Every solid carries a job and a why (Tools/night/n36-jobs.cjs). The one kept room is k5 THE RING (THE KILN
// WYRM's trigger room): its row, lights and dressing copied from Design/levels/n35/kiln.plan.json and moved to the level's end; its crumble31
// boxes re-pointed at the worm's new arena (Design/maps/wyrm39-arena.json, Tools/night/gen-n39-wyrm-arena.cjs) when that file exists.
// Rooms: kb1 THE CHARGE, kb1b THE FLUE (G1), kb2 POUR HALL, kb2b THE STACK (G2a, G2b), kb3 CASTING FLOOR, kb3b THE SPOUT (G3), kb4 REST, k5 THE RING.
// Run: bash Tools/night/n38k-regen.sh (this generator, the n36-jobs checker, levelgen, the copies); gates: n38 gates Design/levels/n38/kiln.plan.json
const fs = require("fs");
const path = require("path");
const { Level, r3 } = require("./n36-lib.cjs");
const ROOT = path.join(__dirname, "../..");

// ---------------------------------------------------------------- the gate numbers (read, not typed: their sources)
// Builder E's envelope table (Design/levels/envelopes38.json, `n38 envelopes --write`: the real sim, the PULSE blast included).
const ENV = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/envelopes38.json"), "utf8")).ground;
const NOBOOST = Object.entries(ENV).filter(([k]) => !k.includes("boost"));
const noBoostApex = Math.max(...NOBOOST.map(([, v]) => v.apex)); // 7.75: jump + the PULSE blast at pitch 70
const climb = ENV["jump+double+boost x2"]; // 13.36: the THUNDERCLAP CLIMB (shot down, double at the apex, shot down)
const climb3 = ENV["jump+double+boost x3"]; // 16.26: the climb with a third shot
const CORNER = 3.5; // the wall-jump corner cheese on the real Mover (NIGHT38 `n38kiln corner`: noride walls still give wall jumps)
// NIGHT39 round 2: builder E's checker (b13c7802) flies the PULSE blast + a wall jump off the noride cliff face: the feet reach 11.31 m and it
// LANDED on every 9.5 m cliff (n38-kiln RED 0/4). A face that leans past the wall threshold (a ramp wedge in front of the cliff) is worse: the
// blast and the dash jump ride the slope (K4's sweep, 3.5-7 m runs: RED 0/5, 240-370 cheats each). The cliff goes UP instead (sweep: 12 m PASS
// +34 %, 12.5 m +29 %, 13 m +24 %; the best other try at 12 m stops -101 % short at the top's height).
const BLASTWALL = 11.31; // builder E's checker, b13c7802 (the blast + wall jump's highest feet); NOT in envelopes38.json yet
const GATE = {
  // over the blast + wall jump's 11.31 m (and every envelope apex + 10 %), under the climb with three shots / 1.1 (14.78): 12 m measured +34 %
  H: 12,
  depth: 4.5, // a cliff top's depth (the landing)
  gapFrom: 1.5, // the take-off point stands this far short of the cliff's face (the capsule clear of it on the jump)
  src: "Design/levels/envelopes38.json (builder E, the real sim: every non-boost apex, the climb `jump+double+boost x2` / x3) + E's checker b13c7802 (blast + wall jump 11.31 m) + K4's round-2 sweep",
};
if (!(GATE.H > noBoostApex * 1.1 && GATE.H > CORNER * 1.1 && GATE.H > BLASTWALL && GATE.H < climb3.apex / 1.1 && GATE.H < climb.apex))
  throw new Error(`gate height ${GATE.H} outside (${noBoostApex} x 1.1, blast + wall jump ${BLASTWALL}; ${climb3.apex} / 1.1, the two-shot climb ${climb.apex}): re-read envelopes38.json`);
const N35 = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n35/kiln.plan.json"), "utf8"));
const ARENA39 = "Design/maps/wyrm39-arena.json";
const ARENA39_FILE = path.join(ROOT, ARENA39);

const OWNS = ["jump", "double", "dash", "slide", "boost", "ride", "walljump"];
const L = new Level("n38-kiln", "LEVEL 3 THE KILN",
  "NIGHT39 builder K4 (design.n38.kiln38 swaps n35-kiln for this file): Tools/night/gen-n38-kiln.cjs from the beat sheet Design/levels/n38/kiln.beats.md. "
  + "The level of the THUNDERCLAP and its CLIMB (jump, shoot down, double jump, shoot down): 3.1 THE CHARGE (the gun up close, one boost onto the rim), "
  + "3.1b THE FLUE (gate G1: the climb taught), 3.2 POUR HALL (an open roofed arena: boost THROUGH the pack onto the crucible), 3.2b THE STACK (gates "
  + "G2a / G2b: two climbs), 3.3 CASTING FLOOR (ride over the pour, climb to the ladle, slug, parry, launch), 3.3b THE SPOUT (gate G3: a climb out of a "
  + "run over a pit), 3.4 REST, 3.5 THE RING (THE KILN WYRM, its AI unchanged, in its new arena). Every gate a noride cliff " + GATE.H + " m high: "
  + "over the PULSE blast's " + noBoostApex + " m + 10 %, under the climb's " + climb.apex + " m. Numbers from " + GATE.src + ".",
  {
    // the verbs the player owns on level 3 (builder E's checker tries every one against each gate; it adds the PULSE blast itself: the gauntlet
    // carries from level 2's j1): no grapple (design.n35.grapple35.fromLevel 4)
    owns38: OWNS,
    rank: Object.assign({}, N35.rank, { _why: "Unity-only (builder K4, NIGHT39): n35-kiln's S/A/B/C cuts kept (NOT calibrated on this level's walk: the walk's level median is in Research/2026-10-14-wave39/K4.md)." }),
  });

const ARENA = (r) => { r.row.arena36 = true; };

/// a light on a face pointing +z (south in web coordinates: the face of a cliff the player runs at)
function southStrip(r, lz, y, w, color, range, intensity) {
  r.light("wall", [r.X(0), r3(y), r3(r.Z(lz) + 0.05)], [0, 0, 1], [w, 0.2, 0.1], color, range, intensity);
}

/// noride liners along both side walls from lz0 to lz1 (from y0 to the roof): a ride along the room wall would jump off beside the cliff; the
/// liners take the ride away and show it (the noride look). Wall jumps off them stay the Mover's (the corner cheese: 3.50 m).
function liners(r, lz0, lz1, y0, why) {
  const a = r.box("hull", -r.w / 2, lz0, -r.w / 2 + 0.3, lz1, y0, r.h, "line", `noride liner, west wall (${why})`);
  const b = r.box("hull", r.w / 2 - 0.3, lz0, r.w / 2, lz1, y0, r.h, "line", `noride liner, east wall (${why})`);
  a.noride = b.noride = true;
}

/// one CLIMB GATE: a cliff wall to wall from lz `face` to `face + depth`, from `base` (the take-off floor's height) up to `top`, faces noride,
/// tagged for builder E's checker; the take-off point `GATE.gapFrom` m short of the face on the axis. Returns the cliff box.
function cliff(r, o) {
  const W = r.w / 2, face = o.face, back = face + (o.depth || GATE.depth), base = o.base || 0, top = o.top, fromLz = face - (o.gapFrom || GATE.gapFrom);
  const c = r.box("deck", -W, face, W, back, o.y0 !== undefined ? o.y0 : base - 1, top, "line", `${o.id}'s CLIFF (top ${top} m, ${r3(top - base)} m over the take-off, wall to wall, faces noride): ${o.why}`);
  c.noride = true;
  c.gate = { id: o.id, verb: "boost", from: r.pt(0, base, fromLz), to: r.pt(0, top, (face + back) / 2), runUp: o.runUp || 6, why: o.why };
  r.row.routeVia = (r.row.routeVia || []).concat([[r.pt(0, base, fromLz), r.pt(0, top, face + 1.5)]]); // the climb only the boost makes
  // light: amber over the take-off, white down the cliff's face, a cyan strip on its lip (where you land), a cyan pool on its top
  // (one accent per room, LevelValidate29: a second climb in the room lights its take-off white)
  r.light("ceiling", [r.X(0), r3(r.h - 0.05), r.Z(fromLz - 2)], [0, -1, 0], [3, 2, 0.1], o.accent === false ? "white" : "amber", Math.min(14, r.h), 1.0);
  southStrip(r, face, top - 0.15, r.w - 1, "cyan", 9, 1.5);
  southStrip(r, face, base + (top - base) / 2, 3, "white", 8, 0.8);
  r.pool(0, (face + back) / 2, 2, "cyan", 6, 1.2, undefined, top);
  r.walk([[0, base, fromLz - 4], [0, base, fromLz]]);
  r.walk([[0, top, face + 0.8], [0, top, back - 0.6]]);
  return c;
}

// every room's par = the walk's median clear (n38kiln walk, 10 seeds; LEVEL-FORMAT: the bot must clear within 4 x par)
// ---------------------------------------------------------------- 3.1 THE CHARGE (teach: the THUNDERCLAP up close; ONE boost onto the rim)
{
  const r = L.room({ id: "kb1", name: "3.1 THE CHARGE", job: "teach", verb: "the THUNDERCLAP (grab it, fire it up close); one boost onto the rim", w: 20, l: 26, h: 10, par: 20, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  // the THUNDERCLAP: design.n31.unlocks' n38-kiln row gives it on entry to this room (its `room` kb1); the room award at the clear is the
  // fallback. The LOADOUT id "shotgun" (G0 b, d52c5042: a room award goes to the loadout as written; a display name made the ghost weapon)
  r.row.award = "shotgun";
  r.solid("plinth", 0, 5, 1.2, 0.6, 1.2, "reward", "the THUNDERCLAP's plinth, 3 m from the door on the axis: the gun is the first thing in the room");
  r.pickup("weaponPickup", 0, 5, 1.0, "the THUNDERCLAP, in view from the door: grab it (the room's unlock gives it on entry anyway)");
  r.propsOut[r.propsOut.length - 1].weapon = "shotgun";
  [[-3, 10], [0, 11], [3, 10]].forEach(([lx, lz]) => r.standIn(lx, 0, lz, "a stand-in in a tight group 5 m past the plinth: one THUNDERCLAP blast up close kills the group"));
  r.nodeAt(-4, 16); r.nodeAt(5, 16); // the first husks: past the stand-ins (14 m from the door: spawn31's first-view rule), walking at you
  r.row.waves = [["husk", "husk"]];
  // THE RIM: a 5 m deck across the west half: one boost (jump, shoot down: 6.76 m) lands on it; its stair (the ramp at the west wall) is the
  // walkers' route and the slow way up (n36-jobs rule 3), so it is a lesson, not a gate
  r.box("deck", -10, 18, -1, 22, 0, 5, "line", "THE RIM (5 m, the west half): the first boost's landing: jump, shoot down (one shot lifts 6.76 m, envelopes38); a jump + double (2.87 m) falls short");
  r.ramp(-8.5, 5, 18, 3, 5, "climb", "the rim's stair along the west wall (walkers: 5 m over 13 m): the slow way up");
  r.pickup("healthPickup", -5, 20, 5.9, "the rim pays health: the first boost's reward");
  r.lesson({ kind: "reached", rect: [-10, 3, 10, 7], minY: -0.2, prompt: "GRAB THE THUNDERCLAP" });
  r.lesson({ kind: "reached", rect: [-1, 13, 10, 18], minY: -0.2, prompt: "JUMP, SHOOT {fire} DOWN: BOOST" });
  r.teach("l3.charge", -8, 7, 8, 12);
  r.ceil(0, 5, 3, 3, "white", 9, 1.1); r.ceil(0, 11, 4, 2, "white", 9, 1.0); r.ceil(-5, 20, 4, 2, "white", 9, 1.0);
  r.pool(-5.5, 20, 2, "cyan", 7, 1.3, undefined, 5);
  r.pool(0, 24, 2.5, "cyan", 7, 1.6);
  r.walkSE([[3, 0, 8], [4, 0, 17]]);
  r.walk([[-8.5, 0, 3.5], [-8.5, 5, 19], [-5, 5, 20]]);
  r.frames([4, 0, 3], [-5, 4, 20], [4, 0, 17], [0, 1, 26]); // mid: from the door past the stand-ins to the rim's lip; exit: past the husks' spot to the door
}

// ---------------------------------------------------------------- 3.1b THE FLUE (teach, flight: gate G1, the climb taught)
{
  const r = L.room({ id: "kb1b", name: "3.1b THE FLUE", kind: "link", job: "teach", verb: "the THUNDERCLAP CLIMB: jump, shoot down, double jump, shoot down", w: 16, l: 26, h: 20, par: 4, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  const face = 11.5, back = face + GATE.depth;
  cliff(r, { id: "G1", face, top: GATE.H, why: `the CLIMB taught: one shot lifts 6.76 m, the climb (shot, double, shot) ${climb.apex} m; the PULSE blast tops at ${noBoostApex} m` });
  liners(r, 0, r.l, -1, "G1: the take-off, the cliff and the landing");
  r.lesson({ kind: "reached", rect: [-6, back + 2, 6, r.l], minY: -0.2, prompt: "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN." });
  r.ceil(0, back + 4, 3, 2, "white", 12, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, back + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, face + 0.5], [0, GATE.H, back - 0.5], [0, 1, r.l]); // mid: from the door up the cliff to its cyan lip; exit: from the top to the door
}

// ---------------------------------------------------------------- 3.2 POUR HALL (practise, fight: the boost THROUGH the pack onto the crucible)
{
  const r = L.room({ id: "kb2", name: "3.2 POUR HALL", job: "practise", verb: "the boost THROUGH the pack: jump into them, shoot down, land on the crucible", w: 36, l: 44, h: 16, par: 45, cx: L.prev.outX, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -6, 16, 6, 26, 0, 6, "line", "THE CRUCIBLE (6 m) in the middle: over every non-boost apex but the blast; the boost from the pack at its foot lands on it (one shot 6.76 m)");
  r.ramp(3, 39, 26, 4, 6, "climb", "the crucible's walkers' ramp up its north face from the far floor (6 m over 13 m): the heavy's way up and down");
  r.box("deck", -18, 8, -14, 34, 0, 3, "line", "the WEST GALLERY (3 m) over the west pour: the spitters' stand");
  r.box("deck", 14, 8, 18, 34, 0, 3, "line", "the EAST GALLERY (3 m) over the east pour: the spitters' stand; its wall is the room's cyan ride");
  r.ramp(-16, 1, 8, 3, 3, "climb", "the west gallery's ramp (walkers: 3 m over 7 m)");
  r.ramp(16, 1, 8, 3, 3, "climb", "the east gallery's ramp");
  r.lava(-13, 10, -9, 32, 0.8, "the WEST POUR (4 x 22 m): the walkers' line down the gallery runs along its rims; a blast or a punch launches them in");
  r.lava(9, 10, 13, 32, 0.8, "the EAST POUR: as the west, under the ride wall");
  r.panel("W", 17.4, 10, 32, 3, 10, "the EAST RIDE (cyan): ride the room's east wall north from the gallery, over the pour (VERBS rows 9 / 11 on this map)");
  // wave 1 in the TROUGH at the crucible's south foot, ahead on the line (13 m or more from the door: spawn31): the boost goes through them
  r.nodeAt(-4, 14.5); r.nodeAt(0, 15); r.nodeAt(4, 14.5); r.nodeAt(-2, 15.5); r.nodeAt(2, 15.5);
  r.nodeAt(-6, 33); r.nodeAt(6, 32); r.nodeAt(-1, 40); r.nodeAt(-7.5, 22); r.nodeAt(7.5, 22);
  r.highNodeAt(-16, 14, 3); r.highNodeAt(16, 14, 3); r.highNodeAt(-16, 28, 3); r.highNodeAt(16, 28, 3); r.highNodeAt(0, 22, 6);
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "husk", "husk"], ["heavy", "swarmer", "swarmer", "swarmer"]];
  r.row.rims36 = { rects: [r.rect(-14, 8, -13, 34), r.rect(-9, 8, -8, 34), r.rect(8, 8, 9, 34), r.rect(13, 8, 14, 34)], why: "the floor strips beside the pours: the walkers' lines down from the galleries and along the floor" };
  r.lesson({ kind: "reached", rect: [-6, 8, 6, 12], minY: -0.2, prompt: "JUMP IN. SHOOT {fire} DOWN. FLY." });
  r.pickup("healthPickup", -3, 20, 6.9, "the crucible pays health: the boost through the pack lands on it");
  r.ceil(0, 8, 4, 4, "white", 13, 1.1); r.ceil(0, 21, 6, 4, "white", 13, 1.1); r.ceil(0, 34, 4, 4, "white", 13, 1.0);
  r.pool(0, 21, 3, "orange", 10, 1.3, { danger: true }, 6); // the room's one accent: the crucible's molten glow (the pours glow on their own)
  r.pool(-16, 21, 2.5, "white", 7, 0.9, undefined, 3); r.pool(16, 21, 2.5, "white", 7, 0.9, undefined, 3);
  r.pool(0, 42, 2.5, "cyan", 8, 1.7);
  r.walkSE([[7.5, 0, 10], [7.5, 0, 40.5]]);
  r.walk([[5, 0, 41], [3, 6, 27], [0, 6, 22]]);
  r.walk([[-16, 0, 0.3], [-16, 3, 10], [-16, 3, 30]]);
  r.frames([3, 0, 4], [0, 6, 20], [0, 6, 25], [0, 1, 43]); // mid: the trough and the crucible from the entry; exit: from the crucible's top to the door
}

// ---------------------------------------------------------------- 3.2b THE STACK (test, flight: gates G2a and G2b, two climbs)
{
  const r = L.room({ id: "kb2b", name: "3.2b THE STACK", kind: "link", job: "test", verb: "two climbs: 12 m, then 12 m more", w: 16, l: 36, h: 32, par: 8, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  const f1 = 11.5, t1End = 23.5, f2 = t1End, b2 = f2 + 5;
  cliff(r, { id: "G2a", face: f1, depth: t1End - f1, top: GATE.H, why: "the first climb onto TIER 1 (12 m deep: a 6 m run-up on it for the second)" });
  cliff(r, { id: "G2b", accent: false, face: f2, depth: b2 - f2, base: GATE.H, y0: -1, top: 2 * GATE.H, why: "the second climb, from TIER 1 onto TIER 2: a miss drops you on tier 1 (or the floor: G2a again), never a pit" });
  liners(r, 0, r.l, -1, "G2a / G2b: the take-offs, the cliffs and the landing");
  r.lesson({ kind: "reached", rect: [-6, b2 + 2, 6, r.l], minY: -0.2, prompt: "CLIMB." });
  r.ceil(0, b2 + 4, 3, 2, "white", 14, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, b2 + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, f1 + 0.5], [0, 2 * GATE.H, b2 - 0.5], [0, 1, r.l]);
}

// ---------------------------------------------------------------- 3.3 CASTING FLOOR (combine: ride over the pour, climb to the ladle, slug, parry, launch)
{
  const r = L.room({ id: "kb3", name: "3.3 CASTING FLOOR", job: "combine", verb: "ride over the pour, climb to the ladle, slug the heavy, parry the spitters, launch into the pour", w: 40, l: 46, h: 18, par: 60, cx: L.prev.outX, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.lava(-8, 18, 8, 28, 0.8, "the POUR (16 x 10 m) in the middle of the floor: the walkers' lines from the gantry and the ladle pass along its rims; punch or blast them in");
  r.panel("W", -2.4, 18.2, 27.8, -0.8, 6, "the WEST POUR RIDE (cyan, standing in the pour): ride it north across the molten metal");
  r.panel("E", 2.4, 18.2, 27.8, -0.8, 6, "the EAST POUR RIDE (cyan, standing in the pour): the other side's ride; jump between the two");
  r.box("deck", 12, 12, 20, 28, 0, 8, "line", "THE LADLE (8 m, east): the high ground; one climb (shot, double, shot) or its long ramp; the heavy's perch in wave 3");
  r.ramp(16, 44, 28, 4, 8, "climb", "the ladle's walkers' ramp from the north floor (8 m over 16 m)");
  r.box("deck", -20, 12, -14, 30, 0, 4, "line", "the WEST GANTRY (4 m): the spitters' stand: parry their globs back from the floor or the ladle");
  r.ramp(-17, 2, 12, 3, 4, "climb", "the west gantry's ramp (walkers: 4 m over 10 m)");
  r.nodeAt(-4, 14.5); r.nodeAt(4, 14.5); r.nodeAt(0, 15.5); r.nodeAt(-8, 14.5); r.nodeAt(8, 14.5); // wave 1 ahead, 12 m or more from the door (spawn31)
  r.nodeAt(-10, 23); r.nodeAt(10, 31); r.nodeAt(-5, 31); r.nodeAt(5, 32);
  r.highNodeAt(-17, 18, 4); r.highNodeAt(-17, 26, 4); r.highNodeAt(16, 20, 8); r.highNodeAt(16, 25, 8);
  r.row.waves = [["husk", "husk", "husk", "husk", "heavy"], ["spitter", "spitter", "husk", "husk", "husk"], ["heavy", "spitter", "spitter", "swarmer", "swarmer"]];
  r.row.rims36 = { rects: [r.rect(-10, 16, -8, 30), r.rect(8, 16, 10, 30), r.rect(-8, 28, 8, 30)], why: "the floor around the pour: the walkers' lines from the doors, the gantry and the ladle's ramp" };
  r.pickup("healthPickup", 16, 20, 8.9, "the ladle pays health: climb to it mid-fight");
  r.ceil(0, 8, 4, 4, "white", 14, 1.1); r.ceil(0, 23, 6, 4, "white", 15, 1.1); r.ceil(0, 38, 4, 4, "white", 14, 1.0);
  r.pool(0, 23, 4, "orange", 10, 1.4, { danger: true }, -0.8);
  r.pool(16, 20, 2.5, "white", 8, 0.9, undefined, 8); r.pool(-17, 21, 2.5, "white", 8, 0.9, undefined, 4);
  r.pool(0, 44, 2.5, "cyan", 8, 1.7);
  r.walkSE([[-11, 0, 8], [-11, 0, 34], [0, 0, 40]]);
  r.walk([[-17, 0, 1], [-17, 4, 14], [-17, 4, 28]]);
  r.walk([[16, 0, 45], [16, 8, 27], [16, 8, 14]]);
  r.frames([-6, 0, 3], [0, 3, 23], [16, 8, 26], [0, 1, 45]); // mid: from the SW corner across the pour's rides to the ladle; exit: from the ladle to the door
}

// ---------------------------------------------------------------- 3.3b THE SPOUT (test, flight: gate G3, a climb out of a run over a pit)
{
  const r = L.room({ id: "kb3b", name: "3.3b THE SPOUT", kind: "link", job: "test", verb: "the climb carried forward over a 4 m pit", w: 16, l: 30, h: 20, par: 5, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  const W = r.w / 2, lip = 9, face = 13, PIT = 3.9;
  // the pit runs 3 m back under the take-off floor (K3's NIGHT38 shape, which levelgen's pit recovery passes): the take-off floor over that strip is
  // a deck, except the west notch that holds the recovery stair (pit floor -> 1.3 m -> 1.3 m -> the floor south of the notch)
  r.pit(-W, lip - 3, W, face, -PIT, "G3's PIT: 4 m wall to wall in the open before the cliff, a dry pit 3.9 m deep; a miss costs the stair's climb in the notch, never health");
  r.box("deck", -W + 2.8, lip - 3, W, lip, -PIT - 1, 0, "line", "G3's take-off floor over the pit's strip: the run-up to the lip");
  r.box("deck", -W + 0.3, lip - 1.5, -W + 2.8, lip, -PIT - 1, -PIT + 1.3, "climb", "G3's recovery stair, step 1 (1.3 m over the pit floor, the west notch)");
  r.box("deck", -W + 0.3, lip - 3, -W + 2.8, lip - 1.5, -PIT - 1, -PIT + 2.6, "climb", "G3's recovery stair, step 2 (1.3 m under the take-off floor)");
  cliff(r, { id: "G3", face, top: GATE.H, gapFrom: face - lip + 0.5, y0: -PIT - 1, why: `the climb carried forward: take off at the pit's lip, ${face - lip} m short of the face` });
  liners(r, 0, r.l, -PIT - 1, "G3: the take-off, the pit, the cliff and the landing");
  r.lesson({ kind: "reached", rect: [-6, face + GATE.depth + 2, 6, r.l], minY: -0.2, prompt: "CLIMB IT ON THE RUN." });
  r.ceil(0, face + GATE.depth + 4, 3, 2, "white", 12, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, face + GATE.depth + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, face + 0.5], [0, GATE.H, face + GATE.depth - 0.5], [0, 1, r.l]);
}

// ---------------------------------------------------------------- 3.4 REST (a breath; the checkpoint before the Wyrm)
{
  const r = L.room({ id: "kb4", name: "3.4 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: 3, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 5, 0.9, "the last rest before THE KILN WYRM: health for the fight");
  r.ceil(0, 5, 3, 3, "white", 8, 1.0);
  r.pool(0, 8.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
}

// ---------------------------------------------------------------- 3.5 THE RING (k5: THE KILN WYRM's room, copied from n35-kiln)
{
  const src = N35.rooms.find((q) => q.id === "k5");
  if (!src) throw new Error("n35-kiln has no k5");
  const [ox0, oz0, ox1, oz1] = src.rect;
  const w = ox1 - ox0, l = oz1 - oz0;
  const prev = L.prev;
  const ocx = (ox0 + ox1) / 2;
  const r = L.room({ id: "k5", name: src.name, kind: "boss", w, l, h: src.height, cx: prev.outX, outX: prev.outX, par: src.par, spawnLz: oz1 - src.spawn.at[2], exitFromN: src.exit[2] - oz0 });
  const dx = r.X(0) - ocx, dz = r.z1 - oz1; // the move: every copied coordinate shifts by (dx, dz)
  const mv = (p) => [r3(p[0] + dx), p[1], r3(p[2] + dz)];
  r.row.boss = src.boss;
  r.row.roof = src.roof;
  const cr = JSON.parse(JSON.stringify(src.crumble31));
  cr.draw.center = [r3(cr.draw.center[0] + dx), r3(cr.draw.center[1] + dz)];
  cr.draw.boxes.forEach((b) => { b.rect = [r3(b.rect[0] + dx), r3(b.rect[1] + dz), r3(b.rect[2] + dx), r3(b.rect[3] + dz)]; });
  // the worm's NEW arena (gen-n39-wyrm-arena.cjs writes its crumble picks into the file): the crumble's box indices are that file's
  if (fs.existsSync(ARENA39_FILE)) {
    const a = JSON.parse(fs.readFileSync(ARENA39_FILE, "utf8"));
    // the worm loads this file instead of design.n26.bosses.wyrm26.arena (SurvivalSim.Kiln39.cs; the lead's call sites: Tools/night/n39k-wyrm-hook.cjs)
    r.row.wyrmArena = ARENA39;
    r.row.wyrmArena_why = "NIGHT39 K4 (GG: 'a new arena, and keep the worm AI the same'): the worm's room for this level: wyrm26-arena.json's shell (half 30, height 30) with a new inside (Tools/night/gen-n39-wyrm-arena.cjs); the paths file and the AI unchanged";
    if (a.crumble39) {
      cr.arena = ARENA39;
      cr.boxes = a.crumble39.boxes;
      cr.keep = a.crumble39.keep;
      cr._why = cr._why + " NIGHT39 K4: re-pointed at " + ARENA39 + " (" + a.crumble39._why + ").";
      cr.draw.boxes = a.crumble39.draw.map((b) => Object.assign({}, b, { rect: [r3(b.rect[0] + cr.draw.center[0]), r3(b.rect[1] + cr.draw.center[1]), r3(b.rect[2] + cr.draw.center[0]), r3(b.rect[3] + cr.draw.center[1])] }));
    }
  }
  r.row.crumble31 = cr;
  r.row.proofs = src.proofs.map((p) => Object.assign({}, p, { points: p.points.map(mv) }));
  r.row.spawn = { at: mv(src.spawn.at), yawDeg: src.spawn.yawDeg };
  r.row.exit = mv(src.exit);
  r.row.copied38 = "n35-kiln k5 (THE KILN WYRM's room) copied and moved by (" + r3(dx) + ", " + r3(dz) + ") to the level's end (GG: the worm's AI the same)";
  for (const li of N35.lights.filter((q) => q.room === "k5")) r.lightsOut.push(Object.assign({}, li, { at: mv(li.at) }));
  for (const pr of N35.props.filter((q) => q.room === "k5")) {
    if (pr.collider) throw new Error("k5 has a solid prop: tag it");
    r.propsOut.push(Object.assign({}, pr, { at: mv(pr.at) }));
  }
  for (const k of ["boxes", "ramps", "pits", "tunnels", "pads", "grapple"]) if ((N35[k] || []).some((q) => q.zone === "k5")) throw new Error("k5 has " + k + ": copy them");
  r.frames([0, 0, 4], [0, 3, 13], [0, 0, 22], [0, 1, 26]);
}

L.write(process.env.K4_OUT || "Design/levels/n38/kiln.plan.json");

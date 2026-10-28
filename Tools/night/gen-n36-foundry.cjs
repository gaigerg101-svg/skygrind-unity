// NIGHT36 builder L (NIGHT36.md L1; PLAN.md section 3; REVIEW.md root causes 7-8): LEVEL 1 THE FOUNDRY written from scratch, whole, from its beat
// sheet (Design/levels/n36/foundry.beats.md, committed before this file). It requires ONLY ./n36-lib.cjs: no n29-n35 generator, lib or room.
// Every solid carries a job (line / cover / climb / hazard / marker / reward) and a why; Tools/night/n36-jobs.cjs checks the plan.
// The last room is THE LANTERN's hall to builder B's spec (Design/levels/n36/lantern-hall.spec.md): B's world layer builds the 16-panel ring at
// load from the room row's lantern36.centre; this file builds the approach (the 4 x 4.5 x 8 m hall = the door) and the row.
// Every fight room carries arena36: true: Game/SurvivalSim.Level36.cs runs ULTRAKILL's arena flow there only (older levels untouched).
// Run: node Tools/night/gen-n36-foundry.cjs  (then bash Tools/night/n36-regen.sh: levelgen + the copies + the checker)
const { Level, MOVE, r3 } = require("./n36-lib.cjs");

// NIGHT37 builder W (NIGHT37.md W1): `--n37` writes n37-foundry instead: rooms a1-a6 unchanged, then THE WICK's two rehearsal rooms (a7 THE FLUE: a short
// climb of 3 ride panels, each 2.2 m higher, where the leap's look pitch matters; a8 PIERCE: a charged pierce from the wall through a line of stand-ins)
// and a9 THE STACK (THE WICK's 40 m furnace shaft, built at load by Core/Enemies/Wick37Stack.cs from the room row's wick37.centre) replacing a7 / a8.
// Run: node Tools/night/gen-n36-foundry.cjs --n37   (then bash Tools/night/n37w-regen.sh)
// NIGHT38 builder F (NIGHT38-BRIEF card F, DECIDE38 D-4): `--n38` writes n38-foundry: n37-foundry's rooms exactly as built (a1-a5, a6 REST, W's a7-a9 and
// THE STACK, untouched), with two NEW rooms after the CRUCIBLE (a5) and before the rest: b1 THE YARD (an open roofed arena: KESTREL + ride walls, enemies
// switched on ahead on the line) and b2 THE GANTRY (a flight room: two gates no jump crosses, each spanned by a chain of four ride walls). The display
// names after b2 are renumbered (1.8 REST .. 1.11 THE STACK); the ids stay. Beat sheet: Design/levels/n38/foundry.beats.md.
// Run: node Tools/night/gen-n36-foundry.cjs --n38   (then bash Tools/night/n38f-regen.sh foundry)
const N38 = process.argv.includes("--n38");
const N37 = N38 || process.argv.includes("--n37");
const OUT = N38 ? "Design/levels/n38/n38-foundry.plan.json" : N37 ? "Design/levels/n36/n37-foundry.plan.json" : "Design/levels/n36/foundry.plan.json";
const L = new Level(N38 ? "n38-foundry" : N37 ? "n37-foundry" : "n36-foundry", "LEVEL 1 THE FOUNDRY",
  N38 ? "NIGHT38 builder F (design.n38.foundry38 swaps n37-foundry for this file): Tools/night/gen-n36-foundry.cjs --n38 from the beat sheets Design/levels/n36/foundry.beats.md "
    + "(a1-a6, kept) and Design/levels/n38/foundry.beats.md (b1 THE YARD, b2 THE GANTRY); THE WICK's rooms a7-a9 are NIGHT37 W's. Spacing from " + MOVE.src + "."
  : "NIGHT36 builder L (design.n36.level36 swaps n35-foundry for this file): Tools/night/gen-n36-foundry.cjs from the beat sheet Design/levels/n36/foundry.beats.md. "
  + "Six rooms and THE LANTERN's hall: a calm intake (move, look, jump, slide, dash), the KESTREL (tap, charge), the wall ride (over a floor, then over lava), the PARRY "
  + "(one spitter alone), the CRUCIBLE (ride + parry + launch), a rest, the walk-in hall. Spacing from " + MOVE.src + ".",
  {
    rank: N38 ? {
      time: [287, 440, 660, 990], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250],
      _why: "Unity-only (builder F, NIGHT38): S/A/B/C cuts: NIGHT36's time cuts plus the two new rooms' beat-sheet times (about 55 s: THE YARD 40-60 s, THE GANTRY 20-35 s) x [1.3, 2, 3, 4.5]; kills and style as n36 (NOT calibrated: the walk bot's medians replace them).",
    } : {
      time: [215, 330, 495, 740], kills: [100, 85, 65, 40], style: [2500, 1500, 700, 250],
      _why: "Unity-only (builder L, NIGHT36): S/A/B/C cuts: time = the beat sheet's summed room times (about 145 s) + THE LANTERN's bot band (15-22 s), x [1.3, 2, 3, 4.5]; kills and style as the n35 level 1 (NOT calibrated: no bot walk yet).",
    },
  });

const ARENA = (r) => { r.row.arena36 = true; };

// ---------------------------------------------------------------- 1.1 INTAKE (teach, calm: move, look, jump, slide, dash; no enemy, no lava)
{
  const r = L.room({ id: "a1", name: "1.1 INTAKE", kind: "link", job: "teach", verb: "move, look, jump, slide, dash", w: 12, l: 31, h: 6, par: 12, spawnLz: 2, exitFromN: 2 });
  // round 2 (levelgen's pit recovery: a trench past the slot could not walk back to the spawn): the order is step > trench > slot
  r.box("deck", -6, 6, 6, 10, 0, 1.0, "line", `the JUMP: a 1.0 m step wall to wall (jump.height ${MOVE.jumpH} m clears it; no way round); its north edge is the trench's take-off`);
  // the dash trench: 7 m (a running jump clears about 5 m; a jump + dash (6 m) or a slide jump clears it); a dry pit 1.5 m deep: a miss costs a climb back
  r.pit(-6, 11, 6, 18, -1.5, "the DASH trench: 7 m wall to wall, a dry 1.5 m pit (no lava in the calm start): jump, then dash");
  r.box("deck", -6, 11, -4, 12.5, -2.5, -0.4, "climb", "the trench's way back: a step 1.1 m up from the pit floor, then 0.4 m onto the take-off floor (a miss costs a climb, never health)");
  // the slide slot: a 4 m wide, 5 m long tunnel with a 1.2 m ceiling (slide.colliderHeight 1.0 passes, a standing player does not); its sides close the hall
  r.tunnel(-2, 22, 2, 27, 1.2, `the SLIDE slot: a 1.2 m ceiling over 5 m (slide collider ${MOVE.slideH} m): the only way north`);
  r.box("hull", -6, 22, -2, 27, 0, 6, "line", "the slot's west side, floor to roof: the hall narrows into the slot");
  r.box("hull", 2, 22, 6, 27, 0, 6, "line", "the slot's east side, floor to roof: the hall narrows into the slot");
  r.box("hull", -2, 22, 2, 27, 3.2, 6, "line", "closes the slot above its 2 m ceiling slab (no way over the slot)");
  r.lesson({ kind: "reached", rect: [-6, 6, 6, 10], minY: 0.9, prompt: "MOVE. LOOK. {jump} JUMP." });
  r.lesson({ kind: "reached", rect: [-6, 18, 6, 22], minY: -0.2, prompt: "" });
  r.lesson({ kind: "slidTunnel", via: [-2, 22, 2, 27], rect: [-6, 27, 6, 31], prompt: "HOLD {slide}: SLIDE UNDER" });
  r.teach("l1.dash", -6, 8, 6, 11, 0.5);
  // the crossings only the real Mover makes (levelgen proves the slot with its own tunnel run; the route graph goes mouth to far side)
  r.row.routeVia = [[r.pt(0, 0, 21), r.pt(0, 0, 28)]];
  // light: white overheads down the hall; cyan edge strips on the step's face (player-usable); a cyan pool at the slot's mouth; the cyan pool at the door (the goal)
  r.ceil(0, 3, 3, 2, "white", 9, 1.0); r.ceil(0, 14.5, 3, 3, "white", 8, 0.9); r.ceil(0, 29, 3, 2, "white", 9, 1.0);
  r.trim("W", 6.2, 3, 0.9, "cyan"); r.trim("E", 6.2, 3, 0.9, "cyan");
  r.pool(0, 20, 2, "cyan", 5, 1.2); // the slot's mouth (4 m of run-up: levelgen's tunnel run starts 4 m short of it)
  r.pool(0, 29.5, 2.5, "cyan", 7, 1.6);
  r.walk([[3, 0, 4], [3, 1.0, 9]], { jump: true, maxSeconds: 6 });
  r.walk([[0, -1.5, 15], [-5, -0.4, 11.8]], { jump: true, maxSeconds: 8 });
  r.walk([[0, 0, 28], [0, 0, 29]]);
  r.frames([0, 1.0, 9], [0, 0.5, 24], [3, 0, 19.5], [0, 0.5, 27]); // mid: on the step, the trench and the slot's mouth ahead; exit: past the trench, the slot (the way out)
}

// ---------------------------------------------------------------- 1.2 KESTREL (teach: the tap fires; the hold charges and pierces)
{
  const r = L.room({ id: "a2", name: "1.2 KESTREL", job: "teach", verb: "fire (tap), charge (hold: pierces)", w: 14, l: 20, h: 7, par: 18, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  [9, 11, 13].forEach((lz) => r.standIn(0, 0, lz, "the stand-in LINE on the axis: one charged shot from the door pierces all three"));
  r.box("deck", -7, 15, -1, 20, 0, 2.0, "line", "the north deck (2 m): where the first husks switch on, seen from the door; the high ground after");
  r.ramp(-4, 10, 15, 3, 2.0, "climb", "the husks' way down from the deck toward you (2 m over 5 m), and the player's way up");
  r.highNodeAt(-5, 18, 2.0); r.highNodeAt(-3, 18.5, 2.0);
  r.row.waves = [["husk", "husk"]];
  r.lesson({ kind: "shots", n: 3, prompt: "" });
  r.lesson({ kind: "husksDead", n: 2, prompt: "" });
  r.teach("l1.fire", -7, 0, 7, 4);
  r.teach("l1.charge", -7, 4, 7, 7);
  r.ceil(0, 11, 2, 6, "white", 9, 1.2); // the line under its light
  r.ceil(-4, 17.5, 4, 3, "amber", 8, 1.0); // where the husks will be
  r.pool(0, 18, 2.5, "cyan", 7, 1.6); // the exit
  r.walkSE([[2, 0, 9], [2, 0, 16]]);
  r.walk([[-4, 0, 9], [-4, 2.0, 16], [-5, 2.0, 18]]);
  r.frames([5, 0, 4], [-2, 1, 13], [-5, 2.0, 18], [3, 1.6, 18.5]); // mid: the stand-in line and the deck's ramp behind it; exit: from the deck across to the exit pool
}

// ---------------------------------------------------------------- 1.3 RIDE (practise: over a floor, then over lava; husks on the bridge)
{
  const r = L.room({ id: "a3", name: "1.3 RIDE", job: "practise", verb: "wall ride (over a floor, then over lava)", w: 16, l: 26, h: 9, par: 22, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const P1 = -4.4, P2 = P1 + 5.0; // ridden planes, 5.0 m apart (inside M's working facing gaps), P1's east face then P2's west face
  if (!MOVE.faceGaps.includes(5)) throw new Error("a 5 m facing gap is not in the measured list");
  r.panel("E", P1, 2, 2 + MOVE.panel, 0, 6, `ride panel P1 over the DRY floor: ride its east face north (${MOVE.panel} m = 1 s at ${MOVE.rideSpeed} m/s); a fall costs nothing`);
  r.panel("W", P2, 12, 23, -0.8, 6, "ride panel P2 over the LAVA, 5.0 m east of P1 plane to plane: jump off P1 across onto its west face, ride north over the lava to the far floor");
  r.lava(-8, 13, 6, 22, 0.8, "the RIDE's hazard (14 x 9 m) and the husks' grave: they cross on the 2 m bridge beside it, where a dash-punch launches them in");
  r.nodeAt(-4, 24.5); r.nodeAt(4, 24.5);
  r.row.waves = [["husk", "husk"]];
  r.row.bridge36 = { rect: r.rect(6, 13, 8, 22), why: "the 2 m dry bridge along the east wall: the only walk route between the floors (the walkers' route; the long way round)" };
  r.pickup("healthPickup", -5.5, 24.5, 0.9, "a ride that ends in the lava costs health: it is paid back on the far floor");
  r.lesson({ kind: "reached", rect: [-8, 23, 8, 26], minY: -0.2, prompt: "HOLD {jump} ON A WALL: RIDE" });
  r.teach("l1.zoom", -5, 3, -2, 12, 0.3);
  r.ceil(-2, 7, 3, 3, "white", 9, 1.0); r.ceil(3, 24, 3, 2, "white", 8, 1.0);
  r.pool(-1, 17.5, 3, "orange", 8, 1.2, { danger: true }, -0.8);
  r.pool(7, 17.5, 1.5, "white", 5, 0.8); // the bridge reads at a glance
  r.pool(0, 24.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[7, 0, 11], [7, 0, 23]]);
  r.frames([6.5, 0, 1.5], [-1.5, 1.5, 14], [-2, 0, 11], [0, 0.5, 25]); // mid: from the SE corner up the line: P1 over the floor, P2 in the lava, the bridge right; exit: from P1's far end over the lava to the exit
}

// ---------------------------------------------------------------- 1.4 PARRY (teach: one spitter alone in a tight space; its glob is yellow)
{
  // round 2 (levelgen: spawn31 wants a node in the first view at 12 m or more; the stairs did not carry walkers): the perch at lz 13..16, ramps up to it
  const r = L.room({ id: "a4", name: "1.4 PARRY", job: "teach", verb: "the parry (punch the yellow glob back)", w: 8, l: 19, h: 6, par: 16, spawnLz: 2, exitFromN: 1.5 });
  ARENA(r);
  r.box("deck", -4, 13, 4, 16, 0, 1.5, "line", "the PERCH (1.5 m) wall to wall: the spitter stands on it in plain view of the door; nothing between you and it");
  r.ramp(-3, 9, 13, 2, 1.5, "climb", "the perch's west ramp (the spitters' and the player's way up: 1.5 m over 4 m)");
  r.ramp(3, 9, 13, 2, 1.5, "climb", "the perch's east ramp");
  r.highNodeAt(0, 14.5, 1.5); r.highNodeAt(-3, 14.5, 1.5); r.highNodeAt(3, 14.5, 1.5);
  r.row.waves = [["spitter"], ["spitter", "spitter"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "PUNCH {punch} THE YELLOW GLOB BACK" });
  r.ceil(0, 14.5, 4, 2, "white", 7, 1.2);
  r.pool(0, 14.5, 2, "amber", 5, 1.4, undefined, 1.5); // yellow = punch it
  r.pool(0, 17.5, 2, "cyan", 6, 1.4);
  r.walk([[-3, 0, 4], [-3, 0, 8.5], [-3, 1.5, 14], [0, 0, 17.5]]);
  r.frames([-3, 0, 4], [0, 2, 14.5], [-3, 1.5, 14.5], [1, 1.5, 18.5]); // mid: the perch and its spitter from the west ramp's foot; exit: from the perch to the exit pool
}

// ---------------------------------------------------------------- 1.5 CRUCIBLE (combine: ride + parry + launch into the lava)
{
  const r = L.room({ id: "a5", name: "1.5 CRUCIBLE", job: "combine", verb: "ride + parry + launch", w: 20, l: 24, h: 10, par: 45, spawnLz: 2, exitFromN: 1.5 });
  ARENA(r);
  if (!MOVE.faceGaps.includes(8)) throw new Error("an 8 m facing gap is not in the measured list");
  r.lava(-6, 5, 6, 16, 0.8, "the CRUCIBLE's centre (12 x 11 m): ride over it under spitter fire; the rims beside it are the husks' only way round: punch them in");
  r.panel("E", -4.0, 5, 5 + MOVE.panel, -0.8, 7, "ride panel W in the lava: its east face north toward the deck, punching the globs on the way");
  r.panel("W", 4.0, 5, 5 + MOVE.panel, -0.8, 7, "ride panel E in the lava, 8 m from W plane to plane: its west face back south (or north to the deck)");
  r.box("deck", -10, 18, 10, 21, 0, 1.2, "line", "the north deck (1.2 m: a jump off a ride lands on it): the spitters' stand and the ride line's landing");
  r.ramp(-8, 14, 18, 3, 1.2, "climb", "the west rim's ramp onto the deck (walkers: 1.2 m over 4 m)");
  r.ramp(8, 14, 18, 3, 1.2, "climb", "the east rim's ramp onto the deck");
  r.nodeAt(-8, 3); r.nodeAt(8, 3); r.nodeAt(-8, 11); r.nodeAt(8, 11);
  r.highNodeAt(-4, 19.5, 1.2); r.highNodeAt(0, 19.5, 1.2); r.highNodeAt(4, 19.5, 1.2);
  r.row.waves = [["husk", "husk"], ["spitter", "husk", "husk"], ["spitter", "spitter", "husk"]];
  r.row.rims36 = { rects: [r.rect(-10, 0, -6, 18), r.rect(6, 0, 10, 18)], why: "the dry rims either side of the lava: the walkers' only way between the door and the deck" };
  r.pickup("healthPickup", 0, 19.5, 2.1, "the high ground you ride to pays health");
  r.ceil(-6, 3, 3, 3, "white", 9, 1.0); r.ceil(6, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 19.5, 6, 2, "white", 9, 1.1);
  r.pool(0, 10.5, 4, "orange", 9, 1.3, { danger: true }, -0.8);
  r.pool(0, 22.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[-8, 0, 2], [-8, 0, 13], [-8, 1.2, 19], [0, 1.2, 20], [0, 0, 22.5]]);
  r.frames([-9, 0, 1], [3, 1.2, 18], [8.5, 0, 13], [0, 0.5, 22.5]); // mid: from the SW corner across the lava: both panels in it, the deck and its spitters behind; exit: from the east rim's north end over the deck to the exit
}

// NIGHT38 F: the two new rooms come after the CRUCIBLE and before the rest (n38-foundry only; Design/levels/n38/foundry.beats.md)
if (N38) require("./n38f-foundry-rooms.cjs")(L, MOVE);

// ---------------------------------------------------------------- 1.6 REST (a breath; the checkpoint)
{
  const r = L.room({ id: "a6", name: "1.6 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: 3, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 5, 0.9, "the last rest before THE LANTERN: full health for the exam");
  r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
  r.ceil(0, 5, 3, 3, "white", 8, 1.0);
  r.pool(0, 8.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
}

if (N37) n37Rooms();
else {
// ---------------------------------------------------------------- 1.7 HALL (the walk-in: a plain 4 m hall, then a 4 m door into the ring)
// round 2 (levelgen's doorFit: a door must be the 4 m between two wall rings, so B's 8 m hall-door does not build): the hall is a LINK room 4 x 4 m
// (arena-local z +19..+23) plus the 4 m door (z +15..+19) into the ring: 12 m of walk-in from the rest's door; the door behind locks on the ring's entry.
{
  const r = L.room({ id: "a7", name: "1.7 HALL", kind: "link", job: "link", verb: "walk in", w: 4, l: 4, h: 4.5, par: 2, spawnLz: 0.5, exitFromN: 0.5 });
  r.pool(0, 3.5, 1.5, "cyan", 4, 0.8); // the goal pool at the hall's mouth (levelgen: one within 8 m of the exit)
  r.ceil(0, 2, 2, 2, "white", 5, 0.5); // dim: the ring's well light is what you see
  r.walkSE([]);
  r.frames([0, 0, 0.8], [0, 1, 40], [0, 0, 3.5], [0, 2, 40]); // the ring's well and the far panel's trim through the door
}

// ---------------------------------------------------------------- 1.8 THE LANTERN (boss: B's ring, built by B's world layer at load)
{
  const APO = 15, ZONE_R = 12.5; // B's spec: the ring's apothem; the room's zone disc
  const prev = L.prev;
  const s = { id: "a8", name: "1.8 THE LANTERN", kind: "boss", job: "boss", verb: "ride + parry (the exam)", w: 2 * APO, l: 2 * APO, h: 14, par: 20, cx: prev.outX, spawnLz: APO - 12, exitFromN: APO };
  const r = L.room(s);
  r.row.roof = false;
  r.row.boss = "hulk";
  const C = [r.X(0), 0, r3(r.z1 - APO)];
  r.row.spawn = { at: [C[0], 0, r3(C[2] + 12)], yawDeg: 0 };
  r.row.exit = [C[0], 0, C[2]];
  r.frames([0, 0, APO - 8], [0, 4.5, APO], [0, 0, APO + 11], [0, 1, APO]); // mid: 8 m from the well, up at the core (4.5 m); exit: from inside the entry at the well
  r.row.lantern36 = { centre: C, entryYawDeg: 90, zoneR: ZONE_R, _why: "builder B's ring is built here (Design/levels/n36/lantern-hall.spec.md); the approach is the 4 x 4 m hall a7 (arena-local z +19..+23) and the 4 m door d7 (z +15..+19)" };
}
}

// ================================================================ NIGHT37 W: THE WICK's rooms (n37-foundry only)
function n37Rooms() {
  const W = JSON.parse(require("fs").readFileSync(require("path").join(__dirname, "../../Design/unity-config.json"), "utf8")).design.n37.wick37;
  const ST = W.stack;
  const RISE = ST.rise, PH = ST.panelHeight;
  // a box panel facing N or S (n36-lib's panel() makes E / W ones): along x from lx0 to lx1, the ridden plane at lz `plane`
  // ---------------------------------------------------------------- 1.7 THE FLUE (teach: the leap's look pitch; three panels, each RISE higher)
  {
    const r = L.room({ id: "a7", name: "1.7 THE FLUE", kind: "link", job: "teach", verb: "ride, look up, release: the leap", w: 12, l: 32, h: 13, par: 14, spawnLz: 1.5, exitFromN: 1.5 });
    const PX = 3.5; // the panels' ridden planes, 7 m apart (in the measured facing gaps: Design/levels/n35-ride-distances.json)
    if (!MOVE.faceGaps.includes(7)) throw new Error("a 7 m facing gap is not in the measured list");
    r.panel("E", -PX, 2, 10, 0, PH, `ride panel F1 on the floor: ride its east face north; at its end LOOK UP and release {jump}: the leap's height follows the look (design.n35.wallride35 upLow..upHigh)`);
    r.panel("W", PX, 9, 17, RISE, RISE + PH, `ride panel F2, ${RISE} m higher than F1 (THE STACK's rise, design.n37.wick37.stack.rise): a level leap passes under it, a leap looking up meets it`);
    r.panel("E", -PX, 16, 24, 2 * RISE, 2 * RISE + PH, `ride panel F3, ${RISE} m higher again: the third step of the climb`);
    const DECK = 3 * RISE - 0.4;
    r.box("deck", -6, 26, 6, 29, 0, DECK, "line", `the FLUE's top deck (${DECK} m) wall to wall: only the climb (F1 -> F2 -> F3, leaping with the look up) gets over it; its north edge drops to the exit floor`);
    r.lesson({ kind: "reached", rect: [-6, 0, 6, 4], minY: -0.2, prompt: "LOOK UP, RELEASE {jump}: LEAP" });
    r.row.routeVia = [[r.pt(0, 0, 12), r.pt(0, DECK, 27.5)]]; // the climb: only the real Mover makes it (harness `wick37 climb` measures the hop)
    r.lesson({ kind: "reached", rect: [-6, 26, 6, 29], minY: DECK - 0.5, prompt: "" });
    r.ceil(0, 6, 3, 3, "white", 9, 1.0); r.ceil(0, 27.5, 4, 2, "white", 9, 1.1);
    r.pool(-PX + 1, 6, 1.5, "cyan", 5, 1.0); r.pool(PX - 1, 13, 1.5, "cyan", 6, 1.0, undefined, 0); r.pool(0, 27.5, 2.5, "cyan", 7, 1.6, undefined, DECK);
    r.pool(0, 30.5, 2, "cyan", 6, 1.4);
    r.walk([[0, 0, 2], [0, 0, 12]]);
    r.walk([[0, DECK, 27.5], [0, 0, 30.5]]);
    r.frames([2, 0, 3], [-PX, 4, 18], [-3, DECK, 27.5], [0, 0.5, 31]); // mid: from the floor up the three panels; exit: from the deck down to the door
  }
  // ---------------------------------------------------------------- 1.8 PIERCE (teach: the charged KESTREL from a ride pierces a line)
  {
    const r = L.room({ id: "a8", name: "1.8 PIERCE", kind: "link", job: "teach", verb: "a charged shot from a ride pierces", w: 14, l: 22, h: 9, par: 14, spawnLz: 1.5, exitFromN: 1.5 });
    r.pit(-7, 3, 7, 19, -4, "the PIERCE pit (4 m deep, dry): the only way across is the ride; a fall costs the climb back up the south steps");
    // the pit's way back: three 1.2 m steps along the west wall behind P1 (jump.height 1.5 clears each), then 0.4 m onto the entry floor
    r.box("deck", -7, 3, -4.8, 4.5, -4, -0.4, "climb", "the pit's way back, step 3 (0.4 m under the entry floor): a fall costs a climb, never health");
    r.box("deck", -7, 4.5, -4.8, 6, -4, -1.6, "climb", "the pit's way back, step 2 (1.2 m under step 3)");
    r.box("deck", -7, 6, -4.8, 7.5, -4, -2.8, "climb", "the pit's way back, step 1 (1.2 m over the pit floor)");
    r.panel("E", -4, 4, 12, -4, 3.2, "ride panel P1 over the pit: ride north; the stand-ins hang in a line ahead of the ride: hold {fire} on the wall and pierce them all");
    r.panel("W", 4, 11, 19, -4, 3.2, "ride panel P2 over the pit, 8 m east of P1: the leap across onto it carries you to the far floor");
    if (!MOVE.faceGaps.includes(8)) throw new Error("an 8 m facing gap is not in the measured list");
    // the line: from a rider on P1 (lx -3.65, eye about 1.8 m, lz 6) along (0.25, 0, 1): three stand-ins one charged shot pierces
    [[-2.15, 12], [-1.4, 15], [-0.65, 18]].forEach(([lx, lz]) => r.standIn(lx, 1.8, lz, "a stand-in on the ride's line over the pit: seen and pierced from the wall (THE WICK's damage rule: a charged shot from a ride)"));
    r.lesson({ kind: "shots", n: 1, prompt: "RIDE, HOLD {fire}: PIERCE" });
    r.row.routeVia = [[r.pt(0, 0, 2.5), r.pt(0, 0, 20)]]; // the crossing over the pit: the rides (as a3's over the lava)
    r.ceil(0, 11, 3, 6, "white", 9, 1.0); r.ceil(0, 20.5, 3, 2, "white", 8, 1.0);
    r.pool(-1.4, 15, 2, "amber", 6, 1.0, undefined, -4);
    r.pool(0, 20.5, 2.5, "cyan", 7, 1.6);
    r.walk([[0, 0, 1.5], [3, 0, 2.5]]);
    r.walk([[0, 0, 20], [0, 0, 21]]);
    r.frames([-3.65, 0.2, 5], [-0.65, 1.8, 18], [2, 0, 20], [0, 1, 22]); // mid: from P1's start down the stand-in line; exit: the far floor to the door
  }
  // ---------------------------------------------------------------- 1.9 THE STACK (boss: THE WICK; the shaft is Core/Enemies/Wick37Stack.cs at load)
  {
    // the cap's height as Wick37Stack lays the spiral out (panels rise by `rise`, a ledge arc after each of ledgeAfter, the cap ledgeOver over the last)
    let b = ST.firstBottom;
    for (let i = 0; i < ST.panelCount - 1; i++) b = ST.ledgeAfter.includes(i) ? b + ST.ledgeOver + ST.sectionStartAbove : b + ST.rise;
    const CAPY = b + ST.ledgeOver;
    const HALF = ST.capRadius + 1, H = Math.ceil(CAPY + ST.capWallHeight + 2);
    const prev = L.prev;
    const r = L.room({ id: "a9", name: "1.9 THE STACK", kind: "boss", job: "boss", verb: "the climb: ride, leap, parry, pierce (the exam)", w: 2 * HALF, l: 2 * HALF, h: H, par: 22, cx: prev.outX, spawnLz: 1.5, exitFromN: HALF });
    r.row.roof = false;
    r.row.boss = "wick";
    const C = [r.X(0), 0, r3(r.z1 - HALF)];
    r.row.spawn = { at: [C[0], 0, r3(C[2] + HALF - 1.5)], yawDeg: 0 };
    r.row.exit = [C[0], 0, C[2]];
    r.frames([0, 0, HALF - 6], [-ST.panelApothem, 6, HALF], [0, 0, HALF + 2], [0, 20, HALF]); // mid: from the gap up the spiral; exit: the stack's well from the floor
    r.row.wick37 = { centre: C, _why: "NIGHT37 W: THE STACK is built here at load (Core/Enemies/Wick37Stack.cs, design.n37.wick37.stack): the round shaft (its entry gap and tunnel on +z, the door's side), the spiral of ride panels, the ledge rings, the cap. The square room's walls stand behind it." };
  }
}

// NIGHT38 F: the display names after the two new rooms renumbered (ids, geometry and rows unchanged)
if (N38) {
  const RENAME = { "1.6 REST": "1.8 REST", "1.7 THE FLUE": "1.9 THE FLUE", "1.8 PIERCE": "1.10 PIERCE", "1.9 THE STACK": "1.11 THE STACK" };
  for (const r of L.rooms) if (RENAME[r.row.name] && r.id[0] === "a") r.row.name = RENAME[r.row.name];
}

L.write(OUT);

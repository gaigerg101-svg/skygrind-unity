// NIGHT41 builder L6 (NIGHT41.md card L6; INVENTORY A13; DECIDE41 D-3): LEVEL 6 THE MIRROR, a GREYBOX from its beat sheet
// Design/levels/n41/finale.beats.md (written first). Writes Design/levels/n41/finale.plan.json (id n41-finale); Tools/night/n41-finale-regen.sh runs
// the jobs checker, levelgen and the copies (Design/levels/n41-finale.json + StreamingAssets), replacing the shared commit's placeholder.
// The campaign plays it only behind design.n41.finale41 (OFF tonight: GG reacts to the plan first; level 6's slot stays locked).
// Rebuilt from scratch (GG 2026-10-13: level 6 "looks awful"): no n35 / n38 finale room, prop or generator is used. Read as DATA only: the n38-finale
// plan's rank kills / style cuts (not calibrated there either) and wallHeight.
// The exam: every room a COMBINE room of levels 1-5's verbs, small (20-30 m), one job each, a climb route for enemies and a walker's line in each:
// m1 6.1 RETURN, m2 6.2 THE GALLERY, m3 6.3 THE STEP, m4 6.4 THE HOOK, m5 6.5 THE CHANNEL, m6 6.6 THE WINDOW, m7 6.7 EVERYTHING, mr 6.8 REST, then
// mx 6.9 THE MIRROR: THE RIVAL's arena, reserved EMPTY (kind boss, boss id "rival41": no sim code answers it, so the room clears at once).
// Room ids m* (round 3; were f1..f7, fr, fx): Walk29Pilot scripts the n29 / n30 foundry rooms by id alone (f2 / f3 GapRoom, f4 Flue, f5 Ride, f7..f9),
// so the walk bot ran THOSE rooms' lanes here (in 6.3 it held x 0 and hopped at the face, killing nothing: `n41fin walk --probe`).
// Run: node Tools/night/gen-n41-finale.cjs   (then bash Tools/night/n41-finale-regen.sh)
const fs = require("fs");
const path = require("path");
const { Level, MOVE } = require("./n36-lib.cjs");

const ROOT = path.join(__dirname, "../..");
const OUT = "Design/levels/n41/finale.plan.json";
const old = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n38/n38-finale.plan.json"), "utf8"));

// the beat sheet's bot times (s): the rank cuts and each room's par
const TIMES = { m1: 30, m2: 35, m3: 35, m4: 35, m5: 30, m6: 35, m7: 60, mr: 4 };
// THE STEP's height: over the jump + double jump (jump.height x (1 + doubleJumpMult) = 1.5 x 1.9 = 2.85 m), under it + one THUNDERCLAP boost (about +2 m,
// n38 THE STEP's sheet): 3.6 m, as n38's (Design/levels/n41/finale.beats.md rules)
const STEP_H = 3.6;
const LAVA_D = 1; // the channel's surface 1 m under the floor (Core/Lava30.cs: it burns the player, kills an enemy)

const L = new Level("n41-finale", "LEVEL 6 THE MIRROR",
  "NIGHT41 builder L6 (design.n41.finale41, OFF: DECIDE41 D-3): Tools/night/gen-n41-finale.cjs from the beat sheet Design/levels/n41/finale.beats.md: "
  + "the exam of levels 1-5's verbs in small combine rooms, one job per room: 6.1 RETURN (parry + launch + the gauntlet), 6.2 THE GALLERY (ride + zoom + the pierce), "
  + "6.3 THE STEP (the boost up, the slam down), 6.4 THE HOOK (whip + bend + swap), 6.5 THE CHANNEL (the one lava, a launch target), 6.6 THE WINDOW (ride jump-off + "
  + "jump dash to the wardens' backs), 6.7 EVERYTHING, 6.8 REST, 6.9 THE MIRROR (THE RIVAL's arena, reserved empty). A GREYBOX: GG reacts before it is built further.",
  { rank: RANK41() });
const ARENA = (r) => { r.row.arena36 = true; };
const nodes = (r, pts) => pts.forEach(([x, z]) => r.nodeAt(x, z));

// ---------------------------------------------------------------- 6.1 RETURN (combine: the parry and the launch, with the gauntlet)
{
  const r = L.room({ id: "m1", name: "6.1 RETURN", job: "combine", verb: "parry the yellow shots back, launch a husk into the others, the gauntlet on the tank", w: 22, l: 24, h: 9, par: TIMES.m1, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -11, 18, -2, 24, 0, 3, "line", "the PERCH's west half (3 m): the spitters' stand; their yellow shots come down at the door, the parry's targets");
  r.box("deck", 2, 18, 11, 24, 0, 3, "line", "the PERCH's east half (3 m); the exit runs between the halves at floor level");
  r.ramp(-6.5, 10, 18, 3, 3, "climb", "the west perch's ramp (3 m over 8 m): the husks walk down it at you (the launch's line), you walk up it");
  r.ramp(6.5, 10, 18, 3, 3, "climb", "the east perch's ramp");
  r.panel("E", -10, 3, 14, 0, 5, "ride panel W (face east, 11 m): the dodge line while the shots fly; no cover in the middle (the parry is the cover)");
  r.panel("W", 10, 3, 14, 0, 5, "ride panel E (face west)");
  nodes(r, [[-3, 15], [0, 15], [3, 15], [0, 17]]); // >= 12 m from the door in its first view (spawn31)
  r.highNodeAt(-7, 21, 3); r.highNodeAt(7, 21, 3); r.highNodeAt(-9, 22.5, 3); r.highNodeAt(9, 22.5, 3);
  r.row.waves = [["spitter", "spitter", "husk", "husk", "husk"], ["tank", "drone"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{punch} THE YELLOW BACK" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 13, 4, 4, "white", 9, 1.0); r.ceil(-7, 21, 4, 4, "amber", 9, 1.1); r.ceil(7, 21, 4, 4, "white", 9, 1.0);
  r.pool(0, 22, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [0, 0, 18]]);
  r.walk([[-6.5, 0, 8], [-6.5, 3, 19], [-7, 3, 21]]);
  r.frames([5, 0, 4], [0, 3, 21], [0, 0, 16], [0, 1, 24]);
}

// ---------------------------------------------------------------- 6.2 THE GALLERY (combine: the ride, the zoom and the charged pierce)
{
  const r = L.room({ id: "m2", name: "6.2 THE GALLERY", job: "combine", verb: "ride the wall above the charger, zoom (slow39) and pierce the husks filing down the ramp", w: 20, l: 32, h: 10, par: TIMES.m2, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.panel("E", -9, 4, 26, 0, 5, "ride panel W (face east, 22 m: two seconds of held ride): the gallery's line above the charger's floor; zoom and charge from it");
  r.panel("W", 9, 4, 15, 0, 5, "ride panel E (face west, 11 m): the line mirrored on the short side");
  r.solid("pillar", -3.5, 13, 2.5, 5, 2.5, "cover", "the PILLAR (2.5 x 2.5 x 5 m) on the charger's line from the north: step aside so it hits this, stunned, its back x3 (Charger.cs); the room's one cover");
  r.box("deck", -10, 27, -2, 32, 0, 4, "line", "the BALCONY's west half (4 m): wave 2's spitters stand on it; the W panel's ride ends under it (the jump-off lands on it)");
  r.box("deck", 2, 27, 10, 32, 0, 4, "line", "the BALCONY's east half (4 m), its ramp's top; the exit runs between the halves at floor level");
  r.ramp(5, 17, 27, 3, 4, "climb", "the east balcony's long ramp (4 m over 10 m): the husks file down it in a single line (the charged pierce's target), the walkers' way up");
  r.ramp(-5.5, 17, 27, 3, 4, "climb", "the west balcony's ramp (round 2: levelgen found the west half unreachable by walkers): the spitters' way down, the second file");
  nodes(r, [[0, 18], [-2, 22], [2, 22]]);
  r.highNodeAt(4, 29.5, 4); r.highNodeAt(7, 29.5, 4); r.highNodeAt(-5, 29.5, 4); r.highNodeAt(-8, 29.5, 4);
  r.row.waves = [["charger", "husk", "husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "RIDE, {altFire}, HOLD {fire}" });
  r.ceil(0, 3, 3, 3, "white", 10, 1.0); r.ceil(0, 14, 4, 6, "white", 10, 1.0); r.ceil(5, 22, 3, 6, "amber", 10, 1.1); r.ceil(-6, 29.5, 4, 3, "white", 10, 1.0);
  r.pool(0, 30, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [1, 0, 20]]);
  r.walk([[5, 0, 15], [5, 4, 28], [6, 4, 29.5]]);
  r.frames([-6, 0, 4], [5, 3, 24], [0, 0, 22], [0, 1, 32]);
}

// ---------------------------------------------------------------- 6.3 THE STEP (combine: the THUNDERCLAP's boost up, the slam and the blast down)
{
  // round 2: 32 m long (was 26): levelgen found no player route over the deck to a 3 m exit strip (a drop only); now a down-ramp north of THE STEP
  const r = L.room({ id: "m3", name: "6.3 THE STEP", job: "combine", verb: `the THUNDERCLAP's mid-air boost up THE STEP (${STEP_H} m), the slam and the blast back down`, w: 22, l: 32, h: 10, par: TIMES.m3, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const deck = r.box("deck", -11, 12, 11, 20, 0, STEP_H, "line",
    `THE STEP (${STEP_H} m) across the room: over the jump + double jump (${MOVE.jumpH} m x 1.9 = ${(MOVE.jumpH * 1.9).toFixed(2)} m), under it + one THUNDERCLAP boost (about +2 m); its spitters shoot down; NORIDE (the ride must not climb it)`);
  deck.noride = true; // builder M's flag: dull, no cyan trim; the held ride cannot climb its face
  r.ramp(-8, 1, 12, 3, STEP_H, "climb", `the WALKER's ramp up (${STEP_H} m over 11 m; its foot 1 m off the south wall, 1.5 m off the west wall): the long way round under fire, the enemies' climb route; the boost saves it`);
  r.ramp(8, 30, 20, 3, STEP_H, "climb", `the down-ramp north of THE STEP (${STEP_H} m over 10 m, 1.5 m off the east wall): the walkers' way down to the exit (a boosted player drops off the lip)`);
  // round 3: THE HALF STEP. The route graph (Core/Nav/NavGraph.cs: the walk bot's BotNav and levelgen's player route) puts nodes on BOX TOPS only and
  // walks an edge only within nav.stepHeight (0.4 m); a ramp (a wedge) carries no node, so levelgen found no player route up THE STEP. A block half the
  // step's height at the face: the graph's hop / climb edges see a way (levelgen: "walk double drop"), and a player without the boost has a slower climb
  // (two jumps; the boost is one). Measured: stairs of 0.4 m boxes made the graph PASS but the real Mover does not walk up a 0.4 m step (levelgen's walk
  // proof stopped at the first one).
  // (round 3, second try: one 1.8 m block let levelgen's graph PASS, but the walk bot stood on it against the face for 1400 s, 2 / 2 seeds, after
  // clearing the room: its follower never flew the 1.8 m double up; two 1.2 m blocks are a hop each, under the jump's 1.5 m)
  r.box("hull", -5.5, 7, -2.5, 9.5, 0, STEP_H / 3, "climb", `THE HOPS, the low block (${STEP_H / 3} m, 3 x 2.5 m): three hops up THE STEP for a player without the boost (the boost is one), and the route graph's way up`);
  r.box("hull", -5.5, 9.5, -2.5, 12, 0, 2 * STEP_H / 3, "climb", `THE HOPS, the high block (${2 * STEP_H / 3} m) against THE STEP's face: the last hop onto the deck`);
  // every spot ON THE STEP (round 2: spots on the north floor put enemies behind the 3.6 m face, out of sight and reach); the spitters stand at the lip,
  // seen from the south floor; the swarmers come down the up-ramp or drop off the lip at you
  r.highNodeAt(-6, 12.8, STEP_H); r.highNodeAt(-2, 14.2, STEP_H); r.highNodeAt(2, 14.2, STEP_H); r.highNodeAt(6, 12.8, STEP_H); // the middle two >= 12 m from the door (spawn31)
  r.highNodeAt(-4, 15.5, STEP_H); r.highNodeAt(4, 15.5, STEP_H); r.highNodeAt(0, 17, STEP_H); r.highNodeAt(-7, 18, STEP_H); r.highNodeAt(7, 18, STEP_H);
  r.row.waves = [["spitter", "spitter", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"], ["tank", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{jump}, {jump}, {fire} DOWN: UP" });
  r.ceil(0, 3, 3, 3, "white", 10, 1.0); r.ceil(0, 8, 4, 4, "white", 10, 1.0); r.ceil(0, 16, 6, 4, "amber", 10, 1.1); r.ceil(0, 26, 6, 4, "white", 10, 1.0);
  r.pool(0, 30, 2, "cyan", 7, 1.6);
  r.walkSE([[-8, 0, 0.6], [-8, STEP_H, 13], [0, STEP_H, 16], [8, STEP_H, 19], [8, 0, 30.5]]);
  r.frames([4, 0, 4], [0, STEP_H, 14], [0, STEP_H, 16], [0, 1, 32]);
}

// ---------------------------------------------------------------- 6.4 THE HOOK (combine: the whip, the bend, the swap to the slug)
{
  const r = L.room({ id: "m4", name: "6.4 THE HOOK", job: "combine", verb: "whip a drone in, swap inside the bend, slug it; hook over the warden to its back", w: 24, l: 28, h: 12, par: TIMES.m4, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.solid("post", -6, 18, 1, 9, 1, "line", "the west POST (1 x 9 x 1 m): it carries the west grapple node at 9.5 m, nothing else (a hook over the warden's head)");
  r.solid("post", 6, 18, 1, 9, 1, "line", "the east POST: the east node");
  r.row.grappleNodes = [r.pt(-6, 9.5, 18), r.pt(6, 9.5, 18)];
  r.row.grapple = "two nodes on posts at 9.5 m either side of the middle: a pull over the warden lands you behind it";
  r.box("deck", -12, 18, -4, 28, 0, 3, "line", "the MEZZANINE (3 m) on the west side: the husks and wave 2 come off it");
  r.ramp(-8, 10, 18, 3, 3, "climb", "the mezzanine's ramp (3 m over 8 m): the husks' and the warden's way down, yours up");
  nodes(r, [[-5, 16], [5, 16], [0, 20], [3, 22], [-2, 23]]);
  r.highNodeAt(-8, 22, 3); r.highNodeAt(-8, 25, 3);
  r.row.waves = [["drone", "drone", "husk", "husk"], ["warden", "drone", "drone"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "WHIP IT, {quickSwap}, SLUG IT" });
  r.ceil(0, 3, 3, 3, "white", 12, 1.0); r.ceil(0, 16, 6, 6, "amber", 12, 1.1); r.ceil(-8, 23, 4, 4, "white", 12, 1.0);
  r.pool(0, 26, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 22]]);
  r.walk([[-8, 0, 8], [-8, 3, 19], [-8, 3, 22]]);
  r.frames([5, 0, 4], [0, 8, 16], [0, 0, 22], [0, 1, 28]);
}

// ---------------------------------------------------------------- 6.5 THE CHANNEL (combine: launch them into the lava; cross three ways)
{
  const r = L.room({ id: "m5", name: "6.5 THE CHANNEL", job: "combine", verb: "launch the husks off the bridge into the lava; cross by the jump dash, the ride, the hook or the bridge", w: 24, l: 28, h: 9, par: TIMES.m5, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const C0 = 12, C1 = 18;
  r.lava(-12, C0, 12, C1, LAVA_D, `THE CHANNEL (6 m across, lava ${LAVA_D} m under the floor; the level's ONLY lava): the husks cross the bridge beside it, so a launched husk goes in (it kills an enemy, it burns the player: Core/Lava30.cs)`);
  r.box("deck", -2, C0, 2, C1, -LAVA_D, 0, "line", "the BRIDGE (4 m wide) over the channel: the husks' file at you, the launch's stage, the walkers' crossing");
  r.box("deck", -12, C0, -10, C1, -LAVA_D, -0.5, "climb", "the channel's west step (0.5 m under the floor): out of the lava for anything that falls in");
  r.box("deck", 10, C0, 12, C1, -LAVA_D, -0.5, "climb", "the channel's east step");
  r.panel("W", 11, 8, 22, -LAVA_D, 5, "ride panel E (face west, 14 m, standing in the lava): the ride over the channel");
  r.row.grappleNodes = [r.pt(-6, 8, 15)];
  r.row.grapple = "one node 8 m over the channel's west half: a hook across";
  nodes(r, [[-1, 21], [1, 21], [0, 23], [-4, 24], [4, 24], [-6, 22], [6, 22]]);
  r.row.waves = [["husk", "husk", "husk", "husk", "husk", "husk"], ["charger", "husk", "husk", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 2, prompt: "{punch} THEM INTO THE LAVA" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 15, 6, 4, "white", 9, 1.0); r.ceil(0, 23, 6, 3, "amber", 9, 1.1);
  r.pool(0, 26, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 20]]);
  r.walk([[-11, -0.5, 15], [-11, 0, 9]], { jump: true, maxSeconds: 8 }); // out of the channel by its west step (round 2: south, as n40's canal proof)
  r.frames([5, 0, 4], [0, -1, 15], [0, 0, 20], [0, 1, 28]);
}

// ---------------------------------------------------------------- 6.6 THE WINDOW (combine: the ride's jump-off and the jump dash through a hole)
{
  const r = L.room({ id: "m6", name: "6.6 THE WINDOW", job: "combine", verb: "ride, jump off with the look up, jump-dash through the window: land behind the wardens", w: 24, l: 26, h: 10, par: TIMES.m6, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const W0 = 13, W1 = 14;
  r.box("hull", -8, W0, -2, W1, 0, r.h, "line", "THE WALL's west part (floor to roof): the wardens' shields face the door beside it; only the window and the west door go through");
  r.box("hull", 2, W0, 12, W1, 0, r.h, "line", "THE WALL's east part (floor to roof)");
  r.box("hull", -2, W0, 2, W1, 0, 2.5, "line", "the WINDOW's sill (2.5 m): the window is 4 m wide, 2.5..5 m up: the ride's jump-off and the jump dash go through it");
  r.box("hull", -2, W0, 2, W1, 5, r.h, "line", "the WINDOW's lintel (5 m to the roof)");
  r.box("hull", -12, W0, -8, W1, 4.5, r.h, "line", "the WEST DOOR's lintel: the door (4 m wide, on the floor) is the walkers' way, through the wardens' shields");
  r.panel("W", 10, 2, 11, 0, 5, "ride panel S (face west, 9 m): the run-up; jump off north with the look up toward the window");
  r.box("deck", 6, 22, 12, 26, 0, 2.5, "line", "the far LEDGE (2.5 m): the spitter's stand behind the wall");
  r.ramp(9, 16, 22, 3, 2.5, "climb", "the ledge's ramp (2.5 m over 6 m; its foot 2 m clear of THE WALL, round 2: levelgen found the old foot against the wall unreachable): the chasers' way up");
  r.row.grappleNodes = [r.pt(0, 7, 16)];
  r.row.grapple = "one node 7 m up just past the window: a hook through it from the south half";
  nodes(r, [[-2, 17], [2, 17], [-5, 21], [0, 22], [4, 18]]);
  r.highNodeAt(8, 24, 2.5); r.highNodeAt(10.5, 24.5, 2.5);
  // the budget (design.n27.budget): wave 1 has 10 points; two wardens + two spitters cost 12 (levelgen, round 2): one spitter
  r.row.waves = [["warden", "warden", "spitter"], ["warden", "warden", "charger"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "THROUGH THE WINDOW, THEN BEHIND" });
  r.ceil(0, 3, 3, 3, "white", 10, 1.0); r.ceil(0, 11, 4, 2, "amber", 10, 1.1); r.ceil(0, 20, 6, 4, "white", 10, 1.0);
  r.pool(0, 24, 2.5, "cyan", 7, 1.6);
  r.walkSE([[-10, 0, 8], [-10, 0, 16], [0, 0, 20]]);
  r.frames([6, 0, 6], [0, 3.5, 13.5], [0, 0, 18], [0, 1, 26]);
}

// ---------------------------------------------------------------- 6.7 EVERYTHING (test: three floors, every verb pays)
{
  const r = L.room({ id: "m7", name: "6.7 EVERYTHING", job: "test", verb: "every verb of levels 1-5 on three floors: the floor, the decks (2.5 m), the catwalk (5 m)", w: 30, l: 32, h: 10, par: TIMES.m7, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -15, 10, -9, 24, 0, 2.5, "line", "the WEST DECK (2.5 m): spitters, then a slam down into the pack");
  r.box("deck", 9, 10, 15, 24, 0, 2.5, "line", "the EAST DECK (2.5 m)");
  r.ramp(-12, 3, 10, 3, 2.5, "climb", "the west deck's ramp (2.5 m over 7 m)");
  r.ramp(12, 3, 10, 3, 2.5, "climb", "the east deck's ramp");
  // round 4: HOPS for the route graph (as 6.3's: nodes on box tops only, a ramp carries none, and the bot's follower fails the 2.5 m double the graph
  // offers): round 3's two misses in 10 were the bot stuck at (lx 8.8, lz 7.7), under the east deck's front, trying to get up. A 1.25 m block at each
  // deck's front (floor -> block -> deck: two hops) and a 3.75 m block in each deck-to-catwalk gap (deck -> block -> catwalk: two hops)
  for (const s of [-1, 1]) {
    const side = s < 0 ? "west" : "east";
    r.box("hull", s < 0 ? -9.5 : 6.5, 7.5, s < 0 ? -6.5 : 9.5, 10, 0, 1.25, "climb", `the ${side} deck's HOP block (1.25 m) at its front: half the deck's 2.5 m, one hop each way (the route graph's way up; the ramp is the walkers')`);
    r.box("hull", s < 0 ? -15 : 9, 24, s < 0 ? -9 : 15, 26, 0, 3.75, "climb", `the ${side} deck-to-catwalk HOP block (3.75 m) in the 2 m gap: deck (2.5) -> block -> catwalk (5), a hop each`);
  }
  r.box("deck", -15, 26, -2, 32, 0, 5, "line", "the CATWALK's west half (5 m): the top floor; a deck's boost (2.5 -> 5 m) or a node reaches it");
  r.box("deck", 2, 26, 15, 32, 0, 5, "line", "the CATWALK's east half; the exit runs between the halves at floor level");
  r.ramp(-7, 16, 26, 3, 5, "climb", "the west catwalk's ramp (5 m over 10 m): the enemies' way up and down");
  r.ramp(7, 16, 26, 3, 5, "climb", "the east catwalk's ramp");
  r.panel("E", -4, 5, 14, 0, 5, "ride panel (face east, 9 m; round 3: ends at lz 14 so the lane between it and the deck opens north of it, before the catwalk ramp: the walk bot stuck 1 / 10 in that lane) facing its pair 8 m away: the middle's ride chain from deck side to deck side");
  r.panel("W", 4, 5, 14, 0, 5, "ride panel (face west, 9 m): the pair's other wall");
  r.row.grappleNodes = [r.pt(-12, 8.5, 28), r.pt(12, 8.5, 28)];
  r.row.grapple = "two nodes over the catwalk's ends at 8.5 m (round 3: the roof 10 m, was 12: a drone kept at 11.4 m out of the walk bot's reach 1 / 10): a pull from a deck to the top floor";
  nodes(r, [[0, 15], [-3, 20], [3, 20], [0, 22], [-2, 24], [2, 24]]); // round 2: (+-6, 20) sat on the catwalk ramps (no grid node); (0, 12) was 10 m in the first view
  r.highNodeAt(-12, 18, 2.5); r.highNodeAt(12, 18, 2.5); r.highNodeAt(-10, 29, 5); r.highNodeAt(10, 29, 5);
  // the budget (design.n27.budget): at most 2 kinds a wave, 3 with a tank; four waves so every kind comes
  r.row.waves = [["husk", "husk", "husk", "husk", "spitter", "spitter"], ["tank", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"],
    ["warden", "drone", "drone"], ["charger", "charger", "husk", "husk"]];
  r.pickup("healthPickup", -12, 20, 3.4, "the west deck pays health between the waves: the high ground, not the floor");
  r.ceil(0, 3, 3, 3, "white", 12, 1.0); r.ceil(0, 12, 6, 6, "white", 12, 1.0); r.ceil(-12, 17, 4, 6, "white", 12, 1.0); r.ceil(12, 17, 4, 6, "white", 12, 1.0); r.ceil(0, 28, 6, 3, "amber", 12, 1.1);
  r.pool(0, 30, 2.5, "cyan", 8, 1.6);
  r.walkSE([[0, 0, 22]]);
  r.walk([[-12, 0, 2], [-12, 2.5, 11], [-12, 2.5, 18]]);
  r.walk([[-7, 0, 15], [-7, 5, 27], [-8, 5, 29]]);
  r.frames([8, 0, 4], [0, 3, 22], [0, 0, 24], [0, 1, 32]);
}

// ---------------------------------------------------------------- 6.8 REST
{
  const r = L.room({ id: "mr", name: "6.8 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: TIMES.mr, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 5, 0.9, "the rest before THE RIVAL: full health");
  r.ceil(0, 5, 3, 3, "white", 8, 1.0);
  r.pool(0, 8.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
}

// ---------------------------------------------------------------- 6.9 THE MIRROR (THE RIVAL's arena, reserved empty)
{
  // RIVAL.md proposes the arena (a pinwheel of four ride panels, a boost-height centre block, two nodes); none of it is built until GG says yes. The
  // room is empty: kind "boss" with the id "rival41", which no sim code answers (SurvivalSim.BossRoomBegin returns false), so it clears at once.
  const S = 34;
  const r = L.room({ id: "mx", name: "6.9 THE MIRROR", kind: "boss", job: "boss", verb: "THE RIVAL (a proposal: Research/2026-10-14-wave41/RIVAL.md); reserved empty", w: S, l: S, h: 14, par: 90, spawnLz: 3, exitFromN: S / 2 });
  r.row.boss = "rival41";
  r.ceil(0, S / 2, 6, 6, "white", 16, 1.2);
  r.pool(0, S / 2, 3, "cyan", 8, 1.2);
  r.frames([0, 0, 3], [0, 1, S / 2], [10, 0, 10], [0, 1, S - 2]);
}

const plan = L.finish();
plan.wallHeight = old.wallHeight;
plan.owns38 = ["jump", "double", "dash", "slide", "ride", "walljump", "boost", "grapple", "dashjump"];
plan.owns38_why = "Builder E's gate checker (Design/levels/n38/GATES38.md): level 6 owns every verb of levels 1-5. n41-finale has no `gate` tag (no traversal gate: every crossing has a walker's line), so `n38 gates` has nothing to judge.";
fs.mkdirSync(path.join(ROOT, path.dirname(OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, OUT), JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", OUT, plan.rooms.length, "rooms [" + plan.rooms.map((q) => q.id).join(" ") + "],", plan.boxes.length, "boxes", plan.ramps.length, "ramps", plan.pits.length, "pits", plan.lights.length, "lights", plan.props.length, "props; floorSize", plan.floorSize);
for (const q of plan.rooms) console.log(`  ${q.id} ${q.name}: rect ${JSON.stringify(q.rect)} waves ${(q.waves || []).map((w) => w.length).join("+") || "-"}`);

/// The S/A/B/C rank cuts: the beat sheet's room times (TIMES) + THE RIVAL's place (90 s, GG's band "one to two minutes") x [1.3, 2, 3, 4.5] as NIGHT36's;
/// kills / style as n38-finale's (NOT calibrated: no person's times; the walk bot's medians are the bot's).
function RANK41() {
  const t = Object.values(TIMES).reduce((a, b) => a + b, 0) + 90;
  return { time: [1.3, 2, 3, 4.5].map((k) => Math.round(t * k / 5) * 5), kills: old.rank.kills, style: old.rank.style,
    _why: `Unity-only (builder L6, NIGHT41): S/A/B/C cuts: time = the beat sheet's room times (${t - 90} s) + THE RIVAL's place 90 s (GG: bosses "one to two minutes"), x [1.3, 2, 3, 4.5] as NIGHT36's; kills and style as n38-finale's (NOT calibrated).` };
}
